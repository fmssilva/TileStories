using UnityEngine;

namespace TileStories
{
    // How the hero moves (_3.3 R11 v4, R6 M3, P4.4a, S4a), pure -- transform and opacity only (R8 S6), every answer a number the view writes:
    //   - Pan And Zoom (hero_motion = pan_zoom, the default): the pan and the zoom together -- the picture drifts across and slowly pushes
    //     in, and back
    //   - Pan (hero_motion = pan): a picture WIDER than its frame slides across its whole width and back, eased at both ends -- the whole
    //     building shows, almost like a video -- never faster than MaxPanSpeed (PeriodFor: a full-screen hero crops a wide picture a lot,
    //     so its period grows with the way it travels, from PanPeriod up). A picture no wider than its frame stays
    //   - Zoom (hero_motion = zoom): a slow push-in to ZoomMax and back over PanPeriod
    //   - Parallax: the picture moves at ParallaxFactor of the scroll inside its frame (a window with depth)
    //   - Text: the hero's words block (kicker, title, line, Listen and the actions, as one) rises faster than the scroll (TextRiseFactor
    //     more) and fades only near the top of the view: over the TextFadeSpan just under the chrome floating there (_3.3 S4a review)
    //   - Reduce motion: the picture still at its centre, no zoom, no parallax, the words still and whole
    public static class HeroMotionRule
    {
        // The hero_motion choices (BuiltInBlocks.HeaderHeroMotionField)
        public const string PanZoom = "pan_zoom";
        public const string Pan = "pan";
        public const string Zoom = "zoom";
        public const string None = "none";

        // One back-and-forth of the pan or the zoom (seconds) at the least: slow enough to read the words over it (R11 v4: ~26 s)
        public const float PanPeriod = 26f;
        // The fastest the picture slides (panel units per second): a drift, never a sweep (_3.3 S4a)
        public const float MaxPanSpeed = 12f;
        // The zoom's furthest push-in
        public const float ZoomMax = 1.06f;
        // The picture's share of the scroll inside its frame (R11: ~0.45)
        public const float ParallaxFactor = 0.45f;
        // The words' extra rise per unit of scroll (up: they outrun the picture), and the height above the chrome's foot over which they fade
        public const float TextRiseFactor = 0.25f;
        public const float TextFadeSpan = 140f;

        // Whether the look moves by itself: pan, zoom or both, and the visitor asked for no reduced motion
        public static bool Moves(string motion, bool reduceMotion) => !reduceMotion && (motion == Pan || motion == Zoom || motion == PanZoom);

        private static bool Pans(string motion) => motion == Pan || motion == PanZoom;
        private static bool Zooms(string motion) => motion == Zoom || motion == PanZoom;

        // One back-and-forth of a picture `overflow` wider than its frame: PanPeriod, or longer so it never slides faster than MaxPanSpeed
        public static float PeriodFor(float overflow)
        {
            float room = float.IsNaN(overflow) ? 0f : Mathf.Max(0f, overflow);
            return Mathf.Max(PanPeriod, 2f * room / MaxPanSpeed);
        }

        // The share of the way across (0 = start, 1 = the far end) at `time`: there and back once per PanPeriod, eased in and out
        public static float Phase(float time) => Phase(time, PanPeriod);

        // The same over `period` seconds
        public static float Phase(float time, float period)
        {
            if (float.IsNaN(time) || float.IsInfinity(time) || !(period > 0f)) return 0f;
            float t = Mathf.Repeat(time, period) / period;
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
            if (!Pans(motion) || reduceMotion) return -room * 0.5f;
            return -room * Phase(time, PeriodFor(room));
        }

        // The picture's scale (1 = fills the frame): zoom and pan-and-zoom, pushed in and out over the period (pan-and-zoom: the pan's own,
        // so the push-in rides the drift); reduce motion keeps 1
        public static float Scale(string motion, bool reduceMotion, float time, float overflow = 0f) =>
            Zooms(motion) && !reduceMotion
                ? 1f + (ZoomMax - 1f) * Phase(time, motion == PanZoom ? PeriodFor(overflow) : PanPeriod)
                : 1f;

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

        // `wordsTop`: the words block's top as drawn, from the view's top; `fadeTop`: the foot of the chrome floating over the view. Whole
        // until the block comes within TextFadeSpan of that foot, gone as it reaches it
        public static float TextOpacity(float wordsTop, float fadeTop, bool reduceMotion)
        {
            if (reduceMotion || float.IsNaN(wordsTop)) return 1f;
            float above = wordsTop - (float.IsNaN(fadeTop) ? 0f : Mathf.Max(0f, fadeTop));
            return Mathf.Clamp01(above / TextFadeSpan);
        }
    }
}
