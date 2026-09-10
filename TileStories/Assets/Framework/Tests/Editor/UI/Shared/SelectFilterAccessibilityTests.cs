using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // Tier-0.5 accessibility checks for the Select/Filter/Search overlay surfaces
    // (spec _2.7 entry 2.6-af, closing its two remaining gaps):
    //   1. filter-tray facet chips meet the 44x44px minimum tap target;
    //   2. results-list / filter-tray text meets WCAG AA against their AUTHORED
    //      background (UIPalette.SurfaceDark) -- the design-token decision that
    //      removed the "theme-resolved background, no deterministic pair" blocker.
    // Every assert reads the REAL authored style values off built VisualElements
    // (no hardcoded assumptions), matching the UIAccessibilityTests pattern.
    public class SelectFilterAccessibilityTests
    {
        private static WallConfigData MakeConfig()
        {
            return new WallConfigData
            {
                wall_id = "a11y_wall",
                wall_name = "Accessibility Test Wall",
                category_styles = new System.Collections.Generic.List<CategoryStyleEntry>
                {
                    new CategoryStyleEntry { category = "religious" }
                },
                badge_categories = new System.Collections.Generic.List<BadgeCategoryEntry>
                {
                    new BadgeCategoryEntry { key = "damage" }
                },
                outline_levels = new System.Collections.Generic.List<OutlineLevelEntry>
                {
                    new OutlineLevelEntry { key = "intact", pct = 0f }
                },
                hierarchy_levels = new System.Collections.Generic.List<HierarchyLevelEntry>
                {
                    new HierarchyLevelEntry { key = "level_1" }
                },
                pois = new System.Collections.Generic.List<POIData>
                {
                    new POIData { id = "poi_a", name = "POI A", category = "religious", x_norm = 0.25f, y_norm = 0.25f },
                    new POIData { id = "poi_b", name = "POI B", category = "religious", x_norm = 0.75f, y_norm = 0.75f }
                }
            };
        }

        // Composite a translucent overlay color over an opaque base color
        // (per-channel alpha lerp). Used to evaluate WCAG contrast for the
        // 0.85-alpha SurfaceDark against both worst-case camera-feed bases.
        private static Color CompositeOver(Color overlay, Color baseColor)
        {
            float a = overlay.a;
            return new Color(
                overlay.r * a + baseColor.r * (1f - a),
                overlay.g * a + baseColor.g * (1f - a),
                overlay.b * a + baseColor.b * (1f - a));
        }

        private static void AssertColorClose(Color actual, Color expected, string context)
        {
            Assert.IsTrue(
                Mathf.Abs(actual.r - expected.r) < 0.001f &&
                Mathf.Abs(actual.g - expected.g) < 0.001f &&
                Mathf.Abs(actual.b - expected.b) < 0.001f &&
                Mathf.Abs(actual.a - expected.a) < 0.001f,
                $"{context}: expected {expected}, was {actual}");
        }

        [Test]
        public void FilterChips_AllFacetToggles_AuthorMinTapTarget44()
        {
            var config = MakeConfig();
            var go = new GameObject("filter-tray-a11y-test");
            try
            {
                var view = go.AddComponent<TileStories.FilterTrayView>();
                var root = new VisualElement();
                view.CreateUI(root);
                view.Initialize(config); // tray container already exists -> facets render

                var toggles = root.Query<Toggle>()
                    .Where(t => t.name != null && t.name.StartsWith("facet-toggle-"))
                    .ToList();

                // One chip per non-empty taxonomy table in the config.
                Assert.GreaterOrEqual(toggles.Count, 4,
                    "expected at least one facet chip per taxonomy table (category/badge/outline/hierarchy)");

                foreach (var toggle in toggles)
                {
                    float minHeight = toggle.style.minHeight.value.value;
                    Assert.GreaterOrEqual(minHeight, 44f,
                        $"facet chip '{toggle.name}' authored min-height {minHeight}px is below the 44px WCAG minimum");
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void FilterTray_ContainerBackground_IsAuthoredSurfaceDark()
        {
            var go = new GameObject("filter-tray-bg-test");
            try
            {
                var view = go.AddComponent<TileStories.FilterTrayView>();
                var root = new VisualElement();
                view.CreateUI(root);

                var tray = root.Q<VisualElement>("filter-tray-container");
                Assert.IsNotNull(tray, "filter-tray-container missing from built UI");
                AssertColorClose(tray.style.backgroundColor.value, TileStories.UIPalette.SurfaceDark,
                    "filter tray authored background");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void ResultsList_ContainerAndRowTemplate_UseAuthoredPalette()
        {
            var go = new GameObject("results-list-a11y-test");
            try
            {
                var view = go.AddComponent<TileStories.ResultsListView>();
                var root = new VisualElement();
                view.CreateUI(root);

                var list = root.Q<VisualElement>("results-list-view");
                Assert.IsNotNull(list, "results-list-view missing from built UI");
                AssertColorClose(list.style.backgroundColor.value, TileStories.UIPalette.SurfaceDark,
                    "results list authored background");

                // Build one real row via the view's own factory and read its styles.
                var makeMethod = typeof(TileStories.ResultsListView).GetMethod("MakeResultItem",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                Assert.IsNotNull(makeMethod, "MakeResultItem not found on ResultsListView");
                var row = (VisualElement)makeMethod.Invoke(view, null);

                AssertColorClose(row.style.backgroundColor.value, TileStories.UIPalette.SurfaceDark,
                    "row template authored background");

                var nameLabel = row.Q<Label>("result-name");
                var categoryLabel = row.Q<Label>("result-category");
                Assert.IsNotNull(nameLabel, "result-name label missing from row template");
                Assert.IsNotNull(categoryLabel, "result-category label missing from row template");
                AssertColorClose(nameLabel.style.color.value, TileStories.UIPalette.TextPrimary,
                    "result name text color");
                AssertColorClose(categoryLabel.style.color.value, TileStories.UIPalette.TextSecondary,
                    "result category text color");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void UIPalette_Text_MeetsWCAG_AA_OnSurfaceDark_OverBlackAndWhiteBases()
        {
            // The overlays are translucent (alpha 0.85), so evaluate contrast
            // against BOTH worst-case composite bases: a black camera feed and a
            // fully-white camera feed. Both must pass the 4.5:1 normal-text bar.
            foreach (var baseColor in new[] { Color.black, Color.white })
            {
                Color surface = CompositeOver(TileStories.UIPalette.SurfaceDark, baseColor);
                Assert.IsTrue(
                    UIAccessibility.MeetsContrastAA(TileStories.UIPalette.TextPrimary, surface, largeOrUIC: false),
                    $"TextPrimary on SurfaceDark-over-{baseColor} fails WCAG AA normal-text");
                Assert.IsTrue(
                    UIAccessibility.MeetsContrastAA(TileStories.UIPalette.TextSecondary, surface, largeOrUIC: false),
                    $"TextSecondary on SurfaceDark-over-{baseColor} fails WCAG AA normal-text");
            }
        }

        [Test]
        public void MinimapDots_AllHitZones_AuthorMinTapTarget44()
        {
            var config = MakeConfig();
            var go = new GameObject("minimap-a11y-test");
            try
            {
                var view = go.AddComponent<TileStories.MinimapView>();
                var root = new VisualElement();
                view.CreateUI(root);
                view.Initialize(config, null); // background exists -> dots render

                var hits = root.Query<VisualElement>()
                    .Where(e => e.name != null && e.name.StartsWith("minimap-hit-"))
                    .ToList();

                Assert.AreEqual(config.pois.Count, hits.Count,
                    "expected one authored hit zone per POI");

                foreach (var hit in hits)
                {
                    float w = hit.style.width.value.value;
                    float h = hit.style.height.value.value;
                    Assert.IsTrue(UIAccessibility.MeetsMinTapTarget(w, h),
                        $"minimap hit zone '{hit.name}' is {w}x{h}px, below the 44x44px WCAG minimum");
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }
    }
}