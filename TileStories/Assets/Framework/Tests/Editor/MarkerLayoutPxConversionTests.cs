using NUnit.Framework;
using UnityEngine;

namespace TileStories.Tests
{
    // Tier-0 EditMode unit tests for MarkerLayout.ScreenPixelsToWorld conversion math (spec _2.5 §4/§12).
    public class MarkerLayoutPxConversionTests
    {
        private GameObject _camGO;
        private Camera _cam;

        [SetUp]
        public void SetUp()
        {
            _camGO = new GameObject("TestCamera");
            _cam = _camGO.AddComponent<Camera>();
            _cam.fieldOfView = 60f;
            // 1920x1080 target viewport (simulated)
        }

        [TearDown]
        public void TearDown()
        {
            if (_camGO != null)
                Object.DestroyImmediate(_camGO);
        }

        [Test]
        public void ScreenPixelsToWorld_ZeroOffset_ReturnsZero()
        {
            var result = MarkerLayout.ScreenPixelsToWorld(Vector2.zero, 2.0f, _cam);
            Assert.AreEqual(Vector2.zero, result);
        }

        [Test]
        public void ScreenPixelsToWorld_NullCamera_ReturnsZero()
        {
            var result = MarkerLayout.ScreenPixelsToWorld(new Vector2(40f, 40f), 2.0f, null);
            Assert.AreEqual(Vector2.zero, result);
        }

        [Test]
        public void ScreenPixelsToWorld_ZeroOrNegativeDistance_ReturnsZero()
        {
            var zeroDist = MarkerLayout.ScreenPixelsToWorld(new Vector2(40f, 40f), 0f, _cam);
            var negDist = MarkerLayout.ScreenPixelsToWorld(new Vector2(40f, 40f), -1.5f, _cam);
            Assert.AreEqual(Vector2.zero, zeroDist);
            Assert.AreEqual(Vector2.zero, negDist);
        }

        [Test]
        public void ScreenPixelsToWorld_DoublingDistance_DoublesWorldOffset()
        {
            var offset1m = MarkerLayout.ScreenPixelsToWorld(new Vector2(100f, 50f), 1.0f, _cam);
            var offset2m = MarkerLayout.ScreenPixelsToWorld(new Vector2(100f, 50f), 2.0f, _cam);

            Assert.AreEqual(offset1m.x * 2f, offset2m.x, 0.0001f);
            Assert.AreEqual(offset1m.y * 2f, offset2m.y, 0.0001f);
        }

        [Test]
        public void ScreenPixelsToWorld_MatchesTheoreticalFOVFormula()
        {
            float fov = 60f;
            float dist = 2.0f;
            _cam.fieldOfView = fov;

            float worldHeight = 2f * Mathf.Tan(Mathf.Deg2Rad * fov * 0.5f) * dist;
            float expectedPerPixel = worldHeight / _cam.pixelHeight;

            var result = MarkerLayout.ScreenPixelsToWorld(new Vector2(40f, 80f), dist, _cam);
            Assert.AreEqual(40f * expectedPerPixel, result.x, 0.0001f);
            Assert.AreEqual(80f * expectedPerPixel, result.y, 0.0001f);
        }
    }
}
