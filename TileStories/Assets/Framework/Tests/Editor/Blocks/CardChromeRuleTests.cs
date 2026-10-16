using NUnit.Framework;

namespace TileStories.Editor.Tests
{
    // CardChromeRule (_3.2 step 5b, section 4.3): what chrome the card shows at each stop, how the scroll hides and brings back the bottom
    // area (the two travel thresholds, a turn, the top and the end), and THE FLOOR -- no chrome where it would leave the stack less than a
    // heading and two lines, the bottom area giving way first -- at The Lamp's measured numbers and just above them.
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

        private static CardChromeRule.Input Lamp(SheetStopRule.Stop stop, string showAt = CardOptions.ShowNavHalfAndFull, bool scrollHid = false) => new()
        {
            Stop = stop, ShowAt = showAt, HasTop = true, HasBottom = true, ScrollHidesBottom = scrollHid,
            SheetHeight = stop == SheetStopRule.Stop.Full ? LampFull : LampHalf, HeaderHeight = LampHeader,
            TopHeight = LampStrip, BottomHeight = LampFooter, ContentHeight = LampContent,
        };

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
            input.HasBottom = false;
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
        public void TheFloor_AtTheLampsHalf_ShowsNoChrome_AndGivesTheStackItsWholeRoom()
        {
            Assert.AreEqual(72f, CardChromeRule.MinStackViewport);
            var input = Lamp(SheetStopRule.Stop.Half);
            Assert.Less(LampHalf - LampHeader - LampStrip - LampFooter, 0f, "precondition: strip + footer do not even fit (the 0 px Half of 5a)");
            Assert.Less(LampHalf - LampHeader - LampStrip, CardChromeRule.MinStackViewport, "precondition: the strip alone leaves less than the floor");
            var chrome = CardChromeRule.Decide(input);
            Assert.IsFalse(chrome.Bottom, "the footer gives way first");
            Assert.IsFalse(chrome.Top, "then the strip");
            Assert.AreEqual(LampHalf - LampHeader, CardChromeRule.Viewport(input, chrome), 0.001f, "the stack gets the whole 106.6 px");

            var full = CardChromeRule.Decide(Lamp(SheetStopRule.Stop.Full));
            Assert.IsTrue(full.Top && full.Bottom, "Full has room for both");
        }

        [Test]
        public void TheFloor_JustAboveTheLampsNumbers_ShowsTheStripFirst_ThenTheFooterToo()
        {
            var input = Lamp(SheetStopRule.Stop.Half);
            float floor = CardChromeRule.MinStackViewport;

            input.SheetHeight = LampHeader + LampStrip + floor - 0.5f;
            var chrome = CardChromeRule.Decide(input);
            Assert.IsFalse(chrome.Top || chrome.Bottom, "half a pixel short of strip + floor: neither");

            input.SheetHeight = LampHeader + LampStrip + floor;
            chrome = CardChromeRule.Decide(input);
            Assert.IsTrue(chrome.Top, "room for the strip and the floor: the strip");
            Assert.IsFalse(chrome.Bottom, "but not the footer as well");
            Assert.AreEqual(floor, CardChromeRule.Viewport(input, chrome), 0.001f, "the stack keeps exactly the floor");

            input.SheetHeight = LampHeader + LampStrip + LampFooter + floor - 0.5f;
            Assert.IsFalse(CardChromeRule.Decide(input).Bottom, "half a pixel short of both: still no footer");
            input.SheetHeight = LampHeader + LampStrip + LampFooter + floor;
            chrome = CardChromeRule.Decide(input);
            Assert.IsTrue(chrome.Top && chrome.Bottom, "room for both and the floor: both");
        }

        [Test]
        public void TheFloor_IsTheSameForTheBottomArea_WithNoStrip()
        {
            // - the bottom bar of step 7 gets the floor for free: it is measured as the bottom area, whatever fills it
            var input = Lamp(SheetStopRule.Stop.Half);
            input.HasTop = false;
            input.SheetHeight = LampHeader + LampFooter + CardChromeRule.MinStackViewport;
            Assert.IsTrue(CardChromeRule.Decide(input).Bottom, "room for the footer and the floor");
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
            input.ContentHeight = 40f;
            var chrome = CardChromeRule.Decide(input);
            Assert.IsTrue(chrome.Top, "57.6 px left after the strip holds the whole 40 px stack");
            Assert.IsFalse(chrome.Bottom, "the footer as well would leave none of it");

            input.ContentHeight = float.NaN;
            Assert.IsFalse(CardChromeRule.Decide(input).Top, "content not measured yet: the full floor");

            input.SheetHeight = float.NaN;
            var unmeasured = CardChromeRule.Decide(input);
            Assert.IsTrue(unmeasured.Top && unmeasured.Bottom, "a sheet not laid out yet: no floor to apply, the stop decides");
        }
    }
}
