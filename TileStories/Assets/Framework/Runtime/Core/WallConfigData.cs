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
        // 3-tier bands (2m/7m/9999m, counts -1/15/5), hybrid density, frustum cull on.
                public LodSettings lod_settings = new();

        // --- Custom keyword field definitions (spec _2.6 section 3 / 15) ---
        // Developer-defined search axes (e.g. "architect", "period", "material").
        // Each definition appears as an editable row in every specific POI section
        // and in the Global Scene Keyword Fields table. System axes (category/hierarchy/
        // badge/outline) are NOT listed here -- their keywords come from the taxonomy
        // tables and are indexed automatically at Build time.
        public List<SearchFieldDefinition> search_fields = new();

        // --- Selection & zoom-on-select infrastructure (spec _2.6 section 11) ---
        // Closed behaviour choice for which tap target triggers an auto-zoom.
        // Framework-controlled (not wall-authored taxonomy), so it is an enum rather
        // than a free-form string like marker_style / marker_shape.
        public enum ZoomOnSelectTrigger { None, Marker, Cluster, Both }

        // When true, tapping a marker dims all other markers to partial alpha so the
        // selected one is the clear focal point; re-tapping it clears the highlight.
        // Defaults true (the section 11 behaviour).
        [Tooltip("Dim non-selected markers to partial alpha when a marker is selected.")]
        public bool selection_highlight_enabled = true;

        // Auto-zoom only when the selected marker has at least this many
        // screen-space neighbours (LODController's last density evaluation). Stops
        // us zooming in on isolated markers with nothing to disambiguate.
        public int zoom_on_select_density_threshold = 2;

        // Multiplier applied to the current ARZoomState.ZoomFactor when zoom-on-
        // select fires (e.g. 2.0 doubles the zoom-in), clamped to [zoom_min,
        // zoom_max] by ARZoomController.SetZoomAnimated.
        public float zoom_on_select_factor = 2.0f;

        // Master toggle for zoom-on-select: when true, selecting a dense marker
        // auto-zooms per zoom_on_select_factor. When false, selection only
        // highlights (if selection_highlight_enabled).
        public bool zoom_on_select_enabled = true;

        // Which tap target auto-zooms: Marker active in Block 2; Cluster/Both
        // reserved for Block 3 cluster-tap wiring (spec _2.6 section 11).
        public ZoomOnSelectTrigger zoom_on_select_trigger = ZoomOnSelectTrigger.Marker;

        // --- Select, filter & search UI settings (spec _2.6 section 3) ---
        // Author-selectable search interaction model.
        // Convention values: "explicit" | "dynamic" | "scoped" | "faceted" | "auto_complete".
                // Free-form string so future walls can use values not foreseen here.
        // Absent/null defaults to "explicit" (plain keyword search bar).
        public string search_mode;

        // No-results message shown when a search or filter returns zero POIs.
        // Supports {query} placeholder for the user's search term.
        public string no_results_message = "No matches for \"{query}\" - try removing a filter.";

        // How many recent search queries to remember locally (PlayerPrefs).
        public int recent_search_count = 5;

        // Whether to show suggested categories based on the wall's actual POI
        // distribution. Computed live, not manually curated.
        public bool show_suggested_categories = true;

        // Where suggestion terms come from (spec _2.6 section 13).
        // "category_distribution" (default): top-N categories by live POI count.
        // "recent_first": visitor's recent queries first, then category back-fill.
        public string suggested_source = "category_distribution";

        // --- Minimap settings (spec _2.6 section 8) ---
        // Whether the minimap feature is enabled for this wall.
        public bool minimap_enabled = true;

        // "always" shows the minimap permanently; "toggle" shows a button to
        // expand/collapse it.
        public string minimap_visibility = "toggle";

        // "dots_only" (plain colored dots), "category_colored_dots" (dots
        // colored by category), "mini_icons" (scaled-down marker icons).
        public string minimap_icon_style = "category_colored_dots";

        // --- View mode settings (spec _2.6 section 10) ---
        // The result view shown by default when the wall loads.
        // "list" | "minimap" | "camera_highlight"
        public string default_result_view = "list";

        // --- Voice search settings (spec _2.6 section 12) ---
        // Off by default -- real permission and reliability caveats (iOS requires
        // two separate permission prompts, on-device model needs initial download).
        public bool voice_search_enabled = false;

        // "all" requires every remaining token to match; "any" matches if any
        // token matches. Default "all" for precision over recall.
        public string voice_search_match_mode = "all";

        // --- Voice search indicator (spec _2.6 section 12) ---
        // How the "listening/processing" voice state is surfaced to the visitor.
        // "mic_text" (default): the mic button text flips to "..." while listening/processing.
        //   Behavior-identical to the legacy inline implementation, so existing walls are
        //   unaffected unless they explicitly opt in.
        // "listen_bar": also renders a dedicated, explicitly-labelled listen/progress bar.
        // Free-form string so future walls can introduce styles without a Framework code
        // change; unknown values fall back to "mic_text" (logged once) in
        // VoiceActivityIndicatorView.ParseStyle.
        public string voice_activity_indicator_style = "mic_text";
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

        // Taxonomy-level search keywords: every POI with this category is
        // automatically indexed with these terms at build time, so the wall
        // author only enters them once per category instead of per-POI.
        public List<string> search_keywords = new();
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

        // Taxonomy-level search keywords: every POI with this badge type is
        // automatically indexed with these terms at build time, so the wall
        // author only enters them once per badge instead of per-POI.
        public List<string> search_keywords = new();
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

        // Taxonomy-level search keywords: every POI with this outline level is
        // automatically indexed with these terms at build time, so the wall
        // author only enters them once per level instead of per-POI.
        public List<string> search_keywords = new();
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

        // Hierarchy-level search keywords: every POI at this level is
        // automatically indexed with these terms at build time.
        public List<string> search_keywords = new();

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

        // Per-POI freeform search keywords (the "Others" bucket).
        // Indexed at keyword rank, no field-key context.
        // Pre-existing field: kept for backward compatibility and as the default
        // "Others" bucket when no custom search_fields are defined.
        public List<string> search_keywords = new();

        // Per-custom-field keyword lists, keyed to SearchFieldDefinition.key entries
        // on WallConfigData.search_fields. Indexed at keyword rank alongside
        // search_keywords above -- the field key is used only by the Editor for
        // display; the runtime index treats all keyword matches equally.
        public List<POISearchKeywordField> search_keyword_fields = new();

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

            // JSON can serialize a nullable List<T> as null; guard against it so
            // downstream search-index code never NPEs on absent keyword lists.
            if (search_keywords == null)
                search_keywords = new();
            if (search_keyword_fields == null)
                search_keyword_fields = new();
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
        public string cluster_band_source = "centroid";            // centroid|nearest_member|farthest_member (cluster centroid band)
        public bool cluster_band_hysteresis_enabled = true;        // reuse hysteresis_margin_m for cluster band index stability
        public int cluster_dissolve_grace_cycles = 3;              // pooled cluster view survival cycles before fading out (0 = immediate)
        // displacement_tiebreak removed — moved to DisplacementSettings in _2.5_Displacement.md (§11 of _2.4_Marker_LOD.md).

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

        // Authoring-only per-band note surfaced by the authoring tool's
        // "Details" popup (EntryDetailsPopup) -- same idiom as the
        // CategoryStyleEntry / OutlineLevelEntry / BadgeCategoryEntry detail
        // fields. The runtime LODController reads only max_distance_m and
        // max_visible_count, so adding this introduces no runtime behavior.
        public string details = string.Empty;
    }

    // One developer-defined search axis (spec _2.6 section 3 / 15).
    // Stored in WallConfigData.search_fields. System axes (category, hierarchy,
    // badge, outline) are not in this list -- their keywords come from the
    // respective taxonomy tables. The reserved "others" axis is never in this
    // list either: it is always rendered last in the per-POI editor and maps to
    // the flat POIData.search_keywords field (the legacy / freeform bucket).
    [Serializable]
    public class SearchFieldDefinition
    {
        // Stable identifier used to link this definition to per-POI keyword lists
        // (POISearchKeywordField.field_key). Never change after authoring begins.
        public string key;

        // Human-readable label shown in the Specific Marker editor.
        public string label;

        // When true, a warning is shown (and validation fires) if a POI's keyword
        // list for this field is empty. The developer opts specific fields into
        // required status; most will be optional.
        public bool forced;

        // Free-text guidance note surfaced via the Details popup (same pattern
        // as CategoryStyleEntry.details and other taxonomy entries).
        public string details;
    }

    // Per-POI keyword list for one custom SearchFieldDefinition.
    // Stored in POIData.search_keyword_fields.
    // The runtime POISearchIndex indexes all keywords at RANK_KEYWORD regardless
    // of which field they came from; field_key is purely an authoring-time seam
    // that keeps the Editor organised.
    [Serializable]
    public class POISearchKeywordField
    {
        // Matches SearchFieldDefinition.key on the wall config.
        public string field_key;

        // The actual keyword strings for this field on this POI.
        public List<string> keywords = new();
    }
}
