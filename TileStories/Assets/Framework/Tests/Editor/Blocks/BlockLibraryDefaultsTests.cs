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
        }
    }
}
