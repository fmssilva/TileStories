using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // Tier 0 EditMode tests for the Block 8 authoring foldout: JSON round-trip of the
    // DisplacementSettings schema (mirrors SaveConfig via JsonUtility, and the runtime
    // WallConfigLoader deserialize path) plus structural presence of the new foldout
    // wiring on the authoring window.
    public class DisplacementAuthoringRoundTripTests
    {
        [Test]
        public void RoundTrip_DisplacementSettings_ThroughJsonUtility()
        {
            var config = new WallConfigData { wall_id = "test_wall" };

            // Drive every control-backed field to a non-default value.
            config.displacement_settings.enabled = false;
            config.displacement_settings.overlap_threshold_px = 12f;
            config.displacement_settings.displacement_algorithm = "candidate_position";
            config.displacement_settings.force_directed_iterations = 6;
            config.displacement_settings.max_displacement_px = 80f;
            config.displacement_settings.displace_target = "both";
            config.displacement_settings.leader_lines_enabled = false;
            config.displacement_settings.leader_line_style = "elbow";
            config.displacement_settings.leader_line_min_distance_px = 9f;
            config.displacement_settings.leader_line_width = 0.03f;
            config.displacement_settings.leader_line_opacity = 0.6f;
            config.displacement_settings.displacement_tiebreak = "lower_priority_only";

            // Mirrors authoring SaveConfig + runtime WallConfigLoader.
            string json = JsonUtility.ToJson(config, true);
            var loaded = JsonUtility.FromJson<WallConfigData>(json);

            Assert.IsNotNull(loaded.displacement_settings, "displacement_settings survives round-trip");
            Assert.AreEqual(false, loaded.displacement_settings.enabled, "enabled survives round-trip");
            Assert.AreEqual(12f, loaded.displacement_settings.overlap_threshold_px, 0.001f, "overlap_threshold_px survives round-trip");
            Assert.AreEqual("candidate_position", loaded.displacement_settings.displacement_algorithm, "displacement_algorithm survives round-trip");
            Assert.AreEqual(6, loaded.displacement_settings.force_directed_iterations, "force_directed_iterations survives round-trip");
            Assert.AreEqual(80f, loaded.displacement_settings.max_displacement_px, 0.001f, "max_displacement_px survives round-trip");
            Assert.AreEqual("both", loaded.displacement_settings.displace_target, "displace_target survives round-trip");
            Assert.AreEqual(false, loaded.displacement_settings.leader_lines_enabled, "leader_lines_enabled survives round-trip");
            Assert.AreEqual("elbow", loaded.displacement_settings.leader_line_style, "leader_line_style survives round-trip");
            Assert.AreEqual(9f, loaded.displacement_settings.leader_line_min_distance_px, 0.001f, "leader_line_min_distance_px survives round-trip");
            Assert.AreEqual(0.03f, loaded.displacement_settings.leader_line_width, 0.001f, "leader_line_width survives round-trip");
            Assert.AreEqual(0.6f, loaded.displacement_settings.leader_line_opacity, 0.001f, "leader_line_opacity survives round-trip");
            Assert.AreEqual("lower_priority_only", loaded.displacement_settings.displacement_tiebreak, "displacement_tiebreak survives round-trip");
        }

        [Test]
        public void FoldoutSectionMethods_AndState_Exist()
        {
            var t = typeof(POIAuthoringToolWindow);
            Assert.IsNotNull(
                t.GetMethod("DrawGlobalDisplacementSection", BindingFlags.NonPublic | BindingFlags.Instance),
                "DrawGlobalDisplacementSection must be wired on the authoring window");
            Assert.IsNotNull(
                t.GetField("_showGlobalDisplacement", BindingFlags.NonPublic | BindingFlags.Instance),
                "_showGlobalDisplacement state field must exist");

            // Assert every field the foldout binds to exists on the schema.
            var dt = typeof(DisplacementSettings);
            Assert.IsNotNull(dt.GetField("enabled"), "DisplacementSettings.enabled must exist for the Enable toggle");
            Assert.IsNotNull(dt.GetField("overlap_threshold_px"), "DisplacementSettings.overlap_threshold_px must exist for the Overlap Threshold field");
            Assert.IsNotNull(dt.GetField("displace_target"), "DisplacementSettings.displace_target must exist for the Displace Target popup");
            Assert.IsNotNull(dt.GetField("displacement_algorithm"), "DisplacementSettings.displacement_algorithm must exist for the Algorithm dropdown (gates Relaxation Steps)");
            Assert.IsNotNull(dt.GetField("force_directed_iterations"), "DisplacementSettings.force_directed_iterations must exist for the Relaxation Steps field");
            Assert.IsNotNull(dt.GetField("max_displacement_px"), "DisplacementSettings.max_displacement_px must exist for the Max Displacement field");
            Assert.IsNotNull(dt.GetField("leader_lines_enabled"), "DisplacementSettings.leader_lines_enabled must exist for the Leader Lines toggle (gates sub-fields)");
            Assert.IsNotNull(dt.GetField("leader_line_style"), "DisplacementSettings.leader_line_style must exist for the Style popup");
            Assert.IsNotNull(dt.GetField("leader_line_min_distance_px"), "DisplacementSettings.leader_line_min_distance_px must exist for the Min Distance field");
            Assert.IsNotNull(dt.GetField("leader_line_width"), "DisplacementSettings.leader_line_width must exist for the Line Width field");
            Assert.IsNotNull(dt.GetField("leader_line_opacity"), "DisplacementSettings.leader_line_opacity must exist for the Line Opacity field");
            Assert.IsNotNull(dt.GetField("displacement_tiebreak"), "DisplacementSettings.displacement_tiebreak must exist for the Tiebreak popup");
        }
    }
}
