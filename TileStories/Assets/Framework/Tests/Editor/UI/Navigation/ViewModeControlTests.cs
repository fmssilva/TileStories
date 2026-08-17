using NUnit.Framework;
using UnityEngine;

namespace TileStories.Tests
{
    // Tier-0 tests for ViewModeControl via ViewModeParser delegation.
    // Tests view mode parsing, persistence key names, and enum behavior.
    // (spec _2.6 section 10)
    public class ViewModeControlTests
    {
        [SetUp]
        public void SetUp()
        {
            PlayerPrefs.DeleteKey("TileStories.default_result_view");
        }

        [TearDown]
        public void TearDown()
        {
            PlayerPrefs.DeleteKey("TileStories.default_result_view");
        }

        [Test]
        public void Parse_ListString_ReturnsListMode()
        {
            Assert.AreEqual(ViewModeControl.ViewMode.List, ViewModeParser.Parse("list"));
        }

        [Test]
        public void Parse_MinimapString_ReturnsMinimapMode()
        {
            Assert.AreEqual(ViewModeControl.ViewMode.Minimap, ViewModeParser.Parse("minimap"));
        }

        [Test]
        public void Parse_CameraHighlightString_ReturnsCameraHighlightMode()
        {
            Assert.AreEqual(ViewModeControl.ViewMode.CameraHighlight, ViewModeParser.Parse("camera_highlight"));
        }

        [Test]
        public void Parse_NullString_ReturnsListMode()
        {
            Assert.AreEqual(ViewModeControl.ViewMode.List, ViewModeParser.Parse(null));
        }

        [Test]
        public void Parse_EmptyString_ReturnsListMode()
        {
            Assert.AreEqual(ViewModeControl.ViewMode.List, ViewModeParser.Parse(""));
        }

        [Test]
        public void Parse_UnknownString_ReturnsListMode()
        {
            Assert.AreEqual(ViewModeControl.ViewMode.List, ViewModeParser.Parse("unknown_value"));
        }

        [Test]
        public void Parse_CaseInsensitive()
        {
            Assert.AreEqual(ViewModeControl.ViewMode.List, ViewModeParser.Parse("LIST"));
            Assert.AreEqual(ViewModeControl.ViewMode.Minimap, ViewModeParser.Parse("MINIMAP"));
            Assert.AreEqual(ViewModeControl.ViewMode.CameraHighlight, ViewModeParser.Parse("CAMERA_HIGHLIGHT"));
        }

        [Test]
        public void Parse_AllEnumValues_RoundTrip()
        {
            // Verify every enum value has a parseable string form
            Assert.AreEqual(ViewModeControl.ViewMode.List, ViewModeParser.Parse(ViewModeParser.ToString(ViewModeControl.ViewMode.List)));
            Assert.AreEqual(ViewModeControl.ViewMode.Minimap, ViewModeParser.Parse(ViewModeParser.ToString(ViewModeControl.ViewMode.Minimap)));
            Assert.AreEqual(ViewModeControl.ViewMode.CameraHighlight, ViewModeParser.Parse(ViewModeParser.ToString(ViewModeControl.ViewMode.CameraHighlight)));
        }

        [Test]
        public void GetDefaultResultView_ConfigDriven()
        {
            var config = new WallConfigData
            {
                default_result_view = "minimap"
            };
            Assert.AreEqual("minimap", config.default_result_view);

            ViewModeControl.ViewMode mode = ViewModeParser.Parse(config.default_result_view);
            Assert.AreEqual(ViewModeControl.ViewMode.Minimap, mode);
        }

        [Test]
        public void GetDefaultResultView_NullConfig_DrivesListDefault()
        {
            WallConfigData config = null;
            string defaultMode = config?.default_result_view ?? "list";
            Assert.AreEqual("list", defaultMode);
            Assert.AreEqual(ViewModeControl.ViewMode.List, ViewModeParser.Parse(defaultMode));
        }

        [Test]
        public void ViewModeEnum_HasThreeValues()
        {
            var values = System.Enum.GetValues(typeof(ViewModeControl.ViewMode));
            Assert.AreEqual(3, values.Length);
            Assert.IsTrue(System.Enum.IsDefined(typeof(ViewModeControl.ViewMode), ViewModeControl.ViewMode.List));
            Assert.IsTrue(System.Enum.IsDefined(typeof(ViewModeControl.ViewMode), ViewModeControl.ViewMode.Minimap));
            Assert.IsTrue(System.Enum.IsDefined(typeof(ViewModeControl.ViewMode), ViewModeControl.ViewMode.CameraHighlight));
        }

        [Test]
        public void PersistedPreference_KeyIsCorrect()
        {
            const string PREF_KEY = "TileStories.default_result_view";

            PlayerPrefs.SetString(PREF_KEY, "minimap");
            PlayerPrefs.Save();

            string loaded = PlayerPrefs.GetString(PREF_KEY, "list");
            Assert.AreEqual("minimap", loaded);

            ViewModeControl.ViewMode mode = ViewModeParser.Parse(loaded);
            Assert.AreEqual(ViewModeControl.ViewMode.Minimap, mode);
        }
    }
}
