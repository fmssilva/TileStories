// POIEditorToolWindow.CardStarter.cs
//
// Partial: Specific Marker > Card Content > Card Starter (_3.2 step 17b; the starters themselves are data, CardStarters). The row is the
// section's first: a popup-looking button that names the starter the point's blocks follow (or Custom) and opens the picker. A pick
// applies the starter inside ONE mutation scope (one Ctrl+Z gives the point back), with no question on a point that has no blocks and
// ONE question first on a point that has some: what would be replaced is authored content, never overwritten silently.

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    public partial class POIEditorToolWindow
    {
        // The Card Starter row: the button reads the starter the blocks follow (Start from scratch with none, else Custom) and opens the
        // picker. Drawing never writes: only a pick does, through ApplyCardStarter
        private void DrawCardStarterRow(POIData poi)
        {
            var current = CardStarters.Matching(poi);
            DrawEditorRow(out float rowWidth, out _);
            float width = FieldWidth(rowWidth, CardStarterHelp);
            Rect row = GUILayoutUtility.GetRect(width, EditorGUIUtility.singleLineHeight, EditorStyles.popup, GUILayout.Width(width), GUILayout.ExpandWidth(false));
            Rect value = EditorGUI.PrefixLabel(row, new GUIContent("Card Starter"));
            if (FieldRowRectProbe != null && Event.current.type == EventType.Repaint) FieldRowRectProbe("Card Starter", value);
            if (GUI.Button(value, current != null ? current.Label : "Custom", EditorStyles.popup))
                EditorPopup.ShowAt(CreateCardStarterPickerPopup(poi.id), value);
            DrawRowHelp("Card Starter", CardStarterHelp);
            EditorRowEnd();
        }

        // Apply `starter` to the point `poiId` (found again by id: the picker may outlive an undo). On a point with blocks, ask first --
        // every answer's outcome:
        //   Confirm ("Replace N Blocks" / "Remove N Blocks"): the blocks are replaced in ONE undo step; Ctrl+Z gives them back
        //   Cancel (also Esc, the X): nothing changes, no undo step, the config stays saved or unsaved as it was
        // On a point with no blocks it asks nothing (nothing authored is at stake, and the apply is undoable)
        internal void ApplyCardStarter(string poiId, CardStarters.Starter starter)
        {
            int index = _config?.pois?.FindIndex(p => p != null && p.id == poiId) ?? -1;
            if (index < 0 || starter == null) return;
            var poi = _config.pois[index];
            int blocks = poi.card?.blocks?.Count ?? 0;
            if (blocks > 0)
            {
                var question = BuildCardStarterQuestion(EditorNames.Poi(index, poi), blocks, starter.Label, starter.Entries.Count);
                if (EditorDecision.Ask(question) != DecisionAnswer.Confirm) return;
            }
            ApplyPopupEdit(() => CardStarters.Apply(poi, starter));
        }

        // Pure: the question a starter asks over a point's blocks, from the real state -- the point by its list title, how many blocks go,
        // what comes instead, and what each button does
        internal static DecisionRequest BuildCardStarterQuestion(string pointName, int blockCount, string starterLabel, int newBlockCount)
        {
            string blocks = blockCount == 1 ? "1 block" : blockCount + " blocks";
            bool scratch = newBlockCount == 0;
            string confirm = (scratch ? "Remove " : "Replace ") + (blockCount == 1 ? "1 Block" : blockCount + " Blocks");
            var lines = new List<string>
            {
                $"{pointName} has {blocks} on its card, with everything written in them.",
                scratch
                    ? $"{starterLabel} removes them all: the card shows the point's name and summary until you add blocks."
                    : $"{starterLabel} replaces them with its own {newBlockCount} blocks, with nothing written in them yet.",
                $"{confirm}: they go, and one Ctrl+Z gives them back. Cancel: nothing changes.",
            };
            return new DecisionRequest
            {
                Title = "Replace this card's blocks?",
                Message = string.Join("\n", lines),
                ConfirmLabel = confirm,
            };
        }
    }
}
