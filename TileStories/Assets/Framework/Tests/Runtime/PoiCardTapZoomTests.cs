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

        // ---------------- pinch and double tap on the screen UI (_3.1 step 7: zoom_image pinches on the card) ----------------

        private void QueueFinger(int id, TouchPhase phase, Vector2 screenPoint) =>
            InputSystem.QueueStateEvent(_screen, new TouchState { touchId = id, phase = phase, position = screenPoint, pressure = 1f });

        // Two real fingers down at `a` and `b`, spread apart by `spread` pixels over `frames` frames, then lifted
        private IEnumerator Pinch(Vector2 a, Vector2 b, float spread, int frames = 8)
        {
            QueueFinger(1, TouchPhase.Began, a);
            yield return null;
            QueueFinger(2, TouchPhase.Began, b);
            yield return null;
            Vector2 away = (b - a).normalized;
            for (int i = 1; i <= frames; i++)
            {
                float d = spread * 0.5f * i / frames;
                QueueFinger(1, TouchPhase.Moved, a - away * d);
                QueueFinger(2, TouchPhase.Moved, b + away * d);
                yield return null;
            }
            QueueFinger(1, TouchPhase.Ended, a - away * spread * 0.5f);
            QueueFinger(2, TouchPhase.Ended, b + away * spread * 0.5f);
            yield return null;
            yield return null;
        }

        // The screen point (pixels) at the centre of a card element
        private static Vector2 ScreenPointOf(UnityEngine.UIElements.VisualElement e)
        {
            var panelSize = e.panel.visualTree.layout.size;
            var c = e.worldBound.center;
            return new Vector2(c.x * Screen.width / panelSize.x, Screen.height - c.y * Screen.height / panelSize.y);
        }

        [UnityTest]
        public IEnumerator ARealPinch_OnEmptyCameraSpace_StillZoomsTheCamera()
        {
            yield return OpenLampCard();
            var a = EmptyScreenPoint();
            var b = EmptyScreenPoint(a, 60f);
            Assert.IsFalse(ScreenUIHit.IsOverScreenUI(a) || ScreenUIHit.IsOverScreenUI(b), "precondition: both fingers on the camera view");
            yield return Pinch(a, b, 160f);
            Assert.Greater(ARZoomState.ZoomFactor, 1.05f, "a pinch on the wall zooms the camera, as before");
        }

        [UnityTest]
        public IEnumerator ARealPinch_OnTheCard_IsTheCardsGesture_TheCameraNeverZooms()
        {
            yield return OpenLampCard();
            Card.Sheet.SetStop(SheetStopRule.Stop.Full);
            yield return CardTestInput.Settle();
            var stack = Card.Sheet.Stack.Scroll.contentViewport;
            var centre = ScreenPointOf(stack);
            var a = centre + new Vector2(-40f, 0f);
            var b = centre + new Vector2(40f, 0f);
            Assert.IsTrue(ScreenUIHit.IsOverScreenUI(a) && ScreenUIHit.IsOverScreenUI(b), "precondition: both fingers on the card");
            yield return Pinch(a, b, 160f);
            Assert.AreEqual(1f, ARZoomState.ZoomFactor, 1e-4f, "fingers that started on the card never zoom the camera");

            yield return Tap(centre);
            yield return Tap(centre + new Vector2(4f, 2f));
            yield return CardTestInput.Settle(Session.ZoomSettings.double_tap_window_s + Session.ZoomSettings.transition_duration_s + 0.2f);
            Assert.AreEqual(1f, ARZoomState.ZoomFactor, 1e-4f, "...nor does a double tap on the card");
            Assert.IsTrue(Card.Sheet.IsOpen, "and the card stays open");
        }

        // _3.1 step 7: The Lamp's zoom_image under two REAL fingers (Touchscreen -> the UI input module -> one UI Toolkit
        // pointer per finger): the picture enlarges about the fingers; the AR camera, which never gets those fingers, does not
        [UnityTest]
        public IEnumerator ARealTwoFingerPinch_OnTheLampsZoomImage_EnlargesThePicture_AndNeverTheCamera()
        {
            yield return OpenLampCard();
            Card.Sheet.SetStop(SheetStopRule.Stop.Full);
            yield return CardTestInput.Settle();
            var stack = Card.Sheet.Stack;
            var zoom = System.Linq.Enumerable.Single(System.Linq.Enumerable.OfType<ZoomImageBlockView>(stack.BoundViews));
            stack.Scroll.ScrollTo(stack.SlotOf(zoom));
            yield return CardTestInput.Settle(0.2f);
            Assert.AreEqual("tile_detail", zoom.Image.Texture.name, "the fixture's detailed picture");
            Assert.AreEqual(1f, zoom.Scale, "precondition: the whole picture");
            var centre = ScreenPointOf(zoom.Frame);
            Assert.IsTrue(ScreenUIHit.IsOverScreenUI(centre), "precondition: the fingers go down on the card");
            float scroll = stack.Scroll.scrollOffset.y;

            yield return Pinch(centre + new Vector2(-30f, 0f), centre + new Vector2(30f, 0f), 180f, frames: 10);
            yield return CardTestInput.Settle(0.2f);
            Assert.Greater(zoom.Scale, 1.5f, "two real fingers spreading enlarged the picture");
            Assert.LessOrEqual(zoom.Scale, ZoomPanRule.MaxScale);
            Assert.AreEqual(1f, ARZoomState.ZoomFactor, 1e-4f, "the camera did not zoom");
            Assert.AreEqual(scroll, stack.Scroll.scrollOffset.y, 1f, "the card did not scroll under the pinch");
            yield return Capture("Card_Lamp_ZoomImage_Pinched");

            // - a plain mouse wheel over the picture is the card's: it scrolls, it never zooms
            float zoomed = zoom.Scale;
            yield return CardTestInput.Wheel(zoom.Frame, -4f, frames: 2);
            yield return CardTestInput.Settle(0.2f);
            Assert.AreEqual(zoomed, zoom.Scale, 1e-4f, "a plain wheel never zooms");
            Assert.Less(stack.Scroll.scrollOffset.y, scroll - 5f, "...it scrolled the long card up");
        }
    }
}
