// POIEditorToolWindow.DeleteButton.cs
//
// Partial: the one shared look for every destructive "remove this row" button in
// this window (taxonomy table rows, LOD bands, search fields, synonym groups, the
// POI header row). Editor-only.

using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    // Mirrors HelpInfoButton's shape: every delete affordance in the window goes
    // through this one method instead of each call site picking its own icon/size,
    // so a new delete button anywhere is visually consistent by construction. Built
    // on the same DrawIconButton/DrawIconButtonLayout primitive every other icon
    // button here uses. Default icon is the Marker Table's own trash glyph
    // (POIEditorToolWindow.TrashIcon); default size matches the POI header row's
    // delete button (IconButtonSize, single-line-height tall, not a square).
    internal static class DeleteButton
    {
        // GUILayout entry point -- taxonomy table rows and other ordinary layout flow.
        public static bool DrawLayout(string tooltip = "Delete", float size = POIEditorToolWindow.IconButtonSize)
        {
            return POIEditorToolWindow.DrawIconButtonLayout(
                (Texture2D)POIEditorToolWindow.TrashIcon.image, tooltip, size);
        }

        // Rect entry point -- for rows that place their icon cluster by hand off a
        // computed right edge (the POI header row).
        public static bool Draw(Rect rect, string tooltip = "Delete")
        {
            return POIEditorToolWindow.DrawIconButton(rect, (Texture2D)POIEditorToolWindow.TrashIcon.image, tooltip);
        }
    }
}
