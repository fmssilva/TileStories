using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // Phase B of the step 6 top elements (_3.2 sections 5.1 and 9) on the REAL LivingRoomScene with The Lamp's real card (56 targets,
    // 8 families, the four default Groups) and real input, one run per element -- icon_tabs, icon_tabs_chips, kind_chips, segmented,
    // progress_map -- each made the wall's Top Bar through the live seam (the Editor's own path):
    //   - a real finger tap on EVERY item (brought into view by a real sideways drag when the strip hides it) lands the block that item
    //     leads to at the viewport's top (+-1 px; the range end for a late one), with one nav_jump naming the element
    //   - after each of six real drags of the stack, the element marks what the screen shows: the section / kind / group of the block
    //     under the spy line, read from the screen
    //   - in Portuguese every item is a whole touch target both ways, inside the card, and every visible word is whole and Portuguese
    // text_tabs has its own fixture (PoiCardTextTabsSceneTests); what shows at which stop is PoiCardChromeSceneTests'.
    public class PoiCardTopElementsSceneTests : SearchSceneFixture
    {
        private const string Lamp = "lamp";

        private static readonly string[] Elements =
        {
            BuiltInNavElements.IconTabs, BuiltInNavElements.IconTabsChips, BuiltInNavElements.KindChips, BuiltInNavElements.Segmented,
            BuiltInNavElements.ProgressMap,
        };

        private PoiCardSheetView Sheet => Card.Sheet;
        private NavController Nav => Sheet.Nav;
        private CardNavModel Model => Nav.Model;
        private ScrollView Scroll => Sheet.Stack.Scroll;

        private sealed class RecordingCardEvents : ICardEvents
        {
            public readonly List<CardEvent> Raised = new();
            public void Raise(CardEvent cardEvent) => Raised.Add(cardEvent);
        }

        private RecordingCardEvents _events;

        // One item of an element as a visitor meets it: the button to tap, the strip that may hide it (null = always in view), the block
        // a tap must land, and whether the element marks it now
        private readonly struct Item
        {
            public readonly string Name;
            public readonly Button Button;
            public readonly NavStrip Strip;
            public readonly string Block;
            public readonly System.Func<bool> Marked;

            public Item(string name, Button button, NavStrip strip, string block, System.Func<bool> marked)
            {
                Name = name;
                Button = button;
                Strip = strip;
                Block = block;
                Marked = marked;
            }
        }

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

        private IEnumerator Push(System.Action<WallConfigData> edit)
        {
            var copy = ConfigCopy();
            edit(copy);
            Session.ApplyCardSettings(copy);
            yield return CardTestInput.Settle(0.3f);
        }

        // The Lamp's card at Full with `element` as the wall's Top Bar, its blocks Grouped By Family (section 4.4's worked example: every
        // family one run, so a chip's jump stays inside the section whose chips are shown)
        private IEnumerator OpenWith(string element)
        {
            yield return Push(c =>
            {
                c.card_settings.navigation.top = element;
                c.card_settings.navigation.block_order = CardOptions.BlockOrderByFamily;
            });
            SelectionEventBus.Select(Lamp);
            yield return CardTestInput.Settle();
            Sheet.SetStop(SheetStopRule.Stop.Full);
            yield return CardTestInput.Settle();
            Assert.AreEqual(SheetStopRule.Stop.Full, Sheet.Stop, "precondition: the card rests at Full");
            Assert.AreEqual(element, Sheet.Slots.TopKey, "precondition: the slot holds " + element);
            Assert.IsTrue(Sheet.Chrome.TopShown, "precondition: the strip shows at Full");
            Assert.AreEqual(56, Model.TargetCount, "precondition: The Lamp's 56 targets");
        }

        private IEnumerator Landed()
        {
            for (int frame = 0; frame < 600 && Nav.IsJumping; frame++) yield return null;
            Assert.IsFalse(Nav.IsJumping, "the jump came to rest");
            yield return CardTestInput.Settle(0.3f);
        }

        private static IEnumerator TouchTap(IPanel panel, Vector2 at) => CardTestInput.TouchDrag(panel, at, Vector2.zero, frames: 1);

        // The target block the reader is in, read from the SCREEN: the spy's own rule over each target's slot as drawn
        private string BlockUnderTheSpyLine() =>
            Model.Targets[CardTestInput.UnderTheSpyLine(Scroll, Model.Targets.Select(t => Sheet.Stack.SlotOfBlock(t)))];

        // The section the reader is in, read from the SCREEN: the spy's own rule over each section's first slot as drawn
        private int SectionUnderTheSpyLine() =>
            CardTestInput.UnderTheSpyLine(Scroll, Model.Sections.Select(s => Sheet.Stack.SlotOfBlock(s.Targets[0])));

        // Every item the element offers now, with the block it leads to and its mark
        private List<Item> ItemsOf(INavElementView view)
        {
            var items = new List<Item>();
            switch (view)
            {
                case IconTabsChipsNavView both:
                    items.AddRange(Tabs(both.Tabs));
                    items.AddRange(Chips(both.Chips));
                    break;
                case TabsNavView tabs:
                    items.AddRange(Tabs(tabs));
                    break;
                case KindChipsNavView chips:
                    items.AddRange(Chips(chips.Row));
                    break;
                case SegmentedNavView segmented:
                    foreach (var s in segmented.Segments)
                        items.Add(new Item(s.Label.text, s.Button, null, Model.Groups[s.Index].Targets[0],
                            () => s.Button.ClassListContains(SegmentedNavView.CurrentClass)));
                    break;
                case ProgressMapNavView map:
                    foreach (var s in map.Segments)
                        items.Add(new Item(Model.Sections[s.Index].Key, s.Button, map.Strip, Model.Sections[s.Index].Targets[0],
                            () => s.Button.ClassListContains(ProgressMapNavView.CurrentClass)));
                    break;
                default:
                    Assert.Fail("no items for " + view);
                    break;
            }
            return items;
        }

        private IEnumerable<Item> Tabs(TabsNavView tabs) => tabs.Tabs.Select(t => new Item(t.Label.text, t.Button, tabs.Strip,
            Model.Sections[t.Index].Targets[0], () => t.Button.ClassListContains(NavTab.CurrentClass)));

        private IEnumerable<Item> Chips(NavChipRow row) => row.Chips.Select(c => new Item(c.Label.text, c.Button, row.Strip, c.Kind.Targets[0],
            () => c.Button.ClassListContains(NavChip.CurrentClass)));

        // An item wholly inside its strip's visible part, after a real sideways drag of the strip when it is not
        private IEnumerator BringIntoView(Item item)
        {
            if (item.Strip == null) yield break;
            var visible = item.Strip.Viewport.worldBound;
            var rect = item.Button.worldBound;
            if (rect.xMin < visible.xMin - 0.5f || rect.xMax > visible.xMax + 0.5f)
            {
                float dx = visible.center.x - rect.center.x;
                yield return CardTestInput.TouchDrag(Sheet.Root.panel, new Vector2(visible.center.x, visible.center.y), new Vector2(dx, 0f));
                yield return null;
            }
            rect = item.Button.worldBound;
            Assert.GreaterOrEqual(rect.xMin, visible.xMin - 0.5f, "precondition: " + item.Name + " is inside its strip " + rect + " in " + visible);
            Assert.LessOrEqual(rect.xMax, visible.xMax + 0.5f, "precondition: " + item.Name + " is inside its strip " + rect + " in " + visible);
        }

        // What the element marks matches the screen: the section (tabs, progress map), the kind (chips) or the group (segments) of what
        // the reader is at -- and exactly one of its items is marked
        private void AssertMarksTheScreen(INavElementView view, string when)
        {
            string block = BlockUnderTheSpyLine();
            Assert.AreEqual(block, Nav.CurrentBlock, when + ": the controller's block is the one on screen");
            switch (view)
            {
                case IconTabsChipsNavView both:
                    AssertMarksTheScreen(both.Tabs, when);
                    if (both.ChipsSection >= 0)
                    {
                        var kind = Model.KindOf(block);
                        var expected = both.Chips.Chips.Any(c => c.Kind == kind) ? kind : null;
                        Assert.AreEqual(expected, both.Chips.Chips.SingleOrDefault(c => c.Button.ClassListContains(NavChip.CurrentClass))?.Kind,
                            when + ": the chip of the kind on screen (none when that kind is not in the row)");
                    }
                    break;
                case TabsNavView tabs:
                    Assert.AreEqual(SectionUnderTheSpyLine(), tabs.CurrentIndex, when + ": the tab of the section on screen");
                    Assert.AreEqual(1, tabs.Tabs.Count(t => t.Button.ClassListContains(NavTab.CurrentClass)), when + ": one tab marked");
                    break;
                case KindChipsNavView chips:
                    var marked = chips.Row.Chips.Where(c => c.Button.ClassListContains(NavChip.CurrentClass)).ToList();
                    Assert.AreEqual(1, marked.Count, when + ": one chip marked");
                    Assert.AreEqual(Model.KindOf(block), marked[0].Kind, when + ": the chip of the kind on screen");
                    break;
                case SegmentedNavView segmented:
                    Assert.AreEqual(Model.GroupOf(block), segmented.CurrentIndex, when + ": the segment of the group on screen");
                    Assert.AreEqual(1, segmented.Segments.Count(s => s.Button.ClassListContains(SegmentedNavView.CurrentClass)), when + ": one segment marked");
                    break;
                case ProgressMapNavView map:
                    Assert.AreEqual(SectionUnderTheSpyLine(), map.CurrentIndex, when + ": the piece of the section on screen");
                    break;
            }
        }

        // ---------------- a real tap on every item ----------------

        [UnityTest]
        public IEnumerator ARealTapOnEveryItem_LandsItsBlockAtTheViewportTop_AndTheMarkFollows([ValueSource(nameof(Elements))] string element)
        {
            yield return OpenWith(element);
            var view = Sheet.Slots.TopView;
            var items = ItemsOf(view);
            Assert.GreaterOrEqual(items.Count, 4, "precondition: the element offers several items on The Lamp");
            // - icon_tabs_chips: the chips of EVERY section with two kinds or more, reached by tapping its tab first
            var sectionsWithChips = view is IconTabsChipsNavView
                ? Enumerable.Range(0, Model.Sections.Count).Where(i => NavOntologyRule.OffersAKindChoice(Model.Sections[i].Kinds.Count)).ToList()
                : new List<int>();

            for (int i = items.Count - 1; i >= 0; i--)
                yield return TapAndLand(items[i], element);

            foreach (int section in sectionsWithChips)
            {
                var both = (IconTabsChipsNavView)view;
                var tab = Tabs(both.Tabs).ElementAt(section);
                yield return TapAndLand(tab, element);
                // - a late section's tab clamps at the range end, where the spy names the LAST section: its chips are reached by scrolling
                if (Sheet.Stack.SlotOfBlock(Model.Sections[section].Targets[0]).layout.y > Range - OnePixel) continue;
                Assert.AreEqual(section, both.ChipsSection, Model.Sections[section].Key + ": its tab shows its kinds as chips");
                foreach (var chip in Chips(both.Chips).ToList()) yield return TapAndLand(chip, element);
            }
        }

        // A real finger taps the item: the block it leads to lands at the viewport's top (or the range end), one nav_jump names the element,
        // and the element marks what the screen then shows
        private IEnumerator TapAndLand(Item item, string element)
        {
            yield return BringIntoView(item);
            int jumpsBefore = NavJumps.Count;
            yield return TouchTap(Sheet.Root.panel, item.Button.worldBound.center);
            yield return Landed();
            var slot = Sheet.Stack.SlotOfBlock(item.Block);
            Assert.AreEqual(SheetStopRule.Stop.Full, Sheet.Stop, item.Name + ": the card stays at Full");
            if (slot.layout.y <= Range - OnePixel)
                Assert.AreEqual(0f, slot.worldBound.yMin - Scroll.contentViewport.worldBound.yMin, OnePixel, item.Name + ": its block's top meets the viewport's top");
            else
                Assert.AreEqual(Range, Scroll.scrollOffset.y, OnePixel, item.Name + ": a late block clamps at the end of the range");
            Assert.AreEqual(jumpsBefore + 1, NavJumps.Count, item.Name + ": one nav_jump");
            Assert.AreEqual(element, NavJumps.Last().Variant, item.Name + ": the event names the element");
            Assert.AreEqual(item.Block, NavJumps.Last().BlockKey, item.Name + ": and the block it landed on");
            AssertMarksTheScreen(Sheet.Slots.TopView, item.Name);
        }

        // ---------------- the mark follows a real drag ----------------

        // A touch drag's stack keeps gliding after the finger lifts (the ScrollView's inertia): 0.6 s after the release it still moved 8-20 px
        // over the next three frames in all 30 drags measured (2026-10-22), so a check then read a moving target and once named the block
        // before the one the controller had reached. Wait until the offset holds for three frames -- the stack at rest -- and say so.
        private IEnumerator StackAtRest(string when)
        {
            const int StillFrames = 3;
            const int MaxFrames = 600;
            float last = Scroll.scrollOffset.y;
            int still = 0;
            for (int frame = 0; frame < MaxFrames && still < StillFrames; frame++)
            {
                yield return null;
                float now = Scroll.scrollOffset.y;
                still = Mathf.Approximately(now, last) ? still + 1 : 0;
                last = now;
            }
            Assert.AreEqual(StillFrames, still, "precondition, " + when + ": the stack came to rest within " + MaxFrames + " frames");
        }

        [UnityTest]
        public IEnumerator ARealDragOfTheStack_MovesTheMark_ToWhatTheScreenShows([ValueSource(nameof(Elements))] string element)
        {
            yield return OpenWith(element);
            var view = Sheet.Slots.TopView;
            AssertMarksTheScreen(view, "at the top");
            var viewport = Scroll.contentViewport.worldBound;
            var seen = new HashSet<string> { Nav.CurrentBlock };
            for (int drag = 0; drag < 6; drag++)
            {
                float before = Scroll.scrollOffset.y;
                yield return CardTestInput.TouchDrag(Scroll.panel, new Vector2(viewport.center.x, viewport.yMax - 40f), new Vector2(0f, -viewport.height * 0.6f));
                yield return StackAtRest("drag " + (drag + 1));
                Assert.Greater(Scroll.scrollOffset.y, before, "precondition: the real drag scrolled the stack");
                AssertMarksTheScreen(view, "after drag " + (drag + 1));
                seen.Add(Nav.CurrentBlock);
            }
            Assert.Greater(seen.Count, 3, "the mark really moved with the reader");
        }

        // ---------------- Portuguese at 390 px ----------------

        [UnityTest]
        public IEnumerator InPortuguese_EveryItemIsAWholeTouchTarget_InsideTheCard_WithWholePortugueseWords([ValueSource(nameof(Elements))] string element)
        {
            yield return Push(c => c.card_settings.preview_language = "pt");
            yield return OpenWith(element);
            Assert.AreEqual("Fechar", Sheet.CloseButton.tooltip, "precondition: the card reads in Portuguese");
            var root = Sheet.Slots.TopView.Root;
            var card = Sheet.Root.worldBound;
            float onePixel = OnePixel;
            Assert.AreEqual(390f, Sheet.Layer.panel.visualTree.layout.width, onePixel, "precondition: the run's 390 px frame");
            Assert.GreaterOrEqual(root.worldBound.xMin, card.xMin - onePixel, "the element starts inside the card");
            Assert.LessOrEqual(root.worldBound.xMax, card.xMax + onePixel, "and ends inside it");

            float touch = CardTestInput.TokenPx("--ts-touch-target");
            var items = ItemsOf(Sheet.Slots.TopView);
            foreach (var item in items)
            {
                Assert.GreaterOrEqual(item.Button.layout.width, touch - onePixel, item.Name + ": wide enough to tap");
                Assert.GreaterOrEqual(item.Button.layout.height, touch - onePixel, item.Name + ": tall enough to tap");
            }
            foreach (var label in root.Query<Label>().ToList().Where(l => l.text.Length > 0 && CardTestInput.IsShown(l, Sheet.Root)))
            {
                bool wraps = label.resolvedStyle.whiteSpace == WhiteSpace.Normal;
                string widest = wraps ? label.text.Split(' ').OrderByDescending(w => label.MeasureTextSize(w, 0, VisualElement.MeasureMode.Undefined, 0,
                    VisualElement.MeasureMode.Undefined).x).First() : label.text;
                var size = label.MeasureTextSize(widest, 0, VisualElement.MeasureMode.Undefined, 0, VisualElement.MeasureMode.Undefined);
                Assert.GreaterOrEqual(label.layout.width + 0.5f, size.x, "'" + widest + "': whole, never cut");
            }
            // - the words are the Portuguese ones: the first section, kind and group
            string words = element switch
            {
                BuiltInNavElements.KindChips => ((KindChipsNavView)Sheet.Slots.TopView).Row.Chips[0].Label.text,
                BuiltInNavElements.Segmented => ((SegmentedNavView)Sheet.Slots.TopView).Segments[0].Label.text,
                BuiltInNavElements.ProgressMap => ((ProgressMapNavView)Sheet.Slots.TopView).Segments[0].Button.tooltip,
                BuiltInNavElements.IconTabsChips => ((IconTabsChipsNavView)Sheet.Slots.TopView).Tabs.Tabs[0].Label.text,
                _ => ((TabsNavView)Sheet.Slots.TopView).Tabs[0].Label.text,
            };
            string expected = element switch
            {
                BuiltInNavElements.KindChips => Model.Sections[0].Kinds[0].Key == BuiltInBlocks.RichTextKind ? "Texto" : null,
                BuiltInNavElements.Segmented => "Descobrir",
                _ => "Sobre",
            };
            if (expected != null) Assert.AreEqual(expected, words, "the first item reads in Portuguese");
            else Assert.AreNotEqual(Model.Sections[0].Kinds[0].Key, words, "a kind chip reads its words, never its key");
            yield return CardGalleryChecks.Render("Nav_Lamp_" + element + "_pt_full_390");
        }
    }
}
