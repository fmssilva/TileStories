// POIEditorToolWindow.MarkerDesign.cs
//
// Partial: the Marker, Badge and Outline sections of Global Scene (_2.2.1, _2.2.2, _2.2.3), each
// ending in a Test sub-foldout with Scene / Playmode / Device guides (_5.1_Editor_Tab.md, "Domain
// Manual Tests"). Every labelled row goes through the shared field drawers (LodZoom.cs). Constants,
// help texts and guides live in POIEditorToolWindow.MarkerDesignHelp.cs.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    public partial class POIEditorToolWindow
    {
        private readonly TestGuideState _markerTest = new TestGuideState();
        private readonly TestGuideState _badgeTest = new TestGuideState();
        private readonly TestGuideState _outlineTest = new TestGuideState();

        // Outline's "Add outline demo grid" switch (mirrors EffectsTest.cs's "Add effects demo grid"):
        // off by default, Editor + development builds only (OutlinePreviewSpawner.IsAllowed).
        private void DrawOutlinePreviewSwitch()
        {
            if (_config.outline_preview == null) _config.outline_preview = new OutlinePreviewSettings();
            var preview = _config.outline_preview;
            bool wasOn = preview.enabled;
            preview.enabled = DrawToggleField("Add outline demo grid", preview.enabled, OutlinePreviewHelp, IndentLevel1);
            MakeThisTheOnlyActiveDemoView(DemoView.OutlineGrid, justTurnedOn: preview.enabled && !wasOn);
            if (preview.enabled)
            {
                EffectUsageSummary.PreviewBaseOptions(_config, out string[] ids, out string[] labels);
                preview.base_poi_id = DrawPopupField("Base marker", preview.base_poi_id, ids, labels,
                    OutlinePreviewBaseHelp, IndentLevel1 + ConditionalAdvance);
            }
        }

        // ---------------- Marker ----------------

        private void DrawMarkerGlobalSection()
        {
            _config.marker_shape = DrawPopupField("Background shape", _config.marker_shape, ShapeOptions, ShapeLabels, MarkerShapeHelp);

            // The icon on every symbol (and badge): its colour and its size inside the symbol
            if (string.IsNullOrWhiteSpace(_config.icon_color_hex))
                _config.icon_color_hex = MarkerVisualSettings.DefaultIconColorHex;
            string iconColor = _config.icon_color_hex;
            DrawColorField("Icon color", ref iconColor, IconColorHelp);
            _config.icon_color_hex = iconColor;
            _config.icon_size_ratio = DrawSliderField("Icon size", _config.icon_size_ratio,
                MarkerVisualSettings.IconSizeRatioMin, MarkerVisualSettings.IconSizeRatioMax, IconSizeHelp);

            // Label typography (gap/font-size/font) lives in its own "Labels, Text & Fonts" section
            // (POIEditorToolWindow.LabelsAndFonts.cs).

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Category Symbols", EditorStyles.boldLabel);

            DrawWallIconLibrarySelector();

            if (_config.category_styles == null)
                _config.category_styles = new List<CategoryStyleEntry>();

            // Seed defaults only if genuinely empty -- not on every load.
            if (_config.category_styles.Count == 0)
                _config.category_styles.AddRange(DefaultCategoryStyles.Create());

            // Resolve Enter/ESC for an in-progress rename BEFORE the shared TextField draws
            // (Unity's TextField consumes the first Return when it sees it -- the same "press
            // Enter twice" trap PoiRenameKeys was built to avoid). Click-outside also commits.
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
                SymbolColumnHelp,
                true,
                e => e.search_keywords,
                (e, v) => e.search_keywords = v,
                // Deleting a category POIs still reference cannot propagate anywhere,
                // so ask first and say how many POIs would be orphaned.
                entry => IdentityRenameResolver.CountReferences(
                    _config.pois, IdentityRenameResolver.PoiUsesCategory, entry.category));

            DrawDomainTestSubSection(_markerTest, MarkerSceneTestGuide, MarkerPlaymodeTestGuide, MarkerDeviceTestGuide);
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

        // ---------------- Badge ----------------

        private void DrawGlobalBadgeSection()
        {
            _config.marker_use_badge = DrawToggleField("Enable badge", _config.marker_use_badge, BadgeEnableHelp);

            if (!_config.marker_use_badge)
            {
                EditorGUILayout.HelpBox("Enable badge to edit badge symbol taxonomy.", MessageType.Info);
                DrawDomainTestSubSection(_badgeTest, BadgeSceneTestGuide, BadgePlaymodeTestGuide, BadgeDeviceTestGuide);
                return;
            }

            if (_config.badge_categories == null)
                _config.badge_categories = new List<BadgeCategoryEntry>();

            // Badge background shape -- independent of marker_shape.
            _config.badge_shape = DrawPopupField("Badge back shape", _config.badge_shape, ShapeOptions, ShapeLabels, BadgeShapeHelp);
            _config.badge_corner = DrawPopupField("Badge corner", _config.badge_corner, BadgeCornerOptions, BadgeCornerLabels, BadgeCornerHelp);
            _config.badge_size_ratio = DrawSliderField("Badge size", _config.badge_size_ratio, 0.2f, 0.5f, BadgeSizeHelp);

            // Seed defaults only if genuinely empty -- not on every load.
            if (_config.badge_categories.Count == 0)
                _config.badge_categories.AddRange(DefaultBadgeCategories.Create());

            // Table title, same as Marker's "Category Symbols"
            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Badge Categories", EditorStyles.boldLabel);

            // Same commit-style rename protection the category table has: the badge key IS the
            // identity POIData.badge_category references, so a raw keystroke straight into it would
            // orphan every POI that uses this badge. Resolve Enter/ESC before the shared TextField draws.
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
                SymbolColumnHelp,
                true,
                e => e.search_keywords,
                (e, v) => e.search_keywords = v,
                entry => IdentityRenameResolver.CountReferences(
                    _config.pois, IdentityRenameResolver.PoiUsesBadgeKey, entry.key));

            DrawDomainTestSubSection(_badgeTest, BadgeSceneTestGuide, BadgePlaymodeTestGuide, BadgeDeviceTestGuide);
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

        // ---------------- Outline ----------------

        private void DrawGlobalOutlineSection()
        {
            bool useOutline = !string.Equals(_config.marker_outline_mode, "none", StringComparison.OrdinalIgnoreCase);
            useOutline = DrawToggleField("Enable outline", useOutline, OutlineEnableHelp);

            if (!useOutline)
            {
                _config.marker_outline_mode = "none";
                EditorGUILayout.HelpBox("Outline disabled. Outline levels are ignored at runtime.", MessageType.Info);
                DrawDomainTestSubSection(_outlineTest, OutlineSceneTestGuide, OutlinePlaymodeTestGuide, OutlineDeviceTestGuide, DrawOutlinePreviewSwitch);
                return;
            }

            // Turning the outline back on from "none" starts from Uniform; an unknown saved value also falls back to Uniform
            if (!MarkerVisualsParser.TryParseOutlineMode(_config.marker_outline_mode, out var parsedMode) || parsedMode == MarkerOutlineMode.None)
                _config.marker_outline_mode = "uniform";
            _config.marker_outline_mode = DrawPopupField("Outline Color", _config.marker_outline_mode, OutlineModeOptions, OutlineModeLabels, OutlineModeHelp);

            // Uniform: one shared colour for every level, edited right here (defaults to gold).
            // Same hue: no colour field at all -- the ring borrows the marker's own category hue.
            // Per outline type: no shared colour here; each row in the table below carries its own.
            if (_config.marker_outline_mode == "uniform")
            {
                if (string.IsNullOrWhiteSpace(_config.outline_uniform_color_hex))
                    _config.outline_uniform_color_hex = "#E3BD72";
                string uniformColor = _config.outline_uniform_color_hex;
                DrawColorField("Outline color", ref uniformColor, OutlineUniformColorHelp);
                _config.outline_uniform_color_hex = uniformColor;
            }

            _config.ring_size_ratio = DrawSliderField("Ring size", _config.ring_size_ratio, 1f, 1.35f, RingSizeHelp);
            _config.contour_spin_deg_per_s = Mathf.Clamp(
                DrawScalarField("Contour spin (deg/s)", _config.contour_spin_deg_per_s, RingSpinHelp), 0f, 360f);

            // Only "Per outline type" gives each row its own colour; Uniform and Same hue both
            // resolve colour above the table (one shared field, or none at all).
            bool showColorColumn = _config.marker_outline_mode == "per_type";

            if (_config.outline_levels == null)
                _config.outline_levels = new List<OutlineLevelEntry>();

            EditorGUILayout.Space(4f);

            // Table title + its (i): what a row is and how to add your own line style
            DrawEditorRow(out float titleRowWidth, out _);
            EditorGUILayout.LabelField("Outline Types", EditorStyles.boldLabel,
                GUILayout.Width(Mathf.Max(40f, titleRowWidth - 36f)), GUILayout.ExpandWidth(false));
            HelpInfoButton.Draw("Outline Types", OutlineTypesHelp);
            EditorRowEnd();

            // Seed defaults only if genuinely empty (section 13.2) -- a brand-new wall,
            // not one that already has entries the developer chose.
            if (_config.outline_levels.Count == 0)
                _config.outline_levels.AddRange(DefaultOutlineLevels.Create());

            // Column headers (5 groups, header mirrors rows exactly):
            // [key+details] | [Style + interactive Preview] | [Color] | [SearchKeywords] | [trash]
            using (new TableRowScope())
            {
                // Group 1: key + notes info
                // The cell below edits entry.label, while entry.key (the identity
                // POIData.status_level_key references, e.g. "level_2") is generated
                // after the row loop. Header must say which one it is or developers
                // assume they are renaming the key.
                EditorGUILayout.LabelField("Outline label", EditorStyles.miniBoldLabel, GUILayout.Width(110f));
                GUILayout.Space(TableGapWithinGroup);
                // Sized to match the per-row Details button's own width (26f) directly
                // below it, same fix as the Marker Table's header (_5.1_Editor_Tab.md).
                HelpInfoButton.Draw("Outline Notes",
                    "Write more information about this outline type here: what it represents, when to use it, example POIs. Stored per row in config.json.", 26f);

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

                // Group 3: per-row colour override -- only in "Per outline type" mode (Uniform and
                // Same hue both resolve colour above the table, see DrawGlobalOutlineSection). When
                // hidden, NEITHER this group nor its leading gap is reserved, mirroring the rows --
                // so exactly one gap (TableGapBetweenGroups, below) separates Outline Style from
                // Search Keywords in every mode. Only when the column is shown does it get the
                // larger, dedicated gap the shared Symbol table uses before ITS Color group
                // (TableGapBeforeColor) -- the standard between-groups gap reads as visually tight
                // here too, with the Outline Style help button sitting right at the boundary
                // (developer feedback, 2026-09-22).
                if (showColorColumn)
                {
                    GUILayout.Space(TableGapBeforeColor);
                    EditorGUILayout.LabelField("Color", EditorStyles.miniBoldLabel, GUILayout.Width(ColorGroupWidth));
                }

                // Group 4: Search keywords
                GUILayout.Space(TableGapBetweenGroups);
                EditorGUILayout.LabelField("Search Keywords", EditorStyles.miniBoldLabel);

                // Group 5: Remove (trash) -- last column, same between-groups gap.
                GUILayout.Space(TableGapBetweenGroups);
                EditorGUILayout.LabelField("", GUILayout.Width(26f)); // Remove (trash)

                // Right-edge scrollbar clearance (_5.1_Editor_Tab.md "Row Indentation &
                // Spacing"): taxonomy tables build their own row layout instead of going
                // through DrawEditorRow, so this needs restating by hand per table.
                GUILayout.Space(AddButtonRowRightMargin);
            }

            for (int i = 0; i < _config.outline_levels.Count; i++)
            {
                var entry = _config.outline_levels[i] ?? new OutlineLevelEntry();
                using (new TableRowScope())
                {
                    // Group 1: key + details
                    entry.label = EditorGUILayout.TextField(entry.label, GUILayout.Width(110f));
                    ReportTableCellRect("Outline label", i);

                    // Details: popup for free-text notes (same pattern as DrawSymbolTable).
                    GUILayout.Space(TableGapWithinGroup);
                    if (GUILayout.Button(DetailsIcon, GUILayout.Width(26f), GUILayout.Height(20f)))
                        EditorPopup.ShowAt(CreateDetailsPopup(entry.label ?? "Outline level", () => entry.details, v => entry.details = v), GUILayoutUtility.GetLastRect());

                    GUILayout.Space(TableGapBetweenGroups);

                    // Group 2: Outline Style + interactive Preview (the preview IS
                    // the curated "choose" affordance -- no separate select button).
                    Sprite current = ResolveSpriteForKey(entry.line_style);
                    Sprite chosen = (Sprite)EditorGUILayout.ObjectField(current, typeof(Sprite), false, GUILayout.Width(140f));
                    if (chosen != current)
                        entry.line_style = AssignSpriteToLibraryAndGetKey(chosen, entry.label);

                    GUILayout.Space(TableGapWithinGroup);

                    // Clicking the preview opens the curated wall + framework picker.
                    // Capture per-iteration: the popup calls back later, so no loop-variable closure.
                    var targetEntry = entry;
                    DrawSpritePreview(chosen != null ? chosen : current,
                        () => EditorPopup.ShowAt(CreateSymbolPickerPopup(key => targetEntry.line_style = key), GUILayoutUtility.GetLastRect()));

                    // Group 3: per-row colour override -- only in "Per outline type" mode (see the
                    // header comment above). The real picker is the first cell of this group; the
                    // hex field follows inside the group after the within-gap. Neither the group nor
                    // its leading gap is reserved when hidden (mirrors the header), so the keywords
                    // gap below is the same single between-groups gap in every mode.
                    if (showColorColumn)
                    {
                        GUILayout.Space(TableGapBeforeColor);
                        string colorHex = entry.color_hex;
                        DrawColorSwatchAndHex(ref colorHex, out _, out _);
                        entry.color_hex = colorHex;
                    }

                    // Group 4: Search keywords
                    GUILayout.Space(TableGapBetweenGroups);
                    entry.search_keywords = DrawKeywordListField(entry.search_keywords, GUILayout.ExpandWidth(true));

                    // Group 5: Remove (trash) -- last column, same between-groups gap.
                    GUILayout.Space(TableGapBetweenGroups);
                    bool deleteOutlineClicked = DeleteButton.DrawLayout($"Delete outline level: {entry.key}");

                    // Same right-edge scrollbar clearance as the header row above.
                    GUILayout.Space(AddButtonRowRightMargin);

                    if (deleteOutlineClicked)
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

            DrawDomainTestSubSection(_outlineTest, OutlineSceneTestGuide, OutlinePlaymodeTestGuide, OutlineDeviceTestGuide, DrawOutlinePreviewSwitch);
        }

        // Auto-space pct whenever the list changes (section 13.4).
        private static void RecomputeLevelPercentSpacing(List<OutlineLevelEntry> levels)
        {
            if (levels == null || levels.Count == 0) return;
            if (levels.Count == 1) { levels[0].pct = 0f; return; }

            for (int i = 0; i < levels.Count; i++)
                levels[i].pct = (100f / (levels.Count - 1)) * i;
        }
    }
}
