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
    //   4. THE HERO (_3.3 P4.4a): the hero holds the card's actions (the sticky one included, HeroActionsRule), so while any of it is in
    //      view the FOOTER waits (the bottom bar, navigation, does not); and no chrome is shown where it would cut the part of the hero in
    //      view -- at Half the hero fills the view (HeroRule.Height), so the visitor sees all of it, picture, title, line and actions
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
            // The hero's bottom edge in the stack's content coordinates (NaN: no hero) and the stack's scroll offset
            public float HeroBottom;
            public float ScrollOffset;
            // _3.3 S4a review: the top strip FLOATS over the stack (it takes no room, so the floor never hides it): it comes once the hero's
            // part in view leaves it room -- the visitor began scrolling the picture away -- or at once with TopAlways (the Navigation Bar's
            // Show = Always). TopHidesOnScroll: the scroll that hides the bottom area hides it too (the bar's Hide While Scrolling, at the top)
            public bool TopFloats;
            public bool TopAlways;
            public bool TopHidesOnScroll;
            // The bottom bar waits for the hero like a floating strip does (the Navigation Bar at the bottom, Show = After The Hero)
            public bool BottomNavAfterHero;
            // The bottom bar FLOATS over the stack's foot (the Navigation Bar at the bottom, v5): it takes no room from the stack, so the
            // floor never hides it -- over the hero too, where it lies on the picture
            public bool BottomNavFloats;
        }

        public readonly struct Chrome
        {
            public readonly bool Top;
            // The bottom area (whatever of the footer and the bottom bar it holds now)
            public readonly bool Bottom;
            // The bottom bar inside that area (only ever with Bottom)
            public readonly bool BottomNav;
            // The footer inside that area (only ever with Bottom): it waits for the hero, which shows the same action (layer 4)
            public readonly bool Footer;

            public Chrome(bool top, bool bottom, bool bottomNav, bool footer = true)
            {
                Top = top;
                Bottom = bottom;
                BottomNav = bottom && bottomNav;
                Footer = bottom && footer;
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
            float heroSeen = HeroRule.VisiblePart(input.ScrollOffset, input.HeroBottom);
            float room0 = input.SheetHeight - input.HeaderHeight;
            // - a floating strip: no floor; it waits for the hero (unless Always) and goes with the scroll when its bar says so
            if (input.TopFloats && top)
                top = (input.TopAlways || float.IsNaN(room0) || heroSeen <= room0 - input.TopHeight) && !(input.TopHidesOnScroll && input.ScrollHidesBottom);
            if (input.BottomNavAfterHero && nav && !float.IsNaN(room0)) nav = heroSeen <= room0 - input.BottomNavHeight;
            // - the hero shows the card's actions while it is in view: the sticky footer comes once it scrolled away
            bool footer = input.HasFooter && heroSeen <= 0f;
            bool bottom = (footer || nav) && IsAboveTheTitle(input.Stop) && !input.ScrollHidesBottom;
            float bottomHeight = BottomHeight(input, footer, nav);
            float room = input.SheetHeight - input.HeaderHeight;
            // - not laid out yet: no floor to apply, the next measure decides
            if (float.IsNaN(room)) return new Chrome(top, bottom, nav, footer);
            if (top && !input.TopFloats && room - input.TopHeight - (bottom ? bottomHeight : 0f) < Need(input.ContentHeight, Mathf.Max(MinStackViewport, heroSeen))) top = false;
            // - the floor weighs only what takes room from the stack: a footer, an in-flow bar -- never a bar floating over the stack's foot
            bool takesRoom = footer || nav && !input.BottomNavFloats;
            if (bottom && takesRoom && room - bottomHeight < Need(input.ContentHeight, Mathf.Max(MinStackViewportUnderFooter, heroSeen))) bottom = false;
            return new Chrome(top, bottom, nav, footer);
        }

        // The stack's viewport a decision leaves (what the visitor reads in)
        public static float Viewport(Input input, Chrome chrome) =>
            input.SheetHeight - input.HeaderHeight - (chrome.Top ? input.TopHeight : 0f) - (chrome.Bottom ? BottomHeight(input, chrome.Footer, chrome.BottomNav) : 0f);

        // The bottom area's height with or without its bar: the footer (when it holds a block) and the bar under it (a floating bar: none)
        public static float BottomHeight(Input input, bool withNav) => BottomHeight(input, true, withNav);

        // ... and with or without its footer (the footer waits for the hero, layer 4)
        public static float BottomHeight(Input input, bool withFooter, bool withNav) =>
            (withFooter && input.HasFooter ? input.FooterHeight : 0f) + (withNav && !input.BottomNavFloats ? input.BottomNavHeight : 0f);

        // The stop a navigation element (the top strip, the bottom bar) may show at: Full always, Half unless the wall says Full Only,
        // never at peek
        public static bool NavShownAt(SheetStopRule.Stop stop, string showAt) =>
            stop == SheetStopRule.Stop.Full || (stop == SheetStopRule.Stop.Half && showAt != CardOptions.ShowNavFullOnly);

        // Hide While Scrolling: anything but "never" hides (the default, an unknown value too)
        public static bool HidesOnScroll(string hideOnScroll) => hideOnScroll != CardOptions.HideOnScrollNever;

        // The stack is at its top / at its end (one pixel of rounding, NavScrollRule's)
        public static bool AtTop(float offset) => offset <= NavScrollRule.EndTolerance;

        // At the end, for the chrome, is measured on the range the stack has WITHOUT the bottom area (`bottomShown`: the area's height while
        // it is shown, else 0): so the area that came back at the end never hides again within its own height of the end. Hiding there gave
        // the visitor nothing more to read -- the grown viewport clamped the stack back and the area came again, a loop that left real drags
        // resting short of the end under the area (_3.3: 125 px on The Lamp with Next Up) or at the end with the area still away
        public static bool AtEnd(float offset, float contentHeight, float viewportHeight, float bottomShown = 0f)
        {
            float range = NavScrollRule.Range(contentHeight, viewportHeight + bottomShown);
            return range > 0f && offset >= range - NavScrollRule.EndTolerance;
        }

        // Where the stack rests once the bottom area came back at the end and took its height off the viewport (`viewportBefore` ->
        // `viewportAfter`): at the end of the NEW range, so the card's end stays in view above the area -- the end a visitor reaches is the
        // range with the chrome shown, the one NavScrollRule reads. A stack that was not at its end, or a viewport that did not shrink, stays
        public static float KeepTheEnd(float offset, float contentHeight, float viewportBefore, float viewportAfter)
        {
            if (!(viewportAfter < viewportBefore)) return offset;
            float before = NavScrollRule.Range(contentHeight, viewportBefore);
            if (!(before > 0f) || offset < before - NavScrollRule.EndTolerance) return offset;
            return NavScrollRule.Range(contentHeight, viewportAfter);
        }

        // The stack's offset `elapsed` seconds into its glide from `from` to the kept end `to` (_3.3 S2b, the resting-end review): the content
        // rises WITH the bottom area as it slides in, over the area's own `duration` and curve (the theme's --ts-ease: ease-out cubic), instead
        // of jumping by the area's height in one frame. No duration (Reduce Motion: the area jumps too), or the glide over: the end
        public static float GlideOffset(float from, float to, float elapsed, float duration)
        {
            if (!(duration > 0f) || float.IsNaN(elapsed) || elapsed >= duration) return to;
            float t = Mathf.Clamp01(elapsed / duration);
            float u = 1f - t;
            return Mathf.Lerp(from, to, 1f - u * u * u);
        }

        // A stack shorter than the floor needs only its own height; one not measured yet, the whole floor
        private static float Need(float contentHeight, float floor) =>
            float.IsNaN(contentHeight) || contentHeight < 0f ? floor : Mathf.Min(floor, contentHeight);

        private static bool IsAboveTheTitle(SheetStopRule.Stop stop) => stop == SheetStopRule.Stop.Half || stop == SheetStopRule.Stop.Full;
    }
}
