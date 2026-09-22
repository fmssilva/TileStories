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
        private int _effectCount;
        private int _levelCount;
        private float _lastAspect;
        private float _lastFov;

        // The grid camera (tests and tools read it; nothing else should).
        public Camera ViewCamera => _view;

        // Columns of the current layout (test visibility).
        public int Columns { get; private set; }

        // Attach the grid camera to the given main camera and lay the cells out.
        public void Begin(Camera source, List<Transform> cells, int effectCount, int levelCount)
        {
            _source = source;
            _cells = cells;
            _effectCount = effectCount;
            _levelCount = levelCount;

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
        }

        private void Update()
        {
            if (_source == null || _view == null) return;

            // Same rotation as the main camera: cells face it and the layout stays upright on screen.
            transform.rotation = _source.transform.rotation;
            Refit(force: false);
        }

        // Recompute the layout and the camera for the main camera's current aspect and field of view.
        private void Refit(bool force)
        {
            float aspect = _source.aspect, fov = _source.fieldOfView;
            _view.targetTexture = _source.targetTexture;
            _view.rect = _source.rect;
            _view.depth = _source.depth + 100f;
            if (!force && Mathf.Approximately(aspect, _lastAspect) && Mathf.Approximately(fov, _lastFov)) return;
            _lastAspect = aspect;
            _lastFov = fov;

            Columns = EffectsPreviewSpawner.ChooseColumns(_effectCount, _levelCount, fov, aspect);
            var positions = EffectsPreviewSpawner.CellPositions(_effectCount, _levelCount, Columns);
            for (int i = 0; i < _cells.Count; i++)
                _cells[i].localPosition = new Vector3(positions[i].x, positions[i].y, 0f);

            float distance = EffectsPreviewSpawner.FitDistance(
                EffectsPreviewSpawner.GridExtent(_effectCount, _levelCount, Columns), fov, aspect);
            _view.fieldOfView = fov;
            _view.farClipPlane = distance + 10f;
            _view.transform.localPosition = new Vector3(0f, 0f, -distance);
            _view.transform.localRotation = Quaternion.identity;
        }
    }
}
