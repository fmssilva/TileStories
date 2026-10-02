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
        public BlockStackView Stack { get; }
        // The full-screen view over the card (a gallery's lightbox), above the sheet in the same layer
        public TakeoverView Takeover { get; }

        // Raised when the visitor closes the card (the X, or a drag / swipe below peek)
        public event Action CloseRequested;

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
        private float _topGap;
        private float _halfMaxRatio = CardContainerSettings.HalfMaxRatioMax;

        // The drag's clock (velocity sampling): Time.unscaledTime in the app. A test sets its own, so a simulated drag's
        // speed is what the test says -- no real frame rate decides whether it reads as a flick (mirrors PoiCardHost.Clock)
        internal System.Func<float> Clock { get; set; } = () => Time.unscaledTime;

        private float _dragStartPointerY;
        private float _dragStartHeight;
        private float _lastSampleHeight;
        private float _lastSampleTime;
        private float _velocity;
        // The shown card's media source and texts: what the full-screen view loads through and words its way back with
        private BlockBindContext _context;

        public PoiCardSheetView(VisualElement parent, BlockRegistry registry, IEnumerable<StyleSheet> styleSheets)
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

            Stack = new BlockStackView(Root, Root, registry);

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

            Layer.Add(Root);
            Takeover = new TakeoverView(Layer);
            parent.Add(Layer);

            foreach (var dragArea in new[] { Handle, Stack.HeaderSlot })
            {
                dragArea.RegisterCallback<PointerDownEvent>(OnPointerDown);
                dragArea.RegisterCallback<PointerMoveEvent>(OnPointerMove);
                dragArea.RegisterCallback<PointerUpEvent>(OnPointerUp);
            }
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
            context.Host ??= this;
            // - another card replaces the full-screen view of the old one
            Takeover.Close();
            _context = context;
            Stack.Bind(entries, context);
            Root.style.display = DisplayStyle.Flex;
            SetStop(IsOpen ? Stop : openStop);
        }

        // Close: unbind every block (views go back to their pool) and collapse
        public void Hide()
        {
            Takeover?.Close();
            _context = null;
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

        // A block asked for the full-screen view: the breadcrumb starts with this card's title
        public void OpenTakeover(string name, int pageCount, int startPage, TakeoverPageDrawer drawPage)
        {
            if (!IsOpen || _context == null) return;
            string title = Stack.BoundViews.Count > 0 && Stack.BoundViews[0] is HeaderBlockView header ? header.TitleText : "";
            string crumb = title.Length > 0 && !string.IsNullOrEmpty(name) ? title + " > " + name : title + name;
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
            if (evt.customStyle.TryGetValue(TopGapProperty, out float gap) && !Mathf.Approximately(gap, _topGap))
            {
                _topGap = gap;
                RecomputeStops();
            }
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (!IsOpen || evt.button != 0) return;
            var area = (VisualElement)evt.currentTarget;
            area.CapturePointer(evt.pointerId);
            IsDragging = true;
            Root.AddToClassList("poi-card--dragging");
            _dragStartPointerY = evt.position.y;
            _dragStartHeight = TargetHeight;
            _lastSampleHeight = TargetHeight;
            _lastSampleTime = Clock();
            _velocity = 0f;
            evt.StopPropagation();
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!IsDragging || !((VisualElement)evt.currentTarget).HasPointerCapture(evt.pointerId)) return;
            // - panel y grows downward: dragging up (smaller y) makes the sheet taller
            float height = Mathf.Clamp(_dragStartHeight + (_dragStartPointerY - evt.position.y), 0f, Stops.Full);
            float now = Clock();
            // - several events in one frame have no time between them: keep the last measured speed
            if (now > _lastSampleTime)
            {
                _velocity = (height - _lastSampleHeight) / (now - _lastSampleTime);
                _lastSampleTime = now;
                _lastSampleHeight = height;
            }
            ApplyHeight(height);
            evt.StopPropagation();
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            var area = (VisualElement)evt.currentTarget;
            if (!IsDragging || !area.HasPointerCapture(evt.pointerId)) return;
            area.ReleasePointer(evt.pointerId);
            IsDragging = false;
            Root.RemoveFromClassList("poi-card--dragging");
            var stop = SheetStopRule.Snap(Stops, TargetHeight, _velocity, Layer.layout.height);
            if (stop == SheetStopRule.Stop.Dismissed) CloseRequested?.Invoke();
            else SetStop(stop);
            evt.StopPropagation();
        }
    }
}
