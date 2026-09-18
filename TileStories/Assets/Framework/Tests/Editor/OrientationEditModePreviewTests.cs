using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using TileStories;
using TileStories.Editor;

namespace TileStories.Tests
{
    // Tier 0 EditMode tests for the Block 7 orientation preview
    // (_2.1_Marker_Orientation.md section 14): the non-destructive-preview guarantee and
    // the wall_fixed Edit-Mode/Play-Mode rotation agreement. Follows this file's own
    // established idiom (see POIHeaderRenameTests) of building the window via
    // ScriptableObject.CreateInstance rather than EditorWindow.GetWindow, so nothing
    // leaks into the user's real editor layout.
    public class OrientationEditModePreviewTests
    {
        private POIEditorToolWindow _window;
        private GameObject _rigGO;
        private GameObject _childA;
        private GameObject _childB;
        private GameObject _camGO;

        [SetUp]
        public void SetUp()
        {
            _window = ScriptableObject.CreateInstance<POIEditorToolWindow>();
            _rigGO = new GameObject("POIEditorRig");
            _childA = new GameObject("poi_a");
            _childA.transform.SetParent(_rigGO.transform);
            _childB = new GameObject("poi_b");
            _childB.transform.SetParent(_rigGO.transform);
            _camGO = new GameObject("PreviewCam", typeof(Camera));
        }

        [TearDown]
        public void TearDown()
        {
            if (_window != null) Object.DestroyImmediate(_window);
            if (_rigGO != null) Object.DestroyImmediate(_rigGO);
            if (_camGO != null) Object.DestroyImmediate(_camGO);
        }

        private static WallConfigData BuildConfig(out POIData poiA, out POIData poiB)
        {
            poiA = new POIData { id = "poi_a", editor_rotation_x_deg = 10f, editor_rotation_deg = 20f, editor_rotation_z_deg = 30f };
            poiB = new POIData { id = "poi_b", editor_rotation_x_deg = 5f, editor_rotation_deg = 15f, editor_rotation_z_deg = 25f };
            var config = new WallConfigData
            {
                wall_id = "test_wall",
                orientation_settings = new OrientationSettings { marker_orientation_mode = "world_up", edit_mode_preview_enabled = true }
            };
            config.pois.Add(poiA);
            config.pois.Add(poiB);
            return config;
        }

        private static void SetConfig(POIEditorToolWindow window, WallConfigData config)
        {
            typeof(POIEditorToolWindow).GetField("_config", BindingFlags.NonPublic | BindingFlags.Instance)!
                .SetValue(window, config);
        }

        [Test]
        public void ApplyOrientationPreview_RotatesRig_WithoutTouchingConfigAngles()
        {
            var config = BuildConfig(out var poiA, out var poiB);
            SetConfig(_window, config);

            Quaternion authoredA = PoiRotationResolver.ToEulerQuaternion(poiA.editor_rotation_x_deg, poiA.editor_rotation_deg, poiA.editor_rotation_z_deg);
            _childA.transform.localRotation = authoredA;
            _childB.transform.localRotation = PoiRotationResolver.ToEulerQuaternion(poiB.editor_rotation_x_deg, poiB.editor_rotation_deg, poiB.editor_rotation_z_deg);

            var cam = _camGO.GetComponent<Camera>();
            cam.transform.position = new Vector3(0f, 0f, -5f);
            cam.transform.rotation = Quaternion.Euler(0f, 0f, 45f); // roll, so world_up visibly differs from authored

            _window.ApplyOrientationPreview(cam);

            // world_up must have actually changed the rig's rotation away from the authored
            // angle (otherwise this test would pass vacuously).
            Assert.Greater(Quaternion.Angle(_childA.transform.rotation, authoredA), 1f,
                "world_up preview with a rolled camera must visibly differ from the authored rotation.");

            // The config's own authored angles must be completely untouched by the preview.
            Assert.AreEqual(10f, poiA.editor_rotation_x_deg);
            Assert.AreEqual(20f, poiA.editor_rotation_deg);
            Assert.AreEqual(30f, poiA.editor_rotation_z_deg);
            Assert.AreEqual(5f, poiB.editor_rotation_x_deg);
            Assert.AreEqual(15f, poiB.editor_rotation_deg);
            Assert.AreEqual(25f, poiB.editor_rotation_z_deg);
        }

        [Test]
        public void PreviewOnOffCycle_LeavesEveryEditorRotationValueByteIdentical()
        {
            var config = BuildConfig(out var poiA, out var poiB);
            SetConfig(_window, config);

            float ax0 = poiA.editor_rotation_x_deg, ay0 = poiA.editor_rotation_deg, az0 = poiA.editor_rotation_z_deg;
            float bx0 = poiB.editor_rotation_x_deg, by0 = poiB.editor_rotation_deg, bz0 = poiB.editor_rotation_z_deg;

            _childA.transform.localRotation = PoiRotationResolver.ToEulerQuaternion(ax0, ay0, az0);
            _childB.transform.localRotation = PoiRotationResolver.ToEulerQuaternion(bx0, by0, bz0);

            var cam = _camGO.GetComponent<Camera>();
            cam.transform.position = new Vector3(0f, 0f, -5f);
            cam.transform.rotation = Quaternion.Euler(10f, 20f, 45f);

            // ON: preview runs, rig rotates away from authored.
            _window.ApplyOrientationPreview(cam);
            Assert.Greater(Quaternion.Angle(_childA.transform.rotation, PoiRotationResolver.ToEulerQuaternion(ax0, ay0, az0)), 0.5f);

            // OFF: restore.
            _window.RestoreRigRotationsFromConfig();

            Assert.Less(Quaternion.Angle(_childA.transform.rotation, PoiRotationResolver.ToEulerQuaternion(ax0, ay0, az0)), 0.01f,
                "restoring must put the rig rotation back to exactly the authored angle.");
            Assert.Less(Quaternion.Angle(_childB.transform.rotation, PoiRotationResolver.ToEulerQuaternion(bx0, by0, bz0)), 0.01f);

            // The stored config angles were never written by either half of the cycle.
            Assert.AreEqual(ax0, poiA.editor_rotation_x_deg);
            Assert.AreEqual(ay0, poiA.editor_rotation_deg);
            Assert.AreEqual(az0, poiA.editor_rotation_z_deg);
            Assert.AreEqual(bx0, poiB.editor_rotation_x_deg);
            Assert.AreEqual(by0, poiB.editor_rotation_deg);
            Assert.AreEqual(bz0, poiB.editor_rotation_z_deg);
        }

        [Test]
        public void CapturePositions_WhilePreviewActive_DoesNotOverwriteAuthoredRotation()
        {
            var config = BuildConfig(out var poiA, out _);
            config.pois.RemoveAll(p => p.id != "poi_a"); // isolate to one POI for this check
            Object.DestroyImmediate(_childB);
            SetConfig(_window, config);

            _childA.transform.localRotation = PoiRotationResolver.ToEulerQuaternion(10f, 20f, 30f);

            var cam = _camGO.GetComponent<Camera>();
            cam.transform.position = new Vector3(0f, 0f, -5f);
            cam.transform.rotation = Quaternion.Euler(0f, 0f, 45f);
            _window.ApplyOrientationPreview(cam); // rig now shows the PREVIEW, not the authored angle

            var capturePositions = typeof(POIEditorToolWindow).GetMethod("CapturePositions", BindingFlags.NonPublic | BindingFlags.Instance);
            capturePositions!.Invoke(_window, new object[] { true });

            Assert.AreEqual(10f, poiA.editor_rotation_x_deg, "CapturePositions must not capture the previewed rotation as authored.");
            Assert.AreEqual(20f, poiA.editor_rotation_deg);
            Assert.AreEqual(30f, poiA.editor_rotation_z_deg);
        }

        [Test]
        public void ApplyOrientationPreview_WallFixed_MatchesResolveRootRotation_ForSameInputs()
        {
            var config = BuildConfig(out var poiA, out _);
            config.orientation_settings.marker_orientation_mode = "wall_fixed";
            SetConfig(_window, config);

            Quaternion authoredA = PoiRotationResolver.ToEulerQuaternion(poiA.editor_rotation_x_deg, poiA.editor_rotation_deg, poiA.editor_rotation_z_deg);
            _childA.transform.localRotation = authoredA;

            var cam = _camGO.GetComponent<Camera>();
            cam.transform.position = new Vector3(1f, 2f, -5f);
            cam.transform.rotation = Quaternion.Euler(15f, 30f, 45f); // must have zero effect in wall_fixed

            _window.ApplyOrientationPreview(cam);

            var expected = MarkerOrientationResolver.ResolveRootRotation(
                config.orientation_settings, "", _childA.transform.position, cam.transform.position, cam.transform.forward,
                MarkerOrientationResolver.ScreenUpWorld(cam), Vector3.up, _rigGO.transform.rotation, authoredA, 0f, ScreenOrientation.Portrait);

            Assert.Less(Quaternion.Angle(_childA.transform.rotation, expected.Rotation), 0.01f,
                "wall_fixed Edit-Mode preview must equal ResolveRootRotation's own result for the same inputs - Edit Mode and Play Mode agree exactly here.");
            Assert.Less(Quaternion.Angle(_childA.transform.rotation, authoredA), 0.01f,
                "wall_fixed must reproduce the authored rotation exactly (parentRotation is identity here).");
        }
    }
}
