using System.Collections.Generic;

namespace TileStories
{
    // Which language a card speaks (_3.1 15.3). Pure, so a test calls it with any inputs. The wall's Languages list says what exists: its
    // FIRST entry is the fallback every text falls back to, the rest are choices a visitor may switch to. A text is then picked by
    // BlockFieldReader.Pick (the ONE text rule): the shown language, else the wall's first, else the first language the field has.
    public static class CardLanguageRule
    {
        // The languages a visitor can choose between: the wall's list, blanks and repeats dropped, in the wall's order
        public static List<string> Choices(IReadOnlyList<string> languages)
        {
            var choices = new List<string>();
            if (languages == null) return choices;
            foreach (var language in languages)
            {
                string code = language?.Trim();
                if (!string.IsNullOrEmpty(code) && !choices.Contains(code)) choices.Add(code);
            }
            return choices;
        }

        // The wall's first language: where a text falls back to when the shown language has none ("" when the wall lists none)
        public static string Fallback(IReadOnlyList<string> languages)
        {
            var choices = Choices(languages);
            return choices.Count > 0 ? choices[0] : "";
        }

        // The language the card is shown in: the visitor's own choice if the wall offers it, else the developer's preview (only where the
        // build allows developer features and the wall offers it), else the wall's first language
        public static string Shown(IReadOnlyList<string> languages, string visitorChoice, string previewLanguage, bool previewAllowed)
        {
            var choices = Choices(languages);
            if (choices.Count == 0) return "";
            if (!string.IsNullOrEmpty(visitorChoice) && choices.Contains(visitorChoice)) return visitorChoice;
            if (previewAllowed && !string.IsNullOrEmpty(previewLanguage) && choices.Contains(previewLanguage)) return previewLanguage;
            return choices[0];
        }

        // The language a tap on the card's language chip switches to: the next choice after the shown one, wrapping round
        // ("" when the visitor has nothing to choose between)
        public static string Next(IReadOnlyList<string> languages, string shown)
        {
            var choices = Choices(languages);
            if (choices.Count < 2) return "";
            int at = choices.IndexOf(shown);
            return choices[(at + 1) % choices.Count];
        }
    }
}
