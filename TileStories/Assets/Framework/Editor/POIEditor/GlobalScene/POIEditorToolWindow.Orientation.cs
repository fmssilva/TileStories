// POIEditorToolWindow.Orientation.cs
//
// Partial: the editor Orientation Settings foldout (Block 6 of
// _2.1_Marker_Orientation.md). Editor-only -- none of this ships. Shares the
// partial class with POIEditorToolWindow.cs and GlobalScene.cs; the foldout
// call itself is wired in GlobalScene.cs, between Marker and Badge.
// Reuses the shared field drawers (DrawScalarField / DrawIntField /
// DrawToggleField / DrawPopupField) defined in LodZoom.cs -- no new
// row-layout code (_5.1_Editor_Tab.md Lessons 2-7, 13-14).

using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    public partial class POIEditorToolWindow
    {
        // Global Scene -> Orientation Settings foldout. Edits
        // _config.orientation_settings (the OrientationSettings schema defined in
        // WallConfigData.cs). All fields already exist on the runtime schema; this
        // is purely an editor surface, no runtime changes.
        private void DrawGlobalOrientationSection()
        {
            if (_config == null || _config.orientation_settings == null)
            {
                EditorGUILayout.HelpBox("No orientation settings to configure.", MessageType.Info);
                return;
            }

            var o = _config.orientation_settings;

            EditorGUILayout.LabelField("Marker Root", EditorStyles.boldLabel);
            o.marker_orientation_mode = DrawPopupField("Marker Orientation", o.marker_orientation_mode,
                MarkerOrientationModeOptions, MarkerOrientationModeLabels, MarkerOrientationModeHelp);

            // Progressive disclosure (30-ui-content.md): facing_basis only for
            // screen_aligned/world_up; up_reference only for world_up/yaw_only; the
            // custom vector only for up_reference == custom.
            bool showsFacingBasis = o.marker_orientation_mode == "screen_aligned" || o.marker_orientation_mode == "world_up";
            bool showsUpReference = o.marker_orientation_mode == "world_up" || o.marker_orientation_mode == "yaw_only";

            if (showsFacingBasis)
                o.facing_basis = DrawPopupField("  Facing Basis", o.facing_basis, FacingBasisOptions, FacingBasisLabels, FacingBasisHelp);

            if (showsUpReference)
            {
                o.up_reference = DrawPopupField("  Up Reference", o.up_reference, UpReferenceOptions, UpReferenceLabels, UpReferenceHelp);
                if (o.up_reference == "custom")
                {
                    o.custom_up_x = DrawScalarField("    Custom Up X", o.custom_up_x, CustomUpHelp);
                    o.custom_up_y = DrawScalarField("    Custom Up Y", o.custom_up_y, CustomUpHelp);
                    o.custom_up_z = DrawScalarField("    Custom Up Z", o.custom_up_z, CustomUpHelp);
                }
            }

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Roll / Pitch Conditioning", EditorStyles.boldLabel);
            o.roll_snap_mode = DrawPopupField("Roll Snap", o.roll_snap_mode, RollSnapModeOptions, RollSnapModeLabels, RollSnapModeHelp);
            if (o.roll_snap_mode != "none")
                o.roll_snap_hysteresis_deg = DrawScalarField("  Snap Hysteresis (deg)", o.roll_snap_hysteresis_deg, RollSnapHysteresisHelp);

            o.clamp_pitch_enabled = DrawToggleField("Clamp Pitch", o.clamp_pitch_enabled, ClampPitchHelp);
            if (o.clamp_pitch_enabled)
                o.max_pitch_deg = DrawScalarField("  Max Pitch (deg)", o.max_pitch_deg, MaxPitchHelp);

            o.rotation_smoothing_time_s = DrawScalarField("Rotation Smoothing (s)", o.rotation_smoothing_time_s, RotationSmoothingHelp);

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Label / Badge", EditorStyles.boldLabel);
            o.label_orientation_mode = DrawPopupField("Label Orientation", o.label_orientation_mode, ChildOrientationModeOptions, ChildOrientationModeLabels, LabelOrientationHelp);
            o.badge_orientation_mode = DrawPopupField("Badge Orientation", o.badge_orientation_mode, ChildOrientationModeOptions, ChildOrientationModeLabels, BadgeOrientationHelp);

            // badge_corner_mode only when the wall actually uses a badge.
            if (_config.marker_use_badge)
                o.badge_corner_mode = DrawPopupField("  Badge Corner", o.badge_corner_mode, BadgeCornerModeOptions, BadgeCornerModeLabels, BadgeCornerModeHelp);

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Clusters", EditorStyles.boldLabel);
            o.cluster_orientation_mode = DrawPopupField("Cluster Orientation", o.cluster_orientation_mode, ClusterOrientationModeOptions, ClusterOrientationModeLabels, ClusterOrientationModeHelp);

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Update Cost", EditorStyles.boldLabel);
            o.update_mode = DrawPopupField("Update Mode", o.update_mode, OrientationUpdateModeOptions, OrientationUpdateModeLabels, OrientationUpdateModeHelp);
            if (o.update_mode == "interval")
                o.update_interval_s = DrawScalarField("  Update Interval (s)", o.update_interval_s, OrientationUpdateIntervalHelp);
            if (o.update_mode == "on_camera_delta")
                o.camera_delta_deg = DrawScalarField("  Camera Delta (deg)", o.camera_delta_deg, OrientationCameraDeltaHelp);

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Editor", EditorStyles.boldLabel);
            o.edit_mode_preview_enabled = DrawToggleField("Edit-Mode Preview", o.edit_mode_preview_enabled, EditModePreviewHelp);
        }
    }
}
