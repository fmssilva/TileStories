using System;
using System.Collections.Generic;

namespace TileStories
{
    // Section 7 tiebreak logic for marker/label displacement (spec _2.5 Section 7,
    // _2.7 2.5-g). A plain static class with no MonoBehaviour/Unity dependency so it
    // is Tier-0 unit-testable with a `new`-free static call over plain int lists.
    //
    // The tiebreak resolves which (if any) member of an overlap group is allowed
    // to stay anchored at its true position while the others displace:
    //   - "symmetric" (default / any unrecognized value): no anchor -- every member
    //     displaces, so each algorithm applies its own symmetric spread.
    //   - "lower_priority_only": the unique member holding the minimum priority
    //     value (lower value = higher importance, per the hierarchy-level table) is
    //     the sole anchor and is pinned to its true position; all other members
    //     displace around it. If the minimum priority is SHARED by two or more
    //     members there is no distinct anchor -- the group falls back to symmetric
    //     behaviour, which enforces _2.5 Section 7's "same-priority ties fall back
    //     to symmetric -- no arbitrary asymmetry between equals" rule without
    //     inventing an arbitrary winner.
    public static class DisplacementTieBreakStrategy
    {
        /// <summary>
        /// Resolve the primary anchor for one overlap group under "lower_priority_only".
        /// </summary>
        /// <param name="tiebreak">The wall's displacement_tiebreak mode string.</param>
        /// <param name="groupPriorities">Priority value (hierarchyLevelIndex) per group
        /// member, parallel to the member list the caller iterates.</param>
        /// <param name="anchorLocalIndex">
        /// OUT: the group-local index of the unique minimum-priority member, or -1
        /// when the group should behave symmetrically (no distinct anchor).
        /// </param>
        /// <returns>
        /// True when a primary anchor was resolved (mode == "lower_priority_only" with a
        /// unique minimum priority); false otherwise (symmetric/unknown mode, empty
        /// group, or a shared minimum priority value).
        /// </returns>
        public static bool TryGetPrimaryAnchorLocalIndex(
            string tiebreak,
            IReadOnlyList<int> groupPriorities,
            out int anchorLocalIndex)
        {
            anchorLocalIndex = -1;
            if (tiebreak != "lower_priority_only")
                return false;
            if (groupPriorities == null || groupPriorities.Count == 0)
                return false;

            int minPriority = int.MaxValue;
            int minCount = 0;
            int minLocalIndex = -1;
            for (int i = 0; i < groupPriorities.Count; i++)
            {
                int p = groupPriorities[i];
                if (p < minPriority)
                {
                    minPriority = p;
                    minCount = 1;
                    minLocalIndex = i;
                }
                else if (p == minPriority)
                {
                    minCount++;
                }
            }

            // Shared minimum priority -> no distinct anchor -> symmetric fallback
            // (no arbitrary asymmetry between equal-priority members).
            if (minCount != 1)
                return false;

            anchorLocalIndex = minLocalIndex;
            return true;
        }
    }
}