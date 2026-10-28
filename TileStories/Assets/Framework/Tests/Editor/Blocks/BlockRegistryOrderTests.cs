using System.Linq;
using NUnit.Framework;

namespace TileStories.Editor.Tests
{
    // The order a person sees kinds in (_3.1 step 11-fix, re-grounded by _3.2 step 1): grouped by family, the families by their
    // REGISTERED Order (no longer by which kind happened to be registered first), the kinds of a family in registration order.
    // All keeps plain registration order (the catalog's own order).
    public class BlockRegistryOrderTests
    {
        private static BlockKindDefinition Kind(string key, string family) => new()
        {
            Key = key, Family = family, Format = CardOptions.FormatRead, IconKey = CardIcons.Info, DisplayName = key, Variants = new[] { "one" }, DefaultVariant = "one",
            DisplayModes = new[] { CardOptions.DisplayInline }, Fields = new BlockFieldDefinition[0],
        };

        // Registers each named family once, in the order given, ten apart -- then the kinds
        private static BlockRegistry Registry(params (string Key, string Family)[] kinds)
        {
            var registry = new BlockRegistry();
            foreach (var (_, family) in kinds)
                if (!registry.TryGetFamily(family, out _))
                    registry.RegisterFamily(new FamilyDefinition { Key = family, Order = (registry.Families.Count + 1) * 10, LabelKey = "f_" + family });
            foreach (var (key, family) in kinds) registry.Register(Kind(key, family), () => null);
            return registry;
        }

        [Test]
        public void Ordered_GroupsByFamily_FamiliesByTheirRegisteredOrder_KindsInRegistrationOrder()
        {
            var registry = Registry(("a1", "alpha"), ("b1", "beta"), ("a2", "alpha"), ("c1", "gamma"), ("b2", "beta"), ("a3", "alpha"));
            CollectionAssert.AreEqual(new[] { "a1", "b1", "a2", "c1", "b2", "a3" }, registry.All.Select(k => k.Key), "All stays in registration order");
            CollectionAssert.AreEqual(new[] { "a1", "a2", "a3", "b1", "b2", "c1" }, registry.Ordered.Select(k => k.Key));
        }

        // The point of families as data: Ordered follows the family's Order, NOT the order its first kind was registered
        [Test]
        public void Ordered_FollowsTheFamilysOwnOrder_NotTheOrderItsFirstKindWasRegistered()
        {
            var registry = new BlockRegistry();
            registry.RegisterFamily(new FamilyDefinition { Key = "late", Order = 90, LabelKey = "f_late" });
            registry.RegisterFamily(new FamilyDefinition { Key = "early", Order = 10, LabelKey = "f_early" });
            registry.Register(Kind("l1", "late"), () => null);     // registered FIRST
            registry.Register(Kind("e1", "early"), () => null);
            CollectionAssert.AreEqual(new[] { "l1", "e1" }, registry.All.Select(k => k.Key), "All is still registration order");
            CollectionAssert.AreEqual(new[] { "e1", "l1" }, registry.Ordered.Select(k => k.Key), "Ordered puts early (10) before late (90)");
        }

        // An app slots its own family into a gap of ten and really lands between two built-in ones (_3.2 2.2)
        [Test]
        public void Ordered_AnAppFamilyInAGap_LandsBetweenTheTwoBuiltInFamiliesItSitsBetween()
        {
            var registry = new BlockRegistry();
            BuiltInFamilies.Register(registry);
            registry.RegisterFamily(new FamilyDefinition { Key = "wall_tour", Order = 45, LabelKey = "f_wall_tour" });
            registry.Register(Kind("guide_clip", BuiltInFamilies.Guides), () => null);     // guides = 40
            registry.Register(Kind("chapter", BuiltInFamilies.Play), () => null);          // play = 60
            registry.Register(Kind("tour_stop", "wall_tour"), () => null);                 // app    = 45
            CollectionAssert.AreEqual(new[] { "guide_clip", "tour_stop", "chapter" }, registry.Ordered.Select(k => k.Key),
                "Order 45 lands between guides (40) and play (60), however late it was registered");
        }

        [Test]
        public void Ordered_APlainRegistryWithOneFamilyOrNone_IsUnchanged()
        {
            Assert.IsEmpty(new BlockRegistry().Ordered);
            var one = Registry(("x", "same"), ("y", "same"), ("z", "same"));
            CollectionAssert.AreEqual(one.All.Select(k => k.Key), one.Ordered.Select(k => k.Key));
        }

        [Test]
        public void Ordered_ANewKindOfAKnownFamilyJoinsThatFamily_AFamilyRegisteredLastWithAHigherOrderGoesLast()
        {
            var registry = Registry(("a1", "alpha"), ("b1", "beta"));
            registry.RegisterFamily(new FamilyDefinition { Key = "zeta", Order = 30, LabelKey = "f_zeta" });
            registry.Register(Kind("a2", "alpha"), () => null);
            registry.Register(Kind("z1", "zeta"), () => null);
            CollectionAssert.AreEqual(new[] { "a1", "a2", "b1", "z1" }, registry.Ordered.Select(k => k.Key));
        }

        [Test]
        public void TheBuiltInKinds_EveryFamilyIsOneUnbrokenGroup_InTheOrderedList()
        {
            var registry = new BlockRegistry();
            BuiltInBlocks.Register(registry);
            CollectionAssert.AreEqual(new[] { "about", "stories", "media", "ar", "guides", "play", "community", "visit", "navigation", "meta" },
                registry.Ordered.Select(k => k.Family).Distinct(), "the canvas order of _3.2 section 2.2");
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

        // A kind's display name reads in a sentence ("About the 3D Model"): no number glued to a lowercase unit like "3d" (_3.1 10B-pre.2);
        // the keys stay what saved configs hold
        [Test]
        public void TheBuiltInKinds_DisplayNamesHaveNoLowercaseUnit_AndThe3DAnd360KindsReadNumberFirst()
        {
            var registry = new BlockRegistry();
            BuiltInBlocks.Register(registry);
            var lowercaseUnit = new System.Text.RegularExpressions.Regex(@"\d+[a-z]");
            foreach (var kind in registry.All)
                Assert.IsFalse(lowercaseUnit.IsMatch(kind.DisplayName), kind.Key + ": display name '" + kind.DisplayName + "' has a lowercase unit");
            Assert.IsTrue(registry.TryGet("model_3d", out var model) && model.DisplayName == "3D Model", "model_3d keeps its key and reads '3D Model'");
            Assert.IsTrue(registry.TryGet("panorama_360", out var panorama) && panorama.DisplayName == "360 Panorama", "panorama_360 keeps its key and reads '360 Panorama'");
        }
    }
}
