using System;
using System.Collections.Generic;
using UnityEngine;

namespace TileStories
{
    /// <summary>
    /// Encapsulates tie-breaking logic for displacement algorithms.
    /// Provides deterministic selection of which marker in an overlap group receives displacement
    /// when using the "lower_priority_only" tie-break mode.
    /// </summary>
    public static class DisplacementTieBreakStrategy
    {
        /// <summary>
        /// Resolve the primary anchor (the group member that stays at its true
        /// position) for the given <paramref name="tiebreak"/> mode.
        ///
        /// - "symmetric" / null / unknown: returns false and anchorLocalIndex = -1,
        ///   so every member displaces (no single anchor).
        /// - "lower_priority_only": returns true with anchorLocalIndex = the unique
        ///   local index of the member with the lowest priority number (highest
        ///   priority). If the lowest priority is shared by more than one member,
        ///   returns false / -1 so the caller falls back to symmetric behaviour
        ///   for that group -- no arbitrary asymmetry between equals (spec _2.5 Section 7).
        /// </summary>
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

            int min = int.MaxValue;
            int minIdx = -1;
            int minCount = 0;

            for (int i = 0; i < groupPriorities.Count; i++)
            {
                int p = groupPriorities[i];
                if (p < min)
                {
                    min = p;
                    minIdx = i;
                    minCount = 1;
                }
                else if (p == min)
                {
                    minCount++;
                }
            }

            if (minCount == 1)
            {
                anchorLocalIndex = minIdx;
                return true;
            }

            // Shared minimum (same-priority tie): fall back to symmetric.
            return false;
        }
    }
}