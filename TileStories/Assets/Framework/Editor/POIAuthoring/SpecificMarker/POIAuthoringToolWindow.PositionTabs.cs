// POIAuthoringToolWindow.PositionTabs.cs
//
// Partial: Position Draft / Precise tabs inside the Specific Marker Position foldout.
// (Step 20 – skeleton). Editor-only.

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    public partial class POIAuthoringToolWindow
    {
        // Tab state per POI (key = POI id)
        private readonly Dictionary<string, PositionTab> _positionTabs = new();

        private enum PositionTab
        {
            Draft,
            Precise
        }

        // Entry point called from DrawSpecificMarkerOptions for each POI.
        internal void DrawPositionTabs(POIData poi)
        {
            if (poi == null) return;

            string key = string.IsNullOrWhiteSpace(poi.id) ? "unknown" : poi.id;
            if (!_positionTabs.TryGetValue(key, out var tab)) tab = PositionTab.Draft;

            // Tab buttons
            using (new EditorGUILayout.HorizontalScope())
            {
                bool isDraft = tab == PositionTab.Draft;
                bool isPrecise = tab == PositionTab.Precise;

                if (GUILayout.Toggle(isDraft, "Draft", EditorStyles.toolbarButton))
                {
                    if (!isDraft) { tab = PositionTab.Draft; _positionTabs[key] = tab; _hasUnsavedChanges = true; }
                }
                if (GUILayout.Toggle(isPrecise, "Precise", EditorStyles.toolbarButton))
                {
                    if (!isPrecise) { tab = PositionTab.Precise; _positionTabs[key] = tab; _hasUnsavedChanges = true; }
                }
            }

            // Show warning when Precise selected but no captured position
            if (tab == PositionTab.Precise && !poi.has_captured_position)
            {
                EditorGUILayout.HelpBox("Capture position first – press the Capture button in Draft tab.", MessageType.Warning);
            }

            // Content per tab
            if (tab == PositionTab.Draft)
                DrawDraftTab(poi);
            else
                DrawPreciseTab(poi);
        }

        private void DrawDraftTab(POIData poi)
        {
            // Normalized position sliders
            poi.x_norm = EditorGUILayout.Slider("X norm", poi.x_norm, 0f, 1f);
            poi.y_norm = EditorGUILayout.Slider("Y norm", poi.y_norm, 0f, 1f);

            EditorGUILayout.Space(4f);
            if (GUILayout.Button("Capture Position", GUILayout.Width(140f)))
            {
                // Delegate to existing rig capture logic for this POI only
                CaptureSinglePoi(poi);
            }
        }

        private void DrawPreciseTab(POIData poi)
        {
            if (poi.captured_position == null)
            {
                EditorGUILayout.LabelField("No captured position yet.", EditorStyles.wordWrappedMiniLabel);
                return;
            }

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
    }
}