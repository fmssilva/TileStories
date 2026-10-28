using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // Phase B of the wall's Full Height (_3.3 S4a) on the REAL LivingRoomScene with The Lamp's card, pushed live the way the POI Editor
    // pushes a Card Container edit (WallSession.ApplyCardSettings):
    //   - To The Top (the default): at Full the sheet's top IS the top of the card layer -- the screen's safe area (SafeAreaHelper keeps the
    //     layer inside it); Leave A Camera Strip: the card's --ts-sheet-top-gap of camera above it; Peek and Half never move
    //   - either way the visitor can still lower it: a real drag down on the grabber (real touch events, the drag's own clock) and the
    //     system Back each take it from Full to Half; the X closes it
    [Category("Core")]
    public class PoiCardFullHeightSceneTests : SearchSceneFixture
    {
        private const string Lamp = "lamp";
        // A slow move's time per event (s): far under a flick, so the release snaps to the nearest stop
        private const float SlowStep = 0.1f;

        private PoiCardSheetView Sheet => Card.Sheet;
        private float OnePixel => Sheet.Layer.panel.visualTree.layout.height / Screen.height;
        private float _now;

        [UnityTearDown]
        public IEnumerator TheRealClockBack()
        {
            if (Card != null && Card.Sheet != null) Card.Sheet.Drag.Clock = () => Time.unscaledTime;
            yield break;
        }

        // Push the wall's Full Height live, then open The Lamp at Full and wait for the sheet to rest there
        private IEnumerator OpenAtFull(string fullHeight)
        {
            var copy = ConfigCopy();
            copy.card_settings.container.full_height = fullHeight;
            Session.ApplyCardSettings(copy);
            yield return CardTestInput.Settle(0.3f);
            if (SelectionEventBus.CurrentPoiId != Lamp)
            {
                SelectionEventBus.Select(Lamp);
                yield return CardTestInput.Settle();
            }
            Sheet.SetStop(SheetStopRule.Stop.Full);
            yield return RestAt(SheetStopRule.Stop.Full);
        }

        // Wait (frame by frame, up to 10 s) until the sheet's drawn height is its stop's height
        private IEnumerator RestAt(SheetStopRule.Stop stop)
        {
            yield return CardTestInput.Settle();
            float rest = Sheet.Stops.HeightOf(stop);
            for (int frame = 0; frame < 600 && Mathf.Abs(Sheet.Root.layout.height - rest) > OnePixel; frame++) yield return null;
            Assert.AreEqual(stop, Sheet.Stop, "the card rests at " + stop);
            Assert.AreEqual(rest, Sheet.Root.layout.height, OnePixel, "...drawn at that stop's height");
        }

        [UnityTest]
        public IEnumerator AtFull_ToTheTopReachesTheSafeAreasTop_ACameraStripLeavesTheGap_PeekAndHalfNeverMove()
        {
            yield return OpenAtFull(CardOptions.FullHeightTop);
            var layer = Sheet.Layer.worldBound;
            Assert.Greater(layer.height, 0f, "precondition: the card layer is laid out");
            var top = Sheet.Stops;
            Assert.AreEqual(layer.height, top.Full, OnePixel, "To The Top: the Full stop is the whole layer (the safe area)");
            Assert.AreEqual(layer.yMin, Sheet.Root.worldBound.yMin, OnePixel, "...the sheet's top at the safe area's top");

            yield return OpenAtFull(CardOptions.FullHeightCameraStrip);
            float gap = CardTestInput.TokenPx("--ts-sheet-top-gap");
            var strip = Sheet.Stops;
            Assert.AreEqual(layer.height - gap, strip.Full, OnePixel, "Leave A Camera Strip: the Full stop leaves the card's top gap");
            Assert.AreEqual(layer.yMin + gap, Sheet.Root.worldBound.yMin, OnePixel, "...a strip of camera above the sheet, pushed live onto the open card");
            Assert.AreEqual(top.Peek, strip.Peek, OnePixel, "Peek never moves with Full Height");
            Assert.AreEqual(top.Half, strip.Half, OnePixel, "...nor Half");

            yield return OpenAtFull(CardOptions.FullHeightTop);
            Assert.AreEqual(layer.yMin, Sheet.Root.worldBound.yMin, OnePixel, "back To The Top, live");
        }

        // The grabber takes a real drag down from Full to Half, the system Back does the same, the X still closes -- at both heights
        [UnityTest]
        public IEnumerator AtEitherHeight_ARealDragDownOnTheGrabber_AndBack_LowerItToHalf_AndTheXCloses(
            [Values(CardOptions.FullHeightTop, CardOptions.FullHeightCameraStrip)] string fullHeight)
        {
            yield return OpenAtFull(fullHeight);
            Assert.IsTrue(Sheet.Layer.worldBound.Contains(Sheet.Handle.worldBound.center), "precondition: the grabber is on the screen at Full");
            _now = 0f;
            Sheet.Drag.Clock = () => _now;
            var panel = Sheet.Root.panel;
            Vector2 at = Sheet.Handle.worldBound.center;
            // - 70 % of the way from Full down to Half, slowly: nearer Half, so the release snaps there
            float down = (Sheet.Stops.Full - Sheet.Stops.Half) * 0.7f;
            CardTestInput.TouchDown(panel, at);
            yield return null;
            Assert.IsTrue(Sheet.Drag.IsDragging, "the grabber takes the finger at Full");
            for (int i = 1; i <= 6; i++)
            {
                _now += SlowStep;
                CardTestInput.TouchMove(panel, at + new Vector2(0f, down * i / 6f));
                yield return null;
            }
            _now += SlowStep;
            CardTestInput.TouchUp(panel, at + new Vector2(0f, down));
            yield return RestAt(SheetStopRule.Stop.Half);

            Sheet.SetStop(SheetStopRule.Stop.Full);
            yield return RestAt(SheetStopRule.Stop.Full);
            Assert.IsTrue(Sheet.Back(), "the system Back is the card's at Full");
            yield return RestAt(SheetStopRule.Stop.Half);

            Sheet.SetStop(SheetStopRule.Stop.Full);
            yield return RestAt(SheetStopRule.Stop.Full);
            yield return CardTestInput.Tap(Sheet.CloseButton.panel, Sheet.CloseButton.worldBound.center);
            yield return CardTestInput.Settle();
            Assert.IsFalse(Sheet.IsOpen, "a tap on the X closes the card at Full");
        }
    }
}
