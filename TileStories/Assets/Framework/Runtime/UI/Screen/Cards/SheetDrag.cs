using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories
{
    // The card sheet's vertical drag (_3.1 section 4): the grabber and the header take the finger at touch-down, the top navigation strip
    // only once a move is clearly vertical; the sheet's height follows the finger, and on release it snaps where the drag and its speed
    // point (SheetStopRule.Snap) -- or the card closes, as its X does, when the sheet was pulled away
    public sealed class SheetDrag
    {
        public bool IsDragging { get; private set; }

        // The drag's clock (velocity sampling): Time.unscaledTime in the app. A test sets its own, so a simulated drag's
        // speed is what the test says -- no real frame rate decides whether it reads as a flick (mirrors PoiCardHost.Clock)
        internal Func<float> Clock { get; set; } = () => Time.unscaledTime;

        private readonly PoiCardSheetView _sheet;
        private float _dragStartPointerY;
        private float _dragStartHeight;
        private float _lastSampleHeight;
        private float _lastSampleTime;
        private float _velocity;
        // A vertical drag on the top navigation strip moves the sheet like one on the header (its sideways drags are the strip's own)
        private readonly SwipeGrab _stripDrag;
        private float _stripDragStartY;

        public SheetDrag(PoiCardSheetView sheet)
        {
            _sheet = sheet;
            foreach (var dragArea in new[] { sheet.Handle, sheet.Stack.HeaderSlot })
            {
                dragArea.RegisterCallback<PointerDownEvent>(OnPointerDown);
                dragArea.RegisterCallback<PointerMoveEvent>(OnPointerMove);
                dragArea.RegisterCallback<PointerUpEvent>(OnPointerUp);
            }
            // - never taken at touch-down: a tap still reaches a tab, a sideways drag the strip; only a clearly vertical one is the sheet's
            _stripDrag = new SwipeGrab(sheet.Slots.Top, SwipeGrabRule.ClaimsVertical);
            _stripDrag.Claimed += start => { _stripDragStartY = start.y; BeginDrag(start.y); };
            _stripDrag.Moved += travel => DragTo(_stripDragStartY + travel.y);
            _stripDrag.Released += _ => EndDrag();
            _stripDrag.Cancelled += EndDrag;
        }

        // The card closed under the finger (Hide): the drag is over, with no snap -- the sheet is already dismissed
        internal void Forget() => IsDragging = false;

        // The grabber and the header take the finger at touch-down (a drag there can only move the sheet)
        private void OnPointerDown(PointerDownEvent evt)
        {
            if (!_sheet.IsOpen || evt.button != 0) return;
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
            if (!_sheet.IsOpen) return;
            IsDragging = true;
            _sheet.Root.AddToClassList("poi-card--dragging");
            _dragStartPointerY = pointerY;
            _dragStartHeight = _sheet.TargetHeight;
            _lastSampleHeight = _sheet.TargetHeight;
            _lastSampleTime = Clock();
            _velocity = 0f;
        }

        // Hold the sheet at the finger's height, and measure how fast it is moving
        private void DragTo(float pointerY)
        {
            if (!IsDragging) return;
            // - panel y grows downward: dragging up (smaller y) makes the sheet taller
            float height = Mathf.Clamp(_dragStartHeight + (_dragStartPointerY - pointerY), 0f, _sheet.Stops.Full);
            float now = Clock();
            // - several events in one frame have no time between them: keep the last measured speed
            if (now > _lastSampleTime)
            {
                _velocity = (height - _lastSampleHeight) / (now - _lastSampleTime);
                _lastSampleTime = now;
                _lastSampleHeight = height;
            }
            _sheet.ApplyHeight(height);
        }

        // The finger let go: snap to the stop the drag and its speed point at, or close
        private void EndDrag()
        {
            if (!IsDragging) return;
            IsDragging = false;
            _sheet.Root.RemoveFromClassList("poi-card--dragging");
            var stop = SheetStopRule.Snap(_sheet.Stops, _sheet.TargetHeight, _velocity, _sheet.Layer.layout.height);
            if (stop == SheetStopRule.Stop.Dismissed) _sheet.RequestClose();
            else _sheet.SetStop(stop);
        }
    }
}
