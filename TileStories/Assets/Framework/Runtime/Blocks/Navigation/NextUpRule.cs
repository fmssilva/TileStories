using System.Collections.Generic;

namespace TileStories
{
    // What the Up next bar (next_up, _3.2 section 5.2, canvas B4) says, pure: given the card's targets in the stack's order and the
    // block being read, which block comes next and what number is it among ALL the targets. The view only draws the answer.
    //   - the block being read is target i -> the next is target i + 1, its number is i + 2 (1-based: "5 of 13" names the 5th target)
    //   - the block being read is the LAST target -> Finished: there is no next, so the bar offers no Next that would go nowhere
    //   - the block being read is no target (nothing read yet, or a key a live edit just removed) -> the next is the FIRST target, 1 of N
    //   - no targets at all -> an empty answer (the card shows no navigation, so no bar either)
    public static class NextUpRule
    {
        public readonly struct Answer
        {
            // The block Next jumps to; null when Finished or when the card has no targets
            public readonly string NextBlock;
            // The 1-based place of NextBlock among all the targets; once Finished, the last target's own place (the card's size)
            public readonly int Position;
            // How many targets the card has
            public readonly int Total;
            // The reader is at the last target
            public readonly bool Finished;

            public Answer(string nextBlock, int position, int total, bool finished)
            {
                NextBlock = nextBlock;
                Position = position;
                Total = total;
                Finished = finished;
            }
        }

        // The answer for `currentBlock` (a key of `targets`, or anything else / null for "no target read")
        public static Answer Of(IReadOnlyList<string> targets, string currentBlock)
        {
            int total = targets?.Count ?? 0;
            if (total == 0) return new Answer(null, 0, 0, finished: false);
            int at = IndexOf(targets, currentBlock);
            if (at < 0) return new Answer(targets[0], 1, total, finished: false);
            if (at == total - 1) return new Answer(null, total, total, finished: true);
            return new Answer(targets[at + 1], at + 2, total, finished: false);
        }

        // Whether the bar shows the block's place ("5 of 13") in this layout: where no other counter is on screen. The pages layout's Family
        // Pager names the page ("3 of 8") and is the card's one counter there -- a second number of another kind (blocks, not pages) beside it
        // would be two answers to "where am I". The bar still names the next block and offers Next
        public static bool ShowsPosition(string effectiveLayout) => !CardLayoutRule.TurnsPages(effectiveLayout);

        private static int IndexOf(IReadOnlyList<string> targets, string blockKey)
        {
            if (blockKey == null) return -1;
            for (int i = 0; i < targets.Count; i++)
                if (targets[i] == blockKey) return i;
            return -1;
        }
    }
}
