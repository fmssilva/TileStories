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

        // LEGACY fallback only -- prefer marker_outline_mode + marker_use_badge
        // below for new walls. Still read by
        // MarkerVisualsParser.DeriveOutlineAndBadgeFromLegacyStyle when
        // marker_outline_mode is absent, kept for walls authored before the split.
        // "outline_gold" / "outline_same_hue" / "badge". Missing/unrecognised falls
        // back to MarkerVisualsParser.DefaultStyle.
        public string marker_style;

        // "circle" / "rounded_square" / "hexagon" / "diamond" / "star" / "none".
        public string marker_shape;

        // "circle" / "rounded_square" / "hexagon" / "diamond" / "star" / "none".
        // Independent of marker_shape -- a wall can have a hexagon symbol with a
        // circle badge, or a badge with no background shape at all. Missing/absent
        // falls back to "circle", same as marker_shape's own default.
        public string badge_shape;

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
        // colour and KnownIcons lookup (section 7 Step 3) -- this list is a hand-picked
        // exception list, not a replacement taxonomy.
        public List<CategoryStyleEntry> category_styles = new();

        // Optional, additive badge taxonomy for a second semantic axis.
        // Each entry defines one selectable badge type (name/icon/color).
        public List<BadgeCategoryEntry> badge_categories = new();

        // Optional status-outline levels editable per wall.
        // Empty/missing -> runtime falls back to StatusRamp.Levels defaults.
        public List<OutlineLevelEntry> outline_levels = new();

        // Optional hierarchy levels editable per wall.
        // Empty/missing -> runtime falls back to MarkerHierarchyResolver.Fallback.
        public List<HierarchyLevelEntry> hierarchy_levels = new();

        public List<POIData> pois = new();
        public List<CalibrationAnchor> calibration_anchors = new();

        // Optional wall-level effect parameter defaults. When present, these values
        // are passed to each marker's effect components at spawn time, overriding
        // the components' compiled-in [SerializeField] defaults. When absent/null,
        // effects fall back to their compiled-in defaults -- fully backward compatible
        // with configs authored before this field existed.
        public EffectDefaults effect_defaults;

        // Optional LOD settings. When absent, LODController uses defaults:
        // 4-tier bands (15m/30m/60m/Inf), hybrid density, frustum cull on.
        public LodSettings lod_settings = new();
    }

    [Serializable]
    public class EffectDefaults
    {
        // Gentle scale "breathing" -- cheapest "worth a look" cue.
        public PulseDefaults pulse = new();

        // Three concentric waves with center-first flow.
        public SunDefaults sun = new();

        // Single reusable accent ring/disc (RingPulse / SimpleSun / Beacon).
        public AccentDefaults accent = new();

        [Serializable]
        public class PulseDefaults
        {
            [Tooltip("How much the marker grows/shrinks per cycle (0=none, 0.45=45% swing).")]
            public float amplitude = 0.18f;
            [Tooltip("Seconds per full pulse cycle.")]
            public float period = 1.6f;
        }

        [Serializable]
        public class SunDefaults
        {
            [Tooltip("Seconds per full sun animation cycle.")]
            public float period = 1.8f;
            [Tooltip("Delay (in cycle units) between inner/middle/outer wave layers.")]
            public float stagger = 0.12f;
            [Tooltip("Starting alpha for the innermost ring/disc.")]
            public float innerAlpha = 0.55f;
            [Tooltip("Starting alpha for the middle ring/disc.")]
            public float middleAlpha = 0.36f;
            [Tooltip("Starting alpha for the outermost ring/disc.")]
            public float outerAlpha = 0.2f;
            [Tooltip("Tint colour for the sun effect rings/discs (hex string, e.g. '#F2CA71').")]
            public string tint_color_hex = "#F2CA71";
        }

        [Serializable]
        public class AccentDefaults
        {
            [Tooltip("Diameter of the accent ring/disc as a fraction of the symbol size.")]
            public float size = 0.24f;
            [Tooltip("Starting alpha of the accent (always visible level).")]
            public float baseAlpha = 0.28f;
            [Tooltip("Outer radius scale for contour-style accents.")]
            public float contourOuterScale = 0.90f;
            [Tooltip("Inner radius scale for contour-style accents.")]
            public float contourInnerScale = 0.80f;
            [Tooltip("Radius scale for filled-circle-style accents.")]
            public float filledRadiusScale = 0.84f;
            [Tooltip("How much the accent grows per breathe cycle (0=none, 0.4=40% swing).")]
            public float breatheAmplitude = 0.15f;
            [Tooltip("Seconds per full breathe/beacon cycle.")]
            public float period = 2.0f;
            [Tooltip("Scale at cycle start for beacon motion.")]
            public float beaconStartScale = 1.0f;
            [Tooltip("Scale at cycle end for beacon motion.")]
            public float beaconEndScale = 1.8f;
            [Tooltip("Tint colour for the accent effect (hex string, e.g. '#F2CA71').")]
            public string tint_color_hex = "#F2CA71";
        }
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

        // Free-text note shown in the authoring tool's details popup. Not read by
        // runtime -- purely authoring metadata for this wall's taxonomy.
        public string details;
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

        // Free-text note shown in the authoring tool's details popup. Not read by
        // runtime -- purely authoring metadata for this wall's taxonomy.
        public string details;
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

        // Ring line style key: solid/dash_long/dash_medium/dash_short/dotted, or
        // any custom key present in the wall's icon library (section 20.3).
        public string line_style;

        // Optional "#RRGGBB" tint override.
        public string color_hex;

        // Optional width override in UI-space units.
        public float ring_width;

        // Free-text note shown in the authoring tool's details popup. Not read by
        // runtime -- purely authoring metadata for this wall's taxonomy.
        public string details;
    }

    [Serializable]
    public class HierarchyLevelEntry
    {
        // Stable key, e.g. "level_1" -- written to POIData.hierarchy_level_key.
        public string key;

                // Developer-facing label, e.g. "1" or "Landmark".
        public string label;

        // Authorable priority for this hierarchy level (lower = higher priority).
        // Convention: a value >= 1 is an explicit developer-assigned priority; a value
        // <= 0 (or the field absent on legacy configs) means "unset" and MarkerHierarchyResolver
        // falls back to the level's table position (1-based). This keeps old configs working
        // and lets a wall reorder priority without shuffling table rows. Consumed by LOD
        // count-cap truncation (lowest priority hidden first) and future displacement tiebreak.
        public int priority;

        // Free text, developer's own notes (shown in the authoring tool details popup).
        public string details;

        // Symbol diameter, real-world centimetres. Converted to metres at the single
        // call site in MarkerView (/100). One conversion, not per-POI.
        public float size_cm;

        // Persistent label visible at this level? false = no label.
        public bool show_label;

        // "none" | "sun_contours" | "sun_circles" -- parsed by MarkerHierarchyResolver.
        public string sun_effect;

        // "none" | "ring_pulse" | "simple_sun" | "beacon" -- parsed by MarkerHierarchyResolver.
        public string accent_effect;

        // Independent of both effect families above -- standalone pulse component.
        public bool pulse;

        // Meaningful only when wall outline mode != none. Controls ring rotation.
        public bool rotate_contour;

                // Seconds after spawn before fade/scale-in begins.
        public float reveal_delay_s;

        // Seconds for the fade/scale-in animation itself. 0 = instant pop-in
        // (jarring -- use only as an explicit artistic choice). Default taper:
        // 0.5s at Level 1 down to 0.25s at Level 5.
        public float reveal_duration_s;
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

        // Optional badge taxonomy key resolved from WallConfigData.badge_categories.
        // Used only when marker_use_badge is enabled.
        public string badge_category;

        // Optional discrete status level key selected in the editor.
        // Runtime still resolves with status_pct; this key is for authoring UX.
        public string status_level_key;

        // Hierarchy level key: selects size/label/effects/reveal-delay from
        // the wall's hierarchy_levels table (section 2.3). Empty/missing ->
        // MarkerView uses MarkerHierarchyResolver.Fallback.
        public string hierarchy_level_key;

        // Hero icon replacement (section 2.3): opts into overriding just this
        // POI's symbol icon via custom_symbol_key. Category fill colour, ring,
        // and badge are unaffected. Only meaningful when true.
        public bool has_custom_symbol;

        // Icon key from IconLibrary.asset used when has_custom_symbol is true.
        public string custom_symbol_key;

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

        [Serializable]
    public class LodSettings
    {
        public bool enabled = true;
        public List<LodBandEntry> bands = new(); // see defaults below
        public float hysteresis_margin_m = 0.5f;
        public float transition_fade_duration_s = 0.3f;
        public float evaluation_interval_s = 0.2f;

        public string density_response_mode = "hybrid"; // none|select_hide|cluster|shrink_and_fade|hybrid
        public float density_radius_px = 40f;            // matches MarkerOverlapResolver's threshold
        public int shrink_start_neighbor_count = 2;       // hybrid/shrink_and_fade: density response begins here
        public int cluster_min_count = 5;                 // select_hide/cluster/hybrid: escalate to hide-or-cluster here
        public bool density_safety_escalation_enabled = true; // see §6.2
        public float density_safety_escalation_multiplier = 2f; // see §6.2
        public string cluster_icon_mode = "pie_and_count"; // pie_and_count|dominant_category|count_only
        public string displacement_tiebreak = "symmetric"; // symmetric|lower_priority_only — see §6

        public bool frustum_culling_enabled = true;
        public float fov_culling_margin_deg = 10f;

        public bool zoom_enabled = true;
        public float zoom_min = 1f;
        public float zoom_max = 4f;          // hard clamp, see §9
        public float zoom_tap_step = 1.5f;
        public int zoom_tap_levels = 2;      // 3rd tap/click returns to 1x
        public float zoom_transition_speed_s = 0.25f; // animation duration for tap/double-tap/button zoom changes
        public bool zoom_show_ui_buttons = true;
    }

    [Serializable]
    public class LodBandEntry
    {
        // band applies up to this distance (meters)
        public float max_distance_m;

        // -1 = unlimited ("show all" band)
        public int max_visible_count;
    }
}
