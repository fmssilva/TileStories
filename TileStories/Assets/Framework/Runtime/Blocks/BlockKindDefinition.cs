using System.Collections.Generic;

namespace TileStories
{
    // One block kind (_3.1 section 3): its key, family, variants, fields and allowed display modes. The ONE
    // description of a kind -- the runtime view, the Editor field drawer and the validator all read it, so a
    // kind registered by an app is drawn and checked exactly like a built-in one.
    public sealed class BlockKindDefinition
    {
        // Stored in BlockInstanceData.kind ("header")
        public string Key;
        // The ontology family (_3.2): "about", "media", "guides", "stories", "ar", "play", "visit", "community", "meta"
        public string Family;
        // The Editor's name for the kind ("Header")
        public string DisplayName;
        // The Editor (i) text of the kind
        public string Help;
        public IReadOnlyList<string> Variants;
        // Used when an instance names no variant or one this kind does not have
        public string DefaultVariant;
        // Every field of the kind: the common ones every kind has (CommonFields: the heading), then the kind's own. A kind
        // sets only its own; one that declares a common key itself is refused by BlockRegistry.Validate (two fields, one key).
        public IReadOnlyList<BlockFieldDefinition> Fields
        {
            get => _fields ??= WithCommonFields(null);
            // - a plain set, not init: Unity's class library has no IsExternalInit, so C# 9 init accessors do not compile
            set => _fields = WithCommonFields(value);
        }
        private IReadOnlyList<BlockFieldDefinition> _fields;

        public IReadOnlyList<string> DisplayModes;
        // The CardStrings key of the heading shown when an instance's own heading is empty (null: no heading then), e.g.
        // compare_points' "Side by side"
        public string DefaultHeadingKey;
        // Whether the kind has something to show for this POI, this block and the wall's POIs (null = always). A block
        // it refuses is skipped (BlockStackBuilder, NotForThisPoint) -- e.g. a status block on a POI without a status
        // never draws an empty ring; a compare block whose other POI is gone or has no status never draws half a pair.
        // The wall's POIs may be null (a caller with no wall): then no other POI resolves.
        public System.Func<POIData, BlockInstanceData, IReadOnlyList<POIData>, bool> ShowsFor;
        // The Editor's words for that skip, under the block's row in Card Content ("this point has no status...")
        public string NotShownForPoiNote;
        // Variants drawn in the card's footer, pinned under the scrolling blocks (a sticky call to action)
        public IReadOnlyList<string> FooterVariants;

        // The key of the common heading field: a short title above the block, drawn by BlockStackView for every kind
        public const string HeadingField = "heading";

        // The fields every kind has, before its own (_3.1 step 6C). Drawn by the stack, not by each view: one look, one gap.
        public static readonly IReadOnlyList<BlockFieldDefinition> CommonFields = new[]
        {
            new BlockFieldDefinition
            {
                Key = HeadingField, Type = BlockFieldType.LocalizedText, Label = "Heading",
                Help = "A short title the card shows above this block (Before the earthquake, How it was made). Empty: no title -- or, " +
                       "for a kind that has one, its own default title (Detail Card > Card Texts).",
            },
        };

        private static IReadOnlyList<BlockFieldDefinition> WithCommonFields(IReadOnlyList<BlockFieldDefinition> own)
        {
            var all = new List<BlockFieldDefinition>(CommonFields);
            if (own != null) all.AddRange(own);
            return all;
        }

        public bool IsFooter(string variant) => FooterVariants != null && variant != null && ContainsString(FooterVariants, variant);

        public bool HasVariant(string variant) => Variants != null && variant != null && ContainsString(Variants, variant);

        // The field definition with this key, or null
        public BlockFieldDefinition Field(string key)
        {
            if (Fields == null) return null;
            for (int i = 0; i < Fields.Count; i++)
                if (Fields[i].Key == key) return Fields[i];
            return null;
        }

        private static bool ContainsString(IReadOnlyList<string> list, string value)
        {
            for (int i = 0; i < list.Count; i++)
                if (list[i] == value) return true;
            return false;
        }
    }
}
