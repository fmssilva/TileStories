using System.Collections.Generic;

namespace TileStories
{
    // The story part of the gallery list (_3.3 S2a): the composed kind's PREVIEW on the card and its PAGE in the full view. A magazine (the
    // prototype's shape: eyebrow, title, lede, paragraphs, a drop cap, headings, a callout, a pull quote, a divider -- long enough to be cut),
    // a short one (shown whole), one SHEET per text and structure element whose Page holds a row per style option (every tone, size,
    // alignment, the drop cap, numbers, the ornament), the Open Row's three emphases, a Custom preview, and the media rows S2b builds (a
    // "not built yet" plate in this gallery only). Each entry is one language (English), like every gallery entry.
    public static partial class CardGalleryDefinitions
    {
        // The content names of the story entries (Entry.Content)
        public const string StoryMagazine = "magazine";
        public const string StoryShort = "short";
        // A preview taller than the cap whatever its picture: a long lede (the magazine's preview fits until its picture is drawn, S2b)
        public const string StoryTall = "tall";
        public const string StoryCustom = "custom";
        public const string StoryMedia = "media";
        public const string StorySheetPrefix = "sheet_";
        public const string StoryEmphasisPrefix = "emphasis_";

        // The text and structure elements, in the order their sheets are listed
        public static readonly string[] StorySheetElements =
        {
            BuiltInElements.Title, BuiltInElements.Eyebrow, BuiltInElements.Heading, BuiltInElements.Lede, BuiltInElements.Paragraph,
            BuiltInElements.Callout, BuiltInElements.PullQuote, BuiltInElements.List, BuiltInElements.Divider, BuiltInElements.Spacer,
        };

        // A row of a sheet: the element, the one style option it shows ("" = every option at its default) and that option's value
        public readonly struct SheetRow
        {
            public readonly string Option;
            public readonly string Value;

            public SheetRow(string option, string value)
            {
                Option = option;
                Value = value;
            }
        }

        // The rows of an element's sheet: the defaults first, then one row per value of each of its style options
        public static List<SheetRow> SheetRowsOf(ElementKindDefinition element)
        {
            var rows = new List<SheetRow> { new("", "") };
            if (element.StyleFields == null) return rows;
            foreach (var style in element.StyleFields)
                foreach (string value in style.Options)
                    if (value != style.ChoiceDefault) rows.Add(new SheetRow(style.Key, value));
            return rows;
        }

        private static void AddStories(List<Entry> list)
        {
            list.Add(new Entry(BuiltInBlocks.StoryKind, BuiltInBlocks.StoryMagazine, StoryMagazine, Magazine()));
            list.Add(new Entry(BuiltInBlocks.StoryKind, BuiltInBlocks.StoryMagazine, StoryShort, ShortStory(null)));
            list.Add(new Entry(BuiltInBlocks.StoryKind, BuiltInBlocks.StoryMagazine, StoryTall, TallStory()));
            foreach (string element in StorySheetElements)
                list.Add(new Entry(BuiltInBlocks.StoryKind, BuiltInBlocks.StoryMagazine, StorySheetPrefix + element, Sheet(element)));
            foreach (string emphasis in ElementOptions.OpenRowEmphases)
                list.Add(new Entry(BuiltInBlocks.StoryKind, BuiltInBlocks.StoryMagazine, StoryEmphasisPrefix + emphasis, ShortStory(emphasis)));
            list.Add(new Entry(BuiltInBlocks.StoryKind, BuiltInBlocks.StoryMagazine, StoryCustom, CustomPreview()));
            list.Add(new Entry(BuiltInBlocks.StoryKind, BuiltInBlocks.StoryMagazine, StoryMedia, MediaRows()));
        }

        // The magazine: the prototype's page in fabricated words, a picture first (a plate here until S2b)
        private static BlockInstanceData Magazine()
        {
            var block = StoryBlock();
            Picture(block, "wide.png");
            Words(block, BuiltInElements.Eyebrow, "The palace on the water");
            Words(block, BuiltInElements.Title, "Two hundred and fifty years by the river");
            Words(block, BuiltInElements.Lede, "For about 250 years the kings lived by the river, in a palace that opened straight onto the water. It is gone now, but the square that took its place still carries its name.");
            Style(Words(block, BuiltInElements.Paragraph, "When the king found the old castle on the hill too cramped for a court growing rich on the sea trade, he moved down to the water's edge. The new palace stood beside the docks where ships unloaded spices, sugar and gold, so the king could watch the fleet come in from his own windows."),
                BuiltInElements.DropCapStyle, BuiltInElements.On);
            Words(block, BuiltInElements.Heading, "Two centuries of additions");
            Words(block, BuiltInElements.Paragraph, "Later kings kept adding to it. The river front received a new facade and a domed corner tower that sailors saw long before they saw the city.");
            var callout = Words(block, BuiltInElements.Callout, "A library of some 70,000 books, paintings and maps, the royal archive, a chapel, offices and workshops.");
            Text(callout, BuiltInElements.CalloutTitleField, "What was inside");
            Style(callout, BuiltInElements.ToneStyle, BuiltInElements.ToneFamily);
            Words(block, BuiltInElements.PullQuote, "Where the palace had stood, an open square was laid out, facing the river like a grand front door.");
            Style(Row(block, BuiltInElements.Divider), BuiltInElements.OrnamentStyle, BuiltInElements.OrnamentAzulejo);
            Words(block, BuiltInElements.Heading, "One morning in November");
            Words(block, BuiltInElements.Paragraph, "On a morning in November an earthquake shook the palace, a wave came up the river and fire finished what was left. The library, the archive and almost everything inside were lost.");
            return block;
        }

        // A story whose Automatic preview (eyebrow, title, a long lede) is taller than half the card
        private static BlockInstanceData TallStory()
        {
            var block = StoryBlock();
            Words(block, BuiltInElements.Eyebrow, "The palace on the water");
            Words(block, BuiltInElements.Title, "Two hundred and fifty years by the river");
            Words(block, BuiltInElements.Lede, "For about 250 years the kings lived by the river, in a palace that opened straight onto the water. It is gone now, but " +
                "the square that took its place still carries its name. When the king found the old castle on the hill too cramped for a court " +
                "growing rich on the sea trade, he moved down to the water's edge. The new palace stood beside the docks where ships unloaded " +
                "spices, sugar and gold, so the king could watch the fleet come in from his own windows. Offices, a chapel, workshops and the " +
                "royal archive grew around it, and the palace became the true centre of the kingdom.");
            Words(block, BuiltInElements.Paragraph, "Later kings kept adding to it.");
            return block;
        }

        // A short story that fits under the cap; `emphasis` (null = the wall's) set through the block's own Open Row override
        private static BlockInstanceData ShortStory(string emphasis)
        {
            var block = StoryBlock();
            Words(block, BuiltInElements.Eyebrow, "Old town");
            Words(block, BuiltInElements.Title, "The gate");
            Words(block, BuiltInElements.Lede, "The town's last gate, rebuilt in 1640.");
            if (emphasis == null) return block;
            var overrides = Value(block, BlockSurfaces.ComponentOverridesField);
            overrides.items.Add(Item(
                new BlockItemFieldValue { key = BlockSurfaces.ComponentKey, value = ElementOptions.ComponentOpenRow },
                new BlockItemFieldValue { key = BlockSurfaces.OverrideKey, flag = true },
                new BlockItemFieldValue { key = BlockSurfaces.EmphasisKey, value = emphasis }));
            return block;
        }

        // An element's sheet: a short title row naming nothing (the sheet is the element), then one row per style option. Words long enough
        // to wrap on the phone (the size and alignment show)
        private static BlockInstanceData Sheet(string key)
        {
            var block = StoryBlock();
            ElementRegistry.Shared.TryGet(key, out var element);
            foreach (var sheetRow in SheetRowsOf(element))
            {
                var row = Row(block, key);
                string label = sheetRow.Option.Length == 0 ? "Default" : sheetRow.Option + " " + sheetRow.Value;
                if (key == BuiltInElements.Callout) Text(row, BuiltInElements.CalloutTitleField, label);
                if (key == BuiltInElements.PullQuote) Text(row, BuiltInElements.AttributionField, label);
                if (element.Field(BuiltInElements.TextField) != null) Text(row, BuiltInElements.TextField, SheetWords(key, label));
                if (sheetRow.Option.Length > 0) Style(row, sheetRow.Option, sheetRow.Value);
            }
            return block;
        }

        private static string SheetWords(string key, string label) => key switch
        {
            BuiltInElements.List => label + "\nA library of some 70,000 books\nPaintings, maps and the royal archive",
            BuiltInElements.Title or BuiltInElements.Heading or BuiltInElements.Eyebrow => label + ": the palace on the water",
            _ => label + ". When the king found the old castle on the hill too cramped for a court growing rich on the sea trade, he moved down to the water's edge.",
        };

        // A Custom preview: its own teaser line, a link to the Page's title drawn large, and a link whose Page row is gone (nothing shows)
        private static BlockInstanceData CustomPreview()
        {
            var block = StoryBlock();
            var title = Words(block, BuiltInElements.Title, "The gate of the old town");
            Words(block, BuiltInElements.Paragraph, "The town's last gate was rebuilt in 1640 after a fire, and kept its old name.");
            Value(block, BlockSurfaces.PreviewModeField).value = ElementOptions.PreviewCustom;
            var preview = Value(block, BlockSurfaces.PreviewRowsField).items;
            ElementRegistry.Shared.TryGet(BuiltInElements.Eyebrow, out var eyebrow);
            var teaser = ElementRow.Create(block, eyebrow);
            Text(teaser, BuiltInElements.TextField, "Teaser of the preview only");
            preview.Add(teaser.Data);
            var link = ElementRow.CreateRef(block, ElementRef.ToPage(title.Id));
            link.SetValue(BuiltInElements.SizeStyle, BuiltInElements.SizeLarge);
            preview.Add(link.Data);
            preview.Add(ElementRow.CreateRef(block, ElementRef.ToPage("el_99")).Data);
            return block;
        }

        // The media rows S2b draws: a picture, a pair, a carousel, a video, around a title
        private static BlockInstanceData MediaRows()
        {
            var block = StoryBlock();
            Words(block, BuiltInElements.Title, "Pictures come next");
            Picture(block, "wide.png");
            var pair = Row(block, BuiltInElements.PicturePair);
            pair.AddMedia("then.png", null, "");
            pair.AddMedia("now.png", null, "");
            var carousel = Row(block, BuiltInElements.Carousel);
            carousel.AddMedia("one.png", null, "");
            carousel.AddMedia("two.png", null, "");
            Row(block, BuiltInElements.Video).AddMedia(LoopClip, null, "");
            Words(block, BuiltInElements.Paragraph, "The rows above are drawn by the next block of work.");
            return block;
        }

        // ---------------- builders ----------------

        private static BlockInstanceData StoryBlock()
        {
            var block = new BlockInstanceData { key = "block_2", kind = BuiltInBlocks.StoryKind };
            Value(block, BuiltInBlocks.StoryPageField);
            return block;
        }

        // A new Page row of `element`, born filled with its defaults (ElementRow.Create)
        private static ElementRow Row(BlockInstanceData block, string element)
        {
            ElementRegistry.Shared.TryGet(element, out var definition);
            var row = ElementRow.Create(block, definition);
            Value(block, BuiltInBlocks.StoryPageField).items.Add(row.Data);
            return row;
        }

        private static ElementRow Words(BlockInstanceData block, string element, string words)
        {
            var row = Row(block, element);
            Text(row, BuiltInElements.TextField, words);
            return row;
        }

        private static void Picture(BlockInstanceData block, string file) => Row(block, BuiltInElements.Picture).AddMedia(file, null, "");

        private static void Text(ElementRow row, string key, string words) => row.Data.fields.Add(ItemText(key, words));

        private static ElementRow Style(ElementRow row, string key, string value)
        {
            row.SetValue(key, value);
            return row;
        }

        // The block's stored value with this key, created when missing
        private static BlockFieldValue Value(BlockInstanceData block, string key)
        {
            var value = block.fields.Find(f => f.key == key);
            if (value != null) return value;
            value = new BlockFieldValue { key = key };
            block.fields.Add(value);
            return value;
        }
    }
}
