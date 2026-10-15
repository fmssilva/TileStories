using System;
using System.Collections.Generic;

namespace TileStories
{
    // Where a BlockRef field can take the visitor (_3.2 sections 3 and 7): a BlockRef holds the key of another block of the SAME
    // POI's card, and a jump to it (IBlockHost.ScrollToBlock) only works for a block that sits shown in the card's SCROLLING stack --
    // not the header, not a pinned-top or footer look, not a block still waiting for Show After Reading, not one the card leaves out.
    // One pure answer, so the card never draws a button that jumps nowhere and the Editor says why a pick is not drawn: an Open Block
    // action today (actions), a hotspot pin (step 10) and a bento tile (step 15) later.
    public static class BlockRefRule
    {
        // Why a BlockRef does not lead anywhere (None: it does)
        public enum Problem
        {
            None,
            // nothing picked
            Blank,
            // the block that holds the field points at itself
            Itself,
            // no block of this POI has the key (it was deleted)
            Missing,
            // the block exists but the card leaves it out (BlockStackBuilder's skip reasons)
            NotShown,
            // the card shows it outside what scrolls: the header, a pinned-top or a footer look
            NotInScroll,
            // it waits for Show After Reading and has not been revealed
            WaitsForReading,
        }

        // The keys of the blocks this card can scroll to now: every entry of the built stack that sits in the scrolling part and is shown
        public static HashSet<string> JumpTargets(IReadOnlyList<BlockStackBuilder.Entry> entries, Func<string, bool> isRevealed)
        {
            var keys = new HashSet<string>();
            if (entries == null) return keys;
            foreach (var entry in entries)
                if (IsInScroll(entry) && !NavOntologyRule.WaitsToBeRevealed(entry, isRevealed) && !string.IsNullOrEmpty(entry.Instance?.key))
                    keys.Add(entry.Instance.key);
            return keys;
        }

        // Whether an entry of the built stack is drawn inside the scrolling part (BlockStackView puts the header, a pinned-top look
        // and a footer look elsewhere)
        public static bool IsInScroll(BlockStackBuilder.Entry entry) =>
            entry.Definition.Key != BuiltInBlocks.HeaderKind && !entry.Definition.IsPinnedTop(entry.Variant) && !entry.Definition.IsFooter(entry.Variant);

        // What is wrong with `key` as a target of a field of `holder`, a block of `poi` whose card was built as `built`
        public static Problem Check(string key, BlockInstanceData holder, POIData poi, BlockStackBuilder.Result built, Func<string, bool> isRevealed)
        {
            if (string.IsNullOrWhiteSpace(key)) return Problem.Blank;
            if (holder != null && holder.key == key) return Problem.Itself;
            var blocks = poi?.card?.blocks;
            if (blocks == null || !blocks.Exists(b => b != null && b.key == key)) return Problem.Missing;
            foreach (var entry in built?.Entries ?? new List<BlockStackBuilder.Entry>())
            {
                if (entry.Instance?.key != key) continue;
                if (!IsInScroll(entry)) return Problem.NotInScroll;
                return NavOntologyRule.WaitsToBeRevealed(entry, isRevealed) ? Problem.WaitsForReading : Problem.None;
            }
            return Problem.NotShown;
        }
    }
}
