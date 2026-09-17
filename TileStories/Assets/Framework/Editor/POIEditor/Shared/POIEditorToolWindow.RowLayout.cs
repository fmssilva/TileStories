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
        internal static void DrawEditorRow(out float rowWidth, out Rect spacerRect)
        {
            EditorGUILayout.BeginHorizontal();

            float indent = EditorGUI.IndentedRect(new Rect(0f, 0f, 0f, 0f)).x;
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
        }

        // Closes a row opened with DrawEditorRow().
        internal static void EditorRowEnd()
        {
            EditorGUILayout.EndHorizontal();
        }
    }
}
