using System.Collections.Generic;

namespace TileStories
{
    // What the last LOD evaluation decided, counted for a developer to read (the POI Editor's live LOD
    // readout): how many markers show, how many each rule hid, the clusters, and each Distance Band's
    // share. Pure: built from the pipeline's final visual units, so it cannot disagree with what the
    // markers do.
    public class LodStats
    {
        // One Distance Band: markers/clusters inside it and how many of them show
        public readonly struct Band
        {
            public readonly float MaxDistanceM;
            public readonly int MaxVisibleCount;  // -1 = all
            public readonly int InBand;           // visual units (a cluster counts as one) in view in this band
            public readonly int Shown;

            public Band(float maxDistanceM, int maxVisibleCount, int inBand, int shown)
            {
                MaxDistanceM = maxDistanceM;
                MaxVisibleCount = maxVisibleCount;
                InBand = inBand;
                Shown = shown;
            }
        }

        public int Markers;           // every marker LOD evaluated
        public int OutOfView;         // skipped by Frustum Culling
        public int Shown;             // markers drawn on their own
        public int Shrunk;            // of those, drawn smaller and fainter by Shrink & Fade / Hybrid
        public int HiddenByCrowding;  // hidden by Select & Hide (a crowded marker not merged into a cluster)
        public int HiddenByMaxMarkers; // hidden by a band's Max markers
        public int Clusters;          // cluster markers drawn
        public int InClusters;        // markers merged into those clusters (a cluster cut by Max markers counts there)
        public float ZoomFactor = 1f;
        public List<Band> Bands = new();

        // Count the final units of one evaluation. `bands` are the Distance Bands in use.
        public static LodStats From(IReadOnlyList<VisualUnit> units, IReadOnlyList<LodBandEntry> bands, float zoomFactor)
        {
            var stats = new LodStats { ZoomFactor = zoomFactor };
            int bandCount = bands?.Count ?? 0;
            var inBand = new int[bandCount];
            var shownInBand = new int[bandCount];

            foreach (var u in units)
            {
                if (u == null) continue;
                bool inRange = u.band.Index >= 0 && u.band.Index < bandCount;

                if (u.clusterMembers != null)
                {
                    // - its members left the unit list when they merged: count them here
                    int members = u.clusterMembers.Count;
                    stats.Markers += members;
                    if (inRange) inBand[u.band.Index]++;
                    if (!u.isVisible) { stats.HiddenByMaxMarkers += members; continue; }
                    stats.Clusters++;
                    stats.InClusters += members;
                    if (inRange) shownInBand[u.band.Index]++;
                    continue;
                }

                stats.Markers++;
                if (!u.inView) { stats.OutOfView++; continue; }
                if (inRange) inBand[u.band.Index]++;
                if (u.isVisible)
                {
                    stats.Shown++;
                    if (u.shrinkScale < 0.999f) stats.Shrunk++;
                    if (inRange) shownInBand[u.band.Index]++;
                }
                else if (u.densityState == DensityState.Clustered) stats.HiddenByCrowding++;
                else stats.HiddenByMaxMarkers++;
            }

            for (int i = 0; i < bandCount; i++)
                stats.Bands.Add(new Band(bands[i].max_distance_m, bands[i].max_visible_count, inBand[i], shownInBand[i]));
            return stats;
        }
    }
}
