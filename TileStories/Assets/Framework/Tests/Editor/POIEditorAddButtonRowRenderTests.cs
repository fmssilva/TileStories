using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace TileStories.Tests
{
    // REAL rendered-layout test for the +Add button row: it opens an actual
    // EditorWindow whose OnGUI calls the production DrawAddButtonRow, records the
    // real GUILayoutUtility rects the controls actually drew, then asserts the
    // invariant: a row containing the transparent indent spacer first, then the
    // real button immediately to its right, width = max(MinRowWidth,
    // min(remaining visible panel, MaxRowWidth)). No source-text checks; this
    // asserts on the actual measured layout produced by a real IMGUI pass.
    public class POIEditorAddButtonRowRenderTests
    {
        private sealed class AddButtonRowHarness : EditorWindow
        {
            public static bool DidDraw;
            public static Rect SpacerRect;
            public static Rect ButtonRect;
            public static float RuntimeIndentAtDraw;
            public static float ViewWidthAtDraw;

            private void OnGUI()
            {
                DidDraw = true;
                // Mirror the real window: the +Add row sits inside the foldout's
                // IndentLevelScope(2), so the runtime indent is 2 * theme width
                // (30px). Without this, the constant-free spacer would be 0 here.
                EditorGUI.indentLevel += 2;
                try
                {
                    RuntimeIndentAtDraw = EditorGUI.IndentedRect(new Rect(0f, 0f, 0f, 0f)).x;
                    ViewWidthAtDraw = EditorGUIUtility.currentViewWidth;
                    global::TileStories.Editor.POIEditorToolWindow.DrawAddButtonRow("+ Add category", () => { }, out SpacerRect, out ButtonRect);
                }
                finally
                {
                    EditorGUI.indentLevel -= 2;
                }
            }
        }

        // The row constants are private consts on the editor window; read them via
        // reflection (same pattern as POIEditorVisualHierarchyTests).
        private static float ReflectConst(string name)
        {
            var fi = typeof(global::TileStories.Editor.POIEditorToolWindow)
                .GetField(name, BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(fi, $"Expected const '{name}' on POIEditorToolWindow");
            return (float)fi.GetValue(null);
        }

        // Drives a genuine OnGUI pass on a live window and reads the real rects.
        private static IEnumerator DrawOnce(float windowWidth = 700f)
        {
            AddButtonRowHarness.DidDraw = false;
            AddButtonRowHarness.SpacerRect = default;
            AddButtonRowHarness.ButtonRect = default;

            var window = EditorWindow.GetWindow<AddButtonRowHarness>(true, "AddButtonRowRenderHarness");
            window.minSize = new Vector2(200f, 80f);
            window.position = new Rect(0f, 0f, windowWidth, 80f);
            window.Focus();
            window.Repaint();

            // Wait until OnGUI actually ran and laid out both buttons.
            for (int i = 0; i < 60 && (!AddButtonRowHarness.DidDraw || AddButtonRowHarness.ButtonRect.width < 50f); i++)
                yield return null;

            window.Close();
        }

        // The row must draw TWO buttons: the transparent spacer, then the real button to
        // its RIGHT, on the SAME horizontal line.
        [UnityTest]
        public IEnumerator AddButtonRow_HasTransparentSpacerThenRealButton_SideBySide()
        {
            yield return DrawOnce();

            Assert.That(AddButtonRowHarness.DidDraw, Is.True, "OnGUI must have run");
            Assert.That(AddButtonRowHarness.ButtonRect.width, Is.GreaterThan(50f),
                "The real +Add button must have been laid out at a real width");

            Rect spacer = AddButtonRowHarness.SpacerRect;
            Rect button = AddButtonRowHarness.ButtonRect;

            Assert.That(spacer.x, Is.GreaterThanOrEqualTo(0f), "Spacer must start at a valid x");
            Assert.That(button.x, Is.GreaterThan(spacer.x),
                "The real button must sit to the RIGHT of the transparent spacer (same row, two buttons)");
            Assert.That(Mathf.Abs(spacer.y - button.y), Is.LessThan(5f),
                "Spacer and button must be on the same horizontal line");

            // The whole point of the runtime measurement: the spacer must be the
            // REAL indented-rect displacement the rows get at foldout indent level 2
            // (30px), NOT the old thin 15px guess.
            Assert.That(AddButtonRowHarness.RuntimeIndentAtDraw, Is.GreaterThan(15f),
                "Runtime indent at foldout level 2 must be wider than the old 15px constant");
            Assert.That(spacer.width, Is.EqualTo(AddButtonRowHarness.RuntimeIndentAtDraw).Within(0.5f),
                "The transparent spacer button must be exactly as wide as the runtime-measured indent of the rows");
        }

        // The real button is capped so the row's right edge lands on
        // min(visible panel, MaxRowWidth) -- with the indent INSIDE that budget, so on a
        // WIDE panel the content width is MaxRowWidth minus this row's indent.
        [UnityTest]
        public IEnumerator AddButtonRow_RealButtonWidthCappedAtMaxRowWidth()
        {
            yield return DrawOnce(900f);

            Rect button = AddButtonRowHarness.ButtonRect;
            float expected = global::TileStories.Editor.POIEditorToolWindow.EditorRowWidthForIndent(
                AddButtonRowHarness.ViewWidthAtDraw, ReflectConst("AddButtonRowRightMargin"),
                AddButtonRowHarness.RuntimeIndentAtDraw);
            Assert.That(AddButtonRowHarness.ViewWidthAtDraw, Is.GreaterThan(700f),
                "Probe sanity: wide panel should actually be wide");
            Assert.That(button.width, Is.EqualTo(expected).Within(2f),
                "Button width must match EditorRowWidth (max(Min, min(remaining, Max))) on a wide panel");
        }

        // On a NARROW panel the button must shrink below MaxRowWidth, not stay at
        // 480 (which would clip against the visible panel).
        [UnityTest]
        public IEnumerator AddButtonRow_RealButtonWidthFitsNarrowPanel()
        {
            yield return DrawOnce(300f);

            Rect button = AddButtonRowHarness.ButtonRect;
            float expected = global::TileStories.Editor.POIEditorToolWindow.EditorRowWidthForIndent(
                AddButtonRowHarness.ViewWidthAtDraw, ReflectConst("AddButtonRowRightMargin"),
                AddButtonRowHarness.RuntimeIndentAtDraw);
            Assert.That(AddButtonRowHarness.ViewWidthAtDraw, Is.LessThan(400f),
                "Probe sanity: narrow panel should actually be narrow");
            Assert.That(button.width, Is.LessThan(ReflectConst("MaxRowWidth")),
                "Button must shrink below MaxRowWidth on a narrow panel");
            Assert.That(button.width, Is.EqualTo(expected).Within(2f),
                "Button width must match EditorRowWidth (max(Min, min(remaining, Max))) on a narrow panel");
            Assert.That(button.x + button.width, Is.LessThanOrEqualTo(AddButtonRowHarness.ViewWidthAtDraw + 2f),
                "Button right edge must stay inside the visible panel on a narrow window");
        }
        // ---- Non-button row: Scene Configuration / DrawPathRow ----
        // The shared row also caps rows whose stretchy element is a LABELLED
        // field (TextField) rather than a button. The field gets rowWidth minus
        // room for the trailing "..." browse button, and the row still opens
        // with the same transparent indent spacer.
        private sealed class PathRowHarness : EditorWindow
        {
            public static bool DidDraw;
            public static Rect SpacerRect;
            public static Rect FieldRect;
            public static Rect BrowseRect;
            public static float RuntimeIndentAtDraw;
            public static float ViewWidthAtDraw;
            public static string PathValue = "Assets/Test.json";

            private void OnGUI()
            {
                DidDraw = true;
                EditorGUI.indentLevel += 2;
                try
                {
                    RuntimeIndentAtDraw = EditorGUI.IndentedRect(new Rect(0f, 0f, 0f, 0f)).x;
                    ViewWidthAtDraw = EditorGUIUtility.currentViewWidth;
                    global::TileStories.Editor.POIEditorToolWindow.DrawPathRow("Config path", ref PathValue, "json", out SpacerRect, out FieldRect, out BrowseRect);
                }
                finally
                {
                    EditorGUI.indentLevel -= 2;
                }
            }
        }

        private static IEnumerator DrawPathOnce(float windowWidth = 700f)
        {
            PathRowHarness.DidDraw = false;
            PathRowHarness.SpacerRect = default;
            PathRowHarness.FieldRect = default;
            PathRowHarness.BrowseRect = default;

            var window = EditorWindow.GetWindow<PathRowHarness>(true, "PathRowRenderHarness");
            window.minSize = new Vector2(200f, 80f);
            window.position = new Rect(0f, 0f, windowWidth, 80f);
            window.Focus();
            window.Repaint();

            for (int i = 0; i < 60 && (!PathRowHarness.DidDraw || PathRowHarness.FieldRect.width < 50f); i++)
                yield return null;

            window.Close();
        }

        // On a WIDE panel the labelled field must be capped: its width equals the
        // shared rowWidth minus the browse-button allowance, and it sits on the
        // same line as the transparent spacer, with the browse button to its right.
        [UnityTest]
        public IEnumerator PathRow_LabelledFieldCappedSameAsButtonRow()
        {
            yield return DrawPathOnce(900f);

            Rect spacer = PathRowHarness.SpacerRect;
            Rect field = PathRowHarness.FieldRect;
            Rect browse = PathRowHarness.BrowseRect;

            Assert.That(PathRowHarness.DidDraw, Is.True, "OnGUI must have run");
            Assert.That(field.x, Is.GreaterThanOrEqualTo(spacer.x),
                "Field must sit at/right of the transparent spacer (indent)");
            Assert.That(browse.x, Is.GreaterThan(field.x),
                "Browse button must sit to the RIGHT of the labelled field (same row)");
            Assert.That(Mathf.Abs(spacer.y - field.y), Is.LessThan(5f),
                "Spacer and field must be on the same horizontal line");

            float expected = global::TileStories.Editor.POIEditorToolWindow.EditorRowWidthForIndent(
                PathRowHarness.ViewWidthAtDraw, ReflectConst("AddButtonRowRightMargin"),
                PathRowHarness.RuntimeIndentAtDraw);
            Assert.That(PathRowHarness.ViewWidthAtDraw, Is.GreaterThan(700f),
                "Probe sanity: wide panel should actually be wide");
            Assert.That(field.width, Is.EqualTo(expected - 36f).Within(2f),
                "Labelled field width must equal shared rowWidth minus the browse-button allowance (36px)");
        }

        // On a NARROW panel both the field and the browse button must stay inside
        // the visible panel (the whole non-button row shrinks with the window).
        [UnityTest]
        public IEnumerator PathRow_NonButtonRowFitsNarrowPanel()
        {
            yield return DrawPathOnce(300f);

            Rect field = PathRowHarness.FieldRect;
            Rect browse = PathRowHarness.BrowseRect;

            Assert.That(PathRowHarness.ViewWidthAtDraw, Is.LessThan(400f),
                "Probe sanity: narrow panel should actually be narrow");
            Assert.That(field.width, Is.LessThan(ReflectConst("MaxRowWidth")),
                "Field must shrink below MaxRowWidth on a narrow panel");
            Assert.That(browse.x + browse.width, Is.LessThanOrEqualTo(PathRowHarness.ViewWidthAtDraw + 2f),
                "Row right edge (browse button) must stay inside the visible panel on a narrow window");
        }
    }
}
