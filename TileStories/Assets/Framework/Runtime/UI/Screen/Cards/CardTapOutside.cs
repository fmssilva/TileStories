using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TileStories
{
    // A tap on empty camera space closes the card (_3.1 section 4), kept out of PoiCardHost (15.4.2): it remembers a press -- and whether
    // anything was under it THEN --, judges the release (CardTapRule: short and still, on nothing at either end -- ScreenUIHit), and while
    // zoom is on waits out the zoom's double-tap window (TapOutsideDismissal) so a double tap zooms instead. Plain C#: the host feeds it the
    // pointer every frame; a test calls HandleScreenTap with real positions and times on its own clock.
    //   - why the press is judged at the press: a button can hide what it sits on when it is clicked (a block page's Back closes the full
    //     view on the release), so at the release the same point may show the camera; the tap was the button's all the same
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
        private bool _pressedOnSomething;

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
                _pressedOnSomething = ScreenUIHit.IsOverAnything(_pressPosition);
            }
            else if (_pressed && pointer.press.wasReleasedThisFrame)
            {
                _pressed = false;
                HandleScreenTap(_pressPosition, pointer.position.ReadValue(), _pressTime, _clock(), _pressedOnSomething);
            }
        }

        // A press at `down` released at `up` (screen pixels, times on the clock). A tap on empty camera space closes the card -- at once while
        // zoom is off, else once the zoom's double-tap window passes with no second tap (IsPending meanwhile). True when it closed NOW.
        // What was under the press is read now (the screen as it is), as a caller holding no earlier reading sees it
        public bool HandleScreenTap(Vector2 down, Vector2 up, float downTime, float upTime) =>
            HandleScreenTap(down, up, downTime, upTime, ScreenUIHit.IsOverAnything(down));

        // The same, with whether anything was under the press when it landed (the poll's own reading, taken at the press)
        public bool HandleScreenTap(Vector2 down, Vector2 up, float downTime, float upTime, bool pressedOnSomething)
        {
            var zoom = _zoom();
            bool isTap = CardTapRule.IsTap(down, up, downTime, upTime, Screen.height);
            bool closes = CardTapRule.ShouldDismiss(_cardOpen(), _settings()?.container.dismiss_on_tap_outside ?? true,
                isTap, isTap && (pressedOnSomething || ScreenUIHit.IsOverAnything(up)));
            float window = TapOutsideDismissal.DelayFor(zoom);
            _dismissal.OnTap(upTime, up, closes, window, zoom?.double_tap_move_tolerance_px ?? 0f);
            // - zoom off: no second tap can make it a double tap, so there is nothing to wait for
            if (!_dismissal.TakeDue(upTime, window)) return false;
            _close();
            return true;
        }
    }
}
