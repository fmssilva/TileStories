using System.Collections.Generic;

namespace TileStories
{
    // The card's own UI texts (_3.1 step 5b, 30-ui-content.md: no visitor string in code). The framework ships a
    // default per key and language (CardStringTable, CardStrings.asset); a wall rewords any of them in
    // card_settings.strings. A text is looked up in this order, so a wall's English wording never hides the
    // framework's Portuguese one from a Portuguese visitor:
    //   wall [language] -> framework [language] -> wall [fallback] -> framework [fallback] -> the key itself
    // (the key only shows for a key missing from the framework table -- CardStringsTests guard that).
    public sealed class CardStrings
    {
        // Every key a card view reads. A key is added with the view that first shows it.
        public static class Keys
        {
            // The close button's name (its tooltip / accessible name; the X itself is drawn by PoiCard.uss)
            public const string Close = "close";
            // The small heading of a fun_fact card
            public const string FunFactLabel = "fun_fact_label";
            // The flip look's invitation to tap
            public const string FunFactHint = "fun_fact_hint";

            // The status block's small heading
            public const string StatusHeading = "status_heading";
            // The mark inside the ring of a condition nobody assessed
            public const string StatusUnknownMark = "status_unknown_mark";
            // The words for an unknown condition when the wall's Outline Types have no row to name it
            public const string StatusUnknown = "status_unknown";
            // "{0}% damaged": a known condition the wall's Outline Types do not name ({0} = the percentage)
            public const string StatusPercent = "status_percent";

            // The sources block's heading, and its confidence chip for verified / draft content
            public const string SourcesHeading = "sources_heading";
            public const string SourcesVerified = "sources_verified";
            public const string SourcesDraft = "sources_draft";

            public static readonly IReadOnlyList<string> All = new[]
            {
                Close, FunFactLabel, FunFactHint, StatusHeading, StatusUnknownMark, StatusUnknown, StatusPercent,
                SourcesHeading, SourcesVerified, SourcesDraft,
            };
        }

        private readonly IReadOnlyList<CardStringEntry> _framework;
        private readonly IReadOnlyList<CardStringEntry> _wall;
        private readonly string _language;
        private readonly string _fallback;

        public CardStrings(IReadOnlyList<CardStringEntry> framework, IReadOnlyList<CardStringEntry> wall, string language, string fallbackLanguage)
        {
            _framework = framework;
            _wall = wall;
            _language = language;
            _fallback = fallbackLanguage;
        }

        public string Get(string key) =>
            Find(_wall, key, _language) ?? Find(_framework, key, _language)
            ?? Find(_wall, key, _fallback) ?? Find(_framework, key, _fallback)
            ?? key;

        // The non-blank text of `key` in `language` in one table, or null
        public static string Find(IReadOnlyList<CardStringEntry> table, string key, string language)
        {
            if (table == null || key == null || language == null) return null;
            for (int i = 0; i < table.Count; i++)
            {
                var entry = table[i];
                if (entry == null || entry.key != key || entry.text == null) continue;
                foreach (var t in entry.text)
                    if (t != null && t.lang == language && !string.IsNullOrWhiteSpace(t.value)) return t.value;
            }
            return null;
        }
    }
}
