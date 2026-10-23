using System.IO;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    // The theme asset's Inspector (_3.3 R12.1): the values as Unity draws them, then whether the generated stylesheet still matches them
    // and the one button that rewrites it (ThemeUssGenerator). A theme edit is not on screen until it is regenerated -- the stale test
    // says so in every run.
    [CustomEditor(typeof(ThemeAsset))]
    public sealed class ThemeAssetEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var theme = (ThemeAsset)target;
            string path = ThemeUssGenerator.PathFor(theme);
            EditorGUILayout.Space();
            bool upToDate = IsUpToDate(theme, path);
            EditorGUILayout.HelpBox(upToDate
                    ? path + " matches these values."
                    : path + " does not match these values yet: screens still draw the old ones. Press Regenerate USS.",
                upToDate ? MessageType.Info : MessageType.Warning);
            if (GUILayout.Button(new GUIContent("Regenerate USS", "Write " + path + " from this asset (every UI Toolkit screen reads it)")))
                ThemeUssGenerator.Write(theme);
        }

        private static bool IsUpToDate(ThemeAsset theme, string path)
        {
            if (!File.Exists(path)) return false;
            try { return File.ReadAllText(path).Replace("\r\n", "\n") == ThemeUssGenerator.Generate(theme, AssetDatabase.GetAssetPath(theme)); }
            catch (System.ArgumentException) { return false; }
        }
    }
}
