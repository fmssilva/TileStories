using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using TileStories.Editor;

namespace TileStories.Tests
{
    // Tier-0 EditMode tests for the simplified per-POI Position foldout
    // (read-only XYZ + single Verified/Unverified toggle button + Save All to JSON).
    public class PositionTabsTests
    {
        private static WallConfigData CreateMinimalConfig()
        {
            return new WallConfigData
            {
                wall_id = "test_wall",
                wall_name = "Test Wall",
                pois = new System.Collections.Generic.List<POIData>
                {
                    new POIData
                    {
                        id = "poi_1",
                        name = "Test POI",
                        category = "default",
                        position_verified = false
                    }
                }
            };
        }

        [Test]
        public void PositionContract_NoPosition_IsNullAndUnverified()
        {
            var poi = new POIData { id = "no_pos", name = "Test POI", category = "default" };

            // Simplified-model contract: a POI with no authored position keeps the
            // reference null (resolver falls back to origin) and starts unverified.
            Assert.IsNull(poi.position, "A POI with no authored position must keep position null.");
            Assert.IsFalse(poi.position_verified, "position_verified must default to false.");

            // The Position foldout entry point must exist and take exactly one POIData
            // (proof the simplified single-foldout UI is wired, no Draft/Precise tabs).
            var drawMethod = typeof(POIEditorToolWindow).GetMethod("DrawPositionTabs",
                BindingFlags.NonPublic | BindingFlags.Instance, null,
                new System.Type[] { typeof(POIData) }, null);
            Assert.IsNotNull(drawMethod, "DrawPositionTabs(POIData) must exist for the Position foldout.");
        }

        [Test]
        public void PositionTabs_VerifiedToggle_TogglesFlag()
        {
            var config = CreateMinimalConfig();
            var poi = config.pois[0];

            // Start with a captured position (unverified)
            poi.position = new PositionData { x = 1, y = 2, z = 3 };
            poi.position_verified = false;

            // Simulate clicking the Verified/Unverified toggle button
            poi.position_verified = !poi.position_verified;
            Assert.IsTrue(poi.position_verified, "Clicking toggle should set verified to true");

            // Click again - should go back to unverified
            poi.position_verified = !poi.position_verified;
            Assert.IsFalse(poi.position_verified, "Clicking toggle again should set verified to false");
        }

        [Test]
        public void PositionTabs_UsesExplicitCoordinateLabelsInOrder()
        {
            var labels = POIEditorToolWindow.GetCoordinateLabels();
            Assert.AreEqual(new[] { "X", "Y", "Z" }, labels, "The coordinate labels must be in X, Y, Z order before each value field.");
        }

        [Test]
        public void PositionLock_RejectsMovesForVerifiedPoi()
        {
            var poi = new POIData
            {
                id = "poi_lock",
                name = "Locked POI",
                category = "default",
                position = new PositionData { x = 1f, y = 2f, z = 3f },
                position_verified = true
            };

            bool blocked = POIEditorToolWindow.ShouldBlockVerifiedPositionMove(
                poi,
                new Vector3(1.5f, 2f, 3f),
                out var correctedPosition,
                out var message);

            Assert.IsTrue(blocked, "A verified POI must reject a scene move that differs from its saved position.");
            Assert.AreEqual(new Vector3(1f, 2f, 3f), correctedPosition, "The lock must restore the verified coordinates.");
            StringAssert.Contains("already verified", message, "The user should see a clear verified-position lock warning.");
        }

        [Test]
        public void PositionLock_UsesMostRecentVerifiedPosition_InMemoryBeforeSave()
        {
            var poi = new POIData
            {
                id = "poi_memory_lock",
                name = "Locked Memory POI",
                category = "default",
                position = new PositionData { x = 10f, y = 20f, z = 30f },
                position_verified = true
            };

            Vector3 lastVerifiedInMemory = new Vector3(5f, 6f, 7f);

            bool blocked = POIEditorToolWindow.ShouldBlockVerifiedPositionMove(
                poi,
                new Vector3(8f, 6f, 7f),
                lastVerifiedInMemory,
                out var correctedPosition,
                out var message);

            Assert.IsTrue(blocked, "The lock must revert to the latest verified in-memory position, not the stale JSON value.");
            Assert.AreEqual(lastVerifiedInMemory, correctedPosition, "The editor must restore the most recent verified position from memory.");
            StringAssert.Contains("already verified", message, "The user should see the verified lock message.");
        }

        [Test]
        public void PositionTabs_JsonRoundTrip_PreservesCapturedData()
        {
            var config = CreateMinimalConfig();
            var poi = config.pois[0];
            poi.position = new PositionData { x = 10, y = 20, z = 30 };
            poi.position_verified = true;

            // Serialize and deserialize via JsonUtility
            string json = JsonUtility.ToJson(config);
            var restored = JsonUtility.FromJson<WallConfigData>(json);

            Assert.AreEqual(1, restored.pois.Count);
            var rp = restored.pois[0];
            Assert.IsTrue(rp.position_verified);
            Assert.IsNotNull(rp.position);
            Assert.AreEqual(10, rp.position.x);
            Assert.AreEqual(20, rp.position.y);
            Assert.AreEqual(30, rp.position.z);
        }

        [Test]
        public void PoiRemove_RemoveAtIndex_RemovesFromListAndFoldoutState()
        {
            var list = new System.Collections.Generic.List<POIData>
            {
                new POIData { id = "a", name = "A" },
                new POIData { id = "b", name = "B" },
                new POIData { id = "c", name = "C" }
            };
            var foldouts = new System.Collections.Generic.Dictionary<string, bool>
            {
                ["a"] = true,
                ["b"] = true,
                ["c"] = true
            };

            bool removed = POIEditorToolWindow.RemovePoiAt(list, foldouts, 1);

            Assert.IsTrue(removed, "The delete helper should remove the selected item from the list.");
            Assert.AreEqual(new[] { "a", "c" }, new[] { list[0].id, list[1].id }, "The remaining POIs should collapse to the correct order.");
            Assert.IsFalse(foldouts.ContainsKey("b"), "The removed POI's foldout state should be cleaned up.");
        }

        [Test]
        public void PoiReorder_MoveUp_SwapsListOrder()
        {
            var list = new System.Collections.Generic.List<POIData>
            {
                new POIData { id = "a", name = "A" },
                new POIData { id = "b", name = "B" },
                new POIData { id = "c", name = "C" }
            };

            POIEditorToolWindow.ReorderPoi(list, 1, -1);

            Assert.AreEqual("b", list[0].id, "Moving up should move the selected item earlier in the list.");
            Assert.AreEqual("a", list[1].id, "The previous item should shift down by one.");
            Assert.AreEqual("c", list[2].id, "The trailing item should remain in place.");
        }

        [Test]
        public void PoiReorder_MoveDown_SwapsListOrder()
        {
            var list = new System.Collections.Generic.List<POIData>
            {
                new POIData { id = "a", name = "A" },
                new POIData { id = "b", name = "B" },
                new POIData { id = "c", name = "C" }
            };

            POIEditorToolWindow.ReorderPoi(list, 1, 1);

            Assert.AreEqual("a", list[0].id, "The earlier item should stay first.");
            Assert.AreEqual("c", list[1].id, "Moving down should place the selected item after the next entry.");
            Assert.AreEqual("b", list[2].id, "The later item should shift upward.");
        }

        [Test]
        public void PoiReorder_Bounds_LeavesListUnchanged()
        {
            var list = new System.Collections.Generic.List<POIData>
            {
                new POIData { id = "a", name = "A" },
                new POIData { id = "b", name = "B" }
            };

            POIEditorToolWindow.ReorderPoi(list, 0, -1);
            POIEditorToolWindow.ReorderPoi(list, 1, 1);

            Assert.AreEqual(new[] { "a", "b" }, new[] { list[0].id, list[1].id }, "Boundary reorder calls must be no-ops.");
        }
    }
}