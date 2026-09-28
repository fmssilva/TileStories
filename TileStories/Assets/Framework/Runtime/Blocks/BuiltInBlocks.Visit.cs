namespace TileStories
{
    // The visit family: what a visitor needs to plan a visit to the real place, and where the point is on its wall.
    // Part of BuiltInBlocks (BuiltInBlocks.cs registers every kind, in catalog order): each kind's key, variant and
    // field constants first, then the definitions.
    public static partial class BuiltInBlocks
    {
        public const string PracticalInfoKind = "practical_info";
        public const string PracticalInfoRows = "rows";
        public const string PracticalInfoItemsField = "rows";
        public const string PracticalInfoIconField = "icon";
        public const string PracticalInfoLabelField = "label";
        public const string PracticalInfoValueField = "value";
        // The icons a row may pick: the visit-information keys of the card's one icon set (CardIcons)
        public static readonly string[] PracticalInfoIcons = { CardIcons.Time, CardIcons.Access, CardIcons.Location, CardIcons.Info, CardIcons.Ticket, CardIcons.Light };

        public const string WallLocatorKind = "wall_locator";
        public const string WallLocatorStrip = "strip";
        public const string WallLocatorNeighbours = "neighbours";

        public const string TodayMapKind = "today_map";
        public const string TodayMapStatic = "static";
        public const string TodayMapBridge = "bridge";
        public const string TodayMapImageField = "map";
        public const string TodayMapLatField = "lat";
        public const string TodayMapLngField = "lng";
        public const string TodayMapUrlField = "maps_url";

        public const string RelatedKind = "related";
        public const string RelatedCarousel = "carousel";
        public const string RelatedNextAlongWall = "next_along_wall";
        public const string RelatedSourceField = "source";
        public const string RelatedItemsField = "items";
        public const string RelatedPoiField = "poi";

        // Tier 2: where this point is along its wall, from the positions of the wall's points (nothing to write)
        public static readonly BlockKindDefinition WallLocator = new()
        {
            Key = WallLocatorKind,
            Family = "visit",
            DisplayName = "Wall Locator",
            Help = "Where this point is along the wall, worked out from the positions of the wall's points: nothing to write. Strip: " +
                   "the wall as a line with a dot for every point, this one marked, and in the app the visitor's own place (You are " +
                   "here). Neighbours: the nearest point on each side along the wall; a tap on one opens its card, as a tap on its " +
                   "marker does. With Heading empty the card titles the block with its Card Texts wording (On this wall). Shown " +
                   "only while this point and at least one other point are on this wall.",
            Variants = new[] { WallLocatorStrip, WallLocatorNeighbours },
            DefaultVariant = WallLocatorStrip,
            DisplayModes = new[] { CardOptions.DisplayInline },
            DefaultHeadingKey = CardStrings.Keys.WallLocatorHeading,
            ShowsFor = (poi, block, _, wallPois) => poi != null && WallAxisRule.Places(wallPois) is var places
                                                 && places.IndexOf(poi.id) >= 0 && places.Pois.Count >= 2,
            NotShownForPoiNote = "this point is the only point on this wall (or its Position holds an invalid number).",
        };

        // Tier 2: where the place on the wall is in the city today: a map picture, its coordinates, directions
        public static readonly BlockKindDefinition TodayMap = new()
        {
            Key = TodayMapKind,
            Family = "visit",
            DisplayName = "Today Map",
            Help = "Where the place the wall shows is in the city today. Static: the map picture, the coordinates under it and a " +
                   "Directions button. Bridge: from the wall to today -- the point as the wall shows it (its header picture and " +
                   "title) beside a small map of where it is now, then the coordinates and Directions. Directions opens Maps Link " +
                   "in the phone's browser or maps app; with no link (or one that is not a web address) there is no button. The " +
                   "coordinates show only when both Latitude and Longitude are set. With Heading empty the card titles the block " +
                   "with its Card Texts wording (Where it is today). The button's words are Detail Card > Card Texts.",
            Variants = new[] { TodayMapStatic, TodayMapBridge },
            DefaultVariant = TodayMapStatic,
            DisplayModes = new[] { CardOptions.DisplayInline },
            DefaultHeadingKey = CardStrings.Keys.TodayMapHeading,
            Fields = new[]
            {
                new BlockFieldDefinition { Key = TodayMapImageField, Type = BlockFieldType.Asset, Media = MediaKind.Image, Label = "Map", Required = true,
                    Help = "A picture of the map around the place (a screenshot or a drawn plan). " + PictureHelp },
                new BlockFieldDefinition
                {
                    Key = TodayMapLatField, Type = BlockFieldType.Number, Label = "Latitude", NumberMin = -90f, NumberMax = 90f, NumberDefault = 0f,
                    Help = "The place's latitude in decimal degrees (north positive, e.g. 38.71390). Shown with Longitude.",
                },
                new BlockFieldDefinition
                {
                    Key = TodayMapLngField, Type = BlockFieldType.Number, Label = "Longitude", NumberMin = -180f, NumberMax = 180f, NumberDefault = 0f,
                    Help = "The place's longitude in decimal degrees (east positive, e.g. -9.13340). Shown with Latitude.",
                },
                new BlockFieldDefinition
                {
                    Key = TodayMapUrlField, Type = BlockFieldType.Url, Label = "Maps Link",
                    Help = "The web address Directions opens (a maps link to the place), starting with https://. May stay empty: no button.",
                },
            },
        };

        // Tier 2: other points worth seeing from here, picked by hand or by rule. Carousel: a horizontal strip of the
        // points, each its header picture (when it has one) and title; a tap selects it (IBlockHost.SelectPoi -- the card
        // rebinds, zoom-on-select as a marker tap). Next Along Wall: one button, the nearest of the picked points to the
        // right along the wall (WallAxisRule, wall_locator's own rule); at the wall's right end it wraps to the nearest on
        // the left. Manual names points by hand; Same Category / Nearest fill the strip from the wall automatically (up to
        // RelatedPoisRule.MaxAutomatic, nearest first).
        public static readonly BlockKindDefinition Related = new()
        {
            Key = RelatedKind,
            Family = "visit",
            DisplayName = "Related",
            Help = "Other points worth seeing from here. Carousel: a strip of pictures, a tap opens that point's card. Next Along " +
                   "Wall: one button, the nearest picked point to the right along the wall (wraps to the left at the wall's end). " +
                   "Manual: the Points list, in the order written. Same Category / Nearest: filled from the wall's other points " +
                   "automatically (nearest first) -- Points is then unused. With Heading empty the card titles the block with its " +
                   "Card Texts wording (Related). Shown only while at least one point can be picked.",
            Variants = new[] { RelatedCarousel, RelatedNextAlongWall },
            DefaultVariant = RelatedCarousel,
            DisplayModes = new[] { CardOptions.DisplayInline },
            DefaultHeadingKey = CardStrings.Keys.RelatedHeading,
            ShowsFor = (poi, block, _, wallPois) => poi != null && RelatedPoisRule.Of(poi, block, wallPois).Count > 0,
            NotShownForPoiNote = "no other point could be picked for it (Manual names none still on this wall; Same Category / " +
                                 "Nearest need at least one other point on this wall).",
            Fields = new[]
            {
                new BlockFieldDefinition
                {
                    Key = RelatedSourceField, Type = BlockFieldType.Choice, Label = "Source",
                    Options = new[] { RelatedPoisRule.SourceManual, RelatedPoisRule.SourceSameCategory, RelatedPoisRule.SourceNearest },
                    OptionLabels = new[] { "Manual", "Same Category", "Nearest" },
                    Help = "Where the picked points come from. Manual: the Points list below. Same Category: this point's other " +
                           "points of the same category on this wall. Nearest: this point's other points on this wall, closest " +
                           "first. (none) is read as Manual.",
                },
                new BlockFieldDefinition
                {
                    Key = RelatedItemsField, Type = BlockFieldType.Items, Label = "Points",
                    Help = "Manual only: the points to show, in this order. A point no longer on this wall, this point itself, or " +
                           "one named twice is left out.",
                    ItemFields = new[]
                    {
                        new BlockFieldDefinition
                        {
                            Key = RelatedPoiField, Type = BlockFieldType.PoiRef, Label = "Point",
                            Help = "The point this row names, as the POI list names it. One no longer on this wall shows as (missing) here and is left out.",
                        },
                    },
                },
            },
        };

        // Tier 1: what a visitor needs to plan the visit
        public static readonly BlockKindDefinition PracticalInfo = new()
        {
            Key = PracticalInfoKind,
            Family = "visit",
            DisplayName = "Practical Info",
            Help = "What a visitor needs to plan a visit to the real place: opening hours, tickets, how to get there, the best " +
                   "light. Rows: one row per item, a small icon beside its label and value, split by thin lines. A row with no " +
                   "label is not shown.",
            Variants = new[] { PracticalInfoRows },
            DefaultVariant = PracticalInfoRows,
            DisplayModes = new[] { CardOptions.DisplayInline },
            Fields = new[]
            {
                new BlockFieldDefinition
                {
                    Key = PracticalInfoItemsField, Type = BlockFieldType.Items, Label = "Rows", Required = true,
                    Help = "One row per item, in the order the card shows them.",
                    ItemFields = new[]
                    {
                        new BlockFieldDefinition
                        {
                            Key = PracticalInfoIconField, Type = BlockFieldType.Choice, Label = "Icon",
                            Options = PracticalInfoIcons, OptionLabels = new[] { "Time", "Access", "Location", "Info", "Ticket", "Light" },
                            Help = "The small picture beside the row: Time (a clock), Access (a doorway), Location (a pin), Info (an i), " +
                                   "Ticket, Light (a bright point). (none): no picture, the words stay in line with the others.",
                        },
                        new BlockFieldDefinition { Key = PracticalInfoLabelField, Type = BlockFieldType.LocalizedText, Label = "Label", Required = true, Help = "What the row is about (Open, Tickets, Getting there)." },
                        new BlockFieldDefinition { Key = PracticalInfoValueField, Type = BlockFieldType.LocalizedText, Label = "Value", Help = "The information itself (Every day, 9:00 to 21:00). May stay empty." },
                    },
                },
            },
        };
    }
}
