using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // Tier 0 EditMode tests for the Block 6 editor foldout (_2.1_Marker_Orientation.md):
    // JSON round-trip of the OrientationSettings schema (mirrors SaveConfig via
    // JsonUtility, and the runtime WallConfigLoader deserialize path), the
    // HierarchyLevelEntry.orientation_mode_override field, a no-block-present defaults
    // test, and structural presence of the new foldout wiring on the editor window.
    public class OrientationEditorRoundTripTests
    {
        [Test]
        public void RoundTrip_OrientationSettings_ThroughJsonUtility()
        {
            var config = new WallConfigData { wall_id = "test_wall" };

            // Drive every control-backed field to a non-default value.
            config.orientation_settings.marker_orientation_mode = "world_up";
            config.orientation_settings.facing_basis = "camera_position";
            config.orientation_settings.up_reference = "custom";
            config.orientation_settings.custom_up_x = 0.1f;
            config.orientation_settings.custom_up_y = 0.8f;
            config.orientation_settings.custom_up_z = 0.3f;
            config.orientation_settings.roll_snap_mode = "quarter_turns";
            config.orientation_settings.roll_snap_hysteresis_deg = 20f;
            config.orientation_settings.clamp_pitch_enabled = true;
            config.orientation_settings.max_pitch_deg = 60f;
            config.orientation_settings.rotation_smoothing_time_s = 0.15f;
            config.orientation_settings.label_orientation_mode = "world_up";
            config.orientation_settings.badge_orientation_mode = "screen_up";
            config.orientation_settings.badge_corner_mode = "screen_fixed";
            config.orientation_settings.cluster_orientation_mode = "yaw_only";
            config.orientation_settings.update_mode = "interval";
            config.orientation_settings.update_interval_s = 0.1f;
            config.orientation_settings.camera_delta_deg = 2f;
            config.orientation_settings.edit_mode_preview_enabled = true;

            config.hierarchy_levels.Add(new HierarchyLevelEntry { key = "level_1", orientation_mode_override = "wall_fixed" });

            // Mirrors editor SaveConfig + runtime WallConfigLoader.
            string json = JsonUtility.ToJson(config, true);
            var loaded = JsonUtility.FromJson<WallConfigData>(json);

            Assert.IsNotNull(loaded.orientation_settings, "orientation_settings survives round-trip");
            Assert.AreEqual("world_up", loaded.orientation_settings.marker_orientation_mode);
            Assert.AreEqual("camera_position", loaded.orientation_settings.facing_basis);
            Assert.AreEqual("custom", loaded.orientation_settings.up_reference);
            Assert.AreEqual(0.1f, loaded.orientation_settings.custom_up_x, 0.001f);
            Assert.AreEqual(0.8f, loaded.orientation_settings.custom_up_y, 0.001f);
            Assert.AreEqual(0.3f, loaded.orientation_settings.custom_up_z, 0.001f);
            Assert.AreEqual("quarter_turns", loaded.orientation_settings.roll_snap_mode);
            Assert.AreEqual(20f, loaded.orientation_settings.roll_snap_hysteresis_deg, 0.001f);
            Assert.AreEqual(true, loaded.orientation_settings.clamp_pitch_enabled);
            Assert.AreEqual(60f, loaded.orientation_settings.max_pitch_deg, 0.001f);
            Assert.AreEqual(0.15f, loaded.orientation_settings.rotation_smoothing_time_s, 0.001f);
            Assert.AreEqual("world_up", loaded.orientation_settings.label_orientation_mode);
            Assert.AreEqual("screen_up", loaded.orientation_settings.badge_orientation_mode);
            Assert.AreEqual("screen_fixed", loaded.orientation_settings.badge_corner_mode);
            Assert.AreEqual("yaw_only", loaded.orientation_settings.cluster_orientation_mode);
            Assert.AreEqual("interval", loaded.orientation_settings.update_mode);
            Assert.AreEqual(0.1f, loaded.orientation_settings.update_interval_s, 0.001f);
            Assert.AreEqual(2f, loaded.orientation_settings.camera_delta_deg, 0.001f);
            Assert.AreEqual(true, loaded.orientation_settings.edit_mode_preview_enabled);

            Assert.AreEqual(1, loaded.hierarchy_levels.Count);
            Assert.AreEqual("wall_fixed", loaded.hierarchy_levels[0].orientation_mode_override, "orientation_mode_override survives round-trip");
        }

        [Test]
        public void NoOrientationBlock_DeserializesToDocumentedDefaults()
        {
            // A config authored before this domain existed has no orientation_settings
            // block in its JSON at all -- JsonUtility must still produce the documented
            // defaults (backward compatibility by default values, not a migration step).
            string json = "{\"wall_id\":\"legacy_wall\"}";
            var loaded = JsonUtility.FromJson<WallConfigData>(json);

            Assert.IsNotNull(loaded.orientation_settings);
            Assert.AreEqual("screen_aligned", loaded.orientation_settings.marker_orientation_mode);
            Assert.AreEqual("view_plane", loaded.orientation_settings.facing_basis);
            Assert.AreEqual("world_gravity", loaded.orientation_settings.up_reference);
            Assert.AreEqual("none", loaded.orientation_settings.roll_snap_mode);
            Assert.AreEqual(15f, loaded.orientation_settings.roll_snap_hysteresis_deg, 0.001f);
            Assert.AreEqual(false, loaded.orientation_settings.clamp_pitch_enabled);
            Assert.AreEqual(75f, loaded.orientation_settings.max_pitch_deg, 0.001f);
            Assert.AreEqual(0f, loaded.orientation_settings.rotation_smoothing_time_s, 0.001f);
            Assert.AreEqual("inherit", loaded.orientation_settings.label_orientation_mode);
            Assert.AreEqual("inherit", loaded.orientation_settings.badge_orientation_mode);
            Assert.AreEqual("inherit", loaded.orientation_settings.badge_corner_mode);
            Assert.AreEqual("inherit", loaded.orientation_settings.cluster_orientation_mode);
            Assert.AreEqual("every_frame", loaded.orientation_settings.update_mode);
            Assert.AreEqual(false, loaded.orientation_settings.edit_mode_preview_enabled);
        }

        [Test]
        public void FoldoutSectionMethods_AndState_Exist()
        {
            var t = typeof(POIEditorToolWindow);
            Assert.IsNotNull(
                t.GetMethod("DrawGlobalOrientationSection", BindingFlags.NonPublic | BindingFlags.Instance),
                "DrawGlobalOrientationSection must be wired on the editor window");
            Assert.IsNotNull(
                t.GetField("_showGlobalOrientation", BindingFlags.NonPublic | BindingFlags.Instance),
                "_showGlobalOrientation state field must exist");

            var ot = typeof(OrientationSettings);
            Assert.IsNotNull(ot.GetField("marker_orientation_mode"), "OrientationSettings.marker_orientation_mode must exist for the Marker Orientation popup");
            Assert.IsNotNull(ot.GetField("facing_basis"), "OrientationSettings.facing_basis must exist for the Facing Basis popup");
            Assert.IsNotNull(ot.GetField("up_reference"), "OrientationSettings.up_reference must exist for the Up Reference popup");
            Assert.IsNotNull(ot.GetField("custom_up_x"), "OrientationSettings.custom_up_x must exist for the Custom Up X field");
            Assert.IsNotNull(ot.GetField("roll_snap_mode"), "OrientationSettings.roll_snap_mode must exist for the Roll Snap popup");
            Assert.IsNotNull(ot.GetField("roll_snap_hysteresis_deg"), "OrientationSettings.roll_snap_hysteresis_deg must exist for the Snap Hysteresis field");
            Assert.IsNotNull(ot.GetField("clamp_pitch_enabled"), "OrientationSettings.clamp_pitch_enabled must exist for the Clamp Pitch toggle");
            Assert.IsNotNull(ot.GetField("max_pitch_deg"), "OrientationSettings.max_pitch_deg must exist for the Max Pitch field");
            Assert.IsNotNull(ot.GetField("rotation_smoothing_time_s"), "OrientationSettings.rotation_smoothing_time_s must exist for the Rotation Smoothing field");
            Assert.IsNotNull(ot.GetField("label_orientation_mode"), "OrientationSettings.label_orientation_mode must exist for the Label Orientation popup");
            Assert.IsNotNull(ot.GetField("badge_orientation_mode"), "OrientationSettings.badge_orientation_mode must exist for the Badge Orientation popup");
            Assert.IsNotNull(ot.GetField("badge_corner_mode"), "OrientationSettings.badge_corner_mode must exist for the Badge Corner popup");
            Assert.IsNotNull(ot.GetField("cluster_orientation_mode"), "OrientationSettings.cluster_orientation_mode must exist for the Cluster Orientation popup");
            Assert.IsNotNull(ot.GetField("update_mode"), "OrientationSettings.update_mode must exist for the Update Mode popup");
            Assert.IsNotNull(ot.GetField("update_interval_s"), "OrientationSettings.update_interval_s must exist for the Update Interval field");
            Assert.IsNotNull(ot.GetField("camera_delta_deg"), "OrientationSettings.camera_delta_deg must exist for the Camera Delta field");
            Assert.IsNotNull(ot.GetField("edit_mode_preview_enabled"), "OrientationSettings.edit_mode_preview_enabled must exist for the Edit-Mode Preview toggle");

            Assert.IsNotNull(typeof(HierarchyLevelEntry).GetField("orientation_mode_override"),
                "HierarchyLevelEntry.orientation_mode_override must exist for the Hierarchy Levels table's Orientation column");
        }
    }
}
