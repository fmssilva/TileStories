using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // BlockRefRule (_3.2 step 4) on the SHIPPED LivingRoom wall through the REAL BlockStackBuilder: the blocks a BlockRef can take the
    // visitor to are exactly the shown blocks of the scrolling stack -- The Lamp's header, its pinned audio hero chip, its sticky footer
    // button and its Show After Reading question (until revealed) are not -- and Check names why a pick leads nowhere. Plus the
    // definition side: BlockRegistry.Validate refuses a broken BlockRef field the way it refuses any other broken field.
    public class BlockRefRuleTests
    {
        private const string Lamp = "lamp";
        // The Lamp's blocks the card draws OUTSIDE what scrolls, or hides until the card is read (config.json, checked below)
        private const string Header = "block_1";
        private const string HeroChip = "block_54";
        private const string StickyFooter = "block_19";
        private const string WaitsForReading = "block_44";

        private static WallConfigData Shipped() =>
            JsonUtility.FromJson<WallConfigData>(File.ReadAllText("Assets/Apps/LivingRoom/config.json"));

        private static POIData LampOf(WallConfigData config) => config.pois.Single(p => p.id == Lamp);

        private static BlockStackBuilder.Result Built(WallConfigData config) =>
            BlockStackBuilder.Build(LampOf(config), config.card_settings, BlockRegistry.Shared, config.pois);

        private static BlockInstanceData Block(WallConfigData config, string key) => LampOf(config).card.blocks.Single(b => b.key == key);

        [Test]
        public void TheLampsFixture_HasTheFourBlocksThisFixtureNames_InTheLooksThatPutThemOutsideTheScroll()
        {
            var config = Shipped();
            Assert.AreEqual(BuiltInBlocks.HeaderKind, Block(config, Header).kind);
            Assert.AreEqual(BuiltInBlocks.AudioGuideHeroChip, Block(config, HeroChip).variant, "the hero chip is a pinned-top look");
            Assert.AreEqual(BuiltInBlocks.ActionsStickyCta, Block(config, StickyFooter).variant, "the sticky call to action is a footer look");
            Assert.IsTrue(new BlockFieldReader(Block(config, WaitsForReading), null, null).Flag(BuiltInBlocks.KnowledgeCheckShowAfterViewedField),
                "the true / false question waits for Show After Reading");
            CollectionAssert.IsEmpty(Built(config).Skipped, "precondition: every block of The Lamp is shown");
        }

        [Test]
        public void JumpTargets_AreEveryShownBlockOfTheScroll_ExceptTheHeaderThePinnedChipTheFooterAndAWaitingBlock_UntilItIsRevealed()
        {
            var built = Built(Shipped());
            var everyKey = built.Entries.Select(e => e.Instance.key).ToList();
            Assert.AreEqual(LampOf(Shipped()).card.blocks.Count, everyKey.Count, "precondition: one entry per authored block");

            var before = BlockRefRule.JumpTargets(built.Entries, _ => false);
            CollectionAssert.AreEquivalent(everyKey.Except(new[] { Header, HeroChip, StickyFooter, WaitsForReading }), before,
                "the scrolling, shown blocks -- the Sources blocks included: a jump can reach a block no navigation element lists");

            var after = BlockRefRule.JumpTargets(built.Entries, key => key == WaitsForReading);
            CollectionAssert.AreEquivalent(before.Append(WaitsForReading), after, "once revealed, the question is a target too, and only it");
            CollectionAssert.IsEmpty(BlockRefRule.JumpTargets(null, _ => true), "no stack, no target");
        }

        [Test]
        public void Check_NamesWhyAPickLeadsNowhere_ForEachKindOfPick()
        {
            var config = Shipped();
            var poi = LampOf(config);
            // - a block the card leaves out: a Rich Text with no words (its required Text is empty)
            poi.card.blocks.Add(new BlockInstanceData { key = "block_63", kind = BuiltInBlocks.RichTextKind, variant = "", display = CardOptions.DisplayInline });
            var built = Built(config);
            var holder = Block(config, "block_18");
            System.Func<string, bool> unread = _ => false;

            Assert.AreEqual(BlockRefRule.Problem.None, BlockRefRule.Check("block_55", holder, poi, built, unread), "the Listen player: in the scroll");
            Assert.AreEqual(BlockRefRule.Problem.None, BlockRefRule.Check("block_15", holder, poi, built, unread), "a Sources block: in the scroll");
            Assert.AreEqual(BlockRefRule.Problem.Blank, BlockRefRule.Check("", holder, poi, built, unread));
            Assert.AreEqual(BlockRefRule.Problem.Blank, BlockRefRule.Check("  ", holder, poi, built, unread));
            Assert.AreEqual(BlockRefRule.Problem.Itself, BlockRefRule.Check("block_18", holder, poi, built, unread));
            Assert.AreEqual(BlockRefRule.Problem.Missing, BlockRefRule.Check("block_999", holder, poi, built, unread), "deleted");
            Assert.AreEqual(BlockRefRule.Problem.NotShown, BlockRefRule.Check("block_63", holder, poi, built, unread), "left out by the card");
            Assert.AreEqual(BlockStackBuilder.SkipReason.MissingRequired, built.Skipped.Single(s => s.Instance.key == "block_63").Reason, "precondition");
            foreach (string outside in new[] { Header, HeroChip, StickyFooter })
                Assert.AreEqual(BlockRefRule.Problem.NotInScroll, BlockRefRule.Check(outside, holder, poi, built, unread), outside);
            Assert.AreEqual(BlockRefRule.Problem.WaitsForReading, BlockRefRule.Check(WaitsForReading, holder, poi, built, unread));
            Assert.AreEqual(BlockRefRule.Problem.None, BlockRefRule.Check(WaitsForReading, holder, poi, built, key => key == WaitsForReading), "revealed");
        }

        [Test]
        public void ABlockRefField_ReadsAsContentOnlyWhenSomethingIsPicked()
        {
            Assert.IsFalse(BlockFieldReader.HasContent(new BlockFieldValue { key = "t", value = " " }, BlockFieldType.BlockRef));
            Assert.IsTrue(BlockFieldReader.HasContent(new BlockFieldValue { key = "t", value = "block_3" }, BlockFieldType.BlockRef));
            var sub = new BlockFieldDefinition { Key = "t", Type = BlockFieldType.BlockRef, Required = true };
            Assert.IsFalse(BlockFieldReader.ItemIsComplete(new BlockItemData { fields = new List<BlockItemFieldValue> { new() { key = "t", value = "" } } }, new[] { sub }),
                "a required BlockRef left blank makes its row incomplete");
            Assert.IsTrue(BlockFieldReader.ItemIsComplete(new BlockItemData { fields = new List<BlockItemFieldValue> { new() { key = "t", value = "block_3" } } }, new[] { sub }));
        }

        // ---------------- the definition (BlockRegistry.Validate) ----------------

        private static BlockKindDefinition KindWith(params BlockFieldDefinition[] fields) => new()
        {
            Key = "k", Family = BuiltInFamilies.About, Format = CardOptions.FormatRead, DisplayName = "K",
            Variants = new[] { "v" }, DefaultVariant = "v", DisplayModes = new[] { CardOptions.DisplayInline }, Fields = fields,
        };

        [Test]
        public void Validate_AcceptsAPlainBlockRef_AtTheTopAndInARow_AndRefusesOneWithAFixedListADefaultOrALibraryDefault()
        {
            Assert.IsNull(BlockRegistry.Validate(KindWith(new BlockFieldDefinition { Key = "target", Type = BlockFieldType.BlockRef })));
            Assert.IsNull(BlockRegistry.Validate(KindWith(new BlockFieldDefinition
            {
                Key = "rows", Type = BlockFieldType.Items,
                ItemFields = new[]
                {
                    new BlockFieldDefinition { Key = "go", Type = BlockFieldType.Choice, Options = new[] { "a", "b" } },
                    new BlockFieldDefinition { Key = "target", Type = BlockFieldType.BlockRef, ShownWhen = FieldShownWhen.Choice("go", "b") },
                },
            })), "a row's BlockRef shown for one option of its row");

            StringAssert.Contains("BlockRef field 'target' lists options", BlockRegistry.Validate(KindWith(
                new BlockFieldDefinition { Key = "target", Type = BlockFieldType.BlockRef, Options = new[] { "block_1" } })));
            StringAssert.Contains("lists options", BlockRegistry.Validate(KindWith(
                new BlockFieldDefinition { Key = "target", Type = BlockFieldType.BlockRef, OptionLabels = new[] { "The story" } })));
            StringAssert.Contains("lists options", BlockRegistry.Validate(KindWith(new BlockFieldDefinition
            {
                Key = "rows", Type = BlockFieldType.Items,
                ItemFields = new[] { new BlockFieldDefinition { Key = "target", Type = BlockFieldType.BlockRef, Options = new[] { "block_1" } } },
            })), "inside a row too");
            StringAssert.Contains("not one of its Choice options", BlockRegistry.Validate(KindWith(
                new BlockFieldDefinition { Key = "target", Type = BlockFieldType.BlockRef, ChoiceDefault = "block_1" })));
            StringAssert.Contains("Block Library default", BlockRegistry.Validate(KindWith(
                new BlockFieldDefinition { Key = "target", Type = BlockFieldType.BlockRef, LibraryDefault = true })));
        }

        // BlankIsFine (step 10: a hotspot spot's optional Opens Block) belongs to a BlockRef, top level or in a row, and to nothing else
        [Test]
        public void Validate_AcceptsBlankIsFineOnABlockRef_AndRefusesItOnAnyOtherType()
        {
            Assert.IsNull(BlockRegistry.Validate(KindWith(new BlockFieldDefinition { Key = "target", Type = BlockFieldType.BlockRef, BlankIsFine = true })));
            Assert.IsNull(BlockRegistry.Validate(KindWith(new BlockFieldDefinition
            {
                Key = "rows", Type = BlockFieldType.Items,
                ItemFields = new[] { new BlockFieldDefinition { Key = "target", Type = BlockFieldType.BlockRef, BlankIsFine = true } },
            })), "inside a row, where a hotspot spot has it");
            StringAssert.Contains("a blank pick is fine", BlockRegistry.Validate(KindWith(
                new BlockFieldDefinition { Key = "words", Type = BlockFieldType.LocalizedText, BlankIsFine = true })));
            StringAssert.Contains("a blank pick is fine", BlockRegistry.Validate(KindWith(
                new BlockFieldDefinition { Key = "who", Type = BlockFieldType.PoiRef, BlankIsFine = true })), "a PoiRef is not a BlockRef");
            var opens = BuiltInBlocks.HotspotImage.Field(BuiltInBlocks.HotspotItemsField).ItemFields.Single(f => f.Key == BuiltInBlocks.HotspotOpensBlockField);
            Assert.AreEqual(BlockFieldType.BlockRef, opens.Type, "the spot's jump reuses the BlockRef type and its drawer");
            Assert.IsTrue(opens.BlankIsFine, "optional: a spot with nothing picked is the normal spot");
            Assert.IsFalse(opens.Required, "and never required: a spot with no pick is still shown");
            Assert.AreEqual("Opens Block", opens.Label, "the same Editor words as an Open Block action's field");
        }

        [Test]
        public void EveryRegisteredKind_StillPassesValidate()
        {
            foreach (var kind in BlockRegistry.Shared.All) Assert.IsNull(BlockRegistry.Validate(kind), kind.Key);
        }
    }
}
