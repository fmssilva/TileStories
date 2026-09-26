// POIEditorToolWindow.SearchFilter.cs
//
// Partial: the Select, Filter & Search section of Global Scene (_2.6_Select_Filter_Search.md). Enable, then
// one sub-foldout per sub-block of select_filter_search -- Selection, Search, Filters, Results & Views,
// Minimap, Voice -- then Keywords & Synonyms (the vocabulary: custom keyword fields and synonym groups) and
// the shared Test sub-foldout (the search demo, Try a Query and the Live Search Readout). Texts live in
// SearchFilterHelp.cs; option values in SelectFilterSearchOptions (runtime).

using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    public partial class POIEditorToolWindow
    {
        // Keyword tables' own columns (this section's two tables only)
        private const float KeywordKeyColumnWidth = 110f;
        private const float KeywordLabelColumnWidth = 130f;
        private const float KeywordRequiredColumnWidth = 64f;
        private const float KeywordFilterColumnWidth = 44f;
        private const float SynonymKeyColumnWidth = 110f;
        private const float SynonymWordsColumnWidth = 230f;

        private bool _showSearchSelection = true;
        private bool _showSearchSearch = true;
        private bool _showSearchFilters = true;
        private bool _showSearchResults = true;
        private bool _showSearchMinimap = true;
        private bool _showSearchVoice = true;
        private bool _showSearchKeywords = true;
        private readonly TestGuideState _searchTest = new TestGuideState();
        private string _tryQuery = "";

        private void DrawGlobalSearchFilterSection()
        {
            if (_config == null) return;
            _config.select_filter_search ??= new SelectFilterSearchSettings();
            var s = _config.select_filter_search;

            s.enabled = DrawToggleField("Enable Select & Search", s.enabled, SearchEnabledHelp);

            // off hides the settings (their values are kept); the Test stays, like every domain's
            if (s.enabled)
            {
                DrawSearchSelectionSubSection(s.selection ??= new SelectionSettings());
                DrawSearchSearchSubSection(s.search ??= new SearchSettings());
                DrawSearchFiltersSubSection(s.filter ??= new FilterSettings());
                DrawSearchResultsSubSection(s.results ??= new ResultsSettings());
                DrawSearchMinimapSubSection(s.minimap ??= new MinimapSettings());
                DrawSearchVoiceSubSection(s.voice ??= new VoiceSettings());
                DrawSearchKeywordsSubSection();
            }

            DrawDomainTestSubSection(_searchTest, SearchSceneTestGuide, SearchPlaymodeTestGuide, SearchDeviceTestGuide,
                DrawSearchTestControls);
        }

        // What a tap does: the highlight dim and zoom-on-select
        private void DrawSearchSelectionSubSection(SelectionSettings sel)
        {
            if (!SubFoldout(ref _showSearchSelection, "Selection")) return;
            float inner = IndentLevel1 + ConditionalAdvance;

            sel.highlight_enabled = DrawToggleField("Highlight Selection", sel.highlight_enabled, HighlightSelectionHelp, IndentLevel1);
            if (sel.highlight_enabled)
                sel.dim_alpha = DrawSliderField("Dim Others To", sel.dim_alpha, SelectionSettings.DimAlphaMin,
                    SelectionSettings.DimAlphaMax, DimOthersHelp, inner);

            var zoom = sel.zoom ??= new ZoomOnSelectSettings();
            zoom.enabled = DrawToggleField("Zoom on Select", zoom.enabled, ZoomOnSelectHelp, IndentLevel1);
            if (!zoom.enabled) return;
            zoom.trigger = DrawPopupField("Zoom Trigger", zoom.trigger, SelectFilterSearchOptions.Triggers, ZoomTriggerLabels,
                ZoomTriggerHelp, inner);
            zoom.neighbour_radius_px = DrawSliderField("Crowd Radius (px)", zoom.neighbour_radius_px,
                ZoomOnSelectSettings.NeighbourRadiusMin, ZoomOnSelectSettings.NeighbourRadiusMax, CrowdRadiusHelp, inner);
            zoom.min_neighbours = DrawIntSliderField("Min Neighbours", zoom.min_neighbours, ZoomOnSelectSettings.MinNeighboursMin,
                ZoomOnSelectSettings.MinNeighboursMax, MinNeighboursHelp, inner);
            zoom.factor = DrawSliderField("Zoom Factor", zoom.factor, ZoomOnSelectSettings.FactorMin, ZoomOnSelectSettings.FactorMax,
                ZoomFactorHelp, inner);
        }

        // How a query matches points
        private void DrawSearchSearchSubSection(SearchSettings search)
        {
            if (!SubFoldout(ref _showSearchSearch, "Search")) return;

            search.mode = DrawPopupField("Search Mode", search.mode, SelectFilterSearchOptions.Modes, SearchModeLabels, SearchModeHelp, IndentLevel1);
            search.match_mode = DrawPopupField("Match Words", search.match_mode, SelectFilterSearchOptions.MatchModes, MatchModeLabels,
                MatchModeHelp, IndentLevel1);
            search.prefix_matching = DrawPopupField("Partial Words", search.prefix_matching, SelectFilterSearchOptions.PrefixModes,
                PrefixModeLabels, PrefixModeHelp, IndentLevel1);
            search.typo_tolerance = DrawIntSliderField("Typo Tolerance", search.typo_tolerance, 0, SearchSettings.TypoToleranceMax,
                TypoToleranceHelp, IndentLevel1);
            search.no_results_message = DrawTextRow("No-Results Message", search.no_results_message, NoResultsHelp, IndentLevel1);
            search.no_results_filters_message = DrawTextRow("No-Results (Filters)", search.no_results_filters_message, NoResultsFiltersHelp, IndentLevel1);
        }

        // Which filters the visitor gets and what happens to everything else
        private void DrawSearchFiltersSubSection(FilterSettings filter)
        {
            if (!SubFoldout(ref _showSearchFilters, "Filters")) return;

            filter.category_facet = DrawToggleField("Category Filter", filter.category_facet, FacetGroupsHelp, IndentLevel1);
            filter.badge_facet = DrawToggleField("Badge Filter", filter.badge_facet, FacetGroupsHelp, IndentLevel1);
            filter.status_facet = DrawToggleField("Status Filter", filter.status_facet, FacetGroupsHelp, IndentLevel1);
            filter.hierarchy_facet = DrawToggleField("Level Filter", filter.hierarchy_facet, FacetGroupsHelp, IndentLevel1);
            filter.mismatch = DrawPopupField("Filtered-Out Markers", filter.mismatch, SelectFilterSearchOptions.Mismatches,
                MismatchLabels, MismatchHelp, IndentLevel1);
            if (filter.mismatch == SelectFilterSearchOptions.MismatchDim)
                filter.dim_alpha = DrawSliderField("Dim Filtered To", filter.dim_alpha, 0f, 1f, DimFilteredHelp, IndentLevel1 + ConditionalAdvance);
            filter.relax_suggestion = DrawToggleField("Relax Suggestion", filter.relax_suggestion, RelaxHelp, IndentLevel1);
        }

        // How results are shown, recent searches and suggestions
        private void DrawSearchResultsSubSection(ResultsSettings results)
        {
            if (!SubFoldout(ref _showSearchResults, "Results & Views")) return;

            results.default_view = DrawPopupField("Default View", results.default_view, SelectFilterSearchOptions.Views,
                ResultViewLabels, DefaultViewHelp, IndentLevel1);
            results.remember_last_view = DrawToggleField("Remember Last View", results.remember_last_view, RememberViewHelp, IndentLevel1);
            results.recent_count = DrawIntSliderField("Recent Searches", results.recent_count, 0, ResultsSettings.RecentCountMax,
                RecentSearchesHelp, IndentLevel1);
            results.suggestions_enabled = DrawToggleField("Suggestions", results.suggestions_enabled, SuggestionsHelp, IndentLevel1);
            if (results.suggestions_enabled)
                results.suggestion_source = DrawPopupField("Suggestion Source", results.suggestion_source,
                    SelectFilterSearchOptions.SuggestionSources, SuggestionSourceLabels, SuggestionsHelp, IndentLevel1 + ConditionalAdvance);
        }

        // The overview map
        private void DrawSearchMinimapSubSection(MinimapSettings map)
        {
            if (!SubFoldout(ref _showSearchMinimap, "Minimap")) return;

            map.enabled = DrawToggleField("Enable Minimap", map.enabled, MinimapEnabledHelp, IndentLevel1);
            if (!map.enabled) return;
            float inner = IndentLevel1 + ConditionalAdvance;
            map.visibility = DrawPopupField("Visibility", map.visibility, SelectFilterSearchOptions.Visibilities,
                MinimapVisibilityLabels, MinimapVisibilityHelp, inner);
            map.icon_style = DrawPopupField("Dot Style", map.icon_style, SelectFilterSearchOptions.IconStyles,
                MinimapIconLabels, MinimapIconHelp, inner);
            map.dot_size_px = DrawSliderField("Dot Size (px)", map.dot_size_px, MinimapSettings.DotSizeMin, MinimapSettings.DotSizeMax,
                MinimapDotSizeHelp, inner);
            map.tap_target_px = DrawSliderField("Tap Target (px)", map.tap_target_px, MinimapSettings.TapTargetMin,
                MinimapSettings.TapTargetMax, MinimapTapTargetHelp, inner);
            map.projection = DrawPopupField("Projection", map.projection, SelectFilterSearchOptions.Projections,
                MinimapProjectionLabels, MinimapProjectionHelp, inner);
            map.bounds_mode = DrawPopupField("Bounds", map.bounds_mode, SelectFilterSearchOptions.BoundsModes,
                MinimapBoundsLabels, MinimapBoundsHelp, inner);
            if (map.bounds_mode != SelectFilterSearchOptions.BoundsManual) return;
            map.bounds_min = DrawVector3Row("Bounds Min (m)", map.bounds_min, MinimapBoundsHelp, inner + ConditionalAdvance);
            map.bounds_max = DrawVector3Row("Bounds Max (m)", map.bounds_max, MinimapBoundsHelp, inner + ConditionalAdvance);
        }

        // Speech input
        private void DrawSearchVoiceSubSection(VoiceSettings voice)
        {
            if (!SubFoldout(ref _showSearchVoice, "Voice")) return;

            voice.enabled = DrawToggleField("Enable Voice Search", voice.enabled, VoiceEnabledHelp, IndentLevel1);
            if (voice.enabled)
                voice.indicator_style = DrawPopupField("Voice Indicator", voice.indicator_style, SelectFilterSearchOptions.IndicatorStyles,
                    VoiceIndicatorLabels, VoiceIndicatorHelp, IndentLevel1 + ConditionalAdvance);
        }

        // The search vocabulary: custom keyword fields and synonym groups
        private void DrawSearchKeywordsSubSection()
        {
            if (!SubFoldout(ref _showSearchKeywords, "Keywords & Synonyms")) return;

            DrawEditorRow(out float rowWidth, out _, IndentLevel1);
            GUILayout.Label("Keyword Fields", EditorStyles.miniBoldLabel, GUILayout.Width(Mathf.Max(40f, rowWidth - 36f)), GUILayout.ExpandWidth(false));
            HelpInfoButton.Draw("Search Keywords", KeywordFieldsHelp);
            EditorRowEnd();
            DrawKeywordFieldsTable();

            EditorGUILayout.Space(6f);
            DrawEditorRow(out float synRow, out _, IndentLevel1);
            GUILayout.Label("Synonym Groups", EditorStyles.miniBoldLabel, GUILayout.Width(Mathf.Max(40f, synRow - 36f)), GUILayout.ExpandWidth(false));
            HelpInfoButton.Draw("Synonym Groups", SynonymGroupsHelp);
            EditorRowEnd();
            DrawSynonymGroupsTable();
        }

        // A collapsible sub-foldout title; true while open
        private static bool SubFoldout(ref bool open, string title)
        {
            EditorGUILayout.Space(4f);
            open = EditorGUILayout.Foldout(open, title, true, EditorStyles.foldoutHeader);
            return open;
        }

        // A labelled single-line text row on the shared row recipe
        private static string DrawTextRow(string label, string value, string helpText, float extraIndentPixels)
        {
            DrawEditorRow(out float rowWidth, out _, extraIndentPixels);
            using (new FieldLabelWidthCompensationScope(extraIndentPixels))
                value = EditorGUILayout.TextField(label, value ?? "", GUILayout.Width(rowWidth - 36f), GUILayout.ExpandWidth(false));
            HelpInfoButton.Draw(label, helpText);
            EditorRowEnd();
            return value;
        }

        // A labelled Vector3 row on the shared row recipe (one line: IMGUI drops X/Y/Z onto a second
        // line whenever wideMode is off, so it is forced on for this row only)
        private static Vector3 DrawVector3Row(string label, Vector3 value, string helpText, float extraIndentPixels)
        {
            DrawEditorRow(out float rowWidth, out _, extraIndentPixels);
            bool wasWide = EditorGUIUtility.wideMode;
            EditorGUIUtility.wideMode = true;
            using (new FieldLabelWidthCompensationScope(extraIndentPixels))
                value = EditorGUILayout.Vector3Field(label, value, GUILayout.Width(rowWidth - 36f), GUILayout.ExpandWidth(false));
            EditorGUIUtility.wideMode = wasWide;
            HelpInfoButton.Draw(label, helpText);
            EditorRowEnd();
            return value;
        }

        // Commit-style rename for the keyword field keys: a SearchFieldDefinition.key is the identity each
        // POI's search_keyword_fields[].field_key points at, so a rename moves every POI's list along
        // (SearchFieldReferenceResolver) instead of stranding it under the old key.
        private IdentityRenameEditState<SearchFieldDefinition> _searchFieldRenameEdit;

        private IdentityRenameEditState<SearchFieldDefinition> SearchFieldRenameEdit =>
            _searchFieldRenameEdit ??= new IdentityRenameEditState<SearchFieldDefinition>(
                "TileStories.SearchFieldEdit",
                () => _config?.search_fields,
                e => e.key,
                (e, v) => e.key = v,
                () => _config?.pois,
                SearchFieldReferenceResolver.RenameFieldKey);

        // Custom keyword fields: Key, Label, Required, Filter, Details, delete
        private void DrawKeywordFieldsTable()
        {
            _config.search_fields ??= new List<SearchFieldDefinition>();
            SearchFieldRenameEdit.HandleEditEvents();

            using (new TableRowScope())
            {
                GUILayout.Space(IndentLevel1);
                GUILayout.Label("Key", EditorStyles.miniBoldLabel, GUILayout.Width(KeywordKeyColumnWidth));
                GUILayout.Space(TableGapWithinGroup);
                GUILayout.Label("Label", EditorStyles.miniBoldLabel, GUILayout.Width(KeywordLabelColumnWidth));
                GUILayout.Space(TableGapBetweenGroups);
                GUILayout.Label("Required", EditorStyles.miniBoldLabel, GUILayout.Width(KeywordRequiredColumnWidth));
                GUILayout.Label("Filter", EditorStyles.miniBoldLabel, GUILayout.Width(KeywordFilterColumnWidth));
                // - one (i) for the four columns, above the Details column (the tables' convention)
                HelpInfoButton.Draw("Keyword Fields", "Key: " + KeywordFieldKeyHelp + "\n\nLabel: " + KeywordFieldLabelHelp +
                    "\n\nRequired: " + KeywordFieldRequiredHelp + "\n\nFilter: " + KeywordFieldFilterHelp, 26f);
                GUILayout.FlexibleSpace();
                GUILayout.Space(AddButtonRowRightMargin);
            }

            int deleteIndex = -1;
            for (int i = 0; i < _config.search_fields.Count; i++)
            {
                var field = _config.search_fields[i];
                using (new TableRowScope())
                {
                    GUILayout.Space(IndentLevel1);
                    SearchFieldRenameEdit.SetLabel(field,
                        EditorGUILayout.TextField(SearchFieldRenameEdit.GetLabel(field), GUILayout.Width(KeywordKeyColumnWidth)));
                    GUILayout.Space(TableGapWithinGroup);
                    field.label = EditorGUILayout.TextField(field.label ?? "", GUILayout.Width(KeywordLabelColumnWidth));
                    GUILayout.Space(TableGapBetweenGroups);
                    field.forced = EditorGUILayout.Toggle(field.forced, GUILayout.Width(KeywordRequiredColumnWidth));
                    field.filterable = EditorGUILayout.Toggle(field.filterable, GUILayout.Width(KeywordFilterColumnWidth));
                    if (GUILayout.Button(DetailsIcon, GUILayout.Width(26f), GUILayout.Height(20f)))
                        EditorPopup.ShowAt(CreateDetailsPopup(
                            field.label ?? field.key ?? "Field", () => field.details, v => field.details = v), GUILayoutUtility.GetLastRect());
                    GUILayout.FlexibleSpace();
                    GUILayout.Space(TableGapBeforeDelete);
                    if (DeleteButton.DrawLayout($"Delete keyword field: {field.key}")) deleteIndex = i;
                    GUILayout.Space(AddButtonRowRightMargin);
                }
            }

            // - applied after the loop: a row is never left half-drawn
            if (deleteIndex >= 0)
            {
                var field = _config.search_fields[deleteIndex];
                if (IdentityDeleteGuard.Confirm("Keyword field", field.key,
                        SearchFieldReferenceResolver.CountPoisUsingField(_config.pois, field.key)))
                    _config.search_fields.RemoveAt(deleteIndex);
            }

            DrawEditorRow(out float rowWidth, out _, IndentLevel1);
            if (GUILayout.Button("+ Add keyword field", GUILayout.Width(rowWidth), GUILayout.ExpandWidth(false)))
                _config.search_fields.Add(new SearchFieldDefinition
                {
                    key = NextFreeSearchFieldKey(_config.search_fields),
                    label = "New Field",
                    details = "",
                });
            EditorRowEnd();
        }

        // "field_1", "field_2", ...: the first key no row uses
        internal static string NextFreeSearchFieldKey(List<SearchFieldDefinition> fields)
        {
            for (int n = 1; ; n++)
            {
                string key = "field_" + n;
                if (fields == null || !fields.Exists(f => f != null && f.key == key)) return key;
            }
        }

        // Synonym groups: Key, the other words, delete
        private void DrawSynonymGroupsTable()
        {
            _config.synonym_groups ??= new List<SynonymGroup>();

            using (new TableRowScope())
            {
                GUILayout.Space(IndentLevel1);
                GUILayout.Label("Word", EditorStyles.miniBoldLabel, GUILayout.Width(SynonymKeyColumnWidth));
                GUILayout.Space(TableGapWithinGroup);
                GUILayout.Label("Same meaning (comma-separated)", EditorStyles.miniBoldLabel, GUILayout.Width(SynonymWordsColumnWidth));
                GUILayout.FlexibleSpace();
                GUILayout.Space(AddButtonRowRightMargin);
            }

            int deleteIndex = -1;
            for (int i = 0; i < _config.synonym_groups.Count; i++)
            {
                var group = _config.synonym_groups[i];
                using (new TableRowScope())
                {
                    GUILayout.Space(IndentLevel1);
                    group.key = EditorGUILayout.TextField(group.key ?? "", GUILayout.Width(SynonymKeyColumnWidth));
                    GUILayout.Space(TableGapWithinGroup);
                    group.synonyms = DrawKeywordListField(group.synonyms, GUILayout.Width(SynonymWordsColumnWidth));
                    GUILayout.FlexibleSpace();
                    GUILayout.Space(TableGapBeforeDelete);
                    if (DeleteButton.DrawLayout($"Delete synonym group: {group.key}")) deleteIndex = i;
                    GUILayout.Space(AddButtonRowRightMargin);
                }
            }
            if (deleteIndex >= 0) _config.synonym_groups.RemoveAt(deleteIndex);

            DrawEditorRow(out float rowWidth, out _, IndentLevel1);
            if (GUILayout.Button("+ Add synonym group", GUILayout.Width(rowWidth), GUILayout.ExpandWidth(false)))
                _config.synonym_groups.Add(new SynonymGroup { key = "", synonyms = new List<string>() });
            EditorRowEnd();
        }

        // Select, Filter & Search > Test, before the guides: the demo, Try a Query, the readout
        private void DrawSearchTestControls()
        {
            DrawSearchDemoControls();
            DrawTryQueryRow();
            DrawSearchLiveReadout();
        }

        // "Add search demo" and, while it is on, its settings (built by SearchDemoSpawner from WallSession)
        private void DrawSearchDemoControls()
        {
            _config.search_demo ??= new SearchDemoSettings();
            var demo = _config.search_demo;

            bool wasOn = demo.enabled;
            demo.enabled = DrawToggleField("Add search demo", demo.enabled, SearchDemoHelp, IndentLevel1);
            MakeThisTheOnlyActiveDemoView(DemoView.SearchDemo, demo.enabled && !wasOn);
            if (!demo.enabled) return;

            float inner = IndentLevel1 + ConditionalAdvance;
            demo.markers_per_category = DrawIntSliderField("Markers per Category", demo.markers_per_category, 1,
                SearchDemoSettings.MaxMarkersPerCategory, SearchDemoPerCategoryHelp, inner);
            demo.test_cases = DrawToggleField("Test Cases", demo.test_cases, SearchDemoTestCasesHelp, inner);
            demo.show_labels = DrawToggleField("Show labels", demo.show_labels, SearchDemoLabelsHelp, inner);
            demo.distance_m = DrawSliderField("Distance (m)", demo.distance_m, SearchDemoSettings.MinDistanceM,
                SearchDemoSettings.MaxDistanceM, SearchDemoDistanceHelp, inner);
            demo.spacing_cm = DrawSliderField("Spacing (cm)", demo.spacing_cm, SearchDemoSettings.MinSpacingCm,
                SearchDemoSettings.MaxSpacingCm, SearchDemoSpacingHelp, inner);
            demo.run_lod = DrawToggleField("Run LOD on the demo", demo.run_lod, SearchDemoRunLodHelp, inner);
        }

        // "Try a Query": search a text in the running app as if the visitor typed it (it is also what the
        // Editor mic hears); with the demo's test cases on, "Demo Query" picks one of them
        private void DrawTryQueryRow()
        {
            EditorGUILayout.Space(4f);
            DrawEditorRow(out float rowWidth, out _, IndentLevel1);
            using (new EditorGUI.DisabledScope(!Application.isPlaying))
            {
                using (new FieldLabelWidthCompensationScope(IndentLevel1))
                    _tryQuery = EditorGUILayout.TextField("Try a Query", _tryQuery, GUILayout.Width(rowWidth - 36f - 70f), GUILayout.ExpandWidth(false));
                if (GUILayout.Button("Search", GUILayout.Width(66f), GUILayout.ExpandWidth(false)))
                    RunTryQuery(_tryQuery);
            }
            HelpInfoButton.Draw("Try a Query", TryQueryHelp);
            EditorRowEnd();
            if (!string.IsNullOrWhiteSpace(_tryQuery)) DebugTranscriber.EditorPhrase = _tryQuery;

            var demo = _config.search_demo;
            if (demo == null || !demo.enabled || !demo.test_cases) return;
            // - the popup's values are the queries themselves; "" = nothing picked (it always shows that again)
            var cases = SearchDemoLayout.TestCases();
            var queries = new string[cases.Count + 1];
            var labels = new string[cases.Count + 1];
            queries[0] = "";
            labels[0] = "(pick a test case)";
            for (int i = 0; i < cases.Count; i++)
            {
                queries[i + 1] = cases[i].Query;
                labels[i + 1] = $"'{cases[i].Query}' - {cases[i].Note}";
            }
            using (new EditorGUI.DisabledScope(!Application.isPlaying))
            {
                string picked = DrawPopupField("Demo Query", "", queries, labels, DemoQueryHelp, IndentLevel1 + ConditionalAdvance);
                if (!string.IsNullOrEmpty(picked))
                {
                    _tryQuery = picked;
                    RunTryQuery(picked);
                }
            }
        }

        private static void RunTryQuery(string query) => FindFirstObjectByType<SearchUIHost>()?.SetQuery(query);

        // "Live Search Readout": in Play Mode, what the running search shows right now
        private void DrawSearchLiveReadout()
        {
            EditorGUILayout.Space(4f);
            DrawEditorRow(out float rowWidth, out _, IndentLevel1);
            GUILayout.Label("Live Search Readout", EditorStyles.miniBoldLabel, GUILayout.Width(Mathf.Max(40f, rowWidth - 36f)), GUILayout.ExpandWidth(false));
            HelpInfoButton.Draw("Live Search Readout", SearchReadoutHelp);
            EditorRowEnd();

            string text;
            if (!Application.isPlaying) text = "Press Play to see what the search shows, live.";
            else
            {
                _liveReadoutDrawnAt = EditorApplication.timeSinceStartup;
                var host = FindFirstObjectByType<SearchUIHost>();
                var wall = FindFirstObjectByType<WallSession>();
                // - the demo's test cases, judged on the running index with the running settings
                var demo = _config.search_demo;
                var verdicts = wall != null && wall.SearchDemoRoot != null && demo != null && demo.test_cases
                    ? SearchDemoCheck.Evaluate(wall.SearchIndex, wall.SelectFilterSearch?.search)
                    : null;
                text = host == null || wall == null ? "The running scene has no search UI (a Search UI object with a SearchUIHost)."
                    : SearchLiveReadout(host.Query, host.Tray?.Selection.Count ?? 0, wall.SearchPois.Count,
                        wall.SearchIndex?.TokenCount ?? 0, host.State, verdicts);
            }
            DrawEditorRow(out float textWidth, out _, IndentLevel1);
            GUILayout.Label(text, EditorStyles.helpBox, GUILayout.Width(textWidth), GUILayout.ExpandWidth(false));
            EditorRowEnd();
        }

        // The readout's text, one fact per line (pure, so its wording is tested); with the demo's test cases,
        // one verdict line per case at the end
        internal static string SearchLiveReadout(string query, int activeFilters, int searchablePois, int indexedWords, ResultSetState state,
            IList<SearchDemoCheck.Verdict> demoVerdicts = null)
        {
            var inv = System.Globalization.CultureInfo.InvariantCulture;
            var sb = new StringBuilder();
            sb.Append("Searchable points: ").Append(searchablePois).Append(" (").Append(indexedWords).Append(" indexed words)");
            sb.Append("\nQuery: ").Append(string.IsNullOrEmpty(query) ? "(none)" : "'" + query + "'");
            sb.Append("\nActive filters: ").Append(activeFilters);
            if (state == null || !state.Active)
                sb.Append("\nResults: none shown (no query, no filter)");
            else
            {
                sb.Append("\nResults: ").Append(state.Results.Count);
                for (int i = 0; i < state.Results.Count && i < 8; i++)
                {
                    var r = state.Results[i];
                    sb.Append("\n  ").Append(i + 1).Append(". ").Append(r.Poi.name).Append("  ")
                      .Append(r.Score.ToString("0.00", inv));
                    if (!string.IsNullOrEmpty(r.Reason)) sb.Append(" (").Append(r.Reason).Append(')');
                }
                if (state.Results.Count == 0 && state.Relax.HasValue)
                    sb.Append("\nRelax suggestion: remove ").Append(state.Relax.Value.Group).Append(" '").Append(state.Relax.Value.Key)
                      .Append("' (").Append(state.Relax.Value.ResultCount).Append(" results)");
            }

            if (demoVerdicts != null && demoVerdicts.Count > 0)
            {
                int ok = 0;
                foreach (var v in demoVerdicts) if (v.Ok) ok++;
                sb.Append("\n\nDemo Test Cases: ").Append(ok).Append('/').Append(demoVerdicts.Count).Append(" as expected");
                foreach (var v in demoVerdicts) sb.Append('\n').Append(SearchDemoCheck.Line(v));
            }
            return sb.ToString();
        }
    }
}
