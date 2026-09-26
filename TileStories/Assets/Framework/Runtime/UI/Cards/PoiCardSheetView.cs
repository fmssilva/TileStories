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
    public sealed class PoiCardSheetView
    {
        // Full-area layer the sheet sits at the bottom of (its height is what the stops divide)
        public VisualElement Layer { get; }
        public VisualElement Root { get; }
        public VisualElement Handle { get; }
        public Button CloseButton { get; }
        public BlockStackView Stack { get; }

        // Raised when the visitor closes the card (the X, or a drag / swipe below peek)
        public event Action CloseRequested;

        public SheetStopRule.Stop Stop { get; private set; } = SheetStopRule.Stop.Dismissed;
        public SheetStopRule.Stops Stops { get; private set; }
        public bool IsOpen => Stop != SheetStopRule.Stop.Dismissed;
        // The height the sheet is set to (it animates there); a drag writes it directly
        public float TargetHeight { get; private set; }
        public bool IsDragging { get; private set; }

        private static readonly CustomStyleProperty<float> TopGapProperty = new("--ts-sheet-top-gap");
        private float _topGap;
        private float _halfMaxRatio = CardContainerSettings.HalfMaxRatioMax;

        private float _dragStartPointerY;
        private float _dragStartHeight;
        private float _lastSampleHeight;
        private float _lastSampleTime;
        private float _velocity;

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

            CloseButton = new Button(() => CloseRequested?.Invoke()) { name = "poi-card-close", text = "X", tooltip = "Close" };
            CloseButton.AddToClassList("poi-card__close");
            Root.Add(CloseButton);

            Layer.Add(Root);
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
            Hide();
        }

        // Show these blocks. A closed sheet opens at `openStop`; an open one keeps its stop (a new selection
        // while the card is open rebinds in place).
        public void Show(IReadOnlyList<BlockStackBuilder.Entry> entries, BlockBindContext context, SheetStopRule.Stop openStop, float halfMaxRatio)
        {
            _halfMaxRatio = halfMaxRatio;
            Stack.Bind(entries, context);
            Root.style.display = DisplayStyle.Flex;
            SetStop(IsOpen ? Stop : openStop);
        }

        // Close: unbind every block (views go back to their pool) and collapse
        public void Hide()
        {
            Stack.UnbindAll();
            Stop = SheetStopRule.Stop.Dismissed;
            IsDragging = false;
            ApplyHeight(0f);
            Root.style.display = DisplayStyle.None;
        }

        // Rest at `stop` (Dismissed hides)
        public void SetStop(SheetStopRule.Stop stop)
        {
            if (stop == SheetStopRule.Stop.Dismissed) { Hide(); return; }
            Stop = stop;
            RecomputeStops();
        }

        // The stops for the layer's current height and the header's current size; re-applies the rest height
        public void RecomputeStops()
        {
            float available = Layer.layout.height;
            if (float.IsNaN(available) || available <= 0f) return;
            var peekPart = Stack.PeekPart;
            float peek = float.IsNaN(peekPart.worldBound.yMax) || float.IsNaN(Root.worldBound.yMin)
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
            _lastSampleTime = Time.unscaledTime;
            _velocity = 0f;
            evt.StopPropagation();
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!IsDragging || !((VisualElement)evt.currentTarget).HasPointerCapture(evt.pointerId)) return;
            // - panel y grows downward: dragging up (smaller y) makes the sheet taller
            float height = Mathf.Clamp(_dragStartHeight + (_dragStartPointerY - evt.position.y), 0f, Stops.Full);
            float now = Time.unscaledTime;
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
