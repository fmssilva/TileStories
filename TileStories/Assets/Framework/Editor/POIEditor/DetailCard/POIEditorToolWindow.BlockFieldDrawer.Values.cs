// POIEditorToolWindow.BlockFieldDrawer.Values.cs
//
// Partial: the single-value rows of the generic block-field drawer (BlockFieldDrawer.cs dispatches to them) --
// Choice, Color, Url, Time, Toggle and Number, each one row with its (i). Drawing never writes: a value is created
// only by a real change.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    public partial class POIEditorToolWindow
    {
        private const string BlockChoiceNoneLabel = "(none)";
        // The empty entry of a Choice that has a default: it says which option the card will use ("Default: Yaw Safe")
        private const string BlockChoiceDefaultPrefix = "Default: ";
        // GUILayout's own gap between two controls on one row (the colour picker and its hex field)
        private const float BlockRowControlGap = 3f;

        // A Choice field: a popup of the definition's option labels (the value stored is the option, never the label). An
        // optional field offers "(none)"; a value that is no longer an option stays, shown as "<value> (missing)", until
        // the developer picks another -- drawing never rewrites it.
        private static void DrawChoiceRow(BlockFieldDefinition field, float indent, string probeName, int probeIndex, Func<string> get, Action<string> set,
            string emptyLabel = null)
        {
            var (values, labels) = BlockChoiceOptions(field, get(), emptyLabel);
            int index = values.IndexOf(get());
            DrawEditorRow(out float rowWidth, out _, indent);
            EditorGUILayout.PrefixLabel(field.Label + (field.Required ? " (required)" : ""));
            int picked = EditorGUILayout.Popup(index, labels.ToArray(), GUILayout.Width(Mathf.Max(40f, rowWidth - EditorGUIUtility.labelWidth - 36f)), GUILayout.ExpandWidth(false));
            ReportTableCellRect(probeName, probeIndex);
            HelpInfoButton.Draw(field.Label, field.Help);
            EditorRowEnd();
            if (picked >= 0 && picked != index) set(values[picked]);
        }

        // A Color field: the picker + hex pair every colour row of the window uses (DrawColorSwatchAndHex), stored as the
        // hex text. Text that is not a colour the card accepts (BlockFieldReader.TryParseColor) stays as typed, with a
        // warning under it -- the card leaves that row out until it is fixed. Drawing never writes.
        private static void DrawColorRow(BlockFieldDefinition field, float indent, string probeName, int probeIndex, Func<string> get, Action<string> set)
        {
            string current = get();
            string edited = current;
            DrawEditorRow(out float rowWidth, out _, indent);
            EditorGUILayout.PrefixLabel(field.Label + (field.Required ? " (required)" : ""));
            DrawColorSwatchAndHex(ref edited, out _, out Rect hexRect);
            ReportTableCellRect(probeName, probeIndex, hexRect);
            // - the (i) in the same column as every other row's (i): the value area ends 36 before the row's end (the
            //   Choice rows' rule); a fixed spacer, since a flexible one runs past the row's width cap
            GUILayout.Space(Mathf.Max(0f, rowWidth - EditorGUIUtility.labelWidth - 36f - ColorPickerWidth - ColorHexFieldWidth - BlockRowControlGap));
            HelpInfoButton.Draw(field.Label, field.Help);
            EditorRowEnd();
            if (edited != current) set(edited);
            if (!string.IsNullOrWhiteSpace(get()) && !BlockFieldReader.TryParseColor(get(), out _))
                EditorGUILayout.HelpBox(CardColorInvalidText(field.Label, get()), MessageType.Warning);
        }

        // A Url field: one text field (a link is the same in every language), stored as typed. Text the card would not open
        // (WebLinkRule) stays, with a warning under it -- the card shows no button for it until it is fixed. Drawing never writes.
        private static void DrawUrlRow(BlockFieldDefinition field, float indent, string probeName, int probeIndex, Func<string> get, Action<string> set)
        {
            string current = get();
            DrawEditorRow(out float rowWidth, out _, indent);
            EditorGUILayout.PrefixLabel(field.Label + (field.Required ? " (required)" : ""));
            string edited = EditorGUILayout.TextField(current, GUILayout.Width(Mathf.Max(40f, rowWidth - EditorGUIUtility.labelWidth - 36f)), GUILayout.ExpandWidth(false));
            ReportTableCellRect(probeName, probeIndex);
            HelpInfoButton.Draw(field.Label, field.Help);
            EditorRowEnd();
            if (edited != current) set(edited);
            if (!string.IsNullOrWhiteSpace(get()) && !WebLinkRule.IsOpenable(get()))
                EditorGUILayout.HelpBox(CardUrlInvalidText(field.Label, get()), MessageType.Warning);
        }

        // A Time field: one text field (a time is the same in every language), stored as typed. Text that is not a time
        // (TimeCodeRule) stays, with a warning under it -- the card leaves that row out until it is fixed. Drawing never writes.
        private static void DrawTimeRow(BlockFieldDefinition field, float indent, string probeName, int probeIndex, Func<string> get, Action<string> set)
        {
            string current = get();
            DrawEditorRow(out float rowWidth, out _, indent);
            EditorGUILayout.PrefixLabel(field.Label + (field.Required ? " (required)" : ""));
            string edited = EditorGUILayout.TextField(current, GUILayout.Width(Mathf.Max(40f, rowWidth - EditorGUIUtility.labelWidth - 36f)), GUILayout.ExpandWidth(false));
            ReportTableCellRect(probeName, probeIndex);
            HelpInfoButton.Draw(field.Label, field.Help);
            EditorRowEnd();
            if (edited != current) set(edited);
            if (!string.IsNullOrWhiteSpace(get()) && !TimeCodeRule.IsValid(get()))
                EditorGUILayout.HelpBox(CardTimeInvalidText(field.Label, get()), MessageType.Warning);
        }

        // A Toggle field: label + checkbox + (i); the value is created by the first real click, never by drawing
        private static void DrawToggleRow(BlockFieldDefinition field, float indent, string probeName, int probeIndex, Func<bool> get, Action<bool> set)
        {
            bool current = get();
            DrawEditorRow(out float rowWidth, out _, indent);
            EditorGUILayout.PrefixLabel(field.Label);
            bool edited = EditorGUILayout.Toggle(current, GUILayout.Width(EditorGUIUtility.singleLineHeight), GUILayout.ExpandWidth(false));
            ReportTableCellRect(probeName, probeIndex);
            GUILayout.Space(Mathf.Max(0f, rowWidth - EditorGUIUtility.labelWidth - 36f - EditorGUIUtility.singleLineHeight));
            HelpInfoButton.Draw(field.Label, field.Help);
            EditorRowEnd();
            if (edited != current) set(edited);
        }

        // A Number field: a slider over the definition's range, showing its default while nothing is stored; the value is
        // created by the first real change, never by drawing
        private static void DrawNumberRow(BlockFieldDefinition field, float indent, string probeName, int probeIndex, Func<float> get, Action<float> set)
        {
            float current = get();
            DrawEditorRow(out float rowWidth, out _, indent);
            EditorGUILayout.PrefixLabel(field.Label);
            float edited = EditorGUILayout.Slider(current, field.NumberMin, field.NumberMax,
                GUILayout.Width(Mathf.Max(40f, rowWidth - EditorGUIUtility.labelWidth - 36f)), GUILayout.ExpandWidth(false));
            ReportTableCellRect(probeName, probeIndex);
            HelpInfoButton.Draw(field.Label, field.Help);
            EditorRowEnd();
            if (!Mathf.Approximately(edited, current)) set(edited);
        }

        // The empty entry's label of a Choice with a default (null without one: the plain "(none)"), naming the option by its label
        internal static string BlockChoiceDefaultLabel(BlockFieldDefinition field, string defaultValue)
        {
            if (field?.ChoiceDefault == null || field.Options == null) return null;
            int at = -1;
            for (int i = 0; i < field.Options.Count; i++) if (field.Options[i] == defaultValue) at = i;
            if (at < 0) return null;
            return BlockChoiceDefaultPrefix + (field.OptionLabels != null ? field.OptionLabels[at] : field.Options[at]);
        }

        // The popup of a Choice field: its values and, in the same order, what the Editor shows for them
        internal static (List<string> Values, List<string> Labels) BlockChoiceOptions(BlockFieldDefinition field, string current, string emptyLabel = null)
        {
            var values = new List<string>();
            var labels = new List<string>();
            if (!field.Required)
            {
                values.Add("");
                labels.Add(emptyLabel ?? BlockChoiceNoneLabel);
            }
            for (int i = 0; i < field.Options.Count; i++)
            {
                values.Add(field.Options[i]);
                labels.Add(field.OptionLabels != null ? field.OptionLabels[i] : field.Options[i]);
            }
            if (!string.IsNullOrEmpty(current) && !values.Contains(current))
            {
                values.Add(current);
                labels.Add(current + " (missing)");
            }
            return (values, labels);
        }
    }
}
