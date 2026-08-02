using NUnit.Framework;
using UnityEngine;

namespace TileStories.Tests
{
    public class StatusRampTests
    {
        [Test]
        public void Resolve_ZeroPercent_ReturnsFirstLevel()
        {
            var level = StatusRamp.Resolve(0f);
            Assert.AreEqual(0f, level.Pct);
            Assert.AreEqual(new Color(0.890f, 0.741f, 0.447f), level.RingColor);
            Assert.AreEqual("solid", level.RingSpriteKey);
        }

        [Test]
        public void Resolve_HundredPercent_ReturnsLastLevel()
        {
            var level = StatusRamp.Resolve(100f);
            Assert.AreEqual(100f, level.Pct);
            Assert.AreEqual(new Color(0.431f, 0.200f, 0.169f), level.RingColor);
            Assert.AreEqual("dotted", level.RingSpriteKey);
        }

        [Test]
        public void Resolve_IntermediatePercent_ReturnsClosestLevel()
        {
            var level = StatusRamp.Resolve(35f);
            Assert.AreEqual(40f, level.Pct); // 40 is closest to 35
        }

        [Test]
        public void ShadeTowardBlack_ZeroPercent_ReturnsBaseColor()
        {
            Color baseColor = Color.red;
            Color result = StatusRamp.ShadeTowardBlack(baseColor, 0f);
            Assert.AreEqual(baseColor, result);
        }

        [Test]
        public void ShadeTowardBlack_HundredPercent_ReturnsDarkerColor()
        {
            Color baseColor = Color.red;
            Color result = StatusRamp.ShadeTowardBlack(baseColor, 100f);
            // Should be darker (lower saturation and value)
            Color.RGBToHSV(baseColor, out float h, out float s, out float v);
            Color.RGBToHSV(result, out float rh, out float rs, out float rv);
            Assert.AreEqual(h, rh, 0.001f); // Hue preserved
            Assert.Less(rs, s); // Saturation reduced
            Assert.Less(rv, v); // Value reduced
        }

        [Test]
        public void UnknownColor_IsDistinctFromEveryKnownLevel()
        {
            foreach (var level in StatusRamp.Levels)
            {
                Assert.AreNotEqual(StatusRamp.UnknownColor, level.RingColor,
                    $"UnknownColor must never coincide with the {level.Pct}% level -- " +
                    "'unknown' must never be mistakable for a known destruction reading.");
            }
        }
    }
}
