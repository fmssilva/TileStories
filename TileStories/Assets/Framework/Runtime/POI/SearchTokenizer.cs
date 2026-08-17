using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace TileStories
{
    // Pure-data-domain utility: tokenizes free-form search input and POI text
    // into a normalized, deduplicated token list. No MonoBehaviour dependency.
    // Used by POISearchIndex at build time and at search time.
    public static class SearchTokenizer
    {
        // Tokenize user input or POI text into normalized, deduplicated tokens.
        // NFKD decomposes composite characters (e.g. accented letters) into base
        // + combining marks; stripping the combining marks folds diacritics
        // (Sao + combining tilde -> sa), so accented and unaccented forms match.
        // Lowercases via invariant culture so 'A' and 'a' collapse. Splits on
        // whitespace and punctuation. Deduplicates by first-occurrence order.
        public static List<string> Tokenize(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return new List<string>();
            }

            string normalized = input.Normalize(NormalizationForm.FormKD);
            var sb = new StringBuilder();
            foreach (char c in normalized)
            {
                var uc = CharUnicodeInfo.GetUnicodeCategory(c);
                if (uc != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }

            string folded = sb.ToString().ToLowerInvariant();

            var tokens = new List<string>();
            var seen = new HashSet<string>();
            var current = new StringBuilder();

            foreach (char c in folded)
            {
                                if (char.IsWhiteSpace(c) || char.IsPunctuation(c) || char.IsSymbol(c))
                {
                    string token = current.ToString();
                    if (!string.IsNullOrEmpty(token) && seen.Add(token))
                    {
                        tokens.Add(token);
                    }
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }

            // Handle the last token after the final delimiter
            string lastToken = current.ToString();
            if (!string.IsNullOrEmpty(lastToken) && seen.Add(lastToken))
            {
                tokens.Add(lastToken);
            }

            return tokens;
        }

        // Validate a single token is usable (non-null, non-empty, has at least
        // one non-whitespace character). Exposed for external consumers.
        public static bool IsTokenValid(string token)
        {
            return token != null && token.Length > 0 && token.Any(c => !char.IsWhiteSpace(c));
        }
    }
}
