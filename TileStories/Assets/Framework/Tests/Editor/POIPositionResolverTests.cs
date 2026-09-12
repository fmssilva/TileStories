using UnityEngine.TestTools;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Tests
{
    // Tests for the simplified POIPositionResolver: captured-position precedence
    // plus a safe origin fallback for POIs with no capture. The old x_norm/y_norm
    // interpolation path and calibration-anchor system have been removed.
    public class POIPositionResolverTests
    {
        // A POI with captured_position present must use it regardless of value —
        // including the origin-point case [0,0,0] which must NOT be treated as missing.
        [Test]
        public void ResolvePosition_WithCapturedPosition_ReturnsCapturedValue()
        {
            var poi = new POIData
            {
                id = "test_poi",
                captured_position = new CapturedPosition { x = 1.5f, y = 2.0f, z = -0.5f },
                has_captured_position = true
            };


            bool result = POIPositionResolver.TryResolvePosition(poi, out Vector3 pos);

            Assert.IsTrue(result, "Should resolve successfully when captured_position is present.");
            Assert.AreEqual(1.5f, pos.x, 0.001f, "X should match captured_position.x");
            Assert.AreEqual(2.0f, pos.y, 0.001f, "Y should match captured_position.y");
            Assert.AreEqual(-0.5f, pos.z, 0.001f, "Z should match captured_position.z");
        }

        // A POI captured exactly at the origin (0,0,0) must still resolve as captured,
        // never falling through to the x_norm/y_norm fallback.
        [Test]
        public void ResolvePosition_WithCapturedPositionAtOrigin_ReturnsOrigin()
        {
            var poi = new POIData
            {
                id = "origin_poi",
                captured_position = new CapturedPosition { x = 0f, y = 0f, z = 0f },
                has_captured_position = true
            };

            bool result = POIPositionResolver.TryResolvePosition(poi, out Vector3 pos);

            Assert.IsTrue(result, "Should resolve successfully when captured_position is (0,0,0).");
            Assert.AreEqual(0f, pos.x, 0.001f);
            Assert.AreEqual(0f, pos.y, 0.001f);
            Assert.AreEqual(0f, pos.z, 0.001f);
        }

        // A POI with no captured_position falls back to origin (0,0,0).
        [Test]
        public void ResolvePosition_WithoutCapturedPosition_ReturnsOriginFallback()
        {
            var poi = new POIData
            {
                id = "fallback_poi",
                captured_position = null,  // no captured position -> origin fallback
                has_captured_position = false
            };

            bool result = POIPositionResolver.TryResolvePosition(poi, out Vector3 pos);

            Assert.IsTrue(result, "Should resolve via origin fallback when captured_position is null.");
            Assert.AreEqual(Vector3.zero, pos, "Fallback position must be origin.");
        }

        // A POI with has_captured_position=false but a non-null captured_position
        // must still fall back to origin (the flag is authoritative).
        [Test]
        public void ResolvePosition_FlagFalseIgnoresCapturedObject()
        {
            var poi = new POIData
            {
                id = "stale_capture_poi",
                captured_position = new CapturedPosition { x = 1f, y = 2f, z = 3f },
                has_captured_position = false
            };

            bool result = POIPositionResolver.TryResolvePosition(poi, out Vector3 pos);

            Assert.IsTrue(result, "Should still resolve via origin fallback.");
            Assert.AreEqual(Vector3.zero, pos, "has_captured_position=false means ignore captured_position.");
        }

        // Null POI data should return false.
        [Test]
        public void ResolvePosition_NullPOI_ReturnsFalse()
        {
            bool result = POIPositionResolver.TryResolvePosition(null, out Vector3 pos, logErrors: false);
            Assert.IsFalse(result, "Null POI should return false.");
            Assert.AreEqual(Vector3.zero, pos);
        }

        // NaN in captured_position should return false.
        [Test]
        public void ResolvePosition_NaNInCapturedPosition_ReturnsFalse()
        {
            var poi = new POIData
            {
                id = "nan_captured",
                captured_position = new CapturedPosition { x = float.NaN, y = 0f, z = 0f },
                has_captured_position = true
            };

            bool result = POIPositionResolver.TryResolvePosition(poi, out Vector3 pos, logErrors: false);
            Assert.IsFalse(result, "NaN in captured_position should return false.");
        }

        // An uncaptured POI still resolves via the origin fallback.
        [Test]
        public void ResolvePosition_Uncaptured_ReturnsOriginFallback()
        {
            var poi = new POIData
            {
                id = "no_capture",
                captured_position = null,
                has_captured_position = false
            };

            bool result = POIPositionResolver.TryResolvePosition(poi, out Vector3 pos);
            Assert.IsTrue(result, "Should still resolve via origin fallback.");
            Assert.AreEqual(Vector3.zero, pos, "Fallback must be origin.");
        }

        // Verifies the save/load contract for uncaptured POIs: the presence flag stays
        // false across JsonUtility round-trip, so runtime code continues to ignore any
        // synthesized nested object and correctly uses the fallback path.
        [Test]
        public void JsonRoundTrip_UncapturedPOI_PreservesPresenceFlagSemantics()
        {
            var config = new WallConfigData
            {
                wall_id = "test_wall",
                pois = new System.Collections.Generic.List<POIData>
                {
                    new POIData
                    {
                        id = "uncaptured_poi",
                        name = "Test POI",
                        captured_position = null, // never captured - this is the case under test
                        has_captured_position = false
                    }
                }
            };

            string json = JsonUtility.ToJson(config);
            var reloaded = JsonUtility.FromJson<WallConfigData>(json);
            var poi = reloaded.pois.Find(p => p.id == "uncaptured_poi");

            Assert.IsNotNull(poi, "The POI itself should survive the round-trip.");
            Assert.IsFalse(poi.has_captured_position,
                "has_captured_position must remain false after a JSON round-trip for an " +
                "uncaptured POI. This flag is the authoritative signal that runtime code " +
                "uses to decide whether captured_position is meaningful.");

            bool resolved = POIPositionResolver.TryResolvePosition(poi, out Vector3 pos);

            Assert.IsTrue(resolved, "Round-tripped uncaptured POI should still resolve via origin fallback.");
            Assert.AreEqual(Vector3.zero, pos,
                "Resolver should ignore any synthesized nested object when has_captured_position is false.");
        }
    }
}