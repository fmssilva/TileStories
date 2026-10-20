using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // Auto Nav Block (_3.2 section 3, step 11a): BlockStackBuilder.InsertAutoNavBlock on the SHIPPED LivingRoom wall, through the real
    // builder and the shared registry. Every branch of the rule: a card that does not meet it keeps EXACTLY the stack it had (compared
    // entry by entry, instance JSON included), a card that meets it gains one block in second place in every Block Order, a card exactly
    // at Show Navigation From gets it and one below does not, a point that shows its own nav block gets none, and none / an unknown key /
    // another family's kind / a kind off in the Block Library insert nothing. And the navigation family is no target: the card's nav
    // model is the same with the grid as without it.
    public class AutoNavBlockTests
    {
        private const string Lamp = "lamp";
        private const string LampMilitary = "lamp_military";
        private const string LampReligious = "lamp_religious";
        private static readonly string AutoKey = BlockStackBuilder.AutoNavBlockKey(BuiltInBlocks.NavIconGridKind);

        private static WallConfigData Shipped() =>
            JsonUtility.FromJson<WallConfigData>(File.ReadAllText("Assets/Apps/LivingRoom/config.json"));

        private static WallConfigData WithAuto(string kind, int? minBlocks = null, string blockOrder = null)
        {
            var config = Shipped();
            config.card_settings.navigation.auto_nav_block = kind;
            if (minBlocks.HasValue) config.card_settings.navigation.min_blocks = minBlocks.Value;
            if (blockOrder != null) config.card_settings.navigation.block_order = blockOrder;
            return config;
        }

        private static List<BlockStackBuilder.Entry> Stack(WallConfigData config, string poiId) =>
            BlockStackBuilder.Build(config.pois.Single(p => p.id == poiId), config.card_settings, BlockRegistry.Shared, config.pois).Entries;

        // One line per entry -- its key, kind, resolved look, synthesized flag and the instance itself as JSON -- so two stacks compare
        // byte for byte, not only by count
        private static List<string> Print(IEnumerable<BlockStackBuilder.Entry> entries) =>
            entries.Select(e => e.Instance.key + "|" + e.Instance.kind + "|" + e.Variant + "|" + e.Synthesized + "|" + JsonUtility.ToJson(e.Instance)).ToList();

        private static int TargetCount(WallConfigData config, string poiId) =>
            CardNavModel.Build(Stack(config, poiId), BlockRegistry.Shared, config.card_settings, null).TargetCount;

        [OneTimeSetUp]
        public void RegisteredKinds() =>
            Assert.IsTrue(BlockRegistry.Shared.TryGet(BuiltInBlocks.NavIconGridKind, out _), "precondition: the framework registers nav_icon_grid");

        [Test]
        public void TheShippedDefault_IsNone_AndEveryFixtureKeepsTheStackItHad()
        {
            var shipped = Shipped();
            Assert.AreEqual(CardOptions.NavNone, new CardNavigationSettings().auto_nav_block, "the type's default is none");
            Assert.AreEqual(CardOptions.NavNone, shipped.card_settings.navigation.auto_nav_block, "the shipped wall leaves it at none");
            var none = WithAuto(CardOptions.NavNone);
            foreach (string poi in new[] { Lamp, LampMilitary, LampReligious })
            {
                var entries = Stack(shipped, poi);
                CollectionAssert.AreEqual(Print(Stack(none, poi)), Print(entries), poi + ": none written out = the shipped stack");
                Assert.IsFalse(entries.Any(e => e.Definition.Family == BuiltInFamilies.Navigation), poi + ": no nav block anywhere");
            }
        }

        [Test]
        public void ACardBelowShowNavigationFrom_KeepsItsStackByteForByte()
        {
            var shipped = Shipped();
            var grid = WithAuto(BuiltInBlocks.NavIconGridKind);
            Assert.Less(TargetCount(shipped, LampMilitary), NavOntologyRule.MinBlocks(shipped.card_settings.navigation), "precondition: Military is below");
            Assert.Less(TargetCount(shipped, LampReligious), NavOntologyRule.MinBlocks(shipped.card_settings.navigation), "precondition: Religious is below");
            foreach (string poi in new[] { LampMilitary, LampReligious })
                CollectionAssert.AreEqual(Print(Stack(shipped, poi)), Print(Stack(grid, poi)), poi + ": below the threshold nothing changes");
        }

        [TestCase(CardOptions.BlockOrderAuthored)]
        [TestCase(CardOptions.BlockOrderEndFamiliesLast)]
        [TestCase(CardOptions.BlockOrderByFamily)]
        public void TheLamp_GainsExactlyOneBlock_InSecondPlace_InEveryBlockOrder(string order)
        {
            var before = Stack(WithAuto(CardOptions.NavNone, blockOrder: order), Lamp);
            var after = Stack(WithAuto(BuiltInBlocks.NavIconGridKind, blockOrder: order), Lamp);

            Assert.AreEqual(before.Count + 1, after.Count, "exactly one block more");
            var inserted = after[1];
            Assert.AreEqual(BuiltInBlocks.NavIconGridKind, inserted.Instance.kind);
            Assert.AreEqual(AutoKey, inserted.Instance.key, "the inserted block's key");
            Assert.IsTrue(inserted.Synthesized, "it is in no config");
            Assert.AreEqual(BuiltInBlocks.NavIconGridTiles, inserted.Variant, "the kind's Block Library look");
            Assert.AreEqual(BuiltInBlocks.HeaderKind, after[0].Definition.Key, "the header stays first");
            after.RemoveAt(1);
            CollectionAssert.AreEqual(Print(before), Print(after), "every other block is exactly where and what it was");
        }

        [Test]
        public void ACardExactlyAtShowNavigationFrom_GetsIt_AndOneTargetShortDoesNot()
        {
            int military = TargetCount(Shipped(), LampMilitary);
            Assert.AreEqual(4, military, "precondition: Lamp - Military has 4 targets (_3.2 4.4)");

            var at = Stack(WithAuto(BuiltInBlocks.NavIconGridKind, minBlocks: military), LampMilitary);
            Assert.AreEqual(AutoKey, at[1].Instance.key, "at the threshold (4 of 4): inserted");

            var below = Stack(WithAuto(BuiltInBlocks.NavIconGridKind, minBlocks: military + 1), LampMilitary);
            Assert.IsFalse(below.Any(e => e.Instance.key == AutoKey), "one short (4 of 5): not inserted");
        }

        [Test]
        public void APointThatShowsItsOwnNavBlock_GainsNone()
        {
            var config = WithAuto(BuiltInBlocks.NavIconGridKind);
            var lamp = config.pois.Single(p => p.id == Lamp);
            // - the point authors its own grid, last in Card Content (Block Order may put it anywhere: it is still the card's own)
            lamp.card.blocks.Add(new BlockInstanceData { key = "block_900", kind = BuiltInBlocks.NavIconGridKind });

            var entries = Stack(config, Lamp);
            var nav = entries.Where(e => e.Definition.Family == BuiltInFamilies.Navigation).ToList();
            Assert.AreEqual(1, nav.Count, "one nav block: the point's own");
            Assert.AreEqual("block_900", nav[0].Instance.key);
            Assert.IsFalse(nav[0].Synthesized);
        }

        [Test]
        public void TheKindOffInTheBlockLibrary_InsertsNothing_AndHidesAnAuthoredOneToo()
        {
            // - the authored grid is switched off in the Block Library: it is not in the stack, and the auto grid is the same kind, so
            //   it is off too -- nothing
            var config = WithAuto(BuiltInBlocks.NavIconGridKind);
            config.pois.Single(p => p.id == Lamp).card.blocks.Add(new BlockInstanceData { key = "block_900", kind = BuiltInBlocks.NavIconGridKind });
            config.card_settings.kinds.Add(new BlockKindSetting { kind = BuiltInBlocks.NavIconGridKind, enabled = false });
            Assert.IsFalse(Stack(config, Lamp).Any(e => e.Definition.Family == BuiltInFamilies.Navigation), "a kind off in the Block Library: no grid at all");
        }

        // _3.2 step 15a: the rails need no authored data (they are the card's own sections), so Auto Nav Block puts them in like the grid --
        // second place in every Block Order, every other entry identical -- while the bento (tiles an author fills in) would be an empty block
        // the builder skips: the card never puts it in, and the kind says why (a Required field), not a list of keys
        [TestCase("authored")]
        [TestCase("end_families_last")]
        [TestCase("by_family")]
        public void TheRails_AreInsertedInSecondPlace_LikeTheGrid_InEveryBlockOrder_AndTheOtherEntriesAreUntouched(string order)
        {
            string railsKey = BlockStackBuilder.AutoNavBlockKey(BuiltInBlocks.NavRailsKind);
            var without = Stack(WithAuto(CardOptions.NavNone, blockOrder: order), Lamp);
            var with = Stack(WithAuto(BuiltInBlocks.NavRailsKind, blockOrder: order), Lamp);
            Assert.AreEqual(without.Count + 1, with.Count, order + ": one block more");
            Assert.AreEqual(railsKey, with[1].Instance.key, order + ": in second place, after the header");
            Assert.IsTrue(with[1].Synthesized, "synthesized: in no config");
            CollectionAssert.AreEqual(Print(without), Print(with.Where(e => e.Instance.key != railsKey)), order + ": every other entry identical");
        }

        [Test]
        public void TheBento_IsNeverInsertedByTheCard_BecauseItsTilesAreRequired_AndTheKindsTellWhy()
        {
            Assert.IsTrue(BlockRegistry.Shared.TryGet(BuiltInBlocks.NavBentoKind, out var bento), "precondition: the framework registers nav_bento");
            Assert.IsTrue(bento.NeedsAuthoredContent, "Highlights has a Required Tiles field");
            foreach (string kind in new[] { BuiltInBlocks.NavIconGridKind, BuiltInBlocks.NavContentsKind, BuiltInBlocks.NavRailsKind })
            {
                Assert.IsTrue(BlockRegistry.Shared.TryGet(kind, out var definition), kind + " is registered");
                Assert.IsFalse(definition.NeedsAuthoredContent, kind + " is drawn from the card's own sections: nothing to author");
            }
            // - the property is the field's, never a list: every registered kind says it exactly when one of its fields is Required
            foreach (var kind in BlockRegistry.Shared.Ordered)
                Assert.AreEqual(kind.Fields.Any(f => f.Required), kind.NeedsAuthoredContent, kind.Key);
            CollectionAssert.AreEqual(Print(Stack(WithAuto(CardOptions.NavNone), Lamp)), Print(Stack(WithAuto(BuiltInBlocks.NavBentoKind), Lamp)),
                "the wall names Highlights: the card inserts nothing, byte for byte the stack it had");
        }

        [TestCase("")]
        [TestCase("not_a_kind")]
        [TestCase(BuiltInBlocks.RichTextKind)]
        [TestCase(BuiltInBlocks.HeaderKind)]
        public void AKeyThatIsNoNavigationKind_InsertsNothing(string kind)
        {
            CollectionAssert.AreEqual(Print(Stack(Shipped(), Lamp)), Print(Stack(WithAuto(kind), Lamp)), "'" + kind + "' is no nav block");
        }

        [Test]
        public void TheNavigationFamily_IsNoTarget_TheLampsModelIsTheSameWithTheGrid()
        {
            Assert.IsTrue(BlockRegistry.Shared.TryGetFamily(BuiltInFamilies.Navigation, out var family), "the family is registered");
            Assert.AreEqual(85, family.Order);
            Assert.IsFalse(family.InNav, "a nav block is never a navigation target");
            Assert.IsFalse(family.AtEnd);

            var config = WithAuto(BuiltInBlocks.NavIconGridKind);
            var without = CardNavModel.Build(Stack(Shipped(), Lamp), BlockRegistry.Shared, config.card_settings, null);
            var entries = Stack(config, Lamp);
            Assert.IsTrue(entries.Any(e => e.Instance.key == AutoKey), "precondition: the grid is in the stack");
            var with = CardNavModel.Build(entries, BlockRegistry.Shared, config.card_settings, null);

            Assert.AreEqual(without.Sections.Count, with.Sections.Count, "the section count does not change");
            CollectionAssert.AreEqual(without.Sections.Select(s => s.Key + ":" + string.Join(",", s.Targets)),
                with.Sections.Select(s => s.Key + ":" + string.Join(",", s.Targets)), "the same sections, the same targets");
            CollectionAssert.DoesNotContain(with.Targets, AutoKey, "the grid itself is no target");
            Assert.IsFalse(with.Sections.Any(s => s.Key == BuiltInFamilies.Navigation), "no element ever shows a Navigation section");
        }

        [Test]
        public void TheInsertedKey_CannotCollideWithAnAuthoredKey()
        {
            StringAssert.Contains(":", AutoKey, "the inserted key holds a ':'");
            StringAssert.DoesNotContain(":", TaxonomyRowKeys.BlockPrefix, "the Editor's block keys are block_N: never a ':'");
            var authored = Shipped().pois.Where(p => p.card?.blocks != null).SelectMany(p => p.card.blocks).Select(b => b.key).ToList();
            Assert.Greater(authored.Count, 60, "precondition: the wall's authored blocks");
            Assert.IsFalse(authored.Any(k => k.Contains(":")), "no authored key on the wall holds a ':'");
            Assert.AreEqual(AutoKey, Stack(WithAuto(BuiltInBlocks.NavIconGridKind), Lamp)[1].Instance.key,
                "and the key is the same on every build (seen state and BlockRefs key off it)");
        }
    }
}
