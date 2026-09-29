using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Video;

namespace TileStories
{
    // The video block (_3.1 Tier 4, step 9B). It never plays anything itself: the card's ONE video owner (BlockBindContext.Video) does, and the
    // block draws that playback through a VideoPanel. How it sits in the stack is the instance's display (BlockDisplayRule):
    //   inline   -- the whole player on the card (inline look: poster, play, bar, captions, full screen; chapters look: + chapter buttons)
    //   takeover -- a TEASER on the card: the poster, a play button and the length; a tap starts the video and opens it full screen
    // Full screen is the card's TakeoverView (IBlockHost.OpenTakeover) with a second panel over the SAME playback: nothing restarts, and Back
    // returns to the card at the stop and scroll it had. The clip is loaded through the block's own scope only for its length; the owner
    // loads its own through the card's source when it plays. Only classes here; Media.uss holds every size and colour.
    public sealed class VideoBlockView : IBlockView
    {
        public VisualElement Root { get; }
        public VideoPanel Panel { get; }
        // The takeover display's teaser (hidden in the inline display)
        public VisualElement Teaser { get; }
        public CardImage TeaserPoster { get; }
        public Button TeaserButton { get; }
        public Label TeaserLength { get; }
        // The panel drawn full screen while the takeover is open (null otherwise)
        public VideoPanel FullScreenPanel { get; private set; }

        public string Variant { get; private set; } = "";
        public string Display { get; private set; } = CardOptions.DisplayInline;
        public VideoTrack Track => _track;

        private BlockBindContext _context;
        private VideoTrack _track;
        private string _posterPath = "";
        private float _clipLength;
        private IReadOnlyList<VttRule.Cue> _cues = System.Array.Empty<VttRule.Cue>();
        private IReadOnlyList<ChapterRule.Chapter> _chapters = System.Array.Empty<ChapterRule.Chapter>();
        private bool _captionsOn;
        private string _title = "";

        public VideoBlockView()
        {
            Root = new VisualElement { name = "card-video-block" };
            Root.AddToClassList("card-block");
            Root.AddToClassList("card-video-block");

            Panel = new VideoPanel();
            Panel.FullScreenRequested += OpenFullScreen;

            Teaser = new VisualElement { name = "card-video-teaser" };
            Teaser.AddToClassList("card-video__surface");
            Teaser.AddToClassList("card-video-teaser");
            TeaserPoster = new CardImage("card-video__poster");
            TeaserButton = new Button { name = "card-video-teaser-open" };
            TeaserButton.AddToClassList("card-video__overlay");
            var glyph = CardIcons.CreateVector(CardIcons.Shape.Play);
            glyph.Filled = true;
            glyph.AddToClassList("card-video__overlay-glyph");
            TeaserButton.Add(glyph);
            TeaserButton.clicked += PlayFullScreen;
            TeaserLength = new Label { pickingMode = PickingMode.Ignore };
            TeaserLength.AddToClassList("card-video__length");
            Teaser.Add(TeaserPoster.Root);
            Teaser.Add(TeaserButton);
            Teaser.Add(TeaserLength);

            Root.Add(Panel.Root);
            Root.Add(Teaser);
        }

        public void Bind(BlockInstanceData instance, BlockBindContext context)
        {
            Unbind();
            _context = context;
            Variant = context.Variant;
            Display = BlockDisplayRule.Resolve(BuiltInBlocks.Video, instance?.display);
            var read = new BlockFieldReader(instance, context.Language, context.FallbackLanguage);

            _title = read.Text(BuiltInBlocks.VideoTitleField);
            if (_title.Length == 0) _title = BlockStackBuilder.CardTitleOf(context.Poi, context.Language, context.FallbackLanguage);
            string clipPath = read.ValidAsset(BuiltInBlocks.VideoClipField, MediaKind.Video);
            _track = new VideoTrack(context.Poi?.id, clipPath, _title);
            // - the clip is loaded here only for its length; the owner loads its own when it plays
            var clip = clipPath.Length > 0 ? context.Media?.Load<VideoClip>(clipPath) : null;
            _clipLength = clip != null ? (float)clip.length : 0f;
            _posterPath = read.ValidAsset(BuiltInBlocks.VideoPosterField, MediaKind.Image);

            string captionsPath = read.ValidAsset(BuiltInBlocks.VideoCaptionsField, MediaKind.Captions);
            var captions = captionsPath.Length > 0 ? context.Media?.Load<TextAsset>(captionsPath) : null;
            _cues = captions != null ? VttRule.Parse(captions.text) : System.Array.Empty<VttRule.Cue>();
            _captionsOn = read.Flag(BuiltInBlocks.VideoCaptionsOnField);
            _chapters = Variant == BuiltInBlocks.VideoChapters ? ChaptersOf(read, _clipLength) : System.Array.Empty<ChapterRule.Chapter>();

            bool teaser = Display == CardOptions.DisplayTakeover;
            Root.EnableInClassList("card-video-block--takeover", teaser);
            Panel.Root.style.display = teaser ? DisplayStyle.None : DisplayStyle.Flex;
            Teaser.style.display = teaser ? DisplayStyle.Flex : DisplayStyle.None;
            if (teaser)
            {
                // - no poster is an authored choice: a plain frame with the play button, never the "picture unavailable" words
                if (_posterPath.Length > 0) TeaserPoster.Show(context.Media, _posterPath, context.Strings);
                else TeaserPoster.Clear(null);
                TeaserPoster.Root.style.display = _posterPath.Length > 0 ? DisplayStyle.Flex : DisplayStyle.None;
                TeaserLength.text = _clipLength > 0f ? AudioTimeRule.Format(_clipLength) : context.Strings?.Get(CardStrings.Keys.VideoUnavailable) ?? "";
                TeaserButton.tooltip = context.Strings?.Get(CardStrings.Keys.VideoFullScreen) ?? "";
                TeaserButton.SetEnabled(_clipLength > 0f);
            }
            else Panel.Bind(_track, context.Video, context.Media, _posterPath, _clipLength, _cues, _captionsOn, _chapters, context.Strings, true);
        }

        public void Unbind()
        {
            Panel.Unbind();
            FullScreenPanel?.Unbind();
            FullScreenPanel = null;
            TeaserPoster.Clear(null);
            _context = null;
            _track = null;
        }

        // The chapter rows of the block as ChapterRule keeps them (complete, readable, inside the clip, in time order)
        private static IReadOnlyList<ChapterRule.Chapter> ChaptersOf(BlockFieldReader read, float length)
        {
            var rows = new List<(string, string)>();
            foreach (var item in read.Items(BuiltInBlocks.VideoChaptersField))
                rows.Add((read.ItemValue(item, BuiltInBlocks.VideoChapterTimeField), read.ItemText(item, BuiltInBlocks.VideoChapterLabelField)));
            return ChapterRule.From(rows, length);
        }

        // The teaser's tap: the video starts (or resumes) and opens full screen
        private void PlayFullScreen()
        {
            var video = _context?.Video;
            if (video == null || _track == null || _clipLength <= 0f) return;
            if (!video.IsCurrent(_track) || video.PlayingLoop || video.State != CardVideoState.Playing) video.Toggle(_track);
            OpenFullScreen();
        }

        // Open the full-screen page: a second panel over the same playback, released when the page goes away
        private void OpenFullScreen()
        {
            if (_context?.Host == null || _track == null) return;
            var context = _context;
            context.Host.OpenTakeover(_title, 1, 0, (view, container, media, page) =>
            {
                var panel = new VideoPanel("card-video--full");
                container.Add(panel.Root);
                // - full screen keeps the visitor's captions choice of the inline player (the teaser has none: the block's default)
                bool captions = Display == CardOptions.DisplayTakeover ? _captionsOn : Panel.CaptionsShown;
                panel.Bind(_track, context.Video, media, _posterPath, _clipLength, _cues, captions, _chapters, context.Strings, false);
                FullScreenPanel = panel;
                // - Back clears the page: the panel stops listening to the owner the moment it leaves the screen
                panel.Root.RegisterCallback<DetachFromPanelEvent>(_ =>
                {
                    panel.Unbind();
                    if (FullScreenPanel == panel) FullScreenPanel = null;
                });
            });
        }
    }
}
