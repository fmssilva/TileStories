using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // Tier 0 of the Navigation Bar's drawn shape (NavBarShapeRule; _3.3 P3a review a: the Raised bump is FLUID): the bell's peak exactly on
    // the disc's top (+-0.5), its crest the disc's own arc (no band of bar above the disc), the tangent continuous at every join (no corner
    // anywhere), each side monotonic, wide soft shoulders, the first and last groups as smooth as a middle one, the flat edge without Raised,
    // the corners per shape, the mirror at Top (the bell hangs down) and the slide's overshoot. The numbers are the card's, read from
    // CardTokens.uss (never retyped), at the phone's frame: a floating bar 358 wide.
    public class NavBarShapeRuleTests
    {
        private const float W = 358f;
        // One drawn path's tolerance: half a pixel (the brief's +-0.5) for positions; half a degree for a tangent's turn at a join
        private const float HalfPixel = 0.5f, SmoothDegrees = 0.5f;

        private static float Token(string name)
        {
            string tokens = System.IO.File.ReadAllText("Assets/Framework/Runtime/UI/Screen/Cards/CardTokens.uss");
            var m = System.Text.RegularExpressions.Regex.Match(tokens, "--ts-navbar-" + name + @":\s*([0-9.]+)");
            Assert.IsTrue(m.Success, "the token --ts-navbar-" + name);
            return float.Parse(m.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);
        }

        private static float Room => Token("room");
        private static float H => Token("height");
        private static float Bump => NavBarShapeRule.BellHeight(Disc);
        private static float Half => Token("bump-half");
        private static float Disc => Token("disc-raised") * 0.5f;
        private static float Corner => NavBarShapeRule.Radius(true, true, H, Token("raised-radius"));

        // The six default groups on the bar's 10 pt padded row: their centres
        private static float GroupCentre(int i) => 10f + (W - 20f) / 6f * (i + 0.5f);

        private static List<NavBarShapeRule.Command> Bell(float x) =>
            NavBarShapeRule.Path(W, H, Room, Corner, Bump, Half, NavBarShapeRule.BumpX(x, W, Disc), Disc);

        // The path as its drawn pieces: each a list of control points from its start (lines 2, quads 3, cubics 4)
        private static List<Vector2[]> Pieces(List<NavBarShapeRule.Command> path)
        {
            var pieces = new List<Vector2[]>();
            var at = path[0].P1;
            foreach (var c in path.Skip(1))
            {
                switch (c.Op)
                {
                    case NavBarShapeRule.Op.Line: pieces.Add(new[] { at, c.P1 }); break;
                    case NavBarShapeRule.Op.Quad: pieces.Add(new[] { at, c.P1, c.P2 }); break;
                    case NavBarShapeRule.Op.Cubic: pieces.Add(new[] { at, c.P1, c.P2, c.P3 }); break;
                    case NavBarShapeRule.Op.Close: if (Vector2.Distance(at, path[0].P1) > 1e-3f) pieces.Add(new[] { at, path[0].P1 }); continue;
                }
                at = c.End;
            }
            return pieces;
        }

        private static Vector2 At(Vector2[] p, float t)
        {
            float u = 1f - t;
            return p.Length == 2 ? u * p[0] + t * p[1]
                : p.Length == 3 ? u * u * p[0] + 2f * u * t * p[1] + t * t * p[2]
                : u * u * u * p[0] + 3f * u * u * t * p[1] + 3f * u * t * t * p[2] + t * t * t * p[3];
        }

        // The direction a piece leaves its start / arrives at its end (the first control point that is not on the end itself)
        private static Vector2 Leaving(Vector2[] p) => (p.Skip(1).First(q => Vector2.Distance(q, p[0]) > 1e-5f) - p[0]).normalized;
        private static Vector2 Arriving(Vector2[] p) => (p[p.Length - 1] - p.Take(p.Length - 1).Reverse().First(q => Vector2.Distance(q, p[p.Length - 1]) > 1e-5f)).normalized;

        private static List<Vector2> Samples(List<NavBarShapeRule.Command> path, int perPiece = 100) =>
            Pieces(path).SelectMany(p => Enumerable.Range(0, perPiece + 1).Select(i => At(p, i / (float)perPiece))).ToList();

        [Test]
        public void TheBellsPeak_IsExactlyTheDiscsTop_OverTheDiscsCentre_ForEveryGroup()
        {
            float discTop = NavBarShapeRule.DiscTop(Room, Bump);
            Assert.GreaterOrEqual(discTop, 0f, "precondition: the room holds the disc");
            for (int g = 0; g < 6; g++)
            {
                float x = GroupCentre(g);
                var points = Samples(Bell(x));
                var peak = points.OrderBy(p => p.y).First();
                Assert.AreEqual(discTop, peak.y, HalfPixel, "group " + g + ": the highest point is the disc's top -- no band of bar above it");
                Assert.AreEqual(x, peak.x, HalfPixel, "group " + g + ": ...over the disc's centre");
                Assert.AreEqual(Room + H, points.Max(p => p.y), 1e-3f, "the bar's foot");
                Assert.AreEqual(0f, points.Min(p => p.x), 1e-3f);
                Assert.AreEqual(W, points.Max(p => p.x), 1e-3f);
            }
        }

        // The crest IS the disc's arc: above the edge the outline never cuts into the disc, and near the peak it runs ON the disc's outline
        // (the bar hugs the disc's top and leaves it only as the bell falls)
        [Test]
        public void TheCrest_RunsOnTheDiscsOutline_AndTheBellNeverCutsIntoTheDisc()
        {
            float x = GroupCentre(2);
            var centre = new Vector2(x, NavBarShapeRule.DiscTop(Room, Bump) + Disc);
            float leaveY = NavBarShapeRule.DiscTop(Room, Bump) + Disc * (1f - Mathf.Cos(NavBarShapeRule.LeaveAngle(Bump, Disc)));
            int onArc = 0;
            foreach (var p in Samples(Bell(x)).Where(p => p.y < Room))
            {
                float d = Vector2.Distance(p, centre);
                Assert.GreaterOrEqual(d, Disc - HalfPixel, "the outline at " + p + " stays outside the disc");
                if (p.y <= leaveY - 1e-3f)
                {
                    onArc++;
                    Assert.AreEqual(Disc, d, HalfPixel, "the crest at " + p + " is the disc's own outline");
                }
            }
            Assert.Greater(onArc, 50, "precondition: the crest was walked");
            Assert.AreEqual(NavBarShapeRule.HugDegrees, NavBarShapeRule.LeaveAngle(Bump, Disc) * Mathf.Rad2Deg, 0.01f, "the bar hugs the disc's top to 50 deg");
        }

        // The P3b review (the developer's red line): the bell is as deep as the disc drawn -- its height the disc's radius, so the disc's centre
        // sits on the flat edge -- the edge within 2 pt of the disc's outline from its top to 50 deg each side, within 3 pt at 0.6 of the
        // radius, and right beside the disc the bar visibly rises (the shoulder P3b's 12 pt bell hid under the disc)
        [Test]
        public void TheBell_IsAsDeepAsTheDisc_AndHugsItsOutlineTo50Degrees_ThenAVisibleShoulder()
        {
            Assert.AreEqual(Disc, Bump, 1e-4f, "the bell's height is the disc's radius");
            Assert.Greater(NavBarShapeRule.DiscTop(Room, Bump), 0f, "precondition: the room above the edge holds the disc");
            foreach (int g in new[] { 1, 2, 3, 4 })
            {
                float cx = GroupCentre(g), cy = NavBarShapeRule.DiscTop(Room, Bump) + Disc;
                var upper = Samples(Bell(cx), 400).Where(p => p.y < Room + 1e-3f).ToList();
                float EdgeAt(float x) => upper.Where(p => Mathf.Abs(p.x - x) < 0.3f).Min(p => p.y);
                foreach (int side in new[] { -1, 1 })
                {
                    for (int deg = 0; deg <= 50; deg++)
                    {
                        float a = deg * Mathf.Deg2Rad, x = cx + side * Disc * Mathf.Sin(a);
                        // - 2 pt: the brief's hug (the developer's red line)
                        Assert.AreEqual(cy - Disc * Mathf.Cos(a), EdgeAt(x), 2f, "group " + g + ", " + side * deg + " deg: the edge hugs the disc");
                    }
                    float at06 = cx + side * 0.6f * Disc;
                    Assert.AreEqual(cy - Mathf.Sqrt(Disc * Disc - 0.36f * Disc * Disc), EdgeAt(at06), 3f, "group " + g + ": at 0.6 R the edge is on the outline");
                    // - beside the disc (one radius out) the bar still stands at least a quarter of the radius above its flat edge: seen, not hidden
                    Assert.Less(EdgeAt(cx + side * Disc), Room - 0.25f * Disc, "group " + g + ": the shoulder rises visibly beside the disc");
                }
            }
        }

        // Raised Depth (the P3c review b) at -1 / -0.5 / 0 / 0.5 / 1, on a middle group and the first: the edge's extreme over the disc's
        // centre is the depth's rise (+: a bell depth x R above the edge; 0: the very flat path; -: a cradle depth x (R + gap) under it, inside
        // the bar); C1 at every join; each side monotonic (into the cradle as into the bell); at -1 the trough runs on a circle concentric with
        // the disc, the gap out from it, and never nearer
        [Test]
        public void RaisedDepth_RisesABell_FlattensAtZero_AndBelowZeroDipsACradleConcentricWithTheDisc()
        {
            float gap = Token("cradle-gap");
            Assert.Greater(gap, 0f, "precondition: the cradle's gap token");
            var flat = NavBarShapeRule.Path(W, H, Room, Corner, 0f, Half, GroupCentre(2), Disc);
            foreach (float depth in new[] { -1f, -0.5f, 0f, 0.5f, 1f })
                foreach (int g in new[] { 0, 2 })
                {
                    float rise = NavBarShapeRule.EdgeRise(depth, Disc, gap), crest = NavBarShapeRule.CrestRadius(rise, Disc, gap);
                    Assert.AreEqual(depth >= 0f ? depth * Disc : depth * (Disc + gap), rise, 1e-4f, "depth " + depth + ": the rise");
                    float bx = NavBarShapeRule.BumpX(GroupCentre(g), W, Disc);
                    var path = NavBarShapeRule.Path(W, H, Room, Corner, rise, Half, bx, crest);
                    string at = $"depth {depth}, group {g}";
                    if (depth == 0f)
                    {
                        Assert.AreEqual(flat.Count, path.Count, at + ": the flat path");
                        for (int i = 0; i < flat.Count; i++) Assert.AreEqual(flat[i].End, path[i].End, at + ": the flat path, piece " + i);
                        continue;
                    }
                    var edge = Samples(path, 200).Where(p => p.x > 0.5f && p.x < W - 0.5f && p.y < Room + H - Corner).ToList();
                    float extreme = depth > 0f ? edge.Min(p => p.y) : edge.Where(p => Mathf.Abs(p.x - bx) < Half + 2f).Max(p => p.y);
                    Assert.AreEqual(Room - rise, extreme, HalfPixel, at + ": the edge's extreme over the disc is the rise");
                    var deepest = depth > 0f ? edge.OrderBy(p => p.y).First() : edge.Where(p => Mathf.Abs(p.x - bx) < Half + 2f).OrderByDescending(p => p.y).First();
                    Assert.AreEqual(bx, deepest.x, HalfPixel, at + ": ...over the disc's centre");
                    Assert.Less(edge.Max(p => p.y), Room + H - 1f, at + ": the cradle stays inside the bar");
                    // - no corner at any join
                    var pieces = Pieces(path);
                    for (int i = 0; i < pieces.Count; i++)
                    {
                        float turn = Vector2.Angle(Arriving(pieces[i]), Leaving(pieces[(i + 1) % pieces.Count]));
                        Assert.Less(turn, SmoothDegrees, $"{at}: a corner of {turn:0.00} deg at {pieces[i][pieces[i].Length - 1]}");
                    }
                    // - each side of the bump monotonic along the top edge, between the bar's two corners (the sides and corners are not the bump)
                    var top = Samples(path).Where(p => Mathf.Abs(p.x - bx) <= Half && p.x > Corner + 1f && p.x < W - Corner - 1f
                        && (depth > 0f ? p.y <= Room + 1e-3f : p.y >= Room - 1e-3f && p.y < Room + H * 0.5f)).ToList();
                    for (int i = 1; i < top.Count; i++)
                    {
                        if (top[i].x < top[i - 1].x - 1e-3f) continue;
                        float dy = (top[i].y - top[i - 1].y) * (depth > 0f ? 1f : -1f);
                        if (top[i].x <= bx) Assert.LessOrEqual(dy, 1e-3f, $"{at}: the left side only goes toward the extreme, at {top[i]}");
                        else if (top[i - 1].x >= bx) Assert.GreaterOrEqual(dy, -1e-3f, $"{at}: the right side only comes back, at {top[i]}");
                    }
                    if (depth == -1f)
                    {
                        // - the disc's centre sits on the flat edge whatever the depth: the trough is concentric with it, `gap` out
                        var centre = new Vector2(bx, Room);
                        float leave = NavBarShapeRule.LeaveAngle(-rise, crest);
                        int onArc = 0;
                        foreach (var p in edge.Where(p => p.y > Room + 1e-3f))
                        {
                            float d = Vector2.Distance(p, centre);
                            Assert.GreaterOrEqual(d, Disc + gap - HalfPixel, $"{at}: the cradle at {p} never comes nearer the disc than the gap");
                            if (p.y >= Room + crest * Mathf.Cos(leave) + 1e-3f)
                            {
                                onArc++;
                                Assert.AreEqual(Disc + gap, d, HalfPixel, $"{at}: the trough at {p} is concentric with the disc");
                            }
                        }
                        Assert.Greater(onArc, 20, "precondition: the trough was walked");
                    }
                }
        }

        // C1: wherever two pieces meet, the second leaves in the direction the first arrived -- a middle group, the first and last of six,
        // a disc pushed against either side, and the mirror at Top
        [Test]
        public void TheOutline_HasNoCorner_AtAnyJoin_ForEveryGroup_NearASide_AndAtTop()
        {
            var centres = Enumerable.Range(0, 6).Select(GroupCentre).Concat(new[] { Disc, W - Disc, Disc + 3f }).ToList();
            foreach (float x in centres)
                foreach (bool top in new[] { false, true })
                {
                    var path = Bell(x);
                    if (top) path = NavBarShapeRule.Mirror(path, Room + H);
                    var pieces = Pieces(path);
                    for (int i = 0; i < pieces.Count; i++)
                    {
                        var next = pieces[(i + 1) % pieces.Count];
                        float turn = Vector2.Angle(Arriving(pieces[i]), Leaving(next));
                        Assert.Less(turn, SmoothDegrees, $"centre {x:0.0} {(top ? "Top" : "Bottom")}: a corner of {turn:0.00} deg at {pieces[i][pieces[i].Length - 1]}");
                    }
                }
        }

        // From each foot to the peak the bell only rises (and from the peak to the far foot only falls), and it never turns back sideways
        [Test]
        public void EachSideOfTheBell_IsMonotonic_FromItsFootToThePeak()
        {
            foreach (float x in new[] { GroupCentre(0), GroupCentre(2), GroupCentre(5) })
            {
                var edge = Samples(Bell(x)).Where(p => p.y <= Room + 1e-3f && p.x > 0f && p.x < W).ToList();
                // - the top edge in the path's order runs left to right
                for (int i = 1; i < edge.Count; i++)
                {
                    Assert.GreaterOrEqual(edge[i].x, edge[i - 1].x - 1e-3f, $"centre {x:0.0}: the edge never turns back at {edge[i]}");
                    if (edge[i].x <= x) Assert.LessOrEqual(edge[i].y, edge[i - 1].y + 1e-3f, $"centre {x:0.0}: the left side only rises, at {edge[i]}");
                    else if (edge[i - 1].x >= x) Assert.GreaterOrEqual(edge[i].y, edge[i - 1].y - 1e-3f, $"centre {x:0.0}: the right side only falls, at {edge[i]}");
                }
            }
        }

        // Wide soft shoulders: the bell's feet stand Half from the disc's centre, well past the disc (a middle group); near a side the
        // shoulder narrows into the top corner, which keeps its radius while the shoulder still fits beside it
        [Test]
        public void TheShoulders_AreWide_AndNearASideTheyNarrowIntoTheCorner()
        {
            Assert.Greater(Half, Disc * 2f, "the bell is over twice as wide as the disc: wide soft shoulders");
            float x = GroupCentre(2);
            // - the feet: where the first shoulder (cubic) leaves the edge and where the last one lands on it (a soft landing: flat for its
            //   last few points, so the feet are read from the path, not from where the outline visibly rises)
            var cubics = Pieces(Bell(x)).Where(p => p.Length == 4).ToList();
            Assert.AreEqual(4, cubics.Count, "precondition: two shoulders and the crest's two arcs");
            Assert.Less(Vector2.Distance(new Vector2(x - Half, Room), cubics[0][0]), 1e-3f, "a middle group: the left foot Half from the centre, on the edge " + cubics[0][0]);
            Assert.Less(Vector2.Distance(new Vector2(x + Half, Room), cubics[3][3]), 1e-3f, "...and the right one " + cubics[3][3]);
            // - the first group: the disc-deep bell needs more room than lies beside it, so its corner gives way part of its radius (never all
            //   of it: a round corner, the C1 test) and the shoulder stays whole
            var first = Bell(GroupCentre(0));
            Assert.Less(first[0].P1.y, Room + Corner, "the first group: the top-left corner gives way to the shoulder");
            Assert.Greater(first[0].P1.y, Room + 0.5f * Corner, "...keeping over half its radius");
            Assert.Less(Samples(first).Where(p => p.y < Room - 1e-3f).Min(p => p.x), GroupCentre(0) - Disc * 0.5f,
                "...and the shoulder still falls past the disc's side");
            // - a disc against the side: the corner gives way, the outline stays smooth (the C1 test)
            Assert.Less(Bell(Disc)[0].P1.y, Room + Corner, "a disc at the side: the corner gives way to the shoulder");
            Assert.AreEqual(Disc, NavBarShapeRule.BumpX(2f, W, Disc), "the bell's centre holds the disc inside the bar");
            Assert.AreEqual(W - Disc, NavBarShapeRule.BumpX(W - 2f, W, Disc));
            Assert.AreEqual(179f, NavBarShapeRule.BumpX(179f, W, Disc), "anywhere else: the disc's own centre");
        }

        [Test]
        public void WithoutRaised_TheEdgeIsFlat_AndTheCornersFollowTheShape()
        {
            var flat = NavBarShapeRule.Path(W, H, Room, H * 0.5f, 0f, Half, 179f, Disc);
            Assert.AreEqual(Room, Samples(flat).Min(p => p.y), 1e-3f, "no point above the edge");
            float raised = Token("raised-radius");
            Assert.AreEqual(H * 0.5f, NavBarShapeRule.Radius(true, false, H, raised), "floating, no bump: a stadium");
            Assert.AreEqual(raised, NavBarShapeRule.Radius(true, true, H, raised), "floating under Raised: v5's smaller corner");
            Assert.AreEqual(0f, NavBarShapeRule.Radius(false, true, H, raised), "full width: square");
            Assert.AreEqual(0f, NavBarShapeRule.Radius(false, false, H, raised));
        }

        [Test]
        public void AtTop_ThePathIsMirrored_TheBellHangsBelowTheBarsFoot_ItsLowestPointTheDiscsFoot()
        {
            float x = GroupCentre(3);
            var path = Bell(x);
            var top = NavBarShapeRule.Mirror(path, Room + H);
            var points = Samples(top);
            Assert.AreEqual(0f, points.Min(p => p.y), 1e-3f, "the bar's top edge at the box's top");
            var lowest = points.OrderByDescending(p => p.y).First();
            Assert.AreEqual(H + Bump, lowest.y, HalfPixel, "the lowest point is the bell's, Bump below the bar's foot (at H) -- the disc's foot");
            Assert.AreEqual(x, lowest.x, HalfPixel, "...under the group");
            Assert.AreEqual(path.Count, top.Count);
        }

        [Test]
        public void TheSlide_OvershootsItsTarget_ThenSettlesOnIt()
        {
            Assert.AreEqual(0f, NavBarShapeRule.Slide(0f));
            Assert.AreEqual(1f, NavBarShapeRule.Slide(1f));
            Assert.AreEqual(1f, NavBarShapeRule.Slide(2f), "past the end: at rest on the target");
            float peak = 0f, peakT = 0f;
            for (int i = 0; i <= 100; i++)
            {
                float v = NavBarShapeRule.Slide(i / 100f);
                if (v > peak) { peak = v; peakT = i / 100f; }
            }
            Assert.Greater(peak, 1.02f, "it passes the target (v5's small overshoot)");
            Assert.Less(peak, 1.25f, "...by a little");
            Assert.Less(peakT, 0.9f, "the overshoot comes before the end, then it settles back");
            Assert.Greater(NavBarShapeRule.Slide(0.25f), 0.5f, "a fast start");
        }
    }
}
