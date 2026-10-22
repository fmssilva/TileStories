using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Dev.StyleSpike.Tests
{
    // DEV-ONLY (_3.3 P0 S5, _3.3 R7): the sheet ground over the camera on 6.3 -- (1) frosted by a 1/8 low-resolution camera copy drawn
    // by a UI Shader Graph material in screen space, theme tint on top; (2) tinted (translucent fill, raw camera behind); (3) solid.
    // White opaque blocks with a 1 px border and a shadow attempt on each. Measured on the PIXELS (linear colour): block edge
    // contrast against the sheet ground, how busy the ground is, and the cost of (1) per frame. The camera stand-in is a sunny
    // azulejo wall photo rendered by a camera (the shared Dev/MarkerGallery backdrop), behind the UI like the AR feed.
    public class S5GlassTests
    {
        private const string PhotoPath = "Assets/Dev/MarkerGallery/Backdrops/backdrop.png";
        private const string FrostMaterialPath = "Assets/Dev/StyleSpike/S5Glass/SpikeFrost.mat";
        private const int Downscale = 8;
        private const float SheetTop = 330f;
        private const int CostFrames = 60;

        private static readonly Color Ground = new Color32(243, 240, 233, 255);       // the light theme's sheet ground (spike value)
        private static readonly Color BlockBorder = new Color32(214, 208, 197, 255);
        private static readonly Color TintFill = new Color(1f, 1f, 1f, 0.55f);           // the tinted level: white at 55 %

        private SpikeStage _stage;
        private SpikeGlassRig _rig;
        private Material _frost;
        private VisualElement _groundLayer;
        private readonly List<VisualElement> _blocks = new List<VisualElement>();

        [UnitySetUp]
        public IEnumerator Build()
        {
            var panel = AssetDatabase.LoadAssetAtPath<PanelSettings>(SpikeStageTests.PanelSettingsPath);
            var photo = AssetDatabase.LoadAssetAtPath<Texture2D>(PhotoPath);
            var frostAsset = AssetDatabase.LoadAssetAtPath<Material>(FrostMaterialPath);
            Assert.IsNotNull(photo, "the sunny-wall photo is missing -- SETUP"); Assert.IsNotNull(frostAsset, "SpikeFrost.mat missing -- SETUP");
            Assert.IsTrue(frostAsset.shader.isSupported, "the frost shader does not compile here -- SETUP");
            _stage = new SpikeStage(panel, Color.black);
            _rig = new SpikeGlassRig(_stage.Camera, photo, Downscale);
            _frost = new Material(frostAsset);
            _frost.SetTexture("_Frost", _rig.LowRes);
            for (int i = 0; i < 4; i++) yield return null;
        }

        [TearDown]
        public void TearDown()
        {
            _stage?.Dispose();
            _rig?.Dispose();
            if (_frost != null) Object.Destroy(_frost);
        }

        [UnityTest]
        public IEnumerator Frosted_SamplesTheCameraCopyWhereTheSheetSits()
        {
            // the photo alone: the reference the frosted ground must match region by region once its tint is off
            Texture2D photoOnly = null;
            yield return SpikeEvidence.Grab(t => photoOnly = t);
            try
            {
                SpikeEvidence.Save(photoOnly, "S5_photo_only");
                float flip0 = 0f, flip1 = 0f;
                foreach (float flip in new[] { 0f, 1f })
                {
                    BuildSheet("frosted", shadows: false, blocks: false);
                    _frost.SetFloat("_TintA", 0f);
                    _frost.SetFloat("_FlipV", flip);
                    for (int i = 0; i < 3; i++) yield return null;
                    Texture2D t = null;
                    yield return SpikeEvidence.Grab(x => t = x);
                    float d = MeanCellDifference(photoOnly, t, _groundLayer.worldBound);
                    if (flip == 0f) flip0 = d; else flip1 = d;
                    Color mp = MeanColour(photoOnly, _groundLayer.worldBound), mf = MeanColour(t, _groundLayer.worldBound);
                    SpikeEvidence.Note("S5", $"FlipV {flip}: mean colour over the sheet -- photo {(Color32)mp}, frost (tint off) {(Color32)mf}");
                    SpikeEvidence.Save(t, $"S5_frost_tintoff_flip{flip}");
                    Object.Destroy(t);
                }
                SpikeEvidence.Note("S5", $"frost alignment, mean 48 px cell difference to the photo: FlipV 0 = {flip0:0.000}, FlipV 1 = {flip1:0.000} (RT {_rig.LowRes.width}x{_rig.LowRes.height})");
                // 0.06 = 15 of 255 per channel: the tint is off, so only the blur separates the two frames
                Assert.Less(Mathf.Min(flip0, flip1), 0.06f, "one orientation of the screen-space lookup lines the frost up with the photo");
                Assert.Greater(Mathf.Max(flip0, flip1), Mathf.Min(flip0, flip1) * 2f, "the other orientation is clearly wrong (the measurement can tell them apart)");
                FoundFlipV = flip0 < flip1 ? 0f : 1f;
            }
            finally { Object.Destroy(photoOnly); }
        }

        // the orientation the alignment test found, read by the other tests in this run (default: GL-style bottom-up RT)
        private static float FoundFlipV = 1f;

        [UnityTest]
        public IEnumerator ThreeGlassLevels_BlockEdges_MeasuredOnThePixels()
        {
            var frames = new List<Texture2D>();
            try
            {
                foreach (var level in new[] { "frosted", "tinted", "solid" })
                {
                    BuildSheet(level, shadows: true, blocks: true);
                    _frost.SetFloat("_FlipV", FoundFlipV);
                    for (int i = 0; i < 4; i++) yield return null;
                    Texture2D t = null;
                    yield return SpikeEvidence.Grab(x => t = x);
                    frames.Add(t);
                    SpikeEvidence.Save(t, "S5_glass_" + level);
                    EdgeContrast(t, out float minIn, out float medIn, out float minBorder, out float medBorder, out float groundSpread, out float groundDetail);
                    SpikeEvidence.Note("S5", $"{level}: block (white) vs ground just outside: min {minIn:0.00}:1, median {medIn:0.00}:1; " +
                        $"1 px border vs ground: min {minBorder:0.00}:1, median {medBorder:0.00}:1; ground luminance spread (p95-p5) {groundSpread:0.000}, ground detail (neighbour diff) {groundDetail:0.0000}");
                }
                var sheet = SpikeEvidence.SideBySide(frames);
                SpikeEvidence.Save(sheet, "S5_glass_frosted_tinted_solid");
                Object.Destroy(sheet);
            }
            finally { foreach (var f in frames) Object.Destroy(f); }
        }

        [UnityTest]
        public IEnumerator Cost_FrostedTintedSolid()
        {
            var results = new Dictionary<string, CostSample>();
            foreach (var level in new[] { "solid", "tinted", "frosted" })
            {
                BuildSheet(level, shadows: false, blocks: true);
                _rig.LowResCamera.enabled = level == "frosted";        // the copy camera only runs for the frosted level
                CostSample c = default;
                yield return SpikeCost.Measure(CostFrames, x => c = x);
                results[level] = c;
                SpikeEvidence.Note("S5", $"cost, {level} sheet + 3 blocks{(level == "frosted" ? $" + 1/{Downscale} copy camera" : "")}: {c}");
            }
            Assert.Greater(results["frosted"].Batches, results["solid"].Batches, "the frosted level costs at least the copy camera's draw");
        }

        // The sheet: a holder whose FIRST child is the ground layer (frost material / tint / solid) and whose blocks sit on top as
        // siblings -- a material on a parent would draw the blocks too (S4)
        private void BuildSheet(string level, bool shadows, bool blocks)
        {
            _stage.Root.Clear();
            _blocks.Clear();
            var sheet = new VisualElement { name = "sheet" };
            sheet.style.position = Position.Absolute;
            sheet.style.left = 0; sheet.style.right = 0; sheet.style.top = SheetTop; sheet.style.bottom = 0;
            sheet.style.overflow = Overflow.Hidden;
            sheet.style.borderTopLeftRadius = 24; sheet.style.borderTopRightRadius = 24;
            _stage.Root.Add(sheet);

            _groundLayer = new VisualElement { name = "ground", pickingMode = PickingMode.Ignore };
            _groundLayer.style.position = Position.Absolute;
            _groundLayer.style.left = 0; _groundLayer.style.right = 0; _groundLayer.style.top = 0; _groundLayer.style.bottom = 0;
            _groundLayer.style.borderTopLeftRadius = 24; _groundLayer.style.borderTopRightRadius = 24;
            switch (level)
            {
                case "frosted":
                    _groundLayer.style.backgroundColor = Color.white;
                    _groundLayer.style.unityMaterial = new StyleMaterialDefinition(_frost);
                    _frost.SetFloat("_RectX", 0f); _frost.SetFloat("_RectY", SheetTop / 844f);
                    _frost.SetFloat("_RectW", 1f); _frost.SetFloat("_RectH", (844f - SheetTop) / 844f);
                    _frost.SetFloat("_TintR", Ground.r); _frost.SetFloat("_TintG", Ground.g); _frost.SetFloat("_TintB", Ground.b);
                    _frost.SetFloat("_TintA", 0.55f);
                    break;
                case "tinted": _groundLayer.style.backgroundColor = TintFill; break;
                default: _groundLayer.style.backgroundColor = Ground; break;
            }
            sheet.Add(_groundLayer);
            if (!blocks) return;

            var column = new VisualElement { name = "column" };
            column.style.paddingLeft = 16; column.style.paddingRight = 16; column.style.paddingTop = 20;
            sheet.Add(column);
            for (int i = 0; i < 3; i++)
            {
                var slot = new VisualElement();
                slot.style.height = 130; slot.style.marginBottom = 16;
                if (shadows)
                {
                    var shadow = new VisualElement { pickingMode = PickingMode.Ignore };
                    shadow.style.position = Position.Absolute;
                    shadow.style.left = 6; shadow.style.right = 6; shadow.style.top = 10; shadow.style.bottom = -6;
                    shadow.style.backgroundColor = new Color(0f, 0f, 0f, 0.28f);
                    Radius(shadow, 16);
                    shadow.style.filter = new List<FilterFunction> { Blur(8f) };
                    slot.Add(shadow);
                }
                var block = new VisualElement();
                block.style.position = Position.Absolute;
                block.style.left = 0; block.style.right = 0; block.style.top = 0; block.style.bottom = 0;
                block.style.backgroundColor = Color.white;
                Radius(block, 16);
                block.style.borderTopWidth = 1; block.style.borderBottomWidth = 1; block.style.borderLeftWidth = 1; block.style.borderRightWidth = 1;
                block.style.borderTopColor = BlockBorder; block.style.borderBottomColor = BlockBorder;
                block.style.borderLeftColor = BlockBorder; block.style.borderRightColor = BlockBorder;
                var l = new Label("A block over the camera -- " + level);
                l.style.color = (Color)new Color32(24, 22, 20, 255);
                l.style.fontSize = 17; l.style.marginLeft = 16; l.style.marginTop = 52;
                block.Add(l);
                slot.Add(block);
                column.Add(slot);
                _blocks.Add(block);
            }
        }

        // Contrast of each block's top edge against the ground 5 px above it (the block's white 4 px inside, and its 1 px border row),
        // sampled every 6 px; plus how busy the ground is (luminance p95 - p5 over the gaps between blocks)
        private void EdgeContrast(Texture2D tex, out float minIn, out float medIn, out float minBorder, out float medBorder, out float groundSpread, out float groundDetail)
        {
            var inside = new List<float>(); var border = new List<float>(); var ground = new List<float>();
            double detail = 0; int nd = 0;
            foreach (var b in _blocks)
            {
                var r = b.worldBound;
                for (float x = r.xMin + 24; x < r.xMax - 24; x += 6)
                {
                    Color g = SpikeEvidence.PixelAt(_stage.Root, new Vector2(x, r.yMin - 5), tex);
                    inside.Add(UIAccessibility.ContrastRatio(SpikeEvidence.PixelAt(_stage.Root, new Vector2(x, r.yMin + 4), tex), g));
                    border.Add(UIAccessibility.ContrastRatio(SpikeEvidence.PixelAt(_stage.Root, new Vector2(x, r.yMin + 0.5f), tex), g));
                    ground.Add(g.grayscale);
                    detail += Mathf.Abs(g.grayscale - SpikeEvidence.PixelAt(_stage.Root, new Vector2(x + 1, r.yMin - 5), tex).grayscale);
                    nd++;
                }
            }
            inside.Sort(); border.Sort(); ground.Sort();
            minIn = inside[0]; medIn = inside[inside.Count / 2];
            minBorder = border[0]; medBorder = border[border.Count / 2];
            groundSpread = ground[(int)(ground.Count * 0.95f)] - ground[(int)(ground.Count * 0.05f)];
            groundDetail = (float)(detail / nd);   // high-frequency busyness: what actually eats a thin edge
        }

        // Mean absolute difference of 24 px cell averages inside `area` between two frames (0 = the same picture region by region)
        private float MeanCellDifference(Texture2D a, Texture2D b, Rect area)
        {
            const float Cell = 48f;   // wider than the frost tent (5 low-res texels = 40 px): the blur barely moves a cell mean, a misplaced lookup moves it a lot
            double sum = 0; int n = 0;
            for (float y = area.yMin + 8; y + Cell < area.yMax - 8; y += Cell)
                for (float x = area.xMin + 8; x + Cell < area.xMax - 8; x += Cell)
                {
                    Color ma = Color.clear, mb = Color.clear; int k = 0;
                    for (float yy = y; yy < y + Cell; yy += 4)
                        for (float xx = x; xx < x + Cell; xx += 4)
                        {
                            ma += SpikeEvidence.PixelAt(_stage.Root, new Vector2(xx, yy), a);
                            mb += SpikeEvidence.PixelAt(_stage.Root, new Vector2(xx, yy), b);
                            k++;
                        }
                    ma /= k; mb /= k;
                    sum += (Mathf.Abs(ma.r - mb.r) + Mathf.Abs(ma.g - mb.g) + Mathf.Abs(ma.b - mb.b)) / 3f;
                    n++;
                }
            return (float)(sum / n);
        }

        // The average colour of a frame inside `area` (a global brightness / colour-space shift shows here, not in the cell detail)
        private Color MeanColour(Texture2D tex, Rect area)
        {
            Color sum = Color.clear; int n = 0;
            for (float y = area.yMin + 8; y < area.yMax - 8; y += 8)
                for (float x = area.xMin + 8; x < area.xMax - 8; x += 8) { sum += SpikeEvidence.PixelAt(_stage.Root, new Vector2(x, y), tex); n++; }
            return sum / n;
        }

        private static FilterFunction Blur(float px)
        {
            var f = new FilterFunction(FilterFunctionType.Blur);
            f.AddParameter(new FilterParameter(px));
            return f;
        }

        private static void Radius(VisualElement e, float r)
        {
            e.style.borderTopLeftRadius = r; e.style.borderTopRightRadius = r;
            e.style.borderBottomLeftRadius = r; e.style.borderBottomRightRadius = r;
        }
    }
}
