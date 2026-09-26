using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TileStories.Tests
{
    // "Can a human actually SEE it?" -- the render-level proof for "Add effects demo grid". The cameras are
    // rendered to a texture (portrait aspect like a phone / narrow Game view), the pixels are read back,
    // and each cell is judged from real pixels. An opaque wall a few centimetres in front of the main
    // camera stands in for the room mesh: the grid must be visible anyway, because it lives far away and
    // is drawn by its own camera. (The first version placed the grid in front of the camera; scenery hid
    // it, and only a pixel test could see that -- viewport-coordinate checks pass while nothing is visible.)
    //
    // Measured per cell, from pixels only:
    //   symbol : the centre of the marker differs from the neutral backdrop (a marker was drawn there);
    //   halo   : pixels that differ from the SAME grid rendered with every effect switched off, in the ring
    //            AROUND the symbol (upper half only, so the label below does not count), i.e. an effect
    //            layer is visibly outside the symbol.
    // Animated effects are sampled over ~2 s (longer than every effect period) and judged on their best frame.
    public class EffectsPreviewRenderTests
    {
        private const string MarkerPrefabPath = "Assets/Framework/Runtime/UI/Markers/POI_Marker.prefab";
        private const int TexWidth = 600;
        private const int TexHeight = 680;
        private const int SampleFrames = 16;
        private const float SampleSpacing = 0.13f;
        private static readonly Color MainBackground = new Color(0.05f, 0.05f, 0.10f, 1f);
        private static readonly Color WallColor = new Color(0.22f, 0.26f, 0.30f, 1f);

        private Camera _cam;
        private GameObject _camGO;
        private GameObject _wall;
        private GameObject _prefab;
        private WallConfigData _config;
        private EffectsPreviewFocus _focus;
        private readonly List<GameObject> _extra = new();

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            WallConfigData config = null;
            yield return WallConfigLoader.LoadFromStreamingAssets("LivingRoom/config.json", c => config = c);
            Assert.IsNotNull(config, "LivingRoom config must load.");
            _config = config;
            _config.effect_defaults.preview = new EffectDefaults.EffectPreviewSettings();   // framework defaults
            CategoryPalette.Configure(_config.category_styles);
            MarkerHierarchyResolver.Configure(_config.hierarchy_levels);
#if UNITY_EDITOR
            _prefab = AssetDatabase.LoadAssetAtPath<GameObject>(MarkerPrefabPath);
#endif
            Assert.IsNotNull(_prefab, "POI_Marker prefab must load.");

            foreach (var stray in GameObject.FindGameObjectsWithTag("MainCamera"))
                Object.DestroyImmediate(stray);
            _camGO = new GameObject("RenderTestCamera") { tag = "MainCamera" };
            _cam = _camGO.AddComponent<Camera>();
            _cam.clearFlags = CameraClearFlags.SolidColor;
            _cam.backgroundColor = MainBackground;
            _cam.fieldOfView = 60f;
            _cam.nearClipPlane = 0.1f;
            _cam.farClipPlane = 50f;
            _cam.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            _cam.targetTexture = new RenderTexture(TexWidth, TexHeight, 24);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            MarkerHierarchyResolver.ResetToDefaults();
            foreach (var go in _extra) if (go != null) Object.DestroyImmediate(go);
            _extra.Clear();
            if (_wall != null) Object.DestroyImmediate(_wall);
            var preview = GameObject.Find("EffectsPreview");
            if (preview != null) Object.DestroyImmediate(preview);
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

        // ---------------- scene + capture helpers ----------------

        // An opaque wall in front of the main camera, like the room mesh in the real scene.
        private void AddWall(float distance)
        {
            _wall = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Object.DestroyImmediate(_wall.GetComponent<Collider>());
            _wall.transform.position = new Vector3(0f, 0f, distance);
            _wall.transform.localScale = new Vector3(40f, 40f, 1f);
            var shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color") ?? Shader.Find("Sprites/Default");
            var mat = new Material(shader) { color = WallColor };
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", WallColor);
            _wall.GetComponent<Renderer>().sharedMaterial = mat;
        }

        // The frame a viewer sees: the main camera, then (if a grid exists) the grid camera on top.
        private Texture2D Capture(bool includeGrid = true)
        {
            var rt = _cam.targetTexture;
            _cam.Render();
            if (includeGrid && _focus != null) _focus.ViewCamera.Render();
            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();
            RenderTexture.active = prev;
            return tex;
        }

        private static Texture2D Uniform(Color color)
        {
            var tex = new Texture2D(TexWidth, TexHeight, TextureFormat.RGBA32, false);
            var pixels = new Color32[TexWidth * TexHeight];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
            tex.SetPixels32(pixels);
            tex.Apply();
            return tex;
        }

        private static bool Differs(Color32 a, Color32 b) =>
            Mathf.Abs(a.r - b.r) > 22 || Mathf.Abs(a.g - b.g) > 22 || Mathf.Abs(a.b - b.b) > 22;

        private static void SavePng(Texture2D tex, string name)
        {
            string dir = Path.Combine(Application.dataPath, "Screenshots");
            Directory.CreateDirectory(dir);
            File.WriteAllBytes(Path.Combine(dir, name), tex.EncodeToPNG());
        }

        private static float PixelRadius(Camera viewCam, Vector3 worldCentre, float worldRadius)
        {
            Vector3 a = viewCam.WorldToScreenPoint(worldCentre);
            Vector3 b = viewCam.WorldToScreenPoint(worldCentre + viewCam.transform.right * worldRadius);
            return Mathf.Abs(b.x - a.x);
        }

        private struct CellReading
        {
            public string Name;
            public float SymbolRadiusPx;
            public float SymbolCoverage;   // 0..1 of the symbol disc that differs from its baseline
            public float BestHalo;         // 0..1 of the surrounding ring that differs, best frame
            public float WorstHalo;        // same, worst frame
            public float MaxExtentRatio;   // farthest changed pixel (upper half) / symbol radius, best frame
        }

        // Judge one cell from pixels only, over all captured frames.
        private CellReading Read(GameObject cell, List<Texture2D> frames, Camera viewCam, Texture2D haloBaseline, Texture2D symbolBaseline)
        {
            var symbol = (RectTransform)cell.transform.Find("Symbol");
            // Rest radius: the Pulse effect scales the symbol itself, so divide its own scale out.
            float worldRadius = symbol.rect.width * 0.5f * symbol.lossyScale.x / symbol.localScale.x;
            Vector3 centreWorld = symbol.position;
            Vector3 sp = viewCam.WorldToScreenPoint(centreWorld);
            float r = PixelRadius(viewCam, centreWorld, worldRadius);

            var reading = new CellReading { Name = cell.name, SymbolRadiusPx = r, WorstHalo = 1f };
            int cx = Mathf.RoundToInt(sp.x), cy = Mathf.RoundToInt(sp.y);
            int span = Mathf.CeilToInt(r * 2.4f);

            for (int f = 0; f < frames.Count; f++)
            {
                int symbolPixels = 0, symbolChanged = 0, haloPixels = 0, haloChanged = 0;
                float extent = 0f;
                for (int y = Mathf.Max(0, cy - span); y < Mathf.Min(TexHeight, cy + span); y++)
                    for (int x = Mathf.Max(0, cx - span); x < Mathf.Min(TexWidth, cx + span); x++)
                    {
                        float dx = x - sp.x, dy = y - sp.y;
                        float dist = Mathf.Sqrt(dx * dx + dy * dy);
                        var pixel = frames[f].GetPixel(x, y);
                        if (dist <= r * 0.7f)
                        {
                            symbolPixels++;
                            if (Differs(pixel, symbolBaseline.GetPixel(x, y))) symbolChanged++;
                        }
                        else if (dy > 0f && dist >= r * 1.05f)
                        {
                            bool changed = Differs(pixel, haloBaseline.GetPixel(x, y));
                            if (dist <= r * 1.9f)
                            {
                                haloPixels++;
                                if (changed) haloChanged++;
                            }
                            if (changed && dist <= r * 2.3f) extent = Mathf.Max(extent, dist);
                        }
                    }

                float cover = symbolPixels == 0 ? 0f : symbolChanged / (float)symbolPixels;
                float halo = haloPixels == 0 ? 0f : haloChanged / (float)haloPixels;
                reading.SymbolCoverage = Mathf.Max(reading.SymbolCoverage, cover);
                reading.WorstHalo = Mathf.Min(reading.WorstHalo, halo);
                if (halo >= reading.BestHalo)
                {
                    reading.BestHalo = halo;
                    reading.MaxExtentRatio = extent / r;
                }
            }
            return reading;
        }

        // Spawn the grid the way WallSession does, with the wall's own effect defaults.
        private GameObject SpawnGrid(string basePoiId = "")
        {
            _config.effect_defaults.preview.enabled = true;
            _config.effect_defaults.preview.base_poi_id = basePoiId;
            var defaults = _config.effect_defaults;
            var root = EffectsPreviewSpawner.TrySpawn(_config, _prefab, _cam,
                (view, anchor, style) => view.Initialise(anchor, MarkerVisualSettings.Default(), MarkerEffectFlags.None, defaults, style));
            Assert.IsNotNull(root, "The grid must spawn.");
            _focus = root.GetComponent<EffectsPreviewFocus>();
            Assert.IsNotNull(_focus, "The grid root owns the focus component.");
            return root;
        }

        private static List<GameObject> CellsOf(GameObject root) =>
            Enumerable.Range(0, root.transform.childCount).Select(i => root.transform.GetChild(i).gameObject)
                .Where(c => c.name.StartsWith("Preview_")).ToList();

        private IEnumerator SampleFrames_(List<Texture2D> frames)
        {
            for (int i = 0; i < SampleFrames; i++)
            {
                yield return new WaitForSeconds(SampleSpacing);
                frames.Add(Capture());
            }
        }

        // Render the SAME grid with every effect switched off: the reference for "what an effect adds".
        private IEnumerator CaptureEffectsOffBaseline(System.Action<Texture2D> done)
        {
            _config.effect_defaults.effects_enabled = false;
            var off = SpawnGrid();
            yield return new WaitForSeconds(1.6f);
            var baseline = Capture();
            Object.DestroyImmediate(off);
            _focus = null;
            _config.effect_defaults.effects_enabled = true;
            done(baseline);
        }

        private static string Describe(CellReading c) =>
            $"{c.Name}: symbol={c.SymbolCoverage:0.00} haloBest={c.BestHalo:0.000} haloWorst={c.WorstHalo:0.000} extent={c.MaxExtentRatio:0.00}r r={c.SymbolRadiusPx:0}px";

        // ---------------- visibility ----------------

        [UnityTest]
        public IEnumerator EveryCellOfTheGrid_IsDrawn_EvenWithAWallRightInFrontOfTheCamera()
        {
            AddWall(0.5f);   // the worst case: scenery half a metre in front of the real camera
            var root = SpawnGrid();
            yield return new WaitForSeconds(1.6f);   // reveals finish (longest level: 1.0 s delay + 0.25 s fade)
            var frames = new List<Texture2D>();
            yield return SampleFrames_(frames);
            SavePng(frames[3], "effects_preview_grid_render.png");

            var symbolBaseline = Uniform(EffectsPreviewSpawner.BackgroundColor);
            var cells = CellsOf(root);
            // Quick row (3) + combo row (6, one a blank spacer with no marker at all) + 5 level cells.
            Assert.AreEqual(EffectsPreviewSpawner.LevelRowStart + 5, cells.Count, "Precondition: 9 quick/combo cells + 5 level cells.");
            var report = new List<string>();
            var failures = new List<string>();
            for (int i = 0; i < cells.Count; i++)
            {
                if (i == EffectsPreviewSpawner.IndexComboSpacer) continue;   // no marker spawned there, nothing to read
                var reading = Read(cells[i], frames, _focus.ViewCamera, symbolBaseline, symbolBaseline);
                report.Add(Describe(reading));
                if (reading.SymbolCoverage < 0.85f)
                    failures.Add($"{cells[i].name} is NOT visible (symbol coverage {reading.SymbolCoverage:0.00})");
                // The quick/combo-row cells must be big enough on this portrait view to judge an effect by eye.
                if (i < EffectsPreviewSpawner.LevelRowStart && reading.SymbolRadiusPx < 18f)
                    failures.Add($"{cells[i].name} is drawn too small to judge (symbol radius {reading.SymbolRadiusPx:0} px, needs >= 18)");
            }
            Debug.Log("[PreviewRender] cells:\n" + string.Join("\n", report));
            CollectionAssert.IsEmpty(failures, string.Join("\n", failures) + "\n--- readings ---\n" + string.Join("\n", report));
        }

        [UnityTest]
        public IEnumerator TheMainCamera_NeverDrawsTheGrid_SoTheRealViewIsUntouchedByIt()
        {
            AddWall(2.0f);
            var before = Capture(includeGrid: false);
            SpawnGrid();
            yield return new WaitForSeconds(1.6f);
            var after = Capture(includeGrid: false);   // main camera only

            int different = 0;
            for (int y = 0; y < TexHeight; y += 2)
                for (int x = 0; x < TexWidth; x += 2)
                    if (Differs(after.GetPixel(x, y), before.GetPixel(x, y))) different++;
            Assert.AreEqual(0, different, "The main camera must render nothing of the far-away grid.");
            Assert.Less(_cam.farClipPlane, EffectsPreviewSpawner.FarOffsetMetres, "Precondition: the grid is beyond the main camera's far plane.");
            Assert.Greater(_focus.ViewCamera.depth, _cam.depth, "The grid camera draws on top.");
            Assert.AreNotEqual(_cam, _focus.ViewCamera);
        }

        [UnityTest]
        public IEnumerator EveryEffectCell_ShowsItsEffectAroundTheSymbol_AndTheControlCellShowsNothing()
        {
            AddWall(0.5f);
            Texture2D haloBaseline = null;
            yield return CaptureEffectsOffBaseline(t => haloBaseline = t);
            var root = SpawnGrid();
            yield return new WaitForSeconds(1.6f);
            var frames = new List<Texture2D>();
            yield return SampleFrames_(frames);

            var symbolBaseline = Uniform(EffectsPreviewSpawner.BackgroundColor);
            var cells = CellsOf(root);

            var control = Read(cells[EffectsPreviewSpawner.IndexNoEffect], frames, _focus.ViewCamera, haloBaseline, symbolBaseline);
            Assert.Less(control.BestHalo, 0.01f, "'No effect' must show nothing around the symbol.");
            Assert.Greater(control.SymbolCoverage, 0.85f, "Precondition: the control marker itself is drawn.");

            var effectCells = new (string name, int index)[]
            {
                ("Pulse", EffectsPreviewSpawner.IndexPulse),
                ("Ripple Rings", EffectsPreviewSpawner.IndexRippleRings),
                ("Ripple Discs", EffectsPreviewSpawner.IndexRippleDiscs),
                ("Halo Ring", EffectsPreviewSpawner.IndexHaloRing),
                ("Halo Disc", EffectsPreviewSpawner.IndexHaloDisc),
                ("Beacon", EffectsPreviewSpawner.IndexBeacon),
            };
            var report = new List<string> { Describe(control) };
            var failures = new List<string>();
            foreach (var e in effectCells)
            {
                var r = Read(cells[e.index], frames, _focus.ViewCamera, haloBaseline, symbolBaseline);
                report.Add(Describe(r));
                if (r.BestHalo < 0.03f)
                    failures.Add($"{e.name} is NOT visibly different from 'No effect' (best halo coverage {r.BestHalo:0.000}, needs >= 0.03)");
                if (r.MaxExtentRatio < 1.06f)
                    failures.Add($"{e.name} never reaches outside the symbol (extent {r.MaxExtentRatio:0.00}r)");
            }
            Debug.Log("[PreviewRender] effect cells:\n" + string.Join("\n", report));
            CollectionAssert.IsEmpty(failures, string.Join("\n", failures) + "\n--- readings ---\n" + string.Join("\n", report));
        }

        [UnityTest]
        public IEnumerator Effects_AnimateOverTime_InsteadOfBeingAStillImage()
        {
            AddWall(0.5f);
            Texture2D haloBaseline = null;
            yield return CaptureEffectsOffBaseline(t => haloBaseline = t);
            var root = SpawnGrid();
            yield return new WaitForSeconds(1.6f);
            var frames = new List<Texture2D>();
            yield return SampleFrames_(frames);

            // Pulse / Halo Ring / Halo Disc breathe and Ripple / Beacon flow: the covered ring area must
            // change between frames (a static image would score the same on every frame).
            var symbolBaseline = Uniform(EffectsPreviewSpawner.BackgroundColor);
            var cells = CellsOf(root);
            var effectCells = new (string name, int index)[]
            {
                ("Pulse", EffectsPreviewSpawner.IndexPulse),
                ("Ripple Rings", EffectsPreviewSpawner.IndexRippleRings),
                ("Ripple Discs", EffectsPreviewSpawner.IndexRippleDiscs),
                ("Halo Ring", EffectsPreviewSpawner.IndexHaloRing),
                ("Halo Disc", EffectsPreviewSpawner.IndexHaloDisc),
                ("Beacon", EffectsPreviewSpawner.IndexBeacon),
            };
            var failures = new List<string>();
            foreach (var e in effectCells)
            {
                var one = Read(cells[e.index], frames, _focus.ViewCamera, haloBaseline, symbolBaseline);
                if (one.BestHalo - one.WorstHalo < 0.005f)
                    failures.Add($"{e.name} does not animate (halo coverage best {one.BestHalo:0.000} vs worst {one.WorstHalo:0.000})");
            }
            var control = Read(cells[EffectsPreviewSpawner.IndexNoEffect], frames, _focus.ViewCamera, haloBaseline, symbolBaseline);
            Assert.Less(control.BestHalo - control.WorstHalo, 0.005f, "The control cell must stay still.");
            CollectionAssert.IsEmpty(failures, string.Join("\n", failures));
        }

        // ---------------- follows the real camera ----------------

        [UnityTest]
        public IEnumerator EveryCellLabel_FacesTheGridCamera_AndIsUpright_EvenWhenTheRealCameraTurnsAndRolls()
        {
            AddWall(0.5f);
            var root = SpawnGrid();
            var cells = CellsOf(root);
            var view = _focus.ViewCamera;

            // The real camera (a phone) yaws, pitches and rolls: the grid must stay readable and on screen.
            foreach (var pose in new[] { Quaternion.identity, Quaternion.Euler(10f, 40f, 20f), Quaternion.Euler(-25f, -70f, -35f) })
            {
                _cam.transform.rotation = pose;
                yield return new WaitForSeconds(0.8f);   // billboards smooth over ~0.12 s

                var failures = new List<string>();
                foreach (var cell in cells)
                {
                    var markerView = cell.GetComponentInChildren<MarkerView>();
                    if (markerView == null) continue;   // the spacer cell has no marker at all
                    var label = markerView.LabelRect;
                    float facing = Vector3.Dot(label.forward, view.transform.forward);
                    float upright = Vector3.Dot(label.up, view.transform.up);
                    Vector3 vp = view.WorldToViewportPoint(cell.transform.position);
                    if (facing < 0.9f || upright < 0.9f)
                        failures.Add($"{cell.name}: label facing={facing:0.00}, upright={upright:0.00} (both need > 0.9)");
                    if (vp.z <= 0f || vp.x < 0.02f || vp.x > 0.98f || vp.y < 0.02f || vp.y > 0.98f)
                        failures.Add($"{cell.name}: outside the grid camera's view {vp}");
                }
                CollectionAssert.IsEmpty(failures, $"main camera rotation {pose.eulerAngles}:\n" + string.Join("\n", failures));
            }
        }

        [UnityTest]
        public IEnumerator TheGridRefitsWhenTheViewChangesShape()
        {
            AddWall(0.5f);
            var root = SpawnGrid();
            var cells = CellsOf(root);
            int portraitColumns = _focus.Columns;

            // A Game view resized to landscape (or a phone rotated).
            var oldTexture = _cam.targetTexture;
            var landscape = new RenderTexture(1000, 500, 24);
            _cam.targetTexture = landscape;
            yield return null;
            yield return null;
            oldTexture.Release();
            Object.DestroyImmediate(oldTexture);

            Assert.Greater(_focus.Columns, portraitColumns, "A wider view wraps the grid into more columns.");
            Assert.AreEqual(landscape, _focus.ViewCamera.targetTexture, "The grid camera follows the main camera's render target.");
            foreach (var cell in cells)
            {
                Vector3 vp = _focus.ViewCamera.WorldToViewportPoint(cell.transform.position);
                Assert.IsTrue(vp.z > 0f && vp.x > 0.02f && vp.x < 0.98f && vp.y > 0.02f && vp.y < 0.98f, $"{cell.name} must stay in view after the resize, was {vp}");
            }

            // Restore a fresh texture (TearDown releases whatever the main camera holds); only release the
            // landscape one after BOTH cameras have moved off it.
            _cam.targetTexture = new RenderTexture(TexWidth, TexHeight, 24);
            yield return null;
            yield return null;
            Assert.AreEqual(_cam.targetTexture, _focus.ViewCamera.targetTexture, "The grid camera follows the restored target too.");
            landscape.Release();
            Object.DestroyImmediate(landscape);
        }

        // ---------------- effects keep their look whatever the marker size ----------------

        [UnityTest]
        public IEnumerator EffectsKeepTheirLookRelativeToTheMarker_WhateverTheMarkerSize()
        {
            // The same effect on a 7 cm, 20 cm and 30 cm marker must reach about the same distance OUTSIDE
            // the symbol (in symbol radii). Absolute effect sizes would hide the halo behind a big marker
            // and blow it up around a small one. Markers here sit in front of the main camera (no grid).
            AddWall(2.0f);
            var baseline = Capture();
            var defaults = _config.effect_defaults;
            var effects = new[] { MarkerEffectFlags.RippleRings, MarkerEffectFlags.RippleDiscs, MarkerEffectFlags.HaloRing, MarkerEffectFlags.HaloDisc, MarkerEffectFlags.Beacon };
            var sizes = new[] { 7f, 20f, 30f };

            var report = new List<string>();
            var failures = new List<string>();
            foreach (var effect in effects)
            {
                var ratios = new List<float>();
                foreach (float sizeCm in sizes)
                {
                    var go = Object.Instantiate(_prefab);
                    _extra.Add(go);
                    go.name = $"Size_{effect}_{sizeCm}";
                    go.transform.position = new Vector3(0f, 0f, 1.0f);
                    var anchor = go.GetComponent<POIAnchor>() ?? go.AddComponent<POIAnchor>();
                    anchor.Initialise(new POIData { id = go.name, name = "x" });
                    var style = new HierarchyStyle(sizeCm, false, effect, false, 0f, 0f);
                    go.GetComponentInChildren<MarkerView>().Initialise(anchor, MarkerVisualSettings.Default(), MarkerEffectFlags.None, defaults, style);
                    go.GetComponentInChildren<MarkerBillboard>()?.Configure(new OrientationSettings(), "", null);
                    // Plain grey symbol so the symbol pixels are recognisable.
                    var symbolImage = go.transform.Find("Symbol").GetComponent<UnityEngine.UI.Image>();
                    symbolImage.sprite = MarkerCircleSpriteFactory.GetFilled(0.98f);
                    symbolImage.color = new Color(0.55f, 0.55f, 0.55f, 1f);
                    go.transform.Find("Symbol/Icon").GetComponent<UnityEngine.UI.Image>().enabled = false;

                    yield return new WaitForSeconds(0.2f);
                    var frames = new List<Texture2D>();
                    yield return SampleFrames_(frames);
                    var reading = Read(go, frames, _cam, baseline, baseline);
                    ratios.Add(reading.MaxExtentRatio);
                    report.Add($"{effect} @ {sizeCm} cm: {Describe(reading)}");
                    Object.DestroyImmediate(go);
                }

                float min = ratios.Min(), max = ratios.Max();
                if (min < 1.06f)
                    failures.Add($"{effect}: not visible outside the symbol on some size (extents {string.Join(", ", ratios.Select(x => x.ToString("0.00")))} r for 7/20/30 cm)");
                else if (max / min > 1.35f)
                    failures.Add($"{effect}: look depends on marker size (extents {string.Join(", ", ratios.Select(x => x.ToString("0.00")))} r for 7/20/30 cm)");
            }
            Debug.Log("[PreviewRender] size sweep:\n" + string.Join("\n", report));
            CollectionAssert.IsEmpty(failures, string.Join("\n", failures) + "\n--- readings ---\n" + string.Join("\n", report));
        }
    }
}
