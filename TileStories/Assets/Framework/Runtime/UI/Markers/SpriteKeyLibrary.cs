using System.Collections.Generic;
using UnityEngine;

namespace TileStories
{
    // A plain "key -> Sprite" lookup asset. Used as TWO separate .asset instances
    // in the project: IconLibrary.asset (key = "temple", "crown", ...) and
    // ShapeLibrary.asset (key = "circle", "hexagon", ...). Same data shape, one
    // class -- see _2_2_Marker_Design.md §4 principle 4.
    [CreateAssetMenu(fileName = "SpriteKeyLibrary", menuName = "TileStories/Sprite Key Library")]
    public class SpriteKeyLibrary : ScriptableObject
    {
        [System.Serializable]
        public struct Entry
        {
            public string key;
            public Sprite sprite;
        }

        [SerializeField] private List<Entry> entries = new();

        public IReadOnlyList<Entry> Entries => entries;

        // Rebuilt on every Get() call to ensure it stays in sync with the
        // serialized entries list. ScriptableObjects are deserialized after
        // construction, so a lazy-initialized dictionary built in the constructor
        // would see an empty entries list and stay empty forever.
        private Dictionary<string, Sprite> BuildLookup()
        {
            var dict = new Dictionary<string, Sprite>();
            if (entries == null) return dict;
            foreach (var e in entries)
            {
                if (!string.IsNullOrEmpty(e.key) && e.sprite != null)
                    dict[e.key] = e.sprite;
            }
            return dict;
        }

        public Sprite Get(string key)
        {
            if (string.IsNullOrEmpty(key)) return null;
            return BuildLookup().TryGetValue(key, out var sprite) ? sprite : null;
        }

        public string FindKeyForSprite(Sprite sprite)
        {
            if (sprite == null || entries == null)
                return null;

            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].sprite == sprite && !string.IsNullOrWhiteSpace(entries[i].key))
                    return entries[i].key;
            }

            return null;
        }

        public void Set(string key, Sprite sprite)
        {
            if (string.IsNullOrWhiteSpace(key) || sprite == null)
                return;

            if (entries == null)
                entries = new List<Entry>();

            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].key == key)
                {
                    entries[i] = new Entry { key = key, sprite = sprite };
                    return;
                }
            }

            entries.Add(new Entry { key = key, sprite = sprite });
        }

        public string EnsureKeyForSprite(Sprite sprite, string suggestedKey)
        {
            if (sprite == null)
                return null;

            string existing = FindKeyForSprite(sprite);
            if (!string.IsNullOrWhiteSpace(existing))
                return existing;

            string normalized = NormalizeKey(!string.IsNullOrWhiteSpace(suggestedKey) ? suggestedKey : sprite.name);
            if (string.IsNullOrWhiteSpace(normalized))
                normalized = "symbol";

            string candidate = normalized;
            int suffix = 1;
            while (Get(candidate) != null)
            {
                suffix++;
                candidate = normalized + "_" + suffix;
            }

            Set(candidate, sprite);
            return candidate;
        }

        public void CopyFrom(SpriteKeyLibrary source)
        {
            if (entries == null)
                entries = new List<Entry>();
            else
                entries.Clear();

            if (source == null || source.Entries == null)
                return;

            foreach (var entry in source.Entries)
            {
                if (string.IsNullOrWhiteSpace(entry.key) || entry.sprite == null)
                    continue;

                entries.Add(new Entry { key = entry.key, sprite = entry.sprite });
            }
        }

        private static string NormalizeKey(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return string.Empty;

            string lowered = raw.Trim().ToLowerInvariant();
            var chars = lowered.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                if (!(char.IsLetterOrDigit(chars[i]) || chars[i] == '_'))
                    chars[i] = '_';
            }

            return new string(chars);
        }
    }
}