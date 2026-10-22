using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // The card's top navigation slot and its first element, text_tabs (_3.2 step 5a), on the REAL LivingRoomScene with The Lamp's real
    // card and real input: the strip sits between the header and the stack, outside the scroll, and never moves the sheet's stops; a
    // real tap on a tab lands that section's first block at the viewport's top; a real drag of the stack moves the underline; a real
    // sideways drag scrolls the strip and not the card, an up / down one moves the sheet; a card below Show Navigation From has no
    // strip; Top Bar and Show Navigation From are live. Since step 5b the strip shows where CardChromeRule allows it: at The Lamp's Half
    // (open header) the floor hides it, so the strip's own tests act at Full; PoiCardChromeSceneTests owns what shows at which stop.
    public partial class PoiCardTextTabsSceneTests : SearchSceneFixture
    {
        private const string Lamp = "lamp";
        private const string LampMilitary = "lamp_military";

        private PoiCardSheetView Sheet => Card.Sheet;
        private NavController Nav => Sheet.Nav;
        private ScrollView Scroll => Sheet.Stack.Scroll;
        private TextTabsNavView Tabs => Sheet.Slots.TopView as TextTabsNavView;

        private sealed class RecordingCardEvents : ICardEvents
        {
            public readonly List<CardEvent> Raised = new();
            public void Raise(CardEvent cardEvent) => Raised.Add(cardEvent);
        }

        private RecordingCardEvents _events;

        [UnitySetUp]
        public IEnumerator UseAnInMemoryCardStateAndRecordTheEvents()
        {
            Card.State = new CardLocalState(new MemoryCardStateStore(), Session.SearchConfig.wall_id);
            _events = new RecordingCardEvents();
            Card.Events = _events;
            yield break;
        }

        // ---------------- helpers ----------------

        // One screen pixel in panel units: the run's pinned frame decides it, so it is measured, not assumed
        private float OnePixel => Sheet.Layer.panel.visualTree.layout.height / Screen.height;

        private float Range => NavScrollRule.Range(Scroll.contentContainer.layout.height, Scroll.contentViewport.layout.height);

        private List<CardEvent> NavJumps => _events.Raised.Where(e => e.Kind == CardEventKinds.NavJump).ToList();

        private IEnumerator Open(string poiId, SheetStopRule.Stop stop)
        {
            SelectionEventBus.Select(poiId);
            yield return CardTestInput.Settle();
            Sheet.SetStop(stop);
            yield return CardTestInput.Settle();
            Assert.AreEqual(stop, Sheet.Stop, "precondition: the card rests at " + stop);
        }

        private IEnumerator Push(System.Action<WallConfigData> edit)
        {
            var copy = ConfigCopy();
            edit(copy);
            Session.ApplyCardSettings(copy);
            yield return CardTestInput.Settle(0.3f);
        }

        private IEnumerator Landed()
        {
            for (int frame = 0; frame < 600 && Nav.IsJumping; frame++) yield return null;
            Assert.IsFalse(Nav.IsJumping, "the jump came to rest");
            yield return CardTestInput.Settle(0.3f);
        }

        // A finger's tap: touch down and up on the same spot (a touch pointer, what a phone sends)
        private static IEnumerator TouchTap(IPanel panel, Vector2 at) => CardTestInput.TouchDrag(panel, at, Vector2.zero, frames: 1);

        // The peek stop is the header's open top part, measured from the sheet's top edge (PoiCardSheetView.RecomputeStops' own formula,
        // re-derived here from the two elements' screen rects)
        private float MeasuredPeek()
        {
            var peekPart = Sheet.Stack.PeekPart;
            return peekPart.worldBound.yMax + peekPart.resolvedStyle.marginBottom - Sheet.Root.worldBound.yMin;
        }

        private static bool IsShown(VisualElement e) => e.resolvedStyle.display == DisplayStyle.Flex;

        // A part of the chrome takes its place in the card: in the flow and visible (CardChromeView hides it out of the flow, not with display)
        private static bool InTheCard(VisualElement e) =>
            IsShown(e) && e.resolvedStyle.visibility == Visibility.Visible && e.resolvedStyle.position == Position.Relative;

        // The section the reader is in, read from the SCREEN: the spy's own rule over each section's first slot as drawn
        private int SectionUnderTheSpyLine() =>
            CardTestInput.UnderTheSpyLine(Scroll, Nav.Model.Sections.Select(s => Sheet.Stack.SlotOfBlock(s.Targets[0])));

        // Exactly one tab is marked, and it is marked twice: its underline shows and its label is bold; every other tab has neither
        private void AssertMarked(int index, string when)
        {
            Assert.AreEqual(index, Tabs.CurrentIndex, when + ": the marked tab");
            foreach (var tab in Tabs.Tabs)
            {
                bool marked = tab.Index == index;
                Assert.AreEqual(marked, tab.Button.ClassListContains(NavTab.CurrentClass), when + ": tab " + tab.Index + " class");
                Assert.AreEqual(marked, IsShown(tab.Underline), when + ": tab " + tab.Index + " underline");
                Assert.AreEqual(marked ? FontStyle.Bold : FontStyle.Normal, tab.Label.resolvedStyle.unityFontStyleAndWeight,
                    when + ": tab " + tab.Index + " weight (never colour alone)");
            }
        }

        // A tab wholly inside the strip's visible part, after a real sideways drag when it is not
        private IEnumerator BringIntoView(NavTab tab)
        {
            var visible = Tabs.Viewport.worldBound;
            var rect = tab.Button.worldBound;
            if (rect.xMin < visible.xMin - 0.5f || rect.xMax > visible.xMax + 0.5f)
            {
                // - drag the strip so the tab's centre comes to the strip's centre (a sideways move: the strip's own)
                float dx = visible.center.x - rect.center.x;
                yield return CardTestInput.TouchDrag(Sheet.Root.panel, new Vector2(visible.center.x, visible.center.y), new Vector2(dx, 0f));
                yield return null;
            }
            rect = tab.Button.worldBound;
            Assert.GreaterOrEqual(rect.xMin, visible.xMin - 0.5f, "precondition: tab " + tab.Index + " is inside the strip " + rect + " in " + visible);
            Assert.LessOrEqual(rect.xMax, visible.xMax + 0.5f, "precondition: tab " + tab.Index + " is inside the strip " + rect + " in " + visible);
        }

        // ---------------- the slot and the stops ----------------

        // The Lamp's three stops in the run's 390 x 844 frame are the header's: the strip under the header is hidden at peek and changes
        // no stop -- the same with Top Bar = Text Tabs as with None (numbers logged for the report)
        [UnityTest]
        public IEnumerator TheLampsStops_AreTheHeadersPeekPart_AndTheStripMovesNone()
        {
            yield return Open(Lamp, SheetStopRule.Stop.Full);
            Assert.IsNotNull(Tabs, "precondition: the Lamp's card (56 targets) shows the default Top Bar, Text Tabs");
            var withStrip = Sheet.Stops;
            Assert.Greater(withStrip.Peek, 0f, "precondition: the peek was measured");
            float onePixel = OnePixel;
            Assert.AreEqual(MeasuredPeek(), withStrip.Peek, onePixel, "the peek stop is the header's open top part");
            var expected = SheetStopRule.Compute(Sheet.Layer.layout.height, withStrip.Peek, ConfigCopy().card_settings.container.half_max_ratio,
                CardTestInput.TokenPx("--ts-sheet-top-gap"));
            Assert.AreEqual(expected.Half, withStrip.Half, 0.01f, "half comes from the peek and the Half Height Max");
            Assert.AreEqual(expected.Full, withStrip.Full, 0.01f, "full leaves the top gap");
            Assert.IsTrue(InTheCard(Sheet.Slots.Top), "the strip shows at Full");
            Assert.GreaterOrEqual(Sheet.Slots.Top.worldBound.yMin, Sheet.Stack.PeekPart.worldBound.yMax - 0.5f, "and sits under the header's peek part");
            Assert.LessOrEqual(Sheet.Slots.Top.worldBound.yMax, Scroll.worldBound.yMin + 0.5f, "above the stack, outside its scroll");

            Sheet.SetStop(SheetStopRule.Stop.Peek);
            yield return CardTestInput.Settle();
            Assert.IsFalse(InTheCard(Sheet.Slots.Top), "never at peek");
            Assert.AreEqual(withStrip.Peek, Sheet.Stops.Peek, 0.01f, "the peek did not move when the strip went");

            yield return Push(c => c.card_settings.navigation.top = CardOptions.NavNone);
            Sheet.SetStop(SheetStopRule.Stop.Half);
            yield return CardTestInput.Settle();
            Assert.IsNull(Sheet.Slots.TopView, "precondition: Top Bar None, no strip");
            Assert.AreEqual(withStrip.Peek, Sheet.Stops.Peek, 0.01f, "peek: the same with and without the strip");
            Assert.AreEqual(withStrip.Half, Sheet.Stops.Half, 0.01f, "half: the same");
            Assert.AreEqual(withStrip.Full, Sheet.Stops.Full, 0.01f, "full: the same");
            Debug.Log("[Card] Lamp stops at " + Sheet.Layer.layout.size + ": peek " + withStrip.Peek.ToString("F2") + ", half "
                + withStrip.Half.ToString("F2") + ", full " + withStrip.Full.ToString("F2") + " (with the strip and without)");
        }

        // ---------------- taps, the spy and drags ----------------

        // The composition, from the selection a marker tap makes (MarkerSelectable -> SelectionEventBus.Select; the marker's own tap is
        // SearchSceneTests'): the REAL host opens the card, every tab -- each brought into view by a real sideways drag when the strip hides
        // it -- is tapped by a real finger, and its section's first block lands at the viewport's top (+-1 px; the range end for a late
        // section), that tab marked, one nav_jump naming text_tabs. At Full: the strip's stop on The Lamp (the raise from a lower stop is
        // PoiCardNavSceneTests')
        [UnityTest]
        public IEnumerator ARealTapOnEveryTab_LandsItsSectionsFirstBlockAtTheViewportTop_AndMarksIt()
        {
            yield return Open(Lamp, SheetStopRule.Stop.Full);
            Assert.AreEqual(Lamp, Card.ShownPoiId, "the real host shows The Lamp's card");
            Assert.IsNotNull(Tabs, "precondition: the strip is there");
            Assert.AreEqual(Nav.Model.Sections.Count, Tabs.Tabs.Count, "one tab per section of the model");
            Assert.GreaterOrEqual(Tabs.Tabs.Count, 5, "precondition: the Lamp's card has many sections");

            for (int i = Tabs.Tabs.Count - 1; i >= 0; i--)
            {
                var tab = Tabs.Tabs[i];
                yield return BringIntoView(tab);
                int jumpsBefore = NavJumps.Count;
                yield return TouchTap(Sheet.Root.panel, tab.Button.worldBound.center);
                yield return Landed();

                var section = Nav.Model.Sections[i];
                var slot = Sheet.Stack.SlotOfBlock(section.Targets[0]);
                float top = slot.worldBound.yMin - Scroll.contentViewport.worldBound.yMin;
                Assert.AreEqual(SheetStopRule.Stop.Full, Sheet.Stop, section.Key + ": the card stays at Full");
                if (slot.layout.y <= Range - OnePixel)
                    Assert.AreEqual(0f, top, OnePixel, section.Key + ": its first block's top meets the viewport's top");
                else
                    Assert.AreEqual(Range, Scroll.scrollOffset.y, OnePixel, section.Key + ": a late section clamps at the end of the range");
                Assert.AreEqual(jumpsBefore + 1, NavJumps.Count, section.Key + ": one nav_jump");
                Assert.AreEqual(BuiltInNavElements.TextTabs, NavJumps.Last().Variant, "the event names the element");
                Assert.AreEqual(section.Key, NavJumps.Last().Value);
                if (slot.layout.y <= Range - OnePixel) AssertMarked(i, section.Key);
            }
        }

        // A real finger drags the stack: the underline follows the section the reader is in (read from the screen), a tab at a time
        [UnityTest]
        public IEnumerator ARealDragOfTheStack_MovesTheUnderline_ToTheSectionOnScreen()
        {
            yield return Open(Lamp, SheetStopRule.Stop.Full);
            Assert.IsNotNull(Tabs);
            AssertMarked(0, "at the top");
            var viewport = Scroll.contentViewport.worldBound;
            var marked = new HashSet<int> { 0 };
            for (int drag = 0; drag < 6; drag++)
            {
                float before = Scroll.scrollOffset.y;
                yield return CardTestInput.TouchDrag(Scroll.panel, new Vector2(viewport.center.x, viewport.yMax - 40f), new Vector2(0f, -viewport.height * 0.6f));
                yield return CardTestInput.Settle(0.6f);
                Assert.Greater(Scroll.scrollOffset.y, before, "precondition: the real drag scrolled the stack");
                int onScreen = SectionUnderTheSpyLine();
                AssertMarked(onScreen, "after drag " + (drag + 1));
                marked.Add(onScreen);
                // - the marked tab is kept in the strip's view as the reader moves on
                var rect = Tabs.Tabs[onScreen].Button.worldBound;
                Assert.GreaterOrEqual(rect.xMin, Tabs.Viewport.worldBound.xMin - 0.5f, "the marked tab is in view");
                Assert.LessOrEqual(rect.xMax, Tabs.Viewport.worldBound.xMax + 0.5f, "the marked tab is in view");
            }
            Assert.Greater(marked.Count, 1, "the underline really moved to another section");
        }

        // A real sideways drag on the strip scrolls the strip by the finger's travel and leaves the card where it was: no stop change, no
        // scroll of the stack, no jump (the tab under the finger was never clicked)
        [UnityTest]
        public IEnumerator ARealSidewaysDragOnTheStrip_ScrollsTheStrip_AndNotTheCard()
        {
            yield return Open(Lamp, SheetStopRule.Stop.Full);
            float max = NavStripRule.MaxOffset(Tabs.Row.layout.width, Tabs.Viewport.contentRect.width);
            Assert.Greater(max, 100f, "precondition: the strip is wider than the screen (" + Tabs.Row.layout.width + " in " + Tabs.Viewport.contentRect.width + ")");
            Assert.AreEqual(0f, Tabs.Offset, "precondition: the strip starts at its start");
            float height = Sheet.TargetHeight, stack = Scroll.scrollOffset.y;
            int jumps = NavJumps.Count;
            var start = Tabs.Viewport.worldBound.center;

            yield return CardTestInput.TouchDrag(Sheet.Root.panel, start, new Vector2(-100f, 6f));
            yield return CardTestInput.Settle(0.3f);
            Assert.AreEqual(100f, Tabs.Offset, OnePixel, "the strip followed the finger's 100 sideways");
            Assert.AreEqual(SheetStopRule.Stop.Full, Sheet.Stop, "the card stayed at Full");
            Assert.AreEqual(height, Sheet.TargetHeight, 0.01f, "the sheet did not move");
            Assert.AreEqual(stack, Scroll.scrollOffset.y, 0.01f, "the stack did not scroll");
            Assert.AreEqual(jumps, NavJumps.Count, "no tab was clicked by the drag");
            Assert.IsFalse(Nav.IsJumping);

            yield return CardTestInput.TouchDrag(Sheet.Root.panel, start, new Vector2(-2000f, 0f));
            yield return CardTestInput.Settle(0.3f);
            Assert.AreEqual(max, Tabs.Offset, OnePixel, "a drag past the end stops at it");
            Assert.LessOrEqual(Tabs.Tabs.Last().Button.worldBound.xMax, Tabs.Viewport.worldBound.xMax + 0.5f, "where the last tab ends inside the strip");
            yield return CardTestInput.TouchDrag(Sheet.Root.panel, start, new Vector2(2000f, 0f));
            yield return CardTestInput.Settle(0.3f);
            Assert.AreEqual(0f, Tabs.Offset, OnePixel, "and past the start at it");
            Assert.AreEqual(jumps, NavJumps.Count, "still no jump");
        }

        // A down drag that starts on a tab moves the sheet like one on the header: Full -> Half, no jump (the strip shows at The Lamp's
        // Full; the up drag from Half is the same claim the other way, and the Lamp's Half has no strip since step 5b)
        [UnityTest]
        public IEnumerator ADownDragOnTheStrip_LowersTheSheet_LikeTheHeader()
        {
            yield return Open(Lamp, SheetStopRule.Stop.Full);
            int jumps = NavJumps.Count;
            float offset = Tabs.Offset;
            yield return CardTestInput.Drag(Tabs.Tabs[0].Button, Sheet.Stops.Full - Sheet.Stops.Half, slowSheet: Sheet);
            yield return CardTestInput.Settle();
            Assert.AreEqual(SheetStopRule.Stop.Half, Sheet.Stop, "the down drag lowered the card");
            Assert.AreEqual(jumps, NavJumps.Count, "and was no tap on the tab it started on");
            Assert.AreEqual(offset, Tabs.Offset, 0.01f, "the strip did not scroll");
        }

        // ---------------- Show Navigation From and Top Bar, live ----------------

        // Lamp - Military (4 targets) is under the default Show Navigation From (8): no strip at Half or Full. Lowering the row to 2 on the
        // running wall gives it one tab per section; Top Bar = None takes the strip off The Lamp too
        [UnityTest]
        public IEnumerator ACardBelowShowNavigationFrom_HasNoStrip_AndBothRowsAreLive()
        {
            yield return Open(LampMilitary, SheetStopRule.Stop.Half);
            Assert.Less(Nav.Model.TargetCount, ConfigCopy().card_settings.navigation.min_blocks, "precondition: under Show Navigation From");
            foreach (var stop in new[] { SheetStopRule.Stop.Half, SheetStopRule.Stop.Full })
            {
                Sheet.SetStop(stop);
                yield return CardTestInput.Settle();
                Assert.IsNull(Sheet.Slots.TopView, stop + ": no element");
                Assert.IsFalse(InTheCard(Sheet.Slots.Top), stop + ": nothing shown");
            }

            yield return Push(c => c.card_settings.navigation.min_blocks = 2);
            Assert.IsNotNull(Tabs, "Show Navigation From 2: the short card has its strip");
            Assert.AreEqual(Nav.Model.Sections.Count, Tabs.Tabs.Count);
            Assert.IsTrue(InTheCard(Sheet.Slots.Top));
            var labels = Tabs.Tabs.Select(t => t.Label.text).ToList();
            var strings = new CardStrings(Card.StringTable.Entries(), CardStringSources.Shared.Entries(), ConfigCopy().card_settings.strings, "en", "en");
            CollectionAssert.AreEqual(Nav.Model.Sections.Select(s => strings.Get(s.LabelKey)).ToList(), labels, "each tab says its section's word");

            yield return Open(Lamp, SheetStopRule.Stop.Half);
            Assert.IsNotNull(Tabs);
            yield return Push(c => c.card_settings.navigation.top = CardOptions.NavNone);
            Assert.IsNull(Sheet.Slots.TopView, "Top Bar None on the running wall: the strip is gone");
            Assert.IsFalse(InTheCard(Sheet.Slots.Top));
            yield return Push(c => c.card_settings.navigation.top = BuiltInNavElements.TextTabs);
            Assert.IsNotNull(Tabs, "and back");
        }

        // ---------------- the look at the phone width ----------------

        // The Lamp at 390 px with the strip at Full (its Half shows content only since step 5b: Chrome_Lamp_half_default_390.png,
        // PoiCardChromeSceneTests): rendered for the vision pass, and checked here -- every tab at least the touch token in both
        // directions, its word whole inside it, the strip inside the card
        [UnityTest]
        public IEnumerator TheLampsStrip_AtFull_HasWholeWordsAndFullSizeTargets_AndIsRendered()
        {
            float touch = CardTestInput.TokenPx("--ts-touch-target");
            yield return Open(Lamp, SheetStopRule.Stop.Full);
            Assert.IsTrue(InTheCard(Sheet.Slots.Top), "precondition: the strip shows at Full");
            var card = Sheet.Root.worldBound;
            Assert.GreaterOrEqual(Tabs.Viewport.worldBound.xMin, card.xMin - 0.5f, "full: the strip starts inside the card");
            Assert.LessOrEqual(Tabs.Viewport.worldBound.xMax, card.xMax + 0.5f, "full: and ends inside it");
            foreach (var tab in Tabs.Tabs)
            {
                Assert.GreaterOrEqual(tab.Button.layout.height, touch - OnePixel, tab.Label.text + ": tall enough to tap");
                Assert.GreaterOrEqual(tab.Button.layout.width, touch - OnePixel, tab.Label.text + ": wide enough to tap");
                var words = tab.Label.MeasureTextSize(tab.Label.text, 0, VisualElement.MeasureMode.Undefined, 0, VisualElement.MeasureMode.Undefined);
                Assert.GreaterOrEqual(tab.Label.layout.width + 0.5f, words.x, tab.Label.text + ": the whole word fits (never clipped)");
            }
            yield return CardGalleryChecks.Render("Nav_Lamp_text_tabs_full_390");
        }
    }
}
