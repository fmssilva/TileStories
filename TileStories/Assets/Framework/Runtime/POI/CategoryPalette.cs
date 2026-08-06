using System.Collections.Generic;
using UnityEngine;

namespace TileStories
{
    // Resolves a wall-defined `category` string into a fill colour and (optionally)
    // an icon key. See _2_2_Marker_Design.md §4-5 for why this stays domain-owned
    // data, not a framework-wide palette. A wall may optionally call Configure()
    // once (WallSession does this automatically, §7 Step 8) to hand-pick specific
    // categories' colours/icons; anything not explicitly configured still falls
    // through to the deterministic hash / KnownIcons lookup below.
    public static class CategoryPalette
    {
        // Opt-in icon keys for the Panorama-style building taxonomy from the
        // Stage 2.3 prototypes. Just a lookup table, not an enforced schema -- add
        // entries here (and a matching sprite in IconLibrary.asset) as new named
        // categories get real icons drawn for them.
        // Maps heritage category names to their icon-key in IconLibrary.asset.
        // The six default heritage categories (seeded by the authoring tool when
        // a wall's config.json has no category_styles) use the same key for both
        // the category name and the icon key, so a developer can read either field
        // and find a matching sprite. Older ad-hoc mappings (royal->crown, civic->
        // columns, commerce->scale, infra->bridge) are removed; maritime->anchor and
        // landscape->leaf are retained as non-default known categories.
        private static readonly Dictionary<string, string> KnownIcons = new()
        {
            { "royal_government", "IconRoyal&Government" },
            { "religious", "IconReligious" },
            { "military", "IconMilitary" },
            { "residential", "IconNobel&PrivateResidence" },
            { "economic", "IconIndustry&Trade" },
            { "infrastructure", "IconInfrastructures" },
            { "maritime", "anchor" },
            { "landscape", "leaf" },
        };

        private static readonly Color DefaultColor = new Color(0.35f, 0.33f, 0.30f);

        // The one piece of mutable static state in this class -- everything else
        // here is a pure function of its arguments. Populated once per wall session
        // by WallSession.Configure (§7 Step 8), immediately after config load and
        // before any marker spawns. Acceptable because this app loads exactly one
        // wall at a time; ClearOverrides() exists specifically so tests (and, if
        // ever needed, a future multi-wall session) don't leak state between calls.
        private static Dictionary<string, Color> _colorOverrides;
        private static Dictionary<string, string> _iconOverrides;
        private static HashSet<string> _declaredCategories;

        // Call once per wall, right after config load. overrides may be null or
        // empty -- both mean "no overrides, hash/KnownIcons decide everything,"
        // identical to never calling this at all.
        public static void Configure(IEnumerable<CategoryStyleEntry> overrides)
        {
            _colorOverrides = new Dictionary<string, Color>();
            _iconOverrides = new Dictionary<string, string>();
            _declaredCategories = new HashSet<string>();
            if (overrides == null) return;

            foreach (var entry in overrides)
            {
                if (entry == null || string.IsNullOrEmpty(entry.category)) continue;

                _declaredCategories.Add(entry.category);

                if (!string.IsNullOrEmpty(entry.color_hex) &&
                    ColorUtility.TryParseHtmlString(entry.color_hex, out var parsedColor))
                {
                    _colorOverrides[entry.category] = parsedColor;
                }

                if (!string.IsNullOrEmpty(entry.icon_key))
                {
                    _iconOverrides[entry.category] = entry.icon_key;
                }
            }
        }

        // Resets to "no overrides configured." Call in test [SetUp]/[TearDown] and
        // whenever a new wall session starts, so one wall's/test's overrides never
        // leak into the next.
        public static void ClearOverrides()
        {
            _colorOverrides = null;
            _iconOverrides = null;
            _declaredCategories = null;
        }

        public static bool TryResolveConfigured(string category, out Color color, out string iconKey)
        {
            color = default;
            iconKey = null;

            if (string.IsNullOrWhiteSpace(category) || _declaredCategories == null || !_declaredCategories.Contains(category))
                return false;

            if (_colorOverrides != null && _colorOverrides.TryGetValue(category, out var overrideColor))
            {
                color = overrideColor;
            }
            else
            {
                int hash = StableHash(category);
                float hue = (hash % 360) / 360f;
                color = Color.HSVToRGB(hue, 0.38f, 0.55f);
            }

            if (_iconOverrides != null && _iconOverrides.TryGetValue(category, out var overrideIcon))
                iconKey = overrideIcon;

            return true;
        }

        public static Color ResolveColor(string category)
        {
            if (string.IsNullOrEmpty(category)) return DefaultColor;

            if (_colorOverrides != null && _colorOverrides.TryGetValue(category, out var overrideColor))
                return overrideColor;

            int hash = StableHash(category);
            float hue = (hash % 360) / 360f;

            // Fixed soft/desaturated saturation & value so every generated category
            // colour reads as "adult museum," never neon -- matches the
            // Stage 2.3 HTML prototypes' manually-tuned range.
            return Color.HSVToRGB(hue, 0.38f, 0.55f);
        }

        public static string ResolveIconKey(string category)
        {
            if (string.IsNullOrEmpty(category)) return null;

            if (_iconOverrides != null && _iconOverrides.TryGetValue(category, out var overrideIcon))
                return overrideIcon;

            return KnownIcons.TryGetValue(category, out var key) ? key : "unknown";
        }

        // FNV-1a: deterministic across runs/platforms/.NET versions, unlike
        // string.GetHashCode(), which .NET explicitly does not guarantee is stable.
        // Public (not internal): CategoryPaletteTests lives in a separate asmdef
        // with no InternalsVisibleTo configured, and adding one is more moving
        // parts than making the method public.
        public static int StableHash(string s)
        {
            unchecked
            {
                uint hash = 2166136261;
                foreach (char c in s)
                {
                    hash ^= c;
                    hash *= 16777619;
                }
                return (int)(hash & 0x7FFFFFFF);
            }
        }
    }
}
