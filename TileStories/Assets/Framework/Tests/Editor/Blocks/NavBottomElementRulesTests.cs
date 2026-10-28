using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using TileStories.Tests;

namespace TileStories.Editor.Tests
{
    // Tier 0 of what the step 7b bottom elements decide (_3.2 section 5.2), on the SHIPPED LivingRoom wall through the real
    // BlockStackBuilder and CardNavModel: NextUpRule (which block is next, what number it is, the last target, a current block that is no
    // target, a reveal growing the card, every Block Order) and BarSheetRule (which tap opens the kinds, on the real Lamp's sections).
    [Ignore(TestSkips.OldNavigationElements)]
    public class NavBottomElementRulesTests
    {
        private const string Lamp = "lamp";
        private const string LampMilitary = "lamp_military";
        private const string LampReligious = "lamp_religious";
        // The Lamp's true / false knowledge check: Show After Reading, a target only once revealed (CardNavModelTests)
        private const string GatedBlock = "block_44";

        [OneTimeSetUp]
        public void TheAppsKindIsRegistered() =>
            Assert.IsTrue(BlockRegistry.Shared.TryGet("size_comparison", out _), "precondition: the LivingRoom app's kind is registered in Edit Mode");

        private static WallConfigData Shipped(string blockOrder)
        {
            var config = JsonUtility.FromJson<WallConfigData>(File.ReadAllText("Assets/Apps/LivingRoom/config.json"));
            config.card_settings.navigation.block_order = blockOrder;
            return config;
        }

        private static CardNavModel Model(WallConfigData config, string poiId = Lamp, System.Func<string, bool> isRevealed = null) =>
            CardNavModel.Build(Entries(config, poiId), BlockRegistry.Shared, config.card_settings, isRevealed);

        private static System.Collections.Generic.List<BlockStackBuilder.Entry> Entries(WallConfigData config, string poiId) =>
            BlockStackBuilder.Build(config.pois.Single(p => p.id == poiId), config.card_settings, BlockRegistry.Shared, config.pois).Entries;

        // ---------------- NextUpRule ----------------

        // The block being read is target i: the next is target i + 1 and its number is i + 2 of ALL the targets -- for every target of
        // The Lamp but the last, in each of the three Block Orders (the next follows the order the stack shows)
        [TestCase(CardOptions.BlockOrderAuthored)]
        [TestCase(CardOptions.BlockOrderEndFamiliesLast)]
        [TestCase(CardOptions.BlockOrderByFamily)]
        public void TheNextTarget_IsTheOneAfterTheBlockBeingRead_AndItsNumberIsItsPlaceAmongAllTheTargets(string blockOrder)
        {
            var model = Model(Shipped(blockOrder));
            Assert.AreEqual(56, model.TargetCount, "precondition: The Lamp's 56 targets in " + blockOrder);
            for (int i = 0; i < model.Targets.Count - 1; i++)
            {
                var answer = NextUpRule.Of(model.Targets, model.Targets[i]);
                Assert.AreEqual(model.Targets[i + 1], answer.NextBlock, "after " + model.Targets[i]);
                Assert.AreEqual(i + 2, answer.Position, model.Targets[i + 1] + " is the " + (i + 2) + "th target");
                Assert.AreEqual(56, answer.Total, "of all 56");
                Assert.IsFalse(answer.Finished, "not the end yet");
            }
        }

        // Audit F2-3: under Pages the Family Pager's "3 of 8" is the card's counter, so Up next leaves its own "N of 56" out; every other layout
        // has no other counter and keeps it
        [Test]
        public void UpNextShowsItsPlace_EverywhereButUnderPages_WhereThePagerIsTheCardsOneCounter()
        {
            Assert.IsFalse(NextUpRule.ShowsPosition(CardOptions.LayoutPages), "pages: the pager counts");
            foreach (string layout in new[] { CardOptions.LayoutScroll, CardOptions.LayoutAccordion, CardOptions.LayoutTabs, CardOptions.LayoutBento })
                Assert.IsTrue(NextUpRule.ShowsPosition(layout), layout + ": no other counter on that screen");
        }

        // The last target: nothing to go to, so the answer says the card is finished (no Next that goes nowhere) and keeps the card's size
        [Test]
        public void AtTheLastTarget_TheCardIsFinished_WithNoNextBlock()
        {
            var model = Model(Shipped(CardOptions.BlockOrderByFamily));
            var answer = NextUpRule.Of(model.Targets, model.Targets[model.Targets.Count - 1]);
            Assert.IsTrue(answer.Finished);
            Assert.IsNull(answer.NextBlock, "no block to jump to");
            Assert.AreEqual(56, answer.Position, "the last target's own place");
            Assert.AreEqual(56, answer.Total);
            var penultimate = NextUpRule.Of(model.Targets, model.Targets[model.Targets.Count - 2]);
            Assert.AreEqual(model.Targets[model.Targets.Count - 1], penultimate.NextBlock, "one before the end still has its Next");
            Assert.IsFalse(penultimate.Finished);
        }

        // A block that is no target (the header, the sticky footer's block, the sources, a key a live edit removed, nothing yet): the next is
        // the FIRST target, 1 of N -- never a crash, never the wrong end of the card
        [Test]
        public void ACurrentBlockThatIsNoTarget_NamesTheFirstTarget_AsOneOfAll()
        {
            var config = Shipped(CardOptions.BlockOrderByFamily);
            var model = Model(config);
            var notTargets = Entries(config, Lamp).Select(e => e.Instance.key).Except(model.Targets).ToList();
            Assert.GreaterOrEqual(notTargets.Count, 3, "precondition: The Lamp has non-target blocks (the header, the pinned footer, the sources): " + string.Join(", ", notTargets));
            foreach (string key in notTargets.Concat(new[] { "block_that_was_deleted", "" }).Append(null))
            {
                var answer = NextUpRule.Of(model.Targets, key);
                Assert.AreEqual(model.Targets[0], answer.NextBlock, "'" + key + "': the first target");
                Assert.AreEqual(1, answer.Position, "'" + key + "': 1 of N");
                Assert.AreEqual(56, answer.Total);
                Assert.IsFalse(answer.Finished, "'" + key + "': nowhere near the end");
            }
        }

        // The model is rebuilt when a waiting block is revealed: the total and every later number move by one, and the block before the
        // revealed one now points at it
        [Test]
        public void ARevealGrowsTheCard_TheTotalAndTheLaterNumbersMoveByOne_AndTheBlockBeforeItNowPointsAtIt()
        {
            var config = Shipped(CardOptions.BlockOrderByFamily);
            var before = Model(config);
            var after = Model(config, isRevealed: key => key == GatedBlock);
            Assert.AreEqual(56, before.TargetCount, "precondition: before the reveal");
            Assert.AreEqual(57, after.TargetCount, "precondition: after the reveal");
            int at = after.Targets.ToList().IndexOf(GatedBlock);
            Assert.Greater(at, 0, "precondition: the revealed block is a target after the reveal, not first");
            string beforeIt = after.Targets[at - 1];
            Assert.AreNotEqual(GatedBlock, NextUpRule.Of(before.Targets, beforeIt).NextBlock, "before the reveal, the block before it points past it");
            var answer = NextUpRule.Of(after.Targets, beforeIt);
            Assert.AreEqual(GatedBlock, answer.NextBlock, "after the reveal it points at the revealed block");
            Assert.AreEqual(at + 1, answer.Position);
            Assert.AreEqual(57, answer.Total);
            // - the block that came right after the gate's place is the same block, numbered one higher now that the gate is open
            var past = NextUpRule.Of(before.Targets, beforeIt);
            var pastNow = NextUpRule.Of(after.Targets, GatedBlock);
            Assert.AreEqual(past.NextBlock, pastNow.NextBlock, "precondition: the same block follows (the gate sits just before it)");
            Assert.AreEqual(past.Position + 1, pastNow.Position, past.NextBlock + " is one place later once the gate is open");
        }

        // The two short cards: Military's four targets count 2, 3, 4 then finish; Religious has none -- an empty answer, not finished
        [Test]
        public void TheShortCards_CountTheirOwnTargets_AndACardWithNoTargetsGivesAnEmptyAnswer()
        {
            var config = Shipped(CardOptions.BlockOrderEndFamiliesLast);
            var military = Model(config, LampMilitary);
            Assert.AreEqual(4, military.TargetCount, "precondition: Lamp Military's 4 targets");
            for (int i = 0; i < 3; i++)
            {
                var answer = NextUpRule.Of(military.Targets, military.Targets[i]);
                Assert.AreEqual(i + 2, answer.Position, "Military, after target " + (i + 1));
                Assert.AreEqual(4, answer.Total);
            }
            Assert.IsTrue(NextUpRule.Of(military.Targets, military.Targets[3]).Finished, "Military's fourth target ends it");

            var religious = Model(config, LampReligious);
            Assert.AreEqual(0, religious.TargetCount, "precondition: Lamp Religious has the header alone");
            var empty = NextUpRule.Of(religious.Targets, null);
            Assert.IsNull(empty.NextBlock);
            Assert.AreEqual(0, empty.Total);
            Assert.IsFalse(empty.Finished, "no targets is not a finished card: there is nothing to be at the end of");
            Assert.AreEqual(0, NextUpRule.Of(null, "block_2").Total, "no list at all reads the same");
        }

        // ---------------- BarSheetRule ----------------

        // The tap decision on its own: only the CURRENT section opens its kinds, and only with a choice (two kinds or more)
        [TestCase(2, 2, 3, true, Description = "the current section with kinds: the second tap opens them")]
        [TestCase(2, 2, 2, true, Description = "two kinds is a choice")]
        [TestCase(2, 2, 1, false, Description = "one kind: nothing to choose, the tap jumps again")]
        [TestCase(2, 2, 0, false, Description = "no kinds")]
        [TestCase(1, 2, 5, false, Description = "another section: the first tap jumps")]
        [TestCase(0, 3, 5, false, Description = "another section, even the first")]
        [TestCase(-1, -1, 5, false, Description = "no section read and none tapped")]
        public void ASecondTapOnTheCurrentSection_OpensItsKinds_OnlyWhenThereIsAChoice(int tapped, int current, int kinds, bool opens) =>
            Assert.AreEqual(opens, BarSheetRule.OpensKinds(tapped, current, kinds));

        // On the real Lamp: the sections with two or more kinds open theirs, a section of one kind (Listen) never does -- and the answer
        // reads the model's own kinds, so the rule and what the list would show cannot differ
        [Test]
        public void OnTheLamp_TheSectionsWithSeveralKindsOpenThem_AndListenWithOneNeverDoes()
        {
            var model = Model(Shipped(CardOptions.BlockOrderByFamily));
            Assert.AreEqual(8, model.Sections.Count, "precondition: The Lamp's eight sections");
            int several = 0;
            for (int i = 0; i < model.Sections.Count; i++)
            {
                var section = model.Sections[i];
                bool opens = BarSheetRule.OpensKinds(i, i, section.Kinds.Count);
                Assert.AreEqual(section.Kinds.Count >= 2, opens, section.Key + " has " + section.Kinds.Count + " kind(s)");
                Assert.IsFalse(BarSheetRule.OpensKinds(i, (i + 1) % model.Sections.Count, section.Kinds.Count), section.Key + ": not while another is read");
                if (opens) several++;
            }
            Assert.Greater(several, 3, "most of The Lamp's families hold several kinds (the element has something to show)");
            var listen = model.Sections.Single(s => s.Key == BuiltInFamilies.Guides);
            Assert.AreEqual(1, listen.Kinds.Count, "precondition: Listen is the one-kind family");
            Assert.IsFalse(BarSheetRule.OpensKinds(model.Sections.ToList().IndexOf(listen), model.Sections.ToList().IndexOf(listen), listen.Kinds.Count));
        }
    }
}
