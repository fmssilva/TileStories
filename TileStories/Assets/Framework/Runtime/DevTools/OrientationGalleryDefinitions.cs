using System.Collections.Generic;

namespace TileStories
{
    // One row in the orientation gallery. Drives BOTH OrientationGalleryHarness (visual)
    // and OrientationGalleryTests (asserts) from a single data list (Phase A recipe, 4.4).
    public sealed class OrientationGalleryEntry
    {
        public string Label;                                   // shown under the row
        public string MarkerMode      = "screen_aligned";
        public string FacingBasis     = "view_plane";
        public string UpReference     = "world_gravity";
        public string LabelMode       = "inherit";
        public string BadgeMode       = "inherit";
        public string BadgeCornerMode = "inherit";
        public string RollSnapMode    = "none";
        public bool   ClampPitch      = false;
    }

    // Phase A orientation gallery definitions (_2.1_Marker_Orientation.md section 12).
    // Built one at a time and confirmed before the next, never debugged together.
    public static class OrientationGalleryDefinitions
    {
        public static readonly List<OrientationGalleryEntry> Entries = new()
        {
            new OrientationGalleryEntry { Label = "1. screen_aligned / defaults", MarkerMode = "screen_aligned" },
            new OrientationGalleryEntry { Label = "2. screen_aligned / camera_position", MarkerMode = "screen_aligned", FacingBasis = "camera_position" },
            new OrientationGalleryEntry { Label = "3. world_up / view_plane / world_gravity", MarkerMode = "world_up", FacingBasis = "view_plane", UpReference = "world_gravity" },
            new OrientationGalleryEntry { Label = "4. world_up / camera_position / world_gravity", MarkerMode = "world_up", FacingBasis = "camera_position", UpReference = "world_gravity" },
            new OrientationGalleryEntry { Label = "5. world_up / view_plane / spawn_root", MarkerMode = "world_up", FacingBasis = "view_plane", UpReference = "spawn_root" },
            new OrientationGalleryEntry { Label = "6. yaw_only / world_gravity", MarkerMode = "yaw_only", UpReference = "world_gravity" },
            new OrientationGalleryEntry { Label = "7. yaw_only / clamp pitch on", MarkerMode = "yaw_only", UpReference = "world_gravity", ClampPitch = true },
            new OrientationGalleryEntry { Label = "8. wall_fixed", MarkerMode = "wall_fixed" },
            new OrientationGalleryEntry { Label = "9. none", MarkerMode = "none" },
            new OrientationGalleryEntry { Label = "10. screen_aligned + label world_up", MarkerMode = "screen_aligned", LabelMode = "world_up" },
            new OrientationGalleryEntry { Label = "11. world_up + label screen_up", MarkerMode = "world_up", LabelMode = "screen_up" },
            new OrientationGalleryEntry { Label = "12. screen_aligned + label screen_up", MarkerMode = "screen_aligned", LabelMode = "screen_up" },
            new OrientationGalleryEntry { Label = "13. screen_aligned + badge world_up", MarkerMode = "screen_aligned", BadgeMode = "world_up" },
            new OrientationGalleryEntry { Label = "14. world_up + badge corner screen_fixed", MarkerMode = "world_up", BadgeCornerMode = "screen_fixed" },
            new OrientationGalleryEntry { Label = "15. world_up + roll snap quarter_turns", MarkerMode = "world_up", RollSnapMode = "quarter_turns" },
            new OrientationGalleryEntry { Label = "16. world_up + roll snap screen_orientation", MarkerMode = "world_up", RollSnapMode = "screen_orientation" },
        };
    }
}
