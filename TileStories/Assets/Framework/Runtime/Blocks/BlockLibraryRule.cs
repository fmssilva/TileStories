namespace TileStories
{
    // The wall-wide Block Library (card_settings.kinds, _3.1 section 5.1) as rules shared by the runtime and the
    // Editor table: a kind with no row is enabled with its own default variant, so a new kind needs no config edit.
    public static class BlockLibraryRule
    {
        // The Block Library row of a kind, or null
        public static BlockKindSetting Row(CardSettings settings, string kind)
        {
            var rows = settings?.kinds;
            if (rows == null || kind == null) return null;
            for (int i = 0; i < rows.Count; i++)
                if (rows[i] != null && rows[i].kind == kind) return rows[i];
            return null;
        }

        // Whether this wall shows blocks of this kind. The header is the card's identity and cannot be switched off.
        public static bool IsEnabled(CardSettings settings, string kind)
        {
            if (kind == BuiltInBlocks.HeaderKind) return true;
            var row = Row(settings, kind);
            return row == null || row.enabled;
        }

        // The option a block of this kind reads for a Choice field while it stores none of its own: the Block Library's default
        // for a LibraryDefault field (when it is one of the options), else the field's own ChoiceDefault ("" without one)
        public static string ChoiceDefault(CardSettings settings, BlockKindDefinition definition, BlockFieldDefinition field)
        {
            if (field == null) return "";
            if (field.LibraryDefault && definition != null)
            {
                var entries = Row(settings, definition.Key)?.field_defaults;
                if (entries != null)
                    foreach (var entry in entries)
                        if (entry != null && entry.key == field.Key && HasOption(field, entry.value)) return entry.value;
            }
            return field.ChoiceDefault ?? "";
        }

        // The ONE read of a Choice field with a default: the block's own value when it is one of the options, else ChoiceDefault
        public static string Choice(CardSettings settings, BlockKindDefinition definition, BlockFieldDefinition field, BlockFieldReader read)
        {
            string own = read?.Value(field.Key) ?? "";
            return HasOption(field, own) ? own : ChoiceDefault(settings, definition, field);
        }

        // How a Toggle's Library default is written in card_settings.kinds[].field_defaults
        public const string FlagTrue = "true";
        public const string FlagFalse = "false";

        // The wall's Block Library default of a LibraryDefault Toggle: ticked only when the kind's row says "true" (no row, no entry: off)
        public static bool FlagDefault(CardSettings settings, BlockKindDefinition definition, BlockFieldDefinition field)
        {
            if (field == null || !field.LibraryDefault || definition == null) return false;
            var entries = Row(settings, definition.Key)?.field_defaults;
            if (entries != null)
                foreach (var entry in entries)
                    if (entry != null && entry.key == field.Key) return entry.value == FlagTrue;
            return false;
        }

        // The ONE read of a Toggle with a Library default: the block's own value once it has stored one (a tick or an unticked box), else
        // the wall's default
        public static bool Flag(CardSettings settings, BlockKindDefinition definition, BlockFieldDefinition field, BlockFieldReader read) =>
            read != null && read.Stored(field.Key) ? read.Flag(field.Key) : FlagDefault(settings, definition, field);

        private static bool HasOption(BlockFieldDefinition field, string value)
        {
            if (string.IsNullOrEmpty(value) || field.Options == null) return false;
            for (int i = 0; i < field.Options.Count; i++)
                if (field.Options[i] == value) return true;
            return false;
        }

        // The variant an instance gets when it names none (or one the kind lacks): the row's, when the kind has it
        public static string DefaultVariant(CardSettings settings, BlockKindDefinition definition)
        {
            var row = Row(settings, definition.Key);
            return row != null && definition.HasVariant(row.default_variant) ? row.default_variant : definition.DefaultVariant;
        }
    }
}
