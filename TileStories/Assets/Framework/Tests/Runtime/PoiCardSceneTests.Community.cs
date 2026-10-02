using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // Phase B of the community family on the real LivingRoomScene: feedback.
    public partial class PoiCardSceneTests
    {
        // _3.1 step 8A: The Lamp's feedback blocks: a real tap on a thumb and on a star each raises ONE event through the card's
        // events seam (wall, POI, block, look, value) and is remembered; a Portuguese card asks and thanks in Portuguese
        [UnityTest]
        public IEnumerator TheLamp_Feedback_ARealTapOnAThumbAndOnAStar_EachRaisesOneEvent_IsRemembered_AndInPortugueseTheWordsFollow()
        {
            yield return OpenFull("lamp");
            var feedback = Sheet.Stack.BoundViews.OfType<FeedbackBlockView>().ToList();
            Assert.AreEqual(2, feedback.Count, "thumbs and stars");
            var thumbs = feedback[0];
            var stars = feedback[1];
            Assert.AreEqual("Was this description useful?", thumbs.Question.text, "the question written for it");
            Assert.AreEqual("How would you rate this?", stars.Question.text, "none written: the card's own");

            yield return ScrollAndTap(thumbs.Votes[0].Button);
            Assert.AreEqual(FeedbackRule.ThumbUp, thumbs.Voted);
            Assert.AreEqual(1, _cardEvents.Raised.Count, "one event for the thumb");
            var first = _cardEvents.Raised[0];
            Assert.AreEqual(CardEventKinds.Feedback, first.Kind);
            Assert.AreEqual(Session.SearchConfig.wall_id, first.WallId);
            Assert.AreEqual("lamp", first.PoiId);
            Assert.AreEqual("block_46", first.BlockKey);
            Assert.AreEqual("thumbs", first.Variant);
            Assert.AreEqual("up", first.Value);
            Assert.AreEqual(FeedbackRule.ThumbUp, Card.State.Vote("lamp", "block_46"));

            yield return ScrollAndTap(stars.Votes[4].Button);
            Assert.AreEqual(5, stars.Voted);
            Assert.AreEqual(2, _cardEvents.Raised.Count);
            Assert.AreEqual("5", _cardEvents.Raised[1].Value);
            Assert.AreEqual("block_47", _cardEvents.Raised[1].BlockKey);
            Assert.IsTrue(stars.Votes.All(v => v.Button.ClassListContains("card-feedback__vote--on")), "five stars filled");
            yield return ScrollTo(stars);
            yield return Capture("Card_Lamp_Feedback");

            // - closed and opened again: both votes shown as given, nothing reported again
            SelectionEventBus.Clear();
            yield return OpenFull("lamp");
            feedback = Sheet.Stack.BoundViews.OfType<FeedbackBlockView>().ToList();
            Assert.AreEqual(FeedbackRule.ThumbUp, feedback[0].Voted);
            Assert.AreEqual(5, feedback[1].Voted);
            Assert.AreEqual(2, _cardEvents.Raised.Count, "a remembered vote reports nothing");

            // - Portuguese, on a fresh device: the authored question, the card's default question and thank-you
            _cardStore.Keys.ToList().ForEach(k => _cardStore.Remove(k));
            SelectionEventBus.Clear();
            LiveSettings.languages = new System.Collections.Generic.List<string> { "pt", "en" };
            yield return OpenFull("lamp");
            feedback = Sheet.Stack.BoundViews.OfType<FeedbackBlockView>().ToList();
            Assert.AreEqual("Esta descri\u00e7\u00e3o foi \u00fatil?", feedback[0].Question.text);
            Assert.AreEqual("Como avalia isto?", feedback[1].Question.text);
            CollectionAssert.AreEqual(new[] { "\u00datil", "Pouco \u00fatil" }, feedback[0].Votes.Select(v => v.Text.text).ToList());
            yield return ScrollAndTap(feedback[0].Votes[1].Button);
            Assert.AreEqual("Obrigado pelo seu coment\u00e1rio", feedback[0].Thanks.text);
            Assert.AreEqual("down", _cardEvents.Raised[2].Value);
        }
    }
}
