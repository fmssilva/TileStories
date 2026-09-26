using UnityEngine;
using System.Collections.Generic;

namespace TileStories
{
    // Shared pure layout math for a dev preview grid built from N stacked blocks (each block starts
    // its own row -- a forced break at the block boundary -- and wraps internally only if its own
    // count exceeds the chosen column count), fitted to fill the screen for the current camera's
    // field of view and aspect ratio. EffectsPreviewSpawner (a quick-effects row, a ripple/halo combo
    // row, then one cell per hierarchy level) and OutlinePreviewSpawner (an outline-modes row, then
    // one cell per outline level) both call this, so the fitting math -- column choice, grid extent,
    // camera distance, cell positions -- has exactly one implementation instead of copies that could
    // drift apart. The original two-block (aCount, bCount) overloads are kept, unchanged in
    // behaviour, as the common case of a 2-element block list.
    public static class DevPreviewGridLayout
    {
        // CellSpacing/RowSpacing (2026-09-22): first tried tightening both to 0.32/0.42 for a more
        // compact grid, but the geometry test caught a real overlap -- 0.49 is not an arbitrary
        // number, it is the safety margin against the legend row's own marker footprint
        // (DefaultSizeCm 28cm * the 1.18x ring-size ratio ~= 0.33m diameter, needing >0.49 between
        // NEIGHBOUR centres, in EITHER axis, or physically adjacent markers visually clip into each
        // other). Left at the original, proven-safe values; GridMargin (the outer margin, not an
        // inter-cell distance) is still safely tightened.
        private const float CellSpacing = 0.5f;
        private const float RowSpacing = 0.65f;
        private const float GridMargin = 0.42f;
        private const float ViewFill = 0.92f;
        private const float MinDistance = 0.5f;

        private static int RowsFor(int count, int columns) => count <= 0 ? 0 : (count + columns - 1) / columns;

        private static int MaxOf(IReadOnlyList<int> counts)
        {
            int max = 0;
            for (int i = 0; i < counts.Count; i++) max = Mathf.Max(max, counts[i]);
            return max;
        }

        // Size of the grid, in metres, when every block is wrapped at `columns` per row.
        public static Vector2 GridExtent(IReadOnlyList<int> blockCounts, int columns)
        {
            int rows = 0, cols = 0;
            for (int i = 0; i < blockCounts.Count; i++)
            {
                rows += RowsFor(blockCounts[i], columns);
                cols = Mathf.Max(cols, Mathf.Min(columns, blockCounts[i]));
            }
            return new Vector2((cols - 1) * CellSpacing + GridMargin, (rows - 1) * RowSpacing + GridMargin * 1.2f);
        }

        public static Vector2 GridExtent(int aCount, int bCount, int columns) =>
            GridExtent(new[] { aCount, bCount }, columns);

        // Camera distance at which a grid of this extent fills (ViewFill of) the view.
        public static float FitDistance(Vector2 extent, float verticalFovDeg, float aspect)
        {
            float tan = Mathf.Tan(verticalFovDeg * Mathf.Deg2Rad * 0.5f);
            float byHeight = extent.y / (2f * tan);
            float byWidth = extent.x / (2f * tan * Mathf.Max(0.1f, aspect));
            return Mathf.Max(MinDistance, Mathf.Max(byWidth, byHeight) / ViewFill);
        }

        // The column count that needs the least distance, i.e. gives the biggest cells on screen
        // (a portrait view wants few columns, a wide one many).
        public static int ChooseColumns(IReadOnlyList<int> blockCounts, float verticalFovDeg, float aspect)
        {
            int best = 1;
            float bestDistance = float.MaxValue;
            int max = Mathf.Max(1, MaxOf(blockCounts));
            for (int columns = 1; columns <= max; columns++)
            {
                float distance = FitDistance(GridExtent(blockCounts, columns), verticalFovDeg, aspect);
                if (distance < bestDistance - 1e-4f)
                {
                    best = columns;
                    bestDistance = distance;
                }
            }
            return best;
        }

        public static int ChooseColumns(int aCount, int bCount, float verticalFovDeg, float aspect) =>
            ChooseColumns(new[] { aCount, bCount }, verticalFovDeg, aspect);

        // Local cell positions: block 0 first, then block 1, etc, each wrapped at `columns`, every
        // row centred, the whole grid centred on the origin.
        public static List<Vector2> CellPositions(IReadOnlyList<int> blockCounts, int columns)
        {
            var positions = new List<Vector2>();
            int row = 0;
            for (int b = 0; b < blockCounts.Count; b++)
            {
                int count = blockCounts[b];
                for (int start = 0; start < count; start += columns)
                {
                    int inRow = Mathf.Min(columns, count - start);
                    for (int i = 0; i < inRow; i++)
                        positions.Add(new Vector2((i - (inRow - 1) * 0.5f) * CellSpacing, -row * RowSpacing));
                    row++;
                }
            }
            float shift = (row - 1) * RowSpacing * 0.5f;
            for (int i = 0; i < positions.Count; i++)
                positions[i] += new Vector2(0f, shift);
            return positions;
        }

        public static List<Vector2> CellPositions(int aCount, int bCount, int columns) =>
            CellPositions(new[] { aCount, bCount }, columns);
    }
}
