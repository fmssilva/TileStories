using System.Linq;
using NUnit.Framework;

namespace TileStories.Editor.Tests
{
    // ElementPreviewRule on The Painting's story (_3.3 R14.3, R17.1, R17.7). Automatic: the first picture-like, title-like and text row in
    // PAGE order, recomputed by every Page edit (a reorder or a delete moves the preview with it); a Page link plays its target's role, a
    // broken one or an unknown element none. Custom: the rows as listed, links resolved, a broken link kept as Broken. UsedPageRows is
    // exactly the Page rows the preview shows (the Editor's "P" marks).
    public class ElementPreviewRuleTests
    {
        private PaintingStory _s;

        [SetUp]
        public void BuildTheStory() => _s = PaintingStory.Build();

        private ElementPreviewRule.Result Preview() => ElementPreviewRule.Of(_s.Scope, _s.Elements);

        [Test]
        public void Automatic_TakesTheFirstPicture_Title_AndText_InPageOrder()
        {
            var preview = Preview();
            Assert.AreEqual(ElementOptions.PreviewAutomatic, preview.Mode, "a block that stores no mode is Automatic");
            CollectionAssert.AreEqual(new[] { _s.Picture.Id, _s.Title.Id, _s.Lede.Id }, preview.Entries.Select(e => e.PageRowId));
            CollectionAssert.AreEqual(new[] { _s.Picture.Id, _s.Title.Id, _s.Lede.Id }, preview.UsedPageRows);
            Assert.IsTrue(preview.Entries.All(e => e.Source == ElementPreviewRule.Source.Page && e.Content == e.Row));
            // - the eyebrow, the later paragraph, the carousel and the divider are not the preview's
            CollectionAssert.DoesNotContain(preview.UsedPageRows, _s.Eyebrow.Id);
            CollectionAssert.DoesNotContain(preview.UsedPageRows, _s.Paragraph.Id);
            CollectionAssert.DoesNotContain(preview.UsedPageRows, _s.Carousel.Id);
        }

        [Test]
        public void Automatic_FollowsEveryPageEdit_AReorderAndADelete()
        {
            // - the carousel moved above the picture: it is now the first picture-like row, and the preview's picture
            var rows = _s.PageValue.items;
            rows.Remove(_s.Carousel.Data);
            rows.Insert(0, _s.Carousel.Data);
            CollectionAssert.AreEqual(new[] { _s.Carousel.Id, _s.Title.Id, _s.Lede.Id }, Preview().UsedPageRows);

            // - the lede deleted: the paragraph is the first text now; both pictures deleted: no picture, two rows
            rows.Remove(_s.Lede.Data);
            rows.Remove(_s.Carousel.Data);
            rows.Remove(_s.Picture.Data);
            CollectionAssert.AreEqual(new[] { _s.Title.Id, _s.Paragraph.Id }, Preview().UsedPageRows);

            // - a heading above the title: the first TITLE-LIKE row in Page order is the heading
            var heading = _s.AddPage(BuiltInElements.Heading);
            rows.Remove(heading.Data);
            rows.Insert(0, heading.Data);
            Assert.AreEqual(heading.Id, Preview().UsedPageRows[0]);
        }

        [Test]
        public void Automatic_APageLinkPlaysItsTargetsRole_ABrokenOneOrAnUnknownElementNone()
        {
            // - the title replaced by a link to the second block's heading: the link row is the preview's title, showing the heading
            var rows = _s.PageValue.items;
            int at = rows.IndexOf(_s.Title.Data);
            var link = ElementRow.CreateRef(_s.Block, ElementRef.ToBlock(PaintingStory.SecondBlockKey, _s.SecondHeading.Id));
            rows[at] = link.Data;
            var preview = Preview();
            var title = preview.Entries.Single(e => e.PageRowId == link.Id);
            Assert.AreSame(link.Data, title.Row);
            Assert.AreSame(_s.SecondHeading.Data, title.Content);

            // - broken: the link names a row that is gone; an unknown element: a row of a kind nobody registered -> neither plays a role
            link.SetRef(ElementRef.ToPage("el_99"));
            new ElementRow(_s.Picture.Data).SetValue(ElementRow.ElementKey, "hologram");
            CollectionAssert.AreEqual(new[] { _s.Lede.Id, _s.Carousel.Id }, Preview().UsedPageRows, "in Page order: the lede is row 4, the carousel row 6");
        }

        [Test]
        public void Custom_ShowsTheRowsAsListed_LinksResolved_ABrokenLinkKeptAsBroken()
        {
            PaintingStory.Value(_s.Block, BlockSurfaces.PreviewModeField).value = ElementOptions.PreviewCustom;
            var teaser = ElementRow.Create(_s.Block, Element(BuiltInElements.Lede));
            var toTitle = ElementRow.CreateRef(_s.Block, ElementRef.ToPage(_s.Title.Id));
            var gone = ElementRow.CreateRef(_s.Block, ElementRef.ToPage("el_99"));
            var toOther = ElementRow.CreateRef(_s.Block, ElementRef.ToBlock(PaintingStory.SecondBlockKey, _s.SecondHeading.Id));
            _s.AddPreview(teaser);
            _s.AddPreview(toTitle);
            _s.AddPreview(gone);
            _s.AddPreview(toOther);

            var preview = Preview();
            Assert.AreEqual(ElementOptions.PreviewCustom, preview.Mode);
            CollectionAssert.AreEqual(new[] { ElementPreviewRule.Source.Own, ElementPreviewRule.Source.Linked, ElementPreviewRule.Source.Broken, ElementPreviewRule.Source.Linked },
                preview.Entries.Select(e => e.Source));
            Assert.AreSame(teaser.Data, preview.Entries[0].Content);
            Assert.AreSame(_s.Title.Data, preview.Entries[1].Content, "a link shows its Page row's content");
            Assert.AreSame(toTitle.Data, preview.Entries[1].Row, "and keeps its own row (its own style and motion)");
            Assert.IsNull(preview.Entries[2].Content);
            Assert.AreEqual(ElementRefRule.Problem.MissingRow, preview.Entries[2].Problem);
            Assert.AreSame(_s.SecondHeading.Data, preview.Entries[3].Content);
            // - only a link into THIS Page marks a Page row
            CollectionAssert.AreEqual(new[] { _s.Title.Id }, preview.UsedPageRows);
        }

        [Test]
        public void SwitchingBackToAutomatic_KeepsTheCustomRows_AndAnUnknownModeReadsAutomatic()
        {
            var mode = PaintingStory.Value(_s.Block, BlockSurfaces.PreviewModeField);
            mode.value = ElementOptions.PreviewCustom;
            _s.AddPreview(ElementRow.CreateRef(_s.Block, ElementRef.ToPage(_s.Title.Id)));
            mode.value = ElementOptions.PreviewAutomatic;
            Assert.AreEqual(3, Preview().Entries.Count);
            Assert.AreEqual(1, BlockSurfaces.PreviewRows(_s.Block).Count, "the Custom rows wait, stored, for the next switch");
            mode.value = "carousel_only";
            Assert.AreEqual(ElementOptions.PreviewAutomatic, Preview().Mode);
        }

        private static ElementKindDefinition Element(string key) => ElementRegistry.Shared.TryGet(key, out var e) ? e : null;
    }
}
