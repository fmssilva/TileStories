// POIEditorToolWindow.FieldRows.cs
//
// Partial: the shared labelled-row drawers every section builds on (_5.1_Editor_Tab.md, "Shared row
// helpers and buttons"). Each is one shared editor row (DrawEditorRow ... EditorRowEnd): the row pays
// the indent once and runs its controls at indentLevel 0, the labelled control takes rowWidth (minus a
// help-button allowance when helpText is set), FieldLabelWidthCompensationScope keeps the value column
// aligned under extraIndentPixels, and ExpandWidth(false) keeps a control from stretching the panel.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    public partial class POIEditorToolWindow
    {
        // Width left for the labelled control once a help button shares the row
        private static float FieldWidth(float rowWidth, string helpText) =>
            string.IsNullOrEmpty(helpText) ? rowWidth : Mathf.Max(40f, rowWidth - 36f);

        // The row's trailing (i) button, when it has a help text
        private static void DrawRowHelp(string label, string helpText)
        {
            if (!string.IsNullOrEmpty(helpText))
                HelpInfoButton.Draw(label, helpText);
        }

        internal static float DrawScalarField(string label, float value, string helpText = "", float extraIndentPixels = 0f)
        {
            DrawEditorRow(out float rowWidth, out _, extraIndentPixels);
            using (new FieldLabelWidthCompensationScope(extraIndentPixels))
                value = EditorGUILayout.FloatField(label, value, GUILayout.Width(FieldWidth(rowWidth, helpText)), GUILayout.ExpandWidth(false));
            DrawRowHelp(label, helpText);
            EditorRowEnd();
            return value;
        }

        internal static int DrawIntField(string label, int value, string helpText = "", float extraIndentPixels = 0f)
        {
            DrawEditorRow(out float rowWidth, out _, extraIndentPixels);
            using (new FieldLabelWidthCompensationScope(extraIndentPixels))
                value = EditorGUILayout.IntField(label, value, GUILayout.Width(FieldWidth(rowWidth, helpText)), GUILayout.ExpandWidth(false));
            DrawRowHelp(label, helpText);
            EditorRowEnd();
            return value;
        }

        internal static bool DrawToggleField(string label, bool value, string helpText = "", float extraIndentPixels = 0f)
        {
            DrawEditorRow(out float rowWidth, out _, extraIndentPixels);
            using (new FieldLabelWidthCompensationScope(extraIndentPixels))
            {
                // A toggle's label column must never clip its own text (it did: "Add Hierarchy demo
                // gri"): widen it just enough, and only when the label is longer than the column.
                // Measured here, inside the real OnGUI pass, where CalcSize is reliable.
                float needed = EditorStyles.label.CalcSize(new GUIContent(label)).x + 6f;
                if (EditorGUIUtility.labelWidth < needed) EditorGUIUtility.labelWidth = needed;
                value = EditorGUILayout.Toggle(label, value, GUILayout.Width(FieldWidth(rowWidth, helpText)), GUILayout.ExpandWidth(false));
            }
            DrawRowHelp(label, helpText);
            EditorRowEnd();
            return value;
        }

        internal static string DrawPopupField(string label, string current, string[] options, string[] labels, string helpText = "", float extraIndentPixels = 0f)
        {
            DrawEditorRow(out float rowWidth, out _, extraIndentPixels);
            int idx = Array.IndexOf(options, current);
            if (idx < 0) idx = 0;
            using (new FieldLabelWidthCompensationScope(extraIndentPixels))
                idx = EditorGUILayout.Popup(label, idx, labels, GUILayout.Width(FieldWidth(rowWidth, helpText)), GUILayout.ExpandWidth(false));
            DrawRowHelp(label, helpText);
            EditorRowEnd();
            return idx >= 0 ? options[idx] : current;
        }

        // Same shared-row shape, for a popup whose value REFERENCES a taxonomy row (a POI's
        // category, badge, hierarchy level, status level). Unlike DrawPopupField it never swaps a
        // value it cannot find for the first option: a stale key stays selected as "<key> (missing)"
        // until the developer picks another row (ReferencePopupOptions).
        internal static string DrawReferencePopupField(string label, string current, IList<string> keys, IList<string> labels,
            bool allowNone, string helpText = "", float extraIndentPixels = 0f)
        {
            var options = ReferencePopupOptions.Build(keys, labels, current, allowNone);
            DrawEditorRow(out float rowWidth, out _, extraIndentPixels);
            int idx;
            using (new FieldLabelWidthCompensationScope(extraIndentPixels))
                idx = EditorGUILayout.Popup(label, options.SelectedIndex, options.Labels, GUILayout.Width(FieldWidth(rowWidth, helpText)), GUILayout.ExpandWidth(false));
            DrawRowHelp(label, helpText);
            EditorRowEnd();
            // Nothing picked: hand back the stored value untouched (null and "" stay as they were)
            return idx == options.SelectedIndex ? current : options.KeyAt(idx);
        }

        // A min/max-clamped slider instead of a free-typed float
        internal static float DrawSliderField(string label, float value, float min, float max, string helpText = "", float extraIndentPixels = 0f)
        {
            DrawEditorRow(out float rowWidth, out _, extraIndentPixels);
            using (new FieldLabelWidthCompensationScope(extraIndentPixels))
                value = EditorGUILayout.Slider(label, value, min, max, GUILayout.Width(FieldWidth(rowWidth, helpText)), GUILayout.ExpandWidth(false));
            DrawRowHelp(label, helpText);
            EditorRowEnd();
            return value;
        }

        // A whole-number slider (counts)
        internal static int DrawIntSliderField(string label, int value, int min, int max, string helpText = "", float extraIndentPixels = 0f)
        {
            DrawEditorRow(out float rowWidth, out _, extraIndentPixels);
            using (new FieldLabelWidthCompensationScope(extraIndentPixels))
                value = EditorGUILayout.IntSlider(label, value, min, max, GUILayout.Width(FieldWidth(rowWidth, helpText)), GUILayout.ExpandWidth(false));
            DrawRowHelp(label, helpText);
            EditorRowEnd();
            return value;
        }

        // A LABELLED standalone colour field (picker + hex) outside a taxonomy table. PrefixLabel draws
        // the label column like every other row; the picker + hex pair is the tables' own
        // DrawColorSwatchAndHex, so both look the same.
        internal static void DrawColorField(string label, ref string colorHex, string helpText = "", float extraIndentPixels = 0f)
        {
            DrawEditorRow(out _, out _, extraIndentPixels);
            using (new FieldLabelWidthCompensationScope(extraIndentPixels))
                EditorGUILayout.PrefixLabel(label);
            string hex = colorHex;
            DrawColorSwatchAndHex(ref hex, out _, out _);
            colorHex = hex;
            DrawRowHelp(label, helpText);
            EditorRowEnd();
        }
    }
}
