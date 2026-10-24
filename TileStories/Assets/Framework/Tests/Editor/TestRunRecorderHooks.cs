using System.IO;
using TileStories.Tests;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TileStories.Editor.Tests
{
    // The Editor half of TestRunRecorder: subscribes to the Editor events after every domain load and writes them (the
    // recorder itself drops every line while its switch is OFF), watches imports, and marks the run's start and end --
    // called by the run's one hook, TestRunBoundary -- writing TestRunReport's jump table when a run ends
    [InitializeOnLoad]
    public static class TestRunRecorderHooks
    {
        private const string MenuPath = "TileStories/Dev/Record Test Runs";

        static TestRunRecorderHooks()
        {
            EditorApplication.focusChanged += active =>
                TestRunRecorder.Write(active ? TestRunRecorder.FocusGained : TestRunRecorder.FocusLost, FocusedWindow() + " log@" + EditorLogLength());
            // - with the caller: a window that takes focus during a run names the code that gave it (a jump's cause, not only its time)
            EditorWindow.windowFocusChanged += () =>
            {
                if (TestRunRecorder.IsOn) TestRunRecorder.Write(TestRunRecorder.Window, FocusedWindow() + " from " + Caller());
            };
            EditorApplication.playModeStateChanged += state => TestRunRecorder.Write(TestRunRecorder.PlayMode, state.ToString());
            SceneManager.sceneLoaded += (scene, mode) => TestRunRecorder.Write(TestRunRecorder.Scene, "loaded " + scene.path + " " + mode);
            EditorSceneManager.sceneOpened += (scene, mode) => TestRunRecorder.Write(TestRunRecorder.Scene, "opened " + scene.path + " " + mode);
            TestRunRecorder.Write(TestRunRecorder.Domain, "loaded, playing=" + EditorApplication.isPlayingOrWillChangePlaymode);
        }

        // The window Unity's keys go to right now (type and title), or none
        private static string FocusedWindow()
        {
            var window = EditorWindow.focusedWindow;
            return window == null ? "window=none" : "window=" + window.GetType().Name + " '" + window.titleContent.text + "'";
        }

        // The managed frames that led here, newest first, without the recorder's own and the event plumbing's (" < " between frames)
        private static string Caller()
        {
            var frames = new System.Diagnostics.StackTrace(2, false).GetFrames() ?? new System.Diagnostics.StackFrame[0];
            var names = new System.Collections.Generic.List<string>();
            foreach (var frame in frames)
            {
                var method = frame.GetMethod();
                if (method?.DeclaringType == null) continue;
                names.Add(method.DeclaringType.Name + "." + method.Name);
                if (names.Count == 12) break;
            }
            return names.Count == 0 ? "native" : string.Join(" < ", names);
        }

        // Where the Editor log stands, so a jump can be matched to the log lines written just before it
        private static long EditorLogLength()
        {
            try { return new FileInfo(Application.consoleLogPath).Length; }
            catch (IOException) { return -1; }
        }

        // The switch: one menu entry, ticked while recording
        [MenuItem(MenuPath)]
        private static void Toggle() => TestRunRecorder.IsOn = !TestRunRecorder.IsOn;

        [MenuItem(MenuPath, true)]
        private static bool ToggleValidate()
        {
            Menu.SetChecked(MenuPath, TestRunRecorder.IsOn);
            return true;
        }

        // Every import, deletion and move, as the AssetDatabase reports it
        private sealed class ImportWatcher : AssetPostprocessor
        {
            private static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
            {
                if (!TestRunRecorder.IsOn) return;
                foreach (string path in imported) TestRunRecorder.Write(TestRunRecorder.Import, path);
                foreach (string path in deleted) TestRunRecorder.Write(TestRunRecorder.Deleted, path);
                for (int i = 0; i < moved.Length; i++) TestRunRecorder.Write(TestRunRecorder.Moved, movedFrom[i] + " -> " + moved[i]);
            }
        }

        // Mark the start of a run (before it enters Play Mode)
        public static void MarkRunStart() =>
            TestRunRecorder.Write(TestRunRecorder.RunStart, "focused=" + UnityEditorInternal.InternalEditorUtility.isApplicationActive);

        // Mark the end (after it left Play Mode) and write the jump table of the run that just ended
        public static void MarkRunEnd()
        {
            if (!TestRunRecorder.IsOn) return;
            TestRunRecorder.Write(TestRunRecorder.RunEnd, "focused=" + UnityEditorInternal.InternalEditorUtility.isApplicationActive);
            var report = TestRunReport.Build(File.ReadAllLines(TestRunRecorder.LogPath));
            File.WriteAllText(TestRunRecorder.ReportPath, report.ToText());
            Debug.Log("[TestRunRecorder] " + report.Summary + " -> " + TestRunRecorder.ReportPath);
        }
    }
}
