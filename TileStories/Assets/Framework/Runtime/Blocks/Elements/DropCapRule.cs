using System;
using System.Collections.Generic;

namespace TileStories
{
    // Where a drop-cap paragraph breaks (_3.3 R14.2, the prototype's magazine page), pure: UI Toolkit cannot flow text around a floated
    // letter, so the paragraph is cut in two -- the words that fill `lines` lines of `width` beside the raised letter, and the rest, which
    // runs the full width under it. Words are wrapped greedily by `measure` (the width of a run in the paragraph's own face and size), a
    // glossary mark ([[shown words|term]]) stays one unbreakable word measured as its shown words, and a single word wider than the line
    // takes a line of its own, as the text engine breaks it.
    public static class DropCapRule
    {
        public readonly struct Cut
        {
            // The words beside the letter, and the words under it ("" when the whole paragraph fits beside it)
            public readonly string Beside;
            public readonly string Under;

            public Cut(string beside, string under)
            {
                Beside = beside;
                Under = under;
            }
        }

        // The cut of `text` (markup kept) for `lines` lines of `width`; the whole text beside the letter for a width or measure that gives no
        // room to judge (the first layout pass, before the paragraph has a width)
        public static Cut Split(string text, float width, int lines, Func<string, float> measure)
        {
            if (string.IsNullOrWhiteSpace(text) || !(width > 0f) || lines <= 0 || measure == null) return new Cut(text ?? "", "");
            var words = Words(text);
            int line = 1;
            string run = "";
            for (int i = 0; i < words.Count; i++)
            {
                string candidate = run.Length == 0 ? words[i] : run + " " + words[i];
                if (run.Length == 0 || measure(GlossaryMarkup.PlainText(candidate)) <= width)
                {
                    run = candidate;
                    continue;
                }
                // - this word opens a new line: past the last line beside the letter, everything from it goes under
                if (line == lines) return new Cut(Join(words, 0, i), Join(words, i, words.Count));
                line++;
                run = words[i];
            }
            return new Cut(text.Trim(), "");
        }

        // The words of a paragraph, split on spaces outside a glossary mark
        public static List<string> Words(string text)
        {
            var words = new List<string>();
            int start = -1;
            bool inMark = false;
            for (int i = 0; i < text.Length; i++)
            {
                if (!inMark && i + 1 < text.Length && text[i] == '[' && text[i + 1] == '[') inMark = true;
                else if (inMark && i + 1 < text.Length && text[i] == ']' && text[i + 1] == ']') inMark = false;
                bool space = !inMark && char.IsWhiteSpace(text[i]);
                if (space && start >= 0)
                {
                    words.Add(text.Substring(start, i - start));
                    start = -1;
                }
                else if (!space && start < 0) start = i;
            }
            if (start >= 0) words.Add(text.Substring(start));
            return words;
        }

        private static string Join(List<string> words, int from, int to) => string.Join(" ", words.GetRange(from, to - from));
    }
}
