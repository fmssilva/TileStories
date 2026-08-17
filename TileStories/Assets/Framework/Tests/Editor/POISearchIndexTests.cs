using NUnit.Framework;
using System.Collections.Generic;

namespace TileStories.Tests
{
    // Tier 0 EditMode tests for POISearchIndex.
    // Pure logic -- no MonoBehaviour, no scene. Instantiable with new().
    public class POISearchIndexTests
    {
        private WallConfigData MakeConfig(params POIData[] pois)
        {
            return new WallConfigData
            {
                pois = new List<POIData>(pois)
            };
        }

        [Test]
        public void EmptyIndex_SearchReturnsEmpty()
        {
            var index = new POISearchIndex();
            var results = index.Search("anything");
            Assert.AreEqual(0, results.Count);
        }

        [Test]
        public void Build_WithNullIdPOI_SkipsIt()
        {
            var config = MakeConfig(
                new POIData { id = "valid", name = "Church" },
                new POIData { id = "", name = "Empty" }
            );
            var index = new POISearchIndex();
            index.Build(config);

            var results = index.Search("church");
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("valid", results[0].POIId);
        }

        [Test]
        public void Build_CalledTwice_ReplacesNotAdditive()
        {
            var config1 = MakeConfig(
                new POIData { id = "a", name = "Church" }
            );
            var index = new POISearchIndex();
            index.Build(config1);

            var config2 = MakeConfig(
                new POIData { id = "b", name = "Tower" }
            );
            index.Build(config2);

            Assert.AreEqual(0, index.Search("church").Count);
            Assert.AreEqual(1, index.Search("tower").Count);
            Assert.AreEqual("b", index.Search("tower")[0].POIId);
        }

        [Test]
        public void Search_NullQuery_ReturnsEmpty()
        {
            var config = MakeConfig(
                new POIData { id = "a", name = "Church" }
            );
            var index = new POISearchIndex();
            index.Build(config);

            var results = index.Search(null);
            Assert.AreEqual(0, results.Count);
        }

        [Test]
        public void SingleKeyword_FindsPOI()
        {
            var config = MakeConfig(
                new POIData { id = "a", name = "Tower", search_keywords = new List<string> { "spire" } }
            );
            var index = new POISearchIndex();
            index.Build(config);

            var results = index.Search("spire");
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("a", results[0].POIId);
        }

        [Test]
        public void NameExact_RankedFirst()
        {
            var config = MakeConfig(
                new POIData { id = "a", name = "Church" },
                new POIData { id = "b", name = "Tower", search_keywords = new List<string> { "church" } }
            );
            var index = new POISearchIndex();
            index.Build(config);

            var results = index.Search("church");
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(1.0f, results[0].Score, 0.001f);
            Assert.AreEqual("a", results[0].POIId);
            Assert.AreEqual(0.7f, results[1].Score, 0.001f);
            Assert.AreEqual("b", results[1].POIId);
        }

        [Test]
        public void NamePrefix_RankedSecond()
        {
            // 'churc' is a prefix of POI_A name token 'church' -> 0.9
            var config = MakeConfig(
                new POIData { id = "a", name = "Church" },
                new POIData { id = "b", name = "Tower" }
            );
            var index = new POISearchIndex();
            index.Build(config);

            var results = index.Search("churc");
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(0.9f, results[0].Score, 0.001f);
            Assert.AreEqual("a", results[0].POIId);
        }

        [Test]
        public void KeywordMatch_RankedThird()
        {
            var config = MakeConfig(
                new POIData { id = "a", name = "Temple" },
                new POIData { id = "b", name = "Tower", search_keywords = new List<string> { "temple" } }
            );
            var index = new POISearchIndex();
            index.Build(config);

            var results = index.Search("temple");
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(1.0f, results[0].Score, 0.001f);
            Assert.AreEqual("a", results[0].POIId);
            Assert.AreEqual(0.7f, results[1].Score, 0.001f);
            Assert.AreEqual("b", results[1].POIId);
        }

        [Test]
        public void SummarySubstring_RankedFourth()
        {
            var config = MakeConfig(
                new POIData { id = "a", name = "Temple" },
                new POIData { id = "b", name = "Tower", summary = "An old temple site" }
            );
            var index = new POISearchIndex();
            index.Build(config);

            var results = index.Search("temple");
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(1.0f, results[0].Score, 0.001f);
            Assert.AreEqual("a", results[0].POIId);
            Assert.AreEqual(0.4f, results[1].Score, 0.001f);
            Assert.AreEqual("b", results[1].POIId);
        }


        [Test]
        public void TaxonomyKeywordMatch_RankedLowest()
        {
            var config = MakeConfig(
                new POIData { id = "a", name = "Temple" },
                new POIData { id = "b", name = "Tower", category = "religious" }
            );
            config.category_styles = new List<CategoryStyleEntry>
            {
                new CategoryStyleEntry { category = "religious", search_keywords = new List<string> { "temple" } }
            };

            var index = new POISearchIndex();
            index.Build(config);

            var results = index.Search("temple");
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(1.0f, results[0].Score, 0.001f);
            Assert.AreEqual("a", results[0].POIId);
            Assert.AreEqual(0.3f, results[1].Score, 0.001f);
            Assert.AreEqual("b", results[1].POIId);
        }

        [Test]
        public void MultiToken_ScoreIsMaxNotSum()
        {
            // 'church' matches name (1.0), 'tower' matches summary (0.4).
            // Score should be max(1.0, 0.4) = 1.0, not sum(1.4).
            var config = MakeConfig(
                new POIData { id = "a", name = "Church", summary = "tower nearby" }
            );
            var index = new POISearchIndex();
            index.Build(config);

            var results = index.Search("church tower");
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(1.0f, results[0].Score, 0.001f);
        }

        [Test]
        public void GetMatchingKeywords_PrefixMatch()
        {
            var config = MakeConfig(
                new POIData { id = "a", name = "Church" },
                new POIData { id = "b", name = "Tower" }
            );
            var index = new POISearchIndex();
            index.Build(config);

            var keywords = index.GetMatchingKeywords("ch");
            Assert.IsTrue(keywords.Contains("church"));
            Assert.IsFalse(keywords.Contains("tower"));
        }

        [Test]
        public void GetMatchingKeywords_EmptyPrefix_ReturnsEmpty()
        {
            var config = MakeConfig(
                new POIData { id = "a", name = "Church" }
            );
            var index = new POISearchIndex();
            index.Build(config);

            var keywords = index.GetMatchingKeywords("");
            Assert.AreEqual(0, keywords.Count);
        }

        [Test]
        public void Clear_DropsIndex()
        {
            var config = MakeConfig(
                new POIData { id = "a", name = "Church" }
            );
            var index = new POISearchIndex();
            index.Build(config);
            Assert.AreEqual(1, index.Search("church").Count);

            index.Clear();
            Assert.AreEqual(0, index.Search("church").Count);
        }

        [Test]
        public void Build_ReplacesPreviousIndex()
        {
            var config1 = MakeConfig(
                new POIData { id = "a", name = "Old" },
                new POIData { id = "b", name = "Church" }
            );
            var index = new POISearchIndex();
            index.Build(config1);
            Assert.AreEqual(1, index.Search("old").Count);

            var config2 = MakeConfig(
                new POIData { id = "c", name = "New" }
            );
            index.Build(config2);
            Assert.AreEqual(0, index.Search("old").Count);
            Assert.AreEqual(1, index.Search("new").Count);
        }

    }
}
