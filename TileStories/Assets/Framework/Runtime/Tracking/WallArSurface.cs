using System;
using UnityEngine;

namespace TileStories
{
    // The wall scene's IArWall (_3.1 step 10B.1): the wall session's frame (MarkerSpawnRoot, where the POIs stand), its tracker's
    // localisation (IWallTracker on the session's own object: MockLocalizationProvider in the Editor, ImmersalWallTracker on a phone),
    // the main camera as the viewer and the markers' own hierarchy sizes (MarkerHierarchyResolver, the table the markers are sized by).
    public sealed class WallArSurface : IArWall, IDisposable
    {
        private readonly WallSession _session;
        private readonly IWallTracker _tracker;

        public event Action LocalisationChanged;

        public WallArSurface(WallSession session)
        {
            _session = session;
            _tracker = session != null ? session.GetComponent<IWallTracker>() : null;
            if (_tracker == null) return;
            _tracker.OnWallLocalised += OnLocalised;
            _tracker.OnTrackingLost += OnLost;
        }

        public bool IsLocalised => _tracker != null && _tracker.IsLocalised;

        public Transform Root => _session != null ? _session.MarkerSpawnRoot : null;

        public bool TryGetViewer(out Vector3 wallPosition)
        {
            var cam = Camera.main;
            var root = Root;
            wallPosition = cam != null && root != null ? root.InverseTransformPoint(cam.transform.position) : Vector3.zero;
            return cam != null && root != null;
        }

        public float MarkerDiameterOf(POIData poi) =>
            (MarkerHierarchyResolver.TryResolveByKey(poi?.hierarchy_level_key, out var style) ? style.SizeCm : MarkerHierarchyResolver.Fallback.SizeCm) / 100f;

        public void Dispose()
        {
            if (_tracker == null) return;
            _tracker.OnWallLocalised -= OnLocalised;
            _tracker.OnTrackingLost -= OnLost;
        }

        private void OnLocalised(Pose _) => LocalisationChanged?.Invoke();

        private void OnLost() => LocalisationChanged?.Invoke();
    }
}
