using System.Collections.Generic;

namespace TileStories
{
    // Which mode the app shows (_3.3 R1: light by default, a visitor toggle). Pure, so a test calls it with any inputs: the visitor's own
    // pick on this device wins, then the wall's Default Mode, then the theme's own default -- each only when the theme has that mode.
    public static class ThemeModeRule
    {
        // The mode to show among `modes` (the theme's mode keys); "" when the theme has none
        public static string Shown(IReadOnlyList<string> modes, string visitorChoice, string wallDefault, string themeDefault)
        {
            if (modes == null || modes.Count == 0) return "";
            foreach (string pick in new[] { visitorChoice, wallDefault, themeDefault })
                if (!string.IsNullOrEmpty(pick) && Contains(modes, pick)) return pick;
            return modes[0];
        }

        private static bool Contains(IReadOnlyList<string> modes, string mode)
        {
            foreach (string m in modes)
                if (m == mode) return true;
            return false;
        }
    }
}
