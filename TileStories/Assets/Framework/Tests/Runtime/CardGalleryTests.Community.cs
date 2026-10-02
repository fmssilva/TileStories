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
    // Phase A promises of the community family: feedback.
    public partial class CardGalleryTests
    {
        [UnityTest]
        public IEnumerator Feedback_Thumbs_ARealTapVotesOnce_RaisesOneEvent_IsRemembered_AndShowsThanks()
        {
            var events = new RecordingEvents();
            _harness.Events = events;
            const string entry = "feedback_thumbs_short";
            FeedbackBlockView feedback = null;
            yield return ShowBlock(entry, v => feedback = (FeedbackBlockView)v);
            Assert.AreEqual(CardGalleryDefinitions.FeedbackQuestion, feedback.Question.text);
            CollectionAssert.AreEqual(new[] { "Helpful", "Not helpful" }, feedback.Votes.Select(v => v.Text.text).ToList(), "the words under each thumb, from the card strings");
            Assert.IsFalse(CardTestInput.IsShown(feedback.Thanks, feedback.Root), "no thank-you before a vote");
            Assert.AreEqual(-1, feedback.Voted);
            foreach (var vote in feedback.Votes)
                Assert.IsTrue(UIAccessibility.MeetsMinTapTarget(vote.Button.worldBound.width, vote.Button.worldBound.height), "a thumb is a tap target >= 44 px");
            Assert.Greater(feedback.Votes[0].Glyph.worldBound.height, 4f, "the thumb has a drawn glyph");
            yield return Render("Card_feedback_thumbs_open");

            yield return ScrollAndTap(feedback.Votes[0].Button);
            Assert.AreEqual(FeedbackRule.ThumbUp, feedback.Voted);
            Assert.AreEqual(1, events.Raised.Count, "exactly one event");
            var raised = events.Raised[0];
            Assert.AreEqual(CardEventKinds.Feedback, raised.Kind);
            Assert.AreEqual("gallery", raised.WallId);
            Assert.AreEqual(entry, raised.PoiId);
            Assert.AreEqual(CardGalleryDefinitions.QuizBlockKey, raised.BlockKey);
            Assert.AreEqual("thumbs", raised.Variant);
            Assert.AreEqual("up", raised.Value);
            Assert.AreEqual(FeedbackRule.ThumbUp, _harness.State.Vote(entry, CardGalleryDefinitions.QuizBlockKey), "remembered");
            Assert.IsTrue(feedback.Votes[0].Button.ClassListContains("card-feedback__vote--on"));
            Assert.IsFalse(feedback.Votes[1].Button.ClassListContains("card-feedback__vote--on"));
            Assert.IsTrue(feedback.Votes[0].Glyph.Filled, "the thumb given is filled");
            Assert.IsFalse(feedback.Votes[1].Glyph.Filled, "the other stays an outline");
            Assert.IsTrue(CardTestInput.IsShown(feedback.Thanks, feedback.Root));
            Assert.AreEqual("Thank you for your feedback", feedback.Thanks.text);
            yield return Render("Card_feedback_thumbs_voted");

            // - the vote is final: a tap on the other thumb changes nothing and raises nothing
            yield return ScrollAndTap(feedback.Votes[1].Button);
            Assert.AreEqual(FeedbackRule.ThumbUp, feedback.Voted);
            Assert.AreEqual(1, events.Raised.Count, "no second event");

            // - closed and opened again: shown as given, and no new event
            yield return ShowBlock(entry, v => feedback = (FeedbackBlockView)v);
            Assert.AreEqual(FeedbackRule.ThumbUp, feedback.Voted, "remembered across a rebind");
            Assert.IsTrue(feedback.Votes[0].Button.ClassListContains("card-feedback__vote--on"));
            Assert.IsTrue(CardTestInput.IsShown(feedback.Thanks, feedback.Root));
            Assert.AreEqual(1, events.Raised.Count, "showing a remembered vote reports nothing");

            // - the thumb down, on another block, reports "down"
            yield return ShowBlock("feedback_thumbs_long", v => feedback = (FeedbackBlockView)v);
            yield return ScrollAndTap(feedback.Votes[1].Button);
            Assert.AreEqual(2, events.Raised.Count);
            Assert.AreEqual("down", events.Raised[1].Value);
            Assert.AreEqual(FeedbackRule.ThumbDown, _harness.State.Vote("feedback_thumbs_long", CardGalleryDefinitions.QuizBlockKey));
        }

        [UnityTest]
        public IEnumerator Feedback_Stars_ARealTapOnTheFourthStarFillsFourAndReportsFour_AndTheQuestionDefaultsToTheCardsOwn()
        {
            var events = new RecordingEvents();
            _harness.Events = events;
            const string entry = "feedback_stars_short";
            FeedbackBlockView feedback = null;
            yield return ShowBlock(entry, v => feedback = (FeedbackBlockView)v);
            Assert.AreEqual(FeedbackRule.StarCount, feedback.Votes.Count);
            Assert.IsFalse(feedback.Votes.Any(v => CardTestInput.IsShown(v.Text, v.Button)), "a star carries no words");
            foreach (var vote in feedback.Votes)
                Assert.IsTrue(UIAccessibility.MeetsMinTapTarget(vote.Button.worldBound.width, vote.Button.worldBound.height), "a star is a tap target >= 44 px");
            Assert.AreEqual("4 of 5 stars", feedback.Votes[3].Button.tooltip);

            yield return ScrollAndTap(feedback.Votes[3].Button);
            Assert.AreEqual(4, feedback.Voted);
            CollectionAssert.AreEqual(new[] { true, true, true, true, false }, feedback.Votes.Select(v => v.Button.ClassListContains("card-feedback__vote--on")).ToList(),
                "the stars up to the vote are filled");
            CollectionAssert.AreEqual(new[] { true, true, true, true, false }, feedback.Votes.Select(v => v.Glyph.Filled).ToList(),
                "filled is a property of the drawn star, not only of the button's class");
            // - a real render: the inside of a filled star is the star's colour, the inside of an outlined one is the card behind it
            var on = feedback.Votes[0].Glyph;
            var off = feedback.Votes[4].Glyph;
            Color onColour = on.resolvedStyle.color;
            yield return PixelAt(on, on.worldBound.center, c => AssertColour(onColour, c, "a filled star is solid"));
            yield return PixelAt(off, off.worldBound.center, c => Assert.Less(c.grayscale, onColour.grayscale - 0.05f, "an outlined star's inside is hollow"));
            Assert.AreEqual(1, events.Raised.Count);
            Assert.AreEqual("4", events.Raised[0].Value);
            Assert.AreEqual("stars", events.Raised[0].Variant);
            Assert.AreEqual(4, _harness.State.Vote(entry, CardGalleryDefinitions.QuizBlockKey));
            yield return Render("Card_feedback_stars_voted");

            // - no question written: the card asks its own, per look
            yield return ShowBlock("feedback_stars_noquestion", v => feedback = (FeedbackBlockView)v);
            Assert.AreEqual("How would you rate this?", feedback.Question.text);
            yield return ShowBlock("feedback_thumbs_noquestion", v => feedback = (FeedbackBlockView)v);
            Assert.AreEqual("Was this useful?", feedback.Question.text);

            // - a stored vote the look cannot have (a 4 for the thumbs) is no vote, and nothing is reported for it
            _harness.State.SetVote("feedback_thumbs_short", CardGalleryDefinitions.QuizBlockKey, 4);
            yield return ShowBlock("feedback_thumbs_short", v => feedback = (FeedbackBlockView)v);
            Assert.AreEqual(-1, feedback.Voted, "a 4 does not exist on the thumbs");
            Assert.IsFalse(CardTestInput.IsShown(feedback.Thanks, feedback.Root));
            Assert.AreEqual(1, events.Raised.Count);
        }
    }
}
