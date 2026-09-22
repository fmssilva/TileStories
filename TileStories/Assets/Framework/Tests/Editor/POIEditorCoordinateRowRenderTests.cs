// POIEditorCoordinateRowRenderTests.cs
//
// REAL rendered-layout test for the per-POI coordinate row (x [val] y [val] z [val]).
// That row's axis letters were invisible for a long time, so this test does not guess at
// geometry: it opens a real EditorWindow, drives the production DrawCoordinateRow inside
// a genuine OnGUI pass at the same indent the Position foldout uses, and asserts on the
// ACTUAL GUILayoutUtility rects the controls drew:
//   - the axis letter must have a REAL width (a sliver here is exactly the bug),
//   - the letter must sit immediately left of its own value field,
//   - the row must fit inside the visible panel (right margin included, so the last
//     pixels are not hidden under the ScrollView's vertical scrollbar).
using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace TileStories.Tests
{
    public class POIEditorCoordinateRowRenderTests
    {
        private sealed class CoordinateRowHarness : EditorWindow
        {
            public static bool DidDraw;
            public static Rect FirstLabelRect;
            public static Rect FirstValueRect;
            public static Rect LastValueRect;
            public static float ViewWidthAtDraw;

            private void OnGUI()
            {
                DidDraw = true;
                // Mirror DrawPositionTabs' net indent: POI foldout (+1) + framed-foldout
                // content (+1) == +2 (the row itself runs its controls at indent 0).
                EditorGUI.indentLevel += 2;
                try
                {
                    ViewWidthAtDraw = EditorGUIUtility.currentViewWidth;
                    global::TileStories.Editor.POIEditorToolWindow.DrawCoordinateRow(
                        -0.99f, -0.87f, -3.677f,
                        out FirstLabelRect, out FirstValueRect, out LastValueRect);
                }
                finally
                {
                    EditorGUI.indentLevel -= 1;
                }
            }
        }

        private static IEnumerator DrawOnce(float windowWidth)
        {
            CoordinateRowHarness.DidDraw = false;
            CoordinateRowHarness.FirstLabelRect = default;
            CoordinateRowHarness.FirstValueRect = default;
            CoordinateRowHarness.LastValueRect = default;

            var window = EditorWindow.GetWindow<CoordinateRowHarness>(true, "CoordinateRowHarness");
            window.minSize = new Vector2(200f, 80f);
            window.position = new Rect(0f, 0f, windowWidth, 80f);
            window.Focus();
            window.Repaint();

            // Wait until OnGUI actually ran and laid the row out.
            for (int i = 0; i < 60 && (!CoordinateRowHarness.DidDraw || CoordinateRowHarness.LastValueRect.width < 40f); i++)
                yield return null;

            window.Close();
        }

        [UnityTest]
        public IEnumerator CoordinateRow_LabelHasRealWidthAndSitsNextToItsValue()
        {
            yield return DrawOnce(700f);

            Assert.That(CoordinateRowHarness.DidDraw, Is.True, "OnGUI must have run");
            Rect label = CoordinateRowHarness.FirstLabelRect;
            Rect value = CoordinateRowHarness.FirstValueRect;

            Assert.That(label.width, Is.GreaterThan(8f),
                "The axis letter must have a real width -- a sliver here is exactly the bug");
            Assert.That(label.height, Is.GreaterThan(8f), "The axis label must occupy a real line");
            Assert.That(value.x, Is.GreaterThan(label.x), "The value field sits right of its axis letter");
            Assert.That(value.x - (label.x + label.width), Is.LessThanOrEqualTo(0.5f),
                "The letter must sit FLUSH against its own value field with no visible gap -- both " +
                "styles' default 3px GUIStyle.margin were zeroed out specifically so this gap is exactly " +
                "0, not just 'small'");
            Assert.That(value.width, Is.LessThanOrEqualTo(90f),
                "The value field must stay a compact bounded width like every other read-only numeric " +
                "field in this window -- stretching it to fill the row leaves a huge empty box next to " +
                "a tiny axis letter, which reads as a big gap around the letter");
            Assert.That(label.width, Is.GreaterThanOrEqualTo(10f).And.LessThanOrEqualTo(16f),
                "The axis letter's allocated width should track its OWN glyph size (EditorStyles." +
                "boldLabel.CalcSize(\"X\") measures ~10.3px on this font/DPI, +2px buffer), not a much " +
                "larger guessed constant -- a wider-than-needed label rect is dead space between the " +
                "letter and the value field, which is the exact bug this row keeps regressing to");
        }

        [UnityTest]
        public IEnumerator CoordinateRow_WholeRowFitsWidePanel()
        {
            yield return DrawOnce(900f);

            Assert.That(CoordinateRowHarness.ViewWidthAtDraw, Is.GreaterThan(700f),
                "Probe sanity: wide panel should actually be wide");
            Rect last = CoordinateRowHarness.LastValueRect;
            Assert.That(last.x + last.width, Is.LessThanOrEqualTo(CoordinateRowHarness.ViewWidthAtDraw + 2f),
                "The row's right edge must stay inside the visible panel (scrollbar margin included)");
        }

        [UnityTest]
        public IEnumerator CoordinateRow_WholeRowFitsNarrowPanel()
        {
            yield return DrawOnce(300f);

            Assert.That(CoordinateRowHarness.ViewWidthAtDraw, Is.LessThan(400f),
                "Probe sanity: narrow panel should actually be narrow");
            Rect last = CoordinateRowHarness.LastValueRect;
            Assert.That(last.x + last.width, Is.LessThanOrEqualTo(CoordinateRowHarness.ViewWidthAtDraw + 2f),
                "Row must stay inside the visible panel on a narrow window (scrollbar margin)");
            Assert.That(CoordinateRowHarness.FirstLabelRect.width, Is.GreaterThan(8f),
                "Even on a narrow panel the axis letter must keep a real width");
        }
    }
}
