using NUnit.Framework;
using TileStories.Tests;

namespace TileStories.Editor.Tests
{
    // Tier 0 of the Segmented element's Show Counts fit (_3.2 audit F6, section 5.1): a count goes when it cannot sit beside its word, the
    // word never does. The widths are the ones the element measured at 390 px in the audit's render (a segment's inner room of about
    // 81 px; "Discover" about 56 px wide, its count "25" about 14 px, a 4 px gap) -- a stand-in for the measurement, never a layout.
    [Ignore(TestSkips.OldNavigationElements)]
    public class NavFitRuleTests
    {
        private const float Gap = 4f;

        [Test]
        public void EveryWordWithItsCountAndGap_InsideTheRoom_ShowsTheCounts()
        {
            Assert.IsTrue(NavFitRule.CountsFit(new[] { 40f, 56f }, new[] { 12f, 14f }, Gap, 81f));
        }

        [Test]
        public void OneSegmentThatDoesNotHoldItsWordAndCount_TakesTheCountsOfTheWholeStripAway()
        {
            // - the audit's "Discover" + "25": 56 + 4 + 14 = 74 fits 81, but a 66 px word with the same count does not -- and the short
            //   segments next to it lose theirs too (one decision for the strip)
            Assert.IsFalse(NavFitRule.CountsFit(new[] { 30f, 66f, 30f }, new[] { 12f, 14f, 12f }, Gap, 81f));
        }

        [Test]
        public void TheGapCounts_AWordAndCountThatExactlyFillTheRoomFit_AHairMoreDoesNot()
        {
            Assert.IsTrue(NavFitRule.CountsFit(new[] { 63f }, new[] { 14f }, Gap, 81f), "63 + 4 + 14 = 81: exactly the room");
            Assert.IsFalse(NavFitRule.CountsFit(new[] { 63.5f }, new[] { 14f }, Gap, 81f), "half a point more");
            Assert.IsFalse(NavFitRule.CountsFit(new[] { 63f }, new[] { 14f }, 5f, 81f), "or a wider gap");
        }

        [Test]
        public void NoSegmentsOrNoMeasures_LeavesNothingOut()
        {
            Assert.IsTrue(NavFitRule.CountsFit(new float[0], new float[0], Gap, 81f));
            Assert.IsTrue(NavFitRule.CountsFit(null, null, Gap, 81f));
        }
    }
}
