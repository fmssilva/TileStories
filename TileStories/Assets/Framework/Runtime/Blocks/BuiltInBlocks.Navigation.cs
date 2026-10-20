namespace TileStories
{
    // The navigation family: blocks IN the card's flow that move the visitor inside it (_3.2 section 5.6). Part of BuiltInBlocks
    // (BuiltInBlocks.cs registers every kind, in catalog order). A nav block is never a navigation target itself (its family has In Nav
    // off), so no tab, bar or menu ever lists it. Detail Card > Navigation > Auto Nav Block can put one after the header of every card
    // with enough blocks (BlockStackBuilder.InsertAutoNavBlock); a point may also author one in Card Content.
    public static partial class BuiltInBlocks
    {
        public const string NavIconGridKind = "nav_icon_grid";
        public const string NavIconGridTiles = "tiles";

        // _3.2 D1: the card's sections as a grid of tiles -- icon, words, block count -- a tap jumps to the section's first block
        public static readonly BlockKindDefinition NavIconGrid = new()
        {
            Key = NavIconGridKind,
            Family = BuiltInFamilies.Navigation,
            Format = CardOptions.FormatInteract,
            LabelKey = CardStrings.Keys.KindNavIconGrid,
            DisplayName = "Icon Grid",
            Help = "A grid of the card's sections -- each tile its icon, its words and how many blocks it holds -- so the visitor sees what " +
                   "the card offers at a glance; a tap moves the card to that section's first block (as the Top Bar's tabs do). The tiles " +
                   "follow Detail Card > Navigation (Group By, the Families table). A card with fewer than two sections draws no tiles. Put " +
                   "it after the header by hand, or let Detail Card > Navigation > Auto Nav Block add it to every card with enough blocks.",
            Variants = new[] { NavIconGridTiles },
            DefaultVariant = NavIconGridTiles,
            DisplayModes = new[] { CardOptions.DisplayInline, CardOptions.DisplayExpandable },
            // - a nav block is never a row of a contents list (its family is out of navigation): no Reading Time
            OffersReadingTime = false,
        };

        public const string NavContentsKind = "nav_contents";
        public const string NavContentsList = "list";
        // Rows (_3.2 step 13): the section titles first, each opening its rows (the default: a long card's contents never shows every row at
        // once), or every row under its title as before
        public const string NavContentsRowsField = "rows";
        public const string NavContentsSectionsFirst = "sections_first";
        public const string NavContentsEveryBlock = "every_block";

        // _3.2 D2: the card's contents -- one row per block it can take the visitor to, grouped under its section in the card's order, with
        // the block's minutes and a tick once the visitor has had it in view (remembered on the device); a tap moves the card to that block
        public static readonly BlockKindDefinition NavContents = new()
        {
            Key = NavContentsKind,
            Family = BuiltInFamilies.Navigation,
            Format = CardOptions.FormatInteract,
            LabelKey = CardStrings.Keys.KindNavContents,
            DisplayName = "Contents",
            Help = "A list of the card's blocks, the way a book's contents lists its chapters: under each section's name a row per block -- its " +
                   "heading, how many minutes it takes (its Reading Time; an audio or video block shows its own clip's length) and a tick " +
                   "once the visitor has had that block in view on this device. A tap moves the card to that block. It suits a long card a " +
                   "visitor reads in several visits: the ticks show what is left. It lists blocks, not sections, so a card whose blocks all " +
                   "belong to one family still gets it. Detail Card > Card Container > Test > Reset Saved Card State clears the ticks on " +
                   "this computer.",
            Variants = new[] { NavContentsList },
            DefaultVariant = NavContentsList,
            DisplayModes = new[] { CardOptions.DisplayInline, CardOptions.DisplayExpandable },
            // - it lists the card's rows; it is none of them itself
            OffersReadingTime = false,
            Fields = new[]
            {
                new BlockFieldDefinition
                {
                    Key = NavContentsRowsField, Type = BlockFieldType.Choice, Label = "Rows",
                    Options = new[] { NavContentsSectionsFirst, NavContentsEveryBlock },
                    OptionLabels = new[] { "Sections First", "Every Block" },
                    ChoiceDefault = NavContentsSectionsFirst,
                    LibraryDefault = true,
                    Help = "Sections First (the default): the list shows each section's name with how many of its blocks were read; a tap on a " +
                           "name opens its rows under it, and another tap folds them away -- a long card's contents stays short until the " +
                           "visitor asks for more. Every Block: every row shows under its section's name at once. A Contents block the card " +
                           "puts in by itself (Detail Card > Navigation > Auto Nav Block) always opens with its sections folded.",
                },
            },
        };

        public const string NavRailsKind = "nav_rails";
        public const string NavRailsRails = "rails";
        // The most swipes a rail may take to reach its last card (NavRailRule): a family with more blocks ends its rail on an "All n" card
        public const string NavRailsMaxSwipesField = "max_swipes";

        // _3.2 D3: one horizontal rail per section, a card per block in it (its icon, words, format and minutes), a tap moves the card to the block
        public static readonly BlockKindDefinition NavRails = new()
        {
            Key = NavRailsKind,
            Family = BuiltInFamilies.Navigation,
            Format = CardOptions.FormatInteract,
            LabelKey = CardStrings.Keys.KindNavRails,
            DisplayName = "Rails",
            Help = "One row per section of the card -- its name, then a card for each block in it, swiped sideways -- so the visitor browses " +
                   "what is in each part like shelves. A card shows the block's icon, its words, its kind of content and its minutes; a tap " +
                   "moves the card to that block. The next card shows cut at the edge as the cue to swipe (Detail Card > Card Container > Peek " +
                   "Next Card decides whether it does). A section with more blocks than Swipes To The End allows ends its row on an \"All\" card " +
                   "that moves the card to the section, so every block stays one tap away. The rows follow Detail Card > Navigation (Group By, " +
                   "the Families table). Put it after the header by hand, or let Detail Card > Navigation > Auto Nav Block add it to every card " +
                   "with enough blocks.",
            Variants = new[] { NavRailsRails },
            DefaultVariant = NavRailsRails,
            DisplayModes = new[] { CardOptions.DisplayInline, CardOptions.DisplayExpandable },
            // - a nav block is never a row of a contents list (its family is out of navigation): no Reading Time
            OffersReadingTime = false,
            Fields = new[]
            {
                new BlockFieldDefinition
                {
                    Key = NavRailsMaxSwipesField, Type = BlockFieldType.Number, Label = "Swipes To The End", NumberMin = 2f, NumberMax = 8f, NumberDefault = 4f,
                    Help = "How many swipes at most a visitor needs to reach the last card of a row (one swipe moves the row about one screen " +
                           "width). Visitors give up on a row that takes more than 3 or 4. A section with more blocks than fit ends its row on " +
                           "an \"All\" card that moves the card to that section.",
                },
            },
        };

        public const string NavBentoKind = "nav_bento";
        public const string NavBentoTiles = "tiles";
        public const string NavBentoTilesField = "tiles";
        public const string NavBentoTargetField = "target";
        public const string NavBentoWordsField = "words";
        public const string NavBentoPictureField = "picture";
        public const string NavBentoOpensField = "opens";
        public const string NavBentoOpensJump = "jump";
        public const string NavBentoOpensFullView = "full_view";

        // _3.2 D4: a few tiles the author picked, each leading to one block of the card, with the author's own words and picture if given
        public static readonly BlockKindDefinition NavBento = new()
        {
            Key = NavBentoKind,
            Family = BuiltInFamilies.Navigation,
            Format = CardOptions.FormatInteract,
            LabelKey = CardStrings.Keys.KindNavBento,
            DisplayName = "Highlights",
            Help = "A few big tiles the author picks -- the way a museum puts its three must-sees on the front door. Each tile leads to one " +
                   "block of this card, with its own words and picture if given (else that block's words), and a tap either moves the card to " +
                   "the block or opens it in the full view. A tile whose block cannot be reached (deleted, the title block, a look pinned under " +
                   "the title or to the bottom, a block that waits for Show After Reading) is not drawn, and its row says which. Unlike the " +
                   "other navigation blocks the tiles are authored, so Auto Nav Block does not offer it.",
            Variants = new[] { NavBentoTiles },
            DefaultVariant = NavBentoTiles,
            DisplayModes = new[] { CardOptions.DisplayInline, CardOptions.DisplayExpandable },
            OffersReadingTime = false,
            Fields = new[]
            {
                new BlockFieldDefinition
                {
                    Key = NavBentoTilesField, Type = BlockFieldType.Items, Label = "Tiles", Required = true,
                    Help = "One row per tile, in the order the card shows them. The first tile leads the grid and spans its whole width; the others " +
                           "sit two to a row.",
                    ItemFields = new[]
                    {
                        new BlockFieldDefinition
                        {
                            Key = NavBentoTargetField, Type = BlockFieldType.BlockRef, Label = "Block", Required = true,
                            Help = "The block of this card the tile leads to.",
                        },
                        new BlockFieldDefinition
                        {
                            Key = NavBentoWordsField, Type = BlockFieldType.LocalizedText, Label = "Words",
                            Help = "What the tile says. Empty: the block's own heading (else its kind's name).",
                        },
                        new BlockFieldDefinition
                        {
                            Key = NavBentoPictureField, Type = BlockFieldType.Asset, Media = MediaKind.Image, Label = "Picture",
                            Help = "Optional. A PNG or JPG inside the wall's Media Folder (Detail Card > Card Container), shown on the tile.",
                        },
                        new BlockFieldDefinition
                        {
                            Key = NavBentoOpensField, Type = BlockFieldType.Choice, Label = "On Tap",
                            Options = new[] { NavBentoOpensJump, NavBentoOpensFullView },
                            OptionLabels = new[] { "Move The Card To It", "Open In Full View" },
                            ChoiceDefault = NavBentoOpensJump,
                            Help = "Move The Card To It: the card scrolls to the block, the way a tab of its navigation does (Detail Card > Navigation > " +
                                   "Jump Opens Card To and Jump Motion apply). Open In Full View: the block opens over the card, full screen, with a " +
                                   "way back to the card.",
                        },
                    },
                },
            },
        };
    }
}
