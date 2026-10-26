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
        public void Automatic_TakesTheFirstPicture_Title_AndText_InPageOrder_TheEyebrowWithItsTitle()
        {
            var preview = Preview();
            Assert.AreEqual(ElementOptions.PreviewAutomatic, preview.Mode, "a block that stores no mode is Automatic");
            // - the eyebrow sits right before the title: it leads into it (the prototype's preview: picture, eyebrow, title, lede)
            CollectionAssert.AreEqual(new[] { _s.Picture.Id, _s.Eyebrow.Id, _s.Title.Id, _s.Lede.Id }, preview.Entries.Select(e => e.PageRowId));
            CollectionAssert.AreEqual(new[] { _s.Picture.Id, _s.Eyebrow.Id, _s.Title.Id, _s.Lede.Id }, preview.UsedPageRows);
            Assert.IsTrue(preview.Entries.All(e => e.Source == ElementPreviewRule.Source.Page && e.Content == e.Row));
            // - the later paragraph, the carousel and the divider are not the preview's
            CollectionAssert.DoesNotContain(preview.UsedPageRows, _s.Paragraph.Id);
            CollectionAssert.DoesNotContain(preview.UsedPageRows, _s.Carousel.Id);
        }

        // A title lead joins only the title it touches: moved away from the title, or with no title at all, it is not the preview's; a
        // second eyebrow before a later title the preview does not take joins nothing
        [Test]
        public void Automatic_AnEyebrowJoins_OnlyRightBeforeTheTitleTaken()
        {
            var rows = _s.PageValue.items;
            // - the eyebrow moved after the lede: the title has no lead now
            rows.Remove(_s.Eyebrow.Data);
            rows.Insert(rows.IndexOf(_s.Lede.Data) + 1, _s.Eyebrow.Data);
            CollectionAssert.AreEqual(new[] { _s.Picture.Id, _s.Title.Id, _s.Lede.Id }, Preview().UsedPageRows, "an eyebrow away from the title stays out");

            // - eyebrow, lede, title: a row between them, so the title the preview takes has no lead
            rows.Remove(_s.Eyebrow.Data);
            rows.Insert(rows.IndexOf(_s.Title.Data), _s.Eyebrow.Data);
            rows.Remove(_s.Title.Data);
            rows.Insert(rows.IndexOf(_s.Lede.Data) + 1, _s.Title.Data);
            CollectionAssert.AreEqual(new[] { _s.Picture.Id, _s.Lede.Id, _s.Title.Id }, Preview().UsedPageRows, "a row between the eyebrow and the title parts them");
            rows.Remove(_s.Title.Data);
            rows.Insert(rows.IndexOf(_s.Lede.Data), _s.Title.Data);

            // - back before the title, and a heading with its own eyebrow later: only the first title's lead comes
            rows.Remove(_s.Eyebrow.Data);
            rows.Insert(rows.IndexOf(_s.Title.Data), _s.Eyebrow.Data);
            var secondEyebrow = _s.AddPage(BuiltInElements.Eyebrow);
            var heading = _s.AddPage(BuiltInElements.Heading);
            CollectionAssert.AreEqual(new[] { _s.Picture.Id, _s.Eyebrow.Id, _s.Title.Id, _s.Lede.Id }, Preview().UsedPageRows);
            CollectionAssert.DoesNotContain(Preview().UsedPageRows, secondEyebrow.Id);
            CollectionAssert.DoesNotContain(Preview().UsedPageRows, heading.Id);

            // - no title at all: the eyebrow leads nowhere
            rows.Remove(_s.Title.Data);
            rows.Remove(heading.Data);
            CollectionAssert.AreEqual(new[] { _s.Picture.Id, _s.Lede.Id }, Preview().UsedPageRows, "an eyebrow alone is never the preview");
        }

        [Test]
        public void Automatic_FollowsEveryPageEdit_AReorderAndADelete()
        {
            // - the carousel moved above the picture: it is now the first picture-like row, and the preview's picture
            var rows = _s.PageValue.items;
            rows.Remove(_s.Carousel.Data);
            rows.Insert(0, _s.Carousel.Data);
            CollectionAssert.AreEqual(new[] { _s.Carousel.Id, _s.Eyebrow.Id, _s.Title.Id, _s.Lede.Id }, Preview().UsedPageRows);

            // - the lede deleted: the paragraph is the first text now; both pictures deleted: no picture
            rows.Remove(_s.Lede.Data);
            rows.Remove(_s.Carousel.Data);
            rows.Remove(_s.Picture.Data);
            CollectionAssert.AreEqual(new[] { _s.Eyebrow.Id, _s.Title.Id, _s.Paragraph.Id }, Preview().UsedPageRows);

            // - a heading above everything: the first TITLE-LIKE row in Page order is the heading, and the eyebrow (before the title the
            //   preview no longer takes) stays out
            var heading = _s.AddPage(BuiltInElements.Heading);
            rows.Remove(heading.Data);
            rows.Insert(0, heading.Data);
            CollectionAssert.AreEqual(new[] { heading.Id, _s.Paragraph.Id }, Preview().UsedPageRows);
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
            Assert.AreEqual(4, Preview().Entries.Count, "picture, eyebrow, title, lede");
            Assert.AreEqual(1, BlockSurfaces.PreviewRows(_s.Block).Count, "the Custom rows wait, stored, for the next switch");
            mode.value = "carousel_only";
            Assert.AreEqual(ElementOptions.PreviewAutomatic, Preview().Mode);
        }

        private static ElementKindDefinition Element(string key) => ElementRegistry.Shared.TryGet(key, out var e) ? e : null;
    }
}
