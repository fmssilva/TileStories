using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace TileStories.Tests
{
    // The LOD / Zoom domain in the REAL wall scene. Also the on-screen zoom buttons, whose stylesheet
    // was never applied (they drew as full-width grey bars above the top of the screen).
    //
    // The LOD demo field in the REAL wall scene (LivingRoomScene: its scanned room mesh, its mock camera
    // standing about a metre from a wall, its WallSession + LODController + shipped config), switched on
    // and edited through WallSession.ApplyDemoField -- the runtime seam the POI Editor's live push calls.
    // Every earlier demo-field test built its own camera at the origin with no room mesh, so none of them
    // could see that the field spawned INSIDE the room mesh (invisible) and that its yaw_only markers were
    // turned edge-on. These tests look through the scene's own camera, render real pixels, and fail on
    // either. Renders are saved under Assets/Screenshots as LodDemoFieldScene_*.png.
    public class LodWallSceneTests
    {
        private const string ScenePath = "Assets/Apps/LivingRoom/LivingRoomScene.unity";

        private WallSession _session;
        private LODController _lod;
        private Camera _camera;
        private readonly List<Object> _tracked = new();

        // Errors logged while the wall scene runs, except the AR SDK's own "no device" complaints
        private readonly List<string> _unexpectedErrors = new();

        // The wall scene carries the real AR/tracking SDK, which logs an error every frame it finds no
        // device camera (expected in the Editor). Only that is tolerated: any other error fails the test.
        private void CollectUnexpectedErrors(string message, string stack, LogType type)
        {
            if ((type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                && !message.StartsWith("[ARFoundationSupport]"))
                _unexpectedErrors.Add(type + ": " + message);
        }

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            LogAssert.ignoreFailingMessages = true;
            _unexpectedErrors.Clear();
            Application.logMessageReceived += CollectUnexpectedErrors;
#if UNITY_EDITOR
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
#else
            Assert.Ignore("Needs the Editor to load the wall scene by path.");
#endif
            // the real tracking SDK polls a device camera every frame and logs an error when there is none;
            // in the Editor the mock tracker drives the camera instead, so its session is paused
            foreach (var mb in Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
                if (mb != null && mb.GetType().FullName == "Immersal.XR.ImmersalSession") mb.enabled = false;
            ARZoomState.SetZoom(1f, 1f, 4f);
            // the wall spawns once its config has loaded and the mock tracker has localised it
            for (int frame = 0; frame < 300; frame++)
            {
                _session = Object.FindFirstObjectByType<WallSession>();
                if (_session != null && _session.SpawnedMarkers.Count > 0) break;
                yield return null;
            }
            Assert.IsNotNull(_session, "the wall scene has a WallSession");
            Assert.Greater(_session.SpawnedMarkers.Count, 0, "precondition: the wall's POIs spawned");
            Assert.IsNull(_session.DemoFieldRoot, "precondition: the shipped config has the demo field off");
            _lod = Object.FindFirstObjectByType<LODController>();
            _camera = Camera.main;
            Assert.IsNotNull(_lod, "the wall scene runs a LODController");
            Assert.IsNotNull(_camera, "the wall scene has a main camera");
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            LogAssert.ignoreFailingMessages = true;
            Application.logMessageReceived -= CollectUnexpectedErrors;
            foreach (var o in _tracked) if (o != null) Object.Destroy(o);
            _tracked.Clear();
            ARZoomState.SetZoom(1f, 1f, 4f);
            MarkerHierarchyResolver.ResetToDefaults();
            StatusRamp.ResetToDefaults();

            // leave NOTHING of the wall scene behind: its camera, tracker and WallSession would keep
            // running inside every later test (they did: 23 unrelated failures)
            var wallScene = SceneManager.GetSceneByPath(ScenePath);
            SceneManager.SetActiveScene(SceneManager.CreateScene("AfterLodWallSceneTest"));
            if (wallScene.IsValid() && wallScene.isLoaded)
                yield return SceneManager.UnloadSceneAsync(wallScene);
            yield return null;
            CollectionAssert.IsEmpty(_unexpectedErrors, "the wall scene logged errors while the demo field ran");
        }

        private DemoFieldSettings Field(int perLevel, int clump) => new()
        {
            enabled = true, seed = 5, distance_m = 1.5f, width_m = 5f, height_m = 2f, depth_m = 6f,
            dense_clump_count = clump, dense_clump_radius_m = 0.08f,
            level_counts = CurrentLevels().Select(l => new DemoFieldLevelCount { level_key = l.key, count = perLevel }).ToList(),
        };

        // The running wall's own hierarchy levels (read back from its settings, not hardcoded)
        private List<HierarchyLevelEntry> CurrentLevels()
        {
            var config = (WallConfigData)typeof(WallSession)
                .GetField("_config", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(_session);
            return config.hierarchy_levels;
        }

        private List<MarkerView> DemoMarkers() => _session.SpawnedMarkers.Where(m => m != null && m.PoiId.StartsWith("demo_")).ToList();

        // Seconds of real frames: reveals, fades and LOD evaluations all run on Time
        private static IEnumerator Wait(float seconds)
        {
            float end = Time.time + seconds;
            while (Time.time < end) yield return null;
        }

        [UnityTest]
        public IEnumerator DemoField_InTheRealWallScene_IsOnAnEmptyStage_SeenAndFacingTheCamera_ThenGivesTheWallBack()
        {
            LogAssert.ignoreFailingMessages = true; // reset by the framework per phase; CollectUnexpectedErrors still guards
            var wallCameraPose = new Pose(_camera.transform.position, _camera.transform.rotation);
            var wallMarkers = _session.SpawnedMarkers.ToList();
            var noLod = new LodSettings { enabled = false };  // every demo marker shown: nothing hides one on purpose
            _session.ApplyLodSettings(noLod);

            _session.ApplyDemoField(Field(perLevel: 3, clump: 0));
            yield return Wait(2f);

            // --- the field is on the stage, the camera with it, the wall paused ---
            var root = _session.DemoFieldRoot;
            Assert.IsNotNull(root, "the field spawned");
            Assert.AreEqual(DemoFieldStage.EditorStagePosition, root.transform.position);
            Assert.AreEqual(DemoFieldStage.EditorStagePosition, _camera.transform.position, "the camera moved to the stage");
            var demo = DemoMarkers();
            Assert.AreEqual(CurrentLevels().Count * 3, demo.Count, "LOD, displacement and selection run the demo markers alone");
            Assert.AreEqual(demo.Count, _session.SpawnedMarkers.Count);
            Assert.IsTrue(wallMarkers.All(m => !m.gameObject.activeInHierarchy), "the wall's own POIs are paused");

            // --- nothing of the scene stands between the camera and any demo marker ---
            var sceneRenderers = Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None)
                .Where(r => r.enabled && r.gameObject.activeInHierarchy && !r.transform.IsChildOf(root.transform)).ToList();
            foreach (var m in demo)
            {
                Vector3 toMarker = m.transform.position - _camera.transform.position;
                var ray = new Ray(_camera.transform.position, toMarker.normalized);
                foreach (var r in sceneRenderers)
                    Assert.IsFalse(r.bounds.IntersectRay(ray, out float hit) && hit < toMarker.magnitude,
                        m.PoiId + " is hidden behind the scene object '" + r.name + "'");
                Assert.Greater(_camera.WorldToViewportPoint(m.transform.position).z, 0f, m.PoiId + " is in front of the camera");
            }
            // the box is wider than the view up close (the developer walks into it), so most -- not all -- are on screen
            int onScreen = demo.Count(m =>
            {
                Vector3 vp = _camera.WorldToViewportPoint(m.transform.position);
                return vp.x >= 0f && vp.x <= 1f && vp.y >= 0f && vp.y <= 1f;
            });
            Assert.GreaterOrEqual(onScreen, demo.Count * 3 / 4, "most of the field is on screen from the stage start");

            // --- every marker shows its readable side to the camera (yaw_only once turned them edge-on) ---
            // - "facing the camera" depends on the wall's Facing Basis: view_plane faces along the camera's
            //   own forward (parallel to the screen), camera_position faces the camera's position
            bool viewPlane = _session.OrientationSettings?.facing_basis == "view_plane";
            foreach (var m in demo)
            {
                var billboard = m.GetComponentInParent<MarkerBillboard>();
                Vector3 forward = Vector3.ProjectOnPlane(billboard.transform.forward, Vector3.up).normalized;
                Vector3 away = Vector3.ProjectOnPlane(viewPlane
                    ? _camera.transform.forward
                    : billboard.transform.position - _camera.transform.position, Vector3.up).normalized;
                Assert.Greater(Vector3.Dot(forward, away), 0.985f, m.PoiId + " must face the camera (within 10 degrees)");
            }

            // --- the pixels: each marker's centre changes the rendered frame (it is really drawn) ---
            var with = Render("LodDemoFieldScene_stage.png");
            root.SetActive(false);
            var without = Render(null);
            root.SetActive(true);
            int checkedMarkers = 0;
            foreach (var m in demo)
            {
                if (ProjectedDiameterPx(m) < 8f) continue; // too small to hit its centre pixel reliably
                Vector3 vp = _camera.WorldToViewportPoint(m.transform.position);
                if (vp.x < 0.02f || vp.x > 0.98f || vp.y < 0.02f || vp.y > 0.98f) continue; // off screen
                // strongest change inside the symbol's inner disc (its exact centre can be a dark icon stroke)
                Vector3 sp = _camera.WorldToScreenPoint(m.transform.position);
                int reach = Mathf.Max(1, Mathf.RoundToInt(ProjectedDiameterPx(m) * 0.3f));
                float strongest = 0f;
                for (int dx = -reach; dx <= reach; dx++)
                for (int dy = -reach; dy <= reach; dy++)
                {
                    if (dx * dx + dy * dy > reach * reach) continue;
                    Color a = with.GetPixel((int)sp.x + dx, (int)sp.y + dy), b = without.GetPixel((int)sp.x + dx, (int)sp.y + dy);
                    strongest = Mathf.Max(strongest, Mathf.Abs(a.r - b.r) + Mathf.Abs(a.g - b.g) + Mathf.Abs(a.b - b.b));
                }
                Assert.Greater(strongest, 0.3f, m.PoiId + " is not drawn at its own screen position");
                checkedMarkers++;
            }
            Assert.GreaterOrEqual(checkedMarkers, demo.Count / 3, "enough demo markers are on screen and big enough to check by pixel");

            // --- off: the camera, the wall's POIs and its markers come back exactly ---
            var off = Field(3, 0);
            off.enabled = false;
            _session.ApplyDemoField(off);
            yield return null;
            Assert.IsNull(_session.DemoFieldRoot);
            Assert.AreEqual(wallCameraPose.position, _camera.transform.position, "the camera is back where it was");
            Assert.Less(Quaternion.Angle(wallCameraPose.rotation, _camera.transform.rotation), 0.01f);
            CollectionAssert.AreEquivalent(wallMarkers, _session.SpawnedMarkers, "LOD runs the wall's own POIs again");
            Assert.IsTrue(wallMarkers.All(m => m.gameObject.activeInHierarchy), "the wall's POIs are active again");
        }

        [UnityTest]
        public IEnumerator DemoField_EachControl_ChangesTheRealField_AndTheLiveReadoutFollowsLod()
        {
            LogAssert.ignoreFailingMessages = true;
            // Hybrid with a cap: clusters and Max markers both have something to do
            var lod = new LodSettings
            {
                density_response_mode = "hybrid", density_radius_px = 60f, transition_fade_duration_s = 0f,
                evaluation_interval_s = 0.05f,
            };
            _session.ApplyLodSettings(lod);
            var levels = CurrentLevels();
            var field = Field(perLevel: 2, clump: 0);
            _session.ApplyDemoField(field);
            yield return Wait(0.5f);
            Assert.AreEqual(levels.Count * 2, DemoMarkers().Count, "precondition");

            // Markers per Hierarchy Level: one slider changes only its own level
            string first = levels[0].key;
            field.level_counts.First(r => r.level_key == first).count = 6;
            _session.ApplyDemoField(field);
            Assert.AreEqual(6, DemoMarkers().Count(m => m.HierarchyLevelKey == first), "the first level now has 6 markers");
            Assert.AreEqual((levels.Count - 1) * 2, DemoMarkers().Count(m => m.HierarchyLevelKey != first), "the others keep 2");

            // Dense Clump: that many extra markers, packed together
            field.dense_clump_count = 9;
            _session.ApplyDemoField(field);
            var clump = DemoMarkers().Where(m => m.PoiId.StartsWith("demo_clump_")).ToList();
            Assert.AreEqual(9, clump.Count);
            float spread = clump.Max(a => clump.Max(b => (a.transform.position - b.transform.position).magnitude));
            Assert.LessOrEqual(spread, 2f * field.dense_clump_radius_m + 1e-3f, "packed within Clump Radius");

            // Show labels: off hides every demo label
            field.show_labels = false;
            _session.ApplyDemoField(field);
            Assert.IsTrue(DemoMarkers().All(m => !m.transform.Find("Label").gameObject.activeSelf), "no demo marker shows a label");
            field.show_labels = true;

            // Reshuffle: same markers, new places, the field itself stays on the stage
            var before = DemoMarkers().ToDictionary(m => m.PoiId, m => m.transform.position);
            field.seed++;
            _session.ApplyDemoField(field);
            Assert.AreEqual(DemoFieldStage.EditorStagePosition, _session.DemoFieldRoot.transform.position, "the field did not move");
            CollectionAssert.AreEquivalent(before.Keys, DemoMarkers().Select(m => m.PoiId), "the same markers");
            Assert.IsTrue(DemoMarkers().Any(m => (m.transform.position - before[m.PoiId]).sqrMagnitude > 1e-4f), "in new places");

            // Live readout: LOD evaluated the demo field, and it counts every marker once
            // (1.5 s also lets the slowest level's reveal, delay + duration, finish before any alpha is read)
            yield return Wait(1.5f);
            var stats = _lod.LastStats;
            Assert.IsNotNull(stats, "LOD evaluated and published its readout");
            Assert.AreEqual(DemoMarkers().Count, stats.Markers, "the readout counts every running marker");
            Assert.AreEqual(stats.Markers, stats.Shown + stats.InClusters + stats.HiddenByCrowding + stats.HiddenByMaxMarkers + stats.OutOfView,
                "every marker is in exactly one outcome");
            Assert.GreaterOrEqual(stats.Clusters, 1, "the clump is a cluster in Hybrid");
            Render("LodDemoFieldScene_hybrid.png");

            // a band's Max markers of 1: the readout shows the cut, and so do the real markers
            var capped = new LodSettings
            {
                density_response_mode = "none", transition_fade_duration_s = 0f, evaluation_interval_s = 0.05f,
                bands = new List<LodBandEntry> { new() { max_distance_m = 9999f, max_visible_count = 1 } },
            };
            _session.ApplyLodSettings(capped);
            yield return Wait(0.6f);
            stats = _lod.LastStats;
            Assert.AreEqual(1, stats.Shown, "Max markers 1: one marker shows");
            Assert.AreEqual(stats.Markers - 1 - stats.OutOfView, stats.HiddenByMaxMarkers, "every other one is cut by Max markers");
            Assert.AreEqual(1, DemoMarkers().Count(m => m.GetComponent<CanvasGroup>().alpha > 0.01f), "and only one is really drawn");
            Assert.AreEqual((1, stats.Markers - stats.OutOfView), (stats.Bands[0].Shown, stats.Bands[0].InBand));
            Render("LodDemoFieldScene_max_markers_1.png");
        }

        [UnityTest]
        public IEnumerator ZoomButtons_InTheRealWallScene_AreSmallRoundButtons_DockedBottomRight_AndFollowShowOnScreenButtons()
        {
            LogAssert.ignoreFailingMessages = true;
            yield return null; // one layout pass after Start mounted the view
            yield return null;
            var view = Object.FindFirstObjectByType<ZoomControlView>();
            Assert.IsNotNull(view, "the wall scene carries the zoom buttons");
            Assert.IsNotNull(view.Root, "they mounted into their UI document");
            var strip = UnityEngine.UIElements.UQueryExtensions.Q(view.Root, "zoom-control-root");
            var panel = view.Root.panel.visualTree.layout;

            foreach (string name in new[] { "zoom-out-button", "zoom-reset-button", "zoom-in-button" })
            {
                var b = UnityEngine.UIElements.UQueryExtensions.Q(view.Root, name);
                Assert.AreEqual(48f, b.resolvedStyle.width, 1f, name + ": the stylesheet's button size (it once stretched to the full width)");
                Assert.GreaterOrEqual(b.resolvedStyle.height, 44f, name + ": at least the 44 px tap target");
            }
            Rect r = strip.worldBound;
            Assert.IsTrue(r.xMin >= 0f && r.yMin >= 0f && r.xMax <= panel.width && r.yMax <= panel.height,
                "the strip is fully on screen (it once sat above the top edge): " + r + " in " + panel);
            Assert.Greater(r.center.x, panel.width * 0.5f, "docked on the right");
            Assert.Greater(r.center.y, panel.height * 0.5f, "docked at the bottom");

            // Show On-Screen Buttons off, live: the strip hides; back on: it shows
            var zoom = _session.ZoomSettings;
            _session.ApplyZoomSettings(new ZoomSettings { enabled = zoom.enabled, show_ui_buttons = false });
            yield return null;
            Assert.IsFalse(view.Root.visible, "hidden while Show On-Screen Buttons is off");
            _session.ApplyZoomSettings(new ZoomSettings { enabled = true, show_ui_buttons = true });
            yield return null;
            Assert.IsTrue(view.Root.visible);
        }

        [UnityTest]
        public IEnumerator ZoomIn_ClickedTwiceQuickly_GoesTwoLevels_AndTheCameraFollows()
        {
            LogAssert.ignoreFailingMessages = true;
            var zoom = Object.FindFirstObjectByType<ARZoomController>();
            Assert.IsNotNull(zoom, "the wall scene carries the zoom controller");
            _session.ApplyZoomSettings(new ZoomSettings { enabled = true, min_factor = 1f, max_factor = 4f, tap_step = 1.5f, tap_levels = 3, transition_duration_s = 0.3f });
            yield return null;
            float baseFov = _camera.fieldOfView;

            // the second click lands while the first is still animating (it once re-targeted 1.5x)
            zoom.ZoomIn();
            yield return null;
            zoom.ZoomIn();
            yield return Wait(1f);

            Assert.AreEqual(2.25f, ARZoomState.ZoomFactor, 1e-3f, "two levels of Double-Tap Step 1.5: 1.5 x 1.5");
            Assert.AreEqual(baseFov / 2.25f, _camera.fieldOfView, 0.05f, "the real camera zoomed with it");

            zoom.ResetToBase();
            yield return null;
            Assert.AreEqual(1f, ARZoomState.ZoomFactor, 1e-4f);
        }

        // The marker's symbol width on screen, in pixels
        private float ProjectedDiameterPx(MarkerView m)
        {
            float dist = Vector3.Distance(_camera.transform.position, m.transform.position);
            float pxPerMetre = _camera.pixelHeight / (2f * dist * Mathf.Tan(_camera.fieldOfView * 0.5f * Mathf.Deg2Rad));
            return m.SymbolDiameterMetres * pxPerMetre;
        }

        // Render the scene's own camera into a texture the size of its view (saved as a PNG when named)
        private Texture2D Render(string fileName)
        {
            int w = _camera.pixelWidth, h = _camera.pixelHeight;
            var rt = new RenderTexture(w, h, 24);
            var previousTarget = _camera.targetTexture;
            _camera.targetTexture = rt;
            _camera.Render();
            _camera.targetTexture = previousTarget;
            var previousActive = RenderTexture.active;
            RenderTexture.active = rt;
            var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            tex.Apply();
            RenderTexture.active = previousActive;
            rt.Release();
            Object.Destroy(rt);
            _tracked.Add(tex);
            if (fileName != null)
            {
                string dir = System.IO.Path.Combine(Application.dataPath, "Screenshots");
                System.IO.Directory.CreateDirectory(dir);
                System.IO.File.WriteAllBytes(System.IO.Path.Combine(dir, fileName), tex.EncodeToPNG());
            }
            return tex;
        }
    }
}
