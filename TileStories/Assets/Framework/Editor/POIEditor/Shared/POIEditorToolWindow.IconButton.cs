// POIEditorToolWindow.IconButton.cs
//
// Partial: the one shared look for every icon-only button in this window (POI header
// row's focus/rename/add/delete, and every "(i)" help button via HelpInfoButton).
// Editor-only.

using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    public partial class POIEditorToolWindow
    {
        // Standard footprint for every icon-only button in this window -- one shared
        // constant so a new call site doesn't have to guess a width/height, and every
        // icon button in the tool stays visually consistent by construction.
        internal const float IconButtonSize = 22f;

        // Real miniButton background (so hover/press feedback is consistent everywhere
        // this is used) with the icon drawn separately via DrawTexture (ScaleToFit into
        // an inset rect) instead of relying on GUIContent's own image, which draws at
        // the source texture's native pixel size and can look tiny inside a button this
        // small. iconInset controls how large the glyph reads: a smaller inset leaves
        // more of the button to the icon (e.g. a bare "?" glyph with no surrounding
        // detail needs a smaller inset than a busier icon to read clearly).
        internal static bool DrawIconButton(Rect rect, Texture2D icon, string tooltip, float iconInset = 4f)
        {
            bool clicked = GUI.Button(rect, new GUIContent(string.Empty, tooltip), EditorStyles.miniButton);
            if (icon != null && Event.current.type == EventType.Repaint)
            {
                var iconRect = new Rect(rect.x + iconInset, rect.y + iconInset,
                    rect.width - iconInset * 2f, rect.height - iconInset * 2f);
                // GUI.DrawTexture ignores GUI.enabled (unlike a GUIStyle-driven draw), so a
                // disabled icon button (e.g. focus with no matching rig child) would
                // otherwise stay full-brightness while still correctly unclickable -- dim it
                // manually to match every other disabled control in this window.
                Color prevColor = GUI.color;
                if (!GUI.enabled)
                    GUI.color = new Color(prevColor.r, prevColor.g, prevColor.b, prevColor.a * 0.5f);
                GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
                GUI.color = prevColor;
            }
            return clicked;
        }

        // GUILayout entry point for DrawIconButton, for the (much more common) case of
        // an ordinary vertical/horizontal layout flow rather than a manually placed Rect
        // (only the POI header row's right-aligned icon cluster needs the Rect overload
        // directly, since its buttons are positioned by hand off the row's right edge).
        //
        // Height defaults to EditorGUIUtility.singleLineHeight, NOT `width` -- the POI
        // header row's own icon buttons are `width` wide but only singleLineHeight tall
        // (they share headerRect.height, a single text line), so a plain square
        // GetRect(size, size, ...) here rendered visibly taller/bigger than that row's
        // buttons despite using the "same" size constant. Confirmed by a real screenshot
        // showing the header row's "?" smaller than Position's/Rotation's even though
        // both passed `size: IconButtonSize` -- the width matched, the height didn't.
        internal static bool DrawIconButtonLayout(Texture2D icon, string tooltip, float width = IconButtonSize, float height = -1f, float iconInset = 4f)
        {
            float resolvedHeight = height > 0f ? height : EditorGUIUtility.singleLineHeight;
            Rect rect = GUILayoutUtility.GetRect(width, resolvedHeight,
                GUILayout.Width(width), GUILayout.Height(resolvedHeight), GUILayout.ExpandWidth(false));
            return DrawIconButton(rect, icon, tooltip, iconInset);
        }
    }
}
