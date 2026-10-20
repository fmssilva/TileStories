using System.Collections.Generic;

namespace TileStories
{
    public static partial class CardGalleryDefinitions
    {
        // ---------------- nav_bento (_3.2 step 15a) ----------------

        // The bento's tiles lead to OTHER blocks of the same card, so its one entry brings two companion blocks (Setup): short Rich Text blocks
        // drawn as compact rows (Expandable) -- a closed row binds no view, which is also what lets the generic "every kind is a compact row"
        // walk (CardGalleryTests.Expandable) show the bento alone on its card. The navigation gallery (CardNavGalleryDefinitions) judges the
        // bento over the five fixture cases; this entry is the card gallery's own: the kind has a Required field, so a heading-only block shows nothing
        private static void AddNavBento(List<Entry> list)
        {
            var block = new BlockInstanceData { key = "block_2", kind = BuiltInBlocks.NavBentoKind, variant = BuiltInBlocks.NavBentoTiles };
            var tiles = new BlockFieldValue { key = BuiltInBlocks.NavBentoTilesField };
            tiles.items.Add(Item(
                new BlockItemFieldValue { key = BuiltInBlocks.NavBentoTargetField, value = "block_3" },
                ItemText(BuiltInBlocks.NavBentoWordsField, "The story")));
            tiles.items.Add(Item(
                new BlockItemFieldValue { key = BuiltInBlocks.NavBentoTargetField, value = "block_4" },
                new BlockItemFieldValue { key = BuiltInBlocks.NavBentoOpensField, value = BuiltInBlocks.NavBentoOpensFullView }));
            block.fields.Add(tiles);
            list.Add(new Entry(BuiltInBlocks.NavBentoKind, BuiltInBlocks.NavBentoTiles, "short", block, setup: AddBentoCompanions));
        }

        // The two blocks the bento's tiles lead to: Rich Text, a heading and a line, shown as closed compact rows
        private static void AddBentoCompanions(POIData poi)
        {
            foreach (var (key, heading) in new[] { ("block_3", "The keep"), ("block_4", "The cistern") })
            {
                var companion = RichText(BuiltInBlocks.RichTextPlain, ShortText, withSections: false);
                companion.key = key;
                companion.display = CardOptions.DisplayExpandable;
                companion.fields.Add(Text(BlockKindDefinition.HeadingField, heading));
                poi.card.blocks.Add(companion);
            }
        }
    }
}
