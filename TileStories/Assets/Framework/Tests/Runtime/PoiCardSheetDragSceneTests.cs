using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // SheetDrag (_3.2 chunk D: the sheet's vertical drag, out of PoiCardSheetView) on the REAL LivingRoomScene with The Lamp's card, real
    // touch events sent to the panel, and the drag's own clock (SheetDrag.Clock) so a "slow" or a "fast" move is what the test says:
    //   - while the finger is down the sheet's height IS the finger's travel (panel units, exact) and the drag class is on; a slow release
    //     snaps to the nearest stop and takes the class off
    //   - the SPEED decides, not only the distance: the same 30 units up from Peek stay at Peek slowly and reach Half as a flick
    //   - a drag past the top holds at Full; one pulled below 60 % of the peek closes the card through CloseRequested, as the X does
    //   - the top strip takes nothing at touch-down; once a move is clearly vertical the sheet follows it from where the finger went down
    public class PoiCardSheetDragSceneTests : SearchSceneFixture
    {
        private const string Lamp = "lamp";
        private const string DraggingClass = "poi-card--dragging";
        // A slow move's time per event (s): 30 units in 0.1 s is 300 units/s, far under a flick (1.2 heights/s, ~1000 at 844)
        private const float SlowStep = 0.1f;
        // A fast move's time per event (s): 15 units in 0.01 s is 1500 units/s, over a flick at any phone height up to 1250
        private const float FastStep = 0.01f;
        // A float's rounding on panel coordinates: the height is the start plus the finger's travel, nothing else
        private const float Exact = 0.01f;

        private PoiCardSheetView Sheet => Card.Sheet;
        private float _now;

        [UnitySetUp]
        public IEnumerator TheDragsOwnClock()
        {
            _now = 0f;
            Card.Sheet.Drag.Clock = () => _now;
            yield break;
        }

        [UnityTearDown]
        public IEnumerator TheRealClockBack()
        {
            if (Card != null && Card.Sheet != null) Card.Sheet.Drag.Clock = () => Time.unscaledTime;
            yield break;
        }

        private IEnumerator Open(SheetStopRule.Stop stop)
        {
            SelectionEventBus.Select(Lamp);
            yield return CardTestInput.Settle();
            Sheet.SetStop(stop);
            yield return CardTestInput.Settle();
            Assert.AreEqual(stop, Sheet.Stop, "precondition: the card rests at " + stop);
            Assert.AreEqual(Sheet.Stops.HeightOf(stop), Sheet.TargetHeight, Exact, "precondition: at that stop's height");
        }

        // Move the finger to `at`, `seconds` of the drag's clock after the last event
        private IEnumerator MoveTo(IPanel panel, Vector2 at, float seconds)
        {
            _now += seconds;
            CardTestInput.TouchMove(panel, at);
            yield return null;
        }

        [UnityTest]
        public IEnumerator WhileTheFingerIsDown_TheSheetFollowsItExactly_AndASlowReleaseSnapsToTheNearestStop()
        {
            yield return Open(SheetStopRule.Stop.Peek);
            float rest = Sheet.TargetHeight;
            var panel = Sheet.Root.panel;
            Vector2 at = Sheet.Handle.worldBound.center;

            CardTestInput.TouchDown(panel, at);
            yield return null;
            Assert.IsTrue(Sheet.Drag.IsDragging, "the grabber takes the finger at touch-down");
            Assert.IsTrue(Sheet.Root.ClassListContains(DraggingClass), "the drag class is on: no height transition under the finger");
            foreach (float up in new[] { 20f, 60f, 100f })
            {
                yield return MoveTo(panel, at + new Vector2(0f, -up), SlowStep);
                Assert.AreEqual(rest + up, Sheet.TargetHeight, Exact, up + " units up: the sheet is exactly that much taller");
            }
            _now += SlowStep;
            CardTestInput.TouchUp(panel, at + new Vector2(0f, -100f));
            yield return null;
            Assert.IsFalse(Sheet.Drag.IsDragging, "the finger lifted: the drag is over");
            Assert.IsFalse(Sheet.Root.ClassListContains(DraggingClass), "the class is off: the sheet animates to its stop");

            // - 100 up from Peek is nearer Half than Peek (the precondition says by how much)
            float released = rest + 100f;
            Assert.Less(Mathf.Abs(released - Sheet.Stops.Half), Mathf.Abs(released - Sheet.Stops.Peek),
                "precondition: " + released + " is nearer Half (" + Sheet.Stops.Half + ") than Peek (" + Sheet.Stops.Peek + ")");
            yield return CardTestInput.Settle();
            Assert.AreEqual(SheetStopRule.Stop.Half, Sheet.Stop, "a slow release goes to the nearest stop");
            Assert.AreEqual(Sheet.Stops.Half, Sheet.TargetHeight, Exact);
        }

        [UnityTest]
        public IEnumerator TheSameShortMoveUp_StaysAtPeekSlowly_AndReachesHalfAsAFlick([Values(false, true)] bool fast)
        {
            yield return Open(SheetStopRule.Stop.Peek);
            var panel = Sheet.Root.panel;
            Vector2 at = Sheet.Handle.worldBound.center;
            float step = fast ? FastStep : SlowStep;
            float flick = SheetStopRule.FlickHeightsPerSecond * Sheet.Layer.layout.height;
            Assert.AreEqual(fast, 15f / step >= flick, "precondition: the move's speed (" + 15f / step + " units/s) is " + (fast ? "over" : "under") + " a flick (" + flick + ")");
            Assert.Less(30f, (Sheet.Stops.Half - Sheet.Stops.Peek) / 2f, "precondition: 30 units is less than half the way to Half: the distance alone says Peek");

            CardTestInput.TouchDown(panel, at);
            yield return null;
            yield return MoveTo(panel, at + new Vector2(0f, -15f), step);
            yield return MoveTo(panel, at + new Vector2(0f, -30f), step);
            CardTestInput.TouchUp(panel, at + new Vector2(0f, -30f));
            yield return CardTestInput.Settle();
            Assert.AreEqual(fast ? SheetStopRule.Stop.Half : SheetStopRule.Stop.Peek, Sheet.Stop,
                fast ? "a flick goes one stop further in its direction" : "a slow short move goes back to where it was");
        }

        [UnityTest]
        public IEnumerator ADragPastTheTop_HoldsAtFull_AndOnePulledBelowThePeek_ClosesTheCard()
        {
            yield return Open(SheetStopRule.Stop.Peek);
            var panel = Sheet.Root.panel;
            Vector2 at = Sheet.Handle.worldBound.center;
            float past = Sheet.Stops.Full - Sheet.Stops.Peek + 200f;

            CardTestInput.TouchDown(panel, at);
            yield return null;
            yield return MoveTo(panel, at + new Vector2(0f, -past / 2f), SlowStep);
            yield return MoveTo(panel, at + new Vector2(0f, -past), SlowStep);
            Assert.AreEqual(Sheet.Stops.Full, Sheet.TargetHeight, Exact, "200 units past Full: held at Full, never above");
            CardTestInput.TouchUp(panel, at + new Vector2(0f, -past));
            yield return CardTestInput.Settle();
            Assert.AreEqual(SheetStopRule.Stop.Full, Sheet.Stop);

            // - back at Peek, then pulled down: a slow release under 60 % of the peek is the visitor pulling the card away
            Sheet.SetStop(SheetStopRule.Stop.Peek);
            yield return CardTestInput.Settle();
            at = Sheet.Handle.worldBound.center;
            float down = Sheet.Stops.Peek * (1f - SheetStopRule.DismissBelowPeekRatio) + 20f;
            bool closeAsked = false;
            void Asked() => closeAsked = true;
            Sheet.CloseRequested += Asked;
            try
            {
                CardTestInput.TouchDown(panel, at);
                yield return null;
                yield return MoveTo(panel, at + new Vector2(0f, down / 2f), SlowStep);
                yield return MoveTo(panel, at + new Vector2(0f, down), SlowStep);
                Assert.Less(Sheet.TargetHeight, Sheet.Stops.Peek * SheetStopRule.DismissBelowPeekRatio, "precondition: held under 60 % of the peek");
                CardTestInput.TouchUp(panel, at + new Vector2(0f, down));
                yield return CardTestInput.Settle();
            }
            finally { Sheet.CloseRequested -= Asked; }
            Assert.IsTrue(closeAsked, "the release asked to close, through the same event as the X");
            Assert.IsFalse(Sheet.IsOpen, "and the card closed");
        }

        [UnityTest]
        public IEnumerator TheTopStrip_TakesNothingAtTouchDown_AndOnceTheMoveIsVertical_TheSheetFollowsIt()
        {
            var copy = ConfigCopy();
            copy.card_settings.navigation.top = BuiltInNavElements.TextTabs;
            Session.ApplyCardSettings(copy);
            yield return CardTestInput.Settle(0.3f);
            yield return Open(SheetStopRule.Stop.Full);
            var tabs = Sheet.Slots.TopView as TextTabsNavView;
            Assert.IsNotNull(tabs, "precondition: Text Tabs in the top strip");
            Assert.IsTrue(Sheet.Chrome.TopShown, "precondition: the strip shows at Full");
            var panel = Sheet.Root.panel;
            Vector2 at = tabs.Tabs[0].Button.worldBound.center;

            CardTestInput.TouchDown(panel, at);
            yield return null;
            Assert.IsFalse(Sheet.Drag.IsDragging, "touch-down on a tab: nothing is the sheet's yet (it may still be a tap)");
            Assert.AreEqual(Sheet.Stops.Full, Sheet.TargetHeight, Exact);
            yield return MoveTo(panel, at + new Vector2(0f, 30f), SlowStep);
            yield return MoveTo(panel, at + new Vector2(0f, 60f), SlowStep);
            Assert.IsTrue(Sheet.Drag.IsDragging, "a clearly vertical move is the sheet's");
            Assert.AreEqual(Sheet.Stops.Full - 60f, Sheet.TargetHeight, Exact, "the height follows the finger from where it went down");
            CardTestInput.TouchUp(panel, at + new Vector2(0f, 60f));
            yield return CardTestInput.Settle();
            Assert.IsFalse(Sheet.Drag.IsDragging);
            Assert.AreEqual(SheetStopRule.Stop.Full, Sheet.Stop, "60 slow units down from Full stay at Full (Half is " + (Sheet.Stops.Full - Sheet.Stops.Half) + " away)");
        }
    }
}
