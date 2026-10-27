using UnityEngine;

namespace TileStories
{
    // The header's two parts (_3.3 R10.9, R11 v3-v4, P4.4a), pure: the BAR -- the grab zone and one row holding the card's title beside the
    // round buttons, pinned, never taller than that row -- and the HERO, first in what scrolls: the picture full-width with the title, one
    // line and the card's actions on it (a plain plate for the text-only looks). The view measures, these answer:
    //   - Height: the whole view Full shows under the bar (FullView: Full - Peek, the bar being all the peek stop holds; _3.3 S4a: the hero
    //     is the screen, its picture cropped to cover it), never under the card's minimum
    //   - WordsBottom: the hero's words and actions end at its foot (S4a review): the picture above them clear, their place fixed
    //   - BarShowsTitle: the bar shows the title only while the hero's title is not on screen -- at Peek (the hero is below the sheet's
    //     edge), and once the visitor scrolled the hero's title away; at the top of Half and Full the hero carries it
    //   - InView: whether any of the hero is still in the stack's viewport (the sticky bar waits until it is not, CardChromeRule)
    // The bar never changes size, so no answer here can move a stop (the old collapse rule's loop has nothing left to feed on).
    public static class HeroRule
    {
        // One pixel of rounding, the scroll rules' own (NavScrollRule.EndTolerance)
        public const float Tolerance = 1f;

        // What Half shows under the bar: the most the hero may be (NaN-safe: 0 before the stops are measured)
        public static float HalfView(SheetStopRule.Stops stops)
        {
            float halfView = stops.Half - stops.Peek;
            return float.IsNaN(halfView) ? 0f : Mathf.Max(0f, halfView);
        }

        // What Full shows under the bar (NaN-safe: 0 before the stops are measured)
        public static float FullView(SheetStopRule.Stops stops)
        {
            float fullView = stops.Full - stops.Peek;
            return float.IsNaN(fullView) ? 0f : Mathf.Max(0f, fullView);
        }

        // The hero frame's height for these stops: the whole sheet at Full -- the picture runs up under the floating bars (_3.3 S4a review) --
        // never under `minHeight` (NaN-safe: the minimum before the stops are measured)
        public static float Height(SheetStopRule.Stops stops, float minHeight) =>
            Mathf.Max(Mathf.Max(0f, minHeight), float.IsNaN(stops.Full) ? 0f : stops.Full);

        // Where the hero's words and actions end, from the hero's top: at its foot, whatever the stop (_3.3 S4a review: the words keep their
        // place on the picture; Media.uss's --ts-hero-words-inset is the gap under them)
        public static float WordsBottom(float heroHeight) => Mathf.Max(0f, float.IsNaN(heroHeight) ? 0f : heroHeight);

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
