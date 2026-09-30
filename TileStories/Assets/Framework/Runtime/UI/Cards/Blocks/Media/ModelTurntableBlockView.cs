using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories
{
    // The model_3d block, turntable look (_3.1 step 10A.2b.3): the card's ONE preview owner (BlockBindContext.Preview)
    // renders the model into a RenderTexture this view draws; a one-finger drag rotates it (TurntableRule), two fingers
    // pinch to zoom within limits, and -- when Auto Spin is on -- it turns on its own after a pause with no touch, the
    // same pause a touch resets. A render is asked for only after a real gesture or an auto-spin tick that actually moved
    // the model, never every frame on its own (IPreviewHandle's own contract). The Fallback Picture (or, absent one,
    // CardImage's own "unavailable" words) shows while the model loads and if it never loads at all; a small "Loading..."
    // note is added on top of that while it is still on its way. Only classes here; Media.uss holds every size and colour.
    public sealed class ModelTurntableBlockView : IBlockView
    {
        public VisualElement Root { get; }
        public VisualElement Frame { get; }
        public VisualElement Surface { get; }
        public CardImage Fallback { get; }
        public Label Loading { get; }
        public Label Hint { get; }

        public TurntableState State { get; private set; } = TurntableState.Start;
        // What the surface is drawing now (the model's real frames, not the fallback picture)
        public bool ShowsModel { get; private set; }
        // The slot's own RenderTexture (null until ready): tests read its real pixels directly, the same texture Surface draws
        public RenderTexture Texture => _slot?.Texture;

        // How many screen pixels of drag turn the model one degree: a plain gesture-sensitivity constant, not a "look"
        private const float DegreesPerPixel = 0.3f;
        // How often an auto-spinning model asks for a new frame: smooth enough to read as motion, cheap enough to leave running
        private const long AutoSpinTickMs = 33;

        private ICardPreview _preview;
        private ICardPreviewSlot _slot;
        private string _key = "";
        private CardStrings _strings;
        private bool _autoSpin;
        private readonly Dictionary<int, Vector2> _pointers = new();
        private float _lastPinchDistance;
        private Vector2 _lastDragPoint;
        private IVisualElementScheduledItem _idleTick;

        public ModelTurntableBlockView()
        {
            Root = new VisualElement { name = "card-model3d-block" };
            Root.AddToClassList("card-block");
            Root.AddToClassList("card-model3d");

            Frame = new VisualElement { name = "card-model3d-frame" };
            Frame.AddToClassList("card-model3d__frame");
            Surface = new VisualElement { name = "card-model3d-surface", pickingMode = PickingMode.Ignore };
            Surface.AddToClassList("card-model3d__surface");
            Fallback = new CardImage("card-model3d__fallback");
            Fallback.Root.pickingMode = PickingMode.Ignore;
            Loading = new Label { pickingMode = PickingMode.Ignore };
            Loading.AddToClassList("card-model3d__loading");
            Frame.Add(Surface);
            Frame.Add(Fallback.Root);
            Frame.Add(Loading);

            Hint = new Label();
            Hint.AddToClassList("card-model3d__hint");

            Root.Add(Frame);
            Root.Add(Hint);

            Frame.RegisterCallback<PointerDownEvent>(OnPointerDown);
            Frame.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            Frame.RegisterCallback<PointerUpEvent>(OnPointerUp);
            Frame.RegisterCallback<PointerCancelEvent>(evt => Lift(evt.pointerId));
        }

        public void Bind(BlockInstanceData instance, BlockBindContext context)
        {
            Unbind();
            _key = instance?.key ?? "";
            _preview = context.Preview;
            _strings = context.Strings;
            var read = new BlockFieldReader(instance, context.Language, context.FallbackLanguage);
            string modelPath = read.ValidAsset(BuiltInBlocks.Model3DModelField, MediaKind.Model);
            string fallbackPath = read.ValidAsset(BuiltInBlocks.Model3DFallbackField, MediaKind.Image);
            _autoSpin = read.Flag(BuiltInBlocks.Model3DAutoSpinField);
            State = TurntableState.Start;

            if (fallbackPath.Length > 0) Fallback.Show(context.Media, fallbackPath, context.Strings);
            else Fallback.Clear(context.Strings);
            Hint.text = context.Strings?.Get(CardStrings.Keys.Model3DHint) ?? "";

            _slot = _preview?.Request(_key, MediaKind.Model, modelPath);
            if (_preview != null) _preview.Changed += Refresh;
            SetAutoSpinTicking(_autoSpin);
            Refresh();
        }

        public void Unbind()
        {
            SetAutoSpinTicking(false);
            if (_preview != null)
            {
                _preview.Changed -= Refresh;
                _preview.Release(_key);
            }
            _preview = null;
            _slot = null;
            _strings = null;
            _pointers.Clear();
            _lastPinchDistance = 0f;
            Surface.style.backgroundImage = StyleKeyword.Null;
            Fallback.Clear(null);
        }

        // Draw the surface, the fallback and the loading note as the slot stands now
        private void Refresh()
        {
            bool ready = _slot != null && !_slot.IsLoading && !_slot.Failed && _slot.Texture != null;
            ShowsModel = ready;
            Surface.style.backgroundImage = ready ? new StyleBackground(Background.FromRenderTexture(_slot.Texture)) : StyleKeyword.Null;
            Surface.style.display = ready ? DisplayStyle.Flex : DisplayStyle.None;
            Fallback.Root.style.display = ready ? DisplayStyle.None : DisplayStyle.Flex;
            bool loading = _slot != null && _slot.IsLoading;
            Loading.style.display = loading ? DisplayStyle.Flex : DisplayStyle.None;
            Loading.text = loading ? _strings?.Get(CardStrings.Keys.Model3DLoading) ?? "" : "";
            if (ready) RenderNow();
        }

        // Render the slot's texture at the current State and ask the panel to repaint: a RenderTexture's pixels change
        // without UI Toolkit's own dirty-tracking noticing (the same reason VideoPanel.Refresh marks its Frames dirty)
        private void RenderNow()
        {
            _slot.RenderNow(State, default);
            Surface.MarkDirtyRepaint();
        }

        private void SetAutoSpinTicking(bool on)
        {
            _idleTick?.Pause();
            _idleTick = on ? Root.schedule.Execute(TickAutoSpin).Every(AutoSpinTickMs) : null;
        }

        private void TickAutoSpin(TimerState ts)
        {
            if (_slot == null || _slot.IsLoading || _slot.Failed || _pointers.Count > 0) return;
            float before = State.Yaw;
            State = TurntableRule.Idle(State, ts.deltaTime / 1000f);
            if (!Mathf.Approximately(before, State.Yaw)) RenderNow();
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            _pointers[evt.pointerId] = evt.localPosition;
            Frame.CapturePointer(evt.pointerId);
            if (_pointers.Count == 2) _lastPinchDistance = PinchDistance();
            else if (_pointers.Count == 1) _lastDragPoint = evt.localPosition;
            evt.StopPropagation();
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!_pointers.ContainsKey(evt.pointerId) || _slot == null) return;
            _pointers[evt.pointerId] = evt.localPosition;
            if (_pointers.Count >= 2)
            {
                float distance = PinchDistance();
                if (_lastPinchDistance > 0f && distance > 0f) Pinch(distance / _lastPinchDistance);
                _lastPinchDistance = distance;
            }
            else
            {
                Vector2 point = evt.localPosition;
                Rotate(point - _lastDragPoint);
                _lastDragPoint = point;
            }
            evt.StopPropagation();
        }

        // Rotate by a drag of `delta` panel units (a real one-finger drag, or a test driving the gesture directly,
        // the same shape as ZoomImageBlockView.ZoomAbout)
        public void Rotate(Vector2 delta)
        {
            if (_slot == null) return;
            State = TurntableRule.Drag(State, delta, DegreesPerPixel);
            RenderNow();
        }

        // Zoom by `factor` (>1 zooms in), the same shape as a real two-finger pinch: exposed so a test can drive it
        // directly, the way a mouse alone cannot produce two simultaneous pointers
        public void Pinch(float factor)
        {
            if (_slot == null) return;
            State = TurntableRule.Pinch(State, factor);
            RenderNow();
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
            foreach (var p in _pointers.Values) _lastDragPoint = p;
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
    }
}
