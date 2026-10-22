using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // Step 10's second piece (_3.2 section 5.5, E4) on the REAL LivingRoomScene with The Lamp's two hotspot blocks and real taps: a spot whose
    // Opens Block names a block the card can scroll to MOVES the card there (the same jump every navigation element makes, raised as one
    // nav_jump named by the hotspot_image kind) and opens no text of its own; every other spot still opens its title and text. The fixture is
    // the wall's own config: block_35 (numbered) and block_36 (loupes) each carry ONE Opens Block, read from it below, never re-typed. A pick
    // that leads nowhere (the title block, a footer look, a deleted block, a block waiting for Show After Reading) draws no jump -- the spot
    // keeps its popup -- pushed through the live seam like an Editor edit.
    public partial class PoiCardNavSceneTests
    {
        private const string NumberedHotspots = "block_35";
        private const string LoupesHotspots = "block_36";

        private HotspotImageBlockView HotspotOf(string block) =>
            Sheet.Stack.BoundViews.OfType<HotspotImageBlockView>().Single(v => Sheet.Stack.SlotOf(v) == Slot(block));

        // The Opens Block of each spot of a Lamp block, as the wall's config holds them ("" = none)
        private List<string> OpensBlockOf(string block)
        {
            var lamp = ConfigCopy().pois.Single(p => p.id == Lamp);
            var instance = lamp.card.blocks.Single(b => b.key == block);
            var read = new BlockFieldReader(instance, "en", "en");
            return read.Items(BuiltInBlocks.HotspotItemsField).Select(row => read.ItemValue(row, BuiltInBlocks.HotspotOpensBlockField)).ToList();
        }

        private static void SetOpensBlock(WallConfigData config, string block, int spot, string target)
        {
            var row = config.pois.Single(p => p.id == Lamp).card.blocks.Single(b => b.key == block)
                .fields.Single(f => f.key == BuiltInBlocks.HotspotItemsField).items[spot];
            var field = row.fields.Find(f => f.key == BuiltInBlocks.HotspotOpensBlockField);
            if (field == null) row.fields.Add(field = new BlockItemFieldValue { key = BuiltInBlocks.HotspotOpensBlockField });
            field.value = target;
        }

        private IEnumerator ShowHotspots(string block)
        {
            // - selecting the point that is already selected would not open it again: clear first
            SelectionEventBus.Clear();
            yield return CardTestInput.Settle();
            yield return Open(Lamp, SheetStopRule.Stop.Full);
            Scroll.ScrollTo(HotspotOf(block).Root);
            yield return CardTestInput.Settle(0.3f);
        }

        private static Button TargetOf(HotspotImageBlockView view, int spot, bool loupes) => loupes ? view.Spots[spot].Loupe : view.Spots[spot].Pin;

        // The numbered look: a real tap on the pin that opens a block moves the card to it, a real tap on a pin without one opens its text
        [UnityTest]
        public IEnumerator ARealTapOnANumberedPinWithOpensBlock_JumpsToItsBlock_AndOpensNoText_APinWithoutOneStillOpensItsPopup()
        {
            var opens = OpensBlockOf(NumberedHotspots);
            Assert.AreEqual(5, opens.Count, "precondition: the five authored spots");
            int jumper = opens.FindIndex(o => o.Length > 0);
            int plain = opens.FindIndex(o => o.Length == 0);
            Assert.GreaterOrEqual(jumper, 0, "precondition: the wall's numbered block has a spot that opens a block");
            Assert.GreaterOrEqual(plain, 0, "precondition: and spots that do not");
            string target = opens[jumper];

            yield return ShowHotspots(NumberedHotspots);
            var view = HotspotOf(NumberedHotspots);
            Assert.AreEqual(opens.Count, view.Spots.Count, "every spot is drawn");
            // - the cue: only the spot that jumps wears the arrow (a badge on the pin's corner, inside its tap area), never colour alone
            for (int i = 0; i < opens.Count; i++)
            {
                var pin = view.Spots[i].Pin;
                var badge = pin.Q(className: "card-hotspot__jump");
                Assert.AreEqual(opens[i].Length > 0, pin.ClassListContains(HotspotImageBlockView.PinJumpsClass), "spot " + i + ": marked as a jump only when it opens a block");
                Assert.AreEqual(opens[i].Length > 0 ? DisplayStyle.Flex : DisplayStyle.None, badge.resolvedStyle.display, "spot " + i + ": the arrow badge shows only on a jump");
                Assert.AreEqual(opens[i], view.Spots[i].Jump, "spot " + i + ": the view's target is the wall's");
            }
            var shownBadge = view.Spots[jumper].Pin.Q(className: "card-hotspot__jump");
            Assert.IsTrue(view.Spots[jumper].Pin.worldBound.Contains(shownBadge.worldBound.min + shownBadge.worldBound.size * 0.5f), "the arrow sits inside the pin's own tap area");
            yield return Capture("Hotspot_OpensBlock_numbered_Lamp_390");

            int jumps = NavJumps.Count;
            yield return CardTestInput.TapInView(Scroll, view.Spots[jumper].Pin);
            yield return Landed();
            Assert.AreEqual(jumps + 1, NavJumps.Count, "a tap on the pin: one jump");
            var jump = NavJumps.Last();
            Assert.AreEqual(target, jump.BlockKey, "nav_jump: the block landed on");
            Assert.AreEqual(BuiltInBlocks.HotspotImageKind, jump.Variant, "nav_jump: the asking block's kind");
            float top = TopInViewport(target);
            if (Slot(target).layout.y <= Range - OnePixel) Assert.AreEqual(0f, top, OnePixel, target + "'s slot top meets the viewport's top (offset " + Scroll.scrollOffset.y + ")");
            else Assert.AreEqual(Range, Scroll.scrollOffset.y, OnePixel, target + ": a late block clamps at the end of the range");
            Assert.AreEqual(-1, view.OpenIndex, "the pin moved the card and opened no text of its own");
            Assert.IsFalse(view.Detail.ClassListContains("card-hotspot__detail--open"), "the detail stays closed");

            // - a spot with nothing picked: its popup, no jump
            yield return CardTestInput.TapInView(Scroll, view.Spots[plain].Pin);
            Assert.AreEqual(plain, view.OpenIndex, "a pin without Opens Block opens its title and text");
            Assert.AreEqual(view.Spots[plain].Title, view.DetailTitle.text, "the spot's own title");
            Assert.AreEqual(jumps + 1, NavJumps.Count, "and no second jump");
        }

        // The loupes look: the round close-up is what a visitor taps, and it follows the same rule
        [UnityTest]
        public IEnumerator ARealTapOnALoupeWithOpensBlock_JumpsToItsBlock_AndALoupeWithoutOneOpensItsText()
        {
            var opens = OpensBlockOf(LoupesHotspots);
            int jumper = opens.FindIndex(o => o.Length > 0);
            int plain = opens.FindIndex(o => o.Length == 0);
            Assert.GreaterOrEqual(jumper, 0, "precondition: the wall's loupes block has a spot that opens a block");
            string target = opens[jumper];
            Assert.AreNotEqual(OpensBlockOf(NumberedHotspots).Find(o => o.Length > 0), target, "precondition: a different block than the numbered one's, so each look proves itself");

            yield return ShowHotspots(LoupesHotspots);
            var view = HotspotOf(LoupesHotspots);
            for (int i = 0; i < opens.Count; i++)
                Assert.AreEqual(opens[i].Length > 0, view.Spots[i].Loupe.ClassListContains(HotspotImageBlockView.LoupeJumpsClass), "loupe " + i + ": the arrow only on a jump");
            Assert.AreEqual(PickingMode.Ignore, view.Spots[jumper].Pin.pickingMode, "precondition: on the picture the ring is only a mark; the close-up is the target");
            yield return Capture("Hotspot_OpensBlock_loupes_Lamp_390");

            int jumps = NavJumps.Count;
            yield return CardTestInput.TapInView(Scroll, view.Spots[jumper].Loupe);
            yield return Landed();
            Assert.AreEqual(jumps + 1, NavJumps.Count, "a tap on the close-up: one jump");
            Assert.AreEqual(target, NavJumps.Last().BlockKey, "to the block the wall names");
            Assert.AreEqual(BuiltInBlocks.HotspotImageKind, NavJumps.Last().Variant);
            Assert.AreEqual(-1, view.OpenIndex, "no text opened");

            yield return CardTestInput.TapInView(Scroll, view.Spots[plain].Loupe);
            Assert.AreEqual(plain, view.OpenIndex, "a loupe without Opens Block opens its text");
            Assert.AreEqual(jumps + 1, NavJumps.Count, "and jumps nowhere");
        }

        // A pick that leads nowhere draws no jump: pushed through the live seam like an Editor edit, the spot keeps its popup -- the title block,
        // a pinned footer look, a deleted block and a block waiting for Show After Reading; the same spot given a real target jumps again
        [UnityTest]
        public IEnumerator AnOpensBlockThatLeadsNowhere_DrawsNoJump_TheSpotKeepsItsPopup_AndAGoodPickJumpsAgain()
        {
            var opens = OpensBlockOf(NumberedHotspots);
            int spot = opens.FindIndex(o => o.Length > 0);
            string good = opens[spot];
            var lamp = ConfigCopy().pois.Single(p => p.id == Lamp);
            string header = lamp.card.blocks.First(b => b.kind == BuiltInBlocks.HeaderKind).key;
            string footer = lamp.card.blocks.First(b => b.kind == BuiltInBlocks.ActionsKind && b.variant == BuiltInBlocks.ActionsStickyCta).key;
            string waits = lamp.card.blocks.First(b => b.kind == BuiltInBlocks.KnowledgeCheckKind
                && new BlockFieldReader(b, "en", "en").Flag(BuiltInBlocks.KnowledgeCheckShowAfterViewedField)).key;

            foreach (var (bad, why) in new[] { (header, "the title block"), (footer, "a look pinned to the bottom"), ("block_999", "a deleted block"), (waits, "a block waiting for Show After Reading") })
            {
                yield return Push(c => SetOpensBlock(c, NumberedHotspots, spot, bad));
                yield return ShowHotspots(NumberedHotspots);
                var view = HotspotOf(NumberedHotspots);
                Assert.AreEqual("", view.Spots[spot].Jump, why + ": no jump target");
                Assert.IsFalse(view.Spots[spot].Pin.ClassListContains(HotspotImageBlockView.PinJumpsClass), why + ": no arrow");
                int jumps = NavJumps.Count;
                yield return CardTestInput.TapInView(Scroll, view.Spots[spot].Pin);
                Assert.AreEqual(spot, view.OpenIndex, why + ": the spot opens its own text");
                Assert.AreEqual(jumps, NavJumps.Count, why + ": and nothing jumps");
            }

            yield return Push(c => SetOpensBlock(c, NumberedHotspots, spot, good));
            yield return ShowHotspots(NumberedHotspots);
            var again = HotspotOf(NumberedHotspots);
            Assert.AreEqual(good, again.Spots[spot].Jump, "a good pick jumps again, live");
            int before = NavJumps.Count;
            yield return CardTestInput.TapInView(Scroll, again.Spots[spot].Pin);
            yield return Landed();
            Assert.AreEqual(before + 1, NavJumps.Count, "one jump");
            Assert.AreEqual(good, NavJumps.Last().BlockKey);
        }
    }
}
