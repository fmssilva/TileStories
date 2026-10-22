using System.Collections.Generic;

namespace TileStories
{
    // The one line that places a POI in the wall's taxonomy: "category name - level name" (either part left out
    // when the POI has none). Read by the results list rows and the card header's chip, so both always agree. The
    // card header shows the category alone unless its Show Level is on (_3.1 step 6C: a hierarchy level is an authoring
    // concept, e.g. "Hub", not something every visitor should read).
    public static class PoiSubtitle
    {
        public static string Of(POIData poi, WallConfigData taxonomy) => Of(poi, taxonomy, withLevel: true);

        // `language`: the card's visitor language -- the category's own name in it when the wall wrote one; null = its one name
        // (the search side has no visitor language yet)
        public static string Of(POIData poi, WallConfigData taxonomy, bool withLevel, string language = null)
        {
            if (poi == null) return "";
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(poi.category))
                parts.Add(language == null ? TaxonomyNames.Category(taxonomy, poi.category) : TaxonomyNames.Category(taxonomy, poi.category, language));
            var level = withLevel ? taxonomy?.hierarchy_levels?.Find(l => l != null && l.key == poi.hierarchy_level_key) : null;
            if (level != null && !string.IsNullOrWhiteSpace(level.level_name)) parts.Add(level.level_name);
            return string.Join(" - ", parts);
        }
    }
}
