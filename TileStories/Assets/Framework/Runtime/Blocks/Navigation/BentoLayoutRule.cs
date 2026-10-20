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
    //   - a tile is never taller than its content (step 16): a card with few sections leaves the room under its tiles empty (RoomLeft) --
    //     "filling the card" was for a card with enough sections to fill it, never a promise to stretch a plate
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
            // The room under the last row no tile could use: every row reached what its content needs (0 when the tiles take all of it)
            public float RoomLeft;
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

        // Every tile's place in a box `width` wide, filling `height` when the minimums allow; `gap` between tiles both ways.
        // `contentHeight(section, tileWidth)`: how tall that section's tile must be to hold everything it shows at that width (its padding, its
        // title line, its block names where the width shows them, its count). A tile never grows past it (_3.2 step 16): a row is capped at its
        // tallest tile's content -- the row's tiles share one height, so no row has a hole -- the room a capped row cannot use is shared among
        // the others, and what nobody can use stays EMPTY under the last row (RoomLeft) instead of stretching plates
        public static Result Layout(IReadOnlyList<int> counts, IReadOnlyList<float> minWidths, float minHeight, float width, float height, float gap,
            System.Func<int, float, float> contentHeight)
        {
            var result = new Result();
            int n = counts?.Count ?? 0;
            if (n == 0) return result;
            result.Rows.AddRange(Rows(counts, minWidths, width, gap));

            // - the widths first: a row's widths never depend on the heights, and a tile's content (whether its names show) depends on its width
            int rowCount = result.Rows.Count;
            var widthsOf = new float[rowCount][];
            var totals = new int[rowCount];
            var rowMinimums = new float[rowCount];
            var rowMaximums = new float[rowCount];
            for (int r = 0; r < rowCount; r++)
            {
                var row = result.Rows[r];
                var rowCounts = new int[row.Count];
                var rowWidths = new float[row.Count];
                for (int k = 0; k < row.Count; k++)
                {
                    rowCounts[k] = counts[row[k]];
                    rowWidths[k] = minWidths[row[k]];
                    totals[r] += counts[row[k]];
                }
                widthsOf[r] = ProgressMapRule.Widths(rowCounts, rowWidths, width - gap * (row.Count - 1));
                rowMinimums[r] = minHeight;
                for (int k = 0; k < row.Count; k++)
                    rowMaximums[r] = System.Math.Max(rowMaximums[r], contentHeight(row[k], widthsOf[r][k]));
                // - a content lower than the minimum still keeps the minimum (the touch target, the title over the count)
                rowMaximums[r] = System.Math.Max(rowMaximums[r], minHeight);
            }

            float room = height - gap * (rowCount - 1);
            float minimumTotal = minHeight * rowCount;
            result.Fills = !float.IsNaN(room) && minimumTotal < room;
            var heights = CappedShares(totals, rowMinimums, rowMaximums, room);

            var tiles = new Tile[n];
            float y = 0f;
            for (int r = 0; r < rowCount; r++)
            {
                var row = result.Rows[r];
                if (heights[r] <= minHeight + 0.001f) result.RowsAtMinimum++;
                float x = 0f;
                for (int k = 0; k < row.Count; k++)
                {
                    tiles[row[k]] = new Tile(row[k], r, x, y, widthsOf[r][k], heights[r]);
                    x += widthsOf[r][k] + gap;
                }
                y += heights[r] + (r < rowCount - 1 ? gap : 0f);
            }
            result.Tiles.AddRange(tiles);
            result.Height = y;
            result.RoomLeft = float.IsNaN(height) ? 0f : System.Math.Max(0f, height - y);
            return result;
        }

        // The rows' heights: ProgressMapRule's share by count (never under a minimum), then every row whose share passed its maximum is fixed at
        // it and the room left is shared again among the others -- until no share passes its maximum. Each round fixes at least one more row,
        // so the rows' count of rounds always settles it; with every row at its maximum the rest of the room is left unused
        private static float[] CappedShares(int[] totals, float[] minimums, float[] maximums, float room)
        {
            int n = totals.Length;
            var heights = new float[n];
            var capped = new bool[n];
            for (int round = 0; round <= n; round++)
            {
                float left = room;
                var freeTotals = new List<int>();
                var freeMinimums = new List<float>();
                var free = new List<int>();
                for (int r = 0; r < n; r++)
                {
                    if (capped[r]) { left -= maximums[r]; continue; }
                    free.Add(r);
                    freeTotals.Add(totals[r]);
                    freeMinimums.Add(minimums[r]);
                }
                var shares = ProgressMapRule.Widths(freeTotals, freeMinimums, left);
                bool changed = false;
                for (int k = 0; k < free.Count; k++)
                {
                    int r = free[k];
                    heights[r] = shares[k];
                    if (shares[k] > maximums[r] + 0.001f)
                    {
                        capped[r] = true;
                        changed = true;
                    }
                }
                for (int r = 0; r < n; r++)
                    if (capped[r]) heights[r] = maximums[r];
                if (!changed) return heights;
            }
            return heights;
        }
    }
}
