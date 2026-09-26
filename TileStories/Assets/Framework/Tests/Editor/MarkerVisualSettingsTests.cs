using NUnit.Framework;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // MarkerVisualSettings.Resolve: pure config-to-runtime-value logic (40-testing.md 4.2 -- a
    // silent bug here affects every wall at once). Ring/badge ratios already worked this way and
    // were only ever proven indirectly through full WallSession integration tests; this covers the
    // resolve+clamp step directly, and is where label_gap_ratio/label_font_size_ratio (_2.2.1) join
    // the same pattern.
    public class MarkerVisualSettingsTests
    {
        private static WallConfigData Config() => new WallConfigData
        {
            marker_shape = "circle",
            badge_shape = "circle",
            marker_outline_mode = "uniform",
        };

        [Test]
        public void Resolve_ReadsLabelRatios_StraightThrough_WhenWithinRange()
        {
            var config = Config();
            config.label_gap_ratio = 0.12f;
            config.label_font_size_ratio = 0.33f;

            var settings = MarkerVisualSettings.Resolve(config, null);

            Assert.AreEqual(0.12f, settings.LabelGapRatio, 1e-4f);
            Assert.AreEqual(0.33f, settings.LabelFontSizeRatio, 1e-4f);
        }

        [Test]
        public void Resolve_ClampsLabelGapRatio_ToZeroToOne()
        {
            var tooSmall = Config();
            tooSmall.label_gap_ratio = -1f;
            Assert.AreEqual(0f, MarkerVisualSettings.Resolve(tooSmall, null).LabelGapRatio, 1e-4f);

            var tooBig = Config();
            tooBig.label_gap_ratio = 5f;
            Assert.AreEqual(1f, MarkerVisualSettings.Resolve(tooBig, null).LabelGapRatio, 1e-4f);
        }

        [Test]
        public void Resolve_ClampsLabelFontSizeRatio_ToPoint08ToOnePointFive()
        {
            var tooSmall = Config();
            tooSmall.label_font_size_ratio = 0f;
            Assert.AreEqual(0.08f, MarkerVisualSettings.Resolve(tooSmall, null).LabelFontSizeRatio, 1e-4f);

            var tooBig = Config();
            tooBig.label_font_size_ratio = 10f;
            Assert.AreEqual(1.5f, MarkerVisualSettings.Resolve(tooBig, null).LabelFontSizeRatio, 1e-4f);
        }

        [Test]
        public void Resolve_NullConfig_ReturnsTheFrameworkDefaults_NeverThrows()
        {
            var settings = MarkerVisualSettings.Resolve(null, null);

            Assert.AreEqual(0.075f, settings.LabelGapRatio, 1e-4f);
            Assert.AreEqual(0.25f, settings.LabelFontSizeRatio, 1e-4f);
        }

        [Test]
        public void Resolve_ReadsLabelFontKey_StraightThrough()
        {
            var config = Config();
            config.label_font_key = "roboto_bold";

            var settings = MarkerVisualSettings.Resolve(config, null);

            Assert.AreEqual("roboto_bold", settings.LabelFontKey);
        }

        [Test]
        public void Resolve_BlankLabelFontKey_FallsBackToLiberationSans()
        {
            var config = Config();
            config.label_font_key = "  ";

            var settings = MarkerVisualSettings.Resolve(config, null);

            Assert.AreEqual("liberation_sans", settings.LabelFontKey);
        }

        [Test]
        public void Resolve_PassesThroughTheGivenFontLibrary_UnchangedReference()
        {
            var lib = ScriptableObject.CreateInstance<FontKeyLibrary>();
            var settings = MarkerVisualSettings.Resolve(Config(), null, lib);

            Assert.AreSame(lib, settings.FontLibrary);
        }

        [Test]
        public void Resolve_NullFontLibrary_LeavesFontLibraryNull_MarkerViewFallsBackToPrefabDefault()
        {
            var settings = MarkerVisualSettings.Resolve(Config(), null, null);

            Assert.IsNull(settings.FontLibrary);
        }

        // icon_color_hex / icon_size_ratio (_2.2.1): the defaults are the framework's original look,
        // so a wall that never sets them renders exactly as before these fields existed.
        [Test]
        public void Resolve_IconDefaults_EqualTheOriginalHardcodedLook()
        {
            var settings = MarkerVisualSettings.Resolve(Config(), null);
            var original = new Color(0.949f, 0.925f, 0.827f);
            Assert.AreEqual(original.r, settings.IconColor.r, 0.003f);
            Assert.AreEqual(original.g, settings.IconColor.g, 0.003f);
            Assert.AreEqual(original.b, settings.IconColor.b, 0.003f);
            Assert.AreEqual(0.56f, settings.IconSizeRatio, 1e-4f);
        }

        [Test]
        public void Resolve_IconColorAndSize_ReadThrough_ClampSize_AndFallBackOnABadHex()
        {
            var config = Config();
            config.icon_color_hex = "#102030";
            config.icon_size_ratio = 0.8f;
            var settings = MarkerVisualSettings.Resolve(config, null);
            ColorUtility.TryParseHtmlString("#102030", out var expected);
            Assert.AreEqual(expected, settings.IconColor);
            Assert.AreEqual(0.8f, settings.IconSizeRatio, 1e-4f);

            config.icon_size_ratio = 5f;
            Assert.AreEqual(MarkerVisualSettings.IconSizeRatioMax, MarkerVisualSettings.Resolve(config, null).IconSizeRatio, 1e-4f);
            config.icon_size_ratio = 0f;
            Assert.AreEqual(MarkerVisualSettings.IconSizeRatioMin, MarkerVisualSettings.Resolve(config, null).IconSizeRatio, 1e-4f);

            config.icon_color_hex = "not-a-colour";
            Assert.AreEqual(MarkerVisualSettings.Resolve(Config(), null).IconColor, MarkerVisualSettings.Resolve(config, null).IconColor,
                "a malformed hex falls back to the default icon colour, never to black or white");
        }
    }
}
