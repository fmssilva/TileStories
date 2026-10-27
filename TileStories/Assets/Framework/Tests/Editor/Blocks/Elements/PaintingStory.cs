using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // The Painting's story as a composed block (_3.3 R14.7: the story slice's point), made from The Painting's OWN shipped content on a
    // fresh copy of the shipped config (never saved): its header's picture, eyebrow and title, its shipped story's lede as the lede and its
    // story's paragraphs as ONE paragraph, its gallery's four pictures with their captions and credits as a carousel, then a divider. The composed
    // kind is registered the way an app registers one (a BlockKindDefinition with an Elements Page into a fresh BlockRegistry), because the
    // framework's own `story` kind arrives with its view at S2. Expected values are read from the shipped blocks, never typed again.
    internal sealed class PaintingStory
    {
        public const string ConfigPath = "Assets/Apps/LivingRoom/config.json";
        public const string StreamingConfigPath = "Assets/StreamingAssets/LivingRoom/config.json";
        public const string PaintingId = "painting";
        public const string StoryKindKey = "test_story";
        public const string PageField = "page";
        public const string StoryBlockKey = "block_story";
        // The second composed block of The Painting: what a scope-block reference points into
        public const string SecondBlockKey = "block_story_2";
        // The Painting's shipped blocks the story is made from
        public const string HeaderKey = "block_1";
        public const string GalleryKey = "block_5";

        public WallConfigData Config;
        public POIData Painting;
        public BlockRegistry Blocks;
        public ElementRegistry Elements;
        public BlockKindDefinition Kind;
        public BlockInstanceData Block;
        public BlockInstanceData Second;
        // The shipped blocks it is made from: the header, The Painting's own story (the framework's story kind: its lede and paragraphs
        // are the texts), the gallery
        public BlockInstanceData Header, Text, Gallery;
        // The Page rows in order: Picture el_1, Eyebrow el_2, Title el_3, Lede el_4, Paragraph el_5, Carousel el_6, Divider el_7
        public ElementRow Picture, Eyebrow, Title, Lede, Paragraph, Carousel, Divider;
        // The second block's one row: a Heading (its id el_1 in ITS block)
        public ElementRow SecondHeading;

        // A composed kind as an app would declare it: a required Elements Page and nothing else of its own
        public static BlockKindDefinition StoryKind() => new()
        {
            Key = StoryKindKey, Family = BuiltInFamilies.About, Format = CardOptions.FormatRead, IconKey = CardIcons.Book,
            LabelKey = CardStrings.Keys.KindRichText, DisplayName = "Test Story", Help = "A composed block of element rows.",
            Variants = new[] { "page" }, DefaultVariant = "page", DisplayModes = new[] { CardOptions.DisplayInline, CardOptions.DisplayExpandable },
            Fields = new[] { new BlockFieldDefinition { Key = PageField, Type = BlockFieldType.Elements, Label = "Page", Required = true, Help = "The rows." } },
        };

        // The framework's kinds plus the composed one, in a registry of the test's own
        public static BlockRegistry RegistryWithStory()
        {
            var registry = new BlockRegistry();
            BuiltInBlocks.Register(registry);
            registry.Register(StoryKind(), () => new RichTextBlockView());
            return registry;
        }

        public static WallConfigData ShippedConfig() => JsonUtility.FromJson<WallConfigData>(File.ReadAllText(ConfigPath));

        // The story on The Painting of a fresh shipped config
        public static PaintingStory Build()
        {
            var s = new PaintingStory { Config = ShippedConfig(), Blocks = RegistryWithStory(), Elements = ElementRegistry.Shared };
            s.Painting = s.Config.pois.Single(p => p.id == PaintingId);
            s.Kind = s.Blocks.TryGet(StoryKindKey, out var kind) ? kind : null;
            s.Header = s.Painting.card.blocks.Single(b => b.key == HeaderKey);
            s.Text = s.Painting.card.blocks.Single(b => b.kind == BuiltInBlocks.StoryKind);
            s.Gallery = s.Painting.card.blocks.Single(b => b.key == GalleryKey);

            s.Block = new BlockInstanceData { key = StoryBlockKey, kind = StoryKindKey };
            s.Painting.card.blocks.Add(s.Block);
            s.Picture = s.AddPage(BuiltInElements.Picture);
            s.Picture.AddMedia(s.Header.fields.Single(f => f.key == BuiltInBlocks.HeaderImageField).asset, null, "");
            s.Eyebrow = s.AddPage(BuiltInElements.Eyebrow);
            SetText(s.Eyebrow, BuiltInElements.TextField, TextOf(s.Header, "eyebrow"));
            s.Title = s.AddPage(BuiltInElements.Title);
            SetText(s.Title, BuiltInElements.TextField, TextOf(s.Header, BlockStackBuilder.HeaderTitleField));
            s.Lede = s.AddPage(BuiltInElements.Lede);
            SetText(s.Lede, BuiltInElements.TextField, Copy(StoryRows(s.Text, BuiltInElements.Lede).First().Field(BuiltInElements.TextField).text));
            s.Paragraph = s.AddPage(BuiltInElements.Paragraph);
            SetText(s.Paragraph, BuiltInElements.TextField, Joined(StoryRows(s.Text, BuiltInElements.Paragraph)));
            s.Carousel = s.AddPage(BuiltInElements.Carousel);
            foreach (var picture in s.GalleryPictures())
                s.Carousel.AddMedia(Sub(picture, BuiltInBlocks.GalleryImageField).asset, Copy(Sub(picture, BuiltInBlocks.GalleryCaptionField).text),
                    Sub(picture, BuiltInBlocks.GalleryCreditField).text.FirstOrDefault()?.value ?? Sub(picture, BuiltInBlocks.GalleryCreditField).value);
            s.Divider = s.AddPage(BuiltInElements.Divider);

            s.Second = new BlockInstanceData { key = SecondBlockKey, kind = StoryKindKey };
            s.Painting.card.blocks.Add(s.Second);
            s.SecondHeading = AddRow(s.Second, s.Elements, BuiltInElements.Heading);
            SetText(s.SecondHeading, BuiltInElements.TextField, new List<LocalizedEntry> { new() { lang = "en", value = "After the earthquake" }, new() { lang = "pt", value = "Depois do terramoto" } });

            Assert.AreEqual("el_7", s.Divider.Id, "precondition: the Page's seven rows were born el_1..el_7");
            Assert.AreEqual(4, s.Carousel.Media.Count, "precondition: the carousel carries The Painting's four gallery pictures");
            return s;
        }

        // The scope of the story block on The Painting (no shared elements: none are stored before phase 2)
        public ElementScope Scope => ElementScope.Of(Painting, Block, Blocks);

        // The story block's Page rows / Preview rows as stored
        public BlockFieldValue PageValue => Value(Block, PageField);
        public BlockFieldValue PreviewValue => Value(Block, BlockSurfaces.PreviewRowsField);

        // A new row of `element` at the end of the story's Page
        public ElementRow AddPage(string element) => AddRow(Block, Elements, element);

        // A new row of `element` at the end of `block`'s Page
        public static ElementRow AddRow(BlockInstanceData block, ElementRegistry elements, string element)
        {
            Assert.IsTrue(elements.TryGet(element, out var definition), "precondition: element '" + element + "' is registered");
            var row = ElementRow.Create(block, definition);
            Value(block, PageField).items.Add(row.Data);
            return row;
        }

        // Add a row to the story's Custom preview
        public void AddPreview(ElementRow row) => PreviewValue.items.Add(row.Data);

        // The shipped gallery's rows
        public IReadOnlyList<BlockItemData> GalleryPictures() => Gallery.fields.Single(f => f.key == BuiltInBlocks.GalleryItemsField).items;

        // A text sub-field of a row, written with these entries
        public static void SetText(ElementRow row, string key, List<LocalizedEntry> entries) =>
            row.Data.fields.Add(new BlockItemFieldValue { key = key, text = entries });

        // The entries of a shipped block's text field, copied
        public static List<LocalizedEntry> TextOf(BlockInstanceData block, string key) => Copy(block.fields.Single(f => f.key == key).text);

        public static List<LocalizedEntry> Copy(List<LocalizedEntry> entries) =>
            entries == null ? new List<LocalizedEntry>() : entries.Select(e => new LocalizedEntry { lang = e.lang, value = e.value }).ToList();

        // The block's stored value with this key, created when missing
        public static BlockFieldValue Value(BlockInstanceData block, string key)
        {
            var value = block.fields.Find(f => f.key == key);
            if (value != null) return value;
            value = new BlockFieldValue { key = key };
            block.fields.Add(value);
            return value;
        }

        private static BlockItemFieldValue Sub(BlockItemData row, string key) => row.fields.Find(f => f.key == key) ?? new BlockItemFieldValue { key = key };

        // The shipped story's Page rows of one element, in order
        private static List<ElementRow> StoryRows(BlockInstanceData story, string element) =>
            BlockSurfaces.PageRows(story, BuiltInBlocks.Story).Select(r => new ElementRow(r)).Where(r => r.Kind == element).ToList();

        // Each language's words of these rows, one paragraph after another (separated by an empty line)
        private static List<LocalizedEntry> Joined(List<ElementRow> rows)
        {
            var languages = rows.SelectMany(r => r.Field(BuiltInElements.TextField).text).Select(e => e.lang).Distinct().ToList();
            Assert.IsNotEmpty(languages, "precondition: the shipped story has paragraphs with words");
            return languages.Select(lang => new LocalizedEntry
            {
                lang = lang,
                value = string.Join("\n\n", rows.Select(r => r.Field(BuiltInElements.TextField).text.FirstOrDefault(e => e.lang == lang)?.value).Where(v => !string.IsNullOrEmpty(v))),
            }).ToList();
        }
    }
}
