using System.Text;

namespace TileStories.Editor
{
    // Live Play Mode updates for a POI's own Hierarchy Level assignment (Specific Marker > Marker
    // Style > Hierarchy Level, _2.3_Marker_Hierarchy.md section 8): which level each POI uses. Its
    // own applier because that one field drives several domains at once (size/label via the marker
    // applier's columns, effects, facing) -- folding it into any one of them would make that domain
    // re-apply the others. The levels' OWN columns stay with their domain appliers.
    public class LivePlayModePoiLevelApplier : ILivePlayModeApplier
    {
        public string Name => "poi-level";

        // Only each POI's id=hierarchy_level_key pair
        public string Fingerprint(WallConfigData config)
        {
            var sb = new StringBuilder();
            if (config.pois != null)
                foreach (var poi in config.pois)
                    if (poi != null) sb.Append(poi.id).Append('=').Append(poi.hierarchy_level_key).Append('|');
            return sb.ToString();
        }

        // Hand the running wall its own copy so it can never mutate the authoring config
        public void Apply(WallSession session, WallConfigData configCopy)
        {
            session.ApplyPoiHierarchyLevels(configCopy.pois);
        }
    }
}
