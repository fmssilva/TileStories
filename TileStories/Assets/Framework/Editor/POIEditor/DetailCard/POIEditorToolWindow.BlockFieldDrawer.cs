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

        // Whether the Editor can draw fields of this type today
        internal static bool HasBlockFieldDrawer(BlockFieldType type) =>
            type == BlockFieldType.LocalizedText || type == BlockFieldType.LocalizedLongText || type == BlockFieldType.Items
            || type == BlockFieldType.Choice;

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
                    DrawItemSubField(item, sub, languages, probe + i + " " + sub.Key, blockIndex);
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
        private static void DrawItemSubField(BlockItemData item, BlockFieldDefinition sub, List<string> languages, string probeName, int probeIndex)
        {
            if (sub.Type == BlockFieldType.Choice)
                DrawChoiceRow(sub, IndentLevel2, probeName, probeIndex, () => ItemChoiceValue(item, sub.Key), value => SetItemChoiceValue(item, sub.Key, value));
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
