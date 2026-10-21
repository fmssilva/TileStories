using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace TileStories.Editor.Tests
{
    // The bento layout's tile arithmetic (_3.2 section 6.1, step 15b), pure: for every section count from 1 to 10 the tiles are one per
    // section in order, never under their minimum (the target wins), fill each row's width and -- while the rows' minimums fit -- the
    // card's height exactly; the lead (most targets) has a row of its own at the full width; a tile left alone spans its row (the leftover
    // row); the area follows the counts wherever no minimum holds; past the fill point every row sits at its minimum and the overview
    // scrolls. The numbers are a phone's at 390 px: the content 358 px wide, The Lamp's Full viewport 565 px, the tile minimum 64 px tall,
    // an 8 px gap (the view reads the real ones from the card; PoiCardBentoLayoutSceneTests measures them)
    public class BentoLayoutRuleTests
    {
        private const float Width = 358f;
        private const float FullHeight = 565f;
        private const float MinHeight = 64f;
        private const float Gap = 8f;
        private const float Touch = 48f;
        // - a pixel's float noise
        private const float Epsilon = 0.01f;

        // The Lamp's eight section counts (by family) and two more, so every count from 1 to 10 has uneven sections
        private static readonly int[] Counts = { 18, 14, 1, 7, 3, 4, 7, 2, 5, 9 };
        // A word's minimum: a short word and a long Portuguese one ("Comunidade") alternate
        private static readonly float[] MinWidths = { 110f, 96f, 150f, 104f, 120f, 96f, 140f, 150f, 100f, 112f };

        // A tile whose content is taller than any card (a section with many names): the cap never binds, the share arithmetic is what is tested
        private static float TallContent(int section, float width) => 10000f;

        private static BentoLayoutRule.Result LayoutOf(int n, float height = FullHeight) =>
            BentoLayoutRule.Layout(Counts.Take(n).ToArray(), MinWidths.Take(n).ToArray(), MinHeight, Width, height, Gap, TallContent);

        [Test]
        public void EverySectionCountFrom1To10_GivesOneTilePerSection_NeverUnderItsMinimum_FillingEachRowAndTheCard()
        {
            for (int n = 1; n <= 10; n++)
            {
                var layout = LayoutOf(n);
                Assert.AreEqual(n, layout.Tiles.Count, n + ": one tile per section");
                for (int i = 0; i < n; i++)
                {
                    var tile = layout.Tiles[i];
                    Assert.AreEqual(i, tile.Section, n + ": in the sections' order");
                    Assert.GreaterOrEqual(tile.Width, MinWidths[i] - Epsilon, n + "/" + i + ": never narrower than its words");
                    Assert.GreaterOrEqual(tile.Height, MinHeight - Epsilon, n + "/" + i + ": never lower than the minimum");
                    Assert.GreaterOrEqual(System.Math.Min(tile.Width, tile.Height), Touch, n + "/" + i + ": a whole touch target");
                    Assert.GreaterOrEqual(tile.X, -Epsilon);
                    Assert.LessOrEqual(tile.X + tile.Width, Width + Epsilon, n + "/" + i + ": inside the card");
                }
                foreach (var row in layout.Rows)
                {
                    var tiles = row.Select(s => layout.Tiles[s]).ToList();
                    Assert.AreEqual(Width, tiles.Sum(t => t.Width) + Gap * (tiles.Count - 1), Epsilon, n + ": a row fills the width");
                    Assert.IsTrue(tiles.All(t => System.Math.Abs(t.Y - tiles[0].Y) < Epsilon && System.Math.Abs(t.Height - tiles[0].Height) < Epsilon),
                        n + ": a row's tiles share their top and height");
                    for (int k = 1; k < tiles.Count; k++)
                        Assert.AreEqual(tiles[k - 1].X + tiles[k - 1].Width + Gap, tiles[k].X, Epsilon, n + ": side by side, one gap apart");
                }
                for (int r = 1; r < layout.Rows.Count; r++)
                {
                    var above = layout.Tiles[layout.Rows[r - 1][0]];
                    Assert.AreEqual(above.Y + above.Height + Gap, layout.Tiles[layout.Rows[r][0]].Y, Epsilon, n + ": rows one gap apart, no overlap");
                }
                Assert.IsTrue(layout.Fills, n + ": ten sections or fewer still fill a Full card");
                Assert.AreEqual(FullHeight, layout.Height, Epsilon, n + ": the tiles fill the card's height exactly");
            }
        }

        // The focal point: the section with the most targets has a row of its own at the full width, the tallest row of the card; the others
        // two to a row in order; a tile left alone (the last of an odd number, or one before the lead) spans its row
        [Test]
        public void TheLeadHasARowOfItsOwn_TheOthersPairInOrder_AndATileLeftAloneSpansItsRow()
        {
            var lamp = LayoutOf(8);
            Assert.AreEqual(0, BentoLayoutRule.Lead(Counts.Take(8).ToArray()), "About, 18 blocks");
            CollectionAssert.AreEqual(new[] { new[] { 0 }, new[] { 1, 2 }, new[] { 3, 4 }, new[] { 5, 6 }, new[] { 7 } },
                lamp.Rows.Select(r => r.ToArray()).ToArray(), "The Lamp: About alone, three pairs, Community left over");
            Assert.AreEqual(Width, lamp.Tiles[0].Width, Epsilon, "the lead spans the card");
            Assert.AreEqual(Width, lamp.Tiles[7].Width, Epsilon, "the leftover row's tile spans it too");
            Assert.IsTrue(lamp.Tiles.All(t => t.Height <= lamp.Tiles[0].Height + Epsilon), "the lead's row is the tallest");
            Assert.AreEqual(lamp.Tiles.Max(t => t.Width * t.Height), lamp.Tiles[0].Width * lamp.Tiles[0].Height, Epsilon, "and its tile the largest");

            // - a lead in the middle: the tile before it is alone and spans its row, the pairing goes on after it
            var middle = BentoLayoutRule.Layout(new[] { 2, 3, 9, 4, 1 }, new[] { 100f, 100f, 100f, 100f, 100f }, MinHeight, Width, FullHeight, Gap, TallContent);
            CollectionAssert.AreEqual(new[] { new[] { 0, 1 }, new[] { 2 }, new[] { 3, 4 } }, middle.Rows.Select(r => r.ToArray()).ToArray());
            var leftBeforeLead = BentoLayoutRule.Layout(new[] { 2, 9, 4, 1 }, new[] { 100f, 100f, 100f, 100f }, MinHeight, Width, FullHeight, Gap, TallContent);
            CollectionAssert.AreEqual(new[] { new[] { 0 }, new[] { 1 }, new[] { 2, 3 } }, leftBeforeLead.Rows.Select(r => r.ToArray()).ToArray());
            Assert.AreEqual(Width, leftBeforeLead.Tiles[0].Width, Epsilon, "alone before the lead: it spans its row");
            Assert.AreEqual(1, BentoLayoutRule.Lead(new[] { 4, 7, 7 }), "the first of equals leads");

            // - two words too wide to sit side by side go one per row, each spanning it
            var wide = BentoLayoutRule.Layout(new[] { 5, 3, 3 }, new[] { 100f, 200f, 200f }, MinHeight, Width, FullHeight, Gap, TallContent);
            CollectionAssert.AreEqual(new[] { new[] { 0 }, new[] { 1 }, new[] { 2 } }, wide.Rows.Select(r => r.ToArray()).ToArray());
        }

        // Proportion where no minimum holds: the rows' heights follow their totals, the widths inside a row follow the counts -- so a tile's
        // area is its count's share. One section is all the card
        [Test]
        public void WhereNoMinimumHolds_TheAreaFollowsTheCounts()
        {
            var even = BentoLayoutRule.Layout(new[] { 6, 3, 3 }, new[] { 80f, 80f, 80f }, MinHeight, Width, FullHeight, Gap, TallContent);
            float room = FullHeight - Gap;
            Assert.AreEqual(room * 6 / 12, even.Tiles[0].Height, Epsilon, "the lead's row: 6 of 12");
            Assert.AreEqual(room * 6 / 12, even.Tiles[1].Height, Epsilon, "the pair's row: 3 + 3 of 12");
            Assert.AreEqual((Width - Gap) / 2, even.Tiles[1].Width, Epsilon, "3 and 3 share their row evenly");
            Assert.AreEqual(0, even.RowsAtMinimum);

            var uneven = BentoLayoutRule.Layout(new[] { 9, 6, 2 }, new[] { 60f, 60f, 60f }, MinHeight, Width, FullHeight, Gap, TallContent);
            Assert.AreEqual((Width - Gap) * 6 / 8, uneven.Tiles[1].Width, Epsilon, "6 of the row's 8");
            Assert.AreEqual((Width - Gap) * 2 / 8, uneven.Tiles[2].Width, Epsilon, "2 of 8");

            var one = BentoLayoutRule.Layout(new[] { 5 }, new[] { 100f }, MinHeight, Width, FullHeight, Gap, TallContent);
            Assert.AreEqual(Width, one.Tiles[0].Width, Epsilon);
            Assert.AreEqual(FullHeight, one.Tiles[0].Height, Epsilon, "one section is the whole card");
        }

        // The target wins (WCAG 2.5.8, the progress map's rule): a share smaller than the minimum takes the minimum and the others share the
        // rest -- The Lamp's one-block Listen beside Media's 14, its one-row Community. Below the fill point (the rows' minimums taller than
        // the room) every row keeps its minimum and the overview scrolls
        [Test]
        public void TheTargetWins_ASmallShareTakesTheMinimum_AndPastTheFillPointEveryRowDoes()
        {
            var lamp = LayoutOf(8);
            Assert.AreEqual(MinWidths[2], lamp.Tiles[2].Width, Epsilon, "Listen (1 of 15 in its row) keeps its words' width");
            Assert.AreEqual(Width - Gap - MinWidths[2], lamp.Tiles[1].Width, Epsilon, "Media takes the rest of the row");
            Assert.AreEqual(MinHeight, lamp.Tiles[7].Height, Epsilon, "Community's row (2 of 56) keeps the minimum height");
            Assert.AreEqual(1, lamp.RowsAtMinimum, "only that row");

            // - Half: 106.6 px of room -- no two rows fit at their minimum, so they all keep it and the overview scrolls
            var half = LayoutOf(8, 106.6f);
            Assert.IsFalse(half.Fills, "at Half the tiles do not fit");
            Assert.AreEqual(half.Rows.Count, half.RowsAtMinimum);
            Assert.AreEqual(half.Rows.Count * MinHeight + (half.Rows.Count - 1) * Gap, half.Height, Epsilon, "the overview is taller than the room");

            // - the fill point at Full, sections of equal size (the proportion between rows lasts while the rows' minimums fit): the lead's row
            //   plus one row per two others, 64 + 8 each, against 565 -- 13 sections fill the card, 14 do not
            int FillPoint()
            {
                for (int n = 1; n <= 30; n++)
                {
                    var equal = Enumerable.Repeat(3, n).ToArray();
                    if (!BentoLayoutRule.Layout(equal, Enumerable.Repeat(100f, n).ToArray(), MinHeight, Width, FullHeight, Gap, TallContent).Fills) return n - 1;
                }
                return 30;
            }
            Assert.AreEqual(13, FillPoint(), "past 13 sections every row is at its minimum on a Full card at 390 px");
        }

        // Step 16 (the 15b review): a tile never grows past what its content needs. The gallery's small case -- 3 sections of 3, 2 and 1
        // blocks at a Full card -- used to stretch three plates over 565 px; now every row is exactly as tall as its tallest tile's content
        // (a row's tiles share one height: no hole), the tiles stack from the top, and the room under the last one stays empty
        [Test]
        public void ATileNeverGrowsPastItsContent_AFewSectionsLeaveTheRoomUnderTheLastTileEmpty()
        {
            int[] counts = { 3, 2, 1 };
            float[] content = { 132f, 108f, 84f };
            var small = BentoLayoutRule.Layout(counts, new[] { 100f, 96f, 120f }, MinHeight, Width, FullHeight, Gap, (s, w) => content[s]);
            CollectionAssert.AreEqual(new[] { new[] { 0 }, new[] { 1, 2 } }, small.Rows.Select(r => r.ToArray()).ToArray(), "precondition: lead + a pair");
            Assert.AreEqual(content[0], small.Tiles[0].Height, Epsilon, "the lead: as tall as its content");
            Assert.AreEqual(content[1], small.Tiles[1].Height, Epsilon, "the pair's row: as tall as its taller tile's content");
            Assert.AreEqual(small.Tiles[1].Height, small.Tiles[2].Height, Epsilon, "and both tiles of the row share it");
            float used = content[0] + Gap + content[1];
            Assert.AreEqual(used, small.Height, Epsilon, "the tiles take exactly their content");
            Assert.AreEqual(FullHeight - used, small.RoomLeft, Epsilon, "the rest of the card stays empty, under the last tile");
            Assert.Greater(small.RoomLeft, 0f);
            Assert.IsTrue(small.Fills, "the minimums fit: the rows were shared by count, then capped");

            // - only ONE row capped: what it cannot use goes to the others, by their counts, and the card is filled again (no room left)
            int[] lamp = { 18, 14, 1, 7 };
            float[] lampContent = { 10000f, 10000f, 10000f, 70f };
            var some = BentoLayoutRule.Layout(lamp, new[] { 100f, 96f, 120f, 104f }, MinHeight, Width, FullHeight, Gap, (s, w) => lampContent[s]);
            CollectionAssert.AreEqual(new[] { new[] { 0 }, new[] { 1, 2 }, new[] { 3 } }, some.Rows.Select(r => r.ToArray()).ToArray());
            Assert.AreEqual(70f, some.Tiles[3].Height, Epsilon, "the capped row keeps its content's height");
            float shared = FullHeight - 2 * Gap - 70f;
            Assert.AreEqual(shared * 18 / 33, some.Tiles[0].Height, Epsilon, "the others share the rest by count: 18 of 33");
            Assert.AreEqual(shared * 15 / 33, some.Tiles[1].Height, Epsilon, "15 of 33");
            Assert.AreEqual(FullHeight, some.Height, Epsilon, "the card is filled");
            Assert.AreEqual(0f, some.RoomLeft, Epsilon);

            // - a content lower than the tile minimum keeps the minimum (the touch target, the title over the count)
            var tiny = BentoLayoutRule.Layout(new[] { 2, 1 }, new[] { 100f, 100f }, MinHeight, Width, FullHeight, Gap, (s, w) => 20f);
            Assert.IsTrue(tiny.Tiles.All(t => System.Math.Abs(t.Height - MinHeight) < Epsilon), "never under the minimum");
            Assert.AreEqual(FullHeight - (2 * MinHeight + Gap), tiny.RoomLeft, Epsilon);

            // - the content is asked for the tile's OWN width (whether its names show depends on it)
            var asked = new List<(int, float)>();
            BentoLayoutRule.Layout(counts, new[] { 100f, 96f, 120f }, MinHeight, Width, FullHeight, Gap, (s, w) => { asked.Add((s, w)); return 80f; });
            CollectionAssert.AreEquivalent(new[] { 0, 1, 2 }, asked.Select(a => a.Item1).Distinct(), "every tile's content is asked");
            Assert.AreEqual(Width, asked.First(a => a.Item1 == 0).Item2, Epsilon, "the lead at the full width");
            Assert.Less(asked.First(a => a.Item1 == 1).Item2, Width, "a paired tile at its share of the row");
        }

        // _3.2 audit F1 (mutation 141): two words whose minimums plus the gap equal the width exactly still sit side by side
        [Test]
        public void TwoTilesWhoseMinimumsExactlyFillTheWidth_ShareARow_AndAHairMoreSplitsThem()
        {
            float half = (Width - Gap) / 2f;
            var exact = BentoLayoutRule.Rows(new[] { 5, 3, 3 }, new[] { 100f, half, half }, Width, Gap);
            CollectionAssert.AreEqual(new[] { new[] { 0 }, new[] { 1, 2 } }, exact.Select(r => r.ToArray()).ToArray(), "minimums + gap == width: a pair");
            var over = BentoLayoutRule.Rows(new[] { 5, 3, 3 }, new[] { 100f, half, half + 0.01f }, Width, Gap);
            CollectionAssert.AreEqual(new[] { new[] { 0 }, new[] { 1 }, new[] { 2 } }, over.Select(r => r.ToArray()).ToArray(), "a hair wider: one per row");
        }

        // _3.2 audit F1 (mutation 144): rows whose minimums exactly equal the room are not shared by count (Fills stays false); a hair more
        // room does share
        [Test]
        public void MinimumsThatExactlyEqualTheRoom_DoNotFill_AHairMoreRoomDoes()
        {
            int[] counts = { 5, 3 };
            float[] widths = { 100f, 100f };
            float exactHeight = 2 * MinHeight + Gap;
            var exact = BentoLayoutRule.Layout(counts, widths, MinHeight, Width, exactHeight, Gap, TallContent);
            Assert.AreEqual(2, exact.Rows.Count, "precondition: two rows (the lead alone, the other alone)");
            Assert.IsFalse(exact.Fills, "rows' minimums == the room: nothing left to share");
            Assert.AreEqual(exactHeight, exact.Height, Epsilon, "every row at its minimum fills it exactly all the same");
            var roomier = BentoLayoutRule.Layout(counts, widths, MinHeight, Width, exactHeight + 1f, Gap, TallContent);
            Assert.IsTrue(roomier.Fills, "one unit more room: the rows share it by count");
        }

        [Test]
        public void NoSections_NoTiles()
        {
            var none = BentoLayoutRule.Layout(new int[0], new float[0], MinHeight, Width, FullHeight, Gap, TallContent);
            Assert.IsEmpty(none.Tiles);
            Assert.AreEqual(0f, none.Height);
            Assert.AreEqual(-1, BentoLayoutRule.Lead(new int[0]));
            Assert.AreEqual(-1, BentoLayoutRule.Lead(null));
        }
    }
}
