using System.Collections.Generic;

namespace TileStories
{
    // Whether a block count fits beside its word (_3.2 section 5.1, the Segmented element with Show Counts on), pure. The segments share a
    // strip in equal parts, so a word and its count can be wider than the part they get. The word always wins: it is what names the
    // group, a count is only a hint -- so when a count cannot sit beside its word the COUNT goes, never a letter of the word.
    //   - one decision for the whole strip: counts on some segments and not on others reads as a mistake, so they show only when EVERY
    //     segment holds its word, the gap and its count; else none shows
    //   - a word wider than its segment with no count still wraps between its words (the element's own promise); that is not this rule's
    public static class NavFitRule
    {
        // `words[i]` and `counts[i]` are the natural widths (no wrapping) of segment i's word and count, `gap` the space between them,
        // `room` the width a segment has inside its padding. No segments at all: nothing to leave out, so true
        public static bool CountsFit(IReadOnlyList<float> words, IReadOnlyList<float> counts, float gap, float room)
        {
            if (words == null || counts == null) return true;
            int n = words.Count < counts.Count ? words.Count : counts.Count;
            for (int i = 0; i < n; i++)
                if (words[i] + gap + counts[i] > room) return false;
            return true;
        }
    }
}
