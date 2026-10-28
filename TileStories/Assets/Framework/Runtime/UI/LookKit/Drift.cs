using UnityEngine;

namespace TileStories.LookKit
{
    // The Drift's options (v6: the divider's tiles sliding slowly sideways, 400 px every 40 s): its speed in panel units a second, Reduce Motion
    public sealed class DriftOptions
    {
        public float Speed = 10f;
        public bool ReduceMotion;
    }

    // The Drift, pure: how far a pattern has slid `seconds` after it started -- a pattern that repeats every `period` wraps, so it loops
    // without a seam. Reduce Motion = still
    public static class DriftRule
    {
        public static float Offset(float seconds, float period, DriftOptions o) =>
            o.ReduceMotion || period <= 0f ? 0f : Mathf.Repeat(Mathf.Max(0f, seconds) * o.Speed, period);
    }
}
