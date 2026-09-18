using UnityEngine;

namespace TileStories
{
    // Result of one orientation resolution call (_2.1_Marker_Orientation.md v4 section 8).
    // The resolver is stateless: it takes everything it needs as parameters and returns
    // this, so the caller (the MonoBehaviour) owns all state and the resolver stays
    // deterministic and testable.
    public struct OrientationResult
    {
        public Quaternion Rotation; // the world rotation to write
        public bool Resolved;       // false = degenerate input; caller keeps its previous rotation
    }

    // Pure orientation math for markers, labels, badges and clusters (_2.1_Marker_Orientation.md).
    // Stateless and static so every decision here is Tier-0 testable with no scene running.
    public static class MarkerOrientationResolver
    {
        // Safety guard for always_facing_camera only: stops a marker swinging to a near
        // edge-on angle when a visitor stands very close to or directly below/above it.
        // Not developer-exposed (_2.1_Marker_Orientation.md v4: roll/pitch conditioning
        // was a whole configurable domain in v3; keeping one hardcoded safety clamp is
        // simpler and covers the one real failure mode without the exposed complexity).
        private const float AlwaysFacingCameraMaxPitchDeg = 80f;

        // Screen "up" as a world-space direction, read from the camera's real projection so it
        // stays correct whether AR Foundation compensates for device rotation in the camera
        // transform or in the display matrix. Never assume cam.transform.up.
        public static Vector3 ScreenUpWorld(Camera cam)
        {
            if (cam == null) return Vector3.up;

            float z = cam.nearClipPlane + 1f;
            Vector3 a = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, z));
            Vector3 b = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.6f, z));
            Vector3 d = b - a;
            return d.sqrMagnitude > 1e-12f ? d.normalized : cam.transform.up;
        }

        // Resolve which "up" direction a Vertical Alignment mode ("world_up" | "screen_up")
        // actually means, given the caller's already-resolved screen-up and up-reference
        // vectors. Shared by the root, the cluster, and (via ResolveChildLocalRotation) the
        // label/badge, so "world_up" and "screen_up" mean exactly the same thing everywhere.
        public static Vector3 ResolveVerticalUp(string verticalAlignmentMode, Vector3 screenUpWorld, Vector3 upReference)
        {
            return verticalAlignmentMode == "screen_up" ? screenUpWorld : upReference; // default world_up
        }

        // Resolve the world rotation for a marker root. facing_mode picks the strategy:
        // wall_fixed returns the authored rotation untouched; yaw_only keeps the authored
        // X/Z wall tilt and replaces only the Y (yaw) with a live camera-facing value;
        // always_facing_camera ignores authored angles entirely and looks at the camera
        // using the resolved vertical-alignment up.
        public static OrientationResult ResolveRootRotation(
            OrientationSettings settings,
            string facingModeOverride,
            Vector3 markerWorldPos, Vector3 camPos, Vector3 camForward,
            Vector3 screenUpWorld, Vector3 upReference,
            Quaternion parentRotation, Quaternion authoredLocalRotation)
        {
            string facingMode = string.IsNullOrEmpty(facingModeOverride) ? settings.facing_mode : facingModeOverride;
            Vector3 up = ResolveVerticalUp(settings.vertical_alignment_mode, screenUpWorld, upReference);

            if (facingMode == "wall_fixed")
            {
                return new OrientationResult { Rotation = parentRotation * authoredLocalRotation, Resolved = true };
            }

            if (facingMode == "yaw_only")
            {
                // Wall tilt (X/Z) stays exactly as authored; only yaw tracks the camera,
                // computed the same way the editor's Y slider already means "yaw" (Unity's
                // Quaternion.Euler applies Y as the outermost/world-space rotation).
                Vector3 camFwdFlat = Vector3.ProjectOnPlane(camPos - markerWorldPos, up);
                if (camFwdFlat.sqrMagnitude < 1e-12f)
                {
                    return new OrientationResult { Rotation = parentRotation * authoredLocalRotation, Resolved = false };
                }

                float liveYawDeg = Quaternion.LookRotation(camFwdFlat, up).eulerAngles.y;
                Vector3 authoredEuler = authoredLocalRotation.eulerAngles;
                Quaternion yawOnly = parentRotation * Quaternion.Euler(authoredEuler.x, liveYawDeg, authoredEuler.z);
                return new OrientationResult { Rotation = yawOnly, Resolved = true };
            }

            // always_facing_camera
            Vector3 fwd = settings.facing_basis == "camera_position"
                ? markerWorldPos - camPos
                : camForward;

            // Degenerate LookRotation guard: view direction nearly parallel to the up
            // reference (e.g. looking straight down at a world_up marker) would produce
            // garbage. Keep the caller's previous rotation instead of snapping to identity.
            if (fwd.sqrMagnitude < 1e-12f || up.sqrMagnitude < 1e-12f ||
                Mathf.Abs(Vector3.Dot(fwd.normalized, up.normalized)) > 0.999f)
            {
                return new OrientationResult { Rotation = parentRotation, Resolved = false };
            }

            Quaternion rotation = Quaternion.LookRotation(fwd, up);
            rotation = ClampPitch(rotation, up, AlwaysFacingCameraMaxPitchDeg);

            return new OrientationResult { Rotation = rotation, Resolved = true };
        }

        // Child LOCAL rotation (pure Z) so the child's up matches the desired vertical
        // alignment. Returns identity for "inherit". Pure Z means it never disturbs
        // anchoredPosition -- the badge's screen corner is never independently held; it
        // simply goes wherever the root's own rotation puts it (_2.1_Marker_Orientation.md
        // v4: v3's badge_corner_mode "screen_fixed" was removed as unneeded complexity).
        public static Quaternion ResolveChildLocalRotation(
            string childVerticalAlignmentMode, Quaternion rootWorldRotation,
            Vector3 screenUpWorld, Vector3 upReference)
        {
            if (string.IsNullOrEmpty(childVerticalAlignmentMode) || childVerticalAlignmentMode == "inherit")
                return Quaternion.identity;

            Vector3 desiredUp = ResolveVerticalUp(childVerticalAlignmentMode, screenUpWorld, upReference);
            float angle = Vector3.SignedAngle(rootWorldRotation * Vector3.up, desiredUp, rootWorldRotation * Vector3.forward);
            return Quaternion.Euler(0f, 0f, angle);
        }

        // The signed roll of the root about its own forward, relative to screen up. Used by
        // MarkerView.ApplyLabelOffset's roll compensation (_2.1_Marker_Orientation.md
        // section 17) so displaced label offsets land in the right screen direction
        // regardless of the root's current vertical alignment.
        public static float RootRollDeg(Quaternion rootWorldRotation, Vector3 screenUpWorld)
        {
            return Vector3.SignedAngle(screenUpWorld, rootWorldRotation * Vector3.up, rootWorldRotation * Vector3.forward);
        }

        // Rotate a 2D offset by degrees (standard CCW rotation matrix). Used by
        // MarkerView.ApplyLabelOffset's roll compensation.
        public static Vector2 Rotate2D(Vector2 v, float degrees)
        {
            float rad = degrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);
            return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
        }

        // Clamp how far a rotation can pitch away from upReference's horizontal plane, so
        // always_facing_camera never tips a marker fully edge-on toward a nearby camera.
        public static Quaternion ClampPitch(Quaternion target, Vector3 upReference, float maxPitchDeg)
        {
            Vector3 up = upReference.normalized;
            Vector3 fwd = target * Vector3.forward;
            Vector3 flatFwd = Vector3.ProjectOnPlane(fwd, up);
            if (flatFwd.sqrMagnitude < 1e-12f) return target; // fwd is parallel to up; nothing sane to clamp toward
            flatFwd.Normalize();

            Vector3 axis = Vector3.Cross(up, flatFwd);
            if (axis.sqrMagnitude < 1e-12f) return target;
            axis.Normalize();

            float pitchDeg = Vector3.SignedAngle(flatFwd, fwd, axis);
            if (Mathf.Abs(pitchDeg) <= maxPitchDeg) return target;

            float clampedPitchDeg = Mathf.Sign(pitchDeg) * maxPitchDeg;
            Vector3 clampedFwd = Quaternion.AngleAxis(clampedPitchDeg, axis) * flatFwd;
            return Quaternion.LookRotation(clampedFwd, up);
        }

        // Whether enough time/angle has passed to re-resolve orientation this frame,
        // per OrientationSettings.update_mode's cost-control policy.
        public static bool ShouldUpdate(OrientationSettings s, float secondsSinceLastUpdate, float cameraAngleDeltaDeg)
        {
            switch (s.update_mode)
            {
                case "interval": return secondsSinceLastUpdate >= s.update_interval_s;
                case "on_camera_delta": return cameraAngleDeltaDeg >= s.camera_delta_deg;
                default: return true; // every_frame
            }
        }

        // Resolve the "world_up" vertical-alignment "up" vector from the wall's
        // up_reference choice. screen_up alignment uses ScreenUpWorld directly instead.
        public static Vector3 ResolveUpReference(OrientationSettings s, Transform spawnRoot)
        {
            switch (s.up_reference)
            {
                case "spawn_root":
                    return spawnRoot != null ? spawnRoot.up : Vector3.up;
                case "custom":
                    var custom = new Vector3(s.custom_up_x, s.custom_up_y, s.custom_up_z);
                    return custom.sqrMagnitude > 1e-12f ? custom.normalized : Vector3.up;
                default: // world_gravity
                    return Vector3.up;
            }
        }
    }
}
