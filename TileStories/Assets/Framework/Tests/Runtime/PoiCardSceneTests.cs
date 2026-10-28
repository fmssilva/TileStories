using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // Phase B of the POI Detail Card (_3.1 section 7): the REAL LivingRoomScene with its PoiCard object, the
    // shipped config (fixture: "The Lamp" = the authored image_parallax header with the wall's pictures, "Lamp - Military" = the compact one, every
    // other POI = no card), real selection through the bus and real marker taps, real pointer drags on the sheet,
    // and the three ways to close: the X, a drag below peek, a tap on empty camera space (EventSystem raycast).
    public partial class PoiCardSceneTests : SearchSceneFixture
    {
        private PoiCardSheetView Sheet => Card.Sheet;
        private HeaderBlockView Header => (HeaderBlockView)Sheet.Stack.BoundViews[0];

        // The running wall's own card settings (the host reads them on every selection)
        private CardSettings LiveSettings => Session.CardSettings;

        private IEnumerator Select(string poiId)
        {
            SelectionEventBus.Select(poiId);
            yield return CardTestInput.Settle();
        }

        // The screen point (pixels) at the centre of a card element
        private static Vector2 ScreenPointOf(VisualElement e)
        {
            var panelSize = e.panel.visualTree.layout.size;
            var c = e.worldBound.center;
            return new Vector2(c.x * Screen.width / panelSize.x, Screen.height - c.y * Screen.height / panelSize.y);
        }

        [UnityTest]
        public IEnumerator TheLamp_OpensAtPeek_WithItsAuthoredHeader_OnlyTheTopShown()
        {
            Assert.IsFalse(Sheet.IsOpen, "nothing selected: no card");
            yield return Select("lamp");
            Assert.IsTrue(Sheet.IsOpen);
            Assert.AreEqual("lamp", Card.ShownPoiId);
            Assert.AreEqual(SheetStopRule.Stop.Peek, Sheet.Stop, "Open At = peek (the shipped default)");
            Assert.AreEqual("St George's Castle", Header.TitleText, "the authored title in the wall's first language, not the POI name");
            // - _3.3 P4.4a: the header names no category any more (Quick Facts > Show Category does, QuickFactsShowTheCategory...)
            Assert.IsNull(Header.Root.Q(className: "card-chip"), "no category chip in the header");
            Assert.AreEqual("Medieval castle and royal palace", Header.SubtitleText);

            Rect card = Sheet.Root.worldBound;
            Assert.LessOrEqual(Header.PeekPart.worldBound.yMax, card.yMax + 1f, "peek shows the whole bar");
            Assert.IsTrue(Header.BarTitleShown, "at Peek the bar carries the title");
            Assert.GreaterOrEqual(Header.HeroPart.Q<Label>("card-header-subtitle").worldBound.yMin, card.yMax - 1f, "...and nothing of the hero below it");
            Assert.AreEqual(ConfigCopy().pois.Single(p => p.id == "lamp").name, Session.SearchPois.First(p => p.id == "lamp").name, "the card never renames the POI");
            yield return Capture("Card_Lamp_Peek");
        }

        [Category("Core")]
        [UnityTest]
        public IEnumerator DraggingUp_GoesToHalf_AtMost40Percent_ThenFull()
        {
            yield return Select("lamp");
            yield return CardTestInput.Drag(Sheet.Handle, -(Sheet.Stops.Half - Sheet.Stops.Peek), slowSheet: Sheet);
            yield return CardTestInput.Settle();
            Assert.AreEqual(SheetStopRule.Stop.Half, Sheet.Stop);
            Assert.LessOrEqual(Sheet.Root.resolvedStyle.height, Sheet.Layer.layout.height * 0.40f + 1f, "half never passes 40% of the screen");
            // - _3.3 S4a: the hero is the whole sheet at Full, its words at its foot -- Half shows the hero's top, the words wait below
            Rect hero = Header.HeroPart.worldBound, view = Sheet.Stack.Scroll.contentViewport.worldBound;
            Assert.AreEqual(view.yMin, hero.yMin, 1f, "half shows the hero's top");
            Assert.Greater(hero.yMax, view.yMax + 1f, "...and the hero runs on under Half's foot");
            yield return Capture("Card_Lamp_Half");

            yield return CardTestInput.Drag(Sheet.Handle, -(Sheet.Stops.Full - Sheet.Stops.Half), slowSheet: Sheet);
            yield return CardTestInput.Settle();
            Assert.AreEqual(SheetStopRule.Stop.Full, Sheet.Stop);
            var container = ConfigCopy().card_settings.container;
            Assert.AreEqual(SheetStopRule.FullTopGap(container.full_height, CardTestInput.TokenPx("--ts-sheet-top-gap")),
                Sheet.Layer.layout.height - Sheet.Stops.Full, 1f, "full leaves the gap the wall's Full Height asks for");
            Assert.AreEqual("lamp", SelectionEventBus.CurrentPoiId, "dragging never changes the selection");
            yield return Capture("Card_Lamp_Full");
        }

        // _3.1 step 6C: the full stop would cover the search bar half-way, so the search top (bar, filter tray, view
        // switch) steps aside while the card rests at full, and comes back at any lower stop or when the card closes
        [UnityTest]
        public IEnumerator AtFull_TheSearchBarStepsAside_BelowFullTheCardNeverOverlapsIt_AndItComesBackOnClose()
        {
            var top = Host.Root.Q("search-top");
            Host.SetQuery(ConfigCopy().pois.Single(p => p.id == "lamp").name);
            yield return Select("lamp");
            Assert.IsTrue(Host.TopShown, "peek: the search bar shows");
            yield return CardTestInput.Drag(Sheet.Handle, -(Sheet.Stops.Half - Sheet.Stops.Peek), slowSheet: Sheet);
            yield return CardTestInput.Settle();
            Assert.AreEqual(SheetStopRule.Stop.Half, Sheet.Stop);
            Assert.IsTrue(Host.TopShown, "half: the search bar shows");
            Assert.AreSame(Sheet.Root.panel, top.panel, "precondition: one shared panel, one coordinate space");
            Assert.GreaterOrEqual(Sheet.Root.worldBound.yMin, top.worldBound.yMax, "half: the sheet's top edge stays below the search bar");
            Rect bar = top.worldBound;

            yield return CardTestInput.Drag(Sheet.Handle, -(Sheet.Stops.Full - Sheet.Stops.Half), slowSheet: Sheet);
            yield return CardTestInput.Settle();
            Assert.AreEqual(SheetStopRule.Stop.Full, Sheet.Stop, "a real drag up to full");
            Assert.IsFalse(Host.TopShown, "full: the search bar steps aside");
            Assert.Less(Sheet.Root.worldBound.yMin, bar.yMax, "precondition: the full card reaches into where the bar was (the overlap 6C removes)");
            var inOverlap = new Vector2(bar.center.x, (Sheet.Root.worldBound.yMin + bar.yMax) / 2f);
            var picked = top.panel.Pick(inOverlap);
            Assert.IsTrue(picked != null && Sheet.Root.Contains(picked), "a tap where card and bar overlapped lands on the card, nothing else");
            Assert.AreEqual(ConfigCopy().pois.Single(p => p.id == "lamp").name, Host.Query, "the query is kept while the bar is away");
            yield return Capture("Card_Lamp_Full_SearchAside");

            SelectionEventBus.Select("lamp_military");
            yield return CardTestInput.Settle();
            Assert.AreEqual(SheetStopRule.Stop.Full, Sheet.Stop, "another POI keeps the stop");
            Assert.IsFalse(Host.TopShown, "...and the bar stays aside");

            yield return CardTestInput.Drag(Sheet.Handle, Sheet.Stops.Full - Sheet.Stops.Half, slowSheet: Sheet);
            yield return CardTestInput.Settle();
            Assert.AreEqual(SheetStopRule.Stop.Half, Sheet.Stop);
            Assert.IsTrue(Host.TopShown, "back at half: the bar is back");
            Assert.GreaterOrEqual(Sheet.Root.worldBound.yMin, top.worldBound.yMax, "...above the card");

            SetStopFull();
            yield return CardTestInput.Settle();
            Assert.IsFalse(Host.TopShown);
            Press(Sheet.CloseButton);
            yield return CardTestInput.Settle();
            Assert.IsFalse(Sheet.IsOpen);
            Assert.IsTrue(Host.TopShown, "the card closed at full: the bar is back");
            Assert.AreEqual(ConfigCopy().pois.Single(p => p.id == "lamp").name, Host.Root.Q<TextField>("search-field").value, "with the visitor's query still in it");
        }

        private void SetStopFull() => Sheet.SetStop(SheetStopRule.Stop.Full);

        // _3.3 P4.4a (was _3.1 6C's collapse): the pinned header is the BAR -- one row that never changes size. At the top of Full the hero
        // carries the title and the bar shows none; a real wheel scroll that takes the hero's title away hands it to the bar, and the top
        // gives it back. Neither ever moves a stop or the bar's height
        [UnityTest]
        public IEnumerator ScrollingTheHeroAway_HandsTheTitleToTheBar_AndTheTopGivesItBack()
        {
            yield return OpenFull("lamp");
            var stack = Sheet.Stack;
            var headerView = Header;
            float bar = stack.HeaderSlot.worldBound.height;
            var openStops = Sheet.Stops;
            Assert.IsFalse(headerView.BarTitleShown, "precondition: at the top of Full the hero carries the title");
            Assert.IsTrue(CardTestInput.IsShown(headerView.HeroTitle, stack.Scroll.contentViewport), "the hero's title is in view");
            var title = headerView.Root.Q<Label>("card-header-title");
            Assert.AreEqual(WhiteSpace.NoWrap, title.resolvedStyle.whiteSpace, "the bar's title is one line: a long title is cut, never wrapped");
            Assert.Less(title.resolvedStyle.opacity, 0.01f, "the bar's title is not shown");

            float heroTitleBottom = headerView.HeroTitleBottom();
            for (int i = 0; i < 12 && stack.Scroll.scrollOffset.y < heroTitleBottom + 20f; i++)
            {
                yield return CardTestInput.Wheel(stack.Scroll.contentViewport, 12f, frames: 4);
                heroTitleBottom = headerView.HeroTitleBottom();
            }
            yield return CardTestInput.Settle(0.4f);
            Assert.Greater(stack.Scroll.scrollOffset.y, headerView.HeroTitleBottom(), "precondition: the real wheel scrolled the hero's title away (offset "
                + stack.Scroll.scrollOffset.y + ", title bottom " + headerView.HeroTitleBottom() + ")");
            Assert.IsTrue(headerView.BarTitleShown && stack.BarShowsTitle, "the bar carries the title now");
            Assert.AreEqual(1f, title.resolvedStyle.opacity, 0.01f, "...shown whole");
            Assert.AreEqual("St George's Castle", headerView.TitleText, "the same title");
            Assert.AreSame(headerView, Header, "the same header view, no second header");
            Assert.AreEqual(bar, stack.HeaderSlot.worldBound.height, 0.5f, "the bar never changes size");
            Assert.AreEqual(openStops.Peek, Sheet.Stops.Peek, 0.5f, "...so the stops never move");
            yield return Capture("Card_Lamp_Full_BarTitle");

            for (int i = 0; i < 12 && stack.Scroll.scrollOffset.y > 0.01f; i++)
                yield return CardTestInput.Wheel(stack.Scroll.contentViewport, -40f, frames: 4);
            yield return CardTestInput.Settle(0.4f);
            Assert.AreEqual(0f, stack.Scroll.scrollOffset.y, 0.01f, "wheeled back to the top");
            Assert.IsFalse(headerView.BarTitleShown, "at the top: the hero carries the title again");
            Assert.AreEqual(bar, stack.HeaderSlot.worldBound.height, 0.5f, "the bar is the same row");
            yield return Capture("Card_Lamp_Full_HeroTitle");

            // - a new card always starts with its hero whole, even after a scrolled one
            for (int i = 0; i < 12 && !stack.BarShowsTitle; i++)
                yield return CardTestInput.Wheel(stack.Scroll.contentViewport, 12f, frames: 4);
            Assert.IsTrue(stack.BarShowsTitle, "precondition: scrolled away again");
            SelectionEventBus.Select("lamp_military");
            yield return CardTestInput.Settle();
            Assert.IsFalse(stack.BarShowsTitle, "another POI's card opens with its hero carrying the title");
        }

        [Category("Core")]
        [UnityTest]
        public IEnumerator TheX_ClosesTheCard_AndClearsTheSelection()
        {
            yield return Select("lamp");
            Press(Sheet.CloseButton);
            yield return null;
            Assert.IsNull(SelectionEventBus.CurrentPoiId, "the X clears through the bus");
            Assert.IsFalse(Sheet.IsOpen);
            Assert.IsNull(Card.ShownPoiId);
            Assert.AreEqual(Session.SpawnedMarkers.Count, MarkersAt(1f).Count, "every marker back to full");
        }

        [Category("Core")]
        [UnityTest]
        public IEnumerator PullingTheCardDownBelowPeek_ClosesIt()
        {
            yield return Select("lamp");
            yield return CardTestInput.Drag(Sheet.Handle, Sheet.Stops.Peek * 0.8f);
            yield return null;
            Assert.IsNull(SelectionEventBus.CurrentPoiId, "a swipe down past peek clears the selection");
            Assert.IsFalse(Sheet.IsOpen);
        }

        [Category("Core")]
        [UnityTest]
        public IEnumerator ATapOnEmptyCameraSpace_Closes_ButATapOnTheCardOrAMarkerDoesNot()
        {
            yield return Select("lamp");
            Assert.IsTrue(ScreenUIHit.IsOverAnything(ScreenPointOf(Sheet.CloseButton)), "the EventSystem sees the card (UI Toolkit panel raycaster)");
            Assert.IsFalse(Card.HandleScreenTap(ScreenPointOf(Header.PeekPart), ScreenPointOf(Header.PeekPart), Time.unscaledTime - 0.1f, Time.unscaledTime), "a tap on the card");

            var marker = Session.SpawnedMarkers.Where(m => m != null && m.IsVisible).Select(m => ScreenPointOf(m)).FirstOrDefault(p => p.HasValue && p.Value.y > Screen.height * 0.5f);
            Assert.IsTrue(marker.HasValue, "precondition: a marker on screen above the card");
            Assert.IsFalse(Card.HandleScreenTap(marker.Value, marker.Value, Time.unscaledTime - 0.1f, Time.unscaledTime), "a tap on a marker selects it, it does not close the card");
            Assert.IsTrue(Sheet.IsOpen);

            var empty = EmptyScreenPoint();
            float now = Time.unscaledTime;
            Assert.IsFalse(Card.HandleScreenTap(empty, empty + new Vector2(0f, Screen.height * 0.1f), now - 0.1f, now), "a drag across the wall is not a tap");
            Assert.IsTrue(Sheet.IsOpen);
            Assert.IsFalse(Card.ClosePending);

            // - zoom is on in the shipped wall: the close waits out its double-tap window (TapOutsideDismissal)
            float window = Session.ZoomSettings.double_tap_window_s;
            Assert.IsTrue(Session.ZoomSettings.enabled && window > 0f, "precondition: the shipped wall has double-tap zoom");
            now = Time.unscaledTime;
            Assert.IsFalse(Card.HandleScreenTap(empty, empty, now - 0.1f, now), "a tap on nothing does not close at once...");
            Assert.IsTrue(Card.ClosePending, "...it waits");
            Assert.IsTrue(Sheet.IsOpen);
            yield return CardTestInput.Settle(window + 0.1f);
            Assert.IsNull(SelectionEventBus.CurrentPoiId, "...and closes once the window passed with no second tap");
            Assert.IsFalse(Sheet.IsOpen);
            Assert.IsFalse(Card.ClosePending);
        }

        [UnityTest]
        public IEnumerator ZoomOff_ATapOnNothing_ClosesAtOnce()
        {
            Session.ZoomSettings.enabled = false;
            yield return Select("lamp");
            var empty = EmptyScreenPoint();
            float now = Time.unscaledTime;
            Assert.IsTrue(Card.HandleScreenTap(empty, empty, now - 0.1f, now), "no double tap exists without zoom: nothing to wait for");
            Assert.IsNull(SelectionEventBus.CurrentPoiId);
            Assert.IsFalse(Sheet.IsOpen);
        }

        [UnityTest]
        public IEnumerator APendingClose_IsCancelledBySelectingAnotherPoi()
        {
            yield return Select("lamp");
            var empty = EmptyScreenPoint();
            float now = Time.unscaledTime;
            Card.HandleScreenTap(empty, empty, now - 0.1f, now);
            Assert.IsTrue(Card.ClosePending);
            SelectionEventBus.Select("lamp_military");
            yield return CardTestInput.Settle(Session.ZoomSettings.double_tap_window_s + 0.1f);
            Assert.AreEqual("lamp_military", SelectionEventBus.CurrentPoiId, "the old tap never closes the newly selected card");
            Assert.IsTrue(Sheet.IsOpen);
        }

        [UnityTest]
        public IEnumerator TapOutsideCloses_Off_KeepsTheCardOpen()
        {
            LiveSettings.container.dismiss_on_tap_outside = false;
            yield return Select("lamp");
            var empty = EmptyScreenPoint();
            Assert.IsFalse(Card.HandleScreenTap(empty, empty, Time.unscaledTime - 0.1f, Time.unscaledTime));
            Assert.IsFalse(Card.ClosePending, "Tap Outside Closes off: nothing is even pending");
            yield return CardTestInput.Settle(Session.ZoomSettings.double_tap_window_s + 0.1f);
            Assert.IsTrue(Sheet.IsOpen, "Tap Outside Closes off: the card stays");
        }

        [UnityTest]
        public IEnumerator SelectingAnotherPoi_WhileOpen_KeepsTheStop_AndRebindsPooledViews()
        {
            yield return Select("lamp");
            Sheet.SetStop(SheetStopRule.Stop.Half);
            yield return CardTestInput.Settle();
            int built = Sheet.Stack.CreatedViewCount;

            yield return Select("lamp_military");
            Assert.AreEqual(SheetStopRule.Stop.Half, Sheet.Stop, "switching POIs keeps the stop");
            Assert.AreEqual("lamp_military", Card.ShownPoiId);
            Assert.AreEqual("Castle Keep", Header.TitleText);
            Assert.IsFalse(Header.SubtitleShown, "no subtitle authored: none shown");
            Assert.IsTrue(Header.HasPicture, "its Video Loop hero draws over its poster");
            Assert.AreEqual(built, Sheet.Stack.CreatedViewCount, "the header view was reused, not rebuilt");
        }

        [UnityTest]
        public IEnumerator APoiWithNoCard_ShowsItsNameAndSummary()
        {
            // - The Camera: a point with no blocks (The Painting has its own card since _3.2 step 17b)
            yield return Select("camera");
            var camera = ConfigCopy().pois.Single(p => p.id == "camera");
            Assert.AreEqual(camera.name, Header.TitleText, "no header block: the point's name");
            Assert.AreEqual(camera.summary, Header.SubtitleText, "and its summary");
            Assert.AreEqual(1, Sheet.Stack.BoundViews.Count, "header only");
        }

        [UnityTest]
        public IEnumerator EnableDetailCard_Off_SelectionStillWorks_NoCard()
        {
            LiveSettings.enabled = false;
            yield return Select("lamp");
            Assert.AreEqual("lamp", SelectionEventBus.CurrentPoiId, "selection still works");
            Assert.IsFalse(Sheet.IsOpen, "no card");
        }

        [UnityTest]
        public IEnumerator OpenAtHalf_AndAHalfCap_AreHonoured()
        {
            LiveSettings.container.open_stop = CardOptions.StopHalf;
            LiveSettings.container.half_max_ratio = 0.25f;
            // - a header-only point (no authored card): its peek content (title + chip only, no pinned audio chip) is short enough
            //   for the cap to actually bind; a mechanism test should not depend on how tall a specific point's own content is
            yield return Select("camera");
            Assert.AreEqual(SheetStopRule.Stop.Half, Sheet.Stop, "Open At = half");
            Assert.AreEqual(Sheet.Layer.layout.height * 0.25f, Sheet.Root.resolvedStyle.height, 1f, "Half Height Max = 25%");
        }

        [UnityTest]
        public IEnumerator TheFirstLanguage_IsTheOneShown()
        {
            LiveSettings.languages = new System.Collections.Generic.List<string> { "pt", "en" };
            yield return Select("lamp");
            Assert.AreEqual("Castelo de São Jorge", Header.TitleText);
            Assert.AreEqual("Castelo medieval e paço real", Header.SubtitleText);
            // - _3.3 P1b: the category's own Portuguese name (Category Names Per Language), never the English label on a PT card -- shown by
            //   the first Quick Facts block's Show Category since P4.4a
            var facts = Sheet.Stack.BoundViews.OfType<QuickFactsBlockView>().First();
            Assert.AreEqual("Categoria", facts.Facts[0].Label.text, "the Category fact first, its label in the card's language");
            Assert.AreEqual("Governo Real", facts.Facts[0].Value.text, "the category in the card's language");
        }

        [UnityTest]
        public IEnumerator TheCloseButton_IsNamedFromTheStringTable_InTheCardsLanguage_AndTheWallsCardTextsWin()
        {
            Assert.IsNotNull(Card.StringTable, "the scene's card has the framework string table");
            yield return Select("lamp");
            Assert.AreEqual("", Sheet.CloseButton.text, "no words on it: the mark says it");
            Assert.AreEqual(CardIcons.Shape.Close, Sheet.CloseGlyph.Kind, "the X is a drawn mark (v5)");
            Assert.IsTrue(Sheet.CloseButton.Contains(Sheet.CloseGlyph), "drawn on the button");
            Assert.AreEqual("Close", Sheet.CloseButton.tooltip, "framework English");

            SelectionEventBus.Clear();
            LiveSettings.languages = new System.Collections.Generic.List<string> { "pt", "en" };
            yield return Select("lamp");
            Assert.AreEqual("Fechar", Sheet.CloseButton.tooltip, "framework Portuguese for a Portuguese card");

            SelectionEventBus.Clear();
            LiveSettings.strings.Add(new CardStringEntry { key = CardStrings.Keys.Close, text = new System.Collections.Generic.List<LocalizedEntry> { new() { lang = "pt", value = "Sair" } } });
            yield return Select("lamp");
            Assert.AreEqual("Sair", Sheet.CloseButton.tooltip, "the wall's Card Texts wording wins");
        }

        // Open `poiId` at full and wait for the layout
        private IEnumerator OpenFull(string poiId)
        {
            yield return Select(poiId);
            Sheet.SetStop(SheetStopRule.Stop.Full);
            yield return CardTestInput.Settle();
        }

        private System.Collections.Generic.List<string> ShownKinds() =>
            Sheet.Stack.BoundViews.Select(v => BlockRegistry.Shared.All.First(k => BlockRegistry.Shared.CreateView(k.Key).GetType() == v.GetType()).Key).ToList();

        [UnityTest]
        public IEnumerator TheLamp_IsTheFullCard_EveryKindAndVariantOfTiers1To4_InCatalogOrder()
        {
            yield return OpenFull("lamp");
            // - a kind the wall's own app registered (LivingRoom's size_comparison, _3.1 step 11) is that app's to test, not the framework
            //   catalog's: this list is what BuiltInBlocks registers
            var builtIn = new BlockRegistry();
            BuiltInBlocks.Register(builtIn);
            CollectionAssert.AreEqual(new[]
            {
                "header", "status", "status", "quick_facts", "quick_facts", "quick_facts", "rich_text", "rich_text", "rich_text", "rich_text",
                "fun_fact", "fun_fact", "pull_quote", "pull_quote",
                "process_steps", "swatches", "timeline", "timeline", "person", "person", "story_chapters", "compare_points", "practical_info",
                "gallery", "gallery", "gallery", "gallery", "before_after", "zoom_image",
                "hotspot_image", "hotspot_image", "video", "video", "wall_locator", "wall_locator", "today_map", "today_map", "related", "related",
                "knowledge_check", "knowledge_check", "knowledge_check", "poll", "collect", "feedback", "feedback", "dialogue", "show_on_wall", "show_on_wall",
                // - _3.3 P4.4a: the hero_chip audio guide is the hero's Listen (HeroActionViews), only the player is in the stack
                "audio_guide",
                "actions", "actions", "actions", "sources", "sources",
                // - the sources were authored before the actions: the card still ends with them (Sources At The End, 15.2.1)
                // - Tier 5 (10A.2b.3 on): the Lamp's own model_3d, panorama_360 and place_in_ar fixtures, excluded from THIS list on purpose
                //   (its own name says Tiers 1 to 4) rather than folded in as more rows every time a later tier adds one
            }, ShownKinds().Where(k => builtIn.TryGet(k, out _) && k != BuiltInBlocks.Model3DKind && k != BuiltInBlocks.Panorama360Kind
                && k != BuiltInBlocks.PlaceInArKind).ToList(),
                "one block per kind and variant of Tiers 1 to 4, nothing skipped");
            var lampConfig = Session.SearchPois.First(p => p.id == "lamp");
            CollectionAssert.IsEmpty(BlockStackBuilder.Build(lampConfig, LiveSettings, BlockRegistry.Shared, Session.SearchPois).Skipped, "no authored block skipped");
            var variants = Sheet.Stack.BoundViews.Select(v => v.Root.GetClasses().FirstOrDefault(c => c.Contains("--"))).Where(c => c != null).ToList();
            foreach (string look in new[] { "card-timeline--vertical", "card-timeline--horizontal", "card-person--row", "card-person--card" })
                CollectionAssert.Contains(variants, look, "each variant on the card");
            var sticky = Sheet.Stack.BoundViews.OfType<ActionsBlockView>().Last();
            Assert.AreEqual(Sheet.Stack.Footer, Sheet.Stack.SlotOf(sticky).parent, "the sticky actions are pinned to the footer");
            CollectionAssert.AreEqual(new[] { typeof(ActionsBlockView), typeof(AudioGuideBlockView) }, Sheet.Stack.HeroActionViews.Select(v => v.GetType()),
                "the hero holds the sticky action and Listen (the hero chip)");
        }

        // _3.1 step 6C: every block of The Lamp shows its authored heading above it (a kind with a default heading --
        // compare, sources -- shows that when none is written, the rest nothing), in the card's language, and the gap under
        // every scrolling block is the one --ts-block-gap token
        [UnityTest]
        public IEnumerator TheLamp_EveryBlockShowsItsHeadingAboveIt_InTheCardsLanguage_AndEveryGapIsTheOneToken()
        {
            yield return OpenFull("lamp");
            // - the question that waits for the reading is revealed the way a visitor does it, so every slot can be measured
            yield return ReadTheWholeCard();
            var lampConfig = Session.SearchPois.First(p => p.id == "lamp");
            var strings = new CardStrings(Card.StringTable.Entries(), Card.StringSources.Entries(), LiveSettings.strings, "en", "en");
            var stack = Sheet.Stack;
            var views = stack.BoundViews;
            // - the card's order, not the authored one: the sources close the card (Sources At The End)
            var built = BlockStackBuilder.Build(lampConfig, LiveSettings, BlockRegistry.Shared, Session.SearchPois).Entries;
            Assert.AreEqual(lampConfig.card.blocks.Count, built.Count, "precondition: every authored block is on the card");
            // - _3.3 P4.4a: a pinned-top look (the hero chip) is the hero's Listen, with no slot or heading of its own
            var stackOrder = built.Where(e => !e.Definition.IsPinnedTop(e.Variant)).ToList();
            Assert.AreEqual(stackOrder.Count, views.Count, "precondition: every block shown, in the card's order");
            float gap = -1f;
            int headed = 0;
            // - layout snaps each edge to a physical pixel: one screen pixel in panel units is the honest tolerance
            float onePixel = RuntimePanelUtils.ScreenToPanel(stack.Scroll.panel, Vector2.right).x - RuntimePanelUtils.ScreenToPanel(stack.Scroll.panel, Vector2.zero).x;
            for (int i = 1; i < views.Count; i++)
            {
                var block = stackOrder[i].Instance;
                BlockRegistry.Shared.TryGet(block.kind, out var definition);
                string authored = new BlockFieldReader(block, "en", "en").Text(BlockKindDefinition.HeadingField);
                string expected = authored.Length > 0 ? authored : definition.DefaultHeadingKey != null ? strings.Get(definition.DefaultHeadingKey) : "";
                var heading = stack.HeadingOf(views[i]);
                Assert.AreEqual(expected, heading.text, block.kind + " #" + i);
                var slot = stack.SlotOf(views[i]);
                if (expected.Length > 0)
                {
                    headed++;
                    Assert.IsTrue(CardTestInput.IsShown(heading, slot), block.kind + " #" + i + ": the heading shows");
                    Assert.LessOrEqual(heading.worldBound.yMax, views[i].Root.worldBound.yMin + 0.5f, block.kind + " #" + i + ": above its block");
                }
                else Assert.AreEqual(DisplayStyle.None, heading.resolvedStyle.display, block.kind + " #" + i + ": no heading, no empty line");
                if (!stack.IsInScroll(slot)) continue;
                Assert.AreEqual(0f, views[i].Root.resolvedStyle.marginBottom, 0.01f, block.kind + " #" + i + ": no margin of its own");
                if (gap < 0f) gap = slot.resolvedStyle.marginBottom;
                Assert.AreEqual(gap, slot.resolvedStyle.marginBottom, onePixel + 0.01f, block.kind + " #" + i + ": the one gap");
            }
            Assert.AreEqual(24f, gap, onePixel + 0.01f, "the gap is --ts-block-gap (CardTokens.uss), snapped to a screen pixel");
            Assert.GreaterOrEqual(headed, 20, "most blocks carry a heading on the fixture (not vacuous)");
            Assert.AreEqual("How it stands today", stack.HeadingOf(views[1]).text, "the first authored heading, word for word");
            Assert.AreEqual("Side by side", stack.HeadingOf(views.OfType<ComparePointsBlockView>().Single()).text, "compare: no heading written, its default");
            yield return Capture("Card_Lamp_Full_Headings");

            SelectionEventBus.Clear();
            LiveSettings.languages = new System.Collections.Generic.List<string> { "pt", "en" };
            yield return OpenFull("lamp");
            Assert.AreEqual("Como está hoje", Sheet.Stack.HeadingOf(Sheet.Stack.BoundViews[1]).text, "the Portuguese heading");
            Assert.AreEqual("Lado a lado", Sheet.Stack.HeadingOf(Sheet.Stack.BoundViews.OfType<ComparePointsBlockView>().Single()).text,
                "the default heading in Portuguese");
        }

        [UnityTest]
        public IEnumerator TheLamp_GroupB_StepsSwatchesTimelinePeople_ShowTheAuthoredContent()
        {
            yield return OpenFull("lamp");
            var steps = Sheet.Stack.BoundViews.OfType<ProcessStepsBlockView>().Single();
            CollectionAssert.AreEqual(new[] { "Shape the clay", "First firing", "Glaze and paint", "Second firing" }, steps.Steps.Select(s => s.Title.text));
            CollectionAssert.AreEqual(new[] { "1", "2", "3", "4" }, steps.Steps.Select(s => s.Number.text));

            var swatches = Sheet.Stack.BoundViews.OfType<SwatchesBlockView>().Single();
            CollectionAssert.AreEqual(new[] { "#1F3F8F", "#F2EEE3", "#D9A93A", "#4A2F45" }, swatches.Swatches.Select(s => s.Code.text));
            ColorUtility.TryParseHtmlString("#1F3F8F", out var cobalt);
            Assert.AreEqual(cobalt.b, swatches.Swatches[0].Sample.resolvedStyle.backgroundColor.b, 1e-3f, "the sample is the config's colour");

            var timelines = Sheet.Stack.BoundViews.OfType<TimelineBlockView>().ToList();
            CollectionAssert.AreEqual(new[] { "11th c.", "1147", "c. 1300", "c. 1500", "1755", "1938 to 1940", "Now" }, timelines[0].Events.Select(e => e.Date.text), "vertical, with Highlight Now");
            Assert.IsFalse(timelines[1].Events.Any(e => e.IsNow), "horizontal, Highlight Now off");
            Assert.AreEqual(timelines[1].Root, timelines[1].Track.parent, "the horizontal one on its swipe track");

            var people = Sheet.Stack.BoundViews.OfType<PersonBlockView>().ToList();
            CollectionAssert.AreEqual(new[] { "Afonso Henriques", "King Manuel I" }, people.Select(p => p.Name.text));
            Assert.AreEqual("A", people[0].Monogram.text);

            Sheet.Stack.Scroll.ScrollTo(timelines[0].Root);
            yield return CardTestInput.Settle(0.2f);
            yield return Capture("Card_Lamp_Timeline");
            Sheet.Stack.Scroll.ScrollTo(swatches.Root);
            yield return CardTestInput.Settle(0.2f);
            yield return Capture("Card_Lamp_StepsSwatches");
        }

        [UnityTest]
        public IEnumerator LampMilitary_IsTheShortCard_HeaderRichTextQuickFactsAndGallery()
        {
            yield return OpenFull("lamp_military");
            CollectionAssert.AreEqual(
                new[] { BuiltInBlocks.HeaderKind, BuiltInBlocks.RichTextKind, BuiltInBlocks.QuickFactsKind, BuiltInBlocks.GalleryKind, BuiltInBlocks.ZoomImageKind },
                ShownKinds(),
                "the short card (_3.1 section 9): header, rich_text, quick_facts, gallery, and (step 13) a zoom_image on a Framework default picture");
            Assert.IsInstanceOf<AudioGuideBlockView>(Sheet.Stack.HeroActionViews.Single(), "(step 9A) its hero chip on the tone is the hero's Listen (_3.3 P4.4a)");
            Assert.IsTrue(Sheet.Stack.BoundViews.OfType<GalleryBlockView>().Single().Shots.All(s => s.Image.Texture != null), "its pictures load");
            var facts = Sheet.Stack.BoundViews.OfType<QuickFactsBlockView>().Single();
            CollectionAssert.AreEqual(new[] { "Last refuge", "3 m thick" }, facts.Facts.Select(f => f.Value.text));
            Assert.AreEqual("Castle Keep", Header.TitleText);
            var rich = Sheet.Stack.BoundViews.OfType<RichTextBlockView>().ToList();
            Assert.AreEqual(1, rich.Count);
            StringAssert.Contains("last refuge of the garrison", rich[0].Body.Paragraphs[0].text);
            Assert.AreEqual(ConfigCopy().pois.Single(p => p.id == "lamp_military").name, Session.SearchPois.First(p => p.id == "lamp_military").name, "the card never renames the POI");
        }

        [UnityTest]
        public IEnumerator TheCardsMediaSource_IsTheWallsMediaFolder_AndAClosedCardHoldsNothing()
        {
            yield return Select("lamp");
            Assert.IsNotNull(Card.Media, "the card has a media source once it opened");
            Assert.AreEqual("LivingRoom/CardMedia", Card.Media.Root, "Media Folder of the shipped wall");
            Assert.IsNotNull(Card.Media.Load<Texture2D>("placeholder_hero.png"), "the wall's fixture media is reachable from the running card");
            Card.Media.Release("placeholder_hero.png");
            Press(Sheet.CloseButton);
            yield return null;
            Assert.AreEqual(0, Card.Media.LoadedCount, "a closed card holds no media");
        }

        [Category("Core")]
        [UnityTest]
        public IEnumerator ARealMarkerTap_OpensTheCard_ForThatPoi()
        {
            var target = Session.SpawnedMarkers.Where(m => m != null && m.IsVisible).Select(m => (m, p: ScreenPointOf(m)))
                .FirstOrDefault(x => x.p.HasValue && x.p.Value.y > Screen.height * 0.4f && x.p.Value.y < Screen.height * 0.72f);
            Assert.IsNotNull(target.m, "precondition: a marker on screen");
            TapScreen(target.p.Value);
            yield return CardTestInput.Settle();
            Assert.IsTrue(Sheet.IsOpen);
            Assert.AreEqual(target.m.PoiId, Card.ShownPoiId);
        }

        // Scroll the stack so `view`'s block sits at the top of the scroll area, and wait for the layout
        private IEnumerator ScrollTo(IBlockView view)
        {
            Sheet.Stack.Scroll.ScrollTo(Sheet.Stack.SlotOf(view));
            yield return CardTestInput.Settle(0.2f);
        }

        // What the card told the events sink (a real ICardEvents implementation that keeps the events)
        private sealed class RecordingCardEvents : ICardEvents
        {
            public readonly System.Collections.Generic.List<CardEvent> Raised = new();
            public void Raise(CardEvent cardEvent) => Raised.Add(cardEvent);
        }

        private MemoryCardStateStore _cardStore;
        private RecordingCardEvents _cardEvents;

        // The card of these tests remembers in memory, so a run never touches the developer's own saved answers (one test below
        // reads the host's own state: the device store, which the run isolates too)
        [UnitySetUp]
        public IEnumerator UseAnInMemoryCardState()
        {
            _cardStore = new MemoryCardStateStore();
            Card.State = new CardLocalState(_cardStore, Session.SearchConfig.wall_id);
            _cardEvents = new RecordingCardEvents();
            Card.Events = _cardEvents;
            yield break;
        }

        // Scroll `target` into view, then a real tap on its centre (a block taller than what shows: the visitor scrolls first)
        private IEnumerator ScrollAndTap(VisualElement target)
        {
            Sheet.Stack.Scroll.ScrollTo(target);
            yield return CardTestInput.Settle(0.2f);
            yield return CardTestInput.Tap(target.panel, target.worldBound.center);
            yield return null;
        }

        // Read the card the way a visitor does: a real wheel, a few notches per frame, until the end of it (what reveals a block
        // that waits for the reading)
        private IEnumerator ReadTheWholeCard()
        {
            for (int notch = 0; notch < 400 && !Sheet.Stack.ContentSeen; notch++) yield return CardTestInput.Wheel(Sheet.Stack.Scroll, 12f);
            Assert.IsTrue(Sheet.Stack.ContentSeen, "the wheel reached the end of the card");
            yield return CardTestInput.Settle(0.15f);
        }

        // _3.1 step 8A: the card's own state, when nothing replaces it, is the DEVICE's store scoped by the wall's id -- PlayerPrefs in the
        // app (DeviceStateStore.App), this test's own store in a run, never the developer's: a real answer reaches it under the scoped key
        [UnityTest]
        public IEnumerator TheCardsOwnState_IsTheDeviceStoreScopedByTheWall_ARealAnswerReachesIt()
        {
            Assert.IsInstanceOf<PlayerPrefsCardStateStore>(DeviceStateStore.App, "the app keeps the visitor's state in PlayerPrefs");
            var device = DeviceStateStore.Current as MemoryCardStateStore;
            Assert.IsNotNull(device, "precondition: the run gave the device a store of its own, never the developer's PlayerPrefs");
            string wall = Session.SearchConfig.wall_id;
            string answerKey = new CardLocalState(device, wall).KeyOf("lamp", "block_43", "answer-0");
            Assert.IsFalse(device.TryGet(answerKey, out _), "precondition: nothing saved under the key");
            Card.State = null; // - the host builds its own on the next selection: the device's
            yield return OpenFull("lamp");
            var quiz = Quizzes()[0];
            Assert.IsFalse(quiz.Answered, "precondition: not answered");
            yield return ScrollAndTap(quiz.Choices[1].Button);
            Assert.AreEqual("Correct!", quiz.Verdict.text);
            Assert.IsTrue(device.TryGet(answerKey, out string stored), "the answer is in the device store under wall.poi.block.slot");
            Assert.AreEqual("1", stored);
            Assert.AreEqual(wall, Card.State.WallId, "the host's state is this wall's");

            SelectionEventBus.Clear();
            yield return OpenFull("lamp");
            Assert.AreEqual(1, Quizzes()[0].Index, "a new bind reads it back from the device store: opens on question 2");
        }

        private T Only<T>() where T : class, IBlockView => Sheet.Stack.BoundViews.OfType<T>().Single();

        private string StoredKey(string poiId, string block, string slot) => "ts.card." + Session.SearchConfig.wall_id + "." + poiId + "." + block + "." + slot;
    }
}
