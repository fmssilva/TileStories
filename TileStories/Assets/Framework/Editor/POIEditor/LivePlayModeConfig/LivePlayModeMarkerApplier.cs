using System.Text;
using UnityEngine;

namespace TileStories.Editor
{
    // Live Play Mode updates for the Marker/Badge/Outline domain (_2.2.1/2/3): the wall-level
    // shape/outline/badge settings, the category/badge/outline taxonomy tables, and each POI's own
    // category/badge/status/custom-symbol fields. Effects and per-POI position/facing are separate
    // domains (LivePlayModeEffectsApplier, LivePlayModeOrientationApplier) and are not touched here.
    public class LivePlayModeMarkerApplier : ILivePlayModeApplier
    {
        public string Name => "marker";

        // Every wall-level marker field plus, per POI, the exact fields WallSession.ApplyMarkerSettings
        // re-applies -- nothing about position, facing or hierarchy_level_key (those belong to the
        // Orientation domain), so an unrelated facing edit never re-triggers a marker re-apply.
        public string Fingerprint(WallConfigData config)
        {
            var sb = new StringBuilder();
            sb.Append(config.marker_shape).Append('|').Append(config.badge_shape).Append('|')
              .Append(config.marker_outline_mode).Append('|').Append(config.outline_uniform_color_hex).Append('|')
              .Append(config.marker_use_badge).Append('|')
              .Append(config.badge_corner).Append('|').Append(config.badge_size_ratio).Append('|')
              .Append(config.ring_size_ratio).Append('|').Append(config.contour_spin_deg_per_s).Append('|')
              .Append(config.marker_icon_library_resources_path).Append('|')
              .Append(config.outline_preview?.enabled).Append('|').Append(config.outline_preview?.base_poi_id);
            sb.Append('|').Append(JsonUtility.ToJson(new Wrapper { list = config.category_styles }, false));
            sb.Append('|').Append(JsonUtility.ToJson(new BadgeWrapper { list = config.badge_categories }, false));
            sb.Append('|').Append(JsonUtility.ToJson(new OutlineWrapper { list = config.outline_levels }, false));
            foreach (var poi in config.pois)
                sb.Append('|').Append(poi.id).Append('=').Append(poi.category).Append(',').Append(poi.badge_category)
                  .Append(',').Append(poi.has_status).Append(',').Append(poi.status_pct).Append(',').Append(poi.status_unknown)
                  .Append(',').Append(poi.status_level_key).Append(',').Append(poi.has_custom_symbol).Append(',').Append(poi.custom_symbol_key);
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
