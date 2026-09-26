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
            // Quick row (3: No effect, Pulse, Spin Ring) + combo row (6: 2 ripples, 1 spacer, 3 halos) + one cell per level.
            Assert.AreEqual(9 + levels, cells.Count, "quick row + combo row (incl. spacer) + one cell per level");
            Assert.AreEqual(_config.pois.Count, ws.SpawnedMarkers.Count, "Preview cells are not counted as wall POIs.");

            // Quick row: No effect, Pulse, Spin Ring.
            Assert.AreEqual("No effect", LabelTextOf(cells[0]));
            AssertCellRunsExactly(cells[0], "none", "none", false, "No effect");
            Assert.AreEqual("Pulse", LabelTextOf(cells[1]));
            AssertCellRunsExactly(cells[1], "none", "none", true, "Pulse");
            Assert.AreEqual("Spin Ring", LabelTextOf(cells[2]));
            AssertCellRunsExactly(cells[2], "none", "none", false, "Spin Ring");

            // Combo row: Ripple Rings, Ripple Discs, spacer (index 5, skipped), Halo Ring, Halo Disc, Beacon.
            var expectedCombo = new (int index, string label, string ripple, string halo)[]
            {
                (3, "Ripple Rings", "ripple_rings", "none"),
                (4, "Ripple Discs", "ripple_discs", "none"),
                (6, "Halo Ring", "none", "halo_ring"),
                (7, "Halo Disc", "none", "halo_disc"),
                (8, "Beacon", "none", "beacon"),
            };
            foreach (var e in expectedCombo)
            {
                Assert.AreEqual(e.label, LabelTextOf(cells[e.index]), "cell label");
                AssertCellRunsExactly(cells[e.index], e.ripple, e.halo, false, e.label);
            }
            Assert.AreEqual("Preview_(spacer)", cells[5].name, "the gap between Ripple and Halo is a real, positioned, invisible cell");
            Assert.IsNull(cells[5].GetComponent<POIAnchor>(), "the spacer spawns no marker");

            // Level row: the base marker's name (plain grey circle here, never the level name), real
            // effects and real size of each level.
            for (int i = 0; i < levels; i++)
            {
                var level = _config.hierarchy_levels[i];
                var cell = cells[9 + i];
                Assert.AreEqual(EffectsPreviewSpawner.PlainCircleName, LabelTextOf(cell), "level cell label is the marker's name");
                if (!string.IsNullOrWhiteSpace(level.level_name))
                    Assert.AreNotEqual(level.level_name, LabelTextOf(cell), "a hierarchy level name must never be shown under a marker");
                AssertCellRunsExactly(cell, level.ripple_effect, level.halo_effect, level.pulse, "level " + level.key);
                var symbol = (RectTransform)cell.transform.Find("Symbol");
                Assert.AreEqual(level.size_cm / 100f, symbol.sizeDelta.x, 1e-4f, level.key + " uses its real size");
            }

            // Everything is in front of the GRID camera and inside its view, cells do not overlap
            // (the spacer has a real position too, but no visible content).
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
            // Quick row (3) + combo row (6, one of them a spacer with no POIAnchor).
            foreach (var cell in cells.Take(9).Where(c => c.GetComponent<POIAnchor>() != null))
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

            // The spacer cell (combo row) has no marker components at all -- skip it here.
            foreach (var cell in Cells(ws.EffectsPreviewRoot).Where(c => c.GetComponent<POIAnchor>() != null))
                AssertCellRunsExactly(cell, "none", "none", false, cell.name);

            // Pulse (quick row) plus every combo-row effect are labelled "(off)"; Spin Ring is not an
            // effect switch and the No effect / level cells are not gated by effects_enabled by name.
            var gatedEffectCells = Cells(ws.EffectsPreviewRoot)
                .Where(c => c.name.Contains("Pulse") || c.name.Contains("Ripple") || c.name.Contains("Halo") || c.name.Contains("Beacon"))
                .ToList();
            Assert.AreEqual(6, gatedEffectCells.Count, "Pulse + 2 Ripple + 2 Halo + Beacon");
            Assert.IsTrue(gatedEffectCells.All(c => c.name.EndsWith("(off)")), "every effect-gated cell is labelled (off)");
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
            Assert.AreEqual(9 + _config.hierarchy_levels.Count, Cells(firstRoot).Count);

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

        // A developer reported not seeing Hierarchy Levels table edits reflected in this grid while
        // Play Mode ran. Size/Show Label/Rotate/Reveal belong to LivePlayModeMarkerApplier, not the
        // effects applier -- ApplyMarkerSettings must rebuild THIS grid too (WallSession.cs), or a
        // live Size edit reaches every real marker (proven separately in
        // LivePlayModeConfigTests.MarkerApplier_DrivesRealMarkers_HierarchyLevelFields) but leaves
        // this preview grid showing the stale size, exactly the gap being fixed here.
        [UnityTest]
        public IEnumerator ApplyMarkerSettings_RebuildsThePreviewGridLive_WhenALevelsSizeChanges()
        {
            _config.effect_defaults.preview.enabled = true;
            var ws = Spawn();
            yield return null;
            var firstRoot = ws.EffectsPreviewRoot;
            Assert.IsNotNull(firstRoot, "Precondition: the grid is on.");
            int levelIndex = 0;
            float originalSizeCm = _config.hierarchy_levels[levelIndex].size_cm;

            var edited = JsonUtility.FromJson<WallConfigData>(JsonUtility.ToJson(_config));
            edited.hierarchy_levels[levelIndex].size_cm = originalSizeCm + 25f;
            ws.ApplyMarkerSettings(edited);
            yield return null;

            Assert.IsTrue(firstRoot == null, "The old grid must be destroyed, not left stale.");
            var symbol = (RectTransform)Cells(ws.EffectsPreviewRoot)[9 + levelIndex].transform.Find("Symbol");
            Assert.AreEqual((originalSizeCm + 25f) / 100f, symbol.sizeDelta.x, 1e-4f,
                "The rebuilt grid's level cell must show the new Size (cm), not the stale one.");
        }

        // With a real POI chosen as the Base marker, every level cell is labelled with THAT POI's own
        // name (e.g. "The Lamp"), exactly like a real POI at that level -- never the level name.
        [UnityTest]
        public IEnumerator LevelCells_AreLabelledWithTheBasePoisOwnName()
        {
            var basePoi = _config.pois.First(p => !string.IsNullOrWhiteSpace(p.name));
            _config.effect_defaults.preview.enabled = true;
            _config.effect_defaults.preview.base_poi_id = basePoi.id;
            var ws = Spawn();
            yield return null;

            var levelCells = Cells(ws.EffectsPreviewRoot).Skip(9).ToList();
            Assert.AreEqual(_config.hierarchy_levels.Count, levelCells.Count, "Precondition: one cell per level.");
            foreach (var cell in levelCells)
                Assert.AreEqual(basePoi.name, LabelTextOf(cell), cell.name + " must show the base marker's name");
            for (int i = 0; i < levelCells.Count; i++)
            {
                var level = _config.hierarchy_levels[i];
                string levelName = string.IsNullOrWhiteSpace(level.level_name) ? level.key : level.level_name;
                Assert.AreEqual("Preview_Level: " + levelName, levelCells[i].name, "the cell stays findable by its level");
            }
        }

        // A level's Marker Label Style reaches its grid cell's REAL label live: ApplyMarkerSettings (the
        // marker applier's runtime seam) rebuilds the grid and the cell's TMP font size and Label
        // offset follow the level's own ratios -- the "Aa sliders do nothing on the grid" bug.
        [UnityTest]
        public IEnumerator ApplyMarkerSettings_ALevelsMarkerLabelStyleReachesItsGridCellLive()
        {
            _config.effect_defaults.preview.enabled = true;
            int levelIndex = _config.hierarchy_levels.FindIndex(l => l.show_label);
            Assert.GreaterOrEqual(levelIndex, 0, "Precondition: the shipped config has a level that shows its label.");
            var ws = Spawn();
            yield return null;

            var edited = JsonUtility.FromJson<WallConfigData>(JsonUtility.ToJson(_config));
            var level = edited.hierarchy_levels[levelIndex];
            level.override_label_style = true;
            level.label_gap_ratio = 0.4f;
            level.label_font_size_ratio = 0.9f;
            level.label_font_key = "liberation_sans";
            ws.ApplyMarkerSettings(edited);
            yield return null;

            var cell = Cells(ws.EffectsPreviewRoot)[9 + levelIndex];
            var labelRect = cell.GetComponentInChildren<MarkerView>().LabelRect;
            var tmp = labelRect.GetComponent("TextMeshProUGUI");
            float fontSize = (float)tmp.GetType().GetProperty("fontSize").GetValue(tmp);
            float diameter = level.size_cm / 100f;
            Assert.AreEqual(0.9f * diameter, fontSize, 1e-4f, "the level's own font size ratio reaches the grid cell");
            Assert.AreEqual(-diameter * 0.5f - 0.4f * diameter, labelRect.anchoredPosition.y, 1e-4f, "the level's own gap reaches the grid cell");

            level.override_label_style = false;
            ws.ApplyMarkerSettings(JsonUtility.FromJson<WallConfigData>(JsonUtility.ToJson(edited)));
            yield return null;
            cell = Cells(ws.EffectsPreviewRoot)[9 + levelIndex];
            tmp = cell.GetComponentInChildren<MarkerView>().LabelRect.GetComponent("TextMeshProUGUI");
            fontSize = (float)tmp.GetType().GetProperty("fontSize").GetValue(tmp);
            Assert.AreEqual(MarkerVisualSettings.ClampLabelFontSizeRatio(edited.label_font_size_ratio) * diameter, fontSize, 1e-4f,
                "override off: the grid cell goes back to the wall default");
        }

        // "Move closer to a marker": the demo grid's own scroll-wheel zoom (DevPreviewCameraDolly),
        // driving the real EffectsPreviewFocus component on a real spawned grid.
        [UnityTest]
        public IEnumerator ScrollingIn_MovesTheGridCameraCloser_AndARebuiltGridKeepsTheSameZoom()
        {
            _config.effect_defaults.preview.enabled = true;
            var ws = Spawn();
            yield return null;
            var focus = ws.EffectsPreviewRoot.GetComponent<EffectsPreviewFocus>();
            float distanceBefore = -focus.ViewCamera.transform.localPosition.z;

            focus.Dolly.ApplyScroll(3f);
            yield return null;

            float distanceAfter = -focus.ViewCamera.transform.localPosition.z;
            Assert.Less(distanceAfter, distanceBefore, "scrolling in must move the grid camera closer");

            // A developer reported the camera snapping back to the auto-fit framing on every editor
            // edit made while Play Mode ran, since a rebuild used to hand the new grid's Focus a
            // fresh zero-offset dolly. WallSession now carries the offset across the rebuild
            // (RebuildEffectsPreview, 2026-09-22) -- a live config push must NOT reset the view.
            float zoomBefore = focus.Dolly.ZoomOffsetMetres;
            var edited = JsonUtility.FromJson<WallConfigData>(JsonUtility.ToJson(_config));
            edited.hierarchy_levels[0].size_cm += 5f;
            ws.ApplyMarkerSettings(edited);
            yield return null;

            var newFocus = ws.EffectsPreviewRoot.GetComponent<EffectsPreviewFocus>();
            Assert.AreNotSame(focus, newFocus, "Precondition: the grid (and its Focus) was rebuilt.");
            Assert.AreEqual(zoomBefore, newFocus.Dolly.ZoomOffsetMetres, 1e-4f,
                "a rebuilt grid must carry the developer's zoom across the rebuild, not reset it");
        }

        // Same guarantee as the zoom test above, for pan -- both offsets must survive a rebuild.
        [UnityTest]
        public IEnumerator PanningTheDolly_ThenRebuilding_KeepsTheSamePan()
        {
            _config.effect_defaults.preview.enabled = true;
            var ws = Spawn();
            yield return null;
            var focus = ws.EffectsPreviewRoot.GetComponent<EffectsPreviewFocus>();
            focus.Dolly.ApplyPan(new Vector2(0.1f, 0.05f));
            yield return null;
            Vector2 panBefore = focus.Dolly.PanOffsetMetres;

            var edited = JsonUtility.FromJson<WallConfigData>(JsonUtility.ToJson(_config));
            edited.hierarchy_levels[0].size_cm += 5f;
            ws.ApplyMarkerSettings(edited);
            yield return null;

            var newFocus = ws.EffectsPreviewRoot.GetComponent<EffectsPreviewFocus>();
            Assert.AreNotSame(focus, newFocus, "Precondition: the grid (and its Focus) was rebuilt.");
            Assert.AreEqual(panBefore.x, newFocus.Dolly.PanOffsetMetres.x, 1e-4f, "pan.x must survive a rebuild");
            Assert.AreEqual(panBefore.y, newFocus.Dolly.PanOffsetMetres.y, 1e-4f, "pan.y must survive a rebuild");
        }

        // "Move around": WASD/mouse-drag pan (DevPreviewCameraDolly.ApplyPan), driving the real
        // EffectsPreviewFocus. DevCameraInput itself needs a real mouse-over-Game-view + Input
        // System state this test cannot simulate headlessly, so this drives the dolly directly --
        // exactly the seam DevCameraInput hands off to (proven wired at the call site by reading
        // EffectsPreviewFocus.cs itself, per 40-testing.md 4.2.1's "confirm the call site is
        // reached" rule, not just that a method with this name exists somewhere).
        [UnityTest]
        public IEnumerator PanningTheDolly_MovesTheGridCameraSideways_ClampedToTheGridsOwnExtent()
        {
            _config.effect_defaults.preview.enabled = true;
            var ws = Spawn();
            yield return null;
            var focus = ws.EffectsPreviewRoot.GetComponent<EffectsPreviewFocus>();
            Vector3 posBefore = focus.ViewCamera.transform.localPosition;

            focus.Dolly.ApplyPan(new Vector2(0.2f, 0.1f));
            yield return null;

            Vector3 posAfter = focus.ViewCamera.transform.localPosition;
            Assert.AreNotEqual(posBefore.x, posAfter.x, "panning must move the grid camera sideways");
            Assert.AreNotEqual(posBefore.y, posAfter.y, "panning must move the grid camera vertically");
            Assert.AreEqual(posBefore.z, posAfter.z, 1e-4f, "panning must not change the zoom distance");

            // An enormous pan is clamped to the grid's own extent, never loses the grid entirely.
            focus.Dolly.ApplyPan(new Vector2(10000f, 10000f));
            yield return null;
            Vector3 posClamped = focus.ViewCamera.transform.localPosition;
            Assert.Less(Mathf.Abs(posClamped.x), 50f, "an extreme pan must still be clamped to a sane range");
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
