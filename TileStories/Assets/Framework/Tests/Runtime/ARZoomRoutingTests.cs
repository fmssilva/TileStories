using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using TileStories;

namespace TileStories.Tests
{
    // Behavioural wiring for the zoom button -> ARZoomController -> ARZoomState path.
    // Lives in PlayMode because ARZoomController is a MonoBehaviour: only in PlayMode does
    // AddComponent run Awake synchronously, so a reflect-injected _wallSession survives. In
    // EditMode the same AddComponent defers Awake and clobbers the injected reference.
    // ARZoomState is the global zoom state observed directly. Pure UXML/USS/tap-target /
    // gating coverage stays in ZoomControlViewEditModeTests (where AssetDatabase works).
    public class ARZoomRoutingTests
    {
        [Test]
        public void Button_handlers_route_to_real_ARZoomController_and_ARZoomState()
        {
            // Active GO: Awake runs sync here, _wallSession = GetComponent<WallSession>() = null.
            var zoomGO = new GameObject("ZoomController");
            var zoom = zoomGO.AddComponent<ARZoomController>();

            // Detached WallSession on an INACTIVE GO so Awake never fires (no IWallTracker
            // error log, no LoadConfigCoroutine racing our injected config). Built via
            // AddComponent (not 'new') to avoid Unity's "MonoBehaviour using 'new'" warning.
            var wsGO = new GameObject("WallSessionHolder");
            wsGO.SetActive(false);
            var ws = wsGO.AddComponent<WallSession>();

            var cfg = new WallConfigData();
            cfg.lod_settings = new LodSettings
            {
                zoom_enabled = true,
                zoom_min = 1f,
                zoom_max = 4f,
                zoom_tap_step = 1.5f,
                zoom_tap_levels = 2,
                zoom_show_ui_buttons = true,
                zoom_transition_speed_s = 0.1f
            };
            SetField(ws, "_config", cfg);        // ws.LodSettings now reads our settings
            SetField(zoom, "_wallSession", ws); // ARZoomController.Settings now resolves

            // Settings must resolve now that _wallSession is injected (the original failure).
            var settingsProp = typeof(ARZoomController).GetProperty("Settings",
                BindingFlags.NonPublic | BindingFlags.Instance);
            var resolved = settingsProp.GetValue(zoom);
            Assert.IsNotNull(resolved,
                "ARZoomController.Settings must resolve after ws/_config injection");
            Assert.IsTrue(((LodSettings)resolved).zoom_enabled);

            // View bound to the real controller (no UXML/UIDocument needed for pure routing).
            var zcvGO = new GameObject("ZoomControl");
            var zcv = zcvGO.AddComponent<ZoomControlView>();
            SetField(zcv, "_zoom", zoom);

            ARZoomState.SetZoom(1f, 1f, 4f); // reset global
            Assert.AreEqual(1f, ARZoomState.ZoomFactor);

            // ZoomIn -> StepLevel(true) -> NextTapLevel(1.0)=1.5 -> SetZoomAnimated(1.5)
            InvokePrivate(zcv, "ZoomInClicked");
            Assert.That((float)GetField(zoom, "_targetZoom"), Is.EqualTo(1.5f).Within(1e-4f));
            Assert.IsTrue((bool)GetField(zoom, "_animating"));

            // ZoomOut -> PreviousTapLevel(1.5) retreats one level back to base (1.0)
            InvokePrivate(zcv, "ZoomOutClicked");
            Assert.That((float)GetField(zoom, "_targetZoom"), Is.EqualTo(1.0f).Within(1e-4f));

            // Reset -> ResetToBase -> ARZoomState.ResetToBase -> SetZoom(1) (instant, no Update tick)
            InvokePrivate(zcv, "ZoomResetClicked");
            Assert.AreEqual(1f, ARZoomState.ZoomFactor);

            Object.DestroyImmediate(zoomGO);
            Object.DestroyImmediate(wsGO);
            Object.DestroyImmediate(zcvGO);
        }

        private static void SetField(object obj, string name, object value) =>
            obj.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)!
                .SetValue(obj, value);

        private static object GetField(object obj, string name) =>
            obj.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)!
                .GetValue(obj);

        private static void InvokePrivate(object obj, string method) =>
            obj.GetType().GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance)!
                .Invoke(obj, null);
    }
}
