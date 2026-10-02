// POIEditorToolWindow.CardDefaultMedia.cs
//
// Partial: Detail Card > Default Media (_3.1_POI_Card_Blocks.md section 8.1, step 13): the wall's own CardMediaLibrary path and the
// Framework's shipped defaults every Asset field's "Pick default..." offers.

using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    public partial class POIEditorToolWindow
    {
        // _3.1 step 13: this wall's own default media library path (like Media Folder), then a read-only table of
        // the Framework's own shipped defaults (preview, kind, key, (i)) -- every Asset field's "Pick default..."
        // offers this wall's own library first, then these
        private const float DefaultMediaPreviewSize = 32f;
        private const float DefaultMediaKindColumnWidth = 70f;
        private const float DefaultMediaKeyColumnWidth = 150f;
        // The gap between two cells of a Default Media row (GUILayout's own gap between two labels)
        private const float DefaultMediaCellGap = 4f;

        // A Default Media row's cells: one rect the size and style of the title row's label (the row less the 36 pt (i) column), reserved
        // in the shared row so the (i) drawn after it sits where the title's does
        private static Rect DefaultMediaCellsRect(float rowWidth, float height)
        {
            float width = Mathf.Max(40f, rowWidth - 36f);
            return GUILayoutUtility.GetRect(width, height, EditorStyles.miniBoldLabel, GUILayout.Width(width), GUILayout.Height(height), GUILayout.ExpandWidth(false));
        }

        private static Rect DefaultMediaKindRect(Rect cells) =>
            new Rect(cells.x + DefaultMediaPreviewSize + DefaultMediaCellGap, cells.y, DefaultMediaKindColumnWidth, EditorGUIUtility.singleLineHeight);

        // - never past the cells' own end, so a narrow window cuts the key instead of pushing it under the (i)
        private static Rect DefaultMediaKeyRect(Rect cells)
        {
            float x = cells.x + DefaultMediaPreviewSize + DefaultMediaKindColumnWidth + 2f * DefaultMediaCellGap;
            return new Rect(x, cells.y, Mathf.Max(0f, Mathf.Min(DefaultMediaKeyColumnWidth, cells.xMax - x)), EditorGUIUtility.singleLineHeight);
        }

        private void DrawCardDefaultMediaSection()
        {
            var s = _config.card_settings;
            s.default_media_library_resources_path = DrawTextRow("Default Media Library", s.default_media_library_resources_path,
                CardDefaultMediaLibraryHelp, IndentLevel0);

            DrawEditorRow(out float titleRow, out _);
            GUILayout.Label("Framework defaults", EditorStyles.miniBoldLabel, GUILayout.Width(Mathf.Max(40f, titleRow - 36f)), GUILayout.ExpandWidth(false));
            HelpInfoButton.Draw("Framework defaults", CardDefaultMediaTableHelp);
            ReportTableCellRect("Default media title help", 0);
            EditorRowEnd();

            var library = CardMediaLibraryLookup.Framework;
            if (library == null || library.Entries.Count == 0)
            {
                EditorGUILayout.HelpBox("No default media found (CardMediaLibrary.asset is missing or empty).", MessageType.Warning);
            }
            else
            {
                // - every row is the shared row (DrawEditorRow) built like the title above: its cells in ONE rect of the title label's
                //   width and style, split here, then the (i). So each (i) lands exactly in the section's (i) column, whatever margins the
                //   cells' own styles have (a FlexibleSpace in an uncapped row once ran it to the window's edge)
                DrawEditorRow(out float headerRow, out _);
                Rect header = DefaultMediaCellsRect(headerRow, EditorGUIUtility.singleLineHeight);
                GUI.Label(DefaultMediaKindRect(header), "Kind", EditorStyles.miniBoldLabel);
                GUI.Label(DefaultMediaKeyRect(header), "Key", EditorStyles.miniBoldLabel);
                EditorRowEnd();
                for (int i = 0; i < library.Entries.Count; i++)
                {
                    var entry = library.Entries[i];
                    DrawEditorRow(out float rowWidth, out _);
                    Rect cells = DefaultMediaCellsRect(rowWidth, DefaultMediaPreviewSize);
                    var previewRect = new Rect(cells.x, cells.y, DefaultMediaPreviewSize, DefaultMediaPreviewSize);
                    Texture2D preview = entry.asset != null ? AssetPreview.GetAssetPreview(entry.asset) ?? AssetPreview.GetMiniThumbnail(entry.asset) : null;
                    GUI.Box(previewRect, preview != null ? (Texture)preview : Texture2D.grayTexture);
                    ReportTableCellRect("Default media preview", i, previewRect);
                    GUI.Label(DefaultMediaKindRect(cells), entry.kind.ToString());
                    GUI.Label(DefaultMediaKeyRect(cells), entry.key);
                    HelpInfoButton.Draw(entry.key, CardDefaultMediaRowHelp(entry));
                    ReportTableCellRect("Default media help", i);
                    EditorRowEnd();
                }
            }

            DrawDomainTestSubSection(_cardDefaultMediaTest, CardDefaultMediaSceneTestGuide, CardDefaultMediaPlaymodeTestGuide, CardDefaultMediaDeviceTestGuide);
        }
    }
}
