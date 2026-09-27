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
