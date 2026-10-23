using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // Phase B of _3.2 step 17b on the REAL LivingRoomScene with real touches: the demo point. The Painting (the Ribeira Palace on the
    // panel) was given its card with the Card Starter's Rich point set plus hand editing, and the wall writes its OWN navigation (a
    // preset applied in Detail Card > Navigation) -- so this fixture, alone among the card fixtures, undoes SearchSceneFixture's pin to the
    // framework default and measures what the wall ships (ShippedNavigation):
    //   - every authored block of The Painting shows, in the order the card's own builder gives, its title in English and Portuguese
    //   - the wall's top element and menu are the ones on the card, and the strip shows at Full
    //   - a REAL tap on every tab lands its section's first block at the viewport top (+-1 px), ONE nav_jump each, the tab marked
    //   - the mark follows real drags of the stack
    //   - Lamp - Residential draws all four in-flow nav kinds at once (section 8.1's demo point)
    //   - renders at 390 px: The Painting in EN and PT at Half and Full, and each nav kind of Lamp - Residential
    public class PoiCardPaintingSceneTests : SearchSceneFixture
    {
        private const string Painting = "painting";
        private const string Residential = "lamp_residential";

        private PoiCardSheetView Sheet => Card.Sheet;
        private NavController Nav => Sheet.Nav;
        private CardNavModel Model => Nav.Model;
        private ScrollView Scroll => Sheet.Stack.Scroll;
        private float OnePixel => Sheet.Layer.panel.visualTree.layout.height / Screen.height;
        private float Range => NavScrollRule.Range(Scroll.contentContainer.layout.height, Scroll.contentViewport.layout.height);

        private sealed class RecordingCardEvents : ICardEvents
        {
            public readonly List<CardEvent> Raised = new();
            public void Raise(CardEvent cardEvent) => Raised.Add(cardEvent);
        }

        private RecordingCardEvents _events;
        private List<string> _shippedLanguages;
        private List<CardEvent> NavJumps => _events.Raised.Where(e => e.Kind == CardEventKinds.NavJump).ToList();

        [UnitySetUp]
        public IEnumerator UseAnInMemoryCardStateRecordTheEventsAndTheWallsOwnNavigation()
        {
            Card.State = new CardLocalState(new MemoryCardStateStore(), Session.SearchConfig.wall_id);
            _events = new RecordingCardEvents();
            Card.Events = _events;
            Assert.IsNotNull(ShippedNavigation, "precondition: the wall writes its own navigation (Detail Card > Navigation, a preset applied)");
            yield return Push(null);
        }

        // ---------------- helpers ----------------

        // The wall as it ships -- its own navigation back over the fixture's pin -- in `language` first, pushed through the live seam
        private IEnumerator Push(Action<WallConfigData> edit, string language = "en")
        {
            var copy = ConfigCopy();
            _shippedLanguages ??= new List<string>(copy.card_settings.languages);
            copy.card_settings.navigation = JsonUtility.FromJson<CardNavigationSettings>(JsonUtility.ToJson(ShippedNavigation));
            copy.card_settings.languages = language == "en" ? new List<string>(_shippedLanguages) : new List<string> { language, "en" };
            edit?.Invoke(copy);
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
            float rest = Sheet.Stops.HeightOf(Sheet.Stop);
            for (int frame = 0; frame < 600 && Mathf.Abs(Sheet.Root.layout.height - rest) > OnePixel; frame++) yield return null;
            Assert.AreEqual(rest, Sheet.Root.layout.height, OnePixel, "precondition: the sheet rests at its " + stop + " height");
            yield return CardTestInput.Settle(0.3f);
        }

        // The tabs of the wall's top element (a tab bar, or the tab row of tabs-with-chips)
        private TabsNavView Tabs()
        {
            var view = Sheet.Slots.TopView;
            if (view is IconTabsChipsNavView both) return both.Tabs;
            Assert.IsInstanceOf<TabsNavView>(view, "precondition: the wall's top element is a row of tabs");
            return (TabsNavView)view;
        }

        private IEnumerator Landed()
        {
            for (int frame = 0; frame < 600 && Nav.IsJumping; frame++) yield return null;
            Assert.IsFalse(Nav.IsJumping, "the jump came to rest");
            yield return CardTestInput.Settle(0.3f);
        }

        // The section the reader is in, read from the SCREEN: the spy's own rule over each section's first slot as drawn
        private int SectionUnderTheSpyLine() =>
            CardTestInput.UnderTheSpyLine(Scroll, Model.Sections.Select(s => Sheet.Stack.SlotOfBlock(s.Targets[0])));

        // A tab wholly inside its strip's visible part, after a real sideways drag of the strip when it is not
        private IEnumerator BringIntoView(TabsNavView tabs, Button tab)
        {
            var visible = tabs.Strip.Viewport.worldBound;
            var rect = tab.worldBound;
            if (rect.xMin < visible.xMin - 0.5f || rect.xMax > visible.xMax + 0.5f)
            {
                yield return CardTestInput.TouchDrag(Sheet.Root.panel, visible.center, new Vector2(visible.center.x - rect.center.x, 0f));
                yield return null;
            }
            rect = tab.worldBound;
            Assert.IsTrue(rect.xMin >= visible.xMin - 0.5f && rect.xMax <= visible.xMax + 0.5f, "precondition: the tab is inside its strip " + rect + " in " + visible);
        }

        // ---------------- The Painting's card ----------------

        [UnityTest]
        public IEnumerator ThePaintingsCard_ShowsEveryAuthoredBlock_InTheBuildersOrder_InEnglishAndPortuguese()
        {
            var config = ConfigCopy();
            var painting = config.pois.Single(p => p.id == Painting);
            var built = BlockStackBuilder.Build(painting, config.card_settings, BlockRegistry.Shared, config.pois);
            Assert.IsEmpty(built.Skipped.Select(s => s.Instance.kind + " " + s.Reason + " " + s.FieldKey),
                "every block of The Painting has what it needs: nothing the starter left empty is left out");
            Assert.AreEqual(painting.card.blocks.Count, built.Entries.Count(e => !e.Synthesized), "every authored block is on the card");
            Assert.IsFalse(built.Entries[0].Synthesized, "the authored header, not the point's name and summary");
            // - the expected titles come from the wall's own header block (40-testing 4.2.4b), and the two languages really differ
            string Title(string language) => new BlockFieldReader(built.Entries[0].Instance, language, "en").Text(BlockStackBuilder.HeaderTitleField);
            Assert.AreNotEqual(Title("en"), Title("pt"), "precondition: the header has its own Portuguese title");

            foreach (string language in new[] { "en", "pt" })
            {
                yield return Push(null, language);
                yield return Open(Painting, SheetStopRule.Stop.Full);
                var header = Sheet.Stack.BoundViews.OfType<HeaderBlockView>().Single();
                Assert.AreEqual(Title(language), header.TitleText, language + ": the card's title");
                // - the stack's own order: each block's slot below the one before, in the builder's order (Sources included, which is
                //   no navigation target: its family is left out of navigation)
                var slots = built.Entries.Skip(1).Select(e => Sheet.Stack.SlotOfBlock(e.Instance.key)).ToList();
                Assert.IsTrue(slots.All(s => s != null), language + ": every block under the header has its slot in the stack");
                for (int i = 1; i < slots.Count; i++)
                    Assert.Greater(slots[i].worldBound.yMin, slots[i - 1].worldBound.yMin, language + ": " + built.Entries[i + 1].Instance.kind + " comes after " + built.Entries[i].Instance.kind);
                yield return CardGalleryChecks.Render("Painting_" + language + "_full_390");
                Sheet.SetStop(SheetStopRule.Stop.Half);
                yield return CardTestInput.Settle(0.5f);
                yield return CardGalleryChecks.Render("Painting_" + language + "_half_390");
                SelectionEventBus.Clear();
                yield return CardTestInput.Settle();
            }
            Assert.AreEqual(ConfigCopy().pois.Single(p => p.id == Painting).name, Session.SearchPois.Single(p => p.id == Painting).name, "the card never renames the point");
        }

        [UnityTest]
        public IEnumerator TheWallsOwnNavigation_IsTheOneOnThePaintingsCard()
        {
            yield return Open(Painting, SheetStopRule.Stop.Full);
            Assert.AreEqual(ShippedNavigation.top, Sheet.Slots.TopKey, "the wall's Top Bar");
            Assert.AreEqual(ShippedNavigation.menu, Sheet.Slots.MenuKey ?? CardOptions.NavNone, "the wall's Menu");
            Assert.AreNotEqual(new CardNavigationSettings().top + new CardNavigationSettings().menu, Sheet.Slots.TopKey + (Sheet.Slots.MenuKey ?? CardOptions.NavNone),
                "the wall's own choice, not the framework default (the reason this fixture undoes the pin)");
            Assert.GreaterOrEqual(Model.TargetCount, ShippedNavigation.min_blocks, "precondition: a card long enough for navigation");
            Assert.IsTrue(Sheet.Chrome.TopShown, "the strip shows at Full");
        }

        [UnityTest]
        public IEnumerator ARealTapOnEveryTab_LandsItsSection_AtTheViewportTop_OneJumpEach_AndTheTabIsMarked()
        {
            yield return Open(Painting, SheetStopRule.Stop.Full);
            var tabs = Tabs();
            Assert.AreEqual(Model.Sections.Count, tabs.Tabs.Count, "one tab per section");
            Assert.Greater(Range, 100f, "precondition: the card really scrolls (range " + Range + ")");
            // - last first: a tap that only worked from the top would pass a top-down walk on its first tab
            foreach (var tab in tabs.Tabs.Reverse())
            {
                yield return BringIntoView(tabs, tab.Button);
                int jumps = NavJumps.Count;
                string block = Model.Sections[tab.Index].Targets[0];
                yield return CardTestInput.TouchDrag(Sheet.Root.panel, tab.Button.worldBound.center, Vector2.zero, frames: 1);
                yield return Landed();
                var slot = Sheet.Stack.SlotOfBlock(block);
                if (slot.layout.y <= Range - OnePixel)
                    Assert.AreEqual(0f, slot.worldBound.yMin - Scroll.contentViewport.worldBound.yMin, OnePixel, tab.Label.text + ": its section's top meets the viewport's top");
                else
                    Assert.AreEqual(Range, Scroll.scrollOffset.y, OnePixel, tab.Label.text + ": a late section clamps at the end of the range");
                Assert.AreEqual(jumps + 1, NavJumps.Count, tab.Label.text + ": one nav_jump");
                Assert.AreEqual(block, NavJumps.Last().BlockKey, tab.Label.text + ": to its section's first block");
                // - the mark is the section on SCREEN (section 4.4): a late section's jump clamps at the end, where the spy marks the last
                Assert.AreEqual(SectionUnderTheSpyLine(), tabs.CurrentIndex, tab.Label.text + ": the tab of the section on screen is marked");
                if (slot.layout.y <= Range - OnePixel) Assert.AreEqual(tab.Index, tabs.CurrentIndex, tab.Label.text + ": a section that reaches the top is its own tab");
            }
        }

        [UnityTest]
        public IEnumerator ARealDragOfThePaintingsCard_MovesTheMark_ToTheSectionOnScreen()
        {
            yield return Open(Painting, SheetStopRule.Stop.Full);
            var tabs = Tabs();
            Assert.AreEqual(SectionUnderTheSpyLine(), tabs.CurrentIndex, "at the top");
            var viewport = Scroll.contentViewport.worldBound;
            var seen = new HashSet<int> { tabs.CurrentIndex };
            // - until the end of the card, whatever its length (P1b's real content doubled it); the cap only stops a drag that never gets there
            for (int drag = 0; drag < 40 && Scroll.scrollOffset.y < Range - NavScrollRule.EndTolerance; drag++)
            {
                float before = Scroll.scrollOffset.y;
                yield return CardTestInput.TouchDrag(Scroll.panel, new Vector2(viewport.center.x, viewport.yMax - 40f), new Vector2(0f, -viewport.height * 0.5f));
                yield return CardTestInput.Settle(0.6f);
                Assert.Greater(Scroll.scrollOffset.y, before, "precondition: the real drag scrolled the stack");
                Assert.AreEqual(SectionUnderTheSpyLine(), tabs.CurrentIndex, "after drag " + (drag + 1) + ": the tab of the section on screen");
                Assert.AreEqual(1, tabs.Tabs.Count(t => t.Button.ClassListContains(NavTab.CurrentClass)), "one tab marked");
                seen.Add(tabs.CurrentIndex);
            }
            Assert.GreaterOrEqual(seen.Count, 3, "the mark really moved with the reader, through several sections");
            Assert.GreaterOrEqual(Scroll.scrollOffset.y, Range - NavScrollRule.EndTolerance, "precondition: the drags reached the end of the card");
            Assert.AreEqual(Model.Sections.Count - 1, tabs.CurrentIndex, "at the end of the card: the last section's tab");
        }

        // The tabs-with-chips strip keeps ONE height whether the section read has chips or not (IconTabsChipsNavView's own promise). The
        // Lamp could never test it -- each of its sections holds two kinds or more -- The Painting can: About and Media have chips, Stories
        // one kind. Found at 17b: the emptied row collapsed, the viewport grew, the spy line crossed back over the boundary and the row
        // filled again, inside one layout pass ("Layout update is struggling", reproduced on every drag across that boundary)
        [UnityTest]
        public IEnumerator TheTabsWithChipsStrip_KeepsOneHeight_OverASectionWithChipsAndOneWithout()
        {
            yield return Open(Painting, SheetStopRule.Stop.Full);
            Assume.That(Sheet.Slots.TopView, Is.InstanceOf<IconTabsChipsNavView>(), "the wall's top element is tabs with chips");
            var both = (IconTabsChipsNavView)Sheet.Slots.TopView;
            var tabs = both.Tabs;
            int withChips = Model.Sections.ToList().FindIndex(s => NavOntologyRule.OffersAKindChoice(s.Kinds.Count));
            int without = Model.Sections.ToList().FindIndex(s => !NavOntologyRule.OffersAKindChoice(s.Kinds.Count));
            Assert.GreaterOrEqual(withChips, 0, "precondition: a section with two kinds or more");
            Assert.GreaterOrEqual(without, 0, "precondition: a section with one kind");

            var strip = new Dictionary<int, float>();
            var viewport = new Dictionary<int, float>();
            foreach (int section in new[] { withChips, without, withChips })
            {
                var tab = tabs.Tabs.Single(t => t.Index == section);
                yield return BringIntoView(tabs, tab.Button);
                yield return CardTestInput.TouchDrag(Sheet.Root.panel, tab.Button.worldBound.center, Vector2.zero, frames: 1);
                yield return Landed();
                Assert.AreEqual(section == withChips, both.ChipsSection >= 0, "precondition: the chip row is " + (section == withChips ? "filled" : "empty"));
                strip[section] = both.Root.layout.height;
                viewport[section] = Scroll.contentViewport.layout.height;
            }
            Assert.AreEqual(strip[withChips], strip[without], OnePixel,
                "the strip is as tall over a one-kind section as over one with chips (" + strip[withChips] + " / " + strip[without] + ")");
            Assert.AreEqual(viewport[withChips], viewport[without], OnePixel, "so the reading space under it never jumps");
        }

        // Where the chrome hides the strip (Half on this card), nothing of it paints: the chip row hides WITH its strip. Found at 17b on
        // The Painting's Half render: the row set its own visibility to Visible when it had chips, which beat the strip's inherited hidden,
        // so the chips floated alone over the card's title
        [UnityTest]
        public IEnumerator WhereTheChromeHidesTheStrip_TheChipRowHidesWithIt_AndShowsWithItAtFull()
        {
            yield return Open(Painting, SheetStopRule.Stop.Half);
            Assume.That(Sheet.Slots.TopView, Is.InstanceOf<IconTabsChipsNavView>(), "the wall's top element is tabs with chips");
            var both = (IconTabsChipsNavView)Sheet.Slots.TopView;
            Assert.IsFalse(Sheet.Chrome.TopShown, "precondition: at Half the chrome hides this card's strip (the header takes the room)");
            Assert.GreaterOrEqual(both.ChipsSection, 0, "precondition: the first section has chips bound");
            Assert.AreEqual(Visibility.Hidden, both.Tabs.Root.resolvedStyle.visibility, "the tabs are hidden with the strip");
            Assert.AreEqual(Visibility.Hidden, both.Chips.Root.resolvedStyle.visibility, "and so is the chip row: nothing floats over the title");

            Sheet.SetStop(SheetStopRule.Stop.Full);
            yield return CardTestInput.Settle(0.5f);
            Assert.IsTrue(Sheet.Chrome.TopShown, "precondition: the strip shows at Full");
            Assert.AreEqual(Visibility.Visible, both.Chips.Root.resolvedStyle.visibility, "with the strip shown, the chips show");
        }

        // ---------------- Lamp - Residential: the whole domain on one card ----------------

        [UnityTest]
        public IEnumerator LampResidential_DrawsAllFourInFlowNavKinds_AtOnce()
        {
            yield return Open(Residential, SheetStopRule.Stop.Full);
            var bound = Sheet.Stack.BoundViews.ToList();
            Assert.AreEqual(1, bound.OfType<NavBentoBlockView>().Count(), "the bento (since 15a)");
            Assert.AreEqual(1, bound.OfType<NavIconGridBlockView>().Count(), "an icon grid");
            Assert.AreEqual(1, bound.OfType<NavContentsBlockView>().Count(), "a contents list");
            Assert.AreEqual(1, bound.OfType<NavRailsBlockView>().Count(), "rails");
            var config = ConfigCopy();
            var poi = config.pois.Single(p => p.id == Residential);
            Assert.IsEmpty(BlockStackBuilder.Build(poi, config.card_settings, BlockRegistry.Shared, config.pois).Skipped, "nothing of its card is left out");

            foreach (var view in new IBlockView[] { bound.OfType<NavBentoBlockView>().Single(), bound.OfType<NavIconGridBlockView>().Single(),
                         bound.OfType<NavContentsBlockView>().Single(), bound.OfType<NavRailsBlockView>().Single() })
            {
                Scroll.ScrollTo(view.Root);
                yield return CardTestInput.Settle(0.4f);
                Assert.Greater(view.Root.layout.height, 0f, view.GetType().Name + ": drawn, not empty");
                yield return CardGalleryChecks.Render("Residential_" + view.GetType().Name + "_full_390");
            }
        }
    }
}
