using System.Collections.Generic;
using NUnit.Framework;

namespace TileStories.Editor.Tests
{
    // NavOntologyRule (_3.2 step 2, section 2.5) case by case, on the REAL built-in kind definitions: which block is a navigation
    // target (rule 1, each exclusion read from the kind's own definition), Show Navigation From (rule 3, with its clamp), the direct
    // jump of a single target (rule 4) and the bar's More slot (rule 5, with its clamp). CardNavModelTests runs them on The Lamp.
    public class NavOntologyRuleTests
    {
        private static BlockKindDefinition Def(string kind)
        {
            Assert.IsTrue(BlockRegistry.Shared.TryGet(kind, out var definition), "precondition: " + kind + " is a registered kind");
            return definition;
        }

        private static BlockStackBuilder.Entry Entry(string kind, string variant, string key = "block_x", bool showAfterViewed = false)
        {
            var instance = new BlockInstanceData { key = key, kind = kind, variant = variant };
            if (showAfterViewed) instance.fields.Add(new BlockFieldValue { key = BuiltInBlocks.KnowledgeCheckShowAfterViewedField, flag = true });
            return new BlockStackBuilder.Entry(instance, Def(kind), variant, synthesized: false);
        }

        private static bool IsTarget(BlockStackBuilder.Entry entry, System.Func<string, bool> revealed = null) =>
            NavOntologyRule.IsTarget(entry, BlockRegistry.Shared, revealed);

        [Test]
        public void TheHeader_IsNeverATarget_AuthoredOrMadeFromTheName()
        {
            Assert.IsFalse(IsTarget(Entry(BuiltInBlocks.HeaderKind, "compact")), "the card's identity: always on top, nowhere to jump");
            Assert.IsFalse(IsTarget(new BlockStackBuilder.Entry(new BlockInstanceData { key = "", kind = BuiltInBlocks.HeaderKind },
                Def(BuiltInBlocks.HeaderKind), "compact", synthesized: true)), "the header made from name + summary neither");
        }

        // The kind's own PinnedTopVariants / FooterVariants decide, per LOOK: the same kind is a target in its other looks
        [Test]
        public void APinnedTopOrFooterLook_IsNoTarget_WhileTheSameKindsOtherLookIs()
        {
            Assert.IsTrue(Def(BuiltInBlocks.AudioGuideKind).IsPinnedTop(BuiltInBlocks.AudioGuideHeroChip), "precondition: hero_chip is pinned top");
            Assert.IsFalse(IsTarget(Entry(BuiltInBlocks.AudioGuideKind, BuiltInBlocks.AudioGuideHeroChip)), "pinned under the header");
            Assert.IsTrue(IsTarget(Entry(BuiltInBlocks.AudioGuideKind, "player")), "the player look scrolls with the card");

            Assert.IsTrue(Def(BuiltInBlocks.ActionsKind).IsFooter(BuiltInBlocks.ActionsStickyCta), "precondition: sticky_cta is a footer look");
            Assert.IsFalse(IsTarget(Entry(BuiltInBlocks.ActionsKind, BuiltInBlocks.ActionsStickyCta)), "pinned in the footer");
            Assert.IsTrue(IsTarget(Entry(BuiltInBlocks.ActionsKind, "pill_row")), "the pill row scrolls with the card");
        }

        [Test]
        public void ABlockWaitingForShowAfterViewed_IsATargetOnlyOnceRevealed()
        {
            var gated = Entry(BuiltInBlocks.KnowledgeCheckKind, "multiple_choice", key: "block_q", showAfterViewed: true);
            Assert.IsFalse(IsTarget(gated), "nothing revealed yet (no reveal state at all)");
            Assert.IsFalse(IsTarget(gated, key => false), "its gate has not opened");
            Assert.IsFalse(IsTarget(gated, key => key == "block_other"), "another block's reveal is not this one's");
            Assert.IsTrue(IsTarget(gated, key => key == "block_q"), "revealed: now a target");

            var ungated = Entry(BuiltInBlocks.KnowledgeCheckKind, "multiple_choice", key: "block_q", showAfterViewed: false);
            Assert.IsTrue(IsTarget(ungated), "the same kind with Show After Reading off never waits");
            Assert.IsFalse(NavOntologyRule.WaitsToBeRevealed(Entry(BuiltInBlocks.RichTextKind, "plain"), null), "a kind with no such field never waits");
        }

        [Test]
        public void AFamilyThatIsNoNavigationTarget_KeepsItsBlocksOut()
        {
            Assert.IsTrue(BlockRegistry.Shared.TryGetFamily(BuiltInFamilies.Meta, out var meta) && !meta.InNav, "precondition: meta is InNav off");
            Assert.IsFalse(IsTarget(Entry(BuiltInBlocks.SourcesKind, "list")), "the credits close the card; no element jumps to them");
            Assert.IsTrue(IsTarget(Entry(BuiltInBlocks.RichTextKind, "plain")), "an about block is a target");

            // - InNav read from the REGISTERED family, so an app family registered off is out too
            var registry = new BlockRegistry();
            registry.RegisterFamily(new FamilyDefinition { Key = BuiltInFamilies.About, Order = 10, LabelKey = CardStrings.Keys.FamilyAbout, InNav = false });
            Assert.IsFalse(NavOntologyRule.IsTarget(Entry(BuiltInBlocks.RichTextKind, "plain"), registry, null), "the family's own flag decides");
            Assert.IsFalse(NavOntologyRule.IsTarget(Entry(BuiltInBlocks.RichTextKind, "plain"), new BlockRegistry(), null), "an unknown family is no target");
            Assert.IsFalse(NavOntologyRule.IsTarget(Entry(BuiltInBlocks.RichTextKind, "plain"), null, null), "no registry: nothing is known");
        }

        [TestCase(null, CardOptions.GroupByFamily)]
        [TestCase("", CardOptions.GroupByFamily)]
        [TestCase("Format", CardOptions.GroupByFamily)]
        [TestCase("family", CardOptions.GroupByFamily)]
        [TestCase("format", CardOptions.GroupByFormat)]
        public void GroupBy_IsFormatOnlyWhenTheWallSaysSo(string stored, string expected)
        {
            Assert.AreEqual(expected, NavOntologyRule.GroupBy(new CardNavigationSettings { group_by = stored }));
            Assert.AreEqual(CardOptions.GroupByFamily, NavOntologyRule.GroupBy(null), "no settings: the default");
        }

        // Rule 3 at its boundary, on the default (8) and through the clamp (2..30)
        [TestCase(8, 7, false), TestCase(8, 8, true)]
        [TestCase(0, 1, false), TestCase(0, 2, true)]
        [TestCase(1, 1, false), TestCase(1, 2, true)]
        [TestCase(30, 29, false), TestCase(30, 30, true)]
        [TestCase(100, 29, false), TestCase(100, 30, true)]
        public void ShowsNavigation_FromMinBlocksTargets_KeptInsideItsRange(int minBlocks, int targets, bool shows)
        {
            Assert.AreEqual(shows, NavOntologyRule.ShowsNavigation(targets, new CardNavigationSettings { min_blocks = minBlocks }));
        }

        [Test]
        public void TheLimits_ComeFromTheSettingsType_AndNoSettingsReadsItsDefaults()
        {
            Assert.AreEqual(new CardNavigationSettings().min_blocks, NavOntologyRule.MinBlocks(null));
            Assert.AreEqual(new CardNavigationSettings().max_slots, NavOntologyRule.MaxSlots(null));
            Assert.AreEqual(CardNavigationSettings.MinBlocksMin, NavOntologyRule.MinBlocks(new CardNavigationSettings { min_blocks = -4 }));
            Assert.AreEqual(CardNavigationSettings.MinBlocksMax, NavOntologyRule.MinBlocks(new CardNavigationSettings { min_blocks = 99 }));
            Assert.AreEqual(CardNavigationSettings.MaxSlotsMin, NavOntologyRule.MaxSlots(new CardNavigationSettings { max_slots = 0 }));
            Assert.AreEqual(CardNavigationSettings.MaxSlotsMax, NavOntologyRule.MaxSlots(new CardNavigationSettings { max_slots = 9 }));
            Assert.AreEqual(4, NavOntologyRule.MaxSlots(new CardNavigationSettings { max_slots = 4 }), "a value in range is kept");
        }

        [Test]
        public void DirectTarget_IsTheOnlyTarget_OrNothingWhenThereIsAChoice()
        {
            Assert.IsNull(NavOntologyRule.DirectTarget(null));
            Assert.IsNull(NavOntologyRule.DirectTarget(new List<string>()));
            Assert.AreEqual("block_55", NavOntologyRule.DirectTarget(new List<string> { "block_55" }));
            Assert.IsNull(NavOntologyRule.DirectTarget(new List<string> { "block_54", "block_55" }));
        }

        // Rule 5: every section fits -> all of them; one too many -> the last slot is More, so one section fewer shows
        [TestCase(0, 5, 0), TestCase(4, 5, 4), TestCase(5, 5, 5), TestCase(6, 5, 4), TestCase(8, 5, 4)]
        [TestCase(3, 3, 3), TestCase(4, 3, 2), TestCase(8, 3, 2)]
        public void SectionsInBar_AllWhenTheyFit_ElseOneSlotGoesToMore(int sections, int slots, int inBar)
        {
            Assert.AreEqual(inBar, NavOntologyRule.SectionsInBar(sections, slots));
        }

        // Rule 7: an element between sections needs at least two of them -- one tab is not a choice
        [TestCase(0, false)]
        [TestCase(1, false)]
        [TestCase(2, true)]
        [TestCase(8, true)]
        public void OffersASectionChoice_FromTwoSections(int sections, bool offers)
        {
            Assert.AreEqual(offers, NavOntologyRule.OffersASectionChoice(sections));
        }

        // Rule 7 belongs to the ELEMENTS that move between sections, not to the card: a one-family card with enough targets still SHOWS
        // navigation, so an element that lists blocks (a contents list, step 11) keeps working on it. ShowsNavigation reads the targets
        // only -- the same answer for one section as for eight
        [Test]
        public void ShowsNavigation_DoesNotReadRule7_AOneFamilyCardStillShowsNavigation()
        {
            var settings = new CardNavigationSettings { min_blocks = 3 };
            Assert.IsTrue(NavOntologyRule.ShowsNavigation(5, settings), "five targets in ONE section: navigation is shown");
            Assert.IsFalse(NavOntologyRule.OffersASectionChoice(1), "while an element between sections has nothing to choose from");
        }
    }
}
