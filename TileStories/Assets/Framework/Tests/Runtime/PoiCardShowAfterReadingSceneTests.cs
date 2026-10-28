using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // Show After Reading across the layouts (_3.2 step 14, ContentSeenRule.HasReadCard: ONE rule the three layouts share) on the REAL
    // LivingRoomScene with real wheel scrolls and real taps, a fresh in-memory card state (nothing read before). The Lamp's true / false
    // question (block_44, Show After Reading) waits for the card to be read:
    //   - scroll: the end of the one stack is the end of the card (unchanged): half way it waits, at the end it shows
    //   - tabs and pages: one section at a time -- the end of a SHORT section (Listen: one block, it fits) is not the card; only once every
    //     section was read to its end does the question show
    public class PoiCardShowAfterReadingSceneTests : SearchSceneFixture
    {
        private const string Lamp = "lamp";
        private const string Question = "block_44";

        private PoiCardSheetView Sheet => Card.Sheet;
        private BlockStackView Stack => Sheet.Stack;
        private NavController Nav => Sheet.Nav;
        private ScrollView Scroll => Stack.Scroll;
        private float OnePixel => Sheet.Layer.panel.visualTree.layout.height / Screen.height;
        private float Range => NavScrollRule.Range(Scroll.contentContainer.layout.height, Scroll.contentViewport.layout.height);
        private bool QuestionShows => Stack.IsRevealed(Question);

        [UnitySetUp]
        public IEnumerator NothingWasReadBefore()
        {
            Card.State = new CardLocalState(new MemoryCardStateStore(), Session.SearchConfig.wall_id);
            yield break;
        }

        // ---------------- helpers ----------------

        private IEnumerator UseLayout(string layout)
        {
            var copy = ConfigCopy();
            copy.card_settings.navigation.layout = layout;
            Session.ApplyCardSettings(copy);
            yield return CardTestInput.Settle(0.3f);
        }

        private IEnumerator OpenFull()
        {
            SelectionEventBus.Clear();
            yield return CardTestInput.Settle();
            SelectionEventBus.Select(Lamp);
            yield return CardTestInput.Settle();
            Sheet.SetStop(SheetStopRule.Stop.Full);
            float rest = 0f;
            for (int frame = 0; frame < 600; frame++)
            {
                rest = Sheet.Stops.HeightOf(Sheet.Stop);
                if (Sheet.Stop == SheetStopRule.Stop.Full && Mathf.Abs(Sheet.Root.layout.height - rest) <= OnePixel) break;
                yield return null;
            }
            Assert.AreEqual(rest, Sheet.Root.layout.height, OnePixel, "precondition: the sheet rests at its full height");
            Assert.IsFalse(QuestionShows, "precondition: the question waits (nothing read on this device)");
            Assert.IsTrue(Stack.TryGetEntryOfBlock(Question, out _), "precondition: The Lamp holds the waiting question");
        }

        // No jump gliding, no page sliding
        private IEnumerator Rested()
        {
            for (int frame = 0; frame < 600 && (Nav.IsJumping || Sheet.Root.ClassListContains(CardPagerView.SettlingClass)); frame++) yield return null;
            yield return CardTestInput.Settle(0.3f);
        }

        // Real wheel notches down to the end of what the stack shows now
        private IEnumerator ReadToTheEnd(string what)
        {
            for (int notch = 0; notch < 400 && Scroll.scrollOffset.y < Range - ContentSeenRule.EndTolerance; notch++)
                yield return CardTestInput.Wheel(Scroll, 12f);
            yield return CardTestInput.Settle(0.3f);
            Assert.GreaterOrEqual(Scroll.scrollOffset.y, Range - ContentSeenRule.EndTolerance - OnePixel, "precondition: " + what + " read to its end");
        }

        // A real tap on the Text Tabs word of section `index` (dragged into the strip's view first, sideways like a thumb)
        private IEnumerator TapTab(int index)
        {
            var tabs = Sheet.Slots.TopView as TextTabsNavView;
            Assert.IsNotNull(tabs, "precondition: the top slot holds Text Tabs");
            // - _3.3 S4a: the strip is chrome and waits for the hero -- read just past it before a finger goes to it
            if (!Sheet.Chrome.TopShown) yield return CardTestInput.PastTheHero(Sheet);
            Assert.IsTrue(Sheet.Chrome.TopShown, "precondition: past the hero the strip shows");
            var tab = tabs.Tabs.Single(t => t.Index == index);
            var visible = tabs.Viewport.worldBound;
            var rect = tab.Button.worldBound;
            if (rect.xMin < visible.xMin - 0.5f || rect.xMax > visible.xMax + 0.5f)
            {
                yield return CardTestInput.TouchDrag(Sheet.Root.panel, visible.center, new Vector2(visible.center.x - rect.center.x, 0f));
                yield return null;
            }
            yield return CardTestInput.Tap(tab.Button.panel, tab.Button.worldBound.center);
            yield return Rested();
            Assert.AreEqual(index, Sheet.Layout.ShownSection, "precondition: a real tap showed section " + index);
        }

        // A real tap on the Family Pager's chevron towards section `index`, as many times as it takes
        private IEnumerator TurnTo(int index)
        {
            var pager = Sheet.Slots.TopView as FamilyPagerNavView;
            Assert.IsNotNull(pager, "precondition: the top slot holds the Family Pager");
            for (int guard = 0; guard < 16 && Sheet.Layout.ShownSection != index; guard++)
            {
                var chevron = index > Sheet.Layout.ShownSection ? pager.Next : pager.Previous;
                yield return CardTestInput.Tap(chevron.panel, chevron.worldBound.center);
                yield return Rested();
            }
            Assert.AreEqual(index, Sheet.Layout.ShownSection, "precondition: the chevrons turned to page " + index);
        }

        // In a layout of one section at a time: the SHORT section first (it fits: its end is reached at once), then every other one read to its
        // end, the question waiting until the last
        private IEnumerator ReadSectionBySection(Func<int, IEnumerator> show)
        {
            var sections = Nav.Model.Sections;
            int listen = sections.ToList().FindIndex(s => s.Key == BuiltInFamilies.Guides);
            Assert.GreaterOrEqual(listen, 0, "precondition: The Lamp has its one-block Listen section");
            yield return show(listen);
            yield return ReadToTheEnd("the Listen section");
            Assert.IsFalse(QuestionShows, "the end of ONE short section is not the end of the card: the question still waits");
            Assert.IsFalse(Stack.ContentSeen, "the card is not read");

            var order = Enumerable.Range(0, sections.Count).Where(i => i != listen).ToList();
            for (int n = 0; n < order.Count; n++)
            {
                yield return show(order[n]);
                Assert.IsFalse(QuestionShows, "section " + sections[order[n]].Key + " shown, " + (order.Count - n) + " of the card still unread: the question waits");
                yield return ReadToTheEnd("section " + sections[order[n]].Key);
            }
            yield return null;
            Assert.IsTrue(QuestionShows, "every section read to its end: the card is read and the question shows");
            Assert.IsTrue(Stack.ContentSeen);
            Assert.IsTrue(Nav.Model.Sections.SelectMany(s => s.Targets).Contains(Question), "and it is a target of the rebuilt map");
        }

        // ---------------- the three layouts ----------------

        [UnityTest]
        public IEnumerator Scroll_HalfWayTheQuestionWaits_AndTheEndOfTheOneStackShowsIt()
        {
            yield return UseLayout(CardOptions.LayoutScroll);
            yield return OpenFull();
            Assert.Greater(Range, 1000f, "precondition: The Lamp's one stack is long");
            for (int notch = 0; notch < 30; notch++) yield return CardTestInput.Wheel(Scroll, 12f);
            yield return CardTestInput.Settle(0.3f);
            Assert.Less(Scroll.scrollOffset.y, Range - 100f, "precondition: half way down, not at the end");
            Assert.IsFalse(QuestionShows, "half way: the question waits");
            yield return ReadToTheEnd("the whole stack");
            yield return null;
            Assert.IsTrue(QuestionShows, "the end of the one stack is the end of the card: the question shows");
        }

        [UnityTest]
        public IEnumerator Tabs_OneShortSectionReadIsNotTheCard_EverySectionReadToItsEndIs()
        {
            yield return UseLayout(CardOptions.LayoutTabs);
            yield return OpenFull();
            Assert.AreEqual(CardOptions.LayoutTabs, Sheet.Layout.Effective, "precondition: The Lamp is drawn in tabs");
            yield return ReadSectionBySection(TapTab);
        }

        [UnityTest]
        public IEnumerator Pages_OneShortPageReadIsNotTheCard_EveryPageReadToItsEndIs()
        {
            yield return UseLayout(CardOptions.LayoutPages);
            yield return OpenFull();
            Assert.AreEqual(CardOptions.LayoutPages, Sheet.Layout.Effective, "precondition: The Lamp is drawn in pages");
            yield return ReadSectionBySection(TurnTo);
        }
    }
}
