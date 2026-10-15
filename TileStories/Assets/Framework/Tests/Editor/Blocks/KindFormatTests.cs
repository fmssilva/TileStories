using System.Linq;
using NUnit.Framework;
using UnityEngine.UIElements;

namespace TileStories.Editor.Tests
{
    // The Format facet (_3.2 step 2, section 2.4): every kind says HOW a visitor takes it in -- one of a FIXED list of six,
    // because formats are universal while families are a wall's vocabulary. What this fixture pins: every kind the framework
    // AND the app register declares a valid one, the kinds the guide names carry the format it names, and the static Validate
    // (a shape check, it needs no registry) refuses a missing or unknown format so a typo can never reach the navigation.
    public class KindFormatTests
    {
        private sealed class PlainView : IBlockView
        {
            public VisualElement Root { get; } = new();
            public void Bind(BlockInstanceData instance, BlockBindContext context) { }
            public void Unbind() { }
        }

        private static BlockKindDefinition Kind(string format) => new()
        {
            Key = "probe", Family = BuiltInFamilies.About, Format = format, DisplayName = "Probe", Variants = new[] { "plain" },
            DefaultVariant = "plain", DisplayModes = new[] { CardOptions.DisplayInline },
        };

        [Test]
        public void EveryRegisteredKind_TheFrameworksAndTheApps_DeclaresOneOfTheSixFormats()
        {
            var shared = BlockRegistry.Shared;
            Assert.IsTrue(shared.TryGet("size_comparison", out _), "precondition: the LivingRoom app's kind is registered in Edit Mode, so the app is covered too");
            Assert.GreaterOrEqual(shared.All.Count, 34, "precondition: the 33 framework kinds and the app's");
            foreach (var kind in shared.All)
                CollectionAssert.Contains(CardOptions.Formats, kind.Format, kind.Key + " declares a format of the fixed list");
        }

        // The ones _3.2 2.4 names itself; the others are decided per kind (the step 2 report) and only need to be valid
        [TestCase(BuiltInBlocks.VideoKind, CardOptions.FormatWatch)]
        [TestCase(BuiltInBlocks.AudioGuideKind, CardOptions.FormatListen)]
        [TestCase(BuiltInBlocks.Model3DKind, CardOptions.FormatModel)]
        [TestCase(BuiltInBlocks.Panorama360Kind, CardOptions.FormatModel)]
        [TestCase(BuiltInBlocks.PlaceInArKind, CardOptions.FormatModel)]
        [TestCase(BuiltInBlocks.RichTextKind, CardOptions.FormatRead)]
        [TestCase(BuiltInBlocks.GalleryKind, CardOptions.FormatLook)]
        [TestCase(BuiltInBlocks.KnowledgeCheckKind, CardOptions.FormatInteract)]
        public void TheKindsTheGuideNames_CarryTheFormatItNames(string kind, string format)
        {
            Assert.IsTrue(BlockRegistry.Shared.TryGet(kind, out var definition), kind + " is registered");
            Assert.AreEqual(format, definition.Format);
        }

        // A format no kind uses would be a dead label in every format-grouped navigation
        [Test]
        public void EveryFormatOfTheList_IsUsedByAtLeastOneFrameworkKind()
        {
            var registry = new BlockRegistry();
            BuiltInBlocks.Register(registry);
            foreach (string format in CardOptions.Formats)
                Assert.IsTrue(registry.All.Any(k => k.Format == format), "some framework kind is '" + format + "'");
        }

        [Test]
        public void Validate_RefusesAMissingFormat()
        {
            foreach (string missing in new[] { null, "", "  " })
                StringAssert.Contains("no format", BlockRegistry.Validate(Kind(missing)), "'" + missing + "' is no format");
        }

        [Test]
        public void Validate_RefusesAnUnknownFormat_NamingItAndTheList_AndRegisterRefusesTheKind()
        {
            string problem = BlockRegistry.Validate(Kind("video"));
            StringAssert.Contains("'video'", problem, "names the typo (video is a kind, watch is the format)");
            StringAssert.Contains(string.Join(", ", CardOptions.Formats), problem, "and says what it may be");

            var registry = new BlockRegistry();
            registry.RegisterFamily(new FamilyDefinition { Key = BuiltInFamilies.About, Order = 10, LabelKey = CardStrings.Keys.FamilyAbout });
            Assert.Throws<System.ArgumentException>(() => registry.Register(Kind("Read"), () => new PlainView()), "formats are matched exactly");
            CollectionAssert.IsEmpty(registry.All, "the refused kind left nothing behind");

            Assert.IsNull(BlockRegistry.Validate(Kind(CardOptions.FormatRead)), "the same kind with a real format is a valid shape");
        }
    }
}
