using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // The Card Starter (_3.1 "Card Starter", _3.2 step 17b) as DATA, on a REAL point of the shipped wall (The Camera: no blocks) with the
    // wall's real points around it: every starter names registered kinds in looks they have and choices that are real options; it writes
    // ordinary blocks and NO content -- every required field it leaves empty is named by the card's own builder as the author's to fill,
    // and the kinds that need nothing show at once; Start from scratch writes nothing on an empty point; a replaced block's key is never
    // reused; the row's name reads back the starter a point's blocks still follow.
    public class CardStartersTests
    {
        private const string EmptyPoint = "camera";

        private static WallConfigData ShippedConfig() =>
            JsonUtility.FromJson<WallConfigData>(File.ReadAllText("Assets/Apps/LivingRoom/config.json"));

        private static POIData Point(WallConfigData config, string id) => config.pois.Single(p => p.id == id);

        private static IEnumerable<CardStarters.Starter> Sets => CardStarters.All.Where(s => !s.IsEmpty);

        [Test]
        public void EveryStarter_NamesRegisteredKinds_InLooksTheyHave_AndChoicesThatAreRealOptions()
        {
            Assert.GreaterOrEqual(Sets.Count(), 3, "three or more ready-made sets");
            Assert.IsTrue(CardStarters.All.Last().IsEmpty, "Start from scratch is the last row");
            Assert.AreEqual("Start from scratch", CardStarters.All.Last().Label);
            Assert.AreEqual(1, CardStarters.All.Count(s => s.IsEmpty), "one empty starter");
            Assert.AreEqual(CardStarters.All.Count - 1, CardStarters.SeparatorBefore, "the picker's line sets the empty card apart");
            Assert.AreEqual(CardStarters.All.Count, CardStarters.All.Select(s => s.Label).Distinct().Count(), "no two starters share a name");

            foreach (var starter in CardStarters.All)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(starter.Suits), starter.Label + " says what it suits");
                Assert.Less(starter.Suits.Length, 160, starter.Label + ": a line, not a paragraph");
                EditorTextChecks.AssertAscii(starter.Label + starter.Suits, starter.Label);
            }
            foreach (var starter in Sets)
            {
                Assert.AreEqual(BuiltInBlocks.HeaderKind, starter.Entries[0].Kind, starter.Label + ": a card starts with its header");
                Assert.AreEqual(starter.Entries.Count, starter.Entries.Select(e => e.Kind).Distinct().Count(), starter.Label + ": one block per kind");
                foreach (var entry in starter.Entries)
                {
                    Assert.IsTrue(BlockRegistry.Shared.TryGet(entry.Kind, out var kind), starter.Label + ": " + entry.Kind + " is a registered kind");
                    Assert.IsTrue(kind.HasVariant(entry.Variant), starter.Label + ": " + entry.Kind + " has the look " + entry.Variant);
                    Assert.AreNotEqual(BuiltInFamilies.Navigation, kind.Family,
                        starter.Label + ": no nav block -- the wall's Auto Nav Block puts one on every long card, a starter must not put a second");
                    foreach (var (field, option) in entry.Choices)
                    {
                        var definition = kind.Field(field);
                        Assert.IsNotNull(definition, starter.Label + ": " + entry.Kind + " has a field " + field);
                        Assert.AreEqual(BlockFieldType.Choice, definition.Type, starter.Label + ": a starter presets choices only, never content");
                        CollectionAssert.Contains(definition.Options, option, starter.Label + ": " + option + " is one of " + field + "'s options");
                    }
                }
            }
        }

        [Test]
        public void AStarterWritesNoContent_EveryRequiredFieldItLeavesEmptyIsNamedByTheCard_AndTheKindsThatNeedNothingShowAtOnce()
        {
            foreach (var starter in Sets)
            {
                var config = ShippedConfig();
                var poi = Point(config, EmptyPoint);
                Assert.IsEmpty(poi.card?.blocks ?? new List<BlockInstanceData>(), "precondition: " + EmptyPoint + " has no blocks");
                CardStarters.Apply(poi, starter);
                var built = BlockStackBuilder.Build(poi, config.card_settings, BlockRegistry.Shared, config.pois);

                foreach (var block in poi.card.blocks)
                {
                    BlockRegistry.Shared.TryGet(block.kind, out var kind);
                    var entry = starter.Entries.Single(e => e.Kind == block.kind);
                    // - no content: the only values written are the starter's own choices
                    CollectionAssert.AreEquivalent(entry.Choices.Select(c => c.Field), block.fields.Select(f => f.key),
                        starter.Label + ": " + block.kind + " holds its preset choices and nothing else");
                    foreach (var value in block.fields)
                    {
                        Assert.IsEmpty(value.text, starter.Label + ": no text written");
                        Assert.AreEqual("", value.asset, starter.Label + ": no file picked");
                        Assert.IsEmpty(value.items, starter.Label + ": no rows written");
                    }

                    var required = kind.Fields.Where(f => f.Required).Select(f => f.Key).ToList();
                    bool shown = built.Entries.Any(e => ReferenceEquals(e.Instance, block));
                    if (required.Count == 0)
                    {
                        Assert.IsTrue(shown, starter.Label + ": " + kind.DisplayName + " needs nothing written, so it shows at once on " + poi.name);
                        continue;
                    }
                    // - a kind that needs words, a picture or a clip: the card leaves it out and says which field is the author's to fill
                    Assert.IsFalse(shown, starter.Label + ": an empty " + kind.DisplayName + " is not shown");
                    var skipped = built.Skipped.Single(s => ReferenceEquals(s.Instance, block));
                    Assert.That(skipped.Reason, Is.EqualTo(BlockStackBuilder.SkipReason.MissingRequired).Or.EqualTo(BlockStackBuilder.SkipReason.NoCompleteRow),
                        starter.Label + ": " + kind.DisplayName + " is left out for an empty required field");
                    Assert.AreEqual(required[0], skipped.FieldKey, starter.Label + ": the card names " + kind.DisplayName + "'s first required field");
                }

                Assert.IsTrue(built.Entries[0].Synthesized, starter.Label + ": until its Title is written, the header still reads the point's name");
                Assert.AreEqual(poi.name, new BlockFieldReader(built.Entries[0].Instance, "en", "en").Text(BlockStackBuilder.HeaderTitleField));
                Assert.Greater(built.Entries.Count, 1, starter.Label + ": something shows besides the header before a word is written");
            }
        }

        [Test]
        public void OnAPointWithNoBlocks_AStarterWritesExactlyItsBlocks_InOrder_WithItsLooksAndFreshKeys()
        {
            foreach (var starter in Sets)
            {
                var poi = Point(ShippedConfig(), EmptyPoint);
                CardStarters.Apply(poi, starter);
                var blocks = poi.card.blocks;
                CollectionAssert.AreEqual(starter.Entries.Select(e => e.Kind), blocks.Select(b => b.kind), starter.Label + ": its kinds, in order");
                CollectionAssert.AreEqual(starter.Entries.Select(e => e.Variant), blocks.Select(b => b.variant), starter.Label + ": each in its look");
                foreach (var block in blocks)
                {
                    BlockRegistry.Shared.TryGet(block.kind, out var kind);
                    Assert.AreEqual(kind.DisplayModes[0], block.display, block.kind + ": the kind's first display, as + Add block gives");
                }
                CollectionAssert.AreEqual(Enumerable.Range(1, blocks.Count).Select(n => "block_" + n), blocks.Select(b => b.key),
                    starter.Label + ": an empty point's first keys");
                Assert.AreSame(starter, CardStarters.Matching(poi), starter.Label + ": the row reads the starter back");
            }
        }

        [Test]
        public void StartFromScratch_OnAPointWithNoBlocks_WritesNothing_NotEvenAnEmptyCard()
        {
            var scratch = CardStarters.All.Single(s => s.IsEmpty);
            var config = ShippedConfig();
            var poi = Point(config, EmptyPoint);
            poi.card = null;
            string before = JsonUtility.ToJson(config);
            CardStarters.Apply(poi, scratch);
            Assert.IsNull(poi.card, "no card made");
            Assert.AreEqual(before, JsonUtility.ToJson(config), "the whole config is exactly as it was");

            poi.card = new POICardData();
            before = JsonUtility.ToJson(config);
            CardStarters.Apply(poi, scratch);
            Assert.AreEqual(before, JsonUtility.ToJson(config), "an empty card stays the same empty card");
            Assert.AreSame(scratch, CardStarters.Matching(poi), "a point with no blocks reads Start from scratch");
        }

        [Test]
        public void ReplacingBlocks_NeverReusesAReplacedKey_AndStartFromScratchEmptiesTheCard()
        {
            var config = ShippedConfig();
            var lamp = Point(config, "lamp");
            var oldKeys = lamp.card.blocks.Select(b => b.key).ToList();
            Assert.Greater(oldKeys.Count, 50, "precondition: The Lamp's long authored card");

            var simple = Sets.First();
            CardStarters.Apply(lamp, simple);
            Assert.AreEqual(simple.Entries.Count, lamp.card.blocks.Count, "the starter's blocks replace the authored ones");
            CollectionAssert.IsEmpty(lamp.card.blocks.Select(b => b.key).Intersect(oldKeys),
                "no new block takes a replaced block's key (what the card saved for an old block must not land on a new one)");
            Assert.AreEqual(lamp.card.blocks.Count, lamp.card.blocks.Select(b => b.key).Distinct().Count(), "every key once");

            var keysOfTheSet = lamp.card.blocks.Select(b => b.key).ToList();
            CardStarters.Apply(lamp, CardStarters.All.Single(s => s.IsEmpty));
            Assert.IsNotNull(lamp.card);
            CollectionAssert.IsEmpty(lamp.card.blocks, "Start from scratch over blocks: an empty card");
            Assert.IsNotEmpty(keysOfTheSet);
        }

        [Test]
        public void TheRowReadsCustom_OnceTheBlocksNoLongerFollowAStarter()
        {
            var poi = Point(ShippedConfig(), EmptyPoint);
            var rich = Sets.Single(s => s.Label == "Rich point");
            CardStarters.Apply(poi, rich);
            Assert.AreSame(rich, CardStarters.Matching(poi));

            poi.card.blocks[1].variant = BuiltInBlocks.QuickFactsChips;
            Assert.IsNull(CardStarters.Matching(poi), "another look: the point's own (Custom)");
            CardStarters.Apply(poi, rich);
            poi.card.blocks.RemoveAt(poi.card.blocks.Count - 1);
            Assert.IsNull(CardStarters.Matching(poi), "a block fewer: Custom");
            Assert.IsNull(CardStarters.Matching(Point(ShippedConfig(), "lamp")), "The Lamp's authored card follows no starter");
        }
    }
}
