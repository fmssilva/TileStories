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

        // ---- Peek Next Card (_3.1 15.4.6): whole cells AND a slice of the next one at rest

        private const float Peek = 32f;

        [Test]
        public void WithPeek_TheTimelinesPhoneStrip_ShowsTwoWholeEventsAndASliceOfTheThird()
        {
            Assert.IsTrue(CardTrackRule.Peeks(Strip, 160f, 0f, Peek, 5), "five events, two fit whole: there is a next one to show");
            float cell = CardTrackRule.CellWidth(Strip, 160f, 0f, Peek, 5);
            Assert.AreEqual(163f, cell, 0.001f, "(358 - 32) / 2");
            Assert.AreEqual(Strip, 2 * cell + Peek, 0.001f, "two whole events and the slice fill the strip exactly");
            Assert.GreaterOrEqual(cell, 160f, "never narrower than the cell token");
        }

        [Test]
        public void WithPeek_TheRelatedCarouselsPhoneStrip_ShowsTwoWholeCardsTheirGapsAndASlice()
        {
            float cell = CardTrackRule.CellWidth(Strip, 140f, 12f, Peek, 3);
            Assert.AreEqual(151f, cell, 0.001f, "(358 - 32) / 2 - 12");
            Assert.AreEqual(Strip, 2 * (cell + 12f) + Peek, 0.001f, "two cards, the gap after each, and the slice fill the strip");
        }

        [Test]
        public void WithPeek_ATrackWhoseCellsAllFitWhole_ShowsNoSlice_ItKeepsTheWholeCellsWidth()
        {
            Assert.IsFalse(CardTrackRule.Peeks(Strip, 160f, 0f, Peek, 2), "two events: nothing after them to peek at");
            Assert.AreEqual(CardTrackRule.CellWidth(Strip, 160f, 0f), CardTrackRule.CellWidth(Strip, 160f, 0f, Peek, 2), 0.001f, "the whole-cells width");
            Assert.AreEqual(CardTrackRule.CellWidth(Strip, 140f, 12f), CardTrackRule.CellWidth(Strip, 140f, 12f, Peek, 1), 0.001f, "a single card");
        }

        [Test]
        public void PeekOff_IsExactlyTheWholeCellsRule()
        {
            foreach (float strip in new[] { 300f, 358f, 640f })
                foreach (int count in new[] { 1, 3, 9 })
                {
                    Assert.IsFalse(CardTrackRule.Peeks(strip, 160f, 0f, 0f, count), "no slice asked: none shown");
                    Assert.AreEqual(CardTrackRule.CellWidth(strip, 160f, 0f), CardTrackRule.CellWidth(strip, 160f, 0f, 0f, count), 0.001f, strip + " / " + count);
                }
        }

        [Test]
        public void WithPeek_OnAnyStrip_TheCellsAndTheSliceFillItExactly_AndNoCellIsNarrowerThanItsMinimum()
        {
            foreach (float strip in new[] { 300f, 358f, 420f, 640f, 1000f })
            {
                float cell = CardTrackRule.CellWidth(strip, 140f, 12f, Peek, 20);
                int whole = CardTrackRule.WholeCells(strip - Peek, 140f, 12f);
                Assert.GreaterOrEqual(cell, 140f - 0.001f, strip + ": not narrower than the token");
                Assert.AreEqual(strip, whole * (cell + 12f) + Peek, 0.001f, strip + ": " + whole + " cells, their gaps and the slice fill the strip");
            }
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
