using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    public partial class POIEditorToolWindow
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
        // Driven by delegates rather than an interface hierarchy â€” simpler for editor-only GUI code,
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
            string primaryLabelHeader,
            string detailsColumnHelp = "",
            string symbolColumnHelp = "",
            bool showSearchKeywords = false,
            Func<T, List<string>> getSearchKeywords = null,
            Action<T, List<string>> setSearchKeywords = null,
            Func<T, int> countPoiReferences = null,
            // Test-observability seam only (default no-op, zero cost when unused): the
            // first data row reports its landmark rects (preview thumbnail, color
            // picker, color hex, keywords/Suggest-Edit button, delete button) so a real
            // OnGUI render test can assert on ACTUAL measured geometry instead of
            // guessing -- same rationale as DrawCoordinateRow's out-Rect params.
            Action<Rect, Rect, Rect, Rect, Rect> captureFirstRowRects = null) where T : class
        {
            // Column order (5 groups, header mirrors rows exactly):
            // [key+details] | [Symbol + interactive Preview] | [Color] | [SearchKeywords] | [trash]
            using (new EditorGUILayout.HorizontalScope())
            {
                // Group 1: primary key + notes info
                EditorGUILayout.LabelField(primaryLabelHeader, EditorStyles.miniBoldLabel, GUILayout.Width(130f));
                GUILayout.Space(TableGapWithinGroup);
                // Sized to match the per-row Details button's own width (26f) directly
                // below it, instead of the shared 22px default every other help button uses.
                HelpInfoButton.Draw(primaryLabelHeader + " Notes", detailsColumnHelp, 26f);

                GUILayout.Space(TableGapBetweenGroups);

                // Group 2: Symbol + interactive Preview info (preview = curated picker).
                // Explicit 36f (wider than the shared 22px default): this button sits
                // directly above the Preview column's own 36f-wide thumbnails, and lining
                // up with THAT column matters more here than matching every other help
                // button's width.
                EditorGUILayout.LabelField("Symbol", EditorStyles.miniBoldLabel, GUILayout.Width(140f));
                GUILayout.Space(SymbolColumnPad);
                HelpInfoButton.Draw(primaryLabelHeader + " Symbols", symbolColumnHelp, 36f);

                // Extra gap (not the standard between-groups gap) before Color: the
                // Preview thumbnail sits directly next to the (intentionally narrow)
                // color swatch, so this boundary needs more breathing room than the
                // other column boundaries to read as clearly separate.
                GUILayout.Space(TableGapBeforeColor);

                // Group 3: Color
                EditorGUILayout.LabelField("Color", EditorStyles.miniBoldLabel, GUILayout.Width(ColorGroupWidth));

                // Group 4: Search keywords -- second half of the row, spaced by the
                // same between-groups gap so the Color group (or its placeholder) and
                // the keywords column read as separate columns.
                GUILayout.Space(TableGapBetweenGroups);
                if (showSearchKeywords)
                    EditorGUILayout.LabelField("Search Keywords", EditorStyles.miniBoldLabel);

                // Group 5: Remove (trash) -- last column. Extra gap (not the standard
                // between-groups gap) so the destructive action reads as clearly set apart
                // from the keyword group next to it, not just another column boundary.
                GUILayout.Space(TableGapBeforeDelete);
                EditorGUILayout.LabelField("", GUILayout.Width(26f)); // Remove (trash)

                // Right-edge scrollbar clearance (_5.1_Editor_Tab.md "Row Indentation &
                // Spacing"): non-table rows get this for free from DrawEditorRow/
                // EditorRowWidthForIndent, but taxonomy tables build their own row layout
                // and were never given it, so their last column's pixels could sit
                // underneath the ScrollView's vertical scrollbar on a long table.
                GUILayout.Space(AddButtonRowRightMargin);
            }

            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                using (new EditorGUILayout.HorizontalScope())
                {
                    // Group 1: key + details
                    setPrimaryLabel(entry, EditorGUILayout.TextField(getPrimaryLabel(entry), GUILayout.Width(130f)));

                    // Details button -- opens a popup with a text area and a Close button.
                    GUILayout.Space(TableGapWithinGroup);
                    if (GUILayout.Button(DetailsIcon, GUILayout.Width(26f), GUILayout.Height(20f)))
                        PopupWindow.Show(GUILayoutUtility.GetLastRect(), new EntryDetailsPopup(getPrimaryLabel(entry), () => getDetails(entry), v => setDetails(entry, v)));

                    GUILayout.Space(TableGapBetweenGroups);

                    // Group 2: Symbol + interactive Preview (the preview IS the
                    // curated "choose" affordance -- no separate select button).
                    Sprite current = ResolveSpriteForKey(getIconKey(entry));
                    Sprite chosen = (Sprite)EditorGUILayout.ObjectField(current, typeof(Sprite), false, GUILayout.Width(140f));
                    if (chosen != current)
                        setIconKey(entry, AssignSpriteToLibraryAndGetKey(chosen, getPrimaryLabel(entry)));

                    GUILayout.Space(TableGapWithinGroup);

                    // Clicking the preview opens the curated wall + framework picker
                    // (section 14.7). Capture per-iteration: PopupWindow.Show is async,
                    // so a lambda must not close over the loop variable directly.
                    EnsureDefaultIconLibraryLoaded();
                    var previewEntry = entry;
                    DrawSpritePreview(chosen != null ? chosen : current,
                        () => PopupWindow.Show(GUILayoutUtility.GetLastRect(),
                            new ExistingSymbolPickerPopup(_wallIconLibrary, _defaultIconLibrary,
                                key => setIconKey(previewEntry, key))));
                    Rect previewRect = GUILayoutUtility.GetLastRect();

                    // Extra gap (not the standard between-groups gap) before Color --
                    // same reason as the header above.
                    GUILayout.Space(TableGapBeforeColor);

                    // Group 3: Color (real picker + hex). The picker CELL is the first
                    // thing in this group -- the between-groups gap above is the only
                    // spacer before it, and the hex field is spaced by the within-group
                    // gap inside the group. No draw-only swatch in front of the picker.
                    Rect colorPickerRect = default, colorHexRect = default;
                    if (showColorPicker(entry))
                    {
                        string colorHex = getColorHex(entry);
                        DrawColorSwatchAndHex(ref colorHex, out colorPickerRect, out colorHexRect);
                        setColorHex(entry, colorHex);
                    }
                    else
                    {
                        // Reserve Group 3's footprint so Group 4 stays aligned.
                        GUILayout.Space(ColorGroupWidth);
                    }

                    GUILayout.Space(TableGapBetweenGroups);

                    // Group 4: Search keywords (optional, shown when showSearchKeywords is true).
                    // The inline cell is a REAL editor for the same field the Edit popup
                    // edits: both go through get/set on every pass, so the two can never
                    // disagree -- the same bidirectional-sync rule as the color picker +
                    // hex pair in DrawColorSwatchAndHex. The popup exists only because this
                    // column is too narrow to read a long keyword list comfortably.
                    Rect keywordsTailRect = default;
                    if (showSearchKeywords && getSearchKeywords != null)
                    {
                        var targetEntry = entry;
                        var keywords = getSearchKeywords(targetEntry) ?? new List<string>();
                        string joined = string.Join(", ", keywords);

                        string edited = EditorGUILayout.TextField(joined, GUILayout.ExpandWidth(true));
                        if (edited != joined)
                            setSearchKeywords(targetEntry, ParseKeywordList(edited));

                        GUILayout.Space(TableGapWithinGroup);
                        string buttonLabel = string.IsNullOrEmpty(joined) ? "Suggest" : "Edit";
                        if (GUILayout.Button(buttonLabel, GUILayout.Width(60f)))
                        {
                            // The popup reads the field live rather than the string captured
                            // at click time, so an inline edit made a moment earlier is what
                            // the popup opens with.
                            PopupWindow.Show(GUILayoutUtility.GetLastRect(),
                                new EntryDetailsPopup("Search Keywords",
                                    () => string.Join(", ", getSearchKeywords(targetEntry) ?? new List<string>()),
                                    v => setSearchKeywords(targetEntry, ParseKeywordList(v))));
                        }
                        keywordsTailRect = GUILayoutUtility.GetLastRect();
                    }

                    // Group 5: Remove (trash) -- last column. Extra gap (not the standard
                    // between-groups gap) so the destructive action reads as clearly set
                    // apart from the keyword group next to it.
                    GUILayout.Space(TableGapBeforeDelete);
                    bool deleteClicked = DeleteButton.DrawLayout($"Delete {primaryLabelHeader}: {getPrimaryLabel(entry)}");
                    Rect deleteRect = GUILayoutUtility.GetLastRect();

                    // Same right-edge scrollbar clearance as the header row above.
                    GUILayout.Space(AddButtonRowRightMargin);

                    if (i == 0)
                        captureFirstRowRects?.Invoke(previewRect, colorPickerRect, colorHexRect, keywordsTailRect, deleteRect);

                    if (deleteClicked)
                    {
                        // Callers whose rows carry a POI-referenced identity pass
                        // countPoiReferences so deleting a row that is still in use asks
                        // first (see IdentityDeleteGuard). Callers with no referencing data
                        // omit it and keep the single-click delete.
                        int references = countPoiReferences != null ? countPoiReferences(entry) : 0;
                        if (IdentityDeleteGuard.Confirm(primaryLabelHeader, getPrimaryLabel(entry), references))
                        {
                            entries.RemoveAt(i);
                            i--;
                            continue;
                        }
                    }
                }
            }

            // +Add button row, drawn as TWO BUTTONS in one row:
            //   1) a TRANSPARENT spacer button whose width is the indent Unity ACTUALLY
            //      applies here (runtime-measured via EditorGUI.IndentedRect --
            //      indentLevel * theme indent width, currently 30px at level 2).
            //      Guessed constants are gone; the spacer always matches the rows.
            //   2) the real "+ Add ..." button, sized min(available, MaxRowWidth).
            DrawAddButtonRow(addButtonLabel, () => entries.Add(createNew()), out _, out _);
        }

        // Draws the +Add button as a one-row pair: transparent spacer then real
        // button, both in one HorizontalScope. Thin wrapper over the shared
        // DrawEditorRow so this row follows the one true layout rule (indent +
        // max(MinRowWidth, min(panel, MaxRowWidth)) + ExpandWidth(false)).
        internal static void DrawAddButtonRow(string label, Action onAdd, out Rect spacerRect, out Rect addRect)
        {
            DrawEditorRow(out float rowWidth, out spacerRect);

            if (GUILayout.Button(label, GUILayout.Width(rowWidth), GUILayout.ExpandWidth(false)))
                onAdd();
            addRect = GUILayoutUtility.GetLastRect();

            EditorRowEnd();
        }

        // Draws a 36f thumbnail of the symbol. With onPick == null it is a plain
        // box (read-only). With onPick set it becomes a BUTTON -- the preview is
        // then the interactive "choose" affordance: hover/tooltip says it opens the
        // curated wall + framework picker, and clicking calls onPick.
        private void DrawSpritePreview(Sprite sprite, System.Action onPick = null)
        {
            Texture preview = null;
            if (sprite != null)
                preview = AssetPreview.GetAssetPreview(sprite) ?? AssetPreview.GetMiniThumbnail(sprite);

            if (onPick == null)
            {
                GUILayout.Box(preview ?? Texture2D.grayTexture, GUILayout.Width(36f), GUILayout.Height(36f));
                return;
            }

            // Empty symbol shows the "choose" hand glyph as the placeholder so the
            // cell reads as an interactive pick target, not a dead grey box.
            Texture display = preview ?? SelectIcon.image;
            var content = new GUIContent(display,
                "Click to choose a symbol from this wall + framework defaults");
            if (GUILayout.Button(content, GUILayout.Width(36f), GUILayout.Height(36f)))
                onPick();
        }

        // Color group cell pair: the REAL editor color picker (forced wide so it is
        // easy to click, native swatch + eyedropper affordance) directly followed by
        // the hex "color name". No flat draw-only swatch in front of the picker --
        // that read as a dead, non-clickable cell and made the picker look broken.
        // No explicit within-group spacer here: GUILayout's default inter-control
        // spacing (~3px) already separates the pair, and the measured layout shows
        // that is all there is between them. Both cells stay in bidirectional sync.
        // internal (not private) + out rects: a real OnGUI render test
        // (POIEditorColorGroupRenderTests) calls this directly and asserts on the
        // ACTUAL GUILayoutUtility rects, the same pattern DrawCoordinateRow uses.
        internal static void DrawColorSwatchAndHex(ref string colorHex, out Rect pickerRect, out Rect hexRect)
        {
            Color parsed = TryParseHexColor(colorHex, out var c) ? c : Color.white;

            // Root cause of the big gap between the picker and the hex field: this pair
            // is drawn inside an ambient indented context (every call site sits inside a
            // DrawFramedFoldout's IndentLevelScope), and TWO EditorGUILayout controls
            // sharing one row each independently re-apply EditorGUI.indentLevel to their
            // own rect (see _5.1_Editor_Tab.md Lesson 4 -- the exact same bug that hit the
            // Position foldout's X/Y/Z coordinate row). Zeroing indentLevel for just these
            // two controls, restored right after, removes the double-indent so the pair
            // sits at its intended flush ~3px GUILayout auto-spacing -- without switching
            // either control away from a real GUILayout ColorField/TextField.
            //
            // ExpandWidth(false) added on both controls per this file's own "Reusable
            // Row-Layout Command" rule ("ALL elements + GUILayout.ExpandWidth(false) --
            // MANDATORY, or the stretchy default style wins") -- this pair never had it,
            // unlike every properly-converted row elsewhere in the window.
            int savedIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            try
            {
                EditorGUI.BeginChangeCheck();
                Color picked = EditorGUILayout.ColorField(GUIContent.none, parsed, false, false, false,
                    GUILayout.Width(ColorPickerWidth), GUILayout.ExpandWidth(false));
                if (EditorGUI.EndChangeCheck())
                    colorHex = ToHexRgb(picked);
                pickerRect = GUILayoutUtility.GetLastRect();

                colorHex = EditorGUILayout.TextField(colorHex ?? string.Empty,
                    GUILayout.Width(ColorHexFieldWidth), GUILayout.ExpandWidth(false));
                hexRect = GUILayoutUtility.GetLastRect();
            }
            finally
            {
                EditorGUI.indentLevel = savedIndent;
            }
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

        // Draw a keyword list inline in a table row, returning the edited list.
        // Shows a TextField (comma-separated) with an Edit popup for richer
        // editing. Used by the search_keywords column in taxonomy tables.
        private static List<string> DrawKeywordListField(List<string> keywords)
        {
            if (keywords == null)
                keywords = new List<string>();

            string joined = string.Join(", ", keywords);
            string edited = EditorGUILayout.TextField(joined, GUILayout.ExpandWidth(true));
            if (edited != joined)
                keywords = ParseKeywordList(edited);

            return keywords;
        }

        // Parse a comma-separated keyword string into a list, trimming and
        // dropping empties. Used by the search_keywords column's Edit popup.
        private static List<string> ParseKeywordList(string text)
        {
            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(text))
                return result;

            foreach (string part in text.Split(','))
            {
                string trimmed = part.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                    result.Add(trimmed);
            }
            return result;
        }
    }
}
