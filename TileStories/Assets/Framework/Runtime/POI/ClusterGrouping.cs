using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace TileStories
{
    // Pure-static cluster geometry (spec §6.1). No MonoBehaviour, no Camera, no
    // scene: screen positions and world positions are passed in by the caller
    // (LODController.ReconcileClusters), so this entire class is Tier-0 testable.
    //
    // A "group" is a connected component of markers whose 2D screen positions are
    // within density_radius_px of each other (same criterion EvaluateDensity uses,
    // spec §5). Output is deterministic -- group order by lowest poiId, members
    // sorted by poiId -- so the same input set never reshuffles between cycles.
    public static class ClusterGrouping
    {
        // Connected components via union-find over screen-space proximity.
        public static List<List<VisualUnit>> Group(List<VisualUnit> aggregatable, IReadOnlyDictionary<string, Vector2> screenPos, float radiusPx)
        {
            var groups = new List<List<VisualUnit>>();
            if (aggregatable == null || aggregatable.Count == 0) return groups;

            int n = aggregatable.Count;
            var parent = new int[n];
            var rank = new int[n];
            for (int i = 0; i < n; i++) parent[i] = i;

            var valid = new List<int>();
            for (int i = 0; i < n; i++)
                if (screenPos != null && screenPos.ContainsKey(aggregatable[i].poiId))
                    valid.Add(i);

            float r2 = radiusPx * radiusPx;
            for (int a = 0; a < valid.Count; a++)
            {
                int ia = valid[a];
                Vector2 pa = screenPos[aggregatable[ia].poiId];
                for (int b = a + 1; b < valid.Count; b++)
                {
                    int ib = valid[b];
                    if ((pa - screenPos[aggregatable[ib].poiId]).sqrMagnitude <= r2)
                        Union(parent, rank, ia, ib);
                }
            }

            var buckets = new Dictionary<int, List<VisualUnit>>();
            foreach (int rootIdx in valid)
            {
                int root = Find(parent, rootIdx);
                if (!buckets.TryGetValue(root, out var bucket))
                {
                    bucket = new List<VisualUnit>();
                    buckets[root] = bucket;
                }
                bucket.Add(aggregatable[rootIdx]);
            }

            foreach (var bucket in buckets.Values)
            {
                bucket.Sort((x, y) => string.CompareOrdinal(x.poiId, y.poiId));
                groups.Add(bucket);
            }
            groups.Sort((a, b) => string.CompareOrdinal(a[0].poiId, b[0].poiId));
            return groups;
        }

        // Mean world position of group members (spec §6.1 centroid).
        public static Vector3 Centroid(List<VisualUnit> members)
        {
            if (members == null || members.Count == 0) return Vector3.zero;
            Vector3 sum = Vector3.zero;
            foreach (var m in members) if (m != null) sum += m.worldPosition;
            return sum / members.Count;
        }

        // Centroid effective distance = real centroid distance / zoom factor (§10).
        public static float CentroidEffectiveDistance(Vector3 centroid, Vector3 camPos)
        {
            float zoom = ARZoomState.ZoomFactor;
            if (zoom <= 0f) zoom = 1f;
            return Vector3.Distance(camPos, centroid) / zoom;
        }

        // True if the two id sets share any member (group-vs-pooled-view reuse test).
        public static bool Overlaps(IReadOnlyCollection<string> poolIds, IReadOnlyCollection<string> groupIds)
        {
            if (poolIds == null || groupIds == null || poolIds.Count == 0 || groupIds.Count == 0) return false;
            var smaller = poolIds.Count <= groupIds.Count ? poolIds : groupIds;
            var larger = System.Object.ReferenceEquals(smaller, poolIds) ? groupIds : poolIds;
            var lset = new HashSet<string>(larger);
            foreach (var id in smaller)
                if (lset.Contains(id)) return true;
            return false;
        }

        // Stable cluster key: sorted, pipe-joined member ids. Drives band-hysteresis
        // cache + dissolve-grace keying so a stable group keeps a stable signature.
        public static string Signature(IReadOnlyCollection<string> poiIds)
        {
            if (poiIds == null || poiIds.Count == 0) return string.Empty;
            var sorted = new List<string>(poiIds);
            sorted.Sort(StringComparer.Ordinal);
            var sb = new StringBuilder();
            for (int i = 0; i < sorted.Count; i++)
            {
                if (i > 0) sb.Append('|');
                sb.Append(sorted[i]);
            }
            return sb.ToString();
        }

        // Extract member poiIds (helper for Signature/Overlaps/BuildAggregate).
        public static List<string> MemberIds(List<VisualUnit> group)
        {
            var ids = new List<string>(group != null ? group.Count : 0);
            if (group == null) return ids;
            foreach (var u in group) if (u != null) ids.Add(u.poiId);
            return ids;
        }

        // Pick WHICH representative distance feeds the band lookup, per the wall's
        // band_source. "centroid" uses the centroid effective distance passed by the
        // caller; nearest/farthest use the min/max of each member's zoom-adjusted
        // effectiveDistance. Returns a full LodBand (index + thresholds).
        public static LodBand ResolveBand(List<VisualUnit> group, string bandSource, float centroidEffectiveDistance, List<LodBandEntry> entries)
        {
            if (entries == null || entries.Count == 0) entries = LODController.DefaultBands();
            string mode = string.IsNullOrEmpty(bandSource) ? "centroid" : bandSource;
            if (mode != "centroid" && mode != "nearest_member" && mode != "farthest_member")
            {
                Debug.LogWarning($"[LOD] cluster band_source '{bandSource}' unrecognised; falling back to 'centroid'");
                mode = "centroid";
            }

            float dist;
            switch (mode)
            {
                case "nearest_member":
                    dist = float.PositiveInfinity;
                    foreach (var m in group) if (m != null && m.effectiveDistance < dist) dist = m.effectiveDistance;
                    break;
                case "farthest_member":
                    dist = float.NegativeInfinity;
                    foreach (var m in group) if (m != null && m.effectiveDistance > dist) dist = m.effectiveDistance;
                    break;
                default:
                    dist = centroidEffectiveDistance;
                    break;
            }
            return LODController.FindBand(dist, entries);
        }

        // Build the aggregate VisualUnit that replaces a group of absorbed members.
        // Sets worldPosition/poiId/priority/members/densityState; leaves clusterView
        // and band for the caller (the only two fields the caller owns, per §6.1).
        public static VisualUnit BuildAggregate(List<VisualUnit> group, int bestMemberPriority, string bandSource, Vector3 centroid, float centroidEffectiveDistance)
        {
            return new VisualUnit
            {
                poiId = Signature(MemberIds(group)),
                worldPosition = centroid,
                effectiveDistance = centroidEffectiveDistance,
                hierarchyLevelIndex = bestMemberPriority,
                densityState = DensityState.Clustered,
                clusterMembers = group != null ? new List<VisualUnit>(group) : null,
                clusterView = null,
                band = default,
            };
        }

        // --- union-find internals ---
        private static int Find(int[] parent, int x)
        {
            while (parent[x] != x)
            {
                parent[x] = parent[parent[x]];
                x = parent[x];
            }
            return x;
        }

        private static void Union(int[] parent, int[] rank, int a, int b)
        {
            int ra = Find(parent, a), rb = Find(parent, b);
            if (ra == rb) return;
            if (rank[ra] < rank[rb]) { parent[ra] = rb; }
            else if (rank[ra] > rank[rb]) { parent[rb] = ra; }
            else { parent[rb] = ra; rank[ra]++; }
        }
    }
}
