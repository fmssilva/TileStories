namespace TileStories
{
    // What a feedback block's vote can be and how it is reported (_3.1 Tier 3), pure so every case is a unit test.
    //   thumbs -- 0 = down, 1 = up
    //   stars  -- 1..StarCount
    // The vote is stored as that number (CardLocalState.Vote) and reported as words (CardEvent.Value): "down" / "up", or the
    // star count. A stored vote the variant cannot have (the look was switched from stars to thumbs after a 4) counts as no vote.
    public static class FeedbackRule
    {
        public const int ThumbDown = 0;
        public const int ThumbUp = 1;

        // How many stars the rating has
        public const int StarCount = 5;

        // Whether `vote` is a vote this variant can have
        public static bool IsValid(string variant, int vote) =>
            variant == BuiltInBlocks.FeedbackStars ? vote >= 1 && vote <= StarCount : vote == ThumbDown || vote == ThumbUp;

        // The vote as the event reports it: "down" / "up" for thumbs, "1".."5" for stars
        public static string EventValue(string variant, int vote)
        {
            if (variant == BuiltInBlocks.FeedbackStars) return vote.ToString(System.Globalization.CultureInfo.InvariantCulture);
            return vote == ThumbUp ? "up" : "down";
        }

        // The stored vote of this variant, or -1: nothing stored, or a value the variant cannot have
        public static int Stored(string variant, int stored) => IsValid(variant, stored) ? stored : -1;
    }
}
