namespace TileStories
{
    // The framework's own block kinds (_3.1 section 6), registered once into BlockRegistry.Shared (and into any
    // registry a test builds). Kinds are added tier by tier, one at a time (_3.1 section 7). Every help text is
    // framework-authored and app-agnostic: it names Editor Tab controls, never a wall's own content.
    public static class BuiltInBlocks
    {
        // The card's identity block: the stack always starts with one (BlockStackBuilder)
        public const string HeaderKind = "header";

        public const string HeaderCompact = "compact";
        public const string HeaderTextOnly = "text_only";

        // Register every built-in kind into `registry`
        public static void Register(BlockRegistry registry)
        {
            registry.Register(Header, () => new HeaderBlockView());
        }

        // Tier 1: the header. Image / video / 3D header variants arrive with Tiers 2, 4 and 5.
        public static readonly BlockKindDefinition Header = new()
        {
            Key = HeaderKind,
            Family = "about",
            DisplayName = "Header",
            Help = "The top of the card: the point's title and its category chip, plus an optional subtitle. " +
                   "It is always shown and always first; a point with no header block gets one made from its name and summary.",
            Variants = new[] { HeaderCompact, HeaderTextOnly },
            DefaultVariant = HeaderTextOnly,
            DisplayModes = new[] { CardOptions.DisplayInline },
            Fields = new[]
            {
                new BlockFieldDefinition
                {
                    Key = BlockStackBuilder.HeaderTitleField, Type = BlockFieldType.LocalizedText, Label = "Title", Required = true,
                    Help = "The big title at the top of the card, one per language. Empty in every language: the header falls back to the point's name.",
                },
                new BlockFieldDefinition
                {
                    Key = BlockStackBuilder.HeaderSubtitleField, Type = BlockFieldType.LocalizedText, Label = "Subtitle",
                    Help = "One short line under the title (a date, a place, a one-line teaser). The Compact variant never shows it.",
                },
            },
        };
    }
}
