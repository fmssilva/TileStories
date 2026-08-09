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

        // Sun effect options for hierarchy level table (maps to HierarchyLevelEntry.sun_effect).
        private static readonly string[] SunEffectOptions = { "none", "sun_contours", "sun_circles" };
        private static readonly string[] SunEffectLabels = { "None", "Contours", "Circles" };

        // Accent effect options for hierarchy level table (maps to HierarchyLevelEntry.accent_effect).
        private static readonly string[] AccentEffectOptions = { "none", "ring_pulse", "simple_sun", "beacon" };
        private static readonly string[] AccentEffectLabels = { "None", "Ring Pulse", "Simple Sun", "Beacon" };

        // Show-label options (explicit wording per §6 of 2.3 doc, clearer than bare checkbox).
        private static readonly string[] ShowLabelOptions = { "Show Label", "NOT show Label" };

        private static readonly Color GlobalSectionColor = new Color(0.35f, 0.55f, 0.95f);
        private static readonly Color InnerSectionColor = new Color(0.5f, 0.5f, 0.5f, 0.6f);

        private static GUIContent _trashIcon;
        private static GUIContent TrashIcon => _trashIcon ?? (_trashIcon = EditorGUIUtility.IconContent("d_TreeEditor.Trash"));
    }
}