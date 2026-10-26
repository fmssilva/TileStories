using System.Collections.Generic;

namespace TileStories
{
    // What a block says about itself in one line (_3.1 step 15.1, moved from the Editor at _3.2 step 12), pure: its Heading, else the first
    // words it holds -- a text field in the kind's order, an Items field's first row included -- read in `language` (then `fallback`, then any
    // language that has words: BlockFieldReader.Pick). Glossary marks read as their shown words, line breaks and runs of spaces as one space.
    // "" for a block with no words at all. The Editor's Card Content rows and BlockRef popups read it in the wall's first language; the
    // card's compact row (CompactRowRule) in the language the card speaks. A composed kind's Page reads its rows (ElementRules.FirstWords,
    // _3.3 R14.3); its Preview surface fields are never its summary (an Open Row's words describe the button, not the block).
    public static class BlockSummaryRule
    {
        public static string FirstWords(BlockInstanceData block, BlockKindDefinition definition, string language, string fallback)
        {
            if (block == null || definition == null) return "";
            var read = new BlockFieldReader(block, language, fallback);
            foreach (var field in definition.Fields)
            {
                if (Contains(BlockSurfaces.FieldKeys, field.Key)) continue;
                if (field.Type == BlockFieldType.Elements)
                {
                    string words = ElementRules.FirstWords(new ElementScope(block, definition, null, null, null), ElementRegistry.Shared, language, fallback);
                    if (words.Length > 0) return words;
                }
                else if (IsTextField(field))
                {
                    string text = OneLine(read.Text(field.Key));
                    if (text.Length > 0) return text;
                }
                else if (field.Type == BlockFieldType.Items && field.ItemFields != null)
                {
                    var rows = read.Items(field.Key);
                    if (rows.Count == 0) continue;
                    foreach (var sub in field.ItemFields)
                    {
                        if (!IsTextField(sub)) continue;
                        string text = OneLine(read.ItemText(rows[0], sub.Key));
                        if (text.Length > 0) return text;
                    }
                }
            }
            return "";
        }

        private static bool Contains(IReadOnlyList<string> list, string value)
        {
            foreach (string s in list)
                if (s == value) return true;
            return false;
        }

        internal static bool IsTextField(BlockFieldDefinition field) =>
            field.Type == BlockFieldType.LocalizedText || field.Type == BlockFieldType.LocalizedLongText;

        // A text as one line: glossary marks as their words, line breaks and runs of spaces as one space
        internal static string OneLine(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "";
            var words = GlossaryMarkup.PlainText(text).Split((char[])null, System.StringSplitOptions.RemoveEmptyEntries);
            return string.Join(" ", words);
        }
    }
}
