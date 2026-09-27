using NUnit.Framework;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // Tap outside vs double-tap zoom (_3.1 step 5b), the pure rule: a tap on empty space closes the card only after
    // the zoom's double-tap window, a second tap inside it (ARZoomMath.IsDoubleTapOK -- the zoom's own judge) keeps
    // the card open, anything else cancels. Times and tolerances come from a real ZoomSettings.
    public class TapOutsideDismissalTests
    {
        private static readonly ZoomSettings Zoom = new() { enabled = true, double_tap_window_s = 0.3f, double_tap_move_tolerance_px = 50f };
        private static float Window => TapOutsideDismissal.DelayFor(Zoom);
        private static float Tolerance => Zoom.double_tap_move_tolerance_px;

        [Test]
        public void TheWait_IsTheZoomsDoubleTapWindow_AndNothingWhileZoomIsOff()
        {
            Assert.AreEqual(0.3f, TapOutsideDismissal.DelayFor(Zoom), 1e-6f);
            Assert.AreEqual(0f, TapOutsideDismissal.DelayFor(new ZoomSettings { enabled = false, double_tap_window_s = 0.3f }), "zoom off: no double tap exists");
            Assert.AreEqual(0f, TapOutsideDismissal.DelayFor(null));
        }

        [Test]
        public void ALoneTapOnNothing_ClosesOnlyOnceTheWindowHasPassed()
        {
            var rule = new TapOutsideDismissal();
            Assert.AreEqual(TapOutsideDismissal.TapOutcome.ClosePending, rule.OnTap(10f, new Vector2(300, 400), true, Window, Tolerance));
            Assert.IsFalse(rule.TakeDue(10.29f, Window), "still inside the window: a second tap may come");
            Assert.IsTrue(rule.TakeDue(10.31f, Window), "the window passed with no second tap: close");
            Assert.IsFalse(rule.IsPending, "taken once");
            Assert.IsFalse(rule.TakeDue(11f, Window), "never twice");
        }

        [Test]
        public void ASecondTapCloseBy_InsideTheWindow_IsTheZoomsDoubleTap_AndTheCardStays()
        {
            var rule = new TapOutsideDismissal();
            rule.OnTap(10f, new Vector2(300, 400), true, Window, Tolerance);
            var outcome = rule.OnTap(10.2f, new Vector2(320, 410), true, Window, Tolerance);
            Assert.AreEqual(TapOutsideDismissal.TapOutcome.DoubleTapCancelledClose, outcome);
            Assert.IsTrue(ARZoomMath.IsDoubleTapOK(0.2f, new Vector2(20, 10), Zoom.double_tap_window_s, Tolerance), "the zoom judges the same pair a double tap");
            Assert.IsFalse(rule.IsPending, "the second tap is spent on the zoom, it is not a new lone tap");
            Assert.IsFalse(rule.TakeDue(20f, Window), "the card never closes for it");
        }

        [Test]
        public void AThirdTap_IsALoneTapAgain_LikeTheZoomWhichNeverFiresTwiceOnATripleTap()
        {
            var rule = new TapOutsideDismissal();
            rule.OnTap(10f, Vector2.zero, true, Window, Tolerance);
            rule.OnTap(10.2f, Vector2.zero, true, Window, Tolerance);
            Assert.AreEqual(TapOutsideDismissal.TapOutcome.ClosePending, rule.OnTap(10.35f, Vector2.zero, true, Window, Tolerance));
        }

        [Test]
        public void ASecondTapFarAway_IsNotADoubleTap_ItIsANewLoneTap()
        {
            var rule = new TapOutsideDismissal();
            rule.OnTap(10f, new Vector2(100, 100), true, Window, Tolerance);
            Assert.AreEqual(TapOutsideDismissal.TapOutcome.ClosePending, rule.OnTap(10.1f, new Vector2(600, 100), true, Window, Tolerance));
            Assert.IsFalse(rule.TakeDue(10.35f, Window), "the wait restarts at the new tap");
            Assert.IsTrue(rule.TakeDue(10.41f, Window));
        }

        [Test]
        public void ATapOnAMarkerOrTheCard_InsideTheWindow_CancelsTheClose()
        {
            var rule = new TapOutsideDismissal();
            rule.OnTap(10f, new Vector2(100, 100), true, Window, Tolerance);
            // - far from the first tap, on something (closesCard = false): the marker selects, the old close must not fire later
            Assert.AreEqual(TapOutsideDismissal.TapOutcome.Nothing, rule.OnTap(10.1f, new Vector2(500, 500), false, Window, Tolerance));
            Assert.IsFalse(rule.TakeDue(11f, Window));
        }

        [Test]
        public void ASelectionChange_CancelsTheClose()
        {
            var rule = new TapOutsideDismissal();
            rule.OnTap(10f, Vector2.zero, true, Window, Tolerance);
            rule.Cancel();
            Assert.IsFalse(rule.TakeDue(11f, Window));
        }

        [Test]
        public void ZoomOff_ATapOnNothingIsDueAtOnce_AndTwoQuickTapsAreTwoLoneTaps()
        {
            var off = new ZoomSettings { enabled = false, double_tap_window_s = 0.3f, double_tap_move_tolerance_px = 50f };
            float window = TapOutsideDismissal.DelayFor(off);
            var rule = new TapOutsideDismissal();
            rule.OnTap(10f, Vector2.zero, true, window, off.double_tap_move_tolerance_px);
            Assert.IsTrue(rule.TakeDue(10f, window), "no double tap to wait for");
            Assert.AreEqual(TapOutsideDismissal.TapOutcome.ClosePending, rule.OnTap(10.1f, Vector2.zero, true, window, off.double_tap_move_tolerance_px),
                "with zoom off a second quick tap is never a double tap");
        }

        [Test]
        public void TheCardsTapRule_AndTheZoomsTapRule_AgreeOnATypicalFingerTap()
        {
            // - a 0.1 s press that wobbles 5 px: the card calls it a tap and the zoom accepts it as one half of a double tap
            Assert.IsTrue(CardTapRule.IsTap(new Vector2(300, 400), new Vector2(303, 404), 0f, 0.1f, 1080f));
            Assert.LessOrEqual(0.1f, Zoom.double_tap_window_s);
            Assert.LessOrEqual(5f, Zoom.double_tap_move_tolerance_px);
        }
    }
}
