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

    // The ONE help-button look for this whole window: every "(i)" affordance anywhere
    // in the POI Editor (Position/Rotation rows, taxonomy table headers, LOD/Zoom
    // fields, ...) goes through this single method instead of each call site picking
    // its own size/style -- that per-call-site drift is exactly what previously made
    // Position's help button render bigger than Rotation's despite both being meant to
    // look identical. Built on the same DrawIconButton primitive (real button
    // background + hover/press feedback) the POI header row's icon cluster uses, so
    // every icon button in the tool -- help or otherwise -- shares one visual family.
    internal static class HelpInfoButton
    {
        // Use where a field/section's purpose isn't self-evident from its label alone
        // -- not blanket-applied to every field (visual noise). `size` only needs to be
        // overridden to line up with a specific neighboring column (e.g. a 36px-wide
        // taxonomy table's Preview column); leave it at the default everywhere else.
        public static void Draw(string title, string bodyText, float size = POIEditorToolWindow.IconButtonSize)
        {
            bool clicked = POIEditorToolWindow.DrawIconButtonLayout(
                (Texture2D)POIEditorToolWindow.InfoIcon.image, title, size, iconInset: 2f);
            if (clicked)
                PopupWindow.Show(GUILayoutUtility.GetLastRect(), new HelpInfoPopup(title, bodyText));
        }
    }
}
