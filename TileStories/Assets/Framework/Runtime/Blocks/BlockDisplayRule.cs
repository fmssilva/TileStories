namespace TileStories
{
    // How a block is displayed (_3.1 section 3, BlockInstanceData.display; step 9B made it live), pure: the instance's own display when its
    // kind offers it, else the kind's first display mode -- so an old config, a typo or a mode a kind dropped still shows the block.
    public static class BlockDisplayRule
    {
        public static string Resolve(BlockKindDefinition kind, string display)
        {
            var modes = kind?.DisplayModes;
            if (modes == null || modes.Count == 0) return CardOptions.DisplayInline;
            for (int i = 0; i < modes.Count; i++)
                if (modes[i] == display) return display;
            return modes[0];
        }
    }
}
