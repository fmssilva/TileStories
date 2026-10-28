using UnityEngine;

namespace TileStories.LookKit
{
    // The kit's easing, pure: a CSS cubic-bezier(x1, y1, x2, y2) at progress t (its x monotone: the parameter whose x is t, by bisection, then
    // its y -- over 1 for an overshoot)
    public static class Easing
    {
        public static float CubicBezier(float x1, float y1, float x2, float y2, float t)
        {
            if (t <= 0f) return 0f;
            if (t >= 1f) return 1f;
            float lo = 0f, hi = 1f, s = t;
            for (int i = 0; i < 32; i++)
            {
                s = (lo + hi) * 0.5f;
                if (Bezier(x1, x2, s) < t) lo = s;
                else hi = s;
            }
            return Bezier(y1, y2, s);
        }

        private static float Bezier(float p1, float p2, float s)
        {
            float u = 1f - s;
            return 3f * u * u * s * p1 + 3f * u * s * s * p2 + s * s * s;
        }
    }

    // The Reveal's options (v6's tiles turning in one after the other as their section enters the view): the delay from one piece to the
    // next, each piece's turn (eased with an overshoot) and fade, how far into the view the section must come, and Reduce Motion
    public sealed class RevealOptions
    {
        public float Stagger = 0.045f;
        public float Duration = 0.7f;
        public float FadeDuration = 0.4f;
        // v6's cubic-bezier(.3, 1.35, .5, 1)
        public Vector4 Ease = new(0.3f, 1.35f, 0.5f, 1f);
        // The section has entered once its top is this far above the view's foot
        public float EnterMargin = 30f;
        public bool ReduceMotion;
    }

    // The Reveal, pure: whether a section has entered the view, and each piece's turn and alpha `elapsed` seconds after it did. Reduce Motion
    // = every piece whole at once. A piece turns from its centre line (no 3D in UI Toolkit: a squeeze along x reads as a turn)
    public static class RevealRule
    {
        public static bool Entered(float sectionTop, float scrollOffset, float viewport, float margin) =>
            !float.IsNaN(sectionTop) && sectionTop - scrollOffset < viewport - margin;

        public static float Delay(int index, RevealOptions o) => Mathf.Max(0, index) * o.Stagger;

        // The piece's horizontal scale (0 hidden .. 1 whole; a little past 1 in the overshoot)
        public static float Turn(int index, float elapsed, RevealOptions o)
        {
            if (o.ReduceMotion) return 1f;
            float t = (elapsed - Delay(index, o)) / Mathf.Max(1e-4f, o.Duration);
            return t <= 0f ? 0f : Easing.CubicBezier(o.Ease.x, o.Ease.y, o.Ease.z, o.Ease.w, t);
        }

        // The piece's alpha, eased out (t (2 - t)) and held at 1 once faded in -- the progress is clamped BEFORE the ease: the parabola falls
        // back to 0 past t = 2, which left a finished row with only its last two tiles drawn
        public static float Alpha(int index, float elapsed, RevealOptions o)
        {
            if (o.ReduceMotion) return 1f;
            float t = Mathf.Clamp01((elapsed - Delay(index, o)) / Mathf.Max(1e-4f, o.FadeDuration));
            return t * (2f - t);
        }

        // Seconds until `count` pieces are all whole
        public static float Length(int count, RevealOptions o) =>
            o.ReduceMotion || count <= 0 ? 0f : Delay(count - 1, o) + Mathf.Max(o.Duration, o.FadeDuration);
    }
}
