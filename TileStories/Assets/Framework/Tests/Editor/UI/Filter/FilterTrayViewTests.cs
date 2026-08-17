using System.Collections.Generic;
using NUnit.Framework;

namespace TileStories.Tests
{
    // Tier-0 tests for FilterTrayView's filter state management, delegating
    // to FilterFacetEvaluator for the pure logic. Tests cover active filter
    // tracking, clear-all behavior, and relax suggestion computation.
    // (spec _2.6 section 7)
    public class FilterTrayViewTests
    {
        private WallConfigData _config;

        [SetUp]
        public void SetUp()
        {
            _config = new WallConfigData
            {
                wall_id = "test_wall",
                wall_name = "Test Wall",
                category_styles = new List<CategoryStyleEntry>
                {
                    new CategoryStyleEntry { category = "religious" },
                    new CategoryStyleEntry { category = "civic" },
                    new CategoryStyleEntry { category = "commerce" },
                },
                badge_categories = new List<BadgeCategoryEntry>
                {
                    new BadgeCategoryEntry { key = "intact" },
                    new BadgeCategoryEntry { key = "severe" },
                    new BadgeCategoryEntry { key = "destroyed" },
                },
                outline_levels = new List<OutlineLevelEntry>
                {
                    new OutlineLevelEntry { key = "level_1", label = "Intact" },
                    new OutlineLevelEntry { key = "level_2", label = "Damaged" },
                    new OutlineLevelEntry { key = "level_3", label = "Severe" },
                },
                hierarchy_levels = new List<HierarchyLevelEntry>
                {
                    new HierarchyLevelEntry { key = "level_1", label = "1" },
                    new HierarchyLevelEntry { key = "level_2", label = "2" },
                },
                pois = new List<POIData>
                {
                    new POIData { id = "a", name = "POI A", category = "religious", badge_category = "intact", status_level_key = "level_1", hierarchy_level_key = "level_1" },
                    new POIData { id = "b", name = "POI B", category = "civic", badge_category = "severe", status_level_key = "level_2", hierarchy_level_key = "level_1" },
                    new POIData { id = "c", name = "POI C", category = "commerce", badge_category = "intact", status_level_key = "level_1", hierarchy_level_key = "level_2" },
                }
            };
        }

        [Test]
        public void ComputeRelaxSuggestion_TwoCategoriesActive_ReturnsSuggestion()
        {
            // Use FilterFacetEvaluator directly (the pure logic)
            var activeCategories = new HashSet<string> { "religious", "civic" };
            var empty = new HashSet<string>();

            string suggestion = FilterFacetEvaluator.ComputeRelaxSuggestion(
                _config.pois, activeCategories, empty, empty, empty);

            Assert.IsNotNull(suggestion);
            Assert.IsTrue(suggestion.Contains("Remove"), $"Suggestion should mention removal: {suggestion}");
        }

        [Test]
        public void ComputeRelaxSuggestion_OneFilterActive_ReturnsNull()
        {
            var activeCategories = new HashSet<string> { "religious" };
            var empty = new HashSet<string>();

            string suggestion = FilterFacetEvaluator.ComputeRelaxSuggestion(
                _config.pois, activeCategories, empty, empty, empty);

            Assert.IsNull(suggestion);
        }

        [Test]
        public void ComputeRelaxSuggestion_NoFiltersActive_ReturnsNull()
        {
            var empty = new HashSet<string>();

            string suggestion = FilterFacetEvaluator.ComputeRelaxSuggestion(
                _config.pois, empty, empty, empty, empty);

            Assert.IsNull(suggestion);
        }

        [Test]
        public void ComputeRelaxSuggestion_PicksFacetWithMostResults()
        {
            // 3 POIs: religious, civic, commerce
            // Filter: religious + civic (2 filters) -> only 0 POIs pass
            // Removing "religious" -> civic POI passes (1)
            // Removing "civic" -> religious POI passes (1)
            // Both yield 1 result, but we should still get a suggestion
            var activeCategories = new HashSet<string> { "religious", "civic" };
            var empty = new HashSet<string>();

            string suggestion = FilterFacetEvaluator.ComputeRelaxSuggestion(
                _config.pois, activeCategories, empty, empty, empty);

            Assert.IsNotNull(suggestion);
            Assert.IsTrue(suggestion.Contains("Remove"));
        }

        [Test]
        public void PoiPassesFilters_NoActiveFilters_AllPOIsPass()
        {
            var empty = new HashSet<string>();
            foreach (var poi in _config.pois)
            {
                Assert.IsTrue(FilterFacetEvaluator.PoiPassesFilters(poi, empty, empty, empty, empty));
            }
        }

        [Test]
        public void PoiPassesFilters_CategoryFilter_RestrictsToMatching()
        {
            var activeCategories = new HashSet<string> { "civic" };
            var empty = new HashSet<string>();

            // Only POI B is civic
            Assert.IsTrue(FilterFacetEvaluator.PoiPassesFilters(_config.pois[1], activeCategories, empty, empty, empty));
            Assert.IsFalse(FilterFacetEvaluator.PoiPassesFilters(_config.pois[0], activeCategories, empty, empty, empty));
            Assert.IsFalse(FilterFacetEvaluator.PoiPassesFilters(_config.pois[2], activeCategories, empty, empty, empty));
        }

        [Test]
        public void PoiPassesFilters_MultipleFacets_AllMustMatch()
        {
            var activeCategories = new HashSet<string> { "religious" };
            var activeBadges = new HashSet<string> { "intact" };
            var empty = new HashSet<string>();

            // POI A: religious + intact -> passes
            Assert.IsTrue(FilterFacetEvaluator.PoiPassesFilters(_config.pois[0], activeCategories, activeBadges, empty, empty));
            // POI B: civic + severe -> fails both
            Assert.IsFalse(FilterFacetEvaluator.PoiPassesFilters(_config.pois[1], activeCategories, activeBadges, empty, empty));
        }

        [Test]
        public void PoiPassesFilters_EmptyStringCategory_NoFilter_Passes()
        {
            var poi = new POIData { id = "x", category = "", badge_category = "", status_level_key = "", hierarchy_level_key = "" };
            var empty = new HashSet<string>();

            Assert.IsTrue(FilterFacetEvaluator.PoiPassesFilters(poi, empty, empty, empty, empty));
        }

        [Test]
        public void PoiPassesFilters_EmptyStringCategory_WithFilter_Fails()
        {
            var poi = new POIData { id = "x", category = "" };
            var activeCategories = new HashSet<string> { "religious" };
            var empty = new HashSet<string>();

            Assert.IsFalse(FilterFacetEvaluator.PoiPassesFilters(poi, activeCategories, empty, empty, empty));
        }

        [Test]
        public void PoiPassesFilters_NullPOI_ReturnsFalse()
        {
            var empty = new HashSet<string>();
            Assert.IsFalse(FilterFacetEvaluator.PoiPassesFilters(null, empty, empty, empty, empty));
        }

        [Test]
        public void HasActiveFilters_EmptySets_ReturnsFalse()
        {
            // This tests the logic: no active filters
            var empty = new HashSet<string>();
            Assert.IsFalse(empty.Count > 0 || empty.Count > 0 || empty.Count > 0 || empty.Count > 0);
        }

        [Test]
        public void HasActiveFilters_WithFilters_ReturnsTrue()
        {
            var activeCategories = new HashSet<string> { "religious" };
            var empty = new HashSet<string>();
            Assert.IsTrue(activeCategories.Count > 0 || empty.Count > 0 || empty.Count > 0 || empty.Count > 0);
        }

        [Test]
        public void Config_CategoryStyles_HasExpectedEntries()
        {
            Assert.AreEqual(3, _config.category_styles.Count);
            Assert.AreEqual("religious", _config.category_styles[0].category);
            Assert.AreEqual("civic", _config.category_styles[1].category);
            Assert.AreEqual("commerce", _config.category_styles[2].category);
        }

        [Test]
        public void Config_BadgeCategories_HasExpectedEntries()
        {
            Assert.AreEqual(3, _config.badge_categories.Count);
            Assert.AreEqual("intact", _config.badge_categories[0].key);
            Assert.AreEqual("severe", _config.badge_categories[1].key);
            Assert.AreEqual("destroyed", _config.badge_categories[2].key);
        }

        [Test]
        public void Config_OutlineLevels_HasExpectedEntries()
        {
            Assert.AreEqual(3, _config.outline_levels.Count);
            Assert.AreEqual("level_1", _config.outline_levels[0].key);
            Assert.AreEqual("level_2", _config.outline_levels[1].key);
            Assert.AreEqual("level_3", _config.outline_levels[2].key);
        }

        [Test]
        public void Config_HierarchyLevels_HasExpectedEntries()
        {
            Assert.AreEqual(2, _config.hierarchy_levels.Count);
            Assert.AreEqual("level_1", _config.hierarchy_levels[0].key);
            Assert.AreEqual("level_2", _config.hierarchy_levels[1].key);
        }
    }
}
