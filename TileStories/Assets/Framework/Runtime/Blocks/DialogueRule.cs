using System.Collections.Generic;

namespace TileStories
{
    // What a dialogue block says, line by line (_3.1 Tier 3, step 8B), pure so every case is a unit test. A row is one line
    // (a speaker and words); it may offer the visitor up to MaxChoices replies -- one choice level, no nesting, so a row holds them
    // as flat slots choice_1..3 (the words of the choice) and reply_1..3 (what the speaker answers to it). A row with no words
    // is left out; a choice with no label is never offered; a choice with no reply just moves on.
    public static class DialogueRule
    {
        // Replies one line can offer
        public const int MaxChoices = 3;

        public sealed class Choice
        {
            public string Label = "";
            // What the line's speaker says back (may be empty: the conversation just goes on)
            public string Reply = "";
        }

        public sealed class Line
        {
            // The authored row's index (0-based)
            public int Row;
            public string Speaker = "";
            public string Text = "";
            public List<Choice> Choices = new();
        }

        // The field key of choice `slot` (1..MaxChoices) and of its reply
        public static string ChoiceField(int slot) => BuiltInBlocks.DialogueChoicePrefix + slot;
        public static string ReplyField(int slot) => BuiltInBlocks.DialogueReplyPrefix + slot;

        // The lines this block says, in the order written
        public static List<Line> Lines(BlockFieldReader read)
        {
            var lines = new List<Line>();
            var rows = read.Items(BuiltInBlocks.DialogueLinesField);
            for (int row = 0; row < rows.Count; row++)
            {
                string text = read.ItemText(rows[row], BuiltInBlocks.DialogueTextField);
                if (text.Length == 0) continue;
                var line = new Line { Row = row, Speaker = read.ItemText(rows[row], BuiltInBlocks.DialogueSpeakerField), Text = text };
                for (int slot = 1; slot <= MaxChoices; slot++)
                {
                    string label = read.ItemText(rows[row], ChoiceField(slot));
                    if (label.Length == 0) continue;
                    line.Choices.Add(new Choice { Label = label, Reply = read.ItemText(rows[row], ReplyField(slot)) });
                }
                lines.Add(line);
            }
            return lines;
        }

        // What the block leaves out that the author probably meant to show, by authored row number counted from 1: rows with no
        // words (`rowsWithoutWords`) and rows holding a reply whose choice has no label (`unlabelledReplyRows`)
        public static void Problems(BlockFieldReader read, out List<int> rowsWithoutWords, out List<int> unlabelledReplyRows)
        {
            rowsWithoutWords = new List<int>();
            unlabelledReplyRows = new List<int>();
            var rows = read.Items(BuiltInBlocks.DialogueLinesField);
            for (int row = 0; row < rows.Count; row++)
            {
                if (read.ItemText(rows[row], BuiltInBlocks.DialogueTextField).Length == 0) rowsWithoutWords.Add(row + 1);
                for (int slot = 1; slot <= MaxChoices; slot++)
                    if (read.ItemText(rows[row], ChoiceField(slot)).Length == 0 && read.ItemText(rows[row], ReplyField(slot)).Length > 0)
                    {
                        unlabelledReplyRows.Add(row + 1);
                        break;
                    }
            }
        }
    }
}
