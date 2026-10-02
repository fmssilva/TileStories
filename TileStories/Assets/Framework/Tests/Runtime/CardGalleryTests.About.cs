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
    // Phase A promises of the about family (_3.1 section 3): rich_text, quick_facts, fun_fact, status, process_steps, swatches, actions,
    // pull_quote, and the header's picture looks. The fixture, its SetUp and the shared helpers are in CardGalleryTests.cs.
    public partial class CardGalleryTests
    {
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
            Assert.AreEqual(2, actions.Actions.Count, "an action this framework does not know (listen) is left out; the button with no words is NOT: it reads its action's card text");
            Assert.AreEqual("Show me where it is", actions.Actions[0].Label.text, "the same words the Show On Wall block's button says");
            Assert.AreEqual("See it on the wall", actions.Actions[1].Label.text);

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

        [UnityTest]
        public IEnumerator HeaderImageParallax_ARealScroll_MovesThePictureAtHalfTheFramesSpeed_TheTitleStaysPinned()
        {
            // A real second block under the header (like a real POI card): a lone header never overflows the Full
            // stop's viewport, so there is nothing to scroll and the parallax law never engages.
            var wall = CardGalleryDefinitions.Taxonomy();
            var poi = new POIData
            {
                id = "header_image_parallax_real_scroll", name = "The Royal Palace of the Kings of Portugal and of the Algarves, before the earthquake",
                category = CardGalleryDefinitions.CategoryKey, hierarchy_level_key = CardGalleryDefinitions.LevelKey,
            };
            var headerBlock = new BlockInstanceData { key = "block_1", kind = BuiltInBlocks.HeaderKind, variant = BuiltInBlocks.HeaderImageParallax };
            headerBlock.fields.Add(LocalizedField(BlockStackBuilder.HeaderTitleField, poi.name));
            headerBlock.fields.Add(LocalizedField(BlockStackBuilder.HeaderSubtitleField,
                "Seen from the river on the panel, with the Customs House, the chapel and the long arcade that the fire destroyed"));
            headerBlock.fields.Add(new BlockFieldValue { key = BuiltInBlocks.HeaderImageField, asset = "wide.png" });
            var body = new BlockInstanceData { key = "block_2", kind = BuiltInBlocks.RichTextKind, variant = BuiltInBlocks.RichTextPlain };
            body.fields.Add(LongBody());
            poi.card.blocks.Add(headerBlock);
            poi.card.blocks.Add(body);

            _harness.ShowPoi(poi, wall, null, SheetStopRule.Stop.Full);
            yield return CardTestInput.Settle();

            var header = GalleryHeader;
            var stack = _harness.Sheet.Stack;
            Assert.IsTrue(header.HasHero);
            // - a point inside the hero shows the picture itself (its own colour, not the frame's or the card's)
            yield return PixelAt(header.HeroPart, header.HeroPart.worldBound.center + new Vector2(10f, 10f), c => AssertColour(PictureColour("wide.png"), c, "the hero shows wide.png"));
            float scrollRange = stack.Scroll.contentContainer.layout.height - stack.Scroll.contentViewport.layout.height;
            Assert.Greater(scrollRange, HeaderCollapseRule.CollapseAfter, "precondition: the real body gives the stack genuine scroll range");
            float frameTop = header.HeroPart.worldBound.yMin, pictureTop = header.Picture.Root.worldBound.yMin, titleTop = header.PeekPart.worldBound.yMin;

            // A real wheel first, to prove the stack itself really scrolls with a real event (the original bug: a lone
            // header had nothing to scroll, so this never fired at all). Its exact resting offset depends on the
            // ScrollView's own built-in elastic/kinetic wheel physics (Unity's, not this project's), which settle
            // over real elapsed time -- proven separately by this precondition, not by the pixel maths below, which
            // needs a KNOWN small offset instead (see next comment).
            yield return CardTestInput.Wheel(stack.Scroll.contentViewport, 1f, frames: 1);
            yield return CardTestInput.Settle(0.2f);
            Assert.Greater(stack.Scroll.scrollOffset.y, 0f, "precondition: the real wheel scrolled the stack");

            // The real wheel above can land anywhere (Unity's own kinetic deceleration, real elapsed time) and may
            // overshoot CollapseAfter, collapsing the header; HeaderCollapseRule's hysteresis then only reopens it
            // back near the very top (OpenAtTop = 0.5), so a known offset set directly afterward would NOT reopen it
            // on its own. Back to the top first (a real state every card starts from) resets that, then to a KNOWN
            // small offset -- the same `scrollOffset` property a drag or a wheel both ultimately move -- well inside
            // the header's open window, so the maths below are checked against a fixed point rather than at the mercy
            // of Unity's own wheel deceleration timing (the same class of machine-speed dependence `_3.1` step 7B
            // already had to design out of the sheet's own drag).
            stack.Scroll.scrollOffset = Vector2.zero;
            yield return null;
            Assert.IsFalse(stack.HeaderCollapsed, "precondition: back at the top, the header re-opened");
            float knownScroll = HeaderCollapseRule.CollapseAfter * 0.5f;
            stack.Scroll.scrollOffset = new Vector2(0f, knownScroll);
            yield return null;
            float scrolled = stack.Scroll.scrollOffset.y;
            Assert.AreEqual(knownScroll, scrolled, 0.5f, "precondition: the known offset actually landed (not clamped away)");
            Assert.IsFalse(stack.HeaderCollapsed, "precondition: still inside the header's open window");
            float frameMoved = frameTop - header.HeroPart.worldBound.yMin;
            float pictureMoved = pictureTop - header.Picture.Root.worldBound.yMin;
            Assert.AreEqual(scrolled * HeaderBlockView.ParallaxFactor, header.ParallaxOffset, 0.5f, "the picture slides by half the scroll inside its frame");
            Assert.AreEqual(frameMoved * (1f - HeaderBlockView.ParallaxFactor), pictureMoved, OnePixel(stack.Scroll) + 0.5f,
                "on screen the picture moves at half the frame's speed (" + pictureMoved + " vs " + frameMoved + ")");
            Assert.AreEqual(titleTop, header.PeekPart.worldBound.yMin, 0.5f, "the title stays pinned");
            yield return Render("Card_header_image_parallax_scrolled");

            stack.Scroll.scrollOffset = Vector2.zero;
            yield return null;
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
    }
}
