using System;
using System.Collections.Generic;

namespace TileStories
{
    // What a collect block counts and names (_3.1 Tier 3, step 8B), pure so every case is a unit test.
    //   - the wall's collectable items are counted FROM THE CONFIG: every collect block on every POI of the wall, unless the wall's
    //     Block Library switches the kind off -- never a number written into the code
    //   - the visitor's count is how many of those items they added (CardLocalState.Collected)
    //   - the item is named by the block's Item Name, else the point's card title; the series by the block's Series, else the
    //     point's category name (what the header's chip shows)
    public static class CollectRule
    {
        public readonly struct Item
        {
            public readonly string PoiId;
            public readonly string BlockKey;

            public Item(string poiId, string blockKey)
            {
                PoiId = poiId;
                BlockKey = blockKey;
            }
        }

        // Every collectable item of the wall, in POI then block order
        public static List<Item> Items(IReadOnlyList<POIData> pois, CardSettings settings)
        {
            var items = new List<Item>();
            if (pois == null || !BlockLibraryRule.IsEnabled(settings, BuiltInBlocks.CollectKind)) return items;
            foreach (var poi in pois)
            {
                var blocks = poi?.card?.blocks;
                if (blocks == null) continue;
                foreach (var block in blocks)
                    if (block != null && block.kind == BuiltInBlocks.CollectKind) items.Add(new Item(poi.id, block.key));
            }
            return items;
        }

        // How many of the wall's items the visitor has and how many there are. The block being shown always counts as one of the
        // wall's items (a card shown for a point the list does not hold still says 1 of 1, never 0 of 0).
        public static (int Have, int Total) Progress(IReadOnlyList<Item> items, string poiId, string blockKey, Func<string, string, bool> isCollected)
        {
            int have = 0;
            bool listed = false;
            foreach (var item in items)
            {
                if (item.PoiId == poiId && item.BlockKey == blockKey) listed = true;
                if (isCollected(item.PoiId, item.BlockKey)) have++;
            }
            int total = items.Count;
            if (!listed)
            {
                total++;
                if (isCollected(poiId, blockKey)) have++;
            }
            return (have, total);
        }

        // The item's name: the block's own, else the point's card title
        public static string ItemName(BlockFieldReader read, POIData poi, string language, string fallbackLanguage)
        {
            string own = read.Text(BuiltInBlocks.CollectItemNameField);
            return own.Length > 0 ? own : BlockStackBuilder.CardTitleOf(poi, language, fallbackLanguage);
        }

        // The series the item belongs to: the block's own label, else the point's category name
        public static string Series(BlockFieldReader read, POIData poi, WallConfigData taxonomy)
        {
            string own = read.Text(BuiltInBlocks.CollectSeriesField);
            return own.Length > 0 ? own : PoiSubtitle.Of(poi, taxonomy, withLevel: false);
        }
    }
}
