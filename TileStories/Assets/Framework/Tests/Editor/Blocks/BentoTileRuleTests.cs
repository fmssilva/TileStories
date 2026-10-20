using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // BentoTileRule (_3.2 step 15a, nav_bento): which of an author's tile rows the card draws, on THE LAMP through the real builder and
    // BlockRefRule -- a tile is drawn exactly when BlockRefRule says its block leads somewhere (the same answer the Editor's warning is made
    // from), the tile's On Tap is read as stored, and the grid spans the first tile and a lonely last one so a row never has a hole.
    public class BentoTileRuleTests
    {
        private const string Lamp = "lamp";
        private const string Header = "block_1";
        private const string HeroChip = "block_54";
        private const string StickyFooter = "block_19";
        private const string WaitsForReading = "block_44";
        private const string ARealBlock = "block_2";

        private static WallConfigData Shipped() =>
            JsonUtility.FromJson<WallConfigData>(File.ReadAllText("Assets/Apps/LivingRoom/config.json"));

        private static BentoTileRule.Tile Row(string target, string words = "", string picture = "", BentoTileRule.Opens opens = BentoTileRule.Opens.Jump) =>
            new(target, words, picture, opens);

        [Test]
        public void OpensOf_ReadsOnlyFullViewAsTheFullView_ABlankOrUnknownValueMovesTheCard()
        {
            Assert.AreEqual(BentoTileRule.Opens.FullView, BentoTileRule.OpensOf(BuiltInBlocks.NavBentoOpensFullView));
            Assert.AreEqual(BentoTileRule.Opens.Jump, BentoTileRule.OpensOf(BuiltInBlocks.NavBentoOpensJump));
            Assert.AreEqual(BentoTileRule.Opens.Jump, BentoTileRule.OpensOf(""), "nothing stored: the default, move the card");
            Assert.AreEqual(BentoTileRule.Opens.Jump, BentoTileRule.OpensOf("sideways"), "an unknown option reads as the default");
            Assert.AreEqual(BentoTileRule.Opens.Jump, BentoTileRule.OpensOf(null));
        }

        // The decision the brief asks a test for: reachable / not reachable, for EVERY block of the shipped card and the keys that are no block
        [Test]
        public void ATileIsDrawnExactlyWhenBlockRefRuleSaysItsBlockLeadsSomewhere_ForEveryBlockOfTheLamp_AndEveryKindOfBadPick()
        {
            var config = Shipped();
            var lamp = config.pois.Single(p => p.id == Lamp);
            var built = BlockStackBuilder.Build(lamp, config.card_settings, BlockRegistry.Shared, config.pois);
            var holder = new BlockInstanceData { key = "block_63", kind = BuiltInBlocks.NavBentoKind };
            var jump = BlockRefRule.JumpTargets(built.Entries, _ => false);

            var keys = lamp.card.blocks.Select(b => b.key).Concat(new[] { "block_999", "", "  ", null }).ToList();
            foreach (string key in keys)
            {
                var tiles = BentoTileRule.Tiles(new[] { Row(key) }, holder.key, jump);
                bool refLeads = BlockRefRule.Check(key, holder, lamp, built, _ => false) == BlockRefRule.Problem.None;
                Assert.AreEqual(refLeads, tiles.Count == 1, "'" + key + "': the tile is drawn exactly when BlockRefRule.Check finds no problem");
            }
            // - the four the fixture names, by their reasons
            foreach (string key in new[] { Header, HeroChip, StickyFooter, WaitsForReading, "block_999" })
                Assert.IsEmpty(BentoTileRule.Tiles(new[] { Row(key) }, holder.key, jump), key + ": not drawn");
            Assert.AreEqual(1, BentoTileRule.Tiles(new[] { Row(ARealBlock) }, holder.key, jump).Count, "a block of the scrolling part: drawn");
            // - a tile never leads to the bento block itself (BlockRefRule.Itself), though it is in the scroll
            var holderInStack = new HashSet<string>(jump) { holder.key };
            Assert.IsEmpty(BentoTileRule.Tiles(new[] { Row(holder.key) }, holder.key, holderInStack), "a tile pointing at its own block is not drawn");
        }

        [Test]
        public void TheTilesKeepTheAuthorsOrderAndWordsAndPictures_AndTheWaitingBlockJoinsOnceRevealed()
        {
            var config = Shipped();
            var lamp = config.pois.Single(p => p.id == Lamp);
            var built = BlockStackBuilder.Build(lamp, config.card_settings, BlockRegistry.Shared, config.pois);
            var rows = new[]
            {
                Row("block_5", "Five", "pics/a.png", BentoTileRule.Opens.FullView),
                Row(Header),
                Row(WaitsForReading, "Question"),
                Row("block_2", "", "", BentoTileRule.Opens.Jump),
            };
            var before = BentoTileRule.Tiles(rows, "block_63", BlockRefRule.JumpTargets(built.Entries, _ => false));
            CollectionAssert.AreEqual(new[] { "block_5", "block_2" }, before.Select(t => t.Target), "the author's order, the dead rows out");
            Assert.AreEqual("Five", before[0].Words);
            Assert.AreEqual("pics/a.png", before[0].Picture);
            Assert.AreEqual(BentoTileRule.Opens.FullView, before[0].Opens);
            Assert.AreEqual("", before[1].Words, "no words of its own: the view says the block's own");

            var after = BentoTileRule.Tiles(rows, "block_63", BlockRefRule.JumpTargets(built.Entries, key => key == WaitsForReading));
            CollectionAssert.AreEqual(new[] { "block_5", WaitsForReading, "block_2" }, after.Select(t => t.Target), "revealed, the waiting block's tile is drawn in its place");
        }

        [Test]
        public void NoRowsOrNoTargets_GiveNoTiles()
        {
            Assert.IsEmpty(BentoTileRule.Tiles(null, "x", new HashSet<string> { "a" }));
            Assert.IsEmpty(BentoTileRule.Tiles(new[] { Row("a") }, "x", null), "no jump targets: nothing leads anywhere");
            Assert.IsEmpty(BentoTileRule.Tiles(new BentoTileRule.Tile[0], "x", new HashSet<string> { "a" }));
        }

        [Test]
        public void Wide_SpansTheFirstTileAndALonelyLastOne_SoNoRowHasAHole()
        {
            CollectionAssert.AreEqual(new bool[0], BentoTileRule.Wide(0));
            CollectionAssert.AreEqual(new[] { true }, BentoTileRule.Wide(1));
            CollectionAssert.AreEqual(new[] { true, true }, BentoTileRule.Wide(2), "the second would sit alone beside nothing");
            CollectionAssert.AreEqual(new[] { true, false, false }, BentoTileRule.Wide(3));
            CollectionAssert.AreEqual(new[] { true, false, false, true }, BentoTileRule.Wide(4));
            CollectionAssert.AreEqual(new[] { true, false, false, false, false }, BentoTileRule.Wide(5));
            CollectionAssert.AreEqual(new[] { true, false, false, false, false, true }, BentoTileRule.Wide(6));
            for (int count = 1; count <= 12; count++)
            {
                // - every row is full: the cells' widths (a wide one is two, the others one) add up to whole rows of two
                int units = BentoTileRule.Wide(count).Sum(w => w ? 2 : 1);
                Assert.AreEqual(0, units % 2, count + " tiles fill whole rows (" + units + " half-widths)");
            }
        }
    }
}
