using System.Collections.Generic;
using NUnit.Framework;

namespace TileStories.Tests
{
    // Tier-0 tests for the SearchMatchMode (any|all) token-coverage policy added
    // to POISearchIndex.Search in Block 4 (spec _2.6 section 5). Locks in the
    // backward-compatible default (Any == existing behavior) and the conjunction
    // semantics of All without relying on Unity lifecycle or a scene.
    public class POISearchIndexMatchModeTests
    {
        private POISearchIndex _index;

        [SetUp]
        public void SetUp()
        {
            _index = new POISearchIndex();
            _index.Build(new WallConfigData
            {
                wall_id = "match_mode_wall",
                wall_name = "Match Mode Wall",
                pois = new List<POIData>
                {
                    // Tokens present: cathedral (religious)
                    new POIData { id = "poi_1", name = "Cathedral", category = "religious", summary = "old stone cathedral" },
                    // Tokens present: market hall (civic)
                    new POIData { id = "poi_2", name = "Market Hall", category = "civic", summary = "covered market hall" },
                    // Tokens present: cathedral market (commerce) -- overlaps both vocabularies
                    new POIData { id = "poi_3", name = "Cathedral Market", category = "commerce", summary = "cathedral market stalls" },
                }
            });
        }

        [Test]
        public void AnyMode_IncludesPOIsMatchingAnyQueryToken()
        {
            // "cathedral market" -> token1=cathedral, token2=market
            // Any: poi_1 (cathedral), poi_2 (market), poi_3 (both) all qualify.
            var results = _index.Search("cathedral market", SearchMatchMode.Any);
            Assert.That(results.Count, Is.EqualTo(3));
        }

        [Test]
        public void AllMode_IncludesOnlyPOIsMatchingEveryQueryToken()
        {
            // All: only poi_3 has both cathedral and market tokens.
            var results = _index.Search("cathedral market", SearchMatchMode.All);
            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0].POIId, Is.EqualTo("poi_3"));
        }

        [Test]
        public void AllMode_ExcludesPOIMissingOneToken()
        {
            var results = _index.Search("cathedral market", SearchMatchMode.All);
            foreach (var r in results)
                Assert.IsTrue(r.POIId == "poi_3", "Only poi_3 has both tokens");
        }

        [Test]
        public void SingleToken_Query_YieldsSameResultsInEitherMode()
        {
            var any = _index.Search("cathedral", SearchMatchMode.Any);
            var all = _index.Search("cathedral", SearchMatchMode.All);
            Assert.That(any.Count, Is.EqualTo(all.Count));
            for (int i = 0; i < any.Count; i++)
                Assert.AreEqual(any[i].POIId, all[i].POIId);
        }

        [Test]
        public void EmptyOrNullQuery_ReturnsEmptyInBothModes()
        {
            Assert.IsEmpty(_index.Search("", SearchMatchMode.Any));
            Assert.IsEmpty(_index.Search("", SearchMatchMode.All));
            Assert.IsEmpty(_index.Search(null, SearchMatchMode.Any));
            Assert.IsEmpty(_index.Search(null, SearchMatchMode.All));
        }

        [Test]
        public void DefaultParameter_IsAny_BackwardCompatible()
        {
            // Existing callers omit the mode; behavior must equal explicit Any.
            var defaulted = _index.Search("cathedral market");
            var any = _index.Search("cathedral market", SearchMatchMode.Any);
            Assert.That(defaulted.Count, Is.EqualTo(any.Count));
        }

        [Test]
        public void AllMode_NoResultWhenOneTokenMatchesNobody()
        {
            // "xyzzy cathedral" -> xyzzy matches nobody; All (conjunction) -> empty.
            var results = _index.Search("xyzzy cathedral", SearchMatchMode.All);
            Assert.IsEmpty(results);
        }

        [Test]
        public void AllMode_PrefixMatchCountsAsTokenCoverage()
        {
            // "cat" is a proper name-prefix of "Cathedral"; matches poi_1 + poi_3 names.
            // "market" matches poi_2 + poi_3. All -> only poi_3.
            var results = _index.Search("cat market", SearchMatchMode.All);
            Assert.That(results.Count, Is.EqualTo(1));
            Assert.AreEqual("poi_3", results[0].POIId);
        }

        [Test]
        public void AllMode_RanksByMaxScore_NotByTokenCount()
        {
            // poi_3 matches both tokens (name rank 1.0 dominates); ensure it still
            // sorts above any POI that only matched a lower-ranked field, and that
            // All-mode does not reorder away the highest-ranked match.
            var results = _index.Search("cathedral market", SearchMatchMode.All);
            Assert.That(results[0].Score, Is.EqualTo(1.0f).Within(0.001f));
        }
    }
}
