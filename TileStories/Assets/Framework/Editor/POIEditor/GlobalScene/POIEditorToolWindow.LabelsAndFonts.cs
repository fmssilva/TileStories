// POIEditorToolWindow.LabelsAndFonts.cs
//
// Partial: the "Labels, Text & Fonts" domain (_2.0_Labels_And_Fonts_Design.md), placed first in
// Global Scene since it is also the future home for UI Toolkit text/font config. It owns the
// wall-default marker label typography (gap/font-size/font), the shared Font + "Add font" rows, and
// the per-level "Marker Label Style" editor the Hierarchy Levels table's "Aa" button opens
// (Shared/Popups/LevelLabelStylePopup.cs is only the popup content around it). Constants and help texts
// live in POIEditorToolWindow.LabelsAndFontsHelp.cs.

using TMPro;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    public partial class POIEditorToolWindow
    {
        private readonly TestGuideState _labelsAndFontsTest = new TestGuideState();

        // "Marker Label" starts OPEN (unlike a fresh top-level section): it is the one group this
        // domain's content lives in today, so collapsing it by default would cost an extra click on
        // the most common path through this section.
        private bool _showMarkerLabelGroup = true;

        private void DrawGlobalLabelsAndFontsSection()
        {
            _showMarkerLabelGroup = EditorGUILayout.Foldout(_showMarkerLabelGroup, "Marker Label", true, EditorStyles.foldoutHeader);
            if (_showMarkerLabelGroup)
            {
                _config.label_gap_ratio = DrawSliderField("Gap (x symbol)", _config.label_gap_ratio,
                    MarkerVisualSettings.LabelGapRatioMin, MarkerVisualSettings.LabelGapRatioMax, LabelGapRatioHelp, IndentLevel1);
                _config.label_font_size_ratio = DrawSliderField("Font size (x symbol)", _config.label_font_size_ratio,
                    MarkerVisualSettings.LabelFontSizeRatioMin, MarkerVisualSettings.LabelFontSizeRatioMax, LabelFontSizeRatioHelp, IndentLevel1);
                _config.label_font_key = DrawLabelFontRows(_config.label_font_key, IndentLevel1);
                DrawWallFontLibraryRow();
            }

            DrawDomainTestSubSection(_labelsAndFontsTest, LabelsAndFontsSceneTestGuide, LabelsAndFontsPlaymodeTestGuide, LabelsAndFontsDeviceTestGuide, DrawLabelsAndFontsPreviewSwitch);
        }

        // The ONE Font picker: a Font popup (framework fonts + the wall's own) and an "Add font" slot
        // that registers a dropped/picked TMP Font Asset in the wall font library and selects it at
        // once. Both the wall default (above) and a hierarchy level's Marker Label Style use it, so
        // adding a font works the same wherever the developer happens to be. Returns the new key.
        internal string DrawLabelFontRows(string currentKey, float extraIndentPixels)
        {
            GetAvailableFontKeyOptions(out string[] keys, out string[] labels);
            string key = DrawPopupField("Font", MarkerVisualSettings.ResolveLabelFontKey(currentKey), keys, labels,
                LabelFontKeyHelp, extraIndentPixels);

            DrawEditorRow(out float rowWidth, out _, extraIndentPixels);
            TMP_FontAsset added;
            using (new FieldLabelWidthCompensationScope(extraIndentPixels))
                added = (TMP_FontAsset)EditorGUILayout.ObjectField("Add font", null, typeof(TMP_FontAsset), false,
                    GUILayout.Width(Mathf.Max(40f, rowWidth - 36f)), GUILayout.ExpandWidth(false));
            HelpInfoButton.Draw("Add font", AddFontHelp);
            EditorRowEnd();

            if (added != null)
            {
                string addedKey = AddFontToWallLibraryAndGetKey(added);
                if (!string.IsNullOrEmpty(addedKey)) key = addedKey;
            }
            return key;
        }

        // Shown only once the wall has its own font library (created by the first "Add font").
        private void DrawWallFontLibraryRow()
        {
            if (_wallFontLibrary == null) return;

            DrawEditorRow(out float rowWidth, out _, IndentLevel1);
            float halfWidth = (rowWidth - 36f - 4f) / 2f;
            if (GUILayout.Button("Select Font List", GUILayout.Width(halfWidth), GUILayout.ExpandWidth(false)))
            {
                EditorGUIUtility.PingObject(_wallFontLibrary);
                Selection.activeObject = _wallFontLibrary;
            }
            GUILayout.Space(4f);
            if (GUILayout.Button("Stop Using Wall Fonts", GUILayout.Width(halfWidth), GUILayout.ExpandWidth(false)))
            {
                _wallFontLibrary = null;
                _config.label_font_library_resources_path = "";
            }
            HelpInfoButton.Draw("Wall font list", WallFontLibraryPresentHelp);
            EditorRowEnd();
        }

        // The SAME Play-Mode preview switch Effects > Test calls "Add effects demo grid" and
        // Hierarchy Levels > Test calls "Add Hierarchy demo grid" (effect_defaults.preview, built
        // by EffectsPreviewSpawner from WallSession): one underlying switch, surfaced here too since
        // its per-level cells render each level's REAL label live. Not a second spawner.
        private void DrawLabelsAndFontsPreviewSwitch()
        {
            var preview = _config.effect_defaults.preview;
            bool wasOn = preview.enabled;
            preview.enabled = DrawToggleField("Add Labels demo grid", preview.enabled, LabelsPreviewHelp, IndentLevel1);
            MakeThisTheOnlyActiveDemoView(DemoView.EffectsGrid, justTurnedOn: preview.enabled && !wasOn);
            if (preview.enabled)
            {
                EffectUsageSummary.PreviewBaseOptions(_config, out string[] ids, out string[] labels);
                preview.base_poi_id = DrawPopupField("Base marker", preview.base_poi_id, ids, labels,
                    EffectPreviewBaseHelp, IndentLevel1 + ConditionalAdvance);
            }
        }

        // ---------------- Marker Label Style (one hierarchy level) ----------------

        // Test seam (null in production): on Repaint, the override toggle's rect and the gap/font
        // size/font the rows are SHOWING, so MarkerLabelStyleWindowTests can click the real toggle
        // and check what the developer sees.
        internal static System.Action<Rect, float, float, string> LevelLabelStyleProbe;

        private HierarchyLevelEntry FindHierarchyLevel(string key)
        {
            if (_config?.hierarchy_levels == null || string.IsNullOrEmpty(key)) return null;
            return _config.hierarchy_levels.Find(l => l != null && l.key == key);
        }

        // Body of the "Marker Label Style" window for one hierarchy level. The level is looked up by
        // key on every call (never cached), so after Undo/Redo swaps _config wholesale the window
        // keeps editing the live config; the edit runs inside this window's own
        // DrawConfigMutationScope, so it gets undo/redo, the unsaved flag, the Scene rig refresh and
        // the live Play Mode push exactly like a field in the main window. Returns false when the
        // level no longer exists (deleted, or undone away) so the shell can close itself.
        internal bool DrawLevelLabelStyleEditor(string levelKey)
        {
            HandleUndoShortcuts();
            var level = FindHierarchyLevel(levelKey);
            if (level == null) return false;

            string levelName = string.IsNullOrWhiteSpace(level.level_name) ? level.key : level.level_name;
            EditorGUILayout.LabelField("Hierarchy level: " + levelName, EditorStyles.miniLabel);
            EditorGUILayout.Space(2f);

            DrawConfigMutationScope(() => DrawLevelLabelStyleFields(level), refreshRigOnChange: true);
            Repaint();
            return true;
        }

        // One bold switch, then the three values. Off: the rows are read-only and show the CURRENT
        // wall default (so they follow any change made in Labels, Text & Fonts). On: the rows edit
        // this level only and never touch the wall default. Ticking it seeds the level from the wall
        // default, so the markers do not jump the moment the switch is turned on.
        private void DrawLevelLabelStyleFields(HierarchyLevelEntry level)
        {
            bool wasOverriding = level.override_label_style;
            DrawEditorRow(out float rowWidth, out _);
            level.override_label_style = EditorGUILayout.ToggleLeft("Override global label config",
                level.override_label_style, EditorStyles.boldLabel,
                GUILayout.Width(Mathf.Max(40f, rowWidth - 36f)), GUILayout.ExpandWidth(false));
            Rect overrideToggleRect = GUILayoutUtility.GetLastRect();
            HelpInfoButton.Draw("Override global label config", LevelLabelStyleOverrideHelp);
            EditorRowEnd();

            if (level.override_label_style && !wasOverriding)
                SeedLevelLabelStyleFromWallDefault(level);

            bool own = level.override_label_style;
            float gap = own ? level.label_gap_ratio : MarkerVisualSettings.ClampLabelGapRatio(_config.label_gap_ratio);
            float size = own ? level.label_font_size_ratio : MarkerVisualSettings.ClampLabelFontSizeRatio(_config.label_font_size_ratio);
            string font = own ? level.label_font_key : _config.label_font_key;

            if (LevelLabelStyleProbe != null && Event.current.type == EventType.Repaint)
                LevelLabelStyleProbe(overrideToggleRect, gap, size, font);

            using (new EditorGUI.DisabledScope(!own))
            {
                gap = DrawSliderField("Gap (x symbol)", gap,
                    MarkerVisualSettings.LabelGapRatioMin, MarkerVisualSettings.LabelGapRatioMax, LabelGapRatioHelp);
                size = DrawSliderField("Font size (x symbol)", size,
                    MarkerVisualSettings.LabelFontSizeRatioMin, MarkerVisualSettings.LabelFontSizeRatioMax, LabelFontSizeRatioHelp);
                font = DrawLabelFontRows(font, 0f);
            }

            if (!own) return;
            level.label_gap_ratio = gap;
            level.label_font_size_ratio = size;
            level.label_font_key = font;
        }

        private void SeedLevelLabelStyleFromWallDefault(HierarchyLevelEntry level)
        {
            level.label_gap_ratio = MarkerVisualSettings.ClampLabelGapRatio(_config.label_gap_ratio);
            level.label_font_size_ratio = MarkerVisualSettings.ClampLabelFontSizeRatio(_config.label_font_size_ratio);
            level.label_font_key = MarkerVisualSettings.ResolveLabelFontKey(_config.label_font_key);
        }
    }
}
