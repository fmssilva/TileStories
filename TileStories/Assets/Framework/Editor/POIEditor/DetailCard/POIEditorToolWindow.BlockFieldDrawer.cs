// POIEditorToolWindow.BlockFieldDrawer.cs
//
// Partial: the ONE generic drawer of a block's fields (_3.1 section 8.2). A kind's rows come from its
// BlockKindDefinition, so a kind an app registers is edited exactly like a built-in one with no Editor code.
// One drawer per BlockFieldType; a type gets its drawer with the first kind that uses it (HasBlockFieldDrawer,
// guarded by DetailCardEditorTabTests). Drawing never writes: a value is created only when the developer types.

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    public partial class POIEditorToolWindow
    {
        // Whether the Editor can draw fields of this type today
        internal static bool HasBlockFieldDrawer(BlockFieldType type) =>
            type == BlockFieldType.LocalizedText || type == BlockFieldType.LocalizedLongText;

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
                DrawLocalizedBlockField(block, field, languages, blockIndex);
            }
        }

        // A localized field: its label + (i), then one row per wall language
        private static void DrawLocalizedBlockField(BlockInstanceData block, BlockFieldDefinition field, List<string> languages, int blockIndex)
        {
            DrawEditorRow(out float labelRow, out _, IndentLevel1);
            GUILayout.Label(field.Label + (field.Required ? " (required)" : ""), EditorStyles.miniBoldLabel,
                GUILayout.Width(Mathf.Max(40f, labelRow - 36f)), GUILayout.ExpandWidth(false));
            HelpInfoButton.Draw(field.Label, field.Help);
            EditorRowEnd();

            bool longText = field.Type == BlockFieldType.LocalizedLongText;
            foreach (string lang in languages)
            {
                string current = LocalizedValue(block, field.Key, lang);
                DrawEditorRow(out float rowWidth, out _, IndentLevel1);
                EditorGUILayout.PrefixLabel(lang);
                float width = Mathf.Max(40f, rowWidth - EditorGUIUtility.labelWidth);
                string edited = longText
                    ? EditorGUILayout.TextArea(current, new GUIStyle(EditorStyles.textArea) { wordWrap = true }, GUILayout.Width(width),
                        GUILayout.MinHeight(EditorGUIUtility.singleLineHeight * 2.5f), GUILayout.ExpandWidth(false))
                    : EditorGUILayout.TextField(current, GUILayout.Width(width), GUILayout.ExpandWidth(false));
                ReportTableCellRect("Block field " + field.Key + " " + lang, blockIndex);
                EditorRowEnd();
                // - write only a real change: drawing an untranslated field must not create an empty entry
                if (edited != current) SetLocalizedValue(block, field.Key, lang, edited);
            }
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
            block.fields ??= new List<BlockFieldValue>();
            var value = block.fields.Find(f => f != null && f.key == fieldKey);
            if (value == null) block.fields.Add(value = new BlockFieldValue { key = fieldKey });
            value.text ??= new List<LocalizedEntry>();
            var entry = value.text.Find(e => e != null && e.lang == lang);
            if (entry == null) value.text.Add(entry = new LocalizedEntry { lang = lang });
            entry.value = text;
        }
    }
}
