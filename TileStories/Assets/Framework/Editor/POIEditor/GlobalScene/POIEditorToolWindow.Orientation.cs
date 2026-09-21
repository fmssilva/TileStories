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
        // The three test guides start collapsed: the developer opens only the one they need.
        private bool _showOrientationSceneTestGuide;
        private bool _showOrientationPlaymodeTestGuide;
        private bool _showOrientationDeviceTestGuide;

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

            // Rows inside a sub-foldout are its CHILDREN: SectionChildIndentPixels puts their label
            // under the foldout title text (a raw pixel step through DrawEditorRow, not an
            // IndentLevelScope, so the row's own control never double-indents; the value column
            // stays aligned through FieldLabelWidthCompensationScope). _5.1_Editor_Tab.md
            // "Row Indentation & Spacing".
            o.vertical_alignment_mode = DrawPopupField("Marker", o.vertical_alignment_mode,
                VerticalAlignmentModeOptions, VerticalAlignmentModeLabels, VerticalAlignmentModeHelp, SectionChildIndentPixels);

            o.label_vertical_alignment_mode = DrawPopupField("Label", o.label_vertical_alignment_mode,
                ChildVerticalAlignmentModeOptions, ChildVerticalAlignmentModeLabels, ChildVerticalAlignmentHelp, SectionChildIndentPixels);

            if (_config.marker_use_badge)
            {
                o.badge_vertical_alignment_mode = DrawPopupField("Badge", o.badge_vertical_alignment_mode,
                    ChildVerticalAlignmentModeOptions, ChildVerticalAlignmentModeLabels, ChildVerticalAlignmentHelp, SectionChildIndentPixels);
            }

            o.cluster_vertical_alignment_mode = DrawPopupField("Clusters", o.cluster_vertical_alignment_mode,
                ChildVerticalAlignmentModeOptions, ChildVerticalAlignmentModeLabels, ChildVerticalAlignmentHelp, SectionChildIndentPixels);

            // up_reference only matters wherever something is actually set to World Up.
            bool anyUsesWorldUp = o.vertical_alignment_mode == "world_up"
                || o.label_vertical_alignment_mode == "world_up"
                || o.badge_vertical_alignment_mode == "world_up"
                || o.cluster_vertical_alignment_mode == "world_up";
            if (anyUsesWorldUp)
            {
                o.up_reference = DrawPopupField("Up Reference", o.up_reference, UpReferenceOptions, UpReferenceLabels, UpReferenceHelp, SectionChildIndentPixels + SubFieldIndentPixels);
                if (o.up_reference == "custom")
                {
                    o.custom_up_x = DrawScalarField("Custom Up X", o.custom_up_x, CustomUpHelp, SectionChildIndentPixels + SubFieldIndentPixels * 2);
                    o.custom_up_y = DrawScalarField("Custom Up Y", o.custom_up_y, CustomUpHelp, SectionChildIndentPixels + SubFieldIndentPixels * 2);
                    o.custom_up_z = DrawScalarField("Custom Up Z", o.custom_up_z, CustomUpHelp, SectionChildIndentPixels + SubFieldIndentPixels * 2);
                }
            }
        }

        private void DrawOrientationFacingOptionsSubSection(OrientationSettings o)
        {
            _showOrientationFacingOptions = EditorGUILayout.Foldout(_showOrientationFacingOptions, "Facing Options", true, EditorStyles.foldoutHeader);
            if (!_showOrientationFacingOptions) return;

            o.facing_mode = DrawPopupField("Facing", o.facing_mode, FacingModeOptions, FacingModeLabels, FacingModeHelp, SectionChildIndentPixels);

            if (o.facing_mode == "always_facing_camera")
                o.facing_basis = DrawPopupField("Facing Basis", o.facing_basis, FacingBasisOptions, FacingBasisLabels, FacingBasisHelp, SectionChildIndentPixels + SubFieldIndentPixels);

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

            o.update_mode = DrawPopupField("Update Mode", o.update_mode, OrientationUpdateModeOptions, OrientationUpdateModeLabels, OrientationUpdateModeHelp, SectionChildIndentPixels);
            if (o.update_mode == "interval")
                o.update_interval_s = DrawScalarField("Update Interval (s)", o.update_interval_s, OrientationUpdateIntervalHelp, SectionChildIndentPixels + SubFieldIndentPixels);
            if (o.update_mode == "on_camera_delta")
                o.camera_delta_deg = DrawScalarField("Camera Delta (deg)", o.camera_delta_deg, OrientationCameraDeltaHelp, SectionChildIndentPixels + SubFieldIndentPixels);
        }

        private void DrawOrientationTestSubSection(OrientationSettings o)
        {
            _showOrientationTest = EditorGUILayout.Foldout(_showOrientationTest, "Test", true, EditorStyles.foldoutHeader);
            if (!_showOrientationTest) return;

            o.edit_mode_preview_enabled = DrawToggleField("Scene-Mode Preview", o.edit_mode_preview_enabled, EditModePreviewHelp, SectionChildIndentPixels);

            EditorGUILayout.Space(4f);

            // Three separate guides, one per test area, each collapsed by default so the
            // developer opens only the kind of test being run. Each stays an always-visible
            // text block (not a popup): the steps are followed while looking at the Scene/Game
            // view, and a popup would close the moment focus moves there.
            _showOrientationSceneTestGuide = DrawTestGuideFoldout(_showOrientationSceneTestGuide, "How to Scene Test", OrientationSceneTestGuide);
            _showOrientationPlaymodeTestGuide = DrawTestGuideFoldout(_showOrientationPlaymodeTestGuide, "How to Playmode Test", OrientationPlaymodeTestGuide);
            _showOrientationDeviceTestGuide = DrawTestGuideFoldout(_showOrientationDeviceTestGuide, "How to Device Test", OrientationDeviceTestGuide);
        }

        // One collapsible guide: a foldout title (a child of "Test", same level as the Scene-Mode
        // Preview row) and, when open, the read-only text block one step deeper. Both go through
        // DrawEditorRow so the spacer supplies the indent; indentLevel is zeroed around the two
        // controls because Foldout and SelectableLabel would otherwise re-apply the ambient indent
        // on top of the spacer (they landed twice as far right before).
        private bool DrawTestGuideFoldout(bool isOpen, string title, string guideText)
        {
            int savedIndent = EditorGUI.indentLevel;
            DrawEditorRow(out _, out _, SectionChildIndentPixels);
            EditorGUI.indentLevel = 0;
            isOpen = EditorGUILayout.Foldout(isOpen, title, true, EditorStyles.foldout);
            EditorGUI.indentLevel = savedIndent;
            EditorRowEnd();
            if (!isOpen) return false;

            DrawEditorRow(out float rowWidth, out _, SectionChildIndentPixels * 2f);
            EditorGUI.indentLevel = 0;
            var guideStyle = EditorStyles.textArea;
            float guideHeight = guideStyle.CalcHeight(new GUIContent(guideText), rowWidth);
            EditorGUILayout.SelectableLabel(guideText, guideStyle,
                GUILayout.Width(rowWidth), GUILayout.Height(guideHeight), GUILayout.ExpandWidth(false));
            EditorGUI.indentLevel = savedIndent;
            EditorRowEnd();
            return true;
        }
    }
}
