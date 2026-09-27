// POIEditorToolWindow.BlockFieldDrawer.cs
//
// Partial: the ONE generic drawer of a block's fields (_3.1 section 8.2). A kind's rows come from its
// BlockKindDefinition, so a kind an app registers is edited exactly like a built-in one with no Editor code.
// One drawer per BlockFieldType; a type gets its drawer with the first kind that uses it (HasBlockFieldDrawer,
// guarded by DetailCardEditorTabTests). Drawing never writes: a value is created only when the developer types.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    public partial class POIEditorToolWindow
    {
        private const float BlockItemButtonWidth = 22f;
        private const string BlockChoiceNoneLabel = "(none)";
        private const string BlockPoiMissingLabel = "(missing)";
        // GUILayout's own gap between two controls on one row (the colour picker and its hex field)
        private const float BlockRowControlGap = 3f;

        // Whether the Editor can draw fields of this type today
        internal static bool HasBlockFieldDrawer(BlockFieldType type) =>
            type == BlockFieldType.LocalizedText || type == BlockFieldType.LocalizedLongText || type == BlockFieldType.Items
            || type == BlockFieldType.Choice || type == BlockFieldType.Color || type == BlockFieldType.Toggle
            || type == BlockFieldType.PoiRef || type == BlockFieldType.Asset || type == BlockFieldType.Number;

        // Every field of one block, as its kind defines them
        private void DrawBlockFields(BlockInstanceData block, BlockKindDefinition definition, int blockIndex)
        {
            var languages = _config.card_settings?.languages;
            if (languages == null || languages.Count == 0)
            {
                EditorGUILayout.HelpBox(CardNoLanguageNote, MessageType.Warning);
                return;
            }
            foreach (var field in definition.Fields)
            {
                if (!HasBlockFieldDrawer(field.Type))
                {
                    EditorGUILayout.HelpBox(field.Label + ": this kind of field cannot be edited here yet.", MessageType.Info);
                    continue;
                }
                if (field.Type == BlockFieldType.Items)
                    DrawItemsBlockField(block, field, languages, blockIndex);
                else if (field.Type == BlockFieldType.Choice)
                    DrawChoiceRow(field, IndentLevel1, "Block field " + field.Key, blockIndex,
                        () => ChoiceValue(block, field.Key), value => SetChoiceValue(block, field.Key, value));
                else if (field.Type == BlockFieldType.Color)
                    DrawColorRow(field, IndentLevel1, "Block field " + field.Key, blockIndex,
                        () => ChoiceValue(block, field.Key), value => SetChoiceValue(block, field.Key, value));
                else if (field.Type == BlockFieldType.PoiRef)
                    DrawPoiRefRow(field, IndentLevel1, "Block field " + field.Key, blockIndex, _config.pois,
                        () => ChoiceValue(block, field.Key), value => SetChoiceValue(block, field.Key, value));
                else if (field.Type == BlockFieldType.Toggle)
                    DrawToggleRow(field, IndentLevel1, "Block field " + field.Key, blockIndex,
                        () => FlagValue(block, field.Key), value => EnsureBlockField(block, field.Key).flag = value);
                else if (field.Type == BlockFieldType.Asset)
                    DrawAssetRow(field, IndentLevel1, "Block field " + field.Key, blockIndex, _config.card_settings?.media_resources_path,
                        () => AssetValue(block, field.Key), value => EnsureBlockField(block, field.Key).asset = value);
                else if (field.Type == BlockFieldType.Number)
                    DrawNumberRow(field, IndentLevel1, "Block field " + field.Key, blockIndex,
                        () => NumberValue(block, field), value => EnsureBlockField(block, field.Key).number = value);
                else
                    DrawLocalizedRows(field, languages, IndentLevel1, "Block field " + field.Key, blockIndex,
                        lang => LocalizedValue(block, field.Key, lang), (lang, text) => SetLocalizedValue(block, field.Key, lang, text));
            }
        }

        // A localized field: its label + (i), then one row per wall language (a text area for long text)
        private static void DrawLocalizedRows(BlockFieldDefinition field, List<string> languages, float indent, string probeName, int probeIndex,
            Func<string, string> get, Action<string, string> set)
        {
            DrawEditorRow(out float labelRow, out _, indent);
            GUILayout.Label(field.Label + (field.Required ? " (required)" : ""), EditorStyles.miniBoldLabel,
                GUILayout.Width(Mathf.Max(40f, labelRow - 36f)), GUILayout.ExpandWidth(false));
            HelpInfoButton.Draw(field.Label, field.Help);
            EditorRowEnd();

            bool longText = field.Type == BlockFieldType.LocalizedLongText;
            foreach (string lang in languages)
            {
                string current = get(lang);
                DrawEditorRow(out float rowWidth, out _, indent);
                EditorGUILayout.PrefixLabel(lang);
                float width = Mathf.Max(40f, rowWidth - EditorGUIUtility.labelWidth);
                string edited = longText
                    ? EditorGUILayout.TextArea(current, new GUIStyle(EditorStyles.textArea) { wordWrap = true }, GUILayout.Width(width),
                        GUILayout.MinHeight(EditorGUIUtility.singleLineHeight * 2.5f), GUILayout.ExpandWidth(false))
                    : EditorGUILayout.TextField(current, GUILayout.Width(width), GUILayout.ExpandWidth(false));
                ReportTableCellRect(probeName + " " + lang, probeIndex);
                EditorRowEnd();
                // - write only a real change: drawing an untranslated field must not create an empty entry
                if (edited != current) set(lang, edited);
            }
        }

        // An Items field (a repeater): its label + (i), then per row a "Row N" line with up / down / delete and the row's
        // own fields, then "+ Add row". The field value is created by the first Add, never by drawing.
        private void DrawItemsBlockField(BlockInstanceData block, BlockFieldDefinition field, List<string> languages, int blockIndex)
        {
            DrawEditorRow(out float labelRow, out _, IndentLevel1);
            GUILayout.Label(field.Label + (field.Required ? " (required)" : ""), EditorStyles.miniBoldLabel,
                GUILayout.Width(Mathf.Max(40f, labelRow - 36f)), GUILayout.ExpandWidth(false));
            HelpInfoButton.Draw(field.Label, field.Help);
            EditorRowEnd();

            var items = block.fields?.Find(f => f != null && f.key == field.Key)?.items;
            int count = items?.Count ?? 0;
            int deleteAt = -1, moveAt = -1, moveBy = 0;
            bool add = false;
            string probe = "Block item " + field.Key + " ";

            for (int i = 0; i < count; i++)
            {
                var item = items[i];
                DrawEditorRow(out _, out _, IndentLevel1);
                GUILayout.Label("Row " + (i + 1), EditorStyles.boldLabel, GUILayout.Width(60f));
                using (new EditorGUI.DisabledScope(i == 0))
                    if (GUILayout.Button("^", EditorStyles.miniButton, GUILayout.Width(BlockItemButtonWidth))) { moveAt = i; moveBy = -1; }
                ReportTableCellRect(probe + i + " up", blockIndex);
                using (new EditorGUI.DisabledScope(i >= count - 1))
                    if (GUILayout.Button("v", EditorStyles.miniButton, GUILayout.Width(BlockItemButtonWidth))) { moveAt = i; moveBy = 1; }
                ReportTableCellRect(probe + i + " down", blockIndex);
                // - the delete right beside its row's own buttons, never at the far edge of a wide window
                GUILayout.Space(TableGapBeforeDelete);
                if (DeleteButton.DrawLayout("Delete row " + (i + 1) + " of " + field.Label)) deleteAt = i;
                ReportTableCellRect(probe + i + " delete", blockIndex);
                GUILayout.FlexibleSpace();
                EditorRowEnd();

                foreach (var sub in field.ItemFields)
                    DrawItemSubField(item, sub, languages, probe + i + " " + sub.Key, blockIndex, _config.card_settings?.media_resources_path);
            }

            DrawEditorRow(out float addRow, out _, IndentLevel1);
            if (GUILayout.Button("+ Add row to " + field.Label, GUILayout.Width(Mathf.Max(40f, addRow)), GUILayout.ExpandWidth(false))) add = true;
            ReportTableCellRect(probe + "add", blockIndex);
            EditorRowEnd();

            // - applied after the rows: a row is never left half-drawn
            if (add) EnsureBlockField(block, field.Key).items.Add(new BlockItemData());
            if (deleteAt >= 0) items.RemoveAt(deleteAt);
            if (moveAt >= 0) (items[moveAt], items[moveAt + moveBy]) = (items[moveAt + moveBy], items[moveAt]);
        }

        // One sub-field of one Items row
        private static void DrawItemSubField(BlockItemData item, BlockFieldDefinition sub, List<string> languages, string probeName, int probeIndex, string mediaFolder)
        {
            if (sub.Type == BlockFieldType.Asset)
                DrawAssetRow(sub, IndentLevel2, probeName, probeIndex, mediaFolder, () => ItemAssetValue(item, sub.Key), value => EnsureItemField(item, sub.Key).asset = value);
            else if (sub.Type == BlockFieldType.Choice)
                DrawChoiceRow(sub, IndentLevel2, probeName, probeIndex, () => ItemChoiceValue(item, sub.Key), value => SetItemChoiceValue(item, sub.Key, value));
            else if (sub.Type == BlockFieldType.Color)
                DrawColorRow(sub, IndentLevel2, probeName, probeIndex, () => ItemChoiceValue(item, sub.Key), value => SetItemChoiceValue(item, sub.Key, value));
            else if (sub.Type == BlockFieldType.Toggle)
                DrawToggleRow(sub, IndentLevel2, probeName, probeIndex, () => ItemFlagValue(item, sub.Key), value => EnsureItemField(item, sub.Key).flag = value);
            else
                DrawLocalizedRows(sub, languages, IndentLevel2, probeName, probeIndex,
                    lang => ItemLocalizedValue(item, sub.Key, lang), (lang, text) => SetItemLocalizedValue(item, sub.Key, lang, text));
        }

        // A Choice field: a popup of the definition's option labels (the value stored is the option, never the label). An
        // optional field offers "(none)"; a value that is no longer an option stays, shown as "<value> (missing)", until
        // the developer picks another -- drawing never rewrites it.
        private static void DrawChoiceRow(BlockFieldDefinition field, float indent, string probeName, int probeIndex, Func<string> get, Action<string> set)
        {
            var (values, labels) = BlockChoiceOptions(field, get());
            int index = values.IndexOf(get());
            DrawEditorRow(out float rowWidth, out _, indent);
            EditorGUILayout.PrefixLabel(field.Label + (field.Required ? " (required)" : ""));
            int picked = EditorGUILayout.Popup(index, labels.ToArray(), GUILayout.Width(Mathf.Max(40f, rowWidth - EditorGUIUtility.labelWidth - 36f)), GUILayout.ExpandWidth(false));
            ReportTableCellRect(probeName, probeIndex);
            HelpInfoButton.Draw(field.Label, field.Help);
            EditorRowEnd();
            if (picked >= 0 && picked != index) set(values[picked]);
        }

        // A Color field: the picker + hex pair every colour row of the window uses (DrawColorSwatchAndHex), stored as the
        // hex text. Text that is not a colour the card accepts (BlockFieldReader.TryParseColor) stays as typed, with a
        // warning under it -- the card leaves that row out until it is fixed. Drawing never writes.
        private static void DrawColorRow(BlockFieldDefinition field, float indent, string probeName, int probeIndex, Func<string> get, Action<string> set)
        {
            string current = get();
            string edited = current;
            DrawEditorRow(out float rowWidth, out _, indent);
            EditorGUILayout.PrefixLabel(field.Label + (field.Required ? " (required)" : ""));
            DrawColorSwatchAndHex(ref edited, out _, out Rect hexRect);
            ReportTableCellRect(probeName, probeIndex, hexRect);
            // - the (i) in the same column as every other row's (i): the value area ends 36 before the row's end (the
            //   Choice rows' rule); a fixed spacer, since a flexible one runs past the row's width cap
            GUILayout.Space(Mathf.Max(0f, rowWidth - EditorGUIUtility.labelWidth - 36f - ColorPickerWidth - ColorHexFieldWidth - BlockRowControlGap));
            HelpInfoButton.Draw(field.Label, field.Help);
            EditorRowEnd();
            if (edited != current) set(edited);
            if (!string.IsNullOrWhiteSpace(get()) && !BlockFieldReader.TryParseColor(get(), out _))
                EditorGUILayout.HelpBox(CardColorInvalidText(field.Label, get()), MessageType.Warning);
        }

        // A PoiRef field: a popup of this wall's POIs named as the POI list names them ("3. North tower", never an id). A
        // stored id no POI has any more stays selected as "(missing)" until the developer picks another -- drawing never
        // rewrites it (the ReferencePopupOptions rule every reference popup of the window follows).
        private static void DrawPoiRefRow(BlockFieldDefinition field, float indent, string probeName, int probeIndex, List<POIData> pois,
            Func<string> get, Action<string> set)
        {
            var options = PoiRefOptions(pois, get(), !field.Required);
            DrawEditorRow(out float rowWidth, out _, indent);
            EditorGUILayout.PrefixLabel(field.Label + (field.Required ? " (required)" : ""));
            int picked = EditorGUILayout.Popup(options.SelectedIndex, options.Labels, GUILayout.Width(Mathf.Max(40f, rowWidth - EditorGUIUtility.labelWidth - 36f)), GUILayout.ExpandWidth(false));
            ReportTableCellRect(probeName, probeIndex);
            HelpInfoButton.Draw(field.Label, field.Help);
            EditorRowEnd();
            if (picked != options.SelectedIndex) set(options.KeyAt(picked) ?? "");
        }

        // The options of a PoiRef popup: every POI by its list title, a blank value as "(none)", a stale id as "(missing)"
        internal static ReferencePopupOptions PoiRefOptions(List<POIData> pois, string current, bool allowNone)
        {
            var ids = new List<string>();
            var titles = new List<string>();
            for (int i = 0; i < (pois?.Count ?? 0); i++)
            {
                ids.Add(pois[i]?.id);
                titles.Add(EditorNames.Poi(i, pois[i]));
            }
            return ReferencePopupOptions.Build(ids, titles, current, allowNone, BlockPoiMissingLabel);
        }

        // A Toggle field: label + checkbox + (i); the value is created by the first real click, never by drawing
        private static void DrawToggleRow(BlockFieldDefinition field, float indent, string probeName, int probeIndex, Func<bool> get, Action<bool> set)
        {
            bool current = get();
            DrawEditorRow(out float rowWidth, out _, indent);
            EditorGUILayout.PrefixLabel(field.Label);
            bool edited = EditorGUILayout.Toggle(current, GUILayout.Width(EditorGUIUtility.singleLineHeight), GUILayout.ExpandWidth(false));
            ReportTableCellRect(probeName, probeIndex);
            GUILayout.Space(Mathf.Max(0f, rowWidth - EditorGUIUtility.labelWidth - 36f - EditorGUIUtility.singleLineHeight));
            HelpInfoButton.Draw(field.Label, field.Help);
            EditorRowEnd();
            if (edited != current) set(edited);
        }

        // An Asset field (a picture today): an object field that offers only files of the field's media kind, stored as the
        // path inside the wall's Media Folder (MediaPathRule.StoredPathFor). A file picked from outside that folder is
        // stored as picked and warned about (the card leaves it out, BlockStackBuilder's InvalidMedia), never silently
        // refused; a stored path with no file behind it is warned about too. Drawing never writes.
        private static void DrawAssetRow(BlockFieldDefinition field, float indent, string probeName, int probeIndex, string mediaFolder,
            Func<string> get, Action<string> set)
        {
            string current = get();
            var shown = MediaAssetFor(current, mediaFolder);
            DrawEditorRow(out float rowWidth, out _, indent);
            EditorGUILayout.PrefixLabel(field.Label + (field.Required ? " (required)" : ""));
            var picked = EditorGUILayout.ObjectField(shown, typeof(Texture2D), false,
                GUILayout.Width(Mathf.Max(40f, rowWidth - EditorGUIUtility.labelWidth - 36f)), GUILayout.ExpandWidth(false));
            ReportTableCellRect(probeName, probeIndex);
            HelpInfoButton.Draw(field.Label, field.Help);
            EditorRowEnd();
            if (picked != shown) set(picked == null ? "" : MediaPathRule.StoredPathFor(AssetDatabase.GetAssetPath(picked), mediaFolder));

            string stored = get();
            var problem = MediaPathRule.Check(stored, field.Media);
            if (problem == MediaPathProblem.OutsideFolder || problem == MediaPathProblem.WrongType)
                EditorGUILayout.HelpBox(CardMediaProblemText(field.Label, field.Media, problem), MessageType.Warning);
            else if (problem == MediaPathProblem.None && MediaAssetFor(stored, mediaFolder) == null)
                EditorGUILayout.HelpBox(CardMediaMissingText(field.Label, stored), MessageType.Warning);
        }

        // The picture a stored path names: inside the Media Folder through Resources (what the app loads), a project path
        // as picked (outside the folder: shown so the developer sees what they chose), else none
        internal static Texture2D MediaAssetFor(string stored, string mediaFolder)
        {
            if (string.IsNullOrWhiteSpace(stored)) return null;
            string p = stored.Trim().Replace('\\', '/');
            if (p.StartsWith("Assets/")) return AssetDatabase.LoadAssetAtPath<Texture2D>(p);
            if (!MediaPathRule.IsValid(p, MediaKind.Image)) return null;
            string folder = (mediaFolder ?? "").Trim().Trim('/');
            string noExtension = p.Substring(0, p.LastIndexOf('.'));
            return Resources.Load<Texture2D>(folder.Length > 0 ? folder + "/" + noExtension : noExtension);
        }

        // A Number field: a slider over the definition's range, showing its default while nothing is stored; the value is
        // created by the first real change, never by drawing
        private static void DrawNumberRow(BlockFieldDefinition field, float indent, string probeName, int probeIndex, Func<float> get, Action<float> set)
        {
            float current = get();
            DrawEditorRow(out float rowWidth, out _, indent);
            EditorGUILayout.PrefixLabel(field.Label);
            float edited = EditorGUILayout.Slider(current, field.NumberMin, field.NumberMax,
                GUILayout.Width(Mathf.Max(40f, rowWidth - EditorGUIUtility.labelWidth - 36f)), GUILayout.ExpandWidth(false));
            ReportTableCellRect(probeName, probeIndex);
            HelpInfoButton.Draw(field.Label, field.Help);
            EditorRowEnd();
            if (!Mathf.Approximately(edited, current)) set(edited);
        }

        internal static string AssetValue(BlockInstanceData block, string key) => block.fields?.Find(f => f != null && f.key == key)?.asset ?? "";

        internal static string ItemAssetValue(BlockItemData item, string key) => item.fields?.Find(f => f != null && f.key == key)?.asset ?? "";

        internal static float NumberValue(BlockInstanceData block, BlockFieldDefinition field) => new BlockFieldReader(block, null, null).Number(field);

        internal static bool FlagValue(BlockInstanceData block, string key) => block.fields?.Find(f => f != null && f.key == key)?.flag ?? false;

        internal static bool ItemFlagValue(BlockItemData item, string key) => item.fields?.Find(f => f != null && f.key == key)?.flag ?? false;

        // The sub-field of this key in one row, created when missing (only ever called by a real edit)
        private static BlockItemFieldValue EnsureItemField(BlockItemData item, string key)
        {
            item.fields ??= new List<BlockItemFieldValue>();
            var v = item.fields.Find(f => f != null && f.key == key);
            if (v == null) item.fields.Add(v = new BlockItemFieldValue { key = key });
            return v;
        }

        // The popup of a Choice field: its values and, in the same order, what the Editor shows for them
        internal static (List<string> Values, List<string> Labels) BlockChoiceOptions(BlockFieldDefinition field, string current)
        {
            var values = new List<string>();
            var labels = new List<string>();
            if (!field.Required)
            {
                values.Add("");
                labels.Add(BlockChoiceNoneLabel);
            }
            for (int i = 0; i < field.Options.Count; i++)
            {
                values.Add(field.Options[i]);
                labels.Add(field.OptionLabels != null ? field.OptionLabels[i] : field.Options[i]);
            }
            if (!string.IsNullOrEmpty(current) && !values.Contains(current))
            {
                values.Add(current);
                labels.Add(current + " (missing)");
            }
            return (values, labels);
        }

        internal static string ChoiceValue(BlockInstanceData block, string key) => block.fields?.Find(f => f != null && f.key == key)?.value ?? "";

        internal static void SetChoiceValue(BlockInstanceData block, string key, string value) => EnsureBlockField(block, key).value = value;

        internal static string ItemChoiceValue(BlockItemData item, string key) => item.fields?.Find(f => f != null && f.key == key)?.value ?? "";

        internal static void SetItemChoiceValue(BlockItemData item, string key, string value)
        {
            item.fields ??= new List<BlockItemFieldValue>();
            var v = item.fields.Find(f => f != null && f.key == key);
            if (v == null) item.fields.Add(v = new BlockItemFieldValue { key = key });
            v.value = value;
        }

        // The field value of this key, created when missing (only ever called by a real edit)
        private static BlockFieldValue EnsureBlockField(BlockInstanceData block, string key)
        {
            block.fields ??= new List<BlockFieldValue>();
            var value = block.fields.Find(f => f != null && f.key == key);
            if (value == null) block.fields.Add(value = new BlockFieldValue { key = key });
            value.items ??= new List<BlockItemData>();
            return value;
        }

        // The text of one language of a field ("" when there is none)
        internal static string LocalizedValue(BlockInstanceData block, string fieldKey, string lang)
        {
            var value = block.fields?.Find(f => f != null && f.key == fieldKey);
            return value?.text?.Find(e => e != null && e.lang == lang)?.value ?? "";
        }

        // Set the text of one language of a field, creating the field and the language entry when missing
        internal static void SetLocalizedValue(BlockInstanceData block, string fieldKey, string lang, string text)
        {
            var value = EnsureBlockField(block, fieldKey);
            value.text ??= new List<LocalizedEntry>();
            SetLanguage(value.text, lang, text);
        }

        // The text of one language of an Items row's sub-field ("" when there is none)
        internal static string ItemLocalizedValue(BlockItemData item, string fieldKey, string lang)
        {
            var value = item.fields?.Find(f => f != null && f.key == fieldKey);
            return value?.text?.Find(e => e != null && e.lang == lang)?.value ?? "";
        }

        internal static void SetItemLocalizedValue(BlockItemData item, string fieldKey, string lang, string text)
        {
            item.fields ??= new List<BlockItemFieldValue>();
            var value = item.fields.Find(f => f != null && f.key == fieldKey);
            if (value == null) item.fields.Add(value = new BlockItemFieldValue { key = fieldKey });
            value.text ??= new List<LocalizedEntry>();
            SetLanguage(value.text, lang, text);
        }

        private static void SetLanguage(List<LocalizedEntry> texts, string lang, string text)
        {
            var entry = texts.Find(e => e != null && e.lang == lang);
            if (entry == null) texts.Add(entry = new LocalizedEntry { lang = lang });
            entry.value = text;
        }
    }
}
