using System.Collections.Generic;

namespace TileStories
{
    // What a hotspot_image pin does when tapped (_3.2 section 5.5, step 10): a pin whose Opens Block names a block the card can scroll
    // to JUMPS there (IBlockHost.ScrollToBlock); every other pin -- nothing picked, or a pick that leads nowhere (BlockRefRule: the
    // title block, a pinned or footer look, a block waiting for Show After Reading, a deleted block) -- keeps what a pin always did and
    // opens its own title and text. Pure: the view only asks, so a visitor never gets a pin that jumps nowhere.
    public static class HotspotPinRule
    {
        // The key of the block the pin jumps to, or "" when it opens its own text. `jumpTargets`: the blocks the card can scroll to
        // (BlockBindContext.JumpTargets; null = none)
        public static string JumpTarget(string opensBlock, ICollection<string> jumpTargets) =>
            !string.IsNullOrWhiteSpace(opensBlock) && jumpTargets != null && jumpTargets.Contains(opensBlock) ? opensBlock : "";
    }
}
