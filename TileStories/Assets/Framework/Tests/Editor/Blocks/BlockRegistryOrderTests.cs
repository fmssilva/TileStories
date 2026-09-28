using System.Linq;
using NUnit.Framework;

namespace TileStories.Editor.Tests
{
    // The order a person sees kinds in (_3.1 step 11-fix): grouped by family, the families in the order each first appeared, the kinds of a
    // family in registration order. All keeps plain registration order (the catalog's own order).
    public class BlockRegistryOrderTests
    {
        private static BlockKindDefinition Kind(string key, string family) => new()
        {
            Key = key, Family = family, DisplayName = key, Variants = new[] { "one" }, DefaultVariant = "one",
            DisplayModes = new[] { CardOptions.DisplayInline }, Fields = new BlockFieldDefinition[0],
        };

        private static BlockRegistry Registry(params (string Key, string Family)[] kinds)
        {
            var registry = new BlockRegistry();
            foreach (var (key, family) in kinds) registry.Register(Kind(key, family), () => null);
            return registry;
        }

        [Test]
        public void Ordered_GroupsByFamily_FamiliesInFirstAppearanceOrder_KindsInRegistrationOrder()
        {
            var registry = Registry(("a1", "alpha"), ("b1", "beta"), ("a2", "alpha"), ("c1", "gamma"), ("b2", "beta"), ("a3", "alpha"));
            CollectionAssert.AreEqual(new[] { "a1", "b1", "a2", "c1", "b2", "a3" }, registry.All.Select(k => k.Key), "All stays in registration order");
            CollectionAssert.AreEqual(new[] { "a1", "a2", "a3", "b1", "b2", "c1" }, registry.Ordered.Select(k => k.Key));
        }

        [Test]
        public void Ordered_APlainRegistryWithOneFamilyOrNone_IsUnchanged()
        {
            Assert.IsEmpty(new BlockRegistry().Ordered);
            var one = Registry(("x", "same"), ("y", "same"), ("z", "same"));
            CollectionAssert.AreEqual(one.All.Select(k => k.Key), one.Ordered.Select(k => k.Key));
        }

        [Test]
        public void Ordered_ANewKindOfAKnownFamilyJoinsThatFamily_ANewFamilyGoesLast()
        {
            var registry = Registry(("a1", "alpha"), ("b1", "beta"));
            registry.Register(Kind("a2", "alpha"), () => null);
            registry.Register(Kind("z1", "zeta"), () => null);
            CollectionAssert.AreEqual(new[] { "a1", "a2", "b1", "z1" }, registry.Ordered.Select(k => k.Key));
        }

        [Test]
        public void TheBuiltInKinds_EveryFamilyIsOneUnbrokenGroup_InTheOrderedList()
        {
            var registry = new BlockRegistry();
            BuiltInBlocks.Register(registry);
            var families = registry.Ordered.Select(k => k.Family).ToList();
            var seen = new System.Collections.Generic.List<string>();
            foreach (string family in families)
            {
                if (seen.Count == 0 || seen[seen.Count - 1] != family)
                {
                    Assert.IsFalse(seen.Contains(family), "family '" + family + "' comes back after another family: the groups are not unbroken");
                    seen.Add(family);
                }
            }
            CollectionAssert.AreEquivalent(registry.All.Select(k => k.Key), registry.Ordered.Select(k => k.Key), "only regrouped, nothing lost");
        }
    }
}
