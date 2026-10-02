using System.Collections.Generic;
using NUnit.Framework;

namespace TileStories.Editor.Tests
{
    // A Choice field's default (_3.1 10B-pre.1): the block's own value, else the wall's Block Library default for a LibraryDefault
    // field, else the field's own ChoiceDefault -- one read (BlockLibraryRule.Choice) shared by the card views and the Editor
    public class BlockLibraryDefaultsTests
    {
        private static CardSettings WallWith(string kind, string key, string value) => new()
        {
            kinds = new List<BlockKindSetting> { new() { kind = kind, field_defaults = new List<BlockFieldDefault> { new() { key = key, value = value } } } },
        };

        private static BlockFieldReader ReaderOf(string key, string value)
        {
            var block = new BlockInstanceData { key = "b", kind = BuiltInBlocks.Model3DKind };
            if (value != null) block.fields.Add(new BlockFieldValue { key = key, value = value });
            return new BlockFieldReader(block, "en", "en");
        }

        private static readonly BlockFieldDefinition Fit = BuiltInBlocks.Model3D.Field(BuiltInBlocks.Model3DFitField);

        [Test]
        public void ModelFit_ReadsTheBlocksOwnValue_ThenTheLibrarysDefault_ThenYawSafe()
        {
            var wall = WallWith(BuiltInBlocks.Model3DKind, BuiltInBlocks.Model3DFitField, ModelFitRule.FitAtRest);
            Assert.AreEqual(ModelFitRule.FitSphere, BlockLibraryRule.Choice(wall, BuiltInBlocks.Model3D, Fit, ReaderOf(Fit.Key, ModelFitRule.FitSphere)), "the block's own value wins");
            Assert.AreEqual(ModelFitRule.FitAtRest, BlockLibraryRule.Choice(wall, BuiltInBlocks.Model3D, Fit, ReaderOf(Fit.Key, null)), "no value: the wall's Block Library default");
            Assert.AreEqual(ModelFitRule.FitYawSafe, BlockLibraryRule.Choice(new CardSettings(), BuiltInBlocks.Model3D, Fit, ReaderOf(Fit.Key, null)), "no Library row: the field's own default");
            Assert.AreEqual(ModelFitRule.FitYawSafe, BlockLibraryRule.Choice(null, BuiltInBlocks.Model3D, Fit, ReaderOf(Fit.Key, null)), "no card settings at all (the gallery)");
        }

        [Test]
        public void AStaleValue_IsIgnored_AtEveryLayer_AndAnotherKindsDefault_NeverReachesThisOne()
        {
            var staleLibrary = WallWith(BuiltInBlocks.Model3DKind, BuiltInBlocks.Model3DFitField, "box");
            Assert.AreEqual(ModelFitRule.FitYawSafe, BlockLibraryRule.Choice(staleLibrary, BuiltInBlocks.Model3D, Fit, ReaderOf(Fit.Key, null)), "a Library value that is no option falls through");
            var library = WallWith(BuiltInBlocks.Model3DKind, BuiltInBlocks.Model3DFitField, ModelFitRule.FitSphere);
            Assert.AreEqual(ModelFitRule.FitSphere, BlockLibraryRule.Choice(library, BuiltInBlocks.Model3D, Fit, ReaderOf(Fit.Key, "box")), "a block value that is no option falls through");

            var headerFit = BuiltInBlocks.Header.Field(BuiltInBlocks.HeaderModelFitField);
            Assert.AreEqual(ModelFitRule.FitYawSafe, BlockLibraryRule.Choice(library, BuiltInBlocks.Header, headerFit, ReaderOf(headerFit.Key, null)),
                "model_3d's Library default is model_3d's: the header's model keeps its own");
        }

        [Test]
        public void TheRegistry_RefusesADefaultThatIsNoOption_AndALibraryDefaultOnAFieldThatCannotHaveOne()
        {
            BlockKindDefinition KindWith(BlockFieldDefinition field) => new()
            {
                Key = "k", Family = "about", DisplayName = "K", Variants = new[] { "v" }, DefaultVariant = "v",
                DisplayModes = new[] { CardOptions.DisplayInline }, Fields = new[] { field },
            };
            StringAssert.Contains("not one of its Choice options", BlockRegistry.Validate(KindWith(new BlockFieldDefinition
                { Key = "c", Type = BlockFieldType.Choice, Options = new[] { "a", "b" }, ChoiceDefault = "z" })));
            StringAssert.Contains("Block Library default", BlockRegistry.Validate(KindWith(new BlockFieldDefinition
                { Key = "c", Type = BlockFieldType.Choice, Options = new[] { "a" }, LibraryDefault = true })), "a Library default needs a field default to fall back to");
            StringAssert.Contains("not one of its Choice options", BlockRegistry.Validate(KindWith(new BlockFieldDefinition
                { Key = "t", Type = BlockFieldType.Toggle, ChoiceDefault = "a" })));
            Assert.IsNull(BlockRegistry.Validate(BuiltInBlocks.Model3D), "the model kind's Fit is a valid Library-default Choice");
            Assert.IsNull(BlockRegistry.Validate(BuiltInBlocks.Header));

            // - a Toggle may take a Library default (off unless the wall says otherwise), but never inside an Items row
            Assert.IsNull(BlockRegistry.Validate(KindWith(new BlockFieldDefinition { Key = "t", Type = BlockFieldType.Toggle, LibraryDefault = true })));
            Assert.IsNull(BlockRegistry.Validate(BuiltInBlocks.PlaceInAr), "Keep Model On Switch is a valid Library-default Toggle");
            StringAssert.Contains("top-level", BlockRegistry.Validate(KindWith(new BlockFieldDefinition
            {
                Key = "rows", Type = BlockFieldType.Items,
                ItemFields = new[] { new BlockFieldDefinition { Key = "t", Type = BlockFieldType.Toggle, LibraryDefault = true } },
            })), "a row's sub-field cannot take a wall-wide default");
            StringAssert.Contains("Block Library default", BlockRegistry.Validate(KindWith(new BlockFieldDefinition { Key = "n", Type = BlockFieldType.Number, LibraryDefault = true })));
        }

        private static readonly BlockFieldDefinition KeepOnSwitch = BuiltInBlocks.PlaceInAr.Field(BuiltInBlocks.PlaceInArKeepOnSwitchField);

        private static BlockFieldReader ToggleOf(bool? stored)
        {
            var block = new BlockInstanceData { key = "b", kind = BuiltInBlocks.PlaceInArKind };
            if (stored.HasValue) block.fields.Add(new BlockFieldValue { key = KeepOnSwitch.Key, flag = stored.Value });
            return new BlockFieldReader(block, "en", "en");
        }

        [Test]
        public void KeepModelOnSwitch_ReadsTheBlocksOwnTick_ThenTheLibrarysDefault_ThenOff_ForBothValuesAtEveryLayer()
        {
            var kind = BuiltInBlocks.PlaceInAr;
            Assert.IsTrue(KeepOnSwitch.LibraryDefault, "precondition: the field has a Block Library row");
            var wallOn = WallWith(BuiltInBlocks.PlaceInArKind, KeepOnSwitch.Key, BlockLibraryRule.FlagTrue);
            var wallOff = WallWith(BuiltInBlocks.PlaceInArKind, KeepOnSwitch.Key, BlockLibraryRule.FlagFalse);
            // - nothing stored by the block: the wall's default; no Library row at all (or no settings): off
            Assert.IsTrue(BlockLibraryRule.Flag(wallOn, kind, KeepOnSwitch, ToggleOf(null)), "the Library says on");
            Assert.IsFalse(BlockLibraryRule.Flag(wallOff, kind, KeepOnSwitch, ToggleOf(null)), "the Library says off");
            Assert.IsFalse(BlockLibraryRule.Flag(new CardSettings(), kind, KeepOnSwitch, ToggleOf(null)), "no Library row: off, the default");
            Assert.IsFalse(BlockLibraryRule.Flag(null, kind, KeepOnSwitch, ToggleOf(null)), "no card settings at all (the gallery)");
            // - the block's own tick wins in both directions, an unticked box too
            Assert.IsTrue(BlockLibraryRule.Flag(wallOff, kind, KeepOnSwitch, ToggleOf(true)), "ticked on the block, off in the Library: on");
            Assert.IsFalse(BlockLibraryRule.Flag(wallOn, kind, KeepOnSwitch, ToggleOf(false)), "unticked on the block, on in the Library: off");
            // - a stale or foreign Library value reads as off; another kind's row never reaches this one
            Assert.IsFalse(BlockLibraryRule.Flag(WallWith(BuiltInBlocks.PlaceInArKind, KeepOnSwitch.Key, "maybe"), kind, KeepOnSwitch, ToggleOf(null)), "an entry that is no true / false");
            Assert.IsFalse(BlockLibraryRule.Flag(WallWith(BuiltInBlocks.Model3DKind, KeepOnSwitch.Key, BlockLibraryRule.FlagTrue), kind, KeepOnSwitch, ToggleOf(null)), "another kind's default");
            Assert.IsFalse(BlockLibraryRule.FlagDefault(wallOn, kind, BuiltInBlocks.PlaceInAr.Field(BuiltInBlocks.PlaceInArLabelField)), "a field without a Library default has none");
        }
    }
}
