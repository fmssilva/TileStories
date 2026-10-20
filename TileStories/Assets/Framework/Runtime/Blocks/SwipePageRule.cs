using UnityEngine;

namespace TileStories
{
    // When one finger's drag across a paged view (the lightbox, _3.1 [7B]; the card's pages layout, _3.2 step 14) turns the page, and which
    // page it lands on. Pure, so every case is a unit test:
    //   - it must travel far enough: a share of the page's width, so a small wobble of a tap never turns a page
    //   - it must be clearly sideways: a mostly vertical drag is not a page swipe
    //   - the finger moving LEFT brings the next page in (as on every phone), moving right the previous one
    public static class SwipePageRule
    {
        // A swipe travels at least this share of the page's width
        public const float MinTravelShare = 0.2f;

        // ...and at least this many times further sideways than up or down
        public const float MinSideways = 2f;

        // +1 = the next page, -1 = the previous page, 0 = not a page swipe. `travel`: the finger's move from press to release
        // (panel units, y down); `pageWidth`: the page's width in the same units.
        public static int Direction(Vector2 travel, float pageWidth)
        {
            if (!(pageWidth > 0f)) return 0;
            float sideways = Mathf.Abs(travel.x);
            if (sideways < MinTravelShare * pageWidth || sideways < MinSideways * Mathf.Abs(travel.y)) return 0;
            return travel.x < 0f ? 1 : -1;
        }

        // The page a released swipe lands on (the card's pages layout, _3.2 step 14): one page on in its direction, never past the first or the
        // last (no wrap: the end of the card is an end). `page`: the page shown (0-based) of `count`
        public static int Landing(int page, int count, int direction)
        {
            if (count <= 0) return -1;
            return Mathf.Clamp(page + System.Math.Sign(direction), 0, count - 1);
        }

        // How far the page follows a sideways finger while it drags: all the way towards a page that exists; towards none (left of the first,
        // right of the last) a fraction of it, so the edge answers the finger without promising a page
        public const float EdgeResistance = 0.3f;

        public static float DragOffset(float travelX, bool hasPrevious, bool hasNext)
        {
            bool towardsNone = travelX > 0f && !hasPrevious || travelX < 0f && !hasNext;
            return towardsNone ? travelX * EdgeResistance : travelX;
        }
    }
}
