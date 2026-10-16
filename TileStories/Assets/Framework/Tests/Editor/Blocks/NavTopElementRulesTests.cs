using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // Tier 0 of what the step 6 top elements read (_3.2 sections 2.5, 3 and 5.1), on the SHIPPED LivingRoom wall through the real
    // BlockStackBuilder and the shared registry: the Groups (rule 2.5.6 -- the defaults, a wall's own rows, a family moved to another
    // group, one left in none, the words), the model's icons, colours, kind words and lookups, the progress map's segment widths, Labels,
    // and which elements rule 2.5.7 draws for a card.
    public class NavTopElementRulesTests
    {
        private const string Lamp = "lamp";

        private static WallConfigData Shipped(string blockOrder = CardOptions.BlockOrderByFamily)
        {
            var config = JsonUtility.FromJson<WallConfigData>(File.ReadAllText("Assets/Apps/LivingRoom/config.json"));
            config.card_settings.navigation.block_order = blockOrder;
            return config;
        }

        private static CardNavModel Model(WallConfigData config, string poiId = Lamp) =>
            CardNavModel.Build(BlockStackBuilder.Build(config.pois.Single(p => p.id == poiId), config.card_settings, BlockRegistry.Shared, config.pois).Entries,
                BlockRegistry.Shared, config.card_settings);

        private static CardStrings Strings(string language) =>
            new(CardStringTableChecks.FrameworkEntries(), CardStringSources.Shared.Entries(), null, language, "en");

        // ---------------- Groups (rule 2.5.6) ----------------

        // The wall writes no Groups: the framework's four, in order, holding the eight families of section 3 -- The Lamp's 56 targets merge
        // into Discover 25 (About 18 + Stories 7), Watch 15 (Media 14 + Listen 1), Explore 7 (In AR 3 + Play 4), Visit 9 (Visit 7 + Community 2)
        [Test]
        public void TheDefaultGroups_MergeTheLampsEightFamilies_IntoFourInTheStacksOrder()
        {
            var config = Shipped();
            Assert.IsEmpty(config.card_settings.navigation.groups, "precondition: the shipped wall has no Groups rows");
            CollectionAssert.AreEqual(NavGroupRule.DefaultKeys, NavGroupRule.Groups(config.card_settings.navigation).Select(g => g.Key));
            var model = Model(config);
            Assert.AreEqual(56, model.TargetCount, "precondition: section 4.4's 56 targets");
            CollectionAssert.AreEqual(new[] { NavGroupRule.Discover, NavGroupRule.Watch, NavGroupRule.Explore, NavGroupRule.Visit }, model.Groups.Select(g => g.Key));
            CollectionAssert.AreEqual(new[] { 25, 15, 7, 9 }, model.Groups.Select(g => g.Count));
            Assert.AreEqual(model.TargetCount, model.Groups.Sum(g => g.Count), "every target is in exactly one group");
            foreach (var group in model.Groups)
                CollectionAssert.AreEqual(model.Targets.Where(group.Targets.Contains), group.Targets, group.Key + ": its targets in the stack's order");
            Assert.AreEqual(BuiltInFamilies.Stories, model.Sections[model.SectionOf(model.Groups[0].Targets.Last())].Key, "Discover ends with a Stories block");
        }

        // A wall's own rows replace the four; a Families row moves a family to another group; a family whose group the wall does not
        // have (and an app family with no row) is in none, and a group with no target is hidden
        [Test]
        public void AWallsOwnGroups_AndAFamilysRow_DecideTheMerge_AndAGroupWithNoTargetIsHidden()
        {
            var config = Shipped();
            var nav = config.card_settings.navigation;
            nav.groups = new List<CardNavGroup>
            {
                new() { key = "group_1", label = new List<LocalizedEntry> { new() { lang = "en", value = "Read" } } },
                new() { key = NavGroupRule.Watch },
                new() { key = "group_3" },
            };
            nav.families.Add(new CardFamilySetting { family = BuiltInFamilies.About, order = 10, group = "group_1" });
            nav.families.Add(new CardFamilySetting { family = BuiltInFamilies.Stories, order = 40, group = "group_1" });

            Assert.AreEqual("group_1", NavGroupRule.GroupOf(BuiltInFamilies.About, nav), "the row's group");
            Assert.AreEqual(NavGroupRule.Watch, NavGroupRule.GroupOf(BuiltInFamilies.Media, nav), "no row: its default, which this wall kept");
            Assert.IsNull(NavGroupRule.GroupOf(BuiltInFamilies.Play, nav), "its default (Explore) is not one of this wall's groups: none");
            Assert.IsNull(NavGroupRule.GroupOf("app_family", nav), "an app family with no row: none");
            Assert.IsNull(NavGroupRule.DefaultGroupOf("app_family"));

            var model = Model(config);
            CollectionAssert.AreEqual(new[] { "group_1", NavGroupRule.Watch }, model.Groups.Select(g => g.Key), "group_3 has no family: hidden");
            CollectionAssert.AreEqual(new[] { 25, 15 }, model.Groups.Select(g => g.Count));
            string playBlock = model.Sections.Single(s => s.Key == BuiltInFamilies.Play).Targets[0];
            Assert.AreEqual(-1, model.GroupOf(playBlock), "a Play block is in no group of this wall");
            Assert.GreaterOrEqual(model.SectionOf(playBlock), 0, "but still in its section");
        }

        // A group's words: the wall's own in the card's language (then the fallback), else the framework's word for a default key, else the key
        [Test]
        public void AGroupsWords_AreTheWallsOwn_ThenTheFrameworksForItsKey_ThenItsKey()
        {
            var own = new NavGroupRule.Group("group_1", new List<LocalizedEntry> { new() { lang = "en", value = "Read" }, new() { lang = "pt", value = "Ler" } }, null);
            Assert.AreEqual("Ler", NavGroupRule.Words(own, Strings("pt")));
            Assert.AreEqual("Read", NavGroupRule.Words(own, Strings("en")));
            var englishOnly = new NavGroupRule.Group("group_1", new List<LocalizedEntry> { new() { lang = "en", value = "Read" } }, null);
            Assert.AreEqual("Read", NavGroupRule.Words(englishOnly, Strings("pt")), "no Portuguese words: the fallback's");

            var defaults = NavGroupRule.Groups(null);
            Assert.AreEqual("Descobrir", NavGroupRule.Words(defaults[0], Strings("pt")), "a default row: the framework's word in Portuguese");
            Assert.AreEqual("Discover", NavGroupRule.Words(defaults[0], Strings("en")));
            Assert.AreEqual("group_9", NavGroupRule.Words(new NavGroupRule.Group("group_9", null, null), Strings("en")), "nothing at all: the key");
        }

        // ---------------- the model's icons, colours, kinds and lookups ----------------

        [Test]
        public void EverySection_CarriesItsFamilysIconAndColour_AndAFormatSectionNeither()
        {
            var config = Shipped();
            var model = Model(config);
            foreach (var section in model.Sections)
            {
                BlockRegistry.Shared.TryGetFamily(section.Key, out var family);
                Assert.AreEqual(family.IconKey, section.IconKey, section.Key);
                Assert.AreEqual(family.ColorToken, section.ColorToken, section.Key);
                foreach (var kind in section.Kinds)
                {
                    BlockRegistry.Shared.TryGet(kind.Key, out var definition);
                    Assert.AreEqual(definition.LabelKey, kind.LabelKey, kind.Key + ": its words");
                    Assert.AreEqual(family.ColorToken, kind.ColorToken, kind.Key + ": its family's colour");
                }
            }

            config.card_settings.navigation.group_by = CardOptions.GroupByFormat;
            var byFormat = Model(config);
            foreach (var section in byFormat.Sections)
            {
                Assert.IsNull(section.IconKey, section.Key + ": a format has no icon");
                Assert.IsNull(section.ColorToken, section.Key + ": nor a colour");
                foreach (var kind in section.Kinds) Assert.IsNotNull(kind.ColorToken, kind.Key + ": a kind keeps its family's colour under a format");
            }
            CollectionAssert.AreEqual(model.Groups.Select(g => g.Count), byFormat.Groups.Select(g => g.Count), "Groups merge FAMILIES whatever Group By says");
        }

        // Targets is every target in the stack's order; SectionOf / KindOf / GroupOf agree with the sections, kinds and groups that hold it,
        // in the authored order too (families interleave there); a block that is no target is in nothing
        [Test]
        public void EveryTarget_IsFoundInItsSectionKindAndGroup_InEveryBlockOrder()
        {
            foreach (string order in CardOptions.BlockOrders)
            {
                var config = Shipped(order);
                var model = Model(config);
                var stack = BlockStackBuilder.Build(config.pois.Single(p => p.id == Lamp), config.card_settings, BlockRegistry.Shared, config.pois).Entries;
                CollectionAssert.AreEqual(stack.Select(e => e.Instance.key).Where(model.Targets.Contains), model.Targets, order + ": the stack's order");
                foreach (string target in model.Targets)
                {
                    var section = model.Sections[model.SectionOf(target)];
                    CollectionAssert.Contains(section.Targets, target, order + ": " + target + " in its section");
                    CollectionAssert.Contains(model.KindOf(target).Targets, target, order + ": in its kind");
                    CollectionAssert.Contains(section.Kinds, model.KindOf(target), order + ": a kind of that section");
                    CollectionAssert.Contains(model.Groups[model.GroupOf(target)].Targets, target, order + ": in its group");
                }
                string sources = stack.First(e => e.Definition.Family == BuiltInFamilies.Meta).Instance.key;
                Assert.AreEqual(-1, model.SectionOf(sources), order + ": Sources is no target");
                Assert.IsNull(model.KindOf(sources));
                Assert.AreEqual(-1, model.GroupOf(sources));
            }
        }

        // ---------------- rule 2.5.7 per element ----------------

        // Text tabs need two sections, segmented two GROUPS, kind chips nothing more than navigation itself: a wall that puts every family in
        // one group keeps its tabs and loses its segments; a one-family card keeps its kind chips
        [Test]
        public void Rule7_CountsWhatEachElementChoosesBetween()
        {
            var config = Shipped();
            var nav = config.card_settings.navigation;
            var tabs = Element(BuiltInNavElements.TextTabs);
            var segmented = Element(BuiltInNavElements.Segmented);
            var chips = Element(BuiltInNavElements.KindChips);
            Assert.AreEqual(NavChoice.Sections, tabs.Choice);
            Assert.AreEqual(NavChoice.Groups, segmented.Choice);
            Assert.AreEqual(NavChoice.Blocks, chips.Choice);
            Assert.AreEqual(NavChoice.Sections, Element(BuiltInNavElements.IconTabs).Choice);
            Assert.AreEqual(NavChoice.Sections, Element(BuiltInNavElements.IconTabsChips).Choice);
            Assert.AreEqual(NavChoice.Sections, Element(BuiltInNavElements.ProgressMap).Choice);

            var model = Model(config);
            Assert.IsTrue(NavSlotRule.OffersAChoice(segmented, model), "four groups: segmented is drawn");
            nav.groups = new List<CardNavGroup> { new() { key = "group_1" } };
            foreach (var family in BlockRegistry.Shared.Families) nav.families.Add(new CardFamilySetting { family = family.Key, order = family.Order, group = "group_1" });
            model = Model(config);
            Assert.AreEqual(1, model.Groups.Count, "precondition: every family in one group");
            Assert.AreEqual(8, model.Sections.Count, "and still eight sections");
            Assert.IsFalse(NavSlotRule.OffersAChoice(segmented, model), "one segment is not a choice");
            Assert.IsTrue(NavSlotRule.OffersAChoice(tabs, model), "the tabs still have eight");

            var oneFamily = Shipped();
            oneFamily.card_settings.navigation.min_blocks = 2;
            var lamp = oneFamily.pois.Single(p => p.id == Lamp);
            lamp.card.blocks.RemoveAll(b => b.kind != BuiltInBlocks.HeaderKind && b.kind != BuiltInBlocks.RichTextKind && b.kind != BuiltInBlocks.QuickFactsKind);
            var one = Model(oneFamily);
            Assert.AreEqual(1, one.Sections.Count, "precondition: an About-only card");
            Assert.IsTrue(one.ShowsNavigation, "precondition: with navigation from 2");
            Assert.IsFalse(NavSlotRule.OffersAChoice(tabs, one), "one tab is not a choice");
            Assert.IsTrue(NavSlotRule.OffersAChoice(chips, one), "the kind chips list kinds: drawn");
            Assert.IsTrue(NavOntologyRule.OffersAKindChoice(one.Sections[0].Kinds.Count), "two kinds there: a chips row has a choice");
            Assert.IsFalse(NavOntologyRule.OffersAKindChoice(1), "one chip would only repeat its tab");
        }

        private static NavElementDefinition Element(string key)
        {
            Assert.IsTrue(NavElementRegistry.Shared.TryGet(key, out var definition), key + " is registered");
            return definition;
        }

        // ---------------- progress_map's widths ----------------

        // The Lamp's eight sections on a 390 px bar with 48 px minimums: 384 of minimums fit, so the bar is filled, each segment at least
        // 48, a bigger section never narrower than a smaller one, and the widths in the counts' proportion wherever no minimum holds them
        [Test]
        public void TheProgressMap_SizesSegmentsByTargetCount_NeverUnderTheirMinimum_AndFillsTheBar()
        {
            var counts = new[] { 18, 14, 1, 7, 3, 4, 7, 2 };
            var minimums = Enumerable.Repeat(48f, 8).ToArray();
            var widths = ProgressMapRule.Widths(counts, minimums, 390f);
            Assert.AreEqual(390f, widths.Sum(), 0.01f, "the bar is filled");
            for (int i = 0; i < 8; i++) Assert.GreaterOrEqual(widths[i], 48f - 0.001f, "segment " + i + " is a whole tap target");
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                    if (counts[i] > counts[j]) Assert.GreaterOrEqual(widths[i], widths[j] - 0.001f, "more targets, never narrower");
            // - at the phone's width only About's share beats the minimum: it takes all the bar the seven minimums leave
            Assert.AreEqual(390f - 7f * 48f, widths[0], 0.01f, "About (18) gets what the seven 48 px segments leave");

            var wide = ProgressMapRule.Widths(counts, minimums, 1200f);
            Assert.AreEqual(18f / 14f, wide[0] / wide[1], 0.001f, "on a wide bar two unclamped segments keep their counts' proportion");
            Assert.AreEqual(1200f, wide.Sum(), 0.01f);

            var roomy = ProgressMapRule.Widths(new[] { 1, 3 }, new[] { 10f, 10f }, 400f);
            CollectionAssert.AreEqual(new[] { 100f, 300f }, roomy, "no minimum in the way: exactly proportional");

            var tight = ProgressMapRule.Widths(new[] { 18, 14, 1, 7, 3, 4, 7, 2, 1 }, Enumerable.Repeat(48f, 9).ToArray(), 390f);
            CollectionAssert.AreEqual(Enumerable.Repeat(48f, 9), tight, "nine minimums are wider than the bar: every segment keeps 48 and the bar scrolls");
            CollectionAssert.IsEmpty(ProgressMapRule.Widths(new int[0], new float[0], 390f));
            CollectionAssert.AreEqual(new[] { 70f, 48f }, ProgressMapRule.Widths(new[] { 1, 1 }, new[] { 70f, 48f }, 100f), "minimums over the room: kept");
        }

        // ---------------- Labels ----------------

        [Test]
        public void Labels_ShowWhatTheyName_AndAnItemWithNoIconAlwaysShowsItsWords()
        {
            Assert.AreEqual(CardOptions.LabelsIconAndText, new CardNavigationSettings().labels, "the default");
            Assert.AreEqual(CardOptions.Labels.Length, CardOptions.LabelsLabels.Length);
            Assert.IsTrue(NavLabelRule.ShowsIcon(CardOptions.LabelsIconAndText, true) && NavLabelRule.ShowsWords(CardOptions.LabelsIconAndText, true));
            Assert.IsTrue(NavLabelRule.ShowsIcon(CardOptions.LabelsIcon, true));
            Assert.IsFalse(NavLabelRule.ShowsWords(CardOptions.LabelsIcon, true), "Icon: the icon alone");
            Assert.IsFalse(NavLabelRule.ShowsIcon(CardOptions.LabelsText, true), "Text: the words alone");
            Assert.IsTrue(NavLabelRule.ShowsWords(CardOptions.LabelsText, true));
            foreach (string labels in CardOptions.Labels)
            {
                Assert.IsFalse(NavLabelRule.ShowsIcon(labels, false), labels + ": no icon to show");
                Assert.IsTrue(NavLabelRule.ShowsWords(labels, false), labels + ": an item with no icon (a format) keeps its words");
            }
            Assert.IsTrue(NavLabelRule.ShowsIcon("typo", true) && NavLabelRule.ShowsWords("typo", true), "an unknown value reads as the default");
        }
    }
}
