using System.Collections.Generic;

namespace TileStories
{
    // The framework's v1 elements (_3.3 R14.2), registered into ElementRegistry.Shared (and any registry a test builds): Text -- title,
    // eyebrow, heading, lede, paragraph, callout, pull quote, list; Media -- picture, picture pair, carousel, video; Structure -- divider,
    // spacer. Each style option is a closed list with a designed default, so a new row looks right before anyone touches it (R14.8).
    // Every help text is framework-authored and app-agnostic.
    public static class BuiltInElements
    {
        public const string Title = "title";
        public const string Eyebrow = "eyebrow";
        public const string Heading = "heading";
        public const string Lede = "lede";
        public const string Paragraph = "paragraph";
        public const string Callout = "callout";
        public const string PullQuote = "pull_quote";
        public const string List = "list";
        public const string Picture = "picture";
        public const string PicturePair = "picture_pair";
        public const string Carousel = "carousel";
        public const string Video = "video";
        public const string Divider = "divider";
        public const string Spacer = "spacer";

        // Content sub-fields
        public const string TextField = "text";
        public const string CalloutTitleField = "title";
        public const string AttributionField = "attribution";
        public const string PosterField = "poster";

        // Style options and their values
        public const string ToneStyle = "tone";
        public const string ToneNeutral = "neutral";
        public const string ToneFamily = "family";
        public const string ToneAccent = "accent";
        public const string ToneInverse = "inverse";
        public const string SizeStyle = "size";
        public const string SizeSmall = "s";
        public const string SizeMedium = "m";
        public const string SizeLarge = "l";
        public const string AlignStyle = "align";
        public const string AlignStart = "start";
        public const string AlignCentre = "center";
        public const string DropCapStyle = "drop_cap";
        public const string Off = "off";
        public const string On = "on";
        public const string ListStyle = "list_style";
        public const string ListBullets = "bullets";
        public const string ListNumbers = "numbers";
        public const string WidthStyle = "width";
        public const string WidthInset = "inset";
        public const string WidthFullBleed = "full_bleed";
        public const string FrameStyle = "frame";
        public const string FrameRectangle = "rectangle";
        public const string FrameArch = "arch";
        public const string OrnamentStyle = "ornament";
        public const string OrnamentLine = "line";
        public const string OrnamentAzulejo = "azulejo";

        // Reading weights: a picture is looked at for about 10 s, a carousel's pictures are swiped past faster, a video counts as half a
        // minute (its real length is not known to a pure rule; the block's Reading Time can always be set by hand)
        public const float PictureSeconds = 10f;
        public const float CarouselPictureSeconds = 6f;
        public const float VideoSeconds = 30f;
        public const int CarouselMaxPictures = 12;

        private const string LongTextHelp = "Leave an empty line between paragraphs. Write [[word]] to link a word to its Glossary entry.";
        private const string CaptionHelp = " Each picture may carry a caption in every wall language and a credit line.";

        // Register the v1 set into `registry`, in the order the "+ Add element" menu lists them
        public static void Register(ElementRegistry registry)
        {
            foreach (var element in All()) registry.Register(element);
        }

        // The v1 set, new definitions each call
        public static IReadOnlyList<ElementKindDefinition> All() => new[]
        {
            Text(Title, "Title", CardIcons.Book, ElementPreviewRole.Title, BlockFieldType.LocalizedText,
                "The story's title, the largest words of the page.", ToneNeutral, SizeLarge),
            Text(Eyebrow, "Eyebrow", CardIcons.Info, ElementPreviewRole.None, BlockFieldType.LocalizedText,
                "A few small words above a title that set the scene (a place, a date, a theme).", ToneFamily, SizeSmall),
            Text(Heading, "Heading", CardIcons.Sources, ElementPreviewRole.Title, BlockFieldType.LocalizedText,
                "A heading that opens a new part of the page.", ToneNeutral, SizeMedium),
            Text(Lede, "Lede", CardIcons.Sources, ElementPreviewRole.Text, BlockFieldType.LocalizedLongText,
                "The opening paragraph that sums the story up, larger than the text after it. " + LongTextHelp, ToneNeutral, SizeLarge),
            WithStyle(Text(Paragraph, "Paragraph", CardIcons.Sources, ElementPreviewRole.Text, BlockFieldType.LocalizedLongText,
                "A paragraph of the story. Drop Cap raises its first letter. " + LongTextHelp, ToneNeutral, SizeMedium),
                Choice(DropCapStyle, "Drop Cap", Off, new[] { Off, On }, new[] { "Off", "On" }, "Raise the paragraph's first letter beside its first lines.")),
            new ElementKindDefinition
            {
                Key = Callout, DisplayName = "Callout", IconKey = CardIcons.Light, Category = ElementCategory.Text,
                Help = "A highlighted box for something the reader should not miss, with an optional title. " + LongTextHelp,
                Fields = new[]
                {
                    TextSub(CalloutTitleField, "Title", BlockFieldType.LocalizedText, false, "The box's short title (optional)."),
                    TextSub(TextField, "Text", BlockFieldType.LocalizedLongText, true, "What the box says. " + LongTextHelp),
                },
                StyleFields = new[] { Tone(ToneAccent), Size(SizeMedium) },
                MotionChoices = TextMotions, AutoMotion = ElementOptions.MotionReveal,
                PreviewRole = ElementPreviewRole.None, SearchFields = new[] { CalloutTitleField, TextField },
            },
            new ElementKindDefinition
            {
                Key = PullQuote, DisplayName = "Pull Quote", IconKey = CardIcons.Chat, Category = ElementCategory.Text,
                Help = "A sentence lifted out of the story in large type, with who said it.",
                Fields = new[]
                {
                    TextSub(TextField, "Quote", BlockFieldType.LocalizedLongText, true, "The quoted words, without quotation marks."),
                    TextSub(AttributionField, "Attribution", BlockFieldType.LocalizedText, false, "Who said or wrote it (optional)."),
                },
                StyleFields = new[] { Tone(ToneFamily), Size(SizeLarge), Align(AlignCentre) },
                MotionChoices = TextMotions, AutoMotion = ElementOptions.MotionReveal,
                PreviewRole = ElementPreviewRole.None, SearchFields = new[] { TextField, AttributionField },
            },
            new ElementKindDefinition
            {
                Key = List, DisplayName = "List", IconKey = CardIcons.Menu, Category = ElementCategory.Text,
                Help = "A short list, one item per line, with bullets or numbers.",
                Fields = new[] { TextSub(TextField, "Items", BlockFieldType.LocalizedLongText, true, "One item per line.") },
                StyleFields = new[]
                {
                    Tone(ToneNeutral), Size(SizeMedium),
                    Choice(ListStyle, "Markers", ListBullets, new[] { ListBullets, ListNumbers }, new[] { "Bullets", "Numbers" }, "Bullets, or numbers when the order matters."),
                },
                MotionChoices = TextMotions, AutoMotion = ElementOptions.MotionReveal,
                PreviewRole = ElementPreviewRole.None, SearchFields = new[] { TextField },
            },
            Media(Picture, "Picture", CardIcons.Picture, MediaKind.Image, 1, 1, PictureSeconds, PictureMotions, ElementOptions.MotionParallax,
                new[] { ElementOptions.TapOpenViewer, ElementOptions.TapNone }, WidthInset,
                "One picture across the page." + CaptionHelp),
            Media(PicturePair, "Picture Pair", CardIcons.Split, MediaKind.Image, 2, 2, PictureSeconds, PairMotions, ElementOptions.MotionReveal,
                new[] { ElementOptions.TapOpenViewer, ElementOptions.TapNone }, WidthInset,
                "Two pictures side by side, for a comparison or two views of one thing." + CaptionHelp),
            Media(Carousel, "Carousel", CardIcons.Grid, MediaKind.Image, 2, CarouselMaxPictures, CarouselPictureSeconds, StripMotions, ElementOptions.MotionReveal,
                null, WidthFullBleed,
                "A strip of pictures the reader swipes through (2 to " + CarouselMaxPictures + ")." + CaptionHelp),
            WithFields(Media(Video, "Video", CardIcons.Video, MediaKind.Video, 1, 1, VideoSeconds, StripMotions, ElementOptions.MotionReveal,
                new[] { ElementOptions.TapPlayInPlace, ElementOptions.TapOpenPlayer }, WidthInset,
                "A video clip shown by its poster until it plays, with a caption and a credit."),
                new BlockFieldDefinition
                {
                    Key = PosterField, Type = BlockFieldType.Asset, Media = MediaKind.Image, Label = "Poster",
                    Help = "The picture shown before the video plays (optional: without one, the first frame).",
                }),
            new ElementKindDefinition
            {
                Key = Divider, DisplayName = "Divider", IconKey = CardIcons.More, Category = ElementCategory.Structure,
                Help = "A pause between two parts of the page: a thin line or a small tile ornament.",
                StyleFields = new[]
                {
                    Choice(OrnamentStyle, "Ornament", OrnamentLine, new[] { OrnamentLine, OrnamentAzulejo }, new[] { "Line", "Tile Ornament" }, "A thin line, or a small tile ornament."),
                },
                MotionChoices = new[] { ElementOptions.MotionAuto, ElementOptions.MotionNone, ElementOptions.MotionReveal }, AutoMotion = ElementOptions.MotionReveal,
            },
            new ElementKindDefinition
            {
                Key = Spacer, DisplayName = "Spacer", IconKey = CardIcons.More, Category = ElementCategory.Structure,
                Help = "Extra space between two rows, when a part of the page needs room to breathe.",
                StyleFields = new[] { Size(SizeMedium) },
                MotionChoices = new[] { ElementOptions.MotionAuto, ElementOptions.MotionNone }, AutoMotion = ElementOptions.MotionNone,
            },
        };

        // What text elements may do (Auto: reveal), what a single picture may do (Auto: a subtle parallax), what a pair may do, and a strip or
        // a clip, which never pan or zoom inside their frame
        private static readonly string[] TextMotions = { ElementOptions.MotionAuto, ElementOptions.MotionNone, ElementOptions.MotionReveal, ElementOptions.MotionSlideIn };
        private static readonly string[] PictureMotions = ElementOptions.Motions;
        private static readonly string[] PairMotions = { ElementOptions.MotionAuto, ElementOptions.MotionNone, ElementOptions.MotionReveal, ElementOptions.MotionSlideIn, ElementOptions.MotionParallax };
        private static readonly string[] StripMotions = TextMotions;

        // A one-text element: its words (required), tone + size + alignment, revealed on entry
        private static ElementKindDefinition Text(string key, string name, string icon, ElementPreviewRole role, BlockFieldType type, string help,
            string tone, string size) => new()
        {
            Key = key, DisplayName = name, IconKey = icon, Category = ElementCategory.Text, Help = help,
            Fields = new[] { TextSub(TextField, "Text", type, true, type == BlockFieldType.LocalizedLongText ? LongTextHelp : "The words, in every wall language.") },
            StyleFields = new[] { Tone(tone), Size(size), Align(AlignStart) },
            MotionChoices = TextMotions, AutoMotion = ElementOptions.MotionReveal,
            PreviewRole = role, SearchFields = new[] { TextField },
        };

        // A media element: its media list, width + frame, its motions and taps; the captions count for search, the credits do not
        private static ElementKindDefinition Media(string key, string name, string icon, MediaKind kind, int min, int max, float seconds,
            string[] motions, string auto, string[] taps, string width, string help) => new()
        {
            Key = key, DisplayName = name, IconKey = icon, Category = ElementCategory.Media, Help = help,
            Media = kind, MinMedia = min, MaxMedia = max, SecondsPerMedia = seconds,
            StyleFields = new[]
            {
                Choice(WidthStyle, "Width", width, new[] { WidthInset, WidthFullBleed }, new[] { "Inset", "Full Bleed" }, "Inside the text's margins, or edge to edge."),
                Choice(FrameStyle, "Frame", FrameRectangle, new[] { FrameRectangle, FrameArch }, new[] { "Rectangle", "Arch" }, "A plain rectangle, or an arch at the top."),
            },
            MotionChoices = motions, AutoMotion = auto, TapChoices = taps,
            PreviewRole = ElementPreviewRole.Picture, SearchesCaptions = true,
        };

        private static ElementKindDefinition WithStyle(ElementKindDefinition d, BlockFieldDefinition extra)
        {
            d.StyleFields = new List<BlockFieldDefinition>(d.StyleFields) { extra };
            return d;
        }

        private static ElementKindDefinition WithFields(ElementKindDefinition d, BlockFieldDefinition extra)
        {
            var fields = new List<BlockFieldDefinition>();
            if (d.Fields != null) fields.AddRange(d.Fields);
            fields.Add(extra);
            d.Fields = fields;
            return d;
        }

        private static BlockFieldDefinition TextSub(string key, string label, BlockFieldType type, bool required, string help) =>
            new() { Key = key, Type = type, Label = label, Required = required, Help = help };

        private static BlockFieldDefinition Choice(string key, string label, string defaultValue, string[] options, string[] labels, string help) =>
            new() { Key = key, Type = BlockFieldType.Choice, Label = label, Options = options, OptionLabels = labels, ChoiceDefault = defaultValue, Help = help };

        private static BlockFieldDefinition Tone(string defaultTone) => Choice(ToneStyle, "Tone", defaultTone,
            new[] { ToneNeutral, ToneFamily, ToneAccent, ToneInverse }, new[] { "Neutral", "Family Colour", "Accent", "Inverse" },
            "The colour of the words: the page's ink, the block family's colour, the accent, or light on dark. Every tone keeps the theme's contrast.");

        private static BlockFieldDefinition Size(string defaultSize) => Choice(SizeStyle, "Size", defaultSize,
            new[] { SizeSmall, SizeMedium, SizeLarge }, new[] { "Small", "Medium", "Large" }, "Small, medium or large, on the theme's scale.");

        private static BlockFieldDefinition Align(string defaultAlign) => Choice(AlignStyle, "Alignment", defaultAlign,
            new[] { AlignStart, AlignCentre }, new[] { "Start", "Centre" }, "Lined up on the text's margin, or centred.");
    }
}
