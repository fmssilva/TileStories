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
    // Live Play Mode updates of the orientation domain (_2.1 section 13.5): the runtime seams
    // WallSession.ApplyOrientationSettings / MarkerBillboard.ReapplySettings /
    // LODController.ReapplyClusterOrientation change what a RUNNING marker does, measured on real
    // rotations of real POI_Marker / POI_Cluster prefabs after real frames (LateUpdate + smoothing).
    // Harness = OrientationWallSessionIntegrationTests / OrientationClusterIntegrationTests idioms.
    public class OrientationLiveUpdateTests
    {
        private const string MarkerPrefabPath = "Assets/Framework/Runtime/UI/Markers/POI_Marker.prefab";
        private const string ClusterPrefabPath = "Assets/Framework/Runtime/UI/Markers/POI_Cluster.prefab";
        private const float Settle = 1.2f;       // seconds: >> the 0.12 s smoothing constant
        private const float AngleTolerance = 2f; // degrees

        private WallConfigData _config;
        private readonly List<GameObject> _tracked = new();
        private Camera _cam;
        private WallSession _ws;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            WallConfigData config = null;
            yield return WallConfigLoader.LoadFromStreamingAssets("LivingRoom/config.json", c => config = c);
            Assert.IsNotNull(config, "StreamingAssets/LivingRoom/config.json must load.");
            _config = config;
            _config.effect_defaults.preview = new EffectDefaults.EffectPreviewSettings(); // no grid camera in these tests
            // Known start: world-up, always facing the camera, every-frame updates, no level overrides.
            _config.orientation_settings = new OrientationSettings();
            foreach (var level in _config.hierarchy_levels) level.facing_mode_override = "";

            CategoryPalette.Configure(_config.category_styles);
            MarkerHierarchyResolver.Configure(_config.hierarchy_levels);

            foreach (var stray in GameObject.FindGameObjectsWithTag("MainCamera")) Object.DestroyImmediate(stray);
            var camGO = new GameObject("TestCamera") { tag = "MainCamera" };
            _cam = camGO.AddComponent<Camera>();
            _tracked.Add(camGO);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            MarkerHierarchyResolver.ResetToDefaults();
            foreach (var go in _tracked) if (go != null) Object.DestroyImmediate(go);
            _tracked.Clear();
            yield return null;
        }

        // Camera at (0,0,-2) looking along +Z, rolled by rollDeg, yawed by yawDeg about world up.
        private void PoseCamera(float yawDeg, float rollDeg)
        {
            var forward = Quaternion.Euler(0f, yawDeg, 0f) * Vector3.forward;
            var up = Quaternion.AngleAxis(rollDeg, forward) * Vector3.up;
            _cam.transform.SetPositionAndRotation(new Vector3(0f, 0f, -2f), Quaternion.LookRotation(forward, up));
        }

        private WallSession SpawnWall()
        {
            var anchor = new GameObject("PlacementCorrectionAnchor");
            _tracked.Add(anchor);
            var wsGO = new GameObject("WallSessionHolder");
            wsGO.SetActive(false);
            _tracked.Add(wsGO);
            var ws = wsGO.AddComponent<WallSession>();
            SetPrivate(ws, "_config", _config);
            SetPrivate(ws, "_effectDefaults", _config.effect_defaults);
            SetPrivate(ws, "poiAnchorPrefab", AssetDatabase.LoadAssetAtPath<GameObject>(MarkerPrefabPath));
            SetPrivate(ws, "correctionAnchor", anchor.transform);
            ws.GetType().GetMethod("SpawnPOIs", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(ws, null);
            foreach (var m in ws.SpawnedMarkers)
            {
                var reveal = m.GetComponent<MarkerRevealEffect>();
                if (reveal != null) { reveal.SkipToEnd(); }
            }
            _ws = ws;
            return ws;
        }

        private static OrientationSettings CopyOf(OrientationSettings s) => new OrientationSettings(s);

        private static List<HierarchyLevelEntry> LevelsCopy(WallConfigData c) =>
            JsonUtility.FromJson<LevelsHolder>(JsonUtility.ToJson(new LevelsHolder { levels = c.hierarchy_levels })).levels;

        [System.Serializable]
        private class LevelsHolder { public List<HierarchyLevelEntry> levels; }

        private static float Angle(Vector3 a, Vector3 b) => Vector3.Angle(a, b);

        // ---------------- vertical alignment ----------------

        [UnityTest]
        public IEnumerator VerticalAlignment_WorldUpToScreenUpAndBack_ChangesTheRunningMarker()
        {
            PoseCamera(0f, 30f);
            var ws = SpawnWall();
            var marker = ws.SpawnedMarkers[0];
            marker.transform.position = Vector3.zero;
            yield return new WaitForSeconds(Settle);

            Vector3 screenUp = MarkerOrientationResolver.ScreenUpWorld(_cam);
            Assert.Greater(Angle(screenUp, Vector3.up), 25f, "Precondition: the camera really is rolled.");
            Assert.Less(Angle(marker.transform.up, Vector3.up), AngleTolerance, "Precondition: world_up keeps the marker upright.");

            var screenUpSettings = CopyOf(_config.orientation_settings);
            screenUpSettings.vertical_alignment_mode = "screen_up";
            ws.ApplyOrientationSettings(screenUpSettings, LevelsCopy(_config));
            yield return new WaitForSeconds(Settle);
            Assert.Less(Angle(marker.transform.up, screenUp), AngleTolerance,
                "Live switch to screen_up: the marker must roll with the screen, without a restart.");

            var worldUpSettings = CopyOf(screenUpSettings);
            worldUpSettings.vertical_alignment_mode = "world_up";
            ws.ApplyOrientationSettings(worldUpSettings, LevelsCopy(_config));
            yield return new WaitForSeconds(Settle);
            Assert.Less(Angle(marker.transform.up, Vector3.up), AngleTolerance, "And back to world_up.");
        }

        [UnityTest]
        public IEnumerator LabelVerticalAlignment_ReachesTheLabelChildLive()
        {
            PoseCamera(0f, 30f);
            var ws = SpawnWall();
            var marker = ws.SpawnedMarkers[0];
            marker.transform.position = Vector3.zero;
            yield return new WaitForSeconds(Settle);

            var label = marker.transform.Find("Label");
            Assert.IsNotNull(label, "POI_Marker has a Label child.");
            Vector3 screenUp = MarkerOrientationResolver.ScreenUpWorld(_cam);
            Assert.Less(Angle(label.up, Vector3.up), AngleTolerance, "Precondition: the label inherits world_up.");

            var s = CopyOf(_config.orientation_settings);
            s.label_vertical_alignment_mode = "screen_up";
            ws.ApplyOrientationSettings(s, LevelsCopy(_config));
            yield return new WaitForSeconds(Settle);

            Assert.Less(Angle(label.up, screenUp), AngleTolerance, "The label alone must follow the screen now.");
            Assert.Less(Angle(marker.transform.up, Vector3.up), AngleTolerance, "The marker root must stay world_up.");
        }

        // ---------------- facing ----------------

        [UnityTest]
        public IEnumerator Facing_WallFixedToCameraAndBack_KeepsTheAuthoredRotation()
        {
            var poi = _config.pois[0];
            poi.editor_rotation_x_deg = 0f; poi.editor_rotation_deg = 0f; poi.editor_rotation_z_deg = 0f;
            _config.orientation_settings.facing_mode = "wall_fixed";
            PoseCamera(40f, 0f);
            var ws = SpawnWall();
            var marker = ws.SpawnedMarkers.First(m => m.name == poi.id);
            yield return new WaitForSeconds(Settle);
            Assert.Less(Quaternion.Angle(marker.transform.rotation, Quaternion.identity), AngleTolerance,
                "Precondition: wall_fixed keeps the authored (identity) rotation.");

            var facing = CopyOf(_config.orientation_settings);
            facing.facing_mode = "always_facing_camera";
            ws.ApplyOrientationSettings(facing, LevelsCopy(_config));
            yield return new WaitForSeconds(Settle);
            Assert.Greater(Quaternion.Angle(marker.transform.rotation, Quaternion.identity), 20f,
                "Live switch to always_facing_camera must turn the marker toward the camera.");

            var fixedAgain = CopyOf(facing);
            fixedAgain.facing_mode = "wall_fixed";
            ws.ApplyOrientationSettings(fixedAgain, LevelsCopy(_config));
            yield return new WaitForSeconds(Settle);
            Assert.Less(Quaternion.Angle(marker.transform.rotation, Quaternion.identity), AngleTolerance,
                "Back to wall_fixed the marker must return to its AUTHORED rotation (Configure would have lost it).");
        }

        // A POI's own Facing X/Y/Z (Specific Marker > Position) edited while the wall runs: a wall_fixed
        // marker turns to the new authored angle, and a yaw_only one takes its X/Z tilt (Y stays live).
        [UnityTest]
        public IEnumerator PoiFacing_LiveEdit_TurnsTheRunningMarkerToTheNewAuthoredAngle()
        {
            var poi = _config.pois[0];
            poi.editor_rotation_x_deg = 0f; poi.editor_rotation_deg = 0f; poi.editor_rotation_z_deg = 0f;
            _config.orientation_settings.facing_mode = "wall_fixed";
            PoseCamera(0f, 0f);
            var ws = SpawnWall();
            var marker = ws.SpawnedMarkers.First(m => m.name == poi.id);
            yield return new WaitForSeconds(Settle);
            Assert.Less(Quaternion.Angle(marker.transform.rotation, Quaternion.identity), AngleTolerance,
                "Precondition: wall_fixed shows the authored (identity) rotation.");

            // The window's private copy of the POI list, with this POI's facing edited
            var edited = JsonUtility.FromJson<PoisHolder>(JsonUtility.ToJson(new PoisHolder { pois = _config.pois })).pois;
            var editedPoi = edited.First(p => p.id == poi.id);
            editedPoi.editor_rotation_x_deg = 20f; editedPoi.editor_rotation_deg = 35f; editedPoi.editor_rotation_z_deg = 10f;
            ws.ApplyPoiFacing(edited);
            yield return new WaitForSeconds(Settle);

            var expected = Quaternion.Euler(20f, 35f, 10f);
            Assert.Less(Quaternion.Angle(marker.transform.rotation, expected), AngleTolerance,
                "wall_fixed: the running marker must turn to the newly authored Facing X/Y/Z, without a restart.");

            // yaw_only keeps the authored X/Z tilt but replaces Y with the live camera yaw
            var yawOnly = CopyOf(_config.orientation_settings);
            yawOnly.facing_mode = "yaw_only";
            ws.ApplyOrientationSettings(yawOnly, LevelsCopy(_config));
            yield return new WaitForSeconds(Settle);
            Vector3 euler = marker.transform.rotation.eulerAngles;
            Assert.AreEqual(20f, Mathf.DeltaAngle(0f, euler.x), AngleTolerance, "yaw_only keeps the new X tilt");
            Assert.AreEqual(10f, Mathf.DeltaAngle(0f, euler.z), AngleTolerance, "yaw_only keeps the new Z tilt");
        }

        [System.Serializable]
        private class PoisHolder { public List<POIData> pois; }

        [UnityTest]
        public IEnumerator Facing_LevelOverride_AppliesOnlyToThatLevelsMarkersLive()
        {
            var levelA = _config.hierarchy_levels[0];
            var poiA = _config.pois.First(p => p.hierarchy_level_key == levelA.key);
            var poiB = _config.pois.First(p => p.hierarchy_level_key != levelA.key);
            foreach (var p in new[] { poiA, poiB }) { p.editor_rotation_x_deg = 0f; p.editor_rotation_deg = 0f; p.editor_rotation_z_deg = 0f; }
            PoseCamera(40f, 0f);
            var ws = SpawnWall();
            var a = ws.SpawnedMarkers.First(m => m.name == poiA.id);
            var b = ws.SpawnedMarkers.First(m => m.name == poiB.id);
            yield return new WaitForSeconds(Settle);
            Assert.Greater(Quaternion.Angle(a.transform.rotation, Quaternion.identity), 20f, "Precondition: both face the camera.");
            Assert.Greater(Quaternion.Angle(b.transform.rotation, Quaternion.identity), 20f);

            var levels = LevelsCopy(_config);
            levels.First(l => l.key == levelA.key).facing_mode_override = "wall_fixed";
            ws.ApplyOrientationSettings(CopyOf(_config.orientation_settings), levels);
            yield return new WaitForSeconds(Settle);

            Assert.Less(Quaternion.Angle(a.transform.rotation, Quaternion.identity), AngleTolerance,
                "The overridden level's marker must become wall_fixed.");
            Assert.Greater(Quaternion.Angle(b.transform.rotation, Quaternion.identity), 20f,
                "Another level's marker must keep facing the camera.");
        }

        // ---------------- update cost ----------------

        [UnityTest]
        public IEnumerator UpdateCost_ChangeIsAppliedEvenWhenTheCameraIsStillAndTheModeWaitsForCameraMovement()
        {
            _config.orientation_settings.update_mode = "on_camera_delta";
            _config.orientation_settings.camera_delta_deg = 90f;   // the camera never moves this much here
            PoseCamera(0f, 30f);
            var ws = SpawnWall();
            var marker = ws.SpawnedMarkers[0];
            marker.transform.position = Vector3.zero;
            yield return new WaitForSeconds(0.3f);

            var s = CopyOf(_config.orientation_settings);
            s.vertical_alignment_mode = "screen_up";
            ws.ApplyOrientationSettings(s, LevelsCopy(_config));
            yield return new WaitForSeconds(Settle);

            Assert.Less(Angle(marker.transform.up, MarkerOrientationResolver.ScreenUpWorld(_cam)), AngleTolerance,
                "A settings change must not wait for the camera to move by camera_delta_deg.");
        }

        // ---------------- isolation ----------------

        [UnityTest]
        public IEnumerator ApplyOrientationSettings_UsesTheCallersCopy_AndLeavesEditedOriginalsAlone()
        {
            PoseCamera(0f, 30f);
            var ws = SpawnWall();
            var marker = ws.SpawnedMarkers[0];
            marker.transform.position = Vector3.zero;
            var mine = CopyOf(_config.orientation_settings);
            mine.vertical_alignment_mode = "screen_up";
            ws.ApplyOrientationSettings(mine, LevelsCopy(_config));
            Assert.AreSame(mine, marker.GetComponentInChildren<MarkerBillboard>().ConfiguredSettings,
                "The marker must be pointed at exactly the settings the caller passed.");
            Assert.AreSame(mine, ws.OrientationSettings);

            mine = null;
            ws.ApplyOrientationSettings(null, null);   // a null push is ignored, not a crash
            yield return null;
            Assert.IsNotNull(marker.GetComponentInChildren<MarkerBillboard>().ConfiguredSettings);
        }

        // ---------------- clusters (real LODController path) ----------------

        [UnityTest]
        public IEnumerator Clusters_OnScreenAggregateFollowsLiveVerticalAlignmentChange()
        {
            var lamp = new[] { "lamp", "lamp_religious", "lamp_military", "lamp_residential", "lamp_economic", "lamp_infrastructure" };
            var centroid = new Vector3(-0.95f, -0.87f, -4.18f);
            _cam.transform.position = centroid + new Vector3(0f, 0f, 20f);
            _cam.transform.rotation = Quaternion.LookRotation(centroid - _cam.transform.position, Quaternion.AngleAxis(30f, centroid - _cam.transform.position) * Vector3.up);
            _cam.fieldOfView = 60f;

            var anchorGO = new GameObject("PlacementCorrectionAnchor"); _tracked.Add(anchorGO);
            var wsGO = new GameObject("WallSessionHolder"); wsGO.SetActive(false); _tracked.Add(wsGO);
            var ws = wsGO.AddComponent<WallSession>();
            SetPrivate(ws, "_config", _config);
            SetPrivate(ws, "correctionAnchor", anchorGO.transform);

            ARZoomState.SetZoom(1f, 1f, 100f);
            var rig = new GameObject("LODControllerRig"); _tracked.Add(rig);
            var lod = rig.AddComponent<LODController>();
            // the wall's own settings object: LODController follows WallSession.LodSettings every frame
            _config.lod_settings = new LodSettings
            {
                enabled = true, density_response_mode = "cluster", density_radius_px = 80f, cluster_min_count = 2,
                shrink_start_neighbor_count = 2, density_safety_escalation_enabled = false, cluster_icon_mode = "pie_and_count",
                cluster_band_source = "centroid", cluster_band_hysteresis_enabled = true, cluster_dissolve_grace_cycles = 3,
                hysteresis_margin_m = 0.5f, transition_fade_duration_s = 0.3f,
            };
            SetPrivate(lod, "_settings", _config.lod_settings);
            SetPrivate(lod, "_clusterPrefab", AssetDatabase.LoadAssetAtPath<GameObject>(ClusterPrefabPath));
            SetPrivate(lod, "_camera", _cam);
            SetPrivate(lod, "_wallSession", ws);

            var markerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(MarkerPrefabPath);
            var markers = new List<MarkerView>();
            foreach (var id in lamp)
            {
                var poi = _config.pois.First(p => p.id == id);
                Assert.IsTrue(POIPositionResolver.TryResolvePosition(poi, out var pos));
                var root = (GameObject)PrefabUtility.InstantiatePrefab(markerPrefab);
                root.transform.position = pos;
                _tracked.Add(root);
                var anchor = root.GetComponent<POIAnchor>() ?? root.AddComponent<POIAnchor>();
                anchor.Initialise(poi);
                var view = root.GetComponent<MarkerView>();
                view.Initialise(anchor, MarkerVisualSettings.Default(), MarkerEffectFlags.None);
                markers.Add(view);
            }

            for (int cycle = 0; cycle < 2; cycle++)
            {
                var visible = lod.FrustumCull(markers);
                var distances = lod.ComputeEffectiveDistances(visible);
                var bands = lod.AssignBands(distances);
                var neighbors = lod.EvaluateDensity(visible);
                var units = lod.ApplyDensityResponse(visible, markers, bands, distances, neighbors);
                lod.ReconcileClusters(ref units);
                lod.ApplyCountCap(units, bands);
                lod.ApplyVisibility(units);
            }
            var views = (List<MarkerClusterView>)typeof(LODController).GetField("_activeClusterViews", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(lod);
            Assert.AreEqual(1, views.Count, "Precondition: one real aggregate formed.");
            var cluster = views[0].gameObject;

            yield return new WaitForSeconds(Settle);
            Vector3 screenUp = MarkerOrientationResolver.ScreenUpWorld(_cam);
            Assert.Greater(Angle(screenUp, Vector3.up), 25f, "Precondition: rolled camera.");
            Assert.Less(Angle(cluster.transform.up, Vector3.up), AngleTolerance, "Precondition: cluster inherits world_up.");

            var s = CopyOf(_config.orientation_settings);
            s.vertical_alignment_mode = "screen_up";
            ws.ApplyOrientationSettings(s, LevelsCopy(_config));
            lod.ReapplyClusterOrientation();
            yield return new WaitForSeconds(Settle);

            Assert.Less(Angle(cluster.transform.up, screenUp), AngleTolerance,
                "The on-screen cluster must follow the live vertical-alignment change (cluster 'inherit').");
            Assert.AreEqual("always_facing_camera",
                cluster.GetComponent<MarkerBillboard>().ConfiguredSettings.facing_mode,
                "Clusters always face the camera, whatever the wall's facing mode.");
        }

        private static void SetPrivate(object target, string name, object value)
        {
            var f = target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            Assert.IsNotNull(f, "field '" + name + "' not found on " + target.GetType().Name);
            f.SetValue(target, value);
        }
    }
}
