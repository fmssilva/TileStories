using UnityEngine;

namespace TileStories
{
    // Rotates the marker to always face the camera (screen-aligned billboard).
    // Attached to the POI_Marker root so the entire marker (symbol + label) rotates
    // as one rigid visual block.
    public class MarkerBillboard : MonoBehaviour
    {
        private Camera _camera;

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

        private void Update()
        {
            if (_camera == null) _camera = Camera.main;
            if (_camera != null)
                transform.rotation = _camera.transform.rotation;
        }

        private void LateUpdate()
        {
            if (_camera == null) _camera = Camera.main;
            if (_camera != null)
                transform.rotation = _camera.transform.rotation;
        }
    }
}
