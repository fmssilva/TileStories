using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using TileStories.Editor;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace TileStories.Tests
{
    // The ONE blocking popup kind: EditorDecision (a question the caller must wait for).
    // A real dialog cannot be clicked by a test, so every test here sets EditorDecision.Responder:
    // it receives the exact question and "clicks" one button. Everything else is REAL: a real
    // POIEditorToolWindow, a real rig of GameObjects in the test scene, real config files on disk
    // (under the project's Temp/ folder, never the shipped config), the real Clear Rig /
    // Load & Populate / Delete POI / unverify / Play / Build gate code. Rule under test: EVERY
    // button of EVERY question does exactly what its label says -- one test per button.
    public class EditorDecisionTests
    {
        private const BindingFlags Instance = BindingFlags.NonPublic | BindingFlags.Instance;
        private static readonly string TempDir = Path.GetFullPath("Temp/__EditorDecisionTests");

        private readonly List<DecisionRequest> _asked = new List<DecisionRequest>();
        private DecisionAnswer _answer;
        private POIEditorToolWindow _window;
        private GameObject _anchor;
        private bool _unverifyWasHidden;

        [SetUp]
        public void SetUp()
        {
            _asked.Clear();
            _answer = DecisionAnswer.Cancel;
            EditorDecision.Responder = request => { _asked.Add(request); return _answer; };
            _unverifyWasHidden = EditorNotice.IsHidden(NoticeKeys.UnverifyPosition);
            EditorNotice.Unhide(NoticeKeys.UnverifyPosition);
            EditorNotice.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            EditorDecision.Responder = TestDialogGuard.CancelEverything;
            if (_unverifyWasHidden) EditorNotice.Hide(NoticeKeys.UnverifyPosition);
            else EditorNotice.Unhide(NoticeKeys.UnverifyPosition);
            EditorNotice.Clear();
            if (_window != null) UnityEngine.Object.DestroyImmediate(_window);
            if (_anchor != null) UnityEngine.Object.DestroyImmediate(_anchor);
            if (Directory.Exists(TempDir)) Directory.Delete(TempDir, true);
        }

        // ---------- the one mapping ----------

        // Unity returns 1 for the Cancel button AND for Esc / closing the dialog, so slot 1 is
        // Cancel for every question: pressing Esc must never discard work or start a build.
        [Test]
        public void Resolve_EscAndAnythingUnexpected_Cancel_AndSlot2OnlyWhenOffered()
        {
            Assert.AreEqual(DecisionAnswer.Confirm, EditorDecision.Resolve(0, hasAlternative: true));
            Assert.AreEqual(DecisionAnswer.Cancel, EditorDecision.Resolve(1, hasAlternative: true), "slot 1 = Cancel button / Esc / X");
            Assert.AreEqual(DecisionAnswer.Alternative, EditorDecision.Resolve(2, hasAlternative: true));
            Assert.AreEqual(DecisionAnswer.Cancel, EditorDecision.Resolve(2, hasAlternative: false),
                "a stray slot 2 on a two-button question (the build gate) must never let the action go on");
            Assert.AreEqual(DecisionAnswer.Cancel, EditorDecision.Resolve(-1, hasAlternative: true), "anything unexpected cancels");
        }

        [Test]
        public void Ask_HandsTheResponderTheExactQuestion_AndReturnsItsAnswer()
        {
            _answer = DecisionAnswer.Alternative;
            var answer = EditorDecision.Ask("Title", "Message", "Do it", alternativeLabel: "Other way");

            Assert.AreEqual(DecisionAnswer.Alternative, answer);
            Assert.AreEqual(1, _asked.Count);
            Assert.AreEqual("Title", _asked[0].Title);
            Assert.AreEqual("Message", _asked[0].Message);
            Assert.AreEqual("Do it", _asked[0].ConfirmLabel);
            Assert.AreEqual("Other way", _asked[0].AlternativeLabel);
        }

        [Test]
        public void AScriptedClickOnAButtonThatWasNotOffered_IsACancel()
        {
            _answer = DecisionAnswer.Alternative;
            Assert.AreEqual(DecisionAnswer.Cancel, EditorDecision.Ask("t", "m", "Do it"),
                "a two-button question has no Alternative: the seam can do no more than a real click");
        }

        [Test]
        public void AHiddenQuestion_AnswersConfirm_WithoutBeingAsked_AndTheResetMenuBringsItBack()
        {
            EditorNotice.Hide(NoticeKeys.UnverifyPosition);
            Assert.AreEqual(DecisionAnswer.Confirm,
                EditorDecision.Ask("t", "m", "Yes", dontAskAgainKey: NoticeKeys.UnverifyPosition));
            Assert.IsEmpty(_asked, "a question the developer chose never to see again must not be shown");

            typeof(EditorNotice).GetMethod("ResetHiddenMessages", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, null);
            EditorDecision.Ask("t", "m", "Yes", dontAskAgainKey: NoticeKeys.UnverifyPosition);
            Assert.AreEqual(1, _asked.Count, "after TileStories > Reset Hidden Messages it is asked again");
        }

        // No second path to a blocking dialog anywhere in the Editor code, and no anchored
        // PopupWindow (the old not-draggable popup kind) either.
        [Test]
        public void TheEditorCode_HasNoDialogOrPopupWindowOutsideTheSharedPopups()
        {
            string root = Path.Combine(Application.dataPath, "Framework/Editor");
            var offenders = new List<string>();
            foreach (string file in Directory.GetFiles(root, "*.cs", SearchOption.AllDirectories))
            {
                bool isDecision = file.Replace('\\', '/').EndsWith("Shared/Popups/EditorDecision.cs");
                string[] lines = File.ReadAllLines(file);
                for (int i = 0; i < lines.Length; i++)
                {
                    string code = Regex.Replace(lines[i], @"//.*$", "");
                    if ((!isDecision && Regex.IsMatch(code, @"\bDisplayDialog(Complex)?\s*\("))
                        || Regex.IsMatch(code, @"\bPopupWindow\s*\.\s*Show\b|:\s*PopupWindowContent\b|\bShowModal"))
                        offenders.Add(Path.GetFileName(file) + ":" + (i + 1));
                }
            }
            Assert.IsEmpty(offenders, "Ask with EditorDecision, show everything else with EditorPopup: " + string.Join(", ", offenders));
        }

        // ---------- fixtures: a real window, a real rig, real files ----------

        private POIEditorToolWindow Window(params POIData[] pois)
        {
            _window = ScriptableObject.CreateInstance<POIEditorToolWindow>();
            var config = new WallConfigData { pois = pois.ToList() };
            typeof(POIEditorToolWindow).GetField("_config", Instance).SetValue(_window, config);
            typeof(POIEditorToolWindow).GetMethod("InitializeConfigHistory", Instance).Invoke(_window, null);
            return _window;
        }

        private WallConfigData Config => (WallConfigData)typeof(POIEditorToolWindow).GetField("_config", Instance).GetValue(_window);
        private bool Unsaved => (bool)typeof(POIEditorToolWindow).GetField("_hasUnsavedChanges", Instance).GetValue(_window);

        private void SetUnsaved(bool value) =>
            typeof(POIEditorToolWindow).GetField("_hasUnsavedChanges", Instance).SetValue(_window, value);

        // A real rig in the test scene: PlacementCorrectionAnchor / POIEditorRig / one child per id
        private Transform Rig(params string[] childIds)
        {
            Assert.IsNull(GameObject.Find("POIEditorRig"), "Premise: no rig in the scene before the test.");
            _anchor = new GameObject("PlacementCorrectionAnchor");
            var rig = new GameObject("POIEditorRig").transform;
            rig.SetParent(_anchor.transform, false);
            foreach (string id in childIds)
                new GameObject(id).transform.SetParent(rig, false);
            return rig;
        }

        private void Call(string method, params object[] args) =>
            typeof(POIEditorToolWindow).GetMethod(method, Instance).Invoke(_window, args);

        // Two POIs saved at x = 1 and x = 2 in a real config.json under Temp/; the window reads,
        // saves and copies ONLY these files (its paths point at them).
        private string SourcePath => Path.Combine(TempDir, "config.json");
        private string StreamingPath => Path.Combine(TempDir, "streaming", "config.json");

        private POIEditorToolWindow WindowOnTempFiles()
        {
            Directory.CreateDirectory(TempDir);
            var saved = new WallConfigData
            {
                pois = new List<POIData>
                {
                    new POIData { id = "poi_a", name = "A", position = new PositionData { x = 1f } },
                    new POIData { id = "poi_b", name = "B", position = new PositionData { x = 2f } },
                }
            };
            File.WriteAllText(SourcePath, JsonUtility.ToJson(saved, true));
            Window();
            typeof(POIEditorToolWindow).GetField("_configPath", Instance).SetValue(_window, SourcePath);
            typeof(POIEditorToolWindow).GetField("_streamingConfigPath", Instance).SetValue(_window, StreamingPath);
            Call("LoadConfig");
            return _window;
        }

        private static WallConfigData Read(string path) => JsonUtility.FromJson<WallConfigData>(File.ReadAllText(path));

        // Put the rig in step with the saved config, then move poi_a to x = 5 and edit poi_b's name
        // without saving: the two kinds of work a reload or a Play would drop.
        private Transform RigWithMovedMarkerAndUnsavedEdit()
        {
            var rig = Rig("poi_a", "poi_b");
            rig.Find("poi_a").localPosition = new Vector3(5f, 0f, 0f);
            rig.Find("poi_b").localPosition = new Vector3(2f, 0f, 0f);
            Config.pois[1].name = "B edited";
            SetUnsaved(true);
            return rig;
        }

        // ---------- Clear Rig ----------

        [Test]
        public void ClearRig_ClearAnyway_ClearsTheRig()
        {
            var rig = Rig("poi_moved", "poi_never_captured");
            Window(new POIData { id = "poi_moved", position = new PositionData { x = 5f } });   // rig child sits at 0: moved

            _answer = DecisionAnswer.Confirm;
            Call("ClearRig");
            Assert.AreEqual(1, _asked.Count, "out-of-sync markers: Clear Rig must ask first");
            StringAssert.Contains("2 marker(s)", _asked[0].Message, "the question says how many markers would be lost");
            Assert.AreEqual("Clear anyway", _asked[0].ConfirmLabel);
            Assert.AreEqual(0, rig.childCount, "Clear anyway clears the rig");
        }

        [Test]
        public void ClearRig_Cancel_KeepsEveryMarker()
        {
            var rig = Rig("poi_moved");
            Window(new POIData { id = "poi_moved", position = new PositionData { x = 5f } });
            Call("ClearRig");
            Assert.AreEqual(1, rig.childCount, "Cancel keeps every marker");
        }

        [Test]
        public void ClearRig_InSync_NeverAsks()
        {
            var rig = Rig("poi_a");
            Window(new POIData { id = "poi_a", position = new PositionData() });
            Call("ClearRig");
            Assert.IsEmpty(_asked, "nothing would be lost: no question");
            Assert.AreEqual(0, rig.childCount);
        }

        // ---------- Load & Populate Rig: one question, three buttons ----------

        [Test]
        public void Reload_Question_ListsExactlyWhatWouldBeDropped()
        {
            var both = POIEditorToolWindow.BuildReloadQuestion(3, true);
            Assert.AreEqual("Save & Reload", both.ConfirmLabel);
            Assert.AreEqual("Discard & Reload", both.AlternativeLabel);
            StringAssert.Contains("3 rig marker(s) moved since the last save", both.Message);
            StringAssert.Contains("unsaved config edits", both.Message);

            var movesOnly = POIEditorToolWindow.BuildReloadQuestion(1, false);
            StringAssert.DoesNotContain("unsaved config edits", movesOnly.Message, "only what is really at stake is listed");
            var editsOnly = POIEditorToolWindow.BuildReloadQuestion(0, true);
            StringAssert.DoesNotContain("moved since", editsOnly.Message);
        }

        [Test]
        public void Reload_NothingToLose_AsksNothing_AndRebuildsTheRigFromTheFile()
        {
            WindowOnTempFiles();
            var rig = Rig("poi_a", "poi_b");
            rig.Find("poi_a").localPosition = new Vector3(1f, 0f, 0f);
            rig.Find("poi_b").localPosition = new Vector3(2f, 0f, 0f);
            var oldMarker = rig.Find("poi_a").gameObject;

            Call("LoadAndPopulateRig");

            Assert.IsEmpty(_asked, "in sync and saved: Load & Populate needs no question (no 'Clear existing rig first?' either)");
            CollectionAssert.AreEquivalent(new[] { "poi_a", "poi_b" }, rig.Cast<Transform>().Select(t => t.name).ToArray(),
                "the rig is rebuilt: one marker per POI");
            Assert.IsTrue(oldMarker == null, "the old stand-ins were replaced, not kept next to the new ones");
        }

        [Test]
        public void Reload_SaveAndReload_SavesTheMoveAndTheEdit_ThenReloadsThem()
        {
            WindowOnTempFiles();
            var rig = RigWithMovedMarkerAndUnsavedEdit();

            _answer = DecisionAnswer.Confirm;
            Call("LoadAndPopulateRig");

            Assert.AreEqual(1, _asked.Count, "ONE question, not a chain of them");
            Assert.AreEqual("Save & Reload", _asked[0].ConfirmLabel);
            var onDisk = Read(SourcePath);
            Assert.AreEqual(5f, onDisk.pois[0].position.x, 1e-4f, "Save: the moved marker's place is in config.json");
            Assert.AreEqual("B edited", onDisk.pois[1].name, "Save: the unsaved edit is in config.json");
            Assert.AreEqual("B edited", Config.pois[1].name, "Reload: the window shows what was saved");
            Assert.IsFalse(Unsaved, "Reload: nothing unsaved any more");
            Assert.AreEqual(5f, rig.Find("poi_a").localPosition.x, 1e-4f, "Reload: the rebuilt marker stands at the saved place");
        }

        [Test]
        public void Reload_DiscardAndReload_DropsTheMoveAndTheEdit_AndLeavesTheFileAlone()
        {
            WindowOnTempFiles();
            var rig = RigWithMovedMarkerAndUnsavedEdit();
            string before = File.ReadAllText(SourcePath);

            _answer = DecisionAnswer.Alternative;
            Call("LoadAndPopulateRig");

            Assert.AreEqual(before, File.ReadAllText(SourcePath), "Discard: config.json is not written");
            Assert.AreEqual("B", Config.pois[1].name, "Discard: the unsaved edit is gone");
            Assert.IsFalse(Unsaved);
            Assert.AreEqual(1f, rig.Find("poi_a").localPosition.x, 1e-4f, "Discard: the marker is back at its saved place");
        }

        [Test]
        public void Reload_Cancel_ChangesNothing()
        {
            WindowOnTempFiles();
            var rig = RigWithMovedMarkerAndUnsavedEdit();
            string before = File.ReadAllText(SourcePath);
            var configBefore = Config;

            Call("LoadAndPopulateRig");

            Assert.AreEqual(before, File.ReadAllText(SourcePath), "Cancel: no file written");
            Assert.AreSame(configBefore, Config, "Cancel: nothing reloaded");
            Assert.AreEqual("B edited", Config.pois[1].name, "Cancel: the unsaved edit survives");
            Assert.IsTrue(Unsaved);
            Assert.AreEqual(5f, rig.Find("poi_a").localPosition.x, 1e-4f, "Cancel: the moved marker stays where it was moved");
        }

        // ---------- Delete POI / delete a row still in use / unverify ----------

        [Test]
        public void DeletePoi_Delete_RemovesItFromConfigAndRig_Cancel_KeepsIt()
        {
            var rig = Rig("poi_a");
            var poi = new POIData { id = "poi_a", name = "Lamp" };
            Window(poi);

            _answer = DecisionAnswer.Cancel;
            Call("TryDeletePoiAt", 0, poi);
            Assert.AreEqual(1, _asked.Count);
            StringAssert.Contains("\"Lamp\"", _asked[0].Message);
            Assert.AreEqual("Delete", _asked[0].ConfirmLabel);
            Assert.AreEqual(1, Config.pois.Count, "Cancel keeps the POI");
            Assert.AreEqual(1, rig.childCount);

            _answer = DecisionAnswer.Confirm;
            Call("TryDeletePoiAt", 0, poi);
            Assert.AreEqual(0, Config.pois.Count, "Delete removes it from the config");
            Assert.AreEqual(0, rig.childCount, "and from the Scene rig");
        }

        [Test]
        public void IdentityDeleteGuard_AsksOnlyWhenPoisStillUseTheRow_AndSaysHowMany()
        {
            Assert.IsTrue(IdentityDeleteGuard.Confirm("Category", "unused", 0));
            Assert.IsEmpty(_asked, "an unused row deletes with one click");

            _answer = DecisionAnswer.Cancel;
            Assert.IsFalse(IdentityDeleteGuard.Confirm("Category", "religious", 3));
            StringAssert.Contains("3 POI(s) still reference 'religious'", _asked[0].Message);
            Assert.AreEqual("Category still in use", _asked[0].Title);
            Assert.AreEqual("Delete anyway", _asked[0].ConfirmLabel);

            _answer = DecisionAnswer.Confirm;
            Assert.IsTrue(IdentityDeleteGuard.Confirm("Category", "religious", 3));
        }

        [Test]
        public void Unverify_YesUnlock_Unlocks_Cancel_StaysLocked_AndOffersDontAskAgain()
        {
            var poi = new POIData { id = "poi_a", name = "Lamp", position = new PositionData(), position_verified = true };
            Window(poi);

            _answer = DecisionAnswer.Cancel;
            _window.TogglePoiVerification(poi);
            Assert.AreEqual(1, _asked.Count);
            Assert.AreEqual("Yes, Unlock", _asked[0].ConfirmLabel);
            Assert.AreEqual(NoticeKeys.UnverifyPosition, _asked[0].DontAskAgainKey, "the native 'do not show again' box is offered");
            Assert.IsNull(_asked[0].AlternativeLabel, "no home-made third button any more");
            Assert.IsTrue(Config.pois[0].position_verified, "Cancel keeps it verified");

            _answer = DecisionAnswer.Confirm;
            _window.TogglePoiVerification(Config.pois[0]);
            Assert.IsFalse(Config.pois[0].position_verified, "Yes, Unlock unverifies it");
        }

        // ---------- Play / Build gate: the labels follow the real state ----------

        [Test]
        public void Gate_Labels_SaySaveOnlyWhenThereIsSomethingToSave_AndAWindowToSaveIt()
        {
            var moved = POIEditorToolWindow.BuildRigSafetyQuestion(26, isBuild: false, canSave: true, movedCount: 3, hasUnsavedEdits: false);
            Assert.AreEqual("Save, Clear & Play", moved.ConfirmLabel);
            Assert.AreEqual("Play With Duplicates", moved.AlternativeLabel);
            StringAssert.Contains("26 POI Editor stand-in marker(s)", moved.Message);
            StringAssert.Contains("3 of them were moved since the last save", moved.Message);
            StringAssert.Contains("StreamingAssets copy", moved.Message, "the message says the save reaches the copy Play reads");
            StringAssert.Contains("show twice", moved.Message, "the consequence of Play With Duplicates is stated");

            var edits = POIEditorToolWindow.BuildRigSafetyQuestion(26, false, true, 0, hasUnsavedEdits: true);
            Assert.AreEqual("Save, Clear & Play", edits.ConfirmLabel);
            StringAssert.Contains("unsaved edits", edits.Message);

            var saved = POIEditorToolWindow.BuildRigSafetyQuestion(26, false, true, 0, false);
            Assert.AreEqual("Clear & Play", saved.ConfirmLabel, "nothing to save: the label must not promise a save");
            StringAssert.Contains("already saved", saved.Message);

            var noWindow = POIEditorToolWindow.BuildRigSafetyQuestion(26, false, canSave: false, 0, false);
            Assert.AreEqual("Clear & Play", noWindow.ConfirmLabel, "no POI Editor open: nothing CAN be saved");
            StringAssert.Contains("loses its new place", noWindow.Message, "and the message says what is lost");

            var build = POIEditorToolWindow.BuildRigSafetyQuestion(26, isBuild: true, true, 3, false);
            Assert.AreEqual("Save, Clear & Build", build.ConfirmLabel);
            Assert.IsNull(build.AlternativeLabel, "a build never goes on with the stand-ins in it");
            StringAssert.DoesNotContain("Duplicates", build.Message);
        }

        [Test]
        public void PlayGate_SaveClearAndPlay_SavesToConfigAndStreamingAssets_ClearsTheRig_AndPlays()
        {
            WindowOnTempFiles();
            var rig = RigWithMovedMarkerAndUnsavedEdit();

            _answer = DecisionAnswer.Confirm;
            bool play = POIEditorToolWindow.PromptBeforePlayOrBuild(isBuild: false, _window);

            Assert.AreEqual("Save, Clear & Play", _asked[0].ConfirmLabel);
            Assert.IsTrue(play, "Play goes on");
            Assert.AreEqual(0, rig.childCount, "Clear: every stand-in is gone");
            Assert.AreEqual(5f, Read(SourcePath).pois[0].position.x, 1e-4f, "Save: the moved marker's place is in config.json");
            Assert.AreEqual("B edited", Read(SourcePath).pois[1].name, "Save: the unsaved edit is in config.json");
            Assert.IsTrue(File.Exists(StreamingPath), "Save: the StreamingAssets copy (what Play reads) is written too");
            Assert.AreEqual(File.ReadAllText(SourcePath), File.ReadAllText(StreamingPath), "and it is the same config");
        }

        [Test]
        public void PlayGate_ClearAndPlay_WhenAllIsSaved_WritesNoFile_ClearsTheRig()
        {
            WindowOnTempFiles();
            var rig = Rig("poi_a", "poi_b");
            rig.Find("poi_a").localPosition = new Vector3(1f, 0f, 0f);
            rig.Find("poi_b").localPosition = new Vector3(2f, 0f, 0f);
            DateTime written = File.GetLastWriteTimeUtc(SourcePath);

            _answer = DecisionAnswer.Confirm;
            Assert.IsTrue(POIEditorToolWindow.PromptBeforePlayOrBuild(false, _window));

            Assert.AreEqual("Clear & Play", _asked[0].ConfirmLabel);
            Assert.AreEqual(0, rig.childCount, "Clear: every stand-in is gone");
            Assert.AreEqual(written, File.GetLastWriteTimeUtc(SourcePath), "no save promised, none done");
            Assert.IsFalse(File.Exists(StreamingPath));
        }

        [Test]
        public void PlayGate_ClearAndPlay_WithNoEditorOpen_ClearsTheRig()
        {
            var rig = Rig("poi_a", "poi_b");
            _answer = DecisionAnswer.Confirm;
            Assert.IsTrue(POIEditorToolWindow.PromptBeforePlayOrBuild(false, null));
            Assert.AreEqual("Clear & Play", _asked[0].ConfirmLabel);
            Assert.AreEqual(0, rig.childCount);
        }

        [Test]
        public void PlayGate_PlayWithDuplicates_LeavesTheRigAndTheFiles()
        {
            WindowOnTempFiles();
            var rig = RigWithMovedMarkerAndUnsavedEdit();
            string before = File.ReadAllText(SourcePath);

            _answer = DecisionAnswer.Alternative;
            Assert.IsTrue(POIEditorToolWindow.PromptBeforePlayOrBuild(false, _window), "Play goes on");

            Assert.AreEqual("Play With Duplicates", _asked[0].AlternativeLabel);
            Assert.AreEqual(2, rig.childCount, "the stand-ins stay in the scene");
            Assert.AreEqual(before, File.ReadAllText(SourcePath), "nothing saved");
            Assert.IsTrue(Unsaved, "the unsaved edit is still unsaved");
        }

        [Test]
        public void PlayGate_Cancel_StopsPlay_AndChangesNothing()
        {
            WindowOnTempFiles();
            var rig = RigWithMovedMarkerAndUnsavedEdit();
            string before = File.ReadAllText(SourcePath);

            Assert.IsFalse(POIEditorToolWindow.PromptBeforePlayOrBuild(false, _window), "Cancel (= Esc) stops Play");
            Assert.AreEqual(2, rig.childCount);
            Assert.AreEqual(before, File.ReadAllText(SourcePath));
        }

        [Test]
        public void BuildGate_SaveClearAndBuild_SavesBothCopies_ClearsTheRig()
        {
            WindowOnTempFiles();
            var rig = RigWithMovedMarkerAndUnsavedEdit();

            _answer = DecisionAnswer.Confirm;
            Assert.IsTrue(POIEditorToolWindow.PromptBeforePlayOrBuild(isBuild: true, _window));

            Assert.AreEqual("Save, Clear & Build", _asked[0].ConfirmLabel);
            Assert.AreEqual(0, rig.childCount);
            Assert.AreEqual(File.ReadAllText(SourcePath), File.ReadAllText(StreamingPath),
                "the build ships the StreamingAssets copy: it holds the saved work");
            Assert.AreEqual(5f, Read(StreamingPath).pois[0].position.x, 1e-4f);
        }

        [Test]
        public void BuildGate_Cancel_OrEvenAStrayAlternative_StopsTheBuild()
        {
            var rig = Rig("poi_a");
            Assert.IsFalse(POIEditorToolWindow.PromptBeforePlayOrBuild(true, null), "Cancel stops the build");
            _answer = DecisionAnswer.Alternative;
            Assert.IsFalse(POIEditorToolWindow.PromptBeforePlayOrBuild(true, null), "a build never continues uncleared");
            Assert.IsNull(_asked[1].AlternativeLabel, "the build question has no 'continue' button at all");
            Assert.AreEqual(1, rig.childCount);
        }

        [Test]
        public void RigSafety_HasNoOptOut_NoSettingAndNoMenuItem()
        {
            // The old "Rig Safety Prompt on Play/Build" toggle let a build skip the prompt and
            // ship the editor rig's markers. It (and its EditorPrefs key) must stay gone.
            const BindingFlags all = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            Assert.IsNull(typeof(POIEditorToolWindow).GetField("SkipPromptPrefKey", all), "The skip setting must not exist.");

            foreach (var method in typeof(POIEditorToolWindow).GetMethods(all))
                foreach (var attribute in method.GetCustomAttributes(typeof(MenuItem), false))
                    StringAssert.DoesNotContain("Rig Safety", ((MenuItem)attribute).menuItem, method.Name);
        }

        [Test]
        public void RigSafety_WithNoRigInTheScene_NeverAsks_ForPlayOrBuild()
        {
            Assert.IsNull(GameObject.Find("POIEditorRig"), "Premise: no rig in the scene.");
            Assert.IsTrue(POIEditorToolWindow.PromptBeforePlayOrBuild(false));
            Assert.IsTrue(POIEditorToolWindow.PromptBeforePlayOrBuild(true));
            Assert.IsEmpty(_asked);
        }

        // ---------- developer-only switch before a build ----------

        [Test]
        public void DevSwitchBuildCheck_BuildAnyway_LetsTheBuildGoOn_Cancel_StopsIt()
        {
            var findings = new List<string> { "LivingRoom: Add effects demo grid is ON" };

            _answer = DecisionAnswer.Confirm;
            Assert.DoesNotThrow(() => DevFeatureBuildCheck.ConfirmOrStopBuild(findings, batchMode: false), "Build anyway: the build goes on");
            Assert.AreEqual("Build anyway", _asked[0].ConfirmLabel);
            StringAssert.Contains("Add effects demo grid is ON", _asked[0].Message, "the question names the switch");

            _answer = DecisionAnswer.Cancel;
            Assert.Throws<BuildFailedException>(() => DevFeatureBuildCheck.ConfirmOrStopBuild(findings, false), "Cancel stops the build");
        }

        [Test]
        public void DevSwitchBuildCheck_NoFindingsOrBatchMode_AsksNothing()
        {
            DevFeatureBuildCheck.ConfirmOrStopBuild(new List<string>(), false);
            UnityEngine.TestTools.LogAssert.Expect(LogType.Warning, new Regex(@"\[DevFeatureBuildCheck\]"));
            DevFeatureBuildCheck.ConfirmOrStopBuild(new List<string> { "x: y is ON" }, batchMode: true);
            Assert.IsEmpty(_asked, "batch mode cannot ask: it warns instead");
        }
    }
}
