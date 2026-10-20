using System.Collections.Generic;

namespace TileStories
{
    // When a visitor has "seen the content" of the open card (_3.1 step 8A, show_after_viewed): the scrolling stack reached
    // its end, or all of it fits in what is visible -- and, in a layout that shows one section at a time, every other section read to its end
    // too (HasReadCard, _3.2 step 14). Pure, so every case is a unit test.
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

        // One section of the card as the reading sees it (_3.2 step 14): whether the stack shows it now, and whether it was read to its end
        public readonly struct Section
        {
            public readonly bool ShownNow;
            public readonly bool ReadToItsEnd;

            public Section(bool shownNow, bool readToItsEnd)
            {
                ShownNow = shownNow;
                ReadToItsEnd = readToItsEnd;
            }
        }

        // Whether the visitor has read the WHOLE card (_3.2 step 14, one rule for every layout): what the stack shows now was scrolled to its
        // end (HasSeenAll), AND every section the layout shows apart from it was read to its end before. Scroll and the accordion show every
        // section in one scroll, so the stack's end reads them all; tabs and pages show one section at a time, so the end of a short one
        // is not the end of the card
        public static bool HasReadCard(bool shownPartAtItsEnd, IEnumerable<Section> sections)
        {
            if (!shownPartAtItsEnd) return false;
            if (sections == null) return true;
            foreach (var section in sections)
                if (!section.ShownNow && !section.ReadToItsEnd) return false;
            return true;
        }

        // Whether a section was read to its end: its LAST target that shows content was viewed (NavScrollRule.HasViewed -- at the end of the
        // stack every block on screen counts). A closed compact row shows none of its block, so the target before it decides; a section of
        // closed rows only has nothing to read. `viewed` / `closedRow`: one per target, in the stack's order
        public static bool ReadToItsEnd(IReadOnlyList<bool> viewed, IReadOnlyList<bool> closedRow)
        {
            for (int i = viewed.Count - 1; i >= 0; i--)
                if (!closedRow[i]) return viewed[i];
            return true;
        }

        // Whether a block that waits for the reading (show_after_viewed) will appear where the visitor is: it is revealed in its own
        // slot once the stack reached its end, so it is only in view when nothing but meta blocks follow it. `familiesAfter`: the
        // family of every block written after it. A block in the middle of the card would appear above the visitor, where nobody
        // is looking, and push the content below it down.
        public static bool RevealsInView(IEnumerable<string> familiesAfter)
        {
            foreach (string family in familiesAfter)
                // - the family whose blocks always close a card (BuiltInFamilies.Meta: AtEnd on, InNav off)
                if (family != BuiltInFamilies.Meta) return false;
            return true;
        }
    }
}
