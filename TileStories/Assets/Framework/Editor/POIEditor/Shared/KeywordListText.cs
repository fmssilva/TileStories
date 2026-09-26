using System.Collections.Generic;

namespace TileStories.Editor
{
    // The one text form of a keyword list in the POI Editor ("castle, tower, sintra"): every Search Keywords
    // cell, keyword field and synonym row parses and shows keywords through here. Pure.
    public static class KeywordListText
    {
        // "a, b ,, c" -> [a, b, c]: split on commas, trim, drop empties and repeats (first one kept)
        public static List<string> Parse(string text)
        {
            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(text))
                return result;
            foreach (string part in text.Split(','))
            {
                string word = part.Trim();
                if (word.Length > 0 && !result.Contains(word))
                    result.Add(word);
            }
            return result;
        }

        // [a, b] -> "a, b"
        public static string Join(List<string> keywords) =>
            keywords == null ? "" : string.Join(", ", keywords);
    }
}
