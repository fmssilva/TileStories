using System;
using System.Collections.Generic;

namespace TileStories
{
    // The only writer of the markers' selection channel (MarkerView.SetSelectionAlpha, spec _2.6
    // sections 7 and 11). It keeps two pieces of state -- the selected POI (from SelectionEventBus) and
    // the active result set (from ResultSetCoordinator, null = no search / filter active) -- and applies
    // SelectionAlphaRule to every running marker whenever either changes. Settings are read from the
    // wall on every apply, so a live config edit takes effect on the next change or Refresh.
    //
    // Plain C#, created by WallSession after spawning and disposed in its OnDisable. The channel is
    // independent of LOD visibility and the crowding fade (MarkerView.ComposeAlpha multiplies them).
    public sealed class SelectionHighlightController : IDisposable
    {
        private const float DefaultFade = 0.15f;

        private readonly WallSession _wallSession;
        private string _selectedId;
        private HashSet<string> _resultSet;
        private bool _disposed;

        public SelectionHighlightController(WallSession wallSession)
        {
            _wallSession = wallSession ?? throw new ArgumentNullException(nameof(wallSession));
            _selectedId = SelectionEventBus.CurrentPoiId;
            SelectionEventBus.OnMarkerSelected += OnSelected;
            SelectionEventBus.OnSelectionCleared += OnCleared;
        }

        // The selected POI id the controller is showing (null = none)
        public string SelectedId => _selectedId;

        // The active result set (null = no search / filter active)
        public IReadOnlyCollection<string> ResultSet => _resultSet;

        private void OnSelected(string poiId)
        {
            _selectedId = poiId;
            Apply(FadeFor(poiId));
        }

        private void OnCleared()
        {
            string previous = _selectedId;
            _selectedId = null;
            Apply(FadeFor(previous));
        }

        // The search / filter result set to show (null = none active: every marker stays full)
        public void SetResultSet(ICollection<string> poiIds)
        {
            _resultSet = poiIds == null ? null : new HashSet<string>(poiIds);
            Apply(DefaultFade);
        }

        // Re-apply to every running marker (after the wall swapped its markers or its settings)
        public void Refresh() => Apply(0f);

        private void Apply(float fade)
        {
            if (_disposed)
                return;
            var markers = _wallSession.SpawnedMarkers;
            if (markers == null)
                return;

            var settings = _wallSession.SelectFilterSearch;
            bool highlight = settings?.selection?.highlight_enabled ?? true;
            float dim = settings?.selection?.dim_alpha ?? 0.3f;
            float mismatch = SelectFilterSearchOptions.MismatchAlpha(settings?.filter);
            bool anySelected = _selectedId != null;

            for (int i = 0; i < markers.Count; i++)
            {
                var m = markers[i];
                if (m == null) continue;
                float alpha = SelectionAlphaRule.AlphaFor(
                    m.PoiId == _selectedId, anySelected,
                    _resultSet != null, _resultSet != null && _resultSet.Contains(m.PoiId),
                    highlight, dim, mismatch);
                m.SetSelectionAlpha(alpha, fade);
            }
        }

        // The fade matches the selected marker's hierarchy reveal duration (its own timing curve)
        private float FadeFor(string poiId)
        {
            var markers = _wallSession.SpawnedMarkers;
            if (poiId == null || markers == null)
                return DefaultFade;
            for (int i = 0; i < markers.Count; i++)
                if (markers[i] != null && markers[i].PoiId == poiId)
                    return markers[i].RevealDurationSeconds;
            return DefaultFade;
        }

        public void Dispose()
        {
            if (_disposed) return;
            SelectionEventBus.OnMarkerSelected -= OnSelected;
            SelectionEventBus.OnSelectionCleared -= OnCleared;
            // - leave every marker full: no half-dimmed wall after teardown
            _selectedId = null;
            _resultSet = null;
            Apply(0f);
            _disposed = true;
        }
    }
}
