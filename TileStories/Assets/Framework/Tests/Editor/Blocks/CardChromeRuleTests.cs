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

        // ---------------- the Navigation Bar floating at the bottom (v5) ----------------

        [Test]
        public void AFloatingBottomBar_ShowsOverTheWholeHero_ItTakesNoRoom_WhileAnInFlowBarWaitsForTheHero()
        {
            // - at rest on the hero: the whole view is hero (it ends far below the view)
            var input = LampWithBar(SheetStopRule.Stop.Full);
            input.HasTop = false;
            input.HeroBottom = 2000f;
            input.ScrollOffset = 0f;
            var inFlow = CardChromeRule.Decide(input);
            Assert.IsFalse(inFlow.Bottom, "an in-flow bar would take room from the hero: the floor keeps it away");
            input.BottomNavFloats = true;
            var floating = CardChromeRule.Decide(input);
            Assert.IsTrue(floating.Bottom, "a floating bar takes no room: it lies on the hero from the start");
            Assert.IsTrue(floating.BottomNav);
            Assert.IsFalse(floating.Footer, "the hero still holds the actions: the sticky footer waits");
            Assert.AreEqual(CardChromeRule.BottomHeight(input, true, false), CardChromeRule.BottomHeight(input, true, true),
                "the floating bar adds nothing to the bottom area's height");
            input.BottomNavAfterHero = true;
            Assert.IsFalse(CardChromeRule.Decide(input).BottomNav, "Show After The Hero: it still waits for the hero");
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

        // ---------------- the resting end (_3.3 R10.8: Finished = the scroll has reached its end) ----------------

        // The Lamp with Next Up at Full, measured by PoiCardRestingEndSceneTests (phase before): the content once the waiting block is
        // revealed, the viewport with the bottom area away and back (the footer 81 + the bar 66 = 147)
        private const float EndContent = 19151f;
        private const float AwayViewport = 690f;
        private const float BackViewport = 543f;
        private const float EndArea = AwayViewport - BackViewport;
        // Where the area comes back: the end of the range with the area AWAY (147 short of the real one). Left there (before the fix) the
        // stack rested short under the area -- measured at true rest 125 short after the loop's last turn
        private static float OldRest => NavScrollRule.Range(EndContent, AwayViewport);
        private static float RealEnd => NavScrollRule.Range(EndContent, BackViewport);

        private static bool IsAtEnd(float offset, float viewport) => offset >= NavScrollRule.Range(EndContent, viewport) - NavScrollRule.EndTolerance;

        // The chrome's two viewports: the area comes back at the end and takes 147 px off the viewport -- the stack follows, so it rests at
        // the end of the range with the area shown (the one NavScrollRule reads); anywhere else, or a viewport that grew, nothing moves
        [Test]
        public void TheBottomAreaComingBackAtTheEnd_KeepsTheEnd_InTheViewportItLeaves()
        {
            Assert.AreEqual(18461f, OldRest, 0.01f, "precondition: the measured rest");
            Assert.IsFalse(IsAtEnd(OldRest, BackViewport), "precondition: left where it was, the stack rests 147 short of the end");
            float kept = CardChromeRule.KeepTheEnd(OldRest, EndContent, AwayViewport, BackViewport);
            Assert.AreEqual(RealEnd, kept, 0.01f, "kept at the end of the range with the area shown");
            Assert.AreEqual(EndArea, kept - OldRest, 0.01f, "moved by exactly the area's height");
            Assert.IsTrue(IsAtEnd(kept, BackViewport), "so the stack is at its end");
            Assert.AreEqual(RealEnd, CardChromeRule.KeepTheEnd(OldRest - 0.5f, EndContent, AwayViewport, BackViewport), 0.01f, "within the end's pixel of rounding");
            Assert.AreEqual(OldRest - 2f, CardChromeRule.KeepTheEnd(OldRest - 2f, EndContent, AwayViewport, BackViewport), "short of the end: the stack stays");
            Assert.AreEqual(9000f, CardChromeRule.KeepTheEnd(9000f, EndContent, AwayViewport, BackViewport), "mid-card: the stack stays");
            Assert.AreEqual(RealEnd, CardChromeRule.KeepTheEnd(RealEnd, EndContent, BackViewport, AwayViewport), "a viewport that grew: nothing to keep");
            Assert.AreEqual(0f, CardChromeRule.KeepTheEnd(0f, 500f, AwayViewport, BackViewport), "a stack that does not scroll stays at its top");
        }

        // The glide (_3.3 S2b): from the old rest to the kept end over the area's own duration and curve -- starts where it was, ends exactly
        // at the end, ease-out (more than half the way in the first third), never past the end; no duration (Reduce Motion) = the end at once
        [Test]
        public void TheKeptEnd_IsReachedByAGlide_OverTheAreasDuration_OrAtOnceWithNoDuration()
        {
            const float duration = 0.22f;
            Assert.AreEqual(OldRest, CardChromeRule.GlideOffset(OldRest, RealEnd, 0f, duration), 0.01f, "it starts where the stack was");
            Assert.AreEqual(RealEnd, CardChromeRule.GlideOffset(OldRest, RealEnd, duration, duration), "it ends exactly at the end");
            Assert.AreEqual(RealEnd, CardChromeRule.GlideOffset(OldRest, RealEnd, 5f, duration), "and stays there");
            float third = CardChromeRule.GlideOffset(OldRest, RealEnd, duration / 3f, duration);
            Assert.Greater(third - OldRest, EndArea * 0.5f, "decelerating: more than half the way in the first third (the bar's ease-out)");
            float last = OldRest;
            for (int i = 1; i <= 22; i++)
            {
                float now = CardChromeRule.GlideOffset(OldRest, RealEnd, i * 0.01f, duration);
                Assert.GreaterOrEqual(now, last, "it only moves towards the end (frame " + i + ")");
                Assert.LessOrEqual(now, RealEnd, "never past it");
                last = now;
            }
            Assert.AreEqual(RealEnd, CardChromeRule.GlideOffset(OldRest, RealEnd, 0f, 0f), "no duration: at once");
            Assert.AreEqual(RealEnd, CardChromeRule.GlideOffset(OldRest, RealEnd, float.NaN, duration), "an unknown time: the end");
        }

        // The end zone: with the area shown, "at the end" is the range WITHOUT it, so the area never hides within its own height of the end
        // -- before, a move down there hid it, the grown viewport clamped the stack back and the area came again: drags rested 147 short
        [Test]
        public void TheEnd_ForTheChrome_IsTheRangeWithoutTheArea_SoItNeverHidesWithinItsOwnHeightOfTheEnd()
        {
            Assert.IsTrue(CardChromeRule.AtEnd(RealEnd, EndContent, BackViewport, EndArea), "the real end");
            Assert.IsTrue(CardChromeRule.AtEnd(RealEnd - EndArea, EndContent, BackViewport, EndArea), "one area up from it: still the end zone");
            Assert.IsFalse(CardChromeRule.AtEnd(RealEnd - EndArea - 2f, EndContent, BackViewport, EndArea), "past the area's height: not the end");
            Assert.AreEqual(CardChromeRule.AtEnd(OldRest, EndContent, AwayViewport), CardChromeRule.AtEnd(OldRest, EndContent, BackViewport, EndArea),
                "the same point whether the area is away (its own viewport) or shown (its height given back)");
            // - a reader at the kept end moves up 50 and down 40: the area stays (the old end, read on the shown viewport, hid it)
            var state = CardChromeRule.Next(default, EndArea, false, true, CardOptions.HideOnScrollDown);
            float offset = RealEnd;
            foreach (float delta in new[] { -50f, 40f })
            {
                offset += delta;
                state = CardChromeRule.Next(state, delta, false, CardChromeRule.AtEnd(offset, EndContent, BackViewport, EndArea), CardOptions.HideOnScrollDown);
                Assert.IsFalse(state.BottomHidden, "moved " + delta + " to " + offset + ": the area stays");
            }
            Assert.IsTrue(CardChromeRule.Next(state, 40f, false, CardChromeRule.AtEnd(offset, EndContent, BackViewport), CardOptions.HideOnScrollDown).BottomHidden,
                "the old reading of the end (the shown viewport's range) hides it there");
        }

        // A SHORT last block (120 px, under The Lamp's real numbers): at the old rest it is still under the area -- not on screen, never seen,
        // its section not read; at the kept end it is seen and its section is the one read (NavScrollRule's own end rule)
        [Test]
        public void AtTheKeptEnd_AShortLastBlock_IsSeenAndItsSectionRead_WhereTheOldRestHidItUnderTheArea()
        {
            float top = EndContent - 120f;
            var sectionTops = new[] { 0f, 9000f, top };
            float kept = CardChromeRule.KeepTheEnd(OldRest, EndContent, AwayViewport, BackViewport);
            Assert.Greater(top, OldRest + BackViewport, "precondition: at the old rest the block starts below the viewport, under the area");
            Assert.IsFalse(NavScrollRule.HasViewed(true, top, EndContent, OldRest, EndContent, BackViewport), "old rest: never on screen, never seen");
            Assert.AreEqual(1, NavScrollRule.CurrentSection(sectionTops, OldRest, EndContent, BackViewport), "old rest: the section above is read");
            Assert.IsTrue(NavScrollRule.HasViewed(true, top, EndContent, kept, EndContent, BackViewport), "kept end: seen");
            Assert.AreEqual(2, NavScrollRule.CurrentSection(sectionTops, kept, EndContent, BackViewport), "kept end: its section is the one read");
        }

        // A TALL last block (951 px, taller than the 543 px viewport -- The Lamp's Sources): already on screen and past the spy line at the old
        // rest, but the scroll's END (R10.8: Finished, the reading of the card) is reached only at the kept end
        [Test]
        public void AtTheKeptEnd_ATallLastBlock_IsTheScrollsEnd_WhereTheOldRestWasNot()
        {
            float top = EndContent - 951f;
            Assert.Greater(951f, BackViewport, "precondition: the block is taller than the viewport");
            float kept = CardChromeRule.KeepTheEnd(OldRest, EndContent, AwayViewport, BackViewport);
            float range = NavScrollRule.Range(EndContent, BackViewport);
            Assert.IsTrue(NavScrollRule.HasViewed(true, top, EndContent, OldRest, EndContent, BackViewport), "precondition: its top passed the line at the old rest");
            Assert.IsFalse(ContentSeenRule.HasSeenAll(OldRest, range, BackViewport), "old rest: the scroll's end is not reached (its last 147 px under the area)");
            Assert.IsTrue(ContentSeenRule.HasSeenAll(kept, range, BackViewport), "kept end: the scroll's end");
            Assert.AreEqual(2, NavScrollRule.CurrentSection(new[] { 0f, 9000f, top }, kept, EndContent, BackViewport), "and its section read");
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
