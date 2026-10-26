using UnityEngine;

namespace TileStories
{
    // How the Open Row moves (_3.3 R14.8, the prototype's affordance row), pure. Plain: still. Nudge: the chevron steps towards the page twice
    // and rests (the prototype's 2.6 s keyframes: still to 60 %, out 6 pt at 70 %, back at 80 %, out 4 pt at 90 %). Shine: the same nudge,
    // and a light crossing the words once every 3.4 s -- a band a quarter of the words wide whose brightness peaks at its centre; a glyph
    // under it takes the shine colour by that much. Reduce Motion: always plain (R17.3).
    public static class OpenRowMotionRule
    {
        public const float NudgePeriod = 2.6f;
        public const float NudgeFar = 6f;
        public const float NudgeNear = 4f;
        public const float ShinePeriod = 3.4f;
        // The band's half-width as a share of the words' width, and how far past the words it starts and ends (so it enters and leaves)
        public const float ShineHalfWidth = 0.125f;

        // The emphasis drawn: the block's (or the wall's) emphasis, plain under Reduce Motion
        public static string Effective(string emphasis, bool reduceMotion) =>
            reduceMotion ? ElementOptions.EmphasisPlain : ElementOptions.OneOf(emphasis, ElementOptions.OpenRowEmphases);

        public static bool Nudges(string effective) => effective == ElementOptions.EmphasisNudge || effective == ElementOptions.EmphasisShine;

        public static bool Shines(string effective) => effective == ElementOptions.EmphasisShine;

        // The chevron's offset towards the page at `time` seconds, in pt
        public static float NudgeOffset(float time)
        {
            float p = Mathf.Repeat(time, NudgePeriod) / NudgePeriod;
            if (p <= 0.6f) return 0f;
            if (p <= 0.7f) return Mathf.Lerp(0f, NudgeFar, (p - 0.6f) / 0.1f);
            if (p <= 0.8f) return Mathf.Lerp(NudgeFar, 0f, (p - 0.7f) / 0.1f);
            if (p <= 0.9f) return Mathf.Lerp(0f, NudgeNear, (p - 0.8f) / 0.1f);
            return Mathf.Lerp(NudgeNear, 0f, (p - 0.9f) / 0.1f);
        }

        // Where the shine band's centre is at `time`, as a share of the words' width: it sweeps from before the first glyph to past the last
        public static float ShineCentre(float time)
        {
            float p = Mathf.Repeat(time, ShinePeriod) / ShinePeriod;
            return Mathf.Lerp(-ShineHalfWidth, 1f + ShineHalfWidth, p);
        }

        // How much a glyph at `x` (a share of the words' width) takes the shine colour while the band's centre is at `centre`: 1 at the
        // centre, easing to 0 at the band's edge
        public static float ShineAmount(float x, float centre)
        {
            float d = Mathf.Abs(x - centre) / ShineHalfWidth;
            return d >= 1f ? 0f : Mathf.SmoothStep(1f, 0f, d);
        }
    }
}
