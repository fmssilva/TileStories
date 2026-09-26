using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    // Read-only, framework-authored help text shown by an (i) button, in the shared EditorPopup
    // window. Nothing is written back anywhere. Distinct from EntryDetailsPopup (a developer-editable
    // note persisted into config.json): do not merge the two (20-code-quality.md).
    internal sealed class HelpInfoPopup : EditorPopupContent
    {
        public const string PopupKind = "help";
        private const float Width = 340f;

        private readonly string _title;
        private readonly string _body;
        private Vector2 _scroll;

        public HelpInfoPopup(string title, string body)
        {
            _title = string.IsNullOrEmpty(title) ? "Help" : title;
            _body = body ?? string.Empty;
        }

        public override string Kind => PopupKind;
        public override string Title => _title;
        internal string Body => _body;

        // Tall enough for the whole text at the start width, capped; the window scrolls beyond that
        public override Vector2 InitialSize
        {
            get
            {
                float textHeight = EditorStyles.wordWrappedLabel.CalcHeight(new GUIContent(_body), Width - 24f);
                return new Vector2(Width, Mathf.Clamp(textHeight + 24f, 90f, 420f));
            }
        }

        public override bool Draw()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            EditorGUILayout.LabelField(_body, EditorStyles.wordWrappedLabel);
            EditorGUILayout.EndScrollView();
            return true;
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
                EditorPopup.ShowAt(new HelpInfoPopup(title, bodyText), GUILayoutUtility.GetLastRect());
        }

        // Rect overload -- for a help button embedded INSIDE a wider header cell it shares with a
        // title label (e.g. the last few px of a spanning group title), rather than appended as an
        // extra column after it. Drawing it this way keeps the group's total reserved layout width
        // identical to its body row's, instead of silently adding size on top of it (the exact
        // mismatch that shifted every column after Reveal Timing/Facing Override in earlier passes).
        public static void Draw(Rect rect, string title, string bodyText)
        {
            bool clicked = POIEditorToolWindow.DrawIconButton(
                rect, (Texture2D)POIEditorToolWindow.InfoIcon.image, title, iconInset: 2f);
            if (clicked)
                EditorPopup.ShowAt(new HelpInfoPopup(title, bodyText), rect);
        }
    }
}
