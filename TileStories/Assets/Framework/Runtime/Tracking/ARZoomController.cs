using UnityEngine;

namespace TileStories
{
    // Tier-0-pure zoom behavior math for the AR camera zoom feature (spec section 9).
    // One system writes ARZoomState (this controller), all others read it
    // (LODController via ARZoomState.ZoomFactor -> effective distance, section 10).
    // Keeping the math static and device-free makes it assertable without a scene.
    //
    // Assumption (documented): the tap cycle is MULTIPLICATIVE per zoom_tap_step.
    // Levels = [1.0, step, step^2, ...] clamped to [zoom_min, zoom_max], base first.
    // The tap past the last level wraps back to 1.0 (base) -- exactly the
    // "3rd tap/click returns to 1x" contract baked into WallConfigData zoom_tap_levels.
    public static class ARZoomMath
    {
        // Discrete tap levels for the tap-button cycle. Base (1.0) is always first;
        // then `tapLevels` multiplied copies of step, clamped, collapsed to a
        // strictly ascending, distinct, base-first sequence.
        // Example: step=1.5, tapLevels=2, min=1, max=4 -> [1.0, 1.5, 2.25].
        public static float[] ComputeTapLevels(float step, int tapLevels, float min, float max)
        {
            if (tapLevels < 0) tapLevels = 0;
            if (step <= 0f) step = 1f; // guard: non-positive step must not produce NaN/inf

            var levels = new System.Collections.Generic.List<float> { 1.0f };
            float acc = 1.0f;
            for (int i = 1; i <= tapLevels; i++)
            {
                acc *= step;
                float clamped = Mathf.Clamp(acc, min, max);
                // keep strictly ascending and distinct from the previous level
                if (clamped - levels[levels.Count - 1] > 1e-5f)
                    levels.Add(clamped);
            }
            return levels.ToArray();
        }

        // Advance one discrete level. With wrap=true (single tap) it cycles past the
        // last level back to base; with wrap=false (+ button) it caps at the top level.
        public static float NextTapLevel(float currentZoom, float step, int tapLevels, float min, float max, bool wrap = true)
        {
            float[] levels = ComputeTapLevels(step, tapLevels, min, max);
            if (levels.Length == 0) return Mathf.Clamp(currentZoom, min, max);

            // snap current to the nearest known level to avoid drift accumulation
            int bestIdx = 0;
            float bestDelta = Mathf.Abs(levels[0] - currentZoom);
            for (int i = 1; i < levels.Length; i++)
            {
                float d = Mathf.Abs(levels[i] - currentZoom);
                if (d < bestDelta) { bestDelta = d; bestIdx = i; }
            }
            int next = bestIdx + 1;
            if (next >= levels.Length)
            {
                // reached the top: wrap back to base (tap), or hold at max (button)
                return wrap ? levels[0] : levels[levels.Length - 1];
            }
            return levels[next];
        }

        // Retreat one discrete level for the zoom-out button. Never wraps -- it floors
        // at the base level so repeated presses stay at 1x instead of snapping home.
        public static float PreviousTapLevel(float currentZoom, float step, int tapLevels, float min, float max)
        {
            float[] levels = ComputeTapLevels(step, tapLevels, min, max);
            if (levels.Length == 0) return Mathf.Clamp(currentZoom, min, max);

            // snap current to the nearest known level to avoid drift accumulation
            int bestIdx = 0;
            float bestDelta = Mathf.Abs(levels[0] - currentZoom);
            for (int i = 1; i < levels.Length; i++)
            {
                float d = Mathf.Abs(levels[i] - currentZoom);
                if (d < bestDelta) { bestDelta = d; bestIdx = i; }
            }
            int prev = bestIdx - 1;
            if (prev < 0) prev = 0; // floor at base
            return levels[prev];
        }

        // Double-tap toggles between base (1.0) and max. Assumption: "base" == min.
        public static float NextDoubleTapTarget(float currentZoom, float min, float max)
        {
            bool nearBase = Mathf.Abs(currentZoom - 1.0f) < 1e-3f;
            return nearBase ? max : 1.0f;
        }

        // Pinch: multiply current zoom factor, then clamp to [min, max].
        public static float ApplyPinchScale(float currentZoom, float scaleFactor, float min, float max)
        {
            return Mathf.Clamp(currentZoom * scaleFactor, min, max);
        }

        // Frame-step toward a target. Reaches the target in roughly
        // transitionSpeedSeconds at low framerate; higher framerate approaches
        // asymptotically with no overshoot. Clamped to the target to stop jitter.
        public static float StepTowardTarget(float from, float to, float transitionSpeedSeconds, float deltaTime)
        {
            if (Mathf.Approximately(from, to)) return to;
            if (transitionSpeedSeconds <= 0f) return to; // instant
            float t = Mathf.Clamp01(deltaTime / transitionSpeedSeconds);
            float next = Mathf.Lerp(from, to, t);
            if (Mathf.Abs(next - to) < 1e-4f) return to;
            return next;
        }
    }

    // Input writer for ARZoomState (spec section 9). Reads zoom_* config from
    // WallSession.LodSettings and drives ARZoomState via ARZoomState.SetZoom,
    // the only place the global zoom factor is mutated.
    //
    // Gesture DETECTION (pinch/double-tap input parsing) is intentionally NOT
    // embedded here: real-device, non-deterministic input plumbing with no
    // Tier-0/0.5 surface. Instead this class exposes pure-behavior entry points
    // (ZoomIn/ZoomOut/OnTap/OnDoubleTap/OnPinch) that UI buttons (1d) and a
    // thin Input-System action binding call. Behavior math is covered by
    // ARZoomMathTests. Mirrors LODController's WallSession access pattern.
    public class ARZoomController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private WallSession _wallSession;

        [Header("Debug")]
        [SerializeField] private bool _debug = false;

        private float _targetZoom;      // where the animation is heading
        private bool _animating;

        private LodSettings Settings => _wallSession != null ? _wallSession.LodSettings : null;
        private LodSettings _lastSettings;
        private float _lastTransitionSpeed;

        private void Awake()
        {
            if (_wallSession == null)
                _wallSession = GetComponent<WallSession>();
        }

        private void Update()
        {
            var settings = Settings;
            if (settings == null || !settings.zoom_enabled)
            {
                _animating = false;
                return;
            }

            if (settings != _lastSettings)
            {
                _lastSettings = settings;
                _lastTransitionSpeed = settings.zoom_transition_speed_s;
            }

            // Drive the smooth transition toward the pending target each frame.
            // ARZoomState.SetZoom writes (and hard-clamps) the global factor that
            // LODController divides into effective distance.
            if (_animating)
            {
                float from = ARZoomState.ZoomFactor;
                float next = ARZoomMath.StepTowardTarget(from, _targetZoom, _lastTransitionSpeed, Time.unscaledDeltaTime);
                ARZoomState.SetZoom(next, settings.zoom_min, settings.zoom_max);
                _animating = Mathf.Abs(next - _targetZoom) > 1e-4f;
            }
        }

        // Snap to a target zoom immediately (used by tests / direct API callers).
        public void SetZoomImmediate(float targetZoomFactor)
        {
            var settings = Settings;
            if (settings == null || !settings.zoom_enabled) return;
            _targetZoom = Mathf.Clamp(targetZoomFactor, settings.zoom_min, settings.zoom_max);
            ARZoomState.SetZoom(_targetZoom, settings.zoom_min, settings.zoom_max);
            _animating = false;
            if (_debug) Debug.Log($"[ARZoom] immediate -> {_targetZoom:F2}");
        }

        public void SetZoomAnimated(float targetZoomFactor)
        {
            var settings = Settings;
            if (settings == null || !settings.zoom_enabled) return;
            _targetZoom = Mathf.Clamp(targetZoomFactor, settings.zoom_min, settings.zoom_max);
            _animating = true;
            if (_debug) Debug.Log($"[ARZoom] animated -> {_targetZoom:F2}");
        }

        // --- entry points invoked by UI buttons / input bindings ---

        public void ZoomIn()  => StepLevel(true);
        public void ZoomOut() => StepLevel(false);

        private void StepLevel(bool up)
        {
            var settings = Settings;
            if (settings == null || !settings.zoom_enabled) return;
            float current = ARZoomState.ZoomFactor;
            // + button caps at max (no wrap); - button retreats one level, floor at base.
            float target = up
                ? ARZoomMath.NextTapLevel(current, settings.zoom_tap_step, settings.zoom_tap_levels, settings.zoom_min, settings.zoom_max, wrap: false)
                : ARZoomMath.PreviousTapLevel(current, settings.zoom_tap_step, settings.zoom_tap_levels, settings.zoom_min, settings.zoom_max);
            SetZoomAnimated(target);
        }

        public void OnTap()
        {
            var settings = Settings;
            if (settings == null || !settings.zoom_enabled) return;
            float target = ARZoomMath.NextTapLevel(ARZoomState.ZoomFactor, settings.zoom_tap_step, settings.zoom_tap_levels, settings.zoom_min, settings.zoom_max);
            SetZoomAnimated(target);
        }

        public void OnDoubleTap()
        {
            var settings = Settings;
            if (settings == null || !settings.zoom_enabled) return;
            float target = ARZoomMath.NextDoubleTapTarget(ARZoomState.ZoomFactor, settings.zoom_min, settings.zoom_max);
            SetZoomAnimated(target);
        }

        // scaleFactor is the multiplicative finger-scale delta (>1 zooms in).
        public void OnPinch(float scaleFactor)
        {
            var settings = Settings;
            if (settings == null || !settings.zoom_enabled) return;
            float current = ARZoomState.ZoomFactor;
            float target = ARZoomMath.ApplyPinchScale(current, scaleFactor, settings.zoom_min, settings.zoom_max);
            SetZoomImmediate(target);
        }

        // "Fit to 1x": reset button handler.
        public void ResetToBase()
        {
            var settings = Settings;
            if (settings == null || !settings.zoom_enabled) return;
            ARZoomState.ResetToBase(settings.zoom_min, settings.zoom_max);
            _targetZoom = ARZoomState.ZoomFactor;
            _animating = false;
            if (_debug) Debug.Log("[ARZoom] reset to base");
        }
    }
}
