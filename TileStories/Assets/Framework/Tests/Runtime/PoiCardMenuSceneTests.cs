using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // Phase B of step 8 (_3.2 section 5.3, the card's overlay owner) on the REAL LivingRoomScene with The Lamp's real card (56 targets,
    // 8 families, Grouped By Family) and real input -- fingers through CardTestInput (4.2.3's focus settings) and Escape on a real Input
    // System keyboard (where Android's Back arrives). Each wall option is pushed through the live seam, the Editor's own path:
    //   - the stops (peek 195 / half 337.60 / full 796 at 390 x 844) do not move with a Menu set, and the header's menu button sits whole
    //     in the header, clear of the language chip and the X
    //   - the menu button opens the drawer over a scrim; a real tap on a kind's line lands its block and the drawer closes; a real tap on the
    //     scrim closes it and the card keeps its scroll; a real Escape closes it and the card stays, the next one lowers the card
    //   - with a Menu, a bar's More opens THAT menu (the big tiles: a real tap on By Format, on a tile with kinds, on a kind)
    //   - the two lists Back could not reach before step 8 (the More list, bar_sheet's kinds list): a real Escape closes them and NOT the
    //     sheet, a real drag on the card closes them, a jump from another element closes them
    //   - a card read in a section of More's list: the More slot says THAT section, and a real tap on it still opens the list
    public class PoiCardMenuSceneTests : SearchSceneFixture
    {
        private const string Lamp = "lamp";

        private PoiCardSheetView Sheet => Card.Sheet;
        private NavController Nav => Sheet.Nav;
        private CardNavModel Model => Nav.Model;
        private ScrollView Scroll => Sheet.Stack.Scroll;
        private CardMenuOverlay Overlay => Sheet.Overlay;

        private sealed class RecordingCardEvents : ICardEvents
        {
            public readonly List<CardEvent> Raised = new();
            public void Raise(CardEvent cardEvent) => Raised.Add(cardEvent);
        }

        private RecordingCardEvents _events;
        private BackgroundSafeInput _backgroundInput;
        private Keyboard _keyboard;

        [UnitySetUp]
        public IEnumerator UseAnInMemoryCardStateAndRecordTheEvents()
        {
            Card.State = new CardLocalState(new MemoryCardStateStore(), Session.SearchConfig.wall_id);
            _events = new RecordingCardEvents();
            Card.Events = _events;
            yield break;
        }

        // The keyboard a test presses goes away with the test (and the input settings it was added under are given back)
        [UnityTearDown]
        public IEnumerator RemoveTheTestKeyboard()
        {
            if (_keyboard != null) InputSystem.RemoveDevice(_keyboard);
            _keyboard = null;
            _backgroundInput?.Dispose();
            _backgroundInput = null;
            yield break;
        }

        // ---------------- helpers ----------------

        private float OnePixel => Sheet.Layer.panel.visualTree.layout.height / Screen.height;

        private float Range => NavScrollRule.Range(Scroll.contentContainer.layout.height, Scroll.contentViewport.layout.height);

        private List<CardEvent> NavJumps => _events.Raised.Where(e => e.Kind == CardEventKinds.NavJump).ToList();

        private CardStrings Words(string language = "en") =>
            new(Card.StringTable.Entries(), CardStringSources.Shared.Entries(), null, language, "en");

        private IEnumerator Push(System.Action<WallConfigData> edit)
        {
            var copy = ConfigCopy();
            edit(copy);
            Session.ApplyCardSettings(copy);
            yield return CardTestInput.Settle(0.3f);
        }

        // The wall's navigation for a test: Grouped By Family (every family one run), the given Top Bar, Bottom Bar and Menu
        private IEnumerator UseNavigation(string top, string bottom, string menu, string language = null)
        {
            yield return Push(c =>
            {
                var nav = c.card_settings.navigation;
                nav.block_order = CardOptions.BlockOrderByFamily;
                nav.top = top;
                nav.bottom = bottom;
                nav.menu = menu;
                nav.max_slots = 5;
                if (language != null) c.card_settings.preview_language = language;
            });
        }

        private IEnumerator Open(SheetStopRule.Stop stop)
        {
            // - Select toggles (re-selecting the selected point clears it): start from nothing selected
            SelectionEventBus.Clear();
            yield return CardTestInput.Settle();
            SelectionEventBus.Select(Lamp);
            yield return CardTestInput.Settle();
            Sheet.SetStop(stop);
            yield return CardTestInput.Settle();
            Assert.AreEqual(stop, Sheet.Stop, "precondition: the card rests at " + stop);
            yield return Rested();
        }

        // The sheet's height animates in real time: every measure waits for it to rest at its stop's height
        private IEnumerator Rested()
        {
            float rest = Sheet.Stops.HeightOf(Sheet.Stop);
            for (int frame = 0; frame < 600 && Mathf.Abs(Sheet.Root.layout.height - rest) > OnePixel; frame++) yield return null;
            Assert.AreEqual(rest, Sheet.Root.layout.height, OnePixel, "precondition: the sheet rests at its " + Sheet.Stop + " height");
            yield return null;
        }

        private IEnumerator Landed()
        {
            for (int frame = 0; frame < 600 && Nav.IsJumping; frame++) yield return null;
            Assert.IsFalse(Nav.IsJumping, "the jump came to rest");
            yield return CardTestInput.Settle(0.3f);
        }

        private static IEnumerator TouchTap(IPanel panel, Vector2 at) => CardTestInput.TouchDrag(panel, at, Vector2.zero, frames: 1);

        // A real tap on a button the visitor sees: inside the card layer (the screen) and, for one in a scroll, inside its visible part
        private IEnumerator Tap(VisualElement button, string name, ScrollView inside = null)
        {
            var screen = Sheet.Layer.worldBound;
            Assert.IsTrue(screen.Contains(button.worldBound.center), "precondition: " + name + " is on screen " + button.worldBound + " in " + screen);
            if (inside != null)
                Assert.IsTrue(inside.contentViewport.worldBound.Contains(button.worldBound.center),
                    "precondition: " + name + " is in the visible part of its list " + button.worldBound + " in " + inside.contentViewport.worldBound);
            yield return TouchTap(Sheet.Layer.panel, button.worldBound.center);
            yield return CardTestInput.Settle();
        }

        // A flicked stack glides on after the finger lifts (the ScrollView's own inertia): wait until its offset holds for 10 frames
        private IEnumerator Still()
        {
            float last = float.NaN;
            int held = 0;
            for (int frame = 0; frame < 600 && held < 10; frame++)
            {
                float now = Scroll.scrollOffset.y;
                held = Mathf.Abs(now - last) < 0.01f ? held + 1 : 0;
                last = now;
                yield return null;
            }
            Assert.GreaterOrEqual(held, 10, "precondition: the stack came to rest after the drag");
        }

        // A real finger drags the stack by `dy` panel units (negative = further down the card), then lets it settle
        private IEnumerator DragStack(float dy)
        {
            var viewport = Scroll.contentViewport.worldBound;
            float inset = Mathf.Min(20f, viewport.height * 0.5f);
            float startY = dy < 0f ? viewport.yMax - inset : viewport.yMin + inset;
            yield return CardTestInput.TouchDrag(Scroll.panel, new Vector2(viewport.center.x, startY), new Vector2(0f, dy));
            yield return CardTestInput.Settle(0.6f);
        }

        // A real press and release of Escape on an Input System keyboard (added under BackgroundSafeInput: a run with Unity in the
        // background still delivers it), one frame each
        private IEnumerator PressEscape()
        {
            if (_keyboard == null)
            {
                _backgroundInput = new BackgroundSafeInput();
                _keyboard = BackgroundSafeInput.AddEnabled<Keyboard>();
            }
            InputSystem.QueueStateEvent(_keyboard, new KeyboardState(Key.Escape));
            yield return null;
            InputSystem.QueueStateEvent(_keyboard, new KeyboardState());
            yield return null;
            yield return CardTestInput.Settle(0.2f);
        }

        // A block's top meets the viewport's top (+-1 px), or -- a late block -- the stack sits at the end of its range
        private void AssertLanded(string block, string name)
        {
            var slot = Sheet.Stack.SlotOfBlock(block);
            if (slot.layout.y <= Range - OnePixel)
                Assert.AreEqual(0f, slot.worldBound.yMin - Scroll.contentViewport.worldBound.yMin, OnePixel, name + ": its block's top meets the viewport's top");
            else
                Assert.AreEqual(Range, Scroll.scrollOffset.y, OnePixel, name + ": a late block clamps at the end of the range");
        }

        // ---------------- the stops and the header ----------------

        // The menu layer is over the sheet and its button is in the header's own row: neither can move a stop. Measured with each menu set,
        // at Full, Half and Peek, against the card with none (5a's standard: peek 195.00 / half 337.60 / full 796.00 at 390 x 844)
        [UnityTest]
        public IEnumerator TheLampsStops_DoNotMove_WithAMenuSet_AndTheMenuButtonSitsWholeInTheHeader()
        {
            Assert.AreEqual(CardOptions.NavNone, ConfigCopy().card_settings.navigation.menu, "precondition: the shipped wall has no Menu");
            yield return Open(SheetStopRule.Stop.Full);
            var unset = Sheet.Stops;
            Assert.AreEqual(DisplayStyle.None, Sheet.MenuButton.resolvedStyle.display, "no Menu: no button");
            foreach (string menu in new[] { BuiltInNavElements.DrawerTree, BuiltInNavElements.GridOverlay })
            {
                yield return Push(c => c.card_settings.navigation.menu = menu);
                yield return Rested();
                Assert.AreEqual(menu, Sheet.Slots.MenuKey, "precondition: " + menu + " is the card's Menu");
                foreach (var stop in new[] { SheetStopRule.Stop.Full, SheetStopRule.Stop.Half, SheetStopRule.Stop.Peek })
                {
                    Sheet.SetStop(stop);
                    yield return CardTestInput.Settle();
                    yield return Rested();
                    var now = Sheet.Stops;
                    Assert.AreEqual(unset.Peek, now.Peek, 0.01f, menu + " at " + stop + ": peek unchanged");
                    Assert.AreEqual(unset.Half, now.Half, 0.01f, menu + " at " + stop + ": half unchanged");
                    Assert.AreEqual(unset.Full, now.Full, 0.01f, menu + " at " + stop + ": full unchanged");
                    Debug.Log("[Card] Stops with Menu " + menu + " at " + stop + ": peek " + now.Peek.ToString("F2") + " half " + now.Half.ToString("F2")
                        + " full " + now.Full.ToString("F2"));
                }
                Sheet.SetStop(SheetStopRule.Stop.Full);
                yield return CardTestInput.Settle();
                yield return Rested();

                // - the button: a whole touch target, inside the card, in the header's row, left of the language chip and the X, the title's
                //   end clear of it
                var button = Sheet.MenuButton.worldBound;
                float touch = CardTestInput.TokenPx("--ts-touch-target");
                Assert.AreEqual(touch, button.width, OnePixel, "a touch target wide");
                Assert.AreEqual(touch, button.height, OnePixel, "and tall");
                Assert.IsTrue(Sheet.Root.worldBound.Contains(button.center), "inside the card");
                Assert.LessOrEqual(button.xMax, Sheet.CloseButton.worldBound.xMin + OnePixel, "left of the X");
                Assert.LessOrEqual(button.xMax, Sheet.SettingsButton.worldBound.xMin + OnePixel, "left of the Aa button");
                Assert.AreEqual(Sheet.CloseButton.worldBound.yMin, button.yMin, OnePixel, "in the X's row");
                var title = ((HeaderBlockView)Sheet.Stack.BoundViews[0]).Root.Q<Label>(className: "card-header__title");
                Assert.IsNotNull(title, "precondition: the header's title");
                Assert.LessOrEqual(title.worldBound.xMax, button.xMin + OnePixel, "the title ends before the button");
            }
        }

        // The renders the visual check reads: The Lamp at 390 px with the default Top Bar and its language chip, in each language -- the header
        // with the menu button (closed), the drawer open on its scrim, the big tiles open on theirs
        [UnityTest]
        public IEnumerator TheLampsMenus_RenderAt390_WithTheHeadersButton_InEachLanguage([Values("en", "pt")] string language)
        {
            yield return UseNavigation(BuiltInNavElements.TextTabs, CardOptions.NavNone, BuiltInNavElements.DrawerTree, language);
            yield return Open(SheetStopRule.Stop.Full);
            Assert.AreEqual(DisplayStyle.Flex, Sheet.SettingsButton.resolvedStyle.display, "precondition: the Aa button shows");
            yield return CardGalleryChecks.Render("Nav_Lamp_menu_button_" + language + "_390");
            yield return Tap(Sheet.MenuButton, "the menu button");
            Assert.AreEqual(BuiltInNavElements.DrawerTree, Overlay.OpenKey, "precondition: the drawer is open");
            yield return CardGalleryChecks.Render("Nav_Lamp_drawer_tree_" + language + "_390");
            yield return PressEscape();

            yield return Push(c => c.card_settings.navigation.menu = BuiltInNavElements.GridOverlay);
            yield return Tap(Sheet.MenuButton, "the menu button");
            Assert.AreEqual(BuiltInNavElements.GridOverlay, Overlay.OpenKey, "precondition: the tiles are open");
            yield return CardGalleryChecks.Render("Nav_Lamp_grid_overlay_" + language + "_390");
            var grid = (GridOverlayNavView)Sheet.Slots.MenuView;
            yield return Tap(grid.ByFormat, "By Format");
            yield return CardGalleryChecks.Render("Nav_Lamp_grid_overlay_format_" + language + "_390");
        }

        // ---------------- the drawer ----------------

        // The header's menu button, a real tap: the drawer opens over a scrim. A real tap on a kind's line moves the card to that kind's first
        // block and the drawer closes -- the jump is what closes it (the overlay owner hears every jump), reported as drawer_tree's
        [UnityTest]
        public IEnumerator ARealTapOnTheMenuButton_OpensTheDrawer_ARealTapOnAKindLandsItsBlock_AndTheDrawerCloses()
        {
            yield return UseNavigation(BuiltInNavElements.TextTabs, CardOptions.NavNone, BuiltInNavElements.DrawerTree);
            yield return Open(SheetStopRule.Stop.Full);
            yield return Tap(Sheet.MenuButton, "the menu button");
            Assert.AreEqual(BuiltInNavElements.DrawerTree, Overlay.OpenKey, "the menu button opened the drawer");
            Assert.IsTrue(Overlay.IsModal);
            var drawer = (DrawerTreeNavView)Sheet.Slots.MenuView;
            Assert.AreEqual(DisplayStyle.Flex, Overlay.Scrim.resolvedStyle.display, "over a scrim");

            // - a kind that does not start its section (so landing it is not landing the section): the first such line the drawer shows
            var pick = drawer.KindRows.FirstOrDefault(k => k.Kind.Targets[0] != Model.Sections[k.Section].Targets[0]
                && drawer.List.contentViewport.worldBound.Contains(k.Row.Button.worldBound.center));
            Assert.IsNotNull(pick.Row, "precondition: a kind line in the visible part of the drawer that does not start its section");
            int before = NavJumps.Count;
            yield return Tap(pick.Row.Button, pick.Kind.Key + "'s line", drawer.List);
            Assert.IsFalse(Overlay.IsOpen, "the pick closed the drawer");
            Assert.AreEqual(DisplayStyle.None, Overlay.Root.resolvedStyle.display, "the scrim is gone");
            yield return Landed();
            AssertLanded(pick.Kind.Targets[0], pick.Kind.Key);
            Assert.AreEqual(before + 1, NavJumps.Count, "one nav_jump");
            Assert.AreEqual(BuiltInNavElements.DrawerTree, NavJumps.Last().Variant, "reported as the drawer's");
            Assert.AreEqual(pick.Kind.Targets[0], NavJumps.Last().BlockKey);
        }

        // A real tap on the scrim beside the drawer closes it, and the card behind has not moved: the same scroll, the same stop
        [UnityTest]
        public IEnumerator ARealTapOnTheScrim_ClosesTheDrawer_AndTheCardKeepsItsScroll()
        {
            yield return UseNavigation(BuiltInNavElements.TextTabs, CardOptions.NavNone, BuiltInNavElements.DrawerTree);
            yield return Open(SheetStopRule.Stop.Full);
            yield return DragStack(-300f);
            yield return Still();
            float scrolled = Scroll.scrollOffset.y;
            Assert.Greater(scrolled, 100f, "precondition: the card is scrolled down (" + scrolled.ToString("F1") + ")");

            yield return Tap(Sheet.MenuButton, "the menu button");
            Assert.IsTrue(Overlay.IsModal, "precondition: the drawer is open");
            Assert.AreEqual(scrolled, Scroll.scrollOffset.y, OnePixel, "opening the drawer keeps the card's scroll");
            var drawer = Sheet.Slots.MenuView.Root.worldBound;
            var beside = new Vector2((drawer.xMax + Sheet.Layer.worldBound.xMax) * 0.5f, Sheet.Layer.worldBound.center.y);
            Assert.Greater(Sheet.Layer.worldBound.xMax - drawer.xMax, 2f * 22f, "precondition: the scrim beside the drawer is wider than a finger");
            Assert.AreSame(Overlay.Scrim, Sheet.Layer.panel.Pick(beside), "precondition: beside the drawer the scrim takes the finger");
            yield return TouchTap(Sheet.Layer.panel, beside);
            yield return CardTestInput.Settle();

            Assert.IsFalse(Overlay.IsOpen, "the scrim closed the drawer");
            Assert.AreEqual(SheetStopRule.Stop.Full, Sheet.Stop, "the card stays at Full");
            Assert.AreEqual(scrolled, Scroll.scrollOffset.y, OnePixel, "and keeps its scroll");
        }

        // The acceptance walk with real Escape presses: the first closes the drawer and NOT the sheet; the second lowers the sheet
        [UnityTest]
        public IEnumerator RealEscape_ClosesTheDrawerFirst_AndTheSheetStays_TheNextLowersTheSheet()
        {
            yield return UseNavigation(BuiltInNavElements.TextTabs, CardOptions.NavNone, BuiltInNavElements.DrawerTree);
            yield return Open(SheetStopRule.Stop.Full);
            yield return Tap(Sheet.MenuButton, "the menu button");
            Assert.IsTrue(Overlay.IsOpen, "precondition: the drawer is open");

            yield return PressEscape();
            Assert.IsFalse(Overlay.IsOpen, "press 1: the drawer closes");
            Assert.AreEqual(SheetStopRule.Stop.Full, Sheet.Stop, "press 1: and the sheet stays at Full");
            yield return PressEscape();
            Assert.AreEqual(SheetStopRule.Stop.Half, Sheet.Stop, "press 2: Full -> Half");
        }

        // ---------------- More opens the Menu; the big tiles with real taps ----------------

        // The seam 7a left open: with a Menu set, a real tap on a bar's More opens THAT menu (here the big tiles), never its small list. Then
        // the tiles with real taps: By Format turns them into formats, a section with several kinds opens them in place, a kind lands its block
        [UnityTest]
        public IEnumerator WithAMenu_ARealTapOnMore_OpensTheTiles_AndRealTapsOnTheTilesLandABlock()
        {
            yield return UseNavigation(CardOptions.NavNone, BuiltInNavElements.BottomBar, BuiltInNavElements.GridOverlay);
            yield return Open(SheetStopRule.Stop.Full);
            var bar = (BottomBarNavView)Sheet.Slots.BottomView;
            Assert.IsTrue(bar.HasMore, "precondition: eight families overflow five slots");
            yield return Tap(bar.More.Button, "More");
            Assert.AreEqual(BuiltInNavElements.GridOverlay, Overlay.OpenKey, "More opened the Menu");
            Assert.IsFalse(bar.MoreOpen, "not the small list");
            var grid = (GridOverlayNavView)Sheet.Slots.MenuView;
            Assert.AreEqual(Model.Sections.Count, grid.Tiles.Count, "a tile per family");

            yield return Tap(grid.ByFormat, "By Format");
            Assert.AreEqual(CardOptions.GroupByFormat, grid.Axis, "the switch went to Format");
            CollectionAssert.AreEqual(Model.By(CardOptions.GroupByFormat).Sections.Select(s => Words().Get(s.LabelKey)), grid.Tiles.Select(t => t.Label.text),
                "the formats' tiles");
            yield return Tap(grid.ByPurpose, "By Purpose");

            int several = Model.Sections.ToList().FindIndex(s => s.Kinds.Count >= 2 && s.Kinds.Any(k => k.Targets[0] != s.Targets[0]));
            Assert.GreaterOrEqual(several, 0, "precondition: a family with several kinds");
            yield return Tap(grid.Tiles[several].Button, Model.Sections[several].Key + "'s tile", grid.Scroll);
            Assert.AreEqual(several, grid.OpenSection, "its kinds opened in place");
            Assert.IsTrue(Overlay.IsOpen, "and the tiles stayed open");
            var kinds = Model.Sections[several].Kinds;
            int kind = kinds.ToList().FindIndex(k => k.Targets[0] != Model.Sections[several].Targets[0]);
            yield return Tap(grid.Tiles[kind].Button, kinds[kind].Key + "'s tile", grid.Scroll);
            Assert.IsFalse(Overlay.IsOpen, "a kind's tile moved the card and closed the tiles");
            yield return Landed();
            AssertLanded(kinds[kind].Targets[0], kinds[kind].Key);
            Assert.AreEqual(BuiltInNavElements.GridOverlay, NavJumps.Last().Variant, "reported as the tiles'");
        }

        // ---------------- the two lists Back could not reach (PART 1) ----------------

        // The More list (no Menu): a real Escape closes it and NOT the sheet; a real drag on the card closes it; a tap on a tab of the Top Bar
        // (a jump from another element) closes it. Before step 8 Back lowered the sheet under the open list and a scroll left it open
        [UnityTest]
        public IEnumerator TheMoreList_ClosesOnARealEscape_NotTheSheet_OnARealDrag_AndOnAnotherElementsJump()
        {
            yield return UseNavigation(BuiltInNavElements.TextTabs, BuiltInNavElements.BottomBar, CardOptions.NavNone);
            yield return Open(SheetStopRule.Stop.Full);
            var bar = (BottomBarNavView)Sheet.Slots.BottomView;
            Assert.IsTrue(Sheet.Chrome.BottomNavShown, "precondition: the bar shows at Full");

            yield return Tap(bar.More.Button, "More");
            Assert.IsTrue(bar.MoreOpen, "precondition: More's list is open");
            yield return PressEscape();
            Assert.IsFalse(bar.MoreOpen, "Escape closed the list");
            Assert.AreEqual(SheetStopRule.Stop.Full, Sheet.Stop, "and not the sheet");

            yield return Tap(bar.More.Button, "More");
            Assert.IsTrue(bar.MoreOpen, "precondition: open again");
            float before = Scroll.scrollOffset.y;
            yield return DragStack(-120f);
            Assert.Greater(Scroll.scrollOffset.y, before + 50f, "precondition: the drag scrolled the card");
            Assert.IsFalse(bar.MoreOpen, "a scroll closed the list");
            Assert.IsFalse(Overlay.IsOpen);

            // - back near the top so the bar is on screen again (Hide While Scrolling slid it away), then the list and a tab
            yield return DragStack(400f);
            Assert.IsTrue(Sheet.Chrome.BottomShown, "precondition: the bar is back");
            yield return Tap(bar.More.Button, "More");
            Assert.IsTrue(bar.MoreOpen, "precondition: open again");
            var tabs = (TextTabsNavView)Sheet.Slots.TopView;
            yield return Tap(tabs.Tabs[1].Button, "the second tab");
            Assert.IsFalse(bar.MoreOpen, "another element's jump closed the list");
            yield return Landed();
        }

        // bar_sheet's kinds list: a real Escape closes it and the sheet stays; the next lowers the sheet
        [UnityTest]
        public IEnumerator TheKindsList_ClosesOnARealEscape_AndTheSheetStays_TheNextLowersIt()
        {
            yield return UseNavigation(CardOptions.NavNone, BuiltInNavElements.BarSheet, CardOptions.NavNone);
            yield return Open(SheetStopRule.Stop.Full);
            var view = (BarSheetNavView)Sheet.Slots.BottomView;
            Assert.AreEqual(0, view.CurrentIndex, "precondition: the card is read at its first family");
            Assert.GreaterOrEqual(Model.Sections[0].Kinds.Count, 2, "precondition: with a choice of kinds");
            yield return Tap(view.Items[0].Button, "the family being read");
            Assert.IsTrue(view.KindsOpen, "precondition: its kinds are open");

            yield return PressEscape();
            Assert.IsFalse(view.KindsOpen, "Escape closed the kinds list");
            Assert.AreEqual(SheetStopRule.Stop.Full, Sheet.Stop, "and the sheet stays at Full");
            yield return PressEscape();
            Assert.AreEqual(SheetStopRule.Stop.Half, Sheet.Stop, "the next Escape lowers it");
        }

        // ---------------- More says where the visitor is (PART 1, decided at the 7b review) ----------------

        // A card read in the 7th of its 8 sections (one of More's list): the More slot shows THAT section's icon and word, with the "opens a
        // list" arrow, in each language -- and a real tap on the slot still opens the list. Back at the first family it says More again
        [UnityTest]
        public IEnumerator ASectionReadInMoresList_ShowsItsWordInTheSlot_AndARealTapStillOpensTheList([Values("en", "pt")] string language)
        {
            yield return UseNavigation(CardOptions.NavNone, BuiltInNavElements.BottomBar, CardOptions.NavNone, language);
            yield return Open(SheetStopRule.Stop.Full);
            var bar = (BottomBarNavView)Sheet.Slots.BottomView;
            var words = Words(language);
            Assert.AreEqual(8, Model.Sections.Count, "precondition: eight sections");
            Assert.AreEqual(4, bar.Items.Count, "precondition: four in the bar, More with the other four");
            Assert.AreEqual(words.Get(CardStrings.Keys.NavMore), bar.More.Label.text, "at the first family the slot says More");
            Assert.AreEqual(DisplayStyle.None, bar.MoreArrow.resolvedStyle.display, "with no arrow");

            yield return Tap(bar.More.Button, "More");
            const int seventh = 6;
            var row = bar.MoreRows[seventh - bar.Items.Count];
            yield return Tap(row.Button, "the 7th section's row");
            yield return Landed();
            Assert.AreEqual(seventh, Nav.Current, "precondition: the card is read in its 7th section (" + Model.Sections[seventh].Key + ")");
            var section = Model.Sections[seventh];
            Assert.AreEqual(words.Get(section.LabelKey), bar.More.Label.text, "the slot says the 7th section's word");
            Assert.AreEqual(section.IconKey, CardIcons.KeyOf(bar.More.Icon), "with its icon");
            Assert.IsTrue(bar.More.Button.ClassListContains(NavTab.CurrentClass), "marked");
            Assert.AreEqual(DisplayStyle.Flex, bar.MoreArrow.resolvedStyle.display, "and the arrow says it opens a list");
            var measured = bar.More.Label.MeasureTextSize(bar.More.Label.text, 0, VisualElement.MeasureMode.Undefined, 0, VisualElement.MeasureMode.Undefined);
            Assert.GreaterOrEqual(bar.More.Label.layout.width + 0.5f, measured.x, "'" + bar.More.Label.text + "': whole in the slot");

            yield return Tap(bar.More.Button, "the slot that says " + bar.More.Label.text);
            Assert.IsTrue(bar.MoreOpen, "a real tap on it still opens the list");
            Assert.AreEqual(BottomBarNavView.MoreListKey, Overlay.OpenKey);
            yield return PressEscape();

            Nav.JumpTo(0, "test");
            yield return Landed();
            Assert.AreEqual(words.Get(CardStrings.Keys.NavMore), bar.More.Label.text, "read at the first family again: More");
            Assert.AreEqual(CardIcons.More, CardIcons.KeyOf(bar.More.Icon));
        }
    }
}
