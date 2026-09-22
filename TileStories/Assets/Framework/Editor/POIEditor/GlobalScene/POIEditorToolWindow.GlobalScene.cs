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

            _showGlobalSearchFilter = DrawFramedFoldout(ref _showGlobalSearchFilter, DrawGlobalSearchFilterSection, "Search & Filter", SearchFilterSectionColor);
        }

        private void DrawGlobalHierarchySection()
        {
            if (_config.hierarchy_levels == null)
                _config.hierarchy_levels = new List<HierarchyLevelEntry>();
            EnsureEffectDefaultsExist();

            EditorGUILayout.LabelField("Hierarchy Levels", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Wall-configurable per-POI hierarchy levels. Each row drives one POI marker's " +
                "size, label visibility, effects (Ripple / Halo / Pulse, defined in the Effects " +
                "section), outline rotation, reveal delay, and reveal duration. An empty table " +
                "means all POIs fall through to the framework default (12cm, no label, no " +
                "effects, 0.35s reveal).",
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

                    // Column 6: Ripple effect (dropdown, mutually exclusive). Only effects ticked in
                    // Global Scene > Effects are offered; a value that points at a disabled effect stays
                    // visible as "(disabled)" so nothing is silently lost.
                    entry.ripple_effect = DrawEffectOptionPopup("Ripple", entry.ripple_effect,
                        RippleEffectOptions, RippleEffectLabels, 120f);

                    // Column 7: Halo effect (dropdown, mutually exclusive), filtered the same way.
                    entry.halo_effect = DrawEffectOptionPopup("Halo", entry.halo_effect,
                        HaloEffectOptions, HaloEffectLabels, 120f);

                    // Column 8: Pulse (checkbox, standalone boolean). Greyed out when the Pulse effect
                    // is switched off in the Effects section; the ticked value is kept.
                    using (new EditorGUI.DisabledScope(!_config.effect_defaults.IsEffectEnabled(MarkerEffectFlags.Pulse)))
                        entry.pulse = EditorGUILayout.Toggle("Pulse", entry.pulse, GUILayout.Width(70f));
                    HelpInfoButton.Draw("Effect columns (Ripple / Halo / Pulse)", HierarchyEffectColumnsHelp);

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

                    // Column 11: Facing Options override (_2.1_Marker_Orientation.md section 4.3).
                    // "" = inherit the wall's facing_mode.
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        int facingIdx = Array.IndexOf(FacingModeOverrideOptions, entry.facing_mode_override ?? "");
                        if (facingIdx < 0) facingIdx = 0;
                        facingIdx = EditorGUILayout.Popup("Facing", facingIdx, FacingModeOverrideLabels, GUILayout.Width(150f));
                        entry.facing_mode_override = FacingModeOverrideOptions[facingIdx];
                        HelpInfoButton.Draw("Facing Options Override",
                            "Overrides the wall's Facing Options mode for POIs at this hierarchy level only. Inherit (default) uses the wall setting above. Lets hero levels stay Always Facing Camera for legibility while background levels sit flat on the wall (Wall Fixed).");
                    }

                    // Column 12: Remove (trash button)
                    bool deleteHierarchyClicked = DeleteButton.DrawLayout($"Delete hierarchy level: {entry.key}");

                    // Right-edge scrollbar clearance (_5.1_Editor_Tab.md "Row Indentation &
                    // Spacing"): this table builds its own row layout instead of going
                    // through DrawEditorRow, so this needs restating by hand.
                    GUILayout.Space(AddButtonRowRightMargin);

                    if (deleteHierarchyClicked)
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
                    ripple_effect = "none",
                    halo_effect = "none",
                    pulse = false,
                    rotate_contour = false,
                    reveal_delay_s = 0f,
                    reveal_duration_s = 0.35f
                });
                _hasUnsavedChanges = true;
            }
            EditorRowEnd();
        }

    }
}
