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
    // Phase A of the POI Detail Card (_3.1 section 7, 40-testing.md 4.4): the real PoiCardSheetView + header view in
    // the isolated gallery scene (Assets/Dev/CardGallery), fed CardGalleryDefinitions -- the same list the harness
    // shows. Per entry: it builds, sits at its stop, the half stop stays within 40%, the peek shows the whole title
    // and chip, no text runs out of the phone-width frame or under the X, text contrast >= 4.5:1, the X >= 44x44,
    // and a render Card_<entry>.png for the vision pass. Plus the sheet's own gestures on the gallery card.
    public class CardGalleryTests
    {
        private const string ScenePath = "Assets/Dev/CardGallery/CardGalleryScene.unity";
        private CardGalleryHarness _harness;

        private static string[] HeaderEntryNames => CardGalleryDefinitions.All.Where(e => e.IsHeader).Select(e => e.Name).ToArray();
        private static string[] BlockEntryNames => CardGalleryDefinitions.All.Where(e => !e.IsHeader).Select(e => e.Name).ToArray();

        private static int IndexOf(string name) => CardGalleryDefinitions.All.ToList().FindIndex(e => e.Name == name);

        // One screen pixel in panel units: layout snaps each edge to a physical pixel, so a width and a height set by the
        // same token may differ by up to that much (the honest tolerance whatever the Game view's size)
        private static float OnePixel(VisualElement e) =>
            RuntimePanelUtils.ScreenToPanel(e.panel, Vector2.right).x - RuntimePanelUtils.ScreenToPanel(e.panel, Vector2.zero).x;

        // Show a block entry and scroll its block into view; returns the block's view
        private IEnumerator ShowBlock(string name, System.Action<IBlockView> got)
        {
            _harness.Show(IndexOf(name));
            yield return CardTestInput.Settle();
            var views = _harness.Sheet.Stack.BoundViews;
            Assert.AreEqual(2, views.Count, name + ": the header + the block");
            // - a footer block (sticky) is pinned outside the scroll: nothing to scroll to
            if (_harness.Sheet.Stack.SlotOf(views[1]).parent == _harness.Sheet.Stack.Scroll.contentContainer) _harness.Sheet.Stack.Scroll.ScrollTo(_harness.Sheet.Stack.SlotOf(views[1]));
            yield return CardTestInput.Settle(0.15f);
            got(views[1]);
        }

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
            // - the harness configures the static marker palettes for the status entries, like a wall does
            StatusRamp.ResetToDefaults();
            CategoryPalette.ClearOverrides();
            MarkerHierarchyResolver.ResetToDefaults();
            var scene = SceneManager.GetSceneByPath(ScenePath);
            SceneManager.SetActiveScene(SceneManager.CreateScene("AfterCardGalleryTest"));
            if (scene.IsValid() && scene.isLoaded) yield return SceneManager.UnloadSceneAsync(scene);
        }

        [UnityTest]
        public IEnumerator EveryHeaderEntry_BuildsFitsReadsAndRenders([ValueSource(nameof(HeaderEntryNames))] string name)
        {
            int index = CardGalleryDefinitions.All.ToList().FindIndex(e => e.Name == name);
            var entry = CardGalleryDefinitions.All[index];
            _harness.Show(index);
            yield return CardTestInput.Settle();

            var sheet = _harness.Sheet;
            var header = (HeaderBlockView)sheet.Stack.BoundViews[0];
            Assert.AreEqual(entry.Stop, sheet.Stop, "rests at its stop");
            // - layout snaps to physical pixels: one screen pixel in panel units is the honest tolerance
            float onePixel = RuntimePanelUtils.ScreenToPanel(sheet.Root.panel, Vector2.right).x - RuntimePanelUtils.ScreenToPanel(sheet.Root.panel, Vector2.zero).x;
            Assert.AreEqual(sheet.Stops.HeightOf(entry.Stop), sheet.Root.resolvedStyle.height, onePixel + 0.01f, "the sheet is that stop's height once the animation ends");
            Assert.LessOrEqual(sheet.Stops.Half, sheet.Layer.layout.height * CardContainerSettings.HalfMaxRatioMax + 0.5f, "half is at most 40% of the screen");
            Assert.Greater(sheet.Stops.Peek, 0f, "peek was measured");

            // content
            Assert.AreEqual(entry.Title, header.TitleText);
            Assert.AreEqual("Civic Buildings", header.ChipText, "the chip names the category (by name, never its key) and no level: Show Level is off");
            bool subtitleExpected = entry.Variant != BuiltInBlocks.HeaderCompact && entry.Subtitle.Length > 0;
            Assert.AreEqual(subtitleExpected, header.SubtitleShown, "compact never shows a subtitle; every other look does");
            // - the picture looks: a hero first in the scroll, its picture really loaded; without a picture, the text-only look
            bool pictureLook = System.Array.IndexOf(BuiltInBlocks.HeaderImageVariants, entry.Variant) >= 0;
            bool heroExpected = pictureLook && entry.Content != CardGalleryDefinitions.NoPicture;
            Assert.AreEqual(heroExpected, header.HasHero, "a hero exactly for a picture look that has its picture");
            if (heroExpected)
            {
                Assert.AreEqual(0, sheet.Stack.Scroll.contentContainer.IndexOf(header.HeroPart), "the hero opens what scrolls, under the pinned title");
                Assert.IsNotNull(header.Picture.Texture, "the picture was loaded (lazily, on bind)");
                Assert.AreEqual(1, _harness.Media.RefCount(header.Picture.Path), "...once, through the header's own media scope");
            }
            else Assert.IsNull(header.HeroPart.parent, "no hero on the card");

            // fit: inside the phone-width frame, the title clear of the X
            Rect frame = _harness.Frame.worldBound, card = sheet.Root.worldBound;
            Assert.AreEqual(frame.width, card.width, 0.5f, "the sheet spans the phone-width frame");
            var title = header.Root.Q<Label>("card-header-title");
            Assert.LessOrEqual(title.worldBound.xMax, sheet.CloseButton.worldBound.xMin + 0.5f, "the title wraps before the X");
            Assert.LessOrEqual(header.PeekPart.worldBound.yMax, card.yMin + sheet.Stops.Peek + 0.5f, "peek shows the whole title and chip");
            if (entry.Stop != SheetStopRule.Stop.Peek && subtitleExpected)
            {
                var subtitle = header.Root.Q<Label>("card-header-subtitle");
                Assert.LessOrEqual(subtitle.worldBound.xMax, card.xMax + 0.5f, "the subtitle wraps inside the card");
                // - half never passes 40%, so a long header may continue below it: half shows the subtitle start, full all of it
                Assert.Less(subtitle.worldBound.yMin, card.yMax, "the subtitle starts inside the card at " + entry.Stop);
                if (entry.Stop == SheetStopRule.Stop.Full)
                    Assert.LessOrEqual(subtitle.worldBound.yMax, card.yMax + 0.5f, "the whole subtitle is visible at full");
            }

            // accessibility: contrast from the resolved (token) colours, the X's size
            Color surface = sheet.Root.resolvedStyle.backgroundColor;
            Assert.GreaterOrEqual(CardTestInput.Contrast(title.resolvedStyle.color, surface), UIAccessibility.MinRatioNormalText, "title on the card");
            var chip = header.Root.Q<Label>("card-header-chip");
            Assert.GreaterOrEqual(CardTestInput.Contrast(chip.resolvedStyle.color, chip.resolvedStyle.backgroundColor), UIAccessibility.MinRatioNormalText, "chip text on the chip");
            var sub = header.Root.Q<Label>("card-header-subtitle");
            Assert.GreaterOrEqual(CardTestInput.Contrast(sub.resolvedStyle.color, surface), UIAccessibility.MinRatioNormalText, "subtitle (ink-2) on the card");
            Assert.IsTrue(UIAccessibility.MeetsMinTapTarget(sheet.CloseButton.worldBound.width, sheet.CloseButton.worldBound.height),
                "the X is " + sheet.CloseButton.worldBound.size + ", below 44x44");
            Assert.AreEqual("Close", sheet.CloseButton.tooltip, "the X is named from the framework string table");
            foreach (var bar in sheet.CloseButton.Query(className: "poi-card__close-bar").ToList())
            {
                Assert.IsTrue(sheet.CloseButton.worldBound.Contains(bar.worldBound.center), "the X's bars cross inside the button");
                Assert.GreaterOrEqual(UIAccessibility.ContrastRatio(bar.resolvedStyle.backgroundColor, sheet.CloseButton.resolvedStyle.backgroundColor),
                    UIAccessibility.MinRatioLargeTextOrUIComponent, "the X reads against its button (3:1, a UI component)");
            }

            yield return Render("Card_" + entry.Name);
        }

        // Every block entry, whatever its kind: it builds at the full stop, every shown text sits inside the phone-width
        // card, reads against what is really behind it (>= 4.5:1, 3:1 for large text), every tap target is >= 44x44,
        // and a render Card_<kind>_<variant>_<content>.png is saved for the vision pass
        [UnityTest]
        public IEnumerator EveryBlockEntry_BuildsFitsReadsAndRenders([ValueSource(nameof(BlockEntryNames))] string name)
        {
            var entry = CardGalleryDefinitions.All[IndexOf(name)];
            IBlockView view = null;
            yield return ShowBlock(name, v => view = v);
            var sheet = _harness.Sheet;
            Assert.AreEqual(SheetStopRule.Stop.Full, sheet.Stop);
            Assert.AreEqual(BlockRegistry.Shared.CreateView(entry.Kind).GetType(), view.GetType(), "the entry's kind drew it");
            Assert.Greater(view.Root.worldBound.height, 0f, "the block takes space on the card");
            Assert.AreEqual(DisplayStyle.None, sheet.Stack.Scroll.verticalScroller.resolvedStyle.display, "no desktop scroll bar on the phone card");

            Rect card = sheet.Root.worldBound;
            int texts = 0;
            foreach (var label in view.Root.Query<Label>().ToList())
            {
                if (string.IsNullOrEmpty(label.text) || !CardTestInput.IsShown(label, view.Root)) continue;
                texts++;
                string what = name + " '" + label.text.Substring(0, System.Math.Min(24, label.text.Length)) + "'";
                // - a label in a horizontal swipe track may lie beyond the card's edge until swiped to: it must fit the
                //   track's viewport (reachable whole), and the track itself must sit inside the card
                var track = label.GetFirstAncestorOfType<ScrollView>();
                if (track != null && track.mode == ScrollViewMode.Horizontal)
                {
                    Assert.LessOrEqual(label.worldBound.width, track.contentViewport.worldBound.width + 0.5f, what + " fits the swipe track's view");
                    Assert.GreaterOrEqual(track.worldBound.xMin, card.xMin - 0.5f, what + ": its track starts inside the card");
                    Assert.LessOrEqual(track.worldBound.xMax, card.xMax + 0.5f, what + ": its track ends inside the card");
                }
                else
                {
                    Assert.GreaterOrEqual(label.worldBound.xMin, card.xMin - 0.5f, what + " starts inside the card");
                    Assert.LessOrEqual(label.worldBound.xMax, card.xMax + 0.5f, what + " wraps inside the card");
                }
                bool large = label.resolvedStyle.fontSize >= 24f || (label.resolvedStyle.fontSize >= 18.66f && label.resolvedStyle.unityFontStyleAndWeight != FontStyle.Normal);
                float ratio = CardTestInput.Contrast(label.resolvedStyle.color, CardTestInput.EffectiveBackground(label));
                Assert.GreaterOrEqual(ratio, large ? UIAccessibility.MinRatioLargeTextOrUIComponent : UIAccessibility.MinRatioNormalText, what + " contrast");
            }
            // - a picture-only look (a grid of pictures) shows no text: a loaded picture counts as content too
            int pictures = view.Root.Query(className: "card-image").ToList().Count(e => CardTestInput.IsShown(e, view.Root) && !e.ClassListContains("card-image--unavailable"));
            Assert.Greater(texts + pictures, 0, name + ": the block shows text or a picture (not vacuous)");
            foreach (var tap in view.Root.Query(className: "card-tap").ToList().Where(e => CardTestInput.IsShown(e, view.Root)))
                Assert.IsTrue(UIAccessibility.MeetsMinTapTarget(tap.worldBound.width, tap.worldBound.height), name + ": a tap target is " + tap.worldBound.size);

            AssertHeadingAndGap(entry, view);
            yield return Render("Card_" + entry.Name);
        }

        // _3.1 step 6C: the stack draws the block's heading (authored, else the kind's default, else none) above the
        // block in ONE look, and the ONE gap under every scrolling block is --ts-block-gap -- no block adds its own margin
        private void AssertHeadingAndGap(CardGalleryDefinitions.Entry entry, IBlockView view)
        {
            var stack = _harness.Sheet.Stack;
            var heading = stack.HeadingOf(view);
            var slot = stack.SlotOf(view);
            var definition = BlockRegistry.Shared.TryGet(entry.Kind, out var d) ? d : null;
            string expected = CardGalleryDefinitions.HasHeading(entry)
                ? CardGalleryDefinitions.HeadingPrefix + entry.Kind.Replace('_', ' ')
                : definition?.DefaultHeadingKey != null ? new CardStrings(_harness.StringTable.Entries(), null, "en", "en").Get(definition.DefaultHeadingKey) : "";
            Assert.AreEqual(expected, heading.text, entry.Name + ": the heading the stack draws");
            if (expected.Length > 0)
            {
                Assert.IsTrue(CardTestInput.IsShown(heading, slot), entry.Name + ": the heading shows");
                Assert.LessOrEqual(heading.worldBound.yMax, view.Root.worldBound.yMin + 0.5f, entry.Name + ": above its block");
                // - the same look in the scroll and in the footer (only the pinned header styles its kicker its own way)
                var reference = _harness.Sheet.Root.Query<Label>(className: "card-block-heading").ToList().First(l => !stack.HeaderSlot.Contains(l));
                Assert.AreEqual(reference.resolvedStyle.fontSize, heading.resolvedStyle.fontSize, "one heading look");
                Assert.GreaterOrEqual(CardTestInput.Contrast(heading.resolvedStyle.color, CardTestInput.EffectiveBackground(heading)), UIAccessibility.MinRatioNormalText,
                    entry.Name + ": heading contrast");
            }
            else Assert.AreEqual(DisplayStyle.None, heading.resolvedStyle.display, entry.Name + ": no heading, no empty line");

            if (slot.parent != stack.Scroll.contentContainer) return;
            Assert.AreEqual(0f, view.Root.resolvedStyle.marginTop, 0.01f, entry.Name + ": the block adds no top margin of its own");
            Assert.AreEqual(0f, view.Root.resolvedStyle.marginBottom, 0.01f, entry.Name + ": ...nor a bottom one");
            Assert.AreEqual(BlockGap(slot), slot.resolvedStyle.marginBottom, OnePixel(slot) + 0.01f, entry.Name + ": the gap under it is the token (snapped to a screen pixel)");
            Assert.AreEqual(slot.worldBound.yMax, view.Root.worldBound.yMax, OnePixel(slot) + 0.01f, entry.Name + ": the block ends where its slot ends");
        }

        // --ts-block-gap as CardTokens.uss defines it (the one place the value lives)
        private static float BlockGap(VisualElement inCard)
        {
            var match = System.Text.RegularExpressions.Regex.Match(
                System.IO.File.ReadAllText("Assets/Framework/Runtime/UI/Cards/CardTokens.uss"), @"--ts-block-gap:\s*([0-9.]+)px");
            Assert.IsTrue(match.Success, "the token is defined in CardTokens.uss");
            float gap = float.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);
            Assert.Greater(gap, 0f);
            return gap;
        }

        // ---------------- rich_text ----------------

        [UnityTest]
        public IEnumerator RichText_EachLookShapesTheParagraphs()
        {
            RichTextBlockView rich = null;
            yield return ShowBlock("rich_text_plain_long", v => rich = (RichTextBlockView)v);
            Assert.AreEqual(4, rich.Body.Paragraphs.Count, "four paragraphs from the blank lines");
            Assert.IsNull(rich.Body.DropCap.panel, "plain: no raised initial on the card");
            Assert.AreEqual(0, rich.Sections.Count, "only the Sections look shows the Sections rows");
            StringAssert.StartsWith("Keep and walls", GlossaryMarkup.PlainText(CardGalleryDefinitions.LongText), "precondition");

            yield return ShowBlock("rich_text_drop_cap_short", v => rich = (RichTextBlockView)v);
            Assert.AreEqual("B", rich.Body.DropCap.text, "the first letter is raised");
            Assert.IsTrue(CardTestInput.IsShown(rich.Body.DropCap, rich.Root));
            StringAssert.Contains("uilt on the hill", rich.Body.Paragraphs[0].text, "...and the paragraph goes on beside it");
            Assert.Greater(rich.Body.DropCap.resolvedStyle.fontSize, rich.Body.Paragraphs[0].resolvedStyle.fontSize * 2f, "the initial is much larger");
            Assert.LessOrEqual(rich.Body.DropCap.worldBound.xMax, rich.Body.Paragraphs[0].worldBound.xMin + 0.5f, "beside, not above");

            yield return ShowBlock("rich_text_drop_cap_long", v => rich = (RichTextBlockView)v);
            Assert.IsNull(rich.Body.DropCap.panel, "a paragraph that opens with a glossary word gets no initial (the link stays whole)");

            yield return ShowBlock("rich_text_lede_long", v => rich = (RichTextBlockView)v);
            Assert.IsTrue(rich.Body.Paragraphs[0].ClassListContains("card-text__lede"));
            Assert.Greater(rich.Body.Paragraphs[0].resolvedStyle.fontSize, rich.Body.Paragraphs[1].resolvedStyle.fontSize, "the lede is larger than the rest");

            yield return ShowBlock("rich_text_sections_long", v => rich = (RichTextBlockView)v);
            CollectionAssert.AreEqual(new[] { "Before the earthquake", "After 1755" }, rich.Sections.Select(s => s.Heading.text), "one heading per Sections row");
            Assert.Greater(rich.Sections[0].Heading.worldBound.yMin, rich.Body.Root.worldBound.yMax - 0.5f, "the sections follow the introduction");

            yield return ShowBlock("rich_text_sections_nosections", v => rich = (RichTextBlockView)v);
            Assert.AreEqual(0, rich.Sections.Count, "no Sections rows: the text alone");
            Assert.AreEqual(1, rich.Body.Paragraphs.Count);
        }

        // ---------------- quick_facts ----------------

        [UnityTest]
        public IEnumerator QuickFacts_EachLookLaysTheFactsOutAsItPromises()
        {
            QuickFactsBlockView facts = null;
            Rect card;

            yield return ShowBlock("quick_facts_chips_long", v => facts = (QuickFactsBlockView)v);
            card = _harness.Sheet.Root.worldBound;
            Assert.AreEqual(6, facts.Facts.Count);
            Assert.Greater(facts.Facts.Select(f => Mathf.Round(f.Cell.worldBound.yMin)).Distinct().Count(), 1, "chips wrap onto more lines");
            foreach (var f in facts.Facts) Assert.LessOrEqual(f.Cell.worldBound.xMax, card.xMax + 0.5f, "every chip inside the card");
            Assert.Less(facts.Facts[0].Label.worldBound.xMax, facts.Facts[0].Value.worldBound.xMin + 0.5f, "a chip reads label then value");

            yield return ShowBlock("quick_facts_grid_hairline_long", v => facts = (QuickFactsBlockView)v);
            var a = facts.Facts[0].Cell.worldBound;
            var b = facts.Facts[1].Cell.worldBound;
            Assert.AreEqual(a.yMin, b.yMin, 0.5f, "two facts per row");
            Assert.Greater(b.xMin, a.xMin + a.width * 0.9f, "side by side");
            Assert.AreEqual(facts.Root.worldBound.width / 2f, a.width, 1f, "two equal columns");
            Assert.Greater(facts.Facts[0].Cell.resolvedStyle.borderTopWidth, 0f, "a hairline above each fact");

            yield return ShowBlock("quick_facts_big_numbers_short", v => facts = (QuickFactsBlockView)v);
            var big = facts.Facts[0];
            Assert.Greater(big.Value.resolvedStyle.fontSize, big.Label.resolvedStyle.fontSize * 1.4f, "the number is big");
            Assert.LessOrEqual(big.Value.worldBound.yMax, big.Label.worldBound.yMin + 0.5f, "the value sits over its label");

            yield return ShowBlock("quick_facts_big_numbers_long", v => facts = (QuickFactsBlockView)v);
            var tall = facts.Facts[2].Cell.worldBound;  // "Unknown master masons..." wraps over several lines
            var towers = facts.Facts[3];                 // "11", beside it
            Assert.AreEqual(tall.yMin, towers.Cell.worldBound.yMin, 0.5f, "precondition: same row");
            Assert.Greater(tall.height, towers.Value.worldBound.height * 2f, "precondition: the neighbour is much taller");
            Assert.Less(towers.Value.worldBound.yMin - towers.Cell.worldBound.yMin, towers.Value.worldBound.height,
                "a short value starts at the top of its row, not at the bottom");

            yield return ShowBlock("quick_facts_grid_hairline_partial", v => facts = (QuickFactsBlockView)v);
            Assert.AreEqual(2, facts.Facts.Count, "a row with only one text is still shown");
            Assert.IsFalse(CardTestInput.IsShown(facts.Facts[0].Label, facts.Root), "no label: only the value");
            Assert.IsFalse(CardTestInput.IsShown(facts.Facts[1].Value, facts.Root), "no value: only the label");
        }

        // ---------------- fun_fact ----------------

        [UnityTest]
        public IEnumerator FunFact_Flip_ARealTapReveals_ATapOnTheTextKeepsIt_ATapOnTheHeadingHidesIt()
        {
            FunFactBlockView fun = null;
            yield return ShowBlock("fun_fact_flip_short", v => fun = (FunFactBlockView)v);
            var fact = fun.Facts.Single();
            Assert.AreEqual("Did you know?", fact.Heading.text, "the heading comes from the card strings table");
            Assert.AreEqual("Tap to reveal", fact.Hint.text);
            Assert.IsFalse(fact.Revealed);
            Assert.IsFalse(CardTestInput.IsShown(fact.Text.Root, fun.Root), "the fact is hidden until tapped");
            Assert.IsTrue(fact.Box.ClassListContains("card-tap"), "the whole card is the tap target");

            yield return CardTestInput.Tap(fact.Box.panel, fact.Box.worldBound.center);
            yield return null;
            Assert.IsTrue(fact.Revealed, "a real tap reveals it");
            Assert.IsTrue(CardTestInput.IsShown(fact.Text.Root, fun.Root));
            Assert.IsFalse(CardTestInput.IsShown(fact.Hint, fun.Root), "the invitation goes away");
            StringAssert.Contains("locked every night", fact.Text.Paragraphs[0].text);
            var text = fact.Text.Paragraphs[0];
            Assert.GreaterOrEqual(CardTestInput.Contrast(text.resolvedStyle.color, CardTestInput.EffectiveBackground(text)), UIAccessibility.MinRatioNormalText, "the fact reads on its card");
            yield return Render("Card_fun_fact_flip_revealed");

            yield return CardTestInput.Tap(text.panel, text.worldBound.center);
            yield return null;
            Assert.IsTrue(fact.Revealed, "a tap on the revealed text leaves it (glossary words live there)");

            yield return CardTestInput.Tap(fact.Heading.panel, fact.Heading.worldBound.center);
            yield return null;
            Assert.IsFalse(fact.Revealed, "a tap on the heading hides it again");
        }

        [UnityTest]
        public IEnumerator FunFact_Postcard_ShowsEveryFactAtOnce_OnATiltedCard()
        {
            FunFactBlockView fun = null;
            yield return ShowBlock("fun_fact_postcard_long", v => fun = (FunFactBlockView)v);
            Assert.AreEqual(3, fun.Facts.Count);
            foreach (var fact in fun.Facts)
            {
                Assert.IsTrue(fact.Revealed && CardTestInput.IsShown(fact.Text.Root, fun.Root), "shown at once");
                Assert.IsFalse(CardTestInput.IsShown(fact.Hint, fun.Root), "no invitation to tap");
                Assert.IsFalse(fact.Box.ClassListContains("card-tap"), "nothing to tap");
                Assert.AreNotEqual(0f, fact.Box.resolvedStyle.rotate.angle.value, "tilted like a postcard");
            }
            Assert.AreEqual(2, fun.Facts[1].Text.Paragraphs.Count, "a fact can hold paragraphs");
        }

        // ---------------- status ----------------

        private static string RingPicture(VisualElement ring) => CardTestInput.BackgroundPictureName(ring);

        private static readonly System.Collections.Generic.Dictionary<string, (string Picture, string Level, bool Unknown)> RingCases = new()
        {
            { "status_ring_intact", ("RingSolid", "Intact", false) },
            { "status_ring_partial", ("RingDashLong", "Partial Damage", false) },
            { "status_ring_unknown", ("RingDotted", "Unknown", true) },
        };

        private static string[] RingCaseNames => RingCases.Keys.ToArray();

        [UnityTest]
        public IEnumerator Status_Ring_IsTheMarkersPicture_InTheColourTheMarkersRuleGives([ValueSource(nameof(RingCaseNames))] string name)
        {
            var (picture, level, unknown) = RingCases[name];
            StatusBlockView status = null;
            yield return ShowBlock(name, v => status = (StatusBlockView)v);
            Assert.IsTrue(status.Status.FromWall, "the fabricated wall has Outline Types: the wall's colour");
            Assert.AreEqual(picture, RingPicture(status.Ring), "the marker's own ring picture for the row's line style");
            Assert.AreEqual(status.Status.Color, status.Ring.resolvedStyle.unityBackgroundImageTintColor, "tinted with the colour the markers' rule gives");
            var entry = CardGalleryDefinitions.All[IndexOf(name)];
            var poi = CardGalleryDefinitions.Poi(entry);
            var expected = MarkerVisualResolver.Resolve(poi, MarkerVisualSettings.Resolve(CardGalleryDefinitions.Taxonomy(), null));
            Assert.AreEqual(expected.RingLevel.RingColor, status.Ring.resolvedStyle.unityBackgroundImageTintColor, "the very ring colour a marker of this POI gets");
            Assert.AreEqual("Condition", status.Heading.text);
            Assert.AreEqual(level, status.Level.text, "the Outline Types row's name");
            Assert.AreEqual(unknown, CardTestInput.IsShown(status.UnknownMark, status.Root), "the question mark only for an unknown condition");
            if (unknown) Assert.AreEqual("?", status.UnknownMark.text);
            Assert.IsFalse(CardTestInput.IsShown(status.ScaleRow, status.Root), "ring: no scale");
        }

        [UnityTest]
        public IEnumerator Status_WithNoOutlineTypesOnTheWall_FallsBackToTheCardsTokens_AndNamesThePercentage()
        {
            StatusBlockView status = null;
            yield return ShowBlock("status_ring_tokens", v => status = (StatusBlockView)v);
            Assert.IsFalse(status.Status.FromWall, "no row: nothing to borrow from the markers");
            Assert.IsTrue(status.Ring.ClassListContains("card-status__ring--tone-2"), "60% -> --ts-status-2");
            // - --ts-status-2 in CardTokens.uss is rgb(224, 145, 58)
            var tint = status.Ring.resolvedStyle.unityBackgroundImageTintColor;
            Assert.AreEqual(224f / 255f, tint.r, 0.01f);
            Assert.AreEqual(145f / 255f, tint.g, 0.01f);
            Assert.AreEqual(58f / 255f, tint.b, 0.01f);
            Assert.AreEqual("RingSolid", RingPicture(status.Ring), "a known condition keeps a solid line");
            Assert.AreEqual("60% damaged", status.Level.text, "the card string names the percentage, never a key");
        }

        [UnityTest]
        public IEnumerator Status_TheLabelField_ReplacesTheConditionsName()
        {
            StatusBlockView status = null;
            yield return ShowBlock("status_ring_label", v => status = (StatusBlockView)v);
            Assert.AreEqual("Kept as the painter saw it, with every merlon in place", status.Level.text);
            Assert.AreEqual("RingSolid", RingPicture(status.Ring), "the ring still shows the real condition");
        }

        [UnityTest]
        public IEnumerator Status_Scale_ShowsEveryKnownOutlineType_InOrder_AndMarksThisPoints()
        {
            StatusBlockView status = null;
            yield return ShowBlock("status_scale_partial", v => status = (StatusBlockView)v);
            Assert.IsFalse(CardTestInput.IsShown(status.Ring, status.Root), "scale: no big ring");
            CollectionAssert.AreEqual(new[] { "Intact", "Partial Damage", "Heavy Damage", "Destroyed" }, status.Steps.Select(s => s.Name.text), "by damage, Unknown left out");
            CollectionAssert.AreEqual(new[] { false, true, false, false }, status.Steps.Select(s => s.Box.ClassListContains("card-status__step--current")), "this point's row marked");
            Assert.AreEqual(status.Status.Color, status.Steps[1].Ring.resolvedStyle.unityBackgroundImageTintColor, "the current step wears the POI's own colour");
            Assert.AreEqual(new[] { "RingSolid", "RingDashLong", "RingDashShort", "RingDotted" }, status.Steps.Select(s => RingPicture(s.Ring)).ToArray(), "each row's own line");
            for (int i = 1; i < status.Steps.Count; i++)
                Assert.Greater(status.Steps[i].Box.worldBound.xMin, status.Steps[i - 1].Box.worldBound.xMin, "left to right");

            yield return ShowBlock("status_scale_unknown", v => status = (StatusBlockView)v);
            Assert.IsFalse(status.Steps.Any(s => s.Box.ClassListContains("card-status__step--current")), "an unknown condition marks no step");
            Assert.AreEqual("Unknown", status.Level.text);
        }

        // ---------------- process_steps ----------------

        [UnityTest]
        public IEnumerator ProcessSteps_AreNumberedInOrder_JoinedByALine_AndAnUntitledRowLeavesNoGap()
        {
            ProcessStepsBlockView steps = null;
            yield return ShowBlock("process_steps_numbered_long", v => steps = (ProcessStepsBlockView)v);
            Assert.AreEqual(5, steps.Steps.Count);
            CollectionAssert.AreEqual(new[] { "1", "2", "3", "4", "5" }, steps.Steps.Select(s => s.Number.text), "the card numbers the steps");
            for (int i = 1; i < steps.Steps.Count; i++)
                Assert.Greater(steps.Steps[i].Row.worldBound.yMin, steps.Steps[i - 1].Row.worldBound.yMin, "top to bottom, first step first");
            var first = steps.Steps[0];
            Assert.Less(first.Number.worldBound.xMax, first.Title.worldBound.xMin, "the number sits left of its title");
            Assert.AreEqual(first.Number.worldBound.width, first.Number.worldBound.height, OnePixel(first.Number) + 0.01f, "a round number badge");
            Assert.GreaterOrEqual(CardTestInput.Contrast(first.Number.resolvedStyle.color, CardTestInput.EffectiveBackground(first.Number)), UIAccessibility.MinRatioNormalText, "the number reads on its circle");
            for (int i = 0; i < steps.Steps.Count; i++)
            {
                var rail = steps.Steps[i].Rail;
                bool last = i == steps.Steps.Count - 1;
                Assert.AreEqual(last ? Visibility.Hidden : Visibility.Visible, rail.resolvedStyle.visibility, "a line down to the next step, none under the last");
                if (!last) Assert.Greater(rail.worldBound.height, 0f, "the line has length");
            }
            Assert.AreEqual(2, first.Text.Paragraphs.Count, "a step text keeps its paragraphs");
            StringAssert.Contains("<link=\"biscuit\">", steps.Steps[2].Text.Paragraphs[0].text, "a step text links glossary words");

            yield return ShowBlock("process_steps_numbered_short", v => steps = (ProcessStepsBlockView)v);
            var one = steps.Steps[0];
            Assert.AreEqual(one.Number.worldBound.center.y, one.Title.worldBound.center.y, 1f, "a one-line title sits level with its number");

            yield return ShowBlock("process_steps_numbered_partial", v => steps = (ProcessStepsBlockView)v);
            CollectionAssert.AreEqual(new[] { "Shape the clay", "Fire it" }, steps.Steps.Select(s => s.Title.text), "the row with no title is not shown");
            CollectionAssert.AreEqual(new[] { "1", "2" }, steps.Steps.Select(s => s.Number.text), "...and the count does not skip a number");
            Assert.IsFalse(CardTestInput.IsShown(steps.Steps[1].Text.Root, steps.Root), "no text written: none shown");
        }

        // ---------------- swatches ----------------

        [UnityTest]
        public IEnumerator Swatches_EachSampleIsExactlyTheConfigsColour_TwoPerRow_AndARowWithNoNameOrNoRealColourIsLeftOut()
        {
            SwatchesBlockView swatches = null;
            yield return ShowBlock("swatches_grid_long", v => swatches = (SwatchesBlockView)v);
            Assert.AreEqual(6, swatches.Swatches.Count);
            var hexes = new[] { "#1F3F8F", "#F2EEE3", "#D9A93A", "#3C7A4E", "#4A2F45", "#9C3B24" };
            for (int i = 0; i < hexes.Length; i++)
            {
                ColorUtility.TryParseHtmlString(hexes[i], out var expected);
                var drawn = swatches.Swatches[i].Sample.resolvedStyle.backgroundColor;
                Assert.AreEqual(expected.r, drawn.r, 1e-3f, hexes[i] + " red");
                Assert.AreEqual(expected.g, drawn.g, 1e-3f, hexes[i] + " green");
                Assert.AreEqual(expected.b, drawn.b, 1e-3f, hexes[i] + " blue");
                Assert.AreEqual(hexes[i], swatches.Swatches[i].Code.text, "the code in its one written form");
                Assert.IsFalse(CardTestInput.IsShown(swatches.Swatches[i].Code, swatches.Root), "Show Code off (the default): no hex code for the visitor");
                Assert.Greater(swatches.Swatches[i].Sample.worldBound.width, 0f, "the sample takes space");
            }
            var a = swatches.Swatches[0].Cell.worldBound;
            var b = swatches.Swatches[1].Cell.worldBound;
            Assert.AreEqual(a.yMin, b.yMin, 0.5f, "grid: two per row");
            Assert.AreEqual(swatches.Root.contentRect.width / 2f, a.width, 1f, "...in two equal columns");
            Assert.Greater(swatches.Swatches[2].Cell.worldBound.yMin, a.yMin, "the third starts the next row");
            Assert.IsFalse(CardTestInput.IsShown(swatches.Swatches[5].Note, swatches.Root), "no note written: none shown");

            yield return ShowBlock("swatches_grid_codes", v => swatches = (SwatchesBlockView)v);
            foreach (var s in swatches.Swatches)
            {
                Assert.IsTrue(CardTestInput.IsShown(s.Code, swatches.Root), "Show Code on: " + s.Code.text + " shows");
                Assert.GreaterOrEqual(s.Code.worldBound.yMin, s.Name.worldBound.yMax - 0.5f, "...under the name");
            }

            yield return ShowBlock("swatches_grid_partial", v => swatches = (SwatchesBlockView)v);
            CollectionAssert.AreEqual(new[] { "Grey", "Umber" }, swatches.Swatches.Select(s => s.Name.text), "no name, '#12' and 'blue' are left out");
            Assert.AreEqual("#AABBCC", swatches.Swatches[0].Code.text, "#abc is shown in the one written form");
            // - a colour close to the card's own still reads as a sample: its hairline border is the card's line token
            var umber = swatches.Swatches[1].Sample;
            Assert.Greater(umber.resolvedStyle.borderTopWidth, 0f, "a border around every sample");
            Assert.AreNotEqual(umber.resolvedStyle.backgroundColor, umber.resolvedStyle.borderTopColor, "...in another colour than the sample");
        }

        // ---------------- timeline ----------------

        [UnityTest]
        public IEnumerator Timeline_Vertical_RunsDownTheCardInTheAuthoredOrder_AndEndsInTheNowPoint()
        {
            TimelineBlockView timeline = null;
            yield return ShowBlock("timeline_vertical_long", v => timeline = (TimelineBlockView)v);
            Assert.AreEqual(7, timeline.Events.Count, "six events + Now");
            CollectionAssert.AreEqual(new[] { "1147", "c. 1300", "1511", "1 Nov 1755", "1910", "1940", "Now" }, timeline.Events.Select(e => e.Date.text),
                "the authored order, never re-sorted; Now from the card strings");
            for (int i = 1; i < timeline.Events.Count; i++)
                Assert.Greater(timeline.Events[i].Box.worldBound.yMin, timeline.Events[i - 1].Box.worldBound.yMin, "down the card");
            Assert.IsNull(timeline.Track.parent, "vertical: no swipe track");
            var now = timeline.Events.Last();
            Assert.IsTrue(now.IsNow && now.Box.ClassListContains("card-timeline__event--now"));
            Assert.IsFalse(CardTestInput.IsShown(now.Title, timeline.Root), "Now has no title of its own");
            Assert.Greater(now.Dot.worldBound.width, timeline.Events[0].Dot.worldBound.width, "the Now point is the larger ring");
            Assert.AreEqual(Visibility.Hidden, now.Rail.resolvedStyle.visibility, "the line ends at Now");
            Assert.AreEqual(Visibility.Visible, timeline.Events[5].Rail.resolvedStyle.visibility, "...and runs up to it");
            Assert.AreEqual(timeline.Events[0].Dot.worldBound.center.x, timeline.Events[6].Dot.worldBound.center.x, 0.5f, "every dot on one line, Now included");
            yield return Render("Card_timeline_vertical_now");

            yield return ShowBlock("timeline_vertical_short", v => timeline = (TimelineBlockView)v);
            Assert.IsFalse(timeline.Events.Any(e => e.IsNow), "Highlight Now off: no Now point");
            Assert.AreEqual(Visibility.Hidden, timeline.Events.Last().Rail.resolvedStyle.visibility, "the line ends at the last event");

            yield return ShowBlock("timeline_vertical_partial", v => timeline = (TimelineBlockView)v);
            CollectionAssert.AreEqual(new[] { "1147", "1940", "Now" }, timeline.Events.Select(e => e.Date.text), "no date or no title: left out");
        }

        [UnityTest]
        public IEnumerator Timeline_Horizontal_LaysTheEventsSideBySide_OnATrackASwipeReachesTheEndOf()
        {
            TimelineBlockView timeline = null;
            yield return ShowBlock("timeline_horizontal_long", v => timeline = (TimelineBlockView)v);
            Assert.AreEqual(timeline.Root, timeline.Track.parent, "horizontal: the events sit on a swipe track");
            Assert.AreEqual(DisplayStyle.None, timeline.Track.horizontalScroller.resolvedStyle.display, "no desktop scroll bar on the phone card");
            var first = timeline.Events[0].Box.worldBound;
            var second = timeline.Events[1].Box.worldBound;
            Assert.AreEqual(first.yMin, second.yMin, 0.5f, "side by side");
            Assert.Greater(second.xMin, first.xMin);
            Assert.AreEqual(timeline.Events[0].Dot.worldBound.center.y, timeline.Events[6].Dot.worldBound.center.y, 0.5f, "every dot on one line");
            Rect card = _harness.Sheet.Root.worldBound;
            var last = timeline.Events.Last();
            Assert.Greater(last.Box.worldBound.xMin, card.xMax, "precondition: the end of the line starts off the card");
            Assert.Greater(timeline.Events[0].Rail.worldBound.width, 0f, "the line runs from each dot towards the next");

            timeline.Track.ScrollTo(last.Box);
            yield return CardTestInput.Settle(0.1f);
            Assert.LessOrEqual(last.Box.worldBound.xMax, card.xMax + 0.5f, "scrolled along the track, the Now point is on the card");
            Assert.AreEqual("Now", last.Date.text);
            yield return Render("Card_timeline_horizontal_end");

            yield return ShowBlock("timeline_horizontal_short", v => timeline = (TimelineBlockView)v);
            Assert.AreEqual(0f, timeline.Track.scrollOffset.x, "a new bind starts at the beginning of the line");
        }

        // ---------------- person ----------------

        [UnityTest]
        public IEnumerator Person_TheInitialSitsBesideTheNameAndRole_TheCardLookIsSetApart_AndEmptyPartsTakeNoSpace()
        {
            PersonBlockView person = null;
            yield return ShowBlock("person_row_short", v => person = (PersonBlockView)v);
            Assert.AreEqual("A", person.Monogram.text, "the first letter of the name holds the photo's place");
            Assert.AreEqual("Afonso Henriques", person.Name.text);
            Assert.AreEqual("First king", person.Role.text);
            Assert.Less(person.Monogram.worldBound.xMax, person.Name.worldBound.xMin, "the initial beside the name");
            Assert.AreEqual(person.Monogram.worldBound.width, person.Monogram.worldBound.height, OnePixel(person.Monogram) + 0.01f, "round");
            Assert.GreaterOrEqual(person.Role.worldBound.yMin, person.Name.worldBound.yMax - 0.5f, "the role under the name");
            Assert.GreaterOrEqual(person.Text.Root.worldBound.yMin, person.Monogram.worldBound.yMax - 0.5f, "the text under them");
            float rowName = person.Name.resolvedStyle.fontSize;
            Assert.AreEqual(0f, person.Root.resolvedStyle.borderTopWidth, "row: in line with the card, no panel");

            yield return ShowBlock("person_card_long", v => person = (PersonBlockView)v);
            Assert.Greater(person.Root.resolvedStyle.borderTopWidth, 0f, "card: an accent edge on its own panel");
            Assert.AreNotEqual(CardTestInput.EffectiveBackground(person.Root.parent), person.Root.resolvedStyle.backgroundColor, "...a panel of its own colour, not the card's behind it");
            Assert.Greater(person.Name.resolvedStyle.fontSize, rowName, "card: the name larger");
            Assert.AreEqual("D", person.Monogram.text);
            Assert.AreEqual(person.Name.worldBound.yMin, person.Monogram.worldBound.yMin, 1f, "a long name: the initial starts level with it, not half way down");
            Assert.AreEqual(2, person.Text.Paragraphs.Count);

            yield return ShowBlock("person_row_nameonly", v => person = (PersonBlockView)v);
            Assert.IsFalse(CardTestInput.IsShown(person.Role, person.Root), "no role written: none shown");
            Assert.IsFalse(CardTestInput.IsShown(person.Text.Root, person.Root), "no text: none shown");

            Assert.AreEqual("M", PersonBlockView.InitialOf("  manuel"), "upper case, spaces ignored");
            Assert.AreEqual("G", PersonBlockView.InitialOf("1640 Gate-keeper"), "skips to the first letter");
            Assert.AreEqual("É", PersonBlockView.InitialOf("élia"), "an accented letter is a letter");
            Assert.AreEqual("", PersonBlockView.InitialOf(""));
        }

        // ---------------- story_chapters ----------------

        [UnityTest]
        public IEnumerator StoryChapters_RealTapsOnNextAndPrevious_TurnTheChapters_TheBarAndTheCounterFollow()
        {
            StoryChaptersBlockView story = null;
            yield return ShowBlock("story_chapters_segmented_long", v => story = (StoryChaptersBlockView)v);
            Assert.AreEqual(4, story.Count);
            Assert.AreEqual(4, story.Segments.Count, "one segment per chapter");
            Assert.AreEqual(0, story.Index, "a bind opens the first chapter");
            Assert.AreEqual("Chapter 1 of 4", story.Counter.text, "the counter from the card strings");
            StringAssert.StartsWith("The siege of the town", story.Title.text);
            Assert.AreEqual(Visibility.Hidden, story.Previous.resolvedStyle.visibility, "nothing before the first chapter");
            Assert.AreEqual("Previous", story.Previous.Q<Label>().text);
            Assert.AreEqual("Next", story.Next.Q<Label>().text);
            Assert.IsTrue(story.Segments[0].ClassListContains("card-story__segment--current"));
            Assert.AreEqual(story.Segments[0].worldBound.width, story.Segments[3].worldBound.width, 0.5f, "equal segments");
            var nextSpot = story.Next.worldBound.center;

            yield return CardTestInput.Tap(story.Next.panel, story.Next.worldBound.center);
            yield return null;
            Assert.AreEqual(1, story.Index, "a real tap on Next");
            Assert.AreEqual("Chapter 2 of 4", story.Counter.text);
            Assert.AreEqual("The palace", story.Title.text);
            StringAssert.Contains("painted tiles", story.Body.Paragraphs[0].text);
            CollectionAssert.AreEqual(new[] { true, true, false, false }, story.Segments.Select(s => s.ClassListContains("card-story__segment--read")), "the bar fills up to the chapter shown");
            Assert.AreEqual(Visibility.Visible, story.Previous.resolvedStyle.visibility);
            Assert.AreEqual(nextSpot.x, story.Next.worldBound.center.x, 0.5f, "Next keeps its side: Previous appearing does not push it");
            Assert.LessOrEqual(story.Next.worldBound.yMax, _harness.Sheet.Root.worldBound.yMax + 0.5f, "...and stays on the card");
            yield return Render("Card_story_chapters_second");

            yield return CardTestInput.Tap(story.Next.panel, story.Next.worldBound.center);
            yield return CardTestInput.Tap(story.Next.panel, story.Next.worldBound.center);
            yield return null;
            Assert.AreEqual(3, story.Index);
            Assert.AreEqual(Visibility.Hidden, story.Next.resolvedStyle.visibility, "nothing after the last chapter");
            yield return CardTestInput.Tap(story.Next.panel, story.Next.worldBound.center);
            yield return null;
            Assert.AreEqual(3, story.Index, "a tap on the hidden Next does nothing");

            yield return CardTestInput.Tap(story.Previous.panel, story.Previous.worldBound.center);
            yield return null;
            Assert.AreEqual(2, story.Index, "a real tap on Previous goes back");
            Assert.AreEqual("The earthquake", story.Title.text);

            yield return ShowBlock("story_chapters_segmented_short", v => story = (StoryChaptersBlockView)v);
            Assert.AreEqual(0, story.Index, "a new bind starts at the first chapter again: nothing remembered");

            yield return ShowBlock("story_chapters_segmented_single", v => story = (StoryChaptersBlockView)v);
            Assert.AreEqual(1, story.Count, "no title or no text: left out");
            Assert.AreEqual("Chapter 1 of 1", story.Counter.text);
            Assert.IsFalse(CardTestInput.IsShown(story.Nav, story.Root), "one chapter: no buttons");
        }

        // _3.1 step 6C: "Next" rendered as an egg (a short word in a round-cornered box). Previous / Next now wear the ONE
        // pill of the actions: the same height and corner radius, the radius half the height, never narrower than tall
        [UnityTest]
        public IEnumerator StoryChapters_PreviousAndNext_AreTheActionsPill_NeverAnEgg()
        {
            StoryChaptersBlockView story = null;
            yield return ShowBlock("story_chapters_segmented_short", v => story = (StoryChaptersBlockView)v);
            story.Show(1);
            yield return CardTestInput.Settle(0.15f);
            var buttons = new[] { story.Previous, story.Next };
            var sizes = buttons.Select(b => (b.worldBound.width, b.worldBound.height, b.resolvedStyle.borderTopLeftRadius)).ToArray();

            ActionsBlockView actions = null;
            yield return ShowBlock("actions_pill_row_short", v => actions = (ActionsBlockView)v);
            var pill = actions.Actions[0].Button;
            Assert.IsTrue(pill.ClassListContains("card-pill"), "precondition: the actions pill");
            float onePixel = OnePixel(pill);
            for (int i = 0; i < buttons.Length; i++)
            {
                string which = i == 0 ? "Previous" : "Next";
                Assert.IsTrue(buttons[i].ClassListContains("card-pill"), which + " wears the shared pill class");
                Assert.AreEqual(pill.worldBound.height, sizes[i].height, onePixel + 0.01f, which + ": the pill's height");
                Assert.AreEqual(pill.resolvedStyle.borderTopLeftRadius, sizes[i].borderTopLeftRadius, 0.01f, which + ": the pill's corner radius");
                Assert.AreEqual(sizes[i].height / 2f, sizes[i].borderTopLeftRadius, onePixel + 0.01f, which + ": a half-height radius (round ends)");
                Assert.GreaterOrEqual(sizes[i].width, sizes[i].height, which + ": never narrower than tall (no egg)");
            }
        }

        // ---------------- compare_points ----------------

        [UnityTest]
        public IEnumerator ComparePoints_BothRingsAreEachPointsMarkerRing_ThisPointFirst_NamedByTheirCardTitles()
        {
            ComparePointsBlockView compare = null;
            yield return ShowBlock("compare_points_rings_short", v => compare = (ComparePointsBlockView)v);
            var entry = CardGalleryDefinitions.All[IndexOf("compare_points_rings_short")];
            var wall = CardGalleryDefinitions.Taxonomy();
            entry.WallSetup(wall);
            var look = MarkerVisualSettings.Resolve(wall, null);
            var pois = new[] { CardGalleryDefinitions.Poi(entry), wall.pois.Single(p => p.id == CardGalleryDefinitions.CompareOtherId) };
            Assert.AreEqual("Side by side", _harness.Sheet.Stack.HeadingOf(compare).text, "no heading written: the kind's default heading from the card strings (6C)");
            for (int i = 0; i < 2; i++)
            {
                var side = compare.Sides[i];
                var marker = MarkerVisualResolver.Resolve(pois[i], look);
                Assert.AreEqual(pois[i].id, side.PoiId, i == 0 ? "this point first" : "the other second");
                Assert.AreEqual(marker.RingLevel.RingColor, side.Ring.resolvedStyle.unityBackgroundImageTintColor, "the very ring colour its marker gets");
                Assert.AreEqual(CardStatusRule.Resolve(pois[i], look, wall).LineStyle, marker.RingLevel.RingSpriteKey, "precondition: the rule's picture is the marker's");
            }
            Assert.AreEqual("RingSolid", CardTestInput.BackgroundPictureName(compare.Sides[0].Ring), "intact: solid");
            Assert.AreEqual("RingDashLong", CardTestInput.BackgroundPictureName(compare.Sides[1].Ring), "partial damage: its dash");
            Assert.AreNotEqual(compare.Sides[0].Ring.resolvedStyle.unityBackgroundImageTintColor, compare.Sides[1].Ring.resolvedStyle.unityBackgroundImageTintColor,
                "per type: two conditions, two colours");
            CollectionAssert.AreEqual(new[] { "Gate", "Old Cathedral" }, compare.Sides.Select(s => s.Title.text), "each point's card title (the other has no header: its name)");
            CollectionAssert.AreEqual(new[] { "Intact", "Partial Damage" }, compare.Sides.Select(s => s.Level.text), "the Outline Types rows' names, never keys");
            Assert.AreEqual(compare.Sides[0].Ring.worldBound.center.y, compare.Sides[1].Ring.worldBound.center.y, 0.5f, "side by side");
            Assert.Less(compare.Sides[0].Ring.worldBound.xMax, compare.Sides[1].Ring.worldBound.xMin);
            Assert.AreEqual(compare.Sides[0].Box.worldBound.center.x, compare.Sides[0].Ring.worldBound.center.x, 0.5f, "each ring centred in its half");

            yield return ShowBlock("compare_points_rings_long", v => compare = (ComparePointsBlockView)v);
            Assert.AreEqual("The Royal Palace of the Kings by the river, before the earthquake", compare.Sides[1].Title.text, "the other point's own header title");

            yield return ShowBlock("compare_points_rings_unknown", v => compare = (ComparePointsBlockView)v);
            Assert.IsTrue(CardTestInput.IsShown(compare.Sides[1].UnknownMark, compare.Root), "the other nobody assessed: its question mark");
            Assert.IsFalse(CardTestInput.IsShown(compare.Sides[0].UnknownMark, compare.Root));
            Assert.AreEqual("Unknown", compare.Sides[1].Level.text);
        }

        // ---------------- practical_info + the card's icon set ----------------

        // The parts of an icon that are drawn (shown and of real size)
        private static int DrawnParts(VisualElement icon) =>
            icon.Q(className: "card-icon__glyph").Children().Count(p => p.resolvedStyle.display == DisplayStyle.Flex && p.worldBound.width > 0f && p.worldBound.height > 0f);

        // The colour an icon is drawn in: its ring's line, else its first shown bar or dot
        private static Color IconInk(VisualElement icon)
        {
            var shown = icon.Q(className: "card-icon__glyph").Children().Where(p => p.resolvedStyle.display == DisplayStyle.Flex).ToList();
            var frame = shown.FirstOrDefault(p => p.ClassListContains("card-icon__frame"));
            return frame != null ? frame.resolvedStyle.borderTopColor : shown[0].resolvedStyle.backgroundColor;
        }

        [UnityTest]
        public IEnumerator PracticalInfo_EachRowWearsItsIconFromTheOneSet_TheWordsLineUp_AndARowWithNoLabelIsLeftOut()
        {
            PracticalInfoBlockView info = null;
            yield return ShowBlock("practical_info_rows_long", v => info = (PracticalInfoBlockView)v);
            CollectionAssert.AreEqual(BuiltInBlocks.PracticalInfoIcons.OrderBy(k => k), info.Rows.Select(r => CardIcons.KeyOf(r.Icon)).OrderBy(k => k), "every icon of the set once");
            var drawings = new System.Collections.Generic.HashSet<string>();
            foreach (var row in info.Rows)
            {
                var glyph = row.Icon.Q(className: "card-icon__glyph");
                Assert.GreaterOrEqual(DrawnParts(row.Icon), 2, CardIcons.KeyOf(row.Icon) + ": drawn from at least two parts");
                foreach (var part in glyph.Children())
                    if (part.resolvedStyle.display == DisplayStyle.Flex)
                        Assert.IsTrue(row.Icon.worldBound.Overlaps(part.worldBound), CardIcons.KeyOf(row.Icon) + ": its parts inside the icon");
                // - a fingerprint of which parts show where: two keys must never draw the same picture
                drawings.Add(string.Join("|", glyph.Children().Where(p => p.resolvedStyle.display == DisplayStyle.Flex)
                    .Select(p => p.worldBound.x - glyph.worldBound.x + "," + (p.worldBound.y - glyph.worldBound.y) + "," + p.worldBound.width + "," + p.worldBound.height + "," + p.resolvedStyle.rotate.angle.value)));
                Assert.GreaterOrEqual(UIAccessibility.ContrastRatio(IconInk(row.Icon), CardTestInput.EffectiveBackground(row.Icon)),
                    UIAccessibility.MinRatioLargeTextOrUIComponent, CardIcons.KeyOf(row.Icon) + ": the icon reads on its badge (3:1, a UI component)");
            }
            Assert.AreEqual(info.Rows.Count, drawings.Count, "six different pictures");
            for (int i = 1; i < info.Rows.Count; i++)
            {
                Assert.AreEqual(info.Rows[0].Label.worldBound.xMin, info.Rows[i].Label.worldBound.xMin, 0.5f, "every row's words start on one line");
                Assert.Greater(info.Rows[i].Box.resolvedStyle.borderTopWidth, 0f, "a hairline between rows");
            }
            Assert.GreaterOrEqual(info.Rows[0].Value.worldBound.yMin, info.Rows[0].Label.worldBound.yMax - 0.5f, "the value under its label");

            yield return ShowBlock("practical_info_rows_partial", v => info = (PracticalInfoBlockView)v);
            CollectionAssert.AreEqual(new[] { "Open", "Tickets", "Where" }, info.Rows.Select(r => r.Label.text), "a row with no label is left out");
            Assert.AreEqual(Visibility.Hidden, info.Rows[0].Icon.resolvedStyle.visibility, "no icon picked: no empty badge");
            Assert.AreEqual(Visibility.Hidden, info.Rows[1].Icon.resolvedStyle.visibility, "an action's icon is not a row icon: none drawn");
            Assert.AreEqual(info.Rows[0].Label.worldBound.xMin, info.Rows[2].Label.worldBound.xMin, 0.5f, "...and the words still line up");
            Assert.IsFalse(CardTestInput.IsShown(info.Rows[2].Value, info.Root), "no value written: none shown");
        }

        [UnityTest]
        public IEnumerator TheActions_DrawTheirIconFromTheSameSet_AsATarget()
        {
            ActionsBlockView actions = null;
            yield return ShowBlock("actions_pill_row_short", v => actions = (ActionsBlockView)v);
            var icon = actions.Actions[0].Icon;
            Assert.IsTrue(icon.ClassListContains("card-icon"), "the card's one icon set");
            Assert.AreEqual(CardIcons.ShowOnWall, CardIcons.KeyOf(icon));
            var frame = icon.Q(className: "card-icon__frame");
            var dot = icon.Q(className: "card-icon__dot");
            Assert.AreEqual(2, DrawnParts(icon), "a ring and a dot");
            Assert.AreEqual(frame.worldBound.center.x, dot.worldBound.center.x, 0.5f, "the dot in the ring's centre");
            Assert.AreEqual(frame.worldBound.center.y, dot.worldBound.center.y, 0.5f);

            yield return ShowBlock("actions_circles_short", v => actions = (ActionsBlockView)v);
            icon = actions.Actions[0].Icon;
            var glyph = icon.Q(className: "card-icon__glyph");
            Assert.GreaterOrEqual(icon.worldBound.width, 44f, "circles: the icon's round badge is finger-sized");
            Assert.Less(glyph.worldBound.width, icon.worldBound.width, "...and the drawing keeps its own size inside it");
            Assert.AreEqual(icon.worldBound.center.x, glyph.worldBound.center.x, 0.5f, "centred");

            yield return ShowBlock("actions_sticky_cta_short", v => actions = (ActionsBlockView)v);
            var sticky = actions.Actions[0];
            Assert.AreEqual(sticky.Label.resolvedStyle.color, sticky.Icon.Q(className: "card-icon__frame").resolvedStyle.borderTopColor, "on the accent button the icon wears the button's text colour");
        }

        // ---------------- sources ----------------

        [UnityTest]
        public IEnumerator Sources_TheConfidenceChip_FollowsContentStatus_OnlyInTheWithConfidenceLook()
        {
            SourcesBlockView sources = null;
            yield return ShowBlock("sources_with_confidence_short", v => sources = (SourcesBlockView)v);
            Assert.AreEqual("Sources", _harness.Sheet.Stack.HeadingOf(sources).text, "no heading written: the kind's default heading from the card strings table (6C)");
            Assert.IsTrue(CardTestInput.IsShown(sources.Confidence, sources.Root));
            Assert.AreEqual("Verified", sources.Confidence.text);
            Assert.IsTrue(sources.Confidence.ClassListContains("card-sources__chip--verified"));
            Assert.GreaterOrEqual(CardTestInput.Contrast(sources.Confidence.resolvedStyle.color, CardTestInput.EffectiveBackground(sources.Confidence)),
                UIAccessibility.MinRatioNormalText, "the chip reads");

            yield return ShowBlock("sources_with_confidence_long", v => sources = (SourcesBlockView)v);
            Assert.AreEqual("Draft", sources.Confidence.text);
            Assert.IsTrue(sources.Confidence.ClassListContains("card-sources__chip--draft"));
            Assert.AreEqual(3, sources.Rows.Count);
            Assert.AreEqual("CC BY 4.0", sources.Rows[2].Licence.text);

            yield return ShowBlock("sources_with_confidence_partial", v => sources = (SourcesBlockView)v);
            Assert.IsFalse(CardTestInput.IsShown(sources.Confidence, sources.Root), "no Content Status: no chip");
            Assert.IsFalse(CardTestInput.IsShown(sources.Rows[0].Author, sources.Root), "no author: none shown");
            Assert.IsFalse(CardTestInput.IsShown(sources.Rows[0].Licence, sources.Root));

            yield return ShowBlock("sources_list_short", v => sources = (SourcesBlockView)v);
            Assert.IsFalse(CardTestInput.IsShown(sources.Confidence, sources.Root), "the list look never shows the chip, even when verified");
        }

        // ---------------- actions ----------------

        [UnityTest]
        public IEnumerator Actions_ARealTapOnShowOnTheWall_LowersTheCardToPeek()
        {
            ActionsBlockView actions = null;
            yield return ShowBlock("actions_pill_row_short", v => actions = (ActionsBlockView)v);
            var sheet = _harness.Sheet;
            Assert.AreEqual(SheetStopRule.Stop.Full, sheet.Stop);
            var button = actions.Actions.Single().Button;
            Assert.AreEqual("See it on the wall", actions.Actions[0].Label.text);
            yield return CardTestInput.Tap(button.panel, button.worldBound.center);
            yield return CardTestInput.Settle();
            Assert.AreEqual(SheetStopRule.Stop.Peek, sheet.Stop, "a real tap: the card lowers to its title, the wall shows");
            Assert.IsTrue(sheet.IsOpen, "...it does not close");
        }

        [UnityTest]
        public IEnumerator Actions_EachLook_AndOnlyButtonsThatCanDoTheirJob()
        {
            ActionsBlockView actions = null;
            yield return ShowBlock("actions_circles_partial", v => actions = (ActionsBlockView)v);
            Assert.AreEqual(1, actions.Actions.Count, "no words, and an action this framework does not know (listen): both left out");

            yield return ShowBlock("actions_circles_long", v => actions = (ActionsBlockView)v);
            Assert.AreEqual(3, actions.Actions.Count);
            var a = actions.Actions[0];
            Assert.LessOrEqual(a.Icon.worldBound.yMax, a.Label.worldBound.yMin + 0.5f, "circles: the icon over its words");
            Assert.GreaterOrEqual(a.Icon.worldBound.width, 44f, "the round icon is itself finger-sized");
            foreach (var other in actions.Actions)
                Assert.AreEqual(a.Icon.worldBound.yMin, other.Icon.worldBound.yMin, 0.5f, "every circle's icon on one top line, whatever its label's length");

            yield return ShowBlock("actions_pill_row_long", v => actions = (ActionsBlockView)v);
            var p = actions.Actions[0];
            Assert.AreEqual(p.Icon.worldBound.center.y, p.Label.worldBound.center.y, 1f, "pill: icon and words on one line");
            Assert.Less(p.Icon.worldBound.xMax, p.Label.worldBound.xMin + 0.5f, "...icon first");

            yield return ShowBlock("actions_sticky_cta_long", v => actions = (ActionsBlockView)v);
            Assert.AreEqual(1, actions.Actions.Count, "sticky: one call to action, the first button");
            Assert.AreEqual("See it on the wall", actions.Actions[0].Label.text);
            var sheet = _harness.Sheet;
            Assert.AreEqual(sheet.Stack.Footer, sheet.Stack.SlotOf(actions).parent, "sticky: pinned in the card's footer, outside the scroll");
            Assert.AreEqual(sheet.Root.worldBound.yMax, sheet.Stack.Footer.worldBound.yMax, 1f, "at the very bottom of the card");
            foreach (var s in actions.Actions)
                Assert.AreEqual(actions.Root.contentRect.width, s.Button.worldBound.width, 1f, "full-width buttons, a long label wrapping inside");
            sheet.SetStop(SheetStopRule.Stop.Half);
            yield return CardTestInput.Settle();
            Assert.LessOrEqual(sheet.Stack.Footer.worldBound.yMax, sheet.Root.worldBound.yMax + 1f, "at half too, still at the bottom, in reach");
            Assert.Greater(sheet.Stack.Footer.worldBound.yMin, sheet.Root.worldBound.yMin, "...and inside the card");
            yield return Render("Card_actions_sticky_cta_half");
        }

        // ---------------- pull_quote ----------------

        [UnityTest]
        public IEnumerator PullQuote_EachLookSetsTheQuoteApart_AndAnEmptyAttributionTakesNoSpace()
        {
            PullQuoteBlockView serif = null, minimal = null;
            yield return ShowBlock("pull_quote_serif_long", v => serif = (PullQuoteBlockView)v);
            var serifText = serif.Quote.Paragraphs[0];
            Assert.AreEqual(FontStyle.Italic, serifText.resolvedStyle.unityFontStyleAndWeight, "a quote is italic");
            Assert.Greater(serif.Root.resolvedStyle.borderLeftWidth, 0f, "serif: an accent bar");
            Assert.GreaterOrEqual(serif.Source.worldBound.yMin, serif.Author.worldBound.yMax - 0.5f, "serif: the source under the author");
            float serifSize = serifText.resolvedStyle.fontSize;

            yield return ShowBlock("pull_quote_minimal_short", v => minimal = (PullQuoteBlockView)v);
            Assert.Less(minimal.Quote.Paragraphs[0].resolvedStyle.fontSize, serifSize, "minimal is body size, serif larger");
            Assert.AreEqual(0f, minimal.Root.resolvedStyle.borderLeftWidth, "minimal: no bar");
            Assert.AreEqual(minimal.Author.worldBound.yMin, minimal.Source.worldBound.yMin, 0.5f, "minimal: author and source on one line");
            Assert.Less(minimal.Author.worldBound.xMax, minimal.Source.worldBound.xMin, "...author first");

            yield return ShowBlock("pull_quote_serif_noauthor", v => serif = (PullQuoteBlockView)v);
            Assert.IsFalse(CardTestInput.IsShown(serif.Attribution, serif.Root), "no author, no source: no attribution line");
        }

        [UnityTest]
        public IEnumerator RichText_ARealTapOnAGlossaryWord_OpensItsDefinitionUnderTheParagraph_AndASecondTapClosesIt()
        {
            RichTextBlockView rich = null;
            yield return ShowBlock("rich_text_plain_long", v => rich = (RichTextBlockView)v);
            var first = rich.Body.Paragraphs[0];
            StringAssert.StartsWith("<link=\"keep\">", first.text, "the paragraph opens with the linked word");
            // - the word is the first thing on the first line: a finger on its left half
            var onWord = new Vector2(first.worldBound.xMin + 12f, first.worldBound.yMin + first.resolvedStyle.fontSize * 0.6f);

            yield return CardTestInput.Tap(first.panel, onWord);
            Assert.AreEqual("keep", rich.Body.OpenTerm, "a real tap on the underlined word opens it");
            Assert.AreEqual("Keep", rich.Body.DefinitionTitle.text, "titled with the words shown");
            Assert.AreEqual("The strongest tower of a castle, its last refuge.", rich.Body.DefinitionText.text);
            var panel = rich.Body.DefinitionPanel;
            Assert.AreEqual(rich.Body.Root.IndexOf(first) + 1, rich.Body.Root.IndexOf(panel), "right under its paragraph");
            yield return CardTestInput.Settle(0.1f);
            Assert.GreaterOrEqual(panel.worldBound.yMin, first.worldBound.yMax - 0.5f);
            Assert.GreaterOrEqual(CardTestInput.Contrast(rich.Body.DefinitionText.resolvedStyle.color, CardTestInput.EffectiveBackground(rich.Body.DefinitionText)),
                UIAccessibility.MinRatioNormalText, "the definition reads on its panel");
            yield return Render("Card_rich_text_glossary_open");

            yield return CardTestInput.Tap(first.panel, onWord);
            Assert.IsNull(rich.Body.OpenTerm, "the same word again closes it");
            Assert.IsNull(panel.parent);

            // - a tap on plain words of the last paragraph opens nothing
            var last = rich.Body.Paragraphs[3];
            yield return CardTestInput.Tap(last.panel, new Vector2(last.worldBound.xMin + 12f, last.worldBound.yMin + last.resolvedStyle.fontSize * 0.6f));
            Assert.IsNull(rich.Body.OpenTerm, "plain text is not a link");
        }

        // The cause of the old 1.5 px flake: a height measured while the sheet still animated. The gallery sheet has no
        // height transition at all, so every geometry assert above measures a sheet at rest -- whatever the frame rate.
        [UnityTest]
        public IEnumerator TheGallerySheet_HasNoHeightAnimation_SoNoMeasurementRacesOne()
        {
            _harness.Show(IndexOf("header_text_only_long_peek"));
            yield return null;
            var sheet = _harness.Sheet;
            Assert.AreEqual("height", sheet.Root.resolvedStyle.transitionProperty.Single().ToString(), "precondition: the card animates its height (PoiCard.uss)");
            Assert.IsTrue(sheet.Root.resolvedStyle.transitionDuration.All(d => d.value == 0f), "the gallery runs it with a zero duration");
            var real = new PoiCardSheetView(new VisualElement(), BlockRegistry.Shared, System.Array.Empty<StyleSheet>());
            Assert.AreEqual(StyleKeyword.Null, real.Root.style.transitionDuration.keyword, "the real card keeps the stylesheet's duration: only the harness sets one");
        }

        [UnityTest]
        public IEnumerator TheSheet_DragsUpThroughItsStops_AndPullingItDownCloses()
        {
            _harness.Show(CardGalleryDefinitions.All.ToList().FindIndex(e => e.Stop == SheetStopRule.Stop.Peek && e.Variant == BuiltInBlocks.HeaderTextOnly));
            yield return CardTestInput.Settle();
            var sheet = _harness.Sheet;

            yield return CardTestInput.Drag(sheet.Handle, -(sheet.Stops.Half - sheet.Stops.Peek));
            yield return CardTestInput.Settle();
            Assert.AreEqual(SheetStopRule.Stop.Half, sheet.Stop, "a slow drag up by the peek-half gap lands on half");

            yield return CardTestInput.Drag(sheet.Stack.HeaderSlot, -(sheet.Stops.Full - sheet.Stops.Half));
            yield return CardTestInput.Settle();
            Assert.AreEqual(SheetStopRule.Stop.Full, sheet.Stop, "the header drags the sheet too");
            // - layout snaps to physical pixels: one screen pixel in panel units is the honest tolerance (as for every header entry)
            float onePixel = RuntimePanelUtils.ScreenToPanel(sheet.Root.panel, Vector2.right).x - RuntimePanelUtils.ScreenToPanel(sheet.Root.panel, Vector2.zero).x;
            Assert.AreEqual(sheet.Stops.Full, sheet.Root.resolvedStyle.height, onePixel + 0.01f);

            yield return CardTestInput.Drag(sheet.Handle, sheet.Stops.Full - sheet.Stops.Peek * 0.3f, frames: 20);
            Assert.IsFalse(sheet.IsOpen, "pulled below peek: the card closes (the harness closes on CloseRequested)");
        }

        [UnityTest]
        public IEnumerator AQuickSwipeUpFromPeek_GoesToHalf_EvenReleasedNearPeek()
        {
            _harness.Show(CardGalleryDefinitions.All.ToList().FindIndex(e => e.Stop == SheetStopRule.Stop.Peek));
            yield return CardTestInput.Settle();
            var sheet = _harness.Sheet;
            float shortMove = (sheet.Stops.Half - sheet.Stops.Peek) * 0.3f;
            yield return CardTestInput.Drag(sheet.Handle, -shortMove, frames: 2);
            yield return CardTestInput.Settle();
            Assert.AreEqual(SheetStopRule.Stop.Half, sheet.Stop, "a flick counts, not only where the finger stopped");
        }

        [UnityTest]
        public IEnumerator TheX_AsksToClose()
        {
            _harness.Show(0);
            yield return CardTestInput.Settle();
            int asked = 0;
            _harness.Sheet.CloseRequested += () => asked++;
            using (var e = NavigationSubmitEvent.GetPooled())
            {
                e.target = _harness.Sheet.CloseButton;
                _harness.Sheet.CloseButton.SendEvent(e);
            }
            yield return null;
            Assert.AreEqual(1, asked);
            Assert.IsFalse(_harness.Sheet.IsOpen);
        }

        // Save what the Game view shows under Assets/Screenshots
        private static IEnumerator Render(string name)
        {
            yield return new WaitForEndOfFrame();
            var tex = ScreenCapture.CaptureScreenshotAsTexture();
            try
            {
                string dir = Path.Combine(Application.dataPath, "Screenshots");
                Directory.CreateDirectory(dir);
                File.WriteAllBytes(Path.Combine(dir, name + ".png"), tex.EncodeToPNG());
            }
            finally { Object.Destroy(tex); }
        }
            // ---------------- Tier 2: pictures (_3.1 step 7) ----------------

        // The colour a real render shows at a panel position (which picture a frame shows): the darkest pixel of the 7x7
        // patch there -- every gallery picture is its flat colour crossed by LIGHTER grid lines (CardGalleryMedia), so the
        // darkest pixel is the picture's own colour wherever the point falls
        private static IEnumerator PixelAt(VisualElement inPanel, Vector2 panelPoint, System.Action<Color> got)
        {
            yield return new WaitForEndOfFrame();
            var tex = ScreenCapture.CaptureScreenshotAsTexture();
            try
            {
                var origin = RuntimePanelUtils.ScreenToPanel(inPanel.panel, Vector2.zero);
                float unit = RuntimePanelUtils.ScreenToPanel(inPanel.panel, Vector2.right).x - origin.x;
                var screen = (panelPoint - origin) / unit;
                int cx = Mathf.RoundToInt(screen.x), cy = tex.height - 1 - Mathf.RoundToInt(screen.y);
                Color darkest = Color.white;
                for (int dy = -3; dy <= 3; dy++)
                    for (int dx = -3; dx <= 3; dx++)
                    {
                        var c = tex.GetPixel(cx + dx, cy + dy);
                        if (c.grayscale < darkest.grayscale) darkest = c;
                    }
                got(darkest);
            }
            finally { Object.Destroy(tex); }
        }

        private static void AssertColour(Color expected, Color seen, string what)
        {
            Assert.AreEqual(expected.r, seen.r, 0.04f, what + " (red)");
            Assert.AreEqual(expected.g, seen.g, 0.04f, what + " (green)");
            Assert.AreEqual(expected.b, seen.b, 0.04f, what + " (blue)");
        }

        private static Color PictureColour(string name) => CardGalleryDefinitions.Pictures[name].Colour;

        private IEnumerator ShowHeader(string name)
        {
            _harness.Show(IndexOf(name));
            yield return CardTestInput.Settle();
        }

        private HeaderBlockView GalleryHeader => (HeaderBlockView)_harness.Sheet.Stack.BoundViews[0];

        [UnityTest]
        public IEnumerator HeaderImageParallax_ARealScroll_MovesThePictureAtHalfTheFramesSpeed_TheTitleStaysPinned()
        {
            yield return ShowHeader("header_image_parallax_long_full");
            var header = GalleryHeader;
            var stack = _harness.Sheet.Stack;
            Assert.IsTrue(header.HasHero);
            // - a point inside the hero shows the picture itself (its own colour, not the frame's or the card's)
            yield return PixelAt(header.HeroPart, header.HeroPart.worldBound.center + new Vector2(10f, 10f), c => AssertColour(PictureColour("wide.png"), c, "the hero shows wide.png"));
            float frameTop = header.HeroPart.worldBound.yMin, pictureTop = header.Picture.Root.worldBound.yMin, titleTop = header.PeekPart.worldBound.yMin;

            yield return CardTestInput.Wheel(stack.Scroll.contentViewport, 6f, frames: 2);
            yield return CardTestInput.Settle(0.2f);
            float scrolled = stack.Scroll.scrollOffset.y;
            Assert.Greater(scrolled, 20f, "precondition: the real wheel scrolled the stack");
            float frameMoved = frameTop - header.HeroPart.worldBound.yMin;
            float pictureMoved = pictureTop - header.Picture.Root.worldBound.yMin;
            Assert.AreEqual(scrolled * HeaderBlockView.ParallaxFactor, header.ParallaxOffset, 0.5f, "the picture slides by half the scroll inside its frame");
            Assert.AreEqual(frameMoved * (1f - HeaderBlockView.ParallaxFactor), pictureMoved, OnePixel(stack.Scroll) + 0.5f,
                "on screen the picture moves at half the frame's speed (" + pictureMoved + " vs " + frameMoved + ")");
            Assert.AreEqual(titleTop, header.PeekPart.worldBound.yMin, 0.5f, "the title stays pinned");
            yield return Render("Card_header_image_parallax_scrolled");

            yield return CardTestInput.Wheel(stack.Scroll.contentViewport, -40f, frames: 2);
            yield return CardTestInput.Settle(0.2f);
            Assert.AreEqual(0f, header.ParallaxOffset, 0.01f, "back at the top: the picture where it started");
        }

        [UnityTest]
        public IEnumerator HeaderSplitThenNow_ShowsBothPicturesSideBySide_LabelledThenAndNow()
        {
            yield return ShowHeader("header_split_then_now_short_full");
            var header = GalleryHeader;
            Assert.IsTrue(header.HasHero);
            Assert.AreEqual("then.png", header.Picture.Path);
            Assert.AreEqual("now.png", header.SecondPicture.Path);
            Rect then = header.Picture.Root.worldBound, now = header.SecondPicture.Root.worldBound;
            Assert.AreEqual(then.width, now.width, OnePixel(header.HeroPart) * 2f + 0.5f, "two equal halves");
            Assert.LessOrEqual(then.xMax, now.xMin + 0.5f, "then on the left, now on the right");
            Assert.AreEqual(header.HeroPart.worldBound.width, then.width + now.width, OnePixel(header.HeroPart) * 3f + 4f, "together they fill the hero (a thin line between)");
            Assert.AreEqual("Then", header.ThenLabel.text);
            Assert.AreEqual("Now", header.NowLabel.text);
            Assert.IsTrue(then.Contains(header.ThenLabel.worldBound.center) && now.Contains(header.NowLabel.worldBound.center), "each label on its own half");
            yield return PixelAt(header.HeroPart, new Vector2(then.center.x, then.yMin + 12f), c => AssertColour(PictureColour("then.png"), c, "left half"));
            yield return PixelAt(header.HeroPart, new Vector2(now.center.x, now.yMin + 12f), c => AssertColour(PictureColour("now.png"), c, "right half"));
        }

        [UnityTest]
        public IEnumerator HeaderSpotlightCrop_EnlargesAroundTheFocus_TheRingSitsOnIt_TheFrameStaysCovered()
        {
            yield return ShowHeader("header_spotlight_crop_short_full");
            var header = GalleryHeader;
            Rect frame = header.HeroPart.worldBound, picture = header.Picture.Root.worldBound, ring = header.Ring.worldBound;
            var cover = SpotlightCropRule.CoverSize(frame.size, header.Picture.Aspect);
            Assert.AreEqual(cover.x * CardGalleryDefinitions.SpotlightZoom, picture.width, 1f, "enlarged Crop Zoom times over a covering size");
            Assert.LessOrEqual(picture.xMin, frame.xMin + 0.5f, "no empty edge on the left");
            Assert.GreaterOrEqual(picture.xMax, frame.xMax - 0.5f, "...nor on the right");
            Assert.LessOrEqual(picture.yMin, frame.yMin + 0.5f);
            Assert.GreaterOrEqual(picture.yMax, frame.yMax - 0.5f);
            var focus = new Vector2(picture.xMin + picture.width * CardGalleryDefinitions.SpotlightFocus.x, picture.yMin + picture.height * CardGalleryDefinitions.SpotlightFocus.y);
            Assert.AreEqual(focus.x, ring.center.x, 1f, "the ring is centred on the focus point (across)");
            Assert.AreEqual(focus.y, ring.center.y, 1f, "(down)");
            Assert.IsTrue(frame.Contains(ring.center), "...inside the frame");
            Assert.AreEqual(ring.width, ring.height, OnePixel(header.Ring) + 0.01f, "a round ring (each side snapped to a screen pixel)");
            Assert.AreEqual(ring.width / 2f, header.Ring.resolvedStyle.borderTopLeftRadius, 1f, "radius half the size: a circle");
        }

        [UnityTest]
        public IEnumerator Gallery_EachLookLaysOutItsPictures_ARowOutsideTheFolderIsLeftOut_AMissingFileShowsUnavailable()
        {
            GalleryBlockView gallery = null;
            yield return ShowBlock("gallery_carousel_long", v => gallery = (GalleryBlockView)v);
            Assert.AreEqual(4, gallery.Shots.Count);
            Assert.AreEqual(gallery.Track, gallery.Shots[0].Box.GetFirstAncestorOfType<ScrollView>(), "carousel: on the swipe track");
            Assert.AreEqual(gallery.Shots[0].Box.worldBound.yMin, gallery.Shots[1].Box.worldBound.yMin, 0.5f, "side by side");
            Assert.AreEqual(4, gallery.Dots.childCount, "a dot per picture");
            Assert.IsTrue(gallery.Dots[0].ClassListContains("card-gallery__dot--current"));
            gallery.Track.scrollOffset = new Vector2(gallery.Shots[2].Box.layout.x, 0f);
            yield return CardTestInput.Settle(0.15f);
            Assert.AreEqual(2, gallery.Current, "scrolled to the third picture: its dot is marked");
            Assert.IsTrue(gallery.Dots[2].ClassListContains("card-gallery__dot--current") && !gallery.Dots[0].ClassListContains("card-gallery__dot--current"));

            yield return ShowBlock("gallery_grid_long", v => gallery = (GalleryBlockView)v);
            Rect a = gallery.Shots[0].Box.worldBound, b = gallery.Shots[1].Box.worldBound, c = gallery.Shots[2].Box.worldBound;
            Assert.AreEqual(a.yMin, b.yMin, 0.5f, "grid: two per row");
            Assert.Greater(c.yMin, a.yMax - 0.5f, "the third on the next row");
            Assert.AreEqual(a.width, b.width, 0.5f, "equal cells");

            yield return ShowBlock("gallery_filmstrip_long", v => gallery = (GalleryBlockView)v);
            Assert.AreEqual("one.png", gallery.Main.Path, "filmstrip: the first picture large");
            Assert.Greater(gallery.Main.Root.worldBound.height, gallery.Shots[0].Box.worldBound.height * 2f, "...much larger than the strip");
            yield return CardTestInput.Tap(gallery.Root.panel, gallery.Shots[2].Box.worldBound.center);
            yield return null;
            Assert.AreEqual(2, gallery.Current, "a real tap on a small picture...");
            Assert.AreEqual("three.png", gallery.Main.Path, "...shows it large");
            Assert.IsFalse(_harness.Sheet.Takeover.IsOpen, "(the filmstrip's small pictures pick, they do not open the lightbox)");
            Assert.IsTrue(gallery.Shots[2].Box.ClassListContains("card-gallery__thumb--current") && !gallery.Shots[0].Box.ClassListContains("card-gallery__thumb--current"));
            Assert.AreEqual(1, _harness.Media.RefCount("one.png"), "the old large picture was given back at once (only its small one holds it)");
            Assert.AreEqual(2, _harness.Media.RefCount("three.png"), "small + large");

            yield return ShowBlock("gallery_stack_long", v => gallery = (GalleryBlockView)v);
            Assert.AreEqual(3, gallery.Shots.Count, "stack: three prints of the four");
            Assert.AreEqual("4 pictures", gallery.Count.text, "and how many there are (CardStrings)");
            Assert.AreEqual(0, gallery.Shots.Last().Page, "the first picture drawn last: on top");
            Assert.AreNotEqual(gallery.Shots[0].Box.worldBound, gallery.Shots.Last().Box.worldBound, "the prints are fanned, not stacked exactly");

            yield return ShowBlock("gallery_grid_partial", v => gallery = (GalleryBlockView)v);
            Assert.AreEqual(2, gallery.PictureCount, "outside the folder and not-a-picture rows left out; the kept one and the missing file stay");
            Assert.IsNotNull(gallery.Shots[0].Image.Texture);
            var ghost = gallery.Shots[1].Image;
            Assert.IsNull(ghost.Texture, "no such file: nothing loaded, no exception");
            Assert.IsTrue(ghost.Root.ClassListContains("card-image--unavailable"));
            Assert.AreEqual("Picture unavailable", ghost.Unavailable.text);
            Assert.IsTrue(CardTestInput.IsShown(ghost.Unavailable, gallery.Root), "the frame says so");
        }

        [UnityTest]
        public IEnumerator Gallery_ARealTapOpensTheLightbox_ChipsAndBack_KeepTheCard_AndEveryPictureIsGivenBack()
        {
            GalleryBlockView gallery = null;
            yield return ShowBlock("gallery_grid_long", v => gallery = (GalleryBlockView)v);
            var sheet = _harness.Sheet;
            var takeover = sheet.Takeover;
            float scroll = sheet.Stack.Scroll.scrollOffset.y;
            int heldOnCard = _harness.Media.HeldCount;

            yield return CardTestInput.Tap(gallery.Root.panel, gallery.Shots[2].Box.worldBound.center);
            yield return CardTestInput.Settle(0.15f);
            Assert.IsTrue(takeover.IsOpen, "a real tap on a picture opens it full screen");
            Assert.AreEqual(2, takeover.PageIndex, "...at that picture");
            Assert.AreEqual("Gate > Gallery", takeover.Crumb.text, "the breadcrumb: card title > the gallery");
            Assert.AreEqual("Back", takeover.Back.text);
            Assert.AreEqual(4, takeover.Chips.childCount, "a chip per sibling picture");
            Assert.IsTrue(takeover.Chips[2].ClassListContains("card-takeover__chip--current"));
            Assert.AreEqual(sheet.Layer.worldBound, takeover.Root.worldBound, "full screen over the card's whole layer");
            var picture = takeover.Page.Q(className: "card-image");
            Assert.IsNotNull(picture);
            yield return PixelAt(picture, picture.worldBound.center, c => AssertColour(PictureColour("three.png"), c, "the lightbox shows the tapped picture"));
            Assert.AreEqual(2, _harness.Media.RefCount("three.png"), "the lightbox loaded its own copy (grid cell + lightbox)");
            Assert.IsTrue(UIAccessibility.MeetsMinTapTarget(takeover.Back.worldBound.width, takeover.Back.worldBound.height), "Back >= 44");
            foreach (var chip in takeover.Chips.Children())
                Assert.IsTrue(UIAccessibility.MeetsMinTapTarget(chip.worldBound.width, chip.worldBound.height), "a chip >= 44");
            Assert.GreaterOrEqual(CardTestInput.Contrast(takeover.Crumb.resolvedStyle.color, CardTestInput.EffectiveBackground(takeover.Crumb)), UIAccessibility.MinRatioNormalText);
            yield return Render("Card_gallery_lightbox");

            yield return CardTestInput.Tap(takeover.Chips[0].panel, takeover.Chips[0].worldBound.center);
            yield return CardTestInput.Settle(0.15f);
            Assert.AreEqual(0, takeover.PageIndex, "a real tap on chip 1: the first picture");
            Assert.AreEqual(1, _harness.Media.RefCount("three.png"), "the page before gave its picture back");
            Assert.AreEqual(2, _harness.Media.RefCount("one.png"));
            var caption = takeover.Page.Q<Label>(className: "card-gallery__full-caption");
            StringAssert.StartsWith("The gate from the square", caption.text, "caption");
            Assert.AreEqual("Photo: Municipal archive, catalogue 12/447", takeover.Page.Q<Label>(className: "card-gallery__full-credit").text, "credit");

            yield return CardTestInput.Tap(takeover.Back.panel, takeover.Back.worldBound.center);
            yield return CardTestInput.Settle(0.15f);
            Assert.IsFalse(takeover.IsOpen, "Back closes it");
            Assert.AreEqual(0, takeover.HeldMedia, "...giving back every picture it loaded");
            Assert.AreEqual(heldOnCard, _harness.Media.HeldCount, "the card holds what it held before");
            Assert.AreEqual(1, _harness.Media.RefCount("one.png"));
            Assert.AreEqual(scroll, sheet.Stack.Scroll.scrollOffset.y, 0.01f, "the card kept its scroll position (the long real card proves it scrolled: PoiCardSceneTests)");
            Assert.AreEqual(SheetStopRule.Stop.Full, sheet.Stop, "...and its stop");

            yield return CardTestInput.Tap(gallery.Root.panel, gallery.Shots[1].Box.worldBound.center);
            yield return CardTestInput.Settle(0.15f);
            Assert.IsTrue(takeover.IsOpen);
            sheet.Hide();
            Assert.IsFalse(takeover.IsOpen, "closing the card closes its full-screen view");
            Assert.AreEqual(0, _harness.Media.HeldCount, "a closed card holds no picture at all (lazy load, release on unbind)");
        }

        [UnityTest]
        public IEnumerator BeforeAfter_ARealDragMovesTheHandle_TheCutFollows_AndTheStackDoesNotScroll()
        {
            BeforeAfterBlockView slider = null;
            yield return ShowBlock("before_after_slider_short", v => slider = (BeforeAfterBlockView)v);
            Rect frame = slider.Frame.worldBound;
            Assert.AreEqual(0.5f, slider.Position, 1e-4f, "Start At unset: half and half");
            Assert.AreEqual(frame.width * 0.5f, slider.BeforeClip.worldBound.width, 1f, "Before shows left of the handle");
            Assert.AreEqual("Before", slider.BeforeLabel.text);
            Assert.AreEqual("After", slider.AfterLabel.text);
            Assert.IsTrue(UIAccessibility.MeetsMinTapTarget(slider.Knob.worldBound.width, slider.Knob.worldBound.height), "the knob >= 44");
            yield return PixelAt(slider.Frame, new Vector2(frame.xMin + frame.width * 0.25f, frame.center.y + 20f), c => AssertColour(PictureColour("before.png"), c, "left of the handle: Before"));
            yield return PixelAt(slider.Frame, new Vector2(frame.xMin + frame.width * 0.75f, frame.center.y + 20f), c => AssertColour(PictureColour("after.png"), c, "right of the handle: After"));
            float scroll = _harness.Sheet.Stack.Scroll.scrollOffset.y;

            yield return CardTestInput.DragFrom(slider.Frame.panel, new Vector2(frame.center.x, frame.center.y), new Vector2(frame.width * 0.3f, 40f));
            yield return null;
            Assert.AreEqual(0.8f, slider.Position, 0.02f, "a real drag to 80% of the width");
            Assert.AreEqual(frame.width * slider.Position, slider.BeforeClip.worldBound.width, 1f, "the cut follows");
            Assert.AreEqual(frame.xMin + frame.width * slider.Position, slider.Handle.worldBound.center.x, 1f, "the handle follows");
            Assert.AreEqual(scroll, _harness.Sheet.Stack.Scroll.scrollOffset.y, 0.01f, "the drag was the slider's: the stack did not scroll");
            Assert.AreEqual(frame.width, slider.Before.Root.worldBound.width, 1f, "Before keeps its full width inside the cut (cut, never squeezed)");
            yield return PixelAt(slider.Frame, new Vector2(frame.xMin + frame.width * 0.7f, frame.center.y + 20f), c => AssertColour(PictureColour("before.png"), c, "70% is now Before"));
            yield return Render("Card_before_after_dragged");

            yield return ShowBlock("before_after_slider_labels", v => slider = (BeforeAfterBlockView)v);
            Assert.AreEqual(0.3f, slider.Position, 1e-4f, "Start At");
            Assert.AreEqual("1740, before the earthquake", slider.BeforeLabel.text, "the block's own words");
            Assert.AreEqual("Today", slider.AfterLabel.text);

            yield return ShowBlock("before_after_slider_missing", v => slider = (BeforeAfterBlockView)v);
            Assert.IsNull(slider.After.Texture, "a missing After file: the unavailable frame, no exception");
            Assert.IsNotNull(slider.Before.Texture);
        }

        [UnityTest]
        public IEnumerator ZoomImage_CtrlWheelZoomsAboutThePointer_ADragPans_AndAtOneAPlainWheelScrollsTheStack()
        {
            ZoomImageBlockView zoom = null;
            yield return ShowBlock("zoom_image_pinch_short", v => zoom = (ZoomImageBlockView)v);
            Rect frame = zoom.Frame.worldBound, picture = zoom.Image.Root.worldBound;
            Assert.AreEqual(1f, zoom.Scale);
            Assert.AreEqual(frame.height, picture.height, 1f, "1x: the square picture fits the frame's height (whole, not cropped)");
            Assert.AreEqual(frame.center.x, picture.center.x, 1f, "...centred");
            Assert.AreEqual("Pinch to zoom in", zoom.Hint.text);

            var at = frame.center + new Vector2(40f, 10f);
            Vector2 before = (at - picture.position) / picture.width;
            yield return CardTestInput.CtrlWheel(zoom.Frame.panel, at, -12f);
            yield return null;
            Assert.AreEqual(2f, zoom.Scale, 0.01f, "one Ctrl + wheel step (12 notches) doubles the scale");
            picture = zoom.Image.Root.worldBound;
            Vector2 after = (at - picture.position) / picture.width;
            Assert.AreEqual(before.x, after.x, 0.01f, "the picture point under the pointer stayed under it (across)");
            Assert.AreEqual(before.y, after.y, 0.01f, "(down)");
            yield return CardTestInput.CtrlWheel(zoom.Frame.panel, at, -60f);
            yield return null;
            Assert.AreEqual(ZoomPanRule.MaxScale, zoom.Scale, 0.01f, "never past 4x");

            float scroll = _harness.Sheet.Stack.Scroll.scrollOffset.y;
            float x0 = zoom.Image.Root.worldBound.x;
            yield return CardTestInput.DragFrom(zoom.Frame.panel, frame.center, new Vector2(-30f, 0f));
            yield return null;
            Assert.AreEqual(x0 - 30f, zoom.Image.Root.worldBound.x, 1f, "a real one-finger drag moves the enlarged picture");
            Assert.AreEqual(scroll, _harness.Sheet.Stack.Scroll.scrollOffset.y, 0.01f, "...and does not scroll the stack");
            Assert.LessOrEqual(zoom.Image.Root.worldBound.xMin, frame.xMin + 0.5f, "never an empty edge");
            yield return Render("Card_zoom_image_zoomed");

            yield return CardTestInput.CtrlWheel(zoom.Frame.panel, at, 200f);
            yield return null;
            Assert.AreEqual(1f, zoom.Scale, 0.01f, "zoomed back out: 1x");
            Assert.AreEqual(frame.center.x, zoom.Image.Root.worldBound.center.x, 1f, "the whole picture, centred again");

            yield return CardTestInput.Wheel(zoom.Frame, -3f);
            yield return CardTestInput.Settle(0.15f);
            Assert.AreEqual(1f, zoom.Scale, "a plain wheel never zooms (it is the stack's: PoiCardTapZoomTests scrolls the long real card with it)");
        }
    }
}
