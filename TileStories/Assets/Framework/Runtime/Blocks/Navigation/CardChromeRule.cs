using UnityEngine;

namespace TileStories
{
    // What chrome the open card shows now (_3.2 section 4.3), pure: CardChromeView measures and applies. Chrome = what the sheet holds
    // besides the header and the content: the TOP strip (the top navigation slot) and the BOTTOM area -- the sticky CTA footer and, under
    // it, the bottom navigation slot (step 7a): ONE area, shown, slid away and given way together. Three layers, each can only hide more:
    //   1. the stop: nothing at peek (the card is its title there); Show Navigation At = full_only keeps the NAVIGATION for Full -- the
    //      top strip and the bottom bar; the footer is no navigation, so it stays at Half
    //   2. the scroll: Hide While Scrolling = down slides the bottom area away once the reader moved down by more than HideAfterTravel,
    //      and brings it back after ShowAfterTravel up, or at the top / end of the stack. The top strip stays: it is the map (NN/g
    //      sticky headers), the bottom is the "partially persistent" part that costs reading space on every screen
    //   3. THE FLOOR: the top strip is not shown where it would leave the stack's viewport under MinStackViewport (with the bottom area
    //      shown too) -- it gives way FIRST: the map is one drag to Full away. The bottom area then needs only one line of content
    //      (MinStackViewportUnderFooter), measured on the PAIR (footer + bottom bar): it outlasts the strip, and where the pair does not
    //      leave one line both go -- never half an area. The wall's options sit on top: they hide more, never less
    public static class CardChromeRule
    {
        // How far the reader must move down before the bottom area slides away (panel units): about a finger's travel, so the
        // wobble of a tap never hides it
        public const float HideAfterTravel = 24f;

        // How far back up brings it again: a small, deliberate move up is the reader looking for it
        public const float ShowAfterTravel = 8f;

        // The least of the stack a stop must keep in view before any chrome is shown (panel units): a block heading and two lines of
        // its block. Measured on The Lamp at 390 x 844 (_3.2 step 5b): heading 22 + its gap 8 + two body lines of 20 = 70, on the
        // 8 px grid. Below it a stop shows chrome and no content -- the 0 px Half of step 5a
        public const float MinStackViewport = 72f;

        // The least of the stack the BOTTOM area must leave (panel units): one body line (20) on the 8 px grid. The sticky CTA is the
        // one irreversible action the card offers and often why it was opened, so it outlasts the strip (the developer's call at the
        // 5b review): The Lamp's Half keeps its button over 25.6 px of content. Below one line it gives way too -- never a 0 px stack
        public const float MinStackViewportUnderFooter = 24f;

        // The scroll's memory: how far the reader moved in the current direction (+ down the card, - up), and whether that hid the
        // bottom area
        public readonly struct ScrollState
        {
            public readonly float Travel;
            public readonly bool BottomHidden;

            public ScrollState(float travel, bool bottomHidden)
            {
                Travel = travel;
                BottomHidden = bottomHidden;
            }
        }

        // What CardChromeView measured, and what the wall and the card say
        public struct Input
        {
            public SheetStopRule.Stop Stop;
            // Show Navigation At (CardOptions.ShowNavigationAts)
            public string ShowAt;
            // An element is bound in the top slot (Top Bar, Show Navigation From and rule 2.5.7 already said yes)
            public bool HasTop;
            // The bottom area's two parts: a footer block (the sticky CTA), an element bound in the bottom navigation slot
            public bool HasFooter;
            public bool HasBottomNav;
            // The scroll hid the bottom area (ScrollState.BottomHidden)
            public bool ScrollHidesBottom;
            // The sheet's height at its stop; everything above the top strip (the grab zone and the header with its pinned part)
            public float SheetHeight;
            public float HeaderHeight;
            // The chrome's own heights, measured shown or hidden; the stack's content
            public float TopHeight;
            public float FooterHeight;
            public float BottomNavHeight;
            public float ContentHeight;
        }

        public readonly struct Chrome
        {
            public readonly bool Top;
            // The bottom area (whatever of the footer and the bottom bar it holds now)
            public readonly bool Bottom;
            // The bottom bar inside that area (only ever with Bottom)
            public readonly bool BottomNav;

            public Chrome(bool top, bool bottom, bool bottomNav)
            {
                Top = top;
                Bottom = bottom;
                BottomNav = bottom && bottomNav;
            }
        }

        // The scroll moved by `delta` (the offset's change: + = further down the card). Travel adds up while the direction holds and
        // restarts on a turn; at the top or the end of the stack the bottom area always comes back. Never = nothing ever hides
        public static ScrollState Next(ScrollState state, float delta, bool atTop, bool atEnd, string hideOnScroll)
        {
            if (!HidesOnScroll(hideOnScroll)) return default;
            float travel = state.Travel;
            if (!float.IsNaN(delta) && delta != 0f)
                travel = travel == 0f || Mathf.Sign(delta) == Mathf.Sign(travel) ? travel + delta : delta;
            bool hidden = state.BottomHidden;
            if (atTop || atEnd) hidden = false;
            else if (travel > HideAfterTravel) hidden = true;
            else if (travel < -ShowAfterTravel) hidden = false;
            return new ScrollState(travel, hidden);
        }

        // How much of a scroll was the READER's: a jump that moves the stack (a tab, a bar slot, an Open Block button) is no reading --
        // counted, a tap on the bottom bar would slide that bar away under the finger that just used it (step 7a)
        public static float ReaderDelta(float delta, bool jumping) => jumping ? 0f : delta;

        // What shows now: the stop's chrome, less what the scroll hid, less what the floor cannot afford -- the strip first (it needs
        // MinStackViewport beside everything else shown), then the bottom area as one (it needs only MinStackViewportUnderFooter)
        public static Chrome Decide(Input input)
        {
            bool top = input.HasTop && NavShownAt(input.Stop, input.ShowAt);
            bool nav = input.HasBottomNav && NavShownAt(input.Stop, input.ShowAt);
            bool bottom = (input.HasFooter || nav) && IsAboveTheTitle(input.Stop) && !input.ScrollHidesBottom;
            float bottomHeight = BottomHeight(input, nav);
            float room = input.SheetHeight - input.HeaderHeight;
            // - not laid out yet: no floor to apply, the next measure decides
            if (float.IsNaN(room)) return new Chrome(top, bottom, nav);
            if (top && room - input.TopHeight - (bottom ? bottomHeight : 0f) < Need(input.ContentHeight, MinStackViewport)) top = false;
            if (bottom && room - bottomHeight < Need(input.ContentHeight, MinStackViewportUnderFooter)) bottom = false;
            return new Chrome(top, bottom, nav);
        }

        // The stack's viewport a decision leaves (what the visitor reads in)
        public static float Viewport(Input input, Chrome chrome) =>
            input.SheetHeight - input.HeaderHeight - (chrome.Top ? input.TopHeight : 0f) - (chrome.Bottom ? BottomHeight(input, chrome.BottomNav) : 0f);

        // The bottom area's height with or without its bar: the footer (when it holds a block) and the bar under it
        public static float BottomHeight(Input input, bool withNav) =>
            (input.HasFooter ? input.FooterHeight : 0f) + (withNav ? input.BottomNavHeight : 0f);

        // The stop a navigation element (the top strip, the bottom bar) may show at: Full always, Half unless the wall says Full Only,
        // never at peek
        public static bool NavShownAt(SheetStopRule.Stop stop, string showAt) =>
            stop == SheetStopRule.Stop.Full || (stop == SheetStopRule.Stop.Half && showAt != CardOptions.ShowNavFullOnly);

        // Hide While Scrolling: anything but "never" hides (the default, an unknown value too)
        public static bool HidesOnScroll(string hideOnScroll) => hideOnScroll != CardOptions.HideOnScrollNever;

        // The stack is at its top / at its end (one pixel of rounding, NavScrollRule's)
        public static bool AtTop(float offset) => offset <= NavScrollRule.EndTolerance;

        public static bool AtEnd(float offset, float contentHeight, float viewportHeight)
        {
            float range = NavScrollRule.Range(contentHeight, viewportHeight);
            return range > 0f && offset >= range - NavScrollRule.EndTolerance;
        }

        // A stack shorter than the floor needs only its own height; one not measured yet, the whole floor
        private static float Need(float contentHeight, float floor) =>
            float.IsNaN(contentHeight) || contentHeight < 0f ? floor : Mathf.Min(floor, contentHeight);

        private static bool IsAboveTheTitle(SheetStopRule.Stop stop) => stop == SheetStopRule.Stop.Half || stop == SheetStopRule.Stop.Full;
    }
}
