using System.Collections.Generic;
using UnityEngine;

namespace TileStories
{
    // Which POIs a related block lists (_3.1 Tier 2 group B), pure so every case is a unit test:
    //   manual        -- the rows' points, in the order written: a point no longer on the wall, this point itself and a
    //                    second row naming the same point are left out
    //   same_category -- the wall's other points of this point's category, nearest first
    //   nearest       -- the wall's other points, nearest first
    // "Nearest" is the straight-line distance between the points' positions (POIPositionResolver); equal distances go by id,
    // so the list never changes order between two runs. The two automatic sources list at most MaxAutomatic points.
    public static class RelatedPoisRule
    {
        public const string SourceManual = "manual";
        public const string SourceSameCategory = "same_category";
        public const string SourceNearest = "nearest";

        // How many points an automatic source lists
        public const int MaxAutomatic = 6;

        // The points this block lists for `poi` among `wallPois`; `source` "" or unknown = manual
        public static List<POIData> Pick(POIData poi, string source, IReadOnlyList<string> manualIds, IReadOnlyList<POIData> wallPois)
        {
            var picked = new List<POIData>();
            if (poi == null || wallPois == null) return picked;
            if (source == SourceSameCategory || source == SourceNearest)
            {
                var others = new List<POIData>();
                foreach (var other in wallPois)
                    if (other != null && other.id != poi.id && (source == SourceNearest || other.category == poi.category)) others.Add(other);
                POIPositionResolver.TryResolvePosition(poi, out var here, logErrors: false);
                others.Sort((a, b) =>
                {
                    int byDistance = DistanceFrom(here, a).CompareTo(DistanceFrom(here, b));
                    return byDistance != 0 ? byDistance : string.CompareOrdinal(a.id, b.id);
                });
                for (int i = 0; i < others.Count && i < MaxAutomatic; i++) picked.Add(others[i]);
                return picked;
            }
            if (manualIds == null) return picked;
            foreach (string id in manualIds)
            {
                if (string.IsNullOrEmpty(id) || id == poi.id || picked.Exists(p => p.id == id)) continue;
                foreach (var other in wallPois)
                    if (other != null && other.id == id) { picked.Add(other); break; }
            }
            return picked;
        }

        // The ids a block's manual rows name, in order (blank rows included: Pick leaves them out)
        public static List<string> ManualIds(BlockInstanceData block)
        {
            var ids = new List<string>();
            var read = new BlockFieldReader(block, null, null);
            foreach (var item in read.Items(BuiltInBlocks.RelatedItemsField)) ids.Add(read.ItemValue(item, BuiltInBlocks.RelatedPoiField));
            return ids;
        }

        // The points a related block lists for `poi` (its source and rows read from the block)
        public static List<POIData> Of(POIData poi, BlockInstanceData block, IReadOnlyList<POIData> wallPois) =>
            Pick(poi, new BlockFieldReader(block, null, null).Value(BuiltInBlocks.RelatedSourceField), ManualIds(block), wallPois);

        // next_along_wall: among `candidates`, the nearest one to the RIGHT of `poi` along the wall (+1); at the wall's right
        // end, the nearest one back to the left (-1); none on the wall: (null, 0). Places come from the whole wall's axis
        // (WallAxisRule, wall_locator's rule), so "along the wall" means the same thing in both blocks.
        public static (POIData Poi, int Direction) NextAlongWall(POIData poi, IReadOnlyList<POIData> candidates, IReadOnlyList<POIData> wallPois)
        {
            var places = WallAxisRule.Places(wallPois);
            int self = places.IndexOf(poi?.id);
            if (self < 0 || candidates == null) return (null, 0);
            var along = new List<float> { places.Along[self] };
            var ids = new List<string> { poi.id };
            var pois = new List<POIData> { poi };
            foreach (var candidate in candidates)
            {
                int at = places.IndexOf(candidate?.id);
                if (at < 0) continue;
                along.Add(places.Along[at]);
                ids.Add(candidate.id);
                pois.Add(candidate);
            }
            var (left, right) = WallAxisRule.Neighbours(along, ids, 0);
            if (right > 0) return (pois[right], 1);
            if (left > 0) return (pois[left], -1);
            return (null, 0);
        }

        private static float DistanceFrom(Vector3 here, POIData other) =>
            POIPositionResolver.TryResolvePosition(other, out var there, logErrors: false) ? Vector3.Distance(here, there) : float.MaxValue;
    }
}
