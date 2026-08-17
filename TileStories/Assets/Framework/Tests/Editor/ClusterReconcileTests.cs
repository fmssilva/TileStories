using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using NUnit.Framework;
using TileStories;

namespace TileStories.Tests
{
    // EditMode tests for LODController.ReconcileClusters (spec section 6.1 cluster lifecycle).
    // ReconcileClusters is `internal` -> visible via Runtime/AssemblyInfo.cs InternalsVisibleTo.
    // Private fields are injected by reflection; the method body runs unmodified (Tier 1).
    // Camera at world origin; co-located members share a screen position -> one group, so
    // grouping is Screen.width-independent and centroid distance maps to FindBand rows.
    public class ClusterReconcileTests
    {
        private const string ClusterPrefab = "Assets/Framework/Runtime/UI/Markers/POI_Cluster.prefab";
        private readonly List<GameObject> _tracked = new();

        [SetUp] public void SetUp() => ARZoomState.SetZoom(1f, 1f, 100f); // pin zoom=1

        [TearDown]
        public void TearDown()
        {
            foreach (var go in _tracked) if (go) Object.DestroyImmediate(go);
            _tracked.Clear();
        }

        private static void SetPrivate(object target, string name, object value)
        {
            var f = target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            Assert.IsNotNull(f, $"field '{name}' not found on {target.GetType().Name}");
            f.SetValue(target, value);
        }

        // SetPrivateProperty: for auto-properties with private setters (e.g. MarkerView.PoiId),
        // where there is no literal field named 'name' to reach by GetField.
        private static void SetPrivateProperty(object target, string name, object value)
        {
            var p = target.GetType().GetProperty(name,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(p, $"property '{name}' not found on {target.GetType().Name}");
            p.SetValue(target, value);
        }

        private static T GetPrivate<T>(object target, string name)
        {
            var f = target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(f, $"field '{name}' not found on {target.GetType().Name}");
            return (T)f.GetValue(target);
        }

        // count_only keeps MarkerClusterView null-safe w/o a wall icon library.
        private static LodSettings Settings(string mode) => new()
        {
            enabled = true,
            density_response_mode = mode,
            density_radius_px = 40f,
            shrink_start_neighbor_count = 2,
            cluster_min_count = 5,
            density_safety_escalation_enabled = true,
            density_safety_escalation_multiplier = 2f,
            cluster_icon_mode = "count_only",
            cluster_band_source = "centroid",
            cluster_band_hysteresis_enabled = true,
            cluster_dissolve_grace_cycles = 3,
            hysteresis_margin_m = 0.5f,
            transition_fade_duration_s = 0.3f,
        };

        private LODController MakeController(LodSettings settings, Vector3 cameraPosition)
        {
            var camGO = new GameObject("TestCam", typeof(Camera));
            camGO.transform.position = cameraPosition;
            _tracked.Add(camGO);
            var rig = new GameObject("LODControllerRig");
            _tracked.Add(rig);
            var controller = rig.AddComponent<LODController>();
            SetPrivate(controller, "_settings", settings);
            SetPrivate(controller, "_camera", camGO.GetComponent<Camera>());
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ClusterPrefab);
            Assert.IsNotNull(prefab, "POI_Cluster.prefab missing at " + ClusterPrefab);
            SetPrivate(controller, "_clusterPrefab", prefab);
            // _wallSession left null: spawnRoot -> controller.transform, iconLibrary -> null.
            return controller;
        }

        // CanvasGroup so MarkerView.SetVisible(false) is observable in EditMode (instant).
        // MakeUnit is a test-only scaffold: we set PoiId directly via reflection because
        // MarkerClusterView.BuildCategoryCounts() reads MarkerView.PoiId (get/private-set),
        // and a null PoiId silently corrupts MemberPoiIds + reuse/overlaps lookups.
        private (MarkerView view, VisualUnit unit) MakeUnit(string poiId, Vector3 worldPosition,
            DensityState state = DensityState.Clustered, int neighbors = 6, int priority = 0)
        {
            var go = new GameObject("member_" + poiId, typeof(MarkerView), typeof(CanvasGroup));
            _tracked.Add(go);
            var view = go.GetComponent<MarkerView>();
            SetPrivateProperty(view, "PoiId", poiId); // set via private setter (auto-property has no field named PoiId)
            return (view, new VisualUnit
            {
                marker = view, poiId = poiId, worldPosition = worldPosition,
                effectiveDistance = Vector3.Distance(Vector3.zero, worldPosition),
                densityState = state, neighborCount = neighbors, hierarchyLevelIndex = priority,
                isVisible = true, shrinkScale = 1f,
            });
        }

        private (MarkerView view, VisualUnit unit)[] Members(Vector3 pos, params string[] ids)
        {
            var arr = new (MarkerView view, VisualUnit unit)[ids.Length];
            for (int i = 0; i < ids.Length; i++) arr[i] = MakeUnit(ids[i], pos);
            return arr;
        }

        // Drive one ReconcileClusters cycle; return the (possibly rebuilt) visual-unit list.
        private static List<VisualUnit> Run(LODController controller, params VisualUnit[] units)
        {
            var list = new List<VisualUnit>(units);
            controller.ReconcileClusters(ref list);
            return list;
        }

        private static void AssertPooled(LODController controller, int count)
            => Assert.AreEqual(count, GetPrivate<List<MarkerClusterView>>(controller, "_activeClusterViews").Count);

        private static void AssertHidden(params (MarkerView view, VisualUnit unit)[] members)
        {
            foreach (var m in members)
                Assert.AreEqual(0f, m.view.GetComponent<CanvasGroup>().alpha,
                    $"absorbed member '{m.unit.poiId}' should be hidden");
        }

                // --- Group 1: band resolution (centroid effective distance -> LodBand) ---

        [Test]
        public void BandResolution_CentroidUnderTwoMeters_IsBand0()
        {
            var c = MakeController(Settings("cluster"), Vector3.zero);
            var m = Members(new Vector3(0, 0, -1.9f), "p1", "p2", "p3"); // |centroid| = 1.9 < 2
            var r = Run(c, m[0].unit, m[1].unit, m[2].unit);
            Assert.AreEqual(1, r.Count);
            Assert.AreEqual(0, r[0].band.Index);
            Assert.AreEqual("p1|p2|p3", r[0].poiId);
            AssertHidden(m);
        }

        [Test]
        public void BandResolution_CentroidAtTwoMeterBoundary_IsBand1()
        {
            var c = MakeController(Settings("cluster"), Vector3.zero);
            var m = Members(new Vector3(0, 0, -2.0f), "p1", "p2", "p3"); // 2.0 is NOT < 2.0
            var r = Run(c, m[0].unit, m[1].unit, m[2].unit);
            Assert.AreEqual(1, r[0].band.Index);
            AssertHidden(m);
        }

        [Test]
        public void BandResolution_CentroidAtSevenMeterBoundary_IsBand2()
        {
            var c = MakeController(Settings("cluster"), Vector3.zero);
            var m = Members(new Vector3(0, 0, -7.0f), "p1", "p2", "p3"); // 7.0 is NOT < 7.0
            var r = Run(c, m[0].unit, m[1].unit, m[2].unit);
            Assert.AreEqual(2, r[0].band.Index);
        }

        [Test]
        public void BandResolution_CentroidBeyondSevenMeters_IsBand2()
        {
            var c = MakeController(Settings("cluster"), Vector3.zero);
            var m = Members(new Vector3(0, 0, -12.0f), "p1", "p2", "p3");
            var r = Run(c, m[0].unit, m[1].unit, m[2].unit);
            Assert.AreEqual(2, r[0].band.Index);
        }

                // --- Group 2: distance-band hysteresis (centroid-band cache, spec section 7) ---

        [Test]
        public void HysteresisEnabled_PromotionHeldAcrossCycles()
        {
            var c = MakeController(Settings("cluster"), Vector3.zero);
            var m = Members(new Vector3(0, 0, -2.4f), "p1", "p2"); // eff 2.4 -> band1, cached
            Run(c, m[0].unit, m[1].unit);
            // Move closer: raw distance 1.6 -> band0 (promotion), but +margin bias = 2.1
            // still lands in band1 (>= prev) -> held, not promoted.
            m[0].unit.worldPosition = new Vector3(0, 0, -1.6f);
            m[1].unit.worldPosition = new Vector3(0, 0, -1.6f);
            var r = Run(c, m[0].unit, m[1].unit);
            Assert.AreEqual(1, r[0].band.Index, "promotion held by hysteresis margin");
        }

        [Test]
        public void HysteresisEnabled_DemotionIsImmediate()
        {
            var c = MakeController(Settings("cluster"), Vector3.zero);
            var m = Members(new Vector3(0, 0, -1.0f), "p1", "p2"); // eff 1.0 -> band0
            Run(c, m[0].unit, m[1].unit);
            m[0].unit.worldPosition = new Vector3(0, 0, -2.6f); // eff 2.6 -> band1
            m[1].unit.worldPosition = new Vector3(0, 0, -2.6f);
            var r = Run(c, m[0].unit, m[1].unit);
            Assert.AreEqual(1, r[0].band.Index, "demotion is immediate, no hold");
        }

        [Test]
        public void HysteresisDisabled_RecomputesBandEachCycle()
        {
            var s = Settings("cluster"); s.cluster_band_hysteresis_enabled = false;
            var c = MakeController(s, Vector3.zero);
            var m = Members(new Vector3(0, 0, -2.4f), "p1", "p2"); // band1 cached
            Run(c, m[0].unit, m[1].unit);
            m[0].unit.worldPosition = new Vector3(0, 0, -1.6f); // eff 1.6 -> band0, no hold
            m[1].unit.worldPosition = new Vector3(0, 0, -1.6f);
            var r = Run(c, m[0].unit, m[1].unit);
            Assert.AreEqual(0, r[0].band.Index, "disabled -> recomputed, not held");
        }

        [Test]
        public void HysteresisCache_KeyedByGroupSignature()
        {
            var c = MakeController(Settings("cluster"), Vector3.zero);
            var m = Members(new Vector3(0, 0, -4.0f), "p1", "p2");
            Run(c, m[0].unit, m[1].unit);
            var cache = GetPrivate<Dictionary<string, LodBand>>(c, "_clusterBandCache");
            Assert.AreEqual(1, cache.Count);
            Assert.IsTrue(cache.ContainsKey("p1|p2"));
            Run(c, m[0].unit, m[1].unit); // same signature -> cache hit, no new key
            Assert.AreEqual(1, cache.Count);
        }

        // --- Group 3: pooled-view dissolve grace (Destroy-call tests moved to PlayMode) ---
        // GraceCycles / GraceZero reach Destroy() in SweepStaleClusterViews, which is
        // illegal in EditMode -> see ClusterReconcilePlayModeTests (UnityTest).

        [Test]
        public void Dissolve_ReuseResetsMissCounter()
        {
            var s = Settings("cluster"); s.cluster_dissolve_grace_cycles = 3;
            var c = MakeController(s, Vector3.zero);
            var m = Members(new Vector3(0, 0, -4f), "p1", "p2");
            Run(c, m[0].unit, m[1].unit); // c1: pooled
            var v = GetPrivate<List<MarkerClusterView>>(c, "_activeClusterViews")[0];
            Run(c); // c2: miss 1
            Run(c, m[0].unit, m[1].unit); // c3: reuse -> miss counter cleared
            var misses = GetPrivate<Dictionary<MarkerClusterView, int>>(c, "_clusterDissolveMisses");
            Assert.IsFalse(misses.ContainsKey(v), "reuse clears the dissolve-miss counter");
            AssertPooled(c, 1);
        }

        [Test]
        public void Dissolve_NullSettings_GuardedSweepKeepsPooledView()
        {
            var c = MakeController(Settings("cluster"), Vector3.zero);
            var m = Members(new Vector3(0, 0, -4f), "p1", "p2");
            Run(c, m[0].unit, m[1].unit); // c1: pooled (valid settings)
            var v = GetPrivate<List<MarkerClusterView>>(c, "_activeClusterViews")[0];
            SetPrivate(c, "_settings", null); // c2: guard branch must not nuke the pool
            Run(c);
            AssertPooled(c, 1);
            Assert.IsTrue(GetPrivate<List<MarkerClusterView>>(c, "_activeClusterViews").Contains(v));
        }

                // --- Group 4: overlap-based pooled-view reuse (spec section 6.1) ---

        [Test]
        public void Reuse_OverlapsAnySharedMember_ReusesView()
        {
            var c = MakeController(Settings("cluster"), Vector3.zero);
            var m = Members(new Vector3(0, 0, -4f), "p1", "p2"); // c1: V{p1,p2}
            Run(c, m[0].unit, m[1].unit);
            var v = GetPrivate<List<MarkerClusterView>>(c, "_activeClusterViews")[0];
            // c2: same screen region + a third member -> group {p1,p2,p3} shares p1,p2 -> reuse.
            var m3 = MakeUnit("p3", new Vector3(0, 0, -4f), DensityState.Clustered);
            var r = Run(c, m[0].unit, m[1].unit, m3.unit);
            Assert.AreSame(v, r[0].clusterView, "any shared member reuses the pooled view");
        }

        [Test]
        public void Reuse_NoOverlap_CreatesNewView()
        {
            var c = MakeController(Settings("cluster"), Vector3.zero);
            var a = MakeUnit("p1", new Vector3(0, 0, -10f), DensityState.Clustered);
            Run(c, a.unit); // c1: V{p1}
            var v = GetPrivate<List<MarkerClusterView>>(c, "_activeClusterViews")[0];
            var b = MakeUnit("p2", new Vector3(20, 0, -10f), DensityState.Clustered); // far -> separate group
            var r = Run(c, a.unit, b.unit);
            var pool = GetPrivate<List<MarkerClusterView>>(c, "_activeClusterViews");
            Assert.AreEqual(2, pool.Count);
            Assert.AreNotEqual(v.GetInstanceID(), r[1].clusterView.GetInstanceID(),
                "{p2} overlaps neither V nor any other pooled view -> new view");
        }

        [Test]
        public void Reuse_SameGroup_SameInstanceIdentity()
        {
            var c = MakeController(Settings("cluster"), Vector3.zero);
            var m = Members(new Vector3(0, 0, -4f), "p1", "p2");
            Run(c, m[0].unit, m[1].unit);
            var v = GetPrivate<List<MarkerClusterView>>(c, "_activeClusterViews")[0];
            var r = Run(c, m[0].unit, m[1].unit);
            Assert.AreSame(v, r[0].clusterView, "identical group reuses the exact view instance");
        }

        [Test]
        public void Reuse_TwoGroups_GetDistinctViewsNotDoubleBooked()
        {
            var c = MakeController(Settings("cluster"), Vector3.zero);
            var a = MakeUnit("pA", new Vector3(0, 0, -10f), DensityState.Clustered);
            var b = MakeUnit("pB", new Vector3(20, 0, -10f), DensityState.Clustered);
            var r = Run(c, a.unit, b.unit); // 2 singleton groups
            Assert.AreEqual(2, r.Count);
            Assert.AreNotEqual(r[0].clusterView.GetInstanceID(), r[1].clusterView.GetInstanceID());
            AssertPooled(c, 2);
            Assert.AreSame(r[0].clusterView,
                GetPrivate<List<MarkerClusterView>>(c, "_activeClusterViews")[0], "lowest-id group first");
        }

        // --- Group 5: ShouldAggregate gate (spec section 6.2, pure static) ---

        [Test]
        public void ShouldAggregate_HybridMode_ClusteredUnit_IsTrue()
        {
            var u = new VisualUnit { poiId = "x", densityState = DensityState.Clustered, neighborCount = 6 };
            Assert.IsTrue(LODController.ShouldAggregate(u, Settings("hybrid")));
        }

        [Test]
        public void ShouldAggregate_SelectHide_SafetyNet_RespectsThreshold()
        {
            // cluster_min=5, safety_mult=2 -> threshold 10 (strict >).
            var s = Settings("select_hide");
            var atThreshold = new VisualUnit { densityState = DensityState.Clustered, neighborCount = 10 };
            var overThreshold = new VisualUnit { densityState = DensityState.Clustered, neighborCount = 11 };
            Assert.IsFalse(LODController.ShouldAggregate(atThreshold, s), "10 is not > 10");
            Assert.IsTrue(LODController.ShouldAggregate(overThreshold, s), "11 > 10 fires");
        }

        [Test]
        public void ShouldAggregate_NoneMode_AlwaysFalse()
        {
            var u = new VisualUnit { densityState = DensityState.Clustered, poiId = "x", neighborCount = 100 };
            Assert.IsFalse(LODController.ShouldAggregate(u, Settings("none")));
        }

        [Test]
        public void ShouldAggregate_NonClusteredState_AlwaysFalse()
        {
            var s = Settings("cluster");
            Assert.IsFalse(LODController.ShouldAggregate(
                new VisualUnit { densityState = DensityState.Shrinking, neighborCount = 100 }, s));
            Assert.IsFalse(LODController.ShouldAggregate(
                new VisualUnit { densityState = DensityState.Normal, neighborCount = 100 }, s));
        }

                // --- Group 6: edge cases / guard branches ---

        [Test]
        public void Edge_EmptyUnits_NoCrashNoPool()
        {
            var c = MakeController(Settings("cluster"), Vector3.zero);
            var r = Run(c); // no units
            Assert.AreEqual(0, r.Count);
            AssertPooled(c, 0);
        }

        [Test]
        public void Edge_NoAggregatables_PooledViewEntersGraceNotDestroyed()
        {
            var s = Settings("cluster"); s.cluster_dissolve_grace_cycles = 3;
            var c = MakeController(s, Vector3.zero);
            var m = Members(new Vector3(0, 0, -4f), "p1", "p2");
            Run(c, m[0].unit, m[1].unit); // c1: pooled
            AssertPooled(c, 1);
            // c2: members no longer Clustered -> not aggregatable -> SweepStale([]) -> grace survivor.
            m[0].unit.densityState = DensityState.Normal;
            m[1].unit.densityState = DensityState.Normal;
            Run(c, m[0].unit, m[1].unit);
            AssertPooled(c, 1);
        }

        [Test]
        public void Edge_SingleMember_FormsAggregate()
        {
            var c = MakeController(Settings("cluster"), Vector3.zero);
            var m = MakeUnit("solo", new Vector3(0, 0, -4f), DensityState.Clustered);
            var r = Run(c, m.unit);
            Assert.AreEqual(1, r.Count);
            Assert.IsNull(r[0].marker); // absorbed -> aggregate
            Assert.AreEqual("solo", r[0].poiId);
            Assert.AreEqual(1, r[0].clusterMembers.Count);
            Assert.IsNotNull(r[0].clusterView);
        }

        [Test]
        public void Edge_MissingClusterPrefab_Guarded()
        {
            var c = MakeController(Settings("cluster"), Vector3.zero);
            var m = MakeUnit("p1", new Vector3(0, 0, -4f), DensityState.Clustered);
            SetPrivate(c, "_clusterPrefab", null); // triggers the null guard at the top of ReconcileClusters
            var r = Run(c, m.unit);
            Assert.AreEqual(1, r.Count); // untouched -- no aggregate formed
            Assert.IsNotNull(r[0].marker); // individual marker retained
            AssertPooled(c, 0);
        }

        // --- Group 7: determinism (spec section 4.4) ---

        [Test]
        public void Determinism_Signature_OrderIndependent()
        {
            var c = MakeController(Settings("cluster"), Vector3.zero);
            var m = Members(new Vector3(0, 0, -4f), "p1", "p2", "p3");
            var r1 = Run(c, m[2].unit, m[0].unit, m[1].unit); // scrambled input
            Assert.AreEqual("p1|p2|p3", r1[0].poiId);
            var r2 = Run(c, m[1].unit, m[2].unit, m[0].unit); // different scramble
            Assert.AreEqual("p1|p2|p3", r2[0].poiId);
        }

        [Test]
        public void Determinism_Band_StableAcrossRepeatedCycles()
        {
            var c = MakeController(Settings("cluster"), Vector3.zero);
            var m = Members(new Vector3(0, 0, -4f), "p1", "p2"); // eff 4 -> band1
            int b1 = Run(c, m[0].unit, m[1].unit)[0].band.Index;
            int b2 = Run(c, m[0].unit, m[1].unit)[0].band.Index;
            int b3 = Run(c, m[0].unit, m[1].unit)[0].band.Index;
            Assert.AreEqual(1, b1); Assert.AreEqual(1, b2); Assert.AreEqual(1, b3);
        }

        [Test]
        public void Determinism_GroupOrder_LowestPoiIdFirst()
        {
            var c = MakeController(Settings("cluster"), Vector3.zero);
            var a = MakeUnit("pA", new Vector3(0, 0, -10f), DensityState.Clustered);
            var b = MakeUnit("pB", new Vector3(20, 0, -10f), DensityState.Clustered);
            var r = Run(c, b.unit, a.unit); // input reversed; internal sort fixes to pA,pB
            Assert.AreEqual(2, r.Count);
            Assert.AreEqual("pA", r[0].poiId);
            Assert.AreEqual("pB", r[1].poiId);
        }

        [Test]
        public void Determinism_AbsorbedSet_NoDuplicates()
        {
            var c = MakeController(Settings("cluster"), Vector3.zero);
            var m = Members(new Vector3(0, 0, -4f), "p1", "p2", "p3");
            var r = Run(c, m[0].unit, m[1].unit, m[2].unit);
            Assert.AreEqual(1, r.Count, "3 absorbed -> 1 aggregate, not 4");
            var ids = new HashSet<string>(r[0].clusterView.MemberPoiIds);
            Assert.AreEqual(3, ids.Count, "distinct members in the aggregate");
            AssertHidden(m);
        }
    }
}