using System;
using System.Collections.Generic;

namespace TileStories
{
    // Single composition seam for Select/Filter/Search (_2_6 sections 5+7 via _2.7 2.6-i):
    // a facet toggle or a search submit produces ONE result set, pushed to every surface --
    // results list, scene markers, minimap dots, and the camera_highlight dimmer -- so they
    // can never show different answers to the same query+filter state.
    //
    // Plain C# (no MonoBehaviour): Tier-0 testable with fabricated config + real views,
    // per the "logic in plain classes" rule. Created/owned by SearchOverlayView once the
    // search/filter UI exists; never wired by the marker prefab or LOD.
    public sealed class ResultSetCoordinator : IDisposable
    {
        private const float ALPHA_FULL = 1f;
        private const float MISMATCH_DIM_ALPHA = 0.3f;
        private const float DEFAULT_FADE = 0.15f;

        private readonly POISearchIndex _index;
        private readonly WallConfigData _config;
        private readonly FilterTrayView _tray;
        private readonly ResultsListView _results;
        private readonly MinimapView _minimap;
        private readonly Func<IReadOnlyList<MarkerView>> _getSpawned;
        private readonly SelectionHighlightController _highlight;
        private bool _disposed;

        // Last submitted query/mode, replayed whenever facets change so a facet toggle
        // re-runs the SAME search against the narrowed candidate set.
        public string CurrentQuery { get; private set; } = "";
        public SearchMatchMode CurrentMatchMode { get; private set; } = SearchMatchMode.Any;

        public ResultSetCoordinator(POISearchIndex index, WallConfigData config,
            FilterTrayView tray, ResultsListView results, MinimapView minimap,
            Func<IReadOnlyList<MarkerView>> getSpawned,
            SelectionHighlightController highlight = null)
        {
            _index = index ?? throw new ArgumentNullException(nameof(index));
            _config = config;
            _tray = tray;
            _results = results;
            _minimap = minimap;
            _getSpawned = getSpawned;
            _highlight = highlight;

            if (_tray != null)
                _tray.OnFiltersChanged += OnFiltersChanged;
        }

        // Called by SearchOverlayView.SubmitSearch (typed + voice input).
        public void RefreshSearch(string query, SearchMatchMode matchMode)
        {
            CurrentQuery = query ?? "";
            CurrentMatchMode = matchMode;
            Refresh();
        }

        // Facet toggle / clear-all path: same query, newly narrowed candidates.
        internal void OnFiltersChanged() => Refresh();

        // Recompute the one result set and push it to every surface.
        public void Refresh()
        {
            if (_disposed || _index == null || _config?.pois == null)
                return;

            bool anyFilterActive = _tray != null && _tray.HasActiveFilters();
            HashSet<string> candidateIds = FilterFacetEvaluator.GetFilterCandidateIds(
                _config.pois,
                ToSet(_tray?.GetActiveCategories()),
                ToSet(_tray?.GetActiveBadgeCategories()),
                ToSet(_tray?.GetActiveOutlineLevels()),
                ToSet(_tray?.GetActiveHierarchyLevels()));

            // No active facet = unrestricted: pass null so Search keeps its original
            // empty-query-means-empty-list semantics (backward compatible).
            ICollection<string> narrow = anyFilterActive ? (ICollection<string>)candidateIds : null;

            // 1. Results list (one result set, not a side channel).
            _results?.RefreshResults(CurrentQuery, CurrentMatchMode, narrow);

            // 2. Minimap dots: matching stay, non-matching hidden.
            _minimap?.SetFilterCandidateIds(narrow);

            // 3. Scene markers + camera_highlight dim. Only act while a filter is active;
            //    with no filter, markers stay fully under LODController's authority.
            if (!anyFilterActive)
            {
                _highlight?.SetTargetCandidates(null);
                return;
            }

            float fade = DEFAULT_FADE;
            bool hide = _config.filter_mismatch_behaviour != "dim";
            float mismatchAlpha = hide ? 0f : MISMATCH_DIM_ALPHA;
            var markers = _getSpawned?.Invoke();
            if (markers != null)
            {
                for (int i = 0; i < markers.Count; i++)
                {
                    var m = markers[i];
                    if (m == null) continue;
                    bool inSet = candidateIds.Contains(m.PoiId);
                    // Alpha seam (not SetVisible(bool)): composes with LODController's
                    // per-tick visibility writes instead of fighting over CanvasGroup.
                    m.SetVisible(inSet ? ALPHA_FULL : mismatchAlpha, fade);
                }
            }

            // 4. camera_highlight: same dim treatment through the selection controller.
            _highlight?.SetTargetCandidates((ICollection<string>)candidateIds);
        }

        private static HashSet<string> ToSet(List<string> list)
        {
            return list == null ? new HashSet<string>() : new HashSet<string>(list);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            if (_tray != null)
                _tray.OnFiltersChanged -= OnFiltersChanged;
        }
    }
}