using System.Collections.Generic;

namespace TileStories
{
    // How the card's accordions open and close (_3.2 section 6.1, step 13), pure: the ONE mechanism every accordion of the card shares -- the
    // accordion layout's rows (BlockStackView's compact rows) and the contents list's section titles (nav_contents' Rows = Sections First).
    // NN/g mobile accordions (Budiu 2015): opening a row never scrolls the page; the open row's header stays reachable -- Back closes the
    // row opened last, and a row closed while its header had scrolled off brings that header back to the top, so the reader keeps their place.
    public static class AccordionRule
    {
        // The rows to close because `opening` opens: none, or -- with One At A Time -- every other open row, in the order they were opened
        public static IReadOnlyList<string> ClosedByOpening(IReadOnlyList<string> openInOrder, string opening, bool oneAtATime)
        {
            var closed = new List<string>();
            if (!oneAtATime || openInOrder == null) return closed;
            foreach (string key in openInOrder)
                if (key != opening) closed.Add(key);
            return closed;
        }

        // The row one Back press closes: the one opened last that is still open (null when none is)
        public static string ClosedByBack(IReadOnlyList<string> openInOrder) =>
            openInOrder != null && openInOrder.Count > 0 ? openInOrder[openInOrder.Count - 1] : null;

        // Where the stack rests after the visitor closed a row: where it was, unless the row's own header had scrolled above the viewport's
        // top -- then that header comes back to the top (the content under it is gone, and the reader would land somewhere else entirely)
        public static float OffsetAfterCollapse(float offset, float rowTop) =>
            !float.IsNaN(rowTop) && rowTop < offset ? rowTop : offset;

        // Where the stack rests after a row ABOVE the one being opened closed for it (One At A Time): the opened row keeps its place on
        // screen -- the content removed above it (`removedHeight`) is taken off the offset, never below 0. A closed row below it moves nothing
        public static float OffsetAfterClosingFor(float offset, float closedRowTop, float removedHeight, float openedRowTop)
        {
            if (float.IsNaN(closedRowTop) || float.IsNaN(openedRowTop) || float.IsNaN(removedHeight) || closedRowTop >= openedRowTop) return offset;
            return offset - removedHeight < 0f ? 0f : offset - removedHeight;
        }
    }
}
