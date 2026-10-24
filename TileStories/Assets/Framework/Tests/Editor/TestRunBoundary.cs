using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.TestTools;

// The run's one hook. The Test Framework runs an [assembly:] PrebuildSetup before it enters Play Mode and a PostBuildCleanup
// after it leaves, for EVERY run (EditMode too, any filter), collected from each loaded assembly whose metadata references
// UnityEditor.TestRunner -- this one -- and it allows ONE of each per assembly
[assembly: PrebuildSetup(typeof(TileStories.Editor.Tests.TestRunBoundary))]
[assembly: PostBuildCleanup(typeof(TileStories.Editor.Tests.TestRunBoundary))]

namespace TileStories.Editor.Tests
{
    // Marks both ends of every test run: one Editor log line each (always), and the recorder's RunStart / RunEnd (while
    // TestRunRecorder is ON); at the end it also leaves no dynamic font asset dirty (DiscardDynamicFontChanges). The log lines are what an agent waits on while a run goes in the background: asking the MCP
    // server (get_test_job) during a run raises Unity to the front (its focus nudge, 40-testing 4.2.5), watching the log does not.
    public sealed class TestRunBoundary : IPrebuildSetup, IPostBuildCleanup
    {
        public const string StartLine = "[TestRun] started", EndLine = "[TestRun] ended";

        // Before the run enters Play Mode
        public void Setup()
        {
            TestRunRecorderHooks.MarkRunStart();
            Debug.Log(Line(StartLine, InternalEditorUtility.isApplicationActive));
        }

        // After the run left Play Mode
        public void Cleanup()
        {
            int fonts = DiscardDynamicFontChanges();
            TestRunRecorderHooks.MarkRunEnd();
            Debug.Log(Line(EndLine, InternalEditorUtility.isApplicationActive) + (fonts > 0 ? ", " + fonts + " dynamic font asset(s) left as on disk" : ""));
        }

        // A dynamic TMP font asset adds every glyph a test draws to its atlas IN MEMORY and turns dirty; the next save anywhere (the
        // next run's own pre-run save) then rewrote it under Assets/ (2026-10-24: Fraunces-SemiBold TMP.asset, written one second before
        // a run started). Unloading the dirty ones gives them back as they are on disk -- a dynamic atlas regrows on demand -- so a
        // run leaves no asset to write. Returns how many were discarded.
        public static int DiscardDynamicFontChanges()
        {
            int discarded = 0;
            foreach (var font in Resources.FindObjectsOfTypeAll<TMP_FontAsset>())
            {
                if (font == null || font.atlasPopulationMode != AtlasPopulationMode.Dynamic || !AssetDatabase.Contains(font)) continue;
                string path = AssetDatabase.GetAssetPath(font);
                var parts = AssetDatabase.LoadAllAssetsAtPath(path);
                if (!parts.Any(EditorUtility.IsDirty)) continue;
                // - the atlas texture and the material are sub-assets of the same file: each part goes back to its disk state
                foreach (var part in parts)
                    if (!(part is GameObject) && !(part is Component)) Resources.UnloadAsset(part);
                discarded++;
            }
            return discarded;
        }

        // A boundary line, saying where Unity was (a run that ends "in front" was not a background run all the way)
        public static string Line(string boundary, bool unityActive) => boundary + " (Unity " + (unityActive ? "in front" : "behind") + ")";
    }
}
