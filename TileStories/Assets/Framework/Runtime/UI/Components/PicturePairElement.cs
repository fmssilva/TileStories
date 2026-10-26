using System.Collections.Generic;
using UnityEngine.UIElements;

namespace TileStories
{
    // The picture pair element (_3.3 R14.2, the prototype's "Before 1755 / Today"): two pictures side by side in frames of ONE proportion
    // (ElementMediaRule.PairAspect, so the two read as one comparison whatever their files), each with its caption and credit under it;
    // rectangle or arch, inset or full bleed. On the PAGE a tap on either opens both in the shared picture viewer at the one tapped (the row's
    // Tap); the preview takes no finger. Both pictures move with the row's motion (IFramedPictures). Any panel (R15.2).
    public sealed class PicturePairElement : VisualElement, IElementComponent, IFramedPictures
    {
        // One side of the pair: its figure (the tap target), frame and words
        public sealed class Side
        {
            public VisualElement Figure;
            public MediaFrame Frame;
            public MediaCaption Words;
            public Clickable Tap;
        }

        public VisualElement Root => this;
        public IReadOnlyList<Side> Sides => _sides;
        public IReadOnlyList<VisualElement> Pictures => _pictures;
        public bool OpensViewer { get; private set; }

        private readonly Side[] _sides = new Side[2];
        private readonly List<VisualElement> _pictures = new();
        private readonly List<PictureViewer.Picture> _shown = new();
        private ElementBindContext _context;

        public PicturePairElement()
        {
            ElementStyle.Init(this, BuiltInElements.PicturePair);
            AddToClassList("ts-el-media");
            for (int i = 0; i < _sides.Length; i++)
            {
                int index = i;
                var side = new Side { Figure = new VisualElement { name = "ts-el-pair-" + i }, Frame = new MediaFrame(), Words = new MediaCaption() };
                side.Figure.AddToClassList("ts-el-picture-pair__side");
                if (i > 0) side.Figure.AddToClassList("ts-el-picture-pair__side--second");
                side.Figure.Add(side.Frame.Root);
                side.Figure.Add(side.Words.Root);
                side.Tap = new Clickable(() => OpenViewer(index));
                _sides[i] = side;
                Add(side.Figure);
            }
        }

        public void Bind(ElementRow row, ElementBindContext context)
        {
            _context = context;
            ElementStyle.Apply(this, row, context);
            var media = row.Media;
            bool arch = ElementStyle.IsArch(row, context);
            _pictures.Clear();
            _shown.Clear();
            OpensViewer = context.Surface == ElementSurface.Page && ElementRules.TapOf(row, context.Element) == ElementOptions.TapOpenViewer
                && context.Block?.Host != null;
            for (int i = 0; i < _sides.Length; i++)
            {
                var side = _sides[i];
                var item = i < media.Count ? media[i] : default;
                var (caption, credit) = i < media.Count ? MediaFrame.WordsOf(item, context) : ("", "");
                bool shown = side.Frame.Show(context.Block?.Media, item.Asset, context.Block?.Strings, arch, ElementMediaRule.PairAspect);
                side.Words.Show(context.Compact ? "" : caption, context.Compact ? "" : credit);
                if (shown) _pictures.Add(side.Frame.Picture);
                _shown.Add(new PictureViewer.Picture(side.Frame.Image.Path, caption, credit));
                side.Figure.RemoveManipulator(side.Tap);
                bool taps = OpensViewer && shown;
                if (taps) side.Figure.AddManipulator(side.Tap);
                side.Figure.EnableInClassList("card-tap", taps);
            }
        }

        public void Unbind()
        {
            foreach (var side in _sides)
            {
                side.Figure.RemoveManipulator(side.Tap);
                side.Frame.Clear();
                side.Words.Show("", "");
            }
            OpensViewer = false;
            _pictures.Clear();
            _shown.Clear();
            _context = null;
        }

        // A tap on side `index`: both pictures in the viewer, starting at that one
        private void OpenViewer(int index)
        {
            if (!OpensViewer || _context?.Block == null) return;
            PictureViewer.Open(_context.Block.Host, _context.Block.Strings?.Get(CardStrings.Keys.GalleryName) ?? "", _shown, index, _context.Block.Strings);
        }
    }
}
