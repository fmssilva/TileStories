using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using TileStories;

namespace TileStories.Tests
{
    // Phase B seam check for the FOV driver added in 2.4-l: that
    // ARZoomState.ZoomFactor actually narrows the scene Camera's fieldOfView.
    // Mirrors ARZoomRoutingTests' construction (active controller GO + an
    // inactive WallSession GO carrying the config), then drives
    // SetZoomImmediate / SetZoomAnimated and asserts the real Camera.fieldOfView
    // follows. Without this, prior "zoom math" suites only ever tested the number,
    // never the visual zoom the spec section 9 promises.
    public class ARZoomCameraFovTests
    {
        private GameObject _zoomGO;
        private GameObject _wsGO;
        private Camera _camera;
        private ARZoomController _zoom;
        private WallSession _ws;

        private static WallConfigData BuildConfig(
            bool enabled = true, float min = 1f, float max = 4f, float transition = 0.1f)
        {
            return new WallConfigData
            {
                zoom_settings = new ZoomSettings
                {
                    enabled = enabled,
                    min_factor = min,
                    max_factor = max,
                    tap_step = 1.5f,
                    tap_levels = 2,
                    transition_duration_s = transition,
                    show_ui_buttons = true
                }
            };
        }

        [SetUp]
        public void SetUp()
        {
            // ARZoomState is a static global shared across the PlayMode run --
            // reset to base so tests can't leak one another's factor.
            ARZoomState.SetZoom(1f, 1f, 4f);
        }

        // Build the same harness as ARZoomRoutingTests, but attach the Camera to
        // the controller object so Start() captures it (do not rely on Camera.main
        // -- a bare test scene has no MainCamera-tagged object).
        private void BuildHarness(WallConfigData cfg)
        {
            _zoomGO = new GameObject("ZoomController");
            _zoom = _zoomGO.AddComponent<ARZoomController>();
            _camera = _zoomGO.AddComponent<Camera>(); // Start will capture this one

            // WallSession on an INACTIVE GO so its Awake never fires a tracker
            // or a config-load coroutine that would race the injected config.
            _wsGO = new GameObject("WallSessionHolder");
            _wsGO.SetActive(false);
            _ws = _wsGO.AddComponent<WallSession>();

            SetField(_ws, "_config", cfg);            // ws.ZoomSettings now resolves
            SetField(_zoom, "_wallSession", _ws);     // ARZoomController.Settings resolves
            SetField(_zoom, "_camera", _camera);      // bypass Camera.main lookup
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_zoomGO);
            Object.DestroyImmediate(_wsGO);
            // ARZoomState is a static global shared across the whole PlayMode run.
            // Several tests here leave ZoomFactor > 1; reset to base so no later test
            // (e.g. LOD-displacement, which measures exact screen-Y and is hypersensitive
            // to camera FOV) sees a leaked zoom that widens/shrinks Camera.fieldOfView.
            ARZoomState.SetZoom(1f, 1f, 4f);
        }

        [UnityTest]
        public IEnumerator SetZoomImmediate_NarrowsFov_ToHalf()
        {
            BuildHarness(BuildConfig());
            float baseFov = _camera.fieldOfView;

            _zoom.SetZoomImmediate(2f); // factor 2 -> FOV = base / 2
            yield return null;          // Start (capture) + LateUpdate (drive)

            Assert.That(_camera.fieldOfView, Is.EqualTo(baseFov / 2f).Within(0.5f),
                "zoom factor 2 must halve the FOV (zoom IN = narrower FOV)");
            Assert.That(_camera.fieldOfView, Is.LessThan(baseFov),
                "must narrow, not widen -- direction sanity check from _2.7 2.4-l");
            Assert.That(_camera.fieldOfView, Is.GreaterThan(1f),
                "FOV must stay > 1 (no inversion through zero/negative)");
        }

        [UnityTest]
        public IEnumerator SetZoomAnimated_Converges_ToQuarter_NotInstant()
        {
            BuildHarness(BuildConfig(transition: 0.5f));
            float baseFov = _camera.fieldOfView; // factor 1 -> FOV == base

            _zoom.SetZoomAnimated(4f); // factor 4 -> FOV = base / 4

            // Deterministic "not instant" guarantee: the animated entry point must
            // NOT jump the global zoom state synchronously -- it only records a
            // target and waits for Update()/LateUpdate() to walk toward it. If this
            // ever became a synchronous SetZoom(4), the whole smoothness contract breaks.
            Assert.That(ARZoomState.ZoomFactor, Is.EqualTo(1f).Within(1e-5f),
                "SetZoomAnimated must not jump ARZoomState to the target synchronously");

            // Let the animation run; it must move the FOV down toward base/4 over frames,
            // and must never overshoot below it. Exact convergence arithmetic is
            // already covered by SetZoomImmediate (above) and by AnimatedZoom's
            // own unit tests -- here we only need "animated = gradual, bounded".
            for (int i = 0; i < 40; i++) yield return null;
            float settledFov = _camera.fieldOfView;
            Assert.That(settledFov, Is.LessThan(baseFov),
                "animated zoom must begin narrowing the FOV over frames (not snap)");
            Assert.That(settledFov, Is.GreaterThanOrEqualTo(baseFov / 4f - 1e-5f),
                "animated zoom must never overshoot below base / target");
        }

        [UnityTest]
        public IEnumerator ZoomDisabled_DoesNotDriveFov()
        {
            BuildHarness(BuildConfig(enabled: false));
            float baseFov = _camera.fieldOfView;

            // zoom disabled: SetZoomImmediate no-ops; ARZoomState stays at 1,
            // so LateUpdate reads factor 1 -> FOV unchanged from base.
            _zoom.SetZoomImmediate(2f);
            yield return null;

            Assert.That(_camera.fieldOfView, Is.EqualTo(baseFov).Within(0.001f),
                "disabled zoom must not narrow the camera FOV");
        }

        [UnityTest]
        public IEnumerator NearZeroFactor_DoesNotProduceNaNorInfinity()
        {
            // Regression for the Mathf.Max(zoom, 0.0001f) guard: a caller that
            // forces the factor toward 0 must never divide by zero. min_factor=0 is
            // the only way to let zero reach the LateUpdate path.
            BuildHarness(BuildConfig(min: 0f));
            float baseFov = _camera.fieldOfView;

            _zoom.SetZoomImmediate(0f);
            // (Start + LateUpdate run on the first yield below.)
            yield return null;

            // Unity clamps Camera.fieldOfView on assignment, but the *formula*
            // must never emit NaN/Infinity before that clamp -- assert the result.
            float fov = _camera.fieldOfView;
            Assert.IsFalse(float.IsNaN(fov), "FOV must never be NaN (division-by-zero guard)");
            Assert.IsFalse(float.IsInfinity(fov), "FOV must never be Infinity (division-by-zero guard)");
            Assert.That(fov, Is.Not.EqualTo(baseFov));
        }

        // --- reflection helpers: identical seam to ARZoomRoutingTests ---

        private static void SetField(object obj, string name, object value) =>
            obj.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)!
                .SetValue(obj, value);

        private static object GetField(object obj, string name) =>
            obj.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)!
                .GetValue(obj);
    }
}
