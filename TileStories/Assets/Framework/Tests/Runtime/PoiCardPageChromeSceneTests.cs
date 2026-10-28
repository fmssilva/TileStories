using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // The Page Chrome and the edge-to-edge block page (_3.3 S2c) on the REAL LivingRoomScene with real input.
    // Phase A -- the top bar on its own, a title short and long and very long, English and Portuguese, light and dark, at the run's 390 pt frame:
    //   one line whatever the title (every part on the Back arrow's row, nothing past the row's edges), the part that gives way first is the
    //   MIDDLE one, then the point's, the page's own name last and bold; text >= 7:1 on the page; the Back arrow a whole target (44 pt)
    // Phase B -- The Painting's story page: a real tap on Back returns to the card at its stop and scroll; real taps on the foot's Previous /
    //   Next cards turn the page and the cards name the neighbours; the hero picture's left and right PIXELS are the phone's edges while the
    //   text keeps its 16 pt gutter; another block's page (the gallery) wears the same chrome; The Painting's story video plays in place
    public class PoiCardPageChromeSceneTests : SearchSceneFixture
    {
        private const string Painting = "painting";
        private const string Evidence = "Card/PageChrome";

        private PoiCardSheetView Sheet => Card.Sheet;
        private ScrollView PageScroll => Sheet.FullView.Scroll;
        private ScrollView Scroll => Sheet.Stack.Scroll;
        private float OnePixel => Sheet.Layer.panel.visualTree.layout.height / Screen.height;
        private float PanelWidth => Sheet.Layer.panel.visualTree.layout.width;

        private BlockInstanceData StoryBlock() => ConfigCopy().pois.Single(p => p.id == Painting).card.blocks.Single(b => b.kind == BuiltInBlocks.StoryKind);

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

        // The story's page opened by a REAL tap on its preview, its motion settled
        private IEnumerator OpenStoryPage(System.Action<ComposedBlockView, ComposedBlockView> got)
        {
            var block = StoryBlock();
            var preview = (ComposedBlockView)Sheet.Stack.ViewOfBlock(block.key);
            Scroll.ScrollTo(preview.Root);
            yield return CardTestInput.Settle(0.3f);
            preview.Motion.Finish();
            yield return CardTestInput.TapInView(Scroll, preview.Preview);
            yield return CardTestInput.Settle(0.3f);
            Assert.IsTrue(Sheet.FullView.IsOpen && Sheet.FullView.BlockKey == block.key, "a real tap on the preview opened the story's page");
            var page = (ComposedBlockView)Sheet.FullView.View;
            page.Motion.Finish();
            got(preview, page);
        }

        private static IEnumerator Grab(string name)
        {
            yield return new WaitForEndOfFrame();
            var shot = ScreenCapture.CaptureScreenshotAsTexture();
            File.WriteAllBytes(TestEvidence.PathFor(Evidence, name + ".png"), shot.EncodeToPNG());
            Object.Destroy(shot);
        }

        [UnityTearDown]
        public IEnumerator BackToLight()
        {
            ThemeController.SetMode(CardOptions.ModeLight);
            yield return null;
        }

        // ---------------- Phase A: the top bar on its own ----------------

        private const string ShortPoint = "Gate";
        private const string LongPoint = "Ribeira Palace of the Kings and the Court at the Terreiro do Paco";
        private const string VeryLongPoint = "The Royal Palace of the Ribeira, the Kings' Court and the Opera House at the Terreiro do Paco beside the River Tagus";

        private static float Natural(Label label, string text) =>
            Mathf.Ceil(label.MeasureTextSize(text, 0f, VisualElement.MeasureMode.Undefined, 0f, VisualElement.MeasureMode.Undefined).x) + 1f;

        // The bar opened by the takeover itself on a title / section / page name, as a block page would
        private void OpenBar(string point, string section, string name, PageChromeChoice? chrome = null)
        {
            var context = Sheet.Context;
            Sheet.Takeover.Open(point, _ => name, 1, 0, context.Media, (t, c, m, p) => { }, context.Strings, null, null, chrome);
            if (section != null) Sheet.Takeover.ShowSection(section);
        }

        [UnityTest]
        public IEnumerator TheTopBar_IsOneLine_TheMiddlePartGivesWayFirst_TheNameLastAndBold_Readable_ABigTarget(
            [Values("en", "pt")] string language, [Values("light", "dark")] string mode,
            [Values(ShortPoint, LongPoint, VeryLongPoint)] string point)
        {
            yield return Open(language, mode);
            string section = language == "en" ? "Stories and people from the whole history of the river front" : "Historias e pessoas de toda a historia da frente ribeirinha";
            string name = language == "en" ? "Two hundred and fifty years by the river" : "Duzentos e cinquenta anos junto ao rio";
            OpenBar(point, section, name);
            yield return CardTestInput.Settle(0.2f);
            var bar = Sheet.Takeover.Bar;
            Assert.IsTrue(Sheet.Takeover.IsOpen, "precondition: the full view is open");
            Assert.AreEqual(bar.Crumb.worldBound.width, bar.Crumb.layout.width, 1f, "precondition: the bar is laid out at the run's frame");
            float room = bar.Crumb.layout.width;
            Assert.Greater(room, 100f, "precondition: the breadcrumb has room to measure against");

            // - ONE line: every visible part on the Back arrow's row, the row no taller than the arrow's target, nothing past the row's edges
            var parts = new VisualElement[] { bar.Point, bar.Section, bar.Name };
            foreach (var part in parts)
            {
                Assert.IsTrue(part.resolvedStyle.display != DisplayStyle.None, "all three parts are there");
                Assert.AreEqual(bar.Back.worldBound.center.y, part.worldBound.center.y, 2f, part.name + " sits on the Back arrow's line");
                Assert.GreaterOrEqual(part.worldBound.xMin, bar.Crumb.worldBound.xMin - 0.5f, "nothing past the row's left edge");
                Assert.LessOrEqual(part.worldBound.xMax, bar.Crumb.worldBound.xMax + 0.5f, "nothing past the row's right edge");
            }
            Assert.LessOrEqual(bar.Crumb.layout.height, bar.Back.layout.height + 1f, "one line: the row is no taller than the arrow's target");

            // - who gave way: the widths are what the rule says for the measured words, and the ORDER holds
            float arrows = bar.Crumb.Query(className: "ts-page-bar__arrow").ToList().Sum(a => a.layout.width + a.resolvedStyle.marginLeft + a.resolvedStyle.marginRight);
            float pad = bar.Section.resolvedStyle.paddingLeft + bar.Section.resolvedStyle.paddingRight;
            var natural = new[] { Natural(bar.Point, point), Mathf.Max(Natural(bar.SectionWords, section) + pad, bar.Section.resolvedStyle.minWidth.value), Natural(bar.Name, name) };
            float ellipsis = Natural(bar.Point, "...");
            float total = natural.Sum();
            var wide = parts.Select(p => p.layout.width).ToArray();
            if (total + arrows <= room + 0.5f)
                for (int i = 0; i < 3; i++) Assert.AreEqual(natural[i], wide[i], 1.5f, "it fits: part " + i + " whole");
            else
            {
                Assert.LessOrEqual(wide.Sum() + arrows, room + 1.5f, "it was cut to the room");
                Assert.Less(wide[PageCrumbFitRule.Section], natural[PageCrumbFitRule.Section], "the MIDDLE part is cut");
                if (wide[PageCrumbFitRule.Point] < natural[PageCrumbFitRule.Point] - 1.5f)
                    Assert.AreEqual(Mathf.Max(ellipsis + pad, bar.Section.resolvedStyle.minWidth.value), wide[PageCrumbFitRule.Section], 2f,
                        "the point is cut only once the middle part is down to its floor (its ellipsis, a whole touch target)");
                if (wide[PageCrumbFitRule.Name] < natural[PageCrumbFitRule.Name] - 1.5f)
                    Assert.AreEqual(ellipsis, wide[PageCrumbFitRule.Point], 2f, "the name is cut only once the point is down to its ellipsis too");
            }

            Assert.AreEqual(FontStyle.Bold, bar.Name.resolvedStyle.unityFontStyleAndWeight, "the page's own name is the bold last part");
            Assert.AreNotEqual(FontStyle.Bold, bar.Point.resolvedStyle.unityFontStyleAndWeight);
            foreach (var part in new[] { bar.Point, bar.SectionWords, bar.Name })
                Assert.GreaterOrEqual(CardTestInput.Contrast(part.resolvedStyle.color, CardTestInput.EffectiveBackground(part)), 7f, part.text + ": text >= 7:1 on the page");
            Assert.IsTrue(UIAccessibility.MeetsMinTapTarget(bar.Back.worldBound.width, bar.Back.worldBound.height), "Back is a whole target (>= 44)");
            Assert.IsTrue(UIAccessibility.MeetsMinTapTarget(bar.Section.worldBound.width, bar.Section.worldBound.height), "the section stays a whole target, even cut: " + bar.Section.worldBound);
            Assert.LessOrEqual(bar.Back.worldBound.width, 48.5f, "...the prototype's 44 pt round target, not the 48 pt pill");
            Assert.AreEqual(bar.Back.worldBound.width, bar.Back.worldBound.height, 0.5f, "round: as wide as tall");
            Assert.AreEqual(Sheet.Context.Strings.Get(CardStrings.Keys.PageBack), bar.Back.tooltip, "its accessible name, in " + language);
            Assert.AreEqual("", bar.Back.text, "the arrow draws no word");
            yield return Grab("bar_" + language + "_" + mode + "_" + (point == ShortPoint ? "short" : point == LongPoint ? "long" : "verylong"));
            Sheet.Takeover.Close();
        }

        [UnityTest]
        public IEnumerator TheTopBar_WithTheBreadcrumbOff_IsTheBackArrowAlone_AndWithNoSectionTheTrailHasTwoParts()
        {
            yield return Open("en", CardOptions.ModeLight);
            OpenBar(ShortPoint, null, "A block", new PageChromeChoice(false, true));
            yield return CardTestInput.Settle(0.2f);
            var bar = Sheet.Takeover.Bar;
            Assert.AreEqual(DisplayStyle.None, bar.Crumb.resolvedStyle.display, "no breadcrumb");
            Assert.IsTrue(bar.Back.worldBound.width >= 44f, "the Back arrow stands alone, still a whole target");
            Sheet.Takeover.Close();

            OpenBar(ShortPoint, null, "A block");
            yield return CardTestInput.Settle(0.2f);
            Assert.AreEqual("Gate > A block", Sheet.Takeover.CrumbText, "no section: the point, then the block");
            Assert.AreEqual(DisplayStyle.None, bar.Section.resolvedStyle.display);
            Sheet.Takeover.Close();
        }

        // ---------------- Phase B: The Painting ----------------

        [Category("Core")]
        [UnityTest]
        public IEnumerator TheStoryPage_ARealTapOnBack_ReturnsToTheCard_AtTheSameStopAndScroll_TheBarReadsOnOneLine(
            [Values("en", "pt")] string language, [Values("light", "dark")] string mode)
        {
            yield return Open(language, mode);
            ComposedBlockView preview = null, page = null;
            var block = StoryBlock();
            preview = (ComposedBlockView)Sheet.Stack.ViewOfBlock(block.key);
            Scroll.ScrollTo(preview.Root);
            yield return CardTestInput.Settle(0.3f);
            float offset = Scroll.scrollOffset.y;
            yield return OpenStoryPage((p, g) => { preview = p; page = g; });

            var takeover = Sheet.Takeover;
            var bar = takeover.Bar;
            var rows = BlockSurfaces.PageRows(block, BuiltInBlocks.Story).Select(r => new ElementRow(r)).ToList();
            string title = BlockFieldReader.Pick(rows.First(r => r.Kind == BuiltInElements.Title).Field(BuiltInElements.TextField).text, language, "en");
            // - the story shows its own big title, so the trail is the context alone: point > section (the S2c review)
            Assert.IsTrue(takeover.SectionShown, "precondition: the story belongs to a section of the card's map");
            Assert.AreEqual(Sheet.Stack.HeaderView.TitleText + " > " + bar.SectionWords.text, takeover.CrumbText,
                "the breadcrumb is the point and the section, nothing after");
            StringAssert.DoesNotContain(title, takeover.CrumbText, "the story's title is not repeated: the page shows it in large type");
            Assert.AreEqual(DisplayStyle.None, bar.Name.resolvedStyle.display, "the name part is not drawn");
            Assert.AreEqual(bar.Back.worldBound.center.y, bar.Section.worldBound.center.y, 2f, "one line: the trail is on the arrow's row");
            Assert.LessOrEqual(bar.Crumb.layout.height, bar.Back.layout.height + 1f);
            yield return Grab("story_page_top_" + language + "_" + mode);

            yield return CardTestInput.Tap(bar.Back.panel, bar.Back.worldBound.center);
            yield return CardTestInput.Settle(0.3f);
            Assert.IsFalse(takeover.IsOpen, "a real tap on the Back arrow closed the page");
            Assert.AreEqual(SheetStopRule.Stop.Full, Sheet.Stop, "the card at the same stop");
            Assert.AreEqual(offset, Scroll.scrollOffset.y, 1f, "...and the same scroll");
            Assert.AreSame(preview, Sheet.Stack.ViewOfBlock(block.key), "the preview stayed bound");
        }

        [Category("Core")]
        [UnityTest]
        public IEnumerator TheFoot_RealTapsOnNextAndPrevious_TurnThePage_AndTheCardsNameTheNeighbours([Values("en", "pt")] string language,
            [Values("light", "dark")] string mode)
        {
            yield return Open(language, mode);
            ComposedBlockView page = null;
            yield return OpenStoryPage((_, g) => page = g);
            var takeover = Sheet.Takeover;
            var foot = Sheet.FullView.Foot;
            var story = Sheet.FullView.BlockKey;
            Assert.Greater(takeover.PageCount, 1, "precondition: the story opens among its section's blocks");

            PageScroll.scrollOffset = new Vector2(0f, PageScroll.verticalScroller.highValue);
            yield return CardTestInput.Settle(0.3f);
            page.Motion.Finish();
            Assert.IsTrue(foot.worldBound.yMax <= PageScroll.contentViewport.worldBound.yMax + 1f && foot.worldBound.yMin >= PageScroll.contentViewport.worldBound.yMin
                          && foot.worldBound.height > 40f,
                "precondition: the foot is at the page's end, in view -- foot " + foot.worldBound + ", viewport " + PageScroll.contentViewport.worldBound +
                ", offset " + PageScroll.scrollOffset.y + " of " + PageScroll.verticalScroller.highValue + ", content " + PageScroll.contentContainer.layout.height +
                ", page " + page.Root.worldBound + ", previous visible " + foot.Previous.visible + ", next visible " + foot.Next.visible);
            int at = takeover.PageIndex;
            var strings = Sheet.Context.Strings;
            Assert.AreEqual(strings.Get(CardStrings.Keys.PagePrevious), foot.PreviousLabel.text);
            Assert.AreEqual(strings.Get(CardStrings.Keys.PageNext), foot.NextLabel.text);
            Assert.IsTrue(UIAccessibility.MeetsMinTapTarget(foot.Next.worldBound.width, foot.Next.worldBound.height), "a foot card is a whole target");
            yield return Grab("story_page_foot_" + language + "_" + mode);

            if (at + 1 < takeover.PageCount)
            {
                string expectedNext = takeover.NameOf(at + 1);
                Assert.AreEqual(expectedNext, foot.NextName.text, "Next names the block after");
                yield return CardTestInput.Tap(foot.Next.panel, foot.Next.worldBound.center);
                yield return CardTestInput.Settle(0.3f);
                Assert.AreEqual(at + 1, takeover.PageIndex, "a real tap on Next turned the page");
                Assert.AreNotEqual(story, Sheet.FullView.BlockKey, "...to the next block");
                Assert.AreEqual(takeover.NameOf(at), foot.PreviousName.text, "its Previous names the story just left");
                PageScroll.scrollOffset = new Vector2(0f, PageScroll.verticalScroller.highValue);
                yield return CardTestInput.Settle(0.3f);
                yield return CardTestInput.Tap(foot.Previous.panel, foot.Previous.worldBound.center);
                yield return CardTestInput.Settle(0.3f);
                Assert.AreEqual(at, takeover.PageIndex, "a real tap on Previous turned back");
                Assert.AreEqual(story, Sheet.FullView.BlockKey, "...to the story");
            }
            else
            {
                Assert.IsFalse(foot.Next.visible, "the last page has no Next card");
                yield return CardTestInput.Tap(foot.Previous.panel, foot.Previous.worldBound.center);
                yield return CardTestInput.Settle(0.3f);
                Assert.AreEqual(at - 1, takeover.PageIndex, "a real tap on Previous turned the page");
            }
        }

        // The pixels the screen shows at a panel point, averaged over a short column so a picture's texture does not decide it
        private static IEnumerator ScreenColumn(VisualElement inPanel, float panelX, float panelY, System.Action<Color> got)
        {
            yield return new WaitForEndOfFrame();
            var tex = ScreenCapture.CaptureScreenshotAsTexture();
            try
            {
                var origin = RuntimePanelUtils.ScreenToPanel(inPanel.panel, Vector2.zero);
                float unit = RuntimePanelUtils.ScreenToPanel(inPanel.panel, Vector2.right).x - origin.x;
                int x = Mathf.Clamp(Mathf.RoundToInt((panelX - origin.x) / unit), 0, tex.width - 1);
                int y0 = tex.height - 1 - Mathf.RoundToInt((panelY - origin.y) / unit);
                Color sum = Color.black;
                int n = 0;
                for (int dy = -8; dy <= 8; dy++)
                {
                    sum += tex.GetPixel(x, Mathf.Clamp(y0 + dy, 0, tex.height - 1));
                    n++;
                }
                got(sum / n);
            }
            finally
            {
                Object.Destroy(tex);
            }
        }

        private static float Distance(Color a, Color b) => Mathf.Abs(a.r - b.r) + Mathf.Abs(a.g - b.g) + Mathf.Abs(a.b - b.b);

        [UnityTest]
        public IEnumerator AFullBleedPicture_ReachesThePhonesEdges_WhileTheTextKeepsItsGutter()
        {
            yield return Open("en", CardOptions.ModeLight);
            ComposedBlockView page = null;
            yield return OpenStoryPage((_, g) => page = g);
            float width = PanelWidth;
            Assert.AreEqual(390f, width, 0.5f, "precondition: the run's phone frame");

            // - the palace front is inset; the ruins (the third picture) are full bleed
            var bleeding = page.Shown.Select(s => s.Component).OfType<PictureElement>().Where(p => p.Root.ClassListContains("ts-el--width-full-bleed")).ToList();
            Assert.GreaterOrEqual(bleeding.Count, 1, "precondition: the story holds a full-bleed picture row");
            var hero = bleeding[0];
            PageScroll.ScrollTo(hero.Root);
            yield return CardTestInput.Settle(0.3f);
            page.Motion.Finish();
            var frame = hero.Frame.Root;
            Assert.AreEqual(0f, frame.worldBound.xMin, 1f, "the picture frame starts at the phone left edge; scroll " + PageScroll.worldBound.xMin + ", content " + PageScroll.contentContainer.worldBound.xMin + ", composed " + page.Root.worldBound.xMin + ", page " + page.Page.worldBound.xMin + ", row " + hero.Root.worldBound.xMin + ", frame " + frame.worldBound.xMin + "; content padding " + PageScroll.contentContainer.resolvedStyle.paddingLeft + ", composed margin " + page.Root.resolvedStyle.marginLeft + ", row margin " + hero.Root.resolvedStyle.marginLeft + ", page padding " + page.Page.resolvedStyle.paddingLeft);
            Assert.AreEqual(width, frame.worldBound.xMax, 1f, "...and ends at its right edge");
            Assert.IsNotNull(hero.Frame.Image.Texture, "precondition: the picture is loaded");

            // - the PIXELS: the screen's first and last columns are the picture's, not the page's ground
            float y = Mathf.Clamp(frame.worldBound.center.y, PageScroll.contentViewport.worldBound.yMin + 20f, PageScroll.contentViewport.worldBound.yMax - 20f);
            Color left = default, leftIn = default, right = default, rightIn = default;
            yield return ScreenColumn(frame, 0.5f, y, c => left = c);
            yield return ScreenColumn(frame, 8f, y, c => leftIn = c);
            yield return ScreenColumn(frame, width - 0.5f, y, c => right = c);
            yield return ScreenColumn(frame, width - 8f, y, c => rightIn = c);
            var ground = CardTestInput.EffectiveBackground(Sheet.Takeover.Root);
            Assert.Greater(Distance(left, ground), 0.12f, "the left-most pixels are not the page's ground (they are the picture's)");
            Assert.Greater(Distance(right, ground), 0.12f, "the right-most pixels are not the page's ground");
            Assert.Less(Distance(left, leftIn), Distance(left, ground), "the left edge looks like the picture beside it, not like the ground");
            Assert.Less(Distance(right, rightIn), Distance(right, ground), "the right edge looks like the picture beside it");
            yield return Grab("story_bleed");

            // - the text keeps the 16 pt gutter: the first title's words start there, the page's scroll reaches the edges
            PageScroll.scrollOffset = Vector2.zero;
            yield return CardTestInput.Settle(0.2f);
            var title = (LineElement)page.Shown.First(s => s.Content.Kind == BuiltInElements.Title).Component;
            float gutter = CardTestInput.TokenPx("--ts-space-4");
            Assert.AreEqual(gutter, title.Text.worldBound.xMin, 1f, "the text starts at the 16 pt gutter");
            Assert.AreEqual(0f, PageScroll.worldBound.xMin, 1f, "the page's scroll starts at the phone's left edge");
            Assert.AreEqual(width, PageScroll.worldBound.xMax, 1f, "...and ends at its right edge");
            Assert.AreEqual(width - gutter, title.Text.worldBound.xMax, 2f, "the text's right margin is the gutter too");
        }

        [UnityTest]
        public IEnumerator AnotherBlocksPage_TheGallery_WearsTheSameChrome_AndKeepsItsOwnGutter([Values("en", "pt")] string language,
            [Values("light", "dark")] string mode)
        {
            yield return Open(language, mode);
            string gallery = ConfigCopy().pois.Single(p => p.id == Painting).card.blocks.First(b => b.kind == BuiltInBlocks.GalleryKind).key;
            Sheet.OpenFullView(gallery);
            yield return CardTestInput.Settle(0.4f);
            var takeover = Sheet.Takeover;
            Assert.IsTrue(Sheet.FullView.IsOpen && Sheet.FullView.BlockKey == gallery, "precondition: the gallery's page is open");
            var bar = takeover.Bar;
            Assert.AreEqual("", bar.Back.text, "no 'Back' word on any block page");
            Assert.AreEqual(bar.Back.worldBound.center.y, bar.Name.worldBound.center.y, 2f, "one line on the gallery's page too");
            Assert.LessOrEqual(bar.Crumb.layout.height, bar.Back.layout.height + 1f);
            Assert.IsTrue(UIAccessibility.MeetsMinTapTarget(bar.Back.worldBound.width, bar.Back.worldBound.height));
            // - a block that is not composed keeps the 16 pt gutter for its content
            var view = Sheet.FullView.View.Root;
            Assert.GreaterOrEqual(view.worldBound.xMin, 15f, "the gallery's content starts at the gutter, not at the phone's edge");
            Assert.AreEqual(0f, PageScroll.worldBound.xMin, 1f, "its scroll reaches the phone's edge like every block page's");
            yield return Grab("gallery_page_top_" + language + "_" + mode);
            PageScroll.scrollOffset = new Vector2(0f, PageScroll.verticalScroller.highValue);
            yield return CardTestInput.Settle(0.3f);
            if (Sheet.FullView.Foot.hierarchy.parent != null) yield return Grab("gallery_page_foot_" + language + "_" + mode);
            yield return CardTestInput.Tap(bar.Back.panel, bar.Back.worldBound.center);
            yield return CardTestInput.Settle(0.3f);
            Assert.IsFalse(takeover.IsOpen, "a real tap on Back closed it");
        }

        // The Painting's story video row: a tap on its poster plays it IN PLACE (S2b review 5) -- the page stays, the card's ONE video owner
        // plays it, no full-screen player opens
        [UnityTest]
        public IEnumerator TheStoryVideo_ARealTapOnThePoster_PlaysInPlace_NoPlayerOpens()
        {
            yield return Open("en", CardOptions.ModeLight);
            ComposedBlockView page = null;
            yield return OpenStoryPage((_, g) => page = g);
            var blockKey = Sheet.FullView.BlockKey;
            var video = page.Shown.Select(s => s.Component).OfType<VideoElement>().Single();
            Assert.AreEqual(ElementOptions.TapPlayInPlace, video.Tap, "the shipped row plays in place");
            var owner = Sheet.Context.Video;
            Assert.IsNotNull(owner, "precondition: the card has its video owner");
            PageScroll.ScrollTo(video.Teaser);
            yield return CardTestInput.Settle(0.2f);
            Assert.IsTrue(PageScroll.contentViewport.worldBound.Contains(video.Teaser.worldBound.center), "precondition: the poster is in view");
            yield return CardTestInput.Tap(video.Teaser.panel, video.Teaser.worldBound.center);
            yield return CardTestInput.Settle(0.3f);

            Assert.IsTrue(video.PlaysInPlace, "the clip plays where the poster was");
            Assert.IsTrue(owner.IsCurrent(video.Track), "the card's ONE video owner holds the story's clip");
            Assert.AreEqual(CardVideoState.Playing, owner.State, "...and plays it");
            Assert.IsTrue(Sheet.FullView.IsOpen && Sheet.FullView.BlockKey == blockKey, "the page stays open");
            Assert.IsNull(Sheet.Takeover.Page.Q(className: "card-video--full"), "no full-screen player opened");
            yield return Grab("story_video_in_place");
            yield return CardTestInput.Tap(Sheet.Takeover.Back.panel, Sheet.Takeover.Back.worldBound.center);
            yield return CardTestInput.Settle(0.3f);
            Assert.IsFalse(Sheet.Takeover.IsOpen, "Back closed the page");
            Assert.AreNotEqual(CardVideoState.Playing, owner.State, "leaving the page paused the clip");
        }

        // The S2c review (5): a video playing in place keeps a full-screen button; a real tap on it opens the card's player over the SAME
        // playback (still the story's clip, still playing), and Back returns to the story's page
        [UnityTest]
        public IEnumerator TheStoryVideo_PlayingInPlace_ARealTapOnFullScreen_OpensTheCardsPlayer_OnTheSamePlayback_BackReturnsToThePage()
        {
            yield return Open("en", CardOptions.ModeLight);
            ComposedBlockView page = null;
            yield return OpenStoryPage((_, g) => page = g);
            var blockKey = Sheet.FullView.BlockKey;
            var video = page.Shown.Select(s => s.Component).OfType<VideoElement>().Single();
            var owner = Sheet.Context.Video;
            PageScroll.ScrollTo(video.Teaser);
            yield return CardTestInput.Settle(0.2f);
            yield return CardTestInput.Tap(video.Teaser.panel, video.Teaser.worldBound.center);
            yield return CardTestInput.Settle(0.3f);
            Assert.IsTrue(video.PlaysInPlace, "precondition: the clip plays in place");
            var track = video.Track;
            var button = video.Panel.FullScreenButton;
            Assert.AreEqual(DisplayStyle.Flex, button.resolvedStyle.display, "the in-place player shows its full-screen button");
            // - the player's controls sit under its picture: the visitor scrolls them into view first
            PageScroll.ScrollTo(button);
            yield return CardTestInput.Settle(0.2f);
            Assert.IsTrue(PageScroll.contentViewport.worldBound.Contains(button.worldBound.center), "precondition: the button is in view (" +
                button.worldBound + " in " + PageScroll.contentViewport.worldBound + ")");

            yield return CardTestInput.Tap(button.panel, button.worldBound.center);
            yield return CardTestInput.Settle(0.3f);
            Assert.IsNotNull(Sheet.Takeover.Page.Q(className: "card-video--full"), "the card's full-screen player opened");
            Assert.IsTrue(owner.IsCurrent(track), "on the same clip");
            Assert.AreEqual(CardVideoState.Playing, owner.State, "still playing: the same playback, not a new start");
            yield return Grab("story_video_full_screen");

            yield return CardTestInput.Tap(Sheet.Takeover.Back.panel, Sheet.Takeover.Back.worldBound.center);
            yield return CardTestInput.Settle(0.3f);
            Assert.IsTrue(Sheet.FullView.IsOpen && Sheet.FullView.BlockKey == blockKey, "Back returns to the story's page");
            Assert.AreNotEqual(CardVideoState.Playing, owner.State, "leaving the player paused the clip");
        }
    }
}
