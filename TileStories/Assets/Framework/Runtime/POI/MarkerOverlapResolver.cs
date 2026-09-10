using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TileStories
{
    // Screen-space marker/label overlap resolver (spec _2.5 Sections 1/4/11a).
    // Replaces the legacy spawn-time resolver: union-find grouping over a fixed
    // screen-space snapshot, offsets written via MarkerView.ApplyLabelOffset /
    // ClearLabelOffset (label_only), re-evaluated every LODController cycle (step 8).
    // Block 2 = fixed_axis + label_only only; other algorithms/targets/tiebreaks
    // warn once and fall back (spec _2.5 Section 11a step 2).
    public static class MarkerOverlapResolver
    {
        private const string AlgorithmNotImplementedFmt = "[Displacement] displacement_algorithm '{0}' is not yet implemented; using fixed_axis.";

        private const string TargetNotImplementedFmt = "[Displacement] displace_target '{0}' is not yet implemented; using label_only.";

        private static readonly HashSet<string> _warnedOnce = new HashSet<string>();
        internal static void ResetWarnings() => _warnedOnce.Clear();
        private static void WarnOnce(string key, string message)
        {
            if (_warnedOnce.Add(key)) Debug.LogWarning(message);
        }

        /// <summary>
        /// Computes screen-space offset vectors for each marker in the input list.
        /// Uses the specified algorithm to resolve overlapping marker groups.
        /// Prioritizes markers by hierarchyLevelIndex (lower index = higher priority).
        /// </summary>
        /// <param name="screenPositions">Screen-space positions of markers (in pixels).</param>
        /// <param name="ids">POI IDs corresponding to each position.</param>
        /// <param name="settings">Displacement settings controlling algorithm and parameters.</param>
        /// <param name="visualUnits">Visual units containing priority information.</param>
        /// <returns>Array of screen-space offset vectors (in pixels) for each marker.</returns>
        public static Vector2[] ComputeOffsets(
            IReadOnlyList<Vector2> screenPositions, 
            IReadOnlyList<string> ids, 
            DisplacementSettings settings,
            IReadOnlyList<VisualUnit> visualUnits = null)
        {
            int n = screenPositions.Count;
            var offsets = new Vector2[n];
            if (n < 2 || settings == null || !settings.enabled)
                return offsets;

            string algorithm = settings.displacement_algorithm ?? "force_directed";
            switch (algorithm)
            {
                case "fixed_axis":
                    return ComputeOffsets_FixedAxis(screenPositions, ids, settings, visualUnits);
                case "candidate_position":
                    return ComputeOffsets_CandidatePosition(screenPositions, ids, settings, visualUnits);
                case "force_directed":
                    return ComputeOffsets_ForceDirected(screenPositions, ids, settings, visualUnits);
                default:
                    WarnOnce("algorithm", string.Format(AlgorithmNotImplementedFmt, algorithm));
                    return ComputeOffsets_FixedAxis(screenPositions, ids, settings, visualUnits);
            }
        }

        private static Vector2[] ComputeOffsets_FixedAxis(IReadOnlyList<Vector2> screenPositions, IReadOnlyList<string> ids, DisplacementSettings settings, IReadOnlyList<VisualUnit> visualUnits)
        {
            int n = screenPositions.Count;
            var offsets = new Vector2[n];
            if (n < 2 || settings == null || !settings.enabled)
                return offsets;

            float threshold = Mathf.Max(0.0001f, settings.overlap_threshold_px);
            float maxDisp = Mathf.Max(0f, settings.max_displacement_px);

            int[] parent = new int[n];
            for (int i = 0; i < n; i++) parent[i] = i;
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
            for (int i = 0; i < n; i++)
                for (int j = i + 1; j < n; j++)
                    if (Vector2.Distance(screenPositions[i], screenPositions[j]) < threshold)
                        Union(i, j);

            var groups = new Dictionary<int, List<int>>();
            for (int i = 0; i < n; i++)
            {
                int root = Find(i);
                if (!groups.TryGetValue(root, out var members))
                    groups[root] = members = new List<int>();
                members.Add(i);
            }

            foreach (var members in groups.Values)
            {
                if (members.Count < 2) continue;

                // Sort by priority (lower hierarchyLevelIndex = higher priority)
                // Fall back to ID comparison for deterministic tiebreaking
                members.Sort((a, b) =>
                {
                    int priorityA = visualUnits != null && a < visualUnits.Count
                        ? visualUnits[a].hierarchyLevelIndex
                        : int.MaxValue;
                    int priorityB = visualUnits != null && b < visualUnits.Count
                        ? visualUnits[b].hierarchyLevelIndex
                        : int.MaxValue;

                    int cmp = priorityA.CompareTo(priorityB);
                    if (cmp != 0) return cmp;

                    // Tiebreaker: ID comparison, then index
                    int c = string.CompareOrdinal(ids[a], ids[b]);
                    return c != 0 ? c : a.CompareTo(b);
                });

                // Apply symmetric vertical spread: higher priority markers get smaller offsets
                int count = members.Count;

                // 2.5-g: under lower_priority_only the unique highest-priority member
                // (the leader) stays pinned at its true position and the rest spread
                // away from it; a shared-minimum tie falls back to symmetric because
                // the strategy returns false (no arbitrary asymmetry between equals).
                var memberPriorities = new List<int>(count);
                foreach (int mi in members)
                    memberPriorities.Add(visualUnits != null && mi < visualUnits.Count
                        ? visualUnits[mi].hierarchyLevelIndex
                        : int.MaxValue);
                bool hasPinnedLeader = DisplacementTieBreakStrategy.TryGetPrimaryAnchorLocalIndex(
                    settings.displacement_tiebreak, memberPriorities, out int leaderLocal);

                for (int k = 0; k < count; k++)
                {
                    // Higher priority (earlier in sorted list) gets smaller vertical offset
                    float offsetY = (k - (count - 1) * 0.5f) * threshold;

                    // Ensure higher priority markers (smaller k) are closer to center
                    // by symmetric distribution around center
                    if (offsetY > maxDisp) offsetY = maxDisp;
                    if (offsetY < -maxDisp) offsetY = -maxDisp;
                    offsets[members[k]] = new Vector2(0f, offsetY);
                }

                if (hasPinnedLeader)
                {
                    // Re-express every member's offset relative to the leader so the
                    // leader itself lands on zero and everyone else sits away from it,
                    // preserving the pairwise separation the spread already produced.
                    Vector2 leaderOffset = offsets[members[leaderLocal]];
                    for (int k = 0; k < count; k++)
                    {
                        Vector2 off = offsets[members[k]] - leaderOffset;
                        if (off.magnitude > maxDisp) off = off.normalized * maxDisp;
                        offsets[members[k]] = off;
                    }
                }
            }
            return offsets;
        }

        /// <summary>
        /// Candidate position displacement algorithm (spec _2.5 Section 3).
        /// Generates candidate positions in concentric circular patterns around each
        /// marker, then greedily selects the best non-overlapping candidate per marker
        /// (processed in priority order: highest priority first).
        /// </summary>
        private static Vector2[] ComputeOffsets_CandidatePosition(
            IReadOnlyList<Vector2> screenPositions,
            IReadOnlyList<string> ids,
            DisplacementSettings settings,
            IReadOnlyList<VisualUnit> visualUnits)
        {
            int n = screenPositions.Count;
            var offsets = new Vector2[n];
            if (n < 2 || settings == null || !settings.enabled)
                return offsets;

            float threshold = Mathf.Max(0.0001f, settings.overlap_threshold_px);
            float maxDisp = Mathf.Max(0f, settings.max_displacement_px);

            // Build overlap groups (reuse existing union-find helper)
            var groups = BuildOverlapGroups(screenPositions, threshold, ids);

            // Process each group independently
            foreach (var kvp in groups)
            {
                var memberIndices = kvp.Value;
                if (memberIndices.Count < 2) continue;

                // Extract member data with priorities
                var members = new List<MemberData>();
                foreach (int idx in memberIndices)
                {
                    members.Add(new MemberData(
                        idx,
                        screenPositions[idx],
                        ids[idx],
                        visualUnits != null && idx < visualUnits.Count
                            ? visualUnits[idx].hierarchyLevelIndex
                            : int.MaxValue
                    ));
                }

                // Sort by priority (ascending: lower index = higher priority first)
                members.Sort((a, b) =>
                {
                    int cmp = a.priority.CompareTo(b.priority);
                    if (cmp != 0) return cmp;
                    int c = string.CompareOrdinal(a.id, b.id); // Tiebreaker: ID
                    return c != 0 ? c : a.index.CompareTo(b.index); // Then index
                });

                // Compute symmetric target angles for equal-priority runs (spec _2.5 Section 7).
                // Equal-priority markers get evenly spaced angular targets so labels distribute
                // symmetrically instead of greedily clustering, which can leave sub-threshold
                // Y gaps between markers placed at complementary angles (e.g. 0 degrees and 135 degrees).
                float[] targetAngles = new float[members.Count];
                for (int i = 0; i < members.Count; i++) targetAngles[i] = -1f; // -1 = no target

                if (settings.displacement_tiebreak == "symmetric")
                {
                    int runStart = 0;
                    for (int i = 0; i <= members.Count; i++)
                    {
                        if (i == members.Count || members[i].priority != members[runStart].priority)
                        {
                            int runSize = i - runStart;
                            if (runSize > 1)
                            {
                                // Start targets at 90 degrees (vertical) so 2-member
                                // equal-priority runs distribute along Y (90/270) instead
                                // of X (0/180), which collapses to Y=0 at ring 0 and
                                // produces a 0px Y-gap. Vertical spread is what the
                                // label Y-separation test measures.
                                for (int k = 0; k < runSize; k++)
                                    targetAngles[runStart + k] = 90f + k * (360f / runSize);
                            }
                            runStart = i;
                        }
                    }
                }

                // 2.5-g: under lower_priority_only the unique highest-priority member
                // stays pinned at its true position (registered below so other
                // candidates must clear it); shared-minimum ties fall back to symmetric.
                var sortedPriorities = new List<int>(members.Count);
                foreach (var mem in members) sortedPriorities.Add(mem.priority);
                bool hasPinnedLeader = DisplacementTieBreakStrategy.TryGetPrimaryAnchorLocalIndex(
                    settings.displacement_tiebreak, sortedPriorities, out int leaderLocal);

                // Track final selected positions to prevent overlap
                var selectedPositions = new Dictionary<int, Vector2>();

                // Process each member in priority order (higher priority first)
                for (int m = 0; m < members.Count; m++)
                {
                    var member = members[m];

                    // 2.5-g: the pinned leader keeps its true wall position; registering
                    // its original screen pos makes later (lower-priority) candidates
                    // resolve to positions at least one threshold away from it, i.e.
                    // radially away from the leader.
                    if (hasPinnedLeader && m == leaderLocal)
                    {
                        selectedPositions[member.index] = member.originalPos;
                        offsets[member.index] = Vector2.zero;
                        continue;
                    }

                    float targetAngle = targetAngles[m];

                    Vector2 bestOffset = Vector2.zero;
                    float bestScore = float.MaxValue;
                    bool foundValid = false;

                    // Generate candidate positions in concentric circular patterns
                    int directions = 16; // Number of angular positions per ring
                    int maxRings = Mathf.CeilToInt(maxDisp / threshold) + 2;

                    // Evaluate ALL rings, not just the first one with a valid candidate.
                    // This lets the Y-separation penalty (below) push a candidate to a higher
                    // ring when all ring-0 positions that are 2D-non-overlapping still leave
                    // Y values too close to an already-placed marker. The inter-ring score gap
                    // (threshold ~= 40px) is large relative to angular/ySeparation terms, so
                    // a ring-1 candidate is only preferred when Y-penalty on ring-0 exceeds it.
                    for (int ring = 0; ring < maxRings; ring++)
                    {
                        float candidateRadius = Mathf.Min((ring + 1) * threshold, maxDisp);

                        for (int dir = 0; dir < directions; dir++)
                        {
                            float angle = (dir * 2f * Mathf.PI / directions) + (ring * Mathf.PI / directions);
                            float candX = member.originalPos.x + Mathf.Cos(angle) * candidateRadius;
                            float candY = member.originalPos.y + Mathf.Sin(angle) * candidateRadius;
                            Vector2 candidatePos = new Vector2(candX, candY);

                            // Calculate offset from original, normalized to exact
                            // candidate radius to eliminate floating-point noise in
                            // cos/sin that causes inconsistent offset magnitudes
                            // across directions (breaks priority ordering at same ring).
                            Vector2 offset = candidatePos - member.originalPos;
                            offset = offset.normalized * candidateRadius;
                            candidatePos = member.originalPos + offset;

                            // Skip if exceeds max displacement
                            if (offset.magnitude > maxDisp) continue;

                            // Check overlap with already selected positions
                            bool overlaps = false;
                            foreach (var selectedKvp in selectedPositions)
                            {
                                Vector2 otherFinalPos = selectedKvp.Value;
                                if (Vector2.Distance(candidatePos, otherFinalPos) < threshold)
                                {
                                    overlaps = true;
                                    break;
                                }
                            }

                            if (overlaps) continue;

                            // Score candidate: prioritize minimal movement (inner ring), then
                            // angular target (symmetric distribution for equal-priority per spec
                            // _2.5 Section 7). Priority ordering is handled by the sort above
                            // (higher-priority members get first pick), NOT by inflating the
                            // score here -- within a single member's evaluation, priority and
                            // id are constants that only shift all candidates equally, so they
                            // serve no discriminating purpose and their large magnitude with
                            // int.MaxValue priorities would destroy float32 precision of the
                            // angularScore (ULP at ~2.1e8 is ~25.5, dwarfing the max 1.8).
                            float distanceScore = offset.magnitude;

                            // Small angular preference so equal-priority markers spread symmetrically.
                            // Weight is tiny: within a ring all candidates share the same
                            // distanceScore so angularScore is the sole differentiator, and the
                            // max (180 * 0.01 = 1.8) stays well below the inter-ring distance gap
                            // (threshold = 40), so inner rings are never skipped.
                            float angularScore = targetAngle >= 0f
                                ? Mathf.Abs(Mathf.DeltaAngle(targetAngle, Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg)) * 0.01f
                                : 0f;
                            // Penalize candidates whose Y is too close to an already-placed marker.
                            // Without this, two markers at complementary ring-0 directions (e.g. 202.5deg
                            // and 337.5deg both give Y ~= -15.3 at radius 40) pass the 2D distance check
                            // (sqrt(73.92^2+0) = 73.9 > 40) yet collapse to a ~0px Y-gap, failing the
                            // label-separation assertion. Weight is calibrated so that a candidate
                            // sitting just under threshold Y away (~25px gap from a placed marker)
                            // accrues enough penalty (~150) to lose to a ring-1 candidate with clean
                            // Y separation (score ~80): the inter-ring gap (40px) is the natural
                            // scale, so weight 10x maps 15px of Y-shortfall to 150 score, comfortably
                            // exceeding the 40px ring jump.
                            float ySeparationPenalty = 0f;
                            foreach (var selectedKvp in selectedPositions)
                            {
                                float yDiff = Mathf.Abs(candidatePos.y - selectedKvp.Value.y);
                                if (yDiff < threshold)
                                    ySeparationPenalty += (threshold - yDiff) * 10f;
                            }
                            float totalScore = distanceScore + angularScore + ySeparationPenalty;

                            if (totalScore < bestScore)
                            {
                                bestScore = totalScore;
                                bestOffset = offset;
                                foundValid = true;
                            }
                        }
                    }

                    // If no valid candidate found, use zero offset
                    if (!foundValid) bestOffset = Vector2.zero;

                    selectedPositions[member.index] = member.originalPos + bestOffset;
                    offsets[member.index] = bestOffset;
                }
            }

            return offsets;
        }

        /// <summary>
        /// Helper struct holding per-marker data for the candidate position algorithm.
        /// </summary>
        private struct MemberData
        {
            public int index;
            public Vector2 originalPos;
            public string id;
            public int priority;

            public MemberData(int index, Vector2 originalPos, string id, int priority)
            {
                this.index = index;
                this.originalPos = originalPos;
                this.id = id;
                this.priority = priority;
            }
        }

        private static Dictionary<int, List<int>> BuildOverlapGroups(IReadOnlyList<Vector2> positions, float threshold, IReadOnlyList<string> ids)
        {
            int n = positions.Count;
            var groups = new Dictionary<int, List<int>>();
            int[] parent = new int[n];
            for (int i = 0; i < n; i++) parent[i] = i;
            int Find(int x)
            {
                while (parent[x] != x) { parent[x] = parent[parent[x]]; x = parent[x]; }
                return x;
            }
            for (int i = 0; i < n; i++)
                for (int j = i + 1; j < n; j++)
                    if (Vector2.Distance(positions[i], positions[j]) < threshold)
                    {
                        int ra = Find(i), rb = Find(j);
                        if (ra != rb) parent[ra] = rb;
                    }
            for (int i = 0; i < n; i++)
            {
                int root = Find(i);
                if (!groups.TryGetValue(root, out var members))
                    groups[root] = members = new List<int>();
                members.Add(i);
            }
            return groups;
        }

        /// <summary>
        /// Force-directed displacement algorithm (spec _2.5 Section 4).
        /// Iteratively applies repulsion between overlapping markers within each
        /// group until equilibrium is reached or max iterations is hit. Higher-
        /// priority markers (lower hierarchyLevelIndex) stay closer to original.
        /// </summary>
        private static Vector2[] ComputeOffsets_ForceDirected(
            IReadOnlyList<Vector2> screenPositions,
            IReadOnlyList<string> ids,
            DisplacementSettings settings,
            IReadOnlyList<VisualUnit> visualUnits)
        {
            int n = screenPositions.Count;
            var offsets = new Vector2[n];
            if (n < 2 || settings == null || !settings.enabled)
                return offsets;

            float threshold = Mathf.Max(0.0001f, settings.overlap_threshold_px);
            float maxDisp = Mathf.Max(0f, settings.max_displacement_px);

            // Build overlap groups using union-find
            var groups = BuildOverlapGroups(screenPositions, threshold, ids);

            foreach (var kvp in groups)
            {
                var memberIndices = kvp.Value;
                if (memberIndices.Count < 2) continue;

                // Build working positions for this group
                var groupPositions = new List<Vector2>();
                var groupOrigPositions = new List<Vector2>();
                var groupPriorities = new List<int>();
                var groupScreenIndices = new List<int>();

                foreach (int idx in memberIndices)
                {
                    groupPositions.Add(screenPositions[idx]);
                    groupOrigPositions.Add(screenPositions[idx]);
                    groupPriorities.Add(visualUnits != null && idx < visualUnits.Count
                        ? visualUnits[idx].hierarchyLevelIndex
                        : int.MaxValue);
                    groupScreenIndices.Add(idx);
                }

                int count = groupPositions.Count;

                // 2.5-g: under lower_priority_only the unique highest-priority member
                // is pinned at its true position; repulsion FROM it still pushes the
                // other members away. Shared-minimum ties fall back to symmetric.
                bool hasPinnedLeader = DisplacementTieBreakStrategy.TryGetPrimaryAnchorLocalIndex(
                    settings.displacement_tiebreak, groupPriorities, out int leaderLocal);

                // Iterative force-directed repulsion
                int maxIterations = Mathf.Max(1, settings.force_directed_iterations);
                float damping = 0.5f;

                for (int iter = 0; iter < maxIterations; iter++)
                {
                    var forces = new Vector2[count];
                    bool anyOverlap = false;

                    for (int i = 0; i < count; i++)
                    {
                        for (int j = 0; j < count; j++)
                        {
                            if (i == j) continue;

                            Vector2 diff = groupPositions[i] - groupPositions[j];
                            float dist = diff.magnitude;

                            if (dist < threshold)
                            {
                                anyOverlap = true;
                                // Repulsion: push apart based on overlap depth
                                float overlapDepth = threshold - dist;
                                float pushStrength = overlapDepth * damping;
                                // When markers sit at the exact same screen position, diff is zero
                                // and normalized yields no direction. Use a deterministic angular
                                // perturbation keyed on the member index so overlapping markers
                                // always separate (prevents a degenerate fixed-point where all
                                // forces cancel and offsets stay zero).
                                // NOTE: angular perturbation alone does not guarantee vertical
                                // label separation -- the anchor force can collapse Y gaps to
                                // zero over many iterations, so a post-processing Y-separation
                                // pass below enforces minimum gaps for equal-priority groups.
                                Vector2 pushDir = dist > 0.0001f ? diff / dist : new Vector2(Mathf.Cos(i * 2.4f), Mathf.Sin(i * 2.4f));
                                forces[i] += pushDir * pushStrength;
                            }
                        }
                    }

                    // Apply forces with priority-aware anchoring
                    for (int i = 0; i < count; i++)
                    {
                        // 2.5-g: the pinned leader never moves -- reset to origin every
                        // iteration so accumulated repulsion cannot drag it off its anchor.
                        if (hasPinnedLeader && i == leaderLocal)
                        {
                            groupPositions[i] = groupOrigPositions[i];
                            continue;
                        }

                        Vector2 pos = groupPositions[i];
                        pos += forces[i];

                        // Higher-priority markers (lower index) are pulled back toward origin more strongly
                        float priorityWeight = 1f / (groupPriorities[i] + 1);
                        Vector2 anchorForce = (groupOrigPositions[i] - pos) * priorityWeight * 0.1f;
                        pos += anchorForce;

                        // Clamp to max displacement
                        Vector2 original = groupOrigPositions[i];
                        Vector2 delta = pos - original;
                        if (delta.magnitude > maxDisp)
                        {
                            delta = delta.normalized * maxDisp;
                            pos = original + delta;
                        }

                        groupPositions[i] = pos;
                    }

                    if (!anyOverlap) break;
                }

                // Only apply Y-separation post-processing for equal-priority groups.
                // When priorities differ, the force-directed anchor forces already
                // establish the correct magnitude ordering; additional Y displacement
                // could violate it.
                bool allEqualPriority = groupPriorities.Count > 0 && groupPriorities.All(p => p == groupPriorities[0]);
                if (allEqualPriority)
                {
                    // Enforce minimum vertical separation between group members.
                    // Force-directed dynamics can leave pairs at identical Y in symmetric
                    // configs (mirror-image markers in odd-count groups), so a targeted
                    // Y-only pass guarantees all label pairs clear the readability threshold.
                    // Higher-priority markers (lower hierarchy index) move less.
                    float minSep = threshold * 0.875f;
                    for (int pass = 0; pass < 5; pass++)
                    {
                        bool changed = false;
                        for (int i = 0; i < count; i++)
                        {
                            for (int j = i + 1; j < count; j++)
                            {
                                float yDiff = groupPositions[i].y - groupPositions[j].y;
                                if (Mathf.Abs(yDiff) < minSep)
                                {
                                    float push = (minSep - Mathf.Abs(yDiff)) + 0.01f;
                                    float wi = 1f / (groupPriorities[i] + 1);
                                    float wj = 1f / (groupPriorities[j] + 1);
                                    float total = wi + wj;
                                    float moveI = push * (wj / total);
                                    float moveJ = push * (wi / total);
                                    Vector2 posI = groupPositions[i];
                                    Vector2 posJ = groupPositions[j];
                                    if (yDiff >= 0)
                                    {
                                        posI.y += moveI;
                                        posJ.y -= moveJ;
                                    }
                                    else
                                    {
                                        posI.y -= moveI;
                                        posJ.y += moveJ;
                                    }
                                    groupPositions[i] = posI;
                                    groupPositions[j] = posJ;
                                    changed = true;
                                }
                            }
                        }

                        // Clamp to max displacement after each adjustment pass
                        for (int i = 0; i < count; i++)
                        {
                            Vector2 deltaPost = groupPositions[i] - groupOrigPositions[i];
                            if (deltaPost.magnitude > maxDisp)
                            {
                                deltaPost = deltaPost.normalized * maxDisp;
                                groupPositions[i] = groupOrigPositions[i] + deltaPost;
                            }
                        }

                        if (!changed) break;
                    }
                }

                // Write final offsets
                for (int i = 0; i < count; i++)
                {
                    offsets[groupScreenIndices[i]] = groupPositions[i] - groupOrigPositions[i];
                }
            }

            return offsets;
        }

        // Section 9: 2-cycle commit gate for overlap-group membership. A marker at the
        // edge of overlap_threshold_px must survive two consecutive in-group
        // observations before displacing; one contrary out-group observation
        // cancels a pending entry; a committed-in member HOLDS its displacement
        // through one out-group observation and only commits out after a second
        // consecutive out-group observation. Returns the committed membership.
        // Mirrors LODController.CommitDensityState (same algorithm; bool instead of
        // the DensityState enum). Pure static so it is Tier-0 testable with no scene
        // or camera. Mutates `stability` in place -- the caller owns the dict.
        public static bool CommitGroupMembership(
            string poiId,
            bool hasNeighbour,
            Dictionary<string, DisplacementStabilityState> stability)
        {
            string key = poiId ?? string.Empty;
            if (!stability.TryGetValue(key, out var h))
                h = new DisplacementStabilityState { committed = false, pending = false, pendingCycles = 0 };

            if (hasNeighbour == h.committed)
            {
                // Stable against the committed state: clear any in-flight transition.
                h.pending = h.committed;
                h.pendingCycles = 0;
            }
            else if (hasNeighbour == h.pending)
            {
                // Second consecutive cycle agrees on the provisional target -> commit.
                h.pendingCycles++;
                if (h.pendingCycles >= 2)
                {
                    h.committed = h.pending;
                    h.pendingCycles = 0;
                }
            }
            else
            {
                // Target differs from both committed and pending: fresh provisional.
                h.pending = hasNeighbour;
                h.pendingCycles = 1;
            }

            stability[key] = h;
            return h.committed;
        }

        public static void ApplyDisplacement(IReadOnlyList<VisualUnit> visibleUnits, Camera cam, DisplacementSettings settings, Dictionary<string, DisplacementStabilityState> stability)
        {
            if (cam == null || visibleUnits == null || settings == null) return;

            if (!settings.enabled)
            {
                for (int i = 0; i < visibleUnits.Count; i++)
                {
                    visibleUnits[i]?.marker?.ClearLabelOffset();
                    visibleUnits[i]?.marker?.ClearMarkerOffset();
                    visibleUnits[i]?.marker?.UpdateLeaderLine(cam, settings);
                }
                return;
            }

            if (settings.displacement_algorithm != "fixed_axis" &&
                settings.displacement_algorithm != "candidate_position" &&
                settings.displacement_algorithm != "force_directed")
                WarnOnce("algorithm", string.Format(AlgorithmNotImplementedFmt, settings.displacement_algorithm));
            // 2.5-g: lower_priority_only is fully implemented across all three algorithm
            // branches via DisplacementTieBreakStrategy.TryGetPrimaryAnchorLocalIndex; no
            // fallback warning is emitted.


            int n = visibleUnits.Count;
            if (n < 2)
            {
                for (int i = 0; i < n; i++)
                {
                    visibleUnits[i]?.marker?.ClearLabelOffset();
                    visibleUnits[i]?.marker?.ClearMarkerOffset();
                    visibleUnits[i]?.marker?.UpdateLeaderLine(cam, settings);
                }
                return;
            }

            var screenPositions = new Vector2[n];
            var ids = new string[n];
            for (int i = 0; i < n; i++)
            {
                var u = visibleUnits[i];
                if (u == null || u.marker == null) { screenPositions[i] = Vector2.zero; ids[i] = string.Empty; continue; }
                Vector3 sp = cam.WorldToScreenPoint(u.worldPosition);
                screenPositions[i] = new Vector2(sp.x, sp.y);
                ids[i] = u.poiId ?? string.Empty;
            }



            Vector2[] offsets = ComputeOffsets(screenPositions, ids, settings, visibleUnits);

            // 2.5-h: decide up front (once per cycle) which lower-priority labels get
            // hidden because displacement hit the max clamp and they still crowd a
            // neighbour. Pure computation; consumed by the apply loop below.
            var hiddenLabels = ResolveLabelsToHide(screenPositions, offsets, visibleUnits, settings);

            float membershipThreshold = Mathf.Max(0.0001f, settings.overlap_threshold_px);
            for (int i = 0; i < n; i++)
            {
                var u = visibleUnits[i];
                var marker = u?.marker;
                if (marker == null) continue;
                bool hasNeighbour = false;
                for (int j = 0; j < n && !hasNeighbour; j++)
                {
                    if (j == i) continue;
                    if (Vector2.Distance(screenPositions[i], screenPositions[j]) < membershipThreshold)
                        hasNeighbour = true;
                }
                // Section 9 hysteresis: group membership must commit through two
                // consecutive cycles before a marker displaces, so a marker at the
                // threshold boundary does not flap on every visitor micro-move.
                string pid = u.poiId ?? string.Empty;
                bool committedInGroup = CommitGroupMembership(pid, hasNeighbour, stability);
                Vector2 applyOffset;
                if (hasNeighbour)
                {
                    // Genuinely overlapping this cycle: recompute fresh and snapshot the
                    // offset to hold for any subsequent out-group observation.
                    applyOffset = offsets[i];
                    var st = stability[pid];
                    st.committedOffset = applyOffset;
                    stability[pid] = st;
                }
                else
                {
                    // Committed-but-out during the hold window: the marker's own applied
                    // offset reads back as "no overlap", so offsets[i] is 0 here and would
                    // flap it to base. Reuse the offset committed while in-group instead.
                    applyOffset = stability[pid].committedOffset;
                }
                if (committedInGroup)
                {
                    // 2.5-h: hide the lower-priority member's label only when the clamp cap
                    // still leaves it crowding a neighbour. Restricted to label_only (hiding
                    // a whole marker is out-of-scope); marker/both always apply their offsets.
                    bool hideLabel = settings.displace_target == "label_only"
                        && hiddenLabels.Contains(i);
                    if (hideLabel)
                    {
                        marker.SetLabelVisible(false);
                        marker.ClearLabelOffset();
                    }
                    else
                    {
                        marker.SetLabelVisible(true);
                        switch (settings.displace_target)
                        {
                            case "marker":   marker.ApplyMarkerOffset(cam, applyOffset); break;
                            case "both":     marker.ApplyLabelOffset(cam, applyOffset);
                                             marker.ApplyMarkerOffset(cam, applyOffset); break;
                            default:         marker.ApplyLabelOffset(cam, applyOffset); break;
                        }
                    }
                }
                else
                {
                    marker.SetLabelVisible(true);
                    marker.ClearLabelOffset();
                    marker.ClearMarkerOffset();
                }
                marker.UpdateLeaderLine(cam, settings);
            }

            // Prune stability entries for markers no longer in the visible set;
            // a despawned or frustum-culled marker must not retain a stale committed
            // displacement it will never get to clear in a later cycle.
            var currentIds = new HashSet<string>();
            for (int i = 0; i < n; i++)
                currentIds.Add(visibleUnits[i]?.poiId ?? string.Empty);
            var staleKeys = new List<string>();
            foreach (var kvp in stability)
                if (!currentIds.Contains(kvp.Key))
                    staleKeys.Add(kvp.Key);
            for (int i = 0; i < staleKeys.Count; i++)
                stability.Remove(staleKeys[i]);
        }
        // 2.5-h: decide which labels to hide when displacement hits the max clamp and
        // members still crowd each other. Tier-0 testable (no Camera/MonoBehaviour dep).
        // Returns indices whose LABEL text should be hidden; restricted to label_only
        // (hiding a whole marker is out-of-scope, per _2.5 Section 7).
        internal static HashSet<int> ResolveLabelsToHide(
            IReadOnlyList<Vector2> screenPositions,
            Vector2[] offsets,
            IReadOnlyList<VisualUnit> visibleUnits,
            DisplacementSettings settings)
        {
            var hidden = new HashSet<int>();
            if (settings == null || settings.displace_target != "label_only")
                return hidden;

            float threshold = Mathf.Max(0.0001f, settings.overlap_threshold_px);
            float maxDisp = Mathf.Max(0f, settings.max_displacement_px);
            int n = screenPositions.Count;
            if (n < 2) return hidden;

            // Crowd-check post-displacement; if a pair still overlaps at build threshold and
            // at least one member is at its clamp, hide the lower-priority member's label.
            // Iterate with a bounded outer pass (a 3+ member group may need more than one hide).
            for (int pass = 0; pass < n + 2; pass++)
            {
                bool anyHide = false;
                for (int i = 0; i < n; i++)
                {
                    if (hidden.Contains(i)) continue;
                    for (int j = i + 1; j < n; j++)
                    {
                        if (hidden.Contains(j)) continue;
                        Vector2 fi = screenPositions[i] + offsets[i];
                        Vector2 fj = screenPositions[j] + offsets[j];
                        if (Vector2.Distance(fi, fj) >= threshold) continue;
                        // Only hide when at least one side is pinned at its max clamp.
                        if (offsets[i].magnitude < maxDisp - 0.001f &&
                            offsets[j].magnitude < maxDisp - 0.001f) continue;
                        int pi = PriorityOf(visibleUnits, i);
                        int pj = PriorityOf(visibleUnits, j);
                        // lower priority = larger hierarchyLevelIndex -> that label hides.
                        int loser = pi >= pj ? i : j;
                        hidden.Add(loser);
                        anyHide = true;
                        break;
                    }
                    if (hidden.Count >= n - 1) break;
                }
                if (!anyHide || hidden.Count >= n - 1) break;
            }
            return hidden;
        }

        private static int PriorityOf(IReadOnlyList<VisualUnit> vu, int i) =>
            vu != null && i >= 0 && i < vu.Count && vu[i] != null
                ? vu[i].hierarchyLevelIndex
                : int.MaxValue;

    }

    // Hysteresis bookkeeping for one marker's overlap-group membership (section 9).
    // Declared in this file (the displacement domain owner) and held by
    // LODController._displacementStability; mirrors DensityHysteresisState
    // (LODController.cs) in shape, not by reference, so the displacement domain
    // stays self-contained.
    public struct DisplacementStabilityState
    {
        public bool committed;      // membership currently driving the apply/clear decision
        public bool pending;        // provisional target mid-transition
        public int pendingCycles;   // consecutive cycles the pending target has held
        // Screen-px offset snapshot taken at the in-group commit cycle; held through the
        // first out-group observation so a marker's own applied offset doesn't read back
        // as "no longer overlapping" and re-apply zero (which would flap it to base).
        public Vector2 committedOffset;
    }
}
