using System;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    // One EditorNotice message in the shared EditorPopup window. A notice is a MESSAGE the developer
    // acknowledges, not a panel they work in, so it follows the message-box convention: the text,
    // then one bottom row with "Don't show this again" on the left (advisory notices only) and the
    // default "OK" on the right (Enter presses it; X and Esc still close it too). The checkbox is a
    // choice about NEXT time, so it is committed when the notice closes, however it closes.
    // Nothing waits for the answer: the Editor keeps running while it is open.
    internal sealed class NoticePopup : EditorPopupContent
    {
        public const string PopupKind = "notice";
        public const string DontShowAgainLabel = "Don't show this again";
        public const string OkLabel = "OK";
        private const float Width = 380f;
        private const float OkWidth = 80f;

        private readonly string _title;
        private readonly string _message;
        private readonly string _dontShowAgainKey;
        private bool _dontShowAgain;
        private Vector2 _scroll;

        public NoticePopup(string title, string message, string dontShowAgainKey)
        {
            _title = string.IsNullOrEmpty(title) ? "Notice" : title;
            _message = message ?? string.Empty;
            _dontShowAgainKey = dontShowAgainKey;
        }

        public override string Kind => PopupKind;
        public override string Title => _title;
        internal string Message => _message;
        internal string DontShowAgainKey => _dontShowAgainKey;

        // Rects of the checkbox and the OK button in the last Repaint (tests click them for real)
        internal static Action<Rect> DontShowAgainRectProbe;
        internal static Action<Rect> OkRectProbe;

        public override Vector2 InitialSize
        {
            get
            {
                float textHeight = EditorStyles.wordWrappedLabel.CalcHeight(new GUIContent(_message), Width - 24f);
                float buttonRow = EditorGUIUtility.singleLineHeight + 16f;
                return new Vector2(Width, Mathf.Clamp(textHeight + buttonRow + 24f, 100f, 480f));
            }
        }

        public override bool Draw()
        {
            // Enter = the default button, checked first so nothing else can take the key
            var e = Event.current;
            if (e.type == EventType.KeyDown && (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter))
            {
                e.Use();
                return false;
            }

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            EditorGUILayout.LabelField(_message, EditorStyles.wordWrappedLabel);
            EditorGUILayout.EndScrollView();

            bool ok;
            EditorGUILayout.Space(4f);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (_dontShowAgainKey != null)
                {
                    _dontShowAgain = EditorGUILayout.ToggleLeft(DontShowAgainLabel, _dontShowAgain, GUILayout.ExpandWidth(false));
                    if (DontShowAgainRectProbe != null && e.type == EventType.Repaint)
                        DontShowAgainRectProbe(GUILayoutUtility.GetLastRect());
                }
                GUILayout.FlexibleSpace();
                ok = GUILayout.Button(OkLabel, GUILayout.Width(OkWidth), GUILayout.ExpandWidth(false));
                if (OkRectProbe != null && e.type == EventType.Repaint)
                    OkRectProbe(GUILayoutUtility.GetLastRect());
            }
            EditorGUILayout.Space(4f);
            return !ok;
        }

        // The checkbox counts however the notice is closed (OK, Enter, X, Esc)
        public override void OnClosed()
        {
            if (_dontShowAgainKey != null && _dontShowAgain)
                EditorNotice.Hide(_dontShowAgainKey);
        }
    }
}
