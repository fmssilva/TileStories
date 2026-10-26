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
        // Not a taxonomy table either: a wall's navigation Groups (_3.2 section 3), which a Families row points at by key
        public const string GroupPrefix = "group_";

        // The key of a NEW block of `card` (_3.1 5.2: block_N, never reused): one past the highest N the card ever handed out
        // (last_block_number) or any of its blocks holds now (a hand-edited file, a file older than the memory), whichever is higher --
        // NextFree alone gave a deleted LAST key out again. Records the number it hands out
        public static string TakeBlockKey(POICardData card)
        {
            int highest = card.last_block_number;
            if (card.blocks != null)
                foreach (var block in card.blocks)
                    highest = Math.Max(highest, BlockNumber(block?.key));
            card.last_block_number = highest + 1;
            return BlockPrefix + card.last_block_number;
        }

        // Bring every card's key memory up to the highest block_N it holds (a file written before the memory existed, or edited by
        // hand): without it, deleting the last block of a card loaded that way handed its key out again. Called once on load, never while
        // drawing. Returns how many cards it raised
        public static int SeedBlockKeyMemory(WallConfigData config)
        {
            if (config?.pois == null) return 0;
            int raised = 0;
            foreach (var poi in config.pois)
            {
                if (poi?.card?.blocks == null) continue;
                int highest = poi.card.last_block_number;
                foreach (var block in poi.card.blocks) highest = Math.Max(highest, BlockNumber(block?.key));
                if (highest == poi.card.last_block_number) continue;
                poi.card.last_block_number = highest;
                raised++;
            }
            return raised;
        }

        // The N of a key block_N, 0 for anything else
        public static int BlockNumber(string key)
        {
            if (string.IsNullOrEmpty(key) || !key.StartsWith(BlockPrefix, StringComparison.Ordinal)) return 0;
            return int.TryParse(key.Substring(BlockPrefix.Length), System.Globalization.NumberStyles.None,
                System.Globalization.CultureInfo.InvariantCulture, out int n) ? n : 0;
        }

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
