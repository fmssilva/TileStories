using System.Collections.Generic;

namespace TileStories
{
    // Which options a poll block shows and how its numbers are read (_3.1 Tier 3, step 8B), pure so every case is a unit test.
    //   - the options are the rows with words, in the order written, at most MaxOptions; a poll needs a question and at least
    //     MinOptions of them, or it is not shown
    //   - a visitor's vote is stored as the option's AUTHORED ROW (Option.Row), so leaving a blank row out never moves the vote
    //   - results exist only if an IPollResults returns counts (no backend today); Percentages turns those counts into shares.
    //     Nothing here ever makes a number up.
    public static class PollRule
    {
        // The most options a poll shows, and the fewest it needs
        public const int MaxOptions = 6;
        public const int MinOptions = 2;

        public readonly struct Option
        {
            // The authored row's index (0-based): what the vote is stored under
            public readonly int Row;
            public readonly string Text;

            public Option(int row, string text)
            {
                Row = row;
                Text = text;
            }
        }

        // The options this block shows (blank rows and rows past MaxOptions are left out)
        public static List<Option> Options(BlockFieldReader read)
        {
            var options = new List<Option>();
            var rows = read.Items(BuiltInBlocks.PollOptionsField);
            for (int row = 0; row < rows.Count && options.Count < MaxOptions; row++)
            {
                string text = read.ItemText(rows[row], BuiltInBlocks.PollOptionTextField);
                if (text.Length > 0) options.Add(new Option(row, text));
            }
            return options;
        }

        // Whether the block has something to ask: a question and at least MinOptions options
        public static bool CanShow(BlockFieldReader read) =>
            read.Text(BuiltInBlocks.PollQuestionField).Length > 0 && Options(read).Count >= MinOptions;

        // Whether `row` is one of the options shown
        public static bool IsShownRow(IReadOnlyList<Option> options, int row)
        {
            foreach (var option in options)
                if (option.Row == row) return true;
            return false;
        }

        // The vote as an event reports it: the option's row number counted from 1
        public static string EventValue(int row) => (row + 1).ToString(System.Globalization.CultureInfo.InvariantCulture);

        // The share of the votes (whole percent, adding up to 100) of each option shown, from the counts an IPollResults gave
        // (indexed by authored row). null = nothing to show: no counts, or nobody has voted.
        public static int[] Percentages(IReadOnlyList<Option> options, IReadOnlyList<int> countsByRow)
        {
            if (countsByRow == null || options.Count == 0) return null;
            var counts = new int[options.Count];
            long total = 0;
            for (int i = 0; i < options.Count; i++)
            {
                int row = options[i].Row;
                counts[i] = row < countsByRow.Count ? System.Math.Max(0, countsByRow[row]) : 0;
                total += counts[i];
            }
            if (total <= 0) return null;
            // - largest remainder: round every share down, then give the missing points to the biggest remainders
            var shares = new int[options.Count];
            var remainders = new double[options.Count];
            int given = 0;
            for (int i = 0; i < counts.Length; i++)
            {
                double exact = counts[i] * 100.0 / total;
                shares[i] = (int)exact;
                remainders[i] = exact - shares[i];
                given += shares[i];
            }
            while (given < 100)
            {
                int best = 0;
                for (int i = 1; i < remainders.Length; i++)
                    if (remainders[i] > remainders[best]) best = i;
                shares[best]++;
                remainders[best] = -1.0;
                given++;
            }
            return shares;
        }
    }
}
