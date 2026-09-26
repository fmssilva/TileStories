using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // MarkerVisualResolver: which Outline Types row a KNOWN status draws. The row the developer picked
    // (status_level_key) wins; the percentage only decides when there is no key. Real config, real
    // palettes (MarkerVisualSettings.ApplyPalettes), real resolver -- no scene needed.
    public class MarkerVisualResolverTests
    {
        private WallConfigData _config;
        private MarkerVisualSettings _settings;

        [SetUp]
        public void SetUp()
        {
            // Two rows share 100 %, the shape of the shipped dev wall's "destroyed" + "unknown" rows
            _config = new WallConfigData
            {
                marker_shape = "circle",
                marker_outline_mode = "per_type",
                marker_use_badge = true,
                category_styles = new List<CategoryStyleEntry> { new CategoryStyleEntry { category = "cat", icon_key = "unknown", color_hex = "#3366CC" } },
                badge_categories = new List<BadgeCategoryEntry>(),
                outline_levels = new List<OutlineLevelEntry>
                {
                    new OutlineLevelEntry { key = "intact", pct = 0f, line_style = "solid", color_hex = "#00FF00" },
                    new OutlineLevelEntry { key = "destroyed", pct = 100f, line_style = "dash_short", color_hex = "#FF0000" },
                    new OutlineLevelEntry { key = "unknown", pct = 100f, line_style = "dotted", color_hex = "#808080" },
                },
                hierarchy_levels = new List<HierarchyLevelEntry>()
            };
            MarkerVisualSettings.ApplyPalettes(_config);
            _settings = MarkerVisualSettings.Resolve(_config, null);
        }

        [TearDown]
        public void TearDown()
        {
            StatusRamp.ResetToDefaults();
            CategoryPalette.ClearOverrides();
            BadgeCategoryPalette.Clear();
            MarkerHierarchyResolver.ResetToDefaults();
        }

        private static POIData Poi(string key, float pct) => new POIData
        {
            id = "p", category = "cat", has_status = true, status_pct = pct, status_level_key = key
        };

        private static Color Hex(string hex) { ColorUtility.TryParseHtmlString(hex, out var c); return c; }

        [Test]
        public void KnownStatus_DrawsThePickedRow_EvenWhenAnotherRowSharesItsPercentage()
        {
            var state = MarkerVisualResolver.Resolve(Poi("unknown", 100f), _settings);
            Assert.IsTrue(state.ShowRing);
            Assert.AreEqual("dotted", state.RingLevel.RingSpriteKey, "the 'unknown' row was picked, not 'destroyed' (same 100 %)");
            Assert.AreEqual(Hex("#808080"), state.RingLevel.RingColor);

            state = MarkerVisualResolver.Resolve(Poi("destroyed", 100f), _settings);
            Assert.AreEqual("dash_short", state.RingLevel.RingSpriteKey);
        }

        [Test]
        public void KnownStatus_KeyWins_OverAStalePercentage()
        {
            // The table re-spaces percentages on add/remove; a POI's stored pct can lag behind
            var state = MarkerVisualResolver.Resolve(Poi("intact", 90f), _settings);
            Assert.AreEqual("solid", state.RingLevel.RingSpriteKey);
        }

        [Test]
        public void KnownStatus_WithoutAKey_OrWithAStaleKey_UsesTheNearestPercentage()
        {
            Assert.AreEqual("solid", MarkerVisualResolver.Resolve(Poi(null, 10f), _settings).RingLevel.RingSpriteKey);
            Assert.AreEqual("dash_short", MarkerVisualResolver.Resolve(Poi("gone_row", 95f), _settings).RingLevel.RingSpriteKey);
        }

        [Test]
        public void StatusFallbackBadge_TakesThePickedRowsColour()
        {
            var state = MarkerVisualResolver.Resolve(Poi("unknown", 100f), _settings);
            Assert.AreEqual(BadgeSource.StatusFallback, state.Badge, "no badge category: the badge falls back to the status colour");
            Assert.AreEqual(Hex("#808080"), state.BadgeColor);
        }

        // _2.2.2 rule 1: no status axis = no badge, even with a real badge category left on the POI
        // (turning "Has status" off in the editor clears the status, not the badge key)
        [Test]
        public void NoStatus_HidesTheBadge_EvenWithARealBadgeCategory()
        {
            _config.badge_categories.Add(new BadgeCategoryEntry { key = "b", icon_key = "unknown", color_hex = "#123456" });
            MarkerVisualSettings.ApplyPalettes(_config);
            var poi = new POIData { id = "p", category = "cat", has_status = false, badge_category = "b" };

            var state = MarkerVisualResolver.Resolve(poi, _settings);
            Assert.AreEqual(BadgeSource.Hidden, state.Badge);
            Assert.IsFalse(state.ShowRing);

            poi.has_status = true; poi.status_level_key = "intact";
            Assert.AreEqual(BadgeSource.Category, MarkerVisualResolver.Resolve(poi, _settings).Badge,
                "with a status the badge category shows again");
        }

        [Test]
        public void SameHue_ShadesByThePickedRowsPercentage_NotAStaleOne()
        {
            _config.marker_outline_mode = "same_hue";
            MarkerVisualSettings.ApplyPalettes(_config);
            _settings = MarkerVisualSettings.Resolve(_config, null);

            var stalePct = MarkerVisualResolver.Resolve(Poi("intact", 90f), _settings);
            var exact = MarkerVisualResolver.Resolve(Poi("intact", 0f), _settings);
            Assert.AreEqual(exact.SymbolFill, stalePct.SymbolFill, "an 'intact' POI must look intact whatever its stored pct");
            Assert.AreEqual(exact.RingHueColor, stalePct.RingHueColor);
        }
    }
}
