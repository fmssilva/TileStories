// POIEditorToolWindow.BlockFieldDrawer.References.cs
//
// Partial: the reference popups of the generic block-field drawer (BlockFieldDrawer.cs dispatches to them). Two
// different things that look alike (20-code-quality, "two UI elements that look identical"): a PoiRef picks another
// POI of the WALL by its list title; a BlockRef (_3.2 step 4) picks another block of the SAME CARD by its kind and
// first words. They share the mechanism -- ReferencePopupOptions, which keeps a stale value selected instead of
// rewriting it -- and keep their own call sites, option builders and missing labels.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    public partial class POIEditorToolWindow
    {
        private const string BlockPoiMissingLabel = "(missing)";
        // How a BlockRef's popup shows a key no block of the card has any more (never the key itself)
        internal const string BlockRefMissingLabel = "(a deleted block)";
        // How many characters of a block's first words its BlockRef entry quotes before "..."
        private const int BlockRefSummaryMaxChars = 40;

        // The card a BlockRef field is drawn for: its POI, the block that holds the field, the card as built, the wall's languages
        private sealed class BlockRefCard
        {
            public POIData Poi;
            public BlockInstanceData Holder;
            public BlockStackBuilder.Result Built;
            public List<string> Languages;
        }

        // A PoiRef field: a popup of this wall's POIs named as the POI list names them ("3. North tower", never an id). A
        // stored id no POI has any more stays selected as "(missing)" until the developer picks another -- drawing never
        // rewrites it (the ReferencePopupOptions rule every reference popup of the window follows).
        private static void DrawPoiRefRow(BlockFieldDefinition field, float indent, string probeName, int probeIndex, List<POIData> pois,
            Func<string> get, Action<string> set)
        {
            var options = PoiRefOptions(pois, get(), !field.Required);
            DrawEditorRow(out float rowWidth, out _, indent);
            EditorGUILayout.PrefixLabel(field.Label + (field.Required ? " (required)" : ""));
            int picked = EditorGUILayout.Popup(options.SelectedIndex, options.Labels, GUILayout.Width(Mathf.Max(40f, rowWidth - EditorGUIUtility.labelWidth - 36f)), GUILayout.ExpandWidth(false));
            ReportTableCellRect(probeName, probeIndex);
            HelpInfoButton.Draw(field.Label, field.Help);
            EditorRowEnd();
            if (picked != options.SelectedIndex) set(options.KeyAt(picked) ?? "");
        }

        // The options of a PoiRef popup: every POI by its list title, a blank value as "(none)", a stale id as "(missing)"
        internal static ReferencePopupOptions PoiRefOptions(List<POIData> pois, string current, bool allowNone)
        {
            var ids = new List<string>();
            var titles = new List<string>();
            for (int i = 0; i < (pois?.Count ?? 0); i++)
            {
                ids.Add(pois[i]?.id);
                titles.Add(EditorNames.Poi(i, pois[i]));
            }
            return ReferencePopupOptions.Build(ids, titles, current, allowNone, BlockPoiMissingLabel);
        }

        // A BlockRef field: a popup of the OTHER blocks of this POI's card, each named the way its Card Content row reads ("5. Rich
        // Text: The story of the keep...", never a key). A key no block has any more stays selected as "(a deleted block)" until the
        // developer picks another -- drawing never rewrites it. Under the row, when the pick leads nowhere, a warning names the block and
        // why the card leaves the jump out (BlockRefRule.Check, read as a visitor who has not read the card yet).
        private static void DrawBlockRefRow(BlockFieldDefinition field, float indent, string probeName, int probeIndex, BlockRefCard card,
            Func<string> get, Action<string> set)
        {
            var options = BlockRefOptions(card.Poi, card.Holder, get(), card.Languages);
            DrawEditorRow(out float rowWidth, out _, indent);
            EditorGUILayout.PrefixLabel(field.Label + (field.Required ? " (required)" : ""));
            int picked = EditorGUILayout.Popup(options.SelectedIndex, options.Labels, GUILayout.Width(Mathf.Max(40f, rowWidth - EditorGUIUtility.labelWidth - 36f)), GUILayout.ExpandWidth(false));
            ReportTableCellRect(probeName, probeIndex);
            HelpInfoButton.Draw(field.Label, field.Help);
            EditorRowEnd();
            if (picked != options.SelectedIndex) set(options.KeyAt(picked) ?? "");

            string stored = get();
            var problem = BlockRefRule.Check(stored, card.Holder, card.Poi, card.Built, _ => false);
            if (problem == BlockRefRule.Problem.None) return;
            // - an optional pick (a hotspot spot's Opens Block): nothing picked is the normal spot, not a dead jump to warn about
            if (problem == BlockRefRule.Problem.Blank && field.BlankIsFine) return;
            EditorGUILayout.HelpBox(CardBlockRefProblemText(field.Label, problem, CardBlockName(card.Poi, stored, card.Languages)), MessageType.Warning);
            ReportTableCellRect(probeName + " warning", probeIndex);
        }

        // The options of a BlockRef popup: "(none)", then every OTHER block of `poi`'s card in Card Content order, named like its row;
        // a stale key as "(a deleted block)"
        internal static ReferencePopupOptions BlockRefOptions(POIData poi, BlockInstanceData holder, string current, List<string> languages)
        {
            var keys = new List<string>();
            var names = new List<string>();
            var blocks = poi?.card?.blocks;
            for (int i = 0; i < (blocks?.Count ?? 0); i++)
            {
                var block = blocks[i];
                // - a block never opens itself: it is not offered
                if (block == null || ReferenceEquals(block, holder) || string.IsNullOrWhiteSpace(block.key)) continue;
                keys.Add(block.key);
                names.Add(CardBlockName(i, block, languages));
            }
            return ReferencePopupOptions.Build(keys, names, current, allowNone: true, BlockRefMissingLabel);
        }

        // A block of `poi`'s card as the developer finds it in Card Content, by its key ("" for a key no block has)
        internal static string CardBlockName(POIData poi, string key, List<string> languages)
        {
            var blocks = poi?.card?.blocks;
            int at = blocks?.FindIndex(b => b != null && b.key == key) ?? -1;
            return at >= 0 ? CardBlockName(at, blocks[at], languages) : "";
        }

        // "<row number>. <kind>: <first words>" -- the row number, the kind and the one-line summary its Card Content row shows
        // (CardBlockSummary), the words cut to BlockRefSummaryMaxChars; just "<row number>. <kind>" for a block with no words
        private static string CardBlockName(int index, BlockInstanceData block, List<string> languages)
        {
            bool known = BlockRegistry.Shared.TryGet(block.kind, out var definition);
            string name = (index + 1) + ". " + (known ? definition.DisplayName : block.kind + " (unknown)");
            string summary = known ? CardBlockSummary(block, definition, languages) : "";
            if (summary.Length > BlockRefSummaryMaxChars) summary = summary.Substring(0, BlockRefSummaryMaxChars).TrimEnd() + CardBlockSummaryEllipsis;
            return summary.Length > 0 ? name + ": " + summary : name;
        }
    }
}
