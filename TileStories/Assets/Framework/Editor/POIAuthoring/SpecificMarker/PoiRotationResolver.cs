namespace TileStories.Editor
{
    // Pure rotation-math for the per-POI "Edit Rotation" slider. The runtime
    // MarkerBillboard always screen-aligns markers to the camera, so this only
    // yaws the rig marker in the Editor Scene view for placement/preview. All
    // math is pure and parameterised so it can be Tier-0 tested without a
    // SceneView. The window applies the returned Quaternion to the rig child.
    public static class PoiRotationResolver
    {
        // Default angle used when a POI is created (identity facing).
        public const int DefaultEditorRotationDeg = 0;

        // Fold any angle into the [0, 360) range, tolerating negatives and
        // values past a full turn (keeps the slider + scene in lockstep).
        public static float NormalizeYawDeg(float degrees)
        {
            degrees %= 360f;
            if (degrees < 0f)
                degrees += 360f;
            return degrees;
        }

        // Yaw around +Y. editor_rotation_deg = 0 must yield identity (the
        // billboard baseline), so the default never visually changes a marker.
        public static UnityEngine.Quaternion ToYawQuaternion(float degrees)
        {
            return UnityEngine.Quaternion.Euler(0f, NormalizeYawDeg(degrees), 0f);
        }
    }
}