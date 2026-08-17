using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using TileStories;

namespace TileStories.Tests
{
    // PlayMode companion to ClusterReconcileTests: the two Dissolve tests that reach
    // Destroy() in SweepStaleClusterViews must run here (Destroy is illegal in EditMode).
    public class ClusterReconcilePlayModeTests
    {
        private const string ClusterPrefab = "Assets/Framework/Runtime/UI/Markers/POI_Cluster.prefab";
        private readonly List<GameObject> _tracked = new();

        [SetUp] public void SetUp() => ARZoomState.SetZoom(1f, 1f, 100f);

        [TearDown]
        public void TearDown()
        {
            foreach (var go in _tracked) if (go) Object.DestroyImmediate(go);
            _tracked.Clear();
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
#if UNITY_EDITOR
            var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(ClusterPrefab);
            Assert.IsNotNull(prefab, "POI_Cluster.prefab missing at " + ClusterPrefab);
            SetPrivate(controller, "_clusterPrefab", prefab);
#else
            Assert.Fail("ClusterReconcilePlayModeTests requires the Unity Editor (AssetDatabase).");
#endif
            // _wallSession left null: spawnRoot -> controller.transform, iconLibrary -> null.
            return controller;
        }

        private (MarkerView view, VisualUnit unit) MakeUnit(string poiId, Vector3 worldPosition,
            DensityState state = DensityState.Clustered, int neighbors = 6, int priority = 0)
        {
            var go = new GameObject("member_" + poiId, typeof(MarkerView), typeof(CanvasGroup));
            _tracked.Add(go);
            var view = go.GetComponent<MarkerView>();
            SetPoiId(view, poiId);
            return (view, new VisualUnit
            {
                marker = view, poiId = poiId, worldPosition = worldPosition,
                effectiveDistance = Vector3.Distance(Vector3.zero, worldPosition),
                densityState = state, neighborCount = neighbors, hierarchyLevelIndex = priority,
                isVisible = true, shrinkScale = 1f,
            });
        }

        private static void SetPoiId(MarkerView view, string id)
        {
            var p = typeof(MarkerView).GetProperty("PoiId",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(p, "MarkerView.PoiId property not found");
            p.SetValue(view, id);
        }

        private (MarkerView view, VisualUnit unit)[] Members(Vector3 pos, params string[] ids)
        {
            var arr = new (MarkerView view, VisualUnit unit)[ids.Length];
            for (int i = 0; i < ids.Length; i++) arr[i] = MakeUnit(ids[i], pos);
            return arr;
        }

        private static List<VisualUnit> Run(LODController controller, params VisualUnit[] units)
        {
            var list = new List<VisualUnit>(units);
            controller.ReconcileClusters(ref list);
            return list;
        }

        private static void AssertPooled(LODController controller, int count)
            => Assert.AreEqual(count, GetPrivate<List<MarkerClusterView>>(controller, "_activeClusterViews").Count);

        // --- Dissolve destroy-path tests (moved from EditMode: Destroy() is PlayMode-only) ---

        [UnityTest]
        public IEnumerator Dissolve_GraceCycles_PooledViewSurvivesUntilThirdMiss()
        {
            var s = Settings("cluster"); s.cluster_dissolve_grace_cycles = 3;
            var c = MakeController(s, Vector3.zero);
            var m = Members(new Vector3(0, 0, -4f), "p1", "p2");
            Run(c, m[0].unit, m[1].unit); // c1: view created + pooled
            Assert.AreEqual(1, GetPrivate<List<MarkerClusterView>>(c, "_activeClusterViews").Count);
            Run(c); // c2: miss 1 -> survives (grace 3)
            Assert.AreEqual(1, GetPrivate<List<MarkerClusterView>>(c, "_activeClusterViews").Count);
            Run(c); // c3: miss 2 -> survives
            Assert.AreEqual(1, GetPrivate<List<MarkerClusterView>>(c, "_activeClusterViews").Count);
            Run(c); // c4: miss 3 (>= grace) -> hidden + removed from pool + Destroy scheduled
            Assert.AreEqual(0, GetPrivate<List<MarkerClusterView>>(c, "_activeClusterViews").Count,
                "_activeClusterViews list rebuilt without the dissolved view");
            yield return new WaitForSeconds(0.4f); // let delayed Destroy + fade finish
        }

        [UnityTest]
        public IEnumerator Dissolve_GraceZero_FadesOutImmediately()
        {
            var s = Settings("cluster"); s.cluster_dissolve_grace_cycles = 0;
            var c = MakeController(s, Vector3.zero);
            var m = Members(new Vector3(0, 0, -4f), "p1", "p2");
            Run(c, m[0].unit, m[1].unit); // c1: pooled
            var v = GetPrivate<List<MarkerClusterView>>(c, "_activeClusterViews")[0];
            Run(c); // c2: grace 0 -> faded + Destroy scheduled
            Assert.AreEqual(0, GetPrivate<List<MarkerClusterView>>(c, "_activeClusterViews").Count,
                "_activeClusterViews list rebuilt without the dissipated view");
            yield return new WaitForSeconds(0.4f); // let Destroy(gameObject, 0.3f) fire
            Assert.IsTrue(v == null, "grace-0 view destroyed immediately (no grace survival)");
        }
    }
}