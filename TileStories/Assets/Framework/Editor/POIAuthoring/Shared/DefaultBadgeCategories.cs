// DefaultBadgeCategories.cs
//
// Returns the four building damage levels the authoring tool seeds into a
// wall's config when it has no badge_categories yet. Editor-only data --
// runtime reads whatever config.json provides, never these defaults.
//
// Sibling factory to DefaultCategoryStyles.cs (see _5.1.2_Default_Icons.md
// section 1). Shared pattern by naming convention, not a generic abstraction,
// because badge rows and category rows use different entry types.

using System.Collections.Generic;

namespace TileStories.Editor
{
    public static class DefaultBadgeCategories
    {
        // The four building damage levels with their icon keys and colours.
        // icon_key values match entries registered in IconLibrary.asset so the
        // editor table previews resolve to real sprites. color_hex is used
        // directly as the badge background tint at runtime (BadgeCategoryPalette.Configure).
        public static List<BadgeCategoryEntry> Create()
        {
            return new List<BadgeCategoryEntry>
            {
                new BadgeCategoryEntry
                {
                    key = "intact",
                    label = "Intact",
                    icon_key = "IconIntact",
                    color_hex = "#22C55E",
                    details = "No visible damage -- building appears complete and maintained."
                },
                new BadgeCategoryEntry
                {
                    key = "partial_damage",
                    label = "Partial Damage",
                    icon_key = "IconPartialDamage",
                    color_hex = "#F97316",
                    details = "Visible damage but structure largely standing."
                },
                new BadgeCategoryEntry
                {
                    key = "destroyed",
                    label = "Destroyed",
                    icon_key = "IconDestroyed",
                    color_hex = "#991B1B",
                    details = "Severely damaged or collapsed -- little original structure remains."
                },
                new BadgeCategoryEntry
                {
                    key = "unknown_damage",
                    label = "Unknown Damage",
                    icon_key = "IconUnknownDamage",
                    color_hex = "#71717A",
                    details = "Damage state not assessed or data unavailable."
                },
            };
        }
    }
}
