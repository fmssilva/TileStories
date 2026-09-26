using System.Collections.Generic;
using UnityEngine;

namespace TileStories
{
    // Lives on the effects grid root (EffectsPreviewSpawner). Owns the dedicated grid camera and keeps
    // it consistent with the main camera: same field of view, render target and viewport, drawn on top
    // (higher depth) and clearing to a neutral colour, so the "focus" view is just the grid whatever the
    // wall or its scenery look like. The grid root also copies the main camera's ROTATION each frame, so
    // the markers (which face the camera) and the layout stay upright on screen when a phone rolls.
    // When the aspect ratio or field of view changes (a resized Game view, a rotated phone, AR zoom)
    // the column count and camera distance are recomputed so the whole grid stays on screen. All the
    // maths is in EffectsPreviewSpawner; this class only applies it.
    public class EffectsPreviewFocus : MonoBehaviour
    {
        private Camera _source;
        private Camera _view;
        private List<Transform> _cells;
        private int[] _blockCounts;
        private float _lastAspect;
        private float _lastFov;
        private float _autoDistance;
        private Vector2 _autoExtent;
        private readonly DevPreviewCameraDolly _dolly = new();

        // The grid camera (tests and tools read it; nothing else should).
        public Camera ViewCamera => _view;

        // Columns of the current layout (test visibility).
        public int Columns { get; private set; }

        // The manual scroll-wheel zoom (test visibility; a fresh grid always starts at zero).
        public DevPreviewCameraDolly Dolly => _dolly;

        // Attach the grid camera to the given main camera and lay the cells out. blockCounts is one
        // entry per stacked block (e.g. quick row, combo row, level row for the effects grid), each
        // starting its own row.
        public void Begin(Camera source, List<Transform> cells, params int[] blockCounts)
        {
            _source = source;
            _cells = cells;
            _blockCounts = blockCounts;

            var go = new GameObject("EffectsPreviewCamera");
            _view = go.AddComponent<Camera>();
            _view.clearFlags = CameraClearFlags.SolidColor;
            _view.backgroundColor = EffectsPreviewSpawner.BackgroundColor;
            _view.cullingMask = ~0;
            _view.nearClipPlane = 0.05f;
            _view.allowHDR = false;
            _view.allowMSAA = false;
            go.transform.SetParent(transform, false);

            Refit(force: true);
            ApplyDistance();
        }

        private void Update()
        {
            if (_source == null || _view == null) return;

            // Same rotation as the main camera: cells face it and the layout stays upright on screen.
            // This is NOT overridden by free-look input -- see DevPreviewCameraDolly's class comment
            // for why (every cell's MarkerBillboard always faces Camera.main, so this grid camera's
            // rotation must keep matching it, never rotate independently).
            transform.rotation = _source.transform.rotation;

            if (Application.isPlaying)
            {
                var mouse = UnityEngine.InputSystem.Mouse.current;
                // Sign only, not the raw magnitude: the Input System's scroll delta is scaled
                // differently per platform/OS setting (a Windows wheel notch can read as ~120, other
                // platforms much smaller), so MetresPerScrollClick calibrates against "one tick",
                // not an unpredictable raw unit. Mathf.Sign(0) returns +1, not 0, so a genuine
                // no-scroll frame must be checked for explicitly -- otherwise the grid would creep
                // closer every single frame even with the mouse untouched.
                float scrollY = mouse != null ? mouse.scroll.ReadValue().y : 0f;
                if (!Mathf.Approximately(scrollY, 0f))
                    _dolly.ApplyScroll(Mathf.Sign(scrollY));

                // WASD/RMB-drag/Alt+LMB-drag/arrows pan the view (DevCameraInput, gated on the mouse
                // being over the Game view so this never also scrolls/pans the POI Editor window).
                // A/D and left/right-drag pan sideways, Q/E pan up/down, W/S zoom (the same channel
                // scrolling uses) -- there is no independent look/roll here, see the class comment.
                var input = DevCameraInput.ReadThisFrame();
                Vector2 pan = input.LookDelta * DevPreviewCameraDolly.PanMetresPerMouseDragPixel
                    + new Vector2(input.MoveDelta.x, input.MoveDelta.y) * DevPreviewCameraDolly.PanMetresPerSecond * Time.deltaTime;
                if (pan != Vector2.zero)
                    _dolly.ApplyPan(pan);
                if (input.MoveDelta.z != 0f)
                    _dolly.ApplyZoomDelta(input.MoveDelta.z * DevPreviewCameraDolly.PanMetresPerSecond * Time.deltaTime);
            }

            Refit(force: false);
            ApplyDistance();
        }

        // Recompute the layout (columns, cell positions, auto-fit distance) for the main camera's
        // current aspect and field of view. Cached: skipped when neither changed, since it is the
        // expensive half (repositions every cell).
        private void Refit(bool force)
        {
            float aspect = _source.aspect, fov = _source.fieldOfView;
            _view.targetTexture = _source.targetTexture;
            _view.rect = _source.rect;
            _view.depth = _source.depth + 100f;
            _view.fieldOfView = fov;
            if (!force && Mathf.Approximately(aspect, _lastAspect) && Mathf.Approximately(fov, _lastFov)) return;
            _lastAspect = aspect;
            _lastFov = fov;

            Columns = EffectsPreviewSpawner.ChooseColumns(_blockCounts, fov, aspect);
            var positions = EffectsPreviewSpawner.CellPositions(_blockCounts, Columns);
            for (int i = 0; i < _cells.Count; i++)
                _cells[i].localPosition = new Vector3(positions[i].x, positions[i].y, 0f);

            _autoExtent = EffectsPreviewSpawner.GridExtent(_blockCounts, Columns);
            _autoDistance = EffectsPreviewSpawner.FitDistance(_autoExtent, fov, aspect);
        }

        // Cheap: applies the dolly's manual zoom/pan on top of the (possibly cached) auto-fit
        // framing. Runs every frame so a scroll/pan alone, with nothing else changing, still moves
        // the camera.
        private void ApplyDistance()
        {
            float distance = _dolly.ApplyZoom(_autoDistance);
            Vector2 pan = _dolly.ClampedPan(_autoExtent);
            _view.farClipPlane = Mathf.Max(_autoDistance, distance) + 10f;
            _view.transform.localPosition = new Vector3(pan.x, pan.y, -distance);
            _view.transform.localRotation = Quaternion.identity;
        }
    }
}
