using System;
using UnityEngine;

namespace TileStories
{
    // Auto-zooms the AR camera when a selected marker sits in a dense region
    // (spec _2.6 section 11) -- surfacing whatever extra context density-
    // disambiguation buys. Block 2 only auto-zooms on POI Marker taps; Cluster/Both
    // are reserved for Block 3 (cluster-tap wiring).
    //
    // Plain C#: the *decision* (gate + target) is a pure static method, Tier-0
    // testable without any GameObject; the wiring (call SetZoomAnimated on the real
    // ARZoomController) is exercised by the PlayMode routing test. ARZoomController
    // is the exclusive writer of ARZoomState (spec section 9).
    public sealed class ZoomOnSelectController : IDisposable
    {
        private readonly WallSession _wallSession;
        private readonly WallConfigData _config;
        private readonly ARZoomController _arZoom;
        private readonly LODController _lod;
        private bool _disposed;

        // ARZoomController/LODController are scene singletons that resolve WallSession
        // themselves; WallSession reaches back to them here for the reverse lookup.
        public ZoomOnSelectController(WallSession wallSession, WallConfigData config,
            ARZoomController arZoom, LODController lod)
        {
            _wallSession = wallSession ?? throw new ArgumentNullException(nameof(wallSession));
            _config = config;
            _arZoom = arZoom;
            _lod = lod;
            SelectionEventBus.OnMarkerSelected += OnMarkerSelected;
        }

        private void OnMarkerSelected(string poiId)
        {
            if (_disposed || _config == null || _arZoom == null) return;

            int? neighborCount = _lod?.GetNeighborCount(poiId);
            float? target = ComputeZoomTarget(
                _config.zoom_on_select_trigger,
                neighborCount,
                _config.zoom_on_select_density_threshold,
                ARZoomState.ZoomFactor,
                _config.zoom_on_select_factor);

            if (target.HasValue)
                _arZoom.SetZoomAnimated(target.Value);
        }

        // Pure decision: returns the next zoom factor to apply, or null to skip.
        // Returns null when the trigger doesn't include this unit kind, or when the
        // selected unit is too isolated (few/no screen-neighbours) to benefit.
        internal static float? ComputeZoomTarget(
            WallConfigData.ZoomOnSelectTrigger trigger,
            int? neighborCount,
            int densityThreshold,
            float currentZoom,
            float factor)
        {
            if (trigger != WallConfigData.ZoomOnSelectTrigger.Marker
                && trigger != WallConfigData.ZoomOnSelectTrigger.Both)
                return null;

            // Frustum-culled / not-yet-evaluated markers have no neighbor count and
            // are ignored; below-threshold density also skips the zoom.
            if (!neighborCount.HasValue || neighborCount.Value < densityThreshold)
                return null;

            return currentZoom * factor;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            SelectionEventBus.OnMarkerSelected -= OnMarkerSelected;
        }
    }
}
