using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace TileStories.Tests
{
    // Phase A tests for the orientation domain (_2.1_Marker_Orientation.md Block 3 / section
    // 15). Driven by OrientationGalleryDefinitions.Entries, spawned through the same
    // OrientationGalleryHarness.SpawnEntry path the visual harness uses -- the composition
    // guarantee 40-testing.md 4.2.1 asks for (a real call site, not a hand-built stand-in).
    public class OrientationGalleryTests
    {
        private GameObject _camGO;
        private Camera _cam;
        private RenderTexture _camTarget;
        private GameObject _eventSystemGO;
        private readonly List<GameObject> _spawned = new();

        [SetUp]
        public void SetUp()
        {
            // Defensive: Camera.main is ambiguous if more than one GameObject is tagged
            // MainCamera at once. MarkerBillboard.Awake resolves Camera.main at spawn time,
            // so a stray leftover MainCamera-tagged object (e.g. a same-frame teardown/setup
            // race between tests) makes the marker orient against the WRONG camera while
            // this test's own screen-space math correctly uses _cam - an order-dependent
            // mismatch that only shows up running the whole class, never a single test alone.
            foreach (var stray in GameObject.FindGameObjectsWithTag("MainCamera"))
                Object.DestroyImmediate(stray);

            _camGO = new GameObject("TestCamera");
            _camGO.tag = "MainCamera";
            _cam = _camGO.AddComponent<Camera>();
            _cam.transform.position = new Vector3(0f, 0f, -3f);
            _cam.transform.rotation = Quaternion.identity;

            // A camera with no render target can report a degenerate pixelWidth/pixelHeight
            // in a headless/background test run (no visible Game View), which makes any
            // WorldToScreenPoint-based pixel measurement meaningless regardless of the real
            // geometry. A fixed RenderTexture target makes pixel-space assertions deterministic.
            _camTarget = new RenderTexture(1024, 768, 16);
            _cam.targetTexture = _camTarget;

            // EventSystem only - no input module. GraphicRaycaster.Raycast needs
            // EventSystem.current to exist but not an active input module, and
            // StandaloneInputModule reads the legacy Input class, which this project has
            // switched off in favour of the Input System package (throws otherwise).
            _eventSystemGO = new GameObject("EventSystem");
            _eventSystemGO.AddComponent<EventSystem>();
        }

        [TearDown]
        public void TearDown()
        {
            // DestroyImmediate, not Destroy: Destroy() defers to end-of-frame, which left a
            // window where the next test's SetUp could see a stale "current" EventSystem
            // still alive, disable its own new one (Unity only allows one active
            // EventSystem), and then the stale one got destroyed anyway - leaving
            // EventSystem.current null for every test after that. Caught via a flaky
            // Entry1_Symbol_IsTopmostRaycastTarget that only failed when run alongside
            // other tests in this class, never alone.
            foreach (var go in _spawned) if (go != null) Object.DestroyImmediate(go);
            _spawned.Clear();
            if (_camGO != null) Object.DestroyImmediate(_camGO);
            if (_eventSystemGO != null) Object.DestroyImmediate(_eventSystemGO);
            if (_camTarget != null) { _camTarget.Release(); Object.DestroyImmediate(_camTarget); }
        }

        private GameObject Spawn(OrientationGalleryEntry entry, int row)
        {
            var prefab = MarkerGalleryTestFixture.LoadPrefab();
            var go = OrientationGalleryHarness.SpawnEntry(prefab, entry, row);
            _spawned.Add(go);
            return go;
        }

        [UnityTest]
        public IEnumerator Entry1_ScreenAligned_Defaults_RootUpMatchesScreenUp()
        {
            var entry = OrientationGalleryDefinitions.Entries[0];
            var go = Spawn(entry, 0);
            _cam.transform.rotation = Quaternion.Euler(10f, 15f, 20f); // includes roll
            yield return null;

            Vector3 screenUp = MarkerOrientationResolver.ScreenUpWorld(_cam);
            Assert.Less(Vector3.Angle(go.transform.rotation * Vector3.up, screenUp), 0.5f);
        }

        [UnityTest]
        public IEnumerator Entry3_WorldUp_RootUpMatchesGravity_DespiteCameraRoll()
        {
            var entry = OrientationGalleryDefinitions.Entries[2]; // world_up / view_plane / world_gravity
            var go = Spawn(entry, 0);
            // Roll only, no pitch: when the camera's forward has pitch, an orthonormal
            // LookRotation basis mathematically cannot keep up exactly at world gravity AND
            // forward exactly at that pitch (up must stay perpendicular to forward) - the
            // roll-independence guarantee this mode makes is about ROLL, not pitch (mirrors
            // the Block 2 resolver test's own camera setup).
            _cam.transform.rotation = Quaternion.Euler(0f, 0f, 45f);
            yield return null;

            Assert.Less(Vector3.Angle(go.transform.rotation * Vector3.up, Vector3.up), 0.5f);
        }

        [UnityTest]
        public IEnumerator Entry8_WallFixed_UsesAuthoredLocalRotation()
        {
            var entry = OrientationGalleryDefinitions.Entries[7]; // wall_fixed
            var prefab = MarkerGalleryTestFixture.LoadPrefab();

            // Author a local rotation before spawn - SpawnEntry's MarkerBillboard.Configure
            // snapshots transform.localRotation as authoredLocalRotation.
            var instance = Object.Instantiate(prefab);
            instance.transform.localRotation = Quaternion.Euler(0f, 20f, 0f);
            var authored = instance.transform.localRotation;
            Object.Destroy(instance);

            var go = Object.Instantiate(prefab);
            _spawned.Add(go);
            go.transform.localRotation = authored;
            var settings = new OrientationSettings { marker_orientation_mode = entry.MarkerMode };
            var anchor = go.GetComponent<POIAnchor>() ?? go.AddComponent<POIAnchor>();
            anchor.Initialise(new POIData { id = "wallfixed", name = "wallfixed", category = "religious" });
            var view = go.GetComponentInChildren<MarkerView>();
            view.Initialise(anchor, MarkerStyle.OutlineGold, MarkerShape.Circle);
            var billboard = go.GetComponent<MarkerBillboard>() ?? go.AddComponent<MarkerBillboard>();
            billboard.Configure(settings, "", null);

            _cam.transform.rotation = Quaternion.Euler(30f, 40f, 50f); // must not affect wall_fixed
            yield return null;

            Assert.Less(Quaternion.Angle(go.transform.rotation, authored), 0.01f);
        }

        [UnityTest]
        public IEnumerator AllEntries_SpawnWithNonNullVisualChildren()
        {
            int row = 0;
            foreach (var entry in OrientationGalleryDefinitions.Entries)
            {
                var go = Spawn(entry, row++);
                yield return null;

                Assert.IsNotNull(go.transform.Find("Symbol"), $"{entry.Label}: Symbol missing");
                Assert.IsNotNull(go.transform.Find("Label"), $"{entry.Label}: Label missing");
                Assert.IsNotNull(go.transform.Find("Ring"), $"{entry.Label}: Ring missing");
                Assert.IsNotNull(go.transform.Find("Badge"), $"{entry.Label}: Badge missing");
            }
        }

        [UnityTest]
        public IEnumerator Entry12_ScreenAlignedLabelScreenUp_RootMatchesEntry1()
        {
            var entry1 = OrientationGalleryDefinitions.Entries[0];
            var entry12 = OrientationGalleryDefinitions.Entries[11];
            Assert.AreEqual("screen_aligned", entry12.MarkerMode);
            Assert.AreEqual("screen_up", entry12.LabelMode);

            var go1 = Spawn(entry1, 0);
            var go12 = Spawn(entry12, 1);
            _cam.transform.rotation = Quaternion.Euler(8f, 12f, 25f);
            yield return null;

            Assert.Less(Quaternion.Angle(go1.transform.rotation, go12.transform.rotation), 0.1f);
        }

        // Tier 0.5 - occlusion / actually-clickable check (40-testing.md section 4.5).
        // Confirms the marker's Symbol is the topmost raycast target at its own centre,
        // which matters here because a canvas rotated past 90 degrees can present its
        // back face and stop receiving raycasts.
        [UnityTest]
        public IEnumerator Entry1_Symbol_IsTopmostRaycastTarget()
        {
            var entry = OrientationGalleryDefinitions.Entries[0];
            var go = Spawn(entry, 0);
            yield return null;

            var symbolRect = go.transform.Find("Symbol") as RectTransform;
            Assert.IsNotNull(symbolRect);
            Assert.IsTrue(IsTopmostRaycastTarget(symbolRect, _cam), "Symbol should be the topmost raycast target at its own centre");
        }

        // Tier 0.5 - minimum tap target, in projected form (40-testing.md section 4.5 /
        // WCAG 2.5.5). yaw_only at max pitch foreshortens the Symbol; assert its projected
        // screen height still clears 44px at the gallery's reference distance.
        [UnityTest]
        public IEnumerator Entry7_YawOnlyClampPitch_SymbolProjectedHeightMeetsMinimumTapTarget()
        {
            var entry = OrientationGalleryDefinitions.Entries[6]; // yaw_only / clamp pitch on
            var go = Spawn(entry, 0);
            // Reference distance: a plausible close-up AR interaction distance, not the
            // gallery's default 3m (at 1.5m+ with a default ~60deg FOV camera, this 12cm
            // fallback-hierarchy Symbol projects under 10px REGARDLESS of angle - that's
            // just camera/distance geometry, not the orientation-specific foreshortening
            // this check exists to catch; verified empirically before settling on 0.4m).
            const float viewDistance = 0.4f;
            const float elevationDeg = 45f; // steep viewing angle up the wall
            float horizontal = viewDistance * Mathf.Cos(elevationDeg * Mathf.Deg2Rad);
            float vertical = viewDistance * Mathf.Sin(elevationDeg * Mathf.Deg2Rad);
            _cam.transform.position = new Vector3(0f, vertical, -horizontal);
            _cam.transform.LookAt(go.transform.position);
            yield return null;

            var symbolRect = go.transform.Find("Symbol") as RectTransform;
            Assert.IsNotNull(symbolRect);
            Vector3[] corners = new Vector3[4];
            symbolRect.GetWorldCorners(corners);
            Vector2 top = _cam.WorldToScreenPoint(corners[1]);
            Vector2 bottom = _cam.WorldToScreenPoint(corners[0]);
            float projectedHeightPx = Vector2.Distance(top, bottom);

            Assert.GreaterOrEqual(projectedHeightPx, 44f,
                $"Symbol projected height {projectedHeightPx:F1}px is below the 44px WCAG 2.5.5 minimum tap target");
        }

        // Confirms a UI element is genuinely the topmost raycast target at its own centre
        // point -- i.e. actually tappable, not visually present but covered by something
        // else (40-testing.md section 4.5).
        private static bool IsTopmostRaycastTarget(RectTransform target, Camera uiCamera)
        {
            var raycaster = target.GetComponentInParent<GraphicRaycaster>();
            if (raycaster == null || EventSystem.current == null) return false;

            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, target.position);
            var pointerData = new PointerEventData(EventSystem.current) { position = screenPoint };
            var results = new List<RaycastResult>();
            raycaster.Raycast(pointerData, results);

            return results.Count > 0 && results[0].gameObject.transform == target;
        }
    }
}
