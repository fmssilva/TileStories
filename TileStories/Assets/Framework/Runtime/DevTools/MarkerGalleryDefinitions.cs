using System;
using System.Collections.Generic;

namespace TileStories
{
    // One row in the marker gallery: a single marker variant to render and assert on.
    // Phase 1 starts with one sanity entry; Phase 2 grows Entries one group at a time.
    public readonly struct MarkerGalleryEntry
    {
        public readonly string Group;
        public readonly string Label;
        public readonly string Category;
        public readonly MarkerStyle Style;
        public readonly MarkerShape Shape;
        public readonly float StatusPct;
        public readonly bool HasStatus;
        public readonly bool StatusUnknown;
        public readonly bool IsHero;
        public readonly MarkerEffectFlags EffectFlags;
        public readonly bool RotateContour;

        public MarkerGalleryEntry(string group, string label, string category, MarkerStyle style,
            MarkerShape shape, float statusPct, bool hasStatus, bool statusUnknown, bool isHero)
            : this(group, label, category, style, shape, statusPct, hasStatus, statusUnknown, isHero, MarkerEffectFlags.None, rotateContour: false)
        {
        }

        public MarkerGalleryEntry(string group, string label, string category, MarkerStyle style,
            MarkerShape shape, float statusPct, bool hasStatus, bool statusUnknown, bool isHero,
            MarkerEffectFlags effectFlags, bool rotateContour = false)
        {
            Group = group; Label = label; Category = category; Style = style; Shape = shape;
            StatusPct = statusPct; HasStatus = hasStatus; StatusUnknown = statusUnknown; IsHero = isHero;
            EffectFlags = effectFlags; RotateContour = rotateContour;
        }
    }

    // Data-driven gallery definitions. The harness and test suite both read Entries
    // from here -- grow Entries incrementally in Phase 2, the harness/tests never change.
    public static class MarkerGalleryDefinitions
    {
        // Applied once via CategoryPalette.Configure before the gallery spawns
        // anything. Uses "furniture" (a real LivingRoom category) so the
        // override is checked against a realistic case, not an invented one.
        public static readonly List<CategoryStyleEntry> Overrides = new()
        {
            new CategoryStyleEntry { category = "furniture", color_hex = "#8033CC" },
        };

        // Phase 1: sanity entry. Phase 2: grows one group at a time (Steps 1-7).
        public static readonly List<MarkerGalleryEntry> Entries = BuildEntries();

        private static List<MarkerGalleryEntry> BuildEntries()
        {
            var list = new List<MarkerGalleryEntry>();
            const MarkerStyle gold = MarkerStyle.OutlineGold;
            const MarkerShape circle = MarkerShape.Circle;

            // Phase 1: sanity check -- one circle, known category, no status.
            list.Add(new MarkerGalleryEntry("Sanity", "Single circle, civic, no status",
                "civic", gold, circle, 0, false, false, false));

            // Phase 2 Step 1: Category spectrum -- 8 named taxonomy categories
            // (each has an icon in KnownIcons) + 2 arbitrary ones (hash fallback,
            // no icon), status off. Tests category->colour->icon resolution.
            string[] categories = { "religious", "royal", "military", "civic", "maritime",
                                     "infra", "landscape", "commerce", "furniture", "art" };
            foreach (var cat in categories)
                list.Add(new MarkerGalleryEntry("Category", cat, cat, gold, circle, 0, false, false, false));

            // Phase 2 Step 2: Shape spectrum -- one per MarkerShape, status off.
            // This is where the shape-key lookup bug (if present) surfaces:
            // Symbol_AlwaysHasNonNullSprite would fail for every shape except Circle.
            foreach (MarkerShape shape in Enum.GetValues(typeof(MarkerShape)))
                list.Add(new MarkerGalleryEntry("Shape", shape.ToString(), "civic", gold, shape, 0, false, false, false));

            // Steps 3-5: Status spectrum, once per MarkerStyle.
            (float pct, string label)[] statusLevels = {
                (0,"0% Intact"), (20,"20%"), (40,"40%"), (60,"60%"), (80,"80%"), (100,"100% Gone")
            };
            foreach (MarkerStyle style in new[] { MarkerStyle.OutlineGold, MarkerStyle.OutlineSameHue, MarkerStyle.Badge })
            {
                string groupName = "Status - " + style;
                foreach (var (pct, label) in statusLevels)
                    list.Add(new MarkerGalleryEntry(groupName, label, "civic", style, circle, pct, true, false, false));
                list.Add(new MarkerGalleryEntry(groupName, "No status axis (baseline)", "civic", style, circle, 0, false, false, false));
                list.Add(new MarkerGalleryEntry(groupName, "Unknown fate (? badge)", "civic", style, circle, 0, true, true, false));
            }

            // Extra matrix coverage requested in review: ring/badge styles with all non-circle shapes.
            MarkerShape[] nonCircleShapes = { MarkerShape.RoundedSquare, MarkerShape.Hexagon, MarkerShape.Diamond, MarkerShape.Star };
            foreach (var shape in nonCircleShapes)
            {
                list.Add(new MarkerGalleryEntry("Status+Shape - OutlineGold", shape + " | 60%", "civic", MarkerStyle.OutlineGold, shape, 60, true, false, false));
                list.Add(new MarkerGalleryEntry("Status+Shape - OutlineSameHue", shape + " | 60%", "civic", MarkerStyle.OutlineSameHue, shape, 60, true, false, false));
                list.Add(new MarkerGalleryEntry("Status+Shape - Badge", shape + " | 60%", "civic", MarkerStyle.Badge, shape, 60, true, false, false));
            }

            // Step 6: Hero tier matrix -- modular effect composition across marker styles.
            AddHeroEffectRows(list,
                group: "Hero Effects - Simple",
                style: MarkerStyle.OutlineGold,
                shape: MarkerShape.Circle,
                hasStatus: false,
                statusPct: 0f);

            AddHeroEffectRows(list,
                group: "Hero Effects - OutlineGold",
                style: MarkerStyle.OutlineGold,
                shape: MarkerShape.Circle,
                hasStatus: true,
                statusPct: 60f);

            AddHeroEffectRows(list,
                group: "Hero Effects - OutlineSameHue",
                style: MarkerStyle.OutlineSameHue,
                shape: MarkerShape.Circle,
                hasStatus: true,
                statusPct: 60f);

            AddHeroEffectRows(list,
                group: "Hero Effects - Badge",
                style: MarkerStyle.Badge,
                shape: MarkerShape.Circle,
                hasStatus: true,
                statusPct: 60f);

            // Step 7: Category colour override.
            list.Add(new MarkerGalleryEntry("Override", "furniture (override color + fallback icon)", "furniture", gold, circle, 0, true, false, false));
            list.Add(new MarkerGalleryEntry("Override", "art (hash color + fallback icon)", "art", gold, circle, 0, true, false, false));

            // Step 8: contour rotation.
            AddRotateContourRows(list);

            // Step 9: Ring Pulse (thin contour, breathing).
            AddAccentEffectRows(list, "Accent - Ring Pulse", MarkerEffectFlags.RingPulse);

            // Step 10: Simple Sun (filled disc, breathing).
            AddAccentEffectRows(list, "Accent - Simple Sun", MarkerEffectFlags.SimpleSun);

            // Step 11: Beacon (thin contour, grow+fade sawtooth).
            AddAccentEffectRows(list, "Accent - Beacon", MarkerEffectFlags.Beacon);

            // Step 12: Background shape None (section 20.1) -- icon-only marker,
            // no backdrop behind the symbol. Category icon must still be visible.
            list.Add(new MarkerGalleryEntry("None Background", "None shape, civic icon",
                "civic", MarkerStyle.OutlineGold, MarkerShape.None, 0, false, false, false));

            // Step 13: Hero icon override (section 21) -- hero POI with a
            // hero_icon_key that overrides the category-derived icon.
            list.Add(new MarkerGalleryEntry("Hero Icon Override", "Hero with override",
                "civic", MarkerStyle.OutlineGold, MarkerShape.Circle, 0, false, false, true));

            return list;
        }

        private static void AddHeroEffectRows(List<MarkerGalleryEntry> list, string group, MarkerStyle style, MarkerShape shape, bool hasStatus, float statusPct)
        {
            const string category = "religious";
            list.Add(new MarkerGalleryEntry(group, "Pulse", category, style, shape, statusPct, hasStatus, false, true, MarkerEffectFlags.Pulse));
            list.Add(new MarkerGalleryEntry(group, "Sun Circles", category, style, shape, statusPct, hasStatus, false, true, MarkerEffectFlags.SunCircles));
            list.Add(new MarkerGalleryEntry(group, "Sun Contours", category, style, shape, statusPct, hasStatus, false, true, MarkerEffectFlags.SunContours));
            list.Add(new MarkerGalleryEntry(group, "Pulse + Sun Circles", category, style, shape, statusPct, hasStatus, false, true, MarkerEffectFlags.PulseSunCircles));
            list.Add(new MarkerGalleryEntry(group, "Pulse + Sun Contours", category, style, shape, statusPct, hasStatus, false, true, MarkerEffectFlags.PulseSunContours));
        }

        // Step 8: Contour rotation -- independent of hero/effect tiers, applies to
        // the status ring itself. OutlineGold and OutlineSameHue only (Badge style
        // still has no ring, so rotation would not be visible there).
        private static void AddRotateContourRows(List<MarkerGalleryEntry> list)
        {
            const string group = "Contour Rotation";
            list.Add(new MarkerGalleryEntry(group, "OutlineGold, static", "civic", MarkerStyle.OutlineGold, MarkerShape.Circle, 40, true, false, false, MarkerEffectFlags.None, rotateContour: false));
            list.Add(new MarkerGalleryEntry(group, "OutlineGold, rotating", "civic", MarkerStyle.OutlineGold, MarkerShape.Circle, 40, true, false, false, MarkerEffectFlags.None, rotateContour: true));
            list.Add(new MarkerGalleryEntry(group, "OutlineSameHue, static", "civic", MarkerStyle.OutlineSameHue, MarkerShape.Circle, 40, true, false, false, MarkerEffectFlags.None, rotateContour: false));
            list.Add(new MarkerGalleryEntry(group, "OutlineSameHue, rotating", "civic", MarkerStyle.OutlineSameHue, MarkerShape.Circle, 40, true, false, false, MarkerEffectFlags.None, rotateContour: true));
        }

        // Steps 9-11: the three new single-accent effects, each proven on BOTH a
        // non-hero and a hero marker -- this is the direct, visual test that they
        // are no longer hero-gated (the whole point of this expansion).
        private static void AddAccentEffectRows(List<MarkerGalleryEntry> list, string group, MarkerEffectFlags effect)
        {
            list.Add(new MarkerGalleryEntry(group, "Non-hero", "civic", MarkerStyle.OutlineGold, MarkerShape.Circle, 0, false, false, false, effect, rotateContour: false));
            list.Add(new MarkerGalleryEntry(group, "Hero (label + effect)", "religious", MarkerStyle.OutlineGold, MarkerShape.Circle, 0, false, false, true, effect, rotateContour: false));
        }
    }
}