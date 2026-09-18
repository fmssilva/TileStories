using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Tests
{
    // Pure Tier-0 tests for MarkerOrientationResolver (_2.1_Marker_Orientation.md Block 2).
    // No scene, no MonoBehaviour -- every call is a plain static function.
    public class MarkerOrientationResolverTests
    {
        private static GameObject CreateCamera(Vector3 pos, Quaternion rot)
        {
            var go = new GameObject("TestCam");
            var cam = go.AddComponent<Camera>();
            cam.transform.SetPositionAndRotation(pos, rot);
            return go;
        }

        [Test]
        public void ScreenUpWorld_UnrotatedCamera_EqualsCameraUp()
        {
            var go = CreateCamera(Vector3.zero, Quaternion.identity);
            var cam = go.GetComponent<Camera>();

            Vector3 result = MarkerOrientationResolver.ScreenUpWorld(cam);

            Assert.Less(Vector3.Angle(result, cam.transform.up), 0.5f);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void ResolveRootRotation_ScreenAligned_MarkerUpMatchesScreenUp()
        {
            var go = CreateCamera(new Vector3(0, 0, -2), Quaternion.Euler(10f, 20f, 33f));
            var cam = go.GetComponent<Camera>();
            var settings = new OrientationSettings { marker_orientation_mode = "screen_aligned" };
            Vector3 screenUp = MarkerOrientationResolver.ScreenUpWorld(cam);

            var result = MarkerOrientationResolver.ResolveRootRotation(
                settings, "", Vector3.zero, cam.transform.position, cam.transform.forward,
                screenUp, Vector3.up, Quaternion.identity, Quaternion.identity, 0f, ScreenOrientation.Portrait);

            Assert.IsTrue(result.Resolved);
            Assert.Less(Vector3.Angle(result.Rotation * Vector3.up, screenUp), 0.5f);
            Object.DestroyImmediate(go);
        }

        [TestCase(0f)]
        [TestCase(45f)]
        [TestCase(90f)]
        public void ResolveRootRotation_WorldUp_MarkerUpMatchesGravity_RegardlessOfCameraRoll(float rollDeg)
        {
            var go = CreateCamera(new Vector3(0, 0, -2), Quaternion.Euler(0f, 0f, rollDeg));
            var cam = go.GetComponent<Camera>();
            var settings = new OrientationSettings { marker_orientation_mode = "world_up" };
            Vector3 screenUp = MarkerOrientationResolver.ScreenUpWorld(cam);

            var result = MarkerOrientationResolver.ResolveRootRotation(
                settings, "", Vector3.zero, cam.transform.position, cam.transform.forward,
                screenUp, Vector3.up, Quaternion.identity, Quaternion.identity, 0f, ScreenOrientation.Portrait);

            Assert.IsTrue(result.Resolved);
            Assert.Less(Vector3.Angle(result.Rotation * Vector3.up, Vector3.up), 0.5f);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void ResolveRootRotation_CameraPositionBasis_ForwardPointsAtCamera()
        {
            var camPos = new Vector3(3, 1, -2);
            var go = CreateCamera(camPos, Quaternion.identity);
            var cam = go.GetComponent<Camera>();
            var settings = new OrientationSettings { marker_orientation_mode = "screen_aligned", facing_basis = "camera_position" };
            Vector3 markerPos = Vector3.zero;
            Vector3 screenUp = MarkerOrientationResolver.ScreenUpWorld(cam);

            var result = MarkerOrientationResolver.ResolveRootRotation(
                settings, "", markerPos, camPos, cam.transform.forward,
                screenUp, Vector3.up, Quaternion.identity, Quaternion.identity, 0f, ScreenOrientation.Portrait);

            // Same forward sign convention as view_plane's camForward (points from camera
            // into the scene), just computed per-marker instead of shared across all markers.
            Vector3 expectedFwd = (markerPos - camPos).normalized;
            Assert.Less(Vector3.Angle(result.Rotation * Vector3.forward, expectedFwd), 0.5f);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void ResolveRootRotation_YawOnly_HasNoPitchComponent()
        {
            var camPos = new Vector3(0, 5, -2); // camera well above the marker
            var go = CreateCamera(camPos, Quaternion.identity);
            var cam = go.GetComponent<Camera>();
            var settings = new OrientationSettings { marker_orientation_mode = "yaw_only" };
            Vector3 markerPos = Vector3.zero;
            Vector3 screenUp = MarkerOrientationResolver.ScreenUpWorld(cam);

            var result = MarkerOrientationResolver.ResolveRootRotation(
                settings, "", markerPos, camPos, cam.transform.forward,
                screenUp, Vector3.up, Quaternion.identity, Quaternion.identity, 0f, ScreenOrientation.Portrait);

            Assert.IsTrue(result.Resolved);
            Vector3 fwd = result.Rotation * Vector3.forward;
            Assert.Less(Mathf.Abs(fwd.y), 0.01f, "yaw_only forward must stay in the horizontal plane");
            Object.DestroyImmediate(go);
        }

        [Test]
        public void ResolveRootRotation_WallFixed_ReturnsParentTimesAuthored()
        {
            var settings = new OrientationSettings { marker_orientation_mode = "wall_fixed" };
            Quaternion parent = Quaternion.Euler(0f, 30f, 0f);
            Quaternion authored = Quaternion.Euler(10f, 0f, 5f);

            var result = MarkerOrientationResolver.ResolveRootRotation(
                settings, "", Vector3.zero, Vector3.zero, Vector3.forward,
                Vector3.up, Vector3.up, parent, authored, 0f, ScreenOrientation.Portrait);

            Assert.IsTrue(result.Resolved);
            Assert.Less(Quaternion.Angle(result.Rotation, parent * authored), 0.01f);
        }

        [Test]
        public void ResolveRootRotation_None_ReturnsParentRotation()
        {
            var settings = new OrientationSettings { marker_orientation_mode = "none" };
            Quaternion parent = Quaternion.Euler(5f, 10f, 15f);

            var result = MarkerOrientationResolver.ResolveRootRotation(
                settings, "", Vector3.zero, Vector3.zero, Vector3.forward,
                Vector3.up, Vector3.up, parent, Quaternion.identity, 0f, ScreenOrientation.Portrait);

            Assert.IsTrue(result.Resolved);
            Assert.Less(Quaternion.Angle(result.Rotation, parent), 0.01f);
        }

        [Test]
        public void ResolveRootRotation_ModeOverride_BeatsWallSetting()
        {
            var settings = new OrientationSettings { marker_orientation_mode = "screen_aligned" };
            Quaternion parent = Quaternion.Euler(5f, 10f, 15f);

            var result = MarkerOrientationResolver.ResolveRootRotation(
                settings, "none", Vector3.zero, Vector3.zero, Vector3.forward,
                Vector3.up, Vector3.up, parent, Quaternion.identity, 0f, ScreenOrientation.Portrait);

            Assert.IsTrue(result.Resolved);
            Assert.Less(Quaternion.Angle(result.Rotation, parent), 0.01f);
        }

        [Test]
        public void ResolveRootRotation_DegenerateLookDirection_ReturnsNotResolved()
        {
            // Camera directly above a world_up marker: forward is parallel to the up reference.
            var camPos = new Vector3(0, 5, 0);
            var settings = new OrientationSettings { marker_orientation_mode = "world_up" };
            Vector3 markerPos = Vector3.zero;
            Vector3 camForward = (markerPos - camPos).normalized; // straight down

            var result = MarkerOrientationResolver.ResolveRootRotation(
                settings, "", markerPos, camPos, camForward,
                Vector3.up, Vector3.up, Quaternion.identity, Quaternion.identity, 0f, ScreenOrientation.Portrait);

            Assert.IsFalse(result.Resolved);
        }

        [Test]
        public void ResolveChildLocalRotation_Inherit_IsIdentity()
        {
            var result = MarkerOrientationResolver.ResolveChildLocalRotation("inherit", Quaternion.Euler(10, 20, 30), Vector3.up, Vector3.up);

            Assert.AreEqual(Quaternion.identity, result);
        }

        [Test]
        public void ResolveChildLocalRotation_WorldUp_CancelsRootRoll()
        {
            Quaternion rootRotation = Quaternion.LookRotation(Vector3.forward, Vector3.up) * Quaternion.Euler(0, 0, 40f);
            Vector3 screenUp = Vector3.up;
            Vector3 upReference = Vector3.up;

            Quaternion childLocal = MarkerOrientationResolver.ResolveChildLocalRotation("world_up", rootRotation, screenUp, upReference);
            Quaternion composed = rootRotation * childLocal;

            Assert.Less(Vector3.Angle(composed * Vector3.up, Vector3.up), 0.5f);
        }

        [Test]
        public void ResolveChildLocalRotation_IsPureZ()
        {
            Quaternion rootRotation = Quaternion.Euler(15f, 25f, 35f);
            Quaternion result = MarkerOrientationResolver.ResolveChildLocalRotation("screen_up", rootRotation, Vector3.up, Vector3.up);

            Assert.AreEqual(0f, result.eulerAngles.x, 0.01f);
            Assert.AreEqual(0f, result.eulerAngles.y, 0.01f);
        }

        [Test]
        public void RootRollDeg_UnrolledRoot_IsZero()
        {
            Quaternion root = Quaternion.LookRotation(Vector3.forward, Vector3.up);
            float roll = MarkerOrientationResolver.RootRollDeg(root, Vector3.up);

            Assert.AreEqual(0f, roll, 0.01f);
        }

        [Test]
        public void Rotate2D_NinetyDegrees_MapsXToY()
        {
            Vector2 result = MarkerOrientationResolver.Rotate2D(new Vector2(1f, 0f), 90f);

            Assert.AreEqual(0f, result.x, 0.001f);
            Assert.AreEqual(1f, result.y, 0.001f);
        }

        [Test]
        public void SnapRollDeg_QuarterTurns_SnapsToNearest90()
        {
            float result = MarkerOrientationResolver.SnapRollDeg(100f, "quarter_turns", 15f, 0f, ScreenOrientation.AutoRotation);

            // 100 is far enough from previous snap (0) to cross the hysteresis boundary.
            Assert.AreEqual(90f, result, 0.01f);
        }

        [Test]
        public void SnapRollDeg_WithinHysteresis_KeepsPreviousValue()
        {
            // 50 degrees is close to the 0/90 boundary (45) but within the 15-degree
            // hysteresis band around the previous snapped value (0) -- must not flicker.
            float result = MarkerOrientationResolver.SnapRollDeg(50f, "quarter_turns", 15f, 0f, ScreenOrientation.AutoRotation);

            Assert.AreEqual(0f, result, 0.01f);
        }

        [Test]
        public void SnapRollDeg_ScreenOrientationMode_LandscapeLeftGives90()
        {
            float result = MarkerOrientationResolver.SnapRollDeg(5f, "screen_orientation", 15f, 0f, ScreenOrientation.LandscapeLeft);

            Assert.AreEqual(90f, result, 0.01f);
        }

        [Test]
        public void SnapRollDeg_ScreenOrientationMode_AutoRotationFallsBackToMeasuredRoll()
        {
            float result = MarkerOrientationResolver.SnapRollDeg(100f, "screen_orientation", 15f, 0f, ScreenOrientation.AutoRotation);

            Assert.AreEqual(90f, result, 0.01f);
        }

        [Test]
        public void ClampPitch_BeyondMax_ClampsToMax()
        {
            // Looking almost straight up: pitch far beyond a 75-degree max.
            Quaternion steep = Quaternion.LookRotation(new Vector3(0f, 0.99f, 0.01f).normalized, Vector3.up);

            Quaternion clamped = MarkerOrientationResolver.ClampPitch(steep, Vector3.up, 75f);

            Vector3 flatFwd = Vector3.ProjectOnPlane(clamped * Vector3.forward, Vector3.up).normalized;
            float pitchDeg = Vector3.Angle(flatFwd, clamped * Vector3.forward);
            Assert.LessOrEqual(pitchDeg, 75.1f);
        }

        [Test]
        public void ShouldUpdate_IntervalMode_FalseBeforeInterval_TrueAfter()
        {
            var settings = new OrientationSettings { update_mode = "interval", update_interval_s = 0.05f };

            Assert.IsFalse(MarkerOrientationResolver.ShouldUpdate(settings, 0.02f, 0f));
            Assert.IsTrue(MarkerOrientationResolver.ShouldUpdate(settings, 0.06f, 0f));
        }

        [Test]
        public void ShouldUpdate_CameraDeltaMode_FalseBelowThreshold()
        {
            var settings = new OrientationSettings { update_mode = "on_camera_delta", camera_delta_deg = 0.5f };

            Assert.IsFalse(MarkerOrientationResolver.ShouldUpdate(settings, 0f, 0.1f));
            Assert.IsTrue(MarkerOrientationResolver.ShouldUpdate(settings, 0f, 1f));
        }

        [Test]
        public void ResolveUpReference_SpawnRoot_UsesTransformUp()
        {
            var go = new GameObject("SpawnRoot");
            go.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            var settings = new OrientationSettings { up_reference = "spawn_root" };

            Vector3 result = MarkerOrientationResolver.ResolveUpReference(settings, go.transform);

            Assert.Less(Vector3.Angle(result, go.transform.up), 0.5f);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void OrientationSettings_CopyConstructor_CopiesEveryField()
        {
            var original = new OrientationSettings();
            var fields = typeof(OrientationSettings).GetFields(BindingFlags.Public | BindingFlags.Instance);

            // Mutate every field to a value guaranteed different from its default, so a
            // field the copy constructor forgot shows up as a still-default value below.
            foreach (var field in fields)
            {
                if (field.FieldType == typeof(string)) field.SetValue(original, "changed_" + field.Name);
                else if (field.FieldType == typeof(float)) field.SetValue(original, 12345.6f);
                else if (field.FieldType == typeof(bool)) field.SetValue(original, !(bool)field.GetValue(original));
            }

            var copy = new OrientationSettings(original);

            foreach (var field in fields)
            {
                Assert.AreEqual(field.GetValue(original), field.GetValue(copy), $"Field '{field.Name}' was not copied by the copy constructor.");
            }
        }
    }
}
