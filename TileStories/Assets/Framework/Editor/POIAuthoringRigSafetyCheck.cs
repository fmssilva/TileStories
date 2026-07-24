using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TileStories.Editor
{
    // Warns if a scene is saved or Play Mode is entered while POIAuthoringRig still
    // has objects in it - this almost always means someone forgot to run "Capture
    // Positions to JSON" and then delete the rig, which causes duplicate-looking
    // markers at runtime (the authoring-rig objects plus the properly spawned ones).
    // See _1.2_final_review.md Section 2 for the full history of why this exists.
    [InitializeOnLoad]
    public static class POIAuthoringRigSafetyCheck
    {
        static POIAuthoringRigSafetyCheck()
        {
            EditorSceneManager.sceneSaving += OnSceneSaving;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnSceneSaving(Scene scene, string path)
        {
            CheckForLeftoverRig("saving the scene");
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
                CheckForLeftoverRig("entering Play Mode");
        }

        private static void CheckForLeftoverRig(string action)
        {
            var rig = GameObject.Find("POIAuthoringRig");
            if (rig != null && rig.transform.childCount > 0)
            {
                Debug.LogWarning(
                    $"[POIAuthoringRigSafetyCheck] POIAuthoringRig still has " +
                    $"{rig.transform.childCount} object(s) while {action}. If you've " +
                    "already run 'Capture Positions to JSON', delete these objects " +
                    "now - leaving them in causes duplicate-looking markers at " +
                    "runtime. If you haven't captured yet, ignore this warning.");
            }
        }
    }
}