using NUnit.Framework;

namespace TileStories.Editor.Tests
{
    // NavScrollRule (_3.2 section 4.4): where a jump lands, which section the spy names, the freeze while a jump glides, and the
    // three navigation options it reads. Pure numbers in the stack's own content units; every expectation is derived from the
    // rule's own constants (the spy line, the end tolerance), never re-typed.
    public class NavScrollRuleTests
    {
        // A stack 3000 tall seen through a 600 viewport (range 2400); four sections whose first targets start at these tops
        private const float Content = 3000f;
        private const float Viewport = 600f;
        private static readonly float[] Tops = { 400f, 1100f, 1900f, 2700f };
        private static float SpyLineAt(float offset) => offset + Viewport * NavScrollRule.SpyLineFraction;

        [Test]
        public void TheRange_IsWhatDoesNotFit_AndNeverNegativeOrNaN()
        {
            Assert.AreEqual(2400f, NavScrollRule.Range(Content, Viewport));
            Assert.AreEqual(0f, NavScrollRule.Range(400f, Viewport), "content shorter than the viewport cannot scroll");
            Assert.AreEqual(0f, NavScrollRule.Range(float.NaN, Viewport), "before layout: nothing to scroll");
        }

        [Test]
        public void ATarget_LandsWithItsSlotTopAtTheViewportTop_ClampedToTheRange()
        {
            Assert.AreEqual(1100f, NavScrollRule.TargetOffset(1100f, Content, Viewport), "inside the range: exactly the slot's top");
            Assert.AreEqual(NavScrollRule.Range(Content, Viewport), NavScrollRule.TargetOffset(2700f, Content, Viewport),
                "a slot too close to the end: the clamp stops at the range (its top then sits lower in the viewport)");
            Assert.AreEqual(0f, NavScrollRule.TargetOffset(-5f, Content, Viewport), "never above the top");
            Assert.AreEqual(0f, NavScrollRule.TargetOffset(float.NaN, Content, Viewport), "an unlaid slot goes nowhere");
            Assert.AreEqual(0f, NavScrollRule.TargetOffset(300f, 400f, Viewport), "a stack that does not scroll stays at 0");
        }

        [Test]
        public void TheSpy_NamesTheLastSectionWhoseFirstTargetIsAboveTheSpyLine_AtThreeOffsets()
        {
            // - offset 0: the line is a third down the viewport, past the first top (400 <= 200? no) -> before any top: the first
            Assert.Less(SpyLineAt(0f), Tops[0], "precondition: at the top the line has not reached the first section");
            Assert.AreEqual(0, NavScrollRule.CurrentSection(Tops, 0f, Content, Viewport), "before any section: the first is named");

            // - just past the second section's top by the line, not the third's
            float second = Tops[1] - Viewport * NavScrollRule.SpyLineFraction;
            Assert.AreEqual(1, NavScrollRule.CurrentSection(Tops, second, Content, Viewport), "the line exactly on section 2's top");
            Assert.AreEqual(0, NavScrollRule.CurrentSection(Tops, second - 1f, Content, Viewport), "one unit earlier: still section 1");

            // - the third section, mid-way into it
            float third = Tops[2];
            Assert.That(SpyLineAt(third), Is.LessThan(Tops[3]), "precondition: the line is before section 4");
            Assert.AreEqual(2, NavScrollRule.CurrentSection(Tops, third, Content, Viewport));
        }

        [Test]
        public void AtTheEndOfTheRange_TheLastSectionIsNamed_EvenIfItsTopNeverReachesTheLine()
        {
            float range = NavScrollRule.Range(Content, Viewport);
            Assert.Greater(Tops[3], SpyLineAt(range), "precondition: the last section is short -- its top stays under the line at the end");
            Assert.AreEqual(3, NavScrollRule.CurrentSection(Tops, range, Content, Viewport), "at the end: the last section");
            Assert.AreEqual(3, NavScrollRule.CurrentSection(Tops, range - NavScrollRule.EndTolerance, Content, Viewport),
                "within the end tolerance of the range: the clamp may rest a fraction short");
            Assert.AreEqual(2, NavScrollRule.CurrentSection(Tops, range - NavScrollRule.EndTolerance - 1f, Content, Viewport),
                "further from the end: back to the line's answer");
        }

        [Test]
        public void AStackThatDoesNotScroll_IsReadFromItsTop_AndNoSectionNamesNone()
        {
            var fits = new[] { 50f, 300f, 450f };
            Assert.Less(SpyLineAt(0f), fits[1], "precondition: the line is before the second section");
            Assert.AreEqual(0, NavScrollRule.CurrentSection(fits, 0f, 500f, Viewport),
                "everything fits (range 0): the top reading wins, not 'at the end' (which would name the last)");
            Assert.AreEqual(-1, NavScrollRule.CurrentSection(new float[0], 0f, Content, Viewport));
            Assert.AreEqual(-1, NavScrollRule.CurrentSection(null, 0f, Content, Viewport));
        }

        // A section SHORTER than the spy line's distance (_3.3 P1b: The Painting's Listen, one audio player, 206 px under a 212 px line):
        // landed at the viewport's top, the next section's top is past the line too -- the one at the top is read, not the next. Found on
        // the real card: a tap on Listen marked Play
        [Test]
        public void AShortSection_LandedAtTheTop_IsTheOneRead_NotTheNextWhoseTopIsPastTheLineToo()
        {
            float spy = Viewport * NavScrollRule.SpyLineFraction;
            float shortTop = 1100f;
            float shortHeight = spy * 0.75f;
            var tops = new[] { 400f, shortTop, shortTop + shortHeight, 2700f };
            Assert.Less(tops[2], SpyLineAt(shortTop), "precondition: landed on the short section, the next one's top is past the line too");
            Assert.Less(shortTop, NavScrollRule.Range(Content, Viewport), "precondition: the short section can reach the viewport's top");

            Assert.AreEqual(1, NavScrollRule.CurrentSection(tops, shortTop, Content, Viewport), "landed: the short section, at the viewport's top");
            Assert.AreEqual(1, NavScrollRule.CurrentSection(tops, shortTop + NavScrollRule.LandingTolerance, Content, Viewport),
                "a landing that rests a rounding past its top: still the short section");
            Assert.AreEqual(2, NavScrollRule.CurrentSection(tops, shortTop + NavScrollRule.LandingTolerance + 1f, Content, Viewport),
                "its top scrolled away: the next section, already past the line, is read");
            // - read by a drag from above: the line reaches the short top first, the old answer holds until then
            Assert.AreEqual(1, NavScrollRule.CurrentSection(tops, shortTop - spy, Content, Viewport), "the line exactly on the short top");
            Assert.AreEqual(0, NavScrollRule.CurrentSection(tops, shortTop - spy - 1f, Content, Viewport), "one unit earlier: still the section above");
            float bothPast = (tops[2] - spy + shortTop) * 0.5f;
            Assert.LessOrEqual(tops[2], SpyLineAt(bothPast), "precondition: the next top is past the line");
            Assert.Greater(shortTop, bothPast, "precondition: the short top sits below the viewport's top");
            Assert.AreEqual(1, NavScrollRule.CurrentSection(tops, bothPast, Content, Viewport),
                "the next top past the line while the short one sits lower on screen: the short one, nearer the top");
        }

        // Two short sections in a row: of the tops past the line, the FIRST still on screen -- never the last one past it
        [Test]
        public void TwoShortSectionsInARow_TheOneAtTheViewportTopIsRead()
        {
            float spy = Viewport * NavScrollRule.SpyLineFraction;
            var tops = new[] { 400f, 1100f, 1100f + spy * 0.4f, 1100f + spy * 0.8f, 2700f };
            Assert.AreEqual(1, NavScrollRule.CurrentSection(tops, tops[1], Content, Viewport), "landed on the first short one");
            Assert.AreEqual(2, NavScrollRule.CurrentSection(tops, tops[2], Content, Viewport), "landed on the second: the one above it is off screen");
            Assert.AreEqual(3, NavScrollRule.CurrentSection(tops, tops[3], Content, Viewport), "landed on the third");
        }

        // The contents list's tick (_3.2 step 11b): a block is seen once the SAME spy line passed its top while it is on screen. Every
        // number comes from the rule's own constants and this fixture's stack (3000 seen through 600, range 2400)
        [Test]
        public void ABlockIsViewed_OnceTheSpyLinePassedItsTop_JustAboveYes_JustBelowNo()
        {
            const float offset = 1000f, height = 150f;
            float line = SpyLineAt(offset);
            Assert.IsTrue(NavScrollRule.HasViewed(true, line - 1f, line - 1f + height, offset, Content, Viewport), "its top one unit above the line");
            Assert.IsTrue(NavScrollRule.HasViewed(true, line, line + height, offset, Content, Viewport), "its top exactly on the line");
            Assert.IsFalse(NavScrollRule.HasViewed(true, line + 1f, line + 1f + height, offset, Content, Viewport),
                "its top one unit below the line: on screen, but not read yet");
            Assert.IsTrue(NavScrollRule.HasViewed(true, offset - 100f, offset + 20f, offset, Content, Viewport),
                "a tall block whose top went off the top while its end still shows: the line passed it");
        }

        [Test]
        public void ABlockOffScreen_IsNeverViewed_EvenWithItsTopAboveTheLine()
        {
            // - after a jump that lands at 1000 the blocks above have tops above the line, but they were flown past, never on screen
            const float offset = 1000f;
            Assert.IsFalse(NavScrollRule.HasViewed(true, 700f, 1000f, offset, Content, Viewport), "it ends exactly at the viewport's top");
            Assert.IsFalse(NavScrollRule.HasViewed(true, 200f, 400f, offset, Content, Viewport), "far above");
            Assert.IsFalse(NavScrollRule.HasViewed(true, offset + Viewport, offset + Viewport + 100f, offset, Content, Viewport), "below the viewport");
        }

        [Test]
        public void AtTheEndOfTheStack_EveryBlockOnScreenIsViewed_AndOnAStackThatDoesNotScroll()
        {
            float range = NavScrollRule.Range(Content, Viewport);
            float lowTop = Content - 120f;
            Assert.Greater(lowTop, SpyLineAt(range), "precondition: the last block's top stays under the line even at the end");
            Assert.IsTrue(NavScrollRule.HasViewed(true, lowTop, Content, range, Content, Viewport), "at the end: the line can go no further, it counts");
            Assert.IsTrue(NavScrollRule.HasViewed(true, lowTop, Content, range - NavScrollRule.EndTolerance, Content, Viewport), "within the end tolerance");
            Assert.IsFalse(NavScrollRule.HasViewed(true, lowTop, Content, range - NavScrollRule.EndTolerance - 1f, Content, Viewport),
                "further from the end: back to the line's answer");
            Assert.IsTrue(NavScrollRule.HasViewed(true, 450f, 520f, 0f, 520f, Viewport),
                "a stack that does not scroll is all in view: its lowest block counts although it sits under the line");
        }

        [Test]
        public void AClosedRow_IsNeverViewed_WhereverTheStackIs_AndAnOpenRowIsMeasuredByItsContent()
        {
            float offset = 600f;
            float line = SpyLineAt(offset);
            // - a closed compact row whose line sits right in the middle of the screen, past the spy line: it shows none of its block
            Assert.IsFalse(NavScrollRule.HasViewed(false, line - 40f, line + 20f, offset, Content, Viewport), "closed, on screen, past the line");
            float range = NavScrollRule.Range(Content, Viewport);
            Assert.IsFalse(NavScrollRule.HasViewed(false, Content - 60f, Content, range, Content, Viewport), "closed, at the end of the stack");
            Assert.IsFalse(NavScrollRule.HasViewed(false, 450f, 520f, 0f, 520f, Viewport), "closed, on a stack that does not scroll");
            // - open: its content (under the line of the row) is what counts -- a header past the line with its content still below is not read
            float rowTop = line - 40f;
            float contentTop = rowTop + 56f;
            Assert.Greater(contentTop, line, "precondition: the row's line is past the spy line, its content is not");
            Assert.IsFalse(NavScrollRule.HasViewed(true, contentTop, contentTop + 300f, offset, Content, Viewport),
                "open, its content still under the line: not read yet");
            Assert.IsTrue(NavScrollRule.HasViewed(true, contentTop, contentTop + 300f, contentTop - Viewport * NavScrollRule.SpyLineFraction, Content, Viewport),
                "open, its content's top reaches the line: read");
        }

        [Test]
        public void NoViewportOrNoLayout_ViewsNothing()
        {
            Assert.IsFalse(NavScrollRule.HasViewed(true, 0f, 100f, 0f, Content, 0f), "the peek stop: no viewport, nothing is visible");
            Assert.IsFalse(NavScrollRule.HasViewed(true, float.NaN, 100f, 0f, Content, Viewport), "a slot not laid out yet");
            Assert.IsFalse(NavScrollRule.HasViewed(true, 0f, float.NaN, 0f, Content, Viewport));
            Assert.IsFalse(NavScrollRule.HasViewed(true, 0f, 100f, 0f, float.NaN, Viewport), "no content height yet");
            Assert.IsFalse(NavScrollRule.HasViewed(true, 0f, 100f, 0f, Content, float.NaN));
        }

        // _3.2 audit F1 (mutation 83): at the peek stop the stack has a 0 px viewport and "peek = never" is the rule's own comment. The
        // checks above pass without the guard (a block at the top is "off screen" in a 0 px viewport anyway), so these blocks STRADDLE the
        // offset: only the guard keeps them from counting
        [Test]
        public void AZeroPixelViewport_NeverCountsABlockAsViewed_EvenOneStraddlingTheOffset()
        {
            const float offset = 500f;
            Assert.IsFalse(NavScrollRule.HasViewed(true, offset - 100f, offset + 100f, offset, Content, 0f),
                "a block across the offset, its top above the line: with no viewport nothing is visible");
            Assert.IsFalse(NavScrollRule.HasViewed(true, offset - 100f, offset + 100f, offset, 0f, 0f), "a stack with no content and no viewport");
            Assert.IsFalse(NavScrollRule.HasViewed(true, offset - 100f, offset + 100f, offset, Content, -1f), "a negative viewport is none either");
            Assert.IsTrue(NavScrollRule.HasViewed(true, offset - 100f, offset + 100f, offset, Content, Viewport),
                "precondition: the same block with a real viewport IS viewed");
        }

        [Test]
        public void WhileAJumpGlides_TheSpyIsFrozenOnItsSection_ThenReleased()
        {
            // - a jump to section 3 glides through sections 0..2: the measured answer changes, the spy does not
            foreach (int passing in new[] { 0, 1, 2 })
                Assert.AreEqual(3, NavScrollRule.Spy(followScroll: true, jumpSection: 3, measured: passing, current: 0), "frozen while gliding past " + passing);
            Assert.AreEqual(2, NavScrollRule.Spy(followScroll: true, jumpSection: -1, measured: 2, current: 3), "released: the scroll's answer again");
            Assert.AreEqual(3, NavScrollRule.Spy(followScroll: false, jumpSection: 3, measured: 1, current: 0), "Follow Scroll off: a jump still moves it");
            Assert.AreEqual(3, NavScrollRule.Spy(followScroll: false, jumpSection: -1, measured: 1, current: 3), "Follow Scroll off: scrolling does not");
        }

        [Test]
        public void TheGlide_EasesFromStartToTarget_AndEndsWhenItsLandingHoldsStill()
        {
            Assert.AreEqual(0f, NavScrollRule.Progress(0f, 0.2f));
            Assert.AreEqual(0.5f, NavScrollRule.Progress(0.1f, 0.2f), 1e-5f, "half the time, half the way (eased in and out)");
            Assert.Less(NavScrollRule.Progress(0.05f, 0.2f), 0.25f, "eased: slow at the start");
            Assert.AreEqual(1f, NavScrollRule.Progress(0.3f, 0.2f), "past its time: there");
            Assert.AreEqual(1f, NavScrollRule.Progress(0f, 0f), "no duration (instant): there at once");
            Assert.AreEqual(250f, NavScrollRule.Glide(100f, 400f, 0.5f));

            const float glide = 0.2f, settle = 0.2f;
            Assert.IsFalse(NavScrollRule.JumpDone(0.1f, glide, settle, landed: true), "not before its glide's time");
            Assert.IsFalse(NavScrollRule.JumpDone(0.25f, glide, settle, landed: false), "its time is up but the stack is not on its landing yet (the sheet grows)");
            Assert.IsTrue(NavScrollRule.JumpDone(0.25f, glide, settle, landed: true));
            Assert.IsTrue(NavScrollRule.JumpDone(glide + settle, glide, settle, landed: false), "the settle window is bounded");
            Assert.IsFalse(NavScrollRule.JumpDone(0f, 0f, settle, landed: true),
                "an instant jump sitting on its landing still holds it: the card reacts to its scroll over the next frames");
            Assert.IsFalse(NavScrollRule.JumpDone(settle - 0.01f, 0f, settle, landed: true), "for one motion length");
            Assert.IsTrue(NavScrollRule.JumpDone(settle, 0f, settle, landed: true), "then it is over once it rests");
            Assert.IsFalse(NavScrollRule.JumpDone(settle, 0f, settle, landed: false), "an instant jump the ScrollView clamped short (a sheet still at peek) holds on");
            Assert.IsTrue(NavScrollRule.JumpDone(2f * settle, 0f, settle, landed: false), "for one more motion length at most");
        }

        [Test]
        public void TheThreeOptions_ReadTheirDefaults_AndReduceMotionForcesInstant()
        {
            var defaults = new CardNavigationSettings();
            Assert.IsTrue(NavScrollRule.RaisesToFull(defaults), "Jump Opens Card To defaults to Full");
            Assert.IsTrue(NavScrollRule.RaisesToFull(null));
            Assert.IsTrue(NavScrollRule.RaisesToFull(new CardNavigationSettings { jump_raises_to = "typo" }), "an unknown value is the default");
            Assert.IsFalse(NavScrollRule.RaisesToFull(new CardNavigationSettings { jump_raises_to = CardOptions.JumpRaisesKeep }));

            Assert.IsFalse(NavScrollRule.IsInstant(defaults, reduceMotion: false), "Jump Motion defaults to smooth");
            Assert.IsTrue(NavScrollRule.IsInstant(new CardNavigationSettings { jump_motion = CardOptions.JumpMotionInstant }, false));
            Assert.IsTrue(NavScrollRule.IsInstant(defaults, reduceMotion: true), "reduce motion forces instant");

            Assert.IsTrue(NavScrollRule.FollowsScroll(defaults), "Follow Scroll defaults to on");
            Assert.IsTrue(NavScrollRule.FollowsScroll(null));
            Assert.IsFalse(NavScrollRule.FollowsScroll(new CardNavigationSettings { follow_scroll = false }));
        }
    }
}
