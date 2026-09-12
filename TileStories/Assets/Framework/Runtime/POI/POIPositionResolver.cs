using UnityEngine;

namespace TileStories
{
    // Resolves a POI's 3D position under the XR Space parent:
    //   - If captured_position is set, use it directly as localPosition.
    //   - Otherwise, return origin (0,0,0) as a safe fallback.
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

            if (poi.has_captured_position && poi.captured_position != null)
            {
                var cp = poi.captured_position;
                if (float.IsNaN(cp.x) || float.IsNaN(cp.y) || float.IsNaN(cp.z) ||
                    float.IsInfinity(cp.x) || float.IsInfinity(cp.y) || float.IsInfinity(cp.z))
                {
                    if (logErrors)
                        Debug.LogError($"[POIPositionResolver] POI '{poi.id}' has invalid captured_position ({cp.x}, {cp.y}, {cp.z}).");
                    return false;
                }

                localPosition = new Vector3(cp.x, cp.y, cp.z);
                return true;
            }

            // No captured position - fallback to origin (0,0,0) as a safe default.
            // This keeps markers from vanishing if a capture is missing.
            localPosition = Vector3.zero;
            return true;
        }
    }
}