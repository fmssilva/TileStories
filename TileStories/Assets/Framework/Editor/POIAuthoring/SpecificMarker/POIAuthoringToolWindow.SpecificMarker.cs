using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    public partial class POIAuthoringToolWindow
    {
        private void DrawSpecificMarkerOptions()
        {
            if (_config.pois == null || _config.pois.Count == 0)
            {
                EditorGUILayout.HelpBox("No POI data loaded.", MessageType.Info);
                return;
            }

            for (int i = 0; i < _config.pois.Count; i++)
            {
                var poi = _config.pois[i];
                if (poi == null)
                    continue;

                string foldoutKey = string.IsNullOrWhiteSpace(poi.id) ? $"poi_{i}" : poi.id;
                bool expanded = GetPoiFoldout(foldoutKey);
                expanded = EditorGUILayout.Foldout(expanded, $"{i + 1}. {poi.name} ({poi.id})", true, CreateFoldoutStyle(PoiHeaderColorFor(foldoutKey, i)));
                _poiFoldouts[foldoutKey] = expanded;
                if (!expanded)
                    continue;

                using (new EditorGUI.IndentLevelScope())
                {
                    _showPoiPosition = DrawFramedFoldout(ref _showPoiPosition, () => DrawPoiPositionFields(poi), "Position", FoldoutDefaultColor);

                    _showPoiMarkerStyle = DrawFramedFoldout(ref _showPoiMarkerStyle, () => DrawPoiMarkerStyleFields(poi), "Marker Style", FoldoutDefaultColor);

                    if (_config.marker_use_badge)
                    {
                        _showPoiBadgeStyle = DrawFramedFoldout(ref _showPoiBadgeStyle, () => DrawPoiBadgeStyleFields(poi), "Badge Style", FoldoutDefaultColor);
                    }

                    // Outline is an independent axis from badge (section 13.0) --
                    // it must be reachable even when badge is off, as long as the
                    // wall has a non-"none" outline mode.
                    bool outlineEnabled = !string.Equals(_config.marker_outline_mode, "none", StringComparison.OrdinalIgnoreCase);
                    if (outlineEnabled)
                    {
                        _showPoiOutline = DrawFramedFoldout(ref _showPoiOutline, () => DrawPoiOutlineFields(poi), "Outline", FoldoutDefaultColor);
                    }

                    // Effects foldout removed: per-POI effect selection is now
                    // driven entirely by the hierarchy level (see DrawPoiMarkerStyleFields).
                    // Global effect *defaults* remain in the Global Scene Effects section.
                    _showPoiSearchKeywords = DrawFramedFoldout(ref _showPoiSearchKeywords, () => DrawPoiSearchKeywordsField(poi), "Search Keywords", FoldoutDefaultColor);
                }

                EditorGUILayout.Space(6f);
            }
        }

        private void DrawPoiPositionFields(POIData poi)
        {
            poi.x_norm = EditorGUILayout.Slider("X norm", poi.x_norm, 0f, 1f);
            poi.y_norm = EditorGUILayout.Slider("Y norm", poi.y_norm, 0f, 1f);

            bool hasCaptured = poi.has_captured_position;
            bool wantsCaptured = EditorGUILayout.Toggle("Use captured position", hasCaptured);

            if (wantsCaptured && !hasCaptured)
            {
                poi.captured_position = new CapturedPosition();
                poi.has_captured_position = true;
                poi.captured_position_source = "manual";
                poi.captured_position_timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            }
            else if (!wantsCaptured && hasCaptured)
            {
                poi.captured_position = null;
                poi.has_captured_position = false;
                poi.captured_position_source = null;
            }

            if (poi.captured_position != null)
            {
                var cp = poi.captured_position;
                cp.x = EditorGUILayout.FloatField("X", cp.x);
                cp.y = EditorGUILayout.FloatField("Y", cp.y);
                cp.z = EditorGUILayout.FloatField("Z", cp.z);
            }
        }

        private void DrawPoiMarkerStyleFields(POIData poi)
        {
            poi.name = EditorGUILayout.TextField("Name", poi.name);
            poi.category = DrawCategoryDropdown("Category", poi.category);

            // Hierarchy Level: selects this POI's size/label/effects/reveal-delay
            // from the wall's hierarchy_levels table (section 2.3). Populated from
            // _config.hierarchy_levels; writes poi.hierarchy_level_key.
            poi.hierarchy_level_key = DrawHierarchyLevelDropdown("Hierarchy Level", poi.hierarchy_level_key);

            // Custom symbol override (section 13.6/21) -- replaces the old "is_hero"
            // concept. When checked, shows a Sprite field + preview. Uses the same
            // assign-to-wall-library-and-get-key flow as the category table.
            // Setting it changes just this POI's icon; category color, ring, and
            // badge are unaffected.
            poi.has_custom_symbol = EditorGUILayout.Toggle("Use Custom Symbol", poi.has_custom_symbol);

            if (poi.has_custom_symbol)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Custom symbol (optional)", GUILayout.MinWidth(130f));
                    Sprite current = ResolveSpriteForKey(poi.custom_symbol_key);
                    DrawSpritePreview(current);
                    Sprite chosen = (Sprite)EditorGUILayout.ObjectField(current, typeof(Sprite), false, GUILayout.MinWidth(140f));
                    if (chosen != current)
                        poi.custom_symbol_key = chosen != null ? AssignSpriteToLibraryAndGetKey(chosen, poi.id + "_symbol") : null;
                }
                EditorGUILayout.LabelField("Overrides just this POI's icon (e.g. a small castle glyph). Category color, ring, and badge stay unchanged.", EditorStyles.wordWrappedMiniLabel);
            }
        }

        private void DrawPoiBadgeStyleFields(POIData poi)
        {
            poi.badge_category = DrawBadgeCategoryDropdown("Badge category", poi.badge_category);
        }

        private void DrawPoiOutlineFields(POIData poi)
        {
            bool hasStatus = poi.has_status;
            bool wantsStatus = EditorGUILayout.Toggle("Has status", hasStatus);

            if (wantsStatus && !hasStatus)
            {
                poi.has_status = true;
                poi.status_pct = 0f;
            }
            else if (!wantsStatus && hasStatus)
            {
                poi.has_status = false;
                poi.status_pct = 0f;
                poi.status_unknown = false;
                poi.status_level_key = null;
            }

            if (poi.has_status)
            {
                if (_config.outline_levels != null && _config.outline_levels.Count > 0)
                    DrawStatusLevelDropdown(poi);
                else
                    poi.status_pct = EditorGUILayout.Slider("Status %", poi.status_pct, 0f, 100f);

                bool wasUnknown = poi.status_unknown;
                poi.status_unknown = EditorGUILayout.Toggle("Status unknown", poi.status_unknown);
                if (!wasUnknown && poi.status_unknown)
                    ApplyUnknownStatusDefaults(poi);
            }

            // rotate_contour is now a hierarchy-level property, not a per-POI field.
            // Configured in the Global Scene Hierarchy table (see DrawGlobalHierarchySection).
        }

        private void ApplyUnknownStatusDefaults(POIData poi)
        {
            if (poi == null)
                return;

            if (_config?.outline_levels != null)
            {
                for (int i = 0; i < _config.outline_levels.Count; i++)
                {
                    var level = _config.outline_levels[i];
                    if (level == null || string.IsNullOrWhiteSpace(level.key))
                        continue;

                    if (level.key == "unknown")
                    {
                        poi.status_level_key = level.key;
                        poi.status_pct = level.pct;
                        break;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(poi.badge_category) && _config?.badge_categories != null)
            {
                for (int i = 0; i < _config.badge_categories.Count; i++)
                {
                    var badge = _config.badge_categories[i];
                    if (badge == null || string.IsNullOrWhiteSpace(badge.key))
                        continue;

                    if (badge.key == "unknown_damage")
                    {
                        poi.badge_category = badge.key;
                        break;
                    }
                }
            }
        }

        // Hierarchy Level dropdown: maps level labels back to their stable keys.
        // Follows the same pattern as DrawStatusLevelDropdown -- select by current
        // key, display by label, write back the key. "(none)" option clears the
        // key so the marker falls through to MarkerHierarchyResolver.Fallback.
        private string DrawHierarchyLevelDropdown(string label, string currentKey)
        {
            if (_config?.hierarchy_levels == null || _config.hierarchy_levels.Count == 0)
            {
                EditorGUILayout.LabelField(label, "No hierarchy levels defined (see Global Scene).", EditorStyles.miniLabel);
                return currentKey;
            }

            var entries = _config.hierarchy_levels;
            var labels = new string[entries.Count + 1];
            labels[0] = "(none)";

            int selectedIndex = 0;
            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (entry == null || string.IsNullOrWhiteSpace(entry.key))
                    continue;

                string display = string.IsNullOrWhiteSpace(entry.label) ? entry.key : entry.label;
                labels[i + 1] = display;

                if (entry.key == currentKey)
                    selectedIndex = i + 1;
            }

            int next = EditorGUILayout.Popup(label, selectedIndex, labels);
            return next == 0 ? null : entries[next - 1].key;
        }

        private string DrawCategoryDropdown(string label, string current)
        {
            var options = CollectCategoryOptions();
            int idx = Mathf.Max(0, options.IndexOf(current));
            int next = EditorGUILayout.Popup(label, idx, options.ToArray());
            return options[next];
        }

        private string DrawBadgeCategoryDropdown(string label, string current)
        {
            var options = new List<string> { "" };
            if (_config?.badge_categories != null)
            {
                foreach (var entry in _config.badge_categories)
                {
                    if (entry == null || string.IsNullOrWhiteSpace(entry.key))
                        continue;
                    if (!options.Contains(entry.key))
                        options.Add(entry.key);
                }
            }

            int idx = Mathf.Max(0, options.IndexOf(current));
            int next = EditorGUILayout.Popup(label, idx, options.ToArray());
            return options[next];
        }

        private void DrawStatusLevelDropdown(POIData poi)
        {
            var levels = _config.outline_levels;
            if (levels == null || levels.Count == 0)
            {
                poi.status_pct = EditorGUILayout.Slider("Status %", poi.status_pct, 0f, 100f);
                return;
            }

            var labels = new string[levels.Count];
            int selectedIndex = 0;
            for (int i = 0; i < levels.Count; i++)
            {
                var level = levels[i];
                string levelLabel = !string.IsNullOrWhiteSpace(level.label) ? level.label : (level.key ?? $"Level {i + 1}");
                labels[i] = levelLabel + " (" + level.pct.ToString("0") + "%)";

                if (!string.IsNullOrWhiteSpace(poi.status_level_key) && poi.status_level_key == level.key)
                    selectedIndex = i;
            }

            int next = EditorGUILayout.Popup("Status level", selectedIndex, labels);
            next = Mathf.Clamp(next, 0, levels.Count - 1);
            poi.status_level_key = levels[next].key;
            poi.status_pct = levels[next].pct;
            EditorGUILayout.LabelField("Resolved status %", poi.status_pct.ToString("0.0"));
        }

        private List<string> CollectCategoryOptions()
        {
            var options = new List<string>();

            if (_config?.category_styles != null)
            {
                foreach (var entry in _config.category_styles)
                {
                    if (entry == null || string.IsNullOrWhiteSpace(entry.category))
                        continue;
                    if (!options.Contains(entry.category))
                        options.Add(entry.category);
                }
            }

            if (options.Count == 0)
                options.Add("unknown");

            return options;
        }

        private bool GetPoiFoldout(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return true;

            if (!_poiFoldouts.TryGetValue(key, out bool expanded))
            {
                expanded = true;
                _poiFoldouts[key] = true;
            }

            return expanded;
        }

        // Per-POI search keywords editor (Block 5, Phase 5.1, task 4).
        // Edits POIData.search_keywords via a multi-line TextField popup.
        private void DrawPoiSearchKeywordsField(POIData poi)
        {
            if (poi == null)
                return;

            if (poi.search_keywords == null)
                poi.search_keywords = new List<string>();

            string joined = string.Join(", ", poi.search_keywords);
            EditorGUILayout.LabelField("Keywords (comma-separated)", EditorStyles.miniLabel);
            string edited = EditorGUILayout.TextField(joined, GUILayout.Height(60f));
            if (edited != joined)
            {
                poi.search_keywords = ParseKeywordListStatic(edited);
                _hasUnsavedChanges = true;
            }
        }

        // Parse a comma-separated keyword string into a list, trimming empties.
        // Duplicated from SymbolTable.cs to avoid assembly-boundary issues
        // (this partial is in the same assembly, but keeps the method self-contained).
        private static List<string> ParseKeywordListStatic(string text)
        {
            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(text))
                return result;

            foreach (string part in text.Split(','))
            {
                string trimmed = part.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                    result.Add(trimmed);
            }
            return result;
        }
    }
}
