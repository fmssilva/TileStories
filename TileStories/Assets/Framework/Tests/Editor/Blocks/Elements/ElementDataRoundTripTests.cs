using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // The composed blocks' data (_3.3 R14.1, R17.6) through JsonUtility, the serializer the config, the Editor's undo history and the
    // runtime loader all use. The element data lives under reserved keys INSIDE the existing classes, so:
    //   - both shipped config copies still round-trip byte for byte, also after every block was read through the new accessors (reading
    //     never writes) -- the guard that no new serialized member slipped into the config classes
    //   - a config written before elements reads as "no rows, Automatic preview, Auto reading time, nothing overridden"
    //   - The Painting's story keeps every row value, media item, reference and surface value through Save / load
    //   - a row id is never handed out twice in a block, even after the row holding it was deleted
    public class ElementDataRoundTripTests
    {
        [TestCase(PaintingStory.ConfigPath)]
        [TestCase(PaintingStory.StreamingConfigPath)]
        public void ShippedConfig_RoundTripsByteIdentical_EvenAfterReadingEveryBlockThroughTheElementAccessors(string path)
        {
            string original = File.ReadAllText(path);
            var config = JsonUtility.FromJson<WallConfigData>(original);
            Assert.AreEqual(original, JsonUtility.ToJson(config, prettyPrint: true), path + ": the shipped file is what the classes write back");

            int blocks = 0;
            foreach (var poi in config.pois)
                foreach (var block in poi.card.blocks)
                {
                    BlockRegistry.Shared.TryGet(block.kind, out var kind);
                    var scope = ElementScope.Of(poi, block, BlockRegistry.Shared);
                    _ = BlockSurfaces.PageRows(block, kind);
                    _ = BlockSurfaces.PreviewRows(block);
                    _ = BlockSurfaces.PreviewMode(block);
                    _ = BlockSurfaces.OverrideOf(block, ElementOptions.ComponentOpenRow).IsOn;
                    _ = ElementPreviewRule.Of(scope, ElementRegistry.Shared);
                    _ = ElementValidation.Of(scope, ElementRegistry.Shared);
                    _ = ReadingTimeRule.Minutes(kind, block, 0f);
                    _ = BlockSummaryRule.FirstWords(block, kind, "en", "en");
                    blocks++;
                }
            Assert.Greater(blocks, 90, "precondition: the shipped wall's blocks were all read (97 at the time of writing)");
            Assert.AreEqual(original, JsonUtility.ToJson(config, prettyPrint: true), path + ": reading through the element accessors wrote nothing");
        }

        [Test]
        public void ABlockWrittenBeforeElements_ReadsAsNoRows_Automatic_AutoMinutes_NothingOverridden()
        {
            // - a composed kind's block as a config from before S1 holds it: key and kind, no element data at all
            var block = JsonUtility.FromJson<BlockInstanceData>("{\"key\":\"block_9\",\"kind\":\"" + PaintingStory.StoryKindKey + "\"}");
            var kind = PaintingStory.StoryKind();
            var scope = new ElementScope(block, kind, null, null, null);

            Assert.AreEqual(0, BlockSurfaces.PageRows(block, kind).Count);
            Assert.AreEqual(0, BlockSurfaces.PreviewRows(block).Count);
            Assert.AreEqual(ElementOptions.PreviewAutomatic, BlockSurfaces.PreviewMode(block));
            Assert.AreEqual(0, BlockSurfaces.MinutesSetByHand(block));
            Assert.IsFalse(BlockSurfaces.OverrideOf(block, ElementOptions.ComponentOpenRow).IsOn);
            Assert.AreEqual(ElementOptions.EmphasisPlain, BlockSurfaces.OverrideOf(block, ElementOptions.ComponentOpenRow).Emphasis);
            Assert.AreEqual(0, BlockSurfaces.RowsToValidate(block, kind));
            Assert.AreEqual(0, ElementRules.Minutes(scope, ElementRegistry.Shared, "en", "en"));
            Assert.AreEqual("", ElementRules.FirstWords(scope, ElementRegistry.Shared, "en", "en"));
            Assert.AreEqual(0, ElementPreviewRule.Of(scope, ElementRegistry.Shared).Entries.Count);
            Assert.AreEqual(0, ElementValidation.Of(scope, ElementRegistry.Shared).Count);
            Assert.AreEqual(0, block.fields.Count, "reading wrote nothing into the block");
        }

        [Test]
        public void ThePaintingsStory_KeepsEveryRowValue_MediaItem_Reference_AndSurfaceValue_ThroughSaveAndLoad()
        {
            var s = PaintingStory.Build();
            // - the parts a story holds beyond its Page: a Custom preview (an own teaser, links into all three scopes), a by-hand Reading
            //   Time, an Open Row override, a strong motion, a stored tap, mixed Validated flags
            PaintingStory.Value(s.Block, BlockSurfaces.PreviewModeField).value = ElementOptions.PreviewCustom;
            var teaser = ElementRow.Create(s.Block, Element(BuiltInElements.Lede));
            PaintingStory.SetText(teaser, BuiltInElements.TextField, Entries(("en", "A palace the sea took back."), ("pt", "Um palácio que o mar levou.")));
            s.AddPreview(teaser);
            var toTitle = ElementRow.CreateRef(s.Block, ElementRef.ToPage(s.Title.Id));
            toTitle.SetValue(BuiltInElements.SizeStyle, BuiltInElements.SizeMedium);
            s.AddPreview(toTitle);
            s.AddPreview(ElementRow.CreateRef(s.Block, ElementRef.ToBlock(PaintingStory.SecondBlockKey, s.SecondHeading.Id)));
            s.AddPreview(ElementRow.CreateRef(s.Block, ElementRef.ToShared("el_40")));
            PaintingStory.Value(s.Block, BlockKindDefinition.MinutesField).number = 7f;
            var open = new BlockItemData();
            open.fields.Add(new BlockItemFieldValue { key = BlockSurfaces.ComponentKey, value = ElementOptions.ComponentOpenRow });
            open.fields.Add(new BlockItemFieldValue { key = BlockSurfaces.OverrideKey, flag = true });
            open.fields.Add(new BlockItemFieldValue { key = BlockSurfaces.WordsKey, text = Entries(("en", "Read the story"), ("pt", "Ler a história")) });
            open.fields.Add(new BlockItemFieldValue { key = BlockSurfaces.EmphasisKey, value = ElementOptions.EmphasisShine });
            PaintingStory.Value(s.Block, BlockSurfaces.ComponentOverridesField).items.Add(open);
            s.Picture.SetMotion(ElementOptions.MotionSlowZoom, ElementOptions.StrengthStrong);
            s.Picture.SetValue(ElementRow.TapKey, ElementOptions.TapNone);
            s.Title.SetValidated(true);
            s.Carousel.SetValidated(true);
            var video = s.AddPage(BuiltInElements.Video);
            video.AddMedia("video/lisbon_square_castle_55s.mp4", Entries(("en", "The square today"), ("pt", "A praça hoje")), "CC BY-SA");
            video.SetValue(ElementRow.TapKey, ElementOptions.TapOpenPlayer);

            string saved = JsonUtility.ToJson(s.Config, prettyPrint: true);
            var loaded = JsonUtility.FromJson<WallConfigData>(saved);
            Assert.AreEqual(saved, JsonUtility.ToJson(loaded, prettyPrint: true), "a second Save writes the same bytes");

            var block = loaded.pois.Single(p => p.id == PaintingStory.PaintingId).card.blocks.Single(b => b.key == PaintingStory.StoryBlockKey);
            AssertSameRows(BlockSurfaces.PageRows(s.Block, s.Kind), BlockSurfaces.PageRows(block, s.Kind));
            AssertSameRows(BlockSurfaces.PreviewRows(s.Block), BlockSurfaces.PreviewRows(block));
            Assert.AreEqual(ElementOptions.PreviewCustom, BlockSurfaces.PreviewMode(block));
            Assert.AreEqual(7, BlockSurfaces.MinutesSetByHand(block));
            var openRow = BlockSurfaces.OverrideOf(block, ElementOptions.ComponentOpenRow);
            Assert.IsTrue(openRow.IsOn);
            Assert.AreEqual("Ler a história", openRow.Words("pt", "en"));
            Assert.AreEqual(ElementOptions.EmphasisShine, openRow.Emphasis);
            Assert.AreEqual(10, BlockSurfaces.RowsToValidate(block, s.Kind), "8 Page rows (Title and Carousel validated: 6 left) + 4 Preview rows (none validated)");
        }

        [Test]
        public void ARowId_IsNeverHandedOutTwice_AfterTheLastRowIsDeleted_AcrossPageAndPreview_AndAboveAHandEditedId()
        {
            var s = PaintingStory.Build();
            // - delete the LAST row (el_7): a free-slot rule would give el_7 back, and a link still naming el_7 would quietly show the new row
            s.PageValue.items.RemoveAt(s.PageValue.items.Count - 1);
            Assert.AreEqual("el_8", s.AddPage(BuiltInElements.Spacer).Id);
            // - a Preview row draws from the same numbers: ids are unique in the block, not per surface
            Assert.AreEqual("el_9", ElementRow.CreateRef(s.Block, ElementRef.ToPage("el_1")).Id);
            // - a hand-edited file holding a higher id moves the count past it
            new ElementRow(s.PageValue.items[0]).SetValue(ElementRow.IdKey, "el_40");
            Assert.AreEqual("el_41", s.AddPage(BuiltInElements.Spacer).Id);
            // - another block counts on its own (the block-key rule is per block)
            Assert.AreEqual("el_2", PaintingStory.AddRow(s.Second, s.Elements, BuiltInElements.Spacer).Id);
            // - the memory survives Save / load
            var loaded = JsonUtility.FromJson<BlockInstanceData>(JsonUtility.ToJson(s.Block));
            loaded.fields.Find(f => f.key == PaintingStory.PageField).items.Clear();
            Assert.AreEqual("el_42", BlockSurfaces.TakeRowId(loaded), "with every row gone, the next id still follows the highest ever handed out");
        }

        [Test]
        public void Duplicate_CopiesEverySubFieldAndMediaItem_WithAFreshId_NotValidated()
        {
            var s = PaintingStory.Build();
            s.Carousel.SetValidated(true);
            var copy = ElementRow.Duplicate(s.Block, s.Carousel);

            Assert.AreEqual("el_8", copy.Id);
            Assert.IsFalse(copy.Validated, "a duplicated row starts to be validated again (R14.8)");
            Assert.AreEqual(s.Carousel.Kind, copy.Kind);
            Assert.AreEqual(s.Carousel.Media.Select(m => m.Asset), copy.Media.Select(m => m.Asset));
            Assert.AreEqual(s.Carousel.Media.Select(m => m.Caption("pt", "en")), copy.Media.Select(m => m.Caption("pt", "en")));
            // - a copy, not a shared list: changing the copy leaves the original alone
            copy.Media[0].Data.text[0].value = "changed";
            Assert.AreNotEqual("changed", s.Carousel.Media[0].Data.text[0].value);
        }

        private static ElementKindDefinition Element(string key) => ElementRegistry.Shared.TryGet(key, out var e) ? e : null;

        private static List<LocalizedEntry> Entries(params (string lang, string value)[] texts) =>
            texts.Select(t => new LocalizedEntry { lang = t.lang, value = t.value }).ToList();

        // Row by row through the typed accessors: identity, kind, reference, motion, tap, Validated, every sub-field, every media item
        private static void AssertSameRows(IReadOnlyList<BlockItemData> expected, IReadOnlyList<BlockItemData> actual)
        {
            Assert.AreEqual(expected.Count, actual.Count, "row count");
            for (int i = 0; i < expected.Count; i++)
            {
                var e = new ElementRow(expected[i]);
                var a = new ElementRow(actual[i]);
                string at = "row " + (i + 1) + " (" + e.Id + ")";
                Assert.AreEqual(e.Id, a.Id, at);
                Assert.AreEqual(e.Kind, a.Kind, at);
                Assert.AreEqual(e.IsRef, a.IsRef, at);
                Assert.AreEqual(e.Ref.Scope, a.Ref.Scope, at);
                Assert.AreEqual(e.Ref.Block, a.Ref.Block, at);
                Assert.AreEqual(e.Ref.Row, a.Ref.Row, at);
                Assert.AreEqual(e.Motion, a.Motion, at);
                Assert.AreEqual(e.Strength, a.Strength, at);
                Assert.AreEqual(e.Tap, a.Tap, at);
                Assert.AreEqual(e.Validated, a.Validated, at);
                Assert.AreEqual(e.Data.fields.Count, a.Data.fields.Count, at + ": sub-field count");
                for (int f = 0; f < e.Data.fields.Count; f++)
                {
                    var ef = e.Data.fields[f];
                    var af = a.Data.fields[f];
                    Assert.AreEqual(ef.key, af.key, at);
                    Assert.AreEqual(ef.value, af.value, at + " " + ef.key);
                    Assert.AreEqual(ef.asset, af.asset, at + " " + ef.key);
                    Assert.AreEqual(ef.flag, af.flag, at + " " + ef.key);
                    Assert.AreEqual(ef.text.Select(t => t.lang + "=" + t.value), af.text.Select(t => t.lang + "=" + t.value), at + " " + ef.key);
                }
                Assert.AreEqual(e.Media.Select(m => m.Asset + "|" + m.Credit), a.Media.Select(m => m.Asset + "|" + m.Credit), at + ": media");
            }
        }
    }
}
