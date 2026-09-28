namespace TileStories
{
    // Where the card's local state lives between visits (_3.1 step 8A): string keys to string values, nothing more, so the
    // rules that decide what is stored (CardLocalState) are plain C# a test builds with `new`. The app keeps them in
    // PlayerPrefs (PlayerPrefsCardStateStore); a test or the Phase A gallery keeps them in memory (MemoryCardStateStore).
    public interface ICardStateStore
    {
        // The value stored under `key`; false (value "") when nothing is
        bool TryGet(string key, out string value);

        // Store `value` under `key`, replacing what was there
        void Set(string key, string value);

        // Forget `key` (nothing happens when it holds nothing)
        void Remove(string key);
    }
}
