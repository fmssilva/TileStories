// POIAuthoringToolWindow.LodZoom.cs
//
// Partial: the authoring-tool LOD and AR-zoom foldouts (Block 2 of
// _2.4_Marker_LOD.md, Implementation Status rows 5b / 12 / 13). Editor-only --
// none of this ships. Shares the partial class with POIAuthoringToolWindow.cs
// and GlobalScene.cs; the foldout calls themselves are wired in GlobalScene.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    public partial class POIAuthoringToolWindow
    {
        // ---- Shared, reusable field drawers for the LOD/Zoom foldouts ----
        // Each returns the edited value and draws an inline "(i)" help button
        // when helpText is non-empty. Genuine reuse -- every scalar/toggle/
        // popup in these two sections goes through one of these.

        private static float DrawScalarField(string label, float value, string helpText = "")
        {
            EditorGUILayout.BeginHorizontal();
            value = EditorGUILayout.FloatField(label, value);
            if (!string.IsNullOrEmpty(helpText))
                HelpInfoButton.Draw(label, helpText);
            EditorGUILayout.EndHorizontal();
            return value;
        }

        private static int DrawIntField(string label, int value, string helpText = "")
        {
            EditorGUILayout.BeginHorizontal();
            value = EditorGUILayout.IntField(label, value);
            if (!string.IsNullOrEmpty(helpText))
                HelpInfoButton.Draw(label, helpText);
            EditorGUILayout.EndHorizontal();
            return value;
        }

        private static bool DrawToggleField(string label, bool value, string helpText = "")
        {
            EditorGUILayout.BeginHorizontal();
            value = EditorGUILayout.Toggle(label, value);
            if (!string.IsNullOrEmpty(helpText))
                HelpInfoButton.Draw(label, helpText);
            EditorGUILayout.EndHorizontal();
            return value;
        }

        private static string DrawPopupField(string label, string current, string[] options, string[] labels, string helpText = "")
        {
            EditorGUILayout.BeginHorizontal();
            int idx = Array.IndexOf(options, current);
            if (idx < 0) idx = 0;
            idx = EditorGUILayout.Popup(label, idx, labels);
            if (!string.IsNullOrEmpty(helpText))
                HelpInfoButton.Draw(label, helpText);
            EditorGUILayout.EndHorizontal();
            return idx >= 0 ? options[idx] : current;
        }

        // ---- LOD section ----
        private void DrawGlobalLodSection()
        {
            if (_config == null || _config.lod_settings == null)
            {
                EditorGUILayout.HelpBox("No LOD settings to configure.", MessageType.Info);
                return;
            }

            var lod = _config.lod_settings;

            lod.enabled = DrawToggleField("Enable LOD pipeline", lod.enabled, LodEnabledHelp);

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Distance Bands", EditorStyles.boldLabel);

            if (lod.bands == null)
                lod.bands = new List<LodBandEntry>();

            for (int i = 0; i < lod.bands.Count; i++)
            {
                var band = lod.bands[i];
                EditorGUILayout.BeginHorizontal();
                band.max_distance_m = EditorGUILayout.FloatField(
                    new GUIContent("Max Distance (m)", LodBandsHelp),
                    band.max_distance_m, GUILayout.Width(120f));
                band.max_visible_count = EditorGUILayout.IntField(
                    new GUIContent("Max Visible", "-1 = show all markers in this band"),
                    band.max_visible_count, GUILayout.Width(80f));

                // Per-band authoring note (EntryDetailsPopup) -- binds to the
                // additive `details` string on LodBandEntry.
                if (GUILayout.Button(new GUIContent("...", "Per-band note"), GUILayout.Width(40f), GUILayout.Height(20f)))
                {
                    var targetBand = band;
                    PopupWindow.Show(GUILayoutUtility.GetLastRect(),
                        new EntryDetailsPopup("LOD Band Note",
                            () => targetBand.details ?? string.Empty,
                            v => { targetBand.details = v; _hasUnsavedChanges = true; }));
                }

                if (GUILayout.Button(TrashIcon, GUILayout.Width(24f), GUILayout.Height(20f)))
                {
                    lod.bands.RemoveAt(i);
                    _hasUnsavedChanges = true;
                    i--;
                    continue;
                }
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("+ Add band"))
            {
                // Default to a far sentinel; the developer edits the distance.
                lod.bands.Add(new LodBandEntry { max_distance_m = 9999f, max_visible_count = 5 });
                _hasUnsavedChanges = true;
            }

            EditorGUILayout.HelpBox(
                "Suggest Values computes a POI-count-driven starting set (3 bands + cluster_min + shrink_start) and writes ordinary field values -- hand-tune afterward.",
                MessageType.Info);
            if (GUILayout.Button("Suggest Values"))
            {
                _config.lod_settings = LodAutoSuggest.Suggest(_config.pois?.Count ?? 0);
                lod = _config.lod_settings; // rebind: Suggest replaces the object
                _hasUnsavedChanges = true;
            }

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Density Response", EditorStyles.boldLabel);
            lod.density_response_mode = DrawPopupField("Mode", lod.density_response_mode, DensityModeOptions, DensityModeLabels, LodDensityResponseHelp);
            lod.density_radius_px = DrawScalarField("Density Radius (px)", lod.density_radius_px, LodDensityRadiusHelp);
            lod.shrink_start_neighbor_count = DrawIntField("Shrink Start (neighbors)", lod.shrink_start_neighbor_count, LodShrinkStartHelp);
            lod.cluster_min_count = DrawIntField("Cluster Min (neighbors)", lod.cluster_min_count, LodClusterMinHelp);

                        if (lod.density_response_mode == "cluster" || lod.density_response_mode == "hybrid")
            {
                lod.cluster_icon_mode = DrawPopupField("Cluster Icon", lod.cluster_icon_mode, ClusterIconOptions, ClusterIconLabels, "");
                lod.cluster_band_source = DrawPopupField("Band Source", lod.cluster_band_source, ClusterBandSourceOptions, ClusterBandSourceLabels, LodClusterBandSourceHelp);
                lod.cluster_band_hysteresis_enabled = DrawToggleField("  Band Hysteresis", lod.cluster_band_hysteresis_enabled, LodClusterBandHysteresisHelp);
                lod.cluster_dissolve_grace_cycles = DrawIntField("  Dissolve Grace (cycles)", lod.cluster_dissolve_grace_cycles, LodClusterDissolveGraceHelp);
            }

            lod.density_safety_escalation_enabled = DrawToggleField("Safety Escalation", lod.density_safety_escalation_enabled, LodSafetyEscalationHelp);
            if (lod.density_safety_escalation_enabled)
                lod.density_safety_escalation_multiplier = DrawScalarField("  Multiplier", lod.density_safety_escalation_multiplier, LodSafetyEscalationHelp);

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Transitions", EditorStyles.boldLabel);
            lod.hysteresis_margin_m = DrawScalarField("Hysteresis Margin (m)", lod.hysteresis_margin_m, LodHysteresisHelp);
            lod.transition_fade_duration_s = DrawScalarField("Transition Fade (s)", lod.transition_fade_duration_s, LodTransitionsHelp);
            lod.evaluation_interval_s = DrawScalarField("Evaluation Interval (s)", lod.evaluation_interval_s, LodEvalIntervalHelp);

            EditorGUILayout.LabelField("Performance", EditorStyles.boldLabel);
            lod.frustum_culling_enabled = DrawToggleField("Frustum Culling", lod.frustum_culling_enabled, LodFrustumHelp);
            if (lod.frustum_culling_enabled)
                lod.fov_culling_margin_deg = DrawScalarField("  FOV Margin (deg)", lod.fov_culling_margin_deg, LodFovMarginHelp);

            _hasUnsavedChanges = true;
        }
        // ---- Zoom section ----
        private void DrawGlobalZoomSection()
        {
            if (_config == null || _config.lod_settings == null)
            {
                EditorGUILayout.HelpBox("No zoom settings to configure.", MessageType.Info);
                return;
            }

            var lod = _config.lod_settings;

            lod.zoom_enabled = DrawToggleField("Enable Zoom", lod.zoom_enabled, ZoomEnabledHelp);
            lod.zoom_min = DrawScalarField("Zoom Min", lod.zoom_min, ZoomMinHelp);
            lod.zoom_max = DrawScalarField("Zoom Max", lod.zoom_max, ZoomMaxHelp);
            lod.zoom_tap_step = DrawScalarField("Tap Step", lod.zoom_tap_step, ZoomTapStepHelp);
            lod.zoom_tap_levels = DrawIntField("Tap Levels", lod.zoom_tap_levels, ZoomTapLevelsHelp);
            lod.zoom_transition_speed_s = DrawScalarField("Transition Speed (s)", lod.zoom_transition_speed_s, ZoomTransitionHelp);
            lod.zoom_show_ui_buttons = DrawToggleField("Show UI Buttons", lod.zoom_show_ui_buttons, ZoomUiButtonsHelp);

            _hasUnsavedChanges = true;
        }
    }
}
