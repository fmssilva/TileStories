using System.Collections.Generic;
using UnityEngine;

namespace TileStories
{
    public static partial class CardGalleryDefinitions
    {
        // ---------------- wall_locator (Tier 2 group B) ----------------

        // Properties, not static fields: All = Build() above runs before any static field declared below it is set
        // (C# initialises static fields in text order), so a field read by Build would still be its default there.

        // The fabricated wall's POIs for wall_locator: x / z on the floor (the shown POI's own place comes from its entry)
        public static (string Id, string Name, float X, float Z)[] ShortWall => new[] { ("west_gate", "West Gate", -2f, 0f), ("east_tower", "East Tower", 3f, 0f) };

        // Where the visitor stands in the short entries (the wall's frame): 1 m right of the shown POI, 2 m in front
        public static Vector3 ShortViewer => new(1f, 1.6f, 2f);

        // The long wall runs at 30 degrees to world x (WallAxisRule finds its own axis)
        public static Vector3 LongWallDirection => new(0.8660254f, 0f, 0.5f);

        private static void AddWallLocators(List<Entry> list)
        {
            System.Action<WallConfigData> shortWall = wall =>
            {
                foreach (var (id, name, x, z) in ShortWall) wall.pois.Add(WallPoi(id, name, new Vector3(x, 1f, z), null));
            };
            // - twelve POIs along the angled wall at three heights (height never counts), the shown one in the middle (t = 0),
            //   long card titles on both sides of it
            System.Action<WallConfigData> longWall = wall =>
            {
                for (int t = -6; t <= 6; t++)
                {
                    if (t == 0) continue;
                    string title = t == -1 ? "The Royal Palace of the Kings by the river, before the earthquake"
                        : t == 1 ? "The chapel of Saint George with its bell tower and the old cemetery" : null;
                    wall.pois.Add(WallPoi("wall_" + (t + 6), "Point " + (t + 6), LongWallDirection * t + new Vector3(0f, (t % 3) * 0.5f, 0f), title));
                }
            };
            foreach (var variant in BuiltInBlocks.WallLocator.Variants)
            {
                bool strip = variant == BuiltInBlocks.WallLocatorStrip;
                list.Add(new Entry(BuiltInBlocks.WallLocatorKind, variant, "short", WallLocatorBlock(variant), At(Vector3.zero), shortWall,
                    strip ? ShortViewer : null));
                list.Add(new Entry(BuiltInBlocks.WallLocatorKind, variant, "long", WallLocatorBlock(variant), At(Vector3.zero), longWall,
                    strip ? LongWallDirection * 20f : null));
                // - strip: no viewer known (Phase A's editor view, a device before tracking); neighbours: the wall's left end
                list.Add(strip
                    ? new Entry(BuiltInBlocks.WallLocatorKind, variant, "noviewer", WallLocatorBlock(variant), At(Vector3.zero), shortWall)
                    : new Entry(BuiltInBlocks.WallLocatorKind, variant, "end", WallLocatorBlock(variant), At(new Vector3(-5f, 1f, 0f)), shortWall));
            }
        }

        private static BlockInstanceData WallLocatorBlock(string variant) =>
            new() { key = "block_2", kind = BuiltInBlocks.WallLocatorKind, variant = variant };

        private static System.Action<POIData> At(Vector3 position) => poi => poi.position = new PositionData { x = position.x, y = position.y, z = position.z };

        private static POIData WallPoi(string id, string name, Vector3 position, string headerTitle)
        {
            var poi = new POIData { id = id, name = name, category = CategoryKey, hierarchy_level_key = LevelKey };
            At(position)(poi);
            if (headerTitle != null)
            {
                var header = new BlockInstanceData { key = "block_1", kind = BuiltInBlocks.HeaderKind };
                header.fields.Add(Text(BlockStackBuilder.HeaderTitleField, headerTitle));
                poi.card.blocks.Add(header);
            }
            return poi;
        }


        // ---------------- today_map (Tier 2 group B) ----------------

        public const string TodayMapUrl = "https://maps.example.org/castle";
        public const float TodayMapLat = 38.7139f;
        public const float TodayMapLng = -9.1334f;

        private static void AddTodayMaps(List<Entry> list)
        {
            // - the shown POI's own header picture (a picture look): the bridge's "On the wall" side
            System.Action<POIData> withHeaderPicture = poi =>
            {
                poi.card.blocks[0].variant = BuiltInBlocks.HeaderImageParallax;
                poi.card.blocks[0].fields.Add(new BlockFieldValue { key = BuiltInBlocks.HeaderImageField, asset = "then.png" });
            };
            // - a picture stored on a TEXT-ONLY header: the card's header shows none, so the bridge shows none either
            System.Action<POIData> pictureOnATextHeader = poi =>
                poi.card.blocks[0].fields.Add(new BlockFieldValue { key = BuiltInBlocks.HeaderImageField, asset = "then.png" });
            foreach (var variant in BuiltInBlocks.TodayMap.Variants)
            {
                bool bridge = variant == BuiltInBlocks.TodayMapBridge;
                string map = bridge ? "now.png" : "wide.png";
                list.Add(new Entry(BuiltInBlocks.TodayMapKind, variant, "short", TodayMapBlock(variant, map, true, true, TodayMapUrl),
                    bridge ? withHeaderPicture : null));
                if (bridge)
                {
                    list.Add(new Entry(BuiltInBlocks.TodayMapKind, variant, "noheaderpicture", TodayMapBlock(variant, map, true, true, TodayMapUrl),
                        pictureOnATextHeader));
                    continue;
                }
                list.Add(new Entry(BuiltInBlocks.TodayMapKind, variant, "nourl", TodayMapBlock(variant, map, true, true, "")));
                list.Add(new Entry(BuiltInBlocks.TodayMapKind, variant, "nocoords", TodayMapBlock(variant, map, true, false, TodayMapUrl)));
                list.Add(new Entry(BuiltInBlocks.TodayMapKind, variant, "badurl", TodayMapBlock(variant, map, true, true, "javascript:alert(1)")));
                list.Add(new Entry(BuiltInBlocks.TodayMapKind, variant, "missing", TodayMapBlock(variant, MissingPicture, true, true, TodayMapUrl)));
            }
        }

        private static BlockInstanceData TodayMapBlock(string variant, string map, bool lat, bool lng, string url)
        {
            var block = new BlockInstanceData { key = "block_2", kind = BuiltInBlocks.TodayMapKind, variant = variant };
            block.fields.Add(new BlockFieldValue { key = BuiltInBlocks.TodayMapImageField, asset = map });
            if (lat) block.fields.Add(new BlockFieldValue { key = BuiltInBlocks.TodayMapLatField, number = TodayMapLat });
            if (lng) block.fields.Add(new BlockFieldValue { key = BuiltInBlocks.TodayMapLngField, number = TodayMapLng });
            if (url.Length > 0) block.fields.Add(new BlockFieldValue { key = BuiltInBlocks.TodayMapUrlField, value = url });
            return block;
        }


        // ---------------- related (Tier 2 group B) ----------------

        // A second category, for same_category / nearest: same_category leaves this one out, nearest keeps it
        public const string RelatedOtherCategoryKey = "category_2";

        private static void AddRelated(List<Entry> list)
        {
            // Manual: two named points, one a long card title; the block also names a point no longer on the wall and
            // itself, both left out by RelatedPoisRule.Pick
            System.Action<WallConfigData> manualWall = wall =>
            {
                wall.pois.Add(WallPoi("related_a", "North Tower", new Vector3(-2f, 0f, 0f), null));
                wall.pois.Add(WallPoi("related_b", "South Tower", new Vector3(2f, 0f, 0f),
                    "The chapel of Saint George with its bell tower and the old cemetery"));
            };
            // same_category / nearest: this point is at the wall's origin (0,0,0); a same-category point close by, one
            // far, and a CLOSER point of the OTHER category (same_category leaves it out, nearest keeps it -- and first)
            System.Action<WallConfigData> categoryWall = wall =>
            {
                wall.pois.Add(CategoryPoi("related_near", "Near Point", new Vector3(1f, 0f, 0f), CategoryKey));
                wall.pois.Add(CategoryPoi("related_far", "Far Point", new Vector3(4f, 0f, 0f), CategoryKey));
                wall.pois.Add(CategoryPoi("related_other", "Other Category Point", new Vector3(0.5f, 0f, 0f), RelatedOtherCategoryKey));
            };
            foreach (var variant in BuiltInBlocks.Related.Variants)
            {
                if (variant == BuiltInBlocks.RelatedCarousel)
                {
                    list.Add(new Entry(BuiltInBlocks.RelatedKind, variant, "manual",
                        RelatedBlock(variant, RelatedPoisRule.SourceManual, "related_a", "related_missing", "related_b"), null, manualWall));
                    list.Add(new Entry(BuiltInBlocks.RelatedKind, variant, "samecategory",
                        RelatedBlock(variant, RelatedPoisRule.SourceSameCategory), null, categoryWall));
                    list.Add(new Entry(BuiltInBlocks.RelatedKind, variant, "nearest",
                        RelatedBlock(variant, RelatedPoisRule.SourceNearest), null, categoryWall));
                    continue;
                }
                // next_along_wall: the shown point between its two picks (right, no wrap), and past both (wraps left)
                list.Add(new Entry(BuiltInBlocks.RelatedKind, variant, "manual",
                    RelatedBlock(variant, RelatedPoisRule.SourceManual, "related_a", "related_b"), null, manualWall));
                list.Add(new Entry(BuiltInBlocks.RelatedKind, variant, "wrap",
                    RelatedBlock(variant, RelatedPoisRule.SourceManual, "related_a", "related_b"), At(new Vector3(5f, 0f, 0f)), manualWall));
            }
        }

        private static BlockInstanceData RelatedBlock(string variant, string source, params string[] manualIds)
        {
            var block = new BlockInstanceData { key = "block_2", kind = BuiltInBlocks.RelatedKind, variant = variant };
            block.fields.Add(new BlockFieldValue { key = BuiltInBlocks.RelatedSourceField, value = source });
            if (manualIds.Length > 0)
            {
                var field = new BlockFieldValue { key = BuiltInBlocks.RelatedItemsField };
                foreach (string id in manualIds) field.items.Add(Item(new BlockItemFieldValue { key = BuiltInBlocks.RelatedPoiField, value = id }));
                block.fields.Add(field);
            }
            return block;
        }

        private static POIData CategoryPoi(string id, string name, Vector3 position, string category)
        {
            var poi = new POIData { id = id, name = name, category = category, hierarchy_level_key = LevelKey };
            At(position)(poi);
            return poi;
        }


        // ---------------- practical_info ----------------

        private static void AddPracticalInfo(List<Entry> list)
        {
            var shortRows = new[] { (CardIcons.Time, "Open", "Every day, 9:00 to 21:00"), (CardIcons.Ticket, "Tickets", "15 EUR") };
            // - every icon of the set once
            var longRows = new[]
            {
                (CardIcons.Time, "Open", "Every day, 9:00 to 21:00 from March to October, and 9:00 to 18:00 in winter"),
                (CardIcons.Ticket, "Tickets", "15 EUR, free for children under 12 and for residents on Sunday mornings"),
                (CardIcons.Access, "Getting in", "Steep cobbled streets; tram 28 stops at the gate"),
                (CardIcons.Location, "Where", "Upper old town, above the river"),
                (CardIcons.Light, "Best light", "Late afternoon, from the west terrace"),
                (CardIcons.Info, "Good to know", "The walls are open to the wind: bring a jacket"),
            };
            // - no icon (the words stay in line), an action's icon (not offered here: drawn as none), no label (left out), no value
            var partialRows = new[] { ("", "Open", "Every day"), (CardIcons.ShowOnWall, "Tickets", "15 EUR"), (CardIcons.Info, "", "No label"), (CardIcons.Location, "Where", "") };
            foreach (var variant in BuiltInBlocks.PracticalInfo.Variants)
            {
                list.Add(new Entry(BuiltInBlocks.PracticalInfoKind, variant, "short", PracticalBlock(variant, shortRows)));
                list.Add(new Entry(BuiltInBlocks.PracticalInfoKind, variant, "long", PracticalBlock(variant, longRows)));
                list.Add(new Entry(BuiltInBlocks.PracticalInfoKind, variant, "partial", PracticalBlock(variant, partialRows)));
            }
        }

        private static BlockInstanceData PracticalBlock(string variant, (string Icon, string Label, string Value)[] rows)
        {
            var block = new BlockInstanceData { key = "block_2", kind = BuiltInBlocks.PracticalInfoKind, variant = variant };
            var field = new BlockFieldValue { key = BuiltInBlocks.PracticalInfoItemsField };
            foreach (var (icon, label, value) in rows)
                field.items.Add(Item(new BlockItemFieldValue { key = BuiltInBlocks.PracticalInfoIconField, value = icon },
                    ItemText(BuiltInBlocks.PracticalInfoLabelField, label), ItemText(BuiltInBlocks.PracticalInfoValueField, value)));
            block.fields.Add(field);
            return block;
        }

    }
}
