using System.Globalization;

namespace TileStories
{
    // What the generic COMPACT ROW says about a block (_3.2 section 6.2, step 12), pure: one look for every kind, read only from the stack
    // entry -- never from a loaded file, so a card of collapsed rows holds no media (the point of `expandable`).
    //   - words: the heading the stack draws over the block (its own, else the kind's default heading), else the block's first words
    //     (BlockSummaryRule, the line the Editor's Card Content row shows), else the kind's words
    //   - the family icon and colour token (a family read twice: the icon and, through the words, what the block is)
    //   - meta: the format's words, and its minutes when the block gives them (its Reading Time field). A clip kind (audio, video) shows
    //     its format only: its length is the clip's, and reading it would load the clip the row exists to leave alone
    public static class CompactRowRule
    {
        public readonly struct Row
        {
            public readonly string Words;
            // The words are the block's first words, not a heading: a teaser of a longer text, which the row shows on one line and lets
            // the stylesheet cut with an ellipsis (a heading is shown whole)
            public readonly bool FromFirstWords;
            public readonly string IconKey;
            public readonly string ColorToken;
            public readonly string FormatWords;
            public readonly int Minutes;
            // The second line: the format's words, with " - 3 min" in the card's own wording when there are minutes
            public readonly string Meta;

            public Row(string words, bool fromFirstWords, string iconKey, string colorToken, string formatWords, int minutes, string meta)
            {
                Words = words;
                FromFirstWords = fromFirstWords;
                IconKey = iconKey;
                ColorToken = colorToken;
                FormatWords = formatWords;
                Minutes = minutes;
                Meta = meta;
            }
        }

        // The row for `entry` in `language` (then `fallback`); `words` reads a CardStrings key in the card's language (null: no words)
        public static Row Of(BlockStackBuilder.Entry entry, BlockRegistry registry, string language, string fallback, System.Func<string, string> words)
        {
            var kind = entry.Definition;
            if (kind == null) return new Row("", false, null, null, "", 0, "");
            string text = new BlockFieldReader(entry.Instance, language, fallback).Text(BlockKindDefinition.HeadingField);
            if (text.Length == 0 && kind.DefaultHeadingKey != null) text = Words(words, kind.DefaultHeadingKey);
            bool firstWords = false;
            if (text.Length == 0)
            {
                text = BlockSummaryRule.FirstWords(entry.Instance, kind, language, fallback);
                firstWords = text.Length > 0;
            }
            if (text.Length == 0) text = Words(words, kind.LabelKey);

            FamilyDefinition family = null;
            registry?.TryGetFamily(kind.Family, out family);
            string format = Words(words, CardNavModel.FormatLabelKey(kind.Format));
            // - a clip kind's minutes are the clip's: 0 here (ReadingTimeRule with no clip), never a load
            int minutes = ReadingTimeRule.Minutes(kind, entry.Instance, 0f);
            string meta = format;
            if (minutes > 0)
            {
                string time = string.Format(CultureInfo.InvariantCulture, Words(words, CardStrings.Keys.NavContentsMinutes), minutes);
                meta = format.Length > 0 ? string.Format(CultureInfo.InvariantCulture, Words(words, CardStrings.Keys.CompactMeta), format, time) : time;
            }
            return new Row(text, firstWords, family?.IconKey, family?.ColorToken, format, minutes, meta);
        }

        private static string Words(System.Func<string, string> words, string key) =>
            key != null && words != null ? words(key) ?? "" : "";
    }
}
