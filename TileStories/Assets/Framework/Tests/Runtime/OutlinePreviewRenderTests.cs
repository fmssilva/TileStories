using System.Collections;
using System.Collections.Generic;
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
    // "Add outline demo grid" (P5), real WallSession + real POI_Marker prefab + the real shipped
    // LivingRoom config, mirroring MarkerConfigDrivesMarkerTests' build pattern (BuildAndSpawn). Reads
    // real component state (Image.color, transform rotation) instead of pixel diffing -- the grid's
    // camera-fit math is already proven by EffectsPreviewRenderTests/EffectsPreviewSpawnerTests via the
    // shared DevPreviewGridLayout, so this file only proves the outline-specific content is correct:
    // one control cell, one real cell per outline level plus "Unknown", correct colour per outline
    // mode, and that only the level cells spin.
    public class OutlinePreviewRenderTests
    {
        private const string MarkerPrefabPath = "Assets/Framework/Runtime/UI/Markers/POI_Marker.prefab";

        private GameObject _wsGO;
        private GameObject _correctionAnchorGO;
        private GameObject _camGO;
        private WallConfigData _config;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            WallConfigData config = null;
            yield return WallConfigLoader.LoadFromStreamingAssets("LivingRoom/config.json", c => config = c);
            Assert.IsNotNull(config, "StreamingAssets/LivingRoom/config.json must load for this test.");
            _config = config;
            _config.effect_defaults.preview = new EffectDefaults.EffectPreviewSettings();   // keep the effects grid off
            _config.outline_preview = new OutlinePreviewSettings { enabled = false, base_poi_id = "" };

            // OutlinePreviewSpawner.TrySpawn needs Camera.main (it copies its field of view and
            // rotation): a PlayMode test scene has none by default, unlike EffectsPreviewRenderTests
            // which builds its own render-target camera for pixel work this file does not need.
            foreach (var stray in GameObject.FindGameObjectsWithTag("MainCamera"))
                Object.DestroyImmediate(stray);
            _camGO = new GameObject("OutlineTestCamera") { tag = "MainCamera" };
            var cam = _camGO.AddComponent<Camera>();
            cam.fieldOfView = 60f;
            cam.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            MarkerHierarchyResolver.ResetToDefaults();
            StatusRamp.ResetToDefaults();
            if (_wsGO != null) Object.DestroyImmediate(_wsGO);
            if (_correctionAnchorGO != null) Object.DestroyImmediate(_correctionAnchorGO);
            if (_camGO != null) Object.DestroyImmediate(_camGO);
            var stray = GameObject.Find("OutlinePreview");
            if (stray != null) Object.DestroyImmediate(stray);
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

        private static List<GameObject> CellsOf(GameObject root) =>
            Enumerable.Range(0, root.transform.childCount).Select(i => root.transform.GetChild(i).gameObject)
                .Where(c => c.name.StartsWith("Preview_")).ToList();

        [UnityTest]
        public IEnumerator Disabled_ByDefault_NothingSpawns()
        {
            var ws = BuildAndSpawn();
            yield return null;
            Assert.IsNull(ws.OutlinePreviewRoot, "outline_preview.enabled defaults to false -- nothing must spawn.");
            Assert.IsNull(GameObject.Find("OutlinePreview"));
        }

        [UnityTest]
        public IEnumerator Enabled_SpawnsOneControlCell_PlusOneCellPerConfiguredLevel()
        {
            _config.outline_preview.enabled = true;
            var ws = BuildAndSpawn();
            yield return null;

            Assert.IsNotNull(ws.OutlinePreviewRoot, "The grid must spawn when the switch is on.");
            var cells = CellsOf(ws.OutlinePreviewRoot);
            // The shipped LivingRoom config already authors its own "unknown" row (regression: the
            // spawner used to always append a synthetic one too, duplicating it in the grid).
            Assert.IsTrue(_config.outline_levels.Exists(l => l.key == "unknown"),
                "Precondition: the shipped config authors its own 'unknown' level.");
            int expected = 1 /* control */ + _config.outline_levels.Count /* real levels, unknown included */;
            Assert.AreEqual(expected, cells.Count);
            Assert.AreEqual("Preview_No outline", cells[0].name);
            Assert.AreEqual("Preview_Unknown", cells[cells.Count - 1].name);
        }

        [UnityTest]
        public IEnumerator NoOwnUnknownLevel_GetsTheSyntheticUnknownCellInstead()
        {
            _config.outline_levels.RemoveAll(l => l.key == "unknown");
            _config.outline_preview.enabled = true;
            var ws = BuildAndSpawn();
            yield return null;

            var cells = CellsOf(ws.OutlinePreviewRoot);
            int expected = 1 /* control */ + _config.outline_levels.Count + 1 /* synthetic unknown */;
            Assert.AreEqual(expected, cells.Count);
            Assert.AreEqual("Preview_Unknown", cells[cells.Count - 1].name);
        }

        [UnityTest]
        public IEnumerator ControlCell_HasNoRing_LevelCells_DoAndShareTheUniformColour()
        {
            _config.marker_outline_mode = "uniform";
            _config.outline_uniform_color_hex = "#3366CC";
            _config.outline_preview.enabled = true;
            var ws = BuildAndSpawn();
            yield return null;

            var cells = CellsOf(ws.OutlinePreviewRoot);
            var controlRing = cells[0].transform.Find("Ring").GetComponent<Image>();
            Assert.IsFalse(controlRing.enabled, "'No outline' must show no ring.");

            ColorUtility.TryParseHtmlString("#3366CC", out var expected);
            for (int i = 1; i < cells.Count; i++)
            {
                var ring = cells[i].transform.Find("Ring").GetComponent<Image>();
                Assert.IsTrue(ring.enabled, $"{cells[i].name} has a status -- it must show a ring.");
                Assert.AreEqual(expected, ring.color, $"{cells[i].name}: Uniform mode must give every ring the same configured colour.");
            }
        }

        [UnityTest]
        public IEnumerator PerTypeMode_EachLevelCellShowsItsOwnConfiguredColour()
        {
            _config.marker_outline_mode = "per_type";
            var first = _config.outline_levels[0];
            var second = _config.outline_levels[1];
            first.color_hex = "#FF0000";
            second.color_hex = "#00FF00";
            _config.outline_preview.enabled = true;
            var ws = BuildAndSpawn();
            yield return null;

            var cells = CellsOf(ws.OutlinePreviewRoot);
            var firstCell = cells.First(c => c.name == "Preview_" + (string.IsNullOrWhiteSpace(first.label) ? first.key : first.label));
            var secondCell = cells.First(c => c.name == "Preview_" + (string.IsNullOrWhiteSpace(second.label) ? second.key : second.label));

            ColorUtility.TryParseHtmlString("#FF0000", out var expectedFirst);
            ColorUtility.TryParseHtmlString("#00FF00", out var expectedSecond);
            Assert.AreEqual(expectedFirst, firstCell.transform.Find("Ring").GetComponent<Image>().color);
            Assert.AreEqual(expectedSecond, secondCell.transform.Find("Ring").GetComponent<Image>().color);
        }

        [UnityTest]
        public IEnumerator LevelCellsSpin_ControlCellDoesNot()
        {
            _config.contour_spin_deg_per_s = 180f;
            _config.outline_preview.enabled = true;
            var ws = BuildAndSpawn();
            yield return null;

            var cells = CellsOf(ws.OutlinePreviewRoot);
            var controlRing = cells[0].transform.Find("Ring");
            var levelRing = cells[1].transform.Find("Ring");
            float controlBefore = controlRing.localRotation.eulerAngles.z;
            float levelBefore = levelRing.localRotation.eulerAngles.z;

            yield return new WaitForSeconds(0.5f);

            float controlAfter = controlRing.localRotation.eulerAngles.z;
            float levelAfter = levelRing.localRotation.eulerAngles.z;
            Assert.AreEqual(controlBefore, controlAfter, 0.01f, "The control cell has no ring, so nothing should rotate.");
            Assert.AreNotEqual(levelBefore, levelAfter, "A level cell with contour spin > 0 must visibly rotate over time.");
        }

        [UnityTest]
        public IEnumerator ApplyMarkerSettings_RebuildsTheGridLive_WhenOutlineLevelsChange()
        {
            _config.outline_preview.enabled = true;
            var ws = BuildAndSpawn();
            yield return null;
            var before = ws.OutlinePreviewRoot;
            Assert.IsNotNull(before);
            int countBefore = CellsOf(before).Count;

            var edited = JsonUtility.FromJson<WallConfigData>(JsonUtility.ToJson(_config));
            edited.outline_levels.Add(new OutlineLevelEntry { key = "extra_level", label = "Extra", pct = 50f, line_style = "solid", color_hex = "" });
            ws.ApplyMarkerSettings(edited);
            yield return null;

            Assert.AreNotSame(before, ws.OutlinePreviewRoot, "The old grid must be destroyed and a fresh one spawned.");
            Assert.AreEqual(countBefore + 1, CellsOf(ws.OutlinePreviewRoot).Count, "The rebuilt grid must reflect the new outline level.");
        }
    }
}
