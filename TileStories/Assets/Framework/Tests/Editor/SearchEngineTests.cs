using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // The search engine (POISearchIndex, SearchTextDistance, spec _2.6 section 5) on a fabricated wall whose
    // expected ranks are known exactly: every source ranks at its tier, every option (Match Words, Partial
    // Words, Typo Tolerance) changes what matches, synonyms work both ways, filters narrow by id, and each
    // bug the 2026-09-25 review found has its regression test (hierarchy keywords not indexed, badge /
    // outline labels not indexed, reverse name prefix, empty filter set treated as no filter, results mapped
    // by list index). Plain C#, real classes, no mocks.
    public class SearchEngineTests
    {
        private static SearchOptions Opt(SearchMatchMode match = SearchMatchMode.All,
            SearchPrefixScope prefix = SearchPrefixScope.AllFields, int typos = 1) => new(match, prefix, typos);

        private static WallConfigData Wall()
        {
            return new WallConfigData
            {
                category_styles = new List<CategoryStyleEntry>
                {
                    new() { category = "religious", search_keywords = new List<string> { "worship" } },
                    new() { category = "military" },
                },
                badge_categories = new List<BadgeCategoryEntry>
                {
                    new() { key = "partial_damage", label = "Cracked Walls", search_keywords = new List<string> { "fissure" } },
                },
                outline_levels = new List<OutlineLevelEntry>
                {
                    new() { key = "lost", label = "Vanished", search_keywords = new List<string> { "gone" } },
                },
                hierarchy_levels = new List<HierarchyLevelEntry>
                {
                    new() { key = "level_1", level_name = "Landmark", priority = 1, search_keywords = new List<string> { "highlight" } },
                    new() { key = "level_2", level_name = "Detail", priority = 2 },
                },
                pois = new List<POIData>
                {
                    new() { id = "a", name = "Church of Mercy", category = "religious", hierarchy_level_key = "level_1",
                            search_keywords = new List<string> { "baroque" } },
                    new() { id = "b", name = "Old Tower", category = "military", badge_category = "partial_damage",
                            has_status = true, status_level_key = "lost", hierarchy_level_key = "level_2",
                            summary = "a watchtower above the harbour" },
                    new() { id = "c", name = "Lantern Gate", category = "military",
                            search_keyword_fields = new List<POISearchKeywordField>
                            { new() { field_key = "material", keywords = new List<string> { "granite" } } } },
                    new() { id = "d", name = "Café Álvaro", category = "religious" },
                    new() { id = "e", name = "Rua de Santa Marta", category = "military" },
                },
            };
        }

        private static POISearchIndex Index(WallConfigData config)
        {
            var index = new POISearchIndex();
            index.Build(config);
            return index;
        }

        private static Dictionary<string, float> Scores(POISearchIndex index, string query, SearchOptions options,
            ICollection<string> candidates = null) =>
            index.Search(query, options, candidates).ToDictionary(r => r.PoiId, r => r.Score);

        // ---- every source at its tier ----

        [Test]
        public void EverySource_RanksAtItsTier()
        {
            var index = Index(Wall());
            var exact = Opt(prefix: SearchPrefixScope.Off, typos: 0);
            Assert.AreEqual(POISearchIndex.RankName, Scores(index, "tower", exact)["b"], 1e-5, "a name word");
            Assert.AreEqual(POISearchIndex.RankKeyword, Scores(index, "baroque", exact)["a"], 1e-5, "the POI's own keyword");
            Assert.AreEqual(POISearchIndex.RankKeyword, Scores(index, "granite", exact)["c"], 1e-5, "a custom keyword field");
            Assert.AreEqual(POISearchIndex.RankSummary, Scores(index, "harbour", exact)["b"], 1e-5, "a summary word");
            Assert.AreEqual(POISearchIndex.RankTaxonomy, Scores(index, "worship", exact)["a"], 1e-5, "a category row keyword");
        }

        [Test]
        public void ANameMatch_OutranksAKeywordMatch_OutranksATaxonomyMatch()
        {
            var config = Wall();
            config.pois[1].search_keywords = new List<string> { "mercy" };     // Old Tower: keyword 'mercy'
            config.category_styles[1].search_keywords = new List<string> { "mercy" }; // every military POI: taxonomy 'mercy'
            var results = Index(config).Search("mercy", Opt(prefix: SearchPrefixScope.Off, typos: 0));
            CollectionAssert.AreEqual(new[] { "a", "b", "c", "e" }, results.Select(r => r.PoiId).ToArray(),
                "name (Church of Mercy) > own keyword (Old Tower) > taxonomy (the other military POIs, config order)");
        }

        // ---- the regressions of the review ----

        [Test]
        public void HierarchyLevelNameAndItsSearchKeywords_AreIndexed()
        {
            var index = Index(Wall());
            var exact = Opt(prefix: SearchPrefixScope.Off, typos: 0);
            CollectionAssert.AreEquivalent(new[] { "a" }, Scores(index, "landmark", exact).Keys, "the level's name");
            CollectionAssert.AreEquivalent(new[] { "a" }, Scores(index, "highlight", exact).Keys,
                "the level's own Search Keywords (were never indexed)");
        }

        [Test]
        public void BadgeAndOutlineLabelsAndKeywords_AreIndexed_TheOutlineOnlyWithAStatus()
        {
            var config = Wall();
            var exact = Opt(prefix: SearchPrefixScope.Off, typos: 0);
            var index = Index(config);
            CollectionAssert.AreEquivalent(new[] { "b" }, Scores(index, "cracked", exact).Keys, "the badge's label");
            CollectionAssert.AreEquivalent(new[] { "b" }, Scores(index, "fissure", exact).Keys, "the badge row's keywords");
            CollectionAssert.AreEquivalent(new[] { "b" }, Scores(index, "vanished", exact).Keys, "the outline type's label");
            CollectionAssert.AreEquivalent(new[] { "b" }, Scores(index, "gone", exact).Keys, "the outline row's keywords");

            config.pois[1].has_status = false;
            CollectionAssert.IsEmpty(Scores(Index(config), "vanished", exact).Keys, "without a status the outline type is not the POI's");
        }

        [Test]
        public void AShortNameWord_NeverMatchesALongerQueryWord()
        {
            // "Rua de Santa Marta" has the name word "de": the old reverse prefix made every query starting with
            // "de" ("detail", "desk") match it at 0.9
            var scores = Scores(Index(Wall()), "desk", Opt(typos: 0));
            Assert.IsFalse(scores.ContainsKey("e"), "'de' in a name must not match the query 'desk'");
        }

        [Test]
        public void AFilterMatchingNothing_LeavesNothingToSearch()
        {
            var index = Index(Wall());
            CollectionAssert.IsEmpty(index.Search("tower", Opt(), new HashSet<string>()),
                "an empty candidate set (a filter that matches no POI) must return nothing, not every POI");
            CollectionAssert.IsNotEmpty(index.Search("tower", Opt(), null), "null = no filter: every POI is searched");
        }

        [Test]
        public void Results_CarryTheirOwnPoi_EvenWhenRowsAreSkipped()
        {
            var config = Wall();
            config.pois.Insert(0, new POIData { id = "", name = "No Id Tower" });           // skipped: no id
            config.pois.Insert(1, new POIData { id = "b", name = "Duplicate Tower" });      // first "b" wins
            var results = Index(config).Search("tower", Opt(typos: 0));
            Assert.AreEqual(1, results.Count, "only one POI named tower is indexed");
            Assert.AreSame(config.pois[1], results[0].Poi, "the result must carry the POI the index holds, not a list index");
            Assert.AreEqual("b", results[0].PoiId);
        }

        // ---- each option ----

        [Test]
        public void MatchWords_EveryWordVersusAnyWord()
        {
            var index = Index(Wall());
            CollectionAssert.AreEquivalent(new[] { "b" }, Scores(index, "old tower", Opt(SearchMatchMode.All, typos: 0)).Keys);
            CollectionAssert.IsEmpty(Scores(index, "tower church", Opt(SearchMatchMode.All, SearchPrefixScope.Off, 0)).Keys,
                "Every word: no POI holds both");
            CollectionAssert.AreEquivalent(new[] { "a", "b" }, Scores(index, "tower church", Opt(SearchMatchMode.Any, SearchPrefixScope.Off, 0)).Keys,
                "Any word: each POI holding one of them");
        }

        [Test]
        public void PartialWords_OffNamesOnlyAllFields()
        {
            var index = Index(Wall());
            CollectionAssert.IsEmpty(Scores(index, "lante", Opt(prefix: SearchPrefixScope.Off, typos: 0)).Keys, "Off: 'lante' completes nothing");
            Assert.AreEqual(POISearchIndex.RankName * POISearchIndex.PrefixFactor,
                Scores(index, "lante", Opt(prefix: SearchPrefixScope.NameOnly, typos: 0))["c"], 1e-5, "Names only: a name word");
            CollectionAssert.IsEmpty(Scores(index, "grani", Opt(prefix: SearchPrefixScope.NameOnly, typos: 0)).Keys,
                "Names only: a keyword is not completed");
            Assert.AreEqual(POISearchIndex.RankKeyword * POISearchIndex.PrefixFactor,
                Scores(index, "grani", Opt(prefix: SearchPrefixScope.AllFields, typos: 0))["c"], 1e-5, "All fields: a keyword too");
            CollectionAssert.IsEmpty(Scores(index, "l", Opt(prefix: SearchPrefixScope.AllFields, typos: 0)).Keys,
                "one letter completes nothing (MinPrefixLength)");
        }

        [Test]
        public void TypoTolerance_ForgivesBySettingAndWordLength()
        {
            var index = Index(Wall());
            CollectionAssert.IsEmpty(Scores(index, "lantren", Opt(prefix: SearchPrefixScope.Off, typos: 0)).Keys, "0: exact only");
            Assert.AreEqual(POISearchIndex.RankName * POISearchIndex.OneTypoFactor,
                Scores(index, "lantren", Opt(prefix: SearchPrefixScope.Off, typos: 1))["c"], 1e-5, "1: a swapped pair is one typo");
            Assert.IsTrue(Scores(index, "lanturn", Opt(prefix: SearchPrefixScope.Off, typos: 1)).ContainsKey("c"), "a wrong letter is one typo");
            CollectionAssert.IsEmpty(Scores(index, "olf", Opt(prefix: SearchPrefixScope.Off, typos: 2)).Keys,
                "a 3-letter word is never corrected ('olf' is not 'old')");
            CollectionAssert.IsEmpty(Scores(index, "latnren", Opt(prefix: SearchPrefixScope.Off, typos: 1)).Keys, "two typos need 2");
            CollectionAssert.IsEmpty(Scores(index, "latnren", Opt(prefix: SearchPrefixScope.Off, typos: 2)).Keys,
                "two typos only for words of 8+ letters");
            Assert.AreEqual(POISearchIndex.RankSummary * POISearchIndex.TwoTypoFactor,
                Scores(index, "watchtwoerr", Opt(prefix: SearchPrefixScope.Off, typos: 2))["b"], 1e-5, "a long word: two typos at 2");
        }

        [Test]
        public void Accents_AreIgnoredBothWays()
        {
            var index = Index(Wall());
            CollectionAssert.AreEquivalent(new[] { "d" }, Scores(index, "cafe alvaro", Opt(typos: 0)).Keys);
            CollectionAssert.AreEquivalent(new[] { "d" }, Scores(index, "CAFÉ", Opt(typos: 0)).Keys);
        }

        [Test]
        public void SynonymGroups_WorkBothWays_AndNeverOutrankTheirSource()
        {
            var config = Wall();
            config.synonym_groups = new List<SynonymGroup> { new() { key = "chapel", synonyms = new List<string> { "church", "temple" } } };
            var index = Index(config);
            var exact = Opt(prefix: SearchPrefixScope.Off, typos: 0);

            // "church" is a NAME word of a (1.0): its synonyms reach a, capped at RankSynonym
            Assert.AreEqual(POISearchIndex.RankSynonym, Scores(index, "temple", exact)["a"], 1e-5, "church -> temple");
            Assert.AreEqual(POISearchIndex.RankSynonym, Scores(index, "chapel", exact)["a"], 1e-5, "church -> the group's key");

            config.pois[1].search_keywords = new List<string> { "chapel" };
            index = Index(config);
            Assert.AreEqual(POISearchIndex.RankSynonym, Scores(index, "church", exact)["b"], 1e-5, "chapel -> church (the reverse way)");

            config.pois[1].search_keywords.Clear();
            config.category_styles[1].search_keywords = new List<string> { "temple" };   // a TAXONOMY word (0.3)
            index = Index(config);
            Assert.AreEqual(POISearchIndex.RankTaxonomy, Scores(index, "chapel", exact)["b"], 1e-5,
                "a synonym of a taxonomy word stays at the taxonomy rank (never above its source)");
        }

        [Test]
        public void AnEmptyQuery_ShowsTheFilteredSetInConfigOrder_OrNothingWithoutAFilter()
        {
            var index = Index(Wall());
            CollectionAssert.IsEmpty(index.Search("", Opt(), null), "no query, no filter: nothing");
            CollectionAssert.AreEqual(new[] { "b", "c", "e" },
                index.Search("  ", Opt(), new HashSet<string> { "e", "c", "b" }).Select(r => r.PoiId).ToArray(),
                "no query + a filter: the filtered POIs in config order");
        }

        [Test]
        public void Build_ReplacesTheIndex_AndReportsItsSize()
        {
            var index = Index(Wall());
            Assert.AreEqual(5, index.PoiCount);
            var smaller = Wall();
            smaller.pois.RemoveAt(0);
            index.Build(smaller);
            Assert.AreEqual(4, index.PoiCount);
            CollectionAssert.IsEmpty(index.Search("mercy", Opt()), "a rebuilt index forgets the old POIs");
        }

        [Test]
        public void EachResult_SaysWhyItMatched()
        {
            var index = Index(Wall());
            Assert.AreEqual("name", index.Search("tower", Opt(typos: 0))[0].Reason);
            Assert.AreEqual("name, partial word", index.Search("towe", Opt(typos: 0))[0].Reason);
            Assert.AreEqual("name, typo", index.Search("lantren", Opt(prefix: SearchPrefixScope.Off))[0].Reason);
            Assert.AreEqual("keyword", index.Search("granite", Opt(typos: 0))[0].Reason);
            Assert.AreEqual("summary", index.Search("harbour", Opt(typos: 0))[0].Reason);
            Assert.AreEqual("taxonomy", index.Search("worship", Opt(typos: 0))[0].Reason);
        }

        [TestCase("lamp", "lamp", 0)]
        [TestCase("lamp", "lmap", 1)]      // swapped pair
        [TestCase("lamp", "lamb", 1)]      // substitution
        [TestCase("lamp", "lampe", 1)]     // insertion
        [TestCase("lamp", "lap", 1)]       // deletion
        [TestCase("lantern", "latnren", 2)]
        public void SearchTextDistance_CountsEdits(string a, string b, int expected)
        {
            Assert.AreEqual(expected, SearchTextDistance.Bounded(a, b, 3));
        }

        [Test]
        public void SearchTextDistance_StopsAtTheLimit()
        {
            Assert.AreEqual(2, SearchTextDistance.Bounded("abcdef", "uvwxyz", 1), "over the limit reports limit + 1");
            Assert.AreEqual(2, SearchTextDistance.Bounded("a", "abcdef", 1), "a length gap over the limit");
        }

        // ---- the shipped wall ----

        [Test]
        public void ShippedWall_EveryPoiIsFoundByItsOwnName()
        {
            var config = JsonUtility.FromJson<WallConfigData>(
                File.ReadAllText(Path.Combine(Application.dataPath, "Apps/LivingRoom/config.json")));
            var index = Index(config);
            foreach (var poi in config.pois)
                Assert.IsTrue(index.Search(poi.name, SelectFilterSearchOptions.ToSearchOptions(config.select_filter_search.search))
                    .Any(r => r.PoiId == poi.id), "'" + poi.name + "' must find " + poi.id);
        }

        // ---- SearchKeywordSources: the one list behind the index AND the Editor's "Found by" ----

        private static WallConfigData WallWithSynonyms()
        {
            var config = Wall();
            config.synonym_groups = new List<SynonymGroup> { new() { key = "church", synonyms = new List<string> { "chapel" } } };
            config.search_fields = new List<SearchFieldDefinition> { new() { key = "material", label = "Material" } };
            return config;
        }

        [Test]
        public void FoundBy_EveryListedWord_FindsItsPoi_AtLeastAtTheListedRank()
        {
            var config = WallWithSynonyms();
            var index = Index(config);
            var exact = Opt(SearchMatchMode.All, SearchPrefixScope.Off, 0);
            int checkedWords = 0;
            foreach (var poi in config.pois)
                foreach (var word in SearchKeywordSources.Collect(config, poi))
                    foreach (var token in SearchTokenizer.Tokenize(word.Text))
                    {
                        var scores = Scores(index, token, exact);
                        Assert.IsTrue(scores.ContainsKey(poi.id), $"'{token}' ({word.Origin}) is listed for {poi.id}, so the search must find it");
                        Assert.GreaterOrEqual(scores[poi.id], word.Rank - 1e-5f, $"'{token}' ranks at least as listed");
                        checkedWords++;
                    }
            Assert.Greater(checkedWords, 20, "precondition: the fabricated wall has words to check");
        }

        [Test]
        public void FoundBy_NamesEveryOrigin_TheOutlineOnlyWithAStatus_AndSynonymsBelowTheirSource()
        {
            var config = WallWithSynonyms();
            List<string> Origins(string id) => SearchKeywordSources.Collect(config, config.pois.First(p => p.id == id))
                .Select(w => w.Origin).Distinct().ToList();

            CollectionAssert.AreEqual(new[] { "Name", "Summary", "Category", "Badge", "Status", "Level" }, Origins("b"),
                "every source of b, strongest first");
            CollectionAssert.Contains(Origins("c"), "Material", "a keyword field is named by its label");
            CollectionAssert.Contains(Origins("a"), "Others");

            var tower = config.pois.First(p => p.id == "b");
            tower.has_status = false;
            CollectionAssert.DoesNotContain(Origins("b"), "Status", "no status: the outline row is not a source");
            Assert.IsFalse(Scores(Index(config), "vanished", Opt()).ContainsKey("b"), "and the index agrees");

            var chapel = SearchKeywordSources.Collect(config, config.pois.First(p => p.id == "a")).Single(w => w.Text == "chapel");
            Assert.AreEqual("Synonyms", chapel.Origin);
            Assert.AreEqual(POISearchIndex.RankSynonym, chapel.Rank, 1e-5, "a synonym of a name word ranks at the synonym tier");
        }

        [Test]
        public void FoundByText_IsOneLinePerOrigin_InRankOrder()
        {
            var config = WallWithSynonyms();
            string text = POIEditorToolWindow.FoundByText(SearchKeywordSources.Collect(config, config.pois.First(p => p.id == "a")));
            var lines = text.Split('\n');
            Assert.AreEqual("Name: Church of Mercy", lines[0]);
            Assert.AreEqual("Others: baroque", lines[1]);
            StringAssert.StartsWith("Category: religious, worship", lines[2]);
            StringAssert.StartsWith("Level: Landmark, highlight", lines[3]);
            Assert.AreEqual("Synonyms: chapel", lines[4]);
            StringAssert.Contains("Nothing yet", POIEditorToolWindow.FoundByText(new List<SearchKeywordSources.Word>()));
        }
    }
}
