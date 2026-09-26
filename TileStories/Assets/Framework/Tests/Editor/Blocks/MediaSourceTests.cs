using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Editor.Tests
{
    // The card's media seam (_3.1 step 5) on the REAL fixture file (Apps/LivingRoom/Resources/LivingRoom/CardMedia/
    // placeholder_hero.png): ResourcesMediaSource counts loads and frees an asset on its last release, a missing file
    // is null + one warning (never an exception), ScopedMediaSource gives back exactly what it loaded, and
    // BlockStackView loads lazily on Bind and releases on unbind. No built-in kind shows media before Tier 2, so the
    // stack test registers one kind through the public BlockRegistry API -- the extension point an app uses.
    public class MediaSourceTests
    {
        private const string Root = "LivingRoom/CardMedia";
        private const string Hero = "placeholder_hero.png";

        // An image block registered the way an app registers a kind: its view loads its Asset field when bound
        private sealed class ImageProbeView : IBlockView
        {
            public VisualElement Root { get; } = new();
            public readonly Image Image = new();
            public ImageProbeView() => Root.Add(Image);

            public void Bind(BlockInstanceData instance, BlockBindContext context)
            {
                string path = new BlockFieldReader(instance, context.Language, context.FallbackLanguage).Asset("image");
                Image.image = context.Media.Load<Texture2D>(path);
                Root.EnableInClassList("card-media--unavailable", Image.image == null);
            }

            public void Unbind() => Image.image = null;
        }

        private static readonly BlockKindDefinition ImageKind = new()
        {
            Key = "image_probe", Family = "media", DisplayName = "Image probe", Help = "test kind",
            Variants = new[] { "plain" }, DefaultVariant = "plain", DisplayModes = new[] { CardOptions.DisplayInline },
            Fields = new[] { new BlockFieldDefinition { Key = "image", Type = BlockFieldType.Asset, Label = "Image" } },
        };

        private static POIData PoiWithImage(string id, string asset)
        {
            var poi = new POIData { id = id, name = id };
            poi.card.blocks.Add(new BlockInstanceData { key = "block_1", kind = ImageKind.Key,
                fields = new List<BlockFieldValue> { new() { key = "image", asset = asset } } });
            return poi;
        }

        [Test]
        public void Load_ReturnsTheRealAsset_CountsLoads_AndTheLastReleaseFreesIt()
        {
            var source = new ResourcesMediaSource(Root);
            var a = source.Load<Texture2D>(Hero);
            var b = source.Load<Texture2D>("placeholder_hero");
            Assert.IsNotNull(a, "the fixture image loads through Resources");
            Assert.AreEqual(512, a.width);
            Assert.AreSame(a, b, "with or without the extension: one asset");
            Assert.AreEqual(2, source.RefCount(Hero));
            Assert.AreEqual(1, source.LoadedCount);
            source.Release(Hero);
            Assert.AreEqual(1, source.RefCount(Hero), "still held by the second Load");
            source.Release(Hero);
            Assert.AreEqual(0, source.RefCount(Hero));
            Assert.AreEqual(0, source.LoadedCount, "the last release frees it");
            source.Release(Hero);
            Assert.AreEqual(0, source.LoadedCount, "an extra release is harmless");
        }

        [Test]
        public void AMissingFile_IsNull_WithOneWarning_NeverAnException()
        {
            var source = new ResourcesMediaSource(Root);
            LogAssert.Expect(LogType.Warning, new Regex(@"\[Card\] media not found: Resources/LivingRoom/CardMedia/nothing_here"));
            Assert.IsNull(source.Load<Texture2D>("nothing_here.png"));
            Assert.IsNull(source.Load<Texture2D>("nothing_here.png"), "asked again: still null, no second warning");
            Assert.IsNull(source.Load<Texture2D>(""), "an empty path is simply nothing");
            Assert.DoesNotThrow(() => source.Release("nothing_here.png"));
            Assert.AreEqual(0, source.LoadedCount);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void AScope_GivesBackExactlyWhatItLoaded()
        {
            var source = new ResourcesMediaSource(Root);
            var first = new ScopedMediaSource(source);
            var second = new ScopedMediaSource(source);
            first.Load<Texture2D>(Hero);
            first.Load<Texture2D>(Hero);
            second.Load<Texture2D>(Hero);
            Assert.AreEqual(3, source.RefCount(Hero));
            first.ReleaseAll();
            Assert.AreEqual(1, source.RefCount(Hero), "the other scope still holds its load");
            first.ReleaseAll();
            Assert.AreEqual(1, source.RefCount(Hero), "releasing twice gives nothing back twice");
            second.Release(Hero);
            Assert.AreEqual(0, source.LoadedCount);
        }

        [Test]
        public void TheStack_LoadsOnBind_AndReleasesOnUnbind_ASwitchOrACloseHoldsNothing()
        {
            var registry = new BlockRegistry();
            BuiltInBlocks.Register(registry);
            registry.Register(ImageKind, () => new ImageProbeView());
            var source = new ResourcesMediaSource(Root);
            var stack = new BlockStackView(new VisualElement(), new VisualElement(), registry);
            var settings = new CardSettings();

            void Show(POIData poi) => stack.Bind(BlockStackBuilder.Build(poi, settings, registry).Entries,
                new BlockBindContext { Poi = poi, Language = "en", FallbackLanguage = "en", Media = source });

            Assert.AreEqual(0, source.LoadedCount, "nothing loaded before a card opens");
            Show(PoiWithImage("with_image", Hero));
            var view = (ImageProbeView)stack.BoundViews[1];
            Assert.IsNotNull(view.Image.image, "loaded when bound");
            Assert.AreEqual(1, source.RefCount(Hero));

            Show(new POIData { id = "plain", name = "Plain" });
            Assert.AreEqual(0, source.LoadedCount, "switching to a POI without media released it");

            Show(PoiWithImage("with_image", Hero));
            Assert.AreSame(view, stack.BoundViews[1], "the pooled view is bound again");
            Assert.AreEqual(1, source.RefCount(Hero), "one load per bind, never piling up");
            stack.UnbindAll();
            Assert.AreEqual(0, source.LoadedCount, "closing the card holds nothing");

            LogAssert.Expect(LogType.Warning, new Regex("media not found"));
            Assert.DoesNotThrow(() => Show(PoiWithImage("missing", "gone.png")));
            view = (ImageProbeView)stack.BoundViews[1];
            Assert.IsNull(view.Image.image);
            Assert.IsTrue(view.Root.ClassListContains("card-media--unavailable"), "the block shows its media-unavailable state");
            stack.UnbindAll();
        }
    }
}
