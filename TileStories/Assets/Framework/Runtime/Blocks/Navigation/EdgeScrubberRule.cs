using UnityEngine;

namespace TileStories
{
    // The geometry of the edge_scrubber element (_3.2 section 5.5, step 10): family dots down the right edge of the stack's viewport.
    // Pure: the view only draws what it says.
    //   - ONE dot per section, each in a full touch target (`target`, the touch token) that tiles the column with no gap, so a plain tap
    //     on any dot reaches its family (WCAG 2.2 SC 2.5.7: the drag has a single-pointer alternative) and no tap falls between two
    //   - the column is centred in the viewport's height; a viewport too short for EVERY target gets no rail at all (Fits), never a
    //     rail with a family a finger cannot reach -- the same way the top strip gives way where a stop leaves it too little room
    //   - a finger dragging along the column picks the dot its y falls in; past either end it keeps the end dot for one more target
    //     (a thumb overshoots), and further out it picks nothing, as the radial does outside its arc
    // All lengths in panel units, y measured from the viewport's top.
    public static class EdgeScrubberRule
    {
        // How far past an end of the column (in targets) a finger still picks the end dot
        public const float OverscrollTargets = 1f;

        public readonly struct Column
        {
            // Where the column starts (from the viewport's top), its height, one target's height and how many dots
            public readonly float Top;
            public readonly float Height;
            public readonly float Pitch;
            public readonly int Count;

            public Column(float top, float height, float pitch, int count)
            {
                Top = top;
                Height = height;
                Pitch = pitch;
                Count = count;
            }

            // The centre (y) of dot `index`
            public float CentreOf(int index) => Top + (index + 0.5f) * Pitch;
        }

        // Whether a viewport `viewportHeight` tall holds `count` full targets of `target` each (and so draws the rail)
        public static bool Fits(int count, float viewportHeight, float target) =>
            count > 0 && target > 0f && !float.IsNaN(viewportHeight) && count * target <= viewportHeight + 0.01f;

        // The column of `count` targets centred in the viewport; only meaningful where Fits says so
        public static Column Layout(int count, float viewportHeight, float target)
        {
            float height = Mathf.Max(0, count) * Mathf.Max(0f, target);
            return new Column(Mathf.Max(0f, (viewportHeight - height) / 2f), height, Mathf.Max(0f, target), Mathf.Max(0, count));
        }

        // The dot a finger at `y` picks (-1 = nothing: it is further past an end than OverscrollTargets)
        public static int IndexAt(Column column, float y)
        {
            if (column.Count <= 0 || column.Pitch <= 0f) return -1;
            float along = y - column.Top;
            float room = OverscrollTargets * column.Pitch;
            if (along < -room || along > column.Height + room) return -1;
            return Mathf.Clamp(Mathf.FloorToInt(along / column.Pitch), 0, column.Count - 1);
        }
    }
}
