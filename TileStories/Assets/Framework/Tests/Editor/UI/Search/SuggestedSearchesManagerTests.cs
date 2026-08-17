using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine; // PlayerPrefs cleanup for order-independent tests

namespace TileStories.Tests
{
    // Tier-0 tests for SuggestedSearchesManager: live category distribution,
    // recent-first surfacing, synonym-key injection and the top-N cap
    // (spec _2.6 section 13).
    public class SuggestedSearchesManagerTests
    {
        private WallConfigData _config;

        [SetUp]
        public void SetUp()
        {
            // PlayerPrefs is the persistence backing for RecentSearchesManager,
            // which SuggestedSearchesManager reads in RecentFirst mode. Wipe it
            // before every test so execution order cannot leak state between
            // tests (matches the isolation discipline in RecentSearchesManagerTests).
            PlayerPrefs.DeleteKey(RecentSearchesManager.PREFS_KEY);

            _config = new WallConfigData
            {
                wall_id = "suggestions_wall",
                wall_name = "Suggestions Wall",
                pois = new List<POIData>
                {
                    new POIData { id = "poi_1", name = "Cathedral", category = "religious" },
                    new POIData { id = "poi_2", name = "Church", category = "religious" },
                    new POIData { id = "poi_3", name = "Town Hall", category = "civic" },
                    new POIData { id = "poi_4", name = "No Category Here", category = "" },
                }
            };
        }

        [TearDown]
        public void TearDown()
        {
            PlayerPrefs.DeleteKey(RecentSearchesManager.PREFS_KEY);
        }

        [Test]
        public void CategoryDistribution_SortedByCountDescThenName()
        {
            var mgr = new SuggestedSearchesManager(topN: 5)
            {
                Source = SuggestedSearchesManager.SuggestedSource.CategoryDistribution
            };

            var result = mgr.BuildSuggestions(_config);

            Assert.That(result, Has.Count.EqualTo(2));
            Assert.AreEqual("religious", result[0]); // 2 POIs -> first
            Assert.AreEqual("civic", result[1]);     // 1 POI -> second
        }

        [Test]
        public void RecentFirst_MixesRecentQueriesThenCategories()
        {
            var recent = new RecentSearchesManager(maxCount: 5);
            recent.Add("bridge");

            var mgr = new SuggestedSearchesManager(topN: 5)
            {
                Source = SuggestedSearchesManager.SuggestedSource.RecentFirst
            };

            var result = mgr.BuildSuggestions(_config, recent);

            Assert.That(result, Has.Count.EqualTo(3));
            Assert.AreEqual("bridge", result[0]);
            Assert.AreEqual("religious", result[1]);
            Assert.AreEqual("civic", result[2]);
        }

        [Test]
        public void RecentFirst_DedupsRecentAgainstCategory()
        {
            // "religious" is both a recent query and a category -> appears once.
            var recent = new RecentSearchesManager(maxCount: 5);
            recent.Add("religious");

            var mgr = new SuggestedSearchesManager(topN: 5)
            {
                Source = SuggestedSearchesManager.SuggestedSource.RecentFirst
            };

            var result = mgr.BuildSuggestions(_config, recent);

            Assert.That(result, Has.Count.EqualTo(2));
            Assert.AreEqual("religious", result[0]);
            Assert.AreEqual("civic", result[1]);
        }

        [Test]
        public void SynonymGroups_KeysSurfacedAsSuggestions()
        {
            var groups = new List<SynonymGroup>
            {
                new SynonymGroup { key = "santuário" },
                new SynonymGroup { key = "azulejo" },
            };

            var mgr = new SuggestedSearchesManager(topN: 5);
            var result = mgr.BuildSuggestions(_config, null, groups);

            // Synonyms first (2), then category back-fill (2) = 4, within topN.
            Assert.That(result, Has.Count.EqualTo(4));
            Assert.AreEqual("santuário", result[0]);
            Assert.AreEqual("azulejo", result[1]);
        }

        [Test]
        public void TopN_CapsResultCount()
        {
            var mgr = new SuggestedSearchesManager(topN: 1);
            var result = mgr.BuildSuggestions(_config);
            Assert.That(result, Has.Count.EqualTo(1));
        }

        [Test]
        public void EmptyConfig_ReturnsNoSuggestions()
        {
            var mgr = new SuggestedSearchesManager();
            var result = mgr.BuildSuggestions(new WallConfigData { pois = new List<POIData>() });
            Assert.IsEmpty(result);
        }

        [Test]
        public void NullConfig_ReturnsNoSuggestions()
        {
            var mgr = new SuggestedSearchesManager();
            var result = mgr.BuildSuggestions(null);
            Assert.IsEmpty(result);
        }
    }
}
