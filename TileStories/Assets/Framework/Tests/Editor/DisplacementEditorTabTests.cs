using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace TileStories.Editor.Tests
{
    // Global Scene > Displacement in the real POIEditorToolWindow (_2.5): the (i) help and Test guide contract
    // (ASCII, app-agnostic, Editor Tab controls only, every drawn row and every option explained, nothing the
    // runtime contradicts), undo/redo of every field through the window's real history, the live Play Mode
    // fingerprints, the real OnGUI in every branch, and the Live Displacement Readout's wording.
    public class DisplacementEditorTabTests
    {
        private const BindingFlags Static = BindingFlags.NonPublic | BindingFlags.Static;
        private const BindingFlags Instance = BindingFlags.NonPublic | BindingFlags.Instance;

        private static string Text(string name)
        {
            var f = typeof(POIEditorToolWindow).GetField(name, Static);
            Assert.IsNotNull(f, name + " must exist");
            return (string)f.GetValue(null);
        }

        private static string[] Options(string name) => (string[])typeof(POIEditorToolWindow).GetField(name, Static).GetValue(null);

        private static readonly string[] HelpNames =
        {
            "DisplacementEnabledHelp", "DisplacementOverlapHelp", "DisplacementTargetHelp", "DisplacementAlgorithmHelp",
            "DisplacementTiebreakHelp", "DisplacementMaxMoveHelp", "LeaderLinesEnabledHelp", "LeaderLineStyleHelp",
            "LeaderLineMinLengthHelp", "LeaderLineWidthHelp", "LeaderLineOpacityHelp", "DisplacementDemoHelp",
            "DisplacementDemoGroupHelp", "DisplacementDemoSpreadHelp", "DisplacementDemoDistanceHelp", "DisplacementDemoLabelsHelp",
            "DisplacementDemoReferenceHelp", "DisplacementDemoRunLodHelp", "DisplacementReadoutHelp",
        };

        private static readonly string[] GuideNames = { "DisplacementSceneTestGuide", "DisplacementPlaymodeTestGuide", "DisplacementDeviceTestGuide" };

        private static string SectionSource()
        {
            string path = Directory.GetFiles(Application.dataPath, "POIEditorToolWindow.Displacement.cs", SearchOption.AllDirectories).Single();
            return File.ReadAllText(path);
        }

        // ---- help and guides ----

        [Test]
        public void EveryHelpAndGuide_IsAscii_AppAgnostic_AndNamesNoCodeOrDoc()
        {
            // - code names, doc names, a wall's real POIs, raw saved option values, and claims the runtime disproves
            string[] forbidden =
            {
                ".cs", ".md", "LODController", "WallSession", "MarkerOverlapResolver", "Evaluate(", "step 8", "LivingRoom",
                "lamp", "painting", "label_only", "fixed_axis", "force_directed", "candidate_position", "lower_priority_only",
                "side_by_side", "deferred", "Relaxation", "density radius",
            };
            foreach (string name in HelpNames.Concat(GuideNames))
            {
                string text = Text(name);
                Assert.IsFalse(string.IsNullOrWhiteSpace(text), name);
                Assert.IsTrue(text.All(c => c < 128), name + " must be ASCII");
                foreach (string bad in forbidden)
                    Assert.IsFalse(text.IndexOf(bad, StringComparison.OrdinalIgnoreCase) >= 0, name + " must not mention '" + bad + "'");
            }
        }

        [Test]
        public void PlaymodeGuide_ExplainsEveryDrawnRow_AndEveryOption_OneBlockPerSubSection()
        {
            string guide = Text("DisplacementPlaymodeTestGuide");
            foreach (string block in new[] { "SETUP", "OVERLAP DETECTION", "RESOLUTION", "LEADER LINES", "DEMO CONTROLS" })
                StringAssert.Contains(block + "\n", guide, "one block per sub-section");

            // - every labelled row the section really draws (read from its source, so a new row cannot be forgotten)
            var drawn = Regex.Matches(SectionSource(), "Draw(?:Toggle|Popup|Slider|IntSlider|Scalar|Int)Field\\(\"([^\"]+)\"")
                .Cast<Match>().Select(m => m.Groups[1].Value).Distinct().ToList();
            Assert.GreaterOrEqual(drawn.Count, 15, "precondition: the section's rows were found");
            foreach (string label in drawn)
                StringAssert.Contains(label, guide, "the Playmode guide must say what '" + label + "' does");
            StringAssert.Contains("Live Displacement Readout", guide);

            foreach (string arrays in new[] { "DisplaceTargetLabels", "DisplacementAlgorithmLabels", "DisplacementTiebreakLabels", "LeaderLineStyleLabels", "DisplacementDemoReferenceLabels" })
                foreach (string option in Options(arrays))
                    StringAssert.Contains(option, guide + Text("DisplacementDemoReferenceHelp"), "option '" + option + "' is explained");
        }

        [Test]
        public void SceneGuide_SaysPlayModeIsNeeded_DeviceGuide_TellsToUntickTheDemo()
        {
            StringAssert.Contains("Not possible in Scene test", Text("DisplacementSceneTestGuide"));
            StringAssert.Contains("untick it before a normal build", Text("DisplacementDeviceTestGuide"));
            StringAssert.Contains("Developer-only", Text("DisplacementDemoHelp"));
        }

        [Test]
        public void EveryOptionArray_MatchesTheValuesTheRuntimeUnderstands()
        {
            CollectionAssert.AreEqual(new[] { "label_only", "marker", "both" }, Options("DisplaceTargetOptions"));
            CollectionAssert.AreEqual(new[] { "fixed_axis", "candidate_position", "force_directed" }, Options("DisplacementAlgorithmOptions"));
            CollectionAssert.AreEqual(new[] { "symmetric", "lower_priority_only" }, Options("DisplacementTiebreakOptions"));
            CollectionAssert.AreEqual(new[] { "straight", "dashed", "elbow" }, Options("LeaderLineStyleOptions"));
            Assert.AreEqual(DisplacementDemoLayout.ReferenceModes.Length, Options("DisplacementDemoReferenceLabels").Length);
            foreach (string arrays in new[] { "DisplaceTarget", "DisplacementAlgorithm", "DisplacementTiebreak", "LeaderLineStyle" })
                Assert.AreEqual(Options(arrays + "Options").Length, Options(arrays + "Labels").Length, arrays + ": one label per option");
        }

        // ---- undo / redo ----

        private static IEnumerable<FieldInfo> Fields(Type t) => t.GetFields(BindingFlags.Public | BindingFlags.Instance);

        // A different, valid value for any displacement field
        private static object Changed(FieldInfo f, object current)
        {
            if (f.FieldType == typeof(bool)) return !(bool)current;
            if (f.FieldType == typeof(int)) return (int)current + 1;
            if (f.FieldType == typeof(float)) return (float)current + 0.5f;
            if (f.FieldType == typeof(string))
            {
                string[] pool = { "marker", "fixed_axis", "lower_priority_only", "dashed", "overlay" };
                return pool.First(v => v != (string)current);
            }
            throw new AssertionException("no test value for " + f.Name + " (" + f.FieldType + ")");
        }

        [Test]
        public void EveryDisplacementAndDemoField_UndoAndRedo_ThroughTheRealWindowHistory()
        {
            var window = ScriptableObject.CreateInstance<POIEditorToolWindow>();
            try
            {
                var t = typeof(POIEditorToolWindow);
                var configField = t.GetField("_config", Instance);
                var initHistory = t.GetMethod("InitializeConfigHistory", Instance);
                var mutate = t.GetMethod("DrawConfigMutationScope", Instance);
                var undo = t.GetMethod("UndoConfigChange", Instance);
                var redo = t.GetMethod("RedoConfigChange", Instance);

                var blocks = new (string name, Func<WallConfigData, object> get)[]
                {
                    ("displacement_settings", c => c.displacement_settings),
                    ("displacement_demo", c => c.displacement_demo),
                };
                int checkedFields = 0;
                foreach (var (blockName, get) in blocks)
                    foreach (var f in Fields(get(new WallConfigData()).GetType()))
                    {
                        var config = new WallConfigData { wall_id = "undo" };
                        configField.SetValue(window, config);
                        initHistory.Invoke(window, null);

                        object original = f.GetValue(get(config));
                        object changed = Changed(f, original);
                        mutate.Invoke(window, new object[] { (Action)(() => f.SetValue(get(config), changed)), false });

                        undo.Invoke(window, null);
                        Assert.AreEqual(original, f.GetValue(get((WallConfigData)configField.GetValue(window))), blockName + "." + f.Name + ": undo restores it");
                        redo.Invoke(window, null);
                        Assert.AreEqual(changed, f.GetValue(get((WallConfigData)configField.GetValue(window))), blockName + "." + f.Name + ": redo re-applies it");
                        checkedFields++;
                    }
                Assert.GreaterOrEqual(checkedFields, 18, "every displacement and demo field was checked");
            }
            finally { UnityEngine.Object.DestroyImmediate(window); }
        }

        // ---- live Play Mode ----

        [Test]
        public void LiveFingerprints_ChangeForEveryFieldOfTheirOwnBlock_AndNothingElse()
        {
            var settingsApplier = new LivePlayModeDisplacementApplier();
            var demoApplier = new LivePlayModeDisplacementDemoApplier();
            var baseline = new WallConfigData();
            string s0 = settingsApplier.Fingerprint(baseline), d0 = demoApplier.Fingerprint(baseline);

            foreach (var f in Fields(typeof(DisplacementSettings)))
            {
                var c = new WallConfigData();
                f.SetValue(c.displacement_settings, Changed(f, f.GetValue(c.displacement_settings)));
                Assert.AreNotEqual(s0, settingsApplier.Fingerprint(c), "displacement_settings." + f.Name + " is pushed live");
                Assert.AreEqual(d0, demoApplier.Fingerprint(c), "a settings edit never rebuilds the demo");
            }
            foreach (var f in Fields(typeof(DisplacementDemoSettings)))
            {
                var c = new WallConfigData();
                f.SetValue(c.displacement_demo, Changed(f, f.GetValue(c.displacement_demo)));
                Assert.AreNotEqual(d0, demoApplier.Fingerprint(c), "displacement_demo." + f.Name + " is pushed live");
                Assert.AreEqual(s0, settingsApplier.Fingerprint(c), "a demo edit never re-applies the settings");
            }

            var lodEdit = new WallConfigData();
            lodEdit.lod_settings.enabled = !lodEdit.lod_settings.enabled;
            Assert.AreEqual(s0, settingsApplier.Fingerprint(lodEdit), "another domain's edit is ignored");
            Assert.AreEqual(d0, demoApplier.Fingerprint(lodEdit));

            var registered = typeof(LivePlayModeConfigPush).GetMethod("CreateDispatcher", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public)
                .Invoke(null, null);
            var appliers = (IEnumerable<ILivePlayModeApplier>)registered.GetType()
                .GetFields(Instance | BindingFlags.Public).First(f => typeof(IEnumerable<ILivePlayModeApplier>).IsAssignableFrom(f.FieldType))
                .GetValue(registered);
            Assert.IsTrue(appliers.Any(a => a is LivePlayModeDisplacementApplier), "the settings applier is registered");
            Assert.IsTrue(appliers.Any(a => a is LivePlayModeDisplacementDemoApplier), "the demo applier is registered");
        }

        // ---- the real OnGUI ----

        private sealed class DisplacementHarness : EditorWindow
        {
            public static bool DidDraw;
            public static Exception Thrown;
            public static bool Enabled, LeaderLines, Demo;

            private void OnGUI()
            {
                DidDraw = true;
                Thrown = null;
                var window = CreateInstance<POIEditorToolWindow>();
                try
                {
                    var config = new WallConfigData();
                    config.displacement_settings.enabled = Enabled;
                    config.displacement_settings.leader_lines_enabled = LeaderLines;
                    config.displacement_demo.enabled = Demo;
                    typeof(POIEditorToolWindow).GetField("_config", Instance).SetValue(window, config);
                    try { typeof(POIEditorToolWindow).GetMethod("DrawGlobalDisplacementSection", Instance).Invoke(window, null); }
                    catch (TargetInvocationException ex) { Thrown = ex.InnerException ?? ex; }
                }
                finally { DestroyImmediate(window); }
            }
        }

        [UnityTest]
        public IEnumerator Section_DrawsWithoutThrowing_InEveryBranch(
            [Values(true, false)] bool enabled, [Values(true, false)] bool leaderLines, [Values(true, false)] bool demo)
        {
            DisplacementHarness.Enabled = enabled;
            DisplacementHarness.LeaderLines = leaderLines;
            DisplacementHarness.Demo = demo;
            DisplacementHarness.DidDraw = false;
            var window = EditorWindow.GetWindow<DisplacementHarness>(true, "DisplacementHarness");
            window.position = new Rect(0f, 0f, 600f, 900f);
            window.Repaint();
            for (int i = 0; i < 60 && !DisplacementHarness.DidDraw; i++)
                yield return null;
            window.Close();
            Assert.IsTrue(DisplacementHarness.DidDraw, "the harness OnGUI ran");
            Assert.IsNull(DisplacementHarness.Thrown, "DrawGlobalDisplacementSection must not throw: " + DisplacementHarness.Thrown);
        }

        // ---- readout ----

        [Test]
        public void LiveReadout_PrintsEveryNumberOnItsOwnLine()
        {
            var text = (string)typeof(POIEditorToolWindow).GetMethod("DisplacementLiveReadout", Static).Invoke(null, new object[]
            {
                new DisplacementStats { Candidates = 13, Groups = 3, Moved = 10, LabelsHidden = 2, LeaderLines = 9, LargestMovePx = 58.4f }
            });
            var lines = text.Split('\n');
            Assert.AreEqual(5, lines.Length);
            Assert.AreEqual("Markers looked at: 13", lines[0]);
            Assert.AreEqual("Crowded groups: 3", lines[1]);
            Assert.AreEqual("Moved: 10 (longest move 58 px)", lines[2]);
            Assert.AreEqual("Labels hidden by Max Move: 2", lines[3]);
            Assert.AreEqual("Leader lines: 9", lines[4]);
        }
    }
}
