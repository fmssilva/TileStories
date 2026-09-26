using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace TileStories.Editor.Tests
{
    // Every taxonomy row has a generated key (what a POI stores, never shown) and a name the developer types and
    // a visitor reads. Driven for real on the REAL window (PoiEditorWindowHost: real clicks on "+ Add" / trash /
    // name cells, real typing, real Ctrl+Z); what a visitor then reads is checked on the real runtime consumers
    // (FilterTrayView.BuildOptions, POISearchIndex). No mocks.
    public class TaxonomyRowIdentityTests
    {
        private const BindingFlags Instance = BindingFlags.NonPublic | BindingFlags.Instance;
        private PoiEditorWindowHost _window;

        private static WallConfigData NewConfig() => new WallConfigData
        {
            pois = new List<POIData>(),
            marker_shape = "circle",
            marker_use_badge = true,
            marker_outline_mode = "uniform",
            effect_defaults = new EffectDefaults(),
            hierarchy_levels = new List<HierarchyLevelEntry>(),
            category_styles = new List<CategoryStyleEntry>(),
            badge_categories = new List<BadgeCategoryEntry>(),
            outline_levels = new List<OutlineLevelEntry>()
        };

        [TearDown]
        public void TearDown() => _window?.Close();

        // ---- the one key generator ----

        // A generated key must never repeat: prefix + (Count + 1) alone collided once a middle row had been deleted
        [Test]
        public void NextFree_NeverRepeatsAnExistingKey()
        {
            var levels = new List<global::TileStories.HierarchyLevelEntry>
            {
                new global::TileStories.HierarchyLevelEntry { key = "level_1" },
                new global::TileStories.HierarchyLevelEntry { key = "level_3" },
            };
            Assert.AreEqual("level_4", TaxonomyRowKeys.NextFree(levels, e => e.key, "level_"), "level_3 is taken, so the next free one");

            levels[1].key = "hub";
            Assert.AreEqual("level_3", TaxonomyRowKeys.NextFree(levels, e => e.key, "level_"));
            Assert.AreEqual("level_1", TaxonomyRowKeys.NextFree(new List<global::TileStories.HierarchyLevelEntry>(), e => e.key, "level_"));
            Assert.AreEqual("level_1", TaxonomyRowKeys.NextFree<global::TileStories.HierarchyLevelEntry>(null, e => e.key, "level_"));

            var badges = new List<BadgeCategoryEntry> { new BadgeCategoryEntry { key = "intact" }, null, new BadgeCategoryEntry { key = " badge_4 " } };
            Assert.AreEqual("badge_5", TaxonomyRowKeys.NextFree(badges, e => e.key, "badge_"),
                "starts at Count + 1 (a null row still counts), and a key is compared trimmed");

            var fields = new List<SearchFieldDefinition> { new() { key = "field_2" }, new() { key = "field_3" } };
            Assert.AreEqual("field_4", TaxonomyRowKeys.NextFree(fields, f => f.key, "field_"));
        }

        // ---- who still points at a row (the delete guard's count), one rule per table ----

        [Test]
        public void CountReferences_EveryTable_CountsOnlyThePoisPointingAtThatKey()
        {
            var pois = new List<POIData>
            {
                new POIData { id = "a", category = "category_1", badge_category = "badge_1", status_level_key = "level_1", hierarchy_level_key = "level_2",
                              search_keyword_fields = new List<POISearchKeywordField> { new() { field_key = "field_1", keywords = new List<string> { "stone" } } } },
                new POIData { id = "b", category = "category_1", badge_category = "badge_2" },
                null,
                new POIData { id = "c", category = "category_2" },
            };
            Assert.AreEqual(2, TaxonomyRowKeys.CountReferences(pois, TaxonomyRowKeys.PoiUsesCategory, "category_1"));
            Assert.AreEqual(1, TaxonomyRowKeys.CountReferences(pois, TaxonomyRowKeys.PoiUsesBadge, "badge_1"));
            Assert.AreEqual(1, TaxonomyRowKeys.CountReferences(pois, TaxonomyRowKeys.PoiUsesStatusLevel, "level_1"));
            Assert.AreEqual(1, TaxonomyRowKeys.CountReferences(pois, TaxonomyRowKeys.PoiUsesHierarchyLevel, "level_2"));
            Assert.AreEqual(1, TaxonomyRowKeys.CountReferences(pois, TaxonomyRowKeys.PoiUsesKeywordField, "field_1"),
                "a keyword field is referenced from inside a POI's keyword lists");
            Assert.AreEqual(0, TaxonomyRowKeys.CountReferences(pois, TaxonomyRowKeys.PoiUsesCategory, "Category_1"), "keys are exact (ordinal)");
            Assert.AreEqual(0, TaxonomyRowKeys.CountReferences(null, TaxonomyRowKeys.PoiUsesCategory, "category_1"));
        }

        // ---- Category: the table that used to be ONE string (identity = visitor name) ----

        [UnityTest]
        public IEnumerator Category_AddButton_GivesAnUnusedKeyAndTheDefaultLabel()
        {
            var config = NewConfig();
            config.category_styles.Add(new CategoryStyleEntry { key = "category_2", label = "Civic", icon_key = "unknown", search_keywords = new List<string>() });
            _window = new PoiEditorWindowHost(config, "_showGlobalMarker");
            yield return _window.WaitForRepaint();

            _window.Click("+ Add category#0");
            yield return _window.WaitForRepaint();

            Assert.AreEqual(2, _window.Config.category_styles.Count, "a real click on '+ Add category' adds one row");
            Assert.AreEqual("category_3", _window.Config.category_styles[1].key, "Count + 1 = category_2 is taken, so the next free key");
            Assert.AreEqual("New Category", _window.Config.category_styles[1].label);
        }

        [UnityTest]
        public IEnumerator CategoryLabel_RealTyping_RenamesWhatVisitorsReadAndEveryPoiKeepsItsCategory()
        {
            var config = NewConfig();
            config.category_styles.Add(new CategoryStyleEntry { key = "religious", label = "religious", icon_key = "unknown", search_keywords = new List<string>() });
            config.category_styles.Add(new CategoryStyleEntry { key = "military", label = "Military", icon_key = "unknown", search_keywords = new List<string>() });
            config.pois.Add(new POIData { id = "poi_1", name = "North tower", category = "religious" });
            config.pois.Add(new POIData { id = "poi_2", name = "South gate", category = "religious" });
            config.pois.Add(new POIData { id = "poi_3", name = "Bastion", category = "military" });
            _window = new PoiEditorWindowHost(config, "_showGlobalMarker");
            yield return _window.WaitForRepaint();

            yield return _window.ReplaceText("Category label#0", "Sacred Art");

            var row = _window.Config.category_styles[0];
            Assert.AreEqual("Sacred Art", row.label, "typing replaces the category's label");
            Assert.AreEqual("religious", row.key, "the key POIs store never changes");
            CollectionAssert.AreEqual(new[] { "religious", "religious", "military" },
                _window.Config.pois.Select(p => p.category).ToList(), "no POI is rewritten by a rename");

            // Every visitor surface reads the new name: the filter chip, the detail card / results subtitle,
            // the search index and the suggested searches
            var chips = FilterTrayView.BuildOptions(_window.Config, new FilterSettings { category_facet = true })
                .Single(g => g.Group.Equals(FacetGroup.Category)).Choices;
            CollectionAssert.Contains(chips, ("religious", "Sacred Art"), "the chip shows the new label and filters by the key");
            var poi1 = _window.Config.pois[0];
            StringAssert.StartsWith("Sacred Art", PoiSubtitle.Of(poi1, _window.Config), "the card chip and results subtitle show the new name");
            var index = new POISearchIndex();
            index.Build(_window.Config);
            CollectionAssert.AreEquivalent(new[] { "poi_1", "poi_2" },
                index.Search("sacred art", SearchOptions.Default).Select(r => r.PoiId).ToList(),
                "searching the new name finds exactly the POIs of this category");
            CollectionAssert.Contains(new SuggestedSearchesManager(10).BuildSuggestions(_window.Config), "Sacred Art",
                "a suggested search is the category's name, never its key");

            // One real Ctrl+Z takes the whole rename back
            yield return _window.ClickAway();
            yield return _window.PressUndo();
            Assert.AreEqual("religious", _window.Config.category_styles[0].label, "one Ctrl+Z undoes the rename");
        }

        // ---- Keyword Fields: the Key column is gone, the label is the field's name ----

        [UnityTest]
        public IEnumerator KeywordFieldLabel_RealTyping_RenamesTheFilterGroupAndEveryPoiKeepsItsKeywords()
        {
            var config = NewConfig();
            config.search_fields = new List<SearchFieldDefinition> { new() { key = "field_1", label = "Material", filterable = true, details = "" } };
            config.pois.Add(new POIData { id = "poi_1", name = "North tower",
                search_keyword_fields = new List<POISearchKeywordField> { new() { field_key = "field_1", keywords = new List<string> { "granite" } } } });
            _window = new PoiEditorWindowHost(config, "_showGlobalSearchFilter");
            // - collapse the sub-foldouts above the table so it is drawn on screen (an off-screen control takes no click)
            foreach (var f in new[] { "_showSearchSelection", "_showSearchSearch", "_showSearchFilters", "_showSearchResults", "_showSearchMinimap", "_showSearchVoice" })
                _window.SetWindowField(f, false);
            yield return _window.WaitForRepaint();

            yield return _window.ReplaceText("Keyword Field label#0", "Stone");

            Assert.AreEqual("Stone", _window.Config.search_fields[0].label);
            Assert.AreEqual("field_1", _window.Config.search_fields[0].key, "the key the POIs' keyword lists are stored under never changes");
            Assert.AreEqual("field_1", _window.Config.pois[0].search_keyword_fields[0].field_key);
            CollectionAssert.AreEqual(new[] { "granite" }, _window.Config.pois[0].search_keyword_fields[0].keywords, "no keyword moved or was lost");

            var group = FilterTrayView.BuildOptions(_window.Config, new FilterSettings()).Single(g => g.Group.Equals(FacetGroup.Field("field_1")));
            Assert.AreEqual("Stone", group.Title, "the visitor's filter group is titled with the new name");
            Assert.IsTrue(SearchKeywordSources.Collect(_window.Config, _window.Config.pois[0]).Any(w => w.Text == "granite" && w.Origin == "Stone"),
                "the editor's 'Found by' names the keyword's field by its new name");
        }

        // ---- the shipped wall: one identity system, every reference resolves ----

        [Test]
        public void ShippedConfig_EveryTaxonomyRowHasAUniqueKeyAndAName_AndEveryPoiReferenceResolves()
        {
            string json = System.IO.File.ReadAllText("Assets/Apps/LivingRoom/config.json");
            Assert.AreEqual(json, System.IO.File.ReadAllText("Assets/StreamingAssets/LivingRoom/config.json"),
                "the StreamingAssets copy (what Play reads) must equal the authoring config");
            var config = JsonUtility.FromJson<WallConfigData>(json);

            void CheckTable<T>(string table, List<T> rows, System.Func<T, string> key, System.Func<T, string> name)
            {
                Assert.IsNotNull(rows, table);
                var keys = rows.Select(key).ToList();
                Assert.IsTrue(keys.All(k => !string.IsNullOrWhiteSpace(k)), table + ": every row has a key");
                CollectionAssert.AllItemsAreUnique(keys, table + ": keys are unique");
                Assert.IsTrue(rows.All(r => !string.IsNullOrWhiteSpace(name(r))), table + ": every row has a name a visitor can read");
            }
            CheckTable("Category", config.category_styles, e => e.key, e => e.label);
            CheckTable("Badge", config.badge_categories, e => e.key, e => e.label);
            CheckTable("Outline Types", config.outline_levels, e => e.key, e => e.label);
            CheckTable("Hierarchy Levels", config.hierarchy_levels, e => e.key, e => e.level_name);
            CheckTable("Keyword Fields", config.search_fields, e => e.key, e => e.label);

            foreach (var poi in config.pois)
            {
                if (!string.IsNullOrEmpty(poi.category))
                    Assert.IsTrue(config.category_styles.Any(e => e.key == poi.category), poi.id + ": category key resolves");
                if (!string.IsNullOrEmpty(poi.badge_category))
                    Assert.IsTrue(config.badge_categories.Any(e => e.key == poi.badge_category), poi.id + ": badge key resolves");
                if (poi.has_status && !string.IsNullOrEmpty(poi.status_level_key))
                    Assert.IsTrue(config.outline_levels.Any(e => e.key == poi.status_level_key), poi.id + ": status level key resolves");
                if (!string.IsNullOrEmpty(poi.hierarchy_level_key))
                    Assert.IsTrue(config.hierarchy_levels.Any(e => e.key == poi.hierarchy_level_key), poi.id + ": hierarchy level key resolves");
                foreach (var list in poi.search_keyword_fields ?? new List<POISearchKeywordField>())
                    Assert.IsTrue(config.search_fields.Any(f => f.key == list.field_key), poi.id + ": keyword field key resolves");
            }

            // The delete guard's count on the real wall: rows in use count their POIs, an unused key counts none
            Assert.Greater(TaxonomyRowKeys.CountReferences(config.pois, TaxonomyRowKeys.PoiUsesCategory, "military"), 0);
            Assert.Greater(TaxonomyRowKeys.CountReferences(config.pois, TaxonomyRowKeys.PoiUsesBadge, "partial_damage"), 0);
            Assert.AreEqual(0, TaxonomyRowKeys.CountReferences(config.pois, TaxonomyRowKeys.PoiUsesBadge, "no_such_badge_key"));
        }

        // ---- Outline Types: delete a middle row, then add ----

        [UnityTest]
        public IEnumerator OutlineTypes_DeleteAMiddleRowThenAdd_EveryRowKeepsItsOwnKey()
        {
            var config = NewConfig();
            config.outline_levels.AddRange(new[]
            {
                new OutlineLevelEntry { key = "level_1", label = "A", line_style = "solid", search_keywords = new List<string>() },
                new OutlineLevelEntry { key = "level_2", label = "B", line_style = "solid", search_keywords = new List<string>() },
                new OutlineLevelEntry { key = "level_3", label = "C", line_style = "solid", search_keywords = new List<string>() },
            });
            config.pois.Add(new POIData { id = "poi_c", has_status = true, status_level_key = "level_3" });
            _window = new PoiEditorWindowHost(config, "_showGlobalOutline");
            yield return _window.WaitForRepaint();

            _window.Click("Outline delete#1");
            yield return _window.WaitForRepaint();
            CollectionAssert.AreEqual(new[] { "A", "C" }, _window.Config.outline_levels.Select(l => l.label).ToList(),
                "a real click on row B's trash removes B (no POI uses it, so nothing is asked)");

            _window.Click("+ Add outline level#0");
            yield return _window.WaitForRepaint();

            var keys = _window.Config.outline_levels.Select(l => l.key).ToList();
            Assert.AreEqual(3, keys.Count, "a real click on '+ Add outline level' adds one row");
            CollectionAssert.AllItemsAreUnique(keys, "the new row must not reuse a key another row still has");
            Assert.AreEqual("C", _window.Config.outline_levels.Single(l => l.key == "level_3").label,
                "the POI on level_3 still points at row C, not at the new row");
            Assert.AreEqual("outline_3", keys[2], "an outline row's key names its own table, never a hierarchy-looking level_N");
            Assert.IsTrue(_window.Unsaved, "both clicks went through the window's mutation scope");
        }

        // ---- Badge Categories: add, then rename what a visitor reads ----

        [UnityTest]
        public IEnumerator BadgeCategories_AddButton_GivesAnUnusedKeyAndTheDefaultLabel()
        {
            var config = NewConfig();
            config.badge_categories.AddRange(new[]
            {
                new BadgeCategoryEntry { key = "intact", label = "Intact", icon_key = "unknown", search_keywords = new List<string>() },
                new BadgeCategoryEntry { key = "badge_3", label = "Other", icon_key = "unknown", search_keywords = new List<string>() },
            });
            _window = new PoiEditorWindowHost(config, "_showGlobalBadge");
            yield return _window.WaitForRepaint();

            _window.Click("+ Add badge category#0");
            yield return _window.WaitForRepaint();

            Assert.AreEqual(3, _window.Config.badge_categories.Count, "a real click on '+ Add badge category' adds one row");
            var added = _window.Config.badge_categories[2];
            Assert.AreEqual("badge_4", added.key, "Count + 1 = badge_3 is taken, so the next free key");
            Assert.AreEqual("New Badge", added.label);
            CollectionAssert.AllItemsAreUnique(_window.Config.badge_categories.Select(b => b.key).ToList());
        }

        [UnityTest]
        public IEnumerator BadgeLabel_RealTyping_RenamesWhatVisitorsReadAndEveryPoiKeepsItsBadge()
        {
            var config = NewConfig();
            config.badge_categories.AddRange(new[]
            {
                new BadgeCategoryEntry { key = "partial_damage", label = "Partial Damage", icon_key = "unknown", search_keywords = new List<string>() },
                new BadgeCategoryEntry { key = "intact", label = "Intact", icon_key = "unknown", search_keywords = new List<string>() },
            });
            config.pois.Add(new POIData { id = "poi_1", name = "North tower", badge_category = "partial_damage" });
            config.pois.Add(new POIData { id = "poi_2", name = "South gate", badge_category = "partial_damage" });
            config.pois.Add(new POIData { id = "poi_3", name = "Chapel", badge_category = "intact" });
            _window = new PoiEditorWindowHost(config, "_showGlobalBadge");
            yield return _window.WaitForRepaint();

            yield return _window.ReplaceText("Badge label#0", "Cracked");

            var row = _window.Config.badge_categories[0];
            Assert.AreEqual("Cracked", row.label, "typing replaces the badge's label");
            Assert.AreEqual("partial_damage", row.key, "the key POIs store never changes");
            CollectionAssert.AreEqual(new[] { "partial_damage", "partial_damage", "intact" },
                _window.Config.pois.Select(p => p.badge_category).ToList(), "no POI is rewritten or orphaned by a rename");
            Assert.IsTrue(_window.Unsaved, "the typing went through the window's mutation scope");

            // What a visitor reads: the Badge filter chip and the search index use the new label
            var groups = FilterTrayView.BuildOptions(_window.Config, new FilterSettings { badge_facet = true });
            var badgeGroup = groups.Single(g => g.Group.Equals(FacetGroup.Badge));
            CollectionAssert.Contains(badgeGroup.Choices, ("partial_damage", "Cracked"), "the chip shows the new label and filters by the key");
            var index = new POISearchIndex();
            index.Build(_window.Config);
            CollectionAssert.AreEquivalent(new[] { "poi_1", "poi_2" },
                index.Search("cracked", SearchOptions.Default).Select(r => r.PoiId).ToList(),
                "searching the new label finds exactly the POIs that carry this badge");

            // One real Ctrl+Z takes the whole rename back (grouped undo), and the key is never touched
            yield return _window.ClickAway();
            yield return _window.PressUndo();
            Assert.AreEqual("Partial Damage", _window.Config.badge_categories[0].label, "one Ctrl+Z undoes the rename");
            Assert.AreEqual("partial_damage", _window.Config.badge_categories[0].key);
        }

        // The framework finds the unknown badge row by its key; renaming the row's label must not lose it
        [Test]
        public void StatusUnknown_AfterTheUnknownBadgeLabelWasRenamed_StillSelectsTheUnknownBadge()
        {
            var config = NewConfig();
            config.badge_categories.AddRange(DefaultBadgeCategories.Create());
            config.outline_levels.AddRange(DefaultOutlineLevels.Create());
            config.badge_categories.Single(b => b.key == "unknown_damage").label = "Fate unknown";
            var poi = new POIData { id = "poi_1", has_status = true, badge_category = "intact", status_level_key = "intact" };
            config.pois.Add(poi);

            var editor = ScriptableObject.CreateInstance<POIEditorToolWindow>();
            try
            {
                typeof(POIEditorToolWindow).GetField("_config", Instance).SetValue(editor, config);
                typeof(POIEditorToolWindow).GetMethod("ApplyUnknownStatusDefaults", Instance).Invoke(editor, new object[] { poi });
            }
            finally { Object.DestroyImmediate(editor); }

            Assert.AreEqual("unknown_damage", poi.badge_category, "the unknown badge is still found (by key) after its label changed");
            Assert.AreEqual("unknown", poi.status_level_key);
        }

        // ---- a hand-edited file: keys repaired ONCE on load, never while drawing ----

        // One table of each kind broken the two ways a hand edit can break it: a blank key, and a key an earlier
        // row already has. The POIs point at the shared keys.
        private static WallConfigData HandEditedConfig()
        {
            var config = NewConfig();
            config.category_styles.AddRange(new[]
            {
                new CategoryStyleEntry { key = "religious", label = "Religious" },
                new CategoryStyleEntry { key = "", label = "Civic" },
                new CategoryStyleEntry { key = "religious", label = "Sacred" },
            });
            config.badge_categories.AddRange(new[]
            {
                new BadgeCategoryEntry { key = "intact", label = "Intact" },
                new BadgeCategoryEntry { key = "intact", label = "Whole" },
            });
            config.outline_levels.Add(new OutlineLevelEntry { key = "  ", label = "Ruined", line_style = "solid" });
            config.hierarchy_levels.AddRange(new[]
            {
                new HierarchyLevelEntry { key = "level_1", level_name = "Hub", size_cm = 10f },
                new HierarchyLevelEntry { key = null, level_name = "Detail", size_cm = 5f },
            });
            config.search_fields = new List<SearchFieldDefinition>
            {
                new() { key = "field_1", label = "Material" },
                new() { key = "field_1", label = "Era" },
            };
            config.pois.Add(new POIData { id = "poi_1", name = "North tower", category = "religious", badge_category = "intact",
                hierarchy_level_key = "level_1",
                search_keyword_fields = new List<POISearchKeywordField> { new() { field_key = "field_1", keywords = new List<string> { "stone" } } } });
            return config;
        }

        [Test]
        public void RepairKeys_GivesEveryBlankOrRepeatedKeyAFreshOne_TheFirstRowKeepsItsKey_AndNoPoiChanges()
        {
            var config = HandEditedConfig();
            string poiBefore = JsonUtility.ToJson(config.pois[0]);

            int repaired = TaxonomyRowKeys.RepairKeys(config);

            Assert.AreEqual(6, repaired, "blank category + repeated category + repeated badge + blank outline + blank level + repeated field");
            CollectionAssert.AreEqual(new[] { "religious", "category_4", "category_5" }, config.category_styles.Select(e => e.key).ToList(),
                "the first 'religious' keeps its key; each broken row gets its table's next free key (NextFree starts at " +
                "Count + 1 = 4, and category_4 was just given out, so the second repair takes category_5)");
            CollectionAssert.AllItemsAreUnique(config.category_styles.Select(e => e.key).ToList());
            CollectionAssert.AreEqual(new[] { "intact", "badge_3" }, config.badge_categories.Select(e => e.key).ToList());
            StringAssert.StartsWith(TaxonomyRowKeys.OutlinePrefix, config.outline_levels[0].key, "each table uses its own prefix");
            CollectionAssert.AreEqual(new[] { "level_1", "level_3" }, config.hierarchy_levels.Select(e => e.key).ToList());
            CollectionAssert.AreEqual(new[] { "field_1", "field_3" }, config.search_fields.Select(e => e.key).ToList());

            Assert.AreEqual(poiBefore, JsonUtility.ToJson(config.pois[0]), "no POI is rewritten: it keeps pointing at the FIRST row, as the runtime already resolved it");
            Assert.AreEqual("Religious", TaxonomyNames.Category(config, "religious"), "so the POI still shows what it showed before");

            Assert.AreEqual(0, TaxonomyRowKeys.RepairKeys(config), "a repaired config needs nothing the second time");
            Assert.AreEqual(0, TaxonomyRowKeys.RepairKeys(null));
        }

        [Test]
        public void LoadConfig_OfAHandEditedFile_RepairsTheKeysBeforeTheUndoBaseline_AndMarksItUnsaved()
        {
            string dir = System.IO.Path.GetFullPath("Temp/__TaxonomyRowIdentityTests");
            string path = System.IO.Path.Combine(dir, "config.json");
            System.IO.Directory.CreateDirectory(dir);
            System.IO.File.WriteAllText(path, JsonUtility.ToJson(HandEditedConfig(), true));
            var editor = ScriptableObject.CreateInstance<POIEditorToolWindow>();
            try
            {
                typeof(POIEditorToolWindow).GetField("_configPath", Instance).SetValue(editor, path);
                LogAssert.Expect(LogType.Log, new System.Text.RegularExpressions.Regex(@"\[POIEditor\] gave 6 taxonomy row\(s\)"));
                typeof(POIEditorToolWindow).GetMethod("LoadConfig", Instance).Invoke(editor, null);

                var config = (WallConfigData)typeof(POIEditorToolWindow).GetField("_config", Instance).GetValue(editor);
                foreach (var keys in new[]
                         {
                             config.category_styles.Select(e => e.key).ToList(), config.badge_categories.Select(e => e.key).ToList(),
                             config.outline_levels.Select(e => e.key).ToList(), config.hierarchy_levels.Select(e => e.key).ToList(),
                             config.search_fields.Select(e => e.key).ToList(),
                         })
                {
                    Assert.IsTrue(keys.All(k => !string.IsNullOrWhiteSpace(k)), "every row of the loaded config has a key");
                    CollectionAssert.AllItemsAreUnique(keys);
                }
                Assert.IsTrue((bool)typeof(POIEditorToolWindow).GetField("_hasUnsavedChanges", Instance).GetValue(editor),
                    "the repair changed the config, so Save All to JSON is lit and writes it");
                Assert.AreEqual(0, (int)typeof(POIEditorToolWindow).GetField("_configHistoryIndex", Instance).GetValue(editor),
                    "the repair is part of what was loaded, not an undo step (Ctrl+Z cannot bring a broken key back)");
                StringAssert.Contains("\"\"", System.IO.File.ReadAllText(path), "loading never writes the file: the developer decides with Save");
            }
            finally
            {
                Object.DestroyImmediate(editor);
                System.IO.Directory.Delete(dir, true);
            }
        }

        // Drawing a table used to give a blank key a new one (the Hierarchy and Outline tables only), so merely
        // OPENING a section wrote the config and recorded an undo step. Drawing must never write.
        [UnityTest]
        public IEnumerator DrawingTheHierarchyAndOutlineTables_NeverWritesTheConfig()
        {
            var config = NewConfig();
            config.hierarchy_levels.Add(new HierarchyLevelEntry { key = "level_1", level_name = "Hub", priority = 1, size_cm = 10f, search_keywords = new List<string>() });
            config.outline_levels.Add(new OutlineLevelEntry { key = "outline_1", label = "Ruined", line_style = "solid", search_keywords = new List<string>() });
            _window = new PoiEditorWindowHost(config, "_showGlobalHierarchy");
            _window.SetWindowField("_showGlobalOutline", true);
            yield return _window.WaitForRepaint();
            yield return _window.WaitForRepaint();
            Assert.IsTrue(_window.RectOf("+ Add outline level#0").width > 0f, "precondition: the Outline table really drew");

            // - every other cell is now in the window's own form; only the keys go blank, as a hand edit would leave them
            _window.Config.hierarchy_levels[0].key = "";
            _window.Config.outline_levels[0].key = "";
            _window.SetWindowField("_hasUnsavedChanges", false);
            string before = JsonUtility.ToJson(_window.Config, true);

            yield return _window.WaitForRepaint();
            yield return _window.WaitForRepaint();

            Assert.AreEqual("", _window.Config.hierarchy_levels[0].key, "drawing the Hierarchy Levels table left the row alone");
            Assert.AreEqual("", _window.Config.outline_levels[0].key, "drawing the Outline Types table left the row alone");
            string after = JsonUtility.ToJson(_window.Config, true);
            Assert.AreEqual(before, after, "drawing changed the config at: " + FirstDifference(before, after));
            Assert.IsFalse(_window.Unsaved, "nothing was edited, so nothing is unsaved");
        }

        // ---- a name a visitor cannot use: warned on Save, by table and row ----

        // The developer types a second category's label to match the first (case and spaces aside), clears a
        // third, and presses Save: the save-time report names both rows by their table and position, never by key.
        [UnityTest]
        public IEnumerator TypingADuplicateAndABlankCategoryLabel_ThenSave_ReportsBothRowsByPosition()
        {
            string dir = System.IO.Path.GetFullPath("Temp/__TaxonomyRowIdentityTests_Save");
            System.IO.Directory.CreateDirectory(dir);
            var config = NewConfig();
            config.category_styles.AddRange(new[]
            {
                new CategoryStyleEntry { key = "category_1", label = "Military", icon_key = "unknown", search_keywords = new List<string>() },
                new CategoryStyleEntry { key = "category_2", label = "Civic", icon_key = "unknown", search_keywords = new List<string>() },
                new CategoryStyleEntry { key = "category_3", label = "Religious", icon_key = "unknown", search_keywords = new List<string>() },
            });
            config.badge_categories.AddRange(DefaultBadgeCategories.Create());
            config.outline_levels.AddRange(DefaultOutlineLevels.Create());
            _window = new PoiEditorWindowHost(config, "_showGlobalMarker");
            _window.SetWindowField("_configPath", System.IO.Path.Combine(dir, "config.json"));
            yield return _window.WaitForRepaint();

            yield return _window.ReplaceText("Category label#1", " military");
            // - clear the third: click in, Ctrl+A (ReplaceText with no text), then a real Backspace
            yield return _window.ReplaceText("Category label#2", "");
            _window.Send(Event.KeyboardEvent("backspace"));
            yield return _window.WaitForRepaint();
            yield return _window.ClickAway();
            Assert.AreEqual(" military", _window.Config.category_styles[1].label, "precondition: the real typing reached the config");
            Assert.AreEqual("", _window.Config.category_styles[2].label);

            try
            {
                typeof(POIEditorToolWindow).GetMethod("SaveAllToJson", Instance).Invoke(_window.Editor, null);

                Assert.AreEqual("Config validation issues (before save)", EditorNotice.PendingTitle, "Save reports what needs attention");
                string message = EditorNotice.PendingMessage;
                StringAssert.Contains("Marker > Category Symbols, rows 1 and 2", message);
                StringAssert.Contains("Two rows share one Category label", message);
                StringAssert.Contains("Marker > Category Symbols, row 3", message);
                StringAssert.Contains("has no Category label", message);
                StringAssert.DoesNotContain("category_", message, "never the generated key");
                Assert.IsTrue(System.IO.File.Exists(System.IO.Path.Combine(dir, "config.json")), "a warning never blocks the save");
            }
            finally { System.IO.Directory.Delete(dir, true); }
        }

        // The first line two JSON texts differ on, so a failure says WHICH field drawing wrote
        private static string FirstDifference(string a, string b)
        {
            string[] x = a.Split('\n'), y = b.Split('\n');
            for (int i = 0; i < Mathf.Min(x.Length, y.Length); i++)
                if (x[i] != y[i]) return x[i].Trim() + "  ->  " + y[i].Trim();
            return x.Length == y.Length ? "(none)" : "(length)";
        }
    }
}
