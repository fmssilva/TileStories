using UnityEngine.UIElements;

namespace TileStories
{
    // One picture of a media element (_3.3 R14.2): a clipped frame (a CardImage: the "picture unavailable" words when the file cannot be
    // shown) whose proportion follows its picture (ElementMediaRule.FrameAspect, or a fixed one: the pair's 4:3), a rectangle or an arch (its
    // top a half circle, ElementMediaRule.ArchRadius, kept as the frame is resized). The picture inside it is what a row's motion moves
    // (Picture): while a row moves its pictures it wears ts-el--moves-picture and the picture overhangs its frame on every side (Components.uss),
    // so parallax, a zoom or a pan never show an empty edge. Plain C#, part of a component; loads through the row's media scope.
    public sealed class MediaFrame
    {
        public const string RootClass = "ts-el-media__frame";
        public const string ArchClass = "ts-el-media__frame--arch";

        public CardImage Image { get; }
        public VisualElement Root => Image.Root;
        // The picture inside the frame: what moves
        public VisualElement Picture => Image.Picture;
        // The frame's width / height now (the picture's own, clamped; or the fixed one)
        public float Aspect { get; private set; } = ElementMediaRule.FallbackAspect;
        public bool IsArch { get; private set; }

        public MediaFrame()
        {
            Image = new CardImage(RootClass);
            Root.RegisterCallback<GeometryChangedEvent>(_ => ShapeTop());
        }

        // Show the picture at `path` through `media` in a rectangle or an arch; `fixedAspect` (NaN = follow the picture) fixes the proportion.
        // Returns whether the picture shows
        public bool Show(IMediaSource media, string path, CardStrings strings, bool arch, float fixedAspect = float.NaN)
        {
            IsArch = arch;
            Root.EnableInClassList(ArchClass, arch);
            bool shown = Image.Show(media, path, strings);
            Aspect = float.IsNaN(fixedAspect) ? ElementMediaRule.FrameAspect(shown ? Image.Aspect : float.NaN) : fixedAspect;
            Root.style.aspectRatio = new StyleRatio(Aspect);
            ShapeTop();
            return shown;
        }

        // Forget the picture (the media scope gives the file back) and the shape
        public void Clear()
        {
            Image.Clear(null);
            IsArch = false;
            Root.RemoveFromClassList(ArchClass);
            Root.style.aspectRatio = StyleKeyword.Null;
            ShapeTop();
        }

        // A media item's words in the card's language (its caption, else none) and its credit line, as a MediaCaption shows them
        public static (string Caption, string Credit) WordsOf(ElementMedia item, ElementBindContext context) =>
            (item.Caption(context.Language, context.Fallback) ?? "", item.Credit ?? "");

        // An arch's top: a half circle as wide as the frame (a USS percentage would round it into an ellipse); a rectangle keeps the stylesheet's
        private void ShapeTop()
        {
            if (!IsArch)
            {
                Root.style.borderTopLeftRadius = StyleKeyword.Null;
                Root.style.borderTopRightRadius = StyleKeyword.Null;
                return;
            }
            float radius = ElementMediaRule.ArchRadius(Root.layout.width, Root.layout.height);
            Root.style.borderTopLeftRadius = radius;
            Root.style.borderTopRightRadius = radius;
        }
    }

    // The words under a media item (_3.3 R14.2 "Caption & Credit"): its caption in the card's language, then its credit (author, licence) in a
    // quieter line -- each hidden when empty, so an item with neither leaves no gap. Both read >= 7:1 on the surface (ink-2, Components.uss)
    public sealed class MediaCaption
    {
        public VisualElement Root { get; }
        public Label Caption { get; }
        public Label Credit { get; }

        public MediaCaption()
        {
            Root = new VisualElement { pickingMode = PickingMode.Ignore };
            Root.AddToClassList("ts-el-media__words");
            Caption = new Label { name = "ts-el-media-caption", pickingMode = PickingMode.Ignore };
            Caption.AddToClassList("ts-el-media__caption");
            Credit = new Label { name = "ts-el-media-credit", pickingMode = PickingMode.Ignore };
            Credit.AddToClassList("ts-el-media__credit");
            Root.Add(Caption);
            Root.Add(Credit);
        }

        public void Show(string caption, string credit)
        {
            Caption.text = caption ?? "";
            Credit.text = credit ?? "";
            Caption.style.display = Caption.text.Length > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            Credit.style.display = Credit.text.Length > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            Root.style.display = Caption.text.Length + Credit.text.Length > 0 ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
