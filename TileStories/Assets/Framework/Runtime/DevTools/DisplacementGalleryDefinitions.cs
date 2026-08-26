using System.Collections.Generic;
using UnityEngine;

namespace TileStories
{
    // Viewing angle definition for gallery testing
    public enum DisplacementViewingAngle
    {
        Normal,
        Shallow
    }

    // One row in the displacement gallery. Drives BOTH DisplacementGalleryHarness (visual)
    // and DisplacementGalleryTests (asserts) from a single data list (Phase A recipe, 4.4).
    public sealed class DisplacementGalleryEntry
    {
        public string Label;
        public int GroupSize;                                // 2, 3, 5+
        public DisplacementViewingAngle ViewingAngle;        // Normal vs Shallow
        public string DisplaceTarget = "label_only";         // "label_only" (Block 2) | "marker" | "both" (Block 4)
        public string Algorithm = "fixed_axis";              // "fixed_axis" | "candidate_position" | "force_directed"
        public bool LeaderLinesEnabled = false;               // true = draw leader line (spec Section 6)
        public string LeaderLineStyle = "straight";           // "straight" | "dashed" | "elbow"
    }

    // Phase A displacement gallery definitions.
    public static class DisplacementGalleryDefinitions
    {
        public static readonly List<CategoryStyleEntry> Overrides = new()
        {
            new CategoryStyleEntry { category = "religious", color_hex = "#6B4226", icon_key = "temple" },
            new CategoryStyleEntry { category = "civic",     color_hex = "#8A5A2B" },
            new CategoryStyleEntry { category = "palace",    color_hex = "#5A6E8C" },
        };

        public static readonly List<DisplacementGalleryEntry> Entries = new()
        {
            // 2 members / Normal angle
            new DisplacementGalleryEntry
            {
                Label = "2 Members / Normal Angle / Fixed Axis",
                GroupSize = 2,
                ViewingAngle = DisplacementViewingAngle.Normal,
                DisplaceTarget = "label_only",
                Algorithm = "fixed_axis"
            },
            // 3 members / Normal angle
            new DisplacementGalleryEntry
            {
                Label = "3 Members / Normal Angle / Fixed Axis",
                GroupSize = 3,
                ViewingAngle = DisplacementViewingAngle.Normal,
                DisplaceTarget = "label_only",
                Algorithm = "fixed_axis"
            },
            // 5 members / Normal angle
            new DisplacementGalleryEntry
            {
                Label = "5 Members / Normal Angle / Fixed Axis",
                GroupSize = 5,
                ViewingAngle = DisplacementViewingAngle.Normal,
                DisplaceTarget = "label_only",
                Algorithm = "fixed_axis"
            },
            // 3 members / Shallow angle (grazing camera)
            new DisplacementGalleryEntry
            {
                Label = "3 Members / Shallow Angle / Fixed Axis",
                GroupSize = 3,
                ViewingAngle = DisplacementViewingAngle.Shallow,
                DisplaceTarget = "label_only",
                Algorithm = "fixed_axis"
            },
            // 2 members / Normal angle / Candidate Position
            new DisplacementGalleryEntry
            {
                Label = "2 Members / Normal Angle / Candidate Position",
                GroupSize = 2,
                ViewingAngle = DisplacementViewingAngle.Normal,
                DisplaceTarget = "label_only",
                Algorithm = "candidate_position"
            },
            // 3 members / Normal angle / Candidate Position
            new DisplacementGalleryEntry
            {
                Label = "3 Members / Normal Angle / Candidate Position",
                GroupSize = 3,
                ViewingAngle = DisplacementViewingAngle.Normal,
                DisplaceTarget = "label_only",
                Algorithm = "candidate_position"
            },
            // 2 members / Normal angle / Force Directed
            new DisplacementGalleryEntry
            {
                Label = "2 Members / Normal Angle / Force Directed",
                GroupSize = 2,
                ViewingAngle = DisplacementViewingAngle.Normal,
                DisplaceTarget = "label_only",
                Algorithm = "force_directed"
            },
            // 3 members / Normal angle / Force Directed
            new DisplacementGalleryEntry
            {
                Label = "3 Members / Normal Angle / Force Directed",
                GroupSize = 3,
                ViewingAngle = DisplacementViewingAngle.Normal,
                DisplaceTarget = "label_only",
                Algorithm = "force_directed"
            },
            // 3 members / Normal / Force Directed / Both / Leader Lines (Block 5)
            new DisplacementGalleryEntry
            {
                Label = "3 Members / Normal / Force Directed / Both / Leader Lines",
                GroupSize = 3,
                ViewingAngle = DisplacementViewingAngle.Normal,
                DisplaceTarget = "both",
                Algorithm = "force_directed",
                LeaderLinesEnabled = true,
                LeaderLineStyle = "straight"
            },
            // 3 members / Shallow / Force Directed / Marker / Leader Lines Elbow (Block 5)
            new DisplacementGalleryEntry
            {
                Label = "3 Members / Shallow / Force Directed / Marker / Leader Lines Elbow",
                GroupSize = 3,
                ViewingAngle = DisplacementViewingAngle.Shallow,
                DisplaceTarget = "marker",
                Algorithm = "force_directed",
                LeaderLinesEnabled = true,
                LeaderLineStyle = "elbow"
            },
            // 3 members / Normal / Fixed Axis / Marker (Block 4)
            new DisplacementGalleryEntry
            {
                Label = "3 Members / Normal / Fixed Axis / Marker",
                GroupSize = 3,
                ViewingAngle = DisplacementViewingAngle.Normal,
                DisplaceTarget = "marker",
                Algorithm = "fixed_axis"
            },
            // 3 members / Normal / Force Directed / Both (Block 4)
            new DisplacementGalleryEntry
            {
                Label = "3 Members / Normal / Force Directed / Both",
                GroupSize = 3,
                ViewingAngle = DisplacementViewingAngle.Normal,
                DisplaceTarget = "both",
                Algorithm = "force_directed"
            },
            // 3 members / Shallow / Force Directed / Marker (Block 4)
            new DisplacementGalleryEntry
            {
                Label = "3 Members / Shallow / Force Directed / Marker",
                GroupSize = 3,
                ViewingAngle = DisplacementViewingAngle.Shallow,
                DisplaceTarget = "marker",
                Algorithm = "force_directed"
            },
            // 3 members / Shallow / Force Directed / Both (Block 4)
            new DisplacementGalleryEntry
            {
                Label = "3 Members / Shallow / Force Directed / Both",
                GroupSize = 3,
                ViewingAngle = DisplacementViewingAngle.Shallow,
                DisplaceTarget = "both",
                Algorithm = "force_directed"
            },
        };
    }
}
