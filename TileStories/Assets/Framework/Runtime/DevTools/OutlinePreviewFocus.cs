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

        // The grid camera (tests and tools read it; nothing else should).
        public Camera ViewCamera => _view;

        // Columns of the current layout (test visibility).
        public int Columns { get; private set; }

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
        }

        private void Update()
        {
            if (_source == null || _view == null) return;

            transform.rotation = _source.transform.rotation;
            Refit(force: false);
        }

        private void Refit(bool force)
        {
            float aspect = _source.aspect, fov = _source.fieldOfView;
            _view.targetTexture = _source.targetTexture;
            _view.rect = _source.rect;
            _view.depth = _source.depth + 100f;
            if (!force && Mathf.Approximately(aspect, _lastAspect) && Mathf.Approximately(fov, _lastFov)) return;
            _lastAspect = aspect;
            _lastFov = fov;

            Columns = DevPreviewGridLayout.ChooseColumns(_modeCount, _levelCount, fov, aspect);
            var positions = DevPreviewGridLayout.CellPositions(_modeCount, _levelCount, Columns);
            for (int i = 0; i < _cells.Count; i++)
                _cells[i].localPosition = new Vector3(positions[i].x, positions[i].y, 0f);

            float distance = DevPreviewGridLayout.FitDistance(
                DevPreviewGridLayout.GridExtent(_modeCount, _levelCount, Columns), fov, aspect);
            _view.fieldOfView = fov;
            _view.farClipPlane = distance + 10f;
            _view.transform.localPosition = new Vector3(0f, 0f, -distance);
            _view.transform.localRotation = Quaternion.identity;
        }
    }
}
