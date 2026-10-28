using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // Phase B of step 15a (_3.2 section 5.6 D4, nav_bento) on the REAL LivingRoomScene with real touches. The fixture is the shipped card of
    // Lamp - Residential (config content added at this step: a short card, a "Start here" bento with four tiles over its text, fun fact, video
    // and 3D model -- the Lamp itself keeps no nav block of its own, which Auto Nav Block's tests rely on):
    //   - the four tiles, in the author's order, with the author's words / the block's own, real pictures loaded from the wall's media folder,
    //     two to a row with the first and the lonely last one spanning it; English and Portuguese
    //   - a REAL tap on a jump tile lands its block (+-1 px) with ONE nav_jump named nav_bento
    //   - a REAL tap on a full-view tile opens that block over the card (no jump, the card does not move), its breadcrumb ends with the block's
    //     words, and Back gives everything back -- ASSERTED ON THE OWNERS as step 12 does: the preview owner's slots (the model's full-view slot
    //     goes, the card's stays), the card's media source (the files the full view loaded), the video owner (nothing playing, no clip)
    //   - a tile whose block a live edit deleted, or that points at the bento itself, is not drawn and the others stay
    //   - renders at 390 px in English and Portuguese: the tiles, one with a picture
    [Ignore(TestSkips.OldNavigationElements)]
    public class PoiCardNavBentoSceneTests : SearchSceneFixture
    {
        private const string Residential = "lamp_residential";
        private const string Bento = "block_6";

        private PoiCardSheetView Sheet => Card.Sheet;
        private NavController Nav => Sheet.Nav;
        private ScrollView Scroll => Sheet.Stack.Scroll;
        private float OnePixel => Sheet.Layer.panel.visualTree.layout.height / Screen.height;
        private float Range => NavScrollRule.Range(Scroll.contentContainer.layout.height, Scroll.contentViewport.layout.height);
        private NavBentoBlockView View => Sheet.Stack.BoundViews.OfType<NavBentoBlockView>().SingleOrDefault();

        private sealed class RecordingCardEvents : ICardEvents
        {
            public readonly List<CardEvent> Raised = new();
            public void Raise(CardEvent cardEvent) => Raised.Add(cardEvent);
        }

        private RecordingCardEvents _events;
        private ManualVideoOutput _videoOut;
        private CardVideoService _video;
        private List<string> _shippedLanguages;
        private List<CardEvent> NavJumps => _events.Raised.Where(e => e.Kind == CardEventKinds.NavJump).ToList();

        [UnitySetUp]
        public IEnumerator UseStandInDevicesAnInMemoryCardStateAndRecordTheEvents()
        {
            _videoOut = new ManualVideoOutput();
            _video = new CardVideoService(_videoOut, () => Card.Media);
            Card.UseVideoService(_video);
            Card.State = new CardLocalState(new MemoryCardStateStore(), Session.SearchConfig.wall_id);
            _events = new RecordingCardEvents();
            Card.Events = _events;
            yield break;
        }

        [UnityTearDown]
        public IEnumerator LetTheStandInsGo()
        {
            _video.CardClosed();
            _videoOut.Release();
            yield break;
        }

        // ---------------- helpers ----------------

        // The wall pushed through the live seam after `edit`: the card's content is applied with it (a live edit rebinds the open card)
        private IEnumerator Push(Action<WallConfigData> edit, string language = "en")
        {
            var copy = ConfigCopy();
            _shippedLanguages ??= new List<string>(ConfigCopy().card_settings.languages);
            copy.card_settings.languages = language == "en" ? new List<string>(_shippedLanguages) : new List<string> { language, "en" };
            edit?.Invoke(copy);
            Session.ApplyCardSettings(copy);
            yield return CardTestInput.Settle(0.3f);
        }

        private IEnumerator Open(string poi, SheetStopRule.Stop stop)
        {
            if (SelectionEventBus.CurrentPoiId != poi)
            {
                SelectionEventBus.Clear();
                yield return CardTestInput.Settle();
                SelectionEventBus.Select(poi);
                yield return CardTestInput.Settle();
            }
            Scroll.scrollOffset = Vector2.zero;
            Sheet.SetStop(stop);
            yield return CardTestInput.Settle();
            Assert.AreEqual(stop, Sheet.Stop, "precondition: the card rests at " + stop);
            float rest = Sheet.Stops.HeightOf(Sheet.Stop);
            for (int frame = 0; frame < 600 && Mathf.Abs(Sheet.Root.layout.height - rest) > OnePixel; frame++) yield return null;
            Assert.AreEqual(rest, Sheet.Root.layout.height, OnePixel, "precondition: the sheet rests at its " + Sheet.Stop + " height");
            yield return CardTestInput.Settle(0.2f);
        }

        private IEnumerator Landed()
        {
            for (int frame = 0; frame < 600 && Nav.IsJumping; frame++) yield return null;
            Assert.IsFalse(Nav.IsJumping, "the jump came to rest");
            yield return CardTestInput.Settle(0.3f);
        }

        private void AssertLanded(string block, string name)
        {
            Assert.Greater(Range, 100f, "precondition: the card really scrolls (range " + Range + ")");
            var slot = Sheet.Stack.SlotOfBlock(block);
            if (Sheet.Stack.LandingTop(slot) <= Range - OnePixel)
                Assert.AreEqual(Sheet.Stack.LandingInset, Sheet.Stack.LandingElement(slot).worldBound.yMin - Scroll.contentViewport.worldBound.yMin, OnePixel, name + ": its top meets the viewport's top");
            else
                Assert.AreEqual(Range, Scroll.scrollOffset.y, OnePixel, name + ": a late block clamps at the end of the range");
        }

        // A real tap by a touch pointer on a point of the card layer, the point first asserted to be inside the visible part of the card
        private IEnumerator TouchTapTile(NavBentoBlockView.Tile tile)
        {
            Scroll.ScrollTo(tile.Button);
            yield return CardTestInput.Settle(0.2f);
            Assert.IsTrue(Scroll.contentViewport.worldBound.Contains(tile.Button.worldBound.center),
                "precondition: the tile is inside the visible part of the card: " + tile.Button.worldBound + " in " + Scroll.contentViewport.worldBound);
            yield return CardTestInput.TouchDrag(Sheet.Layer.panel, tile.Button.worldBound.center, Vector2.zero, frames: 1);
            yield return CardTestInput.Settle();
        }

        private static IEnumerator WaitFor(Func<bool> condition, string what, int frames = 900)
        {
            for (int i = 0; i < frames && !condition(); i++) yield return null;
            Assert.IsTrue(condition(), "timed out waiting for " + what);
        }

        private IEnumerator TapBack()
        {
            yield return CardTestInput.Tap(Sheet.Takeover.Back.panel, Sheet.Takeover.Back.worldBound.center);
            yield return CardTestInput.Settle(0.2f);
            Assert.IsFalse(Sheet.FullView.IsOpen, "a real tap on Back closed the full view");
        }

        // ---------------- the tests ----------------

        [UnityTest]
        public IEnumerator TheShippedBento_DrawsItsFourTiles_WithRealWordsAndPictures_TwoToARowFirstAndLastSpanningIt_InEnglishAndPortuguese()
        {
            foreach (string language in new[] { "en", "pt" })
            {
                yield return Push(null, language);
                SelectionEventBus.Clear();
                yield return CardTestInput.Settle();
                yield return Open(Residential, SheetStopRule.Stop.Full);
                bool pt = language == "pt";
                var view = View;
                Assert.IsNotNull(view, language + ": the shipped card of Lamp - Residential draws its bento");
                var tiles = view.Tiles;
                CollectionAssert.AreEqual(new[] { "block_2", "block_4", "block_5", "block_3" }, tiles.Select(t => t.Target), language + ": the author's tiles in order");
                Assert.AreEqual(pt ? "As casas" : "The houses", tiles[0].Words.text, "the author's words in " + language);
                Assert.AreEqual(pt ? "Ver o filme" : "Watch the film", tiles[1].Words.text);
                for (int i = 2; i < 4; i++)
                {
                    Assert.IsNotEmpty(Nav.RowOf(tiles[i].Target).Words, "precondition: the block has words of its own");
                    Assert.AreEqual(Nav.RowOf(tiles[i].Target).Words, tiles[i].Words.text, "tile " + i + ": no words of its own: the block's, in " + language);
                }
                Assert.IsTrue(tiles[0].HasPicture && tiles[0].Picture.Texture != null, language + ": images/castle_then.jpg loaded from the wall's media folder");
                Assert.IsTrue(tiles[1].HasPicture && tiles[1].Picture.Texture != null, language + ": the video tile's photo loaded");
                Assert.IsFalse(tiles[2].HasPicture || tiles[3].HasPicture, "the other two have none");
                Assert.AreEqual(BentoTileRule.Opens.Jump, tiles[0].Opens);
                Assert.AreEqual(BentoTileRule.Opens.FullView, tiles[1].Opens);
                Assert.AreEqual(BentoTileRule.Opens.FullView, tiles[2].Opens);
                Assert.AreEqual(BentoTileRule.Opens.Jump, tiles[3].Opens);
                CollectionAssert.AreEqual(new[] { true, false, false, true }, BentoTileRule.Wide(tiles.Count), "the rule's spans for four tiles");

                // - the grid as drawn: the two middle tiles share a row (each about half), the first and last span the block
                var grid = view.Root.worldBound;
                Assert.AreEqual(1f, tiles[0].Cell.worldBound.width / grid.width, 0.02f, "the first tile spans the row");
                Assert.AreEqual(1f, tiles[3].Cell.worldBound.width / grid.width, 0.02f, "so does the lonely last one");
                Assert.AreEqual(0.5f, tiles[1].Cell.worldBound.width / grid.width, 0.02f, "the middle two are half a row each");
                Assert.AreEqual(tiles[1].Cell.worldBound.yMin, tiles[2].Cell.worldBound.yMin, OnePixel, "in the same row");
                Assert.Greater(tiles[2].Cell.worldBound.xMin, tiles[1].Cell.worldBound.xMin, "side by side");
                Assert.Greater(tiles[3].Cell.worldBound.yMin, tiles[2].Cell.worldBound.yMax - OnePixel, "the last one under them");
                float touch = CardTestInput.TokenPx("--ts-touch-target");
                foreach (var tile in tiles)
                {
                    Assert.GreaterOrEqual(tile.Button.layout.height, touch - OnePixel, tile.Words.text + ": tall enough to tap");
                    Assert.GreaterOrEqual(tile.Button.layout.width, touch - OnePixel, tile.Words.text + ": wide enough to tap");
                    var size = tile.Words.MeasureTextSize(tile.Words.text, 0, VisualElement.MeasureMode.Undefined, 0, VisualElement.MeasureMode.Undefined);
                    string widest = tile.Words.text.Split(' ').OrderByDescending(w => tile.Words.MeasureTextSize(w, 0, VisualElement.MeasureMode.Undefined, 0,
                        VisualElement.MeasureMode.Undefined).x).First();
                    var word = tile.Words.MeasureTextSize(widest, 0, VisualElement.MeasureMode.Undefined, 0, VisualElement.MeasureMode.Undefined);
                    Assert.GreaterOrEqual(tile.Words.layout.width + 0.5f, word.x, "'" + widest + "': the whole word fits, never cut (" + size.x + " wide in total)");
                    float contrast = CardTestInput.Contrast(tile.Words.resolvedStyle.color, CardTestInput.EffectiveBackground(tile.Words));
                    Assert.GreaterOrEqual(contrast, UIAccessibility.MinRatioNormalText, "'" + tile.Words.text + "' reads at 4.5:1 (" + contrast.ToString("F2") + ")");
                }
                Scroll.ScrollTo(view.Root);
                yield return CardTestInput.Settle(0.2f);
                yield return CardGalleryChecks.Render("Nav_Lamp_residential_bento_" + language + "_390");
            }
        }

        [UnityTest]
        public IEnumerator ARealTapOnAJumpTile_LandsItsBlock_WithOneNavJumpNamedNavBento_AndOpensNoFullView()
        {
            yield return Open(Residential, SheetStopRule.Stop.Full);
            var tiles = View.Tiles;
            foreach (int index in new[] { 3, 0 })
            {
                Scroll.scrollOffset = Vector2.zero;
                yield return CardTestInput.Settle(0.3f);
                _events.Raised.Clear();
                var tile = tiles[index];
                Assert.AreEqual(BentoTileRule.Opens.Jump, tile.Opens, "precondition: tile " + index + " is a jump tile");
                yield return TouchTapTile(tile);
                yield return Landed();
                AssertLanded(tile.Target, "tile " + index + "'s block, from a real tap");
                Assert.AreEqual(1, NavJumps.Count, "one nav_jump");
                Assert.AreEqual(NavBentoBlockView.ElementKey, NavJumps[0].Variant, "named by the bento block");
                Assert.AreEqual(tile.Target, NavJumps[0].BlockKey, "to the tile's block");
                Assert.IsFalse(Sheet.FullView.IsOpen, "a jump tile opens no full view");
            }
        }

        // The brief's acceptance, on the OWNERS: a full-view tile opens the block bound a second time, a real Back gives back exactly what the
        // full view held -- the preview owner's second slot, the files its media scope loaded -- and the video owner was never asked to play
        [UnityTest]
        public IEnumerator ARealTapOnAFullViewTile_OpensTheBlockOverTheCard_TheCardStaysPut_AndBackGivesTheMediaAndTheSlotBack()
        {
            yield return Open(Residential, SheetStopRule.Stop.Full);
            var tiles = View.Tiles;
            var held = Card.PreviewService.HeldKeys.ToList();
            int files = Card.Media.LoadedCount;
            yield return WaitFor(() => Sheet.Stack.ViewOfBlock("block_5") is ModelTurntableBlockView m && m.ShowsSubject, "the card's own model");
            held = Card.PreviewService.HeldKeys.ToList();
            files = Card.Media.LoadedCount;
            CollectionAssert.Contains(held, "block_5", "precondition: the card's inline model holds its slot");

            // - the model tile
            Scroll.scrollOffset = Vector2.zero;
            yield return CardTestInput.Settle(0.3f);
            float offset = Scroll.scrollOffset.y;
            _events.Raised.Clear();
            yield return TouchTapTile(tiles[2]);
            Assert.IsTrue(Sheet.FullView.IsOpen, "a real tap on the model tile opened the full view");
            Assert.AreEqual("block_5", Sheet.FullView.BlockKey);
            Assert.IsEmpty(NavJumps, "it is no jump");
            StringAssert.EndsWith(" > " + Nav.RowOf("block_5").Words, Sheet.Takeover.CrumbText, "the breadcrumb ends with the block's words");
            var full = (ModelTurntableBlockView)Sheet.FullView.View;
            CollectionAssert.AreEquivalent(held.Append("block_5::takeover"), Card.PreviewService.HeldKeys, "the block bound twice holds TWO slots: the card's untouched, the full view's own");
            yield return WaitFor(() => full.ShowsSubject, "the full view's model");
            yield return TapBack();
            CollectionAssert.AreEquivalent(held, Card.PreviewService.HeldKeys, "Back gave back ONLY the full view's slot");
            Assert.AreEqual(files, Card.Media.LoadedCount, "and the card's media holds exactly what it held before the tile was tapped");
            Assert.AreEqual(offset, Scroll.scrollOffset.y, OnePixel, "the card behind never moved");

            // - the video tile: the full view binds a second player; nothing plays, no clip reaches the device, and everything goes back on Back
            Assert.AreEqual(CardVideoState.Idle, _video.State, "precondition: nothing plays");
            yield return TouchTapTile(tiles[1]);
            Assert.IsTrue(Sheet.FullView.IsOpen, "a real tap on the video tile opened the full view");
            Assert.AreEqual("block_4", Sheet.FullView.BlockKey);
            Assert.IsInstanceOf<VideoBlockView>(Sheet.FullView.View, "the video's own player, a second view of it");
            Assert.AreNotSame(Sheet.Stack.ViewOfBlock("block_4"), Sheet.FullView.View, "not the card's own view");
            Assert.AreEqual(CardVideoState.Idle, _video.State, "opening it plays nothing");
            Assert.IsNull(_videoOut.Clip, "and hands the device no clip");
            Assert.IsEmpty(NavJumps, "no jump");
            yield return TapBack();
            CollectionAssert.AreEquivalent(held, Card.PreviewService.HeldKeys, "the preview owner is as it was");
            Assert.AreEqual(files, Card.Media.LoadedCount, "the card's media holds exactly what it held: the full view's files went back");
            Assert.AreEqual(CardVideoState.Idle, _video.State);
            Assert.IsNull(_videoOut.Clip);
        }

        [UnityTest]
        public IEnumerator ATileWhoseBlockALiveEditDeleted_OrThatPointsAtTheBentoItself_IsNotDrawn_AndTheOthersStay()
        {
            yield return Open(Residential, SheetStopRule.Stop.Full);
            Assert.AreEqual(4, View.Tiles.Count, "precondition: four tiles");
            yield return Push(c =>
            {
                var poi = c.pois.Single(p => p.id == Residential);
                // - the fun fact (tile 4's block) is deleted; tile 3 now points at the bento itself
                poi.card.blocks.RemoveAll(b => b.key == "block_3");
                var tiles = poi.card.blocks.Single(b => b.key == Bento).fields.Single(f => f.key == BuiltInBlocks.NavBentoTilesField).items;
                tiles[2].fields.Single(f => f.key == BuiltInBlocks.NavBentoTargetField).value = Bento;
            });
            Assert.IsNotNull(View, "the bento is still drawn");
            CollectionAssert.AreEqual(new[] { "block_2", "block_4" }, View.Tiles.Select(t => t.Target), "the deleted block's tile and the self-pointing one are gone, the others stay in order");
            CollectionAssert.AreEqual(new[] { true, true }, BentoTileRule.Wide(View.Tiles.Count), "and the two left span the row (no hole)");
            Assert.AreEqual(1f, View.Tiles[1].Cell.worldBound.width / View.Root.worldBound.width, 0.02f, "as drawn");

            // - every tile gone: the block draws nothing at all
            yield return Push(c =>
            {
                var poi = c.pois.Single(p => p.id == Residential);
                poi.card.blocks.RemoveAll(b => b.key == "block_2" || b.key == "block_4" || b.key == "block_5" || b.key == "block_3");
            });
            Assert.AreEqual(DisplayStyle.None, View.Root.resolvedStyle.display, "no reachable tile: no block");
        }
    }
}
