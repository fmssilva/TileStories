using NUnit.Framework;

namespace TileStories.Editor.Tests
{
    // NavBackRule (_3.2 step 4, section 4.5): each system Back press undoes the innermost thing -- the full view closes first, whatever
    // the stop; else the sheet goes one stop down, Full -> Half -> Peek; at Peek the card closes. Back Button = none, or no card open,
    // leaves Back to the app; an unknown value reads as the default.
    public class NavBackRuleTests
    {
        private static readonly CardNavigationSettings StepDown = new() { back_button = CardOptions.BackButtonStepDown };
        private static readonly CardNavigationSettings None = new() { back_button = CardOptions.BackButtonNone };

        [Test]
        public void StepDown_WithNoFullView_LowersOneStopPerPress_AndClosesAtPeek()
        {
            Assert.AreEqual(NavBackRule.Step.LowerToHalf, NavBackRule.Next(StepDown, false, SheetStopRule.Stop.Full));
            Assert.AreEqual(NavBackRule.Step.LowerToPeek, NavBackRule.Next(StepDown, false, SheetStopRule.Stop.Half));
            Assert.AreEqual(NavBackRule.Step.CloseCard, NavBackRule.Next(StepDown, false, SheetStopRule.Stop.Peek), "at Peek Back closes, like the X");
        }

        [Test]
        public void AnOpenFullView_ClosesFirst_AtEveryStop()
        {
            foreach (var stop in new[] { SheetStopRule.Stop.Peek, SheetStopRule.Stop.Half, SheetStopRule.Stop.Full })
                Assert.AreEqual(NavBackRule.Step.CloseFullView, NavBackRule.Next(StepDown, true, stop), stop + ": the innermost thing is the full view");
        }

        [Test]
        public void TheWholeOrder_FromAFullViewOverAFullCard_TakesFourPresses()
        {
            // - the walk the Phase B test makes with real Escape presses: full view open over Full -> ... -> closed
            bool fullView = true;
            var stop = SheetStopRule.Stop.Full;
            var walked = new System.Collections.Generic.List<NavBackRule.Step>();
            for (int press = 0; press < 6 && stop != SheetStopRule.Stop.Dismissed; press++)
            {
                var step = NavBackRule.Next(StepDown, fullView, stop);
                walked.Add(step);
                if (step == NavBackRule.Step.CloseFullView) fullView = false;
                else if (step == NavBackRule.Step.LowerToHalf) stop = SheetStopRule.Stop.Half;
                else if (step == NavBackRule.Step.LowerToPeek) stop = SheetStopRule.Stop.Peek;
                else if (step == NavBackRule.Step.CloseCard) stop = SheetStopRule.Stop.Dismissed;
            }
            CollectionAssert.AreEqual(new[] { NavBackRule.Step.CloseFullView, NavBackRule.Step.LowerToHalf, NavBackRule.Step.LowerToPeek, NavBackRule.Step.CloseCard }, walked);
            Assert.AreEqual(NavBackRule.Step.LeaveToApp, NavBackRule.Next(StepDown, false, stop), "a fifth press: no card left, Back is the app's");
        }

        [Test]
        public void BackButtonNone_LeavesEveryPressToTheApp()
        {
            foreach (var stop in new[] { SheetStopRule.Stop.Peek, SheetStopRule.Stop.Half, SheetStopRule.Stop.Full })
            {
                Assert.AreEqual(NavBackRule.Step.LeaveToApp, NavBackRule.Next(None, false, stop), stop.ToString());
                Assert.AreEqual(NavBackRule.Step.LeaveToApp, NavBackRule.Next(None, true, stop), stop + " with the full view open");
            }
            Assert.IsFalse(NavBackRule.StepsDown(None));
        }

        [Test]
        public void TheDefault_AnUnknownValue_AndNoSettings_StepDown_AndAClosedCardNeverTakesBack()
        {
            Assert.AreEqual(CardOptions.BackButtonStepDown, new CardNavigationSettings().back_button, "the default");
            Assert.IsTrue(NavBackRule.StepsDown(new CardNavigationSettings { back_button = "typo" }), "an unknown value reads as the default");
            Assert.IsTrue(NavBackRule.StepsDown(null), "no settings (a caller with none): the default");
            Assert.AreEqual(NavBackRule.Step.LowerToHalf, NavBackRule.Next(null, false, SheetStopRule.Stop.Full));
            Assert.AreEqual(NavBackRule.Step.LeaveToApp, NavBackRule.Next(StepDown, false, SheetStopRule.Stop.Dismissed), "no card: Back is the app's");
            Assert.AreEqual(NavBackRule.Step.LeaveToApp, NavBackRule.Next(StepDown, true, SheetStopRule.Stop.Dismissed));
            CollectionAssert.AreEqual(new[] { CardOptions.BackButtonStepDown, CardOptions.BackButtonNone }, CardOptions.BackButtons);
            Assert.AreEqual(CardOptions.BackButtons.Length, CardOptions.BackButtonLabels.Length, "one label per option");
        }
    }
}
