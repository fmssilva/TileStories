using System.Reflection;
using NUnit.Framework;
using UnityEditor;

namespace TileStories.Tests
{
    // Tier-0 tests for POIEditorToolWindow.ValidateDensityThresholds
    // (spec _2_4 section 6 via _2.7 entry 2.4-d): the shrink/cluster threshold
    // ordering rule, wired through the existing LODController.IsDensityConfigValid
    // detector. Pure data validation, no scene or rendering.
    public class DensityThresholdValidationTests
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
            TileStories.Editor.POIEditorToolWindow window)
        {
            var method = typeof(TileStories.Editor.POIEditorToolWindow).GetMethod("ValidateDensityThresholds",
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(method, "ValidateDensityThresholds not found on POIEditorToolWindow");
            return (System.Collections.Generic.List<TileStories.Editor.EditorAlertItem>)method.Invoke(window, null);
        }

        private static TileStories.WallConfigData ConfigWithLod(int shrinkStart, int clusterMin)
        {
            var config = new TileStories.WallConfigData();
            config.lod_settings = new TileStories.LodSettings
            {
                shrink_start_neighbor_count = shrinkStart,
                cluster_min_count = clusterMin
            };
            return config;
        }

        [Test]
        public void ShrinkBelowCluster_ProducesNoIssues()
        {
            var issues = InvokeValidator(CreateWindowWithConfig(ConfigWithLod(3, 5)));
            Assert.AreEqual(0, issues.Count);
        }

        [Test]
        public void ShrinkEqualToCluster_ProducesOneIssue()
        {
            var issues = InvokeValidator(CreateWindowWithConfig(ConfigWithLod(5, 5)));
            Assert.AreEqual(1, issues.Count);
            StringAssert.Contains("strictly less than", issues[0].problem);
        }

        [Test]
        public void ShrinkAboveCluster_ProducesOneIssue()
        {
            var issues = InvokeValidator(CreateWindowWithConfig(ConfigWithLod(6, 5)));
            Assert.AreEqual(1, issues.Count);
            StringAssert.Contains("Suggest Values", issues[0].fixHint);
        }

        [Test]
        public void NullLodSettings_ProducesNoIssues()
        {
            var config = new TileStories.WallConfigData();
            config.lod_settings = null; // guard clause must not throw
            var issues = InvokeValidator(CreateWindowWithConfig(config));
            Assert.AreEqual(0, issues.Count);
        }
    }
}
