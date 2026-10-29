using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories
{
    // The blocks of the open card (_3.1 section 3): the header in its own slot, pinned at the top of the sheet
    // (the title stays in view while the rest scrolls; peek shows exactly its top part), and every other block
    // in order in a ScrollView. Views come from the card's BlockViewPool: switching POIs rebinds, it never instantiates and
    // destroys a view per tap (work plan performance rule 2). Each bound view gets its own ScopedMediaSource over
    // the card's media source: it loads lazily in Bind, and the stack releases the whole scope on unbind. Once the
    // visitor scrolls the hero's title away, the pinned bar shows the title (HeroRule.BarShowsTitle; the bar never changes size). Every
    // block sits in a SLOT: the common heading
    // (BlockKindDefinition.HeadingField, or the kind's DefaultHeadingKey) above the view's root, and the one block gap
    // (--ts-block-gap) under it -- the stack draws both, so every kind has the same heading look and rhythm. The header's hero
    // (HeaderBlockView.HeroPart: the picture or the plate) opens the scroll, above the first block; the scroll drives its motion, and the
    // stack binds the card's actions into its foot (HeroActionsRule: a second view of the sticky Actions block and of the first Audio
    // Guide, hero_chip look). A block pinned to the top (the hero_chip look) gets no slot of its own: the hero's Listen is its place.
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
        // The card's views, pooled per kind: the stack's and the full view's (BlockFullView takes a SECOND view of a kind from here)
        public BlockViewPool Views { get; }

        private readonly BlockRegistry _registry;
        private readonly VisualElement _sheetRoot;
        // The bars float over the hero's picture: white marks on a soft top shade (PoiCard.uss)
        public const string BarsOnHeroClass = "poi-card--bars-on-hero";

        // The chrome floats over the stack (_3.3 S4a review): TopInset is the bars' foot (the content starts under them, the hero's picture
        // runs up behind them), LandingInset the foot of everything floating there (the bars and the top strip): where a jump lands a block
        // and where the hero's words finish fading. Set by the sheet from its measured layout
        public float TopInset { get; private set; }
        public float LandingInset { get; private set; }

        public void SetInsets(float top, float landing)
        {
            TopInset = Mathf.Max(0f, float.IsNaN(top) ? 0f : top);
            LandingInset = Mathf.Max(TopInset, float.IsNaN(landing) ? 0f : landing);
            Scroll.contentContainer.style.paddingTop = TopInset;
            if (HeaderView != null)
            {
                HeaderView.FadeTop = LandingInset;
                HeaderView.OnStackScrolled(Scroll.scrollOffset.y);
            }
        }
        // The scroll's chapters: every scroll slot sits in its family's glass sheet (_3.3 P4.4c1), so a slot's layout is relative to its
        // sheet -- TopInScroll is the one way to its place in the scroll
        public SectionSheets Sheets { get; }
        // The card's moving light (_3.3 P4.4c2): the sheets' living glass and the rim light of the block being read
        public CardLight Light { get; }
        public RimLightView RimLight { get; }
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
        // The views bound into the hero's action row (the sticky action, the Listen button): their kind and media scope, given back on unbind
        private readonly List<(string Kind, IBlockView View, ScopedMediaSource Media)> _heroViews = new();
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

        // The class a compact row's slot wears (its heading is the row's words, so the slot's own heading stays hidden)
        public const string CompactSlotClass = "card-block-slot--compact";
        // The class a slot heading wears over a block that draws its own title (a composed kind): hidden, its words kept for navigation
        public const string OwnTitleClass = "card-block-heading--own-title";
        // Look Blocks = Open (_3.3 P3): a scroll block sits open on its section (no frame), the stack on the dense open ground
        public const string OpenSlotClass = "card-block-slot--open";
        public const string OpenStackClass = "poi-card__stack--open";

        // Whether the pinned bar shows the card's title now (HeroRule.BarShowsTitle: at Peek, or the hero's title scrolled away)
        public bool BarShowsTitle { get; private set; }

        // The views at the foot of the hero, in order (the primary action first, then Listen)
        public IReadOnlyList<IBlockView> HeroActionViews
        {
            get
            {
                var views = new List<IBlockView>(_heroViews.Count);
                foreach (var h in _heroViews) views.Add(h.View);
                return views;
            }
        }

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

        // How many files the blocks' views hold now, all scopes together (a closed compact row holds none; the hero's action views count
        // too): what "a card of collapsed rows holds no media" is counted by
        public int HeldMedia
        {
            get
            {
                int held = 0;
                foreach (var b in _bound) held += b.Media?.HeldCount ?? 0;
                foreach (var h in _heroViews) held += h.Media?.HeldCount ?? 0;
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

        // The slot element holding a bound view and its heading (the block's full footprint in the stack). In the scroll its layout is
        // relative to its section sheet: its place in the scroll is TopInScroll(slot)
        public VisualElement SlotOf(IBlockView view) => BoundOf(view)?.Slot.Root;

        // The slot of the block with this instance key (null when no such block is bound): its compact row's too (see SlotOf)
        public VisualElement SlotOfBlock(string blockKey) => BoundOf(blockKey)?.Slot.Root;

        // Whether this element sits in the scrolling stack (a scroll slot inside its sheet), not the pinned header or the footer
        public bool IsInScroll(VisualElement element)
        {
            for (var e = element?.parent; e != null; e = e.parent)
                if (e == Scroll.contentContainer) return true;
            return false;
        }

        // Where an element of the scroll starts, in the scroll's content coordinates: its layout's y summed up its parents to the content
        // (a slot is laid out in its sheet, the sheet in the content). Layout, not worldBound: a transform (a block rising in) never moves it.
        // NaN when the element is not in the scroll or not laid out yet
        public float TopInScroll(VisualElement element)
        {
            float top = 0f;
            for (var e = element; e != null; e = e.parent)
            {
                if (e == Scroll.contentContainer) return top;
                top += e.layout.y;
            }
            return float.NaN;
        }

        // Whether the scroll's places are final: the stop shows the blocks, and the hero (a card with a picture hero) stands at the height the
        // sheet gave it -- the sheet sets it after the first layout pass, and until then every block sits a hero's height too high (an open
        // section far down the card read as "in view" right after a bind)
        public bool LaidOut
        {
            get
            {
                if (_context?.Host != null && !SheetStopRule.RevealsBlocks(_context.Host.Stop)) return false;
                var hero = HeaderView;
                if (hero == null || !hero.HasHero || !hero.HasPicture) return true;
                return !float.IsNaN(hero.HeroHeight) && hero.HeroPart.layout.height >= hero.HeroHeight - 1f;
            }
        }

        // How far above the viewport's foot the chrome floating over it begins (a Navigation Bar at the bottom: its band's resting top), asked
        // on every scroll moment -- where an open section's Start has cleared it (SectionSheets.OnScrolled). The sheet sets it; none = 0
        public System.Func<float> FootInset { get; set; } = () => 0f;

        // Where the scroll rests for a jump to this slot (its top meets the viewport's top): the first block a sheet shows lands on the
        // SHEET's top, its opener in view (R10.7: the opener names the chapter it opens); any other block on its own top. NaN when not in
        // the scroll
        // (_3.3 S4a review: under the chrome floating over the view -- LandingInset -- so it lands in sight)
        public float LandingTop(VisualElement slot) => TopInScroll(LandingElement(slot)) - LandingInset;

        // The element whose top a jump to this slot puts at the viewport's top: its section sheet for the sheet's first shown block, else
        // the slot itself
        public VisualElement LandingElement(VisualElement slot)
        {
            var sheet = Sheets.SheetOf(slot);
            return sheet != null && Sheets.IsFirstShown(slot) ? sheet.Root : slot;
        }

        // LandingTop of the block with this instance key (NaN when it is not bound in the scroll)
        public float LandingTopOf(string blockKey)
        {
            var slot = SlotOfBlock(blockKey);
            return slot != null && IsInScroll(slot) ? LandingTop(slot) : float.NaN;
        }

        // What of this block the visitor can read now, in the stack's content coordinates: a block drawn whole -- its whole slot; an open
        // compact row -- the view bound under its line (the line alone says nothing of the block); a closed row -- nothing (`shown` false).
        // False when no such block is bound. What NavScrollRule.HasViewed measures (the stack measures, the rule decides)
        public bool TryGetReadablePart(string blockKey, out bool shown, out float top, out float bottom)
        {
            var b = BoundOf(blockKey);
            shown = false;
            top = bottom = float.NaN;
            if (b == null) return false;
            // - in the scroll's coordinates (the slot is laid out in its sheet)
            float slotTop = IsInScroll(b.Slot.Root) ? TopInScroll(b.Slot.Root) : b.Slot.Root.layout.y;
            float slotBottom = slotTop + b.Slot.Root.layout.height;
            if (!b.Compact) { shown = true; top = slotTop; bottom = slotBottom; return true; }
            // - a closed row: its line's real box, with nothing of the block shown -- the RULE says that is never seen, not a missing measure
            if (b.View == null) { top = slotTop; bottom = slotBottom; return true; }
            // - the view is the slot's child: its box is relative to the slot
            var view = b.View.Root.layout;
            shown = true;
            top = slotTop + view.y;
            bottom = slotTop + view.yMax;
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

        // The element the peek stop must show in full: the header's bar (its whole root for another header view)
        public VisualElement PeekPart
        {
            get
            {
                if (_bound.Count == 0 || _bound[0].Kind != BuiltInBlocks.HeaderKind) return HeaderSlot;
                return _bound[0].View is HeaderBlockView header ? header.PeekPart : _bound[0].View.Root;
            }
        }

        public BlockStackView(VisualElement headerParent, VisualElement scrollParent, BlockRegistry registry)
        {
            _registry = registry;
            _sheetRoot = headerParent;
            Views = new BlockViewPool(registry);
            HeaderSlot = new VisualElement { name = "poi-card-header" };
            HeaderSlot.AddToClassList("poi-card__header");
            headerParent.Add(HeaderSlot);
            // - a phone card scrolls by dragging its content: no desktop scroll bar (it took width and ignored the tokens)
            Scroll = new ScrollView(ScrollViewMode.Vertical)
            {
                name = "poi-card-stack",
                verticalScrollerVisibility = ScrollerVisibility.Hidden,
                horizontalScrollerVisibility = ScrollerVisibility.Hidden,
            };
            Scroll.AddToClassList("poi-card__stack");
            scrollParent.Add(Scroll);
            Light = new CardLight(Scroll);
            Sheets = new SectionSheets(Scroll.contentContainer, registry, Light);
            RimLight = new RimLightView(this, Light);
            Footer = new VisualElement { name = "poi-card-footer" };
            Footer.AddToClassList("poi-card__footer");
            scrollParent.Add(Footer);
            Scroll.verticalScroller.valueChanged += _ =>
            {
                if (HeaderView != null) HeaderView.FadeTop = LandingInset;
                HeaderView?.OnStackScrolled(Scroll.scrollOffset.y);
                UpdateBarTitle();
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
            if (!revealed) return;
            Sheets.Refresh();
            Revealed?.Invoke();
        }

        // Ask the card's reading again when something outside the stack's scroll was read (a section of the full view, bento)
        internal void ReadAgain() => UpdateContentSeen();

        // Show the title in the bar, or leave it to the hero, for the stop and the scroll now (HeroRule.BarShowsTitle): on every scroll,
        // and when the sheet changes stop (the sheet calls it). A header that is no HeaderBlockView keeps its own title
        public void UpdateBarTitle()
        {
            var header = HeaderView;
            var stop = _context?.Host?.Stop ?? SheetStopRule.Stop.Peek;
            // - the hero's title is gone once it passed under the floating bars (their foot is TopInset down the view)
            BarShowsTitle = header == null || HeroRule.BarShowsTitle(stop, Scroll.scrollOffset.y + TopInset, header.HeroTitleBottom());
            header?.SetBarTitleShown(BarShowsTitle);
            _sheetRoot.EnableInClassList(BarsOnHeroClass, header != null && header.HasPicture && !BarShowsTitle);
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
            bool open = SectionStyleRule.IsOpen(context.Settings?.container);
            Scroll.EnableInClassList(OpenStackClass, open);
            Light.SetReduceMotion(context.ReduceMotion);
            // - the scroll's slots go into their section sheets once they are all bound (Sheets.Build), in stack order
            var scrollSlots = new List<VisualElement>();
            var scrollEntries = new List<BlockStackBuilder.Entry>();
            // - the sticky call to action (a footer look) is left out where the bar's Sticky Action says (Automatic: under a bottom bar, where
            //   the Locate Button stands): no footer, no hero pill
            bool sticky = NavBarRule.StickyActionShown(navigation);
            foreach (var entry in entries)
            {
                // - a block pinned to the top (the hero_chip look) lives in the hero's action row now (BindHeroActions): no slot
                if (entry.Definition.IsPinnedTop(entry.Variant)) continue;
                if (!sticky && entry.Definition.IsFooter(entry.Variant)) continue;
                bool header = _bound.Count == 0 && entry.Definition.Key == BuiltInBlocks.HeaderKind;
                bool inScroll = !header && !entry.Definition.IsFooter(entry.Variant);
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
                // - a composed block draws its own title (the title IS its heading, _3.3 S2b): the words stay for navigation, unseen
                b.Slot.Heading.EnableInClassList(OwnTitleClass, BlockSurfaces.Composes(entry.Definition));
                var parent = header ? HeaderSlot
                    : entry.Definition.IsFooter(entry.Variant) ? Footer : Scroll.contentContainer;
                // - a block of the scroll sits in its frame; the bar and the footer are chrome, not blocks
                BlockFrame.Apply(b.Slot.Root, inScroll && !open, entry.Instance?.emphasis);
                b.Slot.Root.EnableInClassList(OpenSlotClass, inScroll && open);
                if (inScroll)
                {
                    scrollSlots.Add(b.Slot.Root);
                    scrollEntries.Add(entry);
                }
                else parent.Add(b.Slot.Root);
                // - the header's hero opens what scrolls: the bar stays pinned while the hero scrolls away
                if (header && b.View is HeaderBlockView h && h.HasHero) Scroll.contentContainer.Insert(0, h.HeroPart);
                _bound.Add(b);
            }
            Sheets.Build(scrollSlots, scrollEntries, context);
            BindHeroActions(entries, context);
            // - rows open before the rebind open again, in the order they were opened (Back keeps closing the last one first)
            if (reopen != null)
                foreach (string key in reopen) Expand(key);
            Scroll.scrollOffset = UnityEngine.Vector2.zero;
            // - a new card starts at the top, its hero whole (the offset may already have been 0: no scroll event then)
            HeaderView?.OnStackScrolled(0f);
            UpdateBarTitle();
            RimLight.Update();
        }

        // The card's actions at the foot of the hero (HeroActionsRule): a second view of the sticky Actions block (the primary action, in
        // its own look) and of the first Audio Guide in the hero_chip look (Listen), each from the pool through a media scope of its own --
        // the same blocks the footer and the stack draw, so one action and one audio owner, drawn twice
        private void BindHeroActions(IReadOnlyList<BlockStackBuilder.Entry> entries, BlockBindContext context)
        {
            var header = HeaderView;
            if (header == null) return;
            var picks = HeroActionsRule.Of(entries);
            if (!NavBarRule.StickyActionShown(context.Settings?.navigation)) picks = new HeroActionsRule.Picks(-1, picks.Listen);
            if (picks.HasPrimary) BindHeroView(header, entries[picks.Primary], entries[picks.Primary].Variant, HeroPrimaryClass, context);
            if (picks.HasListen) BindHeroView(header, entries[picks.Listen], BuiltInBlocks.AudioGuideHeroChip, HeroListenClass, context);
            header.Actions.style.display = _heroViews.Count > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            header.Actions.EnableInClassList("card-hero__actions--listen-only", picks.HasListen && !picks.HasPrimary);
        }

        // The roles the hero's two action views wear (Media.uss styles them over the scrim)
        public const string HeroPrimaryClass = "card-hero__primary";
        public const string HeroListenClass = "card-hero__listen";

        private void BindHeroView(HeaderBlockView header, BlockStackBuilder.Entry entry, string variant, string role, BlockBindContext context)
        {
            var view = Views.Take(entry.Definition.Key);
            if (view == null) return;
            var media = new ScopedMediaSource(context.Media);
            view.Bind(entry.Instance, context.ForBlock(media, variant));
            view.Root.AddToClassList(role);
            header.Actions.Add(view.Root);
            _heroViews.Add((entry.Definition.Key, view, media));
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
            float rowTop = TopInScroll(b.Slot.Root);
            float removed = b.View.Root.layout.height;
            (b.View as ICollapsibleBlockView)?.Collapsing();
            ReleaseView(b);
            b.Slot.Row.SetExpanded(false);
            _open.Remove(blockKey);
            float now = Scroll.scrollOffset.y;
            float offset = offsetAfter switch
            {
                OffsetAfter.OwnRow => AccordionRule.OffsetAfterCollapse(now, rowTop),
                OffsetAfter.KeepOpened => AccordionRule.OffsetAfterClosingFor(now, rowTop, removed, BoundOf(openedKey) is { } opened ? TopInScroll(opened.Slot.Root) : float.NaN),
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
            Sheets.Refresh();
        }

        // Unbind every view and put it back in its kind's pool
        public void UnbindAll()
        {
            foreach (var h in _heroViews)
            {
                h.View.Unbind();
                h.Media?.ReleaseAll();
                h.View.Root.RemoveFromClassList(HeroPrimaryClass);
                h.View.Root.RemoveFromClassList(HeroListenClass);
                h.View.Root.RemoveFromHierarchy();
                Views.Return(h.Kind, h.View);
            }
            _heroViews.Clear();
            RimLight.Clear();
            foreach (var b in _bound)
            {
                if (b.View is HeaderBlockView h) h.HeroPart.RemoveFromHierarchy();
                ReleaseView(b);
                b.Slot.Root.RemoveFromHierarchy();
                b.Slot.Key = null;
                (b.Compact ? _compactSlots : _slots).Push(b.Slot);
            }
            _bound.Clear();
            Sheets.Release();
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
            BlockFrame.Build(slot.Root);
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
