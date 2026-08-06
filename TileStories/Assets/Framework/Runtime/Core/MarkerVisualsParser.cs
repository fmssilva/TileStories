using UnityEngine;

namespace TileStories
{
    // Parses the wall config's freeform marker_style / marker_shape strings into
    // their enums. Kept as strings in JSON (see design principle 2 in
    // _2_2_Marker_Design.md §4). An unrecognised or missing value logs once and
    // falls back to a sane default rather than silently defaulting to enum value 0.
    public static class MarkerVisualsParser
    {
        public const MarkerStyle DefaultStyle = MarkerStyle.OutlineGold;
        public const MarkerShape DefaultShape = MarkerShape.Circle;

        public static MarkerStyle ParseStyle(string raw)
        {
            switch (raw)
            {
                case "outline_gold": return MarkerStyle.OutlineGold;
                case "outline_same_hue": return MarkerStyle.OutlineSameHue;
                case "badge": return MarkerStyle.Badge;
                default:
                    if (!string.IsNullOrEmpty(raw))
                        Debug.LogWarning($"[MarkerVisualsParser] Unknown marker_style '{raw}', falling back to {DefaultStyle}.");
                    return DefaultStyle;
            }
        }

        public static bool TryParseStyle(string raw, out MarkerStyle style)
        {
            switch (raw)
            {
                case "outline_gold": style = MarkerStyle.OutlineGold; return true;
                case "outline_same_hue": style = MarkerStyle.OutlineSameHue; return true;
                case "badge": style = MarkerStyle.Badge; return true;
                default:
                    style = default;
                    return false;
            }
        }

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

        public const MarkerOutlineMode DefaultOutlineMode = MarkerOutlineMode.Gold;
        public const bool DefaultUseBadge = false;

        // Parse split outline mode from wall config. Missing/unrecognized values
        // fall back safely, and legacy marker_style still works through
        // DeriveOutlineAndBadgeFromLegacyStyle when marker_outline_mode is empty.
        public static MarkerOutlineMode ParseOutlineMode(string raw)
        {
            switch (raw)
            {
                case "gold": return MarkerOutlineMode.Gold;
                case "same_hue": return MarkerOutlineMode.SameHue;
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
                case "gold": outlineMode = MarkerOutlineMode.Gold; return true;
                case "same_hue": outlineMode = MarkerOutlineMode.SameHue; return true;
                case "none": outlineMode = MarkerOutlineMode.None; return true;
                default:
                    outlineMode = default;
                    return false;
            }
        }

        // Derive split controls from legacy marker_style for backward-compatible
        // behavior when marker_outline_mode is not authored.
        public static void DeriveOutlineAndBadgeFromLegacyStyle(string rawStyle, out MarkerOutlineMode outlineMode, out bool useBadge)
        {
            switch (ParseStyle(rawStyle))
            {
                case MarkerStyle.OutlineSameHue:
                    outlineMode = MarkerOutlineMode.SameHue;
                    useBadge = false;
                    return;
                case MarkerStyle.Badge:
                    outlineMode = MarkerOutlineMode.None;
                    useBadge = true;
                    return;
                default:
                    outlineMode = MarkerOutlineMode.Gold;
                    useBadge = false;
                    return;
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

        // Default when effect_mode is absent/empty in config.json -- a safe,
        // unsurprising default (no effects) since effects are opt-in per POI
        // and is_hero only controls the label, not which effects run.
        public const MarkerEffectFlags DefaultEffects = MarkerEffectFlags.None;

        // effect_mode is a comma-separated list, e.g. "pulse,sun_contours" or
        // just "beacon". Empty/missing -> None. Unknown tokens warn and skip
        // rather than silently dropping the entire value.
        public static MarkerEffectFlags ParseEffectFlags(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return DefaultEffects;

            var result = MarkerEffectFlags.None;
            foreach (var token in raw.Split(','))
            {
                switch (token.Trim())
                {
                    case "pulse": result |= MarkerEffectFlags.Pulse; break;
                    case "sun_contours": result |= MarkerEffectFlags.SunContours; break;
                    case "sun_circles": result |= MarkerEffectFlags.SunCircles; break;
                    case "ring_pulse": result |= MarkerEffectFlags.RingPulse; break;
                    case "simple_sun": result |= MarkerEffectFlags.SimpleSun; break;
                    case "beacon": result |= MarkerEffectFlags.Beacon; break;
                    case "": break; // tolerate trailing commas
                    default:
                        Debug.LogWarning($"[MarkerVisualsParser] Unknown effect_mode token '{token}', ignoring.");
                        break;
                }
            }
            return result;
        }
    }
}
