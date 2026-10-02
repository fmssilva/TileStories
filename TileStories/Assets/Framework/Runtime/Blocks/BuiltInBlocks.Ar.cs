namespace TileStories
{
    // The ar family: what ties the card to the wall in front of the visitor (_3.1 Tier 3, step 8B). Part of BuiltInBlocks
    // (BuiltInBlocks.cs registers every kind, in catalog order): each kind's key, variant and field constants first, then the
    // definitions. place_in_ar (Tier 5, step 10B) places a 3D model at the point.
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

        public const string PlaceInArKind = "place_in_ar";
        public const string PlaceInArButton = "button";
        public const string PlaceInArModelField = "model";
        public const string PlaceInArAnchorField = "anchor";
        public const string PlaceInArOffsetField = "offset_from_wall_cm";
        public const string PlaceInArScaleField = "scale";
        public const string PlaceInArHeightField = "height_cm";
        public const string PlaceInArMultipleField = "marker_multiple";
        public const string PlaceInArLabelField = "button_label";

        // Tier 5 (_3.1 step 10B): one button that places the block's 3D model in the world at this point of the wall (ArPlacementService,
        // ONE model at a time) and lowers the card to its peek so the visitor sees it; while it stands, a Remove chip on the card takes it away
        public static readonly BlockKindDefinition PlaceInAr = new()
        {
            Key = PlaceInArKind,
            Family = "ar",
            DisplayName = "Place In AR",
            Help = "A button that places a 3D model in the room, at this point of the wall, and lowers the card to its peek so the visitor " +
                   "sees it standing there. One model stands at a time: placing another (from any card) replaces it, and the card's Remove " +
                   "chip or closing the card takes it away. While the wall is not found yet (the camera has not recognised it), the button " +
                   "is disabled and a short line says why. The button's words are Detail Card > Card Texts, unless Button Label is set.",
            Variants = new[] { PlaceInArButton },
            DefaultVariant = PlaceInArButton,
            DisplayModes = new[] { CardOptions.DisplayInline },
            Fields = new[]
            {
                new BlockFieldDefinition
                {
                    Key = PlaceInArModelField, Type = BlockFieldType.Asset, Media = MediaKind.Model, Label = "Model", Required = true,
                    Help = "A .glb or .gltf file inside the wall's Media Folder (Detail Card > Card Container), or a Framework/wall default. " +
                           "It stands showing the visitor the side the 3D Model block shows first.",
                },
                new BlockFieldDefinition
                {
                    Key = PlaceInArAnchorField, Type = BlockFieldType.Choice, Label = "Anchor",
                    Options = new[] { ArPlacementRule.AnchorPoiOnWall },
                    OptionLabels = new[] { "Point On Wall" },
                    ChoiceDefault = ArPlacementRule.AnchorPoiOnWall,
                    Help = "Where the model stands. Point On Wall: at this point's position on the wall, pushed out towards the visitor by " +
                           "Offset From Wall and facing them. Surface (a floor or table the phone's camera finds) is not available yet.",
                },
                new BlockFieldDefinition
                {
                    Key = PlaceInArOffsetField, Type = BlockFieldType.Number, Label = "Offset From Wall (cm)",
                    NumberMin = 0f, NumberMax = 200f, NumberDefault = 10f,
                    Help = "How far in front of the wall the model's centre stands, in centimetres (Point On Wall). 0 puts its centre on the wall.",
                },
                new BlockFieldDefinition
                {
                    Key = PlaceInArScaleField, Type = BlockFieldType.Choice, Label = "Scale Mode",
                    Options = new[] { ArPlacementRule.ScaleRealSize, ArPlacementRule.ScaleHeightCm, ArPlacementRule.ScaleMarkerMultiple },
                    OptionLabels = new[] { "Real Size", "Height", "Marker Multiple" },
                    ChoiceDefault = ArPlacementRule.ScaleRealSize,
                    LibraryDefault = true,
                    Help = "How big the model stands. Real Size: the model file's own size (a model made in metres stands life size). " +
                           "Height: scaled so it is Height (cm) tall. Marker Multiple: scaled so it is Marker Multiple times this point's " +
                           "marker tall, so it keeps its size next to the markers whatever the hierarchy level.",
                },
                new BlockFieldDefinition
                {
                    Key = PlaceInArHeightField, Type = BlockFieldType.Number, Label = "Height (cm)",
                    NumberMin = 1f, NumberMax = 500f, NumberDefault = 30f,
                    ShownWhen = FieldShownWhen.Choice(PlaceInArScaleField, ArPlacementRule.ScaleHeightCm),
                    Help = "Scale Mode Height only: how tall the model stands, in centimetres.",
                },
                new BlockFieldDefinition
                {
                    Key = PlaceInArMultipleField, Type = BlockFieldType.Number, Label = "Marker Multiple",
                    NumberMin = 0.5f, NumberMax = 20f, NumberDefault = 3f,
                    ShownWhen = FieldShownWhen.Choice(PlaceInArScaleField, ArPlacementRule.ScaleMarkerMultiple),
                    Help = "Scale Mode Marker Multiple only: how many times this point's marker tall the model stands.",
                },
                new BlockFieldDefinition
                {
                    Key = PlaceInArLabelField, Type = BlockFieldType.LocalizedText, Label = "Button Label",
                    Help = "The button's words for this block. Empty: the wall's wording in Detail Card > Card Texts.",
                },
            },
        };
    }
}
