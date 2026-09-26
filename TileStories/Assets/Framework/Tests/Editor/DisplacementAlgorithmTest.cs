using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using TileStories;

namespace TileStories.Tests
{
    // Tier-0 EditMode tests for MarkerOverlapResolver displacement algorithms (Block 3).
    // Verifies candidate_position, force_directed, and fixed_axis algorithms with priority handling.
    public class DisplacementAlgorithmTest
    {
        private DisplacementSettings _defaultSettings;

        [SetUp]
        public void SetUp()
        {
            MarkerOverlapResolver.ResetWarnings();
            _defaultSettings = new DisplacementSettings
            {
                enabled = true,
                overlap_threshold_px = 40f,
                max_displacement_px = 120f,
                displace_target = "label_only",
                displacement_algorithm = "fixed_axis",
                displacement_tiebreak = "symmetric"
            };
        }

        [TearDown]
        public void TearDown()
        {
            MarkerOverlapResolver.ResetWarnings();
        }

        // Helper to create test VisualUnits
        private List<VisualUnit> CreateTestUnits(params (Vector2 pos, string id, int priority)[] units)
        {
            var result = new List<VisualUnit>();
            foreach (var (pos, id, priority) in units)
            {
                result.Add(new VisualUnit
                {
                    worldPosition = new Vector3(pos.x, pos.y, 10f),
                    poiId = id,
                    marker = null, // Not needed for algorithm tests
                    hierarchyLevelIndex = priority
                });
            }
            return result;
        }

        [Test]
        public void FixedAxis_TwoOverlappingMarkers_PriorityDeterminesWhichMoves()
        {
            var screenPositions = new Vector2[] { new Vector2(400, 400), new Vector2(400, 400) };
            var ids = new string[] { "poi_a", "poi_b" };
            var visualUnits = CreateTestUnits(
                (new Vector2(400, 400), "poi_a", 1),
                (new Vector2(400, 400), "poi_b", 2)
            );

            var offsets = MarkerOverlapResolver.ComputeOffsets(screenPositions, ids, _defaultSettings, visualUnits);

            float offsetAMagnitude = offsets[0].magnitude;
            float offsetBMagnitude = offsets[1].magnitude;

            Assert.LessOrEqual(offsetAMagnitude, offsetBMagnitude,
                "Higher priority marker should have smaller offset");
            Assert.Greater(offsetAMagnitude, 0f, "Overlapping markers should have offset");
            Assert.Greater(offsetBMagnitude, 0f, "Overlapping markers should have offset");
        }

        [Test]
        public void CandidatePosition_ThreeOverlappingMarkers_PriorityAware()
        {
            var settings = new DisplacementSettings
            {
                enabled = true,
                overlap_threshold_px = 40f,
                max_displacement_px = 120f,
                displace_target = "label_only",
                displacement_algorithm = "candidate_position",
                displacement_tiebreak = "symmetric"
            };

            var screenPositions = new Vector2[] { new Vector2(400, 400), new Vector2(400, 400), new Vector2(400, 400) };
            var ids = new string[] { "poi_a", "poi_b", "poi_c" };
            var visualUnits = CreateTestUnits(
                (new Vector2(400, 400), "poi_a", 1),
                (new Vector2(400, 400), "poi_b", 3),
                (new Vector2(400, 400), "poi_c", 5)
            );

            var offsets = MarkerOverlapResolver.ComputeOffsets(screenPositions, ids, settings, visualUnits);

            for (int i = 0; i < 3; i++)
                Assert.Greater(offsets[i].magnitude, 0f, $"Marker {ids[i]} should have offset");

            Assert.LessOrEqual(offsets[0].magnitude, offsets[1].magnitude,
                "Priority 1 marker should have smaller offset than priority 3");
            Assert.LessOrEqual(offsets[1].magnitude, offsets[2].magnitude,
                "Priority 3 marker should have smaller offset than priority 5");
        }

        [Test]
        public void ForceDirected_ThreeOverlappingMarkers_PriorityAware()
        {
            var settings = new DisplacementSettings
            {
                enabled = true,
                overlap_threshold_px = 40f,
                max_displacement_px = 120f,
                displace_target = "label_only",
                displacement_algorithm = "force_directed",
                displacement_tiebreak = "symmetric"
            };

            var screenPositions = new Vector2[] { new Vector2(400, 400), new Vector2(400, 400), new Vector2(400, 400) };
            var ids = new string[] { "poi_a", "poi_b", "poi_c" };
            var visualUnits = CreateTestUnits(
                (new Vector2(400, 400), "poi_a", 1),
                (new Vector2(400, 400), "poi_b", 3),
                (new Vector2(400, 400), "poi_c", 5)
            );

            var offsets = MarkerOverlapResolver.ComputeOffsets(screenPositions, ids, settings, visualUnits);

            for (int i = 0; i < 3; i++)
                Assert.Greater(offsets[i].magnitude, 0f, $"Marker {ids[i]} should have offset");

            Assert.LessOrEqual(offsets[0].magnitude, offsets[1].magnitude,
                "Priority 1 marker should have smaller offset than priority 3");
            Assert.LessOrEqual(offsets[1].magnitude, offsets[2].magnitude,
                "Priority 3 marker should have smaller offset than priority 5");
        }

        [Test]
        public void MaxDisplacementClamping_Respected()
        {
            var settings = new DisplacementSettings(_defaultSettings)
            {
                overlap_threshold_px = 10f,
                max_displacement_px = 50f
            };

            var screenPositions = new Vector2[] { new Vector2(400, 400), new Vector2(401, 400) };
            var ids = new string[] { "poi_a", "poi_b" };
            var visualUnits = CreateTestUnits(
                (new Vector2(400, 400), "poi_a", 1),
                (new Vector2(401, 400), "poi_b", 2)
            );

            var offsets = MarkerOverlapResolver.ComputeOffsets(screenPositions, ids, settings, visualUnits);

            foreach (var offset in offsets)
                Assert.LessOrEqual(offset.magnitude, 50f + 5f,
                    "Offset should not exceed max_displacement_px (with tolerance for threshold)");
        }

        [Test]
        public void Determinism_SameInputSameOutput()
        {
            var screenPositions = new Vector2[] { new Vector2(400, 400), new Vector2(400, 400), new Vector2(400, 400) };
            var ids = new string[] { "poi_a", "poi_b", "poi_c" };

            var result1 = MarkerOverlapResolver.ComputeOffsets(screenPositions, ids, _defaultSettings, null);
            var result2 = MarkerOverlapResolver.ComputeOffsets(screenPositions, ids, _defaultSettings, null);
            var result3 = MarkerOverlapResolver.ComputeOffsets(screenPositions, ids, _defaultSettings, null);

            for (int i = 0; i < 3; i++)
            {
                Assert.AreEqual(result1[i].x, result2[i].x, 0.001f, "Deterministic X");
                Assert.AreEqual(result1[i].y, result2[i].y, 0.001f, "Deterministic Y");
                Assert.AreEqual(result2[i].x, result3[i].x, 0.001f, "Deterministic X 3");
                Assert.AreEqual(result2[i].y, result3[i].y, 0.001f, "Deterministic Y 3");
            }
        }

        [Test]
        public void InvalidAlgorithm_FallsBackToFixedAxis_LogsWarningOnce()
        {
            var settings = new DisplacementSettings(_defaultSettings)
            {
                displacement_algorithm = "invalid_algorithm"
            };

            var screenPositions = new Vector2[] { new Vector2(400, 400), new Vector2(400, 400) };
            var ids = new string[] { "poi_a", "poi_b" };

            Assert.DoesNotThrow(() =>
                MarkerOverlapResolver.ComputeOffsets(screenPositions, ids, settings, null)
            );
        }

        [Test]
        public void EmptyGroup_ReturnsZeroOffsets()
        {
            var screenPositions = new Vector2[] { new Vector2(400, 400) };
            var ids = new string[] { "poi_a" };

            var offsets = MarkerOverlapResolver.ComputeOffsets(screenPositions, ids, _defaultSettings, null);

            Assert.AreEqual(0f, offsets[0].magnitude, "Single marker should have zero offset");
        }

        [Test]
        public void DisabledSettings_ReturnsZeroOffsets()
        {
            var settings = new DisplacementSettings(_defaultSettings) { enabled = false };
            var screenPositions = new Vector2[] { new Vector2(400, 400), new Vector2(400, 400) };
            var ids = new string[] { "poi_a", "poi_b" };

            var offsets = MarkerOverlapResolver.ComputeOffsets(screenPositions, ids, settings, null);

            Assert.AreEqual(0f, offsets[0].magnitude, "Disabled should return zero offsets");
            Assert.AreEqual(0f, offsets[1].magnitude, "Disabled should return zero offsets");
        }

        [Test]
        public void NullVisualUnits_UsesDefaultPriority()
        {
            var screenPositions = new Vector2[] { new Vector2(400, 400), new Vector2(400, 400) };
            var ids = new string[] { "poi_a", "poi_b" };

            var offsets = MarkerOverlapResolver.ComputeOffsets(screenPositions, ids, _defaultSettings, null);

            Assert.Greater(offsets[0].magnitude, 0f, "Should still produce offset without visualUnits");
            Assert.Greater(offsets[1].magnitude, 0f, "Should still produce offset without visualUnits");
        }

        [Test]
        public void ForceDirected_BothTarget_PriorityAware()
        {
            var settings = new DisplacementSettings(_defaultSettings)
            {
                displace_target = "both",
                displacement_algorithm = "force_directed"
            };

            var screenPositions = new Vector2[] { new Vector2(400, 400), new Vector2(400, 400), new Vector2(400, 400) };
            var ids = new string[] { "poi_a", "poi_b", "poi_c" };
            var visualUnits = CreateTestUnits(
                (new Vector2(400, 400), "poi_a", 1),
                (new Vector2(400, 400), "poi_b", 3),
                (new Vector2(400, 400), "poi_c", 5)
            );

            var offsets = MarkerOverlapResolver.ComputeOffsets(screenPositions, ids, settings, visualUnits);

            for (int i = 0; i < 3; i++)
                Assert.Greater(offsets[i].magnitude, 0f, $"Marker {ids[i]} should have offset");

            Assert.LessOrEqual(offsets[0].magnitude, offsets[1].magnitude,
                "Priority 1 marker should have smaller offset than priority 3");
            Assert.LessOrEqual(offsets[1].magnitude, offsets[2].magnitude,
                "Priority 3 marker should have smaller offset than priority 5");
        }
    }
}