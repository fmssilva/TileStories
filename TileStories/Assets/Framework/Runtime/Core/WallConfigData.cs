using System;
using System.Collections.Generic;
using UnityEngine;

namespace TileStories
{
    [Serializable]
    public class WallConfigData
    {
        public string wall_id;
        public string wall_name;
        public int immersal_map_id;

        // "outline_gold" / "outline_same_hue" / "badge". Missing/unrecognised falls
        // back to MarkerVisualsParser.DefaultStyle -- unlike has_captured_position,
        // an absent value here has one unambiguous safe meaning ("use the framework
        // default"), so no separate has_* flag is needed.
        public string marker_style;

        // "circle" / "rounded_square" / "hexagon" / "diamond" / "star".
        public string marker_shape;

        // Optional split-mode controls for marker visuals.
        // marker_outline_mode: "gold" / "same_hue" / "none"
        // marker_use_badge: true/false
        // If these are absent, WallSession derives behavior from marker_style.
        public string marker_outline_mode;
        public bool marker_use_badge;

        // Optional wall-local icon library load path (Resources-relative, without
        // extension), e.g. "MarkerSymbols/LivingRoom_IconLibrary".
        // When set, WallSession loads this library at runtime and MarkerView uses
        // it for category/badge icon resolution instead of the prefab default.
        public string marker_icon_library_resources_path;

        // Optional, additive per-category colour/icon overrides. Any category not
        // listed here still falls through to CategoryPalette's deterministic hash
        // colour and KnownIcons lookup (§7 Step 3) -- this list is a hand-picked
        // exception list, not a replacement taxonomy.
        public List<CategoryStyleEntry> category_styles = new();

        // Optional, additive badge taxonomy for a second semantic axis.
        // Each entry defines one selectable badge type (name/icon/color).
        public List<BadgeCategoryEntry> badge_categories = new();

        // Optional status-outline levels editable per wall.
        // Empty/missing -> runtime falls back to StatusRamp.Levels defaults.
        public List<OutlineLevelEntry> outline_levels = new();

        public List<POIData> pois = new();
        public List<CalibrationAnchor> calibration_anchors = new();
    }

    [Serializable]
    public class CategoryStyleEntry
    {
        public string category;   // must match POIData.category exactly

        // "#RRGGBB". Empty/omitted -> this category keeps the hash-generated colour.
        public string color_hex;

        // Matches an entry key in IconLibrary.asset. Empty/omitted -> no icon
        // (colour-only circle), same as any other unlisted category.
        public string icon_key;
    }

    [Serializable]
    public class BadgeCategoryEntry
    {
        // Stable key stored in POIData.badge_category.
        public string key;

        // Human label shown in editor dropdowns.
        public string label;

        // Optional "#RRGGBB" tint for badge background.
        public string color_hex;

        // Icon key from IconLibrary.asset.
        public string icon_key;
    }

    [Serializable]
    public class OutlineLevelEntry
    {
        // Stable key for this level (e.g. "intact", "mid", "severe").
        public string key;

        // Human label shown in the editor dropdown (e.g. "25%", "Damaged").
        public string label;

        // Numeric anchor used by runtime resolve logic.
        public float pct;

        // Ring line style key: solid/dash_long/dash_medium/dash_short/dotted.
        public string line_style;

        // Optional "#RRGGBB" tint override.
        public string color_hex;

        // Optional width override in UI-space units.
        public float ring_width;
    }

    [Serializable]
    public class POIData : ISerializationCallbackReceiver
    {
        public string id;
        public string name;
        public string category;
        public float x_norm;
        public float y_norm;

        public CapturedPosition captured_position;
        public bool has_captured_position;
        public string captured_position_source;
        public long captured_position_timestamp;
        public string summary;

        // Destruction status, 0-100. Same has_* guard as captured_position and for
        // the same reason: a POI legitimately at 0% ("fully intact") must never be
        // indistinguishable from a POI whose wall doesn't track status at all (e.g.
        // a mural has no "destroyed" axis). MarkerView must check has_status before
        // reading status_pct for anything.
        public float status_pct;
        public bool has_status;

        // A genuinely separate third state: this wall DOES track status, and this
        // specific POI's fate is a real historical unknown (not merely undocumented in
        // this dataset). Only meaningful when has_status is true. Rendered identically
        // across all three MarkerStyle values as a neutral-grey "?" badge, overriding
        // the style-specific ring/fade/badge rendering for that one POI.
        public bool status_unknown;

        // Marks this POI as a hierarchy "hero": persistent label and any enabled
        // marker effects (pulse/glow) are only shown for hero POIs. Defaults to
        // false when absent -- a safe default here (unlike status) because "not a
        // hero" is an ordinary, unsurprising fallback, not a data-loss risk.
        public bool is_hero;

        // Comma-separated effect tokens: "pulse", "sun_contours", "sun_circles",
        // "ring_pulse", "simple_sun", "beacon". Parsed by
        // MarkerVisualsParser.ParseEffectFlags (section 19.9). Independent of
        // is_hero as of 2026-08-01 -- is_hero now controls only the persistent
        // label. Empty/missing -> no effects, same safe default reasoning as is_hero.
        public string effect_mode;

        // Rotates the status ring (MarkerStyle.OutlineGold/OutlineSameHue only --
        // silently does nothing otherwise, since there's no ring to rotate for
        // Badge style or an unknown-status POI). Safe default false.
        public bool rotate_contour;

        // Optional badge taxonomy key resolved from WallConfigData.badge_categories.
        // Used only when marker_use_badge is enabled.
        public string badge_category;

        // Optional discrete status level key selected in the editor.
        // Runtime still resolves with status_pct; this key is for authoring UX.
        public string status_level_key;

        public void OnBeforeSerialize()
        {
            if (!has_captured_position)
            {
                captured_position = null;
            }
        }

        public void OnAfterDeserialize()
        {
            if (!has_captured_position)
            {
                captured_position = null;
                if (string.IsNullOrEmpty(captured_position_source))
                {
                    captured_position_source = null;
                }

                if (captured_position_timestamp < 0)
                {
                    captured_position_timestamp = 0;
                }
            }

            if (!has_status)
            {
                status_pct = 0f;
                status_unknown = false;
                status_level_key = null;
            }
        }
    }

    [Serializable]
    public class CapturedPosition
    {
        public float x;
        public float y;
        public float z;

        public Vector3 ToVector3() => new(x, y, z);
    }

    [Serializable]
    public class CalibrationAnchor
    {
        public string id;
        public float x_norm;
        public float y_norm;
        public CapturedPosition captured_position;
    }
}