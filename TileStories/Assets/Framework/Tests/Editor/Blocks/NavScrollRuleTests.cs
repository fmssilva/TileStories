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
