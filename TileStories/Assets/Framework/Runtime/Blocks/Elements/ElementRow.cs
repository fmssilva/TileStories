using System.Collections.Generic;

namespace TileStories
{
    // One row of an Elements field (_3.3 R14.1, R17), a typed view over the BlockItemData that stores it. A row is EITHER an element (its
    // kind's key, that kind's sub-fields and style values, an optional media list) OR a reference to another row (ElementRef). Both carry a
    // stable id (el_N, never reused within the block: BlockSurfaces.TakeRowId), ONE motion value (effect + strength, R17.3), a tap where the
    // element offers a choice (R17.4) and the developer-only Validated flag (R14.8, never shown to a visitor).
    // Everything lives under RESERVED sub-field keys of the row, not in new serialized members: JsonUtility writes every member a class has,
    // so a new one would rewrite every Items row of every config on its next save. Reading never writes; a setter creates its sub-field.
    public readonly struct ElementRow
    {
        public const string IdKey = "id";
        public const string ElementKey = "element";
        public const string MotionKey = "motion";
        public const string StrengthKey = "strength";
        public const string TapKey = "tap";
        public const string ValidatedKey = "validated";
        // The one key that repeats: each entry is one media item, in order (ElementMedia)
        public const string MediaKey = "media";
        public const string RefScopeKey = "ref_scope";
        public const string RefBlockKey = "ref_block";
        public const string RefRowKey = "ref_row";

        // The row's own keys: an element kind may not use one for a sub-field (ElementRegistry.Validate)
        public static readonly IReadOnlyList<string> ReservedKeys = new[]
        {
            IdKey, ElementKey, MotionKey, StrengthKey, TapKey, ValidatedKey, MediaKey, RefScopeKey, RefBlockKey, RefRowKey,
        };

        public readonly BlockItemData Data;

        public ElementRow(BlockItemData data) { Data = data; }

        public string Id => Value(IdKey);
        // The element kind's key ("paragraph"); blank on a reference row
        public string Kind => Value(ElementKey);
        public bool IsRef => !string.IsNullOrWhiteSpace(Value(RefScopeKey));
        public ElementRef Ref => new(Value(RefScopeKey), Value(RefBlockKey), Value(RefRowKey));
        // The stored effect and strength, an unknown or missing one read as the default (Auto, Subtle)
        public string Motion => ElementOptions.OneOf(Value(MotionKey), ElementOptions.Motions);
        public string Strength => ElementOptions.OneOf(Value(StrengthKey), ElementOptions.Strengths);
        // The stored tap ("" = none stored): ElementRules.TapOf reads it against what the element offers
        public string Tap => Value(TapKey);
        public bool Validated => Field(ValidatedKey)?.flag ?? false;

        // The row's media items in order (none for a reference row or an element without media)
        public IReadOnlyList<ElementMedia> Media
        {
            get
            {
                var media = new List<ElementMedia>();
                if (Data?.fields == null) return media;
                foreach (var f in Data.fields)
                    if (f != null && f.key == MediaKey) media.Add(new ElementMedia(f));
                return media;
            }
        }

        // The sub-field with this key (the first one; never a media entry), or null
        public BlockItemFieldValue Field(string key)
        {
            if (Data?.fields == null || key == MediaKey) return null;
            foreach (var f in Data.fields)
                if (f != null && f.key == key) return f;
            return null;
        }

        public string Value(string key) => Field(key)?.value ?? "";

        public void SetValue(string key, string value) => Ensure(key).value = value ?? "";

        public void SetMotion(string effect, string strength)
        {
            SetValue(MotionKey, effect);
            SetValue(StrengthKey, strength);
        }

        public void SetValidated(bool validated) => Ensure(ValidatedKey).flag = validated;

        // Make this row point at `target` (its element kind and media are left as they are: a reference shows its target's)
        public void SetRef(ElementRef target)
        {
            SetValue(RefScopeKey, target.Scope);
            SetValue(RefBlockKey, target.Block);
            SetValue(RefRowKey, target.Row);
        }

        // Add one media item at the end of the list
        public ElementMedia AddMedia(string asset, List<LocalizedEntry> captions, string credit)
        {
            var entry = new BlockItemFieldValue { key = MediaKey, asset = asset ?? "", text = captions ?? new List<LocalizedEntry>(), value = credit ?? "" };
            Data.fields.Add(entry);
            return new ElementMedia(entry);
        }

        // A NEW element row of `element` for `block`, born filled with the element's defaults (R14.8): a fresh id, every style option at its
        // default, every content Choice / Number / Toggle at its default, Motion Auto + Subtle, the element's first tap, not validated. Text
        // and file fields stay empty: the Editor marks them (ElementValidation names the required ones). The caller adds it to a row list.
        public static ElementRow Create(BlockInstanceData block, ElementKindDefinition element)
        {
            var row = new ElementRow(new BlockItemData());
            row.SetValue(IdKey, BlockSurfaces.TakeRowId(block));
            row.SetValue(ElementKey, element.Key);
            if (element.Fields != null)
                foreach (var field in element.Fields)
                {
                    if (field.Type == BlockFieldType.Choice && field.ChoiceDefault != null) row.SetValue(field.Key, field.ChoiceDefault);
                    else if (field.Type == BlockFieldType.Number) row.Ensure(field.Key).number = field.NumberDefault;
                    else if (field.Type == BlockFieldType.Toggle) row.Ensure(field.Key).flag = false;
                }
            if (element.StyleFields != null)
                foreach (var style in element.StyleFields) row.SetValue(style.Key, style.ChoiceDefault);
            row.SetMotion(ElementOptions.MotionAuto, ElementOptions.StrengthSubtle);
            if (element.TapChoices != null && element.TapChoices.Count > 0) row.SetValue(TapKey, element.TapChoices[0]);
            row.SetValidated(false);
            return row;
        }

        // A NEW reference row of `block` pointing at `target` (a Preview's linked row, R17.1): a fresh id, Motion Auto + Subtle, not validated
        public static ElementRow CreateRef(BlockInstanceData block, ElementRef target)
        {
            var row = new ElementRow(new BlockItemData());
            row.SetValue(IdKey, BlockSurfaces.TakeRowId(block));
            row.SetRef(target);
            row.SetMotion(ElementOptions.MotionAuto, ElementOptions.StrengthSubtle);
            row.SetValidated(false);
            return row;
        }

        // A copy of `source` for `block` (R14.8 "Duplicate"): every sub-field and media item copied, a fresh id, not validated
        public static ElementRow Duplicate(BlockInstanceData block, ElementRow source)
        {
            var row = new ElementRow(new BlockItemData());
            if (source.Data?.fields != null)
                foreach (var f in source.Data.fields)
                    if (f != null) row.Data.fields.Add(Copy(f));
            row.SetValue(IdKey, BlockSurfaces.TakeRowId(block));
            row.SetValidated(false);
            return row;
        }

        // The sub-field with this key, created when missing (never a media entry: AddMedia adds those)
        private BlockItemFieldValue Ensure(string key)
        {
            var field = Field(key);
            if (field != null) return field;
            field = new BlockItemFieldValue { key = key };
            Data.fields.Add(field);
            return field;
        }

        private static BlockItemFieldValue Copy(BlockItemFieldValue f) => new()
        {
            key = f.key, number = f.number, flag = f.flag, value = f.value, asset = f.asset,
            text = CopyEntries(f.text), asset_by_language = CopyEntries(f.asset_by_language),
        };

        private static List<LocalizedEntry> CopyEntries(List<LocalizedEntry> entries)
        {
            var copy = new List<LocalizedEntry>();
            if (entries != null)
                foreach (var e in entries)
                    if (e != null) copy.Add(new LocalizedEntry { lang = e.lang, value = e.value });
            return copy;
        }
    }

    // One media item of an element row (R14.1): a picture or a clip, its caption per language, its credit. Stored as one `media` sub-field
    // entry of the row -- asset = the file (a path under the wall's media folder, MediaPathRule), text = the caption per language,
    // value = the credit (one line for every language) -- so a row holds a list without nesting rows (JsonUtility allows no recursion).
    public readonly struct ElementMedia
    {
        public readonly BlockItemFieldValue Data;

        public ElementMedia(BlockItemFieldValue data) { Data = data; }

        public string Asset => Data?.asset ?? "";
        public string Credit => Data?.value ?? "";
        // The caption in `language`, else `fallback`, else any language that has words (BlockFieldReader.Pick)
        public string Caption(string language, string fallback) => BlockFieldReader.Pick(Data?.text, language, fallback);
        // Every language's caption (what search reads)
        public IReadOnlyList<LocalizedEntry> Captions => (IReadOnlyList<LocalizedEntry>)Data?.text ?? System.Array.Empty<LocalizedEntry>();
    }
}
