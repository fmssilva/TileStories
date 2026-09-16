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

            // Shared rows: each coordinate (X / Y / Z) is its own read-only row with
            // the transparent indent spacer + width capped to rowWidth (level-2 indent).
            EditorGUI.BeginDisabledGroup(true);
            var coordinateLabels = GetCoordinateLabels();
            for (int i = 0; i < coordinateLabels.Length; i++)
            {
                DrawEditorRow(out float coordRow, out _);
                EditorGUILayout.LabelField(coordinateLabels[i] + ":", EditorStyles.miniBoldLabel, GUILayout.Width(28f));
                float value = i == 0 ? position.x : i == 1 ? position.y : position.z;
                EditorGUILayout.FloatField(value, GUILayout.Width(coordRow - 36f), GUILayout.ExpandWidth(false));
                EditorRowEnd();
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space(2f);

            // Shared editor row: transparent indent spacer + width capped to
            // max(MinRowWidth, min(visible panel, MaxRowWidth)). The verify button
            // takes the capped width (no trailing status text).
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
            child.localRotation = PoiRotationResolver.ToYawQuaternion(poi.editor_rotation_deg);
        }
    }
}
