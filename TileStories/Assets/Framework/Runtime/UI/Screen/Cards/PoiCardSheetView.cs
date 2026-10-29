using System;
using System.Collections.Generic;
using TileStories.LookKit;
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
        // The soft fade behind the bars while they float over the hero's picture
        public VisualElement TopShade { get; }
        // The band behind the chrome row once the hero is read, as the card's Top Band says (v6: Fade -- the sheet's colour fading out, no
        // edge -- or Glass -- a dense band with a hairline at its foot); gone over the hero's picture
        public VisualElement ChromeBand { get; }
        public const string ChromeBandGlassClass = "poi-card__chrome-band--glass";
        // Top Band Fade (_3.3 P2 review (b), the Look Kit's Fade): the sheet's own colour (--ts-top-fade), solid through the chrome row's foot
        // (--ts-top-fade-solid), then fading to nothing at the band's foot (--ts-top-fade-height); no stops under Glass
        public FadePainter TopFade { get; } = new(new FadeOptions(null));
        public const float TopFadeSolidAlpha = 0.99f;
        private static readonly CustomStyleProperty<float> TopFadeSolidToken = new("--ts-top-fade-solid");
        private bool _topBandGlass;
        public Button CloseButton { get; }
        // The X drawn on it (CardIcons.Shape.Close, in its glass disc, coloured by USS)
        public CardIcons.VectorGlyph CloseGlyph { get; }
        // The settings button at the bar's right (_3.3 P4.4b; the premium target's "sliders" mark): opens the visitor's settings --
        // language, light / dark
        public Button SettingsButton { get; }
        // The sliders drawn on it (CardIcons.Shape.Sliders, in its glass disc, coloured by USS)
        public CardIcons.VectorGlyph SettingsGlyph { get; }
        // The chrome marks' classes (PoiCard.uss): the 36 pt glass disc and the thin mark in it
        public const string MarkDiscClass = "poi-card__mark-disc";
        public const string MarkClass = "poi-card__mark";
        // The visitor's settings panel the settings button opens over the card (in the overlay layer)
        public CardVisitorSettingsView Settings { get; }
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
        // The vertical drag on the grabber, the header and the top strip: the height follows the finger, the release snaps or closes
        public SheetDrag Drag { get; }
        // The "Show me where it is" button at the sheet's bottom right (_3.3 P3d, LocateFabRule): placed with the chrome (PlaceLocate)
        public LocateFabView Locate { get; }
        // What opens in the full view (a block among its siblings, a lightbox, a bento section) and what reopens over a same-point rebind
        public CardFullViewOpener FullViewOpener { get; }

        // Raised when the visitor closes the card (the X, or a drag / swipe below peek)
        public event Action CloseRequested;

        // Raised when the visitor picks a language in the Aa settings: the language code to switch to
        public event Action<string> LanguageRequested;

        // Raised when the visitor picks a mode in the Aa settings: the mode to show (CardOptions.Modes)
        public event Action<string> ModeRequested;

        // The overlay key the Aa settings open under
        public const string SettingsKey = "visitor-settings";

        // Raised when the sheet comes to rest at another stop (Dismissed when it closes)
        public event Action<SheetStopRule.Stop> StopChanged;

        public SheetStopRule.Stop Stop { get; private set; } = SheetStopRule.Stop.Dismissed;
        public SheetStopRule.Stops Stops { get; private set; }
        // IBlockHost: the full stop's height (what a preview's cap is a share of)
        public float FullHeight => Stops.Full;
        public bool IsOpen => Stop != SheetStopRule.Stop.Dismissed;
        // The height the sheet is set to (it animates there); a drag writes it directly
        public float TargetHeight { get; private set; }

        private static readonly CustomStyleProperty<float> TopGapProperty = new("--ts-sheet-top-gap");
        private static readonly CustomStyleProperty<float> HeroMinProperty = new("--ts-hero-min-height");
        private static readonly CustomStyleProperty<float> BarClearProperty = new("--ts-navbar-clear");
        private static readonly CustomStyleProperty<float> BarHeroMarginProperty = new("--ts-navbar-hero-margin");
        private readonly BlockRegistry _registry;
        // The shown card's stack entries: what its navigation model is rebuilt from when a waiting block is revealed
        private IReadOnlyList<BlockStackBuilder.Entry> _entries;
        private float _topGap;
        // --ts-hero-min-height: the hero is never shorter than this, whatever the stops (HeroRule.Height)
        private float _heroMin;
        // --ts-navbar-clear: what a Navigation Bar floating at the bottom from the start takes off the hero's foot (its words rise above it);
        // --ts-navbar-hero-margin: how far the hero must reach past the bar for the bar to lie on the picture (NavBarRule.OverHero)
        private float _barClear;
        private float _barHeroMargin;
        private float _halfMaxRatio = CardContainerSettings.HalfMaxRatioMax;
        // The shown card's Full Height (CardOptions.FullHeights): whether the Full stop leaves the camera strip (_topGap) above it
        private string _fullHeight = CardOptions.FullHeightTop;

        // The shown card's media source and texts: what the full-screen view loads through and words its way back with
        private BlockBindContext _context;
        internal BlockBindContext Context => _context;

        // `navElements`: the navigation elements the slots can show (null = NavElementRegistry.Shared, the app's)
        public PoiCardSheetView(VisualElement parent, BlockRegistry registry, IEnumerable<StyleSheet> styleSheets, NavElementRegistry navElements = null)
        {
            Layer = new VisualElement { name = "poi-card-layer", pickingMode = PickingMode.Ignore };
            Layer.AddToClassList("poi-card-layer");
            foreach (var sheet in styleSheets)
                if (sheet != null) Layer.styleSheets.Add(sheet);

            // - the token classes sit on the sheet itself, so its own custom style carries the card's own --ts-* tokens (CardTokens.uss: the
            //   numbers C# reads here); the theme's colours and scales are inherited from the app root (ThemeController)
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
            // - a soft dark fade at the top while the bars float over the hero's picture, so their white marks read on any picture (no
            //   finger: the stack under it still scrolls)
            TopShade = new VisualElement { name = "poi-card-top-shade", pickingMode = PickingMode.Ignore };
            TopShade.AddToClassList("poi-card__top-shade");
            Root.Add(TopShade);
            // - after the hero the chrome row sits on ONE dense glass band (no block text through it); over the picture it is clear
            ChromeBand = new VisualElement { name = "poi-card-chrome-band", pickingMode = PickingMode.Ignore };
            ChromeBand.AddToClassList("poi-card__chrome-band");
            ChromeBand.AddManipulator(TopFade);
            // - the band's height is where a jump lands (RecomputeStops): a band laid out or resized moves the landing with it
            ChromeBand.RegisterCallback<GeometryChangedEvent>(_ =>
            {
                PaintTopFade();
                RecomputeStops();
            });
            ChromeBand.RegisterCallback<AttachToPanelEvent>(_ => { ThemeController.ModeChanged -= PaintTopFade; ThemeController.ModeChanged += PaintTopFade; });
            ChromeBand.RegisterCallback<DetachFromPanelEvent>(_ => ThemeController.ModeChanged -= PaintTopFade);
            Root.Add(ChromeBand);

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
            // - an open section's Start begins its Reveal once it clears a Navigation Bar floating at the stack's foot
            Stack.FootInset = () => Slots.BottomKey == NavBarView.ElementKey ? Slots.Bar.FootClearance : 0f;
            Chrome = new CardChromeView(this);
            Pager = new CardPagerView(Root, Stack.Scroll, Layout, Nav);
            Bento = new CardBentoView(Stack, Layout, Nav);

            // - the premium target's chrome (_3.3 S4 third round, v5): the X at the left, settings at the right, each a small glass DISC with a
            //   thin mark inside a whole touch target (the button); no words on either: the name (tooltip) comes from CardStrings
            CloseButton = new Button(RequestClose) { name = "poi-card-close" };
            CloseButton.AddToClassList("poi-card__close");
            CloseGlyph = AddMark(CloseButton, CardIcons.Shape.Close);
            Root.Add(CloseButton);

            // - "sliders", not a gear (v5: the sign of things to adjust -- language, light / dark); a second tap while its settings are open
            //   closes them
            SettingsButton = new Button(ToggleSettings) { name = "poi-card-settings" };
            SettingsButton.AddToClassList("poi-card__settings");
            SettingsGlyph = AddMark(SettingsButton, CardIcons.Shape.Sliders);
            Root.Add(SettingsButton);
            Settings = new CardVisitorSettingsView();
            Settings.LanguagePicked += code => LanguageRequested?.Invoke(code);
            Settings.ModePicked += mode => ModeRequested?.Invoke(mode);

            // - no glyph text: three bars drawn by the icon set; its name (tooltip) comes from CardStrings
            MenuButton = new Button(() => Overlay.OpenMenu()) { name = "poi-card-menu-button" };
            MenuButton.AddToClassList("poi-card__menu-button");
            var menuIcon = CardIcons.Create();
            CardIcons.SetKey(menuIcon, CardIcons.Menu);
            MenuButton.Add(menuIcon);
            Root.Add(MenuButton);
            Locate = new LocateFabView(ShowOnWall);
            Root.Add(Locate.Root);
            // - the chrome FLOATS over the stack (_3.3 S4a review): the shade, the grab row, the bar and the top strip are out of the flow
            //   (PoiCard.uss) and drawn over the scroll, the round buttons over them -- so the stack starts at the sheet's top, the hero's
            //   picture runs up behind the bars and nothing jumps when the strip comes
            foreach (var floating in new[] { TopShade, ChromeBand, Handle, Stack.HeaderSlot, Slots.Top, CloseButton, SettingsButton, MenuButton, Locate.Root })
                floating.BringToFront();

            Layer.Add(Root);
            Takeover = new TakeoverView(Layer);
            FullView = new BlockFullView(Takeover, Stack.Views);
            FullViewOpener = new CardFullViewOpener(this, registry);
            Overlay = new CardMenuOverlay(this);
            // - over the sheet, under the menu layer (an open drawer covers it) and the full view
            Floating.InsertUnder(Overlay);
            parent.Add(Layer);

            Drag = new SheetDrag(this);
            Root.RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyle);
            Layer.RegisterCallback<GeometryChangedEvent>(_ => RecomputeStops());
            Stack.HeaderSlot.RegisterCallback<GeometryChangedEvent>(_ => RecomputeStops());
            Slots.Top.RegisterCallback<GeometryChangedEvent>(_ => RecomputeStops());
            // - the Navigation Bar turns white on dark glass while it lies on the hero's picture: asked again on every scroll
            Stack.Scroll.verticalScroller.valueChanged += _ => UpdateBarOverHero();
            Hide();
        }

        // Show these blocks. A closed sheet opens at `openStop`; an open one keeps its stop (a new selection
        // while the card is open rebinds in place).
        public void Show(IReadOnlyList<BlockStackBuilder.Entry> entries, BlockBindContext context, SheetStopRule.Stop openStop, float halfMaxRatio)
        {
            _halfMaxRatio = halfMaxRatio;
            _fullHeight = context.Settings?.container?.full_height ?? CardOptions.FullHeightTop;
            _topBandGlass = context.Settings?.container?.top_band == CardOptions.TopBandGlass;
            ChromeBand.EnableInClassList(ChromeBandGlassClass, _topBandGlass);
            PaintTopFade();
            CloseButton.tooltip = context.Strings?.Get(CardStrings.Keys.Close) ?? "";
            Locate.Bind(context.Strings);
            ShowSettingsButton(context);
            context.Host ??= this;
            // - before the old card's full view closes: what of it stays over a rebind of the same point
            FullViewOpener.KeepAcrossRebind(context);
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
            FullViewOpener.ReopenAfterRebind();
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
            UpdateBarOverHero();
        }

        // Tell the Navigation Bar whether it lies on the hero's picture now (NavBarRule.OverHero, both in the panel's frame)
        private void UpdateBarOverHero()
        {
            var bar = Slots.Bar;
            if (Slots.TopKey != NavBarView.ElementKey && Slots.BottomKey != NavBarView.ElementKey) return;
            var header = Stack.HeaderView;
            float heroBottom = header != null && header.HasPicture && header.HeroPart.panel != null ? header.HeroPart.worldBound.yMax : float.NaN;
            var band = bar.Row.worldBound;
            bar.SetOverHero(!float.IsNaN(band.yMin) && NavBarRule.OverHero(NavBarRule.AtBottom(bar.Settings), band.yMin, band.yMax, heroBottom, _barHeroMargin));
        }

        // Place the Locate Button as LocateFabRule says, over what lies under it now -- a bar floating at the bottom (over the bottom area: its
        // highest point, the raised disc's top), a sticky footer shown there -- and tell the hero how much of its right its words leave to it.
        // CardChromeView calls it with every decision of the chrome (a stop, a scroll, a part resized)
        internal void PlaceLocate()
        {
            var nav = _context?.Settings?.navigation;
            bool areaIn = !BottomArea.ClassListContains(CardChromeView.BottomHiddenClass);
            float area = areaIn && !float.IsNaN(BottomArea.layout.height) ? BottomArea.layout.height : 0f;
            bool barBottom = Slots.BottomKey == NavBarView.ElementKey;
            float bottom = LocateFabRule.Bottom(barBottom ? area + Slots.Bar.HighestAboveFoot : float.NaN, area, Locate.Gap);
            Locate.Apply(LocateFabRule.Shown(nav, Stop, false), LocateFabRule.Shown(nav, Stop, Chrome?.Scroll.BottomHidden ?? false), bottom);
            // - the words' room follows the button being ON, never the stop or the scroll: measured at rest in the hero's own frame (the
            //   button's top in the sheet, less where the hero starts in it with the stack unscrolled -- drawn place plus the scroll, which
            //   moves the content by a translation: every offset of the scroll view's own parts is in it)
            bool on = NavBarRule.Enabled(nav) && NavBarRule.LocateButton(nav.bar);
            var header = Stack.HeaderView;
            if (header == null || header.HeroPart.panel == null) return;
            float heroTop = header.HeroPart.worldBound.yMin - Root.worldBound.yMin + Stack.Scroll.scrollOffset.y;
            // - the sheet's REST height at its stop, like the words' own place (SetHeroLayout): a sheet still rising or being dragged never
            //   puts a row beside the button for a moment and leaves it narrowed there
            float buttonTop = Stops.HeightOf(Stop) - bottom - Locate.Size - heroTop;
            header.SetWordsClear(LocateFabRule.WordsClear(on, Locate.Size, Locate.Inset, Locate.Gap), buttonTop);
        }

        // A chrome mark on `button`: the glass disc centred in the touch target, the vector mark centred in the disc (PoiCard.uss sizes
        // and colours both, over the hero and after it)
        private static CardIcons.VectorGlyph AddMark(Button button, CardIcons.Shape shape)
        {
            var disc = new VisualElement { name = button.name + "-disc", pickingMode = PickingMode.Ignore };
            disc.AddToClassList(MarkDiscClass);
            var glyph = CardIcons.CreateVector(shape);
            glyph.AddToClassList(MarkClass);
            disc.Add(glyph);
            button.Add(disc);
            return glyph;
        }

        // The header's menu button exists only while the card has a Menu element; its name is the menu's (CardStrings)
        private void ShowMenuButton()
        {
            bool shown = Slots.MenuView != null;
            MenuButton.style.display = shown ? DisplayStyle.Flex : DisplayStyle.None;
            MenuButton.tooltip = _context?.Strings?.Get(CardStrings.Keys.NavMenu) ?? "";
            Root.EnableInClassList("poi-card--has-menu", shown);
        }

        // The settings button is on every card: even with one language there is the mode to choose. No words on it: the mark says it,
        // its name ("Card settings") is for a screen reader and a long press
        private void ShowSettingsButton(BlockBindContext context)
        {
            SettingsButton.tooltip = context.Strings?.Get(CardStrings.Keys.VisitorSettings) ?? "";
            Root.AddToClassList("poi-card--has-settings");
        }

        // The settings button: open the visitor's settings over the card, or close them when they are open
        private void ToggleSettings()
        {
            if (_context == null) return;
            if (Overlay.OpenKey == SettingsKey) { Overlay.Close(); return; }
            var strings = _context.Strings;
            Settings.Bind(_context.Settings?.languages, _context.Language, ThemeController.CurrentMode, key => strings?.Get(key) ?? key);
            Overlay.OpenPopover(SettingsKey, Settings.Root);
            PlaceSettings();
        }

        // The panel hangs from the Aa button's right edge: under it while the bar is in the upper half of the screen, above it when the
        // sheet is low (at Peek the bar sits near the screen's foot) -- kept the button's own inset from the edges (its top)
        private void PlaceSettings()
        {
            var button = Layer.WorldToLocal(SettingsButton.worldBound);
            float gap = CloseButton.resolvedStyle.top, height = Layer.layout.height;
            var style = Settings.Root.style;
            style.right = Layer.layout.width - button.xMax;
            bool below = button.center.y < height / 2f;
            style.top = below ? button.yMax + gap : StyleKeyword.Auto;
            style.bottom = below ? StyleKeyword.Auto : height - button.yMin + gap;
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
            Drag?.Forget();
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
                case NavBackRule.Step.CloseFullView: Takeover.GoBack(); return true;
                case NavBackRule.Step.CloseOverlay: Overlay.Close(); return true;
                case NavBackRule.Step.CollapseRow: Stack.Collapse(AccordionRule.ClosedByBack(Stack.OpenRows)); return true;
                case NavBackRule.Step.LowerToHalf: SetStop(SheetStopRule.Stop.Half); return true;
                case NavBackRule.Step.LowerToPeek: SetStop(SheetStopRule.Stop.Peek); return true;
                case NavBackRule.Step.CloseCard: RequestClose(); return true;
                default: return false;
            }
        }

        // A tap on empty camera space (CardTapOutside): what is open over the card closes first -- the dial's list, the radial's arc, a bar's
        // list, a menu -- the way a tap outside closes a popover and the order Back keeps; with nothing open the card closes, as its X does
        public void TapOutside()
        {
            if (Overlay.IsOpen) Overlay.Close();
            else RequestClose();
        }

        // Ask the card's owner to close it (the X, Back at peek, a tap outside, a drag pulled away): it decides, then calls Hide
        internal void RequestClose() => CloseRequested?.Invoke();

        // A block asked for the full-screen view: the breadcrumb starts with this card's title and ends with `name`
        public void OpenTakeover(string name, int pageCount, int startPage, TakeoverPageDrawer drawPage) =>
            FullViewOpener.OpenTakeover(name, pageCount, startPage, drawPage);

        // A block asked for its full view (an open compact row's button, a bento tile): it opens among its siblings (CardFullViewOpener)
        public void OpenFullView(string blockKey) => FullViewOpener.OpenFullView(blockKey);

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
            // - the bar carries the title at Peek, the hero at the top of Half and Full
            Stack.UpdateBarTitle();
            Chrome?.Apply();
            Floating?.Refresh();
            UpdateBarOverHero();
            StopChanged?.Invoke(stop);
        }

        // The stops for the layer's current height and the header's current size; re-applies the rest height
        public void RecomputeStops()
        {
            float available = Layer.layout.height;
            if (float.IsNaN(available) || available <= 0f) return;
            // - the peek stop is the bar: the header slot's own row, which never changes size, so nothing the card scrolls or shows can move
            //   it. Measured on the SLOT's layout in the sheet's frame -- the slot is never re-parented, while the bound header view is
            //   re-attached by every rebind and reads a stale world rect until the next layout pass (Show measures right after the bind)
            var slot = Stack.HeaderSlot;
            float peek = float.IsNaN(slot.layout.yMax) ? 0f : slot.layout.yMax + slot.resolvedStyle.marginBottom;
            Stops = SheetStopRule.Compute(available, peek, _halfMaxRatio, SheetStopRule.FullTopGap(_fullHeight, _topGap));
            // - the hero fills Full's view under the bar, its words at its foot: it follows the stops, never feeds them
            // - the content starts under the floating bars; a jump lands a block under the bars, the strip AND the Top Band's whole fade (_3.3
            //   P3a review e: a section's Start lands whole, never dissolved in the band) -- each foot measured from the stack's own top (the
            //   landing is an offset in the stack's view, and the stack does not start at the sheet's y 0)
            float stackTop = float.IsNaN(Stack.Scroll.layout.yMin) ? 0f : Stack.Scroll.layout.yMin;
            float landing = Slots.TopView != null && !float.IsNaN(Slots.Top.layout.yMax) ? Slots.Top.layout.yMax - stackTop : peek;
            if (!float.IsNaN(ChromeBand.layout.yMax)) landing = Mathf.Max(landing, ChromeBand.layout.yMax - stackTop);
            Stack.SetInsets(peek, landing);
            float hero = HeroRule.Height(Stops, _heroMin);
            // - the words at the foot of what the visitor SEES at this stop (v6), above a Navigation Bar floating at the bottom from the start
            float barClear = NavBarRule.ClearsHeroFoot(_context?.Settings?.navigation) ? _barClear : 0f;
            Stack.HeaderView?.SetHeroLayout(hero, HeroRule.WordsBottom(hero, HeroRule.VisibleAt(Stops, Stop), barClear), Stack.TopInset);
            if (IsOpen && !Drag.IsDragging) ApplyHeight(Stops.HeightOf(Stop));
            // - the chrome's room is the stop's height: new stops can give it room, or take it
            Chrome?.Apply();
            UpdateBarOverHero();
        }

        // Set the sheet's height (a rest stop, or the finger's height during a drag -- SheetDrag)
        internal void ApplyHeight(float height)
        {
            TargetHeight = height;
            Root.style.height = height;
        }

        private void OnCustomStyle(CustomStyleResolvedEvent evt)
        {
            // - the card's tokens are declared on this root (.ts-card): the hero's minimum height is read here
            if (evt.customStyle.TryGetValue(HeroMinProperty, out float heroMin) && !Mathf.Approximately(heroMin, _heroMin))
            {
                _heroMin = heroMin;
                RecomputeStops();
            }
            // - a jump glides as long as the sheet takes to rise: both are --ts-duration (never read mid-drag, where the drag class zeroes it)
            if (!Drag.IsDragging) Nav.Duration = HeightTransitionSeconds();
            if (evt.customStyle.TryGetValue(BarHeroMarginProperty, out float margin)) _barHeroMargin = margin;
            if (evt.customStyle.TryGetValue(BarClearProperty, out float clear) && !Mathf.Approximately(clear, _barClear))
            {
                _barClear = clear;
                RecomputeStops();
            }
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
    
        // The Top Band's fade in the theme's colour for the mode shown, its solid line and its foot (none under Glass: the band is the glass)
        private void PaintTopFade()
        {
            float height = ChromeBand.layout.height;
            if (_topBandGlass || float.IsNaN(height) || !CardTokenColours.TryResolve(ChromeBand, "--ts-top-fade", out Color colour))
            {
                TopFade.Options = new FadeOptions(null);
                return;
            }
            float solid = 0f;
            for (var e = ChromeBand.parent; e != null; e = e.parent)
                if (e.ClassListContains("ts-card")) { e.customStyle.TryGetValue(TopFadeSolidToken, out solid); break; }
            colour.a = TopFadeSolidAlpha;
            var clear = colour;
            clear.a = 0f;
            TopFade.Options = new FadeOptions(new List<FadeStop> { new(0f, colour), new(Mathf.Min(solid, height), colour), new(height, clear) });
        }
}
}
