namespace TileStories
{
    // The media family: the point's pictures -- a gallery, a before / after pair, one picture to zoom into, a picture with
    // spots to tap.
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

        public const string HotspotImageKind = "hotspot_image";
        public const string HotspotNumbered = "numbered";
        public const string HotspotLoupes = "loupes";
        public const string HotspotImageField = "image";
        public const string HotspotItemsField = "spots";
        public const string HotspotXField = "x";
        public const string HotspotYField = "y";
        public const string HotspotTitleField = "title";
        public const string HotspotTextField = "text";

        public const string VideoKind = "video";
        public const string VideoInline = "inline";
        public const string VideoChapters = "chapters";
        public const string VideoClipField = "clip";
        public const string VideoPosterField = "poster";
        public const string VideoCaptionsField = "captions";
        public const string VideoTitleField = "title";
        public const string VideoCaptionsOnField = "captions_on";
        public const string VideoChaptersField = "chapters";
        public const string VideoChapterTimeField = "t";
        public const string VideoChapterLabelField = "label";

        public const string Model3DKind = "model_3d";
        public const string Model3DTurntable = "turntable";
        public const string Model3DModelField = "model";
        public const string Model3DFallbackField = "fallback";
        public const string Model3DAutoSpinField = "auto_spin";

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

        // Tier 2: one picture with spots on it; a tap on a spot opens its text under the picture
        public static readonly BlockKindDefinition HotspotImage = new()
        {
            Key = HotspotImageKind,
            Family = "media",
            DisplayName = "Hotspot Image",
            Help = "One picture with spots on it to explore: a tap on a spot opens its title and text under the picture (a " +
                   "second tap closes it). Numbered: numbered circles on the picture itself. Loupes: small rings on the picture " +
                   "and, under it, a row of round close-ups of each spot to tap. A spot with no title is not shown.",
            Variants = new[] { HotspotNumbered, HotspotLoupes },
            DefaultVariant = HotspotNumbered,
            DisplayModes = new[] { CardOptions.DisplayInline },
            Fields = new[]
            {
                new BlockFieldDefinition { Key = HotspotImageField, Type = BlockFieldType.Asset, Media = MediaKind.Image, Label = "Picture", Required = true, Help = PictureHelp },
                new BlockFieldDefinition
                {
                    Key = HotspotItemsField, Type = BlockFieldType.Items, Label = "Spots", Required = true,
                    Help = "One row per spot, numbered in this order. Place each with Across and Down (a click-to-place picker on " +
                           "the picture is planned).",
                    ItemFields = new[]
                    {
                        new BlockFieldDefinition
                        {
                            Key = HotspotXField, Type = BlockFieldType.Number, Label = "Across", NumberMin = 0f, NumberMax = 1f, NumberDefault = 0.5f,
                            Help = "Where the spot is across the picture: 0 = its left edge, 1 = its right edge.",
                        },
                        new BlockFieldDefinition
                        {
                            Key = HotspotYField, Type = BlockFieldType.Number, Label = "Down", NumberMin = 0f, NumberMax = 1f, NumberDefault = 0.5f,
                            Help = "Where the spot is down the picture: 0 = its top edge, 1 = its bottom edge.",
                        },
                        new BlockFieldDefinition { Key = HotspotTitleField, Type = BlockFieldType.LocalizedText, Label = "Title", Required = true, Help = "The spot's name (The coat of arms)." },
                        new BlockFieldDefinition { Key = HotspotTextField, Type = BlockFieldType.LocalizedLongText, Label = "Text", Help = "What the visitor reads when they tap the spot. May stay empty. " + LongTextHelp },
                    },
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

        // Tier 4: a film of the point, played by the card's ONE video owner (step 9B); the first kind with a teaser display
        public static readonly BlockKindDefinition Video = new()
        {
            Key = VideoKind,
            Family = "media",
            DisplayName = "Video",
            Help = "A film the visitor plays on the card. Inline: the poster with a play button, then a bar to drag to any point, the " +
                   "time, a captions switch and a full-screen button. Chapters: the same, plus a button per chapter that jumps to it " +
                   "(the one playing is lit). Display Takeover shows only the poster with a play button on the card; a tap opens the " +
                   "video full screen. Full screen, the same playback goes on, and Back returns to the card where it was. Starting a " +
                   "video pauses the card's audio guide, and starting the audio pauses the video. Closing the card stops the video.",
            Variants = new[] { VideoInline, VideoChapters },
            DefaultVariant = VideoInline,
            DisplayModes = new[] { CardOptions.DisplayInline, CardOptions.DisplayTakeover },
            Fields = new[]
            {
                new BlockFieldDefinition
                {
                    Key = VideoClipField, Type = BlockFieldType.Asset, Media = MediaKind.Video, Label = "Clip", Required = true,
                    Help = "An MP4 (H.264) or WEBM file inside the wall's Media Folder (Detail Card > Card Container). Tick Transcode in " +
                           "its import settings with a Low bitrate to keep the app small.",
                },
                new BlockFieldDefinition
                {
                    Key = VideoPosterField, Type = BlockFieldType.Asset, Media = MediaKind.Image, Label = "Poster",
                    Help = "The still picture shown before the video plays and until its first frame is ready. " + PictureHelp +
                           " Empty: a plain frame with the play button.",
                },
                new BlockFieldDefinition
                {
                    Key = VideoCaptionsField, Type = BlockFieldType.Asset, Media = MediaKind.Captions, Label = "Captions",
                    Help = "A WebVTT (.vtt) captions file inside the Media Folder, in the language the video is spoken in. Empty: no " +
                           "captions switch.",
                },
                new BlockFieldDefinition
                {
                    Key = VideoTitleField, Type = BlockFieldType.LocalizedText, Label = "Title",
                    Help = "The video's name, shown full screen after the card's title. Empty: the point's card title.",
                },
                new BlockFieldDefinition
                {
                    Key = VideoCaptionsOnField, Type = BlockFieldType.Toggle, Label = "Captions On By Default",
                    Help = "Show the caption line as soon as the video is shown (the visitor can still switch it off). Needs a Captions file.",
                },
                new BlockFieldDefinition
                {
                    Key = VideoChaptersField, Type = BlockFieldType.Items, Label = "Chapters",
                    Help = "The Chapters look only: one row per chapter, a button each. A row needs a Start time and a Label; the " +
                           "buttons run in time order, and a time past the end of the clip is not shown.",
                    ItemFields = new[]
                    {
                        new BlockFieldDefinition
                        {
                            Key = VideoChapterTimeField, Type = BlockFieldType.Time, Label = "Start", Required = true,
                            Help = "Where the chapter starts in the clip: minutes:seconds (1:30), hours:minutes:seconds (1:02:03) or seconds (90).",
                        },
                        new BlockFieldDefinition
                        {
                            Key = VideoChapterLabelField, Type = BlockFieldType.LocalizedText, Label = "Label", Required = true,
                            Help = "The chapter's short name on its button, one per language.",
                        },
                    },
                },
            },
        };

        // Tier 5: a 3D model the visitor turns (_3.1 step 10A.2b.3). Turntable: the model on the card, a drag rotates it,
        // a pinch zooms (within limits), and it auto-spins on its own after a pause -- a touch stops the spin and it
        // resumes after the same pause. A fallback picture shows while the model loads and if it fails to load.
        public static readonly BlockKindDefinition Model3D = new()
        {
            Key = Model3DKind,
            Family = "media",
            DisplayName = "Model 3D",
            Help = "A 3D model the visitor turns to look at from every side: a drag rotates it, two fingers pinch to zoom " +
                   "(within limits), and it slowly spins on its own until touched (Auto Spin). The Fallback Picture shows " +
                   "while the model loads and if it cannot be loaded.",
            Variants = new[] { Model3DTurntable },
            DefaultVariant = Model3DTurntable,
            DisplayModes = new[] { CardOptions.DisplayInline },
            Fields = new[]
            {
                new BlockFieldDefinition
                {
                    Key = Model3DModelField, Type = BlockFieldType.Asset, Media = MediaKind.Model, Label = "Model", Required = true,
                    Help = "A .glb or .gltf file inside the wall's Media Folder (Detail Card > Card Container), or a Framework/wall default.",
                },
                new BlockFieldDefinition
                {
                    Key = Model3DFallbackField, Type = BlockFieldType.Asset, Media = MediaKind.Image, Label = "Fallback Picture",
                    Help = "Shown while the model loads and if it cannot be loaded. " + PictureHelp,
                },
                new BlockFieldDefinition
                {
                    Key = Model3DAutoSpinField, Type = BlockFieldType.Toggle, Label = "Auto Spin",
                    Help = "The model slowly turns on its own until the visitor touches it, then resumes a couple of seconds after they let go.",
                },
            },
        };
    }
}
