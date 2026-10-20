using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // NavRailRule (_3.2 step 15a, nav_rails): how many swipes reach the end of a rail and how many cards a rail keeps so it never takes more
    // than the developer allows -- pure arithmetic, then on THE LAMP'S real families through the real builder and model and the track rule's
    // own width (CardTrackRule), the numbers a phone shows. NN/g (Budiu 2018): the last card in 3-4 swipes.
    public class NavRailRuleTests
    {
        // A phone's strip: the card's 390 px less its 16 px gutters (Phase B measures the real strip and asserts it is this wide), a rail
        // card's narrowest width and gap (--ts-rail-card 140, --ts-space-3 12) and Peek Next Card's slice (--ts-track-peek 32)
        private const float Strip = 358f;
        private const float Narrowest = 140f;
        private const float Gap = 12f;
        private const float Peek = 32f;

        private static WallConfigData Shipped() =>
            JsonUtility.FromJson<WallConfigData>(File.ReadAllText("Assets/Apps/LivingRoom/config.json"));

        [Test]
        public void SwipesToEnd_IsTheViewportsOfTravelToTheLastCard_AndZeroWhenTheRailFits()
        {
            Assert.AreEqual(0, NavRailRule.SwipesToEnd(3, 100f, 300f), "three cards of 100 fit a 300 strip: nothing to swipe");
            Assert.AreEqual(1, NavRailRule.SwipesToEnd(4, 100f, 300f), "one card too many: one swipe");
            Assert.AreEqual(1, NavRailRule.SwipesToEnd(6, 100f, 300f), "exactly one viewport past the strip: still one swipe (a float hair does not make it two)");
            Assert.AreEqual(2, NavRailRule.SwipesToEnd(7, 100f, 300f));
            Assert.AreEqual(0, NavRailRule.SwipesToEnd(0, 100f, 300f), "no cards");
            Assert.AreEqual(0, NavRailRule.SwipesToEnd(5, 0f, 300f), "a strip with no step is no rail");
            Assert.AreEqual(0, NavRailRule.SwipesToEnd(5, 100f, 0f), "a strip with no width is not laid out yet");
        }

        [Test]
        public void MostCells_IsTheLongestRailThatStaysWithinTheSwipes_ForEveryStepAndEverySwipeCount()
        {
            Assert.AreEqual(15, NavRailRule.MostCells(100f, 300f, 4), "300 * (4 + 1) / 100");
            for (float step = 90f; step <= 200f; step += 7f)
                for (int swipes = 2; swipes <= 8; swipes++)
                {
                    int most = NavRailRule.MostCells(step, Strip, swipes);
                    Assert.LessOrEqual(NavRailRule.SwipesToEnd(most, step, Strip), swipes, "step " + step + ", " + swipes + " swipes: " + most + " cells reach the end in time");
                    // - and it is the LONGEST one: a cell more takes a swipe more (unless the two-cell floor held it up)
                    if (most > 2) Assert.Greater(NavRailRule.SwipesToEnd(most + 1, step, Strip), swipes, "step " + step + ", " + swipes + " swipes: " + (most + 1) + " cells would not");
                }
            Assert.AreEqual(2, NavRailRule.MostCells(1000f, 300f, 2), "a card wider than the strip still keeps a card and the All card");
            Assert.AreEqual(2, NavRailRule.MostCells(0f, 300f, 4), "no step known yet");
        }

        [Test]
        public void Of_KeepsEveryCardWhileTheRailIsShortEnough_ElseTheCardsThatFitAndTheAllCard()
        {
            var fits = NavRailRule.Of(7, 100f, 300f, 4);
            Assert.AreEqual(7, fits.Cards);
            Assert.IsFalse(fits.HasAll, "7 cards fit within 4 swipes: no All card");
            var exactly = NavRailRule.Of(15, 100f, 300f, 4);
            Assert.AreEqual(15, exactly.Cards, "exactly the most that fits");
            Assert.IsFalse(exactly.HasAll);
            var longer = NavRailRule.Of(16, 100f, 300f, 4);
            Assert.AreEqual(14, longer.Cards, "one over: a cell is kept for the All card");
            Assert.IsTrue(longer.HasAll);
            Assert.AreEqual(15, longer.Cells, "the row is the most the swipes allow");
            Assert.AreEqual(0, NavRailRule.Of(0, 100f, 300f, 4).Cells, "no block, no rail");
            Assert.LessOrEqual(NavRailRule.SwipesToEnd(longer.Cells, 100f, 300f), 4);
        }

        // THE LAMP: its longest family, as a phone lays it out. The brief's question -- how many swipes does the longest family take, and if more
        // than 3-4 what was done -- answered from the real model: the numbers are in the assertion messages
        [Test]
        public void TheLampsFamilies_AtThePhoneWidth_NoRailTakesMoreThanTheSwipesAllowed_AndTheLongestNeededTheAllCard()
        {
            var config = Shipped();
            var lamp = config.pois.Single(p => p.id == "lamp");
            var entries = BlockStackBuilder.Build(lamp, config.card_settings, BlockRegistry.Shared, config.pois).Entries;
            var model = CardNavModel.Build(entries, BlockRegistry.Shared, config.card_settings, null);
            Assert.AreEqual(8, model.Sections.Count, "precondition: The Lamp's eight families");
            int longest = model.Sections.Max(s => s.Targets.Count);
            Assert.AreEqual(18, longest, "precondition: About is the longest family, 18 blocks");

            foreach (int allowed in new[] { 3, 4, 5 })
            {
                bool someRailNeededTheAllCard = false;
                foreach (var section in model.Sections)
                {
                    float width = CardTrackRule.CellWidth(Strip, Narrowest, Gap, Peek, section.Targets.Count);
                    var plan = NavRailRule.Of(section.Targets.Count, width + Gap, Strip, allowed);
                    int swipes = NavRailRule.SwipesToEnd(plan.Cells, width + Gap, Strip);
                    Assert.LessOrEqual(swipes, allowed, section.Key + " (" + section.Targets.Count + " blocks, " + plan.Cards + " cards" + (plan.HasAll ? " + All" : "") + "): " + swipes + " swipes");
                    Assert.AreEqual(plan.HasAll, section.Targets.Count > plan.Cards, section.Key + ": the All card exactly when blocks are left out");
                    someRailNeededTheAllCard |= plan.HasAll;
                }
                Assert.IsTrue(someRailNeededTheAllCard, allowed + " swipes: The Lamp's longest family cannot be reached in time without the All card");
            }

            // - the numbers of the report: About's 18 cards uncapped, and what the cap leaves
            float aboutWidth = CardTrackRule.CellWidth(Strip, Narrowest, Gap, Peek, longest);
            int uncapped = NavRailRule.SwipesToEnd(longest, aboutWidth + Gap, Strip);
            Assert.Greater(uncapped, 4, "uncapped, About's " + longest + " cards (" + aboutWidth + " px + " + Gap + ") take " + uncapped + " swipes: more than 3-4, so the cap is needed");
            var capped = NavRailRule.Of(longest, aboutWidth + Gap, Strip, 4);
            Assert.AreEqual(4, NavRailRule.SwipesToEnd(capped.Cells, aboutWidth + Gap, Strip), "capped at the default 4 swipes: " + capped.Cards + " cards + All");
        }

        [Test]
        public void TheWalkOfTheLampsMediaFamily_PeekNextCardOn_ShowsWholeCardsAndASliceOfTheNext_AndOffNothingCut()
        {
            // - the track rule's own facts the rail reuses (CardTrackRuleTests holds the rest): with the slice the strip shows 2 whole cards
            //   and 32 px of a third; without, 2 whole cards that fill it
            float with = CardTrackRule.CellWidth(Strip, Narrowest, Gap, Peek, 14);
            Assert.AreEqual(2, CardTrackRule.WholeCells(Strip - Peek, Narrowest, Gap), "two whole cards beside the slice");
            Assert.AreEqual(2 * (with + Gap) + Peek, Strip, 0.01f, "the slice is exactly --ts-track-peek wide");
            float without = CardTrackRule.CellWidth(Strip, Narrowest, Gap, 0f, 14);
            Assert.AreEqual(2 * (without + Gap), Strip, 0.01f, "Peek Next Card off: whole cards fill the strip");
        }
    }
}
