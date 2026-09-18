using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Tests
{
    // Pure Tier-0 tests for MarkerOrientationResolver (_2.1_Marker_Orientation.md v4).
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
        public void ResolveRootRotation_ScreenUp_AlwaysFacingCamera_MarkerUpMatchesScreenUp()
        {
            var go = CreateCamera(new Vector3(0, 0, -2), Quaternion.Euler(10f, 20f, 33f));
            var cam = go.GetComponent<Camera>();
            var settings = new OrientationSettings { vertical_alignment_mode = "screen_up", facing_mode = "always_facing_camera" };
            Vector3 screenUp = MarkerOrientationResolver.ScreenUpWorld(cam);

            var result = MarkerOrientationResolver.ResolveRootRotation(
                settings, "", Vector3.zero, cam.transform.position, cam.transform.forward,
                screenUp, Vector3.up, Quaternion.identity, Quaternion.identity);

            Assert.IsTrue(result.Resolved);
            Assert.Less(Vector3.Angle(result.Rotation * Vector3.up, screenUp), 0.5f);
            Object.DestroyImmediate(go);
        }

        [TestCase(0f)]
        [TestCase(45f)]
        [TestCase(90f)]
        public void ResolveRootRotation_WorldUp_AlwaysFacingCamera_MarkerUpMatchesGravity_RegardlessOfCameraRoll(float rollDeg)
        {
            var go = CreateCamera(new Vector3(0, 0, -2), Quaternion.Euler(0f, 0f, rollDeg));
            var cam = go.GetComponent<Camera>();
            var settings = new OrientationSettings { vertical_alignment_mode = "world_up", facing_mode = "always_facing_camera" };
            Vector3 screenUp = MarkerOrientationResolver.ScreenUpWorld(cam);

            var result = MarkerOrientationResolver.ResolveRootRotation(
                settings, "", Vector3.zero, cam.transform.position, cam.transform.forward,
                screenUp, Vector3.up, Quaternion.identity, Quaternion.identity);

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
            var settings = new OrientationSettings { vertical_alignment_mode = "screen_up", facing_mode = "always_facing_camera", facing_basis = "camera_position" };
            Vector3 markerPos = Vector3.zero;
            Vector3 screenUp = MarkerOrientationResolver.ScreenUpWorld(cam);

            var result = MarkerOrientationResolver.ResolveRootRotation(
                settings, "", markerPos, camPos, cam.transform.forward,
                screenUp, Vector3.up, Quaternion.identity, Quaternion.identity);

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
            var settings = new OrientationSettings { vertical_alignment_mode = "world_up", facing_mode = "yaw_only" };
            Vector3 markerPos = Vector3.zero;
            Vector3 screenUp = MarkerOrientationResolver.ScreenUpWorld(cam);

            var result = MarkerOrientationResolver.ResolveRootRotation(
                settings, "", markerPos, camPos, cam.transform.forward,
                screenUp, Vector3.up, Quaternion.identity, Quaternion.identity);

            Assert.IsTrue(result.Resolved);
            Vector3 fwd = result.Rotation * Vector3.forward;
            Assert.Less(Mathf.Abs(fwd.y), 0.01f, "yaw_only with zero authored tilt must stay in the horizontal plane");
        }

        [Test]
        public void ResolveRootRotation_YawOnly_KeepsAuthoredXZTilt_RegardlessOfCameraRoll()
        {
            var camPos = new Vector3(2, 0, -2);
            var go = CreateCamera(camPos, Quaternion.Euler(0f, 0f, 60f)); // roll must not affect yaw_only at all
            var cam = go.GetComponent<Camera>();
            var settings = new OrientationSettings { vertical_alignment_mode = "world_up", facing_mode = "yaw_only" };
            Quaternion authored = Quaternion.Euler(15f, 0f, 8f); // authored wall tilt on X/Z

            var result = MarkerOrientationResolver.ResolveRootRotation(
                settings, "", Vector3.zero, camPos, cam.transform.forward,
                Vector3.up, Vector3.up, Quaternion.identity, authored);

            Assert.IsTrue(result.Resolved);
            Vector3 authoredEuler = authored.eulerAngles;
            Vector3 resultEuler = result.Rotation.eulerAngles;
            Assert.AreEqual(authoredEuler.x, resultEuler.x, 0.01f, "yaw_only must preserve the authored X tilt exactly");
            Assert.AreEqual(authoredEuler.z, resultEuler.z, 0.01f, "yaw_only must preserve the authored Z tilt exactly");
            Object.DestroyImmediate(go);
        }

        [Test]
        public void ResolveRootRotation_WallFixed_ReturnsParentTimesAuthored()
        {
            var settings = new OrientationSettings { facing_mode = "wall_fixed" };
            Quaternion parent = Quaternion.Euler(0f, 30f, 0f);
            Quaternion authored = Quaternion.Euler(10f, 0f, 5f);

            var result = MarkerOrientationResolver.ResolveRootRotation(
                settings, "", Vector3.zero, Vector3.zero, Vector3.forward,
                Vector3.up, Vector3.up, parent, authored);

            Assert.IsTrue(result.Resolved);
            Assert.Less(Quaternion.Angle(result.Rotation, parent * authored), 0.01f);
        }

        [Test]
        public void ResolveRootRotation_WallFixed_UnaffectedByCameraPose()
        {
            var settings = new OrientationSettings { facing_mode = "wall_fixed" };
            Quaternion authored = Quaternion.Euler(10f, 0f, 5f);

            var resultA = MarkerOrientationResolver.ResolveRootRotation(
                settings, "", Vector3.zero, new Vector3(0, 0, -2), Vector3.forward,
                Vector3.up, Vector3.up, Quaternion.identity, authored);
            var resultB = MarkerOrientationResolver.ResolveRootRotation(
                settings, "", Vector3.zero, new Vector3(9, 4, 3), new Vector3(0.5f, 0.5f, 0.5f).normalized,
                Vector3.up, Vector3.up, Quaternion.identity, authored);

            Assert.Less(Quaternion.Angle(resultA.Rotation, resultB.Rotation), 0.01f);
        }

        [Test]
        public void ResolveRootRotation_ModeOverride_BeatsWallSetting()
        {
            var settings = new OrientationSettings { facing_mode = "always_facing_camera" };
            Quaternion parent = Quaternion.Euler(5f, 10f, 15f);
            Quaternion authored = Quaternion.identity;

            var result = MarkerOrientationResolver.ResolveRootRotation(
                settings, "wall_fixed", Vector3.zero, Vector3.zero, Vector3.forward,
                Vector3.up, Vector3.up, parent, authored);

            Assert.IsTrue(result.Resolved);
            Assert.Less(Quaternion.Angle(result.Rotation, parent * authored), 0.01f);
        }

        [Test]
        public void ResolveRootRotation_DegenerateLookDirection_ReturnsNotResolved()
        {
            // Camera directly above a world_up marker: forward is parallel to the up reference.
            var camPos = new Vector3(0, 5, 0);
            var settings = new OrientationSettings { vertical_alignment_mode = "world_up", facing_mode = "always_facing_camera" };
            Vector3 markerPos = Vector3.zero;
            Vector3 camForward = (markerPos - camPos).normalized; // straight down

            var result = MarkerOrientationResolver.ResolveRootRotation(
                settings, "", markerPos, camPos, camForward,
                Vector3.up, Vector3.up, Quaternion.identity, Quaternion.identity);

            Assert.IsFalse(result.Resolved);
        }

        [Test]
        public void ResolveRootRotation_AlwaysFacingCamera_NearCameraDoesNotFlipEdgeOn()
        {
            // Visitor standing close to and mostly below a marker: a raw LookRotation would
            // tip the marker to an ~85-degree pitch (steep, but NOT the near-parallel case
            // ResolveRootRotation_DegenerateLookDirection_ReturnsNotResolved already covers -
            // that guard triggers past ~87.4 degrees (dot > 0.999); this case must stay
            // resolved and instead get caught by the hardcoded safety clamp).
            var settings = new OrientationSettings { vertical_alignment_mode = "world_up", facing_mode = "always_facing_camera" };
            var camPos = new Vector3(0.26f, -3f, 0f);

            var result = MarkerOrientationResolver.ResolveRootRotation(
                settings, "", Vector3.zero, camPos, (Vector3.zero - camPos).normalized,
                Vector3.up, Vector3.up, Quaternion.identity, Quaternion.identity);

            Assert.IsTrue(result.Resolved, "an ~85-degree pitch is steep but not degenerate - it must still resolve");
            Vector3 fwd = result.Rotation * Vector3.forward;
            float pitchFromHorizontal = Vector3.Angle(Vector3.ProjectOnPlane(fwd, Vector3.up), fwd);
            Assert.LessOrEqual(pitchFromHorizontal, 80.5f, "the hardcoded pitch clamp must stop the marker tipping fully edge-on");
        }

        [Test]
        public void ResolveVerticalUp_ScreenUp_ReturnsScreenUpWorld()
        {
            Vector3 screenUp = new Vector3(0.1f, 0.9f, 0f).normalized;
            Vector3 upReference = Vector3.up;

            Vector3 result = MarkerOrientationResolver.ResolveVerticalUp("screen_up", screenUp, upReference);

            Assert.Less(Vector3.Angle(result, screenUp), 0.01f);
        }

        [Test]
        public void ResolveVerticalUp_WorldUp_ReturnsUpReference()
        {
            Vector3 screenUp = new Vector3(0.1f, 0.9f, 0f).normalized;
            Vector3 upReference = new Vector3(0f, 0.7f, 0.7f).normalized;

            Vector3 result = MarkerOrientationResolver.ResolveVerticalUp("world_up", screenUp, upReference);

            Assert.Less(Vector3.Angle(result, upReference), 0.01f);
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
