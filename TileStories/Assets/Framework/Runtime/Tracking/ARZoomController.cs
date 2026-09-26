using UnityEngine;

namespace TileStories
{
    // Tier-0-pure zoom behavior math for the AR camera zoom feature (spec section 9).
    // One system writes ARZoomState (this controller), all others read it
    // (LODController via ARZoomState.ZoomFactor -> effective distance, section 10).
    // Keeping the math static and device-free makes it assertable without a scene.
    //
    // Assumption (documented): the tap cycle is MULTIPLICATIVE per tap_step.
    // Levels = [1.0, step, step^2, ...] clamped to [min_factor, max_factor], base first.
    // The tap past the last level wraps back to 1.0 (base) -- exactly the
    // "3rd tap/click returns to 1x" contract baked into WallConfigData tap_levels.

    // Input writer for ARZoomState (spec section 9). Reads zoom_* config from
    // WallSession.ZoomSettings and drives ARZoomState via ARZoomState.SetZoom,
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

        [Header("FOV")]
        [Tooltip("Camera whose Field of View narrows as ZoomFactor grows (spec section 9: " +
                "global, FOV-based zoom). If left unassigned, defaults to Camera.main at Start.")]
        [SerializeField] private Camera _camera;
        private float _baseFov;       // resting FOV captured once; runtime FOV = _baseFov / ZoomFactor
        private bool _fovCaptured;    // false until Start captures a camera

        private float _targetZoom;      // where the animation is heading
        private float _animFrom;        // where it started
        private float _animElapsed;     // seconds since it started
        private bool _animating;

        // The wall's zoom settings (read live: a Play Mode edit swaps the object). Internal so the
        // on-screen buttons (ZoomControlView) can follow Enable Zoom / Show UI Buttons.
        internal ZoomSettings Settings => _wallSession != null ? _wallSession.ZoomSettings : null;
        private ZoomSettings _lastSettings;

        private void Awake()
        {
            if (_wallSession == null)
                _wallSession = GetComponent<WallSession>();
        }

        // Capture the resting FOV once so zoom can be expressed as FOV = base / ZoomFactor.
        // Camera.main fallback keeps the component attach-and-play in the dev scene
        // (same pattern as WallSession+LODController sharing an object).
        private void Start()
        {
            if (_camera == null)
                _camera = Camera.main;
            if (_camera != null)
            {
                _baseFov = _camera.fieldOfView;
                _fovCaptured = true;
                if (_debug) Debug.Log($"[ARZoom] FOV driver active, base {_baseFov:F1}");
            }
        }

        // LateUpdate runs AFTER Update() each frame, where ARZoomState.ZoomFactor is
        // finalized, so the factor is final before we read it. This is the actual
        // visual zoom (spec section 9: global, FOV-based). The Max guard is a regression
        // backstop: ARZoomState clamps to >= min_factor (>=1) in normal flow, but a direct
        // SetZoom(0) caller elsewhere must never produce a NaN/Inf FOV.
        private void LateUpdate()
        {
            if (!_fovCaptured || _camera == null) return;
            float zoom = ARZoomState.ZoomFactor;
            // zoom > 1 narrows FOV (zoom IN); zoom == 1 is the resting wide view.
            _camera.fieldOfView = _baseFov / Mathf.Max(zoom, 0.0001f);
        }

        private void Update()
        {
            var settings = Settings;
            if (settings == null || !settings.enabled)
            {
                _animating = false;
                // - zoom switched off: the camera goes back to its own field of view
                if (settings != null && !Mathf.Approximately(ARZoomState.ZoomFactor, 1f))
                    ARZoomState.SetZoom(1f, 1f, 1f);
                return;
            }

            if (settings != _lastSettings)
            {
                _lastSettings = settings;
                // - new limits (e.g. a lower Max Zoom edited live) apply to the current zoom too
                ARZoomState.SetZoom(ARZoomState.ZoomFactor, settings.min_factor, settings.max_factor);
            }

            // Drive the smooth transition toward the pending target each frame.
            // ARZoomState.SetZoom writes (and hard-clamps) the global factor that
            // LODController divides into effective distance.
            if (_animating)
            {
                _animElapsed += Time.unscaledDeltaTime;
                float next = ARZoomMath.AnimatedZoom(_animFrom, _targetZoom, _animElapsed, settings.transition_duration_s);
                ARZoomState.SetZoom(next, settings.min_factor, settings.max_factor);
                _animating = _animElapsed < settings.transition_duration_s;
            }
        }

        // Snap to a target zoom immediately (used by tests / direct API callers).
        public void SetZoomImmediate(float targetZoomFactor)
        {
            var settings = Settings;
            if (settings == null || !settings.enabled) return;
            _targetZoom = Mathf.Clamp(targetZoomFactor, settings.min_factor, settings.max_factor);
            ARZoomState.SetZoom(_targetZoom, settings.min_factor, settings.max_factor);
            _animating = false;
            if (_debug) Debug.Log($"[ARZoom] immediate -> {_targetZoom:F2}");
        }

        public void SetZoomAnimated(float targetZoomFactor)
        {
            var settings = Settings;
            if (settings == null || !settings.enabled) return;
            _targetZoom = Mathf.Clamp(targetZoomFactor, settings.min_factor, settings.max_factor);
            _animFrom = ARZoomState.ZoomFactor;   // - a new click restarts from where the view is now
            _animElapsed = 0f;
            _animating = true;
            if (_debug) Debug.Log($"[ARZoom] animated -> {_targetZoom:F2}");
        }

        // --- entry points invoked by UI buttons / input bindings ---

        public void ZoomIn()  => StepLevel(true);
        public void ZoomOut() => StepLevel(false);

        // Where the next step starts: the level an animation is heading for, else the current zoom.
        // A second click while the first still animates must go one level FURTHER, not repeat it.
        private float StepOrigin() => _animating ? _targetZoom : ARZoomState.ZoomFactor;

        private void StepLevel(bool up)
        {
            var settings = Settings;
            if (settings == null || !settings.enabled) return;
            float current = StepOrigin();
            // + button caps at max (no wrap); - button retreats one level, floor at base.
            float target = up
                ? ARZoomMath.NextTapLevel(current, settings.tap_step, settings.tap_levels, settings.min_factor, settings.max_factor, wrap: false)
                : ARZoomMath.PreviousTapLevel(current, settings.tap_step, settings.tap_levels, settings.min_factor, settings.max_factor);
            SetZoomAnimated(target);
        }

        public void OnTap()
        {
            var settings = Settings;
            if (settings == null || !settings.enabled) return;
            float target = ARZoomMath.NextTapLevel(StepOrigin(), settings.tap_step, settings.tap_levels, settings.min_factor, settings.max_factor);
            SetZoomAnimated(target);
        }

        public void OnDoubleTap()
        {
            var settings = Settings;
            if (settings == null || !settings.enabled) return;
            float target = ARZoomMath.NextDoubleTapTarget(StepOrigin(), settings.min_factor, settings.max_factor);
            SetZoomAnimated(target);
        }

        // scaleFactor is the multiplicative finger-scale delta (>1 zooms in).
        public void OnPinch(float scaleFactor)
        {
            var settings = Settings;
            if (settings == null || !settings.enabled) return;
            float current = ARZoomState.ZoomFactor;
            float target = ARZoomMath.ApplyPinchScale(current, scaleFactor, settings.min_factor, settings.max_factor);
            SetZoomImmediate(target);
        }

        // "Fit to 1x": reset button handler.
        public void ResetToBase()
        {
            var settings = Settings;
            if (settings == null || !settings.enabled) return;
            ARZoomState.ResetToBase(settings.min_factor, settings.max_factor);
            _targetZoom = ARZoomState.ZoomFactor;
            _animating = false;
            if (_debug) Debug.Log("[ARZoom] reset to base");
        }
    }
}
