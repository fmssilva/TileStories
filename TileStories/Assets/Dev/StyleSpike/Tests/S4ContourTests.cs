using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Dev.StyleSpike.Tests
{
    // DEV-ONLY (_3.3 P0 S4): ONE UI Shader Graph material (SpikeContour.shadergraph, URP UI target) -- the Stitch `contourRotate`:
    // an animated gradient ring around a pill call to action, 6 s per turn, driven by time, on the card's PanelSettings. Measured:
    // does it draw and turn, does the reduce-motion property (_Animate 0) freeze it, and what it does to batching.
    public class S4ContourTests
    {
        private const string MaterialPath = "Assets/Dev/StyleSpike/S4Contour/SpikeContour.mat";
        private const float Ring = 3f;
        private const float PillW = 280f, PillH = 56f, PillTop = 700f;
        private const int CostFrames = 60;

        private SpikeStage _stage;
        private Material _material;
        private VisualElement _contour, _holder;
        private Button _button;

        [UnitySetUp]
        public IEnumerator Build()
        {
            var panel = AssetDatabase.LoadAssetAtPath<PanelSettings>(SpikeStageTests.PanelSettingsPath);
            var asset = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            Assert.IsNotNull(asset, "SpikeContour.mat missing -- SETUP");
            Assert.IsTrue(asset.shader.isSupported, "the contour shader does not compile here -- SETUP");
            _material = new Material(asset);
            _stage = new SpikeStage(panel, Color.black);
            _stage.Root.style.backgroundColor = (Color)new Color32(243, 240, 233, 255);
            for (int i = 0; i < 10; i++) _stage.Root.Add(Block(i));     // ordinary blocks: what the batching must survive
            BuildPill(withMaterial: true);
            for (int i = 0; i < 4; i++) yield return null;
        }

        [TearDown]
        public void TearDown()
        {
            _stage?.Dispose();
            if (_material != null) Object.Destroy(_material);
        }

        [UnityTest]
        public IEnumerator TheRing_DrawsAndTurns_AndAnimateZeroFreezesIt()
        {
            Assert.AreEqual(_material, _contour.resolvedStyle.unityMaterial.material, "the material reached the element");
            var probe = new Vector2(_contour.worldBound.center.x, _contour.worldBound.yMin + Ring * 0.5f);   // top edge, inside the ring

            Color a = default, b = default, c = default, d = default;
            yield return Sample(probe, x => a = x);
            yield return WaitSeconds(1.5f);                       // a quarter turn of a 6 s loop
            yield return Sample(probe, x => b = x);
            yield return SpikeEvidence.Capture("S4_contour_pill");

            _material.SetFloat("_Animate", 0f);
            yield return Sample(probe, x => c = x);
            yield return WaitSeconds(1.5f);
            yield return Sample(probe, x => d = x);
            SpikeEvidence.Note("S4", $"ring top pixel: animate 1 -> {(Color32)a} then {(Color32)b}; animate 0 -> {(Color32)c} then {(Color32)d}");

            Assert.Greater(Diff(a, b), 0.05f, "with _Animate 1 the ring colour under one point changes as it turns");
            Assert.Less(Diff(c, d), 0.01f, "with _Animate 0 (reduce motion) the ring stands still");
            Assert.Greater(Diff(a, new Color32(243, 240, 233, 255)), 0.1f, "the ring is drawn (not the ground colour)");

            // the button on top keeps its own fill: the material stays on its sibling layer
            Color fill = default;
            yield return Sample(new Vector2(_button.worldBound.xMin + 20, _button.worldBound.center.y), x => fill = x);
            SpikeEvidence.Note("S4", $"button fill beside its text: {(Color32)fill}");
            Assert.Less(Diff(fill, new Color32(24, 22, 20, 255)), 0.06f, "the button keeps its dark fill (the material did not leak into it)");
        }

        [UnityTest]
        public IEnumerator Cost_PlainPill_VersusMaterialPill()
        {
            CostSample withMat = default, plain = default;
            yield return SpikeCost.Measure(CostFrames, x => withMat = x);
            BuildPill(withMaterial: false);
            yield return SpikeCost.Measure(CostFrames, x => plain = x);
            SpikeEvidence.Note("S4", "cost, 10 blocks + plain pill: " + plain);
            SpikeEvidence.Note("S4", "cost, 10 blocks + material contour pill: " + withMat);
            Assert.GreaterOrEqual(withMat.Batches, plain.Batches, "a custom material cannot batch with the default UI shader");
        }

        // (Re)build the pill: a plain holder with TWO siblings -- the ring layer (the material) and the button inset by the ring
        // width on top. Measured: a material on a PARENT also draws the children (the button's fill and its text vanished into the
        // gradient), so the ring must be a sibling behind the content, never its parent.
        private void BuildPill(bool withMaterial)
        {
            _holder?.RemoveFromHierarchy();
            _holder = new VisualElement { name = "pill-holder" };
            var h = _holder.style;
            h.position = Position.Absolute;
            h.left = (390 - PillW) / 2 - Ring; h.top = PillTop - Ring;
            h.width = PillW + 2 * Ring; h.height = PillH + 2 * Ring;

            _contour = new VisualElement { name = "contour", pickingMode = PickingMode.Ignore };
            var s = _contour.style;
            s.position = Position.Absolute;
            s.left = 0; s.top = 0; s.right = 0; s.bottom = 0;
            SetRadius(_contour, (PillH + 2 * Ring) / 2);
            s.backgroundColor = withMaterial ? Color.white : (Color)new Color32(201, 150, 60, 255);
            if (withMaterial) s.unityMaterial = new StyleMaterialDefinition(_material);
            _holder.Add(_contour);

            var button = new Button { text = "See it here in AR" };
            var b = button.style;
            b.position = Position.Absolute;
            b.left = Ring; b.top = Ring; b.width = PillW; b.height = PillH;
            b.marginLeft = 0; b.marginTop = 0; b.marginRight = 0; b.marginBottom = 0;
            b.borderTopWidth = 0; b.borderBottomWidth = 0; b.borderLeftWidth = 0; b.borderRightWidth = 0;
            SetRadius(button, PillH / 2);
            b.backgroundColor = (Color)new Color32(24, 22, 20, 255);
            b.color = Color.white;
            b.fontSize = 17;
            _button = button;
            _holder.Add(button);
            _stage.Root.Add(_holder);
        }

        private static VisualElement Block(int i)
        {
            var e = new VisualElement();
            e.style.marginLeft = 16; e.style.marginRight = 16; e.style.marginTop = 8;
            e.style.height = 52;
            e.style.backgroundColor = Color.white;
            SetRadius(e, 12);
            var l = new Label("Block " + (i + 1) + " -- a line of text");
            l.style.color = (Color)new Color32(24, 22, 20, 255);
            l.style.fontSize = 16; l.style.marginLeft = 12; l.style.marginTop = 14;
            e.Add(l);
            return e;
        }

        private static void SetRadius(VisualElement e, float r)
        {
            e.style.borderTopLeftRadius = r; e.style.borderTopRightRadius = r;
            e.style.borderBottomLeftRadius = r; e.style.borderBottomRightRadius = r;
        }

        private IEnumerator Sample(Vector2 panelPoint, System.Action<Color> got)
        {
            Texture2D tex = null;
            yield return SpikeEvidence.Grab(t => tex = t);
            try { got(SpikeEvidence.PixelAt(_stage.Root, panelPoint, tex)); }
            finally { Object.Destroy(tex); }
        }

        private static IEnumerator WaitSeconds(float s)
        {
            float start = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - start < s) yield return null;
        }

        private static float Diff(Color a, Color b) => Mathf.Abs(a.r - b.r) + Mathf.Abs(a.g - b.g) + Mathf.Abs(a.b - b.b);
    }
}
