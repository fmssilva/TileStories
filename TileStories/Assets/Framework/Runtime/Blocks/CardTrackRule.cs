using UnityEngine;

namespace TileStories
{
    // The width of one cell of a horizontal swipe track (the timeline's events, the related carousel's cards). A fixed cell width cuts a
    // card in half at the edge of the screen (a title read as "1755 / The"), which reads as broken, not as "swipe for more". So the cells
    // are stretched so that a WHOLE number of them, with their gaps, fills the visible strip: at rest the strip ends on a card edge, and
    // since every cell is the same width the end of the swipe ends on one too. Pure, so a test calls it with any numbers.
    public static class CardTrackRule
    {
        // How many whole cells fit the viewport when none may be narrower than `minCell` (at least one: a phone narrower than a cell still shows one)
        public static int WholeCells(float viewport, float minCell, float gap)
        {
            float step = minCell + gap;
            if (viewport <= 0f || step <= 0f) return 1;
            // - a hair of slack: a viewport of exactly n steps must count n, not n-1 after float error
            return Mathf.Max(1, Mathf.FloorToInt(viewport / step + 0.0001f));
        }

        // The cell width that makes WholeCells cells and the gap after each fill the viewport exactly (never below one pixel)
        public static float CellWidth(float viewport, float minCell, float gap)
        {
            if (viewport <= 0f) return minCell;
            int cells = WholeCells(viewport, minCell, gap);
            return Mathf.Max(1f, viewport / cells - gap);
        }
    }
}
