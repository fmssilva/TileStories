// POIEditorToolWindow.LodZoom.cs
//
// Partial: the LOD and Zoom sections of Global Scene (_2.4_Marker_LOD.md) -- their sub-foldouts,
// the Distance Bands table, the LOD demo field controls and the live LOD readout. Texts live in
// LodZoomHelp.cs, rules in LodEditorRules.cs, the shared row drawers in Shared/FieldRows.cs.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    public partial class POIEditorToolWindow
    {
        // ---- LOD section ----
        // Sub-foldouts in the order a developer tunes them: Distance Bands (how many, by distance),
        // Crowding (what a crowded marker does), Clusters (only when clusters can happen),
        // Transitions & Performance, then the shared Test sub-foldout with the LOD demo field.

        private bool _showLodBands = true;
        private bool _showLodCrowding = true;
        private bool _showLodClusters = true;
        private bool _showLodTransitions = true;
        private readonly TestGuideState _lodTest = new TestGuideState();

        // Column widths of the Distance Bands table
        private const float LodBandDistanceWidth = 90f;
        private const float LodBandCountWidth = 90f;
        private const float LodBandDetailsWidth = 26f;

        // Test seam: reports each band table cell's drawn rect (column name, row index) on Repaint.
        // Null in production, so zero cost -- the same idea as HierarchyCheckboxRectProbe.
        internal static Action<string, int, Rect> LodBandCellRectProbe;

        private void DrawGlobalLodSection()
        {
            if (_config == null || _config.lod_settings == null)
            {
                EditorGUILayout.HelpBox("No LOD settings to configure.", MessageType.Info);
                return;
            }

            var lod = _config.lod_settings;
            lod.enabled = DrawToggleField("Enable LOD", lod.enabled, LodEnabledHelp);

            // off hides the settings (their values are kept); the Test stays, like every domain's
            if (lod.enabled)
            {
                DrawLodBandsSubSection(lod);
                DrawLodCrowdingSubSection(lod);
                if (LodEditorRules.CanBuildClusters(lod))
                    DrawLodClustersSubSection(lod);
                DrawLodTransitionsSubSection(lod);
            }

            DrawDomainTestSubSection(_lodTest, LodSceneTestGuide, LodPlaymodeTestGuide, LodDeviceTestGuide, DrawLodTestControls);
        }

        // LOD > Test, before the guides: the demo field, then what LOD is doing right now
        private void DrawLodTestControls()
        {
            DrawLodDemoFieldControls();
            DrawLodLiveReadout();
        }

        // When a live readout (LOD, Displacement) was last drawn: OnInspectorUpdate keeps it moving only
        // while one is on screen
        private double _liveReadoutDrawnAt = -1d;

        // "Live LOD Readout": in Play Mode, what the running LOD pipeline decided in its last evaluation
        private void DrawLodLiveReadout()
        {
            EditorGUILayout.Space(4f);
            DrawEditorRow(out float rowWidth, out _, IndentLevel1);
            GUILayout.Label("Live LOD Readout", EditorStyles.miniBoldLabel, GUILayout.Width(Mathf.Max(40f, rowWidth - 36f)), GUILayout.ExpandWidth(false));
            HelpInfoButton.Draw("Live LOD Readout", LodReadoutHelp);
            EditorRowEnd();

            string text;
            if (!Application.isPlaying) text = "Press Play to see what LOD decides, live.";
            else
            {
                _liveReadoutDrawnAt = EditorApplication.timeSinceStartup;
                var lod = FindFirstObjectByType<LODController>();
                text = lod == null ? "The running scene has no LOD pipeline."
                    : lod.LastStats == null ? "LOD is off (Enable LOD) or has not evaluated yet."
                    : LodLiveReadout(lod.LastStats);
            }
            DrawEditorRow(out float textWidth, out _, IndentLevel1);
            GUILayout.Label(text, EditorStyles.helpBox, GUILayout.Width(textWidth), GUILayout.ExpandWidth(false));
            EditorRowEnd();
        }

        // Keep the live readouts moving while Play runs and one was drawn in the last second
        private void RepaintLiveReadoutsWhilePlaying()
        {
            if (Application.isPlaying && EditorApplication.timeSinceStartup - _liveReadoutDrawnAt < 1d)
                Repaint();
        }

        // The readout's text, one fact per line (pure, so its wording is tested)
        internal static string LodLiveReadout(LodStats s)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append("Shown: ").Append(s.Shown).Append(" of ").Append(s.Markers).Append(" markers");
            if (s.Shrunk > 0) sb.Append(" (").Append(s.Shrunk).Append(" smaller and fainter)");
            sb.Append("\nClusters: ").Append(s.Clusters);
            if (s.Clusters > 0) sb.Append(" (holding ").Append(s.InClusters).Append(" markers)");
            sb.Append("\nHidden by crowding: ").Append(s.HiddenByCrowding)
              .Append(" | by Max markers: ").Append(s.HiddenByMaxMarkers)
              .Append(" | out of view: ").Append(s.OutOfView);
            sb.Append("\nZoom: ").Append(s.ZoomFactor.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture)).Append("x");
            for (int i = 0; i < s.Bands.Count; i++)
            {
                var b = s.Bands[i];
                sb.Append("\nBand ").Append(i + 1).Append(" (up to ")
                  .Append(b.MaxDistanceM.ToString("0.#", System.Globalization.CultureInfo.InvariantCulture)).Append(" m, ")
                  .Append(b.MaxVisibleCount < 0 ? "all" : "max " + b.MaxVisibleCount).Append("): ")
                  .Append(b.Shown).Append(" of ").Append(b.InBand).Append(" shown");
            }
            return sb.ToString();
        }

        private void DrawLodBandsSubSection(LodSettings lod)
        {
            EditorGUILayout.Space(4f);
            _showLodBands = EditorGUILayout.Foldout(_showLodBands, "Distance Bands", true, EditorStyles.foldoutHeader);
            if (!_showLodBands) return;

            if (lod.bands == null) lod.bands = new List<LodBandEntry>();
            DrawLodBandTable(lod.bands);

            foreach (string problem in LodEditorRules.BandProblems(lod.bands))
                EditorGUILayout.HelpBox(problem, MessageType.Warning);

            // + Add band and Suggest Values share one row, half the width each
            DrawEditorRow(out float rowWidth, out _, IndentLevel1);
            float half = (rowWidth - 4f) * 0.5f;
            if (GUILayout.Button("+ Add band", GUILayout.Width(half), GUILayout.ExpandWidth(false)))
            {
                // a new far row: 1 m beyond the current last one, same count as that row
                var last = lod.bands.Count > 0 ? lod.bands[lod.bands.Count - 1] : null;
                lod.bands.Add(new LodBandEntry
                {
                    max_distance_m = last != null ? last.max_distance_m + 1f : 2f,
                    max_visible_count = last != null ? last.max_visible_count : -1,
                });
            }
            GUILayout.Space(4f);
            if (GUILayout.Button("Suggest Values", GUILayout.Width(half), GUILayout.ExpandWidth(false)))
                ApplySuggestedLodValues();
            EditorRowEnd();

            lod.hysteresis_margin_m = Mathf.Max(0f, DrawScalarField("Band Hysteresis (m)", lod.hysteresis_margin_m, LodBandHysteresisHelp, IndentLevel1));
        }

        // The Distance Bands table: header + one row per band, each row a TableRowScope (the cells are
        // EditorGUI rect controls). A delete is applied after the loop, so no layout group is ever left open.
        private void DrawLodBandTable(List<LodBandEntry> bands)
        {
            using (new TableRowScope())
            {
                GUILayout.Space(IndentLevel1);
                GUILayout.Label("Up to (m)", EditorStyles.miniBoldLabel, GUILayout.Width(LodBandDistanceWidth));
                GUILayout.Space(TableGapWithinGroup);
                GUILayout.Label("Max markers", EditorStyles.miniBoldLabel, GUILayout.Width(LodBandCountWidth));
                GUILayout.Space(TableGapBetweenGroups);
                HelpInfoButton.Draw("Distance Bands", LodBandsHelp, LodBandDetailsWidth);
                GUILayout.FlexibleSpace();
                GUILayout.Space(AddButtonRowRightMargin);
            }

            int deleteIndex = -1;
            for (int i = 0; i < bands.Count; i++)
            {
                var band = bands[i];
                using (new TableRowScope())
                {
                    GUILayout.Space(IndentLevel1);
                    band.max_distance_m = EditorGUILayout.FloatField(band.max_distance_m, GUILayout.Width(LodBandDistanceWidth));
                    ProbeLastRect("distance", i);
                    GUILayout.Space(TableGapWithinGroup);
                    band.max_visible_count = EditorGUILayout.IntField(band.max_visible_count, GUILayout.Width(LodBandCountWidth));
                    ProbeLastRect("count", i);
                    GUILayout.Space(TableGapBetweenGroups);
                    if (GUILayout.Button(new GUIContent("...", "Band note"), GUILayout.Width(LodBandDetailsWidth)))
                    {
                        var target = band;
                        EditorPopup.ShowAt(CreateDetailsPopup("Distance Band Note", () => target.details ?? string.Empty, v => target.details = v), GUILayoutUtility.GetLastRect());
                    }
                    GUILayout.Space(TableGapBeforeDelete);
                    if (DeleteButton.DrawLayout("Delete this band")) deleteIndex = i;
                    ProbeLastRect("delete", i);
                    GUILayout.FlexibleSpace();
                    GUILayout.Space(AddButtonRowRightMargin);
                }
            }
            if (deleteIndex >= 0) bands.RemoveAt(deleteIndex);
        }

        private static void ProbeLastRect(string column, int row)
        {
            if (LodBandCellRectProbe != null && Event.current.type == EventType.Repaint)
                LodBandCellRectProbe(column, row, GUILayoutUtility.GetLastRect());
        }

        // Suggest Values: the POI-count starting point for the bands and the two crowding counts only.
        // Every other LOD field (and Zoom) keeps its value. Runs inside the tab's mutation scope, so it
        // is one undoable edit.
        internal void ApplySuggestedLodValues()
        {
            var suggested = LodAutoSuggest.Suggest(_config.pois?.Count ?? 0);
            var lod = _config.lod_settings;
            lod.bands = suggested.bands;
            lod.cluster_min_count = suggested.cluster_min_count;
            lod.shrink_start_neighbor_count = suggested.shrink_start_neighbor_count;
        }

        private void DrawLodCrowdingSubSection(LodSettings lod)
        {
            EditorGUILayout.Space(4f);
            _showLodCrowding = EditorGUILayout.Foldout(_showLodCrowding, "Crowding", true, EditorStyles.foldoutHeader);
            if (!_showLodCrowding) return;

            string mode = lod.density_response_mode;
            lod.density_response_mode = DrawPopupField("Response", mode, DensityModeOptions, DensityModeLabels, LodDensityResponseHelp, IndentLevel1);
            mode = lod.density_response_mode;
            if (mode == "none") return;

            lod.density_radius_px = Mathf.Max(1f, DrawScalarField("Crowding Radius (px)", lod.density_radius_px, LodDensityRadiusHelp, IndentLevel1));
            if (LodEditorRules.UsesShrink(mode))
                lod.shrink_start_neighbor_count = Mathf.Max(0, DrawIntField("Shrink Starts At", lod.shrink_start_neighbor_count, LodShrinkStartHelp, IndentLevel1));
            if (LodEditorRules.UsesCrowdedCount(mode))
                lod.cluster_min_count = Mathf.Max(1, DrawIntField("Crowded At", lod.cluster_min_count, LodClusterMinHelp, IndentLevel1));
            if (LodEditorRules.UsesShrink(mode))
                lod.shrink_min_factor = DrawSliderField("Shrink Floor", lod.shrink_min_factor, LODController.MinShrinkFloor, 1f, LodShrinkFloorHelp, IndentLevel1);
            if (LodEditorRules.UsesShrink(mode) && !LODController.IsDensityConfigValid(lod))
                EditorGUILayout.HelpBox("Shrink Starts At must be smaller than Crowded At, or markers never shrink.", MessageType.Warning);

            if (LodEditorRules.UsesSafetyNet(mode))
            {
                lod.density_safety_escalation_enabled = DrawToggleField("Safety Net", lod.density_safety_escalation_enabled, LodSafetyNetHelp, IndentLevel1);
                if (lod.density_safety_escalation_enabled)
                    lod.density_safety_escalation_multiplier = Mathf.Max(1f, DrawScalarField("Safety Multiplier", lod.density_safety_escalation_multiplier,
                        LodSafetyNetHelp, IndentLevel1 + ConditionalAdvance));
            }
        }

        private void DrawLodClustersSubSection(LodSettings lod)
        {
            EditorGUILayout.Space(4f);
            _showLodClusters = EditorGUILayout.Foldout(_showLodClusters, "Clusters", true, EditorStyles.foldoutHeader);
            if (!_showLodClusters) return;

            lod.cluster_icon_mode = DrawPopupField("Cluster Icon", lod.cluster_icon_mode, ClusterIconOptions, ClusterIconLabels, LodClusterIconHelp, IndentLevel1);
            lod.cluster_size_ratio = DrawSliderField("Cluster Size", lod.cluster_size_ratio,
                MarkerClusterView.MinSizeRatio, MarkerClusterView.MaxSizeRatio, LodClusterSizeHelp, IndentLevel1);
            lod.cluster_band_source = DrawPopupField("Band Source", lod.cluster_band_source, ClusterBandSourceOptions, ClusterBandSourceLabels, LodClusterBandSourceHelp, IndentLevel1);
            lod.cluster_band_hysteresis_enabled = DrawToggleField("Cluster Band Hysteresis", lod.cluster_band_hysteresis_enabled, LodClusterBandHysteresisHelp, IndentLevel1);
            lod.cluster_dissolve_grace_cycles = Mathf.Max(0, DrawIntField("Dissolve Grace (cycles)", lod.cluster_dissolve_grace_cycles, LodClusterDissolveGraceHelp, IndentLevel1));
        }

        private void DrawLodTransitionsSubSection(LodSettings lod)
        {
            EditorGUILayout.Space(4f);
            _showLodTransitions = EditorGUILayout.Foldout(_showLodTransitions, "Transitions & Performance", true, EditorStyles.foldoutHeader);
            if (!_showLodTransitions) return;

            lod.transition_fade_duration_s = Mathf.Max(0f, DrawScalarField("Fade (s)", lod.transition_fade_duration_s, LodTransitionsHelp, IndentLevel1));
            lod.evaluation_interval_s = Mathf.Max(0.02f, DrawScalarField("Evaluation Interval (s)", lod.evaluation_interval_s, LodEvalIntervalHelp, IndentLevel1));
            lod.frustum_culling_enabled = DrawToggleField("Frustum Culling", lod.frustum_culling_enabled, LodFrustumHelp, IndentLevel1);
            if (lod.frustum_culling_enabled)
                lod.fov_culling_margin_deg = Mathf.Max(0f, DrawScalarField("Culling Margin (deg)", lod.fov_culling_margin_deg, LodFovMarginHelp, IndentLevel1 + ConditionalAdvance));
        }

        // "Add LOD demo field" and, while it is on, its settings (built by DemoFieldSpawner from WallSession)
        private void DrawLodDemoFieldControls()
        {
            if (_config.demo_field == null) _config.demo_field = new DemoFieldSettings();
            var field = _config.demo_field;

            bool wasOn = field.enabled;
            field.enabled = DrawToggleField("Add LOD demo field", field.enabled, DemoFieldHelp, IndentLevel1);
            bool justTurnedOn = field.enabled && !wasOn;
            MakeThisTheOnlyActiveDemoView(DemoView.LodDemoField, justTurnedOn);
            // first time on: a few markers per level, so the field is never empty
            if (justTurnedOn && (field.level_counts == null || field.level_counts.Count == 0))
                SeedDemoFieldCounts(field, 4);
            if (!field.enabled) return;

            float inner = IndentLevel1 + ConditionalAdvance;
            field.show_labels = DrawToggleField("Show labels", field.show_labels, DemoFieldLabelsHelp, inner);

            DrawEditorRow(out float headingWidth, out _, inner);
            GUILayout.Label("Markers per Hierarchy Level", EditorStyles.miniBoldLabel,
                GUILayout.Width(Mathf.Max(40f, headingWidth - 36f)), GUILayout.ExpandWidth(false));
            HelpInfoButton.Draw("Markers per Hierarchy Level", DemoFieldCountsHelp);
            EditorRowEnd();
            if (_config.hierarchy_levels == null || _config.hierarchy_levels.Count == 0)
                EditorGUILayout.HelpBox("No Hierarchy Levels yet: add them in the Hierarchy Levels section.", MessageType.Info);
            else
                foreach (var level in _config.hierarchy_levels)
                {
                    if (level == null) continue;
                    string label = string.IsNullOrWhiteSpace(level.level_name) ? level.key : level.level_name;
                    int count = DemoFieldLayout.CountFor(field, level.key);
                    int edited = DrawIntSliderField(label, count, 0, DemoFieldSettings.MaxCountPerLevel, "", inner + ConditionalAdvance);
                    if (edited != count) SetDemoFieldCount(field, level.key, edited);
                }

            field.dense_clump_count = DrawIntSliderField("Dense Clump", field.dense_clump_count, 0, DemoFieldSettings.MaxClumpCount, DemoFieldClumpHelp, inner);
            field.dense_clump_radius_m = DrawSliderField("Clump Radius (m)", field.dense_clump_radius_m, 0.01f, 1f, DemoFieldClumpHelp, inner);
            field.distance_m = DrawSliderField("Field Distance (m)", field.distance_m, 0f, 10f, DemoFieldBoxHelp, inner);
            field.depth_m = DrawSliderField("Field Depth (m)", field.depth_m, 0f, 30f, DemoFieldBoxHelp, inner);
            field.width_m = DrawSliderField("Field Width (m)", field.width_m, 0f, 20f, DemoFieldBoxHelp, inner);
            field.height_m = DrawSliderField("Field Height (m)", field.height_m, 0f, 10f, DemoFieldBoxHelp, inner);

            DrawEditorRow(out float rowWidth, out _, inner);
            if (GUILayout.Button("Reshuffle", GUILayout.Width(Mathf.Max(40f, rowWidth - 36f)), GUILayout.ExpandWidth(false)))
                field.seed++;
            HelpInfoButton.Draw("Reshuffle", DemoFieldReshuffleHelp);
            EditorRowEnd();
        }

        // Give every hierarchy level the same starting count (the switch's first tick)
        private void SeedDemoFieldCounts(DemoFieldSettings field, int perLevel)
        {
            field.level_counts = new List<DemoFieldLevelCount>();
            if (_config.hierarchy_levels == null) return;
            foreach (var level in _config.hierarchy_levels)
                if (level != null)
                    field.level_counts.Add(new DemoFieldLevelCount { level_key = level.key, count = perLevel });
        }

        // Write one level's count, adding its row if the level has none yet
        private static void SetDemoFieldCount(DemoFieldSettings field, string levelKey, int count)
        {
            if (field.level_counts == null) field.level_counts = new List<DemoFieldLevelCount>();
            var row = field.level_counts.Find(r => r != null && r.level_key == levelKey);
            if (row == null) field.level_counts.Add(new DemoFieldLevelCount { level_key = levelKey, count = count });
            else row.count = count;
        }

        // ---- Zoom section ----

        private bool _showZoomRange = true;
        private bool _showZoomGestures = true;
        private readonly TestGuideState _zoomTest = new TestGuideState();

        private void DrawGlobalZoomSection()
        {
            if (_config == null || _config.zoom_settings == null)
            {
                EditorGUILayout.HelpBox("No zoom settings to configure.", MessageType.Info);
                return;
            }

            var zoom = _config.zoom_settings;
            zoom.enabled = DrawToggleField("Enable Zoom", zoom.enabled, ZoomEnabledHelp);
            if (zoom.enabled)
            {
                EditorGUILayout.Space(4f);
                _showZoomRange = EditorGUILayout.Foldout(_showZoomRange, "Zoom Range", true, EditorStyles.foldoutHeader);
                if (_showZoomRange)
                {
                    zoom.min_factor = DrawSliderField("Min Zoom", zoom.min_factor, 1f, 10f, ZoomMinHelp, IndentLevel1);
                    zoom.max_factor = DrawSliderField("Max Zoom", Mathf.Max(zoom.max_factor, zoom.min_factor), zoom.min_factor, 10f, ZoomMaxHelp, IndentLevel1);
                }

                EditorGUILayout.Space(4f);
                _showZoomGestures = EditorGUILayout.Foldout(_showZoomGestures, "Gestures & Buttons", true, EditorStyles.foldoutHeader);
                if (_showZoomGestures)
                {
                    zoom.tap_step = DrawSliderField("Double-Tap Step", zoom.tap_step, 1.1f, 4f, ZoomTapStepHelp, IndentLevel1);
                    zoom.tap_levels = DrawIntSliderField("Double-Tap Levels", zoom.tap_levels, 1, 6, ZoomTapLevelsHelp, IndentLevel1);
                    zoom.transition_duration_s = DrawSliderField("Transition (s)", zoom.transition_duration_s, 0f, 2f, ZoomTransitionHelp, IndentLevel1);
                    zoom.double_tap_window_s = DrawSliderField("Double-Tap Window (s)", zoom.double_tap_window_s, 0.1f, 1f, ZoomDoubleTapWindowHelp, IndentLevel1);
                    zoom.double_tap_move_tolerance_px = DrawSliderField("Tap Tolerance (px)", zoom.double_tap_move_tolerance_px, 5f, 200f, ZoomDoubleTapMoveToleranceHelp, IndentLevel1);
                    zoom.show_ui_buttons = DrawToggleField("Show On-Screen Buttons", zoom.show_ui_buttons, ZoomUiButtonsHelp, IndentLevel1);
                }
            }

            DrawDomainTestSubSection(_zoomTest, ZoomSceneTestGuide, ZoomPlaymodeTestGuide, ZoomDeviceTestGuide);
        }
    }
}
