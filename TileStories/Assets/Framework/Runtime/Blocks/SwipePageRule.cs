using UnityEngine;

namespace TileStories
{
    // When one finger's drag across a paged view (the lightbox, _3.1 [7B]) turns the page. Pure, so every case is a unit test:
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
    }
}
