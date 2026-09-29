// CardMediaDefaultPickerPopup.cs
//
// A default-media key picker, shown in the shared EditorPopup window (_3.1 step 13). Same shape as
// ExistingSymbolPickerPopup: lists the wall's own default media library and the Framework's, de-duplicated by
// asset, filtered to the field's own MediaKind (a picture field never offers an audio key). Picking one stores
// "default:<key>" through the caller's assignKey. Decoupled from POIEditorToolWindow; built only by
// POIEditorToolWindow.CreateCardMediaDefaultPickerPopup (the pick runs in a mutation scope).

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    internal sealed class CardMediaDefaultPickerPopup : EditorPopupContent
    {
        public const string PopupKind = "card-media-default-picker";
        private const float ThumbSize = 32f;
        private const float RowHeight = 40f;
        private const float MaxWindowHeight = 360f;

        private readonly List<CardMediaLibrary.Entry> _rows = new();
        private readonly Action<string> _onPicked;
        private readonly Func<bool> _isAlive;
        private Vector2 _scroll;

        public CardMediaDefaultPickerPopup(MediaKind kind, CardMediaLibrary wallLibrary, CardMediaLibrary frameworkLibrary,
            Action<string> onPicked, Func<bool> isAlive = null)
        {
            _onPicked = onPicked ?? throw new ArgumentNullException(nameof(onPicked));
            _isAlive = isAlive;
            Collect(kind, wallLibrary);
            Collect(kind, frameworkLibrary);
        }

        private void Collect(MediaKind kind, CardMediaLibrary library)
        {
            if (library == null) return;
            foreach (var entry in library.Entries)
            {
                if (entry.kind != kind || string.IsNullOrWhiteSpace(entry.key) || entry.asset == null) continue;
                bool already = false;
                for (int i = 0; i < _rows.Count; i++)
                    if (_rows[i].key == entry.key) { already = true; break; }
                if (!already) _rows.Add(entry);
            }
        }

        private static GUIStyle _labelStyle;
        private static GUIStyle LabelStyle => _labelStyle ??= new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleLeft };

        internal static bool IsPickClick(Event e, Rect row) => e.type == EventType.MouseDown && e.button == 0 && row.Contains(e.mousePosition);

        public override string Kind => PopupKind;
        public override string Title => "Pick Default Media";
        public override bool IsAlive => _isAlive == null || _isAlive();
        internal int RowCount => _rows.Count;

        public override Vector2 InitialSize => new Vector2(260f, Mathf.Min(MaxWindowHeight, Mathf.Max(1, _rows.Count) * RowHeight + 16f));

        // Rect of row i in the popup's last Repaint (tests click it with a real mouse event)
        internal static Action<int, Rect> RowRectProbe;

        public override bool Draw()
        {
            if (_rows.Count == 0)
            {
                EditorGUILayout.LabelField("No default media of this kind.", EditorStyles.wordWrappedLabel);
                return true;
            }

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            for (int i = 0; i < _rows.Count; i++)
            {
                var entry = _rows[i];
                Rect row = GUILayoutUtility.GetRect(0f, RowHeight, GUILayout.ExpandWidth(true));
                if (RowRectProbe != null && Event.current.type == EventType.Repaint)
                    RowRectProbe(i, row);
                if (Event.current.type == EventType.Repaint && row.Contains(Event.current.mousePosition))
                    EditorGUI.DrawRect(row, new Color(1f, 1f, 1f, 0.08f));

                Texture2D thumb = AssetPreview.GetAssetPreview(entry.asset) ?? AssetPreview.GetMiniThumbnail(entry.asset);
                if (thumb != null)
                    GUI.DrawTexture(new Rect(row.x + 4f, row.y + (RowHeight - ThumbSize) * 0.5f, ThumbSize, ThumbSize), thumb, ScaleMode.ScaleToFit);
                GUI.Label(new Rect(row.x + ThumbSize + 12f, row.y, row.width - ThumbSize - 12f, RowHeight), entry.key, LabelStyle);

                if (IsPickClick(Event.current, row))
                {
                    Event.current.Use();
                    _onPicked(entry.key);
                    EditorGUILayout.EndScrollView();
                    return false;
                }
            }
            EditorGUILayout.EndScrollView();
            return true;
        }
    }
}
