using System.Collections.Generic;

namespace TileStories
{
    // A card state store that forgets everything when the app does: the Phase A gallery's, and the store a test hands the
    // card so a run never touches the developer's own saved answers (PlayerPrefsCardStateStore is the app's).
    public sealed class MemoryCardStateStore : ICardStateStore
    {
        private readonly Dictionary<string, string> _values = new();

        public int Count => _values.Count;

        // Every key stored now
        public IReadOnlyCollection<string> Keys => _values.Keys;

        public bool TryGet(string key, out string value)
        {
            if (key != null && _values.TryGetValue(key, out value)) return true;
            value = "";
            return false;
        }

        public void Set(string key, string value)
        {
            if (!string.IsNullOrEmpty(key)) _values[key] = value ?? "";
        }

        public void Remove(string key)
        {
            if (key != null) _values.Remove(key);
        }
    }
}
