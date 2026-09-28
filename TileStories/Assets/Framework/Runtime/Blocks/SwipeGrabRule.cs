using UnityEngine;

namespace TileStories
{
    // When a finger that went down on a swipeable card (the true / false statement) takes the pointer for the swipe, and when it is
    // left to the stack's ScrollView (_3.1 8A-fix). Pure, so every case is a unit test. Beside SwipePageRule, which judges the
    // FINISHED swipe: this one judges the drag while it is still going, so a finger that starts on a big card and moves up or down
    // scrolls the stack instead of being swallowed by the card.
    //   - it must have moved further than a small slop: a wobble of a tap is neither
    //   - and clearly sideways (further across than up or down); a diagonal drag that is mostly vertical stays the ScrollView's
    public static class SwipeGrabRule
    {
        // Panel units a finger must travel before it is judged at all
        public const float Slop = 10f;

        // `travel`: the finger's move from where it went down (panel units, y down)
        public static bool ClaimsPointer(Vector2 travel) => Mathf.Abs(travel.x) > Slop && Mathf.Abs(travel.x) > Mathf.Abs(travel.y);
    }
}
