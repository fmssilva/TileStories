using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // Tier 0 of the Navigation Bar (_3.3 S4a review, NavBarRule): the default groups (one per navigable family, in the families' order),
    // a wall's own rows (hidden, merged, their blanks filled from the first family), the groups ONE card shows -- on the SHIPPED Ribeira
    // Palace through the real BlockStackBuilder and CardNavModel -- the group a block belongs to, the words, and every option's default.
    public class NavBarRuleTests
    {
        private const string Painting = "painting";

        private static WallConfigData Shipped() =>
            JsonUtility.FromJson<WallConfigData>(File.ReadAllText("Assets/Apps/LivingRoom/config.json"));

        private static List<BlockStackBuilder.Entry> Entries(WallConfigData config, string poiId) =>
            BlockStackBuilder.Build(config.pois.Single(p => p.id == poiId), config.card_settings, BlockRegistry.Shared, config.pois).Entries;

        private static string FamilyOf(List<BlockStackBuilder.Entry> entries, string block) =>
            entries.First(e => e.Instance.key == block).Definition.Family;

        [Test]
        public void WithNoRows_TheBarHasOneGroupPerNavigableFamily_InTheFamiliesOrder_WithTheFamilysIconAndColour()
        {
            var nav = new CardNavigationSettings();
            var items = NavBarRule.Items(nav, BlockRegistry.Shared);
            var navigable = BlockRegistry.Shared.Families.Where(f => f.InNav).OrderBy(f => f.Order).Select(f => f.Key).ToList();
            CollectionAssert.AreEqual(navigable, items.Select(i => i.Key).ToList(), "one per navigable family, in order");
            CollectionAssert.IsSubsetOf(new[] { BuiltInFamilies.About, BuiltInFamilies.Media, BuiltInFamilies.Guides, BuiltInFamilies.Stories,
                BuiltInFamilies.Play, BuiltInFamilies.Visit }, items.Select(i => i.Key).ToList(), "today's six groups are among them");
            CollectionAssert.DoesNotContain(items.Select(i => i.Key).ToList(), BuiltInFamilies.Navigation, "a nav block family is never a group");
            BlockRegistry.Shared.TryGetFamily(BuiltInFamilies.Guides, out var guides);
            var listen = items.Single(i => i.Key == BuiltInFamilies.Guides);
            Assert.AreEqual(guides.IconKey, listen.Icon);
            Assert.AreEqual(guides.IconKey, listen.SelectedIcon, "no chosen icon of its own: the same icon");
            Assert.AreEqual(guides.ColorToken, listen.ColorToken);
            Assert.AreEqual(guides.LabelKey, listen.LabelKey);
            CollectionAssert.AreEqual(new[] { BuiltInFamilies.Guides }, listen.Families);
        }

        [Test]
        public void TheFamiliesTable_OrdersTheDefaultGroups()
        {
            var nav = new CardNavigationSettings();
            nav.families.Add(new CardFamilySetting { family = BuiltInFamilies.Visit, order = 1 });
            Assert.AreEqual(BuiltInFamilies.Visit, NavBarRule.Items(nav, BlockRegistry.Shared)[0].Key, "a family moved first leads the bar");
        }

        [Test]
        public void AWallsRows_AreTheBar_HiddenOnesLeftOut_TheirBlanksFromTheFirstFamily()
        {
            var nav = new CardNavigationSettings();
            nav.bar.items.Add(new CardNavBarItem { key = "group_1", families = new List<string> { BuiltInFamilies.Media, BuiltInFamilies.Guides },
                icon = CardIcons.Video, icon_selected = CardIcons.Play, color = "--ts-accent" });
            nav.bar.items.Add(new CardNavBarItem { key = BuiltInFamilies.About, families = new List<string> { BuiltInFamilies.About } });
            nav.bar.items.Add(new CardNavBarItem { key = BuiltInFamilies.Visit, shown = false, families = new List<string> { BuiltInFamilies.Visit } });
            var items = NavBarRule.Items(nav, BlockRegistry.Shared);
            CollectionAssert.AreEqual(new[] { "group_1", BuiltInFamilies.About }, items.Select(i => i.Key).ToList(), "the rows' order, the hidden one out");
            Assert.AreEqual(CardIcons.Video, items[0].Icon);
            Assert.AreEqual(CardIcons.Play, items[0].SelectedIcon);
            Assert.AreEqual("--ts-accent", items[0].ColorToken);
            BlockRegistry.Shared.TryGetFamily(BuiltInFamilies.Media, out var media);
            Assert.AreEqual(media.LabelKey, items[0].LabelKey, "no words of its own: its first family's");
            BlockRegistry.Shared.TryGetFamily(BuiltInFamilies.About, out var about);
            Assert.AreEqual(about.IconKey, items[1].Icon, "a blank icon: the family's");
            Assert.AreEqual(about.ColorToken, items[1].ColorToken, "a blank colour: the family's");
        }

        [Test]
        public void TheRibeiraPalace_ShowsEachGroupItHasBlocksOf_InTheBarsOrder_EachLandingOnItsFirstBlock()
        {
            var config = Shipped();
            var entries = Entries(config, Painting);
            var model = CardNavModel.Build(entries, BlockRegistry.Shared, config.card_settings);
            var items = NavBarRule.Items(new CardNavigationSettings(), BlockRegistry.Shared);
            var bar = NavBarRule.Entries(items, model.Targets, block => FamilyOf(entries, block));
            var families = model.Targets.Select(t => FamilyOf(entries, t)).Distinct().ToList();
            CollectionAssert.AreEquivalent(families.Where(f => items.Any(i => i.Key == f)).ToList(), bar.Select(e => e.Item.Key).ToList(),
                "one group per family the card has blocks of");
            Assert.GreaterOrEqual(bar.Count, NavBarRule.MinEntries, "precondition: the card shows a bar");
            var order = items.Select(i => i.Key).ToList();
            CollectionAssert.IsOrdered(bar.Select(e => order.IndexOf(e.Item.Key)).ToList(), "the bar's order, never the scroll's");
            foreach (var entry in bar)
            {
                string first = model.Targets.First(t => FamilyOf(entries, t) == entry.Item.Key);
                Assert.AreEqual(first, entry.FirstTarget, entry.Item.Key + " lands on its first block in the stack");
                foreach (string target in entry.Targets) Assert.AreEqual(bar.IndexOf(entry), NavBarRule.EntryOf(bar, target), target + " belongs to it");
            }
            Assert.AreEqual(-1, NavBarRule.EntryOf(bar, "no_such_block"));
            Assert.AreEqual(-1, NavBarRule.EntryOf(bar, null));
        }

        [Test]
        public void AFamilyInTwoRows_BelongsToTheFirst_AndAGroupWithNoBlockIsLeftOut()
        {
            var items = new List<NavBarRule.Item>
            {
                new("a", null, null, null, null, null, new[] { "about" }),
                new("b", null, null, null, null, null, new[] { "about", "media" }),
                new("c", null, null, null, null, null, new[] { "visit" }),
            };
            var targets = new[] { "x1", "x2", "x3" };
            var family = new Dictionary<string, string> { ["x1"] = "media", ["x2"] = "about", ["x3"] = "media" };
            var bar = NavBarRule.Entries(items, targets, t => family[t]);
            CollectionAssert.AreEqual(new[] { "a", "b" }, bar.Select(e => e.Item.Key).ToList(), "c has no block here");
            CollectionAssert.AreEqual(new[] { "x2" }, bar[0].Targets, "about's block goes to the first row that holds it");
            CollectionAssert.AreEqual(new[] { "x1", "x3" }, bar[1].Targets);
        }

        [Test]
        public void TheBarShows_OnlyWhenOn_OnACardWithNavigation_WithTwoGroups()
        {
            var nav = new CardNavigationSettings();
            Assert.IsFalse(NavBarRule.Enabled(nav), "off in a config that never wrote it");
            Assert.IsFalse(NavBarRule.Shows(nav, true, 6));
            nav.bar.enabled = true;
            Assert.IsTrue(NavBarRule.Shows(nav, true, 2));
            Assert.IsFalse(NavBarRule.Shows(nav, true, 1), "one group is no choice");
            Assert.IsFalse(NavBarRule.Shows(nav, false, 6), "a short card stays clean (Show Navigation From)");
            Assert.IsFalse(NavBarRule.Enabled(null));
        }

        [Test]
        public void TheWords_AreTheRowsOwn_ElseTheFamilysWord_ElseTheKey()
        {
            var own = new NavBarRule.Item("group_1", new List<LocalizedEntry> { new() { lang = "en", value = "Watch" } }, null, null, null, null, new string[0]);
            Assert.AreEqual("Watch", NavBarRule.Words(own, null), "its own words");
            var bare = new NavBarRule.Item("group_2", null, "no_such_string_key", null, null, null, new string[0]);
            Assert.AreEqual("group_2", NavBarRule.Words(bare, null), "no words anywhere: the key");
        }

        [Test]
        public void EveryOption_ReadsItsDefault_ForAnUnknownValue_AndTheNumbersStayInRange()
        {
            var bar = new CardNavBarSettings
            {
                position = "x", show = "x", hide_on_scroll = "x", shape = "x", fill = "x", indicator = "x", selected_color = "x", idle_color = "x",
                labels = "x", corner_radius = 99f, side_margin = -4f, height = 10f, fill_opacity = 0f,
            };
            Assert.AreEqual(CardOptions.NavBarUnderTitle, NavBarRule.Position(bar));
            Assert.AreEqual(CardOptions.NavBarShowAfterHero, NavBarRule.Show(bar));
            Assert.AreEqual(CardOptions.NavBarHideNever, NavBarRule.HideOnScroll(bar));
            Assert.AreEqual(CardOptions.NavBarFloating, NavBarRule.Shape(bar));
            Assert.AreEqual(CardOptions.NavBarGlass, NavBarRule.Fill(bar));
            Assert.AreEqual(CardOptions.NavBarIndicatorPill, NavBarRule.Indicator(bar));
            Assert.AreEqual(CardOptions.NavBarColourGroup, NavBarRule.SelectedColour(bar));
            Assert.AreEqual(CardOptions.NavBarIdleMuted, NavBarRule.IdleColour(bar));
            Assert.AreEqual(CardOptions.NavBarLabelsIconAndText, NavBarRule.Labels(bar));
            Assert.AreEqual(CardNavBarSettings.CornerRadiusMax, NavBarRule.CornerRadius(bar));
            Assert.AreEqual(0f, NavBarRule.SideMargin(bar));
            Assert.AreEqual(CardNavBarSettings.HeightMin, NavBarRule.Height(bar));
            Assert.AreEqual(CardNavBarSettings.FillOpacityMin, NavBarRule.FillOpacity(bar));
            bar.shape = CardOptions.NavBarFullWidth;
            bar.side_margin = 12f;
            Assert.AreEqual(0f, NavBarRule.CornerRadius(bar), "full width: square");
            Assert.AreEqual(0f, NavBarRule.SideMargin(bar), "...and edge to edge");
            Assert.IsFalse(NavBarRule.AtBottom(bar));
            bar.position = CardOptions.NavBarBottom;
            Assert.IsTrue(NavBarRule.AtBottom(bar));
        }

        [Test]
        public void TheLabels_SayWhatEachGroupShows()
        {
            Assert.IsTrue(NavBarRule.ShowsIcon(CardOptions.NavBarLabelsIconAndText, true));
            Assert.IsFalse(NavBarRule.ShowsIcon(CardOptions.NavBarLabelsText, true));
            Assert.IsFalse(NavBarRule.ShowsIcon(CardOptions.NavBarLabelsIcon, false), "no icon to show");
            Assert.IsTrue(NavBarRule.ShowsWords(CardOptions.NavBarLabelsIcon, false, false), "a group with no icon always shows its words");
            Assert.IsFalse(NavBarRule.ShowsWords(CardOptions.NavBarLabelsIcon, true, true));
            Assert.IsTrue(NavBarRule.ShowsWords(CardOptions.NavBarLabelsSelectedOnly, true, true), "the chosen group's words");
            Assert.IsFalse(NavBarRule.ShowsWords(CardOptions.NavBarLabelsSelectedOnly, true, false), "...the others' not");
        }
    }
}
