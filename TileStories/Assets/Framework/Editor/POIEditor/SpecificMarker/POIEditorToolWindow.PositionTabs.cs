// POIEditorToolWindow.PositionTabs.cs
//
// Partial: Single state-aware Position foldout inside the Specific Marker Position foldout.
// Editor-only.

using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    public partial class POIEditorToolWindow
    {
        internal static string[] GetCoordinateLabels()
        {
            return new[] { "X", "Y", "Z" };
        }

        // Entry point called from DrawSpecificMarkerOptions for each POI.
        // Rows sit inside POI (level 1) + DrawFramedFoldout content (level 2); the
        // -1 scope collapses them to one small step past the sub-foldout title.
        internal void DrawPositionTabs(POIData poi)
        {
            if (poi == null) return;

            using (new EditorGUI.IndentLevelScope(-1))
            {

            bool hasPosition = poi.position != null;

            if (!hasPosition)
            {
                // No position yet: hint that moving the marker in Scene view sets position
                EditorGUILayout.HelpBox("Move the marker in the Scene view to set its position (positions are updated automatically).", MessageType.Info);
                return;
            }

            var position = poi.position;

            // Layout order is intentionally fixed: rotation first, coords second, then verified state.
            // Shared row: label keeps a fixed width; slider takes rowWidth minus the
            // label + spacing, inside the capped row (level-2 indent measured at call time).
            DrawEditorRow(out float rotationRow, out _);
            {
                EditorGUILayout.LabelField("Edit Rotation", EditorStyles.boldLabel, GUILayout.Width(92f));
                DrawConfigMutationScope(
                    () =>
                    {
                        float sliderW = Mathf.Max(120f, rotationRow - 104f);
                        poi.editor_rotation_deg = EditorGUILayout.Slider(poi.editor_rotation_deg, 0f, 360f,
                            GUILayout.Width(sliderW), GUILayout.ExpandWidth(false));
                        if (GUI.changed)
                            ApplyPoiEditorRotation(poi);
                    },
                    refreshRigOnChange: false);
            }
            EditorRowEnd();

            EditorGUILayout.Space(2f);

            // One compact read-only row: x [val]  y [val]  z [val]. The axis letters are
            // drawn OUTSIDE any disabled scope (see DrawCoordinateRow) -- this row is the
            // only one in the window whose label sat inside BeginDisabledGroup(true), and
            // it is the only label that never showed up.
            DrawCoordinateRow(position.x, position.y, position.z, out _, out _, out _);

            EditorGUILayout.Space(2f);

            // Shared editor row: transparent indent spacer + width capped to
            // max(MinRowWidth, min(visible panel, MaxRowWidth)). The verify button
            // takes the capped width (no trailing status text). Nested two indent
            // levels deeper than the coordinate rows so it reads as the final
            // "commit this position" action rather than another coordinate line.
            using (new EditorGUI.IndentLevelScope(2))
            {
                DrawEditorRow(out float verifyRowWidth, out _);
                {
                    string buttonLabel = poi.position_verified ? "Verified" : "Unverified";
                    Color originalColor = GUI.backgroundColor;
                    GUI.backgroundColor = poi.position_verified ? new Color(0.2f, 0.7f, 0.2f) : new Color(0.8f, 0.3f, 0.3f);

                    if (GUILayout.Button(buttonLabel, GUILayout.Width(verifyRowWidth), GUILayout.ExpandWidth(false)))
                    {
                        TogglePoiVerification(poi);
                    }

                    GUI.backgroundColor = originalColor;
                }
                EditorRowEnd();
            }
            }
        }

        // One compact read-only row: x [val]  y [val]  z [val].
        //
        // WHY THE LETTERS LIVE OUTSIDE ANY DISABLED SCOPE: for a long time this was the
        // only row in the whole window that drew its label inside
        // EditorGUI.BeginDisabledGroup(true), and it was the only label that never showed
        // up. Every other visible label here ("Edit Rotation", "Category", "Has status")
        // is drawn un-disabled with the same EditorStyles.boldLabel and renders fine, so
        // the disabled scope was the one structural difference. The fix is therefore
        // structural too: only the VALUE FIELDS are read-only; the axis letters are
        // ordinary un-disabled labels.
        //
        // The first label/value rects are returned via out params so a real OnGUI render
        // test can assert the geometry (same pattern as DrawAddButtonRow).
        internal static void DrawCoordinateRow(float x, float y, float z,
            out Rect firstAxisLabelRect, out Rect firstAxisValueRect, out Rect lastAxisValueRect)
        {
            firstAxisLabelRect = default;
            firstAxisValueRect = default;
            lastAxisValueRect = default;

            DrawEditorRow(out float coordRow, out _);

            const float axisLabelWidth = 16f;
            const float pairGapWidth = 6f;
            float valueWidth = Mathf.Max(60f, (coordRow - 3f * axisLabelWidth - 2f * pairGapWidth) / 3f);

            string[] axes = GetCoordinateLabels();
            float[] axisValues = { x, y, z };
            for (int i = 0; i < axes.Length; i++)
            {
                GUILayout.Label(axes[i], EditorStyles.boldLabel, GUILayout.Width(axisLabelWidth));
                if (i == 0)
                    firstAxisLabelRect = GUILayoutUtility.GetLastRect();

                using (new EditorGUI.DisabledScope(true))
                    EditorGUILayout.FloatField(axisValues[i], GUILayout.Width(valueWidth), GUILayout.ExpandWidth(false));
                if (i == 0)
                    firstAxisValueRect = GUILayoutUtility.GetLastRect();
                lastAxisValueRect = GUILayoutUtility.GetLastRect();

                if (i < axes.Length - 1)
                    GUILayout.Space(pairGapWidth);
            }

            EditorRowEnd();
        }

        // Toggles a POI's verification status with confirmation gating when unlocking.
        internal void TogglePoiVerification(POIData poi)
        {
            if (poi == null) return;

            if (!poi.position_verified)
            {
                // Unverified -> Verified: capture the current in-memory rig position as the
                // last verified anchor before locking the scene object. This is the value we
                // must snap back to if the user drags a verified marker before saving.
                var rig = GetExistingRig();
                var child = rig != null ? rig.Find(poi.id) : null;
                Vector3 verifiedPosition = child != null ? child.localPosition : (poi.position != null ? new Vector3(poi.position.x, poi.position.y, poi.position.z) : Vector3.zero);

                _lastVerifiedPositions[poi.id] = verifiedPosition;
                if (poi.position == null)
                    poi.position = new PositionData();
                poi.position.x = verifiedPosition.x;
                poi.position.y = verifiedPosition.y;
                poi.position.z = verifiedPosition.z;

                DrawConfigMutationScope(() => { poi.position_verified = true; }, true);
            }
            else
            {
                _lastVerifiedPositions.Remove(poi.id);

                // Verified -> Unverified: confirm if the user really wants to unlock editing
                bool skipPrompt = EditorPrefs.GetBool(SkipUnverifyPromptPrefKey, false);
                if (!skipPrompt)
                {
                    int choice = EditorUtility.DisplayDialogComplex(
                        "Edit Verified Positions",
                        $"The positions for '{poi.name}' are already verified.\n\nAre you sure you want to edit this POI's positions again?",
                        "Yes, Unlock",                // 0 = left button
                        "Cancel",                     // 1 = middle button
                        "Yes, and Don't Ask Again"); // 2 = right button

                    if (choice == 0) // Yes, Unlock
                    {
                        DrawConfigMutationScope(() => { poi.position_verified = false; }, true);
                    }
                    else if (choice == 2) // Yes, and Don't Ask Again
                    {
                        EditorPrefs.SetBool(SkipUnverifyPromptPrefKey, true);
                        DrawConfigMutationScope(() => { poi.position_verified = false; }, true);
                    }
                    // choice == 1 or closed via X: Cancel (do nothing)
                }
                else
                {
                    DrawConfigMutationScope(() => { poi.position_verified = false; }, true);
                }
            }
        }

        // Apply the POI's config-driven edit-scene yaw to its rig child so the
        // Scene view reflects the slider live. Runtime billboard ignores this;
        // it only affects the editor preview. Called on slider drag and by
        // RefreshRigVisuals so undo/redo/refresh stay in sync.
        private void ApplyPoiEditorRotation(POIData poi)
        {
            if (poi == null || string.IsNullOrWhiteSpace(poi.id))
                return;
            Transform rig = GetExistingRig();
            if (rig == null)
                return;
            Transform child = rig.Find(poi.id);
            if (child == null)
                return;
            child.localRotation = PoiRotationResolver.ToEulerQuaternion(poi.editor_rotation_x_deg, poi.editor_rotation_deg, poi.editor_rotation_z_deg);
        }
    }
}
