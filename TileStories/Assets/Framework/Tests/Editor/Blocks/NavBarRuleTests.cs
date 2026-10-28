using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // Tier 0 of the Navigation Bar (_3.3 S4 third round, NavBarRule): the six default groups (one per navigable family in the families'
    // order, AR inside Media, Community inside Play), a wall's own rows (hidden, merged, their blanks filled from the first family), the groups
    // ONE card shows -- on the SHIPPED Ribeira Palace through the real BlockStackBuilder and CardNavModel -- the group a block belongs to, the
    // words, v5's defaults (an unknown value reads as its default), a pre-v5 config read unchanged, the disc and the over-the-hero rule.
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
        public void WithNoRows_TheBarHasTheSixGroups_InTheFamiliesOrder_ArInMedia_CommunityInPlay_WithTheFamilysIconAndColour()
        {
            var nav = new CardNavigationSettings();
            var items = NavBarRule.Items(nav, BlockRegistry.Shared);
            // - every navigable family that joins no other, in the families' order (an app's own family is a group of its own)
            var navigable = BlockRegistry.Shared.Families.Where(f => f.InNav).OrderBy(f => f.Order).Select(f => f.Key).ToList();
            var hosts = navigable.Where(f => !NavBarRule.DefaultJoins.ContainsKey(f)).ToList();
            CollectionAssert.AreEqual(hosts, items.Select(i => i.Key).ToList(), "one per family that joins no other, in order");
            var builtIn = new[] { BuiltInFamilies.About, BuiltInFamilies.Media, BuiltInFamilies.Guides, BuiltInFamilies.Stories,
                BuiltInFamilies.Play, BuiltInFamilies.Visit };
            CollectionAssert.AreEqual(builtIn, items.Select(i => i.Key).Where(builtIn.Contains).ToList(), "the framework's six groups, in order");
            CollectionAssert.AreEqual(new[] { BuiltInFamilies.Media, BuiltInFamilies.Ar }, items.Single(i => i.Key == BuiltInFamilies.Media).Families,
                "AR's blocks show the point: in Media, after it");
            CollectionAssert.AreEqual(new[] { BuiltInFamilies.Play, BuiltInFamilies.Community }, items.Single(i => i.Key == BuiltInFamilies.Play).Families,
                "Community's blocks ask the visitor something: in Play");
            CollectionAssert.DoesNotContain(items.Select(i => i.Key).ToList(), BuiltInFamilies.Navigation, "a nav block family is never a group");
            foreach (string family in navigable)
                Assert.AreEqual(1, items.Count(i => i.Families.Contains(family)), family + " sits in exactly one default group");
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
            CollectionAssert.AreEquivalent(items.Where(i => i.Families.Any(families.Contains)).Select(i => i.Key).ToList(),
                bar.Select(e => e.Item.Key).ToList(), "one group per group of families the card has blocks of");
            Assert.GreaterOrEqual(bar.Count, NavBarRule.MinEntries, "precondition: the card shows a bar");
            var order = items.Select(i => i.Key).ToList();
            CollectionAssert.IsOrdered(bar.Select(e => order.IndexOf(e.Item.Key)).ToList(), "the bar's order, never the scroll's");
            foreach (var entry in bar)
            {
                string first = model.Targets.First(t => entry.Item.Families.Contains(FamilyOf(entries, t)));
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
            Assert.IsTrue(NavBarRule.Enabled(nav), "on by default: the visitor's main way round the card");
            Assert.AreEqual(CardOptions.NavNone, nav.top, "with the bar on, the old Top Bar defaults to none");
            Assert.AreEqual(CardOptions.NavNone, nav.bottom);
            Assert.AreEqual(CardOptions.NavNone, nav.menu);
            Assert.AreEqual(CardOptions.NavNone, nav.floating);
            Assert.AreEqual(CardOptions.NavNone, nav.edge);
            Assert.IsTrue(NavBarRule.Shows(nav, true, 2));
            Assert.IsFalse(NavBarRule.Shows(nav, true, 1), "one group is no choice");
            Assert.IsFalse(NavBarRule.Shows(nav, false, 6), "a short card stays clean (Show Navigation From)");
            Assert.IsFalse(NavBarRule.Enabled(null));
            nav.bar.enabled = false;
            Assert.IsFalse(NavBarRule.Shows(nav, true, 6), "off: the old elements instead");
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
        public void TheDefaults_AreV5s_AndEveryOption_ReadsItsDefault_ForAnUnknownValue()
        {
            var fresh = new CardNavBarSettings();
            Assert.AreEqual(CardOptions.NavBarBottom, NavBarRule.Position(fresh));
            Assert.AreEqual(CardOptions.NavBarFloating, NavBarRule.Shape(fresh));
            Assert.AreEqual(CardOptions.NavBarSolid, NavBarRule.Fill(fresh));
            Assert.AreEqual(CardOptions.NavBarIndicatorRaised, NavBarRule.Indicator(fresh));
            Assert.AreEqual(CardOptions.NavBarChosenIconFilled, NavBarRule.ChosenIcon(fresh));
            Assert.AreEqual(CardOptions.NavBarLabelsIconAndText, NavBarRule.Labels(fresh));
            Assert.AreEqual(CardOptions.NavBarColourAccent, NavBarRule.SelectedColour(fresh));
            Assert.IsTrue(NavBarRule.BoldChosen(fresh));
            Assert.AreEqual(CardOptions.NavBarShowAlways, NavBarRule.Show(fresh));
            Assert.IsFalse(NavBarRule.HidesWhileScrolling(fresh));
            // - the raw fields carry the same values (what a new config writes)
            Assert.AreEqual(NavBarRule.Position(fresh), fresh.position);
            Assert.AreEqual(NavBarRule.Fill(fresh), fresh.fill);
            Assert.AreEqual(NavBarRule.Indicator(fresh), fresh.indicator);
            Assert.AreEqual(NavBarRule.ChosenIcon(fresh), fresh.chosen_icon);
            Assert.AreEqual(NavBarRule.SelectedColour(fresh), fresh.selected_color);
            Assert.AreEqual(NavBarRule.Show(fresh), fresh.show);

            var unknown = new CardNavBarSettings
            {
                position = "x", shape = "x", fill = "x", indicator = "x", chosen_icon = "x", labels = "x", selected_color = "x", show = "x",
                hide_on_scroll = "x",
            };
            Assert.AreEqual(NavBarRule.Position(fresh), NavBarRule.Position(unknown));
            Assert.AreEqual(NavBarRule.Shape(fresh), NavBarRule.Shape(unknown));
            Assert.AreEqual(NavBarRule.Fill(fresh), NavBarRule.Fill(unknown));
            Assert.AreEqual(NavBarRule.Indicator(fresh), NavBarRule.Indicator(unknown));
            Assert.AreEqual(NavBarRule.ChosenIcon(fresh), NavBarRule.ChosenIcon(unknown), "Plain is listed first, Filled stays the default");
            Assert.AreEqual(NavBarRule.Labels(fresh), NavBarRule.Labels(unknown));
            Assert.AreEqual(NavBarRule.SelectedColour(fresh), NavBarRule.SelectedColour(unknown));
            Assert.AreEqual(NavBarRule.Show(fresh), NavBarRule.Show(unknown));
            Assert.AreEqual(NavBarRule.HideOnScroll(fresh), NavBarRule.HideOnScroll(unknown));
            Assert.IsTrue(NavBarRule.AtBottom(fresh));
            fresh.position = CardOptions.NavBarTop;
            Assert.IsFalse(NavBarRule.AtBottom(fresh));
            // - the Editor's option lists are v5's, in its order
            CollectionAssert.AreEqual(new[] { "Bottom", "Top" }, CardOptions.NavBarPositionLabels);
            CollectionAssert.AreEqual(new[] { "Floating", "Full Width" }, CardOptions.NavBarShapeLabels);
            CollectionAssert.AreEqual(new[] { "Solid", "Glass", "Clear" }, CardOptions.NavBarFillLabels);
            CollectionAssert.AreEqual(new[] { "Raised", "Pill", "Underline", "None" }, CardOptions.NavBarIndicatorLabels);
            CollectionAssert.AreEqual(new[] { "Plain", "Filled" }, CardOptions.NavBarChosenIconLabels);
            CollectionAssert.AreEqual(new[] { "Icon And Text", "Icon", "Text", "Text On The Chosen" }, CardOptions.NavBarLabelLabels);
            CollectionAssert.AreEqual(new[] { "Accent", "Ink", "Group" }, CardOptions.NavBarSelectedColourLabels);
            CollectionAssert.AreEqual(new[] { "Always", "After The Hero" }, CardOptions.NavBarShowLabels);
        }

        [Test]
        public void APreV5Config_LoadsUnchanged_ItsUnderTitleReadsAsTop_TheDroppedKeysIgnored()
        {
            // - the bar as the shipped wall wrote it before v5 (git history of config.json), with other values and a merged group of its own
            const string json = "{\"enabled\": true, \"position\": \"under_title\", \"show\": \"after_hero\", \"hide_on_scroll\": \"down\", "
                + "\"shape\": \"full_width\", \"corner_radius\": 26.0, \"side_margin\": 12.0, \"height\": 56.0, \"fill\": \"glass\", "
                + "\"fill_opacity\": 0.5, \"border\": true, \"shadow\": true, \"indicator\": \"pill\", \"slide_indicator\": true, "
                + "\"bold_selected\": false, \"enlarge_selected\": true, \"selected_color\": \"group\", \"idle_color\": \"muted\", "
                + "\"labels\": \"selected_only\", \"items\": [{\"key\": \"group_1\", \"shown\": true, \"families\": [\"media\", \"guides\"]}]}";
            var bar = JsonUtility.FromJson<CardNavBarSettings>(json);
            Assert.IsTrue(bar.enabled);
            Assert.AreEqual(CardOptions.NavBarTop, NavBarRule.Position(bar), "under_title is Top");
            Assert.AreEqual(CardOptions.NavBarUnderTitle, bar.position, "...read as Top, never rewritten by a read");
            Assert.AreEqual(CardOptions.NavBarShowAfterHero, NavBarRule.Show(bar));
            Assert.IsTrue(NavBarRule.HidesWhileScrolling(bar));
            Assert.AreEqual(CardOptions.NavBarFullWidth, NavBarRule.Shape(bar));
            Assert.AreEqual(CardOptions.NavBarGlass, NavBarRule.Fill(bar));
            Assert.AreEqual(CardOptions.NavBarIndicatorPill, NavBarRule.Indicator(bar));
            Assert.IsFalse(NavBarRule.BoldChosen(bar));
            Assert.AreEqual(CardOptions.NavBarColourGroup, NavBarRule.SelectedColour(bar), "Group is kept");
            Assert.AreEqual(CardOptions.NavBarLabelsSelectedOnly, NavBarRule.Labels(bar));
            Assert.AreEqual(CardOptions.NavBarChosenIconFilled, NavBarRule.ChosenIcon(bar), "a key it never wrote: v5's default");
            Assert.AreEqual(1, bar.items.Count);
            CollectionAssert.AreEqual(new[] { "media", "guides" }, bar.items[0].families);
            // - written back, the dropped keys are simply gone and everything it set is kept
            string written = JsonUtility.ToJson(bar);
            StringAssert.DoesNotContain("corner_radius", written);
            StringAssert.DoesNotContain("idle_color", written);
            Assert.AreEqual(written, JsonUtility.ToJson(JsonUtility.FromJson<CardNavBarSettings>(written)));
        }

        [Test]
        public void TheShippedWall_CarriesV5sDefaults_AndTheOldElementsAtNone()
        {
            var nav = Shipped().card_settings.navigation;
            Assert.AreEqual(JsonUtility.ToJson(new CardNavBarSettings()), JsonUtility.ToJson(nav.bar), "the wall's bar = the framework's");
            Assert.AreEqual(CardOptions.NavNone, nav.top);
            Assert.AreEqual(CardOptions.NavNone, nav.bottom);
            Assert.AreEqual(CardOptions.NavNone, nav.menu);
            Assert.AreEqual(CardOptions.NavNone, nav.floating);
            Assert.AreEqual(CardOptions.NavNone, nav.edge);
        }

        [Test]
        public void TheChosenDisc_IsRaisedsOrFilleds_NeverOnABarOfWordsAlone()
        {
            var bar = new CardNavBarSettings();
            Assert.IsTrue(NavBarRule.ChosenDisc(bar), "Raised");
            bar.indicator = CardOptions.NavBarIndicatorPill;
            Assert.IsTrue(NavBarRule.ChosenDisc(bar), "Filled with any indicator");
            bar.chosen_icon = CardOptions.NavBarChosenIconPlain;
            Assert.IsFalse(NavBarRule.ChosenDisc(bar), "Plain, no Raised: no disc");
            bar.chosen_icon = CardOptions.NavBarChosenIconFilled;
            bar.labels = CardOptions.NavBarLabelsText;
            Assert.IsFalse(NavBarRule.ChosenDisc(bar), "words alone: no icon to fill");
            bar.indicator = CardOptions.NavBarIndicatorRaised;
            Assert.IsTrue(NavBarRule.ChosenDisc(bar), "Raised always rides a disc");
        }

        [Test]
        public void OverTheHero_AtTheBottom_ByItsTopEdge_AtTop_ByItsFoot_NeverWithoutAPictureHero()
        {
            // - a hero ending at 600; v5's 24 margin
            Assert.IsTrue(NavBarRule.OverHero(true, 575f, 637f, 600f, 24f), "bottom: the top edge 25 above the hero's foot");
            Assert.IsFalse(NavBarRule.OverHero(true, 577f, 639f, 600f, 24f), "...23 above: no longer on it");
            Assert.IsTrue(NavBarRule.OverHero(false, 513f, 575f, 600f, 24f), "top: the foot 25 above the hero's foot");
            Assert.IsFalse(NavBarRule.OverHero(false, 515f, 577f, 600f, 24f));
            Assert.IsFalse(NavBarRule.OverHero(true, 0f, 62f, float.NaN, 24f), "no picture hero");
            var nav = new CardNavigationSettings();
            Assert.IsTrue(NavBarRule.ClearsHeroFoot(nav), "the default bar floats at the bottom from the start: the words rise over it");
            nav.bar.show = CardOptions.NavBarShowAfterHero;
            Assert.IsFalse(NavBarRule.ClearsHeroFoot(nav));
            nav.bar.show = CardOptions.NavBarShowAlways;
            nav.bar.position = CardOptions.NavBarTop;
            Assert.IsFalse(NavBarRule.ClearsHeroFoot(nav));
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
