using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
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
    public class CardMenuOverlayTests
    {
        private const string FrameworkStringsPath = "Assets/Framework/Runtime/UI/Cards/CardStrings.asset";
        private const string Lamp = "lamp";

        private static WallConfigData Shipped() =>
            JsonUtility.FromJson<WallConfigData>(File.ReadAllText("Assets/Apps/LivingRoom/config.json"));

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

        // The card's context hands every element the ONE owner: the bar opens its list through it
        [Test]
        public void TheBarsMoreList_OpensThroughTheOwner_AndItsToggleClosesItThere()
        {
            var sheet = Sheet();
            ShowCard(sheet, With(BuiltInNavElements.BottomBar));
            var bar = (BottomBarNavView)sheet.Slots.BottomView;
            Assert.IsTrue(bar.HasMore, "precondition: The Lamp's 8 sections overflow 5 slots");

            bar.OpenMore();
            Assert.IsTrue(bar.MoreOpen, "More opens its list");
            Assert.AreEqual(BottomBarNavView.MoreListKey, sheet.Overlay.OpenKey, "as the one thing open over the card");
            Assert.IsFalse(sheet.Overlay.IsModal, "anchored in place: no scrim");
            Assert.AreEqual(DisplayStyle.None, sheet.Overlay.Root.style.display.value, "the layer stays hidden for an anchored list");

            bar.OpenMore();
            Assert.IsFalse(bar.MoreOpen, "More again closes it (the opener's own toggle)");
            Assert.IsFalse(sheet.Overlay.IsOpen, "and the owner knows");
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

        // The acceptance walk: with a list open over a Full card, Back closes the list and NOT the sheet; the second Back lowers the sheet
        [Test]
        public void Back_WithTheMoreListOpen_ClosesTheList_AndTheSheetStays_ThenTheStopsStepDown()
        {
            var sheet = Sheet();
            ShowCard(sheet, With(BuiltInNavElements.BottomBar));
            var bar = (BottomBarNavView)sheet.Slots.BottomView;
            bool closedCard = false;
            sheet.CloseRequested += () => closedCard = true;
            bar.OpenMore();
            Assert.IsTrue(bar.MoreOpen, "precondition: the list is open");

            Assert.IsTrue(sheet.Back(), "the card takes the press");
            Assert.IsFalse(bar.MoreOpen, "the first Back closes the list");
            Assert.AreEqual(SheetStopRule.Stop.Full, sheet.Stop, "and the sheet stays at Full");

            Assert.IsTrue(sheet.Back());
            Assert.AreEqual(SheetStopRule.Stop.Half, sheet.Stop, "the second Back lowers the sheet");
            Assert.IsTrue(sheet.Back());
            Assert.AreEqual(SheetStopRule.Stop.Peek, sheet.Stop);
            Assert.IsFalse(closedCard, "precondition: still open at Peek");
            Assert.IsTrue(sheet.Back());
            Assert.IsTrue(closedCard, "at Peek Back closes the card, like the X");
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

        // A full view never stacks over another sheet: it opening closes the list, and then Back closes the full view, then lowers
        [Test]
        public void TheFullViewOpening_ClosesTheList_AndBackThenClosesTheFullViewFirst()
        {
            var sheet = Sheet();
            ShowCard(sheet, With(BuiltInNavElements.BottomBar));
            var bar = (BottomBarNavView)sheet.Slots.BottomView;
            bar.OpenMore();
            Assert.IsTrue(bar.MoreOpen, "precondition: the list is open");

            sheet.OpenTakeover("Gallery", 1, 0, (view, container, media, page) => { });
            Assert.IsTrue(sheet.Takeover.IsOpen, "precondition: the full view opened");
            Assert.IsFalse(bar.MoreOpen, "the full view replaced the list: never stacked");
            Assert.IsFalse(sheet.Overlay.IsOpen);

            Assert.IsTrue(sheet.Back());
            Assert.IsFalse(sheet.Takeover.IsOpen, "the first Back closes the full view");
            Assert.AreEqual(SheetStopRule.Stop.Full, sheet.Stop, "the card stays");
            Assert.IsTrue(sheet.Back());
            Assert.AreEqual(SheetStopRule.Stop.Half, sheet.Stop, "the second lowers it");
        }

        // The sheet resting at another stop and every rebind close what is open (a list left open in a bottom area the stop hides would make
        // the next Back a press that does nothing the visitor can see)
        [Test]
        public void AStopChange_AndARebind_EachCloseTheList()
        {
            var sheet = Sheet();
            var wall = With(BuiltInNavElements.BottomBar);
            ShowCard(sheet, wall);
            var bar = (BottomBarNavView)sheet.Slots.BottomView;

            bar.OpenMore();
            sheet.SetStop(SheetStopRule.Stop.Half);
            Assert.IsFalse(bar.MoreOpen, "the sheet moved to Half: the list closed");
            Assert.IsFalse(sheet.Overlay.IsOpen);

            sheet.SetStop(SheetStopRule.Stop.Full);
            bar.OpenMore();
            Assert.IsTrue(bar.MoreOpen, "precondition: open again");
            ShowCard(sheet, wall);
            Assert.IsFalse(sheet.Overlay.IsOpen, "a rebind (a live edit, another point, a language) closes it");
            Assert.IsFalse(((BottomBarNavView)sheet.Slots.BottomView).MoreOpen);
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

        // The grid: one tile per section with its count, on the wall's axis; the switch shows the formats (the model's pivot, CardNavModel.By);
        // a section with several kinds opens them IN PLACE with a way back; the menu closing brings it back to every section
        [Test]
        public void TheGrid_ShowsTheSections_PivotsToTheFormats_OpensKindsInPlace_AndComesBackToAllSections()
        {
            var sheet = Sheet();
            ShowCard(sheet, With(CardOptions.NavNone, BuiltInNavElements.GridOverlay));
            var grid = (GridOverlayNavView)sheet.Slots.MenuView;
            var model = sheet.Nav.Model;
            var words = Words("en");
            Assert.AreEqual(CardOptions.GroupByFamily, grid.Axis, "it opens on the wall's Group By");
            Assert.IsTrue(grid.ByPurpose.ClassListContains(GridOverlayNavView.OptionCurrentClass), "Purpose marked");
            Assert.AreEqual(model.Sections.Count, grid.Tiles.Count, "a tile per section");
            for (int i = 0; i < model.Sections.Count; i++)
            {
                Assert.AreEqual(words.Get(model.Sections[i].LabelKey), grid.Tiles[i].Label.text);
                Assert.AreEqual(model.Sections[i].Count.ToString(), grid.Tiles[i].Count.text);
                Assert.AreEqual(model.Sections[i].IconKey, CardIcons.KeyOf(grid.Tiles[i].Icon), "the family's icon");
            }

            grid.ShowAxis(CardOptions.GroupByFormat);
            var formats = model.By(CardOptions.GroupByFormat);
            Assert.IsTrue(grid.ByFormat.ClassListContains(GridOverlayNavView.OptionCurrentClass), "Format marked");
            Assert.IsFalse(grid.ByPurpose.ClassListContains(GridOverlayNavView.OptionCurrentClass));
            Assert.AreEqual(6, grid.Tiles.Count, "the six formats");
            CollectionAssert.AreEqual(formats.Sections.Select(s => words.Get(s.LabelKey)), grid.Tiles.Select(t => t.Label.text), "read from the model's pivot");
            Assert.IsTrue(grid.Tiles.All(t => t.Icon.style.display.value == DisplayStyle.None), "a format has no icon: its words name it");

            grid.ShowAxis(CardOptions.GroupByFamily);
            int several = model.Sections.ToList().FindIndex(s => s.Kinds.Count >= 2);
            grid.PickTile(several);
            Assert.AreEqual(several, grid.OpenSection, "a section with several kinds opens them in place");
            Assert.AreEqual(model.Sections[several].Kinds.Count, grid.Tiles.Count, "one tile per kind");
            Assert.AreEqual(DisplayStyle.Flex, grid.BackRow.style.display.value, "with the way back");
            Assert.AreEqual(words.Get(CardStrings.Keys.NavMenuAll), grid.All.text);
            Assert.AreEqual(words.Get(model.Sections[several].LabelKey), grid.OpenTitle.text, "and the section's words");
            grid.OpenKinds(-1);
            Assert.AreEqual(model.Sections.Count, grid.Tiles.Count, "All sections brings every tile back");
            Assert.AreEqual(DisplayStyle.None, grid.BackRow.style.display.value);

            Assert.IsTrue(sheet.Overlay.OpenMenu());
            grid.PickTile(several);
            sheet.Overlay.Close();
            Assert.AreEqual(-1, grid.OpenSection, "closed: it opens again on every section");
        }

        // The seam 7a left open: with a Menu set, a bar's More opens THAT menu instead of its small list; without one, the small list
        [Test]
        public void WithAMenu_TheBarsMoreOpensTheMenu_AndWithoutOne_ItsSmallList()
        {
            var sheet = Sheet();
            ShowCard(sheet, With(BuiltInNavElements.BottomBar, BuiltInNavElements.GridOverlay));
            var bar = (BottomBarNavView)sheet.Slots.BottomView;
            bar.OpenMore();
            Assert.AreEqual(BuiltInNavElements.GridOverlay, sheet.Overlay.OpenKey, "More opened the Menu");
            Assert.IsTrue(sheet.Overlay.IsModal);
            Assert.IsFalse(bar.MoreOpen, "and not the small list");

            sheet.Overlay.Close();
            ShowCard(sheet, With(BuiltInNavElements.BottomBar));
            bar = (BottomBarNavView)sheet.Slots.BottomView;
            bar.OpenMore();
            Assert.IsTrue(bar.MoreOpen, "no Menu: the small list");
            Assert.AreEqual(BottomBarNavView.MoreListKey, sheet.Overlay.OpenKey);
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

        // Back Button = none leaves the press to the app even with a list open: the list stays as it was
        [Test]
        public void BackButtonNone_LeavesThePressToTheApp_AndTheListStaysOpen()
        {
            var sheet = Sheet();
            ShowCard(sheet, With(BuiltInNavElements.BottomBar, back: CardOptions.BackButtonNone));
            var bar = (BottomBarNavView)sheet.Slots.BottomView;
            bar.OpenMore();

            Assert.IsFalse(sheet.Back(), "Back is the app's");
            Assert.IsTrue(bar.MoreOpen, "nothing closed");
            Assert.AreEqual(SheetStopRule.Stop.Full, sheet.Stop);
        }
    }
}
