using System;
using System.Collections.Generic;

namespace TileStories
{
    // The collapse rules of the card's navigation (_3.2 section 2.5), pure, in the order they apply to the rendered stack.
    // They keep the three levels (family -> kind -> block) to at most two taps:
    //   1. only TARGETS count: a block in the scroll -- not the header, not a pinned-top or footer look, not a block still
    //      waiting to be revealed (ShowAfterViewed), not a block of a family that is no navigation target (InNav off: meta)
    //   2. a section with no target is hidden (CardNavModel only ever makes a section out of a target)
    //   3. fewer targets than Show Navigation From (min_blocks) -> the card shows NO navigation element, scroll only
    //   4. a section with one target jumps straight to that block (L2 skipped); a kind with one instance skips L3
    //   5. a bar with more sections than Bar Slots (max_slots) turns its last slot into More, which lists the rest
    // Every answer about a block comes from its kind's own definition (IsPinnedTop, IsFooter, ShowAfterViewedField) and its
    // registered family (InNav) -- the same members BlockStackView places the block by -- never from a list of look names.
    public static class NavOntologyRule
    {
        // What a wall that never wrote card_settings.navigation gets: the defaults of the config type itself
        private static readonly CardNavigationSettings Defaults = new();

        // Rule 1: may a navigation element jump to this block? `isRevealed` says whether a block key's ShowAfterViewed gate
        // has opened (the card's CardLocalState.Seen); null = nothing revealed yet
        public static bool IsTarget(BlockStackBuilder.Entry entry, BlockRegistry registry, Func<string, bool> isRevealed)
        {
            var kind = entry.Definition;
            if (kind == null || kind.Key == BuiltInBlocks.HeaderKind) return false;
            // - pinned under the header / in the footer: always in view, so there is nowhere to jump to
            if (kind.IsPinnedTop(entry.Variant) || kind.IsFooter(entry.Variant)) return false;
            if (WaitsToBeRevealed(entry, isRevealed)) return false;
            return registry != null && registry.TryGetFamily(kind.Family, out var family) && family.InNav;
        }

        // Whether the stack keeps this block hidden right now: its kind names a ShowAfterViewed field, the block has it on, and
        // its reveal has not happened (the same three facts BlockStackView hides the slot on)
        public static bool WaitsToBeRevealed(BlockStackBuilder.Entry entry, Func<string, bool> isRevealed)
        {
            string field = entry.Definition?.ShowAfterViewedField;
            if (field == null || !new BlockFieldReader(entry.Instance, null, null).Flag(field)) return false;
            return !(isRevealed?.Invoke(entry.Instance?.key) ?? false);
        }

        // What the sections are: "format" only when the wall says so; anything else (unset, a typo) is the family default
        public static string GroupBy(CardNavigationSettings settings) =>
            settings?.group_by == CardOptions.GroupByFormat ? CardOptions.GroupByFormat : CardOptions.GroupByFamily;

        // Show Navigation From, kept inside its range (a hand-edited 0 must not turn navigation on for a one-block card)
        public static int MinBlocks(CardNavigationSettings settings) =>
            Clamp((settings ?? Defaults).min_blocks, CardNavigationSettings.MinBlocksMin, CardNavigationSettings.MinBlocksMax);

        // Bar Slots, kept inside the 3..5 a navigation bar holds
        public static int MaxSlots(CardNavigationSettings settings) =>
            Clamp((settings ?? Defaults).max_slots, CardNavigationSettings.MaxSlotsMin, CardNavigationSettings.MaxSlotsMax);

        // Rule 3: does the card show any navigation element at all?
        public static bool ShowsNavigation(int targetCount, CardNavigationSettings settings) => targetCount >= MinBlocks(settings);

        // Rule 4: the one block a section (or a kind) with a single target jumps straight to; null when there is a choice to make
        public static string DirectTarget(IReadOnlyList<string> targets) => targets != null && targets.Count == 1 ? targets[0] : null;

        // Rule 5: how many sections a bar of `maxSlots` shows by themselves. All of them when they fit; else one slot fewer,
        // because the last slot is More
        public static int SectionsInBar(int sectionCount, int maxSlots) => sectionCount <= maxSlots ? sectionCount : maxSlots - 1;

        private static int Clamp(int value, int min, int max) => value < min ? min : value > max ? max : value;
    }
}
