using System.Collections.Generic;
using UnityEngine.UIElements;

namespace TileStories
{
    // The blocks of the open card (_3.1 section 3): the header in its own slot, pinned at the top of the sheet
    // (the title stays in view while the rest scrolls; peek shows exactly its top part), and every other block
    // in order in a ScrollView. Views come from the card's BlockViewPool: switching POIs rebinds, it never instantiates and
    // destroys a view per tap (work plan performance rule 2). Each bound view gets its own ScopedMediaSource over
    // the card's media source: it loads lazily in Bind, and the stack releases the whole scope on unbind. Once the
    // visitor scrolls the stack, the header collapses to its compact look (a class on the header slot,
    // HeaderCollapseRule decides) and opens again at the scroll top. Every block sits in a SLOT: the common heading
    // (BlockKindDefinition.HeadingField, or the kind's DefaultHeadingKey) above the view's root, and the one block gap
    // (--ts-block-gap) under it -- the stack draws both, so every kind has the same heading look and rhythm. A picture
    // header's hero (HeaderBlockView.HeroPart) opens the scroll, above the first block; the scroll drives its parallax.
    // A block whose display is Expandable (_3.2 step 12, BlockDisplayRule.ShowsCompact) sits in its slot as a COMPACT ROW (CompactRowView,
    // CompactRowRule's words) and holds NO view and no media: its real view is taken from the pool and bound only when the row is opened
    // (Expand), and unbound -- its scope released -- when it is closed again (Collapse). Rows left open survive a rebind of the same
    // point (a live edit, a language switch); another point, or the card closing, starts every row closed.
    public sealed class BlockStackView
    {
        public VisualElement HeaderSlot { get; }
        public ScrollView Scroll { get; }
        // Pinned under the scroll: the blocks whose variant is a footer (BlockKindDefinition.FooterVariants)
        public VisualElement Footer { get; }
        // Pinned right under the header, above the scroll: the blocks whose variant is pinned to the top (BlockKindDefinition.PinnedTopVariants)
        public VisualElement PinnedTop { get; }
        // The card's views, pooled per kind: the stack's and the full view's (BlockFullView takes a SECOND view of a kind from here)
        public BlockViewPool Views { get; }

        private readonly BlockRegistry _registry;
        // One block's place in the stack: its heading, then its view -- or, for an expandable block, its compact row and (once opened) its
        // view under it. Slots are pooled apart from the views, one pool for each shape
        private sealed class Slot
        {
            public VisualElement Root;
            public Label Heading;
            public CompactRowView Row;
            // The instance key of the block the slot holds now (what a compact row's taps act on)
            public string Key;
        }

        // A block bound in the stack: its slot, its view and the view's media scope (both null while a compact row is closed)
        private sealed class Bound
        {
            public string Kind;
            public string Key;
            public Slot Slot;
            public IBlockView View;
            public ScopedMediaSource Media;
            public BlockStackBuilder.Entry Entry;
            public bool Compact;
        }

        private readonly Stack<Slot> _slots = new();
        private readonly Stack<Slot> _compactSlots = new();
        private readonly List<Bound> _bound = new();
        // The slots of blocks kept hidden until the card was read (ShowAfterViewedField), with what remembers their reveal
        private readonly List<(Slot Slot, string PoiId, string BlockKey)> _waiting = new();
        // The block keys this binding revealed itself (a card with no CardLocalState still shows them once read)
        private readonly HashSet<string> _revealedHere = new();
        // The compact rows open now, and the point they belong to (kept across a rebind of that same point)
        private readonly HashSet<string> _expanded = new();
        private string _expandedPoi;
        private CardLocalState _state;
        // The stack's own context: what an expandable block's view is bound with when it is opened
        private BlockBindContext _context;

        // Whether the visitor has scrolled past the card's content since it was bound (ContentSeenRule): it latches, and a new
        // card starts unseen
        public bool ContentSeen { get; private set; }

        // Whether a block's ShowAfterViewed gate has opened, by its instance key: remembered on this device (CardLocalState.Seen)
        // or revealed by this binding. The ONE predicate the stack hides a slot on (NavOntologyRule.WaitsToBeRevealed) and the
        // sheet builds the card's CardNavModel with, so the stack and the navigation can never disagree about a hidden block
        public System.Func<string, bool> IsRevealed { get; private set; } = _ => false;

        // Raised when blocks that waited for the card to be read were revealed (the navigation has new targets)
        public event System.Action Revealed;

        // Raised when a compact row was opened or closed (its instance key)
        public event System.Action<string> ExpandedChanged;

        // How many views were ever built (pooling makes this stop growing once every kind was seen)
        public int CreatedViewCount => Views.CreatedCount;

        // The class that turns the pinned header into its compact look (About.uss / PoiCard.uss hide what it gives up)
        public const string CollapsedHeaderClass = "poi-card__header--collapsed";
        // The class a compact row's slot wears (its heading is the row's words, so the slot's own heading stays hidden)
        public const string CompactSlotClass = "card-block-slot--compact";

        // Whether the header is collapsed now (the stack was scrolled down)
        public bool HeaderCollapsed { get; private set; }

        // --ts-header-collapsed-max-height (CardTokens.uss), handed over by the sheet that reads the card's tokens: the one-line
        // collapsed header's ceiling, what "freed" is measured against (0 until the styles resolve: then freed = the whole
        // header, the safe over-estimate)
        internal float CollapsedCeiling { get; set; }

        // The bound views in stack order (the header first): a compact row that is closed has none
        public IReadOnlyList<IBlockView> BoundViews
        {
            get
            {
                var views = new List<IBlockView>(_bound.Count);
                foreach (var b in _bound)
                    if (b.View != null) views.Add(b.View);
                return views;
            }
        }

        // The kind key of each block in the stack, in stack order (the header first), a compact row included: the stack's own answer to
        // "what is shown, and in which order", which is what a Block Order test asserts on (_3.2 section 2.3)
        public IReadOnlyList<string> BoundKinds
        {
            get
            {
                var kinds = new List<string>(_bound.Count);
                foreach (var b in _bound) kinds.Add(b.Kind);
                return kinds;
            }
        }

        // How many files the blocks' views hold now, all scopes together (a closed compact row holds none): what "a card of collapsed
        // rows holds no media" is counted by
        public int HeldMedia
        {
            get
            {
                int held = 0;
                foreach (var b in _bound) held += b.Media?.HeldCount ?? 0;
                return held;
            }
        }

        // The heading drawn above a bound view (hidden when it has no text)
        public Label HeadingOf(IBlockView view) => BoundOf(view)?.Slot.Heading;

        // The words drawn above the block with this instance key: its own heading, else its kind's default -- "" when it shows none
        // or no such block is bound. What a navigation element that names a block (Up next) says, so it reads what the visitor will see
        public string HeadingOfBlock(string blockKey) => BoundOf(blockKey)?.Slot.Heading.text ?? "";

        // The stack entry (kind, instance, look) of the block with this instance key; false when no such block is bound: what a block
        // that lists OTHER blocks of the card reads them from (nav_contents' Reading Time, its clip lengths), and what the full view binds
        public bool TryGetEntryOfBlock(string blockKey, out BlockStackBuilder.Entry entry)
        {
            var b = BoundOf(blockKey);
            entry = b?.Entry ?? default;
            return b != null;
        }

        // The slot element holding a bound view and its heading (the block's full footprint in the stack)
        public VisualElement SlotOf(IBlockView view) => BoundOf(view)?.Slot.Root;

        // The slot of the block with this instance key (null when no such block is bound): its compact row's too
        public VisualElement SlotOfBlock(string blockKey) => BoundOf(blockKey)?.Slot.Root;

        // The view bound for this block now (null for a closed compact row, or no such block)
        public IBlockView ViewOfBlock(string blockKey) => BoundOf(blockKey)?.View;

        // The files this block's view holds now (0 for a closed compact row)
        public int HeldMediaOf(string blockKey) => BoundOf(blockKey)?.Media?.HeldCount ?? 0;

        // The compact row of this block (null when the block is not drawn as one)
        public CompactRowView CompactRowOf(string blockKey)
        {
            var b = BoundOf(blockKey);
            return b != null && b.Compact ? b.Slot.Row : null;
        }

        public bool IsExpanded(string blockKey) => BoundOf(blockKey) is { Compact: true, View: not null };

        // The bound header view (null when the header is another view type, or nothing is bound)
        public HeaderBlockView HeaderView => _bound.Count > 0 ? _bound[0].View as HeaderBlockView : null;

        // The element the peek stop must show in full: the header's top part (its whole root for another header view) -- or, when a block is
        // pinned to it, that pinned strip (Bind() sits it between the title/chip and the subtitle, so its bottom edge already
        // includes the title/chip above it and stays above the subtitle, never dragging the subtitle into the peek stop with it)
        public VisualElement PeekPart
        {
            get
            {
                if (PinnedTop.childCount > 0) return PinnedTop;
                if (_bound.Count == 0 || _bound[0].Kind != BuiltInBlocks.HeaderKind) return HeaderSlot;
                return _bound[0].View is HeaderBlockView header ? header.PeekPart : _bound[0].View.Root;
            }
        }

        public BlockStackView(VisualElement headerParent, VisualElement scrollParent, BlockRegistry registry)
        {
            _registry = registry;
            Views = new BlockViewPool(registry);
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
            // - nothing is read where the card shows no blocks (peek, or a card being bound): asked of the stop, never inferred from the
            //   viewport -- right after a rebind at peek one layout pass measured a 56 px viewport over not-yet-laid-out content (0 px), which
            //   read as "everything fits, seen" and latched (found at _3.2 step 12 by CardGalleryTests' show-after-reading test)
            if (_context?.Host != null && !SheetStopRule.RevealsBlocks(_context.Host.Stop)) return;
            if (!ContentSeenRule.HasSeenAll(Scroll.scrollOffset.y, range, Scroll.contentViewport.layout.height)) return;
            ContentSeen = true;
            if (_waiting.Count == 0) return;
            foreach (var (slot, poiId, blockKey) in _waiting)
            {
                slot.Root.style.display = DisplayStyle.Flex;
                _revealedHere.Add(blockKey);
                _state?.MarkSeen(poiId, blockKey);
            }
            _waiting.Clear();
            Revealed?.Invoke();
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

        // Show these entries (BlockStackBuilder's output), each bound with `context` and its own variant -- an expandable one as its compact
        // row, opened again if it was open on this same point before the rebind
        public void Bind(IReadOnlyList<BlockStackBuilder.Entry> entries, BlockBindContext context)
        {
            string poiId = context.Poi?.id;
            var reopen = poiId != null && poiId == _expandedPoi ? new HashSet<string>(_expanded) : null;
            UnbindAll();
            _context = context;
            _expandedPoi = poiId;
            _state = context.State;
            ContentSeen = false;
            var state = context.State;
            IsRevealed = key => _revealedHere.Contains(key) || (state?.Seen(poiId, key) ?? false);
            foreach (var entry in entries)
            {
                bool header = _bound.Count == 0 && entry.Definition.Key == BuiltInBlocks.HeaderKind;
                bool compact = !header && BlockDisplayRule.ShowsCompact(entry);
                var b = compact ? BindCompact(entry, context) : BindView(entry, context);
                if (b == null) continue;
                // - a block that waits for the card to be read stays out of sight (its heading and gap with it) until then: the
                //   same call the card's navigation model asks (NavOntologyRule), so a hidden block is never a nav target
                bool waits = NavOntologyRule.WaitsToBeRevealed(entry, IsRevealed);
                b.Slot.Root.style.display = waits ? DisplayStyle.None : DisplayStyle.Flex;
                if (waits) _waiting.Add((b.Slot, poiId, entry.Instance.key));
                b.Slot.Heading.text = HeadingWords(entry, context);
                b.Slot.Heading.EnableInClassList("card-block-heading--empty", b.Slot.Heading.text.Length == 0);
                var parent = header ? HeaderSlot
                    : entry.Definition.IsPinnedTop(entry.Variant) ? PinnedTop
                    : entry.Definition.IsFooter(entry.Variant) ? Footer : Scroll.contentContainer;
                parent.Add(b.Slot.Root);
                if (header && b.View is HeaderBlockView h)
                {
                    // - a picture header's hero opens what scrolls: the title stays pinned while the picture scrolls away
                    if (h.HasHero) Scroll.contentContainer.Insert(0, h.HeroPart);
                    // - PinnedTop sits INSIDE the header, between the title/chip and the subtitle (never after the subtitle): a
                    //   pinned block (the audio hero chip) must count in the peek stop WITHOUT dragging the subtitle into peek
                    //   with it -- Root's two children are always [PeekPart, subtitle] (HeaderBlockView's constructor), so index 1
                    //   is exactly between them. Re-inserting an already-attached element just moves it: harmless on every rebind.
                    h.Root.Insert(1, PinnedTop);
                }
                _bound.Add(b);
                if (compact && reopen != null && reopen.Contains(b.Key)) Expand(b.Key);
            }
            Scroll.scrollOffset = UnityEngine.Vector2.zero;
            // - a new card starts at the top, header open (the offset may already have been 0: no scroll event then)
            SetHeaderCollapsed(false);
        }

        // The words the stack draws over a block: its own heading, else its kind's default heading
        private static string HeadingWords(BlockStackBuilder.Entry entry, BlockBindContext context)
        {
            string heading = new BlockFieldReader(entry.Instance, context.Language, context.FallbackLanguage).Text(BlockKindDefinition.HeadingField);
            if (heading.Length == 0 && entry.Definition.DefaultHeadingKey != null && context.Strings != null)
                heading = context.Strings.Get(entry.Definition.DefaultHeadingKey);
            return heading;
        }

        // A block drawn whole: a slot with its heading and a view from the pool, bound through its own media scope
        private Bound BindView(BlockStackBuilder.Entry entry, BlockBindContext context)
        {
            var view = Views.Take(entry.Definition.Key);
            if (view == null) return null;
            var slot = TakeSlot(compact: false);
            var media = new ScopedMediaSource(context.Media);
            view.Bind(entry.Instance, context.ForBlock(media, entry.Variant));
            slot.Root.Add(view.Root);
            slot.Key = entry.Instance.key;
            return new Bound { Kind = entry.Definition.Key, Key = entry.Instance.key, Slot = slot, View = view, Media = media, Entry = entry };
        }

        // A block drawn as its compact row: no view, no media until it is opened
        private Bound BindCompact(BlockStackBuilder.Entry entry, BlockBindContext context)
        {
            var slot = TakeSlot(compact: true);
            slot.Key = entry.Instance.key;
            slot.Row.SetExpanded(false);
            System.Func<string, string> words = context.Strings != null ? context.Strings.Get : null;
            slot.Row.Show(CompactRowRule.Of(entry, _registry, context.Language, context.FallbackLanguage, words), context.Strings);
            return new Bound { Kind = entry.Definition.Key, Key = entry.Instance.key, Slot = slot, Entry = entry, Compact = true };
        }

        // Open a compact row: its block's real view is taken from the pool and bound under it, through a media scope of its own. False when
        // the block is no compact row, already open, or not bound
        public bool Expand(string blockKey)
        {
            var b = BoundOf(blockKey);
            if (b == null || !b.Compact || b.View != null || _context == null) return false;
            var view = Views.Take(b.Kind);
            if (view == null) return false;
            b.Media = new ScopedMediaSource(_context.Media);
            view.Bind(b.Entry.Instance, _context.ForBlock(b.Media, b.Entry.Variant));
            b.View = view;
            b.Slot.Root.Add(view.Root);
            b.Slot.Row.SetExpanded(true);
            _expanded.Add(blockKey);
            ExpandedChanged?.Invoke(blockKey);
            return true;
        }

        // Close an open compact row: a view that keeps something going for the card (a playing video) lets it go first, then the view is
        // unbound, its media scope released and the view given back to the pool -- the row holds nothing again. False when it was not open
        public bool Collapse(string blockKey)
        {
            var b = BoundOf(blockKey);
            if (b == null || !b.Compact || b.View == null) return false;
            (b.View as ICollapsibleBlockView)?.Collapsing();
            ReleaseView(b);
            b.Slot.Row.SetExpanded(false);
            _expanded.Remove(blockKey);
            ExpandedChanged?.Invoke(blockKey);
            return true;
        }

        // Unbind every view and put it back in its kind's pool
        public void UnbindAll()
        {
            foreach (var b in _bound)
            {
                if (b.View is HeaderBlockView h) h.HeroPart.RemoveFromHierarchy();
                ReleaseView(b);
                b.Slot.Root.RemoveFromHierarchy();
                b.Slot.Key = null;
                (b.Compact ? _compactSlots : _slots).Push(b.Slot);
            }
            _bound.Clear();
            _waiting.Clear();
            _revealedHere.Clear();
            _expanded.Clear();
            _context = null;
        }

        // Unbind a block's view, release its scope and give the view back (nothing when it has none)
        private void ReleaseView(Bound b)
        {
            if (b.View == null) return;
            b.View.Unbind();
            b.Media?.ReleaseAll();
            b.View.Root.RemoveFromHierarchy();
            Views.Return(b.Kind, b.View);
            b.View = null;
            b.Media = null;
        }

        private Bound BoundOf(string blockKey)
        {
            foreach (var b in _bound)
                if (b.Key == blockKey) return b;
            return null;
        }

        private Bound BoundOf(IBlockView view)
        {
            if (view == null) return null;
            foreach (var b in _bound)
                if (b.View == view) return b;
            return null;
        }

        // A free slot of one shape: a heading and room for a view, or a compact row's (whose taps open / close the block it holds now, and
        // ask the card for the full view)
        private Slot TakeSlot(bool compact)
        {
            var pool = compact ? _compactSlots : _slots;
            if (pool.Count > 0) return pool.Pop();
            var slot = new Slot { Root = new VisualElement(), Heading = new Label() };
            slot.Root.AddToClassList("card-block-slot");
            slot.Heading.AddToClassList("card-block-heading");
            slot.Root.Add(slot.Heading);
            if (compact)
            {
                slot.Root.AddToClassList(CompactSlotClass);
                slot.Row = new CompactRowView();
                slot.Root.Add(slot.Row.Root);
                slot.Row.Toggled += () =>
                {
                    if (slot.Key == null) return;
                    if (!Collapse(slot.Key)) Expand(slot.Key);
                };
                slot.Row.FullViewRequested += () =>
                {
                    if (slot.Key != null) _context?.Host?.OpenFullView(slot.Key);
                };
            }
            return slot;
        }
    }
}
