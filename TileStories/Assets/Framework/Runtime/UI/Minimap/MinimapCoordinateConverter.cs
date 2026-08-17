using UnityEngine;

namespace TileStories
{
    // Pure logic for converting normalized wall coordinates to minimap pixel positions.
    // Extracted from MinimapView so it can be Tier-0 tested without a scene or
    // UI Toolkit instance (spec _2.6 section 8). Follows the same pattern as
    // MarkerLayout / StatusRamp -- stateless, deterministic, no MonoBehaviour.
    public static class MinimapCoordinateConverter
    {
        // Convert a normalized wall coordinate (0..1) to a minimap pixel position.
        // Returns position relative to the minimap background's top-left corner.
        // Y is inverted because UI Y grows downward but wall coordinates use
        // bottom-left origin (matching the captured_position / texture convention).
        public static Vector2 ConvertToPixel(float xNorm, float yNorm, float widthPx, float heightPx, float elementSizePx = 20f)
        {
            float x = xNorm * widthPx;
            float y = (1f - yNorm) * heightPx;
            // Center the element on the pixel point
            return new Vector2(x - elementSizePx / 2f, y - elementSizePx / 2f);
        }

        // Same conversion but returns the raw (un-centered) position for cases
        // where the caller positions the element's center rather than its corner.
        public static Vector2 ConvertToPixelRaw(float xNorm, float yNorm, float widthPx, float heightPx)
        {
            float x = xNorm * widthPx;
            float y = (1f - yNorm) * heightPx;
            return new Vector2(x, y);
        }

        // Clamp a normalized coordinate to [0, 1]. Guards against malformed
        // config data that places a POI outside the wall bounds.
        public static float ClampNorm(float value)
        {
            return Mathf.Clamp01(value);
        }
    }
}
