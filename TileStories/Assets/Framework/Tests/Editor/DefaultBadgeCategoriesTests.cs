// DefaultBadgeCategoriesTests.cs
//
// EditMode test for the building damage levels seeded by the authoring tool
// when a wall has no badge_categories. Pure data -- no scene, no window, no
// Unity dependencies. Verifies the four damage levels, their icon keys, and
// their hex colours match the spec in _5.3_Default_Icons.md.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace TileStories.Editor.Tests
{
    public class DefaultBadgeCategoriesTests
    {
        [Test]
        public void Create_ReturnsFourEntries()
        {
            List<BadgeCategoryEntry> defaults = DefaultBadgeCategories.Create();
            Assert.AreEqual(4, defaults.Count,
                "Should seed exactly four building damage level defaults.");
        }

        [Test]
        public void Create_EveryEntry_HasRequiredFields()
        {
            List<BadgeCategoryEntry> defaults = DefaultBadgeCategories.Create();

            // Naive validation that avoids the brittle n-detail approach -- every
            // entry must carry all four fields the runtime actually consumes.
            foreach (var entry in defaults)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.key),
                    "Every default entry must have a key.");
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.label),
                    "Every default entry must have a label.");
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.icon_key),
                    "Every default entry must have an icon_key.");
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.color_hex),
                    "Every default entry must have a color_hex.");
            }
        }

        [Test]
        public void Create_DamageLevels_MatchSpec()
        {
            List<BadgeCategoryEntry> defaults = DefaultBadgeCategories.Create();

            var byKey = defaults.ToDictionary(e => e.key);

            Assert.IsTrue(byKey.ContainsKey("intact"));
            Assert.AreEqual("IconIntact", byKey["intact"].icon_key);
            Assert.AreEqual("#22C55E", byKey["intact"].color_hex);

            Assert.IsTrue(byKey.ContainsKey("partial_damage"));
            Assert.AreEqual("IconPartialDamage", byKey["partial_damage"].icon_key);
            Assert.AreEqual("#F97316", byKey["partial_damage"].color_hex);

            Assert.IsTrue(byKey.ContainsKey("destroyed"));
            Assert.AreEqual("IconDestroyed", byKey["destroyed"].icon_key);
            Assert.AreEqual("#991B1B", byKey["destroyed"].color_hex);

            Assert.IsTrue(byKey.ContainsKey("unknown_damage"));
            Assert.AreEqual("IconUnknownDamage", byKey["unknown_damage"].icon_key);
            Assert.AreEqual("#71717A", byKey["unknown_damage"].color_hex);
        }

        [Test]
        public void Create_ReturnsFreshList_EachCall()
        {
            // Each call must return a new list so callers (both LoadConfig and the
            // OnGUI path) can mutate it without leaking state into each other.
            var first = DefaultBadgeCategories.Create();
            var second = DefaultBadgeCategories.Create();

            Assert.AreNotSame(first, second, "Create() must return a fresh list each call.");
            Assert.AreEqual(first.Count, second.Count);
        }
    }
}
