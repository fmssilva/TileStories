using NUnit.Framework;
using TileStories;

namespace TileStories.Tests
{
    // Tier-0 (EditMode, no scene): the Block 2 zoom-on-select GATE is pure logic
    // and runs identically across every wall, so it is the primary Block 2 assertion
    // (plan __curr_plan_tracker.md verification row: "Tier 0 zoom-on-select threshold
    // assert passes"). SelectionEventBus is a static relay and likewise Tier-0 testable.
    public sealed class ZoomOnSelectControllerTests
    {
        // Trigger == None must never zoom, regardless of density.
        [Test]
        public void None_trigger_returns_null()
        {
            var r = ZoomOnSelectController.ComputeZoomTarget(
                WallConfigData.ZoomOnSelectTrigger.None,
                neighborCount: 5, densityThreshold: 2, currentZoom: 1f, factor: 2f);
            Assert.IsNull(r);
        }

        // Cluster trigger alone must NOT zoom a Marker selection (Cluster is wired in
        // Block 3); only Marker/Both pass the gate.
        [Test]
        public void Cluster_trigger_returns_null_for_marker_selection()
        {
            var r = ZoomOnSelectController.ComputeZoomTarget(
                WallConfigData.ZoomOnSelectTrigger.Cluster,
                neighborCount: 5, densityThreshold: 2, currentZoom: 1f, factor: 2f);
            Assert.IsNull(r);
        }

        // Below-threshold density skips the zoom (no point zooming an isolated marker).
        [Test]
        public void Marker_below_threshold_returns_null()
        {
            var r = ZoomOnSelectController.ComputeZoomTarget(
                WallConfigData.ZoomOnSelectTrigger.Marker,
                neighborCount: 1, densityThreshold: 2, currentZoom: 1f, factor: 2f);
            Assert.IsNull(r);
        }

        // No neighbour data (frustum-culled / not yet evaluated) skips the zoom.
        [Test]
        public void Marker_null_neighbor_count_returns_null()
        {
            var r = ZoomOnSelectController.ComputeZoomTarget(
                WallConfigData.ZoomOnSelectTrigger.Marker,
                neighborCount: null, densityThreshold: 2, currentZoom: 1f, factor: 2f);
            Assert.IsNull(r);
        }

        // At exactly the threshold the zoom applies.
        [Test]
        public void Marker_at_threshold_zooms()
        {
            var r = ZoomOnSelectController.ComputeZoomTarget(
                WallConfigData.ZoomOnSelectTrigger.Marker,
                neighborCount: 2, densityThreshold: 2, currentZoom: 1f, factor: 2f);
            Assert.IsNotNull(r);
            Assert.AreEqual(2f, r.Value);
        }

        // Above threshold: zoom is the multiplicative product current * factor.
        // Clamping to [min,max] happens downstream in ARZoomController, not here.
        [Test]
        public void Marker_above_threshold_returns_current_times_factor()
        {
            var r = ZoomOnSelectController.ComputeZoomTarget(
                WallConfigData.ZoomOnSelectTrigger.Marker,
                neighborCount: 9, densityThreshold: 2, currentZoom: 3f, factor: 2f);
            Assert.IsNotNull(r);
            Assert.AreEqual(6f, r.Value);
        }

        // Both trigger includes the Marker unit kind.
        [Test]
        public void Both_trigger_zooms_when_dense()
        {
            var r = ZoomOnSelectController.ComputeZoomTarget(
                WallConfigData.ZoomOnSelectTrigger.Both,
                neighborCount: 3, densityThreshold: 2, currentZoom: 1f, factor: 2f);
            Assert.IsNotNull(r);
            Assert.AreEqual(2f, r.Value);
        }
    }

    public sealed class SelectionEventBusTests
    {
        // RaiseMarkerSelected must deliver the POI id to subscribers.
        [Test]
        public void RaiseMarkerSelected_delivers_poiId()
        {
            string captured = null;
            void Handler(string id) => captured = id;
            SelectionEventBus.OnMarkerSelected += Handler;
            try
            {
                SelectionEventBus.RaiseMarkerSelected("poi_42");
                Assert.AreEqual("poi_42", captured);
            }
            finally { SelectionEventBus.OnMarkerSelected -= Handler; }
        }

        // RaiseSelectionCleared must deliver to subscribers.
        [Test]
        public void RaiseSelectionCleared_delivers()
        {
            bool cleared = false;
            void Handler() => cleared = true;
            SelectionEventBus.OnSelectionCleared += Handler;
            try
            {
                SelectionEventBus.RaiseSelectionCleared();
                Assert.IsTrue(cleared);
            }
            finally { SelectionEventBus.OnSelectionCleared -= Handler; }
        }

        // Unsubscribed listeners must not fire -- the contract that
        // SelectionHighlightController/ZoomOnSelectController dispose uphold.
        [Test]
        public void Unsubscribed_listener_not_called()
        {
            string captured = "initial";
            void Handler(string id) => captured = id;
            SelectionEventBus.OnMarkerSelected += Handler;
            SelectionEventBus.RaiseMarkerSelected("first");
            Assert.AreEqual("first", captured);
            SelectionEventBus.OnMarkerSelected -= Handler;
            SelectionEventBus.RaiseMarkerSelected("second");
            Assert.AreEqual("first", captured); // unchanged after unsubscribe
        }
    }
}
