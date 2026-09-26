namespace TileStories
{
    // Config string <-> ViewMode (select_filter_search.results.default_view); unknown = List
    public static class ViewModeParser
    {
        public static ViewMode Parse(string mode) => mode switch
        {
            SelectFilterSearchOptions.ViewMinimap => ViewMode.Minimap,
            SelectFilterSearchOptions.ViewCameraHighlight => ViewMode.CameraHighlight,
            _ => ViewMode.List,
        };

        public static string ToString(ViewMode mode) => mode switch
        {
            ViewMode.Minimap => SelectFilterSearchOptions.ViewMinimap,
            ViewMode.CameraHighlight => SelectFilterSearchOptions.ViewCameraHighlight,
            _ => SelectFilterSearchOptions.ViewList,
        };
    }
}
