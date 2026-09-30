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

        // A drag of `delta` panel units at `degreesPerUnit` (panel y grows DOWNWARD): the scene follows the finger, so dragging
        // right turns the view left and dragging DOWN looks up (the same grab-the-world feel as a street-view map)
        public static PanoramaViewState Drag(PanoramaViewState state, Vector2 delta, float degreesPerUnit) =>
            Clamp(new PanoramaViewState(state.Yaw - delta.x * degreesPerUnit, state.Pitch + delta.y * degreesPerUnit, state.Fov));

        // The field of view is measured along the stage's SHORTER side, so a zoom feels the same in a wide inline stage and a tall
        // full-screen page (70 degrees across a portrait page, not 36). This is the camera's vertical field of view for it: the field
        // itself on a wide stage (aspect = width / height >= 1), a wider one on a tall stage.
        public static float VerticalFov(float fov, float aspect) =>
            aspect >= 1f || aspect <= 0f ? fov : 2f * Mathf.Atan(Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad) / aspect) * Mathf.Rad2Deg;

        // How many degrees one panel unit of drag turns the view so the scene follows the finger: the stage's shorter side is the field
        // of view (a stage not laid out yet, a side unknown or under one unit, counts as one unit)
        public static float DegreesPerUnit(float fov, float stageShorterSide) => fov / (stageShorterSide >= 1f ? stageShorterSide : 1f);

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
