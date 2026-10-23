using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // The identity of the LivingRoom demo points (_3.3 P1c): the three main points and their fifteen category neighbours carry real
    // place names and real one-line summaries, never fixture labels ("Painting - Military", "POI_painting_military"). Every expectation
    // is read from the shipped config itself (4.2.4b): the test pins the RULE a demo point's identity follows, so a later rename of one
    // point stays green and a fixture label creeping back goes red. Search runs through the real POISearchIndex with the shipped options.
    public class DemoWallIdentityTests
    {
        private static readonly string[] Mains = { "lamp", "painting", "camera" };
        private const string StreamingCopy = "Assets/StreamingAssets/LivingRoom/config.json";

        private static WallConfigData Wall() => JsonUtility.FromJson<WallConfigData>(File.ReadAllText(ShippedWall.ConfigPath));

        // A demo point: a main one or "<main>_<category>"
        private static bool IsDemoPoint(POIData poi) => Mains.Any(m => poi.id == m || poi.id.StartsWith(m + "_"));

        private static string Unaccented(string text) =>
            new string(text.Normalize(NormalizationForm.FormD)
                .Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark).ToArray());

        [Test]
        public void EveryDemoPoint_HasARealName_AndARealSummary_NeverAFixtureLabel()
        {
            var demo = Wall().pois.Where(IsDemoPoint).ToList();
            Assert.AreEqual(Mains.Length * 6, demo.Count, "precondition: three mains and five category neighbours each");
            foreach (var poi in demo)
            {
                Assert.IsNotEmpty(poi.name, poi.id + ": a name");
                Assert.IsFalse(poi.name.Contains(" - "), poi.id + ": '" + poi.name + "' reads like the 'X - Category' fixture label");
                Assert.AreNotEqual(poi.id, poi.name, poi.id + ": the name is not the id");
                Assert.IsNotEmpty(poi.summary, poi.id + ": a summary");
                Assert.IsFalse(poi.summary.StartsWith("POI_"), poi.id + ": '" + poi.summary + "' is a fixture placeholder, not a line a visitor reads");
                Assert.AreNotEqual(poi.id, poi.summary, poi.id + ": the summary is not the id");
                Assert.IsFalse(poi.summary.Contains("\n"), poi.id + ": one line");
            }
        }

        [Test]
        public void TheDemoNames_AreUniqueAcrossTheWall_SoNoTwoMarkersShareALabel()
        {
            var names = Wall().pois.Select(p => p.name).ToList();
            CollectionAssert.AllItemsAreUnique(names, "two points with one name read as one place in the search results and under the markers");
        }

        [Test]
        public void TheTwoConfigCopies_AreIdentical()
        {
            Assert.AreEqual(File.ReadAllText(ShippedWall.ConfigPath), File.ReadAllText(StreamingCopy),
                "the POI Editor's Copy to StreamingAssets keeps the player's copy the same as the Editor's");
        }

        [Test]
        public void TheCategoryNeighbours_StayInTheirCategory_WhateverTheirNames()
        {
            var wall = Wall();
            foreach (string main in Mains)
                foreach (string category in new[] { "religious", "military", "residential", "economic", "infrastructure" })
                {
                    var poi = wall.pois.SingleOrDefault(p => p.id == main + "_" + category);
                    Assert.IsNotNull(poi, main + "_" + category + " is still on the wall");
                    Assert.AreEqual(category, poi.category, poi.id + ": a rename never moves a point to another category");
                }
        }

        [Test]
        public void ANameWord_FindsItsPointFirst_AndThePortugueseNameIsAKeywordThatWorksWithOrWithoutAccents()
        {
            var wall = Wall();
            var index = new POISearchIndex();
            index.Build(wall);
            foreach (string id in new[] { "lamp", "painting" })
            {
                var poi = wall.pois.Single(p => p.id == id);
                string nameWord = poi.name.Split(' ').Last();
                var byName = index.Search(nameWord, SearchOptions.Default);
                Assert.IsNotEmpty(byName, id + ": '" + nameWord + "' finds something");
                Assert.AreEqual(id, byName[0].PoiId, id + ": the last word of its name finds it first (found " + byName[0].PoiId + ")");

                Assert.IsNotEmpty(poi.search_keywords, id + ": its Portuguese name is a keyword (a POI name has no per-language form yet)");
                string keywordWord = poi.search_keywords[0].Split(' ')[0];
                foreach (string typed in new[] { keywordWord, keywordWord.ToLowerInvariant(), Unaccented(keywordWord).ToLowerInvariant() })
                {
                    var byKeyword = index.Search(typed, SearchOptions.Default);
                    Assert.IsNotEmpty(byKeyword, id + ": '" + typed + "' finds something");
                    Assert.AreEqual(id, byKeyword[0].PoiId, id + ": the Portuguese word '" + typed + "' finds it first (found " + byKeyword[0].PoiId + ")");
                }
            }
        }

        [Test]
        public void TheNearbyBlockOfThePainting_ListsPlacesAndNeverATestFixture()
        {
            var wall = Wall();
            var painting = wall.pois.Single(p => p.id == "painting");
            var related = painting.card.blocks.Where(b => b.kind == BuiltInBlocks.RelatedKind).ToList();
            Assert.IsNotEmpty(related, "precondition: The Painting has a related block (Nearby)");
            foreach (var block in related)
            {
                var listed = RelatedPoisRule.Of(painting, block, wall.pois);
                Assert.IsNotEmpty(listed, "the block lists points");
                foreach (var poi in listed)
                {
                    Assert.IsFalse(poi.id.StartsWith("dev_"), "'" + poi.name + "' is a dev fixture, not a place");
                    Assert.IsFalse(poi.name.Contains(" - "), "'" + poi.name + "' is a fixture label");
                    Assert.IsFalse(poi.summary.StartsWith("POI_"), poi.id + " shows its id as its summary");
                }
            }
        }
    }
}
