using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TileStories.Editor
{
    // Intercepts Play Mode entry and scene saving when POIAuthoringRig still
    // has child objects. For Play Mode, delegates to
    // POIAuthoringToolWindow.PromptBeforePlayOrBuild which shows an interactive
    // dialog that can save positions, clear the rig, and either proceed or cancel.
    // For scene saving, logs a non-blocking warning (the developer may be
    // mid-placement and should not be blocked from saving).
    // Build-time checks live in POIAuthoringRigBuildCheck.
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
            var rig = GameObject.Find("POIAuthoringRig");
            if (rig != null && rig.transform.childCount > 0)
            {
                Debug.LogWarning(
                    $"[POIAuthoringRigSafetyCheck] POIAuthoringRig still has " +
                    $"{rig.transform.childCount} object(s) while saving the scene. " +
                    "Clear the rig after capturing positions to JSON.");
            }
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingEditMode)
                return;

            // Delegate to the tool window's dialog. If the user cancels,
            // abort the Play Mode transition.
            if (!POIAuthoringToolWindow.PromptBeforePlayOrBuild(isBuild: false))
            {
                EditorApplication.isPlaying = false;
            }
        }
    }
}
