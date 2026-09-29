using UnityEngine;

namespace TileStories
{
    // Where a 360 viewer (panorama_360, step 10A.4) is looking: yaw wraps all the way around (there is no seam to hit),
    // pitch stops short of straight up/down, and Fov is the field of view a pinch narrows or widens within limits
    public readonly struct PanoramaViewState
    {
        public readonly float Yaw;
        public readonly float Pitch;
        public readonly float Fov;

        public PanoramaViewState(float yaw, float pitch, float fov)
        {
            Yaw = yaw;
            Pitch = pitch;
            Fov = fov;
        }

        public static readonly PanoramaViewState Start = new(0f, 0f, DefaultFov);
        public const float DefaultFov = 70f;
    }

    // Pure so every case is a unit test: a drag looks around (yaw wraps, pitch clamped so it never flips past the
    // poles), a pinch changes the field of view within limits, and a gyro reading sets yaw/pitch directly (device
    // attitude, wrapped/clamped the same way a drag would be) -- the viewer's sphere itself is built from
    // EquirectRule, so "ahead" (yaw 0, pitch 0) is the same direction the generated picture calls ahead
    public static class PanoramaViewRule
    {
        public const float MinPitch = -85f;
        public const float MaxPitch = 85f;
        public const float MinFov = 40f;
        public const float MaxFov = 100f;

        // A drag of `delta` screen units at `degreesPerUnit`: dragging right turns the view left (as if turning your
        // head away from where your hand pulls), dragging up looks up
        public static PanoramaViewState Drag(PanoramaViewState state, Vector2 delta, float degreesPerUnit) =>
            Clamp(new PanoramaViewState(state.Yaw - delta.x * degreesPerUnit, state.Pitch + delta.y * degreesPerUnit, state.Fov));

        // A pinch by `factor` (>1 spreads fingers apart = zooms in = a narrower field of view)
        public static PanoramaViewState Pinch(PanoramaViewState state, float factor) =>
            Clamp(new PanoramaViewState(state.Yaw, state.Pitch, state.Fov / Mathf.Max(factor, 1e-4f)));

        // The device's own attitude (yaw, pitch degrees), read straight from the gyro when one exists
        public static PanoramaViewState Gyro(PanoramaViewState state, float yaw, float pitch) =>
            Clamp(new PanoramaViewState(yaw, pitch, state.Fov));

        private static PanoramaViewState Clamp(PanoramaViewState s) =>
            new(Mathf.Repeat(s.Yaw, 360f), Mathf.Clamp(s.Pitch, MinPitch, MaxPitch), Mathf.Clamp(s.Fov, MinFov, MaxFov));
    }
}
