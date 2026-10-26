using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TileStories
{
    // Zoom-on-select (spec _2.6 section 11, Hoang & Thomas' Augmented Viewport): tapping a marker that
    // sits in a crowd -- or tapping a cluster aggregate -- zooms the AR camera in so the crowd separates
    // and the next tap is precise. An isolated marker is easy to tap already and never zooms.
    //
    // The crowd is counted when the tap happens, from the markers' real screen positions (the other
    // visible markers within neighbour_radius_px), so it does not depend on LOD running. Decisions are
    // pure statics (CountNeighbours, ComputeZoomTarget); ARZoomController is the only zoom writer.
    public sealed class ZoomOnSelectController : IDisposable
    {
        private readonly WallSession _wallSession;
        private readonly ARZoomController _arZoom;
        private bool _disposed;

        public ZoomOnSelectController(WallSession wallSession, ARZoomController arZoom)
        {
            _wallSession = wallSession ?? throw new ArgumentNullException(nameof(wallSession));
            _arZoom = arZoom;
            SelectionEventBus.OnMarkerSelected += OnMarkerSelected;
            SelectionEventBus.OnClusterSelected += OnClusterSelected;
        }

        // The crowd size the last marker tap measured (-1 before any tap), for tests and the readout
        public int LastNeighbourCount { get; private set; } = -1;

        private void OnMarkerSelected(string poiId)
        {
            var zoom = _wallSession.SelectFilterSearch?.selection?.zoom;
            var cam = Camera.main;
            if (_disposed || zoom == null || _arZoom == null || cam == null)
                return;

            MarkerView selected = null;
            var others = new List<Vector2>();
            foreach (var m in _wallSession.SpawnedMarkers)
            {
                if (m == null || !m.IsVisible) continue;
                if (m.PoiId == poiId) { selected = m; continue; }
                // - the true place, not a displacement nudge: a displaced crowd is still a crowd
                Vector3 sp = cam.WorldToScreenPoint(m.UndisplacedWorldPosition);
                if (sp.z > 0f) others.Add(sp);
            }
            if (selected == null)
                return;

            Vector3 selectedScreen = cam.WorldToScreenPoint(selected.UndisplacedWorldPosition);
            LastNeighbourCount = selectedScreen.z > 0f
                ? CountNeighbours(selectedScreen, others, zoom.neighbour_radius_px)
                : 0;
            ApplyTarget(ComputeZoomTarget(zoom, false, LastNeighbourCount, ARZoomState.ZoomFactor,
                MaxZoomKeepingOnScreen(ToViewportOffset(cam, selectedScreen), cam.fieldOfView)));
        }

        private void OnClusterSelected(IReadOnlyList<string> memberIds)
        {
            var zoom = _wallSession.SelectFilterSearch?.selection?.zoom;
            var cam = Camera.main;
            if (_disposed || zoom == null || _arZoom == null || cam == null)
                return;

            // - the cluster stands at its members' centre: that point must stay on screen
            Vector3 sum = Vector3.zero;
            int n = 0;
            foreach (var m in _wallSession.SpawnedMarkers)
                if (m != null && memberIds.Contains(m.PoiId)) { sum += m.UndisplacedWorldPosition; n++; }
            float maxZoom = n > 0 ? MaxZoomKeepingOnScreen(ToViewportOffset(cam, cam.WorldToScreenPoint(sum / n)), cam.fieldOfView) : float.MaxValue;
            // - a cluster IS a crowd: its members are its neighbours
            ApplyTarget(ComputeZoomTarget(zoom, true, memberIds.Count, ARZoomState.ZoomFactor, maxZoom));
        }

        // A screen point as its offset from the screen centre, -1..1 per axis (1 = the edge)
        private static Vector2 ToViewportOffset(Camera cam, Vector3 screen)
        {
            if (screen.z <= 0f) return new Vector2(float.MaxValue, float.MaxValue);
            return new Vector2(screen.x / cam.pixelWidth * 2f - 1f, screen.y / cam.pixelHeight * 2f - 1f);
        }

        // Share of the half-screen the tapped point may reach after the zoom (the rest is a margin)
        public const float KeepOnScreen = 0.85f;

        // The largest extra zoom that keeps a point at this viewport offset on screen. The zoom narrows the
        // vertical field of view (fov / zoom), so a point's offset grows by tan(fov/2) / tan(fov'/2): it stays
        // inside KeepOnScreen while tan(fov'/2) >= offset * tan(fov/2) / KeepOnScreen. The centre never limits.
        public static float MaxZoomKeepingOnScreen(Vector2 viewportOffset, float verticalFovDeg)
        {
            float m = Mathf.Max(Mathf.Abs(viewportOffset.x), Mathf.Abs(viewportOffset.y));
            if (m < 1e-4f) return float.MaxValue;
            if (m >= KeepOnScreen) return 1f;
            float halfFov = verticalFovDeg * 0.5f * Mathf.Deg2Rad;
            float narrowestHalfFov = Mathf.Atan(m * Mathf.Tan(halfFov) / KeepOnScreen);
            return halfFov / narrowestHalfFov;
        }

        private void ApplyTarget(float? target)
        {
            if (target.HasValue)
                _arZoom.SetZoomAnimated(target.Value);
        }

        // How many of the other screen points lie within radiusPx of the selected one
        public static int CountNeighbours(Vector2 selected, IReadOnlyList<Vector2> others, float radiusPx)
        {
            float r2 = radiusPx * radiusPx;
            int n = 0;
            for (int i = 0; i < others.Count; i++)
                if ((others[i] - selected).sqrMagnitude <= r2)
                    n++;
            return n;
        }

        // Smallest extra zoom still worth animating (below it the tapped point sits too near the edge)
        public const float MinUsefulZoom = 1.1f;

        // The zoom factor to animate to, or null: zoom off, a trigger that excludes this kind of tap, a marker
        // with fewer neighbours than min_neighbours, or a point so near the edge that zooming would push it
        // off screen. maxRelativeZoom (MaxZoomKeepingOnScreen) caps Zoom Factor so the tapped point stays visible.
        public static float? ComputeZoomTarget(ZoomOnSelectSettings zoom, bool isCluster, int neighbours, float currentZoom,
            float maxRelativeZoom = float.MaxValue)
        {
            if (zoom == null || !zoom.enabled)
                return null;
            if (!SelectFilterSearchOptions.TriggerAllows(zoom.trigger, isCluster))
                return null;
            if (!isCluster && neighbours < zoom.min_neighbours)
                return null;
            float relative = Mathf.Min(Mathf.Max(1f, zoom.factor), maxRelativeZoom);
            return relative < MinUsefulZoom ? (float?)null : currentZoom * relative;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            SelectionEventBus.OnMarkerSelected -= OnMarkerSelected;
            SelectionEventBus.OnClusterSelected -= OnClusterSelected;
        }
    }
}
