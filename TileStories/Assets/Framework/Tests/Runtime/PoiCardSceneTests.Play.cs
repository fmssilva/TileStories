using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // Phase B of the play family on the real LivingRoomScene: knowledge_check (three looks), poll, collect.
    public partial class PoiCardSceneTests
    {
        private System.Collections.Generic.List<KnowledgeCheckBlockView> Quizzes() => Sheet.Stack.BoundViews.OfType<KnowledgeCheckBlockView>().ToList();

        // _3.1 step 8A: The Lamp's multiple-choice question on the real card: a real tap on a wrong option says "Actually..." with
        // the explanation and marks the right one; the answer is remembered under wall + POI + block + row, so the card reopens
        // on the next question; in Portuguese the card's words and the authored Portuguese text
        [UnityTest]
        public IEnumerator TheLamp_KnowledgeCheck_MultipleChoice_ARealTapAnswers_ItIsRemembered_AndInPortugueseTheWordsFollow()
        {
            yield return OpenFull("lamp");
            var quiz = Quizzes()[0];
            Assert.AreEqual("Test yourself", Sheet.Stack.HeadingOf(quiz).text, "no heading written: the card's default");
            Assert.AreEqual(2, quiz.Count, "two questions authored");
            Assert.AreEqual("What is the tallest tower of a castle called?", quiz.Prompt.text);
            CollectionAssert.AreEqual(new[] { "The curtain wall", "The keep", "The gatehouse" }, quiz.Choices.Select(c => c.Text.text).ToList());
            Assert.AreEqual("Question 1 of 2", quiz.Counter.text);
            Assert.IsFalse(quiz.Answered);

            yield return ScrollAndTap(quiz.Choices[0].Button);
            Assert.AreEqual(0, quiz.Chosen);
            Assert.AreEqual("Actually...", quiz.Verdict.text);
            Assert.AreEqual("The keep is the strongest tower: the last refuge when the walls fell.", quiz.Explanation.text);
            Assert.IsTrue(quiz.Choices[1].Button.ClassListContains("card-quiz__choice--correct"), "the right option is marked");
            Assert.AreEqual(0, Card.State.Answer("lamp", "block_43", 0), "remembered under the wall, the POI, the block and the question's row");
            Assert.IsTrue(_cardStore.TryGet("ts.card." + Session.SearchConfig.wall_id + ".lamp.block_43.answer-0", out string stored), "the scoped key");
            Assert.AreEqual("0", stored);
            yield return ScrollTo(quiz);
            yield return Capture("Card_Lamp_KnowledgeCheck_MultipleChoice");

            // - closed and opened again: the first question is answered, so it opens on the second
            SelectionEventBus.Clear();
            yield return OpenFull("lamp");
            quiz = Quizzes()[0];
            Assert.AreEqual(1, quiz.Index, "question 1 is answered: it opens on question 2");
            Assert.AreEqual("Question 2 of 2", quiz.Counter.text);
            Assert.IsFalse(quiz.Answered);

            // - Portuguese: the authored Portuguese text and the card's Portuguese words; a real tap on the right option
            SelectionEventBus.Clear();
            LiveSettings.languages = new System.Collections.Generic.List<string> { "pt", "en" };
            yield return OpenFull("lamp");
            quiz = Quizzes()[0];
            Assert.AreEqual("Teste-se", Sheet.Stack.HeadingOf(quiz).text, "the default heading in Portuguese");
            Assert.AreEqual("Pergunta 2 de 2", quiz.Counter.text);
            Assert.AreEqual("Qual destes N\u00c3O existia no antigo pal\u00e1cio?", quiz.Prompt.text);
            yield return ScrollAndTap(quiz.Choices[2].Button);
            Assert.AreEqual("Certo!", quiz.Verdict.text);
            StringAssert.StartsWith("O pal\u00e1cio ficava junto ao rio", quiz.Explanation.text);
            Assert.AreEqual(2, Card.State.Answer("lamp", "block_43", 1));
        }

        // _3.1 step 8A: The Lamp's true / false question waits for the reading (show_after_viewed): hidden on a fresh card, a real
        // wheel to the end reveals it (and it is remembered), a real swipe to the right answers True on the real card
        [UnityTest]
        public IEnumerator TheLamp_KnowledgeCheck_TrueFalse_WaitsForTheReading_ARealWheelRevealsIt_ARealSwipeAnswers()
        {
            yield return OpenFull("lamp");
            var quiz = Quizzes()[1];
            var slot = Sheet.Stack.SlotOf(quiz);
            Assert.AreEqual(DisplayStyle.None, slot.resolvedStyle.display, "the card is unread: the question waits");
            Assert.IsFalse(Sheet.Stack.ContentSeen);
            Assert.IsFalse(Card.State.Seen("lamp", "block_44"));
            var others = Quizzes().Where(q => q != quiz).ToList();
            foreach (var other in others) Assert.AreNotEqual(DisplayStyle.None, Sheet.Stack.SlotOf(other).resolvedStyle.display, "only the block that asks to wait waits");

            yield return ReadTheWholeCard();
            Assert.AreNotEqual(DisplayStyle.None, slot.resolvedStyle.display, "the wheel got to the end: the question appeared");
            Assert.IsTrue(Card.State.Seen("lamp", "block_44"), "and that is remembered");

            yield return ScrollTo(quiz);
            Assert.AreEqual("A curtain wall joins the towers of a castle.", quiz.Prompt.text);
            Rect stage = quiz.Stage.worldBound;
            yield return CardTestInput.DragFrom(quiz.Root.panel, stage.center, new Vector2(stage.width * 0.5f, 4f));
            yield return null;
            Assert.AreEqual(KnowledgeCheckRule.ChoiceTrue, quiz.Chosen, "a real swipe to the right answered True");
            Assert.AreEqual("Correct!", quiz.Verdict.text);
            Assert.AreEqual(0, Card.State.Answer("lamp", "block_44", 0));
            yield return Capture("Card_Lamp_KnowledgeCheck_TrueFalse");

            // - a visitor who read it once meets the question at once the next time
            SelectionEventBus.Clear();
            yield return OpenFull("lamp");
            Assert.AreNotEqual(DisplayStyle.None, Sheet.Stack.SlotOf(Quizzes()[1]).resolvedStyle.display, "remembered: no waiting");
            Assert.IsFalse(Sheet.Stack.ContentSeen, "...although this time nothing has been read");
        }

        // _3.1 step 8A: The Lamp's picture question: the pictures come from the wall's media folder, a real tap on the right one
        [UnityTest]
        public IEnumerator TheLamp_KnowledgeCheck_ImageChoice_PicturesFromTheWallsFolder_ARealTapOnTheRightOneConfirms()
        {
            yield return OpenFull("lamp");
            var quiz = Quizzes()[2];
            Assert.AreEqual(2, quiz.Choices.Count);
            Assert.AreEqual("damage_before", quiz.Choices[0].Image.Texture?.name, "the wall's own picture");
            Assert.AreEqual("damage_after", quiz.Choices[1].Image.Texture?.name);
            CollectionAssert.AreEqual(new[] { "Before", "After" }, quiz.Choices.Select(c => c.Text.text).ToList());
            yield return ScrollAndTap(quiz.Choices[0].Button);
            Assert.AreEqual("Correct!", quiz.Verdict.text);
            Assert.AreEqual("The earlier picture still has every tile in place.", quiz.Explanation.text);
            Assert.AreEqual(0, Card.State.Answer("lamp", "block_45", 0));
            yield return ScrollTo(quiz);
            yield return Capture("Card_Lamp_KnowledgeCheck_ImageChoice");
        }

        // _3.1 step 8B: The Lamp's poll on the real card: a real tap votes, the card shows the visitor's own choice and a thank-you and
        // NEVER a percentage (there is no backend), one event goes through the events seam, the vote is kept under wall + POI + block,
        // and a Portuguese card asks and thanks in Portuguese
        [UnityTest]
        public IEnumerator TheLamp_Poll_ARealTapVotes_NoPercentagesWithoutABackend_OneEvent_Kept_AndInPortugueseTheWordsFollow()
        {
            yield return OpenFull("lamp");
            var poll = Only<PollBlockView>();
            Assert.AreEqual("Which part of the castle would you visit first?", poll.Question.text);
            CollectionAssert.AreEqual(new[] { "The keep", "The curtain wall", "The gatehouse" }, poll.Options.Select(o => o.Text.text).ToList());
            Assert.IsFalse(poll.ResultsShown);
            yield return ScrollAndTap(poll.Options[1].Button);
            Assert.AreEqual(1, poll.Voted);
            Assert.AreEqual("Your choice", poll.Options[1].Caption.text);
            Assert.IsTrue(CardTestInput.IsShown(poll.Options[1].Mark, poll.Options[1].Button) && poll.Options[1].Mark.worldBound.width > 4f, "the tick element on the picked option");
            Assert.AreEqual("Thank you for voting", poll.Thanks.text);
            Assert.IsFalse(poll.ResultsShown, "the card's own poll results seam has nothing (no backend)");
            Assert.IsNull(Card.Services.Get<IPollResults>(), "no poll backend is registered today");
            Assert.IsFalse(poll.Root.Query<Label>().ToList().Any(l => CardTestInput.IsShown(l, poll.Root) && l.text.Contains("%")), "no percentage anywhere in the block");
            Assert.AreEqual(1, _cardEvents.Raised.Count(e => e.Kind == CardEventKinds.Poll), "one poll event");
            var raised = _cardEvents.Raised.Single(e => e.Kind == CardEventKinds.Poll);
            Assert.AreEqual(Session.SearchConfig.wall_id, raised.WallId);
            Assert.AreEqual("lamp", raised.PoiId);
            Assert.AreEqual("block_48", raised.BlockKey);
            Assert.AreEqual("bars", raised.Variant);
            Assert.AreEqual("2", raised.Value);
            Assert.AreEqual(1, Card.State.PollVote("lamp", "block_48"));
            Assert.IsTrue(_cardStore.TryGet(StoredKey("lamp", "block_48", "poll"), out string stored), "the scoped key");
            Assert.AreEqual("1", stored);
            yield return ScrollTo(poll);
            yield return Capture("Card_Lamp_Poll");

            // - closed and opened again: the vote shown as given, nothing reported again
            SelectionEventBus.Clear();
            yield return OpenFull("lamp");
            poll = Only<PollBlockView>();
            Assert.AreEqual(1, poll.Voted);
            Assert.AreEqual(1, _cardEvents.Raised.Count(e => e.Kind == CardEventKinds.Poll));

            // - Portuguese, on a fresh device
            _cardStore.Keys.ToList().ForEach(k => _cardStore.Remove(k));
            SelectionEventBus.Clear();
            LiveSettings.languages = new System.Collections.Generic.List<string> { "pt", "en" };
            yield return OpenFull("lamp");
            poll = Only<PollBlockView>();
            Assert.AreEqual("Que parte do castelo visitaria primeiro?", poll.Question.text);
            yield return ScrollAndTap(poll.Options[2].Button);
            Assert.AreEqual("A sua escolha", poll.Options[2].Caption.text);
            Assert.AreEqual("Obrigado pelo seu voto", poll.Thanks.text);
            Assert.AreEqual("A porta principal", poll.Options[2].Text.text);
        }

        // _3.1 step 8B: The Lamp's collect block: a real tap adds its item and it persists; the count is worked out from the wall's
        // CONFIG -- give another point of the wall a collect block and the total grows -- and its words are Portuguese in a
        // Portuguese card
        [UnityTest]
        public IEnumerator TheLamp_Collect_ARealTapAddsTheItem_ItPersists_TheCountComesFromTheWallsConfig_AndInPortugueseTheWordsFollow()
        {
            yield return OpenFull("lamp");
            var collect = Only<CollectBlockView>();
            Assert.AreEqual("Your story", Sheet.Stack.HeadingOf(collect).text, "no heading written: the card's default");
            Assert.AreEqual("Castles and towers", collect.Series.text);
            Assert.AreEqual("Stamp of the castle", collect.ItemName.text);
            Assert.AreEqual(1, collect.Total, "the wall's config has ONE collect block");
            Assert.AreEqual("0 of 1 collected", collect.Progress.text);

            // - another point of the running wall gets a collect block (in memory: the wall's own points have none): the total is 2
            var other = Session.SearchPois.First(p => p.id == "lamp_military");
            other.card.blocks.Add(new BlockInstanceData { key = "block_99", kind = BuiltInBlocks.CollectKind, variant = BuiltInBlocks.CollectAddToStory });
            try
            {
                SelectionEventBus.Clear();
                yield return OpenFull("lamp");
                collect = Only<CollectBlockView>();
                Assert.AreEqual(2, collect.Total, "counted from the config, not a constant");
                Assert.AreEqual("0 of 2 collected", collect.Progress.text);
                yield return ScrollAndTap(collect.Add);
                Assert.IsTrue(collect.IsCollected);
                Assert.AreEqual("In your story", collect.AddLabel.text);
                Assert.AreEqual("1 of 2 collected", collect.Progress.text);
                Assert.IsTrue(collect.StampStar.Filled);
                Assert.AreEqual(1, _cardEvents.Raised.Count(e => e.Kind == CardEventKinds.Collect));
                Assert.IsTrue(Card.State.Collected("lamp", "block_49"));
                Assert.IsTrue(_cardStore.TryGet(StoredKey("lamp", "block_49", "collected"), out _), "the scoped key");
                yield return ScrollTo(collect);
                yield return Capture("Card_Lamp_Collect");

                // - closed and opened again: still in the story
                SelectionEventBus.Clear();
                yield return OpenFull("lamp");
                collect = Only<CollectBlockView>();
                Assert.IsTrue(collect.IsCollected);
                Assert.AreEqual("1 of 2 collected", collect.Progress.text);

                // - the other point's own card counts the same wall: 1 of 2, its own item not yet collected
                SelectionEventBus.Clear();
                yield return OpenFull("lamp_military");
                var military = Only<CollectBlockView>();
                Assert.AreEqual("1 of 2 collected", military.Progress.text, "the count is the wall's, whichever card shows it");
                Assert.IsFalse(military.IsCollected);

                // - Portuguese
                _cardStore.Keys.ToList().ForEach(k => _cardStore.Remove(k));
                SelectionEventBus.Clear();
                LiveSettings.languages = new System.Collections.Generic.List<string> { "pt", "en" };
                yield return OpenFull("lamp");
                collect = Only<CollectBlockView>();
                Assert.AreEqual("A sua hist\u00f3ria", Sheet.Stack.HeadingOf(collect).text);
                Assert.AreEqual("Selo do castelo", collect.ItemName.text);
                Assert.AreEqual("Castelos e torres", collect.Series.text);
                Assert.AreEqual("Adicionar \u00e0 minha hist\u00f3ria", collect.AddLabel.text);
                Assert.AreEqual("0 de 2 recolhidos", collect.Progress.text);
            }
            finally
            {
                other.card.blocks.RemoveAll(b => b.key == "block_99");
            }
        }
    }
}
