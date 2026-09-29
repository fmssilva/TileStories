using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace TileStories.Editor.Tests
{
    // The picture fields' rules (_3.1 step 7): MediaPathRule (what a stored path may be), the path the Editor stores for a
    // picked file, and how BlockStackBuilder / BlockFieldReader use the rule -- a required picture that is outside the
    // media folder or not a picture hides its block (InvalidMedia), a gallery row with such a picture is left out.
    public class MediaRulesTests
    {
        [Test]
        public void MediaPathRule_TakesOnlyARelativePictureInsideTheFolder()
        {
            Assert.AreEqual(MediaPathProblem.None, MediaPathRule.Check("castle_hero.png", MediaKind.Image));
            Assert.AreEqual(MediaPathProblem.None, MediaPathRule.Check(" castle/Hero.JPG ", MediaKind.Image), "a sub-folder, upper case, spaces around");
            Assert.AreEqual(MediaPathProblem.None, MediaPathRule.Check("a\\b.jpeg", MediaKind.Image), "a Windows separator");
            foreach (string empty in new[] { null, "", "   " })
                Assert.AreEqual(MediaPathProblem.Empty, MediaPathRule.Check(empty, MediaKind.Image), "'" + empty + "'");
            foreach (string outside in new[] { "Assets/Art/hero.png", "/hero.png", "C:/art/hero.png", "../hero.png", "a/../../hero.png", "Packages/x/hero.png", ".." })
                Assert.AreEqual(MediaPathProblem.OutsideFolder, MediaPathRule.Check(outside, MediaKind.Image), outside);
            foreach (string wrong in new[] { "clip.mp3", "notes.txt", "hero", "hero.png.meta", "model.glb" })
                Assert.AreEqual(MediaPathProblem.WrongType, MediaPathRule.Check(wrong, MediaKind.Image), wrong);
            Assert.AreEqual(MediaPathProblem.WrongType, MediaPathRule.Check("hero.png", MediaKind.None), "a field that names no media kind takes nothing");
        }

        // _3.1 step 13: a "default:<key>" value names a library entry, not a path -- valid for any kind here
        // (the library lookup, not this syntactic rule, decides whether the key actually holds that kind's media)
        [Test]
        public void MediaPathRule_TakesADefaultKey_AsValidForAnyKind_ButAnEmptyKeyIsStillEmpty()
        {
            Assert.IsTrue(MediaPathRule.IsDefaultKey("default:azulejo_blue"));
            Assert.IsTrue(MediaPathRule.IsDefaultKey(" default:azulejo_blue "), "trimmed first");
            Assert.IsFalse(MediaPathRule.IsDefaultKey("castle_hero.png"));
            Assert.IsFalse(MediaPathRule.IsDefaultKey(""));
            Assert.IsFalse(MediaPathRule.IsDefaultKey(null));

            Assert.AreEqual("azulejo_blue", MediaPathRule.DefaultKeyOf("default:azulejo_blue"));
            Assert.AreEqual("azulejo_blue", MediaPathRule.DefaultKeyOf(" default:azulejo_blue "));
            Assert.IsNull(MediaPathRule.DefaultKeyOf("castle_hero.png"), "not a default key at all");

            Assert.AreEqual("default:azulejo_blue", MediaPathRule.PathForDefaultKey("azulejo_blue"));

            foreach (var kind in new[] { MediaKind.Image, MediaKind.Audio, MediaKind.Captions, MediaKind.Video, MediaKind.Model, MediaKind.Panorama, MediaKind.None })
                Assert.AreEqual(MediaPathProblem.None, MediaPathRule.Check("default:azulejo_blue", kind), kind + ": a non-empty key is a valid value for any kind");
            Assert.AreEqual(MediaPathProblem.Empty, MediaPathRule.Check("default:", MediaKind.Image), "the prefix alone with no key is still nothing picked");
        }

        [Test]
        public void KindOfAssetType_MapsEachLoadedTypeBackToItsMediaKind()
        {
            Assert.AreEqual(MediaKind.Image, MediaPathRule.KindOfAssetType(typeof(UnityEngine.Texture2D)));
            Assert.AreEqual(MediaKind.Audio, MediaPathRule.KindOfAssetType(typeof(UnityEngine.AudioClip)));
            Assert.AreEqual(MediaKind.Captions, MediaPathRule.KindOfAssetType(typeof(UnityEngine.TextAsset)));
            Assert.AreEqual(MediaKind.Video, MediaPathRule.KindOfAssetType(typeof(UnityEngine.Video.VideoClip)));
            Assert.AreEqual(MediaKind.Model, MediaPathRule.KindOfAssetType(typeof(UnityEngine.GameObject)), "a model is the prefab glTFast imports (10A)");
            Assert.AreEqual(MediaKind.None, MediaPathRule.KindOfAssetType(typeof(UnityEngine.Material)));
            Assert.AreEqual(MediaKind.None, MediaPathRule.KindOfAssetType(null));
            CollectionAssert.AreEqual(new[] { MediaKind.Image, MediaKind.Panorama }, MediaPathRule.KindsOfAssetType(typeof(UnityEngine.Texture2D)),
                "a texture is looked up as a picture first, then as a panorama");
            CollectionAssert.AreEqual(new[] { MediaKind.Model }, MediaPathRule.KindsOfAssetType(typeof(UnityEngine.GameObject)));
        }

        // _3.1 step 10A: a model is a .glb / .gltf (glTFast's prefab); a panorama is a picture FILE but its own kind
        [Test]
        public void MediaPathRule_AModelIsAGlbOrGltf_APanoramaIsAPictureFile_EachOnlyInItsOwnField()
        {
            foreach (string model in new[] { "castle.glb", " arch/Arch.GLB ", "scan.gltf" })
                Assert.AreEqual(MediaPathProblem.None, MediaPathRule.Check(model, MediaKind.Model), model);
            foreach (string wrong in new[] { "castle.png", "castle.fbx", "castle.obj", "castle.glb.meta", "castle" })
                Assert.AreEqual(MediaPathProblem.WrongType, MediaPathRule.Check(wrong, MediaKind.Model), wrong + " is not a model the card loads");
            Assert.AreEqual(MediaPathProblem.OutsideFolder, MediaPathRule.Check("Assets/Apps/LivingRoom/scan.glb", MediaKind.Model), "a model outside the folder");
            Assert.AreEqual(MediaPathProblem.WrongType, MediaPathRule.Check("castle.glb", MediaKind.Image), "a model is not a picture");

            foreach (string pano in new[] { "room_360.jpg", "room_360.JPEG", "sky.png" })
                Assert.AreEqual(MediaPathProblem.None, MediaPathRule.Check(pano, MediaKind.Panorama), pano);
            Assert.AreEqual(MediaPathProblem.WrongType, MediaPathRule.Check("room.mp4", MediaKind.Panorama));
            Assert.AreEqual(MediaPathProblem.None, MediaPathRule.Check("default:tiled_room_360", MediaKind.Panorama), "a default key");

            Assert.AreEqual(MediaKind.Model, MediaPathRule.KindOfExtension("arch.glb"));
            Assert.AreEqual(MediaKind.Model, MediaPathRule.KindOfExtension("arch.gltf"));
            Assert.AreEqual(MediaKind.Image, MediaPathRule.KindOfExtension("room_360.jpg"), "by its extension a panorama is a picture file: its field says 360");

            var prefab = new UnityEngine.GameObject("model");
            var picture = new UnityEngine.Texture2D(2, 1);
            try
            {
                Assert.IsTrue(MediaPathRule.IsAssetOfKind(prefab, MediaKind.Model));
                Assert.IsFalse(MediaPathRule.IsAssetOfKind(picture, MediaKind.Model));
                Assert.IsTrue(MediaPathRule.IsAssetOfKind(picture, MediaKind.Panorama));
                Assert.IsFalse(MediaPathRule.IsAssetOfKind(prefab, MediaKind.Panorama));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(prefab);
                UnityEngine.Object.DestroyImmediate(picture);
            }
        }

        [Test]
        public void StoredPathFor_IsThePathInsideTheMediaFolder_ElseTheProjectPathAsPicked()
        {
            Assert.AreEqual("castle_hero.png", MediaPathRule.StoredPathFor("Assets/Apps/LivingRoom/Resources/LivingRoom/CardMedia/castle_hero.png", "LivingRoom/CardMedia"));
            Assert.AreEqual("sub/a.png", MediaPathRule.StoredPathFor("Assets/Apps/X/Resources/X/Media/sub/a.png", "/X/Media/"), "slashes around the folder");
            Assert.AreEqual("Media/a.png", MediaPathRule.StoredPathFor("Assets/Apps/X/Resources/Media/a.png", ""), "no folder: relative to Resources");
            string outside = "Assets/Framework/Runtime/UI/Markers/SymbolCircle.png";
            Assert.AreEqual(outside, MediaPathRule.StoredPathFor(outside, "LivingRoom/CardMedia"), "outside: stored as picked...");
            Assert.AreEqual(MediaPathProblem.OutsideFolder, MediaPathRule.Check(MediaPathRule.StoredPathFor(outside, "LivingRoom/CardMedia"), MediaKind.Image),
                "...so the rule reports it (the Editor never refuses a pick silently)");
            Assert.AreEqual("", MediaPathRule.StoredPathFor(null, "x"));
        }

        private static BlockKindDefinition PictureKind() => new()
        {
            Key = "picture",
            Family = "media",
            DisplayName = "Picture",
            Variants = new[] { "plain" },
            DefaultVariant = "plain",
            DisplayModes = new[] { CardOptions.DisplayInline },
            Fields = new[]
            {
                new BlockFieldDefinition { Key = "image", Type = BlockFieldType.Asset, Media = MediaKind.Image, Label = "Picture", Required = true },
                new BlockFieldDefinition { Key = "focus", Type = BlockFieldType.Number, Label = "Focus", NumberMin = 0f, NumberMax = 1f, NumberDefault = 0.5f },
                new BlockFieldDefinition
                {
                    Key = "rows", Type = BlockFieldType.Items, Label = "Pictures",
                    ItemFields = new[] { new BlockFieldDefinition { Key = "image", Type = BlockFieldType.Asset, Media = MediaKind.Image, Label = "Picture", Required = true } },
                },
            },
        };

        private static BlockStackBuilder.Result Build(BlockInstanceData block)
        {
            var registry = new BlockRegistry();
            BuiltInBlocks.Register(registry);
            registry.Register(PictureKind(), () => null);
            var poi = new POIData { id = "p", name = "Gate" };
            poi.card.blocks.Add(block);
            return BlockStackBuilder.Build(poi, new CardSettings(), registry);
        }

        private static BlockInstanceData Picture(string path) => new()
        {
            key = "block_1", kind = "picture",
            fields = new List<BlockFieldValue> { new() { key = "image", asset = path } },
        };

        [Test]
        public void ARequiredPicture_OutsideTheFolderOrNotAPicture_HidesItsBlock_AsInvalidMedia()
        {
            Assert.IsEmpty(Build(Picture("castle_hero.png")).Skipped, "a picture in the folder shows");
            var outside = Build(Picture("Assets/Art/hero.png")).Skipped.Single();
            Assert.AreEqual(BlockStackBuilder.SkipReason.InvalidMedia, outside.Reason);
            Assert.AreEqual("image", outside.FieldKey, "the reason names the field");
            Assert.AreEqual(BlockStackBuilder.SkipReason.InvalidMedia, Build(Picture("clip.mp3")).Skipped.Single().Reason, "wrong type");
            Assert.AreEqual(BlockStackBuilder.SkipReason.MissingRequired, Build(Picture("")).Skipped.Single().Reason, "nothing picked is still 'empty'");
        }

        [Test]
        public void AGalleryRow_WithAPictureTheRuleRefuses_IsIncomplete()
        {
            var rowFields = PictureKind().Field("rows").ItemFields;
            BlockItemData Row(string path) => new() { fields = new List<BlockItemFieldValue> { new() { key = "image", asset = path } } };
            Assert.IsTrue(BlockFieldReader.ItemIsComplete(Row("gallery_1.png"), rowFields));
            Assert.IsFalse(BlockFieldReader.ItemIsComplete(Row("Assets/x.png"), rowFields), "outside the folder");
            Assert.IsFalse(BlockFieldReader.ItemIsComplete(Row("notes.txt"), rowFields), "not a picture");
            Assert.IsFalse(BlockFieldReader.ItemIsComplete(Row(""), rowFields), "empty");

            var block = Picture("castle_hero.png");
            var reader = new BlockFieldReader(block, "en", "en");
            Assert.AreEqual("", reader.ItemValidAsset(Row("../x.png"), "image", MediaKind.Image), "the view never loads a refused path");
            Assert.AreEqual("gallery_1.png", reader.ItemValidAsset(Row(" gallery_1.png "), "image", MediaKind.Image));
            Assert.AreEqual("castle_hero.png", reader.ValidAsset("image", MediaKind.Image));
        }

        [Test]
        public void ANumberField_ReadsItsDefaultWhileUnset_AndStaysInsideItsRange()
        {
            var focus = PictureKind().Field("focus");
            var block = Picture("castle_hero.png");
            Assert.AreEqual(0.5f, new BlockFieldReader(block, null, null).Number(focus), "nothing stored: the default");
            block.fields.Add(new BlockFieldValue { key = "focus", number = 0f });
            Assert.AreEqual(0f, new BlockFieldReader(block, null, null).Number(focus), "a stored 0 is a value, not 'unset'");
            block.fields[1].number = 7f;
            Assert.AreEqual(1f, new BlockFieldReader(block, null, null).Number(focus), "clamped to the range");
        }

        [Test]
        public void ZoomPanRule_FitsWholeAtOne_ZoomsAboutThePoint_ClampsScaleAndPan()
        {
            var frame = new UnityEngine.Vector2(358f, 220f);
            Assert.AreEqual(new UnityEngine.Vector2(220f, 220f), ZoomPanRule.FitSize(frame, 1f), "a square fits the frame's height");
            Assert.AreEqual(new UnityEngine.Vector2(358f, 358f / 2f), ZoomPanRule.FitSize(frame, 2f), "a wide one fits its width");
            var whole = ZoomPanRule.Whole(frame, 1f);
            Assert.AreEqual(1f, whole.Scale);
            Assert.AreEqual((358f - 220f) / 2f, whole.Offset.x, 1e-3f, "centred across");
            Assert.AreEqual(0f, whole.Offset.y, 1e-3f);

            var point = new UnityEngine.Vector2(200f, 120f);
            var zoomed = ZoomPanRule.ZoomAbout(whole, 2f, point, frame, 1f);
            Assert.AreEqual(2f, zoomed.Scale);
            float u0 = (point.x - whole.Offset.x) / (220f * whole.Scale), u1 = (point.x - zoomed.Offset.x) / (220f * zoomed.Scale);
            Assert.AreEqual(u0, u1, 1e-3f, "the picture point under the fingers stays under them");
            Assert.AreEqual(ZoomPanRule.MaxScale, ZoomPanRule.ZoomAbout(zoomed, 100f, point, frame, 1f).Scale, "never past MaxScale");
            Assert.AreEqual(1f, ZoomPanRule.ZoomAbout(zoomed, 0.01f, point, frame, 1f).Scale, "never under 1");

            var panned = ZoomPanRule.Pan(zoomed, new UnityEngine.Vector2(10000f, -10000f), frame, 1f);
            Assert.AreEqual(0f, panned.Offset.x, 1e-3f, "no empty space on the left");
            Assert.AreEqual(220f - 440f, panned.Offset.y, 1e-3f, "...nor under the picture");
        }

        // _3.1 [7B] the lightbox swipe
        [Test]
        public void SwipePageRule_TurnsOnlyOnAFarEnoughClearlySidewaysDrag_LeftBringsTheNextPage()
        {
            const float page = 390f;
            float far = SwipePageRule.MinTravelShare * page + 1f;
            Assert.AreEqual(1, SwipePageRule.Direction(new UnityEngine.Vector2(-far, 0f), page), "finger moves left: the next page");
            Assert.AreEqual(-1, SwipePageRule.Direction(new UnityEngine.Vector2(far, 0f), page), "finger moves right: the previous page");
            Assert.AreEqual(0, SwipePageRule.Direction(new UnityEngine.Vector2(-(far - 2f), 0f), page), "not far enough: a wobbly tap, no turn");
            Assert.AreEqual(1, SwipePageRule.Direction(new UnityEngine.Vector2(-far, far / SwipePageRule.MinSideways - 1f), page), "a little up or down is fine");
            Assert.AreEqual(0, SwipePageRule.Direction(new UnityEngine.Vector2(-far, far / SwipePageRule.MinSideways + 1f), page), "too diagonal: not a page swipe");
            Assert.AreEqual(0, SwipePageRule.Direction(new UnityEngine.Vector2(0f, -page), page), "straight up: never a turn");
            Assert.AreEqual(0, SwipePageRule.Direction(new UnityEngine.Vector2(-page, 0f), 0f), "no page laid out yet: nothing");
            Assert.AreEqual(0, SwipePageRule.Direction(new UnityEngine.Vector2(-page, 0f), float.NaN), "...nor a NaN width");
        }

        [Test]
        public void SpotlightCropRule_CoversTheFrame_CentresTheFocus_WithinThePicturesEdges()
        {
            var frame = new UnityEngine.Vector2(358f, 200f);
            var cover = SpotlightCropRule.CoverSize(frame, 16f / 9f);
            Assert.AreEqual(358f, cover.x, 1e-3f, "a wide picture in a wider-ratio frame: width fits...");
            Assert.GreaterOrEqual(cover.y, 200f, "...height overflows: covered");
            var centre = SpotlightCropRule.Place(frame, 16f / 9f, new UnityEngine.Vector2(0.5f, 0.5f), 2f);
            Assert.AreEqual(frame / 2f, centre.Focus, "a centred focus lands in the frame's centre");
            var corner = SpotlightCropRule.Place(frame, 16f / 9f, new UnityEngine.Vector2(0f, 0f), 2f);
            Assert.AreEqual(UnityEngine.Vector2.zero, corner.Offset, "a corner focus: the picture's edge stops at the frame's edge");
            Assert.AreEqual(UnityEngine.Vector2.zero, corner.Focus, "...so the ring sits in the corner");
            Assert.AreEqual(cover * 2f, corner.Size, "enlarged zoom times");
            Assert.AreEqual(cover, SpotlightCropRule.Place(frame, 16f / 9f, new UnityEngine.Vector2(0.3f, 0.3f), 0.2f).Size, "zoom below 1 counts as 1");
        }
    }
}
