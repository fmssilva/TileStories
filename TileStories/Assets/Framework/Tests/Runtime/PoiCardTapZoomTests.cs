using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace TileStories.Tests
{
    // Tap outside vs double-tap zoom (_3.1 step 5b) proven with REAL touches in the REAL wall scene: a Touchscreen is
    // added to the running Input System and finger states are queued on it, so the zoom's own gesture reader
    // (ARZoomGestureInput, EnhancedTouch) and the card's tap poll (PoiCardHost, Pointer.current) both receive exactly
    // what a finger gives them -- nothing is called directly. TIME is the test's, not the frame rate's (_3.1 [7B]): every
// finger state carries an explicit event time (the zoom's double tap reads Touch.time) and the card's tap clock
// (PoiCardHost.Clock) reads the same test time, so no slow frame can decide whether two taps fell inside one window.
    // (Not InputTestFixture: its reset re-enables the scene's shared UI actions against a fresh device set, which
    // throws inside InputSystemUIInputModule once another suite has used them.)
    public class PoiCardTapZoomTests : SearchSceneFixture
    {
        private Touchscreen _screen;
        private InputSettings _savedSettings;
        private InputSettings _testSettings;
        // The test's own time: seconds since BeginTestTime; the Input System events get _eventBase + it
        private float _t;
        private double _eventBase;

        [SetUp]
        public void AddFinger()
        {
            // - an unfocused Game view drops pointer input in the Editor; a copy of the settings lets these touches in
            _savedSettings = InputSystem.settings;
            _testSettings = Object.Instantiate(_savedSettings);
            _testSettings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            // - and when the Editor is not the active application (the developer is in another window) the Input System would switch the
            //   touchscreen off (backgroundBehavior's default): every real-finger test then read "no touch" and failed. Keep it on.
            _testSettings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings = _testSettings;
            // - the device is added AFTER the settings: a device added while the default settings are active and the Editor is in the
            //   background is switched off on the spot (InputManager.AddDevice), and swapping the settings later never switches it back on
            _screen = InputSystem.AddDevice<Touchscreen>();
            if (!_screen.enabled) InputSystem.EnableDevice(_screen);
            Assert.IsTrue(_screen.enabled, "precondition: the real touchscreen is on, whichever application has the focus");
        }

        [TearDown]
        public void RemoveFinger()
        {
            if (_screen != null && _screen.added) InputSystem.RemoveDevice(_screen);
            if (_savedSettings != null) InputSystem.settings = _savedSettings;
            if (_testSettings != null) Object.Destroy(_testSettings);
            if (Card != null) Card.Clock = () => Time.unscaledTime;
        }

        // From here on the card's tap clock and the finger events run on the test's time (starting at 0)
        private void BeginTestTime()
        {
            _t = 0f;
            _eventBase = InputState.currentTime;
            Card.Clock = () => _t;
        }

        // Move the test's time forward by `seconds` (a pending close falls due on the card's next frame)
        private IEnumerator Advance(float seconds)
        {
            _t += seconds;
            yield return null;
            yield return null;
        }

        private void Queue(TouchPhase phase, Vector2 screenPoint) =>
            InputSystem.QueueStateEvent(_screen, new TouchState { touchId = 1, phase = phase, position = screenPoint, pressure = 1f },
                _eventBase + _t);

        // How long a tap's finger stays down (test time), well under any double-tap window
        private const float PressSeconds = 0.05f;

        // One finger tap at `screenPoint` starting NOW (test time): down, up PressSeconds later. Each state is processed in
        // its frame's own input update, before MonoBehaviour.Update (where "pressed / released this frame" is true).
        private IEnumerator Tap(Vector2 screenPoint)
        {
            Queue(TouchPhase.Began, screenPoint);
            yield return null;
            yield return null;
            _t += PressSeconds;
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
            float window = Session.ZoomSettings.double_tap_window_s;
            BeginTestTime();

            yield return Tap(empty);
            Assert.IsTrue(Card.ClosePending, "the first real tap on nothing is heard by the card: a close is pending");
            Assert.IsTrue(Card.Sheet.IsOpen, "...but it does not close yet");
            _t += window * 0.3f;
            yield return Tap(empty + new Vector2(6f, -4f));

            // - the whole window passes on the test's clock, then the zoom's step animation gets its real time
            yield return Advance(window * 2f);
            yield return CardTestInput.Settle(Session.ZoomSettings.transition_duration_s + 0.2f);
            Assert.IsTrue(Card.Sheet.IsOpen, "the zoom won: the card is still open");
            Assert.AreEqual("lamp", SelectionEventBus.CurrentPoiId, "and the POI is still selected");
            Assert.IsFalse(Card.ClosePending);
            Assert.Greater(ARZoomState.ZoomFactor, 1.01f, "the same two real taps stepped the zoom in");
        }

        // The proof the test's time decides (_3.1 [7B]): the same two taps as above, a few REAL frames apart but a window and
        // a half apart in event time -- neither the zoom nor the card may call them a double tap
        [UnityTest]
        public IEnumerator TwoRealTaps_AWindowAndAHalfApartInEventTime_AreTwoLoneTaps_EvenWhenFramesAreQuick()
        {
            yield return OpenLampCard();
            var empty = EmptyScreenPoint();
            float window = Session.ZoomSettings.double_tap_window_s;
            BeginTestTime();

            yield return Tap(empty);
            _t += window * 1.5f;
            // - the first tap's close fell due on the card's clock: it closes on this frame, before the second tap lands
            yield return null;
            yield return null;
            Assert.IsFalse(Card.Sheet.IsOpen, "the window passed on the test's clock: the lone first tap closed the card");
            yield return Tap(empty + new Vector2(6f, -4f));
            yield return Advance(window * 2f);
            yield return CardTestInput.Settle(Session.ZoomSettings.transition_duration_s + 0.2f);
            Assert.AreEqual(1f, ARZoomState.ZoomFactor, 1e-4f, "the zoom read the event times too: no double tap, no zoom step");
        }

        [UnityTest]
        public IEnumerator OneRealTap_OnEmptySpace_ClosesTheCardAfterTheWindow_AndNeverZooms()
        {
            yield return OpenLampCard();
            var empty = EmptyScreenPoint();
            float window = Session.ZoomSettings.double_tap_window_s;
            BeginTestTime();

            yield return Tap(empty);
            yield return Advance(window * 0.5f);
            Assert.IsTrue(Card.Sheet.IsOpen, "still inside the double-tap window (half of it passed)");
            Assert.IsTrue(Card.ClosePending);
            yield return Advance(window);
            Assert.IsFalse(Card.Sheet.IsOpen, "no second tap came and the window passed: the card closed");
            Assert.IsNull(SelectionEventBus.CurrentPoiId, "through the bus");
            Assert.AreEqual(1f, ARZoomState.ZoomFactor, 1e-4f, "a single tap is not a zoom");
        }

        [UnityTest]
        public IEnumerator TwoRealTaps_FarApart_AreNotADoubleTap_TheCardCloses_AndNothingZooms()
        {
            yield return OpenLampCard();
            var empty = EmptyScreenPoint();
            var far = EmptyScreenPoint(empty, Session.ZoomSettings.double_tap_move_tolerance_px * 3f);
            float window = Session.ZoomSettings.double_tap_window_s;
            BeginTestTime();

            yield return Tap(empty);
            _t += window * 0.3f;
            yield return Tap(far);
            Assert.Less(_t, window, "by construction: both taps inside one double-tap window (test time)");
            Assert.IsTrue(Card.Sheet.IsOpen, "the second tap came before the first one's close was due");
            Assert.IsTrue(Card.ClosePending, "the far tap is a new lone tap on nothing: its own close is pending");
            yield return Advance(window + PressSeconds);
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

            BeginTestTime();
            yield return Tap(centre);
            _t += Session.ZoomSettings.double_tap_window_s * 0.3f;
            yield return Tap(centre + new Vector2(4f, 2f));
            yield return Advance(Session.ZoomSettings.double_tap_window_s * 2f);
            yield return CardTestInput.Settle(Session.ZoomSettings.transition_duration_s + 0.2f);
            Assert.AreEqual(1f, ARZoomState.ZoomFactor, 1e-4f, "...nor does a double tap on the card");
            Assert.IsTrue(Card.Sheet.IsOpen, "and the card stays open");
        }

        // One real finger down at `from`, moved by `delta` over `frames` frames, lifted
        private IEnumerator Swipe(Vector2 from, Vector2 delta, int frames = 8)
        {
            QueueFinger(1, TouchPhase.Began, from);
            yield return null;
            for (int i = 1; i <= frames; i++)
            {
                QueueFinger(1, TouchPhase.Moved, from + delta * i / frames);
                yield return null;
            }
            QueueFinger(1, TouchPhase.Ended, from + delta);
            yield return null;
            yield return null;
        }

        // _3.1 [7B]: The Lamp's gallery lightbox under REAL fingers -- a tap opens it, one finger swiping left turns to the
        // next picture, two fingers enlarge the picture; the AR camera never zooms, and the enlarged picture is not turned
        [UnityTest]
        public IEnumerator TheLampsLightbox_ARealSwipeTurnsThePage_ARealPinchEnlargesIt_AndTheCameraNeverZooms()
        {
            yield return OpenLampCard();
            Card.Sheet.SetStop(SheetStopRule.Stop.Full);
            yield return CardTestInput.Settle();
            var stack = Card.Sheet.Stack;
            var gallery = System.Linq.Enumerable.First(System.Linq.Enumerable.OfType<GalleryBlockView>(stack.BoundViews),
                g => g.Shots.Count >= 2);
            stack.Scroll.ScrollTo(gallery.Shots[0].Box);
            yield return CardTestInput.Settle(0.2f);
            BeginTestTime();
            yield return Tap(ScreenPointOf(gallery.Shots[0].Box));
            yield return CardTestInput.Settle(0.2f);
            var takeover = Card.Sheet.Takeover;
            Assert.IsTrue(takeover.IsOpen, "a real finger tap on the picture opened the lightbox");
            int start = takeover.PageIndex;
            Assert.Less(start, takeover.PageCount - 1, "precondition: a next picture exists");

            var frame = takeover.Page.Q("card-gallery-full");
            Vector2 centre = ScreenPointOf(frame);
            float travel = frame.worldBound.width * Screen.width / frame.panel.visualTree.layout.width * 0.4f;
            Assert.IsTrue(ScreenUIHit.IsOverScreenUI(centre), "precondition: the finger goes down on the lightbox");
            yield return Swipe(centre, new Vector2(-travel, 0f));
            yield return CardTestInput.Settle(0.2f);
            Assert.AreEqual(start + 1, takeover.PageIndex, "one real finger swiping left: the next picture");
            Assert.IsTrue(takeover.IsOpen && Card.Sheet.IsOpen, "the lightbox and the card stay open");

            frame = takeover.Page.Q("card-gallery-full");
            centre = ScreenPointOf(frame);
            yield return Pinch(centre + new Vector2(-30f, 0f), centre + new Vector2(30f, 0f), 200f, frames: 10);
            yield return CardTestInput.Settle(0.2f);
            var picture = takeover.Page.Q(className: "card-zoompan__image");
            Assert.Greater(picture.worldBound.width, frame.worldBound.width * 1.3f, "two real fingers spreading enlarged the picture");
            Assert.AreEqual(1f, ARZoomState.ZoomFactor, 1e-4f, "the camera did not zoom");
            Assert.AreEqual(start + 1, takeover.PageIndex, "a pinch is not a page turn");
            yield return Capture("Card_Lamp_Lightbox_Pinched");

            yield return Swipe(centre, new Vector2(-travel, 0f));
            yield return CardTestInput.Settle(0.2f);
            Assert.AreEqual(start + 1, takeover.PageIndex, "an enlarged picture moves under a swipe, it does not turn");
            takeover.Close();
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

        private static void AssertStageHoldsNoPointer(KnowledgeCheckBlockView quiz, string when)
        {
            Assert.IsFalse(quiz.IsSwiping, when + ": not a swipe");
            for (int id = 0; id < PointerId.maxPointers; id++)
                Assert.IsFalse(quiz.Stage.HasPointerCapture(id), when + ": the statement card holds no pointer (id " + id + ")");
        }

        // _3.1 8A-fix: The Lamp's true / false statement is a big card, and a finger that lands on it must still be able to scroll the
        // stack. With REAL fingers: a vertical drag that starts on the statement scrolls the stack and answers nothing; a sideways
        // swipe from the same place answers True and does not scroll.
        [UnityTest]
        public IEnumerator TheLampsTrueFalseStatement_ARealVerticalFingerScrollsTheStack_AndAnswersNothing_ARealSidewaysSwipeAnswers()
        {
            // - the answer goes into a memory store: a test run must never write the developer's saved answers
            Card.State = new CardLocalState(new MemoryCardStateStore(), Session.SearchConfig.wall_id);
            yield return OpenLampCard();
            Card.Sheet.SetStop(SheetStopRule.Stop.Full);
            yield return CardTestInput.Settle();
            var stack = Card.Sheet.Stack;
            // - the question waits for the reading: a real wheel to the end of the card reveals it
            for (int notch = 0; notch < 400 && !stack.ContentSeen; notch++) yield return CardTestInput.Wheel(stack.Scroll, 12f);
            Assert.IsTrue(stack.ContentSeen, "precondition: the wheel reached the end of the card");
            var quiz = System.Linq.Enumerable.First(System.Linq.Enumerable.OfType<KnowledgeCheckBlockView>(stack.BoundViews),
                q => q.Root.ClassListContains("card-quiz--" + BuiltInBlocks.KnowledgeCheckTrueFalseSwipe));
            stack.Scroll.ScrollTo(quiz.Stage);
            yield return CardTestInput.Settle(0.2f);
            Assert.Greater(stack.Scroll.scrollOffset.y, 100f, "precondition: room to scroll back up");
            var centre = ScreenPointOf(quiz.Stage);
            Assert.IsTrue(ScreenUIHit.IsOverScreenUI(centre), "precondition: the finger goes down on the statement card");
            float perPanelUnit = Screen.width / quiz.Stage.panel.visualTree.layout.width;

            // - a real finger dragging DOWN from the statement: the stack scrolls up under it, nothing is answered
            float before = stack.Scroll.scrollOffset.y;
            var drag = new Vector2(2f, -160f);
            QueueFinger(1, TouchPhase.Began, centre);
            yield return null;
            // - the finger only TOUCHES the card: the card has not taken it (the old card captured the pointer at once, on the touch)
            AssertStageHoldsNoPointer(quiz, "a finger that only touched the statement");
            for (int i = 1; i <= 10; i++)
            {
                QueueFinger(1, TouchPhase.Moved, centre + drag * i / 10f);
                yield return null;
                if (i == 5) AssertStageHoldsNoPointer(quiz, "halfway down, clearly vertical");
            }
            QueueFinger(1, TouchPhase.Ended, centre + drag);
            yield return null;
            yield return null;
            yield return CardTestInput.Settle(0.2f);
            Assert.Less(stack.Scroll.scrollOffset.y, before - 30f, "a vertical drag that started on the statement scrolled the stack");
            Assert.AreEqual(-1, quiz.Chosen, "...and answered nothing");
            Assert.IsFalse(quiz.IsSwiping, "the card was never taken by the swipe");
            Assert.AreEqual(0f, quiz.Stage.resolvedStyle.translate.x, 0.01f, "the card did not move");

            // - a real sideways swipe from the same place: True (the statement is true), and the stack stays where it is
            stack.Scroll.ScrollTo(quiz.Stage);
            yield return CardTestInput.Settle(0.2f);
            centre = ScreenPointOf(quiz.Stage);
            before = stack.Scroll.scrollOffset.y;
            yield return Swipe(centre, new Vector2(quiz.Stage.worldBound.width * perPanelUnit * 0.5f, 4f), frames: 10);
            yield return CardTestInput.Settle(0.2f);
            Assert.AreEqual(KnowledgeCheckRule.ChoiceTrue, quiz.Chosen, "a real sideways swipe answered True");
            Assert.AreEqual(before, stack.Scroll.scrollOffset.y, 2f, "a sideways swipe does not scroll the stack");
            yield return Capture("Card_Lamp_KnowledgeCheck_TrueFalse_RealSwipe");
        }
    }
}
