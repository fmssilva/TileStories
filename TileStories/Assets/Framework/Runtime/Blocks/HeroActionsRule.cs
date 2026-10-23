using System.Collections.Generic;

namespace TileStories
{
    // The card's actions at the foot of its hero (_3.3 R11 v3, R10.9, D5), pure: which blocks of the card the hero draws there --
    //   - Primary: the card's sticky call to action (the first Actions block in a footer look, BlockKindDefinition.FooterVariants): the
    //     ONE focal action, drawn in the hero while the hero is in view and in the sticky bar once it scrolled away (CardChromeRule)
    //   - Listen: the card's audio guide, "Listen m:ss" -- the first one pinned to the top (the hero_chip look: the hero IS its place now),
    //     else the first audio guide of the card in any look
    // Either may be missing (no sticky action, no audio guide). The stack binds a second view of each block into the hero's action row.
    public static class HeroActionsRule
    {
        public readonly struct Picks
        {
            public readonly int Primary;
            public readonly int Listen;

            public Picks(int primary, int listen)
            {
                Primary = primary;
                Listen = listen;
            }

            public bool HasPrimary => Primary >= 0;
            public bool HasListen => Listen >= 0;
        }

        // The indexes of the picked entries (-1 = none)
        public static Picks Of(IReadOnlyList<BlockStackBuilder.Entry> entries)
        {
            int primary = -1, pinnedAudio = -1, anyAudio = -1;
            if (entries == null) return new Picks(-1, -1);
            for (int i = 0; i < entries.Count; i++)
            {
                var kind = entries[i].Definition;
                if (kind == null) continue;
                if (primary < 0 && kind.Key == BuiltInBlocks.ActionsKind && kind.IsFooter(entries[i].Variant)) primary = i;
                if (kind.Key != BuiltInBlocks.AudioGuideKind) continue;
                if (pinnedAudio < 0 && kind.IsPinnedTop(entries[i].Variant)) pinnedAudio = i;
                if (anyAudio < 0) anyAudio = i;
            }
            return new Picks(primary, pinnedAudio >= 0 ? pinnedAudio : anyAudio);
        }
    }
}
