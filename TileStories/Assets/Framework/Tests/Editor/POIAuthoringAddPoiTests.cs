using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using TileStories;
using TileStories.Editor;

namespace TileStories.Tests
{
    // Tier-0 EditMode tests for the POI header rename (pencil button).
    public class POIHeaderRenameTests
    {
        [Test]
        public void StripNumberPrefix_NumberedDisplayName_ReturnsBareName()
        {
            Assert.AreEqual("Lamp", POIAuthoringToolWindow.StripNumberPrefix("3. Lamp"));
        }

        [Test]
        public void StripNumberPrefix_UnnumberedName_ReturnsTrimmed()
        {
            Assert.AreEqual("Lamp", POIAuthoringToolWindow.StripNumberPrefix("  Lamp  "));
        }

        [Test]
        public void StripNumberPrefix_NullOrBlank_ReturnsEmpty()
        {
            Assert.AreEqual("", POIAuthoringToolWindow.StripNumberPrefix(null));
            Assert.AreEqual("", POIAuthoringToolWindow.StripNumberPrefix("   "));
        }

        [Test]
        public void StripNumberPrefix_DotWithoutSpace_KeepsWholeString()
        {
            // Only ". " (dot+space) is the numbering separator; "v1.2" is a name.
            Assert.AreEqual("v1.2 Wall", POIAuthoringToolWindow.StripNumberPrefix("v1.2 Wall"));
        }

        [Test]
        public void SaveNameChange_NumberedInput_StripsPrefixAndWritesPoiName()
        {
            var config = new WallConfigData
            {
                wall_id = "test_wall",
                pois = new System.Collections.Generic.List<POIData>
                {
                    new POIData { id = "poi_1", name = "Old Name", category = "default" }
                }
            };
            var window = EditorWindow.GetWindow<POIAuthoringToolWindow>();
            typeof(POIAuthoringToolWindow).GetField("_config", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(window, config);
            var save = typeof(POIAuthoringToolWindow).GetMethod("SaveNameChange",
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(save, "SaveNameChange must exist");

            save.Invoke(window, new object[] { config.pois[0], "poi_1", "editName_poi_1", "editValue_poi_1", "1. New Name" });

            Assert.AreEqual("New Name", config.pois[0].name);
        }

        [Test]
        public void SaveNameChange_BlankInput_KeepsOldName()
        {
            var config = new WallConfigData
            {
                wall_id = "test_wall",
                pois = new System.Collections.Generic.List<POIData>
                {
                    new POIData { id = "poi_1", name = "Old Name", category = "default" }
                }
            };
            var window = EditorWindow.GetWindow<POIAuthoringToolWindow>();
            typeof(POIAuthoringToolWindow).GetField("_config", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(window, config);
            var save = typeof(POIAuthoringToolWindow).GetMethod("SaveNameChange",
                BindingFlags.NonPublic | BindingFlags.Instance);

            save.Invoke(window, new object[] { config.pois[0], "poi_1", "editName_poi_1", "editValue_poi_1", "   " });

            Assert.AreEqual("Old Name", config.pois[0].name, "Blank rename must not wipe the name.");
        }

        [Test]
        public void EditIconAssetPath_PointsInsideFrameworkEditor()
        {
            StringAssert.StartsWith("Assets/Framework/Editor/",
                POIAuthoringToolWindow.EditIconAssetPath,
                "Editor-only icons must live under Framework/Editor per 10-structure.");
        }
    }
}

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
                BindingFlags.NonPublic | BindingFlags.Instance, null, System.Type.EmptyTypes, null);
            Assert.IsNotNull(method, "AddNewPoi method not found");
            method.Invoke(window, null);
        }

        // AddNewPoi spawns a real global POIAuthoringRig GameObject (via
        // SpawnAndFocusNewPoi) that is never auto-destroyed. Without this teardown
        // it leaks across tests in the same editor, and GameObject.Find in the
        // unrelated sync-check tests finds the stale rig -> they fail with phantom
        // out-of-sync children. Destroy it so every test starts clean.
        [TearDown]
        public void Teardown()
        {
            var rig = GameObject.Find("POIAuthoringRig");
            if (rig != null)
                Object.DestroyImmediate(rig);
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
                        Assert.AreEqual(0f, newPoi.editor_rotation_deg, 0.0001f);
            Assert.IsFalse(newPoi.position_verified);
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
