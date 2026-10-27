using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // The combinations of navigation options the audit rendered for the first time (_3.2 audit F2 and F6, fix chunk C), on the REAL
    // LivingRoomScene with The Lamp's real card at the run's 390 px frame, through the live seam the Editor uses:
    //   - Segmented with Show Counts: a word is never cut by its count -- the counts go instead (NavFitRule), all of them, or stay when
    //     every word holds its own
    // Each test fails when its fix is taken out (mutation-proven, `_3.2.1_Audit.md` section 11).
    public class PoiCardNavCombinationsSceneTests : SearchSceneFixture
    {
        private const string Lamp = "lamp";

        private PoiCardSheetView Sheet => Card.Sheet;
        private NavController Nav => Sheet.Nav;
        private CardNavModel Model => Nav.Model;

        // An in-memory card state: a run never touches the developer's saved answers, and a point read earlier never changes the card
        [UnitySetUp]
        public IEnumerator UseAnInMemoryCardState()
        {
            Card.State = new CardLocalState(new MemoryCardStateStore(), Session.SearchConfig.wall_id);
            yield break;
        }

        // ---------------- helpers ----------------

        private float OnePixel => Sheet.Layer.panel.visualTree.layout.height / Screen.height;

        // Push an edit the way the Editor's live applier does: a private copy of the running config, through the wall's own seam
        private IEnumerator PushConfig(System.Action<WallConfigData> edit)
        {
            var copy = ConfigCopy();
            edit(copy);
            Session.ApplyCardSettings(copy);
            yield return CardTestInput.Settle(0.3f);
        }

        private IEnumerator Push(System.Action<CardNavigationSettings> edit) => PushConfig(c => edit(c.card_settings.navigation));

        // The Lamp's card at Full, its blocks Grouped By Family
        private IEnumerator OpenTheLamp()
        {
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

        // The card at rest after a tap or a jump: no glide going, the layout settled
        private IEnumerator Rested()
        {
            for (int frame = 0; frame < 600 && Nav.IsJumping; frame++) yield return null;
            Assert.IsFalse(Nav.IsJumping, "the jump came to rest");
            yield return CardTestInput.Settle(0.4f);
        }

        // ---------------- Next Up under Pages and Tabs (audit F2-3: two counters that disagreed) ----------------

        private static IEnumerable<string> LayoutsThatShowOneSectionAtATime() => new[] { CardOptions.LayoutPages, CardOptions.LayoutTabs };

        // Turn the card to section `index` with a real tap on what the layout offers: the pager's chevron (pages), the section's tab (tabs)
        private IEnumerator TurnTo(int index)
        {
            for (int guard = 0; guard < 20 && Nav.Current != index; guard++)
            {
                VisualElement button;
                if (Sheet.Slots.TopView is FamilyPagerNavView pager) button = pager.Next;
                else if (Sheet.Slots.TopView is TabsNavView tabs) button = tabs.Tabs.First(t => t.Index == index).Button;
                else { Assert.Fail("no way to turn the card in " + Sheet.Slots.TopKey); yield break; }
                yield return CardTestInput.Tap(button.panel, button.worldBound.center);
                yield return Rested();
            }
            Assert.AreEqual(index, Nav.Current, "precondition: a real tap turned the card to section " + index);
        }

        [UnityTest]
        public IEnumerator NextUp_AtTheTopOfAPage_NamesABlockOfThatPage_NotTheCardsFirst(
            [ValueSource(nameof(LayoutsThatShowOneSectionAtATime))] string layout)
        {
            yield return Push(n =>
            {
                n.layout = layout;
                n.bottom = BuiltInNavElements.NextUp;
                n.top = BuiltInNavElements.TextTabs;
            });
            yield return OpenTheLamp();
            var nextUp = Sheet.Slots.BottomView as NextUpNavView;
            Assert.IsNotNull(nextUp, "precondition: the Up Next bar is drawn");
            Assert.AreEqual(layout, Sheet.Layout.Effective, "precondition: the card is drawn in " + layout);
            const int section = 2;
            yield return TurnTo(section);

            // - the reader scrolls the page back to its top with a real finger: the page's own first block now sits below the spy line
            //   (the title and picture are above it), so no block of the page has reached the line
            var scroll = Sheet.Stack.Scroll;
            var viewport = scroll.contentViewport.worldBound;
            for (int drag = 0; drag < 8 && scroll.scrollOffset.y > OnePixel; drag++)
            {
                yield return CardTestInput.TouchDrag(scroll.panel, viewport.center, new Vector2(0f, viewport.height * 0.8f));
                yield return CardTestInput.Settle(0.4f);
            }
            Assert.AreEqual(0f, scroll.scrollOffset.y, OnePixel, "precondition: the page rests at its top");
            string first = Model.Sections[section].Targets[0];
            float line = viewport.yMin + viewport.height * NavScrollRule.SpyLineFraction;
            Assert.Greater(Sheet.Stack.SlotOfBlock(first).worldBound.yMin, line, "precondition: the page's first block is below the spy line");
            Assert.AreEqual(section, Sheet.Layout.ShownSection, "precondition: the page shown is section " + section);

            var shown = Model.Sections[section].Targets;
            Assert.IsNotNull(Nav.CurrentBlock, "the reader is at a block");
            CollectionAssert.Contains(shown, Nav.CurrentBlock,
                "the block being read belongs to the page shown (" + Model.Sections[section].Key + "), not the card's first (" + Model.Targets[0] + ")");
            int at = Model.Targets.ToList().IndexOf(Nav.CurrentBlock);
            Assert.AreEqual(Model.Targets[at + 1], nextUp.NextBlock, "Up next names the block after it");

            // - a live edit of the settings re-binds the card on the same point (the Editor's rows do exactly this): the answer is kept
            var before = Model;
            yield return Push(n => { });
            Assert.AreNotSame(before, Model, "precondition: the card was bound again with a new model");
            Assert.AreEqual(section, Sheet.Layout.ShownSection, "the page is kept over the rebind");
            CollectionAssert.Contains(shown, Nav.CurrentBlock, "and so is the page's own block");
        }

        [UnityTest]
        public IEnumerator NextUp_UnderPages_LeavesTheCountToThePager_AndUnderScrollKeepsItsOwn()
        {
            yield return Push(n =>
            {
                n.layout = CardOptions.LayoutPages;
                n.bottom = BuiltInNavElements.NextUp;
            });
            yield return OpenTheLamp();
            var nextUp = (NextUpNavView)Sheet.Slots.BottomView;
            var pager = (FamilyPagerNavView)Sheet.Slots.TopView;
            yield return TurnTo(1);
            Assert.IsTrue(CardTestInput.IsShown(pager.Position, Sheet.Root), "the pager says where the visitor is: " + pager.Position.text);
            Assert.IsFalse(CardTestInput.IsShown(nextUp.Position, Sheet.Root), "and Up next adds no second counter (" + nextUp.Position.text + ")");
            Assert.IsTrue(CardTestInput.IsShown(nextUp.Heading, Sheet.Root), "it still names the next block");
            Assert.IsTrue(CardTestInput.IsShown(nextUp.Next, Sheet.Root), "and offers Next");
            yield return CardGalleryChecks.Render("NavCombinations_next_up_pages_390");

            // - the same bar over a scrolling card keeps its place ("5 of 57"): there is no other counter on that screen
            yield return Push(n => { n.layout = CardOptions.LayoutScroll; n.top = BuiltInNavElements.TextTabs; });
            nextUp = (NextUpNavView)Sheet.Slots.BottomView;
            Assert.IsTrue(CardTestInput.IsShown(nextUp.Position, Sheet.Root), "under Scroll the place stays: " + nextUp.Position.text);
            StringAssert.EndsWith(Model.TargetCount.ToString(), nextUp.Position.text, "of all the card's blocks");
        }

        // ---------------- The edge scrubber under Pages and Accordion (audit F2-1, F2-2: dots on the page's peek / every row's chevron) ----------------

        private static bool IsPartOfTheEdgeScrubber(VisualElement element)
        {
            for (var e = element; e != null; e = e.parent)
                if (e.ClassListContains("nav-edge") || e.ClassListContains("nav-edge__item")) return true;
            return false;
        }

        [UnityTest]
        public IEnumerator TheEdgeScrubber_IsNotOfferedWhereTheLayoutOwnsTheRightEdge_AndAFingerThereFindsTheLayoutsOwnControls(
            [Values(CardOptions.LayoutScroll, CardOptions.LayoutTabs, CardOptions.LayoutPages, CardOptions.LayoutAccordion)] string layout)
        {
            yield return Push(n =>
            {
                n.layout = layout;
                n.edge = BuiltInNavElements.EdgeScrubber;
                n.block_order = CardOptions.BlockOrderByFamily;
            });
            yield return OpenTheLamp();
            var scroll = Sheet.Stack.Scroll;
            var viewport = scroll.contentViewport.worldBound;
            Assert.AreEqual(layout, Sheet.Layout.Effective, "precondition: the card is drawn in " + layout);
            bool offered = layout == CardOptions.LayoutScroll || layout == CardOptions.LayoutTabs;
            Assert.AreEqual(offered ? BuiltInNavElements.EdgeScrubber : null, Sheet.Slots.EdgeKey,
                layout + ": the Edge slot holds the scrubber only where the layout leaves the right edge to the wall");

            // - the column the dots would occupy: one touch target wide, ending at the viewport's right edge
            float touch = CardTestInput.TokenPx("--ts-touch-target");
            float x = viewport.xMax - touch / 2f;
            if (offered)
            {
                var view = (EdgeScrubberNavView)Sheet.Slots.EdgeView;
                Assert.IsTrue(view.IsShown, layout + ": the rail is drawn at Full");
                Assert.GreaterOrEqual(view.Items.Count, 2, layout + ": a dot per section");
                var dot = view.Items[0].Button;
                Assert.IsTrue(IsPartOfTheEdgeScrubber(Sheet.Root.panel.Pick(dot.worldBound.center)), layout + ": a finger on the first dot finds the scrubber");
                yield return CardGalleryChecks.Render("NavCombinations_edge_" + layout + "_390");
                yield break;
            }
            yield return CardGalleryChecks.Render("NavCombinations_edge_" + layout + "_390");
            Assert.IsNull(Sheet.Slots.EdgeView, layout + ": no scrubber view is bound");
            Assert.AreEqual(DisplayStyle.None, Sheet.Slots.Edge.resolvedStyle.display, layout + ": and its slot is empty");
            Assert.AreEqual(0, Sheet.Root.Query(className: "nav-edge__item").ToList().Count, layout + ": no dot is in the card");
            int sampled = 0;
            for (float y = viewport.yMin + 4f; y < viewport.yMax - 4f; y += 16f, sampled++)
            {
                var picked = Sheet.Root.panel.Pick(new Vector2(x, y));
                Assert.IsFalse(IsPartOfTheEdgeScrubber(picked), layout + ": a finger at (" + x + ", " + y + ") finds " + picked?.name + ", never a dot");
            }
            Assert.Greater(sampled, 10, "precondition: the whole height of the viewport was sampled (" + sampled + " points)");
            if (layout == CardOptions.LayoutPages)
            {
                var peek = Sheet.Pager.Peek.worldBound;
                Assert.Greater(peek.xMax, x, "precondition: the next page's cue reaches the column the dots would take");
                Assert.IsFalse(IsPartOfTheEdgeScrubber(Sheet.Root.panel.Pick(peek.center)), "pages: the cue at the right edge is what a finger there finds");
            }
        }

        // ---------------- Segmented + Show Counts (audit F6) ----------------

        // Every word of a label whole: each one's width on one line fits the label's own width (a label that had to cut a word shrank below it)
        private void AssertEveryWordWhole(Label label, string name)
        {
            Assert.IsTrue(CardTestInput.IsShown(label, Sheet.Root), name + ": precondition: the words are drawn");
            Assert.Greater(label.layout.width, 0f, name + ": precondition: the words have a width");
            foreach (string word in label.text.Split(' ', System.StringSplitOptions.RemoveEmptyEntries))
            {
                float natural = label.MeasureTextSize(word, 0f, VisualElement.MeasureMode.Undefined, 0f, VisualElement.MeasureMode.Undefined).x;
                Assert.LessOrEqual(natural, label.layout.width + OnePixel, name + ": the word '" + word + "' is whole (" + natural + " of " + label.layout.width + " px)");
            }
        }

        [UnityTest]
        public IEnumerator SegmentedWithShowCounts_KeepsEveryWordWhole_TheCountsGoInstead([Values("en", "pt")] string language)
        {
            yield return PushConfig(c =>
            {
                c.card_settings.preview_language = language;
                c.card_settings.navigation.top = BuiltInNavElements.Segmented;
                c.card_settings.navigation.block_order = CardOptions.BlockOrderByFamily;
                c.card_settings.navigation.show_counts = true;
            });
            yield return OpenTheLamp();
            Assert.AreEqual(BuiltInNavElements.Segmented, Sheet.Slots.TopKey, "precondition: the slot holds the segmented element");
            // - the hero is the screen at rest: the chrome comes once it is scrolled away (_3.3 S4a)
            yield return CardTestInput.PastTheHero(Sheet);
            Assert.IsTrue(Sheet.Chrome.TopShown, "precondition: the strip shows at Full");
            var view = (SegmentedNavView)Sheet.Slots.TopView;
            Assert.AreEqual(4, view.Segments.Count, "precondition: the framework's four groups");

            foreach (var segment in view.Segments)
                AssertEveryWordWhole(segment.Label, language + " " + segment.Label.text);
            // - counts all or none: a strip with a count on some words and not on others reads as a mistake
            int drawn = view.Segments.Count(s => CardTestInput.IsShown(s.Count, Sheet.Root));
            Assert.IsTrue(drawn == 0 || drawn == view.Segments.Count, "the counts show on every segment or on none (" + drawn + " of " + view.Segments.Count + ")");
            Assert.AreEqual(view.CountsShown, drawn == view.Segments.Count, "and the view says which");
            if (view.CountsShown)
                foreach (var segment in view.Segments)
                    Assert.LessOrEqual(segment.Label.worldBound.xMax, segment.Count.worldBound.xMin + OnePixel, segment.Label.text + ": the count sits beside its word, never over it");
            yield return CardGalleryChecks.Render("NavCombinations_segmented_counts_" + language + "_390");
        }

        [UnityTest]
        public IEnumerator SegmentedWithShowCounts_ShortWords_KeepTheirCountsBesideThem()
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
            var inFirst = families.Where((f, i) => i % 2 == 0).ToList();
            yield return PushConfig(c =>
            {
                var nav = c.card_settings.navigation;
                nav.groups = new List<CardNavGroup>
                {
                    new() { key = "group_a", label = new List<LocalizedEntry> { new() { lang = "en", value = "Art" } } },
                    new() { key = "group_b", label = new List<LocalizedEntry> { new() { lang = "en", value = "Fun" } } },
                };
                nav.families = families.Select(f => new CardFamilySetting { family = f, group = inFirst.Contains(f) ? "group_a" : "group_b" }).ToList();
            });
            var view = (SegmentedNavView)Sheet.Slots.TopView;
            Assert.AreEqual(2, view.Segments.Count, "precondition: the wall's two groups");
            Assert.IsTrue(view.CountsShown, "two short words in halves of the strip hold their counts");
            foreach (var segment in view.Segments)
            {
                Assert.IsTrue(CardTestInput.IsShown(segment.Count, Sheet.Root), segment.Label.text + ": the count is drawn");
                Assert.AreEqual(Model.Groups[segment.Index].Count.ToString(), segment.Count.text, segment.Label.text + ": it is the group's block count");
                AssertEveryWordWhole(segment.Label, segment.Label.text);
                Assert.LessOrEqual(segment.Label.worldBound.xMax, segment.Count.worldBound.xMin + OnePixel, segment.Label.text + ": beside its word");
            }
            yield return CardGalleryChecks.Render("NavCombinations_segmented_counts_short_390");
        }
    }
}
