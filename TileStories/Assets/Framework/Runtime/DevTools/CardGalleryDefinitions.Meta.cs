using System.Collections.Generic;
using UnityEngine;

namespace TileStories
{
    public static partial class CardGalleryDefinitions
    {
        // ---------------- sources ----------------

        private static void AddSources(List<Entry> list)
        {
            var shortRows = new[] { ("Chronicle of the conquest", "Anonymous", "Public domain") };
            var longRows = new[]
            {
                ("Letter on the conquest of the city, written by a priest of the English fleet in the following winter", "Osbern", "Public domain"),
                ("Panel of the city before the earthquake, tile inventory sheet 14", "National Tile Museum", "With permission"),
                ("Photograph of the keep from the river", "Municipal photographic archive", "CC BY 4.0"),
            };
            // - a row with no author and no licence: the title alone
            var partialRows = new[] { ("Oral account of the gardeners", "", "") };
            foreach (var variant in BuiltInBlocks.Sources.Variants)
            {
                list.Add(new Entry(BuiltInBlocks.SourcesKind, variant, "short", SourcesBlock(variant, shortRows, BuiltInBlocks.ContentVerified)));
                list.Add(new Entry(BuiltInBlocks.SourcesKind, variant, "long", SourcesBlock(variant, longRows, BuiltInBlocks.ContentDraft)));
                list.Add(new Entry(BuiltInBlocks.SourcesKind, variant, "partial", SourcesBlock(variant, partialRows, null)));
            }
        }

        private static BlockInstanceData SourcesBlock(string variant, (string Title, string Author, string Licence)[] rows, string status)
        {
            var block = new BlockInstanceData { key = "block_2", kind = BuiltInBlocks.SourcesKind, variant = variant };
            var field = new BlockFieldValue { key = BuiltInBlocks.SourcesItemsField };
            foreach (var (title, author, licence) in rows)
                field.items.Add(Item(ItemText(BuiltInBlocks.SourcesTitleField, title), ItemText(BuiltInBlocks.SourcesAuthorField, author),
                    ItemText(BuiltInBlocks.SourcesLicenceField, licence)));
            block.fields.Add(field);
            if (status != null) block.fields.Add(new BlockFieldValue { key = BuiltInBlocks.SourcesStatusField, value = status });
            return block;
        }

    }
}
