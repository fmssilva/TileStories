using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // The three navigation options a wall author can choose and that nothing observed working (_3.2 audit F1, mutations 200 and 210-214),
    // on the REAL LivingRoomScene with The Lamp's real card, each through the live seam the Editor uses:
    //   - Follow Scroll: off, a real finger drag past other sections leaves the mark where the last jump put it (a jump still moves it);
    //     on, the mark follows the reader
    //   - Show Counts: every element the registry says shows counts draws each item's block count (the model's), and none when off
    //   - Labels: every element the registry says reads Labels draws the icon, the words or both, as the setting says
    // The element lists come from NavElementRegistry's own flags, so an element added later is covered without a new line here.
    public class PoiCardNavOptionsSceneTests : SearchSceneFixture
    {
        private const string Element = "scene_test_element";
        private const string Lamp = "lamp";

        private static IEnumerable<string> ElementsThatShowCounts() => NavElementRegistry.Shared.All.Where(e => e.ShowsCounts).Select(e => e.Key);
        private static IEnumerable<string> ElementsThatReadLabels() => NavElementRegistry.Shared.All.Where(e => e.ReadsLabels).Select(e => e.Key);
        private static IEnumerable<string> LabelSettings() => CardOptions.Labels;

        private PoiCardSheetView Sheet => Card.Sheet;
        private NavController Nav => Sheet.Nav;
        private CardNavModel Model => Nav.Model;
        private ScrollView Scroll => Sheet.Stack.Scroll;

        // An in-memory card state: a run never touches the developer's saved answers, and a point read earlier (its Show After Reading
        // block revealed) never changes the card these tests count
        [UnitySetUp]
        public IEnumerator UseAnInMemoryCardState()
        {
            Card.State = new CardLocalState(new MemoryCardStateStore(), Session.SearchConfig.wall_id);
            yield break;
        }

        // ---------------- helpers ----------------

        private float OnePixel => Sheet.Layer.panel.visualTree.layout.height / Screen.height;

        private float Range => NavScrollRule.Range(Scroll.contentContainer.layout.height, Scroll.contentViewport.layout.height);

        // Push an edit the way the Editor's live applier does: a private copy of the running config, through the wall's own seam
        private IEnumerator Push(System.Action<CardNavigationSettings> edit) => PushConfig(c => edit(c.card_settings.navigation));

        private IEnumerator PushConfig(System.Action<WallConfigData> edit)
        {
            var copy = ConfigCopy();
            edit(copy);
            Session.ApplyCardSettings(copy);
            yield return CardTestInput.Settle(0.3f);
        }

        // The Lamp's card at Full, its blocks Grouped By Family (every family one run: section 4.4's worked example)
        private IEnumerator OpenTheLamp()
        {
            // - Select toggles (re-selecting the selected point clears it): start from nothing selected
            SelectionEventBus.Clear();
            yield return CardTestInput.Settle();
            SelectionEventBus.Select(Lamp);
            yield return CardTestInput.Settle();
            Sheet.SetStop(SheetStopRule.Stop.Full);
            yield return CardTestInput.Settle();
            Assert.AreEqual(SheetStopRule.Stop.Full, Sheet.Stop, "precondition: the card rests at Full");
            float rest = Sheet.Stops.HeightOf(Sheet.Stop);
            for (int frame = 0; frame < 600 && Mathf.Abs(Sheet.Root.layout.height - rest) > OnePixel; frame++) yield return null;
            Assert.AreEqual(rest, Sheet.Root.layout.height, OnePixel, "precondition: the sheet rests at its Full height");
            yield return CardTestInput.Settle(0.3f);
            Assert.IsNotNull(Model, "precondition: the sheet built the card's navigation model");
            Assert.AreEqual(56, Model.TargetCount, "precondition: The Lamp's 56 targets");
        }

        private IEnumerator Landed()
        {
            for (int frame = 0; frame < 600 && Nav.IsJumping; frame++) yield return null;
            Assert.IsFalse(Nav.IsJumping, "the jump came to rest");
            yield return CardTestInput.Settle(0.3f);
        }

        // The section the reader is in, read from the SCREEN (each section's first slot against the spy line, or the last at the end)
        private int SectionUnderTheSpyLine()
        {
            if (Range > 0f && Scroll.scrollOffset.y >= Range - NavScrollRule.EndTolerance) return Model.Sections.Count - 1;
            var viewport = Scroll.contentViewport.worldBound;
            float line = viewport.yMin + viewport.height * NavScrollRule.SpyLineFraction;
            int found = 0;
            for (int i = 0; i < Model.Sections.Count; i++)
                if (Sheet.Stack.SlotOfBlock(Model.Sections[i].Targets[0]).worldBound.yMin <= line) found = i;
            return found;
        }

        // Real finger drags of the stack, upward (the reader goes down the card) until the screen shows `minSection` or a later one
        private IEnumerator DragDownTheCardUntilTheScreenShows(int minSection)
        {
            var viewport = Scroll.contentViewport.worldBound;
            for (int drag = 0; drag < 60 && SectionUnderTheSpyLine() < minSection; drag++)
            {
                yield return CardTestInput.TouchDrag(Scroll.panel, viewport.center, new Vector2(0f, -viewport.height * 0.6f));
                yield return CardTestInput.Settle(0.3f);
            }
            yield return CardTestInput.Settle(0.6f);
        }

        // ---------------- Follow Scroll ----------------

        [UnityTest]
        public IEnumerator FollowScroll_Off_KeepsTheMarkWhereTheLastJumpPutIt_OnFollowsTheReader([Values(false, true)] bool follow)
        {
            yield return Push(n =>
            {
                n.top = BuiltInNavElements.TextTabs;
                n.block_order = CardOptions.BlockOrderByFamily;
                n.follow_scroll = follow;
            });
            Assert.AreEqual(follow, Session.CardSettings.navigation.follow_scroll, "precondition: the wall says Follow Scroll " + follow);
            yield return OpenTheLamp();
            var tabs = (TabsNavView)Sheet.Slots.TopView;
            Assert.GreaterOrEqual(Model.Sections.Count, 5, "precondition: several sections to read past");

            const int jumped = 1;
            Assert.IsTrue(Nav.JumpTo(jumped, Element));
            yield return Landed();
            Assert.AreEqual(jumped, Nav.Current, "precondition: the jump marked its section");
            Assert.AreEqual(jumped, tabs.CurrentIndex, "precondition: and its tab");

            yield return DragDownTheCardUntilTheScreenShows(jumped + 2);
            int onScreen = SectionUnderTheSpyLine();
            Assert.GreaterOrEqual(onScreen, jumped + 2, "precondition: real drags carried the screen past other sections (offset " + Scroll.scrollOffset.y + ")");

            if (follow)
            {
                Assert.AreEqual(onScreen, Nav.Current, "on: the controller names the section on screen");
                Assert.AreEqual(onScreen, tabs.CurrentIndex, "on: and its tab is the marked one");
                Assert.AreEqual(1, tabs.Tabs.Count(t => t.Button.ClassListContains(NavTab.CurrentClass)), "on: one tab marked");
                yield break;
            }
            Assert.AreEqual(jumped, Nav.Current, "off: the mark stayed on the section the last jump put it on, the screen shows " + onScreen);
            Assert.AreEqual(Model.Sections[jumped].Targets[0], Nav.CurrentBlock, "off: and the block mark too");
            Assert.AreEqual(jumped, tabs.CurrentIndex, "off: the tab is the one the jump marked");
            Assert.AreEqual(1, tabs.Tabs.Count(t => t.Button.ClassListContains(NavTab.CurrentClass)), "off: one tab marked");

            // - a jump still moves it, and the scroll after it does not
            Assert.IsTrue(Nav.JumpTo(jumped + 1, Element));
            yield return Landed();
            Assert.AreEqual(jumped + 1, Nav.Current, "off: a jump still moves the mark");
            Assert.AreEqual(jumped + 1, tabs.CurrentIndex, "off: and its tab");
            float before = Scroll.scrollOffset.y;
            var viewport = Scroll.contentViewport.worldBound;
            yield return CardTestInput.TouchDrag(Scroll.panel, viewport.center, new Vector2(0f, -viewport.height * 0.6f));
            yield return CardTestInput.Settle(0.6f);
            Assert.Greater(Mathf.Abs(Scroll.scrollOffset.y - before), OnePixel, "precondition: the drag after the jump scrolled the stack");
            Assert.AreEqual(jumped + 1, Nav.Current, "off: the drag after the jump left the mark alone");
        }

        // ---------------- Show Counts ----------------

        // Every count label of an element: a name, the label and the count the card's model says it leads to
        private List<(string Name, Label Label, int Expected)> CountsOf(INavElementView view)
        {
            var counts = new List<(string, Label, int)>();
            void AddTabs(TabsNavView tabs)
            {
                foreach (var t in tabs.Tabs) counts.Add(("tab " + t.Label.text, t.Count, Model.Sections[t.Index].Count));
            }
            void AddChips(NavChipRow row)
            {
                foreach (var c in row.Chips) counts.Add(("chip " + c.Label.text, c.Count, c.Kind.Count));
            }
            switch (view)
            {
                case IconTabsChipsNavView both:
                    AddTabs(both.Tabs);
                    Assert.GreaterOrEqual(both.ChipsSection, 0, "precondition: the first section holds several kinds, so its chips show");
                    AddChips(both.Chips);
                    break;
                case TabsNavView tabs:
                    AddTabs(tabs);
                    break;
                case KindChipsNavView chips:
                    AddChips(chips.Row);
                    break;
                case SegmentedNavView segmented:
                    foreach (var s in segmented.Segments) counts.Add(("segment " + s.Label.text, s.Count, Model.Groups[s.Index].Count));
                    break;
                default:
                    Assert.Fail("no count labels known for " + view);
                    break;
            }
            return counts;
        }

        [UnityTest]
        public IEnumerator ShowCounts_DrawsEachItemsBlockCount_OnEveryElementThatOffersIt_AndNothingWhenOff(
            [ValueSource(nameof(ElementsThatShowCounts))] string element, [Values(true, false)] bool showCounts)
        {
            Assert.IsTrue(NavElementRegistry.Shared.TryGet(element, out _), "precondition: " + element + " is registered");
            yield return Push(n =>
            {
                n.top = element;
                n.block_order = CardOptions.BlockOrderByFamily;
                n.show_counts = showCounts;
            });
            yield return OpenTheLamp();
            Assert.AreEqual(element, Sheet.Slots.TopKey, "precondition: the slot holds " + element);
            Assert.IsTrue(Sheet.Chrome.TopShown, "precondition: the strip shows at Full");

            var items = CountsOf(Sheet.Slots.TopView);
            Assert.GreaterOrEqual(items.Count, 3, element + ": precondition: several items on The Lamp");
            foreach (var (name, label, expected) in items)
            {
                Assert.Greater(expected, 0, name + ": precondition: the model leads somewhere");
                if (showCounts)
                {
                    Assert.AreEqual(expected.ToString(), label.text, element + " " + name + ": the count of the blocks it leads to");
                    Assert.IsTrue(CardTestInput.IsShown(label, Sheet.Root), element + " " + name + ": the count is displayed");
                    Assert.Greater(label.worldBound.width, 0f, element + " " + name + ": and has a width on screen");
                }
                else
                {
                    Assert.AreEqual("", label.text, element + " " + name + ": Show Counts off writes no number");
                    Assert.IsFalse(CardTestInput.IsShown(label, Sheet.Root), element + " " + name + ": and none is displayed");
                }
            }
            if (showCounts)
            {
                Assert.Greater(items.Select(i => i.Expected).Distinct().Count(), 1, element + ": the counts differ item to item (not one number everywhere)");
                if (Sheet.Slots.TopView is TabsNavView || Sheet.Slots.TopView is KindChipsNavView)
                    Assert.AreEqual(Model.TargetCount, items.Sum(i => i.Expected), element + ": every target of the card is counted once");
            }
            yield return CardGalleryChecks.Render("NavOptions_" + element + "_counts_" + (showCounts ? "on" : "off") + "_390");
        }

        // ---------------- Labels ----------------

        private static IEnumerable<NavTab> TabsOf(INavElementView view)
        {
            switch (view)
            {
                case IconTabsChipsNavView both: return both.Tabs.Tabs;
                case TabsNavView tabs: return tabs.Tabs;
                case BottomBarNavView bar: return bar.Items;
                default:
                    Assert.Fail("no tabs known for " + view);
                    return null;
            }
        }

        [UnityTest]
        public IEnumerator Labels_DrawTheIconTheWordsOrBoth_OnEveryElementThatReadsThem(
            [ValueSource(nameof(ElementsThatReadLabels))] string element, [ValueSource(nameof(LabelSettings))] string labels)
        {
            Assert.IsTrue(NavElementRegistry.Shared.TryGet(element, out var definition));
            bool top = definition.Slot == NavSlotKeys.Top;
            yield return Push(n =>
            {
                n.top = top ? element : CardOptions.NavNone;
                n.bottom = top ? CardOptions.NavNone : element;
                n.block_order = CardOptions.BlockOrderByFamily;
                n.labels = labels;
            });
            yield return OpenTheLamp();
            var view = top ? Sheet.Slots.TopView : Sheet.Slots.BottomView;
            Assert.AreEqual(element, top ? Sheet.Slots.TopKey : Sheet.Slots.BottomKey, "precondition: the slot holds " + element);
            Assert.IsTrue(top ? Sheet.Chrome.TopShown : Sheet.Chrome.BottomShown, "precondition: the element shows at Full");

            var tabs = TabsOf(view).ToList();
            Assert.GreaterOrEqual(tabs.Count, 3, element + ": precondition: several items on The Lamp");
            bool wantsIcon = labels != CardOptions.LabelsText;
            bool wantsWords = labels != CardOptions.LabelsIcon;
            foreach (var tab in tabs)
            {
                string name = element + " / " + labels + " / " + tab.Label.text;
                Assert.IsNotEmpty(tab.Label.text, name + ": precondition: the item has its words");
                Assert.AreEqual(wantsIcon, CardTestInput.IsShown(tab.Icon, Sheet.Root), name + ": the icon is " + (wantsIcon ? "drawn" : "left out"));
                Assert.AreEqual(wantsWords, CardTestInput.IsShown(tab.Label, Sheet.Root), name + ": the words are " + (wantsWords ? "drawn" : "left out"));
                if (wantsIcon) Assert.Greater(tab.Icon.worldBound.width, 0f, name + ": the icon has a size on screen");
                if (wantsWords) Assert.Greater(tab.Label.worldBound.width, 0f, name + ": the words have a width on screen");
                Assert.AreEqual(tab.Label.text, tab.Button.tooltip, name + ": the item is named for a screen reader whichever of the two shows");
            }
            yield return CardGalleryChecks.Render("NavOptions_" + element + "_labels_" + labels + "_390");
        }

        // ---------------- Group By, the Families table and the Groups table (_3.2 audit section 2: no real-card test) ----------------

        private string FormatOf(string block)
        {
            Assert.IsTrue(Nav.TryGetEntry(block, out var entry), "precondition: " + block + " is a block of the card");
            return entry.Definition.Format;
        }

        private string FamilyOf(string block)
        {
            Assert.IsTrue(Nav.TryGetEntry(block, out var entry), "precondition: " + block + " is a block of the card");
            return entry.Definition.Family;
        }

        [UnityTest]
        public IEnumerator GroupByFormat_MakesTheSectionsTheFormatsTheCardHolds_AndAnIconTabShowsItsWordsWhateverLabelsSays()
        {
            yield return Push(n =>
            {
                n.top = BuiltInNavElements.IconTabs;
                n.group_by = CardOptions.GroupByFormat;
                n.labels = CardOptions.LabelsIcon;
            });
            yield return OpenTheLamp();
            var formats = Model.Targets.Select(FormatOf).Distinct().ToList();
            var tabs = ((TabsNavView)Sheet.Slots.TopView).Tabs;
            Assert.GreaterOrEqual(formats.Count, 3, "precondition: The Lamp holds several formats");
            Assert.AreEqual(formats.Count, tabs.Count, "one tab per format the card holds (" + string.Join(", ", formats) + ")");
            foreach (var tab in tabs)
            {
                string key = Model.Sections[tab.Index].Key;
                CollectionAssert.Contains(CardOptions.Formats, key, "the section is a format, not a family");
                Assert.AreEqual(Model.Sections[tab.Index].Targets.Count, Model.Sections[tab.Index].Targets.Count(t => FormatOf(t) == key),
                    key + ": the section holds exactly the blocks of that format");
                Assert.IsNotEmpty(tab.Label.text, key + ": the tab has words");
                Assert.AreNotEqual(key, tab.Label.text, key + ": the visitor's word, never the key");
                Assert.IsFalse(CardTestInput.IsShown(tab.Icon, Sheet.Root), key + ": a format has no icon to draw");
                Assert.IsTrue(CardTestInput.IsShown(tab.Label, Sheet.Root), key + ": Labels = icon still shows the words where no icon stands for them");
            }
            Assert.AreEqual(Model.TargetCount, Model.Sections.Sum(s => s.Targets.Count), "every target is in exactly one format section");
        }

        [UnityTest]
        public IEnumerator AFamiliesRowsOrderAndAtTheEnd_MoveItsSectionOnTheRealCard()
        {
            yield return Push(n =>
            {
                n.top = BuiltInNavElements.TextTabs;
                n.block_order = CardOptions.BlockOrderByFamily;
            });
            yield return OpenTheLamp();
            var before = Model.Sections.Select(s => s.Key).ToList();
            Assert.GreaterOrEqual(before.Count, 5, "precondition: several families");
            string last = before.Last(), first = before.First();
            Assert.AreNotEqual(first, last);
            Assert.Greater(Sheet.Stack.SlotOfBlock(Model.Sections.Single(s => s.Key == last).Targets[0]).layout.y,
                Sheet.Stack.SlotOfBlock(Model.Sections.Single(s => s.Key == first).Targets[0]).layout.y, "precondition: " + last + " is below " + first);

            // - Order: the last family's row puts it before every other
            yield return PushConfig(c => c.card_settings.navigation.families = new List<CardFamilySetting> { new() { family = last, order = -1000 } });
            Assert.AreEqual(last, Model.Sections[0].Key, "Order: the family with the lowest order is the first section");
            var tabs = ((TabsNavView)Sheet.Slots.TopView).Tabs;
            Assert.AreEqual(Model.Sections.Count, tabs.Count, "the strip has one tab per section");
            Assert.AreEqual(Model.Sections[0].Key, Model.Sections[tabs[0].Index].Key, "its tab is the first");
            float lastTop = Sheet.Stack.SlotOfBlock(Model.Sections[0].Targets[0]).layout.y;
            foreach (var section in Model.Sections.Skip(1))
                Assert.Less(lastTop, Sheet.Stack.SlotOfBlock(section.Targets[0]).layout.y, section.Key + ": on the card, " + last + " now comes before it");

            // - At The End (read by the End Families Last block order; By Family reads Order): the first family's row sends it behind every
            //   family that does not close the card by default
            yield return PushConfig(c =>
            {
                c.card_settings.navigation.block_order = CardOptions.BlockOrderEndFamiliesLast;
                c.card_settings.navigation.families = new List<CardFamilySetting> { new() { family = first, at_end = true } };
            });
            var after = Model.Sections.Select(s => s.Key).ToList();
            foreach (string key in after.Where(k => k != first && !(BlockRegistry.Shared.TryGetFamily(k, out var f) && f.AtEnd)))
                Assert.Greater(after.IndexOf(first), after.IndexOf(key), "At The End: " + first + " now closes the card, behind " + key);
            float firstTop = Sheet.Stack.SlotOfBlock(Model.Sections.Single(s => s.Key == first).Targets[0]).layout.y;
            foreach (var section in Model.Sections.Where(s => s.Key != first && !(BlockRegistry.Shared.TryGetFamily(s.Key, out var f) && f.AtEnd)))
                Assert.Greater(firstTop, Sheet.Stack.SlotOfBlock(section.Targets[0]).layout.y, section.Key + ": on the card, " + first + " is below it");
        }

        [UnityTest]
        public IEnumerator CustomGroups_AreTheSegmentsOfTheSegmentedElement_WithTheirWordsAndTheirFamiliesBlocks()
        {
            yield return PushConfig(c =>
            {
                c.card_settings.preview_language = "en";
                c.card_settings.navigation.top = BuiltInNavElements.Segmented;
                c.card_settings.navigation.show_counts = true;
            });
            yield return OpenTheLamp();
            var families = Model.Sections.Select(s => s.Key).ToList();
            Assert.GreaterOrEqual(families.Count, 4, "precondition: several families to share between two groups");
            var inAlpha = families.Where((f, i) => i % 2 == 0).ToList();

            yield return PushConfig(c =>
            {
                var nav = c.card_settings.navigation;
                nav.groups = new List<CardNavGroup>
                {
                    new() { key = "group_alpha", label = new List<LocalizedEntry> { new() { lang = "en", value = "Alpha" } } },
                    new() { key = "group_beta", label = new List<LocalizedEntry> { new() { lang = "en", value = "Beta" } } },
                };
                nav.families = families.Select(f => new CardFamilySetting { family = f, group = inAlpha.Contains(f) ? "group_alpha" : "group_beta" }).ToList();
            });
            var view = Sheet.Slots.TopView as SegmentedNavView;
            Assert.IsNotNull(view, "the Top Bar still draws the segmented element (" + Sheet.Slots.TopKey + ")");
            var segments = view.Segments;
            Assert.AreEqual(2, segments.Count, "the wall's two groups, not the framework's four");
            Assert.AreEqual("Alpha", segments[0].Label.text);
            Assert.AreEqual("Beta", segments[1].Label.text);
            int alpha = Model.Targets.Count(t => inAlpha.Contains(FamilyOf(t)));
            Assert.Greater(alpha, 0);
            Assert.AreEqual(alpha.ToString(), segments[0].Count.text, "Alpha counts the blocks of the families that joined it");
            Assert.AreEqual((Model.TargetCount - alpha).ToString(), segments[1].Count.text, "Beta counts the rest");
        }
    }
}
