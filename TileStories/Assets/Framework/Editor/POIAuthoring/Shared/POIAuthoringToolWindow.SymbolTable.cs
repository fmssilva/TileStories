using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    public partial class POIAuthoringToolWindow
    {
        private void DrawWallIconLibrarySelector()
        {
            EnsureDefaultIconLibraryLoaded();

            // Auto-create the wall library on first sprite drag (section 13.2).
            // The library is managed internally -- no ObjectField shown to the
            // developer since it is always auto-created/assigned.
            if (_wallIconLibrary == null)
                CreateOrAssignWallIconLibrary();

            if (_wallIconLibrary != null)
            {
                string assetPath = AssetDatabase.GetAssetPath(_wallIconLibrary);
                string resourcesPath = AssetPathToResourcesPath(assetPath);
                if (string.IsNullOrWhiteSpace(resourcesPath))
                {
                    EditorGUILayout.HelpBox("Wall icon library must be inside a Resources folder for runtime loading.", MessageType.Warning);
                }
                else
                {
                    // Resources path is auto-populated -- assign silently, no label needed.
                    _config.marker_icon_library_resources_path = resourcesPath;
                }
            }

            // Demystify the wall icon library and the curated symbol picker (sections 13.2, 14.7).
            EditorGUILayout.HelpBox(
                "Symbols are Sprite assets. To add a new one: import a PNG anywhere under this wall's " +
                "MarkerAssets/ folder -- it auto-configures as a Sprite (2D and UI) with alpha and no " +
                "mipmaps, so no manual importer step is needed (section 14.9). Then drag it into any " +
                "Symbol field below -- it registers automatically. A 'Choose existing' button next to each " +
                "picker lets you pick from this wall's symbols and the framework defaults without the full " +
                "project noise. PNG recommended (carries alpha; JPG does not). A crisp vector look is also " +
                "fine if Unity's Vector Graphics / built-in SVG import is available. Importing a " +
                "third-party icon pack is ordinary Unity usage -- no special support required on this side.",
                MessageType.Info);

            // Column header note on the object-picker noise.
            EditorGUILayout.LabelField("The ObjectField shows every Sprite in the project -- use 'Choose existing' for a curated list.", EditorStyles.wordWrappedMiniLabel);
        }

        private void CreateOrAssignWallIconLibrary()
        {
            string directory = GetWallLibraryDirectory();
            string wallName = _config != null && !string.IsNullOrWhiteSpace(_config.wall_id) ? _config.wall_id : "Wall";
            string fileName = SanitizeFileName(wallName) + "_IconLibrary.asset";
            string assetPath = directory + "/" + fileName;

            EnsureAssetDirectory(directory);

            var existing = AssetDatabase.LoadAssetAtPath<SpriteKeyLibrary>(assetPath);
            if (existing != null)
            {
                _wallIconLibrary = existing;
            }
            else
            {
                var created = CreateInstance<SpriteKeyLibrary>();
                if (_defaultIconLibrary != null)
                    created.CopyFrom(_defaultIconLibrary);

                AssetDatabase.CreateAsset(created, assetPath);
                AssetDatabase.SaveAssets();
                _wallIconLibrary = created;
            }

            string resourcesPath = AssetPathToResourcesPath(assetPath);
            if (_config != null && !string.IsNullOrWhiteSpace(resourcesPath))
                _config.marker_icon_library_resources_path = resourcesPath;

            _hasUnsavedChanges = true;
            EditorUtility.SetDirty(this);
            Repaint();
        }

        private string AssignSpriteToLibraryAndGetKey(Sprite sprite, string suggestedName)
        {
            if (sprite == null)
                return null;

            var library = GetEditableIconLibrary();
            if (library == null)
                return null;

            Undo.RecordObject(library, "Assign Marker Symbol");
            string key = library.EnsureKeyForSprite(sprite, suggestedName);
            EditorUtility.SetDirty(library);
            AssetDatabase.SaveAssets();
            return key;
        }

        private Sprite ResolveSpriteForKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return null;

            var wall = _wallIconLibrary != null ? _wallIconLibrary.Get(key) : null;
            if (wall != null)
                return wall;

            EnsureDefaultIconLibraryLoaded();
            return _defaultIconLibrary != null ? _defaultIconLibrary.Get(key) : null;
        }

        private SpriteKeyLibrary GetEditableIconLibrary()
        {
            if (_wallIconLibrary != null)
                return _wallIconLibrary;

            EnsureDefaultIconLibraryLoaded();
            return _defaultIconLibrary;
        }

        // Shared symbol-table renderer for both category and badge sections (section 13.2).
        // Driven by delegates rather than an interface hierarchy — simpler for editor-only GUI code,
        // no serialization constraints to satisfy.
        private void DrawSymbolTable<T>(
            List<T> entries,
            Func<T> createNew,
            Func<T, string> getPrimaryLabel,
            Action<T, string> setPrimaryLabel,
            Func<T, string> getIconKey,
            Action<T, string> setIconKey,
            Func<T, string> getColorHex,
            Action<T, string> setColorHex,
            Func<T, string> getDetails,
            Action<T, string> setDetails,
            Func<T, bool> showColorPicker,
            string addButtonLabel,
            string primaryLabelHeader) where T : class
        {
            // Column order: Category/Key | Details | Symbol | Preview | Color (swatch+hex+remove) | Remove (trash)
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(primaryLabelHeader, EditorStyles.miniBoldLabel, GUILayout.Width(130f));
                EditorGUILayout.LabelField("Details", EditorStyles.miniBoldLabel, GUILayout.Width(26f));
                EditorGUILayout.LabelField("Symbol", EditorStyles.miniBoldLabel, GUILayout.Width(140f));
                EditorGUILayout.LabelField("Preview", EditorStyles.miniBoldLabel, GUILayout.Width(44f));
                EditorGUILayout.LabelField("Color", EditorStyles.miniBoldLabel, GUILayout.Width(152f));
                EditorGUILayout.LabelField("", GUILayout.Width(26f)); // Remove (trash)
            }

            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                using (new EditorGUILayout.HorizontalScope())
                {
                    setPrimaryLabel(entry, EditorGUILayout.TextField(getPrimaryLabel(entry), GUILayout.Width(130f)));

                    // Details button -- opens a popup with a text area and a Close button.
                    if (GUILayout.Button("...", GUILayout.Width(26f)))
                        PopupWindow.Show(GUILayoutUtility.GetLastRect(), new EntryDetailsPopup(getPrimaryLabel(entry), () => getDetails(entry), v => setDetails(entry, v)));

                    // Symbol: ObjectField for sprite assignment (auto-registers to wall library on change).
                    Sprite current = ResolveSpriteForKey(getIconKey(entry));
                    Sprite chosen = (Sprite)EditorGUILayout.ObjectField(current, typeof(Sprite), false, GUILayout.Width(140f));
                    if (chosen != current)
                        setIconKey(entry, AssignSpriteToLibraryAndGetKey(chosen, getPrimaryLabel(entry)));

                    // Choose existing: curated popup over this wall's + framework
                    // symbols only (section 14.7), instead of the ObjectField picker
                    // which lists every Sprite in the whole project.
                    if (GUILayout.Button("Choose", GUILayout.Width(60f)))
                    {
                        // Capture per-iteration: PopupWindow.Show is async, so a
                        // lambda must not close over the loop variable directly.
                        EnsureDefaultIconLibraryLoaded();
                        var targetEntry = entry;
                        PopupWindow.Show(GUILayoutUtility.GetLastRect(),
                            new ExistingSymbolPickerPopup(_wallIconLibrary, _defaultIconLibrary,
                                key => setIconKey(targetEntry, key)));
                    }

                    // Preview: thumbnail of the chosen sprite (separate from the ObjectField).
                    DrawSpritePreview(chosen != null ? chosen : current);

                    // Color swatch (36x36 clickable square) + hex text field (bidirectional sync).
                    if (showColorPicker(entry))
                    {
                        string colorHex = getColorHex(entry);
                        DrawColorSwatchAndHex(ref colorHex);
                        setColorHex(entry, colorHex);

                        if (GUILayout.Button(TrashIcon, GUILayout.Width(26f), GUILayout.Height(22f)))
                        {
                            entries.RemoveAt(i);
                            i--;
                            continue;
                        }
                    }
                    else
                    {
                        GUILayout.Space(36f + 90f + 26f + 8f);
                    }
                }
            }

            if (GUILayout.Button(addButtonLabel))
                entries.Add(createNew());
        }

        private void DrawSpritePreview(Sprite sprite)
        {
            Texture preview = null;
            if (sprite != null)
                preview = AssetPreview.GetAssetPreview(sprite) ?? AssetPreview.GetMiniThumbnail(sprite);

            GUILayout.Box(preview ?? Texture2D.grayTexture, GUILayout.Width(36f), GUILayout.Height(36f));
        }

        // Split color picker into a separate 36x36 clickable square and a hex text field,
        // both kept in bidirectional sync (picker updates hex, hex field updates color).
        private void DrawColorSwatchAndHex(ref string colorHex)
        {
            Color parsed = TryParseHexColor(colorHex, out var c) ? c : Color.white;

            EditorGUI.BeginChangeCheck();
            Color picked = EditorGUILayout.ColorField(GUIContent.none, parsed, false, false, false, GUILayout.Width(36f), GUILayout.Height(36f));
            if (EditorGUI.EndChangeCheck())
            {
                colorHex = ToHexRgb(picked);
            }

            colorHex = EditorGUILayout.TextField(colorHex ?? string.Empty, GUILayout.Width(90f));
        }

        private static bool TryParseHexColor(string hex, out Color color)
        {
            color = default;
            if (string.IsNullOrWhiteSpace(hex))
                return false;

            string normalized = hex.Trim();
            if (!normalized.StartsWith("#", StringComparison.Ordinal))
                normalized = "#" + normalized;

            return ColorUtility.TryParseHtmlString(normalized, out color);
        }

        private static string ToHexRgb(Color color)
        {
            Color32 c32 = color;
            return $"#{c32.r:X2}{c32.g:X2}{c32.b:X2}";
        }
    }
}