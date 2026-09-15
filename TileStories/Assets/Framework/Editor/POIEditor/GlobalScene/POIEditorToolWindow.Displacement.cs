// POIEditorToolWindow.Displacement.cs
//
// Partial: the editor Displacement Settings foldout (Block 8 of
// _2.5_Marker_Displacement.md, spec section 11). Editor-only -- none of
// this ships. Shares the partial class with POIEditorToolWindow.cs
// and GlobalScene.cs; the foldout call itself is wired in GlobalScene.
// Reuses the shared field drawers (DrawScalarField / DrawIntField /
// DrawToggleField / DrawPopupField) defined in LodZoom.cs.

using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    public partial class POIEditorToolWindow
    {
        // Global Scene -> Displacement Settings foldout.
        // Edits _config.displacement_settings (the DisplacementSettings schema
        // defined in WallConfigData.cs). All fields already exist on the
        // runtime schema; this is purely an editor surface, no runtime changes.
        private void DrawGlobalDisplacementSection()
        {
            if (_config == null || _config.displacement_settings == null)
            {
                EditorGUILayout.HelpBox("No displacement settings to configure.", MessageType.Info);
                return;
            }

            var disp = _config.displacement_settings;

            disp.enabled = DrawToggleField("Enable Displacement", disp.enabled, DisplacementEnabledHelp);

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Overlap", EditorStyles.boldLabel);
            disp.overlap_threshold_px = DrawScalarField("Overlap Threshold (px)", disp.overlap_threshold_px, DisplacementOverlapThresholdHelp);

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Placement", EditorStyles.boldLabel);
            disp.displace_target = DrawPopupField("Displace Target", disp.displace_target, DisplaceTargetOptions, DisplaceTargetLabels, DisplacementTargetHelp);
            disp.displacement_algorithm = DrawPopupField("Displacement Algorithm", disp.displacement_algorithm, DisplacementAlgorithmOptions, DisplacementAlgorithmLabels, DisplacementAlgorithmHelp);

            // Conditional: relaxation steps only apply to the force_directed algorithm.
            if (disp.displacement_algorithm == "force_directed")
                disp.force_directed_iterations = DrawIntField("  Relaxation Steps", disp.force_directed_iterations, ForceDirectedIterationsHelp);

            disp.max_displacement_px = DrawScalarField("Max Displacement (px)", disp.max_displacement_px, DisplacementMaxHelp);

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Leader Lines", EditorStyles.boldLabel);
            disp.leader_lines_enabled = DrawToggleField("Enable Leader Lines", disp.leader_lines_enabled, LeaderLinesEnabledHelp);

            if (disp.leader_lines_enabled)
            {
                disp.leader_line_style = DrawPopupField("Style", disp.leader_line_style, LeaderLineStyleOptions, LeaderLineStyleLabels, LeaderLineStyleHelp);
                disp.leader_line_min_distance_px = DrawScalarField("Min Distance (px)", disp.leader_line_min_distance_px, LeaderLineMinDistanceHelp);
                disp.leader_line_width = DrawScalarField("Line Width (world)", disp.leader_line_width, LeaderLineWidthHelp);
                disp.leader_line_opacity = DrawScalarField("Line Opacity (0-1)", disp.leader_line_opacity, LeaderLineOpacityHelp);
            }

            disp.displacement_tiebreak = DrawPopupField("Tiebreak", disp.displacement_tiebreak, DisplacementTiebreakOptions, DisplacementTiebreakLabels, DisplacementTiebreakHelp);

            _hasUnsavedChanges = true;
        }
    }
}
