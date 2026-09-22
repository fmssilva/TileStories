using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using TileStories;

namespace TileStories.Tests
{
    // Tier-0.5 PlayMode tests for MarkerOverlapResolver.ApplyDisplacement against the real
    // POI_Marker prefab (spec _2.5 Section 12). Category "religious" resolves through KnownIcons
    // (no icon-missing warning) so the guardrail test's LogAssert sees only the displacement warn.
    public class DisplacementLabelTests
    {
        

        private GameObject _camGO;
        private Camera _cam;
        private DisplacementSettings _settings;

        [SetUp]
        public void SetUp()
        {
            MarkerOverlapResolver.ResetWarnings();
            _camGO = new GameObject("MainCamera");
            _camGO.tag = "MainCamera";
            _cam = _camGO.AddComponent<Camera>();
            _cam.transform.position = new Vector3(0f, 0f, -10f);
            _settings = new DisplacementSettings
            {
                enabled = true,
                overlap_threshold_px = 40f,
                max_displacement_px = 120f,
                displacement_algorithm = "fixed_axis",
                displace_target = "label_only",
                displacement_tiebreak = "symmetric"
            };
        }

        [TearDown]
        public void TearDown()
        {
            MarkerOverlapResolver.ResetWarnings();
            Object.DestroyImmediate(_camGO);
        }

        // Spawn a real POI_Marker prefab placed so it projects to `screenPos`.
        private MarkerView SpawnMarker(Vector2 screenPos, string id)
        {
            var prefab = MarkerGalleryTestFixture.LoadPrefab();
            var go = Object.Instantiate(prefab);
            go.transform.position = _cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 10f));
            var anchor = go.AddComponent<POIAnchor>();
            anchor.Initialise(new POIData { id = id, name = id, category = "religious", has_status = false });
            var view = go.GetComponentInChildren<MarkerView>();
            view.Initialise(anchor, MarkerVisualSettings.Default());

            // In tests we disable the animated reveal coroutine and force full scale/alpha
            var reveal = go.GetComponent<MarkerRevealEffect>();
            if (reveal != null)
            {
                reveal.StopAllCoroutines();
                reveal.SetFullAlphaAndScale();
            }

            return view;
        }

        private Vector2 ScreenPos(Vector3 worldPos)
        {
            var sp = _cam.WorldToScreenPoint(worldPos);
            return new Vector2(sp.x, sp.y);
        }

        private static List<VisualUnit> Units(params (MarkerView m, string id)[] ms)
        {
            var list = new List<VisualUnit>();
            foreach (var (m, id) in ms)
                list.Add(new VisualUnit { marker = m, poiId = id, worldPosition = m.transform.position });
            return list;
        }

                // Component-wise Vector2 equality with tolerance. NUnit's 3-arg AreEqual<double>
        // rejects Vector2 (CS1503), so compare axes with the float delta overload.
        private static void AssertVecEqual(Vector2 expected, Vector2 actual, float tolerance = 0.5f)
        {
            Assert.AreEqual(expected.x, actual.x, tolerance);
            Assert.AreEqual(expected.y, actual.y, tolerance);
        }

        [UnityTest]
        public IEnumerator OverlappingMarkers_LabelsSeparatedAboveThreshold()
        {
            Vector2 s = new Vector2(400, 400);
            var m0 = SpawnMarker(s, "m0"); yield return null;
            var m1 = SpawnMarker(s, "m1"); yield return null;
            var m2 = SpawnMarker(s, "m2"); yield return null;

            var stab = new Dictionary<string, DisplacementStabilityState>();
            MarkerOverlapResolver.ApplyDisplacement(Units((m0, "m0"), (m1, "m1"), (m2, "m2")), _cam, _settings, stab); // section-9 cycle 1: provisional
            MarkerOverlapResolver.ApplyDisplacement(Units((m0, "m0"), (m1, "m1"), (m2, "m2")), _cam, _settings, stab); // section-9 cycle 2: commits
            yield return null;

            var ys = new List<float> { ScreenPos(m0.LabelRect.position).y, ScreenPos(m1.LabelRect.position).y, ScreenPos(m2.LabelRect.position).y };
            ys.Sort();
            Assert.AreEqual(40f, ys[1] - ys[0], 1.5f);
            Assert.AreEqual(40f, ys[2] - ys[1], 1.5f);
            Assert.IsTrue(m0.HasLabelOffset && m1.HasLabelOffset && m2.HasLabelOffset);
        }

        [UnityTest]
        public IEnumerator MarkerGlyphPositionUnchanged_OnlyLabelMoves()
        {
            Vector2 s = new Vector2(400, 400);
            var m0 = SpawnMarker(s, "m0"); yield return null;
            var m1 = SpawnMarker(s, "m1"); yield return null;
            Vector2 r0 = ScreenPos(m0.transform.position), r1 = ScreenPos(m1.transform.position);

            var stab = new Dictionary<string, DisplacementStabilityState>();
            MarkerOverlapResolver.ApplyDisplacement(Units((m0, "m0"), (m1, "m1")), _cam, _settings, stab); // section-9 cycle 1: provisional
            MarkerOverlapResolver.ApplyDisplacement(Units((m0, "m0"), (m1, "m1")), _cam, _settings, stab); // section-9 cycle 2: commits
            yield return null;

                        AssertVecEqual(r0, ScreenPos(m0.transform.position));
            AssertVecEqual(r1, ScreenPos(m1.transform.position));
            float labelGap = Mathf.Abs(ScreenPos(m0.LabelRect.position).y - ScreenPos(m1.LabelRect.position).y);
            Assert.Greater(labelGap, 30f);
        }

        
        [UnityTest]
        public IEnumerator IdempotentAcrossCycles_LabelsDoNotDrift()
        {
            Vector2 s = new Vector2(400, 400);
            var m0 = SpawnMarker(s, "m0"); yield return null;
            var m1 = SpawnMarker(s, "m1"); yield return null;
            var m2 = SpawnMarker(s, "m2"); yield return null;
            var units = Units((m0, "m0"), (m1, "m1"), (m2, "m2"));

            var stab = new Dictionary<string, DisplacementStabilityState>();
            // Section 9 hysteresis: overlap-group membership commits across TWO consecutive in-group cycles,
            // so the first in-group observation is provisional and must not displace yet.
            MarkerOverlapResolver.ApplyDisplacement(units, _cam, _settings, stab); // cycle 1: provisional
            MarkerOverlapResolver.ApplyDisplacement(units, _cam, _settings, stab); // cycle 2: commits -> offsets applied
            yield return null;
            Vector2 l0 = ScreenPos(m0.LabelRect.position), l1 = ScreenPos(m1.LabelRect.position), l2 = ScreenPos(m2.LabelRect.position);
            
            // Re-run over the same snapshot from the committed state: absolute anchored positions must not drift.
            MarkerOverlapResolver.ApplyDisplacement(units, _cam, _settings, stab); // cycle 3: held, deterministic re-application
            yield return null;
            
            AssertVecEqual(l0, ScreenPos(m0.LabelRect.position));
            AssertVecEqual(l1, ScreenPos(m1.LabelRect.position));
            AssertVecEqual(l2, ScreenPos(m2.LabelRect.position));
        }

        [UnityTest]
        public IEnumerator StoppedOverlapping_LabelsCleared()
        {
            Vector2 s = new Vector2(400, 400);
            var m0 = SpawnMarker(s, "m0"); yield return null;
            var m1 = SpawnMarker(s, "m1"); yield return null;
            var m2 = SpawnMarker(s, "m2"); yield return null;
            var units = Units((m0, "m0"), (m1, "m1"), (m2, "m2"));

            var stab = new Dictionary<string, DisplacementStabilityState>();
            // Section 9 hysteresis: membership commits across two consecutive cycles before displacing.
            MarkerOverlapResolver.ApplyDisplacement(units, _cam, _settings, stab); // cycle 1: provisional
            MarkerOverlapResolver.ApplyDisplacement(units, _cam, _settings, stab); // cycle 2: commits -> displaced
            yield return null;
            Assert.IsTrue(m0.HasLabelOffset && m1.HasLabelOffset && m2.HasLabelOffset);
            
            // move apart past the 40px overlap threshold -> each becomes a singleton
            m0.transform.position = _cam.ScreenToWorldPoint(new Vector3(0f, 400f, 10f));
            m1.transform.position = _cam.ScreenToWorldPoint(new Vector3(700f, 400f, 10f));
            m2.transform.position = _cam.ScreenToWorldPoint(new Vector3(1400f, 400f, 10f));
            units[0].worldPosition = m0.transform.position;
            units[1].worldPosition = m1.transform.position;
            units[2].worldPosition = m2.transform.position;
            // Section 9: one contrary out-cycle HOLDS the committed displacement; only a
            // second consecutive out-group cycle commits the clear.
            MarkerOverlapResolver.ApplyDisplacement(units, _cam, _settings, stab); // out #1: holds
            MarkerOverlapResolver.ApplyDisplacement(units, _cam, _settings, stab); // out #2: commits out -> cleared
            yield return null;
            
            Assert.IsFalse(m0.HasLabelOffset);
            Assert.IsFalse(m1.HasLabelOffset);
            Assert.IsFalse(m2.HasLabelOffset);
        }

        [UnityTest]
        public IEnumerator ForceDirected_ActiveAndSeparatesLabels()
        {
            _settings.displacement_algorithm = "force_directed";
            _settings.force_directed_iterations = 15;
            Vector2 s = new Vector2(400, 400);
            var m0 = SpawnMarker(s, "m0"); yield return null;
            var m1 = SpawnMarker(s, "m1"); yield return null;
            var m2 = SpawnMarker(s, "m2"); yield return null;

            var stab = new Dictionary<string, DisplacementStabilityState>();
            MarkerOverlapResolver.ApplyDisplacement(Units((m0, "m0"), (m1, "m1"), (m2, "m2")), _cam, _settings, stab); // section-9 cycle 1: provisional
            MarkerOverlapResolver.ApplyDisplacement(Units((m0, "m0"), (m1, "m1"), (m2, "m2")), _cam, _settings, stab); // section-9 cycle 2: commits
            yield return null;

            // force_directed should separate overlapping labels with repulsive forces
            var ys = new List<float> { ScreenPos(m0.LabelRect.position).y, ScreenPos(m1.LabelRect.position).y, ScreenPos(m2.LabelRect.position).y };
            ys.Sort();
            // Labels should be separated by at least some amount (at least one marker should move)
            bool separated = (ys[1] - ys[0]) > 1f || (ys[2] - ys[1]) > 1f;
            Assert.IsTrue(separated, "Force-directed algorithm should separate overlapping labels");
            Assert.IsTrue(m0.HasLabelOffset && m1.HasLabelOffset && m2.HasLabelOffset);
        }

        // Block 5 (_2.1_Marker_Orientation.md): ApplyLabelOffset's roll compensation.
        // rootRollDeg is 0 with an unrolled camera regardless of vertical alignment mode,
        // so the first case proves the default (unrolled) path is unchanged; the second
        // proves the requested screen-space offset still lands correctly once the root is
        // actually rolled (world_up + a rolled camera).
        [UnityTest]
        public IEnumerator ApplyLabelOffset_UnrolledCamera_ScreenOffsetMatchesRequest()
        {
            var m0 = SpawnMarker(new Vector2(400, 300), "m0");
            yield return null; // let MarkerBillboard's default (unrolled) LateUpdate settle

            Vector2 markerCentre = ScreenPos(m0.transform.position);
            var requested = new Vector2(30f, 0f);
            m0.ApplyLabelOffset(_cam, requested);
            yield return null;

            Vector2 actualOffset = ScreenPos(m0.LabelRect.position) - markerCentre;
            AssertVecEqual(requested, actualOffset, 5f);
        }

        [UnityTest]
        public IEnumerator ApplyLabelOffset_WorldUpWithRolledCamera_ScreenOffsetStillMatchesRequest()
        {
            var m0 = SpawnMarker(new Vector2(400, 300), "m0");
            var billboard = m0.GetComponent<MarkerBillboard>();
            Assert.IsNotNull(billboard);
            billboard.Configure(new OrientationSettings { vertical_alignment_mode = "world_up", facing_mode = "always_facing_camera" }, "", null);

            _cam.transform.rotation = Quaternion.Euler(0f, 0f, 45f);
            yield return null; // let LateUpdate resolve the rolled, world_up rotation

            Vector2 markerCentre = ScreenPos(m0.transform.position);
            var requested = new Vector2(30f, 0f);
            m0.ApplyLabelOffset(_cam, requested);
            yield return null;

            Vector2 actualOffset = ScreenPos(m0.LabelRect.position) - markerCentre;
            AssertVecEqual(requested, actualOffset, 5f);
        }
    }
}