// DefaultCategoryStylesTests.cs
//
// EditMode test for the heritage category defaults seeded by the authoring tool
// when a wall has no category_styles. Pure data — no scene, no window, no
// Unity dependencies. Verifies the six heritage categories, their icon keys,
// and their hex colours match the spec in _5.3_Defaults.md.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace TileStories.Editor.Tests
{
    public class DefaultCategoryStylesTests
    {
        [Test]
        public void Create_ReturnsSixEntries()
        {
            List<CategoryStyleEntry> defaults = DefaultCategoryStyles.Create();
            Assert.AreEqual(6, defaults.Count,
                "Should seed exactly six heritage category defaults.");
        }

        [Test]
        public void Create_EveryEntry_HasRequiredFields()
        {
            List<CategoryStyleEntry> defaults = DefaultCategoryStyles.Create();

            foreach (var entry in defaults)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.category),
                    "Every default entry must have a category.");
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.icon_key),
                    "Every default entry must have an icon_key.");
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.color_hex),
                    "Every default entry must have a color_hex.");
            }
        }

        [Test]
        public void Create_HeritageCategories_MatchSpec()
        {
            List<CategoryStyleEntry> defaults = DefaultCategoryStyles.Create();

            // Spot-check each of the six heritage categories against the spec.
            var byCategory = defaults.ToDictionary(e => e.category);

            Assert.IsTrue(byCategory.ContainsKey("royal_government"));
            Assert.AreEqual("IconRoyal&Government", byCategory["royal_government"].icon_key);
            Assert.AreEqual("#D97706", byCategory["royal_government"].color_hex);

            Assert.IsTrue(byCategory.ContainsKey("religious"));
            Assert.AreEqual("IconReligious", byCategory["religious"].icon_key);
            Assert.AreEqual("#7C3AED", byCategory["religious"].color_hex);

            Assert.IsTrue(byCategory.ContainsKey("military"));
            Assert.AreEqual("IconMilitary", byCategory["military"].icon_key);
            Assert.AreEqual("#DC2626", byCategory["military"].color_hex);

            Assert.IsTrue(byCategory.ContainsKey("residential"));
            Assert.AreEqual("IconNobel&PrivateResidence", byCategory["residential"].icon_key);
            Assert.AreEqual("#DB2777", byCategory["residential"].color_hex);

            Assert.IsTrue(byCategory.ContainsKey("economic"));
            Assert.AreEqual("IconIndustry&Trade", byCategory["economic"].icon_key);
            Assert.AreEqual("#059669", byCategory["economic"].color_hex);

            Assert.IsTrue(byCategory.ContainsKey("infrastructure"));
            Assert.AreEqual("IconInfrastructures", byCategory["infrastructure"].icon_key);
            Assert.AreEqual("#0284C7", byCategory["infrastructure"].color_hex);
        }

        [Test]
        public void Create_ReturnsFreshList_EachCall()
        {
            // Each call must return a new list so callers can mutate without
            // leaking state into the next caller.
            var first = DefaultCategoryStyles.Create();
            var second = DefaultCategoryStyles.Create();

            Assert.AreNotSame(first, second, "Create() must return a fresh list each call.");
            Assert.AreEqual(first.Count, second.Count);
        }
    }
}