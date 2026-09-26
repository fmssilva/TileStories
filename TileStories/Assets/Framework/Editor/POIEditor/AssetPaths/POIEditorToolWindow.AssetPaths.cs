using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    public partial class POIEditorToolWindow
    {
        // A Scene Configuration row: labelled text field + "..." browse button.
        // Non-button row -- the same shared row layout applies (transparent indent
        // spacer + width capped to max(MinRowWidth, min(visible panel, MaxRowWidth))):
        // here the TEXT FIELD is the stretchy element, so it gets most of rowWidth
        // and the browse button keeps its fixed 30px. Exposed internal static with
        // out rects so the EditMode render harness can measure the real geometry.
        internal static void DrawPathRow(string label, ref string path, string extension,
            out Rect spacerRect, out Rect fieldRect, out Rect browseRect)
        {
            // Shared row: transparent indent spacer + rowWidth = max(MinRowWidth,
            // min(visible panel width, MaxRowWidth)).
            DrawEditorRow(out float rowWidth, out spacerRect);

            // The TextField is labelled and defaults to filling the panel; cap the
            // whole label+field control to rowWidth minus the browse button (30px)
            // and flow spacing, exactly the same capping recipe as a button row.
            float fieldWidth = Mathf.Max(0f, rowWidth - 36f);
            path = EditorGUILayout.TextField(label, path, GUILayout.Width(fieldWidth), GUILayout.ExpandWidth(false));
            fieldRect = GUILayoutUtility.GetLastRect();

            if (GUILayout.Button("...", GUILayout.Width(30f)))
            {
                string abs = EditorUtility.OpenFilePanel("Select " + label, Application.dataPath, extension);
                if (!string.IsNullOrWhiteSpace(abs))
                {
                    string rel = AbsoluteToAssetPath(abs);
                    if (!string.IsNullOrWhiteSpace(rel))
                        path = rel;
                    else
                        EditorNotice.Queue("Invalid path", "Please choose a file inside this Unity project.");
                }
            }
            browseRect = GUILayoutUtility.GetLastRect();

            EditorRowEnd();
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

        // Same recipe as the icon library above (_2.0_Labels_And_Fonts_Design.md section 2.3),
        // resolved by scanning for a FontKeyLibrary whose own Resources-relative path matches
        // the config's label_font_library_resources_path.
        private void TryResolveWallFontLibraryFromConfig()
        {
            _wallFontLibrary = null;
            if (_config == null || string.IsNullOrWhiteSpace(_config.label_font_library_resources_path))
                return;

            string target = _config.label_font_library_resources_path.Trim();
            string[] guids = AssetDatabase.FindAssets("t:FontKeyLibrary");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.Equals(AssetPathToResourcesPath(path), target, StringComparison.Ordinal))
                {
                    _wallFontLibrary = AssetDatabase.LoadAssetAtPath<FontKeyLibrary>(path);
                    break;
                }
            }
        }

        // The wall's own FontKeyLibrary: the one already assigned, else locate-or-create it next to
        // the wall's icon library (seeded with the framework's 3 fonts -- a wall library REPLACES the
        // framework one at runtime, so it must start with everything the wall keeps,
        // _2.0_Labels_And_Fonts_Design.md section 2.3), and point the config at it. Called only when
        // the developer adds a font, so a wall that never adds one never gets a library asset.
        private FontKeyLibrary EnsureWallFontLibrary()
        {
            if (_wallFontLibrary != null) return _wallFontLibrary;

            string directory = GetWallLibraryDirectory();
            string wallName = _config != null && !string.IsNullOrWhiteSpace(_config.wall_id) ? _config.wall_id : "Wall";
            string assetPath = directory + "/" + SanitizeFileName(wallName) + "_FontLibrary.asset";
            EnsureAssetDirectory(directory);

            _wallFontLibrary = AssetDatabase.LoadAssetAtPath<FontKeyLibrary>(assetPath);
            if (_wallFontLibrary == null)
            {
                _wallFontLibrary = CreateInstance<FontKeyLibrary>();
                var defaultLibrary = AssetDatabase.LoadAssetAtPath<FontKeyLibrary>(DefaultFontLibraryPath);
                if (defaultLibrary != null)
                    _wallFontLibrary.CopyFrom(defaultLibrary);
                AssetDatabase.CreateAsset(_wallFontLibrary, assetPath);
                AssetDatabase.SaveAssets();
            }

            string resourcesPath = AssetPathToResourcesPath(assetPath);
            if (_config != null && !string.IsNullOrWhiteSpace(resourcesPath))
                _config.label_font_library_resources_path = resourcesPath;
            return _wallFontLibrary;
        }

        // Register a TMP Font Asset in the wall's font library (creating the library on first use)
        // and return its key -- the font equivalent of AssignSpriteToLibraryAndGetKey. The same call
        // backs "Add font" in both Labels, Text & Fonts and the hierarchy Marker Label Style window.
        private string AddFontToWallLibraryAndGetKey(TMP_FontAsset font)
        {
            if (font == null) return null;
            var library = EnsureWallFontLibrary();
            Undo.RecordObject(library, "Add Label Font");
            string key = library.EnsureKeyForFont(font);
            EditorUtility.SetDirty(library);
            AssetDatabase.SaveAssets();
            return key;
        }

        // The Font popup's option list: the 3 framework keys plus any extra keys the wall's own
        // FontKeyLibrary defines, so a font added to the wall is selectable everywhere a Font popup
        // exists. Rebuilt on every call (a handful of entries) rather than cached, so a freshly added
        // font shows up on the next repaint with no invalidation to wire.
        private void GetAvailableFontKeyOptions(out string[] keys, out string[] labels)
        {
            var keyList = new List<string>(LabelFontKeyOptions);
            var labelList = new List<string>(LabelFontKeyLabels);

            if (_wallFontLibrary != null)
            {
                foreach (string wallKey in _wallFontLibrary.Keys())
                {
                    if (keyList.Contains(wallKey)) continue;
                    keyList.Add(wallKey);
                    labelList.Add(wallKey + " (wall)");
                }
            }

            keys = keyList.ToArray();
            labels = labelList.ToArray();
        }
    }
}
