namespace TileStories.Editor
{
    // Pure "will this facing edit be visible in the Scene view?" advice. With Edit-Mode
    // preview ON the rig shows what the RUNTIME would show (MarkerOrientationResolver), so
    // some authored angles are legitimately overridden by the wall's Facing mode. The
    // developer would otherwise see a slider move and nothing happen, and assume a bug.
    // Null = the edit is visible, no warning. Anything that is neither wall_fixed nor
    // yaw_only behaves as always_facing_camera, exactly as the resolver treats it.
    public static class FacingEditAdvice
    {
        public const string AlwaysFacingCameraMessage =
            "Facing is Always Facing Camera and Scene-Mode Preview is ON (Global Scene > Orientation): " +
            "the marker always faces the camera, so changing the X/Y/Z facing shows no change here.";

        public const string YawOnlyMessage =
            "Facing is Y Rotation Only and Scene-Mode Preview is ON (Global Scene > Orientation): " +
            "Y is replaced by the live camera yaw, so changing the Y facing shows no change here. X and Z do apply.";

        public const string GizmoOverriddenMessage =
            "Facing is Y Rotation Only and Scene-Mode Preview is ON (Global Scene > Orientation): " +
            "the preview overrides the Rotate gizmo. Use the X/Z Facing sliders, or turn the preview off to rotate by hand.";

        // The mode that really applies to a POI: its hierarchy level's override, else the wall's.
        public static string EffectiveFacingMode(OrientationSettings settings, string facingModeOverride)
        {
            if (!string.IsNullOrEmpty(facingModeOverride))
                return facingModeOverride;
            return settings != null ? settings.facing_mode : null;
        }

        // Warning for a Facing-slider edit that touched the given axes, or null when visible.
        public static string GetSliderWarning(OrientationSettings settings, string facingModeOverride,
            bool xChanged, bool yChanged, bool zChanged)
        {
            if (settings == null || !settings.edit_mode_preview_enabled)
                return null;
            if (!xChanged && !yChanged && !zChanged)
                return null;

            string mode = EffectiveFacingMode(settings, facingModeOverride);
            if (mode == "wall_fixed")
                return null;
            if (mode == "yaw_only")
                return yChanged ? YawOnlyMessage : null;
            return AlwaysFacingCameraMessage;
        }

        // Warning for a Rotate-gizmo drag under preview, or null when the gizmo edit is honoured.
        // The gizmo cannot say which axis moved, so yaw_only gets its own "use the sliders" note.
        public static string GetGizmoWarning(OrientationSettings settings, string facingModeOverride)
        {
            if (settings == null || !settings.edit_mode_preview_enabled)
                return null;

            string mode = EffectiveFacingMode(settings, facingModeOverride);
            if (mode == "wall_fixed")
                return null;
            return mode == "yaw_only" ? GizmoOverriddenMessage : AlwaysFacingCameraMessage;
        }
    }
}
