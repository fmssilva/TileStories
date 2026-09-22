using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    public partial class POIEditorToolWindow
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
                {
                    // Spec _2_3 section 11b: an UNSET key silently degrades to
                    // MarkerHierarchyResolver.Fallback -- surface it at editor time
                    // instead of letting the developer discover a generic-looking
                    // marker at runtime. Distinguished from the stale-key branch
                    // below so "no level assigned" reads differently from
                    // "references a deleted level" (2026-09-22: wording leads with
                    // the UI-facing name now, not the raw hierarchy_level_key field,
                    // after a real dialog read as too code-flavoured -- see
                    // HierarchyLevelKeyValidationTests).
                    issues.Add(new EditorAlertItem(
                        poiId: poi.id ?? "<unnamed>",
                        value: "<empty>",
                        problem: "No Hierarchy Level is assigned (config field: hierarchy_level_key). This POI renders at the framework's fallback size, with no label.",
                        fixHint: levelKeys.Count == 0
                            ? "Global Scene > Hierarchy Levels: add at least one row, then assign it to this POI below."
                            : $"Specific Marker tab > this POI > Hierarchy Level dropdown: pick one of {string.Join(", ", levelKeys)}."));
                    continue;
                }

                if (!levelKeys.Contains(key))
                {
                    issues.Add(new EditorAlertItem(
                        poiId: poi.id ?? "<unnamed>",
                        value: key,
                        problem: "This POI's Hierarchy Level no longer matches any row in the wall's Hierarchy Levels table (it was likely renamed or deleted after this POI was set up).",
                        fixHint: levelKeys.Count == 0
                            ? "Global Scene > Hierarchy Levels: add at least one row, or clear this POI's Hierarchy Level dropdown."
                            : $"Global Scene > Hierarchy Levels: add a row keyed '{key}' back, or Specific Marker tab > this POI > Hierarchy Level dropdown: pick one of {string.Join(", ", levelKeys)}."));
                }
            }

            return issues;
        }

        // Validates that every POI's category, badge_category, status_level_key and
        // custom_symbol_key actually resolve to a taxonomy row (_2.2.1/2/3). A stale reference
        // (the row was renamed or deleted after this POI was authored) degrades silently at
        // runtime -- CategoryPalette/BadgeCategoryPalette/StatusRamp all fall back quietly -- so
        // this surfaces it at editor time instead. Non-blocking: never auto-fixes.
        private List<EditorAlertItem> ValidateMarkerTaxonomyReferences()
        {
            var issues = new List<EditorAlertItem>();
            if (_config == null || _config.pois == null)
                return issues;

            var categories = new HashSet<string>((_config.category_styles ?? new List<CategoryStyleEntry>())
                .Where(e => e != null && !string.IsNullOrEmpty(e.category)).Select(e => e.category));
            var badgeKeys = new HashSet<string>((_config.badge_categories ?? new List<BadgeCategoryEntry>())
                .Where(e => e != null && !string.IsNullOrEmpty(e.key)).Select(e => e.key));
            var statusLevelKeys = new HashSet<string>((_config.outline_levels ?? new List<OutlineLevelEntry>())
                .Where(e => e != null && !string.IsNullOrEmpty(e.key)).Select(e => e.key));

            foreach (var poi in _config.pois)
            {
                if (poi == null) continue;
                string poiId = poi.id ?? "<unnamed>";

                if (!string.IsNullOrEmpty(poi.category) && categories.Count > 0 && !categories.Contains(poi.category))
                    issues.Add(new EditorAlertItem(poiId, poi.category,
                        "Category does not match any row in the Marker > Category Symbols table.",
                        "Falls through to CategoryPalette's hash-based colour -- pick a real category or add this one to the table."));

                if (!string.IsNullOrEmpty(poi.badge_category) && badgeKeys.Count > 0 && !badgeKeys.Contains(poi.badge_category))
                    issues.Add(new EditorAlertItem(poiId, poi.badge_category,
                        "Badge category does not match any row in the Badge table.",
                        "Pick a real badge key or add this one to the table."));

                if (poi.has_status && !poi.status_unknown && !string.IsNullOrEmpty(poi.status_level_key) &&
                    statusLevelKeys.Count > 0 && !statusLevelKeys.Contains(poi.status_level_key))
                    issues.Add(new EditorAlertItem(poiId, poi.status_level_key,
                        "Status level key does not match any row in the Outline table.",
                        "Pick this POI's status level again from the Status level dropdown."));

                if (poi.has_custom_symbol && string.IsNullOrWhiteSpace(poi.custom_symbol_key))
                    issues.Add(new EditorAlertItem(poiId, "<empty>",
                        "Use Custom Symbol is ticked but no symbol is assigned.",
                        "Assign a sprite in Marker Style > Custom symbol, or untick Use Custom Symbol."));
            }

            return issues;
        }

        // Soft sanity check on marker-symbol diameters: flags sizes outside the
        // plausible range that usually indicate a unit typo (m vs cm). Warning
        // only -- never blocks editing, never auto-fixes.
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

        // Spec _2_4 section 6: shrink_start_neighbor_count must be strictly less
        // than cluster_min_count. LODController.IsDensityConfigValid already
        // detects violations (and DensityFactor already no-ops safely) -- this
        // just surfaces the detector's answer at editor time so a backwards
        // pair cannot be saved silently.
        private List<EditorAlertItem> ValidateDensityThresholds()
        {
            var issues = new List<EditorAlertItem>();
            var lod = _config?.lod_settings;
            if (lod == null) return issues;
            if (!LODController.IsDensityConfigValid(lod))
            {
                issues.Add(new EditorAlertItem(
                    poiId: "<LOD settings>",
                    value: $"shrink_start_neighbor_count={lod.shrink_start_neighbor_count}, cluster_min_count={lod.cluster_min_count}",
                    problem: "Shrink Start must be strictly less than Cluster Min, or the shrink/fade ramp never activates (DensityFactor always returns 1.0 -- a silent no-op, not a runtime error).",
                    fixHint: "Lower Shrink Start below Cluster Min, or raise Cluster Min above Shrink Start, or use \"Suggest Values\" to regenerate both together."));
            }
            return issues;
        }

        // Runs validation after config load and queues a notice dialog if
        // any hierarchy keys are unresolvable/unset or level sizes look out of range
        // or any search-mode string fields are unknown/inert
        // or any POI is missing keywords for a forced search field
        // or the LOD density thresholds are configured backwards.
        private void ValidateAndAlert(string context)
        {
            var issues = new List<EditorAlertItem>();
            issues.AddRange(ValidateHierarchyLevelKeys());
            issues.AddRange(ValidateMarkerTaxonomyReferences());
            issues.AddRange(ValidateHierarchyLevelSizeRange(_config?.hierarchy_levels));
            issues.AddRange(ValidateSearchEnumFields());
            issues.AddRange(ValidateForcedSearchFields());
            issues.AddRange(ValidateDensityThresholds());
            if (issues.Count == 0)
                return;

            string guidance = "Some configuration needs attention. Fix or add the missing entries noted above; affected items fall back to framework defaults at runtime until fixed.";
            EditorNotice.Queue($"Config validation issues ({context})", EditorAlertItem.FormatList(issues, guidance), 0f, NoticeKeys.ConfigValidation);
        }

        // (D4a) Validates that search-related string fields that map to enums
        // have known values. Non-blocking warnings -- surfaces inert strategy
        // values (scoped/faceted/auto_complete) and unknown values for
        // search_mode, voice_search_match_mode, voice_activity_indicator_style,
        // and suggested_source.
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

            return issues;
        }

        // Validates that every POI has keywords for any search field marked as forced.
        // Non-blocking -- shows a warning icon in the SpecificMarker editor as well,
        // but also surfaces here so the developer sees it on load/save.
        private List<EditorAlertItem> ValidateForcedSearchFields()
        {
            var issues = new List<EditorAlertItem>();
            if (_config == null || _config.pois == null || _config.search_fields == null)
                return issues;

            var forcedFields = _config.search_fields.FindAll(f => f != null && f.forced && !string.IsNullOrWhiteSpace(f.key));
            if (forcedFields.Count == 0)
                return issues;

            foreach (var poi in _config.pois)
            {
                if (poi == null)
                    continue;

                foreach (var field in forcedFields)
                {
                    var entry = poi.search_keyword_fields?.Find(e => e?.field_key == field.key);
                    bool isEmpty = entry == null || entry.keywords == null || entry.keywords.Count == 0;
                    if (isEmpty)
                    {
                        string displayLabel = string.IsNullOrWhiteSpace(field.label) ? field.key : field.label;
                        issues.Add(new EditorAlertItem(
                            poiId: poi.id ?? "<unnamed>",
                            value: field.key,
                            problem: $"POI is missing keywords for the required search field '{displayLabel}'.",
                            fixHint: $"Open the Specific Marker tab, expand this POI, and fill in the '{displayLabel}' keyword field."));
                    }
                }
            }

            return issues;
        }

    }
}
