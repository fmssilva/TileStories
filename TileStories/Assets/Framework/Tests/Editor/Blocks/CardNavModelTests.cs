using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories.Editor.Tests
{
    // CardNavModel (_3.2 step 2, section 4.1) on the SHIPPED LivingRoom wall, through the REAL BlockStackBuilder and the shared
    // registry (framework + the app's kind): The Lamp reproduces section 4.4's worked example exactly, the short fixtures get no
    // navigation, an app family takes its own section where its Order says, a Show After Reading block joins once revealed (the
    // real CardLocalState), group_by = format regroups the very same targets, and Show Navigation From / Bar Slots do what they say.
    public class CardNavModelTests
    {
        private sealed class PlainView : IBlockView
        {
            public VisualElement Root { get; } = new();
            public void Bind(BlockInstanceData instance, BlockBindContext context) { }
            public void Unbind() { }
        }

        private const string Lamp = "lamp";
        private const string LampMilitary = "lamp_military";
        private const string LampReligious = "lamp_religious";

        private static WallConfigData Shipped() =>
            JsonUtility.FromJson<WallConfigData>(File.ReadAllText("Assets/Apps/LivingRoom/config.json"));

        private static WallConfigData ShippedByFamily()
        {
            var config = Shipped();
            config.card_settings.navigation.block_order = CardOptions.BlockOrderByFamily;
            return config;
        }

        private static List<BlockStackBuilder.Entry> Stack(WallConfigData config, string poiId, BlockRegistry registry = null) =>
            BlockStackBuilder.Build(config.pois.Single(p => p.id == poiId), config.card_settings, registry ?? BlockRegistry.Shared, config.pois).Entries;

        private static CardNavModel Model(WallConfigData config, string poiId, Func<string, bool> isRevealed = null) =>
            CardNavModel.Build(Stack(config, poiId), BlockRegistry.Shared, config.card_settings, isRevealed);

        private static IEnumerable<string> Keys(IEnumerable<CardNavModel.Section> sections) => sections.Select(s => s.Key);

        private static IEnumerable<string> AllTargets(CardNavModel model) => model.Sections.SelectMany(s => s.Targets);

        [OneTimeSetUp]
        public void TheAppsKindIsRegistered()
        {
            Assert.IsTrue(BlockRegistry.Shared.TryGet("size_comparison", out _),
                "precondition: the LivingRoom app registers size_comparison in Edit Mode, so The Lamp's 62 blocks all build");
        }

        // _3.2 4.4, block by block: 62 blocks -> 56 targets in 8 sections; Listen jumps straight to its player; no Sources section
        [Test]
        public void TheLamp_GroupedByFamily_ReproducesTheWorkedExampleExactly()
        {
            var config = ShippedByFamily();
            var entries = Stack(config, Lamp);
            Assert.AreEqual(62, entries.Count, "precondition: every one of The Lamp's 62 authored blocks survives the builder");

            var model = CardNavModel.Build(entries, BlockRegistry.Shared, config.card_settings);
            Assert.AreEqual(CardOptions.GroupByFamily, model.GroupBy, "family is the default grouping");
            Assert.AreEqual(56, model.TargetCount);
            CollectionAssert.AreEqual(new[]
            {
                BuiltInFamilies.About, BuiltInFamilies.Media, BuiltInFamilies.Guides, BuiltInFamilies.Stories,
                BuiltInFamilies.Ar, BuiltInFamilies.Play, BuiltInFamilies.Visit, BuiltInFamilies.Community,
            }, Keys(model.Sections), "8 sections in the families' order (by_family: the stack's order IS the family order)");
            CollectionAssert.AreEqual(new[] { 18, 14, 1, 7, 3, 4, 7, 2 }, model.Sections.Select(s => s.Count));
            Assert.AreEqual(56, model.Sections.Sum(s => s.Count), "the counts add up to the targets");
            Assert.IsTrue(model.ShowsNavigation, "56 targets >= Show Navigation From (8)");

            // - exactly the six blocks 4.4 names are left out, each for its rule-1 reason
            var excluded = entries.Select(e => e.Instance.key).Except(AllTargets(model)).ToList();
            CollectionAssert.AreEquivalent(new[]
            {
                "block_1",              // the header
                "block_54",             // audio_guide hero_chip (pinned top)
                "block_19",             // actions sticky_cta (footer)
                "block_15", "block_16", // the two sources (meta: InNav off)
                "block_44",             // the true_false_swipe knowledge_check (Show After Reading, not revealed)
            }, excluded);
            Assert.AreEqual(56, AllTargets(model).Distinct().Count(), "no block is a target twice");
            CollectionAssert.DoesNotContain(Keys(model.Sections), BuiltInFamilies.Meta, "meta is no section at all");

            // - rule 4: Listen has one target -> its item jumps to the player; every other section offers a choice
            var listen = model.Sections.Single(s => s.Key == BuiltInFamilies.Guides);
            Assert.AreEqual("block_55", listen.DirectTarget, "Listen jumps straight to the audio player");
            Assert.IsTrue(model.Sections.Where(s => s != listen).All(s => s.DirectTarget == null), "a section of several targets does not");

            // - each section labelled with its registered family's own words
            foreach (var section in model.Sections)
            {
                Assert.IsTrue(BlockRegistry.Shared.TryGetFamily(section.Key, out var family));
                Assert.AreEqual(family.LabelKey, section.LabelKey, section.Key + "'s label is its family's");
            }
        }

        // L2 / L3: a section's kinds partition its targets; a kind with one block jumps straight to it; the app's kind sits in its family
        [Test]
        public void TheLamp_EachSectionsKindsHoldItsTargets_AndAOneBlockKindSkipsTheThirdLevel()
        {
            var model = Model(ShippedByFamily(), Lamp);
            foreach (var section in model.Sections)
            {
                CollectionAssert.AreEquivalent(section.Targets, section.Kinds.SelectMany(k => k.Targets), section.Key + ": its kinds hold exactly its targets");
                Assert.AreEqual(section.Kinds.Count, section.Kinds.Select(k => k.Key).Distinct().Count(), section.Key + ": a kind appears once");
                foreach (var kind in section.Kinds)
                    Assert.AreEqual(kind.Targets.Count == 1 ? kind.Targets[0] : null, kind.DirectTarget, section.Key + "/" + kind.Key);
            }

            var about = model.Sections.Single(s => s.Key == BuiltInFamilies.About);
            var rich = about.Kinds.Single(k => k.Key == BuiltInBlocks.RichTextKind);
            Assert.AreEqual(4, rich.Targets.Count, "precondition: The Lamp has four rich_text blocks");
            Assert.IsNull(rich.DirectTarget, "four blocks: the kind opens its list (L3)");
            Assert.AreEqual("block_20", about.Kinds.Single(k => k.Key == BuiltInBlocks.ProcessStepsKind).DirectTarget, "one block: straight to it");

            // - the app's own kind (LivingRoom's size_comparison) is navigated like a built-in one, in the family it declares
            var appKind = about.Kinds.Single(k => k.Key == "size_comparison");
            CollectionAssert.AreEqual(new[] { "block_53" }, appKind.Targets);
        }

        // The default Block Order keeps the authored order (meta last): the SAME targets, and the sections follow the stack
        [Test]
        public void TheLampInTheDefaultBlockOrder_HasTheSameTargets_AndItsSectionsFollowWhatTheVisitorScrolls()
        {
            var config = Shipped();
            Assert.AreEqual(CardOptions.BlockOrderEndFamiliesLast, config.card_settings.navigation.block_order, "precondition: the shipped default");
            var entries = Stack(config, Lamp);
            var model = CardNavModel.Build(entries, BlockRegistry.Shared, config.card_settings);
            var byFamily = Model(ShippedByFamily(), Lamp);

            CollectionAssert.AreEquivalent(AllTargets(byFamily), AllTargets(model), "Block Order moves blocks, it never adds or drops a target");
            CollectionAssert.AreNotEqual(Keys(byFamily.Sections), Keys(model.Sections),
                "precondition (not vacuous): in the authored order the families interleave, so the section order really differs");

            var at = entries.Select((e, i) => (e.Instance.key, i)).ToDictionary(p => p.key, p => p.i);
            for (int s = 1; s < model.Sections.Count; s++)
                Assert.Less(at[model.Sections[s - 1].Targets[0]], at[model.Sections[s].Targets[0]],
                    "section " + s + " (" + model.Sections[s].Key + ") starts further down the stack than the one before it");
            foreach (var section in model.Sections)
                for (int t = 1; t < section.Targets.Count; t++)
                    Assert.Less(at[section.Targets[t - 1]], at[section.Targets[t]], section.Key + ": targets in the stack's order");
        }

        [Test]
        public void GroupByFormat_RegroupsTheSameTargets_NoneLost_EachUnderItsKindsFormat()
        {
            var config = ShippedByFamily();
            var byFamily = Model(config, Lamp);
            config.card_settings.navigation.group_by = CardOptions.GroupByFormat;
            var entries = Stack(config, Lamp);
            var byFormat = CardNavModel.Build(entries, BlockRegistry.Shared, config.card_settings);

            Assert.AreEqual(CardOptions.GroupByFormat, byFormat.GroupBy);
            CollectionAssert.AreEquivalent(AllTargets(byFamily), AllTargets(byFormat), "the identical multiset of 56 targets");
            Assert.AreEqual(byFamily.TargetCount, byFormat.TargetCount);
            Assert.AreEqual(byFamily.ShowsNavigation, byFormat.ShowsNavigation, "the pivot never changes whether the card has navigation");
            Assert.AreEqual(6, byFormat.Sections.Count, "precondition (not vacuous): The Lamp has a block of each of the six formats");

            var kindOf = entries.ToDictionary(e => e.Instance.key, e => e.Definition);
            foreach (var section in byFormat.Sections)
            {
                CollectionAssert.Contains(CardOptions.Formats, section.Key);
                Assert.Greater(section.Count, 0, "no empty section");
                foreach (string target in section.Targets)
                    Assert.AreEqual(section.Key, kindOf[target].Format, target + " sits under its kind's format");
                CollectionAssert.Contains(CardStrings.Keys.All, section.LabelKey, section.Key + ": labelled by a CardStrings key");
            }
            Assert.AreEqual(byFormat.Sections.Count, byFormat.Sections.Select(s => s.LabelKey).Distinct().Count(), "one label per format");
            Assert.AreEqual(CardStrings.Keys.FormatWatch, byFormat.Sections.Single(s => s.Key == CardOptions.FormatWatch).LabelKey);
            Assert.AreEqual(2, byFormat.Sections.Single(s => s.Key == CardOptions.FormatWatch).Count, "The Lamp's two videos");

            config.card_settings.navigation.group_by = "Format";
            Assert.AreEqual(CardOptions.GroupByFamily, CardNavModel.Build(entries, BlockRegistry.Shared, config.card_settings).GroupBy,
                "a value that is not exactly 'format' falls back to the family default");
        }

        // Step 8 (grid_overlay's Purpose / Format switch): CardNavModel.By answers the OTHER axis from the same model -- exactly what a model
        // built on that axis says (the same sections in the same order, the same kinds and targets), never a regrouping of its own; its own
        // axis is itself, the pivot is built once and leads back, and anything that is not exactly "format" reads as the families
        [Test]
        public void By_TheOtherAxis_IsExactlyTheModelBuiltOnIt_OnceAndBack()
        {
            var config = ShippedByFamily();
            var byFamily = Model(config, Lamp);
            config.card_settings.navigation.group_by = CardOptions.GroupByFormat;
            var built = Model(config, Lamp);
            Assert.AreEqual(6, built.Sections.Count, "precondition (not vacuous): six formats on The Lamp");

            var pivot = byFamily.By(CardOptions.GroupByFormat);
            Assert.AreEqual(CardOptions.GroupByFormat, pivot.GroupBy);
            CollectionAssert.AreEqual(Keys(built.Sections), Keys(pivot.Sections), "the same sections, in the same order");
            for (int i = 0; i < built.Sections.Count; i++)
            {
                CollectionAssert.AreEqual(built.Sections[i].Targets, pivot.Sections[i].Targets, built.Sections[i].Key + ": the same targets");
                CollectionAssert.AreEqual(built.Sections[i].Kinds.Select(k => k.Key), pivot.Sections[i].Kinds.Select(k => k.Key), built.Sections[i].Key + ": the same kinds");
                Assert.AreEqual(built.Sections[i].LabelKey, pivot.Sections[i].LabelKey);
            }
            CollectionAssert.AreEqual(byFamily.Targets, pivot.Targets, "the card's targets do not change with the axis");
            Assert.AreEqual(byFamily.ShowsNavigation, pivot.ShowsNavigation);
            foreach (string target in pivot.Targets)
                Assert.AreEqual(pivot.Sections[pivot.SectionOf(target)].Key, built.Sections[built.SectionOf(target)].Key, target + ": in the same format");

            Assert.AreSame(byFamily, byFamily.By(CardOptions.GroupByFamily), "its own axis is itself");
            Assert.AreSame(pivot, byFamily.By(CardOptions.GroupByFormat), "the pivot is built once");
            Assert.AreSame(byFamily, pivot.By(CardOptions.GroupByFamily), "and leads back to the model it came from");
            Assert.AreSame(byFamily, byFamily.By("Format"), "anything but exactly 'format' reads as the families");
            CollectionAssert.AreEqual(Keys(byFamily.Sections), Keys(built.By(CardOptions.GroupByFamily).Sections),
                "and a wall Grouped By Format pivots to the same families");
        }

        // The reveal comes from the card's real local state, scoped to this point: rebuilt after it, the model gains the block
        [Test]
        public void AShowAfterReadingBlock_IsNoTargetUntilRevealed_AndTheModelRebuiltAfterTheRevealGainsIt()
        {
            var config = ShippedByFamily();
            var state = new CardLocalState(new MemoryCardStateStore(), config.wall_id);
            Func<string, bool> seen = key => state.Seen(Lamp, key);
            var gated = Stack(config, Lamp).Single(e => e.Instance.key == "block_44");
            Assert.IsTrue(new BlockFieldReader(gated.Instance, null, null).Flag(BuiltInBlocks.KnowledgeCheckShowAfterViewedField),
                "precondition: block_44 has Show After Reading on");

            var before = Model(config, Lamp, seen);
            Assert.AreEqual(56, before.TargetCount);
            Assert.AreEqual(4, before.Sections.Single(s => s.Key == BuiltInFamilies.Play).Count);

            state.MarkSeen(LampMilitary, "block_44");
            Assert.AreEqual(56, Model(config, Lamp, seen).TargetCount, "a reveal on ANOTHER point is not this card's");

            state.MarkSeen(Lamp, "block_44");
            var after = Model(config, Lamp, seen);
            Assert.AreEqual(57, after.TargetCount);
            CollectionAssert.AreEqual(new[] { "block_44" }, AllTargets(after).Except(AllTargets(before)), "the revealed block, and only it, joined");
            var play = after.Sections.Single(s => s.Key == BuiltInFamilies.Play);
            CollectionAssert.AreEqual(new[] { "block_43", "block_44", "block_45", "block_48", "block_49" }, play.Targets,
                "Play has 5 targets, the revealed one in its place in the stack");
            CollectionAssert.AreEqual(new[] { "block_43", "block_44", "block_45" },
                play.Kinds.Single(k => k.Key == BuiltInBlocks.KnowledgeCheckKind).Targets);
        }

        // _3.2 4.4 says 5 targets (and the brief 6): the hero chip is pinned top, so rule 1 leaves 4 -- either way no navigation
        [Test]
        public void LampMilitary_HasFourTargets_BelowShowNavigationFrom_SoNoNavigation_AndItsListenFamilyIsHidden()
        {
            var config = Shipped();
            var entries = Stack(config, LampMilitary);
            Assert.AreEqual(6, entries.Count, "precondition: all six authored blocks build");
            var heroChip = entries.Single(e => e.Definition.Key == BuiltInBlocks.AudioGuideKind);
            Assert.IsTrue(heroChip.Definition.IsPinnedTop(heroChip.Variant), "precondition: its only audio block is the pinned hero chip");

            var model = CardNavModel.Build(entries, BlockRegistry.Shared, config.card_settings);
            CollectionAssert.AreEquivalent(new[] { "block_2", "block_3", "block_4", "block_6" }, AllTargets(model));
            Assert.AreEqual(4, model.TargetCount);
            Assert.IsFalse(model.ShowsNavigation, "4 < 8: the card scrolls, no navigation element");
            CollectionAssert.AreEqual(new[] { BuiltInFamilies.About, BuiltInFamilies.Media }, Keys(model.Sections));
            CollectionAssert.AreEqual(new[] { 2, 2 }, model.Sections.Select(s => s.Count));
            CollectionAssert.DoesNotContain(Keys(model.Sections), BuiltInFamilies.Guides, "rule 2: a family whose only block is no target is hidden");
        }

        [Test]
        public void LampReligious_OnlyItsHeader_HasNoTargetNoSectionAndNoNavigation()
        {
            var config = Shipped();
            var entries = Stack(config, LampReligious);
            Assert.AreEqual(1, entries.Count, "precondition: the header alone");
            var model = CardNavModel.Build(entries, BlockRegistry.Shared, config.card_settings);
            Assert.AreEqual(0, model.TargetCount);
            CollectionAssert.IsEmpty(model.Sections);
            CollectionAssert.IsEmpty(model.BarSections);
            CollectionAssert.IsEmpty(model.MoreSections);
            Assert.IsFalse(model.ShowsNavigation);
            Assert.IsFalse(CardNavModel.Build(null, BlockRegistry.Shared, null).ShowsNavigation, "no stack at all: an empty model, never a throw");
        }

        // The shared registry's families and kinds, the very same definitions, each drawn by a plain view (what a block draws is beside
        // the point of a test about which blocks the stack hides)
        private static BlockRegistry PlainCopyOfTheSharedRegistry()
        {
            var plain = new BlockRegistry();
            foreach (var kind in BlockRegistry.Shared.Ordered)
            {
                if (!plain.TryGetFamily(kind.Family, out _) && BlockRegistry.Shared.TryGetFamily(kind.Family, out var family)) plain.RegisterFamily(family);
                plain.Register(kind, () => new PlainView());
            }
            return plain;
        }

        // _3.2 step 3 (its [step 3] TODO): the REAL stack hides a block on the SAME call the model leaves it out on
        // (NavOntologyRule.WaitsToBeRevealed) with the SAME predicate (the stack's IsRevealed, which the sheet hands CardNavModel.Build),
        // so the stack and the navigation can never disagree -- for The Lamp's waiting block, and for it once revealed
        [Test]
        public void TheStackAndTheModel_AnswerAWaitingBlockWithOneCall_BeforeAndAfterItsReveal()
        {
            var config = ShippedByFamily();
            var registry = PlainCopyOfTheSharedRegistry();
            var lamp = config.pois.Single(p => p.id == Lamp);
            var entries = BlockStackBuilder.Build(lamp, config.card_settings, registry, config.pois).Entries;
            var gated = entries.Single(e => e.Instance.key == "block_44");
            var state = new CardLocalState(new MemoryCardStateStore(), config.wall_id);
            var stack = new BlockStackView(new VisualElement(), new VisualElement(), registry);

            foreach (bool revealed in new[] { false, true })
            {
                if (revealed) state.MarkSeen(Lamp, "block_44");
                stack.Bind(entries, new BlockBindContext { Poi = lamp, Settings = config.card_settings, State = state, Language = "en", FallbackLanguage = "en" });
                var model = CardNavModel.Build(entries, registry, config.card_settings, stack.IsRevealed);
                string when = revealed ? "after its reveal" : "before its reveal";

                bool waits = NavOntologyRule.WaitsToBeRevealed(gated, stack.IsRevealed);
                Assert.AreEqual(!revealed, waits, "precondition, " + when + ": block_44 (Show After Reading) waits exactly until it is revealed");
                Assert.AreEqual(waits, stack.SlotOfBlock("block_44").style.display.value == DisplayStyle.None, when + ": the stack hides it on that answer");
                Assert.AreEqual(!waits, AllTargets(model).Contains("block_44"), when + ": the model leaves it out on that same answer");
                // - a look pinned to the top (the hero chip) is drawn in the hero since _3.3 P4.4a: it has no slot to hide
                foreach (var entry in entries.Where(e => !e.Definition.IsPinnedTop(e.Variant)))
                    Assert.AreEqual(NavOntologyRule.WaitsToBeRevealed(entry, stack.IsRevealed),
                        stack.SlotOfBlock(entry.Instance.key).style.display.value == DisplayStyle.None, when + ": every slot, " + entry.Instance.key);
            }
        }

        // An app registers a family between two built-in ones (Order 35) and a kind in it: it is navigated like any other
        [Test]
        public void AnAppFamilyAndItsKind_GetTheirOwnSection_WhereTheFamilysOrderPutsIt_LabelledByTheAppsKey()
        {
            var registry = new BlockRegistry();
            BuiltInBlocks.Register(registry);
            registry.RegisterFamily(new FamilyDefinition { Key = "workshop", Order = 35, LabelKey = "app_family_workshop" });
            registry.Register(new BlockKindDefinition
            {
                Key = "workshop_note", Family = "workshop", Format = CardOptions.FormatRead, IconKey = CardIcons.Book, DisplayName = "Workshop note",
                Variants = new[] { "plain" }, DefaultVariant = "plain", DisplayModes = new[] { CardOptions.DisplayInline },
            }, () => new PlainView());

            var config = ShippedByFamily();
            var lamp = config.pois.Single(p => p.id == Lamp);
            // - two app blocks authored early in The Lamp's card (an in-memory copy; no identity field touched): by_family moves them
            lamp.card.blocks.Insert(3, new BlockInstanceData { key = "block_app_1", kind = "workshop_note" });
            lamp.card.blocks.Insert(8, new BlockInstanceData { key = "block_app_2", kind = "workshop_note" });

            var model = CardNavModel.Build(Stack(config, Lamp, registry), registry, config.card_settings);
            var keys = Keys(model.Sections).ToList();
            var workshop = model.Sections.Single(s => s.Key == "workshop");
            CollectionAssert.AreEqual(new[] { "block_app_1", "block_app_2" }, workshop.Targets);
            CollectionAssert.AreEqual(new[] { "workshop_note" }, workshop.Kinds.Select(k => k.Key));
            Assert.AreEqual("app_family_workshop", workshop.LabelKey, "the app's own label key");
            Assert.AreEqual(keys.IndexOf(BuiltInFamilies.Guides) + 1, keys.IndexOf("workshop"), "right after Listen (30)");
            Assert.AreEqual(keys.IndexOf("workshop") + 1, keys.IndexOf(BuiltInFamilies.Stories), "right before Stories (40)");
        }

        // Show Navigation From on the real counts: exactly at Lamp - Military's 4 targets (inside the 2..30 range), and the clamp --
        // any value above 30 reads as 30, so The Lamp's 56 targets always show; a 0 reads as 2, so a card with none never does
        [TestCase(LampMilitary, 4, true), TestCase(LampMilitary, 5, false), TestCase(LampMilitary, 8, false)]
        [TestCase(Lamp, 30, true), TestCase(Lamp, 57, true), TestCase(Lamp, 100, true)]
        [TestCase(LampReligious, 0, false), TestCase(LampReligious, 2, false)]
        public void ShowNavigationFrom_TurnsTheNavigationOnExactlyAtItsCount(string poiId, int minBlocks, bool shows)
        {
            var config = Shipped();
            config.card_settings.navigation.min_blocks = minBlocks;
            Assert.AreEqual(shows, Model(config, poiId).ShowsNavigation, poiId + " with Show Navigation From " + minBlocks);
        }

        // Bar Slots on The Lamp's 8 sections: the bar shows (slots - 1) and More names EXACTLY the rest, in order
        [TestCase(5, 4), TestCase(4, 3), TestCase(3, 2), TestCase(9, 4), TestCase(0, 2)]
        public void BarSlots_TurnTheLastSlotIntoMore_ListingExactlyTheOverflowSections(int maxSlots, int inBar)
        {
            var config = ShippedByFamily();
            config.card_settings.navigation.max_slots = maxSlots;
            var model = Model(config, Lamp);
            Assert.AreEqual(8, model.Sections.Count, "precondition: more sections than any bar holds");

            CollectionAssert.AreEqual(Keys(model.Sections.Take(inBar)), Keys(model.BarSections), "the first sections fill the bar");
            CollectionAssert.AreEqual(Keys(model.Sections.Skip(inBar)), Keys(model.MoreSections), "More lists exactly the others, in order");
            if (maxSlots == 5)
                CollectionAssert.AreEqual(new[] { BuiltInFamilies.Ar, BuiltInFamilies.Play, BuiltInFamilies.Visit, BuiltInFamilies.Community },
                    Keys(model.MoreSections), "the default 5 slots: About, Media, Listen, Stories + More");
        }

        // When the sections fit, no More: the Block Library switches off AR and Play's kinds -> 6 sections; Community too -> 5
        [Test]
        public void BarSlots_WithEverySectionFitting_ShowAllAndNoMore()
        {
            var config = ShippedByFamily();
            foreach (string kind in new[] { BuiltInBlocks.ShowOnWallKind, BuiltInBlocks.PlaceInArKind, BuiltInBlocks.KnowledgeCheckKind,
                         BuiltInBlocks.PollKind, BuiltInBlocks.CollectKind, BuiltInBlocks.FeedbackKind })
                config.card_settings.kinds.Add(new BlockKindSetting { kind = kind, enabled = false });
            var model = Model(config, Lamp);
            Assert.AreEqual(5, model.Sections.Count, "precondition: About, Media, Listen, Stories, Visit");
            CollectionAssert.AreEqual(Keys(model.Sections), Keys(model.BarSections), "5 sections in 5 slots");
            CollectionAssert.IsEmpty(model.MoreSections, "nothing overflows, so no More slot");

            var military = Model(config, LampMilitary);
            CollectionAssert.AreEqual(Keys(military.Sections), Keys(military.BarSections), "2 sections always fit");
            CollectionAssert.IsEmpty(military.MoreSections);
        }
    }
}
