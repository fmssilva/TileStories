using UnityEngine;

namespace TileStories
{
    // The card's "Show me where it is" button (_3.3 P2 review e, P3d), pure -- when it shows and where it rests; LocateFabView applies it:
    //   - a first-class action of every open card while the Navigation Bar and its Locate Button are on, at Half and Full (at Peek the wall
    //     is already in view), over the hero and after it; it goes with the bar while Hide While Scrolling slides the bar away (Bottom or Top)
    //   - at the bottom right, `gap` above the bar's highest point when the bar floats at the bottom (the raised disc's top: it never meets
    //     the bar's items, whichever group is chosen), else `gap` above the sheet's foot or a sticky footer shown there
    //   - the hero's words keep clear of it: their column ends `gap` before its left edge (WordsClear)
    public static class LocateFabRule
    {
        // Whether the button shows now: the bar and the Locate Button on, the sheet at Half or Full, and the scroll not hiding the bar
        public static bool Shown(CardNavigationSettings nav, SheetStopRule.Stop stop, bool scrollHidesBar) =>
            NavBarRule.Enabled(nav) && NavBarRule.LocateButton(nav.bar) && !scrollHidesBar
            && (stop == SheetStopRule.Stop.Half || stop == SheetStopRule.Stop.Full);

        // The button's bottom edge above the sheet's foot: `gap` over the highest of what lies under it -- the bar's highest point above the
        // foot (NaN: no bar at the bottom) and the top of a sticky footer shown there (0: none)
        public static float Bottom(float barHighest, float footerTop, float gap) =>
            Mathf.Max(float.IsNaN(barHighest) ? 0f : barHighest, float.IsNaN(footerTop) ? 0f : Mathf.Max(0f, footerTop)) + Mathf.Max(0f, gap);

        // How much of the hero's width, from its right edge, its words leave to the button (0 while it is not shown): the button's own size,
        // its inset from the edge and the gap before it
        public static float WordsClear(bool shown, float size, float inset, float gap) =>
            shown ? Mathf.Max(0f, inset) + Mathf.Max(0f, size) + Mathf.Max(0f, gap) : 0f;
    }
}
