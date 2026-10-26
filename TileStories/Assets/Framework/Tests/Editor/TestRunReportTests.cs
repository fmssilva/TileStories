using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using TileStories.Tests;
using UnityEngine.TestTools;

namespace TileStories.Editor.Tests
{
    // The test-run recorder (TestRunRecorder + TestRunRecorderHooks) and its jump table (TestRunReport): the switch really
    // gates the file, a written line reads back, every test assembly's top suite carries the per-test action, the run
    // boundary is where the Test Framework looks for it, and the table puts each focus gain in the right test and phase
    public class TestRunReportTests
    {
        // A run as the recorder writes it: Play Mode entered, a jump while a test sets up, one in a body, one between two tests
        // right after an import, an older run before it (must be ignored), a torn last line
        private static readonly string[] Log =
        {
            "2026-10-24T10:00:00.000\tRunStart\tfocused=False",
            "2026-10-24T10:00:01.000\tFocusGained\twindow=GameView 'Game' log@1",
            "2026-10-24T10:00:02.000\tRunEnd\tfocused=False",
            "2026-10-24T11:00:00.000\tRunStart\tfocused=False",
            "2026-10-24T11:00:01.000\tPlayMode\tExitingEditMode",
            "2026-10-24T11:00:03.000\tDomain\tloaded, playing=True",
            "2026-10-24T11:00:03.500\tPlayMode\tEnteredPlayMode",
            "2026-10-24T11:00:03.600\tFocusGained\twindow=GameView 'Game' log@100",
            "2026-10-24T11:00:04.000\tTestStart\tA.First",
            "2026-10-24T11:00:04.200\tScene\tloaded Assets/S.unity Single",
            "2026-10-24T11:00:04.300\tFocusGained\twindow=GameView 'Game' log@200",
            "2026-10-24T11:00:04.400\tBodyStart\tA.First",
            "2026-10-24T11:00:05.000\tFocusLost\twindow=GameView 'Game' log@210",
            "2026-10-24T11:00:05.500\tFocusGained\twindow=GameView 'Game' log@220",
            "2026-10-24T11:00:06.000\tBodyEnd\tA.First",
            "2026-10-24T11:00:06.100\tTestEnd\tA.First Passed",
            "2026-10-24T11:00:06.200\tImport\tAssets/Fonts/X TMP.asset",
            "2026-10-24T11:00:06.300\tFocusGained\twindow=InspectorWindow 'Inspector' log@300",
            "2026-10-24T11:00:07.000\tTestStart\tB.Second",
            "2026-10-24T11:00:07.500\tTestEnd\tB.Second Passed",
            "2026-10-24T11:00:20.000\tFocusGained\twindow=none log@400",
            "2026-10-24T11:00:21.000\tRunEnd\tfocused=True",
            "2026-10-24T11:00:2",
        };

        [Test]
        public void TheReport_ReadsOnlyTheLastRun_AndCountsItsTestsAndLosses()
        {
            var report = TestRunReport.Build(Log);
            Assert.AreEqual(new DateTime(2026, 10, 24, 11, 0, 0), report.RunStart, "the LAST RunStart opens the run");
            Assert.AreEqual(new DateTime(2026, 10, 24, 11, 0, 21), report.RunEnd);
            Assert.AreEqual(2, report.Tests);
            Assert.AreEqual(1, report.FocusLosses);
            Assert.AreEqual(5, report.Jumps.Count, "the older run's gain is not this run's");
        }

        [Test]
        public void EachJump_IsPutInTheTestAndPhaseThatWasRunning()
        {
            var jumps = TestRunReport.Build(Log).Jumps;
            Assert.AreEqual("before the first test", jumps[0].Where);
            Assert.AreEqual("in A.First (setup)", jumps[1].Where, "after TestStart, before BodyStart = its UnitySetUp");
            Assert.AreEqual("in A.First (body)", jumps[2].Where);
            Assert.AreEqual("between A.First and B.Second", jumps[3].Where, "a gain between two tests names both");
            Assert.AreEqual("after the last test (B.Second)", jumps[4].Where);
            Assert.AreEqual(TimeSpan.FromSeconds(3.6), jumps[0].SinceRunStart);
            Assert.AreEqual("window=InspectorWindow 'Inspector' log@300", jumps[3].FocusedWindow);
        }

        [Test]
        public void JustBefore_HoldsTheContextEventsOfTheLastThreeSeconds_AndNoTestMarkers()
        {
            var jumps = TestRunReport.Build(Log).Jumps;
            CollectionAssert.AreEqual(new[] { "PlayMode", "Domain", "PlayMode" }, jumps[0].JustBefore.Select(e => e.Kind).ToArray(),
                "ExitingEditMode 2.6 s before, the domain load and EnteredPlayMode -- not the RunStart 3.6 s before");
            CollectionAssert.AreEqual(new[] { "PlayMode", "Scene", "FocusLost", "Import" }, jumps[3].JustBefore.Select(e => e.Kind).ToArray(),
                "from 03.3 s on: EnteredPlayMode, the scene load, the loss, the import");
            Assert.IsTrue(jumps[3].JustBefore.Any(e => e.Kind == "Import" && e.Detail == "Assets/Fonts/X TMP.asset"), "the import 0.1 s before");
            Assert.IsFalse(jumps.SelectMany(j => j.JustBefore).Any(e => e.Kind.StartsWith("Test") || e.Kind.StartsWith("Body")),
                "test markers place a jump, they are not its cause");
            Assert.AreEqual(0, jumps[4].JustBefore.Count, "nothing in the 3 s before the last gain");
            StringAssert.Contains("nothing recorded", TestRunReport.Build(Log).ToText());
        }

        [Test]
        public void ALogWithoutARunStart_SaysTheRecorderWasOff()
        {
            var e = Assert.Throws<ArgumentException>(() => TestRunReport.Build(new[] { "2026-10-24T11:00:01.000\tFocusGained\tx" }));
            StringAssert.Contains("recorder ON", e.Message);
        }

        [Test]
        public void TheSwitch_GatesTheFile_AndALineReadsBack()
        {
            bool wasOn = TestRunRecorder.IsOn;
            try
            {
                long Length() => File.Exists(TestRunRecorder.LogPath) ? new FileInfo(TestRunRecorder.LogPath).Length : 0;
                TestRunRecorder.IsOn = false;
                long before = Length();
                TestRunRecorder.Write("SelfTest", "off");
                Assert.AreEqual(before, Length(), "OFF writes nothing");

                TestRunRecorder.IsOn = true;
                TestRunRecorder.Write("SelfTest", "on\twith a tab\nand a line break");
                var last = TestRunReport.Parse(File.ReadAllLines(TestRunRecorder.LogPath).Last());
                Assert.IsNotNull(last, "the written line parses");
                Assert.AreEqual("SelfTest", last.Kind);
                Assert.AreEqual("on with a tab and a line break", last.Detail, "a detail stays one line");
                Assert.That(last.Time, Is.EqualTo(DateTime.Now).Within(TimeSpan.FromSeconds(5)));
                StringAssert.StartsWith(TestEvidence.Root, TestRunRecorder.LogPath, "the log is test evidence, outside Assets/ (4.2.4)");
            }
            finally { TestRunRecorder.IsOn = wasOn; }
        }

        // Every test assembly's no-namespace SetUpFixture is its top suite: without the attribute there, that assembly's tests
        // would be missing from the table (a new test assembly adds its own fixture AND the attribute)
        [TestCase("TileStories.Tests.Runtime", "FixedFrameForTheRun")]
        [TestCase("TileStories.LivingRoom.Tests.Runtime", "LivingRoomFixedFrameForTheRun")]
        [TestCase("TileStories.Editor.Tests", "TestDialogGuard")]
        public void EveryTestAssemblysTopSuite_CarriesRecordEachTest(string assembly, string fixture)
        {
            var asm = AppDomain.CurrentDomain.GetAssemblies().Single(a => a.GetName().Name == assembly);
            var type = asm.GetType(fixture);
            Assert.IsNotNull(type, fixture + " sits outside any namespace in " + assembly);
            Assert.IsTrue(type.IsDefined(typeof(SetUpFixtureAttribute), false), fixture + " is a SetUpFixture");
            Assert.IsTrue(type.IsDefined(typeof(RecordEachTestAttribute), false), fixture + " carries RecordEachTest");
        }

        // The Test Framework collects assembly-level PrebuildSetup / PostBuildCleanup only from assemblies whose metadata
        // references UnityEditor.TestRunner: the run's one hook sits in such an assembly and names TestRunBoundary for both ends
        [Test]
        public void TheRunsHook_IsWhereTheTestFrameworkLooksForIt()
        {
            var asm = typeof(TestRunBoundary).Assembly;
            Assert.IsTrue(asm.GetReferencedAssemblies().Any(a => a.Name == "UnityEditor.TestRunner"),
                "the finder skips an assembly that does not reference UnityEditor.TestRunner");
            bool Names(Type attribute) => System.Reflection.CustomAttributeData.GetCustomAttributes(asm).Any(a => a.AttributeType == attribute
                && a.ConstructorArguments.Count == 1 && (Type)a.ConstructorArguments[0].Value == typeof(TestRunBoundary));
            Assert.IsTrue(Names(typeof(PrebuildSetupAttribute)), "the start is marked before the run enters Play Mode");
            Assert.IsTrue(Names(typeof(PostBuildCleanupAttribute)), "the end after it leaves");
        }

        // A dynamic font asset that a test drew new glyphs into is given back as it is on disk at the run's end, so no later save writes
        // it under Assets/ (TMP's own dynamic fallback font: real glyphs added for real, the file's bytes compared)
        [Test]
        public void TheRunsEnd_GivesADynamicFontAssetBackAsOnDisk()
        {
            const string path = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF - Fallback.asset";
            byte[] onDisk = File.ReadAllBytes(path);
            var font = UnityEditor.AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>(path);
            Assert.IsNotNull(font, "precondition: TMP's dynamic fallback font exists");
            Assert.AreEqual(TMPro.AtlasPopulationMode.Dynamic, font.atlasPopulationMode, "precondition: it is dynamic");
            int glyphsOnDisk = font.characterTable.Count;

            // - glyphs the asset does not hold yet, the way a test's text adds them while it renders
            string fresh = new string("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz".Where(c => !font.HasCharacter(c)).ToArray());
            Assume.That(fresh.Length, Is.GreaterThan(0), "the fallback font already holds every candidate glyph (nothing to add)");
            font.TryAddCharacters(fresh, out _);
            Assert.Greater(font.characterTable.Count, glyphsOnDisk, "precondition: glyphs were added in memory");
            Assert.IsTrue(UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path).Any(UnityEditor.EditorUtility.IsDirty),
                "precondition: adding glyphs leaves the asset dirty (what a later save would write)");

            Assert.GreaterOrEqual(TestRunBoundary.DiscardDynamicFontChanges(), 1, "the dirty dynamic font is discarded");

            var reloaded = UnityEditor.AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>(path);
            Assert.AreEqual(glyphsOnDisk, reloaded.characterTable.Count, "the asset is back to its glyphs on disk");
            Assert.IsFalse(UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path).Any(UnityEditor.EditorUtility.IsDirty), "nothing left to save");
            CollectionAssert.AreEqual(onDisk, File.ReadAllBytes(path), "the file itself was never written");
        }

        // The two Editor log lines an agent's watcher waits on: fixed prefixes, and where Unity was
        [Test]
        public void TheBoundaryLines_StartWithTheirFixedPrefix_AndSayWhereUnityWas()
        {
            Assert.AreEqual("[TestRun] ended (Unity behind)", TestRunBoundary.Line(TestRunBoundary.EndLine, false));
            Assert.AreEqual("[TestRun] started (Unity in front)", TestRunBoundary.Line(TestRunBoundary.StartLine, true));
        }

        // A real run of this very suite went through the hook: its RunStart is the last one in the log (while the switch is ON)
        [Test]
        public void ThisRun_StartedThroughTheHook_WhenTheRecorderIsOn()
        {
            if (!TestRunRecorder.IsOn) Assert.Ignore("the recorder is OFF on this machine: nothing to read");
            var start = File.ReadAllLines(TestRunRecorder.LogPath).Select(TestRunReport.Parse).Last(e => e != null && e.Kind == TestRunRecorder.RunStart);
            Assert.That(start.Time, Is.GreaterThan(DateTime.Now.AddMinutes(-30)), "this run's RunStart was written before its first test");
        }
    }
}
