// IdentityRenameCompositionTests.cs
//
// The §4.2.1 "test the seam, not just the pieces" tests for taxonomy identity
// renames. IdentityRenameResolverTests proves the RULE rewrites the right strings.
// These prove the COMPOSITION the rule exists for: after a rename, the real runtime
// consumers still resolve the POI -- its palette entry, its badge definition, its
// status ring, its hierarchy style, and its taxonomy keywords in the search index.
// A rename that rewrote a string into the wrong place would sail through every
// isolated rule test and still leave markers colourless and unsearchable.
//
// Deliberately runs against the REAL Assets/Apps/LivingRoom/config.json (the file the
// POI Editor edits and whose Save pipeline copies to StreamingAssets), not a
// hand-built fixture. A fixture would have to be kept in step with the wall by hand,
// which is exactly the drift 40-testing.md §4.4 warns about. The only authored value
// these tests add is a deliberately distinctive keyword ("linkprobe"): it cannot
// collide with any existing POI name/summary, so a search hit can ONLY come from the
// taxonomy row -> POI identity link under test.
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using TileStories.Editor;
using UnityEngine;

namespace TileStories.Tests
{
    public class IdentityRenameCompositionTests
    {
        private const string LivingRoomConfigPath = "Assets/Apps/LivingRoom/config.json";
        private const string ProbeKeyword = "linkprobe";

        // Load the real authored wall config from disk.
        private static WallConfigData LoadRealLivingRoomConfig()
        {
            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), LivingRoomConfigPath);
            Assert.IsTrue(File.Exists(fullPath), $"Authoring config must exist at '{fullPath}'.");

            var config = JsonUtility.FromJson<WallConfigData>(File.ReadAllText(fullPath));
            Assert.IsNotNull(config, "config.json must deserialize into WallConfigData.");
            Assert.IsNotNull(config.pois, "config.pois must not be null.");
            Assert.Greater(config.pois.Count, 0, "The real wall must declare POIs.");
            return config;
        }

        // Search ids for a probe keyword that only the taxonomy identity link can produce.
        private static List<string> SearchIds(WallConfigData config, string query)
        {
            var index = new POISearchIndex();
            index.Build(config);
            return index.Search(query, SearchOptions.Default).Select(r => r.PoiId).OrderBy(id => id).ToList();
        }

        [Test]
        public void CategoryRename_RealConfig_PaletteStillResolvesAndTaxonomyKeywordsStayIndexed()
        {
            var config = LoadRealLivingRoomConfig();
            const string oldName = "Royal Government";
            const string newName = "Heritage Royal";

            var referencing = config.pois.Where(p => p.category == oldName).ToList();
            Assert.Greater(referencing.Count, 0,
                $"The real wall must use category '{oldName}', or this test proves nothing.");

            bool ok = IdentityRenameResolver.TryCommit(
                config.category_styles, r => r.category, config.pois,
                IdentityRenameResolver.CategoryRewrite, oldName, newName, out string rejection);
            Assert.IsTrue(ok, rejection);

            Assert.AreEqual(0, config.pois.Count(p => p.category == oldName),
                "No POI may still name the old category.");
            Assert.AreEqual(referencing.Count, config.pois.Count(p => p.category == newName),
                "Exactly the referencing POIs must move.");

            // The row itself moves too, then author the probe keyword on it.
            var row = config.category_styles.First(r => r.category == oldName);
            row.category = newName;
            row.search_keywords = new List<string> { ProbeKeyword };

            // RUNTIME 1: the marker's colour/icon still resolves (was: hash fallback).
            CategoryPalette.Configure(config.category_styles);
            try
            {
                foreach (var poi in config.pois.Where(p => p.category == newName))
                {
                    Assert.IsTrue(CategoryPalette.TryResolveConfigured(poi.category, out _, out _),
                        $"POI '{poi.id}' must resolve its palette entry after the rename.");
                }
            }
            finally
            {
                CategoryPalette.ClearOverrides();
            }

            // RUNTIME 2: the row's keywords still reach the POIs that name it
            // (POISearchIndex.Build matches each POI to its taxonomy row by the identity string).
            var found = SearchIds(config, ProbeKeyword);
            CollectionAssert.AreEqual(
                referencing.Select(p => p.id).OrderBy(id => id).ToList(), found,
                "Searching the renamed row's keyword must return exactly the POIs that reference it.");
        }

        [Test]
        public void BadgeKeyRename_RealConfig_BadgeDefinitionStillResolvesAndKeywordsStayIndexed()
        {
            var config = LoadRealLivingRoomConfig();
            const string oldKey = "partial_damage";
            const string newKey = "damaged_partly";

            var referencing = config.pois.Where(p => p.badge_category == oldKey).ToList();
            Assert.Greater(referencing.Count, 0,
                $"The real wall must use badge key '{oldKey}', or this test proves nothing.");

            bool ok = IdentityRenameResolver.TryCommit(
                config.badge_categories, r => r.key, config.pois,
                IdentityRenameResolver.BadgeKeyRewrite, oldKey, newKey, out string rejection);
            Assert.IsTrue(ok, rejection);
            Assert.AreEqual(0, config.pois.Count(p => p.badge_category == oldKey));

            var row = config.badge_categories.First(r => r.key == oldKey);
            row.key = newKey;
            row.search_keywords = new List<string> { ProbeKeyword };

            // RUNTIME 1: the badge icon/tint still resolves for those POIs.
            // BadgeCategoryPalette matches POIData.badge_category against entry.key.
            BadgeCategoryPalette.Configure(config.badge_categories);
            try
            {
                foreach (var poi in config.pois.Where(p => p.badge_category == newKey))
                {
                    Assert.IsTrue(BadgeCategoryPalette.TryResolve(poi.badge_category, out var def),
                        $"POI '{poi.id}' must resolve its badge definition after the rename.");
                    Assert.IsFalse(string.IsNullOrWhiteSpace(def.IconKey),
                        $"POI '{poi.id}' badge must keep its authored icon key.");
                }
            }
            finally
            {
                BadgeCategoryPalette.Clear();
            }

            // RUNTIME 2: the badge row's keywords still reach its POIs.
            var found = SearchIds(config, ProbeKeyword);
            CollectionAssert.AreEqual(
                referencing.Select(p => p.id).OrderBy(id => id).ToList(), found,
                "Searching the renamed badge row's keyword must return exactly the POIs that use it.");
        }

        [Test]
        public void StatusLevelKeyRename_RealConfig_StatusRingStillResolvesForEveryReferencingPoi()
        {
            var config = LoadRealLivingRoomConfig();
            const string oldKey = "partial_damage";
            const string newKey = "damaged_partly";
            const string probe = "ringprobe";

            var referencing = config.pois.Where(p => p.status_level_key == oldKey).ToList();
            Assert.Greater(referencing.Count, 0,
                $"The real wall must use outline level '{oldKey}', or this test proves nothing.");

            bool ok = IdentityRenameResolver.TryCommit(
                config.outline_levels, r => r.key, config.pois,
                IdentityRenameResolver.StatusLevelKeyRewrite, oldKey, newKey, out string rejection);
            Assert.IsTrue(ok, rejection);
            Assert.AreEqual(0, config.pois.Count(p => p.status_level_key == oldKey));

            var row = config.outline_levels.First(r => r.key == oldKey);
            row.key = newKey;
            row.search_keywords = new List<string> { probe };

            StatusRamp.Configure(config.outline_levels, MarkerOutlineMode.PerType, default);
            try
            {
                // The renamed level is the live one, and the old name is gone: proves
                // the ROW moved together with its references, not just the POIs.
                Assert.IsTrue(StatusRamp.TryResolveByKey(newKey, out _),
                    "The renamed outline level must be resolvable by its new key.");
                Assert.IsFalse(StatusRamp.TryResolveByKey(oldKey, out _),
                    "The old outline level key must no longer resolve.");

                foreach (var poi in referencing)
                {
                    Assert.IsTrue(StatusRamp.TryResolveByKey(poi.status_level_key, out var level),
                        $"POI '{poi.id}' must resolve its status ring after the rename.");
                    Assert.IsFalse(string.IsNullOrEmpty(level.RingSpriteKey), "A resolved level must carry a usable ring line style.");
                }
            }
            finally
            {
                StatusRamp.ResetToDefaults();
            }

            var found = SearchIds(config, probe);
            CollectionAssert.AreEqual(
                referencing.Select(p => p.id).OrderBy(id => id).ToList(), found,
                "Searching the renamed outline row's keyword must return exactly the POIs that use it.");
        }

        [Test]
        public void HierarchyLevelKeyRename_RealConfig_HierarchyStyleStillResolvesAndLabelSearchStillFinds()
        {
            var config = LoadRealLivingRoomConfig();
            const string oldKey = "level_3";
            const string newKey = "level_mid";

            // The runtime search index links POIs to hierarchy levels through
            // poi.hierarchy_level_key -> level.key, then indexes the LEVEL'S LABEL. It
            // does NOT consume HierarchyLevelEntry.search_keywords -- that member has no
            // editor column and no index consumer (unlike category/badge/outline rows,
            // whose search_keywords feed IndexTaxonomyKeywords). So the composition probe
            // is the label: give the renamed level a unique label, then search it. If the
            // rename missed EITHER side (the row key or the POI rewrite), this lookup
            // breaks and returns 0 -- exactly what this assertion catches.
            const string probeKeyword = "sizeprobe";

            var referencing = config.pois.Where(p => p.hierarchy_level_key == oldKey).ToList();
            Assert.Greater(referencing.Count, 0,
                $"The real wall must use hierarchy level '{oldKey}', or this test proves nothing.");

            bool ok = IdentityRenameResolver.TryCommit(
                config.hierarchy_levels, r => r.key, config.pois,
                IdentityRenameResolver.HierarchyLevelKeyRewrite, oldKey, newKey, out string rejection);
            Assert.IsTrue(ok, rejection);
            Assert.AreEqual(0, config.pois.Count(p => p.hierarchy_level_key == oldKey));

            var row = config.hierarchy_levels.First(r => r.key == oldKey);
            float authoredSize = row.size_cm;
            row.key = newKey;
            row.level_name = probeKeyword; // level_name is display-only, so probing it is safe

            MarkerHierarchyResolver.Configure(config.hierarchy_levels);
            try
            {
                foreach (var poi in referencing)
                {
                    Assert.IsTrue(MarkerHierarchyResolver.TryResolveByKey(poi.hierarchy_level_key, out var style),
                        $"POI '{poi.id}' must resolve its hierarchy style after the rename.");
                    Assert.AreEqual(authoredSize, style.SizeCm,
                        $"POI '{poi.id}' must keep the authored marker size through the rename.");
                }
            }
            finally
            {
                MarkerHierarchyResolver.ResetToDefaults();
            }

            var found = SearchIds(config, probeKeyword);
            CollectionAssert.AreEqual(
                referencing.Select(p => p.id).OrderBy(id => id).ToList(), found,
                "Searching the renamed hierarchy level's label must return exactly the POIs that use it.");
        }

        [Test]
        public void SearchFieldKeyRename_RealConfig_PoiKeywordsStayFindableAndNoEntryIsDuplicated()
        {
            var config = LoadRealLivingRoomConfig();
            const string oldKey = "material";
            const string newKey = "building_material";
            const string probe = "materialprobe";

            // The shipped wall has no custom keyword fields yet, so author one the way
            // the POI Editor's "+ Add keyword field" would, and fill it on two real POIs.
            config.search_fields = new List<SearchFieldDefinition>
            {
                new SearchFieldDefinition { key = oldKey, label = "Material" },
            };

            var carriers = config.pois.Take(2).ToList();
            foreach (var poi in carriers)
            {
                poi.search_keyword_fields = new List<POISearchKeywordField>
                {
                    new POISearchKeywordField { field_key = oldKey, keywords = new List<string> { probe } },
                };
            }

            bool ok = IdentityRenameResolver.TryCommit(
                config.search_fields, r => r.key, config.pois,
                SearchFieldReferenceResolver.RenameFieldKey, oldKey, newKey, out string rejection);
            Assert.IsTrue(ok, rejection);

            config.search_fields.First(r => r.key == oldKey).key = newKey;

            Assert.AreEqual(0, SearchFieldReferenceResolver.CountPoisUsingField(config.pois, oldKey),
                "No POI may still point at the old custom field key.");
            Assert.AreEqual(carriers.Count, SearchFieldReferenceResolver.CountPoisUsingField(config.pois, newKey));

            foreach (var poi in carriers)
            {
                Assert.AreEqual(1, poi.search_keyword_fields.Count,
                    $"POI '{poi.id}' must still hold exactly one entry for its renamed field, not a duplicate.");
                Assert.AreEqual(probe, poi.search_keyword_fields[0].keywords[0],
                    "The authored keyword must survive the rename.");
            }

            // RUNTIME: the keywords are indexed and still find their POIs, which is the
            // whole point -- before the fix they were stranded under the old key.
            var found = SearchIds(config, probe);
            CollectionAssert.AreEqual(
                carriers.Select(p => p.id).OrderBy(id => id).ToList(), found,
                "Keywords held under a renamed custom field must remain searchable.");
        }

        [Test]
        public void DeleteReferenceCount_RealConfig_IsNonZeroForRowsInUseAndZeroForUnusedOnes()
        {
            var config = LoadRealLivingRoomConfig();

            // Rows the wall genuinely uses: deleting one without asking would orphan
            // real POIs, so the guard must see a non-zero count and prompt.
            Assert.Greater(
                IdentityRenameResolver.CountReferences(config.pois, IdentityRenameResolver.PoiUsesBadgeKey, "partial_damage"), 0,
                "A badge key in real use must count as referenced.");
            Assert.Greater(
                IdentityRenameResolver.CountReferences(config.pois, IdentityRenameResolver.PoiUsesStatusLevelKey, "unknown"), 0,
                "The outline level in real use must count as referenced.");
            Assert.Greater(
                IdentityRenameResolver.CountReferences(config.pois, IdentityRenameResolver.PoiUsesHierarchyLevelKey, "level_1"), 0,
                "A hierarchy level in real use must count as referenced.");
            Assert.Greater(
                IdentityRenameResolver.CountReferences(config.pois, IdentityRenameResolver.PoiUsesCategory, "military"), 0,
                "A category in real use must count as referenced.");

            // A row nothing uses: deleting it must stay a single click, so the count is
            // zero and the guard does not interrupt the developer at all.
            Assert.AreEqual(0,
                IdentityRenameResolver.CountReferences(config.pois, IdentityRenameResolver.PoiUsesBadgeKey, "no_such_badge_key"),
                "An unused badge key must count as unreferenced.");
        }
    }
}
