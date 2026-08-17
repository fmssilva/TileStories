using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // Tier 0 tests for the Block 2 authoring foldouts: JSON round-trip of the
    // LodSettings schema (mirrors SaveConfig via JsonUtility, and the runtime
    // WallConfigLoader deserialize path) plus structural presence of the new
    // foldout wiring.

    public class LodAuthoringRoundTripTests
    {
        [Test]
        public void RoundTrip_LodSettings_ThroughJsonUtility()
        {
            var config = new WallConfigData { wall_id = "test_wall" };
            config.lod_settings = LodAutoSuggest.Suggest(18);
            config.lod_settings.density_response_mode = "cluster";
            config.lod_settings.density_safety_escalation_multiplier = 3f;
            config.lod_settings.zoom_max = 5f;

            // 3-7a: set the three new LodSettings fields that the LOD authoring
            // foldout binds to, so the round-trip asserts below cover them.
            config.lod_settings.bands[2].details = "far sentinel";
            config.lod_settings.cluster_band_source = "nearest_member";
            config.lod_settings.cluster_band_hysteresis_enabled = false;
            config.lod_settings.cluster_dissolve_grace_cycles = 7;

            // Mirrors authoring SaveConfig + runtime WallConfigLoader.
            string json = JsonUtility.ToJson(config, true);
            var loaded = JsonUtility.FromJson<WallConfigData>(json);

            Assert.IsNotNull(loaded.lod_settings, "lod_settings survives round-trip");
            Assert.AreEqual(config.lod_settings.bands.Count, loaded.lod_settings.bands.Count);
            Assert.AreEqual(7f, loaded.lod_settings.bands[1].max_distance_m);
            Assert.AreEqual(15, loaded.lod_settings.bands[1].max_visible_count);
            Assert.AreEqual("cluster", loaded.lod_settings.density_response_mode);
            Assert.AreEqual(3f, loaded.lod_settings.density_safety_escalation_multiplier);
            Assert.AreEqual(5f, loaded.lod_settings.zoom_max);
            Assert.AreEqual("far sentinel", loaded.lod_settings.bands[2].details, "additive band details survives");

            // 3-7b: Asserts the three new LodSettings fields survive the
            // JsonUtility round-trip (set above, read back here).
            Assert.AreEqual("nearest_member", loaded.lod_settings.cluster_band_source, "cluster band source survives round-trip");
            Assert.AreEqual(false, loaded.lod_settings.cluster_band_hysteresis_enabled, "cluster hysteresis toggle survives round-trip");
            Assert.AreEqual(7, loaded.lod_settings.cluster_dissolve_grace_cycles, "cluster dissolve grace cycles survives round-trip");

            Assert.AreEqual(config.lod_settings.cluster_min_count, loaded.lod_settings.cluster_min_count);
            Assert.AreEqual(config.lod_settings.shrink_start_neighbor_count, loaded.lod_settings.shrink_start_neighbor_count);
        }

        [Test]
        public void FoldoutSectionMethods_AndState_Exist()
        {
            var t = typeof(POIAuthoringToolWindow);
            Assert.IsNotNull(
                t.GetMethod("DrawGlobalLodSection", BindingFlags.NonPublic | BindingFlags.Instance),
                "DrawGlobalLodSection must be wired on the authoring window");
            Assert.IsNotNull(
                t.GetMethod("DrawGlobalZoomSection", BindingFlags.NonPublic | BindingFlags.Instance),
                "DrawGlobalZoomSection must be wired on the authoring window");
            Assert.IsNotNull(
                t.GetField("_showGlobalLod", BindingFlags.NonPublic | BindingFlags.Instance),
                "_showGlobalLod state field must exist");
            Assert.IsNotNull(
                t.GetField("_showGlobalZoom", BindingFlags.NonPublic | BindingFlags.Instance),
                "_showGlobalZoom state field must exist");

            // 3-7b: the three new LodSettings fields the LOD foldout binds to must exist.
            Assert.IsNotNull(typeof(LodSettings).GetField("cluster_band_source"), "LodSettings.cluster_band_source must exist for the Band Source foldout field");
            Assert.IsNotNull(typeof(LodSettings).GetField("cluster_band_hysteresis_enabled"), "LodSettings.cluster_band_hysteresis_enabled must exist for the Band Hysteresis toggle");
            Assert.IsNotNull(typeof(LodSettings).GetField("cluster_dissolve_grace_cycles"), "LodSettings.cluster_dissolve_grace_cycles must exist for the Dissolve Grace field");
        }
    }
}
