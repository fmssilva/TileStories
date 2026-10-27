using System.Collections.Generic;
using NUnit.Framework;

namespace TileStories.Tests
{
    // Tier-0 tests for SuggestedSearchesManager: live category distribution,
    // recent-first surfacing and the top-N cap
    // (spec _2.6 section 13).
    public class SuggestedSearchesManagerTests
    {
        private WallConfigData _config;
        private IsolatedVisitorState _device;

        [SetUp]
        public void SetUp()
        {
            // The device store backs RecentSearchesManager, which SuggestedSearchesManager reads in RecentFirst mode: each test
            // starts on an empty one of its own, so execution order cannot leak state and the developer's PlayerPrefs are never touched
            _device = new IsolatedVisitorState();

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
        public void TearDown() => _device.Dispose();

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
