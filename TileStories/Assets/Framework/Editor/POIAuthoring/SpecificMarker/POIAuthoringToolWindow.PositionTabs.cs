// POIAuthoringToolWindow.PositionTabs.cs
//
// Partial: Single state-aware Position foldout inside the Specific Marker Position foldout.
// Editor-only.

using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    public partial class POIAuthoringToolWindow
    {
        // Entry point called from DrawSpecificMarkerOptions for each POI.
        internal void DrawPositionTabs(POIData poi)
        {
            if (poi == null) return;

            bool hasCapture = poi.has_captured_position && poi.captured_position != null;

            if (!hasCapture)
            {
                // Not captured state: show hint + Capture Position button
                EditorGUILayout.HelpBox("Move the marker in the Scene view, then press Capture to lock its position.", MessageType.Info);
                EditorGUILayout.Space(4f);
                if (GUILayout.Button("Capture Position", GUILayout.Width(140f)))
                {
                    CaptureSinglePoi(poi);
                }
            }
            else
            {
                // Captured state: show read-only captured position + Clear Capture button
                var cp = poi.captured_position;
                EditorGUILayout.LabelField("Captured Position (world space)", EditorStyles.boldLabel);
                EditorGUI.BeginDisabledGroup(true);
                cp.x = EditorGUILayout.FloatField("X", cp.x);
                cp.y = EditorGUILayout.FloatField("Y", cp.y);
                cp.z = EditorGUILayout.FloatField("Z", cp.z);
                EditorGUI.EndDisabledGroup();

                if (!string.IsNullOrEmpty(poi.captured_position_source))
                    EditorGUILayout.LabelField("Source", poi.captured_position_source);
                if (poi.captured_position_timestamp > 0)
                    EditorGUILayout.LabelField("Timestamp", System.DateTimeOffset.FromUnixTimeSeconds(poi.captured_position_timestamp).ToLocalTime().ToString());

                EditorGUILayout.Space(4f);
                if (GUILayout.Button("Clear Capture", GUILayout.Width(120f)))
                {
                    poi.captured_position = null;
                    poi.has_captured_position = false;
                    poi.captured_position_source = null;
                    poi.captured_position_timestamp = 0;
                    _hasUnsavedChanges = true;
                }
            }

            // Edit Rotation slider (always visible)
            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Edit Rotation", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("", GUILayout.Width(90f));
                DrawConfigMutationScope(
                    () =>
                    {
                        poi.editor_rotation_deg = EditorGUILayout.Slider(poi.editor_rotation_deg, 0f, 360f);
                        if (GUI.changed)
                            ApplyPoiEditorRotation(poi);
                    },
                    refreshRigOnChange: false);
            }
        }

        // Capture position for a single POI using the existing rig capture logic.
        private void CaptureSinglePoi(POIData poi)
        {
            // Ensure rig exists - GetExistingRig internally calls TryResolveSceneReferences
            var rigRoot = GetExistingRig();
            if (rigRoot == null)
            {
                EditorUtility.DisplayDialog("Capture Position", "Rig not found. Please populate the rig first.", "OK");
                return;
            }

            // Find the rig object for this POI
            var poiAnchor = rigRoot.Find(poi.id)?.GetComponent<POIAnchor>();
            if (poiAnchor == null)
            {
                EditorUtility.DisplayDialog("Capture Position", $"POI anchor '{poi.id}' not found in rig.", "OK");
                return;
            }

            // Capture world position of the anchor
            Vector3 worldPos = poiAnchor.transform.position;
            poi.captured_position = new CapturedPosition
            {
                x = worldPos.x,
                y = worldPos.y,
                z = worldPos.z
            };
            poi.has_captured_position = true;
            poi.captured_position_source = "scene";
            poi.captured_position_timestamp = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            _hasUnsavedChanges = true;
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