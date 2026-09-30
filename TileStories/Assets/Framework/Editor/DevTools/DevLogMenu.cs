using UnityEditor;

namespace TileStories
{
    // TileStories > Detail Logs: one tick per LogDomain, remembered per machine (EditorPrefs), applied to DevLog.Enabled
    // after every domain reload (entering Play Mode included), so a ticked domain logs its detail lines in the Editor.
    // Test runs start from whatever is ticked: leave everything off while running the suites (the default).
    [InitializeOnLoad]
    internal static class DevLogMenu
    {
        private const string PrefKey = "TileStories.DetailLogDomains";
        private const string Root = "TileStories/Detail Logs/";

        static DevLogMenu() => DevLog.Enabled = (LogDomain)EditorPrefs.GetInt(PrefKey, 0);

        [MenuItem(Root + "Wall")] private static void Wall() => Toggle(LogDomain.Wall);
        [MenuItem(Root + "Wall", true)] private static bool WallCheck() => Check(Root + "Wall", LogDomain.Wall);
        [MenuItem(Root + "Markers")] private static void Markers() => Toggle(LogDomain.Markers);
        [MenuItem(Root + "Markers", true)] private static bool MarkersCheck() => Check(Root + "Markers", LogDomain.Markers);
        [MenuItem(Root + "LOD")] private static void Lod() => Toggle(LogDomain.Lod);
        [MenuItem(Root + "LOD", true)] private static bool LodCheck() => Check(Root + "LOD", LogDomain.Lod);
        [MenuItem(Root + "Search")] private static void Search() => Toggle(LogDomain.Search);
        [MenuItem(Root + "Search", true)] private static bool SearchCheck() => Check(Root + "Search", LogDomain.Search);
        [MenuItem(Root + "Card")] private static void Card() => Toggle(LogDomain.Card);
        [MenuItem(Root + "Card", true)] private static bool CardCheck() => Check(Root + "Card", LogDomain.Card);
        [MenuItem(Root + "Tracking")] private static void Tracking() => Toggle(LogDomain.Tracking);
        [MenuItem(Root + "Tracking", true)] private static bool TrackingCheck() => Check(Root + "Tracking", LogDomain.Tracking);

        [MenuItem(Root + "All Off", priority = 100)]
        private static void AllOff() => Set(LogDomain.None);

        // Flip one domain and remember the result
        internal static void Toggle(LogDomain domain) => Set(DevLog.Enabled ^ domain);

        // Apply a whole mask and remember it
        internal static void Set(LogDomain mask)
        {
            DevLog.Enabled = mask;
            EditorPrefs.SetInt(PrefKey, (int)mask);
        }

        // Validate callback: draw the tick next to the item (always enabled)
        private static bool Check(string path, LogDomain domain)
        {
            Menu.SetChecked(path, DevLog.IsOn(domain));
            return true;
        }
    }
}
