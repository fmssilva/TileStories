// NavigationPresetPickerPopup.cs
//
// The navigation preset picker, shown in the shared EditorPopup window (_3.2 step 17a). A WORKING popup (the title bar's X and Esc close
// it, no button of its own): one row per preset -- its name, who it suits and a mark on the one the wall's rows already hold -- the four
// strongest first and a separator before the rest. A pick closes it and hands the preset to the caller; the caller applies it inside
// ONE mutation scope (POIEditorToolWindow.CreateNavigationPresetPickerPopup), so a preset is one undo step. Decoupled from the window.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    internal sealed class NavigationPresetPickerPopup : EditorPopupContent
    {
        public const string PopupKind = "navigation-preset-picker";
        private const float RowHeight = 56f;
        private const float SeparatorHeight = 10f;
        private const float MaxWindowHeight = 560f;

        private readonly IReadOnlyList<NavigationPresets.Preset> _presets;
        private readonly Func<NavigationPresets.Preset> _inUse;
        private readonly Action<NavigationPresets.Preset> _onPicked;
        private readonly Func<bool> _isAlive;
        private Vector2 _scroll;

        public NavigationPresetPickerPopup(IReadOnlyList<NavigationPresets.Preset> presets, Func<NavigationPresets.Preset> inUse,
            Action<NavigationPresets.Preset> onPicked, Func<bool> isAlive = null)
        {
            _presets = presets ?? throw new ArgumentNullException(nameof(presets));
            _inUse = inUse;
            _onPicked = onPicked ?? throw new ArgumentNullException(nameof(onPicked));
            _isAlive = isAlive;
        }

        private static GUIStyle _suitsStyle;
        private static GUIStyle SuitsStyle => _suitsStyle ??= new GUIStyle(EditorStyles.miniLabel) { wordWrap = true, alignment = TextAnchor.UpperLeft };

        internal static bool IsPickClick(Event e, Rect row) => e.type == EventType.MouseDown && e.button == 0 && row.Contains(e.mousePosition);

        public override string Kind => PopupKind;
        public override string Title => "Navigation Presets";
        public override bool IsAlive => _isAlive == null || _isAlive();
        internal int RowCount => _presets.Count;

        public override Vector2 InitialSize =>
            new Vector2(380f, Mathf.Min(MaxWindowHeight, _presets.Count * RowHeight + SeparatorHeight + 16f));

        // Rect of row i in the popup's last Repaint (tests click it with a real mouse event)
        internal static Action<int, Rect> RowRectProbe;

        public override bool Draw()
        {
            var inUse = _inUse?.Invoke();
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            for (int i = 0; i < _presets.Count; i++)
            {
                // - the four strongest first, a thin line, then the rest
                if (i == NavigationPresets.Recommended)
                {
                    Rect line = GUILayoutUtility.GetRect(0f, SeparatorHeight, GUILayout.ExpandWidth(true));
                    if (Event.current.type == EventType.Repaint)
                        EditorGUI.DrawRect(new Rect(line.x + 6f, line.center.y, line.width - 12f, 1f), new Color(1f, 1f, 1f, 0.25f));
                }
                var preset = _presets[i];
                Rect row = GUILayoutUtility.GetRect(0f, RowHeight, GUILayout.ExpandWidth(true));
                if (RowRectProbe != null && Event.current.type == EventType.Repaint) RowRectProbe(i, row);
                if (Event.current.type == EventType.Repaint && row.Contains(Event.current.mousePosition))
                    EditorGUI.DrawRect(row, new Color(1f, 1f, 1f, 0.08f));

                GUI.Label(new Rect(row.x + 8f, row.y + 3f, row.width - 90f, 18f), preset.Label, EditorStyles.boldLabel);
                if (ReferenceEquals(preset, inUse))
                    GUI.Label(new Rect(row.xMax - 80f, row.y + 4f, 74f, 16f), "In use", EditorStyles.miniLabel);
                GUI.Label(new Rect(row.x + 8f, row.y + 21f, row.width - 16f, RowHeight - 23f), preset.Suits, SuitsStyle);

                if (IsPickClick(Event.current, row))
                {
                    Event.current.Use();
                    _onPicked(preset);
                    EditorGUILayout.EndScrollView();
                    return false;
                }
            }
            EditorGUILayout.EndScrollView();
            return true;
        }
    }
}
