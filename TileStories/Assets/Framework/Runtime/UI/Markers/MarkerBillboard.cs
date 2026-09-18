using System;
using UnityEngine;

namespace TileStories
{
    // Orients the marker root per OrientationSettings (_2.1_Marker_Orientation.md).
    // Attached to the POI_Marker root so the whole marker rotates as one rigid block,
    // with its Label/Badge children (MarkerChildOrientation) optionally counter-rotating
    // on top. DO NOT RENAME this class - POI_Marker.prefab references it by script GUID.
    public class MarkerBillboard : MonoBehaviour
    {
        private Camera _camera;

        private OrientationSettings _settings = new();
        private string _modeOverride = "";
        private Transform _spawnRoot;
        private Quaternion _authoredLocalRotation = Quaternion.identity;
        private MarkerChildOrientation[] _children = Array.Empty<MarkerChildOrientation>();

        private float _snappedRollDeg;
        private float _lastUpdateTime;
        private Vector3 _lastCamForward;
        private bool _hasLastCamForward;

        // Test-only visibility into the settings this marker was actually configured
        // with, so integration tests can assert the real WallSession -> Configure
        // composition without reaching for reflection on a private field.
        internal OrientationSettings ConfiguredSettings => _settings;
        internal string ConfiguredModeOverride => _modeOverride;

        // The ONE entry point for driving this marker's orientation. Called by
        // WallSession.SpawnPOIs at runtime, by the Editor rig path (section 14), and by
        // the orientation gallery harness/tests. Never called per frame. Snapshots the
        // marker's current localRotation as the "authored" rotation wall_fixed mode uses.
        public void Configure(OrientationSettings settings, string modeOverride, Transform spawnRoot)
        {
            _settings = settings ?? new OrientationSettings();
            _modeOverride = modeOverride ?? "";
            _spawnRoot = spawnRoot;
            _authoredLocalRotation = transform.localRotation;

            _children = GetComponentsInChildren<MarkerChildOrientation>(true);
            foreach (var child in _children)
            {
                if (child.gameObject.name == "Label")
                    child.Configure(_settings.label_orientation_mode, "inherit");
                else if (child.gameObject.name == "Badge")
                    child.Configure(_settings.badge_orientation_mode, _settings.badge_corner_mode);
            }
        }

        private void Awake()
        {
            _camera = Camera.main;
            if (_camera == null)
            {
                Debug.LogWarning("[Marker] MarkerBillboard found no Main Camera - disabling");
                enabled = false;
                return;
            }

            // World Space Canvases require an explicit worldCamera for proper screen-space
            // calculations. Set it to the same camera we billboard toward.
            var canvas = GetComponentInChildren<UnityEngine.Canvas>(true);
            if (canvas != null && canvas.worldCamera == null)
            {
                canvas.worldCamera = _camera;
            }
        }

        // LateUpdate only - the camera's final pose for the frame is only known after
        // every Update has run, so writing rotation in Update reads last frame's camera
        // pose and produces a one-frame lag that shows as swimming markers. Do not add
        // an Update() write back in (_2.1_Marker_Orientation.md section 4.4).
        private void LateUpdate()
        {
            if (_camera == null) _camera = Camera.main;
            if (_camera == null) return;

            float cameraAngleDeltaDeg = _hasLastCamForward ? Vector3.Angle(_lastCamForward, _camera.transform.forward) : 0f;
            float secondsSinceLastUpdate = Time.time - _lastUpdateTime;
            if (!MarkerOrientationResolver.ShouldUpdate(_settings, secondsSinceLastUpdate, cameraAngleDeltaDeg))
                return; // nothing moved enough to justify re-resolving; children are not ticked either

            _lastUpdateTime = Time.time;
            _lastCamForward = _camera.transform.forward;
            _hasLastCamForward = true;

            // Derive up from the camera's projection rather than cam.transform.up, so
            // this stays correct whether device screen rotation is compensated in the
            // camera transform or in the display matrix (_2.1_Marker_Orientation.md section 3).
            Vector3 screenUp = MarkerOrientationResolver.ScreenUpWorld(_camera);
            Vector3 upReference = MarkerOrientationResolver.ResolveUpReference(_settings, _spawnRoot);
            Quaternion parentRotation = transform.parent != null ? transform.parent.rotation : Quaternion.identity;

            var result = MarkerOrientationResolver.ResolveRootRotation(
                _settings, _modeOverride, transform.position, _camera.transform.position, _camera.transform.forward,
                screenUp, upReference, parentRotation, _authoredLocalRotation, _snappedRollDeg, ScreenOrientationSource.Current);

            if (result.Resolved)
            {
                _snappedRollDeg = result.SnappedRollDeg;
                transform.rotation = _settings.rotation_smoothing_time_s > 0f
                    ? Quaternion.Slerp(transform.rotation, result.Rotation, 1f - Mathf.Exp(-Time.deltaTime / _settings.rotation_smoothing_time_s))
                    : result.Rotation;
            }
            // else: degenerate input this frame - keep the previous rotation unchanged.

            // Root rotation for this frame is final before any child measures its roll
            // against it - call children explicitly rather than relying on execution order.
            foreach (var child in _children)
                child.Tick(transform.rotation, screenUp, upReference);
        }
    }
}
