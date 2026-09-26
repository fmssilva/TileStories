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
            _showGlobalLabelsAndFonts = DrawFramedFoldout(ref _showGlobalLabelsAndFonts, DrawGlobalLabelsAndFontsSection, "Labels, Text & Fonts", LabelsAndFontsSectionColor);

            EditorGUILayout.Space(4f);

            _showGlobalMarker = DrawFramedFoldout(ref _showGlobalMarker, DrawMarkerGlobalSection, "Marker", MarkerSectionColor);

            EditorGUILayout.Space(4f);

            _showGlobalOrientation = DrawFramedFoldout(ref _showGlobalOrientation, DrawGlobalOrientationSection, "Orientation", OrientationSectionColor);

            EditorGUILayout.Space(4f);

            _showGlobalBadge = DrawFramedFoldout(ref _showGlobalBadge, DrawGlobalBadgeSection, "Badge", BadgeSectionColor);

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

            _showGlobalSearchFilter = DrawFramedFoldout(ref _showGlobalSearchFilter, DrawGlobalSearchFilterSection, "Select, Filter & Search", SearchFilterSectionColor);
        }

        private readonly TestGuideState _hierarchyTest = new TestGuideState();

        // Test seam: reports each table checkbox's (and the row's trash button's, column "delete") drawn rect (column name, row index) on Repaint.
        // Null in production, so zero cost -- same idea as DrawSymbolTable's captureFirstRowRects.
        // Lets HierarchyTableClickTests click the REAL checkbox where the developer sees it.
        internal static Action<string, int, Rect> HierarchyCheckboxRectProbe;

        private static void ReportCheckboxRect(string column, int row, Rect rect)
        {
            if (HierarchyCheckboxRectProbe != null && Event.current.type == EventType.Repaint)
                HierarchyCheckboxRectProbe(column, row, rect);
        }

        private void DrawGlobalHierarchySection()
        {
            if (_config.hierarchy_levels == null)
                _config.hierarchy_levels = new List<HierarchyLevelEntry>();
            EnsureEffectDefaultsExist();

            EditorGUILayout.HelpBox(
                "Each row is one hierarchy level; every POI picks its level in Specific Marker > " +
                "Marker Style > Hierarchy Level. A level sets its markers' size, text label (shown or " +
                "not, and its style), effects (Ripple / Halo / Pulse, defined in the Effects section), " +
                "outline spin, reveal timing, facing and extra search keywords. A POI with no level " +
                "uses the framework default (12cm, no label, no effects, 0.35s reveal).",
                MessageType.Info);

            int count = _config.hierarchy_levels.Count;
            if (count == 0)
            {
                EditorGUILayout.HelpBox("No hierarchy levels defined. Add at least one row to enable hierarchy-based marker sizing.", MessageType.Info);
            }

            // Column widths, shared by the header and every data row so they cannot drift
            // (_5.1_Editor_Tab.md "Tables"). Groups are separated by TableGapBetweenGroups, siblings
            // inside a group by TableGapWithinGroup; each group's (i) help button lives ONCE in the
            // header at the group's right edge. PulseW/RotateW are generous on purpose: a CalcSize
            // taken outside a real OnGUI pass clipped the titles, so they wait for a real capture.
            const float NameW = 140f, PriorityW = 75f, DetailsW = 26f, LabelStyleW = 26f, SizeW = 88f, ShowLabelW = 130f,
                RippleW = 110f, HaloW = 110f, PulseW = 70f, RotateW = 100f, DelayW = 80f, DurationW = 80f,
                FacingW = 175f, HelpBtnW = 22f, CheckboxGlyphW = 17f;

            // Header row: every column gets a title here, so the row controls draw GUIContent.none
            // instead of repeating it on every row.
            bool outlineEnabled = !string.Equals(_config.marker_outline_mode, "none", StringComparison.OrdinalIgnoreCase);
            if (count > 0)
            {
                using (new TableRowScope())
                {
                    // Group: Hierarchy Level Name + Priority + Details. The name identifies the LEVEL
                    // (the per-POI dropdown shows it); it is never a marker's on-wall text, which is
                    // always the POI's own name. The Details column's header cell IS the group's help
                    // button. Adjacent manual Rects, because miniBoldLabel's built-in margin showed as
                    // a gap between two GUILayout labels.
                    Rect nameRect = GUILayoutUtility.GetRect(NameW + PriorityW, EditorGUIUtility.singleLineHeight, GUILayout.Width(NameW + PriorityW));
                    Rect priorityRect = new Rect(nameRect.x + NameW, nameRect.y, PriorityW, nameRect.height);
                    nameRect.width = NameW;
                    EditorGUI.LabelField(nameRect, "Hierarchy Level Name", EditorStyles.miniBoldLabel);
                    EditorGUI.LabelField(priorityRect, "Priority", EditorStyles.miniBoldLabel);
                    GUILayout.Space(TableGapWithinGroup);
                    // Sized like the Details button below it, not HelpBtnW, so the two line up.
                    HelpInfoButton.Draw("Hierarchy Level Name, Priority, Details",
                        "HIERARCHY LEVEL NAME: identifies this level for humans -- shown in the 'which hierarchy " +
                        "level' dropdown on every POI. This is NOT the text that appears under any marker on the " +
                        "wall: that is always the POI's own name. Priority alone cannot serve as this identifying " +
                        "name, since two levels are allowed to share the same priority (a tie is legal).\n\n" +
                        "PRIORITY: sort key for draw order and count-cap survival (lower number = higher priority). " +
                        "A whole number of 1 or more (any range you like, e.g. 10 to 100); " +
                        "values below 1 are raised to 1. A new POI starts on the level with the smallest number. Duplicate values are legal and meaningful (a tie), not an error.\n\n" +
                        "DETAILS (...): your own free-text notes about this level, saved into config.json.", DetailsW);

                    GUILayout.Space(TableGapBetweenGroups);

                    // Group: Marker Size (no help button: a centimetre value is self-explanatory).
                    EditorGUILayout.LabelField("Marker Size (cm)", EditorStyles.miniBoldLabel, GUILayout.Width(SizeW));

                    GUILayout.Space(TableGapBetweenGroups);

                    // Group: Marker Label -- Show Marker Label? + the "Aa" Marker Label Style button.
                    // Manual Rect title (same margin reason as above); the help button is sized like
                    // the Aa button beneath it.
                    Rect showLabelRect = GUILayoutUtility.GetRect(ShowLabelW, EditorGUIUtility.singleLineHeight, GUILayout.Width(ShowLabelW));
                    EditorGUI.LabelField(showLabelRect, "Show Marker Label?", EditorStyles.miniBoldLabel);
                    GUILayout.Space(TableGapWithinGroup);
                    HelpInfoButton.Draw("Marker Label",
                        "SHOW MARKER LABEL?: whether a persistent text label (the POI's own name) is shown under " +
                        "the marker Symbol at this level. Levels meant to stand out (a framework default " +
                        "suggestion: your most important tier) commonly show one; lower tiers commonly do not, to " +
                        "reduce visual clutter.\n\n" +
                        "AA (Marker Label Style): opens a small window where this level can use its own label " +
                        "gap, font size and font instead of the wall default (Global Scene > Labels, Text & Fonts) " +
                        "-- e.g. to keep this level's text large for accessibility even though its marker is small. " +
                        "The button text is bold while the level uses its own style.",
                        LabelStyleW);

                    GUILayout.Space(TableGapBetweenGroups);

                    // Group: Effects (Ripple / Halo / Pulse), titles centred over their columns.
                    // Ripple + Halo are adjacent Rects, mirrored exactly in the data row so both
                    // rows consume the same width and nothing to their right drifts.
                    Rect rippleHeaderRect = GUILayoutUtility.GetRect(RippleW + HaloW, EditorGUIUtility.singleLineHeight, GUILayout.Width(RippleW + HaloW));
                    Rect haloHeaderRect = new Rect(rippleHeaderRect.x + RippleW, rippleHeaderRect.y, HaloW, rippleHeaderRect.height);
                    rippleHeaderRect.width = RippleW;
                    EditorGUI.LabelField(rippleHeaderRect, "Ripple", CenteredMiniBoldLabel);
                    EditorGUI.LabelField(haloHeaderRect, "Halo", CenteredMiniBoldLabel);
                    GUILayout.Space(TableGapWithinGroup);
                    // Rect-based: EditorGUILayout.LabelField's style margin shifted every later group.
                    Rect pulseHeaderRect = GUILayoutUtility.GetRect(PulseW, EditorGUIUtility.singleLineHeight, GUILayout.Width(PulseW));
                    EditorGUI.LabelField(pulseHeaderRect, "Pulse", CenteredMiniBoldLabel);

                    GUILayout.Space(TableGapBetweenGroups);

                    // Group: Outline spin -- only meaningful (and only shown) while the wall's
                    // Outline section is enabled; hidden entirely, header included, when it is off.
                    if (outlineEnabled)
                    {
                        // Centred over its checkbox column, Rect-based like Pulse.
                        Rect spinHeaderRect = GUILayoutUtility.GetRect(RotateW, EditorGUIUtility.singleLineHeight, GUILayout.Width(RotateW));
                        EditorGUI.LabelField(spinHeaderRect, "Spin Ring", CenteredMiniBoldLabel);
                        GUILayout.Space(TableGapBetweenGroups);
                    }

                    // Group: Reveal timing. One title spanning Delay + Duration; the help button sits
                    // INSIDE the title's rightmost HelpBtnW px (an appended button would add width
                    // the data row never spends and push Facing Override off its column).
                    Rect revealHeaderRect = GUILayoutUtility.GetRect(DelayW + DurationW, EditorGUIUtility.singleLineHeight, GUILayout.Width(DelayW + DurationW));
                    EditorGUI.LabelField(revealHeaderRect, "Reveal Timing (s)", EditorStyles.miniBoldLabel);
                    Rect revealHelpRect = new Rect(revealHeaderRect.xMax - HelpBtnW, revealHeaderRect.y, HelpBtnW, revealHeaderRect.height);
                    HelpInfoButton.Draw(revealHelpRect, "Reveal Delay vs Duration",
                        "DELAY: seconds after spawn before this level's markers start fading/scaling in.\n" +
                        "DURATION: how long that fade/scale-in animation itself takes. A longer delay staggers when " +
                        "markers appear; a longer duration makes each one enter more slowly.");

                    GUILayout.Space(TableGapBetweenGroups);

                    // Group: Facing override
                    EditorGUILayout.LabelField("Facing Override", EditorStyles.miniBoldLabel, GUILayout.Width(FacingW));
                    HelpInfoButton.Draw("Facing Override",
                        "Overrides the wall's own Facing Options mode (Global Scene > Orientation) for POIs at this " +
                        "level only. Inherit (default) uses the wall's setting. A framework default suggestion: let " +
                        "your most important tier stay Always Facing Camera for legibility, while lower tiers use " +
                        "Wall Fixed to sit flat and reduce rotation cost.", HelpBtnW);

                    GUILayout.Space(TableGapBetweenGroups);

                    // Group: Search Keywords
                    EditorGUILayout.LabelField("Search Keywords", EditorStyles.miniBoldLabel);
                    HelpInfoButton.Draw("Search Keywords", HierarchySearchKeywordsHelp, HelpBtnW);

                    GUILayout.Space(TableGapBeforeDelete);
                    EditorGUILayout.LabelField("", GUILayout.Width(26f)); // Remove (trash)
                    GUILayout.Space(AddButtonRowRightMargin);
                }
            }

            for (int i = 0; i < count; i++)
            {
                var entry = _config.hierarchy_levels[i] ?? new HierarchyLevelEntry();
                using (new TableRowScope())
                {
                    // Group: Hierarchy Level Name + Priority + Details, adjacent Rects like the header.
                    Rect nameFieldRect = GUILayoutUtility.GetRect(NameW + PriorityW, EditorGUIUtility.singleLineHeight, GUILayout.Width(NameW + PriorityW));
                    Rect priorityFieldRect = new Rect(nameFieldRect.x + NameW, nameFieldRect.y, PriorityW, nameFieldRect.height);
                    nameFieldRect.width = NameW;
                    entry.level_name = EditorGUI.TextField(nameFieldRect, entry.level_name);
                    entry.priority = Mathf.Max(1, EditorGUI.IntField(priorityFieldRect, entry.priority)); // whole numbers >= 1 (0 used to mean 'unset')
                    GUILayout.Space(TableGapWithinGroup);
                    // Details: EntryDetailsPopup (a free-text note). Raw GetRect + GUI.Button so no
                    // style margin shifts it off the header's help button above.
                    Rect detailsBtnRect = GUILayoutUtility.GetRect(DetailsW, 22f, GUILayout.Width(DetailsW), GUILayout.Height(22f));
                    if (GUI.Button(detailsBtnRect, DetailsIcon))
                        EditorPopup.ShowAt(CreateDetailsPopup(
                            entry.level_name ?? "Hierarchy level", () => entry.details, v => entry.details = v), detailsBtnRect);

                    GUILayout.Space(TableGapBetweenGroups);

                    // Group: Marker Size -- a soft warning icon, not a clamp (a large mural marker may
                    // legitimately need a value outside 0.5-100cm).
                    using (new EditorGUILayout.HorizontalScope(GUILayout.Width(SizeW)))
                    {
                        GUILayout.FlexibleSpace();
                        entry.size_cm = EditorGUILayout.FloatField(entry.size_cm, GUILayout.Width(60f));
                        if (entry.size_cm < 0.5f || entry.size_cm > 100f)
                        {
                            var warnContent = EditorGUIUtility.IconContent("console.warnicon.sml");
                            warnContent.tooltip = "Unusually large or small -- is this a cm/m typo? (0.5-100cm is the expected range)";
                            GUILayout.Label(warnContent, GUILayout.Width(20f), GUILayout.Height(18f));
                        }
                        GUILayout.FlexibleSpace();
                    }

                    GUILayout.Space(TableGapBetweenGroups);

                    // Group: Marker Label -- checkbox centred under its title via a centred Rect.
                    Rect showLabelCellRect = GUILayoutUtility.GetRect(ShowLabelW, EditorGUIUtility.singleLineHeight, GUILayout.Width(ShowLabelW));
                    Rect showLabelCheckRect = new Rect(showLabelCellRect.x + (ShowLabelW - CheckboxGlyphW) / 2f, showLabelCellRect.y, CheckboxGlyphW, showLabelCellRect.height);
                    ReportCheckboxRect("show_label", i, showLabelCheckRect);
                    entry.show_label = EditorGUI.Toggle(showLabelCheckRect, entry.show_label);
                    GUILayout.Space(TableGapWithinGroup);
                    // "Aa": opens this level's Marker Label Style window (functional config, its own
                    // component -- not EntryDetailsPopup's free-text note). Same zero-margin raw
                    // GetRect + GUI.Button as Details above, so it lines up under the header's help
                    // button. Bold when the level overrides the wall default, so the table shows at a
                    // glance which levels have their own label style.
                    var labelStyleContent = new GUIContent("Aa", entry.override_label_style
                        ? "Marker Label Style: this level uses its own gap/font size/font"
                        : "Marker Label Style: this level follows the wall default");
                    Rect aaBtnRect = GUILayoutUtility.GetRect(LabelStyleW, 22f, GUILayout.Width(LabelStyleW), GUILayout.Height(22f));
                    var aaStyle = entry.override_label_style ? BoldTableButton : GUI.skin.button;
                    if (GUI.Button(aaBtnRect, labelStyleContent, aaStyle))
                        LevelLabelStylePopup.Open(this, entry.key, aaBtnRect);

                    GUILayout.Space(TableGapBetweenGroups);

                    // Group: Effects. Only effects ticked in Global Scene > Effects are offered; a
                    // value pointing at a disabled effect stays visible as "(disabled)" so nothing is
                    // silently lost. Ripple + Halo are adjacent Rects mirroring the header.
                    Rect rippleBodyRect = GUILayoutUtility.GetRect(RippleW + HaloW, EditorGUIUtility.singleLineHeight, GUILayout.Width(RippleW + HaloW));
                    Rect haloBodyRect = new Rect(rippleBodyRect.x + RippleW, rippleBodyRect.y, HaloW, rippleBodyRect.height);
                    rippleBodyRect.width = RippleW;
                    entry.ripple_effect = DrawEffectOptionPopup(rippleBodyRect, entry.ripple_effect, RippleEffectOptions, RippleEffectLabels);
                    entry.halo_effect = DrawEffectOptionPopup(haloBodyRect, entry.halo_effect, HaloEffectOptions, HaloEffectLabels);
                    GUILayout.Space(TableGapWithinGroup);
                    // Centered under its title via a manually centered Rect -- a FlexibleSpace-padded
                    // HorizontalScope let the toggle style's own margin nudge the glyph off-centre.
                    Rect pulseCellRect = GUILayoutUtility.GetRect(PulseW, EditorGUIUtility.singleLineHeight, GUILayout.Width(PulseW));
                    Rect pulseCheckRect = new Rect(pulseCellRect.x + (PulseW - CheckboxGlyphW) / 2f, pulseCellRect.y, CheckboxGlyphW, pulseCellRect.height);
                    ReportCheckboxRect("pulse", i, pulseCheckRect);
                    using (new EditorGUI.DisabledScope(!_config.effect_defaults.IsEffectEnabled(MarkerEffectFlags.Pulse)))
                        entry.pulse = EditorGUI.Toggle(pulseCheckRect, entry.pulse);

                    GUILayout.Space(TableGapBetweenGroups);

                    // Group: Outline spin -- outline-gated, hidden entirely (row and header) when off.
                    // Centred under its title like Pulse.
                    if (outlineEnabled)
                    {
                        Rect spinCellRect = GUILayoutUtility.GetRect(RotateW, EditorGUIUtility.singleLineHeight, GUILayout.Width(RotateW));
                        Rect spinCheckRect = new Rect(spinCellRect.x + (RotateW - CheckboxGlyphW) / 2f, spinCellRect.y, CheckboxGlyphW, spinCellRect.height);
                        ReportCheckboxRect("rotate_contour", i, spinCheckRect);
                        entry.rotate_contour = EditorGUI.Toggle(spinCheckRect, entry.rotate_contour);
                        GUILayout.Space(TableGapBetweenGroups);
                    }

                    // Group: Reveal timing -- Delay and Duration as adjacent Rects under the one
                    // spanning title (TableRowScope keeps IndentedRect from opening a gap between them).
                    Rect delayRect = GUILayoutUtility.GetRect(DelayW + DurationW, EditorGUIUtility.singleLineHeight, GUILayout.Width(DelayW + DurationW));
                    Rect durationRect = new Rect(delayRect.x + DelayW, delayRect.y, DurationW, delayRect.height);
                    delayRect.width = DelayW;
                    entry.reveal_delay_s = EditorGUI.FloatField(delayRect, entry.reveal_delay_s);
                    entry.reveal_duration_s = EditorGUI.FloatField(durationRect, entry.reveal_duration_s);

                    GUILayout.Space(TableGapBetweenGroups);

                    // Group: Facing Override (_2.1_Marker_Orientation.md section 4.3). "" = inherit.
                    int facingIdx = Array.IndexOf(FacingModeOverrideOptions, entry.facing_mode_override ?? "");
                    if (facingIdx < 0) facingIdx = 0;
                    facingIdx = EditorGUILayout.Popup(GUIContent.none, facingIdx, FacingModeOverrideLabels, GUILayout.Width(FacingW));
                    entry.facing_mode_override = FacingModeOverrideOptions[facingIdx];

                    GUILayout.Space(TableGapBetweenGroups);

                    // Group: Search Keywords -- extra index terms every POI at this level is
                    // automatically indexed with (_2.6_Select_Filter_Search.md section 6). Same
                    // shared cell every other taxonomy table's Search Keywords column uses.
                    entry.search_keywords = DrawKeywordListField(entry.search_keywords, GUILayout.ExpandWidth(true));

                    GUILayout.Space(TableGapBeforeDelete);

                    // Remove (trash button)
                    string entryDisplayName = string.IsNullOrWhiteSpace(entry.level_name) ? entry.key : entry.level_name;
                    bool deleteHierarchyClicked = DeleteButton.DrawLayout($"Delete hierarchy level: {entryDisplayName}");
                    ReportCheckboxRect("delete", i, GUILayoutUtility.GetLastRect());

                    // Right-edge scrollbar clearance (_5.1_Editor_Tab.md "Row Indentation &
                    // Spacing"): this table builds its own row layout instead of going
                    // through DrawEditorRow, so this needs restating by hand.
                    GUILayout.Space(AddButtonRowRightMargin);

                    if (deleteHierarchyClicked)
                    {
                        // Same rule as outline levels: the row key is identity, so a
                        // delete that orphans POIs asks first. Nothing to propagate.
                        if (IdentityDeleteGuard.Confirm("Hierarchy level", entryDisplayName, CountPoisUsingHierarchyLevel(entry.key)))
                        {
                            _config.hierarchy_levels.RemoveAt(i);
                            i--;
                            count--;
                            continue;
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(entry.key))
                    entry.key = NextFreeHierarchyLevelKey(_config.hierarchy_levels);
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
                    key = NextFreeHierarchyLevelKey(_config.hierarchy_levels),
                    level_name = (_config.hierarchy_levels.Count + 1).ToString(),
                    priority = NextLowestPriority(_config.hierarchy_levels),
                    size_cm = 12f,
                    show_label = false,
                    ripple_effect = "none",
                    halo_effect = "none",
                    pulse = false,
                    rotate_contour = false,
                    reveal_delay_s = 0f,
                    reveal_duration_s = 0.35f
                });
            }
            EditorRowEnd();

            DrawDomainTestSubSection(_hierarchyTest, HierarchySceneTestGuide, HierarchyPlaymodeTestGuide, HierarchyDeviceTestGuide, DrawHierarchyPreviewSwitch);
        }

        // A level key nobody uses yet: "level_N" with the smallest N >= Count + 1 that is free. The
        // key is the identity POIs point at, so it must never repeat -- "level_" + (Count + 1) alone
        // collided as soon as a middle row had been deleted (level_1, level_3 -> a second level_3).
        internal static string NextFreeHierarchyLevelKey(List<HierarchyLevelEntry> levels)
        {
            var used = new HashSet<string>();
            if (levels != null)
                foreach (var level in levels)
                    if (level != null && !string.IsNullOrWhiteSpace(level.key)) used.Add(level.key.Trim());

            int n = (levels?.Count ?? 0) + 1;
            while (used.Contains("level_" + n)) n++;
            return "level_" + n;
        }

        // "Add Hierarchy demo grid": the SAME Play-Mode preview switch Effects > Test calls "Add
        // effects demo grid" (effect_defaults.preview, built by EffectsPreviewSpawner from
        // WallSession) -- one underlying switch, surfaced from both sections since its second block
        // (one cell per hierarchy level, using that level's real size/effects/reveal timing) is
        // exactly this domain's own live preview. Not a second spawner: per 20-code-quality.md
        // ("prove it isn't already built before you build it"), duplicating the grid/camera/focus
        // machinery here would fork behaviour that already exists and is already tested
        // (EffectsPreviewSpawnerTests, EffectsPreviewRenderTests).
        private void DrawHierarchyPreviewSwitch()
        {
            var preview = _config.effect_defaults.preview;
            bool wasOn = preview.enabled;
            preview.enabled = DrawToggleField("Add Hierarchy demo grid", preview.enabled, HierarchyPreviewHelp, IndentLevel1);
            MakeThisTheOnlyActiveDemoView(DemoView.EffectsGrid, justTurnedOn: preview.enabled && !wasOn);
            if (preview.enabled)
            {
                EffectUsageSummary.PreviewBaseOptions(_config, out string[] ids, out string[] labels);
                preview.base_poi_id = DrawPopupField("Base marker", preview.base_poi_id, ids, labels,
                    EffectPreviewBaseHelp, IndentLevel1 + ConditionalAdvance);
            }
        }

    }
}
