using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using TileStories.Editor;

namespace TileStories.Tests
{
    // Real-code EditMode tests for the per-POI Facing feature: X/Y/Z sync with the scene
    // transform, the Verified lock on facing, the auto-reveal state, and the config round
    // trip. Uses a real window instance, a real rig GameObject and the real LivingRoom
    // config -- no mocks. Nothing here calls TogglePoiVerification (its unlock path opens
    // a modal dialog); tests set position_verified directly instead.
    public class PoiFacingLockAndSyncTests
    {
        private POIEditorToolWindow _window;
        private GameObject _rigGO;
        private GameObject _child;

        [SetUp]
        public void SetUp()
        {
            _window = ScriptableObject.CreateInstance<POIEditorToolWindow>();
            _rigGO = new GameObject("POIEditorRig");
            _child = new GameObject("poi_a");
            _child.transform.SetParent(_rigGO.transform);
        }

        [TearDown]
        public void TearDown()
        {
            if (_window != null) Object.DestroyImmediate(_window);
            if (_rigGO != null) Object.DestroyImmediate(_rigGO);
        }

        private static POIData MakePoi(float x, float y, float z, bool verified)
        {
            return new POIData
            {
                id = "poi_a",
                editor_rotation_x_deg = x,
                editor_rotation_deg = y,
                editor_rotation_z_deg = z,
                position_verified = verified,
                position = new PositionData { x = 1f, y = 2f, z = 3f }
            };
        }

        private static T GetField<T>(object target, string name)
        {
            var field = typeof(POIEditorToolWindow).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, $"field {name} must exist");
            return (T)field.GetValue(target);
        }

        // ---- sync: orientation, not euler triples ----

        [Test]
        public void Sync_EquivalentEulerTriple_ReportsNoChange_AndKeepsStoredAngles()
        {
            var poi = MakePoi(100f, 30f, 20f, false);
            Vector3 sceneEuler = Quaternion.Euler(100f, 30f, 20f).eulerAngles;
            Assert.That(Mathf.Abs(sceneEuler.x - 100f), Is.GreaterThan(1f),
                "Premise: Unity reads X=100 back as an equivalent, different triple (~80,...).");

            bool changed = POIEditorToolWindow.SyncPoiRotationFromScene(poi, sceneEuler);

            Assert.IsFalse(changed, "The same orientation must not count as a change.");
            Assert.AreEqual(100f, poi.editor_rotation_x_deg, 1e-4f, "The slider value must not jump to the equivalent triple.");
            Assert.AreEqual(30f, poi.editor_rotation_deg, 1e-4f);
            Assert.AreEqual(20f, poi.editor_rotation_z_deg, 1e-4f);
        }

        // ---- lock rule (pure) ----

        [Test]
        public void RotationLock_VerifiedAndRotated_BlocksAndRestoresStoredAngles()
        {
            var poi = MakePoi(5f, 15f, 25f, true);
            Quaternion stored = PoiRotationResolver.ToEulerQuaternion(5f, 15f, 25f);

            bool blocked = POIEditorToolWindow.ShouldBlockVerifiedRotationChange(
                poi, Quaternion.Euler(40f, 90f, 10f), out Quaternion corrected, out string message);

            Assert.IsTrue(blocked);
            Assert.That(Quaternion.Angle(corrected, stored), Is.LessThan(0.01f));
            StringAssert.Contains("already verified", message);
        }

        [Test]
        public void RotationLock_Unverified_NeverBlocks()
        {
            var poi = MakePoi(5f, 15f, 25f, false);
            bool blocked = POIEditorToolWindow.ShouldBlockVerifiedRotationChange(
                poi, Quaternion.Euler(40f, 90f, 10f), out _, out string message);
            Assert.IsFalse(blocked);
            Assert.AreEqual(string.Empty, message);
        }

        [Test]
        public void RotationLock_VerifiedAndUnchangedOrEquivalent_DoesNotBlock()
        {
            var poi = MakePoi(100f, 30f, 20f, true);
            Quaternion same = PoiRotationResolver.ToEulerQuaternion(100f, 30f, 20f);
            Quaternion viaCanonicalEuler = Quaternion.Euler(same.eulerAngles);

            Assert.IsFalse(POIEditorToolWindow.ShouldBlockVerifiedRotationChange(poi, same, out _, out _));
            Assert.IsFalse(POIEditorToolWindow.ShouldBlockVerifiedRotationChange(poi, viaCanonicalEuler, out _, out _),
                "An equivalent euler triple is not an edit.");
        }

        [Test]
        public void FacingSliders_EditableOnlyWhileUnverified()
        {
            Assert.IsTrue(POIEditorToolWindow.AreFacingSlidersEditable(MakePoi(0, 0, 0, false)));
            Assert.IsFalse(POIEditorToolWindow.AreFacingSlidersEditable(MakePoi(0, 0, 0, true)));
            Assert.IsFalse(POIEditorToolWindow.AreFacingSlidersEditable(null));
        }

        // ---- real rig: what the window does with a transform edit ----

        [Test]
        public void ProcessEdit_Unverified_SceneRotationBecomesAuthoredFacing_AndTransformIsKept()
        {
            var poi = MakePoi(0f, 0f, 0f, false);
            _child.transform.localRotation = Quaternion.Euler(10f, 200f, 30f);

            bool changed = _window.ProcessMarkerTransformEdit(poi, _child.transform, false, null, false, out string message);

            Assert.IsTrue(changed);
            Assert.AreEqual(string.Empty, message);
            Assert.AreEqual(10f, poi.editor_rotation_x_deg, 1e-3f);
            Assert.AreEqual(200f, poi.editor_rotation_deg, 1e-3f);
            Assert.AreEqual(30f, poi.editor_rotation_z_deg, 1e-3f);
            Assert.That(Quaternion.Angle(_child.transform.localRotation, Quaternion.Euler(10f, 200f, 30f)), Is.LessThan(0.01f),
                "An unverified rotation must stay on the marker.");
        }

        [Test]
        public void ProcessEdit_Verified_RotationRevertsToStoredFacing_WithMessage_AndAnglesUntouched()
        {
            var poi = MakePoi(5f, 15f, 25f, true);
            _child.transform.localPosition = new Vector3(1f, 2f, 3f);
            _child.transform.localRotation = Quaternion.Euler(80f, 80f, 80f);

            bool changed = _window.ProcessMarkerTransformEdit(poi, _child.transform, false, null, true, out string message);

            Assert.IsFalse(changed, "A blocked edit must not touch the config.");
            StringAssert.Contains("already verified", message);
            Assert.That(Quaternion.Angle(_child.transform.localRotation, PoiRotationResolver.ToEulerQuaternion(5f, 15f, 25f)), Is.LessThan(0.01f),
                "The marker must snap back to the verified facing.");
            Assert.AreEqual(5f, poi.editor_rotation_x_deg);
            Assert.AreEqual(15f, poi.editor_rotation_deg);
            Assert.AreEqual(25f, poi.editor_rotation_z_deg);
        }

        [Test]
        public void ProcessEdit_Verified_WhilePreviewActive_TreatsRotationAsPreviewNotEdit()
        {
            var poi = MakePoi(5f, 15f, 25f, true);
            _child.transform.localPosition = new Vector3(1f, 2f, 3f);
            Quaternion previewRotation = Quaternion.Euler(80f, 80f, 80f);
            _child.transform.localRotation = previewRotation;

            bool changed = _window.ProcessMarkerTransformEdit(poi, _child.transform, true, "always_facing_camera", false, out string message);

            Assert.IsFalse(changed);
            Assert.AreEqual(string.Empty, message);
            Assert.That(Quaternion.Angle(_child.transform.localRotation, previewRotation), Is.LessThan(0.01f),
                "The preview's rotation must not be reverted.");
        }

        [Test]
        public void ProcessEdit_Verified_PositionLockStillWorks_AndRotateToolStaysSilent()
        {
            var poi = MakePoi(0f, 0f, 0f, true);

            _child.transform.localPosition = new Vector3(9f, 2f, 3f);
            _window.ProcessMarkerTransformEdit(poi, _child.transform, false, null, false, out string moveMessage);
            Assert.AreEqual(new Vector3(1f, 2f, 3f), _child.transform.localPosition, "Move-tool drag must snap back.");
            StringAssert.Contains("already verified", moveMessage);

            _child.transform.localPosition = new Vector3(9f, 2f, 3f);
            _window.ProcessMarkerTransformEdit(poi, _child.transform, false, null, true, out string rotateToolMessage);
            Assert.AreEqual(new Vector3(1f, 2f, 3f), _child.transform.localPosition, "Drift is still corrected under the Rotate tool.");
            Assert.AreEqual(string.Empty, rotateToolMessage, "...but silently, exactly as before.");
        }

        // ---- MarkerEditDetector ----

        [Test]
        public void Detector_FirstSightAndUnchangedPose_AreNotEdits()
        {
            var detector = new MarkerEditDetector();
            Assert.IsFalse(detector.Observe("a", Vector3.one, Quaternion.identity), "Just selecting a marker is not an edit.");
            Assert.IsFalse(detector.Observe("a", Vector3.one, Quaternion.identity));
        }

        [Test]
        public void Detector_MoveOrRotateOfSameMarker_IsAnEdit()
        {
            var detector = new MarkerEditDetector();
            detector.Observe("a", Vector3.zero, Quaternion.identity);
            Assert.IsTrue(detector.Observe("a", new Vector3(0.5f, 0f, 0f), Quaternion.identity), "moved");
            Assert.IsTrue(detector.Observe("a", new Vector3(0.5f, 0f, 0f), Quaternion.Euler(0f, 30f, 0f)), "rotated");
        }

        [Test]
        public void Detector_SwitchingMarkerOrResetting_StartsFresh()
        {
            var detector = new MarkerEditDetector();
            detector.Observe("a", Vector3.zero, Quaternion.identity);
            Assert.IsFalse(detector.Observe("b", Vector3.one, Quaternion.Euler(10f, 0f, 0f)), "A different marker is a first sight.");
            detector.Reset();
            Assert.IsFalse(detector.Observe("b", Vector3.zero, Quaternion.identity), "After Reset the next observation is a first sight.");
        }

        // ---- reveal state ----

        [Test]
        public void Reveal_SwitchesToSpecificMarkerTab_ExpandsPoiAndPosition_AndQueuesScroll()
        {
            var poi = MakePoi(0f, 0f, 0f, false);
            Assert.AreEqual("GlobalScene", GetField<object>(_window, "_selectedTab").ToString(), "Premise: window starts on Global Scene.");

            _window.RevealPoiInSpecificMarkerTab(poi);

            Assert.AreEqual("SpecificMarker", GetField<object>(_window, "_selectedTab").ToString());
            Assert.IsTrue(GetField<Dictionary<string, bool>>(_window, "_poiFoldouts")["poi_a"]);
            Assert.IsTrue(GetField<bool>(_window, "_showPoiPosition"));
            Assert.AreEqual("poi_a", GetField<string>(_window, "_pendingScrollPoiId"));
        }

        // ---- real IMGUI: three Facing rows ----

        private sealed class FacingRowsHarness : EditorWindow
        {
            public static bool DidDraw;
            public static readonly Rect[] SliderRects = new Rect[3];
            public static readonly float[] Returned = new float[3];
            public static float ViewWidthAtDraw;
            public static bool Editable;

            private void OnGUI()
            {
                DidDraw = true;
                ViewWidthAtDraw = EditorGUIUtility.currentViewWidth;
                EditorGUI.indentLevel += 1;
                try
                {
                    Returned[0] = POIEditorToolWindow.DrawFacingSliderRow("Facing X", 12f, Editable, true, out SliderRects[0]);
                    Returned[1] = POIEditorToolWindow.DrawFacingSliderRow("Facing Y", 34f, Editable, false, out SliderRects[1]);
                    Returned[2] = POIEditorToolWindow.DrawFacingSliderRow("Facing Z", 56f, Editable, false, out SliderRects[2]);
                }
                finally
                {
                    EditorGUI.indentLevel -= 1;
                }
            }
        }

        private static IEnumerator DrawFacingRows(bool editable)
        {
            FacingRowsHarness.DidDraw = false;
            FacingRowsHarness.Editable = editable;
            for (int i = 0; i < 3; i++) FacingRowsHarness.SliderRects[i] = default;

            var window = EditorWindow.GetWindow<FacingRowsHarness>(true, "FacingRowsHarness");
            window.minSize = new Vector2(200f, 120f);
            window.position = new Rect(0f, 0f, 700f, 140f);
            window.Focus();
            window.Repaint();

            for (int i = 0; i < 60 && (!FacingRowsHarness.DidDraw || FacingRowsHarness.SliderRects[2].width < 40f); i++)
                yield return null;

            window.Close();
        }

        [UnityTest]
        public IEnumerator FacingRows_ThreeSlidersStackedWithSameWidthInsidePanel()
        {
            yield return DrawFacingRows(true);

            Assert.That(FacingRowsHarness.DidDraw, Is.True);
            var r = FacingRowsHarness.SliderRects;
            Assert.That(r[0].width, Is.GreaterThanOrEqualTo(120f));
            Assert.AreEqual(r[0].width, r[1].width, 0.5f, "All three sliders share one width.");
            Assert.AreEqual(r[0].width, r[2].width, 0.5f);
            Assert.AreEqual(r[0].x, r[1].x, 0.5f, "Sliders line up in one column.");
            Assert.AreEqual(r[0].x, r[2].x, 0.5f);
            Assert.That(r[1].y, Is.GreaterThan(r[0].y + 8f), "Rows are stacked, not overlapping.");
            Assert.That(r[2].y, Is.GreaterThan(r[1].y + 8f));
            Assert.That(r[0].xMax, Is.LessThanOrEqualTo(FacingRowsHarness.ViewWidthAtDraw + 2f), "Row fits inside the visible panel.");
            Assert.AreEqual(12f, FacingRowsHarness.Returned[0], 1e-4f, "No interaction -> values come back unchanged.");
            Assert.AreEqual(34f, FacingRowsHarness.Returned[1], 1e-4f);
            Assert.AreEqual(56f, FacingRowsHarness.Returned[2], 1e-4f);
        }

        [UnityTest]
        public IEnumerator FacingRows_WhenLocked_StillDrawAllThreeAndReturnValuesUnchanged()
        {
            yield return DrawFacingRows(false);

            Assert.That(FacingRowsHarness.DidDraw, Is.True);
            Assert.That(FacingRowsHarness.SliderRects[2].width, Is.GreaterThanOrEqualTo(120f), "Locked rows are still visible (just disabled).");
            Assert.AreEqual(12f, FacingRowsHarness.Returned[0], 1e-4f);
            Assert.AreEqual(34f, FacingRowsHarness.Returned[1], 1e-4f);
            Assert.AreEqual(56f, FacingRowsHarness.Returned[2], 1e-4f);
        }

        // ---- real config: all three angles survive Save/Load and reach StreamingAssets ----

        private static WallConfigData LoadConfig(string assetRelativePath)
        {
            string path = Path.Combine(Application.dataPath, assetRelativePath);
            Assert.IsTrue(File.Exists(path), path);
            return JsonUtility.FromJson<WallConfigData>(File.ReadAllText(path));
        }

        [Test]
        public void LivingRoomConfig_AllThreeFacingAnglesRoundTripThroughJson_PerPoi()
        {
            var original = LoadConfig("Apps/LivingRoom/config.json");
            Assert.That(original.pois.Count, Is.GreaterThan(10));

            var reloaded = JsonUtility.FromJson<WallConfigData>(JsonUtility.ToJson(original, true));

            bool sawNonZeroX = false, sawNonZeroZ = false;
            for (int i = 0; i < original.pois.Count; i++)
            {
                var a = original.pois[i];
                var b = reloaded.pois[i];
                Assert.AreEqual(a.editor_rotation_x_deg, b.editor_rotation_x_deg, 1e-4f, a.id + " x");
                Assert.AreEqual(a.editor_rotation_deg, b.editor_rotation_deg, 1e-4f, a.id + " y");
                Assert.AreEqual(a.editor_rotation_z_deg, b.editor_rotation_z_deg, 1e-4f, a.id + " z");
                sawNonZeroX |= Mathf.Abs(a.editor_rotation_x_deg) > 0.001f;
                sawNonZeroZ |= Mathf.Abs(a.editor_rotation_z_deg) > 0.001f;
            }
            Assert.IsTrue(sawNonZeroX && sawNonZeroZ, "The real config must actually exercise non-zero X and Z, or this test proves nothing.");
        }

        [Test]
        public void LivingRoomConfig_StreamingAssetsCopyCarriesSameFacingAnglesAsAuthoringConfig()
        {
            var authoring = LoadConfig("Apps/LivingRoom/config.json");
            var streaming = LoadConfig("StreamingAssets/LivingRoom/config.json");
            Assert.AreEqual(authoring.pois.Count, streaming.pois.Count);
            for (int i = 0; i < authoring.pois.Count; i++)
            {
                Assert.AreEqual(authoring.pois[i].id, streaming.pois[i].id);
                Assert.AreEqual(authoring.pois[i].editor_rotation_x_deg, streaming.pois[i].editor_rotation_x_deg, 1e-4f, authoring.pois[i].id + " x");
                Assert.AreEqual(authoring.pois[i].editor_rotation_deg, streaming.pois[i].editor_rotation_deg, 1e-4f, authoring.pois[i].id + " y");
                Assert.AreEqual(authoring.pois[i].editor_rotation_z_deg, streaming.pois[i].editor_rotation_z_deg, 1e-4f, authoring.pois[i].id + " z");
            }
        }
    }
}
