using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace TileStories.Tests
{
    // Tap outside vs double-tap zoom (_3.1 step 5b) proven with REAL touches in the REAL wall scene: a Touchscreen is
    // added to the running Input System and finger states are queued on it, so the zoom's own gesture reader
    // (ARZoomGestureInput, EnhancedTouch) and the card's tap poll (PoiCardHost, Pointer.current) both receive exactly
    // what a finger gives them -- nothing is called directly, and event times are the Input System's real clock.
    // (Not InputTestFixture: its reset re-enables the scene's shared UI actions against a fresh device set, which
    // throws inside InputSystemUIInputModule once another suite has used them.)
    public class PoiCardTapZoomTests : SearchSceneFixture
    {
        private Touchscreen _screen;
        private InputSettings _savedSettings;
        private InputSettings _testSettings;

        [SetUp]
        public void AddFinger()
        {
            _screen = InputSystem.AddDevice<Touchscreen>();
            // - an unfocused Game view drops pointer input in the Editor; a copy of the settings lets these touches in
            _savedSettings = InputSystem.settings;
            _testSettings = Object.Instantiate(_savedSettings);
            _testSettings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            InputSystem.settings = _testSettings;
        }

        [TearDown]
        public void RemoveFinger()
        {
            if (_screen != null && _screen.added) InputSystem.RemoveDevice(_screen);
            if (_savedSettings != null) InputSystem.settings = _savedSettings;
            if (_testSettings != null) Object.Destroy(_testSettings);
        }

        private void Queue(TouchPhase phase, Vector2 screenPoint) =>
            InputSystem.QueueStateEvent(_screen, new TouchState { touchId = 1, phase = phase, position = screenPoint, pressure = 1f });

        // One finger tap at `screenPoint`: down, held ~2 frames, up. Each state is processed in its frame's own input
        // update, before MonoBehaviour.Update (where "pressed / released this frame" is true).
        private IEnumerator Tap(Vector2 screenPoint)
        {
            Queue(TouchPhase.Began, screenPoint);
            yield return null;
            yield return null;
            Queue(TouchPhase.Ended, screenPoint);
            yield return null;
            yield return null;
        }

        private IEnumerator OpenLampCard()
        {
            SelectionEventBus.Select("lamp");
            yield return CardTestInput.Settle();
            Assert.IsTrue(Card.Sheet.IsOpen, "precondition: the card is open");
            Assert.AreEqual(1f, ARZoomState.ZoomFactor, 1e-4f, "precondition: no zoom");
            Assert.IsTrue(Session.ZoomSettings.enabled && Session.ZoomSettings.double_tap_window_s > 0f, "precondition: the shipped wall has double-tap zoom");
        }

        [UnityTest]
        public IEnumerator TwoRealTaps_OnEmptySpace_AreTheZoomsDoubleTap_AndTheCardStaysOpen()
        {
            yield return OpenLampCard();
            var empty = EmptyScreenPoint();

            yield return Tap(empty);
            Assert.IsTrue(Card.ClosePending, "the first real tap on nothing is heard by the card: a close is pending");
            Assert.IsTrue(Card.Sheet.IsOpen, "...but it does not close yet");
            yield return Tap(empty + new Vector2(6f, -4f));

            yield return CardTestInput.Settle(Session.ZoomSettings.double_tap_window_s + Session.ZoomSettings.transition_duration_s + 0.2f);
            Assert.IsTrue(Card.Sheet.IsOpen, "the zoom won: the card is still open");
            Assert.AreEqual("lamp", SelectionEventBus.CurrentPoiId, "and the POI is still selected");
            Assert.IsFalse(Card.ClosePending);
            Assert.Greater(ARZoomState.ZoomFactor, 1.01f, "the same two real taps stepped the zoom in");
        }

        [UnityTest]
        public IEnumerator OneRealTap_OnEmptySpace_ClosesTheCardAfterTheWindow_AndNeverZooms()
        {
            yield return OpenLampCard();
            var empty = EmptyScreenPoint();

            yield return Tap(empty);
            Assert.IsTrue(Card.Sheet.IsOpen, "still inside the double-tap window");
            yield return CardTestInput.Settle(Session.ZoomSettings.double_tap_window_s + 0.2f);
            Assert.IsFalse(Card.Sheet.IsOpen, "no second tap came: the card closed");
            Assert.IsNull(SelectionEventBus.CurrentPoiId, "through the bus");
            Assert.AreEqual(1f, ARZoomState.ZoomFactor, 1e-4f, "a single tap is not a zoom");
        }

        [UnityTest]
        public IEnumerator TwoRealTaps_FarApart_AreNotADoubleTap_TheCardCloses_AndNothingZooms()
        {
            yield return OpenLampCard();
            var empty = EmptyScreenPoint();
            var far = EmptyScreenPoint(empty, Session.ZoomSettings.double_tap_move_tolerance_px * 3f);

            float start = Time.unscaledTime;
            yield return Tap(empty);
            yield return Tap(far);
            Assert.Less(Time.unscaledTime - start, Session.ZoomSettings.double_tap_window_s, "precondition: both taps inside one double-tap window");
            Assert.IsTrue(Card.Sheet.IsOpen, "the second tap came before the first one's close was due");
            Assert.IsTrue(Card.ClosePending, "the far tap is a new lone tap on nothing: its own close is pending");
            yield return CardTestInput.Settle(Session.ZoomSettings.double_tap_window_s + 0.2f);
            Assert.IsFalse(Card.Sheet.IsOpen, "two lone taps on nothing: the card closes");
            Assert.AreEqual(1f, ARZoomState.ZoomFactor, 1e-4f, "the zoom did not take them as a double tap either");
        }
    }
}
