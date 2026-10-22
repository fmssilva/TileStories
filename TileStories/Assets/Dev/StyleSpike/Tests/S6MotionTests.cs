using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

namespace TileStories.Dev.StyleSpike.Tests
{
    // DEV-ONLY (_3.3 P0 S6): motion on 6.3 -- a stack of 30 mock blocks; a reveal (translate + opacity) by USS transitions AND by
    // experimental.animation; which easings each accepts (cubic-bezier?); UI cost with 5 / 15 / 30 reveals running at once; and frame
    // sequences (a frame about every 50 ms over 600 ms) of one reveal and of a preview growing into a page, for the developer to judge.
    public class S6MotionTests
    {
        private const string Uss = "Assets/Dev/StyleSpike/S6Motion/Motion.uss";
        private const string BezierUss = "Assets/Dev/StyleSpike/S6Motion/BezierRule.uss";
        private const int MaxSettleFrames = 120;
        private const int Blocks = 30;
        private const int RevealMs = 450;
        private const float FrameStepMs = 50f, SequenceMs = 600f;
        private const int CostFrames = 90;

        private SpikeStage _stage;
        private readonly List<VisualElement> _blocks = new List<VisualElement>();

        [UnitySetUp]
        public IEnumerator Build()
        {
            var panel = AssetDatabase.LoadAssetAtPath<PanelSettings>(SpikeStageTests.PanelSettingsPath);
            var sheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(Uss);
            Assert.IsNotNull(sheet, "Motion.uss missing -- SETUP");
            _stage = new SpikeStage(panel, Color.black);
            _stage.AddSheet(sheet);
            _stage.Root.style.backgroundColor = (Color)new Color32(243, 240, 233, 255);
            _stage.Root.style.paddingTop = 16;
            _blocks.Clear();
            for (int i = 0; i < Blocks; i++)
            {
                var b = new VisualElement();
                b.AddToClassList("s6-block");
                var l = new Label("Block " + (i + 1));
                l.AddToClassList("s6-label");
                b.Add(l);
                _stage.Root.Add(b);
                _blocks.Add(b);
            }
            for (int i = 0; i < 4; i++) yield return null;
        }

        [TearDown]
        public void TearDown() => _stage?.Dispose();

        [UnityTest]
        public IEnumerator Easings_NamedCurvesOnly_InUss_ACustomCurveInExperimentalAnimation()
        {
            var plain = _blocks[0];
            var bezier = _blocks[1];
            _stage.AddSheet(AssetDatabase.LoadAssetAtPath<StyleSheet>(BezierUss));
            LogAssert.Expect(LogType.Exception, new System.Text.RegularExpressions.Regex("ArgumentOutOfRangeException"));
            bezier.AddToClassList("s6-bezier");
            yield return null;
            var plainCurve = plain.resolvedStyle.transitionTimingFunction.First().mode;
            var bezierCurve = bezier.resolvedStyle.transitionTimingFunction.First().mode;
            SpikeEvidence.Note("S6", $"USS easings accepted: {string.Join(" ", System.Enum.GetNames(typeof(EasingMode)))}; " +
                $"ease-out-cubic -> {plainCurve}; a cubic-bezier(0.16, 1, 0.3, 1) rule -> {bezierCurve} and an ArgumentOutOfRangeException in the style update");
            Assert.AreEqual(EasingMode.EaseOutCubic, plainCurve, "a named curve resolves");
            Assert.AreEqual(EasingMode.EaseOutCubic, bezierCurve, "a cubic-bezier() rule is dropped: the element keeps the curve of its other class (and the style update throws, expected above)");

            // the Stitch curve through experimental.animation: compare its half-time value with the named neighbours
            float stitch = SpikeCubicBezier.Evaluate(0.16f, 1f, 0.3f, 1f, 0.25f);
            float outCubic = Easing.OutCubic(0.25f), outQuint = 1f - Mathf.Pow(0.75f, 5f), outExpo = 1f - Mathf.Pow(2f, -10f * 0.25f);
            SpikeEvidence.Note("S6", $"progress at 25 % of the time: Stitch bezier {stitch:0.000}, EaseOutCubic {outCubic:0.000}, quint {outQuint:0.000}, expo {outExpo:0.000}");
        }

        [UnityTest]
        public IEnumerator Reveal_ByUssTransition_FrameSequence()
        {
            var block = _blocks[2];
            block.AddToClassList("s6-no-transition");               // the start state must not animate itself
            block.AddToClassList("s6-block--hidden");
            for (int i = 0; i < MaxSettleFrames && block.resolvedStyle.opacity > 0f; i++) yield return null;
            Assert.AreEqual(0f, block.resolvedStyle.opacity, 0.001f, "starts hidden -- SETUP");
            block.RemoveFromClassList("s6-no-transition");
            yield return null;
            var crop = new Rect(0, block.worldBound.yMin - 30, 390, 130);
            yield return Sequence("S6_reveal_uss", crop, () => block.RemoveFromClassList("s6-block--hidden"),
                () => $"opacity {block.resolvedStyle.opacity:0.00}, y {block.resolvedStyle.translate.y:0.0}");
            Assert.AreEqual(1f, block.resolvedStyle.opacity, 0.001f, "ends fully shown");
        }

        [UnityTest]
        public IEnumerator Reveal_ByExperimentalAnimation_WithTheStitchCurve_FrameSequence()
        {
            var block = _blocks[2];
            block.AddToClassList("s6-no-transition");               // measured: a USS transition would smooth every inline write again
            block.style.opacity = 0f;
            block.style.translate = new Translate(0, 16);
            for (int i = 0; i < 4; i++) yield return null;
            Assert.AreEqual(0f, block.resolvedStyle.opacity, 0.001f, "starts hidden -- SETUP");
            var crop = new Rect(0, block.worldBound.yMin - 30, 390, 130);
            yield return Sequence("S6_reveal_experimental_bezier", crop,
                () => block.experimental.animation.Start(0f, 1f, RevealMs, (e, v) =>
                {
                    e.style.opacity = v;
                    e.style.translate = new Translate(0, 16f * (1f - v));
                }).Ease(SpikeCubicBezier.Curve(0.16f, 1f, 0.3f, 1f)),
                () => $"opacity {block.resolvedStyle.opacity:0.00}, y {block.resolvedStyle.translate.y:0.0}");
            Assert.AreEqual(1f, block.resolvedStyle.opacity, 0.001f, "ends fully shown");
        }

        [UnityTest]
        public IEnumerator PreviewGrowsIntoAPage_FrameSequence()
        {
            var from = _blocks[3].worldBound;
            var page = new VisualElement();
            page.AddToClassList("s6-page");
            page.AddToClassList("s6-no-transition");                // the START (the preview's rect) is applied without animating
            var title = new Label("Block 4 -- the whole story");
            title.AddToClassList("s6-label");
            page.Add(title);
            page.style.scale = new Scale(new Vector3(from.width / 390f, from.height / 844f, 1f));
            page.style.translate = new Translate(from.xMin, from.yMin);
            page.style.borderTopLeftRadius = 12; page.style.borderTopRightRadius = 12;
            _stage.Root.Add(page);
            for (int i = 0; i < 4; i++) yield return null;
            Assert.AreEqual(from.width, page.worldBound.width, 0.5f, "starts at the preview's on-screen width -- SETUP");
            Assert.AreEqual(from.yMin, page.worldBound.yMin, 0.5f, "starts at the preview's on-screen top -- SETUP");
            page.RemoveFromClassList("s6-no-transition");
            yield return null;
            yield return Sequence("S6_preview_grows_into_page", new Rect(0, 0, 390, 844), () =>
            {
                page.style.scale = new Scale(Vector3.one);
                page.style.translate = new Translate(0, 0);
                page.style.borderTopLeftRadius = 0; page.style.borderTopRightRadius = 0;
            }, () => $"resolved scale {page.resolvedStyle.scale.value.x:0.000}, on screen x {page.worldBound.xMin:0} y {page.worldBound.yMin:0} w {page.worldBound.width:0} h {page.worldBound.height:0}", scale: 0.4f);
            Assert.AreEqual(390f, page.worldBound.width, 0.5f, "the page ends at full width (its real on-screen rect)");
            Assert.AreEqual(0f, page.worldBound.yMin, 0.5f, "the page ends at the top");
        }

        [UnityTest]
        public IEnumerator Cost_5_15_30_RevealsAtOnce_UssAndExperimental()
        {
            CostSample idle = default;
            yield return SpikeCost.Measure(CostFrames, x => idle = x);
            SpikeEvidence.Note("S6", "cost, 30 blocks idle: " + idle);
            foreach (int n in new[] { 5, 15, 30 })
            {
                // USS: flip the hidden class every reveal so N transitions are always running
                var uss = _stage.Root.schedule.Execute(() => { for (int i = 0; i < n; i++) _blocks[i].ToggleInClassList("s6-block--hidden"); }).Every(RevealMs);
                CostSample a = default;
                yield return SpikeCost.Measure(CostFrames, x => a = x);
                uss.Pause();
                foreach (var b in _blocks) b.RemoveFromClassList("s6-block--hidden");
                for (int i = 0; i < 30; i++) yield return null;

                // experimental.animation: N animations restarted every reveal (no USS transition under them: no double smoothing)
                for (int i = 0; i < n; i++) _blocks[i].AddToClassList("s6-no-transition");
                var exp =_stage.Root.schedule.Execute(() =>
                {
                    for (int i = 0; i < n; i++)
                        _blocks[i].experimental.animation.Start(0f, 1f, RevealMs, (e, v) => { e.style.opacity = v; e.style.translate = new Translate(0, 16f * (1f - v)); });
                }).Every(RevealMs);
                CostSample b2 = default;
                yield return SpikeCost.Measure(CostFrames, x => b2 = x);
                exp.Pause();
                foreach (var b in _blocks) { b.RemoveFromClassList("s6-no-transition"); b.style.opacity = StyleKeyword.Null; b.style.translate = StyleKeyword.Null; }
                SpikeEvidence.Note("S6", $"cost, {n} reveals at once -- USS transitions: {a}");
                SpikeEvidence.Note("S6", $"cost, {n} reveals at once -- experimental.animation: {b2}");
                for (int i = 0; i < 30; i++) yield return null;
            }
        }

        // Start a motion, then keep the frame nearest each 50 ms mark for 600 ms (stamped with the REAL elapsed ms and the element's
        // state), and paste them side by side
        private IEnumerator Sequence(string name, Rect crop, System.Action start, System.Func<string> state, float scale = 1f)
        {
            var frames = new List<Texture2D>();
            var log = new List<string>();
            start();
            float t0 = Time.realtimeSinceStartup, next = 0f;
            try
            {
                while (true)
                {
                    float ms = (Time.realtimeSinceStartup - t0) * 1000f;
                    if (ms >= next)
                    {
                        Texture2D tex = null;
                        yield return SpikeEvidence.Grab(t => tex = t);
                        var cut = SpikeEvidence.Crop(_stage.Root, crop, tex);
                        Object.Destroy(tex);
                        frames.Add(scale < 1f ? Shrink(cut, scale) : cut);
                        log.Add($"{ms:0}ms {state()}");
                        next += FrameStepMs;
                        if (ms > SequenceMs) break;
                    }
                    else yield return null;
                }
                var strip = SpikeEvidence.SideBySide(frames, 4);
                SpikeEvidence.Save(strip, name);
                Object.Destroy(strip);
                SpikeEvidence.Note("S6", name + ": " + string.Join(" | ", log));
            }
            finally { foreach (var f in frames) Object.Destroy(f); }
        }

        private static Texture2D Shrink(Texture2D tex, float factor)
        {
            var small = SpikeEvidence.Scaled(tex, factor);
            Object.Destroy(tex);
            return small;
        }
    }
}
