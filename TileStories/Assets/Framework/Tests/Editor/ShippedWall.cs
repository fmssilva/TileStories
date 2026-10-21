using System.IO;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // The shipped LivingRoom wall for EditMode tests that measure the FRAMEWORK's navigation (its rows, its elements, its defaults): the
    // wall's own content with card_settings.navigation at the framework default. Since _3.2 step 17b the demo wall ships its own preset;
    // a test of the framework never measures that choice (_3.2 8.2). The PlayMode twin is SearchSceneFixture's pin (ShippedNavigation).
    internal static class ShippedWall
    {
        public const string ConfigPath = "Assets/Apps/LivingRoom/config.json";

        // The shipped config, its navigation replaced by the framework default
        public static WallConfigData WithFrameworkNavigation()
        {
            var config = JsonUtility.FromJson<WallConfigData>(File.ReadAllText(ConfigPath));
            config.card_settings.navigation = new CardNavigationSettings();
            return config;
        }
    }
}
