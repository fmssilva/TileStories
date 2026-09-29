using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace TileStories.Tests
{
    // The open card follows live edits (_3.1 step 12), on the REAL LivingRoomScene: what LivePlayModeCardApplier hands to the running wall
    // (WallSession.ApplyCardSettings, the runtime seam a PlayMode test can reach; the applier and the real window are proven in EditMode by
    // LivePlayModeCardTests) rebinds the open card in place -- new text, a block added or deleted, another look -- with the sheet's stop and
    // the stack's scroll kept, and never touches a POI's name, category, summary or keywords. Then the developer-only demo card: off by
    // default, and when on it opens the chosen POI's card at the chosen stop by itself.
    public class PoiCardLiveUpdateTests : SearchSceneFixture
    {
        private PoiCardSheetView Sheet => Card.Sheet;

        private IEnumerator OpenFull(string poiId)
        {
            SelectionEventBus.Select(poiId);
            yield return CardTestInput.Settle();
            Sheet.SetStop(SheetStopRule.Stop.Full);
            yield return CardTestInput.Settle();
        }

        // Push an edit the way the applier does: on a private copy of the running config, through the wall's own seam
        private IEnumerator Push(Action<WallConfigData> edit)
        {
            var copy = ConfigCopy();
            edit(copy);
            Session.ApplyCardSettings(copy);
            yield return CardTestInput.Settle(0.3f);
        }

        private static BlockInstanceData LampBlock(WallConfigData config, string kind) =>
            config.pois.Single(p => p.id == "lamp").card.blocks.First(b => b.kind == kind);

        private static void SetHeading(BlockInstanceData block, string text)
        {
            var field = block.fields.Find(f => f.key == BlockKindDefinition.HeadingField);
            if (field == null) block.fields.Insert(0, field = new BlockFieldValue { key = BlockKindDefinition.HeadingField });
            field.text = new List<LocalizedEntry> { new() { lang = "en", value = text } };
        }

        private IEnumerator ScrollDown()
        {
            var late = Sheet.Stack.BoundViews.OfType<SourcesBlockView>().First();
            Sheet.Stack.Scroll.ScrollTo(Sheet.Stack.SlotOf(late));
            yield return CardTestInput.Settle(0.2f);
        }

        // ---------------- the open card follows the edit ----------------

        [UnityTest]
        public IEnumerator AnEditedText_ShowsOnTheOpenCard_TheStopAndTheScrollAndThePointAreKept()
        {
            yield return OpenFull("lamp");
            yield return ScrollDown();
            float scrolled = Sheet.Stack.Scroll.scrollOffset.y;
            Assert.Greater(scrolled, 300f, "precondition: the card is scrolled well down");
            var facts = Sheet.Stack.BoundViews.OfType<QuickFactsBlockView>().First();
            string before = Sheet.Stack.HeadingOf(facts).text;

            yield return Push(c => SetHeading(LampBlock(c, "quick_facts"), "A live heading"));

            var after = Sheet.Stack.BoundViews.OfType<QuickFactsBlockView>().First();
            Assert.AreEqual("A live heading", Sheet.Stack.HeadingOf(after).text, "the edited heading is on the open card (was '" + before + "')");
            Assert.AreEqual(SheetStopRule.Stop.Full, Sheet.Stop, "the sheet kept its stop");
            Assert.AreEqual(scrolled, Sheet.Stack.Scroll.scrollOffset.y, 2f, "the stack kept its scroll: the reader is not thrown back to the top");
            Assert.AreEqual("lamp", Card.ShownPoiId, "the same point");
            Assert.IsTrue(Sheet.IsOpen);
        }

        [UnityTest]
        public IEnumerator ABlockAddedAndDeleted_ReachesTheOpenCard_InItsPlace()
        {
            yield return OpenFull("lamp");
            int views = Sheet.Stack.BoundViews.Count;
            int buttons = Sheet.Stack.BoundViews.OfType<ShowOnWallBlockView>().Count();

            yield return Push(c =>
            {
                var blocks = c.pois.Single(p => p.id == "lamp").card.blocks;
                blocks.Insert(blocks.FindIndex(b => b.kind == "sources"), new BlockInstanceData { key = "block_live", kind = "show_on_wall" });
            });
            Assert.AreEqual(views + 1, Sheet.Stack.BoundViews.Count, "the new block is on the open card");
            var shown = Sheet.Stack.BoundViews.ToList();
            Assert.AreEqual(buttons + 1, shown.OfType<ShowOnWallBlockView>().Count(), "and it is a Show On Wall block");
            Assert.Less(shown.FindLastIndex(v => v is ShowOnWallBlockView), shown.FindIndex(v => v is SourcesBlockView), "in the place the config gave it: before the sources");

            yield return Push(c =>
            {
                var blocks = c.pois.Single(p => p.id == "lamp").card.blocks;
                blocks.RemoveAll(b => b.key == "block_live");
            });
            Assert.AreEqual(views, Sheet.Stack.BoundViews.Count, "deleted: the card is as it was");
            Assert.AreEqual(buttons, Sheet.Stack.BoundViews.OfType<ShowOnWallBlockView>().Count());
        }

        [UnityTest]
        public IEnumerator AnotherVariant_IsDrawnOnTheOpenCard()
        {
            yield return OpenFull("lamp");
            // - the Today Map has two looks (static, bridge) and its view names the look on its root
            BlockRegistry.Shared.TryGet("today_map", out var definition);
            Assert.GreaterOrEqual(definition.Variants.Count, 2, "precondition: the kind has another look to change to");
            string current = BlockLibraryRule.DefaultVariant(Session.CardSettings, definition);
            string other = definition.Variants.First(v => v != current);
            var view = Sheet.Stack.BoundViews.OfType<TodayMapBlockView>().First();
            Assert.IsTrue(view.Root.ClassListContains("card-today--" + current), "precondition: the Block Library default look");

            yield return Push(c => LampBlock(c, "today_map").variant = other);

            var after = Sheet.Stack.BoundViews.OfType<TodayMapBlockView>().First();
            Assert.IsTrue(after.Root.ClassListContains("card-today--" + other), "the block is drawn in its new look");
            Assert.IsFalse(after.Root.ClassListContains("card-today--" + current), "and not in the old one");
            yield return Push(c => LampBlock(c, "today_map").variant = "");
            Assert.IsTrue(Sheet.Stack.BoundViews.OfType<TodayMapBlockView>().First().Root.ClassListContains("card-today--" + current), "an empty variant: the Block Library's default again");
        }

        [UnityTest]
        public IEnumerator TheCardSettings_ReachTheOpenCard_TheHalfCapTheCloseWordingAndTheBlockLibrary()
        {
            yield return OpenFull("lamp");
            Sheet.SetStop(SheetStopRule.Stop.Half);
            yield return CardTestInput.Settle();
            // - the peek content (title, chip, the pinned audio hero chip) does not change with the ratio: work out what the new
            //   ratio must give directly from the rule itself (SheetStopRule.Compute), rather than a loose "smaller than before"
            //   guess -- with a pinned block this tall, a looser ratio can already be clamped up to peek, so "smaller" is not
            //   always true; "exactly what the formula says" always is (--ts-sheet-top-gap is 48, DraggingUp_GoesToHalf's own check)
            float expectedHalfAfter = SheetStopRule.Compute(Sheet.Layer.layout.height, Sheet.Stops.Peek, CardContainerSettings.HalfMaxRatioMin, 48f).Half;
            string closeBefore = Sheet.CloseButton.tooltip;
            Assert.AreEqual("Close", closeBefore, "precondition: the framework's wording");
            Assert.Greater(Sheet.Stack.BoundViews.OfType<QuickFactsBlockView>().Count(), 0, "precondition: The Lamp shows quick facts");

            yield return Push(c =>
            {
                c.card_settings.container.half_max_ratio = CardContainerSettings.HalfMaxRatioMin;
                c.card_settings.strings.Add(new CardStringEntry { key = CardStrings.Keys.Close, text = new List<LocalizedEntry> { new() { lang = "en", value = "Dismiss" } } });
                c.card_settings.kinds.Add(new BlockKindSetting { kind = "quick_facts", enabled = false });
            });
            Assert.AreEqual("Dismiss", Sheet.CloseButton.tooltip, "the wall's Card Texts wording reached the close button");
            Assert.AreEqual(SheetStopRule.Stop.Half, Sheet.Stop, "the stop is kept");
            Assert.AreEqual(expectedHalfAfter, Sheet.Stops.Half, 1f, "the Half Height Max is the new one (SheetStopRule.Compute over the unchanged peek content)");
            Assert.AreEqual(0, Sheet.Stack.BoundViews.OfType<QuickFactsBlockView>().Count(), "the Block Library switched the kind off: its blocks are gone");
        }

        [UnityTest]
        public IEnumerator TheFirstLivePushOfARun_AppliesEveryDomainOnce_ButTheDemosBeingOffClosesNoCard_AndAMarkerSwapStillClearsTheSelection()
        {
            // - found by the real-window test: the first push of a Play run hands every domain its settings, and the demo domains rebuilt the marker
            //   set and cleared the selection each time, so the developer's first edit closed the card being read
            yield return OpenFull("lamp");
            var config = ConfigCopy();
            Session.ApplyDemoField(config.demo_field);
            Session.ApplyDisplacementDemo(config.displacement_demo);
            Session.ApplySearchDemo(config.search_demo);
            yield return CardTestInput.Settle(0.3f);
            Assert.AreEqual("lamp", SelectionEventBus.CurrentPoiId, "no demo is on: the marker set did not change, so nothing is deselected");
            Assert.IsTrue(Sheet.IsOpen, "and the open card stays open");
            Assert.AreEqual(SheetStopRule.Stop.Full, Sheet.Stop);

            // - a demo really replacing the marker set is still a new search set: the selection goes (the rule this fix kept)
            var on = ConfigCopy().demo_field;
            on.enabled = true;
            Session.ApplyDemoField(on);
            yield return CardTestInput.Settle(0.3f);
            Assert.IsNull(SelectionEventBus.CurrentPoiId, "the LOD demo field replaced the markers: nothing stays selected across the swap");
            Assert.IsFalse(Sheet.IsOpen);
            var off = ConfigCopy().demo_field;
            off.enabled = false;
            Session.ApplyDemoField(off);
            yield return CardTestInput.Settle(0.3f);
        }

        [UnityTest]
        public IEnumerator EnableDetailCardOff_ClosesTheOpenCard_AndOnAgain_ItStaysClosedUntilAPointIsSelected()
        {
            yield return OpenFull("lamp");
            yield return Push(c => c.card_settings.enabled = false);
            Assert.IsFalse(Sheet.IsOpen, "off: the open card closes");
            yield return Push(c => c.card_settings.enabled = true);
            Assert.IsFalse(Sheet.IsOpen, "on again: nothing reopens by itself");
            SelectionEventBus.Clear();
            yield return CardTestInput.Settle();
            SelectionEventBus.Select("lamp");
            yield return CardTestInput.Settle();
            Assert.IsTrue(Sheet.IsOpen, "the next selection opens it");
        }

        [UnityTest]
        public IEnumerator AnEditWhileTheCardIsClosed_ChangesNothingOnScreen_AndTheNextOpenShowsIt()
        {
            Assert.IsFalse(Sheet.IsOpen, "precondition: no card open");
            yield return Push(c => SetHeading(LampBlock(c, "quick_facts"), "Heading for later"));
            Assert.IsFalse(Sheet.IsOpen, "a live edit never opens a card");
            Assert.IsNull(Card.ShownPoiId);
            yield return OpenFull("lamp");
            Assert.AreEqual("Heading for later", Sheet.Stack.HeadingOf(Sheet.Stack.BoundViews.OfType<QuickFactsBlockView>().First()).text, "the next open shows the edit");
        }

        [UnityTest]
        public IEnumerator ACardPush_NeverTouchesAPointsNameCategorySummaryOrKeywords()
        {
            var lamp = Session.SearchPois.Single(p => p.id == "lamp");
            var painting = Session.SearchPois.Single(p => p.id == "painting");
            string[] before = { lamp.name, lamp.category, lamp.summary, string.Join(",", lamp.search_keywords), lamp.hierarchy_level_key, painting.name, painting.summary };

            yield return Push(c =>
            {
                // - the edit the developer made: a card. The rest of this copy is deliberately different, as if another window edit rode along
                SetHeading(LampBlock(c, "quick_facts"), "Only the card");
                var copyLamp = c.pois.Single(p => p.id == "lamp");
                copyLamp.name = "Not the lamp";
                copyLamp.category = "religious";
                copyLamp.summary = "Not the summary";
                copyLamp.search_keywords.Add("intruder");
                copyLamp.hierarchy_level_key = "level_5";
                c.pois.Single(p => p.id == "painting").name = "Not the painting";
            });

            string[] after = { lamp.name, lamp.category, lamp.summary, string.Join(",", lamp.search_keywords), lamp.hierarchy_level_key, painting.name, painting.summary };
            CollectionAssert.AreEqual(before, after, "a card push carries the card only: id, name, category, summary, keywords and level stay as they are");
            Assert.AreEqual("Only the card", BlockFieldValueOf(lamp, "quick_facts"), "while the card edit did land on the running point");
        }

        private static string BlockFieldValueOf(POIData poi, string kind) =>
            new BlockFieldReader(poi.card.blocks.First(b => b.kind == kind), "en", "en").Text(BlockKindDefinition.HeadingField);

        // ---------------- the developer-only demo card ----------------

        private static void Demo(WallConfigData c, bool enabled, string poi, string stop)
        {
            c.card_settings.demo_card.enabled = enabled;
            c.card_settings.demo_card.poi_id = poi;
            c.card_settings.demo_card.stop = stop;
        }

        [UnityTest]
        public IEnumerator TheDemoCard_IsOffByDefault_TheShippedWallOpensNoCardByItself()
        {
            Assert.IsFalse(Session.CardSettings.demo_card.enabled, "the shipped config has it OFF");
            yield return CardTestInput.Settle();
            Assert.IsFalse(Sheet.IsOpen, "no card opens by itself");
            Assert.IsNull(SelectionEventBus.CurrentPoiId);
        }

        [UnityTest]
        public IEnumerator TheDemoCard_WhenSwitchedOn_OpensThatPointsCard_AtTheChosenStop()
        {
            yield return Push(c => Demo(c, true, "lamp_military", CardOptions.StopFull));
            Assert.AreEqual("lamp_military", Card.ShownPoiId, "the chosen point's card opened by itself");
            Assert.AreEqual("lamp_military", SelectionEventBus.CurrentPoiId, "through the selection bus, like a marker tap");
            Assert.IsTrue(Sheet.IsOpen);
            Assert.AreEqual(SheetStopRule.Stop.Full, Sheet.Stop, "at the chosen stop");

            yield return Push(c => Demo(c, true, "lamp_military", CardOptions.StopPeek));
            Assert.AreEqual(SheetStopRule.Stop.Peek, Sheet.Stop, "another Demo Stop moves the open card");
            Assert.AreEqual("lamp_military", Card.ShownPoiId);

            yield return Push(c => Demo(c, true, "lamp", CardOptions.StopHalf));
            Assert.AreEqual("lamp", Card.ShownPoiId, "another Demo Point opens that point's card");
            Assert.AreEqual(SheetStopRule.Stop.Half, Sheet.Stop);
        }

        [UnityTest]
        public IEnumerator TheDemoCard_ReopensOnlyWhenItsRequestChanges_AVisitorsCloseSticksThroughOtherEdits()
        {
            yield return Push(c => Demo(c, true, "lamp", CardOptions.StopHalf));
            Assert.IsTrue(Sheet.IsOpen);
            SelectionEventBus.Clear();
            yield return CardTestInput.Settle();
            Assert.IsFalse(Sheet.IsOpen, "the developer closed the card");

            yield return Push(c =>
            {
                Demo(c, true, "lamp", CardOptions.StopHalf);
                SetHeading(LampBlock(c, "quick_facts"), "Unrelated edit");
            });
            Assert.IsFalse(Sheet.IsOpen, "an unrelated live edit does not drag the demo card back");

            yield return Push(c => Demo(c, true, "lamp", CardOptions.StopFull));
            Assert.IsTrue(Sheet.IsOpen, "a changed request opens it again");
            Assert.AreEqual(SheetStopRule.Stop.Full, Sheet.Stop);
        }

        [UnityTest]
        public IEnumerator TheDemoCard_Off_NoPointPicked_OrAPointTheWallLacks_OpensNothing_AndSwitchingItOffKeepsTheOpenCard()
        {
            yield return Push(c => Demo(c, false, "lamp", CardOptions.StopHalf));
            Assert.IsFalse(Sheet.IsOpen, "off: nothing opens");
            yield return Push(c => Demo(c, true, "", CardOptions.StopHalf));
            Assert.IsFalse(Sheet.IsOpen, "on with no Demo Point: nothing opens");
            yield return Push(c => Demo(c, true, "no_such_point", CardOptions.StopHalf));
            Assert.IsFalse(Sheet.IsOpen, "a point the wall does not have: nothing opens");
            Assert.IsNull(SelectionEventBus.CurrentPoiId);

            yield return Push(c => Demo(c, true, "lamp", CardOptions.StopHalf));
            Assert.IsTrue(Sheet.IsOpen);
            yield return Push(c => Demo(c, false, "lamp", CardOptions.StopHalf));
            Assert.IsTrue(Sheet.IsOpen, "switching the demo off leaves the card the developer is looking at");
            Assert.AreEqual("lamp", Card.ShownPoiId);
        }

        [UnityTest]
        public IEnumerator TheDemoCard_AnAlreadySelectedPoint_IsNotDeselectedByTheDemo()
        {
            yield return OpenFull("lamp");
            Assert.AreEqual("lamp", SelectionEventBus.CurrentPoiId);
            yield return Push(c => Demo(c, true, "lamp", CardOptions.StopPeek));
            Assert.AreEqual("lamp", SelectionEventBus.CurrentPoiId, "selecting the selected point would clear it: the demo must not");
            Assert.IsTrue(Sheet.IsOpen);
            Assert.AreEqual(SheetStopRule.Stop.Peek, Sheet.Stop, "it only moves the open card to the demo's stop");
        }
    }
}
