using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories
{
    // What model_3d and panorama_360 share (_3.1 step 10A.4; first built as the model's own view in 10A.2b.3 and 10A.3): a stage
    // that draws the card's ONE preview owner (BlockBindContext.Preview) into a RenderTexture, a one-finger drag, a two-finger
    // pinch (or Ctrl + wheel on a desktop), a timer that may move the view on its own, the Fallback Picture + loading note, the
    // Display Takeover teaser and its full-screen page. A subclass says only what is its own: which field holds the media, what a
    // drag / pinch / tick does to ITS state, and what to ask the slot to render. A render is asked for only after a real gesture or
    // a tick that actually moved the view, never every frame on its own (IPreviewHandle's own contract). Display Takeover shows a
    // non-interactive TEASER (the same picture, statically rendered once, the name, an open-full-screen button); a tap opens the
    // subject full screen through the card's TakeoverView (IBlockHost.OpenTakeover), a second instance of the same subclass bound to
    // its own SECOND preview slot (the takeover key is never the card's own resting one) through BindFullScreen -- the same shape as
    // VideoBlockView's takeover, off ICardPreview's slot manager instead of ICardVideo's single-current owner. Only classes here;
    // Media.uss holds every size and colour.
    public abstract class PreviewBlockView : IBlockView
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

        // What the surface is drawing now (the subject's real frames, not the fallback picture)
        public bool ShowsSubject { get; private set; }
        // The slot's own RenderTexture (null until ready): tests read its real pixels directly, the same texture Surface draws
        public RenderTexture Texture => _slot?.Texture;
        public string Display { get; private set; } = CardOptions.DisplayInline;
        // The page drawn full screen while the takeover is open (null otherwise; VideoBlockView.FullScreenPanel's own shape)
        protected PreviewBlockView FullScreen { get; private set; }

        // A second preview slot's key never collides with the block's own resting one (BindFullScreen)
        private const string TakeoverKeySuffix = "::takeover";
        // How often a view that moves on its own asks for a new frame: smooth enough to read as motion, cheap enough to leave running
        private const long TickMs = 33;
        // Wheel notches (Ctrl held) per doubling of the zoom: a trackpad pinch arrives as exactly this
        private const float WheelNotchesPerDoubling = 12f;

        private BlockBindContext _context;
        private BlockInstanceData _instance;
        private ICardPreviewSlot _slot;
        private string _previewKey = "";
        private bool _teaser;
        // The takeover's page (BindFullScreen): it is not under the sheet, so the sheet's stop never pauses it
        private bool _fullScreen;
        private string _title = "";
        private readonly Dictionary<int, Vector2> _pointers = new();
        private float _lastPinchDistance;
        private Vector2 _lastDragPoint;
        private IVisualElementScheduledItem _tick;

        // `kindClass`: the kind's own USS class ("card-model3d"); `modifierClass`: the instance's place ("card-preview--full" for the
        // full-screen page, VideoPanel's own shape)
        protected PreviewBlockView(string kindClass, string modifierClass)
        {
            Root = new VisualElement { name = "card-preview-block" };
            Root.AddToClassList("card-block");
            Root.AddToClassList("card-preview");
            Root.AddToClassList(kindClass);
            if (modifierClass != null) Root.AddToClassList(modifierClass);

            Frame = new VisualElement { name = "card-preview-frame" };
            Frame.AddToClassList("card-preview__frame");
            Surface = new VisualElement { name = "card-preview-surface", pickingMode = PickingMode.Ignore };
            Surface.AddToClassList("card-preview__surface");
            Fallback = new CardImage("card-preview__fallback");
            Fallback.Root.pickingMode = PickingMode.Ignore;
            Loading = new Label { pickingMode = PickingMode.Ignore };
            Loading.AddToClassList("card-preview__loading");
            Frame.Add(Surface);
            Frame.Add(Fallback.Root);
            Frame.Add(Loading);

            TeaserName = new Label { name = "card-preview-teaser-name", pickingMode = PickingMode.Ignore };
            TeaserName.AddToClassList("card-preview__teaser-name");
            TeaserOpenButton = new Button { name = "card-preview-teaser-open" };
            TeaserOpenButton.AddToClassList("card-video__overlay"); // the shared round media-overlay button (video's own)
            var glyph = CardIcons.CreateVector(CardIcons.Shape.FullScreen);
            glyph.AddToClassList("card-video__overlay-glyph");
            TeaserOpenButton.Add(glyph);
            TeaserOpenButton.clicked += OpenFullScreen;
            Frame.Add(TeaserName);
            Frame.Add(TeaserOpenButton);

            Hint = new Label();
            Hint.AddToClassList("card-preview__hint");

            Root.Add(Frame);
            Root.Add(Hint);

            Frame.RegisterCallback<PointerDownEvent>(OnPointerDown);
            Frame.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            Frame.RegisterCallback<PointerUpEvent>(OnPointerUp);
            Frame.RegisterCallback<PointerCancelEvent>(evt => Lift(evt.pointerId));
            Frame.RegisterCallback<WheelEvent>(OnWheel);
            Frame.RegisterCallback<GeometryChangedEvent>(OnFrameResized);
        }

        // ---- what a subclass says ----

        // The kind's definition (Display Takeover is resolved against its DisplayModes)
        protected abstract BlockKindDefinition Kind { get; }
        protected abstract MediaKind SubjectKind { get; }
        protected abstract string SubjectField { get; }
        protected abstract string FallbackField { get; }
        protected abstract string TitleField { get; }
        // The bound instance's own starting state (a model's Start pose and Auto Spin; a panorama's Start Heading and look)
        protected abstract void ResetState(BlockFieldReader read);
        // Whether the timer should run while this view is shown (Auto Spin on; the gyro look)
        protected abstract bool WantsTicks { get; }
        // The caption under the stage (the teaser shows none)
        protected abstract string HintText(CardStrings strings);
        protected abstract string LoadingText(CardStrings strings);
        // A second instance of this same view for the full-screen page
        protected abstract PreviewBlockView NewFullScreenView();
        // Ask `slot` for a frame at the current state
        protected abstract void RenderSlot(ICardPreviewSlot slot);
        // A drag of `delta` panel units / a pinch by `factor` (> 1 zooms in): move the state, the base renders
        protected abstract void OnDrag(Vector2 delta);
        protected abstract void OnPinch(float factor);
        // One timer tick of `deltaSeconds` while the view is shown: true when the picture moved and needs a new frame
        protected virtual bool OnTick(float deltaSeconds) => false;
        // The view became bound / is about to be unbound: start / stop whatever a look needs (the gyro sensor)
        protected virtual void OnBound() { }
        // The slot was just requested for this bind (a model hands it its framing)
        protected virtual void OnSlotRequested(ICardPreviewSlot slot) { }
        protected virtual void OnUnbinding() { }

        // ---- what a subclass may look at ----

        protected BlockBindContext Context => _context;
        protected BlockInstanceData Instance => _instance;
        protected bool IsTeaser => _teaser;
        // A finger is down on the stage right now
        protected bool IsTouched => _pointers.Count > 0;
        // The stage's shorter side in panel units (NaN until it is laid out)
        protected float StageShorterSide => Mathf.Min(Frame.resolvedStyle.width, Frame.resolvedStyle.height);
        // The slot has its subject: a loading or failed one has nothing to turn, so a gesture on it changes nothing
        protected bool IsReady => _slot != null && !_slot.IsLoading && !_slot.Failed;
        // Put the hint's words in (a look can change them while bound)
        protected void RefreshHint() => Hint.text = _teaser ? "" : HintText(_context?.Strings) ?? "";

        // Display Inline or Takeover, resolved from the instance: the card's own resting slot either way
        public void Bind(BlockInstanceData instance, BlockBindContext context)
        {
            Display = BlockDisplayRule.Resolve(Kind, instance?.display);
            _fullScreen = false;
            BindCore(instance, context, instance?.key ?? "", showTeaser: Display == CardOptions.DisplayTakeover);
        }

        // The Display Takeover teaser's full-screen page (TakeoverView): always the full interactive view, through its OWN second
        // preview slot (the card's own resting slot, and anything moving on it, is untouched).
        public void BindFullScreen(BlockInstanceData instance, BlockBindContext context)
        {
            Display = CardOptions.DisplayInline; // the full-screen page is never itself a teaser
            _fullScreen = true;
            BindCore(instance, context, (instance?.key ?? "") + TakeoverKeySuffix, showTeaser: false);
        }

        private void BindCore(BlockInstanceData instance, BlockBindContext context, string previewKey, bool showTeaser)
        {
            Unbind();
            _context = context;
            _instance = instance;
            _previewKey = previewKey;
            _teaser = showTeaser;
            var read = new BlockFieldReader(instance, context.Language, context.FallbackLanguage);
            string subjectPath = read.ValidAsset(SubjectField, SubjectKind);
            string fallbackPath = read.ValidAsset(FallbackField, MediaKind.Image);
            _title = read.Text(TitleField);
            if (_title.Length == 0) _title = BlockStackBuilder.CardTitleOf(context.Poi, context.Language, context.FallbackLanguage);
            ResetState(read);

            if (fallbackPath.Length > 0) Fallback.Show(context.Media, fallbackPath, context.Strings);
            else Fallback.Clear(context.Strings);

            Root.EnableInClassList("card-preview--teaser", _teaser);
            Hint.style.display = _teaser ? DisplayStyle.None : DisplayStyle.Flex;
            TeaserName.style.display = _teaser ? DisplayStyle.Flex : DisplayStyle.None;
            TeaserName.text = _teaser ? _title : "";
            TeaserOpenButton.style.display = _teaser ? DisplayStyle.Flex : DisplayStyle.None;
            TeaserOpenButton.tooltip = _teaser ? context.Strings?.Get(CardStrings.Keys.VideoFullScreen) ?? "" : "";

            if (!_teaser) OnBound();
            RefreshHint();
            _slot = context.Preview?.Request(_previewKey, SubjectKind, subjectPath);
            OnSlotRequested(_slot);
            // - the Frame may already be laid out (a pooled view rebinding to the same slot size): size the new slot's
            // texture at once rather than waiting for a GeometryChangedEvent that may never fire again for this size
            ApplySize(Frame.resolvedStyle.width, Frame.resolvedStyle.height);
            if (context.Preview != null) context.Preview.Changed += Refresh;
            // - the teaser is a static "first render", never moving on its own (_3.1 10A.3.1)
            SetTicking(!_teaser && WantsTicks);
            Refresh();
        }

        public void Unbind()
        {
            SetTicking(false);
            if (_context != null && !_teaser) OnUnbinding();
            var preview = _context?.Preview;
            if (preview != null)
            {
                preview.Changed -= Refresh;
                preview.Release(_previewKey);
            }
            _slot = null;
            _context = null;
            _instance = null;
            FullScreen = null;
            _pointers.Clear();
            _lastPinchDistance = 0f;
            Surface.style.backgroundImage = StyleKeyword.Null;
            Fallback.Clear(null);
        }

        // The teaser's tap (or the open-full-screen button): opens the subject full screen through the card's TakeoverView, a second
        // instance of this same view bound to its own second preview slot
        private void OpenFullScreen()
        {
            if (_context?.Host == null || _instance == null) return;
            var context = _context;
            var instance = _instance;
            string title = _title;
            context.Host.OpenTakeover(title, 1, 0, (view, container, media, page) =>
            {
                var full = NewFullScreenView();
                container.Add(full.Root);
                full.BindFullScreen(instance, context.ForBlock(media, context.Variant));
                FullScreen = full;
                // - Back clears the page: the second slot is released the moment it leaves the screen
                full.Root.RegisterCallback<DetachFromPanelEvent>(_ =>
                {
                    full.Unbind();
                    if (FullScreen == full) FullScreen = null;
                });
            });
        }

        // Draw the surface, the fallback and the loading note as the slot stands now
        private void Refresh()
        {
            bool ready = _slot != null && !_slot.IsLoading && !_slot.Failed && _slot.Texture != null;
            ShowsSubject = ready;
            Surface.style.backgroundImage = ready ? new StyleBackground(Background.FromRenderTexture(_slot.Texture)) : StyleKeyword.Null;
            Surface.style.display = ready ? DisplayStyle.Flex : DisplayStyle.None;
            Fallback.Root.style.display = ready ? DisplayStyle.None : DisplayStyle.Flex;
            bool loading = _slot != null && _slot.IsLoading;
            Loading.style.display = loading ? DisplayStyle.Flex : DisplayStyle.None;
            Loading.text = loading ? LoadingText(_context?.Strings) ?? "" : "";
            if (ready) RenderNow();
        }

        // Render the slot's texture at the current state and ask the panel to repaint: a RenderTexture's pixels change
        // without UI Toolkit's own dirty-tracking noticing (the same reason VideoPanel.Refresh marks its Frames dirty)
        protected void RenderNow()
        {
            if (_slot == null) return;
            RenderSlot(_slot);
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

        private void SetTicking(bool on)
        {
            _tick?.Pause();
            _tick = on ? Root.schedule.Execute(Tick).Every(TickMs) : null;
        }

        private void Tick(TimerState ts)
        {
            if (_slot == null || _slot.IsLoading || _slot.Failed) return;
            // - _3.1 10A.2c.2 and 10A.3-fix.1: a stage scrolled out of the stack's viewport, or under the sheet's Peek stop, asks
            // for nothing: no state advance, no render
            if (!IsStageVisible()) return;
            if (OnTick(ts.deltaTime / 1000f)) RenderNow();
        }

        // Whether the Frame is actually on screen right now. At Peek the stage sits under the sheet yet still inside the scroll
        // viewport's own rectangle (the viewport is not clipped to the sheet), so the geometry alone would call it visible: ask the
        // sheet's stop first. The full-screen page lives in the takeover layer, not under the sheet, so the stop never hides it. Then
        // one geometric check covers scrolling: the Frame overlaps its enclosing scroll viewport. A view outside any ScrollView (the
        // gallery's own static harness setups, if any) is always considered visible.
        private bool IsStageVisible()
        {
            if (!_fullScreen && _context?.Host != null && !SheetStopRule.RevealsBlocks(_context.Host.Stop)) return false;
            var scroll = Frame.GetFirstAncestorOfType<ScrollView>();
            if (scroll == null) return true;
            var viewport = scroll.contentViewport.worldBound;
            if (viewport.width <= 0f || viewport.height <= 0f) return false;
            return Frame.worldBound.Overlaps(viewport);
        }

        // Drag by `delta` panel units (a real one-finger drag, or a test driving the gesture directly, the same shape as
        // ZoomImageBlockView.ZoomAbout)
        public void Drag(Vector2 delta)
        {
            if (!IsReady) return;
            OnDrag(delta);
            RenderNow();
        }

        // Zoom by `factor` (>1 zooms in), the same shape as a real two-finger pinch: exposed so a test can drive it
        // directly, the way a mouse alone cannot produce two simultaneous pointers
        public void Pinch(float factor)
        {
            if (!IsReady) return;
            OnPinch(factor);
            RenderNow();
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
            if (!_pointers.ContainsKey(evt.pointerId) || !IsReady) return;
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
                Drag(point - _lastDragPoint);
                _lastDragPoint = point;
            }
            evt.StopPropagation();
        }

        // Ctrl + wheel zooms (a trackpad's pinch, a desktop's only pinch); a plain wheel belongs to the card, which scrolls
        private void OnWheel(WheelEvent evt)
        {
            if (_teaser || (!evt.ctrlKey && !evt.commandKey)) return;
            Pinch(Mathf.Pow(2f, -evt.delta.y / WheelNotchesPerDoubling));
            evt.StopPropagation();
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
