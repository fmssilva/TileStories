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

        // Marker Label Style (_2.0_Labels_And_Fonts_Design.md section 4). OverridesLabelStyle false
        // = the label follows the wall default and the three values are ignored; MarkerView, not
        // this struct, picks between the two. Values are already clamped by StyleOf.
        public readonly bool OverridesLabelStyle;
        public readonly float LabelGapRatio;
        public readonly float LabelFontSizeRatio;
        public readonly string LabelFontKey;

        public HierarchyStyle(float sizeCm, bool showLabel,
            MarkerEffectFlags effectFlags, bool rotateContour,
            float revealDelaySeconds, float revealDurationSeconds,
            bool overridesLabelStyle = false, float labelGapRatio = 0f,
            float labelFontSizeRatio = 0f, string labelFontKey = "")
        {
            SizeCm = sizeCm;
            ShowLabel = showLabel;
            EffectFlags = effectFlags;
            RotateContour = rotateContour;
            RevealDelaySeconds = revealDelaySeconds;
            RevealDurationSeconds = revealDurationSeconds;
            OverridesLabelStyle = overridesLabelStyle;
            LabelGapRatio = labelGapRatio;
            LabelFontSizeRatio = labelFontSizeRatio;
            LabelFontKey = labelFontKey ?? "";
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
        private static Dictionary<string, string> _facingModeOverrideByKey = new();

        // The effects a level requests, parsed from its ripple_effect / halo_effect strings and
        // pulse flag. The one parser of those strings (the editor reuses it for its usage lines);
        // logWarnings=false keeps repaint-driven callers from spamming the console.
        public static MarkerEffectFlags EffectFlagsOf(HierarchyLevelEntry entry, bool logWarnings = true)
        {
            return entry == null
                ? MarkerEffectFlags.None
                : ParseEffectString(entry.ripple_effect, entry.halo_effect, entry.pulse, logWarnings);
        }

        private static MarkerEffectFlags ParseEffectString(string rippleEffect, string haloEffect, bool pulse, bool logWarnings)
        {
            var flags = MarkerEffectFlags.None;

            switch (rippleEffect)
            {
                case "ripple_rings": flags |= MarkerEffectFlags.RippleRings; break;
                case "ripple_discs": flags |= MarkerEffectFlags.RippleDiscs; break;
                case "none":
                case null:
                case "":
                    break;
                default:
                    if (logWarnings) Debug.LogWarning($"[MarkerHierarchyResolver] Unknown ripple_effect '{rippleEffect}', ignoring.");
                    break;
            }

            switch (haloEffect)
            {
                case "halo_ring": flags |= MarkerEffectFlags.HaloRing; break;
                case "halo_disc": flags |= MarkerEffectFlags.HaloDisc; break;
                case "beacon":    flags |= MarkerEffectFlags.Beacon;   break;
                case "none":
                case null:
                case "":
                    break;
                default:
                    if (logWarnings) Debug.LogWarning($"[MarkerHierarchyResolver] Unknown halo_effect '{haloEffect}', ignoring.");
                    break;
            }

            if (pulse) flags |= MarkerEffectFlags.Pulse;
            return flags;
        }

        // The ONE conversion from a table row to its resolved style. Configure uses it for real
        // markers and the demo grid uses it for its level cells, so a column can never reach one
        // and silently miss the other (the demo grid once dropped every label override this way).
        public static HierarchyStyle StyleOf(HierarchyLevelEntry entry, bool logWarnings = true)
        {
            if (entry == null) return Fallback;
            return new HierarchyStyle(
                entry.size_cm,
                entry.show_label,
                EffectFlagsOf(entry, logWarnings),
                entry.rotate_contour,
                entry.reveal_delay_s,
                entry.reveal_duration_s,
                entry.override_label_style,
                MarkerVisualSettings.ClampLabelGapRatio(entry.label_gap_ratio),
                MarkerVisualSettings.ClampLabelFontSizeRatio(entry.label_font_size_ratio),
                MarkerVisualSettings.ResolveLabelFontKey(entry.label_font_key));
        }

        public static void Configure(IEnumerable<HierarchyLevelEntry> entries)
        {
            _stylesByKey.Clear();
            _levelIndexByKey.Clear();
            _priorityByKey.Clear();
            _facingModeOverrideByKey.Clear();

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

                _stylesByKey[entry.key.Trim()] = StyleOf(entry);
                _levelIndexByKey[entry.key.Trim()] = count;
                _priorityByKey[entry.key.Trim()] = entry.priority;
                _facingModeOverrideByKey[entry.key.Trim()] = entry.facing_mode_override ?? "";
                count++;
            }

            Debug.Log($"[Config] loaded {count} hierarchy levels");
        }

        public static void ResetToDefaults()
        {
            _stylesByKey = new Dictionary<string, HierarchyStyle>();
            _levelIndexByKey = new Dictionary<string, int>();
            _priorityByKey = new Dictionary<string, int>();
            _facingModeOverrideByKey = new Dictionary<string, string>();
        }

        // Resolve a hierarchy level key to its facing_mode_override (spec _2.1
        // section 4.3). Empty/missing key or unknown key both mean "inherit the wall
        // setting" -- returns "" in either case, never throws.
        public static string ResolveFacingModeOverride(string key)
        {
            if (!string.IsNullOrWhiteSpace(key) && _facingModeOverrideByKey != null &&
                _facingModeOverrideByKey.TryGetValue(key.Trim(), out var modeOverride))
            {
                return modeOverride;
            }
            return "";
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