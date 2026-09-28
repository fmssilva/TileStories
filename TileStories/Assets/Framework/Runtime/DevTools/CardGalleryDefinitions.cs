using System.Collections.Generic;
using UnityEngine;

namespace TileStories
{
    // Phase A data list of the POI Detail Card gallery (_3.1 section 7, 40-testing.md 4.4), fabricated content -- no AR,
    // no config.json. The ONE list both CardGalleryHarness (to look at) and CardGalleryTests (to assert) read, so the two
    // cannot drift apart:
    //   - header entries: every header variant x content (short, long, no subtitle) x stop
    //   - block entries: every other kind x variant x content (short, long, and the states that kind has: an optional
    //     field left empty, unknown...), each shown at the full stop under a short header
    public static class CardGalleryDefinitions
    {
        public readonly struct Entry
        {
            public readonly string Name;
            public readonly string Kind;
            public readonly string Variant;
            public readonly string Content;
            public readonly SheetStopRule.Stop Stop;
            // Header entries: the header's own texts
            public readonly string Title;
            public readonly string Subtitle;
            // Block entries: the block shown under the header, and what the POI must hold for it (a status...)
            public readonly BlockInstanceData Block;
            public readonly System.Action<POIData> Setup;
            // Block entries: a change to the fabricated wall this entry needs (e.g. no Outline Types rows)
            public readonly System.Action<WallConfigData> WallSetup;
            // Where the visitor stands in the wall's frame (wall_locator's "You are here"); null = no viewer known
            public readonly Vector3? Viewer;

            public bool IsHeader => Kind == BuiltInBlocks.HeaderKind;

            public Entry(string variant, string content, string title, string subtitle, SheetStopRule.Stop stop)
            {
                Name = "header_" + variant + "_" + content + "_" + stop.ToString().ToLowerInvariant();
                Kind = BuiltInBlocks.HeaderKind;
                Variant = variant;
                Content = content;
                Stop = stop;
                Title = title;
                Subtitle = subtitle;
                Block = null;
                Setup = null;
                WallSetup = null;
                Viewer = null;
            }

            public Entry(string kind, string variant, string content, BlockInstanceData block, System.Action<POIData> setup = null,
                System.Action<WallConfigData> wallSetup = null, Vector3? viewer = null)
            {
                Name = kind + "_" + variant + "_" + content;
                Kind = kind;
                Variant = variant;
                Content = content;
                Stop = SheetStopRule.Stop.Full;
                Title = "Gate";
                Subtitle = "";
                Block = block;
                Setup = setup;
                WallSetup = wallSetup;
                Viewer = viewer;
            }
        }

        public const string CategoryKey = "category_1";
        public const string LevelKey = "level_1";

        public static readonly IReadOnlyList<Entry> All = Build();

        private static List<Entry> Build()
        {
            var list = new List<Entry>();
            AddHeaders(list);
            AddStatus(list);
            AddRichText(list);
            AddQuickFacts(list);
            AddFunFacts(list);
            AddPullQuotes(list);
            AddProcessSteps(list);
            AddSwatches(list);
            AddTimelines(list);
            AddPeople(list);
            AddStoryChapters(list);
            AddComparePoints(list);
            AddPracticalInfo(list);
            AddSources(list);
            AddActions(list);
            AddGalleries(list);
            AddBeforeAfter(list);
            AddZoomImages(list);
            AddHotspots(list);
            AddWallLocators(list);
            AddTodayMaps(list);
            AddRelated(list);
            AddKnowledgeChecks(list);
            AddPolls(list);
            AddCollects(list);
            AddFeedback(list);
            AddDialogues(list);
            AddShowOnWall(list);
            AddHeadings(list);
            return list;
        }

        // The heading every block may have (the stack draws it, _3.1 step 6C): the first entry of each kind x variant
        // carries one, so every kind is seen with a heading above it and without one. A kind with a default heading
        // (compare, sources) gets it on its "long" entry instead: there the authored heading must win over the default.
        public const string HeadingPrefix = "About the ";

        private static void AddHeadings(List<Entry> list)
        {
            var seen = new HashSet<string>();
            foreach (var entry in list)
            {
                if (entry.IsHeader) continue;
                bool hasDefault = BlockRegistry.Shared.TryGet(entry.Kind, out var kind) && kind.DefaultHeadingKey != null;
                if (hasDefault && entry.Content != "long") continue;
                if (!seen.Add(entry.Kind + "/" + entry.Variant)) continue;
                if (entry.Block.fields.Exists(f => f.key == BlockKindDefinition.HeadingField)) continue;
                entry.Block.fields.Add(Text(BlockKindDefinition.HeadingField, HeadingPrefix + entry.Kind.Replace('_', ' ')));
            }
        }

        // Whether this entry's block has an authored heading
        public static bool HasHeading(Entry entry) =>
            !entry.IsHeader && entry.Block.fields.Exists(f => f.key == BlockKindDefinition.HeadingField);

        private static void AddHeaders(List<Entry> list)
        {
            var contents = new[]
            {
                ("short", "Gate", "1640"),
                ("long", "The Royal Palace of the Kings of Portugal and of the Algarves, before the earthquake",
                    "Seen from the river on the panel, with the Customs House, the chapel and the long arcade that the fire destroyed"),
                ("nosubtitle", "Old Cathedral", ""),
            };
            foreach (var variant in BuiltInBlocks.Header.Variants)
                foreach (var (content, title, subtitle) in contents)
                    foreach (var stop in new[] { SheetStopRule.Stop.Peek, SheetStopRule.Stop.Half, SheetStopRule.Stop.Full })
                        list.Add(new Entry(variant, content, title, subtitle, stop));
            // - a picture look with no picture written: the text-only look, no hero
            foreach (string variant in BuiltInBlocks.HeaderImageVariants)
                list.Add(new Entry(variant, NoPicture, "Gate", "1640", SheetStopRule.Stop.Full));
        }

        // ---------------- pictures (Tier 2) ----------------

        public const string NoPicture = "nopicture";

        // The gallery's pictures by name: made in memory by CardGalleryMedia, each its own colour (a render pixel tells them
        // apart) and size (landscape, portrait, square). Any other name is a missing file.
        public static readonly IReadOnlyDictionary<string, (Vector2Int Size, Color Colour)> Pictures = new Dictionary<string, (Vector2Int, Color)>
        {
            ["wide.png"] = (new Vector2Int(768, 432), new Color(0.12f, 0.25f, 0.56f)),
            ["then.png"] = (new Vector2Int(512, 384), new Color(0.72f, 0.52f, 0.18f)),
            ["now.png"] = (new Vector2Int(512, 384), new Color(0.20f, 0.45f, 0.70f)),
            ["one.png"] = (new Vector2Int(512, 384), new Color(0.70f, 0.20f, 0.18f)),
            ["two.png"] = (new Vector2Int(512, 384), new Color(0.20f, 0.55f, 0.30f)),
            ["three.png"] = (new Vector2Int(384, 512), new Color(0.45f, 0.25f, 0.45f)),
            ["four.png"] = (new Vector2Int(512, 384), new Color(0.85f, 0.66f, 0.23f)),
            ["before.png"] = (new Vector2Int(512, 384), new Color(0.60f, 0.35f, 0.20f)),
            ["after.png"] = (new Vector2Int(512, 384), new Color(0.25f, 0.35f, 0.60f)),
            ["detail.png"] = (new Vector2Int(1024, 1024), new Color(0.15f, 0.30f, 0.50f)),
        };

        // Where a spotlight entry's focus is and how much it enlarges
        public static readonly Vector2 SpotlightFocus = new(0.25f, 0.4f);
        public const float SpotlightZoom = 2.5f;

        private static void AddPictureFields(BlockInstanceData header, string variant)
        {
            header.fields.Add(new BlockFieldValue { key = BuiltInBlocks.HeaderImageField, asset = variant == BuiltInBlocks.HeaderSplitThenNow ? "then.png" : "wide.png" });
            if (variant == BuiltInBlocks.HeaderSplitThenNow)
                header.fields.Add(new BlockFieldValue { key = BuiltInBlocks.HeaderSecondImageField, asset = "now.png" });
            if (variant != BuiltInBlocks.HeaderSpotlightCrop) return;
            header.fields.Add(new BlockFieldValue { key = BuiltInBlocks.HeaderFocusXField, number = SpotlightFocus.x });
            header.fields.Add(new BlockFieldValue { key = BuiltInBlocks.HeaderFocusYField, number = SpotlightFocus.y });
            header.fields.Add(new BlockFieldValue { key = BuiltInBlocks.HeaderZoomField, number = SpotlightZoom });
        }

        private static void AddGalleries(List<Entry> list)
        {
            var shortRows = new[] { ("one.png", "The gate from the square", ""), ("two.png", "The river front", "") };
            var longRows = new[]
            {
                ("one.png", "The gate from the square, with the customs house and the long arcade the fire destroyed in 1755", "Photo: Municipal archive, catalogue 12/447"),
                ("two.png", "The river front", "Photo: Municipal archive"),
                ("three.png", "The chapel tower (portrait)", ""),
                ("four.png", "The walls at dusk", "Photo: a visitor, shared under CC BY"),
            };
            // - outside the folder, not a picture, a picture file that is not there: the first two rows are left out, the
            //   third shows its "picture unavailable" frame
            var partialRows = new[] { ("one.png", "Kept", ""), ("Assets/Art/outside.png", "Outside the folder", ""), ("clip.mp3", "Not a picture", ""), ("ghost.png", "No such file", "") };
            foreach (var variant in BuiltInBlocks.Gallery.Variants)
            {
                list.Add(new Entry(BuiltInBlocks.GalleryKind, variant, "short", GalleryBlock(variant, shortRows)));
                list.Add(new Entry(BuiltInBlocks.GalleryKind, variant, "long", GalleryBlock(variant, longRows)));
                list.Add(new Entry(BuiltInBlocks.GalleryKind, variant, "partial", GalleryBlock(variant, partialRows)));
            }
        }

        private static BlockInstanceData GalleryBlock(string variant, (string Image, string Caption, string Credit)[] rows)
        {
            var block = new BlockInstanceData { key = "block_2", kind = BuiltInBlocks.GalleryKind, variant = variant };
            var field = new BlockFieldValue { key = BuiltInBlocks.GalleryItemsField };
            foreach (var (image, caption, credit) in rows)
                field.items.Add(Item(new BlockItemFieldValue { key = BuiltInBlocks.GalleryImageField, asset = image },
                    ItemText(BuiltInBlocks.GalleryCaptionField, caption), ItemText(BuiltInBlocks.GalleryCreditField, credit)));
            block.fields.Add(field);
            return block;
        }

        private static void AddBeforeAfter(List<Entry> list)
        {
            foreach (var variant in BuiltInBlocks.BeforeAfter.Variants)
            {
                list.Add(new Entry(BuiltInBlocks.BeforeAfterKind, variant, "short", BeforeAfterBlock(variant, "after.png", null, null, null)));
                list.Add(new Entry(BuiltInBlocks.BeforeAfterKind, variant, "labels", BeforeAfterBlock(variant, "after.png", "1740, before the earthquake", "Today", 0.3f)));
                list.Add(new Entry(BuiltInBlocks.BeforeAfterKind, variant, "missing", BeforeAfterBlock(variant, "ghost.png", null, null, null)));
            }
        }

        private static BlockInstanceData BeforeAfterBlock(string variant, string after, string beforeLabel, string afterLabel, float? start)
        {
            var block = new BlockInstanceData { key = "block_2", kind = BuiltInBlocks.BeforeAfterKind, variant = variant };
            block.fields.Add(new BlockFieldValue { key = BuiltInBlocks.BeforeAfterBeforeField, asset = "before.png" });
            block.fields.Add(new BlockFieldValue { key = BuiltInBlocks.BeforeAfterAfterField, asset = after });
            if (beforeLabel != null) block.fields.Add(Text(BuiltInBlocks.BeforeAfterBeforeLabelField, beforeLabel));
            if (afterLabel != null) block.fields.Add(Text(BuiltInBlocks.BeforeAfterAfterLabelField, afterLabel));
            if (start.HasValue) block.fields.Add(new BlockFieldValue { key = BuiltInBlocks.BeforeAfterStartField, number = start.Value });
            return block;
        }

        private static void AddZoomImages(List<Entry> list)
        {
            foreach (var variant in BuiltInBlocks.ZoomImage.Variants)
            {
                var withCaption = new BlockInstanceData { key = "block_2", kind = BuiltInBlocks.ZoomImageKind, variant = variant };
                withCaption.fields.Add(new BlockFieldValue { key = BuiltInBlocks.ZoomImageImageField, asset = "detail.png" });
                withCaption.fields.Add(Text(BuiltInBlocks.ZoomImageCaptionField, "Pinch into the panel to see each brush stroke of the painter's cobalt"));
                list.Add(new Entry(BuiltInBlocks.ZoomImageKind, variant, "short", withCaption));
                var wide = new BlockInstanceData { key = "block_2", kind = BuiltInBlocks.ZoomImageKind, variant = variant };
                wide.fields.Add(new BlockFieldValue { key = BuiltInBlocks.ZoomImageImageField, asset = "wide.png" });
                list.Add(new Entry(BuiltInBlocks.ZoomImageKind, variant, "nocaption", wide));
            }
        }

        // ---------------- hotspot_image (Tier 2 group B) ----------------

        // A picture name no file has: the "missing media" content of a picture block
        public const string MissingPicture = "gone.png";

        private static void AddHotspots(List<Entry> list)
        {
            var shortSpots = new[] { (0.3f, 0.4f, "The coat of arms", "Carved over the gate in 1640."), (0.7f, 0.6f, "The bell", "") };
            var longSpots = new[]
            {
                (0.2f, 0.15f, "The coat of arms of the kings of Portugal and of the Algarves, over the gate",
                    "Carved over the gate in 1640, the year the kingdom took back its crown. It hangs on the [[keep]], the last refuge." +
                    "\n\nThe painter drew every quartering of the shield, even the small castles of the border."),
                (0.8f, 0.2f, "The bell tower", "Rebuilt after the earthquake, taller than before."),
                (0.5f, 0.5f, "The chapel", "The oldest part of the building."),
                (0.25f, 0.85f, "The river gate", "Ships unloaded here until the quay was built."),
                (0.75f, 0.8f, "The customs house", "Burned in the fire of 1755."),
            };
            // - a row with no title (left out: the next one is still 2), a spot with no text, spots on the picture's corners
            var partialSpots = new[] { (0f, 0f, "Top left corner", "At the picture's very edge."), (0.5f, 0.5f, "", "No title: not shown"),
                (1f, 1f, "Bottom right corner", "") };
            foreach (var variant in BuiltInBlocks.HotspotImage.Variants)
            {
                list.Add(new Entry(BuiltInBlocks.HotspotImageKind, variant, "short", HotspotBlock(variant, "wide.png", shortSpots)));
                list.Add(new Entry(BuiltInBlocks.HotspotImageKind, variant, "long", HotspotBlock(variant, "three.png", longSpots)));
                list.Add(new Entry(BuiltInBlocks.HotspotImageKind, variant, "partial", HotspotBlock(variant, "one.png", partialSpots)));
                list.Add(new Entry(BuiltInBlocks.HotspotImageKind, variant, "missing", HotspotBlock(variant, MissingPicture, shortSpots)));
            }
        }

        private static BlockInstanceData HotspotBlock(string variant, string picture, (float X, float Y, string Title, string Text)[] spots)
        {
            var block = new BlockInstanceData { key = "block_2", kind = BuiltInBlocks.HotspotImageKind, variant = variant };
            block.fields.Add(new BlockFieldValue { key = BuiltInBlocks.HotspotImageField, asset = picture });
            var field = new BlockFieldValue { key = BuiltInBlocks.HotspotItemsField };
            foreach (var (x, y, title, text) in spots)
                field.items.Add(Item(new BlockItemFieldValue { key = BuiltInBlocks.HotspotXField, number = x },
                    new BlockItemFieldValue { key = BuiltInBlocks.HotspotYField, number = y },
                    ItemText(BuiltInBlocks.HotspotTitleField, title), ItemText(BuiltInBlocks.HotspotTextField, text)));
            block.fields.Add(field);
            return block;
        }

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

        // ---------------- knowledge_check (Tier 3 group A) ----------------

        // One authored question row: the question, the option texts and pictures (slot 1, 2...), the right slot (1-based; 0 = none
        // written), whether a true / false statement is true, and the explanation
        private readonly struct QuestionRow
        {
            public readonly string Question;
            public readonly string[] Options;
            public readonly string[] Images;
            public readonly int Right;
            public readonly bool IsTrue;
            public readonly string Explanation;

            public QuestionRow(string question, string[] options, string[] images, int right, bool isTrue, string explanation)
            {
                Question = question;
                Options = options;
                Images = images;
                Right = right;
                IsTrue = isTrue;
                Explanation = explanation;
            }
        }

        // - a property, not a field: `All = Build()` above runs first, so a static field declared down here would still be null
        private static string[] None => System.Array.Empty<string>();

        // The short question the promise tests answer, right and wrong
        public const string KeepQuestion = "What is the tallest tower of a castle called?";
        public const string KeepExplanation = "The keep is the strongest tower: the last refuge when the walls fell.";
        public const string CurtainStatement = "A curtain wall joins the towers of a castle.";
        public const string CurtainExplanation = "Yes: the curtain wall runs between the towers and closes the ring.";
        public const string PanelQuestion = "Which picture shows the panel before the earthquake?";
        public const string PanelExplanation = "The earlier picture still has the whole arcade standing.";
        // The block key of every knowledge_check entry: the answers are stored under it
        public const string QuizBlockKey = "block_2";

        private static void AddKnowledgeChecks(List<Entry> list)
        {
            foreach (var variant in BuiltInBlocks.KnowledgeCheck.Variants)
                foreach (string content in new[] { "short", "long", "partial" })
                    list.Add(new Entry(BuiltInBlocks.KnowledgeCheckKind, variant, content, KnowledgeBlock(variant, KnowledgeRows(variant, content))));
        }

        private static QuestionRow[] KnowledgeRows(string variant, string content)
        {
            bool tf = variant == BuiltInBlocks.KnowledgeCheckTrueFalseSwipe;
            bool pictures = variant == BuiltInBlocks.KnowledgeCheckImageChoice;
            if (content == "short")
                return new[]
                {
                    tf ? new QuestionRow(CurtainStatement, None, None, 0, true, CurtainExplanation)
                    : pictures ? new QuestionRow(PanelQuestion, new[] { "Before", "After" }, new[] { "before.png", "after.png" }, 1, false, PanelExplanation)
                    : new QuestionRow(KeepQuestion, new[] { "The curtain wall", "The keep", "The gatehouse" }, None, 2, false, KeepExplanation),
                };
            if (content == "long")
                return LongRows(tf, pictures);
            // - "partial": one complete question and rows the look must leave out (no explanation, too few options, a right option
            //   that is not shown, no question)
            const string good = "Complete question.";
            const string why = "Because it is complete.";
            if (tf)
                return new[]
                {
                    new QuestionRow(good, None, None, 0, false, why),
                    new QuestionRow("No explanation written.", None, None, 0, true, ""),
                    new QuestionRow("", None, None, 0, true, why),
                };
            if (pictures)
                return new[]
                {
                    new QuestionRow(good, new[] { "One", "Two" }, new[] { "one.png", "two.png" }, 2, false, why),
                    new QuestionRow("Only one picture.", new[] { "One" }, new[] { "one.png" }, 1, false, why),
                    new QuestionRow("The right one has no picture.", new[] { "One", "Two", "Three" }, new[] { "one.png", "two.png" }, 3, false, why),
                };
            return new[]
            {
                new QuestionRow(good, new[] { "Wrong", "Right", "Also wrong" }, None, 2, false, why),
                new QuestionRow("No explanation written.", new[] { "A", "B" }, None, 1, false, ""),
                new QuestionRow("Only one option.", new[] { "A" }, None, 1, false, why),
                new QuestionRow("The right option is blank.", new[] { "A", "B" }, None, 4, false, why),
            };
        }

        // Three questions with long words, each look: the first is the one the promise tests answer
        private static QuestionRow[] LongRows(bool tf, bool pictures)
        {
            const string longQuestion = "Long ago the castle on the hill was rebuilt again and again, after every siege and every earthquake; which of these is the " +
                                        "tallest and strongest tower, the one the defenders kept as their last refuge when everything else had fallen?";
            const string longWhy = "The keep is the strongest tower of a castle. It stood apart from the walls, so the defenders could hold it even when the " +
                                   "walls and the other towers had been taken, and the painter of the panel drew it larger than all the rest.";
            if (tf)
                return new[]
                {
                    new QuestionRow("The keep of a castle was usually built on the lowest ground, far from the walls, so that the enemy could not see it from the towers.",
                        None, None, 0, false, longWhy),
                    new QuestionRow("Biscuit is clay fired once, hard but not yet glazed.", None, None, 0, true, "Yes: the first firing makes the clay hard, the glaze comes after."),
                    new QuestionRow("The panel was made after the earthquake of 1755.", None, None, 0, false, "No: it shows the city as it was before, which is why it matters."),
                };
            if (pictures)
                return new[]
                {
                    new QuestionRow(longQuestion, new[] { "The panel as first painted, with the arcade", "The panel today, with its missing tiles", "The keep seen from the river", "The old cathedral" },
                        new[] { "one.png", "two.png", "three.png", "four.png" }, 3, false, longWhy),
                    new QuestionRow("Which picture is the after one?", new[] { "Before", "After" }, new[] { "before.png", "after.png" }, 2, false, PanelExplanation),
                    new QuestionRow("Which of these is portrait?", new[] { "One", "Two", "Three" }, new[] { "one.png", "two.png", "three.png" }, 3, false, "The third picture is taller than it is wide."),
                };
            return new[]
            {
                new QuestionRow(longQuestion,
                    new[] { "The curtain wall that joins the towers all around the hill", "The keep, the tallest and strongest tower of the castle",
                            "The gatehouse with its two round towers and the old drawbridge" }, None, 2, false, longWhy),
                new QuestionRow("Which of these was NOT part of the old palace?", new[] { "The chapel", "The arcade", "The lighthouse", "The customs house" }, None, 3,
                    false, "The palace stood on the riverside with a chapel, an arcade and the customs house; there was no lighthouse."),
                new QuestionRow("What is biscuit?", new[] { "Clay fired once, hard but not yet glazed", "Clay that is glazed but not fired" }, None, 1, false,
                    "Biscuit is the first firing of a tile, before any glaze."),
            };
        }

        private static BlockInstanceData KnowledgeBlock(string variant, QuestionRow[] rows, bool showAfterViewed = false)
        {
            var block = new BlockInstanceData { key = QuizBlockKey, kind = BuiltInBlocks.KnowledgeCheckKind, variant = variant };
            var questions = new BlockFieldValue { key = BuiltInBlocks.KnowledgeCheckQuestionsField };
            foreach (var row in rows)
            {
                var item = Item(ItemText(BuiltInBlocks.KnowledgeCheckQuestionField, row.Question), ItemText(BuiltInBlocks.KnowledgeCheckExplanationField, row.Explanation));
                for (int i = 0; i < row.Options.Length; i++) item.fields.Add(ItemText(KnowledgeCheckRule.OptionField(i + 1), row.Options[i]));
                for (int i = 0; i < row.Images.Length; i++) item.fields.Add(new BlockItemFieldValue { key = KnowledgeCheckRule.ImageField(i + 1), asset = row.Images[i] });
                if (row.Right > 0) item.fields.Add(new BlockItemFieldValue { key = BuiltInBlocks.KnowledgeCheckCorrectField, value = row.Right.ToString() });
                if (row.IsTrue) item.fields.Add(new BlockItemFieldValue { key = BuiltInBlocks.KnowledgeCheckIsTrueField, flag = true });
                questions.items.Add(item);
            }
            block.fields.Add(questions);
            if (showAfterViewed) block.fields.Add(new BlockFieldValue { key = BuiltInBlocks.KnowledgeCheckShowAfterViewedField, flag = true });
            return block;
        }

        // The POI of the gated question: a header, a long text, then a multiple-choice question that waits until the card was read
        // (show_after_viewed). `longText` false: the text is one short paragraph, so all of it fits on a card at full
        public const string GatedPoiId = "gated_knowledge_check";
        public const string GatedQuizBlockKey = "block_3";

        public static POIData GatedKnowledgePoi(bool longText)
        {
            var poi = new POIData { id = GatedPoiId, name = "Gate", category = CategoryKey, hierarchy_level_key = LevelKey };
            var header = new BlockInstanceData { key = "block_1", kind = BuiltInBlocks.HeaderKind, variant = BuiltInBlocks.HeaderTextOnly };
            header.fields.Add(Text(BlockStackBuilder.HeaderTitleField, "Gate"));
            poi.card.blocks.Add(header);
            var text = RichText(BuiltInBlocks.RichTextPlain, longText ? LongText + "\n\n" + LongText + "\n\n" + LongText : "Built on the hill.", false);
            text.key = "block_2";
            poi.card.blocks.Add(text);
            var quiz = KnowledgeBlock(BuiltInBlocks.KnowledgeCheckMultipleChoice, KnowledgeRows(BuiltInBlocks.KnowledgeCheckMultipleChoice, "short"), showAfterViewed: true);
            quiz.key = GatedQuizBlockKey;
            poi.card.blocks.Add(quiz);
            return poi;
        }

        // ---------------- feedback (Tier 3 group A) ----------------

        public const string FeedbackQuestion = "Was this description useful?";
        public const string FeedbackLongQuestion = "You have just read what the panel shows and how it was made, from the arcade to the river; how well did this description help you to understand what you saw?";

        private static void AddFeedback(List<Entry> list)
        {
            foreach (var variant in BuiltInBlocks.Feedback.Variants)
            {
                list.Add(new Entry(BuiltInBlocks.FeedbackKind, variant, "short", FeedbackBlock(variant, FeedbackQuestion)));
                list.Add(new Entry(BuiltInBlocks.FeedbackKind, variant, "long", FeedbackBlock(variant, FeedbackLongQuestion)));
                // - no question written: the card asks its own, in its own words
                list.Add(new Entry(BuiltInBlocks.FeedbackKind, variant, "noquestion", FeedbackBlock(variant, null)));
            }
        }

        private static BlockInstanceData FeedbackBlock(string variant, string question)
        {
            var block = new BlockInstanceData { key = QuizBlockKey, kind = BuiltInBlocks.FeedbackKind, variant = variant };
            if (question != null) block.fields.Add(Text(BuiltInBlocks.FeedbackQuestionField, question));
            return block;
        }

        // ---------------- poll, collect, dialogue, show_on_wall (Tier 3 group B) ----------------

        public const string PollQuestion = "Which side of the wall would you visit first?";
        public const string PollLongQuestion = "You have seen how the panel was made and what the earthquake left standing; which part of the whole story would you most like the museum to tell in more detail next season?";
        public static string[] PollShortOptions => new[] { "The arcade", "The river gate", "The bell tower" };
        public static string[] PollLongOptions => new[]
        {
            "The long arcade with its many arches and the market that once stood under it",
            "The river gate", "The bell tower", "The cloister", "The old cemetery beside the chapel of Saint George", "The kitchen garden",
        };

        private static void AddPolls(List<Entry> list)
        {
            foreach (var variant in BuiltInBlocks.Poll.Variants)
            {
                list.Add(new Entry(BuiltInBlocks.PollKind, variant, "short", PollBlock(variant, PollQuestion, PollShortOptions)));
                list.Add(new Entry(BuiltInBlocks.PollKind, variant, "long", PollBlock(variant, PollLongQuestion, PollLongOptions)));
                // - "partial": blank rows are left out; the two with words are still a poll
                list.Add(new Entry(BuiltInBlocks.PollKind, variant, "partial", PollBlock(variant, PollQuestion, "The arcade", "", "The river gate", "  ")));
            }
        }

        private static BlockInstanceData PollBlock(string variant, string question, params string[] options)
        {
            var block = new BlockInstanceData { key = QuizBlockKey, kind = BuiltInBlocks.PollKind, variant = variant };
            block.fields.Add(Text(BuiltInBlocks.PollQuestionField, question));
            var rows = new List<BlockItemData>();
            foreach (string option in options) rows.Add(Item(ItemText(BuiltInBlocks.PollOptionTextField, option)));
            block.fields.Add(new BlockFieldValue { key = BuiltInBlocks.PollOptionsField, items = rows });
            return block;
        }

        public const string CollectItem = "Stamp of the old gate";
        public const string CollectSeries = "Gates and towers";

        // Three more collectable points on the fabricated wall: the wall's total is then 4 (the shown point + these)
        private static void ThreeMoreCollectables(WallConfigData wall)
        {
            for (int i = 1; i <= 3; i++)
            {
                var poi = WallPoi("collect_" + i, "Collectable " + i, new Vector3(i, 0f, 0f), null);
                poi.card.blocks.Add(new BlockInstanceData { key = "block_9", kind = BuiltInBlocks.CollectKind, variant = BuiltInBlocks.CollectAddToStory });
                wall.pois.Add(poi);
            }
        }

        private static void AddCollects(List<Entry> list)
        {
            foreach (var variant in BuiltInBlocks.Collect.Variants)
            {
                list.Add(new Entry(BuiltInBlocks.CollectKind, variant, "short", CollectBlock(variant, CollectItem, CollectSeries), null, ThreeMoreCollectables));
                // - nothing written: the point's card title and its category name
                list.Add(new Entry(BuiltInBlocks.CollectKind, variant, "defaults", CollectBlock(variant, null, null), null, ThreeMoreCollectables));
                list.Add(new Entry(BuiltInBlocks.CollectKind, variant, "long",
                    CollectBlock(variant, "Stamp of the great river gate of the Royal Palace of the Kings of Portugal and of the Algarves",
                        "The gates, towers, arcades and cloisters of the palace by the river"), null, ThreeMoreCollectables));
                // - the only collectable on its wall: 0 of 1
                list.Add(new Entry(BuiltInBlocks.CollectKind, variant, "alone", CollectBlock(variant, CollectItem, CollectSeries)));
            }
        }

        private static BlockInstanceData CollectBlock(string variant, string itemName, string series)
        {
            var block = new BlockInstanceData { key = QuizBlockKey, kind = BuiltInBlocks.CollectKind, variant = variant };
            if (itemName != null) block.fields.Add(Text(BuiltInBlocks.CollectItemNameField, itemName));
            if (series != null) block.fields.Add(Text(BuiltInBlocks.CollectSeriesField, series));
            return block;
        }

        // One authored dialogue row: the speaker, the line, and up to three (choice, reply) pairs
        public readonly struct DialogueRow
        {
            public readonly string Speaker;
            public readonly string Line;
            public readonly (string Choice, string Reply)[] Replies;

            public DialogueRow(string speaker, string line, params (string Choice, string Reply)[] replies)
            {
                Speaker = speaker;
                Line = line;
                Replies = replies;
            }
        }

        public static DialogueRow[] DialoguePlain => new DialogueRow[]
        {
            new("The mason", "Welcome. Mind the dust: we are mending the arch."),
            new("The mason", "The stone comes from the quarry across the river."),
            new("The mason", "Come back next spring and it will look as it did in 1640."),
        };

        public static DialogueRow[] DialogueWithChoices => new DialogueRow[]
        {
            new("The mason", "Welcome. Do you know why this arch was rebuilt?", ("No, tell me", "The earthquake brought it down in 1755."), ("Yes, the earthquake", "Then you know more than most visitors.")),
            new("The mason", "It took eleven winters to finish."),
            new("The mason", "Would you like to hold a chisel?", ("Yes please", "Careful: it is sharper than it looks."), ("Not today", ""), ("Maybe later", "I am here until dusk.")),
            new("The mason", "Thank you for listening."),
        };

        public static DialogueRow[] DialogueLong => new DialogueRow[]
        {
            new("The royal chronicler of the household of the Kings of Portugal and of the Algarves",
                "The palace stood by the river for more than two hundred years and its halls, its gardens and its long arcades were known across the whole of Europe for their tiles and for their paintings, and every visitor to the court was taken through them in the same order."),
            new("The royal chronicler of the household of the Kings of Portugal and of the Algarves",
                "Then, on the morning of the first of November, everything moved.", ("What happened next?", "The river rose, the fires started, and by evening the palace was gone; only the foundations and the memory were left."), ("How do you know?", "It was written down by people who were there, and I have read every page of it.")),
        };

        private static void AddDialogues(List<Entry> list)
        {
            foreach (var variant in BuiltInBlocks.Dialogue.Variants)
            {
                list.Add(new Entry(BuiltInBlocks.DialogueKind, variant, "short", DialogueBlock(variant, DialoguePlain)));
                list.Add(new Entry(BuiltInBlocks.DialogueKind, variant, "choices", DialogueBlock(variant, DialogueWithChoices)));
                list.Add(new Entry(BuiltInBlocks.DialogueKind, variant, "long", DialogueBlock(variant, DialogueLong)));
                // - "partial": a row with no words is left out, a reply with no choice label is never offered, a line may have no speaker
                list.Add(new Entry(BuiltInBlocks.DialogueKind, variant, "partial", DialogueBlock(variant,
                    new DialogueRow("The mason", "The first line."),
                    new DialogueRow("The mason", ""),
                    new DialogueRow("", "A line with no speaker.", ("", "A reply nobody can pick."), ("Go on", "Very well.")))));
                // - one line only: nothing to continue, nothing to start again
                list.Add(new Entry(BuiltInBlocks.DialogueKind, variant, "oneline", DialogueBlock(variant, new DialogueRow("The mason", "Just one thing to say."))));
            }
        }

        private static BlockInstanceData DialogueBlock(string variant, params DialogueRow[] rows)
        {
            var block = new BlockInstanceData { key = QuizBlockKey, kind = BuiltInBlocks.DialogueKind, variant = variant };
            var items = new List<BlockItemData>();
            foreach (var row in rows)
            {
                var fields = new List<BlockItemFieldValue>();
                if (row.Speaker.Length > 0) fields.Add(ItemText(BuiltInBlocks.DialogueSpeakerField, row.Speaker));
                fields.Add(ItemText(BuiltInBlocks.DialogueTextField, row.Line));
                for (int i = 0; row.Replies != null && i < row.Replies.Length; i++)
                {
                    fields.Add(ItemText(DialogueRule.ChoiceField(i + 1), row.Replies[i].Choice));
                    fields.Add(ItemText(DialogueRule.ReplyField(i + 1), row.Replies[i].Reply));
                }
                items.Add(Item(fields.ToArray()));
            }
            block.fields.Add(new BlockFieldValue { key = BuiltInBlocks.DialogueLinesField, items = items });
            return block;
        }

        private static void AddShowOnWall(List<Entry> list)
        {
            System.Action<WallConfigData> neighbours = wall =>
            {
                wall.pois.Add(WallPoi("near_a", "North Tower", new Vector3(-1f, 0f, 0f), null));
                wall.pois.Add(WallPoi("near_b", "South Tower", new Vector3(2f, 0f, 0f), null));
                wall.pois.Add(WallPoi("near_c", "The chapel of Saint George with its bell tower and the old cemetery", new Vector3(3f, 0f, 0f), null));
                wall.pois.Add(WallPoi("near_d", "Too Far To Be Named", new Vector3(30f, 0f, 0f), null));
            };
            // - three neighbours whose card titles are all long: the chips wrap
            System.Action<WallConfigData> longNeighbours = wall =>
            {
                wall.pois.Add(WallPoi("near_l1", "The Royal Palace of the Kings of Portugal and of the Algarves, before the earthquake", new Vector3(-1f, 0f, 0f), null));
                wall.pois.Add(WallPoi("near_l2", "The chapel of Saint George with its bell tower and the old cemetery", new Vector3(2f, 0f, 0f), null));
                wall.pois.Add(WallPoi("near_l3", "The long arcade with its many arches and the market that once stood under it", new Vector3(3f, 0f, 0f), null));
            };
            foreach (var variant in BuiltInBlocks.ShowOnWall.Variants)
            {
                // - with_neighbours: only the nearest few are named (the fourth point is far away and is not); button: no neighbours drawn
                list.Add(new Entry(BuiltInBlocks.ShowOnWallKind, variant, "short", ShowOnWallBlock(variant), null, neighbours));
                if (variant == BuiltInBlocks.ShowOnWallWithNeighbours)
                    list.Add(new Entry(BuiltInBlocks.ShowOnWallKind, variant, "long", ShowOnWallBlock(variant), null, longNeighbours));
            }
        }

        private static BlockInstanceData ShowOnWallBlock(string variant) =>
            new() { key = QuizBlockKey, kind = BuiltInBlocks.ShowOnWallKind, variant = variant };


        // ---------------- status ----------------

        public const string OutlineIntact = "outline_1";
        public const string OutlinePartial = "outline_2";
        public const string OutlineHeavy = "outline_3";
        public const string OutlineDestroyed = "outline_4";
        public const string OutlineUnknown = "unknown";

        private static void AddStatus(List<Entry> list)
        {
            foreach (var variant in BuiltInBlocks.Status.Variants)
            {
                list.Add(new Entry(BuiltInBlocks.StatusKind, variant, "intact", StatusBlock(variant, null), Condition(OutlineIntact, 0f, false)));
                list.Add(new Entry(BuiltInBlocks.StatusKind, variant, "partial", StatusBlock(variant, null), Condition(OutlinePartial, 20f, false)));
                list.Add(new Entry(BuiltInBlocks.StatusKind, variant, "unknown", StatusBlock(variant, null), Condition(OutlineUnknown, 100f, true)));
                list.Add(new Entry(BuiltInBlocks.StatusKind, variant, "label", StatusBlock(variant, "Kept as the painter saw it, with every merlon in place"),
                    Condition(OutlineIntact, 0f, false)));
                // - a wall with no Outline Types rows: no marker ring to borrow, the card's own status tokens
                list.Add(new Entry(BuiltInBlocks.StatusKind, variant, "tokens", StatusBlock(variant, null), Condition("", 60f, false),
                    wall => wall.outline_levels.Clear()));
            }
        }

        private static BlockInstanceData StatusBlock(string variant, string label)
        {
            var block = new BlockInstanceData { key = "block_2", kind = BuiltInBlocks.StatusKind, variant = variant };
            if (label != null) block.fields.Add(Text(BuiltInBlocks.StatusLabelField, label));
            return block;
        }

        private static System.Action<POIData> Condition(string levelKey, float pct, bool unknown) => poi =>
        {
            poi.has_status = true;
            poi.status_level_key = levelKey;
            poi.status_pct = pct;
            poi.status_unknown = unknown;
        };

        // ---------------- rich_text ----------------

        public const string ShortText = "Built on the hill above the river, the gate let carts into the upper town.";
        public const string LongText =
            "[[Keep|keep]] and walls were rebuilt after 1147, when the town changed hands, and again after every siege the " +
            "chroniclers wrote down. The painter shows them whole, with every merlon in place.\n\n" +
            "The [[curtain wall]] runs from the gate to the river and closes the lower town. Its towers are square on the " +
            "hill side and round where the ground falls away.\n\n" +
            "By 1700 the castle was a barracks and a prison; the panel is older than that change and keeps the royal colours " +
            "on the flags.\n\n" +
            "Most of what the painter saw is still standing, but the long arcade by the water was lost in the earthquake.";

        private static void AddRichText(List<Entry> list)
        {
            foreach (var variant in BuiltInBlocks.RichText.Variants)
            {
                list.Add(new Entry(BuiltInBlocks.RichTextKind, variant, "short", RichText(variant, ShortText, withSections: true)));
                list.Add(new Entry(BuiltInBlocks.RichTextKind, variant, "long", RichText(variant, LongText, withSections: true)));
            }
            // - the optional Sections left empty: the Sections look shows the text alone
            list.Add(new Entry(BuiltInBlocks.RichTextKind, BuiltInBlocks.RichTextSections, "nosections",
                RichText(BuiltInBlocks.RichTextSections, ShortText, withSections: false)));
        }

        private static BlockInstanceData RichText(string variant, string body, bool withSections)
        {
            var block = new BlockInstanceData { key = "block_2", kind = BuiltInBlocks.RichTextKind, variant = variant };
            block.fields.Add(Text(BuiltInBlocks.RichTextBodyField, body));
            if (withSections)
            {
                var sections = new BlockFieldValue { key = BuiltInBlocks.RichTextSectionsField };
                sections.items.Add(Item(ItemText(BuiltInBlocks.RichTextSectionTitleField, "Before the earthquake"),
                    ItemText(BuiltInBlocks.RichTextSectionBodyField, "The arcade ran along the water for two hundred metres.")));
                sections.items.Add(Item(ItemText(BuiltInBlocks.RichTextSectionTitleField, "After 1755"),
                    ItemText(BuiltInBlocks.RichTextSectionBodyField, "Only the [[keep]] and the gate stood; the rest was rebuilt in stone.")));
                block.fields.Add(sections);
            }
            return block;
        }

        // ---------------- quick_facts ----------------

        private static void AddQuickFacts(List<Entry> list)
        {
            var shortFacts = new[] { ("Built", "1147"), ("Height", "32 m") };
            var longFacts = new[]
            {
                ("Built", "1147"), ("Rebuilt", "After the earthquake of 1755"), ("Architect", "Unknown master masons of the royal works"),
                ("Towers", "11"), ("Walls", "Rammed earth and limestone"), ("Open", "Every day"),
            };
            // - one row with only a value, one with only a label: each shows the text it has
            var partialFacts = new[] { ("", "1147"), ("Architect", "") };
            foreach (var variant in BuiltInBlocks.QuickFacts.Variants)
            {
                list.Add(new Entry(BuiltInBlocks.QuickFactsKind, variant, "short", Facts(variant, shortFacts)));
                list.Add(new Entry(BuiltInBlocks.QuickFactsKind, variant, "long", Facts(variant, longFacts)));
                list.Add(new Entry(BuiltInBlocks.QuickFactsKind, variant, "partial", Facts(variant, partialFacts)));
            }
        }

        private static BlockInstanceData Facts(string variant, (string Label, string Value)[] facts)
        {
            var block = new BlockInstanceData { key = "block_2", kind = BuiltInBlocks.QuickFactsKind, variant = variant };
            var rows = new BlockFieldValue { key = BuiltInBlocks.QuickFactsItemsField };
            foreach (var (label, value) in facts)
                rows.items.Add(Item(ItemText(BuiltInBlocks.QuickFactsLabelField, label), ItemText(BuiltInBlocks.QuickFactsValueField, value)));
            block.fields.Add(rows);
            return block;
        }

        // ---------------- fun_fact ----------------

        private static void AddFunFacts(List<Entry> list)
        {
            var shortFacts = new[] { "The gate was locked every night at nine." };
            var longFacts = new[]
            {
                "The [[keep]] has no door at ground level: its only entrance is a wooden stair that the garrison could pull up.",
                "Peacocks have lived in the castle gardens for longer than anyone remembers.\n\nThey are not wild: the city feeds them.",
                "The painter left out a whole tower, probably to make room for the ships.",
            };
            foreach (var variant in BuiltInBlocks.FunFact.Variants)
            {
                list.Add(new Entry(BuiltInBlocks.FunFactKind, variant, "short", FunFacts(variant, shortFacts)));
                list.Add(new Entry(BuiltInBlocks.FunFactKind, variant, "long", FunFacts(variant, longFacts)));
            }
        }

        private static BlockInstanceData FunFacts(string variant, string[] facts)
        {
            var block = new BlockInstanceData { key = "block_2", kind = BuiltInBlocks.FunFactKind, variant = variant };
            var rows = new BlockFieldValue { key = BuiltInBlocks.FunFactItemsField };
            foreach (string fact in facts) rows.items.Add(Item(ItemText(BuiltInBlocks.FunFactTextField, fact)));
            block.fields.Add(rows);
            return block;
        }

        // ---------------- pull_quote ----------------

        private static void AddPullQuotes(List<Entry> list)
        {
            foreach (var variant in BuiltInBlocks.PullQuote.Variants)
            {
                list.Add(new Entry(BuiltInBlocks.PullQuoteKind, variant, "short", Quote(variant, "A city on a hill cannot be hidden.", "A chronicler", "1147")));
                list.Add(new Entry(BuiltInBlocks.PullQuoteKind, variant, "long", Quote(variant,
                    "We saw the [[keep]] from the river long before the town, white in the morning, and every man on the ship fell " +
                    "silent, for it looked as if it had been there before the hill itself.",
                    "Osbern, a crusader priest from the English fleet", "Letter on the conquest of the city, written the following winter")));
                // - the optional author and source left empty: no attribution line at all
                list.Add(new Entry(BuiltInBlocks.PullQuoteKind, variant, "noauthor", Quote(variant, "A city on a hill cannot be hidden.", "", "")));
            }
        }

        private static BlockInstanceData Quote(string variant, string quote, string author, string source)
        {
            var block = new BlockInstanceData { key = "block_2", kind = BuiltInBlocks.PullQuoteKind, variant = variant };
            block.fields.Add(Text(BuiltInBlocks.PullQuoteTextField, quote));
            if (author.Length > 0) block.fields.Add(Text(BuiltInBlocks.PullQuoteAuthorField, author));
            if (source.Length > 0) block.fields.Add(Text(BuiltInBlocks.PullQuoteSourceField, source));
            return block;
        }

        // ---------------- process_steps ----------------

        private static void AddProcessSteps(List<Entry> list)
        {
            var shortSteps = new[] { ("Shape the clay", "Pressed into a square wooden frame."), ("Fire it", "") };
            var longSteps = new[]
            {
                ("Dig and wash the clay from the river banks outside the city walls",
                    "The clay was left to rest in water for weeks, so that stones and roots sank to the bottom.\n\nOnly the fine top layer was kept."),
                ("Shape and dry the tiles", "Pressed into square frames and dried in the shade for a month."),
                ("First firing", "A day and a night in the kiln turned the clay into hard [[biscuit]]."),
                ("Glaze and paint", "A coat of white tin glaze, then the drawing in cobalt blue with a fine brush."),
                ("Second firing", "The glaze melted into glass and the blue sank into it for good."),
            };
            // - the second row has no title (not shown, and the count does not skip a number); the last has no text
            var partialSteps = new[] { ("Shape the clay", "Pressed into a frame."), ("", "A row without a title."), ("Fire it", "") };
            foreach (var variant in BuiltInBlocks.ProcessSteps.Variants)
            {
                list.Add(new Entry(BuiltInBlocks.ProcessStepsKind, variant, "short", StepsBlock(variant, shortSteps)));
                list.Add(new Entry(BuiltInBlocks.ProcessStepsKind, variant, "long", StepsBlock(variant, longSteps)));
                list.Add(new Entry(BuiltInBlocks.ProcessStepsKind, variant, "partial", StepsBlock(variant, partialSteps)));
            }
        }

        private static BlockInstanceData StepsBlock(string variant, (string Title, string Text)[] steps)
        {
            var block = new BlockInstanceData { key = "block_2", kind = BuiltInBlocks.ProcessStepsKind, variant = variant };
            var field = new BlockFieldValue { key = BuiltInBlocks.ProcessStepsItemsField };
            foreach (var (title, text) in steps)
                field.items.Add(Item(ItemText(BuiltInBlocks.ProcessStepsTitleField, title), ItemText(BuiltInBlocks.ProcessStepsTextField, text)));
            block.fields.Add(field);
            return block;
        }

        // ---------------- swatches ----------------

        private static void AddSwatches(List<Entry> list)
        {
            var shortSwatches = new[] { ("Cobalt blue", "#1F3F8F", "The drawing"), ("Tin white", "#F2EEE3", "The glaze") };
            var longSwatches = new[]
            {
                ("Cobalt blue", "#1F3F8F", "Every outline and shadow of the panel"),
                ("Tin white", "#F2EEE3", "The glaze under the drawing"),
                ("Antimony yellow", "#D9A93A", "The borders and the royal flags"),
                ("Copper green", "#3C7A4E", "Trees and the river bank, only on the oldest panels"),
                ("Manganese purple, almost black where the brush stopped", "#4A2F45", "Outlines before cobalt became common"),
                ("Iron red", "#9C3B24", ""),
            };
            // - no name, a colour that is not one ("#12", "blue"): three rows left out; "#abc" is the short form of a real colour
            var partialSwatches = new[] { ("", "#1F3F8F", "No name"), ("Bad colour", "#12", ""), ("A word", "blue", ""), ("Grey", "#abc", ""), ("Umber", "#2B2118", "Close to the card's own colour") };
            foreach (var variant in BuiltInBlocks.Swatches.Variants)
            {
                list.Add(new Entry(BuiltInBlocks.SwatchesKind, variant, "short", SwatchesBlock(variant, shortSwatches)));
                list.Add(new Entry(BuiltInBlocks.SwatchesKind, variant, "long", SwatchesBlock(variant, longSwatches)));
                list.Add(new Entry(BuiltInBlocks.SwatchesKind, variant, "partial", SwatchesBlock(variant, partialSwatches)));
                // - Show Code on: the colour's hex under its name (off by default: a visitor rarely wants "#1F3F8F")
                var codes = SwatchesBlock(variant, shortSwatches);
                codes.fields.Add(new BlockFieldValue { key = BuiltInBlocks.SwatchesShowCodeField, flag = true });
                list.Add(new Entry(BuiltInBlocks.SwatchesKind, variant, "codes", codes));
            }
        }

        private static BlockInstanceData SwatchesBlock(string variant, (string Name, string Colour, string Note)[] swatches)
        {
            var block = new BlockInstanceData { key = "block_2", kind = BuiltInBlocks.SwatchesKind, variant = variant };
            var field = new BlockFieldValue { key = BuiltInBlocks.SwatchesItemsField };
            foreach (var (name, colour, note) in swatches)
                field.items.Add(Item(ItemText(BuiltInBlocks.SwatchesNameField, name), new BlockItemFieldValue { key = BuiltInBlocks.SwatchesColourField, value = colour },
                    ItemText(BuiltInBlocks.SwatchesNoteField, note)));
            block.fields.Add(field);
            return block;
        }

        // ---------------- timeline ----------------

        private static void AddTimelines(List<Entry> list)
        {
            var shortEvents = new[] { ("1147", "The siege", ""), ("1755", "The earthquake", "Most of the palace fell.") };
            var longEvents = new[]
            {
                ("1147", "The siege", "Crusaders on their way to the Holy Land helped take the town after four months."),
                ("c. 1300", "A royal palace", "The kings made the castle their home and rebuilt the [[keep]]."),
                ("1511", "The court moves to the river", ""),
                ("1 Nov 1755", "The great earthquake and the fire that followed it for six days", "Most of the palace fell; the walls stood."),
                ("1910", "A national monument", ""),
                ("1940", "Rebuilt as the painter saw it", "The restorers used old panels like this one as their plan."),
            };
            // - a row with no date and one with no title are left out
            var partialEvents = new[] { ("1147", "The siege", ""), ("", "No date", ""), ("1755", "", "No title"), ("1940", "Rebuilt", "") };
            foreach (var variant in BuiltInBlocks.Timeline.Variants)
            {
                list.Add(new Entry(BuiltInBlocks.TimelineKind, variant, "short", TimelineBlock(variant, shortEvents, now: false)));
                list.Add(new Entry(BuiltInBlocks.TimelineKind, variant, "long", TimelineBlock(variant, longEvents, now: true)));
                list.Add(new Entry(BuiltInBlocks.TimelineKind, variant, "partial", TimelineBlock(variant, partialEvents, now: true)));
            }
        }

        private static BlockInstanceData TimelineBlock(string variant, (string Date, string Title, string Text)[] events, bool now)
        {
            var block = new BlockInstanceData { key = "block_2", kind = BuiltInBlocks.TimelineKind, variant = variant };
            var field = new BlockFieldValue { key = BuiltInBlocks.TimelineItemsField };
            foreach (var (date, title, text) in events)
                field.items.Add(Item(ItemText(BuiltInBlocks.TimelineDateField, date), ItemText(BuiltInBlocks.TimelineTitleField, title),
                    ItemText(BuiltInBlocks.TimelineTextField, text)));
            block.fields.Add(field);
            if (now) block.fields.Add(new BlockFieldValue { key = BuiltInBlocks.TimelineHighlightNowField, flag = true });
            return block;
        }

        // ---------------- person ----------------

        private static void AddPeople(List<Entry> list)
        {
            foreach (var variant in BuiltInBlocks.Person.Variants)
            {
                list.Add(new Entry(BuiltInBlocks.PersonKind, variant, "short", PersonBlock(variant, "Afonso Henriques", "First king", "Took the town in 1147.")));
                list.Add(new Entry(BuiltInBlocks.PersonKind, variant, "long", PersonBlock(variant,
                    "Dom Manuel I of Portugal, called the Fortunate by the chroniclers of his reign",
                    "King from 1495 to 1521, who moved the royal court from the castle to the new palace by the river",
                    "He left the castle's rooms to the garrison and the prison and built his palace where the ships came in.\n\n" +
                    "The panel shows the castle still with the royal flags, so it was probably painted from older drawings.")));
                // - the optional role and text left empty: the name alone beside its initial
                list.Add(new Entry(BuiltInBlocks.PersonKind, variant, "nameonly", PersonBlock(variant, "Osbern", "", "")));
            }
        }

        private static BlockInstanceData PersonBlock(string variant, string name, string role, string text)
        {
            var block = new BlockInstanceData { key = "block_2", kind = BuiltInBlocks.PersonKind, variant = variant };
            block.fields.Add(Text(BuiltInBlocks.PersonNameField, name));
            if (role.Length > 0) block.fields.Add(Text(BuiltInBlocks.PersonRoleField, role));
            if (text.Length > 0) block.fields.Add(Text(BuiltInBlocks.PersonTextField, text));
            return block;
        }

        // ---------------- story_chapters ----------------

        private static void AddStoryChapters(List<Entry> list)
        {
            var shortChapters = new[] { ("The siege", "Four months outside the walls."), ("The gate", "It opened in October.") };
            var longChapters = new[]
            {
                ("The siege of the town on the hill, in the summer and autumn of 1147",
                    "Ships of crusaders on their way to the Holy Land stopped in the river and joined the king's army.\n\n" +
                    "For four months they camped outside the walls, and the [[keep]] never fell to an attack."),
                ("The palace", "The kings rebuilt the castle as their home and filled it with painted tiles."),
                ("The earthquake", "In 1755 the ground shook for six minutes; the palace fell and a fire burned for six days."),
                ("Today", "The walls were rebuilt in 1940 from old panels and drawings like this one."),
            };
            // - a row with no text and one with no title are left out: one chapter is left, and needs no buttons
            var partialChapters = new[] { ("The siege", "Four months outside the walls."), ("No text", ""), ("", "No title.") };
            foreach (var variant in BuiltInBlocks.StoryChapters.Variants)
            {
                list.Add(new Entry(BuiltInBlocks.StoryChaptersKind, variant, "short", ChaptersBlock(variant, shortChapters)));
                list.Add(new Entry(BuiltInBlocks.StoryChaptersKind, variant, "long", ChaptersBlock(variant, longChapters)));
                list.Add(new Entry(BuiltInBlocks.StoryChaptersKind, variant, "single", ChaptersBlock(variant, partialChapters)));
            }
        }

        private static BlockInstanceData ChaptersBlock(string variant, (string Title, string Body)[] chapters)
        {
            var block = new BlockInstanceData { key = "block_2", kind = BuiltInBlocks.StoryChaptersKind, variant = variant };
            var field = new BlockFieldValue { key = BuiltInBlocks.StoryChaptersItemsField };
            foreach (var (title, body) in chapters)
                field.items.Add(Item(ItemText(BuiltInBlocks.StoryChaptersTitleField, title), ItemText(BuiltInBlocks.StoryChaptersBodyField, body)));
            block.fields.Add(field);
            return block;
        }

        // ---------------- compare_points ----------------

        public const string CompareOtherId = "compare_other";

        private static void AddComparePoints(List<Entry> list)
        {
            foreach (var variant in BuiltInBlocks.ComparePoints.Variants)
            {
                // - this point intact, the other partly damaged (named by its name: it has no header)
                list.Add(new Entry(BuiltInBlocks.ComparePointsKind, variant, "short", CompareBlock(variant), Condition(OutlineIntact, 0f, false),
                    wall => wall.pois.Add(OtherPoi("Old Cathedral", OutlinePartial, 20f, false, null))));
                // - long titles on both sides; the other named by its card's own header title
                list.Add(new Entry(BuiltInBlocks.ComparePointsKind, variant, "long", CompareBlock(variant), Condition(OutlineHeavy, 60f, false),
                    wall => wall.pois.Add(OtherPoi("Other", OutlineDestroyed, 100f, false,
                        "The Royal Palace of the Kings by the river, before the earthquake"))));
                // - the other nobody assessed: its "?" ring
                list.Add(new Entry(BuiltInBlocks.ComparePointsKind, variant, "unknown", CompareBlock(variant), Condition(OutlinePartial, 20f, false),
                    wall => wall.pois.Add(OtherPoi("Customs House", OutlineUnknown, 100f, true, null))));
            }
        }

        private static BlockInstanceData CompareBlock(string variant)
        {
            var block = new BlockInstanceData { key = "block_2", kind = BuiltInBlocks.ComparePointsKind, variant = variant };
            block.fields.Add(new BlockFieldValue { key = BuiltInBlocks.ComparePointsOtherField, value = CompareOtherId });
            return block;
        }

        private static POIData OtherPoi(string name, string levelKey, float pct, bool unknown, string headerTitle)
        {
            var poi = new POIData { id = CompareOtherId, name = name, category = CategoryKey, hierarchy_level_key = LevelKey };
            Condition(levelKey, pct, unknown)(poi);
            if (headerTitle != null)
            {
                var header = new BlockInstanceData { key = "block_1", kind = BuiltInBlocks.HeaderKind };
                header.fields.Add(Text(BlockStackBuilder.HeaderTitleField, headerTitle));
                poi.card.blocks.Add(header);
            }
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

        // ---------------- sources ----------------

        private static void AddSources(List<Entry> list)
        {
            var shortRows = new[] { ("Chronicle of the conquest", "Anonymous", "Public domain") };
            var longRows = new[]
            {
                ("Letter on the conquest of the city, written by a priest of the English fleet in the following winter", "Osbern", "Public domain"),
                ("Panel of the city before the earthquake, tile inventory sheet 14", "National Tile Museum", "With permission"),
                ("Photograph of the keep from the river", "Municipal photographic archive", "CC BY 4.0"),
            };
            // - a row with no author and no licence: the title alone
            var partialRows = new[] { ("Oral account of the gardeners", "", "") };
            foreach (var variant in BuiltInBlocks.Sources.Variants)
            {
                list.Add(new Entry(BuiltInBlocks.SourcesKind, variant, "short", SourcesBlock(variant, shortRows, BuiltInBlocks.ContentVerified)));
                list.Add(new Entry(BuiltInBlocks.SourcesKind, variant, "long", SourcesBlock(variant, longRows, BuiltInBlocks.ContentDraft)));
                list.Add(new Entry(BuiltInBlocks.SourcesKind, variant, "partial", SourcesBlock(variant, partialRows, null)));
            }
        }

        private static BlockInstanceData SourcesBlock(string variant, (string Title, string Author, string Licence)[] rows, string status)
        {
            var block = new BlockInstanceData { key = "block_2", kind = BuiltInBlocks.SourcesKind, variant = variant };
            var field = new BlockFieldValue { key = BuiltInBlocks.SourcesItemsField };
            foreach (var (title, author, licence) in rows)
                field.items.Add(Item(ItemText(BuiltInBlocks.SourcesTitleField, title), ItemText(BuiltInBlocks.SourcesAuthorField, author),
                    ItemText(BuiltInBlocks.SourcesLicenceField, licence)));
            block.fields.Add(field);
            if (status != null) block.fields.Add(new BlockFieldValue { key = BuiltInBlocks.SourcesStatusField, value = status });
            return block;
        }

        // ---------------- actions ----------------

        private static void AddActions(List<Entry> list)
        {
            var one = new[] { ("See it on the wall", BuiltInBlocks.ActionShowOnWall) };
            var many = new[]
            {
                ("See it on the wall", BuiltInBlocks.ActionShowOnWall),
                ("Find the keep on the panel from the river side", BuiltInBlocks.ActionShowOnWall),
                ("Where is it?", BuiltInBlocks.ActionShowOnWall),
            };
            // - a button with no words and one with an action this framework does not know: both left out
            var partial = new[] { ("", BuiltInBlocks.ActionShowOnWall), ("Listen", "listen"), ("See it on the wall", BuiltInBlocks.ActionShowOnWall) };
            foreach (var variant in BuiltInBlocks.Actions.Variants)
            {
                list.Add(new Entry(BuiltInBlocks.ActionsKind, variant, "short", ActionsBlock(variant, one)));
                list.Add(new Entry(BuiltInBlocks.ActionsKind, variant, "long", ActionsBlock(variant, many)));
                list.Add(new Entry(BuiltInBlocks.ActionsKind, variant, "partial", ActionsBlock(variant, partial)));
            }
        }

        private static BlockInstanceData ActionsBlock(string variant, (string Label, string Action)[] buttons)
        {
            var block = new BlockInstanceData { key = "block_2", kind = BuiltInBlocks.ActionsKind, variant = variant };
            var field = new BlockFieldValue { key = BuiltInBlocks.ActionsItemsField };
            foreach (var (label, action) in buttons)
                field.items.Add(Item(ItemText(BuiltInBlocks.ActionsLabelField, label), new BlockItemFieldValue { key = BuiltInBlocks.ActionsActionField, value = action }));
            block.fields.Add(field);
            return block;
        }

        // ---------------- the fabricated wall ----------------

        // One category and one level (the header chip), a glossary with the terms the texts use
        public static WallConfigData Taxonomy()
        {
            var config = new WallConfigData
            {
                category_styles = new List<CategoryStyleEntry> { new() { key = CategoryKey, label = "Civic Buildings" } },
                hierarchy_levels = new List<HierarchyLevelEntry> { new() { key = LevelKey, level_name = "Landmark" } },
            };
            config.card_settings.languages = new List<string> { "en" };
            // - the markers' Outline Types, per type with the stock colours (an empty colour = the framework ramp)
            config.marker_outline_mode = "per_type";
            config.outline_levels = new List<OutlineLevelEntry>
            {
                new() { key = OutlineIntact, label = "Intact", pct = 0f, line_style = "solid" },
                new() { key = OutlinePartial, label = "Partial Damage", pct = 20f, line_style = "dash_long" },
                new() { key = OutlineHeavy, label = "Heavy Damage", pct = 60f, line_style = "dash_short" },
                new() { key = OutlineDestroyed, label = "Destroyed", pct = 100f, line_style = "dotted" },
                new() { key = OutlineUnknown, label = "Unknown", pct = 100f, line_style = "dotted", color_hex = "#71717A" },
            };
            config.card_settings.glossary = new List<GlossaryEntry>
            {
                Glossary("keep", "The strongest tower of a castle, its last refuge."),
                Glossary("curtain wall", "The wall that joins the towers of a castle."),
                Glossary("biscuit", "Clay fired once, hard but not yet glazed."),
            };
            return config;
        }

        // The POI of one entry: a header entry's own header, or a short header + the entry's block
        public static POIData Poi(Entry e)
        {
            var poi = new POIData { id = e.Name, name = e.Title, category = CategoryKey, hierarchy_level_key = LevelKey };
            var header = new BlockInstanceData { key = "block_1", kind = BuiltInBlocks.HeaderKind, variant = e.IsHeader ? e.Variant : BuiltInBlocks.HeaderTextOnly };
            header.fields.Add(Text(BlockStackBuilder.HeaderTitleField, e.Title));
            if (e.Subtitle.Length > 0) header.fields.Add(Text(BlockStackBuilder.HeaderSubtitleField, e.Subtitle));
            if (e.IsHeader && System.Array.IndexOf(BuiltInBlocks.HeaderImageVariants, e.Variant) >= 0 && e.Content != NoPicture)
                AddPictureFields(header, e.Variant);
            poi.card.blocks.Add(header);
            if (e.Block != null) poi.card.blocks.Add(e.Block);
            e.Setup?.Invoke(poi);
            return poi;
        }

        private static BlockFieldValue Text(string key, string value) =>
            new() { key = key, text = new List<LocalizedEntry> { new() { lang = "en", value = value } } };

        private static BlockItemFieldValue ItemText(string key, string value) =>
            new() { key = key, text = new List<LocalizedEntry> { new() { lang = "en", value = value } } };

        private static BlockItemData Item(params BlockItemFieldValue[] fields) => new() { fields = new List<BlockItemFieldValue>(fields) };

        private static GlossaryEntry Glossary(string term, string definition) =>
            new() { term = term, definition = new List<LocalizedEntry> { new() { lang = "en", value = definition } } };
    }
}
