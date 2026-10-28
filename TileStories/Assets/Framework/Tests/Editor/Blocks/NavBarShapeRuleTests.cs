using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // Tier 0 of the Navigation Bar's drawn shape (NavBarShapeRule, v5's renderVals): the ONE path's bump at the chosen group (its peak, its
    // feet on the edge, clear of the corners), the flat edge without Raised, the corners per shape, the mirror at Top (the bump hangs down),
    // and the slide's overshoot. v5's numbers at the phone's frame: a floating bar 358 wide, 62 high, 26 of room, a 16 bump 25 half wide.
    public class NavBarShapeRuleTests
    {
        private const float W = 358f, H = 62f, Room = 26f, Bump = 16f, Half = 25f, Raised = 18f;

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
        public void TheBump_StaysClearOfTheCorners_AtTheFirstAndLastGroups()
        {
            float r = NavBarShapeRule.Radius(true, true, H, Raised);
            Assert.AreEqual(r + Half + 1f, NavBarShapeRule.BumpX(10f, W, r, Half), "near the left corner: held off it");
            Assert.AreEqual(W - r - Half - 1f, NavBarShapeRule.BumpX(W - 10f, W, r, Half), "near the right corner");
            Assert.AreEqual(179f, NavBarShapeRule.BumpX(179f, W, r, Half), "a middle group: where it is");
            // - the first of six groups on the 10 pt padded row: centre 10 + 338 / 12 = 38.2 > 44 (the clamp) -> held at 44
            float first = 10f + (W - 20f) / 6f * 0.5f;
            Assert.Greater(NavBarShapeRule.BumpX(first, W, r, Half), first, "the first group's bump moves in, never up the corner");
            var path = NavBarShapeRule.Path(W, H, Room, r, Bump, Half, NavBarShapeRule.BumpX(first, W, r, Half));
            Assert.GreaterOrEqual(path[1].P1.x, r, "the left foot after the corner's end");
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
