using System;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    // A developer-editable free-text note of one table row (Details, a band note, a long keyword
    // list), shown in the shared EditorPopup window and persisted into config.json. Built only by
    // POIEditorToolWindow.CreateDetailsPopup, which runs every keystroke through the window's
    // mutation scope (undo, unsaved flag, rig refresh, live Play Mode push) and closes the note once
    // the config it was opened on has been replaced (an undo swaps the whole config object, so the
    // row this note writes to would no longer be the one on screen).
    internal sealed class EntryDetailsPopup : EditorPopupContent
    {
        public const string PopupKind = "note";

        private readonly string _title;
        private readonly Func<string> _get;
        private readonly Action<string> _set;
        private readonly Func<bool> _isAlive;
        private string _buffer;

        public EntryDetailsPopup(string title, Func<string> get, Action<string> set, Func<bool> isAlive = null)
        {
            _title = string.IsNullOrEmpty(title) ? "Details" : title;
            _get = get;
            _set = set;
            _isAlive = isAlive;
            _buffer = get() ?? string.Empty;
        }

        public override string Kind => PopupKind;
        public override string Title => _title;
        public override Vector2 InitialSize => new Vector2(340f, 200f);
        public override bool IsAlive => _isAlive == null || _isAlive();

        // Rect of the text area in the last Repaint (tests click into it with a real mouse event)
        internal static Action<Rect> TextAreaRectProbe;

        public override bool Draw()
        {
            EditorGUILayout.LabelField("What this represents, when to use it, example POIs.", EditorStyles.wordWrappedMiniLabel);
            EditorGUI.BeginChangeCheck();
            _buffer = EditorGUILayout.TextArea(_buffer, GUILayout.ExpandHeight(true));
            if (TextAreaRectProbe != null && Event.current.type == EventType.Repaint)
                TextAreaRectProbe(GUILayoutUtility.GetLastRect());
            if (EditorGUI.EndChangeCheck())
                _set(_buffer);
            return true;
        }
    }
}
