using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace TileStories
{
    // "Open in full view" for ANY block (_3.2 section 6.2, step 12): the block is drawn in the card's TakeoverView by a SECOND view of its kind,
    // taken from the card's one BlockViewPool and bound through the takeover's own media scope -- while the card's own view of it (inline, or
    // an open compact row) stays bound under it. The second binding is marked BlockBindContext.FullView, so an owner keyed by the block hands
    // it a slot of its own and the card's is never touched (a 3D / 360 preview), and a view with its own full screen does not offer it again
    // (a video). Owners that serve ONE thing at a time (audio, video, AR placement) need nothing: both views draw the same playback / the one
    // model, so what the visitor does in the full view is what the card shows once it closes. Leaving the page (Back, another card, the card
    // closing, the view reopened over a rebind) unbinds the view BEFORE the takeover gives its files back, and returns it to the pool.
    // The page scrolls (a long text reads to its end) and a view that fills the screen -- a video, a 3D / 360 stage -- takes all its height.
    // Owned by the sheet (PoiCardSheetView.OpenFullView decides when; this binds and lets go).
    // Among its SIBLINGS (_3.2 step 16, FullViewSiblingRule): the block opens as one page of its section's blocks, a swipe or a chip turning to
    // the next without returning to the card -- each turn hands every owner from one block's view to the next through the takeover's page change.
    public sealed class BlockFullView
    {
        // The class the bound view's root wears while it is in the full view (it grows to the page's height): what a test reads
        public const string BlockClass = "card-full-view__block";

        private readonly TakeoverView _takeover;
        private readonly BlockViewPool _views;
        // The Page Chrome's foot (Previous / Next), built once and put at the end of the page's scroll while the chrome shows it
        private readonly PageFootView _foot = new();

        // The foot of the open block page (in the page's scroll only while it is shown)
        public PageFootView Foot => _foot;

        // The page's own vertical scroll, built once and put back on the page every time it opens (the takeover clears its page on leave)
        public ScrollView Scroll { get; }

        public BlockFullView(TakeoverView takeover, BlockViewPool views)
        {
            _takeover = takeover;
            _views = views;
            // - like the card's stack: dragged on a phone, no desktop scroll bar
            Scroll = new ScrollView(ScrollViewMode.Vertical)
            {
                name = "card-full-view",
                verticalScrollerVisibility = ScrollerVisibility.Hidden,
                horizontalScrollerVisibility = ScrollerVisibility.Hidden,
            };
            Scroll.AddToClassList("card-full-view");
            // - the content is at least as tall as what shows, so a view that fills the page (a video, a 3D / 360 stage) has the page's
            //   height to grow into; a percentage in USS does not resolve against the scroll's viewport, its measured height does
            Scroll.contentViewport.RegisterCallback<GeometryChangedEvent>(e => Scroll.contentContainer.style.minHeight = e.newRect.height);
            // - a page the visitor comes back to holds its scroll while it lays out, until the visitor touches it
            Scroll.contentContainer.RegisterCallback<GeometryChangedEvent>(_ => KeepReturnOffset());
            Scroll.RegisterCallback<PointerDownEvent>(_ => StopHolding(), TrickleDown.TrickleDown);
            Scroll.RegisterCallback<WheelEvent>(_ => StopHolding(), TrickleDown.TrickleDown);

            // - the sibling swipe (_3.2 step 16): the ONE way a part takes a finger, as the pages layout does on the card's stack -- judged once
            //   past the slop, clearly sideways only (reading down the page stays the scroll's), and a finger that lands on a part that drags by
            //   itself (a 3D / 360 stage, a slider, a filmstrip) is that part's. The page follows the finger, then turns or slides back
            var swipe = new SwipeGrab(Scroll.contentViewport, travel => TurnsBetweenBlocks && SwipeGrabRule.ClaimsPointer(travel), leavesToParts: true);
            swipe.Moved += travel => Scroll.style.translate = new Translate(
                SwipePageRule.DragOffset(travel.x, _takeover.PageIndex > 0, _takeover.PageIndex < _takeover.PageCount - 1), 0f);
            swipe.Released += travel =>
            {
                Scroll.style.translate = StyleKeyword.Null;
                int direction = SwipePageRule.Direction(travel, Scroll.contentViewport.layout.width);
                if (direction != 0) _takeover.Turn(direction);
            };
            swipe.Cancelled += () => Scroll.style.translate = StyleKeyword.Null;
        }

        // Whether the full view shows one block among its siblings now (a page per sibling): what a sideways swipe turns
        public bool TurnsBetweenBlocks => View != null && _takeover.IsOpen && _takeover.PageCount > 1;

        // The page drawer for a block among its siblings (FullViewSiblingRule): page i binds `entries[i]` exactly as PageFor does, so a turn is
        // the takeover's own page change -- the leaving view unbinds BEFORE the scope gives its files back, then the next binds
        public TakeoverPageDrawer PagesFor(IReadOnlyList<BlockStackBuilder.Entry> entries, BlockBindContext context)
        {
            return (takeover, container, media, page) => PageFor(entries[page], context)(takeover, container, media, page);
        }

        // The block shown in the full view now (null when none: closed, or the takeover shows something else -- a lightbox)
        public string BlockKey { get; private set; }
        // Its second view (null when none)
        public IBlockView View { get; private set; }
        public bool IsOpen => View != null && _takeover.IsOpen;

        // The page drawer that binds `entry`'s second view with the card's `context` (TakeoverView.Open draws it at once)
        public TakeoverPageDrawer PageFor(BlockStackBuilder.Entry entry, BlockBindContext context)
        {
            return (takeover, container, media, page) =>
            {
                string kind = entry.Definition?.Key;
                var view = _views.Take(kind);
                if (view == null) return;
                var full = context.ForBlock(media, entry.Variant);
                full.FullView = true;
                view.Bind(entry.Instance, full);
                view.Root.AddToClassList(BlockClass);
                Scroll.scrollOffset = UnityEngine.Vector2.zero;
                Scroll.Add(view.Root);
                container.Add(Scroll);
                View = view;
                BlockKey = entry.Instance.key;
                // - the Page Chrome of THIS block: the wall's choice unless the block overrides it (the breadcrumb is the takeover's bar, the
                //   Previous / Next row the foot of this page's scroll)
                var chrome = PageChromeRule.Of(context.Settings?.components, entry.Instance);
                _takeover.ApplyChrome(chrome);
                // - a page that shows its own big title (a story's title row) is not named again: the trail is its context, point > section
                _takeover.ShowPageName(!ShowsItsOwnTitle(entry, full));
                ShowFoot(chrome, context.Strings);
                takeover.WhenPageLeaves(() => Release(kind, view));
            };
        }

        // Whether `entry`'s page draws a title of its own in the language it is bound in: a composing kind whose Page holds a title-like row
        private static bool ShowsItsOwnTitle(BlockStackBuilder.Entry entry, BlockBindContext bound) =>
            BlockSurfaces.Composes(entry.Definition)
            && ElementRules.HasOwnTitle(new ElementScope(entry.Instance, entry.Definition, bound.Poi?.card?.blocks, null, null), ElementRegistry.Shared,
                bound.Language, bound.FallbackLanguage);

        // The foot of the open block page: Previous / Next cards at the end of the scroll, each turning to the neighbouring page (the sibling
        // swipe and the chips stay as they are). None when the chrome hides it or the block has no sibling
        private void ShowFoot(PageChromeChoice chrome, CardStrings strings)
        {
            int page = _takeover.PageIndex, last = _takeover.PageCount - 1;
            if (!chrome.PrevNext || last < 1) return;
            string previous = page > 0 ? _takeover.NameOf(page - 1) : null;
            string next = page < last ? _takeover.NameOf(page + 1) : null;
            _foot.Show(strings?.Get(CardStrings.Keys.PagePrevious), previous, () => _takeover.Turn(-1),
                strings?.Get(CardStrings.Keys.PageNext), next, () => _takeover.Turn(1));
            Scroll.Add(_foot);
        }

        // The page the visitor comes BACK to (a picture's viewer or a video's player opened from it, _3.3 S2b): its scroll is put back where they
        // left it, held while the page lays out (its pictures load) until it is reached or the visitor touches the page
        public void ReturnTo(float offset)
        {
            _returnOffset = offset;
            KeepReturnOffset();
        }

        private float _returnOffset = float.NaN;

        private void KeepReturnOffset()
        {
            if (float.IsNaN(_returnOffset) || View == null) return;
            Scroll.scrollOffset = new UnityEngine.Vector2(0f, _returnOffset);
            if (UnityEngine.Mathf.Abs(Scroll.scrollOffset.y - _returnOffset) < 0.5f) _returnOffset = float.NaN;
        }

        // The visitor touched the page: nothing is held for them any more
        private void StopHolding() => _returnOffset = float.NaN;

        // Unbind the second view and give it back (the takeover releases its scope right after). On a TURN to a sibling (the takeover still
        // open) a view that keeps something going lets it go first -- a playing video pauses: the next sibling covers it, and nothing on screen
        // would show it or stop it. Closing keeps step 12's rule: the card's own view of the block is right under it, showing the same playback
        private void Release(string kind, IBlockView view)
        {
            if (_takeover.IsOpen) (view as ICollapsibleBlockView)?.Collapsing();
            view.Unbind();
            view.Root.RemoveFromClassList(BlockClass);
            view.Root.RemoveFromHierarchy();
            _foot.RemoveFromHierarchy();
            _views.Return(kind, view);
            if (View != view) return;
            View = null;
            BlockKey = null;
            _returnOffset = float.NaN;
        }
    }
}
