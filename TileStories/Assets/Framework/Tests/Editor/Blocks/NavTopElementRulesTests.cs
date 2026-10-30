using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // Tier 0 of the navigation model the card's sections read (_3.2 sections 2.5 and 3), on the SHIPPED LivingRoom wall through the real
    // BlockStackBuilder and the shared registry: each section's icon and colour (none under a format), the kinds' words and colours, and
    // every target found in its section and kind in every Block Order.
    public class NavTopElementRulesTests
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
}
