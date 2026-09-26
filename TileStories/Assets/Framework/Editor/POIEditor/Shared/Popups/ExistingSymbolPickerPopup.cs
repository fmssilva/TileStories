// ExistingSymbolPickerPopup.cs
//
// A curated symbol picker, shown in the shared EditorPopup window. Unity's built-in
// ObjectField sprite picker lists every Sprite in the whole project, including
// unrelated sprites from TMP's emoji atlas and unrelated walls. This popup lists
// only the two libraries that matter to a wall -- the wall's own icon library and
// the framework default icon library -- so a developer can pick a symbol key
// without the noise. Decoupled from POIEditorToolWindow: it only needs the
// two libraries and a callback that accepts the chosen key. Built only by
// POIEditorToolWindow.CreateSymbolPickerPopup (the pick runs in a mutation scope).

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    internal sealed class ExistingSymbolPickerPopup : EditorPopupContent
    {
        public const string PopupKind = "symbol-picker";
        private const float ThumbSize = 32f;
        private const float RowHeight = 40f;
        private const float MaxWindowHeight = 360f;

        private readonly List<SpriteKeyLibrary.Entry> _rows = new();
        private readonly Action<string> _onPicked;
        private readonly Func<bool> _isAlive;
        private Vector2 _scroll;

        public ExistingSymbolPickerPopup(SpriteKeyLibrary wallLibrary,
            SpriteKeyLibrary defaultLibrary, Action<string> onPicked, Func<bool> isAlive = null)
        {
            _onPicked = onPicked ?? throw new ArgumentNullException(nameof(onPicked));
            _isAlive = isAlive;
            Collect(wallLibrary);
            Collect(defaultLibrary);
        }

        // Gather every icon-bearing entry, de-duplicated by sprite instance so the
        // same glyph shared between the wall library and the framework default is
        // offered only once.
        private void Collect(SpriteKeyLibrary library)
        {
            if (library == null)
                return;

            foreach (var entry in library.Entries)
            {
                if (string.IsNullOrWhiteSpace(entry.key) || entry.sprite == null)
                    continue;

                bool alreadyPresent = false;
                for (int i = 0; i < _rows.Count; i++)
                {
                if (ReferenceEquals(_rows[i].sprite, entry.sprite))
                    {
                        alreadyPresent = true;
                        break;
                    }
                }
                if (!alreadyPresent)
                    _rows.Add(entry);
            }
        }

        private static GUIStyle _labelStyle;
        private static GUIStyle LabelStyle => _labelStyle ??= new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.MiddleLeft
        };

        // A left mouse-down anywhere inside the row picks it (pure, so the rule is unit-tested).
        internal static bool IsPickClick(Event e, Rect row) =>
            e.type == EventType.MouseDown && e.button == 0 && row.Contains(e.mousePosition);

        public override string Kind => PopupKind;
        public override string Title => "Choose Existing Symbol";
        public override bool IsAlive => _isAlive == null || _isAlive();
        internal int RowCount => _rows.Count;

        public override Vector2 InitialSize =>
            new Vector2(260f, Mathf.Min(MaxWindowHeight, _rows.Count * RowHeight + 16f));

        // Rect of row i in the popup's last Repaint (tests click it with a real mouse event)
        internal static Action<int, Rect> RowRectProbe;

        public override bool Draw()
        {
            if (_rows.Count == 0)
            {
                EditorGUILayout.LabelField("No symbols found.", EditorStyles.wordWrappedLabel);
                return true;
            }

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            for (int i = 0; i < _rows.Count; i++)
            {
                var entry = _rows[i];
                // The WHOLE row is the button (thumbnail, name and the empty space to its right).
                Rect row = GUILayoutUtility.GetRect(0f, RowHeight, GUILayout.ExpandWidth(true));
                if (RowRectProbe != null && Event.current.type == EventType.Repaint)
                    RowRectProbe(i, row);
                if (Event.current.type == EventType.Repaint && row.Contains(Event.current.mousePosition))
                    EditorGUI.DrawRect(row, new Color(1f, 1f, 1f, 0.08f));

                Texture2D thumb = AssetPreview.GetAssetPreview(entry.sprite);
                if (thumb != null)
                    GUI.DrawTexture(new Rect(row.x + 4f, row.y + (RowHeight - ThumbSize) * 0.5f, ThumbSize, ThumbSize),
                        thumb, ScaleMode.ScaleToFit);
                GUI.Label(new Rect(row.x + ThumbSize + 12f, row.y, row.width - ThumbSize - 12f, RowHeight),
                    entry.key, LabelStyle);

                if (IsPickClick(Event.current, row))
                {
                    Event.current.Use();
                    _onPicked(entry.key);
                    EditorGUILayout.EndScrollView();
                    return false;   // picked: the popup closes
                }
            }
            EditorGUILayout.EndScrollView();
            return true;
        }
    }
}
