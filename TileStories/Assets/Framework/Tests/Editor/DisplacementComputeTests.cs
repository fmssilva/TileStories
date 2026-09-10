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

        // --- 2.5-g: lower_priority_only through ComputeOffsets (all three branches) ---

        private static VisualUnit Unit(string id, int priority) => new VisualUnit
        {
            poiId = id,
            hierarchyLevelIndex = priority
        };

        // fixed_axis: leader (lowest hierarchyLevelIndex) pinned at zero; the other
        // member keeps the full threshold of separation away from it. Symmetric spread
        // would be -20/+20; re-expressed relative to the leader it becomes 0/+40.
        [Test]
        public void FixedAxis_LowerPriorityOnly_LeaderPinnedOtherAway()
        {
            var s = DefaultSettings();
            s.displacement_tiebreak = "lower_priority_only";
            var positions = new[] { new Vector2(100, 100), new Vector2(105, 105) };
            var ids = new[] { "p0", "p1" };
            var units = new[] { Unit("p0", 0), Unit("p1", 1) };

            var offsets = MarkerOverlapResolver.ComputeOffsets(positions, ids, s, units);

            Assert.AreEqual(Vector2.zero, offsets[0], "leader must stay at its true position");
            Assert.GreaterOrEqual(offsets[1].y, s.overlap_threshold_px - 0.5f,
                "non-leader must displace at least one threshold away from the leader");
        }

        // Same but the leader is the second input member: pinning follows priority,
        // not input order.
        [Test]
        public void FixedAxis_LowerPriorityOnly_SecondMemberIsLeader()
        {
            var s = DefaultSettings();
            s.displacement_tiebreak = "lower_priority_only";
            var positions = new[] { new Vector2(100, 100), new Vector2(105, 105) };
            var ids = new[] { "p0", "p1" };
            var units = new[] { Unit("p0", 5), Unit("p1", 0) };

            var offsets = MarkerOverlapResolver.ComputeOffsets(positions, ids, s, units);

            Assert.AreEqual(Vector2.zero, offsets[1], "priority-0 member is the leader and stays put");
            Assert.GreaterOrEqual(offsets[0].y, s.overlap_threshold_px - 0.5f);
        }

        // Shared-minimum priority pair under lower_priority_only falls back to the
        // exact symmetric ladder (spec _2.5 Section 7 + section 12 fallback row).
        [Test]
        public void FixedAxis_LowerPriorityOnly_SharedPriority_FallsBackToSymmetric()
        {
            var s = DefaultSettings();
            s.displacement_tiebreak = "lower_priority_only";
            var positions = new[] { new Vector2(100, 100), new Vector2(105, 105) };
            var ids = new[] { "p0", "p1" };
            var units = new[] { Unit("p0", 2), Unit("p1", 2) };

            var offsets = MarkerOverlapResolver.ComputeOffsets(positions, ids, s, units);

            Assert.AreEqual(-20f, offsets[0].y, 0.001f);
            Assert.AreEqual(20f, offsets[1].y, 0.001f);
        }

        // Leader-relative offsets still respect max_displacement_px (clamp applies
        // after the relative re-expression).
        [Test]
        public void FixedAxis_LowerPriorityOnly_RelativeOffsetStillClamped()
        {
            var s = DefaultSettings();
            s.displacement_tiebreak = "lower_priority_only";
            s.max_displacement_px = 40f;
            var p = new Vector2(200, 200);
            var positions = new[] { p, p, p };
            var ids = new[] { "p0", "p1", "p2" };
            var units = new[] { Unit("p0", 0), Unit("p1", 1), Unit("p2", 1) };

            var offsets = MarkerOverlapResolver.ComputeOffsets(positions, ids, s, units);

            Assert.AreEqual(Vector2.zero, offsets[0]);
            foreach (var o in offsets)
                Assert.LessOrEqual(o.magnitude, s.max_displacement_px + 0.001f);
        }

        // force_directed: the pinned leader must end at exactly zero offset even
        // though it participates in the repulsion field for the other members.
        [Test]
        public void ForceDirected_LowerPriorityOnly_LeaderPinnedOtherMoves()
        {
            var s = DefaultSettings();
            s.displacement_algorithm = "force_directed";
            s.displacement_tiebreak = "lower_priority_only";
            var positions = new[] { new Vector2(100, 100), new Vector2(105, 105) };
            var ids = new[] { "p0", "p1" };
            var units = new[] { Unit("p0", 0), Unit("p1", 1) };

            var offsets = MarkerOverlapResolver.ComputeOffsets(positions, ids, s, units);

            Assert.AreEqual(Vector2.zero, offsets[0], "pinned leader must not move");
            Assert.Greater(offsets[1].magnitude, 0.001f, "non-leader must be pushed away");
        }

        // candidate_position: the leader is registered at its original position so
        // every candidate for the lower-priority member must clear it by a threshold.
        [Test]
        public void CandidatePosition_LowerPriorityOnly_LeaderPinnedOtherClearsIt()
        {
            var s = DefaultSettings();
            s.displacement_algorithm = "candidate_position";
            s.displacement_tiebreak = "lower_priority_only";
            var positions = new[] { new Vector2(100, 100), new Vector2(105, 105) };
            var ids = new[] { "p0", "p1" };
            var units = new[] { Unit("p0", 0), Unit("p1", 1) };

            var offsets = MarkerOverlapResolver.ComputeOffsets(positions, ids, s, units);

            Assert.AreEqual(Vector2.zero, offsets[0], "pinned leader must not move");
            Vector2 finalP1 = positions[1] + offsets[1];
            Assert.GreaterOrEqual(Vector2.Distance(positions[0], finalP1), s.overlap_threshold_px - 0.5f,
                "non-leader final position must clear the leader by one threshold");
        }

        // --- 2.5-h: ResolveLabelsToHide direct Tier-0 tests (internal, InternalsVisibleTo) ---

        [Test]
        public void Hide_LabelOnly_CrowdAtClamp_HidesLowerPriority()
        {
            var s = DefaultSettings();
            var positions = new List<Vector2> { new Vector2(0, 0), new Vector2(30, 0) };
            // both pinned at the max clamp and still crowding after displacement
            var offsets = new Vector2[] { new Vector2(120f, 0f), new Vector2(120f, 0f) };
            var units = new[] { Unit("p0", 5), Unit("p1", 1) };

            var hidden = MarkerOverlapResolver.ResolveLabelsToHide(positions, offsets, units, s);

            Assert.IsTrue(hidden.Contains(0), "lower-priority member (larger hierarchyLevelIndex) hides");
            Assert.IsFalse(hidden.Contains(1), "higher-priority member stays visible");
        }

        [Test]
        public void Hide_BelowClamp_HidesNothing()
        {
            var s = DefaultSettings();
            var positions = new List<Vector2> { new Vector2(0, 0), new Vector2(30, 0) };
            var offsets = new Vector2[] { new Vector2(10f, 0f), new Vector2(10f, 0f) };
            var units = new[] { Unit("p0", 5), Unit("p1", 1) };

            var hidden = MarkerOverlapResolver.ResolveLabelsToHide(positions, offsets, units, s);

            Assert.AreEqual(0, hidden.Count);
        }

        [Test]
        public void Hide_MarkerTarget_NeverHides()
        {
            var s = DefaultSettings();
            s.displace_target = "marker";
            var positions = new List<Vector2> { new Vector2(0, 0), new Vector2(30, 0) };
            var offsets = new Vector2[] { new Vector2(120f, 0f), new Vector2(120f, 0f) };
            var units = new[] { Unit("p0", 5), Unit("p1", 1) };

            var hidden = MarkerOverlapResolver.ResolveLabelsToHide(positions, offsets, units, s);

            Assert.AreEqual(0, hidden.Count);
        }

        [Test]
        public void Hide_NeverHidesAllMembers()
        {
            var s = DefaultSettings();
            int n = 4;
            var positions = new List<Vector2>();
            var offsets = new Vector2[n];
            var units = new VisualUnit[n];
            for (int i = 0; i < n; i++)
            {
                positions.Add(new Vector2(i * 30f, 0f));
                offsets[i] = new Vector2(120f, 0f); // everyone pinned at clamp, still crowded
                units[i] = Unit("p" + i, i);
            }

            var hidden = MarkerOverlapResolver.ResolveLabelsToHide(positions, offsets, units, s);

            Assert.LessOrEqual(hidden.Count, n - 1, "at least one label must always remain visible");
        }
    }
}
