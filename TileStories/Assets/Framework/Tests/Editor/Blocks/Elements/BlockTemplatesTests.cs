using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // BlockTemplates (_3.3 S3b, R14.4) on the SHIPPED wall with the card's own registries: the built-in magazine IS The Painting's shipped
    // story's structure; a template holds structure only (no words, no files); a block made from one gets copies (fresh ids, the looks);
    // Re-apply adds exactly the template rows a block lacks, in their template place, never touching a row it has; template keys are never
    // handed to a new template while a block still names them.
    public class BlockTemplatesTests
    {
        private WallConfigData _config;
        private POIData _painting;
        private BlockInstanceData _story;
        private BlockKindDefinition _kind;
        private static ElementRegistry Elements => ElementRegistry.Shared;

        [SetUp]
        public void LoadTheShippedStory()
        {
            _config = PaintingStory.ShippedConfig();
            _painting = _config.pois.Single(p => p.id == PaintingStory.PaintingId);
            _story = _painting.card.blocks.Single(b => b.kind == BuiltInBlocks.StoryKind);
            Assert.IsTrue(BlockRegistry.Shared.TryGet(BuiltInBlocks.StoryKind, out _kind));
            Assert.AreEqual(17, Page(_story).Count, "precondition: the shipped story's 17 rows");
            Assert.IsEmpty(_config.card_settings.block_templates, "precondition: the shipped wall has no templates of its own");
        }

        private List<BlockItemData> Page(BlockInstanceData block) => ElementEdits.PageList(block, _kind);
        private static BlockTemplateData Magazine => BlockTemplates.BuiltIn(Elements).Single(t => t.key == BlockTemplates.MagazineKey);
        private static BlockTemplateData Short => BlockTemplates.BuiltIn(Elements).Single(t => t.key == BlockTemplates.ShortKey);
        private static List<string> Kinds(IEnumerable<BlockItemData> rows) => rows.Select(r => new ElementRow(r).Kind).ToList();

        // A row's structure in one comparable line: its element, every look value it holds, its motion, strength and tap
        private static string Structure(BlockItemData data)
        {
            var row = new ElementRow(data);
            Elements.TryGet(row.Kind, out var element);
            var looks = element?.StyleFields?.Select(s => s.Key + "=" + row.Value(s.Key)) ?? Enumerable.Empty<string>();
            return row.Kind + "|" + string.Join(",", looks) + "|" + row.Motion + "|" + row.Strength + "|" + row.Tap;
        }

        // Whether a row holds any content: words in a text sub-field or a media item
        private static bool HoldsContent(BlockItemData data) =>
            data.fields.Any(f => f.key == ElementRow.MediaKey || (f.text != null && f.text.Any(t => !string.IsNullOrEmpty(t.value))) || !string.IsNullOrEmpty(f.asset));

        [Test]
        public void TheBuiltInMagazine_IsTheShippedStorysStructure_RowForRow_WithNoContent_AndTheShortOne_TitleLedeParagraphPicture()
        {
            CollectionAssert.AreEqual(Page(_story).Select(Structure), BlockTemplates.RowsOf(Magazine).Select(Structure),
                "Story -- magazine = The Painting's story's elements, looks, motions and taps, in order");
            Assert.IsFalse(BlockTemplates.RowsOf(Magazine).Any(HoldsContent), "a template holds no words and no files");
            Assert.AreEqual(BuiltInBlocks.StoryKind, Magazine.block.kind);
            CollectionAssert.AreEqual(new[] { BuiltInElements.Title, BuiltInElements.Lede, BuiltInElements.Paragraph, BuiltInElements.Picture },
                Kinds(BlockTemplates.RowsOf(Short)));
            Assert.AreEqual("Story, 4 rows: Title, Lede, Paragraph, Picture", BlockTemplates.Describe(Short, Elements));
            // - new objects every call: an edit of one never reaches the code's
            BlockTemplates.RowsOf(Magazine);
            var first = BlockTemplates.BuiltIn(Elements)[0];
            new ElementRow(BlockTemplates.RowsOf(first)[0]).SetValue(BuiltInElements.WidthStyle, BuiltInElements.WidthInset);
            Assert.AreEqual(BuiltInElements.WidthFullBleed, new ElementRow(BlockTemplates.RowsOf(Magazine)[0]).Value(BuiltInElements.WidthStyle));
        }

        [Test]
        public void SaveAs_KeepsEveryRowsStructure_DropsEveryWordAndFile_AndTheBlockNowNamesIt()
        {
            var template = BlockTemplates.SaveAs(_config.card_settings, _config.pois, _story, _kind, "My story", Elements);
            Assert.AreEqual("template_1", template.key);
            Assert.AreSame(template, _config.card_settings.block_templates.Single(), "added to the wall's list");
            CollectionAssert.AreEqual(Page(_story).Select(Structure), BlockTemplates.RowsOf(template).Select(Structure), "the structure, row for row");
            Assert.IsTrue(Page(_story).Any(HoldsContent), "precondition: the story has words and pictures");
            Assert.IsFalse(BlockTemplates.RowsOf(template).Any(HoldsContent), "the template has none of them");
            Assert.IsFalse(BlockTemplates.RowsOf(template).Any(r => new ElementRow(r).Validated), "no row arrives validated");
            Assert.AreEqual(template.key, BlockSurfaces.TemplateOf(_story), "the block is now made from it");
            Assert.AreEqual(1, BlockTemplates.BlocksMadeFrom(_config.pois, template.key));
        }

        [Test]
        public void NewBlock_CopiesEveryRow_FreshIdsAndKey_TheLooks_NoContent_AndRemembersTheTemplate()
        {
            var camera = _config.pois.Single(p => p.id == "camera");
            camera.card ??= new POICardData();
            camera.card.last_block_number = 41;
            var block = BlockTemplates.NewBlock(camera.card, Magazine, Elements);
            Assert.AreEqual("block_42", block.key, "the card's next block key");
            Assert.AreEqual(BuiltInBlocks.StoryKind, block.kind);
            Assert.AreEqual(BlockTemplates.MagazineKey, BlockSurfaces.TemplateOf(block));
            CollectionAssert.AreEqual(BlockTemplates.RowsOf(Magazine).Select(Structure), Page(block).Select(Structure), "every row's structure");
            CollectionAssert.AreEqual(Enumerable.Range(1, 17).Select(n => "el_" + n), Page(block).Select(r => new ElementRow(r).Id), "fresh ids el_1..el_17");
            Assert.IsFalse(Page(block).Any(HoldsContent));
            // - copies, not the template's own rows: a look changed on the block stays the block's
            var template = Short;
            var made = BlockTemplates.NewBlock(camera.card, template, Elements);
            new ElementRow(Page(made)[0]).SetValue(BuiltInElements.ToneStyle, BuiltInElements.ToneAccent);
            Assert.AreEqual(BuiltInElements.ToneNeutral, new ElementRow(BlockTemplates.RowsOf(template)[0]).Value(BuiltInElements.ToneStyle));
        }

        [Test]
        public void Reapply_AddsExactlyTheMissingRows_InTheirTemplatePlace_NeverTouchingTheRowsTheBlockHas()
        {
            var rows = Page(_story);
            // - the developer deleted the eyebrow (2nd), the second heading's paragraph... and the video (last), added a spacer of their own
            //   and rewrote the title
            string Id(int at) => new ElementRow(rows[at]).Id;
            var gone = new[] { Id(1), Id(11), Id(16) };
            rows.RemoveAll(r => gone.Contains(new ElementRow(r).Id));
            ElementEdits.Add(_story, rows, Elements.All.Single(e => e.Key == BuiltInElements.Spacer));
            var title = new ElementRow(rows.Single(r => new ElementRow(r).Kind == BuiltInElements.Title));
            title.Field(BuiltInElements.TextField).text[0].value = "My own title";
            var before = rows.Select(r => JsonUtility.ToJson(r)).ToList();
            var idsBefore = rows.Select(r => new ElementRow(r).Id).ToList();

            var added = BlockTemplates.Reapply(_story, _kind, Magazine, Elements);
            CollectionAssert.AreEqual(new[] { BuiltInElements.Eyebrow, BuiltInElements.Paragraph, BuiltInElements.Video }, added.Select(r => r.Kind),
                "exactly the three rows it lacks, in template order");
            var after = Page(_story);
            Assert.AreEqual(before.Count + 3, after.Count);
            // - every row it had is untouched, its own spacer and its words included, in the same order
            CollectionAssert.AreEqual(before, after.Where(r => idsBefore.Contains(new ElementRow(r).Id)).Select(r => JsonUtility.ToJson(r)));
            // - in their template place: the eyebrow between the picture and the title, the paragraph after the third heading, the video last
            var kinds = Kinds(after);
            Assert.AreEqual(BuiltInElements.Eyebrow, kinds[1]);
            Assert.AreEqual(BuiltInElements.Title, kinds[2]);
            int secondHeadingIndex = kinds.Select((k, i) => (k, i)).Where(p => p.k == BuiltInElements.Heading).ElementAt(1).i;
            Assert.AreEqual(BuiltInElements.Paragraph, kinds[secondHeadingIndex + 1], "the paragraph back under its heading");
            Assert.AreEqual(BuiltInElements.Video, kinds[kinds.Count - 2], "the video at the template's end, before the developer's own spacer");
            Assert.AreEqual(BuiltInElements.Spacer, kinds.Last());
            // - fresh ids, past every id the block ever handed out; empty, with the template's looks
            foreach (var row in added)
            {
                Assert.IsFalse(idsBefore.Contains(row.Id) || gone.Contains(row.Id), row.Id + " is a fresh id");
                Assert.IsFalse(HoldsContent(row.Data), row.Kind + " arrives empty");
            }
            StringAssert.StartsWith("Added 3 rows from \"Story -- magazine\": Eyebrow, Paragraph, Video.",
                BlockTemplates.ReapplyMessage("Story -- magazine", added, Elements));

            // - again: nothing left to add
            var none = BlockTemplates.Reapply(_story, _kind, Magazine, Elements);
            Assert.IsEmpty(none);
            StringAssert.StartsWith("Nothing to add", BlockTemplates.ReapplyMessage("Story -- magazine", none, Elements));
        }

        [Test]
        public void Reapply_OnRowsInAnotherOrder_KeepsThemWhereTheyAre_AndAddsWhatTheLongestMatchingRunLacks()
        {
            var camera = _config.pois.Single(p => p.id == "camera");
            camera.card ??= new POICardData();
            var block = BlockTemplates.NewBlock(camera.card, Short, Elements);
            var rows = Page(block);
            // - the developer moved the picture first and deleted the lede: picture, title, paragraph
            ElementEdits.Move(rows, 3, 0);
            rows.RemoveAt(2);
            CollectionAssert.AreEqual(new[] { BuiltInElements.Picture, BuiltInElements.Title, BuiltInElements.Paragraph }, Kinds(rows));
            var added = BlockTemplates.Reapply(block, _kind, Short, Elements);
            // - the longest run in the template's order is title, paragraph: the lede goes between them, the picture is added at the end
            //   (structure, not ids: the moved picture is out of the template's order)
            CollectionAssert.AreEqual(new[] { BuiltInElements.Lede, BuiltInElements.Picture }, added.Select(r => r.Kind));
            CollectionAssert.AreEqual(new[] { BuiltInElements.Picture, BuiltInElements.Title, BuiltInElements.Lede, BuiltInElements.Paragraph, BuiltInElements.Picture },
                Kinds(Page(block)));
        }

        [Test]
        public void TemplateKeys_AreNeverHandedOutWhileABlockNamesThem_DuplicateCopies_AndABuiltInCannotBeRemoved()
        {
            var saved = BlockTemplates.SaveAs(_config.card_settings, _config.pois, _story, _kind, "Mine", Elements);
            Assert.AreEqual("template_1", saved.key);
            Assert.IsTrue(BlockTemplates.Remove(_config.card_settings, saved.key));
            Assert.IsEmpty(_config.card_settings.block_templates);
            Assert.AreEqual("template_2", BlockTemplates.NextKey(_config.card_settings, _config.pois),
                "the story still names template_1: a new template never takes its key");
            BlockSurfaces.SetTemplate(_story, "");
            Assert.AreEqual("template_1", BlockTemplates.NextKey(_config.card_settings, _config.pois), "named by nothing, the key is free again");

            var copy = BlockTemplates.Duplicate(_config.card_settings, _config.pois, Magazine, Elements);
            Assert.AreEqual("Story -- magazine (copy)", copy.name);
            Assert.IsFalse(BlockTemplates.IsBuiltIn(copy.key));
            CollectionAssert.AreEqual(BlockTemplates.RowsOf(Magazine).Select(Structure), BlockTemplates.RowsOf(copy).Select(Structure));
            Assert.AreSame(copy, BlockTemplates.Find(_config.card_settings, Elements, copy.key));
            Assert.IsFalse(BlockTemplates.Remove(_config.card_settings, BlockTemplates.MagazineKey), "a built-in is code: it cannot be deleted");
            Assert.IsNotNull(BlockTemplates.Find(_config.card_settings, Elements, BlockTemplates.MagazineKey));
        }
    }
}
