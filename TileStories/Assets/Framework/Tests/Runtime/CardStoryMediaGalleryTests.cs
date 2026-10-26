using System.Collections;
using System.Collections.Generic;
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
    // Phase A of the story's MEDIA and MOTION (_3.3 S2b) on the real gallery card (Assets/Dev/CardGallery), fed CardGalleryDefinitions.Story:
    //   - every media element x its width (inset / full bleed) x its frame (rectangle / arch) x light / dark on its PAGE: the frame in its
    //     proportion and shape, edge to edge or in the margins, the pictures loaded, nothing outside the 390 pt frame, captions and credits
    //     >= 7:1 on the render; the carousel's first item on the text's margin and its strip bleeding off the edge
    //   - real input: a tap on a picture opens the shared viewer and Back returns to the PAGE at its scroll; a sideways drag on the carousel
    //     moves the strip and rests an item on the margin while the page stays, an up / down drag on it scrolls the page and not the strip; the
    //     video plays in place through the card's one video owner, or opens the player (the takeover) and Back returns to the page, paused
    //   - motion on the test's clock, frame sequences every 0.1 s over 1 s (TestEvidence/Card/Story/Motion/): a reveal (rows fading in and
    //     rising one by one), a parallax scroll (the picture inside its frame, never an edge), a slow zoom (one mover: the pan beside it holds);
    //     the block's entry on the card; Reduce Motion = identical frames
    public class CardStoryMediaGalleryTests
    {
        private const string ScenePath = "Assets/Dev/CardGallery/CardGalleryScene.unity";
        private CardGalleryHarness _harness;
        private float _time;

        private static string[] MediaSheets =>
            CardGalleryDefinitions.StoryMediaSheetElements.Select(e => CardGalleryDefinitions.StorySheetPrefix + e).ToArray();

        private static string NameOf(string content) => BuiltInBlocks.StoryKind + "_" + BuiltInBlocks.StoryMagazine + "_" + content;

        private static CardGalleryDefinitions.Entry EntryOf(string content) => CardGalleryDefinitions.All.Single(e => e.Name == NameOf(content));

        [UnitySetUp]
        public IEnumerator SetUp()
        {
#if UNITY_EDITOR
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
#else
            Assert.Ignore("Needs the Editor to load the gallery scene by path.");
#endif
            for (int i = 0; i < 60 && _harness == null; i++)
            {
                _harness = Object.FindFirstObjectByType<CardGalleryHarness>();
                yield return null;
            }
            Assert.IsNotNull(_harness, "the gallery scene holds CardGalleryHarness");
            _harness.EnsureBuilt();
            _time = 0f;
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            ThemeController.SetMode(CardOptions.ModeLight);
            var scene = SceneManager.GetSceneByPath(ScenePath);
            SceneManager.SetActiveScene(SceneManager.CreateScene("AfterCardStoryMediaGalleryTest"));
            if (scene.IsValid() && scene.isLoaded) yield return SceneManager.UnloadSceneAsync(scene);
        }

        // ---------------- helpers ----------------

        // Show a story entry at the full stop in `mode` (Reduce Motion on the wall when `reduce`) and hand back its card view (the preview)
        private IEnumerator ShowStory(string content, string mode, bool reduce, System.Action<ComposedBlockView> got)
        {
            ThemeController.SetMode(mode);
            var entry = EntryOf(content);
            CardGalleryChecks.ShowEntry(_harness, reduce
                ? new CardGalleryDefinitions.Entry(entry.Kind, entry.Variant, entry.Content, entry.Block, entry.Setup, wall => wall.card_settings.container.reduce_motion = true)
                : entry);
            CardTestInput.HoldTheWallStill(_harness.Sheet);
            IBlockView view = null;
            yield return CardGalleryChecks.SettledBlock(_harness, entry.Name, v => view = v);
            Assert.IsInstanceOf<ComposedBlockView>(view, content + ": drawn by the composed view");
            got((ComposedBlockView)view);
        }

        // A REAL tap on the preview opens the page; the page's motion runs on this test's clock from its first frame when `onClock` (the
        // pooled page view is handed the clock, then the page is opened again: its rows enter at the clock's time), else it is settled
        private IEnumerator OpenPage(ComposedBlockView preview, bool onClock, System.Action<ComposedBlockView> got)
        {
            yield return TapPreview(preview);
            var page = (ComposedBlockView)_harness.Sheet.FullView.View;
            if (onClock)
            {
                page.Motion.Clock = () => _time;
                _harness.Sheet.Takeover.Close();
                yield return null;
                yield return TapPreview(preview);
                Assert.AreSame(page, _harness.Sheet.FullView.View, "precondition: the pooled page view came back, with the test's clock");
                yield return null;
            }
            else page.Motion.Finish();
            got(page);
        }

        private IEnumerator TapPreview(ComposedBlockView preview)
        {
            preview.Motion.Finish();
            yield return CardTestInput.TapInView(_harness.Sheet.Stack.Scroll, preview.Preview);
            yield return CardTestInput.Settle(0.3f);
            var full = _harness.Sheet.FullView;
            Assert.IsTrue(full.IsOpen, "a real tap on the preview opened the full view");
            Assert.IsTrue(((ComposedBlockView)full.View).IsPage, "...bound as the page");
        }

        private ScrollView PageScroll => _harness.Sheet.FullView.Scroll;

        // The render now, saved under TestEvidence/Card/Story/<folder>; `use` reads it before it is destroyed
        private static IEnumerator Capture(string folder, string name, System.Action<Texture2D> use)
        {
            yield return new WaitForEndOfFrame();
            var shot = ScreenCapture.CaptureScreenshotAsTexture();
            try
            {
                File.WriteAllBytes(TestEvidence.PathFor("Card/Story/" + folder, name + ".png"), shot.EncodeToPNG());
                use?.Invoke(shot);
            }
            finally { Object.Destroy(shot); }
        }

        // The pixels of a panel rect on a render (the region a frame sequence compares)
        private static Color32[] Region(Texture2D shot, VisualElement inPanel, Rect rect)
        {
            var pixels = new List<Color32>();
            float step = 4f;
            for (float y = rect.yMin + 1f; y < rect.yMax - 1f; y += step)
                for (float x = rect.xMin + 1f; x < rect.xMax - 1f; x += step)
                    pixels.Add(CardGalleryChecks.ScreenPixel(shot, inPanel, new Vector2(x, y)));
            return pixels.ToArray();
        }

        // Every shown label of `root` inside the 390 pt frame and the visible page, and >= 7:1 on the render (its words read in the light). A
        // carousel's strip clips its items by design (they bleed off the edge): a label is judged inside the strip's viewport only
        private void AssertWordsFitAndRead(VisualElement root, Texture2D shot, string what, List<string> failures, ref int judged)
        {
            Rect frame = _harness.Frame.worldBound;
            Rect page = PageScroll.contentViewport.worldBound;
            foreach (var label in root.Query<Label>().ToList().Where(l => !string.IsNullOrEmpty(l.text) && CardTestInput.IsShown(l, root)))
            {
                var strip = StripOf(label);
                Rect visible = strip == null ? page : Intersect(page, strip.worldBound);
                if (strip != null && !(label.worldBound.xMin >= visible.xMin - 0.5f && label.worldBound.xMax <= visible.xMax + 0.5f)) continue;
                if (!visible.Overlaps(label.worldBound)) continue;
                Assert.GreaterOrEqual(label.worldBound.xMin, frame.xMin - 0.5f, what + " '" + label.text + "' starts inside the 390 pt frame");
                Assert.LessOrEqual(label.worldBound.xMax, frame.xMax + 0.5f, what + " '" + label.text + "' ends inside it");
                if (!visible.Contains(label.worldBound.center)) continue;
                judged++;
                var (ratio, at) = CardGalleryChecks.WordsOnRender(shot, label, visible);
                if (ratio < UIAccessibility.MinRatioEnhancedText) failures.Add(what + " '" + label.text + "' " + ratio.ToString("0.00") + ":1 at " + at);
            }
        }

        // ---------------- every media element x width x frame x mode ----------------

        [UnityTest]
        public IEnumerator EveryMediaSheet_OnItsPage_EveryWidthAndFrame_FitsAndItsWordsReadSevenToOne_InBothModes(
            [ValueSource(nameof(MediaSheets))] string content, [Values("light", "dark")] string mode)
        {
            string key = content.Substring(CardGalleryDefinitions.StorySheetPrefix.Length);
            Assert.IsTrue(ElementRegistry.Shared.TryGet(key, out var element));
            var sheetRows = CardGalleryDefinitions.SheetRowsOf(element);
            Assert.AreEqual(3, sheetRows.Count, "precondition: the defaults, the other width, the other frame (" + sheetRows.Count + ")");
            ComposedBlockView preview = null, page = null;
            yield return ShowStory(content, mode, false, v => preview = v);
            yield return OpenPage(preview, false, v => page = v);
            Assert.AreEqual(sheetRows.Count, page.Shown.Count, content + ": one page row per option");
            // - the page's side margin as the page draws it: its own padding IS --ts-el-page-inset (a component token: Components.uss)
            float inset = page.Page.resolvedStyle.paddingLeft;
            Assert.Greater(inset, 0f, "precondition: the page has its text margin");
            Rect pageBox = page.Page.worldBound;
            var failures = new List<string>();
            int judged = 0;
            for (int i = 0; i < sheetRows.Count; i++)
            {
                var component = page.Shown[i].Component;
                string option = sheetRows[i].Option, value = sheetRows[i].Value;
                string what = content + " " + mode + " row " + i + " (" + (option.Length == 0 ? "defaults" : option + "=" + value) + ")";
                string width = option == BuiltInElements.WidthStyle ? value : element.Field(BuiltInElements.WidthStyle).ChoiceDefault;
                string shape = option == BuiltInElements.FrameStyle ? value : element.Field(BuiltInElements.FrameStyle).ChoiceDefault;
                PageScroll.ScrollTo(component.Root);
                yield return CardTestInput.Settle(0.15f);
                page.Motion.Finish();
                Assert.IsTrue(component.Root.ClassListContains("ts-el--width-" + (width == BuiltInElements.WidthFullBleed ? "full-bleed" : "inset")), what + ": its width class");
                Assert.IsTrue(component.Root.ClassListContains("ts-el--frame-" + shape), what + ": its frame class");
                // - edge to edge = the page's own width; inset = inside the text's margins
                float expectedWidth = width == BuiltInElements.WidthFullBleed ? pageBox.width : pageBox.width - 2f * inset;
                Assert.AreEqual(expectedWidth, component.Root.worldBound.width, 1f, what + ": " + width + " is " + expectedWidth + " wide");

                var frames = FramesOf(component);
                Assert.Greater(frames.Count, 0, what + ": it draws its frames");
                foreach (var f in frames)
                {
                    if (!(component is VideoElement)) Assert.IsNotNull(f.Image.Texture, what + ": a picture loaded in every frame (" + f.Image.Path + ")");
                    else Assert.IsNotNull(f.Image.Texture, what + ": the poster loaded");
                    float aspect = f.Root.layout.width / f.Root.layout.height;
                    Assert.AreEqual(f.Aspect, aspect, 0.02f, what + ": the frame in its proportion (" + f.Root.layout.size + ")");
                    float expectedAspect = component is PicturePairElement ? ElementMediaRule.PairAspect
                        : component is CarouselElement ? ElementMediaRule.CarouselAspect
                        : component is VideoElement ? f.Aspect
                        : ElementMediaRule.FrameAspect(f.Image.Aspect);
                    Assert.AreEqual(expectedAspect, f.Aspect, 0.001f, what + ": the proportion its rule gives");
                    float radius = f.Root.resolvedStyle.borderTopLeftRadius;
                    if (shape == BuiltInElements.FrameArch)
                        Assert.AreEqual(ElementMediaRule.ArchRadius(f.Root.layout.width, f.Root.layout.height), radius, 0.5f, what + ": an arch's top is a half circle");
                    else Assert.Less(radius, f.Root.layout.width * 0.25f, what + ": a rectangle keeps a small corner (" + radius + ")");
                }
                if (component is CarouselElement carousel)
                {
                    var first = carousel.Items[0].Figure.worldBound;
                    float margin = width == BuiltInElements.WidthFullBleed ? pageBox.xMin + inset : component.Root.worldBound.xMin;
                    Assert.AreEqual(margin, first.xMin, 1f, what + ": the first item lines up with the text's margin");
                    Assert.Greater(carousel.Items[carousel.Items.Count - 1].Figure.worldBound.xMax, carousel.Strip.Viewport.worldBound.xMax,
                        what + ": the strip runs on past the edge (it scrolls)");
                    if (width == BuiltInElements.WidthFullBleed)
                        Assert.AreEqual(pageBox.xMax, carousel.Strip.Viewport.worldBound.xMax, 1f, what + ": full bleed, the strip runs to the page's edge");
                }
                yield return Capture("Media", content + "_" + mode + "_" + i, shot => AssertWordsFitAndRead(component.Root, shot, what, failures, ref judged));
            }
            CollectionAssert.IsEmpty(failures, content + " " + mode + ": every caption, credit and badge >= 7:1 on the render (D1)");
            Assert.Greater(judged, 0, content + ": words were judged (not vacuous)");
        }

        // The carousel strip's clipping viewport a label sits in (null: none)
        private static VisualElement StripOf(VisualElement e)
        {
            for (var p = e.parent; p != null; p = p.parent)
                if (p.ClassListContains("ts-el-carousel__viewport")) return p;
            return null;
        }

        private static Rect Intersect(Rect a, Rect b) =>
            Rect.MinMaxRect(Mathf.Max(a.xMin, b.xMin), Mathf.Max(a.yMin, b.yMin), Mathf.Min(a.xMax, b.xMax), Mathf.Min(a.yMax, b.yMax));

        private static List<MediaFrame> FramesOf(IElementComponent component) => component switch
        {
            PictureElement p => new List<MediaFrame> { p.Frame },
            PicturePairElement pair => pair.Sides.Select(s => s.Frame).ToList(),
            CarouselElement c => c.Items.Select(i => i.Frame).ToList(),
            VideoElement v => new List<MediaFrame> { v.Poster },
            _ => new List<MediaFrame>(),
        };

        // ---------------- real input ----------------

        // A real tap on a picture opens the shared viewer (its caption and credit); Back returns to the PAGE at the scroll it had; the pair's
        // second picture opens the viewer over both at the second; on the preview a picture takes no finger (the whole block opens the page)
        [UnityTest]
        public IEnumerator ARealTapOnAPicture_OpensTheViewer_AndBackReturnsToThePageAtItsScroll()
        {
            ComposedBlockView preview = null, page = null;
            yield return ShowStory(CardGalleryDefinitions.StoryMedia, CardOptions.ModeLight, false, v => preview = v);
            Assert.IsTrue(preview.Shown.Select(s => s.Component).OfType<PictureElement>().All(p => !p.OpensViewer), "on the preview a picture takes no finger");
            yield return OpenPage(preview, false, v => page = v);
            var takeover = _harness.Sheet.Takeover;
            var blockKey = _harness.Sheet.FullView.BlockKey;
            var picture = page.Shown.Select(s => s.Component).OfType<PictureElement>().Single();
            Assert.IsTrue(picture.OpensViewer, "on the page the picture opens the viewer (its Tap's default)");
            PageScroll.ScrollTo(picture.Frame.Root);
            yield return CardTestInput.Settle(0.15f);
            float offset = PageScroll.scrollOffset.y;
            Assert.IsTrue(PageScroll.contentViewport.worldBound.Contains(picture.Frame.Root.worldBound.center), "precondition: the picture is in view");
            yield return CardTestInput.Tap(picture.panel, picture.Frame.Root.worldBound.center);
            yield return CardTestInput.Settle(0.3f);
            Assert.IsTrue(takeover.IsOpen && !_harness.Sheet.FullView.IsOpen, "the viewer replaced the page in the full-screen view");
            Assert.IsTrue(takeover.ReturnsToAPage, "...and Back will return to the page");
            Assert.IsNotNull(takeover.Page.Q("card-gallery-full"), "the shared viewer's picture surface");
            var caption = takeover.Page.Q<Label>(className: "card-gallery__full-caption");
            StringAssert.Contains("the palace front", caption.text, "its caption under it");
            yield return Capture("Taps", "viewer_light", null);

            // - Back: the page again, at the scroll it was left at
            yield return CardTestInput.Tap(takeover.Back.panel, takeover.Back.worldBound.center);
            yield return CardTestInput.Settle(0.4f);
            Assert.IsTrue(_harness.Sheet.FullView.IsOpen, "Back returned to the story's page, not the card");
            Assert.AreEqual(blockKey, _harness.Sheet.FullView.BlockKey);
            Assert.AreEqual(offset, PageScroll.scrollOffset.y, 1f, "...at the scroll the visitor left it");

            // - the pair: its second picture opens the viewer over both, at the second
            page = (ComposedBlockView)_harness.Sheet.FullView.View;
            page.Motion.Finish();
            var pair = page.Shown.Select(s => s.Component).OfType<PicturePairElement>().Single();
            yield return CardTestInput.TapInView(PageScroll, pair.Sides[1].Frame.Root);
            yield return CardTestInput.Settle(0.3f);
            Assert.AreEqual(2, takeover.PageCount, "the pair's two pictures");
            Assert.AreEqual(1, takeover.PageIndex, "...at the one tapped");
            yield return CardTestInput.Tap(takeover.Back.panel, takeover.Back.worldBound.center);
            yield return CardTestInput.Settle(0.3f);
            Assert.IsTrue(_harness.Sheet.FullView.IsOpen, "and back to the page");
            // - a second Back: the card
            yield return CardTestInput.Tap(takeover.Back.panel, takeover.Back.worldBound.center);
            yield return CardTestInput.Settle(0.3f);
            Assert.IsFalse(takeover.IsOpen, "Back from the page: the card");
        }

        // A REAL sideways drag on the carousel moves its strip and rests an item on the text's margin while the page stays; a REAL up / down
        // drag that starts on it scrolls the page and leaves the strip (R10.5: horizontal = the block, vertical = the page)
        [UnityTest]
        public IEnumerator TheCarousel_ASidewaysDragMovesTheStripAndSnaps_AVerticalOneScrollsThePage()
        {
            ComposedBlockView preview = null, page = null;
            yield return ShowStory(CardGalleryDefinitions.StoryMedia, CardOptions.ModeLight, false, v => preview = v);
            yield return OpenPage(preview, false, v => page = v);
            var carousel = page.Shown.Select(s => s.Component).OfType<CarouselElement>().Single();
            PageScroll.ScrollTo(carousel.Root);
            yield return CardTestInput.Settle(0.2f);
            var strip = carousel.Strip;
            Assert.Greater(strip.Row.layout.width, strip.VisibleWidth + 1f, "precondition: the strip is wider than what shows");
            Vector2 on = strip.Viewport.worldBound.center;
            Assert.IsTrue(PageScroll.contentViewport.worldBound.Contains(on), "precondition: the strip is in view");
            float page0 = PageScroll.scrollOffset.y;

            // - sideways: past one item and a bit, released -> an item's start on the margin
            yield return CardTestInput.TouchDrag(strip.Viewport.panel, on, new Vector2(-200f, 6f), 14);
            yield return CardTestInput.Settle(0.5f);
            float inset = strip.Row.resolvedStyle.paddingLeft;
            var starts = carousel.Items.Select(i => i.Figure.layout.x - inset).ToList();
            float max = NavStripRule.MaxOffset(strip.Row.layout.width, strip.VisibleWidth);
            Assert.Greater(strip.Offset, 0f, "the strip moved");
            Assert.IsTrue(starts.Any(s => Mathf.Abs(s - strip.Offset) < 0.5f) || Mathf.Abs(strip.Offset - max) < 0.5f,
                "it rests an item on the margin (offset " + strip.Offset + ", starts " + string.Join(", ", starts) + ")");
            Assert.AreEqual(page0, PageScroll.scrollOffset.y, 1f, "the page did not scroll");
            yield return Capture("Taps", "carousel_swiped_light", null);

            // - vertical, starting on the strip: the page scrolls, the strip stays
            float strip0 = strip.Offset;
            Assert.Greater(PageScroll.verticalScroller.highValue, page0 + 100f, "precondition: the page can scroll on");
            yield return CardTestInput.TouchDrag(strip.Viewport.panel, strip.Viewport.worldBound.center, new Vector2(4f, -160f), 14);
            yield return CardTestInput.Settle(0.4f);
            Assert.Greater(PageScroll.scrollOffset.y, page0 + 40f, "an up / down drag on the carousel scrolls the page");
            Assert.AreEqual(strip0, strip.Offset, 0.5f, "...and leaves the strip where it was");
        }

        // The video, Play In Place: a real tap on its poster plays it where it was through the card's ONE video owner (the gallery's), the
        // teaser giving way to the shared player; closing the page pauses it
        [UnityTest]
        public IEnumerator ARealTapOnTheVideoPoster_PlaysItInPlace_ThroughTheCardsOneVideoOwner()
        {
            ComposedBlockView preview = null, page = null;
            _harness.AutoAdvanceVideo = false;
            yield return ShowStory(CardGalleryDefinitions.StoryMedia, CardOptions.ModeLight, false, v => preview = v);
            yield return OpenPage(preview, false, v => page = v);
            var video = page.Shown.Select(s => s.Component).OfType<VideoElement>().Single();
            Assert.AreEqual(ElementOptions.TapPlayInPlace, video.Tap, "the row's tap");
            Assert.Greater(video.ClipLength, 0f, "precondition: the clip's length was read");
            StringAssert.EndsWith(AudioTimeRule.Format(video.ClipLength), video.BadgeWords.text, "the badge ends with the length");
            StringAssert.StartsWith("The square today", video.BadgeWords.text, "...after the caption");
            yield return CardTestInput.TapInView(PageScroll, video.Teaser);
            yield return CardTestInput.Settle(0.2f);
            var owner = _harness.VideoService;
            Assert.IsTrue(owner.IsCurrent(video.Track) && owner.State == CardVideoState.Playing, "the card's video owner plays the story's clip");
            Assert.IsTrue(video.PlaysInPlace && video.Panel.Root.resolvedStyle.display == DisplayStyle.Flex, "the player shows where the poster was");
            Assert.AreEqual(DisplayStyle.None, video.Teaser.resolvedStyle.display, "the teaser gave way");
            Assert.AreSame(video.Track, video.Panel.Track, "the panel draws THAT playback");
            yield return Capture("Taps", "video_in_place_light", null);
            var takeover = _harness.Sheet.Takeover;
            yield return CardTestInput.Tap(takeover.Back.panel, takeover.Back.worldBound.center);
            yield return CardTestInput.Settle(0.3f);
            Assert.IsFalse(takeover.IsOpen, "Back closed the page");
            Assert.AreEqual(CardVideoState.Paused, owner.State, "leaving the page paused the video it played in place");
        }

        // The video, Open The Player: a real tap on the poster opens the card's full-screen player (the existing takeover) over the owner's
        // playback; Back returns to the PAGE and the video pauses
        [UnityTest]
        public IEnumerator ARealTapOnTheVideoPoster_OpensThePlayer_AndBackReturnsToThePage([Values("light", "dark")] string mode)
        {
            ComposedBlockView preview = null, page = null;
            _harness.AutoAdvanceVideo = false;
            yield return ShowStory(CardGalleryDefinitions.StoryPlayer, mode, false, v => preview = v);
            yield return OpenPage(preview, false, v => page = v);
            var blockKey = _harness.Sheet.FullView.BlockKey;
            var video = page.Shown.Select(s => s.Component).OfType<VideoElement>().Single();
            Assert.AreEqual(ElementOptions.TapOpenPlayer, video.Tap);
            var track = video.Track;
            yield return CardTestInput.TapInView(PageScroll, video.Teaser);
            yield return CardTestInput.Settle(0.3f);
            var takeover = _harness.Sheet.Takeover;
            var owner = _harness.VideoService;
            Assert.IsTrue(takeover.IsOpen && !_harness.Sheet.FullView.IsOpen, "the player opened in the full-screen view");
            Assert.IsNotNull(takeover.Page.Q(className: "card-video--full"), "a full-screen player (the video block's own panel)");
            Assert.IsTrue(owner.IsCurrent(track) && owner.State == CardVideoState.Playing, "the one owner plays the story's video");
            StringAssert.Contains("The square today", takeover.CrumbText, "the breadcrumb names the video");
            yield return Capture("Taps", "video_player_" + mode, null);
            yield return CardTestInput.Tap(takeover.Back.panel, takeover.Back.worldBound.center);
            yield return CardTestInput.Settle(0.4f);
            Assert.IsTrue(_harness.Sheet.FullView.IsOpen && _harness.Sheet.FullView.BlockKey == blockKey, "Back returned to the story's page");
            Assert.IsNull(takeover.Page.Q(className: "card-video--full"), "the player let go");
            Assert.IsTrue(owner.IsCurrent(track) && owner.State == CardVideoState.Paused, "...and the story's video paused");
        }

        // ---------------- motion: frame sequences on the test's clock ----------------

        // A reveal: the page opens on the test's clock -- its first rows fade in and rise one by one (Stagger apart), each at rest from its
        // duration; a row below the fold waits hidden until a scroll brings it past the enter line. Frames every 0.1 s over 1 s
        [UnityTest]
        public IEnumerator AReveal_RowsFadeInAndRiseOneByOne_AsAFrameSequence_AndReduceMotionIsIdenticalFrames([Values(false, true)] bool reduce)
        {
            string content = CardGalleryDefinitions.StoryMotionPrefix + ElementOptions.MotionReveal;
            ComposedBlockView preview = null, page = null;
            yield return ShowStory(content, CardOptions.ModeLight, reduce, v => preview = v);
            yield return OpenPage(preview, true, v => page = v);
            Assert.AreEqual(reduce, page.Motion.ReduceMotion);
            var rows = page.Shown.Select(s => s.Component.Root).ToList();
            Rect visible = PageScroll.contentViewport.worldBound;
            var first = rows.Where(r => r.worldBound.yMin < visible.yMin + visible.height * ElementMotionRule.EnterLine).Take(3).ToList();
            var below = rows.First(r => r.worldBound.yMin > visible.yMax);
            Assert.AreEqual(3, first.Count, "precondition: three rows in the first view");
            if (!reduce) Assert.AreEqual(0f, below.resolvedStyle.opacity, 0.01f, "a row below the fold waits hidden");

            var opacities = new List<float[]>();
            var shots = new List<Color32[]>();
            for (int f = 0; f <= 10; f++)
            {
                _time = f * 0.1f;
                page.Motion.Tick();
                yield return null;
                opacities.Add(first.Select(r => r.resolvedStyle.opacity).ToArray());
                yield return Capture("Motion", "reveal_" + (reduce ? "reduced" : "full") + "_" + f.ToString("00"), shot => shots.Add(Region(shot, page.Root, visible)));
            }
            if (reduce)
            {
                foreach (var o in opacities) CollectionAssert.AreEqual(new[] { 1f, 1f, 1f }, o, "Reduce Motion: every row simply there");
                for (int f = 1; f < shots.Count; f++) CollectionAssert.AreEqual(shots[0], shots[f], "Reduce Motion: frame " + f + " identical to the first");
                yield break;
            }
            // - one by one: at every frame a row is never ahead of the row before it, and at 0.1 s the first is further than the third
            for (int f = 0; f < opacities.Count; f++)
                for (int r = 1; r < 3; r++)
                    Assert.LessOrEqual(opacities[f][r], opacities[f][r - 1] + 1e-4f, "frame " + f + ": row " + r + " never ahead of row " + (r - 1));
            Assert.Greater(opacities[1][0], opacities[1][2] + 0.05f, "0.1 s: the first row ahead of the third (staggered)");
            for (int r = 0; r < 3; r++)
                for (int f = 1; f < opacities.Count; f++) Assert.GreaterOrEqual(opacities[f][r], opacities[f - 1][r] - 1e-4f, "row " + r + ": the opacity never drops");
            float done = ElementMotionRule.RevealDuration + ElementMotionRule.StaggerDelay(2);
            for (int f = 0; f < opacities.Count; f++)
                if (f * 0.1f >= done + 1e-3f) CollectionAssert.AreEqual(new[] { 1f, 1f, 1f }, opacities[f], "frame " + f + ": all at rest");
            Assert.AreEqual(0f, first[0].resolvedStyle.translate.y, 0.01f, "at rest: in its place");

            // - the row below the fold: scrolled to the view's centre (past the enter line) at t, it reveals from t
            PageScroll.scrollOffset = new Vector2(0f, below.layout.y - PageScroll.contentViewport.layout.height * 0.5f);
            yield return null;
            var box = below.parent.ChangeCoordinatesTo(PageScroll.contentViewport, below.layout);
            Assert.IsTrue(ElementMotionRule.Entered(box.yMin, box.yMax, PageScroll.contentViewport.layout.height), "precondition: it is past the enter line now (" + box + ")");
            _time = 5f;
            page.Motion.Tick();
            yield return null;
            Assert.AreEqual(ElementMotionRule.RevealRise, below.resolvedStyle.translate.y, 0.5f, "it entered: it starts RevealRise below its place");
            _time = 5f + ElementMotionRule.RevealDuration / 2f;
            page.Motion.Tick();
            yield return null;
            Assert.That(below.resolvedStyle.opacity, Is.InRange(0.5f, 0.99f), "halfway: past half-faded (decelerating)");
            _time = 5f + ElementMotionRule.RevealDuration;
            page.Motion.Tick();
            yield return null;
            Assert.AreEqual(1f, below.resolvedStyle.opacity, 1e-3f, "at rest");
        }

        // A parallax scroll: the picture lags the page inside its clipped frame by its distance to the view's centre (never showing an edge) as
        // the page scrolls 30 pt a frame; Reduce Motion: the picture still in its frame at every scroll
        [UnityTest]
        public IEnumerator AParallaxScroll_MovesThePictureInsideItsFrame_AsAFrameSequence_AndReduceMotionKeepsItStill([Values(false, true)] bool reduce)
        {
            string content = CardGalleryDefinitions.StoryMotionPrefix + ElementOptions.MotionParallax;
            ComposedBlockView preview = null, page = null;
            yield return ShowStory(content, CardOptions.ModeLight, reduce, v => preview = v);
            yield return OpenPage(preview, true, v => page = v);
            var picture = page.Shown.Select(s => s.Component).OfType<PictureElement>().Single();
            Assert.AreEqual(!reduce, picture.ClassListContains(ElementMotion.MovesPictureClass), "the moving picture overhangs its frame (not under Reduce Motion)");
            // - start with the picture low in the view, then scroll it up through the centre
            var viewport = PageScroll.contentViewport;
            float start = Mathf.Max(0f, picture.layout.y - viewport.layout.height * 0.8f);
            PageScroll.scrollOffset = new Vector2(0f, start);
            yield return null;
            var offsets = new List<float>();
            var shots = new List<Color32[]>();
            for (int f = 0; f <= 10; f++)
            {
                PageScroll.scrollOffset = new Vector2(0f, start + f * 30f);
                _time = f * 0.1f;
                yield return null;
                page.Motion.Tick();
                yield return null;
                var frame = picture.Frame.Root.worldBound;
                var drawn = picture.Frame.Picture.worldBound;
                offsets.Add(drawn.center.y - frame.center.y);
                if (!reduce)
                {
                    Assert.LessOrEqual(drawn.yMin, frame.yMin + 0.5f, "frame " + f + ": never an empty edge at the top");
                    Assert.GreaterOrEqual(drawn.yMax, frame.yMax - 0.5f, "frame " + f + ": nor at the bottom");
                    // - the expected offset from the rule, on the picture's box measured in the view
                    var box = picture.parent.ChangeCoordinatesTo(viewport, picture.layout);
                    float room = (picture.Frame.Picture.layout.height - picture.Frame.Root.layout.height) * 0.5f;
                    float expected = ElementMotionRule.ParallaxOffset(box.center.y, viewport.layout.height * 0.5f, ElementOptions.StrengthSubtle, room);
                    Assert.AreEqual(expected, offsets[f], 0.75f, "frame " + f + ": the rule's offset");
                }
                yield return Capture("Motion", "parallax_" + (reduce ? "reduced" : "full") + "_" + f.ToString("00"),
                    shot => shots.Add(Region(shot, page.Root, picture.Frame.Root.worldBound)));
            }
            if (reduce)
            {
                foreach (float o in offsets) Assert.AreEqual(0f, o, 0.5f, "Reduce Motion: the picture still, centred in its frame");
                yield break;
            }
            // - parallax: as the page goes up, the picture LAGS -- it moves down inside its frame (the prototype's sign)
            Assert.Greater(offsets[10] - offsets[0], 10f, "the picture lagged the page inside its frame (" + offsets[0] + " -> " + offsets[10] + ")");
            for (int f = 1; f < offsets.Count; f++) Assert.GreaterOrEqual(offsets[f], offsets[f - 1] - 0.5f, "frame " + f + ": one direction");
        }

        // A slow zoom: the picture nearest the view's centre pushes in slowly (one mover): the strong pan lower down holds still until it is
        // the nearer one; frames every 0.1 s over 1 s; Reduce Motion: identical frames
        [UnityTest]
        public IEnumerator ASlowZoom_PushesInSlowly_OneMoverAtATime_AsAFrameSequence_AndReduceMotionIsIdenticalFrames([Values(false, true)] bool reduce)
        {
            string content = CardGalleryDefinitions.StoryMotionPrefix + ElementOptions.MotionSlowZoom;
            ComposedBlockView preview = null, page = null;
            yield return ShowStory(content, CardOptions.ModeLight, reduce, v => preview = v);
            yield return OpenPage(preview, true, v => page = v);
            var pictures = page.Shown.Select(s => s.Component).OfType<PictureElement>().ToList();
            Assert.AreEqual(2, pictures.Count, "precondition: the zoom and the pan");
            var zoom = pictures[0];
            var pan = pictures[1];
            // - the zoom at the view's centre
            var viewport = PageScroll.contentViewport;
            PageScroll.scrollOffset = new Vector2(0f, Mathf.Max(0f, zoom.layout.center.y - viewport.layout.height * 0.5f));
            yield return null;
            page.Motion.Finish();
            var scales = new List<float>();
            var shots = new List<Color32[]>();
            float panX = pan.Frame.Picture.resolvedStyle.translate.x;
            for (int f = 0; f <= 10; f++)
            {
                _time = 10f + f * 0.1f;
                page.Motion.Tick();
                yield return null;
                scales.Add(zoom.Frame.Picture.resolvedStyle.scale.value.x);
                if (!reduce) Assert.AreSame(zoom, page.Motion.Mover, "frame " + f + ": the zoom (nearest the centre) is the one mover");
                Assert.AreEqual(panX, pan.Frame.Picture.resolvedStyle.translate.x, 0.01f, "frame " + f + ": the pan holds still meanwhile");
                yield return Capture("Motion", "zoom_" + (reduce ? "reduced" : "full") + "_" + f.ToString("00"), shot => shots.Add(Region(shot, page.Root, zoom.Frame.Root.worldBound)));
            }
            if (reduce)
            {
                Assert.IsNull(page.Motion.Mover, "Reduce Motion: nothing moves by itself");
                foreach (float s in scales) Assert.AreEqual(1f, s, 1e-4f, "the picture at rest");
                for (int f = 1; f < shots.Count; f++) CollectionAssert.AreEqual(shots[0], shots[f], "Reduce Motion: frame " + f + " identical to the first");
                yield break;
            }
            for (int f = 1; f < scales.Count; f++) Assert.Greater(scales[f], scales[f - 1], "frame " + f + ": pushing in");
            Assert.Less(scales[10] - 1f, ElementMotionRule.ZoomMax - 1f, "slow: one second is a fraction of the push-in");

            // - the pan to the centre: it becomes the mover, the zoom holds
            PageScroll.scrollOffset = new Vector2(0f, Mathf.Max(0f, pan.layout.center.y - viewport.layout.height * 0.5f));
            yield return null;
            float held = zoom.Frame.Picture.resolvedStyle.scale.value.x;
            float pan0 = float.NaN;
            for (int f = 0; f <= 5; f++)
            {
                _time = 20f + f * 0.1f;
                page.Motion.Tick();
                yield return null;
                Assert.AreSame(pan, page.Motion.Mover, "the pan nearest the centre now moves");
                Assert.AreEqual(held, zoom.Frame.Picture.resolvedStyle.scale.value.x, 1e-4f, "the zoom holds where it stopped");
                if (float.IsNaN(pan0)) pan0 = pan.Frame.Picture.resolvedStyle.translate.x;
            }
            Assert.AreNotEqual(pan0, pan.Frame.Picture.resolvedStyle.translate.x, "the pan travels");
        }

        // The block's entry on the card: the preview rises and fades in on the test's clock as it enters the stack's view; Reduce Motion: there
        [UnityTest]
        public IEnumerator TheStorysPreview_RisesOntoTheCard_OnItsEntry([Values(false, true)] bool reduce)
        {
            ComposedBlockView preview = null;
            yield return ShowStory(CardGalleryDefinitions.StoryMagazine, CardOptions.ModeLight, reduce, v => preview = v);
            preview.Motion.Clock = () => _time;
            yield return ShowStory(CardGalleryDefinitions.StoryMagazine, CardOptions.ModeLight, reduce, v => preview = v);
            var samples = new List<(float Opacity, float Rise)>();
            for (int f = 0; f <= 5; f++)
            {
                _time = f * 0.1f;
                preview.Motion.Tick();
                yield return null;
                samples.Add((preview.Root.resolvedStyle.opacity, preview.Root.resolvedStyle.translate.y));
            }
            if (reduce)
            {
                Assert.IsTrue(samples.All(s => s.Opacity == 1f && s.Rise == 0f), "Reduce Motion: the preview is simply there");
                yield break;
            }
            Assert.Less(samples[0].Opacity, 0.05f, "it starts transparent");
            Assert.AreEqual(ElementMotionRule.RevealRise, samples[0].Rise, 0.5f, "...RevealRise below its place");
            Assert.AreEqual(1f, samples[5].Opacity, 1e-3f, "at rest after its duration");
            Assert.AreEqual(0f, samples[5].Rise, 0.01f);
            Assert.IsTrue(preview.Shown.All(s => s.Component.Root.resolvedStyle.opacity == 1f), "its rows come in WITH it (no reveal of their own on the preview)");
        }
    }
}
