using System.Collections.Generic;

namespace TileStories
{
    // category + member-count distribution for one fabricated cluster member set.
    // Sum of Count across CategoryPlan == ClusterGalleryEntry.MemberCount.
    public readonly struct CategoryCount
    {
        public CategoryCount(string category, int count)
        {
            Category = category;
            Count = count;
        }
        public string Category { get; }
        public int Count { get; }
    }

    // One row in the cluster gallery. Drives BOTH ClusterGalleryHarness (visual)
    // and ClusterGalleryTests (asserts) from a single data list, so the gallery
    // layout and the automated tests can never drift apart (Phase A recipe, 4.4).
    public sealed class ClusterGalleryEntry
    {
        public string Label;
        public string IconMode;                          // "pie_and_count" | "count_only" | "dominant_category"
        public List<CategoryCount> CategoryPlan;

        public int MemberCount
        {
            get { int n = 0; if (CategoryPlan != null) { foreach (var c in CategoryPlan) n += c.Count; } return n; }
        }
    }

    // Phase A cluster gallery definitions. "religious" resolves through CategoryPalette.Configure
    // -> ResolveIconKey("religious")="temple" (icon_key override on the entry below) ->
    // SpriteKeyLibrary.Get("temple") = IconTemple (verified present in IconLibrary.asset)
    // so dominant-category end-to-end icon resolve works.
    public static class ClusterGalleryDefinitions
    {
        public static readonly List<CategoryStyleEntry> Overrides = new()
        {
            new CategoryStyleEntry { category = "religious", color_hex = "#6B4226", icon_key = "temple" },
            new CategoryStyleEntry { category = "civic",     color_hex = "#8A5A2B" },
            new CategoryStyleEntry { category = "palace",    color_hex = "#5A6E8C" },
        };

        public static readonly List<ClusterGalleryEntry> Entries = new()
        {
            // pie_and_count, 1 category, 2 members -> 1 slice, fill 1.0
            new ClusterGalleryEntry {
                Label = "Pie / 2 members / 1 category",
                IconMode = "pie_and_count",
                CategoryPlan = new List<CategoryCount> { new CategoryCount("religious", 2) },
            },
            // pie_and_count, 2 categories, 5 members -> 2 slices (3/5, 2/5)
            new ClusterGalleryEntry {
                Label = "Pie / 5 members / 2 categories",
                IconMode = "pie_and_count",
                CategoryPlan = new List<CategoryCount>
                {
                    new CategoryCount("religious", 3),
                    new CategoryCount("civic", 2),
                },
            },
            // pie_and_count, 3 categories, 12 members -> 3 slices (5/12, 4/12, 3/12); biggest scale
            new ClusterGalleryEntry {
                Label = "Pie / 12 members / 3 categories",
                IconMode = "pie_and_count",
                CategoryPlan = new List<CategoryCount>
                {
                    new CategoryCount("religious", 5),
                    new CategoryCount("civic", 4),
                    new CategoryCount("palace", 3),
                },
            },
            // count_only, 5 members, 2 categories -> no pie, "+5"
            new ClusterGalleryEntry {
                Label = "Count Only / 5 members",
                IconMode = "count_only",
                CategoryPlan = new List<CategoryCount>
                {
                    new CategoryCount("religious", 3),
                    new CategoryCount("civic", 2),
                },
            },
            // dominant_category, 5 members, 2 categories -> no pie; DominantIcon active + sprite
            // (religious:3 beats civic:2 -> IconReligious -> SpriteKeyLibrary.Get -> non-null)
            new ClusterGalleryEntry {
                Label = "Dominant Category / 5 members",
                IconMode = "dominant_category",
                CategoryPlan = new List<CategoryCount>
                {
                    new CategoryCount("religious", 3),
                    new CategoryCount("civic", 2),
                },
            },
        };
    }
}