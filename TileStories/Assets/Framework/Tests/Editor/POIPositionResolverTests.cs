using UnityEngine.TestTools;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Tests
{
    // Tests for the simplified POIPositionResolver: non-null position is used,
    // null position falls back to origin. position_verified is editor-only QA.
    public class POIPositionResolverTests
    {
        [Test]
        public void ResolvePosition_WithPosition_ReturnsValue()
        {
            var poi = new POIData
            {
                id = "test_poi",
                position = new PositionData { x = 1.5f, y = 2.0f, z = -0.5f },
                position_verified = true
            };

            bool result = POIPositionResolver.TryResolvePosition(poi, out Vector3 pos);

            Assert.IsTrue(result, "Should resolve successfully when position is present.");
            Assert.AreEqual(1.5f, pos.x, 0.001f, "X should match position.x");
            Assert.AreEqual(2.0f, pos.y, 0.001f, "Y should match position.y");
            Assert.AreEqual(-0.5f, pos.z, 0.001f, "Z should match position.z");
        }

        [Test]
        public void ResolvePosition_WithPositionAtOrigin_ReturnsOrigin()
        {
            var poi = new POIData
            {
                id = "origin_poi",
                position = new PositionData { x = 0f, y = 0f, z = 0f },
                position_verified = true
            };

            bool result = POIPositionResolver.TryResolvePosition(poi, out Vector3 pos);

            Assert.IsTrue(result, "Should resolve successfully when position is (0,0,0).");
            Assert.AreEqual(0f, pos.x, 0.001f);
            Assert.AreEqual(0f, pos.y, 0.001f);
            Assert.AreEqual(0f, pos.z, 0.001f);
        }

        [Test]
        public void ResolvePosition_WithoutPosition_ReturnsOriginFallback()
        {
            var poi = new POIData
            {
                id = "fallback_poi",
                position = null,
                position_verified = false
            };

            bool result = POIPositionResolver.TryResolvePosition(poi, out Vector3 pos);

            Assert.IsTrue(result, "Should resolve via origin fallback when position is null.");
            Assert.AreEqual(Vector3.zero, pos, "Fallback position must be origin.");
        }

        // position_verified is editor-only QA: it must never affect runtime resolution.
        [Test]
        public void ResolvePosition_UnverifiedButPresentPosition_IsUsed()
        {
            var poi = new POIData
            {
                id = "unverified_poi",
                position = new PositionData { x = 1f, y = 2f, z = 3f },
                position_verified = false
            };

            bool result = POIPositionResolver.TryResolvePosition(poi, out Vector3 pos);

            Assert.IsTrue(result, "Unverified position must still resolve; verified is editor-only.");
            Assert.AreEqual(1f, pos.x, 0.001f);
            Assert.AreEqual(2f, pos.y, 0.001f);
            Assert.AreEqual(3f, pos.z, 0.001f);
        }

        [Test]
        public void ResolvePosition_NullPOI_ReturnsFalse()
        {
            bool result = POIPositionResolver.TryResolvePosition(null, out Vector3 pos, logErrors: false);
            Assert.IsFalse(result, "Null POI should return false.");
            Assert.AreEqual(Vector3.zero, pos);
        }

        [Test]
        public void ResolvePosition_NaNInPosition_ReturnsFalse()
        {
            var poi = new POIData
            {
                id = "nan_position",
                position = new PositionData { x = float.NaN, y = 0f, z = 0f },
                position_verified = true
            };

            bool result = POIPositionResolver.TryResolvePosition(poi, out Vector3 pos, logErrors: false);
            Assert.IsFalse(result, "NaN in position should return false.");
        }

        // A POI with no authored position must resolve to origin, whether the field is
        // null or synthesized as (0,0,0) by JsonUtility. Under the simplified model,
        // null vs origin is functionally equivalent -- both resolve to origin.
        [Test]
        public void JsonRoundTrip_NoPosition_ResolvesToOrigin()
        {
            var config = new WallConfigData
            {
                wall_id = "test_wall",
                pois = new System.Collections.Generic.List<POIData>
                {
                    new POIData
                    {
                        id = "no_position_poi",
                        name = "Test POI",
                        position = null,
                        position_verified = false
                    }
                }
            };

            string json = JsonUtility.ToJson(config);
            var reloaded = JsonUtility.FromJson<WallConfigData>(json);
            var poi = reloaded.pois.Find(p => p.id == "no_position_poi");

            Assert.IsNotNull(poi, "The POI itself should survive the round-trip.");

            bool resolved = POIPositionResolver.TryResolvePosition(poi, out Vector3 pos);

            Assert.IsTrue(resolved, "Round-tripped POI without position should resolve.");
            Assert.AreEqual(Vector3.zero, pos, "Resolver should fall back to origin.");
        }

        [Test]
        public void JsonRoundTrip_Position_PreservesValue()
        {
            var config = new WallConfigData
            {
                wall_id = "test_wall",
                pois = new System.Collections.Generic.List<POIData>
                {
                    new POIData
                    {
                        id = "position_poi",
                        name = "Test POI",
                        position = new PositionData { x = 1f, y = 2f, z = 3f },
                        position_verified = true
                    }
                }
            };

            string json = JsonUtility.ToJson(config);
            var reloaded = JsonUtility.FromJson<WallConfigData>(json);
            var poi = reloaded.pois.Find(p => p.id == "position_poi");

            Assert.IsNotNull(poi);
            Assert.IsNotNull(poi.position);
            Assert.AreEqual(1f, poi.position.x, 0.001f);
            Assert.AreEqual(2f, poi.position.y, 0.001f);
            Assert.AreEqual(3f, poi.position.z, 0.001f);
            Assert.IsTrue(poi.position_verified);
        }
    }
}
