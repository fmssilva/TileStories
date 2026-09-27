namespace TileStories
{
    // The visit family: what a visitor needs to plan a visit to the real place.
    // Part of BuiltInBlocks (BuiltInBlocks.cs registers every kind, in catalog order): each kind's key, variant and
    // field constants first, then the definitions.
    public static partial class BuiltInBlocks
    {
        public const string PracticalInfoKind = "practical_info";
        public const string PracticalInfoRows = "rows";
        public const string PracticalInfoItemsField = "rows";
        public const string PracticalInfoIconField = "icon";
        public const string PracticalInfoLabelField = "label";
        public const string PracticalInfoValueField = "value";
        // The icons a row may pick: the visit-information keys of the card's one icon set (CardIcons)
        public static readonly string[] PracticalInfoIcons = { CardIcons.Time, CardIcons.Access, CardIcons.Location, CardIcons.Info, CardIcons.Ticket, CardIcons.Light };

        // Tier 1: what a visitor needs to plan the visit
        public static readonly BlockKindDefinition PracticalInfo = new()
        {
            Key = PracticalInfoKind,
            Family = "visit",
            DisplayName = "Practical Info",
            Help = "What a visitor needs to plan a visit to the real place: opening hours, tickets, how to get there, the best " +
                   "light. Rows: one row per item, a small icon beside its label and value, split by thin lines. A row with no " +
                   "label is not shown.",
            Variants = new[] { PracticalInfoRows },
            DefaultVariant = PracticalInfoRows,
            DisplayModes = new[] { CardOptions.DisplayInline },
            Fields = new[]
            {
                new BlockFieldDefinition
                {
                    Key = PracticalInfoItemsField, Type = BlockFieldType.Items, Label = "Rows", Required = true,
                    Help = "One row per item, in the order the card shows them.",
                    ItemFields = new[]
                    {
                        new BlockFieldDefinition
                        {
                            Key = PracticalInfoIconField, Type = BlockFieldType.Choice, Label = "Icon",
                            Options = PracticalInfoIcons, OptionLabels = new[] { "Time", "Access", "Location", "Info", "Ticket", "Light" },
                            Help = "The small picture beside the row: Time (a clock), Access (a doorway), Location (a pin), Info (an i), " +
                                   "Ticket, Light (a bright point). (none): no picture, the words stay in line with the others.",
                        },
                        new BlockFieldDefinition { Key = PracticalInfoLabelField, Type = BlockFieldType.LocalizedText, Label = "Label", Required = true, Help = "What the row is about (Open, Tickets, Getting there)." },
                        new BlockFieldDefinition { Key = PracticalInfoValueField, Type = BlockFieldType.LocalizedText, Label = "Value", Help = "The information itself (Every day, 9:00 to 21:00). May stay empty." },
                    },
                },
            },
        };
    }
}
