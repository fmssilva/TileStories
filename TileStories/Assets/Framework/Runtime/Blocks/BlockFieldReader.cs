using System.Collections.Generic;

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
        public bool Flag(string key) => Find(key)?.flag ?? false;
        public string Value(string key) => Find(key)?.value ?? "";
        public string Asset(string key) => Find(key)?.asset ?? "";
        public IReadOnlyList<BlockItemData> Items(string key) => (IReadOnlyList<BlockItemData>)Find(key)?.items ?? System.Array.Empty<BlockItemData>();

        // A localized sub-field of one Items row
        public string ItemText(BlockItemData item, string key) => Pick(FindItemField(item, key)?.text, _language, _fallback);

        // The Choice value of one Items row's sub-field ("" when there is none)
        public string ItemValue(BlockItemData item, string key) => FindItemField(item, key)?.value ?? "";

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
                case BlockFieldType.Asset:
                    return !string.IsNullOrWhiteSpace(value.asset);
                case BlockFieldType.Items:
                    return value.items != null && value.items.Count > 0;
                default:
                    return true; // a number or a toggle always has a value
            }
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
