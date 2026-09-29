using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories
{
    // One video player drawn on the card (_3.1 step 9B), shared by the video block (inline) and its full-screen page, so both draw the ONE
    // playback the same way. It plays nothing itself: every control asks the card's video owner (ICardVideo) and the panel draws what the owner
    // says -- the owner's picture as the surface's background while a frame of THIS video is there, else the poster -- and redraws on Changed.
    //   surface   -- the poster (a CardImage), the video's frames over it, a big play button in the middle while the video is not playing
    //   controls  -- play / pause, "time / length", the captions switch, the full-screen button (the block's only; the page has none)
    //   scrubber  -- the bar to drag through the video (AudioScrubber, the audio guide's)
    //   captions  -- the caption line of the moment (VttRule), while switched on
    //   chapters  -- a button per chapter (ChapterRule) that jumps to it, the one playing lit (the Chapters look)
    // Only classes here; Media.uss (and Guides.uss for the shared scrubber and chips) hold every size and colour.
    public sealed class VideoPanel
    {
        public VisualElement Root { get; }
        public VisualElement Surface { get; }
        public CardImage Poster { get; }
        public VisualElement Frames { get; }
        public Button PlayOverlay { get; }
        public Button PlayButton { get; }
        public CardIcons.VectorGlyph PlayGlyph { get; }
        public Label TimeText { get; }
        public Label Note { get; }
        public Button CaptionsButton { get; }
        public Button FullScreenButton { get; }
        public AudioScrubber Scrubber { get; }
        public Label CaptionLine { get; }
        public VisualElement ChapterRow { get; }
        public IReadOnlyList<Button> ChapterButtons => _chapterButtons;

        public VideoTrack Track { get; private set; }
        public bool ClipAvailable { get; private set; }
        public bool CaptionsShown { get; private set; }
        public IReadOnlyList<VttRule.Cue> Cues { get; private set; } = Array.Empty<VttRule.Cue>();
        public IReadOnlyList<ChapterRule.Chapter> Chapters { get; private set; } = Array.Empty<ChapterRule.Chapter>();
        // The surface shows the owner's frames now (not the poster)
        public bool ShowsFrames { get; private set; }

        // The full-screen button was tapped (the block opens the full-screen page)
        public event Action FullScreenRequested;

        private readonly List<Button> _chapterButtons = new();
        private ICardVideo _video;
        private CardStrings _strings;
        private float _clipLength;

        // `modifierClass`: the panel's place ("card-video--full" for the full-screen page)
        public VideoPanel(string modifierClass = null)
        {
            Root = new VisualElement { name = "card-video" };
            Root.AddToClassList("card-video");
            if (modifierClass != null) Root.AddToClassList(modifierClass);

            Surface = new VisualElement { name = "card-video-surface" };
            Surface.AddToClassList("card-video__surface");
            Poster = new CardImage("card-video__poster");
            Frames = new VisualElement { name = "card-video-frames", pickingMode = PickingMode.Ignore };
            Frames.AddToClassList("card-video__frames");
            PlayOverlay = new Button { name = "card-video-play-overlay" };
            PlayOverlay.AddToClassList("card-video__overlay");
            var overlayGlyph = CardIcons.CreateVector(CardIcons.Shape.Play);
            overlayGlyph.Filled = true;
            overlayGlyph.AddToClassList("card-video__overlay-glyph");
            PlayOverlay.Add(overlayGlyph);
            PlayOverlay.clicked += Toggle;
            Surface.Add(Poster.Root);
            Surface.Add(Frames);
            Surface.Add(PlayOverlay);

            var controls = new VisualElement { pickingMode = PickingMode.Ignore };
            controls.AddToClassList("card-video__controls");
            PlayButton = new Button { name = "card-video-play" };
            PlayButton.AddToClassList("card-audio__play");
            PlayButton.AddToClassList("card-tap");
            PlayGlyph = CardIcons.CreateVector(CardIcons.Shape.Play);
            PlayGlyph.Filled = true;
            PlayGlyph.AddToClassList("card-audio__glyph");
            PlayButton.Add(PlayGlyph);
            PlayButton.clicked += Toggle;
            var words = new VisualElement { pickingMode = PickingMode.Ignore };
            words.AddToClassList("card-video__words");
            TimeText = new Label { pickingMode = PickingMode.Ignore };
            TimeText.AddToClassList("card-audio__time");
            Note = new Label { pickingMode = PickingMode.Ignore };
            Note.AddToClassList("card-audio__note");
            words.Add(TimeText);
            words.Add(Note);
            CaptionsButton = new Button { name = "card-video-captions" };
            CaptionsButton.AddToClassList("card-audio__chip");
            CaptionsButton.AddToClassList("card-tap");
            CaptionsButton.clicked += () => SetCaptionsShown(!CaptionsShown);
            FullScreenButton = new Button { name = "card-video-full-screen" };
            FullScreenButton.AddToClassList("card-video__full-screen");
            FullScreenButton.AddToClassList("card-tap");
            var fullGlyph = CardIcons.CreateVector(CardIcons.Shape.FullScreen);
            fullGlyph.AddToClassList("card-video__full-screen-glyph");
            FullScreenButton.Add(fullGlyph);
            FullScreenButton.clicked += () => FullScreenRequested?.Invoke();
            controls.Add(PlayButton);
            controls.Add(words);
            controls.Add(CaptionsButton);
            controls.Add(FullScreenButton);

            Scrubber = new AudioScrubber();
            Scrubber.Scrubbed += fraction =>
            {
                if (IsMine()) _video.Seek(AudioTimeRule.TimeAt(fraction, _video.Length));
            };

            CaptionLine = new Label { name = "card-video-caption-line", pickingMode = PickingMode.Ignore };
            CaptionLine.AddToClassList("card-audio__caption-line");

            ChapterRow = new VisualElement { name = "card-video-chapters" };
            ChapterRow.AddToClassList("card-video__chapters");

            Root.Add(Surface);
            Root.Add(controls);
            Root.Add(Scrubber.Root);
            Root.Add(CaptionLine);
            Root.Add(ChapterRow);
        }

        // Show `track` through `video`: the poster from `media`, the clip's length (to draw before it plays), its captions and chapters
        public void Bind(VideoTrack track, ICardVideo video, IMediaSource media, string posterPath, float clipLength, IReadOnlyList<VttRule.Cue> cues,
            bool captionsOn, IReadOnlyList<ChapterRule.Chapter> chapters, CardStrings strings, bool offerFullScreen)
        {
            Unbind();
            Track = track;
            _video = video;
            _strings = strings;
            _clipLength = clipLength;
            ClipAvailable = clipLength > 0f;
            Cues = cues ?? Array.Empty<VttRule.Cue>();
            Chapters = chapters ?? Array.Empty<ChapterRule.Chapter>();
            CaptionsShown = captionsOn && Cues.Count > 0;

            // - no poster is an authored choice, not a missing file: a plain frame, never the "picture unavailable" words
            if (posterPath.Length > 0) Poster.Show(media, posterPath, strings);
            else Poster.Clear(null);
            Root.EnableInClassList("card-video--no-poster", posterPath.Length == 0);

            CaptionsButton.style.display = Cues.Count > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            CaptionsButton.text = strings?.Get(CardStrings.Keys.AudioCaptions) ?? "";
            CaptionsButton.tooltip = CaptionsButton.text;
            FullScreenButton.style.display = offerFullScreen && ClipAvailable ? DisplayStyle.Flex : DisplayStyle.None;
            FullScreenButton.tooltip = strings?.Get(CardStrings.Keys.VideoFullScreen) ?? "";
            Scrubber.Root.tooltip = strings?.Get(CardStrings.Keys.AudioSeek) ?? "";
            Scrubber.SetInteractive(ClipAvailable);
            PlayButton.SetEnabled(ClipAvailable);
            PlayOverlay.SetEnabled(ClipAvailable);

            ChapterRow.Clear();
            _chapterButtons.Clear();
            ChapterRow.tooltip = strings?.Get(CardStrings.Keys.VideoChapters) ?? "";
            foreach (var chapter in Chapters)
            {
                float start = chapter.Start;
                var button = new Button(() => JumpTo(start)) { text = AudioTimeRule.Format(start) + "  " + chapter.Label };
                button.AddToClassList("card-video__chapter");
                button.AddToClassList("card-tap");
                _chapterButtons.Add(button);
                ChapterRow.Add(button);
            }
            ChapterRow.style.display = Chapters.Count > 0 ? DisplayStyle.Flex : DisplayStyle.None;

            if (_video != null) _video.Changed += Refresh;
            Refresh();
        }

        public void Unbind()
        {
            if (_video != null) _video.Changed -= Refresh;
            _video = null;
            Track = null;
            Frames.style.backgroundImage = StyleKeyword.Null;
            ShowsFrames = false;
        }

        // Turn the caption line on or off (the visitor's switch; view state, never stored)
        public void SetCaptionsShown(bool shown)
        {
            CaptionsShown = shown && Cues.Count > 0;
            Refresh();
        }

        // Draw the panel as the video owner says it stands
        public void Refresh()
        {
            if (Track == null) return;
            bool mine = IsMine();
            bool playing = mine && _video.State == CardVideoState.Playing;
            float position = mine ? _video.Position : 0f;
            float length = mine && _video.Length > 0f ? _video.Length : _clipLength;

            ShowsFrames = mine && _video.HasFrame && _video.Texture is RenderTexture;
            Frames.style.backgroundImage = ShowsFrames ? new StyleBackground(Background.FromRenderTexture((RenderTexture)_video.Texture)) : StyleKeyword.Null;
            // - while frames show, the poster steps back: a video of another shape gets plain bars around it, never the poster's edges
            Root.EnableInClassList("card-video--frames", ShowsFrames);
            // - the texture's pixels change every frame without UI Toolkit knowing: ask for a repaint
            if (ShowsFrames) Frames.MarkDirtyRepaint();

            PlayGlyph.Kind = playing ? CardIcons.Shape.Pause : CardIcons.Shape.Play;
            PlayButton.tooltip = _strings?.Get(playing ? CardStrings.Keys.AudioPause : CardStrings.Keys.AudioPlay) ?? "";
            PlayOverlay.tooltip = _strings?.Get(CardStrings.Keys.AudioPlay) ?? "";
            PlayOverlay.style.display = playing || !ClipAvailable ? DisplayStyle.None : DisplayStyle.Flex;
            Root.EnableInClassList("card-video--playing", playing);
            Root.EnableInClassList("card-video--unavailable", !ClipAvailable);

            TimeText.text = length > 0f ? AudioTimeRule.Format(position) + " / " + AudioTimeRule.Format(length) : AudioTimeRule.Format(position);
            Scrubber.SetFraction(AudioTimeRule.Fraction(position, length));
            Note.text = ClipAvailable ? "" : _strings?.Get(CardStrings.Keys.VideoUnavailable) ?? "";
            Note.style.display = Note.text.Length > 0 ? DisplayStyle.Flex : DisplayStyle.None;

            CaptionsButton.EnableInClassList("card-audio__chip--on", CaptionsShown);
            CaptionLine.style.display = CaptionsShown ? DisplayStyle.Flex : DisplayStyle.None;
            CaptionLine.text = CaptionsShown && mine ? VttRule.TextAt(Cues, position) : "";

            int lit = mine ? ChapterRule.IndexAt(Chapters, position) : -1;
            for (int i = 0; i < _chapterButtons.Count; i++) _chapterButtons[i].EnableInClassList("card-video__chapter--current", i == lit);
        }

        // The visitor's video holds the player and it is this panel's (a header loop of the same clip is not)
        private bool IsMine() => _video != null && Track != null && _video.IsCurrent(Track) && !_video.PlayingLoop;

        private void Toggle() => _video?.Toggle(Track);

        // A chapter button: play this video if it is not the one playing (or resume it), then jump to the chapter
        private void JumpTo(float start)
        {
            if (_video == null || !ClipAvailable) return;
            if (!IsMine()) _video.Toggle(Track);
            else if (_video.State == CardVideoState.Paused) _video.Resume();
            _video.Seek(start);
        }
    }
}
