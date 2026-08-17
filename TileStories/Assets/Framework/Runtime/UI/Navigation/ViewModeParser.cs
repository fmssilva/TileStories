namespace TileStories
{
    // Pure logic for parsing and serializing view mode strings.
    // Extracted from ViewModeControl so it can be Tier-0 tested without a scene.
    // (spec _2.6 section 10)
    public static class ViewModeParser
    {
        // Parse a config string into a ViewMode enum.
        // Unknown/null values default to List.
        public static ViewModeControl.ViewMode Parse(string mode)
        {
            return mode?.ToLowerInvariant() switch
            {
                "list" => ViewModeControl.ViewMode.List,
                "minimap" => ViewModeControl.ViewMode.Minimap,
                "camera_highlight" => ViewModeControl.ViewMode.CameraHighlight,
                _ => ViewModeControl.ViewMode.List,
            };
        }

        // Convert a ViewMode enum to its string representation for persistence.
        public static string ToString(ViewModeControl.ViewMode mode)
        {
            return mode switch
            {
                ViewModeControl.ViewMode.List => "list",
                ViewModeControl.ViewMode.Minimap => "minimap",
                ViewModeControl.ViewMode.CameraHighlight => "camera_highlight",
                _ => "list",
            };
        }
    }
}
