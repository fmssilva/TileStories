using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // Phase B of the story (_3.3 S2a, S2b) on the REAL LivingRoomScene with real input: The Painting's own story block (composed through the
    // POI Editor's config path, EN + PT), its text rows and its six media rows (the full-bleed hero, the palace front inset with its caption,
    // the opera ruins full bleed, the before / today pair, the carousel, the video):
    //   - on its card, its PREVIEW: the Page's picture, eyebrow + title and first text in the card's language, no slot heading over it (its
    //     title is its heading), capped at --ts-preview-max of the card's full height, the Open Row saying the kind's words and the minutes
    //   - a real tap opens its PAGE in the full view: every row in order, drawn, every picture loaded, every caption in the card's language,
    //     the breadcrumb naming the story; Previous / Next by real taps on the sibling chips; Back returns to the card at its stop and scroll
    //   - the carousel: a real sideways finger moves its strip (an item rests on the margin) and not the page; an up / down one scrolls the page
    //   - the video: a real tap on its poster opens the card's existing full-screen player over the ONE video owner's playback; Back returns
    //     to the page, the video paused
    //   - light and dark, English and Portuguese renders (TestEvidence/Card/Story/painting_*), for the eye beside the prototype
    //   - the gated cost of the page's motion (TestEvidence/Story/MEASURING): batches and UI Toolkit's CPU with the rows moving vs still
    public class PoiCardStorySceneTests : SearchSceneFixture
    {
        private const string Painting = "painting";

        private PoiCardSheetView Sheet => Card.Sheet;
        private ScrollView Scroll => Sheet.Stack.Scroll;
        private ScrollView PageScroll => Sheet.FullView.Scroll;
        private float OnePixel => Sheet.Layer.panel.visualTree.layout.height / Screen.height;

        // The Painting's story block as the shipped config holds it
        private BlockInstanceData StoryBlock(out POIData painting)
        {
            painting = ConfigCopy().pois.Single(p => p.id == Painting);
            var story = painting.card.blocks.Where(b => b.kind == BuiltInBlocks.StoryKind).ToList();
            Assert.AreEqual(1, story.Count, "precondition: The Painting ships one story block");
            return story[0];
        }

        private static System.Collections.Generic.List<ElementRow> PageRows(BlockInstanceData block) =>
            BlockSurfaces.PageRows(block, BuiltInBlocks.Story).Select(r => new ElementRow(r)).ToList();

        private IEnumerator Open(string language, string mode)
        {
            var copy = ConfigCopy();
            copy.card_settings.languages = language == "en" ? copy.card_settings.languages : new System.Collections.Generic.List<string> { language, "en" };
            Session.ApplyCardSettings(copy);
            yield return CardTestInput.Settle(0.3f);
            ThemeController.SetMode(mode);
            if (SelectionEventBus.CurrentPoiId != Painting)
            {
                SelectionEventBus.Clear();
                yield return CardTestInput.Settle();
                SelectionEventBus.Select(Painting);
                yield return CardTestInput.Settle();
            }
            Sheet.SetStop(SheetStopRule.Stop.Full);
            yield return CardTestInput.Settle();
            float rest = Sheet.Stops.HeightOf(SheetStopRule.Stop.Full);
            for (int frame = 0; frame < 600 && Mathf.Abs(Sheet.Root.layout.height - rest) > OnePixel; frame++) yield return null;
            CardTestInput.HoldTheWallStill(Sheet);
            Assert.AreEqual(language, Sheet.Context.Language, "precondition: the card speaks " + language);
        }

        // The story's view on the card, scrolled into view, its entry settled
        private IEnumerator Preview(System.Action<ComposedBlockView> got)
        {
            var block = StoryBlock(out _);
            var view = Sheet.Stack.ViewOfBlock(block.key);
            Assert.IsInstanceOf<ComposedBlockView>(view, "the story block is on The Painting's card, drawn by the composed view");
            Scroll.ScrollTo(view.Root);
            yield return CardTestInput.Settle(0.3f);
            ((ComposedBlockView)view).Motion.Finish();
            got((ComposedBlockView)view);
        }

        // A real tap on the preview: the page in the full view, its rows' motion settled
        private IEnumerator OpenPage(ComposedBlockView preview, System.Action<ComposedBlockView> got)
        {
            yield return CardTestInput.TapInView(Scroll, preview.Preview);
            yield return CardTestInput.Settle(0.3f);
            Assert.IsTrue(Sheet.FullView.IsOpen, "a real tap on the preview opened the full view");
            var page = (ComposedBlockView)Sheet.FullView.View;
            Assert.IsTrue(page.IsPage);
            page.Motion.Finish();
            got(page);
        }

        private static IEnumerator Grab(string name)
        {
            yield return new WaitForEndOfFrame();
            var shot = ScreenCapture.CaptureScreenshotAsTexture();
            File.WriteAllBytes(TestEvidence.PathFor("Card/Story", name + ".png"), shot.EncodeToPNG());
            Object.Destroy(shot);
        }

        // A label's words without its rich-text tags (the leading, glossary marks)
        private static string Shown(string text) => System.Text.RegularExpressions.Regex.Replace(text ?? "", "<[^>]+>", "");

        [UnityTearDown]
        public IEnumerator BackToLight()
        {
            ThemeController.SetMode(CardOptions.ModeLight);
            yield return null;
        }

        // The preview on the real card, in English and in Portuguese, light and dark: the hero picture, eyebrow + title, the lede
        [UnityTest]
        public IEnumerator ThePaintingsStoryPreview_ShowsItsPictureEyebrowTitleAndLede_CappedWithItsOpenRow([Values("en", "pt")] string language,
            [Values("light", "dark")] string mode)
        {
            yield return Open(language, mode);
            ComposedBlockView preview = null;
            yield return Preview(v => preview = v);
            var block = StoryBlock(out var painting);
            var rows = PageRows(block);

            CollectionAssert.AreEqual(new[] { BuiltInElements.Picture, BuiltInElements.Eyebrow, BuiltInElements.Title, BuiltInElements.Lede },
                preview.Shown.Select(s => s.Content.Kind), "Automatic: the Page's first picture, the eyebrow with its title, the first text");
            var picture = (PictureElement)preview.Shown[0].Component;
            Assert.AreEqual(rows[0].Media[0].Asset, picture.Frame.Image.Path, "the hero picture");
            Assert.IsNotNull(picture.Frame.Image.Texture, "...loaded from the wall's media folder");
            Assert.IsFalse(picture.OpensViewer, "on the preview the picture takes no finger: the whole block opens the page");
            Assert.AreEqual(DisplayStyle.None, picture.Words.Root.resolvedStyle.display, "a summary: the picture without its caption (the page has it)");
            // - full bleed on the card: edge to edge of the block frame's inner edge, past the text's margins on both sides
            float margin = preview.Rows.resolvedStyle.paddingLeft;
            Assert.Greater(margin, 0f, "precondition: the rows keep the text's margin");
            Assert.AreEqual(preview.Root.worldBound.width + 2f * margin, picture.Frame.Root.worldBound.width, 1f, "the hero runs to the frame's edges");
            Assert.AreEqual(preview.Root.worldBound.xMin - margin, picture.Frame.Root.worldBound.xMin, 1f, "...from its left edge");
            string Words(ElementRow row) => BlockFieldReader.Pick(row.Field(BuiltInElements.TextField).text, language, "en");
            Assert.AreEqual(Words(rows.First(r => r.Kind == BuiltInElements.Title)), Shown(((LineElement)preview.Shown[2].Component).Text.text), "the title in " + language);
            Assert.AreEqual(Words(rows.First(r => r.Kind == BuiltInElements.Lede)), Shown(((ProseElement)preview.Shown[3].Component).Text.Paragraphs[0].text), "the lede in " + language);
            // - the review fix: no slot heading over the story (its own title is its heading)
            var heading = Sheet.Stack.HeadingOf(preview);
            Assert.IsTrue(heading.ClassListContains(BlockStackView.OwnTitleClass), "the slot heading is marked as the block's own title");
            Assert.AreEqual(DisplayStyle.None, heading.resolvedStyle.display, "...and not drawn (with or without words of its own)");

            Assert.Greater(Sheet.FullHeight, 0f, "precondition: the full stop is measured");
            Assert.AreEqual(Sheet.FullHeight * 0.5f, preview.Cap, 0.01f, "the cap: half the card's full height");
            Assert.LessOrEqual(preview.Root.worldBound.height, preview.Cap + OnePixel, "the preview never passes its cap");
            Assert.IsTrue(UIAccessibility.MeetsMinTapTarget(preview.OpenRow.worldBound.width, preview.OpenRow.worldBound.height), "the Open Row is a whole touch target");

            int minutes = ElementRules.Minutes(ElementScope.Of(painting, block, BlockRegistry.Shared), ElementRegistry.Shared, language, "en");
            Assert.Greater(minutes, 0, "precondition: the story has words to count");
            var strings = Sheet.Context.Strings;
            string expected = string.Format(strings.Get(CardStrings.Keys.OpenRowMeta), strings.Get(CardStrings.Keys.OpenRowStory),
                string.Format(strings.Get(CardStrings.Keys.NavContentsMinutes), minutes));
            Assert.AreEqual(expected, preview.OpenRow.Words.text, "the Open Row in " + language);
            Assert.AreEqual(ElementOptions.EmphasisNudge, preview.OpenRow.Emphasis, "the wall's default emphasis (card_settings.components)");
            yield return Grab("painting_preview_" + language + "_" + mode);
        }

        // A real tap opens the page: every row in order and drawn, every picture loaded, every caption in the card's language; Previous / Next
        // by real taps; Back returns to the card unchanged
        [UnityTest]
        public IEnumerator ARealTap_OpensTheStoryPage_EveryRowDrawnInOrder_NextAndPreviousTurn_BackReturns([Values("en", "pt")] string language,
            [Values("light", "dark")] string mode)
        {
            yield return Open(language, mode);
            ComposedBlockView preview = null, page = null;
            yield return Preview(v => preview = v);
            float offset = Scroll.scrollOffset.y;
            var block = StoryBlock(out _);
            var rows = PageRows(block);
            yield return OpenPage(preview, v => page = v);

            var full = Sheet.FullView;
            var takeover = Sheet.Takeover;
            Assert.AreEqual(block.key, full.BlockKey, "...on the story");
            Assert.AreEqual(17, rows.Count, "precondition: the shipped story's 17 rows (11 text and structure, 6 media)");
            CollectionAssert.AreEqual(rows.Select(r => r.Kind), page.Shown.Select(s => s.Content.Kind), "every Page row, in order");
            Assert.IsTrue(page.Shown.All(s => s.Component != null), "every row drawn by its component");
            for (int i = 0; i < rows.Count; i++)
            {
                var component = page.Shown[i].Component;
                var media = rows[i].Media;
                if (media.Count == 0) continue;
                string Caption(int m) => media[m].Caption(language, "en");
                switch (component)
                {
                    case PictureElement p:
                        Assert.IsNotNull(p.Frame.Image.Texture, "row " + i + ": its picture loaded (" + media[0].Asset + ")");
                        Assert.AreEqual(Caption(0), p.Words.Caption.text, "row " + i + ": its caption in " + language);
                        Assert.AreEqual(media[0].Credit, p.Words.Credit.text, "row " + i + ": its credit");
                        break;
                    case PicturePairElement pair:
                        for (int s = 0; s < 2; s++)
                        {
                            Assert.IsNotNull(pair.Sides[s].Frame.Image.Texture, "the pair's picture " + s + " loaded");
                            Assert.AreEqual(Caption(s), pair.Sides[s].Words.Caption.text, "the pair's caption " + s + " in " + language);
                        }
                        break;
                    case CarouselElement carousel:
                        Assert.AreEqual(media.Count, carousel.Items.Count, "one item per picture");
                        for (int s = 0; s < media.Count; s++)
                        {
                            Assert.IsNotNull(carousel.Items[s].Frame.Image.Texture, "carousel picture " + s + " loaded");
                            Assert.AreEqual(Caption(s), carousel.Items[s].Words.Caption.text, "carousel caption " + s + " in " + language);
                        }
                        break;
                    case VideoElement video:
                        Assert.IsNotNull(video.Poster.Image.Texture, "the video's poster loaded");
                        Assert.Greater(video.ClipLength, 50f, "the clip's real length was read (the 55 s castle clip)");
                        StringAssert.StartsWith(Caption(0), video.BadgeWords.text, "its badge: the caption in " + language + "...");
                        StringAssert.EndsWith(AudioTimeRule.Format(video.ClipLength), video.BadgeWords.text, "...then its length");
                        break;
                    default:
                        Assert.Fail("row " + i + " (" + rows[i].Kind + ") holds media but is drawn by " + component.GetType().Name);
                        break;
                }
            }
            string title = BlockFieldReader.Pick(rows.First(r => r.Kind == BuiltInElements.Title).Field(BuiltInElements.TextField).text, language, "en");
            StringAssert.Contains(title, takeover.CrumbText, "the breadcrumb names the story by its title");
            yield return Grab("painting_page_top_" + language + "_" + mode);
            foreach (int at in new[] { 5, 9, 13, 15, 16 })
            {
                PageScroll.ScrollTo(page.Shown[at].Component.Root);
                yield return CardTestInput.Settle(0.2f);
                page.Motion.Finish();
                yield return Grab("painting_page_" + at + "_" + language + "_" + mode);
            }

            // - Next / Previous: the story's siblings (its section's blocks), a chip each
            Assert.Greater(takeover.PageCount, 1, "precondition: the story opens among its section's blocks");
            int story = takeover.PageIndex;
            int next = story + 1 < takeover.PageCount ? story + 1 : story - 1;
            var chip = takeover.ChipIcon(next).parent;
            Assert.IsTrue(takeover.ChipStrip.worldBound.Contains(chip.worldBound.center), "precondition: the neighbour's chip is in view");
            yield return CardTestInput.Tap(chip.panel, chip.worldBound.center);
            yield return CardTestInput.Settle(0.3f);
            Assert.AreEqual(next, takeover.PageIndex, "a real tap on the neighbour's chip turned the page");
            Assert.AreNotEqual(block.key, full.BlockKey, "...to another block");
            var back = takeover.ChipIcon(story).parent;
            yield return CardTestInput.Tap(back.panel, back.worldBound.center);
            yield return CardTestInput.Settle(0.3f);
            Assert.AreEqual(block.key, full.BlockKey, "and back to the story");

            yield return CardTestInput.Tap(takeover.Back.panel, takeover.Back.worldBound.center);
            yield return CardTestInput.Settle(0.3f);
            Assert.IsFalse(takeover.IsOpen, "Back closed the page");
            Assert.AreEqual(SheetStopRule.Stop.Full, Sheet.Stop, "the card at the same stop");
            Assert.AreEqual(offset, Scroll.scrollOffset.y, 1f, "...and scroll");
            Assert.AreSame(preview, Sheet.Stack.ViewOfBlock(block.key), "the preview stayed bound");
        }

        // The carousel on the real page: a real sideways finger moves the strip and rests an item on the margin, the page stays; an up / down
        // finger that starts on it scrolls the page and leaves the strip (R10.5)
        [UnityTest]
        public IEnumerator TheCarousel_ARealSidewaysFingerMovesTheStrip_AnUpDownOneScrollsThePage()
        {
            yield return Open("en", CardOptions.ModeLight);
            ComposedBlockView preview = null, page = null;
            yield return Preview(v => preview = v);
            yield return OpenPage(preview, v => page = v);
            var carousel = page.Shown.Select(s => s.Component).OfType<CarouselElement>().Single();
            var strip = carousel.Strip;
            // - the strip in the middle of the view, with page above and below it to scroll
            PageScroll.scrollOffset = new Vector2(0f, carousel.Root.layout.y - PageScroll.contentViewport.layout.height * 0.4f);
            yield return CardTestInput.Settle(0.2f);
            Vector2 on = strip.Viewport.worldBound.center;
            Assert.IsTrue(PageScroll.contentViewport.worldBound.Contains(on), "precondition: the strip is in view");
            Assert.Greater(strip.Row.layout.width, strip.VisibleWidth + 1f, "precondition: the strip is wider than what shows");
            float page0 = PageScroll.scrollOffset.y;

            yield return CardTestInput.TouchDrag(strip.Viewport.panel, on, new Vector2(-220f, 8f), 14);
            yield return CardTestInput.Settle(0.5f);
            float inset = strip.Row.resolvedStyle.paddingLeft;
            var starts = carousel.Items.Select(i => i.Figure.layout.x - inset).ToList();
            float max = NavStripRule.MaxOffset(strip.Row.layout.width, strip.VisibleWidth);
            Assert.Greater(strip.Offset, 0f, "a real sideways finger moved the strip");
            Assert.IsTrue(starts.Any(s => Mathf.Abs(s - strip.Offset) < 0.5f) || Mathf.Abs(strip.Offset - max) < 0.5f,
                "an item rests on the margin (offset " + strip.Offset + ")");
            Assert.AreEqual(page0, PageScroll.scrollOffset.y, 1f, "the page did not move");
            yield return Grab("painting_carousel_swiped");

            float strip0 = strip.Offset;
            Assert.Greater(PageScroll.verticalScroller.highValue, page0 + 100f, "precondition: the page can scroll on");
            yield return CardTestInput.TouchDrag(strip.Viewport.panel, strip.Viewport.worldBound.center, new Vector2(6f, -180f), 14);
            yield return CardTestInput.Settle(0.4f);
            Assert.Greater(PageScroll.scrollOffset.y, page0 + 40f, "a real up / down finger on the carousel scrolled the page");
            Assert.AreEqual(strip0, strip.Offset, 0.5f, "...and left the strip");
        }

        // The video: a real tap on its poster opens the card's existing full-screen player over the ONE video owner's playback; Back returns to
        // the page and the video pauses -- in English and Portuguese (the breadcrumb names the video in the card's language)
        [UnityTest]
        public IEnumerator ARealTapOnTheVideoPoster_OpensTheExistingPlayer_AndBackReturnsToThePage([Values("en", "pt")] string language)
        {
            yield return Open(language, CardOptions.ModeLight);
            ComposedBlockView preview = null, page = null;
            yield return Preview(v => preview = v);
            yield return OpenPage(preview, v => page = v);
            var blockKey = Sheet.FullView.BlockKey;
            var video = page.Shown.Select(s => s.Component).OfType<VideoElement>().Single();
            Assert.AreEqual(ElementOptions.TapOpenPlayer, video.Tap, "the shipped row opens the player");
            var track = video.Track;
            var owner = Sheet.Context.Video;
            Assert.IsNotNull(owner, "precondition: the card has its video owner");
            PageScroll.ScrollTo(video.Teaser);
            yield return CardTestInput.Settle(0.2f);
            float offset = PageScroll.scrollOffset.y;
            Assert.IsTrue(PageScroll.contentViewport.worldBound.Contains(video.Teaser.worldBound.center), "precondition: the poster is in view");
            yield return CardTestInput.Tap(video.Teaser.panel, video.Teaser.worldBound.center);
            yield return CardTestInput.Settle(0.3f);

            var takeover = Sheet.Takeover;
            Assert.IsTrue(takeover.IsOpen && !Sheet.FullView.IsOpen, "the player opened in the full-screen view (the page made way)");
            Assert.IsNotNull(takeover.Page.Q(className: "card-video--full"), "the video block's own full-screen player");
            Assert.IsTrue(owner.IsCurrent(track), "the card's ONE video owner holds the story's clip");
            Assert.AreEqual(CardVideoState.Playing, owner.State, "...and plays it");
            StringAssert.Contains(track.Title, takeover.CrumbText, "the breadcrumb names the video in " + language);
            yield return Grab("painting_video_player_" + language);

            yield return CardTestInput.Tap(takeover.Back.panel, takeover.Back.worldBound.center);
            yield return CardTestInput.Settle(0.4f);
            Assert.IsTrue(Sheet.FullView.IsOpen && Sheet.FullView.BlockKey == blockKey, "Back returned to the story's page");
            Assert.AreEqual(offset, PageScroll.scrollOffset.y, 1f, "...at the poster the visitor left");
            Assert.IsTrue(owner.IsCurrent(track) && owner.State == CardVideoState.Paused, "the video paused");
            yield return CardTestInput.Tap(takeover.Back.panel, takeover.Back.worldBound.center);
            yield return CardTestInput.Settle(0.3f);
            Assert.IsFalse(takeover.IsOpen, "a second Back: the card");
        }

        // ---------------- the gated cost of the page's motion ----------------

        private const int Frames = 60;

        private struct Cost
        {
            public double Batches, UiMs;
            public override string ToString() => $"batches {Batches:0.0}, UI cpu {UiMs:0.000} ms";
        }

        // The page scrolled 4 pt a frame (the parallax works every frame) while the counters run
        private IEnumerator Measure(float from, System.Action<Cost> got)
        {
            for (int i = 0; i < 10; i++) yield return null;
            using var batches = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Batches Count");
            using var update = ProfilerRecorder.StartNew(new ProfilerCategory("PlayerLoop"), "PreLateUpdate.UIElementsUpdatePanels");
            using var repaint = ProfilerRecorder.StartNew(new ProfilerCategory("PlayerLoop"), "PostLateUpdate.UIElementsRepaintPanels");
            var cost = new Cost();
            for (int i = 0; i < Frames; i++)
            {
                PageScroll.scrollOffset = new Vector2(0f, from + i * 4f);
                yield return null;
                cost.Batches += batches.LastValue;
                cost.UiMs += (update.LastValue + repaint.LastValue) * 1e-6;
            }
            cost.Batches /= Frames;
            cost.UiMs /= Frames;
            got(cost);
        }

        // Batches and UI Toolkit's CPU on The Painting's page while it scrolls past its full-bleed ruins (parallax) and the pair: the rows'
        // motion running vs the same scroll with the motion let go. Editor numbers rank the two on one machine; a phone's cost is the device
        // check. Written to TestEvidence/Story/cost.txt
        [UnityTest]
        public IEnumerator ThePagesMotion_Cost_WhileItScrolls()
        {
            if (!File.Exists(Path.Combine(TestEvidence.Root, "Story", "MEASURING")))
                Assert.Ignore("A measurement: create TestEvidence/Story/MEASURING to run it");
            yield return Open("en", CardOptions.ModeLight);
            ComposedBlockView preview = null, page = null;
            yield return Preview(v => preview = v);
            yield return OpenPage(preview, v => page = v);
            var ruins = page.Shown.Select(s => s.Component).OfType<PictureElement>().ElementAt(2);
            float from = Mathf.Max(0f, ruins.layout.y - PageScroll.contentViewport.layout.height);
            Assert.Greater(page.Motion.Count, 0, "precondition: the page's rows move");

            Cost moving = default, still = default;
            yield return Measure(from, c => moving = c);
            page.Motion.Unbind();
            yield return Measure(from, c => still = c);
            string note = $"The Painting's story page, {Frames} frames scrolling 4 pt each past the ruins (parallax) and the pair:\n" +
                          $"  motion running ({page.Shown.Count} rows): {moving}\n  motion let go:            {still}\n";
            File.AppendAllText(TestEvidence.PathFor("Story", "cost.txt"), note);
            Debug.Log("[Card] cost " + note.Replace("\n", " | "));
            Assert.Less(moving.Batches, still.Batches + 4, "transform / opacity only: the motion adds no more than a few batches");
        }
    }
}
