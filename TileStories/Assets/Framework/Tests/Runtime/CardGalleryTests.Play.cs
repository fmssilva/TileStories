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
    // Phase A promises of the play family: knowledge_check, poll, collect.
    public partial class CardGalleryTests
    {
        // ---------------- Tier 3 group A: knowledge_check and feedback (_3.1 step 8A) ----------------

        // The border colour a real render draws around an element (the verdict's colour, from the tokens)
        private static Color BorderOf(VisualElement e) => e.resolvedStyle.borderTopColor;

        // The mark a choice shows (a tick or a cross), or null when it shows none: the element must really be laid out, not only classed
        private static CardIcons.Shape? MarkOf(KnowledgeCheckBlockView.Choice choice) =>
            CardTestInput.IsShown(choice.Mark, choice.Button) && choice.Mark.worldBound.width > 4f ? choice.Mark.Kind : (CardIcons.Shape?)null;

        // 8A-fix: right / wrong is never the colour alone -- the right choice carries a tick, the wrong one picked a cross, the others
        // nothing, and the verdict's words start with the same shape. `wrongChoice` -1 = nothing wrong to show.
        private static void AssertMarks(KnowledgeCheckBlockView quiz, int wrongChoice, int rightChoice)
        {
            for (int i = 0; i < quiz.Choices.Count; i++)
            {
                CardIcons.Shape? expected = i == rightChoice ? CardIcons.Shape.Tick : i == wrongChoice ? CardIcons.Shape.Cross : (CardIcons.Shape?)null;
                Assert.AreEqual(expected, MarkOf(quiz.Choices[i]), "choice " + i + " carries its own mark, as an element and not only a class");
            }
            Assert.IsTrue(CardTestInput.IsShown(quiz.VerdictMark, quiz.Result), "the verdict has its icon");
            Assert.AreEqual(wrongChoice >= 0 ? CardIcons.Shape.Cross : CardIcons.Shape.Tick, quiz.VerdictMark.Kind, "a tick before a right verdict, a cross before a wrong one");
            Assert.Less(quiz.VerdictMark.worldBound.xMax, quiz.Verdict.worldBound.xMin + 1f, "the icon comes before the words");
        }

        // 8A-fix: the question of the two option looks is plain prompt text: no box, no border, and it never takes the verdict's colour
        private static void AssertPlainPrompt(KnowledgeCheckBlockView quiz)
        {
            Assert.AreEqual(0f, quiz.Stage.resolvedStyle.borderTopWidth, "no border round the question");
            Assert.AreEqual(0f, quiz.Stage.resolvedStyle.backgroundColor.a, 0.001f, "no box behind the question");
            Assert.AreEqual(0f, quiz.Stage.resolvedStyle.paddingLeft, "no card padding");
        }

        [UnityTest]
        public IEnumerator KnowledgeCheck_MultipleChoice_AWrongTapSaysActually_ExplainsMarksTheRightOne_IsRemembered_AndNeverRetried()
        {
            const string entry = "knowledge_check_multiple_choice_short";
            KnowledgeCheckBlockView quiz = null;
            yield return ShowBlock(entry, v => quiz = (KnowledgeCheckBlockView)v);
            Assert.AreEqual(1, quiz.Count);
            Assert.AreEqual(CardGalleryDefinitions.KeepQuestion, quiz.Prompt.text);
            CollectionAssert.AreEqual(new[] { "The curtain wall", "The keep", "The gatehouse" }, quiz.Choices.Select(c => c.Text.text).ToList());
            Assert.IsFalse(quiz.Answered, "a fresh visitor: nothing answered");
            Assert.IsFalse(CardTestInput.IsShown(quiz.Result, quiz.Root), "no verdict before an answer");
            Assert.IsFalse(CardTestInput.IsShown(quiz.Counter, quiz.Root), "one question: no 'Question 1 of 1'");
            Assert.IsFalse(CardTestInput.IsShown(quiz.Nav, quiz.Root), "one question: no Previous / Next");
            foreach (var choice in quiz.Choices)
                Assert.IsTrue(UIAccessibility.MeetsMinTapTarget(choice.Button.worldBound.width, choice.Button.worldBound.height), "each choice is a tap target >= 44 px");

            // - a real tap on a WRONG option
            yield return ScrollAndTap(quiz.Choices[0].Button);
            Assert.AreEqual(0, quiz.Chosen);
            Assert.AreEqual("Actually...", quiz.Verdict.text, "a gentle correction, no 'Wrong!'");
            Assert.AreEqual(CardGalleryDefinitions.KeepExplanation, quiz.Explanation.text);
            Assert.IsTrue(CardTestInput.IsShown(quiz.Result, quiz.Root));
            Assert.IsTrue(quiz.Choices[0].Button.ClassListContains("card-quiz__choice--wrong"), "the tapped one is marked wrong");
            Assert.IsTrue(quiz.Choices[1].Button.ClassListContains("card-quiz__choice--correct"), "the right one is shown");
            Assert.IsFalse(quiz.Choices[2].Button.ClassListContains("card-quiz__choice--wrong") || quiz.Choices[2].Button.ClassListContains("card-quiz__choice--correct"));
            Assert.AreNotEqual(BorderOf(quiz.Choices[0].Button), BorderOf(quiz.Choices[1].Button), "wrong and right are told apart by more than a class name");
            AssertMarks(quiz, wrongChoice: 0, rightChoice: 1);
            AssertPlainPrompt(quiz);
            Assert.GreaterOrEqual(CardTestInput.Contrast(quiz.Explanation.resolvedStyle.color, CardTestInput.EffectiveBackground(quiz.Explanation)),
                UIAccessibility.MinRatioNormalText, "the explanation reads");
            Assert.AreEqual(0, _harness.State.Answer(entry, CardGalleryDefinitions.QuizBlockKey, 0), "remembered under the POI, the block and the question's row");
            Assert.IsTrue(_harness.StateStore.TryGet(StateKeyOf(entry), out string stored));
            Assert.AreEqual("0", stored);
            yield return Render("Card_knowledge_check_multiple_choice_wrong");

            // - no retry: a second tap on the right option changes nothing
            yield return ScrollAndTap(quiz.Choices[1].Button);
            Assert.AreEqual(0, quiz.Chosen, "the first answer stands");
            Assert.AreEqual("Actually...", quiz.Verdict.text);
            Assert.AreEqual(0, _harness.State.Answer(entry, CardGalleryDefinitions.QuizBlockKey, 0));

            // - close and open the card again: the answer is still there, with its verdict
            yield return ShowBlock(entry, v => quiz = (KnowledgeCheckBlockView)v);
            Assert.AreEqual(0, quiz.Chosen, "remembered across a rebind");
            Assert.IsTrue(CardTestInput.IsShown(quiz.Result, quiz.Root));
            Assert.AreEqual("Actually...", quiz.Verdict.text);
            Assert.IsTrue(quiz.Choices[1].Button.ClassListContains("card-quiz__choice--correct"));
        }

        [UnityTest]
        public IEnumerator KnowledgeCheck_ARightTapConfirms_SeveralQuestionsMoveWithPreviousNext_AndItReopensAtTheFirstUnanswered()
        {
            const string entry = "knowledge_check_multiple_choice_long";
            KnowledgeCheckBlockView quiz = null;
            yield return ShowBlock(entry, v => quiz = (KnowledgeCheckBlockView)v);
            Assert.AreEqual(3, quiz.Count);
            Assert.AreEqual("Question 1 of 3", quiz.Counter.text, "the counter from the card strings");
            Assert.AreEqual(Visibility.Hidden, quiz.Previous.resolvedStyle.visibility, "nothing before the first question");
            Assert.AreEqual("Next question", quiz.Next.Q<Label>().text);
            Assert.AreEqual(3, quiz.Choices.Count);

            // - the RIGHT option of the first question (the second one)
            yield return ScrollAndTap(quiz.Choices[1].Button);
            Assert.AreEqual(1, quiz.Chosen);
            Assert.AreEqual("Correct!", quiz.Verdict.text);
            StringAssert.StartsWith("The keep is the strongest tower of a castle.", quiz.Explanation.text);
            Assert.IsTrue(quiz.Choices[1].Button.ClassListContains("card-quiz__choice--correct"));
            Assert.IsFalse(quiz.Choices.Any(c => c.Button.ClassListContains("card-quiz__choice--wrong")), "nothing wrong to show");
            AssertMarks(quiz, wrongChoice: -1, rightChoice: 1);
            AssertPlainPrompt(quiz);
            yield return Render("Card_knowledge_check_multiple_choice_correct");

            // - Next: the second question (four options), answered wrong on a real tap; the first one keeps its answer
            yield return ScrollAndTap(quiz.Next);
            Assert.AreEqual(1, quiz.Index);
            Assert.AreEqual("Question 2 of 3", quiz.Counter.text);
            Assert.AreEqual(4, quiz.Choices.Count);
            Assert.IsFalse(quiz.Answered);
            yield return ScrollAndTap(quiz.Choices[0].Button);
            Assert.AreEqual("Actually...", quiz.Verdict.text);
            Assert.AreEqual(2, quiz.Choices.Count(c => c.Button.ClassListContains("card-quiz__choice--wrong") || c.Button.ClassListContains("card-quiz__choice--correct")),
                "one wrong, one right");
            yield return ScrollAndTap(quiz.Previous);
            Assert.AreEqual(0, quiz.Index);
            Assert.AreEqual(1, quiz.Chosen, "question 1 still shows its own answer");
            Assert.AreEqual("Correct!", quiz.Verdict.text);

            // - a rebind opens at the first question with no answer: the third
            yield return ShowBlock(entry, v => quiz = (KnowledgeCheckBlockView)v);
            Assert.AreEqual(2, quiz.Index, "questions 1 and 2 are answered: it opens on the third");
            Assert.AreEqual("Question 3 of 3", quiz.Counter.text);
            Assert.IsFalse(quiz.Answered);
            Assert.AreEqual(Visibility.Hidden, quiz.Next.resolvedStyle.visibility, "nothing after the last question");
            Assert.AreEqual(2, _harness.StateStore.Keys.Count(k => k.Contains(".answer-")), "two answers stored, one per question row");
        }

        [UnityTest]
        public IEnumerator KnowledgeCheck_TrueFalse_ARealSwipeRightIsTrue_LeftIsFalse_ShortOrVerticalDragsDoNothing_TheButtonsWorkToo()
        {
            const string entry = "knowledge_check_true_false_swipe_short";
            KnowledgeCheckBlockView quiz = null;
            yield return ShowBlock(entry, v => quiz = (KnowledgeCheckBlockView)v);
            Assert.AreEqual(CardGalleryDefinitions.CurtainStatement, quiz.Prompt.text);
            Assert.AreEqual("Swipe right for true, left for false", quiz.SwipeHint.text);
            Assert.IsTrue(CardTestInput.IsShown(quiz.SwipeHint, quiz.Root));
            CollectionAssert.AreEqual(new[] { "True", "False" }, quiz.Choices.Select(c => c.Text.text).ToList(), "the two fixed choices, from the card strings");
            Rect stage = quiz.Stage.worldBound;
            Vector2 centre = stage.center;

            // - a short drag and a vertical drag are no answer, and the card is back in its place
            yield return CardTestInput.DragFrom(quiz.Root.panel, centre, new Vector2(stage.width * 0.1f, 0f));
            Assert.IsFalse(quiz.Answered, "a short drag is not a swipe");
            Assert.AreEqual(0f, quiz.Stage.resolvedStyle.translate.x, 0.01f, "the card came back");
            yield return CardTestInput.DragFrom(quiz.Root.panel, centre, new Vector2(0f, 90f));
            Assert.IsFalse(quiz.Answered, "a vertical drag is not a swipe");
            Assert.IsFalse(quiz.IsSwiping, "the pointer was let go");

            // - a real swipe to the right: True, and the statement IS true
            yield return CardTestInput.DragFrom(quiz.Root.panel, centre, new Vector2(stage.width * 0.5f, 6f));
            yield return null;
            Assert.AreEqual(KnowledgeCheckRule.ChoiceTrue, quiz.Chosen, "a real swipe right answered True");
            Assert.AreEqual("Correct!", quiz.Verdict.text);
            AssertMarks(quiz, wrongChoice: -1, rightChoice: KnowledgeCheckRule.ChoiceTrue);
            Assert.AreEqual(CardGalleryDefinitions.CurtainExplanation, quiz.Explanation.text);
            Assert.AreEqual(0f, quiz.Stage.resolvedStyle.translate.x, 0.01f, "the card came back after the swipe");
            Assert.IsFalse(CardTestInput.IsShown(quiz.SwipeHint, quiz.Root), "no hint once answered");
            Assert.AreEqual(0, _harness.State.Answer(entry, CardGalleryDefinitions.QuizBlockKey, 0));
            yield return Render("Card_knowledge_check_true_false_swipe_correct");

            // - answered: another swipe does nothing
            yield return CardTestInput.DragFrom(quiz.Root.panel, centre, new Vector2(-stage.width * 0.5f, 0f));
            Assert.AreEqual(KnowledgeCheckRule.ChoiceTrue, quiz.Chosen, "no retry");

            // - the long look: the first statement is FALSE; a real swipe LEFT is False, and correct
            yield return ShowBlock("knowledge_check_true_false_swipe_long", v => quiz = (KnowledgeCheckBlockView)v);
            stage = quiz.Stage.worldBound;
            yield return CardTestInput.DragFrom(quiz.Root.panel, stage.center, new Vector2(-stage.width * 0.5f, -4f));
            yield return null;
            Assert.AreEqual(KnowledgeCheckRule.ChoiceFalse, quiz.Chosen, "a real swipe left answered False");
            Assert.AreEqual("Correct!", quiz.Verdict.text, "the statement is false: swiping left is right");

            // - the second statement is TRUE; the two buttons are the way without a gesture: a real tap on False is wrong
            yield return ScrollAndTap(quiz.Next);
            Assert.AreEqual("Question 2 of 3", quiz.Counter.text);
            yield return ScrollAndTap(quiz.Choices[KnowledgeCheckRule.ChoiceFalse].Button);
            Assert.AreEqual(KnowledgeCheckRule.ChoiceFalse, quiz.Chosen);
            Assert.AreEqual("Actually...", quiz.Verdict.text);
            Assert.IsTrue(quiz.Choices[KnowledgeCheckRule.ChoiceTrue].Button.ClassListContains("card-quiz__choice--correct"), "True is shown as the right one");
            AssertMarks(quiz, wrongChoice: KnowledgeCheckRule.ChoiceFalse, rightChoice: KnowledgeCheckRule.ChoiceTrue);
            // - the swipe look keeps its bordered statement card (what a swipe moves), and it wears the verdict's colour
            Assert.Greater(quiz.Stage.resolvedStyle.borderTopWidth, 0f, "the swipe card is a box");
            Assert.AreEqual(BorderOf(quiz.Choices[KnowledgeCheckRule.ChoiceFalse].Button), BorderOf(quiz.Stage), "the card wears the wrong verdict's colour");
            yield return Render("Card_knowledge_check_true_false_swipe_wrong");
        }

        [UnityTest]
        public IEnumerator KnowledgeCheck_ImageChoice_PicturesAreTheChoices_ARealTapAnswers_AndEveryPictureIsGivenBack()
        {
            const string entry = "knowledge_check_image_choice_short";
            KnowledgeCheckBlockView quiz = null;
            yield return ShowBlock(entry, v => quiz = (KnowledgeCheckBlockView)v);
            Assert.AreEqual(2, quiz.Choices.Count);
            Assert.AreEqual("before.png", quiz.Choices[0].Image.Path, "the first choice is the first picture");
            Assert.IsNotNull(quiz.Choices[0].Image.Texture, "and it loaded");
            Assert.AreEqual("after.png", quiz.Choices[1].Image.Path);
            CollectionAssert.AreEqual(new[] { "Before", "After" }, quiz.Choices.Select(c => c.Text.text).ToList(), "the captions");
            Assert.AreEqual(1, _harness.Media.RefCount("before.png"));
            Assert.Greater(quiz.Choices[0].Image.Root.worldBound.height, 40f, "a picture, not a text button");

            // - the RIGHT one is the first (the earlier picture): a real tap on the second is wrong
            yield return ScrollAndTap(quiz.Choices[1].Button);
            Assert.AreEqual(1, quiz.Chosen);
            Assert.AreEqual("Actually...", quiz.Verdict.text);
            Assert.AreEqual(CardGalleryDefinitions.PanelExplanation, quiz.Explanation.text);
            Assert.IsTrue(quiz.Choices[0].Button.ClassListContains("card-quiz__choice--correct"), "the right picture is framed");
            Assert.IsTrue(quiz.Choices[1].Button.ClassListContains("card-quiz__choice--wrong"));
            AssertMarks(quiz, wrongChoice: 1, rightChoice: 0);
            AssertPlainPrompt(quiz);
            yield return Render("Card_knowledge_check_image_choice_wrong");

            _harness.Sheet.Hide();
            yield return null;
            Assert.AreEqual(0, _harness.Media.HeldCount, "a closed card holds no picture");

            // - the long look: four pictures with captions; the right one is the third; a real tap on it is right
            yield return ShowBlock("knowledge_check_image_choice_long", v => quiz = (KnowledgeCheckBlockView)v);
            Assert.AreEqual(4, quiz.Choices.Count);
            yield return ScrollAndTap(quiz.Choices[2].Button);
            Assert.AreEqual("Correct!", quiz.Verdict.text);
            yield return Render("Card_knowledge_check_image_choice_correct");
        }

        [UnityTest]
        public IEnumerator KnowledgeCheck_RowsTheLookCannotShow_AreLeftOut_ForEveryLook()
        {
            foreach (string variant in BuiltInBlocks.KnowledgeCheck.Variants)
            {
                KnowledgeCheckBlockView quiz = null;
                yield return ShowBlock("knowledge_check_" + variant + "_partial", v => quiz = (KnowledgeCheckBlockView)v);
                Assert.AreEqual(1, quiz.Count, variant + ": only the complete question is shown");
                Assert.AreEqual("Complete question.", quiz.Prompt.text, variant);
                Assert.IsFalse(CardTestInput.IsShown(quiz.Nav, quiz.Root), variant + ": one question left, no Previous / Next");
            }
        }

        [UnityTest]
        public IEnumerator KnowledgeCheck_ShowAfterReading_StaysHiddenUntilARealWheelReachesTheEnd_ThenStays_AndACardThatFitsShowsItAtOnce()
        {
            var wall = CardGalleryDefinitions.Taxonomy();
            var poi = CardGalleryDefinitions.GatedKnowledgePoi(longText: true);
            _harness.ShowPoi(poi, wall, null, SheetStopRule.Stop.Half);
            yield return CardTestInput.Settle();
            var stack = _harness.Sheet.Stack;
            var quiz = stack.BoundViews.OfType<KnowledgeCheckBlockView>().Single();
            var slot = stack.SlotOf(quiz);
            Assert.AreEqual(DisplayStyle.None, slot.resolvedStyle.display, "hidden while the card is unread (its heading and gap with it)");
            Assert.IsFalse(stack.ContentSeen);
            Assert.IsFalse(_harness.State.Seen(poi.id, CardGalleryDefinitions.GatedQuizBlockKey));
            Assert.Greater(stack.Scroll.contentContainer.layout.height, stack.Scroll.contentViewport.layout.height, "precondition: the long card scrolls");

            // - a real wheel, notch by notch: hidden before every notch, and it appears exactly when the offset reaches the end the
            //   card had before the question was added to it
            float rangeBeforeTheLastNotch = 0f;
            for (int notch = 0; notch < 60 && !stack.ContentSeen; notch++)
            {
                Assert.AreEqual(DisplayStyle.None, slot.resolvedStyle.display, "unread: the question is still hidden (notch " + notch + ")");
                rangeBeforeTheLastNotch = stack.Scroll.contentContainer.layout.height - stack.Scroll.contentViewport.layout.height;
                yield return CardTestInput.Wheel(stack.Scroll, 2f);
            }
            Assert.IsTrue(stack.ContentSeen, "the wheel got to the end");
            Assert.GreaterOrEqual(stack.Scroll.scrollOffset.y, rangeBeforeTheLastNotch - ContentSeenRule.EndTolerance - 1f, "it appeared at the end of the unread card");
            Assert.AreNotEqual(DisplayStyle.None, slot.resolvedStyle.display, "the question appeared");
            Assert.IsTrue(_harness.State.Seen(poi.id, CardGalleryDefinitions.GatedQuizBlockKey), "the reveal is remembered");
            yield return CardTestInput.Wheel(stack.Scroll, -3f);
            Assert.AreNotEqual(DisplayStyle.None, slot.resolvedStyle.display, "once revealed it stays, whichever way the card is scrolled");

            // - opened again: shown at once (remembered), before any scrolling
            _harness.ShowPoi(CardGalleryDefinitions.GatedKnowledgePoi(longText: true), CardGalleryDefinitions.Taxonomy(), null, SheetStopRule.Stop.Half);
            yield return CardTestInput.Settle();
            stack = _harness.Sheet.Stack;
            quiz = stack.BoundViews.OfType<KnowledgeCheckBlockView>().Single();
            Assert.AreNotEqual(DisplayStyle.None, stack.SlotOf(quiz).resolvedStyle.display, "a visitor who read it once meets the question at once");
            Assert.IsFalse(stack.ContentSeen, "...although this time nothing has been scrolled");

            // - a card that all fits on the screen: nothing to scroll past, the question shows at once, at full
            _harness.State.ResetAll();
            var shortPoi = CardGalleryDefinitions.GatedKnowledgePoi(longText: false);
            _harness.ShowPoi(shortPoi, CardGalleryDefinitions.Taxonomy(), null, SheetStopRule.Stop.Full);
            yield return CardTestInput.Settle();
            stack = _harness.Sheet.Stack;
            quiz = stack.BoundViews.OfType<KnowledgeCheckBlockView>().Single();
            Assert.IsTrue(stack.ContentSeen, "everything fits: nothing left to read");
            Assert.AreNotEqual(DisplayStyle.None, stack.SlotOf(quiz).resolvedStyle.display);

            // - at the peek stop nothing is visible, so nothing counts as read
            _harness.State.ResetAll();
            _harness.ShowPoi(CardGalleryDefinitions.GatedKnowledgePoi(longText: false), CardGalleryDefinitions.Taxonomy(), null, SheetStopRule.Stop.Peek);
            yield return CardTestInput.Settle();
            stack = _harness.Sheet.Stack;
            Assert.IsFalse(stack.ContentSeen, "peek: the content is not on screen");
            Assert.AreEqual(DisplayStyle.None, stack.SlotOf(stack.BoundViews.OfType<KnowledgeCheckBlockView>().Single()).resolvedStyle.display);
        }

        // ---------------- Tier 3 group B (step 8B): poll, collect, dialogue, show_on_wall ----------------

        // An IPollResults with numbers (a stand-in for a backend, which does not exist): the seam must draw results from it and only from it
        private sealed class FakePollResults : IPollResults
        {
            public System.Collections.Generic.IReadOnlyList<int> Counts;
            public bool TryGet(string wallId, string poiId, string blockKey, out System.Collections.Generic.IReadOnlyList<int> votesPerRow)
            {
                votesPerRow = Counts;
                return Counts != null;
            }
        }

        private static string PollKeyOf(string entry) => "ts.card.gallery." + entry + "." + CardGalleryDefinitions.QuizBlockKey + ".poll";

        // The percent signs a block SHOWS (a label that is not displayed does not count)
        private static System.Collections.Generic.List<string> ShownPercents(VisualElement root) =>
            root.Query<Label>().ToList().Where(l => CardTestInput.IsShown(l, root) && l.text.Contains("%")).Select(l => l.text).ToList();

        [UnityTest]
        public IEnumerator Poll_ARealTapVotes_ShowsYourChoiceAndAThankYou_NeverAPercentage_OneEvent_Remembered_AndFinal()
        {
            var events = new RecordingEvents();
            _harness.Events = events;
            const string entry = "poll_bars_short";
            PollBlockView poll = null;
            yield return ShowBlock(entry, v => poll = (PollBlockView)v);
            Assert.AreEqual(CardGalleryDefinitions.PollQuestion, poll.Question.text);
            CollectionAssert.AreEqual(CardGalleryDefinitions.PollShortOptions, poll.Options.Select(o => o.Text.text).ToList());
            Assert.AreEqual(-1, poll.Voted, "a fresh visitor: no vote");
            Assert.IsFalse(CardTestInput.IsShown(poll.Thanks, poll.Root), "no thank-you before a vote");
            foreach (var option in poll.Options)
            {
                Assert.IsTrue(UIAccessibility.MeetsMinTapTarget(option.Button.worldBound.width, option.Button.worldBound.height), "an option is a tap target >= 44 px");
                Assert.IsFalse(CardTestInput.IsShown(option.Mark, option.Button), "no tick before a vote");
            }
            yield return Render("Card_poll_bars_open");

            // - a real tap on the second option
            yield return ScrollAndTap(poll.Options[1].Button);
            Assert.AreEqual(1, poll.Voted);
            Assert.IsTrue(CardTestInput.IsShown(poll.Options[1].Mark, poll.Options[1].Button) && poll.Options[1].Mark.worldBound.width > 4f, "the picked option carries a tick ELEMENT");
            Assert.AreEqual(CardIcons.Shape.Tick, poll.Options[1].Mark.Kind);
            Assert.IsTrue(CardTestInput.IsShown(poll.Options[1].Caption, poll.Options[1].Button));
            Assert.AreEqual("Your choice", poll.Options[1].Caption.text, "the caption, from the card strings");
            Assert.IsFalse(CardTestInput.IsShown(poll.Options[0].Caption, poll.Options[0].Button) || CardTestInput.IsShown(poll.Options[2].Mark, poll.Options[2].Button), "only the picked option is marked");
            Assert.IsTrue(CardTestInput.IsShown(poll.Thanks, poll.Root));
            Assert.AreEqual("Thank you for voting", poll.Thanks.text);
            // - there is no backend: not one percentage, not one results bar, however the block is looked at
            Assert.IsFalse(poll.ResultsShown, "no results without an IPollResults that has some");
            CollectionAssert.IsEmpty(ShownPercents(poll.Root), "no invented percentages");
            Assert.IsFalse(poll.Options.Any(o => CardTestInput.IsShown(o.Fill, o.Button)), "no results bar");
            Assert.GreaterOrEqual(CardTestInput.Contrast(poll.Options[1].Caption.resolvedStyle.color, CardTestInput.EffectiveBackground(poll.Options[1].Caption)),
                UIAccessibility.MinRatioNormalText, "Your choice reads");
            // - one event, in the events seam's words: which option, in which block, of which look
            Assert.AreEqual(1, events.Raised.Count);
            var raised = events.Raised[0];
            Assert.AreEqual(CardEventKinds.Poll, raised.Kind);
            Assert.AreEqual("gallery", raised.WallId);
            Assert.AreEqual(entry, raised.PoiId);
            Assert.AreEqual(CardGalleryDefinitions.QuizBlockKey, raised.BlockKey);
            Assert.AreEqual("bars", raised.Variant);
            Assert.AreEqual("2", raised.Value, "the option's row number counted from 1");
            Assert.AreEqual(1, _harness.State.PollVote(entry, CardGalleryDefinitions.QuizBlockKey), "remembered under the POI, the block and the authored row");
            Assert.IsTrue(_harness.StateStore.TryGet(PollKeyOf(entry), out string stored));
            Assert.AreEqual("1", stored);
            yield return Render("Card_poll_bars_voted");

            // - final: a tap on another option changes nothing and raises nothing
            yield return ScrollAndTap(poll.Options[0].Button);
            Assert.AreEqual(1, poll.Voted);
            Assert.AreEqual(1, events.Raised.Count, "no second event");

            // - the card opened again: the vote shown as given, nothing reported
            yield return ShowBlock(entry, v => poll = (PollBlockView)v);
            Assert.AreEqual(1, poll.Voted, "remembered across a rebind");
            Assert.IsTrue(CardTestInput.IsShown(poll.Options[1].Mark, poll.Options[1].Button));
            Assert.IsTrue(CardTestInput.IsShown(poll.Thanks, poll.Root));
            Assert.AreEqual(1, events.Raised.Count, "showing a remembered vote reports nothing");

            // - blank rows are left out, and the vote is kept under the AUTHORED row: the partial poll shows rows 0 and 2
            yield return ShowBlock("poll_bars_partial", v => poll = (PollBlockView)v);
            CollectionAssert.AreEqual(new[] { "The arcade", "The river gate" }, poll.Options.Select(o => o.Text.text).ToList());
            yield return ScrollAndTap(poll.Options[1].Button);
            Assert.AreEqual(2, _harness.State.PollVote("poll_bars_partial", CardGalleryDefinitions.QuizBlockKey), "the second SHOWN option is authored row 2");
            Assert.AreEqual("3", events.Raised[1].Value);
            // - a vote stored for a row that is not shown is no vote
            _harness.State.SetPollVote("poll_bars_short", CardGalleryDefinitions.QuizBlockKey, 5);
            yield return ShowBlock("poll_bars_short", v => poll = (PollBlockView)v);
            Assert.AreEqual(-1, poll.Voted, "there is no option 6 in a poll of three");
        }

        [UnityTest]
        public IEnumerator Poll_TheResultsSeam_DrawsBarsAndSharesOnlyFromWhatAnIPollResultsGives_AfterTheVote()
        {
            var results = new FakePollResults { Counts = new[] { 1, 3, 0 } };
            var withResults = new CardServices();
            withResults.Add<IPollResults>(results);
            _harness.Services = withResults;
            PollBlockView poll = null;
            yield return ShowBlock("poll_bars_short", v => poll = (PollBlockView)v);
            Assert.IsFalse(poll.ResultsShown, "results wait for the visitor's own vote");
            yield return ScrollAndTap(poll.Options[0].Button);
            Assert.IsTrue(poll.ResultsShown, "the seam had numbers: the bars show");
            CollectionAssert.AreEqual(new[] { "25%", "75%", "0%" }, poll.Options.Select(o => o.Percent.text).ToList(),
                "1, 3 and 0 votes of 4 are 25, 75 and 0 percent (counts are per authored row)");
            float first = poll.Options[0].Fill.resolvedStyle.width;
            float second = poll.Options[1].Fill.resolvedStyle.width;
            Assert.Greater(first, 4f, "a bar with a real width");
            Assert.AreEqual(3f, second / first, 0.05f, "the bars are drawn in proportion: 75 to 25");
            Assert.AreEqual(0f, poll.Options[2].Fill.resolvedStyle.width, 0.5f, "no votes: no bar");
            yield return Render("Card_poll_bars_with_results");

            // - the seam answering nothing again (no IPollResults registered, today): the bars are hidden and the block shows no percentage
            _harness.Services = new CardServices();
            yield return ShowBlock("poll_bars_short", v => poll = (PollBlockView)v);
            Assert.IsFalse(poll.ResultsShown);
            CollectionAssert.IsEmpty(ShownPercents(poll.Root), "no data: no percentage");
        }

        [UnityTest]
        public IEnumerator Collect_ARealTapAddsTheItem_TheCountIsReadFromTheWallsConfig_ItPersists_AndReportsOneEvent()
        {
            var events = new RecordingEvents();
            _harness.Events = events;
            const string entry = "collect_add_to_story_short";
            CollectBlockView collect = null;
            // - another point of the wall already has its item in the visitor's story: the count reads across points
            _harness.State.SetCollected("collect_1", "block_9");
            yield return ShowBlock(entry, v => collect = (CollectBlockView)v);
            Assert.AreEqual(CardGalleryDefinitions.CollectSeries, collect.Series.text);
            Assert.AreEqual(CardGalleryDefinitions.CollectItem, collect.ItemName.text);
            Assert.AreEqual(4, collect.Total, "the shown point + the three more collectables the fabricated wall holds: counted from the config");
            Assert.AreEqual(1, collect.Have);
            Assert.AreEqual("1 of 4 collected", collect.Progress.text, "the card strings' words");
            Assert.IsFalse(collect.IsCollected);
            Assert.AreEqual("Add to my story", collect.AddLabel.text);
            Assert.IsFalse(collect.StampStar.Filled, "the stamp's star is an outline until collected");
            Assert.IsTrue(UIAccessibility.MeetsMinTapTarget(collect.Add.worldBound.width, collect.Add.worldBound.height), "the button is a tap target >= 44 px");
            Assert.GreaterOrEqual(CardTestInput.Contrast(collect.AddLabel.resolvedStyle.color, collect.Add.resolvedStyle.backgroundColor), UIAccessibility.MinRatioNormalText, "the button reads");
            yield return Render("Card_collect_open");

            yield return ScrollAndTap(collect.Add);
            Assert.IsTrue(collect.IsCollected);
            Assert.AreEqual("In your story", collect.AddLabel.text, "the button says where it went");
            Assert.IsTrue(CardTestInput.IsShown(collect.AddMark, collect.Add) && collect.AddMark.worldBound.width > 4f, "a tick element, not only a colour");
            Assert.IsTrue(collect.StampStar.Filled, "the stamp's star is filled");
            Assert.AreEqual("2 of 4 collected", collect.Progress.text);
            Assert.AreEqual(0.5f, collect.ProgressFill.resolvedStyle.width / collect.ProgressTrack.resolvedStyle.width, 0.02f, "the bar is half full");
            Assert.AreEqual(1, events.Raised.Count);
            var raised = events.Raised[0];
            Assert.AreEqual(CardEventKinds.Collect, raised.Kind);
            Assert.AreEqual(entry, raised.PoiId);
            Assert.AreEqual(CardGalleryDefinitions.QuizBlockKey, raised.BlockKey);
            Assert.AreEqual("collected", raised.Value);
            Assert.IsTrue(_harness.State.Collected(entry, CardGalleryDefinitions.QuizBlockKey), "kept in CardLocalState under the POI and the block");
            Assert.IsTrue(_harness.StateStore.TryGet("ts.card.gallery." + entry + "." + CardGalleryDefinitions.QuizBlockKey + ".collected", out string stored));
            Assert.AreEqual("1", stored);
            yield return Render("Card_collect_collected");

            // - once: a second tap changes nothing and reports nothing
            yield return ScrollAndTap(collect.Add);
            Assert.AreEqual(1, events.Raised.Count, "no second event");
            Assert.AreEqual("2 of 4 collected", collect.Progress.text);

            // - the card opened again: still collected, nothing reported
            yield return ShowBlock(entry, v => collect = (CollectBlockView)v);
            Assert.IsTrue(collect.IsCollected, "remembered across a rebind");
            Assert.AreEqual("2 of 4 collected", collect.Progress.text);
            Assert.AreEqual(1, events.Raised.Count);

            // - the wall's total is the config's: the same block on a wall where it is the only collectable says 1
            yield return ShowBlock("collect_add_to_story_alone", v => collect = (CollectBlockView)v);
            Assert.AreEqual(1, collect.Total, "a total that is not a constant");
            Assert.AreEqual("0 of 1 collected", collect.Progress.text);
            // - a wall that switches Collect off in its Block Library counts none of its items
            var wall = CardGalleryDefinitions.Taxonomy();
            wall.card_settings.kinds.Add(new BlockKindSetting { kind = BuiltInBlocks.CollectKind, enabled = false });
            Assert.AreEqual(0, CollectRule.Items(new[] { new POIData { id = "a", card = new POICardData { blocks = { new BlockInstanceData { key = "b", kind = BuiltInBlocks.CollectKind } } } } }, wall.card_settings).Count);

            // - nothing written: the point's card title and its category
            yield return ShowBlock("collect_add_to_story_defaults", v => collect = (CollectBlockView)v);
            Assert.AreEqual("Gate", collect.ItemName.text, "the point's card title");
            Assert.AreEqual("Civic Buildings", collect.Series.text, "the point's category name");
        }
    }
}
