using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // The in-card facets (_3.2 step 18, section 5.8): the Needs flag on the KIND and the ONE pure rule both menus read. Run on the SHIPPED
    // LivingRoom wall's real cards, through the real BlockStackBuilder, CardNavModel and ReadingTimeRule: each facet alone, combined,
    // a card with none of a facet, a block with no reading time, and which chips a card offers. A clip's length needs the media a view
    // loads, so here an audio / video block has no time (Phase B measures it with the real clips).
    public class NavFacetRuleTests
    {
        private sealed class PlainView : IBlockView
        {
            public UnityEngine.UIElements.VisualElement Root { get; } = new();
            public void Bind(BlockInstanceData instance, BlockBindContext context) { }
            public void Unbind() { }
        }

        private const string Lamp = "lamp";
        private const string LampMilitary = "lamp_military";
        private const string LampReligious = "lamp_religious";

        private static WallConfigData Shipped() => JsonUtility.FromJson<WallConfigData>(File.ReadAllText("Assets/Apps/LivingRoom/config.json"));

        // One card of the shipped wall: its model (every target) and the facts of each target, from the real kinds and fields
        private sealed class Card
        {
            public CardNavModel Model;
            public Dictionary<string, BlockFacts> Facts = new();
            public Dictionary<string, BlockStackBuilder.Entry> Entries = new();
            public List<string> Targets => Model.Targets.ToList();
            public List<BlockFacts> All => Model.Targets.Select(t => Facts[t]).ToList();
            public List<string> Passing(NavFacets facets) => NavFacetRule.Passing(Model.Targets, t => Facts[t], facets);
        }

        private static Card CardOf(string poiId)
        {
            var config = Shipped();
            var entries = BlockStackBuilder.Build(config.pois.Single(p => p.id == poiId), config.card_settings, BlockRegistry.Shared, config.pois).Entries;
            var card = new Card { Model = CardNavModel.Build(entries, BlockRegistry.Shared, config.card_settings) };
            foreach (var entry in entries) card.Entries[entry.Instance.key] = entry;
            foreach (string target in card.Model.Targets)
            {
                var entry = card.Entries[target];
                card.Facts[target] = NavFacetRule.FactsOf(entry.Definition, ReadingTimeRule.Minutes(entry.Definition, entry.Instance, 0f));
            }
            return card;
        }

        private static NavFacets With(string format = null, int minutes = 0, params string[] without)
        {
            var facets = new NavFacets();
            if (format != null) facets.ToggleFormat(format);
            if (minutes > 0) facets.ToggleMinutes(minutes);
            foreach (string need in without) facets.ToggleWithout(need);
            return facets;
        }

        private static List<NavFacetRule.Chip> Offered(IReadOnlyList<BlockFacts> card, NavFacets facets) => NavFacetRule.Chips(card, facets);

        private static bool Has(IEnumerable<NavFacetRule.Chip> chips, NavFacetRule.Dimension dimension, string value) =>
            chips.Any(c => c.Dimension == dimension && c.Value == value);

        // ---------------- the Needs flag: on the kind, never per block ----------------

        [Test]
        public void TheKinds_DeclareTheirNeeds_AudioGuideHeadphones_PlaceInArAndShowOnWallCamera_AndNoOtherKindNeedsAnything()
        {
            var needs = BlockRegistry.Shared.All.ToDictionary(k => k.Key, k => k.Needs?.ToArray() ?? new string[0]);
            Assert.Greater(needs.Count, 30, "precondition: the whole catalogue, framework and app, is registered");
            CollectionAssert.AreEqual(new[] { CardOptions.NeedHeadphones }, needs[BuiltInBlocks.AudioGuideKind]);
            CollectionAssert.AreEqual(new[] { CardOptions.NeedCamera }, needs[BuiltInBlocks.PlaceInArKind]);
            CollectionAssert.AreEqual(new[] { CardOptions.NeedCamera }, needs[BuiltInBlocks.ShowOnWallKind]);
            var others = needs.Where(pair => pair.Value.Length > 0).Select(pair => pair.Key).Except(new[]
                { BuiltInBlocks.AudioGuideKind, BuiltInBlocks.PlaceInArKind, BuiltInBlocks.ShowOnWallKind }).ToList();
            CollectionAssert.IsEmpty(others, "most kinds need nothing: their absence of the flag means so");
            // - no built-in kind loads anything over the network (every picture, clip and model comes from the wall's media folder), so no
            //   kind declares network today: the word is in the vocabulary for an app kind that streams
            Assert.IsFalse(BlockRegistry.Shared.All.Any(k => k.Needs != null && k.Needs.Contains(CardOptions.NeedNetwork)));
        }

        [Test]
        public void NoKindHasABlockFieldForNeeds_ItIsTheKindsFlagAlone()
        {
            foreach (var kind in BlockRegistry.Shared.All)
                foreach (var field in kind.Fields)
                    StringAssert.DoesNotContain("need", field.Key.ToLowerInvariant(), kind.Key + " would make a wall author declare a need per block");
        }

        [Test]
        public void Validate_RefusesAnUnknownNeed_NamingItAndTheList_AndAcceptsNoneEmptyAndEveryKnownOne()
        {
            BlockKindDefinition Kind(params string[] needs) => new()
            {
                Key = "needs_probe", Family = BuiltInFamilies.About, Format = CardOptions.FormatRead, IconKey = CardIcons.Info, DisplayName = "Probe",
                Variants = new[] { "plain" }, DefaultVariant = "plain", DisplayModes = new[] { CardOptions.DisplayInline }, Needs = needs,
            };
            string problem = BlockRegistry.Validate(Kind("wifi"));
            StringAssert.Contains("'wifi'", problem);
            StringAssert.Contains(string.Join(", ", CardOptions.Needs), problem);
            Assert.IsNull(BlockRegistry.Validate(Kind()), "an empty list is nothing needed");
            Assert.IsNull(BlockRegistry.Validate(Kind(CardOptions.Needs)), "all three known needs together are a valid shape");
            var none = Kind();
            none.Needs = null;
            Assert.IsNull(BlockRegistry.Validate(none), "null is nothing needed (most kinds)");
            var registry = new BlockRegistry();
            registry.RegisterFamily(new FamilyDefinition { Key = BuiltInFamilies.About, Order = 10, LabelKey = CardStrings.Keys.FamilyAbout });
            Assert.Throws<System.ArgumentException>(() => registry.Register(Kind("Headphones"), () => new PlainView()), "needs are matched exactly");
            Assert.DoesNotThrow(() => registry.Register(Kind(CardOptions.NeedHeadphones), () => new PlainView()), "the same kind with a known need registers");
        }

        [Test]
        public void TheFactsOfABlock_CarryItsKindsFormatAndNeeds_AndItsMinutes()
        {
            var facts = NavFacetRule.FactsOf(BuiltInBlocks.AudioGuide, 4);
            Assert.AreEqual(CardOptions.FormatListen, facts.Format);
            Assert.AreEqual(4, facts.Minutes);
            Assert.IsTrue(facts.NeedsA(CardOptions.NeedHeadphones));
            Assert.IsFalse(facts.NeedsA(CardOptions.NeedCamera));
            Assert.IsFalse(NavFacetRule.FactsOf(BuiltInBlocks.RichText, 0).NeedsA(CardOptions.NeedHeadphones), "a kind with no flag needs nothing");
            Assert.IsFalse(default(BlockFacts).NeedsA(CardOptions.NeedCamera), "a default value holds no list and needs nothing");
        }

        // ---------------- the rule, each facet alone, on The Lamp ----------------

        [Test]
        public void NoFacetOn_EveryBlockPasses_InTheStacksOrder()
        {
            var lamp = CardOf(Lamp);
            Assert.AreEqual(56, lamp.Targets.Count, "precondition: The Lamp's 56 targets");
            CollectionAssert.AreEqual(lamp.Targets, lamp.Passing(new NavFacets()));
            CollectionAssert.AreEqual(lamp.Targets, NavFacetRule.Passing(lamp.Targets, t => lamp.Facts[t], null), "no facet object at all is the same");
            Assert.IsFalse(new NavFacets().IsActive);
        }

        [Test]
        public void Format_KeepsExactlyTheBlocksOfThatFormat()
        {
            var lamp = CardOf(Lamp);
            var listen = lamp.Passing(With(CardOptions.FormatListen));
            CollectionAssert.AreEqual(new[] { "block_55" }, listen, "The Lamp's one audio player is its only Listen target");
            foreach (string format in CardOptions.Formats)
            {
                var kept = lamp.Passing(With(format));
                Assert.IsNotEmpty(kept, "precondition: The Lamp holds a " + format + " block");
                Assert.IsTrue(kept.All(b => lamp.Entries[b].Definition.Format == format), format + ": only that format");
                Assert.AreEqual(lamp.Targets.Count(b => lamp.Entries[b].Definition.Format == format), kept.Count, format + ": every block of it");
            }
            CollectionAssert.AreEqual(lamp.Targets.Where(lamp.Passing(With(CardOptions.FormatRead)).Contains), lamp.Passing(With(CardOptions.FormatRead)),
                "the stack's order is kept");
        }

        [Test]
        public void ReadingTime_KeepsTheTimedBlocksWithinTheLimit_AndNeverABlockNobodyTimed()
        {
            var lamp = CardOf(Lamp);
            // - the fixture's four timed blocks (step 11b): block_2 3 min, block_5 4, block_22 2, block_26 5
            CollectionAssert.AreEqual(new[] { "block_22" }, lamp.Passing(With(minutes: NavFacetRule.ShortMinutes)), "<= 2 min: only the 2 min block");
            CollectionAssert.AreEquivalent(new[] { "block_2", "block_5", "block_22", "block_26" }, lamp.Passing(With(minutes: NavFacetRule.MediumMinutes)),
                "<= 5 min: the four timed blocks, never the 52 nobody timed");
            Assert.IsFalse(NavFacetRule.Passes(new BlockFacts(CardOptions.FormatRead, 0, null), With(minutes: NavFacetRule.MediumMinutes)),
                "a block with no reading time does not match a time facet: nothing says it fits");
            Assert.IsFalse(NavFacetRule.Passes(new BlockFacts(CardOptions.FormatRead, 6, null), With(minutes: NavFacetRule.MediumMinutes)), "6 min is over");
            Assert.IsTrue(NavFacetRule.Passes(new BlockFacts(CardOptions.FormatRead, 5, null), With(minutes: NavFacetRule.MediumMinutes)), "the limit itself fits");
        }

        [Test]
        public void Without_LeavesOutTheBlocksOfTheKindsThatNeedIt()
        {
            var lamp = CardOf(Lamp);
            var noHeadphones = lamp.Passing(With(without: CardOptions.NeedHeadphones));
            CollectionAssert.AreEqual(lamp.Targets.Where(t => t != "block_55"), noHeadphones, "everything but the audio player, in order");

            var camera = lamp.Targets.Where(t => lamp.Entries[t].Definition.Needs?.Contains(CardOptions.NeedCamera) == true).ToList();
            Assert.GreaterOrEqual(camera.Count, 2, "precondition: The Lamp has Place In AR and Show On Wall blocks");
            var noCamera = lamp.Passing(With(without: CardOptions.NeedCamera));
            CollectionAssert.AreEqual(lamp.Targets.Except(camera), noCamera);

            CollectionAssert.AreEqual(lamp.Targets, lamp.Passing(With(without: CardOptions.NeedNetwork)),
                "nothing on The Lamp needs a network: that chip leaves every block, so it is not offered either");
        }

        // ---------------- combined ----------------

        [Test]
        public void TheFacetsCombine_FormatAndTimeAndEveryWithout_AsAnAnd()
        {
            var lamp = CardOf(Lamp);
            var read = lamp.Passing(With(CardOptions.FormatRead));
            var fast = lamp.Passing(With(minutes: NavFacetRule.MediumMinutes));
            var both = lamp.Passing(With(CardOptions.FormatRead, NavFacetRule.MediumMinutes));
            CollectionAssert.AreEqual(read.Where(fast.Contains), both, "format AND time = the blocks in both lists");
            Assert.Less(both.Count, read.Count, "precondition: the time facet really narrowed the reading blocks");
            Assert.Greater(both.Count, 0, "precondition: some block is both");

            var all = With(CardOptions.FormatListen, 0, CardOptions.NeedHeadphones);
            CollectionAssert.IsEmpty(lamp.Passing(all), "Listen AND without headphones: the one audio player needs them -- nothing is left");
            var twoWithouts = lamp.Passing(With(without: new[] { CardOptions.NeedHeadphones, CardOptions.NeedCamera }));
            CollectionAssert.AreEqual(lamp.Passing(With(without: CardOptions.NeedHeadphones)).Intersect(lamp.Passing(With(without: CardOptions.NeedCamera))).OrderBy(lamp.Targets.IndexOf),
                twoWithouts, "two withouts leave out both kinds' blocks");
        }

        [Test]
        public void Toggling_TwiceSwitchesAFacetOff_ClearReturnsEverything_AndACopyIsIndependent()
        {
            var facets = With(CardOptions.FormatLook, NavFacetRule.ShortMinutes, CardOptions.NeedCamera);
            Assert.IsTrue(facets.IsActive);
            var copy = facets.Copy();
            facets.ToggleFormat(CardOptions.FormatLook);
            facets.ToggleMinutes(NavFacetRule.ShortMinutes);
            facets.ToggleWithout(CardOptions.NeedCamera);
            Assert.IsFalse(facets.IsActive, "a second tap on each switches it off");
            Assert.IsTrue(copy.IsActive, "the copy made before is untouched");
            copy.ToggleMinutes(NavFacetRule.MediumMinutes);
            Assert.AreEqual(NavFacetRule.MediumMinutes, copy.MaxMinutes, "one time limit at a time: a second limit replaces the first");
            copy.ToggleFormat(CardOptions.FormatWatch);
            Assert.AreEqual(CardOptions.FormatWatch, copy.Format, "one format at a time");
            copy.Clear();
            Assert.IsFalse(copy.IsActive);
            CollectionAssert.IsEmpty(copy.Without);
        }

        // ---------------- which chips a card offers ----------------

        [Test]
        public void TheLampOffersEveryChipItsBlocksSupport_InTheMenusOrder_AndNoneItDoesNot()
        {
            var lamp = CardOf(Lamp);
            var chips = Offered(lamp.All, new NavFacets());
            var time = chips.Where(c => c.Dimension == NavFacetRule.Dimension.Time).Select(c => c.Minutes);
            CollectionAssert.AreEqual(new[] { 2, 5 }, time, "both limits: a 2 min block, and three blocks between 2 and 5");
            var needs = chips.Where(c => c.Dimension == NavFacetRule.Dimension.Without).Select(c => c.Value);
            CollectionAssert.AreEqual(new[] { CardOptions.NeedHeadphones, CardOptions.NeedCamera }, needs, "network is not offered: nothing needs it");
            var formats = chips.Where(c => c.Dimension == NavFacetRule.Dimension.Format).Select(c => c.Value);
            CollectionAssert.AreEqual(CardOptions.Formats.Where(f => lamp.Targets.Any(t => lamp.Entries[t].Definition.Format == f)), formats,
                "exactly the formats this card holds, in the list's order");
            Assert.IsFalse(chips.Any(c => c.Active), "nothing is on yet");
            var order = chips.Select(c => (int)c.Dimension).ToList();
            CollectionAssert.AreEqual(order.OrderBy(d => d), order, "reading time, then needs, then formats");
        }

        [Test]
        public void ACardWithNoneOfAFacet_OffersNoChipForIt()
        {
            var military = CardOf(LampMilitary);
            Assert.AreEqual(4, military.Targets.Count, "precondition: Lamp - Military's four targets");
            var chips = Offered(military.All, new NavFacets());
            Assert.IsFalse(chips.Any(c => c.Dimension == NavFacetRule.Dimension.Without), "no audio guide, no placement: no needs chip");
            Assert.IsFalse(chips.Any(c => c.Dimension == NavFacetRule.Dimension.Time), "nothing on it is timed: no time chip");

            var religious = CardOf(LampReligious);
            CollectionAssert.IsEmpty(Offered(religious.All, new NavFacets()), "a one-block card has nothing to narrow: no chip at all");
            CollectionAssert.IsEmpty(Offered(new List<BlockFacts>(), new NavFacets()), "an empty card offers none");
        }

        [Test]
        public void AChipIsJudgedAgainstTheBlocksTheOtherFacetsLeave_NeverAgainstTheWholeCard()
        {
            var lamp = CardOf(Lamp);
            // - Listen is on: the one audio player is left. A 2 min limit would leave nothing, and "without headphones" nothing either
            var listen = Offered(lamp.All, With(CardOptions.FormatListen));
            Assert.IsTrue(listen.Any(c => c.Dimension == NavFacetRule.Dimension.Format && c.Value == CardOptions.FormatListen && c.Active),
                "the chip that is on stays, so it can be switched off");
            Assert.IsFalse(Has(listen, NavFacetRule.Dimension.Time, "2"), "Listen + 2 min would be an empty list");
            Assert.IsFalse(listen.Any(c => c.Dimension == NavFacetRule.Dimension.Time), "the one audio block is untimed here: no limit keeps it");
            // - the other formats stay offered while one is on: a tap switches to it (the format dimension is judged without its own choice)
            Assert.IsTrue(Has(listen, NavFacetRule.Dimension.Format, CardOptions.FormatRead), "another format is still one tap away");

            // - a 2 min limit leaves one block (block_22): a facet that changes nothing in a list of one is not offered
            var fast = Offered(lamp.All, With(minutes: NavFacetRule.ShortMinutes));
            Assert.IsTrue(fast.Any(c => c.Dimension == NavFacetRule.Dimension.Time && c.Minutes == 2 && c.Active));
            Assert.IsFalse(fast.Any(c => c.Dimension == NavFacetRule.Dimension.Without), "no needs chip over a list that needs nothing");
        }

        [Test]
        public void TheLongerTimeChip_OnlyAppearsWhenABlockLiesBetweenTheTwoLimits()
        {
            BlockFacts Block(int minutes) => new(CardOptions.FormatRead, minutes, null);
            var shortOnly = new List<BlockFacts> { Block(1), Block(2), Block(0), Block(9) };
            var chips = Offered(shortOnly, new NavFacets());
            Assert.IsTrue(Has(chips, NavFacetRule.Dimension.Time, "2"));
            Assert.IsFalse(Has(chips, NavFacetRule.Dimension.Time, "5"), "<= 5 would list what <= 2 lists: no news, no chip");
            var withBetween = new List<BlockFacts>(shortOnly) { Block(4) };
            Assert.IsTrue(Has(Offered(withBetween, new NavFacets()), NavFacetRule.Dimension.Time, "5"), "a 4 min block makes <= 5 a different list");

            var untimed = new List<BlockFacts> { Block(0), Block(0), Block(0) };
            CollectionAssert.IsEmpty(Offered(untimed, new NavFacets()).Where(c => c.Dimension == NavFacetRule.Dimension.Time),
                "a card nobody timed offers no time chip: it would filter to nothing");
        }

        [Test]
        public void EveryChip_HasItsWords_AndAnUnknownNeedHasNone()
        {
            foreach (var chip in new[]
            {
                new NavFacetRule.Chip(NavFacetRule.Dimension.Time, "2", 2, false),
                new NavFacetRule.Chip(NavFacetRule.Dimension.Without, CardOptions.NeedHeadphones, 0, false),
                new NavFacetRule.Chip(NavFacetRule.Dimension.Without, CardOptions.NeedCamera, 0, false),
                new NavFacetRule.Chip(NavFacetRule.Dimension.Without, CardOptions.NeedNetwork, 0, false),
                new NavFacetRule.Chip(NavFacetRule.Dimension.Format, CardOptions.FormatModel, 0, false),
            })
            {
                string key = NavFacetRule.LabelKeyOf(chip);
                Assert.IsNotNull(key, chip.Dimension + " " + chip.Value + " has a CardStrings key");
                Assert.IsTrue(CardStrings.Keys.All.Contains(key), key + " is in the framework's key list (a row in en + pt is checked by the table tests)");
            }
            Assert.IsNull(NavFacetRule.LabelKeyOf(new NavFacetRule.Chip(NavFacetRule.Dimension.Without, "wifi", 0, false)));
        }
    }
}
