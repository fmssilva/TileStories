using System;

namespace TileStories
{
    // Edit distance for search typo tolerance (spec _2.6 section 5): optimal string alignment, the
    // Levenshtein distance plus "two neighbouring letters swapped" counted as ONE edit ("lmap" -> "lamp"),
    // the most common typing mistake. Pure, allocation-light, stops early once over the limit.
    public static class SearchTextDistance
    {
        // Edits turning a into b, or maxEdits + 1 as soon as it is known to exceed maxEdits
        public static int Bounded(string a, string b, int maxEdits)
        {
            if (a == null || b == null)
                return maxEdits + 1;
            if (Math.Abs(a.Length - b.Length) > maxEdits)
                return maxEdits + 1;

            // three rolling rows: two back (for the swap rule), previous, current
            var twoBack = new int[b.Length + 1];
            var prev = new int[b.Length + 1];
            var cur = new int[b.Length + 1];
            for (int j = 0; j <= b.Length; j++)
                prev[j] = j;

            for (int i = 1; i <= a.Length; i++)
            {
                cur[0] = i;
                int rowMin = cur[0];
                for (int j = 1; j <= b.Length; j++)
                {
                    int cost = a[i - 1] == b[j - 1] ? 0 : 1;
                    int d = Math.Min(Math.Min(prev[j] + 1, cur[j - 1] + 1), prev[j - 1] + cost);
                    if (i > 1 && j > 1 && a[i - 1] == b[j - 2] && a[i - 2] == b[j - 1])
                        d = Math.Min(d, twoBack[j - 2] + 1);
                    cur[j] = d;
                    if (d < rowMin) rowMin = d;
                }
                // - every later row is at least this row's minimum: stop once it is out of reach
                if (rowMin > maxEdits)
                    return maxEdits + 1;
                var recycled = twoBack;
                twoBack = prev;
                prev = cur;
                cur = recycled;
            }
            return Math.Min(prev[b.Length], maxEdits + 1);
        }
    }
}
