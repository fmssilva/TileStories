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
                expanded = EditorGUILayout.Foldout(expanded, $"{i + 1}. {poi.name} ({poi.id})", true);
                _poiFoldouts[foldoutKey] = expanded;
                if (!expanded)
                    continue;

                using (new EditorGUI.IndentLevelScope())
                {
                    _showPoiPosition = EditorGUILayout.Foldout(_showPoiPosition, "Position", true);
                    if (_showPoiPosition)
                    {
                        using (new EditorGUI.IndentLevelScope())
                            DrawPoiPositionFields(poi);
                    }

                    _showPoiMarkerStyle = EditorGUILayout.Foldout(_showPoiMarkerStyle, "Marker Style", true);
                    if (_showPoiMarkerStyle)
                    {
                        using (new EditorGUI.IndentLevelScope())
                            DrawPoiMarkerStyleFields(poi);
                    }

                    if (_config.marker_use_badge)
                    {
                        _showPoiBadgeStyle = EditorGUILayout.Foldout(_showPoiBadgeStyle, "Badge Style", true);
                        if (_showPoiBadgeStyle)
                        {
                            using (new EditorGUI.IndentLevelScope())
                                DrawPoiBadgeStyleFields(poi);
                        }
                    }

                    // Outline is an independent axis from badge (section 13.0) --
                    // it must be reachable even when badge is off, as long as the
                    // wall has a non-"none" outline mode.
                    bool outlineEnabled = !string.Equals(_config.marker_outline_mode, "none", StringComparison.OrdinalIgnoreCase);
                    if (outlineEnabled)
                    {
                        _showPoiOutline = EditorGUILayout.Foldout(_showPoiOutline, "Outline", true);
                        if (_showPoiOutline)
                        {
                            using (new EditorGUI.IndentLevelScope())
                                DrawPoiOutlineFields(poi);
                        }
                    }

                    _showPoiEffects = EditorGUILayout.Foldout(_showPoiEffects, "Effects", true);
                    if (_showPoiEffects)
                    {
                        using (new EditorGUI.IndentLevelScope())
                            DrawPoiEffectsFields(poi);
                    }
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
            poi.is_hero = EditorGUILayout.Toggle("Is hero", poi.is_hero);

            // Hero icon override (section 13.6/21) -- shown only when is_hero is
            // checked. Uses the same assign-to-wall-library-and-get-key flow as
            // the category table. Setting it changes just this POI's icon; category
            // colour, ring, and badge stay unchanged.
            if (poi.is_hero)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Hero symbol (optional)", GUILayout.MinWidth(130f));
                    Sprite current = ResolveSpriteForKey(poi.hero_icon_key);
                    DrawSpritePreview(current);
                    Sprite chosen = (Sprite)EditorGUILayout.ObjectField(current, typeof(Sprite), false, GUILayout.MinWidth(140f));
                    if (chosen != current)
                        poi.hero_icon_key = chosen != null ? AssignSpriteToLibraryAndGetKey(chosen, poi.id + "_hero") : null;
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

                poi.status_unknown = EditorGUILayout.Toggle("Status unknown", poi.status_unknown);
            }

            poi.rotate_contour = EditorGUILayout.Toggle("Rotate contour", poi.rotate_contour);
        }

        private void DrawPoiEffectsFields(POIData poi)
        {
            var current = MarkerVisualsParser.ParseEffectFlags(poi.effect_mode);
            var tokens = new List<string>();

            if (EditorGUILayout.Toggle("Pulse", current.HasFlag(MarkerEffectFlags.Pulse))) tokens.Add("pulse");
            if (EditorGUILayout.Toggle("Sun Contours", current.HasFlag(MarkerEffectFlags.SunContours))) tokens.Add("sun_contours");
            if (EditorGUILayout.Toggle("Sun Circles", current.HasFlag(MarkerEffectFlags.SunCircles))) tokens.Add("sun_circles");
            if (EditorGUILayout.Toggle("Ring Pulse", current.HasFlag(MarkerEffectFlags.RingPulse))) tokens.Add("ring_pulse");
            if (EditorGUILayout.Toggle("Simple Sun", current.HasFlag(MarkerEffectFlags.SimpleSun))) tokens.Add("simple_sun");
            if (EditorGUILayout.Toggle("Beacon", current.HasFlag(MarkerEffectFlags.Beacon))) tokens.Add("beacon");

            poi.effect_mode = string.Join(",", tokens);
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
    }
}