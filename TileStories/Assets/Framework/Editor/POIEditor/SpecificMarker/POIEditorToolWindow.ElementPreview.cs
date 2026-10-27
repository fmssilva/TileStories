// POIEditorToolWindow.ElementPreview.cs
//
// Partial: the PREVIEW of a composed block (_3.3 R17.1, R17.5, R17.7, R14.8) -- what the card shows before the block is opened, under its
// Page. Mode Automatic (the default) says live which Page rows it takes ("Shows: Picture #1, Eyebrow #2, Title #3, Lede #4"), recomputed
// on every Page edit by the card's own rule (ElementPreviewRule). Custom is its own row list: a LINKED row shows a Page row's content with
// its own look ("from Page #3", Unlink makes it a copy), an OWN row holds content only the preview shows, a link whose Page row is gone is a
// warning row (Unlink / Remove), never a silent blank. Then the Open Row override (inherit the wall's, or this block's words and emphasis)
// and Reading Time (Auto from the rows, or minutes set by hand).

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    public partial class POIEditorToolWindow
    {
        private void DrawStoryPreview(POIData poi, BlockInstanceData block, BlockKindDefinition kind, ElementScope scope, ElementRegistry elements,
            ElementPreviewRule.Result preview, List<string> languages)
        {
            if (!DrawComposedFoldout(ComposedKey(poi.id, block.key, "preview"), true, "Preview", "what the card shows before it is opened",
                    StoryPreviewHelp, IndentLevel1, "Story preview section")) return;

            var pageRows = BlockSurfaces.PageRows(block, kind);
            int mode = preview.Mode == ElementOptions.PreviewCustom ? 1 : 0;
            int picked = DrawSegmentRow("Mode", mode, ElementOptions.PreviewModeLabels, StoryPreviewModeHelp, IndentLevel1, "Story preview mode", 0);
            if (picked >= 0 && picked != mode)
                StoryQueue(() => ElementEdits.SetPreviewMode(scope, elements, ElementOptions.PreviewModes[picked]));

            if (preview.Mode != ElementOptions.PreviewCustom)
                DrawStoryShows(preview, pageRows, elements);
            else
                DrawStoryCustomRows(poi, block, kind, scope, elements, preview, pageRows, languages);

            DrawStoryOpenRow(block, languages);
            DrawStoryReadingTime(block, scope, elements, languages);
        }

        // Automatic: which Page rows the preview shows now, by element and number, and that it follows the Page
        private void DrawStoryShows(ElementPreviewRule.Result preview, IReadOnlyList<BlockItemData> pageRows, ElementRegistry elements)
        {
            string shows = StoryShowsLine(preview, pageRows, elements);
            DrawEditorRow(out float rowWidth, out _, IndentLevel2);
            GUILayout.Label("Shows: " + (shows.Length > 0 ? shows : "nothing yet (write a title or a text on the Page)"), EditorStyles.boldLabel,
                GUILayout.Width(Mathf.Max(40f, rowWidth - 36f)), GUILayout.ExpandWidth(false));
            ReportTableCellRect("Story preview shows", 0);
            EditorRowEnd();
            DrawStoryNote("Follows the Page: reorder or delete a row above and this changes with it (the rows marked P in the Page table).", IndentLevel2);
        }

        // "Picture #1, Eyebrow #2, Title #3, Lede #4": the Automatic preview's rows in Page order, named as the table names them
        internal static string StoryShowsLine(ElementPreviewRule.Result preview, IReadOnlyList<BlockItemData> pageRows, ElementRegistry elements)
        {
            var parts = new List<string>();
            foreach (var entry in preview.Entries)
            {
                int number = IndexOfRow(pageRows, entry.PageRowId) + 1;
                string name = elements.TryGet(new ElementRow(entry.Content).Kind, out var element) ? element.DisplayName : "Row";
                parts.Add(number > 0 ? name + " #" + number : name);
            }
            return string.Join(", ", parts);
        }

        // Custom: the preview's own table -- a mark (link / own / broken), its number, what it shows, Validated, up / down, Unlink, delete --
        // an open row under its line, then "+ Link a Page row" (the Page's rows to pick from) and "+ Add own row" (the palette)
        private void DrawStoryCustomRows(POIData poi, BlockInstanceData block, BlockKindDefinition kind, ElementScope scope, ElementRegistry elements,
            ElementPreviewRule.Result preview, IReadOnlyList<BlockItemData> pageRows, List<string> languages)
        {
            var rows = BlockSurfaces.PreviewRows(block);
            for (int i = 0; i < rows.Count; i++)
            {
                var row = new ElementRow(rows[i]);
                var entry = preview.Entries.Find(e => ReferenceEquals(e.Row, rows[i]));
                bool broken = entry.Source == ElementPreviewRule.Source.Broken;
                var content = broken || entry.Content == null ? row : new ElementRow(entry.Content);
                elements.TryGet(row.IsRef ? content.Kind.Length > 0 ? content.Kind : row.Kind : row.Kind, out var element);
                int pageNumber = row.IsRef ? IndexOfRow(pageRows, row.Ref.Row) + 1 : 0;
                DrawStoryPreviewRow(poi, block, rows, i, row, content, element, broken, pageNumber, languages);
                if (!broken && IsComposedOpen(ComposedKey(poi.id, block.key, "row/" + row.Id), false))
                    DrawElementRowPanel(poi, block, scope, elements, row, content, element, isLink: row.IsRef, pageNumber, "Story preview", i, languages);
            }

            // - link a Page row: the rows not linked yet, one button each
            string linkKey = ComposedKey(poi.id, block.key, "preview/link");
            bool linking = IsComposedOpen(linkKey, false);
            DrawEditorRow(out float rowWidth, out _, IndentLevel1);
            float half = (Mathf.Max(80f, rowWidth - 36f) - 4f) / 2f;
            if (GUILayout.Button(linking ? "- Close the Page rows" : "+ Link a Page row", GUILayout.Width(half), GUILayout.ExpandWidth(false)))
                _composedOpen[linkKey] = !linking;
            ReportTableCellRect("Story preview link", 0);
            string addKey = ComposedKey(poi.id, block.key, "preview/add");
            bool adding = IsComposedOpen(addKey, false);
            if (GUILayout.Button(adding ? "- Close the element list" : "+ Add own row", GUILayout.Width(half), GUILayout.ExpandWidth(false)))
                _composedOpen[addKey] = !adding;
            ReportTableCellRect("Story preview own", 0);
            HelpInfoButton.Draw("Custom preview rows", StoryCustomRowsHelp);
            EditorRowEnd();

            if (linking)
                for (int p = 0; p < pageRows.Count; p++)
                {
                    var pageRow = new ElementRow(pageRows[p]);
                    if (preview.UsedPageRows.Contains(pageRow.Id)) continue;
                    elements.TryGet(pageRow.Kind, out var element);
                    DrawEditorRow(out float lineWidth, out _, IndentLevel2);
                    string words = "#" + (p + 1) + " " + (element?.DisplayName ?? pageRow.Kind) + ": " + StoryRowSummary(pageRow, element, FirstLanguageOf(languages));
                    if (GUILayout.Button(new GUIContent(FitWithEllipsis(words, lineWidth - 46f, EditorStyles.miniButton), words), EditorStyles.miniButton,
                            GUILayout.Width(Mathf.Max(40f, lineWidth - 36f)), GUILayout.ExpandWidth(false)))
                    {
                        string id = pageRow.Id;
                        StoryQueue(() =>
                        {
                            ElementEdits.PreviewList(block).Add(ElementEdits.Link(scope, id).Data);
                            _composedOpen[linkKey] = false;
                        });
                    }
                    ReportTableCellRect("Story preview link page", p);
                    EditorRowEnd();
                }
            // - the same palette as the Page's, its pick added to the preview's own rows
            if (adding) DrawStoryPalette(poi, block, addKey, "Story preview add", element => ElementEdits.Add(block, ElementEdits.PreviewList(block), element));
        }

        // One Custom preview row's line: [link / own / warning] [chevron] [#] what it shows, Validated, up / down, Unlink, delete
        private void DrawStoryPreviewRow(POIData poi, BlockInstanceData block, IReadOnlyList<BlockItemData> rows, int i, ElementRow row, ElementRow content,
            ElementKindDefinition element, bool broken, int pageNumber, List<string> languages)
        {
            var r = StoryRowRect(IndentLevel1, StoryRowHeight);
            string rowKey = ComposedKey(poi.id, block.key, "row/" + row.Id);
            bool open = IsComposedOpen(rowKey, false);
            if (Event.current.type == EventType.Repaint && (open || broken)) EditorGUI.DrawRect(r, broken ? StoryBrokenTint : StoryOpenRowTint);
            float x = r.x;
            var mark = new Rect(x, r.y + 2f, StoryHandleWidth + 2f, StoryHandleWidth + 2f);
            if (Event.current.type == EventType.Repaint)
            {
                string icon = broken ? "console.warnicon.sml" : row.IsRef ? "Linked" : null;
                if (icon != null) GUI.DrawTexture(mark, EditorGUIUtility.IconContent(icon).image, ScaleMode.ScaleToFit);
            }
            ReportTableCellRect("Story preview mark", i, mark);
            x += StoryHandleWidth + 2f;
            bool toggled = open;
            if (!broken)
            {
                var chevron = new Rect(x, r.y + 2f, StoryCell - 6f, r.height - 4f);
                toggled = StoryChevron(chevron, open);
                ReportTableCellRect("Story preview open", i, chevron);
            }
            x += StoryCell;
            GUI.Label(new Rect(x, r.y, StoryNumberWidth, r.height), (i + 1).ToString(), EditorStyles.miniLabel);
            x += StoryNumberWidth;

            float right = r.xMax;
            var delete = new Rect(right - StoryCell, r.y, StoryCell, EditorGUIUtility.singleLineHeight);
            right -= StoryCell + TableGapBeforeDelete;
            // - as wide as its longer words, so "Unlink (make a copy)" is never cut
            float unlinkWidth = EditorStyles.miniButton.CalcSize(new GUIContent("Unlink (make a copy)")).x + 10f;
            var unlink = new Rect(right - unlinkWidth, r.y, unlinkWidth, EditorGUIUtility.singleLineHeight);
            var down = new Rect(unlink.x - StoryGap - StoryCell, r.y, StoryCell, EditorGUIUtility.singleLineHeight);
            var up = new Rect(down.x - StoryGap - StoryCell, r.y, StoryCell, EditorGUIUtility.singleLineHeight);
            var valid = new Rect(up.x - StoryGap - StoryCell - 4f, r.y, StoryCell, EditorGUIUtility.singleLineHeight);

            var cell = new Rect(x, r.y, Mathf.Max(0f, valid.x - StoryGap - x), r.height);
            if (broken)
                GUI.Label(cell, new GUIContent(StoryBrokenRowWords, StoryBrokenLinkNote), StoryEmptyStyle);
            else
            {
                if (GUI.Button(cell, GUIContent.none, GUIStyle.none)) toggled = !open;
                if (row.IsRef)
                {
                    string name = element?.DisplayName ?? content.Kind;
                    string words = name + "  from Page #" + pageNumber + ": " + StoryRowSummary(content, element, FirstLanguageOf(languages));
                    GUI.Label(cell, new GUIContent(FitWithEllipsis(words, cell.width, EditorStyles.label), words), EditorStyles.label);
                }
                else DrawStoryElementCell(cell, row, element, languages);
            }
            ReportTableCellRect("Story preview element", i, cell);

            if (DrawStoryValidatedButton(valid, row)) row.SetValidated(!row.Validated);
            ReportTableCellRect("Story preview validated", i, valid);
            using (new EditorGUI.DisabledScope(i == 0))
                if (GUI.Button(up, "^", EditorStyles.miniButton)) StoryQueue(() => ElementEdits.Move(ElementEdits.PreviewList(block), i, i - 1));
            ReportTableCellRect("Story preview up", i, up);
            using (new EditorGUI.DisabledScope(i >= rows.Count - 1))
                if (GUI.Button(down, "v", EditorStyles.miniButton)) StoryQueue(() => ElementEdits.Move(ElementEdits.PreviewList(block), i, i + 1));
            ReportTableCellRect("Story preview down", i, down);
            if (row.IsRef)
            {
                if (GUI.Button(unlink, new GUIContent(broken ? "Unlink" : "Unlink (make a copy)", broken ? StoryUnlinkBrokenTooltip : StoryUnlinkTooltip), EditorStyles.miniButton))
                {
                    var scope = ElementScope.Of(poi, block, BlockRegistry.Shared);
                    StoryQueue(() =>
                    {
                        ElementEdits.Unlink(scope, ElementRegistry.Shared, i);
                        OpenComposedRow(poi.id, block.key, row.Id);
                    });
                }
                ReportTableCellRect("Story preview unlink", i, unlink);
            }
            if (DeleteButton.Draw(delete, broken ? "Remove this broken link" : "Remove row " + (i + 1) + " from the preview"))
                StoryQueue(() => ElementEdits.Remove(ElementEdits.PreviewList(block), i));
            ReportTableCellRect("Story preview remove", i, delete);
            if (toggled != open) _composedOpen[rowKey] = toggled;
        }

        // The Open Row override (R17.5): inherit the wall's emphasis and the card's words, or this block's own words and emphasis
        private void DrawStoryOpenRow(BlockInstanceData block, List<string> languages)
        {
            var wall = _config.card_settings?.components ?? new CardComponentSettings();
            var own = BlockSurfaces.OverrideOf(block, ElementOptions.ComponentOpenRow);
            int mode = DrawSegmentRow("Open Row", own.IsOn ? 1 : 0, StoryInheritOrOverride, StoryOpenRowOverrideHelp, IndentLevel1, "Story open row", 0);
            if (mode != (own.IsOn ? 1 : 0))
                StoryQueue(() => ElementEdits.SetOverride(block, ElementOptions.ComponentOpenRow, mode == 1, _config.card_settings?.components));
            if (!own.IsOn)
            {
                DrawStoryNote("The wall's: emphasis " + StoryLabel(ElementOptions.OpenRowEmphases, ElementOptions.OpenRowEmphasisLabels,
                    ElementOptions.OneOf(wall.open_row_emphasis, ElementOptions.OpenRowEmphases)).text +
                    " (Detail Card > Card Components), the words of Detail Card > Card Texts.", IndentLevel2);
                return;
            }
            var words = new BlockFieldDefinition { Key = BlockSurfaces.WordsKey, Type = BlockFieldType.LocalizedText, Label = "Words", Help = StoryOpenRowWordsHelp };
            DrawLocalizedRows(words, languages, IndentLevel2, "Story open row words", 0,
                lang => ItemLocalizedValue(own.Row, BlockSurfaces.WordsKey, lang), (lang, text) => SetItemLocalizedValue(own.Row, BlockSurfaces.WordsKey, lang, text));
            DrawStoryPartChoice("Emphasis", own.Row, BlockSurfaces.EmphasisKey, ElementOptions.OpenRowEmphases, ElementOptions.OpenRowEmphasisLabels,
                StoryOpenRowEmphasisHelp, "Story open row emphasis");
        }

        // Reading Time (R14.8): Auto -- counted from the Page's words and media -- or minutes set by hand (switching to it starts from the
        // Auto count, so the card shows the same number until it is changed)
        private void DrawStoryReadingTime(BlockInstanceData block, ElementScope scope, ElementRegistry elements, List<string> languages)
        {
            int byHand = BlockSurfaces.MinutesSetByHand(block);
            string first = FirstLanguageOf(languages);
            int auto = byHand > 0 ? 0 : ElementRules.Minutes(scope, elements, first, first);
            string[] labels = { byHand > 0 ? "Auto" : "Auto (" + auto + " min)", "Set by hand" };
            int mode = DrawSegmentRow("Reading Time", byHand > 0 ? 1 : 0, labels, StoryReadingTimeHelp, IndentLevel1, "Story reading time", 0);
            if (mode == 1 && byHand == 0) StoryQueue(() => ElementEdits.SetMinutes(block, Mathf.Max(1, auto)));
            else if (mode == 0 && byHand > 0) StoryQueue(() => ElementEdits.SetMinutes(block, 0));
            if (byHand <= 0) return;
            DrawEditorRow(out float rowWidth, out _, IndentLevel2);
            EditorGUILayout.PrefixLabel("Minutes");
            int edited = EditorGUILayout.IntField(byHand, GUILayout.Width(Mathf.Max(40f, rowWidth - EditorGUIUtility.labelWidth - 36f)), GUILayout.ExpandWidth(false));
            ReportTableCellRect("Story reading time minutes", 0);
            HelpInfoButton.Draw("Minutes", StoryReadingTimeHelp);
            EditorRowEnd();
            // - what is typed is clamped (a value below 1 would turn Auto back on behind the developer's back)
            if (edited != byHand) ElementEdits.SetMinutes(block, Mathf.Clamp(edited, 1, 120));
        }

        // The number of the row with this id in `rows` (0-based), -1 when none
        private static int IndexOfRow(IReadOnlyList<BlockItemData> rows, string id)
        {
            for (int i = 0; i < rows.Count; i++)
                if (new ElementRow(rows[i]).Id == id) return i;
            return -1;
        }

        private const string StoryBrokenRowWords = "A link whose Page row is gone: Unlink it (an empty row of the same element) or remove it.";
        private static readonly Color StoryBrokenTint = new(0.85f, 0.55f, 0.10f, 0.20f);
    }
}
