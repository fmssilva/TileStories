// POIAuthoringToolWindow.SearchFilter.cs
//
// Partial: the authoring-tool Global Scene -> Search & Filter foldout
// (spec _2.6 sections 5/15; split out of GlobalScene.cs per _2.7 #5.1-b
// to restore the one-domain-per-partial-file convention). Editor-only --
// none of this ships. Shares the partial class with POIAuthoringToolWindow.cs
// and GlobalScene.cs; the foldout call itself is wired in GlobalScene.
// Option arrays and help strings stay in Constants.cs, mirroring how the
// LOD/Zoom/Displacement options live there while their draw code is here.

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    public partial class POIAuthoringToolWindow
    {
        // ---- Search & Filter foldout (Block 5, Phase 5.1) ----
        // Exposes ONLY config fields that are read AND consumed at runtime (D1).
        // Organized into labelled subgroups: Search / Voice / Recent & Suggested /
        // Results & Navigation / Selection & Zoom / Ranking & Weights (conditional).
        private void DrawGlobalSearchFilterSection()
        {
            if (_config == null)
                return;

            // --- Master enable/disable (2.6-d): one switch for the whole domain ---
            EditorGUILayout.HelpBox(MasterToggleHelp, MessageType.None);
            _config.search_filter_select_enabled = DrawToggleField(
                "Enable Search / Filter / Select", _config.search_filter_select_enabled, MasterToggleHelp);
            if (!_config.search_filter_select_enabled)
            {
                EditorGUILayout.HelpBox(
                    "Search, filter, minimap, results list and marker-selection responders are fully disabled for this wall.",
                    MessageType.Warning);
                return;
            }

            // --- Keyword Fields table ---
            EditorGUILayout.Space(2f);
            EditorGUILayout.HelpBox(SearchFieldsTableHelp, MessageType.None);
            EditorGUILayout.Space(4f);
            DrawSearchFieldsTable();

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Search", EditorStyles.boldLabel);
            _config.search_mode = DrawPopupField("Search mode", _config.search_mode,
                SearchModeOptions, SearchModeLabels, SearchModeHelp);
            _config.filter_mismatch_behaviour = DrawPopupField("Filtered-out markers", _config.filter_mismatch_behaviour,
                FilterMismatchOptions, FilterMismatchLabels, FilterMismatchHelp);

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Results & Navigation", EditorStyles.boldLabel);
            _config.no_results_message = EditorGUILayout.TextField("No-results message", _config.no_results_message);
            _config.default_result_view = DrawPopupField("Default result view", _config.default_result_view,
                ResultViewOptions, ResultViewLabels, ResultViewHelp);

            EditorGUILayout.Space(3f);
            EditorGUILayout.LabelField("Minimap", EditorStyles.miniLabel);
            _config.minimap_enabled = DrawToggleField("Enable minimap", _config.minimap_enabled, MinimapHelp);
            if (_config.minimap_enabled)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    _config.minimap_visibility = DrawPopupField("Visibility", _config.minimap_visibility,
                        MinimapVisibilityOptions, MinimapVisibilityLabels, MinimapVisibilityHelp);
                    _config.minimap_icon_style = DrawPopupField("Icon style", _config.minimap_icon_style,
                        MinimapIconOptions, MinimapIconLabels, MinimapIconHelp);
                    _config.minimap_dot_size_px = DrawScalarField("Dot size (px)", _config.minimap_dot_size_px, MinimapDotSizeHelp);
                    _config.minimap_dot_tap_target_px = DrawScalarField("Dot tap target (px)", _config.minimap_dot_tap_target_px, MinimapTapTargetHelp);
                }
            }

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Recent & Suggested", EditorStyles.boldLabel);
            _config.recent_search_count = DrawIntField("Recent search count", _config.recent_search_count, RecentCountHelp);
            _config.show_suggested_categories = DrawToggleField("Show suggested categories", _config.show_suggested_categories, SuggestedHelp);
            _config.suggested_source = DrawPopupField("Suggested source", _config.suggested_source,
                SuggestedSourceOptions, SuggestedSourceLabels, SuggestedSourceHelp);

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Voice", EditorStyles.boldLabel);
            _config.voice_search_enabled = DrawToggleField("Enable voice search", _config.voice_search_enabled, VoiceEnabledHelp);
            if (_config.voice_search_enabled)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    _config.voice_search_match_mode = DrawPopupField("Match mode", _config.voice_search_match_mode,
                        VoiceMatchModeOptions, VoiceMatchModeLabels, VoiceMatchModeHelp);
                    _config.voice_activity_indicator_style = DrawPopupField("Indicator style", _config.voice_activity_indicator_style,
                        VoiceIndicatorOptions, VoiceIndicatorLabels, VoiceIndicatorHelp);
                }
            }

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Selection & Zoom", EditorStyles.boldLabel);
            _config.selection_highlight_enabled = DrawToggleField("Selection highlight", _config.selection_highlight_enabled, SelectionHighlightHelp);
            _config.zoom_on_select_enabled = DrawToggleField("Zoom on select", _config.zoom_on_select_enabled, ZoomOnSelectHelp);
            if (_config.zoom_on_select_enabled)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    _config.zoom_on_select_trigger = (WallConfigData.ZoomOnSelectTrigger)EditorGUILayout.EnumPopup("Trigger target", _config.zoom_on_select_trigger);
                    _config.zoom_on_select_density_threshold = DrawIntField("Density threshold", _config.zoom_on_select_density_threshold, ZoomOnSelectDensityHelp);
                    _config.zoom_on_select_factor = DrawScalarField("Zoom factor", _config.zoom_on_select_factor, ZoomOnSelectFactorHelp);
                }
            }

            // --- Synonym groups (_2.6-al) ---
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Synonym Groups", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Define synonym groups for search expansion. Each group maps a canonical term to alternative " +
                "search terms. At build time, any POI indexed with the key term will also match its synonyms.",
                MessageType.None);
            DrawSynonymGroupsTable();

            _hasUnsavedChanges = true;
        }

        // Renders the synonym groups table: key + synonyms list + add/remove.
        private void DrawSynonymGroupsTable()
        {
            if (_config.synonym_groups == null)
                _config.synonym_groups = new List<SynonymGroup>();

            // Column headers.
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Key (canonical term)", EditorStyles.miniBoldLabel, GUILayout.Width(140f));
                EditorGUILayout.LabelField("Synonyms (comma-separated)", EditorStyles.miniBoldLabel, GUILayout.Width(200f));
                GUILayout.FlexibleSpace();
            }

            // Editable rows.
            for (int i = 0; i < _config.synonym_groups.Count; i++)
            {
                var group = _config.synonym_groups[i];
                using (new EditorGUILayout.HorizontalScope())
                {
                    // Key field.
                    string newKey = EditorGUILayout.TextField(group.key ?? string.Empty, GUILayout.Width(140f));
                    if (newKey != group.key)
                    {
                        group.key = newKey;
                        _hasUnsavedChanges = true;
                    }

                    // Synonyms field (comma-separated).
                    string synonymsStr = group.synonyms != null ? string.Join(", ", group.synonyms) : string.Empty;
                    string newSynonymsStr = EditorGUILayout.TextField(synonymsStr, GUILayout.Width(200f));
                    if (newSynonymsStr != synonymsStr)
                    {
                        group.synonyms = new List<string>();
                        foreach (var syn in newSynonymsStr.Split(','))
                        {
                            string trimmed = syn.Trim();
                            if (!string.IsNullOrEmpty(trimmed))
                                group.synonyms.Add(trimmed);
                        }
                        _hasUnsavedChanges = true;
                    }

                    GUILayout.FlexibleSpace();

                    // Remove button.
                    if (GUILayout.Button(TrashIcon, GUILayout.Width(26f), GUILayout.Height(22f)))
                    {
                        _config.synonym_groups.RemoveAt(i);
                        _hasUnsavedChanges = true;
                        break;
                    }
                }
            }

            // Add new group button.
            EditorGUILayout.Space(2f);
            if (GUILayout.Button("+ Add synonym group", GUILayout.Width(160f)))
            {
                _config.synonym_groups.Add(new SynonymGroup
                {
                    key = string.Empty,
                    synonyms = new List<string>()
                });
                _hasUnsavedChanges = true;
            }
        }

        // Renders the custom keyword field definitions table.
        // Three row types: read-only system rows (category/hierarchy/badge/outline),
        // editable custom rows (from search_fields), and a permanent "Others" row.
        private void DrawSearchFieldsTable()
        {
            if (_config.search_fields == null)
                _config.search_fields = new List<SearchFieldDefinition>();

            // Column headers.
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Key", EditorStyles.miniBoldLabel, GUILayout.Width(90f));
                EditorGUILayout.LabelField("Label", EditorStyles.miniBoldLabel, GUILayout.Width(100f));
                EditorGUILayout.LabelField("Forced", EditorStyles.miniBoldLabel, GUILayout.Width(50f));
                EditorGUILayout.LabelField("Details", EditorStyles.miniBoldLabel, GUILayout.Width(50f));
                GUILayout.FlexibleSpace();
            }

            // Read-only system rows (derived from taxonomy tables, shown for context).
            DrawSystemKeywordRow("category", "Category");
            DrawSystemKeywordRow("hierarchy", "Hierarchy Level");
            DrawSystemKeywordRow("badge", "Badge");
            DrawSystemKeywordRow("outline", "Outline / Status");

            // Editable custom rows.
            for (int i = 0; i < _config.search_fields.Count; i++)
            {
                var field = _config.search_fields[i];
                using (new EditorGUILayout.HorizontalScope())
                {
                    // Key (editable but warn if changed after first authoring session).
                    string newKey = EditorGUILayout.TextField(field.key ?? string.Empty, GUILayout.Width(90f));
                    if (newKey != field.key)
                    {
                        field.key = newKey;
                        _hasUnsavedChanges = true;
                    }

                    // Label.
                    string newLabel = EditorGUILayout.TextField(field.label ?? string.Empty, GUILayout.Width(100f));
                    if (newLabel != field.label)
                    {
                        field.label = newLabel;
                        _hasUnsavedChanges = true;
                    }

                    // Forced toggle.
                    bool newForced = EditorGUILayout.Toggle(field.forced, GUILayout.Width(50f));
                    if (newForced != field.forced)
                    {
                        field.forced = newForced;
                        _hasUnsavedChanges = true;
                    }

                    // Details popup + help.
                    using (new EditorGUILayout.HorizontalScope(GUILayout.Width(76f)))
                    {
                        if (GUILayout.Button("...", GUILayout.Width(26f)))
                            PopupWindow.Show(GUILayoutUtility.GetLastRect(), new EntryDetailsPopup(
                                field.label ?? field.key ?? "Field",
                                () => field.details,
                                v => { field.details = v; _hasUnsavedChanges = true; }));

                        if (GUILayout.Button("?", GUILayout.Width(26f)))
                            PopupWindow.Show(GUILayoutUtility.GetLastRect(), new HelpInfoPopup(
                                "Search Field Help",
                                $"Key: {SearchFieldKeyHelp}\n\nLabel: {SearchFieldLabelHelp}\n\nForced: {SearchFieldForcedHelp}\n\nDetails: {SearchFieldDetailsHelp}"));
                    }

                    GUILayout.FlexibleSpace();

                    // Remove button.
                    if (GUILayout.Button(TrashIcon, GUILayout.Width(26f), GUILayout.Height(22f)))
                    {
                        _config.search_fields.RemoveAt(i);
                        _hasUnsavedChanges = true;
                        break;
                    }
                }
            }

            // Permanent read-only "Others" row (the flat search_keywords bucket).
            DrawSystemKeywordRow("others", "Others (freeform)");

            // Add new field button.
            EditorGUILayout.Space(2f);
            if (GUILayout.Button("+ Add keyword field", GUILayout.Width(160f)))
            {
                _config.search_fields.Add(new SearchFieldDefinition
                {
                    key = "field_" + (_config.search_fields.Count + 1),
                    label = "New Field",
                    forced = false,
                    details = string.Empty
                });
                _hasUnsavedChanges = true;
            }
        }

        // Renders one read-only labelled row in the keyword fields table.
        private static void DrawSystemKeywordRow(string key, string label)
        {
            using (new EditorGUI.DisabledScope(true))
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.TextField(key, GUILayout.Width(90f));
                EditorGUILayout.TextField(label, GUILayout.Width(100f));
                EditorGUILayout.Toggle(false, GUILayout.Width(50f)); // forced always false for system rows
                GUILayout.Button("...", GUILayout.Width(26f));
            }
        }
    }
}