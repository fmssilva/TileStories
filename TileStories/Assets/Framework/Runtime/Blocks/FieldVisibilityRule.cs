using System.Collections.Generic;

namespace TileStories
{
    // Whether the Editor draws a field (_3.1 step 15.1), pure: a field's FieldShownWhen judged against what the card will really use --
    // the look the block is drawn in (its own, else the Block Library's default) and each Choice's effective option (the block's own,
    // else the Library's default, else the field's own default). The ONE place the generic BlockFieldDrawer and the Block Library ask.
    public static class FieldVisibilityRule
    {
        // The look a block is drawn in: its own when its kind has it, else the Block Library's default (BlockStackBuilder's rule)
        public static string ResolvedVariant(BlockInstanceData block, BlockKindDefinition kind, CardSettings settings) =>
            kind.HasVariant(block?.variant) ? block.variant : BlockLibraryRule.DefaultVariant(settings, kind);

        // A field of the block itself
        public static bool IsShown(BlockFieldDefinition field, BlockInstanceData block, BlockKindDefinition kind, CardSettings settings)
        {
            var when = field?.ShownWhen;
            if (when == null) return true;
            var read = new BlockFieldReader(block, null, null);
            return when.Holds(ResolvedVariant(block, kind, settings),
                key =>
                {
                    var other = kind.Field(key);
                    return other != null ? BlockLibraryRule.Choice(settings, kind, other, read) : read.Value(key);
                },
                key => read.Text(key).Length > 0);
        }

        // A sub-field of one Items row: the look is the block's, the other values are the same row's
        public static bool IsShown(BlockFieldDefinition sub, BlockItemData row, BlockFieldDefinition itemsField, BlockInstanceData block,
            BlockKindDefinition kind, CardSettings settings)
        {
            var when = sub?.ShownWhen;
            if (when == null) return true;
            var read = new BlockFieldReader(block, null, null);
            return when.Holds(ResolvedVariant(block, kind, settings),
                key => ItemChoice(read, row, SubField(itemsField, key), key),
                key => read.ItemText(row, key).Length > 0);
        }

        // A Block Library default row (a LibraryDefault field): drawn while the default could reach a block -- the condition holds for a
        // block that names nothing (the Library's default look and options), or for any block of the kind on the wall's points. A
        // default a real block uses is never hidden.
        public static bool IsShownInLibrary(BlockFieldDefinition field, BlockKindDefinition kind, CardSettings settings, IReadOnlyList<POIData> pois)
        {
            if (field?.ShownWhen == null) return true;
            if (IsShown(field, new BlockInstanceData { kind = kind.Key, variant = "" }, kind, settings)) return true;
            if (pois == null) return false;
            foreach (var poi in pois)
            {
                var blocks = poi?.card?.blocks;
                if (blocks == null) continue;
                foreach (var block in blocks)
                    if (block != null && block.kind == kind.Key && IsShown(field, block, kind, settings)) return true;
            }
            return false;
        }

        // A row's Choice option: its own when it is one of the options, else the sub-field's default ("" without one)
        private static string ItemChoice(BlockFieldReader read, BlockItemData row, BlockFieldDefinition sub, string key)
        {
            string own = read.ItemValue(row, key);
            if (sub?.Options == null) return own;
            for (int i = 0; i < sub.Options.Count; i++)
                if (sub.Options[i] == own) return own;
            return sub.ChoiceDefault ?? "";
        }

        private static BlockFieldDefinition SubField(BlockFieldDefinition itemsField, string key)
        {
            var subs = itemsField?.ItemFields;
            if (subs == null) return null;
            for (int i = 0; i < subs.Count; i++)
                if (subs[i].Key == key) return subs[i];
            return null;
        }
    }
}
