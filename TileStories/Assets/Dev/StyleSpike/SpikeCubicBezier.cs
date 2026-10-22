using System;

namespace TileStories.Dev.StyleSpike
{
    // DEV-ONLY (_3.3 P0 S6): a CSS cubic-bezier(x1, y1, x2, y2) timing curve as a plain function, for UI Toolkit's
    // experimental.animation (which takes any Func<float, float> easing -- USS transitions only take the named curves).
    public static class SpikeCubicBezier
    {
        // The curve's progress y for time t (0..1): solve x(s) = t for the curve parameter s, then return y(s)
        public static float Evaluate(float x1, float y1, float x2, float y2, float t)
        {
            if (t <= 0f) return 0f;
            if (t >= 1f) return 1f;
            float s = t;
            for (int i = 0; i < 8; i++)                 // Newton: the curve is monotonic in x for 0 <= x1, x2 <= 1
            {
                float dx = Coord(x1, x2, s) - t;
                float d = Slope(x1, x2, s);
                if (Math.Abs(dx) < 1e-5f) break;
                if (Math.Abs(d) < 1e-6f) { s = Bisect(x1, x2, t); break; }
                s -= dx / d;
            }
            return Coord(y1, y2, s);
        }

        // Make an easing delegate for one curve (the Stitch reveal: 0.16, 1, 0.3, 1)
        public static Func<float, float> Curve(float x1, float y1, float x2, float y2) => t => Evaluate(x1, y1, x2, y2, t);

        // One coordinate of the curve from (0,0) to (1,1) with control points a, b, at parameter s
        private static float Coord(float a, float b, float s)
        {
            float u = 1f - s;
            return 3f * u * u * s * a + 3f * u * s * s * b + s * s * s;
        }

        private static float Slope(float a, float b, float s)
        {
            float u = 1f - s;
            return 3f * u * u * a + 6f * u * s * (b - a) + 3f * s * s * (1f - b);
        }

        // The fallback when Newton stalls on a flat stretch
        private static float Bisect(float a, float b, float t)
        {
            float lo = 0f, hi = 1f, s = t;
            for (int i = 0; i < 30; i++)
            {
                s = (lo + hi) * 0.5f;
                if (Coord(a, b, s) < t) lo = s; else hi = s;
            }
            return s;
        }
    }
}
