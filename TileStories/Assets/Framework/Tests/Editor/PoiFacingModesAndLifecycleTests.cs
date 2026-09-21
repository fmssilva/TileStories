using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using TileStories.Editor;

namespace TileStories.Tests
{
    // Real-code EditMode tests for the facing feature's end-to-end behaviour:
    //  - per facing mode (wall_fixed / yaw_only / always_facing_camera), what each of the
    //    X/Y/Z authored angles really does to a REAL POI_Marker prefab instance under the
    //    Edit-Mode preview (the pipeline the Scene view shows),
    //  - the preview warnings that tell the developer when an axis is overridden,
    //  - the "unverified in the window but the Scene tool says verified" wiring: the config
    //    the live handler reads, and the leaked scene-GUI handler of a closed window,
    //  - auto-reveal, including when the edit is blocked by the Verified lock.
    // No mocks: real window instance, real prefab, real rig, real Selection, real resolver.
    public class PoiFacingModesAndLifecycleTests
    {
        private const string PrefabPath = "Assets/Framework/Runtime/UI/Markers/POI_Marker.prefab";
        private const string PoiId = "poi_facing";

        private POIEditorToolWindow _window;
        private GameObject _rigGO;
        private Transform _marker;
        private GameObject _camGO;
        private Camera _cam;
        private WallConfigData _config;
        private POIData _poi;
        private GameObject _previousSelection;
        private bool _facingWarningWasHidden;

        [SetUp]
        public void SetUp()
        {
            _previousSelection = Selection.activeGameObject;
            // If the developer chose "Don't show again" for the preview warning, these tests
            // must still see it: unhide for the test, restore their choice afterwards.
            _facingWarningWasHidden = EditorNotice.IsHidden(NoticeKeys.FacingPreviewWarning);
            EditorNotice.Unhide(NoticeKeys.FacingPreviewWarning);
            _window = ScriptableObject.CreateInstance<POIEditorToolWindow>();

            _rigGO = new GameObject("POIEditorRig");
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            Assert.IsNotNull(prefab, "The real marker prefab must load: " + PrefabPath);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, _rigGO.transform);
            instance.name = PoiId;
            instance.transform.localPosition = new Vector3(2f, 0f, 0f);
            _marker = instance.transform;

            _camGO = new GameObject("FacingTestCam", typeof(Camera));
            _camGO.transform.position = new Vector3(0f, 1f, -6f);
            _cam = _camGO.GetComponent<Camera>();

            _poi = new POIData
            {
                id = PoiId,
                position = new PositionData { x = 2f, y = 0f, z = 0f },
                position_verified = false
            };
            _config = new WallConfigData
            {
                wall_id = "test_wall",
                orientation_settings = new OrientationSettings
                {
                    vertical_alignment_mode = "world_up",
                    facing_mode = "wall_fixed",
                    edit_mode_preview_enabled = true
                }
            };
            _config.pois.Add(_poi);
            SetPrivate("_config", _config);
        }

        [TearDown]
        public void TearDown()
        {
            MarkerHierarchyResolver.ResetToDefaults();
            EditorNotice.Clear();
            if (_facingWarningWasHidden) EditorUtility.SetDialogOptOutDecision(DialogOptOutDecisionType.ForThisMachine, NoticeKeys.FacingPreviewWarning, true);
            Selection.activeGameObject = _previousSelection;
            if (_window != null) UnityEngine.Object.DestroyImmediate(_window);
            if (_rigGO != null) UnityEngine.Object.DestroyImmediate(_rigGO);
            if (_camGO != null) UnityEngine.Object.DestroyImmediate(_camGO);
        }

        // ---------- helpers ----------

        private void SetPrivate(string field, object value)
        {
            var f = typeof(POIEditorToolWindow).GetField(field, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(f, field);
            f.SetValue(_window, value);
        }

        private T GetPrivate<T>(string field)
        {
            var f = typeof(POIEditorToolWindow).GetField(field, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(f, field);
            return (T)f.GetValue(_window);
        }

        // Mimic the Facing sliders: write the three angles, then the same call the slider
        // path makes to show them on the rig child.
        private void DragSliders(float x, float y, float z)
        {
            _poi.editor_rotation_x_deg = x;
            _poi.editor_rotation_deg = y;
            _poi.editor_rotation_z_deg = z;
            var m = typeof(POIEditorToolWindow).GetMethod("ApplyPoiEditorRotation", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(m);
            m.Invoke(_window, new object[] { _poi });
        }

        // What the Scene view shows for these angles in this mode: slider write + one real
        // preview repaint on the real prefab instance.
        private Quaternion PreviewFor(string globalMode, float x, float y, float z)
        {
            _config.orientation_settings.facing_mode = globalMode;
            DragSliders(x, y, z);
            _window.ApplyOrientationPreview(_cam);
            return _marker.rotation;
        }

        private static void AssertSameRotation(Quaternion a, Quaternion b, string why)
        {
            Assert.That(Quaternion.Angle(a, b), Is.LessThan(0.05f), why);
        }

        private static void AssertDifferentRotation(Quaternion a, Quaternion b, string why)
        {
            Assert.That(Quaternion.Angle(a, b), Is.GreaterThan(1f), why);
        }

        // ---------- (b) each axis, each mode, on the real prefab ----------

        [Test]
        public void WallFixed_EveryAxisChangesTheRealMarker_ExactlyAsAuthored()
        {
            Quaternion identity = PreviewFor("wall_fixed", 0f, 0f, 0f);
            AssertSameRotation(identity, Quaternion.identity, "Zero facing = the prefab's own orientation.");

            Quaternion x = PreviewFor("wall_fixed", 30f, 0f, 0f);
            AssertSameRotation(x, Quaternion.Euler(30f, 0f, 0f), "X must reach the marker.");

            Quaternion y = PreviewFor("wall_fixed", 0f, 75f, 0f);
            AssertSameRotation(y, Quaternion.Euler(0f, 75f, 0f), "Y must reach the marker in wall_fixed.");

            Quaternion z = PreviewFor("wall_fixed", 0f, 0f, 40f);
            AssertSameRotation(z, Quaternion.Euler(0f, 0f, 40f), "Z must reach the marker in wall_fixed.");

            Quaternion all = PreviewFor("wall_fixed", 20f, 50f, 70f);
            AssertSameRotation(all, Quaternion.Euler(20f, 50f, 70f), "All three combine.");

            // What a human sees: the marker's facing normal (forward) and its up vector.
            PreviewFor("wall_fixed", 0f, 90f, 0f);
            Assert.That(Vector3.Distance(_marker.forward, Vector3.right), Is.LessThan(1e-3f), "Y=90 turns the marker to face +X.");
            PreviewFor("wall_fixed", 90f, 0f, 0f);
            Assert.That(Vector3.Distance(_marker.forward, Vector3.down), Is.LessThan(1e-3f), "X=90 tips it to face down.");
            PreviewFor("wall_fixed", 0f, 0f, 90f);
            Assert.That(Vector3.Distance(_marker.up, Vector3.left), Is.LessThan(1e-3f), "Z=90 rolls it in place.");
            Assert.That(Vector3.Distance(_marker.forward, Vector3.forward), Is.LessThan(1e-3f), "...without changing what it faces.");
        }

        [Test]
        public void YawOnly_XAndZApply_YIsReplacedByLiveCameraYaw()
        {
            Quaternion baseline = PreviewFor("yaw_only", 0f, 0f, 0f);

            Quaternion x = PreviewFor("yaw_only", 30f, 0f, 0f);
            Assert.That(Quaternion.Angle(baseline, x), Is.EqualTo(30f).Within(0.1f), "X tilt applies (30 degrees).");

            Quaternion z = PreviewFor("yaw_only", 0f, 0f, 40f);
            Assert.That(Quaternion.Angle(baseline, z), Is.EqualTo(40f).Within(0.1f), "Z roll applies (40 degrees).");

            Quaternion y1 = PreviewFor("yaw_only", 0f, 10f, 0f);
            Quaternion y2 = PreviewFor("yaw_only", 0f, 200f, 0f);
            AssertSameRotation(y1, baseline, "Authored Y must be ignored in yaw_only.");
            AssertSameRotation(y2, baseline, "Authored Y must be ignored in yaw_only (any value).");

            // The yaw it does use is the live one: it changes when the camera moves.
            _camGO.transform.position = new Vector3(6f, 1f, 0f);
            _window.ApplyOrientationPreview(_cam);
            AssertDifferentRotation(_marker.rotation, baseline, "yaw_only follows the camera.");
        }

        [Test]
        public void AlwaysFacingCamera_NoAuthoredAxisReachesTheMarker()
        {
            Quaternion baseline = PreviewFor("always_facing_camera", 0f, 0f, 0f);
            AssertSameRotation(PreviewFor("always_facing_camera", 30f, 0f, 0f), baseline, "X ignored.");
            AssertSameRotation(PreviewFor("always_facing_camera", 0f, 75f, 0f), baseline, "Y ignored.");
            AssertSameRotation(PreviewFor("always_facing_camera", 0f, 0f, 40f), baseline, "Z ignored.");
            AssertSameRotation(PreviewFor("always_facing_camera", 20f, 50f, 70f), baseline, "All ignored.");
        }

        [Test]
        public void HierarchyLevelOverride_WinsOverGlobalMode_InThePreview()
        {
            MarkerHierarchyResolver.Configure(new List<HierarchyLevelEntry>
            {
                new HierarchyLevelEntry { key = "flat_level", facing_mode_override = "wall_fixed" }
            });
            _poi.hierarchy_level_key = "flat_level";

            // Global says always_facing_camera, but this POI's level says wall_fixed.
            Quaternion y = PreviewFor("always_facing_camera", 0f, 75f, 0f);
            AssertSameRotation(y, Quaternion.Euler(0f, 75f, 0f), "The level override (wall_fixed) must make Y visible.");
        }

        // ---------- (a) warnings when an axis is overridden by the preview ----------

        [Test]
        public void Advice_PreviewOff_NeverWarns_InAnyModeOrAxis()
        {
            foreach (string mode in new[] { "wall_fixed", "yaw_only", "always_facing_camera" })
            {
                var s = new OrientationSettings { facing_mode = mode, edit_mode_preview_enabled = false };
                Assert.IsNull(FacingEditAdvice.GetSliderWarning(s, null, true, true, true), mode);
                Assert.IsNull(FacingEditAdvice.GetGizmoWarning(s, null), mode);
            }
        }

        [Test]
        public void Advice_PreviewOn_WallFixed_NeverWarns()
        {
            var s = new OrientationSettings { facing_mode = "wall_fixed", edit_mode_preview_enabled = true };
            Assert.IsNull(FacingEditAdvice.GetSliderWarning(s, null, true, false, false));
            Assert.IsNull(FacingEditAdvice.GetSliderWarning(s, null, false, true, false));
            Assert.IsNull(FacingEditAdvice.GetSliderWarning(s, null, false, false, true));
            Assert.IsNull(FacingEditAdvice.GetGizmoWarning(s, null));
        }

        [Test]
        public void Advice_PreviewOn_YawOnly_WarnsOnlyForY()
        {
            var s = new OrientationSettings { facing_mode = "yaw_only", edit_mode_preview_enabled = true };
            Assert.IsNull(FacingEditAdvice.GetSliderWarning(s, null, true, false, false), "X is visible in yaw_only.");
            Assert.IsNull(FacingEditAdvice.GetSliderWarning(s, null, false, false, true), "Z is visible in yaw_only.");
            Assert.IsNull(FacingEditAdvice.GetSliderWarning(s, null, true, false, true), "X and Z together are visible.");
            Assert.AreEqual(FacingEditAdvice.YawOnlyMessage, FacingEditAdvice.GetSliderWarning(s, null, false, true, false), "Y is overridden.");
            Assert.AreEqual(FacingEditAdvice.YawOnlyMessage, FacingEditAdvice.GetSliderWarning(s, null, true, true, false), "Y among the changed axes still warns.");
            Assert.IsNull(FacingEditAdvice.GetSliderWarning(s, null, false, false, false), "Nothing changed, nothing to warn about.");
            Assert.AreEqual(FacingEditAdvice.GizmoOverriddenMessage, FacingEditAdvice.GetGizmoWarning(s, null));
        }

        [Test]
        public void Advice_PreviewOn_AlwaysFacingCamera_WarnsForEveryAxis()
        {
            var s = new OrientationSettings { facing_mode = "always_facing_camera", edit_mode_preview_enabled = true };
            Assert.AreEqual(FacingEditAdvice.AlwaysFacingCameraMessage, FacingEditAdvice.GetSliderWarning(s, null, true, false, false));
            Assert.AreEqual(FacingEditAdvice.AlwaysFacingCameraMessage, FacingEditAdvice.GetSliderWarning(s, null, false, true, false));
            Assert.AreEqual(FacingEditAdvice.AlwaysFacingCameraMessage, FacingEditAdvice.GetSliderWarning(s, null, false, false, true));
            Assert.AreEqual(FacingEditAdvice.AlwaysFacingCameraMessage, FacingEditAdvice.GetGizmoWarning(s, null));
            StringAssert.Contains("Always Facing Camera", FacingEditAdvice.AlwaysFacingCameraMessage);
            StringAssert.Contains("Edit-Mode Preview", FacingEditAdvice.AlwaysFacingCameraMessage);
        }

        [Test]
        public void Advice_UsesEffectiveMode_LevelOverrideBeatsGlobal_AndUnknownMeansAlwaysFacing()
        {
            var s = new OrientationSettings { facing_mode = "always_facing_camera", edit_mode_preview_enabled = true };
            Assert.IsNull(FacingEditAdvice.GetSliderWarning(s, "wall_fixed", false, true, false), "A wall_fixed level override makes Y visible.");

            s.facing_mode = "wall_fixed";
            Assert.AreEqual(FacingEditAdvice.AlwaysFacingCameraMessage,
                FacingEditAdvice.GetSliderWarning(s, "always_facing_camera", false, true, false), "An always-facing override warns even on a wall_fixed wall.");

            s.facing_mode = "something_unknown";
            Assert.AreEqual(FacingEditAdvice.AlwaysFacingCameraMessage,
                FacingEditAdvice.GetSliderWarning(s, null, true, false, false), "The resolver treats unknown as always-facing; so must the advice.");
        }

        [Test]
        public void Advice_AgreesWithTheRealPipeline_WarnsExactlyWhenTheMarkerDoesNotChange()
        {
            // The warning must never lie: for every mode and axis, "warns" <=> "no visible change".
            foreach (string mode in new[] { "wall_fixed", "yaw_only", "always_facing_camera" })
            {
                Quaternion baseline = PreviewFor(mode, 0f, 0f, 0f);
                var s = _config.orientation_settings;
                bool[] axisVisible =
                {
                    Quaternion.Angle(baseline, PreviewFor(mode, 30f, 0f, 0f)) > 1f,
                    Quaternion.Angle(baseline, PreviewFor(mode, 0f, 75f, 0f)) > 1f,
                    Quaternion.Angle(baseline, PreviewFor(mode, 0f, 0f, 40f)) > 1f
                };
                bool[] warned =
                {
                    FacingEditAdvice.GetSliderWarning(s, null, true, false, false) != null,
                    FacingEditAdvice.GetSliderWarning(s, null, false, true, false) != null,
                    FacingEditAdvice.GetSliderWarning(s, null, false, false, true) != null
                };
                for (int axis = 0; axis < 3; axis++)
                    Assert.AreEqual(!axisVisible[axis], warned[axis], $"mode={mode} axis={"XYZ"[axis]}: warning must match the real visibility.");
            }
        }

        // ---------- gizmo gesture detection ----------

        [Test]
        public void GizmoGesture_OnlyForAHandleDragWithAnEditTool_NotCameraOrbit()
        {
            Assert.IsTrue(MarkerEditDetector.IsGizmoEditGesture(true, 1234, false, Tool.Rotate));
            Assert.IsTrue(MarkerEditDetector.IsGizmoEditGesture(true, 1234, false, Tool.Move));
            Assert.IsTrue(MarkerEditDetector.IsGizmoEditGesture(true, 1234, false, Tool.Rect));
            Assert.IsFalse(MarkerEditDetector.IsGizmoEditGesture(false, 1234, false, Tool.Rotate), "Not a drag event.");
            Assert.IsFalse(MarkerEditDetector.IsGizmoEditGesture(true, 0, false, Tool.Rotate), "No handle owns the mouse.");
            Assert.IsFalse(MarkerEditDetector.IsGizmoEditGesture(true, 1234, true, Tool.Rotate), "Alt-orbit / pan is a view tool.");
            Assert.IsFalse(MarkerEditDetector.IsGizmoEditGesture(true, 1234, false, Tool.View), "The hand tool never edits a marker.");
        }

        // ---------- gizmo rotation under preview ----------

        [Test]
        public void GizmoRotation_UnderPreview_IsAnEditOnlyInWallFixed()
        {
            _marker.localRotation = Quaternion.Euler(10f, 20f, 30f);

            Assert.IsFalse(_window.ProcessMarkerTransformEdit(_poi, _marker, true, "always_facing_camera", true, out _),
                "always_facing_camera preview output must not be captured as authored facing.");
            Assert.IsFalse(_window.ProcessMarkerTransformEdit(_poi, _marker, true, "yaw_only", true, out _),
                "yaw_only preview output must not be captured either.");
            Assert.AreEqual(0f, _poi.editor_rotation_deg);

            Assert.IsTrue(_window.ProcessMarkerTransformEdit(_poi, _marker, true, "wall_fixed", true, out _),
                "In wall_fixed the preview shows the authored angles untouched, so the gizmo IS the edit.");
            Assert.AreEqual(10f, _poi.editor_rotation_x_deg, 1e-3f);
            Assert.AreEqual(20f, _poi.editor_rotation_deg, 1e-3f);
            Assert.AreEqual(30f, _poi.editor_rotation_z_deg, 1e-3f);
        }

        [Test]
        public void VerifiedLock_UnderWallFixedPreview_StillRevertsGizmoRotation()
        {
            DragSliders(5f, 15f, 25f);
            _poi.position_verified = true;
            _marker.localRotation = Quaternion.Euler(80f, 80f, 80f);

            _window.ProcessMarkerTransformEdit(_poi, _marker, true, "wall_fixed", true, out string message);

            StringAssert.Contains("already verified", message);
            AssertSameRotation(_marker.localRotation, Quaternion.Euler(5f, 15f, 25f), "Verified facing must snap back under wall_fixed preview too.");
        }

        // ---------- (c) unverified in the window, then the Unity rotate tool ----------

        [Test]
        public void Unverified_SlidersThenUnityRotateTool_NeverShowsTheVerifiedWarning_AndBothStayInSync()
        {
            _config.orientation_settings.edit_mode_preview_enabled = false;

            // 1. sliders in the window
            DragSliders(25f, 40f, 55f);
            AssertSameRotation(_marker.localRotation, Quaternion.Euler(25f, 40f, 55f), "Sliders move the marker.");

            // 2. Unity's own rotation (gizmo / Inspector) on the SAME marker, same POI object
            _marker.localRotation = Quaternion.Euler(10f, 20f, 30f);
            bool changed = _window.ProcessMarkerTransformEdit(_poi, _marker, false, null, true, out string message);

            Assert.AreEqual(string.Empty, message, "An unverified POI must never get the verified warning.");
            Assert.IsTrue(changed);
            Assert.AreEqual(10f, _poi.editor_rotation_x_deg, 1e-3f, "Sliders follow the Unity rotation (X).");
            Assert.AreEqual(20f, _poi.editor_rotation_deg, 1e-3f, "(Y)");
            Assert.AreEqual(30f, _poi.editor_rotation_z_deg, 1e-3f, "(Z)");

            // 3. back to the sliders: they win again and the marker follows
            DragSliders(90f, 0f, 0f);
            AssertSameRotation(_marker.localRotation, Quaternion.Euler(90f, 0f, 0f), "Slider edits after a gizmo edit still apply.");
        }

        [Test]
        public void VerifyThenUnverify_LockFollowsTheLivePoiFlag_Immediately()
        {
            _config.orientation_settings.edit_mode_preview_enabled = false;
            DragSliders(10f, 20f, 30f);

            _poi.position_verified = true;
            _marker.localRotation = Quaternion.Euler(0f, 0f, 0f);
            _window.ProcessMarkerTransformEdit(_poi, _marker, false, null, true, out string lockedMessage);
            StringAssert.Contains("already verified", lockedMessage);
            AssertSameRotation(_marker.localRotation, Quaternion.Euler(10f, 20f, 30f), "Locked.");

            // The dialog path is not exercised here (it is modal); the state it ends in is.
            _poi.position_verified = false;
            _marker.localRotation = Quaternion.Euler(1f, 2f, 3f);
            _window.ProcessMarkerTransformEdit(_poi, _marker, false, null, true, out string openMessage);
            Assert.AreEqual(string.Empty, openMessage, "Unlocked again: no warning.");
            Assert.AreEqual(1f, _poi.editor_rotation_x_deg, 1e-3f);
        }

        [Test]
        public void VerifiedFlag_SurvivesSaveAndReload_AndTheReloadedPoiIsLocked()
        {
            DragSliders(10f, 20f, 30f);
            _poi.position_verified = true;

            var reloaded = JsonUtility.FromJson<WallConfigData>(JsonUtility.ToJson(_config));
            var poi = reloaded.pois[0];
            Assert.IsTrue(poi.position_verified, "Verified must persist in the config JSON, not only in memory.");

            _marker.localRotation = Quaternion.Euler(70f, 70f, 70f);
            _window.ProcessMarkerTransformEdit(poi, _marker, false, null, true, out string message);
            StringAssert.Contains("already verified", message);
            AssertSameRotation(_marker.localRotation, Quaternion.Euler(10f, 20f, 30f), "The reloaded POI locks to its saved angles.");
        }

        // ---------- (c) root cause: a closed window's scene handler must not survive ----------

        private static List<Delegate> SceneGuiHandlers()
        {
            var handlers = new List<Delegate>();
            foreach (var f in typeof(SceneView).GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (f.FieldType != typeof(Action<SceneView>)) continue;
                if (f.GetValue(null) is Delegate d)
                    handlers.AddRange(d.GetInvocationList());
            }
            return handlers;
        }

        [Test]
        public void ClosedWindow_LeavesNoSceneGuiHandlerBehind()
        {
            // Prove the probe works first: a live window's handlers are visible to it.
            var w = ScriptableObject.CreateInstance<POIEditorToolWindow>();
            int during = SceneGuiHandlers().FindAll(d => ReferenceEquals(d.Target, w)).Count;
            Assert.That(during, Is.GreaterThanOrEqualTo(2), "Premise: an enabled window subscribes its scene handlers (edit + preview).");

            UnityEngine.Object.DestroyImmediate(w);

            var leaked = SceneGuiHandlers().FindAll(d => ReferenceEquals(d.Target, w));
            Assert.AreEqual(0, leaked.Count,
                "A destroyed window must unsubscribe ALL its Scene-view handlers; a leaked one keeps reverting " +
                "rotations with its stale (still-verified) config and shows the verified warning.");
        }

        // ---------- (d) reveal, including when the edit is blocked ----------

        private void SelectMarker() { Selection.activeGameObject = _marker.gameObject; }

        private string SelectedTab() { return GetPrivate<object>("_selectedTab").ToString(); }

        private void ForgetReveal()
        {
            SetPrivate("_selectedTab", Enum.Parse(typeof(POIEditorToolWindow).GetNestedType("TabSelection", BindingFlags.NonPublic), "GlobalScene"));
            GetPrivate<Dictionary<string, bool>>("_poiFoldouts").Clear();
            SetPrivate("_pendingScrollPoiId", null);
        }

        [Test]
        public void Reveal_HappensEvenWhenTheEditIsBlockedByTheVerifiedLock()
        {
            _config.orientation_settings.edit_mode_preview_enabled = false;
            DragSliders(5f, 15f, 25f);
            _poi.position_verified = true;
            SelectMarker();

            _window.HandleSelectedMarkerEdit(true, false);            // first sight: baseline only
            Assert.AreEqual("GlobalScene", SelectedTab(), "Selecting alone must not reveal.");

            _marker.localRotation = Quaternion.Euler(80f, 80f, 80f);         // the blocked rotate
            _window.HandleSelectedMarkerEdit(true, false);

            AssertSameRotation(_marker.localRotation, Quaternion.Euler(5f, 15f, 25f), "The edit was blocked (reverted)...");
            Assert.AreEqual("SpecificMarker", SelectedTab(), "...and the Specific Marker tab still opened.");
            Assert.IsTrue(GetPrivate<Dictionary<string, bool>>("_poiFoldouts")[PoiId], "The POI section is expanded.");
            Assert.IsTrue(GetPrivate<bool>("_showPoiPosition"), "Its Position foldout is open.");
            Assert.AreEqual(PoiId, GetPrivate<string>("_pendingScrollPoiId"), "A scroll to it is queued.");
        }

        [Test]
        public void Reveal_UnderPreview_IsTriggeredByTheGizmoGesture_EvenWithNoPoseChange()
        {
            _config.orientation_settings.facing_mode = "always_facing_camera";
            _config.orientation_settings.edit_mode_preview_enabled = true;
            SelectMarker();

            _window.HandleSelectedMarkerEdit(true, false);
            Assert.AreEqual("GlobalScene", SelectedTab());

            _window.HandleSelectedMarkerEdit(true, true);              // Rotate-handle drag
            Assert.AreEqual("SpecificMarker", SelectedTab(), "The overridden gizmo drag still reveals the POI.");
            Assert.AreEqual(PoiId, GetPrivate<string>("_pendingScrollPoiId"));
        }

        [Test]
        public void Reveal_PreviewOwnedPoseChanges_AreNotMistakenForAnEdit()
        {
            _config.orientation_settings.edit_mode_preview_enabled = true;
            SelectMarker();
            _window.HandleSelectedMarkerEdit(true, false);

            _marker.rotation = Quaternion.Euler(0f, 130f, 0f);               // e.g. the preview following a camera orbit
            _window.HandleSelectedMarkerEdit(true, false);

            Assert.AreEqual("GlobalScene", SelectedTab(), "A preview-driven rotation is not a developer edit.");
        }

        [Test]
        public void Reveal_OncePerSelectedMarker_ResetsWhenSelectionChanges_AndNeverFromTheInspectorPoll()
        {
            _config.orientation_settings.edit_mode_preview_enabled = false;
            SelectMarker();
            _window.HandleSelectedMarkerEdit(true, false);

            // Inspector poll path (revealOnEdit=false) never reveals.
            _marker.localRotation = Quaternion.Euler(10f, 0f, 0f);
            _window.HandleSelectedMarkerEdit(false, false);
            Assert.AreEqual("GlobalScene", SelectedTab(), "The ~10 Hz poll must not move the window.");

            // Scene gesture path reveals once...
            _marker.localRotation = Quaternion.Euler(20f, 0f, 0f);
            _window.HandleSelectedMarkerEdit(true, false);
            Assert.AreEqual("SpecificMarker", SelectedTab());

            // ...and not again for the same marker (the developer may have scrolled away).
            ForgetReveal();
            _marker.localRotation = Quaternion.Euler(30f, 0f, 0f);
            _window.HandleSelectedMarkerEdit(true, false);
            Assert.AreEqual("GlobalScene", SelectedTab(), "Same marker: no second reveal.");

            // Deselect, reselect: fresh again.
            Selection.activeGameObject = null;
            _window.HandleSelectedMarkerEdit(true, false);
            SelectMarker();
            _window.HandleSelectedMarkerEdit(true, false);
            _marker.localRotation = Quaternion.Euler(40f, 0f, 0f);
            _window.HandleSelectedMarkerEdit(true, false);
            Assert.AreEqual("SpecificMarker", SelectedTab(), "A new selection can reveal again.");
        }

        // ---------- the messages actually reach the big overlay ----------

        private void WarnSliders(bool x, bool y, bool z)
        {
            var m = typeof(POIEditorToolWindow).GetMethod("WarnIfFacingEditInvisible", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(m);
            m.Invoke(_window, new object[] { _poi, x, y, z });
        }

        [Test]
        public void Overlay_SliderEdit_ShowsTheModeSpecificWarning_OnlyWhenAnAxisIsOverridden()
        {
            _config.orientation_settings.edit_mode_preview_enabled = true;

            _config.orientation_settings.facing_mode = "always_facing_camera";
            EditorNotice.Clear();
            WarnSliders(true, false, false);
            Assert.AreEqual(FacingEditAdvice.AlwaysFacingCameraMessage, EditorNotice.PendingMessage);

            _config.orientation_settings.facing_mode = "yaw_only";
            EditorNotice.Clear();
            WarnSliders(true, false, true);
            Assert.IsNull(EditorNotice.PendingMessage, "X/Z are visible in yaw_only: no overlay.");
            WarnSliders(false, true, false);
            Assert.AreEqual(FacingEditAdvice.YawOnlyMessage, EditorNotice.PendingMessage);

            _config.orientation_settings.facing_mode = "wall_fixed";
            EditorNotice.Clear();
            WarnSliders(true, true, true);
            Assert.IsNull(EditorNotice.PendingMessage, "wall_fixed never warns.");
        }

        [Test]
        public void Overlay_BlockedVerifiedRotation_ShowsTheLockMessage_EvenWithNoSceneViewOpen()
        {
            _config.orientation_settings.edit_mode_preview_enabled = false;
            DragSliders(5f, 15f, 25f);
            _poi.position_verified = true;
            SelectMarker();
            EditorNotice.Clear();

            _window.HandleSelectedMarkerEdit(true, false);
            Assert.IsNull(EditorNotice.PendingMessage, "Nothing blocked yet.");

            _marker.localRotation = Quaternion.Euler(80f, 80f, 80f);
            _window.HandleSelectedMarkerEdit(true, false);

            StringAssert.Contains("already verified", EditorNotice.PendingMessage);
            Assert.IsNull(EditorNotice.PendingDontShowAgainKey, "The verified-lock message explains a refused edit and can never be hidden.");
        }

        [Test]
        public void Overlay_UnverifiedGizmoRotation_ShowsNoVerifiedWarning()
        {
            _config.orientation_settings.edit_mode_preview_enabled = false;
            SelectMarker();
            EditorNotice.Clear();

            _window.HandleSelectedMarkerEdit(true, false);
            _marker.localRotation = Quaternion.Euler(10f, 20f, 30f);
            _window.HandleSelectedMarkerEdit(true, false);

            Assert.IsNull(EditorNotice.PendingMessage, "An unverified POI must never show the lock message.");
        }

        [Test]
        public void HandleEdit_WithNothingOrANonRigObjectSelected_DoesNothing()
        {
            Selection.activeGameObject = null;
            _window.HandleSelectedMarkerEdit(true, true);
            Assert.AreEqual("GlobalScene", SelectedTab());

            var stray = new GameObject("not_in_rig");
            try
            {
                Selection.activeGameObject = stray;
                _window.HandleSelectedMarkerEdit(true, true);
                Assert.AreEqual("GlobalScene", SelectedTab());
            }
            finally { UnityEngine.Object.DestroyImmediate(stray); }
        }
    }
}
