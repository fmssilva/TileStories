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
    // A whole SECTION (the bento layout, _3.2 step 15b: the card's own views of it are set aside, so nothing is bound twice): its targets one
    // under the other, each under the words the stack draws over it, through the takeover's one media scope, all let go when the page leaves.
    // The page lands on the block the jump named, and reports each block the visitor had in view (NavScrollRule.HasViewed, the stack's own
    // rule), so the card's reading and the contents list's ticks hold in this layout too.
    public sealed class BlockFullView
    {
        // The class the bound view's root wears while it is in the full view (it grows to the page's height): what a test reads
        public const string BlockClass = "card-full-view__block";

        private readonly TakeoverView _takeover;
        private readonly BlockViewPool _views;

        // The class a section page's content wears, and each of its blocks' slot
        public const string SectionClass = "card-full-view__section";
        public const string SectionBlockClass = "card-full-view__section-block";

        // The page's own vertical scroll, built once and put back on the page every time it opens (the takeover clears its page on leave)
        public ScrollView Scroll { get; }

        // A section page's blocks: their kind, view and slot, in the section's order
        private readonly List<(string Kind, string Key, IBlockView View, VisualElement Slot)> _section = new();
        // The blocks of the open section reported as viewed on this page
        private readonly HashSet<string> _viewed = new();
        // The block a section page lands on (null once the visitor touched the page, or none asked)
        private string _landing;

        // The section shown in the full view now (its key; null when none)
        public string SectionKey { get; private set; }
        // Its blocks, in order
        public IEnumerable<string> SectionBlocks { get { foreach (var b in _section) yield return b.Key; } }

        // The view a section page bound for a block (null when it is not on the page)
        public IBlockView SectionViewOf(string blockKey)
        {
            foreach (var b in _section)
                if (b.Key == blockKey) return b.View;
            return null;
        }

        // The slot a section page holds a block in (null when it is not on the page)
        public VisualElement SectionSlotOf(string blockKey)
        {
            foreach (var b in _section)
                if (b.Key == blockKey) return b.Slot;
            return null;
        }

        // A block of the open section the visitor has had in view on this page (once per page)
        public event Action<string> Viewed;
        // A section page left the screen (closed, or replaced)
        public event Action SectionLeft;

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
            // - a section page: what the visitor had in view, on every scroll and size change; the landing held while pictures load,
            //   until the visitor touches the page
            Scroll.verticalScroller.valueChanged += _ => MeasureViewed();
            Scroll.contentContainer.RegisterCallback<GeometryChangedEvent>(_ =>
            {
                Land();
                MeasureViewed();
            });
            Scroll.contentViewport.RegisterCallback<GeometryChangedEvent>(_ => MeasureViewed());
            Scroll.RegisterCallback<PointerDownEvent>(_ => _landing = null, TrickleDown.TrickleDown);
            Scroll.RegisterCallback<WheelEvent>(_ => _landing = null, TrickleDown.TrickleDown);
        }

        // The block shown in the full view now (null when none: closed, or the takeover shows something else -- a lightbox)
        public string BlockKey { get; private set; }
        // Its second view (null when none)
        public IBlockView View { get; private set; }
        public bool IsOpen => (View != null || _section.Count > 0) && _takeover.IsOpen;

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
                takeover.WhenPageLeaves(() => Release(kind, view));
            };
        }

        // The page drawer that binds a whole section (`sectionKey`): each of `entries` (its targets, in order) under the words `headingOf` gives
        // for it, through the takeover's scope, landing on `landingKey` (null: the section's top)
        public TakeoverPageDrawer SectionPageFor(IReadOnlyList<BlockStackBuilder.Entry> entries, string sectionKey, Func<string, string> headingOf,
            BlockBindContext context, string landingKey)
        {
            return (takeover, container, media, page) =>
            {
                foreach (var entry in entries)
                {
                    string kind = entry.Definition?.Key;
                    var view = _views.Take(kind);
                    if (view == null) continue;
                    var full = context.ForBlock(media, entry.Variant);
                    full.FullView = true;
                    view.Bind(entry.Instance, full);
                    var slot = new VisualElement();
                    slot.AddToClassList("card-block-slot");
                    slot.AddToClassList(SectionBlockClass);
                    var heading = new Label(headingOf?.Invoke(entry.Instance.key) ?? "");
                    heading.AddToClassList("card-block-heading");
                    heading.EnableInClassList("card-block-heading--empty", heading.text.Length == 0);
                    slot.Add(heading);
                    slot.Add(view.Root);
                    Scroll.Add(slot);
                    _section.Add((kind, entry.Instance.key, view, slot));
                }
                Scroll.contentContainer.AddToClassList(SectionClass);
                Scroll.scrollOffset = UnityEngine.Vector2.zero;
                container.Add(Scroll);
                SectionKey = sectionKey;
                _landing = landingKey;
                _viewed.Clear();
                takeover.WhenPageLeaves(ReleaseSection);
            };
        }

        // Keep the landing block's top at the page's top while the page lays out (a picture loading moves it) -- until the visitor touches it
        private void Land()
        {
            if (_landing == null || SectionKey == null) return;
            var slot = SectionSlotOf(_landing);
            if (slot == null || float.IsNaN(slot.layout.y)) return;
            float offset = NavScrollRule.TargetOffset(slot.layout.y, Scroll.contentContainer.layout.height, Scroll.contentViewport.layout.height);
            if (UnityEngine.Mathf.Abs(Scroll.scrollOffset.y - offset) > 0.5f) Scroll.scrollOffset = new UnityEngine.Vector2(0f, offset);
        }

        // Report every block of the section page now in view with its top past the spy line, once (NavScrollRule.HasViewed)
        private void MeasureViewed()
        {
            if (SectionKey == null) return;
            float offset = Scroll.scrollOffset.y;
            float content = Scroll.contentContainer.layout.height;
            float viewport = Scroll.contentViewport.layout.height;
            if (float.IsNaN(content) || float.IsNaN(viewport) || viewport <= 0f) return;
            foreach (var b in _section)
            {
                if (_viewed.Contains(b.Key) || float.IsNaN(b.Slot.layout.y)) continue;
                if (!NavScrollRule.HasViewed(true, b.Slot.layout.y, b.Slot.layout.yMax, offset, content, viewport)) continue;
                _viewed.Add(b.Key);
                Viewed?.Invoke(b.Key);
            }
        }

        // A section page leaves: a view that keeps something going lets it go first (a playing video: the card holds no other view of the
        // section to show it, as a section tabs puts away), then every view is unbound BEFORE the takeover gives the files back, and returned
        // to the pool
        private void ReleaseSection()
        {
            foreach (var b in _section)
            {
                (b.View as ICollapsibleBlockView)?.Collapsing();
                b.View.Unbind();
                b.View.Root.RemoveFromHierarchy();
                b.Slot.RemoveFromHierarchy();
                _views.Return(b.Kind, b.View);
            }
            _section.Clear();
            _viewed.Clear();
            _landing = null;
            Scroll.contentContainer.RemoveFromClassList(SectionClass);
            if (SectionKey == null) return;
            SectionKey = null;
            SectionLeft?.Invoke();
        }

        // Unbind the second view and give it back (the takeover releases its scope right after)
        private void Release(string kind, IBlockView view)
        {
            view.Unbind();
            view.Root.RemoveFromClassList(BlockClass);
            view.Root.RemoveFromHierarchy();
            _views.Return(kind, view);
            if (View != view) return;
            View = null;
            BlockKey = null;
        }
    }
}
