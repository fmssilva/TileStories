using System.Collections.Generic;

namespace TileStories
{
    // Where the bento layout's tiles go (_3.2 section 6.1, step 15b), pure: one tile per section, its AREA following the section's target
    // count, the tiles together filling the card's visible part. The progress map's problem on two axes, decided the same way: a share by
    // count, never under a tile's own minimum (the touch target, its words) -- the target wins (ProgressMapRule.Widths, the one share
    // arithmetic, sizes the widths inside a row and the rows' heights in the card).
    //   - the LEAD: the section with the most targets (the first of equals) takes a row of its own at the full width -- the screen's one
    //     focal point, never a grid of equals
    //   - the others two to a row in the model's order; a tile left alone (before the lead, or the last one) spans its row -- no hole. Two
    //     whose minimums do not fit side by side go one per row
    //   - the rows share the height by their target totals; when the rows' minimums alone are taller than the room every row keeps its
    //     minimum and the overview scrolls (Fills false): from then on only the widths inside a row still show the proportion
    public static class BentoLayoutRule
    {
        public readonly struct Tile
        {
            public readonly int Section;
            public readonly int Row;
            public readonly float X, Y, Width, Height;

            public Tile(int section, int row, float x, float y, float width, float height)
            {
                Section = section;
                Row = row;
                X = x;
                Y = y;
                Width = width;
                Height = height;
            }
        }

        public sealed class Result
        {
            // In the sections' order (the model's)
            public readonly List<Tile> Tiles = new();
            public readonly List<List<int>> Rows = new();
            // The height the tiles take: the room when they fill it, more when the rows' minimums are taller
            public float Height;
            // Whether the rows share the room by count (false: every row at its minimum, the overview scrolls)
            public bool Fills;
            // Rows held at their minimum height (their share was smaller)
            public int RowsAtMinimum;
        }

        // The section that leads (the most targets, the first of equals); -1 with none
        public static int Lead(IReadOnlyList<int> counts)
        {
            int lead = -1;
            for (int i = 0; i < (counts?.Count ?? 0); i++)
                if (lead < 0 || counts[i] > counts[lead]) lead = i;
            return lead;
        }

        // The rows, each a list of section indices in order: the lead alone, the others in pairs that fit side by side
        public static List<List<int>> Rows(IReadOnlyList<int> counts, IReadOnlyList<float> minWidths, float width, float gap)
        {
            var rows = new List<List<int>>();
            int lead = Lead(counts);
            List<int> pending = null;
            for (int i = 0; i < (counts?.Count ?? 0); i++)
            {
                if (i == lead)
                {
                    if (pending != null) rows.Add(pending);
                    pending = null;
                    rows.Add(new List<int> { i });
                    continue;
                }
                if (pending != null && minWidths[pending[0]] + gap + minWidths[i] <= width)
                {
                    pending.Add(i);
                    rows.Add(pending);
                    pending = null;
                    continue;
                }
                if (pending != null) rows.Add(pending);
                pending = new List<int> { i };
            }
            if (pending != null) rows.Add(pending);
            return rows;
        }

        // Every tile's place in a box `width` wide, filling `height` when the minimums allow; `gap` between tiles both ways
        public static Result Layout(IReadOnlyList<int> counts, IReadOnlyList<float> minWidths, float minHeight, float width, float height, float gap)
        {
            var result = new Result();
            int n = counts?.Count ?? 0;
            if (n == 0) return result;
            result.Rows.AddRange(Rows(counts, minWidths, width, gap));

            int rowCount = result.Rows.Count;
            var totals = new int[rowCount];
            var rowMinimums = new float[rowCount];
            for (int r = 0; r < rowCount; r++)
            {
                foreach (int s in result.Rows[r]) totals[r] += counts[s];
                rowMinimums[r] = minHeight;
            }
            float room = height - gap * (rowCount - 1);
            var heights = ProgressMapRule.Widths(totals, rowMinimums, room);
            float minimumTotal = minHeight * rowCount;
            result.Fills = !float.IsNaN(room) && minimumTotal < room;

            var tiles = new Tile[n];
            float y = 0f;
            for (int r = 0; r < rowCount; r++)
            {
                var row = result.Rows[r];
                if (heights[r] <= minHeight + 0.001f) result.RowsAtMinimum++;
                var rowCounts = new int[row.Count];
                var rowWidths = new float[row.Count];
                for (int k = 0; k < row.Count; k++)
                {
                    rowCounts[k] = counts[row[k]];
                    rowWidths[k] = minWidths[row[k]];
                }
                var widths = ProgressMapRule.Widths(rowCounts, rowWidths, width - gap * (row.Count - 1));
                float x = 0f;
                for (int k = 0; k < row.Count; k++)
                {
                    tiles[row[k]] = new Tile(row[k], r, x, y, widths[k], heights[r]);
                    x += widths[k] + gap;
                }
                y += heights[r] + (r < rowCount - 1 ? gap : 0f);
            }
            result.Tiles.AddRange(tiles);
            result.Height = y;
            return result;
        }
    }
}
