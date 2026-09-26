// POIEditorToolWindow.Displacement.cs
//
// Partial: the Displacement section of Global Scene (_2.5_Marker_Displacement.md) -- Enable, then
// sub-foldouts in the order a developer tunes them: Overlap Detection (when is a group crowded),
// Resolution (what moves, how, who gives way, how far), Leader Lines, and the shared Test sub-foldout
// with the displacement demo and the Live Displacement Readout. Texts live in DisplacementHelp.cs.

using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    public partial class POIEditorToolWindow
    {
        private bool _showDisplacementOverlap = true;
        private bool _showDisplacementResolution = true;
        private bool _showDisplacementLeaderLines = true;
        private readonly TestGuideState _displacementTest = new TestGuideState();

        private void DrawGlobalDisplacementSection()
        {
            if (_config == null || _config.displacement_settings == null)
            {
                EditorGUILayout.HelpBox("No displacement settings to configure.", MessageType.Info);
                return;
            }

            var disp = _config.displacement_settings;
            disp.enabled = DrawToggleField("Enable Displacement", disp.enabled, DisplacementEnabledHelp);

            // off hides the settings (their values are kept); the Test stays, like every domain's
            if (disp.enabled)
            {
                DrawDisplacementOverlapSubSection(disp);
                DrawDisplacementResolutionSubSection(disp);
                DrawDisplacementLeaderLinesSubSection(disp);
            }

            DrawDomainTestSubSection(_displacementTest, DisplacementSceneTestGuide, DisplacementPlaymodeTestGuide,
                DisplacementDeviceTestGuide, DrawDisplacementTestControls);
        }

        // When is a group crowded
        private void DrawDisplacementOverlapSubSection(DisplacementSettings disp)
        {
            EditorGUILayout.Space(4f);
            _showDisplacementOverlap = EditorGUILayout.Foldout(_showDisplacementOverlap, "Overlap Detection", true, EditorStyles.foldoutHeader);
            if (!_showDisplacementOverlap) return;

            disp.overlap_threshold_px = DrawSliderField("Overlap Distance (px)", disp.overlap_threshold_px,
                DisplacementSettings.MinOverlapPx, DisplacementSettings.MaxOverlapPx, DisplacementOverlapHelp, IndentLevel1);
        }

        // What moves, how the group is spread, who gives way, how far
        private void DrawDisplacementResolutionSubSection(DisplacementSettings disp)
        {
            EditorGUILayout.Space(4f);
            _showDisplacementResolution = EditorGUILayout.Foldout(_showDisplacementResolution, "Resolution", true, EditorStyles.foldoutHeader);
            if (!_showDisplacementResolution) return;

            disp.displace_target = DrawPopupField("What Moves", disp.displace_target, DisplaceTargetOptions, DisplaceTargetLabels,
                DisplacementTargetHelp, IndentLevel1);
            disp.displacement_algorithm = DrawPopupField("Algorithm", disp.displacement_algorithm, DisplacementAlgorithmOptions,
                DisplacementAlgorithmLabels, DisplacementAlgorithmHelp, IndentLevel1);
            disp.displacement_tiebreak = DrawPopupField("Who Gives Way", disp.displacement_tiebreak, DisplacementTiebreakOptions,
                DisplacementTiebreakLabels, DisplacementTiebreakHelp, IndentLevel1);
            disp.max_displacement_px = DrawSliderField("Max Move (px)", disp.max_displacement_px, 0f,
                DisplacementSettings.MaxMovePxLimit, DisplacementMaxMoveHelp, IndentLevel1);
        }

        // The line from a moved element back to where it belongs
        private void DrawDisplacementLeaderLinesSubSection(DisplacementSettings disp)
        {
            EditorGUILayout.Space(4f);
            _showDisplacementLeaderLines = EditorGUILayout.Foldout(_showDisplacementLeaderLines, "Leader Lines", true, EditorStyles.foldoutHeader);
            if (!_showDisplacementLeaderLines) return;

            disp.leader_lines_enabled = DrawToggleField("Enable Leader Lines", disp.leader_lines_enabled, LeaderLinesEnabledHelp, IndentLevel1);
            if (!disp.leader_lines_enabled) return;

            float inner = IndentLevel1 + ConditionalAdvance;
            disp.leader_line_style = DrawPopupField("Style", disp.leader_line_style, LeaderLineStyleOptions, LeaderLineStyleLabels,
                LeaderLineStyleHelp, inner);
            disp.leader_line_min_distance_px = DrawSliderField("Min Length (px)", disp.leader_line_min_distance_px, 0f,
                DisplacementSettings.MaxLeaderMinLengthPx, LeaderLineMinLengthHelp, inner);
            disp.leader_line_width = DrawSliderField("Width (m)", disp.leader_line_width, DisplacementSettings.MinLeaderWidthM,
                DisplacementSettings.MaxLeaderWidthM, LeaderLineWidthHelp, inner);
            disp.leader_line_opacity = DrawSliderField("Opacity", disp.leader_line_opacity, 0f, 1f, LeaderLineOpacityHelp, inner);
        }

        // Displacement > Test, before the guides: the demo, then what displacement is doing right now
        private void DrawDisplacementTestControls()
        {
            DrawDisplacementDemoControls();
            DrawDisplacementLiveReadout();
        }

        // "Add displacement demo" and, while it is on, its settings (built by DisplacementDemoSpawner from WallSession)
        private void DrawDisplacementDemoControls()
        {
            if (_config.displacement_demo == null) _config.displacement_demo = new DisplacementDemoSettings();
            var demo = _config.displacement_demo;

            bool wasOn = demo.enabled;
            demo.enabled = DrawToggleField("Add displacement demo", demo.enabled, DisplacementDemoHelp, IndentLevel1);
            MakeThisTheOnlyActiveDemoView(DemoView.DisplacementDemo, demo.enabled && !wasOn);
            if (!demo.enabled) return;

            float inner = IndentLevel1 + ConditionalAdvance;
            demo.markers_per_group = DrawIntSliderField("Markers per Group", demo.markers_per_group,
                DisplacementDemoSettings.MinMarkersPerGroup, DisplacementDemoSettings.MaxMarkersPerGroup, DisplacementDemoGroupHelp, inner);
            demo.spread_cm = DrawSliderField("Spread (cm)", demo.spread_cm, 0f, DisplacementDemoSettings.MaxSpreadCm,
                DisplacementDemoSpreadHelp, inner);
            demo.distance_m = DrawSliderField("Distance (m)", demo.distance_m, DisplacementDemoSettings.MinDistanceM,
                DisplacementDemoSettings.MaxDistanceM, DisplacementDemoDistanceHelp, inner);
            demo.show_labels = DrawToggleField("Show labels", demo.show_labels, DisplacementDemoLabelsHelp, inner);
            demo.reference_copies = DrawPopupField("Reference copies", demo.reference_copies, DisplacementDemoLayout.ReferenceModes,
                DisplacementDemoReferenceLabels, DisplacementDemoReferenceHelp, inner);
            demo.run_lod = DrawToggleField("Run LOD on the demo", demo.run_lod, DisplacementDemoRunLodHelp, inner);
        }

        // "Live Displacement Readout": in Play Mode, what the running displacement step decided last time
        private void DrawDisplacementLiveReadout()
        {
            EditorGUILayout.Space(4f);
            DrawEditorRow(out float rowWidth, out _, IndentLevel1);
            GUILayout.Label("Live Displacement Readout", EditorStyles.miniBoldLabel, GUILayout.Width(Mathf.Max(40f, rowWidth - 36f)), GUILayout.ExpandWidth(false));
            HelpInfoButton.Draw("Live Displacement Readout", DisplacementReadoutHelp);
            EditorRowEnd();

            string text;
            if (!Application.isPlaying) text = "Press Play to see what displacement decides, live.";
            else
            {
                _liveReadoutDrawnAt = EditorApplication.timeSinceStartup;
                var lod = FindFirstObjectByType<LODController>();
                text = lod == null ? "The running scene has no LOD pipeline (displacement runs inside it)."
                    : lod.LastDisplacementStats == null ? "Displacement is off (Enable Displacement) or has not run yet."
                    : DisplacementLiveReadout(lod.LastDisplacementStats);
            }
            DrawEditorRow(out float textWidth, out _, IndentLevel1);
            GUILayout.Label(text, EditorStyles.helpBox, GUILayout.Width(textWidth), GUILayout.ExpandWidth(false));
            EditorRowEnd();
        }

        // The readout's text, one fact per line (pure, so its wording is tested)
        internal static string DisplacementLiveReadout(DisplacementStats s)
        {
            var inv = System.Globalization.CultureInfo.InvariantCulture;
            var sb = new System.Text.StringBuilder();
            sb.Append("Markers looked at: ").Append(s.Candidates);
            sb.Append("\nCrowded groups: ").Append(s.Groups);
            sb.Append("\nMoved: ").Append(s.Moved).Append(" (longest move ").Append(s.LargestMovePx.ToString("0", inv)).Append(" px)");
            sb.Append("\nLabels hidden by Max Move: ").Append(s.LabelsHidden);
            sb.Append("\nLeader lines: ").Append(s.LeaderLines);
            return sb.ToString();
        }
    }
}
