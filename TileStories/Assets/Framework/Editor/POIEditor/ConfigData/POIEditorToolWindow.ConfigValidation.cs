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
                        "The marker gets an automatic colour until fixed: Specific Marker > this POI > Marker Style > Category, pick a real category (shown as '(missing)' now), or add this one to Global Scene > Marker > Category Symbols."));

                if (!string.IsNullOrEmpty(poi.badge_category) && badgeKeys.Count > 0 && !badgeKeys.Contains(poi.badge_category))
                    issues.Add(new EditorAlertItem(poiId, poi.badge_category,
                        "Badge category does not match any row in the Badge table.",
                        "Specific Marker > this POI > Badge Style > Badge category: pick a real badge key, or add this one to Global Scene > Badge."));

                if (poi.has_status && !poi.status_unknown && !string.IsNullOrEmpty(poi.status_level_key) &&
                    statusLevelKeys.Count > 0 && !statusLevelKeys.Contains(poi.status_level_key))
                    issues.Add(new EditorAlertItem(poiId, poi.status_level_key,
                        "Status level does not match any row in the Outline Types table.",
                        "Specific Marker > this POI > Outline > Status level: pick a real outline type (shown as '(missing)' now)."));

                if (poi.has_custom_symbol && string.IsNullOrWhiteSpace(poi.custom_symbol_key))
                    issues.Add(new EditorAlertItem(poiId, "<empty>",
                        "Use Custom Symbol is ticked but no symbol is assigned.",
                        "Specific Marker > this POI > Marker Style: assign a symbol under Custom symbol, or untick Use Custom Symbol."));
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

        // Priority is a whole number >= 1 (the Priority field enforces it while editing); a value below 1
        // can only come from a hand-edited file, where it silently falls back to the row position.
        internal static List<EditorAlertItem> ValidateHierarchyLevelPriorities(
            IEnumerable<global::TileStories.HierarchyLevelEntry> levels)
        {
            var issues = new List<EditorAlertItem>();
            if (levels == null)
                return issues;
            foreach (var entry in levels)
            {
                if (entry == null || entry.priority >= 1)
                    continue;
                issues.Add(new EditorAlertItem(
                    poiId: entry.key ?? "<unnamed>",
                    value: entry.priority.ToString(),
                    problem: "Priority is below 1, so the level falls back to its row position.",
                    fixHint: "Global Scene > Hierarchy Levels > Priority: enter a whole number of 1 or more (lower = higher priority)."));
            }
            return issues;
        }

        // Spec _2_4 section 6: in a mode that shrinks, Shrink Starts At must be smaller than Crowded At,
        // or markers never shrink (LODController.IsDensityConfigValid is the runtime's own detector).
        private List<EditorAlertItem> ValidateDensityThresholds()
        {
            var issues = new List<EditorAlertItem>();
            var lod = _config?.lod_settings;
            if (lod == null || !LodEditorRules.UsesShrink(lod.density_response_mode)) return issues;
            if (!LODController.IsDensityConfigValid(lod))
            {
                issues.Add(new EditorAlertItem(
                    poiId: "LOD > Crowding",
                    value: $"Shrink Starts At = {lod.shrink_start_neighbor_count}, Crowded At = {lod.cluster_min_count}",
                    problem: "Shrink Starts At must be strictly less than Crowded At, or crowded markers never shrink or fade.",
                    fixHint: "Lower Shrink Starts At or raise Crowded At, or click Suggest Values under Distance Bands to set both."));
            }
            return issues;
        }

        // Spec _2_4 section 3: the Distance Bands table must go from near to far with sensible values
        private List<EditorAlertItem> ValidateLodBands()
        {
            var issues = new List<EditorAlertItem>();
            var lod = _config?.lod_settings;
            if (lod == null || !lod.enabled) return issues;
            foreach (string problem in LodEditorRules.BandProblems(lod.bands))
                issues.Add(new EditorAlertItem(
                    poiId: "LOD > Distance Bands",
                    value: (lod.bands?.Count ?? 0) + " band(s)",
                    problem: problem,
                    fixHint: "Edit the Distance Bands table, or click Suggest Values."));
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
            issues.AddRange(ValidateHierarchyLevelPriorities(_config?.hierarchy_levels));
            issues.AddRange(ValidateSearchEnumFields());
            issues.AddRange(ValidateForcedSearchFields());
            issues.AddRange(ValidateDensityThresholds());
            issues.AddRange(ValidateLodBands());
            if (issues.Count == 0)
                return;

            string guidance = "Some configuration needs attention. Fix or add the missing entries noted above; affected items fall back to framework defaults at runtime until fixed.";
            EditorNotice.Queue($"Config validation issues ({context})", EditorAlertItem.FormatList(issues, guidance), 0f, NoticeKeys.ConfigValidation);
        }

        // Every Select, Filter & Search option string must be one the runtime understands (the value
        // arrays of SelectFilterSearchOptions, which the Editor dropdowns offer). A hand-edited or old
        // config with anything else is reported; the runtime would fall back to a default.
        private List<EditorAlertItem> ValidateSearchEnumFields()
        {
            var issues = new List<EditorAlertItem>();
            var s = _config?.select_filter_search;
            if (s == null)
                return issues;

            string wallId = _config.wall_id ?? "<unnamed>";
            void Check(string field, string value, string[] known)
            {
                if (!string.IsNullOrEmpty(value) && System.Array.IndexOf(known, value) >= 0) return;
                issues.Add(new EditorAlertItem(
                    poiId: wallId,
                    value: value ?? "(empty)",
                    problem: $"Select, Filter & Search > {field} has a value the app does not know.",
                    fixHint: $"Pick one of the options in Global Scene > Select, Filter & Search > {field}."));
            }

            Check("Zoom Trigger", s.selection?.zoom?.trigger, SelectFilterSearchOptions.Triggers);
            Check("Search Mode", s.search?.mode, SelectFilterSearchOptions.Modes);
            Check("Match Words", s.search?.match_mode, SelectFilterSearchOptions.MatchModes);
            Check("Partial Words", s.search?.prefix_matching, SelectFilterSearchOptions.PrefixModes);
            Check("Filtered-Out Markers", s.filter?.mismatch, SelectFilterSearchOptions.Mismatches);
            Check("Default View", s.results?.default_view, SelectFilterSearchOptions.Views);
            Check("Suggestion Source", s.results?.suggestion_source, SelectFilterSearchOptions.SuggestionSources);
            Check("Visibility", s.minimap?.visibility, SelectFilterSearchOptions.Visibilities);
            Check("Dot Style", s.minimap?.icon_style, SelectFilterSearchOptions.IconStyles);
            Check("Projection", s.minimap?.projection, SelectFilterSearchOptions.Projections);
            Check("Bounds", s.minimap?.bounds_mode, SelectFilterSearchOptions.BoundsModes);
            Check("Voice Indicator", s.voice?.indicator_style, SelectFilterSearchOptions.IndicatorStyles);
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
                            problem: $"POI is missing keywords for the required keyword field '{displayLabel}'.",
                            fixHint: $"Open Specific Marker > this POI > Summary & Keywords and fill in '{displayLabel}'."));
                    }
                }
            }

            return issues;
        }

    }
}
