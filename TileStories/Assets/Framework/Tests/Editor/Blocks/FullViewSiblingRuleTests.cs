using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories.Editor.Tests
{
    // Which blocks the full view swipes between (_3.2 section 5.7, step 16), pure, on the SHIPPED LivingRoom wall through the real
    // BlockStackBuilder, registry and CardNavModel:
    //   - a block's siblings are its section's targets whose kind supports the full view, in the stack's order, itself among them: The Lamp's
    //     Media block_58 (a 3D model) has the 14 Media blocks, block_56 and block_57 (two videos) among them
    //   - a family of one (The Lamp's Listen: the audio guide alone) has none, nor does a block that is no target (the header, the sources)
    //   - an app kind that does not list Expandable is no sibling, and opens alone
    //   - Group By = format: the section is the format the card shows, so a video's siblings are the card's other video
    public class FullViewSiblingRuleTests
    {
        private const string Lamp = "lamp";

        private sealed class PlainView : IBlockView
        {
            public VisualElement Root { get; } = new VisualElement();
            public void Bind(BlockInstanceData instance, BlockBindContext context) { }
            public void Unbind() { }
        }

        private static WallConfigData Shipped() => JsonUtility.FromJson<WallConfigData>(File.ReadAllText("Assets/Apps/LivingRoom/config.json"));

        private static List<BlockStackBuilder.Entry> Stack(WallConfigData config, BlockRegistry registry) =>
            BlockStackBuilder.Build(config.pois.Single(p => p.id == Lamp), config.card_settings, registry, config.pois).Entries;

        private static CardNavModel Model(WallConfigData config, BlockRegistry registry = null)
        {
            registry ??= BlockRegistry.Shared;
            return CardNavModel.Build(Stack(config, registry), registry, config.card_settings);
        }

        [Test]
        public void TheLampsMediaBlock_HasItsSectionsTargets_InTheStacksOrder_ItselfAmongThem()
        {
            var config = Shipped();
            var model = Model(config);
            int media = model.Sections.ToList().FindIndex(s => s.Key == BuiltInFamilies.Media);
            Assert.GreaterOrEqual(media, 0, "precondition: The Lamp has a Media section");
            var targets = model.Sections[media].Targets;
            Assert.AreEqual(14, targets.Count, "precondition: The Lamp's 14 Media blocks");

            var siblings = FullViewSiblingRule.Siblings(model, BlockRegistry.Shared, "block_58");
            CollectionAssert.AreEqual(targets, siblings, "every Media target, in the section's (the stack's) order");
            CollectionAssert.Contains(siblings, "block_56");
            CollectionAssert.Contains(siblings, "block_57");
            Assert.Less(siblings.ToList().IndexOf("block_56"), siblings.ToList().IndexOf("block_57"), "the two videos in the card's order");
            Assert.AreEqual(media, FullViewSiblingRule.SectionOf(model, "block_58"), "the breadcrumb names Media");
            foreach (string key in targets)
                CollectionAssert.AreEqual(siblings, FullViewSiblingRule.Siblings(model, BlockRegistry.Shared, key), key + ": the same siblings from any of them");
            // - the stack's own order, measured on the stack itself
            var order = Stack(config, BlockRegistry.Shared).Select(e => e.Instance.key).ToList();
            CollectionAssert.AreEqual(siblings.OrderBy(k => order.IndexOf(k)).ToList(), siblings, "the stack's order");
        }

        [Test]
        public void AFamilyOfOne_AndABlockThatIsNoTarget_HaveNoSiblings()
        {
            var config = Shipped();
            var model = Model(config);
            var listen = model.Sections.Single(s => s.Key == BuiltInFamilies.Guides);
            Assert.AreEqual(1, listen.Targets.Count, "precondition: Listen holds the audio guide alone");
            CollectionAssert.IsEmpty(FullViewSiblingRule.Siblings(model, BlockRegistry.Shared, listen.Targets[0]), "a family of one: no chips");
            Assert.GreaterOrEqual(FullViewSiblingRule.SectionOf(model, listen.Targets[0]), 0, "but its breadcrumb still names Listen");

            var entries = Stack(config, BlockRegistry.Shared);
            string header = entries[0].Instance.key;
            var sources = entries.Where(e => e.Definition.Key == "sources").Select(e => e.Instance.key).ToList();
            Assert.AreEqual(2, sources.Count, "precondition: The Lamp's two sources");
            foreach (string key in sources.Append(header).Append("block_999").Append(null))
            {
                CollectionAssert.IsEmpty(FullViewSiblingRule.Siblings(model, BlockRegistry.Shared, key), key + ": no target, no siblings");
                Assert.AreEqual(-1, FullViewSiblingRule.SectionOf(model, key), key + ": no section to name");
            }
            CollectionAssert.IsEmpty(FullViewSiblingRule.Siblings(null, BlockRegistry.Shared, "block_58"), "no model: none");
        }

        [Test]
        public void AnAppKindThatDoesNotListExpandable_IsNoSibling_AndOpensAlone()
        {
            var registry = new BlockRegistry();
            BuiltInBlocks.Register(registry);
            registry.Register(new BlockKindDefinition
            {
                Key = "app_still", Family = BuiltInFamilies.Media, Format = CardOptions.FormatLook, DisplayName = "App still",
                Variants = new[] { "plain" }, DefaultVariant = "plain", DisplayModes = new[] { CardOptions.DisplayInline },
            }, () => new PlainView());
            var config = Shipped();
            // - an in-memory copy (no identity field touched): one app block among The Lamp's Media blocks
            config.pois.Single(p => p.id == Lamp).card.blocks.Add(new BlockInstanceData { key = "block_app", kind = "app_still" });
            var model = Model(config, registry);
            int media = FullViewSiblingRule.SectionOf(model, "block_app");
            Assert.AreEqual(BuiltInFamilies.Media, model.Sections[media].Key, "precondition: the app block is a Media target");
            Assert.IsFalse(FullViewSiblingRule.SupportsTheFullView("app_still", registry));
            Assert.IsTrue(FullViewSiblingRule.SupportsTheFullView(BuiltInBlocks.Video.Key, registry));
            Assert.IsFalse(FullViewSiblingRule.SupportsTheFullView("header", registry), "the header lists no Expandable");

            var siblings = FullViewSiblingRule.Siblings(model, registry, "block_58");
            CollectionAssert.DoesNotContain(siblings, "block_app", "a kind that refuses the full view is not swiped to");
            Assert.AreEqual(model.Sections[media].Targets.Count - 1, siblings.Count, "every other Media target is");
            CollectionAssert.IsEmpty(FullViewSiblingRule.Siblings(model, registry, "block_app"), "and it opens alone");
        }

        // The chips' strip (NavStripRule.OffsetToCentre): the current chip goes to the middle, so the NEXT chip is always in view to tap --
        // the least move (OffsetToShow) leaves it cut at the edge, which found Phase B unable to step on by taps. 20 chips of 56 in a 358 strip
        [Test]
        public void TheChipStrip_CentresTheCurrentChip_SoItsNeighboursStayATapAway_AndStopsAtBothEnds()
        {
            const float chip = 56f, strip = 358f;
            const float content = chip * 20;
            for (int current = 0; current < 20; current++)
            {
                float offset = NavStripRule.OffsetToCentre(current * chip, chip, strip, content, 0f);
                Assert.GreaterOrEqual(offset, 0f, current + ": never before the row's start");
                Assert.LessOrEqual(offset, content - strip + 0.01f, current + ": never past its end");
                bool InView(int i) => i * chip >= offset - 0.01f && (i + 1) * chip <= offset + strip + 0.01f;
                Assert.IsTrue(InView(current), current + ": the current chip whole in view");
                if (current < 19) Assert.IsTrue(InView(current + 1), current + ": the next chip whole in view");
                if (current > 0) Assert.IsTrue(InView(current - 1), current + ": the previous chip whole in view");
            }
            float least = NavStripRule.OffsetToShow(10 * chip, chip, strip, content, 0f);
            Assert.Less(least + strip, 12 * chip, "precondition: the least move leaves chip 11 cut at the edge");
            Assert.AreEqual(0f, NavStripRule.OffsetToCentre(0f, chip, strip, chip * 4, 0f), "a row that fits never moves");
            Assert.AreEqual(12f, NavStripRule.OffsetToCentre(float.NaN, chip, strip, content, 12f), "not laid out yet: stays");
        }

        [Test]
        public void GroupByFormat_TheSectionIsTheFormat_SoAVideosSiblingsAreTheCardsVideos()
        {
            var config = Shipped();
            config.card_settings.navigation.group_by = CardOptions.GroupByFormat;
            var model = Model(config);
            int section = FullViewSiblingRule.SectionOf(model, "block_56");
            Assert.AreEqual(CardOptions.FormatWatch, model.Sections[section].Key, "precondition: the video's section is Watch");
            CollectionAssert.AreEqual(new[] { "block_56", "block_57" }, FullViewSiblingRule.Siblings(model, BlockRegistry.Shared, "block_56"),
                "the two videos: what the breadcrumb (Watch) and its jump also name");
        }
    }
}
