using UnityEngine;

namespace TileStories
{
    // Resolves a POI's 3D position: non-null position is used directly,
    // otherwise falls back to origin. position_verified is never consulted here.
    public static class POIPositionResolver
    {
        public static bool TryResolvePosition(POIData poi, out Vector3 localPosition, bool logErrors = true)
        {
            localPosition = Vector3.zero;

            if (poi == null)
            {
                if (logErrors)
                    Debug.LogError("[POIPositionResolver] POI data is null.");
                return false;
            }

            if (poi.position != null)
            {
                var p = poi.position;
                if (float.IsNaN(p.x) || float.IsNaN(p.y) || float.IsNaN(p.z) ||
                    float.IsInfinity(p.x) || float.IsInfinity(p.y) || float.IsInfinity(p.z))
                {
                    if (logErrors)
                        Debug.LogError($"[POIPositionResolver] POI '{poi.id}' has invalid position ({p.x}, {p.y}, {p.z}).");
                    return false;
                }

                localPosition = new Vector3(p.x, p.y, p.z);
                return true;
            }

            localPosition = Vector3.zero;
            return true;
        }
    }
}