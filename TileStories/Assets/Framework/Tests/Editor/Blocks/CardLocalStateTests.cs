using System;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // The card's one local state store (_3.1 step 8A): CardLocalState's scoping, its reading of a damaged store, its reset, on
    // the in-memory store AND on the real PlayerPrefs one (a new instance = the app restarted), plus the pure "content seen" rule.
    public class CardLocalStateTests
    {
        // ---------------- scoping ----------------

        [Test]
        public void AnswersVotesAndSeen_AreScopedByWallPoiAndBlock_AndUnsetReadsAsNothing()
        {
            var store = new MemoryCardStateStore();
            var state = new CardLocalState(store, "living_room");
            Assert.AreEqual(-1, state.Answer("lamp", "block_1", 0), "nothing answered yet");
            Assert.AreEqual(-1, state.Vote("lamp", "block_2"));
            Assert.IsFalse(state.Seen("lamp", "block_1"));

            state.SetAnswer("lamp", "block_1", 0, 2);
            state.SetVote("lamp", "block_2", 4);
            state.MarkSeen("lamp", "block_1");
            Assert.AreEqual(2, state.Answer("lamp", "block_1", 0));
            Assert.AreEqual(4, state.Vote("lamp", "block_2"));
            Assert.IsTrue(state.Seen("lamp", "block_1"));

            Assert.AreEqual(-1, state.Answer("lamp", "block_9", 0), "another block");
            Assert.AreEqual(-1, state.Answer("lamp_military", "block_1", 0), "another POI");
            Assert.AreEqual(-1, new CardLocalState(store, "chafariz").Answer("lamp", "block_1", 0), "another wall on the same store");
            Assert.IsFalse(state.Seen("lamp", "block_2"), "a vote is not a 'seen'");
            Assert.AreEqual(-1, state.Vote("lamp", "block_1"), "an answer is not a vote");
        }

        [Test]
        public void EachQuestionOfABlock_KeepsItsOwnAnswer_ZeroIsAnAnswerToo()
        {
            var state = new CardLocalState(new MemoryCardStateStore(), "w");
            state.SetAnswer("p", "b", 0, 0);
            state.SetAnswer("p", "b", 1, 3);
            state.SetAnswer("p", "b", 1, 1);
            Assert.AreEqual(0, state.Answer("p", "b", 0), "choice 0 is an answer, not 'none'");
            Assert.AreEqual(1, state.Answer("p", "b", 1), "the later choice replaced the earlier one");
            Assert.AreEqual(-1, state.Answer("p", "b", 2));
        }

        [Test]
        public void IdsWithDotsOrPercentSigns_NeverMakeTwoPlacesOneKey()
        {
            var store = new MemoryCardStateStore();
            var state = new CardLocalState(store, "w");
            Assert.AreNotEqual(state.KeyOf("a.b", "c", "vote"), state.KeyOf("a", "b.c", "vote"), "a dot moved between two parts");
            Assert.AreNotEqual(state.KeyOf("a%2Eb", "c", "vote"), state.KeyOf("a.b", "c", "vote"), "a literal escape is not a dot");
            state.SetVote("a.b", "c", 1);
            Assert.AreEqual(1, state.Vote("a.b", "c"));
            Assert.AreEqual(-1, state.Vote("a", "b.c"));
            Assert.AreEqual(-1, state.Vote("a%2Eb", "c"));
        }

        // ---------------- what is not a state ----------------

        [Test]
        public void ANegativeChoice_IsNotStored_AndDamagedStoredValues_ReadAsNothing()
        {
            var store = new MemoryCardStateStore();
            var state = new CardLocalState(store, "w");
            state.SetAnswer("p", "b", 0, -1);
            state.SetVote("p", "b", -5);
            Assert.AreEqual(0, store.Count, "nothing was written for a value that is not a choice");
            foreach (string damaged in new[] { "", "abc", "-3", "2.5", " " })
            {
                store.Set(state.KeyOf("p", "b", "answer-0"), damaged);
                Assert.AreEqual(-1, state.Answer("p", "b", 0), "'" + damaged + "' is not a stored choice");
            }
            store.Set(state.KeyOf("p", "b", "seen"), "2");
            Assert.IsFalse(state.Seen("p", "b"), "only 1 means seen");
        }

        [Test]
        public void ANullStore_IsRefused()
        {
            Assert.Throws<ArgumentNullException>(() => new CardLocalState(null, "w"));
        }

        // ---------------- reset ----------------

        [Test]
        public void ResetAll_RemovesExactlyThisWallsEntries_AndCountsThem()
        {
            var store = new MemoryCardStateStore();
            var a = new CardLocalState(store, "wall_a");
            var b = new CardLocalState(store, "wall_b");
            a.SetAnswer("lamp", "block_1", 0, 1);
            a.SetAnswer("lamp", "block_1", 1, 2);
            a.SetVote("lamp", "block_2", 5);
            a.MarkSeen("lamp", "block_3");
            b.SetAnswer("lamp", "block_1", 0, 3);
            b.SetVote("lamp", "block_2", 1);

            Assert.AreEqual(4, a.ResetAll(), "two answers, a vote and a seen");
            Assert.AreEqual(-1, a.Answer("lamp", "block_1", 0));
            Assert.AreEqual(-1, a.Answer("lamp", "block_1", 1));
            Assert.AreEqual(-1, a.Vote("lamp", "block_2"));
            Assert.IsFalse(a.Seen("lamp", "block_3"));
            Assert.AreEqual(3, b.Answer("lamp", "block_1", 0), "the other wall keeps its own");
            Assert.AreEqual(1, b.Vote("lamp", "block_2"));
            foreach (string key in store.Keys) StringAssert.DoesNotContain("wall_a", key, "no key of the reset wall is left, its index included");

            Assert.AreEqual(0, a.ResetAll(), "a second reset has nothing left");
            a.SetVote("lamp", "block_2", 2);
            Assert.AreEqual(1, a.ResetAll(), "the state works again after a reset");
        }

        [Test]
        public void WritingTheSameStateTwice_IndexesItOnce()
        {
            var store = new MemoryCardStateStore();
            var state = new CardLocalState(store, "w");
            state.SetVote("p", "b", 1);
            state.SetVote("p", "b", 5);
            Assert.AreEqual(2, store.Count, "the vote and one index entry");
            Assert.AreEqual(1, state.ResetAll(), "one thing to forget");
        }

        // ---------------- the real PlayerPrefs store ----------------

        [Test]
        public void OnPlayerPrefs_TheStateSurvivesANewInstance_AndResetLeavesNoTrace()
        {
            string wall = "test_wall_" + Guid.NewGuid().ToString("N");
            var first = new CardLocalState(new PlayerPrefsCardStateStore(), wall);
            string answerKey = first.KeyOf("lamp", "block_1", "answer-0");
            string voteKey = first.KeyOf("lamp", "block_2", "vote");
            try
            {
                first.SetAnswer("lamp", "block_1", 0, 2);
                first.SetVote("lamp", "block_2", 5);
                first.MarkSeen("lamp", "block_1");
                Assert.IsTrue(PlayerPrefs.HasKey(answerKey), "written to PlayerPrefs itself, under the scoped key");

                var restarted = new CardLocalState(new PlayerPrefsCardStateStore(), wall);
                Assert.AreEqual(2, restarted.Answer("lamp", "block_1", 0), "a new instance (the app restarted) reads the answer");
                Assert.AreEqual(5, restarted.Vote("lamp", "block_2"));
                Assert.IsTrue(restarted.Seen("lamp", "block_1"));

                Assert.AreEqual(3, restarted.ResetAll(), "a reset by the new instance finds what the first wrote (the index is in the store)");
                Assert.IsFalse(PlayerPrefs.HasKey(answerKey));
                Assert.IsFalse(PlayerPrefs.HasKey(voteKey));
                Assert.IsFalse(PlayerPrefs.HasKey(restarted.KeyOf("lamp", "block_1", "seen")));
                Assert.AreEqual(-1, first.Answer("lamp", "block_1", 0));
            }
            finally
            {
                first.ResetAll();
                PlayerPrefs.DeleteKey(answerKey);
                PlayerPrefs.DeleteKey(voteKey);
            }
        }

        [Test]
        public void ThePlayerPrefsStore_ReportsAMissingKey_AndRemovingOneIsHarmless()
        {
            var store = new PlayerPrefsCardStateStore();
            string key = CardLocalState.KeyPrefix + "test_" + Guid.NewGuid().ToString("N");
            try
            {
                Assert.IsFalse(store.TryGet(key, out string none));
                Assert.AreEqual("", none);
                store.Remove(key);
                store.Set(key, "");
                Assert.IsTrue(store.TryGet(key, out string empty), "an empty value is stored, not 'missing'");
                Assert.AreEqual("", empty);
                store.Set(key, "7");
                Assert.IsTrue(store.TryGet(key, out string seven));
                Assert.AreEqual("7", seven);
                store.Remove(key);
                Assert.IsFalse(store.TryGet(key, out _));
            }
            finally { PlayerPrefs.DeleteKey(key); }
        }

        // ---------------- viewed (the contents list's tick, _3.2 step 11b) ----------------

        [Test]
        public void Viewed_IsScopedByWallPoiAndBlock_AndIsNotASeenOrAnAnswer()
        {
            var store = new MemoryCardStateStore();
            var state = new CardLocalState(store, "living_room");
            Assert.IsFalse(state.Viewed("lamp", "block_2"), "never in view yet");
            state.MarkViewed("lamp", "block_2");
            Assert.IsTrue(state.Viewed("lamp", "block_2"));
            // - risk (a) of the brief: the key holds the wall, so a visitor who walks to another wall arrives with no tick
            Assert.IsFalse(new CardLocalState(store, "chafariz").Viewed("lamp", "block_2"), "another wall on the same device: no tick");
            Assert.IsFalse(state.Viewed("lamp_military", "block_2"), "another POI");
            Assert.IsFalse(state.Viewed("lamp", "block_3"), "another block");
            Assert.IsFalse(state.Seen("lamp", "block_2"), "in view is not 'revealed'");
            state.MarkSeen("lamp", "block_4");
            Assert.IsFalse(state.Viewed("lamp", "block_4"), "a revealed block is not ticked by that alone");
            StringAssert.Contains("living_room", state.KeyOf("lamp", "block_2", CardLocalState.ViewedSlot), "the wall is a part of the key");
        }

        [Test]
        public void MarkViewedTwice_WritesOnce_AndCountViewedCountsOnlyTheViewedEntries()
        {
            var store = new MemoryCardStateStore();
            var state = new CardLocalState(store, "w");
            Assert.AreEqual(0, state.CountViewed(), "nothing saved");
            state.MarkViewed("p", "b1");
            int afterOne = store.Count;
            state.MarkViewed("p", "b1");
            Assert.AreEqual(afterOne, store.Count, "a second mark writes nothing");
            state.MarkViewed("p", "b2");
            state.SetAnswer("p", "b3", 0, 1);
            state.MarkSeen("p", "b4");
            state.SetLanguage("pt");
            Assert.AreEqual(2, state.CountViewed(), "two viewed blocks, whatever else is saved");
            Assert.AreEqual(0, new CardLocalState(store, "other").CountViewed(), "another wall's count is its own");
            Assert.AreEqual(5, state.ResetAll(), "the reset clears the viewed slot with everything else: 2 viewed + answer + seen + language");
            Assert.AreEqual(0, state.CountViewed());
            Assert.IsFalse(state.Viewed("p", "b1"));
        }

        [Test]
        public void OnPlayerPrefs_AViewedTick_SurvivesARestart_UnderAColonKey_AndTheResetRemovesIt()
        {
            // - risk (b) of the brief: an Auto Nav Block's key is "auto:<kind>" -- a colon in a PlayerPrefs key (the registry on Windows,
            //   an XML attribute on Android); it must read back exactly and be found by the reset
            string wall = "test_wall_" + Guid.NewGuid().ToString("N");
            string autoKey = BlockStackBuilder.AutoNavBlockKey("nav_contents");
            StringAssert.Contains(":", autoKey, "precondition: the auto key holds its colon");
            var first = new CardLocalState(new PlayerPrefsCardStateStore(), wall);
            string viewedKey = first.KeyOf("lamp", autoKey, CardLocalState.ViewedSlot);
            string blockKey = first.KeyOf("lamp", "block_2", CardLocalState.ViewedSlot);
            try
            {
                first.MarkViewed("lamp", autoKey);
                first.MarkViewed("lamp", "block_2");
                PlayerPrefs.Save();
                Assert.IsTrue(PlayerPrefs.HasKey(viewedKey), "written to PlayerPrefs itself, colon and all: " + viewedKey);
                Assert.AreEqual("1", PlayerPrefs.GetString(viewedKey, ""), "read back exactly");

                var restarted = new CardLocalState(new PlayerPrefsCardStateStore(), wall);
                Assert.IsTrue(restarted.Viewed("lamp", autoKey), "a new instance (the app restarted) still has the colon key's tick");
                Assert.IsTrue(restarted.Viewed("lamp", "block_2"));
                Assert.AreEqual(2, restarted.CountViewed());
                Assert.AreEqual(2, restarted.ResetAll(), "the reset walks the index and finds the colon key too");
                Assert.IsFalse(PlayerPrefs.HasKey(viewedKey));
                Assert.IsFalse(PlayerPrefs.HasKey(blockKey));
                Assert.IsFalse(first.Viewed("lamp", autoKey));
            }
            finally
            {
                first.ResetAll();
                PlayerPrefs.DeleteKey(viewedKey);
                PlayerPrefs.DeleteKey(blockKey);
            }
        }

        // ---------------- content seen ----------------

        [Test]
        public void ContentSeen_WhenTheEndIsReached_OrAllOfItFits_NeverBeforeALayoutOrAtPeek()
        {
            Assert.IsFalse(ContentSeenRule.HasSeenAll(0f, 400f, 300f), "top of a long card");
            Assert.IsFalse(ContentSeenRule.HasSeenAll(200f, 400f, 300f), "half way");
            Assert.IsFalse(ContentSeenRule.HasSeenAll(400f - ContentSeenRule.EndTolerance - 1f, 400f, 300f), "just short of the tolerance");
            Assert.IsTrue(ContentSeenRule.HasSeenAll(400f - ContentSeenRule.EndTolerance, 400f, 300f), "within the tolerance of the end");
            Assert.IsTrue(ContentSeenRule.HasSeenAll(400f, 400f, 300f), "at the end");
            Assert.IsTrue(ContentSeenRule.HasSeenAll(0f, 0f, 300f), "all of it fits: nothing to scroll");
            Assert.IsTrue(ContentSeenRule.HasSeenAll(0f, ContentSeenRule.EndTolerance, 300f), "a range of about nothing counts as fitting");
            Assert.IsFalse(ContentSeenRule.HasSeenAll(0f, 400f, 0f), "no viewport (the peek stop): nothing is visible");
            Assert.IsFalse(ContentSeenRule.HasSeenAll(0f, 0f, 0f), "...even when there is nothing to scroll");
            Assert.IsFalse(ContentSeenRule.HasSeenAll(float.NaN, 0f, 300f), "no layout yet");
            Assert.IsFalse(ContentSeenRule.HasSeenAll(0f, float.NaN, 300f));
            Assert.IsFalse(ContentSeenRule.HasSeenAll(0f, 0f, float.NaN));
        }

        // _3.2 step 14: ONE reading rule for every layout. Scroll / accordion show every section in one scroll (all shown now); tabs and
        // pages show one section and set the others aside, which then count only once each was read to its end
        [Test]
        public void TheCard_IsRead_WhenTheShownPartIsAtItsEnd_AndEverySectionSetAsideWasReadToItsEnd()
        {
            var shown = new ContentSeenRule.Section(shownNow: true, readToItsEnd: false);
            var asideRead = new ContentSeenRule.Section(shownNow: false, readToItsEnd: true);
            var asideUnread = new ContentSeenRule.Section(shownNow: false, readToItsEnd: false);

            // - scroll: every section in the one scroll, so the stack's end is the card's end (unchanged)
            Assert.IsTrue(ContentSeenRule.HasReadCard(true, new[] { shown, shown, shown }), "scroll at its end");
            Assert.IsFalse(ContentSeenRule.HasReadCard(false, new[] { shown, shown, shown }), "scroll half way");
            Assert.IsTrue(ContentSeenRule.HasReadCard(true, null), "no layout bound: the stack's end decides");
            // - tabs / pages: a short section at its end is not the card
            Assert.IsFalse(ContentSeenRule.HasReadCard(true, new[] { asideUnread, shown, asideUnread }), "one short section read, two left");
            Assert.IsFalse(ContentSeenRule.HasReadCard(true, new[] { asideRead, shown, asideUnread }), "one section left");
            Assert.IsTrue(ContentSeenRule.HasReadCard(true, new[] { asideRead, shown, asideRead }), "the last one read to its end");
            Assert.IsFalse(ContentSeenRule.HasReadCard(false, new[] { asideRead, shown, asideRead }), "every other read, the shown one not at its end");
        }

        [Test]
        public void ASection_IsReadToItsEnd_WhenItsLastTargetShowingContentWasViewed()
        {
            Assert.IsTrue(ContentSeenRule.ReadToItsEnd(new[] { false, false, true }, new[] { false, false, false }),
                "the last block viewed: the reader reached the end (blocks above flown past still count)");
            Assert.IsFalse(ContentSeenRule.ReadToItsEnd(new[] { true, true, false }, new[] { false, false, false }), "stopped before the last block");
            Assert.IsTrue(ContentSeenRule.ReadToItsEnd(new[] { false, true, false }, new[] { false, false, true }),
                "a closed row last shows nothing: the block before it decides");
            Assert.IsFalse(ContentSeenRule.ReadToItsEnd(new[] { true, false, false }, new[] { false, false, true }), "...and it was not viewed");
            Assert.IsTrue(ContentSeenRule.ReadToItsEnd(new[] { false, false }, new[] { true, true }), "only closed rows: nothing to read");
        }
    }
}
