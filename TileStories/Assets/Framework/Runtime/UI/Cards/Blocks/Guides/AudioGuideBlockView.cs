using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories
{
    // The audio_guide block (_3.1 Tier 4). It never plays anything itself: every button asks the card's ONE audio owner (ICardAudio,
    // BlockBindContext.Audio) and the block draws whatever that owner says, so a player and a hero chip that name the same clip are
    // one audio drawn twice, and the block redraws on the owner's Changed (a state, a seek, the speed, and every tick while playing).
    //   player    -- a round play / pause button, the title and "time / length", the scrubber, a speed chip, a captions switch and the
    //                caption line of the moment (from the clip's .vtt through VttRule)
    //   hero_chip -- the same button and the time and title in one line with a thin progress line, pinned under the card's header
    //                (BlockKindDefinition.PinnedTopVariants) so it plays from the peek stop
    // The clip is loaded through the block's own scope only to know its length; the owner loads it again (through the card's source)
    // when it plays, so it lives on after this block is unbound. A clip that cannot be loaded shows "Audio unavailable" and stays quiet.
    // Only classes here; Guides.uss holds every size and colour.
    public sealed class AudioGuideBlockView : IBlockView
    {
        public VisualElement Root { get; }
        public Button PlayButton { get; }
        public CardIcons.VectorGlyph PlayGlyph { get; }
        public Label Title { get; }
        public Label TimeText { get; }
        public Label Note { get; }
        public AudioScrubber Scrubber { get; }
        public Button SpeedChip { get; }
        public Button CaptionsButton { get; }
        public Label CaptionLine { get; }

        // What the block shows
        public AudioTrack Track { get; private set; }
        public bool ClipAvailable { get; private set; }
        public bool CaptionsShown { get; private set; }
        public IReadOnlyList<VttRule.Cue> Cues { get; private set; } = System.Array.Empty<VttRule.Cue>();
        public string Variant { get; private set; } = "";

        private ICardAudio _audio;
        private CardStrings _strings;
        private IReadOnlyList<float> _speeds = AudioSpeedRule.OptionsOf(null);
        private float _clipLength;
        private string _variantClass;

        public AudioGuideBlockView()
        {
            Root = new VisualElement { name = "card-audio" };
            Root.AddToClassList("card-block");
            Root.AddToClassList("card-audio");

            var top = new VisualElement { pickingMode = PickingMode.Ignore };
            top.AddToClassList("card-audio__top");
            PlayButton = new Button { name = "card-audio-play" };
            PlayButton.AddToClassList("card-audio__play");
            PlayButton.AddToClassList("card-tap");
            PlayGlyph = CardIcons.CreateVector(CardIcons.Shape.Play);
            PlayGlyph.Filled = true;
            PlayGlyph.AddToClassList("card-audio__glyph");
            PlayButton.Add(PlayGlyph);
            PlayButton.clicked += () => _audio?.Toggle(Track);

            var words = new VisualElement { pickingMode = PickingMode.Ignore };
            words.AddToClassList("card-audio__words");
            Title = new Label { pickingMode = PickingMode.Ignore };
            Title.AddToClassList("card-audio__title");
            TimeText = new Label { pickingMode = PickingMode.Ignore };
            TimeText.AddToClassList("card-audio__time");
            Note = new Label { pickingMode = PickingMode.Ignore };
            Note.AddToClassList("card-audio__note");
            words.Add(Title);
            words.Add(TimeText);
            words.Add(Note);
            top.Add(PlayButton);
            top.Add(words);

            Scrubber = new AudioScrubber();
            Scrubber.Scrubbed += fraction =>
            {
                if (_audio != null && _audio.IsCurrent(Track)) _audio.Seek(AudioTimeRule.TimeAt(fraction, _audio.Length));
            };

            var chips = new VisualElement { pickingMode = PickingMode.Ignore };
            chips.AddToClassList("card-audio__chips");
            SpeedChip = new Button { name = "card-audio-speed" };
            SpeedChip.AddToClassList("card-audio__chip");
            SpeedChip.AddToClassList("card-tap");
            SpeedChip.clicked += () => _audio?.SetSpeed(AudioSpeedRule.Next(_speeds, _audio.Speed));
            CaptionsButton = new Button { name = "card-audio-captions" };
            CaptionsButton.AddToClassList("card-audio__chip");
            CaptionsButton.AddToClassList("card-tap");
            CaptionsButton.clicked += () => SetCaptionsShown(!CaptionsShown);
            chips.Add(SpeedChip);
            chips.Add(CaptionsButton);

            CaptionLine = new Label { name = "card-audio-caption-line", pickingMode = PickingMode.Ignore };
            CaptionLine.AddToClassList("card-audio__caption-line");

            Root.Add(top);
            Root.Add(Scrubber.Root);
            Root.Add(chips);
            Root.Add(CaptionLine);
        }

        public void Bind(BlockInstanceData instance, BlockBindContext context)
        {
            Unbind();
            Variant = context.Variant;
            _variantClass = "card-audio--" + Variant;
            Root.AddToClassList(_variantClass);
            _strings = context.Strings;
            _audio = context.Audio;
            var read = new BlockFieldReader(instance, context.Language, context.FallbackLanguage);

            string title = read.Text(BuiltInBlocks.AudioGuideTitleField);
            if (title.Length == 0) title = BlockStackBuilder.CardTitleOf(context.Poi, context.Language, context.FallbackLanguage);
            string clipPath = read.ValidAsset(BuiltInBlocks.AudioGuideClipField, MediaKind.Audio);
            Track = new AudioTrack(context.Poi?.id, clipPath, title);
            // - a live edit rebinds this block over audio that is already playing: the mini-player follows the block's new title
            _audio?.UpdateTitle(Track);
            // - the clip is loaded here only for its length (streamed clips are light); the owner loads its own when it plays
            var clip = clipPath.Length > 0 ? context.Media?.Load<AudioClip>(clipPath) : null;
            ClipAvailable = clip != null;
            _clipLength = clip != null ? clip.length : 0f;

            string captionsPath = read.ValidAsset(BuiltInBlocks.AudioGuideCaptionsField, MediaKind.Captions);
            var captions = captionsPath.Length > 0 ? context.Media?.Load<TextAsset>(captionsPath) : null;
            Cues = captions != null ? VttRule.Parse(captions.text) : System.Array.Empty<VttRule.Cue>();

            string preset = read.Value(BuiltInBlocks.AudioGuideSpeedsField);
            _speeds = AudioSpeedRule.OptionsOf(preset);
            SpeedChip.style.display = AudioSpeedRule.OffersChoice(preset) && ClipAvailable ? DisplayStyle.Flex : DisplayStyle.None;
            CaptionsButton.style.display = Cues.Count > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            CaptionsShown = Cues.Count > 0 && read.Flag(BuiltInBlocks.AudioGuideCaptionsOnField);

            Title.text = title;
            CaptionsButton.text = _strings?.Get(CardStrings.Keys.AudioCaptions) ?? "";
            CaptionsButton.tooltip = CaptionsButton.text;
            SpeedChip.tooltip = _strings?.Get(CardStrings.Keys.AudioSpeed) ?? "";
            Scrubber.Root.tooltip = _strings?.Get(CardStrings.Keys.AudioSeek) ?? "";
            Scrubber.SetInteractive(Variant == BuiltInBlocks.AudioGuidePlayer && ClipAvailable);
            PlayButton.SetEnabled(ClipAvailable);
            if (_audio != null) _audio.Changed += Refresh;
            Refresh();
        }

        public void Unbind()
        {
            if (_audio != null) _audio.Changed -= Refresh;
            _audio = null;
            Track = null;
            Cues = System.Array.Empty<VttRule.Cue>();
            if (_variantClass != null) Root.RemoveFromClassList(_variantClass);
            _variantClass = null;
        }

        // Turn the caption line on or off (the visitor's switch; view state, never stored)
        public void SetCaptionsShown(bool shown)
        {
            CaptionsShown = shown && Cues.Count > 0;
            Refresh();
        }

        // Draw the block as the audio owner says it stands: the button, the time, the bar, the speed, the caption line of the moment
        public void Refresh()
        {
            if (Track == null) return;
            bool current = _audio != null && _audio.IsCurrent(Track);
            bool playing = current && _audio.State == CardAudioState.Playing;
            bool queued = _audio != null && _audio.IsQueued(Track);
            float position = current ? _audio.Position : 0f;
            float length = current && _audio.Length > 0f ? _audio.Length : _clipLength;

            PlayGlyph.Kind = playing ? CardIcons.Shape.Pause : CardIcons.Shape.Play;
            PlayButton.tooltip = _strings?.Get(playing ? CardStrings.Keys.AudioPause : CardStrings.Keys.AudioPlay) ?? "";
            Root.EnableInClassList("card-audio--playing", playing);
            Root.EnableInClassList("card-audio--queued", queued);
            Root.EnableInClassList("card-audio--unavailable", !ClipAvailable);

            TimeText.text = length > 0f ? AudioTimeRule.Format(position) + " / " + AudioTimeRule.Format(length) : AudioTimeRule.Format(position);
            Scrubber.SetFraction(AudioTimeRule.Fraction(position, length));
            SpeedChip.text = AudioSpeedRule.Label(_audio != null ? _audio.Speed : AudioSpeedRule.Normal);

            // - one small note under the title: an audio waiting its turn, or a clip that cannot be played
            string note = !ClipAvailable ? _strings?.Get(CardStrings.Keys.AudioUnavailable) ?? "" : queued ? _strings?.Get(CardStrings.Keys.AudioQueued) ?? "" : "";
            Note.text = note;
            Note.style.display = note.Length > 0 ? DisplayStyle.Flex : DisplayStyle.None;

            CaptionsButton.EnableInClassList("card-audio__chip--on", CaptionsShown);
            CaptionLine.style.display = CaptionsShown && Variant == BuiltInBlocks.AudioGuidePlayer ? DisplayStyle.Flex : DisplayStyle.None;
            CaptionLine.text = CaptionsShown && current ? VttRule.TextAt(Cues, position) : "";
        }
    }
}
