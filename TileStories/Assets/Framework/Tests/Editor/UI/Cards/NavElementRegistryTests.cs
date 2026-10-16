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
        private static NavElementDefinition AppDots(string slot = NavSlotKeys.Top) => new(AppKey, slot, "App Dots", "One dot per block.", betweenSections: false);

        [Test]
        public void TheRegistry_RefusesANullOrBlankElement_TheWordNone_ASecondOfTheSameKey_AnUnbuiltSlot_AndNoFactory()
        {
            var registry = new NavElementRegistry();
            Assert.Throws<ArgumentNullException>(() => registry.Register(null, () => new AppDotsView()));
            Assert.Throws<ArgumentException>(() => registry.Register(new NavElementDefinition(" ", NavSlotKeys.Top, "x", "x", true), () => new AppDotsView()));
            Assert.Throws<ArgumentException>(() => registry.Register(new NavElementDefinition(CardOptions.NavNone, NavSlotKeys.Top, "x", "x", true), () => new AppDotsView()),
                "'none' is what a wall stores for no element: no element may be called that");
            var unbuilt = Assert.Throws<ArgumentException>(() => registry.Register(AppDots("bottom"), () => new AppDotsView()));
            StringAssert.Contains("'bottom'", unbuilt.Message, "the refusal names the slot");
            Assert.Throws<ArgumentException>(() => registry.Register(AppDots(null), () => new AppDotsView()));
            Assert.Throws<ArgumentNullException>(() => registry.Register(AppDots(), null));
            CollectionAssert.IsEmpty(registry.All, "nothing refused was kept");

            registry.Register(AppDots(), () => new AppDotsView());
            var twice = Assert.Throws<ArgumentException>(() => registry.Register(AppDots(), () => new AppDotsView()));
            StringAssert.Contains(AppKey, twice.Message);
            Assert.AreEqual(1, registry.All.Count);
        }

        [Test]
        public void TheFrameworksElements_AreRegisteredInShared_ForTheirSlot_WithEditorWordsInAscii()
        {
            Assert.IsTrue(NavElementRegistry.Shared.TryGet(BuiltInNavElements.TextTabs, out var tabs), "text_tabs is a framework element");
            Assert.AreEqual(NavSlotKeys.Top, tabs.Slot);
            Assert.IsTrue(tabs.BetweenSections, "text tabs move between sections: rule 2.5.7 applies to them");
            Assert.AreEqual(new CardNavigationSettings().top, BuiltInNavElements.TextTabs, "Top Bar's default is the registered text_tabs");
            foreach (var element in NavElementRegistry.Shared.All)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(element.Label), element.Key + " has an Editor label");
                Assert.IsFalse(string.IsNullOrWhiteSpace(element.Help), element.Key + " has its (i) help");
                EditorTextChecks.AssertAscii(element.Label + element.Help, element.Key);
                Assert.IsInstanceOf<INavElementView>(NavElementRegistry.Shared.CreateView(element.Key), element.Key + " makes its view");
            }
            CollectionAssert.AreEqual(new[] { BuiltInNavElements.TextTabs }, NavElementRegistry.Shared.OfSlot(NavSlotKeys.Top).Select(e => e.Key));
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
            CollectionAssert.AreEqual(new[] { BuiltInNavElements.TextTabs, AppKey }, registry.OfSlot(NavSlotKeys.Top).Select(e => e.Key),
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
            var tabs = new NavElementDefinition(BuiltInNavElements.TextTabs, NavSlotKeys.Top, "Text Tabs", "x", betweenSections: true);
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
