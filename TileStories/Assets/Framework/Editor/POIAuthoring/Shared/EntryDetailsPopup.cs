using System;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    // Details popup for category/badge/outline tables (section 13.5).
    // Self-contained PopupWindowContent -- takes only constructor parameters,
    // no dependency on POIAuthoringToolWindow instance state.
    public class EntryDetailsPopup : PopupWindowContent
    {
        private readonly string _title;
        private readonly Func<string> _get;
        private readonly Action<string> _set;
        private string _buffer;

        public EntryDetailsPopup(string title, Func<string> get, Action<string> set)
        {
            _title = title;
            _get = get;
            _set = set;
            _buffer = get() ?? string.Empty;
        }

        public override Vector2 GetWindowSize() => new Vector2(320f, 160f);

        public override void OnGUI(Rect rect)
        {
            EditorGUILayout.LabelField(_title, EditorStyles.boldLabel);
            EditorGUILayout.LabelField("What this represents, when to use it, example POIs.", EditorStyles.wordWrappedMiniLabel);
            EditorGUI.BeginChangeCheck();
            _buffer = EditorGUILayout.TextArea(_buffer, GUILayout.ExpandHeight(true));
            if (EditorGUI.EndChangeCheck())
                _set(_buffer);

            EditorGUILayout.Space(4f);
            if (GUILayout.Button("Close", GUILayout.Height(22f)))
            {
                var focused = EditorWindow.focusedWindow;
                if (focused is PopupWindow pw)
                    pw.Close();
                else if (focused != null)
                    focused.Close();
            }
        }
    }
}