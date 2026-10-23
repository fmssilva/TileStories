using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace TileStories.Tests
{
    // The measuring instrument of a change that must move NO pixel (_3.3 R12.7 steps 1-2: the theme pipeline, the token split): a fixed set
    // of renders covering the card's colours, compared pixel by pixel with baselines taken before the change (TestEvidence/ThemePixels/:
    // baseline/ holds them, latest/ the last run's renders). Before the change: create baseline/COLLECT and run the set several times
    // (Unity in front and in the background) to collect every state the unchanged tree draws; delete COLLECT; make the change; run the
    // set: every render must equal one of its baselines exactly (Judge). Delete the baseline folder to start over.
    // A measurement, not a suite member: it runs only while the local switch file TestEvidence/ThemePixels/MEASURING exists (create it
    // for the measurement, delete it after) and is ignored otherwise -- [Explicit] alone did NOT keep it out of an unfiltered MCP full run
    // (_3.3 P4.1), and a baseline left from before a deliberate visual change must never fail somebody's full run.
    // Text pixels depend on the Editor's dynamic font atlas, shared by every scene and filled in the order text is drawn since it was last
    // reset: twice, right after a re-import, ThemePipelineTests' Lamp card drawn first moved 1-10 text pixels of 7 gallery renders by 1-2
    // levels -- with the old token file exactly as with the new one (_3.3 P4.1, A/B). Run the set alone in its job.
    internal static class ThemePixelSet
    {
        private const string Domain = "ThemePixels";

        // Ignore the test unless the measuring switch is on
        public static void RequireMeasuring()
        {
            if (!File.Exists(Path.Combine(TestEvidence.Root, Domain, "MEASURING")))
                Assert.Ignore("A measurement: create TestEvidence/" + Domain + "/MEASURING to run it (ThemePixelSet)");
        }

        // Render the screen (or only `cropTo`'s rect) as `name`: the baseline when there is none yet, else compared with it.
        // A cropped element's two top corners are painted out (a square of its own resolved radius): its rounded corner lets the live
        // wall behind it through, which moves between runs (measured: 28 px at the sheet's top-right corner, nothing else)
        // - the picture is taken once it holds still: two grabs StableFrames apart identical. A wait in seconds is not enough -- an unfocused
        //   Editor runs fewer frames, and the real audio chip's time text settled a frame later there (1256 px of The Lamp's Half moved
        //   between a focused and an unfocused run, the same 1256 in both unfocused ones)
        public static IEnumerator Capture(string name, VisualElement cropTo = null)
        {
            Texture2D render = null;
            for (int attempt = 0; attempt < MaxAttempts; attempt++)
            {
                if (attempt > 0) for (int f = 0; f < StableFrames; f++) yield return null;
                yield return new WaitForEndOfFrame();
                var next = Grab(cropTo);
                bool still = render != null && Same(render, next);
                if (render != null) Object.Destroy(render);
                render = next;
                if (still) break;
                Assert.Less(attempt, MaxAttempts - 1, name + ": the picture never held still for " + StableFrames + " frames");
            }
            try { Judge(name, render); }
            finally { Object.Destroy(render); }
        }

        private const int StableFrames = 10;
        private const int MaxAttempts = 30;

        // The frame on screen now (call at the end of a frame), cropped to `cropTo` with its top corners painted out
        private static Texture2D Grab(VisualElement cropTo)
        {
            var shot = ScreenCapture.CaptureScreenshotAsTexture();
            try
            {
                var area = cropTo == null ? new RectInt(0, 0, shot.width, shot.height) : ScreenRectOf(cropTo, shot);
                Assert.Greater(area.width * area.height, 0, "precondition: the area to render is not empty");
                var render = new Texture2D(area.width, area.height, TextureFormat.RGBA32, false);
                render.SetPixels(shot.GetPixels(area.x, area.y, area.width, area.height));
                if (cropTo != null) PaintOutTopCorners(render, Mathf.CeilToInt(cropTo.resolvedStyle.borderTopLeftRadius / PanelUnitsPerPixel(cropTo)));
                render.Apply();
                return render;
            }
            finally { Object.Destroy(shot); }
        }

        private static bool Same(Texture2D a, Texture2D b)
        {
            if (a.width != b.width || a.height != b.height) return false;
            Color32[] pa = a.GetPixels32(), pb = b.GetPixels32();
            for (int i = 0; i < pa.Length; i++)
                if (pa[i].r != pb[i].r || pa[i].g != pb[i].g || pa[i].b != pb[i].b || pa[i].a != pb[i].a) return false;
            return true;
        }

        // - judged against a baseline taken in the SAME focus state: The Lamp's real audio chip lays its text column out a sub-pixel apart
        //   with Unity in front and in the background (1256 px of its Half, the same colours, stable within each state), which is no
        //   change of the code under measure
        // - the real wall's card also draws its text with a sub-pixel difference between runs of the SAME tree in the same focus state
        //   (661 px of The Lamp's Full, the status block's words; the gallery never varies): a few discrete states, not drift. So a render
        //   may have several baselines -- <name>.<state>.<k>.png -- and passes when it equals ONE of them exactly. While the file
        //   baseline/COLLECT exists (before the change, over several runs), a render that equals none is kept as one more baseline;
        //   without it (after the change), it fails and the closest baseline's differing pixels are reported
        private static void Judge(string name, Texture2D render)
        {
            name += Application.isFocused ? ".fg" : ".bg";
            string folder = Path.Combine(TestEvidence.Root, Domain, "baseline");
            Directory.CreateDirectory(folder);
            File.WriteAllBytes(TestEvidence.PathFor(Domain + "/latest", name + ".png"), render.EncodeToPNG());
            var baselines = Directory.GetFiles(folder, name + ".*.png").OrderBy(p => p).ToList();
            int bestDiffering = int.MaxValue, bestDelta = 0;
            string best = null;
            foreach (string path in baselines)
            {
                var (differing, maxDelta) = Compare(path, render, name);
                if (differing >= bestDiffering) continue;
                (bestDiffering, bestDelta, best) = (differing, maxDelta, Path.GetFileName(path));
                if (differing == 0) break;
            }
            if (bestDiffering == 0)
            {
                Debug.Log("[ThemePixels] " + name + ": equals " + best + " (0 px differ, of " + baselines.Count + " baselines)");
                return;
            }
            bool collecting = File.Exists(Path.Combine(folder, "COLLECT"));
            Assert.IsTrue(collecting || baselines.Count > 0, name + ": no baseline in this focus state (collect one before the change)");
            if (collecting)
            {
                string kept = name + "." + baselines.Count + ".png";
                File.WriteAllBytes(Path.Combine(folder, kept), render.EncodeToPNG());
                Debug.Log("[ThemePixels] " + name + ": baseline " + kept + " kept" + (best == null ? "" : " (closest " + best + ": " + bestDiffering + " px, max delta " + bestDelta + ")"));
                return;
            }
            // - no exact match: every pixel that is NOT noise (the same in all the baselines of the unchanged tree) must still be identical
            var (outside, outsideDelta, inside) = CompareOutsideNoise(baselines, render, name);
            Debug.Log("[ThemePixels] " + name + ": no exact baseline (closest " + best + ": " + bestDiffering + " px); outside the measured noise " +
                      outside + " px differ (max delta " + outsideDelta + "), inside it " + inside);
            Assert.AreEqual(0, outside, name + ": " + outside + " px outside the measured noise differ from the baselines (max channel delta " + outsideDelta + ")");
        }

        // The noise = every pixel where the baselines disagree among themselves; outside it they all hold one value, which the render must
        // equal. Returns the pixels outside the noise that differ (and their largest delta) and how many differ inside it
        private static (int Outside, int MaxDelta, int Inside) CompareOutsideNoise(System.Collections.Generic.List<string> baselines, Texture2D render, string name)
        {
            Assert.Greater(baselines.Count, 1, name + ": a noise mask needs at least two baselines of the unchanged tree");
            var pictures = baselines.Select(path => { var t = new Texture2D(2, 2); t.LoadImage(File.ReadAllBytes(path)); return t; }).ToList();
            try
            {
                var first = pictures[0].GetPixels32();
                var noise = new bool[first.Length];
                foreach (var other in pictures.Skip(1).Select(t => t.GetPixels32()))
                    for (int i = 0; i < first.Length; i++)
                        if (first[i].r != other[i].r || first[i].g != other[i].g || first[i].b != other[i].b || first[i].a != other[i].a) noise[i] = true;
                var now = render.GetPixels32();
                int outside = 0, inside = 0, maxDelta = 0;
                for (int i = 0; i < first.Length; i++)
                {
                    int d = Mathf.Max(Mathf.Abs(first[i].r - now[i].r), Mathf.Abs(first[i].g - now[i].g), Mathf.Abs(first[i].b - now[i].b), Mathf.Abs(first[i].a - now[i].a));
                    if (d == 0) continue;
                    if (noise[i]) { inside++; continue; }
                    outside++;
                    maxDelta = Mathf.Max(maxDelta, d);
                }
                return (outside, maxDelta, inside);
            }
            finally { foreach (var t in pictures) Object.Destroy(t); }
        }

        // How many pixels of `render` differ from the PNG at `path`, and by how much at most (any channel)
        private static (int Differing, int MaxDelta) Compare(string path, Texture2D render, string name)
        {
            var before = new Texture2D(2, 2);
            try
            {
                Assert.IsTrue(before.LoadImage(File.ReadAllBytes(path)), name + ": the baseline PNG loads");
                Assert.AreEqual(new Vector2Int(before.width, before.height), new Vector2Int(render.width, render.height),
                    name + ": the render has the baseline's size (same frame, same crop)");
                Color32[] a = before.GetPixels32(), b = render.GetPixels32();
                int differing = 0, maxDelta = 0;
                for (int i = 0; i < a.Length; i++)
                {
                    int d = Mathf.Max(Mathf.Abs(a[i].r - b[i].r), Mathf.Abs(a[i].g - b[i].g), Mathf.Abs(a[i].b - b[i].b), Mathf.Abs(a[i].a - b[i].a));
                    if (d == 0) continue;
                    differing++;
                    maxDelta = Mathf.Max(maxDelta, d);
                }
                return (differing, maxDelta);
            }
            finally { Object.Destroy(before); }
        }

        // A square of `side` px at each top corner in one flat colour, the same in every run
        private static void PaintOutTopCorners(Texture2D render, int side)
        {
            side = Mathf.Min(side, render.width / 2, render.height);
            for (int y = render.height - side; y < render.height; y++)
                for (int x = 0; x < side; x++)
                {
                    render.SetPixel(x, y, Color.magenta);
                    render.SetPixel(render.width - 1 - x, y, Color.magenta);
                }
        }

        private static float PanelUnitsPerPixel(VisualElement e) =>
            RuntimePanelUtils.ScreenToPanel(e.panel, Vector2.right).x - RuntimePanelUtils.ScreenToPanel(e.panel, Vector2.zero).x;

        // An element's rect in the screenshot's pixels (the texture's y runs bottom-up)
        private static RectInt ScreenRectOf(VisualElement e, Texture2D shot)
        {
            var origin = RuntimePanelUtils.ScreenToPanel(e.panel, Vector2.zero);
            float unit = PanelUnitsPerPixel(e);
            Rect r = e.worldBound;
            int xMin = Mathf.Clamp(Mathf.CeilToInt((r.xMin - origin.x) / unit), 0, shot.width);
            int xMax = Mathf.Clamp(Mathf.FloorToInt((r.xMax - origin.x) / unit), 0, shot.width);
            int yTop = Mathf.Clamp(Mathf.CeilToInt((r.yMin - origin.y) / unit), 0, shot.height);
            int yBottom = Mathf.Clamp(Mathf.FloorToInt((r.yMax - origin.y) / unit), 0, shot.height);
            return new RectInt(xMin, shot.height - yBottom, xMax - xMin, yBottom - yTop);
        }
    }

    // The gallery part of the set: block looks over the token colours (hero header, text, status ramp, media, a quiz, the audio player), the
    // navigation's family colours (icon chips, the progress map), the menu overlay over its scrim, the floating slot, the full view, the mini-player
    [Explicit("a measurement: run by name before and after a change that must move no pixel")]
    public class ThemePixelSetGalleryTests
    {
        private const string ScenePath = "Assets/Dev/CardNavGallery/CardNavGalleryScene.unity";
        private CardNavGalleryHarness _harness;

        private static readonly string[] BlockEntries =
        {
            "header_image_parallax_short_half", "rich_text_drop_cap_long", "status_ring_tokens", "gallery_carousel_short",
            "knowledge_check_multiple_choice_short", "audio_guide_player_short",
        };

        private static readonly string[] NavEntries =
        {
            "Nav_icon_tabs_chips_big", "Nav_progress_map_big", "Nav_drawer_tree_big", "Nav_fab_radial_big",
        };

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            ThemePixelSet.RequireMeasuring();
#if UNITY_EDITOR
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
#else
            Assert.Ignore("Needs the Editor to load the gallery scene by path.");
#endif
            for (int i = 0; i < 60 && _harness == null; i++)
            {
                _harness = Object.FindFirstObjectByType<CardNavGalleryHarness>();
                yield return null;
            }
            Assert.IsNotNull(_harness, "the navigation gallery scene holds CardNavGalleryHarness");
            // - time is the test's: a playing clip moves only through AdvanceAudio (its progress line would creep with real time)
            _harness.Gallery.AutoAdvanceAudio = false;
            _harness.Gallery.AutoAdvanceVideo = false;
            yield return null;
        }

        [UnityTest]
        public IEnumerator BlockLooks([ValueSource(nameof(BlockEntries))] string name)
        {
            int index = CardGalleryDefinitions.All.ToList().FindIndex(e => e.Name == name);
            Assert.GreaterOrEqual(index, 0, "precondition: the gallery defines " + name);
            _harness.Gallery.Show(index);
            yield return CardTestInput.Settle();
            yield return ThemePixelSet.Capture("block_" + name);
        }

        [UnityTest]
        public IEnumerator NavigationElements([ValueSource(nameof(NavEntries))] string name)
        {
            int index = CardNavGalleryDefinitions.All.ToList().FindIndex(e => e.Name == name);
            Assert.GreaterOrEqual(index, 0, "precondition: the navigation gallery defines " + name);
            _harness.Show(index);
            yield return CardTestInput.Settle();
            yield return ThemePixelSet.Capture(name);
        }

        [UnityTest]
        public IEnumerator TheFullView()
        {
            _harness.Show(CardNavGalleryDefinitions.All.ToList().FindIndex(e => e.Name == "Nav_text_tabs_big"));
            yield return CardTestInput.Settle();
            var sheet = _harness.Gallery.Sheet;
            sheet.OpenFullView(sheet.Nav.Model.Sections[0].Targets[0]);
            yield return CardTestInput.Settle();
            Assert.IsTrue(sheet.FullView.IsOpen, "precondition: the full view opened");
            yield return ThemePixelSet.Capture("full_view");
        }

        [UnityTest]
        public IEnumerator TheMiniPlayer()
        {
            var gallery = _harness.Gallery;
            gallery.Show(CardGalleryDefinitions.All.ToList().FindIndex(e => e.Name == "audio_guide_player_short"));
            yield return CardTestInput.Settle();
            var player = gallery.Sheet.Stack.BoundViews.OfType<AudioGuideBlockView>().First();
            yield return CardTestInput.Tap(player.PlayButton.panel, player.PlayButton.worldBound.center);
            gallery.AdvanceAudio(2f);
            yield return CardTestInput.Tap(gallery.Sheet.CloseButton.panel, gallery.Sheet.CloseButton.worldBound.center);
            yield return CardTestInput.Settle();
            Assert.IsTrue(gallery.AudioCoordinator.Mini.IsShown, "precondition: the mini-player took over");
            yield return ThemePixelSet.Capture("mini_player");
        }
    }

    // The real card on the wall: The Lamp at Half and at Full, cropped to the sheet (the markers behind it animate)
    [Explicit("a measurement: run by name before and after a change that must move no pixel")]
    public class ThemePixelSetSceneTests : SearchSceneFixture
    {
        [UnityTest]
        public IEnumerator TheLamp_AtHalfAndFull()
        {
            ThemePixelSet.RequireMeasuring();
            SelectionEventBus.Select("lamp");
            yield return CardTestInput.Settle();
            var sheet = Card.Sheet;
            Assert.IsTrue(sheet.IsOpen, "precondition: The Lamp's card is open");
            sheet.SetStop(SheetStopRule.Stop.Half);
            yield return CardTestInput.Settle();
            yield return ThemePixelSet.Capture("lamp_half", sheet.Root);
            sheet.SetStop(SheetStopRule.Stop.Full);
            yield return CardTestInput.Settle();
            yield return ThemePixelSet.Capture("lamp_full", sheet.Root);
        }
    }
}
