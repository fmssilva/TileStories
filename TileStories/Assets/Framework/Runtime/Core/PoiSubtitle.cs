using System.Collections.Generic;

namespace TileStories
{
    // The one line that places a POI in the wall's taxonomy: "category name - level name" (either part left out
    // when the POI has none). Read by the results list rows and the card header's chip, so both always agree.
    public static class PoiSubtitle
    {
        public static string Of(POIData poi, WallConfigData taxonomy)
        {
            if (poi == null) return "";
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(poi.category)) parts.Add(TaxonomyNames.Category(taxonomy, poi.category));
            var level = taxonomy?.hierarchy_levels?.Find(l => l != null && l.key == poi.hierarchy_level_key);
            if (level != null && !string.IsNullOrWhiteSpace(level.level_name)) parts.Add(level.level_name);
            return string.Join(" - ", parts);
        }
    }
}
