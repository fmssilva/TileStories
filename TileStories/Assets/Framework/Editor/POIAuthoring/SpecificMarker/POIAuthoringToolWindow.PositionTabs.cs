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

            bool hasPosition = poi.position != null;

            if (!hasPosition)
            {
                // No position yet: show hint + Capture Position button
                EditorGUILayout.HelpBox("Move the marker in the Scene view, then press Capture to store its position.", MessageType.Info);
                EditorGUILayout.Space(4f);
                if (GUILayout.Button("Capture Position", GUILayout.Width(140f)))
                {
                    CaptureSinglePoi(poi);
                }
            }
            else
            {
                // Position state: show read-only position + verified toggle + clear button
                var p = poi.position;
                EditorGUILayout.LabelField("Position (world space)", EditorStyles.boldLabel);
                EditorGUI.BeginDisabledGroup(true);
                p.x = EditorGUILayout.FloatField("X", p.x);
                p.y = EditorGUILayout.FloatField("Y", p.y);
                p.z = EditorGUILayout.FloatField("Z", p.z);
                EditorGUI.EndDisabledGroup();

                EditorGUILayout.Space(4f);
                bool newVerified = EditorGUILayout.Toggle("Verified", poi.position_verified);
                if (newVerified != poi.position_verified)
                {
                    DrawConfigMutationScope(() => { poi.position_verified = newVerified; }, true);
                }

                EditorGUILayout.Space(4f);
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Capture Position", GUILayout.Width(140f)))
                    {
                        CaptureSinglePoi(poi);
                    }
                    if (GUILayout.Button("Clear Position", GUILayout.Width(120f)))
                    {
                        DrawConfigMutationScope(() =>
                        {
                            poi.position = null;
                            poi.position_verified = false;
                        }, true);
                    }
                    if (GUILayout.Button("Mark Unverified", GUILayout.Width(130f)))
                    {
                        DrawConfigMutationScope(() => { poi.position_verified = false; }, true);
                    }
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
            DrawConfigMutationScope(() =>
            {
                poi.position = new PositionData
                {
                    x = worldPos.x,
                    y = worldPos.y,
                    z = worldPos.z
                };
                poi.position_verified = true;
            }, true);
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
