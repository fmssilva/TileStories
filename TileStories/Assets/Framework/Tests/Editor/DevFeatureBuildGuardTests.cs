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
            StringAssert.Contains("Focus on Effects Grid", messages[0]);
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
            var effectsSwitch = DevFeatureBuildGuard.Registry.Single(r => r.Name == "Focus on Effects Grid");
            Assert.IsFalse(effectsSwitch.ActiveInReleaseBuild);
            Assert.IsFalse(EffectsPreviewSpawner.IsAllowed(isEditor: false, isDebugBuild: false), "release must ignore the grid");
            Assert.IsTrue(EffectsPreviewSpawner.IsAllowed(isEditor: false, isDebugBuild: true), "development builds DO show it");
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
