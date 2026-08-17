using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    public partial class POIAuthoringToolWindow
    {
        // Validates that every POI's hierarchy_level_key resolves to an entry
        // in _config.hierarchy_levels. Returns a list of issues (empty if clean).
        // Also detects the edge case where hierarchy_levels is empty/null but
        // POIs have non-empty hierarchy_level_key values.
        private List<EditorAlertItem> ValidateHierarchyLevelKeys()
        {
            var issues = new List<EditorAlertItem>();

            if (_config == null || _config.pois == null)
                return issues;

            var levelKeys = _config.hierarchy_levels != null
                ? new HashSet<string>(_config.hierarchy_levels
                    .Where(e => e != null && !string.IsNullOrEmpty(e.key))
                    .Select(e => e.key))
                : new HashSet<string>();

            foreach (var poi in _config.pois)
            {
                if (poi == null) continue;

                string key = poi.hierarchy_level_key;
                if (string.IsNullOrEmpty(key))
                    continue; // Empty key is allowed -- falls back to framework default

                if (!levelKeys.Contains(key))
                {
                    issues.Add(new EditorAlertItem(
                        poiId: poi.id ?? "<unnamed>",
                        value: key,
                        problem: $"Hierarchy level key does not match any entry in the hierarchy_levels table.",
                        fixHint: levelKeys.Count == 0
                            ? "Add at least one hierarchy level row, or clear this POI's hierarchy_level_key."
                            : $"Add a row with key '{key}' or change this POI's key to one of: {string.Join(", ", levelKeys)}."));
                }
            }

            return issues;
        }

        // Soft sanity check on marker-symbol diameters: flags sizes outside the
        // plausible range that usually indicate a unit typo (m vs cm). Warning
        // only -- never blocks authoring, never auto-fixes.
        internal static List<EditorAlertItem> ValidateHierarchyLevelSizeRange(
            IEnumerable<global::TileStories.HierarchyLevelEntry> levels)
        {
            var issues = new List<EditorAlertItem>();
            if (levels == null)
                return issues;
            foreach (var entry in levels)
            {
                if (entry == null)
                    continue;
                float s = entry.size_cm;
                if (s < 0.5f || s > 100f)
                {
                    issues.Add(new EditorAlertItem(
                        poiId: entry.key ?? "<unnamed>",
                        value: $"{s:0.##} cm",
                        problem: "size_cm is outside the plausible marker symbol range.",
                        fixHint: "Real marker symbols are ~0.5cm..100cm. Check units (cm vs m)."));
                }
            }
            return issues;
        }

        // Runs validation after config load and shows a non-blocking alert if
        // any hierarchy keys are unresolvable or level sizes look out of range
        // or any search-mode enum-string fields are unknown/inert.
        private void ValidateAndAlert(string context)
        {
            var issues = new List<EditorAlertItem>();
            issues.AddRange(ValidateHierarchyLevelKeys());
            issues.AddRange(ValidateHierarchyLevelSizeRange(_config?.hierarchy_levels));
            issues.AddRange(ValidateSearchEnumFields());
            if (issues.Count == 0)
                return;

            string title = $"Config validation issues ({context})";
            string guidance = "Some configuration needs attention. Fix or add the missing entries noted below; affected items fall back to framework defaults at runtime until fixed.";
            PopupWindow.Show(new Rect(100, 100, 100, 100), new EditorAlertPopup(title, issues, guidance));
        }

        // (D4a) Validates that search-related string fields that map to enums
        // have known values. Non-blocking warnings -- surfaces inert strategy
        // values (scoped/faceted/auto_complete) and unknown values for
        // search_mode, voice_search_match_mode, voice_activity_indicator_style,
        // suggested_source, and search_index_strategy.
        private List<EditorAlertItem> ValidateSearchEnumFields()
        {
            var issues = new List<EditorAlertItem>();
            if (_config == null)
                return issues;

            string wallId = _config.wall_id ?? "<unnamed>";

            // search_mode: dynamic and explicit are operative; scoped/faceted/auto_complete
            // are recognized but inert (fall back to dynamic).
            string[] searchModeActive = { "dynamic", "explicit" };
            if (!string.IsNullOrEmpty(_config.search_mode) &&
                !System.Array.Exists(searchModeActive, m => m.Equals(_config.search_mode, StringComparison.OrdinalIgnoreCase)))
            {
                string[] inertModes = { "scoped", "faceted", "auto_complete" };
                string detail = System.Array.Exists(inertModes, m => m.Equals(_config.search_mode, StringComparison.OrdinalIgnoreCase))
                    ? "inert -- falls back to dynamic at runtime"
                    : "unrecognized value";
                issues.Add(new EditorAlertItem(
                    poiId: wallId,
                    value: _config.search_mode,
                    problem: $"search_mode is {detail}.",
                    fixHint: $"Use 'dynamic' or 'explicit'. 'scoped', 'faceted', 'auto_complete' are recognized but not yet implemented."));
            }

            // search_index_strategy
            string[] validStrategies = { "keyword_ranked", "weighted_fields" };
            if (!string.IsNullOrEmpty(_config.search_index_strategy) &&
                !System.Array.Exists(validStrategies, s => s.Equals(_config.search_index_strategy, StringComparison.OrdinalIgnoreCase)))
            {
                issues.Add(new EditorAlertItem(
                    poiId: wallId,
                    value: _config.search_index_strategy,
                    problem: "search_index_strategy has an unrecognized value.",
                    fixHint: "Use 'keyword_ranked' or 'weighted_fields'."));
            }

            // voice_search_match_mode
            string[] validMatchModes = { "all", "any" };
            if (!string.IsNullOrEmpty(_config.voice_search_match_mode) &&
                !System.Array.Exists(validMatchModes, m => m.Equals(_config.voice_search_match_mode, StringComparison.OrdinalIgnoreCase)))
            {
                issues.Add(new EditorAlertItem(
                    poiId: wallId,
                    value: _config.voice_search_match_mode,
                    problem: "voice_search_match_mode has an unrecognized value.",
                    fixHint: "Use 'all' (conjunction) or 'any' (disjunction)."));
            }

            // voice_activity_indicator_style
            string[] validIndicatorStyles = { "mic_text", "listen_bar" };
            if (!string.IsNullOrEmpty(_config.voice_activity_indicator_style) &&
                !System.Array.Exists(validIndicatorStyles, s => s.Equals(_config.voice_activity_indicator_style, StringComparison.OrdinalIgnoreCase)))
            {
                issues.Add(new EditorAlertItem(
                    poiId: wallId,
                    value: _config.voice_activity_indicator_style,
                    problem: "voice_activity_indicator_style has an unrecognized value.",
                    fixHint: "Use 'mic_text' or 'listen_bar'."));
            }

            // suggested_source
            string[] validSources = { "category_distribution", "recent_first" };
            if (!string.IsNullOrEmpty(_config.suggested_source) &&
                !System.Array.Exists(validSources, s => s.Equals(_config.suggested_source, StringComparison.OrdinalIgnoreCase)))
            {
                issues.Add(new EditorAlertItem(
                    poiId: wallId,
                    value: _config.suggested_source,
                    problem: "suggested_source has an unrecognized value.",
                    fixHint: "Use 'category_distribution' or 'recent_first'."));
            }

            // weight_* fields (only relevant when using the weighted_fields strategy;
            // keyword_ranked does not read them at runtime, so they are inert and need
            // no validation when that strategy is active)
            if (string.Equals(_config.search_index_strategy, "weighted_fields", StringComparison.OrdinalIgnoreCase))
            {
                if (_config.weight_name <= 0)
                {
                    issues.Add(new EditorAlertItem(
                        poiId: wallId,
                        value: _config.weight_name.ToString(),
                        problem: "weight_name must be positive when using the 'weighted_fields' strategy.",
                        fixHint: "Set to a positive number (default 3)."));
                }

                if (_config.weight_custom_field <= 0)
                {
                    issues.Add(new EditorAlertItem(
                        poiId: wallId,
                        value: _config.weight_custom_field.ToString(),
                        problem: "weight_custom_field must be positive when using the 'weighted_fields' strategy.",
                        fixHint: "Set to a positive number (default 2)."));
                }

                if (_config.weight_derived_label <= 0)
                {
                    issues.Add(new EditorAlertItem(
                        poiId: wallId,
                        value: _config.weight_derived_label.ToString(),
                        problem: "weight_derived_label must be positive when using the 'weighted_fields' strategy.",
                        fixHint: "Set to a positive number (default 2)."));
                }

                if (_config.weight_others <= 0)
                {
                    issues.Add(new EditorAlertItem(
                        poiId: wallId,
                        value: _config.weight_others.ToString(),
                        problem: "weight_others must be positive when using the 'weighted_fields' strategy.",
                        fixHint: "Set to a positive number (default 1)."));
                }
            }

            return issues;
        }
    }
}
