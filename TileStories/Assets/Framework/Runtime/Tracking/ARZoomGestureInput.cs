using UnityEngine;
using EnhancedTouch = UnityEngine.InputSystem.EnhancedTouch;
using InputTouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace TileStories
{
    // Recognizes pinch and double-tap zoom gestures from real touch input and
    // forwards them to ARZoomController.OnPinch / ARZoomController.OnDoubleTap.
    // This is the "third layer" of spec section 9's three zoom mechanisms
    // (continuous pinch, double-tap step-cycle, and the on-screen UI buttons).
    //
    // Detection lives HERE, separate from ARZoomController, matching that class's
    // own comment ("a thin Input-System action binding call"). The actual touch
    // recognition is inherently real-device input plumbing (finger phase, timing);
    // only the pure time/space math (ARZoomMath.IsDoubleTapOK / PinchScaleForDelta)
    // is Tier-0 unit-tested. Recognition itself is verified on a human device in
    // Tier 2 (see _2.7 2.4-m: this domain has no Phase B -- no non-device pipeline).
    //
    // Uses the Enhanced Touch API of the new Input System. Verified in this project:
    // ProjectSettings.asset activeInputHandler = 1 (new Input System only), so the
    // legacy Input.touchCount API is disabled project-wide here.
    //
    // Place this alongside ARZoomController on the same GameObject (same pattern as
    // WallSession + LODController sharing one object).
    [DisallowMultipleComponent]
    public sealed class ARZoomGestureInput : MonoBehaviour
    {
        [Header("Drivers")]
        [Tooltip("Controller whose OnPinch / OnDoubleTap the gestures invoke.")]
        [SerializeField] private ARZoomController _zoom;

        [Tooltip("WallSession whose ZoomSettings supplies the double-tap window/tolerance.")]
        [SerializeField] private WallSession _wallSession;

        // Pinch state: last two-finger distance (negative = no pinch in flight).
        private float _lastPinchDistance = -1f;

        // Double-tap candidate memory.
        private float _lastTapTime = -1f;
        private Vector2 _lastTapPos;

        private ZoomSettings Settings => _wallSession != null ? _wallSession.ZoomSettings : null;

        private void OnEnable() => EnhancedTouch.EnhancedTouchSupport.Enable();
        private void OnDisable() => EnhancedTouch.EnhancedTouchSupport.Disable();

        private void Update()
        {
            var settings = Settings;
            if (settings == null || !settings.enabled || _zoom == null)
                return;

            HandlePinch();
            HandleDoubleTap(settings);
        }

        // Continuous two-finger pinch. Uses the two most distant active touch points
        // each frame and feeds the frame-to-frame distance ratio into ARZoomController.
        private void HandlePinch()
        {
            Vector2? a = null, b = null;
            foreach (var t in EnhancedTouch.Touch.activeTouches)
            {
                if (t.phase == InputTouchPhase.Canceled || t.phase == InputTouchPhase.Ended) continue;
                if (!a.HasValue) a = t.screenPosition;
                else if (!b.HasValue && Vector2.Distance(a.Value, t.screenPosition) > 1f) b = t.screenPosition;
                if (a.HasValue && b.HasValue) break;
            }

            float dist = (a.HasValue && b.HasValue) ? Vector2.Distance(a.Value, b.Value) : 0f;
            if (dist <= 0f)
            {
                _lastPinchDistance = -1f; // pinch ended / not yet started
                return;
            }

            // First frame of a pinch: _lastPinchDistance is -1 -> PinchScaleForDelta
            // returns 1 (no abrupt jump).
            float scale = ARZoomMath.PinchScaleForDelta(_lastPinchDistance, dist);
            _lastPinchDistance = dist;
            _zoom.OnPinch(scale);
        }

        // Detect a double tap (two quick, close taps) and route it as a zoom step.
        // A single tap routes nothing -- select-type interactions are owned by
        // MarkerSelectable (Domain 2.6), not here.
        private void HandleDoubleTap(ZoomSettings settings)
        {
            float window = settings.double_tap_window_s;
            float tolerance = settings.double_tap_move_tolerance_px;

            foreach (var t in EnhancedTouch.Touch.activeTouches)
            {
                if (t.phase != InputTouchPhase.Ended) continue;

                // A tap requires the touch to be short-lived and confined to a small
                // area -- a long hold or a swipe is a drag, not a tap. Touch carries
                // its true start position/time, so no external finger bookkeeping.
                float duration = (float)(t.time - t.startTime);
                Vector2 delta = t.screenPosition - t.startScreenPosition;
                if (duration > window || delta.magnitude > tolerance)
                    continue;

                // If a previous valid tap is remembered, check this new tap against it.
                float timeSinceLast = _lastTapTime < 0f
                    ? float.MaxValue
                    : (float)(t.time - _lastTapTime);
                Vector2 tapDelta = _lastTapTime < 0f
                    ? Vector2.positiveInfinity
                    : t.screenPosition - _lastTapPos;

                if (ARZoomMath.IsDoubleTapOK(timeSinceLast, tapDelta, window, tolerance))
                {
                    _zoom.OnDoubleTap();
                    _lastTapTime = -1f; // consume: triple-tap must not re-fire
                    _lastTapPos = Vector2.zero;
                }
                else
                {
                    _lastTapTime = (float)t.time;
                    _lastTapPos = t.screenPosition;
                }
            }
        }
    }
}