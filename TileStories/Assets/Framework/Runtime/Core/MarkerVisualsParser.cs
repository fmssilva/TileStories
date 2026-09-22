using UnityEngine;

namespace TileStories
{
    // Parses the wall config's marker_shape / badge_shape / marker_outline_mode / badge_corner
    // strings into their enums. Kept as strings in JSON (see _2.2.1_Marker_Design.md, design
    // principle 2). An unrecognised or missing value logs once and
    // falls back to a sane default rather than silently defaulting to enum value 0.
    public static class MarkerVisualsParser
    {
        public const MarkerShape DefaultShape = MarkerShape.Circle;

        public static MarkerShape ParseShape(string raw)
        {
            switch (raw)
            {
                case "circle": return MarkerShape.Circle;
                case "rounded_square": return MarkerShape.RoundedSquare;
                case "hexagon": return MarkerShape.Hexagon;
                case "diamond": return MarkerShape.Diamond;
                case "star": return MarkerShape.Star;
                case "none": return MarkerShape.None;
                default:
                    if (!string.IsNullOrEmpty(raw))
                        Debug.LogWarning($"[MarkerVisualsParser] Unknown marker_shape '{raw}', falling back to {DefaultShape}.");
                    return DefaultShape;
            }
        }

        public static bool TryParseShape(string raw, out MarkerShape shape)
        {
            switch (raw)
            {
                case "circle": shape = MarkerShape.Circle; return true;
                case "rounded_square": shape = MarkerShape.RoundedSquare; return true;
                case "hexagon": shape = MarkerShape.Hexagon; return true;
                case "diamond": shape = MarkerShape.Diamond; return true;
                case "star": shape = MarkerShape.Star; return true;
                case "none": shape = MarkerShape.None; return true;
                default:
                    shape = default;
                    return false;
            }
        }

        public const MarkerOutlineMode DefaultOutlineMode = MarkerOutlineMode.Uniform;
        public const bool DefaultUseBadge = false;

            // Parse the outline mode from wall config. Missing/unrecognized values fall back safely.
        public static MarkerOutlineMode ParseOutlineMode(string raw)
        {
            switch (raw)
            {
                case "uniform": return MarkerOutlineMode.Uniform;
                case "same_hue": return MarkerOutlineMode.SameHue;
                case "per_type": return MarkerOutlineMode.PerType;
                case "none": return MarkerOutlineMode.None;
                default:
                    if (!string.IsNullOrEmpty(raw))
                        Debug.LogWarning($"[MarkerVisualsParser] Unknown marker_outline_mode '{raw}', falling back to {DefaultOutlineMode}.");
                    return DefaultOutlineMode;
            }
        }

        public static bool TryParseOutlineMode(string raw, out MarkerOutlineMode outlineMode)
        {
            switch (raw)
            {
                case "uniform": outlineMode = MarkerOutlineMode.Uniform; return true;
                case "same_hue": outlineMode = MarkerOutlineMode.SameHue; return true;
                case "per_type": outlineMode = MarkerOutlineMode.PerType; return true;
                case "none": outlineMode = MarkerOutlineMode.None; return true;
                default:
                    outlineMode = default;
                    return false;
            }
        }

        // Badge corner key -> normalised direction from the symbol centre to the badge centre.
        // (0.7, 0.7) = top right, the composition chosen in the interactive prototype.
        public static bool TryParseBadgeCorner(string raw, out Vector2 direction)
        {
            const float d = 0.7f;
            switch (raw)
            {
                case "top_right": direction = new Vector2(d, d); return true;
                case "top_left": direction = new Vector2(-d, d); return true;
                case "bottom_right": direction = new Vector2(d, -d); return true;
                case "bottom_left": direction = new Vector2(-d, -d); return true;
                default: direction = new Vector2(d, d); return false;
            }
        }

        public static string NormalizeLineStyle(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return "solid";

            string trimmed = raw.Trim();
            switch (trimmed)
            {
                case "continuous":
                case "solid":
                    return "solid";
                case "big_dashed":
                case "dash_long":
                    return "dash_long";
                case "medium_dashed":
                case "dash_medium":
                    return "dash_medium";
                case "small_dashed":
                case "dash_short":
                    return "dash_short";
                case "big_dots":
                case "small_dots":
                case "dotted":
                    return "dotted";
                default:
                    // Unknown keys are passed through unchanged (section 20.3) so a
                    // wall can author a custom line style (e.g. "line_wavy") that
                    // resolves from its icon library. The ring view falls back to
                    // solid if the key isn't present in the library.
                    return trimmed;
            }
        }
    }
}
