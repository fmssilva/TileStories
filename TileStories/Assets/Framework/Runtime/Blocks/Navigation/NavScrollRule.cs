using System.Collections.Generic;
using UnityEngine;

namespace TileStories
{
    // Where a navigation jump lands and which section the reader is in (_3.2 section 4.4), pure: NavController only applies it.
    // Everything is measured in the stack's own content (a slot's layout.y, the scroll offset): the nav chrome sits OUTSIDE the
    // scroll (_3.2 section 4.2), so no offset for it ever enters the arithmetic.
    //   - a jump's target offset = the slot's top, clamped to [0, content - viewport]
    //   - the current section = the last one whose first target's top is at or above the SPY LINE (a third down the viewport),
    //     and the LAST section once the stack is scrolled to its end (else a short final family could never be marked)
    //   - while a jump glides, the spy is frozen on the jump's section (else an element's marker flickers through every family
    //     the glide passes), then released
    public static class NavScrollRule
    {
        // The spy line, as a fraction of the viewport height: a block counts as "being read" once it fills the upper third
        public const float SpyLineFraction = 1f / 3f;

        // How close to the end of the range counts as "at the end" (panel units): one pixel, since a scroll stopped by the clamp
        // can rest a fraction short of the range
        public const float EndTolerance = 1f;

        // How close the stack's offset must be to a jump's landing to count as resting on it (panel units): one pixel of rounding
        public const float LandingTolerance = 1f;

        // How far the stack can scroll: what of the content does not fit the viewport (0 when it all fits, or before layout)
        public static float Range(float contentHeight, float viewportHeight)
        {
            float range = contentHeight - viewportHeight;
            return float.IsNaN(range) || range < 0f ? 0f : range;
        }

        // The scroll offset that puts a slot's top at the viewport's top, clamped to what the stack can scroll
        public static float TargetOffset(float slotTop, float contentHeight, float viewportHeight) =>
            Mathf.Clamp(float.IsNaN(slotTop) ? 0f : slotTop, 0f, Range(contentHeight, viewportHeight));

        // The section the reader is in at `offset`, from the tops of each section's first target (in the stack's order, as
        // CardNavModel gives them); -1 with no section. Before the first section's top reaches the line the first section is
        // named (an element always marks one), and a stack that does not scroll is read from its top, never "at its end".
        public static int CurrentSection(IReadOnlyList<float> sectionTops, float offset, float contentHeight, float viewportHeight)
        {
            if (sectionTops == null || sectionTops.Count == 0) return -1;
            if (IsAtEnd(offset, contentHeight, viewportHeight)) return sectionTops.Count - 1;
            float spyLine = SpyLine(offset, viewportHeight);
            int current = 0;
            for (int i = 1; i < sectionTops.Count; i++)
                if (sectionTops[i] <= spyLine) current = i;
            return current;
        }

        // Whether a block counts as SEEN at `offset` (_3.2 step 11b, the contents list's tick): it is on screen and the spy line has passed
        // its top -- the very line CurrentSection reads, so the tick and the marked section can never disagree. Where the line can never
        // pass the lower blocks, every block on screen counts: at the end of the range (the same rule that names the last section there) and
        // on a stack that does not scroll at all (all of it is in view -- ContentSeenRule's "all of it fits"). On screen matters: after a
        // jump the blocks above the landing have their tops above the line too, yet the visitor never saw them. No viewport (the peek stop)
        // or no layout: never
        public static bool HasViewed(float blockTop, float blockBottom, float offset, float contentHeight, float viewportHeight)
        {
            if (float.IsNaN(blockTop) || float.IsNaN(blockBottom) || float.IsNaN(contentHeight) || float.IsNaN(viewportHeight) || viewportHeight <= 0f)
                return false;
            bool onScreen = blockBottom > offset && blockTop < offset + viewportHeight;
            if (!onScreen) return false;
            bool lineCannotReach = IsAtEnd(offset, contentHeight, viewportHeight) || Range(contentHeight, viewportHeight) <= 0f;
            return lineCannotReach || blockTop <= SpyLine(offset, viewportHeight);
        }

        // Where the spy line sits in the content at `offset`
        private static float SpyLine(float offset, float viewportHeight) => offset + viewportHeight * SpyLineFraction;

        // Whether the stack rests at the end of what it can scroll (a stack that does not scroll is read from its top, never "at its end")
        private static bool IsAtEnd(float offset, float contentHeight, float viewportHeight)
        {
            float range = Range(contentHeight, viewportHeight);
            return range > 0f && offset >= range - EndTolerance;
        }

        // What the spy names now: a running jump's section while it glides (frozen); else what the scroll shows -- unless the
        // wall turned Follow Scroll off, when it stays where the last jump put it (`current`)
        public static int Spy(bool followScroll, int jumpSection, int measured, int current) =>
            jumpSection >= 0 ? jumpSection : followScroll ? measured : current;

        // How far a glide has come after `elapsed` of `duration` seconds: eased in and out, 1 once the time is up
        public static float Progress(float elapsed, float duration)
        {
            if (duration <= 0f || elapsed >= duration) return 1f;
            float t = Mathf.Clamp01(elapsed / duration);
            return t * t * (3f - 2f * t);
        }

        // The offset a glide from `start` shows at `progress` towards `target` (the caller re-reads the target every frame: the
        // sheet may still be growing to Full, which moves the clamp)
        public static float Glide(float start, float target, float progress) => start + (target - start) * progress;

        // Whether a jump is over. Every jump holds its landing for at least one motion length (`settle`), the glide included: the card
        // reacts to the scroll it made over the next frames -- the sheet raised to Full grows, the header collapses, the strip comes back
        // and the footer slides away (_3.2 step 6: an instant jump at Half ended before that and its block landed 60 px low) -- and until
        // then the ScrollView can also clamp the offset short of the landing (at peek, to 0). After that it ends once the stack rests on a
        // landing that held still for a frame, and one motion length later whatever happens
        public static bool JumpDone(float elapsed, float glide, float settle, bool landed)
        {
            float hold = Mathf.Max(glide, settle);
            return elapsed >= hold && (landed || elapsed >= hold + settle);
        }

        // Jump Opens Card To: anything but "keep" raises the sheet to Full first (the default; an unknown value too)
        public static bool RaisesToFull(CardNavigationSettings settings) => settings?.jump_raises_to != CardOptions.JumpRaisesKeep;

        // Jump Motion: set at once when the wall says instant or the card reduces motion; else glide
        public static bool IsInstant(CardNavigationSettings settings, bool reduceMotion) =>
            reduceMotion || settings?.jump_motion == CardOptions.JumpMotionInstant;

        // Follow Scroll (on unless the wall turned it off)
        public static bool FollowsScroll(CardNavigationSettings settings) => settings?.follow_scroll ?? true;
    }
}
