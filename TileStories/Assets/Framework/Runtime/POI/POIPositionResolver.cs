using UnityEngine;

namespace TileStories
{
    // Resolves a POI's 3D position under the XR Space parent:
    //   - If captured_position is set, use it directly as localPosition.
    //   - Otherwise, interpolate x_norm/y_norm between calibration anchors.
    public static class POIPositionResolver
    {
        public static bool TryResolvePosition(POIData poi, CalibrationAnchor[] calibrationAnchors, out Vector3 localPosition, bool logErrors = true)
        {
            localPosition = Vector3.zero;

            if (poi == null)
            {
                if (logErrors)
                    Debug.LogError("[POIPositionResolver] POI data is null.");
                return false;
            }

// Use captured_position if explicitly marked as present (has_captured_position is the
            // authoritative signal; captured_position != null is a defensive backup check)
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

            // Fallback: project x_norm/y_norm onto the wall surface via calibration anchors
            // x_norm/y_norm are % positions on a flat reference photo (0,0 = bottom-left, 1,1 = top-right)
            if (float.IsNaN(poi.x_norm) || float.IsNaN(poi.y_norm) ||
                float.IsInfinity(poi.x_norm) || float.IsInfinity(poi.y_norm))
            {
                if (logErrors)
                    Debug.LogError($"[POIPositionResolver] POI '{poi.id}' has invalid x_norm/y_norm ({poi.x_norm}, {poi.y_norm}).");
                return false;
            }

            float xn = Mathf.Clamp01(poi.x_norm);
            float yn = Mathf.Clamp01(poi.y_norm);

            if (calibrationAnchors != null && calibrationAnchors.Length >= 2)
            {
                // Walk along the wall between known anchor points, then offset vertically by yn
                localPosition = InterpolateBetweenAnchors(xn, yn, calibrationAnchors);
            }
            else
            {
                // No anchors at all — flat 4x3m plane as a rough default
                const float defaultWallWidth = 4f;
                const float defaultWallHeight = 3f;
                localPosition = new Vector3(
                    (xn - 0.5f) * defaultWallWidth,
                    (yn - 0.5f) * defaultWallHeight,
                    0.5f
                );
            }

            return true;
        }

        // Sort anchors by x_norm, find the two bracketing this POI, lerp between them,
        // then add a vertical offset proportional to yn and the wall's estimated height
        private static Vector3 InterpolateBetweenAnchors(float xn, float yn, CalibrationAnchor[] anchors)
        {
            var sorted = new CalibrationAnchor[anchors.Length];
            System.Array.Copy(anchors, sorted, anchors.Length);
            System.Array.Sort(sorted, (a, b) => a.x_norm.CompareTo(b.x_norm));

            CalibrationAnchor left = sorted[0];
            CalibrationAnchor right = sorted[sorted.Length - 1];

            for (int i = 0; i < sorted.Length - 1; i++)
            {
                if (sorted[i].x_norm <= xn && sorted[i + 1].x_norm >= xn)
                {
                    left = sorted[i];
                    right = sorted[i + 1];
                    break;
                }
            }

            float range = right.x_norm - left.x_norm;
            float t = range > 0.0001f ? (xn - left.x_norm) / range : 0f;
            t = Mathf.Clamp01(t);

            Vector3 basePos = Vector3.Lerp(left.captured_position.ToVector3(), right.captured_position.ToVector3(), t);

            // IMPORTANT ASSUMPTION: this Y-axis calculation assumes calibration anchors are
            // captured near the wall's vertical center (y_norm approximately 0.5). It does not
            // use each anchor's own y_norm value to correct for anchors placed at other
            // heights. LivingRoom's current anchors are both at y_norm=0.5, so this works
            // correctly here, but this is NOT enforced or validated anywhere - a future wall
            // (e.g. Chafariz, where anchors will exist at panel joins across the wall's full
            // height range, not just its center) will need this logic revisited before it can
            // be integrated correctly. See _1.2_final_review.md Section 4, Task 4.3.
            float wallHeight = EstimateWallHeight(sorted);
            float yOffset = (yn - 0.5f) * wallHeight;

            return basePos + new Vector3(0f, yOffset, 0f);
        }

        // Estimate wall height from vertical spread of calibration anchors, default 3m if too few or all at same height
        private static float EstimateWallHeight(CalibrationAnchor[] sorted)
        {
            if (sorted.Length < 2) return 3f;

            float minY = float.MaxValue;
            float maxY = float.MinValue;
            foreach (var a in sorted)
            {
                if (a.captured_position.y < minY) minY = a.captured_position.y;
                if (a.captured_position.y > maxY) maxY = a.captured_position.y;
            }

            float spread = maxY - minY;
            return spread > 0.1f ? spread : 3f;
        }
    }

}