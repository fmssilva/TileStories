using UnityEngine;

namespace TileStories
{
    // Global, FOV-based AR camera zoom state. One system writes (ARZoomController),
    // all others read (LODController via effective distance) -- no tight coupling.
    // Mirrors the resolver pattern used everywhere else in this codebase.
    //
    // Spec §9: zoom is global (whole view scales), not a local magnifier.
    // Spec §10: effectiveDistance = realDistance / ZoomFactor.
    public static class ARZoomState
    {
        // Current zoom factor (1.0 = no zoom, 4.0 = 4x zoom). Default 1f.
        public static float ZoomFactor { get; private set; } = 1f;

        // Set zoom with hard clamp to [min, max]. Enforced here, not just in UI,
        // because an unclamped zoom drives effectiveDistance toward zero and
        // breaks every downstream LOD formula (spec §9, "Hard clamp").
        public static void SetZoom(float value, float min, float max)
        {
            ZoomFactor = Mathf.Clamp(value, min, max);
        }

        // Reset to no-zoom. Called by "fit to 1x" UI button and double-tap cycle.
        public static void ResetToBase(float min, float max)
        {
            SetZoom(1f, min, max);
        }
    }
}
