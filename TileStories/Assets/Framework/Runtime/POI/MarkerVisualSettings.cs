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

        // Wall-local font library (null = the prefab's framework default FontLibrary.asset),
        // same fallback rule as WallIconLibrary above. The wall's chosen default font key.
        public FontKeyLibrary FontLibrary;
        public string LabelFontKey = DefaultLabelFontKey;

        // Label typography limits: the ONE answer to "how far can these go", shared by this runtime
        // clamp and both editor UIs (Labels, Text & Fonts and the hierarchy "Marker Label Style"
        // window), so a slider can never offer a value the runtime silently clamps away.
        public const float LabelGapRatioMin = 0f;
        public const float LabelGapRatioMax = 1f;
        public const float LabelFontSizeRatioMin = 0.08f;
        public const float LabelFontSizeRatioMax = 1.5f;
        public const string DefaultLabelFontKey = "liberation_sans";

        public static float ClampLabelGapRatio(float value) => Mathf.Clamp(value, LabelGapRatioMin, LabelGapRatioMax);
        public static float ClampLabelFontSizeRatio(float value) => Mathf.Clamp(value, LabelFontSizeRatioMin, LabelFontSizeRatioMax);
        public static string ResolveLabelFontKey(string key) => string.IsNullOrWhiteSpace(key) ? DefaultLabelFontKey : key.Trim();

        // Layout (fractions of the symbol diameter) and ring spin speed
        public Vector2 BadgeDirection = new Vector2(0.7f, 0.7f);
        public float BadgeSizeRatio = 0.36f;
        public float RingSizeRatio = 1.18f;
        public float ContourSpinDegPerSecond = 60f;
        public float LabelGapRatio = 0.075f;
        public float LabelFontSizeRatio = 0.25f;

        // Icon colour (symbol + badge icons) and the symbol icon's size inside the symbol. The limits
        // are shared with the editor slider so it can never offer a value the runtime clamps away.
        public const float IconSizeRatioMin = 0.35f;
        public const float IconSizeRatioMax = 0.9f;
        public const string DefaultIconColorHex = "#F2ECD3";
        public Color IconColor = ParseColorOr(DefaultIconColorHex, Color.white);
        public float IconSizeRatio = 0.56f;

        // The framework look with nothing authored: gold outline, no badge, circle
        public static MarkerVisualSettings Default(MarkerShape shape = MarkerShape.Circle,
            MarkerOutlineMode outlineMode = MarkerOutlineMode.Uniform, bool useBadge = false) =>
            new MarkerVisualSettings { Shape = shape, OutlineMode = outlineMode, UseBadge = useBadge };

        // Resolve every wall-level marker field. Missing/invalid values degrade to a no-op look.
        // fontLibrary is the ALREADY-RESOLVED library to use (wall override if WallSession found
        // one, else the framework default) -- Resolve just reads the wall's chosen key out of it.
        public static MarkerVisualSettings Resolve(WallConfigData config, SpriteKeyLibrary wallIconLibrary,
            FontKeyLibrary fontLibrary = null)
        {
            var settings = new MarkerVisualSettings { WallIconLibrary = wallIconLibrary, FontLibrary = fontLibrary };
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
            settings.LabelGapRatio = ClampLabelGapRatio(config.label_gap_ratio);
            settings.LabelFontSizeRatio = ClampLabelFontSizeRatio(config.label_font_size_ratio);
            settings.LabelFontKey = ResolveLabelFontKey(config.label_font_key);
            settings.IconColor = ParseColorOr(config.icon_color_hex, settings.IconColor);
            settings.IconSizeRatio = Mathf.Clamp(config.icon_size_ratio, IconSizeRatioMin, IconSizeRatioMax);
            return settings;
        }

        // A "#RRGGBB" config colour, or the fallback when it is missing or malformed
        private static Color ParseColorOr(string hex, Color fallback) =>
            !string.IsNullOrWhiteSpace(hex) && ColorUtility.TryParseHtmlString(hex.Trim(), out var parsed) ? parsed : fallback;

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
