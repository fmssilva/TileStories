using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    public partial class POIAuthoringToolWindow
    {
        // Validates that every POI's hierarchy_level_key resolves to an entry
        // in _config.hierarchy_levels. Returns a list of issues (empty if clean).
        // Also detects the edge case where hierarchy_levels is empty/null but
        // POIs have non-empty hierarchy_level_key values.
        private List<EditorAlertItem> ValidateHierarchyLevelKeys()
        {
            var issues = new List<EditorAlertItem>();

            if (_config == null || _config.pois == null)
                return issues;

            var levelKeys = _config.hierarchy_levels != null
                ? new HashSet<string>(_config.hierarchy_levels
                    .Where(e => e != null && !string.IsNullOrEmpty(e.key))
                    .Select(e => e.key))
                : new HashSet<string>();

            foreach (var poi in _config.pois)
            {
                if (poi == null) continue;

                string key = poi.hierarchy_level_key;
                if (string.IsNullOrEmpty(key))
                    continue; // Empty key is allowed -- falls back to framework default

                if (!levelKeys.Contains(key))
                {
                    issues.Add(new EditorAlertItem(
                        poiId: poi.id ?? "<unnamed>",
                        value: key,
                        problem: $"Hierarchy level key does not match any entry in the hierarchy_levels table.",
                        fixHint: levelKeys.Count == 0
                            ? "Add at least one hierarchy level row, or clear this POI's hierarchy_level_key."
                            : $"Add a row with key '{key}' or change this POI's key to one of: {string.Join(", ", levelKeys)}."));
                }
            }

            return issues;
        }

        // Soft sanity check on marker-symbol diameters: flags sizes outside the
        // plausible range that usually indicate a unit typo (m vs cm). Warning
        // only -- never blocks authoring, never auto-fixes.
        internal static List<EditorAlertItem> ValidateHierarchyLevelSizeRange(
            IEnumerable<global::TileStories.HierarchyLevelEntry> levels)
        {
            var issues = new List<EditorAlertItem>();
            if (levels == null)
                return issues;
            foreach (var entry in levels)
            {
                if (entry == null)
                    continue;
                float s = entry.size_cm;
                if (s < 0.5f || s > 100f)
                {
                    issues.Add(new EditorAlertItem(
                        poiId: entry.key ?? "<unnamed>",
                        value: $"{s:0.##} cm",
                        problem: "size_cm is outside the plausible marker symbol range.",
                        fixHint: "Real marker symbols are ~0.5cm..100cm. Check units (cm vs m)."));
                }
            }
            return issues;
        }

        // Runs validation after config load and shows a non-blocking alert if
        // any hierarchy keys are unresolvable or level sizes look out of range.
        private void ValidateAndAlert(string context)
        {
            var issues = new List<EditorAlertItem>();
            issues.AddRange(ValidateHierarchyLevelKeys());
            issues.AddRange(ValidateHierarchyLevelSizeRange(_config?.hierarchy_levels));
            if (issues.Count == 0)
                return;

            string title = $"Hierarchy config issues ({context})";
            string guidance = "Some hierarchy configuration needs attention before building: fix or add the missing level rows for unresolvable keys, and correct any level sizes outside 0.5-100cm (likely a cm/m unit typo). Levels with issues fall back to framework defaults at runtime until fixed.";
            PopupWindow.Show(new Rect(100, 100, 100, 100), new EditorAlertPopup(title, issues, guidance));
        }
    }
}
