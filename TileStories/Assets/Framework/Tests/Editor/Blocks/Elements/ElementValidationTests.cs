using System.Linq;
using NUnit.Framework;

namespace TileStories.Editor.Tests
{
    // ElementValidation on The Painting's story (_3.3 R14.1, R17.8): the story as built has no problem; each broken thing is reported once,
    // naming its ROW -- surface, number, element name, the field -- so the Editor can word it without an id; ids are unique across the Page
    // AND the Preview; and an id problem never hides content (RowIsComplete is what the card shows, not what the Editor tidies).
    public class ElementValidationTests
    {
        private PaintingStory _s;

        [SetUp]
        public void BuildTheStory() => _s = PaintingStory.Build();

        private ElementValidation.Problem Single() => ElementValidation.Of(_s.Scope, _s.Elements).Single();

        [Test]
        public void ThePaintingsStory_AsBuilt_HasNoProblem()
        {
            CollectionAssert.IsEmpty(ElementValidation.Of(_s.Scope, _s.Elements).Select(p => p.Surface + " " + p.Row + " " + p.Kind));
        }

        [Test]
        public void AnUnknownElement_IsNamedByItsRow()
        {
            new ElementRow(_s.Eyebrow.Data).SetValue(ElementRow.ElementKey, "headline");
            var p = Single();
            Assert.AreEqual(ElementValidation.Kind.UnknownElement, p.Kind);
            Assert.AreEqual((ElementValidation.Surface.Page, 2, _s.Eyebrow.Id, ""), (p.Surface, p.Row, p.RowId, p.ElementName));
        }

        [Test]
        public void ARequiredFieldWithNoWords_NamesTheElementAndTheField()
        {
            foreach (var entry in _s.Paragraph.Field(BuiltInElements.TextField).text) entry.value = "  ";
            var p = Single();
            Assert.AreEqual(ElementValidation.Kind.MissingRequired, p.Kind);
            Assert.AreEqual((5, "Paragraph", BuiltInElements.TextField, "Text"), (p.Row, p.ElementName, p.FieldKey, p.FieldLabel));
            // - words in ONE language are words: the card falls back to them (BlockFieldReader.Pick)
            _s.Paragraph.Field(BuiltInElements.TextField).text[1].value = "Uma frase.";
            CollectionAssert.IsEmpty(ElementValidation.Of(_s.Scope, _s.Elements));
        }

        [Test]
        public void AMediaListOutsideTheElementsCount_AndAFileTheMediaRuleRefuses_AreNamed()
        {
            // - a carousel of ONE picture (it holds 2 to 12), a picture whose file is a .gif (the picture rule takes png / jpg / jpeg)
            var kept = _s.Carousel.Media[0].Data;
            _s.Carousel.Data.fields.RemoveAll(f => f.key == ElementRow.MediaKey && f != kept);
            _s.Picture.Media[0].Data.asset = "images/paco_hero.gif";
            var problems = ElementValidation.Of(_s.Scope, _s.Elements);
            var count = problems.Single(p => p.Kind == ElementValidation.Kind.MediaCount);
            Assert.AreEqual((6, "Carousel", 1), (count.Row, count.ElementName, count.Media));
            var file = problems.Single(p => p.Kind == ElementValidation.Kind.InvalidMedia);
            Assert.AreEqual((1, "Picture", 1), (file.Row, file.ElementName, file.Media));
            Assert.AreEqual(2, problems.Count);
        }

        [Test]
        public void ABrokenLink_IsNamedWithItsReason_OnItsSurface()
        {
            _s.AddPreview(ElementRow.CreateRef(_s.Block, ElementRef.ToPage(_s.Title.Id)));
            _s.AddPreview(ElementRow.CreateRef(_s.Block, ElementRef.ToBlock("block_404", "el_1")));
            var p = Single();
            Assert.AreEqual(ElementValidation.Kind.BrokenRef, p.Kind);
            Assert.AreEqual((ElementValidation.Surface.Preview, 2, ElementRefRule.Problem.MissingBlock), (p.Surface, p.Row, p.RefProblem));
        }

        [Test]
        public void IdsAreUniqueAcrossPageAndPreview_ABlankOrRepeatedOneIsNamed_AndNeverHidesContent()
        {
            // - a Preview row holding the Title's id (a hand-edited file), and a Page row with no id
            var copy = ElementRow.CreateRef(_s.Block, ElementRef.ToPage(_s.Lede.Id));
            copy.SetValue(ElementRow.IdKey, _s.Title.Id);
            _s.AddPreview(copy);
            _s.Divider.SetValue(ElementRow.IdKey, "");
            var problems = ElementValidation.Of(_s.Scope, _s.Elements);
            var repeated = problems.Single(p => p.Kind == ElementValidation.Kind.DuplicateId);
            Assert.AreEqual((ElementValidation.Surface.Preview, 1, _s.Title.Id), (repeated.Surface, repeated.Row, repeated.RowId), "the LATER holder of the id is named");
            var blank = problems.Single(p => p.Kind == ElementValidation.Kind.MissingId);
            Assert.AreEqual((ElementValidation.Surface.Page, 7, "Divider"), (blank.Surface, blank.Row, blank.ElementName));
            Assert.AreEqual(2, problems.Count);
            Assert.IsTrue(ElementValidation.RowIsComplete(_s.Divider.Data, _s.Scope, _s.Elements), "an id problem does not keep a row off the card");
            Assert.IsTrue(ElementValidation.RowIsComplete(copy.Data, _s.Scope, _s.Elements));
        }
    }
}
