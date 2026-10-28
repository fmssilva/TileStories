using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // Tier 0 of the Navigation Bar's drawn shape (NavBarShapeRule, v5's renderVals): the ONE path's bump at the chosen group (its peak, its
    // feet on the edge, a top corner giving way beside it), the flat edge without Raised, the corners per shape, the mirror at Top (the bump hangs down),
    // and the slide's overshoot. v5's numbers at the phone's frame: a floating bar 358 wide, 62 high, 26 of room, an 18 bump 38 half wide (P3: wider and deeper than its 40 disc).
    public class NavBarShapeRuleTests
    {
        private const float W = 358f, H = 62f, Room = 26f, Bump = 18f, Half = 38f, Raised = 18f;

        private static IEnumerable<Vector2> Points(List<NavBarShapeRule.Command> path)
        {
            foreach (var c in path)
                switch (c.Op)
                {
                    case NavBarShapeRule.Op.Move:
                    case NavBarShapeRule.Op.Line: yield return c.P1; break;
                    case NavBarShapeRule.Op.Quad: yield return c.P1; yield return c.P2; break;
                    case NavBarShapeRule.Op.Cubic: yield return c.P1; yield return c.P2; yield return c.P3; break;
                }
        }

        [Test]
        public void Raised_BendsTheEdgeIntoABump_PeakingAtTheGroup_ItsFeetOnTheEdge_TheRestOfTheBarUntouched()
        {
            float r = NavBarShapeRule.Radius(true, true, H, Raised);
            float x = NavBarShapeRule.BumpX(150f, W, r, Half);
            var path = NavBarShapeRule.Path(W, H, Room, r, Bump, Half, x);
            var points = Points(path).ToList();
            Assert.AreEqual(Room - Bump, points.Min(p => p.y), 1e-4f, "the highest point is the bump's peak, Bump above the edge");
            var peak = path[2].P3;
            Assert.AreEqual(new Vector2(150f, Room - Bump), peak, "the peak sits over the group's centre");
            Assert.AreEqual(new Vector2(150f - Half, Room), path[1].P1, "the left foot on the edge");
            Assert.AreEqual(new Vector2(150f + Half, Room), path[3].P3, "the right foot on the edge");
            Assert.AreEqual(Room + H, points.Max(p => p.y), 1e-4f, "the bar's foot");
            Assert.AreEqual(0f, points.Min(p => p.x), 1e-4f);
            Assert.AreEqual(W, points.Max(p => p.x), 1e-4f);
            Assert.AreEqual(NavBarShapeRule.Op.Move, path[0].Op);
            Assert.AreEqual(NavBarShapeRule.Op.Close, path[path.Count - 1].Op, "one closed path");
            Assert.AreEqual(path[0].P1, path[path.Count - 2].P2, "it ends where it began");
        }

        [Test]
        public void ABumpNearASide_TakesThatTopCornersRoom_SoItSitsUnderTheFirstAndLastGroups()
        {
            float r = NavBarShapeRule.Radius(true, true, H, Raised);
            Assert.AreEqual(Half + 1f, NavBarShapeRule.BumpX(10f, W, r, Half), "near the left side: its foot stays inside the bar");
            Assert.AreEqual(W - Half - 1f, NavBarShapeRule.BumpX(W - 10f, W, r, Half), "near the right side");
            Assert.AreEqual(179f, NavBarShapeRule.BumpX(179f, W, r, Half), "a middle group: where it is");
            // - the first of six groups on the 10 pt padded row: centre 10 + 338 / 12 = 38.2 -- the bump is drawn there (1 pt inside at most)
            float first = 10f + (W - 20f) / 6f * 0.5f;
            float x = NavBarShapeRule.BumpX(first, W, r, Half);
            Assert.AreEqual(first, x, 1f, "the first group's bump sits under it, not held off by the corner");
            var path = NavBarShapeRule.Path(W, H, Room, r, Bump, Half, x);
            float foot = x - Half;
            Assert.AreEqual(new Vector2(foot, Room), path[0].P1, "the top-left corner shrank to the room left beside the bump's foot");
            Assert.AreEqual(new Vector2(0f, Room + foot), path[path.Count - 3].P1, "...on the side too: a smaller quarter turn");
            Assert.AreEqual(new Vector2(W - r, Room), path[4].P1, "the far corner keeps its own radius");
            // - no bump (another indicator): both corners keep theirs, wherever the centre is
            var flat = NavBarShapeRule.Path(W, H, Room, r, 0f, Half, x);
            Assert.AreEqual(new Vector2(r, Room), flat[0].P1, "a flat edge keeps the corner");
            Assert.AreEqual(r, NavBarShapeRule.TopCorner(r, 40f));
            Assert.AreEqual(5f, NavBarShapeRule.TopCorner(r, 5f));
            Assert.AreEqual(0f, NavBarShapeRule.TopCorner(r, -3f));
        }

        [Test]
        public void TheBump_IsWiderAndDeeperThanItsDisc_SoTheEdgeBendsRoundIt()
        {
            // - P3's numbers, read from the card's tokens file (never retyped): a 76 x 18 bump round a 40 disc
            string tokens = System.IO.File.ReadAllText("Assets/Framework/Runtime/UI/Screen/Cards/CardTokens.uss");
            float Read(string name)
            {
                var m = System.Text.RegularExpressions.Regex.Match(tokens, "--ts-navbar-" + name + @":\s*([0-9.]+)");
                Assert.IsTrue(m.Success, "the token --ts-navbar-" + name);
                return float.Parse(m.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);
            }
            float half = Read("bump-half"), bump = Read("bump"), disc = Read("disc-raised"), gap = Read("disc-gap"), room = Read("room");
            Assert.AreEqual(Half, half, "this fixture's numbers are the card's");
            Assert.AreEqual(Bump, bump);
            Assert.Greater(half * 2f, disc + 30f, "the bump's foot reaches well past the disc on both sides (15 pt each at least)");
            Assert.GreaterOrEqual(bump, 18f, "deep enough to read as a bend");
            Assert.GreaterOrEqual(gap, 4f, "a ring of bar round the disc's top wide enough to see");
            float top = NavBarShapeRule.DiscTop(room, bump, gap);
            Assert.AreEqual(room - bump + gap, top, 1e-4f, "the disc's top a gap under the bump's peak");
            Assert.GreaterOrEqual(top, 0f, "...inside the bar's box: the room holds it");
        }

        // _3.3 P3a: the bump is a CUT-OUT round the disc -- every point of its outline above the edge clears the disc by the gap (one
        // physical pixel of tolerance for the curve's sampling), so the bar's colour shows all round the disc's top, never a disc on a flat edge
        [Test]
        public void TheBumpsOutline_RunsRoundTheDisc_AGapAwayAllAlongIt()
        {
            string tokens = System.IO.File.ReadAllText("Assets/Framework/Runtime/UI/Screen/Cards/CardTokens.uss");
            float Read(string name) => float.Parse(System.Text.RegularExpressions.Regex.Match(tokens, "--ts-navbar-" + name + @":\s*([0-9.]+)").Groups[1].Value,
                System.Globalization.CultureInfo.InvariantCulture);
            float bump = Read("bump"), half = Read("bump-half"), disc = Read("disc-raised"), gap = Read("disc-gap"), room = Read("room");
            const float x = 200f;
            var path = NavBarShapeRule.Path(W, H, room, NavBarShapeRule.Radius(true, true, H, Raised), bump, half, x);
            var centre = new Vector2(x, NavBarShapeRule.DiscTop(room, bump, gap) + disc * 0.5f);
            int sampled = 0;
            foreach (var c in path.Where(c => c.Op == NavBarShapeRule.Op.Cubic))
            {
                var from = path[path.IndexOf(c) - 1];
                var p0 = from.Op == NavBarShapeRule.Op.Cubic ? from.P3 : from.P1;
                for (float s = 0f; s <= 1f; s += 0.02f)
                {
                    float u = 1f - s;
                    var p = u * u * u * p0 + 3f * u * u * s * c.P1 + 3f * u * s * s * c.P2 + s * s * s * c.P3;
                    if (p.y >= room) continue;
                    sampled++;
                    Assert.GreaterOrEqual(Vector2.Distance(p, centre), disc * 0.5f + gap - 1f, "the outline at " + p + " stays a gap off the disc");
                }
            }
            Assert.Greater(sampled, 50, "precondition: the bump's outline above the edge was walked");
            Assert.AreEqual(room - bump, Points(path).Min(p => p.y), 1e-4f, "...and rises the bump's full height over the disc's centre");
        }

        [Test]
        public void WithoutRaised_TheEdgeIsFlat_AndTheCornersFollowTheShape()
        {
            var flat = NavBarShapeRule.Path(W, H, Room, H * 0.5f, 0f, Half, 179f);
            Assert.AreEqual(Room, Points(flat).Where(p => p.y < Room + 1f).Min(p => p.y), 1e-4f, "no point above the edge");
            Assert.AreEqual(H * 0.5f, NavBarShapeRule.Radius(true, false, H, Raised), "floating, no bump: a stadium");
            Assert.AreEqual(Raised, NavBarShapeRule.Radius(true, true, H, Raised), "floating under Raised: v5's smaller corner");
            Assert.AreEqual(0f, NavBarShapeRule.Radius(false, true, H, Raised), "full width: square");
            Assert.AreEqual(0f, NavBarShapeRule.Radius(false, false, H, Raised));
        }

        [Test]
        public void AtTop_ThePathIsMirrored_TheBumpHangsBelowTheBarsFoot()
        {
            float r = NavBarShapeRule.Radius(true, true, H, Raised);
            var path = NavBarShapeRule.Path(W, H, Room, r, Bump, Half, 200f);
            var top = NavBarShapeRule.Mirror(path, Room + H);
            var points = Points(top).ToList();
            Assert.AreEqual(0f, points.Min(p => p.y), 1e-4f, "the bar's top edge at the box's top");
            Assert.AreEqual(H + Bump, points.Max(p => p.y), 1e-4f, "the lowest point is the bump, Bump below the bar's foot (at H)");
            Assert.AreEqual(new Vector2(200f, H + Bump), top[2].P3, "under the group");
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
