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
    // Phase A of the story (_3.3 S2a) on the real gallery card (Assets/Dev/CardGallery), fed CardGalleryDefinitions.Story: every text and
    // structure element x every style option x light / dark on its PAGE (opened by a real tap on its preview) -- inside the 390 pt frame,
    // >= 7:1 on the RENDER, each option drawing what it says (the tone's token colour, sizes in order, the alignment, the drop cap's lines,
    // the numbers, the ornament, the spacer's heights); the PREVIEW capped at --ts-preview-max of the card's full height with the fade and
    // the Open Row (a whole touch target, its words and minutes from their sources), a short story whole; the Open Row's plain / nudge /
    // shine as frame sequences with the brightest frame's ratio; a Custom preview. Every render is judged with the rows' motion settled
    // (ElementMotion.Finish: a reveal starts transparent); the media elements and the motion are CardStoryMediaGalleryTests'. Renders in
    // TestEvidence/Card/Story/.
    public class CardStoryGalleryTests
    {
        private const string ScenePath = "Assets/Dev/CardGallery/CardGalleryScene.unity";
        private CardGalleryHarness _harness;

        private static string[] SheetContents =>
            CardGalleryDefinitions.StorySheetElements.Select(e => CardGalleryDefinitions.StorySheetPrefix + e).ToArray();

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
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            ThemeController.SetMode(CardOptions.ModeLight);
            var scene = SceneManager.GetSceneByPath(ScenePath);
            SceneManager.SetActiveScene(SceneManager.CreateScene("AfterCardStoryGalleryTest"));
            if (scene.IsValid() && scene.isLoaded) yield return SceneManager.UnloadSceneAsync(scene);
        }

        // ---------------- helpers ----------------

        // Show a story entry at the full stop in `mode` and hand back its card view (the preview)
        private IEnumerator ShowStory(string content, string mode, System.Action<ComposedBlockView> got)
        {
            ThemeController.SetMode(mode);
            _harness.Show(CardGalleryDefinitions.All.ToList().FindIndex(e => e.Name == NameOf(content)));
            CardTestInput.HoldTheWallStill(_harness.Sheet);
            IBlockView view = null;
            yield return CardGalleryChecks.SettledBlock(_harness, NameOf(content), v => view = v);
            Assert.IsInstanceOf<ComposedBlockView>(view, content + ": the story kind is drawn by the composed view");
            var composed = (ComposedBlockView)view;
            Assert.IsFalse(composed.IsPage, content + ": on the card it is the preview");
            composed.Motion.Finish();
            got(composed);
        }

        // A REAL tap on the preview's centre opens the page in the full view: the second view, bound as the page
        private IEnumerator OpenPage(ComposedBlockView preview, System.Action<ComposedBlockView> got)
        {
            yield return CardTestInput.TapInReach(_harness.Sheet, preview.Preview);
            yield return CardTestInput.Settle(0.3f);
            var full = _harness.Sheet.FullView;
            Assert.IsTrue(full.IsOpen, "a real tap on the preview opened the full view");
            Assert.IsInstanceOf<ComposedBlockView>(full.View, "the full view holds a second composed view");
            var page = (ComposedBlockView)full.View;
            Assert.IsTrue(page.IsPage, "...bound as the page");
            Assert.AreNotSame(preview, page, "a second view: the card's preview stays bound under it");
            page.Motion.Finish();
            got(page);
        }

        private static IEnumerator Capture(string name, System.Action<Texture2D> use)
        {
            yield return new WaitForEndOfFrame();
            var shot = ScreenCapture.CaptureScreenshotAsTexture();
            try
            {
                File.WriteAllBytes(TestEvidence.PathFor("Card/Story", name + ".png"), shot.EncodeToPNG());
                use?.Invoke(shot);
            }
            finally { Object.Destroy(shot); }
        }

        private static Color Token(VisualElement from, string token)
        {
            Assert.IsTrue(ThemeController.TryResolverFor(from, out var resolver), "the theme resolves on the card");
            Assert.IsTrue(resolver.TryColour(token, out var colour), token + " is a theme colour");
            return colour;
        }

        private static void AssertColour(Color expected, Color actual, string what) =>
            Assert.IsTrue(Mathf.Abs(expected.r - actual.r) < 0.01f && Mathf.Abs(expected.g - actual.g) < 0.01f && Mathf.Abs(expected.b - actual.b) < 0.01f,
                what + ": expected " + (Color32)expected + " but was " + (Color32)actual);

        // The part of a component whose colour the TONE sets (the words of a text; the box's title; the quote's mark)
        private static VisualElement ToneTarget(IElementComponent component) => component switch
        {
            LineElement line => line.Text,
            ProseElement prose => prose.Text.Paragraphs[0],
            ListElement list => list.Items[0].Words,
            CalloutElement callout => callout.Title,
            PullQuoteElement quote => quote.Mark,
            _ => null,
        };

        // The words whose size the SIZE sets
        private static VisualElement SizeTarget(IElementComponent component) => component switch
        {
            LineElement line => line.Text,
            ProseElement prose => prose.Text.Paragraphs[0],
            ListElement list => list.Items[0].Words,
            CalloutElement callout => callout.Text.Paragraphs[0],
            PullQuoteElement quote => quote.Quote.Paragraphs[0],
            _ => null,
        };

        // ---------------- every element x every option x both modes, on the page ----------------

        [UnityTest]
        public IEnumerator EveryElementSheet_OnItsPage_EveryOptionDrawn_FitsAndReadsSevenToOne_InBothModes(
            [ValueSource(nameof(SheetContents))] string content, [Values("light", "dark")] string mode)
        {
            string key = content.Substring(CardGalleryDefinitions.StorySheetPrefix.Length);
            Assert.IsTrue(ElementRegistry.Shared.TryGet(key, out var element));
            var sheetRows = CardGalleryDefinitions.SheetRowsOf(element);
            ComposedBlockView preview = null, page = null;
            yield return ShowStory(content, mode, v => preview = v);
            yield return OpenPage(preview, v => page = v);
            Assert.AreEqual(sheetRows.Count, page.Shown.Count, content + ": one page row per style option (and the defaults)");

            var scroll = _harness.Sheet.FullView.Scroll;
            Rect frame = _harness.Frame.worldBound;
            var sizes = new Dictionary<string, float>();
            var failures = new List<string>();
            int texts = 0;
            for (int i = 0; i < sheetRows.Count; i++)
            {
                var (_, _, component) = page.Shown[i];
                Assert.IsNotNull(component, content + " row " + i + ": drawn by its component");
                string what = content + " " + mode + " row " + i + " (" + (sheetRows[i].Option.Length == 0 ? "defaults" : sheetRows[i].Option + "=" + sheetRows[i].Value) + ")";
                scroll.ScrollTo(component.Root);
                yield return CardTestInput.Settle(0.12f);

                // - every option is a class, and the class draws what it says
                string option = sheetRows[i].Option, value = sheetRows[i].Value;
                foreach (var style in element.StyleFields ?? new BlockFieldDefinition[0])
                {
                    string expected = style.Key == option ? value : style.ChoiceDefault;
                    if (style.Key == BuiltInElements.ToneStyle || style.Key == BuiltInElements.SizeStyle || style.Key == BuiltInElements.AlignStyle)
                        Assert.IsTrue(component.Root.ClassListContains("ts-el--" + style.Key + "-" + expected), what + ": wears ts-el--" + style.Key + "-" + expected);
                }
                var toneTarget = ToneTarget(component);
                if (toneTarget != null && (option == BuiltInElements.ToneStyle || option == ""))
                {
                    string tone = option == BuiltInElements.ToneStyle ? value : element.Field(BuiltInElements.ToneStyle)?.ChoiceDefault;
                    // - the gallery's story is an About block: its family colour; a quote's neutral mark is the line colour, never ink
                    string token = tone switch
                    {
                        BuiltInElements.ToneFamily => "--ts-family-about",
                        BuiltInElements.ToneAccent => "--ts-accent-text",
                        BuiltInElements.ToneInverse => "--ts-surface",
                        _ => component is PullQuoteElement ? "--ts-line" : "--ts-ink",
                    };
                    if (tone != null) AssertColour(Token(toneTarget, token), toneTarget.resolvedStyle.color, what + ": the " + tone + " tone is " + token);
                }
                var sizeTarget = SizeTarget(component);
                if (sizeTarget != null && (option == BuiltInElements.SizeStyle || option == ""))
                    sizes[option == "" ? element.Field(BuiltInElements.SizeStyle)?.ChoiceDefault ?? "" : value] = sizeTarget.resolvedStyle.fontSize;
                if (component is SpacerElement && (option == BuiltInElements.SizeStyle || option == ""))
                    sizes[option == "" ? BuiltInElements.SizeMedium : value] = component.Root.layout.height;
                if (option == BuiltInElements.AlignStyle || (option == "" && element.Field(BuiltInElements.AlignStyle) != null))
                {
                    string align = option == BuiltInElements.AlignStyle ? value : element.Field(BuiltInElements.AlignStyle).ChoiceDefault;
                    var text = (TextElement)SizeTarget(component);
                    Assert.AreEqual(align == BuiltInElements.AlignCentre ? TextAnchor.UpperCenter : TextAnchor.UpperLeft, text.resolvedStyle.unityTextAlign, what + ": aligned " + align);
                }
                if (component is ProseElement prose && prose.HasDropCap)
                {
                    var cap = prose.Text.DropCap;
                    Assert.IsTrue(CardTestInput.IsShown(cap, page.Root) && cap.text.Length == 1, what + ": the raised letter shows");
                    Assert.Greater(prose.Text.FlowBeside.text.Length, 0, what + ": words beside the letter");
                    Assert.Greater(prose.Text.FlowUnder.text.Length, 0, what + ": the rest runs under it");
                    Assert.GreaterOrEqual(prose.Text.FlowBeside.worldBound.xMin, cap.worldBound.xMax - 0.5f, what + ": beside the letter");
                    Assert.AreEqual(cap.worldBound.xMin, prose.Text.FlowUnder.worldBound.xMin, 1f, what + ": the rest starts at the letter's left edge (full width)");
                    Assert.GreaterOrEqual(prose.Text.FlowUnder.worldBound.yMin, prose.Text.FlowBeside.worldBound.yMax - 0.5f, what + ": under the lines beside it");
                }
                if (component is ListElement list)
                {
                    bool numbers = option == BuiltInElements.ListStyle && value == BuiltInElements.ListNumbers;
                    Assert.AreEqual(3, list.Items.Count, what + ": one item per line");
                    for (int n = 0; n < list.Items.Count; n++)
                    {
                        Assert.AreEqual(numbers ? DisplayStyle.Flex : DisplayStyle.None, list.Items[n].Number.resolvedStyle.display, what + ": numbers " + numbers);
                        Assert.AreEqual(numbers ? DisplayStyle.None : DisplayStyle.Flex, list.Items[n].Bullet.resolvedStyle.display, what + ": bullets " + !numbers);
                        if (numbers) Assert.AreEqual((n + 1) + ".", list.Items[n].Number.text);
                    }
                }
                if (component is DividerElement divider)
                {
                    bool ornament = option == BuiltInElements.OrnamentStyle && value == BuiltInElements.OrnamentAzulejo;
                    Assert.AreEqual(ornament, divider.ShowsOrnament, what);
                    Assert.AreEqual(ornament ? DisplayStyle.Flex : DisplayStyle.None, divider.Ornament.resolvedStyle.display, what + ": the ornament");
                    if (ornament) AssertColour(Token(divider, "--ts-family-about"), divider.Ornament.resolvedStyle.color, what + ": the ornament in the family colour");
                    Assert.Greater(divider.LeadingLine.layout.width, 0f, what + ": its line is drawn");
                }

                // - fit: inside the phone frame and the page's own width; then the render, >= 7:1 for every word
                var labels = component.Root.Query<Label>().ToList().Where(l => !string.IsNullOrEmpty(l.text) && CardTestInput.IsShown(l, component.Root)).ToList();
                yield return Capture(content + "_" + mode + "_" + i, shot =>
                {
                    Rect visible = scroll.contentViewport.worldBound;
                    foreach (var label in labels)
                    {
                        texts++;
                        Assert.GreaterOrEqual(label.worldBound.xMin, frame.xMin - 0.5f, what + " '" + label.text + "' starts inside the 390 pt frame");
                        Assert.LessOrEqual(label.worldBound.xMax, frame.xMax + 0.5f, what + " '" + label.text + "' ends inside it");
                        Assert.LessOrEqual(label.worldBound.xMax, page.Page.worldBound.xMax + 0.5f, what + ": inside the page");
                        // - and inside what the full view SHOWS: a page wider than its scroll's viewport is clipped at the sides
                        Assert.GreaterOrEqual(label.worldBound.xMin, visible.xMin - 0.5f, what + " '" + Short(label.text) + "' starts inside the full view's visible page (" + visible + ")");
                        Assert.LessOrEqual(label.worldBound.xMax, visible.xMax + 0.5f, what + " '" + Short(label.text) + "' ends inside it");
                        if (!visible.Contains(label.worldBound.center)) continue;
                        // - a drop cap's letter is drawn deeper than its two-line box by design (the lines beside it set the row): its ground
                        //   is read above, before and after it, never under it, where the letter itself is
                        var judged = visible;
                        if (label.ClassListContains("card-text__dropcap")) judged.yMax = label.worldBound.yMax;
                        var (ratio, at) = CardGalleryChecks.WordsOnRender(shot, label, judged);
                        if (ratio < UIAccessibility.MinRatioEnhancedText) failures.Add(what + " '" + Short(label.text) + "' " + ratio.ToString("0.00") + ":1 at " + at);
                    }
                });
            }
            CollectionAssert.IsEmpty(failures, content + " " + mode + ": every word >= 7:1 on the render (D1)");
            if (element.Key != BuiltInElements.Divider && element.Key != BuiltInElements.Spacer)
                Assert.Greater(texts, 0, content + ": words were judged (not vacuous)");
            if (sizes.Count == 3)
                Assert.IsTrue(sizes[BuiltInElements.SizeSmall] < sizes[BuiltInElements.SizeMedium] && sizes[BuiltInElements.SizeMedium] < sizes[BuiltInElements.SizeLarge],
                    content + ": small < medium < large (" + string.Join(", ", sizes.Select(s => s.Key + " " + s.Value)) + ")");
            else if (element.Field(BuiltInElements.SizeStyle) != null) Assert.Fail(content + ": every size measured (" + sizes.Count + ")");
        }

        private static string Short(string text) => text.Length <= 24 ? text : text.Substring(0, 24);

        // Two translations equal within half a panel unit (the layout's own rounding)
        private sealed class FloatTolerance : System.Collections.IComparer
        {
            private readonly float _tolerance;
            public FloatTolerance(float tolerance) => _tolerance = tolerance;
            public int Compare(object x, object y) => Mathf.Abs((float)x - (float)y) <= _tolerance ? 0 : 1;
        }

        // ---------------- the preview ----------------

        // The words the Open Row must say for an entry: the kind's words and the block's minutes, joined as the card's texts say
        private string ExpectedOpenRowWords(CardGalleryDefinitions.Entry entry)
        {
            var poi = CardGalleryDefinitions.Poi(entry);
            var block = poi.card.blocks.Single(b => b.kind == BuiltInBlocks.StoryKind);
            int minutes = ElementRules.Minutes(ElementScope.Of(poi, block, BlockRegistry.Shared), ElementRegistry.Shared, "en", "en");
            Assert.Greater(minutes, 0, "precondition: the story has words to count");
            var strings = new CardStrings(_harness.StringTable.Entries(), _harness.StringSources.Entries(), null, "en", "en");
            string time = string.Format(strings.Get(CardStrings.Keys.NavContentsMinutes), minutes);
            return string.Format(strings.Get(CardStrings.Keys.OpenRowMeta), strings.Get(CardStrings.Keys.OpenRowStory), time);
        }

        // The magazine is taller than the cap: its rows are cut at --ts-preview-max of the card's full height (the measured full stop) with
        // the fade over the cut, the Open Row last -- a whole touch target saying the kind's words and the counted minutes, >= 7:1 on the
        // render; Automatic draws the Page's picture, eyebrow, title and lede, every text at its smallest size
        [UnityTest]
        public IEnumerator ATallPreview_IsCappedAtHalfTheCard_CutWithAFade_ThenTheOpenRow_InBothModes([Values("light", "dark")] string mode)
        {
            ComposedBlockView preview = null;
            yield return ShowStory(CardGalleryDefinitions.StoryTall, mode, v => preview = v);
            var sheet = _harness.Sheet;
            float full = sheet.FullHeight;
            Assert.Greater(full, 0f, "precondition: the card's full stop is measured");
            Assert.AreEqual(full * 0.5f, preview.Cap, 0.01f, "the cap is --ts-preview-max (0.5) of the card's full height");
            Assert.IsTrue(preview.IsCut, "a preview taller than the cap: cut");
            Assert.LessOrEqual(preview.Root.worldBound.height, preview.Cap + CardGalleryChecks.OnePixel(preview.Root), "the preview is never taller than its cap");
            Assert.Greater(preview.Rows.layout.height, preview.Clip.layout.height + 1f, "precondition: the rows really are taller than what shows");
            Assert.AreEqual(DisplayStyle.Flex, preview.Fade.resolvedStyle.display, "the fade shows over the cut");
            Assert.AreEqual(preview.Clip.worldBound.yMax, preview.Fade.worldBound.yMax, 1f, "...at the foot of what shows");
            Assert.GreaterOrEqual(preview.OpenRow.worldBound.yMin, preview.Clip.worldBound.yMax - 0.5f, "the Open Row comes after the cut");
            Assert.AreEqual(preview.Root.worldBound.yMax, preview.OpenRow.worldBound.yMax, CardGalleryChecks.OnePixel(preview.Root) + 0.5f, "...and ends the preview");
            Assert.IsTrue(UIAccessibility.MeetsMinTapTarget(preview.OpenRow.worldBound.width, preview.OpenRow.worldBound.height),
                "the Open Row is a whole touch target (" + preview.OpenRow.worldBound.size + ")");
            Assert.GreaterOrEqual(preview.OpenRow.worldBound.height, 48f - 0.5f, "48 pt tall at least");
            Assert.AreEqual(ExpectedOpenRowWords(EntryOf(CardGalleryDefinitions.StoryTall)), preview.OpenRow.Words.text, "the Open Row's words + minutes");
            Assert.AreEqual(ElementOptions.EmphasisNudge, preview.OpenRow.Emphasis, "the wall's default emphasis");
            yield return Capture("tall_preview_" + mode, shot =>
            {
                var visible = sheet.Stack.Scroll.contentViewport.worldBound;
                var (ratio, at) = CardGalleryChecks.WordsOnRender(shot, preview.OpenRow.Words, visible);
                Assert.GreaterOrEqual(ratio, UIAccessibility.MinRatioEnhancedText, mode + ": the Open Row's words on the render, at " + at);
            });

            // - the magazine (the prototype's shape): Automatic draws the Page's picture, its eyebrow + title, its first text, every text at
            //   its smallest size; it is cut exactly when its rows are taller than the room above the Open Row
            yield return ShowStory(CardGalleryDefinitions.StoryMagazine, mode, v => preview = v);
            CollectionAssert.AreEqual(new[] { BuiltInElements.Picture, BuiltInElements.Eyebrow, BuiltInElements.Title, BuiltInElements.Lede },
                preview.Shown.Select(s => s.Content.Kind), "Automatic: the Page's picture, its eyebrow + title, its first text");
            Assert.IsInstanceOf<PictureElement>(preview.Shown[0].Component, "the picture drawn by its component");
            // - the review fix (S2b): no slot heading over a composed block -- its own title is its heading
            var heading = _harness.Sheet.Stack.HeadingOf(preview);
            Assert.IsTrue(heading.ClassListContains(BlockStackView.OwnTitleClass) && heading.resolvedStyle.display == DisplayStyle.None,
                "no slot heading above the story's own title");
            foreach (var (_, _, component) in preview.Shown.Where(s => !(s.Component is PictureElement)))
                Assert.IsTrue(component.Root.ClassListContains("ts-el--size-s") && component.Root.ClassListContains(ElementStyle.PreviewClass),
                    component.Root.name + ": an Automatic preview draws every text at its smallest size");
            float room = preview.Cap - preview.OpenRow.layout.height - preview.OpenRow.resolvedStyle.marginTop;
            Assert.AreEqual(preview.Rows.layout.height > room + 0.5f, preview.IsCut, "cut exactly when the rows (" + preview.Rows.layout.height + ") pass the room (" + room + ")");
            yield return Capture("magazine_preview_" + mode, null);
        }

        // A story shorter than the cap shows whole: no cut, no fade -- the Open Row still says there is a page
        [UnityTest]
        public IEnumerator AShortStory_ShowsWhole_WithNoFade_AndItsOpenRow()
        {
            ComposedBlockView preview = null;
            yield return ShowStory(CardGalleryDefinitions.StoryShort, CardOptions.ModeLight, v => preview = v);
            Assert.Greater(preview.Cap, 0f, "precondition: a cap applies");
            Assert.IsFalse(preview.IsCut, "it fits: not cut");
            Assert.AreEqual(preview.Rows.layout.height, preview.Clip.layout.height, 0.5f, "every row shows whole");
            Assert.AreEqual(DisplayStyle.None, preview.Fade.resolvedStyle.display, "no fade");
            Assert.AreEqual(DisplayStyle.Flex, preview.OpenRow.resolvedStyle.display, "the Open Row");
            yield return Capture("short_preview_light", null);
        }

        // A REAL tap on the preview opens the page in the full view: every Page row in order, no Open Row; Back returns to the card at the
        // same stop and scroll, the preview still bound
        [UnityTest]
        public IEnumerator ARealTapOnThePreview_OpensThePage_EveryRowInOrder_AndBackReturnsToTheCard([Values("light", "dark")] string mode)
        {
            ComposedBlockView preview = null, page = null;
            yield return ShowStory(CardGalleryDefinitions.StoryMagazine, mode, v => preview = v);
            var stack = _harness.Sheet.Stack;
            yield return OpenPage(preview, v => page = v);
            float offset = stack.Scroll.scrollOffset.y;
            var entry = EntryOf(CardGalleryDefinitions.StoryMagazine);
            var rows = entry.Block.fields.Single(f => f.key == BuiltInBlocks.StoryPageField).items.Select(r => new ElementRow(r).Kind).ToList();
            CollectionAssert.AreEqual(rows, page.Shown.Select(s => s.Content.Kind), "the page draws every row, in order");
            Assert.AreEqual(DisplayStyle.None, page.Preview.resolvedStyle.display, "the page has no preview, no Open Row");
            Assert.AreEqual(DisplayStyle.Flex, page.Page.resolvedStyle.display);
            yield return Capture("magazine_page_top_" + mode, null);
            var scroll = _harness.Sheet.FullView.Scroll;
            foreach (int at in new[] { 4, 8, rows.Count - 1 })
            {
                scroll.ScrollTo(page.Shown[at].Component?.Root ?? page.Page);
                yield return CardTestInput.Settle(0.15f);
                yield return Capture("magazine_page_" + at + "_" + mode, null);
            }
            var takeover = _harness.Sheet.Takeover;
            yield return CardTestInput.Tap(takeover.Back.panel, takeover.Back.worldBound.center);
            yield return CardTestInput.Settle(0.3f);
            Assert.IsFalse(takeover.IsOpen, "Back closed the page");
            Assert.AreEqual(SheetStopRule.Stop.Full, _harness.Sheet.Stop, "the card at the same stop");
            Assert.AreEqual(offset, stack.Scroll.scrollOffset.y, 1f, "...and scroll");
            Assert.AreSame(preview, stack.BoundViews[1], "the card's preview stayed bound");
        }

        // The Open Row's three emphases over the card's clock: plain never moves; nudge steps the chevron out NudgeFar and back, the words'
        // colour fixed; shine also sweeps a light across the words -- every glyph's tint on every frame >= 7:1 on the block's surface, the
        // brightest frame included (the frames saved for the eye); Reduce Motion draws shine as plain
        [UnityTest]
        public IEnumerator TheOpenRow_PlainNudgeShine_AsFrameSequences_TheBrightestFrameAtSevenToOne([Values("plain", "nudge", "shine")] string emphasis,
            [Values("light", "dark")] string mode)
        {
            ComposedBlockView preview = null;
            yield return ShowStory(CardGalleryDefinitions.StoryEmphasisPrefix + emphasis, mode, v => preview = v);
            var row = preview.OpenRow;
            Assert.AreEqual(emphasis, row.Emphasis, "the block's own override sets the emphasis");
            float time = 0f;
            row.Clock = () => time;
            Color surface = Token(row, "--ts-surface");
            float worst = float.MaxValue, farthest = 0f;
            var tints = new HashSet<Color32>();
            float brightestStray = -1f;
            int brightestFrame = -1;
            const int frames = 12;
            for (int f = 0; f < frames; f++)
            {
                time = f * OpenRowMotionRule.ShinePeriod / frames;
                row.Tick();
                yield return null;
                yield return null;
                farthest = Mathf.Max(farthest, Mathf.Abs(row.Chevron.resolvedStyle.translate.x));
                foreach (var tint in row.LastGlyphTints)
                {
                    tints.Add(tint);
                    float ratio = UIAccessibility.ContrastRatio(tint, surface);
                    worst = Mathf.Min(worst, ratio);
                }
                // - the brightest frame: where the words' tints stray furthest from their own colour
                float stray = row.LastGlyphTints.Length == 0 ? 0f : row.LastGlyphTints.Max(t => Mathf.Abs(((Color)t).grayscale - row.Words.resolvedStyle.color.grayscale));
                if (stray > brightestStray) { brightestStray = stray; brightestFrame = f; }
                yield return Capture("open_row_" + emphasis + "_" + mode + "_" + f.ToString("00"), null);
            }
            Assert.Greater(row.LastGlyphTints.Length, 0, "precondition: the words' glyphs were painted");
            File.AppendAllText(TestEvidence.PathFor("Card/Story", "open_row_contrast.txt"),
                $"{emphasis} {mode}: worst glyph {worst:0.00}:1 over {frames} frames, brightest frame {brightestFrame}, chevron out {farthest:0.0}\n");
            Assert.GreaterOrEqual(worst, UIAccessibility.MinRatioEnhancedText, emphasis + " " + mode + ": every glyph of every frame >= 7:1 on the surface (brightest frame " + brightestFrame + ")");
            if (emphasis == ElementOptions.EmphasisPlain)
            {
                Assert.AreEqual(0f, farthest, 0.01f, "plain: the chevron never moves");
                Assert.AreEqual(1, tints.Count, "plain: one colour for every glyph on every frame");
            }
            else
            {
                // - the nudge runs on its own 2.6 s cycle: sampled at its own keyframes (0, the far step at 70 %, back at 80 %)
                var steps = new List<float>();
                foreach (float share in new[] { 0f, 0.7f, 0.8f })
                {
                    time = share * OpenRowMotionRule.NudgePeriod;
                    row.Tick();
                    yield return null;
                    yield return null;
                    steps.Add(row.Chevron.resolvedStyle.translate.x);
                }
                CollectionAssert.AreEqual(new[] { 0f, OpenRowMotionRule.NudgeFar, 0f }, steps, new FloatTolerance(0.5f),
                    emphasis + ": still, out its full step, back (" + string.Join(", ", steps) + ")");
            }
            if (emphasis == ElementOptions.EmphasisNudge) Assert.AreEqual(1, tints.Count, "nudge: the words never change colour");
            if (emphasis == ElementOptions.EmphasisShine) Assert.Greater(tints.Count, 3, "shine: the light changes the words' tints across the frames");
        }

        // Reduce Motion (the wall's switch): a shining Open Row is drawn plain -- still chevron, one colour
        [UnityTest]
        public IEnumerator UnderReduceMotion_AShiningOpenRow_IsPlain()
        {
            var entry = EntryOf(CardGalleryDefinitions.StoryEmphasisPrefix + ElementOptions.EmphasisShine);
            CardGalleryChecks.ShowEntry(_harness, new CardGalleryDefinitions.Entry(entry.Kind, entry.Variant, entry.Content, entry.Block, entry.Setup,
                wall => wall.card_settings.container.reduce_motion = true));
            IBlockView view = null;
            yield return CardGalleryChecks.SettledBlock(_harness, entry.Name, v => view = v);
            var row = ((ComposedBlockView)view).OpenRow;
            Assert.AreEqual(ElementOptions.EmphasisPlain, row.Emphasis, "Reduce Motion: plain");
            float time = 0f;
            row.Clock = () => time;
            var tints = new HashSet<Color32>();
            for (int f = 0; f < 8; f++)
            {
                time = f * 0.4f;
                row.Tick();
                yield return null;
                yield return null;
                Assert.AreEqual(0f, row.Chevron.resolvedStyle.translate.x, 0.01f, "the chevron stays");
                foreach (var tint in row.LastGlyphTints) tints.Add(tint);
            }
            Assert.AreEqual(1, tints.Count, "the words keep one colour");
        }

        // ---------------- Custom preview, reuse ----------------

        // Custom: the listed rows -- an own row, a link to the Page's title drawn in the LINK's style (large, not the compact size), and a
        // link whose Page row is gone, which draws nothing
        [UnityTest]
        public IEnumerator ACustomPreview_DrawsItsOwnRow_ALinkInItsOwnStyle_AndNothingForABrokenLink()
        {
            ComposedBlockView preview = null;
            yield return ShowStory(CardGalleryDefinitions.StoryCustom, CardOptions.ModeLight, v => preview = v);
            CollectionAssert.AreEqual(new[] { BuiltInElements.Eyebrow, BuiltInElements.Title }, preview.Shown.Select(s => s.Content.Kind),
                "the own row, the linked title; the broken link draws nothing");
            var title = (LineElement)preview.Shown[1].Component;
            Assert.AreEqual("The gate of the old town", title.Text.text.Substring(title.Text.text.IndexOf('>') + 1), "the link shows its Page row's words");
            Assert.IsTrue(title.ClassListContains("ts-el--size-l"), "...in the link's own size (Custom keeps each row's style)");
            Assert.AreEqual("TEASER OF THE PREVIEW ONLY", ((LineElement)preview.Shown[0].Component).Text.text.Substring(((LineElement)preview.Shown[0].Component).Text.text.IndexOf('>') + 1));
            yield return Capture("custom_preview_light", null);
        }

        // The components are pooled: showing the magazine again, and another story, makes no new component of a kind it already made
        [UnityTest]
        public IEnumerator ShowingTheStoryAgain_ReusesItsComponents()
        {
            ComposedBlockView first = null;
            yield return ShowStory(CardGalleryDefinitions.StoryMagazine, CardOptions.ModeLight, v => first = v);
            int made = first.Components.CreatedCount;
            Assert.Greater(made, 0, "precondition: components were made");
            ComposedBlockView again = null;
            yield return ShowStory(CardGalleryDefinitions.StoryMagazine, CardOptions.ModeLight, v => again = v);
            Assert.AreSame(first, again, "the card's pooled view of the kind");
            Assert.AreEqual(made, again.Components.CreatedCount, "no new component for the same rows");
        }
    }
}
