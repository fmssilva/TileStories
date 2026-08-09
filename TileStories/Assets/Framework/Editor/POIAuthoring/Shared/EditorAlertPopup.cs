using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    // Reusable floating alert popup for validation warnings/errors surfaced
    // by the POI Authoring Tool. Unlike HelpInfoPopup (static, read-only,
    // one title+body), this popup renders a scrollable list of one or more
    // warning items, each with its own POI id, key value, and fix guidance.
    // Non-blocking -- the user can close it and keep working.
    internal class EditorAlertPopup : PopupWindowContent
    {
        private readonly string _title;
        private readonly List<EditorAlertItem> _items;
        private readonly string _fixGuidance;
        private const float WindowWidth = 340f;
        private const float WindowHeight = 260f;
        private Vector2 _scrollPos;

        public EditorAlertPopup(string title, List<EditorAlertItem> items, string fixGuidance = null)
        {
            _title = title;
            _items = items ?? new List<EditorAlertItem>();
            _fixGuidance = fixGuidance ?? string.Empty;
        }

        public override Vector2 GetWindowSize() => new Vector2(WindowWidth, WindowHeight);

        public override void OnGUI(Rect rect)
        {
            EditorGUILayout.LabelField(_title, EditorStyles.boldLabel);

            if (_items.Count == 0)
            {
                EditorGUILayout.LabelField("No issues found.", EditorStyles.wordWrappedMiniLabel);
            }
            else
            {
                EditorGUILayout.LabelField($"{_items.Count} issue(s):", EditorStyles.wordWrappedMiniLabel);
                EditorGUILayout.Space(4f);

                _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Height(140f));
                foreach (var item in _items)
                {
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    EditorGUILayout.LabelField(item.poiId, EditorStyles.boldLabel);
                    EditorGUILayout.LabelField($"Value: {item.value}", EditorStyles.wordWrappedMiniLabel);
                    EditorGUILayout.LabelField(item.problem, EditorStyles.wordWrappedMiniLabel);
                    if (!string.IsNullOrEmpty(item.fixHint))
                        EditorGUILayout.LabelField($"Fix: {item.fixHint}", EditorStyles.wordWrappedMiniLabel);
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(2f);
                }
                EditorGUILayout.EndScrollView();
            }

            if (!string.IsNullOrEmpty(_fixGuidance))
            {
                EditorGUILayout.Space(8f);
                EditorGUILayout.HelpBox(_fixGuidance, MessageType.Info);
            }

            EditorGUILayout.Space(4f);
            if (GUILayout.Button("Close", GUILayout.Height(22f)))
            {
                CloseWindow();
            }
        }

        private void CloseWindow()
        {
            var focused = EditorWindow.focusedWindow;
            if (focused is PopupWindow pw)
                pw.Close();
            else if (focused != null)
                focused.Close();
        }
    }

    // One row in an EditorAlertPopup list.
    internal struct EditorAlertItem
    {
        public readonly string poiId;
        public readonly string value;
        public readonly string problem;
        public readonly string fixHint;

        public EditorAlertItem(string poiId, string value, string problem, string fixHint = null)
        {
            this.poiId = poiId;
            this.value = value;
            this.problem = problem;
            this.fixHint = fixHint;
        }
    }

    internal static class EditorAlertButton
    {
        // Draws a small warning triangle button that opens an EditorAlertPopup
        // anchored to the button's own last-drawn rect. Call after a validation
        // scan that returns a non-empty list of issues.
        public static void Draw(string title, List<EditorAlertItem> items, string fixGuidance = null)
        {
            if (items == null || items.Count == 0)
                return;

            if (GUILayout.Button("!", GUILayout.Width(26f), GUILayout.Height(20f)))
                PopupWindow.Show(GUILayoutUtility.GetLastRect(), new EditorAlertPopup(title, items, fixGuidance));
        }
    }
}
