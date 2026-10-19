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
    public sealed class BlockFullView
    {
        // The class the bound view's root wears while it is in the full view (it grows to the page's height): what a test reads
        public const string BlockClass = "card-full-view__block";

        private readonly TakeoverView _takeover;
        private readonly BlockViewPool _views;

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
                takeover.WhenPageLeaves(() => Release(kind, view));
            };
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
