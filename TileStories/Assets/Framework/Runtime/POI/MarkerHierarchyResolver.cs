using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TileStories
{
    // Resolved visual style for a single hierarchy level. Readonly struct, like
    // StatusLevel -- value semantics so callers can't mutate a shared instance.
        public readonly struct HierarchyStyle
    {
        public readonly float SizeCm;
        public readonly bool ShowLabel;
        public readonly MarkerEffectFlags EffectFlags;
        public readonly bool RotateContour;
        public readonly float RevealDelaySeconds;
        public readonly float RevealDurationSeconds;

        public HierarchyStyle(float sizeCm, bool showLabel,
            MarkerEffectFlags effectFlags, bool rotateContour,
            float revealDelaySeconds, float revealDurationSeconds)
        {
            SizeCm = sizeCm;
            ShowLabel = showLabel;
            EffectFlags = effectFlags;
            RotateContour = rotateContour;
            RevealDelaySeconds = revealDelaySeconds;
            RevealDurationSeconds = revealDurationSeconds;
        }
    }

    // Wall-configurable hierarchy levels. Same pattern as StatusRamp: a static
    // class with Configure / ResetToDefaults / TryResolveByKey. Empty or missing
    // config means TryResolveByKey returns false and callers fall back to Fallback.
    public static class MarkerHierarchyResolver
    {
                // A 0cm marker is invisible, not gracefully degraded -- so the fallback
        // size matches today's prefab default (12cm) rather than 0. Every other
        // field defaults to the visually-inert value. 0.35s reveal duration matches
        // the original hardcoded default in MarkerRevealEffect.
        public static readonly HierarchyStyle Fallback =
            new HierarchyStyle(12f, false, MarkerEffectFlags.None, false, 0f, 0.35f);

        private static readonly HierarchyLevelEntry[] _defaultEntries =
            System.Array.Empty<HierarchyLevelEntry>();

        private static Dictionary<string, HierarchyStyle> _stylesByKey = new();

        // Parse an effect-mode string into MarkerEffectFlags. Local to this file
        // -- this is the only consumer of sun_effect/accent_effect parsing.
        private static MarkerEffectFlags ParseEffectString(string sunEffect, string accentEffect, bool pulse)
        {
            var flags = MarkerEffectFlags.None;

            switch (sunEffect)
            {
                case "sun_contours": flags |= MarkerEffectFlags.SunContours; break;
                case "sun_circles":  flags |= MarkerEffectFlags.SunCircles;  break;
                case "none":
                case null:
                case "":
                    break;
                default:
                    Debug.LogWarning($"[MarkerHierarchyResolver] Unknown sun_effect '{sunEffect}', ignoring.");
                    break;
            }

            switch (accentEffect)
            {
                case "ring_pulse":  flags |= MarkerEffectFlags.RingPulse;  break;
                case "simple_sun":  flags |= MarkerEffectFlags.SimpleSun;  break;
                case "beacon":      flags |= MarkerEffectFlags.Beacon;      break;
                case "none":
                case null:
                case "":
                    break;
                default:
                    Debug.LogWarning($"[MarkerHierarchyResolver] Unknown accent_effect '{accentEffect}', ignoring.");
                    break;
            }

            if (pulse) flags |= MarkerEffectFlags.Pulse;
            return flags;
        }

        public static void Configure(IEnumerable<HierarchyLevelEntry> entries)
        {
            _stylesByKey.Clear();

            if (entries == null)
            {
                Debug.Log("[Config] loaded 0 hierarchy levels");
                return;
            }

            int count = 0;
            foreach (var entry in entries)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.key))
                    continue;

                                var style = new HierarchyStyle(
                    entry.size_cm,
                    entry.show_label,
                    ParseEffectString(entry.sun_effect, entry.accent_effect, entry.pulse),
                    entry.rotate_contour,
                    entry.reveal_delay_s,
                    entry.reveal_duration_s);

                _stylesByKey[entry.key.Trim()] = style;
                count++;
            }

            Debug.Log($"[Config] loaded {count} hierarchy levels");
        }

        public static void ResetToDefaults()
        {
            _stylesByKey = new Dictionary<string, HierarchyStyle>();
        }

        public static bool TryResolveByKey(string key, out HierarchyStyle style)
        {
            style = default;
            if (string.IsNullOrWhiteSpace(key) || _stylesByKey == null)
                return false;

            return _stylesByKey.TryGetValue(key.Trim(), out style);
        }
    }
}
