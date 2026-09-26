using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // Labels, Text & Fonts domain authoring-side fixes (_2.0_Labels_And_Fonts_Design.md, 2026-09-24
    // pass): the wall font library's keys must actually reach the Font popups, and the Scene-view
    // rig preview must actually resolve a wall's font library the same way the running wall does.
    // Real code only, same reflection-driven pattern as DemoGridExclusivityTests.
    public class LabelsAndFontsAuthoringTests
    {
        private const BindingFlags Instance = BindingFlags.NonPublic | BindingFlags.Instance;
        private const BindingFlags Static = BindingFlags.NonPublic | BindingFlags.Static;

        private static POIEditorToolWindow NewWindow(WallConfigData config, FontKeyLibrary wallFontLibrary = null)
        {
            var window = ScriptableObject.CreateInstance<POIEditorToolWindow>();
            typeof(POIEditorToolWindow).GetField("_config", Instance).SetValue(window, config);
            typeof(POIEditorToolWindow).GetField("_wallFontLibrary", Instance).SetValue(window, wallFontLibrary);
            return window;
        }

        private static void GetAvailableFontKeyOptions(POIEditorToolWindow window, out string[] keys, out string[] labels)
        {
            var method = typeof(POIEditorToolWindow).GetMethod("GetAvailableFontKeyOptions", Instance);
            Assert.IsNotNull(method, "GetAvailableFontKeyOptions must exist");
            var args = new object[] { null, null };
            method.Invoke(window, args);
            keys = (string[])args[0];
            labels = (string[])args[1];
        }

        [Test]
        public void GetAvailableFontKeyOptions_WithNoWallLibrary_OffersOnlyTheThreeFrameworkKeys()
        {
            var window = NewWindow(new WallConfigData());
            try
            {
                GetAvailableFontKeyOptions(window, out string[] keys, out string[] labels);
                Assert.AreEqual(3, keys.Length, "Precondition: liberation_sans, roboto_bold, oswald_bold.");
                Assert.AreEqual(keys.Length, labels.Length, "Every key must have a matching label.");
                CollectionAssert.Contains(keys, "liberation_sans");
            }
            finally { Object.DestroyImmediate(window); }
        }

        // The real bug this guards against: a wall that created its own FontKeyLibrary and added a
        // 4th font had NO way to ever select it, because the Font popup's option array was a
        // hardcoded 3-key literal (_2.0_Labels_And_Fonts_Design.md section 6). Building a real
        // FontKeyLibrary with a real extra entry and reading it back through the actual method under
        // test is the composition proof (40-testing.md 4.2.1), not just that the method compiles.
        [Test]
        public void GetAvailableFontKeyOptions_WithAWallLibrary_AddsItsExtraKey_WithoutDuplicatingFrameworkKeys()
        {
            var wallLibrary = ScriptableObject.CreateInstance<FontKeyLibrary>();
            var entries = new System.Collections.Generic.List<FontKeyLibrary.Entry>
            {
                new FontKeyLibrary.Entry { key = "liberation_sans", font = MakeFontAsset() },
                new FontKeyLibrary.Entry { key = "wall_house_style", font = MakeFontAsset() },
            };
            typeof(FontKeyLibrary).GetField("entries", Instance).SetValue(wallLibrary, entries);

            var window = NewWindow(new WallConfigData(), wallLibrary);
            try
            {
                GetAvailableFontKeyOptions(window, out string[] keys, out string[] labels);

                Assert.AreEqual(4, keys.Length, "3 framework keys plus the wall's own 1 extra key, no duplicate liberation_sans.");
                CollectionAssert.Contains(keys, "wall_house_style", "The wall's own font key must be selectable.");
                int idx = System.Array.IndexOf(keys, "wall_house_style");
                StringAssert.Contains("wall", labels[idx], "A wall-added key's label should mark it as wall-specific.");
            }
            finally
            {
                Object.DestroyImmediate(window);
                Object.DestroyImmediate(wallLibrary);
            }
        }

        // Composition proof for the Scene-view rig preview fix: before this session,
        // PrepareRigVisuals called MarkerVisualSettings.Resolve WITHOUT the wall font library
        // argument, so a wall's own font override rendered correctly in Play Mode (WallSession
        // passes it) but never in the Scene-view rig. This drives the REAL private method the rig
        // refresh path calls, not a re-implementation of it.
        [Test]
        public void PrepareRigVisuals_ResolvesTheWallFontLibrary_SoScenePreviewMatchesPlayMode()
        {
            var wallLibrary = ScriptableObject.CreateInstance<FontKeyLibrary>();
            var window = NewWindow(new WallConfigData(), wallLibrary);
            try
            {
                var method = typeof(POIEditorToolWindow).GetMethod("PrepareRigVisuals", Instance);
                Assert.IsNotNull(method, "PrepareRigVisuals must exist");
                var settings = (MarkerVisualSettings)method.Invoke(window, null);

                Assert.AreSame(wallLibrary, settings.FontLibrary,
                    "The Scene-view rig must resolve the SAME wall font library the running wall (WallSession) resolves.");
            }
            finally
            {
                Object.DestroyImmediate(window);
                Object.DestroyImmediate(wallLibrary);
            }
        }

        // The label slider ranges exist ONCE (MarkerVisualSettings) and the runtime clamp really uses
        // them: an out-of-range config resolves to exactly those limits. Before, the same numbers
        // were typed in three places (runtime clamp, Labels section, the per-level window).
        [Test]
        public void LabelSliderRanges_AreGenerous_AndAreTheExactLimitsTheRuntimeClampsTo()
        {
            Assert.AreEqual(0f, MarkerVisualSettings.LabelGapRatioMin);
            Assert.GreaterOrEqual(MarkerVisualSettings.LabelGapRatioMax, 1f, "Gap ratio ceiling must not be the old 0.3 cap.");
            Assert.AreEqual(0.08f, MarkerVisualSettings.LabelFontSizeRatioMin);
            Assert.GreaterOrEqual(MarkerVisualSettings.LabelFontSizeRatioMax, 1.5f, "Font size ratio ceiling must not be the old 0.6 cap.");

            var high = MarkerVisualSettings.Resolve(new WallConfigData { label_gap_ratio = 99f, label_font_size_ratio = 99f }, null);
            var low = MarkerVisualSettings.Resolve(new WallConfigData { label_gap_ratio = -1f, label_font_size_ratio = 0f }, null);
            Assert.AreEqual(MarkerVisualSettings.LabelGapRatioMax, high.LabelGapRatio);
            Assert.AreEqual(MarkerVisualSettings.LabelFontSizeRatioMax, high.LabelFontSizeRatio);
            Assert.AreEqual(MarkerVisualSettings.LabelGapRatioMin, low.LabelGapRatio);
            Assert.AreEqual(MarkerVisualSettings.LabelFontSizeRatioMin, low.LabelFontSizeRatio);
        }

        // "Add font": the real window method registers a font in the wall's library, returns its
        // key, the key is then offered by every Font popup, and adding the same font twice reuses
        // the entry instead of duplicating it. A same-named but different font gets a suffixed key.
        [Test]
        public void AddFont_RegistersInTheWallLibrary_AppearsInTheFontOptions_AndIsIdempotent()
        {
            var wallLibrary = ScriptableObject.CreateInstance<FontKeyLibrary>();
            var font = MakeFontAsset();
            font.name = "House Style SDF";
            var twin = MakeFontAsset();
            twin.name = "House Style SDF";
            var window = NewWindow(new WallConfigData(), wallLibrary);
            try
            {
                var add = typeof(POIEditorToolWindow).GetMethod("AddFontToWallLibraryAndGetKey", Instance);
                Assert.IsNotNull(add, "AddFontToWallLibraryAndGetKey must exist");

                string key = (string)add.Invoke(window, new object[] { font });
                Assert.AreEqual("house_style_sdf", key, "key comes from the asset name, lower-case, spaces -> _");
                Assert.AreSame(font, wallLibrary.Get(key), "the font is really in the wall library");

                GetAvailableFontKeyOptions(window, out string[] keys, out _);
                CollectionAssert.Contains(keys, key, "an added font is offered by the Font popups");

                Assert.AreEqual(key, (string)add.Invoke(window, new object[] { font }), "adding the same font again reuses its key");
                Assert.AreEqual("house_style_sdf_2", (string)add.Invoke(window, new object[] { twin }), "a different font with the same name gets a free key");
                Assert.AreEqual(2, wallLibrary.Keys().Count, "no duplicate entry for the repeated font");
            }
            finally
            {
                Object.DestroyImmediate(window);
                Object.DestroyImmediate(wallLibrary);
                Object.DestroyImmediate(font);
                Object.DestroyImmediate(twin);
            }
        }

        private static TMPro.TMP_FontAsset MakeFontAsset() => ScriptableObject.CreateInstance<TMPro.TMP_FontAsset>();
    }
}
