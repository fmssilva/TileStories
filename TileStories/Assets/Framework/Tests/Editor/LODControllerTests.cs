﻿using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Tests
{
    // Tier 0 tests for LODController static methods and VisualUnit comparison logic.
    // Pure static-class + struct logic: no scene, no MonoBehaviour, no Camera.
    public class LODControllerTests
    {
        private List<LodBandEntry> _defaultBands;

        [SetUp]
        public void SetUp()
        {
            MarkerHierarchyResolver.ResetToDefaults();
            _defaultBands = LODController.DefaultBands();
        }

        [TearDown]
        public void TearDown()
        {
            MarkerHierarchyResolver.ResetToDefaults();
        }

        // --- FindBand ---

        [Test]
        public void FindBand_NearZeroDistance_ReturnsFirstBand()
        {
            var band = LODController.FindBand(0f, _defaultBands);
            Assert.AreEqual(0, band.Index);
            Assert.AreEqual(2f, band.MaxDistanceM);
            Assert.AreEqual(-1, band.MaxVisibleCount);
        }

        [Test]
        public void FindBand_JustUnderFirstBoundary_ReturnsFirstBand()
        {
            var band = LODController.FindBand(1.999f, _defaultBands);
            Assert.AreEqual(0, band.Index);
        }

        [Test]
        public void FindBand_ExactlyAtFirstBoundary_ReturnsSecondBand()
        {
            // 2.0 is NOT < 2.0, so it falls through to band 1 (max_distance 7m)
            var band = LODController.FindBand(2f, _defaultBands);
            Assert.AreEqual(1, band.Index);
            Assert.AreEqual(7f, band.MaxDistanceM);
            Assert.AreEqual(15, band.MaxVisibleCount);
        }

        [Test]
        public void FindBand_ExactlyAtSecondBoundary_ReturnsThirdBand()
        {
            // 7.0 is NOT < 7.0, so it falls through to band 2 (max_distance 9999m)
            var band = LODController.FindBand(7f, _defaultBands);
            Assert.AreEqual(2, band.Index);
            Assert.AreEqual(9999f, band.MaxDistanceM);
            Assert.AreEqual(5, band.MaxVisibleCount);
        }

        [Test]
        public void FindBand_VeryLargeDistance_ReturnsLastBand()
        {
            var band = LODController.FindBand(9999f, _defaultBands);
            Assert.AreEqual(2, band.Index);
        }

        [Test]
        public void FindBand_EmptyBands_FallsBackToDefaults()
        {
            var band = LODController.FindBand(1f, new List<LodBandEntry>());
            Assert.AreEqual(0, band.Index);
            Assert.AreEqual(-1, band.MaxVisibleCount);
        }

        // --- FindBandWithHysteresis ---

        [Test]
        public void FindBandWithHysteresis_DemotionIsImmediate()
        {
            // Marker at band 0 (close), distance crosses into band 1 (far) -> immediate demotion
            var prevBand = LODController.FindBand(0f, _defaultBands);
            Assert.AreEqual(0, prevBand.Index);

            var newBand = LODController.FindBandWithHysteresis(2.5f, _defaultBands, prevBand, 0.5f);
            Assert.AreEqual(1, newBand.Index, "Should demote to band 1 immediately at threshold");
        }

        [Test]
        public void FindBandWithHysteresis_StaysWhenWithinMargin()
        {
            // Marker at band 1 (far), distance decreases but stays within hysteresis margin
            var prevBand = new LodBand { Index = 1, MaxDistanceM = 7f, MaxVisibleCount = 15 };

            // 2.5m is within band 0 range (< 2m? no, 2.5 > 2, so it's band 1).
            // Actually 2.5 < 7 so rawBand = band 1, same as prev -> no change.
            // Let's test: marker at band 1, distance drops to 1.9m (band 0 raw),
            // but with 0.5m margin, effectiveDistance + margin = 2.4m which is still > 2m
            // so biasedBand lands in band 1 (not promoted).
            var newBand = LODController.FindBandWithHysteresis(1.9f, _defaultBands, prevBand, 0.5f);
            Assert.AreEqual(1, newBand.Index, "Should stay at band 1 within hysteresis margin (1.9 + 0.5 = 2.4 > 2.0)");
        }

        [Test]
        public void FindBandWithHysteresis_PromotesWhenBeyondMargin()
        {
            // Marker at band 1 (far), distance drops well below threshold + margin
            var prevBand = new LodBand { Index = 1, MaxDistanceM = 7f, MaxVisibleCount = 15 };

            // 1.0m -> rawBand = band 0. But 1.0 + 0.5 = 1.5 < 2.0 -> biasedBand = band 0 -> promoted.
            var newBand = LODController.FindBandWithHysteresis(1.0f, _defaultBands, prevBand, 0.5f);
            Assert.AreEqual(0, newBand.Index, "Should promote to band 0 when distance is well below threshold + margin");
        }

        [Test]
        public void FindBandWithHysteresis_SameBandNoChange()
        {
            var prevBand = new LodBand { Index = 1, MaxDistanceM = 7f, MaxVisibleCount = 15 };
            var newBand = LODController.FindBandWithHysteresis(4f, _defaultBands, prevBand, 0.5f);
            Assert.AreEqual(1, newBand.Index);
        }

        // --- DensityFactor ---

        [Test]
        public void DensityFactor_AtShrinkStart_ReturnsOne()
        {
            // At shrink_start_neighbor_count, factor should be 1.0 (no shrink yet)
            float f = LODController.DensityFactor(2f, 2, 5);
            Assert.AreEqual(1.0f, f, 0.001f);
        }

        [Test]
        public void DensityFactor_AtClusterMin_ReturnsFloor()
        {
            // At cluster_min_count, factor should be 0.4 (floor)
            float f = LODController.DensityFactor(5f, 2, 5);
            Assert.AreEqual(0.4f, f, 0.001f);
        }

        [Test]
        public void DensityFactor_AtMidpoint_ReturnsHalfway()
        {
            // Midpoint between 2 and 5 is 3.5, factor should be ~0.7 (midpoint of 1.0 and 0.4)
            float f = LODController.DensityFactor(3.5f, 2, 5);
            Assert.AreEqual(0.7f, f, 0.001f, "Midpoint of ramp should yield 0.7");
        }

        [Test]
        public void DensityFactor_BelowShrinkStart_ReturnsOne()
        {
            // Below shrink_start, no shrink yet
            float f = LODController.DensityFactor(0f, 2, 5);
            Assert.AreEqual(1.0f, f, 0.001f);
        }

        [Test]
        public void DensityFactor_AboveClusterMin_CappedAtFloor()
        {
            // Above cluster_min, factor should still be 0.4 (floor, never goes lower)
            float f = LODController.DensityFactor(10f, 2, 5);
            Assert.AreEqual(0.4f, f, 0.001f);
        }

        [Test]
        public void DensityFactor_ClusterMinEqualsShrinkStart_NoShrink()
        {
            // Misconfigured (equal), no shrink
            float f = LODController.DensityFactor(3f, 5, 5);
            Assert.AreEqual(1.0f, f, 0.001f);
        }

        // --- ComparePriority (Phase 1: ApplyCountCap ordering) ---

        [Test]
        public void ComparePriority_HigherLevelIndex_Loses()
        {
            // level 0 (higher priority, lower index) should sort before level 2 (lower priority)
            var a = new VisualUnit { hierarchyLevelIndex = 0, effectiveDistance = 10f };
            var b = new VisualUnit { hierarchyLevelIndex = 2, effectiveDistance = 1f };
            int cmp = LODController.ComparePriority(a, b);
            Assert.Less(cmp, 0, "Level 0 (higher priority) should win despite being farther");
        }

        [Test]
        public void ComparePriority_SameLevel_CloserDistance_Wins()
        {
            // Same hierarchy level: closer marker wins (lower effectiveDistance)
            var a = new VisualUnit { hierarchyLevelIndex = 1, effectiveDistance = 3f };
            var b = new VisualUnit { hierarchyLevelIndex = 1, effectiveDistance = 10f };
            int cmp = LODController.ComparePriority(a, b);
            Assert.Less(cmp, 0, "Closer marker at same level should win");
        }

        [Test]
        public void ComparePriority_UnknownLevel_FallsBackToDistance()
        {
            // Unknown hierarchy level (int.MaxValue): all unknown, sort by distance only
            var a = new VisualUnit { hierarchyLevelIndex = int.MaxValue, effectiveDistance = 3f };
            var b = new VisualUnit { hierarchyLevelIndex = int.MaxValue, effectiveDistance = 10f };
            int cmp = LODController.ComparePriority(a, b);
            Assert.Less(cmp, 0, "Unknown levels should fall back to distance comparison");
        }

        [Test]
        public void ComparePriority_KnownVsUnknown_KnownWins()
        {
            // Known level (index 2) should beat unknown (int.MaxValue) regardless of distance
            var a = new VisualUnit { hierarchyLevelIndex = 2, effectiveDistance = 100f };
            var b = new VisualUnit { hierarchyLevelIndex = int.MaxValue, effectiveDistance = 1f };
            int cmp = LODController.ComparePriority(a, b);
            Assert.Less(cmp, 0, "Known hierarchy level should beat unknown level");
        }

        [Test]
        public void ComparePriority_EqualUnits_ReturnsZero()
        {
            var a = new VisualUnit { hierarchyLevelIndex = 3, effectiveDistance = 5f };
            var b = new VisualUnit { hierarchyLevelIndex = 3, effectiveDistance = 5f };
            int cmp = LODController.ComparePriority(a, b);
            Assert.AreEqual(0, cmp);
        }

        // --- MarkerHierarchyResolver.GetLevelPriority integration ---

        [Test]
        public void GetLevelPriority_FallbackPriority_Is1BasedRowOrder()
        {
            var entries = new List<HierarchyLevelEntry>
            {
                new HierarchyLevelEntry { key = "level_1" },
                new HierarchyLevelEntry { key = "level_2" },
                new HierarchyLevelEntry { key = "level_3" },
            };
            MarkerHierarchyResolver.Configure(entries);

            Assert.AreEqual(1, MarkerHierarchyResolver.GetLevelPriority("level_1"));
            Assert.AreEqual(2, MarkerHierarchyResolver.GetLevelPriority("level_2"));
            Assert.AreEqual(3, MarkerHierarchyResolver.GetLevelPriority("level_3"));
        }

        [Test]
        public void GetLevelPriority_UnknownKey_ReturnsMaxValue()
        {
            var entries = new List<HierarchyLevelEntry>
            {
                new HierarchyLevelEntry { key = "level_1" },
                new HierarchyLevelEntry { key = "level_2" },
            };
            MarkerHierarchyResolver.Configure(entries);

            Assert.AreEqual(int.MaxValue, MarkerHierarchyResolver.GetLevelPriority("unknown_key"));
            Assert.AreEqual(int.MaxValue, MarkerHierarchyResolver.GetLevelPriority(""));
            Assert.AreEqual(int.MaxValue, MarkerHierarchyResolver.GetLevelPriority(null));
        }

        [Test]
        public void GetLevelPriority_AfterReset_ReturnsMaxValue()
        {
            var entries = new List<HierarchyLevelEntry>
            {
                new HierarchyLevelEntry { key = "level_1" },
            };
            MarkerHierarchyResolver.Configure(entries);
            Assert.AreEqual(1, MarkerHierarchyResolver.GetLevelPriority("level_1"));

            MarkerHierarchyResolver.ResetToDefaults();
            Assert.AreEqual(int.MaxValue, MarkerHierarchyResolver.GetLevelPriority("level_1"));
        }
private static LodSettings MakeSettings(
            string mode,
            int shrinkStart = 2,
            int clusterMin = 5,
            bool safetyEnabled = true,
            float safetyMultiplier = 2f)
        {
            return new LodSettings
            {
                density_response_mode = mode,
                shrink_start_neighbor_count = shrinkStart,
                cluster_min_count = clusterMin,
                density_safety_escalation_enabled = safetyEnabled,
                density_safety_escalation_multiplier = safetyMultiplier,
            };
        }

        // Helper: fabricate a visible unit with just the density inputs the
        // strategy reads (marker = null so no MonoBehaviour is needed).
        private static VisualUnit MakeUnit(string id, int neighbors, int hierarchyLevel = 0, float distance = 1f)
        {
            return new VisualUnit
            {
                poiId = id,
                marker = null,
                isVisible = true,
                neighborCount = neighbors,
                hierarchyLevelIndex = hierarchyLevel,
                effectiveDistance = distance,
                band = new LodBand { Index = 0, MaxDistanceM = 2f, MaxVisibleCount = -1 },
            };
        }

        // --- ComputeTargetDensityState: strategy dispatch is pure + deterministic ---

        [Test]
        public void Target_None_AlwaysNormal()
        {
            var s = MakeSettings("none");
            Assert.AreEqual(DensityState.Normal, LODController.ComputeTargetDensityState(100, s, 2, 5));
        }

        [Test]
        public void Target_SelectHide_OverThresholdIsClustered()
        {
            var s = MakeSettings("select_hide");
            Assert.AreEqual(DensityState.Normal, LODController.ComputeTargetDensityState(4, s, 2, 5));
            Assert.AreEqual(DensityState.Clustered, LODController.ComputeTargetDensityState(5, s, 2, 5));
            Assert.AreEqual(DensityState.Clustered, LODController.ComputeTargetDensityState(9, s, 2, 5));
        }

        [Test]
        public void Target_ShrinkAndFade_RampsNeverClusters()
        {
            var s = MakeSettings("shrink_and_fade");
            Assert.AreEqual(DensityState.Normal, LODController.ComputeTargetDensityState(1, s, 2, 5));
            Assert.AreEqual(DensityState.Shrinking, LODController.ComputeTargetDensityState(2, s, 2, 5));
            Assert.AreEqual(DensityState.Shrinking, LODController.ComputeTargetDensityState(5, s, 2, 5));
            // n=9 sits below the safety-net threshold (cluster_min 5 * mult 2 = 10),
            // so shrink_and_fade stays in the ramp rather than being escalated.
            Assert.AreEqual(DensityState.Shrinking, LODController.ComputeTargetDensityState(9, s, 2, 5));
        }

        [Test]
        public void Target_Hybrid_ShrikThenEscalate()
        {
            var s = MakeSettings("hybrid");
            Assert.AreEqual(DensityState.Normal, LODController.ComputeTargetDensityState(1, s, 2, 5));
            Assert.AreEqual(DensityState.Shrinking, LODController.ComputeTargetDensityState(2, s, 2, 5));
            Assert.AreEqual(DensityState.Shrinking, LODController.ComputeTargetDensityState(4, s, 2, 5));
            Assert.AreEqual(DensityState.Clustered, LODController.ComputeTargetDensityState(5, s, 2, 5));
        }

        [Test]
        public void Target_UnknownMode_FallsBackToNormal()
        {
            var s = MakeSettings("bogus");
            Assert.AreEqual(DensityState.Normal, LODController.ComputeTargetDensityState(9, s, 2, 5));
        }

        // --- Safety-net escalation (§6.2): non-hybrid + over threshold -> cluster ---

        [Test]
        public void SafetyNet_ShrinkAndFade_AtThresholdNoFire_JustOverFires()
        {
            // cluster_min=5, mult=2 -> threshold 10 (strict >).
            var s = MakeSettings("shrink_and_fade", safetyMultiplier: 2f);
            Assert.AreEqual(DensityState.Shrinking, LODController.ComputeTargetDensityState(10, s, 2, 5));
            Assert.AreEqual(DensityState.Clustered, LODController.ComputeTargetDensityState(11, s, 2, 5));
        }

        [Test]
        public void SafetyNet_Hybrid_NotApplied()
        {
            // Safety net is gated off for hybrid (hybrid escalates via its own rule).
            var s = MakeSettings("hybrid", safetyMultiplier: 2f);
            Assert.AreEqual(DensityState.Shrinking, LODController.ComputeTargetDensityState(3, s, 2, 5));
            Assert.AreEqual(DensityState.Clustered, LODController.ComputeTargetDensityState(11, s, 2, 5));
        }

        [Test]
        public void SafetyNet_Disabled_NoOverride()
        {
            var s = MakeSettings("shrink_and_fade", safetyEnabled: false);
            Assert.AreEqual(DensityState.Shrinking, LODController.ComputeTargetDensityState(999, s, 2, 5));
        }

        // --- Hysteresis: 2-cycle commit (state does not flip on first agreement) ---

        [Test]
        public void Hysteresis_ChangesOnlyAfterTwoAgreeingCycles()
        {
            var hys = new Dictionary<string, DensityHysteresisState>();
            // Cycle 1: Normal -> Clustered is provisional; committed unchanged.
            Assert.AreEqual(DensityState.Normal, LODController.CommitDensityState("p1", DensityState.Clustered, hys));
            // Cycle 2: same target -> commit.
            Assert.AreEqual(DensityState.Clustered, LODController.CommitDensityState("p1", DensityState.Clustered, hys));
        }

        [Test]
        public void Hysteresis_SingleContraryCycleCancelsPending()
        {
            var hys = new Dictionary<string, DensityHysteresisState>();
            // Cycle 1: Normal -> Clustered provisional.
            LODController.CommitDensityState("p1", DensityState.Clustered, hys);
            // Cycle 2: reverts to Normal -> pending reset, no commit.
            var c = LODController.CommitDensityState("p1", DensityState.Normal, hys);
            Assert.AreEqual(DensityState.Normal, c);
            Assert.AreEqual(DensityState.Normal, hys["p1"].pending);
                        Assert.AreEqual(0, hys["p1"].pendingCycles);
        }

        // --- ApplyDensityStrategy end-to-end: decision -> shrinkScale / isVisible ---

        [Test]
        public void Strategy_ShrinkAndFade_RampScale()
        {
            var s = MakeSettings("shrink_and_fade");
            var hys = new Dictionary<string, DensityHysteresisState>();
            var u = MakeUnit("p1", 3);
            // Cycle 1: provisional, committed still Normal -> scale 1.
            LODController.ApplyDensityStrategy(new List<VisualUnit> { u }, s, hys);
            Assert.AreEqual(1f, u.shrinkScale, 0.001f);
            Assert.IsTrue(u.isVisible);
            // Cycle 2: commit Shrinking -> scale = DensityFactor(3,2,5) = 0.8.
            LODController.ApplyDensityStrategy(new List<VisualUnit> { u }, s, hys);
            Assert.AreEqual(0.8f, u.shrinkScale, 0.001f);
            Assert.IsTrue(u.isVisible, "shrink_and_fade never hides");
        }

        [Test]
        public void Strategy_ShrinkAndFade_FloorAtClusterMin()
        {
            var s = MakeSettings("shrink_and_fade");
            var hys = new Dictionary<string, DensityHysteresisState>();
            var u = MakeUnit("p1", 5); // at cluster_min -> floor 0.4
            for (int i = 0; i < 2; i++) LODController.ApplyDensityStrategy(new List<VisualUnit> { u }, s, hys);
            Assert.AreEqual(0.4f, u.shrinkScale, 0.001f, "floor at cluster_min");
            Assert.IsTrue(u.isVisible);
        }

        [Test]
        public void Strategy_SelectHide_HidesAtOrAboveClusterMin()
        {
            var s = MakeSettings("select_hide");
            var hys = new Dictionary<string, DensityHysteresisState>();
            var u = MakeUnit("p1", 5); // >= cluster_min
            LODController.ApplyDensityStrategy(new List<VisualUnit> { u }, s, hys);
            Assert.IsTrue(u.isVisible, "provisional: not yet hidden");
            LODController.ApplyDensityStrategy(new List<VisualUnit> { u }, s, hys);
            Assert.IsFalse(u.isVisible, "select_hide hides once committed clustered");
            Assert.AreEqual(1f, u.shrinkScale, 0.001f);
        }

        [Test]
        public void Strategy_Hybrid_ShrinkingThenEscalateToCluster()
        {
            var s = MakeSettings("hybrid");
            var hys = new Dictionary<string, DensityHysteresisState>();
            var u = MakeUnit("p1", 3); // Shrinking
            for (int i = 0; i < 2; i++) LODController.ApplyDensityStrategy(new List<VisualUnit> { u }, s, hys);
            Assert.AreEqual(0.8f, u.shrinkScale, 0.001f);
            Assert.IsTrue(u.isVisible);

            // Density jumps to 6 (>= cluster_min 5) -> target Clustered.
            u.neighborCount = 6;
            LODController.ApplyDensityStrategy(new List<VisualUnit> { u }, s, hys); // provisional
            Assert.AreEqual(DensityState.Shrinking, hys["p1"].committed, "no commit on first agreement");
            Assert.IsTrue(u.isVisible, "still visible until clustered commits");
            LODController.ApplyDensityStrategy(new List<VisualUnit> { u }, s, hys); // commit
            Assert.AreEqual(DensityState.Clustered, hys["p1"].committed);
            Assert.IsFalse(u.isVisible, "clustered members hidden (Phase 2 renders aggregate)");
        }

        [Test]
        public void Strategy_SafetyNet_OverridesShrinkAtHighDensity()
        {
            // shrink_and_fade but safety net fires at n=11 (>10) -> Cluster -> hidden.
            var s = MakeSettings("shrink_and_fade", safetyMultiplier: 2f);
            var hys = new Dictionary<string, DensityHysteresisState>();
            var u = MakeUnit("p1", 11);
            for (int i = 0; i < 2; i++) LODController.ApplyDensityStrategy(new List<VisualUnit> { u }, s, hys);
            Assert.IsFalse(u.isVisible, "safety net overrode shrink -> cluster -> hidden");
        }

        [Test]
        public void Strategy_None_NeverMutatesVisibilityOrScale()
        {
            var s = MakeSettings("none");
            var hys = new Dictionary<string, DensityHysteresisState>();
            var u = MakeUnit("p1", 100); // dense, but none = no action
            // 'none' is a total opt-out: even at extreme density (n=100, above the
            // safety threshold), nothing acts -- no shrink, no hide.
            for (int i = 0; i < 2; i++)
                LODController.ApplyDensityStrategy(new List<VisualUnit> { u }, s, hys);
            Assert.IsTrue(u.isVisible);
            Assert.AreEqual(1f, u.shrinkScale, 0.001f);
        }

        // --- IsDensityConfigValid: shrink_start < cluster_min (§6) ---

        [Test]
        public void IsDensityConfigValid_Valid_ReturnsTrue()
        {
            Assert.IsTrue(LODController.IsDensityConfigValid(MakeSettings("hybrid")));
        }

        [Test]
        public void IsDensityConfigValid_EqualOrReversed_ReturnsFalse()
        {
            Assert.IsFalse(LODController.IsDensityConfigValid(MakeSettings("hybrid", shrinkStart: 5, clusterMin: 5)));
            Assert.IsFalse(LODController.IsDensityConfigValid(MakeSettings("hybrid", shrinkStart: 6, clusterMin: 5)));
            Assert.IsFalse(LODController.IsDensityConfigValid(MakeSettings("hybrid", shrinkStart: 2, clusterMin: 1)));
        }
    }
}
