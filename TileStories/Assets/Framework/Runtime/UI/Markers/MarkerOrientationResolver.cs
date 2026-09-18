using UnityEngine;

namespace TileStories
{
    // Result of one orientation resolution call (_2.1_Marker_Orientation.md section 8).
    // The resolver is stateless: everything it needs to remember between frames is
    // passed in (previousSnappedRollDeg) and handed back here, so the caller (the
    // MonoBehaviour) owns all state and the resolver stays deterministic and testable.
    public struct OrientationResult
    {
        public Quaternion Rotation;      // the world rotation to write
        public float SnappedRollDeg;     // feed back as previousSnappedRollDeg next frame
        public bool Resolved;            // false = degenerate input; caller keeps its previous rotation
    }

    // Pure orientation math for markers, labels, badges and clusters (_2.1_Marker_Orientation.md).
    // Stateless and static so every decision here is Tier-0 testable with no scene running.
    public static class MarkerOrientationResolver
    {
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

        // Resolve the world rotation for a marker root. wall_fixed and none ignore the
        // facing/up computation entirely and return the parent-relative rotation directly;
        // every other mode picks a forward + up basis, guards against a degenerate
        // LookRotation, then applies roll snapping and pitch clamp on top.
        public static OrientationResult ResolveRootRotation(
            OrientationSettings settings,
            string modeOverride,
            Vector3 markerWorldPos, Vector3 camPos, Vector3 camForward,
            Vector3 screenUpWorld, Vector3 upReference,
            Quaternion parentRotation, Quaternion authoredLocalRotation,
            float previousSnappedRollDeg,
            ScreenOrientation screenOrientation)
        {
            string mode = string.IsNullOrEmpty(modeOverride) ? settings.marker_orientation_mode : modeOverride;

            if (mode == "wall_fixed")
            {
                return new OrientationResult
                {
                    Rotation = parentRotation * authoredLocalRotation,
                    SnappedRollDeg = previousSnappedRollDeg,
                    Resolved = true
                };
            }

            if (mode == "none")
            {
                return new OrientationResult
                {
                    Rotation = parentRotation,
                    SnappedRollDeg = previousSnappedRollDeg,
                    Resolved = true
                };
            }

            Vector3 fwd;
            Vector3 up;

            if (mode == "yaw_only")
            {
                up = upReference;
                fwd = Vector3.ProjectOnPlane(camPos - markerWorldPos, up);
            }
            else // screen_aligned | world_up
            {
                up = mode == "world_up" ? upReference : screenUpWorld;
                fwd = settings.facing_basis == "camera_position"
                    ? markerWorldPos - camPos
                    : camForward;
            }

            // Degenerate LookRotation guard: view direction nearly parallel to the up
            // reference (e.g. looking straight down at a world_up marker) would produce
            // garbage. Keep the caller's previous rotation instead of snapping to identity.
            if (fwd.sqrMagnitude < 1e-12f || up.sqrMagnitude < 1e-12f ||
                Mathf.Abs(Vector3.Dot(fwd.normalized, up.normalized)) > 0.999f)
            {
                return new OrientationResult { Rotation = parentRotation, SnappedRollDeg = previousSnappedRollDeg, Resolved = false };
            }

            Quaternion rotation = Quaternion.LookRotation(fwd, up);

            float measuredRollDeg = RootRollDeg(rotation, screenUpWorld);
            float snappedRollDeg = SnapRollDeg(measuredRollDeg, settings.roll_snap_mode, settings.roll_snap_hysteresis_deg, previousSnappedRollDeg, screenOrientation);
            float rollCorrectionDeg = snappedRollDeg - measuredRollDeg;
            if (Mathf.Abs(rollCorrectionDeg) > 1e-4f)
            {
                rotation = Quaternion.AngleAxis(rollCorrectionDeg, fwd.normalized) * rotation;
            }

            if (settings.clamp_pitch_enabled)
            {
                rotation = ClampPitch(rotation, up, settings.max_pitch_deg);
            }

            return new OrientationResult { Rotation = rotation, SnappedRollDeg = snappedRollDeg, Resolved = true };
        }

        // Child LOCAL rotation (pure Z) so the child's up matches the desired basis.
        // Returns identity for "inherit". Pure Z means it never disturbs anchoredPosition.
        public static Quaternion ResolveChildLocalRotation(
            string childMode, Quaternion rootWorldRotation,
            Vector3 screenUpWorld, Vector3 upReference)
        {
            if (string.IsNullOrEmpty(childMode) || childMode == "inherit") return Quaternion.identity;

            Vector3 desiredUp = childMode == "screen_up" ? screenUpWorld : upReference;
            float angle = Vector3.SignedAngle(rootWorldRotation * Vector3.up, desiredUp, rootWorldRotation * Vector3.forward);
            return Quaternion.Euler(0f, 0f, angle);
        }

        // The signed roll of the root about its own forward, relative to screen up.
        // Shared by ResolveChildLocalRotation's callers, the badge corner hold and Block 5.
        public static float RootRollDeg(Quaternion rootWorldRotation, Vector3 screenUpWorld)
        {
            return Vector3.SignedAngle(screenUpWorld, rootWorldRotation * Vector3.up, rootWorldRotation * Vector3.forward);
        }

        // Rotate a 2D offset by degrees (standard CCW rotation matrix). Used for the
        // badge corner hold and Block 5's label offset compensation.
        public static Vector2 Rotate2D(Vector2 v, float degrees)
        {
            float rad = degrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);
            return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
        }

        // Snap a measured roll to the nearest quarter turn (or to the OS's committed
        // screen orientation), with hysteresis so the snap cannot flicker at a boundary.
        public static float SnapRollDeg(
            float rollDeg, string snapMode, float hysteresisDeg,
            float previousSnappedDeg, ScreenOrientation screenOrientation)
        {
            if (string.IsNullOrEmpty(snapMode) || snapMode == "none") return rollDeg;

            if (snapMode == "screen_orientation")
            {
                float? mapped = MapScreenOrientationDeg(screenOrientation);
                if (mapped.HasValue) return mapped.Value;
                // AutoRotation / unknown: fall back to quarter_turns behaviour on the measured roll.
            }

            float nearestQuarter = Mathf.Round(rollDeg / 90f) * 90f;
            nearestQuarter = ((nearestQuarter % 360f) + 360f) % 360f;

            float distanceFromPrevious = Mathf.Abs(Mathf.DeltaAngle(rollDeg, previousSnappedDeg));
            if (distanceFromPrevious <= 45f + hysteresisDeg) return previousSnappedDeg;

            return nearestQuarter;
        }

        private static float? MapScreenOrientationDeg(ScreenOrientation orientation)
        {
            switch (orientation)
            {
                case ScreenOrientation.Portrait: return 0f;
                case ScreenOrientation.LandscapeLeft: return 90f;
                case ScreenOrientation.PortraitUpsideDown: return 180f;
                case ScreenOrientation.LandscapeRight: return 270f;
                default: return null;
            }
        }

        // Clamp how far the marker can pitch up/down away from its up reference's
        // horizontal plane, so a marker never tips fully edge-on when a visitor looks
        // steeply up a tall wall.
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

        // Resolve the world_up / yaw_only "up" vector from the wall's up_reference choice.
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
