using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories
{
    // A picture in a frame the visitor can look into (_3.1 Tier 2): the zoom_image block's frame and the lightbox's page
    // (_3.1 [7B]) are this one surface. Two fingers pinch it (1x..4x, the point between the fingers stays put), Ctrl + wheel
    // does the same on a desktop (a trackpad pinch arrives as exactly that), one finger moves an enlarged picture.
    // At 1x a one-finger drag is not the surface's -- in a scrolling card the stack scrolls -- unless the surface pages
    // (Swipes on): then a sideways drag turns the page (SwipePageRule). The AR camera never sees these fingers
    // (ARZoomGestureInput leaves screen-UI touches alone). ZoomPanRule decides every position; this only follows the
    // pointers and places the picture. Plain C#: the owner adds Frame where it wants it.
    public sealed class ZoomPanSurface
    {
        public VisualElement Frame { get; }
        public CardImage Image { get; }
        public ZoomPan State { get; private set; } = new(1f, Vector2.zero);
        public float Scale => State.Scale;

        // Whether a one-finger sideways drag at 1x turns the page (Swiped) instead of being left to the parent
        public bool Swipes { get; set; }

        // A page swipe ended: +1 = the next page, -1 = the previous one (SwipePageRule)
        public event System.Action<int> Swiped;

        // Wheel notches per doubling of the scale (a Ctrl + wheel step)
        private const float WheelNotchesPerDoubling = 12f;

        private readonly Dictionary<int, Vector2> _pointers = new();
        private float _lastPinchDistance;
        private Vector2 _lastPanPoint;
        // The one finger a page swipe may come from (-1: none): cancelled by a second finger or by the picture enlarging
        private int _swipePointer = -1;
        private Vector2 _swipeStart;

        // `frameClass` / `imageClass`: the owner's look for the frame and the picture (its family's USS)
        public ZoomPanSurface(string frameClass, string imageClass)
        {
            Frame = new VisualElement();
            Frame.AddToClassList("card-zoompan");
            Frame.AddToClassList(frameClass);
            Image = new CardImage(imageClass);
            Image.Root.AddToClassList("card-zoompan__image");
            Image.Root.pickingMode = PickingMode.Ignore;
            Frame.Add(Image.Root);

            Frame.RegisterCallback<PointerDownEvent>(OnPointerDown);
            Frame.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            Frame.RegisterCallback<PointerUpEvent>(OnPointerUp);
            Frame.RegisterCallback<PointerCancelEvent>(evt => Lift(evt.pointerId));
            Frame.RegisterCallback<WheelEvent>(OnWheel);
            Frame.RegisterCallback<GeometryChangedEvent>(_ => Place(ZoomPanRule.Clamp(State, FrameSize, Image.Aspect)));
        }

        // Show the picture at `path` whole (1x, centred)
        public void Show(IMediaSource media, string path, CardStrings strings)
        {
            Image.Show(media, path, strings);
            Place(ZoomPanRule.Whole(FrameSize, Image.Aspect));
        }

        // Let go of every finger and the picture; back to 1x
        public void Clear()
        {
            foreach (int id in new List<int>(_pointers.Keys)) Lift(id);
            Image.Clear(null);
            State = new ZoomPan(1f, Vector2.zero);
        }

        private Vector2 FrameSize => new(Frame.layout.width, Frame.layout.height);

        // Zoom by `factor` about `point` (frame coordinates), as a pinch or a wheel does
        public void ZoomAbout(float factor, Vector2 point) => Place(ZoomPanRule.ZoomAbout(State, factor, point, FrameSize, Image.Aspect));

        // Put the picture where `state` says: its size and top-left corner inside the frame
        private void Place(ZoomPan state)
        {
            State = state;
            var size = ZoomPanRule.SizeOf(state, FrameSize, Image.Aspect);
            if (float.IsNaN(size.x) || size.x <= 0f) return;
            Image.Root.style.width = size.x;
            Image.Root.style.height = size.y;
            Image.Root.style.left = state.Offset.x;
            Image.Root.style.top = state.Offset.y;
        }

        private bool Enlarged => State.Scale > 1.001f;

        private void OnPointerDown(PointerDownEvent evt)
        {
            _pointers[evt.pointerId] = evt.localPosition;
            if (_pointers.Count == 2)
            {
                _swipePointer = -1;
                _lastPinchDistance = PinchDistance();
                foreach (int id in _pointers.Keys) if (!Frame.HasPointerCapture(id)) Frame.CapturePointer(id);
                evt.StopPropagation();
            }
            else if (_pointers.Count == 1 && Enlarged)
            {
                // - an enlarged picture: one finger moves it (and the stack must not scroll meanwhile)
                _lastPanPoint = evt.localPosition;
                Frame.CapturePointer(evt.pointerId);
                evt.StopPropagation();
            }
            else if (_pointers.Count == 1 && Swipes)
            {
                // - a paged surface at 1x: this finger may turn the page on release
                _swipePointer = evt.pointerId;
                _swipeStart = evt.localPosition;
                Frame.CapturePointer(evt.pointerId);
                evt.StopPropagation();
            }
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!_pointers.ContainsKey(evt.pointerId)) return;
            _pointers[evt.pointerId] = evt.localPosition;
            if (_pointers.Count >= 2)
            {
                float distance = PinchDistance();
                if (_lastPinchDistance > 0f && distance > 0f) ZoomAbout(distance / _lastPinchDistance, PinchMidpoint());
                _lastPinchDistance = distance;
                evt.StopPropagation();
            }
            else if (Frame.HasPointerCapture(evt.pointerId) && evt.pointerId != _swipePointer)
            {
                Vector2 point = evt.localPosition;
                Place(ZoomPanRule.Pan(State, point - _lastPanPoint, FrameSize, Image.Aspect));
                _lastPanPoint = point;
                evt.StopPropagation();
            }
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            bool mine = Frame.HasPointerCapture(evt.pointerId);
            // - judged before Lift: a second finger or an enlarged picture already cancelled it
            int turn = evt.pointerId == _swipePointer && !Enlarged
                ? SwipePageRule.Direction((Vector2)evt.localPosition - _swipeStart, Frame.layout.width) : 0;
            Lift(evt.pointerId);
            if (mine) evt.StopPropagation();
            // - last: turning the page may clear the frame this handler runs on
            if (turn != 0) Swiped?.Invoke(turn);
        }

        private void Lift(int pointerId)
        {
            _pointers.Remove(pointerId);
            if (pointerId == _swipePointer) _swipePointer = -1;
            if (Frame.HasPointerCapture(pointerId)) Frame.ReleasePointer(pointerId);
            _lastPinchDistance = _pointers.Count >= 2 ? PinchDistance() : 0f;
            // - the finger left on the picture after a pinch goes on moving it from where it is now
            foreach (var p in _pointers.Values) _lastPanPoint = p;
        }

        // Ctrl + wheel zooms at the pointer; a plain wheel belongs to the parent (a card scrolls)
        private void OnWheel(WheelEvent evt)
        {
            if (!evt.ctrlKey && !evt.commandKey) return;
            ZoomAbout(Mathf.Pow(2f, -evt.delta.y / WheelNotchesPerDoubling), evt.localMousePosition);
            evt.StopPropagation();
        }

        private float PinchDistance()
        {
            Vector2? a = null;
            foreach (var p in _pointers.Values)
            {
                if (a == null) { a = p; continue; }
                return Vector2.Distance(a.Value, p);
            }
            return 0f;
        }

        private Vector2 PinchMidpoint()
        {
            Vector2 sum = Vector2.zero;
            int n = 0;
            foreach (var p in _pointers.Values)
            {
                if (n == 2) break;
                sum += p;
                n++;
            }
            return n > 0 ? sum / n : Vector2.zero;
        }
    }
}
