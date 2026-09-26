using System;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    // What one popup shows: a help text, a note, a symbol picker, a hierarchy level's label style,
    // a notice. The window around it (EditorPopup) is the same for all of them, so every popup in
    // the POI Editor drags, closes and behaves the same way ("Popups: the two kinds", _5.1).
    internal abstract class EditorPopupContent
    {
        // One open popup per kind: showing another content of the same kind reuses that window
        // (clicking a second (i) retargets the open help popup instead of stacking a new one).
        public abstract string Kind { get; }
        public abstract string Title { get; }
        public virtual Vector2 InitialSize => new Vector2(320f, 180f);

        // False once there is nothing left to show or edit (owner window closed, row deleted, the
        // config it was opened on replaced by an undo): the popup closes instead of editing a dead object.
        public virtual bool IsAlive => true;

        // Draw the body. Return false to close the popup (a pick was made, the edited row is gone).
        public abstract bool Draw();

        // Called once when the popup closes, whichever way (its own button, X, Esc, retargeted away)
        public virtual void OnClosed() { }
    }

    // The ONE non-blocking popup window of the POI Editor: a floating utility window with the OS
    // title bar (drag it anywhere, X closes it), Esc closes it too, and it never blocks the Editor,
    // so Unity MCP, a running Play Mode and a test run keep going while it is open. Only a question
    // the caller must wait for goes to EditorDecision (a native modal dialog) instead.
    //
    // Replaces Unity's PopupWindowContent on purpose: that one is anchored, cannot be dragged and
    // closes on the first click outside it, so a note or a style could not stay open next to the
    // Game view while the developer watched the result.
    internal sealed class EditorPopup : EditorWindow
    {
        private const float AnchorGap = 4f;

        // Content objects live only in memory: after a domain reload the window has none and closes
        [NonSerialized] private EditorPopupContent _content;

        internal EditorPopupContent Content => _content;

        // Show content next to a GUI-space rect of the calling window (call from inside its OnGUI)
        internal static EditorPopup ShowAt(EditorPopupContent content, Rect anchorGuiRect)
        {
            return Show(content, GUIUtility.GUIToScreenRect(anchorGuiRect));
        }

        // Show content: reuse the open popup of the same kind (keeping where the developer dragged it),
        // else open a new one under the anchor, or centred on the Editor when there is no anchor.
        internal static EditorPopup Show(EditorPopupContent content, Rect? anchorScreenRect = null)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));

            EditorPopup window = FindOpen(content.Kind);
            bool reused = window != null;
            if (!reused)
                window = CreateInstance<EditorPopup>();

            if (reused && !ReferenceEquals(window._content, content))
                window._content.OnClosed();   // the old content leaves: it closes too
            window._content = content;
            window.titleContent = new GUIContent(content.Title);
            window.minSize = new Vector2(200f, 80f);

            if (!reused)
            {
                window.ShowUtility();
                window.position = InitialPosition(content.InitialSize, anchorScreenRect);
            }
            window.Focus();
            window.Repaint();
            return window;
        }

        // The open popup of a kind, if any (null content = a leftover from before a domain reload)
        internal static EditorPopup FindOpen(string kind)
        {
            foreach (var window in Resources.FindObjectsOfTypeAll<EditorPopup>())
                if (window != null && window._content != null && window._content.Kind == kind)
                    return window;
            return null;
        }

        // Close every open popup (tests tidy up with it)
        internal static void CloseAll()
        {
            foreach (var window in Resources.FindObjectsOfTypeAll<EditorPopup>())
                if (window != null)
                    window.Close();
        }

        // Under the anchor's left edge, else centred on the main Editor window
        private static Rect InitialPosition(Vector2 size, Rect? anchor)
        {
            if (anchor.HasValue)
                return new Rect(anchor.Value.x, anchor.Value.yMax + AnchorGap, size.x, size.y);

            Rect main = EditorGUIUtility.GetMainWindowPosition();
            return new Rect(main.center.x - size.x * 0.5f, main.center.y - size.y * 0.5f, size.x, size.y);
        }

        private void OnGUI()
        {
            if (_content == null || !_content.IsAlive)
            {
                Close();
                return;
            }

            // Esc closes any popup; checked before the body so a text area cannot swallow it
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
            {
                Event.current.Use();
                Close();
                return;
            }

            if (!_content.Draw())
                Close();
        }

        // A notice or help text can change behind an open popup; repaint it ~10x a second
        private void OnInspectorUpdate() => Repaint();

        // Every way a popup closes ends here (its own button, X, Esc, Close() from code)
        private void OnDestroy()
        {
            var content = _content;
            _content = null;
            content?.OnClosed();
        }
    }
}
