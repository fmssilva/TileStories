using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace TileStories
{
    // The card's long-text markup (_3.1 section 6, "inline glossary terms"): a text is split into paragraphs at blank
    // lines, and a glossary word is written [[term]] or [[shown words|term]] (the second form lets a Portuguese text
    // show "torre de menagem" for the glossary entry "keep"). Pure: the view turns the runs into UI Toolkit rich text
    // (ToRichText) and handles the taps; everything the author typed is shown literally (no tag of theirs is obeyed).
    public static class GlossaryMarkup
    {
        // One piece of a paragraph: plain text, or a glossary word (Term = the entry it points at)
        public readonly struct Run
        {
            public readonly string Text;
            public readonly string Term;
            public bool IsTerm => Term != null;

            public Run(string text, string term)
            {
                Text = text;
                Term = term;
            }
        }

        private static readonly Regex TermPattern = new(@"\[\[([^\[\]]+?)\]\]", RegexOptions.Compiled);
        private static readonly Regex BlankLine = new(@"\r?\n[ \t]*\r?\n", RegexOptions.Compiled);

        // The paragraphs of a long text (blank-line separated, trimmed, empty ones dropped)
        public static List<string> Paragraphs(string text)
        {
            var list = new List<string>();
            if (string.IsNullOrWhiteSpace(text)) return list;
            foreach (string p in BlankLine.Split(text))
                if (!string.IsNullOrWhiteSpace(p)) list.Add(p.Trim());
            return list;
        }

        // A paragraph as plain runs and glossary runs, in order
        public static List<Run> Parse(string paragraph)
        {
            var runs = new List<Run>();
            if (string.IsNullOrEmpty(paragraph)) return runs;
            int at = 0;
            foreach (Match m in TermPattern.Matches(paragraph))
            {
                if (m.Index > at) runs.Add(new Run(paragraph.Substring(at, m.Index - at), null));
                string inner = m.Groups[1].Value;
                int bar = inner.IndexOf('|');
                string shown = (bar >= 0 ? inner.Substring(0, bar) : inner).Trim();
                string term = (bar >= 0 ? inner.Substring(bar + 1) : inner).Trim();
                // - "[[|x]]" or "[[x|]]": nothing sensible to show or link, keep the author's text visible
                if (shown.Length == 0 || term.Length == 0) runs.Add(new Run(m.Value, null));
                else runs.Add(new Run(shown, term));
                at = m.Index + m.Length;
            }
            if (at < paragraph.Length) runs.Add(new Run(paragraph.Substring(at), null));
            return runs;
        }

        // The text a visitor reads (the markup removed)
        public static string PlainText(string paragraph)
        {
            var sb = new StringBuilder();
            foreach (var run in Parse(paragraph)) sb.Append(run.Text);
            return sb.ToString();
        }

        // Every glossary term a text points at, in order, each once
        public static List<string> Terms(string text)
        {
            var terms = new List<string>();
            foreach (string p in Paragraphs(text))
                foreach (var run in Parse(p))
                    if (run.IsTerm && !terms.Exists(t => CardGlossary.SameTerm(t, run.Term))) terms.Add(run.Term);
            return terms;
        }

        // UI Toolkit rich text of a paragraph: a known term becomes a <link> (underlined), an unknown one plain text.
        // Every author character is wrapped in <noparse> so a "<" they typed is shown, never read as a tag.
        public static string ToRichText(IReadOnlyList<Run> runs, System.Func<string, bool> isKnownTerm)
        {
            var sb = new StringBuilder();
            foreach (var run in runs)
            {
                string text = "<noparse>" + run.Text.Replace("</noparse>", "</ noparse>") + "</noparse>";
                if (run.IsTerm && isKnownTerm(run.Term))
                    sb.Append("<link=\"").Append(run.Term.Replace("\"", "'")).Append("\"><u>").Append(text).Append("</u></link>");
                else
                    sb.Append(text);
            }
            return sb.ToString();
        }
    }
}
