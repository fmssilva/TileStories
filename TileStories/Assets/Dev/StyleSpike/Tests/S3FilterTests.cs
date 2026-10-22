using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Dev.StyleSpike.Tests
{
    // DEV-ONLY (_3.3 P0 S3): USS `filter:` on 6000.3 -- what blur / tint / grayscale look like on a picture, what they cost (UI CPU
    // markers, batches, GPU frame), and whether `transition` animates a filter.
    public class S3FilterTests
    {
        private const string Uss = "Assets/Dev/StyleSpike/S3Filter/Filter.uss";
        private const int CostFrames = 60;

        private SpikeStage _stage;

        [UnitySetUp]
        public IEnumerator Build()
        {
            var panel = AssetDatabase.LoadAssetAtPath<PanelSettings>(SpikeStageTests.PanelSettingsPath);
            var sheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(Uss);
            Assert.IsNotNull(sheet, "Filter.uss missing -- SETUP");
            _stage = new SpikeStage(panel, Color.black);
            _stage.AddSheet(sheet);
            yield return null;
        }

        [TearDown]
        public void TearDown() => _stage?.Dispose();

        [UnityTest]
        public IEnumerator BlurTintGrayscale_OnAPicture_ParseAndRender()
        {
            var plain = Picture(null, "none");
            var blur = Picture("s3-blur-6", "blur(6px)");
            var tint = Picture("s3-tint", "tint(warm)");
            var grey = Picture("s3-grayscale", "grayscale(100%)");
            for (int i = 0; i < 4; i++) yield return null;

            Assert.AreEqual(0, plain.resolvedStyle.filter.Count(), "the plain picture carries no filter");
            Assert.AreEqual(FilterFunctionType.Blur, Only(blur), "blur(6px) parsed");
            Assert.AreEqual(FilterFunctionType.Tint, Only(tint), "tint(...) parsed");
            Assert.AreEqual(FilterFunctionType.Grayscale, Only(grey), "grayscale(100%) parsed");

            Texture2D tex = null;
            yield return SpikeEvidence.Grab(t => tex = t);
            try
            {
                SpikeEvidence.Save(tex, "S3_filter_none_blur_tint_grayscale");
                float sharp = Detail(plain, tex), soft = Detail(blur, tex);
                Color greyPx = SpikeEvidence.PixelAt(_stage.Root, grey.worldBound.center, tex);
                SpikeEvidence.Note("S3", $"detail (mean neighbour difference): none {sharp:0.0000}, blur(6px) {soft:0.0000}; grayscale centre {(Color32)greyPx}");
                Assert.Less(soft, sharp * 0.6f, "blur(6px) must visibly soften the picture (less neighbour detail)");
                Assert.AreEqual(greyPx.r, greyPx.g, 0.02f, "grayscale: r = g"); Assert.AreEqual(greyPx.g, greyPx.b, 0.02f, "grayscale: g = b");
            }
            finally { Object.Destroy(tex); }
        }

        [UnityTest]
        public IEnumerator Cost_OfBlur_ZeroOneTen_AndRadius()
        {
            CostSample none = default, one = default, ten = default, tenPlain = default, oneR2 = default, oneR16 = default;
            yield return Measure(10, null, c => tenPlain = c);
            yield return Measure(0, "s3-blur-6", c => none = c);
            yield return Measure(1, "s3-blur-6", c => one = c);
            yield return Measure(10, "s3-blur-6", c => ten = c);
            yield return Measure(1, "s3-blur-2", c => oneR2 = c);
            yield return Measure(1, "s3-blur-16", c => oneR16 = c);
            SpikeEvidence.Note("S3", "cost, no picture: " + none);
            SpikeEvidence.Note("S3", "cost, 10 plain pictures: " + tenPlain);
            SpikeEvidence.Note("S3", "cost, 1 blur(6px): " + one);
            SpikeEvidence.Note("S3", "cost, 10 blur(6px): " + ten);
            SpikeEvidence.Note("S3", "cost, 1 blur(2px): " + oneR2);
            SpikeEvidence.Note("S3", "cost, 1 blur(16px): " + oneR16);
            Assert.Greater(ten.Batches, tenPlain.Batches, "a filter must show up in the batch count (each filtered element is drawn on its own)");
        }

        [UnityTest]
        public IEnumerator Transition_AnimatesTheFilter()
        {
            var pic = Picture("s3-anim", "transition blur 0 -> 16 over 1 s");
            for (int i = 0; i < 4; i++) yield return null;
            Assert.AreEqual(0f, Radius(pic), 0.01f, "starts unblurred");

            pic.AddToClassList("s3-anim--on");
            float start = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - start < 0.5f) yield return null;
            float mid = Radius(pic);
            yield return SpikeEvidence.Capture("S3_filter_transition_mid");
            while (Time.realtimeSinceStartup - start < 1.3f) yield return null;
            float end = Radius(pic);
            SpikeEvidence.Note("S3", $"transition: blur radius at ~0.5 s = {mid:0.00} px, after 1.3 s = {end:0.00} px");
            Assert.Greater(mid, 2f, "half way the radius is between the ends (the filter is animated, not snapped)");
            Assert.Less(mid, 14f, "half way the radius is between the ends (the filter is animated, not snapped)");
            Assert.AreEqual(16f, end, 0.01f, "it ends at the target radius");
        }

        // One labelled picture row carrying one filter class (or none)
        private VisualElement Picture(string cls, string caption)
        {
            var p = new VisualElement();
            p.AddToClassList("s3-picture");
            if (cls != null) p.AddToClassList(cls);
            var l = new Label(caption);
            l.AddToClassList("s3-label");
            p.Add(l);
            _stage.Root.Add(p);
            return p;
        }

        // Rebuild the stage with N pictures carrying `cls` and measure them
        private IEnumerator Measure(int count, string cls, System.Action<CostSample> got)
        {
            _stage.Root.Clear();
            for (int i = 0; i < count; i++)
            {
                var p = new VisualElement();
                p.AddToClassList("s3-picture");
                if (cls != null) p.AddToClassList(cls);
                p.style.position = Position.Absolute;
                p.style.left = 0; p.style.right = 0; p.style.top = 60 * i;
                _stage.Root.Add(p);
            }
            yield return SpikeCost.Measure(CostFrames, got);
        }

        private static FilterFunctionType Only(VisualElement e)
        {
            Assert.AreEqual(1, e.resolvedStyle.filter.Count(), "exactly one filter function resolved");
            foreach (var f in e.resolvedStyle.filter) return f.type;
            return FilterFunctionType.None;
        }

        private static float Radius(VisualElement e)
        {
            foreach (var f in e.resolvedStyle.filter) return f.parameterCount > 0 ? f.GetParameter(0).floatValue : -1f;
            return 0f;
        }

        // Mean difference between horizontal neighbours inside the element: high for a sharp photo, low once blurred
        private float Detail(VisualElement e, Texture2D tex)
        {
            var r = e.worldBound;
            double sum = 0; int n = 0;
            for (float y = r.yMin + 40; y < r.yMax - 10; y += 3)
                for (float x = r.xMin + 10; x < r.xMax - 10; x += 2)
                {
                    sum += Mathf.Abs(SpikeEvidence.PixelAt(_stage.Root, new Vector2(x, y), tex).grayscale - SpikeEvidence.PixelAt(_stage.Root, new Vector2(x + 1, y), tex).grayscale);
                    n++;
                }
            return (float)(sum / n);
        }
    }
}
