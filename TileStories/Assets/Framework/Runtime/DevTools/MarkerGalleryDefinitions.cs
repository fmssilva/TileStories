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
        public readonly string HierarchyLevelKey;
        public readonly MarkerEffectFlags EffectFlags;

        public MarkerGalleryEntry(string group, string label, string category, MarkerStyle style,
            MarkerShape shape, float statusPct, bool hasStatus, bool statusUnknown,
            string hierarchyLevelKey = null, MarkerEffectFlags effectFlags = MarkerEffectFlags.None)
        {
            Group = group; Label = label; Category = category; Style = style; Shape = shape;
            StatusPct = statusPct; HasStatus = hasStatus; StatusUnknown = statusUnknown;
            HierarchyLevelKey = hierarchyLevelKey;
            EffectFlags = effectFlags;
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

        // Five fabricated hierarchy levels mirroring section 13 of
        // _2_3_Marker_Hierarchy.md. Used both for Tier 0 resolver assertions and
        // for gallery entries that opt into hierarchy-based sizing/label/effects.
        public static List<HierarchyLevelEntry> BuildHierarchyLevels()
        {
            return new List<HierarchyLevelEntry>
            {
                new HierarchyLevelEntry
                {
                                        key = "level_1", label = "1", size_cm = 20f, show_label = true,
                    sun_effect = "sun_circles", accent_effect = "ring_pulse",
                    pulse = true, rotate_contour = true, reveal_delay_s = 0f,
                    reveal_duration_s = 0.5f
                },
                new HierarchyLevelEntry
                {
                                        key = "level_2", label = "2", size_cm = 15f, show_label = false,
                    sun_effect = "sun_contours", accent_effect = "none",
                    pulse = true, rotate_contour = true, reveal_delay_s = 0.15f,
                    reveal_duration_s = 0.4f
                },
                new HierarchyLevelEntry
                {
                                        key = "level_3", label = "3", size_cm = 10f, show_label = false,
                    sun_effect = "none", accent_effect = "simple_sun",
                    pulse = true, rotate_contour = true, reveal_delay_s = 0.3f,
                    reveal_duration_s = 0.35f
                },
                new HierarchyLevelEntry
                {
                                        key = "level_4", label = "4", size_cm = 5f, show_label = false,
                    sun_effect = "none", accent_effect = "beacon",
                    pulse = true, rotate_contour = true, reveal_delay_s = 0.45f,
                    reveal_duration_s = 0.3f
                },
                new HierarchyLevelEntry
                {
                                        key = "level_5", label = "5", size_cm = 2f, show_label = false,
                    sun_effect = "none", accent_effect = "none",
                    pulse = true, rotate_contour = true, reveal_delay_s = 0.6f,
                    reveal_duration_s = 0.25f
                },
            };
        }

        // Phase 1: sanity entry. Phase 2: grows Entries one group at a time (Steps 1-7).
        public static readonly List<MarkerGalleryEntry> Entries = BuildEntries();

        private static List<MarkerGalleryEntry> BuildEntries()
        {
            // Configure the resolver with fabricated levels so any entry with
            // a hierarchy_level_key resolves correctly, and so the resolver is
            // in the expected state when the harness/tests run.
            MarkerHierarchyResolver.Configure(BuildHierarchyLevels());

            var list = new List<MarkerGalleryEntry>();
            const MarkerStyle gold = MarkerStyle.OutlineGold;
            const MarkerShape circle = MarkerShape.Circle;

            // Phase 1: sanity check -- one circle, known category, no status.
            list.Add(new MarkerGalleryEntry("Sanity", "Single circle, civic, no status",
                "civic", gold, circle, 0, false, false));

            // Phase 2 Step 1: Category spectrum -- 8 named taxonomy categories
            // (each has an icon in KnownIcons) + 2 arbitrary ones (hash fallback,
            // no icon), status off. Tests category->colour->icon resolution.
            string[] categories = { "religious", "royal", "military", "civic", "maritime",
                                     "infra", "landscape", "commerce", "furniture", "art" };
            foreach (var cat in categories)
                list.Add(new MarkerGalleryEntry("Category", cat, cat, gold, circle, 0, false, false));

            // Phase 2 Step 2: Shape spectrum -- one per MarkerShape, status off.
            // This is where the shape-key lookup bug (if present) surfaces:
            // Symbol_AlwaysHasNonNullSprite would fail for every shape except Circle.
            foreach (MarkerShape shape in Enum.GetValues(typeof(MarkerShape)))
                list.Add(new MarkerGalleryEntry("Shape", shape.ToString(), "civic", gold, shape, 0, false, false));

            // Steps 3-5: Status spectrum, once per MarkerStyle.
            (float pct, string label)[] statusLevels = {
                (0,"0% Intact"), (20,"20%"), (40,"40%"), (60,"60%"), (80,"80%"), (100,"100% Gone")
            };
            foreach (MarkerStyle style in new[] { MarkerStyle.OutlineGold, MarkerStyle.OutlineSameHue, MarkerStyle.Badge })
            {
                string groupName = "Status - " + style;
                foreach (var (pct, label) in statusLevels)
                    list.Add(new MarkerGalleryEntry(groupName, label, "civic", style, circle, pct, true, false));
                list.Add(new MarkerGalleryEntry(groupName, "No status axis (baseline)", "civic", style, circle, 0, false, false));
                list.Add(new MarkerGalleryEntry(groupName, "Unknown fate (? badge)", "civic", style, circle, 0, true, true));
            }

            // Extra matrix coverage requested in review: ring/badge styles with all non-circle shapes.
            MarkerShape[] nonCircleShapes = { MarkerShape.RoundedSquare, MarkerShape.Hexagon, MarkerShape.Diamond, MarkerShape.Star };
            foreach (var shape in nonCircleShapes)
            {
                list.Add(new MarkerGalleryEntry("Status+Shape - OutlineGold", shape + " | 60%", "civic", MarkerStyle.OutlineGold, shape, 60, true, false));
                list.Add(new MarkerGalleryEntry("Status+Shape - OutlineSameHue", shape + " | 60%", "civic", MarkerStyle.OutlineSameHue, shape, 60, true, false));
                list.Add(new MarkerGalleryEntry("Status+Shape - Badge", shape + " | 60%", "civic", MarkerStyle.Badge, shape, 60, true, false));
            }

            // Step 6: Effect composition matrix -- modular effect combinations across marker styles.
            // No hierarchy key: effectFlags parameter drives effects (gallery fallback path),
            // proving effects are no longer gated on hero status.
            AddEffectRows(list,
                group: "Effect Composition - Simple",
                style: MarkerStyle.OutlineGold,
                shape: MarkerShape.Circle,
                hasStatus: false,
                statusPct: 0f);

            AddEffectRows(list,
                group: "Effect Composition - OutlineGold",
                style: MarkerStyle.OutlineGold,
                shape: MarkerShape.Circle,
                hasStatus: true,
                statusPct: 60f);

            AddEffectRows(list,
                group: "Effect Composition - OutlineSameHue",
                style: MarkerStyle.OutlineSameHue,
                shape: MarkerShape.Circle,
                hasStatus: true,
                statusPct: 60f);

            AddEffectRows(list,
                group: "Effect Composition - Badge",
                style: MarkerStyle.Badge,
                shape: MarkerShape.Circle,
                hasStatus: true,
                statusPct: 60f);

            // Step 7: Category colour override.
            list.Add(new MarkerGalleryEntry("Override", "furniture (override color + fallback icon)", "furniture", gold, circle, 0, true, false));
            list.Add(new MarkerGalleryEntry("Override", "art (hash color + fallback icon)", "art", gold, circle, 0, true, false));

            // Step 8: contour rotation -- driven by hierarchy level key now, not a
            // per-entry bool. Entries without a key use the Fallback (rotate=false);
            // entries with "level_5" get rotate_contour=true from the configured level.
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
                "civic", MarkerStyle.OutlineGold, MarkerShape.None, 0, false, false));

            // Step 13: Custom symbol override (section 21) -- POI with
            // has_custom_symbol=true and custom_symbol_key that overrides the
            // category-derived icon. The test sets these on POIData manually.
            list.Add(new MarkerGalleryEntry("Custom Symbol Override", "POI with override",
                "civic", MarkerStyle.OutlineGold, MarkerShape.Circle, 0, false, false));

            return list;
        }

        // Renders every effect-flag combination on a non-hierarchy marker (no
        // hierarchy_level_key, so the effectFlags parameter drives the visuals).
        private static void AddEffectRows(List<MarkerGalleryEntry> list, string group, MarkerStyle style, MarkerShape shape, bool hasStatus, float statusPct)
        {
            const string category = "religious";
            list.Add(new MarkerGalleryEntry(group, "Pulse", category, style, shape, statusPct, hasStatus, false, effectFlags: MarkerEffectFlags.Pulse));
            list.Add(new MarkerGalleryEntry(group, "Sun Circles", category, style, shape, statusPct, hasStatus, false, effectFlags: MarkerEffectFlags.SunCircles));
            list.Add(new MarkerGalleryEntry(group, "Sun Contours", category, style, shape, statusPct, hasStatus, false, effectFlags: MarkerEffectFlags.SunContours));
            list.Add(new MarkerGalleryEntry(group, "Pulse + Sun Circles", category, style, shape, statusPct, hasStatus, false, effectFlags: MarkerEffectFlags.PulseSunCircles));
            list.Add(new MarkerGalleryEntry(group, "Pulse + Sun Contours", category, style, shape, statusPct, hasStatus, false, effectFlags: MarkerEffectFlags.PulseSunContours));
        }

        // Contourse rotation -- hierarchy driven. Entries with a level key get
        // rotation from that level; entries without fall back to Fallback
        // (rotate_contour = false).
        private static void AddRotateContourRows(List<MarkerGalleryEntry> list)
        {
            const string group = "Contour Rotation";
            list.Add(new MarkerGalleryEntry(group, "OutlineGold, static", "civic", MarkerStyle.OutlineGold, MarkerShape.Circle, 40, true, false, effectFlags: MarkerEffectFlags.None));
            list.Add(new MarkerGalleryEntry(group, "OutlineGold, rotating", "civic", MarkerStyle.OutlineGold, MarkerShape.Circle, 40, true, false, hierarchyLevelKey: "level_5", effectFlags: MarkerEffectFlags.None));
            list.Add(new MarkerGalleryEntry(group, "OutlineSameHue, static", "civic", MarkerStyle.OutlineSameHue, MarkerShape.Circle, 40, true, false, effectFlags: MarkerEffectFlags.None));
            list.Add(new MarkerGalleryEntry(group, "OutlineSameHue, rotating", "civic", MarkerStyle.OutlineSameHue, MarkerShape.Circle, 40, true, false, hierarchyLevelKey: "level_5", effectFlags: MarkerEffectFlags.None));
        }

        // Steps 9-11: the three single-accent effects, each proven on BOTH a
        // non-hierarchy and a hierarchy-keyed marker -- this is the direct,
        // visual test that they are no longer hierarchy-gated (the effect flags
        // fallback rule in MarkerView applies when no hierarchy key is set).
        private static void AddAccentEffectRows(List<MarkerGalleryEntry> list, string group, MarkerEffectFlags effect)
        {
            list.Add(new MarkerGalleryEntry(group, "No hierarchy", "civic", MarkerStyle.OutlineGold, MarkerShape.Circle, 0, false, false, effectFlags: effect));
            list.Add(new MarkerGalleryEntry(group, "With hierarchy level_1", "religious", MarkerStyle.OutlineGold, MarkerShape.Circle, 0, false, false, hierarchyLevelKey: "level_1", effectFlags: effect));
        }
    }
}
