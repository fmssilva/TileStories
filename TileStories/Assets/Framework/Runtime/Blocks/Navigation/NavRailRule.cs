using UnityEngine;

namespace TileStories
{
    // How long a rail of the nav_rails block may be (_3.2 section 5.6, D3), pure. NN/g (Budiu 2018): the last item of a carousel must be
    // reachable in 3-4 swipes or visitors give up before it. A SWIPE here is one viewport of travel (a thumb's stroke across the strip), so
    // a rail of `cells` cells `step` wide (a card and its gap, CardTrackRule's width) is SwipesToEnd away from its end. A family with more
    // blocks than MostCells lets the rail keep its first cards and end on an "All n" card that moves the card to the family -- every block
    // stays one tap away and the rail never runs longer than the swipes the developer allows.
    public static class NavRailRule
    {
        // What a rail draws: this many block cards, then the "All n" card when blocks are left over
        public readonly struct Plan
        {
            public readonly int Cards;
            public readonly bool HasAll;

            public Plan(int cards, bool hasAll)
            {
                Cards = cards;
                HasAll = hasAll;
            }

            // Cells in the row, the "All" card included
            public int Cells => Cards + (HasAll ? 1 : 0);
        }

        // How many swipes reach the end of a rail of `cells` cells (0 when it fits the viewport)
        public static int SwipesToEnd(int cells, float step, float viewport)
        {
            if (cells <= 0 || step <= 0f || viewport <= 0f) return 0;
            float reach = cells * step - viewport;
            return reach <= 0f ? 0 : Mathf.CeilToInt(reach / viewport - 0.0001f);
        }

        // The most cells a rail may hold and still end within `maxSwipes` swipes (at least two: a card and the "All" card)
        public static int MostCells(float step, float viewport, int maxSwipes)
        {
            if (step <= 0f || viewport <= 0f) return 2;
            return Mathf.Max(2, Mathf.FloorToInt(viewport * (Mathf.Max(0, maxSwipes) + 1) / step + 0.0001f));
        }

        // The rail of a family with `targets` blocks: every block while the rail stays within the swipes, else the cards that fit with one
        // cell kept for the "All n" card
        public static Plan Of(int targets, float step, float viewport, int maxSwipes)
        {
            if (targets <= 0) return new Plan(0, false);
            int most = MostCells(step, viewport, maxSwipes);
            return targets <= most ? new Plan(targets, false) : new Plan(most - 1, true);
        }
    }
}
