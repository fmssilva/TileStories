using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;

namespace TileStories.Tests
{
    // Tier-0 tests for POIEditorToolWindow.ValidateMarkerTaxonomyReferences (_2.2.1/2/3): a POI's
    // category / badge_category / status_level_key / custom_symbol_key must resolve to a real
    // taxonomy row, or the editor silently degrades at runtime with no warning. Pure data, no scene.
    public class MarkerTaxonomyReferenceValidationTests
    {
        private static TileStories.Editor.POIEditorToolWindow CreateWindowWithConfig(WallConfigData config)
        {
            var window = EditorWindow.GetWindow<TileStories.Editor.POIEditorToolWindow>();
            var field = typeof(TileStories.Editor.POIEditorToolWindow).GetField("_config", BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(window, config);
            return window;
        }

        private static List<TileStories.Editor.EditorAlertItem> Validate(WallConfigData config)
        {
            var window = CreateWindowWithConfig(config);
            var method = typeof(TileStories.Editor.POIEditorToolWindow).GetMethod("ValidateMarkerTaxonomyReferences", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(method, "ValidateMarkerTaxonomyReferences not found on POIEditorToolWindow");
            return (List<TileStories.Editor.EditorAlertItem>)method.Invoke(window, null);
        }

        private static WallConfigData BaseConfig()
        {
            return new WallConfigData
            {
                category_styles = new List<CategoryStyleEntry> { new CategoryStyleEntry { category = "religious" } },
                badge_categories = new List<BadgeCategoryEntry> { new BadgeCategoryEntry { key = "intact" } },
                outline_levels = new List<OutlineLevelEntry> { new OutlineLevelEntry { key = "partial_damage" } },
                pois = new List<POIData>(),
            };
        }

        [Test]
        public void RealCategory_ProducesNoIssue()
        {
            var config = BaseConfig();
            config.pois.Add(new POIData { id = "a", category = "religious" });
            Assert.IsEmpty(Validate(config));
        }

        [Test]
        public void UnknownCategory_ProducesOneIssue()
        {
            var config = BaseConfig();
            config.pois.Add(new POIData { id = "a", category = "ghost_category" });
            var issues = Validate(config);
            Assert.AreEqual(1, issues.Count);
            StringAssert.Contains("Category", issues[0].problem);
        }

        [Test]
        public void UnknownBadgeCategory_ProducesOneIssue()
        {
            var config = BaseConfig();
            config.pois.Add(new POIData { id = "a", category = "religious", badge_category = "ghost_badge" });
            var issues = Validate(config);
            Assert.AreEqual(1, issues.Count);
            StringAssert.Contains("Badge", issues[0].problem);
        }

        [Test]
        public void UnknownStatusLevelKey_KnownStatus_ProducesOneIssue()
        {
            var config = BaseConfig();
            config.pois.Add(new POIData { id = "a", category = "religious", has_status = true, status_unknown = false, status_level_key = "ghost_level" });
            var issues = Validate(config);
            Assert.AreEqual(1, issues.Count);
            StringAssert.Contains("Status level", issues[0].problem);
        }

        [Test]
        public void UnknownStatusLevelKey_WhenStatusUnknown_ProducesNoIssue()
        {
            // status_unknown POIs render the universal "?" badge regardless of status_level_key,
            // so a stale key here is harmless and must not be flagged.
            var config = BaseConfig();
            config.pois.Add(new POIData { id = "a", category = "religious", has_status = true, status_unknown = true, status_level_key = "ghost_level" });
            Assert.IsEmpty(Validate(config));
        }

        [Test]
        public void CustomSymbolTicked_WithNoKey_ProducesOneIssue()
        {
            var config = BaseConfig();
            config.pois.Add(new POIData { id = "a", category = "religious", has_custom_symbol = true, custom_symbol_key = null });
            var issues = Validate(config);
            Assert.AreEqual(1, issues.Count);
            StringAssert.Contains("Custom Symbol", issues[0].problem);
        }

        [Test]
        public void EmptyTaxonomyTables_NeverFlagAnyPoi()
        {
            // A brand-new wall with no taxonomy authored yet has nothing to validate against --
            // every POI reference is "unresolved" only in the sense that nothing exists yet, which
            // is not the same bug as a stale reference into a real, non-empty table.
            var config = new WallConfigData { pois = new List<POIData>() };
            config.pois.Add(new POIData { id = "a", category = "anything", badge_category = "anything", has_status = true, status_level_key = "anything" });
            Assert.IsEmpty(Validate(config));
        }
    }
}
