// NavigationWarningRule.cs
//
// The conditions of Detail Card > Navigation's notes (_3.2 section 7), pure and Editor-only: a note tells the developer that a
// choice leaves a visitor with less than it seems (more groups than a phone's bar holds, a preview card with no navigation), it never
// changes a card and no visitor ever sees it. A "section" is what the card's own map counts (CardNavModel.Sections: the families, or the
// formats under Group By = Format) -- read from the REAL model of the busiest card of the wall, not guessed from the family list.

using System.Collections.Generic;

namespace TileStories.Editor
{
    internal static class NavigationWarningRule
    {
        // The card of the wall with the most sections, among the cards that show navigation at all (a shorter card only scrolls)
        internal readonly struct BusiestCard
        {
            // How many sections its map has (0 when no card of the wall shows navigation)
            public readonly int Sections;
            // Its index in the wall's list of points (-1 for none): what a message names it by (its list title, never its id)
            public readonly int PoiIndex;

            public BusiestCard(int sections, int poiIndex)
            {
                Sections = sections;
                PoiIndex = poiIndex;
            }
        }

        // The card that shows the most Navigation Bar groups (_3.3 S4a review): a card shows only the groups it has blocks of, so a wall's
        // long table may still fit every phone. Built only when the table is long enough to matter (a stack build per point)
        public static BusiestCard BusiestBar(CardSettings settings, IReadOnlyList<POIData> pois, BlockRegistry registry)
        {
            int best = 0, at = -1;
            if (pois == null || registry == null) return new BusiestCard(0, -1);
            var items = NavBarRule.Items(settings?.navigation, registry);
            for (int i = 0; i < pois.Count; i++)
            {
                if (pois[i]?.card?.blocks == null || pois[i].card.blocks.Count == 0) continue;
                var entries = BlockStackBuilder.Build(pois[i], settings, registry, pois).Entries;
                var model = CardNavModel.Build(entries, registry, settings);
                if (!model.ShowsNavigation) continue;
                var family = new Dictionary<string, string>();
                foreach (var entry in entries) family[entry.Instance.key] = entry.Definition.Family;
                int groups = NavBarRule.Entries(items, model.Targets, t => family.TryGetValue(t, out string f) ? f : null).Count;
                if (groups <= best) continue;
                best = groups;
                at = i;
            }
            return new BusiestCard(best, at);
        }

        // Whether the card of `pois[poiIndex]` shows navigation at all (what Preview Navigation needs to have something to show)
        public static bool CardShowsNavigation(CardSettings settings, IReadOnlyList<POIData> pois, int poiIndex, BlockRegistry registry) =>
            poiIndex >= 0 && poiIndex < pois.Count &&
            CardNavModel.Build(BlockStackBuilder.Build(pois[poiIndex], settings, registry, pois).Entries, registry, settings).ShowsNavigation;
    }
}
