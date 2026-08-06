// ExistingSymbolPickerPopup.cs
//
// A curated symbol picker (section 14.7 of _5.1_Editor_Tab.md). Unity's built-in
// ObjectField sprite picker lists every Sprite in the whole project, including
// unrelated sprites from TMP's emoji atlas and unrelated walls. This popup lists
// only the two libraries that matter to a wall -- the wall's own icon library and
// the framework default icon library -- so a developer can pick a symbol key
// without the noise. Decoupled from POIAuthoringToolWindow: it only needs the
// two libraries and a callback that accepts the chosen key.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    internal sealed class ExistingSymbolPickerPopup : PopupWindowContent
    {
        private const float ThumbSize = 32f;
        private const float RowHeight = 40f;
        private const float MaxWindowHeight = 360f;

        private readonly List<SpriteKeyLibrary.Entry> _rows = new();
        private readonly Action<string> _onPicked;
        private Vector2 _scroll;

        public ExistingSymbolPickerPopup(SpriteKeyLibrary wallLibrary,
            SpriteKeyLibrary defaultLibrary, Action<string> onPicked)
        {
            _onPicked = onPicked ?? throw new ArgumentNullException(nameof(onPicked));
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

        public override Vector2 GetWindowSize() =>
            new Vector2(260f, Mathf.Min(MaxWindowHeight, 40f + _rows.Count * RowHeight + 20f));

        public override void OnGUI(Rect rect)
        {
            if (_rows.Count == 0)
            {
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("No symbols found.", EditorStyles.wordWrappedLabel);
                if (GUILayout.Button("Close"))
                    editorWindow?.Close();
                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUILayout.LabelField("Choose existing symbol", EditorStyles.boldLabel);
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            foreach (var entry in _rows)
            {
                Texture2D thumb = AssetPreview.GetAssetPreview(entry.sprite);
                GUILayout.BeginHorizontal(GUILayout.Height(RowHeight));
                if (GUILayout.Button(thumb, GUIStyle.none,
                    GUILayout.Width(ThumbSize), GUILayout.Height(ThumbSize)))
                {
                    _onPicked(entry.key);
                    editorWindow?.Close();
                    return;
                }
                GUILayout.Label(entry.key, GUILayout.Height(18f));
                GUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Cancel"))
                editorWindow?.Close();
        }
    }
}
