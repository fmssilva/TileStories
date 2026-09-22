using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace TileStories.Tests
{
    // Phase A tests for the orientation domain (_2.1_Marker_Orientation.md v4 section
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
        public IEnumerator Entry1_WorldUp_AlwaysFacingCamera_Default_RootUpMatchesGravity_DespiteCameraRoll()
        {
            var entry = OrientationGalleryDefinitions.Entries[0];
            var go = Spawn(entry, 0);
            _cam.transform.rotation = Quaternion.Euler(0f, 0f, 45f); // roll only (see Entry2's own pitch+roll note)
            yield return null;

            Assert.Less(Vector3.Angle(go.transform.rotation * Vector3.up, Vector3.up), 0.5f);
        }

        [UnityTest]
        public IEnumerator Entry2_ScreenUp_AlwaysFacingCamera_RootUpMatchesScreenUp()
        {
            var entry = OrientationGalleryDefinitions.Entries[1];
            var go = Spawn(entry, 0);
            _cam.transform.rotation = Quaternion.Euler(10f, 15f, 20f); // includes roll
            yield return null;

            Vector3 screenUp = MarkerOrientationResolver.ScreenUpWorld(_cam);
            Assert.Less(Vector3.Angle(go.transform.rotation * Vector3.up, screenUp), 0.5f);
        }

        [UnityTest]
        public IEnumerator Entry4_WallFixed_UsesAuthoredLocalRotation()
        {
            var entry = OrientationGalleryDefinitions.Entries[3]; // world_up / wall_fixed
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
            var settings = new OrientationSettings { vertical_alignment_mode = entry.VerticalAlignmentMode, facing_mode = entry.FacingMode };
            var anchor = go.GetComponent<POIAnchor>() ?? go.AddComponent<POIAnchor>();
            anchor.Initialise(new POIData { id = "wallfixed", name = "wallfixed", category = "religious" });
            var view = go.GetComponentInChildren<MarkerView>();
            view.Initialise(anchor, MarkerVisualSettings.Default());
            var billboard = go.GetComponent<MarkerBillboard>() ?? go.AddComponent<MarkerBillboard>();
            billboard.Configure(settings, "", null);

            _cam.transform.rotation = Quaternion.Euler(30f, 40f, 50f); // must not affect wall_fixed
            yield return null;

            Assert.Less(Quaternion.Angle(go.transform.rotation, authored), 0.01f);
        }

        [UnityTest]
        public IEnumerator Entry5_YawOnly_RollHasNoEffect()
        {
            var entry = OrientationGalleryDefinitions.Entries[4]; // world_up / yaw_only
            var go = Spawn(entry, 0);
            yield return null;
            Quaternion beforeRoll = go.transform.rotation;

            _cam.transform.rotation = Quaternion.Euler(0f, 0f, 70f); // roll only, no yaw/pitch change
            yield return null;

            Assert.Less(Quaternion.Angle(beforeRoll, go.transform.rotation), 1f,
                "yaw_only must not react to camera roll at all - only camera position drives its yaw");
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
        public IEnumerator Entry8_WorldUpLabelScreenUp_RootMatchesEntry1()
        {
            var entry1 = OrientationGalleryDefinitions.Entries[0];
            var entry8 = OrientationGalleryDefinitions.Entries[7];
            Assert.AreEqual("world_up", entry8.VerticalAlignmentMode);
            Assert.AreEqual("screen_up", entry8.LabelMode);

            var go1 = Spawn(entry1, 0);
            var go8 = Spawn(entry8, 1);
            _cam.transform.rotation = Quaternion.Euler(8f, 12f, 25f);
            yield return null;

            Assert.Less(Quaternion.Angle(go1.transform.rotation, go8.transform.rotation), 0.1f,
                "an independent Label vertical-alignment override must never change the root's own rotation");
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

        // Tier 0.5 - minimum tap target (40-testing.md section 4.5 / WCAG 2.5.5). A
        // visitor standing almost directly below an always_facing_camera marker is the
        // one geometry that could tip it edge-on; the resolver's hardcoded pitch clamp
        // (MarkerOrientationResolver.AlwaysFacingCameraMaxPitchDeg) exists specifically
        // to stop that. Assert the projected Symbol height still clears 44px there.
        [UnityTest]
        public IEnumerator Entry1_AlwaysFacingCamera_NearCameraBelow_SymbolProjectedHeightMeetsMinimumTapTarget()
        {
            var entry = OrientationGalleryDefinitions.Entries[0]; // world_up / always_facing_camera
            var go = Spawn(entry, 0);
            _cam.transform.position = new Vector3(0.05f, -0.35f, 0f); // almost straight below, close range
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
