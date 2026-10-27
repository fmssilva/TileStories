// POIEditorToolWindow.ComposedBlock.cs
//
// Partial: Specific Marker > Card Content, a block that COMPOSES its own page (_3.3 R14.5, R14.8, R17.1, R17.3, R17.7: the story). Under
// the block's common rows it draws its two surfaces, the PAGE first (the developer writes the content once) then the PREVIEW
// (ElementPreview.cs). The Page is an element table -- a row per element: drag handle, open chevron, number, icon or thumbnail, name and
// first words, the live "P" of the rows the preview shows, the developer's Validated mark, up / down / duplicate / delete -- then "+ Add
// element" (an inline palette by category), the Page Chrome override and the Motion table (one line per row: the same motion value the
// row's own Motion part edits). An open row is ElementRowEditor.cs. Every edit is decided while drawing and applied after the rows
// (ElementEdits), inside the tab's DrawConfigMutationScope: one undo step, the unsaved flag, the live Play Mode push. Drawing never writes.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    public partial class POIEditorToolWindow
    {
        private const float StoryRowHeight = 22f;
        private const float StoryCell = 22f;
        private const float StoryHandleWidth = 14f;
        private const float StoryNumberWidth = 24f;
        private const float StoryMarkWidth = 22f;
        private const float StoryGap = 4f;

        // Which parts of the composed blocks' Editor are open, by ComposedKey (UI state, never saved): the Page / Preview / Motion sections,
        // each row and each row's Look / Motion / Tap part, the add palettes
        private readonly Dictionary<string, bool> _composedOpen = new();
        // The edit a click asked for, applied once the block's rows are drawn (a row is never left half-drawn)
        private Action _composedEdit;
        // The Page row a drag holds (its block's ComposedKey, its row number) and where the pointer is now
        private string _storyDragTable;
        private int _storyDragFrom = -1;
        private float _storyDragY;

        // The key of one part of one block's composed Editor ("poi/block/page", "poi/block/row/el_3/Look")
        internal static string ComposedKey(string poiId, string blockKey, string part) => poiId + "/" + blockKey + "/" + part;

        private bool IsComposedOpen(string key, bool byDefault) => _composedOpen.TryGetValue(key, out bool open) ? open : byDefault;

        // A row's open state and its three parts', all at once: a new row is born fully open (R14.8)
        private void OpenComposedRow(string poiId, string blockKey, string rowId)
        {
            _composedOpen[ComposedKey(poiId, blockKey, "row/" + rowId)] = true;
            foreach (string part in StoryParts) _composedOpen[ComposedKey(poiId, blockKey, "row/" + rowId + "/" + part)] = true;
        }

        private static readonly string[] StoryParts = { StoryPartLook, StoryPartMotion, StoryPartTap };
        private const string StoryPartLook = "Look";
        private const string StoryPartMotion = "Motion";
        private const string StoryPartTap = "Tap";

        // A composed block's two surfaces, Page then Preview, under its common rows
        private void DrawComposedSurfaces(BlockInstanceData block, BlockKindDefinition kind, POIData poi, List<string> languages)
        {
            var scope = ElementScope.Of(poi, block, BlockRegistry.Shared);
            var elements = ElementRegistry.Shared;
            var preview = ElementPreviewRule.Of(scope, elements);
            _composedEdit = null;
            DrawStoryPage(poi, block, kind, scope, elements, preview, languages);
            DrawStoryPreview(poi, block, kind, scope, elements, preview, languages);
            // - room before the next block's row: the two surfaces are a long panel
            GUILayout.Space(10f);
            // - after both surfaces: the one structural edit a click asked for
            var edit = _composedEdit;
            _composedEdit = null;
            edit?.Invoke();
        }

        // ---------------- the Page ----------------

        private void DrawStoryPage(POIData poi, BlockInstanceData block, BlockKindDefinition kind, ElementScope scope, ElementRegistry elements,
            ElementPreviewRule.Result preview, List<string> languages)
        {
            var rows = BlockSurfaces.PageRows(block, kind);
            string note = rows.Count + (rows.Count == 1 ? " row" : " rows") + ", the block's own page";
            if (!DrawComposedFoldout(ComposedKey(poi.id, block.key, "page"), true, "Page", note, StoryPageHelp, IndentLevel1, "Story page section")) return;

            DrawStoryTableHeader(true);
            string table = ComposedKey(poi.id, block.key, "page");
            int dragId = GUIUtility.GetControlID(FocusType.Passive);
            var lines = new List<Rect>();
            for (int i = 0; i < rows.Count; i++)
            {
                var row = new ElementRow(rows[i]);
                elements.TryGet(row.Kind, out var element);
                bool usedByPreview = preview.UsedPageRows.Contains(row.Id);
                lines.Add(DrawStoryPageRow(poi, block, kind, rows, i, row, element, usedByPreview, table, dragId, languages));
                if (IsComposedOpen(ComposedKey(poi.id, block.key, "row/" + row.Id), false))
                    DrawElementRowPanel(poi, block, scope, elements, row, row, element, isLink: false, pageNumber: 0, "Story page", i, languages);
            }
            HandleStoryDrag(dragId, table, lines, block, kind);
            DrawStoryAddRow(poi, block, "page", "Story page add", element => ElementEdits.Add(block, ElementEdits.PageList(block, kind), element));
            DrawStoryPageChromeRow(block);
            DrawStoryMotionTable(poi, block, kind, rows, elements);
        }

        // The table's header line: the columns' names (the Preview mark is "P" on the Page, "Link" on the Preview)
        private void DrawStoryTableHeader(bool page)
        {
            var r = StoryRowRect(IndentLevel1, EditorGUIUtility.singleLineHeight);
            var style = EditorStyles.miniLabel;
            float x = r.x + StoryHandleWidth + StoryCell;
            GUI.Label(new Rect(x, r.y, StoryNumberWidth, r.height), "#", style);
            x += StoryNumberWidth;
            GUI.Label(new Rect(x, r.y, 120f, r.height), "Element", style);
            float right = r.xMax - (StoryCell * 4f + StoryGap * 3f + TableGapBeforeDelete);
            GUI.Label(new Rect(right - StoryMarkWidth * 2f - StoryGap, r.y, StoryMarkWidth + 6f, r.height), new GUIContent(page ? "P" : "", StoryPreviewMarkTooltip), style);
            GUI.Label(new Rect(right - StoryMarkWidth - 4f, r.y, StoryMarkWidth + 24f, r.height), new GUIContent("Valid.", StoryValidatedTooltip), style);
        }

        // One Page row's line; returns its rect (what a drag measures against)
        private Rect DrawStoryPageRow(POIData poi, BlockInstanceData block, BlockKindDefinition kind, IReadOnlyList<BlockItemData> rows, int i, ElementRow row,
            ElementKindDefinition element, bool usedByPreview, string table, int dragId, List<string> languages)
        {
            var r = StoryRowRect(IndentLevel1, StoryRowHeight);
            string rowKey = ComposedKey(poi.id, block.key, "row/" + row.Id);
            bool open = IsComposedOpen(rowKey, false);
            if (Event.current.type == EventType.Repaint && open) EditorGUI.DrawRect(r, StoryOpenRowTint);
            float x = r.x;

            // - the drag handle: press and drag the row to another place
            var handle = new Rect(x, r.y, StoryHandleWidth, r.height);
            DrawStoryHandle(handle);
            ReportTableCellRect("Story page handle", i, handle);
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && handle.Contains(Event.current.mousePosition))
            {
                GUIUtility.hotControl = dragId;
                _storyDragTable = table;
                _storyDragFrom = i;
                _storyDragY = Event.current.mousePosition.y;
                Event.current.Use();
            }
            x += StoryHandleWidth;

            var chevron = new Rect(x, r.y + 2f, StoryCell - 6f, r.height - 4f);
            bool toggled = StoryChevron(chevron, open);
            ReportTableCellRect("Story page open", i, chevron);
            x += StoryCell;
            GUI.Label(new Rect(x, r.y, StoryNumberWidth, r.height), (i + 1).ToString(), EditorStyles.miniLabel);
            x += StoryNumberWidth;

            // - the actions, right to left
            float right = r.xMax;
            var delete = new Rect(right - StoryCell, r.y, StoryCell, EditorGUIUtility.singleLineHeight);
            right -= StoryCell + TableGapBeforeDelete;
            var duplicate = new Rect(right - StoryCell, r.y, StoryCell, EditorGUIUtility.singleLineHeight);
            var down = new Rect(duplicate.x - StoryGap - StoryCell, r.y, StoryCell, EditorGUIUtility.singleLineHeight);
            var up = new Rect(down.x - StoryGap - StoryCell, r.y, StoryCell, EditorGUIUtility.singleLineHeight);
            var valid = new Rect(up.x - StoryGap - StoryCell - 4f, r.y, StoryCell, EditorGUIUtility.singleLineHeight);
            var mark = new Rect(valid.x - StoryGap - StoryMarkWidth, r.y, StoryMarkWidth, r.height);

            // - the element: icon (or the picture itself), name, first words; a click opens the row like the chevron
            var cell = new Rect(x, r.y, Mathf.Max(0f, mark.x - StoryGap - x), r.height);
            if (GUI.Button(cell, GUIContent.none, GUIStyle.none)) toggled = !open;
            ReportTableCellRect("Story page element", i, cell);
            DrawStoryElementCell(cell, row, element, languages);

            // - reported only where it is drawn: what a test reads to know which rows wear the P
            if (usedByPreview)
            {
                GUI.Label(mark, new GUIContent("P", StoryPreviewMarkTooltip), StoryMarkStyle);
                ReportTableCellRect("Story page preview mark", i, mark);
            }
            if (DrawStoryValidatedButton(valid, row)) row.SetValidated(!row.Validated);
            ReportTableCellRect("Story page validated", i, valid);
            using (new EditorGUI.DisabledScope(i == 0))
                if (GUI.Button(up, "^", EditorStyles.miniButton)) StoryQueue(() => ElementEdits.Move(ElementEdits.PageList(block, kind), i, i - 1));
            ReportTableCellRect("Story page up", i, up);
            using (new EditorGUI.DisabledScope(i >= rows.Count - 1))
                if (GUI.Button(down, "v", EditorStyles.miniButton)) StoryQueue(() => ElementEdits.Move(ElementEdits.PageList(block, kind), i, i + 1));
            ReportTableCellRect("Story page down", i, down);
            if (DrawIconButton(duplicate, StoryDuplicateIcon, "Duplicate this row"))
                StoryQueue(() =>
                {
                    var copy = ElementEdits.Duplicate(block, ElementEdits.PageList(block, kind), i);
                    OpenComposedRow(poi.id, block.key, copy.Id);
                });
            ReportTableCellRect("Story page duplicate", i, duplicate);
            if (DeleteButton.Draw(delete, "Delete row " + (i + 1) + (element != null ? " (" + element.DisplayName + ")" : "")))
                StoryQueue(() => ElementEdits.Remove(ElementEdits.PageList(block, kind), i));
            ReportTableCellRect("Story page delete", i, delete);

            if (toggled != open) _composedOpen[rowKey] = toggled;
            return r;
        }

        // Press-drag-release on a row's handle: the row moves to where it was let go (the line between two rows shows where while dragging)
        private void HandleStoryDrag(int dragId, string table, List<Rect> lines, BlockInstanceData block, BlockKindDefinition kind)
        {
            if (_storyDragTable != table || _storyDragFrom < 0) return;
            var e = Event.current;
            if (GUIUtility.hotControl == dragId && e.type == EventType.MouseDrag)
            {
                _storyDragY = e.mousePosition.y;
                e.Use();
                Repaint();
            }
            else if (e.type == EventType.Repaint && lines.Count > 0)
            {
                int before = StoryInsertBefore(lines, _storyDragY);
                float y = before < lines.Count ? lines[before].y - 1f : lines[lines.Count - 1].yMax;
                EditorGUI.DrawRect(new Rect(lines[0].x, y, lines[0].width, 2f), StoryDropLineColour);
            }
            else if (GUIUtility.hotControl == dragId && e.type == EventType.MouseUp)
            {
                GUIUtility.hotControl = 0;
                int from = _storyDragFrom;
                int before = StoryInsertBefore(lines, e.mousePosition.y);
                int to = before > from ? before - 1 : before;
                _storyDragTable = null;
                _storyDragFrom = -1;
                e.Use();
                if (to != from) StoryQueue(() => ElementEdits.Move(ElementEdits.PageList(block, kind), from, to));
            }
        }

        // How many rows' middles are above `y`: the place a dragged row lands before
        internal static int StoryInsertBefore(List<Rect> lines, float y)
        {
            int before = 0;
            foreach (var line in lines)
                if (line.center.y < y) before++;
            return before;
        }

        // "+ Add element", then (open) the palette: a line per category of its elements' buttons; a pick adds the row born fully open
        private void DrawStoryAddRow(POIData poi, BlockInstanceData block, string surface, string probe, Func<ElementKindDefinition, ElementRow> add)
        {
            string key = ComposedKey(poi.id, block.key, surface + "/add");
            bool open = IsComposedOpen(key, false);
            DrawEditorRow(out float rowWidth, out _, IndentLevel1);
            if (GUILayout.Button(open ? "- Close the element list" : "+ Add element", GUILayout.Width(Mathf.Max(40f, rowWidth - 36f)), GUILayout.ExpandWidth(false)))
                _composedOpen[key] = !open;
            ReportTableCellRect(probe, 0);
            HelpInfoButton.Draw("Add element", StoryAddElementHelp);
            EditorRowEnd();
            if (open) DrawStoryPalette(poi, block, key, probe, add);
        }

        // The element palette: a line per category (Text, Media, Structure) of its elements' buttons; a pick adds the row born fully open
        // and closes the palette (`key` is its open state)
        private void DrawStoryPalette(POIData poi, BlockInstanceData block, string key, string probe, Func<ElementKindDefinition, ElementRow> add)
        {
            foreach (ElementCategory category in Enum.GetValues(typeof(ElementCategory)))
            {
                var inCategory = new List<ElementKindDefinition>();
                foreach (var e in ElementRegistry.Shared.All) if (e.Category == category) inCategory.Add(e);
                if (inCategory.Count == 0) continue;
                DrawEditorRow(out float lineWidth, out _, IndentLevel2);
                GUILayout.Label(category.ToString(), EditorStyles.miniBoldLabel, GUILayout.Width(70f));
                float buttonWidth = Mathf.Max(60f, (lineWidth - 70f - 36f) / 4f - 4f);
                int onLine = 0;
                foreach (var element in inCategory)
                {
                    if (onLine == 4)
                    {
                        EditorRowEnd();
                        DrawEditorRow(out _, out _, IndentLevel2);
                        GUILayout.Space(74f);
                        onLine = 0;
                    }
                    if (GUILayout.Button(new GUIContent(element.DisplayName, element.Help), EditorStyles.miniButton, GUILayout.Width(buttonWidth)))
                    {
                        var picked = element;
                        StoryQueue(() =>
                        {
                            var row = add(picked);
                            OpenComposedRow(poi.id, block.key, row.Id);
                            _composedOpen[key] = false;
                        });
                    }
                    ReportTableCellRect(probe + " " + element.Key, 0);
                    onLine++;
                }
                EditorRowEnd();
            }
        }

        // The Page Chrome override (R17.5): inherit the wall's (Detail Card > Card Components) or the block's own breadcrumb and Previous /
        // Next; on seeds from the wall, off keeps the block's values
        private void DrawStoryPageChromeRow(BlockInstanceData block)
        {
            var wall = _config.card_settings?.components ?? new CardComponentSettings();
            var own = BlockSurfaces.OverrideOf(block, ElementOptions.ComponentPageChrome);
            int mode = DrawSegmentRow("Page Chrome", own.IsOn ? 1 : 0, StoryInheritOrOverride, StoryPageChromeOverrideHelp, IndentLevel1, "Story page chrome", 0);
            if (mode != (own.IsOn ? 1 : 0))
                StoryQueue(() => ElementEdits.SetOverride(block, ElementOptions.ComponentPageChrome, mode == 1, _config.card_settings?.components));
            if (!own.IsOn)
            {
                DrawStoryNote("The wall's: breadcrumb " + (wall.page_chrome_breadcrumb ? "shown" : "hidden") + ", Previous / Next " +
                              (wall.page_chrome_prev_next ? "shown" : "hidden") + " (Detail Card > Card Components).", IndentLevel2);
                return;
            }
            DrawStoryPartChoice("Breadcrumb", own.Row, BlockSurfaces.BreadcrumbKey, ElementOptions.ChromeParts, ElementOptions.ChromePartLabels,
                StoryBreadcrumbHelp, "Story page chrome breadcrumb");
            DrawStoryPartChoice("Previous / Next", own.Row, BlockSurfaces.PrevNextKey, ElementOptions.ChromeParts, ElementOptions.ChromePartLabels,
                StoryPrevNextHelp, "Story page chrome prev next");
        }

        // One closed choice of an override row, as a segment row
        private void DrawStoryPartChoice(string label, BlockItemData row, string key, string[] options, string[] labels, string help, string probe)
        {
            string current = ElementOptions.OneOf(ItemChoiceValue(row, key), options);
            int at = System.Array.IndexOf(options, current);
            int picked = DrawSegmentRow(label, at, labels, help, IndentLevel2, probe, 0);
            if (picked >= 0 && picked != at) SetItemChoiceValue(row, key, options[picked]);
        }

        // The Page's choreography (R17.3): the page's own line, then one line per row -- the SAME motion value as the row's Motion part, a
        // click on the chip steps to the next effect the element offers -- and what Reduce Motion makes of it, read-only
        private void DrawStoryMotionTable(POIData poi, BlockInstanceData block, BlockKindDefinition kind, IReadOnlyList<BlockItemData> rows, ElementRegistry elements)
        {
            if (!DrawComposedFoldout(ComposedKey(poi.id, block.key, "motion"), false, "Motion", "the page's choreography, one line per row",
                    StoryMotionTableHelp, IndentLevel1, "Story motion section")) return;
            var first = StoryRowRect(IndentLevel2, EditorGUIUtility.singleLineHeight);
            float nameWidth = Mathf.Min(220f, first.width * 0.38f);
            float reducedWidth = 110f;
            GUI.Label(new Rect(first.x + StoryNumberWidth, first.y, nameWidth, first.height), "The page", EditorStyles.boldLabel);
            GUI.Label(new Rect(first.x + StoryNumberWidth + nameWidth, first.y, first.width - StoryNumberWidth - nameWidth - reducedWidth, first.height),
                "Rows enter one by one as the page scrolls", EditorStyles.miniLabel);
            GUI.Label(new Rect(first.xMax - reducedWidth, first.y, reducedWidth, first.height), new GUIContent("Reduced: still", StoryReducedTooltip), EditorStyles.miniLabel);

            for (int i = 0; i < rows.Count; i++)
            {
                var row = new ElementRow(rows[i]);
                elements.TryGet(row.Kind, out var element);
                var r = StoryRowRect(IndentLevel2, StoryRowHeight);
                GUI.Label(new Rect(r.x, r.y, StoryNumberWidth, r.height), (i + 1).ToString(), EditorStyles.miniLabel);
                string name = element != null ? element.DisplayName : row.Kind + " (unknown)";
                GUI.Label(new Rect(r.x + StoryNumberWidth, r.y, nameWidth, r.height),
                    new GUIContent(FitWithEllipsis(name + "  " + StoryRowSummary(row, element), nameWidth, EditorStyles.label), name), EditorStyles.label);
                if (element == null) continue;
                float chipsX = r.x + StoryNumberWidth + nameWidth;
                float chipsWidth = r.xMax - reducedWidth - chipsX - StoryGap;
                var effectChip = new Rect(chipsX, r.y + 1f, chipsWidth * 0.62f, EditorGUIUtility.singleLineHeight);
                var strengthChip = new Rect(effectChip.xMax + StoryGap, r.y + 1f, chipsWidth * 0.38f - StoryGap, EditorGUIUtility.singleLineHeight);
                if (GUI.Button(effectChip, new GUIContent(StoryMotionWords(row, element), StoryMotionChipTooltip), EditorStyles.miniButton))
                    row.SetMotion(StoryNextOf(element.MotionChoices, row.Motion), row.Strength);
                ReportTableCellRect("Story motion effect", i, effectChip);
                bool still = ElementMotionRule.EffectOf(row, element) == ElementOptions.MotionNone;
                using (new EditorGUI.DisabledScope(still))
                    if (GUI.Button(strengthChip, StoryLabel(ElementOptions.Strengths, ElementOptions.StrengthLabels, row.Strength), EditorStyles.miniButton))
                        row.SetMotion(row.Motion, StoryNextOf(ElementOptions.Strengths, row.Strength));
                ReportTableCellRect("Story motion strength", i, strengthChip);
                GUI.Label(new Rect(r.xMax - reducedWidth, r.y, reducedWidth, r.height), new GUIContent(still ? "Reduced: -" : "Reduced: still", StoryReducedTooltip),
                    EditorStyles.miniLabel);
            }
        }

        // The block header's validation status (R14.8) at the start of `rect`: a red cross and "N to validate", or a green check and "all
        // validated"; returns what is left of the rect for the block's one-line summary
        private static Rect DrawStoryValidationStatus(Rect rect, int left, int blockIndex)
        {
            string words = StoryValidationWords(left);
            float width = Mathf.Min(rect.width, EditorStyles.miniBoldLabel.CalcSize(new GUIContent(words)).x + 20f);
            var area = new Rect(rect.x, rect.y, width, rect.height);
            if (Event.current.type == EventType.Repaint)
            {
                var icon = EditorGUIUtility.IconContent(left > 0 ? "TestFailed" : "TestPassed").image;
                GUI.DrawTexture(new Rect(area.x, area.y + (area.height - 14f) / 2f, 14f, 14f), icon, ScaleMode.ScaleToFit);
                GUI.Label(new Rect(area.x + 17f, area.y, Mathf.Max(0f, area.width - 17f), area.height), new GUIContent(words, StoryBlockValidationTooltip),
                    EditorStyles.miniBoldLabel);
            }
            ReportTableCellRect("Card block validation", blockIndex, area);
            return new Rect(area.xMax + 8f, rect.y, Mathf.Max(0f, rect.xMax - area.xMax - 8f), rect.height);
        }

        // "3 to validate" / "all validated"
        internal static string StoryValidationWords(int left) => left > 0 ? left + " to validate" : "all validated";

        // ---------------- shared pieces ----------------

        // Ask for one structural edit: it runs after the block's rows are drawn (the last asked wins: one click, one edit)
        private void StoryQueue(Action edit) => _composedEdit = edit;

        // A rect of one table line at `indent`, as wide as a Card Content row (the same width rule, so it never runs past the window)
        private static Rect StoryRowRect(float indent, float height)
        {
            float start = EditorGUI.IndentedRect(new Rect(0f, 0f, 0f, 0f)).x + indent;
            float width = CardBlockRowWidth(EditorGUIUtility.currentViewWidth, start);
            var r = GUILayoutUtility.GetRect(start + width, height, GUILayout.Width(start + width), GUILayout.ExpandWidth(false));
            r.x += start;
            r.width = width;
            return r;
        }

        // A foldout line of the composed Editor (a surface, the Motion table, a row's part): its title, a short note after it, its (i)
        private bool DrawComposedFoldout(string key, bool byDefault, string title, string note, string help, float indent, string probe)
        {
            bool open = IsComposedOpen(key, byDefault);
            DrawEditorRow(out float rowWidth, out _, indent);
            var rect = GUILayoutUtility.GetRect(Mathf.Max(40f, rowWidth - 36f), EditorGUIUtility.singleLineHeight,
                GUILayout.Width(Mathf.Max(40f, rowWidth - 36f)), GUILayout.ExpandWidth(false));
            string words = string.IsNullOrEmpty(note) ? title : title + "  -  " + note;
            bool now = EditorGUI.Foldout(rect, open, new GUIContent(FitWithEllipsis(words, rect.width - 16f, StoryFoldoutStyle), words), true, StoryFoldoutStyle);
            ReportTableCellRect(probe, 0, rect);
            HelpInfoButton.Draw(title, help);
            EditorRowEnd();
            if (now != open) _composedOpen[key] = now;
            return now;
        }

        // A labelled row whose value is one of a few options, all in view: segments of one width (the widest label's), as many per line as
        // the value column holds -- they WRAP onto more lines rather than cut a word (the mock's chips) -- one real click picks; its (i) in
        // the row's (i) column. Each segment reports its own rect (probe + " " + its number). Returns the picked index; the caller writes
        // only a changed one (drawing never writes)
        private static int DrawSegmentRow(string label, int current, string[] labels, string help, float indent, string probe, int probeIndex)
        {
            DrawEditorRow(out float rowWidth, out _, indent);
            EditorGUILayout.PrefixLabel(label);
            float width = Mathf.Max(40f, rowWidth - EditorGUIUtility.labelWidth - 36f);
            var (columns, lines) = SegmentLayout(labels, width);
            // - the grid lays its cells out as GUI.SelectionGrid does: each cell the style's fixed height, the style's margin between two
            //   cells (a probe that ignored the gap pointed a test's click INTO it, where nothing takes it)
            var style = EditorStyles.miniButton;
            float cellHeight = style.fixedHeight > 0f ? style.fixedHeight : EditorGUIUtility.singleLineHeight;
            float gapX = Mathf.Max(style.margin.left, style.margin.right), gapY = Mathf.Max(style.margin.top, style.margin.bottom);
            float height = lines * cellHeight + (lines - 1) * gapY;
            // - reserved with a popup's layout margins, so its end and the (i) after it line up with the Choice rows' popups
            var grid = GUILayoutUtility.GetRect(width, height, EditorStyles.popup, GUILayout.Width(width), GUILayout.Height(height), GUILayout.ExpandWidth(false));
            int picked = GUI.SelectionGrid(grid, current, labels, columns, style);
            ReportTableCellRect(probe, probeIndex, grid);
            float cellWidth = (grid.width - (columns - 1) * gapX) / columns;
            for (int k = 0; k < labels.Length; k++)
                ReportTableCellRect(probe + " " + k, probeIndex,
                    new Rect(grid.x + (k % columns) * (cellWidth + gapX), grid.y + (k / columns) * (cellHeight + gapY), cellWidth, cellHeight));
            HelpInfoButton.Draw(label, help);
            EditorRowEnd();
            return picked;
        }

        // How segments of these labels lay out in `width`: as many columns as fit the widest label whole, and the lines that takes
        internal static (int Columns, int Lines) SegmentLayout(string[] labels, float width)
        {
            float widest = 0f;
            // - CalcSize already holds the button's own padding: 2 px more is all a cell needs
            foreach (string l in labels) widest = Mathf.Max(widest, EditorStyles.miniButton.CalcSize(new GUIContent(l)).x + 2f);
            int columns = Mathf.Clamp(Mathf.FloorToInt(width / Mathf.Max(1f, widest)), 1, Mathf.Max(1, labels.Length));
            // - even lines: 4 options in 3 columns would leave one alone on a line; 2 + 2 reads better
            int lines = Mathf.CeilToInt(labels.Length / (float)columns);
            columns = Mathf.CeilToInt(labels.Length / (float)lines);
            return (columns, lines);
        }

        // A short grey line under a row (what an inherited value is, where it is set)
        private static void DrawStoryNote(string words, float indent)
        {
            DrawEditorRow(out float rowWidth, out _, indent);
            GUILayout.Label(words, StoryNoteStyle, GUILayout.Width(Mathf.Max(40f, rowWidth - 36f)), GUILayout.ExpandWidth(false));
            EditorRowEnd();
        }

        // The element cell of a table line: its icon (a picture row shows its picture), its name, its first words
        private void DrawStoryElementCell(Rect cell, ElementRow row, ElementKindDefinition element, List<string> languages)
        {
            var icon = new Rect(cell.x, cell.y + 2f, StoryRowHeight - 4f, StoryRowHeight - 4f);
            if (Event.current.type == EventType.Repaint)
            {
                var picture = StoryThumbnail(row, element);
                if (picture != null) GUI.DrawTexture(icon, picture, ScaleMode.ScaleAndCrop);
                else
                {
                    EditorGUI.DrawRect(icon, StoryCategoryColour(element));
                    GUI.Label(icon, StoryTagOf(element), StoryTagStyle);
                }
            }
            string name = element != null ? element.DisplayName : (row.Kind.Length > 0 ? row.Kind : "?") + " (unknown)";
            float nameWidth = Mathf.Min(EditorStyles.boldLabel.CalcSize(new GUIContent(name)).x + 4f, cell.width * 0.45f);
            var nameRect = new Rect(icon.xMax + 6f, cell.y, nameWidth, cell.height);
            GUI.Label(nameRect, name, EditorStyles.boldLabel);
            string summary = StoryRowSummary(row, element, FirstLanguageOf(languages));
            var summaryRect = new Rect(nameRect.xMax + 4f, cell.y, Mathf.Max(0f, cell.xMax - nameRect.xMax - 4f), cell.height);
            bool empty = summary.Length == 0;
            GUI.Label(summaryRect, new GUIContent(empty ? StoryEmptyWords : FitWithEllipsis(summary, summaryRect.width, EditorStyles.miniLabel), summary),
                empty ? StoryEmptyStyle : EditorStyles.miniLabel);
        }

        // A row in one line: its words (the first text field with some, in `language`), else its files, else its look; "" = nothing written
        internal static string StoryRowSummary(ElementRow row, ElementKindDefinition element, string language = null)
        {
            if (element == null) return "";
            if (element.Fields != null)
                foreach (var field in element.Fields)
                {
                    if (!BlockSummaryRule.IsTextField(field)) continue;
                    string words = BlockSummaryRule.OneLine(BlockFieldReader.Pick(row.Field(field.Key)?.text, language, language));
                    if (words.Length > 0) return words;
                }
            var media = row.Media;
            if (media.Count > 0)
            {
                string first = System.IO.Path.GetFileName(media[0].Asset);
                if (first.Length == 0) return "";
                return media.Count == 1 ? first : first + " + " + (media.Count - 1) + " more";
            }
            if (element.Media == MediaKind.None && element.StyleFields != null && element.StyleFields.Count > 0 && (element.Fields == null || element.Fields.Count == 0))
            {
                var style = element.StyleFields[0];
                return style.Label + ": " + StoryLabel(StoryArray(style.Options), StoryArray(style.OptionLabels), ElementOptions.OneOf(row.Value(style.Key), StoryArray(style.Options))).text;
            }
            return "";
        }

        // The picture a picture-like row shows (its first file, or a video's poster), null when none can be drawn
        private Texture StoryThumbnail(ElementRow row, ElementKindDefinition element)
        {
            if (element == null) return null;
            string asset = element.Media == MediaKind.Image && row.Media.Count > 0 ? row.Media[0].Asset
                : element.Media == MediaKind.Video ? row.Field(BuiltInElements.PosterField)?.asset ?? "" : "";
            if (string.IsNullOrWhiteSpace(asset)) return null;
            return MediaAssetFor(asset, _config.card_settings?.media_resources_path, MediaKind.Image, _config.card_settings?.default_media_library_resources_path) as Texture;
        }

        // An element's two-letter tag on its icon: the initials of a two-word name ("Pull Quote" -> PQ), else its first two letters
        internal static string StoryTagOf(ElementKindDefinition element)
        {
            if (element == null || string.IsNullOrWhiteSpace(element.DisplayName)) return "?";
            var words = element.DisplayName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length > 1) return (words[0].Substring(0, 1) + words[1].Substring(0, 1)).ToUpperInvariant();
            return words[0].Length > 1 ? words[0].Substring(0, 2) : words[0];
        }

        private static Color StoryCategoryColour(ElementKindDefinition element) => element == null ? StoryUnknownColour : element.Category switch
        {
            ElementCategory.Text => StoryTextColour,
            ElementCategory.Media => StoryMediaColour,
            _ => StoryStructureColour,
        };

        // The Validated mark as a button: a green check once the developer checked the row, a red cross until then (never seen by a visitor)
        private static bool DrawStoryValidatedButton(Rect rect, ElementRow row)
        {
            var icon = EditorGUIUtility.IconContent(row.Validated ? "TestPassed" : "TestFailed").image as Texture2D;
            return DrawIconButton(rect, icon, row.Validated ? StoryValidatedOnTooltip : StoryValidatedOffTooltip, 3f);
        }

        // A table line's open chevron at exactly `rect`: the line's rect already holds the indent, and an EditorGUI control would add the
        // ambient one again (the arrow drew under the row's number: _5.1 "Tables")
        private static bool StoryChevron(Rect rect, bool open)
        {
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            try { return EditorGUI.Foldout(rect, open, GUIContent.none, true); }
            finally { EditorGUI.indentLevel = indent; }
        }

        // Three short lines: the grip a row is dragged by
        private static void DrawStoryHandle(Rect rect)
        {
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Pan);
            if (Event.current.type != EventType.Repaint) return;
            for (int k = 0; k < 3; k++)
                EditorGUI.DrawRect(new Rect(rect.x + 2f, rect.center.y - 4f + k * 4f, rect.width - 6f, 1f), StoryHandleColour);
        }

        // The motion chip's words: Auto says what it plays ("Auto (Reveal)"), then the effect; "Reduced" is the rule, shown beside it
        internal static string StoryMotionWords(ElementRow row, ElementKindDefinition element)
        {
            string label = StoryLabel(ElementOptions.Motions, ElementOptions.MotionLabels, row.Motion).text;
            if (row.Motion == ElementOptions.MotionAuto || element == null || !element.OffersMotion(row.Motion))
                return "Auto (" + StoryLabel(ElementOptions.Motions, ElementOptions.MotionLabels, element?.AutoMotion ?? ElementOptions.MotionNone).text + ")";
            return label;
        }

        // The option after `current` in `options`, round to the first
        private static string StoryNextOf(IReadOnlyList<string> options, string current)
        {
            if (options == null || options.Count == 0) return current;
            int at = -1;
            for (int i = 0; i < options.Count; i++) if (options[i] == current) at = i;
            return options[(at + 1) % options.Count];
        }

        // The label of `value` among `options` (the value itself when it has none)
        private static GUIContent StoryLabel(string[] options, string[] labels, string value)
        {
            int at = System.Array.IndexOf(options, value);
            return new GUIContent(at >= 0 && labels != null && at < labels.Length ? labels[at] : value);
        }

        private static string[] StoryArray(IReadOnlyList<string> list)
        {
            if (list == null) return System.Array.Empty<string>();
            var copy = new string[list.Count];
            for (int i = 0; i < list.Count; i++) copy[i] = list[i];
            return copy;
        }

        private static readonly string[] StoryInheritOrOverride = { "Inherit the wall's", "Override" };
        private const string StoryEmptyWords = "(empty: fill it in)";

        private static readonly Color StoryOpenRowTint = new(0.24f, 0.45f, 0.70f, 0.18f);
        private static readonly Color StoryDropLineColour = new(0.30f, 0.60f, 1f, 1f);
        private static readonly Color StoryHandleColour = new(0.5f, 0.5f, 0.5f, 1f);
        private static readonly Color StoryTextColour = new(0.42f, 0.35f, 0.24f, 1f);
        private static readonly Color StoryMediaColour = new(0.12f, 0.31f, 0.55f, 1f);
        private static readonly Color StoryStructureColour = new(0.33f, 0.33f, 0.33f, 1f);
        private static readonly Color StoryUnknownColour = new(0.55f, 0.15f, 0.15f, 1f);

        private static Texture2D StoryDuplicateIcon => EditorGUIUtility.IconContent("TreeEditor.Duplicate").image as Texture2D;

        private static GUIStyle _storyTagStyle, _storyMarkStyle, _storyEmptyStyle, _storyNoteStyle, _storyFoldoutStyle;
        private static GUIStyle StoryTagStyle => _storyTagStyle ??= new GUIStyle(EditorStyles.miniBoldLabel)
            { alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.white }, fontSize = 9 };
        private static GUIStyle StoryMarkStyle => _storyMarkStyle ??= new GUIStyle(EditorStyles.boldLabel)
            { alignment = TextAnchor.MiddleCenter, normal = { textColor = new Color(0.30f, 0.58f, 0.95f) } };
        private static GUIStyle StoryEmptyStyle => _storyEmptyStyle ??= new GUIStyle(EditorStyles.miniLabel)
            { fontStyle = FontStyle.Italic, normal = { textColor = new Color(0.90f, 0.45f, 0.25f) } };
        private static GUIStyle StoryNoteStyle => _storyNoteStyle ??= new GUIStyle(EditorStyles.miniLabel) { wordWrap = true };
        private static GUIStyle StoryFoldoutStyle => _storyFoldoutStyle ??= new GUIStyle(EditorStyles.foldout) { fontStyle = FontStyle.Bold };
    }
}
