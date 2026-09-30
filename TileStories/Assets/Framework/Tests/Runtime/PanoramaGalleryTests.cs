using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace TileStories.Tests
{
    // Phase A of panorama_360, drag and gyro (_3.1 step 10A.4.3): the real CardPreviewStage and CardPreviewService behind the gallery card
    // (CardGalleryHarness wires the SAME real stage as the wall's card), the Framework's own default 360 picture. A real one-finger drag turns
    // the view (CardTestInput.DragFrom, exactly the pointer events a finger sends); the pinch is a real Ctrl + wheel (a trackpad pinch arrives
    // as exactly that; a mouse alone cannot make two pointers). What is judged is what the stage DREW: the slot's own RenderTexture, read back,
    // never only the state that says where the view looks. The gyro look is fed by a real Input System attitude sensor. Generic fit / contrast /
    // tap-target / heading / render checks of every entry are CardGalleryTests' own.
    public class PanoramaGalleryTests
    {
        private const string ScenePath = "Assets/Dev/CardGallery/CardGalleryScene.unity";
        private CardGalleryHarness _harness;
        private BackgroundSafeInput _input;
        private AttitudeSensor _sensor;

        private static int IndexOf(string name) => CardGalleryDefinitions.All.ToList().FindIndex(e => e.Name == name);

        [UnitySetUp]
        public IEnumerator SetUp()
        {
#if UNITY_EDITOR
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
#else
            Assert.Ignore("Needs the Editor to load the gallery scene by path.");
#endif
            for (int i = 0; i < 60 && _harness == null; i++)
            {
                _harness = Object.FindFirstObjectByType<CardGalleryHarness>();
                yield return null;
            }
            Assert.IsNotNull(_harness, "the gallery scene holds CardGalleryHarness");
            _harness.EnsureBuilt();
            yield return null;
            // - the sensor (gyro tests) keeps working whichever application has the focus; the attitude is read exactly as queued
            _input = new BackgroundSafeInput(compensateForScreenOrientation: false);
        }

        [TearDown]
        public void TearDown()
        {
            if (_sensor != null && _sensor.added) InputSystem.RemoveDevice(_sensor);
            _sensor = null;
            _input?.Dispose();
        }

        private IEnumerator ShowBlock(string name, System.Action<PanoramaBlockView> got)
        {
            _harness.Show(IndexOf(name));
            yield return CardGalleryChecks.SettledBlock(_harness, name, v => got((PanoramaBlockView)v));
        }

        // A phone held upright facing the horizon; turning right is negative about the sensor's up axis
        private static Quaternion TurnedRight(float degrees) => Quaternion.AngleAxis(-degrees, Vector3.forward) * Quaternion.AngleAxis(90f, Vector3.right);

        private static float Difference(Color a, Color b) => Mathf.Abs(a.r - b.r) + Mathf.Abs(a.g - b.g) + Mathf.Abs(a.b - b.b);

        [UnityTest]
        public IEnumerator DragLook_DefaultPanorama_LoadsAndRenders_ARealDragTurnsTheViewTheWayTheFingerPulls()
        {
            PanoramaBlockView view = null;
            yield return ShowBlock("panorama_360_drag_default", v => view = v);
            yield return null;

            Assert.IsTrue(view.ShowsPanorama, "the Framework's own default 360 picture loaded and is drawn");
            Assert.IsFalse(view.IsGyroLook);
            Assert.AreEqual("Drag to look around, pinch to zoom", view.Hint.text);
            Assert.AreEqual(0f, view.State.Yaw, "the first view is straight ahead");
            var drawn = PreviewPixels.DrawnBounds(view.Texture);
            Assert.AreEqual(new RectInt(0, 0, view.Texture.width, view.Texture.height), drawn, "from inside the sphere every pixel of the stage is the picture");
            Color before = PreviewPixels.Average(view.Texture);

            yield return CardTestInput.DragFrom(view.Frame.panel, view.Frame.worldBound.center, new Vector2(120f, 0f));
            yield return null;
            // - the scene follows the finger: dragging right turns the view LEFT, so yaw 0 wraps to just under 360
            Assert.Greater(view.State.Yaw, 180f, "a real one-finger drag to the right turned the view left: " + view.State.Yaw);
            float shorterSide = Mathf.Min(view.Frame.resolvedStyle.width, view.Frame.resolvedStyle.height);
            float expectedTurn = 120f * PanoramaViewRule.DegreesPerUnit(PanoramaViewState.DefaultFov, shorterSide);
            Assert.AreEqual(360f - expectedTurn, view.State.Yaw, 1f, "a drag of N panel units turns N * fov / the stage's shorter side degrees");
            Color afterDrag = PreviewPixels.Average(view.Texture);
            Assert.Greater(Difference(before, afterDrag), 0.01f, "the turn changed what is DRAWN, not just the state: " + before + " -> " + afterDrag);

            // - dragging down looks up (pitch grows), never past the pole
            yield return CardTestInput.DragFrom(view.Frame.panel, view.Frame.worldBound.center, new Vector2(0f, 60f));
            yield return null;
            Assert.Greater(view.State.Pitch, 0f, "dragging down looked up");
            yield return CardTestInput.DragFrom(view.Frame.panel, view.Frame.worldBound.center, new Vector2(0f, 4000f), frames: 4);
            yield return null;
            Assert.AreEqual(PanoramaViewRule.MaxPitch, view.State.Pitch, 0.01f, "a huge drag stops short of straight up");
            yield return CardGalleryChecks.Render("Card_panorama_drag_dragged");
        }

        [UnityTest]
        public IEnumerator DragLook_ARealCtrlWheelPinch_ChangesTheFieldOfView_WithinTheRulesLimits_AndTheRender()
        {
            PanoramaBlockView view = null;
            yield return ShowBlock("panorama_360_drag_default", v => view = v);
            yield return null;
            Assert.AreEqual(PanoramaViewState.DefaultFov, view.State.Fov, 1e-3f);
            Color wide = PreviewPixels.Average(view.Texture);
            Vector2 at = view.Frame.worldBound.center;

            // - 6 notches in (negative) are a factor of 2^(6/12): the field of view narrows by that much
            yield return CardTestInput.CtrlWheel(view.Frame.panel, at, -6f);
            yield return null;
            Assert.AreEqual(PanoramaViewState.DefaultFov / Mathf.Pow(2f, 0.5f), view.State.Fov, 0.05f, "a real Ctrl + wheel notch run zoomed in");
            Assert.AreEqual(0f, view.State.Yaw, 1e-3f, "a pinch never turns the view");
            Color zoomed = PreviewPixels.Average(view.Texture);
            Assert.Greater(Difference(wide, zoomed), 0.005f, "a narrower field of view draws a different picture: " + wide + " -> " + zoomed);

            yield return CardTestInput.CtrlWheel(view.Frame.panel, at, -200f);
            yield return null;
            Assert.AreEqual(PanoramaViewRule.MinFov, view.State.Fov, 1e-3f, "zoom stops at the rule's narrowest");
            yield return CardTestInput.CtrlWheel(view.Frame.panel, at, 400f);
            yield return null;
            Assert.AreEqual(PanoramaViewRule.MaxFov, view.State.Fov, 1e-3f, "and at its widest");

            // - a plain wheel belongs to the card (it scrolls): it never zooms
            yield return CardTestInput.Wheel(view.Frame, 3f);
            yield return null;
            Assert.AreEqual(PanoramaViewRule.MaxFov, view.State.Fov, 1e-3f, "a wheel without Ctrl is not a pinch");
        }

        [UnityTest]
        public IEnumerator StartHeading_TurnsTheFirstView_AndTheRenderDiffersFromTheDefault()
        {
            PanoramaBlockView view = null;
            yield return ShowBlock("panorama_360_drag_default", v => view = v);
            yield return null;
            Color ahead = PreviewPixels.Average(view.Texture);

            yield return ShowBlock("panorama_360_drag_start-heading", v => view = v);
            yield return null;
            Assert.AreEqual(90f, view.State.Yaw, 0.01f, "the authored Start Heading is where the view opens");
            Assert.AreEqual(90f, view.StartHeading, 0.01f);
            Assert.Greater(Difference(ahead, PreviewPixels.Average(view.Texture)), 0.01f, "a quarter turn right looks at another wall of the room");
        }

        [UnityTest]
        public IEnumerator GyroLook_WithNoSensor_FallsBackToDragging_AndSaysSo()
        {
            Assert.IsFalse(DeviceAttitude.Available, "precondition: no attitude sensor on this machine");
            PanoramaBlockView view = null;
            yield return ShowBlock("panorama_360_gyro_default", v => view = v);
            yield return null;
            Assert.IsTrue(view.IsGyroLook);
            Assert.IsTrue(view.ShowsPanorama);
            Assert.IsFalse(view.IsFollowingSensor);
            Assert.AreEqual("Drag to look around, pinch to zoom", view.Hint.text, "no sensor: the drag words");

            yield return CardTestInput.DragFrom(view.Frame.panel, view.Frame.worldBound.center, new Vector2(100f, 0f));
            yield return null;
            Assert.AreNotEqual(0f, view.State.Yaw, "where no sensor exists a real drag still turns the gyro look");
        }

        [UnityTest]
        public IEnumerator GyroLook_ARealSimulatedSensor_TurnsTheRenderedView_AndADragNoLongerDoes()
        {
            _sensor = InputSystem.AddDevice<AttitudeSensor>();
            Assert.AreSame(_sensor, AttitudeSensor.current, "precondition: the simulated sensor is the Input System's current one");

            PanoramaBlockView view = null;
            yield return ShowBlock("panorama_360_gyro_default", v => view = v);
            yield return null;
            Assert.IsTrue(_sensor.enabled, "the bound gyro view switched the sensor on");
            Assert.AreEqual("Move your phone to look around", view.Hint.text, "a sensor is there: the 'move your phone' hint");

            // - the first reading (the phone held ahead) anchors the view; then the phone turns a quarter to the right
            InputSystem.QueueDeltaStateEvent(_sensor.attitude, TurnedRight(0f));
            for (int i = 0; i < 120 && !view.IsFollowingSensor; i++) yield return null;
            Assert.IsTrue(view.IsFollowingSensor, "the view's own timer read the sensor");
            Assert.AreEqual(0f, view.State.Yaw, 0.05f, "anchored at the start heading");
            Color ahead = PreviewPixels.Average(view.Texture);

            InputSystem.QueueDeltaStateEvent(_sensor.attitude, TurnedRight(90f));
            for (int i = 0; i < 120 && Mathf.Abs(Mathf.DeltaAngle(view.State.Yaw, 90f)) > 0.1f; i++) yield return null;
            Assert.AreEqual(90f, view.State.Yaw, 0.1f, "the phone turned 90 degrees right: so did the view");
            Assert.Greater(Difference(ahead, PreviewPixels.Average(view.Texture)), 0.01f, "and the stage DREW the other wall, not just the state");
            yield return CardGalleryChecks.Render("Card_panorama_gyro_turned");

            float yaw = view.State.Yaw;
            yield return CardTestInput.DragFrom(view.Frame.panel, view.Frame.worldBound.center, new Vector2(150f, 0f));
            yield return null;
            Assert.AreEqual(yaw, view.State.Yaw, 0.05f, "with a sensor a finger does not fight the phone");

            // - the view going away gives the sensor back
            Assert.AreEqual(1, DeviceAttitude.Users);
            _harness.Show(IndexOf("panorama_360_drag_default"));
            yield return CardTestInput.Settle(0.2f);
            Assert.AreEqual(0, DeviceAttitude.Users, "no gyro view is bound any more");
            Assert.IsFalse(_sensor.enabled, "the sensor was switched off again");
        }

        [UnityTest]
        public IEnumerator AMissingPanorama_WithAFallbackPicture_ShowsTheFallback_NeverTheSurface()
        {
            PanoramaBlockView view = null;
            yield return ShowBlock("panorama_360_drag_missing-with-fallback", v => view = v);
            yield return null;
            Assert.IsFalse(view.ShowsPanorama);
            Assert.AreEqual(DisplayStyle.Flex, view.Fallback.Root.resolvedStyle.display);
            Assert.IsNotNull(view.Fallback.Texture, "the authored Fallback Picture loaded and shows in place of the panorama");
        }

        [UnityTest]
        public IEnumerator AMissingPanorama_WithNoFallbackPicture_ShowsCardImagesOwnUnavailableWords_BothLooks()
        {
            foreach (string look in new[] { "drag", "gyro" })
            {
                PanoramaBlockView view = null;
                yield return ShowBlock("panorama_360_" + look + "_missing-no-fallback", v => view = v);
                yield return null;
                Assert.IsFalse(view.ShowsPanorama, look);
                Assert.AreEqual(DisplayStyle.Flex, view.Fallback.Root.resolvedStyle.display, look);
                Assert.AreNotEqual("", view.Fallback.Unavailable.text, look + ": no Fallback Picture authored: CardImage's own 'unavailable' words");
                // - a failed slot draws nothing and a drag on it changes nothing (the stage never had a panorama to turn)
                float yaw = view.State.Yaw;
                yield return CardTestInput.DragFrom(view.Frame.panel, view.Frame.worldBound.center, new Vector2(100f, 0f));
                yield return null;
                Assert.AreEqual(yaw, view.State.Yaw, look + ": nothing to turn");
            }
        }

        // The Display Takeover of both looks: a static teaser (no hint, no sensor), a real tap opens the panorama full screen through a SECOND
        // slot, a real drag turns that page only, Back releases the slot and leaves nothing behind.
        [UnityTest]
        public IEnumerator DisplayTakeover_ATeaserThatARealTapOpensFullScreen_ARealDragTurnsIt_BackReleasesTheSecondSlot()
        {
            foreach (string look in new[] { "drag", "gyro" })
            {
                PanoramaBlockView view = null;
                yield return ShowBlock("panorama_360_" + look + "_takeover", v => view = v);
                yield return null;
                Assert.AreEqual(CardOptions.DisplayTakeover, view.Display, look);
                Assert.AreEqual(DisplayStyle.None, view.Hint.resolvedStyle.display, look + ": no hint on a teaser that cannot be dragged");
                Assert.AreEqual("The tiled room", view.TeaserName.text, look);
                Assert.IsTrue(view.ShowsPanorama, look + ": the teaser's own static first render");
                Assert.AreEqual(0, DeviceAttitude.Users, look + ": a static teaser never follows the sensor");
                Assert.IsTrue(UIAccessibility.MeetsMinTapTarget(view.TeaserOpenButton.worldBound.width, view.TeaserOpenButton.worldBound.height));
                yield return CardGalleryChecks.Render("Card_panorama_" + look + "_takeover_teaser");

                float restingYaw = view.State.Yaw;
                yield return CardTestInput.DragFrom(view.Frame.panel, view.Frame.worldBound.center, new Vector2(120f, 0f));
                yield return null;
                Assert.AreEqual(restingYaw, view.State.Yaw, look + ": the teaser itself never turns on drag");

                int before = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None).Length;
                yield return CardTestInput.Tap(view.TeaserOpenButton.panel, view.TeaserOpenButton.worldBound.center);
                yield return CardTestInput.Settle(0.15f);
                var takeover = _harness.Sheet.Takeover;
                Assert.IsTrue(takeover.IsOpen, look + ": a real tap on the teaser's button opened it full screen");
                Assert.IsTrue(takeover.Crumb.text.EndsWith(" > The tiled room"), look + ": the card's title, then the panorama's own");

                var full = view.FullScreenView;
                Assert.IsNotNull(full, look + ": a second instance over a second preview slot");
                Assert.AreEqual(CardOptions.DisplayInline, full.Display);
                Assert.AreEqual(look == "gyro", full.IsGyroLook, "the page keeps the block's look");
                Assert.AreEqual(DisplayStyle.Flex, full.Hint.resolvedStyle.display);
                Assert.AreNotSame(view.Texture, full.Texture, "a second slot: its own RenderTexture");
                Assert.IsTrue(full.ShowsPanorama);
                yield return CardGalleryChecks.Render("Card_panorama_" + look + "_takeover_fullscreen");

                float before2 = full.State.Yaw;
                yield return CardTestInput.DragFrom(full.Frame.panel, full.Frame.worldBound.center, new Vector2(120f, 0f));
                yield return null;
                Assert.AreNotEqual(before2, full.State.Yaw, look + ": a real one-finger drag turns it full screen");
                Assert.AreEqual(restingYaw, view.State.Yaw, look + ": and never touches the teaser's own resting state");

                yield return CardTestInput.Tap(takeover.Back.panel, takeover.Back.worldBound.center);
                yield return CardTestInput.Settle(0.15f);
                Assert.IsFalse(takeover.IsOpen, look + ": Back closed it");
                Assert.IsNull(view.FullScreenView, look + ": the second slot was let go");
                Assert.IsTrue(view.ShowsPanorama, look + ": the card's own teaser picture is untouched");
                Assert.AreEqual(0, DeviceAttitude.Users, look + ": the page gave the sensor back");

                yield return null;
                int after = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None).Length;
                Assert.AreEqual(before, after, look + ": nothing left behind: the second slot's sphere and root are gone, the shared rig stays");
            }
        }
    }
}
