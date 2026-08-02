using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using TileStories;

namespace TileStories.Tests
{
    // Tests for MarkerOverlapResolver: clustering-based overlap detection and resolution.
    // See _1_2_POI_Colision_Solver.md Section 4 for the test design rationale.
    // These are PlayMode tests (in Runtime folder) because they create GameObjects
    // and use WorldToScreenPoint which requires a camera component.
    public class MarkerOverlapResolverTests
    {
        // Helper: creates a minimal MarkerView-like setup for testing.
        // Since MarkerView requires a POIAnchor with Data, we create a minimal setup.
        private static MarkerView CreateTestMarker(string id, Vector3 position, Transform parent = null)
        {
            var go = new GameObject(id);
            if (parent != null) go.transform.SetParent(parent);
            go.transform.position = position;

            // Create a minimal POIAnchor with POIData
            var anchor = go.AddComponent<POIAnchor>();
            var poiData = new POIData { id = id, name = id, category = "test" };
            anchor.Initialise(poiData);

            // Add MarkerView and initialize it
            var markerView = go.AddComponent<MarkerView>();
            markerView.Initialise(anchor, MarkerStyle.OutlineGold, MarkerShape.Circle);

            return markerView;
        }

        [UnityTest]
        public IEnumerator ApplyOverlapOffsets_FiveClusteredMarkers_AllEndUpVisuallySeparated()
        {
            // Arrange: a camera looking at a cluster of 5 markers, all within a
            // few centimeters of each other - close enough to land well within
            // the 40px screen-space overlap threshold if left unoffset.
            // Use a closer camera (0.5m) so the 15cm offset creates visible separation.
            // At 0.5m distance, 15cm in world space = ~222px on screen (for 60deg FOV).
            var camGO = new GameObject("TestCamera");
            var cam = camGO.AddComponent<Camera>();
            cam.transform.position = new Vector3(0, 1.5f, -0.5f);
            cam.transform.LookAt(new Vector3(0, 1.5f, 0));

            string[] ids = { "painting", "painting_1", "painting_2", "painting_3", "painting_4" };
            var markers = new List<MarkerView>();

            for (int i = 0; i < ids.Length; i++)
            {
                // Small deliberate jitter (a few cm) so positions aren't
                // bit-for-bit identical, matching how a human would actually
                // place near-duplicate markers by hand
                var marker = CreateTestMarker(ids[i], new Vector3(
                    0 + i * 0.01f, 1.5f + i * 0.01f, 0));
                markers.Add(marker);
            }

            // Act
            MarkerOverlapResolver.ApplyOverlapOffsets(markers, cam);

            // Assert: every pair of markers must now be more than 40px apart on
            // screen (the same threshold the resolver itself uses) - the actual
            // proof that visitors would no longer see overlapping markers.
            for (int i = 0; i < markers.Count; i++)
            {
                for (int j = i + 1; j < markers.Count; j++)
                {
                    Vector3 screenA = cam.WorldToScreenPoint(markers[i].transform.position);
                    Vector3 screenB = cam.WorldToScreenPoint(markers[j].transform.position);
                    float dist = Vector2.Distance(screenA, screenB);

                    Assert.Greater(dist, 40f,
                        $"{markers[i].name} and {markers[j].name} are still within " +
                        $"the overlap threshold after ApplyOverlapOffsets ({dist:F1}px apart).");
                }
            }

            // Cleanup
            foreach (var m in markers) Object.Destroy(m.gameObject);
            Object.Destroy(camGO);

            yield return null;
        }

        [UnityTest]
        public IEnumerator ApplyOverlapOffsets_CalledTwice_ProducesSameResultBothTimes()
        {
            // Regression test for the idempotence question from Section 1's
            // audit: calling the resolver again on markers that are already
            // spread apart must not push them further away a second time.
            var camGO = new GameObject("TestCamera");
            var cam = camGO.AddComponent<Camera>();
            cam.transform.position = new Vector3(0, 1.5f, -1f);
            cam.transform.LookAt(new Vector3(0, 1.5f, 0));

            // Create 2 overlapping markers
            var markerA = CreateTestMarker("marker_a", new Vector3(0, 1.5f, 0));
            var markerB = CreateTestMarker("marker_b", new Vector3(0.01f, 1.5f, 0));
            var markers = new List<MarkerView> { markerA, markerB };

            // Act: first call
            MarkerOverlapResolver.ApplyOverlapOffsets(markers, cam);

            // Record positions after first call
            Vector3 posA1 = markerA.transform.position;
            Vector3 posB1 = markerB.transform.position;

            // Act: second call on the same markers
            MarkerOverlapResolver.ApplyOverlapOffsets(markers, cam);

            // Record positions after second call
            Vector3 posA2 = markerA.transform.position;
            Vector3 posB2 = markerB.transform.position;

            // Assert: positions should be identical after both calls
            Assert.AreEqual(posA1.x, posA2.x, 0.0001f, "X position should be unchanged on second call.");
            Assert.AreEqual(posA1.y, posA2.y, 0.0001f, "Y position should be unchanged on second call.");
            Assert.AreEqual(posA1.z, posA2.z, 0.0001f, "Z position should be unchanged on second call.");

            Assert.AreEqual(posB1.x, posB2.x, 0.0001f, "X position should be unchanged on second call.");
            Assert.AreEqual(posB1.y, posB2.y, 0.0001f, "Y position should be unchanged on second call.");
            Assert.AreEqual(posB1.z, posB2.z, 0.0001f, "Z position should be unchanged on second call.");

            // Cleanup
            Object.Destroy(markerA.gameObject);
            Object.Destroy(markerB.gameObject);
            Object.Destroy(camGO);

            yield return null;
        }

        [UnityTest]
        public IEnumerator ApplyOverlapOffsets_NoOverlap_LeavesPositionsUnchanged()
        {
            // Regression test: markers that are already far apart (like
            // LivingRoom's original 3 POIs) must not be moved at all.
            var camGO = new GameObject("TestCamera");
            var cam = camGO.AddComponent<Camera>();
            cam.transform.position = new Vector3(0, 1.5f, -1f);
            cam.transform.LookAt(new Vector3(0, 1.5f, 0));

            // Create 2 markers far apart on screen (one far left, one far right)
            var markerA = CreateTestMarker("far_left", new Vector3(-5f, 1.5f, 0));
            var markerB = CreateTestMarker("far_right", new Vector3(5f, 1.5f, 0));
            var markers = new List<MarkerView> { markerA, markerB };

            // Record initial positions
            Vector3 initialA = markerA.transform.position;
            Vector3 initialB = markerB.transform.position;

            // Act
            MarkerOverlapResolver.ApplyOverlapOffsets(markers, cam);

            // Assert: positions should be unchanged
            Vector3 finalA = markerA.transform.position;
            Vector3 finalB = markerB.transform.position;

            Assert.AreEqual(initialA.x, finalA.x, 0.0001f, "Marker A X should be unchanged.");
            Assert.AreEqual(initialA.y, finalA.y, 0.0001f, "Marker A Y should be unchanged.");
            Assert.AreEqual(initialA.z, finalA.z, 0.0001f, "Marker A Z should be unchanged.");

            Assert.AreEqual(initialB.x, finalB.x, 0.0001f, "Marker B X should be unchanged.");
            Assert.AreEqual(initialB.y, finalB.y, 0.0001f, "Marker B Y should be unchanged.");
            Assert.AreEqual(initialB.z, finalB.z, 0.0001f, "Marker B Z should be unchanged.");

            // Cleanup
            Object.Destroy(markerA.gameObject);
            Object.Destroy(markerB.gameObject);
            Object.Destroy(camGO);

            yield return null;
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

             // Assert: marker's world rotation should equal camera's world rotation
             // (no additional offset since we removed the 180-degree yaw to prevent text mirroring)
             var expectedRotation = cam.transform.rotation;
             float angleDiff = Quaternion.Angle(markerGO.transform.rotation, expectedRotation);
             Assert.Less(angleDiff, 0.1f,
                 $"Marker rotation should match camera rotation (no offset). Angle difference: {angleDiff:F3} degrees.");

            // Cleanup
            Object.Destroy(markerGO);
            Object.Destroy(camGO);
        }
    }
}
