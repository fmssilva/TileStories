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

        // How many contiguous family runs a shown stack has (9 families in 15 runs = several families are split up)
        private static int RunCount(List<string> families)
        {
            int runs = 0;
            for (int i = 0; i < families.Count; i++)
                if (i == 0 || families[i] != families[i - 1]) runs++;
            return runs;
        }

        // The family of each block the card really bound, in stack order
        private List<string> ShownFamilies() =>
            Sheet.Stack.BoundKinds.Select(k => BlockRegistry.Shared.TryGet(k, out var d) ? d.Family : "?").ToList();

        // Where a family's blocks sit in the shown stack
        private List<int> PositionsOf(string family)
        {
            var families = ShownFamilies();
            return Enumerable.Range(0, families.Count).Where(i => families[i] == family).ToList();
        }

        // Put the first block that is NOT of `family` at the top of the viewport. That is where the Block Orders really
        // differ on this wall: the card opens with a long About run in every order, and what FOLLOWS it is Stories when
        // the blocks keep their authored places and Media when they are grouped by family.
        // (Two earlier render positions came out pixel-identical -- the top of the card, and the first Media block --
        // because About opens the card in every order and the first Media block is the same gallery either way.)
        private IEnumerator ScrollToFirstBlockAfterTheRunOf(string family)
        {
            var families = ShownFamilies();
            int i = families.FindIndex(f => f != family);
            Assert.GreaterOrEqual(i, 0, "precondition: the card holds a block of another family than '" + family + "'");
            Assert.AreEqual(family, families[0], "precondition: the card really opens with the '" + family + "' run");
            var slot = Sheet.Stack.SlotOf(Sheet.Stack.BoundViews[i]);
            Assert.IsNotNull(slot, "precondition: that block has a slot in the stack");
            float range = Mathf.Max(0f, Sheet.Stack.Scroll.contentContainer.layout.height - Sheet.Stack.Scroll.contentViewport.layout.height);
            Assert.Greater(range, 0f, "precondition: the card is taller than its viewport, so there is something to scroll");
            Sheet.Stack.Scroll.scrollOffset = new Vector2(0f, Mathf.Clamp(slot.layout.y, 0f, range));
            yield return CardTestInput.Settle();
        }

        // _3.2 step 1, Phase B on the real LivingRoomScene: The Lamp authors its two Sources blocks before its actions and its Tier 5
        // blocks, yet its card ends with them under the default Block Order (end_families_last, which is exactly what Sources At The End
        // used to do). Switched live to `authored` they sit where they were written; switched live to `by_family` every family becomes
        // ONE contiguous run and Sources still close the card. Relative order only, on real block keys (40-testing 4.2.4b).
        [UnityTest]
        public IEnumerator BlockOrder_TheLampsCardRendersInAllThreeOrders_AndTheOptionIsLive()
        {
            var lamp = Session.SearchPois.Single(p => p.id == "lamp");
            int authoredAt = lamp.card.blocks.FindIndex(b => b.kind == "sources");
            Assert.Less(authoredAt, lamp.card.blocks.Count - 3, "precondition: the fixture writes the sources in the middle of the card");
            Assert.AreEqual(CardOptions.BlockOrderEndFamiliesLast, Session.CardSettings.navigation.block_order,
                "precondition: the shipped wall never set it, so it reads the default");

            yield return OpenFull("lamp");
            var views = Sheet.Stack.BoundViews.ToList();
            Assert.AreEqual(2, views.OfType<SourcesBlockView>().Count(), "precondition: both Sources blocks are shown");
            // - _3.3 P4.4a: the hero chip is the hero's Listen, not a stack view
            Assert.AreEqual(lamp.card.blocks.Count, views.Count + Sheet.Stack.HeroActionViews.OfType<AudioGuideBlockView>().Count(), "nothing was dropped by ordering them");
            Assert.Greater(ShownFamilies().Distinct().Count(), 3, "precondition: the Lamp's card really spans several families");

            // the default behaves exactly as Sources At The End did
            Assert.IsTrue(views.Skip(views.Count - 2).All(v => v is SourcesBlockView), "default: the two Sources blocks close the card");
            yield return ScrollToFirstBlockAfterTheRunOf(BuiltInFamilies.About);
            yield return CardGalleryChecks.Render("Card_BlockOrder_default_390");
            Sheet.Stack.Scroll.scrollOffset = Vector2.zero;
            yield return CardTestInput.Settle();

            // authored: every block exactly where Card Content wrote it
            yield return Push(c => c.card_settings.navigation.block_order = CardOptions.BlockOrderAuthored);
            views = Sheet.Stack.BoundViews.ToList();
            // - _3.3 P4.4a: a pinned-top block written before it (the hero chip) is the hero's Listen, not a stack view
            int pinnedBefore = lamp.card.blocks.Take(authoredAt).Count(b => b.kind == BuiltInBlocks.AudioGuideKind && b.variant == BuiltInBlocks.AudioGuideHeroChip);
            Assert.AreEqual(authoredAt - pinnedBefore, views.FindIndex(v => v is SourcesBlockView), "authored: the first Sources block is back where the config wrote it");
            Assert.IsFalse(views[views.Count - 1] is SourcesBlockView, "authored: the card no longer ends with sources");
            Assert.AreEqual(SheetStopRule.Stop.Full, Sheet.Stop, "the sheet kept its stop");

            // - precondition for the whole by_family claim: the authored order really DOES scatter a family over several
            //   runs, so "every family is one contiguous run" below is something the default does not already satisfy
            yield return Push(c => c.card_settings.navigation.block_order = CardOptions.BlockOrderEndFamiliesLast);
            var defaultFamilies = ShownFamilies();
            Assert.Greater(RunCount(defaultFamilies), defaultFamilies.Distinct().Count(),
                "precondition: under the default a family IS split over several runs -- " + RunCount(defaultFamilies)
                + " runs for " + defaultFamilies.Distinct().Count() + " families; if these were equal, the card would already "
                + "be grouped and the by_family assertions below would prove nothing");

            // by_family: one contiguous run per family, in the registered family order, Sources last
            yield return Push(c => c.card_settings.navigation.block_order = CardOptions.BlockOrderByFamily);
            var families = ShownFamilies();
            Assert.AreEqual(lamp.card.blocks.Count, families.Count + Sheet.Stack.HeroActionViews.OfType<AudioGuideBlockView>().Count(),
                "by_family: still every block, only reordered (the hero chip: the hero's Listen, _3.3 P4.4a)");
            foreach (string family in families.Distinct())
            {
                var at = PositionsOf(family);
                Assert.AreEqual(at.Count, at[at.Count - 1] - at[0] + 1,
                    "by_family: '" + family + "' must be ONE contiguous run, but its blocks sit at " + string.Join(", ", at));
            }
            Assert.AreEqual(families.Distinct().Count(), RunCount(families),
                "by_family: exactly one run per family (the default needed " + RunCount(defaultFamilies) + " runs for the same card)");

            // - the runs follow the registered family Order, compared relatively (never a hard index)
            var orders = families.Distinct().Select(f => BlockRegistry.Shared.TryGetFamily(f, out var d) ? d.Order : int.MaxValue).ToList();
            CollectionAssert.AreEqual(orders.OrderBy(o => o), orders, "by_family: the runs follow the families' registered Order");
            Assert.IsTrue(Sheet.Stack.BoundViews.Skip(families.Count - 2).All(v => v is SourcesBlockView),
                "by_family: Sources is the At The End family, so it still closes the card");
            yield return ScrollToFirstBlockAfterTheRunOf(BuiltInFamilies.About);
            yield return CardGalleryChecks.Render("Card_BlockOrder_by_family_390");
            Sheet.Stack.Scroll.scrollOffset = Vector2.zero;
            yield return CardTestInput.Settle();

            // back to the default, live
            yield return Push(c => c.card_settings.navigation.block_order = CardOptions.BlockOrderEndFamiliesLast);
            views = Sheet.Stack.BoundViews.ToList();
            Assert.IsTrue(views.Skip(views.Count - 2).All(v => v is SourcesBlockView), "default again: they close the card again");
            Assert.Greater(views.FindIndex(v => v is SourcesBlockView), authoredAt, "default again: the sources moved back down from where they were written");
        }

        // 15.4.6 on the real LivingRoomScene: The Lamp's related carousel (three written points: more than fit whole) shows a slice of its
        // third card at rest while Peek Next Card is on (the shipped default); switched off live, its cards are whole, on again, the slice
        [UnityTest]
        public IEnumerator PeekNextCard_TheLampsCarouselShowsASliceOfTheNextCard_AndTheContainerOptionIsLive()
        {
            Assert.IsTrue(Session.CardSettings.container.peek_next_card, "precondition: the shipped wall never set it, so it reads the default (on)");
            yield return OpenFull("lamp");
            var carousel = Sheet.Stack.BoundViews.OfType<RelatedBlockView>().First(r => r.Cards.Count > 0);
            Sheet.Stack.Scroll.ScrollTo(Sheet.Stack.SlotOf(carousel));
            yield return CardTestInput.Settle(0.2f);
            Assert.Greater(carousel.Track.horizontalScroller.highValue, 0f, "precondition: more cards than fit whole");
            float slice = CardTestInput.TokenPx("--ts-track-peek");
            float tol = CardGalleryChecks.OnePixel(carousel.Track) * 2f + 0.5f;
            Assert.AreEqual(slice, CutAtTheRightEdge(carousel), tol, "on: the strip shows a token-wide slice of the next card");

            yield return Push(c => c.card_settings.container.peek_next_card = false);
            carousel = Sheet.Stack.BoundViews.OfType<RelatedBlockView>().First(r => r.Cards.Count > 0);
            yield return CardTestInput.Settle(0.2f);
            Assert.AreEqual(0f, CutAtTheRightEdge(carousel), tol, "off, live: no card is cut, the strip ends on a card");
            Assert.AreEqual(SheetStopRule.Stop.Full, Sheet.Stop, "the sheet kept its stop");

            yield return Push(c => c.card_settings.container.peek_next_card = true);
            carousel = Sheet.Stack.BoundViews.OfType<RelatedBlockView>().First(r => r.Cards.Count > 0);
            yield return CardTestInput.Settle(0.2f);
            Assert.AreEqual(slice, CutAtTheRightEdge(carousel), tol, "on again: the slice is back");
        }

        // How much of the card the strip's right edge cuts shows in the strip (0 when the edge falls between cards)
        private static float CutAtTheRightEdge(RelatedBlockView carousel)
        {
            Rect strip = carousel.Track.contentViewport.worldBound;
            foreach (var card in carousel.Cards)
            {
                Rect r = card.Box.worldBound;
                if (r.xMin < strip.xMax - 1f && r.xMax > strip.xMax + 1f) return strip.xMax - r.xMin;
            }
            return 0f;
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
