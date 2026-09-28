namespace TileStories
{
    // What the card tells the rest of the app about the visitor (_3.1 step 8A): the ONE seam where the telemetry work of the
    // plan's Stage 3 (Type A / B events) will plug in. There is no backend here: a block raises an event through this
    // interface and whoever implements it decides what to do -- the default (LogCardEvents) writes one log line.
    public interface ICardEvents
    {
        // A visitor did something a block reports (CardEvent)
        void Raise(CardEvent cardEvent);
    }

    // What happened: its kind (CardEventKinds), where (wall, POI, block, variant) and its value
    public readonly struct CardEvent
    {
        public readonly string Kind;
        public readonly string WallId;
        public readonly string PoiId;
        public readonly string BlockKey;
        public readonly string Variant;
        // What the visitor chose, as words: "up" / "down" for thumbs, "1".."5" for stars, the option number of a poll
        public readonly string Value;

        public CardEvent(string kind, string wallId, string poiId, string blockKey, string variant, string value)
        {
            Kind = kind;
            WallId = wallId;
            PoiId = poiId;
            BlockKey = blockKey;
            Variant = variant;
            Value = value;
        }
    }

    // Every kind of event a card raises
    public static class CardEventKinds
    {
        // A visitor gave feedback (thumbs / stars) on a block: raised once per block
        public const string Feedback = "feedback";

        // A visitor voted in a poll block: raised once per block (Value: the option's authored row number, from 1)
        public const string Poll = "poll";

        // A visitor added this point's item to their story (a collect block): raised once per block (Value: "collected")
        public const string Collect = "collect";
    }
}
