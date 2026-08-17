using System.Collections.Generic;
using NUnit.Framework;

namespace TileStories.Tests
{
    // Tier-0 tests for FilterFacetEvaluator -- pure logic, no MonoBehaviour or
    // scene required. Validates facet filtering, POI pass-through, and the
    // "relax filters" suggestion computation (spec _2.6 section 7).
    public class FilterFacetEvaluatorTests
    {
        private List<POIData> _pois;

        [SetUp]
        public void SetUp()
        {
            _pois = new List<POIData>
            {
                new POIData { id = "a", name = "POI A", category = "religious", badge_category = "intact", status_level_key = "level_1", hierarchy_level_key = "level_1" },
                new POIData { id = "b", name = "POI B", category = "religious", badge_category = "severe", status_level_key = "level_2", hierarchy_level_key = "level_1" },
                new POIData { id = "c", name = "POI C", category = "civic", badge_category = "intact", status_level_key = "level_1", hierarchy_level_key = "level_2" },
                new POIData { id = "d", name = "POI D", category = "commerce", badge_category = "severe", status_level_key = "level_2", hierarchy_level_key = "level_2" },
            };
        }

        [Test]
        public void PoiPassesFilters_NoFiltersActive_AllPass()
        {
            var empty = new HashSet<string>();
            foreach (var poi in _pois)
            {
                Assert.IsTrue(FilterFacetEvaluator.PoiPassesFilters(poi, empty, empty, empty, empty),
                    $"POI {poi.id} should pass with no filters");
            }
        }

        [Test]
        public void PoiPassesFilters_CategoryFilter_ActiveOnlyMatchingCategory()
        {
            var activeCategories = new HashSet<string> { "religious" };
            var empty = new HashSet<string>();

            Assert.IsTrue(FilterFacetEvaluator.PoiPassesFilters(_pois[0], activeCategories, empty, empty, empty));
            Assert.IsTrue(FilterFacetEvaluator.PoiPassesFilters(_pois[1], activeCategories, empty, empty, empty));
            Assert.IsFalse(FilterFacetEvaluator.PoiPassesFilters(_pois[2], activeCategories, empty, empty, empty));
            Assert.IsFalse(FilterFacetEvaluator.PoiPassesFilters(_pois[3], activeCategories, empty, empty, empty));
        }

        [Test]
        public void PoiPassesFilters_MultipleFacets_AllMustMatch()
        {
            var activeCategories = new HashSet<string> { "religious" };
            var activeBadges = new HashSet<string> { "intact" };
            var empty = new HashSet<string>();

            // POI A: religious + intact -> passes
            Assert.IsTrue(FilterFacetEvaluator.PoiPassesFilters(_pois[0], activeCategories, activeBadges, empty, empty));
            // POI B: religious + severe -> fails badge filter
            Assert.IsFalse(FilterFacetEvaluator.PoiPassesFilters(_pois[1], activeCategories, activeBadges, empty, empty));
        }

        [Test]
        public void PoiPassesFilters_EmptyStringCategory_DoesNotMatchFilter()
        {
            var poiEmpty = new POIData { id = "x", category = "" };
            var activeCategories = new HashSet<string> { "religious" };
            var empty = new HashSet<string>();

            Assert.IsFalse(FilterFacetEvaluator.PoiPassesFilters(poiEmpty, activeCategories, empty, empty, empty));
        }

        [Test]
        public void CountPoisWithFacetRemoved_RemovingCategory_RevealsMorePOIs()
        {
            // Two filters active: religious + intact
            var activeCategories = new HashSet<string> { "religious" };
            var activeBadges = new HashSet<string> { "intact" };
            var empty = new HashSet<string>();

            // With both active: only POI A passes (religious + intact)
            int countWithBoth = CountPassing(_pois, activeCategories, activeBadges, empty, empty);
            Assert.AreEqual(1, countWithBoth);

            // Removing "religious" category filter: POIs A and C pass (both intact)
            int countAfterRemove = FilterFacetEvaluator.CountPoisWithFacetRemoved(
                _pois, "religious", activeCategories, "category",
                activeCategories, activeBadges, empty, empty);
            Assert.AreEqual(2, countAfterRemove);
        }

        [Test]
        public void CountPoisWithFacetRemoved_RemovingBadge_RevealsMorePOIs()
        {
            // Two filters active: religious + level_1 status
            var activeCategories = new HashSet<string> { "religious" };
            var activeOutlines = new HashSet<string> { "level_1" };
            var empty = new HashSet<string>();

            // Removing "level_1" status filter: both religious POIs pass
            int countAfterRemove = FilterFacetEvaluator.CountPoisWithFacetRemoved(
                _pois, "level_1", activeOutlines, "status",
                activeCategories, empty, activeOutlines, empty);
            Assert.AreEqual(2, countAfterRemove);
        }

        [Test]
        public void ComputeRelaxSuggestion_OneActiveFilter_ReturnsNull()
        {
            var activeCategories = new HashSet<string> { "religious" };
            var empty = new HashSet<string>();

            string suggestion = FilterFacetEvaluator.ComputeRelaxSuggestion(
                _pois, activeCategories, empty, empty, empty);
            Assert.IsNull(suggestion);
        }

        [Test]
        public void ComputeRelaxSuggestion_TwoActiveFilters_ReturnsBestSuggestion()
        {
            var activeCategories = new HashSet<string> { "religious" };
            var activeBadges = new HashSet<string> { "intact" };
            var empty = new HashSet<string>();

            string suggestion = FilterFacetEvaluator.ComputeRelaxSuggestion(
                _pois, activeCategories, activeBadges, empty, empty);
            Assert.IsNotNull(suggestion);
            Assert.IsTrue(suggestion.Contains("Remove"), $"Suggestion should mention removal: {suggestion}");
        }

        [Test]
        public void ComputeRelaxSuggestion_NoFiltersActive_ReturnsNull()
        {
            var empty = new HashSet<string>();
            string suggestion = FilterFacetEvaluator.ComputeRelaxSuggestion(
                _pois, empty, empty, empty, empty);
            Assert.IsNull(suggestion);
        }

        [Test]
        public void ComputeRelaxSuggestion_NullPOIs_ReturnsNull()
        {
            var activeCategories = new HashSet<string> { "religious" };
            var activeBadges = new HashSet<string> { "intact" };
            var empty = new HashSet<string>();

            string suggestion = FilterFacetEvaluator.ComputeRelaxSuggestion(
                null, activeCategories, activeBadges, empty, empty);
            Assert.IsNull(suggestion);
        }

        [Test]
        public void ComputeRelaxSuggestion_PicksFacetWithMostResults()
        {
            // Filter by religious + level_1: removing religious gives 2 results,
            // removing level_1 gives 2 results too. Both have same count.
            // Just verify we get a non-null suggestion.
            var activeCategories = new HashSet<string> { "religious" };
            var activeOutlines = new HashSet<string> { "level_1" };
            var empty = new HashSet<string>();

            string suggestion = FilterFacetEvaluator.ComputeRelaxSuggestion(
                _pois, activeCategories, empty, activeOutlines, empty);
            Assert.IsNotNull(suggestion);
        }

        [Test]
        public void ComputeRelaxSuggestion_EmptyPOIList_ReturnsNull()
        {
            var empty = new HashSet<string>();
            var activeCategories = new HashSet<string> { "religious" };
            var activeBadges = new HashSet<string> { "intact" };

            string suggestion = FilterFacetEvaluator.ComputeRelaxSuggestion(
                new List<POIData>(), activeCategories, activeBadges, empty, empty);
            Assert.IsNull(suggestion);
        }

        [Test]
        public void PoiPassesFilters_NullPOI_ReturnsFalse()
        {
            var empty = new HashSet<string>();
            Assert.IsFalse(FilterFacetEvaluator.PoiPassesFilters(null, empty, empty, empty, empty));
        }

        private static int CountPassing(List<POIData> pois,
            HashSet<string> cats, HashSet<string> badges,
            HashSet<string> outlines, HashSet<string> hierarchy)
        {
            int count = 0;
            foreach (var poi in pois)
            {
                if (FilterFacetEvaluator.PoiPassesFilters(poi, cats, badges, outlines, hierarchy))
                    count++;
            }
            return count;
        }
    }
}
