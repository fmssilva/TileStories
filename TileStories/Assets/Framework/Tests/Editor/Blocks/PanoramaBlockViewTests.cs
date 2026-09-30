using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TileStories.Editor.Tests
{
    // The panorama_360 view's gyro look (_3.1 step 10A.4.2) fed by a REAL Input System attitude sensor: a test device added to the
    // Input System (the same kind of device a phone registers), a state event queued into it, and the view read back. The stage behind
    // the slot is the fake ManualPreviewStage -- the stage is not the subject here; what the view does with the sensor is. The real
    // stage's pixels following the sensor are PanoramaGalleryTests'. The view's timer needs a panel, so a test calls PollAttitude,
    // the method the timer calls.
    public class PanoramaBlockViewTests
    {
        private TileStories.Tests.BackgroundSafeInput _input;
        private AttitudeSensor _sensor;
        private CardPreviewService _service;
        private PanoramaBlockView _view;
        // The framework's real card words (CardStrings.asset), so the hint tests read what a visitor would read
        private static CardStrings Words() =>
            new(AssetDatabase.LoadAssetAtPath<CardStringTable>("Assets/Framework/Runtime/UI/Cards/CardStrings.asset").Entries(), null, null, "en", "en");

        [SetUp]
        public void SetUp()
        {
            // - the attitude control compensates for the screen's orientation (the Editor's own is arbitrary): off, so a queued quaternion reads back
            // exactly; and the sensor keeps working whichever application has the focus
            _input = new TileStories.Tests.BackgroundSafeInput(compensateForScreenOrientation: false);
            _service = new CardPreviewService(new ManualPreviewStage(), () => null);
            _view = new PanoramaBlockView();
        }

        [TearDown]
        public void TearDown()
        {
            _view.Unbind();
            _service.ReleaseAll();
            if (_sensor != null) InputSystem.RemoveDevice(_sensor);
            _sensor = null;
            _input.Dispose();
            Assert.AreEqual(0, DeviceAttitude.Users, "every bound view gave the sensor back");
        }

        // A sensor as a phone leaves it while nobody follows it: present but OFF (a device added in the Editor comes up on, so the
        // test switches it off, which is the state DeviceAttitude.Begin has to bring it out of)
        private void AddSensor()
        {
            _sensor = InputSystem.AddDevice<AttitudeSensor>();
            Assert.AreSame(_sensor, AttitudeSensor.current, "precondition: the test sensor is the Input System's current one");
            InputSystem.DisableDevice(_sensor);
            Assert.IsFalse(_sensor.enabled, "precondition: the sensor is switched off");
        }

        private void Feed(Quaternion attitude)
        {
            Assert.IsTrue(_sensor.enabled, "precondition: events only reach a sensor that is switched on (a bound gyro view did that)");
            InputSystem.QueueDeltaStateEvent(_sensor.attitude, attitude);
            InputSystem.Update();
        }

        private void Bind(string variant, string display = CardOptions.DisplayInline, float startHeading = 0f)
        {
            var instance = new BlockInstanceData { key = "block_1", kind = BuiltInBlocks.Panorama360Kind, variant = variant, display = display };
            instance.fields.Add(new BlockFieldValue { key = BuiltInBlocks.Panorama360PanoramaField, asset = MediaPathRule.PathForDefaultKey("tiled_room_360") });
            if (startHeading != 0f) instance.fields.Add(new BlockFieldValue { key = BuiltInBlocks.Panorama360StartHeadingField, number = startHeading });
            _view.Bind(instance, new BlockBindContext
            {
                Poi = new POIData { id = "p" },
                Language = "en",
                FallbackLanguage = "en",
                Variant = variant,
                Preview = _service,
                Strings = Words(),
            });
        }

        // A phone held upright, its back facing the horizon: +90 degrees about the sensor's x axis; turning right is negative about up
        private static Quaternion Upright => Quaternion.AngleAxis(90f, Vector3.right);
        private static Quaternion TurnedRight(float degrees) => Quaternion.AngleAxis(-degrees, Vector3.forward) * Upright;

        [Test]
        public void GyroLook_WithASensor_TheViewFollowsThePhone_TheFirstReadingIsTheStartHeading()
        {
            AddSensor();
            Bind(BuiltInBlocks.Panorama360Gyro, startHeading: 90f);
            Assert.IsTrue(_view.IsGyroLook);
            Assert.AreEqual(1, DeviceAttitude.Users, "a bound gyro view follows the sensor");
            Assert.IsTrue(_sensor.enabled, "and switches it on");
            Assert.AreEqual(90f, _view.State.Yaw, 0.01f, "before any reading the view points at the authored Start Heading");

            Assert.IsFalse(_view.PollAttitude(), "a sensor that has delivered nothing yet is no reading: the view stays put");
            Assert.IsFalse(_view.IsFollowingSensor);

            Feed(TurnedRight(40f));
            _view.PollAttitude();
            Assert.IsTrue(_view.IsFollowingSensor, "a reading was delivered: raw " + _sensor.attitude.ReadValue() + ", sensor enabled " + _sensor.enabled +
                ", current " + (AttitudeSensor.current == _sensor) + ", users " + DeviceAttitude.Users + ", gyro look " + _view.IsGyroLook);
            Assert.AreEqual(90f, _view.State.Yaw, 0.01f, "the first reading anchors: however the visitor faces now IS the start heading");

            Feed(TurnedRight(70f));
            Assert.IsTrue(_view.PollAttitude(), "the phone turned: a new frame is needed");
            Assert.AreEqual(120f, _view.State.Yaw, 0.01f, "30 degrees further right on the phone, 30 further right in the view");
            Feed(TurnedRight(10f));
            _view.PollAttitude();
            Assert.AreEqual(60f, _view.State.Yaw, 0.01f);

            Assert.IsFalse(_view.PollAttitude(), "a still phone asks for no new frame");
        }

        [Test]
        public void GyroLook_ThePhoneLeaningBack_LooksUp_AndThePinchIsStillTheVisitors()
        {
            AddSensor();
            Bind(BuiltInBlocks.Panorama360Gyro);
            Feed(Upright);
            _view.PollAttitude();
            Assert.AreEqual(0f, _view.State.Pitch, 0.01f);

            _view.Pinch(1.4f);
            float zoomed = _view.State.Fov;
            Assert.Less(zoomed, PanoramaViewState.DefaultFov, "two fingers still zoom the gyro view");
            Feed(Upright * Quaternion.AngleAxis(30f, Vector3.right));
            _view.PollAttitude();
            Assert.AreEqual(30f, _view.State.Pitch, 0.01f, "leaning the phone back looks up");
            Assert.AreEqual(zoomed, _view.State.Fov, 1e-4f, "and the sensor never touches the zoom");
        }

        [Test]
        public void GyroLook_WithASensor_ADragDoesNothing_ThePhoneDecides()
        {
            AddSensor();
            Bind(BuiltInBlocks.Panorama360Gyro);
            Feed(Upright);
            _view.PollAttitude();
            var before = _view.State;
            _view.Drag(new Vector2(80f, 40f));
            Assert.AreEqual(before.Yaw, _view.State.Yaw, "a finger does not fight the phone");
            Assert.AreEqual(before.Pitch, _view.State.Pitch);
        }

        [Test]
        public void GyroLook_WithNoSensor_FallsBackToDragging_AndSaysSo()
        {
            Assert.IsFalse(DeviceAttitude.Available, "precondition: no attitude sensor in this Editor");
            Bind(BuiltInBlocks.Panorama360Gyro);
            Assert.IsFalse(_view.PollAttitude());
            Assert.IsFalse(_view.IsFollowingSensor);
            Assert.AreEqual(Words().Get(CardStrings.Keys.Panorama360DragHint), _view.Hint.text, "no sensor: the drag words, not 'move your phone'");

            _view.Drag(new Vector2(50f, 0f));
            Assert.AreNotEqual(0f, _view.State.Yaw, "where no sensor exists a drag turns the view");
        }

        [Test]
        public void GyroLook_TheHintSaysMoveYourPhone_OnlyWhileASensorIsThere_AndDragLookNeverReadsOne()
        {
            AddSensor();
            Bind(BuiltInBlocks.Panorama360Gyro);
            Assert.AreEqual(Words().Get(CardStrings.Keys.Panorama360GyroHint), _view.Hint.text, "a sensor is there: the gyro words");
            StringAssert.Contains("phone", _view.Hint.text);

            _view.Unbind();
            Bind(BuiltInBlocks.Panorama360Drag);
            Assert.IsFalse(_view.IsGyroLook);
            Assert.AreEqual(0, DeviceAttitude.Users, "the drag look never follows the sensor");
            Assert.IsFalse(_sensor.enabled, "and leaves it off");
            Assert.IsFalse(_view.PollAttitude());
            Assert.AreEqual(0f, _view.State.Yaw, "a sensor present changes nothing for the drag look");
            Assert.AreEqual(Words().Get(CardStrings.Keys.Panorama360DragHint), _view.Hint.text);
        }

        [Test]
        public void TheSensorDrawsPowerOnlyWhileAViewFollowsIt_TwoViewsShareIt_TheTeaserNeverFollows()
        {
            AddSensor();
            Bind(BuiltInBlocks.Panorama360Gyro);
            var second = new PanoramaBlockView();
            second.Bind(new BlockInstanceData { key = "block_2", kind = BuiltInBlocks.Panorama360Kind, variant = BuiltInBlocks.Panorama360Gyro, fields = { new BlockFieldValue { key = BuiltInBlocks.Panorama360PanoramaField, asset = "default:tiled_room_360" } } },
                new BlockBindContext { Poi = new POIData { id = "p" }, Language = "en", FallbackLanguage = "en", Variant = BuiltInBlocks.Panorama360Gyro, Preview = _service });
            Assert.AreEqual(2, DeviceAttitude.Users);
            Assert.IsTrue(_sensor.enabled);

            second.Unbind();
            Assert.AreEqual(1, DeviceAttitude.Users);
            Assert.IsTrue(_sensor.enabled, "one view still follows it");
            _view.Unbind();
            Assert.AreEqual(0, DeviceAttitude.Users);
            Assert.IsFalse(_sensor.enabled, "the last one to let go switches it off");

            Bind(BuiltInBlocks.Panorama360Gyro, CardOptions.DisplayTakeover);
            Assert.AreEqual(0, DeviceAttitude.Users, "the Display Takeover teaser is a static picture: it never follows the sensor");
            Assert.IsFalse(_sensor.enabled);
        }

        [Test]
        public void TheStartHeading_IsTheFirstViewOfEveryLook_AFullTurnIsTheSameHeadingAsNone()
        {
            Bind(BuiltInBlocks.Panorama360Drag, startHeading: 90f);
            Assert.AreEqual(90f, _view.State.Yaw, 0.01f, "the drag look opens at the authored heading");
            Assert.AreEqual(0f, _view.State.Pitch);
            Assert.AreEqual(PanoramaViewState.DefaultFov, _view.State.Fov);

            Bind(BuiltInBlocks.Panorama360Drag, startHeading: 360f);
            Assert.AreEqual(0f, _view.State.Yaw, 0.01f, "360 (the field's top) is the picture's middle again, never a seam");
        }

        [Test]
        public void ADragLook_ADragRightTurnsTheViewLeft_DownLooksUp_AndThePinchStaysInsideItsLimits()
        {
            Bind(BuiltInBlocks.Panorama360Drag);
            // - an unparented view has no layout, so its stage height counts as one unit: one unit of drag is one field of view
            float perUnit = PanoramaViewRule.DegreesPerUnit(PanoramaViewState.DefaultFov, float.NaN);
            _view.Drag(new Vector2(1f, 0f));
            Assert.AreEqual(360f - perUnit, _view.State.Yaw, 0.01f, "dragging right turns the view left: yaw 0 wraps to just under 360");
            _view.Drag(new Vector2(0f, 1f));
            Assert.AreEqual(Mathf.Min(perUnit, PanoramaViewRule.MaxPitch), _view.State.Pitch, 0.01f, "dragging down looks up: the scene follows the finger");

            _view.Pinch(1000f);
            Assert.AreEqual(PanoramaViewRule.MinFov, _view.State.Fov, 1e-3f);
            _view.Pinch(0.0001f);
            Assert.AreEqual(PanoramaViewRule.MaxFov, _view.State.Fov, 1e-3f);
        }
    }
}
