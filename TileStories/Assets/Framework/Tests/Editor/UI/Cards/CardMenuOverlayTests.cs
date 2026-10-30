using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using TileStories.Tests;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories.Editor.Tests
{
    // The card's overlay owner (CardMenuOverlay, _3.2 step 8, sections 4.5 and 5.3) on a REAL sheet with The Lamp's real card and the
    // shipped words. Tier 0 of its state: where its layer sits (over the sheet, under the full view), ONE thing open at a time (the bar's
    // More list and bar_sheet's kinds list are its clients), and what system Back does at each layer -- the list first and the sheet stays,
    // then the stops, with a full view the innermost thing, Back Button = none leaving it all to the app. The close triggers that need real
    // layout and real fingers (a jump, a touch on the stack, the scrim) are PoiCardMenuSceneTests'; the ones a sheet can show without a panel
    // (a stop change, a rebind, the full view opening, the opener's own toggle) are here.
    [Ignore(TestSkips.OldNavigationElements)]
    public class CardMenuOverlayTests
    {
        private const string FrameworkStringsPath = "Assets/Framework/Runtime/UI/Screen/Cards/CardStrings.asset";
        private const string Lamp = "lamp";

        // - the Navigation Bar at the framework default (off): these tests measure the elements, never the wall's own bar (40-testing)
        private static WallConfigData Shipped()
        {
            var config = JsonUtility.FromJson<WallConfigData>(File.ReadAllText("Assets/Apps/LivingRoom/config.json"));
            config.card_settings.navigation.bar = new CardNavBarSettings();
            return config;
        }

        // The shipped wall with `bottom` as its Bottom Bar (and `menu` as its Menu), no Top Bar, the blocks Grouped By Family (8 sections:
        // four in the bar, More with four)
        private static WallConfigData With(string bottom, string menu = CardOptions.NavNone, string back = CardOptions.BackButtonStepDown)
        {
            var wall = Shipped();
            var nav = wall.card_settings.navigation;
            nav.bottom = bottom;
            nav.menu = menu;
            nav.top = CardOptions.NavNone;
            nav.block_order = CardOptions.BlockOrderByFamily;
            nav.max_slots = 5;
            nav.back_button = back;
            return wall;
        }

        private static PoiCardSheetView Sheet() => new(new VisualElement(), BlockRegistry.Shared, Array.Empty<StyleSheet>());

        private static void ShowCard(PoiCardSheetView sheet, WallConfigData wall, string language = "en")
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

        // Section 4.2: the `menu` layer is the card layer over the sheet, BELOW TakeoverView -- paint order is child order
        [Test]
        public void TheOverlaysLayer_SitsOverTheSheet_AndUnderTheFullView_AndStartsClosed()
        {
            var sheet = Sheet();
            int sheetAt = sheet.Layer.IndexOf(sheet.Root);
            int overlayAt = sheet.Layer.IndexOf(sheet.Overlay.Root);
            int fullViewAt = sheet.Layer.IndexOf(sheet.Takeover.Root);
            Assert.GreaterOrEqual(sheetAt, 0, "precondition: the sheet is in the card layer");
            Assert.Greater(overlayAt, sheetAt, "over the sheet");
            Assert.Greater(fullViewAt, overlayAt, "under the full view");
            Assert.IsFalse(sheet.Overlay.IsOpen, "nothing open");
            Assert.AreEqual(DisplayStyle.None, sheet.Overlay.Root.style.display.value, "the layer is hidden while nothing modal is open");
            Assert.AreSame(sheet.Overlay.Scrim.parent, sheet.Overlay.Root, "the scrim is the layer's");
        }

        // One at a time: the kinds list and More's list of bar_sheet are both the owner's, and opening one closes the other
        [Test]
        public void OneAtATime_TheKindsListAndMoresList_CloseEachOther()
        {
            var sheet = Sheet();
            ShowCard(sheet, With(BuiltInNavElements.BarSheet));
            var view = (BarSheetNavView)sheet.Slots.BottomView;
            Assert.AreEqual(0, view.CurrentIndex, "precondition: the card opens on its first family");
            Assert.GreaterOrEqual(sheet.Nav.Model.Sections[0].Kinds.Count, 2, "precondition: About has several kinds");

            view.Tap(0);
            Assert.IsTrue(view.KindsOpen, "a tap on the family being read opens its kinds");
            Assert.AreEqual(BarSheetNavView.KindsListKey, sheet.Overlay.OpenKey);

            view.OpenMore();
            Assert.IsTrue(view.MoreOpen, "More opens");
            Assert.IsFalse(view.KindsOpen, "and the kinds list closed: never two at once");
            Assert.AreEqual(BottomBarNavView.MoreListKey, sheet.Overlay.OpenKey);

            view.Tap(0);
            Assert.IsTrue(view.KindsOpen, "the kinds again");
            Assert.IsFalse(view.MoreOpen, "and More's list closed");
            Assert.AreEqual(BarSheetNavView.KindsListKey, sheet.Overlay.OpenKey);
        }

        // The same for bar_sheet's kinds list: the two lists Back could not reach before step 8
        [Test]
        public void Back_WithTheKindsListOpen_ClosesTheList_AndTheSheetStays()
        {
            var sheet = Sheet();
            ShowCard(sheet, With(BuiltInNavElements.BarSheet));
            var view = (BarSheetNavView)sheet.Slots.BottomView;
            view.Tap(0);
            Assert.IsTrue(view.KindsOpen, "precondition: About's kinds are open");

            Assert.IsTrue(sheet.Back());
            Assert.IsFalse(view.KindsOpen, "Back closes the kinds list");
            Assert.AreEqual(SheetStopRule.Stop.Full, sheet.Stop, "and the sheet stays at Full");
            Assert.IsTrue(sheet.Back());
            Assert.AreEqual(SheetStopRule.Stop.Half, sheet.Stop, "then the sheet goes down");
        }

        private static CardStrings Words(string language) =>
            new(AssetDatabase.LoadAssetAtPath<CardStringTable>(FrameworkStringsPath).Entries(), CardStringSources.Shared.Entries(), null, language, "en");

        // The header's menu button exists only with a Menu (default None: none), and opens the Menu as a MODAL: the layer and its scrim
        // show, the element's own panel is drawn on it; Back then closes it and the sheet stays
        [Test]
        public void TheMenuButton_IsDrawnOnlyWithAMenu_AndOpensItOnTheScrim_WhichBackCloses()
        {
            var sheet = Sheet();
            ShowCard(sheet, With(CardOptions.NavNone));
            Assert.AreEqual(DisplayStyle.None, sheet.MenuButton.style.display.value, "Menu = None (the default): no button");
            Assert.IsFalse(sheet.Overlay.HasMenu);
            Assert.IsFalse(sheet.Overlay.OpenMenu(), "nothing to open");

            ShowCard(sheet, With(CardOptions.NavNone, BuiltInNavElements.DrawerTree));
            Assert.AreEqual(DisplayStyle.Flex, sheet.MenuButton.style.display.value, "with a Menu: the button");
            Assert.IsTrue(sheet.Root.ClassListContains("poi-card--has-menu"), "and the header keeps its end clear of it");
            Assert.AreEqual(Words("en").Get(CardStrings.Keys.NavMenu), sheet.MenuButton.tooltip, "named by CardStrings");
            Assert.AreEqual(CardIcons.Menu, CardIcons.KeyOf(sheet.MenuButton.Q(className: "card-icon")), "three bars");

            Assert.IsTrue(sheet.Overlay.OpenMenu());
            Assert.AreEqual(BuiltInNavElements.DrawerTree, sheet.Overlay.OpenKey);
            Assert.IsTrue(sheet.Overlay.IsModal, "a menu is modal");
            Assert.AreEqual(DisplayStyle.Flex, sheet.Overlay.Root.style.display.value, "the layer shows, with its scrim");
            Assert.AreSame(sheet.Overlay.Panel, sheet.Slots.MenuView.Root.parent, "the drawer drawn on the scrim's panel");

            Assert.IsTrue(sheet.Back());
            Assert.IsFalse(sheet.Overlay.IsOpen, "Back closes the menu");
            Assert.IsNull(sheet.Slots.MenuView.Root.parent, "its panel taken off the layer");
            Assert.AreEqual(SheetStopRule.Stop.Full, sheet.Stop, "the sheet stays");
            Assert.IsTrue(sheet.Back());
            Assert.AreEqual(SheetStopRule.Stop.Half, sheet.Stop, "the next Back lowers it");
        }

        // The drawer: every section of the model with its count, in its order; under a section with two kinds or more, its kinds with theirs
        // (a one-kind section lists none: Listen); the words are the card's language
        [TestCase("en")]
        [TestCase("pt")]
        public void TheDrawer_ListsEverySectionWithItsCount_AndTheKindsOfEachWithAChoice(string language)
        {
            var sheet = Sheet();
            ShowCard(sheet, With(CardOptions.NavNone, BuiltInNavElements.DrawerTree), language);
            var drawer = (DrawerTreeNavView)sheet.Slots.MenuView;
            var model = sheet.Nav.Model;
            var words = Words(language);
            Assert.AreEqual(8, model.Sections.Count, "precondition: The Lamp's eight families");

            Assert.AreEqual(model.Sections.Count, drawer.SectionRows.Count, "one line per section");
            for (int s = 0; s < model.Sections.Count; s++)
            {
                Assert.AreEqual(words.Get(model.Sections[s].LabelKey), drawer.SectionRows[s].Label.text, "section " + s + ": its words");
                Assert.AreEqual(model.Sections[s].Count.ToString(), drawer.SectionRows[s].Count.text, model.Sections[s].Key + ": its count");
                var listed = drawer.KindRows.Where(k => k.Section == s).Select(k => k.Kind.Key).ToList();
                if (model.Sections[s].Kinds.Count >= 2)
                    CollectionAssert.AreEqual(model.Sections[s].Kinds.Select(k => k.Key), listed, model.Sections[s].Key + ": every kind, in order");
                else
                    CollectionAssert.IsEmpty(listed, model.Sections[s].Key + ": one kind lists none");
            }
            Assert.IsTrue(drawer.KindRows.Any(), "precondition (not vacuous): kinds are listed");
            foreach (var (row, s, kind) in drawer.KindRows)
            {
                Assert.AreEqual(words.Get(kind.LabelKey), row.Label.text, kind.Key + ": the kind's words");
                Assert.AreEqual(kind.Count.ToString(), row.Count.text, kind.Key + ": its count");
                Assert.IsTrue(row.Button.ClassListContains(DrawerTreeNavView.KindRowClass), "indented under its section");
            }
            // - every line sits after its section's, in the outline's order
            var lines = drawer.List.contentContainer.Children().ToList();
            foreach (var (row, s, _) in drawer.KindRows)
                Assert.Greater(lines.IndexOf(row.Button), lines.IndexOf(drawer.SectionRows[s].Button), "a kind under its own section");
            Assert.AreEqual(words.Get(CardStrings.Keys.NavMenu), drawer.Header.Title.text, "the drawer's title");
            Assert.AreEqual(words.Get(CardStrings.Keys.Close), drawer.Header.Close.tooltip, "a visible close, named");
            Assert.IsTrue(drawer.SectionRows[0].Button.ClassListContains(NavTab.CurrentClass), "the section being read (the first) is marked");
        }

        // One at a time across the two kinds: a list open in place closes when the menu opens over the card
        [Test]
        public void OpeningTheMenu_ClosesAnOpenList()
        {
            var sheet = Sheet();
            ShowCard(sheet, With(BuiltInNavElements.BarSheet, BuiltInNavElements.DrawerTree));
            var view = (BarSheetNavView)sheet.Slots.BottomView;
            view.Tap(0);
            Assert.IsTrue(view.KindsOpen, "precondition: the kinds list is open");
            Assert.IsTrue(sheet.Overlay.OpenMenu());
            Assert.IsFalse(view.KindsOpen, "the menu replaced it");
            Assert.AreEqual(BuiltInNavElements.DrawerTree, sheet.Overlay.OpenKey);
        }

    }
}
