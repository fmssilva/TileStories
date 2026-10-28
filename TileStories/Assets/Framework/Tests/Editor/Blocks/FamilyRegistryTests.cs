using System.Linq;
using NUnit.Framework;
using UnityEngine.UIElements;

namespace TileStories.Editor.Tests
{
    // Families as DATA (_3.2 step 1, section 2.2): a family is a registered FamilyDefinition with its own Order and flags,
    // not a string literal a kind happens to spell. What this fixture pins: a family can be registered and read back, a
    // duplicate key is refused, a kind whose family nobody registered is refused WITH the family named, the built-in nine
    // carry the orders and flags the canvas gives them, and every kind the framework ships belongs to a registered family.
    public class FamilyRegistryTests
    {
        private sealed class PlainView : IBlockView
        {
            public VisualElement Root { get; } = new();
            public void Bind(BlockInstanceData instance, BlockBindContext context) { }
            public void Unbind() { }
        }

        private static BlockKindDefinition Kind(string key, string family) => new()
        {
            Key = key, Family = family, Format = CardOptions.FormatRead, IconKey = CardIcons.Info, DisplayName = key, Variants = new[] { "plain" }, DefaultVariant = "plain",
            DisplayModes = new[] { CardOptions.DisplayInline }, Fields = new BlockFieldDefinition[0],
        };

        [Test]
        public void AFamily_IsRegisteredAndReadBack_WithItsOrderLabelAndFlags()
        {
            var registry = new BlockRegistry();
            CollectionAssert.IsEmpty(registry.Families, "a fresh registry knows no family");
            Assert.IsFalse(registry.TryGetFamily("about", out _), "and finds none");

            registry.RegisterFamily(new FamilyDefinition { Key = "about", Order = 10, LabelKey = "family_about" });
            Assert.IsTrue(registry.TryGetFamily("about", out var about));
            Assert.AreEqual(10, about.Order);
            Assert.AreEqual("family_about", about.LabelKey);
            Assert.IsTrue(about.InNav, "a family is a navigation target unless it says otherwise");
            Assert.IsFalse(about.AtEnd, "and does not close the card unless it says otherwise");
            CollectionAssert.AreEqual(new[] { "about" }, registry.Families.Select(f => f.Key));
        }

        [Test]
        public void TheRegistry_RefusesANullFamily_ABlankKey_AndASecondFamilyOfTheSameKey()
        {
            var registry = new BlockRegistry();
            Assert.Throws<System.ArgumentNullException>(() => registry.RegisterFamily(null), "no definition");
            Assert.Throws<System.ArgumentException>(() => registry.RegisterFamily(new FamilyDefinition { Key = "  " }), "no key");

            registry.RegisterFamily(new FamilyDefinition { Key = "about", Order = 10, LabelKey = "family_about" });
            var clash = Assert.Throws<System.ArgumentException>(
                () => registry.RegisterFamily(new FamilyDefinition { Key = "about", Order = 99, LabelKey = "other" }));
            StringAssert.Contains("about", clash.Message, "the message names the family that clashed");
            Assert.AreEqual(1, registry.Families.Count, "the refused one left nothing behind");
            Assert.IsTrue(registry.TryGetFamily("about", out var kept) && kept.Order == 10, "the first registration still stands");
        }

        // The whole point of the check: a typo in a kind's Family used to invent a family in silence
        [Test]
        public void AKindWhoseFamilyIsNotRegistered_IsRefused_WithAMessageThatNamesTheFamily()
        {
            var registry = new BlockRegistry();
            registry.RegisterFamily(new FamilyDefinition { Key = "about", Order = 10, LabelKey = "family_about" });

            var refused = Assert.Throws<System.ArgumentException>(() => registry.Register(Kind("fun_fact", "abuot"), () => new PlainView()));
            StringAssert.Contains("abuot", refused.Message, "the message names the family nobody registered, so the typo is readable");
            StringAssert.Contains("fun_fact", refused.Message, "and the kind that carries it");
            CollectionAssert.IsEmpty(registry.All, "the refused kind was not added");
            Assert.IsNull(registry.CreateView("fun_fact"), "and no view was wired for it");

            registry.Register(Kind("fun_fact", "about"), () => new PlainView());
            Assert.IsTrue(registry.TryGet("fun_fact", out _), "the same kind with the real family is accepted");
        }

        // Validate keeps its exact signature and its SHAPE checks: 8 call sites depend on it, and it knows no registry
        [Test]
        public void Validate_StillOnlyChecksTheShape_AndSaysNothingAboutWhetherTheFamilyIsRegistered()
        {
            Assert.IsNull(BlockRegistry.Validate(Kind("fun_fact", "never_registered_anywhere")), "an unregistered family is not a shape problem");
            StringAssert.Contains("no family", BlockRegistry.Validate(Kind("fun_fact", "")), "a kind with NO family still fails the shape check");
        }

        [Test]
        public void TheBuiltInFamilies_AreTheTenOfTheCanvas_InOrder_OnlyMetaClosesTheCard_AndNavigationIsNoTarget()
        {
            var registry = new BlockRegistry();
            BuiltInFamilies.Register(registry);

            CollectionAssert.AreEqual(
                new[] { "about", "stories", "media", "ar", "guides", "play", "community", "visit", "navigation", "meta" },
                registry.Families.Select(f => f.Key), "the ten families that have kinds today, in canvas order");
            CollectionAssert.AreEqual(new[] { 10, 20, 30, 35, 40, 60, 65, 70, 85, 90 }, registry.Families.Select(f => f.Order),
                "gaps of ten, so an app can slot its own family between any two; navigation (step 11a) at 85, between community and meta");

            CollectionAssert.AreEqual(new[] { "meta" }, registry.Families.Where(f => f.AtEnd).Select(f => f.Key),
                "only the credits close the card");
            CollectionAssert.AreEqual(new[] { "navigation", "meta" }, registry.Families.Where(f => !f.InNav).Select(f => f.Key),
                "the credits and the nav blocks are no navigation target: a nav block never becomes a section of any element");
            foreach (var family in registry.Families)
                Assert.IsTrue(CardStrings.Keys.All.Contains(family.LabelKey), family.Key + " labels itself with a known card text key");
        }

        [Test]
        public void EveryKindTheFrameworkShips_BelongsToARegisteredFamily()
        {
            var registry = new BlockRegistry();
            BuiltInBlocks.Register(registry);
            Assert.Greater(registry.All.Count, 30, "precondition: the kinds were really registered");
            foreach (var kind in registry.All)
                Assert.IsTrue(registry.TryGetFamily(kind.Family, out _), kind.Key + " names the unregistered family '" + kind.Family + "'");
            // - and the app's registry, which adds its own kinds on top of the framework's
            foreach (var kind in BlockRegistry.Shared.All)
                Assert.IsTrue(BlockRegistry.Shared.TryGetFamily(kind.Family, out _), kind.Key + " (shared) names an unregistered family");
        }
    }
}
