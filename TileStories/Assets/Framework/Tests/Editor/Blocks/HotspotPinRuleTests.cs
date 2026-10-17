using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // Tier 0 of the hotspot pin's jump (_3.2 section 5.5, step 10) on The Lamp's real card: HotspotPinRule says a pin jumps only to a
    // block the card can scroll to (BlockRefRule.JumpTargets over the real built stack), so a pin never draws a dead jump -- nothing
    // picked, the title block, a pinned or footer look, a block waiting for Show After Reading and a deleted block all keep the pin's
    // own popup. The fixture: The Lamp's two hotspot blocks carry ONE Opens Block each, and no pin is left with a pick that goes nowhere.
    public class HotspotPinRuleTests
    {
        private const string Lamp = "lamp";
        private const string Header = "block_1";
        private const string HeroChip = "block_54";
        private const string StickyFooter = "block_19";
        private const string WaitsForReading = "block_44";

        private static WallConfigData Shipped() =>
            JsonUtility.FromJson<WallConfigData>(File.ReadAllText("Assets/Apps/LivingRoom/config.json"));

        private static System.Collections.Generic.HashSet<string> LampTargets(WallConfigData config, bool revealed = false)
        {
            var lamp = config.pois.Single(p => p.id == Lamp);
            var built = BlockStackBuilder.Build(lamp, config.card_settings, BlockRegistry.Shared, config.pois);
            return BlockRefRule.JumpTargets(built.Entries, _ => revealed);
        }

        [Test]
        public void APin_JumpsOnlyToABlockTheCardCanScrollTo()
        {
            var targets = LampTargets(Shipped());
            Assert.AreEqual("block_2", HotspotPinRule.JumpTarget("block_2", targets), "a shown block of the scrolling stack: the pin jumps there");
            Assert.AreEqual("", HotspotPinRule.JumpTarget("", targets), "nothing picked: the pin opens its own text");
            Assert.AreEqual("", HotspotPinRule.JumpTarget("  ", targets), "blank: the same");
            Assert.AreEqual("", HotspotPinRule.JumpTarget(null, targets), "no value at all: the same");
            Assert.AreEqual("", HotspotPinRule.JumpTarget(Header, targets), "the title block is not in the scroll");
            Assert.AreEqual("", HotspotPinRule.JumpTarget(HeroChip, targets), "a look pinned under the title is not in the scroll");
            Assert.AreEqual("", HotspotPinRule.JumpTarget(StickyFooter, targets), "a footer look is not in the scroll");
            Assert.AreEqual("", HotspotPinRule.JumpTarget(WaitsForReading, targets), "a block waiting for Show After Reading is not there yet");
            Assert.AreEqual("block_44", HotspotPinRule.JumpTarget(WaitsForReading, LampTargets(Shipped(), revealed: true)), "...and is, once the card was read");
            Assert.AreEqual("", HotspotPinRule.JumpTarget("block_999", targets), "a deleted block");
            Assert.AreEqual("", HotspotPinRule.JumpTarget("block_2", null), "a card that hands no targets (an unbound one): never a jump");
        }

        // The fixture content: which pins of The Lamp open a block, and that each of those picks really leads somewhere
        [Test]
        public void TheLampsHotspotBlocks_CarryOneOpensBlockEach_AndEachPickLeadsSomewhere()
        {
            var config = Shipped();
            var lamp = config.pois.Single(p => p.id == Lamp);
            var targets = LampTargets(config);
            var opens = new System.Collections.Generic.List<(string Block, int Pin, string Target)>();
            foreach (var block in lamp.card.blocks.Where(b => b.kind == BuiltInBlocks.HotspotImageKind))
            {
                var read = new BlockFieldReader(block, "en", "en");
                var pins = read.Items(BuiltInBlocks.HotspotItemsField);
                for (int i = 0; i < pins.Count; i++)
                {
                    string stored = read.ItemValue(pins[i], BuiltInBlocks.HotspotOpensBlockField);
                    if (stored.Length > 0) opens.Add((block.key, i, stored));
                }
            }
            Assert.AreEqual(2, opens.Count, "one Opens Block on the numbered block, one on the loupes block");
            foreach (var (block, pin, target) in opens)
            {
                Assert.AreEqual(target, HotspotPinRule.JumpTarget(target, targets), block + " pin " + pin + " opens '" + target + "': reachable");
                Assert.AreNotEqual(block, target, block + " pin " + pin + ": not its own block");
            }
        }
    }
}
