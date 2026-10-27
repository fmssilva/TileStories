// NamedOptionPickerPopup.cs
//
// A picker of named options, shown in the shared EditorPopup window: one row per option -- its name and, under it, who or what it
// suits -- a thin line before each group of options after the first (the strongest first), and a mark on the option already in use. A WORKING popup
// (the title bar's X and Esc close it, no button of its own); a pick closes it and hands the option's index to the caller, which applies
// it inside ONE mutation scope (its factory in POIEditorToolWindow.ConfigHistory.cs), so a pick is one undo step. Two pickers use it,
// each with its own kind and its own data: Detail Card > Navigation > Preset (_3.2 step 17a, NavigationPresets) and Specific Marker >
// Card Content > Card Starter (_3.2 step 17b, CardStarters). It knows nothing of either: it only draws words and reports a row.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    internal sealed class NamedOptionPickerPopup : EditorPopupContent
    {
        // One row: the option's name and the line under it
        internal readonly struct Option
        {
            public readonly string Label;
            public readonly string Suits;

            public Option(string label, string suits)
            {
                Label = label;
                Suits = suits;
            }
        }

        private const float RowHeight = 56f;
        private const float SeparatorHeight = 10f;
        private const float MaxWindowHeight = 560f;
        private const float Width = 380f;

        private readonly string _kind;
        private readonly string _title;
        private readonly IReadOnlyList<Option> _options;
        // The indexes a thin line is drawn above (0 or the count = no line)
        private readonly IReadOnlyCollection<int> _separatorsBefore;
        private readonly Func<int> _inUse;
        private readonly Action<int> _onPicked;
        private readonly Func<bool> _isAlive;
        private Vector2 _scroll;

        // `inUse`: the index of the option the config already holds, or -1 (asked on every draw, so the mark follows an undo)
        public NamedOptionPickerPopup(string kind, string title, IReadOnlyList<Option> options, IReadOnlyCollection<int> separatorsBefore, Func<int> inUse,
            Action<int> onPicked, Func<bool> isAlive = null)
        {
            _kind = kind ?? throw new ArgumentNullException(nameof(kind));
            _title = title ?? "";
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _separatorsBefore = separatorsBefore ?? Array.Empty<int>();
            _inUse = inUse;
            _onPicked = onPicked ?? throw new ArgumentNullException(nameof(onPicked));
            _isAlive = isAlive;
        }

        private static GUIStyle _suitsStyle;
        private static GUIStyle SuitsStyle => _suitsStyle ??= new GUIStyle(EditorStyles.miniLabel) { wordWrap = true, alignment = TextAnchor.UpperLeft };

        internal static bool IsPickClick(Event e, Rect row) => e.type == EventType.MouseDown && e.button == 0 && row.Contains(e.mousePosition);

        private bool HasSeparatorBefore(int index) => index > 0 && index < _options.Count && System.Linq.Enumerable.Contains(_separatorsBefore, index);

        private int SeparatorCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < _options.Count; i++) if (HasSeparatorBefore(i)) count++;
                return count;
            }
        }

        public override string Kind => _kind;
        public override string Title => _title;
        public override bool IsAlive => _isAlive == null || _isAlive();
        internal int RowCount => _options.Count;

        public override Vector2 InitialSize =>
            new Vector2(Width, Mathf.Min(MaxWindowHeight, _options.Count * RowHeight + SeparatorCount * SeparatorHeight + 16f));

        // Rect of row i in the popup's last Repaint (tests click it with a real mouse event)
        internal static Action<int, Rect> RowRectProbe;

        public override bool Draw()
        {
            int inUse = _inUse?.Invoke() ?? -1;
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            for (int i = 0; i < _options.Count; i++)
            {
                // - the strongest first, a thin line, then the rest
                if (HasSeparatorBefore(i))
                {
                    Rect line = GUILayoutUtility.GetRect(0f, SeparatorHeight, GUILayout.ExpandWidth(true));
                    if (Event.current.type == EventType.Repaint)
                        EditorGUI.DrawRect(new Rect(line.x + 6f, line.center.y, line.width - 12f, 1f), new Color(1f, 1f, 1f, 0.25f));
                }
                var option = _options[i];
                Rect row = GUILayoutUtility.GetRect(0f, RowHeight, GUILayout.ExpandWidth(true));
                if (RowRectProbe != null && Event.current.type == EventType.Repaint) RowRectProbe(i, row);
                if (Event.current.type == EventType.Repaint && row.Contains(Event.current.mousePosition))
                    EditorGUI.DrawRect(row, new Color(1f, 1f, 1f, 0.08f));

                GUI.Label(new Rect(row.x + 8f, row.y + 3f, row.width - 90f, 18f), option.Label, EditorStyles.boldLabel);
                if (i == inUse)
                    GUI.Label(new Rect(row.xMax - 80f, row.y + 4f, 74f, 16f), "In use", EditorStyles.miniLabel);
                GUI.Label(new Rect(row.x + 8f, row.y + 21f, row.width - 16f, RowHeight - 23f), option.Suits, SuitsStyle);

                if (IsPickClick(Event.current, row))
                {
                    Event.current.Use();
                    _onPicked(i);
                    EditorGUILayout.EndScrollView();
                    return false;
                }
            }
            EditorGUILayout.EndScrollView();
            return true;
        }
    }
}
