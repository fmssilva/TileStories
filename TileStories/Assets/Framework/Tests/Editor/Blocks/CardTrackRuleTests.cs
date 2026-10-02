using NUnit.Framework;

namespace TileStories.Editor.Tests
{
    // _3.1 15.3.4: the cells of a horizontal swipe track fill the strip with a WHOLE number of cells, so a card is never cut in half at the
    // edge of the screen. The numbers below are the phone card's: a 390 px sheet with 16 px sides leaves a 358 px strip.
    public class CardTrackRuleTests
    {
        private const float Strip = 358f;

        [Test]
        public void TheTimelinesPhoneStrip_FitsTwoWholeEvents_NotTwoAndAHalf()
        {
            Assert.AreEqual(2, CardTrackRule.WholeCells(Strip, 160f, 0f), "358 / 160 = 2.2: two whole events");
            Assert.AreEqual(179f, CardTrackRule.CellWidth(Strip, 160f, 0f), 0.001f, "stretched to fill the strip: 2 x 179");
        }

        [Test]
        public void TheRelatedCarouselsPhoneStrip_FitsTwoWholeCards_TheGapAfterEachIncluded()
        {
            Assert.AreEqual(2, CardTrackRule.WholeCells(Strip, 140f, 12f), "358 / (140 + 12) = 2.35: two whole cards");
            float cell = CardTrackRule.CellWidth(Strip, 140f, 12f);
            Assert.AreEqual(167f, cell, 0.001f);
            Assert.AreEqual(Strip, 2 * (cell + 12f), 0.001f, "two cards and the gap after each fill the strip exactly");
        }

        [Test]
        public void ACellIsNeverNarrowerThanItsMinimum_WhileMoreThanOneFits()
        {
            foreach (float strip in new[] { 300f, 358f, 420f, 640f, 1000f })
            {
                int cells = CardTrackRule.WholeCells(strip, 160f, 0f);
                float cell = CardTrackRule.CellWidth(strip, 160f, 0f);
                Assert.GreaterOrEqual(cell, 160f - 0.001f, strip + ": not narrower than the token");
                Assert.AreEqual(strip, cells * cell, 0.001f, strip + ": " + cells + " cells fill the strip exactly");
            }
        }

        [Test]
        public void AStripOfExactlyNSteps_CountsN_NotNMinusOne()
        {
            Assert.AreEqual(3, CardTrackRule.WholeCells(480f, 160f, 0f), "no float error loses a card");
            Assert.AreEqual(160f, CardTrackRule.CellWidth(480f, 160f, 0f), 0.001f);
            Assert.AreEqual(2, CardTrackRule.WholeCells(304f, 140f, 12f));
        }

        [Test]
        public void AStripNarrowerThanOneCell_ShowsOneCell_AsWideAsTheStripAllows()
        {
            Assert.AreEqual(1, CardTrackRule.WholeCells(120f, 160f, 0f));
            Assert.AreEqual(120f, CardTrackRule.CellWidth(120f, 160f, 0f), 0.001f);
            Assert.AreEqual(108f, CardTrackRule.CellWidth(120f, 140f, 12f), 0.001f, "the gap stays out of the card");
        }

        [Test]
        public void NoSizeYet_KeepsTheMinimum_AndNeverReturnsZeroOrNegative()
        {
            Assert.AreEqual(1, CardTrackRule.WholeCells(0f, 160f, 0f));
            Assert.AreEqual(160f, CardTrackRule.CellWidth(0f, 160f, 0f), "before the strip is laid out the token width shows");
            Assert.AreEqual(160f, CardTrackRule.CellWidth(-5f, 160f, 0f));
            Assert.GreaterOrEqual(CardTrackRule.CellWidth(5f, 160f, 40f), 1f);
        }
    }
}
