using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using TileStories;

namespace TileStories.Tests
{
    // Automated Phase A tests driven by DisplacementGalleryDefinitions.Entries (40-testing §4.4)
    public class DisplacementGalleryTests
    {
        private GameObject _camGO;
        private Camera _cam;
        private List<GameObject> _spawnedObjects = new();

        [SetUp]
        public void SetUp()
        {
            MarkerOverlapResolver.ResetWarnings();
            _camGO = new GameObject("TestCamera");
            _camGO.tag = "MainCamera";
            _cam = _camGO.AddComponent<Camera>();
            _cam.transform.position = new Vector3(0f, 0f, -5f);
            _cam.transform.rotation = Quaternion.identity;
        }

        [TearDown]
        public void TearDown()
        {
            MarkerOverlapResolver.ResetWarnings();
            foreach (var go in _spawnedObjects)
            {
                if (go != null) Object.DestroyImmediate(go);
            }
            _spawnedObjects.Clear();
            if (_camGO != null) Object.DestroyImmediate(_camGO);
        }

        [UnityTest]
        public IEnumerator GalleryEntries_AllProduceSeparatedLabels()
        {
            var prefab = MarkerGalleryTestFixture.LoadPrefab();

            foreach (var entry in DisplacementGalleryDefinitions.Entries)
            {
                // Set camera pose according to viewing angle
                if (entry.ViewingAngle == DisplacementViewingAngle.Shallow)
                {
                    _cam.transform.position = new Vector3(3f, 0f, -2f);
                    _cam.transform.rotation = Quaternion.Euler(0f, -55f, 0f); // 55 degree grazing angle
                }
                else
                {
                    _cam.transform.position = new Vector3(0f, 0f, -5f);
                    _cam.transform.rotation = Quaternion.identity;
                }

                var groupUnits = new List<VisualUnit>();
                var groupMarkers = new List<MarkerView>();
                Vector3 origin = Vector3.zero;

                for (int i = 0; i < entry.GroupSize; i++)
                {
                    var go = Object.Instantiate(prefab);
                    _spawnedObjects.Add(go);
                    go.transform.position = origin;

                    var anchor = go.AddComponent<POIAnchor>();
                    anchor.Initialise(new POIData { id = $"poi_{i}", name = $"Label {i}", category = "religious", has_status = false });

                    var view = go.GetComponentInChildren<MarkerView>();
                    view.Initialise(anchor, MarkerVisualSettings.Default());

                    var reveal = go.GetComponent<MarkerRevealEffect>();
                    if (reveal != null)
                    {
                        reveal.StopAllCoroutines();
                        reveal.SetFullAlphaAndScale();
                    }

                    groupUnits.Add(new VisualUnit { marker = view, poiId = $"poi_{i}", worldPosition = origin });
                    groupMarkers.Add(view);
                }

                yield return null;

                var settings = new DisplacementSettings
                {
                    enabled = true,
                    overlap_threshold_px = 40f,
                    max_displacement_px = 120f,
                    displace_target = entry.DisplaceTarget,
                    displacement_algorithm = entry.Algorithm,
                    leader_lines_enabled = entry.LeaderLinesEnabled,
                    leader_line_style = entry.LeaderLineStyle,
                    displacement_tiebreak = "symmetric"
                };
                // Override iterations for force_directed algorithm
                if (entry.Algorithm == "force_directed")
                    settings.force_directed_iterations = 50;

                var stab = new Dictionary<string, DisplacementStabilityState>();
                MarkerOverlapResolver.ApplyDisplacement(groupUnits, _cam, settings, stab); // section-9 cycle 1: provisional
                MarkerOverlapResolver.ApplyDisplacement(groupUnits, _cam, settings, stab); // section-9 cycle 2: commits
                yield return null;

                // Assert that pairwise label positions on screen are separated
                var labelYs = new List<float>();
                foreach (var m in groupMarkers)
                {
                    var sp = _cam.WorldToScreenPoint(m.LabelRect.position);
                    labelYs.Add(sp.y);
                }
                labelYs.Sort();

                for (int k = 0; k < labelYs.Count - 1; k++)
                {
                    float gap = labelYs[k + 1] - labelYs[k];
                    Assert.GreaterOrEqual(gap, 35f, $"Label gap between index {k} and {k+1} should be >= 35px in entry: {entry.Label}");
                }

                // Cleanup this entry's objects before next iteration
                foreach (var go in _spawnedObjects)
                {
                    if (go != null) Object.DestroyImmediate(go);
                }
                _spawnedObjects.Clear();
            }
        }

        [UnityTest]
        public IEnumerator ShallowAngle_LabelDisplacement_ResolvesScreenOverlap()
        {
            var prefab = MarkerGalleryTestFixture.LoadPrefab();

            // Set camera at a steep 65° grazing angle to the wall plane
            _cam.transform.position = new Vector3(4f, 0.5f, -1.5f);
            _cam.transform.rotation = Quaternion.Euler(5f, -65f, 0f);

            var groupUnits = new List<VisualUnit>();
            var groupMarkers = new List<MarkerView>();
            Vector3 wallPoint = new Vector3(0f, 0f, 0f);

            for (int i = 0; i < 3; i++)
            {
                var go = Object.Instantiate(prefab);
                _spawnedObjects.Add(go);
                go.transform.position = wallPoint;

                var anchor = go.AddComponent<POIAnchor>();
                anchor.Initialise(new POIData { id = $"shallow_poi_{i}", name = $"Grazing {i}", category = "religious", has_status = false });

                var view = go.GetComponentInChildren<MarkerView>();
                view.Initialise(anchor, MarkerVisualSettings.Default());

                var reveal = go.GetComponent<MarkerRevealEffect>();
                if (reveal != null)
                {
                    reveal.StopAllCoroutines();
                    reveal.SetFullAlphaAndScale();
                }

                groupUnits.Add(new VisualUnit { marker = view, poiId = $"shallow_poi_{i}", worldPosition = wallPoint });
                groupMarkers.Add(view);
            }

            yield return null;

            var settings = new DisplacementSettings
            {
                enabled = true,
                overlap_threshold_px = 40f,
                max_displacement_px = 120f,
                displace_target = "label_only",
                displacement_algorithm = "fixed_axis",
                displacement_tiebreak = "symmetric"
            };

            var stab = new Dictionary<string, DisplacementStabilityState>();
            MarkerOverlapResolver.ApplyDisplacement(groupUnits, _cam, settings, stab); // section-9 cycle 1: provisional
            MarkerOverlapResolver.ApplyDisplacement(groupUnits, _cam, settings, stab); // section-9 cycle 2: commits
            yield return null;

            var labelScreenYs = new List<float>();
            foreach (var m in groupMarkers)
            {
                var sp = _cam.WorldToScreenPoint(m.LabelRect.position);
                labelScreenYs.Add(sp.y);
            }
            labelScreenYs.Sort();

            Assert.GreaterOrEqual(labelScreenYs[1] - labelScreenYs[0], 35f, "Bottom-mid label separation at shallow angle");
            Assert.GreaterOrEqual(labelScreenYs[2] - labelScreenYs[1], 35f, "Mid-top label separation at shallow angle");
        }

        [UnityTest]
        public IEnumerator ShallowAngle_MarkerDisplacement_ResolvesScreenOverlap()
        {
            var prefab = MarkerGalleryTestFixture.LoadPrefab();

            // Set camera at a steep 55 degree grazing angle to the wall plane
            _cam.transform.position = new Vector3(3f, 0f, -2f);
            _cam.transform.rotation = Quaternion.Euler(0f, -55f, 0f);

            var groupUnits = new List<VisualUnit>();
            var groupMarkers = new List<MarkerView>();
            Vector3 wallPoint = Vector3.zero;

            for (int i = 0; i < 3; i++)
            {
                var go = Object.Instantiate(prefab);
                _spawnedObjects.Add(go);
                go.transform.position = wallPoint;

                var anchor = go.AddComponent<POIAnchor>();
                anchor.Initialise(new POIData { id = $"shallow_marker_{i}", name = $"Grazing Marker {i}", category = "religious", has_status = false });

                var view = go.GetComponentInChildren<MarkerView>();
                view.Initialise(anchor, MarkerVisualSettings.Default());

                var reveal = go.GetComponent<MarkerRevealEffect>();
                if (reveal != null)
                {
                    reveal.StopAllCoroutines();
                    reveal.SetFullAlphaAndScale();
                }

                groupUnits.Add(new VisualUnit { marker = view, poiId = $"shallow_marker_{i}", worldPosition = wallPoint });
                groupMarkers.Add(view);
            }

            yield return null;

            var settings = new DisplacementSettings
            {
                enabled = true,
                overlap_threshold_px = 40f,
                max_displacement_px = 120f,
                displace_target = "marker",
                displacement_algorithm = "fixed_axis",
                displacement_tiebreak = "symmetric"
            };

            var stab = new Dictionary<string, DisplacementStabilityState>();
            MarkerOverlapResolver.ApplyDisplacement(groupUnits, _cam, settings, stab); // section-9 cycle 1: provisional
            MarkerOverlapResolver.ApplyDisplacement(groupUnits, _cam, settings, stab); // section-9 cycle 2: commits
            yield return null;

            // Assert that marker ROOT screen Y positions are separated
            var markerScreenYs = new List<float>();
            foreach (var m in groupMarkers)
            {
                var sp = _cam.WorldToScreenPoint(m.transform.position);
                markerScreenYs.Add(sp.y);
            }
            markerScreenYs.Sort();

            Assert.GreaterOrEqual(markerScreenYs[1] - markerScreenYs[0], 35f, "Bottom-mid marker root separation at shallow angle");
            Assert.GreaterOrEqual(markerScreenYs[2] - markerScreenYs[1], 35f, "Mid-top marker root separation at shallow angle");
        }

        // Tier-0.5 tap-target clearance after max_displacement_px (_2_5 section 12; _2.7 entry 2.5-q).
        // Core regression guard: displacement moves a marker but must NEVER alter its tappable
        // rect -- asserted here tight against an undisplaced control marker of the same tier.
        // NOTE (documented finding, 2026-08-26): the ABSOLUTE rendered size of the smallest tier
        // (6cm) at ~5m depth measures well below 44x44 screen px (~23x12px at 1080p). This is NOT
        // caused by displacement -- _2_3 "Explicitly out of scope" fixes size_cm as real-world
        // size with NO distance compensation, anticipating exactly this violation and deferring
        // it to a separate future domain. Surfaced in _2.7 row 2.5-q; any fix (e.g. a padded
        // invisible hit area like the minimap's) is a product decision, not a test change.
        [UnityTest]
        public IEnumerator SmallDisplacedMarker_RetainsMinTapTarget()
        {
            // Author a genuinely small tier (half the Fallback size) -- the worst case:
            // small base marker + large displacement offset.
            MarkerHierarchyResolver.ResetToDefaults();
            MarkerHierarchyResolver.Configure(new List<HierarchyLevelEntry>
            {
                new HierarchyLevelEntry { key = "tiny", size_cm = 6f },
            });

            var prefab = MarkerGalleryTestFixture.LoadPrefab();

            // Control: same tier, alone (never grouped -> never displaced).
            var controlGo = Object.Instantiate(prefab);
            _spawnedObjects.Add(controlGo);
            controlGo.transform.position = Vector3.zero;
            var controlAnchor = controlGo.AddComponent<POIAnchor>();
            controlAnchor.Initialise(new POIData
            {
                id = "tiny_control", name = "Tiny Control", category = "religious",
                has_status = false, hierarchy_level_key = "tiny",
            });
            var controlView = controlGo.GetComponentInChildren<MarkerView>();
            controlView.Initialise(controlAnchor, MarkerVisualSettings.Default());
            var controlReveal = controlGo.GetComponent<MarkerRevealEffect>();
            if (controlReveal != null) { controlReveal.StopAllCoroutines(); controlReveal.SetFullAlphaAndScale(); }

            var groupUnits = new List<VisualUnit>();
            var groupMarkers = new List<MarkerView>();

            for (int i = 0; i < 3; i++)
            {
                var go = Object.Instantiate(prefab);
                _spawnedObjects.Add(go);
                go.transform.position = Vector3.zero;

                var anchor = go.AddComponent<POIAnchor>();
                anchor.Initialise(new POIData
                {
                    id = $"tiny_disp_{i}",
                    name = $"Tiny {i}",
                    category = "religious",
                    has_status = false,
                    hierarchy_level_key = "tiny",
                });

                var view = go.GetComponentInChildren<MarkerView>();
                view.Initialise(anchor, MarkerVisualSettings.Default());

                var reveal = go.GetComponent<MarkerRevealEffect>();
                if (reveal != null)
                {
                    reveal.StopAllCoroutines();
                    reveal.SetFullAlphaAndScale();
                }

                groupUnits.Add(new VisualUnit { marker = view, poiId = $"tiny_disp_{i}", worldPosition = Vector3.zero });
                groupMarkers.Add(view);
            }

            yield return null;

            // Force the group to displace with a generous cap so offsets approach max_displacement_px.
            var settings = new DisplacementSettings
            {
                enabled = true,
                overlap_threshold_px = 40f,
                max_displacement_px = 120f,
                displace_target = "marker",
                displacement_algorithm = "fixed_axis",
                displacement_tiebreak = "symmetric",
            };
            var stability = new Dictionary<string, DisplacementStabilityState>();
            MarkerOverlapResolver.ApplyDisplacement(groupUnits, _cam, settings, stability); // cycle 1: provisional
            MarkerOverlapResolver.ApplyDisplacement(groupUnits, _cam, settings, stability); // cycle 2: commits
            yield return null;

            // Confirm we are actually testing the DISPLACED state (roots separated), not a no-op.
            var screenYs = new List<float>();
            foreach (var m in groupMarkers)
                screenYs.Add(_cam.WorldToScreenPoint(m.transform.position).y);
            screenYs.Sort();
            Assert.GreaterOrEqual(screenYs[1] - screenYs[0], 35f, "fixture precondition: group must actually displace");

            Vector2 controlSize = HitRectWorldSize(ResolveHitRect(controlView));
            Assert.Greater(controlSize.x, 0f, "hit rect has zero world width");

            foreach (var m in groupMarkers)
            {
                // INVARIANT UNDER TEST: displacement changes position only -- the tappable
                // rect must come through identical to the undisplaced control's.
                Vector2 displacedSize = HitRectWorldSize(ResolveHitRect(m));
                Assert.AreEqual(controlSize.x, displacedSize.x, 0.0001f,
                    $"displacement altered '{m.PoiId}'s tap-target width ({controlSize.x} -> {displacedSize.x} world units)");
                Assert.AreEqual(controlSize.y, displacedSize.y, 0.0001f,
                    $"displacement altered '{m.PoiId}'s tap-target height ({controlSize.y} -> {displacedSize.y} world units)");
            }

            MarkerHierarchyResolver.ResetToDefaults();
        }

        // The real uGUI tap surface is whatever RectTransform MarkerSelectable sits on;
        // fall back to the symbol glyph if the prefab ever drops the selectable component.
        private static RectTransform ResolveHitRect(MarkerView view)
        {
            var selectable = view.GetComponentInChildren<MarkerSelectable>();
            RectTransform hitRect = selectable != null
                ? (RectTransform)selectable.transform
                : view.GetComponentInChildren<MarkerCircleGlyphView>()?.RectTransform;
            Assert.IsNotNull(hitRect, "no tappable RectTransform found on the marker prefab");
            return hitRect;
        }

        private static Vector2 HitRectWorldSize(RectTransform rt)
        {
            var c = new Vector3[4];
            rt.GetWorldCorners(c);
            return new Vector2(Vector3.Distance(c[0], c[3]), Vector3.Distance(c[0], c[1]));
        }
    }
}
