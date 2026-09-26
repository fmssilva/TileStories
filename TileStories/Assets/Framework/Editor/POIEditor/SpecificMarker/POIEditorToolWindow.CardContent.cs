// POIEditorToolWindow.CardContent.cs
//
// Partial: Specific Marker > "Card Content" (_3.1 section 8.2), one POI's Detail Card blocks, top to bottom. A
// table (reorder, Kind, Variant, Display, open, delete) whose open rows draw the block's fields through the
// generic BlockFieldDrawer; under a row that would not show, the reason BlockStackBuilder gives. "+ Add block"
// appends the kind picked beside it. A delete asks nothing: Ctrl+Z brings the block back.

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    public partial class POIEditorToolWindow
    {
        private const float CardBlockMoveButtonWidth = 22f;
        private const float CardBlockKindColumnWidth = 130f;
        private const float CardBlockVariantColumnWidth = 150f;
        private const float CardBlockDisplayColumnWidth = 90f;
        private const string CardBlockDefaultVariantLabel = "(Block Library default)";

        // Which block rows are open, by "poiId/blockKey" (UI state, never saved)
        private readonly Dictionary<string, bool> _cardBlockFoldouts = new();
        // The kind "+ Add block" adds (index into BlockRegistry.Shared.All)
        private int _newCardBlockKindIndex;

        private void DrawPoiCardContent(POIData poi)
        {
            if (poi == null) return;
            var settings = _config.card_settings ?? new CardSettings();
            if (!settings.enabled)
                EditorGUILayout.HelpBox(CardContentOffNote, MessageType.Info);

            DrawEditorRow(out float titleRow, out _);
            GUILayout.Label("Blocks", EditorStyles.miniBoldLabel, GUILayout.Width(Mathf.Max(40f, titleRow - 36f)), GUILayout.ExpandWidth(false));
            HelpInfoButton.Draw("Card Content", CardContentHelp);
            EditorRowEnd();

            var blocks = poi.card?.blocks ?? new List<BlockInstanceData>();
            var built = BlockStackBuilder.Build(poi, settings, BlockRegistry.Shared);
            int deleteIndex = -1, moveIndex = -1, moveBy = 0;

            for (int i = 0; i < blocks.Count; i++)
            {
                var block = blocks[i];
                BlockRegistry.Shared.TryGet(block.kind, out var definition);
                string foldoutKey = poi.id + "/" + block.key;
                _cardBlockFoldouts.TryGetValue(foldoutKey, out bool open);

                using (new TableRowScope())
                {
                    using (new EditorGUI.DisabledScope(i == 0))
                        if (GUILayout.Button("^", EditorStyles.miniButton, GUILayout.Width(CardBlockMoveButtonWidth))) { moveIndex = i; moveBy = -1; }
                    ReportTableCellRect("Card block up", i);
                    using (new EditorGUI.DisabledScope(i >= blocks.Count - 1))
                        if (GUILayout.Button("v", EditorStyles.miniButton, GUILayout.Width(CardBlockMoveButtonWidth))) { moveIndex = i; moveBy = 1; }
                    ReportTableCellRect("Card block down", i);

                    string kindName = definition != null ? definition.DisplayName : block.kind + " (unknown)";
                    var foldRect = GUILayoutUtility.GetRect(new GUIContent(kindName), EditorStyles.foldout,
                        GUILayout.Width(CardBlockKindColumnWidth), GUILayout.ExpandWidth(false));
                    open = EditorGUI.Foldout(foldRect, open, kindName, true);
                    ReportTableCellRect("Card block open", i, foldRect);

                    if (definition != null)
                    {
                        var variantOptions = new List<string> { "" };
                        variantOptions.AddRange(definition.Variants);
                        var variantLabels = new List<string> { CardBlockDefaultVariantLabel };
                        variantLabels.AddRange(definition.Variants);
                        int variantIndex = Mathf.Max(0, variantOptions.IndexOf(block.variant ?? ""));
                        int pickedVariant = EditorGUILayout.Popup(variantIndex, variantLabels.ToArray(), GUILayout.Width(CardBlockVariantColumnWidth));
                        if (pickedVariant != variantIndex) block.variant = variantOptions[pickedVariant];

                        var modes = new List<string>(definition.DisplayModes);
                        int modeIndex = modes.IndexOf(block.display);
                        int pickedMode = EditorGUILayout.Popup(modeIndex, modes.ToArray(), GUILayout.Width(CardBlockDisplayColumnWidth));
                        if (pickedMode >= 0 && pickedMode != modeIndex) block.display = modes[pickedMode];
                    }

                    GUILayout.FlexibleSpace();
                    GUILayout.Space(TableGapBeforeDelete);
                    if (DeleteButton.DrawLayout("Delete block: " + kindName)) deleteIndex = i;
                    ReportTableCellRect("Card block delete", i);
                    GUILayout.Space(AddButtonRowRightMargin);
                }
                _cardBlockFoldouts[foldoutKey] = open;

                foreach (var skipped in built.Skipped)
                    if (ReferenceEquals(skipped.Instance, block))
                        EditorGUILayout.HelpBox(CardBlockSkipText(skipped.Reason, definition?.Field(skipped.FieldKey)?.Label ?? skipped.FieldKey), MessageType.Warning);

                if (open && definition != null)
                    DrawBlockFields(block, definition, i);
            }

            // - applied after the loop: a row is never left half-drawn
            if (deleteIndex >= 0) blocks.RemoveAt(deleteIndex);
            if (moveIndex >= 0)
            {
                int target = moveIndex + moveBy;
                (blocks[moveIndex], blocks[target]) = (blocks[target], blocks[moveIndex]);
            }

            DrawCardAddBlockRow(poi);
        }

        // The kind picker + "+ Add block": appends a new block of that kind, open, with a fresh key
        private void DrawCardAddBlockRow(POIData poi)
        {
            var kinds = BlockRegistry.Shared.All;
            if (kinds.Count == 0) return;
            var labels = new string[kinds.Count];
            for (int k = 0; k < kinds.Count; k++) labels[k] = kinds[k].Family + "/" + kinds[k].DisplayName;
            _newCardBlockKindIndex = Mathf.Clamp(_newCardBlockKindIndex, 0, kinds.Count - 1);

            DrawEditorRow(out float rowWidth, out _);
            float half = (rowWidth - 4f) / 2f;
            _newCardBlockKindIndex = EditorGUILayout.Popup(_newCardBlockKindIndex, labels, GUILayout.Width(half), GUILayout.ExpandWidth(false));
            if (GUILayout.Button("+ Add block", GUILayout.Width(half), GUILayout.ExpandWidth(false)))
                AddCardBlock(poi, kinds[_newCardBlockKindIndex]);
            ReportTableCellRect("Card add block", 0);
            EditorRowEnd();
        }

        internal void AddCardBlock(POIData poi, BlockKindDefinition kind)
        {
            poi.card ??= new POICardData();
            poi.card.blocks ??= new List<BlockInstanceData>();
            var block = new BlockInstanceData
            {
                key = TaxonomyRowKeys.NextFree(poi.card.blocks, b => b.key, TaxonomyRowKeys.BlockPrefix),
                kind = kind.Key,
                variant = "",
                display = kind.DisplayModes[0],
            };
            poi.card.blocks.Add(block);
            _cardBlockFoldouts[poi.id + "/" + block.key] = true;
        }
    }
}
