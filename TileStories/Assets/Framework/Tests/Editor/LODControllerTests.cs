using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Tests
{
    // Tier 0 tests for LODController static methods and VisualUnit comparison logic.
    // Pure static-class + struct logic: no scene, no MonoBehaviour, no Camera.
    public class LODControllerTests
    {
        private List<LodBandEntry> _defaultBands;
        private readonly List<GameObject> _trackedGOs = new();

        [SetUp]
        public void SetUp()
        {
            MarkerHierarchyResolver.ResetToDefaults();
            _defaultBands = LODController.DefaultBands();
        }

        [TearDown]
        public void TearDown()
        {
            ResetZoomState();
            MarkerHierarchyResolver.ResetToDefaults();
            foreach (var go in _trackedGOs)
                if (go) UnityEngine.Object.DestroyImmediate(go);
            _trackedGOs.Clear();
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

        // --- BuildPassthroughVisualUnits (Block 6, LOD-disabled passthrough) ---
        // Tier-0: static method, no scene needed. Creates lightweight MarkerView
        // GameObjects (no full prefab wiring required because BuildPassthroughVisualUnits
        // only reads PoiId, transform.position, and HierarchyLevelKey).

        private static MarkerView MakeBareMarker(string poiId, Vector3 position)
        {
            var go = new GameObject("MarkerViewTest", typeof(MarkerView));
            var pv = go.GetComponent<MarkerView>();
            var backing = typeof(MarkerView).GetField("<PoiId>k__BackingField",
                BindingFlags.NonPublic | BindingFlags.Instance);
            backing?.SetValue(pv, poiId);
            go.transform.position = position;
            return pv;
        }

        [Test]
        public void BuildPassthroughVisualUnits_EmptyList_ReturnsEmpty()
        {
            var result = LODController.BuildPassthroughVisualUnits(new List<MarkerView>());
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void BuildPassthroughVisualUnits_NullMarkers_Skipped()
        {
            var markers = new List<MarkerView> { null, null };
            var result = LODController.BuildPassthroughVisualUnits(markers);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void BuildPassthroughVisualUnits_AllUnitsVisible()
        {
            var m1 = MakeBareMarker("p1", Vector3.zero);
            var m2 = MakeBareMarker("p2", Vector3.forward);
            var result = LODController.BuildPassthroughVisualUnits(new List<MarkerView> { m1, m2 });
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result[0].isVisible);
            Assert.IsTrue(result[1].isVisible);
        }

        [Test]
        public void BuildPassthroughVisualUnits_WorldPositionFromTransform()
        {
            var m1 = MakeBareMarker("p1", new Vector3(1, 2, 3));
            var m2 = MakeBareMarker("p2", new Vector3(4, 5, 6));
            var result = LODController.BuildPassthroughVisualUnits(new List<MarkerView> { m1, m2 });
            Assert.AreEqual(new Vector3(1, 2, 3), result[0].worldPosition);
            Assert.AreEqual(new Vector3(4, 5, 6), result[1].worldPosition);
        }

        [Test]
        public void BuildPassthroughVisualUnits_UnknownKey_HierarchyLevelIndexIsMaxValue()
        {
            // With no _anchor set, HierarchyLevelKey returns "" and GetLevelPriority("")
            // resolves to int.MaxValue (lowest priority).
            var m = MakeBareMarker("p1", Vector3.zero);
            var result = LODController.BuildPassthroughVisualUnits(new List<MarkerView> { m });
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(int.MaxValue, result[0].hierarchyLevelIndex);
        }

        [Test]
        public void BuildPassthroughVisualUnits_PoiIdPreserved()
        {
            var m = MakeBareMarker("hero_poi", Vector3.zero);
            var result = LODController.BuildPassthroughVisualUnits(new List<MarkerView> { m });
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("hero_poi", result[0].poiId);
        }

        [Test]
        public void BuildPassthroughVisualUnits_MarkerReferencePreserved()
        {
            var m = MakeBareMarker("p1", Vector3.zero);
            var result = LODController.BuildPassthroughVisualUnits(new List<MarkerView> { m });
            Assert.AreSame(m, result[0].marker);
        }

        // --- Phase A: shallow-angle foreshortening + zoom-unlocks-markers (spec _2_4 sections 2/10; _2.7 entry 2.4-p) ---
        // These two are the domain's most load-bearing correctness claims: distance truth under a
        // grazing camera, and zoom actually changing the visible set through effective distance.

        private static readonly System.Reflection.BindingFlags InstanceFlags =
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

        // Minimal controller rig for the pure distance->band->cap pipeline (no cluster prefab needed:
        // ComputeEffectiveDistances / AssignBands / ApplyCountCap never touch it).
        private static LODController MakePipelineController(Vector3 cameraPosition, List<GameObject> tracked)
        {
            var camGO = new GameObject("PipelineTestCam", typeof(Camera));
            camGO.transform.position = cameraPosition;
            tracked.Add(camGO);
            var rig = new GameObject("PipelineControllerRig");
            tracked.Add(rig);
            var controller = rig.AddComponent<LODController>();
            var settings = new LodSettings { enabled = true }; // empty bands -> AssignBands falls back to DefaultBands()
            typeof(LODController)
                .GetField("_settings", InstanceFlags)?
                .SetValue(controller, settings);
            typeof(LODController)
                .GetField("_camera", InstanceFlags)?
                .SetValue(controller, camGO.GetComponent<Camera>());
            return controller;
        }

        // ZoomFactor is static state -- hand it back at 1x after every test so no
        // test leaks zoom into another (folded into the single allowed TearDown).
        private static void ResetZoomState()
        {
            ARZoomState.ResetToBase(0.5f, 10f);
        }

        [Test]
        public void ShallowGrazingAngle_TrueDistanceDrivesThinning_NotScreenCloseness()
        {
            // Camera sits just off the wall's edge looking almost ALONG the row (grazing angle):
            // every marker appears similarly "near the wall", but real 3D distance spans ~1m to ~5.5m.
            // The core invariant: band assignment must follow TRUE 3D distance, so far markers land in
            // a farther band (thinned/shrunk) even though the camera is physically near all of them.
            var tracked = new List<GameObject>();
            var markers = new List<MarkerView>();
            for (int k = 0; k < 10; k++)
                markers.Add(MakeBareMarker($"shallow_{k}", new Vector3(0f, 0f, k * 0.5f)));
            tracked.AddRange(markers.Select(m => m.gameObject));

            try
            {
                var camPos = new Vector3(0.3f, 0.3f, -1f); // just off the wall start, grazing down the row
                var controller = MakePipelineController(camPos, tracked);
                controller.transform.rotation = Quaternion.LookRotation(new Vector3(-0.3f, -0.3f, 1f));

                ARZoomState.SetZoom(1f, 0.5f, 10f);
                var distances = controller.ComputeEffectiveDistances(markers);

                // Effective distances must strictly increase along the row and equal true 3D distance at 1x.
                for (int k = 1; k < 10; k++)
                {
                    Assert.Greater(distances[$"shallow_{k}"], distances[$"shallow_{k - 1}"],
                        $"marker {k} must be truly farther than marker {k - 1} despite the grazing angle");
                    Assert.AreEqual(
                        Vector3.Distance(camPos, markers[k].transform.position), distances[$"shallow_{k}"], 0.001f,
                        "effective distance must equal real 3D distance at ZoomFactor=1");
                }

                // Far end of the row lands in a strictly farther band than the near end.
                var bands = controller.AssignBands(distances);
                Assert.Less(bands[$"shallow_0"].Index, bands[$"shallow_9"].Index,
                    "far marker must be thinned into a farther band than the near one -- not treated as uniformly close");
            }
            finally
            {
                foreach (var go in tracked)
                    if (go) UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void ZoomFactorIncrease_SamePositions_UnlocksMoreMarkersThroughCountCap()
        {
            // Section 10's reason zoom exists: same real positions, higher ZoomFactor -> smaller
            // effectiveDistance -> nearer band -> higher count cap -> strictly more visible markers.
            const int count = 8;
            var tracked = new List<GameObject>();
            var markers = new List<MarkerView>();
            for (int k = 0; k < count; k++)
                markers.Add(MakeBareMarker($"zoom_{k}", new Vector3(0f, 0f, 12f))); // 12m: band2 at 1x, band1 at 3x
            tracked.AddRange(markers.Select(m => m.gameObject));

            try
            {
                int CountVisible(float zoom)
                {
                    ARZoomState.SetZoom(zoom, 0.5f, 10f);
                    // Fresh controller per pass: AssignBands' hysteresis cache must not couple the two runs.
                    var controller = MakePipelineController(Vector3.zero, tracked);
                    var distances = controller.ComputeEffectiveDistances(markers);
                    var bands = controller.AssignBands(distances);
                    var units = LODController.BuildPassthroughVisualUnits(markers);
                    // Stamp each unit with the band/distance data ApplyDensityResponse would carry in
                    // the full pipeline; here we exercise only the distance->band->cap seam.
                    foreach (var u in units)
                    {
                        u.band = bands[u.poiId];
                        u.effectiveDistance = distances[u.poiId];
                    }
                    controller.ApplyCountCap(units, bands);
                    return units.Count(u => u.isVisible);
                }

                Assert.AreEqual(4f, 12f / 3f, 0.001f); // sanity on the fixture math itself
                int visibleAt1x = CountVisible(1f);   // eff 12m -> band 2, cap 5
                int visibleAt3x = CountVisible(3f);   // eff 4m  -> band 1, cap 15

                Assert.LessOrEqual(visibleAt1x, 5, "at 1x the outer band's count cap must hide some markers");
                Assert.AreEqual(count, visibleAt3x, "at 3x every marker must fit inside the mid band cap");
                Assert.Greater(visibleAt3x, visibleAt1x,
                    "zooming in MUST unlock more markers for identical real positions (section 10 hookup)");
            }
            finally
            {
                foreach (var go in tracked)
                    if (go) UnityEngine.Object.DestroyImmediate(go);
            }
        }
    }
}
