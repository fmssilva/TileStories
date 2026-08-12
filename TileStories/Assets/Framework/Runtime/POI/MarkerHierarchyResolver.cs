﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TileStories
{
    // Resolved visual style for a single hierarchy level. Readonly struct, like
    // StatusLevel -- value semantics so callers cannot mutate a shared instance.
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
    // class with Configure / ResetToDefaults / TryResolveByKey / TryResolvePriority (priority-ordered).
    // Empty or missing config means TryResolveByKey returns false and callers fall back to Fallback.
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
        private static Dictionary<string, int> _levelIndexByKey = new();
        private static Dictionary<string, int> _priorityByKey = new();

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
            _levelIndexByKey.Clear();
            _priorityByKey.Clear();

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
                _levelIndexByKey[entry.key.Trim()] = count;
                _priorityByKey[entry.key.Trim()] = entry.priority;
                count++;
            }

            Debug.Log($"[Config] loaded {count} hierarchy levels");
        }

        public static void ResetToDefaults()
        {
            _stylesByKey = new Dictionary<string, HierarchyStyle>();
            _levelIndexByKey = new Dictionary<string, int>();
            _priorityByKey = new Dictionary<string, int>();
        }

        // Resolve a hierarchy level key to its HierarchyStyle. On any failure path
        // (blank/null key, unconfigured resolver, or unknown key) the out value is
        // the framework Fallback (12cm, no label/effects) -- never a 0cm/invisible
        // default, which is exactly the footgun Fallback exists to prevent. MarkerView
        // reassigns Fallback defensively, so this is idempotent for it; LODController
        // does not call this path. Callers that need ordering use GetLevelPriority.
        public static bool TryResolveByKey(string key, out HierarchyStyle style)
        {
            if (!string.IsNullOrWhiteSpace(key) && _stylesByKey != null &&
                _stylesByKey.TryGetValue(key.Trim(), out var resolved))
            {
                style = resolved;
                return true;
            }

            style = Fallback;
            return false;
        }

        // Resolve a hierarchy level key to its priority (lower = drawn on top /
        // survives count-cap truncation first). Semantics:
        //   unknown / blank / unconfigured -> int.MaxValue + false  (lowest priority)
        //   authored priority >= 1         -> returned as-is        (explicit author order)
        //   authored priority <= 0 (unset)   -> positional index (1-based row fallback)
        //   duplicate priorities           -> both keys resolve to the same value
        // Unknown keys resolve to int.MaxValue so they lose every tie -- this is what
        // LODController's count-cap truncation relies on (lowest priority hidden first).
        // Kept distinct from the positional _levelIndexByKey so a wall can reorder
        // priority without reshuffling table rows.
        public static bool TryResolvePriority(string key, out int priority)
        {
            priority = int.MaxValue;
            if (string.IsNullOrWhiteSpace(key) || _priorityByKey == null)
                return false;

            if (!_priorityByKey.TryGetValue(key.Trim(), out var authored))
                return false;

            // Explicit author value wins (>= 1). A non-positive value is "unset" and
            // falls back to the level's 1-based table position (legacy positional order,
            // preserved unchanged for backward compatibility); if the level is not
            // in the table, treat as lowest priority rather than an arbitrary value.
            priority = authored >= 1
                ? authored
                : (_levelIndexByKey.TryGetValue(key.Trim(), out var idx) ? idx + 1 : int.MaxValue);
            return true;
        }

        // Convenience wrapper: ordering priority for a key, unknown keys ranking last.
        public static int GetLevelPriority(string key)
        {
            return TryResolvePriority(key, out var p) ? p : int.MaxValue;
        }
    }
}