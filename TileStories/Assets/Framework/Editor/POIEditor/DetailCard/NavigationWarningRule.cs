// NavigationWarningRule.cs
//
// The conditions of Detail Card > Navigation's warnings (_3.2 section 7), pure and Editor-only: a warning tells the developer that a
// choice leaves a visitor with less than it seems (too many bars for the reading room, too many sections for a picture), it never
// changes a card and no visitor ever sees it. A "section" is what the card's own map counts (CardNavModel.Sections: the families, or the
// formats under Group By = Format) -- read from the REAL model of the busiest card of the wall, not guessed from the family list.

using System.Collections.Generic;

namespace TileStories.Editor
{
    internal static class NavigationWarningRule
    {
        // The sections past which the Progress Map's pieces stop showing their share: each keeps a whole finger's width and the bar
        // scrolls (about 4-5 on a phone, measured at _3.2 step 6; the (i) of Top Bar and the Test guide say it in the same words)
        public const int ProgressMapSectionsThatFit = 5;
        // The sections past which the bento's tiles stop filling the card: about 13 with the title open and the bottom buttons shown, 17 once
        // the title has folded away (measured at _3.2 step 15b; at Half the tiles are always at their smallest)
        public const int BentoSectionsThatFill = 13;
        public const int BentoSectionsThatFillWithTheTitleFolded = 17;

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

        // The busiest card of `pois`, each built by the card's own stack builder and map exactly as the visitor's card is
        public static BusiestCard BusiestOf(CardSettings settings, IReadOnlyList<POIData> pois, BlockRegistry registry)
        {
            int best = 0, at = -1;
            if (pois == null || registry == null) return new BusiestCard(0, -1);
            for (int i = 0; i < pois.Count; i++)
            {
                if (pois[i]?.card?.blocks == null || pois[i].card.blocks.Count == 0) continue;
                var model = CardNavModel.Build(BlockStackBuilder.Build(pois[i], settings, registry, pois).Entries, registry, settings);
                if (!model.ShowsNavigation || model.Sections.Count <= best) continue;
                best = model.Sections.Count;
                at = i;
            }
            return new BusiestCard(best, at);
        }

        // The Progress Map is the Top Bar the card draws and the busiest card has more sections than its pieces can show a share for.
        // `sections` is asked only once the choice itself says it matters: a card's map costs a stack build per point
        public static bool ProgressMapOutgrown(CardNavigationSettings nav, System.Func<int> sections) =>
            CardLayoutRule.TopElement(nav) == BuiltInNavElements.ProgressMap && sections() > ProgressMapSectionsThatFit;

        // Layout = Bento and the busiest card has more sections than the tiles can fill the card with
        public static bool BentoOutgrown(CardNavigationSettings nav, System.Func<int> sections) =>
            CardLayoutRule.Layout(nav) == CardOptions.LayoutBento && sections() > BentoSectionsThatFill;

        // The Top Bar the card draws, the Bottom Bar and the Edge are all on: three bars share a screen that is mostly for reading
        public static bool MoreThanTwoBars(CardNavigationSettings nav) =>
            !IsNone(CardLayoutRule.TopElement(nav)) && !IsNone(nav?.bottom) && !IsNone(nav?.edge);

        // A bar (or the radial) is on, its Bar Slots is below the sections of the busiest card AND the row could still be raised: the last
        // slot is More and hides the rest. At the row's own maximum nothing is left for the developer to fix, so no warning
        public static bool BarSlotsBelowTheSections(CardNavigationSettings nav, bool anElementReadsBarSlots, System.Func<int> sections) =>
            anElementReadsBarSlots && NavOntologyRule.MaxSlots(nav) < CardNavigationSettings.MaxSlotsMax && NavOntologyRule.MaxSlots(nav) < sections();

        // Whether the card of `pois[poiIndex]` shows navigation at all (what Preview Navigation needs to have something to show)
        public static bool CardShowsNavigation(CardSettings settings, IReadOnlyList<POIData> pois, int poiIndex, BlockRegistry registry) =>
            poiIndex >= 0 && poiIndex < pois.Count &&
            CardNavModel.Build(BlockStackBuilder.Build(pois[poiIndex], settings, registry, pois).Entries, registry, settings).ShowsNavigation;

        private static bool IsNone(string key) => string.IsNullOrEmpty(key) || key == CardOptions.NavNone;
    }
}
