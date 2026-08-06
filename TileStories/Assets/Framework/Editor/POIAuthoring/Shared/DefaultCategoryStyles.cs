// DefaultCategoryStyles.cs
//
// Returns the six heritage category defaults the authoring tool seeds into a
// wall's config when it has no category_styles yet. Editor-only data — runtime
// reads whatever config.json provides, never these defaults.
//
// Extracted from the duplicated seeding in LoadConfig (ConfigFileIO.cs) and
// DrawMarkerGlobalSection (GlobalScene.cs) so the defaults live in one place
// and are directly unit-testable. Follows the sibling-factory convention
// documented in _5.3_Defaults.md: future badge/outline defaults will be
// separate sibling files (DefaultBadgeCategories.cs, DefaultOutlineLevels.cs),
// not a generic abstraction, since each table uses a different entry type.

using System.Collections.Generic;

namespace TileStories.Editor
{
    public static class DefaultCategoryStyles
    {
        // The six heritage building categories with their icon keys and colours.
        // icon_key values match entries registered in IconLibrary.asset so the
        // editor table previews resolve to real sprites.
        public static List<CategoryStyleEntry> Create()
        {
            return new List<CategoryStyleEntry>
            {
                new CategoryStyleEntry
                {
                    category = "royal_government",
                    icon_key = "IconRoyal&Government",
                    color_hex = "#D97706",
                    details = "Royal & Government"
                },
                new CategoryStyleEntry
                {
                    category = "religious",
                    icon_key = "IconReligious",
                    color_hex = "#7C3AED",
                    details = "Sacred Architecture"
                },
                new CategoryStyleEntry
                {
                    category = "military",
                    icon_key = "IconMilitary",
                    color_hex = "#DC2626",
                    details = "Defense & Fortifications"
                },
                new CategoryStyleEntry
                {
                    category = "residential",
                    icon_key = "IconNobel&PrivateResidence",
                    color_hex = "#DB2777",
                    details = "Noble & Private Housing"
                },
                new CategoryStyleEntry
                {
                    category = "economic",
                    icon_key = "IconIndustry&Trade",
                    color_hex = "#059669",
                    details = "Trade, Industry & Harbor"
                },
                new CategoryStyleEntry
                {
                    category = "infrastructure",
                    icon_key = "IconInfrastructures",
                    color_hex = "#0284C7",
                    details = "Public Utilities & Works"
                },
            };
        }
    }
}
