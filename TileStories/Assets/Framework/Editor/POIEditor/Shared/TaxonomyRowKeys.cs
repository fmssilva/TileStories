using System;
using System.Collections.Generic;

namespace TileStories.Editor
{
    // The identity of every taxonomy row (Category, Badge, Outline Types, Hierarchy Levels, Keyword Fields): a key
    // generated once and never shown -- POIs store it, the developer only ever edits the row's name. So a rename
    // never touches a POI; the only thing that can orphan one is deleting a row still in use, which the delete
    // guard counts here. Pure.
    public static class TaxonomyRowKeys
    {
        // A key nobody uses yet: prefix + the smallest N >= rows.Count + 1 that is free. A key must never
        // repeat -- prefix + (Count + 1) alone collided once a middle row had been deleted (level_1,
        // level_3 -> a second level_3), and two rows sharing a key make their POIs resolve to the wrong one.
        public static string NextFree<T>(IList<T> rows, Func<T, string> keyOf, string prefix) where T : class
        {
            var used = new HashSet<string>();
            if (rows != null)
                foreach (var row in rows)
                {
                    string key = row == null ? null : keyOf(row);
                    if (!string.IsNullOrWhiteSpace(key)) used.Add(key.Trim());
                }

            int n = (rows?.Count ?? 0) + 1;
            while (used.Contains(prefix + n)) n++;
            return prefix + n;
        }

        // The key prefix of each table, one per table so a POI's stored key says which table it points into
        public const string CategoryPrefix = "category_";
        public const string BadgePrefix = "badge_";
        public const string OutlinePrefix = "outline_";
        public const string LevelPrefix = "level_";
        public const string FieldPrefix = "field_";
        // Not a taxonomy table, same identity rule: a POI's card blocks (_3.1 section 5.2, never shown, never reused)
        public const string BlockPrefix = "block_";

        // Give every row of every table a key of its own: a blank key, or one an earlier row already has (only a
        // hand-edited file gets there -- "+ Add" never repeats one), gets NextFree. The FIRST row keeps a shared key,
        // so the POIs using it keep showing what they showed (the runtime also finds the first row). POIs are never
        // touched. Returns how many rows got a new key. Called once on load, never while drawing.
        public static int RepairKeys(WallConfigData config)
        {
            if (config == null) return 0;
            return RepairKeys(config.category_styles, e => e.key, (e, k) => e.key = k, CategoryPrefix)
                 + RepairKeys(config.badge_categories, e => e.key, (e, k) => e.key = k, BadgePrefix)
                 + RepairKeys(config.outline_levels, e => e.key, (e, k) => e.key = k, OutlinePrefix)
                 + RepairKeys(config.hierarchy_levels, e => e.key, (e, k) => e.key = k, LevelPrefix)
                 + RepairKeys(config.search_fields, e => e.key, (e, k) => e.key = k, FieldPrefix);
        }

        private static int RepairKeys<T>(IList<T> rows, Func<T, string> keyOf, Action<T, string> setKey, string prefix) where T : class
        {
            if (rows == null) return 0;
            var seen = new HashSet<string>();
            int repaired = 0;
            foreach (var row in rows)
            {
                if (row == null) continue;
                string key = keyOf(row)?.Trim();
                if (!string.IsNullOrEmpty(key) && seen.Add(key)) continue;
                // - NextFree skips every key in the table, including the ones this loop has not reached yet
                string fresh = NextFree(rows, keyOf, prefix);
                setKey(row, fresh);
                seen.Add(fresh);
                repaired++;
            }
            return repaired;
        }

        // Does this POI point at the row with this key? One rule per table, each defined once here.
        public delegate bool PoiReference(POIData poi, string key);

        public static readonly PoiReference PoiUsesCategory = (poi, key) => string.Equals(poi.category, key, StringComparison.Ordinal);
        public static readonly PoiReference PoiUsesBadge = (poi, key) => string.Equals(poi.badge_category, key, StringComparison.Ordinal);
        public static readonly PoiReference PoiUsesStatusLevel = (poi, key) => string.Equals(poi.status_level_key, key, StringComparison.Ordinal);
        public static readonly PoiReference PoiUsesHierarchyLevel = (poi, key) => string.Equals(poi.hierarchy_level_key, key, StringComparison.Ordinal);

        // A Keyword Field is referenced from inside a list: the POI holds a keyword list under that field key
        public static readonly PoiReference PoiUsesKeywordField = (poi, key) =>
            poi.search_keyword_fields != null &&
            poi.search_keyword_fields.Exists(f => f != null && string.Equals(f.field_key, key, StringComparison.Ordinal));

        // How many POIs still point at this key: what deleting its row would leave naming a row that no longer exists
        public static int CountReferences(IList<POIData> pois, PoiReference uses, string key)
        {
            if (pois == null || uses == null) return 0;
            int count = 0;
            foreach (var poi in pois)
                if (poi != null && uses(poi, key)) count++;
            return count;
        }
    }
}
