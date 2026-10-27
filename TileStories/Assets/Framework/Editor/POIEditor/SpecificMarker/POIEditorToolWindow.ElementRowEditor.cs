// POIEditorToolWindow.ElementRowEditor.cs
//
// Partial: one OPEN row of a composed block (_3.3 R14.5, R14.8, R17.3, R17.4) -- its content (each text in every wall language, its
// media items with their file, caption and credit, a video's poster), then its parts as INLINE accordions (no floating popup): Look (the
// element's closed style options), Motion (the row's one motion value: effect + strength, Reduce Motion read-only), Tap (only where the
// element offers a choice). Each part's line is clickable and says what it holds. A required field with nothing in it is marked; what
// else keeps the row off the card is named under it. A Page row adds "Use in the preview (linked)" / "Copy to the preview". A linked
// preview row shows its parts only: its content is the Page row's. Every value is one real click on a segment (or typing).

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    public partial class POIEditorToolWindow
    {
        // The open panel of `row`: `content` is the row whose content shows (the row itself, or a link's Page row); `pageNumber` names that
        // Page row for a link (1-based); `probe` + `index` name its controls for the tests
        // `owner` names whose UI state it is (a POI's id, or a template's); a `template` row shows its parts only: a template holds no content
        private void DrawElementRowPanel(string owner, BlockInstanceData block, ElementScope scope, ElementRegistry elements, ElementRow row, ElementRow content,
            ElementKindDefinition element, bool isLink, int pageNumber, string probe, int index, List<string> languages, bool template = false)
        {
            if (element == null)
            {
                DrawStoryNote(isLink ? StoryBrokenLinkNote : StoryUnknownElementNote(row.Kind), IndentLevel2);
                return;
            }
            if (isLink) DrawStoryNote("Its content is Page row " + pageNumber + "'s: edit it there. This row sets its own look and motion.", IndentLevel2);
            else if (!template)
            {
                DrawElementContent(row, element, probe, index, languages);
                if (element.Media != MediaKind.None) DrawElementMedia(row, element, probe, index, languages);
            }

            string rowKey = ComposedKey(owner, block.key, "row/" + row.Id);
            if (element.StyleFields != null && element.StyleFields.Count > 0 && DrawStoryPart(rowKey, StoryPartLook, LookSummary(row, element), StoryLookHelp, probe, index))
                foreach (var style in element.StyleFields)
                {
                    var options = StoryArray(style.Options);
                    string current = ElementOptions.OneOf(row.Value(style.Key), options);
                    int at = System.Array.IndexOf(options, current);
                    int picked = DrawSegmentRow(style.Label, at, StoryArray(style.OptionLabels), style.Help, IndentLevel3, probe + " look " + style.Key, index);
                    if (picked >= 0 && picked != at) row.SetValue(style.Key, options[picked]);
                }

            if (DrawStoryPart(rowKey, StoryPartMotion, StoryMotionWords(row, element) + ", " + StoryLabel(ElementOptions.Strengths, ElementOptions.StrengthLabels, row.Strength).text,
                    StoryRowMotionHelp, probe, index))
            {
                // - short words on the segments, so they sit side by side; what Auto plays is said under them
                var motions = StoryArray(element.MotionChoices);
                var labels = new string[motions.Length];
                for (int m = 0; m < motions.Length; m++) labels[m] = StoryLabel(ElementOptions.Motions, ElementOptions.MotionLabels, motions[m]).text;
                int at = System.Array.IndexOf(motions, element.OffersMotion(row.Motion) ? row.Motion : ElementOptions.MotionAuto);
                int picked = DrawSegmentRow("Effect", at, labels, StoryRowMotionHelp, IndentLevel3, probe + " motion effect", index);
                if (picked >= 0 && picked != at) row.SetMotion(motions[picked], row.Strength);
                bool still = ElementMotionRule.EffectOf(row, element) == ElementOptions.MotionNone;
                using (new EditorGUI.DisabledScope(still))
                {
                    int strength = System.Array.IndexOf(ElementOptions.Strengths, row.Strength);
                    int pickedStrength = DrawSegmentRow("Strength", strength, ElementOptions.StrengthLabels, StoryStrengthHelp, IndentLevel3, probe + " motion strength", index);
                    if (pickedStrength >= 0 && pickedStrength != strength) row.SetMotion(row.Motion, ElementOptions.Strengths[pickedStrength]);
                }
                DrawStoryNote("Auto plays " + StoryLabel(ElementOptions.Motions, ElementOptions.MotionLabels, element.AutoMotion).text + " for a " +
                              element.DisplayName + ". Reduce Motion: " + (still ? "nothing moves anyway." : "still -- the row is simply there (a rule, not a choice)."),
                    IndentLevel3);
            }

            if (element.TapChoices != null && DrawStoryPart(rowKey, StoryPartTap,
                    StoryLabel(ElementOptions.Taps, ElementOptions.TapLabels, ElementRules.TapOf(row, element)).text, StoryTapHelp, probe, index))
            {
                var taps = StoryArray(element.TapChoices);
                var labels = new string[taps.Length];
                for (int t = 0; t < taps.Length; t++) labels[t] = StoryLabel(ElementOptions.Taps, ElementOptions.TapLabels, taps[t]).text;
                int at = System.Array.IndexOf(taps, ElementRules.TapOf(row, element));
                int picked = DrawSegmentRow("On Tap", at, labels, StoryTapHelp, IndentLevel3, probe + " tap", index);
                if (picked >= 0 && picked != at) row.SetValue(ElementRow.TapKey, taps[picked]);
            }

            if (template) return;
            if (!isLink) DrawElementProblems(scope, elements, row);
            if (!isLink && probe == StoryPageProbe)
            {
                DrawEditorRow(out float rowWidth, out _, IndentLevel2);
                float half = (Mathf.Max(80f, rowWidth - 36f) - 4f) / 2f;
                if (GUILayout.Button(new GUIContent("Use in the preview (linked)", StoryUseLinkedTooltip), EditorStyles.miniButton, GUILayout.Width(half)))
                {
                    string id = row.Id;
                    StoryQueue(() => ElementEdits.UseInPreview(scope, elements, id, linked: true));
                }
                ReportTableCellRect(probe + " use linked", index);
                if (GUILayout.Button(new GUIContent("Copy to the preview", StoryCopyTooltip), EditorStyles.miniButton, GUILayout.Width(half)))
                {
                    string id = row.Id;
                    StoryQueue(() =>
                    {
                        var copy = ElementEdits.UseInPreview(scope, elements, id, linked: false);
                        OpenComposedRow(owner, block.key, copy.Id);
                    });
                }
                ReportTableCellRect(probe + " copy", index);
                HelpInfoButton.Draw("Use in the preview", StoryUseInPreviewHelp);
                EditorRowEnd();
            }
        }

        // An accordion line of an open row: the part's name and what it holds now; the whole line opens it
        private bool DrawStoryPart(string rowKey, string part, string summary, string help, string probe, int index)
        {
            string key = rowKey + "/" + part;
            bool open = IsComposedOpen(key, false);
            DrawEditorRow(out float rowWidth, out _, IndentLevel2);
            var rect = GUILayoutUtility.GetRect(Mathf.Max(40f, rowWidth - 36f), EditorGUIUtility.singleLineHeight + 2f,
                GUILayout.Width(Mathf.Max(40f, rowWidth - 36f)), GUILayout.ExpandWidth(false));
            if (Event.current.type == EventType.Repaint) EditorStyles.helpBox.Draw(rect, false, false, false, false);
            string words = part + "   " + summary;
            bool now = EditorGUI.Foldout(rect, open, new GUIContent(FitWithEllipsis(words, rect.width - 18f, StoryFoldoutStyle), words), true, StoryFoldoutStyle);
            ReportTableCellRect(probe + " part " + part, index, rect);
            HelpInfoButton.Draw(part, help);
            EditorRowEnd();
            if (now != open) _composedOpen[key] = now;
            return now;
        }

        // The row's content fields: a text field in every wall language (a required one with no words marked), any other field generically
        private void DrawElementContent(ElementRow row, ElementKindDefinition element, string probe, int index, List<string> languages)
        {
            if (element.Fields == null) return;
            foreach (var field in element.Fields)
            {
                string fieldProbe = probe + " " + field.Key;
                if (field.Type == BlockFieldType.LocalizedText || field.Type == BlockFieldType.LocalizedLongText)
                {
                    bool empty = field.Required && !BlockFieldReader.HasItemContent(row.Field(field.Key), field);
                    DrawLocalizedRows(field, languages, IndentLevel2, fieldProbe, index,
                        lang => ItemLocalizedValue(row.Data, field.Key, lang), (lang, text) => SetItemLocalizedValue(row.Data, field.Key, lang, text), empty);
                }
                else
                    DrawItemSubField(row.Data, field, languages, fieldProbe, index, _config.card_settings?.media_resources_path,
                        _config.card_settings?.default_media_library_resources_path, _config.pois, null);
            }
        }

        // The row's media items: each its file, caption (every language) and credit, with up / down / delete; "+ Add" while it holds fewer
        // than its element's most
        private void DrawElementMedia(ElementRow row, ElementKindDefinition element, string probe, int index, List<string> languages)
        {
            string noun = element.Media == MediaKind.Video ? "Clip" : "Picture";
            var media = row.Media;
            DrawEditorRow(out float titleRow, out _, IndentLevel2);
            string range = element.MinMedia == element.MaxMedia ? element.MaxMedia.ToString() : element.MinMedia + " to " + element.MaxMedia;
            GUILayout.Label(noun + "s (" + range + ")" + (media.Count < element.MinMedia ? "  -  " + (element.MinMedia - media.Count) + " missing" : ""),
                media.Count < element.MinMedia ? StoryEmptyStyle : EditorStyles.miniBoldLabel, GUILayout.Width(Mathf.Max(40f, titleRow - 36f)), GUILayout.ExpandWidth(false));
            HelpInfoButton.Draw(noun + "s", StoryMediaHelp);
            EditorRowEnd();

            int moveAt = -1, moveBy = 0, deleteAt = -1;
            var fileField = new BlockFieldDefinition { Key = ElementRow.MediaKey, Type = BlockFieldType.Asset, Media = element.Media, Label = "File", Required = true,
                Help = StoryMediaFileHelp };
            var captionField = new BlockFieldDefinition { Key = ElementRow.MediaKey, Type = BlockFieldType.LocalizedText, Label = "Caption", Help = StoryCaptionHelp };
            for (int m = 0; m < media.Count; m++)
            {
                var item = media[m].Data;
                string itemProbe = probe + " media " + m;
                DrawEditorRow(out _, out _, IndentLevel2);
                GUILayout.Label(noun + " " + (m + 1), EditorStyles.boldLabel, GUILayout.Width(80f));
                using (new EditorGUI.DisabledScope(m == 0))
                    if (GUILayout.Button("^", EditorStyles.miniButton, GUILayout.Width(BlockItemButtonWidth))) { moveAt = m; moveBy = -1; }
                ReportTableCellRect(itemProbe + " up", index);
                using (new EditorGUI.DisabledScope(m >= media.Count - 1))
                    if (GUILayout.Button("v", EditorStyles.miniButton, GUILayout.Width(BlockItemButtonWidth))) { moveAt = m; moveBy = 1; }
                ReportTableCellRect(itemProbe + " down", index);
                GUILayout.Space(TableGapBeforeDelete);
                if (DeleteButton.DrawLayout("Delete " + noun.ToLowerInvariant() + " " + (m + 1))) deleteAt = m;
                ReportTableCellRect(itemProbe + " delete", index);
                GUILayout.FlexibleSpace();
                EditorRowEnd();

                DrawAssetRow(fileField, IndentLevel3, itemProbe + " file", index, _config.card_settings?.media_resources_path,
                    _config.card_settings?.default_media_library_resources_path, () => item.asset ?? "", value => item.asset = value);
                DrawLocalizedRows(captionField, languages, IndentLevel3, itemProbe + " caption", index,
                    lang => item.text?.Find(e => e != null && e.lang == lang)?.value ?? "",
                    (lang, text) => SetLanguage(item.text ??= new List<LocalizedEntry>(), lang, text));
                DrawStoryCreditRow(item, itemProbe + " credit", index);
            }
            if (media.Count < element.MaxMedia)
            {
                DrawEditorRow(out float addRow, out _, IndentLevel2);
                if (GUILayout.Button("+ Add " + noun.ToLowerInvariant(), GUILayout.Width(Mathf.Max(40f, addRow - 36f)), GUILayout.ExpandWidth(false)))
                    row.AddMedia("", new List<LocalizedEntry>(), "");
                ReportTableCellRect(probe + " media add", index);
                EditorRowEnd();
            }
            // - applied after the items: an item is never left half-drawn
            if (deleteAt >= 0) ElementEdits.RemoveMedia(row, deleteAt);
            if (moveAt >= 0) ElementEdits.MoveMedia(row, moveAt, moveAt + moveBy);
        }

        // A media item's credit: one line of text for every language (an author, a year, a licence)
        private static void DrawStoryCreditRow(BlockItemFieldValue item, string probe, int index)
        {
            string current = item.value ?? "";
            DrawEditorRow(out float rowWidth, out _, IndentLevel3);
            EditorGUILayout.PrefixLabel("Credit");
            string edited = EditorGUILayout.TextField(current, GUILayout.Width(Mathf.Max(40f, rowWidth - EditorGUIUtility.labelWidth - 36f)), GUILayout.ExpandWidth(false));
            ReportTableCellRect(probe, index);
            HelpInfoButton.Draw("Credit", StoryCreditHelp);
            EditorRowEnd();
            if (edited != current) item.value = edited;
        }

        // What still keeps this row off the card, in the Editor's words (an empty required field is already marked on its label)
        private static void DrawElementProblems(ElementScope scope, ElementRegistry elements, ElementRow row)
        {
            var lines = new List<string>();
            foreach (var problem in ElementValidation.Of(scope, elements))
            {
                if (problem.RowId != row.Id) continue;
                switch (problem.Kind)
                {
                    case ElementValidation.Kind.MissingRequired: lines.Add(problem.FieldLabel + " is empty."); break;
                    case ElementValidation.Kind.InvalidMedia: lines.Add("Item " + problem.Media + ": not a file of the right kind inside the wall's Media Folder."); break;
                    case ElementValidation.Kind.MediaCount: lines.Add("It holds " + problem.Media + ": " + problem.ElementName + " needs the number above."); break;
                    case ElementValidation.Kind.UnknownElement: lines.Add("No element of this kind is registered."); break;
                }
            }
            if (lines.Count > 0) EditorGUILayout.HelpBox("Not shown on the card yet: " + string.Join(" ", lines), MessageType.Warning);
        }

        // "Tone: Neutral, Size: Large, Alignment: Start"
        private static string LookSummary(ElementRow row, ElementKindDefinition element)
        {
            var parts = new List<string>();
            foreach (var style in element.StyleFields)
            {
                var options = StoryArray(style.Options);
                parts.Add(style.Label + ": " + StoryLabel(options, StoryArray(style.OptionLabels), ElementOptions.OneOf(row.Value(style.Key), options)).text);
            }
            return string.Join(", ", parts);
        }
    }
}
