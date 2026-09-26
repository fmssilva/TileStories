using System.Text;
using UnityEngine;

namespace TileStories.Editor
{
    // Live Play Mode updates for the Select, Filter & Search domain (_2.6): its settings block, the keyword
    // vocabulary (custom search fields, synonym groups) and each POI's own searchable text (summary, keywords,
    // custom-field keywords). WallSession.ApplySearchSettings rebuilds the index and the search UI. A POI's
    // name, its taxonomy rows and their Search Keywords belong to the Marker applier, whose seam re-indexes too.
    public class LivePlayModeSearchApplier : ILivePlayModeApplier
    {
        public string Name => "select filter search";

        public string Fingerprint(WallConfigData config)
        {
            var sb = new StringBuilder();
            sb.Append(JsonUtility.ToJson(config.select_filter_search ?? new SelectFilterSearchSettings(), false));
            if (config.search_fields != null)
                foreach (var f in config.search_fields)
                    sb.Append('|').Append(JsonUtility.ToJson(f, false));
            if (config.synonym_groups != null)
                foreach (var g in config.synonym_groups)
                    sb.Append('|').Append(JsonUtility.ToJson(g, false));
            if (config.pois != null)
                foreach (var p in config.pois)
                {
                    if (p == null) continue;
                    sb.Append('|').Append(p.id).Append('=').Append(p.summary).Append(';')
                      .Append(string.Join(",", p.search_keywords ?? new System.Collections.Generic.List<string>()));
                    if (p.search_keyword_fields != null)
                        foreach (var kf in p.search_keyword_fields)
                            sb.Append(';').Append(JsonUtility.ToJson(kf, false));
                }
            return sb.ToString();
        }

        // Hand the running wall its own copy so it can never mutate the authoring config
        public void Apply(WallSession session, WallConfigData configCopy) => session.ApplySearchSettings(configCopy);
    }
}
