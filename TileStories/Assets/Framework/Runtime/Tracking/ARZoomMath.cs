using UnityEngine;
using System.Collections.Generic;

namespace TileStories
{
    /// <summary>
    /// Pure math helpers for zoom gestures (pinch and double-tap) and tap-button cycling.
    /// All methods are static and side-effect free, making them suitable for Tier-0 unit tests.
    /// </summary>
    public static class ARZoomMath
    {
        // Discrete tap levels for the tap-button cycle. Base (1.0) is always first;
        // then `tapLevels` multiplied copies of step, clamped, collapsed to a
        // strictly ascending, distinct, base-first sequence.
        // Example: step=1.5, tapLevels=2, min=1, max=4 -> [1.0, 1.5, 2.25].
        public static float[] ComputeTapLevels(float step, int tapLevels, float min, float max)
        {
            if (tapLevels < 0) tapLevels = 0;
            if (step <= 0f) step = 1f; // guard: non-positive step must not produce NaN/inf

            var levels = new List<float> { 1.0f };
            float acc = 1.0f;
            for (int i = 1; i <= tapLevels; i++)
            {
                acc *= step;
                float clamped = Mathf.Clamp(acc, min, max);
                // keep strictly ascending and distinct from the previous level
                if (clamped - levels[levels.Count - 1] > 1e-5f)
                    levels.Add(clamped);
            }
            return levels.ToArray();
        }

        // Advance one discrete level. With wrap=true (single tap) it cycles past the
        // last level back to base; with wrap=false (+ button) it caps at the top level.
        public static float NextTapLevel(float currentZoom, float step, int tapLevels, float min, float max, bool wrap = true)
        {
            float[] levels = ComputeTapLevels(step, tapLevels, min, max);
            if (levels.Length == 0) return Mathf.Clamp(currentZoom, min, max);

            // snap current to the nearest known level to avoid drift accumulation
            int bestIdx = 0;
            float bestDelta = Mathf.Abs(levels[0] - currentZoom);
            for (int i = 1; i < levels.Length; i++)
            {
                float d = Mathf.Abs(levels[i] - currentZoom);
                if (d < bestDelta) { bestDelta = d; bestIdx = i; }
            }
            int next = bestIdx + 1;
            if (next >= levels.Length)
            {
                // reached the top: wrap back to base (tap), or hold at max (button)
                return wrap ? levels[0] : levels[levels.Length - 1];
            }
            return levels[next];
        }

        // Retreat one discrete level for the zoom-out button. Never wraps -- it floors
        // at the base level so repeated presses stay at 1x instead of snapping home.
        public static float PreviousTapLevel(float currentZoom, float step, int tapLevels, float min, float max)
        {
            float[] levels = ComputeTapLevels(step, tapLevels, min, max);
            if (levels.Length == 0) return Mathf.Clamp(currentZoom, min, max);

            // snap current to the nearest known level to avoid drift accumulation
            int bestIdx = 0;
            float bestDelta = Mathf.Abs(levels[0] - currentZoom);
            for (int i = 1; i < levels.Length; i++)
            {
                float d = Mathf.Abs(levels[i] - currentZoom);
                if (d < bestDelta) { bestDelta = d; bestIdx = i; }
            }
            int prev = bestIdx - 1;
            if (prev < 0) prev = 0; // floor at base
            return levels[prev];
        }

        // Double-tap toggles between base (1.0) and max. Assumption: "base" == min.
        public static float NextDoubleTapTarget(float currentZoom, float min, float max)
        {
            bool nearBase = Mathf.Abs(currentZoom - 1.0f) < 1e-3f;
            return nearBase ? max : 1.0f;
        }

        // Pinch: multiply current zoom factor, then clamp to [min, max].
        public static float ApplyPinchScale(float currentZoom, float scaleFactor, float min, float max)
        {
            return Mathf.Clamp(currentZoom * scaleFactor, min, max);
        }

        // Frame-step toward a target. Reaches the target in roughly
        // transitionSpeedSeconds at low framerate; higher framerate approaches
        // asymptotically with no overshoot. Clamped to the target to stop jitter.
        public static float StepTowardTarget(float from, float to, float transitionSpeedSeconds, float deltaTime)
        {
            if (Mathf.Approximately(from, to)) return to;
            if (transitionSpeedSeconds <= 0f) return to; // instant
            float t = Mathf.Clamp01(deltaTime / transitionSpeedSeconds);
            float next = Mathf.Lerp(from, to, t);
            if (Mathf.Abs(next - to) < 1e-4f) return to;
            return next;
        }

        // A quick helper pair for the gesture driver (ARZoomGestureInput) --
        // pure timing/space checks, Tier-0 testable without real touch hardware.

        // A double-tap = two single taps that land close in time and space: the
        // second must arrive within windowSeconds of the first AND within
        // moveTolerancePx of its position (a quick tap then a finger running across
        // the screen is a drag, not a double-tap).
        public static bool IsDoubleTapOK(float timeSinceLastTap, Vector2 posDelta,
            float windowSeconds, float moveTolerancePx)
        {
            // Guard against non-positive windows: they must never accept a real tap.
            if (windowSeconds <= 0f)
                return false;

            return timeSinceLastTap <= windowSeconds && posDelta.magnitude <= moveTolerancePx;
        }

        // Pinch scale factor for a frame: current/previous two-finger distance.
        // prevDist <= 0 means no pinch in flight yet -> no change this frame.
        public static float PinchScaleForDelta(float prevDist, float curDist)
        {
            return prevDist > 0f ? curDist / prevDist : 1f;
        }
    }
}