using UnityEngine;

namespace TileStories
{
    // How the hero moves (_3.3 R11 v4, R6 M3, P4.4a), pure -- transform and opacity only (R8 S6), every answer a number the view writes:
    //   - Pan (hero_motion = pan, the default): a picture WIDER than its frame slides across its whole width and back, once every
    //     PanPeriod seconds, eased at both ends -- the whole building shows, almost like a video. A picture no wider than its frame stays
    //   - Zoom (hero_motion = zoom): a slow push-in to ZoomMax and back over the same period
    //   - Parallax: the picture moves at ParallaxFactor of the scroll inside its frame (a window with depth)
    //   - Text: the hero's words rise (TextRiseFactor of the scroll) and fade out over TextFadeDistance as the card scrolls
    //   - Reduce motion: the picture still at its centre, no zoom, no parallax, the words still and whole
    public static class HeroMotionRule
    {
        // The hero_motion choices (BuiltInBlocks.HeaderHeroMotionField)
        public const string Pan = "pan";
        public const string Zoom = "zoom";
        public const string None = "none";

        // One back-and-forth of the pan or the zoom (seconds): slow enough to read the words over it (R11 v4: ~26 s)
        public const float PanPeriod = 26f;
        // The zoom's furthest push-in
        public const float ZoomMax = 1.06f;
        // The picture's share of the scroll inside its frame (R11: ~0.45)
        public const float ParallaxFactor = 0.45f;
        // The words' rise per unit of scroll (up), and the scroll over which they fade out
        public const float TextRiseFactor = 0.18f;
        public const float TextFadeDistance = 300f;

        // Whether the look moves by itself: pan or zoom, and the visitor asked for no reduced motion
        public static bool Moves(string motion, bool reduceMotion) => !reduceMotion && (motion == Pan || motion == Zoom);

        // The share of the way across (0 = start, 1 = the far end) at `time`: there and back once per period, eased in and out
        public static float Phase(float time)
        {
            if (float.IsNaN(time) || float.IsInfinity(time)) return 0f;
            float t = Mathf.Repeat(time, PanPeriod) / PanPeriod;
            float there = t < 0.5f ? t * 2f : 2f - t * 2f;
            // - ease in-out (smoothstep): it slows at both ends, where the eye rests on the building's ends
            return there * there * (3f - 2f * there);
        }

        // The picture's sideways offset (panel units, negative = moved left) inside a frame `overflow` narrower than the picture.
        // Still (none, zoom, reduce motion): its centre
        public static float PanOffset(string motion, bool reduceMotion, float time, float overflow)
        {
            float room = float.IsNaN(overflow) ? 0f : Mathf.Max(0f, overflow);
            if (room <= 0f) return 0f;
            if (motion != Pan || reduceMotion) return -room * 0.5f;
            return -room * Phase(time);
        }

        // The picture's scale (1 = fills the frame): zoom only, pushed in and out over the period; reduce motion keeps 1
        public static float Scale(string motion, bool reduceMotion, float time) =>
            motion == Zoom && !reduceMotion ? 1f + (ZoomMax - 1f) * Phase(time) : 1f;

        // The picture's slide down inside its frame for the stack's scroll (never past the frame); reduce motion: none
        public static float Parallax(float scrollY, float frameHeight, bool reduceMotion)
        {
            if (reduceMotion || float.IsNaN(scrollY)) return 0f;
            float frame = float.IsNaN(frameHeight) ? 0f : Mathf.Max(0f, frameHeight);
            return Mathf.Clamp(scrollY * ParallaxFactor, 0f, frame);
        }

        // The words' rise (panel units, negative = up) and opacity for the stack's scroll; reduce motion: still and whole
        public static float TextRise(float scrollY, bool reduceMotion) =>
            reduceMotion || float.IsNaN(scrollY) ? 0f : -Mathf.Max(0f, scrollY) * TextRiseFactor;

        public static float TextOpacity(float scrollY, bool reduceMotion) =>
            reduceMotion || float.IsNaN(scrollY) ? 1f : Mathf.Clamp01(1f - Mathf.Max(0f, scrollY) / TextFadeDistance);
    }
}
