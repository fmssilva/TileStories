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
                    view.Initialise(anchor, MarkerStyle.OutlineGold, MarkerShape.Circle);

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
                view.Initialise(anchor, MarkerStyle.OutlineGold, MarkerShape.Circle);

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
                view.Initialise(anchor, MarkerStyle.OutlineGold, MarkerShape.Circle);

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
    }
}
