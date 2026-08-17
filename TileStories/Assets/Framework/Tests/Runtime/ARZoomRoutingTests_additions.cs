using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using TileStories;

namespace TileStories.Tests
{
    // PlayMode routing proof for Block 2 (spec _2.6 section 11): the static
    // SelectionEventBus relays a marker selection into ZoomOnSelectController,
    // which (via ComputeZoomTarget) drives ARZoomController.SetZoomAnimated.
    // Setup mirrors ARZoomRoutingTests: inject _config + _wallSession by reflection
    // on an INACTIVE WallSession GO so Awake does not fire IWallTracker / load config.
    public sealed class MarkerSelectRoutingTests
    {
        [Test]
        public void Dense_marker_selection_routes_bus_to_ARZoomController_target()
        {
            var wsGO = new GameObject("WallSessionHolder");
            wsGO.SetActive(false); // Awake deferred: no IWallTracker error, no config coroutine
            var ws = wsGO.AddComponent<WallSession>();

            var cfg = new WallConfigData
            {
                selection_highlight_enabled = true,
                zoom_on_select_trigger = WallConfigData.ZoomOnSelectTrigger.Marker,
                zoom_on_select_density_threshold = 2,
                zoom_on_select_factor = 2f,
                lod_settings = new LodSettings
                {
                    zoom_enabled = true,
                    zoom_min = 1f,
                    zoom_max = 4f,
                    zoom_tap_step = 1.5f,
                    zoom_tap_levels = 2,
                    zoom_show_ui_buttons = true,
                    zoom_transition_speed_s = 0.1f
                }
            };
            SetField(ws, "_config", cfg); // ws.LodSettings now resolves our settings

            // ARZoomController + LODController co-locate on the WallSession GO
            // (their Awake resolves WallSession via GetComponent -- see their source).
            var zoom = wsGO.AddComponent<ARZoomController>();
            var lod = wsGO.AddComponent<LODController>();
            SetField(zoom, "_wallSession", ws); // ARZoomController.Settings now resolves
            SetField(lod, "_wallSession", ws);

            // Seed density: poi_dense is dense enough, poi_sparse is isolated,
            // poi_unknown has no neighbour data at all.
            SetField(lod, "_lastNeighborCounts",
                new Dictionary<string, int> { ["poi_dense"] = 5, ["poi_sparse"] = 1 });

            var controller = new ZoomOnSelectController(ws, cfg, zoom, lod);

            ARZoomState.SetZoom(1f, 1f, 4f); // global reset to base
            Assert.AreEqual(1f, ARZoomState.ZoomFactor);

            // SelectionEventBus.RaiseMarkerSelected is the exact call
            // MarkerSelectable.OnPointerClick publishes on a real tap.
            SelectionEventBus.RaiseMarkerSelected("poi_dense");

            // Gate passed -> SetZoomAnimated(1.0 * 2.0) -> _targetZoom clamped to 2.0.
            Assert.That((float)GetField(zoom, "_targetZoom"), Is.EqualTo(2.0f).Within(1e-4f));
            Assert.IsTrue((bool)GetField(zoom, "_animating"));

            // Isolated marker: density below threshold -> gate returns null -> zoom untouched.
            float before = (float)GetField(zoom, "_targetZoom");
            SelectionEventBus.RaiseMarkerSelected("poi_sparse");
            Assert.That((float)GetField(zoom, "_targetZoom"), Is.EqualTo(before).Within(1e-4f));

            // Unknown marker: no neighbour data -> gate returns null -> zoom untouched.
            before = (float)GetField(zoom, "_targetZoom");
            SelectionEventBus.RaiseMarkerSelected("poi_unknown");
            Assert.That((float)GetField(zoom, "_targetZoom"), Is.EqualTo(before).Within(1e-4f));

            // Block 2 contract: dispose releases the static bus subscription so the
            // next test/PlayMode domain starts clean.
            controller.Dispose();
            Object.DestroyImmediate(wsGO);
        }

        private static void SetField(object obj, string name, object value) =>
            obj.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)!
               .SetValue(obj, value);

        private static object GetField(object obj, string name) =>
            obj.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)!
               .GetValue(obj);
    }
}
