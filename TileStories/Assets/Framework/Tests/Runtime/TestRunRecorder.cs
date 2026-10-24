using System;
using System.Collections;
using System.IO;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using UnityEngine.TestTools;

namespace TileStories.Tests
{
    // Developer tool, OFF by default (menu TileStories > Dev > Record Test Runs, remembered per machine): while ON, every test
    // run writes one line per event that can explain Unity jumping to the front -- the application gaining / losing focus, the
    // focused Editor window, Play Mode changes, asset imports, scene loads, the Game view's size, and the start / body / end of
    // every test -- to TestEvidence/Harness/TestRunRecorder.log, and a jump table at the end of the run (TestRunReport).
    // A file, not memory: a PlayMode run reloads the C# domain going in and out of Play Mode, and the log must outlive both.
    // The Editor-side hooks live in TileStories.Editor.Tests (TestRunRecorderHooks); this half is what PlayMode code can reach.
    public static class TestRunRecorder
    {
        public const string SwitchKey = "TileStories.TestRunRecorder";

        // The event kinds of a log line (TestRunReport reads them back)
        public const string RunStart = "RunStart", RunEnd = "RunEnd", Domain = "Domain",
            FocusGained = "FocusGained", FocusLost = "FocusLost", Window = "Window", PlayMode = "PlayMode",
            Import = "Import", Deleted = "Deleted", Moved = "Moved", Scene = "Scene", GameViewSize = "GameViewSize",
            TestStart = "TestStart", BodyStart = "BodyStart", BodyEnd = "BodyEnd", TestEnd = "TestEnd";

        public static string LogPath => TestEvidence.PathFor("Harness", "TestRunRecorder.log");
        public static string ReportPath => TestEvidence.PathFor("Harness", "TestRunRecorder_report.txt");

#if UNITY_EDITOR
        public static bool IsOn
        {
            get => UnityEditor.EditorPrefs.GetBool(SwitchKey, false);
            set => UnityEditor.EditorPrefs.SetBool(SwitchKey, value);
        }
#else
        public static bool IsOn => false;
#endif

        // One log line: local time to the millisecond, the kind, the detail (tabs and line breaks flattened so a line stays one line)
        public static string FormatLine(DateTime time, string kind, string detail) =>
            time.ToString("yyyy-MM-ddTHH:mm:ss.fff") + "\t" + kind + "\t"
            + (detail ?? "").Replace('\t', ' ').Replace('\r', ' ').Replace('\n', ' ');

        // Append one event, only while the switch is ON (OFF: nothing is opened or written)
        public static void Write(string kind, string detail)
        {
            if (!IsOn) return;
            File.AppendAllText(LogPath, FormatLine(DateTime.Now, kind, detail) + "\n");
        }
    }

    // Put on an assembly's no-namespace [SetUpFixture]: that fixture is the top suite of its whole assembly, and the Test
    // Framework collects test actions from a test's parent suites (an [assembly:] action is never seen on the [UnityTest] path).
    // - the outer action brackets [UnitySetUp] / [UnityTearDown] too, the inner one only the test's own body
    // - neither yields a frame (the inner command never skips one, the outer enumerators end at once): a test's timing is the
    //   same with the attribute present, switch ON or OFF
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class RecordEachTestAttribute : NUnitAttribute, ITestAction, IOuterUnityTestAction
    {
        public ActionTargets Targets => ActionTargets.Test;

        // The body starts (after SetUp)
        void ITestAction.BeforeTest(ITest test) => TestRunRecorder.Write(TestRunRecorder.BodyStart, test.FullName);

        // The body ended (before TearDown)
        void ITestAction.AfterTest(ITest test) => TestRunRecorder.Write(TestRunRecorder.BodyEnd, test.FullName);

        // The test starts (before UnitySetUp)
        IEnumerator IOuterUnityTestAction.BeforeTest(ITest test)
        {
            TestRunRecorder.Write(TestRunRecorder.TestStart, test.FullName);
            yield break;
        }

        // The test ended (after UnityTearDown), with its outcome
        IEnumerator IOuterUnityTestAction.AfterTest(ITest test)
        {
            TestRunRecorder.Write(TestRunRecorder.TestEnd, test.FullName + " " + TestContext.CurrentContext.Result.Outcome.Status);
            yield break;
        }
    }
}
