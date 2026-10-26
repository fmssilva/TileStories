using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // Phase B of the story (_3.3 S2a) on the REAL LivingRoomScene with real taps: The Painting's own story block (composed through the POI
    // Editor's config path from its shipped text, EN + PT):
    //   - on its card, its PREVIEW: the Page's eyebrow + title + first text in the card's language, capped at --ts-preview-max of the
    //     card's full height, the Open Row saying the kind's words and the minutes counted from the rows
    //   - a real tap opens its PAGE in the full view: every row in order, the breadcrumb naming the story by its title
    //   - Previous / Next: a real tap on the next sibling's chip turns to it (the story's view leaves), on the story's chip back
    //   - Back returns to the card at the same stop and scroll
    //   - light and dark renders of the preview and the page (TestEvidence/Card/Story/painting_*), for the eye beside the prototype
    public class PoiCardStorySceneTests : SearchSceneFixture
    {
        private const string Painting = "painting";

        private PoiCardSheetView Sheet => Card.Sheet;
        private ScrollView Scroll => Sheet.Stack.Scroll;
        private float OnePixel => Sheet.Layer.panel.visualTree.layout.height / Screen.height;

        // The Painting's story block as the shipped config holds it
        private BlockInstanceData StoryBlock(out POIData painting)
        {
            painting = ConfigCopy().pois.Single(p => p.id == Painting);
            var story = painting.card.blocks.Where(b => b.kind == BuiltInBlocks.StoryKind).ToList();
            Assert.AreEqual(1, story.Count, "precondition: The Painting ships one story block");
            return story[0];
        }

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

        // The story's view on the card, scrolled into view
        private IEnumerator Preview(System.Action<ComposedBlockView> got)
        {
            var block = StoryBlock(out _);
            var view = Sheet.Stack.ViewOfBlock(block.key);
            Assert.IsInstanceOf<ComposedBlockView>(view, "the story block is on The Painting's card, drawn by the composed view");
            Scroll.ScrollTo(view.Root);
            yield return CardTestInput.Settle(0.3f);
            got((ComposedBlockView)view);
        }

        private static IEnumerator Grab(string name)
        {
            yield return new WaitForEndOfFrame();
            var shot = ScreenCapture.CaptureScreenshotAsTexture();
            File.WriteAllBytes(TestEvidence.PathFor("Card/Story", name + ".png"), shot.EncodeToPNG());
            Object.Destroy(shot);
        }

        [UnityTearDown]
        public IEnumerator BackToLight()
        {
            ThemeController.SetMode(CardOptions.ModeLight);
            yield return null;
        }

        // The preview on the real card, in English and in Portuguese, light and dark
        [UnityTest]
        public IEnumerator ThePaintingsStoryPreview_ShowsItsEyebrowTitleAndLede_CappedWithItsOpenRow([Values("en", "pt")] string language,
            [Values("light", "dark")] string mode)
        {
            yield return Open(language, mode);
            ComposedBlockView preview = null;
            yield return Preview(v => preview = v);
            var block = StoryBlock(out var painting);
            var rows = BlockSurfaces.PageRows(block, BuiltInBlocks.Story).Select(r => new ElementRow(r)).ToList();

            CollectionAssert.AreEqual(new[] { BuiltInElements.Eyebrow, BuiltInElements.Title, BuiltInElements.Lede }, preview.Shown.Select(s => s.Content.Kind),
                "Automatic: the eyebrow with its title, the first text (no picture row yet)");
            string Words(ElementRow row) => BlockFieldReader.Pick(row.Field(BuiltInElements.TextField).text, language, "en");
            // - a label's text carries rich-text tags (the leading, glossary marks): the words are what is left without them
            string Shown(string text) => System.Text.RegularExpressions.Regex.Replace(text, "<[^>]+>", "");
            var title = (LineElement)preview.Shown[1].Component;
            Assert.AreEqual(Words(rows.First(r => r.Kind == BuiltInElements.Title)), Shown(title.Text.text), "the title in " + language);
            var lede = (ProseElement)preview.Shown[2].Component;
            Assert.AreEqual(Words(rows.First(r => r.Kind == BuiltInElements.Lede)), Shown(lede.Text.Paragraphs[0].text), "the lede in " + language);

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

        // A real tap opens the page; Previous / Next by real taps on the sibling chips; Back returns to the card unchanged
        [UnityTest]
        public IEnumerator ARealTap_OpensTheStoryPage_EveryRowInOrder_NextAndPreviousTurn_BackReturns([Values("light", "dark")] string mode)
        {
            yield return Open("en", mode);
            ComposedBlockView preview = null;
            yield return Preview(v => preview = v);
            float offset = Scroll.scrollOffset.y;
            var block = StoryBlock(out _);
            yield return CardTestInput.TapInView(Scroll, preview.Preview);
            yield return CardTestInput.Settle(0.3f);

            var full = Sheet.FullView;
            var takeover = Sheet.Takeover;
            Assert.IsTrue(full.IsOpen, "a real tap on the preview opened the full view");
            Assert.AreEqual(block.key, full.BlockKey, "...on the story");
            var page = (ComposedBlockView)full.View;
            Assert.IsTrue(page.IsPage);
            CollectionAssert.AreEqual(BlockSurfaces.PageRows(block, BuiltInBlocks.Story).Select(r => new ElementRow(r).Kind), page.Shown.Select(s => s.Content.Kind),
                "every Page row, in order (11)");
            string title = BlockFieldReader.Pick(new ElementRow(BlockSurfaces.PageRows(block, BuiltInBlocks.Story).First(r => new ElementRow(r).Kind == BuiltInElements.Title))
                .Field(BuiltInElements.TextField).text, "en", "en");
            StringAssert.Contains(title, takeover.CrumbText, "the breadcrumb names the story by its title");
            yield return Grab("painting_page_top_" + mode);
            foreach (int at in new[] { 3, 7, 10 })
            {
                full.Scroll.ScrollTo(page.Shown[at].Component.Root);
                yield return CardTestInput.Settle(0.2f);
                yield return Grab("painting_page_" + at + "_" + mode);
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
    }
}
