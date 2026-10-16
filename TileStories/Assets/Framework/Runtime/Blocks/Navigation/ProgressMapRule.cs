using System.Collections.Generic;

namespace TileStories
{
    // The widths of the progress_map's segments (_3.2 section 5.1, canvas E2), pure: one segment per section, sized by its target count,
    // filling the bar -- but never narrower than its own minimum (a whole tap target, or its words when it has no icon). Found by fixing
    // the segments whose share falls under their minimum and sharing what is left among the others, until every share holds. When the
    // minimums alone are wider than the bar, every segment keeps its minimum and the bar scrolls sideways (the strip's NavStripRule).
    public static class ProgressMapRule
    {
        public static float[] Widths(IReadOnlyList<int> counts, IReadOnlyList<float> minimums, float available)
        {
            int n = counts?.Count ?? 0;
            var widths = new float[n];
            if (n == 0) return widths;
            float minimumTotal = 0f;
            for (int i = 0; i < n; i++) minimumTotal += minimums[i];
            if (float.IsNaN(available) || minimumTotal >= available)
            {
                for (int i = 0; i < n; i++) widths[i] = minimums[i];
                return widths;
            }

            var atMinimum = new bool[n];
            // - each round fixes at least one more segment at its minimum, so n rounds always settle it
            for (int round = 0; round <= n; round++)
            {
                float room = available;
                int countLeft = 0;
                for (int i = 0; i < n; i++)
                    if (atMinimum[i]) room -= minimums[i];
                    else countLeft += counts[i] > 0 ? counts[i] : 0;
                bool changed = false;
                for (int i = 0; i < n; i++)
                {
                    if (atMinimum[i]) { widths[i] = minimums[i]; continue; }
                    float share = countLeft > 0 ? room * (counts[i] > 0 ? counts[i] : 0) / countLeft : 0f;
                    if (share < minimums[i])
                    {
                        atMinimum[i] = true;
                        changed = true;
                    }
                    widths[i] = share;
                }
                if (!changed) return widths;
            }
            return widths;
        }
    }
}
