// CardStarters.cs
//
// Specific Marker > Card Content > Card Starter (_3.1 "Card Starter", _3.2 step 17b): ready-made sets of BLOCKS a developer applies to a
// point in one click, plus Start from scratch. A starter is DATA -- a list of block kinds, each with its look and, where a kind needs one
// to show anything at all, a preset choice (Related's Source = Nearest) -- never content: it writes ordinary block instances with every
// text, picture and clip left empty for the author to fill, and the Editor names each empty required field under its row, exactly as for
// a block added by hand. The kinds that read the point itself (Wall Locator, Related by Nearest, Show On Wall) show at once, so a fresh
// starter card already has something to show besides its header. The Editor assembly owns it: the card never learns a starter exists.

using System.Collections.Generic;

namespace TileStories.Editor
{
    internal static class CardStarters
    {
        // One block of a starter: its kind, its look and the choices it presets (a choice is a setting, never content)
        internal sealed class Entry
        {
            public readonly string Kind;
            public readonly string Variant;
            // (field key, option) pairs: each a Choice field of the kind and one of its options (CardStartersTests checks both)
            public readonly IReadOnlyList<(string Field, string Option)> Choices;

            public Entry(string kind, string variant, params (string Field, string Option)[] choices)
            {
                Kind = kind;
                Variant = variant;
                Choices = choices;
            }
        }

        // One starter: the words the picker shows and the blocks it writes, top to bottom
        internal sealed class Starter
        {
            public readonly string Label;
            // What it suits, in a wall author's words
            public readonly string Suits;
            public readonly IReadOnlyList<Entry> Entries;

            public Starter(string label, string suits, params Entry[] entries)
            {
                Label = label;
                Suits = suits;
                Entries = entries;
            }

            // Start from scratch: the starter with no blocks
            public bool IsEmpty => Entries.Count == 0;
        }

        // The popup kind of the starter picker (NamedOptionPickerPopup): one open picker of this kind at a time
        public const string PickerKind = "card-starter-picker";
        public const string PickerTitle = "Card Starters";

        private static Entry Header => new(BuiltInBlocks.HeaderKind, BuiltInBlocks.HeaderImageParallax);
        private static Entry Story => new(BuiltInBlocks.RichTextKind, BuiltInBlocks.RichTextLede);
        private static Entry Pictures => new(BuiltInBlocks.GalleryKind, BuiltInBlocks.GalleryCarousel);
        private static Entry WhereOnTheWall => new(BuiltInBlocks.WallLocatorKind, BuiltInBlocks.WallLocatorStrip);
        private static Entry Nearby => new(BuiltInBlocks.RelatedKind, BuiltInBlocks.RelatedCarousel,
            (BuiltInBlocks.RelatedSourceField, RelatedPoisRule.SourceNearest));
        private static Entry Sources => new(BuiltInBlocks.SourcesKind, BuiltInBlocks.SourcesList);

        // The starters, the picker's order: the three sets, then Start from scratch (after the picker's line)
        public static readonly IReadOnlyList<Starter> All = new[]
        {
            // - judged on the real card at _3.2 step 17b: every set ends with where the point is on the wall and the points near it --
            //   the two blocks a visitor of ANY wall wants and an author never has to write, so a fresh starter card already leads somewhere
            new Starter("Simple point", "A point told in a few lines and pictures: the story, a gallery, where it is on the wall, the points nearby, the sources.",
                Header, Story, Pictures, WhereOnTheWall, Nearby, Sources),
            new Starter("Rich point", "A point worth a long visit: facts, the story, a timeline, pictures, a fun fact, an audio guide, where it is and what is near.",
                Header,
                new Entry(BuiltInBlocks.QuickFactsKind, BuiltInBlocks.QuickFactsGrid),
                Story,
                new Entry(BuiltInBlocks.TimelineKind, BuiltInBlocks.TimelineVertical),
                Pictures,
                new Entry(BuiltInBlocks.FunFactKind, BuiltInBlocks.FunFactFlip),
                new Entry(BuiltInBlocks.AudioGuideKind, BuiltInBlocks.AudioGuidePlayer),
                WhereOnTheWall, Nearby, Sources),
            new Starter("AR point", "A point with a 3D model: the story, the model to turn, a button to place it in the room and one to find it on the wall.",
                Header, Story,
                new Entry(BuiltInBlocks.Model3DKind, BuiltInBlocks.Model3DTurntable),
                new Entry(BuiltInBlocks.PlaceInArKind, BuiltInBlocks.PlaceInArButton),
                new Entry(BuiltInBlocks.ShowOnWallKind, BuiltInBlocks.ShowOnWallButton),
                Nearby, Sources),
            new Starter("Start from scratch", "No blocks: the card shows the point's name and summary until you add blocks with + Add block."),
        };

        // The picker draws its line above Start from scratch: the sets first, the empty card apart
        public static int SeparatorBefore => All.Count - 1;

        // The picker's rows, in All's order: each starter's name and what it suits
        public static IReadOnlyList<NamedOptionPickerPopup.Option> PickerOptions()
        {
            var options = new List<NamedOptionPickerPopup.Option>(All.Count);
            foreach (var starter in All) options.Add(new NamedOptionPickerPopup.Option(starter.Label, starter.Suits));
            return options;
        }

        // Replace `poi`'s blocks with the starter's: one new block per entry, in order, its look and preset choices set and nothing else.
        // A new block never takes the key of a block it replaces (a key is never reused: what the card saved for an old block -- an answer,
        // a Contents tick -- must not land on a new one). Start from scratch on a point with no blocks writes nothing at all (not even an
        // empty card). Called inside ONE mutation scope, so one Ctrl+Z gives the point back exactly as it was
        public static void Apply(POIData poi, Starter starter)
        {
            if (poi == null || starter == null) return;
            var replaced = poi.card?.blocks ?? new List<BlockInstanceData>();
            if (starter.IsEmpty && replaced.Count == 0) return;

            // - every key the point has held in this card, old and new: NextFree skips them all
            var taken = new List<BlockInstanceData>(replaced);
            var blocks = new List<BlockInstanceData>(starter.Entries.Count);
            foreach (var entry in starter.Entries)
            {
                BlockRegistry.Shared.TryGet(entry.Kind, out var definition);
                var block = new BlockInstanceData
                {
                    key = TaxonomyRowKeys.NextFree(taken, b => b.key, TaxonomyRowKeys.BlockPrefix),
                    kind = entry.Kind,
                    variant = entry.Variant,
                    // - the kind's first display, as "+ Add block" gives a new block
                    display = definition?.DisplayModes != null && definition.DisplayModes.Count > 0 ? definition.DisplayModes[0] : CardOptions.DisplayInline,
                };
                foreach (var (field, option) in entry.Choices)
                    block.fields.Add(new BlockFieldValue { key = field, value = option });
                blocks.Add(block);
                taken.Add(block);
            }
            poi.card ??= new POICardData();
            poi.card.blocks = blocks;
        }

        // The starter whose kinds and looks `poi`'s blocks are, in order (what the Card Starter row reads), or null (the point's own:
        // Custom). A point with no blocks reads Start from scratch
        public static Starter Matching(POIData poi)
        {
            var blocks = poi?.card?.blocks;
            foreach (var starter in All)
            {
                if ((blocks?.Count ?? 0) != starter.Entries.Count) continue;
                bool same = true;
                for (int i = 0; i < starter.Entries.Count && same; i++)
                    same = blocks[i] != null && blocks[i].kind == starter.Entries[i].Kind && blocks[i].variant == starter.Entries[i].Variant;
                if (same) return starter;
            }
            return null;
        }

        // The index of `starter` in All, or -1
        public static int IndexOf(Starter starter)
        {
            for (int i = 0; i < All.Count; i++)
                if (ReferenceEquals(All[i], starter)) return i;
            return -1;
        }
    }
}
