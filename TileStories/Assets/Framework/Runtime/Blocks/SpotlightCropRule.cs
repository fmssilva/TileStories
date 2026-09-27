using UnityEngine;

namespace TileStories
{
    // The header's spotlight_crop look (_3.1 Tier 2), pure: the picture covers its frame (no empty edge), enlarged `zoom`
    // times, moved so the focus point (0..1 across and down the picture) sits as near the frame's centre as the picture's
    // edges allow; the ring goes on the focus point wherever it ends up.
    public static class SpotlightCropRule
    {
        public readonly struct Placement
        {
            // The picture's size and its top-left corner in the frame
            public readonly Vector2 Size;
            public readonly Vector2 Offset;
            // Where the focus point lands in the frame
            public readonly Vector2 Focus;

            public Placement(Vector2 size, Vector2 offset, Vector2 focus)
            {
                Size = size;
                Offset = offset;
                Focus = focus;
            }
        }

        // The size that covers `frame` with the picture's aspect (width / height): one side fits, the other overflows
        public static Vector2 CoverSize(Vector2 frame, float aspect)
        {
            if (frame.x <= 0f || frame.y <= 0f || aspect <= 0f) return Vector2.zero;
            return frame.x / frame.y > aspect ? new Vector2(frame.x, frame.x / aspect) : new Vector2(frame.y * aspect, frame.y);
        }

        public static Placement Place(Vector2 frame, float aspect, Vector2 focus, float zoom)
        {
            var size = CoverSize(frame, aspect) * Mathf.Max(1f, zoom);
            var f = new Vector2(Mathf.Clamp01(focus.x), Mathf.Clamp01(focus.y));
            var offset = frame / 2f - Vector2.Scale(f, size);
            offset = new Vector2(Mathf.Clamp(offset.x, frame.x - size.x, 0f), Mathf.Clamp(offset.y, frame.y - size.y, 0f));
            return new Placement(size, offset, offset + Vector2.Scale(f, size));
        }
    }
}
