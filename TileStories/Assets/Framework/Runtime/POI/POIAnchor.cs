using UnityEngine;

namespace TileStories
{
    /// <summary>
    /// Data anchor for one POI in world space.
    /// Visual marker and interaction components are attached separately.
    /// </summary>
    public class POIAnchor : MonoBehaviour
    {
        public POIData Data { get; private set; }

        public void Initialise(POIData data)
        {
            Data = data;
            gameObject.name = $"POI_{data.id}";
        }
    }
}
