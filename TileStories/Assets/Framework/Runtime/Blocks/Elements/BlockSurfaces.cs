using System.Collections.Generic;
using UnityEngine;

namespace TileStories
{
    // A composed block's two surfaces as data (_3.3 R17.1): the PAGE -- its kind's own Elements field, the content written once -- and the
    // PREVIEW -- Automatic (derived from the Page, ElementPreviewRule) or Custom (its own row list: own rows and references) --, plus its
    // shared-component overrides (R17.5, the Open Row first) and its Reading Time (the common `minutes` field: 0 = Auto, ElementRules).
    // The surface fields are COMMON fields: BlockKindDefinition appends them AFTER the own fields of every kind that declares an Elements
    // field (Page first, then Preview, R17.7), so validation, the Editor's drawer and the round trip see them like any other field.
    public static class BlockSurfaces
    {
        public const string PreviewModeField = "preview_mode";
        public const string PreviewRowsField = "preview_rows";
        public const string ComponentOverridesField = "component_overrides";

        // A component override row's sub-fields: which component, whether the block overrides it (off = the wall's, values kept -- the
        // _2.0 label-style pattern), and the Open Row's own values
        public const string ComponentKey = "component";
        public const string OverrideKey = "override";
        public const string WordsKey = "words";
        public const string EmphasisKey = "emphasis";

        // The highest row number this block ever handed out (TakeRowId): not a field -- nothing to edit -- the id rule's memory
        public const string LastRowIdValue = "element_last_id";
        public const string RowIdPrefix = "el_";

        public static readonly BlockFieldDefinition PreviewModeDefinition = new()
        {
            Key = PreviewModeField, Type = BlockFieldType.Choice, Label = "Preview",
            Options = ElementOptions.PreviewModes, OptionLabels = ElementOptions.PreviewModeLabels, ChoiceDefault = ElementOptions.PreviewAutomatic,
            Help = "What the card shows of this block before it is opened. Automatic (the default): the Page's first picture, its title and " +
                   "its first text, following every Page edit. Custom: the Preview Rows below, each a row of its own or a link to a Page row.",
        };

        public static readonly BlockFieldDefinition PreviewRowsDefinition = new()
        {
            Key = PreviewRowsField, Type = BlockFieldType.Elements, Label = "Preview Rows",
            ShownWhen = FieldShownWhen.Choice(PreviewModeField, ElementOptions.PreviewCustom),
            Help = "The Custom preview, in order: a linked row shows a Page row's content with its own style (edit the content once, on the " +
                   "Page); an own row holds content only the preview shows. A linked row whose Page row is gone is shown as a warning.",
        };

        public static readonly BlockFieldDefinition ComponentOverridesDefinition = new()
        {
            Key = ComponentOverridesField, Type = BlockFieldType.Items, Label = "Component Overrides",
            Help = "The shared card components this block draws differently from the wall's (Detail Card > Card Components). Override off: " +
                   "the block follows the wall's, and keeps its own values for later.",
            ItemFields = new[]
            {
                new BlockFieldDefinition
                {
                    Key = ComponentKey, Type = BlockFieldType.Choice, Label = "Component",
                    Options = ElementOptions.Components, OptionLabels = ElementOptions.ComponentLabels, ChoiceDefault = ElementOptions.ComponentOpenRow,
                    Help = "Which shared component this row changes for the block.",
                },
                new BlockFieldDefinition
                {
                    Key = OverrideKey, Type = BlockFieldType.Toggle, Label = "Override",
                    Help = "On: the block uses the values below. Off: the wall's, the values below kept.",
                },
                new BlockFieldDefinition
                {
                    Key = WordsKey, Type = BlockFieldType.LocalizedText, Label = "Words",
                    ShownWhen = FieldShownWhen.Choice(ComponentKey, ElementOptions.ComponentOpenRow),
                    Help = "The Open Row's words (Read the story). Empty: the wall's words for this kind.",
                },
                new BlockFieldDefinition
                {
                    Key = EmphasisKey, Type = BlockFieldType.Choice, Label = "Emphasis",
                    Options = ElementOptions.OpenRowEmphases, OptionLabels = ElementOptions.OpenRowEmphasisLabels, ChoiceDefault = ElementOptions.EmphasisPlain,
                    ShownWhen = FieldShownWhen.Choice(ComponentKey, ElementOptions.ComponentOpenRow),
                    Help = "Plain, Nudge (the chevron moves) or Shine (a light crosses the words). Reduce Motion always shows Plain.",
                },
            },
        };

        // The surface fields, in the order a composing kind carries them after its own
        public static readonly IReadOnlyList<BlockFieldDefinition> Definitions = new[] { PreviewModeDefinition, PreviewRowsDefinition, ComponentOverridesDefinition };
        public static readonly IReadOnlyList<string> FieldKeys = new[] { PreviewModeField, PreviewRowsField, ComponentOverridesField };

        // The kind's Page: its one OWN Elements field (BlockRegistry refuses a second), or null for a kind that composes nothing
        public static BlockFieldDefinition PageFieldOf(BlockKindDefinition kind)
        {
            if (kind?.OwnFields == null) return null;
            foreach (var field in kind.OwnFields)
                if (field != null && field.Type == BlockFieldType.Elements) return field;
            return null;
        }

        public static bool Composes(BlockKindDefinition kind) => PageFieldOf(kind) != null;

        // The Page's rows, in order (none for a kind that composes nothing)
        public static IReadOnlyList<BlockItemData> PageRows(BlockInstanceData block, BlockKindDefinition kind)
        {
            var page = PageFieldOf(kind);
            return page == null ? System.Array.Empty<BlockItemData>() : RowsOf(block, page.Key);
        }

        // The Custom preview's rows, in order (stored even while the mode is Automatic: switching back keeps them)
        public static IReadOnlyList<BlockItemData> PreviewRows(BlockInstanceData block) => RowsOf(block, PreviewRowsField);

        // automatic | custom; a block that stores none (or an unknown word) is Automatic
        public static string PreviewMode(BlockInstanceData block) =>
            ElementOptions.OneOf(new BlockFieldReader(block, null, null).Value(PreviewModeField), ElementOptions.PreviewModes);

        // The minutes set by hand in Reading Time, 0 = Auto (ElementRules computes them from the Page)
        public static int MinutesSetByHand(BlockInstanceData block) =>
            Mathf.Max(0, Mathf.RoundToInt(new BlockFieldReader(block, null, null).Number(BlockKindDefinition.MinutesDefinition)));

        // The block's override of one shared component (the first row naming it); IsOn false when it has none or its switch is off
        public static ComponentOverride OverrideOf(BlockInstanceData block, string component)
        {
            var read = new BlockFieldReader(block, null, null);
            foreach (var row in read.Items(ComponentOverridesField))
            {
                string named = ElementOptions.OneOf(read.ItemValue(row, ComponentKey), ElementOptions.Components);
                if (named == component) return new ComponentOverride(row);
            }
            return default;
        }

        // How many of the block's rows (Page + Preview) the developer has not marked Validated yet: the block header's "N to validate" (R14.8)
        public static int RowsToValidate(BlockInstanceData block, BlockKindDefinition kind)
        {
            int count = 0;
            foreach (var row in PageRows(block, kind)) if (!new ElementRow(row).Validated) count++;
            foreach (var row in PreviewRows(block)) if (!new ElementRow(row).Validated) count++;
            return count;
        }

        // The id of a NEW row of `block` (the block-key rule: el_N, never reused within the block): one past the highest number the block
        // ever handed out (remembered in LastRowIdValue) or any row of it holds now (a hand-edited file), whichever is higher -- so a row id
        // a reference may still name is never given to another row, even after that row was deleted. Records the number it hands out.
        public static string TakeRowId(BlockInstanceData block)
        {
            int highest = Mathf.RoundToInt(StoredValue(block, LastRowIdValue)?.number ?? 0f);
            if (block?.fields != null)
                foreach (var field in block.fields)
                {
                    if (field?.items == null) continue;
                    foreach (var row in field.items)
                    {
                        int n = RowNumber(new ElementRow(row).Id);
                        if (n > highest) highest = n;
                    }
                }
            int next = highest + 1;
            var memory = StoredValue(block, LastRowIdValue);
            if (memory == null)
            {
                memory = new BlockFieldValue { key = LastRowIdValue };
                block.fields ??= new List<BlockFieldValue>();
                block.fields.Add(memory);
            }
            memory.number = next;
            return RowIdPrefix + next;
        }

        // The N of an id el_N, 0 for anything else
        public static int RowNumber(string id)
        {
            if (string.IsNullOrEmpty(id) || !id.StartsWith(RowIdPrefix, System.StringComparison.Ordinal)) return 0;
            return int.TryParse(id.Substring(RowIdPrefix.Length), System.Globalization.NumberStyles.None,
                System.Globalization.CultureInfo.InvariantCulture, out int n) ? n : 0;
        }

        private static IReadOnlyList<BlockItemData> RowsOf(BlockInstanceData block, string key) =>
            (IReadOnlyList<BlockItemData>)StoredValue(block, key)?.items ?? System.Array.Empty<BlockItemData>();

        private static BlockFieldValue StoredValue(BlockInstanceData block, string key)
        {
            if (block?.fields == null) return null;
            foreach (var f in block.fields)
                if (f != null && f.key == key) return f;
            return null;
        }
    }

    // One block's override of a shared component (R17.5), read from its Component Overrides row; default = no row (IsOn false)
    public readonly struct ComponentOverride
    {
        public readonly BlockItemData Row;

        public ComponentOverride(BlockItemData row) { Row = row; }

        // Whether the block draws this component its own way (off, or no row: the wall's)
        public bool IsOn => Row != null && new BlockFieldReader(null, null, null).ItemFlag(Row, BlockSurfaces.OverrideKey);
        // The Open Row's words in `language` ("" = the wall's words)
        public string Words(string language, string fallback) => Row == null ? "" : new BlockFieldReader(null, language, fallback).ItemText(Row, BlockSurfaces.WordsKey);
        // The Open Row's emphasis, plain when unset or unknown
        public string Emphasis => ElementOptions.OneOf(Row == null ? null : new BlockFieldReader(null, null, null).ItemValue(Row, BlockSurfaces.EmphasisKey),
            ElementOptions.OpenRowEmphases);
    }
}
