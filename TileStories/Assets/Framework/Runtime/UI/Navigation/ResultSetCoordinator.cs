using System.Collections.Generic;

namespace TileStories
{
    // What the search UI should show for the current query and filters (spec _2.6 sections 5 and 7)
    public sealed class ResultSetState
    {
        // Results are active while there is query text or at least one filter
        public bool Active;
        // The ranked results (best first) and their ids -- the ONE set every surface shows
        public List<POISearchIndex.SearchResult> Results = new();
        public HashSet<string> Ids = new();
        // With no results: the wall's message, and the filter worth removing (if any)
        public string EmptyMessage = "";
        public RelaxSuggestion? Relax;
    }

    // Joins search and filters into one result set (spec _2.6 sections 5 and 7): filters narrow first
    // (OR within a group, AND across groups), the query ranks within what is left, and the list, the
    // minimap and the markers all show that same set. Pure given the index: no UI, no scene.
    public static class ResultSetCoordinator
    {
        public static ResultSetState Compute(POISearchIndex index, IReadOnlyList<POIData> pois, string query,
            FacetSelection facets, SelectFilterSearchSettings settings)
        {
            var state = new ResultSetState();
            query = (query ?? "").Trim();
            bool filtering = facets != null && facets.Any;
            state.Active = query.Length > 0 || filtering;
            if (!state.Active || index == null)
                return state;

            var options = SelectFilterSearchOptions.ToSearchOptions(settings?.search);
            state.Results = Run(index, pois, query, facets, options);
            foreach (var r in state.Results)
                state.Ids.Add(r.PoiId);

            if (state.Results.Count == 0)
            {
                // - filters alone: their own message ("No matches for """ is what the query one read as)
                state.EmptyMessage = query.Length > 0
                    ? (settings?.search?.no_results_message ?? "").Replace("{query}", query)
                    : settings?.search?.no_results_filters_message ?? "";
                if (filtering && (settings?.filter?.relax_suggestion ?? true))
                    state.Relax = FilterFacetEvaluator.ComputeRelaxSuggestion(facets,
                        fewer => Run(index, pois, query, fewer, options).Count);
            }
            return state;
        }

        // The search within the filtered candidates (every POI when no filter is on)
        private static List<POISearchIndex.SearchResult> Run(POISearchIndex index, IReadOnlyList<POIData> pois,
            string query, FacetSelection facets, SearchOptions options)
        {
            ICollection<string> candidates = facets != null && facets.Any
                ? FilterFacetEvaluator.CandidateIds(pois, facets)
                : null;
            return index.Search(query, options, candidates);
        }

        // The relax button's text for a suggestion: "Remove filter: <label> (N results)"
        public static string RelaxText(RelaxSuggestion s, IReadOnlyList<FacetGroupOptions> groups)
        {
            string label = s.Key;
            if (groups != null)
                foreach (var g in groups)
                    if (g.Group == s.Group)
                        foreach (var (key, l) in g.Choices)
                            if (key == s.Key) label = l;
            return $"Remove filter: {label} ({s.ResultCount} results)";
        }
    }
}
