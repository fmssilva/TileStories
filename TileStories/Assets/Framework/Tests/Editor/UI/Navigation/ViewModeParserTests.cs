using NUnit.Framework;

namespace TileStories.Tests
{
    // Tier-0 tests for ViewModeParser -- pure string parsing/serialization
    // logic, no Unity lifecycle or scene required.
    // (spec _2.6 section 10)
    public class ViewModeParserTests
    {
        [Test]
        public void Parse_ListString_ReturnsList()
        {
            Assert.AreEqual(ViewModeControl.ViewMode.List, ViewModeParser.Parse("list"));
        }

        [Test]
        public void Parse_MinimapString_ReturnsMinimap()
        {
            Assert.AreEqual(ViewModeControl.ViewMode.Minimap, ViewModeParser.Parse("minimap"));
        }

        [Test]
        public void Parse_CameraHighlightString_ReturnsCameraHighlight()
        {
            Assert.AreEqual(ViewModeControl.ViewMode.CameraHighlight, ViewModeParser.Parse("camera_highlight"));
        }

        [Test]
        public void Parse_NullString_ReturnsList()
        {
            Assert.AreEqual(ViewModeControl.ViewMode.List, ViewModeParser.Parse(null));
        }

        [Test]
        public void Parse_EmptyString_ReturnsList()
        {
            Assert.AreEqual(ViewModeControl.ViewMode.List, ViewModeParser.Parse(""));
        }

        [Test]
        public void Parse_UnknownString_ReturnsList()
        {
            Assert.AreEqual(ViewModeControl.ViewMode.List, ViewModeParser.Parse("unknown"));
        }

        [Test]
        public void Parse_CaseInsensitive()
        {
            Assert.AreEqual(ViewModeControl.ViewMode.List, ViewModeParser.Parse("LIST"));
            Assert.AreEqual(ViewModeControl.ViewMode.Minimap, ViewModeParser.Parse("MINIMAP"));
            Assert.AreEqual(ViewModeControl.ViewMode.CameraHighlight, ViewModeParser.Parse("CAMERA_HIGHLIGHT"));
        }

        [Test]
        public void ToString_List_ReturnsListString()
        {
            Assert.AreEqual("list", ViewModeParser.ToString(ViewModeControl.ViewMode.List));
        }

        [Test]
        public void ToString_Minimap_ReturnsMinimapString()
        {
            Assert.AreEqual("minimap", ViewModeParser.ToString(ViewModeControl.ViewMode.Minimap));
        }

        [Test]
        public void ToString_CameraHighlight_ReturnsCameraHighlightString()
        {
            Assert.AreEqual("camera_highlight", ViewModeParser.ToString(ViewModeControl.ViewMode.CameraHighlight));
        }

        [Test]
        public void RoundTrip_ParseToGetString()
        {
            // Ensure parse -> string -> parse is identity
            foreach (ViewModeControl.ViewMode mode in System.Enum.GetValues(typeof(ViewModeControl.ViewMode)))
            {
                string str = ViewModeParser.ToString(mode);
                ViewModeControl.ViewMode parsed = ViewModeParser.Parse(str);
                Assert.AreEqual(mode, parsed, $"Round trip failed for {mode}");
            }
        }
    }
}
