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
        public void NullKey_ProducesNoHierarchyLevelAssignedIssue()
        {
            var config = ConfigWithLevels("level_1");
            AddPoi(config, "poi_a", null);
            var issues = InvokeValidator(CreateWindowWithConfig(config), "ValidateHierarchyLevelKeys");

            Assert.AreEqual(1, issues.Count);
            StringAssert.Contains("No Hierarchy Level is assigned", issues[0].problem);
        }

        [Test]
        public void EmptyKey_ProducesNoHierarchyLevelAssignedIssue()
        {
            var config = ConfigWithLevels("level_1");
            AddPoi(config, "poi_a", "");
            var issues = InvokeValidator(CreateWindowWithConfig(config), "ValidateHierarchyLevelKeys");

            Assert.AreEqual(1, issues.Count);
            StringAssert.Contains("No Hierarchy Level is assigned", issues[0].problem);
            StringAssert.AreEqualIgnoringCase("<empty>", issues[0].value);
        }

        // Regression (P5, developer feedback on a confusing dialog): the problem/fix text must lead
        // with the UI-facing name ("Hierarchy Level", the dropdown label) and say which tab it is
        // in, not just the raw JSON field name -- a developer reading the popup should not need to
        // already know the config schema to act on it.
        [Test]
        public void EmptyKey_FixHint_NamesTheRealTabAndDropdown()
        {
            var config = ConfigWithLevels("level_1");
            AddPoi(config, "poi_a", "");
            var issues = InvokeValidator(CreateWindowWithConfig(config), "ValidateHierarchyLevelKeys");

            StringAssert.Contains("Specific Marker tab", issues[0].fixHint);
            StringAssert.Contains("Hierarchy Level dropdown", issues[0].fixHint);
        }

        [Test]
        public void StaleKey_ProducesStaleReferenceIssue()
        {
            var config = ConfigWithLevels("level_1");
            AddPoi(config, "poi_a", "deleted_level");
            var issues = InvokeValidator(CreateWindowWithConfig(config), "ValidateHierarchyLevelKeys");

            Assert.AreEqual(1, issues.Count);
            StringAssert.Contains("no longer matches any row", issues[0].problem);
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
            StringAssert.Contains("Global Scene > Hierarchy Levels", issues[0].fixHint);
            StringAssert.Contains("add at least one row", issues[0].fixHint);
        }
    }
}
