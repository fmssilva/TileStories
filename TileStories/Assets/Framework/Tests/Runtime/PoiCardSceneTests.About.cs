using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // Phase B of the about family on the real LivingRoomScene: The Lamp's rich_text (and its glossary in Portuguese), quick_facts, fun_fact,
    // pull_quote, status (the marker's own ring), the header chip and picture looks, and the sticky action. The fixture, its SetUp and the shared
    // helpers are in PoiCardSceneTests.cs.
    public partial class PoiCardSceneTests
    {
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
            Assert.AreEqual("A torre mais forte de um castelo, o seu \u00faltimo ref\u00fagio.", plain.Body.DefinitionText.text, "the wall's glossary, in Portuguese");
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
            StringAssert.Contains("n\u00e3o tem porta ao n\u00edvel do ch\u00e3o", flip.Text.Paragraphs[0].text);
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

        // 15.2.2 on the real scene: The Lamp's sticky footer carries no words of its own, so it reads the framework's Show On Wall words -- the
        // same action says the same thing on the footer and on the Show On Wall block, in each language (PT used to read two ways)
        [UnityTest]
        public IEnumerator TheLamp_TheStickyFooterAndTheShowOnWallBlock_SayTheSameWords_InEnglishAndInPortuguese()
        {
            var lampConfig = Session.SearchPois.First(p => p.id == "lamp");
            var stickyBlock = lampConfig.card.blocks.First(b => b.kind == BuiltInBlocks.ActionsKind && b.variant == BuiltInBlocks.ActionsStickyCta);
            var stickyRow = new BlockFieldReader(stickyBlock, null, null).Items(BuiltInBlocks.ActionsItemsField).Single();
            Assert.AreEqual("", new BlockFieldReader(stickyBlock, null, null).ItemText(stickyRow, BuiltInBlocks.ActionsLabelField), "precondition: the fixture's sticky button has no words of its own");

            foreach (var (language, expected) in new[] { ("en", "Show me where it is"), ("pt", "Mostra-me onde fica") })
            {
                SelectionEventBus.Clear();
                LiveSettings.languages = new System.Collections.Generic.List<string> { language, "en" };
                yield return OpenFull("lamp");
                var sticky = Sheet.Stack.BoundViews.OfType<ActionsBlockView>().Last();
                var wallButton = Sheet.Stack.BoundViews.OfType<ShowOnWallBlockView>().First();
                Assert.AreEqual(expected, sticky.Actions.Single().Label.text, language + ": the footer's words are the action's own");
                Assert.AreEqual(expected, wallButton.ButtonLabel.text, language + ": the same words on the Show On Wall block");
                Assert.AreEqual(Sheet.Stack.Footer, Sheet.Stack.SlotOf(sticky).parent, language + ": and it is still the pinned footer");
                yield return Capture("Card_Lamp_StickyWording_" + language);
            }
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
    }
}
