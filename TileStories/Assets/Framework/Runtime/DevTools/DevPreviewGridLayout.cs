using UnityEngine;
using System.Collections.Generic;

namespace TileStories
{
    // Shared pure layout math for a "two-block" dev preview grid: a row of A-cells, then a row of
    // B-cells, both wrapped at a column count chosen to fill the screen for the current camera's
    // field of view and aspect ratio. EffectsPreviewSpawner (block A = effects, block B = hierarchy
    // levels) and OutlinePreviewSpawner (block A = outline modes, block B = outline levels) both call
    // this, so the fitting math -- column choice, grid extent, camera distance, cell positions -- has
    // exactly one implementation instead of two copies that could drift apart.
    public static class DevPreviewGridLayout
    {
        private const float CellSpacing = 0.5f;
        private const float RowSpacing = 0.65f;
        private const float GridMargin = 0.6f;
        private const float ViewFill = 0.92f;
        private const float MinDistance = 0.5f;

        private static int RowsFor(int count, int columns) => count <= 0 ? 0 : (count + columns - 1) / columns;

        // Size of the grid, in metres, when the two blocks are wrapped at `columns` per row.
        public static Vector2 GridExtent(int aCount, int bCount, int columns)
        {
            int rows = RowsFor(aCount, columns) + RowsFor(bCount, columns);
            int cols = Mathf.Min(columns, Mathf.Max(aCount, bCount));
            return new Vector2((cols - 1) * CellSpacing + GridMargin, (rows - 1) * RowSpacing + GridMargin * 1.2f);
        }

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
        public static int ChooseColumns(int aCount, int bCount, float verticalFovDeg, float aspect)
        {
            int best = 1;
            float bestDistance = float.MaxValue;
            int max = Mathf.Max(1, Mathf.Max(aCount, bCount));
            for (int columns = 1; columns <= max; columns++)
            {
                float distance = FitDistance(GridExtent(aCount, bCount, columns), verticalFovDeg, aspect);
                if (distance < bestDistance - 1e-4f)
                {
                    best = columns;
                    bestDistance = distance;
                }
            }
            return best;
        }

        // Local cell positions: block A first, then block B, each wrapped at `columns`, every row
        // centred, the whole grid centred on the origin.
        public static List<Vector2> CellPositions(int aCount, int bCount, int columns)
        {
            var positions = new List<Vector2>();
            int row = 0;
            foreach (int count in new[] { aCount, bCount })
            {
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
    }
}
