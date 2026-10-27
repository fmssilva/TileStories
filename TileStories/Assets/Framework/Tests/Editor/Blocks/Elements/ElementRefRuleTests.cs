using System.Collections.Generic;
using NUnit.Framework;

namespace TileStories.Editor.Tests
{
    // ElementRefRule on The Painting's story (_3.3 R17.8): a reference resolves in each of its three scopes -- this block's Page, another
    // block of the same POI (found through its kind in the real registry), the POI's shared pool -- one level only (a reference to a
    // reference is invalid in every scope), and every way of pointing at nothing is Broken with its own reason.
    public class ElementRefRuleTests
    {
        private PaintingStory _s;

        [SetUp]
        public void BuildTheStory() => _s = PaintingStory.Build();

        [Test]
        public void APageReference_ResolvesToThatRowOfThisBlock()
        {
            var result = ElementRefRule.Resolve(ElementRef.ToPage(_s.Title.Id), _s.Scope);
            Assert.IsFalse(result.IsBroken);
            Assert.AreSame(_s.Title.Data, result.Target);
        }

        [Test]
        public void ABlockReference_ResolvesToThatRowOfTheOtherBlocksPage_ThroughItsKind()
        {
            // - the second block's row is el_1 too: an id is unique in its block, so the block key is what tells them apart
            Assert.AreEqual(_s.Picture.Id, _s.SecondHeading.Id, "precondition: both blocks hold an el_1");
            var result = ElementRefRule.Resolve(ElementRef.ToBlock(PaintingStory.SecondBlockKey, _s.SecondHeading.Id), _s.Scope);
            Assert.IsFalse(result.IsBroken);
            Assert.AreSame(_s.SecondHeading.Data, result.Target);
        }

        [Test]
        public void ASharedReference_ResolvesInTheSharedPoolPassedIn_AndIsMissingWhileThePoiHasNone()
        {
            var pooled = ElementRow.Create(new BlockInstanceData { key = "pool" }, Element(BuiltInElements.PullQuote));
            var scope = new ElementScope(_s.Block, _s.Kind, _s.Painting.card.blocks, _s.Blocks, new List<BlockItemData> { pooled.Data });
            Assert.AreSame(pooled.Data, ElementRefRule.Resolve(ElementRef.ToShared(pooled.Id), scope).Target);
            // - The Painting stores no shared elements (phase 2): ElementScope.Of brings an empty pool
            Assert.AreEqual(ElementRefRule.Problem.MissingRow, ElementRefRule.Resolve(ElementRef.ToShared(pooled.Id), _s.Scope).Problem);
        }

        [Test]
        public void AReferenceToAReference_IsBroken_InEveryScope()
        {
            // - a Page link row, a link row in the second block's Page, a link row in the shared pool: each a valid target's id
            var pageLink = _s.AddPage(BuiltInElements.Spacer);
            pageLink.SetRef(ElementRef.ToPage(_s.Title.Id));
            var otherLink = PaintingStory.AddRow(_s.Second, _s.Elements, BuiltInElements.Spacer);
            otherLink.SetRef(ElementRef.ToPage(_s.SecondHeading.Id));
            var pooledLink = ElementRow.CreateRef(new BlockInstanceData { key = "pool" }, ElementRef.ToPage("el_1"));
            var scope = new ElementScope(_s.Block, _s.Kind, _s.Painting.card.blocks, _s.Blocks, new List<BlockItemData> { pooledLink.Data });

            Assert.AreEqual(ElementRefRule.Problem.RefToRef, ElementRefRule.Resolve(ElementRef.ToPage(pageLink.Id), scope).Problem);
            Assert.AreEqual(ElementRefRule.Problem.RefToRef, ElementRefRule.Resolve(ElementRef.ToBlock(PaintingStory.SecondBlockKey, otherLink.Id), scope).Problem);
            Assert.AreEqual(ElementRefRule.Problem.RefToRef, ElementRefRule.Resolve(ElementRef.ToShared(pooledLink.Id), scope).Problem);
            Assert.IsNull(ElementRefRule.Resolve(ElementRef.ToPage(pageLink.Id), scope).Target, "a broken reference shows nothing");
        }

        [Test]
        public void PointingAtNothing_IsBroken_WithItsOwnReason()
        {
            Assert.AreEqual(ElementRefRule.Problem.MissingRow, ElementRefRule.Resolve(ElementRef.ToPage("el_99"), _s.Scope).Problem, "no such row");
            Assert.AreEqual(ElementRefRule.Problem.MissingBlock, ElementRefRule.Resolve(ElementRef.ToBlock("block_404", "el_1"), _s.Scope).Problem, "no such block");
            // - The Painting's shipped gallery exists but composes nothing: it has no Page to point into
            Assert.AreEqual(ElementRefRule.Problem.NoPage, ElementRefRule.Resolve(ElementRef.ToBlock(PaintingStory.GalleryKey, "el_1"), _s.Scope).Problem, "a block with no Page");
            Assert.AreEqual(ElementRefRule.Problem.UnknownScope, ElementRefRule.Resolve(new ElementRef("elsewhere", "", _s.Title.Id), _s.Scope).Problem);
            Assert.AreEqual(ElementRefRule.Problem.MissingRow, ElementRefRule.Resolve(ElementRef.ToPage(""), _s.Scope).Problem, "a blank row id");
            // - a deleted row: the link that named it breaks; it never jumps to a neighbour
            string titleId = _s.Title.Id;
            _s.PageValue.items.Remove(_s.Title.Data);
            Assert.AreEqual(ElementRefRule.Problem.MissingRow, ElementRefRule.Resolve(ElementRef.ToPage(titleId), _s.Scope).Problem, "the row was deleted");
        }

        [Test]
        public void ContentOf_IsTheRowItselfForAnElement_AndTheTargetForALink()
        {
            Assert.AreSame(_s.Lede.Data, ElementRefRule.ContentOf(_s.Lede.Data, _s.Scope).Target);
            var link = ElementRow.CreateRef(_s.Block, ElementRef.ToPage(_s.Lede.Id));
            Assert.AreSame(_s.Lede.Data, ElementRefRule.ContentOf(link.Data, _s.Scope).Target);
            Assert.AreSame(_s.Kind, _s.Scope.HolderKind, "ElementScope.Of found the holder's kind in the registry");
        }

        private static ElementKindDefinition Element(string key) => ElementRegistry.Shared.TryGet(key, out var e) ? e : null;
    }
}
