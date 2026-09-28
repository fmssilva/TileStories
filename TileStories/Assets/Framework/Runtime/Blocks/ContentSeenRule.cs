using System.Collections.Generic;

namespace TileStories
{
    // When a visitor has "seen the content" of the open card (_3.1 step 8A, show_after_viewed): the scrolling stack reached
    // its end, or all of it fits in what is visible. Pure, so every case is a unit test.
    //   - the stack's viewport must have a size: at the peek stop nothing is visible (viewport ~0), so nothing can have been seen
    //   - no layout yet (NaN) is not "seen"
    //   - within EndTolerance of the end counts as the end: a fling never lands on the exact last pixel
    public static class ContentSeenRule
    {
        // How far from the end still counts as the end (panel units)
        public const float EndTolerance = 2f;

        // `scrollOffset`: how far the stack is scrolled; `scrollRange`: how far it CAN scroll (content height - viewport height);
        // `viewportHeight`: the visible part's height
        public static bool HasSeenAll(float scrollOffset, float scrollRange, float viewportHeight)
        {
            if (float.IsNaN(scrollOffset) || float.IsNaN(scrollRange) || float.IsNaN(viewportHeight)) return false;
            if (!(viewportHeight > 0f)) return false;
            return scrollRange <= EndTolerance || scrollOffset >= scrollRange - EndTolerance;
        }

        // The family whose blocks always close a card (_3.2's ontology: "always last, never in primary nav")
        public const string MetaFamily = "meta";

        // Whether a block that waits for the reading (show_after_viewed) will appear where the visitor is: it is revealed in its own
        // slot once the stack reached its end, so it is only in view when nothing but meta blocks follow it. `familiesAfter`: the
        // family of every block written after it. A block in the middle of the card would appear above the visitor, where nobody
        // is looking, and push the content below it down.
        public static bool RevealsInView(IEnumerable<string> familiesAfter)
        {
            foreach (string family in familiesAfter)
                if (family != MetaFamily) return false;
            return true;
        }
    }
}
