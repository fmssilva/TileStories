using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // The card's layouts (_3.2 section 6.1, step 13) as pure rules, on the SHIPPED LivingRoom wall through the real BlockStackBuilder and
    // registry: which layout a card is really drawn in (tabs needs sections to switch between), which blocks the accordion draws as rows,
    // what a jump does first per layout, what the spy names, the Text Tabs fallback, Open One At A Time (AccordionRule, shared with the
    // contents list), where the stack rests after a row closes, the contents block's Rows rule, and the spy at the end of a tabs card.
    public class CardLayoutRuleTests
    {
        private const string Lamp = "lamp";
        private const string LampMilitary = "lamp_military";
        private const string LampReligious = "lamp_religious";

        private static WallConfigData Shipped() =>
            JsonUtility.FromJson<WallConfigData>(File.ReadAllText("Assets/Apps/LivingRoom/config.json"));

        private static List<BlockStackBuilder.Entry> Stack(WallConfigData config, string poiId) =>
            BlockStackBuilder.Build(config.pois.Single(p => p.id == poiId), config.card_settings, BlockRegistry.Shared, config.pois).Entries;

        private static CardNavModel Model(WallConfigData config, string poiId) =>
            CardNavModel.Build(Stack(config, poiId), BlockRegistry.Shared, config.card_settings);

        private static CardNavigationSettings Nav(string layout, bool oneOpen = false, string top = null, string bottom = null)
        {
            var nav = new CardNavigationSettings { layout = layout, accordion_one_open = oneOpen };
            if (top != null) nav.top = top;
            if (bottom != null) nav.bottom = bottom;
            return nav;
        }

        [Test]
        public void TheLayout_IsScrollByDefault_AndForAnythingUnknown()
        {
            Assert.AreEqual(CardOptions.LayoutScroll, new CardNavigationSettings().layout, "the shipped default");
            Assert.AreEqual(CardOptions.LayoutScroll, CardLayoutRule.Layout(null));
            Assert.AreEqual(CardOptions.LayoutScroll, CardLayoutRule.Layout(Nav("pages")), "a later step's layout reads as scroll until it is built");
            Assert.AreEqual(CardOptions.LayoutScroll, CardLayoutRule.Layout(Nav("")));
            Assert.AreEqual(CardOptions.LayoutAccordion, CardLayoutRule.Layout(Nav(CardOptions.LayoutAccordion)));
            Assert.AreEqual(CardOptions.LayoutTabs, CardLayoutRule.Layout(Nav(CardOptions.LayoutTabs)));
        }

        // Tabs needs something to switch between: The Lamp (56 targets, 8 sections) is drawn in tabs; Lamp - Military (4 targets, under Show
        // Navigation From) and Lamp - Religious (the header alone) scroll -- they would otherwise hide blocks behind a bar they never draw
        [Test]
        public void Tabs_IsUsedOnlyWhereTheCardHasSectionsToSwitchBetween_AndTheAccordionEverywhere()
        {
            var config = Shipped();
            config.card_settings.navigation.layout = CardOptions.LayoutTabs;
            var lamp = Model(config, Lamp);
            Assert.IsTrue(lamp.ShowsNavigation && lamp.Sections.Count >= 2, "precondition: The Lamp shows navigation over several sections");
            Assert.AreEqual(CardOptions.LayoutTabs, CardLayoutRule.Effective(config.card_settings.navigation, lamp));
            var military = Model(config, LampMilitary);
            Assert.IsFalse(military.ShowsNavigation, "precondition: Lamp - Military is under Show Navigation From");
            Assert.AreEqual(CardOptions.LayoutScroll, CardLayoutRule.Effective(config.card_settings.navigation, military));
            Assert.AreEqual(CardOptions.LayoutScroll, CardLayoutRule.Effective(config.card_settings.navigation, Model(config, LampReligious)));
            // - Show Navigation From lowered to 2: Military shows navigation, and its sections decide (one tab is not a choice)
            config.card_settings.navigation.min_blocks = 2;
            var lowered = Model(config, LampMilitary);
            Assert.AreEqual(lowered.Sections.Count >= 2 ? CardOptions.LayoutTabs : CardOptions.LayoutScroll,
                CardLayoutRule.Effective(config.card_settings.navigation, lowered), "rule 2.5.7 decides from its sections (" + lowered.Sections.Count + ")");

            config.card_settings.navigation.layout = CardOptions.LayoutAccordion;
            foreach (string poi in new[] { Lamp, LampMilitary, LampReligious })
                Assert.AreEqual(CardOptions.LayoutAccordion, CardLayoutRule.Effective(config.card_settings.navigation, Model(config, poi)), poi + ": rows need no bar");
        }

        // The accordion draws every navigation TARGET as a row -- counted with the reveal gate open, so the Show After Reading question is a
        // (hidden) row too -- and nothing else: the header, the pinned hero chip, the sticky CTA footer, the two sources (meta: no target)
        [Test]
        public void TheAccordion_DrawsEveryTargetAsARow_TheWaitingQuestionToo_AndNothingElse()
        {
            var config = Shipped();
            var entries = Stack(config, Lamp);
            Assert.AreEqual(62, entries.Count, "precondition: The Lamp's 62 blocks");
            var scroll = Nav(CardOptions.LayoutScroll);
            Assert.IsFalse(entries.Any(e => CardLayoutRule.ShowsCompact(scroll, e, BlockRegistry.Shared)), "scroll: the shipped blocks are all inline");

            var accordion = Nav(CardOptions.LayoutAccordion);
            var rows = entries.Where(e => CardLayoutRule.ShowsCompact(accordion, e, BlockRegistry.Shared)).Select(e => e.Instance.key).ToList();
            var targets = CardNavModel.Build(entries, BlockRegistry.Shared, config.card_settings, _ => true).Targets;
            Assert.AreEqual(57, targets.Count, "precondition: 56 targets, 57 once the question is revealed");
            CollectionAssert.AreEquivalent(targets, rows, "a row for exactly the targets, the waiting question included");
            CollectionAssert.IsSubsetOf(new[] { "block_44" }, rows, "the true / false question that waits for the reading");
            foreach (string whole in new[] { "block_1", "block_54", "block_19", "block_15", "block_16" })
                CollectionAssert.DoesNotContain(rows, whole, whole + " stays drawn whole (header, hero chip, sticky CTA, sources)");

            // - an Expandable block is a row in any layout (its own display)
            var expandable = entries.First(e => e.Instance.key == "block_2");
            expandable.Instance.display = CardOptions.DisplayExpandable;
            Assert.IsTrue(CardLayoutRule.ShowsCompact(scroll, expandable, BlockRegistry.Shared));
        }

        [Test]
        public void AJump_ShowsTheSectionFirstInTabs_AndOpensAClosedRowInAnyLayout()
        {
            Assert.IsTrue(CardLayoutRule.ShowsSectionBeforeJump(CardOptions.LayoutTabs));
            Assert.IsFalse(CardLayoutRule.ShowsSectionBeforeJump(CardOptions.LayoutScroll));
            Assert.IsFalse(CardLayoutRule.ShowsSectionBeforeJump(CardOptions.LayoutAccordion));
            Assert.IsTrue(CardLayoutRule.OpensRowBeforeJump(true), "a closed row opens: the visitor jumped there to see the block");
            Assert.IsFalse(CardLayoutRule.OpensRowBeforeJump(false), "a block drawn whole, or a row already open: just the scroll");
        }

        [Test]
        public void TheSpy_NamesTheShownSectionInTabs_AndWhatWasMeasuredElsewhere()
        {
            Assert.AreEqual(3, CardLayoutRule.SpySection(CardOptions.LayoutTabs, 3, 0), "tabs: the section shown, whatever the scroll measured");
            Assert.AreEqual(0, CardLayoutRule.SpySection(CardOptions.LayoutTabs, -1, 0), "tabs with nothing shown yet: the measure");
            Assert.AreEqual(5, CardLayoutRule.SpySection(CardOptions.LayoutScroll, 3, 5));
            Assert.AreEqual(5, CardLayoutRule.SpySection(CardOptions.LayoutAccordion, 3, 5));
            Assert.IsTrue(CardLayoutRule.ShowsWith(-1, 4), "a block of no section (sources, a nav block) shows with every section");
            Assert.IsTrue(CardLayoutRule.ShowsWith(4, 4));
            Assert.IsFalse(CardLayoutRule.ShowsWith(2, 4));
        }

        // Tabs with neither a Top Bar nor a Bottom Bar uses Text Tabs (and the Editor warns); any bar the wall set is kept
        [Test]
        public void TabsWithNoBar_UsesTextTabs_AnyBarTheWallSetIsKept_AndNoOtherLayoutChangesTheTopBar()
        {
            string none = CardOptions.NavNone;
            Assert.AreEqual(BuiltInNavElements.TextTabs, CardLayoutRule.TopElement(Nav(CardOptions.LayoutTabs, top: none, bottom: none)));
            Assert.IsTrue(CardLayoutRule.TabsWithoutAnElement(Nav(CardOptions.LayoutTabs, top: none, bottom: none)));
            Assert.AreEqual(none, CardLayoutRule.TopElement(Nav(CardOptions.LayoutTabs, top: none, bottom: "bottom_bar")), "a bottom bar switches sections");
            Assert.IsFalse(CardLayoutRule.TabsWithoutAnElement(Nav(CardOptions.LayoutTabs, top: none, bottom: "bottom_bar")));
            Assert.AreEqual("icon_tabs", CardLayoutRule.TopElement(Nav(CardOptions.LayoutTabs, top: "icon_tabs", bottom: none)));
            Assert.AreEqual(none, CardLayoutRule.TopElement(Nav(CardOptions.LayoutScroll, top: none, bottom: none)), "scroll: None stays None");
            Assert.IsFalse(CardLayoutRule.TabsWithoutAnElement(Nav(CardOptions.LayoutAccordion, top: none, bottom: none)));
            // - through the slot rule the card draws: The Lamp in tabs with both bars None holds Text Tabs at the top
            var config = Shipped();
            config.card_settings.navigation = Nav(CardOptions.LayoutTabs, top: none, bottom: none);
            var model = Model(config, Lamp);
            Assert.AreEqual(BuiltInNavElements.TextTabs, NavSlotRule.TopElement(config.card_settings.navigation, model,
                key => NavElementRegistry.Shared.TryGet(key, out var d) && d.Slot == NavSlotKeys.Top ? d : null));
        }

        [Test]
        public void OpenOneAtATime_ClosesTheOthers_OnlyInTheAccordion_AndBackClosesTheLastOpened()
        {
            Assert.IsTrue(CardLayoutRule.OneOpenAtATime(Nav(CardOptions.LayoutAccordion, oneOpen: true)));
            Assert.IsFalse(CardLayoutRule.OneOpenAtATime(Nav(CardOptions.LayoutAccordion, oneOpen: false)), "the default: rows open independently");
            Assert.IsFalse(CardLayoutRule.OneOpenAtATime(Nav(CardOptions.LayoutScroll, oneOpen: true)), "an accordion option: no effect elsewhere");
            Assert.IsFalse(CardLayoutRule.OneOpenAtATime(Nav(CardOptions.LayoutTabs, oneOpen: true)));

            var open = new List<string> { "block_2", "block_9", "block_22" };
            CollectionAssert.AreEqual(new[] { "block_2", "block_9" }, AccordionRule.ClosedByOpening(open, "block_22", oneAtATime: true),
                "every other open row, in the order they were opened");
            CollectionAssert.IsEmpty(AccordionRule.ClosedByOpening(open, "block_22", oneAtATime: false));
            CollectionAssert.IsEmpty(AccordionRule.ClosedByOpening(null, "block_22", oneAtATime: true));
            Assert.AreEqual("block_22", AccordionRule.ClosedByBack(open), "Back closes the row opened last");
            Assert.IsNull(AccordionRule.ClosedByBack(new List<string>()));
        }

        // NN/g mobile accordions: opening never scrolls; closing a row whose header scrolled off brings the header back; a row closed FOR
        // another (One At A Time) above it keeps the opened row where it is on screen
        [Test]
        public void AfterARowCloses_ItsHeaderComesBackIfItScrolledOff_AndARowOpenedForItKeepsItsPlace()
        {
            Assert.AreEqual(400f, AccordionRule.OffsetAfterCollapse(400f, 900f), "the header below the top: the stack stays");
            Assert.AreEqual(400f, AccordionRule.OffsetAfterCollapse(400f, 400f), "exactly at the top: stays");
            Assert.AreEqual(250f, AccordionRule.OffsetAfterCollapse(400f, 250f), "scrolled off above: the header back at the top");
            Assert.AreEqual(400f, AccordionRule.OffsetAfterCollapse(400f, float.NaN), "not laid out: nothing moves");

            Assert.AreEqual(100f, AccordionRule.OffsetAfterClosingFor(400f, 200f, 300f, 900f), "300 px removed above the opened row: taken off");
            Assert.AreEqual(0f, AccordionRule.OffsetAfterClosingFor(100f, 50f, 300f, 900f), "never above the stack's top");
            Assert.AreEqual(400f, AccordionRule.OffsetAfterClosingFor(400f, 1200f, 300f, 900f), "a row closed below the opened one moves nothing");
            Assert.AreEqual(400f, AccordionRule.OffsetAfterClosingFor(400f, 200f, 300f, float.NaN), "no opened row measured: nothing moves");
        }

        // The contents block's Rows: Sections First by default and for the block the card inserts (always), Every Block when a POI's block
        // or the wall's Block Library says so
        [Test]
        public void TheContentsRows_SectionsFirstByDefault_AlwaysForTheCardsOwnBlock_AndEveryBlockWhereAsked()
        {
            var config = Shipped();
            var kind = BuiltInBlocks.NavContents;
            var field = kind.Field(BuiltInBlocks.NavContentsRowsField);
            Assert.IsNotNull(field, "precondition: nav_contents has its Rows field");
            Assert.AreEqual(BlockFieldType.Choice, field.Type);
            Assert.IsTrue(field.LibraryDefault, "a wall default through the Block Library");
            Assert.AreEqual(BuiltInBlocks.NavContentsSectionsFirst, field.ChoiceDefault);

            var authored = new BlockStackBuilder.Entry(new BlockInstanceData { key = "block_90", kind = kind.Key }, kind, BuiltInBlocks.NavContentsList, false);
            Assert.IsTrue(NavContentsRule.SectionsFirst(authored, config.card_settings), "an authored block storing nothing: the default");

            config.card_settings.kinds.Add(new BlockKindSetting
            {
                kind = kind.Key,
                field_defaults = new List<BlockFieldDefault> { new() { key = field.Key, value = BuiltInBlocks.NavContentsEveryBlock } },
            });
            Assert.IsFalse(NavContentsRule.SectionsFirst(authored, config.card_settings), "the wall's Block Library default: Every Block");
            var synthesized = new BlockStackBuilder.Entry(new BlockInstanceData { key = BlockStackBuilder.AutoNavBlockKey(kind.Key), kind = kind.Key }, kind,
                BuiltInBlocks.NavContentsList, true);
            Assert.IsTrue(NavContentsRule.SectionsFirst(synthesized, config.card_settings), "the card's own block opens folded, whatever the wall says");

            authored.Instance.fields.Add(new BlockFieldValue { key = field.Key, value = BuiltInBlocks.NavContentsSectionsFirst });
            Assert.IsTrue(NavContentsRule.SectionsFirst(authored, config.card_settings), "the block's own Rows wins over the wall's");

            Assert.AreEqual(2, NavContentsRule.ReadCount(new[] { "a", "b", "c" }, k => k != "b"));
            Assert.AreEqual(0, NavContentsRule.ReadCount(null, _ => true));
        }

        // Tabs sets the other sections aside: their first targets are not laid out (infinite tops). At the end of the shown section the spy
        // must name the last section that IS laid out, never a hidden one
        [Test]
        public void AtTheEnd_TheSpyNamesTheLastLaidOutSection_NeverAHiddenOne()
        {
            float inf = float.PositiveInfinity;
            var tops = new List<float> { inf, inf, 120f, inf, inf };
            Assert.AreEqual(2, NavScrollRule.CurrentSection(tops, 900f, 1500f, 600f), "at the end of a tabs card showing section 2");
            Assert.AreEqual(4, NavScrollRule.CurrentSection(new List<float> { 0f, 100f, 200f, 300f, 400f }, 900f, 1500f, 600f),
                "every section laid out (scroll): the last, as before");
            Assert.AreEqual(0, NavScrollRule.CurrentSection(new List<float> { 0f, float.NaN }, 900f, 1500f, 600f), "a NaN top counts as not laid out");
        }
    }
}
