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
        // Always-on smoothing against AR pose jitter. Not developer-exposed
        // (_2.1_Marker_Orientation.md v4: "just implement it to work normally and
        // smooth" -- v3's exposed 0=instant toggle was unnecessary complexity).
        private const float RotationSmoothingTimeS = 0.12f;

        private Camera _camera;

        private OrientationSettings _settings = new();
        private string _facingModeOverride = "";
        private Transform _spawnRoot;
        private Quaternion _authoredLocalRotation = Quaternion.identity;
        private MarkerChildOrientation[] _children = Array.Empty<MarkerChildOrientation>();

        private float _lastUpdateTime;
        private Vector3 _lastCamForward;
        private bool _hasLastCamForward;
        private bool _hasResolvedOnce;
        private bool _forceNextResolve;

        // Test-only visibility into the settings this marker was actually configured
        // with, so integration tests can assert the real WallSession -> Configure
        // composition without reaching for reflection on a private field.
        internal OrientationSettings ConfiguredSettings => _settings;
        internal string ConfiguredModeOverride => _facingModeOverride;

        // The ONE entry point for driving this marker's orientation. Called by
        // WallSession.SpawnPOIs at runtime, by the Editor rig path (section 14), and by
        // the orientation gallery harness/tests. Never called per frame. Snapshots the
        // marker's current localRotation as the "authored" rotation wall_fixed/yaw_only use.
        public void Configure(OrientationSettings settings, string facingModeOverride, Transform spawnRoot)
        {
            _settings = settings ?? new OrientationSettings();
            _facingModeOverride = facingModeOverride ?? "";
            _spawnRoot = spawnRoot;
            _authoredLocalRotation = transform.localRotation;
            // Re-Configure (e.g. a pooled cluster view reused for a different aggregate)
            // must snap to the new correct rotation immediately, not smooth in from
            // whatever rotation this object happened to have from its previous use.
            _hasResolvedOnce = false;

            ConfigureChildren();
        }

        // Swap in new settings on a marker that is already running (live Play Mode edits). Unlike
        // Configure it keeps the authored rotation (wall_fixed depends on it: the current rotation
        // is no longer the authored one), keeps the spawn root, and lets the rotation glide to the
        // new result instead of snapping.
        public void ReapplySettings(OrientationSettings settings, string facingModeOverride)
        {
            _settings = settings ?? new OrientationSettings();
            _facingModeOverride = facingModeOverride ?? "";
            ConfigureChildren();
            // interval / on_camera_delta would otherwise wait for the next tick or camera move
            _forceNextResolve = true;
        }

        // The authored rotation wall_fixed uses fully and yaw_only partially (X/Z)
        public Quaternion AuthoredLocalRotation => _authoredLocalRotation;

        // Swap in a new authored rotation on a running marker (a POI's Facing X/Y/Z edited live in
        // Play Mode). Configure cannot be reused for it: it would snapshot the CURRENT, already
        // camera-resolved rotation instead of the authored one. Glides in like ReapplySettings.
        public void SetAuthoredLocalRotation(Quaternion authoredLocalRotation)
        {
            _authoredLocalRotation = authoredLocalRotation;
            _forceNextResolve = true;
        }

        // Hand the Label and Badge children their own vertical-alignment mode
        private void ConfigureChildren()
        {
            _children = GetComponentsInChildren<MarkerChildOrientation>(true);
            foreach (var child in _children)
            {
                if (child.gameObject.name == "Label")
                    child.Configure(_settings.label_vertical_alignment_mode);
                else if (child.gameObject.name == "Badge")
                    child.Configure(_settings.badge_vertical_alignment_mode);
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
            if (!_forceNextResolve && !MarkerOrientationResolver.ShouldUpdate(_settings, secondsSinceLastUpdate, cameraAngleDeltaDeg))
                return; // nothing moved enough to justify re-resolving; children are not ticked either
            _forceNextResolve = false;

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
                _settings, _facingModeOverride, transform.position, _camera.transform.position, _camera.transform.forward,
                screenUp, upReference, parentRotation, _authoredLocalRotation);

            if (result.Resolved)
            {
                // The very first resolve after (re)Configure snaps immediately - a freshly
                // spawned or reused marker must already show its correct orientation on its
                // first visible frame, never visibly rotate in from an arbitrary starting
                // pose. Smoothing only applies to subsequent frames, against real camera
                // movement / AR pose jitter.
                transform.rotation = _hasResolvedOnce
                    ? Quaternion.Slerp(transform.rotation, result.Rotation, 1f - Mathf.Exp(-Time.deltaTime / RotationSmoothingTimeS))
                    : result.Rotation;
                _hasResolvedOnce = true;
            }
            // else: degenerate input this frame - keep the previous rotation unchanged.

            // Root rotation for this frame is final before any child measures its roll
            // against it - call children explicitly rather than relying on execution order.
            foreach (var child in _children)
                child.Tick(transform.rotation, screenUp, upReference);
        }
    }
}
