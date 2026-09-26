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
            // - a fix names the levels the way the dropdown lists them, never by key
            string levelNames = string.Join(", ", (_config.hierarchy_levels ?? new List<HierarchyLevelEntry>())
                .Where(e => e != null && !string.IsNullOrEmpty(e.key)).Select(e => "'" + EditorNames.Level(e) + "'"));

            foreach (var poi in _config.pois)
            {
                if (poi == null) continue;

                string key = poi.hierarchy_level_key;
                if (string.IsNullOrEmpty(key))
                {
                    // Spec _2_3 section 11b: an UNSET key silently degrades to MarkerHierarchyResolver.Fallback --
                    // surface it at editor time. Distinguished from the stale-key branch below so "no level
                    // assigned" reads differently from "references a deleted level".
                    issues.Add(new EditorAlertItem(
                        subject: EditorNames.Poi(_config.pois, poi),
                        value: "<empty>",
                        problem: "No Hierarchy Level is assigned. This POI renders at the framework's fallback size, with no label.",
                        fixHint: levelKeys.Count == 0
                            ? "Global Scene > Hierarchy Levels: add at least one row, then assign it to this POI below."
                            : $"Specific Marker tab > this POI > Marker Style > Hierarchy Level dropdown: pick one of {levelNames}."));
                    continue;
                }

                if (!levelKeys.Contains(key))
                {
                    // - the level's row is gone, so it has no name any more: quote exactly what its dropdown shows
                    issues.Add(new EditorAlertItem(
                        subject: EditorNames.Poi(_config.pois, poi),
                        value: MissingAsShown(key),
                        problem: "This POI's Hierarchy Level no longer matches any row in the wall's Hierarchy Levels table (the level was deleted after this POI was set up).",
                        fixHint: levelKeys.Count == 0
                            ? "Global Scene > Hierarchy Levels: add at least one row and pick it in this POI's Hierarchy Level dropdown, or set that dropdown to (none)."
                            : $"Specific Marker tab > this POI > Marker Style > Hierarchy Level dropdown: pick one of {levelNames}, or (none)."));
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
                .Where(e => e != null && !string.IsNullOrEmpty(e.key)).Select(e => e.key));
            var badgeKeys = new HashSet<string>((_config.badge_categories ?? new List<BadgeCategoryEntry>())
                .Where(e => e != null && !string.IsNullOrEmpty(e.key)).Select(e => e.key));
            var statusLevelKeys = new HashSet<string>((_config.outline_levels ?? new List<OutlineLevelEntry>())
                .Where(e => e != null && !string.IsNullOrEmpty(e.key)).Select(e => e.key));

            foreach (var poi in _config.pois)
            {
                if (poi == null) continue;
                string poiName = EditorNames.Poi(_config.pois, poi);

                if (!string.IsNullOrEmpty(poi.category) && categories.Count > 0 && !categories.Contains(poi.category))
                    issues.Add(new EditorAlertItem(poiName, MissingAsShown(poi.category),
                        "Category does not match any row in the Marker > Category Symbols table.",
                        "The marker gets an automatic colour until fixed: Specific Marker > this POI > Marker Style > Category, pick a real category (shown as '(missing)' now)."));

                if (!string.IsNullOrEmpty(poi.badge_category) && badgeKeys.Count > 0 && !badgeKeys.Contains(poi.badge_category))
                    issues.Add(new EditorAlertItem(poiName, MissingAsShown(poi.badge_category),
                        "Badge category does not match any row in the Badge table.",
                        "Specific Marker > this POI > Badge Style > Badge category: pick a real badge (shown as '(missing)' now)."));

                if (poi.has_status && !poi.status_unknown && !string.IsNullOrEmpty(poi.status_level_key) &&
                    statusLevelKeys.Count > 0 && !statusLevelKeys.Contains(poi.status_level_key))
                    issues.Add(new EditorAlertItem(poiName, MissingAsShown(poi.status_level_key),
                        "Status level does not match any row in the Outline Types table.",
                        "Specific Marker > this POI > Outline > Status level: pick a real outline type (shown as '(missing)' now)."));

                if (poi.has_custom_symbol && string.IsNullOrWhiteSpace(poi.custom_symbol_key))
                    issues.Add(new EditorAlertItem(poiName, "<empty>",
                        "Use Custom Symbol is ticked but no symbol is assigned.",
                        "Specific Marker > this POI > Marker Style: assign a symbol under Custom symbol, or untick Use Custom Symbol."));
            }

            return issues;
        }

        // Every taxonomy row needs a name of its own: a visitor reads it (filter chip, card, results) and the
        // developer picks it in a POI's dropdown. A blank name makes both show the hidden generated key instead;
        // two rows with one name make two chips / dropdown entries nobody can tell apart. Rows are named by
        // their table and position ("row 2"), never by their key.
        private List<EditorAlertItem> ValidateTaxonomyRows()
        {
            var issues = new List<EditorAlertItem>();
            if (_config == null)
                return issues;

            void Check<T>(string table, string nameColumn, List<T> rows, Func<T, string> nameOf) where T : class
            {
                if (rows == null) return;
                var firstRowOfName = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < rows.Count; i++)
                {
                    if (rows[i] == null) continue;
                    string name = nameOf(rows[i])?.Trim();
                    if (string.IsNullOrEmpty(name))
                    {
                        issues.Add(new EditorAlertItem(table + ", row " + (i + 1), "<empty>",
                            $"This row has no {nameColumn}, so visitors and the POI dropdowns see an internal id instead of a name.",
                            $"Global Scene > {table}: type a {nameColumn} in row {i + 1}."));
                        continue;
                    }
                    if (firstRowOfName.TryGetValue(name, out int first))
                        issues.Add(new EditorAlertItem(table + ", rows " + (first + 1) + " and " + (i + 1), "'" + name + "'",
                            $"Two rows share one {nameColumn}, so visitors see two identical filter choices and the POI dropdowns list it twice.",
                            $"Global Scene > {table}: give row {i + 1} a different {nameColumn}."));
                    else
                        firstRowOfName[name] = i;
                }
            }

            Check("Marker > Category Symbols", "Category label", _config.category_styles, e => e.label);
            Check("Badge > Badge Categories", "Badge label", _config.badge_categories, e => e.label);
            Check("Outline > Outline Types", "Outline label", _config.outline_levels, e => e.label);
            Check("Hierarchy Levels", "Hierarchy Level Name", _config.hierarchy_levels, e => e.level_name);
            Check("Select, Filter & Search > Keywords & Synonyms > Keyword Fields", "Label", _config.search_fields, e => e.label);
            return issues;
        }

        // A reference whose row was deleted has no name left: say exactly what its dropdown shows for it
        private static string MissingAsShown(string key) => "shown as '" + key + ReferencePopupOptions.MissingSuffix + "'";

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
                        subject: "Hierarchy level '" + EditorNames.Level(entry) + "'",
                        value: $"{s:0.##} cm",
                        problem: "Marker Size (cm) is outside the plausible marker symbol range.",
                        fixHint: "Global Scene > Hierarchy Levels > Marker Size (cm): real marker symbols are ~0.5 to 100 cm. Check units (cm vs m)."));
                }
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
                    subject: "LOD > Crowding",
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
                    subject: "LOD > Distance Bands",
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
            issues.AddRange(ValidateTaxonomyRows());
            issues.AddRange(ValidateHierarchyLevelSizeRange(_config?.hierarchy_levels));
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

            void Check(string field, string value, string[] known)
            {
                if (!string.IsNullOrEmpty(value) && System.Array.IndexOf(known, value) >= 0) return;
                issues.Add(new EditorAlertItem(
                    subject: "Select, Filter & Search",
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
                        string displayLabel = TaxonomyNames.NameOr(field.label, field.key);
                        issues.Add(new EditorAlertItem(
                            subject: EditorNames.Poi(_config.pois, poi),
                            value: null,
                            problem: $"POI is missing keywords for the required keyword field '{displayLabel}'.",
                            fixHint: $"Open Specific Marker > this POI > Summary & Keywords and fill in '{displayLabel}'."));
                    }
                }
            }

            return issues;
        }

    }
}
