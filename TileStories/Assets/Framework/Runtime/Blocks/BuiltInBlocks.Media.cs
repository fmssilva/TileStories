namespace TileStories
{
    // The media family: the point's pictures -- a gallery, a before / after pair, one picture to zoom into.
    // Part of BuiltInBlocks (BuiltInBlocks.cs registers every kind, in catalog order): each kind's key, variant and
    // field constants first, then the definitions.
    public static partial class BuiltInBlocks
    {
        public const string GalleryKind = "gallery";
        public const string GalleryCarousel = "carousel";
        public const string GalleryGrid = "grid";
        public const string GalleryFilmstrip = "filmstrip";
        public const string GalleryStack = "stack";
        public const string GalleryItemsField = "pictures";
        public const string GalleryImageField = "image";
        public const string GalleryCaptionField = "caption";
        public const string GalleryCreditField = "credit";

        public const string BeforeAfterKind = "before_after";
        public const string BeforeAfterSlider = "slider";
        public const string BeforeAfterBeforeField = "before";
        public const string BeforeAfterAfterField = "after";
        public const string BeforeAfterBeforeLabelField = "before_label";
        public const string BeforeAfterAfterLabelField = "after_label";
        public const string BeforeAfterStartField = "start";

        public const string ZoomImageKind = "zoom_image";
        public const string ZoomImagePinch = "pinch";
        public const string ZoomImageImageField = "image";
        public const string ZoomImageCaptionField = "caption";

        private const string PictureHelp = "A PNG or JPG inside the wall's Media Folder (Detail Card > Card Container).";

        // Tier 2: several pictures; a tap on one opens it full screen (the lightbox)
        public static readonly BlockKindDefinition Gallery = new()
        {
            Key = GalleryKind,
            Family = "media",
            DisplayName = "Gallery",
            Help = "Several pictures of the point. Carousel: one picture at a time, swiped sideways, with dots. Grid: small " +
                   "pictures two per row. Filmstrip: one large picture, the others as a strip of small ones to pick from. Stack: " +
                   "the pictures fanned like prints, with how many there are. In every look a tap on a picture opens it full " +
                   "screen, with its caption and credit and a way back to the card. A row with no picture (or one outside the " +
                   "Media Folder) is not shown.",
            Variants = new[] { GalleryCarousel, GalleryGrid, GalleryFilmstrip, GalleryStack },
            DefaultVariant = GalleryCarousel,
            DisplayModes = new[] { CardOptions.DisplayInline },
            Fields = new[]
            {
                new BlockFieldDefinition
                {
                    Key = GalleryItemsField, Type = BlockFieldType.Items, Label = "Pictures", Required = true,
                    Help = "One row per picture, in the order the card shows them.",
                    ItemFields = new[]
                    {
                        new BlockFieldDefinition { Key = GalleryImageField, Type = BlockFieldType.Asset, Media = MediaKind.Image, Label = "Picture", Required = true, Help = PictureHelp },
                        new BlockFieldDefinition { Key = GalleryCaptionField, Type = BlockFieldType.LocalizedText, Label = "Caption", Help = "One line about the picture, under it. May stay empty." },
                        new BlockFieldDefinition { Key = GalleryCreditField, Type = BlockFieldType.LocalizedText, Label = "Credit", Help = "Who made or owns the picture, shown small in the full-screen view. May stay empty." },
                    },
                },
            },
        };

        // Tier 2: the same view before and after, a handle dragged across to compare
        public static readonly BlockKindDefinition BeforeAfter = new()
        {
            Key = BeforeAfterKind,
            Family = "media",
            DisplayName = "Before and After",
            Help = "Two pictures of the same view laid on each other: the visitor drags the handle sideways to show more of one " +
                   "or the other (Before on the left, After on the right). Use two pictures of the same size and framing.",
            Variants = new[] { BeforeAfterSlider },
            DefaultVariant = BeforeAfterSlider,
            DisplayModes = new[] { CardOptions.DisplayInline },
            Fields = new[]
            {
                new BlockFieldDefinition { Key = BeforeAfterBeforeField, Type = BlockFieldType.Asset, Media = MediaKind.Image, Label = "Before", Required = true, Help = "The earlier picture, on the left of the handle. " + PictureHelp },
                new BlockFieldDefinition { Key = BeforeAfterAfterField, Type = BlockFieldType.Asset, Media = MediaKind.Image, Label = "After", Required = true, Help = "The later picture, on the right of the handle. " + PictureHelp },
                new BlockFieldDefinition { Key = BeforeAfterBeforeLabelField, Type = BlockFieldType.LocalizedText, Label = "Before Label", Help = "The word on the left picture (1740). Empty: the card's own word (Detail Card > Card Texts)." },
                new BlockFieldDefinition { Key = BeforeAfterAfterLabelField, Type = BlockFieldType.LocalizedText, Label = "After Label", Help = "The word on the right picture (Today). Empty: the card's own word (Detail Card > Card Texts)." },
                new BlockFieldDefinition
                {
                    Key = BeforeAfterStartField, Type = BlockFieldType.Number, Label = "Start At", NumberMin = 0f, NumberMax = 1f, NumberDefault = 0.5f,
                    Help = "Where the handle starts: 0 = all After, 1 = all Before, 0.5 = half and half.",
                },
            },
        };

        // Tier 2: one large picture the visitor pinches to look closer
        public static readonly BlockKindDefinition ZoomImage = new()
        {
            Key = ZoomImageKind,
            Family = "media",
            DisplayName = "Zoom Image",
            Help = "One detailed picture the visitor looks into: two fingers pinch to enlarge it (up to four times), one finger " +
                   "moves the enlarged picture, pinching back returns to the whole picture. A large picture (1024 px or more) " +
                   "keeps its detail when enlarged.",
            Variants = new[] { ZoomImagePinch },
            DefaultVariant = ZoomImagePinch,
            DisplayModes = new[] { CardOptions.DisplayInline },
            Fields = new[]
            {
                new BlockFieldDefinition { Key = ZoomImageImageField, Type = BlockFieldType.Asset, Media = MediaKind.Image, Label = "Picture", Required = true, Help = PictureHelp },
                new BlockFieldDefinition { Key = ZoomImageCaptionField, Type = BlockFieldType.LocalizedText, Label = "Caption", Help = "One line under the picture. May stay empty." },
            },
        };
    }
}
