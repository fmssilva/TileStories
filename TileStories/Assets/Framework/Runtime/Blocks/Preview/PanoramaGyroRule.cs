using UnityEngine;

namespace TileStories
{
    // How the phone's own turning becomes the panorama's view (_3.1 step 10A.4.2, the `gyro` look). Pure, so every case is a unit test;
    // DeviceAttitude only reads the Input System's attitude sensor and hands the quaternion here.
    public static class PanoramaGyroRule
    {
        // The Input System's AttitudeSensor is "the same as Gyroscope.attitude" (its own source says so): a right-handed rotation of the
        // device, flat on a table = identity. Unity's well-known conversion turns it into the rotation of a camera looking out of the
        // device's BACK: flat on a table that camera looks straight down, held upright it looks at the horizon.
        public static Quaternion ToCameraRotation(Quaternion attitude) =>
            Quaternion.Euler(90f, 0f, 0f) * new Quaternion(attitude.x, attitude.y, -attitude.z, -attitude.w);

        // Where that camera looks, as the panorama's own angles: yaw grows turning right, pitch is positive looking up
        public static (float Yaw, float Pitch) ViewAngles(Quaternion attitude)
        {
            Vector3 forward = ToCameraRotation(attitude) * Vector3.forward;
            float yaw = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
            float pitch = Mathf.Asin(Mathf.Clamp(forward.y, -1f, 1f)) * Mathf.Rad2Deg;
            return (yaw, pitch);
        }
    }

    // Follows the phone: the FIRST reading anchors the view, so whichever way the visitor happens to face when the panorama appears is
    // the authored Start Heading (a sensor's own north means nothing to a picture); every later reading turns the view by how far the
    // phone has turned since. Pitch is absolute (gravity is the same for everybody). Plain C#: one per bound gyro view.
    public sealed class PanoramaGyroFollower
    {
        private readonly float _startHeading;
        private float? _yawOffset;

        public PanoramaGyroFollower(float startHeading)
        {
            _startHeading = startHeading;
        }

        // Whether a first reading has been taken
        public bool IsAnchored => _yawOffset.HasValue;

        // The view for this attitude, keeping `state`'s field of view (a pinch is the visitor's own)
        public PanoramaViewState Follow(PanoramaViewState state, Quaternion attitude)
        {
            var (yaw, pitch) = PanoramaGyroRule.ViewAngles(attitude);
            _yawOffset ??= _startHeading - yaw;
            return PanoramaViewRule.Gyro(state, yaw + _yawOffset.Value, pitch);
        }

        // Forget the anchor: the next reading is the start heading again
        public void Reset() => _yawOffset = null;
    }
}
