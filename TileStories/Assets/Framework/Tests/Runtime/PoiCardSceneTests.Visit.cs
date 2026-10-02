using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // Phase B of the visit family on the real LivingRoomScene: wall_locator, today_map, related (carousel, next_along_wall), practical_info.
    public partial class PoiCardSceneTests
    {
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
    }
}
