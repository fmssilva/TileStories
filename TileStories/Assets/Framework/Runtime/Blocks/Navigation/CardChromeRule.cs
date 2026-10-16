using UnityEngine;

namespace TileStories
{
    // What chrome the open card shows now (_3.2 section 4.3), pure: CardChromeView measures and applies. Chrome = what the sheet holds
    // besides the header and the content: the TOP strip (the top navigation slot) and the BOTTOM area (the sticky CTA footer, and the
    // bottom navigation slot from step 7 on). Three layers, and each one can only hide more:
    //   1. the stop: nothing at peek (the card is its title there); Show Navigation At = full_only keeps the top strip for Full
    //   2. the scroll: Hide While Scrolling = down slides the bottom area away once the reader moved down by more than HideAfterTravel,
    //      and brings it back after ShowAfterTravel up, or at the top / end of the stack. The top strip stays: it is the map (NN/g
    //      sticky headers), the bottom is the "partially persistent" part that costs reading space on every screen
    //   3. THE FLOOR: a slot's chrome is not shown at a stop where it would leave the stack's viewport under MinStackViewport. The
    //      bottom area gives way first, then the top strip. The wall's options sit on top of it: they hide more, never less
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
            // The bottom area holds something (a footer block)
            public bool HasBottom;
            // The scroll hid the bottom area (ScrollState.BottomHidden)
            public bool ScrollHidesBottom;
            // The sheet's height at its stop; everything above the top strip (the grab zone and the header with its pinned part)
            public float SheetHeight;
            public float HeaderHeight;
            // The chrome's own heights, measured shown or hidden; the stack's content
            public float TopHeight;
            public float BottomHeight;
            public float ContentHeight;
        }

        public readonly struct Chrome
        {
            public readonly bool Top;
            public readonly bool Bottom;

            public Chrome(bool top, bool bottom)
            {
                Top = top;
                Bottom = bottom;
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

        // What shows now: the stop's chrome, less what the scroll hid, less what the floor cannot afford (bottom first)
        public static Chrome Decide(Input input)
        {
            bool top = input.HasTop && TopShownAt(input.Stop, input.ShowAt);
            bool bottom = input.HasBottom && IsAboveTheTitle(input.Stop) && !input.ScrollHidesBottom;
            float room = input.SheetHeight - input.HeaderHeight;
            // - not laid out yet: no floor to apply, the next measure decides
            if (float.IsNaN(room)) return new Chrome(top, bottom);
            float need = Need(input.ContentHeight);
            if (bottom && room - (top ? input.TopHeight : 0f) - input.BottomHeight < need) bottom = false;
            if (top && room - input.TopHeight < need) top = false;
            return new Chrome(top, bottom);
        }

        // The stack's viewport a decision leaves (what the visitor reads in)
        public static float Viewport(Input input, Chrome chrome) =>
            input.SheetHeight - input.HeaderHeight - (chrome.Top ? input.TopHeight : 0f) - (chrome.Bottom ? input.BottomHeight : 0f);

        // The stop the top strip may show at: Full always, Half unless the wall says Full Only, never at peek
        public static bool TopShownAt(SheetStopRule.Stop stop, string showAt) =>
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
        private static float Need(float contentHeight) =>
            float.IsNaN(contentHeight) || contentHeight < 0f ? MinStackViewport : Mathf.Min(MinStackViewport, contentHeight);

        private static bool IsAboveTheTitle(SheetStopRule.Stop stop) => stop == SheetStopRule.Stop.Half || stop == SheetStopRule.Stop.Full;
    }
}
