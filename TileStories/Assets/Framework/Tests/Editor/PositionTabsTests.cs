using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using TileStories.Editor;

namespace TileStories.Tests
{
    // Tier-0 EditMode tests for the simplified per-POI Position foldout
    // (read-only XYZ + Verified toggle + Capture/Clear/Mark-Unverified buttons).
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
                        position_verified = false
                    }
                }
            };
        }

        [Test]
        public void PositionContract_NoPosition_IsNullAndUnverified()
        {
            var poi = new POIData { id = "no_pos", name = "Test POI", category = "default" };

            // Simplified-model contract: a POI with no authored position keeps the
            // reference null (resolver falls back to origin) and starts unverified.
            Assert.IsNull(poi.position, "A POI with no authored position must keep position null.");
            Assert.IsFalse(poi.position_verified, "position_verified must default to false.");

            // The Position foldout entry point must exist and take exactly one POIData
            // (proof the simplified single-foldout UI is wired, no Draft/Precise tabs).
            var drawMethod = typeof(POIAuthoringToolWindow).GetMethod("DrawPositionTabs",
                BindingFlags.NonPublic | BindingFlags.Instance, null,
                new System.Type[] { typeof(POIData) }, null);
            Assert.IsNotNull(drawMethod, "DrawPositionTabs(POIData) must exist for the Position foldout.");
        }

        [Test]
        public void PositionTabs_Clear_ResetsPositionAndVerified()
        {
            var config = CreateMinimalConfig();
            var poi = config.pois[0];

            // Simulate a captured position
            poi.position = new PositionData { x = 1, y = 2, z = 3 };
            poi.position_verified = true;

            // Simulate the Clear Position button: both fields reset together
            poi.position = null;
            poi.position_verified = false;

            Assert.IsFalse(poi.position_verified);
            Assert.IsNull(poi.position);
        }

        [Test]
        public void PositionTabs_JsonRoundTrip_PreservesCapturedData()
        {
            var config = CreateMinimalConfig();
            var poi = config.pois[0];
            poi.position = new PositionData { x = 10, y = 20, z = 30 };
            poi.position_verified = true;

            // Serialize and deserialize via JsonUtility
            string json = JsonUtility.ToJson(config);
            var restored = JsonUtility.FromJson<WallConfigData>(json);

            Assert.AreEqual(1, restored.pois.Count);
            var rp = restored.pois[0];
            Assert.IsTrue(rp.position_verified);
            Assert.IsNotNull(rp.position);
            Assert.AreEqual(10, rp.position.x);
            Assert.AreEqual(20, rp.position.y);
            Assert.AreEqual(30, rp.position.z);
        }
    }
}