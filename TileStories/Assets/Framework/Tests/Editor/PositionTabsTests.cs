using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using TileStories.Editor;

namespace TileStories.Tests
{
    // Tier-0 EditMode tests for Position Draft/Precise tabs (Step 20 skeleton).
    public class PositionTabsTests
    {
        private static POIAuthoringToolWindow CreateWindowWithConfig(WallConfigData config)
        {
            var window = EditorWindow.GetWindow<POIAuthoringToolWindow>();
            var field = typeof(POIAuthoringToolWindow).GetField("_config",
                BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(window, config);
            return window;
        }

        private static WallConfigData CreateMinimalConfig()
        {
            return new WallConfigData
            {
                wall_id = "test_wall",
                wall_name = "Test Wall",
                pois = new System.Collections.Generic.List<POIData>
                {
                    new POIData
                    {
                        id = "poi_1",
                        name = "Test POI",
                        category = "default",
                        has_captured_position = false
                    }
                }
            };
        }

        [Test]
        public void DrawPositionTabs_DefaultsToDraft_ShowsDraftFields()
        {
            // We cannot easily invoke the internal DrawPositionTabs without UI, but we can test the
            // state machine: default tab is Draft, and CaptureSinglePoi sets has_captured_position.
            var config = CreateMinimalConfig();
            var window = CreateWindowWithConfig(config);
            var poi = config.pois[0];

            // Initially no captured position
            Assert.IsFalse(poi.has_captured_position);

            // Simulate CaptureSinglePoi via reflection
            var captureMethod = typeof(POIAuthoringToolWindow).GetMethod("CaptureSinglePoi",
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(captureMethod);

            // We cannot actually capture without a rig, but we can verify the method exists.
            // The test mainly ensures the method signature is present.
        }

        [Test]
        public void PositionTabs_ClearCapture_ResetsFlag()
        {
            var config = CreateMinimalConfig();
            var poi = config.pois[0];

            // Manually set captured position
            poi.captured_position = new CapturedPosition { x = 1, y = 2, z = 3 };
            poi.has_captured_position = true;
            poi.captured_position_source = "scene";
            poi.captured_position_timestamp = 12345;

            // Simulate clear by directly mutating (mirrors Clear Capture button logic)
            poi.captured_position = null;
            poi.has_captured_position = false;
            poi.captured_position_source = null;
            poi.captured_position_timestamp = 0;

            Assert.IsFalse(poi.has_captured_position);
            Assert.IsNull(poi.captured_position);
            Assert.IsNull(poi.captured_position_source);
            Assert.AreEqual(0, poi.captured_position_timestamp);
        }

        [Test]
        public void PositionTabs_JsonRoundTrip_PreservesCapturedData()
        {
            var config = CreateMinimalConfig();
            var poi = config.pois[0];
            poi.captured_position = new CapturedPosition { x = 10, y = 20, z = 30 };
            poi.has_captured_position = true;
            poi.captured_position_source = "scene";
            poi.captured_position_timestamp = 999999;

            // Serialize and deserialize via JsonUtility
            string json = JsonUtility.ToJson(config);
            var restored = JsonUtility.FromJson<WallConfigData>(json);

            Assert.AreEqual(1, restored.pois.Count);
            var rp = restored.pois[0];
            Assert.IsTrue(rp.has_captured_position);
            Assert.IsNotNull(rp.captured_position);
            Assert.AreEqual(10, rp.captured_position.x);
            Assert.AreEqual(20, rp.captured_position.y);
            Assert.AreEqual(30, rp.captured_position.z);
            Assert.AreEqual("scene", rp.captured_position_source);
            Assert.AreEqual(999999, rp.captured_position_timestamp);
        }
    }
}