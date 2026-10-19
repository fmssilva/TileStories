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
    }
}
