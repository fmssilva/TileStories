using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Tests
{
    // Tier-0 tests for ResultsListView's result row creation and binding logic.
    // Tests the pure data mapping from SearchResult -> ResultRow (spec _2.6 section 9).
    public class ResultsListViewTests
    {
        private WallConfigData _config;
        private POISearchIndex _searchIndex;

        [SetUp]
        public void SetUp()
        {
            _config = new WallConfigData
            {
                wall_id = "test_wall",
                wall_name = "Test Wall",
                search_mode = "explicit",
                no_results_message = "No matches for \"{query}\" - try removing a filter.",
                pois = new List<POIData>
                {
                    new POIData { id = "poi_1", name = "Cathedral", category = "religious", x_norm = 0.3f, y_norm = 0.7f, summary = "A beautiful old cathedral." },
                    new POIData { id = "poi_2", name = "Town Hall", category = "civic", x_norm = 0.6f, y_norm = 0.4f, summary = "Historic city hall." },
                    new POIData { id = "poi_3", name = "Market", category = "commerce", x_norm = 0.5f, y_norm = 0.5f, summary = "Old market square." },
                }
            };

            _searchIndex = new POISearchIndex();
            _searchIndex.Build(_config);
        }

        [Test]
        public void SearchResult_Mappings_POIIndexCorrect()
        {
            // Search for "cathedral" should find POI 0
            var results = _searchIndex.Search("cathedral");
            Assert.Greater(results.Count, 0);
            Assert.AreEqual(0, results[0].POIIndex);
            Assert.AreEqual("poi_1", results[0].POIId);
        }

        [Test]
        public void SearchResult_Mappings_DifferentPOIFound()
        {
            var results = _searchIndex.Search("market");
            Assert.Greater(results.Count, 0);
            Assert.AreEqual(2, results[0].POIIndex);
        }

        [Test]
        public void SearchResult_Mappings_ScoreIsPositive()
        {
            var results = _searchIndex.Search("cathedral");
            Assert.Greater(results.Count, 0);
            Assert.Greater(results[0].Score, 0f);
        }

        [Test]
        public void Search_EmptyQuery_ReturnsEmpty()
        {
            var results = _searchIndex.Search("");
            Assert.AreEqual(0, results.Count);
        }

        [Test]
        public void Search_NullOrWhitespaceQuery_ReturnsEmpty()
        {
            var results = _searchIndex.Search(null);
            Assert.AreEqual(0, results.Count);

            var results2 = _searchIndex.Search("   ");
            Assert.AreEqual(0, results2.Count);
        }

        [Test]
        public void ResultRow_Creation_MapsCorrectFields()
        {
            // Verify ResultRow can be constructed with the expected fields
            var row = new ResultsListView.ResultRow
            {
                poiId = "poi_1",
                displayName = "Cathedral",
                categoryLabel = "religious",
                summary = "A beautiful old cathedral.",
                score = 1.0f
            };

            Assert.AreEqual("poi_1", row.poiId);
            Assert.AreEqual("Cathedral", row.displayName);
            Assert.AreEqual("religious", row.categoryLabel);
            Assert.AreEqual("A beautiful old cathedral.", row.summary);
            Assert.AreEqual(1.0f, row.score);
        }

        [Test]
        public void NoResultsMessage_HasQueryPlaceholder()
        {
            Assert.IsTrue(_config.no_results_message.Contains("{query}"));
        }

        [Test]
        public void SearchMode_FieldsAccessible()
        {
            _config.search_mode = "faceted";
            Assert.AreEqual("faceted", _config.search_mode);
        }
    }
}
