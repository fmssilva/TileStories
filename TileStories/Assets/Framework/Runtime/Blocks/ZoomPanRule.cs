using UnityEngine;

namespace TileStories
{
    // Where a zoomed picture sits in its frame: its scale (1 = the whole picture fitted in the frame) and the offset of its
    // top-left corner from the frame's (panel units, <= 0 on an axis where it is larger than the frame)
    public readonly struct ZoomPan
    {
        public readonly float Scale;
        public readonly Vector2 Offset;

        public ZoomPan(float scale, Vector2 offset)
        {
            Scale = scale;
            Offset = offset;
        }
    }

    // The zoom_image rules (_3.1 Tier 2), pure so every case is a unit test:
    //   - at scale 1 the picture fits the frame whole, centred ("contain": no part cut off)
    //   - scale stays between 1 and MaxScale
    //   - a zoom (pinch, Ctrl+wheel) keeps the picture point under the fingers / pointer where it is
    //   - a pan never shows empty space beside a picture larger than the frame; a side that fits stays centred
    public static class ZoomPanRule
    {
        public const float MaxScale = 4f;

        // The picture's size at scale 1: as large as fits in `frame` with its aspect (width / height)
        public static Vector2 FitSize(Vector2 frame, float aspect)
        {
            if (frame.x <= 0f || frame.y <= 0f || aspect <= 0f) return Vector2.zero;
            return frame.x / frame.y > aspect ? new Vector2(frame.y * aspect, frame.y) : new Vector2(frame.x, frame.x / aspect);
        }

        // The whole picture, centred
        public static ZoomPan Whole(Vector2 frame, float aspect) => Clamp(new ZoomPan(1f, Vector2.zero), frame, aspect);

        // Zoom by `factor` about `point` (frame coordinates): that point of the picture stays under it
        public static ZoomPan ZoomAbout(ZoomPan state, float factor, Vector2 point, Vector2 frame, float aspect)
        {
            float scale = Mathf.Clamp(state.Scale * factor, 1f, MaxScale);
            float ratio = scale / Mathf.Max(state.Scale, 1e-4f);
            var offset = point - (point - state.Offset) * ratio;
            return Clamp(new ZoomPan(scale, offset), frame, aspect);
        }

        // Move the picture by `delta` (frame units)
        public static ZoomPan Pan(ZoomPan state, Vector2 delta, Vector2 frame, float aspect) =>
            Clamp(new ZoomPan(state.Scale, state.Offset + delta), frame, aspect);

        // The picture's size at this state
        public static Vector2 SizeOf(ZoomPan state, Vector2 frame, float aspect) => FitSize(frame, aspect) * state.Scale;

        // Keep the state inside the rules: scale in range, no empty space beside a larger-than-frame side, a fitting side centred
        public static ZoomPan Clamp(ZoomPan state, Vector2 frame, float aspect)
        {
            float scale = Mathf.Clamp(state.Scale, 1f, MaxScale);
            var size = FitSize(frame, aspect) * scale;
            return new ZoomPan(scale, new Vector2(ClampAxis(state.Offset.x, size.x, frame.x), ClampAxis(state.Offset.y, size.y, frame.y)));
        }

        private static float ClampAxis(float offset, float size, float frame) =>
            size <= frame ? (frame - size) / 2f : Mathf.Clamp(offset, frame - size, 0f);
    }
}
