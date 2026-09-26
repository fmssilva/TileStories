using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TileStories
{
    // A plain "key -> TMP_FontAsset" lookup asset. Same shape as SpriteKeyLibrary
    // (_2.2.1_Marker_Design.md), kept as its own class rather than a shared generic:
    // a font asset is a genuinely different resource type, and Unity's own generic
    // ScriptableObject serialization would cost more machinery than this small
    // duplication does -- see _2.0_Labels_And_Fonts_Design.md.
    [CreateAssetMenu(fileName = "FontKeyLibrary", menuName = "TileStories/Font Key Library")]
    public class FontKeyLibrary : ScriptableObject
    {
        [System.Serializable]
        public struct Entry
        {
            public string key;
            public TMP_FontAsset font;
        }

        [SerializeField] private List<Entry> entries = new();

        public IReadOnlyList<Entry> Entries => entries;

        // Rebuilt on every Get() call, same reasoning as SpriteKeyLibrary: a
        // ScriptableObject is deserialized after construction, so a lazily-built
        // dictionary from the constructor would see an empty list and stay empty.
        private Dictionary<string, TMP_FontAsset> BuildLookup()
        {
            var dict = new Dictionary<string, TMP_FontAsset>();
            if (entries == null) return dict;
            foreach (var e in entries)
            {
                if (!string.IsNullOrEmpty(e.key) && e.font != null)
                    dict[e.key] = e.font;
            }
            return dict;
        }

        public TMP_FontAsset Get(string key)
        {
            if (string.IsNullOrEmpty(key)) return null;
            return BuildLookup().TryGetValue(key, out var font) ? font : null;
        }

        public IReadOnlyList<string> Keys()
        {
            var keys = new List<string>();
            if (entries == null) return keys;
            foreach (var e in entries)
                if (!string.IsNullOrEmpty(e.key) && e.font != null)
                    keys.Add(e.key);
            return keys;
        }

        // Register a font and return its key: the existing key if this font is already listed,
        // else a new one from its asset name ("Oswald Bold SDF" -> "oswald_bold_sdf", suffixed
        // _2, _3... on a clash). Same recipe as SpriteKeyLibrary.EnsureKeyForSprite, so adding the
        // same font twice never creates a duplicate entry.
        public string EnsureKeyForFont(TMP_FontAsset font)
        {
            if (font == null) return null;

            entries ??= new List<Entry>();
            foreach (var e in entries)
                if (e.font == font && !string.IsNullOrEmpty(e.key))
                    return e.key;

            string baseKey = NormalizeKey(font.name);
            if (string.IsNullOrEmpty(baseKey)) baseKey = "font";

            string candidate = baseKey;
            for (int suffix = 2; entries.Exists(e => e.key == candidate); suffix++)
                candidate = baseKey + "_" + suffix;

            entries.Add(new Entry { key = candidate, font = font });
            return candidate;
        }

        private static string NormalizeKey(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
            var chars = raw.Trim().ToLowerInvariant().ToCharArray();
            for (int i = 0; i < chars.Length; i++)
                if (!(char.IsLetterOrDigit(chars[i]) || chars[i] == '_'))
                    chars[i] = '_';
            return new string(chars);
        }

        // Seeds a newly-created wall library with another library's entries (the framework
        // default's 3 fonts), same role as SpriteKeyLibrary.CopyFrom: a wall's own font library
        // REPLACES the framework one entirely at runtime, so it must start with everything the
        // wall wants to keep (_2.0_Labels_And_Fonts_Design.md section 2.3).
        public void CopyFrom(FontKeyLibrary source)
        {
            entries ??= new List<Entry>();
            entries.Clear();

            if (source == null || source.Entries == null)
                return;

            foreach (var entry in source.Entries)
                entries.Add(entry);
        }
    }
}
