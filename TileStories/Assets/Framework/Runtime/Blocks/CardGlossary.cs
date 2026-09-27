using System.Collections.Generic;

namespace TileStories
{
    // The wall's glossary (card_settings.glossary) in one card's language: a [[term]] of a card text is looked up here
    // for the definition the visitor taps open. Language -> fallback -> any, like every localized card field.
    public sealed class CardGlossary
    {
        private readonly IReadOnlyList<GlossaryEntry> _entries;
        private readonly string _language;
        private readonly string _fallback;

        public CardGlossary(IReadOnlyList<GlossaryEntry> entries, string language, string fallbackLanguage)
        {
            _entries = entries;
            _language = language;
            _fallback = fallbackLanguage;
        }

        // A term's definition, or null when the wall has no entry for it (the word then shows as plain text)
        public string Definition(string term)
        {
            var entry = Find(_entries, term);
            if (entry == null) return null;
            string text = BlockFieldReader.Pick(entry.definition, _language, _fallback);
            return text.Length > 0 ? text : null;
        }

        public bool Knows(string term) => Definition(term) != null;

        // The glossary entry for a term, or null
        public static GlossaryEntry Find(IReadOnlyList<GlossaryEntry> entries, string term)
        {
            if (entries == null || string.IsNullOrWhiteSpace(term)) return null;
            for (int i = 0; i < entries.Count; i++)
                if (entries[i] != null && SameTerm(entries[i].term, term)) return entries[i];
            return null;
        }

        // Terms match ignoring case and surrounding spaces
        public static bool SameTerm(string a, string b) =>
            a != null && b != null && string.Equals(a.Trim(), b.Trim(), System.StringComparison.OrdinalIgnoreCase);
    }
}
