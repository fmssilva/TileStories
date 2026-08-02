using NUnit.Framework;

namespace TileStories.Tests
{
    public class MarkerVisualsParserTests
    {
        [Test]
        public void ParseStyle_ValidString_ReturnsCorrectEnum()
        {
            Assert.AreEqual(MarkerStyle.OutlineGold, MarkerVisualsParser.ParseStyle("outline_gold"));
            Assert.AreEqual(MarkerStyle.OutlineSameHue, MarkerVisualsParser.ParseStyle("outline_same_hue"));
            Assert.AreEqual(MarkerStyle.Badge, MarkerVisualsParser.ParseStyle("badge"));
        }

        [Test]
        public void ParseStyle_InvalidString_ReturnsDefault()
        {
            Assert.AreEqual(MarkerStyle.OutlineGold, MarkerVisualsParser.ParseStyle("invalid"));
            Assert.AreEqual(MarkerStyle.OutlineGold, MarkerVisualsParser.ParseStyle(null));
            Assert.AreEqual(MarkerStyle.OutlineGold, MarkerVisualsParser.ParseStyle(""));
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
    }
}