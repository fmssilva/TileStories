using UnityEngine;

namespace TileStories
{
    // The width of one cell of a horizontal swipe track (the timeline's events, the related carousel's cards). A fixed cell width cuts a
    // card in half at the edge of the screen (a title read as "1755 / The"), which reads as broken, not as "swipe for more". So the cells
    // are stretched so that a WHOLE number of them, with their gaps, fills the visible strip, and since every cell is the same width the end
    // of the swipe ends on one too. With Peek Next Card (card_settings.container.peek_next_card, 15.4.6) the strip at rest instead holds the
    // whole cells and a SLICE of the next one (`peek` wide) -- the "there is more" cue -- and the swipe still ends on a whole last cell.
    // Pure, so a test calls it with any numbers.
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

        // Whether a track of `cellCount` cells shows a slice of the next one at rest: only when the option is on (peek > 0) and there IS a
        // next cell -- more cells than fit whole beside the slice
        public static bool Peeks(float viewport, float minCell, float gap, float peek, int cellCount) =>
            peek > 0f && viewport > peek && cellCount > WholeCells(viewport - peek, minCell, gap);

        // The cell width for a track of `cellCount` cells with Peek Next Card asking for a `peek`-wide slice of the next cell: the whole cells
        // and their gaps fill the viewport less the slice. Where there is no next cell to show (Peeks false), the whole-cells width above
        public static float CellWidth(float viewport, float minCell, float gap, float peek, int cellCount)
        {
            if (!Peeks(viewport, minCell, gap, peek, cellCount)) return CellWidth(viewport, minCell, gap);
            return CellWidth(viewport - peek, minCell, gap);
        }
    }
}
