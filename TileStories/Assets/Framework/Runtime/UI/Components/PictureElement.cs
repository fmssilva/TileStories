using System.Collections.Generic;
using UnityEngine.UIElements;

namespace TileStories
{
    // The picture element (_3.3 R14.2, the prototype's magazine pictures): one picture in a clipped frame that takes the picture's own
    // proportion, a rectangle or an arch, inset in the text's margins or full bleed (edge to edge), then its caption and its credit. On the
    // PAGE a tap opens it in the shared picture viewer when the row's Tap says so (R17.4, Open The Viewer by default); on the preview the
    // whole block is the tap (it opens the page), so the picture takes no finger there. Its picture is what the row's parallax / slow zoom /
    // pan moves (IFramedPictures, ElementMotion). Loads through the block's own media scope; any panel (R15.2).
    public sealed class PictureElement : VisualElement, IElementComponent, IFramedPictures
    {
        public VisualElement Root => this;
        public MediaFrame Frame { get; }
        public MediaCaption Words { get; }
        public IReadOnlyList<VisualElement> Pictures => _pictures;
        // Whether a tap here opens the viewer now (the page, and the row's Tap)
        public bool OpensViewer { get; private set; }

        private readonly List<VisualElement> _pictures = new();
        private readonly Clickable _tap;
        private ElementBindContext _context;
        private PictureViewer.Picture _shown;

        public PictureElement()
        {
            ElementStyle.Init(this, BuiltInElements.Picture);
            AddToClassList("ts-el-media");
            Frame = new MediaFrame();
            Frame.Root.name = "ts-el-picture-frame";
            Words = new MediaCaption();
            Add(Frame.Root);
            Add(Words.Root);
            _tap = new Clickable(OpenViewer);
        }

        public void Bind(ElementRow row, ElementBindContext context)
        {
            _context = context;
            ElementStyle.Apply(this, row, context);
            var media = row.Media;
            var item = media.Count > 0 ? media[0] : default;
            var (caption, credit) = media.Count > 0 ? MediaFrame.WordsOf(item, context) : ("", "");
            bool shown = Frame.Show(context.Block?.Media, item.Asset, context.Block?.Strings, ElementStyle.IsArch(row, context));
            // - an Automatic preview is a summary: the picture without its words (the page carries them)
            Words.Show(context.Compact ? "" : caption, context.Compact ? "" : credit);
            _shown = new PictureViewer.Picture(Frame.Image.Path, caption, credit);
            _pictures.Clear();
            if (shown) _pictures.Add(Frame.Picture);
            OpensViewer = shown && context.Surface == ElementSurface.Page
                && ElementRules.TapOf(row, context.Element) == ElementOptions.TapOpenViewer && context.Block?.Host != null;
            this.RemoveManipulator(_tap);
            if (OpensViewer) this.AddManipulator(_tap);
            EnableInClassList("card-tap", OpensViewer);
        }

        public void Unbind()
        {
            this.RemoveManipulator(_tap);
            OpensViewer = false;
            _pictures.Clear();
            Frame.Clear();
            Words.Show("", "");
            _context = null;
        }

        // The tap: this picture in the viewer, its caption and credit under it (Back returns to the page)
        private void OpenViewer()
        {
            if (!OpensViewer || _context?.Block == null) return;
            PictureViewer.Open(_context.Block.Host, _context.Block.Strings?.Get(CardStrings.Keys.GalleryName) ?? "", new[] { _shown }, 0, _context.Block.Strings);
        }
    }
}
