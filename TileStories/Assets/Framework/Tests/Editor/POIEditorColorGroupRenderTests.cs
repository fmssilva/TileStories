// POIEditorColorGroupRenderTests.cs
//
// REAL rendered-layout test for the Marker Table's Color group (developer-reported:
// color picker rendered too wide/non-square, the gap before it to the Symbol group
// looked collapsed, and the gap before the delete button looked no bigger than any
// other column gap -- "this has been happening for ages"). Like
// POIEditorCoordinateRowRenderTests, this does not guess at geometry: it opens a real
// EditorWindow, drives the ACTUAL production DrawSymbolTable<CategoryStyleEntry> (via
// reflection, since it is a private generic method) inside a genuine OnGUI pass with
// one fabricated category row, and asserts on the REAL GUILayoutUtility rects the
// controls drew, captured through DrawSymbolTable's captureFirstRowRects test seam.
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using TileStories;

namespace TileStories.Tests
{
    public class POIEditorColorGroupRenderTests
    {
        private static float ReflectConst(string fieldName)
        {
            var fi = typeof(TileStories.Editor.POIEditorToolWindow).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(fi, $"Expected private static const '{fieldName}' on POIEditorToolWindow");
            return (float)fi.GetValue(null);
        }

        private sealed class ColorGroupHarness : EditorWindow
        {
            public static bool DidDraw;
            public static Rect PreviewRect;
            public static Rect ColorPickerRect;
            public static Rect ColorHexRect;
            public static Rect KeywordsTailRect;
            public static Rect DeleteRect;

            private void OnGUI()
            {
                DidDraw = true;

                // Mirror the real ambient indent when the Marker section's table
                // renders: DrawTabContentContainer's IndentLevelScope (+1) +
                // DrawFramedFoldout("Marker", ...)'s content IndentLevelScope (+1).
                EditorGUI.indentLevel += 2;
                try
                {
                    var window = CreateInstance<TileStories.Editor.POIEditorToolWindow>();
                    try
                    {
                        var entry = new CategoryStyleEntry
                        {
                            category = "test_category",
                            color_hex = "#FFC786",
                            icon_key = string.Empty,
                            details = string.Empty,
                            search_keywords = new List<string>()
                        };
                        var entries = new List<CategoryStyleEntry> { entry };

                        MethodInfo generic = typeof(TileStories.Editor.POIEditorToolWindow).GetMethod(
                            "DrawSymbolTable", BindingFlags.NonPublic | BindingFlags.Instance);
                        Assert.IsNotNull(generic, "DrawSymbolTable must exist on POIEditorToolWindow");
                        MethodInfo closed = generic.MakeGenericMethod(typeof(CategoryStyleEntry));

                        System.Func<CategoryStyleEntry> createNew = () => new CategoryStyleEntry();
                        System.Func<CategoryStyleEntry, string> getPrimary = e => e.category;
                        System.Action<CategoryStyleEntry, string> setPrimary = (e, v) => e.category = v;
                        System.Func<CategoryStyleEntry, string> getIcon = e => e.icon_key;
                        System.Action<CategoryStyleEntry, string> setIcon = (e, v) => e.icon_key = v;
                        System.Func<CategoryStyleEntry, string> getColor = e => e.color_hex;
                        System.Action<CategoryStyleEntry, string> setColor = (e, v) => e.color_hex = v;
                        System.Func<CategoryStyleEntry, string> getDetails = e => e.details;
                        System.Action<CategoryStyleEntry, string> setDetails = (e, v) => e.details = v;
                        System.Func<CategoryStyleEntry, bool> showColorPicker = e => true;
                        System.Func<CategoryStyleEntry, List<string>> getKeywords = e => e.search_keywords;
                        System.Action<CategoryStyleEntry, List<string>> setKeywords = (e, v) => e.search_keywords = v;
                        System.Func<CategoryStyleEntry, int> countRefs = e => 0;
                        System.Action<Rect, Rect, Rect, Rect, Rect> capture = (preview, picker, hex, keywords, delete) =>
                        {
                            PreviewRect = preview;
                            ColorPickerRect = picker;
                            ColorHexRect = hex;
                            KeywordsTailRect = keywords;
                            DeleteRect = delete;
                        };

                        closed.Invoke(window, new object[]
                        {
                            entries, createNew, getPrimary, setPrimary, getIcon, setIcon,
                            getColor, setColor, getDetails, setDetails, showColorPicker,
                            "+ Add category", "Category", "", "", true, getKeywords, setKeywords,
                            countRefs, capture
                        });
                    }
                    finally
                    {
                        Object.DestroyImmediate(window);
                    }
                }
                finally
                {
                    EditorGUI.indentLevel -= 2;
                }
            }
        }

        private static IEnumerator DrawOnce(float windowWidth)
        {
            ColorGroupHarness.DidDraw = false;
            ColorGroupHarness.PreviewRect = default;
            ColorGroupHarness.ColorPickerRect = default;
            ColorGroupHarness.ColorHexRect = default;
            ColorGroupHarness.KeywordsTailRect = default;
            ColorGroupHarness.DeleteRect = default;

            var window = EditorWindow.GetWindow<ColorGroupHarness>(true, "ColorGroupHarness");
            window.minSize = new Vector2(200f, 120f);
            window.position = new Rect(0f, 0f, windowWidth, 120f);
            window.Focus();
            window.Repaint();

            for (int i = 0; i < 60 && (!ColorGroupHarness.DidDraw || ColorGroupHarness.DeleteRect.width < 5f); i++)
                yield return null;

            window.Close();
        }

        [UnityTest]
        public IEnumerator ColorPicker_IsNarrowAndRoughlySquare()
        {
            yield return DrawOnce(900f);

            Assert.That(ColorGroupHarness.DidDraw, Is.True, "OnGUI must have run");
            Rect picker = ColorGroupHarness.ColorPickerRect;
            float declaredWidth = ReflectConst("ColorPickerWidth");

            Assert.That(picker.width, Is.EqualTo(declaredWidth).Within(1f),
                "The color picker must render at its declared ColorPickerWidth, not stretch wider " +
                "(ExpandWidth(false) must be honored)");
            Assert.That(picker.width, Is.LessThanOrEqualTo(picker.height * 2.5f),
                "The color picker should read as roughly square-like against the row's single-line " +
                "height, not a long wide bar");
        }

        [UnityTest]
        public IEnumerator ColorPickerAndHex_SitFlushWithNoLargeGap()
        {
            yield return DrawOnce(900f);

            Rect picker = ColorGroupHarness.ColorPickerRect;
            Rect hex = ColorGroupHarness.ColorHexRect;

            Assert.That(hex.x, Is.GreaterThan(picker.x), "The hex field sits right of the picker");
            float gap = hex.x - (picker.x + picker.width);
            Assert.That(gap, Is.LessThanOrEqualTo(5f),
                "The picker and hex field must sit flush (GUILayout's own ~3px auto-spacing only) -- " +
                "a large gap here is the indent-doubling regression (Lesson 4)");
        }

        [UnityTest]
        public IEnumerator Gap_BetweenSymbolPreviewAndColorPicker_MatchesBeforeColorConstant()
        {
            yield return DrawOnce(900f);

            Rect preview = ColorGroupHarness.PreviewRect;
            Rect picker = ColorGroupHarness.ColorPickerRect;
            float expectedGap = ReflectConst("TableGapBeforeColor") + GUILayoutAutoSpacing;
            float betweenGroups = ReflectConst("TableGapBetweenGroups") + GUILayoutAutoSpacing;

            float actualGap = picker.x - (preview.x + preview.width);
            Assert.That(actualGap, Is.EqualTo(expectedGap).Within(1f),
                "The gap between the Symbol group's preview thumbnail and the Color group must equal " +
                "the dedicated TableGapBeforeColor constant plus GUILayout's own ~3px auto-spacing " +
                "(this file's Lesson 3: that auto-spacing is additive on top of any explicit " +
                "GUILayout.Space, not replaced by it) -- widened past the standard between-groups gap " +
                "per direct developer screenshot feedback (2026-09-18) that the standard gap read as " +
                "visually too tight next to the (intentionally narrow) color swatch");
            Assert.That(actualGap, Is.GreaterThan(betweenGroups),
                "The pre-color gap must render visibly bigger than an ordinary between-groups gap");
        }

        // GUILayout's own default inter-control spacing, additive on top of any explicit
        // GUILayout.Space() between two controls in the same row (this file's Lesson 3;
        // measured directly here via the two gap tests below, both landing at
        // constant + 3f).
        private const float GUILayoutAutoSpacing = 3f;

        [UnityTest]
        public IEnumerator Gap_BeforeDeleteButton_IsLargerThanBetweenGroupsGap()
        {
            yield return DrawOnce(900f);

            Rect keywordsTail = ColorGroupHarness.KeywordsTailRect;
            Rect delete = ColorGroupHarness.DeleteRect;
            float betweenGroups = ReflectConst("TableGapBetweenGroups") + GUILayoutAutoSpacing;
            float beforeDelete = ReflectConst("TableGapBeforeDelete") + GUILayoutAutoSpacing;

            float actualGap = delete.x - (keywordsTail.x + keywordsTail.width);
            Assert.That(actualGap, Is.EqualTo(beforeDelete).Within(1f),
                "The gap before the delete button must equal the dedicated TableGapBeforeDelete " +
                "constant plus GUILayout's own ~3px auto-spacing");
            Assert.That(actualGap, Is.GreaterThan(betweenGroups),
                "The pre-delete gap must render visibly bigger than an ordinary between-groups gap");
        }
    }
}
