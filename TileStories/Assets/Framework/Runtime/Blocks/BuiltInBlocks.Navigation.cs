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
            DisplayModes = new[] { CardOptions.DisplayInline },
        };
    }
}
