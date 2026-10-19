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

        // Whether the stack draws this block as a COMPACT ROW that binds its real view only when opened (_3.2 step 12, display Expandable):
        // its resolved display is expandable AND it sits in what scrolls. A look the card pins under the header or in the footer (the audio
        // hero chip, the sticky call to action) stays where its look puts it, drawn whole: a row there would hide the very thing it is for
        public static bool ShowsCompact(BlockStackBuilder.Entry entry)
        {
            var kind = entry.Definition;
            if (kind == null || Resolve(kind, entry.Instance?.display) != CardOptions.DisplayExpandable) return false;
            return !kind.IsPinnedTop(entry.Variant) && !kind.IsFooter(entry.Variant);
        }
    }
}
