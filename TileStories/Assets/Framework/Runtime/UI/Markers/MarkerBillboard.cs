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
            }
        }

         private void LateUpdate()
         {
             if (_camera == null) return;

             // Face the camera directly without additional rotation to prevent text mirroring
             transform.rotation = _camera.transform.rotation;
         }
    }
}
