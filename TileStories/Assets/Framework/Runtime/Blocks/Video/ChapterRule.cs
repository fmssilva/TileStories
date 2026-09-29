using System.Collections.Generic;

namespace TileStories
{
    // A video's chapters (_3.1 step 9B), pure: which authored rows become chips and which chip is lit at a time of the clip. A row shows when
    // it has a label and a time TimeCodeRule reads that falls inside the clip; the chips run in time order whatever order they were written in
    // (two rows at the same time keep the first). The chapter of a time is the last one starting at or before it (-1 before the first).
    public static class ChapterRule
    {
        public readonly struct Chapter
        {
            public readonly float Start;
            public readonly string Label;
            // The authored row it came from (a test and the Editor name rows by their written place)
            public readonly int Row;

            public Chapter(float start, string label, int row)
            {
                Start = start;
                Label = label;
                Row = row;
            }
        }

        // `rows`: each row's time text and label in written order; `length`: the clip's seconds (0 = not known: no row is past it)
        public static IReadOnlyList<Chapter> From(IReadOnlyList<(string Time, string Label)> rows, float length)
        {
            var chapters = new List<Chapter>();
            if (rows == null) return chapters;
            for (int i = 0; i < rows.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(rows[i].Label) || !TimeCodeRule.TryParse(rows[i].Time, out float start)) continue;
                if (length > 0f && start >= length) continue;
                if (chapters.Exists(c => c.Start == start)) continue;
                chapters.Add(new Chapter(start, rows[i].Label.Trim(), i));
            }
            // - a stable insertion sort by start: written order is kept for rows already in order
            for (int i = 1; i < chapters.Count; i++)
            {
                var chapter = chapters[i];
                int j = i - 1;
                while (j >= 0 && chapters[j].Start > chapter.Start) { chapters[j + 1] = chapters[j]; j--; }
                chapters[j + 1] = chapter;
            }
            return chapters;
        }

        public static int IndexAt(IReadOnlyList<Chapter> chapters, float time)
        {
            if (chapters == null || float.IsNaN(time)) return -1;
            for (int i = chapters.Count - 1; i >= 0; i--)
                if (time >= chapters[i].Start) return i;
            return -1;
        }
    }
}
