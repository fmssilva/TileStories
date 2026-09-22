using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using TileStories;

namespace TileStories.Tests
{
    // MarkerOverlapResolver tests retained from the legacy file (Block 2 rewrite).
    // CreateTestMarker is the shared marker-factory helper; MarkerBillboard verifies
    // the camera-facing rotation contract. The grouping/displacement math now lives in
    // DisplacementComputeTests (EditMode) and DisplacementLabelTests (PlayMode); this
    // file keeps only the billboard test it always owned.
    public class MarkerOverlapResolverTests
    {
        // Helper: creates a minimal MarkerView-backed marker for testing.
        private static MarkerView CreateTestMarker(string id, Vector3 position, Transform parent = null)
        {
            var go = new GameObject(id);
            if (parent != null) go.transform.SetParent(parent);
            go.transform.position = position;

            // POIAnchor holds the POIData this MarkerView renders.
            var anchor = go.AddComponent<POIAnchor>();
            var poiData = new POIData { id = id, name = id, category = "test" };
            anchor.Initialise(poiData);

            var markerView = go.AddComponent<MarkerView>();
            markerView.Initialise(anchor, MarkerVisualSettings.Default());

            return markerView;
        }

        [UnityTest]
        public IEnumerator MarkerBillboard_CameraMoved_MarkerRotationMatchesCamera()
        {
            // Arrange: create a camera and a marker with MarkerBillboard at an arbitrary starting rotation
            var camGO = new GameObject("TestCamera");
            camGO.tag = "MainCamera";
            var cam = camGO.AddComponent<Camera>();
            cam.transform.position = new Vector3(0, 1.5f, -2f);
            cam.transform.rotation = Quaternion.Euler(10f, 0f, 0f); // Arbitrary starting rotation

            var markerGO = new GameObject("TestMarker");
            markerGO.transform.position = new Vector3(0, 1.5f, 0);
            markerGO.transform.rotation = Quaternion.Euler(45f, 90f, 30f); // Arbitrary starting rotation
            var billboard = markerGO.AddComponent<MarkerBillboard>();

            // Act: move/rotate the camera to a new pose
            cam.transform.rotation = Quaternion.Euler(15f, 20f, 5f);

            // Wait one frame so LateUpdate executes
            yield return null;

            // Assert: marker faces the camera using the resolver's own default basis - world
            // gravity up (_2.1_Marker_Orientation.md v4: world_up + always_facing_camera is
            // the framework default), not a raw copy of camera-transform rotation and not
            // screen-up (that was v3's default; a wall developer who wants screen-up now
            // sets vertical_alignment_mode explicitly).
            var expectedRotation = Quaternion.LookRotation(cam.transform.forward, Vector3.up);
            float angleDiff = Quaternion.Angle(markerGO.transform.rotation, expectedRotation);
            Assert.Less(angleDiff, 0.1f,
                $"Marker rotation should face the camera with world-gravity up as its default basis. Angle difference: {angleDiff:F3} degrees.");

            // Cleanup
            Object.Destroy(markerGO);
            Object.Destroy(camGO);
        }

        [Test]
        public void MarkerBillboard_HasNoUpdateMethod_OnlyLateUpdate()
        {
            // Structural guarantee: rotation is written exactly once per frame, in
            // LateUpdate, since the camera's final pose for the frame is only known
            // after every Update has run (_2.1_Marker_Orientation.md section 4.4).
            var updateMethod = typeof(MarkerBillboard).GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNull(updateMethod, "MarkerBillboard must not define Update() - rotation is written only in LateUpdate.");
        }
    }
}