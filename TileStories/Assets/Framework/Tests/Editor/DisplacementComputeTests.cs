using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using TileStories;

namespace TileStories.Tests
{
    // Tier-0 EditMode tests for MarkerOverlapResolver.ComputeOffsets (spec _2.5 Section 12).
    // Pure screen-space math: fabricated positions + ids, no scene/camera.
    public class DisplacementComputeTests
    {
        private static DisplacementSettings DefaultSettings() => new DisplacementSettings
        {
            enabled = true,
            overlap_threshold_px = 40f,
            max_displacement_px = 120f,
            displacement_algorithm = "fixed_axis",
            displace_target = "label_only",
            displacement_tiebreak = "symmetric"
        };

        // Two markers within overlap_threshold_px share one group; symmetric ladder
        // offset = (k - (n-1)/2) * threshold = (k - 0.5) * 40 => -20 / +20.
        [Test]
        public void TwoOverlapping_ProducesSymmetricOffsets()
        {
            var s = DefaultSettings();
            var positions = new[] { new Vector2(100, 100), new Vector2(105, 105) };
            var offsets = MarkerOverlapResolver.ComputeOffsets(positions, new[] { "p0", "p1" }, s);
            Assert.AreEqual(-20f, offsets[0].y, 0.001f);
            Assert.AreEqual(20f, offsets[1].y, 0.001f);
            Assert.AreEqual(0f, offsets[0].x, 0.001f);
        }

        [Test]
        public void ThreeStacked_LadderIsSymmetricAndSeparated()
        {
            var s = DefaultSettings();
            var p = new Vector2(200, 200);
            var offsets = MarkerOverlapResolver.ComputeOffsets(new[] { p, p, p }, new[] { "p0", "p1", "p2" }, s);
            var sorted = new List<float> { offsets[0].y, offsets[1].y, offsets[2].y };
            sorted.Sort();
            Assert.AreEqual(-40f, sorted[0], 0.001f);
            Assert.AreEqual(0f, sorted[1], 0.001f);
            Assert.AreEqual(40f, sorted[2], 0.001f);
            for (int i = 0; i < sorted.Count; i++)
                for (int j = i + 1; j < sorted.Count; j++)
                    Assert.GreaterOrEqual(sorted[j] - sorted[i], s.overlap_threshold_px - 0.5f);
        }

        [Test]
        public void FiveStacked_FullSymmetricLadderNoClamp()
        {
            var s = DefaultSettings();
            var p = new Vector2(200, 200);
            var offsets = MarkerOverlapResolver.ComputeOffsets(new[] { p, p, p, p, p }, new[] { "p0", "p1", "p2", "p3", "p4" }, s);
            var sorted = new List<float> { offsets[0].y, offsets[1].y, offsets[2].y, offsets[3].y, offsets[4].y };
            sorted.Sort();
            float[] expected = { -80f, -40f, 0f, 40f, 80f };
            for (int i = 0; i < expected.Length; i++) Assert.AreEqual(expected[i], sorted[i], 0.001f);
            Assert.LessOrEqual(sorted[4], s.max_displacement_px);
        }

        [Test]
        public void MaxDisplacement_ClampsOuterMembers()
        {
            var s = DefaultSettings();
            s.max_displacement_px = 40f; // threshold also 40 -> outer rungs exceed the cap
            var p = new Vector2(200, 200);
            var positions = new Vector2[8];
            var ids = new string[8];
            for (int i = 0; i < 8; i++) { positions[i] = p; ids[i] = "p" + i; }
            var offsets = MarkerOverlapResolver.ComputeOffsets(positions, ids, s);
            foreach (var o in offsets) Assert.LessOrEqual(Mathf.Abs(o.y), s.max_displacement_px + 0.001f);
            // raw outer rung (7 - 3.5) * 40 = 140, clamped to 40
            Assert.AreEqual(40f, Mathf.Max(Mathf.Abs(offsets[0].y), Mathf.Abs(offsets[7].y)), 0.001f);
        }

        [Test]
        public void NoOverlap_ReturnsAllZeros()
        {
            var s = DefaultSettings();
            var offsets = MarkerOverlapResolver.ComputeOffsets(
                new[] { new Vector2(0, 0), new Vector2(600, 0), new Vector2(0, 800) },
                new[] { "a", "b", "c" }, s);
            foreach (var o in offsets) Assert.AreEqual(Vector2.zero, o);
        }

        [Test]
        public void Disabled_ReturnsAllZeros()
        {
            var s = DefaultSettings();
            s.enabled = false;
            var offsets = MarkerOverlapResolver.ComputeOffsets(
                new[] { new Vector2(100, 100), new Vector2(110, 110) }, new[] { "a", "b" }, s);
            foreach (var o in offsets) Assert.AreEqual(Vector2.zero, o);
        }

        [Test]
        public void OrderIndependent_DeterministicAssignment()
        {
            var s = DefaultSettings();
            var positions = new[] { new Vector2(100, 100), new Vector2(100, 100), new Vector2(100, 100) };
            var offsetsA = MarkerOverlapResolver.ComputeOffsets(positions, new[] { "b", "a", "c" }, s);
            var offsetsB = MarkerOverlapResolver.ComputeOffsets(positions, new[] { "a", "c", "b" }, s);
            // sorted by id a<b<c: a -> -40, b -> 0, c -> +40 regardless of input index
            Assert.AreEqual(-40f, offsetsA[1].y, 0.001f);
            Assert.AreEqual(0f, offsetsA[0].y, 0.001f);
            Assert.AreEqual(40f, offsetsA[2].y, 0.001f);
            Assert.AreEqual(-40f, offsetsB[0].y, 0.001f);
            Assert.AreEqual(0f, offsetsB[2].y, 0.001f);
            Assert.AreEqual(40f, offsetsB[1].y, 0.001f);
        }

        [Test]
        public void ComputeOffsets_TargetAgnostic_OffsetsIdenticalRegardlessOfTarget()
        {
            var positions = new[] { new Vector2(100, 100), new Vector2(105, 105) };
            var ids = new[] { "p0", "p1" };

            var labelOnly = DefaultSettings();
            var marker = DefaultSettings();
            marker.displace_target = "marker";
            var both = DefaultSettings();
            both.displace_target = "both";

            var offsetsL = MarkerOverlapResolver.ComputeOffsets(positions, ids, labelOnly);
            var offsetsM = MarkerOverlapResolver.ComputeOffsets(positions, ids, marker);
            var offsetsB = MarkerOverlapResolver.ComputeOffsets(positions, ids, both);

            for (int i = 0; i < 2; i++)
            {
                Assert.AreEqual(offsetsL[i].x, offsetsM[i].x, 0.001f, $"X offset for {i} should be target-agnostic");
                Assert.AreEqual(offsetsL[i].y, offsetsM[i].y, 0.001f, $"Y offset for {i} should be target-agnostic");
                Assert.AreEqual(offsetsL[i].x, offsetsB[i].x, 0.001f, $"X offset for {i} should be target-agnostic");
                Assert.AreEqual(offsetsL[i].y, offsetsB[i].y, 0.001f, $"Y offset for {i} should be target-agnostic");
            }
        }
    }
}
