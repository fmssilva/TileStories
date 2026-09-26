using System.Collections.Generic;

namespace TileStories
{
    // Phase A data list of the POI Detail Card gallery (_3.1 section 7, 40-testing.md 4.4): every header variant x
    // content (short, long, no subtitle) x stop, with fabricated content -- no AR, no config.json. The ONE list
    // both CardGalleryHarness (to look at) and CardGalleryTests (to assert) read, so the two cannot drift apart.
    public static class CardGalleryDefinitions
    {
        public readonly struct Entry
        {
            public readonly string Name;
            public readonly string Variant;
            public readonly string Title;
            public readonly string Subtitle;
            public readonly SheetStopRule.Stop Stop;

            public Entry(string variant, string content, string title, string subtitle, SheetStopRule.Stop stop)
            {
                Name = "header_" + variant + "_" + content + "_" + stop.ToString().ToLowerInvariant();
                Variant = variant;
                Title = title;
                Subtitle = subtitle;
                Stop = stop;
            }
        }

        public const string CategoryKey = "category_1";
        public const string LevelKey = "level_1";

        public static readonly IReadOnlyList<Entry> All = Build();

        private static List<Entry> Build()
        {
            var contents = new[]
            {
                ("short", "Gate", "1640"),
                ("long", "The Royal Palace of the Kings of Portugal and of the Algarves, before the earthquake",
                    "Seen from the river on the panel, with the Customs House, the chapel and the long arcade that the fire destroyed"),
                ("nosubtitle", "Old Cathedral", ""),
            };
            var list = new List<Entry>();
            foreach (var variant in BuiltInBlocks.Header.Variants)
                foreach (var (content, title, subtitle) in contents)
                    foreach (var stop in new[] { SheetStopRule.Stop.Peek, SheetStopRule.Stop.Half, SheetStopRule.Stop.Full })
                        list.Add(new Entry(variant, content, title, subtitle, stop));
            return list;
        }

        // The fabricated wall: one category and one level, so the header chip has something to show
        public static WallConfigData Taxonomy() => new()
        {
            category_styles = new List<CategoryStyleEntry> { new() { key = CategoryKey, label = "Civic Buildings" } },
            hierarchy_levels = new List<HierarchyLevelEntry> { new() { key = LevelKey, level_name = "Landmark" } },
        };

        // The POI of one entry: its card holds exactly the entry's header
        public static POIData Poi(Entry e)
        {
            var poi = new POIData { id = e.Name, name = e.Title, category = CategoryKey, hierarchy_level_key = LevelKey };
            var header = new BlockInstanceData { key = "block_1", kind = BuiltInBlocks.HeaderKind, variant = e.Variant };
            header.fields.Add(new BlockFieldValue { key = BlockStackBuilder.HeaderTitleField, text = new List<LocalizedEntry> { new() { lang = "en", value = e.Title } } });
            if (e.Subtitle.Length > 0)
                header.fields.Add(new BlockFieldValue { key = BlockStackBuilder.HeaderSubtitleField, text = new List<LocalizedEntry> { new() { lang = "en", value = e.Subtitle } } });
            poi.card.blocks.Add(header);
            return poi;
        }
    }
}
