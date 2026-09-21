using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using TileStories.Editor;

namespace TileStories.Tests
{
    // Tests for the one shared "tell the developer" queue. The final native dialog cannot be
    // clicked by a test, so it is switched off for the whole run (TestDialogGuard); everything
    // that decides WHAT is shown and WHEN is real code exercised here.
    public class EditorNoticeTests
    {
        private const string TestKey = "test-only-notice-key";
        private readonly Dictionary<string, bool> _savedHidden = new Dictionary<string, bool>();

        // Hiding a notice writes Unity's real opt-out setting: remember the developer's own
        // choices before each test and put them back after, so running tests never
        // un-hides (or hides) anything for them.
        [SetUp]
        public void SetUp()
        {
            EditorNotice.Clear();
            _savedHidden.Clear();
            foreach (string key in NoticeKeys.All)
            {
                _savedHidden[key] = EditorNotice.IsHidden(key);
                EditorNotice.Unhide(key);
            }
            EditorNotice.Unhide(TestKey);
        }

        [TearDown]
        public void TearDown()
        {
            EditorNotice.Clear();
            foreach (var kv in _savedHidden)
            {
                if (kv.Value) EditorUtility.SetDialogOptOutDecision(DialogOptOutDecisionType.ForThisMachine, kv.Key, true);
                else EditorNotice.Unhide(kv.Key);
            }
            EditorNotice.Unhide(TestKey);
        }

        // ---------- queue timing ----------

        [Test]
        public void Discrete_Notice_IsDueImmediately_AndTakenOnce()
        {
            EditorNotice.Queue("Invalid path", "Please choose a file inside this Unity project.");
            Assert.IsTrue(EditorNotice.HasPending);

            Assert.IsTrue(EditorNotice.TryTakeDue(EditorApplication.timeSinceStartup + 0.01, out string title, out string message, out _));
            Assert.AreEqual("Invalid path", title);
            Assert.AreEqual("Please choose a file inside this Unity project.", message);
            Assert.IsFalse(EditorNotice.HasPending, "Taking a notice clears it: it shows exactly once.");
            Assert.IsFalse(EditorNotice.TryTakeDue(EditorApplication.timeSinceStartup + 10.0, out _, out _, out _));
        }

        [Test]
        public void Gesture_Notice_WaitsForTheQuietPeriod_AndEachRepeatRestartsIt()
        {
            double t0 = EditorApplication.timeSinceStartup;
            EditorNotice.Queue("Locked", "msg", EditorNotice.GestureQuietSeconds);

            Assert.IsFalse(EditorNotice.TryTakeDue(t0 + 0.1, out _, out _, out _), "Mid-drag: a modal now would break the drag.");
            Assert.IsTrue(EditorNotice.HasPending, "Still waiting.");

            // The drag keeps firing the same warning: the wait is re-armed, not shown.
            EditorNotice.Queue("Locked", "msg", EditorNotice.GestureQuietSeconds);
            Assert.IsFalse(EditorNotice.TryTakeDue(t0 + 0.2, out _, out _, out _));

            Assert.IsTrue(EditorNotice.TryTakeDue(EditorApplication.timeSinceStartup + EditorNotice.GestureQuietSeconds + 0.5, out _, out string message, out _),
                "Once the developer stops, it shows.");
            Assert.AreEqual("msg", message);
        }

        [Test]
        public void NewNoticeReplacesThePendingOne_EmptyMessagesAreIgnored_MissingTitleGetsADefault()
        {
            EditorNotice.Queue("A", "first");
            EditorNotice.Queue("B", "second");
            Assert.AreEqual("B", EditorNotice.PendingTitle);
            Assert.AreEqual("second", EditorNotice.PendingMessage);

            EditorNotice.Queue("C", "");
            EditorNotice.Queue("C", null);
            Assert.AreEqual("second", EditorNotice.PendingMessage, "Empty messages must not wipe a real one.");

            EditorNotice.Clear();
            EditorNotice.Queue(null, "no title");
            Assert.AreEqual("Notice", EditorNotice.PendingTitle);
        }

        [Test]
        public void TheTestRun_HasDialogsSwitchedOff_SoAQueuedNoticeCanNeverOpenAModal()
        {
            Assert.IsFalse(EditorNotice.ShowDialogs, "TestDialogGuard must have disabled real dialogs for this run.");

            // Give the real editor-update pump a chance to run: the notice must still be there.
            EditorNotice.Queue("Would be modal", "msg");
            var pump = typeof(EditorNotice).GetMethod("Pump", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(pump);
            pump.Invoke(null, null);
            Assert.IsTrue(EditorNotice.HasPending, "With dialogs off the pump must leave the queue untouched.");
        }

        // ---------- opt-out checkbox ("do not show again") ----------

        [Test]
        public void HiddenState_IsUnitysOwnOptOut_AndUnhideRestoresIt()
        {
            Assert.IsFalse(EditorNotice.IsHidden(TestKey));

            // What ticking the dialog's checkbox does (Unity stores it; we only read it).
            EditorUtility.SetDialogOptOutDecision(DialogOptOutDecisionType.ForThisMachine, TestKey, true);
            Assert.IsTrue(EditorNotice.IsHidden(TestKey));

            EditorNotice.Unhide(TestKey);
            Assert.IsFalse(EditorNotice.IsHidden(TestKey), "Unhide (the reset menu) brings it back.");
        }

        [Test]
        public void HiddenNotice_IsNeverQueued_ButOtherNoticesAre()
        {
            EditorNotice.Queue("t", "advice", 0f, TestKey);
            Assert.AreEqual(TestKey, EditorNotice.PendingDontShowAgainKey, "The pending notice carries its key so the dialog can offer the checkbox.");
            EditorNotice.Clear();

            EditorUtility.SetDialogOptOutDecision(DialogOptOutDecisionType.ForThisMachine, TestKey, true);
            EditorNotice.Queue("t", "advice", 0f, TestKey);
            Assert.IsFalse(EditorNotice.HasPending, "A hidden notice must not reach the developer.");

            EditorNotice.Queue("t", "a refusal explanation");   // no key: cannot be hidden
            Assert.AreEqual("a refusal explanation", EditorNotice.PendingMessage);
            Assert.IsNull(EditorNotice.PendingDontShowAgainKey, "No key = no opt-out checkbox.");
        }

        [Test]
        public void ResetHiddenNotices_CoversEveryHideableNotice()
        {
            CollectionAssert.AreEquivalent(
                new[] { NoticeKeys.FacingPreviewWarning, NoticeKeys.ConfigValidation }, NoticeKeys.All,
                "Every notice that offers the opt-out checkbox must be in All, or the reset menu cannot bring it back.");

            foreach (string key in NoticeKeys.All) EditorUtility.SetDialogOptOutDecision(DialogOptOutDecisionType.ForThisMachine, key, true);
            var reset = typeof(EditorNotice).GetMethod("ResetHiddenNotices", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(reset);
            reset.Invoke(null, null);
            foreach (string key in NoticeKeys.All)
                Assert.IsFalse(EditorNotice.IsHidden(key), key + " must be visible again after the reset menu item.");
        }

        [Test]
        public void FacingPreviewWarning_CarriesTheKey_AndStaysSilentOnceHidden()
        {
            var window = ScriptableObject.CreateInstance<POIEditorToolWindow>();
            try
            {
                var poi = new POIData { id = "p" };
                var config = new WallConfigData
                {
                    orientation_settings = new OrientationSettings { facing_mode = "always_facing_camera", edit_mode_preview_enabled = true }
                };
                config.pois.Add(poi);
                typeof(POIEditorToolWindow).GetField("_config", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(window, config);
                var warn = typeof(POIEditorToolWindow).GetMethod("WarnIfFacingEditInvisible", BindingFlags.NonPublic | BindingFlags.Instance);

                warn.Invoke(window, new object[] { poi, true, false, false });
                Assert.AreEqual(NoticeKeys.FacingPreviewWarning, EditorNotice.PendingDontShowAgainKey);
                EditorNotice.Clear();

                EditorUtility.SetDialogOptOutDecision(DialogOptOutDecisionType.ForThisMachine, NoticeKeys.FacingPreviewWarning, true);
                warn.Invoke(window, new object[] { poi, true, true, true });
                Assert.IsFalse(EditorNotice.HasPending, "Hidden: no warning for any axis.");
            }
            finally { Object.DestroyImmediate(window); }
        }

        [Test]
        public void ResultOfTheDevelopersOwnClick_IsNeverHideable()
        {
            var window = ScriptableObject.CreateInstance<POIEditorToolWindow>();
            try
            {
                typeof(POIEditorToolWindow).GetMethod("ClearRig", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(window, null);
                Assert.AreEqual("Nothing to clear", EditorNotice.PendingTitle);
                Assert.IsNull(EditorNotice.PendingDontShowAgainKey, "Hiding the answer to a click would make the button look broken.");
            }
            finally { Object.DestroyImmediate(window); }
        }

        // ---------- validation report text ----------

        [Test]
        public void FormatList_ShowsEachFindingWithValueAndFix_ThenTheGuidance()
        {
            var items = new List<EditorAlertItem>
            {
                new EditorAlertItem("poi_lamp", "level_x", "Unknown hierarchy level.", "Pick an existing level."),
                new EditorAlertItem("<LOD settings>", "", "Backwards thresholds.")
            };

            string text = EditorAlertItem.FormatList(items, "Fix these.");

            StringAssert.Contains("- poi_lamp (level_x): Unknown hierarchy level.", text);
            StringAssert.Contains("Fix: Pick an existing level.", text);
            StringAssert.Contains("- <LOD settings>: Backwards thresholds.", text);
            StringAssert.DoesNotContain("<LOD settings> (", text, "An empty value adds no parentheses.");
            Assert.IsTrue(text.EndsWith("Fix these."), "Guidance closes the message.");
        }

        [Test]
        public void FormatList_CapsALongReport_AndSaysHowManyMore()
        {
            var items = new List<EditorAlertItem>();
            for (int i = 0; i < 10; i++)
                items.Add(new EditorAlertItem("poi_" + i, "v", "problem " + i));

            string text = EditorAlertItem.FormatList(items, null);

            StringAssert.Contains("poi_5", text);
            StringAssert.DoesNotContain("poi_6", text, "Only the first 6 are listed.");
            StringAssert.Contains("...and 4 more.", text);
        }

        // ---------- converted call sites: what used to be a modal / vanishing HelpBox / popup ----------

        [Test]
        public void ClearRig_WithNothingToClear_QueuesANoticeInsteadOfOpeningAModal()
        {
            Assert.IsNull(GameObject.Find("POIEditorRig"), "Premise: no rig in the scene for this test.");

            var window = ScriptableObject.CreateInstance<POIEditorToolWindow>();
            try
            {
                var clear = typeof(POIEditorToolWindow).GetMethod("ClearRig", BindingFlags.NonPublic | BindingFlags.Instance);
                Assert.IsNotNull(clear);
                clear.Invoke(window, null);

                Assert.AreEqual("Nothing to clear", EditorNotice.PendingTitle);
                StringAssert.Contains("no children", EditorNotice.PendingMessage);
            }
            finally { Object.DestroyImmediate(window); }
        }

        [Test]
        public void ClearRig_WithAnEmptyRig_AlsoQueuesTheNotice()
        {
            var rig = new GameObject("POIEditorRig");
            var window = ScriptableObject.CreateInstance<POIEditorToolWindow>();
            try
            {
                typeof(POIEditorToolWindow).GetMethod("ClearRig", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(window, null);
                Assert.AreEqual("Nothing to clear", EditorNotice.PendingTitle);
            }
            finally
            {
                Object.DestroyImmediate(window);
                Object.DestroyImmediate(rig);
            }
        }

        [Test]
        public void ConfigValidation_WithIssues_QueuesOneNoticeListingThem()
        {
            var window = ScriptableObject.CreateInstance<POIEditorToolWindow>();
            try
            {
                var config = new WallConfigData
                {
                    lod_settings = new LodSettings { shrink_start_neighbor_count = 9, cluster_min_count = 3 }   // backwards
                };
                typeof(POIEditorToolWindow).GetField("_config", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(window, config);

                typeof(POIEditorToolWindow).GetMethod("ValidateAndAlert", BindingFlags.NonPublic | BindingFlags.Instance)
                    .Invoke(window, new object[] { "after load" });

                Assert.AreEqual("Config validation issues (after load)", EditorNotice.PendingTitle);
                Assert.AreEqual(NoticeKeys.ConfigValidation, EditorNotice.PendingDontShowAgainKey, "Repeating advisory: offers the opt-out checkbox.");
                StringAssert.Contains("<LOD settings>", EditorNotice.PendingMessage);
                StringAssert.Contains("Shrink Start must be strictly less than Cluster Min", EditorNotice.PendingMessage);
            }
            finally { Object.DestroyImmediate(window); }
        }

        [Test]
        public void ConfigValidation_WithNoIssues_QueuesNothing()
        {
            var window = ScriptableObject.CreateInstance<POIEditorToolWindow>();
            try
            {
                var config = new WallConfigData();
                typeof(POIEditorToolWindow).GetField("_config", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(window, config);

                typeof(POIEditorToolWindow).GetMethod("ValidateAndAlert", BindingFlags.NonPublic | BindingFlags.Instance)
                    .Invoke(window, new object[] { "before save" });

                Assert.IsFalse(EditorNotice.HasPending, "A clean config must not nag.");
            }
            finally { Object.DestroyImmediate(window); }
        }

        [Test]
        public void FacingWarnings_UseGestureTiming_AndTheRightTitles()
        {
            Assert.AreEqual("POI is verified", POIEditorToolWindow.VerifiedNoticeTitle);
            Assert.AreEqual("Facing has no visible effect", POIEditorToolWindow.FacingNoticeTitle);

            var window = ScriptableObject.CreateInstance<POIEditorToolWindow>();
            try
            {
                var poi = new POIData { id = "p" };
                var config = new WallConfigData
                {
                    orientation_settings = new OrientationSettings { facing_mode = "always_facing_camera", edit_mode_preview_enabled = true }
                };
                config.pois.Add(poi);
                typeof(POIEditorToolWindow).GetField("_config", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(window, config);

                typeof(POIEditorToolWindow).GetMethod("WarnIfFacingEditInvisible", BindingFlags.NonPublic | BindingFlags.Instance)
                    .Invoke(window, new object[] { poi, true, false, false });

                Assert.AreEqual("Facing has no visible effect", EditorNotice.PendingTitle);
                Assert.AreEqual(FacingEditAdvice.AlwaysFacingCameraMessage, EditorNotice.PendingMessage);
                Assert.IsFalse(EditorNotice.TryTakeDue(EditorApplication.timeSinceStartup + 0.05, out _, out _, out _),
                    "A slider drag must not open a dialog mid-drag.");
            }
            finally { Object.DestroyImmediate(window); }
        }
    }
}
