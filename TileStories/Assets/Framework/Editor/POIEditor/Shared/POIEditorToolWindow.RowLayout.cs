using System;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    public partial class POIEditorToolWindow
    {
        // --- Reusable non-table row layout ---
        // Any row that should sit visually "inside" its foldout section and NOT
        // stretch across the whole window uses DrawEditorRow. It owns, in one
        // place, all the layout rules that are easy to get subtly wrong:
        //
        //  1. INDENT: a TRANSPARENT spacer button whose width is the indent Unity
        //     actually applies here (EditorGUI.IndentedRect = indentLevel *
        //     theme width). It is a real GUILayout.Button, so it has real layout
        //     geometry (a bare GUILayout.Space is layout-only and easy to lose);
        //     its border is stripped and its tint is zero-alpha, so nothing is
        //     visibly drawn and the row only shows its own controls.

        //
        //  2. WIDTH RULE: content width = max(MinRowWidth, rightLimit - indent) where
        //     rightLimit = min(visible panel width, MaxRowWidth) and "visible panel
        //     width" = EditorGUIUtility.currentViewWidth (the VISIBLE panel; the scroll
        //     content can be much wider because the tables force it). The indent is
        //     INSIDE that budget, so the row's right edge lands at rightLimit for every
        //     indent -- change the indent and the right edge stays aligned.
        //
        //  3. STABILITY: the width is computed from currentViewWidth and the
        //     indent ONLY, never from a just-drawn control's GetLastRect -- in the
        //     Layout IMGUI pass that rect is still the 1x1 placeholder, so using it
        //     makes the Layout and Repaint passes disagree and GUILayout keeps the
        //     larger Layout width (measured 299 vs 267 on a 300px panel).

        // Available width for one non-table row's controls.
        // max(MinRowWidth, min(remainingRowWidth, MaxRowWidth)).
        internal static float EditorRowWidth(float remainingRowWidth)
        {
            return Mathf.Max(MinRowWidth, Mathf.Min(remainingRowWidth, MaxRowWidth));
        }

        // Usable CONTENT width for one row, given that row's runtime indent.
        //
        // The right limit is measured from x=0 and the indent is part of that budget, so
        // a deeper indent NARROWS the content while every row in the window still ends at
        // the same x. This is the whole point: previously the cap applied to the content
        // only (indent + MaxRowWidth), so a deeper indent pushed the right edge further
        // right by the indent width -- which is what made the (more-indented) Verified
        // button overhang rows that sat at a shallower indent.
        //
        // Pure: takes the view width / margin / indent as parameters so a test can assert
        // the SHIPPED decision instead of re-deriving the arithmetic.
        internal static float EditorRowWidthForIndent(float viewWidth, float rightMargin, float indent)
        {
            float rightLimit = Mathf.Min(viewWidth - rightMargin, MaxRowWidth);
            return EditorRowWidth(Mathf.Max(0f, rightLimit - indent));
        }

        // Opens a reusable row: draws a transparent indent spacer and computes the
        // row's usable width for the caller's controls. Caller draws its elements
        // with GUILayout.Width(rowWidth) (+ GUILayout.ExpandWidth(false) for
        // buttons), then calls EditorRowEnd(). Exposed as internal static so an
        // EditMode test can run it inside a real EditorWindow.OnGUI pass.
        //
        // extraIndentPixels: an optional RAW pixel nudge on top of the runtime
        // indent, for a conditional/dependent row that should sit only slightly
        // deeper than its section's own base fields -- narrower than a whole
        // EditorGUI.IndentLevelScope step (which also makes every EditorGUILayout
        // control inside the row re-apply the ambient indentLevel a second time on
        // its own label, see this file's row-layout Lesson 4). A raw pixel nudge
        // only moves the spacer/row start; it does not touch EditorGUI.indentLevel,
        // so the field's own internal indent stays identical to its sibling rows.
        // See ConditionalAdvance (Constants.cs) for the one shared value.
        internal static void DrawEditorRow(out float rowWidth, out Rect spacerRect, float extraIndentPixels = 0f)
        {
            EditorGUILayout.BeginHorizontal();

            float indent = EditorGUI.IndentedRect(new Rect(0f, 0f, 0f, 0f)).x + extraIndentPixels;
            // TRANSPARENT indent spacer: a real button (so it occupies exact
            // layout pixels and is measurable as geometry) with its border
            // stripped, so no visible border or fill. Zero-alpha tint means
            // nothing is drawn; width = the runtime indent of the rows.
            // Note: starts from a real texture background -- setting `.background`
            // to null makes GUI.skin.button fall back to the built-in grey
            // button texture, which IS visible.
            var spacerStyle = new GUIStyle(GUI.skin.button);
            spacerStyle.border = new RectOffset(0, 0, 0, 0);
            spacerStyle.normal.background = EditorGUIUtility.whiteTexture;
            spacerStyle.hover.background = EditorGUIUtility.whiteTexture;
            spacerStyle.active.background = EditorGUIUtility.whiteTexture;
            spacerStyle.focused.background = EditorGUIUtility.whiteTexture;
            Color prevBg = GUI.backgroundColor;
            GUI.backgroundColor = new Color(1f, 1f, 1f, 0f); // zero alpha -> invisible
            GUILayout.Button(GUIContent.none, spacerStyle,
                GUILayout.Width(indent), GUILayout.Height(EditorGUIUtility.singleLineHeight));
            GUI.backgroundColor = prevBg;
            spacerRect = GUILayoutUtility.GetLastRect();

            // See the class comment: width from stable inputs only.
            rowWidth = EditorRowWidthForIndent(EditorGUIUtility.currentViewWidth, AddButtonRowRightMargin, indent);

            // The spacer above has already paid the indent. Every EditorGUILayout control after it
            // would pay it AGAIN on its own label/rect (the double-indent trap), so the row's
            // controls run at indentLevel 0; EditorRowEnd restores it. Doing it here means no
            // caller can forget it.
            RowSavedIndentLevels.Push(EditorGUI.indentLevel);
            EditorGUI.indentLevel = 0;
        }

        // Ambient indent levels saved by open rows (rows never interleave, a stack keeps it safe)
        private static readonly System.Collections.Generic.Stack<int> RowSavedIndentLevels = new();

        // Closes a row opened with DrawEditorRow() and restores the ambient indent level.
        internal static void EditorRowEnd()
        {
            EditorGUILayout.EndHorizontal();
            if (RowSavedIndentLevels.Count > 0)
                EditorGUI.indentLevel = RowSavedIndentLevels.Pop();
        }

        // One row (header or data) of a table whose cells sit at rects the table computes itself.
        // EditorGUI's Rect controls (Toggle, TextField, IntField, FloatField, Popup) run their rect
        // through EditorGUI.IndentedRect, which inside a DrawFramedFoldout section moves EVERY cell
        // indentLevel * 15 px right and shrinks it as much: a checkbox's hit box slid off its glyph
        // (clicks missed) and two adjacent fields showed a gap. This pays the section's indent ONCE,
        // as a leading space, then zeroes indentLevel for the cells. (Setting labelWidth to 0 does
        // not help: Unity reads 0 as "use the default width".)
        internal sealed class TableRowScope : IDisposable
        {
            private readonly int _savedIndent;

            public TableRowScope()
            {
                EditorGUILayout.BeginHorizontal();
                _savedIndent = EditorGUI.indentLevel;
                GUILayout.Space(EditorGUI.IndentedRect(new Rect(0f, 0f, 0f, 0f)).x);
                EditorGUI.indentLevel = 0;
            }

            public void Dispose()
            {
                EditorGUI.indentLevel = _savedIndent;
                EditorGUILayout.EndHorizontal();
            }
        }

        // Compensates a labelled field row's reserved label column by exactly the
        // same extraIndentPixels its own DrawEditorRow spacer was widened by, so a
        // nested/conditional row's VALUE box lands at the same x as its shallower
        // siblings instead of drifting right by the nudge amount. Deliberately LOCAL,
        // not a global indent-level-to-label-width lookup table: extraIndentPixels
        // already IS the one piece of data needed (how much wider this row's spacer
        // is than a sibling's), so this just re-spends it on the label column instead
        // of introducing a second, parallel source of truth that could drift out of
        // sync with it. See _5.1_Editor_Tab.md "Row Indentation & Spacing" for the
        // full reasoning and the screenshot that prompted this (Orientation's "Up
        // Reference" / "Facing Basis" value boxes sitting right of their siblings').
        internal readonly struct FieldLabelWidthCompensationScope : IDisposable
        {
            private readonly float _savedLabelWidth;

            public FieldLabelWidthCompensationScope(float extraIndentPixels)
            {
                _savedLabelWidth = EditorGUIUtility.labelWidth;
                if (extraIndentPixels > 0f)
                    EditorGUIUtility.labelWidth = Mathf.Max(10f, _savedLabelWidth - extraIndentPixels);
            }

            public void Dispose()
            {
                EditorGUIUtility.labelWidth = _savedLabelWidth;
            }
        }
    }
}
