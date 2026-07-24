using UnityEngine.TestTools;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Tests
{
    // Tests for POIPositionResolver: captured_position precedence, fallback interpolation,
    // and invalid-data handling.
    public class POIPositionResolverTests
    {
        // A POI with captured_position present must use it regardless of value —
        // including the origin-point case [0,0,0] which must NOT fall through to the fallback.
        [Test]
        public void ResolvePosition_WithCapturedPosition_ReturnsCapturedValue()
        {
            var poi = new POIData
            {
                id = "test_poi",
                x_norm = 0.5f,
                y_norm = 0.5f,
                captured_position = new CapturedPosition { x = 1.5f, y = 2.0f, z = -0.5f },
                has_captured_position = true
            };

            bool result = POIPositionResolver.TryResolvePosition(poi, null, out Vector3 pos);

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
                x_norm = 0.5f,
                y_norm = 0.5f,
                captured_position = new CapturedPosition { x = 0f, y = 0f, z = 0f },
                has_captured_position = true
            };

            bool result = POIPositionResolver.TryResolvePosition(poi, null, out Vector3 pos);

            Assert.IsTrue(result, "Should resolve successfully when captured_position is (0,0,0).");
            Assert.AreEqual(0f, pos.x, 0.001f);
            Assert.AreEqual(0f, pos.y, 0.001f);
            Assert.AreEqual(0f, pos.z, 0.001f);
        }

        // A POI with no captured_position should use the calibration-anchor fallback.
        // Given two anchors at x_norm=0 and x_norm=1, a POI at x_norm=0.5 should land
        // halfway between them in X, with y offset from y_norm.
        [Test]
        public void ResolvePosition_WithoutCapturedPosition_UsesInterpolationFallback()
        {
            var poi = new POIData
            {
                id = "interp_poi",
                x_norm = 0.5f,
                y_norm = 0.5f,
                captured_position = null,  // no captured position -> use fallback
                has_captured_position = false
            };

            var anchors = new CalibrationAnchor[]
            {
                new CalibrationAnchor
                {
                    id = "cal_left",
                    x_norm = 0f,
                    y_norm = 0.5f,
                    captured_position = new CapturedPosition { x = 0f, y = 0f, z = 0f }
                },
                new CalibrationAnchor
                {
                    id = "cal_right",
                    x_norm = 1f,
                    y_norm = 0.5f,
                    captured_position = new CapturedPosition { x = 4f, y = 0f, z = 0f }
                }
            };

            bool result = POIPositionResolver.TryResolvePosition(poi, anchors, out Vector3 pos);

            Assert.IsTrue(result, "Should resolve via fallback when captured_position is null.");
            Assert.AreEqual(2.0f, pos.x, 0.001f, "Should be halfway between 0 and 4.");
            Assert.AreEqual(0f, pos.y, 0.001f, "y_norm=0.5 with anchor y spread should give 0.");
        }

        // A POI at the low edge of x_norm should return position near the left anchor.
        [Test]
        public void ResolvePosition_WithoutCapturedPosition_EdgeCaseLowX()
        {
            var poi = new POIData
            {
                id = "low_poi",
                x_norm = 0.0f,
                y_norm = 0.5f,
                captured_position = null,
                has_captured_position = false
            };

            var anchors = new CalibrationAnchor[]
            {
                new CalibrationAnchor
                {
                    id = "cal_left",
                    x_norm = 0f,
                    y_norm = 0.5f,
                    captured_position = new CapturedPosition { x = 0f, y = 0f, z = 0f }
                },
                new CalibrationAnchor
                {
                    id = "cal_right",
                    x_norm = 1f,
                    y_norm = 0.5f,
                    captured_position = new CapturedPosition { x = 4f, y = 0f, z = 0f }
                }
            };

            bool result = POIPositionResolver.TryResolvePosition(poi, anchors, out Vector3 pos);

            Assert.IsTrue(result);
            Assert.AreEqual(0f, pos.x, 0.001f, "x_norm=0 should give left anchor x.");
        }

        // A POI at the high edge of x_norm should return position near the right anchor.
        [Test]
        public void ResolvePosition_WithoutCapturedPosition_EdgeCaseHighX()
        {
            var poi = new POIData
            {
                id = "high_poi",
                x_norm = 1.0f,
                y_norm = 0.5f,
                captured_position = null,
                has_captured_position = false
            };

            var anchors = new CalibrationAnchor[]
            {
                new CalibrationAnchor
                {
                    id = "cal_left",
                    x_norm = 0f,
                    y_norm = 0.5f,
                    captured_position = new CapturedPosition { x = 0f, y = 0f, z = 0f }
                },
                new CalibrationAnchor
                {
                    id = "cal_right",
                    x_norm = 1f,
                    y_norm = 0.5f,
                    captured_position = new CapturedPosition { x = 4f, y = 0f, z = 0f }
                }
            };

            bool result = POIPositionResolver.TryResolvePosition(poi, anchors, out Vector3 pos);

            Assert.IsTrue(result);
            Assert.AreEqual(4f, pos.x, 0.001f, "x_norm=1 should give right anchor x.");
        }

        // Null POI data should return false.
        [Test]
        public void ResolvePosition_NullPOI_ReturnsFalse()
        {
            bool result = POIPositionResolver.TryResolvePosition(null, null, out Vector3 pos, logErrors: false);
            Assert.IsFalse(result, "Null POI should return false.");
            Assert.AreEqual(Vector3.zero, pos);
        }

        // NaN in x_norm should return false (invalid data, not silently tolerated).
        [Test]
        public void ResolvePosition_NaNxNorm_ReturnsFalse()
        {
            var poi = new POIData
            {
                id = "nan_poi",
                x_norm = float.NaN,
                y_norm = 0.5f,
                captured_position = null,
                has_captured_position = false
            };

            var anchors = new CalibrationAnchor[]
            {
                new CalibrationAnchor
                {
                    id = "cal_left",
                    x_norm = 0f,
                    y_norm = 0.5f,
                    captured_position = new CapturedPosition { x = 0f, y = 0f, z = 0f }
                },
                new CalibrationAnchor
                {
                    id = "cal_right",
                    x_norm = 1f,
                    y_norm = 0.5f,
                    captured_position = new CapturedPosition { x = 4f, y = 0f, z = 0f }
                }
            };

            bool result = POIPositionResolver.TryResolvePosition(poi, anchors, out Vector3 pos, logErrors: false);
            Assert.IsFalse(result, "NaN x_norm should return false.");
        }

        // NaN in captured_position should return false.
        [Test]
        public void ResolvePosition_NaNInCapturedPosition_ReturnsFalse()
        {
            var poi = new POIData
            {
                id = "nan_captured",
                x_norm = 0.5f,
                y_norm = 0.5f,
                captured_position = new CapturedPosition { x = float.NaN, y = 0f, z = 0f },
                has_captured_position = true
            };

            bool result = POIPositionResolver.TryResolvePosition(poi, null, out Vector3 pos, logErrors: false);
            Assert.IsFalse(result, "NaN in captured_position should return false.");
        }

// With zero calibration anchors, fallback should still return a position (flat plane default).
        [Test]
        public void ResolvePosition_NoCalibrationAnchors_ReturnsFlatDefault()
        {
            var poi = new POIData
            {
                id = "no_anchors",
                x_norm = 0.5f,
                y_norm = 0.5f,
                captured_position = null,
                has_captured_position = false
            };

            bool result = POIPositionResolver.TryResolvePosition(poi, null, out Vector3 pos);
            Assert.IsTrue(result, "Should still resolve with no anchors (flat default).");
            Assert.IsFalse(float.IsNaN(pos.x), "X should not be NaN.");
            Assert.IsFalse(float.IsNaN(pos.y), "Y should not be NaN.");
            Assert.IsFalse(float.IsNaN(pos.z), "Z should not be NaN.");
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
                        x_norm = 0.5f,
                        y_norm = 0.5f,
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

            var anchors = new CalibrationAnchor[]
            {
                new CalibrationAnchor
                {
                    id = "cal_left",
                    x_norm = 0f,
                    y_norm = 0.5f,
                    captured_position = new CapturedPosition { x = 0f, y = 0f, z = 0f }
                },
                new CalibrationAnchor
                {
                    id = "cal_right",
                    x_norm = 1f,
                    y_norm = 0.5f,
                    captured_position = new CapturedPosition { x = 4f, y = 0f, z = 0f }
                }
            };

            bool resolved = POIPositionResolver.TryResolvePosition(poi, anchors, out Vector3 pos);

            Assert.IsTrue(resolved, "Round-tripped uncaptured POI should still resolve via fallback.");
            Assert.AreEqual(2f, pos.x, 0.001f,
                "Resolver should ignore any synthesized nested object when has_captured_position is false.");
        }
    }
}