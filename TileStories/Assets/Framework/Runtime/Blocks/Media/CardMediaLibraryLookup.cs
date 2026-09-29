using UnityEngine;

namespace TileStories
{
    // Where the two CardMediaLibrary instances a "default:<key>" value can resolve from come from (_3.1 step 13),
    // same pattern as the marker icon / font libraries: the Framework's own shipped asset at a fixed Resources
    // path, and a wall's own optional override loaded from card_settings.default_media_library_resources_path.
    public static class CardMediaLibraryLookup
    {
        // Resources-relative path of the Framework's own default media library asset
        public const string FrameworkResourcesPath = "TileStories/CardMediaLibrary";

        public static CardMediaLibrary Framework => Resources.Load<CardMediaLibrary>(FrameworkResourcesPath);

        public static CardMediaLibrary WallFrom(string resourcesPath) =>
            string.IsNullOrWhiteSpace(resourcesPath) ? null : Resources.Load<CardMediaLibrary>(resourcesPath.Trim().Trim('/'));

        // Wall's own entry first, else the Framework's; null when neither library has this key/kind
        public static Object Resolve(string key, MediaKind kind, CardMediaLibrary wallLibrary, CardMediaLibrary frameworkLibrary)
        {
            var fromWall = wallLibrary != null ? wallLibrary.Get(key, kind) : null;
            if (fromWall != null) return fromWall;
            return frameworkLibrary != null ? frameworkLibrary.Get(key, kind) : null;
        }
    }
}
