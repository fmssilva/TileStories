using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    public partial class POIAuthoringToolWindow
    {
        private void DrawGlobalSceneOptions()
        {
            _showGlobalMarker = DrawFramedFoldout(ref _showGlobalMarker, DrawMarkerGlobalSection, "Marker", MarkerSectionColor);

            EditorGUILayout.Space(4f);

            _showGlobalBadge = DrawFramedFoldout(ref _showGlobalBadge, () =>
            {
                _config.marker_use_badge = EditorGUILayout.Toggle("Enable badge", _config.marker_use_badge);
                if (_config.marker_use_badge)
                    DrawGlobalBadgeSection();
                else
                    EditorGUILayout.HelpBox("Enable badge to edit badge symbol taxonomy.", MessageType.Info);
            }, "Badge", BadgeSectionColor);

            EditorGUILayout.Space(4f);

            _showGlobalOutline = DrawFramedFoldout(ref _showGlobalOutline, DrawGlobalOutlineSection, "Outline", OutlineSectionColor);

            EditorGUILayout.Space(4f);

            _showGlobalEffects = DrawFramedFoldout(ref _showGlobalEffects, DrawGlobalEffectsSection, "Effects", EffectsSectionColor);

            EditorGUILayout.Space(4f);

            _showGlobalHierarchy = DrawFramedFoldout(ref _showGlobalHierarchy, DrawGlobalHierarchySection, "Hierarchy Levels", HierarchySectionColor);

            EditorGUILayout.Space(4f);

            _showGlobalLod = DrawFramedFoldout(ref _showGlobalLod, DrawGlobalLodSection, "LOD", LodSectionColor);

            EditorGUILayout.Space(4f);

            _showGlobalZoom = DrawFramedFoldout(ref _showGlobalZoom, DrawGlobalZoomSection, "Zoom", ZoomSectionColor);

            EditorGUILayout.Space(4f);

            _showGlobalSearchFilter = DrawFramedFoldout(ref _showGlobalSearchFilter, DrawGlobalSearchFilterSection, "Search & Filter", SearchFilterSectionColor);
        }

        private void DrawMarkerGlobalSection()
        {
            int shapeIdx = Array.IndexOf(ShapeOptions, _config.marker_shape);
            if (shapeIdx < 0) shapeIdx = 0;
            shapeIdx = EditorGUILayout.Popup("Background shape", shapeIdx, ShapeLabels);
            _config.marker_shape = ShapeOptions[shapeIdx];

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Category Symbols", EditorStyles.boldLabel);

            DrawWallIconLibrarySelector();

            if (_config.category_styles == null)
                _config.category_styles = new List<CategoryStyleEntry>();

            // Seed defaults only if genuinely empty (section 13.2) -- not on every load.
            if (_config.category_styles.Count == 0)
                _config.category_styles.AddRange(DefaultCategoryStyles.Create());

            DrawSymbolTable(
                _config.category_styles,
                () => new CategoryStyleEntry { category = "new_category", icon_key = "unknown", color_hex = string.Empty },
                e => e.category,
                (e, v) => e.category = v,
                e => e.icon_key,
                (e, v) => e.icon_key = v,
                e => e.color_hex,
                (e, v) => e.color_hex = v,
                e => e.details,
                (e, v) => e.details = v,
                e => _config.marker_shape != "none",
                "+ Add category",
                "Category",
                true,
                e => e.search_keywords,
                (e, v) => e.search_keywords = v);
        }

        private void DrawGlobalBadgeSection()
        {
            if (_config.badge_categories == null)
                _config.badge_categories = new List<BadgeCategoryEntry>();

            // Badge background shape (section 13.3/20.2) -- independent of marker_shape.
            int badgeShapeIdx = Array.IndexOf(ShapeOptions, _config.badge_shape);
            if (badgeShapeIdx < 0) badgeShapeIdx = 0; // default to "circle"
            badgeShapeIdx = EditorGUILayout.Popup("Badge background shape", badgeShapeIdx, ShapeLabels);
            _config.badge_shape = ShapeOptions[badgeShapeIdx];

            // Seed defaults only if genuinely empty (section 13.2) -- not on every load.
            if (_config.badge_categories.Count == 0)
                _config.badge_categories.AddRange(DefaultBadgeCategories.Create());

            DrawSymbolTable(
                _config.badge_categories,
                () => new BadgeCategoryEntry { key = "new_badge", label = "New Badge", icon_key = "unknown", color_hex = "#B3B3B3" },
                e => e.key,
                (e, v) => e.key = v,
                e => e.icon_key,
                (e, v) => e.icon_key = v,
                e => e.color_hex,
                (e, v) => e.color_hex = v,
                e => e.details,
                (e, v) => e.details = v,
                e => _config.badge_shape != "none",
                "+ Add badge category",
                "Badge Key",
                true,
                e => e.search_keywords,
                (e, v) => e.search_keywords = v);
        }

        private void DrawGlobalOutlineSection()
        {
            bool useOutline = !string.Equals(_config.marker_outline_mode, "none", StringComparison.OrdinalIgnoreCase);
            useOutline = EditorGUILayout.Toggle("Enable outline", useOutline);

            if (!useOutline)
            {
                _config.marker_outline_mode = "none";
                EditorGUILayout.HelpBox("Outline disabled. Outline levels are ignored at runtime.", MessageType.Info);
                return;
            }

            // "free_colors" is an editor-only mode that controls whether the color
            // column is shown in the outline table. It is not a runtime MarkerOutlineMode
            // enum value -- at runtime, free_colors just means the per-level color_hex
            // values from config are used directly (StatusRamp.Configure already handles
            // this). So we handle it here in the editor normalization, not in
            // MarkerVisualsParser.TryParseOutlineMode.
            string normalizedOutlineMode;
            if (_config.marker_outline_mode == "free_colors")
            {
                normalizedOutlineMode = "free_colors";
            }
            else
            {
                normalizedOutlineMode = MarkerVisualsParser.TryParseOutlineMode(_config.marker_outline_mode, out var parsedOutlineMode)
                    ? (parsedOutlineMode == MarkerOutlineMode.SameHue ? "same_hue" : "gold")
                    : "gold";
            }
            int idx = Array.IndexOf(OutlineModeOptions, normalizedOutlineMode);
            if (idx < 0) idx = 0;
            idx = EditorGUILayout.Popup("Outline Color", idx, OutlineModeLabels);
            _config.marker_outline_mode = OutlineModeOptions[idx];

            bool isFreeColors = _config.marker_outline_mode == "free_colors";

            if (_config.outline_levels == null)
                _config.outline_levels = new List<OutlineLevelEntry>();

            EditorGUILayout.Space(4f);

            // Column headers for the outline table (section 13.4).
            EditorGUILayout.LabelField("Outline Types", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Each row is one discrete outline type a POI can be set to (e.g. \"Intact\", \"25% damaged\"). " +
                "To add a custom line (e.g. a wavy or double line): import a transparent PNG ring/dash pattern " +
                "as a Sprite, then use the Type column the same way as a marker/badge symbol.",
                MessageType.Info);

            // Seed defaults only if genuinely empty (section 13.2) -- a brand-new wall,
            // not one that already has entries the developer chose.
            if (_config.outline_levels.Count == 0)
                _config.outline_levels.AddRange(DefaultOutlineLevels.Create());

            // Column headers: Outline key | Details | Type | Preview | Color | Search Keywords | Remove
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Outline key", EditorStyles.miniBoldLabel, GUILayout.Width(110f));

                EditorGUILayout.LabelField("Details", EditorStyles.miniBoldLabel, GUILayout.Width(26f));
                EditorGUILayout.LabelField("Type", EditorStyles.miniBoldLabel, GUILayout.Width(140f));
                EditorGUILayout.LabelField("Preview", EditorStyles.miniBoldLabel, GUILayout.Width(44f));
                if (isFreeColors)
                    EditorGUILayout.LabelField("Color", EditorStyles.miniBoldLabel, GUILayout.Width(152f));
                EditorGUILayout.LabelField("Search Keywords", EditorStyles.miniBoldLabel);
                EditorGUILayout.LabelField("", GUILayout.Width(26f)); // Remove (trash)
            }

            for (int i = 0; i < _config.outline_levels.Count; i++)
            {
                var entry = _config.outline_levels[i] ?? new OutlineLevelEntry();
                using (new EditorGUILayout.HorizontalScope())
                {
                    // Outline key: text field for the label
                    entry.label = EditorGUILayout.TextField(entry.label, GUILayout.Width(110f));

                    // Details: popup for free-text notes (same pattern as DrawSymbolTable)
                    if (GUILayout.Button("...", GUILayout.Width(26f)))
                        PopupWindow.Show(GUILayoutUtility.GetLastRect(), new EntryDetailsPopup(entry.label ?? "Outline level", () => entry.details, v => entry.details = v));

                    // Type: sprite picker with auto-register
                    Sprite current = ResolveSpriteForKey(entry.line_style);
                    Sprite chosen = (Sprite)EditorGUILayout.ObjectField(current, typeof(Sprite), false, GUILayout.Width(140f));
                    if (chosen != current)
                        entry.line_style = AssignSpriteToLibraryAndGetKey(chosen, entry.label);

                    // Preview: thumbnail of the chosen sprite (separate from the ObjectField)
                    DrawSpritePreview(chosen != null ? chosen : current);

                    // Color swatch + hex -- only in Free Colors mode.
                    // Gold/Same Hue modes derive colours from StatusRamp at runtime.
                    if (isFreeColors)
                    {
                        string colorHex = entry.color_hex;
                        DrawColorSwatchAndHex(ref colorHex);
                        entry.color_hex = colorHex;
                    }

                    // Search keywords column
                    entry.search_keywords = DrawKeywordListField(entry.search_keywords);

                    // Remove button
                    if (GUILayout.Button(TrashIcon, GUILayout.Width(26f), GUILayout.Height(22f)))
                    {
                        _config.outline_levels.RemoveAt(i);
                        RecomputeLevelPercentSpacing(_config.outline_levels);
                        i--;
                        continue;
                    }
                }

                entry.key = string.IsNullOrWhiteSpace(entry.key) ? $"level_{i + 1}" : entry.key;
                _config.outline_levels[i] = entry;
            }

            // No cap on number of outline rows -- developers may add as many as needed.
            if (GUILayout.Button("+ Add outline level"))
            {
                _config.outline_levels.Add(new OutlineLevelEntry
                {
                    key = "level_" + (_config.outline_levels.Count + 1),
                    label = "Level " + (_config.outline_levels.Count + 1),
                    line_style = "solid",
                    color_hex = string.Empty
                });
                RecomputeLevelPercentSpacing(_config.outline_levels);
            }
        }

        // Auto-space pct whenever the list changes (section 13.4).
        private static void RecomputeLevelPercentSpacing(List<OutlineLevelEntry> levels)
        {
            if (levels == null || levels.Count == 0) return;
            if (levels.Count == 1) { levels[0].pct = 0f; return; }

            for (int i = 0; i < levels.Count; i++)
                levels[i].pct = (100f / (levels.Count - 1)) * i;
        }

        // Global effect defaults editor (section 19). Lets the wall developer tune
        // the amplitude, period, and colour of each effect type from config.json
        // rather than recompiling. Per-POI effect selection (which effects are
        // active) is handled in the per-marker "Effects" foldout; this section
        // only controls the shared parameters those active effects use.
        private void DrawGlobalEffectsSection()
        {
            // Lazily ensure the nested EffectDefaults objects exist so the property
            // drawers below never hit a null reference.
            EnsureEffectDefaultsExist();

            EditorGUILayout.HelpBox(
                "These defaults control the look and timing of each effect type. " +
                "Per-POI effect selection (which effects are active on a given marker) " +
                "is set in each marker's 'Effects' foldout below. Changes here apply to " +
                "all markers using the corresponding effect on this wall.",
                MessageType.Info);

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Available Effects", EditorStyles.boldLabel);

            EditorGUILayout.LabelField("Pulse - Gentle scale breathing", EditorStyles.miniBoldLabel);
            EditorGUILayout.LabelField("Sun Contours / Sun Circles - Three concentric waves, center-first flow", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("Ring Pulse - Thin contour, breathing", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("Simple Sun - Filled disc, breathing", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("Beacon - Thin contour, grow+fade sawtooth", EditorStyles.miniLabel);

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Pulse Defaults", EditorStyles.boldLabel);
            var pulse = _config.effect_defaults.pulse;
            pulse.amplitude = EditorGUILayout.Slider("Amplitude", pulse.amplitude, 0f, 0.45f);
            pulse.period = EditorGUILayout.FloatField("Period (s)", pulse.period);

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Sun Defaults", EditorStyles.boldLabel);
            var sun = _config.effect_defaults.sun;
            sun.period = EditorGUILayout.FloatField("Period (s)", sun.period);
            sun.stagger = EditorGUILayout.Slider("Stagger", sun.stagger, 0f, 0.25f);
            sun.innerAlpha = EditorGUILayout.Slider("Inner alpha", sun.innerAlpha, 0f, 1f);
            sun.middleAlpha = EditorGUILayout.Slider("Middle alpha", sun.middleAlpha, 0f, 1f);
            sun.outerAlpha = EditorGUILayout.Slider("Outer alpha", sun.outerAlpha, 0f, 1f);
            string sunTint = sun.tint_color_hex;
            DrawColorSwatchAndHex(ref sunTint);
            sun.tint_color_hex = sunTint;

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Accent Defaults", EditorStyles.boldLabel);
            var accent = _config.effect_defaults.accent;
            accent.size = EditorGUILayout.Slider("Size", accent.size, 0f, 1f);
            accent.baseAlpha = EditorGUILayout.Slider("Base alpha", accent.baseAlpha, 0f, 1f);
            accent.contourOuterScale = EditorGUILayout.Slider("Contour outer scale", accent.contourOuterScale, 0.72f, 0.98f);
            accent.contourInnerScale = EditorGUILayout.Slider("Contour inner scale", accent.contourInnerScale, 0.5f, 0.9f);
            accent.filledRadiusScale = EditorGUILayout.Slider("Filled radius scale", accent.filledRadiusScale, 0.85f, 1f);
            accent.breatheAmplitude = EditorGUILayout.Slider("Breathe amplitude", accent.breatheAmplitude, 0f, 0.4f);
            accent.period = EditorGUILayout.FloatField("Period (s)", accent.period);
            accent.beaconStartScale = EditorGUILayout.FloatField("Beacon start scale", accent.beaconStartScale);
            accent.beaconEndScale = EditorGUILayout.FloatField("Beacon end scale", accent.beaconEndScale);
            string accentTint = accent.tint_color_hex;
            DrawColorSwatchAndHex(ref accentTint);
            accent.tint_color_hex = accentTint;

            EditorGUILayout.Space(8f);
            EditorGUILayout.HelpBox(
                "To add a new marker effect: create a C# class inheriting MarkerEffect, " +
                "add a flag to MarkerEffectFlags, update MarkerVisualsParser.ParseEffectFlags, " +
                "wire it in MarkerView.ApplyHeroState, add a toggle in DrawPoiEffectsFields, " +
                "and add a gallery entry in MarkerGalleryDefinitions.",
                MessageType.Info);

            _hasUnsavedChanges = true;
        }

        private void DrawGlobalHierarchySection()
        {
            if (_config.hierarchy_levels == null)
                _config.hierarchy_levels = new List<HierarchyLevelEntry>();

            EditorGUILayout.LabelField("Hierarchy Levels", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Wall-configurable per-POI hierarchy levels. Each row drives one POI marker's " +
                "size, label visibility, effect combination, outline rotation, reveal delay, " +
                "and reveal duration. An empty table means all POIs fall through to the " +
                "framework default (12cm, no label, no effects, 0.35s reveal).",
                MessageType.Info);

            int count = _config.hierarchy_levels.Count;
            if (count == 0)
            {
                EditorGUILayout.HelpBox("No hierarchy levels defined. Add at least one row to enable hierarchy-based marker sizing.", MessageType.Info);
            }

            for (int i = 0; i < count; i++)
            {
                var entry = _config.hierarchy_levels[i] ?? new HierarchyLevelEntry();
                using (new EditorGUILayout.HorizontalScope())
                {
                    // Column 1: Label (text field)
                    entry.label = EditorGUILayout.TextField(entry.label, GUILayout.Width(100f));

                    // Column 2: Priority (int) + info button
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        entry.priority = EditorGUILayout.IntField(entry.priority, GUILayout.Width(40f));
                        HelpInfoButton.Draw("Priority",
                            "Sort key for draw order + count-cap survival (lower = higher priority). " +
                            "Explicit value >= 1 is author order; leave 0 to fall back to this row's " +
                            "1-based position. Duplicates are legal; magnitude is a pure sort key.");
                    }

                    // Column 3: Details (...) - reuses EntryDetailsPopup exactly as-is
                    if (GUILayout.Button("...", GUILayout.Width(26f), GUILayout.Height(22f)))
                        PopupWindow.Show(GUILayoutUtility.GetLastRect(), new EntryDetailsPopup(
                            entry.label ?? "Hierarchy level", () => entry.details, v => entry.details = v));

                    // Column 4: Size (cm) + soft sanity warning + info button.
                    // Soft, not a hard clamp -- a genuinely large mural marker may need
                    // a value outside 0.5-100cm, so we warn but never block the author.
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        entry.size_cm = EditorGUILayout.FloatField(entry.size_cm, GUILayout.Width(50f));
                        if (entry.size_cm < 0.5f || entry.size_cm > 100f)
                        {
                            var warnContent = EditorGUIUtility.IconContent("console.warnicon.sml");
                            warnContent.tooltip = "Unusually large or small -- is this a cm/m typo? (0.5-100cm is the expected range)";
                            GUILayout.Label(warnContent, GUILayout.Width(20f), GUILayout.Height(18f));
                        }
                        HelpInfoButton.Draw("Size (cm)",
                            "Real-world printed size of the marker Symbol. This is not yet adjusted for viewing distance -- that's a separate future feature.");
                    }

                    // Column 5: Show Label (dropdown, explicit wording per Â§6)
                    int showLabelIdx = entry.show_label ? 0 : 1;
                    showLabelIdx = EditorGUILayout.Popup("Show Label", showLabelIdx, ShowLabelOptions, GUILayout.Width(130f));
                    entry.show_label = showLabelIdx == 0;

                    // Column 6: Sun Effect (dropdown, mutually exclusive)
                    int sunIdx = Array.IndexOf(SunEffectOptions, entry.sun_effect);
                    if (sunIdx < 0) sunIdx = 0;
                    sunIdx = EditorGUILayout.Popup("Sun", sunIdx, SunEffectLabels, GUILayout.Width(110f));
                    entry.sun_effect = SunEffectOptions[sunIdx];

                    // Column 7: Accent Effect (dropdown, mutually exclusive)
                    int accentIdx = Array.IndexOf(AccentEffectOptions, entry.accent_effect);
                    if (accentIdx < 0) accentIdx = 0;
                    accentIdx = EditorGUILayout.Popup("Accent", accentIdx, AccentEffectLabels, GUILayout.Width(120f));
                    entry.accent_effect = AccentEffectOptions[accentIdx];

                    // Column 8: Pulse (checkbox, standalone boolean)
                    entry.pulse = EditorGUILayout.Toggle("Pulse", entry.pulse, GUILayout.Width(70f));

                    // Column 9: Rotate Contour (checkbox, outline-gated)
                    bool outlineEnabled = !string.Equals(_config.marker_outline_mode, "none", StringComparison.OrdinalIgnoreCase);
                    if (outlineEnabled)
                    {
                        entry.rotate_contour = EditorGUILayout.Toggle("Rotate", entry.rotate_contour, GUILayout.Width(70f));
                    }

                                        // Column 10: Reveal Delay (s) + Duration (s) + info button
                    entry.reveal_delay_s = EditorGUILayout.FloatField(entry.reveal_delay_s, GUILayout.Width(50f));
                    entry.reveal_duration_s = EditorGUILayout.FloatField(entry.reveal_duration_s, GUILayout.Width(50f));
                    HelpInfoButton.Draw("Reveal Delay vs Duration",
                        "Delay: seconds after spawn before the fade/scale-in begins.\nDuration: how long the fade/scale-in animation itself takes. A longer delay staggers appearance; a longer duration makes each marker enter more slowly. Default: 0.5s L1 -> 0.25s L5.");

                    // Column 11: Remove (trash button)
                    if (GUILayout.Button(TrashIcon, GUILayout.Width(26f), GUILayout.Height(22f)))
                    {
                        _config.hierarchy_levels.RemoveAt(i);
                        i--;
                        count--;
                        _hasUnsavedChanges = true;
                        continue;
                    }
                }

                entry.key = string.IsNullOrWhiteSpace(entry.key) ? $"level_{i + 1}" : entry.key;
                _config.hierarchy_levels[i] = entry;
            }

            EditorGUILayout.Space(4f);
            if (GUILayout.Button("+ Add hierarchy level"))
            {
                                _config.hierarchy_levels.Add(new HierarchyLevelEntry
                {
                    key = "level_" + (_config.hierarchy_levels.Count + 1),
                    label = (_config.hierarchy_levels.Count + 1).ToString(),
                    priority = _config.hierarchy_levels.Count + 1,
                    size_cm = 12f,
                    show_label = false,
                    sun_effect = "none",
                    accent_effect = "none",
                    pulse = false,
                    rotate_contour = false,
                    reveal_delay_s = 0f,
                    reveal_duration_s = 0.35f
                });
                _hasUnsavedChanges = true;
            }

            _hasUnsavedChanges = true;
        }

        private void EnsureEffectDefaultsExist()
        {
            if (_config.effect_defaults == null)
            {
                _config.effect_defaults = new EffectDefaults();
                _hasUnsavedChanges = true;
            }
        }

        // ---- Search & Filter foldout (Block 5, Phase 5.1) ----
        // Exposes ONLY config fields that are read AND consumed at runtime (D1).
        // Organized into labelled subgroups: Search / Voice / Recent & Suggested /
        // Results & Navigation / Selection & Zoom / Ranking & Weights (conditional).
        private void DrawGlobalSearchFilterSection()
        {
            if (_config == null)
                return;

            // --- Keyword Fields table ---
            EditorGUILayout.Space(2f);
            EditorGUILayout.HelpBox(SearchFieldsTableHelp, MessageType.None);
            EditorGUILayout.Space(4f);
            DrawSearchFieldsTable();

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Search", EditorStyles.boldLabel);
            _config.search_mode = DrawPopupField("Search mode", _config.search_mode,
                SearchModeOptions, SearchModeLabels, SearchModeHelp);

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Results & Navigation", EditorStyles.boldLabel);
            _config.no_results_message = EditorGUILayout.TextField("No-results message", _config.no_results_message);
            _config.default_result_view = DrawPopupField("Default result view", _config.default_result_view,
                ResultViewOptions, ResultViewLabels, ResultViewHelp);

            EditorGUILayout.Space(3f);
            EditorGUILayout.LabelField("Minimap", EditorStyles.miniLabel);
            _config.minimap_enabled = DrawToggleField("Enable minimap", _config.minimap_enabled, MinimapHelp);
            if (_config.minimap_enabled)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    _config.minimap_visibility = DrawPopupField("Visibility", _config.minimap_visibility,
                        MinimapVisibilityOptions, MinimapVisibilityLabels, MinimapVisibilityHelp);
                    _config.minimap_icon_style = DrawPopupField("Icon style", _config.minimap_icon_style,
                        MinimapIconOptions, MinimapIconLabels, MinimapIconHelp);
                }
            }

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Recent & Suggested", EditorStyles.boldLabel);
            _config.recent_search_count = DrawIntField("Recent search count", _config.recent_search_count, RecentCountHelp);
            _config.show_suggested_categories = DrawToggleField("Show suggested categories", _config.show_suggested_categories, SuggestedHelp);
            _config.suggested_source = DrawPopupField("Suggested source", _config.suggested_source,
                SuggestedSourceOptions, SuggestedSourceLabels, SuggestedSourceHelp);

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Voice", EditorStyles.boldLabel);
            _config.voice_search_enabled = DrawToggleField("Enable voice search", _config.voice_search_enabled, VoiceEnabledHelp);
            if (_config.voice_search_enabled)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    _config.voice_search_match_mode = DrawPopupField("Match mode", _config.voice_search_match_mode,
                        VoiceMatchModeOptions, VoiceMatchModeLabels, VoiceMatchModeHelp);
                    _config.voice_activity_indicator_style = DrawPopupField("Indicator style", _config.voice_activity_indicator_style,
                        VoiceIndicatorOptions, VoiceIndicatorLabels, VoiceIndicatorHelp);
                }
            }

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Selection & Zoom", EditorStyles.boldLabel);
            _config.selection_highlight_enabled = DrawToggleField("Selection highlight", _config.selection_highlight_enabled, SelectionHighlightHelp);
            _config.zoom_on_select_enabled = DrawToggleField("Zoom on select", _config.zoom_on_select_enabled, ZoomOnSelectHelp);
            if (_config.zoom_on_select_enabled)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    _config.zoom_on_select_trigger = (WallConfigData.ZoomOnSelectTrigger)EditorGUILayout.EnumPopup("Trigger target", _config.zoom_on_select_trigger);
                    _config.zoom_on_select_density_threshold = DrawIntField("Density threshold", _config.zoom_on_select_density_threshold, ZoomOnSelectDensityHelp);
                    _config.zoom_on_select_factor = DrawScalarField("Zoom factor", _config.zoom_on_select_factor, ZoomOnSelectFactorHelp);
                }
            }

            _hasUnsavedChanges = true;
        }

        // Renders the custom keyword field definitions table.
        // Three row types: read-only system rows (category/hierarchy/badge/outline),
        // editable custom rows (from search_fields), and a permanent "Others" row.
        private void DrawSearchFieldsTable()
        {
            if (_config.search_fields == null)
                _config.search_fields = new List<SearchFieldDefinition>();

            // Column headers.
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Key", EditorStyles.miniBoldLabel, GUILayout.Width(90f));
                EditorGUILayout.LabelField("Label", EditorStyles.miniBoldLabel, GUILayout.Width(100f));
                EditorGUILayout.LabelField("Forced", EditorStyles.miniBoldLabel, GUILayout.Width(50f));
                EditorGUILayout.LabelField("Details", EditorStyles.miniBoldLabel, GUILayout.Width(50f));
                GUILayout.FlexibleSpace();
            }

            // Read-only system rows (derived from taxonomy tables, shown for context).
            DrawSystemKeywordRow("category", "Category");
            DrawSystemKeywordRow("hierarchy", "Hierarchy Level");
            DrawSystemKeywordRow("badge", "Badge");
            DrawSystemKeywordRow("outline", "Outline / Status");

            // Editable custom rows.
            for (int i = 0; i < _config.search_fields.Count; i++)
            {
                var field = _config.search_fields[i];
                using (new EditorGUILayout.HorizontalScope())
                {
                    // Key (editable but warn if changed after first authoring session).
                    string newKey = EditorGUILayout.TextField(field.key ?? string.Empty, GUILayout.Width(90f));
                    if (newKey != field.key)
                    {
                        field.key = newKey;
                        _hasUnsavedChanges = true;
                    }

                    // Label.
                    string newLabel = EditorGUILayout.TextField(field.label ?? string.Empty, GUILayout.Width(100f));
                    if (newLabel != field.label)
                    {
                        field.label = newLabel;
                        _hasUnsavedChanges = true;
                    }

                    // Forced toggle.
                    bool newForced = EditorGUILayout.Toggle(field.forced, GUILayout.Width(50f));
                    if (newForced != field.forced)
                    {
                        field.forced = newForced;
                        _hasUnsavedChanges = true;
                    }

                    // Details popup + help.
                    using (new EditorGUILayout.HorizontalScope(GUILayout.Width(76f)))
                    {
                        if (GUILayout.Button("...", GUILayout.Width(26f)))
                            PopupWindow.Show(GUILayoutUtility.GetLastRect(), new EntryDetailsPopup(
                                field.label ?? field.key ?? "Field",
                                () => field.details,
                                v => { field.details = v; _hasUnsavedChanges = true; }));

                        if (GUILayout.Button("?", GUILayout.Width(26f)))
                            PopupWindow.Show(GUILayoutUtility.GetLastRect(), new HelpInfoPopup(
                                "Search Field Help",
                                $"Key: {SearchFieldKeyHelp}\n\nLabel: {SearchFieldLabelHelp}\n\nForced: {SearchFieldForcedHelp}\n\nDetails: {SearchFieldDetailsHelp}"));
                    }

                    GUILayout.FlexibleSpace();

                    // Remove button.
                    if (GUILayout.Button(TrashIcon, GUILayout.Width(26f), GUILayout.Height(22f)))
                    {
                        _config.search_fields.RemoveAt(i);
                        _hasUnsavedChanges = true;
                        break;
                    }
                }
            }

            // Permanent read-only "Others" row (the flat search_keywords bucket).
            DrawSystemKeywordRow("others", "Others (freeform)");

            // Add new field button.
            EditorGUILayout.Space(2f);
            if (GUILayout.Button("+ Add keyword field", GUILayout.Width(160f)))
            {
                _config.search_fields.Add(new SearchFieldDefinition
                {
                    key = "field_" + (_config.search_fields.Count + 1),
                    label = "New Field",
                    forced = false,
                    details = string.Empty
                });
                _hasUnsavedChanges = true;
            }
        }

        // Renders one read-only labelled row in the keyword fields table.
        private static void DrawSystemKeywordRow(string key, string label)
        {
            using (new EditorGUI.DisabledScope(true))
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.TextField(key, GUILayout.Width(90f));
                EditorGUILayout.TextField(label, GUILayout.Width(100f));
                EditorGUILayout.Toggle(false, GUILayout.Width(50f)); // forced always false for system rows
                GUILayout.Button("...", GUILayout.Width(26f));
            }
        }
    }
}
