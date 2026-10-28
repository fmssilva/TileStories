using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // Phase B of the bottom slot and the bottom_bar element (_3.2 step 7a, sections 4.2, 4.3, 5.2) on the REAL LivingRoomScene with The
    // Lamp's real card (56 targets, 8 families, its sticky footer) and real input; the bar is made the wall's Bottom Bar through the live
    // seam (the Editor's own path):
    //   - the stops do not move with the Bottom Bar set or unset (5a's standard: peek 195.00 / half 337.60 / full 796.00)
    //   - at Half the footer and the bar give way TOGETHER (one area, one floor); Full shows both, the bar under the footer
    //   - a real tap on every slot lands its section's first block at the viewport's top (+-1 px; the range end for a late one), with one
    //     nav_jump naming bottom_bar -- and the bar the visitor just used stays (a jump is no reading)
    //   - a real tap on More lists exactly the overflow sections, a real tap on a row lands its block and closes the list
    //   - after real drags the bar marks the slot of the section on screen (More for one in its list)
    //   - a real drag down hides the footer AND the bar together, a drag up brings both back; nothing at peek
    //   - in Portuguese every slot is a whole touch target with its whole word
    public class PoiCardBottomBarSceneTests : SearchSceneFixture
    {
        private const string Lamp = "lamp";

        private PoiCardSheetView Sheet => Card.Sheet;
        private NavController Nav => Sheet.Nav;
        private CardNavModel Model => Nav.Model;
        private ScrollView Scroll => Sheet.Stack.Scroll;
        private VisualElement Footer => Sheet.Stack.Footer;
        private VisualElement BarSlot => Sheet.Slots.Bottom;
        private BottomBarNavView Bar => Sheet.Slots.BottomView as BottomBarNavView;
        private CardChromeView Chrome => Sheet.Chrome;

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

        // The stack's room at the stop: from the stack's top to the sheet's foot -- since _3.3 S4a the header and the strip float over the
        // stack, only the bottom area takes room from it
        private float StackRoom => Sheet.Stops.HeightOf(Sheet.Stop) - Scroll.layout.yMin;

        private float Viewport => Scroll.contentViewport.layout.height;

        private List<CardEvent> NavJumps => _events.Raised.Where(e => e.Kind == CardEventKinds.NavJump).ToList();

        // A part is where a thumb reaches it: inside the card's visible bottom edge
        private bool OnScreen(VisualElement e) =>
            e.worldBound.yMax <= Sheet.Root.worldBound.yMax + OnePixel && e.worldBound.yMin < Sheet.Root.worldBound.yMax - OnePixel;

        // ... or slid below the card's clipped bottom edge
        private bool BelowTheCard(VisualElement e) => e.worldBound.yMin >= Sheet.Root.worldBound.yMax - OnePixel;

        private string Parts() =>
            "root " + Sheet.Root.layout + ", header " + Sheet.Stack.HeaderSlot.layout + ", strip " + Sheet.Slots.Top.layout + ", stack "
            + Scroll.layout + ", viewport " + Scroll.contentViewport.layout + ", footer " + Footer.layout + ", bar " + BarSlot.layout
            + ", area " + Sheet.BottomArea.layout + " (" + Sheet.BottomArea.resolvedStyle.position + ")";

        private IEnumerator Push(System.Action<WallConfigData> edit)
        {
            var copy = ConfigCopy();
            edit(copy);
            Session.ApplyCardSettings(copy);
            yield return CardTestInput.Settle(0.3f);
        }

        // The wall's Bottom Bar = bottom_bar, its blocks Grouped By Family (every family one run: section 4.4's worked example); the Top
        // Bar none unless `withTopBar` keeps the default text tabs
        private IEnumerator UseTheBar(bool withTopBar = false)
        {
            yield return Push(c =>
            {
                c.card_settings.navigation.bottom = BuiltInNavElements.BottomBar;
                c.card_settings.navigation.block_order = CardOptions.BlockOrderByFamily;
                if (!withTopBar) c.card_settings.navigation.top = CardOptions.NavNone;
            });
        }

        private IEnumerator Open(SheetStopRule.Stop stop)
        {
            SelectionEventBus.Select(Lamp);
            yield return CardTestInput.Settle();
            Sheet.SetStop(stop);
            yield return CardTestInput.Settle();
            Assert.AreEqual(stop, Sheet.Stop, "precondition: the card rests at " + stop);
            yield return Rested();
            // - _3.3 S4a: at Full the hero is the screen and the bottom chrome waits for it: read just past it
            if (stop == SheetStopRule.Stop.Full) yield return CardTestInput.PastTheHero(Sheet);
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

        // A real finger drags the stack by `dy` panel units (negative = up the screen = further down the card), then lets it settle
        private IEnumerator DragStack(float dy)
        {
            // - the part a finger reaches: under the floating header and strip (_3.3 S4a), above the bottom area
            var viewport = CardTestInput.ReachableStack(Sheet);
            float inset = Mathf.Min(20f, viewport.height * 0.5f);
            float startY = dy < 0f ? viewport.yMax - inset : viewport.yMin + inset;
            yield return CardTestInput.TouchDrag(Scroll.panel, new Vector2(viewport.center.x, startY), new Vector2(0f, dy));
            yield return CardTestInput.Settle(0.6f);
        }

        // The section the reader is in, read from the SCREEN: the spy's own rule over each section's first slot as drawn
        private int SectionUnderTheSpyLine() =>
            CardTestInput.SectionUnderTheSpyLine(Scroll, Model, Sheet.Stack.SlotOfBlock);

        // The bar marks the section on screen: its own slot, or More for a section in More's list -- and exactly one of them
        private void AssertMarksTheScreen(string when)
        {
            int section = SectionUnderTheSpyLine();
            Assert.AreEqual(section, Bar.CurrentIndex, when + ": the bar follows the section on screen");
            var slots = Bar.Items.Select(t => t.Button).Append(Bar.More.Button).ToList();
            var expected = section < Bar.Items.Count ? Bar.Items[section].Button : Bar.More.Button;
            Assert.IsTrue(expected.ClassListContains(NavTab.CurrentClass), when + ": " + expected.name + " is marked");
            Assert.AreEqual(1, slots.Count(b => b.ClassListContains(NavTab.CurrentClass)), when + ": exactly one slot marked");
        }

        // A real tap lands a block: its top at the viewport's top (+-1 px), or the range end for a late block; one nav_jump naming the bar
        private IEnumerator TapAndLand(Button button, string block, string name)
        {
            Assert.IsTrue(OnScreen(button), "precondition: " + name + " is on screen to tap " + button.worldBound + " in " + Sheet.Root.worldBound);
            int jumpsBefore = NavJumps.Count;
            yield return TouchTap(Sheet.Root.panel, button.worldBound.center);
            yield return Landed();
            var slot = Sheet.Stack.SlotOfBlock(block);
            if (Sheet.Stack.LandingTop(slot) <= Range - OnePixel)
                Assert.AreEqual(Sheet.Stack.LandingInset, Sheet.Stack.LandingElement(slot).worldBound.yMin - Scroll.contentViewport.worldBound.yMin, OnePixel, name + ": its block's top meets the viewport's top");
            else
                Assert.AreEqual(Range, Scroll.scrollOffset.y, OnePixel, name + ": a late block clamps at the end of the range");
            Assert.AreEqual(jumpsBefore + 1, NavJumps.Count, name + ": one nav_jump");
            Assert.AreEqual(BuiltInNavElements.BottomBar, NavJumps.Last().Variant, name + ": the event names the bar");
            Assert.AreEqual(block, NavJumps.Last().BlockKey, name + ": and the block it landed on");
            Assert.IsTrue(Chrome.BottomNavShown, name + ": the bar the visitor just used stays (a jump is no reading)");
        }

        // ---------------- the stops ----------------

        // The bottom slot is under the header's peek part: the stops are the same with the Bottom Bar unset (the shipped wall) and set --
        // measured at Full, Half and Peek both ways, and logged
        [UnityTest]
        public IEnumerator TheLampsStops_DoNotMove_WithTheBottomBarSetOrUnset()
        {
            Assert.AreEqual(CardOptions.NavNone, ConfigCopy().card_settings.navigation.bottom, "precondition: the shipped wall has no Bottom Bar");
            yield return Open(SheetStopRule.Stop.Full);
            Assert.IsNull(Sheet.Slots.BottomView, "precondition: unset, no bar");
            var unset = Sheet.Stops;
            var expected = SheetStopRule.Compute(Sheet.Layer.layout.height, unset.Peek, ConfigCopy().card_settings.container.half_max_ratio,
                CardTestInput.TokenPx("--ts-sheet-top-gap"));
            Assert.AreEqual(expected.Half, unset.Half, 0.01f, "half comes from the peek and the Half Height Max");

            yield return UseTheBar(withTopBar: true);
            yield return Rested();
            Assert.AreEqual(BuiltInNavElements.BottomBar, Sheet.Slots.BottomKey, "precondition: set, the bar is in the bottom slot");
            // - the hero is the screen at rest: the chrome comes once it is scrolled away (_3.3 S4a)
            yield return CardTestInput.PastTheHero(Sheet);
            Assert.IsTrue(Chrome.BottomNavShown, "and shows at Full");
            Assert.GreaterOrEqual(BarSlot.worldBound.yMin, Sheet.Stack.PeekPart.worldBound.yMax, "the bar sits far under the header's peek part");
            var set = Sheet.Stops;
            Assert.AreEqual(unset.Peek, set.Peek, 0.01f, "peek: the same with and without the bar");
            Assert.AreEqual(unset.Half, set.Half, 0.01f, "half: the same");
            Assert.AreEqual(unset.Full, set.Full, 0.01f, "full: the same");
            foreach (var stop in new[] { SheetStopRule.Stop.Half, SheetStopRule.Stop.Peek })
            {
                Sheet.SetStop(stop);
                yield return CardTestInput.Settle();
                yield return Rested();
                Assert.AreEqual(unset.Peek, Sheet.Stops.Peek, 0.01f, stop + ": peek unchanged");
                Assert.AreEqual(unset.Half, Sheet.Stops.Half, 0.01f, stop + ": half unchanged");
                Assert.AreEqual(unset.Full, Sheet.Stops.Full, 0.01f, stop + ": full unchanged");
            }
            Debug.Log("[Card] Lamp stops at " + Sheet.Layer.layout.size + ": peek " + set.Peek.ToString("F2") + ", half " + set.Half.ToString("F2")
                + ", full " + set.Full.ToString("F2") + " (Bottom Bar set and unset)");
        }

        // ---------------- the bottom area: one pair ----------------

        // The Lamp at Half with the bar (and the default Top Bar): the room under the header cannot hold the footer, the bar and one line,
        // so the WHOLE area gives way -- never the footer without its bar -- and the stack gets every pixel of it (the rule working). At Full
        // all three show, the bar under the footer at the card's very bottom. Both renders
        [UnityTest]
        public IEnumerator TheLampAtHalf_TheFooterAndTheBarGiveWayTogether_AndFullShowsTheBarUnderTheFooter()
        {
            yield return UseTheBar(withTopBar: true);
            yield return Open(SheetStopRule.Stop.Full);
            // - _3.3 P4.4a: the footer waits while the hero shows the action: read past it first
            yield return CardTestInput.ReadPastTheHero(Sheet, DragStack);
            Assert.IsTrue(Chrome.BottomShown && Chrome.BottomNavShown, "precondition: Full shows the area with its bar: " + Parts());
            Assert.Greater(Footer.childCount, 0, "precondition: The Lamp has its sticky footer");
            float footer = Footer.layout.height, bar = BarSlot.layout.height;
            Assert.LessOrEqual(Footer.worldBound.yMax, BarSlot.worldBound.yMin + OnePixel, "the footer sits above the bar: the CTA nearest the content");
            Assert.AreEqual(Sheet.Root.worldBound.yMax, BarSlot.worldBound.yMax, OnePixel, "the bar closes the card, in the thumb zone");
            Assert.AreEqual(StackRoom - footer - bar, Viewport, OnePixel, "the stack has what footer and bar leave (the strip floats over it): " + Parts());
            yield return CardGalleryChecks.Render("Nav_Lamp_bottom_bar_full_390");

            Scroll.scrollOffset = Vector2.zero;
            yield return null;
            Sheet.SetStop(SheetStopRule.Stop.Half);
            yield return CardTestInput.Settle();
            yield return Rested();
            float room = StackRoom;
            // - _3.3 P4.4a: back at the top, Half shows the whole hero -- the footer waits for it and the bar would cut it, so the pair gives
            //   way together (once the hero is read, Half's 281 px under the 48 px bar hold both: PoiCardChromeSceneTests)
            Assert.AreEqual(0f, Scroll.scrollOffset.y, 0.5f, "precondition: at the top of the card");
            float hero = Sheet.Stack.HeaderView.HeroBottom();
            Assert.Less(room - bar, Mathf.Min(hero, Scroll.contentContainer.layout.height), "precondition: the bar would cut the hero: " + Parts());
            Assert.IsFalse(Chrome.BottomShown, "the bottom area gives way as ONE");
            Assert.IsFalse(Chrome.BottomNavShown, "with its bar");
            Assert.IsFalse(Chrome.TopShown, "and the strip");
            Assert.IsTrue(BelowTheCard(Footer) && BelowTheCard(BarSlot), "both slid below the card's edge: " + Parts());
            Assert.AreEqual(room, Viewport, OnePixel, "the stack keeps the whole room");
            Debug.Log("[Card] Lamp at Half with a Bottom Bar: room " + room.ToString("F1") + " = viewport " + Viewport.ToString("F1") + " (footer "
                + footer.ToString("F1") + " + bar " + bar.ToString("F1") + " give way together, the strip too)");
            yield return CardGalleryChecks.Render("Nav_Lamp_bottom_bar_half_390");
        }

        // A real drag down the card slides the footer AND the bar away together and gives both heights to the stack; a drag up brings both
        // back together
        [UnityTest]
        public IEnumerator ARealDragDown_HidesTheFooterAndTheBarTogether_AndADragUpBringsBothBack()
        {
            yield return UseTheBar();
            yield return Open(SheetStopRule.Stop.Full);
            // - _3.3 P4.4a: the footer waits while the hero shows the action: read past it first
            yield return CardTestInput.ReadPastTheHero(Sheet, DragStack);
            Assert.IsTrue(Chrome.BottomShown && Chrome.BottomNavShown, "precondition: both show at Full");
            Assert.IsTrue(OnScreen(Footer) && OnScreen(BarSlot), "precondition: both on screen: " + Parts());
            Assert.Greater(Range, 1000f, "precondition: the stack really scrolls");
            float footer = Footer.layout.height, bar = BarSlot.layout.height;

            yield return DragStack(-300f);
            Assert.Greater(Scroll.scrollOffset.y, CardChromeRule.HideAfterTravel, "precondition: the drag moved down past the threshold");
            Assert.IsFalse(Chrome.BottomShown, "moving down hid the area");
            Assert.IsFalse(Chrome.BottomNavShown, "the bar with it");
            Assert.IsTrue(BelowTheCard(Footer), "the footer slid below the card's edge: " + Parts());
            Assert.IsTrue(BelowTheCard(BarSlot), "and the bar too");
            Assert.AreEqual(StackRoom, Viewport, OnePixel, "both heights went to the stack");

            float offset = Scroll.scrollOffset.y;
            yield return DragStack(60f);
            Assert.Less(Scroll.scrollOffset.y, offset - CardChromeRule.ShowAfterTravel, "precondition: the drag moved back up past the threshold");
            Assert.IsTrue(Chrome.BottomShown && Chrome.BottomNavShown, "moving up brought both back");
            Assert.IsTrue(OnScreen(Footer) && OnScreen(BarSlot), "both on screen again: " + Parts());
            Assert.AreEqual(StackRoom - footer - bar, Viewport, OnePixel, "and the stack gave both heights back");
        }

        [UnityTest]
        public IEnumerator NothingShowsAtPeek()
        {
            yield return UseTheBar();
            yield return Open(SheetStopRule.Stop.Full);
            // - the hero is the screen at rest: the chrome comes once it is scrolled away (_3.3 S4a)
            yield return CardTestInput.PastTheHero(Sheet);
            Assert.IsTrue(Chrome.BottomNavShown, "precondition: the bar shows at Full");
            Sheet.SetStop(SheetStopRule.Stop.Peek);
            yield return CardTestInput.Settle();
            yield return Rested();
            Assert.IsFalse(Chrome.BottomShown || Chrome.BottomNavShown, "nothing at peek");
            Assert.IsTrue(BelowTheCard(BarSlot), "the bar is below the card's edge: " + Parts());
        }

        // ---------------- real taps ----------------

        // A real finger taps every slot of the bar (the last first): each lands its section's first block, marks its slot, and the bar stays
        [UnityTest]
        public IEnumerator ARealTapOnEverySlot_LandsItsSectionsFirstBlock_MarksIt_AndTheBarStays()
        {
            yield return UseTheBar();
            yield return Open(SheetStopRule.Stop.Full);
            Assert.AreEqual(56, Model.TargetCount, "precondition: The Lamp's 56 targets");
            Assert.AreEqual(8, Model.Sections.Count, "precondition: 8 sections");
            Assert.AreEqual(4, Bar.Items.Count, "the default 5 slots: four sections and More");
            for (int i = Bar.Items.Count - 1; i >= 0; i--)
            {
                var tab = Bar.Items[i];
                yield return TapAndLand(tab.Button, Model.Sections[i].Targets[0], tab.Label.text);
                AssertMarksTheScreen(tab.Label.text);
                Assert.AreEqual(i, Bar.CurrentIndex, tab.Label.text + ": its own slot is marked");
            }
        }

        // A real tap on More opens a list of EXACTLY the overflow sections, in order, on screen above the bar; a real tap on a row lands
        // that section's first block, closes the list and marks More; a second real tap on More closes an open list
        [UnityTest]
        public IEnumerator ARealTapOnMore_ListsExactlyTheOverflow_AndARealTapOnARowLandsItsBlock()
        {
            yield return UseTheBar();
            yield return Open(SheetStopRule.Stop.Full);
            Assert.IsTrue(Bar.HasMore, "precondition: 8 sections overflow 5 slots");
            Assert.IsFalse(Bar.MoreOpen);
            yield return TouchTap(Sheet.Root.panel, Bar.More.Button.worldBound.center);
            yield return CardTestInput.Settle();
            Assert.IsTrue(Bar.MoreOpen, "a real tap on More opens its list");
            var strings = new CardStrings(Card.StringTable.Entries(), CardStringSources.Shared.Entries(), null, "en", "en");
            var overflow = Model.MoreSections;
            CollectionAssert.AreEqual(Model.Sections.Skip(Bar.Items.Count).Select(s => s.Key), overflow.Select(s => s.Key), "precondition: the model's overflow");
            CollectionAssert.AreEqual(overflow.Select(s => strings.Get(s.LabelKey)), Bar.MoreRows.Select(r => r.Label.text),
                "the list holds EXACTLY the overflow sections, in order");
            foreach (var row in Bar.MoreRows)
                Assert.IsTrue(OnScreen(row.Button) && row.Button.worldBound.yMin >= Sheet.Root.worldBound.yMin, row.Label.text + ": on screen in the card");
            Assert.LessOrEqual(Bar.MoreList.worldBound.yMax, Bar.Bar.worldBound.yMin + OnePixel, "above the bar");
            yield return CardGalleryChecks.Render("Nav_Lamp_bottom_bar_more_390");

            var first = Bar.MoreRows[0];
            int section = Bar.Items.Count + first.Index;
            yield return TapAndLand(first.Button, Model.Sections[section].Targets[0], first.Label.text);
            Assert.IsFalse(Bar.MoreOpen, "a pick closes the list");
            AssertMarksTheScreen(first.Label.text);
            Assert.IsTrue(Bar.More.Button.ClassListContains(NavTab.CurrentClass), "a section of the list is read: More is marked");

            yield return TouchTap(Sheet.Root.panel, Bar.More.Button.worldBound.center);
            yield return CardTestInput.Settle();
            Assert.IsTrue(Bar.MoreOpen, "precondition: open again");
            Assert.IsTrue(Bar.MoreRows[first.Index].Button.ClassListContains(NavTab.CurrentClass), "the row of the section being read is marked");
            yield return TouchTap(Sheet.Root.panel, Bar.More.Button.worldBound.center);
            yield return CardTestInput.Settle();
            Assert.IsFalse(Bar.MoreOpen, "a second real tap on More closes it");
        }

        // ---------------- the spy ----------------

        // Real drags down the card -- until a section of More's list is on screen -- and after EACH the bar marks the section on screen:
        // its own slot, then More once the reader reaches the overflow; once a drag up brings the bar back it still marks what the screen
        // shows
        [UnityTest]
        public IEnumerator ARealDragOfTheStack_MarksTheSlotOfTheSectionOnScreen()
        {
            yield return UseTheBar();
            yield return Open(SheetStopRule.Stop.Full);
            AssertMarksTheScreen("at the top");
            var seen = new HashSet<int> { Bar.CurrentIndex };
            for (int drag = 0; drag < 40 && Bar.CurrentIndex < Bar.Items.Count; drag++)
            {
                var viewport = Scroll.contentViewport.worldBound;
                float before = Scroll.scrollOffset.y;
                yield return CardTestInput.TouchDrag(Scroll.panel, new Vector2(viewport.center.x, viewport.yMax - 40f), new Vector2(0f, -viewport.height * 0.6f));
                yield return CardTestInput.Settle(0.6f);
                Assert.Greater(Scroll.scrollOffset.y, before, "precondition: the real drag scrolled the stack");
                AssertMarksTheScreen("after drag " + (drag + 1));
                seen.Add(Bar.CurrentIndex);
            }
            Assert.GreaterOrEqual(Bar.CurrentIndex, Bar.Items.Count, "precondition: the drags reached a section of More's list");
            Assert.IsTrue(Bar.More.Button.ClassListContains(NavTab.CurrentClass), "More is marked for it");
            Assert.Greater(seen.Count, 3, "the mark moved through the bar's sections with the reader: " + string.Join(", ", seen));
            yield return DragStack(60f);
            Assert.IsTrue(Chrome.BottomNavShown, "precondition: a drag up brought the bar back");
            AssertMarksTheScreen("with the bar back");
        }

        // ---------------- Portuguese at 390 px ----------------

        [UnityTest]
        public IEnumerator InPortuguese_EverySlotIsAWholeTouchTarget_WithItsWholePortugueseWord()
        {
            yield return Push(c => c.card_settings.preview_language = "pt");
            yield return UseTheBar();
            yield return Open(SheetStopRule.Stop.Full);
            Assert.AreEqual("Fechar", Sheet.CloseButton.tooltip, "precondition: the card reads in Portuguese");
            Assert.AreEqual(390f, Sheet.Layer.panel.visualTree.layout.width, OnePixel, "precondition: the run's 390 px frame");
            float touch = CardTestInput.TokenPx("--ts-touch-target");
            var slots = Bar.Items.Append(Bar.More).ToList();
            foreach (var tab in slots)
            {
                Assert.GreaterOrEqual(tab.Button.layout.width, touch - OnePixel, tab.Label.text + ": wide enough to tap");
                Assert.GreaterOrEqual(tab.Button.layout.height, touch - OnePixel, tab.Label.text + ": tall enough to tap");
                var words = tab.Label.MeasureTextSize(tab.Label.text, 0, VisualElement.MeasureMode.Undefined, 0, VisualElement.MeasureMode.Undefined);
                Assert.GreaterOrEqual(tab.Label.layout.width + 0.5f, words.x, "'" + tab.Label.text + "': whole, never cut");
                Assert.GreaterOrEqual(tab.Button.worldBound.xMin, Sheet.Root.worldBound.xMin - OnePixel, tab.Label.text + ": inside the card");
                Assert.LessOrEqual(tab.Button.worldBound.xMax, Sheet.Root.worldBound.xMax + OnePixel, tab.Label.text + ": inside the card");
            }
            Assert.AreEqual("Sobre", Bar.Items[0].Label.text, "the first slot reads in Portuguese");
            Assert.AreEqual("Mais", Bar.More.Label.text, "and More");
            yield return CardGalleryChecks.Render("Nav_Lamp_bottom_bar_pt_full_390");
        }
    }
}
