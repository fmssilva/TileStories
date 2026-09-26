using System.Collections.Generic;

namespace TileStories.Editor
{
    // How the POI Editor names a POI or a hierarchy level to the developer: exactly the text the window shows for
    // it -- a POI's list title ("3. North tower"), a row's name -- never a POI id or a generated key, which the
    // Editor Tab never shows. Every validation finding, notice, tooltip and dropdown names things this way (the
    // other taxonomy rows go through TaxonomyNames.NameOr). Pure.
    public static class EditorNames
    {
        // A POI's title in the Specific Marker list: "<position>. <name>"
        public static string Poi(int index, POIData poi) =>
            $"{index + 1}. {(string.IsNullOrWhiteSpace(poi?.name) ? "(unnamed)" : poi.name)}";

        // Same, when only the POI is at hand: its position is looked up in the wall's list
        public static string Poi(IList<POIData> pois, POIData poi)
        {
            int index = pois?.IndexOf(poi) ?? -1;
            if (index >= 0) return Poi(index, poi);
            return string.IsNullOrWhiteSpace(poi?.name) ? "(unnamed)" : poi.name;
        }

        // A hierarchy level as its table shows it
        public static string Level(HierarchyLevelEntry level) => TaxonomyNames.NameOr(level?.level_name, level?.key);
    }
}
