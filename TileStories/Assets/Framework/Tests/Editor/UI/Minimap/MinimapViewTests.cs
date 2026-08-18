using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Tests
{
    // Tier-0 tests for MinimapView's coordinate conversion and SelectionEventBus
    // integration. Uses MinimapCoordinateConverter directly (the extracted pure
    // logic) and verifies event-driven selection highlighting behavior.
    // (spec _2.6 section 8)
    public class MinimapViewTests
    {
        private WallConfigData _config;
        private POISearchIndex _searchIndex;

        [SetUp]
        public void SetUp()
        {
            // Reset CategoryPalette state between tests
            CategoryPalette.ClearOverrides();

            // Build a simple wall config with 3 POIs
            _config = new WallConfigData
            {
                wall_id = "test_wall",
                wall_name = "Test Wall",
                minimap_icon_style = "category_colored_dots",
                minimap_enabled = true,
                category_styles = new List<CategoryStyleEntry>
                {
                    new CategoryStyleEntry { category = "religious", color_hex = "#FF0000" },
                    new CategoryStyleEntry { category = "civic", color_hex = "#00FF00" },
                },
                pois = new List<POIData>
                {
                    new POIData { id = "poi_1", name = "POI One", category = "religious", x_norm = 0.2f, y_norm = 0.8f },
                    new POIData { id = "poi_2", name = "POI Two", category = "civic", x_norm = 0.5f, y_norm = 0.5f },
                    new POIData { id = "poi_3", name = "POI Three", category = "religious", x_norm = 0.8f, y_norm = 0.2f },
                }
            };

            _searchIndex = new POISearchIndex();
            _searchIndex.Build(_config);

            // Configure CategoryPalette with the test config
            CategoryPalette.Configure(_config.category_styles);
        }

        [TearDown]
        public void TearDown()
        {
            CategoryPalette.ClearOverrides();
        }

        [Test]
        public void CoordinateConversion_TopLeftPOI_MapsToBottomLeftOfMinimap()
        {
            // POI at x_norm=0.2, y_norm=0.8 -> y is inverted: (1-0.8)*200 = 40
            Vector2 pos = MinimapCoordinateConverter.ConvertToPixel(0.2f, 0.8f, 200f, 200f, 20f);
            Assert.AreEqual(0.2f * 200f - 10f, pos.x, 0.001f);
            Assert.AreEqual((1f - 0.8f) * 200f - 10f, pos.y, 0.001f);
        }

        [Test]
        public void CoordinateConversion_BottomRightPOI_MapsToTopRight()
        {
            // POI at x_norm=0.8, y_norm=0.2 -> y inverted: (1-0.2)*200 = 160
            Vector2 pos = MinimapCoordinateConverter.ConvertToPixel(0.8f, 0.2f, 200f, 200f, 20f);
            Assert.AreEqual(0.8f * 200f - 10f, pos.x, 0.001f);
            Assert.AreEqual((1f - 0.2f) * 200f - 10f, pos.y, 0.001f);
        }

        [Test]
        public void CoordinateConversion_CenterPOI_MapsToCenter()
        {
            // POI at x_norm=0.5, y_norm=0.5 -> center
            Vector2 pos = MinimapCoordinateConverter.ConvertToPixel(0.5f, 0.5f, 200f, 200f, 20f);
            Assert.AreEqual(100f - 10f, pos.x, 0.001f);
            Assert.AreEqual(100f - 10f, pos.y, 0.001f);
        }

        [Test]
        public void ClampNorm_NegativeValue_ClampsToZero()
        {
            Assert.AreEqual(0f, MinimapCoordinateConverter.ClampNorm(-1f), 0.001f);
        }

        [Test]
        public void ClampNorm_ValueAboveOne_ClampsToOne()
        {
            Assert.AreEqual(1f, MinimapCoordinateConverter.ClampNorm(2f), 0.001f);
        }

                [Test]
        public void SelectionEventBus_RaiseAndClear_RoundTrip()
        {
            // Verify SelectionEventBus works as expected for minimap dot taps
            string selectedId = null;
            System.Action<string> handler = id => selectedId = id;
            SelectionEventBus.OnMarkerSelected += handler;

            SelectionEventBus.RaiseMarkerSelected("poi_1");
            Assert.AreEqual("poi_1", selectedId);

            SelectionEventBus.RaiseMarkerSelected("poi_2");
            Assert.AreEqual("poi_2", selectedId);

            // Unsubscribe using the stored delegate reference
            SelectionEventBus.OnMarkerSelected -= handler;
            selectedId = "stale";
            SelectionEventBus.RaiseMarkerSelected("poi_3");
            Assert.AreEqual("stale", selectedId, "Handler should be unsubscribed -- value should not change");
        }

        [Test]
        public void CategoryPalette_ConfiguredFromConfig_ResolvesCategoryColors()
        {
            // Verify colors are resolved for configured categories
            Color religiousColor = CategoryPalette.ResolveColor("religious");
            Color civicColor = CategoryPalette.ResolveColor("civic");
            Color unknownColor = CategoryPalette.ResolveColor("unknown_category");

            // Configured categories should return their hex colors
            Color expectedReligious;
            ColorUtility.TryParseHtmlString("#FF0000", out expectedReligious);
            Assert.AreEqual(expectedReligious, religiousColor, "Religious color should match hex config");

            Color expectedCivic;
            ColorUtility.TryParseHtmlString("#00FF00", out expectedCivic);
            Assert.AreEqual(expectedCivic, civicColor, "Civic color should match hex config");

            // Unconfigured categories fall through to hash-based color
            Assert.AreNotEqual(Color.clear, unknownColor, "Unknown category should fall through to hash color");
        }

        [Test]
        public void MinimapConfig_FieldsAccessibleFromWallConfigData()
        {
            // Verify the config fields added in Block 3 are accessible
            Assert.IsTrue(_config.minimap_enabled);
            Assert.AreEqual("category_colored_dots", _config.minimap_icon_style);
            Assert.AreEqual("toggle", _config.minimap_visibility);
        }

        [Test]
        public void SearchMode_FieldsAccessibleFromWallConfigData()
        {
            _config.search_mode = "faceted";
            Assert.AreEqual("faceted", _config.search_mode);
        }

        [Test]
        public void ViewModeConfig_FieldsAccessibleFromWallConfigData()
        {
            _config.default_result_view = "minimap";
            Assert.AreEqual("minimap", _config.default_result_view);
        }

        [Test]
        public void VoiceSearchConfig_FieldsAccessibleFromWallConfigData()
        {
            Assert.IsFalse(_config.voice_search_enabled);
            _config.voice_search_enabled = true;
            Assert.IsTrue(_config.voice_search_enabled);
            Assert.AreEqual("all", _config.voice_search_match_mode);
        }

        [Test]
        public void SearchFields_DefaultIsEmptyList()
        {
            // search_fields replaces the removed weight_* fields as the configurable
            // search axis system. Default must be an empty (non-null) list.
            Assert.IsNotNull(_config.search_fields);
            Assert.AreEqual(0, _config.search_fields.Count);
        }

        [Test]
        public void NoResultsMessage_HasQueryPlaceholder()
        {
            Assert.IsNotNull(_config.no_results_message);
            Assert.IsTrue(_config.no_results_message.Contains("{query}"));
        }

        [Test]
        public void RecentSearchCount_DefaultIsFive()
        {
            Assert.AreEqual(5, _config.recent_search_count);
        }

        [Test]
        public void ShowSuggestedCategories_DefaultIsTrue()
        {
            Assert.IsTrue(_config.show_suggested_categories);
        }
    }
}
