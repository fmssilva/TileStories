namespace TileStories.Editor
{
    // Pure rotation math for the per-POI Facing X/Y/Z sliders. The authored angles are the
    // marker's rotation in the Scene view, and at runtime they are consumed according to the
    // wall's Facing mode (wall_fixed: all three; yaw_only: X and Z; always_facing_camera:
    // none) -- see MarkerOrientationResolver and _2.1_Marker_Orientation.md. All math is
    // pure and parameterised so it can be Tier-0 tested without a SceneView; the window
    // applies the returned Quaternion to the rig child.
    public static class PoiRotationResolver
    {
        // Full Euler rotation around (X, Y, Z) in degrees. Each axis is normalized into
        // [0, 360) so the sliders and the Scene view stay in lockstep.
        public static UnityEngine.Quaternion ToEulerQuaternion(float xDeg, float yDeg, float zDeg)
        {
            return UnityEngine.Quaternion.Euler(
                NormalizeAngleDeg(xDeg),
                NormalizeAngleDeg(yDeg),
                NormalizeAngleDeg(zDeg));
        }

        // Fold any angle into the [0, 360) range, tolerating negatives and
        // values past a full turn.
        public static float NormalizeAngleDeg(float degrees)
        {
            degrees %= 360f;
            if (degrees < 0f)
                degrees += 360f;
            return degrees;
        }

        // True when two rotations describe the same orientation. Euler triples are not
        // unique (Quaternion.Euler(100,0,0).eulerAngles reads back as (80,180,180)), so
        // "did the rotation change?" must compare orientations, never raw x/y/z floats --
        // otherwise a slider at X=100 would jump to the equivalent triple on every sync.
        public static bool IsSameOrientation(UnityEngine.Quaternion a, UnityEngine.Quaternion b)
        {
            return UnityEngine.Quaternion.Angle(a, b) < 0.05f;
        }
    }
}
