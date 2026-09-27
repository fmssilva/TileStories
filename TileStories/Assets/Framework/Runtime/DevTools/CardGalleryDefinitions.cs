using System.Collections.Generic;

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
            }

            public Entry(string kind, string variant, string content, BlockInstanceData block, System.Action<POIData> setup = null,
                System.Action<WallConfigData> wallSetup = null)
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
            AddSources(list);
            AddActions(list);
            return list;
        }

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
        }

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
