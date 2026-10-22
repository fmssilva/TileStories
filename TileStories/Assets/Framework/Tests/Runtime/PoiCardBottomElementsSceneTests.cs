using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // Phase B of the step 7b bottom elements (_3.2 section 5.2) on the REAL LivingRoomScene with The Lamp's real card (56 targets, 8 families,
    // its sticky footer, Grouped By Family) and real fingers (CardTestInput, 4.2.3's focus settings); each element is made the wall's Bottom
    // Bar through the live seam (the Editor's own path):
    //   - dock: after real drags through the card the widened item is the section ON SCREEN (the spy's, never a tap's), the pill never
    //     grows past the card -- in Portuguese at 3, 4 and 5 slots -- and it slides away and back with the footer, never hiding itself
    //   - bar_sheet: a real tap on another family jumps and opens nothing; a second real tap on the current one opens ITS kinds (the card
    //     does not move), a real tap on a kind lands its block and closes the list, a one-kind family opens nothing -- every step by taps
    //   - next_up: a real tap on Next lands the next block, the counter moves forward and the bar names the one after the block read, all the
    //     way to the end (through the reveal of the blocks that wait for it), where it says the card is finished with no Next
    //   - the stops (peek 195 / half 337.60 / full 796 at 390 x 844) do not move with any of them
    public class PoiCardBottomElementsSceneTests : SearchSceneFixture
    {
        private const string Lamp = "lamp";

        private PoiCardSheetView Sheet => Card.Sheet;
        private NavController Nav => Sheet.Nav;
        private CardNavModel Model => Nav.Model;
        private ScrollView Scroll => Sheet.Stack.Scroll;
        private CardChromeView Chrome => Sheet.Chrome;
        private VisualElement BarSlot => Sheet.Slots.Bottom;
        private DockNavView Dock => Sheet.Slots.BottomView as DockNavView;
        private BarSheetNavView BarSheet => Sheet.Slots.BottomView as BarSheetNavView;
        private NextUpNavView NextUp => Sheet.Slots.BottomView as NextUpNavView;

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

        private float OnePixel => Sheet.Layer.panel.visualTree.layout.height / Screen.height;

        private float Range => NavScrollRule.Range(Scroll.contentContainer.layout.height, Scroll.contentViewport.layout.height);

        private List<CardEvent> NavJumps => _events.Raised.Where(e => e.Kind == CardEventKinds.NavJump).ToList();

        private CardStrings Words(string language = "en") =>
            new(Card.StringTable.Entries(), CardStringSources.Shared.Entries(), null, language, "en");

        // A part is where a thumb reaches it: inside the card's visible bottom edge
        private bool OnScreen(VisualElement e) =>
            e.worldBound.yMax <= Sheet.Root.worldBound.yMax + OnePixel && e.worldBound.yMin < Sheet.Root.worldBound.yMax - OnePixel;

        // ... or slid below the card's clipped bottom edge
        private bool BelowTheCard(VisualElement e) => e.worldBound.yMin >= Sheet.Root.worldBound.yMax - OnePixel;

        private IEnumerator Push(System.Action<WallConfigData> edit)
        {
            var copy = ConfigCopy();
            edit(copy);
            Session.ApplyCardSettings(copy);
            yield return CardTestInput.Settle(0.3f);
        }

        // The wall's Bottom Bar = `element`, its blocks Grouped By Family (every family one run: section 4.4's worked example), no Top Bar
        private IEnumerator UseTheElement(string element, int slots = 5, string language = null)
        {
            yield return Push(c =>
            {
                c.card_settings.navigation.bottom = element;
                c.card_settings.navigation.block_order = CardOptions.BlockOrderByFamily;
                c.card_settings.navigation.top = CardOptions.NavNone;
                c.card_settings.navigation.max_slots = slots;
                if (language != null) c.card_settings.preview_language = language;
            });
        }

        private IEnumerator Open(SheetStopRule.Stop stop)
        {
            // - Select toggles (re-selecting the selected point clears it): start from nothing selected so a second Open in one test works
            SelectionEventBus.Clear();
            yield return CardTestInput.Settle();
            SelectionEventBus.Select(Lamp);
            yield return CardTestInput.Settle();
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
        }

        private IEnumerator Landed()
        {
            for (int frame = 0; frame < 600 && Nav.IsJumping; frame++) yield return null;
            Assert.IsFalse(Nav.IsJumping, "the jump came to rest");
            yield return CardTestInput.Settle(0.3f);
        }

        private static IEnumerator TouchTap(IPanel panel, Vector2 at) => CardTestInput.TouchDrag(panel, at, Vector2.zero, frames: 1);

        // A real tap on a button that sits on screen
        private IEnumerator Tap(Button button, string name)
        {
            Assert.IsTrue(OnScreen(button), "precondition: " + name + " is on screen to tap " + button.worldBound + " in " + Sheet.Root.worldBound);
            Assert.IsTrue(button.worldBound.xMin >= Sheet.Root.worldBound.xMin - OnePixel && button.worldBound.xMax <= Sheet.Root.worldBound.xMax + OnePixel,
                "precondition: " + name + " is inside the card");
            yield return TouchTap(Sheet.Root.panel, button.worldBound.center);
            yield return CardTestInput.Settle();
        }

        // A real finger drags the stack by `dy` panel units (negative = up the screen = further down the card), then lets it settle
        private IEnumerator DragStack(float dy)
        {
            var viewport = Scroll.contentViewport.worldBound;
            float inset = Mathf.Min(20f, viewport.height * 0.5f);
            float startY = dy < 0f ? viewport.yMax - inset : viewport.yMin + inset;
            yield return CardTestInput.TouchDrag(Scroll.panel, new Vector2(viewport.center.x, startY), new Vector2(0f, dy));
            yield return CardTestInput.Settle(0.6f);
        }

        // Real drags up the card until the stack sits at its top (each drag shorter than the screen, so the finger never leaves it)
        private IEnumerator ScrollToTop()
        {
            for (int drag = 0; drag < 80 && Scroll.scrollOffset.y > 1f; drag++) yield return DragStack(Scroll.contentViewport.layout.height * 0.7f);
            Assert.LessOrEqual(Scroll.scrollOffset.y, 1f, "precondition: back at the top of the card");
        }

        // Real drags down the card until the stack rests at the END of its range (the range is read again each time: a reveal at the end
        // grows the card). The end, not the last target, is where the blocks waiting for it are revealed and where Hide While Scrolling
        // gives the bottom area back -- since _3.3 P1b The Lamp's Sources, under its last target, are taller than the screen
        private IEnumerator DragToTheEnd()
        {
            for (int drag = 0; drag < 80 && Scroll.scrollOffset.y < Range - NavScrollRule.EndTolerance; drag++)
                yield return DragStack(-Scroll.contentViewport.layout.height * 0.7f);
            Assert.GreaterOrEqual(Scroll.scrollOffset.y, Range - NavScrollRule.EndTolerance,
                "precondition: the drags reached the end of the card (offset " + Scroll.scrollOffset.y + ", range " + Range + ")");
        }

        // The section the reader is in, read from the SCREEN: the spy's own rule over each section's first slot as drawn
        private int SectionUnderTheSpyLine() =>
            CardTestInput.UnderTheSpyLine(Scroll, Model.Sections.Select(s => Sheet.Stack.SlotOfBlock(s.Targets[0])));

        // A block's top meets the viewport's top (+-1 px), or -- a late block -- the stack sits at the end of its range
        private void AssertLanded(string block, string name)
        {
            var slot = Sheet.Stack.SlotOfBlock(block);
            if (slot.layout.y <= Range - OnePixel)
                Assert.AreEqual(0f, slot.worldBound.yMin - Scroll.contentViewport.worldBound.yMin, OnePixel, name + ": its block's top meets the viewport's top");
            else
                Assert.AreEqual(Range, Scroll.scrollOffset.y, OnePixel, name + ": a late block clamps at the end of the range");
        }

        // ---------------- the stops ----------------

        // The bottom slot is under the header's peek part, whichever element it holds: the stops are the same with each of the three new
        // elements set as with none (5a's standard: peek 195.00 / half 337.60 / full 796.00 at 390 x 844) -- measured at Full, Half and Peek
        [UnityTest]
        public IEnumerator TheLampsStops_DoNotMove_WithAnyOfTheThreeNewElementsSet()
        {
            Assert.AreEqual(CardOptions.NavNone, ConfigCopy().card_settings.navigation.bottom, "precondition: the shipped wall has no Bottom Bar");
            yield return Open(SheetStopRule.Stop.Full);
            var unset = Sheet.Stops;
            foreach (string element in new[] { BuiltInNavElements.Dock, BuiltInNavElements.BarSheet, BuiltInNavElements.NextUp })
            {
                yield return UseTheElement(element);
                yield return Rested();
                Assert.AreEqual(element, Sheet.Slots.BottomKey, "precondition: " + element + " is in the bottom slot");
                Assert.IsTrue(Chrome.BottomNavShown, element + ": shows at Full");
                Assert.GreaterOrEqual(BarSlot.worldBound.yMin, Sheet.Stack.PeekPart.worldBound.yMax, element + ": far under the header's peek part");
                foreach (var stop in new[] { SheetStopRule.Stop.Full, SheetStopRule.Stop.Half, SheetStopRule.Stop.Peek })
                {
                    Sheet.SetStop(stop);
                    yield return CardTestInput.Settle();
                    yield return Rested();
                    var now = Sheet.Stops;
                    Assert.AreEqual(unset.Peek, now.Peek, 0.01f, element + " at " + stop + ": peek unchanged");
                    Assert.AreEqual(unset.Half, now.Half, 0.01f, element + " at " + stop + ": half unchanged");
                    Assert.AreEqual(unset.Full, now.Full, 0.01f, element + " at " + stop + ": full unchanged");
                }
                Sheet.SetStop(SheetStopRule.Stop.Full);
                yield return CardTestInput.Settle();
            }
            Debug.Log("[Card] Lamp stops at " + Sheet.Layer.layout.size + ": peek " + unset.Peek.ToString("F2") + ", half " + unset.Half.ToString("F2")
                + ", full " + unset.Full.ToString("F2") + " (dock, bar_sheet, next_up set and unset)");
        }

        // ---------------- dock ----------------

        private bool WordsShown(NavTab slot) => slot.Label.parent.resolvedStyle.display == DisplayStyle.Flex;

        // The dock widens the section ON SCREEN: its slot (or More for a section in More's list) and no other shows its word beside its icon,
        // wider than every other, the pill inside the card
        private void AssertTheDockWidensTheScreen(string when)
        {
            int section = SectionUnderTheSpyLine();
            var slots = Dock.Items.Append(Dock.More).ToList();
            int expected = section < Dock.Items.Count ? section : Dock.Items.Count;
            Assert.AreEqual(section, Dock.CurrentIndex, when + ": the dock follows the section on screen");
            for (int i = 0; i < slots.Count; i++)
            {
                bool marked = i == expected;
                Assert.AreEqual(marked, slots[i].Button.ClassListContains(NavTab.CurrentClass), when + ": slot " + i + " marked");
                Assert.AreEqual(marked, WordsShown(slots[i]), when + ": slot " + i + " says its word only while it is the section on screen");
                if (!marked) Assert.Less(slots[i].Button.layout.width, slots[expected].Button.layout.width, when + ": slot " + i + " is narrower than the widened one");
            }
            var pill = Dock.Bar.worldBound;
            Assert.LessOrEqual(pill.width, Sheet.Root.worldBound.width + OnePixel, when + ": the pill is no wider than the card");
            Assert.GreaterOrEqual(pill.xMin, Sheet.Root.worldBound.xMin - OnePixel, when + ": and inside it");
            Assert.LessOrEqual(pill.xMax, Sheet.Root.worldBound.xMax + OnePixel, when + ": (right)");
        }

        // Real drags down the card -- until the reader has been through at least three sections -- and after EACH the widened item is the
        // slot of the section on screen: the widening travels with the reader's scroll. A real tap on a slot only moves the card; the
        // widened item is then the one the SCREEN shows (the spy's), which a short landed block can make the next family's, never a tap's own
        [UnityTest]
        public IEnumerator ARealDragThroughSections_MovesTheWidenedItem_WithTheSpy_NotTheTap()
        {
            yield return UseTheElement(BuiltInNavElements.Dock);
            yield return Open(SheetStopRule.Stop.Full);
            Assert.AreEqual(BuiltInNavElements.Dock, Sheet.Slots.BottomKey);
            Assert.Greater(Range, 1000f, "precondition: the stack really scrolls");
            AssertTheDockWidensTheScreen("at the top");
            var seen = new List<int> { Dock.CurrentIndex };
            for (int drag = 0; drag < 40 && seen.Distinct().Count() < 4; drag++)
            {
                var viewport = Scroll.contentViewport.worldBound;
                float before = Scroll.scrollOffset.y;
                yield return CardTestInput.TouchDrag(Scroll.panel, new Vector2(viewport.center.x, viewport.yMax - 40f), new Vector2(0f, -viewport.height * 0.6f));
                yield return CardTestInput.Settle(0.6f);
                Assert.Greater(Scroll.scrollOffset.y, before, "precondition: the real drag scrolled the stack");
                AssertTheDockWidensTheScreen("after drag " + (drag + 1));
                if (Dock.CurrentIndex != seen[seen.Count - 1]) seen.Add(Dock.CurrentIndex);
            }
            Assert.GreaterOrEqual(seen.Distinct().Count(), 3, "the widening moved through at least three sections with the reader: " + string.Join(", ", seen));
            CollectionAssert.AreEqual(seen.OrderBy(s => s), seen, "and only forward, as the reader read down");
            yield return DragStack(60f);
            Assert.IsTrue(Chrome.BottomNavShown, "precondition: a drag up brought the dock back");
            AssertTheDockWidensTheScreen("with the dock back");
            var marked = Dock.Items.Append(Dock.More).First(s => s.Button.ClassListContains(NavTab.CurrentClass));
            Assert.IsTrue(CardTestInput.IsShown(marked.Label, Sheet.Root), "its word is on screen, beside its icon");
            yield return CardGalleryChecks.Render("Nav_Lamp_dock_scrolled_390");

            // - a real tap on a slot moves the card; the widened item is then the section the screen shows
            yield return ScrollToTop();
            Assert.AreEqual(0, SectionUnderTheSpyLine(), "precondition: back at the top");
            var tab = Dock.Items[2];
            Assert.IsTrue(OnScreen(tab.Button), "precondition: the third slot is on screen");
            int jumpsBefore = NavJumps.Count;
            yield return Tap(tab.Button, tab.Label.text);
            yield return Landed();
            AssertLanded(Model.Sections[2].Targets[0], tab.Label.text);
            Assert.AreEqual(jumpsBefore + 1, NavJumps.Count, "one nav_jump");
            Assert.AreEqual(BuiltInNavElements.Dock, NavJumps.Last().Variant, "naming the dock");
            AssertTheDockWidensTheScreen("after a tap on " + tab.Label.text);
            Assert.IsTrue(Chrome.BottomNavShown, "the dock the visitor just used stays (a jump is no reading)");
        }

        // The pill never grows past the card with the Portuguese labels, at every Bar Slots from 3 to 5, whichever section is read: a real
        // tap on every slot, and on a row of More's list, then the widest state measured; rendered Nav_Lamp_dock_pt_full_390
        [UnityTest, Timeout(600000)]
        public IEnumerator InPortuguese_ThePillNeverGrowsPastTheCard_AtEveryBarSlots_AndEverySlotIsATouchTarget()
        {
            float touch = CardTestInput.TokenPx("--ts-touch-target");
            float widest = 0f;
            foreach (int slots in new[] { 5, 4, 3 })
            {
                yield return UseTheElement(BuiltInNavElements.Dock, slots, "pt");
                yield return Open(SheetStopRule.Stop.Full);
                Assert.AreEqual("Fechar", Sheet.CloseButton.tooltip, "precondition: the card reads in Portuguese");
                Assert.AreEqual(390f, Sheet.Layer.panel.visualTree.layout.width, OnePixel, "precondition: the run's 390 px frame");
                Assert.AreEqual(slots - 1, Dock.Items.Count, slots + " slots: that many sections and More");
                Assert.AreEqual("Sobre", Dock.Items[0].Label.text, "the first slot reads in Portuguese");
                for (int i = Dock.Items.Count - 1; i >= 0; i--)
                {
                    var slot = Dock.Items[i];
                    Assert.GreaterOrEqual(slot.Button.layout.width, touch - OnePixel, slot.Label.text + ": wide enough to tap");
                    Assert.GreaterOrEqual(slot.Button.layout.height, touch - OnePixel, slot.Label.text + ": tall enough to tap");
                    yield return Tap(slot.Button, slot.Label.text);
                    yield return Landed();
                    AssertTheDockWidensTheScreen(slots + " slots, after a tap on " + slot.Label.text);
                    var marked = Dock.Items.Append(Dock.More).First(s => s.Button.ClassListContains(NavTab.CurrentClass));
                    var words = marked.Label.MeasureTextSize(marked.Label.text, 0, VisualElement.MeasureMode.Undefined, 0, VisualElement.MeasureMode.Undefined);
                    Assert.GreaterOrEqual(marked.Label.layout.width + 0.5f, words.x, "'" + marked.Label.text + "': whole, never cut");
                    widest = Mathf.Max(widest, Dock.Bar.worldBound.width);
                }
                Assert.IsTrue(Dock.HasMore, "the Lamp's eight sections overflow " + slots + " slots");
                Assert.GreaterOrEqual(Dock.More.Button.layout.width, touch - OnePixel, "More: wide enough to tap");
                if (slots == 5) yield return CardGalleryChecks.Render("Nav_Lamp_dock_pt_full_390");
            }
            Debug.Log("[Card] Dock in Portuguese at 390 px, 3 to 5 slots: widest pill " + widest.ToString("F1") + " px");
            Assert.LessOrEqual(widest, Sheet.Root.worldBound.width + OnePixel, "never past the card");
        }

        // The dock hides on scroll down with the footer and comes back on a drag up -- that is the area's job (CardChromeRule); the view
        // itself never hides: still bound, still displayed, only slid below the card's edge. Nothing at peek
        [UnityTest]
        public IEnumerator TheDock_SlidesAwayWithTheArea_OnARealDragDown_AndBack_AndNothingShowsAtPeek()
        {
            yield return UseTheElement(BuiltInNavElements.Dock);
            yield return Open(SheetStopRule.Stop.Full);
            Assert.IsTrue(Chrome.BottomShown && Chrome.BottomNavShown && OnScreen(BarSlot), "precondition: the dock shows at Full");
            yield return CardGalleryChecks.Render("Nav_Lamp_dock_full_390");
            yield return DragStack(-300f);
            Assert.Greater(Scroll.scrollOffset.y, CardChromeRule.HideAfterTravel, "precondition: the drag moved down past the threshold");
            Assert.IsFalse(Chrome.BottomNavShown, "moving down hid the area with the dock");
            Assert.IsTrue(BelowTheCard(BarSlot), "the dock slid below the card's edge");
            Assert.IsNotNull(Dock, "the view is still bound: the area hid it, not the view");
            Assert.AreNotEqual(DisplayStyle.None, Dock.Root.style.display.value, "and still displayed");
            yield return DragStack(60f);
            Assert.IsTrue(Chrome.BottomNavShown && OnScreen(BarSlot), "a drag up brought it back");
            Sheet.SetStop(SheetStopRule.Stop.Peek);
            yield return CardTestInput.Settle();
            yield return Rested();
            Assert.IsFalse(Chrome.BottomNavShown, "nothing at peek");
            Assert.IsTrue(BelowTheCard(BarSlot), "the dock is below the card's edge at peek");
        }

        // ---------------- bar_sheet ----------------

        // Every step by real taps: a tap on another family jumps and opens nothing; a second tap on the current one opens ITS kinds (the card
        // does NOT move: the two outcomes differ on screen); a tap on a kind lands its block and closes the list; a family with one kind
        // opens nothing; renders Nav_Lamp_bar_sheet_{full,kinds}_390
        [UnityTest]
        public IEnumerator ARealFirstTapJumpsOnly_ASecondTapOpensTheKinds_AKindLandsAndClosesTheList_AllWithTaps()
        {
            yield return UseTheElement(BuiltInNavElements.BarSheet);
            yield return Open(SheetStopRule.Stop.Full);
            Assert.AreEqual(BuiltInNavElements.BarSheet, Sheet.Slots.BottomKey);
            var words = Words();
            yield return CardGalleryChecks.Render("Nav_Lamp_bar_sheet_full_390");

            // - the first real tap on a family that is not the current one (Media): it jumps, nothing opens
            var media = BarSheet.Items[1];
            Assert.AreEqual(0, BarSheet.CurrentIndex, "precondition: the card opens on its first family");
            float offsetBefore = Scroll.scrollOffset.y;
            int jumps = NavJumps.Count;
            yield return Tap(media.Button, media.Label.text);
            yield return Landed();
            float afterFirst = Scroll.scrollOffset.y;
            Assert.Greater(Mathf.Abs(afterFirst - offsetBefore), 100f, "the FIRST tap moved the card (" + offsetBefore.ToString("F0") + " -> " + afterFirst.ToString("F0") + ")");
            Assert.IsFalse(BarSheet.KindsOpen, "and opened no list");
            AssertLanded(Model.Sections[1].Targets[0], media.Label.text);
            Assert.AreEqual(jumps + 1, NavJumps.Count, "one nav_jump");
            Assert.AreEqual(BuiltInNavElements.BarSheet, NavJumps.Last().Variant, "naming bar_sheet");
            Assert.AreEqual(1, BarSheet.CurrentIndex, "precondition: Media is now the family being read");
            Assert.AreEqual(DisplayStyle.Flex, BarSheet.Arrows[1].Arrow.resolvedStyle.display, "its slot wears the arrow: a tap here opens a list");

            // - the second real tap on it: the kinds, with their counts, and the card where it was
            yield return Tap(media.Button, media.Label.text);
            Assert.IsTrue(BarSheet.KindsOpen, "the SECOND tap opens Media's kinds");
            Assert.AreEqual(afterFirst, Scroll.scrollOffset.y, OnePixel, "and the card did not move: the two taps differ on screen");
            Assert.AreEqual(jumps + 1, NavJumps.Count, "opening a list is no jump");
            var kinds = Model.Sections[1].Kinds;
            Assert.GreaterOrEqual(kinds.Count, 2, "precondition: Media holds several kinds on The Lamp");
            CollectionAssert.AreEqual(kinds.Select(k => words.Get(k.LabelKey)), BarSheet.KindRows.Select(r => r.Label.text), "exactly Media's kinds, in the model's order");
            CollectionAssert.AreEqual(kinds.Select(k => k.Count.ToString()), BarSheet.KindRows.Select(r => r.Count.text), "each with its block count");
            float touch = CardTestInput.TokenPx("--ts-touch-target");
            foreach (var row in BarSheet.KindRows)
            {
                Assert.IsTrue(OnScreen(row.Button) && row.Button.worldBound.yMin >= Sheet.Root.worldBound.yMin, row.Label.text + ": on screen in the card");
                Assert.GreaterOrEqual(row.Button.layout.height, touch - OnePixel, row.Label.text + ": tall enough to tap");
            }
            Assert.LessOrEqual(BarSheet.KindsList.worldBound.yMax, BarSheet.Bar.worldBound.yMin + OnePixel, "above the bar");
            yield return CardGalleryChecks.Render("Nav_Lamp_bar_sheet_kinds_390");

            // - a real tap on a kind (the last one): its first block lands and the list closes
            int pick = kinds.Count - 1;
            string block = kinds[pick].Targets[0];
            Assert.AreNotEqual(Model.Sections[1].Targets[0], block, "precondition: the last kind starts at another block than the family does");
            yield return Tap(BarSheet.KindRows[pick].Button, kinds[pick].Key);
            yield return Landed();
            Assert.IsFalse(BarSheet.KindsOpen, "a pick closes the list");
            AssertLanded(block, kinds[pick].Key);
            Assert.AreEqual(jumps + 2, NavJumps.Count, "the pick is one nav_jump");
            Assert.AreEqual(BuiltInNavElements.BarSheet, NavJumps.Last().Variant);
            Assert.AreEqual(block, NavJumps.Last().BlockKey, "to that kind's first block");

            // - a family with one kind (Listen): a tap, then a second tap -- the card jumps both times, no list ever opens
            int listen = Model.Sections.ToList().FindIndex(s => s.Key == BuiltInFamilies.Guides);
            Assert.AreEqual(1, Model.Sections[listen].Kinds.Count, "precondition: Listen is the one-kind family");
            var tab = BarSheet.Items[listen];
            yield return Tap(tab.Button, tab.Label.text);
            yield return Landed();
            yield return Tap(tab.Button, tab.Label.text);
            yield return Landed();
            Assert.IsFalse(BarSheet.KindsOpen, "a family with one kind opens no list, however often it is tapped");

            // - the list opens only for the family being read, and a tap on another family closes it by jumping
            var stories = BarSheet.Items[3];
            yield return Tap(stories.Button, stories.Label.text);
            yield return Landed();
            Assert.AreEqual(3, BarSheet.CurrentIndex, "precondition: Stories is being read");
            Assert.GreaterOrEqual(Model.Sections[3].Kinds.Count, 2, "precondition: Stories holds several kinds");
            yield return Tap(stories.Button, stories.Label.text);
            Assert.IsTrue(BarSheet.KindsOpen, "a second tap opens Stories' kinds");
            var other = BarSheet.Items[1];
            yield return Tap(other.Button, other.Label.text);
            yield return Landed();
            Assert.IsFalse(BarSheet.KindsOpen, "a tap on another family jumps there and the open list closes");
        }

        // In Portuguese the kinds list is whole at 390 px (the kind words are the per-kind Portuguese ones), rendered Nav_Lamp_bar_sheet_kinds_pt_390
        [UnityTest]
        public IEnumerator InPortuguese_TheKindsListIsWhole_WithTheKindsWords_AtThePhoneWidth()
        {
            yield return UseTheElement(BuiltInNavElements.BarSheet, 5, "pt");
            yield return Open(SheetStopRule.Stop.Full);
            Assert.AreEqual("Fechar", Sheet.CloseButton.tooltip, "precondition: the card reads in Portuguese");
            var words = Words("pt");
            var tab = BarSheet.Items[1];
            yield return Tap(tab.Button, tab.Label.text);
            yield return Landed();
            yield return Tap(tab.Button, tab.Label.text);
            Assert.IsTrue(BarSheet.KindsOpen, "the second tap opens the kinds");
            var kinds = Model.Sections[1].Kinds;
            CollectionAssert.AreEqual(kinds.Select(k => words.Get(k.LabelKey)), BarSheet.KindRows.Select(r => r.Label.text), "the kinds' Portuguese words");
            float touch = CardTestInput.TokenPx("--ts-touch-target");
            foreach (var row in BarSheet.KindRows)
            {
                var measured = row.Label.MeasureTextSize(row.Label.text, 0, VisualElement.MeasureMode.Undefined, 0, VisualElement.MeasureMode.Undefined);
                Assert.GreaterOrEqual(row.Label.layout.width + 0.5f, measured.x, "'" + row.Label.text + "': whole, never cut");
                Assert.GreaterOrEqual(row.Button.layout.height, touch - OnePixel, row.Label.text + ": tall enough to tap");
                Assert.GreaterOrEqual(row.Button.worldBound.xMin, Sheet.Root.worldBound.xMin - OnePixel, row.Label.text + ": inside the card");
                Assert.LessOrEqual(row.Button.worldBound.xMax, Sheet.Root.worldBound.xMax + OnePixel, row.Label.text + ": inside the card");
            }
            yield return CardGalleryChecks.Render("Nav_Lamp_bar_sheet_kinds_pt_390");
        }

        // ---------------- next_up ----------------

        // The bar names the block AFTER the one on screen (the spy's), and a real tap on Next lands that block (+-1 px) with the counter
        // moving forward -- from the top of the card all the way to its end: every press is a real finger, every word is checked against the
        // model and the stack, and the walk goes through the reveal of the blocks that wait for the end of the card (the total grows and the
        // bar follows)
        [UnityTest, Timeout(900000)]
        public IEnumerator ARealTapOnNext_LandsTheNextBlock_TheCounterMoves_AllTheWayToTheEnd()
        {
            yield return UseTheElement(BuiltInNavElements.NextUp);
            yield return Open(SheetStopRule.Stop.Full);
            Assert.AreEqual(BuiltInNavElements.NextUp, Sheet.Slots.BottomKey);
            Assert.AreEqual(56, Model.TargetCount, "precondition: The Lamp's 56 targets (one more once its waiting block is revealed)");
            var words = Words();
            yield return CardGalleryChecks.Render("Nav_Lamp_next_up_full_390");
            int presses = 0;
            int firstTotal = Model.TargetCount;
            bool grew = false;
            int lastPosition = 0;
            while (!NextUp.Finished && presses < 90)
            {
                var model = Model;
                int at = model.Targets.ToList().IndexOf(Nav.CurrentBlock);
                Assert.GreaterOrEqual(at, 0, "press " + presses + ": a target is being read");
                string next = model.Targets[at + 1];
                string when = "press " + (presses + 1) + " (" + Nav.CurrentBlock + " -> " + next + ")";
                Assert.AreEqual(next, NextUp.NextBlock, when + ": the bar names the block after the one read");
                Assert.AreEqual(words.Get(CardStrings.Keys.NavNextUp), NextUp.Caption.text);
                Assert.AreEqual((at + 2) + " of " + model.TargetCount, NextUp.Position.text, when + ": its place among all the targets");
                int position = at + 2;
                Assert.Greater(position, lastPosition, when + ": the counter only moves forward");
                lastPosition = position;
                string drawn = Sheet.Stack.HeadingOfBlock(next);
                Assert.AreEqual(drawn.Length > 0 ? drawn : words.Get(model.KindOf(next).LabelKey), NextUp.Heading.text, when + ": the words over that block (else its kind's)");
                Assert.IsNotEmpty(NextUp.Heading.text, when + ": the bar never names a block with nothing");
                int jumps = NavJumps.Count;
                yield return Tap(NextUp.Next, "Next");
                yield return Landed();
                AssertLanded(next, when);
                Assert.AreEqual(jumps + 1, NavJumps.Count, when + ": one nav_jump");
                Assert.AreEqual(BuiltInNavElements.NextUp, NavJumps.Last().Variant, when + ": naming next_up");
                Assert.AreEqual(next, NavJumps.Last().BlockKey, when + ": and the block it landed on");
                Assert.IsTrue(Chrome.BottomNavShown, when + ": the bar the visitor just used stays (a jump is no reading)");
                if (Model.TargetCount > firstTotal) grew = true;
                presses++;
            }
            Assert.IsTrue(NextUp.Finished, "the walk reached the last target (" + presses + " presses)");
            Assert.AreEqual(Model.Targets[Model.TargetCount - 1], Nav.CurrentBlock, "the card is read at its last target");
            // - the last target is not the end of the card when a taller-than-the-screen block follows it (The Lamp's Sources): the reader
            //   goes on by hand to the end, which reveals the block waiting for it
            yield return DragToTheEnd();
            grew |= Model.TargetCount > firstTotal;
            Assert.IsTrue(grew, "the end of the card revealed the block waiting for it: the total grew from " + firstTotal + " to " + Model.TargetCount);
            Assert.IsTrue(NextUp.Finished, "at the end the bar still says the card is finished");
            Assert.AreEqual(Model.Targets[Model.TargetCount - 1], Nav.CurrentBlock, "read at the last target of the grown card");
            Assert.AreEqual(words.Get(CardStrings.Keys.NavNextUpDone), NextUp.Caption.text, "it says the card is finished");
            Assert.AreEqual(Model.TargetCount + " of " + Model.TargetCount, NextUp.Position.text, "keeping the card's size, as the reveal made it");
            Assert.IsFalse(CardTestInput.IsShown(NextUp.Next, Sheet.Root), "no Next that goes nowhere");
            Assert.IsNull(NextUp.NextBlock);
            Debug.Log("[Card] Up next walked The Lamp in " + presses + " presses; total " + firstTotal + " -> " + Model.TargetCount);
            yield return CardGalleryChecks.Render("Nav_Lamp_next_up_end_390");
        }

        // Reading by hand moves the bar too: a real drag to the end of the card ends it -- the finished words, no button -- and a real
        // drag back up brings Next and a block back; in Portuguese every word is the Portuguese one, whole at 390 px
        [UnityTest, Timeout(600000)]
        public IEnumerator ARealDragToTheEnd_FinishesTheBar_AndBackUpItNamesABlockAgain_InPortuguese()
        {
            yield return UseTheElement(BuiltInNavElements.NextUp, 5, "pt");
            yield return Open(SheetStopRule.Stop.Full);
            Assert.AreEqual("Fechar", Sheet.CloseButton.tooltip, "precondition: the card reads in Portuguese");
            var words = Words("pt");
            Assert.AreEqual(words.Get(CardStrings.Keys.NavNextUp), NextUp.Caption.text, "A seguir");
            Assert.AreEqual("A seguir", NextUp.Caption.text);
            Assert.AreEqual("Seguinte", NextUp.NextLabel.text);
            Assert.IsTrue(NextUp.Position.text.Contains(" de "), "the place reads 'N de M': " + NextUp.Position.text);
            yield return CardGalleryChecks.Render("Nav_Lamp_next_up_pt_full_390");
            // - to the scroll END, not to the first Finished: the last target comes before The Lamp's taller-than-the-screen Sources, and
            //   while the reader still drags down Hide While Scrolling keeps the bar away -- the end gives it back
            yield return DragToTheEnd();
            Assert.IsTrue(NextUp.Finished, "a real drag to the end finishes the bar");
            Assert.AreEqual("Chegou ao fim", NextUp.Caption.text, "in Portuguese");
            Assert.IsFalse(CardTestInput.IsShown(NextUp.Next, Sheet.Root), "no Next at the end");
            Assert.IsTrue(CardTestInput.IsShown(NextUp.Caption, Sheet.Root) && Chrome.BottomNavShown, "the words are on screen");
            var caption = NextUp.Caption.MeasureTextSize(NextUp.Caption.text, 0, VisualElement.MeasureMode.Undefined, 0, VisualElement.MeasureMode.Undefined);
            Assert.GreaterOrEqual(NextUp.Caption.layout.width + 0.5f, caption.x, "the finished words are whole at 390 px");
            yield return CardGalleryChecks.Render("Nav_Lamp_next_up_end_pt_390");
            yield return ScrollToTop();
            Assert.IsTrue(Chrome.BottomNavShown, "precondition: the drags up brought the bar back");
            Assert.IsFalse(NextUp.Finished, "back up the card the bar names a block again");
            Assert.IsTrue(CardTestInput.IsShown(NextUp.Next, Sheet.Root), "with its Next button");
            Assert.IsNotEmpty(NextUp.Heading.text);
        }
    }
}
