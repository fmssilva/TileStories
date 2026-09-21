using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // Tier 0 EditMode tests for the Orientation foldout (_2.1_Marker_Orientation.md v4):
    // JSON round-trip of the OrientationSettings schema (mirrors SaveConfig via
    // JsonUtility, and the runtime WallConfigLoader deserialize path), the
    // HierarchyLevelEntry.facing_mode_override field, a no-block-present defaults
    // test, and structural presence of the editor foldout wiring on the editor window.
    public class OrientationEditorRoundTripTests
    {
        [Test]
        public void RoundTrip_OrientationSettings_ThroughJsonUtility()
        {
            var config = new WallConfigData { wall_id = "test_wall" };

            // Drive every control-backed field to a non-default value.
            config.orientation_settings.vertical_alignment_mode = "screen_up";
            config.orientation_settings.label_vertical_alignment_mode = "world_up";
            config.orientation_settings.badge_vertical_alignment_mode = "screen_up";
            config.orientation_settings.cluster_vertical_alignment_mode = "world_up";
            config.orientation_settings.up_reference = "custom";
            config.orientation_settings.custom_up_x = 0.1f;
            config.orientation_settings.custom_up_y = 0.8f;
            config.orientation_settings.custom_up_z = 0.3f;
            config.orientation_settings.facing_mode = "yaw_only";
            config.orientation_settings.facing_basis = "camera_position";
            config.orientation_settings.update_mode = "interval";
            config.orientation_settings.update_interval_s = 0.1f;
            config.orientation_settings.camera_delta_deg = 2f;
            config.orientation_settings.edit_mode_preview_enabled = true;

            config.hierarchy_levels.Add(new HierarchyLevelEntry { key = "level_1", facing_mode_override = "wall_fixed" });

            // Mirrors editor SaveConfig + runtime WallConfigLoader.
            string json = JsonUtility.ToJson(config, true);
            var loaded = JsonUtility.FromJson<WallConfigData>(json);

            Assert.IsNotNull(loaded.orientation_settings, "orientation_settings survives round-trip");
            Assert.AreEqual("screen_up", loaded.orientation_settings.vertical_alignment_mode);
            Assert.AreEqual("world_up", loaded.orientation_settings.label_vertical_alignment_mode);
            Assert.AreEqual("screen_up", loaded.orientation_settings.badge_vertical_alignment_mode);
            Assert.AreEqual("world_up", loaded.orientation_settings.cluster_vertical_alignment_mode);
            Assert.AreEqual("custom", loaded.orientation_settings.up_reference);
            Assert.AreEqual(0.1f, loaded.orientation_settings.custom_up_x, 0.001f);
            Assert.AreEqual(0.8f, loaded.orientation_settings.custom_up_y, 0.001f);
            Assert.AreEqual(0.3f, loaded.orientation_settings.custom_up_z, 0.001f);
            Assert.AreEqual("yaw_only", loaded.orientation_settings.facing_mode);
            Assert.AreEqual("camera_position", loaded.orientation_settings.facing_basis);
            Assert.AreEqual("interval", loaded.orientation_settings.update_mode);
            Assert.AreEqual(0.1f, loaded.orientation_settings.update_interval_s, 0.001f);
            Assert.AreEqual(2f, loaded.orientation_settings.camera_delta_deg, 0.001f);
            Assert.AreEqual(true, loaded.orientation_settings.edit_mode_preview_enabled);

            Assert.AreEqual(1, loaded.hierarchy_levels.Count);
            Assert.AreEqual("wall_fixed", loaded.hierarchy_levels[0].facing_mode_override, "facing_mode_override survives round-trip");
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
            Assert.AreEqual("world_up", loaded.orientation_settings.vertical_alignment_mode);
            Assert.AreEqual("inherit", loaded.orientation_settings.label_vertical_alignment_mode);
            Assert.AreEqual("inherit", loaded.orientation_settings.badge_vertical_alignment_mode);
            Assert.AreEqual("inherit", loaded.orientation_settings.cluster_vertical_alignment_mode);
            Assert.AreEqual("world_gravity", loaded.orientation_settings.up_reference);
            Assert.AreEqual("always_facing_camera", loaded.orientation_settings.facing_mode);
            Assert.AreEqual("view_plane", loaded.orientation_settings.facing_basis);
            Assert.AreEqual("every_frame", loaded.orientation_settings.update_mode);
            Assert.AreEqual(true, loaded.orientation_settings.edit_mode_preview_enabled, "Scene-Mode Preview defaults to ON");
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
            Assert.IsNotNull(
                t.GetMethod("DrawOrientationVerticalAlignmentSubSection", BindingFlags.NonPublic | BindingFlags.Instance),
                "Vertical Alignment sub-section must be wired");
            Assert.IsNotNull(
                t.GetMethod("DrawOrientationFacingOptionsSubSection", BindingFlags.NonPublic | BindingFlags.Instance),
                "Facing Options sub-section must be wired");
            Assert.IsNotNull(
                t.GetMethod("DrawOrientationUpdateCostSubSection", BindingFlags.NonPublic | BindingFlags.Instance),
                "Update Cost sub-section must be wired");
            Assert.IsNotNull(
                t.GetMethod("DrawOrientationTestSubSection", BindingFlags.NonPublic | BindingFlags.Instance),
                "Test sub-section must be wired");

            var ot = typeof(OrientationSettings);
            Assert.IsNotNull(ot.GetField("vertical_alignment_mode"), "OrientationSettings.vertical_alignment_mode must exist for the Marker vertical-alignment popup");
            Assert.IsNotNull(ot.GetField("label_vertical_alignment_mode"), "OrientationSettings.label_vertical_alignment_mode must exist for the Label popup");
            Assert.IsNotNull(ot.GetField("badge_vertical_alignment_mode"), "OrientationSettings.badge_vertical_alignment_mode must exist for the Badge popup");
            Assert.IsNotNull(ot.GetField("cluster_vertical_alignment_mode"), "OrientationSettings.cluster_vertical_alignment_mode must exist for the Clusters popup");
            Assert.IsNotNull(ot.GetField("up_reference"), "OrientationSettings.up_reference must exist for the Up Reference popup");
            Assert.IsNotNull(ot.GetField("custom_up_x"), "OrientationSettings.custom_up_x must exist for the Custom Up X field");
            Assert.IsNotNull(ot.GetField("facing_mode"), "OrientationSettings.facing_mode must exist for the Facing popup");
            Assert.IsNotNull(ot.GetField("facing_basis"), "OrientationSettings.facing_basis must exist for the Facing Basis popup");
            Assert.IsNotNull(ot.GetField("update_mode"), "OrientationSettings.update_mode must exist for the Update Mode popup");
            Assert.IsNotNull(ot.GetField("update_interval_s"), "OrientationSettings.update_interval_s must exist for the Update Interval field");
            Assert.IsNotNull(ot.GetField("camera_delta_deg"), "OrientationSettings.camera_delta_deg must exist for the Camera Delta field");
            Assert.IsNotNull(ot.GetField("edit_mode_preview_enabled"), "OrientationSettings.edit_mode_preview_enabled must exist for the Scene-Mode Preview toggle");

            // v4 deletions: these fields must NOT exist (dead-code removal, not a rename miss).
            Assert.IsNull(ot.GetField("roll_snap_mode"), "roll_snap_mode was deliberately removed in v4");
            Assert.IsNull(ot.GetField("clamp_pitch_enabled"), "clamp_pitch_enabled was deliberately removed in v4 (hardcoded instead)");
            Assert.IsNull(ot.GetField("badge_corner_mode"), "badge_corner_mode was deliberately removed in v4");
            Assert.IsNull(ot.GetField("rotation_smoothing_time_s"), "rotation_smoothing_time_s was deliberately removed in v4 (hardcoded instead)");

            Assert.IsNotNull(typeof(HierarchyLevelEntry).GetField("facing_mode_override"),
                "HierarchyLevelEntry.facing_mode_override must exist for the Hierarchy Levels table's Facing column");
            Assert.IsNull(typeof(HierarchyLevelEntry).GetField("orientation_mode_override"),
                "orientation_mode_override was renamed to facing_mode_override in v4");
        }

        // The three Test guides are the developer's only test script, so they must not drift
        // from the real fields: each one has to exist, be ASCII, have one block per Orientation
        // sub-section, name every Facing option label the popups really offer (read from the same
        // arrays the popups use), and start collapsed.
        [Test]
        public void TestGuides_ExistAsciiCoverEverySubSection_AndStartCollapsed()
        {
            var t = typeof(POIEditorToolWindow);
            const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Static;

            var facingTerms = new System.Collections.Generic.List<string> { "Hierarchy Levels" };
            foreach (string arrayName in new[] { "FacingModeLabels", "FacingBasisLabels" })
            {
                var labels = (string[])t.GetField(arrayName, flags)?.GetValue(null);
                Assert.IsNotNull(labels, arrayName + " must exist on the editor window");
                facingTerms.AddRange(labels);
            }
            var verticalLabels = (string[])t.GetField("VerticalAlignmentModeLabels", flags)?.GetValue(null);
            Assert.IsNotNull(verticalLabels, "VerticalAlignmentModeLabels must exist on the editor window");

            foreach (string guideName in new[] { "OrientationSceneTestGuide", "OrientationPlaymodeTestGuide", "OrientationDeviceTestGuide" })
            {
                var guide = (string)t.GetField(guideName, flags)?.GetValue(null);
                Assert.IsFalse(string.IsNullOrWhiteSpace(guide), guideName + " must exist and not be empty");
                foreach (char c in guide)
                    Assert.IsTrue(c == '\n' || (c >= ' ' && c <= '~'), guideName + " must be ASCII only, found char code " + (int)c);

                // One block per Orientation sub-section, not per field.
                StringAssert.Contains("VERTICAL ALIGNMENT", guide, guideName);
                StringAssert.Contains("FACING OPTIONS", guide, guideName);
                StringAssert.Contains("UPDATE COST", guide, guideName);
                foreach (string term in facingTerms)
                    StringAssert.Contains(term, guide, guideName + " must mention '" + term + "'");

                // Play and Device really test vertical alignment; Scene must say it cannot.
                if (guideName == "OrientationSceneTestGuide")
                {
                    StringAssert.Contains("Not possible in Scene test", guide, guideName);
                    StringAssert.Contains("Scene-Mode Preview", guide, guideName);
                }
                else
                {
                    foreach (string label in verticalLabels)
                        StringAssert.Contains(label, guide, guideName + " must mention '" + label + "'");
                }
            }

            var window = ScriptableObject.CreateInstance<POIEditorToolWindow>();
            try
            {
                foreach (string foldoutField in new[] { "_showOrientationSceneTestGuide", "_showOrientationPlaymodeTestGuide", "_showOrientationDeviceTestGuide" })
                {
                    var f = t.GetField(foldoutField, BindingFlags.NonPublic | BindingFlags.Instance);
                    Assert.IsNotNull(f, foldoutField + " must exist");
                    Assert.IsFalse((bool)f.GetValue(window), foldoutField + " must default to collapsed");
                }
            }
            finally { UnityEngine.Object.DestroyImmediate(window); }
        }
    }
}
