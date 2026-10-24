// POIEditorToolWindow.BlockFieldDrawer.cs
//
// Partial: the ONE generic drawer of a block's fields (_3.1 section 8.2). A kind's rows come from its
// BlockKindDefinition, so a kind an app registers is edited exactly like a built-in one with no Editor code.
// One drawer per BlockFieldType; a type gets its drawer with the first kind that uses it (HasBlockFieldDrawer,
// guarded by DetailCardEditorTabTests). Drawing never writes: a value is created only when the developer types.
// Split by type family (_3.2 step 4): this file dispatches and draws the text and Items rows and holds the value
// accessors; the single-value rows are in .Values.cs, the reference popups (PoiRef, BlockRef) in .References.cs,
// the media rows in .Assets.cs.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    public partial class POIEditorToolWindow
    {
        private const float BlockItemButtonWidth = 22f;

        // Whether the Editor can draw fields of this type today
        internal static bool HasBlockFieldDrawer(BlockFieldType type) =>
            type == BlockFieldType.LocalizedText || type == BlockFieldType.LocalizedLongText || type == BlockFieldType.Items
            || type == BlockFieldType.Choice || type == BlockFieldType.Color || type == BlockFieldType.Toggle
            || type == BlockFieldType.PoiRef || type == BlockFieldType.Asset || type == BlockFieldType.Number || type == BlockFieldType.Url
            || type == BlockFieldType.Time || type == BlockFieldType.BlockRef;

        // Every field of one block of `poi`, as its kind defines them. `built`: the POI's card as BlockStackBuilder builds it (a BlockRef
        // field says where its pick leads from it)
        private void DrawBlockFields(BlockInstanceData block, BlockKindDefinition definition, int blockIndex, POIData poi, BlockStackBuilder.Result built)
        {
            var languages = _config.card_settings?.languages;
            if (languages == null || languages.Count == 0)
            {
                EditorGUILayout.HelpBox(CardNoLanguageNote, MessageType.Warning);
                return;
            }
            var card = new BlockRefCard { Poi = poi, Holder = block, Built = built, Languages = languages };
            // - the frame's variant is every block's, not a field of its kind: the first row of an open block
            block.emphasis = DrawPopupField("Emphasis", BlockFrame.EmphasisOf(block.emphasis), CardOptions.Emphases, CardOptions.EmphasisLabels,
                CardBlockEmphasisHelp, IndentLevel1);
            foreach (var field in definition.Fields)
            {
                // - a field the block's look or options do not use is not drawn; its value stays (hiding is drawing, never writing)
                if (!FieldVisibilityRule.IsShown(field, block, definition, _config.card_settings)) continue;
                if (!HasBlockFieldDrawer(field.Type))
                {
                    EditorGUILayout.HelpBox(field.Label + ": this kind of field cannot be edited here yet.", MessageType.Info);
                    continue;
                }
                if (field.Type == BlockFieldType.Items)
                    DrawItemsBlockField(block, definition, field, languages, blockIndex, card);
                else if (field.Type == BlockFieldType.Choice)
                    DrawChoiceRow(field, IndentLevel1, "Block field " + field.Key, blockIndex,
                        () => ChoiceValue(block, field.Key), value => SetChoiceValue(block, field.Key, value),
                        BlockChoiceDefaultLabel(field, BlockLibraryRule.ChoiceDefault(_config.card_settings, definition, field)));
                else if (field.Type == BlockFieldType.Color)
                    DrawColorRow(field, IndentLevel1, "Block field " + field.Key, blockIndex,
                        () => ChoiceValue(block, field.Key), value => SetChoiceValue(block, field.Key, value));
                else if (field.Type == BlockFieldType.PoiRef)
                    DrawPoiRefRow(field, IndentLevel1, "Block field " + field.Key, blockIndex, _config.pois,
                        () => ChoiceValue(block, field.Key), value => SetChoiceValue(block, field.Key, value));
                else if (field.Type == BlockFieldType.BlockRef)
                    DrawBlockRefRow(field, IndentLevel1, "Block field " + field.Key, blockIndex, card,
                        () => ChoiceValue(block, field.Key), value => SetChoiceValue(block, field.Key, value));
                else if (field.Type == BlockFieldType.Toggle)
                    DrawToggleRow(field, IndentLevel1, "Block field " + field.Key, blockIndex,
                        // - a block that has stored nothing shows what the card will read: the wall's Block Library default
                        () => field.LibraryDefault ? BlockLibraryRule.Flag(_config.card_settings, definition, field, new BlockFieldReader(block, null, null)) : FlagValue(block, field.Key),
                        value => EnsureBlockField(block, field.Key).flag = value);
                else if (field.Type == BlockFieldType.Asset)
                {
                    DrawAssetRow(field, IndentLevel1, "Block field " + field.Key, blockIndex, _config.card_settings?.media_resources_path,
                        _config.card_settings?.default_media_library_resources_path,
                        () => AssetValue(block, field.Key), value => EnsureBlockField(block, field.Key).asset = value);
                    if (field.PerLanguage)
                        DrawPerLanguageAssetRows(field, languages, IndentLevel1, "Block field " + field.Key, blockIndex,
                            lang => LanguageEntryValue(block.fields?.Find(f => f != null && f.key == field.Key)?.asset_by_language, lang),
                            (lang, value) => SetLanguageEntry(EnsureBlockField(block, field.Key).asset_by_language, lang, value));
                }
                else if (field.Type == BlockFieldType.Number)
                    DrawNumberRow(field, IndentLevel1, "Block field " + field.Key, blockIndex,
                        () => NumberValue(block, field), value => EnsureBlockField(block, field.Key).number = value);
                else if (field.Type == BlockFieldType.Url)
                    DrawUrlRow(field, IndentLevel1, "Block field " + field.Key, blockIndex,
                        () => ChoiceValue(block, field.Key), value => SetChoiceValue(block, field.Key, value));
                else if (field.Type == BlockFieldType.Time)
                    DrawTimeRow(field, IndentLevel1, "Block field " + field.Key, blockIndex,
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
        private void DrawItemsBlockField(BlockInstanceData block, BlockKindDefinition definition, BlockFieldDefinition field, List<string> languages, int blockIndex,
            BlockRefCard card)
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
                    if (FieldVisibilityRule.IsShown(sub, item, field, block, definition, _config.card_settings))
                        DrawItemSubField(item, sub, languages, probe + i + " " + sub.Key, blockIndex, _config.card_settings?.media_resources_path,
                            _config.card_settings?.default_media_library_resources_path, _config.pois, card);
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
        private void DrawItemSubField(BlockItemData item, BlockFieldDefinition sub, List<string> languages, string probeName, int probeIndex, string mediaFolder,
            string defaultLibraryPath, List<POIData> pois, BlockRefCard card)
        {
            if (sub.Type == BlockFieldType.Asset)
            {
                DrawAssetRow(sub, IndentLevel2, probeName, probeIndex, mediaFolder, defaultLibraryPath,
                    () => ItemAssetValue(item, sub.Key), value => EnsureItemField(item, sub.Key).asset = value);
                if (sub.PerLanguage)
                    DrawPerLanguageAssetRows(sub, languages, IndentLevel2, probeName, probeIndex,
                        lang => LanguageEntryValue(item.fields?.Find(f => f != null && f.key == sub.Key)?.asset_by_language, lang),
                        (lang, value) => SetLanguageEntry(EnsureItemField(item, sub.Key).asset_by_language, lang, value));
            }
            else if (sub.Type == BlockFieldType.Choice)
                // - an empty row reads as the field's default (a tile's On Tap moves the card): the popup says so instead of "(none)"
                DrawChoiceRow(sub, IndentLevel2, probeName, probeIndex, () => ItemChoiceValue(item, sub.Key), value => SetItemChoiceValue(item, sub.Key, value),
                    BlockChoiceDefaultLabel(sub, sub.ChoiceDefault));
            else if (sub.Type == BlockFieldType.Color)
                DrawColorRow(sub, IndentLevel2, probeName, probeIndex, () => ItemChoiceValue(item, sub.Key), value => SetItemChoiceValue(item, sub.Key, value));
            else if (sub.Type == BlockFieldType.Toggle)
                DrawToggleRow(sub, IndentLevel2, probeName, probeIndex, () => ItemFlagValue(item, sub.Key), value => EnsureItemField(item, sub.Key).flag = value);
            else if (sub.Type == BlockFieldType.Number)
                DrawNumberRow(sub, IndentLevel2, probeName, probeIndex, () => BlockFieldReader.ItemNumber(item, sub), value => EnsureItemField(item, sub.Key).number = value);
            else if (sub.Type == BlockFieldType.PoiRef)
                DrawPoiRefRow(sub, IndentLevel2, probeName, probeIndex, pois, () => ItemChoiceValue(item, sub.Key), value => SetItemChoiceValue(item, sub.Key, value));
            else if (sub.Type == BlockFieldType.BlockRef)
                DrawBlockRefRow(sub, IndentLevel2, probeName, probeIndex, card, () => ItemChoiceValue(item, sub.Key), value => SetItemChoiceValue(item, sub.Key, value));
            else if (sub.Type == BlockFieldType.Url)
                DrawUrlRow(sub, IndentLevel2, probeName, probeIndex, () => ItemChoiceValue(item, sub.Key), value => SetItemChoiceValue(item, sub.Key, value));
            else if (sub.Type == BlockFieldType.Time)
                DrawTimeRow(sub, IndentLevel2, probeName, probeIndex, () => ItemChoiceValue(item, sub.Key), value => SetItemChoiceValue(item, sub.Key, value));
            else
                DrawLocalizedRows(sub, languages, IndentLevel2, probeName, probeIndex,
                    lang => ItemLocalizedValue(item, sub.Key, lang), (lang, text) => SetItemLocalizedValue(item, sub.Key, lang, text));
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
