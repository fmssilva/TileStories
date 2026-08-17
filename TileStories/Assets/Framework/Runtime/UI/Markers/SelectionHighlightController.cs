using System;
using System.Collections.Generic;
using UnityEngine;

namespace TileStories
{
    // Drives the selection highlight/dim (spec _2.6 section 11). When a marker is
    // selected, all other markers fade to a partial alpha so the selected one reads
    // as the clear focal point; re-selecting the active marker clears the highlight.
    //
    // Plain C# (no MonoBehaviour) so the decision logic is Tier-0 testable with a
    // lightweight WallSession + real MarkerViews (Editor/AddComponent), per the
    // "logic in plain classes" rule. WallSession only wires it once and disposes it
    // in OnDisable; the marker prefab never holds a controller reference.
    //
    // Coordinate seam: dim is applied through MarkerView.SetVisible(float, float),
    // which stamps _highlightAlpha so LODController's per-tick SetVisible(true,...)
    // targets the dimmed level instead of full -- the two alpha writers compose
    // through one seam instead of fighting over the shared CanvasGroup.alpha.
    public sealed class SelectionHighlightController : IDisposable
    {
        // Non-selected marker opacity while a highlight is active. 0.3 keeps the wall
        // readable (structure still visible) while subduing non-selected markers.
        // Not a WallConfigData field: section 11 defines no per-wall dim alpha, so
        // exposing a named default leaves the door open for future config without
        // inventing scope now (20-code-quality: no speculative fields).
        private const float DIM_ALPHA = 0.3f;
        private const float ALPHA_FULL = 1f;
        private const float DEFAULT_FADE = 0.15f;

        private readonly WallSession _wallSession;
        private readonly WallConfigData _config;
        private bool _disposed;

        // Currently highlighted POI id (null = no highlight).
        private string _selectedId;

        // Constructed by WallSession after markers are spawned; config is the same
        // WallConfigData the session resolved, so a wall can disable highlight.
        public SelectionHighlightController(WallSession wallSession, WallConfigData config)
        {
            _wallSession = wallSession ?? throw new ArgumentNullException(nameof(wallSession));
            _config = config;
            SelectionEventBus.OnMarkerSelected += OnSelected;
            SelectionEventBus.OnSelectionCleared += OnCleared;
        }

        // Handler subscribed to SelectionEventBus.OnMarkerSelected.
        internal void OnSelected(string poiId) => Select(poiId);

        // Handler subscribed to SelectionEventBus.OnSelectionCleared.
        internal void OnCleared() => Clear();

        // Highlight the selected marker and dim every other one. Re-selecting the
        // already-selected marker toggles the highlight off.
        internal void Select(string poiId)
        {
            if (_disposed || _config == null || !_config.selection_highlight_enabled) return;
            var markers = _wallSession.SpawnedMarkers;
            if (markers == null) return;

            float fade = ResolvedFadeDuration(poiId);
            bool alreadySelected = string.Equals(_selectedId, poiId, StringComparison.Ordinal);

            if (alreadySelected)
            {
                // Re-tap the selected marker clears the highlight entirely.
                RestoreAll(fade);
                _selectedId = null;
                return;
            }

            _selectedId = poiId;
            for (int i = 0; i < markers.Count; i++)
            {
                var m = markers[i];
                if (m == null) continue;
                if (string.Equals(m.PoiId, poiId, StringComparison.Ordinal))
                {
                    // Selected marker: full opacity, instant pop (it is the focal point).
                    m.SetVisible(ALPHA_FULL, 0f);
                }
                else
                {
                    m.SetVisible(DIM_ALPHA, fade);
                }
            }
        }

        // Restore every marker to full opacity and drop the dim seam.
        internal void Clear()
        {
            if (_disposed || _config == null) return;
            RestoreAll(ResolvedFadeDuration(_selectedId));
            _selectedId = null;
        }

        private void RestoreAll(float fade)
        {
            var markers = _wallSession.SpawnedMarkers;
            if (markers == null) return;
            for (int i = 0; i < markers.Count; i++)
                if (markers[i] != null)
                    markers[i].SetVisible(ALPHA_FULL, fade);
        }

        // Match the dim fade length to the selected marker's hierarchy reveal curve
        // (falls back to the framework default when no hierarchy is configured).
        private float ResolvedFadeDuration(string targetPoiId)
        {
            if (string.IsNullOrEmpty(targetPoiId) || _wallSession.SpawnedMarkers == null)
                return DEFAULT_FADE;
            var markers = _wallSession.SpawnedMarkers;
            for (int i = 0; i < markers.Count; i++)
            {
                if (string.Equals(markers[i]?.PoiId, targetPoiId, StringComparison.Ordinal))
                    return markers[i].RevealDurationSeconds;
            }
            return DEFAULT_FADE;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            SelectionEventBus.OnMarkerSelected -= OnSelected;
            SelectionEventBus.OnSelectionCleared -= OnCleared;
            // Snap everyone back to full immediately on teardown (no half-dead state).
            RestoreAll(0f);
        }
    }
}
