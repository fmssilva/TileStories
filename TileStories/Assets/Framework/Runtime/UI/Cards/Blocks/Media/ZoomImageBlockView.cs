using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories
{
    // The zoom_image block (_3.1 Tier 2), pinch: one picture in a frame the visitor looks into -- a ZoomPanSurface (two
    // fingers or Ctrl + wheel zoom 1x..4x about the point, one finger moves an enlarged picture; at 1x a one-finger drag
    // is left to the stack, which scrolls as usual). This view adds the caption and the hint.
    public sealed class ZoomImageBlockView : IBlockView
    {
        public VisualElement Root { get; }
        public VisualElement Frame => _surface.Frame;
        public CardImage Image => _surface.Image;
        public Label Caption { get; }
        public Label Hint { get; }

        public ZoomPan State => _surface.State;
        public float Scale => _surface.Scale;

        private readonly ZoomPanSurface _surface = new("card-zoom__frame", "card-zoom__image");

        public ZoomImageBlockView()
        {
            Root = new VisualElement();
            Root.AddToClassList("card-block");
            Root.AddToClassList("card-zoom");
            Frame.name = "card-zoom-frame";
            Caption = new Label();
            Caption.AddToClassList("card-zoom__caption");
            Hint = new Label();
            Hint.AddToClassList("card-zoom__hint");
            Root.Add(Frame);
            Root.Add(Caption);
            Root.Add(Hint);
        }

        public void Bind(BlockInstanceData instance, BlockBindContext context)
        {
            var read = new BlockFieldReader(instance, context.Language, context.FallbackLanguage);
            _surface.Show(context.Media, read.ValidAsset(BuiltInBlocks.ZoomImageImageField, MediaKind.Image), context.Strings);
            Caption.text = read.Text(BuiltInBlocks.ZoomImageCaptionField);
            Caption.style.display = Caption.text.Length > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            Hint.text = context.Strings?.Get(CardStrings.Keys.ZoomHint) ?? "";
        }

        public void Unbind() => _surface.Clear();

        // Zoom by `factor` about `point` (frame coordinates), as a pinch or a wheel does
        public void ZoomAbout(float factor, Vector2 point) => _surface.ZoomAbout(factor, point);
    }
}
