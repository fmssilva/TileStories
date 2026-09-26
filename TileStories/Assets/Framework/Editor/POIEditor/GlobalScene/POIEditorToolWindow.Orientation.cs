// POIEditorToolWindow.Orientation.cs
//
// Partial: the editor Orientation Settings foldout (_2.1_Marker_Orientation.md v4).
// Editor-only -- none of this ships. Shares the partial class with
// POIEditorToolWindow.cs and GlobalScene.cs; the foldout call itself is wired in
// GlobalScene.cs, between Marker and Badge. Reuses the shared field drawers
// (DrawScalarField / DrawIntField / DrawToggleField / DrawPopupField) defined in
// LodZoom.cs -- no new row-layout code (_5.1_Editor_Tab.md Lessons 2-7, 13-14).
//
// Four sub-domains, matching the framework's own mental model:
//   Vertical Alignment - which way is "up" for the marker and each of its elements.
//   Facing Options     - what the marker points at (wall / yaw-to-camera / full camera-facing).
//   Update Cost        - how often orientation is re-resolved.
//   Test               - how to actually verify a choice, in Scene mode and Play mode.

using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    public partial class POIEditorToolWindow
    {
        private bool _showOrientationVerticalAlignment = true;
        private bool _showOrientationFacingOptions = true;
        private bool _showOrientationUpdateCost = true;
        private readonly TestGuideState _orientationTest = new TestGuideState();

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

            DrawOrientationVerticalAlignmentSubSection(o);
            EditorGUILayout.Space(4f);
            DrawOrientationFacingOptionsSubSection(o);
            EditorGUILayout.Space(4f);
            DrawOrientationUpdateCostSubSection(o);
            // Test: the Scene-Mode Preview switch, then the three guides (Shared/DomainTest.cs)
            DrawDomainTestSubSection(_orientationTest, OrientationSceneTestGuide, OrientationPlaymodeTestGuide,
                OrientationDeviceTestGuide, () => o.edit_mode_preview_enabled =
                    DrawToggleField("Scene-Mode Preview", o.edit_mode_preview_enabled, EditModePreviewHelp, IndentLevel1));
        }

        private void DrawOrientationVerticalAlignmentSubSection(OrientationSettings o)
        {
            _showOrientationVerticalAlignment = EditorGUILayout.Foldout(_showOrientationVerticalAlignment, "Vertical Alignment", true, EditorStyles.foldoutHeader);
            if (!_showOrientationVerticalAlignment) return;

            // Rows inside a sub-foldout are its CHILDREN: IndentLevel1 puts their label
            // under the foldout title text (a raw pixel step through DrawEditorRow, not an
            // IndentLevelScope, so the row's own control never double-indents; the value column
            // stays aligned through FieldLabelWidthCompensationScope). _5.1_Editor_Tab.md
            // "Row Indentation & Spacing".
            o.vertical_alignment_mode = DrawPopupField("Marker", o.vertical_alignment_mode,
                VerticalAlignmentModeOptions, VerticalAlignmentModeLabels, VerticalAlignmentModeHelp, IndentLevel1);

            o.label_vertical_alignment_mode = DrawPopupField("Label", o.label_vertical_alignment_mode,
                ChildVerticalAlignmentModeOptions, ChildVerticalAlignmentModeLabels, ChildVerticalAlignmentHelp, IndentLevel1);

            if (_config.marker_use_badge)
            {
                o.badge_vertical_alignment_mode = DrawPopupField("Badge", o.badge_vertical_alignment_mode,
                    ChildVerticalAlignmentModeOptions, ChildVerticalAlignmentModeLabels, ChildVerticalAlignmentHelp, IndentLevel1);
            }

            o.cluster_vertical_alignment_mode = DrawPopupField("Clusters", o.cluster_vertical_alignment_mode,
                ChildVerticalAlignmentModeOptions, ChildVerticalAlignmentModeLabels, ChildVerticalAlignmentHelp, IndentLevel1);

            // up_reference only matters wherever something is actually set to World Up.
            bool anyUsesWorldUp = o.vertical_alignment_mode == "world_up"
                || o.label_vertical_alignment_mode == "world_up"
                || o.badge_vertical_alignment_mode == "world_up"
                || o.cluster_vertical_alignment_mode == "world_up";
            if (anyUsesWorldUp)
            {
                o.up_reference = DrawPopupField("Up Reference", o.up_reference, UpReferenceOptions, UpReferenceLabels, UpReferenceHelp, IndentLevel1 + ConditionalAdvance);
                if (o.up_reference == "custom")
                {
                    o.custom_up_x = DrawScalarField("Custom Up X", o.custom_up_x, CustomUpHelp, IndentLevel1 + ConditionalAdvance * 2);
                    o.custom_up_y = DrawScalarField("Custom Up Y", o.custom_up_y, CustomUpHelp, IndentLevel1 + ConditionalAdvance * 2);
                    o.custom_up_z = DrawScalarField("Custom Up Z", o.custom_up_z, CustomUpHelp, IndentLevel1 + ConditionalAdvance * 2);
                }
            }
        }

        private void DrawOrientationFacingOptionsSubSection(OrientationSettings o)
        {
            _showOrientationFacingOptions = EditorGUILayout.Foldout(_showOrientationFacingOptions, "Facing Options", true, EditorStyles.foldoutHeader);
            if (!_showOrientationFacingOptions) return;

            o.facing_mode = DrawPopupField("Facing", o.facing_mode, FacingModeOptions, FacingModeLabels, FacingModeHelp, IndentLevel1);

            if (o.facing_mode == "always_facing_camera")
                o.facing_basis = DrawPopupField("Facing Basis", o.facing_basis, FacingBasisOptions, FacingBasisLabels, FacingBasisHelp, IndentLevel1 + ConditionalAdvance);

            if (o.facing_mode == "wall_fixed" || o.facing_mode == "yaw_only")
            {
                EditorGUILayout.HelpBox(
                    "This mode uses each POI's own angles: Specific Marker > POI > Position > Facing X / Y / Z. Wall Fixed uses all three; Y Rotation Only keeps the X/Z tilt and replaces Y with a live camera-facing yaw.",
                    MessageType.Info);
            }
        }

        private void DrawOrientationUpdateCostSubSection(OrientationSettings o)
        {
            _showOrientationUpdateCost = EditorGUILayout.Foldout(_showOrientationUpdateCost, "Update Cost", true, EditorStyles.foldoutHeader);
            if (!_showOrientationUpdateCost) return;

            o.update_mode = DrawPopupField("Update Mode", o.update_mode, OrientationUpdateModeOptions, OrientationUpdateModeLabels, OrientationUpdateModeHelp, IndentLevel1);
            if (o.update_mode == "interval")
                o.update_interval_s = DrawScalarField("Update Interval (s)", o.update_interval_s, OrientationUpdateIntervalHelp, IndentLevel1 + ConditionalAdvance);
            if (o.update_mode == "on_camera_delta")
                o.camera_delta_deg = DrawScalarField("Camera Delta (deg)", o.camera_delta_deg, OrientationCameraDeltaHelp, IndentLevel1 + ConditionalAdvance);
        }
    }
}
