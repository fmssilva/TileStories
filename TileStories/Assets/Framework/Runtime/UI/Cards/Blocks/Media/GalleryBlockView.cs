using System.Collections.Generic;
using UnityEngine.UIElements;

namespace TileStories
{
    // The gallery block (_3.1 Tier 2): the POI's pictures in one of four looks, and in every look a tap on a picture opens
    // it full screen (the lightbox: IBlockHost.OpenTakeover, one page per picture, caption + credit under it).
    //   carousel  -- one picture at a time on a horizontal swipe track, dots under it following the scroll
    //   grid      -- small pictures two per row
    //   filmstrip -- one large picture + caption, the others as a strip of small ones; a tap on a small one shows it large
    //   stack     -- up to three pictures fanned like prints, "n pictures" under them
    // Only complete rows are shown (a picture MediaPathRule accepts). Pictures load through the block's own media scope.
    // Only classes here; Media.uss holds every size and colour.
    public sealed class GalleryBlockView : IBlockView
    {
        // One picture on the card: its tap target, its CardImage, its caption (carousel only), its page in the lightbox
        public sealed class Shot
        {
            public VisualElement Box;
            public CardImage Image;
            public Label Caption;
            public int Page;
        }

        private readonly struct Picture
        {
            public readonly string Path, Caption, Credit;
            public Picture(string path, string caption, string credit) { Path = path; Caption = caption; Credit = credit; }
        }

        public VisualElement Root { get; }
        // carousel / filmstrip: the horizontal track of pictures (the filmstrip's small ones)
        public ScrollView Track { get; }
        // carousel: one dot per picture, the shown one marked
        public VisualElement Dots { get; }
        // filmstrip: the large picture and its caption
        public CardImage Main { get; }
        public Label MainCaption { get; }
        // stack: "n pictures"
        public Label Count { get; }
        // grid / stack: the pictures' own container
        public VisualElement Grid { get; }

        public IReadOnlyList<Shot> Shots => _shown;
        public int PictureCount => _pictures.Count;
        // filmstrip: which picture is large; carousel: which one the dots mark
        public int Current { get; private set; }

        private readonly List<Shot> _pool = new();
        private readonly List<Shot> _shown = new();
        private readonly List<Picture> _pictures = new();
        private readonly List<VisualElement> _dots = new();
        private BlockBindContext _context;
        private string _variantClass;

        public GalleryBlockView()
        {
            Root = new VisualElement();
            Root.AddToClassList("card-block");
            Root.AddToClassList("card-gallery");
            Track = new ScrollView(ScrollViewMode.Horizontal)
            {
                horizontalScrollerVisibility = ScrollerVisibility.Hidden,
                verticalScrollerVisibility = ScrollerVisibility.Hidden,
            };
            Track.AddToClassList("card-gallery__track");
            Track.contentContainer.AddToClassList("card-gallery__track-content");
            Track.horizontalScroller.valueChanged += _ => FollowTrack();
            Dots = new VisualElement { pickingMode = PickingMode.Ignore };
            Dots.AddToClassList("card-gallery__dots");
            Main = new CardImage("card-gallery__main");
            Main.Root.AddToClassList("card-tap");
            Main.Root.AddManipulator(new Clickable(() => OpenLightbox(Current)));
            MainCaption = new Label();
            MainCaption.AddToClassList("card-gallery__caption");
            Count = new Label();
            Count.AddToClassList("card-gallery__count");
            Grid = new VisualElement();
            Grid.AddToClassList("card-gallery__grid");
        }

        public void Bind(BlockInstanceData instance, BlockBindContext context)
        {
            Unbind();
            _context = context;
            _variantClass = "card-gallery--" + context.Variant;
            Root.AddToClassList(_variantClass);
            var read = new BlockFieldReader(instance, context.Language, context.FallbackLanguage);
            var rowFields = BuiltInBlocks.Gallery.Field(BuiltInBlocks.GalleryItemsField).ItemFields;
            foreach (var item in read.Items(BuiltInBlocks.GalleryItemsField))
            {
                if (!BlockFieldReader.ItemIsComplete(item, rowFields)) continue;
                _pictures.Add(new Picture(read.ItemValidAsset(item, BuiltInBlocks.GalleryImageField, MediaKind.Image),
                    read.ItemText(item, BuiltInBlocks.GalleryCaptionField), read.ItemText(item, BuiltInBlocks.GalleryCreditField)));
            }

            switch (context.Variant)
            {
                case BuiltInBlocks.GalleryGrid:
                    Root.Add(Grid);
                    for (int i = 0; i < _pictures.Count; i++) Grid.Add(TakeShot(i, withCaption: false).Box);
                    break;
                case BuiltInBlocks.GalleryFilmstrip:
                    Root.Add(Main.Root);
                    Root.Add(MainCaption);
                    Root.Add(Track);
                    for (int i = 0; i < _pictures.Count; i++)
                    {
                        var shot = TakeShot(i, withCaption: false);
                        shot.Box.AddToClassList("card-gallery__thumb");
                        Track.Add(shot.Box);
                    }
                    ShowLarge(0);
                    break;
                case BuiltInBlocks.GalleryStack:
                    Root.Add(Grid);
                    // - drawn back to front: the first picture ends on top
                    for (int i = System.Math.Min(_pictures.Count, 3) - 1; i >= 0; i--)
                    {
                        var shot = TakeShot(i, withCaption: false);
                        shot.Box.AddToClassList("card-gallery__print--" + i);
                        Grid.Add(shot.Box);
                    }
                    Count.text = string.Format(context.Strings?.Get(CardStrings.Keys.GalleryCount) ?? "{0}", _pictures.Count);
                    Root.Add(Count);
                    break;
                default:
                    Root.Add(Track);
                    Root.Add(Dots);
                    for (int i = 0; i < _pictures.Count; i++)
                    {
                        Track.Add(TakeShot(i, withCaption: true).Box);
                        var dot = new VisualElement { pickingMode = PickingMode.Ignore };
                        dot.AddToClassList("card-gallery__dot");
                        _dots.Add(dot);
                        Dots.Add(dot);
                    }
                    Current = 0;
                    MarkDots();
                    break;
            }
        }

        public void Unbind()
        {
            foreach (var shot in _shown)
            {
                shot.Box.RemoveFromHierarchy();
                shot.Box.RemoveFromClassList("card-gallery__thumb");
                for (int i = 0; i < 3; i++) shot.Box.RemoveFromClassList("card-gallery__print--" + i);
                shot.Image.Clear(null);
                _pool.Add(shot);
            }
            _shown.Clear();
            _pictures.Clear();
            foreach (var dot in _dots) dot.RemoveFromHierarchy();
            _dots.Clear();
            Main.Clear(null);
            Root.Clear();
            Track.Clear();
            Grid.Clear();
            Track.scrollOffset = UnityEngine.Vector2.zero;
            if (_variantClass != null) Root.RemoveFromClassList(_variantClass);
            _variantClass = null;
            _context = null;
            Current = 0;
        }

        // The filmstrip's large picture: picture `index`, its small one marked
        public void ShowLarge(int index)
        {
            if (index < 0 || index >= _pictures.Count) return;
            Current = index;
            // - the large picture shown before goes back to the source now, not at unbind (ScopedMediaSource.Release: one load)
            if (Main.Texture != null) _context.Media.Release(Main.Path);
            Main.Show(_context.Media, _pictures[index].Path, _context.Strings);
            MainCaption.text = _pictures[index].Caption;
            MainCaption.style.display = MainCaption.text.Length > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            foreach (var shot in _shown) shot.Box.EnableInClassList("card-gallery__thumb--current", shot.Page == index);
        }

        // Open picture `page` full screen (the card stays as it is underneath)
        public void OpenLightbox(int page)
        {
            if (_context?.Host == null || _pictures.Count == 0) return;
            var pictures = new List<Picture>(_pictures);
            var strings = _context.Strings;
            _context.Host.OpenTakeover(strings?.Get(CardStrings.Keys.GalleryName) ?? "", pictures.Count, page,
                (view, container, media, index) => DrawLightboxPage(view, container, media, strings, pictures[index]));
        }

        // One lightbox page: the picture fitted to the screen -- a swipe sideways turns the page, two fingers look closer
        // (ZoomPanSurface, as zoom_image) -- its caption and credit under it
        private static void DrawLightboxPage(TakeoverView view, VisualElement container, IMediaSource media, CardStrings strings, Picture picture)
        {
            var surface = new ZoomPanSurface("card-gallery__full", "card-gallery__full-picture") { Swipes = true };
            surface.Frame.name = "card-gallery-full";
            surface.Swiped += view.Turn;
            container.Add(surface.Frame);
            surface.Show(media, picture.Path, strings);
            foreach (var (text, cls) in new[] { (picture.Caption, "card-gallery__full-caption"), (picture.Credit, "card-gallery__full-credit") })
            {
                if (text.Length == 0) continue;
                var label = new Label(text);
                label.AddToClassList(cls);
                container.Add(label);
            }
        }

        private Shot TakeShot(int page, bool withCaption)
        {
            Shot shot;
            if (_pool.Count > 0)
            {
                shot = _pool[_pool.Count - 1];
                _pool.RemoveAt(_pool.Count - 1);
            }
            else
            {
                shot = new Shot { Box = new VisualElement(), Image = new CardImage("card-gallery__picture"), Caption = new Label() };
                shot.Box.AddToClassList("card-gallery__shot");
                shot.Box.AddToClassList("card-tap");
                shot.Caption.AddToClassList("card-gallery__caption");
                shot.Box.Add(shot.Image.Root);
                shot.Box.Add(shot.Caption);
                var s = shot;
                shot.Box.AddManipulator(new Clickable(() => OnShotTapped(s)));
            }
            shot.Page = page;
            shot.Image.Show(_context.Media, _pictures[page].Path, _context.Strings);
            shot.Caption.text = withCaption ? _pictures[page].Caption : "";
            shot.Caption.style.display = shot.Caption.text.Length > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            _shown.Add(shot);
            return shot;
        }

        // A tap on a small filmstrip picture shows it large; on any other picture it opens the lightbox there
        private void OnShotTapped(Shot shot)
        {
            if (_context == null) return;
            if (_context.Variant == BuiltInBlocks.GalleryFilmstrip) ShowLarge(shot.Page);
            else OpenLightbox(shot.Page);
        }

        // Carousel: the dot of the picture nearest the track's left edge
        private void FollowTrack()
        {
            if (_dots.Count == 0 || _shown.Count == 0) return;
            float page = _shown[0].Box.layout.width;
            if (float.IsNaN(page) || page <= 0f) return;
            Current = UnityEngine.Mathf.Clamp(UnityEngine.Mathf.RoundToInt(Track.scrollOffset.x / page), 0, _dots.Count - 1);
            MarkDots();
        }

        private void MarkDots()
        {
            for (int i = 0; i < _dots.Count; i++) _dots[i].EnableInClassList("card-gallery__dot--current", i == Current);
        }
    }
}
