// SearchFieldReferenceResolver.cs
//
// Editor-only concrete rule for the ONE taxonomy whose identity is referenced from
// inside a nested list rather than from a single POI field: a
// SearchFieldDefinition.key is the identity each POI's
// POIData.search_keyword_fields[].field_key points at.
//
// The scalar tables (category_styles, badge_categories, outline_levels,
// hierarchy_levels) express their reference inline as a single field comparison, so
// they call IdentityRenameResolver directly with a one-line lambda. This one cannot:
// it needs a scan of the inner list, and BOTH the rename rewrite and the
// delete-reference count need that same scan. Keeping the scan here -- instead of as
// a private method inside the editor window -- is what makes the seam testable with
// plain `new` and no EditorWindow running (the §4.2.1 "test the composition" rule).
using System.Collections.Generic;

namespace TileStories.Editor
{
    public static class SearchFieldReferenceResolver
    {
        // True when this POI holds a keyword list under the given custom field key.
        public static bool PoiHasField(POIData poi, string fieldKey)
        {
            if (poi == null || poi.search_keyword_fields == null)
                return false;

            foreach (var entry in poi.search_keyword_fields)
            {
                if (entry != null && string.Equals(entry.field_key, fieldKey, System.StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        // Rename the field key IN PLACE on every keyword list that points at it.
        // In place, not a new entry: the authored keywords must travel with the
        // renamed row. Adding a fresh entry under the new key is exactly the silent
        // data growth (plus silently stranded keywords) this rule exists to prevent.
        public static void RenameFieldKey(POIData poi, string oldKey, string newKey)
        {
            if (poi == null || poi.search_keyword_fields == null)
                return;

            foreach (var entry in poi.search_keyword_fields)
            {
                if (entry != null && string.Equals(entry.field_key, oldKey, System.StringComparison.Ordinal))
                    entry.field_key = newKey;
            }
        }

        // How many POIs would be orphaned by deleting this custom field row.
        public static int CountPoisUsingField(IList<POIData> pois, string fieldKey)
        {
            return IdentityRenameResolver.CountReferences(pois, PoiHasField, fieldKey);
        }
    }
}
