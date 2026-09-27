using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // Phase B of the POI Detail Card (_3.1 section 7): the REAL LivingRoomScene with its PoiCard object, the
    // shipped config (fixture: "The Lamp" = the authored text_only header, "Lamp - Military" = the compact one, every
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
            Assert.AreEqual("Royal Government - Hub", Header.ChipText, "category name - level name");
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
            yield return CardTestInput.Drag(Sheet.Handle, -(Sheet.Stops.Half - Sheet.Stops.Peek));
            yield return CardTestInput.Settle();
            Assert.AreEqual(SheetStopRule.Stop.Half, Sheet.Stop);
            Assert.LessOrEqual(Sheet.Root.resolvedStyle.height, Sheet.Layer.layout.height * 0.40f + 1f, "half never passes 40% of the screen");
            var subtitle = Header.Root.Q<Label>("card-header-subtitle");
            Assert.LessOrEqual(subtitle.worldBound.yMax, Sheet.Root.worldBound.yMax + 1f, "half shows the subtitle");
            yield return Capture("Card_Lamp_Half");

            yield return CardTestInput.Drag(Sheet.Handle, -(Sheet.Stops.Full - Sheet.Stops.Half));
            yield return CardTestInput.Settle();
            Assert.AreEqual(SheetStopRule.Stop.Full, Sheet.Stop);
            Assert.AreEqual(Sheet.Layer.layout.height - Sheet.Stops.Full, 48f, 1f, "full leaves the token's top gap (--ts-sheet-top-gap)");
            Assert.AreEqual("lamp", SelectionEventBus.CurrentPoiId, "dragging never changes the selection");
            yield return Capture("Card_Lamp_Full");
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
            Assert.IsTrue(PoiCardHost.AnythingUnder(ScreenPointOf(Sheet.CloseButton)), "the EventSystem sees the card (UI Toolkit panel raycaster)");
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
            Assert.AreEqual("Military - 3", Header.ChipText);
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
            Assert.AreEqual("Fontes", sources[0].Heading.text, "the framework's Portuguese heading");
            Assert.AreEqual("Carta sobre a conquista de Lisboa", sources[0].Rows[0].Title.text);
            Assert.AreEqual("Museu Nacional do Azulejo", sources[0].Rows[1].Author.text);
            Assert.IsFalse(CardTestInput.IsShown(sources[0].Confidence, sources[0].Root), "list: no chip");
            Assert.AreEqual("Verificado", sources[1].Confidence.text, "with_confidence + verified: the Portuguese chip");
            Sheet.Stack.Scroll.ScrollTo(sources[1].Root);
            yield return CardTestInput.Settle(0.2f);
            yield return Capture("Card_Lamp_Sources");
        }

        [UnityTest]
        public IEnumerator TheLamp_IsTheFullCard_EveryTier1GroupAKindAndVariant_InCatalogOrder()
        {
            yield return OpenFull("lamp");
            CollectionAssert.AreEqual(new[]
            {
                "header", "status", "status", "quick_facts", "quick_facts", "quick_facts", "rich_text", "rich_text", "rich_text", "rich_text",
                "fun_fact", "fun_fact", "pull_quote", "pull_quote", "sources", "sources", "actions", "actions", "actions",
            }, ShownKinds(), "one block per kind and variant, nothing skipped");
            var sticky = Sheet.Stack.BoundViews.OfType<ActionsBlockView>().Last();
            Assert.AreEqual(Sheet.Stack.Footer, sticky.Root.parent, "the sticky actions are pinned to the footer");
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
        public IEnumerator LampMilitary_IsTheShortCard_HeaderRichTextAndQuickFacts()
        {
            yield return OpenFull("lamp_military");
            CollectionAssert.AreEqual(new[] { BuiltInBlocks.HeaderKind, BuiltInBlocks.RichTextKind, BuiltInBlocks.QuickFactsKind }, ShownKinds(),
                "the short card: header, rich_text, quick_facts only");
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
    }
}
