using UnityEngine;

namespace TileStories
{
    // Tap outside vs double-tap zoom (_3.1 step 5b): ONE rule for a tap on empty camera space while the card is open.
    // A lone tap there closes the card; two quick taps there are the zoom's double tap (ARZoomGestureInput) and
    // must not close it. So a tap on nothing does not close at once: it waits out the zoom's own double-tap window
    // (zoom_settings.double_tap_window_s; 0 while zoom is off = close at once). A second tap inside that window
    // cancels the close -- judged by the SAME ARZoomMath.IsDoubleTapOK the zoom uses, so the two can never
    // disagree about what a double tap is. Any other tap (a marker, the card) and any selection change cancel it too.
    // Plain C#: PoiCardHost feeds it taps and the clock; it only says when to close.
    public sealed class TapOutsideDismissal
    {
        public enum TapOutcome { Nothing, ClosePending, DoubleTapCancelledClose }

        public bool IsPending { get; private set; }
        private float _tapTime;
        private Vector2 _tapPosition;

        // How long a tap on nothing waits before it closes: the zoom's double-tap window while zoom is on
        public static float DelayFor(ZoomSettings zoom) => zoom != null && zoom.enabled ? Mathf.Max(0f, zoom.double_tap_window_s) : 0f;

        // A released tap at `position` (screen pixels, `time` seconds). `closesCard` = CardTapRule.ShouldDismiss for
        // this tap on its own. `window` / `tolerancePx` are the zoom's double-tap settings.
        public TapOutcome OnTap(float time, Vector2 position, bool closesCard, float window, float tolerancePx)
        {
            if (IsPending)
            {
                bool secondOfDoubleTap = ARZoomMath.IsDoubleTapOK(time - _tapTime, position - _tapPosition, window, tolerancePx);
                IsPending = false;
                // - the zoom wins, and this second tap is spent: it is not a new lone tap on nothing
                if (secondOfDoubleTap) return TapOutcome.DoubleTapCancelledClose;
            }
            if (!closesCard) return TapOutcome.Nothing;
            IsPending = true;
            _tapTime = time;
            _tapPosition = position;
            return TapOutcome.ClosePending;
        }

        // Whether the pending close is due now (its window passed with no second tap). Taking it clears it.
        public bool TakeDue(float now, float delay)
        {
            if (!IsPending || now - _tapTime < delay) return false;
            IsPending = false;
            return true;
        }

        // Forget a pending close (the selection changed, the card closed another way)
        public void Cancel() => IsPending = false;
    }
}
