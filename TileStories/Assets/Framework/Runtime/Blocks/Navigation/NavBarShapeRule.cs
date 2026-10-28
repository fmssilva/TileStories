using System.Collections.Generic;
using UnityEngine;

namespace TileStories
{
    // The Navigation Bar's drawn shape (_3.3 S4 third round, v5's renderVals; P3a review: a FLUID bump), pure: the ONE path the bar's surface
    // paints, and the motion its raised bump slides with.
    //   - Path: the bar's outline in its own box (x right, y down, panel units): a rounded rectangle whose top edge (at y = room) rises into one
    //     smooth bell centred on the chosen group's disc. The bell's crest IS the disc's top arc (its peak exactly at the disc's top, no band of
    //     bar above it); it leaves the disc where the bar has fallen LeaveShare of the bell and lands on the edge in a wide soft shoulder. Every
    //     join is tangent-continuous (C1). A bump of 0 is the flat edge, so one path draws every indicator. At Top the path is mirrored in its
    //     box (Mirror): the bell hangs down
    //   - Radius: a floating bar's corner (a stadium, or v5's smaller corner under Raised so the bell has a straight edge to rise from)
    //   - BumpX: the bell's centre = the disc's, only held so the disc stays inside the bar; near a side the shoulder narrows into the top corner
    //     (the corner gives way only when the shoulder has no room left), so the first and last groups stay smooth
    //   - Slide: v5's cubic-bezier(.3, 1.4, .5, 1) over SlideSeconds -- the bump passes its target a little and settles back
    public static class NavBarShapeRule
    {
        // How long the bump (and the disc riding it) takes to slide to the next group
        public const float SlideSeconds = 0.5f;
        // The share of the bell's height the crest follows the disc's arc down before the shoulder takes over
        public const float LeaveShare = 0.6f;
        // The steepest the bell leaves the disc (from the top of its arc): past it a deep bell would hang on the disc's sides
        public const float MaxLeaveDegrees = 70f;
        // How far the shoulder's foot handle reaches back along the edge, as a share of the room it has: the softness of the landing
        public const float ShoulderEase = 0.55f;

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

            // Where the step ends (Close: nowhere new)
            public Vector2 End => Op == Op.Cubic ? P3 : Op == Op.Quad ? P2 : P1;
        }

        // The corner of a bar: none at full width; floating, a stadium (half its height) -- or `raisedRadius` under Raised
        public static float Radius(bool floating, bool raised, float height, float raisedRadius) =>
            !floating ? 0f : raised ? Mathf.Min(raisedRadius, height * 0.5f) : height * 0.5f;

        // The bell's centre for a disc centred at `centre`: the disc's own, held so the disc stays inside the bar
        public static float BumpX(float centre, float width, float discRadius) =>
            width < discRadius * 2f ? width * 0.5f : Mathf.Clamp(centre, discRadius, width - discRadius);

        // The raised disc's top in the bar's box (bottom bar: y down from the box's top, the edge at `room`): the bell's peak
        public static float DiscTop(float room, float bump) => room - bump;

        // The angle (radians, from the top of the disc) where the bell leaves the disc's arc for its shoulder
        public static float LeaveAngle(float bump, float discRadius)
        {
            if (!(bump > 0f) || !(discRadius > 0f)) return 0f;
            float cos = Mathf.Clamp(1f - LeaveShare * bump / discRadius, Mathf.Cos(MaxLeaveDegrees * Mathf.Deg2Rad), 1f);
            return Mathf.Acos(cos);
        }

        // The outline of a bar `width` x `height` whose top edge sits `room` down its box, corners `radius`, a bell `bump` high and `bumpHalf`
        // half wide round a disc of `discRadius` centred at bumpX
        public static List<Command> Path(float width, float height, float room, float radius, float bump, float bumpHalf, float bumpX, float discRadius)
        {
            float a = room, b = room + height, r = Mathf.Clamp(radius, 0f, height * 0.5f);
            var path = new List<Command>();
            if (!(bump > 0f) || !(discRadius > 0f))
            {
                // - a flat edge: the rounded rectangle alone
                path.Add(new Command(Op.Move, new Vector2(0f, a + r)));
                path.Add(new Command(Op.Quad, new Vector2(0f, a), new Vector2(r, a)));
                path.Add(new Command(Op.Line, new Vector2(width - r, a)));
                path.Add(new Command(Op.Quad, new Vector2(width, a), new Vector2(width, a + r)));
                CloseBelow(path, width, a, b, r, r, r);
                return path;
            }

            float R = discRadius, bx = bumpX, top = a - bump, phi = LeaveAngle(bump, R);
            float sin = Mathf.Sin(phi), cos = Mathf.Cos(phi);
            // - the crest: the disc's arc from its top down to the leave angle, each side one cubic (handle 4/3 tan(phi / 4) R)
            float k = 4f / 3f * Mathf.Tan(phi * 0.25f) * R;
            var peak = new Vector2(bx, top);
            float leaveY = top + R * (1f - cos);
            var leaveL = new Vector2(bx - R * sin, leaveY);
            var leaveR = new Vector2(bx + R * sin, leaveY);
            // - the shoulder leaves along the arc's tangent; its handle reaches down to the edge's height, so it lands flat
            float reach = sin > 0f ? (a - leaveY) / sin : 0f;
            var left = Shoulder(leaveL.x, bumpHalf - R * sin, r, reach, cos);
            var right = Shoulder(width - leaveR.x, bumpHalf - R * sin, r, reach, cos);

            // - the left side's top end, its corner, the edge up to the left foot
            path.Add(new Command(Op.Move, new Vector2(0f, a + left.Corner)));
            path.Add(new Command(Op.Quad, new Vector2(0f, a), new Vector2(left.Corner, a)));
            var footL = new Vector2(leaveL.x - left.Span, a);
            if (footL.x - left.Corner > 1e-3f) path.Add(new Command(Op.Line, footL));
            // - the left shoulder up to the disc, the crest over its top, the right shoulder down
            path.Add(new Command(Op.Cubic, footL + new Vector2(left.Handle, 0f), leaveL + left.Reach * new Vector2(-cos, sin), leaveL));
            path.Add(new Command(Op.Cubic, leaveL + k * new Vector2(cos, -sin), peak - new Vector2(k, 0f), peak));
            path.Add(new Command(Op.Cubic, peak + new Vector2(k, 0f), leaveR - k * new Vector2(cos, sin), leaveR));
            var footR = new Vector2(leaveR.x + right.Span, a);
            path.Add(new Command(Op.Cubic, leaveR + right.Reach * new Vector2(cos, sin), footR - new Vector2(right.Handle, 0f), footR));
            // - the edge on to the right corner, the corner, then the rest of the bar
            if (width - right.Corner - footR.x > 1e-3f) path.Add(new Command(Op.Line, new Vector2(width - right.Corner, a)));
            path.Add(new Command(Op.Quad, new Vector2(width, a), new Vector2(width, a + right.Corner)));
            CloseBelow(path, width, a, b, r, left.Corner, right.Corner);
            return path;
        }

        // One side's shoulder: its span from the leave point to its foot, the foot's handle, the leave handle's length and that side's top corner
        private readonly struct Side
        {
            public readonly float Span, Handle, Reach, Corner;

            public Side(float span, float handle, float reach, float corner)
            {
                Span = span;
                Handle = handle;
                Reach = reach;
                Corner = corner;
            }
        }

        // `room`: from the leave point to the bar's side; `wanted`: the shoulder's own span; `radius`: the corner's own
        private static Side Shoulder(float room, float wanted, float radius, float reach, float cos)
        {
            // - the least span that keeps the shoulder monotone: its leave handle's own horizontal reach
            float least = reach * cos;
            // - the corner keeps its radius while the shoulder still fits beside it; past that it gives way, never the shoulder's smoothness
            float corner = room - radius >= least ? radius : Mathf.Clamp(room - least, 0f, radius);
            float span = Mathf.Max(0f, Mathf.Min(wanted, room - corner));
            // - a disc almost at the side: the leave handle shortens to half the room (still along the arc's tangent), so the foot keeps a
            //   handle of its own and lands flat -- a short shoulder, never a kink
            float reachHere = cos > 0f ? Mathf.Min(reach, 0.5f * span / cos) : reach;
            float handle = Mathf.Max(0f, span - reachHere * cos) * ShoulderEase;
            return new Side(span, handle, reachHere, corner);
        }

        // The bar's right side down, its foot and its left side up to where the path began
        private static void CloseBelow(List<Command> path, float width, float a, float b, float r, float leftCorner, float rightCorner)
        {
            if (b - r > a + rightCorner + 1e-3f) path.Add(new Command(Op.Line, new Vector2(width, b - r)));
            path.Add(new Command(Op.Quad, new Vector2(width, b), new Vector2(width - r, b)));
            path.Add(new Command(Op.Line, new Vector2(r, b)));
            path.Add(new Command(Op.Quad, new Vector2(0f, b), new Vector2(0f, b - r)));
            if (b - r > a + leftCorner + 1e-3f) path.Add(new Command(Op.Line, new Vector2(0f, a + leftCorner)));
            path.Add(new Command(Op.Close));
        }

        // The same path upside down in a box `boxHeight` tall (the bar at Top: its bell hangs below its foot)
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
