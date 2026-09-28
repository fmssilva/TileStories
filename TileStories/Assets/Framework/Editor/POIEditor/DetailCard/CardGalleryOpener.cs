using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TileStories.Editor
{
    // Detail Card > Card Container > Test > Open Gallery (_3.1 step 12): opens the card's isolated Phase A scene, the gallery, where every block
    // kind, look and content state is shown on a fabricated point (no AR, no wall data). In Play Mode the scene is loaded on top of the running
    // one; in Edit Mode it replaces the open scene -- but never over unsaved changes: the developer is told to save first, with a notice, not a
    // native dialog (a modal dialog would freeze the Editor and every test run). Developer-only: the scene is excluded from Build Settings.
    public static class CardGalleryOpener
    {
        public const string ScenePath = "Assets/Dev/CardGallery/CardGalleryScene.unity";

        public enum Plan { LoadInPlayMode, OpenInEditor, SaveTheOpenSceneFirst }

        // What pressing Open Gallery does, from plain facts (a test calls it with every combination)
        public static Plan PlanFor(bool isPlaying, bool anySceneHasUnsavedChanges) =>
            isPlaying ? Plan.LoadInPlayMode : anySceneHasUnsavedChanges ? Plan.SaveTheOpenSceneFirst : Plan.OpenInEditor;

        // Do it; false when nothing was opened (no gallery scene in the project, or unsaved changes in an open scene)
        public static bool Open()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
            {
                EditorNotice.Queue("Card gallery not found", "The card gallery scene is missing from the project, so there is nothing to open. Reimport the framework's Dev folder.");
                return false;
            }
            switch (PlanFor(Application.isPlaying, AnySceneHasUnsavedChanges()))
            {
                case Plan.LoadInPlayMode:
                    EditorSceneManager.LoadSceneInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
                    return true;
                case Plan.OpenInEditor:
                    EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                    return true;
                default:
                    EditorNotice.Queue("Save the open scene first", "The open scene has unsaved changes and opening the gallery would replace it. Save the scene (Ctrl+S), then press Open Gallery again.");
                    return false;
            }
        }

        private static bool AnySceneHasUnsavedChanges()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
                if (SceneManager.GetSceneAt(i).isDirty) return true;
            return false;
        }
    }
}
