namespace TileStories
{
    // The community family: what the visitor tells back (_3.1 Tier 3). Part of BuiltInBlocks (BuiltInBlocks.cs registers every
    // kind, in catalog order): each kind's key, variant and field constants first, then the definitions. A vote is remembered by
    // CardLocalState and reported once through ICardEvents; there is no backend yet.
    public static partial class BuiltInBlocks
    {
        public const string FeedbackKind = "feedback";
        public const string FeedbackThumbs = "thumbs";
        public const string FeedbackStars = "stars";
        public const string FeedbackQuestionField = "question";

        // Tier 3: one question asked of the visitor, answered with a thumb or a star rating
        public static readonly BlockKindDefinition Feedback = new()
        {
            Key = FeedbackKind,
            Family = "community",
            DisplayName = "Feedback",
            Help = "Asks the visitor one question about this card and takes one answer. Thumbs: a thumb up or down. Stars: one to five stars. " +
                   "The vote is kept on the visitor's device (the block then shows it as given, with a thank-you) and reported once as a " +
                   "feedback event; nothing is sent anywhere yet. With Question empty the card asks its own question for the look " +
                   "(Detail Card > Card Texts); the thank-you and the button words are Card Texts too.",
            Variants = new[] { FeedbackThumbs, FeedbackStars },
            DefaultVariant = FeedbackThumbs,
            DisplayModes = new[] { CardOptions.DisplayInline },
            Fields = new[]
            {
                new BlockFieldDefinition
                {
                    Key = FeedbackQuestionField, Type = BlockFieldType.LocalizedText, Label = "Question",
                    Help = "What the visitor is asked (Was this useful?). Empty: the card's own question for the look.",
                },
            },
        };
    }
}
