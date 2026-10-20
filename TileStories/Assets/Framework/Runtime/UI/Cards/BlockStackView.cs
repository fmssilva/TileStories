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
    // The card's LAYOUT (_3.2 step 13, CardLayoutRule) only moves these same slots: the accordion makes every navigation target a compact
    // row (Bind), the tabs layout sets the sections not shown aside (SetParked: out of sight and unbound, so they hold no media). A view never
    // learns which layout it is in.
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
            // In the scroll (not the header, a pinned-top or a footer look): only these a layout may set aside
            public bool InScroll;
            // Kept out of sight until the card was read (ShowAfterViewedField)
            public bool Waits;
            // Set aside by the layout (tabs: a section not shown): out of sight AND unbound -- it holds no view and no media
            public bool Parked;
        }

        private readonly Stack<Slot> _slots = new();
        private readonly Stack<Slot> _compactSlots = new();
        private readonly List<Bound> _bound = new();
        // The block keys this binding revealed itself (a card with no CardLocalState still shows them once read)
        private readonly HashSet<string> _revealedHere = new();
        // The compact rows open now IN THE ORDER THEY WERE OPENED (Back closes the last, AccordionRule), and the point they belong to (kept
        // across a rebind of that same point)
        private readonly List<string> _open = new();
        private string _expandedPoi;
        private string _poiId;
        private CardLocalState _state;
        // The stack's own context: what an expandable block's view is bound with when it is opened
        private BlockBindContext _context;

        // Whether the visitor has read the card since it was bound (ContentSeenRule.HasReadCard): it latches, and a new card starts unseen
        public bool ContentSeen { get; private set; }

        // The card's sections as its reading sees them (_3.2 step 14), handed over by the sheet from its layout: a section set aside (tabs,
        // pages) counts only once it was read to its end. Null = every block is in this one scroll
        internal System.Func<IEnumerable<ContentSeenRule.Section>> Reading { get; set; }

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

        // What the compact row says about this block (CompactRowRule over its entry, read from data only -- no view, no file), whether or not
        // the stack draws it as a row, a set-aside block of another page included: what a part that names OTHER blocks draws (the pages'
        // arriving cue, a rail's card, a bento tile). An empty row when no such block is bound
        public CompactRowRule.Row RowOfBlock(string blockKey, string language, string fallback, System.Func<string, string> words) =>
            CompactRowRule.Of(TryGetEntryOfBlock(blockKey, out var entry) ? entry : default, _registry, language, fallback, words);

        // The slot element holding a bound view and its heading (the block's full footprint in the stack)
        public VisualElement SlotOf(IBlockView view) => BoundOf(view)?.Slot.Root;

        // The slot of the block with this instance key (null when no such block is bound): its compact row's too
        public VisualElement SlotOfBlock(string blockKey) => BoundOf(blockKey)?.Slot.Root;

        // What of this block the visitor can read now, in the stack's content coordinates: a block drawn whole -- its whole slot; an open
        // compact row -- the view bound under its line (the line alone says nothing of the block); a closed row -- nothing (`shown` false).
        // False when no such block is bound. What NavScrollRule.HasViewed measures (the stack measures, the rule decides)
        public bool TryGetReadablePart(string blockKey, out bool shown, out float top, out float bottom)
        {
            var b = BoundOf(blockKey);
            shown = false;
            top = bottom = float.NaN;
            if (b == null) return false;
            var slot = b.Slot.Root.layout;
            if (!b.Compact) { shown = true; top = slot.y; bottom = slot.yMax; return true; }
            // - a closed row: its line's real box, with nothing of the block shown -- the RULE says that is never seen, not a missing measure
            if (b.View == null) { top = slot.y; bottom = slot.yMax; return true; }
            // - the view is the slot's child: its box is relative to the slot
            var view = b.View.Root.layout;
            shown = true;
            top = slot.y + view.y;
            bottom = slot.y + view.yMax;
            return true;
        }

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

        // The compact rows open now, the one opened first first (what Back and Open One At A Time read, AccordionRule)
        public IReadOnlyList<string> OpenRows => _open;

        // Whether the layout has set this block aside now (tabs: its section is not shown)
        public bool IsParked(string blockKey) => BoundOf(blockKey)?.Parked ?? false;

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
            bool atItsEnd = ContentSeenRule.HasSeenAll(Scroll.scrollOffset.y, range, Scroll.contentViewport.layout.height);
            // - the end of what the stack shows is the end of the card only when no section is set aside unread (tabs, pages)
            if (!ContentSeenRule.HasReadCard(atItsEnd, Reading?.Invoke())) return;
            ContentSeen = true;
            bool revealed = false;
            foreach (var b in _bound)
            {
                if (!b.Waits) continue;
                b.Waits = false;
                ApplyDisplay(b);
                _revealedHere.Add(b.Key);
                _state?.MarkSeen(_poiId, b.Key);
                revealed = true;
            }
            if (revealed) Revealed?.Invoke();
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

        // Show these entries (BlockStackBuilder's output), each bound with `context` and its own variant -- a compact one (its own display, or
        // the card's accordion layout: CardLayoutRule.ShowsCompact) as its compact row, opened again if it was open on this same point before
        // the rebind. `parked` (null = none): the blocks the layout sets aside from the start (tabs: the sections not shown), bound with no
        // view -- the layout gives them their views when it shows them (SetParked)
        public void Bind(IReadOnlyList<BlockStackBuilder.Entry> entries, BlockBindContext context, System.Func<string, bool> parked = null)
        {
            string poiId = context.Poi?.id;
            var reopen = poiId != null && poiId == _expandedPoi ? new List<string>(_open) : null;
            UnbindAll();
            _context = context;
            _expandedPoi = poiId;
            _poiId = poiId;
            _state = context.State;
            ContentSeen = false;
            var state = context.State;
            IsRevealed = key => _revealedHere.Contains(key) || (state?.Seen(poiId, key) ?? false);
            var navigation = context.Settings?.navigation;
            foreach (var entry in entries)
            {
                bool header = _bound.Count == 0 && entry.Definition.Key == BuiltInBlocks.HeaderKind;
                bool inScroll = !header && !entry.Definition.IsPinnedTop(entry.Variant) && !entry.Definition.IsFooter(entry.Variant);
                bool compact = inScroll && CardLayoutRule.ShowsCompact(navigation, entry, _registry);
                bool parks = inScroll && (parked?.Invoke(entry.Instance.key) ?? false);
                var b = compact ? BindCompact(entry, context) : parks ? BindParked(entry) : BindView(entry, context);
                if (b == null) continue;
                b.InScroll = inScroll;
                b.Parked = parks;
                // - a block that waits for the card to be read stays out of sight (its heading and gap with it) until then: the
                //   same call the card's navigation model asks (NavOntologyRule), so a hidden block is never a nav target
                b.Waits = NavOntologyRule.WaitsToBeRevealed(entry, IsRevealed);
                ApplyDisplay(b);
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
            }
            // - rows open before the rebind open again, in the order they were opened (Back keeps closing the last one first)
            if (reopen != null)
                foreach (string key in reopen) Expand(key);
            Scroll.scrollOffset = UnityEngine.Vector2.zero;
            // - a new card starts at the top, header open (the offset may already have been 0: no scroll event then)
            SetHeaderCollapsed(false);
        }

        // A block is out of sight while it waits for the card to be read or the layout has set it aside
        private static void ApplyDisplay(Bound b) =>
            b.Slot.Root.style.display = b.Waits || b.Parked ? DisplayStyle.None : DisplayStyle.Flex;

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
            var b = BindParked(entry);
            if (AttachView(b, context)) return b;
            // - a kind with no view: nothing is drawn, the slot goes back to its pool
            b.Slot.Key = null;
            _slots.Push(b.Slot);
            return null;
        }

        // A block drawn whole, with its slot only: no view, no media (a block the layout set aside, or one about to get its view)
        private Bound BindParked(BlockStackBuilder.Entry entry)
        {
            var slot = TakeSlot(compact: false);
            slot.Key = entry.Instance.key;
            return new Bound { Kind = entry.Definition.Key, Key = entry.Instance.key, Slot = slot, Entry = entry };
        }

        // Give a whole block its view from the pool, bound under its heading through a scope of its own. False when the kind has no view
        private bool AttachView(Bound b, BlockBindContext context)
        {
            var view = Views.Take(b.Kind);
            if (view == null) return false;
            b.Media = new ScopedMediaSource(context.Media);
            view.Bind(b.Entry.Instance, context.ForBlock(b.Media, b.Entry.Variant));
            b.Slot.Root.Add(view.Root);
            b.View = view;
            return true;
        }

        // A block drawn as its compact row: no view, no media until it is opened
        private Bound BindCompact(BlockStackBuilder.Entry entry, BlockBindContext context)
        {
            var slot = TakeSlot(compact: true);
            slot.Key = entry.Instance.key;
            slot.Row.SetExpanded(false);
            System.Func<string, string> words = context.Strings != null ? context.Strings.Get : null;
            slot.Row.Show(CompactRowRule.Of(entry, _registry, context.Language, context.FallbackLanguage, words), context.Strings,
                offersFullView: !entry.Definition.OffersOwnFullView);
            return new Bound { Kind = entry.Definition.Key, Key = entry.Instance.key, Slot = slot, Entry = entry, Compact = true };
        }

        // Open a compact row: its block's real view is taken from the pool and bound under it, through a media scope of its own. Nothing
        // scrolls (NN/g mobile accordions: an open row never moves the page). False when the block is no compact row, already open, set
        // aside by the layout, or not bound
        public bool Expand(string blockKey)
        {
            var b = BoundOf(blockKey);
            if (b == null || !b.Compact || b.View != null || b.Parked || _context == null) return false;
            var view = Views.Take(b.Kind);
            if (view == null) return false;
            b.Media = new ScopedMediaSource(_context.Media);
            view.Bind(b.Entry.Instance, _context.ForBlock(b.Media, b.Entry.Variant));
            b.View = view;
            b.Slot.Root.Add(view.Root);
            b.Slot.Row.SetExpanded(true);
            _open.Add(blockKey);
            ExpandedChanged?.Invoke(blockKey);
            return true;
        }

        // Close an open compact row: a view that keeps something going for the card (a playing video) lets it go first, then the view is
        // unbound, its media scope released and the view given back to the pool -- the row holds nothing again. A row whose line had
        // scrolled above the viewport brings that line back to the top (AccordionRule). False when it was not open
        public bool Collapse(string blockKey) => CollapseRow(blockKey, OffsetAfter.OwnRow, null);

        // Close an open row FOR another one being opened (Open One At A Time): the opened row keeps its place on screen
        public bool CollapseFor(string blockKey, string openedKey) => CollapseRow(blockKey, OffsetAfter.KeepOpened, openedKey);

        private enum OffsetAfter { OwnRow, KeepOpened, Unchanged }

        private bool CollapseRow(string blockKey, OffsetAfter offsetAfter, string openedKey)
        {
            var b = BoundOf(blockKey);
            if (b == null || !b.Compact || b.View == null) return false;
            float rowTop = b.Slot.Root.layout.y;
            float removed = b.View.Root.layout.height;
            (b.View as ICollapsibleBlockView)?.Collapsing();
            ReleaseView(b);
            b.Slot.Row.SetExpanded(false);
            _open.Remove(blockKey);
            float now = Scroll.scrollOffset.y;
            float offset = offsetAfter switch
            {
                OffsetAfter.OwnRow => AccordionRule.OffsetAfterCollapse(now, rowTop),
                OffsetAfter.KeepOpened => AccordionRule.OffsetAfterClosingFor(now, rowTop, removed, BoundOf(openedKey)?.Slot.Root.layout.y ?? float.NaN),
                _ => now,
            };
            if (!offset.Equals(now)) Scroll.scrollOffset = new UnityEngine.Vector2(Scroll.scrollOffset.x, offset);
            ExpandedChanged?.Invoke(blockKey);
            return true;
        }

        // The layout sets blocks of the scroll aside or brings them back (tabs: the sections not shown). A block set aside goes out of
        // sight AND gives its view back -- an open row closes, a playing video lets go first (ICollapsibleBlockView), every file is released
        // -- so a hidden section holds no media; one brought back gets a view again. The header, a pinned-top and a footer look never move
        public void SetParked(System.Func<string, bool> parked)
        {
            if (_context == null) return;
            foreach (var b in _bound)
            {
                if (!b.InScroll) continue;
                bool parks = parked?.Invoke(b.Key) ?? false;
                if (parks == b.Parked) continue;
                if (parks)
                {
                    // - it leaves the screen: no offset to keep for it
                    if (b.Compact) CollapseRow(b.Key, OffsetAfter.Unchanged, null);
                    else if (b.View != null)
                    {
                        (b.View as ICollapsibleBlockView)?.Collapsing();
                        ReleaseView(b);
                    }
                }
                b.Parked = parks;
                if (!parks && !b.Compact) AttachView(b, _context);
                ApplyDisplay(b);
            }
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
            _revealedHere.Clear();
            _open.Clear();
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
