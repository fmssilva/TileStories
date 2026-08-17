using System.Reflection;
using UnityEditor;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // Tier 0 EditMode tests for the Block 5 Phase 5.1 Search & Filter authoring
    // foldout: JSON round-trip of the new fields and enum-string validation.
    // Mirrors SaveConfig via JsonUtility, and the runtime WallConfigLoader
    // deserialize path (spec _2.6 section 3 / Block 5).
    public class POIAuthoringToolSearchRoundTripTests
    {
        [Test]
        public void RoundTrip_SearchConfigFields_ThroughJsonUtility()
        {
            var config = new WallConfigData
            {
                wall_id = "search_test_wall",
                search_mode = "dynamic",
                search_index_strategy = "weighted_fields",
                weight_name = 4f,
                weight_custom_field = 3f,
                weight_derived_label = 2f,
                weight_others = 1f,
                no_results_message = "No matches for \"{query}\" -- broadened.",
                recent_search_count = 8,
                show_suggested_categories = false,
                suggested_source = "recent_first",
                minimap_enabled = false,
                minimap_visibility = "always",
                minimap_icon_style = "dots_only",
                default_result_view = "minimap",
                voice_search_enabled = true,
                voice_search_match_mode = "any",
                voice_activity_indicator_style = "listen_bar"
            };

            // Mirrors authoring SaveConfig + runtime WallConfigLoader.
            string json = JsonUtility.ToJson(config, true);
            var loaded = JsonUtility.FromJson<WallConfigData>(json);

            // String-field (dropdowns)
            Assert.AreEqual("dynamic", loaded.search_mode);
            Assert.AreEqual("weighted_fields", loaded.search_index_strategy);
            Assert.AreEqual("recent_first", loaded.suggested_source);
            Assert.AreEqual("always", loaded.minimap_visibility);
            Assert.AreEqual("dots_only", loaded.minimap_icon_style);
            Assert.AreEqual("minimap", loaded.default_result_view);
            Assert.AreEqual("any", loaded.voice_search_match_mode);
            Assert.AreEqual("listen_bar", loaded.voice_activity_indicator_style);
            Assert.AreEqual("No matches for \"{query}\" -- broadened.", loaded.no_results_message);

            // Scalar fields
            Assert.AreEqual(4f, loaded.weight_name);
            Assert.AreEqual(3f, loaded.weight_custom_field);
            Assert.AreEqual(2f, loaded.weight_derived_label);
            Assert.AreEqual(1f, loaded.weight_others);

            // Int fields
            Assert.AreEqual(8, loaded.recent_search_count);

            // Toggle fields
            Assert.IsFalse(loaded.show_suggested_categories);
            Assert.IsFalse(loaded.minimap_enabled);
            Assert.IsTrue(loaded.voice_search_enabled);
        }

        [Test]
        public void RoundTrip_DefaultValues_Persist()
        {
            var config = new WallConfigData { wall_id = "defaults_wall" };

            string json = JsonUtility.ToJson(config, true);
            var loaded = JsonUtility.FromJson<WallConfigData>(json);

            Assert.AreEqual("keyword_ranked", loaded.search_index_strategy);
            Assert.AreEqual("category_distribution", loaded.suggested_source);
            Assert.AreEqual(3f, loaded.weight_name);
            Assert.AreEqual(2f, loaded.weight_custom_field);
            Assert.AreEqual(2f, loaded.weight_derived_label);
            Assert.AreEqual(1f, loaded.weight_others);
            Assert.IsTrue(loaded.selection_highlight_enabled);
            Assert.IsTrue(loaded.zoom_on_select_enabled);
        }

        [Test]
        public void RoundTrip_PoiSearchKeywords_SurvivesJsonUtility()
        {
            var config = new WallConfigData
            {
                wall_id = "poi_keywords_wall",
                pois = new System.Collections.Generic.List<POIData>
                {
                    new POIData
                    {
                        id = "poi_1",
                        name = "Cathedral",
                        search_keywords = new System.Collections.Generic.List<string> { "igreja", "santuário", "heritage" }
                    }
                }
            };

            string json = JsonUtility.ToJson(config, true);
            var loaded = JsonUtility.FromJson<WallConfigData>(json);

            Assert.AreEqual(1, loaded.pois.Count);
            Assert.AreEqual("poi_1", loaded.pois[0].id);
            Assert.AreEqual(3, loaded.pois[0].search_keywords.Count);
            Assert.AreEqual("igreja", loaded.pois[0].search_keywords[0]);
            Assert.AreEqual("santuário", loaded.pois[0].search_keywords[1]);
                        Assert.AreEqual("heritage", loaded.pois[0].search_keywords[2]);
        }

        [Test]
        public void FoldoutSectionMethods_AndState_Exist()
        {
            var t = typeof(POIAuthoringToolWindow);

            Assert.IsNotNull(
                t.GetMethod("DrawGlobalSearchFilterSection",
                    BindingFlags.NonPublic | BindingFlags.Instance),
                "DrawGlobalSearchFilterSection must be wired on the authoring window");

            Assert.IsNotNull(
                t.GetMethod("DrawPoiSearchKeywordsField",
                    BindingFlags.NonPublic | BindingFlags.Instance),
                "DrawPoiSearchKeywordsField must be wired on the authoring window");

            Assert.IsNotNull(
                t.GetField("_showGlobalSearchFilter",
                    BindingFlags.NonPublic | BindingFlags.Instance),
                "_showGlobalSearchFilter state field must exist");

            Assert.IsNotNull(
                t.GetField("_searchIndexStrategy",
                    BindingFlags.NonPublic | BindingFlags.Instance),
                "_searchIndexStrategy state field must exist");
        }

        [Test]
        public void ValidateSearchEnumFields_WarnsOnUnknownSearchMode()
        {
            var window = EditorWindow.CreateInstance<POIAuthoringToolWindow>();
            var config = new WallConfigData
            {
                wall_id = "validate_wall",
                search_mode = "bogus_mode"
            };

            var configField = typeof(POIAuthoringToolWindow)
                .GetField("_config", BindingFlags.NonPublic | BindingFlags.Instance);
            configField.SetValue(window, config);

            var validateMethod = typeof(POIAuthoringToolWindow)
                .GetMethod("ValidateSearchEnumFields",
                    BindingFlags.NonPublic | BindingFlags.Instance);
            var issues = (System.Collections.Generic.List<EditorAlertItem>)validateMethod.Invoke(window, null);

            Assert.IsTrue(issues.Count > 0, "Should warn on unknown search_mode");
            Assert.IsTrue(issues[0].problem.Contains("search_mode"));
        }

        [Test]
        public void ValidateSearchEnumFields_CleansKnownValues()
        {
            var window = EditorWindow.CreateInstance<POIAuthoringToolWindow>();
            var config = new WallConfigData
            {
                wall_id = "validate_wall",
                search_mode = "dynamic",
                search_index_strategy = "keyword_ranked",
                voice_search_match_mode = "all",
                voice_activity_indicator_style = "mic_text",
                suggested_source = "category_distribution"
            };

            var configField = typeof(POIAuthoringToolWindow)
                .GetField("_config", BindingFlags.NonPublic | BindingFlags.Instance);
            configField.SetValue(window, config);

            var validateMethod = typeof(POIAuthoringToolWindow)
                .GetMethod("ValidateSearchEnumFields",
                    BindingFlags.NonPublic | BindingFlags.Instance);
            var issues = (System.Collections.Generic.List<EditorAlertItem>)validateMethod.Invoke(window, null);

            Assert.IsEmpty(issues, "Should be clean with all known values");
        }

        [Test]
        public void ValidateSearchEnumFields_WarnsOnInertSearchMode()
        {
            var window = EditorWindow.CreateInstance<POIAuthoringToolWindow>();
            var config = new WallConfigData
            {
                wall_id = "validate_wall",
                search_mode = "faceted"
            };

            var configField = typeof(POIAuthoringToolWindow)
                .GetField("_config", BindingFlags.NonPublic | BindingFlags.Instance);
            configField.SetValue(window, config);

            var validateMethod = typeof(POIAuthoringToolWindow)
                .GetMethod("ValidateSearchEnumFields",
                    BindingFlags.NonPublic | BindingFlags.Instance);
            var issues = (System.Collections.Generic.List<EditorAlertItem>)validateMethod.Invoke(window, null);

            Assert.IsTrue(issues.Count > 0, "Should warn on inert search_mode");
            Assert.IsTrue(issues[0].problem.Contains("inert"));
        }

        [Test]
        public void SuggestedSourceApplier_AppliesSourceFromConfig()
        {
            var config = new WallConfigData { suggested_source = "recent_first" };
            var manager = new SuggestedSearchesManager(topN: 5);

            SuggestedSourceApplier.Apply(config, manager);

            Assert.AreEqual(SuggestedSearchesManager.SuggestedSource.RecentFirst, manager.Source);
        }

        [Test]
        public void SuggestedSourceApplier_NullSafe()
        {
            var manager = new SuggestedSearchesManager();

            SuggestedSourceApplier.Apply(null, manager);
            Assert.AreEqual(SuggestedSearchesManager.SuggestedSource.CategoryDistribution, manager.Source);

            SuggestedSourceApplier.Apply(new WallConfigData(), null);
        }

        [Test]
        public void SuggestedSourceApplier_EmptyOrUnknownFallsBackToCategory()
        {
            var manager = new SuggestedSearchesManager();

            SuggestedSourceApplier.Apply(new WallConfigData { suggested_source = "" }, manager);
            Assert.AreEqual(SuggestedSearchesManager.SuggestedSource.CategoryDistribution, manager.Source);

            SuggestedSourceApplier.Apply(new WallConfigData { suggested_source = "bogus" }, manager);
            Assert.AreEqual(SuggestedSearchesManager.SuggestedSource.CategoryDistribution, manager.Source);
        }

        [TearDown]
        public void TearDown()
        {
            var windows = Resources.FindObjectsOfTypeAll<POIAuthoringToolWindow>();
            foreach (var w in windows)
                Object.DestroyImmediate(w);
        }
    }
}