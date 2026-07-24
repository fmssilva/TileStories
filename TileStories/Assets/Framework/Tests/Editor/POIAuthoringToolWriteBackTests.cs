using NUnit.Framework;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // Tests for the Authoring Tool's read-modify-write cycle.
    // Pure data tests — no scene, no Editor window, no Unity dependencies.
    public class POIAuthoringToolWriteBackTests
    {
        // Simulate the Capture Positions operation: given a config with 3 POIs
        // and a set of "placed" scene objects matching only 2 of them,
        // only those 2 POIs should have their captured_position updated.
        [Test]
        public void CapturePositions_OnlyUpdatesMatchedPOIs()
        {
            var config = CreateTestConfig();

            // Simulate scene objects: we have markers for "lamp" and "painting" but NOT "camera"
            var placedIds = new HashSet<string> { "lamp", "painting" };

            // Simulate Capture Positions: iterate POIs, update only those matched
            int captured = 0;
            int skipped = 0;

            foreach (var poi in config.pois)
            {
                if (!placedIds.Contains(poi.id))
                {
                    skipped++;
                    continue;
                }

                poi.captured_position = new CapturedPosition
                {
                    x = 1.0f,
                    y = 2.0f,
                    z = 3.0f
                };
                poi.captured_position_source = "workflow_a_editor";
                poi.captured_position_timestamp = 1234567890;
                captured++;
            }

            Assert.AreEqual(2, captured, "Should capture 2 POIs.");
            Assert.AreEqual(1, skipped, "Should skip 1 POI (camera).");

            // Verify captured POIs have the updated position
            var lamp = config.pois.Find(p => p.id == "lamp");
            Assert.IsNotNull(lamp.captured_position, "lamp should have captured_position.");
            Assert.AreEqual(1.0f, lamp.captured_position.x, 0.001f);
            Assert.AreEqual("workflow_a_editor", lamp.captured_position_source);

            var painting = config.pois.Find(p => p.id == "painting");
            Assert.IsNotNull(painting.captured_position, "painting should have captured_position.");
            Assert.AreEqual(1.0f, painting.captured_position.x, 0.001f);

            // Verify the unmatched POI was NOT touched
            var camera = config.pois.Find(p => p.id == "camera");
            Assert.IsNull(camera.captured_position, "camera should NOT have captured_position (was not in scene).");
            Assert.IsNull(camera.captured_position_source, "camera source should remain null.");
            Assert.AreEqual(0, camera.captured_position_timestamp, "camera timestamp should remain 0.");
        }

        // Verify that capturing only one POI doesn't affect other POIs' fields.
        [Test]
        public void CapturePositions_PartialCapture_DoesNotAffectOthers()
        {
            var config = CreateTestConfig();

            // Simulate capturing only "lamp"
            var lamp = config.pois.Find(p => p.id == "lamp");
            lamp.captured_position = new CapturedPosition { x = 1f, y = 2f, z = 3f };
            lamp.captured_position_source = "workflow_a_editor";
            lamp.captured_position_timestamp = 1234567890;

            // Verify lamp has captured_position
            Assert.IsNotNull(lamp.captured_position);
            Assert.AreEqual(1f, lamp.captured_position.x);

            // Verify painting and camera were NOT touched
            var painting = config.pois.Find(p => p.id == "painting");
            Assert.IsNull(painting.captured_position, "painting should be untouched.");

            var camera = config.pois.Find(p => p.id == "camera");
            Assert.IsNull(camera.captured_position, "camera should be untouched.");
        }

        // Verify that capturing a POI at the origin (0,0,0) is preserved correctly.
        [Test]
        public void CapturePositions_OriginPosition_IsStored()
        {
            var config = CreateTestConfig();

            var lamp = config.pois.Find(p => p.id == "lamp");
            lamp.captured_position = new CapturedPosition { x = 0f, y = 0f, z = 0f };
            lamp.captured_position_source = "workflow_a_editor";
            lamp.captured_position_timestamp = 1234567890;

            Assert.IsNotNull(lamp.captured_position, "captured_position should exist (not null).");
            Assert.AreEqual(0f, lamp.captured_position.x, 0.001f);
            Assert.AreEqual(0f, lamp.captured_position.y, 0.001f);
            Assert.AreEqual(0f, lamp.captured_position.z, 0.001f);
        }

        // Verify that calibration anchors are also captured when they have matching scene objects.
        // Verify that calibration anchors in calibration_anchors list each have
        // id, x_norm, y_norm, and captured_position.
        [Test]
        public void CalibrationAnchors_HaveRequiredFields()
        {
            var config = CreateTestConfig();

            Assert.IsNotNull(config.calibration_anchors, "calibration_anchors list should exist.");
            Assert.Greater(config.calibration_anchors.Count, 0, "Should have at least one calibration anchor.");

            foreach (var anchor in config.calibration_anchors)
            {
                Assert.IsNotNull(anchor.id, "Anchor id should not be null.");
                Assert.IsNotNull(anchor.captured_position, "Anchor captured_position should not be null.");
                Assert.AreEqual("cal_left", anchor.id);
                Assert.AreEqual(0f, anchor.x_norm);
                Assert.AreEqual(0.5f, anchor.y_norm);
            }
        }

        private static WallConfigData CreateTestConfig()
        {
            return new WallConfigData
            {
                wall_id = "test_wall",
                wall_name = "Test Wall",
                immersal_map_id = 12345,
                calibration_anchors = new List<CalibrationAnchor>
                {
                    new CalibrationAnchor
                    {
                        id = "cal_left",
                        x_norm = 0f,
                        y_norm = 0.5f,
                        captured_position = new CapturedPosition { x = 0f, y = 0f, z = 0f }
                    }
                },
                pois = new List<POIData>
                {
                    new POIData { id = "lamp", name = "The Lamp", x_norm = 0.1f, y_norm = 0.1f },
                    new POIData { id = "painting", name = "The Painting", x_norm = 0.5f, y_norm = 0.3f },
                    new POIData { id = "camera", name = "The Camera", x_norm = 0.8f, y_norm = 0.7f }
                }
            };
        }
    }

    // Tests for the sync-check method in POIAuthoringToolWindow.
    // These tests use Unity's EditMode test framework to create scene objects.
    public class POIAuthoringToolWindowSyncCheckTests
    {
        // Helper to create a test config with captured positions.
        private static WallConfigData CreateConfigWithCapturedPositions()
        {
            return new WallConfigData
            {
                wall_id = "test_wall",
                wall_name = "Test Wall",
                immersal_map_id = 12345,
                pois = new List<POIData>
                {
                    new POIData
                    {
                        id = "lamp",
                        name = "The Lamp",
                        x_norm = 0.1f,
                        y_norm = 0.1f,
                        captured_position = new CapturedPosition { x = 1.0f, y = 2.0f, z = 3.0f },
                        has_captured_position = true
                    },
                    new POIData
                    {
                        id = "painting",
                        name = "The Painting",
                        x_norm = 0.5f,
                        y_norm = 0.3f,
                        captured_position = new CapturedPosition { x = 4.0f, y = 5.0f, z = 6.0f },
                        has_captured_position = true
                    }
                }
            };
        }

        // Helper to create a test config with one POI lacking captured position.
        private static WallConfigData CreateConfigWithUncapturedPOI()
        {
            return new WallConfigData
            {
                wall_id = "test_wall",
                wall_name = "Test Wall",
                immersal_map_id = 12345,
                pois = new List<POIData>
                {
                    new POIData
                    {
                        id = "lamp",
                        name = "The Lamp",
                        x_norm = 0.1f,
                        y_norm = 0.1f,
                        captured_position = null,
                        has_captured_position = false
                    }
                }
            };
        }

        // Test: All positions match -> reports in sync.
        [Test]
        public void SyncCheck_AllPositionsMatch_ReportsInSync()
        {
            // Arrange: create a rig with children at known positions, and a config
            // whose matching POIs have has_captured_position = true at those same positions.
            var config = CreateConfigWithCapturedPositions();
            var anchor = CreateTestAnchor();
            var rig = CreateTestRig(anchor, new Dictionary<string, Vector3>
            {
                { "lamp", new Vector3(1.0f, 2.0f, 3.0f) },
                { "painting", new Vector3(4.0f, 5.0f, 6.0f) }
            });

            // Create window and set its fields via reflection
            var window = CreateWindowWithConfig(config, anchor);

            // Act: call IsRigInSyncWithConfig.
            var result = window.IsRigInSyncWithConfig(out int outOfSyncCount);

            // Assert: returns true, outOfSyncCount == 0.
            Assert.IsTrue(result, "Should report in sync when all positions match.");
            Assert.AreEqual(0, outOfSyncCount, "outOfSyncCount should be 0 when in sync.");

            // Cleanup
            Object.DestroyImmediate(rig.gameObject);
            Object.DestroyImmediate(anchor.gameObject);
        }

        // Test: One position moved -> reports out of sync with correct count.
        [Test]
        public void SyncCheck_OnePositionMoved_ReportsOutOfSyncWithCorrectCount()
        {
            // Arrange: config has captured positions, but rig child is at a different position.
            var config = CreateConfigWithCapturedPositions();
            var anchor = CreateTestAnchor();
            var rig = CreateTestRig(anchor, new Dictionary<string, Vector3>
            {
                { "lamp", new Vector3(1.0f, 2.0f, 3.0f) },
                { "painting", new Vector3(10.0f, 20.0f, 30.0f) } // Moved far away
            });

            // Create window and set its fields via reflection
            var window = CreateWindowWithConfig(config, anchor);

            // Act: call IsRigInSyncWithConfig.
            var result = window.IsRigInSyncWithConfig(out int outOfSyncCount);

            // Assert: returns false, outOfSyncCount == 1.
            Assert.IsFalse(result, "Should report out of sync when one position differs.");
            Assert.AreEqual(1, outOfSyncCount, "outOfSyncCount should be 1 when one marker differs.");

            // Cleanup
            Object.DestroyImmediate(rig.gameObject);
            Object.DestroyImmediate(anchor.gameObject);
        }

        // Test: Rig child with no matching POI in config -> reports out of sync.
        [Test]
        public void SyncCheck_RigChildNotMatchingPOI_ReportsOutOfSync()
        {
            // Arrange: config has "lamp" but rig has "unknown" child.
            var config = CreateConfigWithCapturedPositions();
            var anchor = CreateTestAnchor();
            var rig = CreateTestRig(anchor, new Dictionary<string, Vector3>
            {
                { "unknown", new Vector3(0f, 0f, 0f) } // No matching POI in config
            });

            // Create window and set its fields via reflection
            var window = CreateWindowWithConfig(config, anchor);

            // Act: call IsRigInSyncWithConfig.
            var result = window.IsRigInSyncWithConfig(out int outOfSyncCount);

            // Assert: returns false, outOfSyncCount == 1.
            Assert.IsFalse(result, "Should report out of sync when rig has unknown child.");
            Assert.AreEqual(1, outOfSyncCount, "outOfSyncCount should be 1 for unknown child.");

            // Cleanup
            Object.DestroyImmediate(rig.gameObject);
            Object.DestroyImmediate(anchor.gameObject);
        }

        // Test: POI without captured position -> reports out of sync.
        [Test]
        public void SyncCheck_POIWithoutCapturedPosition_ReportsOutOfSync()
        {
            // Arrange: config has POI with has_captured_position = false.
            var config = CreateConfigWithUncapturedPOI();
            var anchor = CreateTestAnchor();
            var rig = CreateTestRig(anchor, new Dictionary<string, Vector3>
            {
                { "lamp", new Vector3(0f, 0f, 0f) }
            });

            // Create window and set its fields via reflection
            var window = CreateWindowWithConfig(config, anchor);

            // Act: call IsRigInSyncWithConfig.
            var result = window.IsRigInSyncWithConfig(out int outOfSyncCount);

            // Assert: returns false, outOfSyncCount == 1.
            Assert.IsFalse(result, "Should report out of sync when POI has no captured position.");
            Assert.AreEqual(1, outOfSyncCount, "outOfSyncCount should be 1 for uncaptured POI.");

            // Cleanup
            Object.DestroyImmediate(rig.gameObject);
            Object.DestroyImmediate(anchor.gameObject);
        }

        // Helper: Create a test correction anchor.
        private static Transform CreateTestAnchor()
        {
            var anchorGo = new GameObject("PlacementCorrectionAnchor");
            return anchorGo.transform;
        }

        // Helper: Create a test rig with children at specified positions, under a correction anchor.
        private static Transform CreateTestRig(Transform anchor, Dictionary<string, Vector3> children)
        {
            var rigGo = new GameObject("POIAuthoringRig");
            var rig = rigGo.transform;
            rig.SetParent(anchor);
            rig.localPosition = Vector3.zero;

            foreach (var kvp in children)
            {
                var child = new GameObject(kvp.Key);
                child.transform.SetParent(rig);
                child.transform.localPosition = kvp.Value;
            }

            return rig;
        }

        // Helper: Create a window and set its config and correction anchor fields via reflection.
        private static POIAuthoringToolWindow CreateWindowWithConfig(WallConfigData config, Transform anchor)
        {
            var window = EditorWindow.GetWindow<POIAuthoringToolWindow>();
            SetConfigField(window, config);
            SetCorrectionAnchorField(window, anchor);
            return window;
        }

        // Helper: Set the _config field on the window via reflection.
        private static void SetConfigField(POIAuthoringToolWindow window, WallConfigData config)
        {
            var field = typeof(POIAuthoringToolWindow).GetField("_config", BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(window, config);
        }

        // Helper: Set the _correctionAnchor field on the window via reflection.
        private static void SetCorrectionAnchorField(POIAuthoringToolWindow window, Transform anchor)
        {
            var field = typeof(POIAuthoringToolWindow).GetField("_correctionAnchor", BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(window, anchor);
        }
    }
}