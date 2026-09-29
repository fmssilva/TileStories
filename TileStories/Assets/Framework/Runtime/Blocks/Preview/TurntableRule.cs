using UnityEngine;

namespace TileStories
{
    // Where a turntable preview (model_3d, step 10A.2) is looking: yaw wraps all the way around, pitch and zoom stay
    // inside their limits, and IdleSeconds is how long since a finger last touched it (drives auto-spin resume)
    public readonly struct TurntableState
    {
        public readonly float Yaw;
        public readonly float Pitch;
        public readonly float Zoom;
        public readonly float IdleSeconds;

        public TurntableState(float yaw, float pitch, float zoom, float idleSeconds)
        {
            Yaw = yaw;
            Pitch = pitch;
            Zoom = zoom;
            IdleSeconds = idleSeconds;
        }

        public static readonly TurntableState Start = new(0f, 0f, 1f, 0f);
    }

    // Pure so every case is a unit test: a drag turns the model (yaw wraps, pitch stops short of the poles so it never
    // flips upside down), a pinch zooms within limits, and while nothing touches it the model auto-spins -- but only
    // after a pause, so a finger that just lifted does not fight the spin starting under it
    public static class TurntableRule
    {
        public const float MinPitch = -80f;
        public const float MaxPitch = 80f;
        public const float MinZoom = 1f;
        public const float MaxZoom = 3f;
        // Degrees per second the model turns on its own once idle long enough
        public const float AutoSpinSpeed = 12f;
        // A touch must have lifted this long before auto-spin resumes
        public const float AutoSpinResumeAfter = 2f;

        // A drag of `delta` screen units at `degreesPerUnit` (a look sets its own sensitivity)
        public static TurntableState Drag(TurntableState state, Vector2 delta, float degreesPerUnit) =>
            Clamp(new TurntableState(state.Yaw + delta.x * degreesPerUnit, state.Pitch - delta.y * degreesPerUnit, state.Zoom, 0f));

        // A pinch by `factor` (>1 zooms in)
        public static TurntableState Pinch(TurntableState state, float factor) =>
            Clamp(new TurntableState(state.Yaw, state.Pitch, state.Zoom * factor, 0f));

        // `deltaSeconds` pass with no finger on it: the idle clock grows, and past AutoSpinResumeAfter the model turns
        public static TurntableState Idle(TurntableState state, float deltaSeconds)
        {
            float idle = state.IdleSeconds + deltaSeconds;
            float yaw = state.Yaw;
            if (idle > AutoSpinResumeAfter) yaw += AutoSpinSpeed * deltaSeconds;
            return Clamp(new TurntableState(yaw, state.Pitch, state.Zoom, idle));
        }

        private static TurntableState Clamp(TurntableState s) =>
            new(WrapDegrees(s.Yaw), Mathf.Clamp(s.Pitch, MinPitch, MaxPitch), Mathf.Clamp(s.Zoom, MinZoom, MaxZoom), s.IdleSeconds);

        private static float WrapDegrees(float degrees) => Mathf.Repeat(degrees, 360f);
    }
}
