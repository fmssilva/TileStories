using UnityEngine;

namespace TileStories
{
    // The wall-level marker look (_2.2.1 / _2.2.2 / _2.2.3), resolved ONCE from WallConfigData.
    // WallSession, the POI Editor's Scene rig and the live Play Mode applier all build it here,
    // so "what does this config mean" has one answer. Plain data + pure resolve: no scene needed.
    public sealed class MarkerVisualSettings
    {
        public MarkerShape Shape = MarkerShape.Circle;
        public MarkerShape BadgeShape = MarkerShape.Circle;
        public MarkerOutlineMode OutlineMode = MarkerOutlineMode.Uniform;
        public bool UseBadge;

        // Whether the config actually authors each part; false = leave the prefab's own look alone
        public bool HasCategoryDefinitions = true;
        public bool HasShapeFromConfig = true;
        public bool HasOutlineLevels = true;

        // Wall-local icon library (null = the prefab's framework default library)
        public SpriteKeyLibrary WallIconLibrary;

        // Layout (fractions of the symbol diameter) and ring spin speed
        public Vector2 BadgeDirection = new Vector2(0.7f, 0.7f);
        public float BadgeSizeRatio = 0.36f;
        public float RingSizeRatio = 1.18f;
        public float ContourSpinDegPerSecond = 60f;

        // The framework look with nothing authored: gold outline, no badge, circle
        public static MarkerVisualSettings Default(MarkerShape shape = MarkerShape.Circle,
            MarkerOutlineMode outlineMode = MarkerOutlineMode.Uniform, bool useBadge = false) =>
            new MarkerVisualSettings { Shape = shape, OutlineMode = outlineMode, UseBadge = useBadge };

        // Resolve every wall-level marker field. Missing/invalid values degrade to a no-op look.
        public static MarkerVisualSettings Resolve(WallConfigData config, SpriteKeyLibrary wallIconLibrary)
        {
            var settings = new MarkerVisualSettings { WallIconLibrary = wallIconLibrary };
            if (config == null) return settings;

            settings.HasShapeFromConfig = MarkerVisualsParser.TryParseShape(config.marker_shape, out settings.Shape);
            if (!MarkerVisualsParser.TryParseShape(config.badge_shape, out settings.BadgeShape))
                settings.BadgeShape = MarkerShape.Circle;

            // A missing or unrecognised outline mode means NO outline (never a guessed default)
            if (!MarkerVisualsParser.TryParseOutlineMode(config.marker_outline_mode, out settings.OutlineMode))
                settings.OutlineMode = MarkerOutlineMode.None;
            settings.UseBadge = config.marker_use_badge;

            settings.HasCategoryDefinitions = config.category_styles != null && config.category_styles.Count > 0;
            settings.HasOutlineLevels = config.outline_levels != null && config.outline_levels.Count > 0;

            MarkerVisualsParser.TryParseBadgeCorner(config.badge_corner, out settings.BadgeDirection);
            settings.BadgeSizeRatio = Mathf.Clamp(config.badge_size_ratio, 0.2f, 0.5f);
            settings.RingSizeRatio = Mathf.Clamp(config.ring_size_ratio, 1f, 1.35f);
            settings.ContourSpinDegPerSecond = Mathf.Clamp(config.contour_spin_deg_per_s, 0f, 360f);
            return settings;
        }

        // Point the static palettes at this config. Every path that shows markers calls this, so the
        // three palettes and the level resolver can never disagree, and an empty table resets to the
        // framework defaults instead of keeping the previous wall's values.
        public static void ApplyPalettes(WallConfigData config)
        {
            if (config == null) return;

            if (config.category_styles != null && config.category_styles.Count > 0) CategoryPalette.Configure(config.category_styles);
            else CategoryPalette.ClearOverrides();

            BadgeCategoryPalette.Configure(config.badge_categories);

            if (config.outline_levels != null && config.outline_levels.Count > 0)
            {
                MarkerVisualsParser.TryParseOutlineMode(config.marker_outline_mode, out var mode);
                Color uniformColor = ParseUniformColor(config.outline_uniform_color_hex);
                StatusRamp.Configure(config.outline_levels, mode, uniformColor);
            }
            else StatusRamp.ResetToDefaults();

            MarkerHierarchyResolver.Configure(config.hierarchy_levels);
        }

        // Falls back to the framework's own gold when the hex is missing or malformed, mirroring the
        // schema default (WallConfigData.outline_uniform_color_hex) rather than a bare gray.
        private static Color ParseUniformColor(string hex)
        {
            if (!string.IsNullOrWhiteSpace(hex) && ColorUtility.TryParseHtmlString(hex, out var parsed))
                return parsed;
            ColorUtility.TryParseHtmlString("#E3BD72", out var fallback);
            return fallback;
        }
    }
}
