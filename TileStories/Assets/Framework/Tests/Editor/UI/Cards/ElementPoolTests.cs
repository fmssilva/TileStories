using System.Collections.Generic;
using NUnit.Framework;

namespace TileStories.Editor.Tests
{
    // ElementPool (_3.1 15.4.1): the one row pool every card view uses. Pure, so plain objects stand in for rows; what a view gets from it
    // on the card (the same rows reused, none left behind) is proven on the real views by CardGalleryTests and PoiCardSceneTests.
    public class ElementPoolTests
    {
        private sealed class Row
        {
            public int Place;
            public bool OnCard;
        }

        private readonly List<int> _made = new();
        private readonly List<Row> _released = new();

        private ElementPool<Row> NewPool() => new ElementPool<Row>(
            place => { _made.Add(place); return new Row { Place = place }; },
            row => { row.OnCard = false; _released.Add(row); });

        [SetUp]
        public void SetUp()
        {
            _made.Clear();
            _released.Clear();
        }

        [Test]
        public void Take_HandsOutRowsInOrder_EachMadeOnceForItsPlace()
        {
            var pool = NewPool();
            var a = pool.Take();
            var b = pool.Take();
            var c = pool.Take();
            Assert.AreEqual(new[] { 0, 1, 2 }, new[] { a.Place, b.Place, c.Place }, "each row knows its place");
            CollectionAssert.AreEqual(new[] { 0, 1, 2 }, _made, "one create per place, in order");
            CollectionAssert.AreEqual(new[] { a, b, c }, pool.Shown);
            Assert.AreEqual(3, pool.Created);
        }

        [Test]
        public void AfterReleaseAll_TheNextBindGetsTheSameRows_AndMakesOnlyWhatItNeedsBeyondThem()
        {
            var pool = NewPool();
            var first = new[] { pool.Take(), pool.Take() };
            pool.ReleaseAll();
            Assert.AreEqual(0, pool.Shown.Count, "nothing shown after a release");

            var again = new[] { pool.Take(), pool.Take(), pool.Take() };
            Assert.AreSame(first[0], again[0], "place 0 is the same object");
            Assert.AreSame(first[1], again[1], "place 1 too");
            Assert.AreEqual(2, again[2].Place, "a third row is made only now");
            CollectionAssert.AreEqual(new[] { 0, 1, 2 }, _made, "never a second object for a place");
        }

        [Test]
        public void ReleaseAll_ReleasesExactlyTheShownRows_InOrder_NotTheRestOfThePool()
        {
            var pool = NewPool();
            for (int i = 0; i < 3; i++) pool.Take().OnCard = true;
            pool.ReleaseAll();
            _released.Clear();

            var shown = pool.Take();
            shown.OnCard = true;
            pool.ReleaseAll();
            CollectionAssert.AreEqual(new[] { shown }, _released, "a bind that showed one row releases that one, not the two resting in the pool");
            Assert.IsFalse(shown.OnCard);
        }

        [Test]
        public void AFewerRowsBind_LeavesTheExtraRowsResting_AndAnEmptyBindMakesNothing()
        {
            var pool = NewPool();
            pool.Take();
            pool.Take();
            pool.ReleaseAll();
            pool.Take();
            Assert.AreEqual(1, pool.Shown.Count);
            Assert.AreEqual(2, pool.Created, "the second row waits for a later bind, never made again");
            pool.ReleaseAll();
            pool.ReleaseAll();
            Assert.AreEqual(0, pool.Shown.Count, "a second release is harmless");
            Assert.AreEqual(2, _made.Count);
        }

        [Test]
        public void ANullCreateOrRelease_IsRefused()
        {
            Assert.Throws<System.ArgumentNullException>(() => new ElementPool<Row>(null, _ => { }));
            Assert.Throws<System.ArgumentNullException>(() => new ElementPool<Row>(_ => new Row(), null));
        }
    }
}
