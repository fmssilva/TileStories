using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TileStories.Tests
{
    // The effects preview grid (effect_defaults.preview), proven through the REAL runtime bootstrap
    // path: WallSession.SpawnPOIs on the shipped LivingRoom config, real POI_Marker prefab, real
    // MarkerView.Initialise, a real camera. Follows the established pattern of
    // OrientationWallSessionIntegrationTests (WallSession on an inactive GameObject, private fields
    // injected by reflection, SpawnPOIs invoked directly) -- 40-testing.md 4.2.1: the seam between the
    // config switch and the spawner is what these tests exist to prove.
    public class EffectsPreviewSpawnerTests
    {
        private const string MarkerPrefabPath = "Assets/Framework/Runtime/UI/Markers/POI_Marker.prefab";

        private GameObject _wsGO;
        private GameObject _anchorGO;
        private GameObject _camGO;
        private Camera _cam;
        private WallConfigData _config;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            WallConfigData config = null;
            yield return WallConfigLoader.LoadFromStreamingAssets("LivingRoom/config.json", c => config = c);
            Assert.IsNotNull(config, "StreamingAssets/LivingRoom/config.json must load.");
            _config = config;

            // Start from the framework defaults (off, plain circle): the developer's own saved preview
            // settings in the shipped config must never change what these tests prove.
            _config.effect_defaults.preview = new EffectDefaults.EffectPreviewSettings();

            CategoryPalette.Configure(_config.category_styles);
            MarkerHierarchyResolver.Configure(_config.hierarchy_levels);

            foreach (var stray in GameObject.FindGameObjectsWithTag("MainCamera"))
                Object.DestroyImmediate(stray);
            _camGO = new GameObject("TestCamera") { tag = "MainCamera" };
            _cam = _camGO.AddComponent<Camera>();
            _cam.transform.position = new Vector3(0f, 0f, -3f);
            _cam.targetTexture = new RenderTexture(1024, 768, 16);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            MarkerHierarchyResolver.ResetToDefaults();
            foreach (var go in new[] { _wsGO, _anchorGO, GameObject.Find("EffectsPreview") })
                if (go != null) Object.DestroyImmediate(go);
            if (_camGO != null)
            {
                var tex = _cam.targetTexture;
                _cam.targetTexture = null;
                tex.Release();
                Object.DestroyImmediate(tex);
                Object.DestroyImmediate(_camGO);
            }
            yield return null;
        }

        private WallSession Spawn()
        {
            _anchorGO = new GameObject("PlacementCorrectionAnchor");
            _wsGO = new GameObject("WallSessionHolder");
            _wsGO.SetActive(false);
            var ws = _wsGO.AddComponent<WallSession>();
#if UNITY_EDITOR
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(MarkerPrefabPath);
#else
            GameObject prefab = null;
#endif
            SetField(ws, "_config", _config);
            SetField(ws, "_effectDefaults", _config.effect_defaults);
            SetField(ws, "poiAnchorPrefab", prefab);
            SetField(ws, "correctionAnchor", _anchorGO.transform);
            ws.GetType().GetMethod("SpawnPOIs", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(ws, null);
            return ws;
        }

        private static void SetField(object obj, string name, object value) =>
            obj.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(obj, value);

        private static string LabelTextOf(GameObject cell)
        {
            var view = cell.GetComponentInChildren<MarkerView>();
            var label = view.LabelRect.GetComponent("TextMeshProUGUI");
            Assert.IsNotNull(label, "The marker's label component must exist.");
            return (string)label.GetType().GetProperty("text").GetValue(label);
        }

        private static List<GameObject> Cells(GameObject root) =>
            Enumerable.Range(0, root.transform.childCount).Select(i => root.transform.GetChild(i).gameObject)
                .Where(c => c.name.StartsWith("Preview_")).ToList();   // the grid camera is a child too

        // Independent oracle for the option strings.
        private static void AssertCellRunsExactly(GameObject cell, string ripple, string halo, bool pulse, string context)
        {
            var rippleFx = cell.GetComponent<MarkerRippleEffect>();
            var haloFx = cell.GetComponent<MarkerHaloEffect>();
            Assert.AreEqual(pulse, cell.GetComponent<MarkerPulseEffect>().IsActive, context + ": pulse");
            Assert.AreEqual(ripple != "none", rippleFx.IsActive, context + ": ripple active");
            if (ripple == "ripple_rings") Assert.AreEqual(MarkerRippleEffect.RippleStyle.Rings, rippleFx.CurrentStyle, context);
            if (ripple == "ripple_discs") Assert.AreEqual(MarkerRippleEffect.RippleStyle.Discs, rippleFx.CurrentStyle, context);
            Assert.AreEqual(halo != "none", haloFx.IsActive, context + ": halo active");
            if (halo == "halo_ring") Assert.AreEqual(MarkerHaloEffect.HaloVariant.Ring, haloFx.CurrentVariant, context);
            if (halo == "halo_disc") Assert.AreEqual(MarkerHaloEffect.HaloVariant.Disc, haloFx.CurrentVariant, context);
            if (halo == "beacon") Assert.AreEqual(MarkerHaloEffect.HaloVariant.Beacon, haloFx.CurrentVariant, context);
        }

        // ---------------- off by default ----------------

        [UnityTest]
        public IEnumerator PreviewOff_SpawnsNothing_AndTheWallIsUntouched()
        {
            Assert.IsFalse(_config.effect_defaults.preview.enabled, "Precondition: the preview is off (framework default).");
            var ws = Spawn();
            yield return null;

            Assert.IsNull(ws.EffectsPreviewRoot);
            Assert.IsNull(GameObject.Find("EffectsPreview"));
            Assert.AreEqual(_config.pois.Count, ws.SpawnedMarkers.Count, "Every real POI still spawned.");
        }

        // ---------------- the grid ----------------

        [UnityTest]
        public IEnumerator PreviewOn_SpawnsLabelledGrid_OneCellPerEffectAndLevel_AllInFrontOfTheCamera()
        {
            _config.effect_defaults.preview.enabled = true;
            var ws = Spawn();
            yield return null;

            var root = ws.EffectsPreviewRoot;
            Assert.IsNotNull(root, "The preview grid must be reached from WallSession.SpawnPOIs.");
            var cells = Cells(root);
            int levels = _config.hierarchy_levels.Count;
            Assert.Greater(levels, 0, "Precondition: the wall authors levels.");
            Assert.AreEqual(7 + levels, cells.Count, "No effect + 6 effects + one cell per level");
            Assert.AreEqual(_config.pois.Count, ws.SpawnedMarkers.Count, "Preview cells are not counted as wall POIs.");

            // Effect row.
            Assert.AreEqual("No effect", LabelTextOf(cells[0]));
            AssertCellRunsExactly(cells[0], "none", "none", false, "No effect");
            var expectedEffects = new (string label, string ripple, string halo, bool pulse)[]
            {
                ("Pulse", "none", "none", true),
                ("Ripple Rings", "ripple_rings", "none", false),
                ("Ripple Discs", "ripple_discs", "none", false),
                ("Halo Ring", "none", "halo_ring", false),
                ("Halo Disc", "none", "halo_disc", false),
                ("Beacon", "none", "beacon", false),
            };
            for (int i = 0; i < expectedEffects.Length; i++)
            {
                var e = expectedEffects[i];
                Assert.AreEqual(e.label, LabelTextOf(cells[i + 1]), "cell label");
                AssertCellRunsExactly(cells[i + 1], e.ripple, e.halo, e.pulse, e.label);
            }

            // Level row: real effects and real size of each level.
            for (int i = 0; i < levels; i++)
            {
                var level = _config.hierarchy_levels[i];
                var cell = cells[7 + i];
                Assert.AreEqual(level.key, LabelTextOf(cell), "level cell label");
                AssertCellRunsExactly(cell, level.ripple_effect, level.halo_effect, level.pulse, "level " + level.key);
                var symbol = (RectTransform)cell.transform.Find("Symbol");
                Assert.AreEqual(level.size_cm / 100f, symbol.sizeDelta.x, 1e-4f, level.key + " uses its real size");
            }

            // Everything is in front of the GRID camera and inside its view, cells do not overlap.
            var viewCam = root.GetComponent<EffectsPreviewFocus>().ViewCamera;
            Assert.IsNotNull(viewCam, "The grid owns its own camera.");
            var seen = new HashSet<Vector3>();
            foreach (var cell in cells)
            {
                Vector3 vp = viewCam.WorldToViewportPoint(cell.transform.position);
                Assert.Greater(vp.z, 0f, cell.name + " is in front of the grid camera");
                Assert.IsTrue(vp.x > 0f && vp.x < 1f && vp.y > 0f && vp.y < 1f, $"{cell.name} inside the view, was {vp}");
                Assert.IsTrue(seen.Add(new Vector3(Mathf.Round(cell.transform.position.x * 100f), Mathf.Round(cell.transform.position.y * 100f), Mathf.Round(cell.transform.position.z * 100f))), cell.name + " overlaps another cell");
            }
        }

        [UnityTest]
        public IEnumerator PreviewCells_UseThePlainGreyCircle_WhenNoBasePoiIsChosen()
        {
            _config.effect_defaults.preview.enabled = true;
            var ws = Spawn();
            yield return null;

            var cell = Cells(ws.EffectsPreviewRoot)[1];
            var symbol = cell.transform.Find("Symbol").GetComponent<Image>();
            Assert.AreEqual(0.55f, symbol.color.r, 0.01f, "grey");
            Assert.AreEqual(symbol.color.r, symbol.color.g, 0.001f, "neutral: r == g");
            Assert.AreEqual(symbol.color.r, symbol.color.b, 0.001f, "neutral: r == b");
            Assert.IsTrue(symbol.enabled, "the circle is drawn");
            Assert.IsFalse(cell.transform.Find("Symbol/Icon").GetComponent<Image>().enabled, "no icon on the plain circle");
        }

        [UnityTest]
        public IEnumerator PreviewCells_CopyTheChosenRealPoi_CategoryStatusAndSize()
        {
            var basePoi = _config.pois.First(p => p.id == "lamp_military");
            Assert.IsTrue(basePoi.has_status, "Precondition: the chosen POI carries a status.");
            _config.effect_defaults.preview.enabled = true;
            _config.effect_defaults.preview.base_poi_id = basePoi.id;
            var ws = Spawn();
            yield return null;

            var level = _config.hierarchy_levels.First(l => l.key == basePoi.hierarchy_level_key);
            var cells = Cells(ws.EffectsPreviewRoot);
            foreach (var cell in cells.Take(7))
            {
                var data = cell.GetComponent<POIAnchor>().Data;
                Assert.AreEqual(basePoi.category, data.category, cell.name + ": category copied");
                Assert.AreEqual(basePoi.status_level_key, data.status_level_key, cell.name + ": status copied");
                Assert.AreNotEqual(basePoi.id, data.id, cell.name + ": must not reuse the real POI's id");
                Assert.IsNull(data.hierarchy_level_key, cell.name + ": effects come from the cell, not the copied level");
            }
            var symbol = (RectTransform)cells[1].transform.Find("Symbol");
            Assert.AreEqual(level.size_cm / 100f, symbol.sizeDelta.x, 1e-4f, "effect cells take the size of the chosen POI's level");
            var color = cells[1].transform.Find("Symbol").GetComponent<Image>().color;
            Assert.IsFalse(Mathf.Abs(color.r - 0.55f) < 0.01f && Mathf.Abs(color.g - 0.55f) < 0.01f && Mathf.Abs(color.b - 0.55f) < 0.01f,
                "a real POI keeps its category colour, it is not painted plain grey");
        }

        // ---------------- switches are honoured and explained ----------------

        [UnityTest]
        public IEnumerator PreviewCells_RespectTheSwitches_AndSayWhenAnEffectIsOff()
        {
            _config.effect_defaults.preview.enabled = true;
            _config.effect_defaults.halo_ring.enabled = false;
            var ws = Spawn();
            yield return null;

            var cells = Cells(ws.EffectsPreviewRoot);
            var haloRing = cells.First(c => c.name == "Preview_Halo Ring (off)");
            AssertCellRunsExactly(haloRing, "none", "none", false, "Halo Ring switched off");
            var haloDisc = cells.First(c => c.name == "Preview_Halo Disc");
            AssertCellRunsExactly(haloDisc, "none", "halo_disc", false, "Halo Disc still on");
            Assert.AreEqual("Halo Ring (off)", LabelTextOf(haloRing));
        }

        [UnityTest]
        public IEnumerator PreviewCells_AreAllStill_WhenTheMasterSwitchIsOff()
        {
            _config.effect_defaults.preview.enabled = true;
            _config.effect_defaults.effects_enabled = false;
            var ws = Spawn();
            yield return null;

            foreach (var cell in Cells(ws.EffectsPreviewRoot))
                AssertCellRunsExactly(cell, "none", "none", false, cell.name);
            Assert.IsTrue(Cells(ws.EffectsPreviewRoot).Skip(1).Take(6).All(c => c.name.EndsWith("(off)")), "effect cells are labelled (off)");
        }

        // ---------------- live update of a running wall (Live Play Mode Config) ----------------

        private static EffectDefaults CopyOf(EffectDefaults d) =>
            JsonUtility.FromJson<EffectDefaults>(JsonUtility.ToJson(d));

        [UnityTest]
        public IEnumerator ApplyEffectSettings_TurnsThePreviewGridOnAndOffOnARunningWall()
        {
            var ws = Spawn();
            yield return null;
            Assert.IsNull(ws.EffectsPreviewRoot, "Precondition: grid off.");

            var on = CopyOf(_config.effect_defaults);
            on.preview.enabled = true;
            ws.ApplyEffectSettings(on, _config.hierarchy_levels);
            yield return null;
            var firstRoot = ws.EffectsPreviewRoot;
            Assert.IsNotNull(firstRoot, "Turning the switch on must build the grid without a restart.");
            Assert.AreEqual(7 + _config.hierarchy_levels.Count, Cells(firstRoot).Count);

            // A second apply rebuilds: the old grid (and its camera) is gone, exactly one grid exists.
            ws.ApplyEffectSettings(CopyOf(on), _config.hierarchy_levels);
            yield return null;
            Assert.IsTrue(firstRoot == null, "The old grid must be destroyed.");
            Assert.IsNotNull(ws.EffectsPreviewRoot);
            Assert.AreEqual(1, Object.FindObjectsByType<EffectsPreviewFocus>(FindObjectsSortMode.None).Length,
                "Exactly one grid (and grid camera) may exist after a rebuild.");

            var off = CopyOf(on);
            off.preview.enabled = false;
            ws.ApplyEffectSettings(off, _config.hierarchy_levels);
            yield return null;
            Assert.IsNull(ws.EffectsPreviewRoot);
            Assert.AreEqual(0, Object.FindObjectsByType<EffectsPreviewFocus>(FindObjectsSortMode.None).Length);
        }

        [UnityTest]
        public IEnumerator ApplyEffectSettings_ReappliesEffectsOnEverySpawnedRealMarker()
        {
            var level = _config.hierarchy_levels[0];
            level.pulse = true;
            level.ripple_effect = "ripple_rings";
            level.halo_effect = "beacon";
            MarkerHierarchyResolver.Configure(_config.hierarchy_levels);
            var ws = Spawn();
            yield return null;

            var marker = ws.SpawnedMarkers.First(m => _config.pois.First(p => p.id == m.name).hierarchy_level_key == level.key);
            Assert.IsTrue(marker.GetComponent<MarkerPulseEffect>().IsActive, "Precondition: pulse runs.");
            Assert.AreEqual(MarkerRippleEffect.RippleStyle.Rings, marker.GetComponent<MarkerRippleEffect>().CurrentStyle);

            var levels = JsonUtility.FromJson<LevelsHolder>(JsonUtility.ToJson(new LevelsHolder { levels = _config.hierarchy_levels })).levels;
            levels[0].ripple_effect = "ripple_discs";
            levels[0].halo_effect = "none";
            var defaults = CopyOf(_config.effect_defaults);
            defaults.pulse.enabled = false;
            ws.ApplyEffectSettings(defaults, levels);
            yield return null;

            Assert.IsFalse(marker.GetComponent<MarkerPulseEffect>().IsActive, "Per-effect switch off must stop the pulse live.");
            Assert.AreEqual(MarkerRippleEffect.RippleStyle.Discs, marker.GetComponent<MarkerRippleEffect>().CurrentStyle,
                "The level's new ripple choice must reach the running marker.");
            Assert.IsFalse(marker.GetComponent<MarkerHaloEffect>().IsActive, "Halo none must switch the halo off.");
        }

        [System.Serializable]
        private class LevelsHolder { public List<HierarchyLevelEntry> levels; }
    }
}
