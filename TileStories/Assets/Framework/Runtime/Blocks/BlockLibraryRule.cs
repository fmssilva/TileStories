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

        // The variant an instance gets when it names none (or one the kind lacks): the row's, when the kind has it
        public static string DefaultVariant(CardSettings settings, BlockKindDefinition definition)
        {
            var row = Row(settings, definition.Key);
            return row != null && definition.HasVariant(row.default_variant) ? row.default_variant : definition.DefaultVariant;
        }
    }
}
