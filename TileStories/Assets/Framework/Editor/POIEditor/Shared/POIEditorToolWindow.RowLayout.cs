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
        //  2. WIDTH RULE: max(MinRowWidth, min(remaining visible panel width,
        //     MaxRowWidth)). "Remaining" = EditorGUIUtility.currentViewWidth
        //     (= the VISIBLE panel; the scroll content can be much wider because
        //     the tables force it) minus the indent and a right margin.
        //
        //  3. STABILITY: the width is computed from currentViewWidth and the
        //     indent ONLY, never from a just-drawn control's GetLastRect -- in the
        //     Layout IMGUI pass that rect is still the 1x1 placeholder, so using it
        //     makes the Layout and Repaint passes disagree and GUILayout keeps the
        //     larger Layout width (measured 299 vs 267 on a 300px panel).

        // Available width for one non-table row's controls.
        // max(MinRowWidth, min(remaining visible panel width, MaxRowWidth)).
        internal static float EditorRowWidth(float remainingRowWidth)
        {
            return Mathf.Max(MinRowWidth, Mathf.Min(remainingRowWidth, MaxRowWidth));
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
            float remaining = Mathf.Max(0f, EditorGUIUtility.currentViewWidth - AddButtonRowRightMargin - indent);
            rowWidth = EditorRowWidth(remaining);
        }

        // Closes a row opened with DrawEditorRow().
        internal static void EditorRowEnd()
        {
            EditorGUILayout.EndHorizontal();
        }
    }
}
