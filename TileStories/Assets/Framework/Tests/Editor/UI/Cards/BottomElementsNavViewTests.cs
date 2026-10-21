using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories.Editor.Tests
{
    // The step 7b bottom elements (_3.2 section 5.2) on a REAL sheet with The Lamp's real card (Grouped By Family: 8 sections, 56 targets)
    // and the shipped words. Edit Mode has no panel and no layout, so no jump can run here: this fixture proves what holds WITHOUT one --
    // the structure each element builds from the model, its words in both languages, and the decisions a tap makes while the card is
    // still on its first family (the card opens there, so a tap on it IS a "second tap"). Everything that needs the card to MOVE -- the
    // first tap jumping, the spy leaving a family, Next walking the card, the last block -- is the PlayMode gallery's and
    // PoiCardBottomElementsSceneTests' (real layout, real fingers).
    //   - dock: the bar's slots and More under the dock's class, the stylesheet (not Labels) deciding which words show
    //   - bar_sheet: the kinds list closed at first, between More's list and the slots; a tap on the family being read opens ITS kinds with
    //     their counts, in the model's order; a third tap closes them; a pick, More and a rebind close them; the arrow only on that slot
    //   - next_up: it names the second target (the card opens on the first) with the heading the stack draws over it, its place among all
    //     the targets, the Next button -- in English and Portuguese
    public class BottomElementsNavViewTests
    {
        private const string FrameworkStringsPath = "Assets/Framework/Runtime/UI/Cards/CardStrings.asset";
        private const string Lamp = "lamp";

        // - the shipped wall with the framework navigation: these tests measure the framework
        private static WallConfigData Shipped() => ShippedWall.WithFrameworkNavigation();

        // The shipped wall with `element` as its Bottom Bar, no Top Bar, the blocks Grouped By Family, five slots
        private static WallConfigData With(string element)
        {
            var wall = Shipped();
            var nav = wall.card_settings.navigation;
            nav.bottom = element;
            nav.top = CardOptions.NavNone;
            nav.block_order = CardOptions.BlockOrderByFamily;
            nav.max_slots = 5;
            return wall;
        }

        private static PoiCardSheetView Sheet() => new(new VisualElement(), BlockRegistry.Shared, Array.Empty<StyleSheet>());

        private static CardStrings Words(string language) =>
            new(AssetDatabase.LoadAssetAtPath<CardStringTable>(FrameworkStringsPath).Entries(), CardStringSources.Shared.Entries(), null, language, "en");

        private static PoiCardSheetView ShowLamp(WallConfigData wall, string language = "en")
        {
            var sheet = Sheet();
            Show(sheet, wall, language);
            return sheet;
        }

        private static void Show(PoiCardSheetView sheet, WallConfigData wall, string language = "en")
        {
            var poi = wall.pois.Single(p => p.id == Lamp);
            var stack = BlockStackBuilder.Build(poi, wall.card_settings, BlockRegistry.Shared, wall.pois);
            var context = CardContextBuilder.Build(new BlockBindContext(), poi, wall, wall.card_settings, language,
                AssetDatabase.LoadAssetAtPath<CardStringTable>(FrameworkStringsPath).Entries(), CardStringSources.Shared.Entries());
            sheet.Show(stack.Entries, context, SheetStopRule.Stop.Full, wall.card_settings.container.half_max_ratio);
        }

        [OneTimeSetUp]
        public void TheAppsKindIsRegistered()
        {
            Assert.IsTrue(BlockRegistry.Shared.TryGet("size_comparison", out _), "precondition: the LivingRoom app's kind is registered in Edit Mode");
            Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<CardStringTable>(FrameworkStringsPath), "precondition: the framework's words");
        }

        // ---------------- dock ----------------

        // The dock is the bar's slots and More, wearing the dock class; the Labels setting does not touch what shows (the stylesheet widens
        // the marked item and hides the other words): a slot's icon and word are left to it, a More list row always says its word
        [TestCase(null)]
        [TestCase("icon")]
        [TestCase("text")]
        public void TheDock_IsTheBarsSlotsAndMore_WithTheDockClass_AndLabelsDoesNotDecideItsWords(string labels)
        {
            var wall = With(BuiltInNavElements.Dock);
            if (labels != null) wall.card_settings.navigation.labels = labels;
            var sheet = ShowLamp(wall);
            var model = sheet.Nav.Model;
            Assert.AreEqual(BuiltInNavElements.Dock, sheet.Slots.BottomKey, "the wall's Bottom Bar is in the bottom slot");
            var dock = (DockNavView)sheet.Slots.BottomView;
            Assert.IsInstanceOf<BottomBarNavView>(dock, "a bar: slots, More and the mark are its own");
            Assert.IsTrue(dock.Root.ClassListContains(DockNavView.DockClass), "the stylesheet's hook for the pill");
            Assert.AreEqual("nav-dock", dock.Root.name);
            Assert.AreEqual(4, dock.Items.Count, "the default 5 slots: four sections and More");
            Assert.IsTrue(dock.HasMore);
            CollectionAssert.AreEqual(model.Sections.Take(4).Select(s => Words("en").Get(s.LabelKey)), dock.Items.Select(t => t.Label.text),
                "the first sections by their words (an icon-only slot still names itself: its tooltip)");
            CollectionAssert.AreEqual(dock.Items.Select(t => t.Label.text), dock.Items.Select(t => t.Button.tooltip), "every slot's tooltip is its word");
            foreach (var slot in dock.Items.Append(dock.More))
            {
                Assert.AreEqual(StyleKeyword.Null, slot.Label.style.display.keyword, slot.Label.text + ": the stylesheet decides the word (Labels = " + (labels ?? "default") + ")");
                Assert.AreEqual(StyleKeyword.Null, slot.Icon.style.display.keyword, slot.Label.text + ": and the icon");
            }
            Assert.AreEqual(4, dock.MoreRows.Count, "More lists the four sections that did not fit");
            foreach (var row in dock.MoreRows)
                Assert.AreEqual(DisplayStyle.Flex, row.Label.style.display.value, row.Label.text + ": a list row always says its word");
            Assert.IsTrue(dock.Items[0].Button.ClassListContains(NavTab.CurrentClass), "the card opens on its first section: that slot is the marked one");
            Assert.AreEqual(1, dock.Items.Count(t => t.Button.ClassListContains(NavTab.CurrentClass)), "and only it");
        }

        // The bar is unchanged by the refactor that opened it up: its own words are still driven by Labels
        [Test]
        public void TheBottomBar_StillObeysLabels_AfterTheDockAndBarSheetWereBuiltOnIt()
        {
            var wall = With(BuiltInNavElements.BottomBar);
            wall.card_settings.navigation.labels = "icon";
            var bar = (BottomBarNavView)ShowLamp(wall).Slots.BottomView;
            Assert.AreSame(typeof(BottomBarNavView), bar.GetType(), "precondition: the plain bar, not a subclass");
            foreach (var tab in bar.Items)
            {
                Assert.AreEqual(DisplayStyle.None, tab.Label.style.display.value, tab.Index + ": Labels = icon hides the word");
                Assert.AreEqual(DisplayStyle.Flex, tab.Icon.style.display.value, tab.Index + ": and keeps the icon");
            }
        }

        // ---------------- bar_sheet ----------------

        private static BarSheetNavView BarSheet(PoiCardSheetView sheet) => (BarSheetNavView)sheet.Slots.BottomView;

        // The card opens on its first family (About, several kinds): a tap on it IS a second tap, so it opens ITS kinds -- each with its words
        // and block count, in the model's order, above the bar and under More's list; a tap on it again closes them, and so on
        [Test]
        public void ATapOnTheFamilyBeingRead_OpensItsKindsWithTheirCounts_AndATapAgainCloses()
        {
            var sheet = ShowLamp(With(BuiltInNavElements.BarSheet));
            var view = BarSheet(sheet);
            var model = sheet.Nav.Model;
            var words = Words("en");
            Assert.AreEqual(BuiltInNavElements.BarSheet, sheet.Slots.BottomKey);
            Assert.AreEqual(0, sheet.Nav.Current, "precondition: the card opens on its first family");
            Assert.IsFalse(view.KindsOpen, "the list starts closed");
            Assert.AreEqual(DisplayStyle.None, view.KindsList.style.display.value);
            Assert.AreEqual(view.Root.IndexOf(view.MoreList) + 1, view.Root.IndexOf(view.KindsList), "the kinds list sits right under More's list...");
            Assert.AreEqual(view.Root.IndexOf(view.KindsList) + 1, view.Root.IndexOf(view.Bar), "...and right above the slots");

            var section = model.Sections[0];
            Assert.GreaterOrEqual(section.Kinds.Count, 2, "precondition: About holds several kinds");
            view.Tap(0);
            Assert.IsTrue(view.KindsOpen, "a tap on the family being read opens its kinds");
            Assert.AreEqual(0, view.KindsSection);
            Assert.AreEqual(DisplayStyle.Flex, view.KindsList.style.display.value);
            CollectionAssert.AreEqual(section.Kinds.Select(k => words.Get(k.LabelKey)), view.KindRows.Select(r => r.Label.text),
                "exactly About's kinds on this card, in the model's order, by the per-kind words step 6 added");
            CollectionAssert.AreEqual(section.Kinds.Select(k => k.Count.ToString()), view.KindRows.Select(r => r.Count.text), "each with its block count");
            Assert.IsTrue(view.KindRows.All(r => r.Count.style.display.value == DisplayStyle.Flex), "the counts are shown");
            Assert.AreEqual(section.Kinds.Count, view.KindsList.childCount, "nothing else in the list");
            Assert.AreEqual(section.Count, section.Kinds.Sum(k => k.Count), "precondition: the counts add up to the family's targets");

            view.Tap(0);
            Assert.IsFalse(view.KindsOpen, "a tap again closes it");
            view.Tap(0);
            Assert.IsTrue(view.KindsOpen, "and the next opens it again");
        }

        // A pick closes the list; More opening closes it (one list above the bar at a time); a rebind closes it; and More's list still works
        [Test]
        public void APick_MoreOpening_AndARebind_EachCloseTheKinds()
        {
            var sheet = ShowLamp(With(BuiltInNavElements.BarSheet));
            var view = BarSheet(sheet);
            view.Tap(0);
            Assert.IsTrue(view.KindsOpen, "precondition: About's kinds are open");
            view.PickKind(1);
            Assert.IsFalse(view.KindsOpen, "a pick closes the list");
            view.PickKind(1);
            Assert.IsFalse(view.KindsOpen, "a pick with no list open does nothing");

            view.Tap(0);
            Assert.IsTrue(view.KindsOpen);
            view.OpenMore();
            Assert.IsFalse(view.KindsOpen, "More opening closes the kinds: one list above the bar at a time");
            Assert.IsTrue(view.MoreOpen, "and More's own list opens as ever");
            view.OpenMore();
            Assert.IsFalse(view.MoreOpen);

            view.Tap(0);
            Assert.IsTrue(view.KindsOpen);
            Show(sheet, With(BuiltInNavElements.BarSheet));
            Assert.IsFalse(BarSheet(sheet).KindsOpen, "a rebind (a live edit, another point) closes it");
            Assert.AreEqual(0, BarSheet(sheet).KindRows.Count, "and lets go of its rows");
        }

        // The arrow says what a tap on the slot would do: on the slot being read only, where kinds would open -- and none on the others
        [Test]
        public void TheArrow_IsOnTheSlotBeingRead_AndNoOther()
        {
            var sheet = ShowLamp(With(BuiltInNavElements.BarSheet));
            var view = BarSheet(sheet);
            Assert.AreEqual(view.Items.Count, view.Arrows.Count, "one arrow per slot of the bar");
            foreach (var (slot, arrow) in view.Arrows)
                Assert.AreEqual(CardIcons.OpenBlock, CardIcons.KeyOf(arrow), slot.Label.text + ": the card's one arrow, turned up by the stylesheet");
            CollectionAssert.AreEqual(new[] { true, false, false, false }, view.Arrows.Select(a => a.Arrow.style.display.value == DisplayStyle.Flex),
                "the card opens on About (several kinds): its slot alone wears the arrow");
            Show(sheet, With(BuiltInNavElements.BarSheet));
            Assert.AreEqual(view.Items.Count, BarSheet(sheet).Arrows.Count, "a rebind leaves exactly one arrow per slot, not a second set");
            CollectionAssert.AreEqual(new[] { 4 }, BarSheet(sheet).Items.Select(t => t.Button.childCount).Distinct(), "icon, words, underline and ONE arrow in each slot");
        }

        // ---------------- next_up ----------------

        private static NextUpNavView NextUp(PoiCardSheetView sheet) => (NextUpNavView)sheet.Slots.BottomView;

        // The card opens on its first target, so the bar names the SECOND (2 of 56) by the words the stack draws over it, with the button;
        // in Portuguese every word of the bar is the Portuguese one
        [TestCase("en", "Up next", "2 of 56", "Next")]
        [TestCase("pt", "A seguir", "2 de 56", "Seguinte")]
        public void NextUp_OnAFreshCard_NamesTheSecondTarget_WithTheHeadingTheStackDraws_InEachLanguage(string language, string caption, string place, string button)
        {
            var wall = With(BuiltInNavElements.NextUp);
            wall.card_settings.languages = new System.Collections.Generic.List<string> { "en", "pt" };
            var sheet = ShowLamp(wall, language);
            var view = NextUp(sheet);
            var model = sheet.Nav.Model;
            var words = Words(language);
            Assert.AreEqual(BuiltInNavElements.NextUp, sheet.Slots.BottomKey, "the wall's Bottom Bar is in the bottom slot");
            Assert.AreEqual(56, model.TargetCount, "precondition: The Lamp's 56 targets");
            Assert.AreEqual(model.Targets[0], sheet.Nav.CurrentBlock, "precondition: the card opens on its first target");
            Assert.AreEqual(model.Targets[1], view.NextBlock, "the bar points at the target after the one read");
            Assert.AreEqual(caption, view.Caption.text, "Up next, in " + language);
            Assert.AreEqual(place, view.Position.text, "its place among ALL the targets, in " + language);
            Assert.AreEqual(button, view.NextLabel.text, "the Next button, in " + language);
            string drawn = sheet.Stack.HeadingOfBlock(model.Targets[1]);
            string expected = drawn.Length > 0 ? drawn : words.Get(model.KindOf(model.Targets[1]).LabelKey);
            Assert.IsNotEmpty(expected, "precondition: block " + model.Targets[1] + " has words to be named by");
            Assert.AreEqual(expected, view.Heading.text, "the words the stack draws over it (else its kind's)");
            Assert.AreEqual(DisplayStyle.Flex, view.Next.style.display.value, "a Next button while there is a next block");
            Assert.IsFalse(view.Finished);
        }

        // A block with no heading of its own is named by its kind -- and the Lamp has some: the bar never shows an empty line for one
        [Test]
        public void EveryTargetOfTheLamp_HasWordsTheBarCanName_ItsHeadingOrItsKinds()
        {
            var sheet = ShowLamp(With(BuiltInNavElements.NextUp));
            var model = sheet.Nav.Model;
            var words = Words("en");
            int byKind = 0;
            foreach (string target in model.Targets)
            {
                string drawn = sheet.Stack.HeadingOfBlock(target);
                string name = drawn.Length > 0 ? drawn : words.Get(model.KindOf(target).LabelKey);
                Assert.IsNotEmpty(name, target + " can be named");
                if (drawn.Length == 0) byKind++;
            }
            Assert.Greater(byKind, 0, "precondition: some of The Lamp's blocks draw no heading (the fallback is exercised)");
        }
    }
}
