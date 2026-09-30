using System.Collections.Generic;
using UnityEngine.UIElements;

namespace TileStories
{
    // The blocks of the open card (_3.1 section 3): the header in its own slot, pinned at the top of the sheet
    // (the title stays in view while the rest scrolls; peek shows exactly its top part), and every other block
    // in order in a ScrollView. Views are pooled per kind: switching POIs rebinds, it never instantiates and
    // destroys a view per tap (work plan performance rule 2). Each bound view gets its own ScopedMediaSource over
    // the card's media source: it loads lazily in Bind, and the stack releases the whole scope on unbind. Once the
    // visitor scrolls the stack, the header collapses to its compact look (a class on the header slot,
    // HeaderCollapseRule decides) and opens again at the scroll top. Every block sits in a SLOT: the common heading
    // (BlockKindDefinition.HeadingField, or the kind's DefaultHeadingKey) above the view's root, and the one block gap
    // (--ts-block-gap) under it -- the stack draws both, so every kind has the same heading look and rhythm. A picture
    // header's hero (HeaderBlockView.HeroPart) opens the scroll, above the first block; the scroll drives its parallax.
    public sealed class BlockStackView
    {
        public VisualElement HeaderSlot { get; }
        public ScrollView Scroll { get; }
        // Pinned under the scroll: the blocks whose variant is a footer (BlockKindDefinition.FooterVariants)
        public VisualElement Footer { get; }
        // Pinned right under the header, above the scroll: the blocks whose variant is pinned to the top (BlockKindDefinition.PinnedTopVariants)
        public VisualElement PinnedTop { get; }

        private readonly BlockRegistry _registry;
        // One block's place in the stack: its heading, then its view (pooled together per kind)
        private sealed class Slot
        {
            public VisualElement Root;
            public Label Heading;
            public IBlockView View;
        }

        private readonly Dictionary<string, Stack<Slot>> _pool = new();
        private readonly List<(string Kind, Slot Slot, ScopedMediaSource Media)> _bound = new();
        // The slots of blocks kept hidden until the card was read (ShowAfterViewedField), with what remembers their reveal
        private readonly List<(Slot Slot, string PoiId, string BlockKey)> _waiting = new();
        private CardLocalState _state;

        // Whether the visitor has scrolled past the card's content since it was bound (ContentSeenRule): it latches, and a new
        // card starts unseen
        public bool ContentSeen { get; private set; }

        // How many views were ever built (pooling makes this stop growing once every kind was seen)
        public int CreatedViewCount { get; private set; }

        // The class that turns the pinned header into its compact look (About.uss / PoiCard.uss hide what it gives up)
        public const string CollapsedHeaderClass = "poi-card__header--collapsed";

        // Whether the header is collapsed now (the stack was scrolled down)
        public bool HeaderCollapsed { get; private set; }

        // --ts-header-collapsed-max-height (CardTokens.uss), handed over by the sheet that reads the card's tokens: the one-line
        // collapsed header's ceiling, what "freed" is measured against (0 until the styles resolve: then freed = the whole
        // header, the safe over-estimate)
        internal float CollapsedCeiling { get; set; }

        // The bound views in stack order (the header first)
        public IReadOnlyList<IBlockView> BoundViews
        {
            get
            {
                var views = new List<IBlockView>(_bound.Count);
                foreach (var b in _bound) views.Add(b.Slot.View);
                return views;
            }
        }

        // The heading drawn above a bound view (hidden when it has no text)
        public Label HeadingOf(IBlockView view)
        {
            foreach (var b in _bound)
                if (b.Slot.View == view) return b.Slot.Heading;
            return null;
        }

        // The slot element holding a bound view and its heading (the block's full footprint in the stack)
        public VisualElement SlotOf(IBlockView view)
        {
            foreach (var b in _bound)
                if (b.Slot.View == view) return b.Slot.Root;
            return null;
        }

        // The bound header view (null when the header is another view type, or nothing is bound)
        public HeaderBlockView HeaderView => _bound.Count > 0 ? _bound[0].Slot.View as HeaderBlockView : null;

        // The element the peek stop must show in full: the header's top part (its whole root for another header view) -- or, when a block is
        // pinned to it, that pinned strip (Bind() sits it between the title/chip and the subtitle, so its bottom edge already
        // includes the title/chip above it and stays above the subtitle, never dragging the subtitle into the peek stop with it)
        public VisualElement PeekPart
        {
            get
            {
                if (PinnedTop.childCount > 0) return PinnedTop;
                if (_bound.Count == 0 || _bound[0].Kind != BuiltInBlocks.HeaderKind) return HeaderSlot;
                return _bound[0].Slot.View is HeaderBlockView header ? header.PeekPart : _bound[0].Slot.View.Root;
            }
        }

        public BlockStackView(VisualElement headerParent, VisualElement scrollParent, BlockRegistry registry)
        {
            _registry = registry;
            HeaderSlot = new VisualElement { name = "poi-card-header" };
            HeaderSlot.AddToClassList("poi-card__header");
            headerParent.Add(HeaderSlot);
            // - a placeholder position until the header binds and moves it inside itself (Bind()): between the title/chip and the
            //   subtitle, so a pinned block (the audio hero chip) counts in the peek stop without dragging the subtitle in with it
            PinnedTop = new VisualElement { name = "poi-card-pinned-top" };
            PinnedTop.AddToClassList("poi-card__pinned-top");
            headerParent.Add(PinnedTop);
            // - a phone card scrolls by dragging its content: no desktop scroll bar (it took width and ignored the tokens)
            Scroll = new ScrollView(ScrollViewMode.Vertical)
            {
                name = "poi-card-stack",
                verticalScrollerVisibility = ScrollerVisibility.Hidden,
                horizontalScrollerVisibility = ScrollerVisibility.Hidden,
            };
            Scroll.AddToClassList("poi-card__stack");
            scrollParent.Add(Scroll);
            Footer = new VisualElement { name = "poi-card-footer" };
            Footer.AddToClassList("poi-card__footer");
            scrollParent.Add(Footer);
            Scroll.verticalScroller.valueChanged += _ =>
            {
                UpdateHeaderCollapse();
                HeaderView?.OnStackScrolled(Scroll.scrollOffset.y);
                UpdateContentSeen();
            };
            // - the content or the visible part changing size (a sheet dragged to another stop, a picture loading) can also
            //   leave nothing more to scroll to: the scroll offset alone would miss it
            Scroll.contentContainer.RegisterCallback<GeometryChangedEvent>(_ => UpdateContentSeen());
            Scroll.contentViewport.RegisterCallback<GeometryChangedEvent>(_ => UpdateContentSeen());
        }

        // Reveal the blocks that wait for the card to be read once the visitor got to the end of its content (ContentSeenRule),
        // and remember it so they show at once the next time
        private void UpdateContentSeen()
        {
            if (ContentSeen) return;
            float range = Scroll.contentContainer.layout.height - Scroll.contentViewport.layout.height;
            if (!ContentSeenRule.HasSeenAll(Scroll.scrollOffset.y, range, Scroll.contentViewport.layout.height)) return;
            ContentSeen = true;
            foreach (var (slot, poiId, blockKey) in _waiting)
            {
                slot.Root.style.display = DisplayStyle.Flex;
                _state?.MarkSeen(poiId, blockKey);
            }
            _waiting.Clear();
        }

        // Collapse or open the header for the stack's current scroll (HeaderCollapseRule)
        private void UpdateHeaderCollapse()
        {
            float range = Scroll.contentContainer.layout.height - Scroll.contentViewport.layout.height;
            // - what collapsing gives back: the open header down to the one-line look's ceiling (the token). Never an
            //   under-estimate, so the rule's "can it still scroll afterwards" guard stays safe
            float freed = HeaderCollapsed || float.IsNaN(range) ? 0f : UnityEngine.Mathf.Max(0f, HeaderSlot.layout.height - CollapsedCeiling);
            SetHeaderCollapsed(HeaderCollapseRule.Next(HeaderCollapsed, Scroll.scrollOffset.y, float.IsNaN(range) ? 0f : range, freed));
        }

        private void SetHeaderCollapsed(bool collapsed)
        {
            if (collapsed == HeaderCollapsed) return;
            HeaderCollapsed = collapsed;
            HeaderSlot.EnableInClassList(CollapsedHeaderClass, collapsed);
        }

        // Show these entries (BlockStackBuilder's output), each bound with `context` and its own variant
        public void Bind(IReadOnlyList<BlockStackBuilder.Entry> entries, BlockBindContext context)
        {
            UnbindAll();
            _state = context.State;
            ContentSeen = false;
            foreach (var entry in entries)
            {
                var slotOf = Take(entry.Definition.Key);
                if (slotOf == null) continue;
                var view = slotOf.View;
                var media = new ScopedMediaSource(context.Media);
                var read = new BlockFieldReader(entry.Instance, context.Language, context.FallbackLanguage);
                // - a block that waits for the card to be read stays out of sight (its heading and gap with it) until then
                bool waits = entry.Definition.ShowAfterViewedField != null && read.Flag(entry.Definition.ShowAfterViewedField)
                    && !(context.State?.Seen(context.Poi?.id, entry.Instance.key) ?? false);
                slotOf.Root.style.display = waits ? DisplayStyle.None : DisplayStyle.Flex;
                if (waits) _waiting.Add((slotOf, context.Poi?.id, entry.Instance.key));
                string heading = read.Text(BlockKindDefinition.HeadingField);
                if (heading.Length == 0 && entry.Definition.DefaultHeadingKey != null && context.Strings != null)
                    heading = context.Strings.Get(entry.Definition.DefaultHeadingKey);
                slotOf.Heading.text = heading;
                slotOf.Heading.EnableInClassList("card-block-heading--empty", heading.Length == 0);
                view.Bind(entry.Instance, new BlockBindContext
                {
                    Poi = context.Poi,
                    Taxonomy = context.Taxonomy,
                    Variant = entry.Variant,
                    Language = context.Language,
                    FallbackLanguage = context.FallbackLanguage,
                    Media = media,
                    Strings = context.Strings,
                    MarkerLook = context.MarkerLook,
                    Glossary = context.Glossary,
                    Host = context.Host,
                    State = context.State,
                    Events = context.Events,
                    Services = context.Services,
                    Audio = context.Audio,
                    Video = context.Video,
                    Preview = context.Preview,
                    ReduceMotion = context.ReduceMotion,
                });
                bool header = _bound.Count == 0 && entry.Definition.Key == BuiltInBlocks.HeaderKind;
                var parent = header ? HeaderSlot
                    : entry.Definition.IsPinnedTop(entry.Variant) ? PinnedTop
                    : entry.Definition.IsFooter(entry.Variant) ? Footer : Scroll.contentContainer;
                parent.Add(slotOf.Root);
                if (header && view is HeaderBlockView h)
                {
                    // - a picture header's hero opens what scrolls: the title stays pinned while the picture scrolls away
                    if (h.HasHero) Scroll.contentContainer.Insert(0, h.HeroPart);
                    // - PinnedTop sits INSIDE the header, between the title/chip and the subtitle (never after the subtitle): a
                    //   pinned block (the audio hero chip) must count in the peek stop WITHOUT dragging the subtitle into peek
                    //   with it -- Root's two children are always [PeekPart, subtitle] (HeaderBlockView's constructor), so index 1
                    //   is exactly between them. Re-inserting an already-attached element just moves it: harmless on every rebind.
                    h.Root.Insert(1, PinnedTop);
                }
                _bound.Add((entry.Definition.Key, slotOf, media));
            }
            Scroll.scrollOffset = UnityEngine.Vector2.zero;
            // - a new card starts at the top, header open (the offset may already have been 0: no scroll event then)
            SetHeaderCollapsed(false);
        }

        // Unbind every view and put it back in its kind's pool
        public void UnbindAll()
        {
            foreach (var (kind, slot, media) in _bound)
            {
                if (slot.View is HeaderBlockView h) h.HeroPart.RemoveFromHierarchy();
                slot.View.Unbind();
                media.ReleaseAll();
                slot.Root.RemoveFromHierarchy();
                if (!_pool.TryGetValue(kind, out var stack)) _pool[kind] = stack = new Stack<Slot>();
                stack.Push(slot);
            }
            _bound.Clear();
            _waiting.Clear();
        }

        private Slot Take(string kind)
        {
            if (_pool.TryGetValue(kind, out var stack) && stack.Count > 0) return stack.Pop();
            var view = _registry.CreateView(kind);
            if (view == null) return null;
            CreatedViewCount++;
            var slot = new Slot { Root = new VisualElement(), Heading = new Label(), View = view };
            slot.Root.AddToClassList("card-block-slot");
            slot.Heading.AddToClassList("card-block-heading");
            slot.Root.Add(slot.Heading);
            slot.Root.Add(view.Root);
            return slot;
        }
    }
}
