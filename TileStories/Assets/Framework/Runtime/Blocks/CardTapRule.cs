using UnityEngine;

namespace TileStories
{
    // "Tap on empty camera space closes the card" (_3.1 section 4) as pure rules. A press-and-release counts as a
    // TAP only when the finger barely moved and did not linger (a drag or a long press looks at the wall, it does
    // not close anything); it closes the card only when nothing the EventSystem knows (a marker, the card, the
    // search UI) was under it.
    public static class CardTapRule
    {
        // Movement allowed for a tap, as a share of the screen height (about a fingertip's wobble)
        public const float MaxMoveScreenRatio = 0.02f;

        // Longest press that still counts as a tap
        public const float MaxTapSeconds = 0.35f;

        public static bool IsTap(Vector2 down, Vector2 up, float downTime, float upTime, float screenHeight) =>
            upTime - downTime <= MaxTapSeconds
            && Vector2.Distance(down, up) <= MaxMoveScreenRatio * Mathf.Max(1f, screenHeight);

        public static bool ShouldDismiss(bool cardOpen, bool dismissOnTapOutside, bool isTap, bool anythingUnderTap) =>
            cardOpen && dismissOnTapOutside && isTap && !anythingUnderTap;
    }
}
