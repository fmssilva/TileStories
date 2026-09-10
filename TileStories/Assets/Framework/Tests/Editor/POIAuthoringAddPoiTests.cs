using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using TileStories.Editor;

namespace TileStories.Tests
{
    // Tier-0 EditMode tests for the "Add POI" button (Step 19).
    public class POIAuthoringAddPoiTests
    {
        private static POIAuthoringToolWindow CreateWindowWithConfig(WallConfigData config)
        {
            var window = EditorWindow.GetWindow<POIAuthoringToolWindow>();
            var field = typeof(POIAuthoringToolWindow).GetField("_config",
                BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(window, config);
            return window;
        }

        private static void InvokeAddNewPoi(POIAuthoringToolWindow window)
        {
            var method = typeof(POIAuthoringToolWindow).GetMethod("AddNewPoi",
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(method, "AddNewPoi method not found");
            method.Invoke(window, null);
        }

        private static WallConfigData CreateMinimalConfig()
        {
            var config = new WallConfigData
            {
                wall_id = "test_wall",
                wall_name = "Test Wall",
                pois = new System.Collections.Generic.List<POIData>()
            };
            return config;
        }

        [Test]
        public void AddNewPoi_IncrementsListAndSetsDefaults()
        {
            var config = CreateMinimalConfig();
            var window = CreateWindowWithConfig(config);

            int beforeCount = config.pois.Count;
            InvokeAddNewPoi(window);
            int afterCount = config.pois.Count;

            Assert.AreEqual(beforeCount + 1, afterCount, "POI list should increment by one");

            var newPoi = config.pois[config.pois.Count - 1];
            Assert.IsFalse(string.IsNullOrEmpty(newPoi.id), "New POI should have a GUID id");
            Assert.AreEqual("New POI", newPoi.name);
            Assert.AreEqual("default", newPoi.category);
            Assert.AreEqual(0.5f, newPoi.x_norm, 0.0001f);
            Assert.AreEqual(0.5f, newPoi.y_norm, 0.0001f);
            Assert.IsFalse(newPoi.has_captured_position);
            Assert.AreEqual(0f, newPoi.status_pct);
            Assert.IsFalse(newPoi.has_status);
            Assert.IsFalse(newPoi.status_unknown);
            Assert.IsNull(newPoi.hierarchy_level_key);
            Assert.IsFalse(newPoi.has_custom_symbol);
            Assert.IsNull(newPoi.custom_symbol_key);
            Assert.IsNull(newPoi.badge_category);
            Assert.IsNotNull(newPoi.search_keywords);
            Assert.IsNotNull(newPoi.search_keyword_fields);
        }

        [Test]
        public void AddNewPoi_GeneratesUniqueIds()
        {
            var config = CreateMinimalConfig();
            var window = CreateWindowWithConfig(config);

            InvokeAddNewPoi(window);
            var id1 = config.pois[0].id;

            InvokeAddNewPoi(window);
            var id2 = config.pois[1].id;

            Assert.AreNotEqual(id1, id2, "Each POI should receive a unique GUID");
        }
    }
}