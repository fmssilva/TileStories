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
        // The navigation slots (_3.2 steps 5a, 7a): the top strip between the header and the stack, the bottom slot under the footer, and
        // the element each one shows
        public NavSlots Slots { get; }
        // What of the chrome -- the top strip, the bottom area -- shows now (_3.2 step 5b, CardChromeRule)
        public CardChromeView Chrome { get; }
        // The full-screen view over the card (a gallery's lightbox), above the sheet in the same layer
        public TakeoverView Takeover { get; }
        // What is open over the card -- the wall's Menu on a scrim, a bar's More or kinds list -- and when it closes (_3.2 step 8): a layer
        // between the sheet and the full view
        public CardMenuOverlay Overlay { get; }
        // The header's menu button (_3.2 step 8): beside the language chip, drawn only while the card has a Menu element; opens it
        public Button MenuButton { get; }

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
            Stack.Revealed += BindNav;
            // - the footer moves into the bottom area, which takes its place under the stack: the footer first (the CTA nearest the
            //   content), the bottom slot after it (NavSlots)
            BottomArea = new VisualElement { name = "poi-card-bottom" };
            BottomArea.AddToClassList("poi-card__bottom");
            Root.Insert(Root.IndexOf(Stack.Footer), BottomArea);
            BottomArea.Add(Stack.Footer);
            Slots = new NavSlots(Root, Stack.Scroll, BottomArea, navElements ?? NavElementRegistry.Shared);
            Chrome = new CardChromeView(this);

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
            Overlay = new CardMenuOverlay(this);
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
            // - another card replaces the full-screen view of the old one, and whatever was open over it
            Takeover.Close();
            Overlay.Close();
            context.Overlay = Overlay;
            _context = context;
            _entries = entries;
            // - before the stack binds: a block that offers a jump reads where the card can scroll (a block waiting for Show After Reading
            //   is not there yet -- the same remembered reveal the stack hides it on)
            var state = context.State;
            string poiId = context.Poi?.id;
            context.JumpTargets = BlockRefRule.JumpTargets(entries, key => state?.Seen(poiId, key) ?? false);
            Stack.Bind(entries, context);
            BindNav();
            Root.style.display = DisplayStyle.Flex;
            SetStop(IsOpen ? Stop : openStop);
        }

        // Build the shown card's navigation model -- with the stack's own reveal predicate, so a block the stack hides is never a target --
        // and hand it to the controller (on Show: a POI switch, a live edit, a language switch; and on a reveal)
        private void BindNav()
        {
            if (_context == null || _entries == null) return;
            // - a reveal rebinds every element: a menu or list open over the old ones closes with them
            Overlay.Close();
            Nav.Bind(CardNavModel.Build(_entries, _registry, _context.Settings, Stack.IsRevealed), _context);
            Slots.Bind(Nav.Model, Nav, _context);
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
            Slots?.Unbind();
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
            if (IsOpen) SetStop(SheetStopRule.Stop.Peek);
        }

        // A block asked for the header at the peek: scroll to the top first (a peek over a scrolled stack shows a half-cut line of whatever
        // block was in view), then lower the card
        public void ShowHeaderAtPeek()
        {
            if (!IsOpen) return;
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
            switch (NavBackRule.Next(_context?.Settings?.navigation, Takeover.IsOpen, Overlay.IsOpen, Stop))
            {
                case NavBackRule.Step.CloseFullView: Takeover.Close(); return true;
                case NavBackRule.Step.CloseOverlay: Overlay.Close(); return true;
                case NavBackRule.Step.LowerToHalf: SetStop(SheetStopRule.Stop.Half); return true;
                case NavBackRule.Step.LowerToPeek: SetStop(SheetStopRule.Stop.Peek); return true;
                case NavBackRule.Step.CloseCard: CloseRequested?.Invoke(); return true;
                default: return false;
            }
        }

        // A block asked for the full-screen view: the breadcrumb starts with this card's title
        public void OpenTakeover(string name, int pageCount, int startPage, TakeoverPageDrawer drawPage)
        {
            if (!IsOpen || _context == null) return;
            string title = Stack.BoundViews.Count > 0 && Stack.BoundViews[0] is HeaderBlockView header ? header.TitleText : "";
            string crumb = title.Length > 0 && !string.IsNullOrEmpty(name) ? title + " > " + name : title + name;
            // - never stacked: the full view replaces what was open over the card
            Overlay.Close();
            Takeover.Open(crumb, pageCount, startPage, _context.Media, drawPage, _context.Strings);
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
