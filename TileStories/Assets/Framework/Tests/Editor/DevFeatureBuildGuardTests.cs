using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // The build guard for developer-only switches (_5.1 "Dev-only features and build safety"): which
    // switches ON in a config would really change the shipped app, per kind of build. The modal
    // "Build anyway?" dialog is not clicked by tests; its decision logic is what is proven here.
    public class DevFeatureBuildGuardTests
    {
        private static WallConfigData ShippedConfig()
        {
            var path = Path.Combine(Application.streamingAssetsPath, "LivingRoom/config.json");
            var config = JsonUtility.FromJson<WallConfigData>(File.ReadAllText(path));
            Assert.IsNotNull(config, "The shipped LivingRoom config must load.");
            return config;
        }

        [Test]
        public void PreviewOn_DevelopmentBuild_IsReported_WithTheHowToDisableText()
        {
            var config = ShippedConfig();
            config.effect_defaults.preview.enabled = true;

            var messages = DevFeatureBuildGuard.ActiveMessages(config, developmentBuild: true);

            Assert.AreEqual(1, messages.Count);
            StringAssert.Contains("Add effects demo grid", messages[0]);
            StringAssert.Contains("untick", messages[0]);
            StringAssert.Contains("Copy to StreamingAssets", messages[0]);
        }

        [Test]
        public void PreviewOn_ReleaseBuild_IsNotReported_BecauseTheRuntimeIgnoresIt()
        {
            var config = ShippedConfig();
            config.effect_defaults.preview.enabled = true;

            Assert.IsEmpty(DevFeatureBuildGuard.ActiveMessages(config, developmentBuild: false));
        }

        [Test]
        public void PreviewOff_NothingIsReported_InAnyBuild()
        {
            var config = ShippedConfig();
            config.effect_defaults.preview.enabled = false;

            Assert.IsEmpty(DevFeatureBuildGuard.ActiveMessages(config, developmentBuild: true));
            Assert.IsEmpty(DevFeatureBuildGuard.ActiveMessages(config, developmentBuild: false));
        }

        [Test]
        public void NullConfigOrBlocks_AreSafe()
        {
            Assert.IsEmpty(DevFeatureBuildGuard.ActiveMessages(null, true));
            var bare = new WallConfigData { effect_defaults = null };
            Assert.IsEmpty(DevFeatureBuildGuard.ActiveMessages(bare, true));
        }

        [Test]
        public void ReleaseFlag_MatchesWhatTheRuntimeReallyDoes()
        {
            // The guard's "ignored in release builds" claim is a claim about EffectsPreviewSpawner: assert it.
            var effectsSwitch = DevFeatureBuildGuard.Registry.Single(r => r.Name == "Add effects demo grid");
            Assert.IsFalse(effectsSwitch.ActiveInReleaseBuild);
            Assert.IsFalse(EffectsPreviewSpawner.IsAllowed(isEditor: false, isDebugBuild: false), "release must ignore the grid");
            Assert.IsTrue(EffectsPreviewSpawner.IsAllowed(isEditor: false, isDebugBuild: true), "development builds DO show it");
        }

        [Test]
        public void OutlinePreviewOn_DevelopmentBuild_IsReported_WithTheHowToDisableText()
        {
            var config = ShippedConfig();
            config.outline_preview.enabled = true;

            var messages = DevFeatureBuildGuard.ActiveMessages(config, developmentBuild: true);

            Assert.AreEqual(1, messages.Count);
            StringAssert.Contains("Add outline demo grid", messages[0]);
            StringAssert.Contains("untick", messages[0]);
            StringAssert.Contains("Copy to StreamingAssets", messages[0]);
        }

        [Test]
        public void OutlinePreviewOn_ReleaseBuild_IsNotReported_BecauseTheRuntimeIgnoresIt()
        {
            var config = ShippedConfig();
            config.outline_preview.enabled = true;

            Assert.IsEmpty(DevFeatureBuildGuard.ActiveMessages(config, developmentBuild: false));
        }

        [Test]
        public void OutlinePreviewReleaseFlag_MatchesWhatTheRuntimeReallyDoes()
        {
            var outlineSwitch = DevFeatureBuildGuard.Registry.Single(r => r.Name == "Add outline demo grid");
            Assert.IsFalse(outlineSwitch.ActiveInReleaseBuild);
            Assert.IsFalse(OutlinePreviewSpawner.IsAllowed(isEditor: false, isDebugBuild: false), "release must ignore the grid");
            Assert.IsTrue(OutlinePreviewSpawner.IsAllowed(isEditor: false, isDebugBuild: true), "development builds DO show it");
        }

        [Test]
        public void LodDemoField_On_IsReportedForADevelopmentBuild_AndIgnoredForRelease()
        {
            var config = new WallConfigData();
            Assert.IsEmpty(DevFeatureBuildGuard.ActiveMessages(config, developmentBuild: true), "off by default: nothing to report");
            config.demo_field.enabled = true;
            var messages = DevFeatureBuildGuard.ActiveMessages(config, developmentBuild: true);
            Assert.AreEqual(1, messages.Count);
            StringAssert.Contains("Add LOD demo field", messages[0]);
            StringAssert.Contains("Global Scene > LOD > Test", messages[0]);
            Assert.IsEmpty(DevFeatureBuildGuard.ActiveMessages(config, developmentBuild: false), "release builds ignore the field");
        }

        [Test]
        public void LodDemoFieldReleaseFlag_MatchesWhatTheRuntimeReallyDoes()
        {
            var fieldSwitch = DevFeatureBuildGuard.Registry.Single(r => r.Name == "Add LOD demo field");
            Assert.IsFalse(fieldSwitch.ActiveInReleaseBuild);
            Assert.IsFalse(DemoFieldSpawner.IsAllowed(isEditor: false, isDebugBuild: false), "release must ignore the field");
            Assert.IsTrue(DemoFieldSpawner.IsAllowed(isEditor: false, isDebugBuild: true), "development builds DO show it");
            Assert.IsFalse(new DemoFieldSettings().enabled, "the switch is OFF by default");
        }

        [Test]
        public void DisplacementDemo_On_IsReportedForADevelopmentBuild_AndIgnoredForRelease()
        {
            var config = new WallConfigData();
            Assert.IsEmpty(DevFeatureBuildGuard.ActiveMessages(config, developmentBuild: true), "off by default: nothing to report");
            config.displacement_demo.enabled = true;
            var messages = DevFeatureBuildGuard.ActiveMessages(config, developmentBuild: true);
            Assert.AreEqual(1, messages.Count);
            StringAssert.Contains("Add displacement demo", messages[0]);
            StringAssert.Contains("Global Scene > Displacement > Test", messages[0]);
            Assert.IsEmpty(DevFeatureBuildGuard.ActiveMessages(config, developmentBuild: false), "release builds ignore the demo");

            var sw = DevFeatureBuildGuard.Registry.Single(r => r.Name == "Add displacement demo");
            Assert.IsFalse(sw.ActiveInReleaseBuild);
            Assert.IsFalse(DisplacementDemoSpawner.IsAllowed(isEditor: false, isDebugBuild: false), "release must ignore the demo");
            Assert.IsTrue(DisplacementDemoSpawner.IsAllowed(isEditor: false, isDebugBuild: true), "development builds DO show it");
            Assert.IsFalse(new DisplacementDemoSettings().enabled, "the switch is OFF by default");
        }

        [Test]
        public void EveryRegisteredSwitch_HasNameHowToAndPredicate_InAscii()
        {
            foreach (var sw in DevFeatureBuildGuard.Registry)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(sw.Name));
                Assert.IsFalse(string.IsNullOrWhiteSpace(sw.HowToDisable), sw.Name + " needs a how-to-disable text");
                Assert.IsNotNull(sw.IsOn, sw.Name + " needs a predicate");
                Assert.IsTrue((sw.Name + sw.HowToDisable).All(ch => ch < 128), sw.Name + " must be ASCII");
            }
        }
    }
}
