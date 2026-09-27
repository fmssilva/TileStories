namespace TileStories
{
    // The framework's own block kinds (_3.1 section 6), registered once into BlockRegistry.Shared (and into any
    // registry a test builds). Kinds are added tier by tier, one at a time (_3.1 section 7). Every help text is
    // framework-authored and app-agnostic: it names Editor Tab controls, never a wall's own content.
    public static class BuiltInBlocks
    {
        // The card's identity block: the stack always starts with one (BlockStackBuilder)
        public const string HeaderKind = "header";

        public const string HeaderCompact = "compact";
        public const string HeaderTextOnly = "text_only";

        public const string RichTextKind = "rich_text";
        public const string RichTextPlain = "plain";
        public const string RichTextDropCap = "drop_cap";
        public const string RichTextLede = "lede";
        public const string RichTextSections = "sections";
        public const string RichTextBodyField = "body";
        public const string RichTextSectionsField = "sections";
        public const string RichTextSectionTitleField = "title";
        public const string RichTextSectionBodyField = "body";

        public const string QuickFactsKind = "quick_facts";
        public const string QuickFactsChips = "chips";
        public const string QuickFactsGrid = "grid_hairline";
        public const string QuickFactsBigNumbers = "big_numbers";
        public const string QuickFactsItemsField = "facts";
        public const string QuickFactsLabelField = "label";
        public const string QuickFactsValueField = "value";

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

        public const string StatusKind = "status";
        public const string StatusRing = "ring";
        public const string StatusScale = "scale";
        public const string StatusLabelField = "label";

        public const string SourcesKind = "sources";
        public const string SourcesList = "list";
        public const string SourcesWithConfidence = "with_confidence";
        public const string SourcesItemsField = "sources";
        public const string SourcesTitleField = "title";
        public const string SourcesAuthorField = "author";
        public const string SourcesLicenceField = "licence";
        public const string SourcesStatusField = "content_status";
        public const string ContentVerified = "verified";
        public const string ContentDraft = "draft";

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

        // What every long-text help says about paragraphs and glossary words
        private const string LongTextHelp =
            "Leave an empty line between paragraphs. Write [[word]] to link a word to its Glossary entry (Detail Card > " +
            "Glossary): the visitor taps it to read the definition. [[shown words|word]] shows other words for the same entry.";

        // Register every built-in kind into `registry`
        public static void Register(BlockRegistry registry)
        {
            registry.Register(Header, () => new HeaderBlockView());
            registry.Register(Status, () => new StatusBlockView());
            registry.Register(QuickFacts, () => new QuickFactsBlockView());
            registry.Register(RichText, () => new RichTextBlockView());
            registry.Register(FunFact, () => new FunFactBlockView());
            registry.Register(PullQuote, () => new PullQuoteBlockView());
            registry.Register(Sources, () => new SourcesBlockView());
            registry.Register(Actions, () => new ActionsBlockView());
        }

        // Tier 1: the header. Image / video / 3D header variants arrive with Tiers 2, 4 and 5.
        public static readonly BlockKindDefinition Header = new()
        {
            Key = HeaderKind,
            Family = "about",
            DisplayName = "Header",
            Help = "The top of the card: the point's title and its category chip, plus an optional subtitle. " +
                   "It is always shown and always first; a point with no header block gets one made from its name and summary.",
            Variants = new[] { HeaderCompact, HeaderTextOnly },
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
            },
        };

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
            ShowsFor = poi => poi != null && poi.has_status,
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

        // Tier 1: where the card's content comes from (the meta family: always last, _3.2)
        public static readonly BlockKindDefinition Sources = new()
        {
            Key = SourcesKind,
            Family = "meta",
            DisplayName = "Sources",
            Help = "Where this card's content comes from: books, archives, photographs, with their author and licence. " +
                   "List: the sources under a heading. With Confidence: the same, plus a chip that says whether the content " +
                   "was checked (Verified) or is still a draft (Content Status). The heading and the chip words are Detail Card > Card Texts.",
            Variants = new[] { SourcesList, SourcesWithConfidence },
            DefaultVariant = SourcesList,
            DisplayModes = new[] { CardOptions.DisplayInline },
            Fields = new[]
            {
                new BlockFieldDefinition
                {
                    Key = SourcesItemsField, Type = BlockFieldType.Items, Label = "Sources", Required = true,
                    Help = "One row per source.",
                    ItemFields = new[]
                    {
                        new BlockFieldDefinition { Key = SourcesTitleField, Type = BlockFieldType.LocalizedText, Label = "Title", Help = "The book, archive, photograph or page." },
                        new BlockFieldDefinition { Key = SourcesAuthorField, Type = BlockFieldType.LocalizedText, Label = "Author", Help = "Who made it (a person, an institution)." },
                        new BlockFieldDefinition { Key = SourcesLicenceField, Type = BlockFieldType.LocalizedText, Label = "Licence", Help = "Its licence or rights line (CC BY 4.0, Public domain, With permission)." },
                    },
                },
                new BlockFieldDefinition
                {
                    Key = SourcesStatusField, Type = BlockFieldType.Choice, Label = "Content Status",
                    Options = new[] { ContentVerified, ContentDraft }, OptionLabels = new[] { "Verified", "Draft" },
                    Help = "Whether this card's content was checked against its sources (Verified) or is still a draft (Draft). " +
                           "Only the With Confidence look shows it; (none) shows no chip.",
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
