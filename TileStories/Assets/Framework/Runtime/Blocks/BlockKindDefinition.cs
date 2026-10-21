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
        // How a visitor takes the block in (_3.2 section 2.4): one of CardOptions.Formats (read, look, watch, listen, model,
        // interact). The second axis the card's navigation can group by (group_by = format); required, like Family
        public string Format;
        // What the kind needs from the visitor (_3.2 section 5.8): any of CardOptions.Needs (headphones, camera, network). Null / empty =
        // nothing needed, which is most kinds. Set here, never per block: a wall author does not have to say an audio guide needs headphones.
        // The card's menus filter on it (NavFacetRule)
        public IReadOnlyList<string> Needs;
        // The kind's own icon (_3.2 step 17a): a CardIcons key, what a sibling chip of the card's full view shows for a block of this kind, so
        // a visitor tells a 360 panorama from a gallery at a glance. Required, like Format: a kind with none would draw an empty chip
        public string IconKey;
        // The words a visitor reads for the kind (_3.2 step 6): a CardStrings key -- the framework's table for a built-in kind, the app's own
        // table for an app kind -- so a chip of the card's navigation names it in the visitor's language. Never the Editor's DisplayName
        public string LabelKey;
        // The Editor's name for the kind ("Header")
        public string DisplayName;
        // The Editor (i) text of the kind
        public string Help;
        public IReadOnlyList<string> Variants;
        // Used when an instance names no variant or one this kind does not have
        public string DefaultVariant;
        // Every field of the kind: the common ones it carries (CommonFieldsOf: the heading, and Reading Time unless the kind reads a
        // clip or is never a contents row), then the kind's own. A kind sets only its own; one that declares a common key itself is
        // refused by BlockRegistry.Validate (two fields, one key).
        public IReadOnlyList<BlockFieldDefinition> Fields
        {
            // - merged on the first read, not in the setter: an object initializer may set ClipLengthField / OffersReadingTime AFTER
            //   Fields, and the common list depends on them
            get => _fields ??= WithCommonFields(_ownFields);
            // - a plain set, not init: Unity's class library has no IsExternalInit, so C# 9 init accessors do not compile
            set { _ownFields = value; _fields = null; }
        }
        private IReadOnlyList<BlockFieldDefinition> _ownFields;
        private IReadOnlyList<BlockFieldDefinition> _fields;

        // The key of one of the kind's Asset fields holding an audio or video clip (null = none): the clip's own length is the block's
        // reading time, so the kind carries no Reading Time field (audio_guide, video). BlockRegistry.Validate checks it names such a field
        public string ClipLengthField;

        // Whether the kind carries the Reading Time field (on unless the kind says otherwise): off for a kind that is never a row of a
        // contents list -- the header (never a navigation target) and the navigation blocks themselves
        public bool OffersReadingTime = true;

        // Whether the kind's own view already offers its way into a full-screen view (a video player's full-screen button): its compact row,
        // open, then draws no full-view button of its own -- one control per job, never two identical ones a few lines apart (_3.2 step 13)
        public bool OffersOwnFullView;

        public IReadOnlyList<string> DisplayModes;
        // The CardStrings key of the heading shown when an instance's own heading is empty (null: no heading then), e.g.
        // compare_points' "Side by side"
        public string DefaultHeadingKey;
        // Whether the kind has something to show for this POI, this block and the wall's POIs (null = always). A block
        // it refuses is skipped (BlockStackBuilder, NotForThisPoint) -- e.g. a status block on a POI without a status
        // never draws an empty ring; a compare block whose other POI is gone or has no status never draws half a pair.
        // The wall's POIs may be null (a caller with no wall): then no other POI resolves. The string is the variant the block
        // WILL be drawn in (its own, else the Block Library's default): a kind whose rows are valid per variant reads it.
        public System.Func<POIData, BlockInstanceData, string, IReadOnlyList<POIData>, bool> ShowsFor;
        // The Editor's words for that skip, under the block's row in Card Content ("this point has no status...")
        public string NotShownForPoiNote;
        // Variants drawn in the card's footer, pinned under the scrolling blocks (a sticky call to action)
        public IReadOnlyList<string> FooterVariants;
        // Variants drawn in a slot pinned right under the card's header, above the scrolling blocks, and counted in the peek stop (an audio
        // guide's hero chip: it plays from the top of the card without opening it)
        public IReadOnlyList<string> PinnedTopVariants;

        // The key of one of the kind's Toggle fields (null = none): while it is on, the stack keeps the block hidden until the
        // visitor has scrolled past the card's content (ContentSeenRule), then shows it and remembers that (CardLocalState.Seen).
        // A question that should come after the reading (knowledge_check's show_after_viewed)
        public string ShowAfterViewedField;

        // The key of the common heading field: a short title above the block, drawn by BlockStackView for every kind
        public const string HeadingField = "heading";

        // The key of the common Reading Time field (_3.2 step 11b): the minutes a contents list (nav_contents) shows beside the block
        public const string MinutesField = "minutes";

        // The heading every kind has (_3.1 step 6C). Drawn by the stack, not by each view: one look, one gap.
        public static readonly BlockFieldDefinition HeadingDefinition = new()
        {
            Key = HeadingField, Type = BlockFieldType.LocalizedText, Label = "Heading",
            Help = "A short title the card shows above this block (Before the earthquake, How it was made). Empty: no title -- or, " +
                   "for a kind that has one, its own default title (Detail Card > Card Texts).",
        };

        // Reading Time, optional, per block (_3.2 step 11b): whole minutes, 0 = not given (the contents list then shows no time). Read by
        // ReadingTimeRule; a kind that reads a clip's length (ClipLengthField) or is never a contents row (OffersReadingTime off) has none
        public static readonly BlockFieldDefinition MinutesDefinition = new()
        {
            Key = MinutesField, Type = BlockFieldType.Number, Label = "Reading Time", NumberMin = 0f, NumberMax = 60f, NumberDefault = 0f,
            Help = "About how many minutes this block takes to read or to do, shown beside it in a contents list (the Contents block). " +
                   "0 (the default): no time is shown for it. Audio and video blocks need none: the list shows their clip's own length.",
        };

        // The common fields this kind carries, before its own: the heading always, Reading Time unless it reads a clip or is never a row
        public IReadOnlyList<BlockFieldDefinition> CommonFieldsOf() =>
            OffersReadingTime && ClipLengthField == null ? new[] { HeadingDefinition, MinutesDefinition } : new[] { HeadingDefinition };

        // Every common key any kind may carry: what BlockRegistry.Validate refuses as a kind's own field key
        public static readonly IReadOnlyList<string> CommonFieldKeys = new[] { HeadingField, MinutesField };

        private IReadOnlyList<BlockFieldDefinition> WithCommonFields(IReadOnlyList<BlockFieldDefinition> own)
        {
            var all = new List<BlockFieldDefinition>(CommonFieldsOf());
            if (own != null) all.AddRange(own);
            return all;
        }

        public bool IsFooter(string variant) => FooterVariants != null && variant != null && ContainsString(FooterVariants, variant);

        public bool IsPinnedTop(string variant) => PinnedTopVariants != null && variant != null && ContainsString(PinnedTopVariants, variant);

        public bool HasVariant(string variant) => Variants != null && variant != null && ContainsString(Variants, variant);

        // Whether the kind has a Required field: a block of it holds nothing to show until an author fills that field in (BlockStackBuilder skips
        // it while empty), so the card cannot put one in by itself -- what Detail Card > Navigation > Auto Nav Block never offers
        public bool NeedsAuthoredContent
        {
            get
            {
                if (Fields == null) return false;
                for (int i = 0; i < Fields.Count; i++)
                    if (Fields[i].Required) return true;
                return false;
            }
        }

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
