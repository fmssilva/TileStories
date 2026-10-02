namespace TileStories.LivingRoom
{
    // The living room's own block kind (_3.1 step 11): the point's real size next to a familiar object, both drawn to one scale.
    // It is registered by LivingRoomBlocks through the public BlockRegistry, so the Framework never names it; the Editor's Block
    // Library, "+ Add block" and Card Content draw it from this definition like a built-in kind.
    public static class SizeComparisonBlock
    {
        public const string Kind = "size_comparison";
        public const string SideBySide = "side_by_side";
        public const string ObjectField = "object";
        public const string WidthField = "width_cm";
        public const string HeightField = "height_cm";
        public const string CaptionField = "caption";
        // What the point's drawing is called under it (empty: the card's title)
        public const string PoiLabelField = "poi_label";

        // The slider's top: no point on a wall is wider or taller than this (centimetres)
        public const float MaxSizeCm = 300f;

        public static readonly BlockKindDefinition Definition = new()
        {
            Key = Kind,
            Family = "about",
            DisplayName = "Size Comparison",
            Help = "How big the point really is: it is drawn next to a familiar object, both to the same scale, so the visitor can judge its " +
                   "size at a glance. Pick the Object, set the point's real Width and Height in centimetres, and write a short Caption that " +
                   "says the comparison in words. The block is not shown while the Width or the Height is 0.",
            Variants = new[] { SideBySide },
            DefaultVariant = SideBySide,
            DisplayModes = new[] { CardOptions.DisplayInline },
            Fields = new[]
            {
                new BlockFieldDefinition
                {
                    Key = ObjectField, Type = BlockFieldType.Choice, Label = "Object", Required = true,
                    Help = "The familiar object the point is compared with. Its real size comes from the app, so it is drawn true to scale.",
                    Options = FamiliarObjects.Keys, OptionLabels = FamiliarObjects.EditorLabels,
                },
                new BlockFieldDefinition
                {
                    Key = WidthField, Type = BlockFieldType.Number, Label = "Width (cm)",
                    Help = "The point's real width in centimetres. 0 means not written yet: the block is not shown.",
                    NumberMin = 0f, NumberMax = MaxSizeCm, NumberDefault = 0f,
                },
                new BlockFieldDefinition
                {
                    Key = HeightField, Type = BlockFieldType.Number, Label = "Height (cm)",
                    Help = "The point's real height in centimetres. 0 means not written yet: the block is not shown.",
                    NumberMin = 0f, NumberMax = MaxSizeCm, NumberDefault = 0f,
                },
                new BlockFieldDefinition
                {
                    Key = PoiLabelField, Type = BlockFieldType.LocalizedText, Label = "Point Label",
                    Help = "What the point's drawing is called, under it (for example \"The tile panel\"), so the visitor knows which thing is " +
                           "that size: a point whose card title names a whole building would otherwise look as small as the object. Write it in " +
                           "every language. Empty: the card's title.",
                },
                new BlockFieldDefinition
                {
                    Key = CaptionField, Type = BlockFieldType.LocalizedText, Label = "Caption", Required = true,
                    Help = "The comparison in words, under the drawing (for example how many of the object it takes). Write it in every language.",
                },
            },
            ShowsFor = (poi, block, variant, wallPois) =>
            {
                var read = new BlockFieldReader(block, null, null);
                return SizeComparisonRule.HasSize(read.Number(Definition.Field(WidthField)), read.Number(Definition.Field(HeightField)));
            },
            NotShownForPoiNote = "the Width and the Height must both be above 0 centimetres.",
        };
    }
}
