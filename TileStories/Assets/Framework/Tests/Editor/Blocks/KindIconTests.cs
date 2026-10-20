using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace TileStories.Editor.Tests
{
    // The kinds' icons (_3.2 step 17a, BlockKindDefinition.IconKey): what a sibling chip of the full view shows, so a visitor tells a 360
    // panorama from a gallery without opening either. Every kind has one the card's icon set draws; the registry refuses a kind without
    // one; and the kinds that can be siblings of one section never share an icon -- the whole point of a chip is that two of them differ.
    public class KindIconTests
    {
        [Test]
        public void EveryRegisteredKind_HasAnIconTheCardsIconSetDraws()
        {
            Assert.Greater(BlockRegistry.Shared.All.Count, 30, "precondition: the framework's kinds (and the app's) are registered");
            foreach (var kind in BlockRegistry.Shared.All)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(kind.IconKey), kind.Key + " has an icon");
                CollectionAssert.Contains(CardIcons.All, kind.IconKey, kind.Key + ": '" + kind.IconKey + "' is one the set draws");
            }
        }

        [Test]
        public void TheRegistry_RefusesAKindWithNoIcon_OrAnIconTheSetDoesNotHave()
        {
            BlockKindDefinition Kind(string icon) => new()
            {
                Key = "icon_probe", Family = BuiltInFamilies.About, Format = CardOptions.FormatRead, IconKey = icon, DisplayName = "Probe",
                Variants = new[] { "plain" }, DefaultVariant = "plain", DisplayModes = new[] { CardOptions.DisplayInline },
            };
            Assert.IsNull(BlockRegistry.Validate(Kind(CardIcons.Info)), "a kind with a real icon registers");
            Assert.AreEqual("no icon", BlockRegistry.Validate(Kind(null)));
            Assert.AreEqual("no icon", BlockRegistry.Validate(Kind("  ")));
            StringAssert.Contains("not one of the card's icons", BlockRegistry.Validate(Kind("sparkles")), "a typo is named, not drawn as an empty chip");
        }

        // The kinds a block can open the full view as, in a family the card's map lists: their chips share a row, so their icons differ
        [Test]
        public void TheKindsOfOneSection_NeverShareAnIcon()
        {
            var shared = new List<string>();
            foreach (var family in BlockRegistry.Shared.Families.Where(f => f.InNav))
            {
                var chipKinds = BlockRegistry.Shared.All
                    .Where(k => k.Family == family.Key && FullViewSiblingRule.SupportsTheFullView(k.Key, BlockRegistry.Shared) && k.Key != BuiltInBlocks.HeaderKind)
                    .ToList();
                foreach (var group in chipKinds.GroupBy(k => k.IconKey).Where(g => g.Count() > 1))
                    shared.Add(family.Key + ": " + string.Join(" and ", group.Select(k => k.Key)) + " both use '" + group.Key + "'");
            }
            Assert.IsEmpty(shared, "two chips of one section with one icon cannot be told apart: " + string.Join("; ", shared));
        }

        [Test]
        public void TheMediaFamily_TellsEveryKindApart_TheGalleryTheVideoThe3DModelAndThe360Panorama()
        {
            string Icon(string kind)
            {
                Assert.IsTrue(BlockRegistry.Shared.TryGet(kind, out var definition), kind);
                return definition.IconKey;
            }
            var media = new[]
            {
                BuiltInBlocks.GalleryKind, BuiltInBlocks.BeforeAfterKind, BuiltInBlocks.ZoomImageKind, BuiltInBlocks.HotspotImageKind,
                BuiltInBlocks.VideoKind, BuiltInBlocks.Model3DKind, BuiltInBlocks.Panorama360Kind,
            };
            Assert.AreEqual(media.Length, media.Select(Icon).Distinct().Count(), "seven Media kinds, seven different icons");
            Assert.AreNotEqual(Icon(BuiltInBlocks.Panorama360Kind), Icon(BuiltInBlocks.GalleryKind), "the 360 panorama is not the gallery (the step 16 finding)");
            Assert.AreNotEqual(Icon(BuiltInBlocks.Panorama360Kind), Icon(BuiltInBlocks.Model3DKind));
            Assert.AreEqual(CardIcons.Cube, Icon(BuiltInBlocks.PlaceInArKind), "a model placed in the room wears the model's cube");
        }
    }
}
