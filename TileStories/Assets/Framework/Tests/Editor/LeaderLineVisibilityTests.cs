using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using TileStories;

namespace TileStories.Tests
{
    // Tier-0 EditMode tests for MarkerLeaderLine (spec _2.5 Section 6).
    // Pure component logic: Configure, UpdateVisibility, Evaluate -- no scene, no prefab.
    // LeaderLineEnabled, PositionCount, BaselinePos are internal test seams
    // (InternalsVisibleTo grants TileStories.Editor.Tests access).
    public class LeaderLineVisibilityTests
    {
        private GameObject _go;
        private GameObject _camGO;
        private Camera _cam;
        private RenderTexture _rt;
        private MarkerLeaderLine _leaderLine;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("TestLeaderLine");
            _go.transform.position = Vector3.zero;
            _go.AddComponent<LineRenderer>(); // explicitly add before MarkerLeaderLine
            _leaderLine = _go.AddComponent<MarkerLeaderLine>();

            _camGO = new GameObject("TestCamera");
            _cam = _camGO.AddComponent<Camera>();
            // Assign a render target so WorldToScreenPoint has valid pixel dimensions
            // in EditMode (default pixelWidth/Height are 0 without one).
            _rt = new RenderTexture(640, 480, 24);
            _cam.targetTexture = _rt;
            // Camera 2m back, default FOV 60: 0.3m world displacement -> ~62px screen
            _cam.transform.position = new Vector3(0f, 0f, -2f);
            _cam.transform.rotation = Quaternion.identity;
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
            Object.DestroyImmediate(_camGO);
            Object.DestroyImmediate(_rt);
        }

        [Test]
        public void DisabledInSettings_HidesLineRenderer()
        {
            var settings = new DisplacementSettings { leader_lines_enabled = false };
            _leaderLine.Configure(Vector3.zero, Color.red);
            _leaderLine.UpdateVisibility(_cam, settings);
            _leaderLine.Evaluate();

            Assert.IsFalse(_leaderLine.IsLineEnabled,
                "Leader line should be hidden when leader_lines_enabled is false");
        }

        [Test]
        public void BelowMinDistance_HidesLineRenderer()
        {
            // Marker at baseline: zero displacement, zero screen-space distance
            _go.transform.position = Vector3.zero;
            var settings = new DisplacementSettings
            {
                leader_lines_enabled = true,
                leader_line_min_distance_px = 15f
            };
            _leaderLine.Configure(Vector3.zero, Color.red);
            _leaderLine.UpdateVisibility(_cam, settings);
            _leaderLine.Evaluate();

            Assert.IsFalse(_leaderLine.IsLineEnabled,
                "Leader line should be hidden when displacement is below min_distance_px");
        }

        [Test]
        public void AboveMinDistance_ShowsLineRenderer()
        {
            // 30cm displacement at 2m distance -> ~62px screen distance (> 15 threshold)
            _go.transform.position = new Vector3(0.3f, 0f, 0f);
            var settings = new DisplacementSettings
            {
                leader_lines_enabled = true,
                leader_line_min_distance_px = 15f
            };
            _leaderLine.Configure(Vector3.zero, Color.red);
            _leaderLine.UpdateVisibility(_cam, settings);
            _leaderLine.Evaluate();

            Assert.IsTrue(_leaderLine.IsLineEnabled,
                "Leader line should be visible when displacement exceeds min_distance_px");
        }

        [Test]
        public void StraightStyle_TwoPositions()
        {
            _go.transform.position = new Vector3(0.3f, 0f, 0f);
            var settings = new DisplacementSettings
            {
                leader_lines_enabled = true,
                leader_line_min_distance_px = 15f,
                leader_line_style = "straight"
            };
            _leaderLine.Configure(Vector3.zero, Color.red);
            _leaderLine.UpdateVisibility(_cam, settings);
            _leaderLine.Evaluate();

            Assert.IsTrue(_leaderLine.IsLineEnabled);
            Assert.AreEqual(2, _leaderLine.CurrentPositionCount,
                "Straight style should use 2 LineRenderer positions");
        }

        [Test]
        public void DashedStyle_TwoPositions()
        {
            _go.transform.position = new Vector3(0.3f, 0f, 0f);
            var settings = new DisplacementSettings
            {
                leader_lines_enabled = true,
                leader_line_min_distance_px = 15f,
                leader_line_style = "dashed"
            };
            _leaderLine.Configure(Vector3.zero, Color.red);
            _leaderLine.UpdateVisibility(_cam, settings);
            _leaderLine.Evaluate();

            Assert.IsTrue(_leaderLine.IsLineEnabled);
            Assert.AreEqual(2, _leaderLine.CurrentPositionCount,
                "Dashed style should use 2 LineRenderer positions (dash is material-based)");
        }

        [Test]
        public void ElbowStyle_ThreePositions()
        {
            _go.transform.position = new Vector3(0.3f, 0f, 0f);
            var settings = new DisplacementSettings
            {
                leader_lines_enabled = true,
                leader_line_min_distance_px = 15f,
                leader_line_style = "elbow"
            };
            _leaderLine.Configure(Vector3.zero, Color.red);
            _leaderLine.UpdateVisibility(_cam, settings);
            _leaderLine.Evaluate();

            Assert.IsTrue(_leaderLine.IsLineEnabled);
            Assert.AreEqual(3, _leaderLine.CurrentPositionCount,
                "Elbow style should use 3 LineRenderer positions (right-angle bend)");
        }

        [Test]
        public void Configure_SetsBaselineAndColor()
        {
            var baseline = new Vector3(1f, 2f, 3f);
            _leaderLine.Configure(baseline, Color.blue);

            Assert.IsTrue(_leaderLine.IsConfigured);
            Assert.AreEqual(baseline, _leaderLine.BaselinePos,
                "Configure should store the baseline position");
        }

        [Test]
        public void NotConfigured_HidesLineRenderer()
        {
            var settings = new DisplacementSettings
            {
                leader_lines_enabled = true,
                leader_line_min_distance_px = 0f
            };
            // Intentionally skip Configure -- line should not render
            _go.transform.position = new Vector3(0.3f, 0f, 0f);
            _leaderLine.UpdateVisibility(_cam, settings);
            _leaderLine.Evaluate();

            Assert.IsFalse(_leaderLine.IsLineEnabled,
                "Leader line should be hidden when not configured with a baseline");
        }

        [Test]
        public void NullCamera_NoCrash()
        {
            _go.transform.position = new Vector3(0.3f, 0f, 0f);
            var settings = new DisplacementSettings { leader_lines_enabled = true };
            _leaderLine.Configure(Vector3.zero, Color.red);
            _leaderLine.UpdateVisibility(null, settings);
            _leaderLine.Evaluate();

            Assert.Pass("Evaluate should not throw when camera is null");
        }

        [Test]
        public void NullSettings_NoCrash()
        {
            _go.transform.position = new Vector3(0.3f, 0f, 0f);
            _leaderLine.Configure(Vector3.zero, Color.red);
            _leaderLine.UpdateVisibility(_cam, null);
            _leaderLine.Evaluate();

            Assert.Pass("Evaluate should not throw when settings are null");
        }
    }
}
