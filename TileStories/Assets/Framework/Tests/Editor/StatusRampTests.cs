using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace TileStories.Tests
{
    public class StatusRampTests
    {
        [SetUp]
        public void SetUp()
        {
            CloseEditorWindows();
            StatusRamp.ResetToDefaults();
        }

        [TearDown]
        public void TearDown()
        {
            StatusRamp.ResetToDefaults();
            CloseEditorWindows();
        }

        private static void CloseEditorWindows()
        {
            foreach (var window in Resources.FindObjectsOfTypeAll<TileStories.Editor.POIEditorToolWindow>())
            {
                window.Close();
            }
        }

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

        // Regression (P5): before this, StatusRamp.Configure read every row's color_hex regardless of
        // mode, so a developer who once typed a per-row colour kept seeing it even after switching the
        // Outline Color dropdown to Uniform -- the table and the dropdown disagreed about what
        // "uniform" meant.
        [Test]
        public void Configure_UniformMode_EveryLevelSharesTheUniformColour_IgnoringPerRowColorHex()
        {
            var uniformColor = new Color(0.2f, 0.4f, 0.6f, 1f);
            var entries = new System.Collections.Generic.List<OutlineLevelEntry>
            {
                new OutlineLevelEntry { key = "a", pct = 0f, line_style = "solid", color_hex = "#FF0000" },
                new OutlineLevelEntry { key = "b", pct = 50f, line_style = "medium_dashed", color_hex = "" },
                new OutlineLevelEntry { key = "c", pct = 100f, line_style = "dotted", color_hex = "#00FF00" },
            };

            StatusRamp.Configure(entries, MarkerOutlineMode.Uniform, uniformColor);

            foreach (var level in StatusRamp.ActiveLevels)
                Assert.AreEqual(uniformColor, level.RingColor, $"level at {level.Pct}% must use the uniform colour, not its own color_hex.");

            // Line style stays per-row even in Uniform mode -- only colour is shared.
            Assert.AreEqual("solid", StatusRamp.ActiveLevels[0].RingSpriteKey);
            Assert.AreEqual("dash_medium", StatusRamp.ActiveLevels[1].RingSpriteKey);
            Assert.AreEqual("dotted", StatusRamp.ActiveLevels[2].RingSpriteKey);
        }

        [Test]
        public void Configure_PerTypeMode_HonoursEachRowsOwnColorHex()
        {
            var entries = new System.Collections.Generic.List<OutlineLevelEntry>
            {
                new OutlineLevelEntry { key = "a", pct = 0f, line_style = "solid", color_hex = "#FF0000" },
                new OutlineLevelEntry { key = "b", pct = 100f, line_style = "dotted", color_hex = "" },
            };

            StatusRamp.Configure(entries, MarkerOutlineMode.PerType, Color.magenta);

            ColorUtility.TryParseHtmlString("#FF0000", out var expectedRed);
            Assert.AreEqual(expectedRed, StatusRamp.ActiveLevels[0].RingColor, "a row's own colour must be honoured in Per outline type mode.");
            Assert.AreNotEqual(Color.magenta, StatusRamp.ActiveLevels[1].RingColor, "an empty row falls back to the stock ramp colour, not the (irrelevant here) uniform colour.");
        }
    }
}
