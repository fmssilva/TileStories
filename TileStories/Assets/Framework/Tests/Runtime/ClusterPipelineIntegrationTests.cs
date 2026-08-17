using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using TileStories;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TileStories.Tests
{
    // SECTION 14 / 16 L763-2: end-to-end integration of the cluster LOD pipeline
    // using the REAL LivingRoom config.json (not fabricated units).
    //
    // Drives the genuine Evaluate() stage order end-to-end:
    //   FrustumCull -> ComputeEffectiveDistances -> AssignBands ->
    //   EvaluateDensity -> ApplyDensityResponse (x2, for 2-cycle density hysteresis
    //   to COMMIT densityState=Clustered) -> ReconcileClusters -> ApplyCountCap ->
    //   ApplyVisibility
    // against the 6 real lamp_* POIs (all has_captured_position=true, clustered at
    // the wall origin). Asserts the resulting aggregate's real member set, real
    // per-category counts (6 distinct heritage categories), real pie slices, and the
    // real "+6" count label -- then re-runs the pipeline to prove determinism.
    [Category("Integration/ClusterPipeline/Real")]
    public class ClusterPipelineIntegrationTests
    {
        private const string MarkerPrefab  = "Assets/Framework/Runtime/UI/Markers/POI_Marker.prefab";
        private const string ClusterPrefab = "Assets/Framework/Runtime/UI/Markers/POI_Cluster.prefab";

        // Real lamp_* family: all captured_position, clustered near the wall origin
        // (-0.95, -0.87, -4.18), each a distinct heritage category -> exactly one
        // real aggregate of 6 members with 6 pie slices.
        private static readonly (string id, string category)[] LampFamily =
        {
            ("lamp",                 "royal_government"),
            ("lamp_religious",       "religious"),
            ("lamp_military",        "military"),
            ("lamp_residential",     "residential"),
            ("lamp_economic",        "economic"),
            ("lamp_infrastructure",  "infrastructure"),
        };

        private static readonly Vector3 LampCentroid = new Vector3(-0.95f, -0.87f, -4.18f);

        private WallConfigData _config;
        private LODController _controller;
        private Camera _camera;
        private List<MarkerView> _allMarkers = new();
        private readonly List<GameObject> _tracked = new();

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            // --- real config load from shipped StreamingAssets ---
            WallConfigData config = null;
            yield return WallConfigLoader.LoadFromStreamingAssets("LivingRoom/config.json",
                c => config = c);
            Assert.IsNotNull(config, "StreamingAssets/LivingRoom/config.json must load for this integration test.");
            _config = config;

            // --- real palette + hierarchy configuration from the wall's own taxonomy ---
            CategoryPalette.Configure(_config.category_styles);
            MarkerHierarchyResolver.Configure(_config.hierarchy_levels);

            // --- real camera standoff: lamps sit ~19m in front, centered, all in frustum ---
            var camGO = new GameObject("TestCam", typeof(Camera));
            _camera = camGO.GetComponent<Camera>();
            _camera.transform.position = LampCentroid + new Vector3(0f, 0f, 20f);
            _camera.transform.LookAt(LampCentroid);
            _camera.fieldOfView = 60f;
            _tracked.Add(camGO);

            // --- real LODController via the proven reflection harness ---
            ARZoomState.SetZoom(1f, 1f, 100f);
            _controller = MakeController(TestSettings());
            // camera + LookAt already positioned; assign to controller
            SetPrivate(_controller, "_camera", _camera);

            // --- spawn the 6 REAL lamp_* POIs at REAL resolved positions ---
            var prefab = LoadMarkerPrefab();
            foreach (var (id, category) in LampFamily)
            {
                var poi = _config.pois.FirstOrDefault(p => p.id == id);
                Assert.IsNotNull(poi, $"Lamp family POI '{id}' must exist in LivingRoom/config.json.");

                Assert.IsTrue(
                    POIPositionResolver.TryResolvePosition(poi, _config.calibration_anchors.ToArray(),
                        out var worldPos),
                    $"real captured_position resolution must succeed for {id}.");
                Assert.IsTrue(float.IsFinite(worldPos.x) && float.IsFinite(worldPos.y) && float.IsFinite(worldPos.z),
                    $"real resolved position must be finite for {id}.");

                var root = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                Assert.IsNotNull(root, "POI_Marker.prefab must instantiate.");
                root.transform.position = worldPos;
                _tracked.Add(root);

                var anchor = root.GetComponent<POIAnchor>();
                if (anchor == null) anchor = root.AddComponent<POIAnchor>();
                anchor.Initialise(poi);

                var marker = root.GetComponent<MarkerView>();
                Assert.IsNotNull(marker, "POI_Marker.prefab root must carry a MarkerView.");
                marker.Initialise(anchor, MarkerStyle.OutlineGold, MarkerShape.Circle, MarkerEffectFlags.None);
                _allMarkers.Add(marker);
            }

            Assert.AreEqual(6, _allMarkers.Count, "must spawn the 6 real lamp_* markers.");

            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            MarkerHierarchyResolver.ResetToDefaults();
            foreach (var go in _tracked)
                if (go) UnityEngine.Object.DestroyImmediate(go);
            _tracked.Clear();
            _allMarkers.Clear();
            yield return null;
        }

        private LodSettings TestSettings()
        {
            var s = Settings("cluster");
            s.density_radius_px = 80f;            // generous: 6 lamps (~0.45m at ~19m) project well inside
            s.cluster_min_count = 2;              // 5 real neighbours >= 2 -> Clustered, comfortably off-boundary
            s.density_safety_escalation_enabled = false; // no surprise escalation; deterministic on raw neighbour count
            s.cluster_icon_mode = "pie_and_count";       // exercises real pie + "+N" label rendering
            return s;
        }

        // Reproduces Evaluate()'s real stage order exactly, returning the post-Reconcile
        // units list (absorbed members stripped, aggregate inserted).
        private List<VisualUnit> RunPipelineOnce()
        {
            var visible = _controller.FrustumCull(_allMarkers);
            var distances = _controller.ComputeEffectiveDistances(visible);
            var bands = _controller.AssignBands(distances);
            var neighbors = _controller.EvaluateDensity(visible);

            var units = _controller.ApplyDensityResponse(visible, _allMarkers, bands, distances, neighbors);
            _controller.ReconcileClusters(ref units);
            _controller.ApplyCountCap(units, bands);
            _controller.ApplyVisibility(units);
            return units;
        }

        // ===== TEST 1: real config -> real dense family -> real aggregate =====
        [UnityTest]
        public IEnumerator RealConfig_LampFamily_AggregatesIntoOneRealCluster()
        {
            // Two real ApplyDensityResponse passes so the 2-cycle density hysteresis
            // (CommitDensityState) actually commits densityState=Clustered -- the genuine
            // reason ReconcileClusters has aggregatable members to fold.
            RunPipelineOnce(); // cycle 1: pendingClusters=1, densityState stays Normal, no aggregate
            var units = RunPipelineOnce(); // cycle 2: densityState committed Clustered -> aggregate forms

            var views = GetPrivate<List<MarkerClusterView>>(_controller, "_activeClusterViews");
            Assert.AreEqual(1, views.Count, "exactly one real aggregate must form for the lamp family.");

            var cv = views[0];

            // REAL member set == the 6 real lamp_* ids (no fabricated ordering).
            var memberIds = new HashSet<string>(cv.MemberPoiIds);
            CollectionAssert.AreEquivalent(LampFamily.Select(p => p.id).ToArray(),
                memberIds.ToArray(),
                "aggregate must contain exactly the 6 real lamp_* ids.");

            // REAL per-category counts: 6 distinct heritage categories, each exactly once.
            var counts = cv.CategoryCounts;
            Assert.AreEqual(6, counts.Count, "pie must resolve exactly 6 real categories.");
            foreach (var (id, category) in LampFamily)
            {
                Assert.IsTrue(counts.ContainsKey(category),
                    $"category '{category}' (from {id}) must be present in the real palette.");
                Assert.AreEqual(1, counts[category],
                    $"category '{category}' must appear exactly once in the real aggregate.");
            }

            // REAL pie slices: one stacked slice per distinct category in pie_and_count mode.
            var slices = GetPrivate<List<GameObject>>(cv, "_slices");
            Assert.AreEqual(6, slices.Count, "pie_and_count must render 6 real slices for 6 categories.");

            // REAL count label: "+6" sourced from the real member count.
            var countLabelText = GetPrivateText(cv, "countLabel");
            Assert.IsNotNull(countLabelText, "MarkerClusterView must wire a countLabel TMP.");
            Assert.AreEqual("+6", countLabelText, "count label must read '+6' for 6 real members.");

            // REAL absorption: lamp members are stripped from the unit list (replaced by
            // the aggregate), not left as standalone visible units.
            var lampIds = new HashSet<string>(LampFamily.Select(p => p.id));
            Assert.IsFalse(units.Any(u => u != null && u.marker != null && lampIds.Contains(u.marker.PoiId)),
                "absorbed lamp members must not survive as standalone units.");
            Assert.IsTrue(units.Any(u => u != null && u.clusterView != null),
                "the aggregate unit (clusterView set) must be the surviving unit.");

            yield return null;
        }

        // ===== TEST 2: determinism -- re-run the real pipeline, snapshot must match =====
        [UnityTest]
        public IEnumerator RealConfig_LampFamily_DeterministicAcrossReRuns()
        {
            RunPipelineOnce(); // cycle 1 (no aggregate yet)
            RunPipelineOnce(); // cycle 2 (first real aggregate)
            var snap1 = Snapshot();

            // Fresh re-run: densityState already committed Clustered, so this pass must
            // REUSE (not duplicate) the pooled aggregate and produce an identical snapshot.
            RunPipelineOnce();
            RunPipelineOnce();
            var snap2 = Snapshot();

            Assert.AreEqual(1, snap1.aggregateCount, "first run must yield exactly one aggregate.");
            Assert.AreEqual(1, snap2.aggregateCount, "re-run must not duplicate the aggregate.");
            CollectionAssert.AreEqual(snap1.memberIds.OrderBy(x => x).ToArray(),
                snap2.memberIds.OrderBy(x => x).ToArray(),
                "deterministic: identical member set across re-runs.");
            CollectionAssert.AreEqual(snap1.categories.OrderBy(x => x).ToArray(),
                snap2.categories.OrderBy(x => x).ToArray(),
                "deterministic: identical category palette across re-runs.");

            yield return null;
        }

        // ---- small snapshot helper for the determinism test ----
        private (int aggregateCount, List<string> memberIds, List<string> categories) Snapshot()
        {
            var views = GetPrivate<List<MarkerClusterView>>(_controller, "_activeClusterViews");
            var members = new List<string>();
            var cats = new List<string>();
            foreach (var v in views)
            {
                members.AddRange(v.MemberPoiIds);
                foreach (var kvp in v.CategoryCounts) cats.Add(kvp.Key);
            }
            return (views.Count, members, cats);
        }

        // =========================== proven harness (mirrors ClusterReconcilePlayModeTests) ===========================

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

        private LODController MakeController(LodSettings settings)
        {
            var rig = new GameObject("LODControllerRig");
            _tracked.Add(rig);
            var controller = rig.AddComponent<LODController>();
            SetPrivate(controller, "_settings", settings);

#if UNITY_EDITOR
            var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(ClusterPrefab);
            Assert.IsNotNull(prefab, "POI_Cluster.prefab missing at " + ClusterPrefab);
            SetPrivate(controller, "_clusterPrefab", prefab);
#else
            Assert.Fail("ClusterPipelineIntegrationTests requires the Unity Editor (AssetDatabase).");
#endif
            // _wallSession left null: spawnRoot -> controller.transform, iconLibrary -> null.
            return controller;
        }

        private static GameObject LoadMarkerPrefab()
        {
            var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(MarkerPrefab);
            Assert.IsNotNull(prefab, "POI_Marker.prefab missing at " + MarkerPrefab);
            return prefab;
        }

        private static void SetPrivate(object target, string name, object value)
        {
            var f = target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            Assert.IsNotNull(f, "field '" + name + "' not found on " + target.GetType().Name);
            f.SetValue(target, value);
        }

        private static T GetPrivate<T>(object target, string name)
        {
            var f = target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(f, "field '" + name + "' not found on " + target.GetType().Name);
            return (T)f.GetValue(target);
        }

        // Reads a serialized TMPro label's `text` via reflection so this assembly
        // need not reference the TMPro type directly (matching the green sibling
        // ClusterReconcilePlayModeTests, which avoids naming the type too).
        private static string GetPrivateText(object target, string name)
        {
            var f = target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(f, "field '" + name + "' not found on " + target.GetType().Name);
            var val = f.GetValue(target);
            Assert.IsNotNull(val, "field '" + name + "' is null on " + target.GetType().Name);
            var textProp = val.GetType().GetProperty("text");
            Assert.IsNotNull(textProp, "field '" + name + "' has no 'text' property.");
            return textProp.GetValue(val)?.ToString();
        }
    }
}
