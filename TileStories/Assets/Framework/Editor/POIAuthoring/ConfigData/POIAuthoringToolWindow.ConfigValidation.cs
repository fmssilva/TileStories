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

        // Runs validation after config load and shows a non-blocking alert if
        // any hierarchy_level_key values are unresolvable.
        private void ValidateAndAlert(string context)
        {
            var issues = ValidateHierarchyLevelKeys();
            if (issues.Count == 0)
                return;

            string title = $"Unresolvable Hierarchy Keys ({context})";
            string guidance = "POIs with unresolvable hierarchy_level_key values will use the framework default level (12cm, no label, no effects) at runtime. Fix the keys or add the missing level rows before building.";
            PopupWindow.Show(new Rect(100, 100, 100, 100), new EditorAlertPopup(title, issues, guidance));
        }
    }
}
