using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;
using TileStories.Tests;

namespace TileStories.Editor.Tests
{
    // A wall whose busiest card has MORE families than the framework ships (an app can register as many as it likes): a REAL registry with
    // the built-ins plus `families` app families of one kind each, and a REAL point whose card authors one block per family. Shared by the
    // warning rule tests and the Editor window's.
    public static class ManySectionsFixture
    {
        private sealed class PlainView : IBlockView
        {
            public VisualElement Root { get; } = new();
            public void Bind(BlockInstanceData instance, BlockBindContext context) { }
            public void Unbind() { }
        }

        public const string PoiId = "many_sections";

        public static BlockRegistry Registry(int families)
        {
            var registry = new BlockRegistry();
            BuiltInBlocks.Register(registry);
            for (int i = 1; i <= families; i++)
            {
                registry.RegisterFamily(new FamilyDefinition { Key = "fixture_family_" + i, Order = 100 + i, LabelKey = "fixture_family_label_" + i });
                registry.Register(new BlockKindDefinition
                {
                    Key = "fixture_kind_" + i, Family = "fixture_family_" + i, Format = CardOptions.FormatRead, IconKey = CardIcons.Info, DisplayName = "Fixture kind " + i,
                    Variants = new[] { "plain" }, DefaultVariant = "plain", DisplayModes = new[] { CardOptions.DisplayInline },
                }, () => new PlainView());
            }
            return registry;
        }

        // The shipped wall plus one more point whose card has one block in each of `families` app families
        public static WallConfigData Wall(int families)
        {
            var config = JsonUtility.FromJson<WallConfigData>(File.ReadAllText("Assets/Apps/LivingRoom/config.json"));
            // - the Navigation Bar at the framework default (off): these warnings are the elements', never the wall's bar (40-testing)
            config.card_settings.navigation.bar = new CardNavBarSettings();
            var poi = new POIData { id = PoiId, name = "Many Sections" };
            for (int i = 1; i <= families; i++)
                poi.card.blocks.Add(new BlockInstanceData { key = "block_f" + i, kind = "fixture_kind_" + i });
            config.pois.Add(poi);
            return config;
        }
    }

    // The conditions of Detail Card > Navigation's warnings (_3.2 section 7), pure: each at its edge, each absent otherwise, and the
    // section count read from the REAL card map of the wall's busiest card.
    [Ignore(TestSkips.OldNavigationElements)]
    public class NavigationWarningRuleTests
    {
        private static WallConfigData Shipped() =>
            JsonUtility.FromJson<WallConfigData>(File.ReadAllText("Assets/Apps/LivingRoom/config.json"));

        private static CardNavigationSettings Nav(string layout = CardOptions.LayoutScroll, string top = CardOptions.NavNone,
            string bottom = CardOptions.NavNone, string edge = CardOptions.NavNone, int slots = 5) =>
            new() { layout = layout, top = top, bottom = bottom, edge = edge, max_slots = slots };

        [Test]
        public void TheProgressMapWarning_StartsPastItsLimit_OnlyForTheProgressMap_AndAsksTheCountOnlyThen()
        {
            int asked = 0;
            Func<int> sections(int n) => () => { asked++; return n; };
            var map = Nav(top: BuiltInNavElements.ProgressMap);
            Assert.IsFalse(NavigationWarningRule.ProgressMapOutgrown(map, sections(NavigationWarningRule.ProgressMapSectionsThatFit)), "at the limit: every piece still fits");
            Assert.IsTrue(NavigationWarningRule.ProgressMapOutgrown(map, sections(NavigationWarningRule.ProgressMapSectionsThatFit + 1)), "one past it");
            asked = 0;
            foreach (string top in new[] { CardOptions.NavNone, BuiltInNavElements.TextTabs, BuiltInNavElements.IconTabs, BuiltInNavElements.Segmented })
                Assert.IsFalse(NavigationWarningRule.ProgressMapOutgrown(Nav(top: top), sections(40)), top + " is no progress map: no warning however many sections");
            Assert.AreEqual(0, asked, "the card's map is never built for a Top Bar the warning is not about");
            // - Pages and Bento choose the Top Bar themselves (the Family Pager, none): a stored progress_map is not what the card draws
            Assert.IsFalse(NavigationWarningRule.ProgressMapOutgrown(Nav(CardOptions.LayoutPages, BuiltInNavElements.ProgressMap), sections(40)));
            Assert.IsFalse(NavigationWarningRule.ProgressMapOutgrown(Nav(CardOptions.LayoutBento, BuiltInNavElements.ProgressMap), sections(40)));
        }

        [Test]
        public void TheBentoWarning_StartsPastItsLimit_OnlyForBento_AndAsksTheCountOnlyThen()
        {
            int asked = 0;
            Func<int> sections(int n) => () => { asked++; return n; };
            var bento = Nav(CardOptions.LayoutBento);
            Assert.IsFalse(NavigationWarningRule.BentoOutgrown(bento, sections(NavigationWarningRule.BentoSectionsThatFill)), "13 sections still fill the card");
            Assert.IsTrue(NavigationWarningRule.BentoOutgrown(bento, sections(NavigationWarningRule.BentoSectionsThatFill + 1)), "14 do not");
            asked = 0;
            foreach (string layout in new[] { CardOptions.LayoutScroll, CardOptions.LayoutAccordion, CardOptions.LayoutTabs, CardOptions.LayoutPages })
                Assert.IsFalse(NavigationWarningRule.BentoOutgrown(Nav(layout), sections(40)), layout + " has no tiles");
            Assert.AreEqual(0, asked, "the card's map is never built for another layout");
            Assert.Less(NavigationWarningRule.BentoSectionsThatFill, NavigationWarningRule.BentoSectionsThatFillWithTheTitleFolded);
        }

        [Test]
        public void MoreThanTwoBars_NeedsTheTopBarTheBottomBarAndTheEdgeAllOn_AndAnEdgeTheLayoutKeepsOffTheCardIsNoBar()
        {
            string top = BuiltInNavElements.TextTabs, bottom = BuiltInNavElements.BottomBar, edge = BuiltInNavElements.EdgeScrubber, none = CardOptions.NavNone;
            Assert.IsTrue(NavigationWarningRule.MoreThanTwoBars(Nav(top: top, bottom: bottom, edge: edge)));
            foreach (var (t, b, e) in new[] { (none, bottom, edge), (top, none, edge), (top, bottom, none), (top, none, none), (none, bottom, none), (none, none, edge), (none, none, none) })
                Assert.IsFalse(NavigationWarningRule.MoreThanTwoBars(Nav(top: t, bottom: b, edge: e)), "top " + t + ", bottom " + b + ", edge " + e + ": at most two bars");
            Assert.IsTrue(NavigationWarningRule.MoreThanTwoBars(Nav(CardOptions.LayoutTabs, top, bottom, edge)), "Tabs draws the Edge: three bars");
            Assert.IsFalse(NavigationWarningRule.MoreThanTwoBars(Nav(CardOptions.LayoutPages, none, bottom, edge)),
                "Pages draws its Family Pager at the top, but keeps the right edge for itself: the Edge is never drawn, so two bars (not three)");
            Assert.IsFalse(NavigationWarningRule.MoreThanTwoBars(Nav(CardOptions.LayoutAccordion, top, bottom, edge)), "the accordion keeps the edge too");
            Assert.IsFalse(NavigationWarningRule.MoreThanTwoBars(Nav(CardOptions.LayoutBento, top, bottom, edge)),
                "Bento draws no top bar whatever Top Bar says: two");
            Assert.IsFalse(NavigationWarningRule.MoreThanTwoBars(null));
        }

        [Test]
        public void TheEdgeWarning_NeedsAnEdgePicked_UnderALayoutThatOwnsTheRightEdge()
        {
            string edge = BuiltInNavElements.EdgeScrubber, none = CardOptions.NavNone;
            foreach (string layout in new[] { CardOptions.LayoutPages, CardOptions.LayoutAccordion })
            {
                Assert.IsTrue(NavigationWarningRule.EdgeUnderALayoutThatOwnsIt(Nav(layout, edge: edge)), layout + ": the card draws no scrubber over it");
                Assert.IsFalse(NavigationWarningRule.EdgeUnderALayoutThatOwnsIt(Nav(layout, edge: none)), layout + " with no Edge: nothing to say");
            }
            foreach (string layout in new[] { CardOptions.LayoutScroll, CardOptions.LayoutTabs, CardOptions.LayoutBento })
                Assert.IsFalse(NavigationWarningRule.EdgeUnderALayoutThatOwnsIt(Nav(layout, edge: edge)), layout + ": the Edge is drawn there");
            Assert.IsFalse(NavigationWarningRule.EdgeUnderALayoutThatOwnsIt(null));
        }

        [Test]
        public void TheBarSlotsWarning_NeedsABarThatReadsThem_AndARowThatCouldStillBeRaised_AndAsksTheCountOnlyThen()
        {
            int asked = 0;
            Func<int> sections(int n) => () => { asked++; return n; };
            Assert.IsTrue(NavigationWarningRule.BarSlotsBelowTheSections(Nav(slots: 3), true, sections(8)), "3 slots, 8 sections: More hides five");
            Assert.IsTrue(NavigationWarningRule.BarSlotsBelowTheSections(Nav(slots: 4), true, sections(5)), "4 slots, 5 sections: one more slot would show them all");
            Assert.IsFalse(NavigationWarningRule.BarSlotsBelowTheSections(Nav(slots: 4), true, sections(4)), "4 slots, 4 sections: nothing hidden");
            Assert.IsFalse(NavigationWarningRule.BarSlotsBelowTheSections(Nav(slots: 3), true, sections(3)), "as many slots as sections");
            Assert.IsFalse(NavigationWarningRule.BarSlotsBelowTheSections(Nav(slots: 5), true, sections(8)), "at the row's maximum nothing is left to raise: no warning");
            Assert.IsTrue(NavigationWarningRule.BarSlotsBelowTheSections(Nav(slots: 0), true, sections(8)), "a hand-edited 0 reads as the minimum, 3");
            Assert.IsFalse(NavigationWarningRule.BarSlotsBelowTheSections(Nav(slots: 9), true, sections(8)), "a hand-edited 9 reads as the maximum, 5");
            asked = 0;
            Assert.IsFalse(NavigationWarningRule.BarSlotsBelowTheSections(Nav(slots: 3), false, sections(40)), "no bar reads the slots: no row, no warning");
            Assert.IsFalse(NavigationWarningRule.BarSlotsBelowTheSections(Nav(slots: 5), true, sections(40)));
            Assert.AreEqual(0, asked, "the map is not built when the choice already says no");
            Assert.IsFalse(NavigationWarningRule.BarSlotsBelowTheSections(null, true, sections(40)), "no settings read as the defaults: 5 slots, the maximum");
        }

        [Test]
        public void TheBusiestCard_OfTheShippedWall_IsTheLampsMap_WhateverTheGroupBy()
        {
            var config = Shipped();
            var busiest = NavigationWarningRule.BusiestOf(config.card_settings, config.pois, BlockRegistry.Shared);
            Assert.AreEqual(8, busiest.Sections, "The Lamp's 56 targets sit in 8 families (the map's own count)");
            Assert.AreEqual("lamp", config.pois[busiest.PoiIndex].id, "and The Lamp is the busiest card");
            config.card_settings.navigation.group_by = CardOptions.GroupByFormat;
            var byFormat = NavigationWarningRule.BusiestOf(config.card_settings, config.pois, BlockRegistry.Shared);
            Assert.Less(byFormat.Sections, busiest.Sections, "formats are fewer than families on The Lamp");
            Assert.Greater(byFormat.Sections, 0);
        }

        [Test]
        public void TheBusiestCard_CountsOnlyCardsThatShowNavigation_AndFindsAWallWithMoreFamiliesThanTheFrameworkShips()
        {
            var config = Shipped();
            config.card_settings.navigation.min_blocks = CardNavigationSettings.MinBlocksMax;
            Assert.AreEqual(8, NavigationWarningRule.BusiestOf(config.card_settings, config.pois, BlockRegistry.Shared).Sections, "The Lamp's 56 targets still pass 30");
            foreach (var poi in config.pois.Where(p => p.id == "lamp").ToList()) config.pois.Remove(poi);
            Assert.AreEqual(0, NavigationWarningRule.BusiestOf(config.card_settings, config.pois, BlockRegistry.Shared).Sections,
                "no card of the rest of the wall reaches 30 targets: none shows navigation, so there is no busiest card");
            Assert.AreEqual(-1, NavigationWarningRule.BusiestOf(config.card_settings, config.pois, BlockRegistry.Shared).PoiIndex);
            Assert.AreEqual(0, NavigationWarningRule.BusiestOf(config.card_settings, null, BlockRegistry.Shared).Sections, "no points at all: never a throw");

            var big = ManySectionsFixture.Wall(14);
            var registry = ManySectionsFixture.Registry(14);
            var found = NavigationWarningRule.BusiestOf(big.card_settings, big.pois, registry);
            Assert.AreEqual(14, found.Sections, "one block in each of 14 app families = 14 sections");
            Assert.AreEqual(ManySectionsFixture.PoiId, big.pois[found.PoiIndex].id);
            Assert.IsTrue(NavigationWarningRule.BentoOutgrown(Nav(CardOptions.LayoutBento), () => found.Sections), "and 14 is past what bento fills");
        }

        [Test]
        public void CardShowsNavigation_IsTheCardsOwnAnswer_ForALongCardAndForOneWithNoBlocks()
        {
            var config = Shipped();
            int lamp = config.pois.FindIndex(p => p.id == "lamp");
            int blockless = config.pois.FindIndex(p => p.card.blocks.Count == 0);
            Assert.GreaterOrEqual(blockless, 0, "precondition: the wall has points with no blocks (25 of 26)");
            Assert.IsTrue(NavigationWarningRule.CardShowsNavigation(config.card_settings, config.pois, lamp, BlockRegistry.Shared));
            Assert.IsFalse(NavigationWarningRule.CardShowsNavigation(config.card_settings, config.pois, blockless, BlockRegistry.Shared));
            Assert.IsFalse(NavigationWarningRule.CardShowsNavigation(config.card_settings, config.pois, -1, BlockRegistry.Shared));
            Assert.IsFalse(NavigationWarningRule.CardShowsNavigation(config.card_settings, config.pois, config.pois.Count, BlockRegistry.Shared));
        }
    }
}
