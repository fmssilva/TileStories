// IdentityRenameResolverTests.cs
//
// EditMode tests for the shared identity-rename rule. Every taxonomy table in the
// POI Editor whose primary cell doubles as an identity that OTHER data references
// obeys one rule: committing a rename must rewrite every referencing POI, and a
// blank or colliding target must be refused so references never become ambiguous.
//
// Covered here: all four scalar identity shapes (category, badge key, status level
// key, hierarchy level key) plus the nested custom search-field key, all against the
// SAME rule -- this supersedes the category-only CategoryRenameResolverTests.
//
// The rewrites used here are the SHIPPED delegates (IdentityRenameResolver.
// CategoryRewrite etc.), not re-stated lambdas: a test that writes its own copy of
// the comparison proves nothing about what the window actually runs.
//
// Pure logic: no scene, no window, no Unity lifecycle, so it runs headless. The
// end-to-end question -- "does the RUNTIME still resolve the POI after a rename?" --
// lives in IdentityRenameCompositionTests, because a rule test can pass green while
// the composition is still broken (40-testing.md §4.2.1).
using System.Collections.Generic;
using NUnit.Framework;
using TileStories.Editor;

namespace TileStories.Tests
{
    public class IdentityRenameResolverTests
    {
        // --- category (row identity = CategoryStyleEntry.category) ---

        [Test]
        public void CategoryRename_SameName_IsNoOpAndSucceeds()
        {
            var rows = new List<CategoryStyleEntry>
            {
                new CategoryStyleEntry { category = "Royal Government" },
                new CategoryStyleEntry { category = "religious" },
            };
            var pois = new List<POIData> { new POIData { id = "a", category = "religious" } };

            bool ok = IdentityRenameResolver.TryCommit(
                rows, r => r.category, pois, IdentityRenameResolver.CategoryRewrite,
                "religious", "religious", out string rejection);

            Assert.IsTrue(ok, "Same-name rename is a legal no-op.");
            Assert.IsNull(rejection);
            Assert.AreEqual("religious", pois[0].category);
        }

        [Test]
        public void CategoryRename_RewritesEveryReferencingPoiAndLeavesOthersAlone()
        {
            var rows = new List<CategoryStyleEntry>
            {
                new CategoryStyleEntry { category = "Royal Government" },
                new CategoryStyleEntry { category = "military" },
            };
            var pois = new List<POIData>
            {
                new POIData { id = "a", category = "Royal Government" },
                new POIData { id = "b", category = "military" },
                new POIData { id = "c", category = "Royal Government" },
            };

            bool ok = IdentityRenameResolver.TryCommit(
                rows, r => r.category, pois, IdentityRenameResolver.CategoryRewrite,
                "Royal Government", "Heritage Royal", out string rejection);

            Assert.IsTrue(ok, rejection);
            Assert.AreEqual("Heritage Royal", pois[0].category);
            Assert.AreEqual("Heritage Royal", pois[2].category);
            Assert.AreEqual("military", pois[1].category, "Unrelated POI must not move.");
        }

        [Test]
        public void CategoryRename_BlankTarget_IsRejectedAndPoisUntouched()
        {
            var rows = new List<CategoryStyleEntry> { new CategoryStyleEntry { category = "military" } };
            var pois = new List<POIData> { new POIData { id = "a", category = "military" } };

            bool ok = IdentityRenameResolver.TryCommit(
                rows, r => r.category, pois, IdentityRenameResolver.CategoryRewrite,
                "military", "   ", out string rejection);

            Assert.IsFalse(ok, "Whitespace-only identity must be refused.");
            Assert.IsNotNull(rejection);
            Assert.AreEqual("military", pois[0].category, "POI must stay untouched on rejection.");
        }

        [Test]
        public void CategoryRename_CollidingWithAnotherRow_IsRejectedAndPoisUntouched()
        {
            var rows = new List<CategoryStyleEntry>
            {
                new CategoryStyleEntry { category = "religious" },
                new CategoryStyleEntry { category = "military" },
            };
            var pois = new List<POIData> { new POIData { id = "a", category = "religious" } };

            bool ok = IdentityRenameResolver.TryCommit(
                rows, r => r.category, pois, IdentityRenameResolver.CategoryRewrite,
                "religious", "military", out string rejection);

            Assert.IsFalse(ok, "Target already used by another row -> ambiguous -> refused.");
            Assert.IsNotNull(rejection);
            Assert.AreEqual("religious", pois[0].category);
        }

        [Test]
        public void CategoryRename_NullPoiList_StillSucceeds()
        {
            var rows = new List<CategoryStyleEntry> { new CategoryStyleEntry { category = "religious" } };

            bool ok = IdentityRenameResolver.TryCommit(
                rows, r => r.category, null, IdentityRenameResolver.CategoryRewrite,
                "religious", "faith", out string rejection);

            Assert.IsTrue(ok, "No POIs to propagate to is still a valid row rename.");
            Assert.IsNull(rejection);
        }

        [Test]
        public void CategoryRename_IsCaseSensitive()
        {
            var rows = new List<CategoryStyleEntry> { new CategoryStyleEntry { category = "Royal Government" } };
            var pois = new List<POIData>
            {
                new POIData { id = "a", category = "Royal Government" },
                new POIData { id = "b", category = "royal government" },
            };

            bool ok = IdentityRenameResolver.TryCommit(
                rows, r => r.category, pois, IdentityRenameResolver.CategoryRewrite,
                "Royal Government", "Heritage", out _);

            Assert.IsTrue(ok);
            Assert.AreEqual("Heritage", pois[0].category);
            Assert.AreEqual("royal government", pois[1].category,
                "Ordinal identity: a differently-cased name is a different category, not the same one.");
        }

        [Test]
        public void HasCollision_OnlyOtherRowsCount()
        {
            var rows = new List<CategoryStyleEntry>
            {
                new CategoryStyleEntry { category = "religious" },
                new CategoryStyleEntry { category = "military" },
            };

            Assert.IsFalse(IdentityRenameResolver.HasCollision(rows, r => r.category, "religious", "religious"),
                "A row does not collide with itself.");
            Assert.IsTrue(IdentityRenameResolver.HasCollision(rows, r => r.category, "religious", "military"));
            Assert.IsFalse(IdentityRenameResolver.HasCollision((IList<CategoryStyleEntry>)null, r => r.category, "a", "b"));
        }

        // --- badge key (row identity = BadgeCategoryEntry.key) ---

        [Test]
        public void BadgeKeyRename_RewritesPoiBadgeCategoryAndNothingElse()
        {
            var rows = new List<BadgeCategoryEntry>
            {
                new BadgeCategoryEntry { key = "partial_damage" },
                new BadgeCategoryEntry { key = "intact" },
            };
            var pois = new List<POIData>
            {
                new POIData { id = "a", badge_category = "partial_damage", category = "military" },
                new POIData { id = "b", badge_category = "intact" },
            };

            bool ok = IdentityRenameResolver.TryCommit(
                rows, r => r.key, pois, IdentityRenameResolver.BadgeKeyRewrite,
                "partial_damage", "damaged_partly", out string rejection);

            Assert.IsTrue(ok, rejection);
            Assert.AreEqual("damaged_partly", pois[0].badge_category);
            Assert.AreEqual("intact", pois[1].badge_category);
            Assert.AreEqual("military", pois[0].category, "A badge rename must not touch category.");
        }

        // --- status level key (row identity = OutlineLevelEntry.key) ---

        [Test]
        public void StatusLevelKeyRename_RewritesPoiStatusLevelKeyOnly()
        {
            var rows = new List<OutlineLevelEntry>
            {
                new OutlineLevelEntry { key = "level_2", label = "Partial Damage" },
                new OutlineLevelEntry { key = "level_1", label = "Intact" },
            };
            // POI "a" deliberately carries the SAME string in badge_category as in
            // status_level_key, so a rewrite that ignored which field it belongs to
            // would corrupt the badge identity and be caught here.
            var pois = new List<POIData>
            {
                new POIData { id = "a", status_level_key = "level_2", badge_category = "level_2" },
                new POIData { id = "b", status_level_key = "level_1" },
            };

            bool ok = IdentityRenameResolver.TryCommit(
                rows, r => r.key, pois, IdentityRenameResolver.StatusLevelKeyRewrite,
                "level_2", "level_mid", out string rejection);

            Assert.IsTrue(ok, rejection);
            Assert.AreEqual("level_mid", pois[0].status_level_key);
            Assert.AreEqual("level_1", pois[1].status_level_key);
            Assert.AreEqual("level_2", pois[0].badge_category,
                "A status-level rename must leave the badge identity alone even when the strings collide.");
        }

        // --- hierarchy level key (row identity = HierarchyLevelEntry.key) ---

        [Test]
        public void HierarchyLevelKeyRename_RewritesPoiHierarchyLevelKeyOnly()
        {
            var rows = new List<HierarchyLevelEntry>
            {
                new HierarchyLevelEntry { key = "level_3", level_name = "3" },
                new HierarchyLevelEntry { key = "level_1", level_name = "Hub" },
            };
            var pois = new List<POIData>
            {
                new POIData { id = "a", hierarchy_level_key = "level_3" },
                new POIData { id = "b", hierarchy_level_key = "level_1" },
                new POIData { id = "c", hierarchy_level_key = null },
            };

            bool ok = IdentityRenameResolver.TryCommit(
                rows, r => r.key, pois, IdentityRenameResolver.HierarchyLevelKeyRewrite,
                "level_3", "level_mid", out string rejection);

            Assert.IsTrue(ok, rejection);
            Assert.AreEqual("level_mid", pois[0].hierarchy_level_key);
            Assert.AreEqual("level_1", pois[1].hierarchy_level_key);
            Assert.IsNull(pois[2].hierarchy_level_key, "A POI with no level must stay unset.");
        }

        // --- custom search-field key (identity referenced from a nested list) ---

        private static POIData MakePoiWithFields(string id, params (string key, string[] keywords)[] fields)
        {
            var poi = new POIData { id = id, search_keyword_fields = new List<POISearchKeywordField>() };
            foreach (var (key, keywords) in fields)
            {
                poi.search_keyword_fields.Add(new POISearchKeywordField
                {
                    field_key = key,
                    keywords = new List<string>(keywords),
                });
            }
            return poi;
        }

        [Test]
        public void SearchFieldKeyRename_RenamesInnerFieldKeyInPlaceAndKeepsKeywords()
        {
            var rows = new List<SearchFieldDefinition> { new SearchFieldDefinition { key = "material" } };
            var pois = new List<POIData>
            {
                MakePoiWithFields("a", ("material", new[] { "marble", "stone" }), ("era", new[] { "baroque" })),
                MakePoiWithFields("b", ("era", new[] { "gothic" })),
            };

            bool ok = IdentityRenameResolver.TryCommit(
                rows, r => r.key, pois, SearchFieldReferenceResolver.RenameFieldKey,
                "material", "building_material", out string rejection);

            Assert.IsTrue(ok, rejection);

            // Same entry, renamed key, keywords intact -- NOT an added entry.
            Assert.AreEqual(2, pois[0].search_keyword_fields.Count,
                "Renaming must rewrite in place; adding a second entry is the silent-data-growth bug.");
            Assert.AreEqual("building_material", pois[0].search_keyword_fields[0].field_key);
            CollectionAssert.AreEqual(new List<string> { "marble", "stone" },
                pois[0].search_keyword_fields[0].keywords);

            // Sibling field and unrelated POI untouched.
            Assert.AreEqual("era", pois[0].search_keyword_fields[1].field_key);
            Assert.AreEqual("era", pois[1].search_keyword_fields[0].field_key);

            // The reference moved: the new key is now what this POI points at.
            Assert.IsFalse(SearchFieldReferenceResolver.PoiHasField(pois[0], "material"));
            Assert.IsTrue(SearchFieldReferenceResolver.PoiHasField(pois[0], "building_material"));
        }

        // --- delete-reference counting (the other half: deleting cannot propagate) ---

        [Test]
        public void CountReferences_ScalarRules_CountOnlyPoisPointingAtThatIdentity()
        {
            var pois = new List<POIData>
            {
                new POIData { id = "a", category = "religious", badge_category = "intact",
                              status_level_key = "level_1", hierarchy_level_key = "level_1" },
                new POIData { id = "b", category = "military", badge_category = "intact",
                              status_level_key = "level_2", hierarchy_level_key = "level_2" },
                new POIData { id = "c", category = "religious", badge_category = "destroyed",
                              status_level_key = null, hierarchy_level_key = null },
            };

            Assert.AreEqual(2, IdentityRenameResolver.CountReferences(pois, IdentityRenameResolver.PoiUsesCategory, "religious"));
            Assert.AreEqual(2, IdentityRenameResolver.CountReferences(pois, IdentityRenameResolver.PoiUsesBadgeKey, "intact"));
            Assert.AreEqual(1, IdentityRenameResolver.CountReferences(pois, IdentityRenameResolver.PoiUsesStatusLevelKey, "level_1"));
            Assert.AreEqual(1, IdentityRenameResolver.CountReferences(pois, IdentityRenameResolver.PoiUsesHierarchyLevelKey, "level_2"));
            Assert.AreEqual(0, IdentityRenameResolver.CountReferences(pois, IdentityRenameResolver.PoiUsesCategory, "nothing_uses_this"));
        }

        [Test]
        public void CountReferences_SearchFieldRule_CountsPoisHoldingThatNestedKey()
        {
            var pois = new List<POIData>
            {
                MakePoiWithFields("a", ("material", new[] { "marble" }), ("era", new[] { "baroque" })),
                MakePoiWithFields("b", ("era", new[] { "gothic" })),
            };

            Assert.AreEqual(1, SearchFieldReferenceResolver.CountPoisUsingField(pois, "material"));
            Assert.AreEqual(2, SearchFieldReferenceResolver.CountPoisUsingField(pois, "era"));
            Assert.AreEqual(0, SearchFieldReferenceResolver.CountPoisUsingField(pois, "unused"));
        }

        [Test]
        public void CountReferences_NullPoiList_IsZero()
        {
            Assert.AreEqual(0, IdentityRenameResolver.CountReferences(null, IdentityRenameResolver.PoiUsesCategory, "x"));
            Assert.AreEqual(0, SearchFieldReferenceResolver.CountPoisUsingField(null, "x"));
        }
    }
}