using UnityEngine;

namespace TileStories.LivingRoom
{
    // The pure geometry of the size_comparison drawing (_3.1 step 11, labels in step 11-fix): two upright shapes standing on one ground
    // line, drawn at ONE scale (pixels per centimetre) so their sizes read true to each other, the pair as large as the stage lets it be.
    // Each shape sits in a SLOT as wide as its label needs (a coin is a few pixels wide, its name is not), and the slots, not the
    // shapes, must fit the stage's width.
    public static class SizeComparisonRule
    {
        // The two shapes' sizes in pixels and the width of the slot each one (and its label) stands in. The object's are zero when the
        // app does not know the object: the point is drawn alone.
        public readonly struct Shapes
        {
            public readonly Vector2 Poi;
            public readonly Vector2 Object;
            public readonly float PixelsPerCm;
            public readonly float PoiSlotWidth;
            public readonly float ObjectSlotWidth;

            public Shapes(Vector2 poi, Vector2 objectSize, float pixelsPerCm, float poiSlotWidth, float objectSlotWidth)
            {
                Poi = poi;
                Object = objectSize;
                PixelsPerCm = pixelsPerCm;
                PoiSlotWidth = poiSlotWidth;
                ObjectSlotWidth = objectSlotWidth;
            }
        }

        // Whether a real size can be drawn at all: both sides above zero
        public static bool HasSize(float widthCm, float heightCm) => widthCm > 0f && heightCm > 0f;

        // Fit the point (and the object beside it, when there is one) into a stage. Each slot is as wide as its shape or its label,
        // whichever is wider (a label wider than half of what is left wraps: the slot never takes more than that); the two slots plus
        // `gap` must fit the width and the taller shape the height. The largest scale that fits wins, so nothing is cropped or squashed.
        public static bool TryFit(float stageWidth, float stageHeight, float gap, Vector2 poiCm, Vector2? objectCm,
            float poiLabelWidth, float objectLabelWidth, out Shapes shapes)
        {
            shapes = default;
            if (!HasSize(poiCm.x, poiCm.y)) return false;
            bool withObject = objectCm.HasValue && HasSize(objectCm.Value.x, objectCm.Value.y);
            var other = withObject ? objectCm.Value : Vector2.zero;
            float between = withObject ? gap : 0f;
            float available = stageWidth - between;
            if (available <= 0f || stageHeight <= 0f) return false;

            // - a label never asks for more than its share of the row: past that it wraps inside its slot
            float labelCap = withObject ? available / 2f : available;
            float poiMin = Mathf.Min(Mathf.Max(0f, poiLabelWidth), labelCap);
            float objectMin = withObject ? Mathf.Min(Mathf.Max(0f, objectLabelWidth), labelCap) : 0f;

            float tallest = Mathf.Max(poiCm.y, other.y);
            float best = stageHeight / tallest;
            // - the slots' total width grows with the scale, so the largest scale that fits is found by halving the range
            if (SlotsWidth(best, poiCm.x, other.x, poiMin, objectMin) > available)
            {
                float low = 0f, high = best;
                for (int i = 0; i < 40; i++)
                {
                    float middle = (low + high) / 2f;
                    if (SlotsWidth(middle, poiCm.x, other.x, poiMin, objectMin) <= available) low = middle; else high = middle;
                }
                best = low;
            }
            shapes = new Shapes(poiCm * best, other * best, best, Mathf.Max(poiCm.x * best, poiMin), withObject ? Mathf.Max(other.x * best, objectMin) : 0f);
            return true;
        }

        // The two slots' widths together at this scale
        private static float SlotsWidth(float scale, float poiWidthCm, float objectWidthCm, float poiMin, float objectMin) =>
            Mathf.Max(poiWidthCm * scale, poiMin) + (objectWidthCm > 0f ? Mathf.Max(objectWidthCm * scale, objectMin) : 0f);
    }
}
