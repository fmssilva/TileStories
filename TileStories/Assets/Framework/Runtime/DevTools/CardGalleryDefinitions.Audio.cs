using System.Collections.Generic;

namespace TileStories
{
    // The audio_guide part of the Phase A gallery list (_3.1 step 9A): both looks x (short, long, no captions, no speed chip, a clip that
    // is not there). The clips are silent and the captions plain text made in memory by CardGalleryMedia from the two lists below.
    public static partial class CardGalleryDefinitions
    {
        public const string AudioBlockKey = "block_2";

        // The gallery's clips by name (seconds; the name must be an .mp3 / .wav / .ogg path) and captions by name (WebVTT text). Any other
        // name is a missing file.
        public static readonly IReadOnlyDictionary<string, float> Clips = new Dictionary<string, float>
        {
            ["short.mp3"] = 12f,
            ["long.mp3"] = 615f,
            ["second.mp3"] = 30f,
        };

        // Captions of the short clip: three lines, and a half second with none between the second and the third
        public const string ShortCaptionOne = "The first caption line.";
        public const string ShortCaptionTwo = "The second caption line, a little longer than the first one.";
        public const string ShortCaptionThree = "The last line.";

        // The long clip's lines are long on purpose: they wrap over several lines of the phone-wide card
        public const string LongCaptionOne = "This caption is much longer than a line of the card, so it wraps over two or three lines and the block must grow with it without moving the buttons above.";
        public const string LongCaptionTwo = "And a second long caption for the same clip, one that says a good deal more than anyone can read in a single glance at the screen.";

        public static readonly IReadOnlyDictionary<string, string> Captions = new Dictionary<string, string>
        {
            ["short.vtt"] = "WEBVTT\n\n1\n00:00.000 --> 00:04.000\n" + ShortCaptionOne + "\n\n2\n00:04.000 --> 00:08.000\n" + ShortCaptionTwo
                            + "\n\n3\n00:08.500 --> 00:12.000\n" + ShortCaptionThree + "\n",
            ["long.vtt"] = "WEBVTT\n\n00:00:00.000 --> 00:05:00.000\n" + LongCaptionOne + "\n\n00:05:00.000 --> 00:10:15.000\n" + LongCaptionTwo + "\n",
        };

        public const string AudioTitle = "The castle guide";
        public const string AudioLongTitle = "A very long title for the audio guide of the royal palace before the earthquake, read aloud by the curator of the museum";

        private static void AddAudioGuides(List<Entry> list)
        {
            foreach (var variant in BuiltInBlocks.AudioGuide.Variants)
            {
                list.Add(new Entry(BuiltInBlocks.AudioGuideKind, variant, "short", AudioBlock(variant, "short.mp3", "short.vtt", AudioTitle)));
                list.Add(new Entry(BuiltInBlocks.AudioGuideKind, variant, "long", AudioBlock(variant, "long.mp3", "long.vtt", AudioLongTitle)));
                list.Add(new Entry(BuiltInBlocks.AudioGuideKind, variant, "nocaptions", AudioBlock(variant, "short.mp3", null, AudioTitle)));
                // - the Speeds field says Off: no speed chip, normal speed only
                list.Add(new Entry(BuiltInBlocks.AudioGuideKind, variant, "nospeed", AudioBlock(variant, "short.mp3", "short.vtt", AudioTitle, AudioSpeedRule.Off)));
                // - a clip that is not there: the block says so and stays quiet
                list.Add(new Entry(BuiltInBlocks.AudioGuideKind, variant, "unavailable", AudioBlock(variant, "ghost.mp3", null, AudioTitle)));
                // _3.1 step 13: a default:<key> clip (the Framework's own generated chime) binds and renders exactly like an authored one
                list.Add(new Entry(BuiltInBlocks.AudioGuideKind, variant, "default", AudioBlock(variant, MediaPathRule.PathForDefaultKey("chime"), null, AudioTitle)));
            }
        }

        // An audio_guide block over the gallery's clips: `captions` null leaves the field out, `speeds` null leaves the preset to the default
        public static BlockInstanceData AudioBlock(string variant, string clip, string captions, string title, string speeds = null, bool captionsOn = false)
        {
            var block = new BlockInstanceData { key = AudioBlockKey, kind = BuiltInBlocks.AudioGuideKind, variant = variant };
            block.fields.Add(new BlockFieldValue { key = BuiltInBlocks.AudioGuideClipField, asset = clip });
            if (captions != null) block.fields.Add(new BlockFieldValue { key = BuiltInBlocks.AudioGuideCaptionsField, asset = captions });
            if (title != null) block.fields.Add(Text(BuiltInBlocks.AudioGuideTitleField, title));
            if (speeds != null) block.fields.Add(new BlockFieldValue { key = BuiltInBlocks.AudioGuideSpeedsField, value = speeds });
            if (captionsOn) block.fields.Add(new BlockFieldValue { key = BuiltInBlocks.AudioGuideCaptionsOnField, flag = true });
            return block;
        }
    }
}
