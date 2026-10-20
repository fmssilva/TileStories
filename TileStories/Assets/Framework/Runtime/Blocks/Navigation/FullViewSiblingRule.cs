using System.Collections.Generic;

namespace TileStories
{
    // Which blocks the full view moves between without returning to the card (_3.2 section 5.7, step 16, F4), pure: a block's SIBLINGS are
    // the targets of its own section in the card's map (CardNavModel) whose kind supports the full view (lists the Expandable display, the
    // same predicate that offers a compact row's full-view button), in the stack's order -- the block itself among them.
    //   - the section, not a family lookup: with Group By = family (the default) it IS the block's family; with Group By = format it is the
    //     format the card's own elements show, so the breadcrumb's middle segment, the jump it makes and the siblings always agree
    //   - none for a block that is no target (the header, a pinned or footer look, a nav block, the sources) and none for a section of one:
    //     a single page is not a choice, so the full view then draws no chips and takes no swipe
    public static class FullViewSiblingRule
    {
        // The section the breadcrumb names for `blockKey` (its index in model.Sections), -1 for none
        public static int SectionOf(CardNavModel model, string blockKey) => model?.SectionOf(blockKey) ?? -1;

        // The block's siblings, itself included, in the stack's order; empty when it has none to move to
        public static IReadOnlyList<string> Siblings(CardNavModel model, BlockRegistry registry, string blockKey)
        {
            var siblings = new List<string>();
            int section = SectionOf(model, blockKey);
            if (section < 0 || registry == null) return siblings;
            foreach (string target in model.Sections[section].Targets)
                if (SupportsTheFullView(model.KindOf(target)?.Key, registry)) siblings.Add(target);
            // - one page is not a choice; and a block whose own kind refuses the full view opens alone
            if (siblings.Count < 2 || !siblings.Contains(blockKey)) siblings.Clear();
            return siblings;
        }

        // Whether a kind can be drawn in the full view: it lists the Expandable display (every built-in kind but the header)
        public static bool SupportsTheFullView(string kind, BlockRegistry registry)
        {
            if (kind == null || !registry.TryGet(kind, out var definition) || definition.DisplayModes == null) return false;
            foreach (string mode in definition.DisplayModes)
                if (mode == CardOptions.DisplayExpandable) return true;
            return false;
        }
    }
}
