using UnityEngine;

namespace TileStories.LivingRoom
{
    // The pure geometry of the size_comparison drawing (_3.1 step 11): two upright shapes standing on one ground line, drawn at ONE
    // scale (pixels per centimetre) so their sizes read true to each other, the pair as large as the stage lets it be.
    public static class SizeComparisonRule
    {
        // The two shapes' sizes in pixels (the object's is zero when the app does not know the object: the point is drawn alone)
        public readonly struct Shapes
        {
            public readonly Vector2 Poi;
            public readonly Vector2 Object;
            public readonly float PixelsPerCm;

            public Shapes(Vector2 poi, Vector2 objectSize, float pixelsPerCm)
            {
                Poi = poi;
                Object = objectSize;
                PixelsPerCm = pixelsPerCm;
            }
        }

        // Whether a real size can be drawn at all: both sides above zero
        public static bool HasSize(float widthCm, float heightCm) => widthCm > 0f && heightCm > 0f;

        // Fit the point (and the object beside it, when there is one) into a stage. The pair (plus `gap` between them) must fit the
        // width and the taller of the two the height; the smaller of the two scales wins, so nothing is ever cropped or squashed.
        public static bool TryFit(float stageWidth, float stageHeight, float gap, Vector2 poiCm, Vector2? objectCm, out Shapes shapes)
        {
            shapes = default;
            if (!HasSize(poiCm.x, poiCm.y)) return false;
            bool withObject = objectCm.HasValue && HasSize(objectCm.Value.x, objectCm.Value.y);
            var other = withObject ? objectCm.Value : Vector2.zero;
            float widthNeeded = poiCm.x + other.x;
            float availableWidth = stageWidth - (withObject ? gap : 0f);
            float tallest = Mathf.Max(poiCm.y, other.y);
            if (availableWidth <= 0f || stageHeight <= 0f) return false;
            float scale = Mathf.Min(availableWidth / widthNeeded, stageHeight / tallest);
            shapes = new Shapes(poiCm * scale, other * scale, scale);
            return true;
        }
    }
}
