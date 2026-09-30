using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories
{
    // The model_3d block (_3.1 step 10A.2b.3, Display Takeover in 10A.3.1). Display Inline draws the card's ONE preview
    // owner (BlockBindContext.Preview) into a RenderTexture this view draws; a one-finger drag rotates it
    // (TurntableRule), two fingers pinch to zoom within limits, and -- when Auto Spin is on -- it turns on its own
    // after a pause with no touch, the same pause a touch resets. A render is asked for only after a real gesture or
    // an auto-spin tick that actually moved the model, never every frame on its own (IPreviewHandle's own contract).
    // Display Takeover instead shows a non-interactive TEASER (the same picture, statically rendered once, the
    // model's name, an open-full-screen button); a tap opens the model full screen through the card's TakeoverView
    // (IBlockHost.OpenTakeover), a second instance of this same class bound to its own SECOND preview slot (the
    // takeover key is never the card's own resting one) through BindFullScreen -- same shape as VideoBlockView's
    // takeover, off ICardPreview's slot manager instead of ICardVideo's single-current owner. The Fallback Picture
    // (or, absent one, CardImage's own "unavailable" words) shows while the model loads and if it never loads at
    // all; a small "Loading..." note is added on top of that while it is still on its way. Only classes here;
    // Media.uss holds every size and colour.
    public sealed class ModelTurntableBlockView : IBlockView
    {
        public VisualElement Root { get; }
        public VisualElement Frame { get; }
        public VisualElement Surface { get; }
        public CardImage Fallback { get; }
        public Label Loading { get; }
        public Label Hint { get; }
        // The Display Takeover teaser's own elements (hidden on Display Inline)
        public Label TeaserName { get; }
        public Button TeaserOpenButton { get; }
        // The second instance drawn full screen while the takeover is open (null otherwise; VideoBlockView.FullScreenPanel's own shape)
        public ModelTurntableBlockView FullScreenView { get; private set; }

        public TurntableState State { get; private set; } = TurntableState.Start;
        // What the surface is drawing now (the model's real frames, not the fallback picture)
        public bool ShowsModel { get; private set; }
        // The slot's own RenderTexture (null until ready): tests read its real pixels directly, the same texture Surface draws
        public RenderTexture Texture => _slot?.Texture;
        public string Display { get; private set; } = CardOptions.DisplayInline;

        // A second preview slot's key never collides with the block's own resting one (BindFullScreen)
        private const string TakeoverKeySuffix = "::takeover";
        // How many screen pixels of drag turn the model one degree: a plain gesture-sensitivity constant, not a "look"
        private const float DegreesPerPixel = 0.3f;
        // How often an auto-spinning model asks for a new frame: smooth enough to read as motion, cheap enough to leave running
        private const long AutoSpinTickMs = 33;

        private BlockBindContext _context;
        private BlockInstanceData _instance;
        private ICardPreview _preview;
        private ICardPreviewSlot _slot;
        private string _previewKey = "";
        private CardStrings _strings;
        private bool _autoSpin;
        private bool _teaser;
        private string _title = "";
        private readonly Dictionary<int, Vector2> _pointers = new();
        private float _lastPinchDistance;
        private Vector2 _lastDragPoint;
        private IVisualElementScheduledItem _idleTick;

        // `modifierClass`: the instance's place ("card-model3d--full" for the full-screen page, VideoPanel's own shape)
        public ModelTurntableBlockView(string modifierClass = null)
        {
            Root = new VisualElement { name = "card-model3d-block" };
            Root.AddToClassList("card-block");
            Root.AddToClassList("card-model3d");
            if (modifierClass != null) Root.AddToClassList(modifierClass);

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

            TeaserName = new Label { name = "card-model3d-teaser-name", pickingMode = PickingMode.Ignore };
            TeaserName.AddToClassList("card-model3d__teaser-name");
            TeaserOpenButton = new Button { name = "card-model3d-teaser-open" };
            TeaserOpenButton.AddToClassList("card-video__overlay"); // the shared round media-overlay button (video's own)
            var glyph = CardIcons.CreateVector(CardIcons.Shape.FullScreen);
            glyph.AddToClassList("card-video__overlay-glyph");
            TeaserOpenButton.Add(glyph);
            TeaserOpenButton.clicked += OpenFullScreen;
            Frame.Add(TeaserName);
            Frame.Add(TeaserOpenButton);

            Hint = new Label();
            Hint.AddToClassList("card-model3d__hint");

            Root.Add(Frame);
            Root.Add(Hint);

            Frame.RegisterCallback<PointerDownEvent>(OnPointerDown);
            Frame.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            Frame.RegisterCallback<PointerUpEvent>(OnPointerUp);
            Frame.RegisterCallback<PointerCancelEvent>(evt => Lift(evt.pointerId));
            Frame.RegisterCallback<GeometryChangedEvent>(OnFrameResized);
        }

        // Display Inline or Takeover, resolved from the instance: the card's own resting slot either way
        public void Bind(BlockInstanceData instance, BlockBindContext context)
        {
            Display = BlockDisplayRule.Resolve(BuiltInBlocks.Model3D, instance?.display);
            BindCore(instance, context, instance?.key ?? "", showTeaser: Display == CardOptions.DisplayTakeover);
        }

        // The Display Takeover teaser's full-screen page (TakeoverView): always the full interactive turntable,
        // through its OWN second preview slot (the card's own resting slot, and any auto-spin on it, is untouched).
        public void BindFullScreen(BlockInstanceData instance, BlockBindContext context)
        {
            Display = CardOptions.DisplayInline; // the full-screen page is never itself a teaser
            BindCore(instance, context, (instance?.key ?? "") + TakeoverKeySuffix, showTeaser: false);
        }

        private void BindCore(BlockInstanceData instance, BlockBindContext context, string previewKey, bool showTeaser)
        {
            Unbind();
            _context = context;
            _instance = instance;
            _previewKey = previewKey;
            _teaser = showTeaser;
            _preview = context.Preview;
            _strings = context.Strings;
            var read = new BlockFieldReader(instance, context.Language, context.FallbackLanguage);
            string modelPath = read.ValidAsset(BuiltInBlocks.Model3DModelField, MediaKind.Model);
            string fallbackPath = read.ValidAsset(BuiltInBlocks.Model3DFallbackField, MediaKind.Image);
            _autoSpin = read.Flag(BuiltInBlocks.Model3DAutoSpinField);
            _title = read.Text(BuiltInBlocks.Model3DTitleField);
            if (_title.Length == 0) _title = BlockStackBuilder.CardTitleOf(context.Poi, context.Language, context.FallbackLanguage);
            State = TurntableState.Start;

            if (fallbackPath.Length > 0) Fallback.Show(context.Media, fallbackPath, context.Strings);
            else Fallback.Clear(context.Strings);

            Root.EnableInClassList("card-model3d--teaser", _teaser);
            Hint.style.display = _teaser ? DisplayStyle.None : DisplayStyle.Flex;
            Hint.text = _teaser ? "" : context.Strings?.Get(CardStrings.Keys.Model3DHint) ?? "";
            TeaserName.style.display = _teaser ? DisplayStyle.Flex : DisplayStyle.None;
            TeaserName.text = _teaser ? _title : "";
            TeaserOpenButton.style.display = _teaser ? DisplayStyle.Flex : DisplayStyle.None;
            TeaserOpenButton.tooltip = _teaser ? context.Strings?.Get(CardStrings.Keys.VideoFullScreen) ?? "" : "";

            _slot = _preview?.Request(_previewKey, MediaKind.Model, modelPath);
            // - the Frame may already be laid out (a pooled view rebinding to the same slot size): size the new slot's
            // texture at once rather than waiting for a GeometryChangedEvent that may never fire again for this size
            ApplySize(Frame.resolvedStyle.width, Frame.resolvedStyle.height);
            if (_preview != null) _preview.Changed += Refresh;
            // - the teaser is a static "first render", never spinning on its own (_3.1 10A.3.1)
            SetAutoSpinTicking(!_teaser && _autoSpin);
            Refresh();
        }

        public void Unbind()
        {
            SetAutoSpinTicking(false);
            if (_preview != null)
            {
                _preview.Changed -= Refresh;
                _preview.Release(_previewKey);
            }
            _preview = null;
            _slot = null;
            _strings = null;
            _context = null;
            _instance = null;
            FullScreenView = null;
            _pointers.Clear();
            _lastPinchDistance = 0f;
            Surface.style.backgroundImage = StyleKeyword.Null;
            Fallback.Clear(null);
        }

        // The teaser's tap (or the open-full-screen button): opens the model full screen through the card's
        // TakeoverView, a second instance of this same view bound to its own second preview slot
        private void OpenFullScreen()
        {
            if (_context?.Host == null || _instance == null) return;
            var context = _context;
            var instance = _instance;
            string title = _title;
            context.Host.OpenTakeover(title, 1, 0, (view, container, media, page) =>
            {
                var full = new ModelTurntableBlockView("card-model3d--full");
                container.Add(full.Root);
                full.BindFullScreen(instance, context.ForBlock(media, context.Variant));
                FullScreenView = full;
                // - Back clears the page: the second slot is released the moment it leaves the screen
                full.Root.RegisterCallback<DetachFromPanelEvent>(_ =>
                {
                    full.Unbind();
                    if (FullScreenView == full) FullScreenView = null;
                });
            });
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

        // The stage's real pixel size, once it changes (or reports for the first time): the RenderTexture is
        // recreated at that aspect so it matches the box it is shown in instead of a square letterboxed into it.
        private void ApplySize(float width, float height)
        {
            if (_slot == null) return;
            int w = Mathf.RoundToInt(width);
            int h = Mathf.RoundToInt(height);
            if (w <= 0 || h <= 0) return;
            _slot.Resize(w, h);
        }

        private void OnFrameResized(GeometryChangedEvent evt)
        {
            ApplySize(evt.newRect.width, evt.newRect.height);
            Refresh();
        }

        private void SetAutoSpinTicking(bool on)
        {
            _idleTick?.Pause();
            _idleTick = on ? Root.schedule.Execute(TickAutoSpin).Every(AutoSpinTickMs) : null;
        }

        private void TickAutoSpin(TimerState ts)
        {
            if (_slot == null || _slot.IsLoading || _slot.Failed || _pointers.Count > 0) return;
            // - _3.1 10A.2c.2: a block scrolled out of the stack's viewport, or sitting below the sheet's Peek stop
            // (which shrinks the same scroll viewport to just the header), asks for nothing: no state advance, no render.
            if (!IsStageVisible()) return;
            float before = State.Yaw;
            State = TurntableRule.Idle(State, ts.deltaTime / 1000f);
            if (!Mathf.Approximately(before, State.Yaw)) RenderNow();
        }

        // Whether the Frame is actually inside the visible part of its enclosing scroll viewport right now (Peek
        // shrinks that same viewport to the header's height, and scrolling moves a block above/below it -- one
        // geometric check covers both cases the TODO asks for). A view outside any ScrollView (the gallery's own
        // static harness setups, if any) is always considered visible.
        private bool IsStageVisible()
        {
            var scroll = Frame.GetFirstAncestorOfType<ScrollView>();
            if (scroll == null) return true;
            var viewport = scroll.contentViewport.worldBound;
            if (viewport.width <= 0f || viewport.height <= 0f) return false;
            return Frame.worldBound.Overlaps(viewport);
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            // - the teaser is not itself draggable: only its open-full-screen button reacts (_3.1 10A.3.1)
            if (_teaser) return;
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
