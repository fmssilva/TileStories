namespace TileStories
{
    // The about family: what the point is -- its header, condition, facts, texts, quotes, how it was made, its colours, and the buttons that act on it.
    // Part of BuiltInBlocks (BuiltInBlocks.cs registers every kind, in catalog order): each kind's key, variant and
    // field constants first, then the definitions.
    public static partial class BuiltInBlocks
    {
        // The card's identity block: the stack always starts with one (BlockStackBuilder)
        public const string HeaderKind = "header";

        public const string HeaderCompact = "compact";
        public const string HeaderTextOnly = "text_only";
        public const string HeaderShowLevelField = "show_level";
        // Tier 2: the looks with a picture under the title (the hero, BlockStackView puts it at the top of the scroll)
        public const string HeaderImageParallax = "image_parallax";
        public const string HeaderSplitThenNow = "split_then_now";
        public const string HeaderSpotlightCrop = "spotlight_crop";
        public static readonly string[] HeaderImageVariants = { HeaderImageParallax, HeaderSplitThenNow, HeaderSpotlightCrop };
        public const string HeaderImageField = "image";
        public const string HeaderSecondImageField = "second_image";
        public const string HeaderFocusXField = "focus_x";
        public const string HeaderFocusYField = "focus_y";
        public const string HeaderZoomField = "zoom";

        public const string StatusKind = "status";
        public const string StatusRing = "ring";
        public const string StatusScale = "scale";
        public const string StatusLabelField = "label";

        public const string QuickFactsKind = "quick_facts";
        public const string QuickFactsChips = "chips";
        public const string QuickFactsGrid = "grid_hairline";
        public const string QuickFactsBigNumbers = "big_numbers";
        public const string QuickFactsItemsField = "facts";
        public const string QuickFactsLabelField = "label";
        public const string QuickFactsValueField = "value";

        public const string RichTextKind = "rich_text";
        public const string RichTextPlain = "plain";
        public const string RichTextDropCap = "drop_cap";
        public const string RichTextLede = "lede";
        public const string RichTextSections = "sections";
        public const string RichTextBodyField = "body";
        public const string RichTextSectionsField = "sections";
        public const string RichTextSectionTitleField = "title";
        public const string RichTextSectionBodyField = "body";

        public const string FunFactKind = "fun_fact";
        public const string FunFactFlip = "flip";
        public const string FunFactPostcard = "postcard";
        public const string FunFactItemsField = "facts";
        public const string FunFactTextField = "text";

        public const string PullQuoteKind = "pull_quote";
        public const string PullQuoteSerif = "serif";
        public const string PullQuoteMinimal = "minimal";
        public const string PullQuoteTextField = "quote";
        public const string PullQuoteAuthorField = "author";
        public const string PullQuoteSourceField = "source";

        public const string ProcessStepsKind = "process_steps";
        public const string ProcessStepsNumbered = "numbered";
        public const string ProcessStepsItemsField = "steps";
        public const string ProcessStepsTitleField = "title";
        public const string ProcessStepsTextField = "text";

        public const string SwatchesKind = "swatches";
        public const string SwatchesGrid = "grid";
        public const string SwatchesItemsField = "swatches";
        public const string SwatchesNameField = "name";
        public const string SwatchesColourField = "colour";
        public const string SwatchesNoteField = "note";
        public const string SwatchesShowCodeField = "show_code";

        public const string ActionsKind = "actions";
        public const string ActionsCircles = "circles";
        public const string ActionsPillRow = "pill_row";
        public const string ActionsStickyCta = "sticky_cta";
        public const string ActionsItemsField = "actions";
        public const string ActionsLabelField = "label";
        public const string ActionsActionField = "action";
        // The actions a button can do today. listen (Tier 4 audio), save (Tier 3 local state) and open_block (_3.2
        // navigation) join this list with their tiers: a button is only offered once it can really do its job.
        public const string ActionShowOnWall = "show_on_wall";
        public static readonly string[] ActionOptions = { ActionShowOnWall };

        // Tier 1: the header; Tier 2 adds the picture looks. Video / 3D header variants arrive with Tiers 4 and 5.
        public static readonly BlockKindDefinition Header = new()
        {
            Key = HeaderKind,
            Family = "about",
            DisplayName = "Header",
            Help = "The top of the card: the point's title and its category chip, plus an optional subtitle. The chip names the " +
                   "point's category only; Show Level adds its hierarchy level. Compact: title and chip. Text only: with the " +
                   "subtitle. The picture looks show a picture under them, at the top of what scrolls: Image Parallax (the " +
                   "picture slides slower than the text as the visitor scrolls), Split Then Now (Picture as it was beside Second " +
                   "Picture as it is), Spotlight Crop (Picture enlarged around a focus point, with a ring on it). A picture look " +
                   "without its picture(s) shows the text-only look. It is always shown and always first; a point with no " +
                   "header block gets one made from its name and summary.",
            Variants = new[] { HeaderCompact, HeaderTextOnly, HeaderImageParallax, HeaderSplitThenNow, HeaderSpotlightCrop },
            DefaultVariant = HeaderTextOnly,
            DisplayModes = new[] { CardOptions.DisplayInline },
            Fields = new[]
            {
                new BlockFieldDefinition
                {
                    Key = BlockStackBuilder.HeaderTitleField, Type = BlockFieldType.LocalizedText, Label = "Title", Required = true,
                    Help = "The big title at the top of the card, one per language. Empty in every language: the header falls back to the point's name.",
                },
                new BlockFieldDefinition
                {
                    Key = BlockStackBuilder.HeaderSubtitleField, Type = BlockFieldType.LocalizedText, Label = "Subtitle",
                    Help = "One short line under the title (a date, a place, a one-line teaser). The Compact variant never shows it.",
                },
                new BlockFieldDefinition
                {
                    Key = HeaderShowLevelField, Type = BlockFieldType.Toggle, Label = "Show Level",
                    Help = "On: the chip under the title names the point's hierarchy level after its category (Category - " +
                           "Level). Off: the category alone -- a level name is often an authoring word a visitor does not need.",
                },
                new BlockFieldDefinition
                {
                    Key = HeaderImageField, Type = BlockFieldType.Asset, Media = MediaKind.Image, Label = "Picture",
                    Help = "The picture of the picture looks (in Split Then Now: as it was). A PNG or JPG inside the wall's Media " +
                           "Folder (Detail Card > Card Container). The text looks do not show it.",
                },
                new BlockFieldDefinition
                {
                    Key = HeaderSecondImageField, Type = BlockFieldType.Asset, Media = MediaKind.Image, Label = "Second Picture",
                    Help = "Split Then Now only: the picture as it is today, shown on the right. A PNG or JPG inside the Media Folder.",
                },
                new BlockFieldDefinition
                {
                    Key = HeaderFocusXField, Type = BlockFieldType.Number, Label = "Focus Across", NumberMin = 0f, NumberMax = 1f, NumberDefault = 0.5f,
                    Help = "Spotlight Crop only: where the point is across the picture, 0 = left edge, 1 = right edge.",
                },
                new BlockFieldDefinition
                {
                    Key = HeaderFocusYField, Type = BlockFieldType.Number, Label = "Focus Down", NumberMin = 0f, NumberMax = 1f, NumberDefault = 0.5f,
                    Help = "Spotlight Crop only: where the point is down the picture, 0 = top edge, 1 = bottom edge.",
                },
                new BlockFieldDefinition
                {
                    Key = HeaderZoomField, Type = BlockFieldType.Number, Label = "Crop Zoom", NumberMin = 1f, NumberMax = 4f, NumberDefault = 2f,
                    Help = "Spotlight Crop only: how much the picture is enlarged around the focus point (1 = the whole picture).",
                },
            },
        };

        // Whether a header look shows a picture, and whether this header has what that look needs (a picture the card
        // can load, and for Split Then Now the second one too). A picture look without it falls back to text only.
        public static bool HeaderShowsPicture(string variant, BlockInstanceData header)
        {
            if (System.Array.IndexOf(HeaderImageVariants, variant) < 0) return false;
            var read = new BlockFieldReader(header, null, null);
            if (read.ValidAsset(HeaderImageField, MediaKind.Image).Length == 0) return false;
            return variant != HeaderSplitThenNow || read.ValidAsset(HeaderSecondImageField, MediaKind.Image).Length > 0;
        }

        // Tier 1: the point's condition, drawn like its marker's outline
        public static readonly BlockKindDefinition Status = new()
        {
            Key = StatusKind,
            Family = "about",
            DisplayName = "Status",
            Help = "The point's condition, drawn the way its marker draws it: the same outline picture in the same colour " +
                   "(Global Scene > Outline and this point's Outline rows decide both). Ring: one outline beside the condition's " +
                   "name. Scale: every outline type in order of damage, this point's one marked. A condition nobody assessed " +
                   "shows a question mark. A point without a status never shows this block.",
            Variants = new[] { StatusRing, StatusScale },
            DefaultVariant = StatusRing,
            DisplayModes = new[] { CardOptions.DisplayInline },
            ShowsFor = (poi, _, _) => poi != null && poi.has_status,
            NotShownForPoiNote = "this point has no status (Specific Marker > Outline > Has status is off).",
            Fields = new[]
            {
                new BlockFieldDefinition
                {
                    Key = StatusLabelField, Type = BlockFieldType.LocalizedText, Label = "Label",
                    Help = "Your own words for the condition. Empty: the name of the point's outline type (Global Scene > Outline).",
                },
            },
        };

        // Tier 1: short label + value facts
        public static readonly BlockKindDefinition QuickFacts = new()
        {
            Key = QuickFactsKind,
            Family = "about",
            DisplayName = "Quick Facts",
            Help = "A few short facts at a glance, each a label and a value (Built: 1147). Chips: small pills that wrap. " +
                   "Grid Hairline: two columns split by thin lines. Big Numbers: the value large with its label under it, best for " +
                   "numbers and dates. A row with nothing in it is not shown.",
            Variants = new[] { QuickFactsChips, QuickFactsGrid, QuickFactsBigNumbers },
            DefaultVariant = QuickFactsGrid,
            DisplayModes = new[] { CardOptions.DisplayInline },
            Fields = new[]
            {
                new BlockFieldDefinition
                {
                    Key = QuickFactsItemsField, Type = BlockFieldType.Items, Label = "Facts", Required = true,
                    Help = "One row per fact. Keep both texts short: a label of one or two words and a value of a few.",
                    ItemFields = new[]
                    {
                        new BlockFieldDefinition { Key = QuickFactsLabelField, Type = BlockFieldType.LocalizedText, Label = "Label", Help = "What the fact is (Built, Height, Architect)." },
                        new BlockFieldDefinition { Key = QuickFactsValueField, Type = BlockFieldType.LocalizedText, Label = "Value", Help = "The fact itself (1147, 32 m, Unknown)." },
                    },
                },
            },
        };

        // Tier 1: surprising facts, one small card each
        public static readonly BlockKindDefinition FunFact = new()
        {
            Key = FunFactKind,
            Family = "about",
            DisplayName = "Fun Fact",
            Help = "Surprising facts about the point, one small card per fact under the heading of Detail Card > Card Texts. " +
                   "Flip: the fact is hidden until the visitor taps the card (a tap on the heading hides it again). Postcard: " +
                   "the fact shown at once on a tilted postcard. " + LongTextHelp,
            Variants = new[] { FunFactFlip, FunFactPostcard },
            DefaultVariant = FunFactFlip,
            DisplayModes = new[] { CardOptions.DisplayInline },
            Fields = new[]
            {
                new BlockFieldDefinition
                {
                    Key = FunFactItemsField, Type = BlockFieldType.Items, Label = "Facts", Required = true,
                    Help = "One row per fact: one or two sentences each.",
                    ItemFields = new[]
                    {
                        new BlockFieldDefinition { Key = FunFactTextField, Type = BlockFieldType.LocalizedLongText, Label = "Fact", Help = "The fact itself. " + LongTextHelp },
                    },
                },
            },
        };

        // Tier 1: a quotation set apart
        public static readonly BlockKindDefinition PullQuote = new()
        {
            Key = PullQuoteKind,
            Family = "about",
            DisplayName = "Pull Quote",
            Help = "A quotation set apart from the text: what someone wrote or said about the point, with who and where. " +
                   "Serif: large italic text behind an accent bar, the author and the source under it. Minimal: body-size " +
                   "italic text with the author and the source on one line. Author and Source may stay empty.",
            Variants = new[] { PullQuoteSerif, PullQuoteMinimal },
            DefaultVariant = PullQuoteSerif,
            DisplayModes = new[] { CardOptions.DisplayInline },
            Fields = new[]
            {
                new BlockFieldDefinition
                {
                    Key = PullQuoteTextField, Type = BlockFieldType.LocalizedLongText, Label = "Quote", Required = true,
                    Help = "The quoted words, without quotation marks (the look sets them apart). " + LongTextHelp,
                },
                new BlockFieldDefinition
                {
                    Key = PullQuoteAuthorField, Type = BlockFieldType.LocalizedText, Label = "Author",
                    Help = "Who wrote or said it (a name, or a role such as A chronicler).",
                },
                new BlockFieldDefinition
                {
                    Key = PullQuoteSourceField, Type = BlockFieldType.LocalizedText, Label = "Source",
                    Help = "Where it comes from: a book, a letter, a year.",
                },
            },
        };

        // Tier 1: how something was made or done, step by step
        public static readonly BlockKindDefinition ProcessSteps = new()
        {
            Key = ProcessStepsKind,
            Family = "about",
            DisplayName = "Process Steps",
            Help = "How something was made or done, step by step (how the tiles were fired, how the wall was restored). " +
                   "Numbered: each step's number in a circle beside its title and text, the steps joined by a line, in the " +
                   "order of the rows. A row with no title is not shown. " + LongTextHelp,
            Variants = new[] { ProcessStepsNumbered },
            DefaultVariant = ProcessStepsNumbered,
            DisplayModes = new[] { CardOptions.DisplayInline },
            Fields = new[]
            {
                new BlockFieldDefinition
                {
                    Key = ProcessStepsItemsField, Type = BlockFieldType.Items, Label = "Steps", Required = true,
                    Help = "One row per step, first step first. The card numbers them itself.",
                    ItemFields = new[]
                    {
                        new BlockFieldDefinition { Key = ProcessStepsTitleField, Type = BlockFieldType.LocalizedText, Label = "Title", Required = true, Help = "The step in a few words (Shape the clay)." },
                        new BlockFieldDefinition { Key = ProcessStepsTextField, Type = BlockFieldType.LocalizedLongText, Label = "Text", Help = "What happens in this step, in a sentence or two. May stay empty. " + LongTextHelp },
                    },
                },
            },
        };

        // Tier 1: the colours of the thing itself (a palette, the pigments of a panel)
        public static readonly BlockKindDefinition Swatches = new()
        {
            Key = SwatchesKind,
            Family = "about",
            DisplayName = "Swatches",
            Help = "The colours of the point itself (the pigments of a tile panel, the paints of a facade), each a sample with " +
                   "its name and an optional note. Grid: the samples in two columns. These colours are content, shown exactly as " +
                   "written; the card's own colours never change them. A row with no name, or whose colour is not written as " +
                   "#RRGGBB, is not shown.",
            Variants = new[] { SwatchesGrid },
            DefaultVariant = SwatchesGrid,
            DisplayModes = new[] { CardOptions.DisplayInline },
            Fields = new[]
            {
                new BlockFieldDefinition
                {
                    Key = SwatchesItemsField, Type = BlockFieldType.Items, Label = "Swatches", Required = true,
                    Help = "One row per colour, in the order the card shows them.",
                    ItemFields = new[]
                    {
                        new BlockFieldDefinition { Key = SwatchesNameField, Type = BlockFieldType.LocalizedText, Label = "Name", Required = true, Help = "The colour's name (Cobalt blue)." },
                        new BlockFieldDefinition
                        {
                            Key = SwatchesColourField, Type = BlockFieldType.Color, Label = "Colour", Required = true,
                            Help = "The colour itself: pick it, or type it as #RRGGBB (#1F3F8F). A colour written any other way is not shown.",
                        },
                        new BlockFieldDefinition { Key = SwatchesNoteField, Type = BlockFieldType.LocalizedText, Label = "Note", Help = "A few words on where or why it is used (The outlines). May stay empty." },
                    },
                },
                new BlockFieldDefinition
                {
                    Key = SwatchesShowCodeField, Type = BlockFieldType.Toggle, Label = "Show Code",
                    Help = "On: each sample also prints its colour code (#1F3F8F), for visitors who work with colour. Off: the " +
                           "name and the note only.",
                },
            },
        };

        // Tier 1: buttons that do something for the visitor
        public static readonly BlockKindDefinition Actions = new()
        {
            Key = ActionsKind,
            Family = "about",
            DisplayName = "Actions",
            Help = "Buttons that do something for the visitor, each with your own words and one action. Show On The Wall: " +
                   "lowers the card so the point's marker shows on the wall. Circles: round buttons with the words under them. " +
                   "Pill Row: small pills side by side. Sticky: the FIRST button only, full width and pinned to the bottom of the card, " +
                   "always in reach at Half and Full (one call to action). More actions arrive with the blocks that need them.",
            Variants = new[] { ActionsCircles, ActionsPillRow, ActionsStickyCta },
            DefaultVariant = ActionsPillRow,
            DisplayModes = new[] { CardOptions.DisplayInline },
            FooterVariants = new[] { ActionsStickyCta },
            Fields = new[]
            {
                new BlockFieldDefinition
                {
                    Key = ActionsItemsField, Type = BlockFieldType.Items, Label = "Buttons", Required = true,
                    Help = "One row per button. A row with no words, or with no action picked, is not shown.",
                    ItemFields = new[]
                    {
                        new BlockFieldDefinition { Key = ActionsLabelField, Type = BlockFieldType.LocalizedText, Label = "Words", Help = "What the button says (See it on the wall)." },
                        new BlockFieldDefinition
                        {
                            Key = ActionsActionField, Type = BlockFieldType.Choice, Label = "Action", Required = true,
                            Options = ActionOptions, OptionLabels = new[] { "Show On The Wall" },
                            Help = "What the button does. Show On The Wall: the card lowers to its title so the marker shows on the wall.",
                        },
                    },
                },
            },
        };

        // Tier 1: text in paragraphs, with glossary words
        public static readonly BlockKindDefinition RichText = new()
        {
            Key = RichTextKind,
            Family = "about",
            DisplayName = "Rich Text",
            Help = "Paragraphs of text about the point. Plain: paragraphs only. Drop Cap: the first letter raised beside the first " +
                   "paragraph. Lede: a larger first paragraph that sums the point up. Sections: the text as an introduction, then " +
                   "one heading and its paragraphs per Sections row. " + LongTextHelp,
            Variants = new[] { RichTextPlain, RichTextDropCap, RichTextLede, RichTextSections },
            DefaultVariant = RichTextPlain,
            DisplayModes = new[] { CardOptions.DisplayInline },
            Fields = new[]
            {
                new BlockFieldDefinition
                {
                    Key = RichTextBodyField, Type = BlockFieldType.LocalizedLongText, Label = "Text", Required = true,
                    Help = "The paragraphs, one text per language. " + LongTextHelp,
                },
                new BlockFieldDefinition
                {
                    Key = RichTextSectionsField, Type = BlockFieldType.Items, Label = "Sections",
                    Help = "Headed parts after the text, shown only by the Sections look: one row per part, each a heading and its paragraphs.",
                    ItemFields = new[]
                    {
                        new BlockFieldDefinition
                        {
                            Key = RichTextSectionTitleField, Type = BlockFieldType.LocalizedText, Label = "Heading",
                            Help = "The part's short heading.",
                        },
                        new BlockFieldDefinition
                        {
                            Key = RichTextSectionBodyField, Type = BlockFieldType.LocalizedLongText, Label = "Text",
                            Help = "The part's paragraphs. " + LongTextHelp,
                        },
                    },
                },
            },
        };
    }
}
