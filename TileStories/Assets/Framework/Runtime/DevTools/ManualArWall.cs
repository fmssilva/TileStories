using System;
using UnityEngine;

namespace TileStories
{
    // A stand-in IArWall (_3.1 step 10B.1) for the card gallery and tests: a frame of its own at the world origin, localised or not as the
    // caller says, an optional viewer and a fixed marker size -- the same seat ManualPreviewStage takes for previews. The wall scene uses
    // WallArSurface.
    public sealed class ManualArWall : IArWall, IDisposable
    {
        private bool _localised;
        private GameObject _root;

        public event Action LocalisationChanged;

        public ManualArWall(bool localised = true)
        {
            _localised = localised;
        }

        public bool IsLocalised
        {
            get => _localised;
            set
            {
                if (_localised == value) return;
                _localised = value;
                LocalisationChanged?.Invoke();
            }
        }

        public Transform Root
        {
            get
            {
                if (_root == null) _root = new GameObject("ManualArWall");
                return _root.transform;
            }
        }

        // Where the viewer stands in the wall frame (null: unknown)
        public Vector3? Viewer { get; set; }

        public bool TryGetViewer(out Vector3 wallPosition)
        {
            wallPosition = Viewer ?? Vector3.zero;
            return Viewer.HasValue;
        }

        // Every POI's marker diameter in the wall frame
        public float MarkerDiameter { get; set; } = 0.1f;

        public float MarkerDiameterOf(POIData poi) => MarkerDiameter;

        public void Dispose()
        {
            if (_root == null) return;
            if (Application.isPlaying) UnityEngine.Object.Destroy(_root);
            else UnityEngine.Object.DestroyImmediate(_root);
            _root = null;
        }
    }
}
