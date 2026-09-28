using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // knowledge_check's pure rules (_3.1 Tier 3): which question rows each look can show and how a swipe is judged, and the
    // feedback vote rule -- plus how the card's builder uses them (the variant it resolves, the Block Library's default).
    public class KnowledgeCheckRuleTests
    {
        private const string MultipleChoice = BuiltInBlocks.KnowledgeCheckMultipleChoice;
        private const string TrueFalse = BuiltInBlocks.KnowledgeCheckTrueFalseSwipe;
        private const string ImageChoice = BuiltInBlocks.KnowledgeCheckImageChoice;

        private static List<LocalizedEntry> En(string value) => new() { new LocalizedEntry { lang = "en", value = value } };

        // One authored row: fields given as key -> text; "correct" and "is_true" are set through their own arguments
        private static BlockItemData Row(string question, string explanation, string[] options = null, string[] images = null, string correct = null, bool isTrue = false)
        {
            var row = new BlockItemData();
            if (question != null) row.fields.Add(new BlockItemFieldValue { key = "question", text = En(question) });
            if (explanation != null) row.fields.Add(new BlockItemFieldValue { key = "explanation", text = En(explanation) });
            if (options != null)
                for (int i = 0; i < options.Length; i++)
                    if (options[i] != null) row.fields.Add(new BlockItemFieldValue { key = "option_" + (i + 1), text = En(options[i]) });
            if (images != null)
                for (int i = 0; i < images.Length; i++)
                    if (images[i] != null) row.fields.Add(new BlockItemFieldValue { key = "image_" + (i + 1), asset = images[i] });
            if (correct != null) row.fields.Add(new BlockItemFieldValue { key = "correct", value = correct });
            if (isTrue) row.fields.Add(new BlockItemFieldValue { key = "is_true", flag = true });
            return row;
        }

        private static BlockInstanceData Block(string variant, params BlockItemData[] rows)
        {
            var block = new BlockInstanceData { key = "block_2", kind = BuiltInBlocks.KnowledgeCheckKind, variant = variant };
            var questions = new BlockFieldValue { key = "questions" };
            questions.items.AddRange(rows);
            block.fields.Add(questions);
            return block;
        }

        private static KnowledgeCheckRule.Problem ReadRow(BlockItemData row, string variant, out KnowledgeCheckRule.Question question) =>
            KnowledgeCheckRule.Read(new BlockFieldReader(new BlockInstanceData(), "en", "en"), row, 0, variant, out question);

        // ---------------- multiple_choice ----------------

        [Test]
        public void MultipleChoice_ShowsTheOptionsThatHaveText_InOrder_AndTheRightSlotBecomesAShownIndex()
        {
            // - slot 1 blank: the shown options are slots 2, 3, 4; "correct 3" is the SECOND shown option
            var row = Row("Which tower?", "The keep.", new[] { "  ", "Gate", "Keep", "Wall" }, correct: "3");
            Assert.AreEqual(KnowledgeCheckRule.Problem.None, ReadRow(row, MultipleChoice, out var q));
            CollectionAssert.AreEqual(new[] { "Gate", "Keep", "Wall" }, q.Options.Select(o => o.Text).ToList());
            Assert.AreEqual(1, q.Correct, "slot 3 = the second shown option");
            Assert.AreEqual("Which tower?", q.Text);
            Assert.AreEqual("The keep.", q.Explanation);
            Assert.IsTrue(KnowledgeCheckRule.IsCorrect(q, 1));
            Assert.IsFalse(KnowledgeCheckRule.IsCorrect(q, 0));
            Assert.IsFalse(KnowledgeCheckRule.IsCorrect(q, -1), "no answer is not the right one");
            Assert.IsFalse(KnowledgeCheckRule.IsCorrect(null, 0));
        }

        [TestCase(null, "why", "a", "b", "1", KnowledgeCheckRule.Problem.NoQuestion)]
        [TestCase("  ", "why", "a", "b", "1", KnowledgeCheckRule.Problem.NoQuestion)]
        [TestCase("q", null, "a", "b", "1", KnowledgeCheckRule.Problem.NoExplanation)]
        [TestCase("q", " ", "a", "b", "1", KnowledgeCheckRule.Problem.NoExplanation)]
        [TestCase("q", "why", "a", null, "1", KnowledgeCheckRule.Problem.TooFewOptions)]
        [TestCase("q", "why", null, null, "1", KnowledgeCheckRule.Problem.TooFewOptions)]
        [TestCase("q", "why", "a", "b", null, KnowledgeCheckRule.Problem.NoCorrect)]
        [TestCase("q", "why", "a", "b", "0", KnowledgeCheckRule.Problem.NoCorrect)]
        [TestCase("q", "why", "a", "b", "5", KnowledgeCheckRule.Problem.NoCorrect)]
        [TestCase("q", "why", "a", "b", "two", KnowledgeCheckRule.Problem.NoCorrect)]
        [TestCase("q", "why", "a", "b", "3", KnowledgeCheckRule.Problem.CorrectIsEmpty)]
        [TestCase("q", "why", "a", "b", "2", KnowledgeCheckRule.Problem.None)]
        public void MultipleChoice_EachWayARowCannotBeShown_HasItsOwnProblem(string question, string explanation, string first, string second, string correct,
            KnowledgeCheckRule.Problem expected)
        {
            var row = Row(question, explanation, new[] { first, second }, correct: correct);
            Assert.AreEqual(expected, ReadRow(row, MultipleChoice, out var q));
            Assert.AreEqual(expected == KnowledgeCheckRule.Problem.None, q != null, "a question only when there is no problem");
        }

        [Test]
        public void MultipleChoice_FourOptionsAreTheMost_AndAFifthSlotDoesNotExist()
        {
            var row = Row("q", "why", new[] { "a", "b", "c", "d", "e" }, correct: "4");
            Assert.AreEqual(KnowledgeCheckRule.Problem.None, ReadRow(row, MultipleChoice, out var q));
            Assert.AreEqual(KnowledgeCheckRule.MaxOptions, q.Options.Count);
            Assert.AreEqual(3, q.Correct);
        }

        // ---------------- image_choice ----------------

        [Test]
        public void ImageChoice_CountsPicturesNotText_TheCaptionIsOptional_AndARefusedPathIsNoPicture()
        {
            var row = Row("Which?", "This one.", options: new[] { "First", null, "Third" }, images: new[] { "a.png", "../outside.png", "c.jpg", "notes.txt" }, correct: "3");
            Assert.AreEqual(KnowledgeCheckRule.Problem.None, ReadRow(row, ImageChoice, out var q));
            CollectionAssert.AreEqual(new[] { "a.png", "c.jpg" }, q.Options.Select(o => o.Image).ToList(), "a path outside the folder and a text file are no pictures");
            CollectionAssert.AreEqual(new[] { "First", "Third" }, q.Options.Select(o => o.Text).ToList(), "the captions follow their pictures");
            Assert.AreEqual(1, q.Correct, "slot 3 is the second SHOWN picture");

            var captionless = Row("Which?", "This one.", images: new[] { "a.png", "b.png" }, correct: "1");
            Assert.AreEqual(KnowledgeCheckRule.Problem.None, ReadRow(captionless, ImageChoice, out var q2), "no caption needed");
            Assert.AreEqual("", q2.Options[0].Text);
        }

        [Test]
        public void ImageChoice_NeedsTwoPictures_AndTheRightOneMustHaveOne()
        {
            Assert.AreEqual(KnowledgeCheckRule.Problem.TooFewOptions, ReadRow(Row("q", "why", new[] { "a", "b", "c" }, images: new[] { "a.png" }, correct: "1"), ImageChoice, out _),
                "text options are not pictures");
            Assert.AreEqual(KnowledgeCheckRule.Problem.CorrectIsEmpty, ReadRow(Row("q", "why", images: new[] { "a.png", "b.png" }, correct: "3"), ImageChoice, out _));
            Assert.AreEqual(KnowledgeCheckRule.Problem.NoCorrect, ReadRow(Row("q", "why", images: new[] { "a.png", "b.png" }), ImageChoice, out _));
        }

        // ---------------- true_false_swipe ----------------

        [Test]
        public void TrueFalse_NeedsOnlyTheStatementAndItsExplanation_TrueIsChoiceZero_FalseChoiceOne()
        {
            Assert.AreEqual(KnowledgeCheckRule.Problem.None, ReadRow(Row("The wall is old.", "It is.", isTrue: true), TrueFalse, out var yes));
            Assert.AreEqual(KnowledgeCheckRule.ChoiceTrue, yes.Correct);
            Assert.AreEqual(2, yes.Options.Count, "two fixed choices");
            Assert.AreEqual(KnowledgeCheckRule.Problem.None, ReadRow(Row("The wall is new.", "It is not."), TrueFalse, out var no));
            Assert.AreEqual(KnowledgeCheckRule.ChoiceFalse, no.Correct);
            Assert.IsTrue(KnowledgeCheckRule.IsCorrect(no, KnowledgeCheckRule.ChoiceFalse));

            var withOptions = Row("Statement.", "Why.", new[] { "ignored", "also ignored" }, correct: "4", isTrue: true);
            Assert.AreEqual(KnowledgeCheckRule.Problem.None, ReadRow(withOptions, TrueFalse, out var ignored), "options and correct belong to the other looks");
            Assert.AreEqual(KnowledgeCheckRule.ChoiceTrue, ignored.Correct);
            Assert.AreEqual(KnowledgeCheckRule.Problem.NoQuestion, ReadRow(Row(null, "Why."), TrueFalse, out _));
            Assert.AreEqual(KnowledgeCheckRule.Problem.NoExplanation, ReadRow(Row("Statement.", null), TrueFalse, out _));
        }

        // ---------------- the rows of a block ----------------

        [Test]
        public void TheRowsOfABlock_KeepTheirAuthoredIndex_WhenOthersAreLeftOut()
        {
            var block = Block(MultipleChoice,
                Row("first", "why", new[] { "a", "b" }, correct: "1"),
                Row("no explanation", null, new[] { "a", "b" }, correct: "1"),
                Row("third", "why", new[] { "a", "b" }, correct: "2"));
            var questions = KnowledgeCheckRule.Questions(block, MultipleChoice);
            CollectionAssert.AreEqual(new[] { "first", "third" }, questions.Select(q => q.Text).ToList());
            CollectionAssert.AreEqual(new[] { 0, 2 }, questions.Select(q => q.Row).ToList(), "the saved answer of row 2 stays under 2 (leaving row 1 out moves nothing)");
        }

        [Test]
        public void TheSameBlock_ShowsDifferentRows_ForEachLook()
        {
            // - text options, no pictures, no is_true: the multiple-choice look shows it, the picture look does not; the
            //   true / false look shows it too (it ignores options) and a block with no rows shows nothing at all
            var block = Block(MultipleChoice, Row("q", "why", new[] { "a", "b" }, correct: "2"));
            Assert.AreEqual(1, KnowledgeCheckRule.Questions(block, MultipleChoice).Count);
            Assert.AreEqual(0, KnowledgeCheckRule.Questions(block, ImageChoice).Count, "no pictures");
            Assert.AreEqual(1, KnowledgeCheckRule.Questions(block, TrueFalse).Count);
            Assert.AreEqual(0, KnowledgeCheckRule.Questions(new BlockInstanceData(), MultipleChoice).Count);
            Assert.AreEqual(0, KnowledgeCheckRule.Questions(null, MultipleChoice).Count);
        }

        [Test]
        public void TheTextsAreReadInTheVisitorsLanguage_ThenTheFallback()
        {
            var row = new BlockItemData();
            row.fields.Add(new BlockItemFieldValue { key = "question", text = new List<LocalizedEntry> { new() { lang = "en", value = "Which?" }, new() { lang = "pt", value = "Qual?" } } });
            row.fields.Add(new BlockItemFieldValue { key = "explanation", text = En("Because.") });
            row.fields.Add(new BlockItemFieldValue { key = "is_true", flag = true });
            var q = KnowledgeCheckRule.Questions(Block(TrueFalse, row), TrueFalse, "pt", "en").Single();
            Assert.AreEqual("Qual?", q.Text);
            Assert.AreEqual("Because.", q.Explanation, "no Portuguese explanation: the fallback language");
        }

        // ---------------- the swipe ----------------

        [Test]
        public void ASwipe_RightIsTrue_LeftIsFalse_AShortOrVerticalDragIsNeither()
        {
            const float width = 300f;
            float far = width * SwipePageRule.MinTravelShare;
            Assert.AreEqual(KnowledgeCheckRule.ChoiceTrue, KnowledgeCheckRule.SwipeChoice(new Vector2(far + 1f, 0f), width), "right");
            Assert.AreEqual(KnowledgeCheckRule.ChoiceFalse, KnowledgeCheckRule.SwipeChoice(new Vector2(-far - 1f, 0f), width), "left");
            Assert.AreEqual(KnowledgeCheckRule.ChoiceTrue, KnowledgeCheckRule.SwipeChoice(new Vector2(far + 0.5f, 2f), width), "just far enough, a little off level");
            Assert.AreEqual(-1, KnowledgeCheckRule.SwipeChoice(new Vector2(far - 1f, 0f), width), "not far enough");
            Assert.AreEqual(-1, KnowledgeCheckRule.SwipeChoice(new Vector2(far + 20f, far + 20f), width), "as far up as across: a scroll, not a swipe");
            Assert.AreEqual(-1, KnowledgeCheckRule.SwipeChoice(new Vector2(0f, 200f), width), "vertical");
            Assert.AreEqual(-1, KnowledgeCheckRule.SwipeChoice(new Vector2(500f, 0f), 0f), "a card with no width");
        }

        // ---------------- the builder ----------------

        [Test]
        public void TheBuilder_ShowsAKnowledgeCheck_OnlyWhenTheLookItWillBeDrawnInCanShowARow()
        {
            // - true / false rows have no options: multiple_choice would leave them out, true_false_swipe shows them. The look the
            //   builder resolves (the block's own, else the Block Library's default) must reach the kind's ShowsFor
            var rows = Block("", Row("Statement.", "Why.", isTrue: true));
            var poi = new POIData { id = "p", name = "Tower" };
            poi.card.blocks.Add(rows);

            var byDefault = BlockStackBuilder.Build(poi, new CardSettings(), BlockRegistry.Shared, new List<POIData> { poi });
            Assert.AreEqual(BlockStackBuilder.SkipReason.NotForThisPoint, byDefault.Skipped.Single().Reason, "the default look is multiple choice: no options, nothing to show");

            var settings = new CardSettings();
            settings.kinds.Add(new BlockKindSetting { kind = BuiltInBlocks.KnowledgeCheckKind, default_variant = TrueFalse });
            var libraryDefault = BlockStackBuilder.Build(poi, settings, BlockRegistry.Shared, new List<POIData> { poi });
            Assert.IsEmpty(libraryDefault.Skipped, "the Block Library made true / false its default");
            Assert.AreEqual(TrueFalse, libraryDefault.Entries.Last().Variant);

            rows.variant = MultipleChoice;
            var own = BlockStackBuilder.Build(poi, settings, BlockRegistry.Shared, new List<POIData> { poi });
            Assert.AreEqual(BlockStackBuilder.SkipReason.NotForThisPoint, own.Skipped.Single().Reason, "the block's own look wins over the library's");
        }

        [Test]
        public void TheBuilder_SkipsAKnowledgeCheckWithNoQuestionRows_AndOneWhoseRowsAreAllIncomplete_ButShowsOneCompleteRow()
        {
            var poi = new POIData { id = "p", name = "Tower" };
            // - the kind's own ShowsFor runs before the generic required-field check, so both cases read as "nothing to show" and
            //   the Editor says why in the kind's own words (NotShownForPoiNote)
            poi.card.blocks.Add(new BlockInstanceData { key = "block_2", kind = BuiltInBlocks.KnowledgeCheckKind });
            Assert.AreEqual(BlockStackBuilder.SkipReason.NotForThisPoint, BlockStackBuilder.Build(poi, new CardSettings(), BlockRegistry.Shared).Skipped.Single().Reason, "no rows");
            poi.card.blocks[0] = Block(MultipleChoice, Row("q", null, new[] { "a", "b" }, correct: "1"));
            Assert.AreEqual(BlockStackBuilder.SkipReason.NotForThisPoint, BlockStackBuilder.Build(poi, new CardSettings(), BlockRegistry.Shared).Skipped.Single().Reason,
                "a row with no explanation");
            StringAssert.Contains("question and an explanation", BuiltInBlocks.KnowledgeCheck.NotShownForPoiNote);
            poi.card.blocks[0] = Block(MultipleChoice, Row("q", "why", new[] { "a", "b" }, correct: "1"));
            var shown = BlockStackBuilder.Build(poi, new CardSettings(), BlockRegistry.Shared);
            Assert.IsEmpty(shown.Skipped);
            Assert.AreEqual(BuiltInBlocks.KnowledgeCheckKind, shown.Entries.Last().Definition.Key);
        }

        [Test]
        public void ARegisteredKind_MayOnlyNameAToggleAsItsShowAfterViewedField()
        {
            BlockKindDefinition Kind(string field, BlockFieldType type) => new()
            {
                Key = "quiz_like", Family = "play", Variants = new[] { "plain" }, DefaultVariant = "plain", DisplayModes = new[] { CardOptions.DisplayInline },
                Fields = new[] { new BlockFieldDefinition { Key = "wait", Type = type } }, ShowAfterViewedField = field,
            };
            Assert.IsNull(BlockRegistry.Validate(Kind("wait", BlockFieldType.Toggle)), "a Toggle of the kind");
            StringAssert.Contains("show-after-viewed", BlockRegistry.Validate(Kind("wait", BlockFieldType.LocalizedText)), "a text field is no switch");
            StringAssert.Contains("show-after-viewed", BlockRegistry.Validate(Kind("other", BlockFieldType.Toggle)), "a field the kind does not have");
            Assert.AreEqual(BuiltInBlocks.KnowledgeCheckShowAfterViewedField, BuiltInBlocks.KnowledgeCheck.ShowAfterViewedField);
            Assert.IsNull(BlockRegistry.Validate(BuiltInBlocks.KnowledgeCheck), "the built-in kind is valid");
            Assert.IsNull(BlockRegistry.Validate(BuiltInBlocks.Feedback), "and so is feedback");
        }

        // ---------------- feedback ----------------

        [Test]
        public void AFeedbackVote_IsThumbsZeroOrOne_OrStarsOneToFive_AndReportedAsWords()
        {
            Assert.IsTrue(FeedbackRule.IsValid(BuiltInBlocks.FeedbackThumbs, 0));
            Assert.IsTrue(FeedbackRule.IsValid(BuiltInBlocks.FeedbackThumbs, 1));
            Assert.IsFalse(FeedbackRule.IsValid(BuiltInBlocks.FeedbackThumbs, 2), "no third thumb");
            Assert.IsFalse(FeedbackRule.IsValid(BuiltInBlocks.FeedbackThumbs, -1));
            Assert.IsFalse(FeedbackRule.IsValid(BuiltInBlocks.FeedbackStars, 0), "no zero stars");
            for (int star = 1; star <= FeedbackRule.StarCount; star++) Assert.IsTrue(FeedbackRule.IsValid(BuiltInBlocks.FeedbackStars, star), star + " stars");
            Assert.IsFalse(FeedbackRule.IsValid(BuiltInBlocks.FeedbackStars, FeedbackRule.StarCount + 1));

            Assert.AreEqual("up", FeedbackRule.EventValue(BuiltInBlocks.FeedbackThumbs, FeedbackRule.ThumbUp));
            Assert.AreEqual("down", FeedbackRule.EventValue(BuiltInBlocks.FeedbackThumbs, FeedbackRule.ThumbDown));
            Assert.AreEqual("4", FeedbackRule.EventValue(BuiltInBlocks.FeedbackStars, 4));

            Assert.AreEqual(4, FeedbackRule.Stored(BuiltInBlocks.FeedbackStars, 4));
            Assert.AreEqual(-1, FeedbackRule.Stored(BuiltInBlocks.FeedbackThumbs, 4), "a 4 stored before the look was switched to thumbs is no vote");
            Assert.AreEqual(-1, FeedbackRule.Stored(BuiltInBlocks.FeedbackStars, 0));
            Assert.AreEqual(-1, FeedbackRule.Stored(BuiltInBlocks.FeedbackStars, -1), "nothing stored");
        }
    }
}
