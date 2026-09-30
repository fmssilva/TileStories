using System.Collections.Generic;
using UnityEngine;

namespace TileStories
{
    public static partial class CardGalleryDefinitions
    {
        private static void AddGalleries(List<Entry> list)
        {
            var shortRows = new[] { ("one.png", "The gate from the square", ""), ("two.png", "The river front", "") };
            var longRows = new[]
            {
                ("one.png", "The gate from the square, with the customs house and the long arcade the fire destroyed in 1755", "Photo: Municipal archive, catalogue 12/447"),
                ("two.png", "The river front", "Photo: Municipal archive"),
                ("three.png", "The chapel tower (portrait)", ""),
                ("four.png", "The walls at dusk", "Photo: a visitor, shared under CC BY"),
            };
            // - outside the folder, not a picture, a picture file that is not there: the first two rows are left out, the
            //   third shows its "picture unavailable" frame
            var partialRows = new[] { ("one.png", "Kept", ""), ("Assets/Art/outside.png", "Outside the folder", ""), ("clip.mp3", "Not a picture", ""), ("ghost.png", "No such file", "") };
            foreach (var variant in BuiltInBlocks.Gallery.Variants)
            {
                list.Add(new Entry(BuiltInBlocks.GalleryKind, variant, "short", GalleryBlock(variant, shortRows)));
                list.Add(new Entry(BuiltInBlocks.GalleryKind, variant, "long", GalleryBlock(variant, longRows)));
                list.Add(new Entry(BuiltInBlocks.GalleryKind, variant, "partial", GalleryBlock(variant, partialRows)));
            }
        }

        private static BlockInstanceData GalleryBlock(string variant, (string Image, string Caption, string Credit)[] rows)
        {
            var block = new BlockInstanceData { key = "block_2", kind = BuiltInBlocks.GalleryKind, variant = variant };
            var field = new BlockFieldValue { key = BuiltInBlocks.GalleryItemsField };
            foreach (var (image, caption, credit) in rows)
                field.items.Add(Item(new BlockItemFieldValue { key = BuiltInBlocks.GalleryImageField, asset = image },
                    ItemText(BuiltInBlocks.GalleryCaptionField, caption), ItemText(BuiltInBlocks.GalleryCreditField, credit)));
            block.fields.Add(field);
            return block;
        }


        private static void AddBeforeAfter(List<Entry> list)
        {
            foreach (var variant in BuiltInBlocks.BeforeAfter.Variants)
            {
                list.Add(new Entry(BuiltInBlocks.BeforeAfterKind, variant, "short", BeforeAfterBlock(variant, "after.png", null, null, null)));
                list.Add(new Entry(BuiltInBlocks.BeforeAfterKind, variant, "labels", BeforeAfterBlock(variant, "after.png", "1740, before the earthquake", "Today", 0.3f)));
                list.Add(new Entry(BuiltInBlocks.BeforeAfterKind, variant, "missing", BeforeAfterBlock(variant, "ghost.png", null, null, null)));
            }
        }

        private static BlockInstanceData BeforeAfterBlock(string variant, string after, string beforeLabel, string afterLabel, float? start)
        {
            var block = new BlockInstanceData { key = "block_2", kind = BuiltInBlocks.BeforeAfterKind, variant = variant };
            block.fields.Add(new BlockFieldValue { key = BuiltInBlocks.BeforeAfterBeforeField, asset = "before.png" });
            block.fields.Add(new BlockFieldValue { key = BuiltInBlocks.BeforeAfterAfterField, asset = after });
            if (beforeLabel != null) block.fields.Add(Text(BuiltInBlocks.BeforeAfterBeforeLabelField, beforeLabel));
            if (afterLabel != null) block.fields.Add(Text(BuiltInBlocks.BeforeAfterAfterLabelField, afterLabel));
            if (start.HasValue) block.fields.Add(new BlockFieldValue { key = BuiltInBlocks.BeforeAfterStartField, number = start.Value });
            return block;
        }


        private static void AddZoomImages(List<Entry> list)
        {
            foreach (var variant in BuiltInBlocks.ZoomImage.Variants)
            {
                var withCaption = new BlockInstanceData { key = "block_2", kind = BuiltInBlocks.ZoomImageKind, variant = variant };
                withCaption.fields.Add(new BlockFieldValue { key = BuiltInBlocks.ZoomImageImageField, asset = "detail.png" });
                withCaption.fields.Add(Text(BuiltInBlocks.ZoomImageCaptionField, "Pinch into the panel to see each brush stroke of the painter's cobalt"));
                list.Add(new Entry(BuiltInBlocks.ZoomImageKind, variant, "short", withCaption));
                var wide = new BlockInstanceData { key = "block_2", kind = BuiltInBlocks.ZoomImageKind, variant = variant };
                wide.fields.Add(new BlockFieldValue { key = BuiltInBlocks.ZoomImageImageField, asset = "wide.png" });
                list.Add(new Entry(BuiltInBlocks.ZoomImageKind, variant, "nocaption", wide));
                // _3.1 step 13: a default:<key> picture (the Framework's own default library, no wall file) binds and renders
                // exactly like an authored one
                var usingDefault = new BlockInstanceData { key = "block_2", kind = BuiltInBlocks.ZoomImageKind, variant = variant };
                usingDefault.fields.Add(new BlockFieldValue { key = BuiltInBlocks.ZoomImageImageField, asset = MediaPathRule.PathForDefaultKey("azulejo_blue") });
                usingDefault.fields.Add(Text(BuiltInBlocks.ZoomImageCaptionField, "A Framework default picture, no wall file authored"));
                list.Add(new Entry(BuiltInBlocks.ZoomImageKind, variant, "default", usingDefault));
            }
        }


        // ---------------- hotspot_image (Tier 2 group B) ----------------

        // A picture name no file has: the "missing media" content of a picture block
        public const string MissingPicture = "gone.png";

        private static void AddHotspots(List<Entry> list)
        {
            var shortSpots = new[] { (0.3f, 0.4f, "The coat of arms", "Carved over the gate in 1640."), (0.7f, 0.6f, "The bell", "") };
            var longSpots = new[]
            {
                (0.2f, 0.15f, "The coat of arms of the kings of Portugal and of the Algarves, over the gate",
                    "Carved over the gate in 1640, the year the kingdom took back its crown. It hangs on the [[keep]], the last refuge." +
                    "\n\nThe painter drew every quartering of the shield, even the small castles of the border."),
                (0.8f, 0.2f, "The bell tower", "Rebuilt after the earthquake, taller than before."),
                (0.5f, 0.5f, "The chapel", "The oldest part of the building."),
                (0.25f, 0.85f, "The river gate", "Ships unloaded here until the quay was built."),
                (0.75f, 0.8f, "The customs house", "Burned in the fire of 1755."),
            };
            // - a row with no title (left out: the next one is still 2), a spot with no text, spots on the picture's corners
            var partialSpots = new[] { (0f, 0f, "Top left corner", "At the picture's very edge."), (0.5f, 0.5f, "", "No title: not shown"),
                (1f, 1f, "Bottom right corner", "") };
            foreach (var variant in BuiltInBlocks.HotspotImage.Variants)
            {
                list.Add(new Entry(BuiltInBlocks.HotspotImageKind, variant, "short", HotspotBlock(variant, "wide.png", shortSpots)));
                list.Add(new Entry(BuiltInBlocks.HotspotImageKind, variant, "long", HotspotBlock(variant, "three.png", longSpots)));
                list.Add(new Entry(BuiltInBlocks.HotspotImageKind, variant, "partial", HotspotBlock(variant, "one.png", partialSpots)));
                list.Add(new Entry(BuiltInBlocks.HotspotImageKind, variant, "missing", HotspotBlock(variant, MissingPicture, shortSpots)));
            }
        }

        private static BlockInstanceData HotspotBlock(string variant, string picture, (float X, float Y, string Title, string Text)[] spots)
        {
            var block = new BlockInstanceData { key = "block_2", kind = BuiltInBlocks.HotspotImageKind, variant = variant };
            block.fields.Add(new BlockFieldValue { key = BuiltInBlocks.HotspotImageField, asset = picture });
            var field = new BlockFieldValue { key = BuiltInBlocks.HotspotItemsField };
            foreach (var (x, y, title, text) in spots)
                field.items.Add(Item(new BlockItemFieldValue { key = BuiltInBlocks.HotspotXField, number = x },
                    new BlockItemFieldValue { key = BuiltInBlocks.HotspotYField, number = y },
                    ItemText(BuiltInBlocks.HotspotTitleField, title), ItemText(BuiltInBlocks.HotspotTextField, text)));
            block.fields.Add(field);
            return block;
        }


        // ---------------- model_3d, turntable (_3.1 step 10A.2b.3) ----------------

        private static void AddModel3D(List<Entry> list)
        {
            foreach (var variant in BuiltInBlocks.Model3D.Variants)
            {
                // - the Framework's own default model (_3.1 step 13): a "default:<key>" model resolves and renders
                //   exactly like an authored .glb, real drag and pinch move the turntable
                var withDefault = new BlockInstanceData { key = "block_2", kind = BuiltInBlocks.Model3DKind, variant = variant };
                withDefault.fields.Add(new BlockFieldValue { key = BuiltInBlocks.Model3DModelField, asset = MediaPathRule.PathForDefaultKey("azulejo_arch") });
                list.Add(new Entry(BuiltInBlocks.Model3DKind, variant, "default", withDefault));

                var autoSpin = new BlockInstanceData { key = "block_2", kind = BuiltInBlocks.Model3DKind, variant = variant };
                autoSpin.fields.Add(new BlockFieldValue { key = BuiltInBlocks.Model3DModelField, asset = MediaPathRule.PathForDefaultKey("azulejo_arch") });
                autoSpin.fields.Add(new BlockFieldValue { key = BuiltInBlocks.Model3DAutoSpinField, flag = true });
                list.Add(new Entry(BuiltInBlocks.Model3DKind, variant, "autospin", autoSpin));

                // - a model file that is not there: the authored Fallback Picture shows instead
                var missingWithFallback = new BlockInstanceData { key = "block_2", kind = BuiltInBlocks.Model3DKind, variant = variant };
                missingWithFallback.fields.Add(new BlockFieldValue { key = BuiltInBlocks.Model3DModelField, asset = "ghost.glb" });
                missingWithFallback.fields.Add(new BlockFieldValue { key = BuiltInBlocks.Model3DFallbackField, asset = "wide.png" });
                list.Add(new Entry(BuiltInBlocks.Model3DKind, variant, "missing-with-fallback", missingWithFallback));

                // - a model file that is not there, and no Fallback Picture authored: CardImage's own "unavailable" words
                var missingNoFallback = new BlockInstanceData { key = "block_2", kind = BuiltInBlocks.Model3DKind, variant = variant };
                missingNoFallback.fields.Add(new BlockFieldValue { key = BuiltInBlocks.Model3DModelField, asset = "ghost.glb" });
                list.Add(new Entry(BuiltInBlocks.Model3DKind, variant, "missing-no-fallback", missingNoFallback));
            }
        }
    }
}
