// POIEditorToolWindow.LodZoom.cs
//
// Partial: the editor LOD and AR-zoom foldouts (Block 2 of
// _2.4_Marker_LOD.md, Implementation Status rows 5b / 12 / 13). Editor-only --
// none of this ships. Shares the partial class with POIEditorToolWindow.cs
// and GlobalScene.cs; the foldout calls themselves are wired in GlobalScene.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    public partial class POIEditorToolWindow
    {
        // ---- Shared, reusable field drawers used by LOD / Zoom / Displacement /
        // SearchFilter foldouts ----
        // Each is a shared editor row: transparent indent spacer + width capped to
        // max(MinRowWidth, min(visible panel, MaxRowWidth)). The labelled field takes
        // rowWidth (minus a help-button allowance when helpText is set); ExpandWidth(false)
        // keeps the control from stretching the panel. The spacer is measured at call time,
        // so rows inside a nested IndentLevelScope keep their own deeper indent.
        //
        // EditorGUI.indentLevel is zeroed around the labelled control itself (2026-09-18,
        // developer screenshot feedback): DrawEditorRow's spacer already pays the row's
        // indent once as real layout width, but a labelled EditorGUILayout control (Popup/
        // Toggle/FloatField/IntField) independently re-applies the ambient indentLevel a
        // SECOND time via its own internal PrefixLabel -- not as a rect.x shift (invisible
        // to GUILayoutUtility.GetLastRect, this file's Lesson 6 trap) but as extra padding
        // before the label glyph itself, which is exactly why these rows sat visibly
        // deeper than a plain sibling section label (e.g. "Category Symbols") despite
        // rect-based measurements not showing it. Zeroing indentLevel here does not affect
        // extraIndentPixels' own nudge (a raw pixel add to the spacer's width, independent
        // of indentLevel) or the spacer's own reserved space.
        internal static float DrawScalarField(string label, float value, string helpText = "", float extraIndentPixels = 0f)
        {
            DrawEditorRow(out float rowWidth, out _, extraIndentPixels);
            float fieldWidth = string.IsNullOrEmpty(helpText) ? rowWidth : Mathf.Max(40f, rowWidth - 36f);
            int savedIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            using (new FieldLabelWidthCompensationScope(extraIndentPixels))
                value = EditorGUILayout.FloatField(label, value, GUILayout.Width(fieldWidth), GUILayout.ExpandWidth(false));
            EditorGUI.indentLevel = savedIndent;
            if (!string.IsNullOrEmpty(helpText))
                HelpInfoButton.Draw(label, helpText);
            EditorRowEnd();
            return value;
        }

        internal static int DrawIntField(string label, int value, string helpText = "", float extraIndentPixels = 0f)
        {
            DrawEditorRow(out float rowWidth, out _, extraIndentPixels);
            float fieldWidth = string.IsNullOrEmpty(helpText) ? rowWidth : Mathf.Max(40f, rowWidth - 36f);
            int savedIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            using (new FieldLabelWidthCompensationScope(extraIndentPixels))
                value = EditorGUILayout.IntField(label, value, GUILayout.Width(fieldWidth), GUILayout.ExpandWidth(false));
            EditorGUI.indentLevel = savedIndent;
            if (!string.IsNullOrEmpty(helpText))
                HelpInfoButton.Draw(label, helpText);
            EditorRowEnd();
            return value;
        }

        internal static bool DrawToggleField(string label, bool value, string helpText = "", float extraIndentPixels = 0f)
        {
            DrawEditorRow(out float rowWidth, out _, extraIndentPixels);
            float fieldWidth = string.IsNullOrEmpty(helpText) ? rowWidth : Mathf.Max(40f, rowWidth - 36f);
            int savedIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            using (new FieldLabelWidthCompensationScope(extraIndentPixels))
                value = EditorGUILayout.Toggle(label, value, GUILayout.Width(fieldWidth), GUILayout.ExpandWidth(false));
            EditorGUI.indentLevel = savedIndent;
            if (!string.IsNullOrEmpty(helpText))
                HelpInfoButton.Draw(label, helpText);
            EditorRowEnd();
            return value;
        }

        internal static string DrawPopupField(string label, string current, string[] options, string[] labels, string helpText = "", float extraIndentPixels = 0f)
        {
            DrawEditorRow(out float rowWidth, out _, extraIndentPixels);
            float fieldWidth = string.IsNullOrEmpty(helpText) ? rowWidth : Mathf.Max(40f, rowWidth - 36f);
            int idx = Array.IndexOf(options, current);
            if (idx < 0) idx = 0;
            int savedIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            using (new FieldLabelWidthCompensationScope(extraIndentPixels))
                idx = EditorGUILayout.Popup(label, idx, labels, GUILayout.Width(fieldWidth), GUILayout.ExpandWidth(false));
            EditorGUI.indentLevel = savedIndent;
            if (!string.IsNullOrEmpty(helpText))
                HelpInfoButton.Draw(label, helpText);
            EditorRowEnd();
            return idx >= 0 ? options[idx] : current;
        }

        // Same shared-row shape as DrawScalarField, for a min/max-clamped slider
        // instead of a free-typed float (Effects section defaults: amplitude, alpha
        // ramps, scale factors, all 0-1 or similarly bounded).
        internal static float DrawSliderField(string label, float value, float min, float max, string helpText = "", float extraIndentPixels = 0f)
        {
            DrawEditorRow(out float rowWidth, out _, extraIndentPixels);
            float fieldWidth = string.IsNullOrEmpty(helpText) ? rowWidth : Mathf.Max(40f, rowWidth - 36f);
            int savedIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            using (new FieldLabelWidthCompensationScope(extraIndentPixels))
                value = EditorGUILayout.Slider(label, value, min, max, GUILayout.Width(fieldWidth), GUILayout.ExpandWidth(false));
            EditorGUI.indentLevel = savedIndent;
            if (!string.IsNullOrEmpty(helpText))
                HelpInfoButton.Draw(label, helpText);
            EditorRowEnd();
            return value;
        }

        // Same shared-row shape again, for a LABELLED standalone color field (picker +
        // hex) outside a taxonomy table -- e.g. the Effects section's Sun/Accent tint
        // colors, which previously had no row wrapper, no indent, and no label at all
        // (2026-09-18 audit finding: they rendered as two bare, unlabeled, left-edge
        // controls stacked vertically instead of a proper row). PrefixLabel draws the
        // label column exactly like every other field row's built-in label; the actual
        // picker + hex pair still goes through DrawColorSwatchAndHex unchanged, so the
        // taxonomy tables' own (unlabelled) call sites are unaffected.
        internal static void DrawColorField(string label, ref string colorHex, string helpText = "", float extraIndentPixels = 0f)
        {
            DrawEditorRow(out float rowWidth, out _, extraIndentPixels);
            int savedIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            using (new FieldLabelWidthCompensationScope(extraIndentPixels))
                EditorGUILayout.PrefixLabel(label);
            string hex = colorHex;
            DrawColorSwatchAndHex(ref hex, out _, out _);
            colorHex = hex;
            EditorGUI.indentLevel = savedIndent;
            if (!string.IsNullOrEmpty(helpText))
                HelpInfoButton.Draw(label, helpText);
            EditorRowEnd();
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

                // Per-band editor note (EntryDetailsPopup) -- binds to the
                // additive `details` string on LodBandEntry.
                if (GUILayout.Button(new GUIContent("...", "Per-band note"), GUILayout.Width(40f), GUILayout.Height(20f)))
                {
                    var targetBand = band;
                    PopupWindow.Show(GUILayoutUtility.GetLastRect(),
                        new EntryDetailsPopup("LOD Band Note",
                            () => targetBand.details ?? string.Empty,
                            v => { targetBand.details = v; _hasUnsavedChanges = true; }));
                }

                bool deleteBandClicked = DeleteButton.DrawLayout("Delete LOD band");

                // Right-edge scrollbar clearance (_5.1_Editor_Tab.md "Row Indentation &
                // Spacing"): this table builds its own row layout instead of going
                // through DrawEditorRow, so this needs restating by hand.
                GUILayout.Space(AddButtonRowRightMargin);

                if (deleteBandClicked)
                {
                    lod.bands.RemoveAt(i);
                    _hasUnsavedChanges = true;
                    i--;
                    continue;
                }
                EditorGUILayout.EndHorizontal();
            }

            // Rendered as a shared editor row: transparent indent spacer + width
            // capped to max(MinRowWidth, min(visible panel, MaxRowWidth)).
            DrawEditorRow(out float rowWidth, out _);
            if (GUILayout.Button("+ Add band", GUILayout.Width(rowWidth), GUILayout.ExpandWidth(false)))
            {
                // Default to a far sentinel; the developer edits the distance.
                lod.bands.Add(new LodBandEntry { max_distance_m = 9999f, max_visible_count = 5 });
                _hasUnsavedChanges = true;
            }
            EditorRowEnd();

            EditorGUILayout.HelpBox(
                "Suggest Values computes a POI-count-driven starting set (3 bands + cluster_min + shrink_start) and writes ordinary field values -- hand-tune afterward.",
                MessageType.Info);
            DrawEditorRow(out float suggestWidth, out _);
            if (GUILayout.Button("Suggest Values", GUILayout.Width(suggestWidth), GUILayout.ExpandWidth(false)))
            {
                _config.lod_settings = LodAutoSuggest.Suggest(_config.pois?.Count ?? 0);
                lod = _config.lod_settings; // rebind: Suggest replaces the object
                _hasUnsavedChanges = true;
            }
            EditorRowEnd();

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
                lod.cluster_band_hysteresis_enabled = DrawToggleField("Band Hysteresis", lod.cluster_band_hysteresis_enabled, LodClusterBandHysteresisHelp, SubFieldIndentPixels);
                lod.cluster_dissolve_grace_cycles = DrawIntField("Dissolve Grace (cycles)", lod.cluster_dissolve_grace_cycles, LodClusterDissolveGraceHelp, SubFieldIndentPixels);
            }

            lod.density_safety_escalation_enabled = DrawToggleField("Safety Escalation", lod.density_safety_escalation_enabled, LodSafetyEscalationHelp);
            if (lod.density_safety_escalation_enabled)
                lod.density_safety_escalation_multiplier = DrawScalarField("Multiplier", lod.density_safety_escalation_multiplier, LodSafetyEscalationHelp, SubFieldIndentPixels);

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Transitions", EditorStyles.boldLabel);
            lod.hysteresis_margin_m = DrawScalarField("Hysteresis Margin (m)", lod.hysteresis_margin_m, LodHysteresisHelp);
            lod.transition_fade_duration_s = DrawScalarField("Transition Fade (s)", lod.transition_fade_duration_s, LodTransitionsHelp);
            lod.evaluation_interval_s = DrawScalarField("Evaluation Interval (s)", lod.evaluation_interval_s, LodEvalIntervalHelp);

            EditorGUILayout.LabelField("Performance", EditorStyles.boldLabel);
            lod.frustum_culling_enabled = DrawToggleField("Frustum Culling", lod.frustum_culling_enabled, LodFrustumHelp);
            if (lod.frustum_culling_enabled)
                lod.fov_culling_margin_deg = DrawScalarField("FOV Margin (deg)", lod.fov_culling_margin_deg, LodFovMarginHelp, SubFieldIndentPixels);
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
            lod.zoom_double_tap_window_s = DrawScalarField("Double-Tap Window (s)", lod.zoom_double_tap_window_s, ZoomDoubleTapWindowHelp);
            lod.zoom_double_tap_move_tolerance_px = DrawScalarField("Double-Tap Move Tolerance (px)", lod.zoom_double_tap_move_tolerance_px, ZoomDoubleTapMoveToleranceHelp);
        }
    }
}
