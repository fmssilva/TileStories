using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace TileStories.Tests
{
    // Tier 0 EditMode tests for the synonym expansion path of POISearchIndex.
    // Requires #if UNITY_EDITOR (EditMode tests always define UNITY_EDITOR).
    public class SearchSynonymGroupsTests
    {
        private WallConfigData MakeConfigWithSingleKeywordPoi(string id, string name, string keyword)
        {
            return new WallConfigData
            {
                pois = new List<POIData>
                {
                    new POIData { id = id, name = name, search_keywords = new List<string> { keyword } }
                }
            };
        }

        // 2.6-al: synonyms are now plain Runtime data (List<SynonymGroup>) on WallConfigData,
        // not an Editor-only ScriptableObject asset. The deleted SearchSynonymGroups asset was
        // never needed -- ConfigureWithSynonyms consumes the plain data list directly.
        private static List<SynonymGroup> MakeSynonymGroups(string key, params string[] synonyms)
        {
            return new List<SynonymGroup>
            {
                new SynonymGroup { key = key, synonyms = new List<string>(synonyms) }
            };
        }

        [Test]
        public void ConfigureWithSynonyms_ExtendsSearch()
        {
            var config = MakeConfigWithSingleKeywordPoi("b", "Tower", "church");
            var index = new POISearchIndex();
            index.Build(config);

            // Before synonym configuration: "templo" finds nothing
            var beforeResults = index.Search("templo");
            Assert.AreEqual(0, beforeResults.Count);

            // After: "templo" is a synonym of "church", so POI_B should appear
            var synonyms = MakeSynonymGroups("church", "templo");
            index.ConfigureWithSynonyms(synonyms);

            var afterResults = index.Search("templo");
            Assert.AreEqual(1, afterResults.Count);
            Assert.AreEqual("b", afterResults[0].POIId);
        }

        [Test]
        public void ConfigureWithSynonyms_EmptyGroups_NoChange()
        {
            var config = MakeConfigWithSingleKeywordPoi("a", "Tower", "church");
            var index = new POISearchIndex();
            index.Build(config);

            int countBefore = index.Search("church").Count;
            Assert.AreEqual(1, countBefore);

            var emptyGroups = new List<SynonymGroup>();
            index.ConfigureWithSynonyms(emptyGroups);

            // Existing search must be unchanged
            int countAfter = index.Search("church").Count;
            Assert.AreEqual(countBefore, countAfter);
        }

        [Test]
        public void BuildAfterConfigure_PreservesSynonymIndex()
        {
            var config = MakeConfigWithSingleKeywordPoi("b", "Tower", "church");
            var index = new POISearchIndex();

            // Build, configure synonyms, verify synonym search works
            index.Build(config);
            var synonyms = MakeSynonymGroups("church", "templo");
            index.ConfigureWithSynonyms(synonyms);

            Assert.AreEqual(1, index.Search("templo").Count);

            // Re-Build (which calls Clear) then re-configure: synonyms must
            // reappear after the second Configure call
            index.Build(config);
            Assert.AreEqual(0, index.Search("templo").Count, "Build should clear synonyms");

            index.ConfigureWithSynonyms(synonyms);
            Assert.AreEqual(1, index.Search("templo").Count, "Re-configure should restore synonyms");
        }
    }
}
