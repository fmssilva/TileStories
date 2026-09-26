using System.Text;
using UnityEngine;

namespace TileStories.Editor
{
    // Live Play Mode updates for the Marker/Badge/Outline domain (_2.2.1/2/3), plus the Hierarchy
    // Levels domain's (_2.3) own non-effect, non-facing columns: the wall-level shape/outline/badge
    // settings, the category/badge/outline taxonomy tables, each POI's own category/badge/status/
    // custom-symbol fields, and each hierarchy level's label/priority/details/size/show-label/
    // rotate-contour/reveal timing/search keywords/Marker Label Style (override switch + gap/font-size/
    // font key, _2.0_Labels_And_Fonts_Design.md). A level's ripple/halo/pulse columns belong to
    // LivePlayModeEffectsApplier and its facing_mode_override to LivePlayModeOrientationApplier --
    // excluded here so an unrelated effect or facing edit never re-triggers a marker re-apply (and
    // vice versa: editing Size here never re-triggers an effects rebuild). Apply() rebuilds BOTH the
    // outline demo grid and the effects/hierarchy demo grid (WallSession.ApplyMarkerSettings) --
    // the effects grid's per-level cells are built from a HierarchyStyle this applier owns
    // (size_cm/show_label/rotate_contour/reveal), so it would otherwise go stale on exactly the
    // fields "Add Hierarchy demo grid" exists to preview live. A POI's OWN hierarchy_level_key
    // (which level it uses) belongs to LivePlayModePoiLevelApplier, not here (_2.3_Marker_Hierarchy.md
    // section 8).
    public class LivePlayModeMarkerApplier : ILivePlayModeApplier
    {
        public string Name => "marker";

        // Every wall-level marker field (including label_gap_ratio/label_font_size_ratio/
        // label_font_key/label_font_library_resources_path, _2.0_Labels_And_Fonts_Design.md) plus, per
        // POI, the exact fields WallSession.ApplyMarkerSettings re-applies (name = the label text)
        // -- nothing about position, facing (LivePlayModeOrientationApplier) or hierarchy_level_key
        // (LivePlayModePoiLevelApplier), so an unrelated facing edit never re-triggers a marker re-apply.
        public string Fingerprint(WallConfigData config)
        {
            var sb = new StringBuilder();
            sb.Append(config.marker_shape).Append('|').Append(config.badge_shape).Append('|')
              .Append(config.marker_outline_mode).Append('|').Append(config.outline_uniform_color_hex).Append('|')
              .Append(config.marker_use_badge).Append('|')
              .Append(config.badge_corner).Append('|').Append(config.badge_size_ratio).Append('|')
              .Append(config.ring_size_ratio).Append('|').Append(config.contour_spin_deg_per_s).Append('|')
              .Append(config.icon_color_hex).Append('|').Append(config.icon_size_ratio).Append('|')
              .Append(config.label_gap_ratio).Append('|').Append(config.label_font_size_ratio).Append('|')
              .Append(config.label_font_key).Append('|').Append(config.label_font_library_resources_path).Append('|')
              .Append(config.marker_icon_library_resources_path).Append('|')
              .Append(config.outline_preview?.enabled).Append('|').Append(config.outline_preview?.base_poi_id);
            sb.Append('|').Append(JsonUtility.ToJson(new Wrapper { list = config.category_styles }, false));
            sb.Append('|').Append(JsonUtility.ToJson(new BadgeWrapper { list = config.badge_categories }, false));
            sb.Append('|').Append(JsonUtility.ToJson(new OutlineWrapper { list = config.outline_levels }, false));
            // poi.name is the marker's label text
            foreach (var poi in config.pois)
                sb.Append('|').Append(poi.id).Append('=').Append(poi.name).Append(',').Append(poi.category).Append(',').Append(poi.badge_category)
                  .Append(',').Append(poi.has_status).Append(',').Append(poi.status_pct).Append(',').Append(poi.status_unknown)
                  .Append(',').Append(poi.status_level_key).Append(',').Append(poi.has_custom_symbol).Append(',').Append(poi.custom_symbol_key);
            foreach (var level in config.hierarchy_levels ?? new System.Collections.Generic.List<HierarchyLevelEntry>())
            {
                if (level == null) continue;
                sb.Append('|').Append(level.key).Append('=').Append(level.level_name).Append(',').Append(level.priority)
                  .Append(',').Append(level.details).Append(',').Append(level.size_cm).Append(',').Append(level.show_label)
                  .Append(',').Append(level.rotate_contour).Append(',').Append(level.reveal_delay_s).Append(',').Append(level.reveal_duration_s)
                  .Append(',').Append(level.override_label_style).Append(',').Append(level.label_gap_ratio)
                  .Append(',').Append(level.label_font_size_ratio).Append(',').Append(level.label_font_key)
                  .Append(',').Append(string.Join(";", level.search_keywords ?? new System.Collections.Generic.List<string>()));
            }
            return sb.ToString();
        }

        // Hand the running wall its own copy so it can never mutate the authoring config
        public void Apply(WallSession session, WallConfigData configCopy)
        {
            session.ApplyMarkerSettings(configCopy);
        }

        [System.Serializable] private class Wrapper { public System.Collections.Generic.List<CategoryStyleEntry> list; }
        [System.Serializable] private class BadgeWrapper { public System.Collections.Generic.List<BadgeCategoryEntry> list; }
        [System.Serializable] private class OutlineWrapper { public System.Collections.Generic.List<OutlineLevelEntry> list; }
    }
}
