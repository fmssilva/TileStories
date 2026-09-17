namespace TileStories.Editor
{
    // Pure rotation-math for the per-POI "Edit Rotation" slider. The runtime
    // MarkerBillboard always screen-aligns markers to the camera, so this only
    // tilts the rig marker in the Editor Scene view for placement/preview. All
    // math is pure and parameterised so it can be Tier-0 tested without a
    // SceneView. The window applies the returned Quaternion to the rig child.
    public static class PoiRotationResolver
    {
        // Default angle used when a POI is created (identity facing).
        public const int DefaultEditorRotationDeg = 0;

        // Yaw around +Y. editor_rotation_deg = 0 must yield identity (the
        // billboard baseline), so the default never visually changes a marker.
        // Kept as a thin wrapper so existing tests and call sites still compile;
        // the general form is ToEulerQuaternion below.
        public static UnityEngine.Quaternion ToYawQuaternion(float degrees)
        {
            return ToEulerQuaternion(0f, degrees, 0f);
        }

        // Full Euler rotation around (X, Y, Z) in degrees. Used by the editor
        // rig to apply the developer's scene-rotate angles (including pitch and
        // roll, which the runtime billboard ignores at play time). Each axis is
        // normalized into [0, 360) so the slider and scene stay in lockstep.
        public static UnityEngine.Quaternion ToEulerQuaternion(float xDeg, float yDeg, float zDeg)
        {
            return UnityEngine.Quaternion.Euler(
                NormalizeAngleDeg(xDeg),
                NormalizeAngleDeg(yDeg),
                NormalizeAngleDeg(zDeg));
        }

        // Fold any angle into the [0, 360) range, tolerating negatives and
        // values past a full turn (keeps the slider + scene in lockstep).
        public static float NormalizeAngleDeg(float degrees)
        {
            degrees %= 360f;
            if (degrees < 0f)
                degrees += 360f;
            return degrees;
        }

        // Legacy alias kept so existing tests and call sites that read
        // "NormalizeYawDeg" still compile; NormalizeAngleDeg is the general form.
        public static float NormalizeYawDeg(float degrees) => NormalizeAngleDeg(degrees);
    }
}