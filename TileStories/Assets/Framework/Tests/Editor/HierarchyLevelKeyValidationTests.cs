using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace TileStories.Tests
{
    // Tier-0 tests for POIEditorToolWindow.ValidateHierarchyLevelKeys
    // (spec _2_3 section 11b via _2.7 entry 2.3-j): covers BOTH failure modes --
    // unset key (new warning) and stale key reference (existing behaviour).
    // Pure data validation, no scene or rendering.
    public class HierarchyLevelKeyValidationTests
    {
        private static TileStories.Editor.POIEditorToolWindow CreateWindowWithConfig(TileStories.WallConfigData config)
        {
            var window = EditorWindow.GetWindow<TileStories.Editor.POIEditorToolWindow>();
            var field = typeof(TileStories.Editor.POIEditorToolWindow).GetField("_config",
                BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(window, config);
            return window;
        }

        private static System.Collections.Generic.List<TileStories.Editor.EditorAlertItem> InvokeValidator(
            TileStories.Editor.POIEditorToolWindow window, string methodName)
        {
            var method = typeof(TileStories.Editor.POIEditorToolWindow).GetMethod(methodName,
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(method, $"method {methodName} not found on POIEditorToolWindow");
            return (System.Collections.Generic.List<TileStories.Editor.EditorAlertItem>)method.Invoke(window, null);
        }

        private static TileStories.WallConfigData ConfigWithLevels(params string[] keys)
        {
            var config = new TileStories.WallConfigData();
            if (keys != null)
            {
                config.hierarchy_levels = new System.Collections.Generic.List<TileStories.HierarchyLevelEntry>();
                foreach (var k in keys)
                    config.hierarchy_levels.Add(new TileStories.HierarchyLevelEntry { key = k });
            }
            config.pois = new System.Collections.Generic.List<TileStories.POIData>();
            return config;
        }

        private static void AddPoi(TileStories.WallConfigData config, string id, string hierarchyKey)
        {
            config.pois.Add(new TileStories.POIData { id = id, hierarchy_level_key = hierarchyKey });
        }

        [Test]
        public void NullKey_ProducesNeverAssignedIssue()
        {
            var config = ConfigWithLevels("level_1");
            AddPoi(config, "poi_a", null);
            var issues = InvokeValidator(CreateWindowWithConfig(config), "ValidateHierarchyLevelKeys");

            Assert.AreEqual(1, issues.Count);
            StringAssert.Contains("never assigned", issues[0].problem);
        }

        [Test]
        public void EmptyKey_ProducesNeverAssignedIssue()
        {
            var config = ConfigWithLevels("level_1");
            AddPoi(config, "poi_a", "");
            var issues = InvokeValidator(CreateWindowWithConfig(config), "ValidateHierarchyLevelKeys");

            Assert.AreEqual(1, issues.Count);
            StringAssert.Contains("never assigned", issues[0].problem);
            StringAssert.AreEqualIgnoringCase("<empty>", issues[0].value);
        }

        [Test]
        public void StaleKey_ProducesExistingStaleReferenceText_Unchanged()
        {
            var config = ConfigWithLevels("level_1");
            AddPoi(config, "poi_a", "deleted_level");
            var issues = InvokeValidator(CreateWindowWithConfig(config), "ValidateHierarchyLevelKeys");

            Assert.AreEqual(1, issues.Count);
            StringAssert.Contains("does not match any entry", issues[0].problem);
            StringAssert.AreNotEqualIgnoringCase("<empty>", issues[0].value);
        }

        [Test]
        public void ValidKey_ProducesNoIssues()
        {
            var config = ConfigWithLevels("level_1");
            AddPoi(config, "poi_a", "level_1");
            var issues = InvokeValidator(CreateWindowWithConfig(config), "ValidateHierarchyLevelKeys");

            Assert.AreEqual(0, issues.Count);
        }

        [Test]
        public void EmptyLevelsTable_EmptyKey_ShowsAddRowFixHint()
        {
            // hierarchy_levels left null -> levelKeys empty -> the fixHint must
            // tell the developer to add a row first, not to assign a key.
            var config = ConfigWithLevels(null);
            AddPoi(config, "poi_a", "");
            var issues = InvokeValidator(CreateWindowWithConfig(config), "ValidateHierarchyLevelKeys");

            Assert.AreEqual(1, issues.Count);
            StringAssert.Contains("Add at least one hierarchy level row", issues[0].fixHint);
        }
    }
}
