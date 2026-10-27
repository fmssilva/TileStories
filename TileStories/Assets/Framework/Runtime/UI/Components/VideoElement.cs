using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Video;

namespace TileStories
{
    // The video element (_3.3 R14.2, the prototype's "The square today · 0:55"): a TEASER -- the poster in a clipped frame of the clip's own
    // proportion (inset or full bleed, rectangle or arch), a play button over it and a badge with its caption and length -- then its credit.
    // It never plays anything itself: the card's ONE video owner (BlockBindContext.Video) does, so the story's video is the same playback the
    // card's video block would draw, never a second player. On the PAGE a tap does what the row's Tap says (R17.4):
    //   - Play In Place (the default): the teaser gives way to the shared VideoPanel, drawing the owner's playback where the poster was; its
    //     full-screen button opens the card's player over the same playback (the S2c review), Back returns to the page
    //   - Open The Player: the card's full-screen view opens with a VideoPanel over the same playback (the video block's takeover), and Back
    //     returns to the page; leaving that player pauses the video, as nothing else on the page would show it
    // On the preview the whole block is the tap (it opens the page): the teaser takes no finger. Leaving the page while the video plays in
    // place pauses it too. Its frame and words are Media.uss's video looks + Components.uss; any panel (R15.2).
    public sealed class VideoElement : VisualElement, IElementComponent
    {
        public VisualElement Root => this;
        public VisualElement Teaser { get; }
        public MediaFrame Poster { get; }
        public VisualElement PlayDisc { get; }
        // The badge at the poster's foot: a plate (the surface) and its words (caption + length)
        public VisualElement Badge { get; }
        public Label BadgeWords { get; }
        public MediaCaption Words { get; }
        // The player drawn in place once the visitor played the video there (hidden until then)
        public VideoPanel Panel { get; }

        public VideoTrack Track { get; private set; }
        public float ClipLength { get; private set; }
        // What a tap does here now: nothing (the preview, no clip), play in place, open the player
        public string Tap { get; private set; } = ElementOptions.TapNone;
        public bool PlaysInPlace { get; private set; }

        private readonly Clickable _tap;
        private ElementBindContext _context;
        private string _posterPath = "";

        public VideoElement()
        {
            ElementStyle.Init(this, BuiltInElements.Video);
            AddToClassList("ts-el-media");
            Teaser = new VisualElement { name = "ts-el-video-teaser" };
            Teaser.AddToClassList("ts-el-video__teaser");
            Poster = new MediaFrame();
            Poster.Root.name = "ts-el-video-poster";
            PlayDisc = new VisualElement { name = "ts-el-video-play", pickingMode = PickingMode.Ignore };
            PlayDisc.AddToClassList("card-video__overlay");
            PlayDisc.AddToClassList("ts-el-video__play");
            var glyph = CardIcons.CreateVector(CardIcons.Shape.Play);
            glyph.Filled = true;
            glyph.pickingMode = PickingMode.Ignore;
            glyph.AddToClassList("card-video__overlay-glyph");
            PlayDisc.Add(glyph);
            Badge = new VisualElement { name = "ts-el-video-badge", pickingMode = PickingMode.Ignore };
            Badge.AddToClassList("ts-el-video__badge");
            BadgeWords = new Label { name = "ts-el-video-badge-words", pickingMode = PickingMode.Ignore };
            BadgeWords.AddToClassList("ts-el-video__badge-words");
            Badge.Add(BadgeWords);
            Poster.Root.Add(PlayDisc);
            Poster.Root.Add(Badge);
            Teaser.Add(Poster.Root);
            Panel = new VideoPanel("ts-el-video__panel");
            Panel.FullScreenRequested += OnFullScreen;
            Words = new MediaCaption();
            Add(Teaser);
            Add(Panel.Root);
            Add(Words.Root);
            _tap = new Clickable(OnTap);
        }

        public void Bind(ElementRow row, ElementBindContext context)
        {
            Unbind();
            _context = context;
            ElementStyle.Apply(this, row, context);
            var block = context.Block;
            var media = row.Media;
            var item = media.Count > 0 ? media[0] : default;
            string clipPath = MediaPathRule.IsValid(item.Asset, MediaKind.Video) ? item.Asset : "";
            // - the clip is loaded here only for its length and proportion; the owner loads its own when it plays
            var clip = clipPath.Length > 0 ? block?.Media?.Load<VideoClip>(clipPath) : null;
            ClipLength = clip != null ? (float)clip.length : 0f;
            string poster = row.Field(BuiltInElements.PosterField)?.asset ?? "";
            _posterPath = MediaPathRule.IsValid(poster, MediaKind.Image) ? poster : "";
            var (caption, credit) = media.Count > 0 ? MediaFrame.WordsOf(item, context) : ("", "");
            string title = caption.Length > 0 ? caption : BlockStackBuilder.CardTitleOf(block?.Poi, context.Language, context.Fallback);
            Track = new VideoTrack(block?.Poi?.id, clipPath, title);

            // - the frame takes the CLIP's proportion (the poster is a frame of it); no poster is an authored choice: a plain frame, never
            //   the "picture unavailable" words
            float clipAspect = clip != null && clip.height > 0 ? (float)clip.width / clip.height : float.NaN;
            if (_posterPath.Length > 0) Poster.Show(block?.Media, _posterPath, block?.Strings, ElementStyle.IsArch(row, context), ElementMediaRule.FrameAspect(clipAspect));
            else
            {
                Poster.Clear();
                Poster.Root.style.aspectRatio = new StyleRatio(ElementMediaRule.FrameAspect(clipAspect));
            }
            string length = ClipLength > 0f ? AudioTimeRule.Format(ClipLength) : block?.Strings?.Get(CardStrings.Keys.VideoUnavailable) ?? "";
            var strings = block?.Strings;
            BadgeWords.text = caption.Length > 0 && length.Length > 0 && strings != null ? string.Format(strings.Get(CardStrings.Keys.MediaBadge), caption, length)
                : caption.Length > 0 ? caption : length;
            Words.Show("", context.Compact ? "" : credit);

            Tap = context.Surface == ElementSurface.Page && ClipLength > 0f && block?.Video != null ? ElementRules.TapOf(row, context.Element) : ElementOptions.TapNone;
            if (Tap == ElementOptions.TapOpenPlayer && block.Host == null) Tap = ElementOptions.TapNone;
            if (Tap != ElementOptions.TapNone) Teaser.AddManipulator(_tap);
            Teaser.EnableInClassList("card-tap", Tap != ElementOptions.TapNone);
            ShowInPlace(false);
        }

        public void Unbind()
        {
            // - leaving the page while the visitor's video plays in place: nothing would be left to show it or stop it
            if (PlaysInPlace) PauseMine();
            Teaser.RemoveManipulator(_tap);
            Panel.Unbind();
            ShowInPlace(false);
            Poster.Clear();
            Words.Show("", "");
            BadgeWords.text = "";
            Tap = ElementOptions.TapNone;
            Track = null;
            ClipLength = 0f;
            _context = null;
        }

        // A tap on the teaser: play in place, or open the player
        private void OnTap()
        {
            var video = _context?.Block?.Video;
            if (video == null || Track == null) return;
            if (Tap == ElementOptions.TapPlayInPlace)
            {
                // - the full-screen button only where a full view can open (a host)
                Panel.Bind(Track, video, _context.Block.Media, _posterPath, ClipLength, null, false, null, _context.Block.Strings,
                    offerFullScreen: _context.Block.Host != null);
                ShowInPlace(true);
                if (!video.IsCurrent(Track) || video.PlayingLoop || video.State != CardVideoState.Playing) video.Toggle(Track);
            }
            else if (Tap == ElementOptions.TapOpenPlayer) OpenPlayer();
        }

        // The in-place player's full-screen button: the same playback goes on in the card's player (it was playing, so it keeps playing there)
        private void OnFullScreen()
        {
            if (PlaysInPlace && Track != null && _context?.Block?.Host != null && _context.Block.Video != null) OpenPlayer();
        }

        // The card's full-screen view with a player over the owner's playback; the video starts once the page is up (opening the view lets
        // this page go first -- this element is unbound by then, so the player keeps its own copy of what it shows -- and a page that leaves
        // pauses what it played)
        private void OpenPlayer()
        {
            var block = _context.Block;
            var video = block.Video;
            var track = Track;
            string posterPath = _posterPath;
            float length = ClipLength;
            var strings = block.Strings;
            block.Host.OpenTakeover(track.Title, 1, 0, (view, container, media, page) =>
            {
                var panel = new VideoPanel("card-video--full");
                container.Add(panel.Root);
                panel.Bind(track, video, media, posterPath, length, null, false, null, strings, false);
                if (!video.IsCurrent(track) || video.PlayingLoop || video.State != CardVideoState.Playing) video.Toggle(track);
                view.WhenPageLeaves(() =>
                {
                    if (video.IsCurrent(track) && !video.PlayingLoop && video.State == CardVideoState.Playing) video.Pause();
                    panel.Unbind();
                });
            });
        }

        private void ShowInPlace(bool inPlace)
        {
            PlaysInPlace = inPlace;
            Teaser.style.display = inPlace ? DisplayStyle.None : DisplayStyle.Flex;
            Panel.Root.style.display = inPlace ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void PauseMine()
        {
            var video = _context?.Block?.Video;
            if (video != null && Track != null && video.IsCurrent(Track) && !video.PlayingLoop && video.State == CardVideoState.Playing) video.Pause();
        }
    }
}
