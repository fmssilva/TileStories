using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories
{
    // The card's bottom sheet (_3.1 section 4): a grabber, the close X, the pinned header and the scrolling
    // block stack. It rests at a stop (peek / half / full, SheetStopRule), follows a vertical drag on the
    // grabber or the header, and on release snaps -- or asks to close (CloseRequested) when pulled away. The X
    // asks the same. The sheet's HEIGHT is its state (the ScrollView's viewport is exactly what is visible),
    // animated by a USS transition. Plain C#: built into the parent it is handed; PoiCard.uss styles it.
    public sealed class PoiCardSheetView : IBlockHost
    {
        // Full-area layer the sheet sits at the bottom of (its height is what the stops divide)
        public VisualElement Layer { get; }
        public VisualElement Root { get; }
        public VisualElement Handle { get; }
        public Button CloseButton { get; }
        // The language chip beside the X (15.3.2): shows the code of the language a tap switches to; hidden when the wall offers one language
        public Button LanguageButton { get; }
        public BlockStackView Stack { get; }
        // Moving inside the card (_3.2 step 3): jumps and the scroll-spy over the card's navigation model, rebuilt on every Show and reveal
        public NavController Nav { get; }
        // The bottom area (_3.2 step 7a): the sticky CTA footer and, under it, the bottom navigation slot -- one part of the chrome, shown,
        // slid away and given way TOGETHER (CardChromeRule), so it is one element the chrome hides
        public VisualElement BottomArea { get; }
        // The navigation slots (_3.2 steps 5a, 7a, 10): the top strip between the header and the stack, the bottom slot under the footer, the
        // edge host over the stack viewport's right edge, and the element each one shows
        public NavSlots Slots { get; }
        // What of the chrome -- the top strip, the bottom area -- shows now (_3.2 step 5b, CardChromeRule)
        public CardChromeView Chrome { get; }
        // How the card arranges its blocks (_3.2 step 13, CardLayoutRule): the accordion's one-at-a-time rows, the tabs' shown section; the
        // controller asks it before every jump
        public CardLayoutView Layout { get; }
        // The pages layout's sideways turn (_3.2 step 14): the next page's peek, the slide with the finger, the turn
        public CardPagerView Pager { get; }
        // The bento layout's overview (_3.2 step 15b): the sections as tiles where the blocks were; a tile opens its section in the full view
        public CardBentoView Bento { get; }
        // The full-screen view over the card (a gallery's lightbox), above the sheet in the same layer
        public TakeoverView Takeover { get; }
        // Any block of the card in that full view (_3.2 step 12): a second view of its kind, while the card's own stays bound
        public BlockFullView FullView { get; }
        // What is open over the card -- the wall's Menu on a scrim, a bar's More or kinds list -- and when it closes (_3.2 step 8): a layer
        // between the sheet and the full view
        public CardMenuOverlay Overlay { get; }
        // The header's menu button (_3.2 step 8): beside the language chip, drawn only while the card has a Menu element; opens it
        public Button MenuButton { get; }
        // The row just above the sheet's top-right edge at every stop (_3.2 step 9): the placed-model chip and the Floating Button element;
        // a layer between the sheet and the menu layer
        public FloatingSlot Floating { get; }

        // Raised when the visitor closes the card (the X, or a drag / swipe below peek)
        public event Action CloseRequested;

        // Raised when the visitor taps the language chip: the language code to switch to
        public event Action<string> LanguageRequested;

        // Raised when the sheet comes to rest at another stop (Dismissed when it closes)
        public event Action<SheetStopRule.Stop> StopChanged;

        public SheetStopRule.Stop Stop { get; private set; } = SheetStopRule.Stop.Dismissed;
        public SheetStopRule.Stops Stops { get; private set; }
        public bool IsOpen => Stop != SheetStopRule.Stop.Dismissed;
        // The height the sheet is set to (it animates there); a drag writes it directly
        public float TargetHeight { get; private set; }
        public bool IsDragging { get; private set; }

        private static readonly CustomStyleProperty<float> TopGapProperty = new("--ts-sheet-top-gap");
        private static readonly CustomStyleProperty<float> CollapsedHeaderProperty = new("--ts-header-collapsed-max-height");
        private readonly BlockRegistry _registry;
        // The shown card's stack entries: what its navigation model is rebuilt from when a waiting block is revealed
        private IReadOnlyList<BlockStackBuilder.Entry> _entries;
        private float _topGap;
        private float _halfMaxRatio = CardContainerSettings.HalfMaxRatioMax;
        // The language the chip's next tap switches to ("" = the chip is hidden)
        private string _nextLanguage = "";

        // The drag's clock (velocity sampling): Time.unscaledTime in the app. A test sets its own, so a simulated drag's
        // speed is what the test says -- no real frame rate decides whether it reads as a flick (mirrors PoiCardHost.Clock)
        internal System.Func<float> Clock { get; set; } = () => Time.unscaledTime;

        private float _dragStartPointerY;
        private float _dragStartHeight;
        private float _lastSampleHeight;
        private float _lastSampleTime;
        private float _velocity;
        // A vertical drag on the top navigation strip moves the sheet like one on the header (its sideways drags are the strip's own)
        private readonly SwipeGrab _stripDrag;
        private float _stripDragStartY;
        // The shown card's media source and texts: what the full-screen view loads through and words its way back with
        private BlockBindContext _context;

        // `navElements`: the navigation elements the slots can show (null = NavElementRegistry.Shared, the app's)
        public PoiCardSheetView(VisualElement parent, BlockRegistry registry, IEnumerable<StyleSheet> styleSheets, NavElementRegistry navElements = null)
        {
            Layer = new VisualElement { name = "poi-card-layer", pickingMode = PickingMode.Ignore };
            Layer.AddToClassList("poi-card-layer");
            foreach (var sheet in styleSheets)
                if (sheet != null) Layer.styleSheets.Add(sheet);

            // - the token classes sit on the sheet itself, so its own custom style carries every --ts-* token
            Root = new VisualElement { name = "poi-card" };
            Root.AddToClassList("ts-card");
            Root.AddToClassList("ts-theme-default");
            Root.AddToClassList("poi-card");

            Handle = new VisualElement { name = "poi-card-handle" };
            Handle.AddToClassList("poi-card__handle");
            var grabber = new VisualElement { name = "poi-card-grabber", pickingMode = PickingMode.Ignore };
            grabber.AddToClassList("poi-card__grabber");
            Handle.Add(grabber);
            Root.Add(Handle);

            _registry = registry;
            Stack = new BlockStackView(Root, Root, registry);
            Nav = new NavController(Stack, () => { if (IsOpen && Stop != SheetStopRule.Stop.Full) SetStop(SheetStopRule.Stop.Full); });
            Layout = new CardLayoutView(Stack);
            Nav.Layout = Layout;
            // - the card is read once the shown part is at its end AND every section the layout set aside was read to its end before
            Stack.Reading = () => Layout.Reading(Nav.HasViewed);
            Stack.Revealed += BindNav;
            // - the footer moves into the bottom area, which takes its place under the stack: the footer first (the CTA nearest the
            //   content), the bottom slot after it (NavSlots)
            BottomArea = new VisualElement { name = "poi-card-bottom" };
            BottomArea.AddToClassList("poi-card__bottom");
            Root.Insert(Root.IndexOf(Stack.Footer), BottomArea);
            BottomArea.Add(Stack.Footer);
            // - the floating slot is a layer of its own (the sheet clips what it holds); its element host is the floating slot's
            Floating = new FloatingSlot(this);
            Slots = new NavSlots(Root, Stack.Scroll, BottomArea, Floating.Elements, navElements ?? NavElementRegistry.Shared);
            Chrome = new CardChromeView(this);
            Pager = new CardPagerView(Root, Stack.Scroll, Layout, Nav);
            Bento = new CardBentoView(Stack, Layout, Nav);

            // - no glyph: the X is two crossed bars drawn by PoiCard.uss; its name (tooltip) comes from CardStrings
            CloseButton = new Button(() => CloseRequested?.Invoke()) { name = "poi-card-close" };
            CloseButton.AddToClassList("poi-card__close");
            foreach (string bar in new[] { "poi-card__close-bar--a", "poi-card__close-bar--b" })
            {
                var stroke = new VisualElement { pickingMode = PickingMode.Ignore };
                stroke.AddToClassList("poi-card__close-bar");
                stroke.AddToClassList(bar);
                CloseButton.Add(stroke);
            }
            Root.Add(CloseButton);

            // - the chip's words are the language code (data), its name comes from CardStrings
            LanguageButton = new Button(() => { if (_nextLanguage.Length > 0) LanguageRequested?.Invoke(_nextLanguage); }) { name = "poi-card-language" };
            LanguageButton.AddToClassList("poi-card__language");
            Root.Add(LanguageButton);

            // - no glyph text: three bars drawn by the icon set; its name (tooltip) comes from CardStrings
            MenuButton = new Button(() => Overlay.OpenMenu()) { name = "poi-card-menu-button" };
            MenuButton.AddToClassList("poi-card__menu-button");
            var menuIcon = CardIcons.Create();
            CardIcons.SetKey(menuIcon, CardIcons.Menu);
            MenuButton.Add(menuIcon);
            Root.Add(MenuButton);

            Layer.Add(Root);
            Takeover = new TakeoverView(Layer);
            FullView = new BlockFullView(Takeover, Stack.Views);
            // - bento: a jump opens its section in the full view; what the visitor reads there counts as read, and closing it is the overview
            //   again (the reading asked again: the card may now have been read to its end)
            Layout.SectionOpener = OpenSectionView;
            FullView.Viewed += Nav.ViewedInFullView;
            FullView.SectionLeft += () =>
            {
                Layout.SectionViewClosed();
                if (IsOpen) Stack.ReadAgain();
            };
            // - a jump moves the card under the full view: the visitor would see nothing of it, so the full view closes (an Open Block
            //   button or a hotspot pin used inside it lands on the card)
            Nav.Jumped += Takeover.Close;
            Takeover.SectionTapped += JumpToCrumbSection;
            Overlay = new CardMenuOverlay(this);
            // - over the sheet, under the menu layer (an open drawer covers it) and the full view
            Floating.InsertUnder(Overlay);
            parent.Add(Layer);

            foreach (var dragArea in new[] { Handle, Stack.HeaderSlot })
            {
                dragArea.RegisterCallback<PointerDownEvent>(OnPointerDown);
                dragArea.RegisterCallback<PointerMoveEvent>(OnPointerMove);
                dragArea.RegisterCallback<PointerUpEvent>(OnPointerUp);
            }
            // - never taken at touch-down: a tap still reaches a tab, a sideways drag the strip; only a clearly vertical one is the sheet's
            _stripDrag = new SwipeGrab(Slots.Top, SwipeGrabRule.ClaimsVertical);
            _stripDrag.Claimed += start => { _stripDragStartY = start.y; BeginDrag(start.y); };
            _stripDrag.Moved += travel => DragTo(_stripDragStartY + travel.y);
            _stripDrag.Released += _ => EndDrag();
            _stripDrag.Cancelled += EndDrag;
            Root.RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyle);
            Layer.RegisterCallback<GeometryChangedEvent>(_ => RecomputeStops());
            Stack.HeaderSlot.RegisterCallback<GeometryChangedEvent>(_ => RecomputeStops());
            // - a block pinned under the header (the audio hero chip) is part of the peek stop: its size moves the stops too
            Stack.PinnedTop.RegisterCallback<GeometryChangedEvent>(_ => RecomputeStops());
            Hide();
        }

        // Show these blocks. A closed sheet opens at `openStop`; an open one keeps its stop (a new selection
        // while the card is open rebinds in place).
        public void Show(IReadOnlyList<BlockStackBuilder.Entry> entries, BlockBindContext context, SheetStopRule.Stop openStop, float halfMaxRatio)
        {
            _halfMaxRatio = halfMaxRatio;
            CloseButton.tooltip = context.Strings?.Get(CardStrings.Keys.Close) ?? "";
            ShowLanguageChip(context);
            context.Host ??= this;
            // - a block in the full view stays there over a rebind of the SAME point (a live edit, a language switch): it is drawn again
            //   from the new stack once the card is bound. Another point replaces the full-screen view of the old one
            bool samePoint = FullView.IsOpen && context.Poi?.id != null && context.Poi?.id == _context?.Poi?.id;
            string reopen = samePoint ? FullView.BlockKey : null;
            string reopenSection = samePoint ? FullView.SectionKey : null;
            // - and whatever was open over the old card closes
            Takeover.Close();
            Overlay.Close();
            context.Overlay = Overlay;
            // - a nav block in the stack (the icon grid) draws from the controller once BindNav below has given it this card's model
            context.Nav = Nav;
            _context = context;
            _entries = entries;
            // - before the stack binds: a block that offers a jump reads where the card can scroll (a block waiting for Show After Reading
            //   is not there yet -- the same remembered reveal the stack hides it on)
            var state = context.State;
            string poiId = context.Poi?.id;
            context.JumpTargets = BlockRefRule.JumpTargets(entries, key => state?.Seen(poiId, key) ?? false);
            // - until BindNav below, the controller's model is the replaced card's: a nav block bound now waits for it
            Nav.Rebinding();
            // - tabs and pages bind only the section they show: the card's map, built from these entries with the reveal the stack starts
            //   from (the device's remembered one), says which blocks start set aside
            var parked = CardLayoutRule.ParksSections(CardLayoutRule.Layout(context.Settings?.navigation))
                ? Layout.ParkedAtBind(CardNavModel.Build(entries, _registry, context.Settings, key => state?.Seen(poiId, key) ?? false), context)
                : null;
            Stack.Bind(entries, context, parked);
            BindNav();
            Root.style.display = DisplayStyle.Flex;
            SetStop(IsOpen ? Stop : openStop);
            Floating.Bind(context);
            // - a block the rebind dropped (a live edit deleted it) is not reopened: OpenFullView finds no entry; a section only while the card
            //   is still drawn as bento and still has it
            if (reopen != null) OpenFullView(reopen);
            else if (reopenSection != null) Layout.ReopenSection(reopenSection);
        }

        // Build the shown card's navigation model -- with the stack's own reveal predicate, so a block the stack hides is never a target --
        // and hand it to the controller (on Show: a POI switch, a live edit, a language switch; and on a reveal)
        private void BindNav()
        {
            if (_context == null || _entries == null) return;
            // - a reveal rebinds every element: a menu or list open over the old ones closes with them
            Overlay.Close();
            var model = CardNavModel.Build(_entries, _registry, _context.Settings, Stack.IsRevealed);
            // - the layout first: the controller's spy reads the section tabs shows
            Layout.Bind(model, _context);
            Pager.Bind(model, _context);
            Bento.Bind(model, _context);
            Nav.Bind(model, _context);
            Slots.Bind(Nav.Model, Nav, _context);
            Floating.Refresh();
            ShowMenuButton();
            Chrome.Bind(_context.Settings?.navigation, _context.ReduceMotion);
        }

        // The header's menu button exists only while the card has a Menu element; its name is the menu's (CardStrings)
        private void ShowMenuButton()
        {
            bool shown = Slots.MenuView != null;
            MenuButton.style.display = shown ? DisplayStyle.Flex : DisplayStyle.None;
            MenuButton.tooltip = _context?.Strings?.Get(CardStrings.Keys.NavMenu) ?? "";
            Root.EnableInClassList("poi-card--has-menu", shown);
        }

        // The chip names the language a tap switches to; with fewer than two languages there is nothing to choose and it is hidden
        private void ShowLanguageChip(BlockBindContext context)
        {
            _nextLanguage = CardLanguageRule.Next(context.Settings?.languages, context.Language);
            LanguageButton.text = CardLanguageRule.ChipLabel(context.Settings?.languages, context.Language);
            bool shown = LanguageButton.text.Length > 0;
            LanguageButton.style.display = shown ? DisplayStyle.Flex : DisplayStyle.None;
            LanguageButton.tooltip = context.Strings?.Get(CardStrings.Keys.LanguageSwitch) ?? "";
            Root.EnableInClassList("poi-card--has-language", shown);
        }

        // Close: unbind every block (views go back to their pool) and collapse
        public void Hide()
        {
            Takeover?.Close();
            Overlay?.Close();
            _context = null;
            _entries = null;
            Nav?.Unbind();
            Layout?.Unbind();
            Pager?.Unbind();
            Bento?.Unbind();
            Slots?.Unbind();
            Floating?.Unbind();
            if (MenuButton != null) ShowMenuButton();
            Chrome?.Unbind();
            Stack.UnbindAll();
            SetStopState(SheetStopRule.Stop.Dismissed);
            IsDragging = false;
            ApplyHeight(0f);
            Root.style.display = DisplayStyle.None;
        }

        // A block asked to see the POI on the wall: lower the card to peek (the selection and its marker stay)
        public void ShowOnWall()
        {
            if (!IsOpen) return;
            // - the wall is what the visitor asked to see: a full view over the card would hide it (a block asking from the full view)
            Takeover.Close();
            SetStop(SheetStopRule.Stop.Peek);
        }

        // A block asked for the header at the peek: scroll to the top first (a peek over a scrolled stack shows a half-cut line of whatever
        // block was in view), then lower the card
        public void ShowHeaderAtPeek()
        {
            if (!IsOpen) return;
            // - a model placed from the full view stands in the room: the full view goes, or it would cover what was just placed
            Takeover.Close();
            Stack.Scroll.scrollOffset = Vector2.zero;
            SetStop(SheetStopRule.Stop.Peek);
        }

        // Where the visitor stands in the wall's frame, handed over by the card's owner (the scene's camera through the wall;
        // a gallery's fabricated place); null = none known
        public System.Func<Vector3?> Viewer { get; set; }

        // Hands a block's web link to the device (Application.OpenURL in the app; a test sets its own to see the call)
        public IUrlOpener UrlOpener { get; set; } = new ApplicationUrlOpener();

        // A block asked to open a web link: only an open card, only a link the rule accepts
        public void OpenUrl(string url)
        {
            if (IsOpen && WebLinkRule.IsOpenable(url)) UrlOpener?.Open(url.Trim());
        }

        // A block asked to open another POI of the wall: through the one selection bus, like a marker tap
        public void SelectPoi(string poiId)
        {
            if (IsOpen && !string.IsNullOrEmpty(poiId)) SelectionEventBus.Select(poiId);
        }

        public bool TryGetViewer(out Vector3 wallPosition)
        {
            var viewer = Viewer?.Invoke();
            wallPosition = viewer ?? Vector3.zero;
            return viewer.HasValue;
        }

        // A block asked to move the card to another of its blocks: the same jump every navigation element makes
        public void ScrollToBlock(string blockKey, string elementKey)
        {
            if (IsOpen) Nav.JumpToBlock(blockKey, elementKey);
        }

        // A system Back press (_3.2 section 4.5): undo the innermost thing NavBackRule names -- close the full view, close what is open over
        // the card, lower the sheet one stop, or close the card through CloseRequested, exactly as its X does. False when Back is left to
        // the app (Back Button = none, or no card open)
        public bool Back()
        {
            switch (NavBackRule.Next(_context?.Settings?.navigation, Takeover.IsOpen, Overlay.IsOpen, Stack.OpenRows.Count > 0, Stop))
            {
                case NavBackRule.Step.CloseFullView: Takeover.Close(); return true;
                case NavBackRule.Step.CloseOverlay: Overlay.Close(); return true;
                case NavBackRule.Step.CollapseRow: Stack.Collapse(AccordionRule.ClosedByBack(Stack.OpenRows)); return true;
                case NavBackRule.Step.LowerToHalf: SetStop(SheetStopRule.Stop.Half); return true;
                case NavBackRule.Step.LowerToPeek: SetStop(SheetStopRule.Stop.Peek); return true;
                case NavBackRule.Step.CloseCard: CloseRequested?.Invoke(); return true;
                default: return false;
            }
        }

        // A tap on empty camera space (CardTapOutside): what is open over the card closes first -- the dial's list, the radial's arc, a bar's
        // list, a menu -- the way a tap outside closes a popover and the order Back keeps; with nothing open the card closes, as its X does
        public void TapOutside()
        {
            if (Overlay.IsOpen) Overlay.Close();
            else CloseRequested?.Invoke();
        }

        // A block asked for the full-screen view: the breadcrumb starts with this card's title and ends with `name`
        public void OpenTakeover(string name, int pageCount, int startPage, TakeoverPageDrawer drawPage) =>
            OpenPages(_ => name, pageCount, startPage, drawPage);

        // The full-screen view with a name per page; no section in the breadcrumb until ShowCrumbSection names one
        private void OpenPages(Func<int, string> nameOfPage, int pageCount, int startPage, TakeoverPageDrawer drawPage)
        {
            if (!IsOpen || _context == null) return;
            string title = Stack.BoundViews.Count > 0 && Stack.BoundViews[0] is HeaderBlockView header ? header.TitleText : "";
            // - never stacked: the full view replaces what was open over the card
            Overlay.Close();
            _crumbSection = -1;
            Takeover.Open(title, nameOfPage, pageCount, startPage, _context.Media, drawPage, _context.Strings);
        }

        // A block asked for its full view (an open compact row's button, a bento tile): the page holds a second view of its kind, one page per
        // SIBLING (FullViewSiblingRule: its section's blocks that support the full view, a swipe or a chip away), each named by the words its
        // compact row shows; the breadcrumb names the section between the card's title and the block (_3.2 step 16). A key that is not on this
        // card opens nothing
        public void OpenFullView(string blockKey)
        {
            if (!IsOpen || _context == null || !Stack.TryGetEntryOfBlock(blockKey, out var entry)) return;
            var pages = new List<BlockStackBuilder.Entry>();
            foreach (string sibling in FullViewSiblingRule.Siblings(Nav.Model, _registry, blockKey))
                if (Stack.TryGetEntryOfBlock(sibling, out var siblingEntry)) pages.Add(siblingEntry);
            if (pages.Count < 2) pages = new List<BlockStackBuilder.Entry> { entry };
            int start = Mathf.Max(0, pages.FindIndex(p => p.Instance.key == blockKey));
            Func<string, string> words = _context.Strings != null ? _context.Strings.Get : null;
            string language = _context.Language, fallback = _context.FallbackLanguage;
            OpenPages(page => CompactRowRule.Of(pages[page], _registry, language, fallback, words).Words, pages.Count, start,
                FullView.PagesFor(pages, _context));
            ShowCrumbSection(FullViewSiblingRule.SectionOf(Nav.Model, blockKey));
        }

        // The section the open full view's breadcrumb names (its index in the model; -1 = none): what a tap on it jumps to
        private int _crumbSection = -1;

        // The breadcrumb's middle segment: section `index` of the card's map, in the card's words, with its family icon
        private void ShowCrumbSection(int index)
        {
            var model = Nav.Model;
            if (!Takeover.IsOpen || model == null || index < 0 || index >= model.Sections.Count) return;
            var section = model.Sections[index];
            string words = _context.Strings != null && section.LabelKey != null ? _context.Strings.Get(section.LabelKey) : "";
            _crumbSection = index;
            Takeover.ShowSection(string.IsNullOrEmpty(words) ? section.Key : words, section.IconKey);
        }

        // The visitor tapped the breadcrumb's section: the card's ONE jump to that section (the full view closes on Jumped; in bento the jump
        // opens the section's page instead) -- no second path
        private void JumpToCrumbSection()
        {
            if (IsOpen && _crumbSection >= 0) Nav.JumpTo(_crumbSection, TakeoverView.CrumbElementKey);
        }

        // Bento: section `index` of the card's map in the full view -- its targets one under the other, landing on `landingKey` (null: its
        // top); the breadcrumb ends with the section's words. CardLayoutView asks (SectionOpener); false when nothing could be drawn
        private bool OpenSectionView(int index, string landingKey)
        {
            var model = Nav.Model;
            if (!IsOpen || _context == null || model == null || index < 0 || index >= model.Sections.Count) return false;
            var section = model.Sections[index];
            var entries = new List<BlockStackBuilder.Entry>();
            foreach (string target in section.Targets)
                if (Stack.TryGetEntryOfBlock(target, out var entry)) entries.Add(entry);
            if (entries.Count == 0) return false;
            string words = _context.Strings != null && section.LabelKey != null ? _context.Strings.Get(section.LabelKey) : "";
            OpenTakeover(string.IsNullOrEmpty(words) ? section.Key : words, 1, 0,
                FullView.SectionPageFor(entries, section.Key, Stack.HeadingOfBlock, _context, landingKey));
            return FullView.IsOpen;
        }

        // Rest at `stop` (Dismissed hides)
        public void SetStop(SheetStopRule.Stop stop)
        {
            if (stop == SheetStopRule.Stop.Dismissed) { Hide(); return; }
            SetStopState(stop);
            RecomputeStops();
        }

        private void SetStopState(SheetStopRule.Stop stop)
        {
            if (stop == Stop) return;
            Stop = stop;
            Chrome?.Apply();
            Floating?.Refresh();
            StopChanged?.Invoke(stop);
        }

        // The stops for the layer's current height and the header's current size; re-applies the rest height
        public void RecomputeStops()
        {
            float available = Layer.layout.height;
            if (float.IsNaN(available) || available <= 0f) return;
            var peekPart = Stack.PeekPart;
            // - the peek stop is the OPEN header's top part: a collapsed (one-line) header keeps the last measure, or
            //   collapsing would move the stops, resize the sheet, change the scroll range... and loop
            float peek = Stack.HeaderCollapsed ? Stops.Peek
                : float.IsNaN(peekPart.worldBound.yMax) || float.IsNaN(Root.worldBound.yMin)
                ? 0f
                : peekPart.worldBound.yMax + peekPart.resolvedStyle.marginBottom - Root.worldBound.yMin;
            Stops = SheetStopRule.Compute(available, peek, _halfMaxRatio, _topGap);
            if (IsOpen && !IsDragging) ApplyHeight(Stops.HeightOf(Stop));
            // - the chrome's room is the stop's height: new stops can give it room, or take it
            Chrome?.Apply();
        }

        private void ApplyHeight(float height)
        {
            TargetHeight = height;
            Root.style.height = height;
        }

        private void OnCustomStyle(CustomStyleResolvedEvent evt)
        {
            // - the card's tokens are declared on this root (.ts-card): the stack's header reads its ceiling from here
            if (evt.customStyle.TryGetValue(CollapsedHeaderProperty, out float ceiling)) Stack.CollapsedCeiling = ceiling;
            // - a jump glides as long as the sheet takes to rise: both are --ts-duration (never read mid-drag, where the drag class zeroes it)
            if (!IsDragging) Nav.Duration = HeightTransitionSeconds();
            if (evt.customStyle.TryGetValue(TopGapProperty, out float gap) && !Mathf.Approximately(gap, _topGap))
            {
                _topGap = gap;
                RecomputeStops();
            }
        }

        // The sheet's height transition (PoiCard.uss: transition-duration var(--ts-duration)), in seconds. Read from the resolved style:
        // Unity has no time-typed custom style property, so --ts-duration itself cannot be read as a number
        private float HeightTransitionSeconds()
        {
            foreach (var duration in Root.resolvedStyle.transitionDuration)
                return duration.unit == TimeUnit.Millisecond ? duration.value / 1000f : duration.value;
            return 0f;
        }

        // The grabber and the header take the finger at touch-down (a drag there can only move the sheet)
        private void OnPointerDown(PointerDownEvent evt)
        {
            if (!IsOpen || evt.button != 0) return;
            ((VisualElement)evt.currentTarget).CapturePointer(evt.pointerId);
            BeginDrag(evt.position.y);
            evt.StopPropagation();
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!IsDragging || !((VisualElement)evt.currentTarget).HasPointerCapture(evt.pointerId)) return;
            DragTo(evt.position.y);
            evt.StopPropagation();
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            var area = (VisualElement)evt.currentTarget;
            if (!IsDragging || !area.HasPointerCapture(evt.pointerId)) return;
            area.ReleasePointer(evt.pointerId);
            EndDrag();
            evt.StopPropagation();
        }

        // A drag of the sheet starts with the finger at this panel y (the grabber, the header or the top strip)
        private void BeginDrag(float pointerY)
        {
            if (!IsOpen) return;
            IsDragging = true;
            Root.AddToClassList("poi-card--dragging");
            _dragStartPointerY = pointerY;
            _dragStartHeight = TargetHeight;
            _lastSampleHeight = TargetHeight;
            _lastSampleTime = Clock();
            _velocity = 0f;
        }

        private void DragTo(float pointerY)
        {
            if (!IsDragging) return;
            // - panel y grows downward: dragging up (smaller y) makes the sheet taller
            float height = Mathf.Clamp(_dragStartHeight + (_dragStartPointerY - pointerY), 0f, Stops.Full);
            float now = Clock();
            // - several events in one frame have no time between them: keep the last measured speed
            if (now > _lastSampleTime)
            {
                _velocity = (height - _lastSampleHeight) / (now - _lastSampleTime);
                _lastSampleTime = now;
                _lastSampleHeight = height;
            }
            ApplyHeight(height);
        }

        // The finger let go: snap to the stop the drag and its speed point at, or close
        private void EndDrag()
        {
            if (!IsDragging) return;
            IsDragging = false;
            Root.RemoveFromClassList("poi-card--dragging");
            var stop = SheetStopRule.Snap(Stops, TargetHeight, _velocity, Layer.layout.height);
            if (stop == SheetStopRule.Stop.Dismissed) CloseRequested?.Invoke();
            else SetStop(stop);
        }
    }
}
