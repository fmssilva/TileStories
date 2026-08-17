using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TileStories
{
    // Pure logic for evaluating which POIs pass the active facet filters and
    // computing the "relax filters" suggestion when zero results remain.
    // Extracted from FilterTrayView so it can be Tier-0 tested without a scene
    // or UI Toolkit instance. Stateless per-call (spec _2.6 section 7).
    public static class FilterFacetEvaluator
    {
        // Check whether a single POI passes all active facet filters.
        public static bool PoiPassesFilters(POIData poi,
            HashSet<string> activeCategories,
            HashSet<string> activeBadgeCategories,
            HashSet<string> activeOutlineLevels,
            HashSet<string> activeHierarchyLevels)
        {
            if (poi == null) return false;

            // No filter is always a pass; an empty active set means "show all"
            bool passesCategory = activeCategories.Count == 0 ||
                (!string.IsNullOrEmpty(poi.category) && activeCategories.Contains(poi.category));

            bool passesBadge = activeBadgeCategories.Count == 0 ||
                (!string.IsNullOrEmpty(poi.badge_category) && activeBadgeCategories.Contains(poi.badge_category));

            bool passesOutline = activeOutlineLevels.Count == 0 ||
                (!string.IsNullOrEmpty(poi.status_level_key) && activeOutlineLevels.Contains(poi.status_level_key));

            bool passesHierarchy = activeHierarchyLevels.Count == 0 ||
                (!string.IsNullOrEmpty(poi.hierarchy_level_key) && activeHierarchyLevels.Contains(poi.hierarchy_level_key));

            return passesCategory && passesBadge && passesOutline && passesHierarchy;
        }

        // Count how many POIs pass all filters when one specific facet value
        // is removed from the active set. Used by ComputeRelaxSuggestion.
        public static int CountPoisWithFacetRemoved(List<POIData> pois,
            string removedKey, HashSet<string> activeSet,
            string facetType,
            HashSet<string> activeCategories,
            HashSet<string> activeBadgeCategories,
            HashSet<string> activeOutlineLevels,
            HashSet<string> activeHierarchyLevels)
        {
            // Build temporary active sets with the removed key excluded
            var tempCategories = activeCategories.Count > 0 ? new HashSet<string>(activeCategories) : activeCategories;
            var tempBadges = activeBadgeCategories.Count > 0 ? new HashSet<string>(activeBadgeCategories) : activeBadgeCategories;
            var tempOutlines = activeOutlineLevels.Count > 0 ? new HashSet<string>(activeOutlineLevels) : activeOutlineLevels;
            var tempHierarchy = activeHierarchyLevels.Count > 0 ? new HashSet<string>(activeHierarchyLevels) : activeHierarchyLevels;

            switch (facetType)
            {
                case "category": tempCategories.Remove(removedKey); break;
                case "badge": tempBadges.Remove(removedKey); break;
                case "status": tempOutlines.Remove(removedKey); break;
                case "hierarchy": tempHierarchy.Remove(removedKey); break;
            }

            int count = 0;
            foreach (var poi in pois)
            {
                if (PoiPassesFilters(poi, tempCategories, tempBadges, tempOutlines, tempHierarchy))
                    count++;
            }
            return count;
        }

        // Compute which single facet removal would yield the most results.
        // Returns a description string, or null if fewer than 2 facets are active.
        public static string ComputeRelaxSuggestion(List<POIData> pois,
            HashSet<string> activeCategories,
            HashSet<string> activeBadgeCategories,
            HashSet<string> activeOutlineLevels,
            HashSet<string> activeHierarchyLevels)
        {
            int activeTotal = activeCategories.Count + activeBadgeCategories.Count +
                              activeOutlineLevels.Count + activeHierarchyLevels.Count;

            if (activeTotal < 2 || pois == null)
                return null;

            int bestCount = 0;
            string bestSuggestion = null;

            foreach (string cat in activeCategories)
            {
                int count = CountPoisWithFacetRemoved(pois, cat, activeCategories, "category",
                    activeCategories, activeBadgeCategories, activeOutlineLevels, activeHierarchyLevels);
                if (count > bestCount)
                {
                    bestCount = count;
                    bestSuggestion = $"Remove category \"{cat}\" filter";
                }
            }

            foreach (string badge in activeBadgeCategories)
            {
                int count = CountPoisWithFacetRemoved(pois, badge, activeBadgeCategories, "badge",
                    activeCategories, activeBadgeCategories, activeOutlineLevels, activeHierarchyLevels);
                if (count > bestCount)
                {
                    bestCount = count;
                    bestSuggestion = $"Remove badge \"{badge}\" filter";
                }
            }

            foreach (string level in activeOutlineLevels)
            {
                int count = CountPoisWithFacetRemoved(pois, level, activeOutlineLevels, "status",
                    activeCategories, activeBadgeCategories, activeOutlineLevels, activeHierarchyLevels);
                if (count > bestCount)
                {
                    bestCount = count;
                    bestSuggestion = $"Remove status level \"{level}\" filter";
                }
            }

            foreach (string level in activeHierarchyLevels)
            {
                int count = CountPoisWithFacetRemoved(pois, level, activeHierarchyLevels, "hierarchy",
                    activeCategories, activeBadgeCategories, activeOutlineLevels, activeHierarchyLevels);
                if (count > bestCount)
                {
                    bestCount = count;
                    bestSuggestion = $"Remove hierarchy level \"{level}\" filter";
                }
            }

            return bestSuggestion;
        }
    }
}
