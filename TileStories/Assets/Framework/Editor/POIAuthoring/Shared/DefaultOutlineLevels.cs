// DefaultOutlineLevels.cs
//
// Returns the four destruction-status outline levels the authoring tool seeds into
// a wall's config when it has no outline_levels yet. Editor-only data --
// runtime reads whatever config.json provides, never these defaults.
//
// Sibling factory to DefaultCategoryStyles.cs and DefaultBadgeCategories.cs
// (see _5.3_Default_Icons.md section 1). Shared pattern by naming convention,
// not a generic abstraction, because outline rows use a different entry type.

using System.Collections.Generic;

namespace TileStories.Editor
{
    // Four heritage outline types: three destruction levels (intact, partial
    // damage, destroyed) plus a semantic "unknown" state. Each line_style matches
    // an entry registered in IconLibrary.asset so the editor table previews resolve
    // to real ring sprites. color_hex is left empty for the three status levels so
    // runtime falls back to StatusRamp colors; "unknown" carries an explicit grey so
    // it reads as a different kind of signal ("we don't know") not a point on the
    // destruction ramp.
    public static class DefaultOutlineLevels
    {
        public static List<OutlineLevelEntry> Create()
        {
            return new List<OutlineLevelEntry>
            {
                new OutlineLevelEntry
                {
                    key = "intact",
                    label = "Intact",
                    pct = 0f,
                    line_style = "solid",
                    color_hex = string.Empty,
                    ring_width = 3.2f,
                    details = "No visible damage -- structure appears complete and maintained."
                },
                new OutlineLevelEntry
                {
                    key = "partial_damage",
                    label = "Partial Damage",
                    pct = 20f,
                    line_style = "dash_long",
                    color_hex = string.Empty,
                    ring_width = 2.8f,
                    details = "Minor to moderate damage -- small cracks or chips."
                },
                new OutlineLevelEntry
                {
                    key = "destroyed",
                    label = "Destroyed",
                    pct = 100f,
                    line_style = "dash_short",
                    color_hex = string.Empty,
                    ring_width = 2.0f,
                    details = "Severely damaged or collapsed -- little original structure remains."
                },
                new OutlineLevelEntry
                {
                    key = "unknown",
                    label = "Unknown",
                    pct = 100f,
                    line_style = "dotted",
                    color_hex = "#71717A",
                    ring_width = 1.8f,
                    details = "Damage state not assessed or data unavailable."
                },
            };
        }
    }
}
