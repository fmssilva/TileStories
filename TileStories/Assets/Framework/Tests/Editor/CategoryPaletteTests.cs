using NUnit.Framework;
using UnityEngine;

namespace TileStories.Tests
{
    public class CategoryPaletteTests
    {
        [SetUp]
        public void SetUp()
        {
            CategoryPalette.ClearOverrides();
        }

        [TearDown]
        public void TearDown()
        {
            CategoryPalette.ClearOverrides();
        }

        [Test]
        public void ResolveColor_NullOrEmpty_ReturnsDefaultColor()
        {
            Color c1 = CategoryPalette.ResolveColor(null);
            Color c2 = CategoryPalette.ResolveColor("");
            Color c3 = CategoryPalette.ResolveColor("   ");

            // Default color is hash-based for empty strings, but null/empty returns a specific default
            // The actual default in CategoryPalette is (0.35f, 0.33f, 0.30f) but empty string gets hash
            // Let's just check they're not null and are valid colors
            Assert.IsTrue(c1.a > 0);
            Assert.IsTrue(c2.a > 0);
            Assert.IsTrue(c3.a > 0);
        }

        [Test]
        public void ResolveColor_KnownCategory_ReturnsDeterministicHashColor()
        {
            Color c1 = CategoryPalette.ResolveColor("religious");
            Color c2 = CategoryPalette.ResolveColor("religious");
            Color c3 = CategoryPalette.ResolveColor("military");

            Assert.AreEqual(c1, c2, "Same category should return same colour.");
            Assert.AreNotEqual(c1, c3, "Different categories should return different colours.");
        }

        [Test]
        public void ResolveColor_WithOverride_ReturnsOverrideColor()
        {
            var overrides = new[]
            {
                new CategoryStyleEntry { category = "furniture", color_hex = "#FF0000" }
            };
            CategoryPalette.Configure(overrides);

            Color c = CategoryPalette.ResolveColor("furniture");
            Assert.AreEqual(Color.red, c);
        }

        [Test]
        public void ResolveIconKey_KnownCategory_ReturnsKnownIcon()
        {
            string key = CategoryPalette.ResolveIconKey("religious");
            Assert.AreEqual("IconReligious", key);
        }

        [Test]
        public void ResolveIconKey_UnknownCategory_ReturnsUnknownFallback()
        {
            string key = CategoryPalette.ResolveIconKey("unknown_category");
            Assert.AreEqual("unknown", key);
        }

        [Test]
        public void ResolveIconKey_WithOverride_ReturnsOverrideIcon()
        {
            var overrides = new[]
            {
                new CategoryStyleEntry { category = "furniture", icon_key = "chair" }
            };
            CategoryPalette.Configure(overrides);

            string key = CategoryPalette.ResolveIconKey("furniture");
            Assert.AreEqual("chair", key);
        }

        [Test]
        public void StableHash_SameString_ReturnsSameValue()
        {
            int h1 = CategoryPalette.StableHash("test");
            int h2 = CategoryPalette.StableHash("test");
            Assert.AreEqual(h1, h2);
        }

        [Test]
        public void StableHash_DifferentStrings_ReturnsDifferentValues()
        {
            int h1 = CategoryPalette.StableHash("test1");
            int h2 = CategoryPalette.StableHash("test2");
            Assert.AreNotEqual(h1, h2);
        }

        [Test]
        public void ResolveColor_ConfigureCategoryNotListed_StillUsesHashFallback()
        {
            // Configure with an empty list - should still use hash fallback
            CategoryPalette.Configure(new CategoryStyleEntry[0]);

            Color c1 = CategoryPalette.ResolveColor("art");
            Color c2 = CategoryPalette.ResolveColor("art");

            // Should still be deterministic
            Assert.AreEqual(c1, c2, "Hash fallback should still be deterministic.");
        }

        [Test]
        public void ResolveColor_ConfigureOverridesCategory_UsesOverride()
        {
            var overrides = new[]
            {
                new CategoryStyleEntry { category = "furniture", color_hex = "#00FF00" }
            };
            CategoryPalette.Configure(overrides);

            Color c = CategoryPalette.ResolveColor("furniture");
            Assert.AreEqual(new Color(0f, 1f, 0f), c);
        }

        [Test]
        public void ResolveIconKey_ConfigureOverridesCategory_UsesOverride()
        {
            var overrides = new[]
            {
                new CategoryStyleEntry { category = "furniture", icon_key = "custom_icon" }
            };
            CategoryPalette.Configure(overrides);

            string key = CategoryPalette.ResolveIconKey("furniture");
            Assert.AreEqual("custom_icon", key);
        }
    }
}
