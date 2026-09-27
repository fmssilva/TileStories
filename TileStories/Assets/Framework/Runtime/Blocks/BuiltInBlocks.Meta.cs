namespace TileStories
{
    // The meta family: about the card itself (where its content comes from); always last in navigation (_3.2).
    // Part of BuiltInBlocks (BuiltInBlocks.cs registers every kind, in catalog order): each kind's key, variant and
    // field constants first, then the definitions.
    public static partial class BuiltInBlocks
    {
        public const string SourcesKind = "sources";
        public const string SourcesList = "list";
        public const string SourcesWithConfidence = "with_confidence";
        public const string SourcesItemsField = "sources";
        public const string SourcesTitleField = "title";
        public const string SourcesAuthorField = "author";
        public const string SourcesLicenceField = "licence";
        public const string SourcesStatusField = "content_status";
        public const string ContentVerified = "verified";
        public const string ContentDraft = "draft";

        // Tier 1: where the card's content comes from (the meta family: always last, _3.2)
        public static readonly BlockKindDefinition Sources = new()
        {
            Key = SourcesKind,
            Family = "meta",
            DisplayName = "Sources",
            Help = "Where this card's content comes from: books, archives, photographs, with their author and licence. " +
                   "List: one row per source. With Confidence: the same, plus a chip that says whether the content " +
                   "was checked (Verified) or is still a draft (Content Status). With Heading empty the card titles the block with its " +
                   "Card Texts wording (Sources); the chip words are Card Texts too.",
            Variants = new[] { SourcesList, SourcesWithConfidence },
            DefaultVariant = SourcesList,
            DisplayModes = new[] { CardOptions.DisplayInline },
            DefaultHeadingKey = CardStrings.Keys.SourcesHeading,
            Fields = new[]
            {
                new BlockFieldDefinition
                {
                    Key = SourcesItemsField, Type = BlockFieldType.Items, Label = "Sources", Required = true,
                    Help = "One row per source.",
                    ItemFields = new[]
                    {
                        new BlockFieldDefinition { Key = SourcesTitleField, Type = BlockFieldType.LocalizedText, Label = "Title", Help = "The book, archive, photograph or page." },
                        new BlockFieldDefinition { Key = SourcesAuthorField, Type = BlockFieldType.LocalizedText, Label = "Author", Help = "Who made it (a person, an institution)." },
                        new BlockFieldDefinition { Key = SourcesLicenceField, Type = BlockFieldType.LocalizedText, Label = "Licence", Help = "Its licence or rights line (CC BY 4.0, Public domain, With permission)." },
                    },
                },
                new BlockFieldDefinition
                {
                    Key = SourcesStatusField, Type = BlockFieldType.Choice, Label = "Content Status",
                    Options = new[] { ContentVerified, ContentDraft }, OptionLabels = new[] { "Verified", "Draft" },
                    Help = "Whether this card's content was checked against its sources (Verified) or is still a draft (Draft). " +
                           "Only the With Confidence look shows it; (none) shows no chip.",
                },
            },
        };
    }
}
