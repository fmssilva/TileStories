using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TileStories.Tests
{
    // LOD domain (_2.4), end to end on REAL objects: the shipped LivingRoom config spawned by a real
    // WallSession through the real POI_Marker prefab, evaluated by a real LODController holding the
    // real POI_Cluster prefab, seen by a real MainCamera rendering into a fixed portrait target (so
    // pixel sizes are deterministic). The lamp family (6 POIs within 30 cm) is the crowded region.
    // Every assertion reads what the visitor would see: CanvasGroup alpha, root scale, cluster views.
    public class LodRealPipelineTests
    {
        private const string MarkerPrefabPath = "Assets/Framework/Runtime/UI/Markers/POI_Marker.prefab";
        private const string ClusterPrefabPath = "Assets/Framework/Runtime/UI/Markers/POI_Cluster.prefab";
        private static readonly string[] LampFamily =
            { "lamp", "lamp_religious", "lamp_military", "lamp_residential", "lamp_economic", "lamp_infrastructure" };

        private readonly List<Object> _tracked = new();
        private WallConfigData _config;
        private WallSession _session;
        private LODController _lod;
        private Camera _camera;
        private Transform _spawnRoot;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            WallConfigData config = null;
            yield return WallConfigLoader.LoadFromStreamingAssets("LivingRoom/config.json", c => config = c);
            Assert.IsNotNull(config, "StreamingAssets/LivingRoom/config.json must load for this test.");
            _config = config;
            _config.effect_defaults.preview = new EffectDefaults.EffectPreviewSettings();
            ARZoomState.SetZoom(1f, 1f, 4f);

            var camGO = new GameObject("LodTestCamera", typeof(Camera)) { tag = "MainCamera" };
            _camera = camGO.GetComponent<Camera>();
            _camera.fieldOfView = 60f;
            var target = new RenderTexture(1080, 1920, 16);
            _camera.targetTexture = target;
            _tracked.Add(camGO);
            _tracked.Add(target);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            MarkerHierarchyResolver.ResetToDefaults();
            StatusRamp.ResetToDefaults();
            ARZoomState.SetZoom(1f, 1f, 4f);
            foreach (var o in _tracked)
                if (o != null) Object.DestroyImmediate(o);
            _tracked.Clear();
            yield return null;
        }

        // Spawn the real wall with these LOD settings and wire a real LODController to it
        private void Build(LodSettings lod)
        {
            _config.lod_settings = lod;
            var anchorGO = new GameObject("PlacementCorrectionAnchor");
            _spawnRoot = anchorGO.transform;
            var wsGO = new GameObject("WallSessionHolder");
            wsGO.SetActive(false); // no Awake: no config reload, no tracker lookup
            _session = wsGO.AddComponent<WallSession>();
            _tracked.Add(anchorGO);
            _tracked.Add(wsGO);
#if UNITY_EDITOR
            SetField(_session, "_config", _config);
            SetField(_session, "poiAnchorPrefab", AssetDatabase.LoadAssetAtPath<GameObject>(MarkerPrefabPath));
            SetField(_session, "correctionAnchor", _spawnRoot);
            _session.GetType().GetMethod("SpawnPOIs", BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(_session, null);

            var lodGO = new GameObject("LODController", typeof(LODController));
            _tracked.Add(lodGO);
            _lod = lodGO.GetComponent<LODController>();
            SetField(_lod, "_wallSession", _session);
            SetField(_lod, "_camera", _camera);
            SetField(_lod, "_clusterPrefab", AssetDatabase.LoadAssetAtPath<GameObject>(ClusterPrefabPath));
#else
            Assert.Fail("Needs the Editor (AssetDatabase).");
#endif
            // settle every reveal so alpha/scale read the resting look, not a mid-animation frame
            foreach (var m in _session.SpawnedMarkers)
                m.GetComponent<MarkerRevealEffect>()?.SkipToEnd();
            if (!_config.demo_field.enabled)
                Assert.AreEqual(_config.pois.Count, _session.SpawnedMarkers.Count, "precondition: every POI spawned");
        }

        // A LOD setup with instant fades and no automatic cycles (the test drives Evaluate itself)
        private static LodSettings Settings(string mode, List<LodBandEntry> bands = null) => new LodSettings
        {
            enabled = true,
            bands = bands ?? LodSettings.DefaultBands(),
            density_response_mode = mode,
            density_radius_px = 300f,
            shrink_start_neighbor_count = 1,
            cluster_min_count = 5,
            shrink_min_factor = 0.25f,
            density_safety_escalation_enabled = false,
            transition_fade_duration_s = 0f,
            evaluation_interval_s = 999f,
            frustum_culling_enabled = true,
            cluster_dissolve_grace_cycles = 0,
        };

        private Vector3 LampCentroid()
        {
            var sum = Vector3.zero;
            foreach (var id in LampFamily) sum += Marker(id).transform.position;
            return sum / LampFamily.Length;
        }

        // Put the camera `distance` metres in front of the lamp family, looking straight at it
        private void LookAtLampsFrom(float distance)
        {
            var c = LampCentroid();
            _camera.transform.position = c + Vector3.forward * distance;
            _camera.transform.LookAt(c);
        }

        // Crowding radius (px) that just covers the lamp family on screen from the current camera, so
        // every lamp marker has all 5 siblings as neighbours: measured, never a guessed constant.
        private float RadiusCoveringLampFamily()
        {
            float max = 0f;
            foreach (var a in LampFamily)
            foreach (var b in LampFamily)
            {
                Vector2 pa = _camera.WorldToScreenPoint(Marker(a).transform.position);
                Vector2 pb = _camera.WorldToScreenPoint(Marker(b).transform.position);
                max = Mathf.Max(max, (pa - pb).magnitude);
            }
            return max + 10f;
        }

        private MarkerView Marker(string id) => _session.SpawnedMarkers.First(m => m.PoiId == id);
        private static float Alpha(MarkerView m) => m.GetComponent<CanvasGroup>().alpha;
        private MarkerClusterView[] Clusters() => _spawnRoot.GetComponentsInChildren<MarkerClusterView>(true);

        // Two cycles: density/cluster decisions only commit on the second agreeing cycle (spec 5)
        private void EvaluateTwice()
        {
            _lod.Evaluate();
            _lod.Evaluate();
        }

        [UnityTest]
        public IEnumerator HiddenByCountCap_ComesBack_WhenTheCameraWalksCloser()
        {
            // far band shows ONE marker; near band (under 2 m) shows all
            Build(Settings("none", new List<LodBandEntry>
            {
                new LodBandEntry { max_distance_m = 2f, max_visible_count = -1 },
                new LodBandEntry { max_distance_m = 9999f, max_visible_count = 1 },
            }));
            yield return null;

            LookAtLampsFrom(6f);
            EvaluateTwice();
            Assert.AreEqual(0f, Alpha(Marker("lamp_religious")), 1e-4f,
                "precondition: at 6 m the count cap of 1 hides a level-2 lamp marker");

            LookAtLampsFrom(1.2f);
            EvaluateTwice();
            foreach (var id in LampFamily)
                Assert.AreEqual(1f, Alpha(Marker(id)), 1e-4f,
                    id + " must be fully visible again once the camera is inside the unlimited 2 m band");
        }

        [UnityTest]
        public IEnumerator ShrinkAndFade_ScalesAndFadesTheCrowdedRealMarkers_ToTheConfiguredFloor()
        {
            Build(Settings("shrink_and_fade"));
            yield return null;

            LookAtLampsFrom(3f);
            _config.lod_settings.density_radius_px = RadiusCoveringLampFamily();
            EvaluateTwice();
            foreach (var id in LampFamily)
            {
                var m = Marker(id);
                // 5 neighbours = cluster_min_count -> the ramp's end = shrink_min_factor (0.25)
                Assert.AreEqual(0.25f, m.transform.localScale.x, 1e-3f, id + ": root scale follows the crowding factor");
                Assert.AreEqual(0.25f, Alpha(m), 1e-3f, id + ": opacity follows the same factor");
                Assert.IsTrue(m.IsVisible, id + ": Shrink & Fade never hides a marker");
            }
            Assert.AreEqual(0, Clusters().Length, "Shrink & Fade never builds clusters");
        }

        [UnityTest]
        public IEnumerator SelectAndHide_RealLampFamily_KeepsItsMostImportantMarkers_AndHidesTheRest()
        {
            var lod = Settings("select_hide");
            lod.cluster_min_count = 3;   // Crowded At: at most 3 of the family may stay near each other
            Build(lod);
            yield return null;

            LookAtLampsFrom(1.5f);
            lod.density_radius_px = RadiusCoveringLampFamily();
            EvaluateTwice();

            // levels: lamp 1, religious 2, military 3 stay; residential 4, economic 5, infrastructure 5 go
            foreach (var id in new[] { "lamp", "lamp_religious", "lamp_military" })
                Assert.AreEqual(1f, Alpha(Marker(id)), 1e-4f, id + " is among the 3 most important: it must stay");
            foreach (var id in new[] { "lamp_residential", "lamp_economic", "lamp_infrastructure" })
                Assert.AreEqual(0f, Alpha(Marker(id)), 1e-4f, id + " is less important: hidden");
            Assert.AreEqual(0, Clusters().Length, "Select & Hide never builds clusters (Safety Net off)");
        }

        [UnityTest]
        public IEnumerator Cluster_RealLampFamily_BecomesOneMarkerSizedCluster_ThatIsTappable()
        {
            Build(Settings("cluster"));
            yield return null;

            LookAtLampsFrom(1.5f);
            _config.lod_settings.density_radius_px = RadiusCoveringLampFamily();
            EvaluateTwice();

            var clusters = Clusters();
            Assert.AreEqual(1, clusters.Length, "the lamp family collapses into exactly one cluster");
            var cluster = clusters[0];
            CollectionAssert.AreEquivalent(LampFamily, cluster.MemberPoiIds, "cluster members are the lamp POIs");
            foreach (var id in LampFamily)
                Assert.AreEqual(0f, Alpha(Marker(id)), 1e-4f, id + " is absorbed (hidden) while its cluster shows");

            float largest = LampFamily.Max(id => Marker(id).SymbolDiameterMetres);
            Assert.AreEqual(MarkerClusterView.ComputeDiameterMetres(largest, _config.lod_settings.cluster_size_ratio, 6), cluster.DiameterMetres, 1e-4f,
                "cluster diameter = largest member symbol x cluster_size_ratio x count growth");
            Assert.AreEqual(cluster.DiameterMetres, ((RectTransform)cluster.transform).sizeDelta.x * cluster.transform.lossyScale.x, 1e-4f,
                "the cluster's real world width equals its diameter (metres, not pixels)");

            // Tier 0.5 tap target (WCAG 2.5.5): the cluster's projected width on the real camera
            float dist = Vector3.Distance(_camera.transform.position, cluster.transform.position);
            float pixelsPerMetre = _camera.pixelHeight / (2f * dist * Mathf.Tan(_camera.fieldOfView * 0.5f * Mathf.Deg2Rad));
            float widthPx = cluster.DiameterMetres * pixelsPerMetre;
            Assert.GreaterOrEqual(widthPx, 44f, "cluster must be at least 44 px wide on a 1080x1920 screen at 1.5 m");
            Assert.Less(widthPx, _camera.pixelWidth * 0.5f, "and never cover most of the screen");
        }

        [UnityTest]
        public IEnumerator BandSource_DecidesTheClusterBand()
        {
            // bands chosen so the lamp family straddles 2 m: nearest member inside, farthest outside
            var bands = new List<LodBandEntry>
            {
                new LodBandEntry { max_distance_m = 2f, max_visible_count = -1 },
                new LodBandEntry { max_distance_m = 9999f, max_visible_count = 15 },
            };
            var lod = Settings("cluster", bands);
            Build(lod);
            yield return null;
            LookAtLampsFrom(2.05f); // centroid just beyond 2 m; the family is 60 cm deep
            lod.density_radius_px = RadiusCoveringLampFamily();

            int BandFor(string source)
            {
                lod.cluster_band_source = source;
                _lod.RestoreAllMarkers();
                var units = RunStagesUpToClusters();
                var agg = units.Single(u => u.clusterMembers != null);
                return agg.band.Index;
            }

            Assert.AreEqual(1, BandFor("centroid"), "centroid (2.05 m) is in the far band");
            Assert.AreEqual(0, BandFor("nearest_member"), "the nearest lamp member is inside 2 m");
            Assert.AreEqual(1, BandFor("farthest_member"), "the farthest lamp member is beyond 2 m");
        }

        // Evaluate()'s real stages 1-5b, returning the visual units right after cluster reconciliation
        private List<VisualUnit> RunStagesUpToClusters()
        {
            SetField(_lod, "_settings", _config.lod_settings);
            List<VisualUnit> units = null;
            for (int cycle = 0; cycle < 2; cycle++)
            {
                var all = _session.SpawnedMarkers.ToList();
                var visible = _lod.FrustumCull(all);
                var distances = _lod.ComputeEffectiveDistances(visible);
                var bands = _lod.AssignBands(distances);
                var counts = _lod.EvaluateDensity(visible);
                units = _lod.ApplyDensityResponse(visible, all, bands, distances, counts);
                _lod.ReconcileClusters(ref units);
            }
            return units;
        }

        [UnityTest]
        public IEnumerator SwitchingLodOff_Live_RestoresEveryMarkerAndRemovesClusters()
        {
            Build(Settings("cluster"));
            yield return null;
            LookAtLampsFrom(1.5f);
            _config.lod_settings.density_radius_px = RadiusCoveringLampFamily();
            EvaluateTwice();
            Assert.AreEqual(1, Clusters().Length, "precondition: a cluster exists");
            Assert.AreEqual(0f, Alpha(Marker("lamp")), 1e-4f, "precondition: lamp absorbed");

            // live Play Mode edit: the wall gets a NEW settings object with LOD off
            var off = Settings("cluster");
            off.enabled = false;
            _config.lod_settings = off;
            yield return null; // LODController.Update notices the swap
            yield return null; // Destroy() lands at end of frame

            Assert.AreEqual(0, Clusters().Length, "every cluster view is removed");
            foreach (var m in _session.SpawnedMarkers)
            {
                Assert.AreEqual(1f, Alpha(m), 1e-4f, m.PoiId + " fully visible again");
                Assert.AreEqual(1f, m.transform.localScale.x, 1e-4f, m.PoiId + " back to full size");
            }
        }

        [UnityTest]
        public IEnumerator SelectionDim_SurvivesAnLodHideAndShow()
        {
            Build(Settings("none", new List<LodBandEntry>
            {
                new LodBandEntry { max_distance_m = 2f, max_visible_count = -1 },
                new LodBandEntry { max_distance_m = 9999f, max_visible_count = 1 },
            }));
            yield return null;
            var m = Marker("lamp_religious");
            m.SetSelectionAlpha(0.3f, 0f);

            LookAtLampsFrom(6f);
            EvaluateTwice();
            Assert.AreEqual(0f, Alpha(m), 1e-4f, "precondition: hidden by the far band's cap");
            LookAtLampsFrom(1.2f);
            EvaluateTwice();
            Assert.AreEqual(0.3f, Alpha(m), 1e-4f, "comes back at its selection dim, not full and not zero");
        }

        // ---------------- the LOD demo field through the real pipeline ----------------

        // Spawn the wall with the demo field on (the wall's own POIs hidden), camera at the origin looking +z
        private void BuildWithDemoField(LodSettings lod, int perLevel, int clump)
        {
            _camera.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            _config.demo_field = new DemoFieldSettings
            {
                enabled = true, seed = 7, distance_m = 1.5f, width_m = 4f, height_m = 2f, depth_m = 8f,
                dense_clump_count = clump, dense_clump_radius_m = 0.08f,
                level_counts = _config.hierarchy_levels.Select(l => new DemoFieldLevelCount { level_key = l.key, count = perLevel }).ToList(),
            };
            Build(lod);
        }

        private int VisibleCount() => _session.SpawnedMarkers.Count(m => Alpha(m) > 0.001f);

        [UnityTest]
        public IEnumerator DemoField_ZoomingIn_RevealsMoreIndividualMarkers()
        {
            // default bands: all under 2 m, 15 under 7 m, 5 beyond -- the field spans 1.5 m .. 9.5 m
            BuildWithDemoField(Settings("none", LodSettings.DefaultBands()), perLevel: 8, clump: 0);
            yield return null;
            Assert.AreEqual(_config.hierarchy_levels.Count * 8, _session.SpawnedMarkers.Count, "precondition: only demo markers run");

            EvaluateTwice();
            int atOneX = VisibleCount();
            ARZoomState.SetZoom(4f, 1f, 4f);
            EvaluateTwice();
            int atFourX = VisibleCount();

            Assert.Less(atOneX, _session.SpawnedMarkers.Count, "precondition: the band caps thin the field at 1x");
            Assert.Greater(atFourX, atOneX, "zooming in (effective distance / 4) must unlock more markers -- the spec's zoom check");
        }

        [UnityTest]
        public IEnumerator DemoField_Clump_BecomesACluster_InHybrid_WhileTheScatteredMarkersStay()
        {
            var lod = Settings("hybrid");
            BuildWithDemoField(lod, perLevel: 1, clump: 8);
            yield return null;

            // crowding radius measured from the clump's real on-screen spread (never a guess)
            var clumpMarkers = _session.SpawnedMarkers.Where(m => m.PoiId.StartsWith("demo_clump_")).ToList();
            Assert.AreEqual(8, clumpMarkers.Count, "precondition: the clump spawned");
            float spread = 0f;
            foreach (var a in clumpMarkers)
            foreach (var b in clumpMarkers)
                spread = Mathf.Max(spread, ((Vector2)_camera.WorldToScreenPoint(a.transform.position) - (Vector2)_camera.WorldToScreenPoint(b.transform.position)).magnitude);
            lod.density_radius_px = spread + 10f;
            EvaluateTwice();

            var clusters = Clusters();
            Assert.GreaterOrEqual(clusters.Length, 1, "the dense clump collapses into a cluster");
            var clumpIds = clumpMarkers.Select(m => m.PoiId).ToList();
            Assert.IsTrue(clusters.Any(c => clumpIds.All(id => c.MemberPoiIds.Contains(id))), "one cluster holds the whole clump");
        }

        // ---------------- Tier 1 evidence: what the camera really renders ----------------

        // Render the test camera once and save it under Assets/Screenshots (a vision-check artefact);
        // returns the rendered pixels (the caller destroys the texture)
        private Texture2D RenderToPng(string fileName)
        {
            _camera.clearFlags = CameraClearFlags.SolidColor;
            _camera.backgroundColor = new Color(0.16f, 0.17f, 0.20f, 1f);
            var rt = _camera.targetTexture;
            _camera.Render();
            var previous = RenderTexture.active;
            RenderTexture.active = rt;
            var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();
            RenderTexture.active = previous;
            string dir = System.IO.Path.Combine(Application.dataPath, "Screenshots");
            System.IO.Directory.CreateDirectory(dir);
            System.IO.File.WriteAllBytes(System.IO.Path.Combine(dir, fileName), tex.EncodeToPNG());
            return tex;
        }

        // The rendered colour at a world point, read from the texture the camera rendered into
        private Color PixelAt(Texture2D tex, Vector3 world)
        {
            Vector3 sp = _camera.WorldToScreenPoint(world);
            return tex.GetPixel(Mathf.RoundToInt(sp.x), Mathf.RoundToInt(sp.y));
        }

        [UnityTest]
        public IEnumerator DemoField_RendersEachResponseMode_ForTheVisionCheck(
            [Values("none", "shrink_and_fade", "cluster", "hybrid")] string mode)
        {
            var lod = Settings(mode);
            lod.density_radius_px = 150f;
            BuildWithDemoField(lod, perLevel: 3, clump: 8);
            yield return null;
            EvaluateTwice();
            yield return null;

            int clusters = Clusters().Length;
            float smallest = _session.SpawnedMarkers.Where(m => m.IsVisible).Min(m => m.DensityFactor);
            if (mode == "none") { Assert.AreEqual(0, clusters); Assert.AreEqual(1f, smallest, 1e-4f, "None never shrinks"); }
            if (mode == "shrink_and_fade") { Assert.AreEqual(0, clusters); Assert.Less(smallest, 1f, "the clump shrinks"); }
            if (mode == "cluster" || mode == "hybrid") Assert.GreaterOrEqual(clusters, 1, "the clump clusters");
            var tex = RenderToPng("LodDemoField_" + mode + ".png");
            try
            {
                if (mode == "cluster" || mode == "hybrid")
                {
                    // Tier 0.5 pixel check: the cluster really draws its pie + dark centre disc (it once
                    // rendered as a plain white square covering both)
                    var cluster = Clusters()[0];
                    var centre = cluster.transform.position;
                    var ring = centre + _camera.transform.right * (cluster.DiameterMetres * 0.40f);
                    Color hole = PixelAt(tex, centre + _camera.transform.up * (cluster.DiameterMetres * 0.2f));
                    Color pieRing = PixelAt(tex, ring);
                    Assert.Less(hole.grayscale, 0.3f, "the centre disc is dark (the '+N' sits on it), not white: " + hole);
                    Assert.IsFalse(pieRing.r > 0.95f && pieRing.g > 0.95f && pieRing.b > 0.95f, "the pie ring is coloured, not plain white: " + pieRing);
                    Assert.Greater(Mathf.Abs(pieRing.grayscale - _camera.backgroundColor.grayscale), 0.05f, "the pie ring is drawn (not background): " + pieRing);
                    // the pie closes all the way round: no wedge-shaped gap anywhere in the ring
                    for (int i = 0; i < 12; i++)
                    {
                        float a = i * 30f * Mathf.Deg2Rad;
                        var p = centre + (_camera.transform.right * Mathf.Cos(a) + _camera.transform.up * Mathf.Sin(a)) * (cluster.DiameterMetres * 0.40f);
                        Color c = PixelAt(tex, p);
                        Assert.Greater(Mathf.Abs(c.grayscale - _camera.backgroundColor.grayscale), 0.05f,
                            "pie ring drawn at " + (i * 30) + " degrees (a gap shows the background): " + c);
                    }
                }
            }
            finally { Object.DestroyImmediate(tex); }
        }

        [UnityTest]
        public IEnumerator DemoField_RendersZoomBeforeAndAfter_ForTheVisionCheck()
        {
            BuildWithDemoField(Settings("none", LodSettings.DefaultBands()), perLevel: 8, clump: 0);
            yield return null;
            EvaluateTwice();
            int before = VisibleCount();
            Object.DestroyImmediate(RenderToPng("LodDemoField_zoom_1x.png"));

            ARZoomState.SetZoom(4f, 1f, 4f);
            EvaluateTwice();
            int after = VisibleCount();
            Object.DestroyImmediate(RenderToPng("LodDemoField_zoom_4x_lod_only.png")); // same FOV: shows the LOD unlock alone
            Assert.Greater(after, before, "more markers after zooming in");
        }

        private static void SetField(object obj, string name, object value)
        {
            var f = obj.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            Assert.IsNotNull(f, name + " not found on " + obj.GetType().Name);
            f.SetValue(obj, value);
        }
    }
}
