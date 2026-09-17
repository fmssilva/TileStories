using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    public partial class POIEditorToolWindow
    {
        private void DrawGlobalSceneOptions()
        {
            _showGlobalMarker = DrawFramedFoldout(ref _showGlobalMarker, DrawMarkerGlobalSection, "Marker", MarkerSectionColor);

            EditorGUILayout.Space(4f);

            _showGlobalBadge = DrawFramedFoldout(ref _showGlobalBadge, () =>
            {
                // Shared row: transparent indent spacer + labelled Toggle capped to rowWidth.
                DrawEditorRow(out float badgeRowWidth, out _);
                _config.marker_use_badge = EditorGUILayout.Toggle("Enable badge", _config.marker_use_badge,
                    GUILayout.Width(badgeRowWidth), GUILayout.ExpandWidth(false));
                EditorRowEnd();
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

            _showGlobalDisplacement = DrawFramedFoldout(ref _showGlobalDisplacement, DrawGlobalDisplacementSection, "Displacement", DisplacementSectionColor);

            EditorGUILayout.Space(4f);

            _showGlobalSearchFilter = DrawFramedFoldout(ref _showGlobalSearchFilter, DrawGlobalSearchFilterSection, "Search & Filter", SearchFilterSectionColor);
        }

        private void DrawMarkerGlobalSection()
        {
            int shapeIdx = Array.IndexOf(ShapeOptions, _config.marker_shape);
            if (shapeIdx < 0) shapeIdx = 0;
            // Shared row: transparent indent spacer + labelled Popup capped to rowWidth.
            DrawEditorRow(out float shapeRowWidth, out _);
            shapeIdx = EditorGUILayout.Popup("Background shape", shapeIdx, ShapeLabels,
                GUILayout.Width(shapeRowWidth), GUILayout.ExpandWidth(false));
            EditorRowEnd();
            _config.marker_shape = ShapeOptions[shapeIdx];

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Category Symbols", EditorStyles.boldLabel);

            DrawWallIconLibrarySelector();

            if (_config.category_styles == null)
                _config.category_styles = new List<CategoryStyleEntry>();

            // Seed defaults only if genuinely empty (section 13.2) -- not on every load.
            if (_config.category_styles.Count == 0)
                _config.category_styles.AddRange(DefaultCategoryStyles.Create());

            // Resolve Enter/ESC for an in-progress rename BEFORE the shared TextField
            // draws (Unity's TextField consumes the first Return when it sees it --
            // the same "press Enter twice" trap PoiRenameKeys was built to avoid).
            // Click-outside also commits the draft.
            CategoryRenameEdit.HandleEditEvents();

            DrawSymbolTable(
                _config.category_styles,
                () => new CategoryStyleEntry { category = "new_category", icon_key = "unknown", color_hex = string.Empty },
                CategoryRenameEdit.GetLabel,
                CategoryRenameEdit.SetLabel,
                e => e.icon_key,
                (e, v) => e.icon_key = v,
                e => e.color_hex,
                (e, v) => e.color_hex = v,
                e => e.details,
                (e, v) => e.details = v,
                e => _config.marker_shape != "none",
                "+ Add category",
                "Category",
                "Write more information about this category here: what it represents, when to use it, example POIs. Stored per row in config.json.",
                "SYMBOL (ObjectField cell): lists every Sprite in the whole project. CLICK THE THUMBNAIL PREVIEW: opens the curated picker narrowed to just this wall's symbols plus the framework defaults. All cells write to the same field.\n\nTO ADD YOUR OWN IMAGE: drop a PNG anywhere under this wall's Assets/Apps/<Wall>/MarkerAssets/ folder -- Unity auto-imports it as a Sprite. Then pick it from either list; it registers into this wall's icon library automatically. Same flow for badges and outline ring art.",
                true,
                e => e.search_keywords,
                (e, v) => e.search_keywords = v,
                // Deleting a category POIs still reference cannot propagate anywhere,
                // so ask first and say how many POIs would be orphaned.
                entry => IdentityRenameResolver.CountReferences(
                    _config.pois, IdentityRenameResolver.PoiUsesCategory, entry.category));
        }

        // ---- Commit-style identity renames (category + badge key) ----
        // In both tables the primary cell IS the identity their POIs reference, so
        // renaming a row must propagate to every POI holding the old string.
        // Editing is buffered in SessionState: keystrokes only update the draft,
        // the row's identity stays pristine until Commit writes it once and
        // IdentityRenameResolver rewrites the POIs. ESC discards the draft (no
        // change); Enter or a click outside the field commits the final word,
        // mirroring the PoiRenameKeys pattern the POI header rename uses.
        //
        // Each state object is built once and reused. It reads rows/POIs through
        // providers, so it keeps pointing at the live _config even after undo
        // replaces it wholesale (ApplyConfigSnapshot) or a reload swaps in a fresh
        // WallConfigData -- a captured list reference would silently detach.
        private IdentityRenameEditState<CategoryStyleEntry> _categoryRenameEdit;
        private IdentityRenameEditState<BadgeCategoryEntry> _badgeRenameEdit;

        private IdentityRenameEditState<CategoryStyleEntry> CategoryRenameEdit =>
            _categoryRenameEdit ??= new IdentityRenameEditState<CategoryStyleEntry>(
                "TileStories.CategoryEdit",
                () => _config?.category_styles,
                e => e.category,
                (e, v) => e.category = v,
                () => _config?.pois,
                IdentityRenameResolver.CategoryRewrite);

        private IdentityRenameEditState<BadgeCategoryEntry> BadgeRenameEdit =>
            _badgeRenameEdit ??= new IdentityRenameEditState<BadgeCategoryEntry>(
                "TileStories.BadgeEdit",
                () => _config?.badge_categories,
                e => e.key,
                (e, v) => e.key = v,
                () => _config?.pois,
                IdentityRenameResolver.BadgeKeyRewrite);

        // The get/set pair for the primary cell now lives on IdentityRenameEditState:
        // it owns the "is this keystroke a draft or the committed word?" decision, so
        // both tables call CategoryRenameEdit.GetLabel / .SetLabel directly instead of
        // each partial re-implementing the SessionState dance.

        private void DrawGlobalBadgeSection()
        {
            if (_config.badge_categories == null)
                _config.badge_categories = new List<BadgeCategoryEntry>();

            // Badge background shape (section 13.3/20.2) -- independent of marker_shape.
            int badgeShapeIdx = Array.IndexOf(ShapeOptions, _config.badge_shape);
            if (badgeShapeIdx < 0) badgeShapeIdx = 0; // default to "circle"
            // Shared row: transparent indent spacer + labelled Popup capped to rowWidth.
            DrawEditorRow(out float badgeShapeRow, out _);
            badgeShapeIdx = EditorGUILayout.Popup("Badge background shape", badgeShapeIdx, ShapeLabels,
                GUILayout.Width(badgeShapeRow), GUILayout.ExpandWidth(false));
            EditorRowEnd();
            _config.badge_shape = ShapeOptions[badgeShapeIdx];

            // Seed defaults only if genuinely empty (section 13.2) -- not on every load.
            if (_config.badge_categories.Count == 0)
                _config.badge_categories.AddRange(DefaultBadgeCategories.Create());

            // Same commit-style rename protection the category table has: the badge
            // key IS the identity POIData.badge_category references, so a raw
            // keystroke straight into it would orphan every POI that uses this badge
            // (badge icon lost, badge keyword index lost, Badges facet broken).
            // Resolve Enter/ESC before the shared TextField draws -- see the
            // PoiRenameKeys note on Unity's TextField eating the first Return.
            BadgeRenameEdit.HandleEditEvents();

            DrawSymbolTable(
                _config.badge_categories,
                () => new BadgeCategoryEntry { key = "new_badge", label = "New Badge", icon_key = "unknown", color_hex = "#B3B3B3" },
                BadgeRenameEdit.GetLabel,
                BadgeRenameEdit.SetLabel,
                e => e.icon_key,
                (e, v) => e.icon_key = v,
                e => e.color_hex,
                (e, v) => e.color_hex = v,
                e => e.details,
                (e, v) => e.details = v,
                e => _config.badge_shape != "none",
                "+ Add badge category",
                "Badge Key",
                "Write more information about this badge here: what it represents, when to use it, example POIs. Stored per row in config.json.",
                "SYMBOL (ObjectField cell): lists every Sprite in the whole project. CLICK THE THUMBNAIL PREVIEW: opens the curated picker narrowed to just this wall's symbols plus the framework defaults. All cells write to the same field.\n\nTO ADD YOUR OWN IMAGE: drop a PNG anywhere under this wall's Assets/Apps/<Wall>/MarkerAssets/ folder -- Unity auto-imports it as a Sprite. Then pick it from either list; it registers into this wall's icon library automatically. Same flow for badges and outline ring art.",
                true,
                e => e.search_keywords,
                (e, v) => e.search_keywords = v,
                // Deleting a badge key POIs still reference cannot propagate anywhere,
                // so ask first and say how many POIs would be orphaned.
                entry => IdentityRenameResolver.CountReferences(
                    _config.pois, IdentityRenameResolver.PoiUsesBadgeKey, entry.key));
        }


        // How many POIs still name this outline level. Outline level keys are
        // identity (POIData.status_level_key stores them) but they are generated
        // (level_N), never typed, so they cannot be renamed out from under a POI --
        // deleting the row is the only way to orphan one, hence the guard below.
        private int CountPoisUsingStatusLevel(string key) =>
            IdentityRenameResolver.CountReferences(_config.pois, IdentityRenameResolver.PoiUsesStatusLevelKey, key);

        // Same for hierarchy levels: POIData.hierarchy_level_key stores the row key,
        // so removing a level in use drops those markers to the framework fallback.
        private int CountPoisUsingHierarchyLevel(string key) =>
            IdentityRenameResolver.CountReferences(_config.pois, IdentityRenameResolver.PoiUsesHierarchyLevelKey, key);

        private void DrawGlobalOutlineSection()
        {
            bool useOutline = !string.Equals(_config.marker_outline_mode, "none", StringComparison.OrdinalIgnoreCase);
            // Shared row: transparent indent spacer + labelled Toggle capped to rowWidth.
            DrawEditorRow(out float outlineEnableRowWidth, out _);
            useOutline = EditorGUILayout.Toggle("Enable outline", useOutline,
                GUILayout.Width(outlineEnableRowWidth), GUILayout.ExpandWidth(false));
            EditorRowEnd();

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
            // Shared row: transparent indent spacer + labelled Popup capped to rowWidth.
            DrawEditorRow(out float outlineColorRowWidth, out _);
            idx = EditorGUILayout.Popup("Outline Color", idx, OutlineModeLabels,
                GUILayout.Width(outlineColorRowWidth), GUILayout.ExpandWidth(false));
            EditorRowEnd();
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
                "as a Sprite, then use the Outline Style column the same way as a marker/badge symbol.",
                MessageType.Info);

            // Seed defaults only if genuinely empty (section 13.2) -- a brand-new wall,
            // not one that already has entries the developer chose.
            if (_config.outline_levels.Count == 0)
                _config.outline_levels.AddRange(DefaultOutlineLevels.Create());

            // Column headers (5 groups, header mirrors rows exactly):
            // [key+details] | [Style + interactive Preview] | [Color] | [SearchKeywords] | [trash]
            using (new EditorGUILayout.HorizontalScope())
            {
                // Group 1: key + notes info
                // The cell below edits entry.label, while entry.key (the identity
                // POIData.status_level_key references, e.g. "level_2") is generated
                // after the row loop. Header must say which one it is or developers
                // assume they are renaming the key.
                EditorGUILayout.LabelField("Outline label", EditorStyles.miniBoldLabel, GUILayout.Width(110f));
                GUILayout.Space(TableGapWithinGroup);
                HelpInfoButton.Draw("Outline Notes",
                    "Write more information about this outline type here: what it represents, when to use it, example POIs. Stored per row in config.json.");

                GUILayout.Space(TableGapBetweenGroups);

                // Group 2: Outline Style + interactive Preview info (preview = curated picker).
                EditorGUILayout.LabelField("Outline Style", EditorStyles.miniBoldLabel, GUILayout.Width(140f));
                GUILayout.Space(SymbolColumnPad);
                HelpInfoButton.Draw("Outline Style",
                    "STYLE (ObjectField cell): lists every Sprite in the whole project. " +
                    "CLICK THE THUMBNAIL PREVIEW: opens the curated picker narrowed to just this wall's symbols plus the " +
                    "framework defaults. All cells write to the same field.\n\n" +
                    "TO ADD YOUR OWN RING STYLE: drop a PNG anywhere under this wall's Assets/Apps/<Wall>/MarkerAssets/ " +
                    "folder -- Unity auto-imports it as a Sprite. Then pick it from either list; it registers into " +
                    "this wall's icon library automatically.", 36f);

                GUILayout.Space(TableGapBetweenGroups);

                // Group 3: Color (free-colors mode only). When hidden, no footprint
                // is reserved, mirroring the rows -- so the gap before keywords stays
                // the only spacer either way and header never drifts from rows.
                if (isFreeColors)
                    EditorGUILayout.LabelField("Color", EditorStyles.miniBoldLabel, GUILayout.Width(ColorGroupWidth));

                // Group 4: Search keywords
                GUILayout.Space(TableGapBetweenGroups);
                EditorGUILayout.LabelField("Search Keywords", EditorStyles.miniBoldLabel);

                // Group 5: Remove (trash) -- last column, same between-groups gap.
                GUILayout.Space(TableGapBetweenGroups);
                EditorGUILayout.LabelField("", GUILayout.Width(26f)); // Remove (trash)
            }

            for (int i = 0; i < _config.outline_levels.Count; i++)
            {
                var entry = _config.outline_levels[i] ?? new OutlineLevelEntry();
                using (new EditorGUILayout.HorizontalScope())
                {
                    // Group 1: key + details
                    entry.label = EditorGUILayout.TextField(entry.label, GUILayout.Width(110f));

                    // Details: popup for free-text notes (same pattern as DrawSymbolTable).
                    GUILayout.Space(TableGapWithinGroup);
                    if (GUILayout.Button(DetailsIcon, GUILayout.Width(26f), GUILayout.Height(20f)))
                        PopupWindow.Show(GUILayoutUtility.GetLastRect(), new EntryDetailsPopup(entry.label ?? "Outline level", () => entry.details, v => entry.details = v));

                    GUILayout.Space(TableGapBetweenGroups);

                    // Group 2: Outline Style + interactive Preview (the preview IS
                    // the curated "choose" affordance -- no separate select button).
                    Sprite current = ResolveSpriteForKey(entry.line_style);
                    Sprite chosen = (Sprite)EditorGUILayout.ObjectField(current, typeof(Sprite), false, GUILayout.Width(140f));
                    if (chosen != current)
                        entry.line_style = AssignSpriteToLibraryAndGetKey(chosen, entry.label);

                    GUILayout.Space(TableGapWithinGroup);

                    // Clicking the preview opens the curated wall + framework picker
                    // (section 14.7). Capture per-iteration: PopupWindow.Show is async.
                    EnsureDefaultIconLibraryLoaded();
                    var targetEntry = entry;
                    DrawSpritePreview(chosen != null ? chosen : current,
                        () => PopupWindow.Show(GUILayoutUtility.GetLastRect(),
                            new ExistingSymbolPickerPopup(_wallIconLibrary, _defaultIconLibrary,
                                key => targetEntry.line_style = key)));

                    GUILayout.Space(TableGapBetweenGroups);

                    // Group 3: Color (free-colors mode only). The real picker is the
                    // first cell of this group -- above is the between-groups gap, and
                    // the hex field follows inside the group after the within-gap.
                    // Not reserved when hidden (mirrors the header), so the keywords gap
                    // below is the same between-groups gap in both modes.
                    if (isFreeColors)
                    {
                        string colorHex = entry.color_hex;
                        DrawColorSwatchAndHex(ref colorHex);
                        entry.color_hex = colorHex;
                    }

                    // Group 4: Search keywords
                    GUILayout.Space(TableGapBetweenGroups);
                    entry.search_keywords = DrawKeywordListField(entry.search_keywords);

                    // Group 5: Remove (trash) -- last column, same between-groups gap.
                    GUILayout.Space(TableGapBetweenGroups);
                    if (GUILayout.Button(TrashIcon, GUILayout.Width(26f), GUILayout.Height(22f)))
                    {
                        // Deleting a level cannot propagate to the POIs that name it,
                        // so confirm with a count when any still do (see IdentityDeleteGuard).
                        if (IdentityDeleteGuard.Confirm("Outline level", entry.key, CountPoisUsingStatusLevel(entry.key)))
                        {
                            _config.outline_levels.RemoveAt(i);
                            RecomputeLevelPercentSpacing(_config.outline_levels);
                            i--;
                            continue;
                        }
                    }
                }

                entry.key = string.IsNullOrWhiteSpace(entry.key) ? $"level_{i + 1}" : entry.key;
                _config.outline_levels[i] = entry;
            }

            // No cap on number of outline rows -- developers may add as many as needed.
            // Rendered as a shared editor row: transparent indent spacer + width
            // capped to max(MinRowWidth, min(visible panel, MaxRowWidth)).
            DrawEditorRow(out float rowWidth, out _);
            if (GUILayout.Button("+ Add outline level", GUILayout.Width(rowWidth), GUILayout.ExpandWidth(false)))
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
            EditorRowEnd();
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
            DrawEditorRow(out float pulseAmpRow, out _);
            pulse.amplitude = EditorGUILayout.Slider("Amplitude", pulse.amplitude, 0f, 0.45f,
                GUILayout.Width(pulseAmpRow), GUILayout.ExpandWidth(false));
            EditorRowEnd();
            DrawEditorRow(out float pulsePeriodRow, out _);
            pulse.period = EditorGUILayout.FloatField("Period (s)", pulse.period,
                GUILayout.Width(pulsePeriodRow), GUILayout.ExpandWidth(false));
            EditorRowEnd();

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Sun Defaults", EditorStyles.boldLabel);
            var sun = _config.effect_defaults.sun;
            DrawEditorRow(out float sunPeriodRow, out _);
            sun.period = EditorGUILayout.FloatField("Period (s)", sun.period,
                GUILayout.Width(sunPeriodRow), GUILayout.ExpandWidth(false));
            EditorRowEnd();
            DrawEditorRow(out float sunStaggerRow, out _);
            sun.stagger = EditorGUILayout.Slider("Stagger", sun.stagger, 0f, 0.25f,
                GUILayout.Width(sunStaggerRow), GUILayout.ExpandWidth(false));
            EditorRowEnd();
            DrawEditorRow(out float sunInnerRow, out _);
            sun.innerAlpha = EditorGUILayout.Slider("Inner alpha", sun.innerAlpha, 0f, 1f,
                GUILayout.Width(sunInnerRow), GUILayout.ExpandWidth(false));
            EditorRowEnd();
            DrawEditorRow(out float sunMiddleRow, out _);
            sun.middleAlpha = EditorGUILayout.Slider("Middle alpha", sun.middleAlpha, 0f, 1f,
                GUILayout.Width(sunMiddleRow), GUILayout.ExpandWidth(false));
            EditorRowEnd();
            DrawEditorRow(out float sunOuterRow, out _);
            sun.outerAlpha = EditorGUILayout.Slider("Outer alpha", sun.outerAlpha, 0f, 1f,
                GUILayout.Width(sunOuterRow), GUILayout.ExpandWidth(false));
            EditorRowEnd();
            string sunTint = sun.tint_color_hex;
            DrawColorSwatchAndHex(ref sunTint);
            sun.tint_color_hex = sunTint;

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Accent Defaults", EditorStyles.boldLabel);
            var accent = _config.effect_defaults.accent;
            DrawEditorRow(out float accentSizeRow, out _);
            accent.size = EditorGUILayout.Slider("Size", accent.size, 0f, 1f,
                GUILayout.Width(accentSizeRow), GUILayout.ExpandWidth(false));
            EditorRowEnd();
            DrawEditorRow(out float accentAlphaRow, out _);
            accent.baseAlpha = EditorGUILayout.Slider("Base alpha", accent.baseAlpha, 0f, 1f,
                GUILayout.Width(accentAlphaRow), GUILayout.ExpandWidth(false));
            EditorRowEnd();
            DrawEditorRow(out float accentOuterRow, out _);
            accent.contourOuterScale = EditorGUILayout.Slider("Contour outer scale", accent.contourOuterScale, 0.72f, 0.98f,
                GUILayout.Width(accentOuterRow), GUILayout.ExpandWidth(false));
            EditorRowEnd();
            DrawEditorRow(out float accentInnerRow, out _);
            accent.contourInnerScale = EditorGUILayout.Slider("Contour inner scale", accent.contourInnerScale, 0.5f, 0.9f,
                GUILayout.Width(accentInnerRow), GUILayout.ExpandWidth(false));
            EditorRowEnd();
            DrawEditorRow(out float accentFilledRow, out _);
            accent.filledRadiusScale = EditorGUILayout.Slider("Filled radius scale", accent.filledRadiusScale, 0.85f, 1f,
                GUILayout.Width(accentFilledRow), GUILayout.ExpandWidth(false));
            EditorRowEnd();
            DrawEditorRow(out float accentBreatheRow, out _);
            accent.breatheAmplitude = EditorGUILayout.Slider("Breathe amplitude", accent.breatheAmplitude, 0f, 0.4f,
                GUILayout.Width(accentBreatheRow), GUILayout.ExpandWidth(false));
            EditorRowEnd();
            DrawEditorRow(out float accentPeriodRow, out _);
            accent.period = EditorGUILayout.FloatField("Period (s)", accent.period,
                GUILayout.Width(accentPeriodRow), GUILayout.ExpandWidth(false));
            EditorRowEnd();
            DrawEditorRow(out float accentBeaconStartRow, out _);
            accent.beaconStartScale = EditorGUILayout.FloatField("Beacon start scale", accent.beaconStartScale,
                GUILayout.Width(accentBeaconStartRow), GUILayout.ExpandWidth(false));
            EditorRowEnd();
            DrawEditorRow(out float accentBeaconEndRow, out _);
            accent.beaconEndScale = EditorGUILayout.FloatField("Beacon end scale", accent.beaconEndScale,
                GUILayout.Width(accentBeaconEndRow), GUILayout.ExpandWidth(false));
            EditorRowEnd();
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

                    // Column 3: Details (icon) - reuses EntryDetailsPopup exactly as-is
                    if (GUILayout.Button(DetailsIcon, GUILayout.Width(26f), GUILayout.Height(22f)))
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
                        // Same rule as outline levels: the row key is identity, so a
                        // delete that orphans POIs asks first. Nothing to propagate.
                        if (IdentityDeleteGuard.Confirm("Hierarchy level", entry.key, CountPoisUsingHierarchyLevel(entry.key)))
                        {
                            _config.hierarchy_levels.RemoveAt(i);
                            i--;
                            count--;
                            continue;
                        }
                    }
                }

                entry.key = string.IsNullOrWhiteSpace(entry.key) ? $"level_{i + 1}" : entry.key;
                _config.hierarchy_levels[i] = entry;
            }

            EditorGUILayout.Space(4f);
            // No cap on hierarchy rows -- developers may add as many as needed.
            // Rendered as a shared editor row: transparent indent spacer + width
            // capped to max(MinRowWidth, min(visible panel, MaxRowWidth)).
            DrawEditorRow(out float rowWidth, out _);
            if (GUILayout.Button("+ Add hierarchy level", GUILayout.Width(rowWidth), GUILayout.ExpandWidth(false)))
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
            EditorRowEnd();
        }

        private void EnsureEffectDefaultsExist()
        {
            if (_config.effect_defaults == null)
            {
                _config.effect_defaults = new EffectDefaults();
                _hasUnsavedChanges = true;
            }
        }

    }
}
