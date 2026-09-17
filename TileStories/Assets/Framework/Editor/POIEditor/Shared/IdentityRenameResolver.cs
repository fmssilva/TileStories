// IdentityRenameResolver.cs
//
// Editor-only pure decision table for renaming an IDENTITY string in any taxonomy
// table where the row's identity is referenced by other data (POIs, per-POI
// keyword lists). All three real cases are the same rule:
//   - category_styles  row identity = CategoryStyleEntry.category,
//                       referenced by POIData.category
//   - badge_categories row identity = BadgeCategoryEntry.key,
//                       referenced by POIData.badge_category
//   - search_fields    row identity = SearchFieldDefinition.key,
//                       referenced by POIData.search_keyword_fields[].field_key
// Renaming such a row without rewriting the referencing rows silently orphans
// them (icons/keywords/colors/facets stop matching). This class decides WHEN a
// commit is legal and WHAT it does. Pure logic, no IMGUI/UnityEditor references,
// Tier-0 testable with `new`.
//
// Commit rules (identical to the old CategoryRenameResolver, now generalized):
//   - old == new                  -> no-op, succeeds
//   - new is blank/whitespace     -> rejected (identity must not be empty)
//   - new collides with ANOTHER row's identity -> rejected (ambiguous)
//   - otherwise                   -> every POI whose ref == old is rewritten to new
//                                    via the supplied poiRefSet delegate
//
// Cancel needs no logic here: the editor only writes the draft to the row on
// Commit, so cancelling just discards the SessionState draft.
using System.Collections.Generic;

namespace TileStories.Editor
{
    public static class IdentityRenameResolver
    {
        public delegate string RowIdentityGetter<T>(T row);
        public delegate void RowIdentitySetter<T>(T row, string value);

        // Rewrites a POI's reference(s) to this identity. The scalar cases
        // (category, badge key) overwrite one field; the search-field case must
        // rewrite every match inside POIData.search_keyword_fields[].field_key.
        public delegate void PoiIdentityRewrite(POIData poi, string oldIdentity, string newIdentity);

        // Rewrites every POI whose reference equals oldName to newName. Returns
        // false (with a human-readable reason) when the rename must not happen:
        // blank target or a collision with another row.
        public static bool TryCommit<T>(
            IList<T> rows,
            RowIdentityGetter<T> rowIdentity,
            IList<POIData> pois,
            PoiIdentityRewrite poiRewrite,
            string oldName,
            string newName,
            out string rejection)
        {
            rejection = null;

            if (string.Equals(oldName, newName, System.StringComparison.Ordinal))
                return true; // nothing to do -- same name, no propagation needed

            if (string.IsNullOrWhiteSpace(newName))
            {
                rejection = "Identity must not be empty.";
                return false;
            }

            if (HasCollision(rows, rowIdentity, oldName, newName))
            {
                rejection = "Another row already uses this identity.";
                return false;
            }

            if (pois == null)
                return true;

            for (int i = 0; i < pois.Count; i++)
            {
                var poi = pois[i];
                if (poi != null)
                    poiRewrite(poi, oldName, newName);
            }

            return true;
        }

        // True when some OTHER row (not the one being renamed) already declares
        // newName as its identity -- the rename would make references ambiguous.
        public static bool HasCollision<T>(
            IList<T> rows,
            RowIdentityGetter<T> rowIdentity,
            string oldName,
            string newName)
        {
            if (rows == null)
                return false;

            for (int i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                if (row == null)
                    continue;

                // Same identity as the row being renamed is not a collision.
                string id = rowIdentity(row);
                if (string.Equals(id, oldName, System.StringComparison.Ordinal))
                    continue;

                if (string.Equals(id, newName, System.StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        // True when this POI's own data still points at the given identity. The
        // scalar cases (category, badge key, status level key, hierarchy key)
        // compare one field; the search-field case scans the per-POI keyword
        // lists. Distinct from PoiIdentityRewrite because counting references
        // needs only a yes/no answer, never a new value.
        public delegate bool PoiReferenceTest(POIData poi, string identity);

        // How many POIs still point at this identity. Used before a taxonomy row
        // is deleted out from under the POIs that reference it -- removing such a
        // row leaves them naming an identity that no longer exists, so their
        // icon/colour/ring silently falls back to the framework default.
        public static int CountReferences(IList<POIData> pois, PoiReferenceTest references, string identity)
        {
            if (pois == null || references == null)
                return 0;

            int count = 0;
            for (int i = 0; i < pois.Count; i++)
            {
                var poi = pois[i];
                if (poi != null && references(poi, identity))
                    count++;
            }

            return count;
        }

        // ---- Concrete rules: one per identity shape ----
        //
        // These are the exact delegates the POI Editor hands to TryCommit and
        // CountReferences. They live here, not as inline lambdas in the window, for
        // two reasons:
        //   1. a test then exercises the SHIPPED rewrite instead of re-stating the
        //      same comparison and proving nothing about the window (40-testing.md
        //      §4.2.1 -- a call site existing is not evidence the composition works);
        //   2. "which identity maps to which POI field" is defined exactly once.
        // The one nested identity shape (per-POI keyword lists) cannot be expressed as
        // a single field comparison and lives in SearchFieldReferenceResolver instead.
        //
        // Every comparison is Ordinal. Identity is an exact string, so two names
        // differing only in case are genuinely two different identities -- and the
        // rename rule refuses collisions, never fuzzily merging them.

        public static readonly PoiIdentityRewrite CategoryRewrite =
            (poi, oldIdentity, newIdentity) =>
            {
                if (string.Equals(poi.category, oldIdentity, System.StringComparison.Ordinal))
                    poi.category = newIdentity;
            };

        public static readonly PoiIdentityRewrite BadgeKeyRewrite =
            (poi, oldIdentity, newIdentity) =>
            {
                if (string.Equals(poi.badge_category, oldIdentity, System.StringComparison.Ordinal))
                    poi.badge_category = newIdentity;
            };

        public static readonly PoiIdentityRewrite StatusLevelKeyRewrite =
            (poi, oldIdentity, newIdentity) =>
            {
                if (string.Equals(poi.status_level_key, oldIdentity, System.StringComparison.Ordinal))
                    poi.status_level_key = newIdentity;
            };

        public static readonly PoiIdentityRewrite HierarchyLevelKeyRewrite =
            (poi, oldIdentity, newIdentity) =>
            {
                if (string.Equals(poi.hierarchy_level_key, oldIdentity, System.StringComparison.Ordinal))
                    poi.hierarchy_level_key = newIdentity;
            };

        // Same four identities as read-only tests, for the delete-reference count.
        public static readonly PoiReferenceTest PoiUsesCategory =
            (poi, identity) => string.Equals(poi.category, identity, System.StringComparison.Ordinal);

        public static readonly PoiReferenceTest PoiUsesBadgeKey =
            (poi, identity) => string.Equals(poi.badge_category, identity, System.StringComparison.Ordinal);

        public static readonly PoiReferenceTest PoiUsesStatusLevelKey =
            (poi, identity) => string.Equals(poi.status_level_key, identity, System.StringComparison.Ordinal);

        public static readonly PoiReferenceTest PoiUsesHierarchyLevelKey =
            (poi, identity) => string.Equals(poi.hierarchy_level_key, identity, System.StringComparison.Ordinal);
    }
}
