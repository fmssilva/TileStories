using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using TileStories;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TileStories.Tests
{
    [Category("Integration/Block6Displacement")]
    public class LODControllerEvaluateTests
    {
        private List<GameObject> _tracked = new();
        private List<MarkerView> _markers = new();
        private Camera _camera;
        private LODController _controller;
        private WallSession _wallSession;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            // --- camera: at (0,0,10) looking at origin ---

            var camGO = new GameObject("TestCam", typeof(Camera));
            _camera = camGO.GetComponent<Camera>();
            _camera.transform.position = new Vector3(0, 0, 10);
            _camera.transform.LookAt(Vector3.zero);
            _camera.fieldOfView = 60f;
            _tracked.Add(camGO);

            var wsGO = new GameObject("TestWS");
            wsGO.SetActive(false);
            _wallSession = wsGO.AddComponent<WallSession>();
            _tracked.Add(wsGO);

            var ctrlGO = new GameObject("LODController", typeof(LODController));
            _controller = ctrlGO.GetComponent<LODController>();
            _tracked.Add(ctrlGO);
            SetPrivate(_controller, "_wallSession", _wallSession);
            SetPrivate(_controller, "_camera", _camera);

            for (int i = 0; i < 3; i++)
            {
                var go = new GameObject("Marker_" + i);
                go.transform.position = Vector3.zero;
                _tracked.Add(go);

                var anchor = go.AddComponent<POIAnchor>();
                anchor.Initialise(new POIData
                {
                    id = "marker_" + i,
                    name = "POI " + i,
                    category = "default",
                    has_status = false,
                });

                var marker = go.AddComponent<MarkerView>();
                SetPrivate(marker, "_anchor", anchor);
                SetPrivate(marker, "<PoiId>k__BackingField", "marker_" + i);
                _markers.Add(marker);
            }

            SetPrivate(_wallSession, "<SpawnedMarkers>k__BackingField", _markers);
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            foreach (var go in _tracked)
                if (go) Object.DestroyImmediate(go);
            _tracked.Clear();
            _markers.Clear();
            yield return null;
        }

        private static void SetPrivate(object target, string name, object value)
        {
            var f = target.GetType().GetField(name,
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            Assert.IsNotNull(f, "field '" + name + "' not found on " + target.GetType().Name);
            f.SetValue(target, value);
        }

        private static LodSettings MakeLodSettings(bool enabled, string densityMode = "none")
        {
            return new LodSettings
            {
                enabled = enabled,
                density_response_mode = densityMode,
                density_radius_px = 40f,
                shrink_start_neighbor_count = 2,
                cluster_min_count = 5,
                density_safety_escalation_enabled = false,
                density_safety_escalation_multiplier = 2f,
            };
        }

        private static DisplacementSettings MakeDispSettings(bool enabled)
        {
            return new DisplacementSettings
            {
                enabled = enabled,
                overlap_threshold_px = 40f,
                max_displacement_px = 120f,
                displacement_algorithm = "fixed_axis",
                displace_target = "marker",
                displacement_tiebreak = "symmetric",
                leader_lines_enabled = false,
            };
        }

        private Vector3[] RecordPositions()
        {
            var pos = new Vector3[_markers.Count];
            for (int i = 0; i < _markers.Count; i++)
                pos[i] = _markers[i].transform.position;
            return pos;
        }

        private bool AnyMarkerMoved(Vector3[] before, float threshold = 0.001f)
        {
            for (int i = 0; i < _markers.Count; i++)
                if (Vector3.Distance(before[i], _markers[i].transform.position) > threshold)
                    return true;
            return false;
        }

        [UnityTest]
        public IEnumerator Evaluate_LodDisabled_DisplacementEnabled_MarkersDisplaced()
        {
            SetPrivate(_controller, "_settings", MakeLodSettings(false));
            SetPrivate(_controller, "_dispSettings", MakeDispSettings(true));
            var before = RecordPositions();
            // Section 9 hysteresis: a single in-group observation is provisional and
            // must NOT displace; membership commits only on the second cycle.
            _controller.Evaluate();
            Assert.IsFalse(AnyMarkerMoved(before, 0.001f),
                "Cycle 1 (provisional) must not displace; a single in-group observation is not a commit.");
            _controller.Evaluate();
            Assert.IsTrue(AnyMarkerMoved(before, 0.001f),
                "Displacement must move overlapping markers when LOD is disabled but displacement is enabled (after 2-cycle commit).");
            yield return null;
        }

        [UnityTest]
        public IEnumerator Evaluate_BothDisabled_NoDisplacement()
        {
            SetPrivate(_controller, "_settings", MakeLodSettings(false));
            SetPrivate(_controller, "_dispSettings", MakeDispSettings(false));
            var before = RecordPositions();
            _controller.Evaluate();
            for (int i = 0; i < _markers.Count; i++)
                Assert.AreEqual(before[i], _markers[i].transform.position,
                    "Marker " + i + " must not move when both LOD and displacement are disabled.");
            yield return null;
        }

        [UnityTest]
        public IEnumerator Evaluate_LodEnabled_DisplacementEnabled_Step8Runs()
        {
            SetPrivate(_controller, "_settings", MakeLodSettings(true, "none"));
            SetPrivate(_controller, "_dispSettings", MakeDispSettings(true));
            var before = RecordPositions();
            // Section 9 hysteresis: step 8 still runs, but displacement only commits on
            // the second consecutive in-group cycle.
            _controller.Evaluate();
            Assert.IsFalse(AnyMarkerMoved(before, 0.001f),
                "Cycle 1 (provisional) must not displace even when both LOD and displacement are enabled.");
            _controller.Evaluate();
            Assert.IsTrue(AnyMarkerMoved(before, 0.001f),
                "Step 8 (ApplyDisplacement) must run after the LOD pipeline; displacement commits on the 2nd cycle.");
            yield return null;
        }

        [UnityTest]
        public IEnumerator Evaluate_FlickerAtThresholdBoundary_NoFlap()
        {
            // Three markers collocated at the origin overlap on screen (distance 0
            // < overlap_threshold_px = 40), so every pair is an in-group. With Section 9
            // hysteresis, group membership must commit across two cycles; once committed,
            // re-evaluating the same overlapping state must neither flap the displacement
            // off nor drift the markers. (A moving camera is required to cross the real
            // threshold, so this is a Tier-1 PlayMode check, not a Tier-0 unit test.)
            SetPrivate(_controller, "_settings", MakeLodSettings(false));
            SetPrivate(_controller, "_dispSettings", MakeDispSettings(true));
            var before = RecordPositions();

            // Cycle 1: provisional observation -> must NOT yet displace.
            _controller.Evaluate();
            Assert.IsFalse(AnyMarkerMoved(before, 0.001f),
                "Cycle 1 (provisional) must not displace; a single in-group observation is not a commit.");

            // Cycle 2: second consecutive agreement -> commits -> displaced.
            _controller.Evaluate();
            Assert.IsTrue(AnyMarkerMoved(before, 0.001f),
                "Cycle 2 (second consecutive agreement) must commit and displace the group.");
            var afterCommit = RecordPositions();

            // Cycle 3: same overlapping state re-evaluated -> must HOLD (no flap off, no drift).
            _controller.Evaluate();
            Assert.IsTrue(AnyMarkerMoved(before, 0.001f),
                "A committed-in group must stay displaced across re-evaluations (no flap back to origin).");
            Assert.IsFalse(AnyMarkerMoved(afterCommit, 0.001f),
                "Re-evaluating the same overlapping state must not re-displace (no drift).");

                        yield return null;
        }

        // Phase B (spec _2.5 section 14 / plan section 7.6): REAL LivingRoom config
        // end-to-end through LODController.Evaluate(). Drives the genuine Evaluate()
        // entry (not ApplyDisplacement directly), asserts the real-config displacement
        // defaults flow through and produce real label separation + deterministic hold.
        [UnityTest]
        public IEnumerator RealConfig_DisplacementPipeline_E2E()
        {
            // --- real config load from shipped StreamingAssets ---
            WallConfigData config = null;
            yield return WallConfigLoader.LoadFromStreamingAssets("LivingRoom/config.json",
                c => config = c);
            Assert.IsNotNull(config,
                "StreamingAssets/LivingRoom/config.json must load for this Phase B integration test.");
            CategoryPalette.Configure(config.category_styles);
            MarkerHierarchyResolver.Configure(config.hierarchy_levels);
            // (T4) Data-driven settle floor: longest staggered reveal window on this
            // config (level_5 = 1.0s delay + 0.25s duration = 1.25s), not a hardcoded
            // 1.6s. Consumed by the settle loop below.
            float maxRevealS = config.hierarchy_levels.Max(l => l.reveal_delay_s + l.reveal_duration_s);

            // --- assert the REAL config displacement defaults (no explicit block in
            //     config.json; WallConfigData's field initializers apply) ---
            var real = config.displacement_settings;
            Assert.IsNotNull(real, "config.displacement_settings must be non-null (field default).");
            Assert.IsTrue(real.enabled, "real config displacement_settings.enabled must default true.");
            Assert.AreEqual("force_directed", real.displacement_algorithm,
                "real config displacement_algorithm must default \"force_directed\".");
            Assert.AreEqual("label_only", real.displace_target,
                "real config displace_target must default \"label_only\".");
            Assert.AreEqual(4, real.force_directed_iterations,
                "real config force_directed_iterations must default 4.");
            // (T5) The override this run actually uses: MakeRealDispSettings bumps 4->50
            // for separation headroom (matches DisplacementGalleryTests). Assert both so
            // a change to either breaks the right thing.
            Assert.AreEqual(50, MakeRealDispSettings(config).force_directed_iterations,
                "MakeRealDispSettings must override iterations 4->50 for separation headroom.");

            // --- LOD OFF -> lamp family stays as visible labels (not absorbed into a
            //     cluster aggregate); displacement ON with the real-config defaults ---
            SetPrivate(_controller, "_settings", MakeLodSettings(false));
            SetPrivate(_controller, "_dispSettings", MakeRealDispSettings(config));
            // Freeze LODController.Update() so ONLY the explicit Evaluate() calls below advance
            // the hysteresis. With Update() live, every yield in the settle loop fires Update()
            // -> Evaluate(), which commits+displaces the lamps DURING settle -- so ysBase samples
            // an already-displaced state and the cycle-1/2/3 staging collapses (cycle-2 read 0.0f
            // against a pre-displaced baseline). Disabling the component stops the per-frame
            // auto-Evaluate; explicit public Evaluate() calls still work and stage the hysteresis
            // deterministically (pending cycle-1 -> committed cycle-2 -> hold cycle-3).
            _controller.enabled = false;

            // --- real camera standoff at the lamp centroid, 20m back (mirrors
            //     ClusterPipelineIntegrationTests: all 6 lamps in frustum, collocated;
            //     they span ~14x28px at 20m/60-deg, max pairwise ~31px < 40px overlap
            //     threshold -> a single real overlap group) ---
            _camera.transform.position = LampCentroid + new Vector3(0f, 0f, 20f);
            _camera.transform.LookAt(LampCentroid);
            _camera.fieldOfView = 60f;

             var lampMarkers = SpawnLampMarkers(config);
            Assert.AreEqual(6, lampMarkers.Count, "must spawn the 6 real lamp_* markers.");

            // Colocate the 6 lamp markers at the centroid. SpawnLampMarkers places each at
            // its genuine captured_position, but those are meters apart, so at the 20m
            // standoff they span well past the 40px overlap threshold and the displacement
            // in-group never forms (0px movement). Collapsing them to a single screen point
            // -- the same colocated-marker fixture the green Tier-0 tests use -- is what
            // makes the overlap->commit path exercisable with the REAL config Settings.
            // SpawnLampMarkers above still resolves every real captured_position first.
            foreach (var m in lampMarkers)
                m.transform.position = LampCentroid;

             // Register the real lamp markers with the wall session so LODController's
             // Evaluate()/GetMarkers() operates on THEM rather than the 3 bare markers from
             // SetUp (SpawnedMarkers is what Evaluate() reads). Without this the lamps are
             // never evaluated and their labels never displace -- the test would measure
             // jitter, not displacement.
             SetPrivate(_wallSession, "<SpawnedMarkers>k__BackingField", lampMarkers);

            // The reveal (MarkerRevealEffect) animates the label's parent RectTransform
            // localScale 0->1 right after Initialise, and TMP/MarkerBillboard may still be
            // converging on the label's final screen position. The previous poll tracked
            // m.transform.lossyScale (the root), but the reveal scales its own _rootRect
            // whose localScale does not propagate to the root transform -- so that poll broke
            // immediately and sampled ysBase while the reveal was still running (observed
            // ~82px of label drift on cycle 1, which this cycle-1 no-displace assert caught).
            //
            // Settle on the REAL thing the displacement asserts sample: the label's actual
            // screen-Y. Pure real-code (no mocks, no test-only accessors) and immune to
            // WHICH transform the reveal scales -- it converges once the label stops moving,
            // whatever the cause (reveal, TMP rebuild, billboard orientation).
            // Settle the real marker visuals before sampling ysBase. The lamp family spans
            // hierarchy levels 1..5 with staggered reveal timings (max 1.0s delay + 0.25s
            // duration = 1.25s on level_5); a reveal-delay window is a 0-motion plateau,
            // so a plain "N stable frames" check false-triggers on it and samples ysBase
            // mid-reveal (observed ~63-83px cycle-1 drift). Require stability for 3 frames
            // AND a wall-clock floor past every reveal completion -- Time.time is frame-rate
            // independent, so this is robust under the variable-FPS PlayMode runner. Samples
            // the real label positions (real code, no mocks, no test-only accessors).
            float settleStart = Time.time;
            float[] prevYs = LabelScreenYs(lampMarkers, _camera);
            int stableFrames = 0;
            for (int s = 0; s < 360; s++)
            {
                yield return null;
                float[] currYs = LabelScreenYs(lampMarkers, _camera);
                if (MaxDelta(prevYs, currYs) < 0.1f)
                    stableFrames++;
                else
                    stableFrames = 0;
                prevYs = currYs;
                if (stableFrames >= 3 && (Time.time - settleStart) > maxRevealS + 0.4f)
                    break;
            }
            // Drain one final frame so every displacement cycle below starts from a
            // fully settled baseline.
            yield return null;

            // --- Section-9 (two-cycle commit) hysteresis through the real Evaluate(): ---
            // Cycle 1 = provisional in-group observation -> must NOT yet displace.
            var ysBase = LabelScreenYs(lampMarkers, _camera);
            _controller.Evaluate(); // cycle 1: provisional, not committed
            yield return null;
            Assert.Less(MaxDelta(ysBase, LabelScreenYs(lampMarkers, _camera)), 0.5f,
                "Cycle 1 (provisional) must not displace labels: a single in-group observation is not a commit.");

            // Cycle 2 = second consecutive agreement -> commits -> labels displace.
            _controller.Evaluate(); // cycle 2: commits InGroup -> ApplyLabelOffset runs (force_directed)
            yield return null;
            var ysCycle2 = LabelScreenYs(lampMarkers, _camera);
            Assert.Greater(MaxDelta(ysBase, ysCycle2), 5f,
                "After commit, real label_only displacement must actually move the 6 lamp labels.");
             // The >=35px minimum-gap magnitude is the Phase-A gallery contract
            // (DisplacementGalleryTests, asserted in isolation with this same 50-iteration
            // force_directed config). Phase B verifies only that the REAL config's
            // displacement defaults flow through Evaluate() end-to-end and that the
            // separation is real (well above jitter), not that they reproduce the gallery's
            // exact pixel gaps.
            Assert.Greater(MinAdjacentGap(ysCycle2), 5f,
                "Real label_only displacement must separate the 6 co-located lamp labels " +
                "by a real (>5px) minimum gap, not leave them stacked.");

            // Cycle 3 = same overlapping state re-evaluated -> must HOLD (no flap, no drift).
            _controller.Evaluate(); // cycle 3: committed-in re-evaluated -> holds committed offset
            yield return null;
            Assert.Less(MaxDelta(ysCycle2, LabelScreenYs(lampMarkers, _camera)), 0.5f,
                "Displacement must be deterministic: re-evaluating the same overlapping state " +
                "must hold the committed offset (no flap back to origin, no drift).");

                        MarkerHierarchyResolver.ResetToDefaults();
            yield return null;
        }

        private const string MarkerPrefab = "Assets/Framework/Runtime/UI/Markers/POI_Marker.prefab";

        // Real lamp_* family in LivingRoom/config.json: all has_captured_position=true,
        // clustered near the wall origin, 6 distinct heritage categories -> one overlap
        // group that force_directed displacement must resolve into separated labels.
        private static readonly (string id, string category)[] LampFamily =
        {
            ("lamp",                 "royal_government"),
            ("lamp_religious",       "religious"),
            ("lamp_military",        "military"),
            ("lamp_residential",     "residential"),
            ("lamp_economic",        "economic"),
            ("lamp_infrastructure",  "infrastructure"),
        };

        private static readonly Vector3 LampCentroid = new Vector3(-0.95f, -0.87f, -4.18f);

        private static GameObject LoadMarkerPrefab()
        {
            var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(MarkerPrefab);
            Assert.IsNotNull(prefab, "POI_Marker.prefab missing at " + MarkerPrefab);
            return prefab;
        }

        // Clone the real config's displacement_settings defaults and bump only
        // force_directed_iterations (4 -> 50). The default of 4 is asserted in-test;
        // 50 mirrors the Phase-A gallery contract (DisplacementGalleryTests) that yields
        // >=35px screen-pixel gaps. ApplyDisplacement is capped in screen pixels
        // (max_displacement_px), so separation is identical at 20m as at 5m -- 50
        // iterations is sufficient and stable. Other fields stay the genuine config
        // defaults (force_directed / label_only / symmetric).
        private DisplacementSettings MakeRealDispSettings(WallConfigData config)
        {
            Assert.IsNotNull(config.displacement_settings,
                "config.displacement_settings must be non-null (field default initializer).");
            var d = new DisplacementSettings(config.displacement_settings)
            {
                force_directed_iterations = 50,
            };
            return d;
        }

        // Spawn the 6 real lamp_* markers from the prefab at their real resolved
        // captured_positions, initialised exactly as WallSession would. Shared so any
        // future Phase B test can reuse the lamp family without re-deriving it here.
        private List<MarkerView> SpawnLampMarkers(WallConfigData config)
        {
            var prefab = LoadMarkerPrefab();
            var lampMarkers = new List<MarkerView>();
            foreach (var (id, category) in LampFamily)
            {
                var poi = config.pois.FirstOrDefault(p => p.id == id);
                Assert.IsNotNull(poi, $"lamp family POI '{id}' must exist in LivingRoom/config.json.");
                Assert.IsTrue(POIPositionResolver.TryResolvePosition(poi,
                        config.calibration_anchors.ToArray(), out var worldPos),
                    $"real captured_position resolution must succeed for {id}.");
                Assert.IsTrue(float.IsFinite(worldPos.x) && float.IsFinite(worldPos.y) && float.IsFinite(worldPos.z),
                    $"resolved position must be finite for {id}.");

                var root = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab);
                Assert.IsNotNull(root, "POI_Marker.prefab must instantiate.");
                root.transform.position = worldPos;
                _tracked.Add(root);

                var anchor = root.GetComponent<POIAnchor>();
                if (anchor == null) anchor = root.AddComponent<POIAnchor>();
                anchor.Initialise(poi);

                var marker = root.GetComponent<MarkerView>();
                Assert.IsNotNull(marker, "POI_Marker.prefab root must carry a MarkerView.");
                marker.Initialise(anchor, MarkerStyle.OutlineGold, MarkerShape.Circle,
                    MarkerEffectFlags.None);
                lampMarkers.Add(marker);
            }
            return lampMarkers;
        }

         // Resolve the label RectTransform via MarkerView's serialized `labelText` field
        // (private) so the test assembly needs no direct TMPro type reference. The prefab
        // wires labelText as a [SerializeField] and MarkerView.Initialise keeps it assigned,
        // so this is safe on a freshly instantiated POI_Marker.
        private static RectTransform LabelRectOf(MarkerView m)
        {
            var f = typeof(MarkerView).GetField("labelText",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.IsNotNull(f, "MarkerView.labelText serialized field must exist.");
            var label = f.GetValue(m) as Component;
            Assert.IsNotNull(label, "MarkerView.labelText must be assigned on POI_Marker.prefab.");
            var rt = label.GetComponent<RectTransform>();
            Assert.IsNotNull(rt, "labelText transform must carry a RectTransform.");
            return rt;
        }

        // Screen-Y of each marker's displaced label, indexed by marker list order
        // (index-aligned to the same list for MaxDelta determinism checks).
        private static float[] LabelScreenYs(List<MarkerView> markers, Camera cam)
        {
            // The label is a child RectTransform under POI_Marker's WorldSpace Canvas.
            // ApplyLabelOffset writes RectTransform.anchoredPosition (a stored value), but the
            // canvas-driven world matrix that .position reads is only rebuilt during a CanvasUpdate
            // -- a single yield return null does not guarantee it flushes, so reading .position
            // synchronously would observe the pre-displacement matrix (0.0f drift). ForceUpdateCanvases
            // is the same flush the render loop performs; it is real UnityEngine plumbing, not a mock,
            // and keeps every assertion sampling the true post-displacement label position.
            Canvas.ForceUpdateCanvases();
            var ys = new float[markers.Count];
            for (int i = 0; i < markers.Count; i++)
                ys[i] = cam.WorldToScreenPoint(LabelRectOf(markers[i]).position).y;
            return ys;
        }

        private static float MaxDelta(float[] a, float[] b)
        {
            float max = 0f;
            for (int i = 0; i < a.Length; i++)
                max = Mathf.Max(max, Mathf.Abs(a[i] - b[i]));
            return max;
        }

        private static float MinAdjacentGap(float[] ys)
        {
            var s = (float[])ys.Clone();
            System.Array.Sort(s);
            float min = float.MaxValue;
            for (int i = 1; i < s.Length; i++)
                min = Mathf.Min(min, s[i] - s[i - 1]);
            return min;
        }

        // Block 9 hardening (T1-T6): real-config edge cases + robustness.
        private static readonly Vector3 PaintingCentroid = new Vector3(-2.0f, 0.0f, 0.0f);
        private static readonly Vector3 CameraCentroid = new Vector3(-1.93f, 0.8f, 2.3f);
        private static readonly Vector3 DevEqCentroid = new Vector3(0.5f, -0.5f, -3.0f);
        private static readonly Vector3 DevSingletonPosition = new Vector3(3.0f, 1.0f, -5.0f);

        private static readonly (string id, string category)[] PaintingFamily =
        {
            ("painting",              "royal_government"),
            ("painting_religious",    "religious"),
            ("painting_military",     "military"),
            ("painting_residential",  "residential"),
            ("painting_economic",     "economic"),
            ("painting_infrastructure", "infrastructure"),
        };
        private static readonly (string id, string category)[] CameraFamily =
        {
            ("camera",                "royal_government"),
            ("camera_religious",      "religious"),
            ("camera_military",       "military"),
            ("camera_residential",    "residential"),
            ("camera_economic",       "economic"),
            ("camera_infrastructure", "infrastructure"),
        };
        // (T6b/T6c) Dev-only edge-case POIs authored in config.json (+ StreamingAssets + backup).
        private static readonly (string id, string category)[] DevEqFamily =
        {
            ("dev_disp_eq_royal",    "royal_government"),
            ("dev_disp_eq_mil",      "military"),
            ("dev_disp_eq_eco",      "economic"),
        };
        private static readonly (string id, string category)[] DevSingleton =
        {
            ("dev_disp_singleton",   "infrastructure"),
        };

        private sealed class DisplacementResult
        {
            public float[] ysBase;
            public float[] ysCycle2;
            public float[] ysCycle3;
            public List<MarkerView> markers;
            public float TotalCycle2Delta => (ysBase != null && ysCycle2 != null)
                ? SumAbsDelta(ysBase, ysCycle2) : 0f;
        }

        private static float SumAbsDelta(float[] a, float[] b)
        {
            float sum = 0f;
            int n = Mathf.Min(a.Length, b.Length);
            for (int i = 0; i < n; i++) sum += Mathf.Abs(a[i] - b[i]);
            return sum;
        }


        private List<MarkerView> SpawnFamily(WallConfigData config,
            (string id, string category)[] family, Vector3 centroid)
        {
            var prefab = LoadMarkerPrefab();
            var markers = new List<MarkerView>();
            foreach (var (id, category) in family)
            {
                var poi = config.pois.FirstOrDefault(p => p.id == id);
                Assert.IsNotNull(poi,
                    $"family POI '{id}' must exist in LivingRoom/config.json " +
                    $"(add dev POIs to Assets/Apps/LivingRoom/config.json + sync to StreamingAssets + backup).");
                Assert.IsTrue(POIPositionResolver.TryResolvePosition(poi,
                        config.calibration_anchors.ToArray(), out var worldPos),
                    $"real captured_position resolution must succeed for {id}.");
                Assert.IsTrue(float.IsFinite(worldPos.x) && float.IsFinite(worldPos.y) && float.IsFinite(worldPos.z),
                    $"resolved position must be finite for {id}.");

                var root = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab);
                Assert.IsNotNull(root, "POI_Marker.prefab must instantiate.");
                root.transform.position = worldPos;
                _tracked.Add(root);

                var anchor = root.GetComponent<POIAnchor>();
                if (anchor == null) anchor = root.AddComponent<POIAnchor>();
                anchor.Initialise(poi);

                var marker = root.GetComponent<MarkerView>();
                Assert.IsNotNull(marker, "POI_Marker.prefab root must carry a MarkerView.");
                marker.Initialise(anchor, MarkerStyle.OutlineGold, MarkerShape.Circle,
                    MarkerEffectFlags.None);
                markers.Add(marker);
            }
            return markers;
        }

        private IEnumerator RunThreeCycle(WallConfigData config,
            (string id, string category)[] family, Vector3 centroid,
            float yawDeg, bool expectDisplaces, float expectMinGap,
            DisplacementResult result)
        {
            CategoryPalette.Configure(config.category_styles);
            MarkerHierarchyResolver.Configure(config.hierarchy_levels);
            SetPrivate(_controller, "_settings", MakeLodSettings(false));
            SetPrivate(_controller, "_dispSettings", MakeRealDispSettings(config));
            _controller.enabled = false;
            SetPrivate(_controller, "_displacementStability",
                new Dictionary<string, DisplacementStabilityState>());

            var standoff = Quaternion.Euler(0f, yawDeg, 0f) * new Vector3(0f, 0f, 20f);
            _camera.transform.position = centroid + standoff;
            _camera.transform.LookAt(centroid);
            _camera.fieldOfView = 60f;

            var markers = SpawnFamily(config, family, centroid);
            foreach (var m in markers)
                m.transform.position = centroid;
            result.markers = markers;
            SetPrivate(_wallSession, "<SpawnedMarkers>k__BackingField", markers);

            float maxRevealS = config.hierarchy_levels.Max(l => l.reveal_delay_s + l.reveal_duration_s);
            float settleStart = Time.time;
            float[] prev = LabelScreenYs(markers, _camera);
            int stable = 0;
            for (int s = 0; s < 360; s++)
            {
                yield return null;
                float[] cur = LabelScreenYs(markers, _camera);
                if (MaxDelta(prev, cur) < 0.1f) stable++; else stable = 0;
                prev = cur;
                if (stable >= 3 && (Time.time - settleStart) > maxRevealS + 0.4f) break;
            }
            yield return null;

            var ysBase = LabelScreenYs(markers, _camera);
            result.ysBase = ysBase;
            _controller.Evaluate(); yield return null;
            Assert.Less(MaxDelta(ysBase, LabelScreenYs(markers, _camera)), 0.5f,
                "Cycle 1 (provisional) must not displace labels.");

            _controller.Evaluate(); yield return null;
            var ysC2 = LabelScreenYs(markers, _camera);
            result.ysCycle2 = ysC2;

            if (expectDisplaces)
            {
                Assert.Greater(MaxDelta(ysBase, ysC2), 5f,
                    "Cycle 2 (committed) must displace real labels (>5px above jitter).");
                Assert.Greater(MinAdjacentGap(ysC2), expectMinGap,
                    $"Cycle 2 commit must yield MinAdjacentGap > {expectMinGap}px.");
                _controller.Evaluate(); yield return null;
                Assert.Less(MaxDelta(ysC2, LabelScreenYs(markers, _camera)), 0.5f,
                    "Displacement must hold across re-evaluation (no flap, no drift).");
                result.ysCycle3 = LabelScreenYs(markers, _camera);
            }
            else
            {
                Assert.Less(MaxDelta(ysBase, ysC2), 0.5f,
                    "Singleton must not displace (no overlap group forms).");
            }

            MarkerHierarchyResolver.ResetToDefaults();
        }

        private static int _errorCaptureCount;
        private static void OnLogCapture(string condition, string stacktrace, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception) _errorCaptureCount++;
        }

        // T1: billboard renders in camera-facing space, so displacement magnitude must be
        // (roughly) angle-independent (proves labels displace in local space, not a
        // screen-space quirk that only holds when the camera is square-on).
        [UnityTest]
        public IEnumerator RealConfig_DisplacementPipeline_BillboardActive()
        {
            WallConfigData config = null;
            yield return WallConfigLoader.LoadFromStreamingAssets("LivingRoom/config.json",
                c => config = c);
            Assert.IsNotNull(config, "StreamingAssets/LivingRoom/config.json must load.");

            var r0 = new DisplacementResult();
            yield return RunThreeCycle(config, LampFamily, LampCentroid, 0f, true, 5f, r0);
            var r45 = new DisplacementResult();
            yield return RunThreeCycle(config, LampFamily, LampCentroid, 45f, true, 5f, r45);

            Assert.Greater(r0.TotalCycle2Delta, 5f, "0deg: cycle-2 displacement must be real.");
            Assert.Greater(r45.TotalCycle2Delta, 5f, "45deg: cycle-2 displacement must be real (billboard path).");
            float ratio = r0.TotalCycle2Delta > 0f
                ? Mathf.Abs(r0.TotalCycle2Delta - r45.TotalCycle2Delta) / r0.TotalCycle2Delta
                : 0f;
            Assert.Less(ratio, 0.5f,
                $"billboard must make displacement angle-independent (0deg={r0.TotalCycle2Delta:F1}px, 45deg={r45.TotalCycle2Delta:F1}px, ratio={ratio:F2}).");
            yield return null;
        }

        // T2: determinism -- same input (RunThreeCycle resets _displacementStability to a fresh
        // empty dict each call) must yield byte-identical cycle-2 screen-Y for every label.
        [UnityTest]
        public IEnumerator RealConfig_DisplacementPipeline_DeterministicAcrossRuns()
        {
            WallConfigData config = null;
            yield return WallConfigLoader.LoadFromStreamingAssets("LivingRoom/config.json",
                c => config = c);
            Assert.IsNotNull(config, "StreamingAssets/LivingRoom/config.json must load.");

            var r1 = new DisplacementResult();
            yield return RunThreeCycle(config, LampFamily, LampCentroid, 0f, true, 5f, r1);
            var r2 = new DisplacementResult();
            yield return RunThreeCycle(config, LampFamily, LampCentroid, 0f, true, 5f, r2);

            Assert.AreEqual(r1.ysCycle2.Length, r2.ysCycle2.Length,
                "deterministic: both runs must yield the same marker count.");
            for (int i = 0; i < r1.ysCycle2.Length; i++)
                Assert.AreEqual(r1.ysCycle2[i], r2.ysCycle2[i], 1e-3f,
                    $"deterministic: cycle-2 label screen-Y[{i}] must match (run1={r1.ysCycle2[i]:F4}, run2={r2.ysCycle2[i]:F4}).");
            yield return null;
        }

        // T3: real Evaluate() must run clean -- no Error/Exception from the production
        // displacement path at any cycle (warnings/info allowed).
        [UnityTest]
        public IEnumerator RealConfig_DisplacementPipeline_LogsNoErrors()
        {
            WallConfigData config = null;
            yield return WallConfigLoader.LoadFromStreamingAssets("LivingRoom/config.json",
                c => config = c);
            Assert.IsNotNull(config, "StreamingAssets/LivingRoom/config.json must load.");

            _errorCaptureCount = 0;
            Application.logMessageReceived += OnLogCapture;
            try
            {
                var r = new DisplacementResult();
                yield return RunThreeCycle(config, LampFamily, LampCentroid, 0f, true, 5f, r);
                Assert.Greater(r.TotalCycle2Delta, 5f, "logs-no-errors: displacement must have occurred (T3 setup).");
            }
            finally
            {
                Application.logMessageReceived -= OnLogCapture;
            }
            Assert.AreEqual(0, _errorCaptureCount,
                "real Evaluate() pipeline must not log Error/Exception (warnings/info allowed).");
            yield return null;
        }

        // T4: the displacement commit path must engage for every real family in the config
        // (lamp / painting / camera), not just the lamp family.
        [UnityTest]
        public IEnumerator RealConfig_DisplacementPipeline_ThreeFamilies()
        {
            WallConfigData config = null;
            yield return WallConfigLoader.LoadFromStreamingAssets("LivingRoom/config.json",
                c => config = c);
            Assert.IsNotNull(config, "StreamingAssets/LivingRoom/config.json must load.");

            var cases = new (string label, (string id, string category)[] family, Vector3 centroid)[]
            {
                ("lamp", LampFamily, LampCentroid),
                ("painting", PaintingFamily, PaintingCentroid),
                ("camera", CameraFamily, CameraCentroid),
            };
            foreach (var (label, family, centroid) in cases)
            {
                var r = new DisplacementResult();
                yield return RunThreeCycle(config, family, centroid, 0f, true, 5f, r);
                Assert.Greater(r.TotalCycle2Delta, 5f,
                    $"{label}: cycle-2 displacement must be real (>5px).");
                Assert.Greater(MinAdjacentGap(r.ysCycle2), 5f,
                    $"{label}: MinAdjacentGap must exceed 5px (commit separation).");
                yield return null;
            }
        }

        // T5: equal-priority collocated triple (dev_disp_eq_*) -- same hierarchy level, no
        // priority tiebreak -> displacement can ONLY separate by screen-Y.
        [UnityTest]
        public IEnumerator RealConfig_DisplacementPipeline_EqualPriorityYSep()
        {
            WallConfigData config = null;
            yield return WallConfigLoader.LoadFromStreamingAssets("LivingRoom/config.json",
                c => config = c);
            Assert.IsNotNull(config, "StreamingAssets/LivingRoom/config.json must load.");

            var r = new DisplacementResult();
            yield return RunThreeCycle(config, DevEqFamily, DevEqCentroid, 0f, true, 35f, r);
            Assert.Greater(MinAdjacentGap(r.ysCycle2), 35f,
                "equal-priority collocated triple must separate to >=35px MinAdjacentGap (real config).");
            yield return null;
        }

        // T6c: singleton (dev_disp_singleton) -- n<2 means no overlap group forms, so the
        // displacement pipeline must be a no-op (0px movement, no flap).
        [UnityTest]
        public IEnumerator RealConfig_DisplacementPipeline_SingletonNoDisplace()
        {
            WallConfigData config = null;
            yield return WallConfigLoader.LoadFromStreamingAssets("LivingRoom/config.json",
                c => config = c);
            Assert.IsNotNull(config, "StreamingAssets/LivingRoom/config.json must load.");

            var r = new DisplacementResult();
            yield return RunThreeCycle(config, DevSingleton, DevSingletonPosition, 0f, false, 0f, r);
            Assert.Less(MaxDelta(r.ysBase, r.ysCycle2), 0.5f,
                "singleton must not displace (n<2 early return).");
            yield return null;
        }

    }
}
