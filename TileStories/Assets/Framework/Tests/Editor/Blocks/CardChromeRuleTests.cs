using NUnit.Framework;

namespace TileStories.Editor.Tests
{
    // CardChromeRule (_3.2 step 5b, section 4.3): what chrome the card shows at each stop, how the scroll hides and brings back the bottom
    // area (the two travel thresholds, a turn, the top and the end), and THE FLOOR -- no chrome where it would leave the stack less than a
    // heading and two lines beside the strip, one line beside the footer, the strip giving way first -- at The Lamp's measured numbers and just above them.
    // Since step 7a the bottom area is a PAIR (the footer and the bottom bar under it): one floor for both, they go and come back together,
    // and Show Navigation At = Full Only hides the bar alone at Half (it is navigation; the footer is not).
    public class CardChromeRuleTests
    {
        // The Lamp at 390 x 844, measured at step 5b (PoiCardChromeSceneTests measures them again on the real card): the sheet at Half and
        // Full, everything above the strip (the grab zone 24 + its border 1 + the header 206), the strip, the sticky footer, the stack
        private const float LampHalf = 337.6f;
        private const float LampFull = 796f;
        private const float LampHeader = 231f;
        private const float LampStrip = 49f;
        private const float LampFooter = 81f;
        private const float LampContent = 13669f;
        // The bottom bar (bottom_bar at 390 px: icon + word in a touch-target slot, 52 px measured by PoiCardBottomBarSceneTests)
        private const float LampBar = 52f;

        private static CardChromeRule.Input Lamp(SheetStopRule.Stop stop, string showAt = CardOptions.ShowNavHalfAndFull, bool scrollHid = false) => new()
        {
            Stop = stop, ShowAt = showAt, HasTop = true, HasFooter = true, ScrollHidesBottom = scrollHid,
            SheetHeight = stop == SheetStopRule.Stop.Full ? LampFull : LampHalf, HeaderHeight = LampHeader,
            TopHeight = LampStrip, FooterHeight = LampFooter, ContentHeight = LampContent,
        };

        // The Lamp with a Bottom Bar under its footer
        private static CardChromeRule.Input LampWithBar(SheetStopRule.Stop stop, string showAt = CardOptions.ShowNavHalfAndFull, bool scrollHid = false)
        {
            var input = Lamp(stop, showAt, scrollHid);
            input.HasBottomNav = true;
            input.BottomNavHeight = LampBar;
            return input;
        }

        // A sheet with room for the strip, the footer AND the floor: only the stop and the options decide
        private static CardChromeRule.Input Roomy(SheetStopRule.Stop stop, string showAt)
        {
            var input = Lamp(stop, showAt);
            input.SheetHeight = LampFull;
            return input;
        }

        private static CardChromeRule.ScrollState Scroll(CardChromeRule.ScrollState state, string hideOnScroll, params float[] deltas)
        {
            foreach (float delta in deltas) state = CardChromeRule.Next(state, delta, false, false, hideOnScroll);
            return state;
        }

        // ---------------- the stop and Show Navigation At ----------------

        [Test]
        public void EveryStop_WithBothShowNavigationAtOptions_ShowsWhatTheStopAllows()
        {
            foreach (string showAt in CardOptions.ShowNavigationAts)
            {
                var peek = CardChromeRule.Decide(Roomy(SheetStopRule.Stop.Peek, showAt));
                Assert.IsFalse(peek.Top, showAt + ": nothing at peek, top");
                Assert.IsFalse(peek.Bottom, showAt + ": nothing at peek, bottom");
                var dismissed = CardChromeRule.Decide(Roomy(SheetStopRule.Stop.Dismissed, showAt));
                Assert.IsFalse(dismissed.Top || dismissed.Bottom, showAt + ": nothing on a closed card");
                var full = CardChromeRule.Decide(Roomy(SheetStopRule.Stop.Full, showAt));
                Assert.IsTrue(full.Top && full.Bottom, showAt + ": everything at Full");
            }
            Assert.IsTrue(CardChromeRule.Decide(Roomy(SheetStopRule.Stop.Half, CardOptions.ShowNavHalfAndFull)).Top, "Half And Full: the strip at Half");
            var fullOnly = CardChromeRule.Decide(Roomy(SheetStopRule.Stop.Half, CardOptions.ShowNavFullOnly));
            Assert.IsFalse(fullOnly.Top, "Full Only: no strip at Half");
            Assert.IsTrue(fullOnly.Bottom, "Full Only is about the strip: the footer still shows at Half");
            Assert.IsTrue(CardChromeRule.Decide(Roomy(SheetStopRule.Stop.Half, "typo")).Top, "an unknown value reads as the default, Half And Full");
            Assert.AreEqual(CardOptions.ShowNavHalfAndFull, new CardNavigationSettings().show_at, "the default");
        }

        [Test]
        public void NoElementOrNoFooter_IsNeverShown_WhateverTheRoom()
        {
            var input = Roomy(SheetStopRule.Stop.Full, CardOptions.ShowNavHalfAndFull);
            input.HasTop = false;
            input.HasFooter = false;
            var chrome = CardChromeRule.Decide(input);
            Assert.IsFalse(chrome.Top);
            Assert.IsFalse(chrome.Bottom);
        }

        // ---------------- the scroll ----------------

        [Test]
        public void MovingDown_HidesTheBottomArea_OnlyPastHideAfterTravel_AddedUpOverSmallMoves()
        {
            Assert.AreEqual(24f, CardChromeRule.HideAfterTravel);
            var state = Scroll(default, CardOptions.HideOnScrollDown, 5f, 5f, 5f, 5f, 3f);
            Assert.AreEqual(23f, state.Travel, 0.001f);
            Assert.IsFalse(state.BottomHidden, "23 px down: a tap's wobble, still shown");
            state = Scroll(state, CardOptions.HideOnScrollDown, 2f);
            Assert.IsTrue(state.BottomHidden, "25 px down: hidden");
        }

        [Test]
        public void MovingUp_BringsItBack_OnlyPastShowAfterTravel()
        {
            Assert.AreEqual(8f, CardChromeRule.ShowAfterTravel);
            var state = Scroll(default, CardOptions.HideOnScrollDown, 40f);
            Assert.IsTrue(state.BottomHidden, "precondition: hidden");
            state = Scroll(state, CardOptions.HideOnScrollDown, -3f, -4f);
            Assert.AreEqual(-7f, state.Travel, 0.001f, "a turn restarts the travel from the new direction");
            Assert.IsTrue(state.BottomHidden, "7 px up: still hidden");
            state = Scroll(state, CardOptions.HideOnScrollDown, -2f);
            Assert.IsFalse(state.BottomHidden, "9 px up: back");
        }

        [Test]
        public void ATurn_RestartsTheTravel_SoTwoShortMovesDownAroundAnUpOneHideNothing()
        {
            var state = Scroll(default, CardOptions.HideOnScrollDown, 20f, -3f, 20f);
            Assert.AreEqual(20f, state.Travel, 0.001f);
            Assert.IsFalse(state.BottomHidden, "20 down, 3 up, 20 down: never 24 in one direction");
            Assert.AreEqual(20f, CardChromeRule.Next(state, 0f, false, false, CardOptions.HideOnScrollDown).Travel, 0.001f, "no move keeps the travel");
        }

        [Test]
        public void AtTheTopOrTheEnd_TheBottomAreaIsShown_WhateverTheTravel()
        {
            var hidden = Scroll(default, CardOptions.HideOnScrollDown, 100f);
            Assert.IsTrue(hidden.BottomHidden);
            Assert.IsFalse(CardChromeRule.Next(hidden, 50f, false, true, CardOptions.HideOnScrollDown).BottomHidden, "at the end of the stack: back");
            Assert.IsFalse(CardChromeRule.Next(hidden, -100f, true, false, CardOptions.HideOnScrollDown).BottomHidden, "at its top: back");
            Assert.IsTrue(CardChromeRule.AtTop(0f) && CardChromeRule.AtTop(NavScrollRule.EndTolerance) && !CardChromeRule.AtTop(2f));
            Assert.IsTrue(CardChromeRule.AtEnd(900f, 1000f, 100f), "offset = range");
            Assert.IsTrue(CardChromeRule.AtEnd(899.5f, 1000f, 100f), "within one pixel of it");
            Assert.IsFalse(CardChromeRule.AtEnd(850f, 1000f, 100f));
            Assert.IsFalse(CardChromeRule.AtEnd(0f, 50f, 100f), "a stack that does not scroll is at its top, never at its end");
        }

        // _3.2 audit F1 (mutation 23): reaching the top brings the area back on its own, however small the move up was (a small move up
        // alone is not past ShowAfterTravel); the same at the end after a small move down
        [Test]
        public void ReachingTheTopAfterASmallMoveUp_BringsTheBottomAreaBack_AndSoDoesTheEndAfterASmallMoveDown()
        {
            var hidden = Scroll(default, CardOptions.HideOnScrollDown, 40f);
            Assert.IsTrue(hidden.BottomHidden, "precondition: hidden");
            var small = CardChromeRule.Next(hidden, -(CardChromeRule.ShowAfterTravel - 1f), true, false, CardOptions.HideOnScrollDown);
            Assert.Greater(small.Travel, -CardChromeRule.ShowAfterTravel, "precondition: the move up alone is not past ShowAfterTravel");
            Assert.IsFalse(small.BottomHidden, "the stack reached its top: the area is back");
            var elsewhere = CardChromeRule.Next(hidden, -(CardChromeRule.ShowAfterTravel - 1f), false, false, CardOptions.HideOnScrollDown);
            Assert.IsTrue(elsewhere.BottomHidden, "the same small move up away from the top: still hidden");
            var atEnd = CardChromeRule.Next(hidden, 1f, false, true, CardOptions.HideOnScrollDown);
            Assert.IsFalse(atEnd.BottomHidden, "the stack reached its end: the area is back");
        }

        // _3.2 audit F1 (mutations 25 and 27): the two travels are exact -- exactly HideAfterTravel down hides nothing, exactly
        // ShowAfterTravel up brings nothing back; one unit past each does
        [Test]
        public void TheTwoTravelsAreExact_TheLimitItselfChangesNothing_OneUnitPastItDoes()
        {
            const float past = 0.01f;
            var atLimit = Scroll(default, CardOptions.HideOnScrollDown, CardChromeRule.HideAfterTravel);
            Assert.IsFalse(atLimit.BottomHidden, "exactly HideAfterTravel down: not past it");
            Assert.IsTrue(Scroll(atLimit, CardOptions.HideOnScrollDown, past).BottomHidden, "a hair past it: hidden");

            var hidden = Scroll(default, CardOptions.HideOnScrollDown, CardChromeRule.HideAfterTravel * 2f);
            Assert.IsTrue(hidden.BottomHidden, "precondition: hidden");
            var upToLimit = Scroll(hidden, CardOptions.HideOnScrollDown, -CardChromeRule.ShowAfterTravel);
            Assert.AreEqual(-CardChromeRule.ShowAfterTravel, upToLimit.Travel, 0.0001f, "precondition: the turn restarted the travel at the limit");
            Assert.IsTrue(upToLimit.BottomHidden, "exactly ShowAfterTravel up: not past it");
            Assert.IsFalse(Scroll(upToLimit, CardOptions.HideOnScrollDown, -past).BottomHidden, "a hair past it: back");
        }

        // A jump moving the stack is not the reader moving: its scroll never hides the bottom area (the bar a visitor just tapped stays)
        [Test]
        public void AJumpsScroll_IsNoReading_AndHidesNothing()
        {
            Assert.AreEqual(0f, CardChromeRule.ReaderDelta(800f, jumping: true), "a jump's 800 px count for nothing");
            Assert.AreEqual(800f, CardChromeRule.ReaderDelta(800f, jumping: false), "the reader's own 800 px count");
            var state = CardChromeRule.Next(default, CardChromeRule.ReaderDelta(800f, true), false, false, CardOptions.HideOnScrollDown);
            Assert.IsFalse(state.BottomHidden, "after the jump the bottom area still shows");
        }

        [Test]
        public void HideWhileScrollingNever_NeverHides()
        {
            var state = Scroll(default, CardOptions.HideOnScrollNever, 100f, 300f, 1000f);
            Assert.IsFalse(state.BottomHidden);
            Assert.AreEqual(CardOptions.HideOnScrollDown, new CardNavigationSettings().hide_on_scroll, "the default hides");
            Assert.IsTrue(CardChromeRule.HidesOnScroll("typo"), "an unknown value reads as the default");
        }

        [Test]
        public void WhatTheScrollHid_IsTheBottomAreaOnly_TheStripStays()
        {
            var chrome = CardChromeRule.Decide(Lamp(SheetStopRule.Stop.Full, scrollHid: true));
            Assert.IsTrue(chrome.Top, "the strip is the map: it stays");
            Assert.IsFalse(chrome.Bottom, "the footer is gone");
            Assert.AreEqual(LampFull - LampHeader - LampStrip, CardChromeRule.Viewport(Lamp(SheetStopRule.Stop.Full, scrollHid: true), chrome), 0.001f,
                "and its height went to the stack");
        }

        // ---------------- the floor ----------------

        [Test]
        public void TheFloor_AtTheLampsHalf_KeepsTheFooter_AndTheStripGivesWayFirst()
        {
            Assert.AreEqual(72f, CardChromeRule.MinStackViewport);
            Assert.AreEqual(24f, CardChromeRule.MinStackViewportUnderFooter);
            var input = Lamp(SheetStopRule.Stop.Half);
            Assert.Less(LampHalf - LampHeader - LampStrip - LampFooter, 0f, "precondition: strip + footer do not even fit (the 0 px Half of 5a)");
            Assert.GreaterOrEqual(LampHalf - LampHeader - LampFooter, CardChromeRule.MinStackViewportUnderFooter,
                "precondition: the footer alone leaves one line (25.6 px)");
            var chrome = CardChromeRule.Decide(input);
            Assert.IsFalse(chrome.Top, "the strip gives way first");
            Assert.IsTrue(chrome.Bottom, "the footer stays: its button is why the card was opened");
            Assert.AreEqual(LampHalf - LampHeader - LampFooter, CardChromeRule.Viewport(input, chrome), 0.001f, "the stack keeps 25.6 px");

            var full = CardChromeRule.Decide(Lamp(SheetStopRule.Stop.Full));
            Assert.IsTrue(full.Top && full.Bottom, "Full has room for both");
        }

        [Test]
        public void TheFloor_AroundTheLampsNumbers_KeepsTheFooterFirst_ThenTheStripToo()
        {
            var input = Lamp(SheetStopRule.Stop.Half);
            float floor = CardChromeRule.MinStackViewport;
            float line = CardChromeRule.MinStackViewportUnderFooter;

            input.SheetHeight = LampHeader + LampFooter + line - 0.5f;
            var chrome = CardChromeRule.Decide(input);
            Assert.IsFalse(chrome.Top || chrome.Bottom, "half a pixel short of footer + one line: neither");

            input.SheetHeight = LampHeader + LampFooter + line;
            chrome = CardChromeRule.Decide(input);
            Assert.IsTrue(chrome.Bottom, "room for the footer and one line: the footer");
            Assert.IsFalse(chrome.Top, "but not the strip as well");
            Assert.AreEqual(line, CardChromeRule.Viewport(input, chrome), 0.001f, "the stack keeps exactly one line");

            input.SheetHeight = LampHeader + LampStrip + LampFooter + floor - 0.5f;
            Assert.IsFalse(CardChromeRule.Decide(input).Top, "half a pixel short of both and the floor: still no strip");
            input.SheetHeight = LampHeader + LampStrip + LampFooter + floor;
            chrome = CardChromeRule.Decide(input);
            Assert.IsTrue(chrome.Top && chrome.Bottom, "room for both and the floor: both");
        }

        [Test]
        public void TheStrip_WithTheFooterScrolledAway_NeedsTheFloorBesideItselfOnly()
        {
            // - the footer slid away while reading: the strip comes back as soon as it alone leaves the floor
            var input = Lamp(SheetStopRule.Stop.Half, scrollHid: true);
            input.SheetHeight = LampHeader + LampStrip + CardChromeRule.MinStackViewport;
            var chrome = CardChromeRule.Decide(input);
            Assert.IsTrue(chrome.Top, "room for the strip and the floor");
            Assert.IsFalse(chrome.Bottom, "the scroll hid the footer");
            input.SheetHeight -= 1f;
            Assert.IsFalse(CardChromeRule.Decide(input).Top, "one pixel less: the strip gives way");
        }

        [Test]
        public void TheFloor_ForTheBottomArea_WithNoStrip_IsOneLine()
        {
            // - the bottom bar of step 7 gets the floor for free: it is measured as the bottom area, whatever fills it
            var input = Lamp(SheetStopRule.Stop.Half);
            input.HasTop = false;
            input.SheetHeight = LampHeader + LampFooter + CardChromeRule.MinStackViewportUnderFooter;
            Assert.IsTrue(CardChromeRule.Decide(input).Bottom, "room for the footer and one line");
            input.SheetHeight -= 1f;
            Assert.IsFalse(CardChromeRule.Decide(input).Bottom, "one pixel less: the footer gives way");
        }

        [Test]
        public void ShowNavigationAt_SitsOnTopOfTheFloor_ItHidesMoreNeverLess()
        {
            var input = Lamp(SheetStopRule.Stop.Half, CardOptions.ShowNavFullOnly);
            input.SheetHeight = LampFull;
            Assert.IsFalse(CardChromeRule.Decide(input).Top, "plenty of room at Half, but Full Only: no strip");
            input = Lamp(SheetStopRule.Stop.Half, CardOptions.ShowNavHalfAndFull);
            Assert.IsFalse(CardChromeRule.Decide(input).Top, "Half And Full cannot bring the strip back where the floor hid it");
        }

        [Test]
        public void AStackShorterThanTheFloor_NeedsOnlyItsOwnHeight_AndAStackNotLaidOutYetTheWholeFloor()
        {
            var input = Lamp(SheetStopRule.Stop.Half);
            input.HasFooter = false;
            input.ContentHeight = 40f;
            Assert.IsTrue(CardChromeRule.Decide(input).Top, "57.6 px left after the strip holds the whole 40 px stack");

            input.HasFooter = true;
            input.ContentHeight = 20f;
            var chrome = CardChromeRule.Decide(input);
            Assert.IsTrue(chrome.Bottom, "a 20 px stack fits the 25.6 px the footer leaves");
            Assert.IsFalse(chrome.Top, "the strip as well would leave none of it");

            input.HasFooter = false;
            input.ContentHeight = float.NaN;
            Assert.IsFalse(CardChromeRule.Decide(input).Top, "content not measured yet: the full floor");

            input.HasFooter = true;
            input.SheetHeight = float.NaN;
            var unmeasured = CardChromeRule.Decide(input);
            Assert.IsTrue(unmeasured.Top && unmeasured.Bottom, "a sheet not laid out yet: no floor to apply, the stop decides");
        }

        // ---------------- the bottom area as a pair (step 7a) ----------------

        // The Lamp at Half with a Bottom Bar: 106.6 px under the header cannot hold footer 81 + bar 52 + one line, so the WHOLE area goes
        // (never half an area) and so does the strip -- the stack gets every pixel. The rule working, not a bug: Full has room for all
        [Test]
        public void TheLampAtHalf_WithABottomBar_GivesTheWholeRoomToTheStack_AndFullShowsEverything()
        {
            var input = LampWithBar(SheetStopRule.Stop.Half);
            float room = LampHalf - LampHeader;
            Assert.Less(room - LampFooter - LampBar, CardChromeRule.MinStackViewportUnderFooter, "precondition: the pair leaves less than one line");
            var chrome = CardChromeRule.Decide(input);
            Assert.IsFalse(chrome.Bottom, "the bottom area gives way as one");
            Assert.IsFalse(chrome.BottomNav, "the bar with it");
            Assert.IsFalse(chrome.Top, "and the strip (it gave way first)");
            Assert.AreEqual(room, CardChromeRule.Viewport(input, chrome), 0.001f, "the stack keeps all 106.6 px");

            var full = LampWithBar(SheetStopRule.Stop.Full);
            chrome = CardChromeRule.Decide(full);
            Assert.IsTrue(chrome.Top && chrome.Bottom && chrome.BottomNav, "Full: strip, footer and bar");
            Assert.AreEqual(LampFull - LampHeader - LampStrip - LampFooter - LampBar, CardChromeRule.Viewport(full, chrome), 0.001f,
                "the stack keeps what the three leave");
        }

        // One floor measures the pair, just above and below it: room for footer + bar + one line shows both, half a pixel less shows neither
        [Test]
        public void TheFloor_MeasuresTheFooterAndTheBarTogether()
        {
            var input = LampWithBar(SheetStopRule.Stop.Half);
            input.HasTop = false;
            input.SheetHeight = LampHeader + LampFooter + LampBar + CardChromeRule.MinStackViewportUnderFooter;
            var chrome = CardChromeRule.Decide(input);
            Assert.IsTrue(chrome.Bottom && chrome.BottomNav, "room for the pair and one line: both");
            Assert.AreEqual(CardChromeRule.MinStackViewportUnderFooter, CardChromeRule.Viewport(input, chrome), 0.001f, "exactly one line left");
            input.SheetHeight -= 0.5f;
            chrome = CardChromeRule.Decide(input);
            Assert.IsFalse(chrome.Bottom || chrome.BottomNav, "half a pixel less: neither -- the footer does not stay without its bar");

            // - the strip still needs its own floor beside the whole pair
            input.HasTop = true;
            input.SheetHeight = LampHeader + LampStrip + LampFooter + LampBar + CardChromeRule.MinStackViewport;
            Assert.IsTrue(CardChromeRule.Decide(input).Top, "room for the strip, the pair and the floor");
            input.SheetHeight -= 0.5f;
            chrome = CardChromeRule.Decide(input);
            Assert.IsFalse(chrome.Top, "half a pixel less: the strip gives way first");
            Assert.IsTrue(chrome.Bottom && chrome.BottomNav, "the pair stays");
        }

        // A card with a bar and no sticky footer: the area is the bar alone, with the same one-line floor
        [Test]
        public void ABarWithNoFooter_IsTheWholeArea()
        {
            var input = LampWithBar(SheetStopRule.Stop.Half);
            input.HasTop = false;
            input.HasFooter = false;
            input.SheetHeight = LampHeader + LampBar + CardChromeRule.MinStackViewportUnderFooter;
            var chrome = CardChromeRule.Decide(input);
            Assert.IsTrue(chrome.Bottom && chrome.BottomNav, "the bar alone");
            Assert.AreEqual(LampBar, CardChromeRule.BottomHeight(input, true), 0.001f, "a footer with no block adds nothing");
            input.SheetHeight -= 1f;
            Assert.IsFalse(CardChromeRule.Decide(input).Bottom, "one pixel less: it gives way");
            input.HasBottomNav = false;
            input.SheetHeight = LampFull;
            Assert.IsFalse(CardChromeRule.Decide(input).Bottom, "neither part: no area");
        }

        // Hide While Scrolling slides the PAIR away, and both come back together
        [Test]
        public void TheScroll_HidesTheFooterAndTheBarTogether()
        {
            var chrome = CardChromeRule.Decide(LampWithBar(SheetStopRule.Stop.Full, scrollHid: true));
            Assert.IsFalse(chrome.Bottom, "the area slid away");
            Assert.IsFalse(chrome.BottomNav, "with its bar");
            Assert.IsTrue(chrome.Top, "the strip stays");
            Assert.AreEqual(LampFull - LampHeader - LampStrip, CardChromeRule.Viewport(LampWithBar(SheetStopRule.Stop.Full, scrollHid: true), chrome), 0.001f,
                "both heights went to the stack");
            chrome = CardChromeRule.Decide(LampWithBar(SheetStopRule.Stop.Full));
            Assert.IsTrue(chrome.Bottom && chrome.BottomNav, "the scroll gave them back together");
        }

        // Show Navigation At applies to the NAVIGATION: Full Only hides the bar at Half, the footer (no navigation) stays; nothing at peek
        [Test]
        public void ShowNavigationAt_FullOnly_HidesTheBarAtHalf_TheFooterStays()
        {
            var input = LampWithBar(SheetStopRule.Stop.Half, CardOptions.ShowNavFullOnly);
            input.SheetHeight = LampFull;
            var chrome = CardChromeRule.Decide(input);
            Assert.IsTrue(chrome.Bottom, "the footer stays at Half");
            Assert.IsFalse(chrome.BottomNav, "the bar waits for Full");
            Assert.AreEqual(LampFull - LampHeader - LampFooter, CardChromeRule.Viewport(input, chrome), 0.001f, "the bar takes no room");
            input.Stop = SheetStopRule.Stop.Full;
            Assert.IsTrue(CardChromeRule.Decide(input).BottomNav, "Full Only: the bar at Full");
            input.Stop = SheetStopRule.Stop.Peek;
            chrome = CardChromeRule.Decide(input);
            Assert.IsFalse(chrome.Bottom || chrome.BottomNav, "nothing at peek");
            Assert.IsFalse(new CardChromeRule.Chrome(false, false, true).BottomNav, "a bar is never shown outside a shown area");
        }
    }
}
