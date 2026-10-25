using System.Collections.Generic;

namespace TileStories
{
    // Pure (_3.3 P4.4c2, R11): the ONE block whose rim catches the moving light -- the block being read. Of the blocks the viewport shows
    // (any part of them), the one whose centre is nearest a line LineFraction down the viewport (the prototype's 45 %); the first of two at
    // the same distance. Measured over every framed block of the scroll, not only the navigation's targets: the light is the card's, not
    // the navigation's, and a card with no navigation lights its blocks too.
    public static class LitBlockRule
    {
        public const float LineFraction = 0.45f;

        // One block in scroll space: its top and bottom (content coordinates, the stack's TopInScroll)
        public readonly struct Block
        {
            public readonly float Top;
            public readonly float Bottom;

            public Block(float top, float bottom)
            {
                Top = top;
                Bottom = bottom;
            }

            public float Centre => (Top + Bottom) * 0.5f;
        }

        // The index of the lit block at this scroll offset, -1 when no block shows (an empty list, all above or below the viewport)
        public static int Pick(IReadOnlyList<Block> blocks, float offset, float viewport)
        {
            float line = offset + viewport * LineFraction;
            int best = -1;
            float bestDistance = float.MaxValue;
            for (int i = 0; i < blocks.Count; i++)
            {
                var block = blocks[i];
                // - a block touching an edge only (bottom == offset) shows none of itself
                if (block.Bottom <= offset || block.Top >= offset + viewport) continue;
                float distance = System.Math.Abs(block.Centre - line);
                if (distance >= bestDistance) continue;
                bestDistance = distance;
                best = i;
            }
            return best;
        }
    }
}
