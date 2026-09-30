using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using TileStories;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TileStories.Tests
{
    // Block 4 composition test (_2.1_Marker_Orientation.md section 15, test (c)): a
    // spawned cluster aggregate's rotation must change when the camera rotates, driven
    // through the REAL LODController cluster-spawn path (not a hand-built cluster -
    // 40-testing.md 4.2.1 requires the actual call site be reached, not just grep-visible).
    // This is the direct regression test for S7 (POI_Cluster.prefab previously had no
    // MarkerBillboard at all and was spawned with Quaternion.identity forever).
    // Harness mirrors ClusterPipelineIntegrationTests.cs's proven real-config approach.
    public class OrientationClusterIntegrationTests
    {
        private const string MarkerPrefab = "Assets/Framework/Runtime/UI/Markers/POI_Marker.prefab";
        private const string ClusterPrefab = "Assets/Framework/Runtime/UI/Markers/POI_Cluster.prefab";

        private static readonly (string id, string category)[] LampFamily =
        {
            ("lamp", "Royal Government"),
            ("lamp_religious", "religious"),
            ("lamp_military", "military"),
            ("lamp_residential", "residential"),
            ("lamp_economic", "economic"),
            ("lamp_infrastructure", "infrastructure"),
        };

        private static readonly Vector3 LampCentroid = new Vector3(-0.95f, -0.87f, -4.18f);

        private WallConfigData _config;
        private LODController _controller;
        private WallSession _wallSession;
        private Camera _camera;
        private readonly List<MarkerView> _allMarkers = new();
        private readonly List<GameObject> _tracked = new();

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            WallConfigData config = null;
            yield return WallConfigLoader.LoadFromStreamingAssets("LivingRoom/config.json", c => config = c);
            Assert.IsNotNull(config, "StreamingAssets/LivingRoom/config.json must load for this integration test.");
            _config = config;

            CategoryPalette.Configure(_config.category_styles);
            MarkerHierarchyResolver.Configure(_config.hierarchy_levels);

            // - MarkerBillboard finds its camera through Camera.main, which is the FIRST MainCamera-tagged camera alive: one left by
            // an earlier fixture (seen in full runs and in the model fixtures run before this one) made every billboard follow that
            // camera, so turning this test's own camera moved nothing. Clear strays first, as EffectsPreviewRenderTests does.
            foreach (var stray in GameObject.FindGameObjectsWithTag("MainCamera"))
                Object.DestroyImmediate(stray);
            var camGO = new GameObject("TestCam", typeof(Camera));
            camGO.tag = "MainCamera";
            _camera = camGO.GetComponent<Camera>();
            Assert.AreSame(_camera, Camera.main, "precondition: this test's camera is THE main camera the billboards will follow");
            _camera.transform.position = LampCentroid + new Vector3(0f, 0f, 20f);
            _camera.transform.LookAt(LampCentroid);
            _camera.fieldOfView = 60f;
            _tracked.Add(camGO);

            // Real WallSession, inactive so Awake never fires - same idiom as
            // ARZoomRoutingTests.cs - with _config injected so its OrientationSettings
            // property resolves for LODController.EnsureSettings to pick up.
            var correctionAnchorGO = new GameObject("PlacementCorrectionAnchor");
            _tracked.Add(correctionAnchorGO);

            var wsGO = new GameObject("WallSessionHolder");
            wsGO.SetActive(false);
            _wallSession = wsGO.AddComponent<WallSession>();
            SetPrivate(_wallSession, "_config", _config);
            SetPrivate(_wallSession, "correctionAnchor", correctionAnchorGO.transform);
            _tracked.Add(wsGO);

            ARZoomState.SetZoom(1f, 1f, 100f);
            _controller = MakeController(TestSettings());
            SetPrivate(_controller, "_camera", _camera);
            SetPrivate(_controller, "_wallSession", _wallSession);

            var prefab = LoadMarkerPrefab();
            foreach (var (id, category) in LampFamily)
            {
                var poi = _config.pois.FirstOrDefault(p => p.id == id);
                Assert.IsNotNull(poi, $"Lamp family POI '{id}' must exist in LivingRoom/config.json.");
                Assert.IsTrue(POIPositionResolver.TryResolvePosition(poi, out var worldPos));

                var root = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                root.transform.position = worldPos;
                _tracked.Add(root);

                var anchor = root.GetComponent<POIAnchor>() ?? root.AddComponent<POIAnchor>();
                anchor.Initialise(poi);
                var marker = root.GetComponent<MarkerView>();
                marker.Initialise(anchor, MarkerVisualSettings.Default(), MarkerEffectFlags.None);
                _allMarkers.Add(marker);
            }

            Assert.AreEqual(6, _allMarkers.Count, "must spawn the 6 real lamp_* markers.");
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            MarkerHierarchyResolver.ResetToDefaults();
            foreach (var go in _tracked) if (go) Object.DestroyImmediate(go);
            _tracked.Clear();
            _allMarkers.Clear();
            yield return null;
        }

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

        [UnityTest]
        public IEnumerator RealConfig_LampFamily_ClusterRotationChangesWhenCameraRotates()
        {
            RunPipelineOnce(); // cycle 1: pendingClusters, densityState stays Normal
            RunPipelineOnce(); // cycle 2: densityState commits Clustered -> real aggregate forms

            var views = GetPrivate<List<MarkerClusterView>>(_controller, "_activeClusterViews");
            Assert.AreEqual(1, views.Count, "exactly one real aggregate must form for the lamp family.");

            var clusterGO = views[0].gameObject;
            var billboard = clusterGO.GetComponent<MarkerBillboard>();
            Assert.IsNotNull(billboard, "POI_Cluster.prefab must carry a MarkerBillboard (S7 regression).");
            Assert.IsNotNull(billboard.ConfiguredSettings, "LODController must have called MarkerBillboard.Configure for the new cluster view.");

            yield return null; // let LateUpdate resolve the first rotation
            Quaternion before = clusterGO.transform.rotation;

            _camera.transform.RotateAround(LampCentroid, Vector3.up, 60f);
            // - the billboard eases toward the new pose by Time.deltaTime (MarkerBillboard.RotationSmoothingTimeS), so ONE frame moves
            // it by however long that frame was: wait for the move by condition, with a frame cap, instead of betting on one
            // frame (40-testing 4.2.4b, "time is the test's").
            int frames = 0;
            while (frames < 60 && Quaternion.Angle(before, clusterGO.transform.rotation) <= 1f)
            {
                yield return null;
                frames++;
            }

            Quaternion after = clusterGO.transform.rotation;
            Assert.Greater(Quaternion.Angle(before, after), 1f,
                "the real spawned cluster's rotation must change when the camera rotates - it must not be stuck at Quaternion.identity forever (S7)."
                + " After " + frames + " frames, Time.deltaTime " + Time.deltaTime + ", timeScale " + Time.timeScale + ".");
        }

        // ---- harness (mirrors ClusterPipelineIntegrationTests.cs) ----

        private static LodSettings TestSettings() => new()
        {
            enabled = true,
            density_response_mode = "cluster",
            density_radius_px = 80f,
            cluster_min_count = 2,
            shrink_start_neighbor_count = 2,
            density_safety_escalation_enabled = false,
            cluster_icon_mode = "pie_and_count",
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

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ClusterPrefab);
            Assert.IsNotNull(prefab, "POI_Cluster.prefab missing at " + ClusterPrefab);
            SetPrivate(controller, "_clusterPrefab", prefab);
            return controller;
        }

        private static GameObject LoadMarkerPrefab()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(MarkerPrefab);
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
    }
}
