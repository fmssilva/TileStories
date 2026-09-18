using System.Collections.Generic;

namespace TileStories
{
    // One row in the orientation gallery. Drives BOTH OrientationGalleryHarness (visual)
    // and OrientationGalleryTests (asserts) from a single data list (Phase A recipe, 4.4).
    public sealed class OrientationGalleryEntry
    {
        public string Label;                                       // shown under the row
        public string VerticalAlignmentMode = "world_up";           // world_up | screen_up
        public string FacingMode            = "always_facing_camera"; // wall_fixed | yaw_only | always_facing_camera
        public string FacingBasis           = "view_plane";
        public string UpReference           = "world_gravity";
        public string LabelMode             = "inherit";
        public string BadgeMode             = "inherit";
    }

    // Phase A orientation gallery definitions (_2.1_Marker_Orientation.md v4 section 12).
    // Built one at a time and confirmed before the next, never debugged together.
    public static class OrientationGalleryDefinitions
    {
        public static readonly List<OrientationGalleryEntry> Entries = new()
        {
            new OrientationGalleryEntry { Label = "1. world_up / always_facing_camera (default)", VerticalAlignmentMode = "world_up", FacingMode = "always_facing_camera" },
            new OrientationGalleryEntry { Label = "2. screen_up / always_facing_camera", VerticalAlignmentMode = "screen_up", FacingMode = "always_facing_camera" },
            new OrientationGalleryEntry { Label = "3. world_up / always_facing_camera / camera_position basis", VerticalAlignmentMode = "world_up", FacingMode = "always_facing_camera", FacingBasis = "camera_position" },
            new OrientationGalleryEntry { Label = "4. world_up / wall_fixed", VerticalAlignmentMode = "world_up", FacingMode = "wall_fixed" },
            new OrientationGalleryEntry { Label = "5. world_up / yaw_only", VerticalAlignmentMode = "world_up", FacingMode = "yaw_only" },
            new OrientationGalleryEntry { Label = "6. screen_up / wall_fixed", VerticalAlignmentMode = "screen_up", FacingMode = "wall_fixed" },
            new OrientationGalleryEntry { Label = "7. world_up / always_facing_camera / world_gravity via spawn_root up_reference", VerticalAlignmentMode = "world_up", FacingMode = "always_facing_camera", UpReference = "spawn_root" },
            new OrientationGalleryEntry { Label = "8. world_up + label screen_up (headline combo)", VerticalAlignmentMode = "world_up", FacingMode = "always_facing_camera", LabelMode = "screen_up" },
            new OrientationGalleryEntry { Label = "9. screen_up + label world_up (reverse)", VerticalAlignmentMode = "screen_up", FacingMode = "always_facing_camera", LabelMode = "world_up" },
            new OrientationGalleryEntry { Label = "10. world_up + badge screen_up", VerticalAlignmentMode = "world_up", FacingMode = "always_facing_camera", BadgeMode = "screen_up" },
            new OrientationGalleryEntry { Label = "11. world_up / yaw_only, camera rolled (regression: roll must not affect yaw_only)", VerticalAlignmentMode = "world_up", FacingMode = "yaw_only" },
        };
    }
}
