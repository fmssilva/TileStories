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

        // "circle" / "rounded_square" / "hexagon" / "diamond" / "star" / "none".
        public string marker_shape;

        // "circle" / "rounded_square" / "hexagon" / "diamond" / "star" / "none".
        // Independent of marker_shape -- a wall can have a hexagon symbol with a
        // circle badge, or a badge with no background shape at all. Missing/absent
        // falls back to "circle", same as marker_shape's own default.
        public string badge_shape;

        // Outline (status ring) mode: "uniform" (one shared, developer-adjustable colour, see
        // outline_uniform_color_hex) / "same_hue" (category hue drained toward black) / "per_type"
        // (each outline level below carries its own colour) / "none". Missing or unrecognised = no
        // outline.
        public string marker_outline_mode;

        // The single ring colour used when marker_outline_mode is "uniform". Defaults to the
        // framework's original gold. Ignored in every other mode (same_hue derives its colour from
        // the category, per_type from each outline level's own color_hex).
        public string outline_uniform_color_hex = "#E3BD72";

        // Show the badge (second meaning axis) on markers that name a badge_category.
        public bool marker_use_badge;

        // Badge placement and size on the symbol (_2.2.2). Corner: "top_right" / "top_left" /
        // "bottom_right" / "bottom_left". Size is a fraction of the symbol diameter.
        public string badge_corner = "top_right";
        public float badge_size_ratio = 0.36f;

        // Outline ring size as a multiple of the symbol diameter (_2.2.3): larger = more gap.
        public float ring_size_ratio = 1.18f;

        // The icon drawn on the symbol and on the badge (_2.2.1): its colour (every marker) and the
        // symbol icon's diameter as a fraction of the symbol. Defaults = the framework's original look.
        public string icon_color_hex = "#F2ECD3";
        public float icon_size_ratio = 0.56f;

        // Text label gap and font size, both as a multiple of the symbol diameter (_2.2.1), same
        // proportional-sizing convention as ring_size_ratio/badge_size_ratio above: a ratio (not an
        // absolute distance/point size) means every hierarchy level's label sits and reads correctly
        // relative to THAT level's own marker size automatically, with no per-level override needed.
        public float label_gap_ratio = 0.075f;
        public float label_font_size_ratio = 0.25f;

        // Wall-default label font, a key into FontKeyLibrary (_2.0_Labels_And_Fonts_Design.md).
        // "liberation_sans" ships in the framework default library and always resolves.
        public string label_font_key = "liberation_sans";

        // Optional wall-local font library load path (Resources-relative, without extension),
        // same recipe as marker_icon_library_resources_path: when set, WallSession loads this
        // FontKeyLibrary and label_font_key (and any per-level override) is looked up there
        // instead of the framework default library.
        public string label_font_library_resources_path;

        // Dev-only "Add outline demo grid" (see OutlinePreviewSpawner): off by default, only honoured
        // in the Editor and development builds, same rule as effect_defaults.preview.
        public OutlinePreviewSettings outline_preview = new();
        // Dev-only "Add LOD demo field" (Editor Play Mode and development builds only): extra, generated
        // markers in front of the camera so every LOD / crowding / cluster / displacement setting can be
        // seen working. See DemoFieldSpawner. Never saved into pois, never searchable.
        public DemoFieldSettings demo_field = new();
        // Dev-only "Add displacement demo" (Editor Play Mode and development builds only): crowded groups of
        // generated markers on an empty stage, each beside a faded copy at its true place, so every
        // Displacement setting can be seen working. See DisplacementDemoSpawner. Never saved into pois.
        public DisplacementDemoSettings displacement_demo = new();

        // Ring spin speed in degrees per second for hierarchy levels with rotate_contour on.
        public float contour_spin_deg_per_s = 60f;

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

        // Optional wall-level effect parameter defaults. When present, these values
        // are passed to each marker's effect components at spawn time, overriding
        // the components' compiled-in [SerializeField] defaults. When absent/null,
        // effects fall back to their compiled-in defaults -- fully backward compatible
        // with configs authored before this field existed.
        public EffectDefaults effect_defaults;

        // Optional LOD settings. When absent, LODController uses defaults:
        // 3-tier bands (2m/7m/9999m, counts -1/15/5), hybrid density, frustum cull on.
                public LodSettings lod_settings = new();
        public ZoomSettings zoom_settings = new();

        // Optional marker/label displacement settings (spec _2.5 section 10). When
        // absent, MarkerOverlapResolver.ApplyDisplacement uses these compiled-in
        // defaults (label_only + fixed_axis, mirrors LodSettings' own default idiom).
        public DisplacementSettings displacement_settings = new();

        // Optional marker/label/badge/cluster orientation settings (spec _2.1 section 7).
        // Absent -> OrientationSettings' own field defaults: world_up vertical alignment,
        // always_facing_camera facing, every_frame updates.
        public OrientationSettings orientation_settings = new();

        // The Select, Filter & Search domain (_2.6): selection, search, filter, results, minimap
        // and voice settings, one sub-block each (SelectFilterSearchSettings.cs).
        public SelectFilterSearchSettings select_filter_search = new();

        // Keyword vocabulary of the search domain, next to the taxonomy tables whose rows carry
        // their own search_keywords:
        // - search_fields: developer-defined search axes ("architect", "period"); every POI gets
        //   one keyword list per axis (POIData.search_keyword_fields).
        // - synonym_groups: words that mean the same thing; a POI matching one member of a group
        //   also matches every other member (POISearchIndex.Build).
        public List<SearchFieldDefinition> search_fields = new();
        public List<SynonymGroup> synonym_groups = new();

        // Dev-only "Add search & filter demo" (Editor Play Mode and development builds only):
        // generated POIs on their own stage built to exercise every search / filter / selection
        // setting. See SearchDemoSpawner. Never saved into pois.
        public SearchDemoSettings search_demo = new();
    }

    // Dev-only "Add outline demo grid" (Editor Play Mode and development builds; release builds
    // ignore it, same rule as EffectPreviewSettings). See OutlinePreviewSpawner.
    [Serializable]
    public class OutlinePreviewSettings
    {
        public bool enabled = false;
        // POI id whose category the "Same hue" comparison cell borrows. Empty = plain grey circle.
        public string base_poi_id = "";
    }

    // Dev-only LOD demo field (spec _2.4 section 7.2): a box of generated markers on an empty stage (the
    // Editor moves the camera there; a device uses its start pose), with a chosen number of markers per
    // hierarchy level plus one dense clump, so distance bands, crowding, clusters and displacement are
    // all reachable in Play Mode. Placement is deterministic for a given seed (DemoFieldLayout). Off by
    // default; release builds ignore it.
    [Serializable]
    public class DemoFieldSettings
    {
        public bool enabled = false;                // on: only the demo markers run (the wall's own POIs are paused)
        public bool show_labels = true;             // off = no text labels on demo markers, whatever the level says
        public int seed = 1;                        // same seed = same placement; "Reshuffle" picks a new one
        public float distance_m = 1.5f;             // from the stage start (the camera) to the near face of the field
        public float width_m = 6f;                  // left-right spread
        public float height_m = 2.5f;               // up-down spread
        public float depth_m = 8f;                  // how far the field reaches away (spans several distance bands)
        public int dense_clump_count = 8;           // extra markers packed together (crowding / cluster test)
        public float dense_clump_radius_m = 0.08f;
        public List<DemoFieldLevelCount> level_counts = new(); // markers per hierarchy level (missing = 0)

        // Limits shared by the Editor sliders and the layout clamp
        public const int MaxCountPerLevel = 60;
        public const int MaxClumpCount = 30;
    }

    // Dev-only displacement demo (spec _2.5 section 12): four crowded scenarios on a flat demo wall facing
    // the camera -- same level, mixed levels, one big marker among small ones, a lone control marker --
    // each optionally beside a faded, never-displaced reference copy at the true positions. Placement is
    // pure (DisplacementDemoLayout). Off by default; release builds ignore it.
    [Serializable]
    public class DisplacementDemoSettings
    {
        public bool enabled = false;                          // on: only the demo markers run (the wall's own POIs are paused)
        public int markers_per_group = 4;                     // members of each crowded group
        public float spread_cm = 4f;                          // how far apart a group's true positions are (0 = stacked)
        public float distance_m = 3f;                         // camera to the demo wall (farther = smaller = more crowded)
        public bool show_labels = true;                       // every demo marker shows its text label (off: none do)
        public string reference_copies = "side_by_side";      // "side_by_side" | "overlay" | "off"
        public bool run_lod = false;                          // off: LOD leaves the demo alone so only displacement acts

        // Limits shared by the Editor sliders and the layout clamp
        public const int MinMarkersPerGroup = 2;
        public const int MaxMarkersPerGroup = 8;
        public const float MaxSpreadCm = 30f;
        public const float MinDistanceM = 0.3f;
        public const float MaxDistanceM = 6f;
    }

    // Dev-only search & filter demo (spec _2.6 section 17): a flat wall of generated POIs on its own stage
    // facing the camera, one column block per wall category, cycling through every hierarchy level, badge
    // and outline type so every facet has something to filter, plus named test cases (an accented name,
    // a typo twin, a synonym, a custom-field keyword, a dense clump). While it is on the wall searches,
    // filters and selects the demo's POIs instead of its own. Placement is pure (SearchDemoLayout). Off
    // by default; release builds ignore it.
    [Serializable]
    public class SearchDemoSettings
    {
        public bool enabled = false;          // on: only the demo POIs run (the wall's own are paused)
        public int markers_per_category = 3;  // generated POIs per wall category
        public bool test_cases = true;        // add the named test-case POIs and the demo synonym group
        public bool show_labels = true;       // every demo marker shows its text label (off: none do)
        public float distance_m = 3.5f;       // camera to the demo wall
        public float spacing_cm = 40f;        // between neighbouring markers (the clump ignores it)
        public bool run_lod = false;          // off: every demo marker stays visible; on: LOD / clusters act

        // Limits shared by the Editor sliders and the layout clamp
        public const int MaxMarkersPerCategory = 6;
        public const float MinDistanceM = 0.5f;
        public const float MaxDistanceM = 8f;
        public const float MinSpacingCm = 5f;
        public const float MaxSpacingCm = 80f;
    }

    [Serializable]
    public class DemoFieldLevelCount
    {
        public string level_key = "";
        public int count;
    }

    // Wall-level effect settings (_2.2.4): a master switch, one parameter block per
    // effect (each with its own "enabled" checkbox) and the dev-only preview grid settings.
    // Which effect a marker runs is chosen per hierarchy level (ripple_effect / halo_effect /
    // pulse columns), not here: this class only defines how each effect looks.
    [Serializable]
    public class EffectDefaults
    {
        // Wall-wide master switch: false silences every marker effect whatever the hierarchy
        // levels say. Useful for weak devices and as a "reduce motion" accessibility option.
        // Reveal timing is a separate per-level setting.
        public bool effects_enabled = true;

        public PulseDefaults pulse = new();
        public RippleDefaults ripple_rings = new();
        public RippleDefaults ripple_discs = new();
        public HaloRingDefaults halo_ring = new();
        public HaloDiscDefaults halo_disc = new();
        public BeaconDefaults beacon = new();
        public EffectPreviewSettings preview = new();

        // Every selectable effect, in display order (editor foldouts, usage lines, tests).
        public static readonly MarkerEffectFlags[] SelectableEffects =
        {
            MarkerEffectFlags.Pulse, MarkerEffectFlags.RippleRings, MarkerEffectFlags.RippleDiscs,
            MarkerEffectFlags.HaloRing, MarkerEffectFlags.HaloDisc, MarkerEffectFlags.Beacon,
        };

        // This one effect's own "enabled" checkbox (ignores the master switch).
        public bool IsEffectEnabled(MarkerEffectFlags effect)
        {
            switch (effect)
            {
                case MarkerEffectFlags.Pulse: return pulse.enabled;
                case MarkerEffectFlags.RippleRings: return ripple_rings.enabled;
                case MarkerEffectFlags.RippleDiscs: return ripple_discs.enabled;
                case MarkerEffectFlags.HaloRing: return halo_ring.enabled;
                case MarkerEffectFlags.HaloDisc: return halo_disc.enabled;
                case MarkerEffectFlags.Beacon: return beacon.enabled;
                default: return true;
            }
        }

        // Keep only the requested effects that are switched on: the master switch first,
        // then each effect's own "enabled" checkbox. The one place both rules live, so the
        // rendering path and the displacement radius can never disagree.
        public MarkerEffectFlags FilterEnabled(MarkerEffectFlags requested)
        {
            if (!effects_enabled)
                return MarkerEffectFlags.None;

            var allowed = MarkerEffectFlags.None;
            foreach (var effect in SelectableEffects)
                if ((requested & effect) != 0 && IsEffectEnabled(effect))
                    allowed |= effect;
            return allowed;
        }

        // Whole-symbol scale breathing.
        [Serializable]
        public class PulseDefaults
        {
            public bool enabled = true;
            // How much the symbol grows per cycle (0 = none, 0.45 = 45% swing).
            public float amplitude = 0.18f;
            // Seconds per full cycle.
            public float period = 1.6f;
        }

        // Three staggered waves flowing outward; used twice (rings and discs), so a wall can
        // give the filled discs lower alphas than the thin rings.
        [Serializable]
        public class RippleDefaults
        {
            public bool enabled = true;
            public float period = 1.8f;
            // Delay (in cycle units) between the inner, middle and outer wave.
            public float stagger = 0.12f;
            public float inner_alpha = 0.55f;
            public float middle_alpha = 0.36f;
            public float outer_alpha = 0.2f;
            public string tint_color_hex = "#F2CA71";
        }

        // One breathing ring behind the symbol.
        [Serializable]
        public class HaloRingDefaults
        {
            public bool enabled = true;
            // Diameter as a MULTIPLE of the symbol diameter (1 = same size, 1.2 = 20 percent bigger).
            public float size = 1.2f;
            public float base_alpha = 0.28f;
            public string tint_color_hex = "#F2CA71";
            public float period = 2.0f;
            public float breathe_amplitude = 0.15f;
            public float outer_scale = 0.90f;
            public float inner_scale = 0.80f;
        }

        // One breathing filled disc behind the symbol.
        [Serializable]
        public class HaloDiscDefaults
        {
            public bool enabled = true;
            public float size = 1.2f;
            public float base_alpha = 0.28f;
            public string tint_color_hex = "#F2CA71";
            public float period = 2.0f;
            public float breathe_amplitude = 0.15f;
            public float radius_scale = 0.85f;
        }

        // One ring that grows and fades, then restarts (a single outward wave).
        [Serializable]
        public class BeaconDefaults
        {
            public bool enabled = true;
            public float size = 1.2f;
            public float base_alpha = 0.28f;
            public string tint_color_hex = "#F2CA71";
            public float period = 2.0f;
            public float start_scale = 1.0f;
            public float end_scale = 1.8f;
            public float outer_scale = 0.90f;
            public float inner_scale = 0.80f;
        }

        // Dev-only "Add effects demo grid" (Editor Play Mode and development builds; release
        // builds ignore it). See EffectsPreviewSpawner.
        [Serializable]
        public class EffectPreviewSettings
        {
            public bool enabled = false;
            // POI id whose category / status / badge the preview cells copy. Empty = plain grey circle.
            public string base_poi_id = "";
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

        // Developer-facing name for THIS LEVEL (e.g. "Hub", "Landmark") -- shown in the per-POI
        // "which hierarchy level" dropdown. Never a marker's on-wall text: under a marker (real or
        // demo grid) the label is always the POI's own name.
        public string level_name;

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

        // Marker Label Style (the table's "Aa" window, _2.0_Labels_And_Fonts_Design.md section 4).
        // override_label_style = false: this level's label follows the wall default (Labels, Text &
        // Fonts) and the three values below are ignored. true: the level uses its own values. One
        // explicit switch instead of three "<= 0 means inherit" sentinels, so the UI is one checkbox
        // and the runtime rule is one if. Ticking it seeds the values from the current wall default.
        // Exists for cases the wall ratio can't cover -- e.g. keeping one level's text large for
        // accessibility even though its marker is small.
        public bool override_label_style;
        public float label_gap_ratio;
        public float label_font_size_ratio;
        public string label_font_key = "";

        // "none" | "ripple_rings" | "ripple_discs" -- parsed by MarkerHierarchyResolver.
        public string ripple_effect;

        // "none" | "halo_ring" | "halo_disc" | "beacon" -- parsed by MarkerHierarchyResolver.
        public string halo_effect;

        // Independent of both effect slots above -- standalone pulse component.
        public bool pulse;

        // Meaningful only when wall outline mode != none. Controls ring rotation.
        public bool rotate_contour;

        // Seconds after spawn before fade/scale-in begins.
        public float reveal_delay_s;

        // Seconds for the fade/scale-in animation itself. 0 = instant pop-in
        // (jarring -- use only as an explicit artistic choice). Default taper:
        // 0.5s at Level 1 down to 0.25s at Level 5.
        public float reveal_duration_s;

        // Empty string = inherit the wall's facing_mode. Lets hero levels stay
        // always_facing_camera for legibility while background levels sit flat on the
        // wall via wall_fixed (_2.1_Marker_Orientation.md section 4.3).
        public string facing_mode_override = "";
    }

    [Serializable]
    public class POIData : ISerializationCallbackReceiver
    {
        public string id;
        public string name;
        public string category;

        // Edit-scene yaw (degrees) around the marker's up/Y axis, authored via the Scene
        // view Rotate tool or the Specific Marker Facing Options slider. Which facing_mode
        // values actually read this at runtime (_2.1_Marker_Orientation.md section 4):
        // wall_fixed uses all three authored angles unchanged; yaw_only uses X/Z (the wall
        // tilt) but replaces this Y value with a live camera-facing yaw every frame;
        // always_facing_camera ignores all three and always faces the camera. Kept under
        // its original name for backward compatibility with existing config.json files.
        public float editor_rotation_deg;

        // Edit-scene pitch/roll (degrees) around the marker's X and Z axes, authored
        // alongside editor_rotation_deg (the Y/yaw axis). See that field's comment for
        // which facing_mode values read these at runtime.
        public float editor_rotation_x_deg;
        public float editor_rotation_z_deg;

        public PositionData position;
        public bool position_verified;
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

        // Destruction status, 0-100. Same has_* guard as position and for
        // the same reason: a POI legitimately at 0% ("fully intact") must never be
        // indistinguishable from a POI whose wall doesn't track status at all (e.g.
        // a mural has no "destroyed" axis). MarkerView must check has_status before
        // reading status_pct for anything.
        public float status_pct;
        public bool has_status;

        // A genuinely separate third state: this wall DOES track status, and this
        // specific POI's fate is a real historical unknown (not merely undocumented in
        // this dataset). Only meaningful when has_status is true. Rendered identically
        // in every outline mode as a neutral-grey "?" badge, overriding
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
        }

        public void OnAfterDeserialize()
        {
            // Position needs no guard: JsonUtility synthesizes a zero-initialized
            // PositionData for an absent position, and null vs (0,0,0) is functionally
            // equivalent (both resolve to origin at runtime). No extra null-restoring
            // flag is needed -- simpler, and the distinction has no consumer.

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
    public class PositionData
    {
        public float x;
        public float y;
        public float z;

        public Vector3 ToVector3() => new(x, y, z);
    }

    

        [Serializable]
    public class LodSettings
    {
        public bool enabled = true;
        // Distance tiers, sorted ascending by max_distance_m; the last row's large value is a real
        // catch-all. A new wall starts with the framework's three explicit tiers (DefaultBands).
        public List<LodBandEntry> bands = DefaultBands();

        // The framework default tiers (spec 3): under 2 m all markers, under 7 m 15, beyond that 5.
        public static List<LodBandEntry> DefaultBands() => new()
        {
            new LodBandEntry { max_distance_m = 2f, max_visible_count = -1 },
            new LodBandEntry { max_distance_m = 7f, max_visible_count = 15 },
            new LodBandEntry { max_distance_m = 9999f, max_visible_count = 5 },
        };
        public float hysteresis_margin_m = 0.5f;
        public float transition_fade_duration_s = 0.3f;
        public float evaluation_interval_s = 0.2f;

        public string density_response_mode = "hybrid"; // none|select_hide|cluster|shrink_and_fade|hybrid
        public float density_radius_px = 40f;            // matches MarkerOverlapResolver's threshold
        public int shrink_start_neighbor_count = 2;       // hybrid/shrink_and_fade: density response begins here
        public int cluster_min_count = 5;                 // select_hide/cluster/hybrid: escalate to hide-or-cluster here
        public float shrink_min_factor = 0.4f;            // size AND opacity of a marker at the end of the shrink ramp (never vanishes)
        public float cluster_size_ratio = 1.2f;           // cluster diameter as a multiple of its largest member's symbol
        public bool density_safety_escalation_enabled = true; // see §6.2
        public float density_safety_escalation_multiplier = 2f; // see §6.2
        public string cluster_icon_mode = "pie_and_count"; // pie_and_count|dominant_category|count_only
        public string cluster_band_source = "centroid";            // centroid|nearest_member|farthest_member (cluster centroid band)
        public bool cluster_band_hysteresis_enabled = true;        // reuse hysteresis_margin_m for cluster band index stability
        public int cluster_dissolve_grace_cycles = 3;              // pooled cluster view survival cycles before fading out (0 = immediate)
        // displacement_tiebreak removed — moved to DisplacementSettings in _2.5_Displacement.md (§11 of _2.4_Marker_LOD.md).

        public bool frustum_culling_enabled = true;
        public float fov_culling_margin_deg = 10f;
    }

    // AR camera zoom (spec _2.4 section 3.9): its own block, separate from LOD. LOD only READS the
    // resulting zoom factor (effective distance = real distance / zoom).
    [Serializable]
    public class ZoomSettings
    {
        public bool enabled = true;
        public float min_factor = 1f;            // 1 = native camera field of view
        public float max_factor = 4f;            // hard clamp (ARZoomState.SetZoom)
        public float tap_step = 1.5f;            // zoom multiplier per double-tap step
        public int tap_levels = 2;               // steps before the next double-tap returns to 1x
        public float transition_duration_s = 0.25f; // animation length of double-tap and button zoom changes
        public bool show_ui_buttons = true;      // on-screen zoom in / out / reset buttons

        // Double-tap gesture tunables (read by ARZoomGestureInput): seconds between the two taps, and
        // how far the second tap may land from the first and still count as a double-tap.
        public float double_tap_window_s = 0.3f;
        public float double_tap_move_tolerance_px = 50f;
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

    // Marker/label overlap-displacement settings (spec _2.5 section 10). Continuous,
    // re-evaluated every LODController cycle (step 8) -- not a one-shot spawn-time fix.
    [Serializable]
    public class DisplacementSettings
    {
        public bool enabled = true;
        public float overlap_threshold_px = 40f;                  // was a hardcoded const in MarkerOverlapResolver; now configurable
        public string displace_target = "label_only";             // "label_only" | "marker" | "both"
        public string displacement_algorithm = "force_directed";  // "fixed_axis" | "candidate_position" | "force_directed"
        public float max_displacement_px = 120f;                  // cap; beyond it, hide the label
        public bool leader_lines_enabled = true;
        public string leader_line_style = "straight";             // "straight" | "dashed" | "elbow"
        public float leader_line_min_distance_px = 15f;
        public float leader_line_width = 0.01f;                // world-space width of the line (spec Section 6)
        public float leader_line_opacity = 1.0f;               // 0-1 alpha multiplier on the category color
        public string displacement_tiebreak = "symmetric";        // "symmetric" | "lower_priority_only"

        // Slider ranges of the POI Editor's Displacement rows (the one place they are defined)
        public const float MinOverlapPx = 5f;
        public const float MaxOverlapPx = 200f;
        public const float MaxMovePxLimit = 400f;
        public const float MaxLeaderMinLengthPx = 200f;
        public const float MinLeaderWidthM = 0.001f;
        public const float MaxLeaderWidthM = 0.05f;

        // Parameterless constructor (required because the copy constructor below
        // would otherwise suppress the compiler-generated default).
        public DisplacementSettings() { }

        // Copy constructor for test scenarios that need a baseline then override a few fields.
        public DisplacementSettings(DisplacementSettings other)
        {
            if (other == null) return;
            enabled = other.enabled;
            overlap_threshold_px = other.overlap_threshold_px;
            displace_target = other.displace_target;
            displacement_algorithm = other.displacement_algorithm;
            max_displacement_px = other.max_displacement_px;
            leader_lines_enabled = other.leader_lines_enabled;
            leader_line_style = other.leader_line_style;
            leader_line_min_distance_px = other.leader_line_min_distance_px;
            leader_line_width = other.leader_line_width;
            leader_line_opacity = other.leader_line_opacity;
            displacement_tiebreak = other.displacement_tiebreak;
        }
    }

    // Marker / label / badge / cluster orientation settings (_2.1_Marker_Orientation.md
    // v4). Two orthogonal domains: Vertical Alignment (which way "up" is) and Facing
    // Options (what the marker points at), plus Update Cost. Every field is
    // developer-selectable in the POI Editor's Global Scene > Orientation section --
    // nothing here is a hardcoded framework choice, except the small always-on rotation
    // smoothing and always_facing_camera's degenerate-angle pitch guard, which are
    // implementation details, not developer decisions (v3 exposed both as fields and
    // that turned out to be unnecessary complexity for a first version).
    [Serializable]
    public class OrientationSettings
    {
        // --- Vertical Alignment ---
        public string vertical_alignment_mode = "world_up";        // world_up | screen_up (marker root)
        public string label_vertical_alignment_mode = "inherit";   // inherit | world_up | screen_up
        public string badge_vertical_alignment_mode = "inherit";   // inherit | world_up | screen_up
        public string cluster_vertical_alignment_mode = "inherit"; // inherit | world_up | screen_up
        public string up_reference = "world_gravity";              // world_gravity | spawn_root | custom
        public float custom_up_x = 0f, custom_up_y = 1f, custom_up_z = 0f;

        // --- Facing Options ---
        public string facing_mode = "always_facing_camera";
            // wall_fixed | yaw_only | always_facing_camera
        public string facing_basis = "view_plane";                 // view_plane | camera_position (always_facing_camera only)

        // --- Update Cost ---
        public string update_mode = "every_frame";        // every_frame | interval | on_camera_delta
        public float update_interval_s = 0.05f;
        public float camera_delta_deg  = 0.5f;

        // --- Test ---
        public bool edit_mode_preview_enabled = true;

        // Parameterless constructor (required because the copy constructor below
        // would otherwise suppress the compiler-generated default).
        public OrientationSettings() { }

        // Copy constructor, mirrors DisplacementSettings' own pattern -- used by
        // LODController to build an effective per-cluster settings copy without
        // mutating the wall's shared instance.
        public OrientationSettings(OrientationSettings other)
        {
            if (other == null) return;
            vertical_alignment_mode = other.vertical_alignment_mode;
            label_vertical_alignment_mode = other.label_vertical_alignment_mode;
            badge_vertical_alignment_mode = other.badge_vertical_alignment_mode;
            cluster_vertical_alignment_mode = other.cluster_vertical_alignment_mode;
            up_reference = other.up_reference;
            custom_up_x = other.custom_up_x;
            custom_up_y = other.custom_up_y;
            custom_up_z = other.custom_up_z;
            facing_mode = other.facing_mode;
            facing_basis = other.facing_basis;
            update_mode = other.update_mode;
            update_interval_s = other.update_interval_s;
            camera_delta_deg = other.camera_delta_deg;
            edit_mode_preview_enabled = other.edit_mode_preview_enabled;
        }
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

        // When true, the visitor's Filters panel gets a group for this field: one chip per keyword the POIs
        // hold in it (for example a "Period" field -> chips "Baroque", "Gothic"), filtered like a category.
        public bool filterable;

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
