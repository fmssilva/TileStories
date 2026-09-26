using UnityEngine;

namespace TileStories.Editor
{
    // "Marker Label Style": one hierarchy level's own label gap/font-size/font (the Hierarchy Levels
    // table's "Aa" button, _2.0_Labels_And_Fonts_Design.md section 4), shown in the shared EditorPopup
    // window. The fields are drawn by the owning POI Editor window (DrawLevelLabelStyleEditor,
    // LabelsAndFonts.cs) so every edit runs through that window's DrawConfigMutationScope -- undo/redo,
    // the unsaved flag, the Scene rig refresh and the live Play Mode push all come from the one path
    // every other field uses. The level is held by KEY, not by reference, so an Undo that swaps the
    // whole config never leaves it editing a stale object. Distinct from EntryDetailsPopup on purpose:
    // that holds a free-text note, this holds functional config (20-code-quality.md).
    internal sealed class LevelLabelStylePopup : EditorPopupContent
    {
        public const string PopupKind = "label-style";
        public const string WindowTitle = "Marker Label Style";

        private readonly POIEditorToolWindow _owner;
        private readonly string _levelKey;

        private LevelLabelStylePopup(POIEditorToolWindow owner, string levelKey)
        {
            _owner = owner;
            _levelKey = levelKey;
        }

        internal string LevelKey => _levelKey;

        public override string Kind => PopupKind;
        public override string Title => WindowTitle;
        public override Vector2 InitialSize => new Vector2(380f, 170f);
        public override bool IsAlive => _owner != null;

        // Show the popup for one level; opening another level's "Aa" retargets the open one
        internal static EditorPopup Open(POIEditorToolWindow owner, string levelKey, Rect anchorGuiRect)
        {
            return EditorPopup.ShowAt(new LevelLabelStylePopup(owner, levelKey), anchorGuiRect);
        }

        internal static EditorPopup Open(POIEditorToolWindow owner, string levelKey)
        {
            return EditorPopup.Show(new LevelLabelStylePopup(owner, levelKey));
        }

        // The level was deleted or undone away: nothing left to edit, the popup closes
        public override bool Draw() => _owner.DrawLevelLabelStyleEditor(_levelKey);
    }
}
