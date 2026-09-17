using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    // Read-only variant of EntryDetailsPopup's PopupWindowContent -- static body text,
    // no text-edit buffer, nothing written back anywhere. Used for fixed, framework-authored
    // explanations of a column's purpose; distinct from EntryDetailsPopup (which persists
    // developer-editable notes into config.json). Do not merge the two.
    internal class HelpInfoPopup : PopupWindowContent
    {
        private readonly string _title;
        private readonly string _body;
        private const float WindowWidth = 300f;
        private const float WindowHeight = 140f;

        public HelpInfoPopup(string title, string body)
        {
            _title = title;
            _body = body ?? string.Empty;
        }

        // Window height scales with body length so multi-paragraph help
        // (e.g. the Symbol/picker/Preview explanation) never clips.
        public override Vector2 GetWindowSize()
        {
            float lines = Mathf.CeilToInt(_body.Length / 55f) + 2f;
            float h = Mathf.Max(WindowHeight, 46f + lines * 14f);
            return new Vector2(WindowWidth, h);
        }

        public override void OnGUI(Rect rect)
        {
            EditorGUILayout.LabelField(_title, EditorStyles.boldLabel);
            EditorGUILayout.LabelField(_body, EditorStyles.wordWrappedMiniLabel);

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

    internal static class HelpInfoButton
    {
        // Draws a small "(i)" button; on click opens HelpInfoPopup anchored to the
        // button's own last-drawn rect. Call from any partial-class file.
        // Use where a column's purpose isn't self-evident from its label alone --
        // not blanket-applied to every field (visual noise).
        public static void Draw(string title, string bodyText)
        {
            if (GUILayout.Button("(i)", GUILayout.Width(26f), GUILayout.Height(20f)))
                PopupWindow.Show(GUILayoutUtility.GetLastRect(), new HelpInfoPopup(title, bodyText));
        }
        // Icon-only twin of Draw: 26f footprint in a fixed-width header cell by
        // default. Callers that sit above the 36f Preview column pass 36f so the
        // button aligns with the preview's width for clean vertical stacking.
        public static void DrawCompact(string title, string bodyText, float width = 26f)
        {
            if (GUILayout.Button(POIEditorToolWindow.InfoIcon, GUILayout.Width(width), GUILayout.Height(20f)))
                PopupWindow.Show(GUILayoutUtility.GetLastRect(), new HelpInfoPopup(title, bodyText));
        }
    }
}
