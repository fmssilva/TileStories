using System.Collections.Generic;
using System.Linq;

namespace TileStories.Editor
{
    // Pure text and list logic behind the Effects editor section (no IMGUI, no scene), so it can be
    // tested with plain `new` data: display names, "which levels use this effect" lines, the
    // per-POI effects line and the dropdown filtering that keeps disabled effects out of the
    // Hierarchy Levels table.
    internal static class EffectUsageSummary
    {
        // "Used by: level_1, level_2 (7 POIs)" or a plain "not used" line.
        public static string DescribeUsage(WallConfigData config, MarkerEffectFlags effect)
        {
            var usingLevels = new List<string>();
            int poiCount = 0;
            if (config?.hierarchy_levels != null)
            {
                foreach (var level in config.hierarchy_levels)
                {
                    if (level == null || string.IsNullOrWhiteSpace(level.key)) continue;
                    if ((MarkerHierarchyResolver.EffectFlagsOf(level, logWarnings: false) & effect) == 0) continue;

                    usingLevels.Add(level.key);
                    if (config.pois != null)
                        poiCount += config.pois.Count(p => p != null && p.hierarchy_level_key == level.key);
                }
            }

            if (usingLevels.Count == 0)
                return "Not used by any hierarchy level yet. Pick it in Hierarchy Levels.";

            string pois = poiCount == 1 ? "1 POI" : poiCount + " POIs";
            return $"Used by: {string.Join(", ", usingLevels)} ({pois}).";
        }

        // "Effects: Ripple Discs, Halo Ring, Pulse. Reveal: 0.2 s delay, 0.4 s fade-in." for one level,
        // marking requested effects that are switched off in the Effects section.
        public static string DescribeLevelEffects(HierarchyLevelEntry level, EffectDefaults defaults)
        {
            if (level == null)
                return "Effects: none (no hierarchy level selected).";

            var requested = MarkerHierarchyResolver.EffectFlagsOf(level, logWarnings: false);
            var shown = new List<string>();
            foreach (var effect in EffectDefaults.SelectableEffects)
            {
                if ((requested & effect) == 0) continue;
                bool off = defaults != null && (!defaults.effects_enabled || !defaults.IsEffectEnabled(effect));
                shown.Add(off ? MarkerEffectNames.DisplayName(effect) + " (off)" : MarkerEffectNames.DisplayName(effect));
            }

            string effects = shown.Count == 0 ? "none" : string.Join(", ", shown);
            return $"Effects: {effects}. Reveal: {level.reveal_delay_s:0.##} s delay, {level.reveal_duration_s:0.##} s fade-in.";
        }

        // Preview base-marker dropdown: "Plain grey circle" (empty id) plus every POI of the wall.
        public static void PreviewBaseOptions(WallConfigData config, out string[] ids, out string[] labels)
        {
            var i = new List<string> { "" };
            var l = new List<string> { "Plain grey circle" };
            if (config?.pois != null)
            {
                foreach (var poi in config.pois)
                {
                    if (poi == null || string.IsNullOrWhiteSpace(poi.id)) continue;
                    i.Add(poi.id);
                    l.Add(string.IsNullOrWhiteSpace(poi.name) ? poi.id : $"{poi.name} ({poi.id})");
                }
            }
            ids = i.ToArray();
            labels = l.ToArray();
        }

        // Dropdown options for one level-table column: only enabled effects, plus the level's current
        // value when it points at a disabled one (labelled "(disabled)") so it is never silently lost.
        public static void FilterEnabledOptions(string[] options, string[] labels, EffectDefaults defaults,
            string current, out string[] shownOptions, out string[] shownLabels)
        {
            var o = new List<string>();
            var l = new List<string>();
            for (int i = 0; i < options.Length; i++)
            {
                var flag = MarkerHierarchyResolver.EffectFlagsOf(new HierarchyLevelEntry { ripple_effect = options[i], halo_effect = options[i] }, logWarnings: false);
                bool isNone = flag == MarkerEffectFlags.None;
                bool enabled = isNone || defaults == null || defaults.IsEffectEnabled(flag);
                bool isCurrent = options[i] == current;

                if (!enabled && !isCurrent) continue;
                o.Add(options[i]);
                l.Add(enabled ? labels[i] : labels[i] + " (disabled)");
            }
            shownOptions = o.ToArray();
            shownLabels = l.ToArray();
        }
    }
}
