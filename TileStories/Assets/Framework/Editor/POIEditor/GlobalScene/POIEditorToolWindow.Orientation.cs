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
        private bool _showOrientationTest = true;

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
            EditorGUILayout.Space(4f);
            DrawOrientationTestSubSection(o);
        }

        private void DrawOrientationVerticalAlignmentSubSection(OrientationSettings o)
        {
            _showOrientationVerticalAlignment = EditorGUILayout.Foldout(_showOrientationVerticalAlignment, "Vertical Alignment", true, EditorStyles.foldoutHeader);
            if (!_showOrientationVerticalAlignment) return;

            // No extra IndentLevelScope here: these fields sit at the SAME ambient
            // indent level the "Vertical Alignment" foldout title itself was drawn at
            // (matching DrawGlobalLodSection's fields and Specific Marker's per-POI
            // sub-drawers, which collapse DrawFramedFoldout's own +1 back with a -1).
            // The foldout's own arrow glyph already reads as "one step in" without an
            // additional indent level (_5.1_Editor_Tab.md "Row Indentation & Spacing").
            o.vertical_alignment_mode = DrawPopupField("Marker", o.vertical_alignment_mode,
                VerticalAlignmentModeOptions, VerticalAlignmentModeLabels, VerticalAlignmentModeHelp);

            o.label_vertical_alignment_mode = DrawPopupField("Label", o.label_vertical_alignment_mode,
                ChildVerticalAlignmentModeOptions, ChildVerticalAlignmentModeLabels, ChildVerticalAlignmentHelp);

            if (_config.marker_use_badge)
            {
                o.badge_vertical_alignment_mode = DrawPopupField("Badge", o.badge_vertical_alignment_mode,
                    ChildVerticalAlignmentModeOptions, ChildVerticalAlignmentModeLabels, ChildVerticalAlignmentHelp);
            }

            o.cluster_vertical_alignment_mode = DrawPopupField("Clusters", o.cluster_vertical_alignment_mode,
                ChildVerticalAlignmentModeOptions, ChildVerticalAlignmentModeLabels, ChildVerticalAlignmentHelp);

            // up_reference only matters wherever something is actually set to World Up.
            bool anyUsesWorldUp = o.vertical_alignment_mode == "world_up"
                || o.label_vertical_alignment_mode == "world_up"
                || o.badge_vertical_alignment_mode == "world_up"
                || o.cluster_vertical_alignment_mode == "world_up";
            if (anyUsesWorldUp)
            {
                o.up_reference = DrawPopupField("Up Reference", o.up_reference, UpReferenceOptions, UpReferenceLabels, UpReferenceHelp, SubFieldIndentPixels);
                if (o.up_reference == "custom")
                {
                    o.custom_up_x = DrawScalarField("Custom Up X", o.custom_up_x, CustomUpHelp, SubFieldIndentPixels * 2);
                    o.custom_up_y = DrawScalarField("Custom Up Y", o.custom_up_y, CustomUpHelp, SubFieldIndentPixels * 2);
                    o.custom_up_z = DrawScalarField("Custom Up Z", o.custom_up_z, CustomUpHelp, SubFieldIndentPixels * 2);
                }
            }
        }

        private void DrawOrientationFacingOptionsSubSection(OrientationSettings o)
        {
            _showOrientationFacingOptions = EditorGUILayout.Foldout(_showOrientationFacingOptions, "Facing Options", true, EditorStyles.foldoutHeader);
            if (!_showOrientationFacingOptions) return;

            o.facing_mode = DrawPopupField("Facing", o.facing_mode, FacingModeOptions, FacingModeLabels, FacingModeHelp);

            if (o.facing_mode == "always_facing_camera")
                o.facing_basis = DrawPopupField("Facing Basis", o.facing_basis, FacingBasisOptions, FacingBasisLabels, FacingBasisHelp, SubFieldIndentPixels);

            if (o.facing_mode == "wall_fixed" || o.facing_mode == "yaw_only")
            {
                EditorGUILayout.HelpBox(
                    "This mode uses the per-POI angles authored in the Specific Marker tab's own Facing Options row (Position foldout). Wall Fixed uses all three angles; Y Rotation Only uses only the X/Z tilt and replaces Y with a live camera-facing yaw.",
                    MessageType.Info);
            }
        }

        private void DrawOrientationUpdateCostSubSection(OrientationSettings o)
        {
            _showOrientationUpdateCost = EditorGUILayout.Foldout(_showOrientationUpdateCost, "Update Cost", true, EditorStyles.foldoutHeader);
            if (!_showOrientationUpdateCost) return;

            o.update_mode = DrawPopupField("Update Mode", o.update_mode, OrientationUpdateModeOptions, OrientationUpdateModeLabels, OrientationUpdateModeHelp);
            if (o.update_mode == "interval")
                o.update_interval_s = DrawScalarField("Update Interval (s)", o.update_interval_s, OrientationUpdateIntervalHelp, SubFieldIndentPixels);
            if (o.update_mode == "on_camera_delta")
                o.camera_delta_deg = DrawScalarField("Camera Delta (deg)", o.camera_delta_deg, OrientationCameraDeltaHelp, SubFieldIndentPixels);
        }

        private void DrawOrientationTestSubSection(OrientationSettings o)
        {
            _showOrientationTest = EditorGUILayout.Foldout(_showOrientationTest, "Test", true, EditorStyles.foldoutHeader);
            if (!_showOrientationTest) return;

            o.edit_mode_preview_enabled = DrawToggleField("Edit-Mode Preview", o.edit_mode_preview_enabled, EditModePreviewHelp);

            EditorGUILayout.Space(4f);

            // "How To Test" must sit at the SAME level as "Edit-Mode Preview" above it --
            // a plain EditorGUILayout.LabelField with no DrawEditorRow spacer renders
            // flush with the ambient indentLevel only, which does NOT match the manual
            // spacer width DrawToggleField's row draws, so the two visibly misaligned.
            // Routing the label through the same DrawEditorRow/EditorRowEnd pair every
            // other row in this window uses (Reusable Row-Layout Command, single-label
            // recipe) fixes that by construction.
            DrawEditorRow(out float labelRowWidth, out _);
            EditorGUILayout.LabelField("How To Test", EditorStyles.boldLabel, GUILayout.Width(labelRowWidth), GUILayout.ExpandWidth(false));
            EditorRowEnd();

            // Always-visible guide instead of a popup: the developer runs the click-by-
            // click steps below while looking at the Scene/Game view, so the text needs
            // to stay on screen the whole time rather than living behind an "(i)" button
            // that closes the moment focus moves elsewhere.
            DrawEditorRow(out float rowWidth, out _);
            {
                var guideStyle = EditorStyles.textArea;
                float guideHeight = guideStyle.CalcHeight(new GUIContent(OrientationTestWorkflowHelp), rowWidth);
                EditorGUILayout.SelectableLabel(OrientationTestWorkflowHelp, guideStyle,
                    GUILayout.Width(rowWidth), GUILayout.Height(guideHeight), GUILayout.ExpandWidth(false));
            }
            EditorRowEnd();
        }
    }
}
