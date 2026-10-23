using UnityEngine;

namespace TileStories
{
    // The header's two parts (_3.3 R10.9, R11 v3-v4, P4.4a), pure: the BAR -- the grab zone and one row holding the card's title beside the
    // round buttons, pinned, never taller than that row -- and the HERO, first in what scrolls: the picture full-width with the title, one
    // line and the card's actions on it (a plain plate for the text-only looks). The view measures, these answer:
    //   - Height: the hero fills what Half shows under the bar (Half - Peek, the bar being all the peek stop holds), so at Half the visitor
    //     sees the whole hero; never under the theme's minimum
    //   - BarShowsTitle: the bar shows the title only while the hero's title is not on screen -- at Peek (the hero is below the sheet's
    //     edge), and once the visitor scrolled the hero's title away; at the top of Half and Full the hero carries it
    //   - InView: whether any of the hero is still in the stack's viewport (the sticky bar waits until it is not, CardChromeRule)
    // The bar never changes size, so no answer here can move a stop (the old collapse rule's loop has nothing left to feed on).
    public static class HeroRule
    {
        // One pixel of rounding, the scroll rules' own (NavScrollRule.EndTolerance)
        public const float Tolerance = 1f;

        // The hero frame's height for these stops: Half's view under the bar, never under `minHeight`
        public static float Height(SheetStopRule.Stops stops, float minHeight)
        {
            float halfView = stops.Half - stops.Peek;
            return Mathf.Max(Mathf.Max(0f, minHeight), float.IsNaN(halfView) ? 0f : halfView);
        }

        // Whether the bar shows the title: at Peek always; at Half / Full once the hero's title (its bottom edge, in the stack's content
        // coordinates, as drawn: risen by the scroll) has left the viewport's top. Not laid out yet (NaN) = the hero still carries it
        public static bool BarShowsTitle(SheetStopRule.Stop stop, float scrollY, float heroTitleBottom)
        {
            if (stop == SheetStopRule.Stop.Peek) return true;
            if (float.IsNaN(heroTitleBottom) || float.IsNaN(scrollY)) return false;
            return scrollY >= heroTitleBottom - Tolerance;
        }

        // Whether some of the hero is still in the viewport: its bottom edge (content coordinates) below the scroll offset. No hero (NaN) =
        // nothing in view
        public static bool InView(float scrollY, float heroBottom) =>
            !float.IsNaN(heroBottom) && heroBottom - (float.IsNaN(scrollY) ? 0f : scrollY) > Tolerance;

        // How much of the hero is in view (0 when none): what the top strip must leave room for beside it
        public static float VisiblePart(float scrollY, float heroBottom) =>
            InView(scrollY, heroBottom) ? heroBottom - (float.IsNaN(scrollY) ? 0f : scrollY) : 0f;

        // The hero's title size class (R12.3: "hero 40-46 by width"): a phone takes the smaller display size, so a long title with its eyebrow,
        // line and two rows of actions still fits the hero Half shows; 46 from a large phone / tablet up
        public const float NarrowBelow = 400f;
        public static bool IsNarrow(float heroWidth) => !float.IsNaN(heroWidth) && heroWidth > 0f && heroWidth < NarrowBelow;
    }
}
