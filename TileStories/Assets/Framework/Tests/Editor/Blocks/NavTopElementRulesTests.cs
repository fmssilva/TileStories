using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using TileStories.Tests;

namespace TileStories.Editor.Tests
{
    // NavTopElementRulesTests's shared ground: its set-up and helpers, so its own tests and its old-navigation ones stand on one fixture
    public abstract class NavTopElementRulesFixture
    {
        protected const string Lamp = "lamp";

        protected static WallConfigData Shipped(string blockOrder = CardOptions.BlockOrderByFamily)
        {
            var config = JsonUtility.FromJson<WallConfigData>(File.ReadAllText("Assets/Apps/LivingRoom/config.json"));
            config.card_settings.navigation.block_order = blockOrder;
            return config;
        }

        protected static CardNavModel Model(WallConfigData config, string poiId = Lamp) =>
            CardNavModel.Build(BlockStackBuilder.Build(config.pois.Single(p => p.id == poiId), config.card_settings, BlockRegistry.Shared, config.pois).Entries,
                BlockRegistry.Shared, config.card_settings);

        protected static CardStrings Strings(string language) =>
            new(CardStringTableChecks.FrameworkEntries(), CardStringSources.Shared.Entries(), null, language, "en");

        protected static NavElementDefinition Element(string key)
        {
            Assert.IsTrue(NavElementRegistry.Shared.TryGet(key, out var definition), key + " is registered");
            return definition;
        }
    }

    // Tier 0 of what the step 6 top elements read (_3.2 sections 2.5, 3 and 5.1), on the SHIPPED LivingRoom wall through the real
    // BlockStackBuilder and the shared registry: the Groups (rule 2.5.6 -- the defaults, a wall's own rows, a family moved to another
    // group, one left in none, the words), the model's icons, colours, kind words and lookups, the progress map's segment widths, Labels,
    // and which elements rule 2.5.7 draws for a card.
    public class NavTopElementRulesTests : NavTopElementRulesFixture
    {
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
        }

        // Targets is every target in the stack's order; SectionOf / KindOf agree with the sections and kinds that hold it,
        // in the authored order too (families interleave there); a block that is no target is in nothing
        [Test]
        public void EveryTarget_IsFoundInItsSectionAndKind_InEveryBlockOrder()
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
                }
                string sources = stack.First(e => e.Definition.Family == BuiltInFamilies.Meta).Instance.key;
                Assert.AreEqual(-1, model.SectionOf(sources), order + ": Sources is no target");
                Assert.IsNull(model.KindOf(sources));
            }
        }
    }

    // NavTopElementRulesTests's tests whose subject is an old navigation element: they wait for its rebuild on the Navigation Bar's pattern
    [Ignore(TestSkips.OldNavigationElements)]
    public class NavTopElementRulesOldNavigationTests : NavTopElementRulesFixture
    {
        // ---------------- Groups (rule 2.5.6) ----------------

        // ---------------- rule 2.5.7 per element ----------------

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
