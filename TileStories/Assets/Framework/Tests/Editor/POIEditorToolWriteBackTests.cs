using NUnit.Framework;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // Tests for the POI Editor's read-modify-write cycle.
    // Pure data tests — no scene, no Editor window, no Unity dependencies.
    public class POIEditorToolWriteBackTests
    {
        // Simulate the Capture Positions operation: given a config with 3 POIs
        // and a set of "placed" scene objects matching only 2 of them,
        // only those 2 POIs should have their position updated.
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

                poi.position = new PositionData
                {
                    x = 1.0f,
                    y = 2.0f,
                    z = 3.0f
                };
                captured++;
            }

            Assert.AreEqual(2, captured, "Should capture 2 POIs.");
            Assert.AreEqual(1, skipped, "Should skip 1 POI (camera).");

            // Verify captured POIs have the updated position
            var lamp = config.pois.Find(p => p.id == "lamp");
            Assert.IsNotNull(lamp.position, "lamp should have position.");
            Assert.AreEqual(1.0f, lamp.position.x, 0.001f);

            var painting = config.pois.Find(p => p.id == "painting");
            Assert.IsNotNull(painting.position, "painting should have position.");
            Assert.AreEqual(1.0f, painting.position.x, 0.001f);

            // Verify the unmatched POI was NOT touched
            var camera = config.pois.Find(p => p.id == "camera");
            Assert.IsNull(camera.position, "camera should NOT have position (was not in scene).");
        }

        // Verify that capturing only one POI doesn't affect other POIs' fields.
        [Test]
        public void CapturePositions_PartialCapture_DoesNotAffectOthers()
        {
            var config = CreateTestConfig();

            // Simulate capturing only "lamp"
            var lamp = config.pois.Find(p => p.id == "lamp");
            lamp.position = new PositionData { x = 1f, y = 2f, z = 3f };

            // Verify lamp has position
            Assert.IsNotNull(lamp.position);
            Assert.AreEqual(1f, lamp.position.x);

            // Verify painting and camera were NOT touched
            var painting = config.pois.Find(p => p.id == "painting");
            Assert.IsNull(painting.position, "painting should be untouched.");

            var camera = config.pois.Find(p => p.id == "camera");
            Assert.IsNull(camera.position, "camera should be untouched.");
        }

        // Verify that capturing a POI at the origin (0,0,0) is preserved correctly.
        [Test]
        public void CapturePositions_OriginPosition_IsStored()
        {
            var config = CreateTestConfig();

            var lamp = config.pois.Find(p => p.id == "lamp");
            lamp.position = new PositionData { x = 0f, y = 0f, z = 0f };

            Assert.IsNotNull(lamp.position, "position should exist (not null).");
            Assert.AreEqual(0f, lamp.position.x, 0.001f);
            Assert.AreEqual(0f, lamp.position.y, 0.001f);
            Assert.AreEqual(0f, lamp.position.z, 0.001f);
        }

        private static WallConfigData CreateTestConfig()
        {
            return new WallConfigData
            {
                wall_id = "test_wall",
                wall_name = "Test Wall",
                immersal_map_id = 12345,
                pois = new List<POIData>
                {
                    // lamp starts captured (previously placed in a scene);
                    // painting and camera start uncaptured so write-back tests
                    // can assert they are only touched when actually matched.
                    new POIData { id = "lamp", name = "The Lamp", position_verified = true, position = new PositionData { x = 0.1f, y = 0f, z = 0.1f } },
                    new POIData { id = "painting", name = "The Painting", position_verified = false, position = null },
                    new POIData { id = "camera", name = "The Camera", position_verified = false, position = null }
                }
            };
        }
    }

    // Tests for the sync-check method in POIEditorToolWindow.
    // These tests use Unity's EditMode test framework to create scene objects.
    public class POIEditorToolWindowSyncCheckTests
    {
        // Helper to create a test config with captured positions.
        private static WallConfigData CreateConfigWithPositionDatas()
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
                        position = new PositionData { x = 1.0f, y = 2.0f, z = 3.0f },
                        position_verified = true
                    },
                    new POIData
                    {
                        id = "painting",
                        name = "The Painting",
                        position = new PositionData { x = 4.0f, y = 5.0f, z = 6.0f },
                        position_verified = true
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
                        position = null,
                        position_verified = false
                    }
                }
            };
        }

        // Test: All positions match -> reports in sync.
        [Test]
        public void SyncCheck_AllPositionsMatch_ReportsInSync()
        {
            // Arrange: create a rig with children at known positions, and a config
            // whose matching POIs have position_verified = true at those same positions.
            var config = CreateConfigWithPositionDatas();
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
            var config = CreateConfigWithPositionDatas();
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
            var config = CreateConfigWithPositionDatas();
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
        public void SyncCheck_POIWithoutPositionData_ReportsOutOfSync()
        {
            // Arrange: config has POI with position_verified = false.
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
            var rigGo = new GameObject("POIEditorRig");
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

        // Unshown window instance -- CreateInstance never docks a tab in the
        // user's layout. EditorWindow.GetWindow would park a real tab whose
        // HostView leaves a stale reference and NREs in GetExtraButtonsWidth
        // while repainting the tab strip.
        private static POIEditorToolWindow _testWindow;

        // Helper: Create an unshown window and set its config and correction anchor fields.
        private static POIEditorToolWindow CreateWindowWithConfig(WallConfigData config, Transform anchor)
        {
            _testWindow = ScriptableObject.CreateInstance<POIEditorToolWindow>();
            SetConfigField(_testWindow, config);
            SetCorrectionAnchorField(_testWindow, anchor);
            return _testWindow;
        }

        [TearDown]
        public void TeardownTestWindow()
        {
            if (_testWindow != null)
            {
                Object.DestroyImmediate(_testWindow);
                _testWindow = null;
            }
        }

        // Helper: Set the _config field on the window via reflection.
        private static void SetConfigField(POIEditorToolWindow window, WallConfigData config)
        {
            var field = typeof(POIEditorToolWindow).GetField("_config", BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(window, config);
        }

        // Helper: Set the _correctionAnchor field on the window via reflection.
        private static void SetCorrectionAnchorField(POIEditorToolWindow window, Transform anchor)
        {
            var field = typeof(POIEditorToolWindow).GetField("_correctionAnchor", BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(window, anchor);
        }
    }
}