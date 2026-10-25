using System.Collections.Generic;
using NUnit.Framework;

namespace TileStories.Editor.Tests
{
    // Tier 0 of LitBlockRule: the block whose centre is nearest the line LineFraction down the viewport, among the blocks that show any
    // part of themselves. A 600 px viewport over three 400 px blocks with 24 px gaps (tops 0 / 424 / 848)
    public class LitBlockRuleTests
    {
        private const float Viewport = 600f;
        private static readonly LitBlockRule.Block[] Three =
        {
            new(0f, 400f), new(424f, 824f), new(848f, 1248f),
        };

        private static float Line(float offset) => offset + Viewport * LitBlockRule.LineFraction;

        [Test]
        public void AtTheTop_TheFirstBlockIsLit()
        {
            // line at 270: centres 200 (70 away) and 624 (354 away)
            Assert.AreEqual(0, LitBlockRule.Pick(Three, 0f, Viewport));
        }

        [Test]
        public void TheLightMoves_WhereTheLineCrossesTheMidpointBetweenTwoCentres()
        {
            // centres 200 and 624: equal distance where the line is at 412, i.e. offset 412 - 270 = 142
            float midway = (Three[0].Centre + Three[1].Centre) * 0.5f - Viewport * LitBlockRule.LineFraction;
            Assert.AreEqual(0, LitBlockRule.Pick(Three, midway - 1f, Viewport), "1 px before the midpoint the first block keeps the light");
            Assert.AreEqual(1, LitBlockRule.Pick(Three, midway + 1f, Viewport), "1 px after it the second block takes the light");
        }

        [Test]
        public void ASweepOverTheWholeRange_LightsEachBlockOnce_InOrder()
        {
            var seen = new List<int>();
            for (float offset = 0f; offset <= 1248f - Viewport; offset += 1f)
            {
                int lit = LitBlockRule.Pick(Three, offset, Viewport);
                Assert.GreaterOrEqual(lit, 0, "some block is lit at offset " + offset);
                float distance = System.Math.Abs(Three[lit].Centre - Line(offset));
                foreach (var block in Three)
                    if (block.Bottom > offset && block.Top < offset + Viewport)
                        Assert.LessOrEqual(distance, System.Math.Abs(block.Centre - Line(offset)), "the lit block is the nearest at offset " + offset);
                if (seen.Count == 0 || seen[^1] != lit) seen.Add(lit);
            }
            CollectionAssert.AreEqual(new[] { 0, 1, 2 }, seen, "the light walks down the card once, never back");
        }

        [Test]
        public void ABlockOutOfView_IsNeverLit_EvenWhenItsCentreIsNearer()
        {
            // a tall block in view (centre 1590, 1320 from the line at 270) and a short one starting right at the viewport's foot (centre
            // 620, 350 from the line): the short one would win on distance alone
            var blocks = new[] { new LitBlockRule.Block(590f, 2590f), new LitBlockRule.Block(600f, 640f) };
            Assert.AreEqual(0, LitBlockRule.Pick(blocks, 0f, Viewport), "a block starting exactly at the viewport's foot shows nothing of itself");
            Assert.AreEqual(1, LitBlockRule.Pick(blocks, 1f, Viewport), "1 px of it in view: it is a candidate again, and the nearer");
        }

        [Test]
        public void NoBlocks_OrNoneInView_LightsNothing()
        {
            Assert.AreEqual(-1, LitBlockRule.Pick(new LitBlockRule.Block[0], 0f, Viewport));
            Assert.AreEqual(-1, LitBlockRule.Pick(Three, 5000f, Viewport), "every block above the viewport");
        }

        [Test]
        public void ABlockTallerThanTheViewport_StaysLit_WhileItFillsTheScreen()
        {
            var blocks = new[] { new LitBlockRule.Block(0f, 300f), new LitBlockRule.Block(324f, 2324f), new LitBlockRule.Block(2348f, 2648f) };
            for (float offset = 324f; offset <= 2324f - Viewport; offset += 50f)
                Assert.AreEqual(1, LitBlockRule.Pick(blocks, offset, Viewport), "the tall block alone fills the viewport at offset " + offset);
        }
    }
}
