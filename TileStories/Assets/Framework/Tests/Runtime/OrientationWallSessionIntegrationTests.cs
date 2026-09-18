using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;
using TileStories;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TileStories.Tests
{
    // Block 4 composition test (_2.1_Marker_Orientation.md section 15): proves
    // WallSession.SpawnPOIs's new orientation wiring is actually reached from the real
    // runtime bootstrap path, not just callable in isolation (40-testing.md 4.2.1). Follows
    // this codebase's established pattern (see ARZoomRoutingTests.cs) of building
    // WallSession on an INACTIVE GameObject so Awake never fires, reflect-injecting _config,
    // then invoking the private SpawnPOIs() directly - this avoids racing the real
    // LoadConfigCoroutine/IWallTracker lifecycle, which every other integration test in this
    // codebase also avoids for the same reason.
    public class OrientationWallSessionIntegrationTests
    {
        private const string MarkerPrefabPath = "Assets/Framework/Runtime/UI/Markers/POI_Marker.prefab";

        private GameObject _wsGO;
        private GameObject _correctionAnchorGO;
        private GameObject _camGO;
        private GameObject _eventSystemGO;
        private WallConfigData _config;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            WallConfigData config = null;
            yield return WallConfigLoader.LoadFromStreamingAssets("LivingRoom/config.json", c => config = c);
            Assert.IsNotNull(config, "StreamingAssets/LivingRoom/config.json must load for this integration test.");
            _config = config;

            CategoryPalette.Configure(_config.category_styles);
            MarkerHierarchyResolver.Configure(_config.hierarchy_levels);

            foreach (var stray in GameObject.FindGameObjectsWithTag("MainCamera"))
                Object.DestroyImmediate(stray);
            _camGO = new GameObject("TestCamera");
            _camGO.tag = "MainCamera";
            var cam = _camGO.AddComponent<Camera>();
            cam.transform.position = new Vector3(0f, 0f, -3f);
            var target = new RenderTexture(1024, 768, 16);
            cam.targetTexture = target;

            _eventSystemGO = new GameObject("EventSystem");
            _eventSystemGO.AddComponent<EventSystem>();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            MarkerHierarchyResolver.ResetToDefaults();
            if (_wsGO != null) Object.DestroyImmediate(_wsGO);
            if (_correctionAnchorGO != null) Object.DestroyImmediate(_correctionAnchorGO);
            if (_camGO != null)
            {
                var cam = _camGO.GetComponent<Camera>();
                if (cam != null && cam.targetTexture != null)
                {
                    var tex = cam.targetTexture;
                    cam.targetTexture = null;
                    tex.Release();
                    Object.DestroyImmediate(tex);
                }
                Object.DestroyImmediate(_camGO);
            }
            if (_eventSystemGO != null) Object.DestroyImmediate(_eventSystemGO);
            yield return null;
        }

        private WallSession BuildAndSpawn(WallConfigData config)
        {
            _correctionAnchorGO = new GameObject("PlacementCorrectionAnchor");

            _wsGO = new GameObject("WallSessionHolder");
            _wsGO.SetActive(false);
            var ws = _wsGO.AddComponent<WallSession>();

#if UNITY_EDITOR
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(MarkerPrefabPath);
#else
            GameObject prefab = null;
#endif
            SetField(ws, "_config", config);
            SetField(ws, "poiAnchorPrefab", prefab);
            SetField(ws, "correctionAnchor", _correctionAnchorGO.transform);
            InvokePrivate(ws, "SpawnPOIs");
            return ws;
        }

        [UnityTest]
        public IEnumerator SpawnPOIs_EveryMarker_BillboardReceivesNonNullOrientationSettings()
        {
            var ws = BuildAndSpawn(_config);
            yield return null;

            Assert.Greater(ws.SpawnedMarkers.Count, 0, "LivingRoom config must spawn at least one marker.");
            foreach (var marker in ws.SpawnedMarkers)
            {
                var billboard = marker.GetComponent<MarkerBillboard>();
                Assert.IsNotNull(billboard, $"POI '{marker.name}' must carry a MarkerBillboard.");
                Assert.IsNotNull(billboard.ConfiguredSettings, $"POI '{marker.name}' billboard must have received OrientationSettings from SpawnPOIs.");
                Assert.AreSame(_config.orientation_settings, billboard.ConfiguredSettings,
                    "SpawnPOIs must pass the wall's actual orientation_settings instance, not a copy or default.");
            }
        }

        [UnityTest]
        public IEnumerator SpawnPOIs_WallFixed_UsesPoiAuthoredRotation()
        {
            // Real bug caught while cross-checking Block 4 against Block 7: SpawnPOIs used to
            // hard-set localRotation = Quaternion.identity before MarkerBillboard.Configure
            // captured it as the "authored" rotation, so wall_fixed markers could never
            // reflect editor_rotation_x/y/z_deg at runtime. Fixed in the same session.
            _config.orientation_settings.facing_mode = "wall_fixed";
            var poi = _config.pois.First();
            poi.editor_rotation_x_deg = 12f;
            poi.editor_rotation_deg = 34f;
            poi.editor_rotation_z_deg = 56f;

            var ws = BuildAndSpawn(_config);
            yield return null;

            var marker = ws.SpawnedMarkers.FirstOrDefault(m => m.name == poi.id);
            Assert.IsNotNull(marker, $"POI '{poi.id}' must have spawned.");

            var expected = Quaternion.Euler(poi.editor_rotation_x_deg, poi.editor_rotation_deg, poi.editor_rotation_z_deg);
            Assert.Less(Quaternion.Angle(marker.transform.rotation, expected), 0.1f,
                "wall_fixed must reproduce the POI's authored editor rotation at runtime.");
        }

        [UnityTest]
        public IEnumerator SpawnPOIs_HierarchyLevelOverride_BeatsWallDefault()
        {
            // Pick a real hierarchy level and give it an override programmatically - no
            // shipped wall config authors this field yet since it is brand new.
            var overriddenLevel = _config.hierarchy_levels.First();
            overriddenLevel.facing_mode_override = "wall_fixed";
            MarkerHierarchyResolver.Configure(_config.hierarchy_levels); // re-apply with the override

            var poiOnThatLevel = _config.pois.First(p => p.hierarchy_level_key == overriddenLevel.key);

            var ws = BuildAndSpawn(_config);
            yield return null;

            var marker = ws.SpawnedMarkers.FirstOrDefault(m => m.name == poiOnThatLevel.id);
            Assert.IsNotNull(marker, $"POI '{poiOnThatLevel.id}' must have spawned.");
            var billboard = marker.GetComponent<MarkerBillboard>();
            Assert.AreEqual("wall_fixed", billboard.ConfiguredModeOverride,
                "The POI's hierarchy level override must reach MarkerBillboard.Configure, not the wall's default mode.");
        }

        // Tier 0.5 - occlusion check on a real spawned marker (section 15 Block 4).
        [UnityTest]
        public IEnumerator SpawnPOIs_RealMarker_SymbolIsTopmostRaycastTarget()
        {
            var ws = BuildAndSpawn(_config);
            Assert.Greater(ws.SpawnedMarkers.Count, 0);
            var marker = ws.SpawnedMarkers[0];

            // Skip past the reveal fade/scale-in so tappability doesn't depend on frame
            // timing (same fix as OrientationGalleryHarness.SpawnEntry / DisplacementGalleryTests).
            var reveal = marker.GetComponent<MarkerRevealEffect>();
            if (reveal != null) { reveal.StopAllCoroutines(); reveal.SetFullAlphaAndScale(); }

            var cam = _camGO.GetComponent<Camera>();
            marker.transform.position = new Vector3(0f, 0f, 0f);
            cam.transform.position = new Vector3(0f, 0f, -0.4f);
            cam.transform.LookAt(marker.transform.position);
            yield return null;

            var symbolRect = marker.transform.Find("Symbol") as RectTransform;
            Assert.IsNotNull(symbolRect);
            Assert.IsTrue(IsTopmostRaycastTarget(symbolRect, cam),
                "A real WallSession-spawned marker's Symbol should be the topmost raycast target at its own centre.");
        }

        private static bool IsTopmostRaycastTarget(RectTransform target, Camera uiCamera)
        {
            var raycaster = target.GetComponentInParent<GraphicRaycaster>();
            if (raycaster == null || EventSystem.current == null) return false;

            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, target.position);
            var pointerData = new PointerEventData(EventSystem.current) { position = screenPoint };
            var results = new List<RaycastResult>();
            raycaster.Raycast(pointerData, results);

            return results.Count > 0 && results[0].gameObject.transform == target;
        }

        private static void SetField(object obj, string name, object value) =>
            obj.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)!
                .SetValue(obj, value);

        private static void InvokePrivate(object obj, string method) =>
            obj.GetType().GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance)!
                .Invoke(obj, null);
    }
}
