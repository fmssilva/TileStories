namespace TileStories
{
    // When the card's pinned header collapses to its compact look (_3.1 step 6C): once the visitor scrolls the stack, the
    // header gives its lower part (subtitle, picture, the heading above it) back to the scroll area; at the scroll top it
    // opens again. Pure, so the two traps are unit tests:
    //   - hysteresis: it collapses a little way down and opens only back at the very top, so a finger resting near the
    //     top does not flicker it
    //   - no loop: collapsing frees `freedHeight` of view, which shortens the scroll range by as much; a stack that would
    //     no longer scroll after collapsing would snap to the top, open the header, scroll again... so it never collapses
    //     unless it can still scroll afterwards
    public static class HeaderCollapseRule
    {
        // Scrolled further than this (panel units) the header collapses
        public const float CollapseAfter = 8f;

        // Scrolled back to within this of the top it opens again
        public const float OpenAtTop = 0.5f;

        // The next state. `scrollY`: the stack's scroll offset; `scrollRange`: how far it can scroll with the header open;
        // `freedHeight`: how much taller the scroll area gets when the header collapses.
        public static bool Next(bool collapsed, float scrollY, float scrollRange, float freedHeight)
        {
            if (collapsed) return scrollY > OpenAtTop;
            return scrollY > CollapseAfter && scrollRange - freedHeight > CollapseAfter;
        }
    }
}
