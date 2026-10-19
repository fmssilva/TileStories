using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace TileStories.Tests
{
    // Phase A of _3.2 step 12: EVERY kind the framework ships, compact and opened (CardGalleryChecks.AssertCompactAndExpanded), walked from
    // the catalogue itself -- BuiltInBlocks registered into a fresh registry -- never from a hand-made list, so the day a kind is added it is
    // walked too. Its block is the kind's first gallery entry drawn in the scroll (a pinned or footer look is drawn whole, never as a row);
    // a kind the gallery has no entry for (the navigation kinds: CardNavGallery's) shows with only a heading, and a kind that cannot show that
    // way fails here, naming itself, until the gallery gets an entry for it. The app's own kind is walked by its own gallery fixture.
    public partial class CardGalleryTests
    {
        private static string[] FrameworkKinds()
        {
            var catalogue = new BlockRegistry();
            BuiltInBlocks.Register(catalogue);
            return catalogue.All.Where(k => k.Key != BuiltInBlocks.HeaderKind).Select(k => k.Key).ToArray();
        }

        // The kind's first gallery entry whose look sits in the scroll, else a block holding only a heading
        private static CardGalleryDefinitions.Entry EntryForKind(string kind)
        {
            BlockRegistry.Shared.TryGet(kind, out var definition);
            foreach (var e in CardGalleryDefinitions.All)
                if (!e.IsHeader && e.Kind == kind && !definition.IsPinnedTop(e.Variant) && !definition.IsFooter(e.Variant)) return e;
            var block = new BlockInstanceData { key = "block_2", kind = kind };
            block.fields.Add(new BlockFieldValue
            {
                key = BlockKindDefinition.HeadingField,
                text = new System.Collections.Generic.List<LocalizedEntry> { new() { lang = "en", value = CardGalleryDefinitions.HeadingFor(kind) } },
            });
            return new CardGalleryDefinitions.Entry(kind, definition.DefaultVariant, "heading_only", block);
        }

        [UnityTest]
        public IEnumerator EveryKind_IsACompactRow_ThatARealTapOpensAndCloses([ValueSource(nameof(FrameworkKinds))] string kind)
        {
            Assert.IsTrue(BlockRegistry.Shared.TryGet(kind, out var definition), "precondition: " + kind + " is registered on the running card");
            CollectionAssert.Contains(definition.DisplayModes, CardOptions.DisplayExpandable, kind + " offers Expandable");
            yield return CardGalleryChecks.AssertCompactAndExpanded(_harness, EntryForKind(kind), "Card_Expandable_" + kind);
        }
    }
}
