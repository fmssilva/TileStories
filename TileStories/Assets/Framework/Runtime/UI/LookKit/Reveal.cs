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

    // The Reveal's options (v6's tiles turning in one after the other, driven by the row's PLACE in the view, never by time): each piece's
    // share of the row's travel, the share of its own turn it fades in over, its ease (with an overshoot), where the travel ends, and Reduce
    // Motion
    public sealed class RevealOptions
    {
        // The share of the row's travel one piece takes to turn; the rest staggers the pieces, in order (16 tiles: each starts 5 % after
        // the one before)
        public float Window = 0.25f;
        // The share of a piece's own turn over which it fades in (v6: a 0.4 s fade in a 0.7 s turn)
        public float FadeShare = 0.4f / 0.7f;
        // v6's cubic-bezier(.3, 1.35, .5, 1)
        public Vector4 Ease = new(0.3f, 1.35f, 0.5f, 1f);
        // The travel ends once the row is this share of the view up from its foot (a third)
        public float EndShare = 1f / 3f;
        public bool ReduceMotion;
    }

    // The Reveal, pure: how far a row has travelled up the view (0 as it clears the line under it -- a bar floating at the view's foot --, 1
    // once it is EndShare up the view), what it shows (Once: whole stays whole), and each piece's turn and alpha at that progress -- the
    // same function both ways, so scrolling back turns the pieces out again. Reduce Motion = every piece whole. A piece turns from its
    // centre line (no 3D in UI Toolkit: a squeeze along x reads as a turn)
    public static class RevealRule
    {
        // 0 while the row's bottom is at or under `startLine`, 1 at or over `endLine` (both measured down from the view's top); a row or line
        // not laid out (NaN) has not travelled
        public static float Progress(float rowBottom, float startLine, float endLine)
        {
            if (float.IsNaN(rowBottom) || float.IsNaN(startLine) || float.IsNaN(endLine)) return 0f;
            // - a line under the start (a tall bar in a short view) still leaves a one-unit travel: never a division by zero
            return Mathf.Clamp01((startLine - rowBottom) / Mathf.Max(1f, startLine - endLine));
        }

        // Where the travel ends in a view `viewport` high, measured down from its top
        public static float EndLine(float viewport, RevealOptions o) => viewport * (1f - Mathf.Clamp01(o.EndShare));

        // What the row shows next: the progress (Every Pass), or -- Once -- whole for good once it has been whole
        public static float Held(float shown, float progress, bool once) => once && shown >= 1f ? 1f : Mathf.Clamp01(progress);

        // Piece `index`'s own share of its turn at `progress`: the pieces start one after the other, each over Window of the travel, the last
        // ending at 1
        public static float PieceProgress(int index, int count, float progress, RevealOptions o)
        {
            if (o.ReduceMotion) return 1f;
            float window = Mathf.Clamp(o.Window, 1e-3f, 1f);
            float step = count > 1 ? (1f - window) / (count - 1) : 0f;
            return Mathf.Clamp01((progress - Mathf.Clamp(index, 0, Mathf.Max(0, count - 1)) * step) / window);
        }

        // The piece's horizontal scale (0 hidden .. 1 whole; a little past 1 in the overshoot)
        public static float Turn(int index, int count, float progress, RevealOptions o)
        {
            float q = PieceProgress(index, count, progress, o);
            return q <= 0f ? 0f : Easing.CubicBezier(o.Ease.x, o.Ease.y, o.Ease.z, o.Ease.w, q);
        }

        // The piece's alpha, eased out (a (2 - a)) over the first FadeShare of its turn -- clamped BEFORE the ease: the parabola falls back
        // to 0 past a = 2, which once left a finished row with only its last two tiles drawn
        public static float Alpha(int index, int count, float progress, RevealOptions o)
        {
            float a = Mathf.Clamp01(PieceProgress(index, count, progress, o) / Mathf.Max(1e-4f, o.FadeShare));
            return a * (2f - a);
        }
    }
}
