using System.Collections.Generic;

namespace TileStories
{
    // Where a poll block would get the OTHER visitors' votes (_3.1 step 8B). There is no backend today, so the only
    // implementation is NoPollResults, which has none: the card then shows the visitor's own choice and a thank-you, and never a
    // percentage it made up. When a backend exists (plan Stage 3 telemetry), an implementation of this interface returns the
    // counts and the poll view draws its results bars without any other change.
    public interface IPollResults
    {
        // The number of votes each option of the poll has, indexed by the option's AUTHORED ROW (the same number the visitor's own
        // vote is stored under). False = there are no results to show (the view hides its results bars).
        bool TryGet(string wallId, string poiId, string blockKey, out IReadOnlyList<int> votesPerRow);
    }

    // The default: no results anywhere (no backend)
    public sealed class NoPollResults : IPollResults
    {
        public bool TryGet(string wallId, string poiId, string blockKey, out IReadOnlyList<int> votesPerRow)
        {
            votesPerRow = null;
            return false;
        }
    }
}
