namespace TileStories
{
    // The ar family: what ties the card to the wall in front of the visitor (_3.1 Tier 3, step 8B). Part of BuiltInBlocks
    // (BuiltInBlocks.cs registers every kind, in catalog order): each kind's key, variant and field constants first, then the
    // definitions. place_in_ar joins with Tier 5.
    public static partial class BuiltInBlocks
    {
        public const string ShowOnWallKind = "show_on_wall";
        public const string ShowOnWallButton = "button";
        public const string ShowOnWallWithNeighbours = "with_neighbours";

        // How many neighbours the with_neighbours look names
        public const int ShowOnWallNeighbourCount = 3;

        // Tier 3: one button that lowers the card so the point's marker shows on the wall
        public static readonly BlockKindDefinition ShowOnWall = new()
        {
            Key = ShowOnWallKind,
            Family = "ar",
            DisplayName = "Show On Wall",
            Help = "A button that lowers the card to its peek so the visitor can see this point's marker on the wall (the point stays " +
                   "selected: its marker stays lit and the others dim, as after a tap on it). Nothing to write. Button: just the button. " +
                   "With Neighbours: the button and, under it, the nearest points on the wall by name; a tap on one opens its card. " +
                   "The button's words and the Also nearby caption are Detail Card > Card Texts. The With Neighbours look shows only " +
                   "while at least one other point is on the wall.",
            Variants = new[] { ShowOnWallButton, ShowOnWallWithNeighbours },
            DefaultVariant = ShowOnWallButton,
            DisplayModes = new[] { CardOptions.DisplayInline },
            ShowsFor = (poi, _, variant, wallPois) => variant != ShowOnWallWithNeighbours || RelatedPoisRule.Pick(poi, RelatedPoisRule.SourceNearest, null, wallPois).Count > 0,
            NotShownForPoiNote = "the With Neighbours look needs at least one other point on this wall.",
        };
    }
}
