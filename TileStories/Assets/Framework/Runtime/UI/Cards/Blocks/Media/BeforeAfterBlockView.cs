using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories
{
    // The before_after block (_3.1 Tier 2), slider: the After picture fills the frame, the Before picture lies over it cut
    // off at the handle -- Before shows left of the handle, After right of it. A press anywhere on the frame moves the
    // handle there and a drag follows the finger (pointer capture, so the stack does not scroll meanwhile). The labels
    // are the block's own words, else CardStrings' Before / After. Position (0..1) is view state, reset on every bind.
    // Only classes here; Media.uss holds every size and colour (the cut is a percentage of the frame, from the position).
    public sealed class BeforeAfterBlockView : IBlockView
    {
        public VisualElement Root { get; }
        public VisualElement Frame { get; }
        public CardImage After { get; }
        // The Before picture's clip: its width is the handle's position
        public VisualElement BeforeClip { get; }
        public CardImage Before { get; }
        public VisualElement Handle { get; }
        public VisualElement Knob { get; }
        public Label BeforeLabel { get; }
        public Label AfterLabel { get; }

        // 0 = all After, 1 = all Before
        public float Position { get; private set; }
        public bool IsDragging { get; private set; }

        public BeforeAfterBlockView()
        {
            Root = new VisualElement();
            Root.AddToClassList("card-block");
            Root.AddToClassList("card-before-after");
            Frame = new VisualElement { name = "card-before-after-frame" };
            Frame.AddToClassList("card-before-after__frame");
            After = new CardImage("card-before-after__after");
            BeforeClip = new VisualElement { pickingMode = PickingMode.Ignore };
            BeforeClip.AddToClassList("card-before-after__clip");
            Before = new CardImage("card-before-after__before");
            Before.Root.pickingMode = PickingMode.Ignore;
            After.Root.pickingMode = PickingMode.Ignore;
            BeforeClip.Add(Before.Root);
            Handle = new VisualElement { pickingMode = PickingMode.Ignore };
            Handle.AddToClassList("card-before-after__handle");
            Knob = new VisualElement { pickingMode = PickingMode.Ignore };
            Knob.AddToClassList("card-before-after__knob");
            Handle.Add(Knob);
            BeforeLabel = new Label { pickingMode = PickingMode.Ignore };
            BeforeLabel.AddToClassList("card-before-after__label");
            BeforeLabel.AddToClassList("card-before-after__label--before");
            AfterLabel = new Label { pickingMode = PickingMode.Ignore };
            AfterLabel.AddToClassList("card-before-after__label");
            AfterLabel.AddToClassList("card-before-after__label--after");
            Frame.Add(After.Root);
            Frame.Add(BeforeClip);
            Frame.Add(Handle);
            Frame.Add(BeforeLabel);
            Frame.Add(AfterLabel);
            Root.Add(Frame);

            Frame.RegisterCallback<PointerDownEvent>(OnPointerDown);
            Frame.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            Frame.RegisterCallback<PointerUpEvent>(OnPointerUp);
            // - the Before picture keeps the frame's full width inside its narrower clip, so it is cut, never squeezed
            Frame.RegisterCallback<GeometryChangedEvent>(_ => Before.Root.style.width = Frame.contentRect.width);
        }

        public void Bind(BlockInstanceData instance, BlockBindContext context)
        {
            var read = new BlockFieldReader(instance, context.Language, context.FallbackLanguage);
            Before.Show(context.Media, read.ValidAsset(BuiltInBlocks.BeforeAfterBeforeField, MediaKind.Image), context.Strings);
            After.Show(context.Media, read.ValidAsset(BuiltInBlocks.BeforeAfterAfterField, MediaKind.Image), context.Strings);
            BeforeLabel.text = LabelOr(read.Text(BuiltInBlocks.BeforeAfterBeforeLabelField), context.Strings, CardStrings.Keys.BeforeLabel);
            AfterLabel.text = LabelOr(read.Text(BuiltInBlocks.BeforeAfterAfterLabelField), context.Strings, CardStrings.Keys.AfterLabel);
            SetPosition(read.Number(BuiltInBlocks.BeforeAfter.Field(BuiltInBlocks.BeforeAfterStartField)));
        }

        public void Unbind()
        {
            IsDragging = false;
            Before.Clear(null);
            After.Clear(null);
        }

        // Put the handle at `position` (clamped to 0..1)
        public void SetPosition(float position)
        {
            Position = Mathf.Clamp01(position);
            BeforeClip.style.width = Length.Percent(Position * 100f);
            Handle.style.left = Length.Percent(Position * 100f);
        }

        private static string LabelOr(string own, CardStrings strings, string key) => own.Length > 0 ? own : strings?.Get(key) ?? "";

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.button != 0 && evt.pointerType == UnityEngine.UIElements.PointerType.mouse) return;
            Frame.CapturePointer(evt.pointerId);
            IsDragging = true;
            Follow(evt.localPosition.x);
            evt.StopPropagation();
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!IsDragging || !Frame.HasPointerCapture(evt.pointerId)) return;
            Follow(evt.localPosition.x);
            evt.StopPropagation();
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (!Frame.HasPointerCapture(evt.pointerId)) return;
            Frame.ReleasePointer(evt.pointerId);
            IsDragging = false;
            evt.StopPropagation();
        }

        private void Follow(float localX)
        {
            float width = Frame.layout.width;
            if (width > 0f) SetPosition(localX / width);
        }
    }
}
