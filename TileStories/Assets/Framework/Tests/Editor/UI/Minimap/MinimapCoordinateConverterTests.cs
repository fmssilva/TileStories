using NUnit.Framework;
using UnityEngine;

namespace TileStories.Tests
{
    // Tier-0 tests for MinimapCoordinateConverter -- pure math, no Unity
    // lifecycle, no scene required. Validates coordinate conversion from
    // normalized wall coords to minimap pixel positions.
    public class MinimapCoordinateConverterTests
    {
        private const float MAP_WIDTH = 200f;
        private const float MAP_HEIGHT = 200f;
        private const float DOT_SIZE = 20f;

        [Test]
        public void ConvertToPixel_TopLeftCorner_MapsToZeroZero()
        {
            // x_norm=0, y_norm=1 (top-left in wall coords) -> bottom-left in UI
            Vector2 pos = MinimapCoordinateConverter.ConvertToPixel(0f, 1f, MAP_WIDTH, MAP_HEIGHT, DOT_SIZE);
            Assert.AreEqual(0f - DOT_SIZE / 2f, pos.x, 0.001f);
            Assert.AreEqual(0f - DOT_SIZE / 2f, pos.y, 0.001f);
        }

        [Test]
        public void ConvertToPixel_BottomRightCorner_MapsToFullWidthHeight()
        {
            // x_norm=1, y_norm=0 (bottom-right in wall coords) -> top-right in UI
            Vector2 pos = MinimapCoordinateConverter.ConvertToPixel(1f, 0f, MAP_WIDTH, MAP_HEIGHT, DOT_SIZE);
            Assert.AreEqual(MAP_WIDTH - DOT_SIZE / 2f, pos.x, 0.001f);
            Assert.AreEqual(MAP_HEIGHT - DOT_SIZE / 2f, pos.y, 0.001f);
        }

        [Test]
        public void ConvertToPixel_Center_MapsToCenter()
        {
            Vector2 pos = MinimapCoordinateConverter.ConvertToPixel(0.5f, 0.5f, MAP_WIDTH, MAP_HEIGHT, DOT_SIZE);
            Assert.AreEqual(MAP_WIDTH / 2f - DOT_SIZE / 2f, pos.x, 0.001f);
            Assert.AreEqual(MAP_HEIGHT / 2f - DOT_SIZE / 2f, pos.y, 0.001f);
        }

        [Test]
        public void ConvertToPixel_QuarterPoint_MapsCorrectly()
        {
            // x_norm=0.25, y_norm=0.75 (wall coords) -> x=50, y=50 in UI (inverted Y)
            Vector2 pos = MinimapCoordinateConverter.ConvertToPixel(0.25f, 0.75f, MAP_WIDTH, MAP_HEIGHT, DOT_SIZE);
            Assert.AreEqual(50f - DOT_SIZE / 2f, pos.x, 0.001f);
            Assert.AreEqual(50f - DOT_SIZE / 2f, pos.y, 0.001f);
        }

        [Test]
        public void ConvertToPixelRaw_NoCentering_Offset()
        {
            Vector2 pos = MinimapCoordinateConverter.ConvertToPixelRaw(0.5f, 0.5f, MAP_WIDTH, MAP_HEIGHT);
            Assert.AreEqual(100f, pos.x, 0.001f);
            Assert.AreEqual(100f, pos.y, 0.001f);
        }

        [Test]
        public void ConvertToPixelRaw_Corner_MapsToEdge()
        {
            // y_norm=1 -> y = 0 (top of minimap, inverted)
            Vector2 pos = MinimapCoordinateConverter.ConvertToPixelRaw(0f, 1f, MAP_WIDTH, MAP_HEIGHT);
            Assert.AreEqual(0f, pos.x, 0.001f);
            Assert.AreEqual(0f, pos.y, 0.001f);
        }

        [Test]
        public void ClampNorm_ValueWithinRange_ReturnsValue()
        {
            Assert.AreEqual(0.5f, MinimapCoordinateConverter.ClampNorm(0.5f), 0.001f);
        }

        [Test]
        public void ClampNorm_ValueAboveOne_ClampsToOne()
        {
            Assert.AreEqual(1f, MinimapCoordinateConverter.ClampNorm(1.5f), 0.001f);
        }

        [Test]
        public void ClampNorm_ValueBelowZero_ClampsToZero()
        {
            Assert.AreEqual(0f, MinimapCoordinateConverter.ClampNorm(-0.5f), 0.001f);
        }

        [Test]
        public void ClampNorm_ExactZeroAndOne_ReturnsUnchanged()
        {
            Assert.AreEqual(0f, MinimapCoordinateConverter.ClampNorm(0f), 0.001f);
            Assert.AreEqual(1f, MinimapCoordinateConverter.ClampNorm(1f), 0.001f);
        }
    }
}
