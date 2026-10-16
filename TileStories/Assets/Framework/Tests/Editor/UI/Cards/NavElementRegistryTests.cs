using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories.Editor.Tests
{
    // The navigation elements as data (_3.2 step 5a): NavElementRegistry has BlockRegistry's shape -- refusals, the framework's
    // elements, an app's own element for a slot with zero Framework edits, which the Editor lists and a real sheet then shows --,
    // NavSlotRule (which element a slot holds, rule 2.5.7 included; WHEN it shows is CardChromeRule's) and NavStripRule (the strip's
    // sideways arithmetic).
    public class NavElementRegistryTests
    {
        // An app's own element: it remembers what it was bound with, so a test sees the sheet hand it the card's model and controller
        private sealed class AppDotsView : INavElementView
        {
            public VisualElement Root { get; } = new VisualElement { name = "app-dots" };
            public CardNavModel Model;
            public NavController Nav;
            public int Binds;
            public int Unbinds;

            public void Bind(CardNavModel model, NavController nav, BlockBindContext context)
            {
                Model = model;
                Nav = nav;
                Binds++;
            }

            public void Unbind()
            {
                Model = null;
                Nav = null;
                Unbinds++;
            }
        }

        private const string AppKey = "app_dots";
        // - it lists BLOCKS, one dot each (not sections): rule 2.5.7 leaves it on a one-family card
        private static NavElementDefinition AppDots(string slot = NavSlotKeys.Top) => new(AppKey, slot, "App Dots", "One dot per block.", NavChoice.Blocks);

        [Test]
        public void TheRegistry_RefusesANullOrBlankElement_TheWordNone_ASecondOfTheSameKey_AnUnbuiltSlot_AndNoFactory()
        {
            var registry = new NavElementRegistry();
            Assert.Throws<ArgumentNullException>(() => registry.Register(null, () => new AppDotsView()));
            Assert.Throws<ArgumentException>(() => registry.Register(new NavElementDefinition(" ", NavSlotKeys.Top, "x", "x", NavChoice.Sections), () => new AppDotsView()));
            Assert.Throws<ArgumentException>(() => registry.Register(new NavElementDefinition(CardOptions.NavNone, NavSlotKeys.Top, "x", "x", NavChoice.Sections), () => new AppDotsView()),
                "'none' is what a wall stores for no element: no element may be called that");
            var unbuilt = Assert.Throws<ArgumentException>(() => registry.Register(AppDots("floating"), () => new AppDotsView()));
            StringAssert.Contains("'floating'", unbuilt.Message, "the refusal names the slot");
            CollectionAssert.AreEqual(new[] { NavSlotKeys.Top, NavSlotKeys.Bottom }, NavSlotKeys.Built, "the slots the sheet draws today (step 7a: the bottom)");
            Assert.Throws<ArgumentException>(() => registry.Register(AppDots(null), () => new AppDotsView()));
            Assert.Throws<ArgumentNullException>(() => registry.Register(AppDots(), null));
            CollectionAssert.IsEmpty(registry.All, "nothing refused was kept");

            registry.Register(AppDots(), () => new AppDotsView());
            var twice = Assert.Throws<ArgumentException>(() => registry.Register(AppDots(), () => new AppDotsView()));
            StringAssert.Contains(AppKey, twice.Message);
            Assert.AreEqual(1, registry.All.Count);
        }

        // Section 5.1's top elements, in its order
        private static readonly string[] FrameworkTopElements =
        {
            BuiltInNavElements.TextTabs, BuiltInNavElements.IconTabs, BuiltInNavElements.IconTabsChips, BuiltInNavElements.KindChips,
            BuiltInNavElements.Segmented, BuiltInNavElements.ProgressMap,
        };

        [Test]
        public void TheFrameworksElements_AreRegisteredInShared_ForTheirSlot_WithEditorWordsInAscii()
        {
            Assert.IsTrue(NavElementRegistry.Shared.TryGet(BuiltInNavElements.TextTabs, out var tabs), "text_tabs is a framework element");
            Assert.AreEqual(NavSlotKeys.Top, tabs.Slot);
            Assert.AreEqual(NavChoice.Sections, tabs.Choice, "text tabs move between sections: rule 2.5.7 applies to them");
            Assert.AreEqual(new CardNavigationSettings().top, BuiltInNavElements.TextTabs, "Top Bar's default is the registered text_tabs");
            foreach (var element in NavElementRegistry.Shared.All)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(element.Label), element.Key + " has an Editor label");
                Assert.IsFalse(string.IsNullOrWhiteSpace(element.Help), element.Key + " has its (i) help");
                EditorTextChecks.AssertAscii(element.Label + element.Help, element.Key);
                Assert.IsInstanceOf<INavElementView>(NavElementRegistry.Shared.CreateView(element.Key), element.Key + " makes its view");
            }
            CollectionAssert.AreEqual(FrameworkTopElements, NavElementRegistry.Shared.OfSlot(NavSlotKeys.Top).Select(e => e.Key), "the six top elements of section 5.1, in its order");
            CollectionAssert.AreEqual(new[] { BuiltInNavElements.BottomBar }, NavElementRegistry.Shared.OfSlot(NavSlotKeys.Bottom).Select(e => e.Key),
                "the bottom slot's element of step 7a");
            Assert.IsTrue(NavElementRegistry.Shared.TryGet(BuiltInNavElements.BottomBar, out var bar));
            Assert.AreEqual(NavChoice.Sections, bar.Choice, "a bar moves between sections: rule 2.5.7 applies");
            Assert.IsTrue(bar.ReadsBarSlots && bar.ReadsLabels, "it reads Bar Slots and Labels");
            Assert.IsFalse(bar.ShowsCounts, "a count would crowd a fifth of the screen's width: no Show Counts");
            Assert.IsFalse(NavElementRegistry.Shared.All.Where(e => e.Slot == NavSlotKeys.Top).Any(e => e.ReadsBarSlots), "no top element is a bar of slots");
            Assert.AreEqual(CardOptions.NavNone, new CardNavigationSettings().bottom, "Bottom Bar's default is None");
            Assert.IsNull(NavElementRegistry.Shared.CreateView("no_such_element"));
        }

        // An app adds its own top element with no Framework edit: the registry lists it for its slot (what the Editor's Top Bar popup reads)
        // and a REAL sheet built over that registry shows it when the wall picks it -- bound to the card's own model and controller,
        // pooled across cards, let go when the card closes or the wall picks None
        [Test]
        public void AnAppsOwnElement_IsListedForItsSlot_AndARealSheetShowsItBoundToTheCardsModelAndController()
        {
            var registry = new NavElementRegistry();
            BuiltInNavElements.Register(registry);
            var made = new List<AppDotsView>();
            registry.Register(AppDots(), () => { var v = new AppDotsView(); made.Add(v); return v; });
            CollectionAssert.AreEqual(FrameworkTopElements.Append(AppKey), registry.OfSlot(NavSlotKeys.Top).Select(e => e.Key),
                "listed after the framework's, in registration order");

            var sheet = new PoiCardSheetView(new VisualElement(), BlockRegistry.Shared, Array.Empty<StyleSheet>(), registry);
            var wall = new WallConfigData { card_settings = new CardSettings() };
            wall.card_settings.navigation.top = AppKey;
            wall.card_settings.navigation.min_blocks = 2;
            var poi = TextPoi("app_point", 3);

            ShowCard(sheet, poi, wall);
            Assert.AreEqual(AppKey, sheet.Slots.TopKey, "the wall's pick is shown");
            Assert.AreEqual(1, made.Count);
            var view = made[0];
            Assert.AreSame(sheet.Nav, view.Nav, "bound to the card's own controller");
            Assert.AreSame(sheet.Nav.Model, view.Model, "and to the model the controller drives");
            Assert.AreEqual(1, view.Model.Sections.Count, "precondition: three About blocks make one section");
            Assert.AreSame(sheet.Slots.Top, view.Root.parent, "its root sits in the top slot");
            Assert.AreEqual(sheet.Root.IndexOf(sheet.Stack.Scroll) - 1, sheet.Root.IndexOf(sheet.Slots.Top), "the slot sits right before the stack's ScrollView");
            Assert.Less(sheet.Root.IndexOf(sheet.Stack.HeaderSlot), sheet.Root.IndexOf(sheet.Slots.Top), "and after the header");

            ShowCard(sheet, TextPoi("another_point", 4), wall);
            Assert.AreEqual(1, made.Count, "another card rebinds the same view (pooled per key)");
            Assert.AreEqual(2, view.Binds);

            wall.card_settings.navigation.top = CardOptions.NavNone;
            ShowCard(sheet, poi, wall);
            Assert.IsNull(sheet.Slots.TopView, "None: the slot is empty");
            Assert.IsNull(view.Nav, "and the app's view let go of the old card");
            Assert.AreEqual(0, sheet.Slots.Top.childCount);

            // - rule 2.5.7 is the ELEMENT's: on this one-section card Text Tabs (between sections) draws nothing, while the app's element,
            //   which lists blocks, was drawn above -- the card still shows navigation (ShowsNavigation is unchanged)
            wall.card_settings.navigation.top = BuiltInNavElements.TextTabs;
            ShowCard(sheet, poi, wall);
            Assert.IsTrue(sheet.Nav.Model.ShowsNavigation, "the one-section card still shows navigation");
            Assert.IsNull(sheet.Slots.TopView, "but Text Tabs, which moves between sections, is not drawn for one");

            wall.card_settings.navigation.top = AppKey;
            ShowCard(sheet, poi, wall);
            sheet.Hide();
            Assert.IsNull(sheet.Slots.TopView, "closing the card empties the slot");
            Assert.AreEqual(view.Binds, view.Unbinds, "every bind was unbound");
        }

        // A real sheet holds the strip in its slot and hides it at peek through the chrome (CardChromeRule: out of the flow, still laid
        // out), shows it at Half and Full -- this sheet has no layout, so no floor applies; the floor is PoiCardChromeSceneTests' -- and a
        // card below Show Navigation From holds no element at all
        [Test]
        public void TheTopSlot_IsHiddenAtPeekByTheChrome_AndEmptyForACardBelowShowNavigationFrom()
        {
            var sheet = new PoiCardSheetView(new VisualElement(), BlockRegistry.Shared, Array.Empty<StyleSheet>());
            var wall = new WallConfigData { card_settings = new CardSettings() };
            wall.card_settings.navigation.min_blocks = 3;
            ShowCard(sheet, TextPoi("long", 3, secondFamily: true), wall);
            Assert.AreEqual(BuiltInNavElements.TextTabs, sheet.Slots.TopKey, "the default Top Bar");
            Assert.AreEqual(DisplayStyle.Flex, sheet.Slots.Top.style.display.value, "a held element is in the sheet");
            foreach (var (stop, shown) in new[] { (SheetStopRule.Stop.Peek, false), (SheetStopRule.Stop.Half, true), (SheetStopRule.Stop.Full, true) })
            {
                sheet.SetStop(stop);
                Assert.AreEqual(shown, sheet.Chrome.TopShown, stop.ToString());
                Assert.AreEqual(!shown, sheet.Slots.Top.ClassListContains(CardChromeView.TopHiddenClass), stop + ": the hidden class");
            }
            ShowCard(sheet, TextPoi("short", 1, secondFamily: true), wall);
            Assert.IsNull(sheet.Slots.TopView, "two targets under Show Navigation From 3: no element");
            Assert.AreEqual(DisplayStyle.None, sheet.Slots.Top.style.display.value, "and no slot at all");
            Assert.IsFalse(sheet.Chrome.TopShown);
        }

        [Test]
        public void NavSlotRule_PicksTheWallsElement_OnlyWhenTheCardShowsNavigation_ItIsATopElement_AndRule7Allows()
        {
            var tabs = new NavElementDefinition(BuiltInNavElements.TextTabs, NavSlotKeys.Top, "Text Tabs", "x", NavChoice.Sections);
            Func<string, NavElementDefinition> top = key => key == BuiltInNavElements.TextTabs ? tabs : key == AppKey ? AppDots() : null;
            var wall = new WallConfigData { card_settings = new CardSettings() };
            wall.card_settings.navigation.min_blocks = 3;
            var twoSections = ModelOf(TextPoi("two", 3, secondFamily: true), wall);
            var oneSection = ModelOf(TextPoi("one", 3), wall);
            var shortCard = ModelOf(TextPoi("short", 1, secondFamily: true), wall);
            Assert.AreEqual(2, twoSections.Sections.Count, "precondition");
            Assert.AreEqual(1, oneSection.Sections.Count, "precondition");

            var nav = new CardNavigationSettings();
            Assert.AreEqual(BuiltInNavElements.TextTabs, NavSlotRule.TopElement(nav, twoSections, top), "the default");
            Assert.AreEqual(BuiltInNavElements.TextTabs, NavSlotRule.TopElement(null, twoSections, top), "no settings read as the defaults");
            Assert.IsNull(NavSlotRule.TopElement(nav, shortCard, top), "a card below Show Navigation From");
            Assert.IsNull(NavSlotRule.TopElement(nav, null, top), "no model");
            Assert.IsNull(NavSlotRule.TopElement(nav, oneSection, top), "rule 2.5.7: one section is no choice for an element between sections");
            nav.top = AppKey;
            Assert.AreEqual(AppKey, NavSlotRule.TopElement(nav, oneSection, top), "an element that lists blocks is drawn for one section");
            foreach (string none in new[] { CardOptions.NavNone, "", "  ", null })
            {
                nav.top = none;
                Assert.IsNull(NavSlotRule.TopElement(nav, twoSections, top), "'" + none + "' is no element");
            }
            nav.top = "unregistered";
            Assert.IsNull(NavSlotRule.TopElement(nav, twoSections, top), "a key no top element has: nothing, never another element");
        }

        // The bottom slot's rule is the top one's on the wall's Bottom Bar: the pick when the card shows navigation and rule 2.5.7 allows,
        // never a key registered for the OTHER slot, and the two slots answer independently
        [Test]
        public void NavSlotRule_BottomElement_IsTheWallsBottomBar_OnlyForTheBottomSlot_AndIndependentOfTheTop()
        {
            var registry = NavElementRegistry.Shared;
            Func<string, NavElementDefinition> bottom = key => registry.TryGet(key, out var d) && d.Slot == NavSlotKeys.Bottom ? d : null;
            Func<string, NavElementDefinition> top = key => registry.TryGet(key, out var d) && d.Slot == NavSlotKeys.Top ? d : null;
            var wall = new WallConfigData { card_settings = new CardSettings() };
            wall.card_settings.navigation.min_blocks = 3;
            var twoSections = ModelOf(TextPoi("two", 3, secondFamily: true), wall);
            var oneSection = ModelOf(TextPoi("one", 3), wall);
            var shortCard = ModelOf(TextPoi("short", 1, secondFamily: true), wall);

            var nav = new CardNavigationSettings();
            Assert.IsNull(NavSlotRule.BottomElement(nav, twoSections, bottom), "the default None: no bar");
            nav.bottom = BuiltInNavElements.BottomBar;
            Assert.AreEqual(BuiltInNavElements.BottomBar, NavSlotRule.BottomElement(nav, twoSections, bottom), "the wall's pick");
            Assert.AreEqual(BuiltInNavElements.TextTabs, NavSlotRule.TopElement(nav, twoSections, top), "and the Top Bar still answers for itself");
            Assert.IsNull(NavSlotRule.BottomElement(nav, shortCard, bottom), "a card below Show Navigation From");
            Assert.IsNull(NavSlotRule.BottomElement(nav, oneSection, bottom), "rule 2.5.7: one section is no choice for a bar");
            nav.bottom = BuiltInNavElements.TextTabs;
            Assert.IsNull(NavSlotRule.BottomElement(nav, twoSections, bottom), "a top element is never drawn in the bottom slot");
            nav.top = BuiltInNavElements.BottomBar;
            Assert.IsNull(NavSlotRule.TopElement(nav, twoSections, top), "nor the bar in the top slot");
        }

        // A real sheet: the bar is held in the bottom slot and shown with the bottom area -- at Half and Full, never at peek (this sheet
        // has no layout, so no floor applies; PoiCardBottomBarSceneTests measures the floor) -- and Full Only hides the bar alone at Half
        [Test]
        public void TheBottomSlot_ShowsWithTheBottomArea_NeverAtPeek_AndFullOnlyKeepsItForFull()
        {
            var sheet = new PoiCardSheetView(new VisualElement(), BlockRegistry.Shared, Array.Empty<StyleSheet>());
            var wall = new WallConfigData { card_settings = new CardSettings() };
            wall.card_settings.navigation.min_blocks = 3;
            wall.card_settings.navigation.bottom = BuiltInNavElements.BottomBar;
            ShowCard(sheet, TextPoi("long", 3, secondFamily: true), wall);
            Assert.AreEqual(BuiltInNavElements.BottomBar, sheet.Slots.BottomKey);
            Assert.AreEqual(BuiltInNavElements.TextTabs, sheet.Slots.TopKey, "the two slots hold their own elements");
            foreach (var (stop, shown) in new[] { (SheetStopRule.Stop.Peek, false), (SheetStopRule.Stop.Half, true), (SheetStopRule.Stop.Full, true) })
            {
                sheet.SetStop(stop);
                Assert.AreEqual(shown, sheet.Chrome.BottomShown, stop + ": the area");
                Assert.AreEqual(shown, sheet.Chrome.BottomNavShown, stop + ": the bar");
                Assert.AreEqual(!shown, sheet.BottomArea.ClassListContains(CardChromeView.BottomHiddenClass), stop + ": the area's hidden class");
                Assert.IsFalse(sheet.Slots.Bottom.ClassListContains(CardChromeView.BottomNavHiddenClass), stop + ": the bar is never hidden on its own here");
            }
            wall.card_settings.navigation.show_at = CardOptions.ShowNavFullOnly;
            ShowCard(sheet, TextPoi("long", 3, secondFamily: true), wall);
            sheet.SetStop(SheetStopRule.Stop.Half);
            Assert.IsFalse(sheet.Chrome.BottomShown, "Full Only at Half: this card has no footer, so the area is the bar alone and hides");
            sheet.SetStop(SheetStopRule.Stop.Full);
            Assert.IsTrue(sheet.Chrome.BottomNavShown, "at Full it shows");
        }

        [Test]
        public void NavStripRule_KeepsTheOffsetInsideTheStrip_AndMovesTheLeastToShowAWholeItem()
        {
            Assert.AreEqual(0f, NavStripRule.MaxOffset(300f, 390f), "a strip that fits does not move");
            Assert.AreEqual(210f, NavStripRule.MaxOffset(600f, 390f));
            Assert.AreEqual(0f, NavStripRule.Clamp(-20f, 600f, 390f), "never past its start");
            Assert.AreEqual(210f, NavStripRule.Clamp(500f, 600f, 390f), "never past its end");
            Assert.AreEqual(0f, NavStripRule.MaxOffset(float.NaN, 390f), "not laid out yet");
            // - an item already in view: unchanged
            Assert.AreEqual(50f, NavStripRule.OffsetToShow(100f, 80f, 390f, 600f, 50f));
            // - to the right: its end meets the right edge
            Assert.AreEqual(500f - 390f, NavStripRule.OffsetToShow(420f, 80f, 390f, 600f, 0f));
            // - to the left: its start meets the left edge
            Assert.AreEqual(30f, NavStripRule.OffsetToShow(30f, 80f, 390f, 600f, 150f));
            // - the last item: clamped to the end, never beyond
            Assert.AreEqual(210f, NavStripRule.OffsetToShow(560f, 40f, 390f, 600f, 0f));
            Assert.AreEqual(40f, NavStripRule.OffsetToShow(float.NaN, 80f, 390f, 600f, 40f), "an item not laid out keeps the offset");
        }

        [Test]
        public void SwipeGrabRule_ClaimsVertical_IsTheOtherAxis_AndNoMoveIsClaimedByBoth()
        {
            Assert.IsFalse(SwipeGrabRule.ClaimsVertical(new Vector2(0f, SwipeGrabRule.Slop)), "exactly the slop: not yet");
            Assert.IsTrue(SwipeGrabRule.ClaimsVertical(new Vector2(0f, SwipeGrabRule.Slop + 1f)), "down past the slop");
            Assert.IsTrue(SwipeGrabRule.ClaimsVertical(new Vector2(5f, -60f)), "up, a little off straight");
            Assert.IsFalse(SwipeGrabRule.ClaimsVertical(new Vector2(60f, 20f)), "mostly sideways: the strip's");
            Assert.IsFalse(SwipeGrabRule.ClaimsVertical(new Vector2(40f, 40f)), "an exact diagonal is nobody's");
            for (float x = -60f; x <= 60f; x += 7.5f)
                for (float y = -60f; y <= 60f; y += 7.5f)
                {
                    var travel = new Vector2(x, y);
                    Assert.IsFalse(SwipeGrabRule.ClaimsPointer(travel) && SwipeGrabRule.ClaimsVertical(travel), travel + " claimed by both");
                }
        }

        // A card of `textBlocks` rich texts under a header: every one an About target; `secondFamily` adds a Show On Wall button (In AR,
        // a field-less block), so the card has two sections
        private static POIData TextPoi(string id, int textBlocks, bool secondFamily = false)
        {
            var poi = new POIData { id = id, name = id, card = new POICardData() };
            for (int i = 0; i < textBlocks; i++)
            {
                var block = new BlockInstanceData { key = "block_" + (i + 1), kind = BuiltInBlocks.RichTextKind };
                block.fields.Add(new BlockFieldValue { key = BuiltInBlocks.RichTextBodyField, text = new List<LocalizedEntry> { new() { lang = "en", value = "Text " + i } } });
                poi.card.blocks.Add(block);
            }
            if (secondFamily)
                poi.card.blocks.Add(new BlockInstanceData { key = "block_" + (textBlocks + 1), kind = BuiltInBlocks.ShowOnWallKind });
            return poi;
        }

        private static CardNavModel ModelOf(POIData poi, WallConfigData wall) =>
            CardNavModel.Build(BlockStackBuilder.Build(poi, wall.card_settings, BlockRegistry.Shared).Entries, BlockRegistry.Shared, wall.card_settings);

        private static void ShowCard(PoiCardSheetView sheet, POIData poi, WallConfigData wall)
        {
            var stack = BlockStackBuilder.Build(poi, wall.card_settings, BlockRegistry.Shared);
            var context = CardContextBuilder.Build(new BlockBindContext(), poi, wall, wall.card_settings, "en", null, null);
            sheet.Show(stack.Entries, context, SheetStopRule.Stop.Half, wall.card_settings.container.half_max_ratio);
        }
    }
}
