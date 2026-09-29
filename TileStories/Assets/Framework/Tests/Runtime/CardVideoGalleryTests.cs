using System.Collections;
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
    // Phase A of the video (_3.1 step 9B, 40-testing.md 4.4): both looks, the takeover teaser and the header's loop on the real gallery card,
    // driven with REAL pointer events (the play button, a chapter button, the captions switch, the full-screen button, the takeover's Back)
    // over the card's real video service and sound coordinator. The video device is the gallery's ManualVideoOutput and video time moves only
    // through AdvanceVideo, so no assertion waits for a clip; what the screen shows is read off a real render (CardGalleryChecks.PixelAt). The
    // generic fit / contrast / tap-target / heading / render checks of every entry are CardGalleryTests' own (the video entries are in its list).
    public class CardVideoGalleryTests
    {
        private const string ScenePath = "Assets/Dev/CardGallery/CardGalleryScene.unity";
        private CardGalleryHarness _harness;

        private static int IndexOf(string name) => CardGalleryDefinitions.All.ToList().FindIndex(e => e.Name == name);

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
            // - video and audio time are numbers the test sets: real time never moves a clip
            _harness.AutoAdvanceVideo = false;
            _harness.AutoAdvanceAudio = false;
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            var scene = SceneManager.GetSceneByPath(ScenePath);
            SceneManager.SetActiveScene(SceneManager.CreateScene("AfterCardVideoGalleryTest"));
            if (scene.IsValid() && scene.isLoaded) yield return SceneManager.UnloadSceneAsync(scene);
        }

        private CardVideoService Video => _harness.VideoService;
        private ManualVideoOutput Output => _harness.VideoOutput;
        private static Color PosterColour => CardGalleryDefinitions.Pictures["wide.png"].Colour;

        private IEnumerator Show(string name, System.Action<VideoBlockView> got)
        {
            _harness.Show(IndexOf(name));
            IBlockView view = null;
            yield return CardGalleryChecks.SettledBlock(_harness, name, v => view = v);
            got((VideoBlockView)view);
        }

        private IEnumerator ShowCustom(CardGalleryDefinitions.Entry entry)
        {
            CardGalleryChecks.ShowEntry(_harness, entry);
            yield return CardTestInput.Settle();
        }

        private static IEnumerator Tap(VisualElement target)
        {
            yield return CardTestInput.Tap(target.panel, target.worldBound.center);
        }

        // The colour a render shows in the middle of `surface`
        private static IEnumerator ColourIn(VisualElement surface, System.Action<Color> got)
        {
            yield return CardGalleryChecks.PixelAt(surface, surface.worldBound.center, got);
        }

        private static bool Near(Color a, Color b) => Mathf.Abs(a.r - b.r) < 0.05f && Mathf.Abs(a.g - b.g) < 0.05f && Mathf.Abs(a.b - b.b) < 0.05f;

        // ---------------- the inline look ----------------

        [UnityTest]
        public IEnumerator Inline_ARealTapOnPlay_PlaysThisVideo_ThePosterStaysUntilTheFirstFrame_ThenTheFrames_AndTheButtonsPauseAndResume()
        {
            VideoBlockView view = null;
            yield return Show("video_inline_poster", v => view = v);
            var panel = view.Panel;
            Assert.AreEqual(CardOptions.DisplayInline, view.Display);
            Assert.AreEqual(DisplayStyle.None, view.Teaser.resolvedStyle.display, "the inline display has no teaser");
            Assert.AreEqual("0:00 / 0:12", panel.TimeText.text, "the clip's real length is known before it plays");
            Assert.IsNotNull(panel.Poster.Texture, "the poster was loaded through the block's scope");
            Assert.AreEqual(DisplayStyle.Flex, panel.PlayOverlay.resolvedStyle.display, "a play button over the poster");
            Assert.AreEqual(DisplayStyle.None, panel.ChapterRow.resolvedStyle.display, "the inline look shows no chapter buttons, even with rows written");
            Assert.AreEqual(DisplayStyle.Flex, panel.FullScreenButton.resolvedStyle.display);
            Assert.AreEqual("Full screen", panel.FullScreenButton.tooltip);
            foreach (var button in new[] { panel.PlayOverlay, panel.PlayButton, panel.FullScreenButton, panel.CaptionsButton })
                Assert.IsTrue(UIAccessibility.MeetsMinTapTarget(button.worldBound.width, button.worldBound.height), button.name + " is finger-sized");
            Assert.AreEqual(CardVideoState.Idle, Video.State, "nothing plays until asked");
            Color seen = default;
            // - a point beside the round play button (its glyph sits in the middle of the surface)
            Vector2 Beside(VisualElement s) => new Vector2(s.worldBound.x + s.worldBound.width * 0.2f, s.worldBound.center.y);
            yield return CardGalleryChecks.PixelAt(panel.Surface, Beside(panel.Surface), c => seen = c);
            Assert.IsTrue(Near(PosterColour, seen), "the poster shows: " + seen);
            yield return CardGalleryChecks.Render("Card_video_inline_poster_idle");

            yield return Tap(panel.PlayOverlay);
            Assert.AreEqual(CardVideoState.Playing, Video.State, "a real tap on the play button started it");
            Assert.IsTrue(Video.IsCurrent(view.Track));
            Assert.IsFalse(Output.Muted, "with its sound");
            Assert.IsFalse(panel.ShowsFrames, "no frame yet: the poster stays");
            yield return CardGalleryChecks.PixelAt(panel.Surface, Beside(panel.Surface), c => seen = c);
            Assert.IsTrue(Near(PosterColour, seen), "still the poster while the clip prepares");

            _harness.AdvanceVideo(2.5f);
            yield return null;
            Assert.IsTrue(panel.ShowsFrames, "the first frame arrived");
            Assert.AreEqual(DisplayStyle.None, panel.PlayOverlay.resolvedStyle.display, "the big play button gives way while it plays");
            Assert.AreEqual("0:02 / 0:12", panel.TimeText.text);
            Assert.AreEqual(CardIcons.Shape.Pause, panel.PlayGlyph.Kind);
            yield return CardGalleryChecks.PixelAt(panel.Surface, Beside(panel.Surface), c => seen = c);
            Assert.IsFalse(Near(PosterColour, seen), "the video's frame shows now, not the poster: " + seen);
            yield return CardGalleryChecks.Render("Card_video_inline_poster_playing");

            yield return Tap(panel.PlayButton);
            Assert.AreEqual(CardVideoState.Paused, Video.State, "the small button pauses");
            Assert.AreEqual(DisplayStyle.Flex, panel.PlayOverlay.resolvedStyle.display, "the big play button is back");
            _harness.AdvanceVideo(3f);
            Assert.AreEqual("0:02 / 0:12", panel.TimeText.text, "paused: the time stands still");
            yield return Tap(panel.PlayOverlay);
            Assert.AreEqual(CardVideoState.Playing, Video.State, "and a tap resumes");

            Rect bar = panel.Scrubber.Root.worldBound;
            yield return CardTestInput.Tap(panel.Root.panel, new Vector2(bar.x + bar.width * 0.75f, bar.center.y));
            Assert.AreEqual(9f, Video.Position, 0.6f, "a real press three quarters along the bar: 9 s of 12");
            Assert.AreEqual(1, Output.LoadCount, "one load from the first tap to now");
        }

        [UnityTest]
        public IEnumerator Captions_ARealTapShowsTheLineOfTheMoment_AndNoCaptionsFileMeansNoSwitch()
        {
            VideoBlockView view = null;
            yield return Show("video_inline_poster", v => view = v);
            var panel = view.Panel;
            Assert.AreEqual("Captions", panel.CaptionsButton.text);
            Assert.AreEqual(DisplayStyle.None, panel.CaptionLine.resolvedStyle.display, "off until the visitor switches them on");
            yield return Tap(panel.CaptionsButton);
            Assert.IsTrue(panel.CaptionsShown);
            yield return Tap(panel.PlayOverlay);
            _harness.AdvanceVideo(5f);
            yield return null;
            Assert.AreEqual(CardGalleryDefinitions.ShortCaptionTwo, panel.CaptionLine.text, "at 0:05 the second caption (4 s to 8 s)");
            Assert.AreEqual(DisplayStyle.Flex, panel.CaptionLine.resolvedStyle.display);
            yield return CardGalleryChecks.Render("Card_video_inline_captions_on");

            yield return Show("video_inline_nocaptions", v => view = v);
            Assert.AreEqual(DisplayStyle.None, view.Panel.CaptionsButton.resolvedStyle.display, "no captions file: no switch");
        }

        [UnityTest]
        public IEnumerator AClipThatIsNotThere_SaysVideoUnavailable_AndPlaysNothing()
        {
            VideoBlockView view = null;
            yield return Show("video_inline_unavailable", v => view = v);
            Assert.AreEqual("Video unavailable", view.Panel.Note.text);
            Assert.AreEqual(DisplayStyle.None, view.Panel.PlayOverlay.resolvedStyle.display, "no play button to press");
            Assert.AreEqual(DisplayStyle.None, view.Panel.FullScreenButton.resolvedStyle.display);
            Assert.IsFalse(view.Panel.PlayButton.enabledSelf);
            Assert.AreEqual(DisplayStyle.None, view.Panel.Scrubber.Root.resolvedStyle.display, "no bar with a knob that only looks draggable");
            yield return Tap(view.Panel.PlayButton);
            Assert.AreEqual(CardVideoState.Idle, Video.State);
        }

        [UnityTest]
        public IEnumerator NoPosterWritten_IsAPlainFrameWithThePlayButton_NeverThePictureUnavailableWords()
        {
            VideoBlockView view = null;
            yield return Show("video_inline_noposter", v => view = v);
            Assert.AreEqual(DisplayStyle.None, view.Panel.Poster.Root.resolvedStyle.display, "no poster drawn at all");
            Assert.AreEqual("", view.Panel.Poster.Unavailable.text, "and no 'picture unavailable' words");
            Assert.AreEqual(DisplayStyle.Flex, view.Panel.PlayOverlay.resolvedStyle.display, "the play button on the plain frame");
            Assert.IsTrue(view.Panel.PlayOverlay.enabledSelf);
            yield return CardGalleryChecks.Render("Card_video_inline_noposter_plain");
        }

        // ---------------- the chapters look ----------------

        [UnityTest]
        public IEnumerator Chapters_RunInTimeOrder_ARealTapOnOneJumpsThere_AndTheChapterPlayingIsLit()
        {
            VideoBlockView view = null;
            yield return Show("video_chapters_poster", v => view = v);
            var panel = view.Panel;
            CollectionAssert.AreEqual(new[] { "0:00  Opening", "0:04  The walls", "0:08  The view" }, panel.ChapterButtons.Select(b => b.text).ToList(),
                "written out of order, shown in time order");
            Assert.AreEqual("Chapters", panel.ChapterRow.tooltip);
            foreach (var chip in panel.ChapterButtons)
                Assert.IsTrue(UIAccessibility.MeetsMinTapTarget(chip.worldBound.width, chip.worldBound.height), chip.text + " is finger-sized");

            yield return Tap(panel.ChapterButtons[1]);
            Assert.AreEqual(CardVideoState.Playing, Video.State, "a chapter button of an idle video starts it...");
            Assert.AreEqual(4f, Video.Position, 0.01f, "...at its chapter");
            _harness.AdvanceVideo(0.5f);
            yield return null;
            Assert.IsTrue(panel.ChapterButtons[1].ClassListContains("card-video__chapter--current"), "the chapter playing is lit");
            Assert.IsFalse(panel.ChapterButtons[0].ClassListContains("card-video__chapter--current"));
            Color atWalls = default, atView = default;
            yield return ColourIn(panel.Surface, c => atWalls = c);

            yield return Tap(panel.ChapterButtons[2]);
            _harness.AdvanceVideo(0.5f);
            yield return null;
            Assert.AreEqual(8.5f, Video.Position, 0.01f, "the last chapter");
            Assert.IsTrue(panel.ChapterButtons[2].ClassListContains("card-video__chapter--current"));
            yield return ColourIn(panel.Surface, c => atView = c);
            Assert.IsFalse(Near(atWalls, atView), "the picture moved to another second of the video: " + atWalls + " then " + atView);
            yield return CardGalleryChecks.Render("Card_video_chapters_playing");
        }

        [UnityTest]
        public IEnumerator LongChapters_OnlyTheReadableOnesInsideTheClipBecomeButtons_TheyWrapInsideTheCard_AndTheLastOneWorks()
        {
            VideoBlockView view = null;
            yield return Show("video_chapters_longchapters", v => view = v);
            var panel = view.Panel;
            Assert.AreEqual(8, panel.ChapterButtons.Count, "ten rows written: 'later' is no time and 2:00 is past the 95 s clip");
            Rect card = _harness.Sheet.Root.worldBound;
            foreach (var chip in panel.ChapterButtons)
                Assert.LessOrEqual(chip.worldBound.xMax, card.xMax + 0.5f, chip.text + " stays inside the card");
            Assert.Greater(panel.ChapterButtons[0].worldBound.height, CardTestInput.TokenPx("--ts-touch-target") + 1f, "a long name wraps onto a second line");
            _harness.Sheet.Stack.Scroll.ScrollTo(panel.ChapterButtons[7]);
            yield return CardTestInput.Settle(0.15f);
            yield return Tap(panel.ChapterButtons[7]);
            Assert.AreEqual(90f, Video.Position, 0.01f, "the last button: 1:30");
            yield return CardGalleryChecks.Render("Card_video_chapters_longchapters");
        }

        // ---------------- full screen ----------------

        [UnityTest]
        public IEnumerator FullScreen_ShowsTheSamePlayback_NothingRestarts_AndBackReturnsToTheCardWhereItWas()
        {
            VideoBlockView view = null;
            yield return Show("video_chapters_poster", v => view = v);
            yield return Tap(view.Panel.PlayOverlay);
            _harness.AdvanceVideo(3f);
            yield return Tap(view.Panel.CaptionsButton);
            var scroll = _harness.Sheet.Stack.Scroll;
            float offset = scroll.scrollOffset.y;
            Assert.Greater(offset, 0f, "precondition: the card is scrolled to the video");

            yield return Tap(view.Panel.FullScreenButton);
            yield return CardTestInput.Settle(0.15f);
            yield return CardGalleryChecks.Render("Card_video_fullscreen_opened");
            var takeover = _harness.Sheet.Takeover;
            Assert.IsTrue(takeover.IsOpen, "a real tap opened the full-screen view");
            Assert.AreEqual("Gate > " + CardGalleryDefinitions.VideoTitle, takeover.Crumb.text, "the card's title, then the video's");
            var full = view.FullScreenPanel;
            Assert.IsNotNull(full, "a second panel over the same playback");
            Assert.AreEqual(1, Output.LoadCount, "nothing was loaded again");
            Assert.AreEqual(CardVideoState.Playing, Video.State, "it plays on");
            Assert.AreEqual(3f, Video.Position, 0.01f, "from the same place");
            Assert.IsTrue(full.CaptionsShown, "the visitor's captions choice came along");
            Assert.AreEqual(3, full.ChapterButtons.Count, "and the chapter buttons");
            Assert.AreEqual(DisplayStyle.None, full.FullScreenButton.resolvedStyle.display, "no full-screen button on the full-screen view");
            // - full screen the picture takes ALL the height the screen leaves it (more on a taller phone): the panel fills the page and
            //   nothing is left empty under its last part
            Rect page = takeover.Page.worldBound;
            Assert.AreEqual(page.yMax, full.Root.worldBound.yMax, 1f, "the full-screen panel fills the page");
            Assert.AreEqual(page.yMax, full.ChapterRow.worldBound.yMax + full.ChapterRow.resolvedStyle.marginBottom, 2f, "the chapter buttons sit at the page's bottom: the picture took the rest");
            Assert.Greater(full.Surface.worldBound.height, 0f);
            Assert.GreaterOrEqual(full.Surface.worldBound.width, view.Panel.Surface.worldBound.width - 1f, "as wide as on the card");
            _harness.AdvanceVideo(1f);
            yield return null;
            Assert.IsTrue(full.ShowsFrames, "the full-screen panel draws the frames");
            Assert.AreEqual("0:04 / 0:12", full.TimeText.text);
            yield return CardGalleryChecks.Render("Card_video_fullscreen");

            yield return Tap(takeover.Back);
            yield return CardTestInput.Settle(0.15f);
            Assert.IsFalse(takeover.IsOpen, "Back closed it");
            Assert.IsNull(view.FullScreenPanel, "the full-screen panel was let go");
            Assert.IsNull(full.Track, "and stopped listening to the video");
            Assert.AreEqual(SheetStopRule.Stop.Full, _harness.Sheet.Stop, "the card at the stop it had");
            Assert.AreEqual(offset, scroll.scrollOffset.y, 1f, "and the scroll it had");
            Assert.AreEqual(CardVideoState.Playing, Video.State, "the video still plays on the card");
            Assert.AreEqual("0:04 / 0:12", view.Panel.TimeText.text);
        }

        [UnityTest]
        public IEnumerator TheTakeoverDisplay_IsATeaserOnTheCard_ARealTapPlaysItFullScreen_AndBackLeavesItPlaying()
        {
            VideoBlockView view = null;
            yield return Show("video_inline_takeover", v => view = v);
            Assert.AreEqual(CardOptions.DisplayTakeover, view.Display);
            Assert.AreEqual(DisplayStyle.None, view.Panel.Root.resolvedStyle.display, "no player on the card");
            Assert.AreEqual(DisplayStyle.Flex, view.Teaser.resolvedStyle.display, "a teaser instead");
            Assert.IsNotNull(view.TeaserPoster.Texture, "its poster");
            Assert.AreEqual("0:12", view.TeaserLength.text, "and the clip's length");
            Assert.IsTrue(UIAccessibility.MeetsMinTapTarget(view.TeaserButton.worldBound.width, view.TeaserButton.worldBound.height));
            yield return CardGalleryChecks.Render("Card_video_takeover_teaser");

            yield return Tap(view.TeaserButton);
            yield return CardTestInput.Settle(0.15f);
            Assert.IsTrue(_harness.Sheet.Takeover.IsOpen, "a real tap on the teaser opened it full screen");
            Assert.AreEqual(CardVideoState.Playing, Video.State, "and started it");
            Assert.IsTrue(Video.IsCurrent(view.Track));
            _harness.AdvanceVideo(2f);
            yield return Tap(_harness.Sheet.Takeover.Back);
            yield return CardTestInput.Settle(0.15f);
            Assert.IsFalse(_harness.Sheet.Takeover.IsOpen);
            Assert.AreEqual(CardVideoState.Playing, Video.State, "Back leaves it playing (the card closing stops it)");
            _harness.Sheet.Hide();
            _harness.VideoService.CardClosed();
            Assert.AreEqual(CardVideoState.Idle, Video.State);
            Assert.AreEqual(0, _harness.Media.RefCount("short.mp4"), "the clip was given back");
        }

        // ---------------- the video and the audio guide on one card ----------------

        [UnityTest]
        public IEnumerator OnOneCard_ARealTapOnTheVideo_PausesTheAudioGuide_AndARealTapOnTheAudio_PausesTheVideo()
        {
            var entry = new CardGalleryDefinitions.Entry(BuiltInBlocks.VideoKind, BuiltInBlocks.VideoInline, "withaudio",
                CardGalleryDefinitions.VideoBlock(BuiltInBlocks.VideoInline, "short.mp4", "wide.png", null, null),
                setup: poi =>
                {
                    var audio = CardGalleryDefinitions.AudioBlock(BuiltInBlocks.AudioGuidePlayer, "short.mp3", null, CardGalleryDefinitions.AudioTitle);
                    audio.key = "block_3";
                    poi.card.blocks.Add(audio);
                });
            yield return ShowCustom(entry);
            var video = _harness.Sheet.Stack.BoundViews.OfType<VideoBlockView>().Single();
            var audioView = _harness.Sheet.Stack.BoundViews.OfType<AudioGuideBlockView>().Single();
            _harness.Sheet.Stack.Scroll.ScrollTo(_harness.Sheet.Stack.SlotOf(audioView));
            yield return CardTestInput.Settle(0.15f);
            yield return Tap(audioView.PlayButton);
            Assert.AreEqual(CardAudioState.Playing, _harness.AudioService.State, "precondition: the audio guide plays");

            _harness.Sheet.Stack.Scroll.ScrollTo(_harness.Sheet.Stack.SlotOf(video));
            yield return CardTestInput.Settle(0.15f);
            yield return Tap(video.Panel.PlayOverlay);
            Assert.AreEqual(CardVideoState.Playing, Video.State);
            Assert.AreEqual(CardAudioState.Paused, _harness.AudioService.State, "the video started: the audio guide paused");
            Assert.AreEqual(CardIcons.Shape.Play, audioView.PlayGlyph.Kind, "its button offers play again");

            _harness.Sheet.Stack.Scroll.ScrollTo(_harness.Sheet.Stack.SlotOf(audioView));
            yield return CardTestInput.Settle(0.15f);
            yield return Tap(audioView.PlayButton);
            Assert.AreEqual(CardAudioState.Playing, _harness.AudioService.State);
            Assert.AreEqual(CardVideoState.Paused, Video.State, "the audio started: the video paused");
        }

        // ---------------- the header's loop ----------------

        [UnityTest]
        public IEnumerator TheHeadersLoop_ShowsItsPosterFirst_ThenMutedLoopingFrames_AndGivesWayToTheVisitorsVideo()
        {
            var entry = new CardGalleryDefinitions.Entry(BuiltInBlocks.VideoKind, BuiltInBlocks.VideoInline, "underloop",
                CardGalleryDefinitions.VideoBlock(BuiltInBlocks.VideoInline, "short.mp4", null, null, null), setup: LoopHeader);
            yield return ShowCustom(entry);
            var header = (HeaderBlockView)_harness.Sheet.Stack.BoundViews[0];
            _harness.Sheet.Stack.Scroll.scrollOffset = Vector2.zero;
            yield return CardTestInput.Settle(0.15f);
            Assert.IsTrue(header.HasHero, "the loop look has its hero");
            Assert.IsTrue(Video.PlayingLoop, "the header asked for its loop: nothing else held the player");
            Assert.IsTrue(Output.Muted, "muted");
            Assert.IsTrue(Output.Looping, "looping");
            Assert.IsFalse(header.LoopShowsFrames, "no frame yet: the poster shows first");
            Color seen = default;
            yield return ColourIn(header.HeroPart, c => seen = c);
            Assert.IsTrue(Near(PosterColour, seen), "the poster: " + seen);
            yield return CardGalleryChecks.Render("Card_header_video_loop_poster_first");

            _harness.AdvanceVideo(0.5f);
            yield return null;
            Assert.IsTrue(header.LoopShowsFrames, "the loop's first frame arrived");
            yield return ColourIn(header.HeroPart, c => seen = c);
            Assert.IsFalse(Near(PosterColour, seen), "the loop's frames show: " + seen);
            _harness.AdvanceVideo(100f);
            Assert.IsTrue(Video.PlayingLoop, "it loops: still playing 100 s into a 95 s clip");
            yield return CardGalleryChecks.Render("Card_header_video_loop_playing");

            var film = _harness.Sheet.Stack.BoundViews.OfType<VideoBlockView>().Single();
            _harness.Sheet.Stack.Scroll.ScrollTo(_harness.Sheet.Stack.SlotOf(film));
            yield return CardTestInput.Settle(0.15f);
            yield return Tap(film.Panel.PlayOverlay);
            Assert.IsFalse(Video.PlayingLoop, "the visitor's video took the ONE player");
            Assert.IsFalse(header.LoopShowsFrames, "the header shows its poster meanwhile");
            yield return Tap(film.Panel.PlayButton);
            Video.Stop();
            Assert.IsTrue(Video.PlayingLoop, "the visitor's video stopped: the loop took the player back");
        }

        [UnityTest]
        public IEnumerator WithReduceMotionOn_TheHeaderShowsOnlyItsPoster_AndNoLoopIsPlayed()
        {
            var entry = new CardGalleryDefinitions.Entry(BuiltInBlocks.VideoKind, BuiltInBlocks.VideoInline, "reducemotion",
                CardGalleryDefinitions.VideoBlock(BuiltInBlocks.VideoInline, "short.mp4", null, null, null), setup: LoopHeader,
                wallSetup: wall => wall.card_settings.container.reduce_motion = true);
            yield return ShowCustom(entry);
            var header = (HeaderBlockView)_harness.Sheet.Stack.BoundViews[0];
            _harness.Sheet.Stack.Scroll.scrollOffset = Vector2.zero;
            _harness.AdvanceVideo(1f);
            yield return CardTestInput.Settle(0.15f);
            Assert.IsTrue(header.HasHero, "the hero is there");
            Assert.AreEqual(CardVideoState.Idle, Video.State, "no loop was asked for");
            Assert.IsFalse(header.LoopShowsFrames);
            Color seen = default;
            yield return ColourIn(header.HeroPart, c => seen = c);
            Assert.IsTrue(Near(PosterColour, seen), "only the poster: " + seen);
            yield return CardGalleryChecks.Render("Card_header_video_loop_reduce_motion");
        }

        // A card whose header is the video loop look over the gallery's long clip, the wide picture as its poster
        private static void LoopHeader(POIData poi)
        {
            var header = poi.card.blocks[0];
            header.variant = BuiltInBlocks.HeaderVideoLoop;
            header.fields.Add(new BlockFieldValue { key = BuiltInBlocks.HeaderLoopClipField, asset = CardGalleryDefinitions.LoopClip });
            header.fields.Add(new BlockFieldValue { key = BuiltInBlocks.HeaderImageField, asset = "wide.png" });
        }
    }
}
