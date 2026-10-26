using UnityEngine;

namespace TileStories
{
    // Pure: how wide each part of the page's one-line breadcrumb (point > section > page) may be (_3.3 S2c). When the line does not fit, the
    // MIDDLE part is cut first (an ellipsis), then the point's, and the page's own name, the bold last part, only when nothing else is left.
    // Plain flex-shrink would cut all three by their sizes, so the order is decided here and the view sets the widths.
    public static class PageCrumbFitRule
    {
        // The parts, in the line's order
        public const int Point = 0;
        public const int Section = 1;
        public const int Name = 2;

        // What gives way first (the middle part), then the point, then the name
        private static readonly int[] CutOrder = { Section, Point, Name };

        // The width of each part: `natural` (what each needs whole; 0 = the part is not there), inside `available`, none cut below its
        // `floor` (what an ellipsis alone needs). Parts that fit keep their natural width; when even the floors do not fit, the floors
        public static float[] Widths(float[] natural, float available, float[] floor)
        {
            var widths = (float[])natural.Clone();
            float over = Sum(widths) - available;
            foreach (int part in CutOrder)
            {
                if (over <= 0f) break;
                if (widths[part] <= 0f) continue;
                float least = Mathf.Min(floor[part], widths[part]);
                float cut = Mathf.Min(over, widths[part] - least);
                widths[part] -= cut;
                over -= cut;
            }
            return widths;
        }

        private static float Sum(float[] values)
        {
            float sum = 0f;
            foreach (float v in values) sum += v;
            return sum;
        }
    }
}
