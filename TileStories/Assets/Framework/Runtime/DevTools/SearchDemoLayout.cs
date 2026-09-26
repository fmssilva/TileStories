using System.Collections.Generic;
using UnityEngine;

namespace TileStories
{
    // Pure placement and content of the dev-only search & filter demo (SearchDemoSettings, spec _2.6
    // section 8): a flat demo wall facing the camera, local +z = away from the camera.
    //   - one column per wall category, markers_per_category POIs each; hierarchy levels, badges, outline
    //     types and the demo "Material" keyword field cycle over the whole grid, so every filter value --
    //     the wall's own taxonomy rows and a keyword-field filter -- has POIs to act on
    //   - (test_cases) a column of named test POIs, a dense clump at the centre, plus the TEST CASES: each a query, the
    //     POI it must find, the Editor control it depends on and the rule saying when it is found
    //     (SearchDemoCheck runs them against the real index for the Live Search Readout and the tests)
    // Everything uses the wall's own taxonomy, so filtering the demo filters by the wall's real rows.
    public static class SearchDemoLayout
    {
        public const string IdPrefix = "search_demo_";
        public const int ClumpCount = 5;
        public const float ClumpSpacingM = 0.015f;

        // The demo's own Keyword Field, marked Filter: the Filters panel gets a "Material" group on the demo
        public const string MaterialFieldKey = "demo_material";
        public static readonly string[] Materials = { "granite", "marble", "limestone" };

        // One demo POI and where it stands on the demo wall
        public struct Entry
        {
            public POIData Poi;
            public Vector3 LocalPosition;
        }

        // One search behaviour: typing Query finds PoiId exactly when Expected(settings) says so
        public sealed class TestCase
        {
            public string Query;
            public string PoiId;
            public string PoiName;
            // What it shows, and the Editor control it depends on ("" = found whatever the settings)
            public string Note;
            public string DependsOn;
            public System.Func<SearchSettings, bool> Expected;
        }

        // Ids of the named test POIs
        public const string AccentId = IdPrefix + "accent";
        public const string TypoId = IdPrefix + "typo";
        public const string TwoTyposId = IdPrefix + "two_typos";
        public const string SynonymId = IdPrefix + "synonym";
        public const string FieldId = IdPrefix + "field";
        public const string SummaryId = IdPrefix + "summary";
        public const string ClumpIdPrefix = IdPrefix + "clump_";
        public const int TestPoiCount = 6;

        // The demo's synonym group: "church" finds the POI tagged "chapel" and the reverse
        public static SynonymGroup DemoSynonymGroup() =>
            new SynonymGroup { key = "church", synonyms = new List<string> { "chapel", "temple" } };

        // The demo's Keyword Field definition (filterable)
        public static SearchFieldDefinition DemoMaterialField() =>
            new SearchFieldDefinition { key = MaterialFieldKey, label = "Material", filterable = true, details = "" };

        // The vocabulary the demo searches with on top of the wall's own: its Material field always, its
        // synonym group with Test Cases (the index WallSession builds while the demo is on)
        public static void AddDemoVocabulary(SearchDemoSettings settings, List<SearchFieldDefinition> fields, List<SynonymGroup> synonyms)
        {
            fields.Add(DemoMaterialField());
            if (settings == null || settings.test_cases)
                synonyms.Add(DemoSynonymGroup());
        }

        // Every test case, in the order the readout lists them
        public static List<TestCase> TestCases() => new()
        {
            Case("cafe alvaro", AccentId, "Café Álvaro", "accents and case are ignored", "", s => true),
            Case("lantren", TypoId, "Lantern Tower", "one typo (two letters swapped)", "Typo Tolerance", s => Typos(s) >= 1),
            Case("obsrevatroy", TwoTyposId, "Observatory Hill", "two typos in a long word", "Typo Tolerance", s => Typos(s) >= 2),
            Case("lant", TypoId, "Lantern Tower", "an unfinished name word", "Partial Words",
                s => s.prefix_matching != SelectFilterSearchOptions.PrefixOff),
            Case("grani", FieldId, "Stone Gate", "an unfinished keyword", "Partial Words",
                s => s.prefix_matching == SelectFilterSearchOptions.PrefixAllFields),
            Case("stone lantern", FieldId, "Stone Gate", "two words, one point has only one", "Match Words",
                s => s.match_mode == SelectFilterSearchOptions.MatchAny),
            Case("church", SynonymId, "Old Hall", "a synonym (demo group church / chapel / temple)", "", s => true),
            Case("granite", FieldId, "Stone Gate", "a keyword of the Material keyword field", "", s => true),
            Case("spring", SummaryId, "Square Well", "a word of the summary", "", s => true),
            Case("crowd", ClumpIdPrefix + "0", "Crowd 1", "the five-point clump (Zoom on Select, clusters)", "", s => true),
        };

        private static TestCase Case(string query, string id, string name, string note, string dependsOn,
            System.Func<SearchSettings, bool> expected) =>
            new TestCase { Query = query, PoiId = id, PoiName = name, Note = note, DependsOn = dependsOn, Expected = expected };

        private static int Typos(SearchSettings s) => Mathf.Clamp(s?.typo_tolerance ?? 0, 0, SearchSettings.TypoToleranceMax);

        public static int MarkersPerCategory(SearchDemoSettings s) =>
            Mathf.Clamp(s?.markers_per_category ?? 0, 1, SearchDemoSettings.MaxMarkersPerCategory);

        public static float DistanceMetres(SearchDemoSettings s) =>
            Mathf.Clamp(s?.distance_m ?? 3.5f, SearchDemoSettings.MinDistanceM, SearchDemoSettings.MaxDistanceM);

        public static float SpacingMetres(SearchDemoSettings s) =>
            Mathf.Clamp(s?.spacing_cm ?? 40f, SearchDemoSettings.MinSpacingCm, SearchDemoSettings.MaxSpacingCm) / 100f;

        // Build the demo's POIs and positions from the wall's taxonomy
        public static List<Entry> Build(SearchDemoSettings settings, WallConfigData config)
        {
            var entries = new List<Entry>();
            var categories = new List<string>();
            if (config?.category_styles != null)
                foreach (var c in config.category_styles)
                    if (c != null && !string.IsNullOrEmpty(c.category)) categories.Add(c.category);
            if (categories.Count == 0)
                categories.Add("demo");

            var levels = LevelKeysByPriority(config?.hierarchy_levels);
            var badges = Keys(config?.badge_categories, b => b.key);
            var outlines = Keys(config?.outline_levels, o => o.key);

            int perCategory = MarkersPerCategory(settings);
            bool testCases = settings == null || settings.test_cases;
            int columns = categories.Count + (testCases ? 1 : 0);
            float spacing = SpacingMetres(settings);
            float distance = DistanceMetres(settings);
            int rows = Mathf.Max(perCategory, testCases ? TestPoiCount : 0);

            // - the grid is centred on the camera's line of sight
            Vector3 CellPosition(int col, int row) => new Vector3(
                (col - (columns - 1) * 0.5f) * spacing,
                ((rows - 1) * 0.5f - row) * spacing,
                distance);

            int n = 0;
            for (int col = 0; col < categories.Count; col++)
            {
                for (int row = 0; row < perCategory; row++, n++)
                {
                    var poi = NewPoi($"{IdPrefix}{n}", $"{Pretty(categories[col])} {row + 1}", categories[col],
                        Pick(levels, n), Pick(badges, n), Pick(outlines, n));
                    SetMaterial(poi, Materials[n % Materials.Length]);
                    entries.Add(new Entry { Poi = poi, LocalPosition = CellPosition(col, row) });
                }
            }

            if (!testCases)
                return entries;

            int testCol = categories.Count;
            AddTestPois(entries, categories, levels, badges, outlines, p => CellPosition(testCol, p));

            // - the clump: ClumpCount POIs a finger-width apart on the camera's line of sight, in the gap between
            //   the cells nearest it (half a cell off any cell centre), so zoom-on-select can zoom in fully
            //   without pushing it off screen
            Vector3 clumpCentre = new Vector3(columns % 2 == 1 ? spacing * 0.5f : 0f, rows % 2 == 1 ? spacing * 0.5f : 0f, distance);
            for (int i = 0; i < ClumpCount; i++)
            {
                float angle = i * Mathf.PI * 2f / ClumpCount;
                var poi = NewPoi($"{ClumpIdPrefix}{i}", $"Crowd {i + 1}", categories[i % categories.Count],
                    Pick(levels, i), Pick(badges, i), Pick(outlines, i));
                entries.Add(new Entry
                {
                    Poi = poi,
                    LocalPosition = clumpCentre + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * ClumpSpacingM,
                });
            }
            return entries;
        }

        // The named test POIs the test cases search for (one column)
        private static void AddTestPois(List<Entry> entries, List<string> categories, List<string> levels,
            List<string> badges, List<string> outlines, System.Func<int, Vector3> position)
        {
            string cat(int i) => categories[i % categories.Count];
            POIData Poi(string id, string name, int i) => NewPoi(id, name, cat(i), Pick(levels, i), Pick(badges, i), Pick(outlines, i));

            var accent = Poi(AccentId, "Café Álvaro", 0);
            var typo = Poi(TypoId, "Lantern Tower", 1);
            var twoTypos = Poi(TwoTyposId, "Observatory Hill", 2);
            var synonym = Poi(SynonymId, "Old Hall", 3);
            synonym.search_keywords.Add("chapel");
            var field = Poi(FieldId, "Stone Gate", 4);
            SetMaterial(field, "granite");
            var summary = Poi(SummaryId, "Square Well", 5);
            summary.summary = "a hidden spring under the square";

            var pois = new[] { accent, typo, twoTypos, synonym, field, summary };
            for (int i = 0; i < pois.Length; i++)
                entries.Add(new Entry { Poi = pois[i], LocalPosition = position(i) });
        }

        private static void SetMaterial(POIData poi, string material) =>
            poi.search_keyword_fields.Add(new POISearchKeywordField { field_key = MaterialFieldKey, keywords = new List<string> { material } });

        private static POIData NewPoi(string id, string name, string category, string level, string badge, string outline) =>
            new POIData
            {
                id = id,
                name = name,
                category = category,
                hierarchy_level_key = level,
                badge_category = badge,
                has_status = !string.IsNullOrEmpty(outline),
                status_level_key = outline,
            };

        // Level keys ordered by priority (the Hierarchy table's own order)
        private static List<string> LevelKeysByPriority(List<HierarchyLevelEntry> levels)
        {
            var sorted = new List<HierarchyLevelEntry>();
            if (levels != null)
                foreach (var l in levels)
                    if (l != null && !string.IsNullOrEmpty(l.key)) sorted.Add(l);
            sorted.Sort((a, b) => a.priority.CompareTo(b.priority));
            return sorted.ConvertAll(l => l.key);
        }

        private static List<string> Keys<T>(List<T> rows, System.Func<T, string> key) where T : class
        {
            var keys = new List<string>();
            if (rows != null)
                foreach (var r in rows)
                    if (r != null && !string.IsNullOrEmpty(key(r))) keys.Add(key(r));
            return keys;
        }

        private static string Pick(List<string> keys, int i) => keys.Count == 0 ? "" : keys[i % keys.Count];

        // "infrastructure" -> "Infrastructure" (the category key reads as a name)
        private static string Pretty(string category) =>
            string.IsNullOrEmpty(category) ? category : char.ToUpperInvariant(category[0]) + category.Substring(1);
    }
}
