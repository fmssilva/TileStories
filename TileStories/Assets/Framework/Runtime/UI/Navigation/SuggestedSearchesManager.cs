using System;
using System.Collections.Generic;
using System.Linq;

namespace TileStories
{
    // Builds surfaced search suggestions from live wall data (spec _2.6 section 13).
    // Plain C# class -- no MonoBehaviour, so it is directly unit-testable without a
    // scene. Category suggestions are computed from the wall's actual POI category
    // distribution, so they can never go stale as POIs are added or removed.
    public sealed class SuggestedSearchesManager
    {
        // Where suggestion terms come from (spec _2.6 section 13 / results.suggestion_source).
        public enum SuggestedSource
        {
            // Top-N categories by live POI count (developer-maintained-free default).
            CategoryDistribution,
            // Recent user queries first, then category back-fill.
            RecentFirst
        }

        // Parse the config string `results.suggestion_source` into the enum. Case-insensitive;
        // unknown/empty/null falls back to CategoryDistribution (the default).
        public static SuggestedSource ParseSource(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return SuggestedSource.CategoryDistribution;

            if (value.Equals("category_distribution", StringComparison.OrdinalIgnoreCase))
                return SuggestedSource.CategoryDistribution;

            if (value.Equals("recent_first", StringComparison.OrdinalIgnoreCase))
                return SuggestedSource.RecentFirst;

            return SuggestedSource.CategoryDistribution;
        }

        private readonly int _topN;

        public SuggestedSearchesManager(int topN = 5)
        {
            _topN = Math.Max(1, topN);
        }

        public SuggestedSource Source { get; set; } = SuggestedSource.CategoryDistribution;

        // Build the suggestion list: the visitor's recent queries first (recent_first only), then the
        // wall's categories by how many POIs they hold, deduplicated, at most topN terms.
        public List<string> BuildSuggestions(WallConfigData config, RecentSearchesManager recent = null)
        {
            var suggestions = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            void Add(string term)
            {
                if (string.IsNullOrWhiteSpace(term))
                    return;
                term = term.Trim();
                if (!seen.Add(term))
                    return;
                suggestions.Add(term);
            }

            // recent_first: surface the visitor's own history before categories.
            if (Source == SuggestedSource.RecentFirst && recent != null)
            {
                foreach (string query in recent.Entries)
                    Add(query);
            }

            // Always back-fill with live category distribution so suggestions are
            // meaningful even with no recent history or synonyms.
            foreach (string category in CategoryDistribution(config))
                Add(category);

            while (suggestions.Count > _topN)
                suggestions.RemoveAt(suggestions.Count - 1);

            return suggestions;
        }

        // Return category names ordered by descending POI count, then name
        // ascending (stable for deterministic tests).
        private IEnumerable<string> CategoryDistribution(WallConfigData config)
        {
            if (config?.pois == null)
                yield break;

            var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var poi in config.pois)
            {
                string category = poi.category;
                if (string.IsNullOrEmpty(category))
                    continue;

                counts[category] = counts.ContainsKey(category) ? counts[category] + 1 : 1;
            }

            foreach (var kvp in counts.OrderByDescending(kvp => kvp.Value)
                                     .ThenBy(kvp => kvp.Key, StringComparer.OrdinalIgnoreCase))
                yield return kvp.Key;
        }
    }
}
