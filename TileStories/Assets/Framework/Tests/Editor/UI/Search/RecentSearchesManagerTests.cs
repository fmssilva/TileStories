using NUnit.Framework;
using UnityEngine;

namespace TileStories.Tests
{
    // Tier-0 tests for RecentSearchesManager: recency ordering, count cap, dedup,
    // persistence round-trip and whitespace handling (spec _2.6 section 13).
    public class RecentSearchesManagerTests
    {
        [SetUp]
        public void SetUp()
        {
            PlayerPrefs.DeleteKey(RecentSearchesManager.PREFS_KEY);
        }

        [TearDown]
        public void TearDown()
        {
            PlayerPrefs.DeleteKey(RecentSearchesManager.PREFS_KEY);
        }

        [Test]
        public void Add_InsertsAtFront_RecencyOrdered()
        {
            var mgr = new RecentSearchesManager(maxCount: 5);
            mgr.Add("cathedral");
            mgr.Add("market");
            mgr.Add("bridge");

            Assert.AreEqual(3, mgr.Entries.Count);
            Assert.AreEqual("bridge", mgr.Entries[0]);
            Assert.AreEqual("market", mgr.Entries[1]);
            Assert.AreEqual("cathedral", mgr.Entries[2]);
        }

        [Test]
        public void Add_Dedups_MovingExistingToFront()
        {
            var mgr = new RecentSearchesManager(maxCount: 5);
            mgr.Add("cathedral");
            mgr.Add("market");
            mgr.Add("cathedral");

            Assert.AreEqual(2, mgr.Entries.Count);
            Assert.AreEqual("cathedral", mgr.Entries[0]);
            Assert.AreEqual("market", mgr.Entries[1]);
        }

        [Test]
        public void Add_TrimsToMaxCount()
        {
            var mgr = new RecentSearchesManager(maxCount: 3);
            mgr.Add("a");
            mgr.Add("b");
            mgr.Add("c");
            mgr.Add("d");

            Assert.AreEqual(3, mgr.Entries.Count);
            Assert.AreEqual("d", mgr.Entries[0]);
            // Oldest ("a") should have been dropped.
            CollectionAssert.DoesNotContain(mgr.Entries, "a");
        }

        [Test]
        public void Add_NullEmptyOrWhitespace_IsNoOp()
        {
            var mgr = new RecentSearchesManager();
            mgr.Add(null);
            mgr.Add("");
            mgr.Add("   ");
            Assert.AreEqual(0, mgr.Entries.Count);
        }

        [Test]
        public void Add_TrimsWhitespace()
        {
            var mgr = new RecentSearchesManager();
            mgr.Add("  cathedral  ");
            Assert.AreEqual(1, mgr.Entries.Count);
            Assert.AreEqual("cathedral", mgr.Entries[0]);
        }

        [Test]
        public void Clear_RemovesAllEntries()
        {
            var mgr = new RecentSearchesManager();
            mgr.Add("cathedral");
            mgr.Add("market");
            mgr.Clear();
            Assert.AreEqual(0, mgr.Entries.Count);
        }

        [Test]
        public void Persistence_NextInstanceReadsSavedEntries()
        {
            var first = new RecentSearchesManager(maxCount: 5);
            first.Add("cathedral");
            first.Add("market");

            var second = new RecentSearchesManager(maxCount: 5);
            Assert.AreEqual(2, second.Entries.Count);
            Assert.AreEqual("market", second.Entries[0]);
            Assert.AreEqual("cathedral", second.Entries[1]);
        }

        [Test]
        public void CorruptedJson_DoesNotThrow_StartsEmpty()
        {
            PlayerPrefs.SetString(RecentSearchesManager.PREFS_KEY, "{ not valid json");
            var mgr = new RecentSearchesManager();
            Assert.AreEqual(0, mgr.Entries.Count);
        }
    }
}
