using NUnit.Framework;
using UnityEngine;

namespace TileStories.Tests
{
    public class MarkerVisualsParserTests
    {
        [Test]
        public void TryParseOutlineMode_ValidString_ReturnsCorrectEnum()
        {
            Assert.IsTrue(MarkerVisualsParser.TryParseOutlineMode("uniform", out var uniform));
            Assert.AreEqual(MarkerOutlineMode.Uniform, uniform);
            Assert.IsTrue(MarkerVisualsParser.TryParseOutlineMode("same_hue", out var sameHue));
            Assert.AreEqual(MarkerOutlineMode.SameHue, sameHue);
            Assert.IsTrue(MarkerVisualsParser.TryParseOutlineMode("per_type", out var perType));
            Assert.AreEqual(MarkerOutlineMode.PerType, perType);
            Assert.IsTrue(MarkerVisualsParser.TryParseOutlineMode("none", out var none));
            Assert.AreEqual(MarkerOutlineMode.None, none);
        }

        [Test]
        public void TryParseOutlineMode_InvalidOrMissing_ReturnsFalse()
        {
            Assert.IsFalse(MarkerVisualsParser.TryParseOutlineMode("gold", out _),
                "gold was renamed to uniform (P5): the old string is no longer understood.");
            Assert.IsFalse(MarkerVisualsParser.TryParseOutlineMode("free_colors", out _),
                "free_colors was removed as a runtime outline mode; the editor no longer offers it.");
            Assert.IsFalse(MarkerVisualsParser.TryParseOutlineMode("invalid", out _));
            Assert.IsFalse(MarkerVisualsParser.TryParseOutlineMode(null, out _));
            Assert.IsFalse(MarkerVisualsParser.TryParseOutlineMode("", out _));
        }

        [Test]
        public void ParseShape_ValidString_ReturnsCorrectEnum()
        {
            Assert.AreEqual(MarkerShape.Circle, MarkerVisualsParser.ParseShape("circle"));
            Assert.AreEqual(MarkerShape.RoundedSquare, MarkerVisualsParser.ParseShape("rounded_square"));
            Assert.AreEqual(MarkerShape.Hexagon, MarkerVisualsParser.ParseShape("hexagon"));
            Assert.AreEqual(MarkerShape.Diamond, MarkerVisualsParser.ParseShape("diamond"));
            Assert.AreEqual(MarkerShape.Star, MarkerVisualsParser.ParseShape("star"));
        }

        [Test]
        public void ParseShape_InvalidString_ReturnsDefault()
        {
            Assert.AreEqual(MarkerShape.Circle, MarkerVisualsParser.ParseShape("invalid"));
            Assert.AreEqual(MarkerShape.Circle, MarkerVisualsParser.ParseShape(null));
            Assert.AreEqual(MarkerShape.Circle, MarkerVisualsParser.ParseShape(""));
        }

        [Test]
        public void TryParseBadgeCorner_ValidString_ReturnsCorrectDirection()
        {
            Assert.IsTrue(MarkerVisualsParser.TryParseBadgeCorner("top_right", out var topRight));
            Assert.AreEqual(new Vector2(0.7f, 0.7f), topRight);
            Assert.IsTrue(MarkerVisualsParser.TryParseBadgeCorner("bottom_left", out var bottomLeft));
            Assert.AreEqual(new Vector2(-0.7f, -0.7f), bottomLeft);
        }

        [Test]
        public void TryParseBadgeCorner_InvalidString_ReturnsFalseAndTopRightDefault()
        {
            Assert.IsFalse(MarkerVisualsParser.TryParseBadgeCorner("invalid", out var direction));
            Assert.AreEqual(new Vector2(0.7f, 0.7f), direction);
        }
    }
}
