using UnityEngine;

namespace TileStories
{
    /// <summary>
    /// Pure-rotation helper for the editor / development camera look controls.
    /// Extracts the yaw-pitch math from MockLocalizationProvider into a static
    /// class so it can be unit-tested without a scene or a camera in the scene.
    /// </summary>
    public static class EditorCameraLook
    {
        // Euler-angle wrap threshold for pitch: Unity stores angles in [0, 360),
        // so any value > 180 actually represents a negative angle.
        private const float PitchWrapThreshold = 180f;

        // Hard clamp preventing gimbal lock near straight up / down.
        private const float PitchClamp = 80f;

        /// <summary>
        /// Apply a look delta (mouse or arrow-key derived) to the camera's current
        /// rotation and return the resulting rotation.
        /// The delta is always relative to currentRotation, never to a hardcoded
        /// zero-origin. This is the fix for the bug where arrow-key look snapped the
        /// camera back to identity when mouse-look had already rotated it.
        /// Kept for existing call sites; delegates to the roll-aware overload with roll = 0.
        /// </summary>
        public static Quaternion ApplyDelta(Quaternion currentRotation, Vector2 delta, float deltaTime)
        {
            return ApplyDelta(currentRotation, delta, deltaTime, roll: 0f);
        }

        /// <summary>
        /// Same as the 3-argument overload, but also writes an explicit roll (Z) angle,
        /// so the Editor mock camera can reach device-roll-dependent orientation
        /// behaviour that a hard-zeroed Z otherwise makes unreachable in Tier A
        /// (_2.1_Marker_Orientation.md S8 / .clinerules/40-testing.md 4.1).
        /// </summary>
        public static Quaternion ApplyDelta(Quaternion currentRotation, Vector2 delta, float deltaTime, float roll)
        {
            // Read yaw and pitch from the camera's actual current rotation.
            float yaw   = currentRotation.eulerAngles.y;
            float pitch = currentRotation.eulerAngles.x;

            // Unity stores euler angles in the 0-360 range; unwrap pitch so we
            // can clamp symmetrically around zero before writing back.
            if (pitch > PitchWrapThreshold) pitch -= 360f;

            yaw   += delta.x * deltaTime * 10f;
            pitch -= delta.y * deltaTime * 10f;
            pitch  = Mathf.Clamp(pitch, -PitchClamp, PitchClamp);

            return Quaternion.Euler(pitch, yaw, roll);
        }
    }
}
