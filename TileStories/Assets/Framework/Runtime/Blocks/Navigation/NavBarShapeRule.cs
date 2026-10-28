using System.Collections.Generic;
using UnityEngine;

namespace TileStories
{
    // The Navigation Bar's drawn shape (_3.3 S4 third round, v5's renderVals), pure: the ONE path the bar's surface paints, and the motion its
    // raised bump slides with.
    //   - Path: the bar's outline in its own box (x right, y down, panel units): a rounded rectangle whose top edge (at y = room) bends up into a
    //     bump centred on bumpX -- two cubic curves with v5's control points; a bump of 0 is the flat edge, so one path draws every indicator.
    //     The room above the edge is where the bump and its disc rise. At Top the path is mirrored in its box (Mirror): the bump hangs down
    //   - Radius: a floating bar's corner (a stadium, or v5's smaller corner under Raised so the bump has a straight edge to rise from)
    //   - BumpX: the bump's centre for a group's centre, held clear of the corners
    //   - Slide: v5's cubic-bezier(.3, 1.4, .5, 1) over SlideSeconds -- the bump passes its target a little and settles back
    public static class NavBarShapeRule
    {
        // How long the bump (and the disc riding it) takes to slide to the next group
        public const float SlideSeconds = 0.5f;

        public enum Op { Move, Line, Cubic, Quad, Close }

        // One step of the path: Move / Line to P1; Quad through control P1 to P2; Cubic through P1, P2 to P3
        public readonly struct Command
        {
            public readonly Op Op;
            public readonly Vector2 P1, P2, P3;

            public Command(Op op, Vector2 p1 = default, Vector2 p2 = default, Vector2 p3 = default)
            {
                Op = op;
                P1 = p1;
                P2 = p2;
                P3 = p3;
            }
        }

        // The corner of a bar: none at full width; floating, a stadium (half its height) -- or `raisedRadius` under Raised
        public static float Radius(bool floating, bool raised, float height, float raisedRadius) =>
            !floating ? 0f : raised ? Mathf.Min(raisedRadius, height * 0.5f) : height * 0.5f;

        // The bump's centre for a group centred at `centre`: never so near a corner that the bump would climb it
        public static float BumpX(float centre, float width, float radius, float bumpHalf)
        {
            float low = radius + bumpHalf + 1f, high = width - radius - bumpHalf - 1f;
            return low > high ? width * 0.5f : Mathf.Clamp(centre, low, high);
        }

        // The outline of a bar `width` x `height` whose top edge sits `room` down its box, corners `radius`, a bump `bump` high and `bumpHalf`
        // half wide at bumpX (v5's control points: 0.45 and 0.62 of the half width)
        public static List<Command> Path(float width, float height, float room, float radius, float bump, float bumpHalf, float bumpX)
        {
            float a = room, b = room + height, r = Mathf.Clamp(radius, 0f, height * 0.5f), bx = bumpX, bw = bumpHalf, top = a - bump;
            return new List<Command>
            {
                new(Op.Move, new Vector2(r, a)),
                new(Op.Line, new Vector2(bx - bw, a)),
                new(Op.Cubic, new Vector2(bx - bw * 0.45f, a), new Vector2(bx - bw * 0.62f, top), new Vector2(bx, top)),
                new(Op.Cubic, new Vector2(bx + bw * 0.62f, top), new Vector2(bx + bw * 0.45f, a), new Vector2(bx + bw, a)),
                new(Op.Line, new Vector2(width - r, a)),
                new(Op.Quad, new Vector2(width, a), new Vector2(width, a + r)),
                new(Op.Line, new Vector2(width, b - r)),
                new(Op.Quad, new Vector2(width, b), new Vector2(width - r, b)),
                new(Op.Line, new Vector2(r, b)),
                new(Op.Quad, new Vector2(0f, b), new Vector2(0f, b - r)),
                new(Op.Line, new Vector2(0f, a + r)),
                new(Op.Quad, new Vector2(0f, a), new Vector2(r, a)),
                new(Op.Close),
            };
        }

        // The same path upside down in a box `boxHeight` tall (the bar at Top: its bump hangs below its foot)
        public static List<Command> Mirror(List<Command> path, float boxHeight)
        {
            Vector2 Flip(Vector2 p) => new(p.x, boxHeight - p.y);
            var mirrored = new List<Command>(path.Count);
            foreach (var c in path) mirrored.Add(new Command(c.Op, Flip(c.P1), Flip(c.P2), Flip(c.P3)));
            return mirrored;
        }

        // Where the slide is at `t` (0 = its start, 1 = its end; values past 1 lie beyond the target): v5's cubic-bezier(.3, 1.4, .5, 1)
        public static float Slide(float t)
        {
            if (t <= 0f) return 0f;
            if (t >= 1f) return 1f;
            const float x1 = 0.3f, y1 = 1.4f, x2 = 0.5f, y2 = 1f;
            // - the curve's x is monotone (both x controls in 0..1): find the parameter whose x is t by bisection, then read its y
            float lo = 0f, hi = 1f, s = t;
            for (int i = 0; i < 32; i++)
            {
                s = (lo + hi) * 0.5f;
                if (Bezier(x1, x2, s) < t) lo = s;
                else hi = s;
            }
            return Bezier(y1, y2, s);
        }

        // One coordinate of a cubic bezier from (0,0) to (1,1) with controls p1, p2, at parameter s
        private static float Bezier(float p1, float p2, float s)
        {
            float u = 1f - s;
            return 3f * u * u * s * p1 + 3f * u * s * s * p2 + s * s * s;
        }
    }
}
