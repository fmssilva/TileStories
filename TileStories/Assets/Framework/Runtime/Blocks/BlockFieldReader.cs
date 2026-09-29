using System.Collections.Generic;
using UnityEngine;

namespace TileStories
{
    // Typed, language-aware reads of one block's fields (_3.1 section 3). A localized field gives the visitor's
    // language, else the wall's fallback language (card_settings.languages[0]), else the first language that has
    // text -- never an empty card because one translation is missing. A missing field reads as empty.
    public sealed class BlockFieldReader
    {
        private readonly BlockInstanceData _instance;
        private readonly string _language;
        private readonly string _fallback;

        public BlockFieldReader(BlockInstanceData instance, string language, string fallbackLanguage)
        {
            _instance = instance;
            _language = language;
            _fallback = fallbackLanguage;
        }

        public string Text(string key) => Pick(Find(key)?.text, _language, _fallback);
        public float Number(string key) => Find(key)?.number ?? 0f;
        // A Number field, or its definition's default while none is stored, kept inside the definition's range
        public float Number(BlockFieldDefinition field)
        {
            var value = Find(field.Key);
            return value == null ? field.NumberDefault : Mathf.Clamp(value.number, field.NumberMin, field.NumberMax);
        }
        public bool Flag(string key) => Find(key)?.flag ?? false;
        public string Value(string key) => Find(key)?.value ?? "";
        // Whether this field was ever written (a Number or Toggle that was never set reads its default: not stored)
        public bool Stored(string key) => Find(key) != null;
        // A Url field's link when WebLinkRule opens it, else ""
        public string OpenableUrl(string key) => WebLinkRule.Openable(Value(key));
        public string Asset(string key) => Find(key)?.asset ?? "";
        // An image field's path when MediaPathRule accepts it, else "" (the view shows its "unavailable" state)
        public string ValidAsset(string key, MediaKind kind) => MediaPathRule.IsValid(Asset(key), kind) ? Asset(key).Trim() : "";
        public IReadOnlyList<BlockItemData> Items(string key) => (IReadOnlyList<BlockItemData>)Find(key)?.items ?? System.Array.Empty<BlockItemData>();

        // A localized sub-field of one Items row
        public string ItemText(BlockItemData item, string key) => Pick(FindItemField(item, key)?.text, _language, _fallback);

        // The Choice value of one Items row's sub-field ("" when there is none)
        public string ItemValue(BlockItemData item, string key) => FindItemField(item, key)?.value ?? "";

        // The Asset of one Items row's sub-field when MediaPathRule accepts it, else ""
        public string ItemValidAsset(BlockItemData item, string key, MediaKind kind)
        {
            string path = FindItemField(item, key)?.asset ?? "";
            return MediaPathRule.IsValid(path, kind) ? path.Trim() : "";
        }

        // The Toggle of one Items row's sub-field
        public bool ItemFlag(BlockItemData item, string key) => FindItemField(item, key)?.flag ?? false;
        // A Number sub-field of one row, or its definition's default while none is stored, kept inside the definition's range
        // (static: the Editor's slider shows the same value)
        public static float ItemNumber(BlockItemData item, BlockFieldDefinition sub)
        {
            var value = FindItemField(item, sub.Key);
            return value == null ? sub.NumberDefault : Mathf.Clamp(value.number, sub.NumberMin, sub.NumberMax);
        }

        // The colour of one Items row's Color sub-field; false when it is empty or not a colour (TryParseColor)
        public bool ItemColor(BlockItemData item, string key, out Color color) => TryParseColor(FindItemField(item, key)?.value, out color);

        // The seconds of one Items row's Time sub-field; false when it is empty or not a time (TimeCodeRule)
        public bool ItemTime(BlockItemData item, string key, out float seconds) => TimeCodeRule.TryParse(FindItemField(item, key)?.value, out seconds);

        // Whether the field holds anything a visitor would see (a required field that fails this hides the block)
        public static bool HasContent(BlockFieldValue value, BlockFieldType type)
        {
            if (value == null) return false;
            switch (type)
            {
                case BlockFieldType.LocalizedText:
                case BlockFieldType.LocalizedLongText:
                    return Pick(value.text, null, null).Length > 0;
                case BlockFieldType.Choice:
                case BlockFieldType.PoiRef:
                    return !string.IsNullOrWhiteSpace(value.value);
                case BlockFieldType.Color:
                    return TryParseColor(value.value, out _);
                case BlockFieldType.Url:
                    return WebLinkRule.IsOpenable(value.value);
                case BlockFieldType.Time:
                    return TimeCodeRule.IsValid(value.value);
                case BlockFieldType.Asset:
                    return !string.IsNullOrWhiteSpace(value.asset);
                case BlockFieldType.Items:
                    return value.items != null && value.items.Count > 0;
                default:
                    return true; // a number or a toggle always has a value
            }
        }

        // Whether one Items row is complete: every sub-field its definition marks Required holds something a visitor
        // would see (a Color: a real colour; a Time: a time TimeCodeRule reads; an Asset: a path MediaPathRule accepts). An incomplete row is not shown; a required Items field with no complete
        // row hides the block (BlockStackBuilder, NoCompleteRow). A row of a field with no required sub-field is complete.
        public static bool ItemIsComplete(BlockItemData item, IReadOnlyList<BlockFieldDefinition> itemFields)
        {
            if (item == null) return false;
            if (itemFields == null) return true;
            foreach (var sub in itemFields)
                if (sub.Required && !HasItemContent(FindItemField(item, sub.Key), sub)) return false;
            return true;
        }

        // Same as HasContent, for one sub-field of an Items row
        public static bool HasItemContent(BlockItemFieldValue value, BlockFieldDefinition sub)
        {
            if (value == null) return false;
            switch (sub.Type)
            {
                case BlockFieldType.LocalizedText:
                case BlockFieldType.LocalizedLongText:
                    return Pick(value.text, null, null).Length > 0;
                case BlockFieldType.Choice:
                case BlockFieldType.PoiRef:
                    return !string.IsNullOrWhiteSpace(value.value);
                case BlockFieldType.Color:
                    return TryParseColor(value.value, out _);
                case BlockFieldType.Url:
                    return WebLinkRule.IsOpenable(value.value);
                case BlockFieldType.Time:
                    return TimeCodeRule.IsValid(value.value);
                case BlockFieldType.Asset:
                    return MediaPathRule.IsValid(value.asset, sub.Media);
                default:
                    return true;
            }
        }

        // A content colour as the config stores it: "#RRGGBB" or "#RGB" (hex digits, either case, surrounding spaces
        // ignored). Stricter than ColorUtility on purpose: no colour names, no alpha -- one written form the Editor can
        // show and the card can trust.
        public static bool TryParseColor(string hex, out Color color)
        {
            color = default;
            if (string.IsNullOrWhiteSpace(hex)) return false;
            string h = hex.Trim();
            if ((h.Length != 4 && h.Length != 7) || h[0] != '#') return false;
            for (int i = 1; i < h.Length; i++)
                if (!System.Uri.IsHexDigit(h[i])) return false;
            return ColorUtility.TryParseHtmlString(h, out color);
        }

        // language -> fallback -> the first non-blank entry -> ""
        public static string Pick(List<LocalizedEntry> entries, string language, string fallback)
        {
            if (entries == null) return "";
            string byFallback = null, anyText = null;
            foreach (var e in entries)
            {
                if (e == null || string.IsNullOrWhiteSpace(e.value)) continue;
                if (e.lang == language) return e.value;
                if (byFallback == null && e.lang == fallback) byFallback = e.value;
                anyText ??= e.value;
            }
            return byFallback ?? anyText ?? "";
        }

        private BlockFieldValue Find(string key)
        {
            var fields = _instance?.fields;
            if (fields == null) return null;
            for (int i = 0; i < fields.Count; i++)
                if (fields[i] != null && fields[i].key == key) return fields[i];
            return null;
        }

        private static BlockItemFieldValue FindItemField(BlockItemData item, string key)
        {
            var fields = item?.fields;
            if (fields == null) return null;
            for (int i = 0; i < fields.Count; i++)
                if (fields[i] != null && fields[i].key == key) return fields[i];
            return null;
        }
    }
}
