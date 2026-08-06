using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    public partial class POIAuthoringToolWindow
    {
        private void DrawPathRow(string label, ref string path, string extension)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                path = EditorGUILayout.TextField(label, path);
                if (GUILayout.Button("...", GUILayout.Width(30f)))
                {
                    string abs = EditorUtility.OpenFilePanel($"Select {label}", Application.dataPath, extension);
                    if (!string.IsNullOrWhiteSpace(abs))
                    {
                        string rel = AbsoluteToAssetPath(abs);
                        if (!string.IsNullOrWhiteSpace(rel))
                            path = rel;
                        else
                            EditorUtility.DisplayDialog("Invalid path", "Please choose a file inside this Unity project.", "OK");
                    }
                }
            }
        }

        private static string AbsoluteToAssetPath(string absolutePath)
        {
            string normalizedAbs = absolutePath.Replace("\\", "/");
            string normalizedAssets = Application.dataPath.Replace("\\", "/");
            if (!normalizedAbs.StartsWith(normalizedAssets, StringComparison.OrdinalIgnoreCase))
                return null;

            return "Assets" + normalizedAbs.Substring(normalizedAssets.Length);
        }

        private static string AssetPathToResourcesPath(string assetPath)
        {
            if (string.IsNullOrWhiteSpace(assetPath))
                return null;

            string normalized = assetPath.Replace("\\", "/");
            int resourcesIndex = normalized.IndexOf("/Resources/", StringComparison.OrdinalIgnoreCase);
            if (resourcesIndex < 0)
                return null;

            string relative = normalized.Substring(resourcesIndex + "/Resources/".Length);
            if (relative.EndsWith(".asset", StringComparison.OrdinalIgnoreCase))
                relative = relative.Substring(0, relative.Length - ".asset".Length);
            return relative;
        }

        private string GetWallLibraryDirectory()
        {
            string configDir = Path.GetDirectoryName(_configPath)?.Replace("\\", "/");
            if (string.IsNullOrWhiteSpace(configDir) || !configDir.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
                configDir = "Assets/Apps/LivingRoom";

            return configDir + "/MarkerAssets/Resources/MarkerSymbols";
        }

        private static void EnsureAssetDirectory(string assetDirectory)
        {
            string relative = assetDirectory.Replace("\\", "/");
            if (!relative.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
                return;

            string root = Directory.GetParent(Application.dataPath)?.FullName?.Replace("\\", "/");
            if (string.IsNullOrWhiteSpace(root))
                return;

            string absolute = root + "/" + relative;
            if (!Directory.Exists(absolute))
                Directory.CreateDirectory(absolute);

            AssetDatabase.Refresh();
        }

        private static string SanitizeFileName(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return "Wall";

            var invalid = Path.GetInvalidFileNameChars();
            var chars = raw.Trim().ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                if (invalid.Contains(chars[i]) || char.IsWhiteSpace(chars[i]))
                    chars[i] = '_';
            }

            return new string(chars);
        }

        private void EnsureDefaultIconLibraryLoaded()
        {
            if (_defaultIconLibrary == null)
                _defaultIconLibrary = AssetDatabase.LoadAssetAtPath<SpriteKeyLibrary>(DefaultIconLibraryPath);
        }

        private void TryResolveWallIconLibraryFromConfig()
        {
            _wallIconLibrary = null;
            if (_config == null || string.IsNullOrWhiteSpace(_config.marker_icon_library_resources_path))
                return;

            string target = _config.marker_icon_library_resources_path.Trim();
            string[] guids = AssetDatabase.FindAssets("t:SpriteKeyLibrary");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.Equals(AssetPathToResourcesPath(path), target, StringComparison.Ordinal))
                {
                    _wallIconLibrary = AssetDatabase.LoadAssetAtPath<SpriteKeyLibrary>(path);
                    break;
                }
            }
        }
    }
}