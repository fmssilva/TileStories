using UnityEngine;

namespace TileStories
{
    // The arithmetic of a strip of items wider than the screen that scrolls sideways (_3.2 step 5a: the text tabs; the chips and
    // segments of step 6 reuse it). Pure: the view only sets the offset it gets. An offset is how far the strip's content is moved
    // left, in panel units: 0 = its start in view, MaxOffset = its end against the right edge.
    public static class NavStripRule
    {
        // How far the content can move: nothing when it fits
        public static float MaxOffset(float contentWidth, float viewportWidth) =>
            float.IsNaN(contentWidth) || float.IsNaN(viewportWidth) ? 0f : Mathf.Max(0f, contentWidth - viewportWidth);

        // An offset kept inside the strip (a drag past either end stops at it)
        public static float Clamp(float offset, float contentWidth, float viewportWidth) =>
            Mathf.Clamp(offset, 0f, MaxOffset(contentWidth, viewportWidth));

        // The offset that shows a whole item (the one the spy marks): unchanged while it is already in view, else the least move that
        // brings it in -- its start to the left edge when it lies to the left, its end to the right edge when it lies to the right.
        // An item not laid out yet (NaN) leaves the offset where it is
        public static float OffsetToShow(float itemStart, float itemWidth, float viewportWidth, float contentWidth, float offset)
        {
            if (float.IsNaN(itemStart) || float.IsNaN(itemWidth) || float.IsNaN(viewportWidth) || viewportWidth <= 0f)
                return Clamp(offset, contentWidth, viewportWidth);
            float target = offset;
            if (itemStart < offset) target = itemStart;
            else if (itemStart + itemWidth > offset + viewportWidth) target = itemStart + itemWidth - viewportWidth;
            return Clamp(target, contentWidth, viewportWidth);
        }

        // The offset that puts an item in the MIDDLE of the strip (clamped at both ends): for a row a visitor steps through by tapping (the full
        // view's page chips, _3.2 step 16) -- its neighbours on both sides stay in view, so the next one is always a tap away, never cut at the
        // edge as the least move leaves it. An item not laid out yet (NaN) leaves the offset where it is
        public static float OffsetToCentre(float itemStart, float itemWidth, float viewportWidth, float contentWidth, float offset)
        {
            if (float.IsNaN(itemStart) || float.IsNaN(itemWidth) || float.IsNaN(viewportWidth) || viewportWidth <= 0f)
                return Clamp(offset, contentWidth, viewportWidth);
            return Clamp(itemStart + itemWidth / 2f - viewportWidth / 2f, contentWidth, viewportWidth);
        }
    }
}
