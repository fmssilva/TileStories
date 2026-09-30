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
        // The kind "+ Add block" adds (index into BlockRegistry.Shared.Ordered: the picker lists kinds by family)
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
            var built = BlockStackBuilder.Build(poi, settings, BlockRegistry.Shared, _config.pois);
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

                    // - the delete right after the row's cells, never at the far edge of a wide window
                    GUILayout.Space(TableGapBeforeDelete);
                    if (DeleteButton.DrawLayout("Delete block: " + kindName)) deleteIndex = i;
                    ReportTableCellRect("Card block delete", i);
                    GUILayout.FlexibleSpace();
                }
                _cardBlockFoldouts[foldoutKey] = open;

                foreach (var skipped in built.Skipped)
                    if (ReferenceEquals(skipped.Instance, block))
                        EditorGUILayout.HelpBox(CardBlockSkipText(skipped.Reason, definition?.Field(skipped.FieldKey), definition?.NotShownForPoiNote), MessageType.Warning);

                if (definition != null)
                {
                    var missing = MissingGlossaryTerms(block, definition, settings.glossary);
                    if (missing.Count > 0) EditorGUILayout.HelpBox(CardGlossaryMissingText(missing), MessageType.Warning);
                    foreach (string warning in CardBlockWarnings(block, definition, settings, poi))
                        EditorGUILayout.HelpBox(warning, MessageType.Warning);
                }

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
            var kinds = BlockRegistry.Shared.Ordered;
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

        // The [[terms]] of a block's long texts (every language, item rows included) that the wall's Glossary lacks
        internal static List<string> MissingGlossaryTerms(BlockInstanceData block, BlockKindDefinition definition, List<GlossaryEntry> glossary)
        {
            var texts = new List<string>();
            foreach (var field in definition.Fields)
            {
                var value = block.fields?.Find(f => f != null && f.key == field.Key);
                if (value == null) continue;
                if (field.Type == BlockFieldType.LocalizedLongText && value.text != null)
                    foreach (var t in value.text) if (t != null) texts.Add(t.value);
                if (field.Type != BlockFieldType.Items || value.items == null) continue;
                foreach (var sub in field.ItemFields)
                {
                    if (sub.Type != BlockFieldType.LocalizedLongText) continue;
                    foreach (var item in value.items)
                    {
                        var subValue = item?.fields?.Find(f => f != null && f.key == sub.Key);
                        if (subValue?.text != null) foreach (var t in subValue.text) if (t != null) texts.Add(t.value);
                    }
                }
            }
            var missing = new List<string>();
            foreach (string text in texts)
                foreach (string term in GlossaryMarkup.Terms(text))
                    if (CardGlossary.Find(glossary, term) == null && !missing.Exists(m => CardGlossary.SameTerm(m, term))) missing.Add(term);
            return missing;
        }

        // What a block that DOES show hides or gets wrong, in the Editor's words (the "Not shown" reasons are
        // BlockStackBuilder's): a sticky call to action with more than one button shows only the first; a compare block
        // pointed at its own point compares it with itself; a header picture look without its picture(s) shows text only
        internal static List<string> CardBlockWarnings(BlockInstanceData block, BlockKindDefinition definition, CardSettings settings, POIData poi)
        {
            var warnings = new List<string>();
            string variant = definition.HasVariant(block.variant) ? block.variant : BlockLibraryRule.DefaultVariant(settings, definition);
            if (definition.Key == BuiltInBlocks.ActionsKind && variant == BuiltInBlocks.ActionsStickyCta)
            {
                int rows = block.fields?.Find(f => f != null && f.key == BuiltInBlocks.ActionsItemsField)?.items?.Count ?? 0;
                if (rows > 1) warnings.Add(CardStickyExtraButtonsText(rows - 1));
            }
            if (definition.Key == BuiltInBlocks.ComparePointsKind && poi != null
                && new BlockFieldReader(block, null, null).Value(BuiltInBlocks.ComparePointsOtherField) == poi.id)
                warnings.Add(CardCompareWithItselfNote);
            // - a header picture look without its picture(s) -- the video loop without its clip, the turntable without its model --
            //   quietly falls back to text only: say so
            if (definition.Key == BuiltInBlocks.HeaderKind
                && (System.Array.IndexOf(BuiltInBlocks.HeaderImageVariants, variant) >= 0
                    || variant == BuiltInBlocks.HeaderVideoLoop || variant == BuiltInBlocks.HeaderModelTurntable)
                && !BuiltInBlocks.HeaderShowsPicture(variant, block))
                warnings.Add(CardHeaderNeedsPictureText(variant));
            // - a block that waits for the reading, in the middle of the card, is revealed above the visitor
            if (definition.ShowAfterViewedField != null && new BlockFieldReader(block, null, null).Flag(definition.ShowAfterViewedField))
            {
                var blocks = poi?.card?.blocks;
                int at = blocks?.IndexOf(block) ?? -1;
                if (at >= 0 && !ContentSeenRule.RevealsInView(FamiliesAfter(blocks, at)))
                    warnings.Add(CardShowAfterReadingMidCardNote);
            }
            // - a Show On Wall block on a card whose pinned (sticky) button already does the same
            if (definition.Key == BuiltInBlocks.ShowOnWallKind && HasStickyShowOnWall(poi, settings))
                warnings.Add(CardShowOnWallRepeatsStickyText);
            // - a poll with more options than it shows
            if (definition.Key == BuiltInBlocks.PollKind)
            {
                var read = new BlockFieldReader(block, null, null);
                int withWords = 0;
                foreach (var row in read.Items(BuiltInBlocks.PollOptionsField))
                    if (read.ItemText(row, BuiltInBlocks.PollOptionTextField).Length > 0) withWords++;
                if (withWords > PollRule.MaxOptions) warnings.Add(CardPollExtraOptionsText(withWords - PollRule.MaxOptions));
            }
            // - a dialogue's rows that will not show, and replies nobody can pick
            if (definition.Key == BuiltInBlocks.DialogueKind)
            {
                DialogueRule.Problems(new BlockFieldReader(block, null, null), out var noWords, out var orphans);
                if (noWords.Count > 0) warnings.Add(CardDialogueEmptyRowsText(noWords));
                if (orphans.Count > 0) warnings.Add(CardDialogueOrphanReplyText(orphans));
            }
            // - a question row the look would leave out (the block still shows its other rows): name the row and the reason
            if (definition.Key == BuiltInBlocks.KnowledgeCheckKind)
            {
                var read = new BlockFieldReader(block, null, null);
                var rows = read.Items(BuiltInBlocks.KnowledgeCheckQuestionsField);
                for (int row = 0; row < rows.Count; row++)
                {
                    var problem = KnowledgeCheckRule.Read(read, rows[row], row, variant, out _);
                    if (problem != KnowledgeCheckRule.Problem.None) warnings.Add(CardQuestionProblemText(row + 1, problem, variant));
                }
            }
            return warnings;
        }

        // Whether this card pins a Show On The Wall button in its footer: an enabled Actions block whose look is Sticky (its own, else the
        // Block Library's) and whose FIRST shown button -- the only one the sticky look draws -- is that action
        private static bool HasStickyShowOnWall(POIData poi, CardSettings settings)
        {
            var blocks = poi?.card?.blocks;
            if (blocks == null || !BlockLibraryRule.IsEnabled(settings, BuiltInBlocks.ActionsKind)) return false;
            foreach (var block in blocks)
            {
                if (block?.kind != BuiltInBlocks.ActionsKind) continue;
                string variant = BuiltInBlocks.Actions.HasVariant(block.variant) ? block.variant : BlockLibraryRule.DefaultVariant(settings, BuiltInBlocks.Actions);
                if (variant != BuiltInBlocks.ActionsStickyCta) continue;
                var read = new BlockFieldReader(block, null, null);
                foreach (var row in read.Items(BuiltInBlocks.ActionsItemsField))
                {
                    // - a row with no words, or with no known action, is not drawn: the first one that is drawn is the sticky button
                    if (read.ItemText(row, BuiltInBlocks.ActionsLabelField).Length == 0) continue;
                    string action = read.ItemValue(row, BuiltInBlocks.ActionsActionField);
                    if (System.Array.IndexOf(BuiltInBlocks.ActionOptions, action) < 0) continue;
                    if (action == BuiltInBlocks.ActionShowOnWall) return true;
                    break;
                }
            }
            return false;
        }

        // The family of every block written after `blocks[index]` (a kind that is not registered counts as content)
        private static IEnumerable<string> FamiliesAfter(List<BlockInstanceData> blocks, int index)
        {
            for (int i = index + 1; i < blocks.Count; i++)
                yield return BlockRegistry.Shared.TryGet(blocks[i]?.kind, out var later) ? later.Family : "";
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
