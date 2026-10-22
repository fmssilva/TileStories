using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Dev.StyleSpike.Tests
{
    // DEV-ONLY (_3.3 P0 S7): elevation on 6.3 without drop-shadow() -- a 9-slice soft shadow texture vs a UI Shader Graph shadow
    // (rounded-box SDF, SpikeShadow.shadergraph) vs a border only, on light AND dark grounds. Captures side by side, how deep each
    // shadow measures under the block, and the cost.
    public class S7ShadowTests
    {
        private const string MaterialPath = "Assets/Dev/StyleSpike/S7Shadow/SpikeShadow.mat";
        private const float BlockW = 300f, BlockH = 150f, Pad = 24f, Radius = 16f, Softness = 14f, OffsetY = 6f, Opacity = 0.22f;
        private const int CostFrames = 60;

        private struct Mode { public string Name; public Color Ground, Block, Border; }
        private static readonly Mode Light = new Mode { Name = "light", Ground = new Color32(243, 240, 233, 255), Block = Color.white, Border = new Color32(214, 208, 197, 255) };
        private static readonly Mode Dark = new Mode { Name = "dark", Ground = new Color32(18, 16, 14, 255), Block = new Color32(36, 32, 28, 255), Border = new Color32(64, 58, 52, 255) };

        private SpikeStage _stage;
        private Material _material;
        private Texture2D _sliceTex;
        private int _slice;
        private readonly List<VisualElement> _blocks = new List<VisualElement>();

        [UnitySetUp]
        public IEnumerator Build()
        {
            var panel = AssetDatabase.LoadAssetAtPath<PanelSettings>(SpikeStageTests.PanelSettingsPath);
            var asset = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            Assert.IsNotNull(asset, "SpikeShadow.mat missing -- SETUP");
            _material = new Material(asset);
            _material.SetFloat("_Width", BlockW + 2 * Pad); _material.SetFloat("_Height", BlockH + 2 * Pad);
            _material.SetFloat("_Pad", Pad); _material.SetFloat("_Radius", Radius); _material.SetFloat("_Softness", Softness);
            _material.SetFloat("_OffsetY", OffsetY); _material.SetFloat("_Opacity", Opacity);
            _sliceTex = SpikeShadowSprite.Make((int)Pad, (int)Radius, Softness, Opacity, out _slice);
            _stage = new SpikeStage(panel, Color.black);
            yield return null;
        }

        [TearDown]
        public void TearDown()
        {
            _stage?.Dispose();
            if (_material != null) Object.Destroy(_material);
            if (_sliceTex != null) Object.Destroy(_sliceTex);
        }

        [UnityTest]
        public IEnumerator ThreeElevations_LightAndDark_RenderedAndMeasured()
        {
            var frames = new List<Texture2D>();
            try
            {
                foreach (var mode in new[] { Light, Dark })
                {
                    BuildColumn(mode, new[] { "border", "nineslice", "shadergraph" });
                    for (int i = 0; i < 4; i++) yield return null;
                    Texture2D t = null;
                    yield return SpikeEvidence.Grab(x => t = x);
                    frames.Add(t);
                    SpikeEvidence.Save(t, "S7_shadows_" + mode.Name);
                    string[] names = { "border only", "9-slice", "shader graph" };
                    for (int i = 0; i < 3; i++)
                    {
                        var r = _blocks[i].worldBound;
                        Color ground = SpikeEvidence.PixelAt(_stage.Root, new Vector2(r.xMax + Pad + 12, r.center.y), t);   // far right: untouched ground
                        Color under = SpikeEvidence.PixelAt(_stage.Root, new Vector2(r.center.x, r.yMax + 4), t);          // just under the block
                        Color edge = SpikeEvidence.PixelAt(_stage.Root, new Vector2(r.center.x, r.yMax - 0.5f), t);         // the block's bottom border row
                        SpikeEvidence.Note("S7", $"{mode.Name} {names[i]}: ground {(Color32)ground}, 4 px under the block {(Color32)under} " +
                            $"(shadow depth {(ground.grayscale - under.grayscale) * 255f:0} of 255, {UIAccessibility.ContrastRatio(under, ground):0.00}:1); " +
                            $"bottom edge vs just under {UIAccessibility.ContrastRatio(edge, under):0.00}:1");
                        if (i > 0) Assert.Greater(ground.grayscale - under.grayscale, 0.005f, $"{mode.Name} {names[i]}: a shadow is drawn under the block");
                    }
                }
                var sheet = SpikeEvidence.SideBySide(frames);
                SpikeEvidence.Save(sheet, "S7_shadows_light_dark");
                Object.Destroy(sheet);
            }
            finally { foreach (var f in frames) Object.Destroy(f); }
        }

        [UnityTest]
        public IEnumerator Cost_FiveBlocks_PerTechnique()
        {
            foreach (var tech in new[] { "border", "nineslice", "shadergraph" })
            {
                BuildColumn(Light, new[] { tech, tech, tech, tech, tech }, compact: true);
                CostSample c = default;
                yield return SpikeCost.Measure(CostFrames, x => c = x);
                SpikeEvidence.Note("S7", $"cost, 5 blocks with {tech}: {c}");
            }
        }

        // One column of blocks on a ground, each with its own elevation technique (the shadow is a SIBLING behind the block)
        private void BuildColumn(Mode mode, string[] techniques, bool compact = false)
        {
            _stage.Root.Clear();
            _blocks.Clear();
            _stage.Root.style.backgroundColor = mode.Ground;
            float step = compact ? BlockH * 0.9f + 12 : BlockH + 2 * Pad + 30;
            for (int i = 0; i < techniques.Length; i++)
            {
                float top = 30 + i * step, left = (390 - BlockW) / 2;
                float h = compact ? BlockH * 0.9f : BlockH;
                switch (techniques[i])
                {
                    case "nineslice":
                        var s9 = Absolute(left - Pad, top - Pad + OffsetY, BlockW + 2 * Pad, h + 2 * Pad);
                        s9.style.backgroundImage = Background.FromTexture2D(_sliceTex);
                        s9.style.unitySliceLeft = _slice; s9.style.unitySliceRight = _slice;
                        s9.style.unitySliceTop = _slice; s9.style.unitySliceBottom = _slice;
                        _stage.Root.Add(s9);
                        break;
                    case "shadergraph":
                        var sg = Absolute(left - Pad, top - Pad, BlockW + 2 * Pad, h + 2 * Pad);
                        sg.style.backgroundColor = Color.white;
                        sg.style.unityMaterial = new StyleMaterialDefinition(_material);
                        _stage.Root.Add(sg);
                        break;
                }
                var block = Absolute(left, top, BlockW, h);
                block.style.backgroundColor = mode.Block;
                foreach (var set in new System.Action<float>[] { r => block.style.borderTopLeftRadius = r, r => block.style.borderTopRightRadius = r,
                                                                 r => block.style.borderBottomLeftRadius = r, r => block.style.borderBottomRightRadius = r }) set(Radius);
                block.style.borderTopWidth = 1; block.style.borderBottomWidth = 1; block.style.borderLeftWidth = 1; block.style.borderRightWidth = 1;
                block.style.borderTopColor = mode.Border; block.style.borderBottomColor = mode.Border;
                block.style.borderLeftColor = mode.Border; block.style.borderRightColor = mode.Border;
                var l = new Label(techniques[i] == "border" ? "Border only" : techniques[i] == "nineslice" ? "9-slice soft shadow" : "Shader Graph shadow");
                l.style.color = mode.Name == "light" ? (Color)new Color32(24, 22, 20, 255) : (Color)new Color32(240, 236, 228, 255);
                l.style.fontSize = 17; l.style.marginLeft = 16; l.style.marginTop = 16;
                block.Add(l);
                _stage.Root.Add(block);
                _blocks.Add(block);
            }
        }

        private static VisualElement Absolute(float left, float top, float width, float height)
        {
            var e = new VisualElement { pickingMode = PickingMode.Ignore };
            e.style.position = Position.Absolute;
            e.style.left = left; e.style.top = top; e.style.width = width; e.style.height = height;
            return e;
        }
    }
}
