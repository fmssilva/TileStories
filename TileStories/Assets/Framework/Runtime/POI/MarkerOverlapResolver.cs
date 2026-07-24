using System.Collections.Generic;
using UnityEngine;

namespace TileStories
{
    // Groups spawned markers that overlap on screen (within a pixel
    // threshold) and assigns each group's members a stable vertical offset
    // index, so overlapping markers spread apart instead of stacking on top
    // of each other. Grouping is computed once, from each marker's initial
    // (pre-offset) screen position - see _1_2_POI_Colision_Solver.md Section 2
    // for why this replaced an earlier pairwise-incremental approach.
    //
    // Audit findings (Section 1 of _1_2_POI_Colision_Solver.md):
    // - Original ApplyOverlapOffset was ADDITIVE (not idempotent), causing
    //   over-offsetting when called multiple times.
    // - Original ApplyNearOverlapOffsets had stale comparison positions and
    //   multiple additive calls, making results order-dependent.
    // - This implementation fixes both by using union-find clustering on a
    //   fixed snapshot of screen positions, and calling ApplyOverlapOffset
    //   exactly once per marker.
    public static class MarkerOverlapResolver
    {
        private const float OverlapThresholdPixels = 40f;

        public static void ApplyOverlapOffsets(
            IReadOnlyList<MarkerView> spawnedMarkers,
            Camera cam)
        {
            if (cam == null || spawnedMarkers == null || spawnedMarkers.Count < 2)
                return;

            int count = spawnedMarkers.Count;

            // Step 1: capture every marker's screen position ONCE, before any
            // offsets are applied. Every grouping decision below is made from
            // this fixed snapshot, never from a marker's live (possibly
            // already-offset) position.
            var screenPositions = new Vector2[count];
            for (int i = 0; i < count; i++)
            {
                Vector3 sp = cam.WorldToScreenPoint(spawnedMarkers[i].transform.position);
                screenPositions[i] = new Vector2(sp.x, sp.y);
            }

            // Step 2: union-find over the fixed snapshot to group markers
            // that are mutually within the overlap threshold, directly or
            // transitively (A overlaps B, B overlaps C => A, B, C are one
            // group, even if A and C aren't within threshold of each other).
            int[] parent = new int[count];
            for (int i = 0; i < count; i++) parent[i] = i;

            int Find(int x)
            {
                while (parent[x] != x) { parent[x] = parent[parent[x]]; x = parent[x]; }
                return x;
            }

            void Union(int a, int b)
            {
                int ra = Find(a), rb = Find(b);
                if (ra != rb) parent[ra] = rb;
            }

            for (int i = 0; i < count; i++)
            {
                for (int j = i + 1; j < count; j++)
                {
                    if (Vector2.Distance(screenPositions[i], screenPositions[j]) < OverlapThresholdPixels)
                        Union(i, j);
                }
            }

            // Step 3: assign a stable, deterministic offset index within each
            // group. Ordering by POI id (not spawn/array order) keeps the
            // result reproducible regardless of config.json entry order.
            var groups = new Dictionary<int, List<int>>();
            for (int i = 0; i < count; i++)
            {
                int root = Find(i);
                if (!groups.TryGetValue(root, out var members))
                    groups[root] = members = new List<int>();
                members.Add(i);
            }

            foreach (var members in groups.Values)
            {
                if (members.Count < 2)
                    continue; // no overlap in this group, nothing to offset

                members.Sort((a, b) => string.CompareOrdinal(
                    spawnedMarkers[a].PoiId, spawnedMarkers[b].PoiId));

                for (int k = 0; k < members.Count; k++)
                {
                    // ApplyOverlapOffset must be idempotent (sets an absolute
                    // offset from a stored base position) - see Section 1's
                    // audit. It is called exactly once per marker here, so
                    // idempotence isn't strictly required for THIS call site
                    // any more, but keep it idempotent regardless: other
                    // future call sites should not have to know this
                    // constraint to be safe.
                    spawnedMarkers[members[k]].ApplyOverlapOffset(k);
                }

                // Debug log: show which POIs were grouped together
                string groupIds = string.Join(", ", members.ConvertAll(idx => spawnedMarkers[idx].PoiId));
                Debug.Log($"[MarkerOverlapResolver] Grouped {members.Count} overlapping markers: {groupIds}");
            }
        }
    }
}