using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace TileStories.Tests
{
    // LayoutStruggleGuard (_3.2 chunk G) on its own, no scene: the line Unity logs, written as Unity writes it, in each phase a scene fixture
    // goes through. The guard fails naming the test, the scene and every phase it heard the line in; any other error, a line that only
    // mentions the words, or a line logged once it stopped listening, is not its business
    public class LayoutStruggleGuardTests
    {
        // The line as UI Toolkit's layout updater logs it (Editor-prev.log, 2026-10-21, the audit's morning run)
        private const string UnitysLine = LayoutStruggleGuard.Struggling +
            " to process current layout (consider simplifying to avoid recursive layout): PanelRootElement PanelSettings (x:0.00, y:0.00, width:390.00, height:844.00)";
        private const string Scene = "Assets/Apps/LivingRoom/LivingRoomScene.unity";

        private static void LogExpected(string line)
        {
            LogAssert.Expect(LogType.Error, line);
            Debug.LogError(line);
        }

        [Test]
        public void TheLine_InAnyPhase_FailsEnd_NamingTheTestTheSceneAndEachPhase()
        {
            var guard = new LayoutStruggleGuard();
            guard.Begin(Scene);
            LogExpected(UnitysLine);
            guard.Phase("the test");
            guard.Phase("TearDown, while the scene unloads");
            LogExpected(UnitysLine);
            LogExpected(UnitysLine);

            // - Finish is End without the Assert.Fail: what the fixture's TearDown would fail with
            string failure = guard.Finish();
            Assert.IsNotNull(failure, "the guard fails on the line");
            StringAssert.Contains(TestContext.CurrentContext.Test.FullName, failure, "it names the test");
            StringAssert.Contains(Scene, failure, "the scene");
            StringAssert.IsMatch("scene load #[0-9]+", failure, "and the load's number");
            StringAssert.Contains("3 time(s)", failure, "every line counts");
            Assert.AreEqual(1, Regex.Matches(failure, "Error in SetUp, while the scene loads").Count, "one line heard while the scene loaded");
            Assert.AreEqual(2, Regex.Matches(failure, "Error in TearDown, while the scene unloads").Count, "two while it unloaded");
            StringAssert.Contains("PanelRootElement PanelSettings", failure, "and Unity's own line, to see which panel");
            Assert.IsFalse(guard.Listening, "the guard let go before failing: nothing of this test reaches the next");
        }

        [Test]
        public void OtherErrors_ALineThatOnlyMentionsTheWords_AndOneAfterEnd_PassThrough()
        {
            var guard = new LayoutStruggleGuard();
            guard.Begin(Scene);
            guard.Phase("the test");
            LogExpected("[ARFoundationSupport] no camera");
            LogExpected("[Card] something else: " + LayoutStruggleGuard.Struggling);
            Assert.IsNull(guard.Finish(), "neither is the layout line");
            Assert.IsFalse(guard.Listening);

            // - once it let go the guard hears nothing, and a second Finish has nothing to say
            LogExpected(UnitysLine);
            Assert.IsNull(guard.Finish());

            // - a new Begin starts clean: nothing heard before it counts
            guard.Begin(Scene);
            Assert.IsTrue(guard.Listening);
            Assert.IsNull(guard.Finish(), "the line logged before this Begin is not this load's");
        }
    }
}
