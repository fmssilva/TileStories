using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using TileStories.Tests;

namespace TileStories.Editor.Tests
{
    // Tier 0 of the edge scrubber (_3.2 section 5.5, step 10) on THE LAMP's real section count (its card through the real builder and
    // the real nav model, never a typed 8) and the numbers of the run's phone frame: the touch token is 48 px, the stack's viewport at
    // Full is 565 px (measured at step 5b) and 106.6 px at Half. What EdgeScrubberRule decides: whether the rail is drawn at all (every
    // family needs a full touch target), where the column sits, and which dot a finger at a y position picks -- the centres, the
    // boundaries, the two ends, the overshoot past them and a release further out.
    [Ignore(TestSkips.OldNavigationElements)]
    public class EdgeScrubberRuleTests
    {
        // --ts-touch-target
        private const float Target = 48f;
        private const float FullViewport = 565f;
        private const float HalfViewport = 106.6f;

        private static int LampSections()
        {
            var config = JsonUtility.FromJson<WallConfigData>(File.ReadAllText("Assets/Apps/LivingRoom/config.json"));
            var lamp = config.pois.Single(p => p.id == "lamp");
            var built = BlockStackBuilder.Build(lamp, config.card_settings, BlockRegistry.Shared, config.pois);
            return CardNavModel.Build(built.Entries, BlockRegistry.Shared, config.card_settings).Sections.Count;
        }

        // ---------------- Fits ----------------

        [Test]
        public void TheRail_IsDrawnOnlyWhereEveryFamilyGetsAFullTouchTarget()
        {
            int lamp = LampSections();
            Assert.AreEqual(8, lamp, "precondition: The Lamp has eight sections (families that navigate)");
            Assert.IsTrue(EdgeScrubberRule.Fits(lamp, FullViewport, Target), "Full: 8 x 48 = 384 in 565");
            Assert.IsTrue(EdgeScrubberRule.Fits(lamp, lamp * Target, Target), "exactly enough room: it fits");
            Assert.IsFalse(EdgeScrubberRule.Fits(lamp, lamp * Target - 0.5f, Target), "half a pixel short: no rail, never a squeezed target");
            Assert.IsFalse(EdgeScrubberRule.Fits(lamp, HalfViewport, Target), "The Lamp's Half (106.6): only two targets fit, so no rail");
            Assert.IsFalse(EdgeScrubberRule.Fits(lamp, 0f, Target), "Peek: the viewport has no height");
            Assert.IsTrue(EdgeScrubberRule.Fits(9, FullViewport, Target), "an app's ninth family still fits at Full (432)");
            Assert.IsFalse(EdgeScrubberRule.Fits(12, FullViewport, Target), "twelve families (576) do not");
            Assert.IsFalse(EdgeScrubberRule.Fits(0, FullViewport, Target), "no family, no rail");
            Assert.IsFalse(EdgeScrubberRule.Fits(lamp, float.NaN, Target), "a viewport not laid out yet draws nothing");
        }

        // ---------------- the column ----------------

        [Test]
        public void TheColumn_IsCentredInTheViewport_AndItsTargetsTileWithNoGap()
        {
            int lamp = LampSections();
            var column = EdgeScrubberRule.Layout(lamp, FullViewport, Target);
            Assert.AreEqual(lamp * Target, column.Height, 1e-3f, "one 48 px target per family");
            Assert.AreEqual((FullViewport - lamp * Target) / 2f, column.Top, 1e-3f, "centred: equal room above and below");
            Assert.AreEqual(column.Top, FullViewport - column.Top - column.Height, 1e-3f, "the same room under it");
            for (int i = 0; i < lamp; i++)
                Assert.AreEqual(column.Top + (i + 0.5f) * Target, column.CentreOf(i), 1e-3f, "dot " + i + " sits in the middle of its target");
            Assert.AreEqual(Target, column.CentreOf(1) - column.CentreOf(0), 1e-3f, "neighbouring dots are one target apart: the targets touch");
        }

        // ---------------- which family a y picks ----------------

        [Test]
        public void EveryDotsCentre_PicksItself_AndTheBoundaryBelongsToTheNextDot()
        {
            int lamp = LampSections();
            var column = EdgeScrubberRule.Layout(lamp, FullViewport, Target);
            for (int i = 0; i < lamp; i++) Assert.AreEqual(i, EdgeScrubberRule.IndexAt(column, column.CentreOf(i)), "the centre of dot " + i);
            for (int i = 1; i < lamp; i++)
            {
                float boundary = column.Top + i * Target;
                Assert.AreEqual(i - 1, EdgeScrubberRule.IndexAt(column, boundary - 0.01f), "just above the line between " + (i - 1) + " and " + i);
                Assert.AreEqual(i, EdgeScrubberRule.IndexAt(column, boundary), "ON the line: the lower target");
            }
        }

        [Test]
        public void AFingerSweptDownTheViewport_PicksEveryFamilyOnce_InOrder_NeverGoingBack()
        {
            int lamp = LampSections();
            var column = EdgeScrubberRule.Layout(lamp, FullViewport, Target);
            int last = -1;
            var seen = new System.Collections.Generic.List<int>();
            for (float y = 0f; y <= FullViewport; y += 0.5f)
            {
                int picked = EdgeScrubberRule.IndexAt(column, y);
                if (picked < 0) continue;
                Assert.GreaterOrEqual(picked, last, "y " + y + ": the pick never goes back up the column");
                if (picked != last) seen.Add(picked);
                last = picked;
            }
            CollectionAssert.AreEqual(Enumerable.Range(0, lamp).ToList(), seen, "every family is picked exactly once, first to last");
        }

        [Test]
        public void TheEnds_OvershootKeepsTheEndDot_AndFurtherOutPicksNothing()
        {
            int lamp = LampSections();
            var column = EdgeScrubberRule.Layout(lamp, FullViewport, Target);
            float bottom = column.Top + column.Height;
            Assert.AreEqual(0, EdgeScrubberRule.IndexAt(column, column.Top), "the very top of the column: the first family");
            Assert.AreEqual(lamp - 1, EdgeScrubberRule.IndexAt(column, bottom), "the very bottom edge: the last family");
            Assert.AreEqual(lamp - 1, EdgeScrubberRule.IndexAt(column, bottom - 0.01f), "just inside it: the last family");
            Assert.AreEqual(0, EdgeScrubberRule.IndexAt(column, column.Top - Target), "a whole target above the top (a thumb overshoots): still the first");
            Assert.AreEqual(lamp - 1, EdgeScrubberRule.IndexAt(column, bottom + Target), "a whole target below the bottom: still the last");
            Assert.AreEqual(-1, EdgeScrubberRule.IndexAt(column, column.Top - Target - 0.01f), "further above: a release there picks nothing");
            Assert.AreEqual(-1, EdgeScrubberRule.IndexAt(column, bottom + Target + 0.01f), "further below: nothing");
            Assert.AreEqual(-1, EdgeScrubberRule.IndexAt(EdgeScrubberRule.Layout(0, FullViewport, Target), 100f), "no dots: nothing to pick");
        }
    }
}
