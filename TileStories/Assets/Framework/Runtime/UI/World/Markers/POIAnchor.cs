using UnityEngine;

namespace TileStories
{
    // Data holder attached to each spawned POI marker.
    // Stores the POIData reference so other components (MarkerView, tap handler) can read it.
    public class POIAnchor : MonoBehaviour
    {
        public POIData Data { get; private set; }

        public void Initialise(POIData data)
        {
            Data = data;
        }
    }
}