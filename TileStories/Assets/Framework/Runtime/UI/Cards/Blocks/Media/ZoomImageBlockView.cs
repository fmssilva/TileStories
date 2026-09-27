using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories
{
    // The zoom_image block (_3.1 Tier 2), pinch: one picture in a frame the visitor looks into. Two fingers pinch it
    // (1x..4x, the point between the fingers stays put), Ctrl + wheel does the same on a desktop (a trackpad pinch
    // arrives as exactly that), one finger moves an enlarged picture. At 1x a one-finger drag is left to the stack, which
    // scrolls as usual; the AR camera never sees these fingers (ARZoomGestureInput leaves screen-UI touches alone).
    // ZoomPanRule decides every position; this view only follows the pointers and places the picture.
    public sealed class ZoomImageBlockView : IBlockView
    {
        public VisualElement Root { get; }
        public VisualElement Frame { get; }
        public CardImage Image { get; }
        public Label Caption { get; }
        public Label Hint { get; }

        public ZoomPan State { get; private set; } = new(1f, Vector2.zero);
        public float Scale => State.Scale;

        // Wheel notches per doubling of the scale (a Ctrl + wheel step)
        private const float WheelNotchesPerDoubling = 12f;

        private readonly Dictionary<int, Vector2> _pointers = new();
        private float _lastPinchDistance;
        private Vector2 _lastPanPoint;

        public ZoomImageBlockView()
        {
            Root = new VisualElement();
            Root.AddToClassList("card-block");
            Root.AddToClassList("card-zoom");
            Frame = new VisualElement { name = "card-zoom-frame" };
            Frame.AddToClassList("card-zoom__frame");
            Image = new CardImage("card-zoom__image");
            Image.Root.pickingMode = PickingMode.Ignore;
            Frame.Add(Image.Root);
            Caption = new Label();
            Caption.AddToClassList("card-zoom__caption");
            Hint = new Label();
            Hint.AddToClassList("card-zoom__hint");
            Root.Add(Frame);
            Root.Add(Caption);
            Root.Add(Hint);

            Frame.RegisterCallback<PointerDownEvent>(OnPointerDown);
            Frame.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            Frame.RegisterCallback<PointerUpEvent>(OnPointerUp);
            Frame.RegisterCallback<PointerCancelEvent>(evt => Lift(evt.pointerId));
            Frame.RegisterCallback<WheelEvent>(OnWheel);
            Frame.RegisterCallback<GeometryChangedEvent>(_ => Place(ZoomPanRule.Clamp(State, FrameSize, Image.Aspect)));
        }

        public void Bind(BlockInstanceData instance, BlockBindContext context)
        {
            var read = new BlockFieldReader(instance, context.Language, context.FallbackLanguage);
            Image.Show(context.Media, read.ValidAsset(BuiltInBlocks.ZoomImageImageField, MediaKind.Image), context.Strings);
            Caption.text = read.Text(BuiltInBlocks.ZoomImageCaptionField);
            Caption.style.display = Caption.text.Length > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            Hint.text = context.Strings?.Get(CardStrings.Keys.ZoomHint) ?? "";
            Place(ZoomPanRule.Whole(FrameSize, Image.Aspect));
        }

        public void Unbind()
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

        private void OnPointerDown(PointerDownEvent evt)
        {
            _pointers[evt.pointerId] = evt.localPosition;
            if (_pointers.Count == 2)
            {
                _lastPinchDistance = PinchDistance();
                foreach (int id in _pointers.Keys) if (!Frame.HasPointerCapture(id)) Frame.CapturePointer(id);
                evt.StopPropagation();
            }
            else if (_pointers.Count == 1 && State.Scale > 1.001f)
            {
                // - an enlarged picture: one finger moves it (and the stack must not scroll meanwhile)
                _lastPanPoint = evt.localPosition;
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
            else if (Frame.HasPointerCapture(evt.pointerId))
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
            Lift(evt.pointerId);
            if (mine) evt.StopPropagation();
        }

        private void Lift(int pointerId)
        {
            _pointers.Remove(pointerId);
            if (Frame.HasPointerCapture(pointerId)) Frame.ReleasePointer(pointerId);
            _lastPinchDistance = _pointers.Count >= 2 ? PinchDistance() : 0f;
            // - the finger left on the picture after a pinch goes on moving it from where it is now
            foreach (var p in _pointers.Values) _lastPanPoint = p;
        }

        // Ctrl + wheel zooms at the pointer; a plain wheel belongs to the stack (it scrolls)
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
