using System.Globalization;

namespace TileStories
{
    // The ONE store of what a visitor did on the cards, on this device (_3.1 step 8A): the answer chosen for a knowledge
    // question, the vote given to a feedback block, that a gated block was revealed ("seen"). Plain C# over an
    // ICardStateStore, so every rule is a test with `new`; the app's store is PlayerPrefsCardStateStore.
    //
    // A key names its place: wall + POI + block key + what it is, e.g. ts.card.living_room.lamp.block_44.answer-0 -- so two
    // walls, two POIs or two blocks never share an answer, and a block key that is never reused (TaxonomyRowKeys) never
    // inherits a deleted block's. View-only state does NOT belong here: the fun-fact flip, the glossary that is open, which
    // question or chapter is showing -- they are the view's and reset on every bind (_3.1 Tier 3 note).
    //
    // PlayerPrefs cannot list its keys, so every key this class writes is also recorded in one index entry of the same
    // store; ResetAll removes exactly those.
    public sealed class CardLocalState
    {
        public const string KeyPrefix = "ts.card.";
        private const string IndexSlot = "index";
        private const char IndexSeparator = '\n';

        private readonly ICardStateStore _store;

        // The wall this state belongs to (config wall_id): a second wall on the same device keeps its own
        public string WallId { get; }

        public CardLocalState(ICardStateStore store, string wallId)
        {
            _store = store ?? throw new System.ArgumentNullException(nameof(store));
            WallId = wallId ?? "";
        }

        // The store key of one piece of state. Each part is escaped, so a "." or a line break inside an id can never make two
        // different places one key.
        public string KeyOf(string poiId, string blockKey, string slot) =>
            KeyPrefix + Escape(WallId) + "." + Escape(poiId) + "." + Escape(blockKey) + "." + slot;

        private string IndexKey => KeyPrefix + Escape(WallId) + "." + IndexSlot;

        // The choice the visitor made for question `question` (the authored row, 0-based) of a block; -1 = not answered
        public int Answer(string poiId, string blockKey, int question) => ReadInt(KeyOf(poiId, blockKey, "answer-" + question));

        // Remember the choice made (0 and up; anything else is not an answer and is ignored)
        public void SetAnswer(string poiId, string blockKey, int question, int choice) => WriteInt(KeyOf(poiId, blockKey, "answer-" + question), choice);

        // What the visitor voted on a feedback block (thumbs: 0 down / 1 up; stars: 1..5); -1 = no vote yet
        public int Vote(string poiId, string blockKey) => ReadInt(KeyOf(poiId, blockKey, "vote"));

        // Remember the vote (0 and up; anything else is ignored)
        public void SetVote(string poiId, string blockKey, int value) => WriteInt(KeyOf(poiId, blockKey, "vote"), value);

        // The option the visitor voted for in a poll block, as the AUTHORED ROW of that option (0-based); -1 = no vote yet
        public int PollVote(string poiId, string blockKey) => ReadInt(KeyOf(poiId, blockKey, "poll"));

        // Remember the poll vote (0 and up; anything else is ignored)
        public void SetPollVote(string poiId, string blockKey, int row) => WriteInt(KeyOf(poiId, blockKey, "poll"), row);

        // Whether the visitor added the item of a collect block to their story
        public bool Collected(string poiId, string blockKey) => ReadInt(KeyOf(poiId, blockKey, "collected")) == 1;

        // Remember that the item was collected
        public void SetCollected(string poiId, string blockKey) => WriteInt(KeyOf(poiId, blockKey, "collected"), 1);

        // Whether the block was already revealed to this visitor (a block that waits until the card was read: show_after_viewed)
        public bool Seen(string poiId, string blockKey) => ReadInt(KeyOf(poiId, blockKey, "seen")) == 1;

        // Remember that the block was revealed, so it shows at once the next time
        public void MarkSeen(string poiId, string blockKey) => WriteInt(KeyOf(poiId, blockKey, "seen"), 1);

        // The language the visitor picked on this wall's cards ("" = never picked: the wall's first language shows). One choice for the
        // whole wall, not per point, so it is keyed by the wall alone
        public string Language() => _store.TryGet(LanguageKey, out string code) ? code : "";

        // Remember the picked language ("" or blank forgets it)
        public void SetLanguage(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) { _store.Remove(LanguageKey); return; }
            _store.Set(LanguageKey, code.Trim());
            AddToIndex(LanguageKey);
        }

        private string LanguageKey => KeyPrefix + Escape(WallId) + ".language";

        // Forget everything this wall's cards stored (the POI Editor's reset); how many entries went
        public int ResetAll()
        {
            if (!_store.TryGet(IndexKey, out string index)) return 0;
            int removed = 0;
            foreach (string key in index.Split(IndexSeparator))
            {
                if (key.Length == 0 || !_store.TryGet(key, out _)) continue;
                _store.Remove(key);
                removed++;
            }
            _store.Remove(IndexKey);
            return removed;
        }

        // A stored whole number, or -1 for nothing, text that is not a number, or a negative one
        private int ReadInt(string key) =>
            _store.TryGet(key, out string text) && int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value) && value >= 0
                ? value : -1;

        private void WriteInt(string key, int value)
        {
            if (value < 0) return;
            _store.Set(key, value.ToString(CultureInfo.InvariantCulture));
            AddToIndex(key);
        }

        // Record a key we wrote, once (ResetAll walks this list)
        private void AddToIndex(string key)
        {
            _store.TryGet(IndexKey, out string index);
            foreach (string line in index.Split(IndexSeparator))
                if (line == key) return;
            _store.Set(IndexKey, index.Length == 0 ? key : index + IndexSeparator + key);
        }

        // "%" first, so the escapes of the others are never escaped twice
        private static string Escape(string part) =>
            (part ?? "").Replace("%", "%25").Replace(".", "%2E").Replace("\n", "%0A");
    }
}
