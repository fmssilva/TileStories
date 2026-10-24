using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // Phase B of step 15a (_3.2 section 5.6 D3, nav_rails) on the REAL LivingRoomScene with real touches, on The Lamp's own families:
    //   - Auto Nav Block = Rails (the live seam) puts a rail per family right after the header; Lamp - Military (under Show Navigation From)
    //     gets none; the rails are no target of the card's own navigation
    //   - a REAL sideways drag on a rail moves the rail and does NOT scroll the card; a REAL vertical drag on a rail scrolls the card and never
    //     moves the rail (SwipeGrab judges it once); in the pages layout a drag on a rail is the rail's, the page never turns
    //   - the longest family: its All n card is reached by real swipes of one viewport each in no more than the swipes the block allows, and a
    //     real tap on it lands the family's first block
    //   - a REAL tap on a card lands its block (+-1 px) with ONE nav_jump named nav_rails, on a rail at the top and on one lower down
    //   - Peek Next Card governs the rails live: on, the next card shows its slice; off, whole cards only
    //   - renders at 390 px in English and Portuguese: a rail at rest with its cut next card, and swiped
    public class PoiCardNavRailsSceneTests : SearchSceneFixture
    {
        private const string Lamp = "lamp";
        private const string LampMilitary = "lamp_military";
        private static readonly string AutoKey = BlockStackBuilder.AutoNavBlockKey(BuiltInBlocks.NavRailsKind);

        private PoiCardSheetView Sheet => Card.Sheet;
        private NavController Nav => Sheet.Nav;
        private CardNavModel Model => Nav.Model;
        private ScrollView Scroll => Sheet.Stack.Scroll;
        private float OnePixel => Sheet.Layer.panel.visualTree.layout.height / Screen.height;
        private float Range => NavScrollRule.Range(Scroll.contentContainer.layout.height, Scroll.contentViewport.layout.height);
        private NavRailsBlockView View => Sheet.Stack.BoundViews.OfType<NavRailsBlockView>().SingleOrDefault();

        private sealed class RecordingCardEvents : ICardEvents
        {
            public readonly List<CardEvent> Raised = new();
            public void Raise(CardEvent cardEvent) => Raised.Add(cardEvent);
        }

        private RecordingCardEvents _events;
        private List<string> _shippedLanguages;
        private List<CardEvent> NavJumps => _events.Raised.Where(e => e.Kind == CardEventKinds.NavJump).ToList();

        [UnitySetUp]
        public IEnumerator UseAnInMemoryCardStateAndRecordTheEvents()
        {
            Card.State = new CardLocalState(new MemoryCardStateStore(), Session.SearchConfig.wall_id);
            _events = new RecordingCardEvents();
            Card.Events = _events;
            yield break;
        }

        // ---------------- helpers ----------------

        // The wall's navigation for a test, pushed through the live seam: Auto Nav Block `auto`, the card in `language`, Peek Next Card as given
        private IEnumerator UseAuto(string auto, string language = "en", bool peekNext = true, string layout = null)
        {
            var copy = ConfigCopy();
            copy.card_settings.navigation.auto_nav_block = auto;
            copy.card_settings.container.peek_next_card = peekNext;
            if (layout != null) copy.card_settings.navigation.layout = layout;
            _shippedLanguages ??= new List<string>(ConfigCopy().card_settings.languages);
            copy.card_settings.languages = language == "en" ? new List<string>(_shippedLanguages) : new List<string> { language, "en" };
            Session.ApplyCardSettings(copy);
            yield return CardTestInput.Settle(0.3f);
        }

        private IEnumerator Open(string poi, SheetStopRule.Stop stop)
        {
            if (SelectionEventBus.CurrentPoiId != poi)
            {
                SelectionEventBus.Clear();
                yield return CardTestInput.Settle();
                SelectionEventBus.Select(poi);
                yield return CardTestInput.Settle();
            }
            Scroll.scrollOffset = Vector2.zero;
            Sheet.SetStop(stop);
            yield return CardTestInput.Settle();
            Assert.AreEqual(stop, Sheet.Stop, "precondition: the card rests at " + stop);
            yield return Rested();
        }

        // The sheet's height animates in real time: every measure waits for it to rest at its stop's height
        private IEnumerator Rested()
        {
            float rest = Sheet.Stops.HeightOf(Sheet.Stop);
            for (int frame = 0; frame < 600 && Mathf.Abs(Sheet.Root.layout.height - rest) > OnePixel; frame++) yield return null;
            Assert.AreEqual(rest, Sheet.Root.layout.height, OnePixel, "precondition: the sheet rests at its " + Sheet.Stop + " height");
            yield return null;
            yield return null;
        }

        private IEnumerator Landed()
        {
            for (int frame = 0; frame < 600 && Nav.IsJumping; frame++) yield return null;
            Assert.IsFalse(Nav.IsJumping, "the jump came to rest");
            yield return CardTestInput.Settle(0.3f);
        }

        // A block's top lands at the viewport's top (relative, one pixel), or the card's end clamps it
        private void AssertLanded(string block, string name)
        {
            var slot = Sheet.Stack.SlotOfBlock(block);
            if (Sheet.Stack.LandingTop(slot) <= Range - OnePixel)
                Assert.AreEqual(0f, Sheet.Stack.LandingElement(slot).worldBound.yMin - Scroll.contentViewport.worldBound.yMin, OnePixel, name + ": its top meets the viewport's top");
            else
                Assert.AreEqual(Range, Scroll.scrollOffset.y, OnePixel, name + ": a late block clamps at the end of the range");
        }

        // A rail scrolled into the visible part of the card, with its whole strip inside it (a drag or a tap on it needs the finger on it)
        private IEnumerator BringIntoView(NavRailsBlockView.Rail rail)
        {
            // - scrolling a card whose header collapses moves the viewport itself: scroll, let it settle, and scroll again to the settled place
            for (int pass = 0; pass < 2; pass++)
            {
                Scroll.ScrollTo(rail.Root);
                yield return CardTestInput.Settle(0.3f);
            }
            var viewport = Scroll.contentViewport.worldBound;
            var strip = rail.Strip.Viewport.worldBound;
            Assert.IsTrue(viewport.Contains(strip.center) && viewport.Contains(new Vector2(strip.xMin + 2f, strip.yMax - 2f)),
                "precondition: the rail's strip " + strip + " is inside the visible part " + viewport + " (offset " + Scroll.scrollOffset.y + " of " + Range + ")");
        }

        // A real tap by a touch pointer on a point of the card layer
        private IEnumerator TouchTap(Vector2 at)
        {
            yield return CardTestInput.TouchDrag(Sheet.Layer.panel, at, Vector2.zero, frames: 1);
            yield return CardTestInput.Settle();
        }

        // One SWIPE: the finger across one viewport of the rail (the unit NavRailRule counts), from near its right edge to near its left, on a card
        private IEnumerator Swipe(NavRailsBlockView.Rail rail)
        {
            var strip = rail.Strip.Viewport.worldBound;
            var start = new Vector2(strip.xMax - 1f, strip.center.y);
            yield return CardTestInput.TouchDrag(Sheet.Layer.panel, start, new Vector2(-(strip.width - 2f), 0f), frames: 10);
            yield return CardTestInput.Settle(0.2f);
        }

        // ---------------- the tests ----------------

        [UnityTest]
        public IEnumerator AutoNavBlockRails_PutsARailPerFamilyFirstOnTheLampsScroll_NotOnLampMilitary_AndNoneTakesThemAway_Live()
        {
            yield return UseAuto(CardOptions.NavNone);
            yield return Open(Lamp, SheetStopRule.Stop.Full);
            Assert.IsNull(View, "the shipped default (None): no rails");
            string without = string.Join(" / ", Model.Sections.Select(s => s.Key + ":" + string.Join(",", s.Targets)));

            yield return UseAuto(BuiltInBlocks.NavRailsKind);
            yield return Open(Lamp, SheetStopRule.Stop.Full);
            Assert.IsNotNull(View, "Rails, live: The Lamp's open card gains the block");
            var slot = Sheet.Stack.SlotOfBlock(AutoKey);
            Assert.AreEqual(Scroll.contentContainer.IndexOf(Sheet.Stack.Sheets.SheetOf(Sheet.Stack.SlotOfBlock(Model.Targets[0])).Root), Scroll.contentContainer.IndexOf(Sheet.Stack.Sheets.SheetOf(slot).Root) + 1,
                "right before the card's first block (after the header's own hero)");
            Assert.AreEqual(Model.Sections.Count, View.Rails.Count, "a rail per family of The Lamp");
            Assert.AreEqual(8, View.Rails.Count, "precondition: The Lamp's eight families that navigate");
            Assert.AreEqual(without, string.Join(" / ", Model.Sections.Select(s => s.Key + ":" + string.Join(",", s.Targets))),
                "the same sections and targets with the rails as without them");
            CollectionAssert.DoesNotContain(Model.Targets, AutoKey, "the rails are no target");
            for (int i = 0; i < View.Rails.Count; i++)
                Assert.AreEqual(Model.Sections[i].Count, int.Parse(View.Rails[i].TitleCount.text), Model.Sections[i].Key + ": its block count");

            yield return Open(LampMilitary, SheetStopRule.Stop.Full);
            Assert.Less(Model.TargetCount, NavOntologyRule.MinBlocks(ConfigCopy().card_settings.navigation), "precondition: Lamp - Military is under Show Navigation From");
            Assert.IsNull(View, "Lamp - Military gets no rails");

            yield return Open(Lamp, SheetStopRule.Stop.Full);
            yield return UseAuto(CardOptions.NavNone);
            Assert.IsNull(View, "None, live: gone again");
        }

        [UnityTest]
        public IEnumerator ASidewaysDragMovesARail_AndNeverScrollsTheCard_AVerticalDragScrollsTheCard_AndNeverMovesTheRail()
        {
            yield return UseAuto(BuiltInBlocks.NavRailsKind);
            yield return Open(Lamp, SheetStopRule.Stop.Full);
            var rail = View.Rails[0];
            yield return BringIntoView(rail);
            var panel = Sheet.Layer.panel;
            var strip = rail.Strip.Viewport.worldBound;
            Assert.Greater(NavStripRule.MaxOffset(rail.Strip.Row.layout.width, rail.Strip.VisibleWidth), 100f,
                "precondition: About's rail is wider than its strip, so it really moves (row " + rail.Strip.Row.layout.width + " in " + rail.Strip.VisibleWidth + ")");
            var firstCard = rail.ShownCards.First().Button;
            float cardRest = firstCard.worldBound.xMin;

            // - CLEARLY SIDEWAYS, on a card: the rail follows the finger; the card never scrolls
            float y0 = Scroll.scrollOffset.y;
            float scrolled = 0f;
            var start = new Vector2(strip.center.x, strip.center.y);
            CardTestInput.TouchDown(panel, start);
            yield return null;
            for (int step = 1; step <= 10; step++)
            {
                CardTestInput.TouchMove(panel, start + new Vector2(-strip.width * 0.5f * step / 10f, 0f));
                yield return null;
                scrolled = Mathf.Max(scrolled, Mathf.Abs(Scroll.scrollOffset.y - y0));
            }
            CardTestInput.TouchUp(panel, start + new Vector2(-strip.width * 0.5f, 0f));
            yield return CardTestInput.Settle(0.2f);
            Assert.AreEqual(0f, scrolled, OnePixel, "the card never scrolled while the finger went sideways");
            Assert.AreEqual(y0, Scroll.scrollOffset.y, OnePixel, "and rests where it was");
            Assert.Greater(rail.Strip.Offset, strip.width * 0.3f, "the rail followed the finger (offset " + rail.Strip.Offset + ")");
            Assert.AreEqual(cardRest - rail.Strip.Offset, firstCard.worldBound.xMin, OnePixel, "what is DRAWN moved with it: the first card is where the offset puts it");
            Assert.Less(Mathf.Abs(View.Rails[1].Strip.Offset), 0.5f, "no other rail moved");
            Assert.IsEmpty(NavJumps, "a swipe is no jump");

            // - CLEARLY VERTICAL, on the same rail: the card scrolls; the rail stays where it was
            float railBefore = rail.Strip.Offset;
            float y1 = Scroll.scrollOffset.y;
            var down = new Vector2(strip.center.x, strip.center.y);
            float moved = 0f;
            CardTestInput.TouchDown(panel, down);
            yield return null;
            for (int step = 1; step <= 12; step++)
            {
                CardTestInput.TouchMove(panel, down + new Vector2(0f, -Scroll.contentViewport.worldBound.height * 0.3f * step / 12f));
                yield return null;
                moved = Mathf.Max(moved, Mathf.Abs(rail.Strip.Offset - railBefore));
            }
            CardTestInput.TouchUp(panel, down + new Vector2(0f, -Scroll.contentViewport.worldBound.height * 0.3f));
            yield return CardTestInput.Settle(0.4f);
            Assert.Greater(Scroll.scrollOffset.y, y1 + 20f, "a vertical drag on the rail scrolled the card (" + y1 + " -> " + Scroll.scrollOffset.y + ")");
            Assert.AreEqual(0f, moved, 0.01f, "and the rail never moved");
            Assert.IsEmpty(NavJumps, "no jump");
        }

        // NN/g (Budiu 2018): the last item in 3-4 swipes. The Lamp's longest family (About, 18 blocks) takes 8 swipes as plain cards; the rail keeps
        // within the swipes the block allows and ends on an All card. Counted with REAL swipes of one viewport each
        [UnityTest]
        public IEnumerator TheLongestFamily_ReachesItsAllCardInNoMoreSwipesThanTheBlockAllows_AndATapOnItLandsTheFamily()
        {
            yield return UseAuto(BuiltInBlocks.NavRailsKind);
            yield return Open(Lamp, SheetStopRule.Stop.Full);
            var model = Model;
            int longest = model.Sections.ToList().FindIndex(s => s.Targets.Count == model.Sections.Max(x => x.Targets.Count));
            var rail = View.Rails[longest];
            Assert.AreEqual(18, rail.Targets, "precondition: the longest family is About with 18 blocks");
            Assert.IsTrue(rail.Plan.HasAll, "its rail ends on an All card: 18 plain cards would not reach the end in time");
            float step = rail.ShownCards.First().Button.layout.width + rail.ShownCards.First().Button.resolvedStyle.marginRight;
            int asPlainCards = NavRailRule.SwipesToEnd(rail.Targets, step, rail.Strip.VisibleWidth);
            Assert.Greater(asPlainCards, 4, "as plain cards its " + rail.Targets + " blocks would take " + asPlainCards + " swipes");
            yield return BringIntoView(rail);

            int swipes = 0;
            while (swipes < 8 && rail.AllCard.Button.worldBound.xMax > rail.Strip.Viewport.worldBound.xMax + OnePixel)
            {
                float before = rail.Strip.Offset;
                yield return Swipe(rail);
                swipes++;
                Assert.Greater(rail.Strip.Offset, before + 1f, "swipe " + swipes + " moved the rail (" + before + " -> " + rail.Strip.Offset + ")");
            }
            Assert.LessOrEqual(swipes, 4, "the All card of The Lamp's longest family was reached in " + swipes + " real swipes");
            Assert.GreaterOrEqual(swipes, 1, "precondition: it was off screen at rest");
            Assert.LessOrEqual(rail.AllCard.Button.worldBound.xMax, rail.Strip.Viewport.worldBound.xMax + OnePixel, "the All card is inside the strip");
            Assert.AreEqual(rail.Strip.Offset, NavStripRule.MaxOffset(rail.Strip.Row.layout.width, rail.Strip.VisibleWidth), OnePixel * 2f,
                "the rail ends on a whole card: its end is the end of the swipe");

            // - a real tap on the All card lands the family's first block, one nav_jump
            _events.Raised.Clear();
            yield return BringIntoView(rail);
            yield return TouchTap(rail.AllCard.Button.worldBound.center);
            yield return Landed();
            AssertLanded(model.Sections[longest].Targets[0], "the All card");
            Assert.AreEqual(1, NavJumps.Count, "one nav_jump");
            Assert.AreEqual(NavRailsBlockView.ElementKey, NavJumps[0].Variant, "named by the rails block");
            Assert.AreEqual(model.Sections[longest].Targets[0], NavJumps[0].BlockKey);
        }

        [UnityTest]
        public IEnumerator ARealTapOnARailCard_LandsItsBlock_WithOneNavJump_OnTheFirstRailAndOnOneLowerDown()
        {
            yield return UseAuto(BuiltInBlocks.NavRailsKind);
            yield return Open(Lamp, SheetStopRule.Stop.Full);
            foreach (int railIndex in new[] { 0, 3 })
            {
                var rail = View.Rails[railIndex];
                var card = rail.ShownCards.ElementAt(Mathf.Min(1, rail.ShownCards.Count() - 1));
                yield return BringIntoView(rail);
                // - a card the strip does not show cannot be tapped (_3.3 P4.4b: a narrower frame left card 1 off the screen, at x 465)
                Assert.IsTrue(rail.Strip.Viewport.worldBound.Contains(card.Button.worldBound.center), "precondition: rail " + railIndex + "'s card "
                    + card.Button.worldBound + " shows inside its strip " + rail.Strip.Viewport.worldBound);
                _events.Raised.Clear();
                yield return TouchTap(card.Button.worldBound.center);
                yield return Landed();
                AssertLanded(card.BlockKey, "rail " + railIndex + "'s card for " + card.BlockKey);
                Assert.AreEqual(1, NavJumps.Count, "one nav_jump");
                Assert.AreEqual(NavRailsBlockView.ElementKey, NavJumps[0].Variant, "named by the rails block");
                Assert.AreEqual(card.BlockKey, NavJumps[0].BlockKey, "to the card's block");
                Assert.AreEqual(Model.Sections[railIndex].Key, NavJumps[0].Value, "in its family");
                Scroll.scrollOffset = Vector2.zero;
                yield return CardTestInput.Settle(0.3f);
            }
        }

        [UnityTest]
        public IEnumerator InThePagesLayout_ADragOnARailIsTheRailsOwn_ThePageNeverTurns()
        {
            yield return UseAuto(BuiltInBlocks.NavRailsKind, layout: CardOptions.LayoutPages);
            yield return Open(Lamp, SheetStopRule.Stop.Full);
            Assert.AreEqual(CardOptions.LayoutPages, Sheet.Layout.Effective, "precondition: The Lamp is drawn in pages");
            Assert.IsNotNull(View, "the rails (a block of no section) show on the page");
            var rail = View.Rails[Sheet.Layout.ShownSection];
            yield return BringIntoView(rail);
            Assert.Greater(NavStripRule.MaxOffset(rail.Strip.Row.layout.width, rail.Strip.VisibleWidth), 20f, "precondition: the rail moves");
            int page = Sheet.Layout.ShownSection;
            var strip = rail.Strip.Viewport.worldBound;
            var panel = Sheet.Layer.panel;
            var start = new Vector2(strip.center.x, strip.center.y);
            float turned = 0f;
            CardTestInput.TouchDown(panel, start);
            yield return null;
            for (int step = 1; step <= 10; step++)
            {
                CardTestInput.TouchMove(panel, start + new Vector2(-strip.width * 0.4f * step / 10f, 0f));
                yield return null;
                turned = Mathf.Max(turned, Mathf.Abs(Sheet.Pager.Offset));
            }
            CardTestInput.TouchUp(panel, start + new Vector2(-strip.width * 0.4f, 0f));
            yield return CardTestInput.Settle(0.5f);
            Assert.AreEqual(0f, turned, 0.01f, "the page never followed the finger");
            Assert.AreEqual(page, Sheet.Layout.ShownSection, "and never turned");
            Assert.Greater(rail.Strip.Offset, strip.width * 0.2f, "the rail moved (offset " + rail.Strip.Offset + ")");
        }

        [UnityTest]
        public IEnumerator PeekNextCardGovernsTheRails_OnTheNextCardShowsItsSlice_OffWholeCardsOnly_Live()
        {
            yield return UseAuto(BuiltInBlocks.NavRailsKind, peekNext: true);
            yield return Open(Lamp, SheetStopRule.Stop.Full);
            var rail = View.Rails[0];
            float narrowest = CardTestInput.TokenPx("--ts-rail-card");
            float gap = rail.ShownCards.First().Button.resolvedStyle.marginRight;
            float viewport = rail.Strip.VisibleWidth;
            float peek = CardTrackFit.PeekOf(rail.Strip.Viewport, true);
            int whole = CardTrackRule.WholeCells(viewport - peek, narrowest, gap);
            var cells = rail.ShownCards.Select(c => c.Button).Append(rail.AllCard.Button).ToList();
            float edge = rail.Strip.Viewport.worldBound.xMax;
            Assert.AreEqual(peek, edge - cells[whole].worldBound.xMin, OnePixel * 2f, "Peek Next Card on: the next card shows exactly the peek token of itself");

            yield return UseAuto(BuiltInBlocks.NavRailsKind, peekNext: false);
            yield return Open(Lamp, SheetStopRule.Stop.Full);
            rail = View.Rails[0];
            viewport = rail.Strip.VisibleWidth;
            gap = rail.ShownCards.First().Button.resolvedStyle.marginRight;
            whole = CardTrackRule.WholeCells(viewport, narrowest, gap);
            cells = rail.ShownCards.Select(c => c.Button).Append(rail.AllCard.Button).ToList();
            edge = rail.Strip.Viewport.worldBound.xMax;
            Assert.AreEqual(CardTrackRule.CellWidth(viewport, narrowest, gap), cells[0].layout.width, OnePixel, "off: whole cards fill the strip");
            Assert.GreaterOrEqual(cells[whole].worldBound.xMin, edge - OnePixel, "off: the next card starts at the strip's edge, no slice shows");
        }

        [UnityTest]
        public IEnumerator Renders_TheRailsOnTheLamp_AtRestWithTheCutNextCard_AndSwiped_InEnglishAndPortuguese()
        {
            foreach (string language in new[] { "en", "pt" })
            {
                yield return UseAuto(BuiltInBlocks.NavRailsKind, language);
                SelectionEventBus.Clear();
                yield return CardTestInput.Settle();
                yield return Open(Lamp, SheetStopRule.Stop.Full);
                var rail = View.Rails[0];
                // - the rails block's top at the top of the visible part: the picture shows the rails, not the header's hero
                Scroll.scrollOffset = new Vector2(0f, Mathf.Min(Sheet.Stack.TopInScroll(Sheet.Stack.SlotOfBlock(AutoKey)), Range));
                yield return CardTestInput.Settle(0.4f);
                yield return BringIntoView(rail);
                yield return CardGalleryChecks.Render("Nav_Lamp_rails_rest_" + language + "_390");
                yield return Swipe(rail);
                yield return CardGalleryChecks.Render("Nav_Lamp_rails_swiped_" + language + "_390");
                // - the lower rails, each family's words whole
                var lower = View.Rails[4];
                yield return BringIntoView(lower);
                Scroll.scrollOffset = new Vector2(0f, Mathf.Min(Scroll.scrollOffset.y + lower.Root.worldBound.yMin - Scroll.contentViewport.worldBound.yMin - 8f, Range));
                yield return CardTestInput.Settle(0.4f);
                yield return CardGalleryChecks.Render("Nav_Lamp_rails_lower_" + language + "_390");
                Scroll.scrollOffset = Vector2.zero;
            }
        }
    }
}
