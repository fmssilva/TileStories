using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TestTools;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TileStories.Tests
{
    // Marker design domain (_2.2.1/2/3), end to end: a real WallSession spawning the real shipped
    // LivingRoom config through the real POI_Marker prefab, asserting the edge cases the "normal"
    // POIs never exercise -- no status axis, no badge category, no hierarchy level, a custom symbol.
    // Companion to OrientationWallSessionIntegrationTests (same build pattern) and MarkerGalleryTests
    // (fabricated data instead of the real config).
    public class MarkerConfigDrivesMarkerTests
    {
        private const string MarkerPrefabPath = "Assets/Framework/Runtime/UI/Markers/POI_Marker.prefab";

        private GameObject _wsGO;
        private GameObject _correctionAnchorGO;
        private WallConfigData _config;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            WallConfigData config = null;
            yield return WallConfigLoader.LoadFromStreamingAssets("LivingRoom/config.json", c => config = c);
            Assert.IsNotNull(config, "StreamingAssets/LivingRoom/config.json must load for this test.");
            _config = config;
            _config.effect_defaults.preview = new EffectDefaults.EffectPreviewSettings();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            MarkerHierarchyResolver.ResetToDefaults();
            StatusRamp.ResetToDefaults();
            if (_wsGO != null) Object.DestroyImmediate(_wsGO);
            if (_correctionAnchorGO != null) Object.DestroyImmediate(_correctionAnchorGO);
            yield return null;
        }

        private WallSession BuildAndSpawn()
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
            SetField(ws, "_config", _config);
            SetField(ws, "poiAnchorPrefab", prefab);
            SetField(ws, "correctionAnchor", _correctionAnchorGO.transform);
            InvokePrivate(ws, "SpawnPOIs");
            return ws;
        }

        private static void SetField(object obj, string name, object value) =>
            obj.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(obj, value);

        private static void InvokePrivate(object obj, string method) =>
            obj.GetType().GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(obj, null);

        [UnityTest]
        public IEnumerator NoStatusPoi_HasNoRingAndNoBadge()
        {
            var ws = BuildAndSpawn();
            yield return null;
            var marker = ws.SpawnedMarkers.First(m => m.name == "dev_marker_nostatus");

            Assert.IsFalse(marker.transform.Find("Ring").GetComponent<Image>().enabled,
                "has_status=false must render no ring.");
            Assert.IsFalse(marker.transform.Find("Badge").gameObject.activeSelf,
                "has_status=false must render no badge.");
        }

        [UnityTest]
        public IEnumerator NoBadgeCategoryPoi_KnownStatus_FallsBackToStatusColouredBadge()
        {
            var ws = BuildAndSpawn();
            yield return null;
            var marker = ws.SpawnedMarkers.First(m => m.name == "dev_marker_nobadge");

            Assert.IsTrue(marker.transform.Find("Ring").GetComponent<Image>().enabled,
                "has_status=true must render a ring.");
            var badge = marker.transform.Find("Badge").gameObject;
            Assert.IsTrue(badge.activeSelf,
                "marker_use_badge is true wall-wide, so an empty badge_category still falls back to a status-coloured badge, never a hidden one.");
            var badgeColor = badge.GetComponent<Image>().color;
            Assert.AreEqual(StatusRamp.Resolve(40f).RingColor, badgeColor,
                "The fallback badge must use the resolved status colour, not a badge-category colour.");
        }

        [UnityTest]
        public IEnumerator NoHierarchyLevelPoi_UsesFrameworkFallbackSize()
        {
            var ws = BuildAndSpawn();
            yield return null;
            var marker = ws.SpawnedMarkers.First(m => m.name == "dev_marker_nolevel");
            var symbol = marker.transform.Find("Symbol").GetComponent<RectTransform>();

            Assert.AreEqual(MarkerHierarchyResolver.Fallback.SizeCm / 100f, symbol.sizeDelta.x, 0.001f,
                "An empty hierarchy_level_key must resolve to MarkerHierarchyResolver.Fallback's size (12cm), not 0 or a stale value.");
            Assert.IsFalse(marker.transform.Find("Label").gameObject.activeSelf,
                "The framework fallback shows no persistent label.");
        }

        [UnityTest]
        public IEnumerator CustomSymbolPoi_OverridesIconOnly_CategoryColourUnaffected()
        {
            var ws = BuildAndSpawn();
            yield return null;
            var marker = ws.SpawnedMarkers.First(m => m.name == "dev_marker_custom_symbol");
            var symbolIcon = marker.transform.Find("Symbol/Icon").GetComponent<Image>();
            var symbolBackground = marker.transform.Find("Symbol").GetComponent<Image>();

            Assert.AreEqual("IconInfrastructures", symbolIcon.sprite.name,
                "custom_symbol_key must override the icon, ignoring the POI's own category icon.");

            // dev_marker_custom_symbol is authored with category "military" -- a real LivingRoom
            // category with its own configured colour, unrelated to the custom icon override.
            var militaryEntry = _config.category_styles.First(e => e.category == "military");
            Color expectedFill;
            ColorUtility.TryParseHtmlString(militaryEntry.color_hex, out expectedFill);
            Assert.AreEqual(expectedFill, symbolBackground.color,
                "The custom symbol override must replace only the icon; the category fill colour is unaffected.");
        }
    }
}
