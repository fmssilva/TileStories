using System;
using System.Collections.Generic;

namespace TileStories
{
    // What the facets know about one block (_3.2 section 5.8): the format and the needs of its KIND and its minutes (ReadingTimeRule: the
    // Reading Time field or a clip's own length, 0 = none given). Plain data: the rule judges it, a view fills it
    public readonly struct BlockFacts
    {
        public readonly string Format;
        public readonly int Minutes;
        public readonly IReadOnlyList<string> Needs;

        public BlockFacts(string format, int minutes, IReadOnlyList<string> needs)
        {
            Format = format;
            Minutes = minutes;
            Needs = needs ?? Array.Empty<string>();
        }

        // Whether the block's kind needs this (a CardOptions.Needs key)
        public bool NeedsA(string need)
        {
            // - a default(BlockFacts) holds no list at all
            if (Needs == null) return false;
            for (int i = 0; i < Needs.Count; i++)
                if (Needs[i] == need) return true;
            return false;
        }
    }

    // The facets a visitor has switched on in a card's menu (drawer_tree, grid_overlay): ONE format, ONE time limit and any number of
    // "without" needs. Facets combine with AND (format AND time AND every without). Plain state, no view: a menu owns one and a pure rule
    // (NavFacetRule) judges blocks against it, so both menus filter the same way.
    public sealed class NavFacets
    {
        // The format chosen (null = any) and the longest time chosen in minutes (0 = any)
        public string Format { get; private set; }
        public int MaxMinutes { get; private set; }
        // The needs the visitor cannot meet: a block of a kind needing one of them is left out
        public IReadOnlyList<string> Without => _without;
        public bool IsActive => Format != null || MaxMinutes > 0 || _without.Count > 0;

        private readonly List<string> _without = new();

        // A chip tap: on when off, off when on (a second tap clears that facet)
        public void ToggleFormat(string format) => Format = Format == format ? null : format;
        public void ToggleMinutes(int minutes) => MaxMinutes = MaxMinutes == minutes ? 0 : minutes;

        public void ToggleWithout(string need)
        {
            if (!_without.Remove(need)) _without.Add(need);
        }

        // Every facet off: the full list again
        public void Clear()
        {
            Format = null;
            MaxMinutes = 0;
            _without.Clear();
        }

        public NavFacets Copy()
        {
            var copy = new NavFacets { Format = Format, MaxMinutes = MaxMinutes };
            copy._without.AddRange(_without);
            return copy;
        }

        // The same facets with one dimension cleared (what the OTHER chips are judged against)
        internal NavFacets WithoutDimension(NavFacetRule.Dimension dimension)
        {
            var copy = Copy();
            if (dimension == NavFacetRule.Dimension.Format) copy.Format = null;
            else if (dimension == NavFacetRule.Dimension.Time) copy.MaxMinutes = 0;
            else copy._without.Clear();
            return copy;
        }
    }

    // The ONE rule that answers "does this block pass these facets" (_3.2 section 5.8, step 18), read by drawer_tree AND grid_overlay -- never
    // one filter per element. Pure: facts in, a yes or no out.
    //   - Format: the block's kind has that format
    //   - Time (<= 2 min, <= 5 min): the block has a time and it is within the limit. A block with NO time given does not match: nothing says
    //     it fits, so "I have five minutes" never lists a block nobody timed
    //   - Without a need: the block's kind does not need it
    // A chip is OFFERED only when tapping it would change the list without emptying it, judged against the blocks the OTHER facets let
    // through -- a chip that filters to nothing, or to everything, is noise. A facet already on always stays, so it can be switched off.
    public static class NavFacetRule
    {
        // The two reading-time limits a visitor can choose, in minutes (the shorter first)
        public const int ShortMinutes = 2;
        public const int MediumMinutes = 5;
        public static readonly int[] TimeLimits = { ShortMinutes, MediumMinutes };

        public enum Dimension { Time, Without, Format }

        // One chip: its dimension, its value (a format, a need, or a limit in Minutes) and whether it is on
        public readonly struct Chip
        {
            public readonly Dimension Dimension;
            public readonly string Value;
            public readonly int Minutes;
            public readonly bool Active;

            public Chip(Dimension dimension, string value, int minutes, bool active)
            {
                Dimension = dimension;
                Value = value;
                Minutes = minutes;
                Active = active;
            }

            // Switch this chip's facet on or off
            public void Toggle(NavFacets facets)
            {
                if (Dimension == Dimension.Format) facets.ToggleFormat(Value);
                else if (Dimension == Dimension.Time) facets.ToggleMinutes(Minutes);
                else facets.ToggleWithout(Value);
            }
        }

        // The facts of a block of this kind with these minutes
        public static BlockFacts FactsOf(BlockKindDefinition kind, int minutes) => new(kind?.Format, minutes, kind?.Needs);

        // The CardStrings key of a chip's words: "Up to {0} min", the "No ..." of a need, or the format's own words; null for an unknown value
        public static string LabelKeyOf(Chip chip)
        {
            if (chip.Dimension == Dimension.Time) return CardStrings.Keys.NavFacetUpTo;
            if (chip.Dimension == Dimension.Format) return CardNavModel.FormatLabelKey(chip.Value);
            switch (chip.Value)
            {
                case CardOptions.NeedHeadphones: return CardStrings.Keys.NavFacetNoHeadphones;
                case CardOptions.NeedCamera: return CardStrings.Keys.NavFacetNoCamera;
                case CardOptions.NeedNetwork: return CardStrings.Keys.NavFacetNoNetwork;
                default: return null;
            }
        }

        // Whether the block passes every facet that is on (no facet on: every block passes)
        public static bool Passes(BlockFacts block, NavFacets facets)
        {
            if (facets == null) return true;
            if (facets.Format != null && block.Format != facets.Format) return false;
            if (facets.MaxMinutes > 0 && (block.Minutes <= 0 || block.Minutes > facets.MaxMinutes)) return false;
            foreach (string need in facets.Without)
                if (block.NeedsA(need)) return false;
            return true;
        }

        // The blocks of `blocks` that pass, in their order
        public static List<string> Passing(IReadOnlyList<string> blocks, Func<string, BlockFacts> factsOf, NavFacets facets)
        {
            var kept = new List<string>(blocks.Count);
            foreach (string block in blocks)
                if (Passes(factsOf(block), facets)) kept.Add(block);
            return kept;
        }

        // The chips to show for a card whose blocks have these facts, in the menu's order: reading time, what is needed, then the formats
        public static List<Chip> Chips(IReadOnlyList<BlockFacts> card, NavFacets facets)
        {
            var chips = new List<Chip>();
            foreach (int limit in TimeLimits)
            {
                bool active = facets.MaxMinutes == limit;
                // - the longer limit only means something when a block between the two limits exists: otherwise it lists what the shorter one does
                if (active || (Narrows(card, facets, Dimension.Time, f => f.ToggleMinutes(limit)) && (limit == ShortMinutes || HasBetween(card, facets))))
                    chips.Add(new Chip(Dimension.Time, limit.ToString(), limit, active));
            }
            foreach (string need in CardOptions.Needs)
            {
                bool active = ContainsString(facets.Without, need);
                if (active || Narrows(card, facets, Dimension.Without, f => f.ToggleWithout(need)))
                    chips.Add(new Chip(Dimension.Without, need, 0, active));
            }
            foreach (string format in CardOptions.Formats)
            {
                bool active = facets.Format == format;
                if (active || Narrows(card, facets, Dimension.Format, f => f.ToggleFormat(format)))
                    chips.Add(new Chip(Dimension.Format, format, 0, active));
            }
            return chips;
        }

        // Whether switching the facet on keeps at least one block and drops at least one, among the blocks the other facets let through
        private static bool Narrows(IReadOnlyList<BlockFacts> card, NavFacets facets, Dimension dimension, Action<NavFacets> switchOn)
        {
            // - the pool is what the OTHER facets leave; for a "without" chip the pool also keeps the other withouts, which stay AND-ed
            var others = dimension == Dimension.Without ? facets.Copy() : facets.WithoutDimension(dimension);
            var with = others.Copy();
            switchOn(with);
            int pool = 0, kept = 0;
            foreach (var block in card)
            {
                if (!Passes(block, others)) continue;
                pool++;
                if (Passes(block, with)) kept++;
            }
            return kept > 0 && kept < pool;
        }

        // Whether a block the other facets let through takes more than the short limit and at most the medium one
        private static bool HasBetween(IReadOnlyList<BlockFacts> card, NavFacets facets)
        {
            var others = facets.WithoutDimension(Dimension.Time);
            foreach (var block in card)
                if (Passes(block, others) && block.Minutes > ShortMinutes && block.Minutes <= MediumMinutes) return true;
            return false;
        }

        private static bool ContainsString(IReadOnlyList<string> list, string value)
        {
            for (int i = 0; i < list.Count; i++)
                if (list[i] == value) return true;
            return false;
        }
    }
}
