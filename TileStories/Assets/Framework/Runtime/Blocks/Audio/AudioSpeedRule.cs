using System.Collections.Generic;
using System.Globalization;

namespace TileStories
{
    // The playback speeds an audio_guide offers (_3.1 step 9A), pure. The block's Speeds field names a PRESET; the speed chip cycles
    // through the preset's options. Speed is the AudioSource's pitch: Unity has no time-stretch, so a faster narration is also a
    // higher one (the field's help says so).
    public static class AudioSpeedRule
    {
        public const float Normal = 1f;

        public const string Off = "off";
        public const string Narration = "narration";
        public const string Wide = "wide";

        // The presets in the Editor's order, and the words the Editor shows for them
        public static readonly IReadOnlyList<string> Presets = new[] { Narration, Wide, Off };
        public static readonly IReadOnlyList<string> PresetLabels = new[] { "Narration (0.75x to 1.5x)", "Wide (0.5x to 2x)", "Off (normal speed only)" };

        private static readonly float[] NarrationSpeeds = { 0.75f, 1f, 1.25f, 1.5f };
        private static readonly float[] WideSpeeds = { 0.5f, 0.75f, 1f, 1.25f, 1.5f, 2f };
        private static readonly float[] OffSpeeds = { 1f };

        // The speeds of a preset, slowest first, always containing normal speed; an unknown or empty name is the default preset
        public static IReadOnlyList<float> OptionsOf(string preset) => preset switch
        {
            Wide => WideSpeeds,
            Off => OffSpeeds,
            _ => NarrationSpeeds,
        };

        // Whether the chip is drawn at all: a preset with only normal speed has nothing to choose
        public static bool OffersChoice(string preset) => OptionsOf(preset).Count > 1;

        // The speed after `current` in `options`, wrapping to the first; a current speed the list lacks goes to normal speed
        public static float Next(IReadOnlyList<float> options, float current)
        {
            if (options == null || options.Count == 0) return Normal;
            for (int i = 0; i < options.Count; i++)
                if (System.Math.Abs(options[i] - current) < 0.001f) return options[(i + 1) % options.Count];
            return Normal;
        }

        // "1x", "1.25x", "0.75x": the chip's text (a number and an x, no words)
        public static string Label(float speed) => speed.ToString("0.##", CultureInfo.InvariantCulture) + "x";
    }
}
