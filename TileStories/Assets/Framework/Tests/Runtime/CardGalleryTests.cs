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

        // Show a block entry and scroll its block into view; returns the block's view
        private IEnumerator ShowBlock(string name, System.Action<IBlockView> got)
        {
            _harness.Show(IndexOf(name));
            yield return CardTestInput.Settle();
            var views = _harness.Sheet.Stack.BoundViews;
            Assert.AreEqual(2, views.Count, name + ": the header + the block");
            // - a footer block (sticky) is pinned outside the scroll: nothing to scroll to
            if (views[1].Root.parent == _harness.Sheet.Stack.Scroll.contentContainer) _harness.Sheet.Stack.Scroll.ScrollTo(views[1].Root);
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
            Assert.AreEqual("Civic Buildings - Landmark", header.ChipText, "the chip names the category and level, never keys");
            bool subtitleExpected = entry.Variant == BuiltInBlocks.HeaderTextOnly && entry.Subtitle.Length > 0;
            Assert.AreEqual(subtitleExpected, header.SubtitleShown, "compact never shows a subtitle");

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
                Assert.GreaterOrEqual(label.worldBound.xMin, card.xMin - 0.5f, what + " starts inside the card");
                Assert.LessOrEqual(label.worldBound.xMax, card.xMax + 0.5f, what + " wraps inside the card");
                bool large = label.resolvedStyle.fontSize >= 24f || (label.resolvedStyle.fontSize >= 18.66f && label.resolvedStyle.unityFontStyleAndWeight != FontStyle.Normal);
                float ratio = CardTestInput.Contrast(label.resolvedStyle.color, CardTestInput.EffectiveBackground(label));
                Assert.GreaterOrEqual(ratio, large ? UIAccessibility.MinRatioLargeTextOrUIComponent : UIAccessibility.MinRatioNormalText, what + " contrast");
            }
            Assert.Greater(texts, 0, name + ": the block shows text (not vacuous)");
            foreach (var tap in view.Root.Query(className: "card-tap").ToList().Where(e => CardTestInput.IsShown(e, view.Root)))
                Assert.IsTrue(UIAccessibility.MeetsMinTapTarget(tap.worldBound.width, tap.worldBound.height), name + ": a tap target is " + tap.worldBound.size);

            yield return Render("Card_" + entry.Name);
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

        // ---------------- sources ----------------

        [UnityTest]
        public IEnumerator Sources_TheConfidenceChip_FollowsContentStatus_OnlyInTheWithConfidenceLook()
        {
            SourcesBlockView sources = null;
            yield return ShowBlock("sources_with_confidence_short", v => sources = (SourcesBlockView)v);
            Assert.AreEqual("Sources", sources.Heading.text, "the heading comes from the card strings table");
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
            Assert.AreEqual(sheet.Stack.Footer, actions.Root.parent, "sticky: pinned in the card's footer, outside the scroll");
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
            Assert.AreEqual(sheet.Stops.Full, sheet.Root.resolvedStyle.height, 0.5f);

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
    }
}
