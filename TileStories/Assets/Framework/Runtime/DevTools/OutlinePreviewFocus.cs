using System.Collections.Generic;
using UnityEngine;

namespace TileStories
{
    // Lives on the outline grid root (OutlinePreviewSpawner). Same shape as EffectsPreviewFocus: owns
    // a dedicated grid camera kept consistent with the main camera (field of view, render target,
    // viewport), drawn on top and clearing to a neutral colour, so the grid reads the same whatever
    // the wall or its scenery look like; copies the main camera's rotation each frame so the cells
    // (which face the camera) stay upright on screen when a phone rolls. The fitting math is shared
    // with the effects grid via DevPreviewGridLayout.
    public class OutlinePreviewFocus : MonoBehaviour
    {
        private Camera _source;
        private Camera _view;
        private List<Transform> _cells;
        private int _modeCount;
        private int _levelCount;
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

        // Attach the grid camera to the given main camera and lay the cells out.
        public void Begin(Camera source, List<Transform> cells, int modeCount, int levelCount)
        {
            _source = source;
            _cells = cells;
            _modeCount = modeCount;
            _levelCount = levelCount;

            var go = new GameObject("OutlinePreviewCamera");
            _view = go.AddComponent<Camera>();
            _view.clearFlags = CameraClearFlags.SolidColor;
            _view.backgroundColor = OutlinePreviewSpawner.BackgroundColor;
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

            // Same rotation as the main camera: NOT overridden by free-look input -- see
            // DevPreviewCameraDolly's class comment (every cell's MarkerBillboard always faces
            // Camera.main, so this grid camera's rotation must keep matching it).
            transform.rotation = _source.transform.rotation;

            if (Application.isPlaying)
            {
                var mouse = UnityEngine.InputSystem.Mouse.current;
                // Sign only (platform-dependent raw scale) and explicitly guarded against
                // Mathf.Sign(0) == +1, same reasoning as EffectsPreviewFocus.
                float scrollY = mouse != null ? mouse.scroll.ReadValue().y : 0f;
                if (!Mathf.Approximately(scrollY, 0f))
                    _dolly.ApplyScroll(Mathf.Sign(scrollY));

                // WASD/RMB-drag/Alt+LMB-drag/arrows pan the view, W/S zoom, same as the effects
                // grid -- see DevCameraInput and DevPreviewCameraDolly.
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
        // current aspect and field of view. Cached: skipped when neither changed.
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

            Columns = DevPreviewGridLayout.ChooseColumns(_modeCount, _levelCount, fov, aspect);
            var positions = DevPreviewGridLayout.CellPositions(_modeCount, _levelCount, Columns);
            for (int i = 0; i < _cells.Count; i++)
                _cells[i].localPosition = new Vector3(positions[i].x, positions[i].y, 0f);

            _autoExtent = DevPreviewGridLayout.GridExtent(_modeCount, _levelCount, Columns);
            _autoDistance = DevPreviewGridLayout.FitDistance(_autoExtent, fov, aspect);
        }

        // Cheap: applies the dolly's manual zoom/pan on top of the (possibly cached) auto-fit framing.
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
