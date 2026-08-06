using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    public partial class POIAuthoringToolWindow
    {
        private static readonly string[] OutlineModeOptions = { "gold", "same_hue", "free_colors" };
        private static readonly string[] OutlineModeLabels = { "Gold", "Same Hue", "Free Colors" };
        private static readonly string[] LineStyleOptions = { "solid", "dash_long", "dash_medium", "dash_short", "dotted" };
        private static readonly string[] LineStyleLabels = { "Continuous", "Big Dashed", "Medium Dashed", "Small Dashed", "Dots" };
        private static readonly string[] ShapeOptions = { "circle", "rounded_square", "hexagon", "diamond", "star", "none" };
        private static readonly string[] ShapeLabels = { "Circle", "Rounded Square", "Hexagon", "Diamond", "Star", "None" };

        private static readonly Color GlobalSectionColor = new Color(0.35f, 0.55f, 0.95f);
        private static readonly Color InnerSectionColor = new Color(0.5f, 0.5f, 0.5f, 0.6f);

        private static GUIContent _trashIcon;
        private static GUIContent TrashIcon => _trashIcon ?? (_trashIcon = EditorGUIUtility.IconContent("d_TreeEditor.Trash"));
    }
}