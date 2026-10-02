using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TileStories
{
    // A tap on empty camera space closes the card (_3.1 section 4), kept out of PoiCardHost (15.4.2): it remembers a press, judges the
    // release (CardTapRule: short and still, on nothing -- ScreenUIHit), and while zoom is on waits out the zoom's double-tap window
    // (TapOutsideDismissal) so a double tap zooms instead. Plain C#: the host feeds it the pointer every frame; a test calls HandleScreenTap
    // with real positions and times on its own clock.
    public sealed class CardTapOutside
    {
        private readonly TapOutsideDismissal _dismissal = new();
        private readonly Func<float> _clock;
        private readonly Func<CardSettings> _settings;
        private readonly Func<ZoomSettings> _zoom;
        private readonly Func<bool> _cardOpen;
        private readonly Action _close;
        private Vector2 _pressPosition;
        private float _pressTime;
        private bool _pressed;

        // The clock taps are timed on, the wall's card and zoom settings (Tap Outside Closes, the double-tap window), whether the card is open,
        // and what closing it means (the host clears the selection through the bus)
        public CardTapOutside(Func<float> clock, Func<CardSettings> settings, Func<ZoomSettings> zoom, Func<bool> cardOpen, Action close)
        {
            _clock = clock;
            _settings = settings;
            _zoom = zoom;
            _cardOpen = cardOpen;
            _close = close;
        }

        // A tap on nothing is waiting out the double-tap window
        public bool IsPending => _dismissal.IsPending;

        // Forget a waiting close (another tap, a selection change, the card closing)
        public void Cancel() => _dismissal.Cancel();

        // One frame: a waiting close that fell due closes the card; a press is remembered; its release is judged as a tap
        public void Poll(Pointer pointer)
        {
            if (_dismissal.TakeDue(_clock(), TapOutsideDismissal.DelayFor(_zoom()))) _close();
            if (pointer == null) return;
            if (pointer.press.wasPressedThisFrame)
            {
                _pressed = true;
                _pressPosition = pointer.position.ReadValue();
                _pressTime = _clock();
            }
            else if (_pressed && pointer.press.wasReleasedThisFrame)
            {
                _pressed = false;
                HandleScreenTap(_pressPosition, pointer.position.ReadValue(), _pressTime, _clock());
            }
        }

        // A press at `down` released at `up` (screen pixels, times on the clock). A tap on empty camera space closes the card -- at once while
        // zoom is off, else once the zoom's double-tap window passes with no second tap (IsPending meanwhile). True when it closed NOW.
        public bool HandleScreenTap(Vector2 down, Vector2 up, float downTime, float upTime)
        {
            var zoom = _zoom();
            bool isTap = CardTapRule.IsTap(down, up, downTime, upTime, Screen.height);
            bool closes = CardTapRule.ShouldDismiss(_cardOpen(), _settings()?.container.dismiss_on_tap_outside ?? true,
                isTap, isTap && ScreenUIHit.IsOverAnything(up));
            float window = TapOutsideDismissal.DelayFor(zoom);
            _dismissal.OnTap(upTime, up, closes, window, zoom?.double_tap_move_tolerance_px ?? 0f);
            // - zoom off: no second tap can make it a double tap, so there is nothing to wait for
            if (!_dismissal.TakeDue(upTime, window)) return false;
            _close();
            return true;
        }
    }
}
