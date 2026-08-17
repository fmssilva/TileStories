// LodAutoSuggest.cs
//
// Editor-only heuristic that proposes a starting-point LodSettings from a
// wall's total POI count. Exposed via the authoring tool's "Suggest Values"
// button (section 6.2 of _2.4_Marker_LOD.md, Implementation Status row 5b).
//
// One-shot convenience, not a persistent auto-mode: it writes ordinary manual
// field values; the developer hand-tunes afterward exactly as if they had
// typed the numbers. Pure static -- no MonoBehaviour, no runtime dependency.

using UnityEngine;

namespace TileStories.Editor
{
    internal static class LodAutoSuggest
    {
        // Section 6.2, reproduced verbatim so the numbers stay explainable here
        // instead of reading as magic. The Min/Max calls give the floors for
        // free (cluster_min_count >= 3, shrink_start_neighbor_count >= 2), so a
        // small or zero POI count needs no special-case guard.
        public static LodSettings Suggest(int totalPoiCount)
        {
            int outer = Mathf.Min(5, totalPoiCount);
            int middle = Mathf.Min(15, totalPoiCount);
            int clusterMin = Mathf.Max(3, Mathf.RoundToInt(totalPoiCount / 10f));
            int shrinkStart = Mathf.Max(2, clusterMin / 2);

            return new LodSettings
            {
                bands = new()
                {
                    new LodBandEntry { max_distance_m = 2f, max_visible_count = -1 },
                    new LodBandEntry { max_distance_m = 7f, max_visible_count = middle },
                    new LodBandEntry { max_distance_m = 9999f, max_visible_count = outer },
                }, // all three tiers explicit -- no implicit "beyond the last row" case
                cluster_min_count = clusterMin,
                shrink_start_neighbor_count = shrinkStart,
                // all other fields left at their LodSettings defaults
            };
        }
    }
}
