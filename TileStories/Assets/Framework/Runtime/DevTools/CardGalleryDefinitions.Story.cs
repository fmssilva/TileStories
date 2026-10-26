using System.Collections.Generic;

namespace TileStories
{
    // The story part of the gallery list (_3.3 S2a, S2b): the composed kind's PREVIEW on the card and its PAGE in the full view. A magazine (the
    // prototype's shape: a picture, eyebrow, title, lede, paragraphs, a drop cap, headings, a callout, a pull quote, a divider -- long enough to
    // be cut), a short one (shown whole), one SHEET per element whose Page holds a row per style option (every tone, size, alignment, the drop
    // cap, numbers, the ornament; every media element's width x frame, with its media, captions and credits), the Open Row's three emphases,
    // a Custom preview, the four media rows together (a video that plays in place, and one that opens the player), and one story per motion
    // the frame sequences record (a reveal, a parallax picture, a slow zoom beside a pan). Each entry is one language (English).
    public static partial class CardGalleryDefinitions
    {
        // The content names of the story entries (Entry.Content)
        public const string StoryMagazine = "magazine";
        public const string StoryShort = "short";
        // A preview taller than the cap whatever its picture: a long lede (the magazine's preview fits until its picture is drawn, S2b)
        public const string StoryTall = "tall";
        public const string StoryCustom = "custom";
        public const string StoryMedia = "media";
        // The media rows with a video whose tap opens the full-screen player
        public const string StoryPlayer = "player";
        // One story per motion the frame sequences record (StoryMotionPrefix + effect)
        public const string StoryMotionPrefix = "motion_";
        public static readonly string[] StoryMotions = { ElementOptions.MotionReveal, ElementOptions.MotionParallax, ElementOptions.MotionSlowZoom };
        public const string StorySheetPrefix = "sheet_";
        public const string StoryEmphasisPrefix = "emphasis_";

        // The text and structure elements, in the order their sheets are listed
        public static readonly string[] StorySheetElements =
        {
            BuiltInElements.Title, BuiltInElements.Eyebrow, BuiltInElements.Heading, BuiltInElements.Lede, BuiltInElements.Paragraph,
            BuiltInElements.Callout, BuiltInElements.PullQuote, BuiltInElements.List, BuiltInElements.Divider, BuiltInElements.Spacer,
        };

        // The media elements, in the order their sheets are listed
        public static readonly string[] StoryMediaSheetElements =
        {
            BuiltInElements.Picture, BuiltInElements.PicturePair, BuiltInElements.Carousel, BuiltInElements.Video,
        };

        // The gallery's media a media row shows: its pictures (CardGalleryDefinitions.Pictures), a video clip (Videos) and its poster
        public const string StoryVideoClip = "short.mp4";
        public const string StoryVideoPoster = "wide.png";
        public static readonly string[] StoryCarouselPictures = { "one.png", "two.png", "three.png", "four.png" };

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
            foreach (string element in StoryMediaSheetElements)
                list.Add(new Entry(BuiltInBlocks.StoryKind, BuiltInBlocks.StoryMagazine, StorySheetPrefix + element, Sheet(element)));
            foreach (string emphasis in ElementOptions.OpenRowEmphases)
                list.Add(new Entry(BuiltInBlocks.StoryKind, BuiltInBlocks.StoryMagazine, StoryEmphasisPrefix + emphasis, ShortStory(emphasis)));
            list.Add(new Entry(BuiltInBlocks.StoryKind, BuiltInBlocks.StoryMagazine, StoryCustom, CustomPreview()));
            list.Add(new Entry(BuiltInBlocks.StoryKind, BuiltInBlocks.StoryMagazine, StoryMedia, MediaRows(ElementOptions.TapPlayInPlace)));
            list.Add(new Entry(BuiltInBlocks.StoryKind, BuiltInBlocks.StoryMagazine, StoryPlayer, MediaRows(ElementOptions.TapOpenPlayer)));
            foreach (string motion in StoryMotions)
                list.Add(new Entry(BuiltInBlocks.StoryKind, BuiltInBlocks.StoryMagazine, StoryMotionPrefix + motion, MotionStory(motion)));
        }

        // The magazine: the prototype's page in fabricated words, a full-bleed picture first
        private static BlockInstanceData Magazine()
        {
            var block = StoryBlock();
            Style(Picture(block, "wide.png"), BuiltInElements.WidthStyle, BuiltInElements.WidthFullBleed);
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

        // An element's sheet: one row per style option, the defaults first. Words long enough to wrap on the phone (the size and alignment
        // show); a media row with its media, captions and credits
        private static BlockInstanceData Sheet(string key)
        {
            var block = StoryBlock();
            ElementRegistry.Shared.TryGet(key, out var element);
            foreach (var sheetRow in SheetRowsOf(element))
            {
                var row = Row(block, key);
                if (element.Category == ElementCategory.Media) FillMedia(row, key, sheetRow.Option.Length == 0 ? "Default" : sheetRow.Option + " " + sheetRow.Value);
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

        // The four media rows on one page, around words: a picture, a pair, a carousel, a video whose tap is `videoTap`
        private static BlockInstanceData MediaRows(string videoTap)
        {
            var block = StoryBlock();
            Words(block, BuiltInElements.Title, "The square by the river");
            FillMedia(Row(block, BuiltInElements.Picture), BuiltInElements.Picture, "");
            Words(block, BuiltInElements.Paragraph, "Later kings kept adding to it. The river front received a new facade and a domed corner tower that sailors saw long before they saw the city.");
            FillMedia(Row(block, BuiltInElements.PicturePair), BuiltInElements.PicturePair, "");
            FillMedia(Row(block, BuiltInElements.Carousel), BuiltInElements.Carousel, "");
            Words(block, BuiltInElements.Paragraph, "Many still call the square by its old name: the palace yard.");
            FillMedia(Row(block, BuiltInElements.Video), BuiltInElements.Video, "").SetValue(ElementRow.TapKey, videoTap);
            Words(block, BuiltInElements.Paragraph, "The rows above are the story's media, and the page goes on under them so it scrolls.");
            Words(block, BuiltInElements.Paragraph, "Where the palace had stood, an open square was laid out, facing the river like a grand front door.");
            return block;
        }

        // A story long enough to scroll on its page, with the motion `effect` on its picture: a reveal on every row (the picture too); a
        // parallax picture; a slow zoom with a strong pan beside it lower down, so only one of the two moves at a time
        private static BlockInstanceData MotionStory(string effect)
        {
            var block = StoryBlock();
            Words(block, BuiltInElements.Title, "Two hundred and fifty years by the river");
            Words(block, BuiltInElements.Lede, "For about 250 years the kings lived by the river, in a palace that opened straight onto the water.");
            Words(block, BuiltInElements.Paragraph, "When the king found the old castle on the hill too cramped for a court growing rich on the sea trade, he moved down to the water's edge.");
            Words(block, BuiltInElements.Paragraph, "The new palace stood beside the docks where ships unloaded spices, sugar and gold, so the king could watch the fleet come in.");
            Picture(block, "wide.png").SetMotion(effect, ElementOptions.StrengthSubtle);
            Words(block, BuiltInElements.Heading, "Two centuries of additions");
            Words(block, BuiltInElements.Paragraph, "Later kings kept adding to it. The river front received a new facade and a domed corner tower that sailors saw long before they saw the city.");
            if (effect == ElementOptions.MotionSlowZoom) Picture(block, "four.png").SetMotion(ElementOptions.MotionPan, ElementOptions.StrengthStrong);
            Words(block, BuiltInElements.Paragraph, "Inside were a famous library of tens of thousands of books, paintings and maps, and a royal opera house.");
            Words(block, BuiltInElements.Heading, "One morning in November");
            Words(block, BuiltInElements.Paragraph, "On a morning in November an earthquake shook the palace, a wave came up the river and fire finished what was left.");
            Words(block, BuiltInElements.Paragraph, "Where the palace had stood, an open square was laid out, facing the river like a grand front door.");
            Words(block, BuiltInElements.Paragraph, "Many still call it by its old name, the palace yard, and the river steps still lead down to the water.");
            return block;
        }

        // A media row's media: a picture (wide), a pair (then / now), a carousel (four), a video (the clip and its poster); each item with a
        // caption (`label` first, so a sheet row says which option it shows) and a credit
        private static ElementRow FillMedia(ElementRow row, string key, string label)
        {
            string Caption(string words) => label.Length > 0 ? label + ": " + words : words;
            switch (key)
            {
                case BuiltInElements.Picture:
                    row.AddMedia("wide.png", CaptionIn(Caption("the palace front and its garden, drawn for a firework display")), "Print by an unknown artist. Public domain.");
                    break;
                case BuiltInElements.PicturePair:
                    row.AddMedia("then.png", CaptionIn(label.Length > 0 ? label : "Before"), "Anonymous painting. Public domain.");
                    row.AddMedia("now.png", CaptionIn("Today"), "Photograph, CC BY 4.0.");
                    break;
                case BuiltInElements.Carousel:
                    for (int i = 0; i < StoryCarouselPictures.Length; i++)
                        row.AddMedia(StoryCarouselPictures[i], CaptionIn(Caption("picture " + (i + 1) + " of the strip")), i == 0 ? "Public domain." : "");
                    break;
                case BuiltInElements.Video:
                    row.AddMedia(StoryVideoClip, CaptionIn(label.Length > 0 ? label : "The square today"), "Video by the gallery, CC BY-SA 3.0.");
                    row.Data.fields.Add(new BlockItemFieldValue { key = BuiltInElements.PosterField, asset = StoryVideoPoster });
                    break;
            }
            return row;
        }

        private static List<LocalizedEntry> CaptionIn(string words) => new() { new LocalizedEntry { lang = "en", value = words } };

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

        private static ElementRow Picture(BlockInstanceData block, string file)
        {
            var row = Row(block, BuiltInElements.Picture);
            row.AddMedia(file, null, "");
            return row;
        }

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
