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
    public class PoiCardSceneTests : SearchSceneFixture
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
            Assert.AreEqual("Royal Government", Header.ChipText, "the category name only: a hierarchy level (Hub) is an authoring word");
            Assert.AreEqual("Military - c. 1700 on the panel", Header.SubtitleText);

            Rect card = Sheet.Root.worldBound;
            Assert.LessOrEqual(Header.PeekPart.worldBound.yMax, card.yMax + 1f, "peek shows the whole title and chip");
            Assert.GreaterOrEqual(Header.Root.Q<Label>("card-header-subtitle").worldBound.yMin, card.yMax - 1f, "...and nothing below them");
            Assert.AreEqual("The Lamp", Session.SearchPois.First(p => p.id == "lamp").name, "the card never renames the POI");
            yield return Capture("Card_Lamp_Peek");
        }

        [UnityTest]
        public IEnumerator DraggingUp_GoesToHalf_AtMost40Percent_ThenFull()
        {
            yield return Select("lamp");
            yield return CardTestInput.Drag(Sheet.Handle, -(Sheet.Stops.Half - Sheet.Stops.Peek), slowSheet: Sheet);
            yield return CardTestInput.Settle();
            Assert.AreEqual(SheetStopRule.Stop.Half, Sheet.Stop);
            Assert.LessOrEqual(Sheet.Root.resolvedStyle.height, Sheet.Layer.layout.height * 0.40f + 1f, "half never passes 40% of the screen");
            var subtitle = Header.Root.Q<Label>("card-header-subtitle");
            Assert.LessOrEqual(subtitle.worldBound.yMax, Sheet.Root.worldBound.yMax + 1f, "half shows the subtitle");
            yield return Capture("Card_Lamp_Half");

            yield return CardTestInput.Drag(Sheet.Handle, -(Sheet.Stops.Full - Sheet.Stops.Half), slowSheet: Sheet);
            yield return CardTestInput.Settle();
            Assert.AreEqual(SheetStopRule.Stop.Full, Sheet.Stop);
            Assert.AreEqual(Sheet.Layer.layout.height - Sheet.Stops.Full, 48f, 1f, "full leaves the token's top gap (--ts-sheet-top-gap)");
            Assert.AreEqual("lamp", SelectionEventBus.CurrentPoiId, "dragging never changes the selection");
            yield return Capture("Card_Lamp_Full");
        }

        // _3.1 step 6C: the full stop would cover the search bar half-way, so the search top (bar, filter tray, view
        // switch) steps aside while the card rests at full, and comes back at any lower stop or when the card closes
        [UnityTest]
        public IEnumerator AtFull_TheSearchBarStepsAside_BelowFullTheCardNeverOverlapsIt_AndItComesBackOnClose()
        {
            var top = Host.Root.Q("search-top");
            Host.SetQuery("lamp");
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
            Assert.AreEqual("lamp", Host.Query, "the query is kept while the bar is away");
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
            Assert.AreEqual("lamp", Host.Root.Q<TextField>("search-field").value, "with the visitor's query still in it");
        }

        private void SetStopFull() => Sheet.SetStop(SheetStopRule.Stop.Full);

        // _3.1 step 6C: the pinned header (title, chip, subtitle) ate the scroll area at full. A real wheel scroll down the
        // stack collapses it to its compact look -- the same header, one class on its slot -- and scrolling back to the top
        // opens it again
        [UnityTest]
        public IEnumerator ScrollingTheStack_CollapsesTheHeaderToItsCompactLook_AndTheTopOpensItAgain()
        {
            yield return OpenFull("lamp");
            var stack = Sheet.Stack;
            float openHeader = stack.HeaderSlot.worldBound.height, openViewport = stack.Scroll.contentViewport.worldBound.height;
            var headerView = Header;
            Assert.IsTrue(headerView.SubtitleShown && !stack.HeaderCollapsed, "precondition: open, the subtitle shown");
            float openTitleSize = headerView.Root.Q<Label>("card-header-title").resolvedStyle.fontSize;
            var openStops = Sheet.Stops;
            Assert.IsTrue(CardTestInput.IsShown(headerView.Root.Q<Label>("card-header-chip"), stack.HeaderSlot), "precondition: the open header shows its chip");

            yield return CardTestInput.Wheel(stack.Scroll.contentViewport, 12f, frames: 4);
            yield return CardTestInput.Settle(0.2f);
            Assert.Greater(stack.Scroll.scrollOffset.y, HeaderCollapseRule.CollapseAfter, "the real wheel scrolled the stack");
            Assert.IsTrue(stack.HeaderCollapsed, "scrolled: collapsed");
            Assert.IsTrue(stack.HeaderSlot.ClassListContains(BlockStackView.CollapsedHeaderClass), "a class swap on the slot");
            Assert.AreSame(headerView, Header, "the same header view, no second header");
            Assert.IsFalse(headerView.SubtitleShown, "the subtitle steps out");
            Assert.AreEqual("St George's Castle", headerView.TitleText, "the title stays");
            // _3.1 [7B]: collapsed = ONE line -- a smaller title, no chip, the whole header under the token
            var title = headerView.Root.Q<Label>("card-header-title");
            Assert.IsTrue(CardTestInput.IsShown(title, stack.HeaderSlot), "the title is the one line left");
            Assert.IsFalse(CardTestInput.IsShown(headerView.Root.Q<Label>("card-header-chip"), stack.HeaderSlot), "the chip steps out too");
            Assert.Less(title.resolvedStyle.fontSize, openTitleSize, "a smaller title");
            Assert.AreEqual(WhiteSpace.NoWrap, title.resolvedStyle.whiteSpace, "one line: a long title is cut, never wrapped");
            float ceiling = CardTestInput.TokenPx("--ts-header-collapsed-max-height");
            Assert.AreEqual(ceiling, stack.CollapsedCeiling, 0.01f, "the stack reads the same token (its no-loop estimate is measured against it)");
            Assert.LessOrEqual(stack.HeaderSlot.worldBound.height, ceiling,
                "the collapsed header (" + stack.HeaderSlot.worldBound.height + ") is at most --ts-header-collapsed-max-height");
            Assert.Less(stack.HeaderSlot.worldBound.height, openHeader - 10f, "the header is shorter");
            Assert.Greater(stack.Scroll.contentViewport.worldBound.height, openViewport + 10f, "...and the scroll area got that room");
            Assert.AreEqual(openStops.Peek, Sheet.Stops.Peek, 0.5f, "collapsing never moves the peek stop (it is the open header's)");
            yield return Capture("Card_Lamp_Full_HeaderCollapsed");

            yield return CardTestInput.Wheel(stack.Scroll.contentViewport, -40f, frames: 4);
            yield return CardTestInput.Settle(0.2f);
            Assert.AreEqual(0f, stack.Scroll.scrollOffset.y, 0.01f, "wheeled back to the top");
            Assert.IsFalse(stack.HeaderCollapsed, "at the top: open again");
            Assert.IsTrue(headerView.SubtitleShown);
            Assert.IsTrue(CardTestInput.IsShown(headerView.Root.Q<Label>("card-header-chip"), stack.HeaderSlot), "the chip is back");
            Assert.AreEqual(openTitleSize, headerView.Root.Q<Label>("card-header-title").resolvedStyle.fontSize, 0.01f, "the full-size title is back");
            Assert.AreEqual(openHeader, stack.HeaderSlot.worldBound.height, 1f, "the header is its full size again");
            yield return Capture("Card_Lamp_Full_HeaderOpenAgain");

            // - a new card always starts open, even after a scrolled one
            yield return CardTestInput.Wheel(stack.Scroll.contentViewport, 12f, frames: 4);
            yield return CardTestInput.Settle(0.2f);
            Assert.IsTrue(stack.HeaderCollapsed, "precondition: collapsed again");
            SelectionEventBus.Select("lamp_military");
            yield return CardTestInput.Settle();
            Assert.IsFalse(stack.HeaderCollapsed, "another POI's card opens with its header open");
        }

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

        [UnityTest]
        public IEnumerator PullingTheCardDownBelowPeek_ClosesIt()
        {
            yield return Select("lamp");
            yield return CardTestInput.Drag(Sheet.Handle, Sheet.Stops.Peek * 0.8f);
            yield return null;
            Assert.IsNull(SelectionEventBus.CurrentPoiId, "a swipe down past peek clears the selection");
            Assert.IsFalse(Sheet.IsOpen);
        }

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
            Assert.IsFalse(Header.SubtitleShown, "the compact variant has no subtitle");
            Assert.AreEqual("Military", Header.ChipText, "the category only");
            Assert.AreEqual(built, Sheet.Stack.CreatedViewCount, "the header view was reused, not rebuilt");
        }

        [UnityTest]
        public IEnumerator APoiWithNoCard_ShowsItsNameAndSummary()
        {
            yield return Select("painting");
            Assert.AreEqual("The Painting", Header.TitleText);
            Assert.AreEqual("POI_Painting", Header.SubtitleText);
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
            yield return Select("lamp");
            Assert.AreEqual(SheetStopRule.Stop.Half, Sheet.Stop, "Open At = half");
            Assert.AreEqual(Sheet.Layer.layout.height * 0.25f, Sheet.Root.resolvedStyle.height, 1f, "Half Height Max = 25%");
        }

        [UnityTest]
        public IEnumerator TheFirstLanguage_IsTheOneShown()
        {
            LiveSettings.languages = new System.Collections.Generic.List<string> { "pt", "en" };
            yield return Select("lamp");
            Assert.AreEqual("Castelo de Sao Jorge", Header.TitleText);
            Assert.AreEqual("Militar - c. 1700 no painel", Header.SubtitleText);
        }

        [UnityTest]
        public IEnumerator TheCloseButton_IsNamedFromTheStringTable_InTheCardsLanguage_AndTheWallsCardTextsWin()
        {
            Assert.IsNotNull(Card.StringTable, "the scene's card has the framework string table");
            yield return Select("lamp");
            Assert.AreEqual("", Sheet.CloseButton.text, "no glyph in code: the X is drawn by the style sheet");
            Assert.AreEqual(2, Sheet.CloseButton.Query(className: "poi-card__close-bar").ToList().Count, "two crossed bars");
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
        public IEnumerator TheLamp_AtFull_ShowsItsRichTextBlocks_InTheAuthoredOrder_WithTheAuthoredText()
        {
            yield return OpenFull("lamp");
            var rich = Sheet.Stack.BoundViews.OfType<RichTextBlockView>().ToList();
            Assert.AreEqual(4, rich.Count, "one rich_text per look on the fixture");
            int first = ShownKinds().IndexOf(BuiltInBlocks.RichTextKind);
            Assert.Greater(first, 0, "after the header");

            StringAssert.StartsWith("<link=\"keep\">", rich[0].Body.Paragraphs[0].text, "plain: the linked word opens the text");
            StringAssert.Contains("crown the highest hill", rich[0].Body.Paragraphs[0].text);
            Assert.AreEqual(2, rich[0].Body.Paragraphs.Count);
            Assert.AreEqual("B", rich[1].Body.DropCap.text, "drop cap");
            StringAssert.Contains("uilt by the Moors", rich[1].Body.Paragraphs[0].text);
            Assert.IsTrue(rich[2].Body.Paragraphs[0].ClassListContains("card-text__lede"), "lede");
            CollectionAssert.AreEqual(new[] { "The siege of 1147", "The royal palace", "After the earthquake" }, rich[3].Sections.Select(s => s.Heading.text), "sections");

            Sheet.Stack.Scroll.ScrollTo(rich[3].Root);
            yield return CardTestInput.Settle(0.2f);
            yield return Capture("Card_Lamp_RichText");
        }

        [UnityTest]
        public IEnumerator InPortuguese_TheTextsAndTheGlossaryAreTheWallsPortuguese_AndARealTapOpensTheDefinition()
        {
            LiveSettings.languages = new System.Collections.Generic.List<string> { "pt", "en" };
            yield return OpenFull("lamp");
            var plain = Sheet.Stack.BoundViews.OfType<RichTextBlockView>().First();
            var first = plain.Body.Paragraphs[0];
            StringAssert.Contains("Torre de menagem", first.text, "the Portuguese words shown for the glossary entry");
            StringAssert.Contains("coroam a colina", first.text);

            Sheet.Stack.Scroll.ScrollTo(plain.Root);
            yield return CardTestInput.Settle(0.2f);
            var onWord = new Vector2(first.worldBound.xMin + 12f, first.worldBound.yMin + first.resolvedStyle.fontSize * 0.6f);
            yield return CardTestInput.Tap(first.panel, onWord);
            Assert.AreEqual("keep", plain.Body.OpenTerm, "a real tap on the linked word");
            Assert.AreEqual("Torre de menagem", plain.Body.DefinitionTitle.text);
            Assert.AreEqual("A torre mais forte de um castelo, o seu ultimo refugio.", plain.Body.DefinitionText.text, "the wall's glossary, in Portuguese");
            yield return Capture("Card_Lamp_Glossary_pt");
        }

        [UnityTest]
        public IEnumerator TheLamp_ShowsItsQuickFacts_InEachLook_WithTheAuthoredFacts()
        {
            yield return OpenFull("lamp");
            var facts = Sheet.Stack.BoundViews.OfType<QuickFactsBlockView>().ToList();
            Assert.AreEqual(3, facts.Count, "chips, grid_hairline, big_numbers");
            CollectionAssert.AreEqual(new[] { "Style", "Material", "Access" }, facts[0].Facts.Select(f => f.Label.text));
            CollectionAssert.AreEqual(new[] { "1147", "11", "1511", "c. 1700" }, facts[1].Facts.Select(f => f.Value.text));
            Assert.AreEqual("364", facts[2].Facts[1].Value.text);
            Assert.IsTrue(facts[2].Root.ClassListContains("card-facts--big_numbers"));

            Sheet.Stack.Scroll.ScrollTo(facts[1].Root);
            yield return CardTestInput.Settle(0.2f);
            yield return Capture("Card_Lamp_QuickFacts");
        }

        [UnityTest]
        public IEnumerator TheLamp_FunFacts_FlipOpensOnARealTap_ThePostcardShowsAtOnce_InTheCardsLanguage()
        {
            LiveSettings.languages = new System.Collections.Generic.List<string> { "pt", "en" };
            yield return OpenFull("lamp");
            var fun = Sheet.Stack.BoundViews.OfType<FunFactBlockView>().ToList();
            Assert.AreEqual(2, fun.Count, "flip and postcard");
            var flip = fun[0].Facts[0];
            Assert.AreEqual("Sabia que?", flip.Heading.text, "the framework's Portuguese heading");
            Assert.AreEqual("Toque para ver", flip.Hint.text);
            Assert.IsFalse(flip.Revealed);

            Sheet.Stack.Scroll.ScrollTo(fun[0].Root);
            yield return CardTestInput.Settle(0.2f);
            yield return CardTestInput.Tap(flip.Box.panel, flip.Box.worldBound.center);
            yield return null;
            Assert.IsTrue(flip.Revealed, "a real tap on the card reveals the fact");
            StringAssert.Contains("nao tem porta ao nivel do chao", flip.Text.Paragraphs[0].text);
            Assert.IsTrue(fun[1].Facts[0].Revealed, "the postcard shows at once");
            StringAssert.Contains("uma torre inteira", fun[1].Facts[0].Text.Paragraphs[0].text);
            yield return CardTestInput.Settle(0.1f);
            yield return Capture("Card_Lamp_FunFacts");
        }

        [UnityTest]
        public IEnumerator TheLamp_PullQuotes_ShowTheAuthoredQuote_AndOnlyTheAttributionThatWasWritten()
        {
            yield return OpenFull("lamp");
            var quotes = Sheet.Stack.BoundViews.OfType<PullQuoteBlockView>().ToList();
            Assert.AreEqual(2, quotes.Count, "serif and minimal");
            StringAssert.Contains("from the river long before the town", quotes[0].Quote.Paragraphs[0].text);
            StringAssert.Contains("<link=\"keep\">", quotes[0].Quote.Paragraphs[0].text, "a quote can link glossary words");
            Assert.AreEqual("Osbern, a crusader priest", quotes[0].Author.text);
            Assert.AreEqual("Letter on the conquest, 1147", quotes[0].Source.text);
            Assert.AreEqual("A traveller", quotes[1].Author.text);
            Assert.IsFalse(CardTestInput.IsShown(quotes[1].Source, quotes[1].Root), "no source written: none shown");

            Sheet.Stack.Scroll.ScrollTo(quotes[0].Root);
            yield return CardTestInput.Settle(0.2f);
            yield return Capture("Card_Lamp_PullQuotes");
        }

        // The running marker's own ring: its Image colour and picture (MarkerRingView draws it)
        private (Color Color, string Picture, bool Shown) MarkerRing(string poiId)
        {
            var ringView = Marker(poiId).GetComponentInChildren<MarkerRingView>(true);
            var image = (UnityEngine.UI.Image)typeof(MarkerRingView)
                .GetField("ringImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(ringView);
            return (image.color, image.sprite != null ? image.sprite.name : null, image.enabled);
        }

        [UnityTest]
        public IEnumerator TheStatusBlock_WearsExactlyItsMarkersRing_AndFollowsTheWallsOutlineRule()
        {
            yield return OpenFull("lamp");
            var status = Sheet.Stack.BoundViews.OfType<StatusBlockView>().ToList();
            Assert.AreEqual(2, status.Count, "ring and scale");
            var marker = MarkerRing("lamp");
            Assert.IsTrue(marker.Shown, "precondition: the lamp's marker draws its ring");
            ColorUtility.TryParseHtmlString("#E3BD72", out var gold);
            Assert.AreEqual(gold, marker.Color, "precondition: the shipped wall is uniform gold");
            Assert.AreEqual(marker.Color, status[0].Ring.resolvedStyle.unityBackgroundImageTintColor, "the card's ring is the marker's colour -- gold, not a green token");
            Assert.AreEqual(marker.Picture, CardTestInput.BackgroundPictureName(status[0].Ring), "and the marker's picture");
            Assert.AreEqual("Intact", status[0].Level.text);
            Sheet.Stack.Scroll.ScrollTo(status[1].Root);
            yield return CardTestInput.Settle(0.2f);
            yield return Capture("Card_Lamp_Status");

            // - the Editor's live push: outline mode per type, the lamp moved to Partial Damage
            var copy = ConfigCopy();
            copy.marker_outline_mode = "per_type";
            var lamp = copy.pois.First(p => p.id == "lamp");
            lamp.status_level_key = "partial_damage";
            lamp.status_pct = 20f;
            Session.ApplyMarkerSettings(copy);
            yield return null;
            SelectionEventBus.Clear();
            yield return OpenFull("lamp");
            status = Sheet.Stack.BoundViews.OfType<StatusBlockView>().ToList();
            var moved = MarkerRing("lamp");
            Assert.AreEqual("RingDashLong", moved.Picture, "precondition: the marker now draws Partial Damage's dash");
            Assert.AreNotEqual(gold, moved.Color, "precondition: per type is not the uniform gold");
            Assert.AreEqual(moved.Color, status[0].Ring.resolvedStyle.unityBackgroundImageTintColor, "the card follows the marker's new colour");
            Assert.AreEqual(moved.Picture, CardTestInput.BackgroundPictureName(status[0].Ring), "and its new dash");
            Assert.AreEqual("Partial Damage", status[0].Level.text);
            Assert.IsTrue(status[1].Steps.Single(s => s.Box.ClassListContains("card-status__step--current")).Key == "partial_damage", "the scale marks the new row");

            // - Has status switched off: no status block at all, never an empty ring
            lamp.has_status = false;
            Session.ApplyMarkerSettings(copy);
            yield return null;
            SelectionEventBus.Clear();
            yield return OpenFull("lamp");
            Assert.IsFalse(MarkerRing("lamp").Shown, "precondition: the marker has no ring either");
            Assert.AreEqual(0, Sheet.Stack.BoundViews.OfType<StatusBlockView>().Count(), "has_status off: the status blocks are not shown");
            Assert.Greater(Sheet.Stack.BoundViews.Count, 5, "the rest of the card still is");
        }

        [UnityTest]
        public IEnumerator TheLamp_Sources_ListAndVerified_InTheCardsLanguage()
        {
            LiveSettings.languages = new System.Collections.Generic.List<string> { "pt", "en" };
            yield return OpenFull("lamp");
            var sources = Sheet.Stack.BoundViews.OfType<SourcesBlockView>().ToList();
            Assert.AreEqual(2, sources.Count, "list and with_confidence");
            Assert.AreEqual("Fontes", Sheet.Stack.HeadingOf(sources[0]).text, "the framework's Portuguese default heading");
            Assert.AreEqual("Carta sobre a conquista de Lisboa", sources[0].Rows[0].Title.text);
            Assert.AreEqual("Museu Nacional do Azulejo", sources[0].Rows[1].Author.text);
            Assert.IsFalse(CardTestInput.IsShown(sources[0].Confidence, sources[0].Root), "list: no chip");
            Assert.AreEqual("Verificado", sources[1].Confidence.text, "with_confidence + verified: the Portuguese chip");
            Sheet.Stack.Scroll.ScrollTo(sources[1].Root);
            yield return CardTestInput.Settle(0.2f);
            yield return Capture("Card_Lamp_Sources");
        }

        [UnityTest]
        public IEnumerator TheLamp_IsTheFullCard_EveryKindAndVariantOfTiers1To3_InCatalogOrder()
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
                "hotspot_image", "hotspot_image", "wall_locator", "wall_locator", "today_map", "today_map", "related", "related",
                "knowledge_check", "knowledge_check", "knowledge_check", "poll", "collect", "feedback", "feedback", "dialogue", "show_on_wall", "show_on_wall",
                "sources", "sources", "actions", "actions", "actions",
            }, ShownKinds().Where(k => builtIn.TryGet(k, out _)).ToList(), "one block per kind and variant (Tiers 1 to 3), nothing skipped");
            var lampConfig = Session.SearchPois.First(p => p.id == "lamp");
            CollectionAssert.IsEmpty(BlockStackBuilder.Build(lampConfig, LiveSettings, BlockRegistry.Shared, Session.SearchPois).Skipped, "no authored block skipped");
            var variants = Sheet.Stack.BoundViews.Select(v => v.Root.GetClasses().FirstOrDefault(c => c.Contains("--"))).Where(c => c != null).ToList();
            foreach (string look in new[] { "card-timeline--vertical", "card-timeline--horizontal", "card-person--row", "card-person--card" })
                CollectionAssert.Contains(variants, look, "each variant on the card");
            var sticky = Sheet.Stack.BoundViews.OfType<ActionsBlockView>().Last();
            Assert.AreEqual(Sheet.Stack.Footer, Sheet.Stack.SlotOf(sticky).parent, "the sticky actions are pinned to the footer");
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
            Assert.AreEqual(lampConfig.card.blocks.Count, views.Count, "precondition: every block shown, in order");
            float gap = -1f;
            int headed = 0;
            // - layout snaps each edge to a physical pixel: one screen pixel in panel units is the honest tolerance
            float onePixel = RuntimePanelUtils.ScreenToPanel(stack.Scroll.panel, Vector2.right).x - RuntimePanelUtils.ScreenToPanel(stack.Scroll.panel, Vector2.zero).x;
            for (int i = 1; i < views.Count; i++)
            {
                var block = lampConfig.card.blocks[i];
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
                if (slot.parent != stack.Scroll.contentContainer) continue;
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
            Assert.AreEqual("Como esta hoje", Sheet.Stack.HeadingOf(Sheet.Stack.BoundViews[1]).text, "the Portuguese heading");
            Assert.AreEqual("Lado a lado", Sheet.Stack.HeadingOf(Sheet.Stack.BoundViews.OfType<ComparePointsBlockView>().Single()).text,
                "the default heading in Portuguese");
        }

        // _3.1 step 6C: the header chip names the category only; the header's Show Level adds the hierarchy level
        [UnityTest]
        public IEnumerator TheHeaderChip_IsTheCategoryAlone_AndShowLevelAddsTheLevel()
        {
            yield return Select("lamp");
            Assert.AreEqual("Royal Government", Header.ChipText, "default: no hierarchy level word for the visitor");
            SelectionEventBus.Clear();
            var header = Session.SearchPois.First(p => p.id == "lamp").card.blocks[0];
            header.fields.Add(new BlockFieldValue { key = BuiltInBlocks.HeaderShowLevelField, flag = true });
            yield return Select("lamp");
            Assert.AreEqual("Royal Government - Hub", Header.ChipText, "Show Level on: category - level");
        }

        // ---------------- Tier 1 group B on the real scene ----------------

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
            CollectionAssert.AreEqual(new[] { "1147", "c. 1300", "1755", "1940", "Now" }, timelines[0].Events.Select(e => e.Date.text), "vertical, with Highlight Now");
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

        // _3.1 step 7B: The Lamp's two hotspot_image blocks on the real wall: the picture from the wall's media folder, each
        // spot on its point of the picture, a real tap opens its authored text (a second closes it), a loupe does the same
        [UnityTest]
        public IEnumerator TheLamp_HotspotImage_ARealTapOnASpotOpensItsAuthoredText_ALoupeToo_AndInPortuguese()
        {
            yield return OpenFull("lamp");
            var hotspots = Sheet.Stack.BoundViews.OfType<HotspotImageBlockView>().ToList();
            Assert.AreEqual(2, hotspots.Count, "numbered and loupes");
            var numbered = hotspots[0];
            Assert.AreEqual("castle_then", numbered.Image.Texture?.name, "the picture from the wall's own media folder (the historic panel, its own file: the header counts castle_hero alone)");
            Assert.AreEqual(4, numbered.Spots.Count, "the four authored spots");
            Assert.AreEqual("Explore the panel", Sheet.Stack.HeadingOf(numbered).text, "its authored heading");
            Sheet.Stack.Scroll.ScrollTo(numbered.Root);
            yield return CardTestInput.Settle(0.2f);
            Rect picture = numbered.Image.Root.worldBound;
            Assert.AreEqual(picture.x + 0.5f * picture.width, numbered.Spots[1].Pin.worldBound.center.x, 1f, "the keep's spot: half across the picture");
            Assert.AreEqual(picture.y + 0.3f * picture.height, numbered.Spots[1].Pin.worldBound.center.y, 1f, "...and 0.3 down it");

            yield return CardTestInput.Tap(numbered.Root.panel, numbered.Spots[1].Pin.worldBound.center);
            yield return CardTestInput.Settle(0.1f);
            Assert.AreEqual(1, numbered.OpenIndex, "a real tap on spot 2");
            Assert.AreEqual("The keep", numbered.DetailTitle.text);
            StringAssert.Contains("the last refuge of the castle", numbered.DetailText.Paragraphs[0].text);
            Assert.IsTrue(Sheet.IsOpen, "a tap on a spot never closes the card");
            yield return Capture("Card_Lamp_Hotspot_Numbered");
            yield return CardTestInput.Tap(numbered.Root.panel, numbered.Spots[1].Pin.worldBound.center);
            yield return CardTestInput.Settle(0.1f);
            Assert.AreEqual(-1, numbered.OpenIndex, "a second tap closes it");

            var loupes = hotspots[1];
            Sheet.Stack.Scroll.ScrollTo(loupes.Root);
            yield return CardTestInput.Settle(0.2f);
            Assert.AreEqual(4, loupes.Loupes.childCount, "a close-up per spot");
            yield return CardTestInput.Tap(loupes.Root.panel, loupes.Spots[3].Loupe.worldBound.center);
            yield return CardTestInput.Settle(0.1f);
            Assert.AreEqual("The curtain wall", loupes.DetailTitle.text, "a real tap on the fourth close-up");
            Assert.AreEqual(0, loupes.DetailText.Paragraphs.Count, "a spot with no text: its title alone");
            yield return Capture("Card_Lamp_Hotspot_Loupes");

            LiveSettings.languages = new System.Collections.Generic.List<string> { "pt", "en" };
            SelectionEventBus.Clear();
            yield return OpenFull("lamp");
            numbered = Sheet.Stack.BoundViews.OfType<HotspotImageBlockView>().First();
            Assert.AreEqual("Toque num ponto para saber mais", numbered.Hint.text, "the framework's Portuguese hint");
            Sheet.Stack.Scroll.ScrollTo(numbered.Root);
            yield return CardTestInput.Settle(0.2f);
            yield return CardTestInput.Tap(numbered.Root.panel, numbered.Spots[1].Pin.worldBound.center);
            yield return CardTestInput.Settle(0.1f);
            Assert.AreEqual("A torre de menagem", numbered.DetailTitle.text, "the wall's Portuguese title");
        }

        // _3.1 step 7B: The Lamp's wall_locator on the real wall: the strip lays out the RUNNING wall's POIs by the one
        // along-the-wall rule with the scene camera as the visitor; a real tap on a neighbour selects it through the
        // selection bus -- the same OnMarkerSelected a marker tap raises (zoom-on-select listens to it) -- and the card rebinds
        [UnityTest]
        public IEnumerator TheLamp_WallLocator_LaysOutTheRunningWall_WithTheCamera_AndARealTapOnANeighbourOpensItsCard()
        {
            yield return OpenFull("lamp");
            var locators = Sheet.Stack.BoundViews.OfType<WallLocatorBlockView>().ToList();
            Assert.AreEqual(2, locators.Count, "strip and neighbours");
            var strip = locators[0];
            var places = WallAxisRule.Places(Session.SearchPois);
            Assert.AreEqual(Session.SearchPois.Count - 1, strip.Dots.Count, "a dot per other running POI");
            Assert.AreEqual("On this wall", Sheet.Stack.HeadingOf(strip).text, "no heading written: the default");
            float min = places.Along.Min(), max = places.Along.Max();
            Assert.AreEqual((places.Along[places.IndexOf("lamp")] - min) / (max - min), strip.SelfShare, 1e-4f, "The Lamp at its place by the one rule");
            var viewer = Session.MarkerSpawnRoot.InverseTransformPoint(Camera.main.transform.position);
            Assert.AreEqual(Mathf.Clamp01((places.Axis.Along(viewer) - min) / (max - min)), strip.YouShare, 1e-4f, "the scene camera is the visitor");
            Assert.IsTrue(CardTestInput.IsShown(strip.You, strip.Root), "'You are here' shows in the app");
            Sheet.Stack.Scroll.ScrollTo(strip.Root);
            yield return CardTestInput.Settle(0.2f);
            yield return Capture("Card_Lamp_WallLocator_Strip");

            var neighbours = locators[1];
            Assert.AreEqual("Next to it on the wall", Sheet.Stack.HeadingOf(neighbours).text);
            Assert.AreEqual("lamp_military", neighbours.LeftPoiId, "its own satellite on the left along the room's wall");
            Assert.AreEqual("lamp_religious", neighbours.RightPoiId, "...and on the right");
            var military = Session.SearchPois.First(p => p.id == "lamp_military");
            Assert.AreEqual(BlockStackBuilder.CardTitleOf(military, "en", "en"), neighbours.LeftTitle.text, "named by its card title");
            Sheet.Stack.Scroll.ScrollTo(neighbours.Root);
            yield return CardTestInput.Settle(0.2f);

            var raised = new System.Collections.Generic.List<string>();
            System.Action<string> probe = raised.Add;
            SelectionEventBus.OnMarkerSelected += probe;
            try
            {
                yield return CardTestInput.Tap(neighbours.Root.panel, neighbours.Left.worldBound.center);
                yield return CardTestInput.Settle();
            }
            finally { SelectionEventBus.OnMarkerSelected -= probe; }
            CollectionAssert.AreEqual(new[] { "lamp_military" }, raised, "one OnMarkerSelected, the event a marker tap raises");
            Assert.AreEqual("lamp_military", SelectionEventBus.CurrentPoiId);
            Assert.AreEqual("lamp_military", Card.ShownPoiId, "the card rebound to the neighbour");
            Assert.IsTrue(Sheet.IsOpen);
            Assert.AreEqual(BlockStackBuilder.CardTitleOf(military, "en", "en"), Header.TitleText, "its own header");
        }

        // What the card would hand to the device (the IUrlOpener seam: nothing leaves Unity)
        private sealed class RecordingOpener : IUrlOpener
        {
            public readonly System.Collections.Generic.List<string> Opened = new();
            public void Open(string url) => Opened.Add(url);
        }

        // _3.1 step 7B: The Lamp's today_map on the real wall: the map from the wall's media folder, the castle's
        // coordinates, a real tap on Directions hands exactly the authored link to the device once, the bridge shows the
        // header's own picture; in Portuguese the card's words
        [UnityTest]
        public IEnumerator TheLamp_TodayMap_ARealTapOnDirectionsOpensTheAuthoredLink_TheBridgeShowsTheHeadersPicture()
        {
            yield return OpenFull("lamp");
            var opener = new RecordingOpener();
            Sheet.UrlOpener = opener;
            var maps = Sheet.Stack.BoundViews.OfType<TodayMapBlockView>().ToList();
            Assert.AreEqual(2, maps.Count, "static and bridge");
            var map = maps[0];
            Assert.AreEqual("castle_map", map.Map.Texture?.name, "the street map from the wall's media folder");
            Assert.AreEqual("38.71390, -9.13340", map.Coordinates.text, "the castle's coordinates");
            Assert.AreEqual(BackgroundSizeType.Contain, map.Map.Picture.resolvedStyle.backgroundSize.sizeType, "the map shows whole (a crop could hide the pin)");
            Assert.AreEqual("Where it is today", Sheet.Stack.HeadingOf(map).text);
            Sheet.Stack.Scroll.ScrollTo(map.Root);
            yield return CardTestInput.Settle(0.2f);
            yield return CardTestInput.Tap(map.Root.panel, map.Directions.worldBound.center);
            yield return null;
            var authored = Session.SearchPois.First(p => p.id == "lamp").card.blocks.First(b => b.kind == BuiltInBlocks.TodayMapKind)
                .fields.First(f => f.key == BuiltInBlocks.TodayMapUrlField).value;
            CollectionAssert.AreEqual(new[] { authored }, opener.Opened, "a real tap handed the authored Maps Link to the device, once");
            Assert.IsTrue(Sheet.IsOpen, "the card stays as it was");
            yield return Capture("Card_Lamp_TodayMap_Static");

            var bridge = maps[1];
            Assert.AreEqual("castle_hero", bridge.ThenPicture.Texture?.name, "the wall's side: The Lamp's own header picture");
            Assert.AreEqual("castle_map", bridge.NowMap.Texture?.name, "today's side: the map");
            Assert.AreEqual(Header.TitleText, bridge.ThenTitle.text);
            Sheet.Stack.Scroll.ScrollTo(bridge.Root);
            yield return CardTestInput.Settle(0.2f);
            yield return Capture("Card_Lamp_TodayMap_Bridge");

            LiveSettings.languages = new System.Collections.Generic.List<string> { "pt", "en" };
            SelectionEventBus.Clear();
            yield return OpenFull("lamp");
            bridge = Sheet.Stack.BoundViews.OfType<TodayMapBlockView>().Last();
            Assert.AreEqual("Como chegar", bridge.Directions.text);
            Assert.AreEqual("Na parede", bridge.ThenLabel.text);
            Assert.AreEqual("Hoje", bridge.NowLabel.text);
            Assert.AreEqual("Do painel a hoje", Sheet.Stack.HeadingOf(bridge).text, "the wall's Portuguese heading");
        }

        // The ids one real tap makes the selection bus announce (the event a marker tap raises), tapping `target` on its panel
        private IEnumerator TapAndCollectSelections(VisualElement target, System.Collections.Generic.List<string> raised)
        {
            System.Action<string> probe = raised.Add;
            SelectionEventBus.OnMarkerSelected += probe;
            try
            {
                yield return CardTestInput.Tap(target.panel, target.worldBound.center);
                yield return CardTestInput.Settle();
            }
            finally { SelectionEventBus.OnMarkerSelected -= probe; }
        }

        // _3.1 step 7C: The Lamp's related carousel on the real wall: the written Points in the written order (Lamp - Military
        // first), each named by its card title; a real tap on a card selects that POI through the selection bus -- exactly one
        // OnMarkerSelected, as a marker tap raises -- and the card rebinds to it
        [UnityTest]
        public IEnumerator TheLamp_RelatedCarousel_ARealTapOnACardSelectsThatPoiThroughTheBus_AndTheCardRebinds()
        {
            yield return OpenFull("lamp");
            var blocks = Sheet.Stack.BoundViews.OfType<RelatedBlockView>().ToList();
            Assert.AreEqual(2, blocks.Count, "carousel and next_along_wall");
            var carousel = blocks[0];
            Assert.AreEqual("Also worth seeing", Sheet.Stack.HeadingOf(carousel).text, "the heading written for it");
            CollectionAssert.AreEqual(new[] { "lamp_military", "lamp_religious", "lamp_residential" }, carousel.Cards.Select(c => c.PoiId).ToList(),
                "the Points, in the order written");
            var military = Session.SearchPois.First(p => p.id == "lamp_military");
            Assert.AreEqual("Lamp - Military", military.name, "the fixture's own name is untouched");
            Assert.AreEqual(BlockStackBuilder.CardTitleOf(military, "en", "en"), carousel.Cards[0].Title.text, "named by its card title");
            foreach (var card in carousel.Cards)
                Assert.IsTrue(CardTestInput.IsShown(card.Box, carousel.Root), "every picked card can be seen in the strip");
            yield return ScrollTo(carousel);
            yield return Capture("Card_Lamp_Related_Carousel");

            var raised = new System.Collections.Generic.List<string>();
            yield return TapAndCollectSelections(carousel.Cards[0].Box, raised);
            CollectionAssert.AreEqual(new[] { "lamp_military" }, raised, "one OnMarkerSelected");
            Assert.AreEqual("lamp_military", SelectionEventBus.CurrentPoiId);
            Assert.AreEqual("lamp_military", Card.ShownPoiId, "the card rebound to the tapped POI");
            Assert.IsTrue(Sheet.IsOpen);
            Assert.AreEqual(BlockStackBuilder.CardTitleOf(military, "en", "en"), Header.TitleText, "its own header");
            CollectionAssert.IsEmpty(Sheet.Stack.BoundViews.OfType<RelatedBlockView>().ToList(), "Lamp - Military's short card has no related block");
        }

        // _3.1 step 7C: The Lamp's next_along_wall on the running wall goes where the wall's axis says: the picked (nearest) point
        // with the smallest place to the RIGHT of The Lamp -- worked out here from WallAxisRule.Places alone, not from the rule
        // under test -- and a real tap on the button selects it through the bus
        [UnityTest]
        public IEnumerator TheLamp_NextAlongWall_GoesToTheNeighbourTheWallAxisSays_ARealTapSelectsIt()
        {
            yield return OpenFull("lamp");
            var next = Sheet.Stack.BoundViews.OfType<RelatedBlockView>().Last();
            Assert.AreEqual("Related", Sheet.Stack.HeadingOf(next).text, "no heading written: the default");
            var wall = Session.SearchPois;
            var lamp = wall.First(p => p.id == "lamp");
            var places = WallAxisRule.Places(wall);
            var block = lamp.card.blocks.Last(b => b.kind == BuiltInBlocks.RelatedKind);
            var picked = RelatedPoisRule.Of(lamp, block, wall);
            Assert.AreEqual(RelatedPoisRule.MaxAutomatic, picked.Count, "nearest: the wall's six nearest other points");
            float self = places.Along[places.IndexOf("lamp")];
            float expected = picked.Select(p => places.Along[places.IndexOf(p.id)]).Where(a => a > self).Min();
            Assert.AreEqual(expected, places.Along[places.IndexOf(next.NextPoiId)], 1e-5f, "the nearest picked point to the right along the wall");
            Assert.Greater(places.Along[places.IndexOf(next.NextPoiId)], self, "to the right, not the left");
            Assert.AreEqual(BlockStackBuilder.CardTitleOf(wall.First(p => p.id == next.NextPoiId), "en", "en"), next.NextTitle.text);
            Assert.IsFalse(next.NextChevron.ClassListContains("card-related__chevron--wrap"), "not a wrap");
            yield return ScrollTo(next);
            yield return Capture("Card_Lamp_Related_NextAlongWall");

            string target = next.NextPoiId;
            var raised = new System.Collections.Generic.List<string>();
            yield return TapAndCollectSelections(next.Next, raised);
            CollectionAssert.AreEqual(new[] { target }, raised, "one OnMarkerSelected");
            Assert.AreEqual(target, Card.ShownPoiId, "the card rebound to the neighbour");
        }

        // _3.1 step 7C: at the wall's right end there is nothing to the right: next_along_wall wraps to the nearest point on the
        // left. Run on the real wall by giving its rightmost POI such a block in memory (the running session only)
        [UnityTest]
        public IEnumerator NextAlongWall_AtTheWallsRightEnd_WrapsLeftOnTheRunningWall_ARealTapSelectsThatPoint()
        {
            var wall = Session.SearchPois;
            var places = WallAxisRule.Places(wall);
            int last = places.Along.IndexOf(places.Along.Max());
            var rightmost = places.Pois[last];
            var block = new BlockInstanceData { key = "block_wrap", kind = BuiltInBlocks.RelatedKind, variant = BuiltInBlocks.RelatedNextAlongWall };
            block.fields.Add(new BlockFieldValue { key = BuiltInBlocks.RelatedSourceField, value = RelatedPoisRule.SourceNearest });
            rightmost.card = new POICardData { blocks = new System.Collections.Generic.List<BlockInstanceData> { block } };
            yield return OpenFull(rightmost.id);
            var next = Sheet.Stack.BoundViews.OfType<RelatedBlockView>().Single();
            var picked = RelatedPoisRule.Of(rightmost, block, wall);
            float expected = picked.Select(p => places.Along[places.IndexOf(p.id)]).Where(a => a < places.Along[last]).Max();
            Assert.AreEqual(expected, places.Along[places.IndexOf(next.NextPoiId)], 1e-5f, "the nearest picked point on the LEFT");
            Assert.IsTrue(next.NextChevron.ClassListContains("card-related__chevron--wrap"), "shown as a wrap");
            string target = next.NextPoiId;
            yield return ScrollTo(next);
            var raised = new System.Collections.Generic.List<string>();
            yield return TapAndCollectSelections(next.Next, raised);
            CollectionAssert.AreEqual(new[] { target }, raised);
            Assert.AreEqual(target, Card.ShownPoiId);
        }

        [UnityTest]
        public IEnumerator InPortuguese_TheStoryTurnsOnARealTap_WithThePortugueseCounterAndButtons()
        {
            LiveSettings.languages = new System.Collections.Generic.List<string> { "pt", "en" };
            yield return OpenFull("lamp");
            var story = Sheet.Stack.BoundViews.OfType<StoryChaptersBlockView>().Single();
            Assert.AreEqual("Capitulo 1 de 3", story.Counter.text, "the framework's Portuguese counter");
            Assert.AreEqual("O cerco", story.Title.text);
            Assert.AreEqual("Seguinte", story.Next.Q<Label>().text);
            Sheet.Stack.Scroll.ScrollTo(story.Root);
            yield return CardTestInput.Settle(0.2f);

            yield return CardTestInput.Tap(story.Next.panel, story.Next.worldBound.center);
            yield return null;
            Assert.AreEqual("Capitulo 2 de 3", story.Counter.text, "a real tap on Seguinte");
            Assert.AreEqual("O palacio", story.Title.text);
            Assert.AreEqual("Anterior", story.Previous.Q<Label>().text);
            yield return Capture("Card_Lamp_Story_pt");

            var person = Sheet.Stack.BoundViews.OfType<PersonBlockView>().Last();
            Assert.AreEqual("D. Manuel I", person.Name.text, "person texts in Portuguese too");
            var info = Sheet.Stack.BoundViews.OfType<PracticalInfoBlockView>().Single();
            Assert.AreEqual("Aberto", info.Rows[0].Label.text);
        }

        [UnityTest]
        public IEnumerator TheCompareBlock_WearsTheLampsAndLampMilitarysOwnMarkerRings()
        {
            yield return OpenFull("lamp");
            var compare = Sheet.Stack.BoundViews.OfType<ComparePointsBlockView>().Single();
            Assert.AreEqual("lamp", compare.Sides[0].PoiId);
            Assert.AreEqual("lamp_military", compare.Sides[1].PoiId);
            for (int i = 0; i < 2; i++)
            {
                var marker = MarkerRing(compare.Sides[i].PoiId);
                Assert.IsTrue(marker.Shown, "precondition: the marker draws its ring");
                Assert.AreEqual(marker.Color, compare.Sides[i].Ring.resolvedStyle.unityBackgroundImageTintColor, compare.Sides[i].PoiId + ": the running marker's ring colour");
                Assert.AreEqual(marker.Picture, CardTestInput.BackgroundPictureName(compare.Sides[i].Ring), compare.Sides[i].PoiId + ": and its picture");
            }
            CollectionAssert.AreEqual(new[] { "St George's Castle", "Castle Keep" }, compare.Sides.Select(s => s.Title.text), "each named by its card's title");
            CollectionAssert.AreEqual(new[] { "Intact", "Partial Damage" }, compare.Sides.Select(s => s.Level.text));
            Sheet.Stack.Scroll.ScrollTo(compare.Root);
            yield return CardTestInput.Settle(0.2f);
            yield return Capture("Card_Lamp_Compare");

            // - the other point loses its status (a live marker push): no half pair on the card
            var copy = ConfigCopy();
            copy.pois.First(p => p.id == "lamp_military").has_status = false;
            Session.ApplyMarkerSettings(copy);
            yield return null;
            SelectionEventBus.Clear();
            yield return OpenFull("lamp");
            Assert.AreEqual(0, Sheet.Stack.BoundViews.OfType<ComparePointsBlockView>().Count(), "Lamp - Military without a status: the compare block is not shown");
            Assert.AreEqual(1, Sheet.Stack.BoundViews.OfType<SwatchesBlockView>().Count(), "the rest of the card still is");
        }

        [UnityTest]
        public IEnumerator TheLamp_PracticalInfo_EveryRowWithItsIcon()
        {
            yield return OpenFull("lamp");
            var info = Sheet.Stack.BoundViews.OfType<PracticalInfoBlockView>().Single();
            CollectionAssert.AreEqual(new[] { "time", "ticket", "access", "location", "light", "info" }, info.Rows.Select(r => CardIcons.KeyOf(r.Icon)));
            CollectionAssert.AreEqual(new[] { "Open", "Tickets", "Getting in", "Where", "Best light", "Good to know" }, info.Rows.Select(r => r.Label.text));
            Sheet.Stack.Scroll.ScrollTo(info.Root);
            yield return CardTestInput.Settle(0.2f);
            yield return Capture("Card_Lamp_PracticalInfo");
        }

        [UnityTest]
        public IEnumerator ARealTapOnTheStickyAction_ShowsThePointOnTheWall_TheCardLowersAndTheSelectionStays()
        {
            yield return OpenFull("lamp");
            var sticky = Sheet.Stack.BoundViews.OfType<ActionsBlockView>().Last();
            var button = sticky.Actions.Single().Button;
            Assert.AreEqual("Show me where it is", sticky.Actions[0].Label.text);
            Assert.LessOrEqual(button.worldBound.yMax, Sheet.Root.worldBound.yMax + 1f, "precondition: in reach at the bottom of the full card");
            yield return Capture("Card_Lamp_Actions");

            yield return CardTestInput.Tap(button.panel, button.worldBound.center);
            yield return CardTestInput.Settle();
            Assert.AreEqual(SheetStopRule.Stop.Peek, Sheet.Stop, "the card lowers to its title");
            Assert.AreEqual("lamp", SelectionEventBus.CurrentPoiId, "the point stays selected: its marker stays highlighted on the wall");
            Assert.IsTrue(Marker("lamp").IsVisible);
            yield return Capture("Card_Lamp_ShowOnWall");
        }

        [UnityTest]
        public IEnumerator LampMilitary_IsTheShortCard_HeaderRichTextQuickFactsAndGallery()
        {
            yield return OpenFull("lamp_military");
            CollectionAssert.AreEqual(new[] { BuiltInBlocks.HeaderKind, BuiltInBlocks.RichTextKind, BuiltInBlocks.QuickFactsKind, BuiltInBlocks.GalleryKind }, ShownKinds(),
                "the short card (_3.1 section 9): header, rich_text, quick_facts, gallery");
            Assert.IsTrue(Sheet.Stack.BoundViews.OfType<GalleryBlockView>().Single().Shots.All(s => s.Image.Texture != null), "its pictures load");
            var facts = Sheet.Stack.BoundViews.OfType<QuickFactsBlockView>().Single();
            CollectionAssert.AreEqual(new[] { "Last refuge", "3 m thick" }, facts.Facts.Select(f => f.Value.text));
            Assert.AreEqual("Castle Keep", Header.TitleText);
            var rich = Sheet.Stack.BoundViews.OfType<RichTextBlockView>().ToList();
            Assert.AreEqual(1, rich.Count);
            StringAssert.Contains("last refuge of the garrison", rich[0].Body.Paragraphs[0].text);
            Assert.AreEqual("Lamp - Military", Session.SearchPois.First(p => p.id == "lamp_military").name, "the card never renames the POI");
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

        // ---------------- Tier 2 group A on the real scene (_3.1 step 7) ----------------

        // Scroll the stack so `view`'s block sits at the top of the scroll area, and wait for the layout
        private IEnumerator ScrollTo(IBlockView view)
        {
            Sheet.Stack.Scroll.ScrollTo(Sheet.Stack.SlotOf(view));
            yield return CardTestInput.Settle(0.2f);
        }

        [UnityTest]
        public IEnumerator TheLamp_HeaderPicture_LoadsTheWallsFixture_AndARealWheelDrivesItsParallax()
        {
            yield return OpenFull("lamp");
            Assert.IsTrue(Header.HasHero, "The Lamp's header is the Image Parallax look");
            Assert.IsNotNull(Header.Picture.Texture, "castle_hero.png loaded from the wall's Media Folder (Resources)");
            Assert.AreEqual("castle_hero", Header.Picture.Texture.name);
            // - The Lamp's today_map bridge shows the header's picture as the wall's side of the bridge (its own copy)
            int bridges = Sheet.Stack.BoundViews.OfType<TodayMapBlockView>().Count(v => v.ThenPicture.Path == "castle_hero.png");
            Assert.AreEqual(1, bridges, "precondition: the fixture's bridge holds the header picture too");
            Assert.AreEqual(1 + bridges, Card.Media.RefCount("castle_hero.png"), "loaded once by the header, lazily when the card was bound (+ the bridge's copy)");
            Assert.AreEqual(0, Sheet.Stack.Scroll.contentContainer.IndexOf(Header.HeroPart), "the hero opens what scrolls");
            yield return Capture("Card_Lamp_Full_Hero");

            float frameTop = Header.HeroPart.worldBound.yMin, pictureTop = Header.Picture.Root.worldBound.yMin;
            yield return CardTestInput.Wheel(Sheet.Stack.Scroll.contentViewport, 6f, frames: 2);
            yield return CardTestInput.Settle(0.2f);
            float scrolled = Sheet.Stack.Scroll.scrollOffset.y;
            Assert.Greater(scrolled, 20f, "the real wheel scrolled the real card");
            Assert.AreEqual(scrolled * HeaderBlockView.ParallaxFactor, Header.ParallaxOffset, 0.5f, "the picture lags at half the scroll");
            float frameMoved = frameTop - Header.HeroPart.worldBound.yMin, pictureMoved = pictureTop - Header.Picture.Root.worldBound.yMin;
            Assert.AreEqual(Header.ParallaxOffset, frameMoved - pictureMoved, 1f, "on screen the picture moved less than its frame, by exactly its slide");
            Assert.IsTrue(Sheet.Stack.HeaderCollapsed, "and the pinned header collapsed as for any scroll");

            SelectionEventBus.Clear();
            yield return null;
            Assert.AreEqual(0, Card.Media.LoadedCount, "the card closed: no picture is held any more (every scope released)");
        }

        [UnityTest]
        public IEnumerator TheLampsHeader_SwitchedLiveToSplitAndSpotlight_ShowsEachLook_WithTheWallsPictures()
        {
            var header = Session.SearchPois.First(p => p.id == "lamp").card.blocks[0];
            header.variant = BuiltInBlocks.HeaderSplitThenNow;
            header.fields.Find(f => f.key == BuiltInBlocks.HeaderImageField).asset = "castle_then.png";
            header.fields.Add(new BlockFieldValue { key = BuiltInBlocks.HeaderSecondImageField, asset = "castle_now.png" });
            yield return OpenFull("lamp");
            Assert.IsTrue(Header.HasHero);
            Assert.AreEqual("castle_then", Header.Picture.Texture.name);
            Assert.AreEqual("castle_now", Header.SecondPicture.Texture.name);
            Assert.AreEqual("Then", Header.ThenLabel.text);
            Assert.AreEqual("Now", Header.NowLabel.text);
            Assert.LessOrEqual(Header.Picture.Root.worldBound.xMax, Header.SecondPicture.Root.worldBound.xMin + 0.5f, "side by side");
            yield return Capture("Card_Lamp_Full_HeroSplit");

            SelectionEventBus.Clear();
            header.variant = BuiltInBlocks.HeaderSpotlightCrop;
            header.fields.Find(f => f.key == BuiltInBlocks.HeaderImageField).asset = "castle_hero.png";
            header.fields.Add(new BlockFieldValue { key = BuiltInBlocks.HeaderFocusXField, number = 0.55f });
            header.fields.Add(new BlockFieldValue { key = BuiltInBlocks.HeaderFocusYField, number = 0.35f });
            yield return OpenFull("lamp");
            Rect frame = Header.HeroPart.worldBound, picture = Header.Picture.Root.worldBound;
            Assert.Greater(picture.width, frame.width * 1.5f, "enlarged (Crop Zoom's default, 2)");
            Assert.AreEqual(picture.xMin + picture.width * 0.55f, Header.Ring.worldBound.center.x, 1f, "the ring on the focus point");
            Assert.AreEqual(picture.yMin + picture.height * 0.35f, Header.Ring.worldBound.center.y, 1f);
            Assert.IsTrue(frame.Contains(Header.Ring.worldBound.center));
            yield return Capture("Card_Lamp_Full_HeroSpotlight");

            SelectionEventBus.Clear();
            header.variant = BuiltInBlocks.HeaderTextOnly;
            yield return OpenFull("lamp");
            Assert.IsFalse(Header.HasHero, "the text-only look: no picture");
            Assert.AreEqual(0, Card.Media.RefCount("castle_hero.png"), "...and none loaded");
        }

        [UnityTest]
        public IEnumerator TheLamp_ARealTapOnAGalleryPicture_OpensItFullScreen_BackKeepsTheScroll_AndClosingGivesEverythingBack()
        {
            yield return OpenFull("lamp");
            var grid = Sheet.Stack.BoundViews.OfType<GalleryBlockView>().Single(g => g.Root.ClassListContains("card-gallery--grid"));
            yield return ScrollTo(grid);
            float scroll = Sheet.Stack.Scroll.scrollOffset.y;
            Assert.Greater(scroll, 500f, "precondition: the long card is scrolled far down to its grid");
            int before = Card.Media.RefCount("gallery_2.png");
            Assert.Greater(before, 0, "the gallery looks hold gallery_2.png");
            foreach (var g in Sheet.Stack.BoundViews.OfType<GalleryBlockView>())
                foreach (var shot in g.Shots) Assert.IsNotNull(shot.Image.Texture, "every gallery picture of the fixture loaded (" + shot.Image.Path + ")");

            var target = grid.Shots[1].Box;
            yield return CardTestInput.Tap(target.panel, target.worldBound.center);
            yield return CardTestInput.Settle(0.2f);
            var takeover = Sheet.Takeover;
            Assert.IsTrue(takeover.IsOpen, "a real tap on the second picture opens it full screen");
            Assert.AreEqual(1, takeover.PageIndex);
            Assert.AreEqual("St George's Castle > Gallery", takeover.Crumb.text, "the card's title > the gallery");
            Assert.AreEqual(before + 1, Card.Media.RefCount("gallery_2.png"), "the full-screen view loaded its own copy");
            Assert.IsTrue(ScreenUIHit.IsOverAnything(ScreenPointOf(takeover.Page)), "a tap on the full-screen view is on the UI: never 'empty space'");
            Assert.AreEqual("lamp", SelectionEventBus.CurrentPoiId, "the selection stays");
            yield return Capture("Card_Lamp_Lightbox");

            yield return CardTestInput.Tap(takeover.Chips[3].panel, takeover.Chips[3].worldBound.center);
            yield return CardTestInput.Settle(0.15f);
            Assert.AreEqual(3, takeover.PageIndex, "a real tap on chip 4");
            Assert.AreEqual(before, Card.Media.RefCount("gallery_2.png"), "the page before gave its picture back");

            yield return CardTestInput.Tap(takeover.Back.panel, takeover.Back.worldBound.center);
            yield return CardTestInput.Settle(0.2f);
            Assert.IsFalse(takeover.IsOpen, "Back");
            Assert.AreEqual(0, takeover.HeldMedia);
            Assert.AreEqual(scroll, Sheet.Stack.Scroll.scrollOffset.y, 0.5f, "the card is where the visitor left it");
            Assert.AreEqual(SheetStopRule.Stop.Full, Sheet.Stop);

            Press(Sheet.CloseButton);
            yield return null;
            Assert.AreEqual(0, Card.Media.LoadedCount, "closed: every picture of the card given back");
        }

        [UnityTest]
        public IEnumerator TheLamp_BeforeAfter_ARealDragOnTheRealCard_MovesTheCut_AndTheCardDoesNotScroll()
        {
            yield return OpenFull("lamp");
            var slider = Sheet.Stack.BoundViews.OfType<BeforeAfterBlockView>().Single();
            yield return ScrollTo(slider);
            Assert.AreEqual("Before 1755", slider.BeforeLabel.text);
            Assert.AreEqual("After the earthquake", slider.AfterLabel.text);
            Assert.AreEqual("damage_before", slider.Before.Texture.name);
            Assert.AreEqual("damage_after", slider.After.Texture.name);
            float scroll = Sheet.Stack.Scroll.scrollOffset.y;
            Rect frame = slider.Frame.worldBound;
            yield return CardTestInput.DragFrom(slider.Frame.panel, frame.center, new Vector2(-frame.width * 0.35f, 0f));
            yield return null;
            Assert.AreEqual(0.15f, slider.Position, 0.02f, "dragged from the middle to 15%");
            Assert.AreEqual(frame.width * slider.Position, slider.BeforeClip.worldBound.width, 1f);
            Assert.AreEqual(scroll, Sheet.Stack.Scroll.scrollOffset.y, 0.01f, "the card did not scroll under the drag");
            yield return Capture("Card_Lamp_BeforeAfter");
        }

        // ---------------- Tier 3 group A: knowledge_check and feedback on the real wall (_3.1 step 8A) ----------------

        // What the card told the events sink (a real ICardEvents implementation that keeps the events)
        private sealed class RecordingCardEvents : ICardEvents
        {
            public readonly System.Collections.Generic.List<CardEvent> Raised = new();
            public void Raise(CardEvent cardEvent) => Raised.Add(cardEvent);
        }

        private MemoryCardStateStore _cardStore;
        private RecordingCardEvents _cardEvents;

        // The card of these tests remembers in memory, so a run never touches the developer's own saved answers (one test below
        // uses the real PlayerPrefs store, cleans up after itself and says so)
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

        private System.Collections.Generic.List<KnowledgeCheckBlockView> Quizzes() => Sheet.Stack.BoundViews.OfType<KnowledgeCheckBlockView>().ToList();

        // _3.1 step 8A: The Lamp's multiple-choice question on the real card: a real tap on a wrong option says "Actually..." with
        // the explanation and marks the right one; the answer is remembered under wall + POI + block + row, so the card reopens
        // on the next question; in Portuguese the card's words and the authored Portuguese text
        [UnityTest]
        public IEnumerator TheLamp_KnowledgeCheck_MultipleChoice_ARealTapAnswers_ItIsRemembered_AndInPortugueseTheWordsFollow()
        {
            yield return OpenFull("lamp");
            var quiz = Quizzes()[0];
            Assert.AreEqual("Test yourself", Sheet.Stack.HeadingOf(quiz).text, "no heading written: the card's default");
            Assert.AreEqual(2, quiz.Count, "two questions authored");
            Assert.AreEqual("What is the tallest tower of a castle called?", quiz.Prompt.text);
            CollectionAssert.AreEqual(new[] { "The curtain wall", "The keep", "The gatehouse" }, quiz.Choices.Select(c => c.Text.text).ToList());
            Assert.AreEqual("Question 1 of 2", quiz.Counter.text);
            Assert.IsFalse(quiz.Answered);

            yield return ScrollAndTap(quiz.Choices[0].Button);
            Assert.AreEqual(0, quiz.Chosen);
            Assert.AreEqual("Actually...", quiz.Verdict.text);
            Assert.AreEqual("The keep is the strongest tower: the last refuge when the walls fell.", quiz.Explanation.text);
            Assert.IsTrue(quiz.Choices[1].Button.ClassListContains("card-quiz__choice--correct"), "the right option is marked");
            Assert.AreEqual(0, Card.State.Answer("lamp", "block_43", 0), "remembered under the wall, the POI, the block and the question's row");
            Assert.IsTrue(_cardStore.TryGet("ts.card." + Session.SearchConfig.wall_id + ".lamp.block_43.answer-0", out string stored), "the scoped key");
            Assert.AreEqual("0", stored);
            yield return ScrollTo(quiz);
            yield return Capture("Card_Lamp_KnowledgeCheck_MultipleChoice");

            // - closed and opened again: the first question is answered, so it opens on the second
            SelectionEventBus.Clear();
            yield return OpenFull("lamp");
            quiz = Quizzes()[0];
            Assert.AreEqual(1, quiz.Index, "question 1 is answered: it opens on question 2");
            Assert.AreEqual("Question 2 of 2", quiz.Counter.text);
            Assert.IsFalse(quiz.Answered);

            // - Portuguese: the authored Portuguese text and the card's Portuguese words; a real tap on the right option
            SelectionEventBus.Clear();
            LiveSettings.languages = new System.Collections.Generic.List<string> { "pt", "en" };
            yield return OpenFull("lamp");
            quiz = Quizzes()[0];
            Assert.AreEqual("Teste-se", Sheet.Stack.HeadingOf(quiz).text, "the default heading in Portuguese");
            Assert.AreEqual("Pergunta 2 de 2", quiz.Counter.text);
            Assert.AreEqual("Qual destes NAO existia no antigo palacio?", quiz.Prompt.text);
            yield return ScrollAndTap(quiz.Choices[2].Button);
            Assert.AreEqual("Certo!", quiz.Verdict.text);
            StringAssert.StartsWith("O palacio ficava junto ao rio", quiz.Explanation.text);
            Assert.AreEqual(2, Card.State.Answer("lamp", "block_43", 1));
        }

        // _3.1 step 8A: The Lamp's true / false question waits for the reading (show_after_viewed): hidden on a fresh card, a real
        // wheel to the end reveals it (and it is remembered), a real swipe to the right answers True on the real card
        [UnityTest]
        public IEnumerator TheLamp_KnowledgeCheck_TrueFalse_WaitsForTheReading_ARealWheelRevealsIt_ARealSwipeAnswers()
        {
            yield return OpenFull("lamp");
            var quiz = Quizzes()[1];
            var slot = Sheet.Stack.SlotOf(quiz);
            Assert.AreEqual(DisplayStyle.None, slot.resolvedStyle.display, "the card is unread: the question waits");
            Assert.IsFalse(Sheet.Stack.ContentSeen);
            Assert.IsFalse(Card.State.Seen("lamp", "block_44"));
            var others = Quizzes().Where(q => q != quiz).ToList();
            foreach (var other in others) Assert.AreNotEqual(DisplayStyle.None, Sheet.Stack.SlotOf(other).resolvedStyle.display, "only the block that asks to wait waits");

            yield return ReadTheWholeCard();
            Assert.AreNotEqual(DisplayStyle.None, slot.resolvedStyle.display, "the wheel got to the end: the question appeared");
            Assert.IsTrue(Card.State.Seen("lamp", "block_44"), "and that is remembered");

            yield return ScrollTo(quiz);
            Assert.AreEqual("A curtain wall joins the towers of a castle.", quiz.Prompt.text);
            Rect stage = quiz.Stage.worldBound;
            yield return CardTestInput.DragFrom(quiz.Root.panel, stage.center, new Vector2(stage.width * 0.5f, 4f));
            yield return null;
            Assert.AreEqual(KnowledgeCheckRule.ChoiceTrue, quiz.Chosen, "a real swipe to the right answered True");
            Assert.AreEqual("Correct!", quiz.Verdict.text);
            Assert.AreEqual(0, Card.State.Answer("lamp", "block_44", 0));
            yield return Capture("Card_Lamp_KnowledgeCheck_TrueFalse");

            // - a visitor who read it once meets the question at once the next time
            SelectionEventBus.Clear();
            yield return OpenFull("lamp");
            Assert.AreNotEqual(DisplayStyle.None, Sheet.Stack.SlotOf(Quizzes()[1]).resolvedStyle.display, "remembered: no waiting");
            Assert.IsFalse(Sheet.Stack.ContentSeen, "...although this time nothing has been read");
        }

        // _3.1 step 8A: The Lamp's picture question: the pictures come from the wall's media folder, a real tap on the right one
        [UnityTest]
        public IEnumerator TheLamp_KnowledgeCheck_ImageChoice_PicturesFromTheWallsFolder_ARealTapOnTheRightOneConfirms()
        {
            yield return OpenFull("lamp");
            var quiz = Quizzes()[2];
            Assert.AreEqual(2, quiz.Choices.Count);
            Assert.AreEqual("damage_before", quiz.Choices[0].Image.Texture?.name, "the wall's own picture");
            Assert.AreEqual("damage_after", quiz.Choices[1].Image.Texture?.name);
            CollectionAssert.AreEqual(new[] { "Before", "After" }, quiz.Choices.Select(c => c.Text.text).ToList());
            yield return ScrollAndTap(quiz.Choices[0].Button);
            Assert.AreEqual("Correct!", quiz.Verdict.text);
            Assert.AreEqual("The earlier picture still has every tile in place.", quiz.Explanation.text);
            Assert.AreEqual(0, Card.State.Answer("lamp", "block_45", 0));
            yield return ScrollTo(quiz);
            yield return Capture("Card_Lamp_KnowledgeCheck_ImageChoice");
        }

        // _3.1 step 8A: The Lamp's feedback blocks: a real tap on a thumb and on a star each raises ONE event through the card's
        // events seam (wall, POI, block, look, value) and is remembered; a Portuguese card asks and thanks in Portuguese
        [UnityTest]
        public IEnumerator TheLamp_Feedback_ARealTapOnAThumbAndOnAStar_EachRaisesOneEvent_IsRemembered_AndInPortugueseTheWordsFollow()
        {
            yield return OpenFull("lamp");
            var feedback = Sheet.Stack.BoundViews.OfType<FeedbackBlockView>().ToList();
            Assert.AreEqual(2, feedback.Count, "thumbs and stars");
            var thumbs = feedback[0];
            var stars = feedback[1];
            Assert.AreEqual("Was this description useful?", thumbs.Question.text, "the question written for it");
            Assert.AreEqual("How would you rate this?", stars.Question.text, "none written: the card's own");

            yield return ScrollAndTap(thumbs.Votes[0].Button);
            Assert.AreEqual(FeedbackRule.ThumbUp, thumbs.Voted);
            Assert.AreEqual(1, _cardEvents.Raised.Count, "one event for the thumb");
            var first = _cardEvents.Raised[0];
            Assert.AreEqual(CardEventKinds.Feedback, first.Kind);
            Assert.AreEqual(Session.SearchConfig.wall_id, first.WallId);
            Assert.AreEqual("lamp", first.PoiId);
            Assert.AreEqual("block_46", first.BlockKey);
            Assert.AreEqual("thumbs", first.Variant);
            Assert.AreEqual("up", first.Value);
            Assert.AreEqual(FeedbackRule.ThumbUp, Card.State.Vote("lamp", "block_46"));

            yield return ScrollAndTap(stars.Votes[4].Button);
            Assert.AreEqual(5, stars.Voted);
            Assert.AreEqual(2, _cardEvents.Raised.Count);
            Assert.AreEqual("5", _cardEvents.Raised[1].Value);
            Assert.AreEqual("block_47", _cardEvents.Raised[1].BlockKey);
            Assert.IsTrue(stars.Votes.All(v => v.Button.ClassListContains("card-feedback__vote--on")), "five stars filled");
            yield return ScrollTo(stars);
            yield return Capture("Card_Lamp_Feedback");

            // - closed and opened again: both votes shown as given, nothing reported again
            SelectionEventBus.Clear();
            yield return OpenFull("lamp");
            feedback = Sheet.Stack.BoundViews.OfType<FeedbackBlockView>().ToList();
            Assert.AreEqual(FeedbackRule.ThumbUp, feedback[0].Voted);
            Assert.AreEqual(5, feedback[1].Voted);
            Assert.AreEqual(2, _cardEvents.Raised.Count, "a remembered vote reports nothing");

            // - Portuguese, on a fresh device: the authored question, the card's default question and thank-you
            _cardStore.Keys.ToList().ForEach(k => _cardStore.Remove(k));
            SelectionEventBus.Clear();
            LiveSettings.languages = new System.Collections.Generic.List<string> { "pt", "en" };
            yield return OpenFull("lamp");
            feedback = Sheet.Stack.BoundViews.OfType<FeedbackBlockView>().ToList();
            Assert.AreEqual("Esta descricao foi util?", feedback[0].Question.text);
            Assert.AreEqual("Como avalia isto?", feedback[1].Question.text);
            CollectionAssert.AreEqual(new[] { "Util", "Pouco util" }, feedback[0].Votes.Select(v => v.Text.text).ToList());
            yield return ScrollAndTap(feedback[0].Votes[1].Button);
            Assert.AreEqual("Obrigado pelo seu comentario", feedback[0].Thanks.text);
            Assert.AreEqual("down", _cardEvents.Raised[2].Value);
        }

        // _3.1 step 8A: the card's own state, when nothing replaces it, is PlayerPrefs scoped by the wall's id: a real answer
        // reaches PlayerPrefs under the scoped key. It touches the developer's PlayerPrefs, so it puts back exactly what was there
        [UnityTest]
        public IEnumerator TheCardsOwnState_IsPlayerPrefsScopedByTheWall_ARealAnswerReachesIt()
        {
            string wall = Session.SearchConfig.wall_id;
            var real = new CardLocalState(new PlayerPrefsCardStateStore(), wall);
            string answerKey = real.KeyOf("lamp", "block_43", "answer-0");
            string indexKey = CardLocalState.KeyPrefix + wall + ".index";
            string answerBefore = PlayerPrefs.HasKey(answerKey) ? PlayerPrefs.GetString(answerKey) : null;
            string indexBefore = PlayerPrefs.HasKey(indexKey) ? PlayerPrefs.GetString(indexKey) : null;
            try
            {
                PlayerPrefs.DeleteKey(answerKey);
                Card.State = null; // - the host builds its own on the next selection: the app's
                yield return OpenFull("lamp");
                var quiz = Quizzes()[0];
                Assert.IsFalse(quiz.Answered, "precondition: nothing saved under the key");
                yield return ScrollAndTap(quiz.Choices[1].Button);
                Assert.AreEqual("Correct!", quiz.Verdict.text);
                Assert.IsTrue(PlayerPrefs.HasKey(answerKey), "the answer is in PlayerPrefs under wall.poi.block.slot");
                Assert.AreEqual("1", PlayerPrefs.GetString(answerKey));
                Assert.AreEqual(wall, Card.State.WallId, "the host's state is this wall's");

                SelectionEventBus.Clear();
                yield return OpenFull("lamp");
                Assert.AreEqual(1, Quizzes()[0].Index, "a new bind reads it back from PlayerPrefs: opens on question 2");
            }
            finally
            {
                if (answerBefore == null) PlayerPrefs.DeleteKey(answerKey); else PlayerPrefs.SetString(answerKey, answerBefore);
                if (indexBefore == null) PlayerPrefs.DeleteKey(indexKey); else PlayerPrefs.SetString(indexKey, indexBefore);
                PlayerPrefs.Save();
            }
        }

        // ---------------- Tier 3 group B on the real wall (_3.1 step 8B) ----------------

        private T Only<T>() where T : class, IBlockView => Sheet.Stack.BoundViews.OfType<T>().Single();

        private string StoredKey(string poiId, string block, string slot) => "ts.card." + Session.SearchConfig.wall_id + "." + poiId + "." + block + "." + slot;

        // _3.1 step 8B: The Lamp's poll on the real card: a real tap votes, the card shows the visitor's own choice and a thank-you and
        // NEVER a percentage (there is no backend), one event goes through the events seam, the vote is kept under wall + POI + block,
        // and a Portuguese card asks and thanks in Portuguese
        [UnityTest]
        public IEnumerator TheLamp_Poll_ARealTapVotes_NoPercentagesWithoutABackend_OneEvent_Kept_AndInPortugueseTheWordsFollow()
        {
            yield return OpenFull("lamp");
            var poll = Only<PollBlockView>();
            Assert.AreEqual("Which part of the castle would you visit first?", poll.Question.text);
            CollectionAssert.AreEqual(new[] { "The keep", "The curtain wall", "The gatehouse" }, poll.Options.Select(o => o.Text.text).ToList());
            Assert.IsFalse(poll.ResultsShown);
            yield return ScrollAndTap(poll.Options[1].Button);
            Assert.AreEqual(1, poll.Voted);
            Assert.AreEqual("Your choice", poll.Options[1].Caption.text);
            Assert.IsTrue(CardTestInput.IsShown(poll.Options[1].Mark, poll.Options[1].Button) && poll.Options[1].Mark.worldBound.width > 4f, "the tick element on the picked option");
            Assert.AreEqual("Thank you for voting", poll.Thanks.text);
            Assert.IsFalse(poll.ResultsShown, "the card's own poll results seam has nothing (no backend)");
            Assert.IsNull(Card.Services.Get<IPollResults>(), "no poll backend is registered today");
            Assert.IsFalse(poll.Root.Query<Label>().ToList().Any(l => CardTestInput.IsShown(l, poll.Root) && l.text.Contains("%")), "no percentage anywhere in the block");
            Assert.AreEqual(1, _cardEvents.Raised.Count(e => e.Kind == CardEventKinds.Poll), "one poll event");
            var raised = _cardEvents.Raised.Single(e => e.Kind == CardEventKinds.Poll);
            Assert.AreEqual(Session.SearchConfig.wall_id, raised.WallId);
            Assert.AreEqual("lamp", raised.PoiId);
            Assert.AreEqual("block_48", raised.BlockKey);
            Assert.AreEqual("bars", raised.Variant);
            Assert.AreEqual("2", raised.Value);
            Assert.AreEqual(1, Card.State.PollVote("lamp", "block_48"));
            Assert.IsTrue(_cardStore.TryGet(StoredKey("lamp", "block_48", "poll"), out string stored), "the scoped key");
            Assert.AreEqual("1", stored);
            yield return ScrollTo(poll);
            yield return Capture("Card_Lamp_Poll");

            // - closed and opened again: the vote shown as given, nothing reported again
            SelectionEventBus.Clear();
            yield return OpenFull("lamp");
            poll = Only<PollBlockView>();
            Assert.AreEqual(1, poll.Voted);
            Assert.AreEqual(1, _cardEvents.Raised.Count(e => e.Kind == CardEventKinds.Poll));

            // - Portuguese, on a fresh device
            _cardStore.Keys.ToList().ForEach(k => _cardStore.Remove(k));
            SelectionEventBus.Clear();
            LiveSettings.languages = new System.Collections.Generic.List<string> { "pt", "en" };
            yield return OpenFull("lamp");
            poll = Only<PollBlockView>();
            Assert.AreEqual("Que parte do castelo visitaria primeiro?", poll.Question.text);
            yield return ScrollAndTap(poll.Options[2].Button);
            Assert.AreEqual("A sua escolha", poll.Options[2].Caption.text);
            Assert.AreEqual("Obrigado pelo seu voto", poll.Thanks.text);
            Assert.AreEqual("A porta principal", poll.Options[2].Text.text);
        }

        // _3.1 step 8B: The Lamp's collect block: a real tap adds its item and it persists; the count is worked out from the wall's
        // CONFIG -- give another point of the wall a collect block and the total grows -- and its words are Portuguese in a
        // Portuguese card
        [UnityTest]
        public IEnumerator TheLamp_Collect_ARealTapAddsTheItem_ItPersists_TheCountComesFromTheWallsConfig_AndInPortugueseTheWordsFollow()
        {
            yield return OpenFull("lamp");
            var collect = Only<CollectBlockView>();
            Assert.AreEqual("Your story", Sheet.Stack.HeadingOf(collect).text, "no heading written: the card's default");
            Assert.AreEqual("Castles and towers", collect.Series.text);
            Assert.AreEqual("Stamp of the castle", collect.ItemName.text);
            Assert.AreEqual(1, collect.Total, "the wall's config has ONE collect block");
            Assert.AreEqual("0 of 1 collected", collect.Progress.text);

            // - another point of the running wall gets a collect block (in memory: the wall's own points have none): the total is 2
            var other = Session.SearchPois.First(p => p.id == "lamp_military");
            other.card.blocks.Add(new BlockInstanceData { key = "block_99", kind = BuiltInBlocks.CollectKind, variant = BuiltInBlocks.CollectAddToStory });
            try
            {
                SelectionEventBus.Clear();
                yield return OpenFull("lamp");
                collect = Only<CollectBlockView>();
                Assert.AreEqual(2, collect.Total, "counted from the config, not a constant");
                Assert.AreEqual("0 of 2 collected", collect.Progress.text);
                yield return ScrollAndTap(collect.Add);
                Assert.IsTrue(collect.IsCollected);
                Assert.AreEqual("In your story", collect.AddLabel.text);
                Assert.AreEqual("1 of 2 collected", collect.Progress.text);
                Assert.IsTrue(collect.StampStar.Filled);
                Assert.AreEqual(1, _cardEvents.Raised.Count(e => e.Kind == CardEventKinds.Collect));
                Assert.IsTrue(Card.State.Collected("lamp", "block_49"));
                Assert.IsTrue(_cardStore.TryGet(StoredKey("lamp", "block_49", "collected"), out _), "the scoped key");
                yield return ScrollTo(collect);
                yield return Capture("Card_Lamp_Collect");

                // - closed and opened again: still in the story
                SelectionEventBus.Clear();
                yield return OpenFull("lamp");
                collect = Only<CollectBlockView>();
                Assert.IsTrue(collect.IsCollected);
                Assert.AreEqual("1 of 2 collected", collect.Progress.text);

                // - the other point's own card counts the same wall: 1 of 2, its own item not yet collected
                SelectionEventBus.Clear();
                yield return OpenFull("lamp_military");
                var military = Only<CollectBlockView>();
                Assert.AreEqual("1 of 2 collected", military.Progress.text, "the count is the wall's, whichever card shows it");
                Assert.IsFalse(military.IsCollected);

                // - Portuguese
                _cardStore.Keys.ToList().ForEach(k => _cardStore.Remove(k));
                SelectionEventBus.Clear();
                LiveSettings.languages = new System.Collections.Generic.List<string> { "pt", "en" };
                yield return OpenFull("lamp");
                collect = Only<CollectBlockView>();
                Assert.AreEqual("A sua historia", Sheet.Stack.HeadingOf(collect).text);
                Assert.AreEqual("Selo do castelo", collect.ItemName.text);
                Assert.AreEqual("Castelos e torres", collect.Series.text);
                Assert.AreEqual("Adicionar a minha historia", collect.AddLabel.text);
                Assert.AreEqual("0 de 2 recolhidos", collect.Progress.text);
            }
            finally
            {
                other.card.blocks.RemoveAll(b => b.key == "block_99");
            }
        }

        // _3.1 step 8B: The Lamp's dialogue: real taps reveal one line at a time, the reply the visitor picks joins the thread, and
        // where they got to is never stored; a Portuguese card says the lines in Portuguese
        [UnityTest]
        public IEnumerator TheLamp_Dialogue_ARealTapRevealsOneLineAtATime_ARepliesJoinTheThread_NothingIsStored_AndInPortugueseTheWordsFollow()
        {
            yield return OpenFull("lamp");
            var dialogue = Only<DialogueBlockView>();
            Assert.AreEqual(3, dialogue.Count);
            Assert.AreEqual(1, dialogue.Bubbles.Count);
            Assert.AreEqual("The stonemason", dialogue.Bubbles[0].Speaker.text);
            Assert.AreEqual("Welcome to the castle. Do you know why its walls are so thick?", dialogue.Bubbles[0].Text.text);
            Assert.IsTrue(dialogue.AwaitsChoice, "the first line offers two replies");
            yield return ScrollAndTap(dialogue.Choices[0].Button);
            Assert.AreEqual(3, dialogue.Bubbles.Count, "the line, the visitor's reply and the stonemason's answer");
            Assert.AreEqual("You", dialogue.Bubbles[1].Speaker.text);
            Assert.AreEqual("No, tell me", dialogue.Bubbles[1].Text.text);
            Assert.AreEqual("Because it had to hold out against a siege for weeks.", dialogue.Bubbles[2].Text.text);
            yield return ScrollAndTap(dialogue.Continue);
            yield return ScrollAndTap(dialogue.Continue);
            Assert.AreEqual(5, dialogue.Bubbles.Count);
            Assert.AreEqual(3, dialogue.Reached);
            Assert.IsTrue(CardTestInput.IsShown(dialogue.Again, dialogue.Root));
            yield return ScrollTo(dialogue);
            yield return Capture("Card_Lamp_Dialogue");
            Assert.IsFalse(_cardStore.Keys.Any(k => k.Contains(".block_50.")), "the reached line and the picked reply are view state: nothing stored");
            Assert.AreEqual(0, _cardEvents.Raised.Count, "and nothing reported");

            SelectionEventBus.Clear();
            yield return OpenFull("lamp");
            Assert.AreEqual(1, Only<DialogueBlockView>().Bubbles.Count, "a new bind starts at the first line");

            SelectionEventBus.Clear();
            LiveSettings.languages = new System.Collections.Generic.List<string> { "pt", "en" };
            yield return OpenFull("lamp");
            dialogue = Only<DialogueBlockView>();
            Assert.AreEqual("O pedreiro", dialogue.Bubbles[0].Speaker.text);
            Assert.AreEqual("Nao, conte-me", dialogue.Choices[0].Label.text);
            yield return ScrollAndTap(dialogue.Choices[1].Button);
            Assert.AreEqual("Voce", dialogue.Bubbles[1].Speaker.text, "the visitor's own name in Portuguese");
            Assert.AreEqual("Entao tem bom olho.", dialogue.Bubbles[2].Text.text);
            Assert.AreEqual("Continuar", dialogue.Continue.Q<Label>().text);
        }

        // _3.1 step 8B: The Lamp's show_on_wall blocks on the RUNNING wall: a real tap on the button lowers the card to its peek and the
        // point's marker is the lit one (the selection stays: the others dim); the neighbour look names the three nearest points of the
        // wall and a real tap on one selects it
        [UnityTest]
        public IEnumerator TheLamp_ShowOnWall_ARealTapDropsTheCardToPeek_TheRightMarkerStaysLit_AndNeighboursAreTheNearestPoints()
        {
            yield return OpenFull("lamp");
            var views = Sheet.Stack.BoundViews.OfType<ShowOnWallBlockView>().ToList();
            Assert.AreEqual(2, views.Count, "the button and the neighbour look");
            Assert.AreEqual(0, views[0].Neighbours.Count);
            Assert.AreEqual(SheetStopRule.Stop.Full, Sheet.Stop);

            yield return ScrollAndTap(views[0].Button);
            yield return CardTestInput.Settle();
            Assert.AreEqual(SheetStopRule.Stop.Peek, Sheet.Stop, "the card dropped to its peek");
            Assert.IsTrue(Sheet.IsOpen);
            Assert.AreEqual("lamp", SelectionEventBus.CurrentPoiId, "the point stays selected");
            Assert.AreEqual(1f, Marker("lamp").SelectionAlpha, 1e-3, "THE RIGHT marker is the lit one on the running wall");
            Assert.AreEqual(Session.SpawnedMarkers.Count - 1, MarkersAt(0.3f).Count, "every other marker dimmed");
            yield return Capture("Card_Lamp_ShowOnWall_Peek");

            // - the neighbours: the three nearest other points by straight-line distance (worked out here, from the positions alone)
            SelectionEventBus.Clear();
            yield return OpenFull("lamp");
            var neighbours = Sheet.Stack.BoundViews.OfType<ShowOnWallBlockView>().Last();
            var lampPoi = Session.SearchPois.First(p => p.id == "lamp");
            POIPositionResolver.TryResolvePosition(lampPoi, out var here, logErrors: false);
            var expected = Session.SearchPois.Where(p => p.id != "lamp")
                .Select(p => (p, ok: POIPositionResolver.TryResolvePosition(p, out var at, logErrors: false), at))
                .Where(x => x.ok)
                .OrderBy(x => Vector3.Distance(here, x.at)).ThenBy(x => x.p.id, System.StringComparer.Ordinal)
                .Take(3).Select(x => x.p.id).ToList();
            CollectionAssert.AreEqual(expected, neighbours.Neighbours.Select(n => n.PoiId).ToList(), "the nearest three, nearest first");
            for (int i = 0; i < 3; i++)
                Assert.AreEqual(BlockStackBuilder.CardTitleOf(Session.SearchPois.First(p => p.id == expected[i]), "en", "en"), neighbours.Neighbours[i].Title.text, "named by card title");
            yield return ScrollTo(neighbours);
            yield return Capture("Card_Lamp_ShowOnWall_Neighbours");
            yield return ScrollAndTap(neighbours.Neighbours[0].Button);
            yield return CardTestInput.Settle();
            Assert.AreEqual(expected[0], SelectionEventBus.CurrentPoiId, "a real tap on a neighbour selected it");
            Assert.AreEqual(expected[0], Card.ShownPoiId, "the card is now its card");
        }
    }
}
