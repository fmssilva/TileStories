using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories
{
    // Marks a screen's root with the app theme (_3.3 R12.1): ts-app + ts-theme-<key> + ts-mode-<mode>, the classes the generated theme
    // rule matches (Generated/Theme.<key>.uss, imported by TileStoriesTheme.tss = PanelSettings.themeUss), so every element under the
    // root inherits the theme's tokens. Every UIDocument host calls Apply on its root as it builds -- PoiCardHost, SearchUIHost,
    // ZoomControlView, the card gallery harness -- and C# reads the theme an element sits under through TryResolverFor. Switching mode
    // later = swapping one class on the root (no rebuild).
    public static class ThemeController
    {
        public const string AppClass = "ts-app";
        public const string ThemeClassPrefix = "ts-theme-";
        public const string ModeClassPrefix = "ts-mode-";
        // The framework's theme until a wall picks one (R12.4: _3.3 P4.5)
        public const string DefaultThemeKey = "default";
        // Themes load from Resources/TileStories/Themes/Theme.<key>
        public const string ResourcesFolder = "TileStories/Themes/";

        private static readonly Dictionary<string, ThemeAsset> Loaded = new();

        // The theme asset `key` names, or null when Resources holds none
        public static ThemeAsset Load(string key)
        {
            if (string.IsNullOrEmpty(key)) return null;
            if (Loaded.TryGetValue(key, out var theme) && theme != null) return theme;
            theme = Resources.Load<ThemeAsset>(ResourcesFolder + "Theme." + key);
            Loaded[key] = theme;
            return theme;
        }

        // Mark a screen's root with the default theme in its default mode
        public static void Apply(VisualElement root)
        {
            var theme = Load(DefaultThemeKey);
            Debug.Assert(theme != null, "[Theme] no Resources/" + ResourcesFolder + "Theme." + DefaultThemeKey + ": the screen gets no colours");
            if (theme == null) return;
            // - one theme and one mode at a time: a root marked before is re-marked, never doubly
            foreach (string old in root.GetClasses().Where(c => c.StartsWith(ThemeClassPrefix) || c.StartsWith(ModeClassPrefix)).ToList())
                root.RemoveFromClassList(old);
            root.AddToClassList(AppClass);
            root.AddToClassList(ThemeClassPrefix + theme.Key);
            root.AddToClassList(ModeClassPrefix + theme.DefaultMode);
        }

        // The resolver of the theme `from` sits under: its nearest .ts-app ancestor's theme and mode. False outside a marked root, or
        // when that root names a theme or mode that does not exist
        public static bool TryResolverFor(VisualElement from, out ThemeResolver resolver)
        {
            resolver = null;
            for (var e = from; e != null; e = e.parent)
            {
                if (!e.ClassListContains(AppClass)) continue;
                string themeKey = null, mode = null;
                foreach (string c in e.GetClasses())
                {
                    if (c.StartsWith(ThemeClassPrefix)) themeKey = c.Substring(ThemeClassPrefix.Length);
                    else if (c.StartsWith(ModeClassPrefix)) mode = c.Substring(ModeClassPrefix.Length);
                }
                var theme = Load(themeKey);
                resolver = theme != null && mode != null ? theme.ResolverFor(mode) : null;
                return resolver != null;
            }
            return false;
        }
    }
}
