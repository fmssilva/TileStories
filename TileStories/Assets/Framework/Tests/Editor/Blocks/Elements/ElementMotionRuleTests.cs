using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // The motion and media maths of the composed blocks (_3.3 S2b: R17.3, R6, R11 v4, the P4.4b carousel), pure: when a row has entered the
    // view, how rows that enter together follow one another, a reveal's pose frame by frame, parallax inside a frame's room, the slow zoom and
    // the pan there and back, the ONE row that moves by itself, Reduce Motion as a rule; a frame's proportion and an arch's top; where a
    // snapping strip rests. The numbers are a phone's (an 844 pt view) and every expectation is worked out from the rule's own constants.
    public class ElementMotionRuleTests
    {
        private const float View = 844f;

        [Test]
        public void ARow_HasEnteredTheView_OnceItsTopCrossesTheEnterLine_AndUntilItIsWhollyAbove()
        {
            // - judged half a pixel either side of the line: exactly ON it is no real input (the product's last bit depends on the runtime's
            //   float precision), so "<" and "<=" there are the same rule
            float line = View * ElementMotionRule.EnterLine;
            Assert.IsFalse(ElementMotionRule.Entered(line + 1f, line + 200f, View), "its top under the line: not yet");
            Assert.IsFalse(ElementMotionRule.Entered(line + 0.5f, line + 200f, View), "half a pixel under the line: not yet");
            Assert.IsTrue(ElementMotionRule.Entered(line - 0.5f, line + 200f, View), "half a pixel above the line: entered");
            Assert.IsTrue(ElementMotionRule.Entered(line - 1f, line + 200f, View), "just above the line: entered");
            Assert.IsTrue(ElementMotionRule.Entered(-150f, 10f, View), "scrolled mostly past but still showing: entered");
            Assert.IsFalse(ElementMotionRule.Entered(-300f, -1f, View), "wholly above the view (a jump went past it): not entered");
            Assert.IsFalse(ElementMotionRule.Entered(10f, 50f, 0f), "no view measured: never");
            Assert.IsFalse(ElementMotionRule.Entered(float.NaN, 50f, View), "not laid out: never");
        }

        [Test]
        public void RowsEnteringTogether_FollowOneByOne_TheWaitCappedAtFiveSteps()
        {
            Assert.AreEqual(0f, ElementMotionRule.StaggerDelay(0), "the first starts at once");
            for (int order = 1; order <= ElementMotionRule.MaxStaggerSteps; order++)
                Assert.AreEqual(order * ElementMotionRule.Stagger, ElementMotionRule.StaggerDelay(order), 1e-5f, "row " + order);
            Assert.AreEqual(ElementMotionRule.StaggerDelay(ElementMotionRule.MaxStaggerSteps), ElementMotionRule.StaggerDelay(12), "the twelfth waits no longer than the fifth");
            Assert.AreEqual(0f, ElementMotionRule.StaggerDelay(-3), "never a negative wait");
        }

        [Test]
        public void AReveal_FadesInAndRises_Decelerating_ToRestAtItsDuration_AndStrongTravelsTwiceAsFar()
        {
            Assert.AreEqual(0f, ElementMotionRule.RevealProgress(0f));
            Assert.AreEqual(0f, ElementMotionRule.RevealProgress(-1f), "before it starts (a staggered row waiting)");
            Assert.AreEqual(1f, ElementMotionRule.RevealProgress(ElementMotionRule.RevealDuration), 1e-5f);
            Assert.AreEqual(1f, ElementMotionRule.RevealProgress(10f), "and stays at rest");
            Assert.Greater(ElementMotionRule.RevealProgress(ElementMotionRule.RevealDuration / 3f), 0.5f, "decelerating: past half in the first third");

            // - frame by frame (every 0.1 s over 1 s): opacity only grows, the rise only shrinks, at rest from the duration on
            float lastOpacity = -1f, lastRise = float.MaxValue;
            for (int f = 0; f <= 10; f++)
            {
                float t = f * 0.1f;
                var (opacity, offset) = ElementMotionRule.EnterPose(ElementOptions.MotionReveal, ElementOptions.StrengthSubtle, ElementMotionRule.RevealProgress(t));
                Assert.GreaterOrEqual(opacity, lastOpacity, "frame " + f + ": the opacity never drops");
                Assert.LessOrEqual(offset.y, lastRise, "frame " + f + ": the rise never grows");
                Assert.AreEqual(0f, offset.x, "a reveal only rises");
                if (t >= ElementMotionRule.RevealDuration) Assert.AreEqual((1f, Vector2.zero), (opacity, offset), "frame " + f + ": at rest");
                lastOpacity = opacity;
                lastRise = offset.y;
            }
            Assert.AreEqual((0f, new Vector2(0f, ElementMotionRule.RevealRise)), ElementMotionRule.EnterPose(ElementOptions.MotionReveal, ElementOptions.StrengthSubtle, 0f), "before: hidden, RevealRise below its place");
            Assert.AreEqual(ElementMotionRule.RevealRise * ElementMotionRule.StrongFactor,
                ElementMotionRule.EnterPose(ElementOptions.MotionReveal, ElementOptions.StrengthStrong, 0f).Offset.y, "strong: twice the rise");
            Assert.AreEqual(new Vector2(-ElementMotionRule.SlideDistance, 0f), ElementMotionRule.EnterPose(ElementOptions.MotionSlideIn, ElementOptions.StrengthSubtle, 0f).Offset,
                "slide in: from the start side");
            foreach (string still in new[] { ElementOptions.MotionNone, ElementOptions.MotionParallax, ElementOptions.MotionSlowZoom, ElementOptions.MotionPan })
                Assert.AreEqual((1f, Vector2.zero), ElementMotionRule.EnterPose(still, ElementOptions.StrengthStrong, 0f), still + " does not enter: always at rest");
        }

        [Test]
        public void Parallax_MovesThePictureAgainstItsDistanceToTheViewsCentre_InsideItsFramesRoom()
        {
            const float centre = View / 2f, room = 40f;
            Assert.AreEqual(0f, ElementMotionRule.ParallaxOffset(centre, centre, ElementOptions.StrengthSubtle, room), "at the centre: at rest");
            // - a picture 200 below the centre sits 0.1 x 200 = 20 up inside its frame; above the centre, down
            Assert.AreEqual(-200f * ElementMotionRule.ParallaxFactor, ElementMotionRule.ParallaxOffset(centre + 200f, centre, ElementOptions.StrengthSubtle, room), 1e-4f);
            Assert.AreEqual(200f * ElementMotionRule.ParallaxFactor, ElementMotionRule.ParallaxOffset(centre - 200f, centre, ElementOptions.StrengthSubtle, room), 1e-4f);
            Assert.AreEqual(-200f * ElementMotionRule.ParallaxFactor * ElementMotionRule.StrongFactor,
                ElementMotionRule.ParallaxOffset(centre + 200f, centre, ElementOptions.StrengthStrong, room), 1e-4f, "strong: twice");
            Assert.AreEqual(-room, ElementMotionRule.ParallaxOffset(centre + 5000f, centre, ElementOptions.StrengthSubtle, room), "never past the frame's room (no empty edge)");
            Assert.AreEqual(room, ElementMotionRule.ParallaxOffset(centre - 5000f, centre, ElementOptions.StrengthStrong, room));
            Assert.AreEqual(0f, ElementMotionRule.ParallaxOffset(centre + 200f, centre, ElementOptions.StrengthSubtle, 0f), "no room: still");
            Assert.AreEqual(0f, ElementMotionRule.ParallaxOffset(float.NaN, centre, ElementOptions.StrengthSubtle, room), "not laid out: still");
        }

        [Test]
        public void SlowZoomAndPan_GoThereAndBackOncePerPeriod_EasedAtBothEnds_InsideTheRoom()
        {
            Assert.AreEqual(1f, ElementMotionRule.ZoomScale(0f, ElementOptions.StrengthSubtle), 1e-5f, "starts filling the frame");
            Assert.AreEqual(ElementMotionRule.ZoomMax, ElementMotionRule.ZoomScale(ElementMotionRule.ZoomPeriod / 2f, ElementOptions.StrengthSubtle), 1e-5f, "pushed in halfway");
            Assert.AreEqual(1f, ElementMotionRule.ZoomScale(ElementMotionRule.ZoomPeriod, ElementOptions.StrengthSubtle), 1e-5f, "back after one period");
            Assert.AreEqual(1f + (ElementMotionRule.ZoomMax - 1f) * ElementMotionRule.StrongFactor,
                ElementMotionRule.ZoomScale(ElementMotionRule.ZoomPeriod / 2f, ElementOptions.StrengthStrong), 1e-5f, "strong: twice the push-in");
            // - slow: over one second (the frame sequence's span) a subtle zoom moves well under 1 %
            Assert.Less(ElementMotionRule.ZoomScale(1f, ElementOptions.StrengthSubtle) - 1f, 0.01f, "slow");
            Assert.Greater(ElementMotionRule.ZoomScale(1f, ElementOptions.StrengthSubtle), 1f, "but moving");

            const float room = 30f;
            float travel = room * ElementMotionRule.PanShare;
            Assert.AreEqual(travel, ElementMotionRule.PanOffset(0f, ElementOptions.StrengthSubtle, room), 1e-4f, "starts at one side");
            Assert.AreEqual(-travel, ElementMotionRule.PanOffset(ElementMotionRule.PanPeriod / 2f, ElementOptions.StrengthSubtle, room), 1e-4f, "the other side halfway");
            Assert.AreEqual(-room, ElementMotionRule.PanOffset(ElementMotionRule.PanPeriod / 2f, ElementOptions.StrengthStrong, room), 1e-4f, "strong: the whole room, never past it");
            Assert.AreEqual(0f, ElementMotionRule.PanOffset(3f, ElementOptions.StrengthStrong, 0f), "no room: still");
        }

        [Test]
        public void OnlyOneRowMovesByItself_TheOneInViewNearestTheCentre()
        {
            var rows = new List<(bool, float, float)>
            {
                (false, 1f, 0f),     // a parallax picture at the centre: moves with the scroll, never counted
                (true, 0f, 10f),     // a zoom out of view: stopped
                (true, 0.5f, 300f),  // a pan in view, far from the centre
                (true, 1f, -120f),   // a zoom in view, nearer (above the centre)
                (true, 1f, 120f),    // a zoom as near, below: the first of equals wins
            };
            Assert.AreEqual(3, ElementMotionRule.Mover(rows));
            rows[3] = (true, 0f, -120f);
            Assert.AreEqual(4, ElementMotionRule.Mover(rows), "the nearest scrolled out: the next nearest takes over");
            Assert.AreEqual(-1, ElementMotionRule.Mover(new List<(bool, float, float)> { (true, 0f, 0f), (false, 1f, 0f) }), "none in view: none moves");
        }

        [Test]
        public void ReduceMotion_LeavesEveryEffectStill()
        {
            foreach (string effect in ElementOptions.Motions)
                Assert.AreEqual(ElementMotionRule.Reduced.Still, ElementMotionRule.ReducedOf(effect), effect);
            Assert.IsTrue(ElementMotionRule.Enters(ElementOptions.MotionReveal) && ElementMotionRule.Enters(ElementOptions.MotionSlideIn));
            Assert.IsTrue(ElementMotionRule.MovesByItself(ElementOptions.MotionSlowZoom) && ElementMotionRule.MovesByItself(ElementOptions.MotionPan));
            Assert.IsFalse(ElementMotionRule.MovesByItself(ElementOptions.MotionParallax), "parallax moves only with the visitor's scroll");
        }

        [Test]
        public void AFrame_FollowsItsPicturesProportion_BetweenAFourByFivePortraitAndATwoByOnePanorama()
        {
            Assert.AreEqual(16f / 9f, ElementMediaRule.FrameAspect(16f / 9f), 1e-5f, "a wide picture keeps its proportion");
            Assert.AreEqual(4f / 3f, ElementMediaRule.FrameAspect(4f / 3f), 1e-5f, "a 4:3 engraving keeps its proportion");
            Assert.AreEqual(ElementMediaRule.TallestAspect, ElementMediaRule.FrameAspect(0.75f), "a 3:4 portrait print is framed at 4:5");
            Assert.AreEqual(ElementMediaRule.TallestAspect, ElementMediaRule.FrameAspect(0.4f), "a very tall picture: no taller than 4:5");
            Assert.AreEqual(ElementMediaRule.WidestAspect, ElementMediaRule.FrameAspect(4f), "a panorama: no flatter than 2:1");
            Assert.AreEqual(ElementMediaRule.FallbackAspect, ElementMediaRule.FrameAspect(0f), "not loaded: the fallback");
            Assert.AreEqual(ElementMediaRule.FallbackAspect, ElementMediaRule.FrameAspect(float.NaN));
            Assert.AreEqual(179f, ElementMediaRule.ArchRadius(358f, 268f), "an arch: a half circle as wide as the frame");
            Assert.AreEqual(100f, ElementMediaRule.ArchRadius(358f, 100f), "never past the frame's height");
            Assert.AreEqual(0f, ElementMediaRule.ArchRadius(float.NaN, 100f));
        }

        // The story's carousel (App Store style): items 280 wide, 10 apart, seen through a 374 strip (390 less its 16 pt left inset, the
        // viewport's padding) -- a released strip rests on the nearest item start, and the last ones against the end
        [Test]
        public void ASnappingStrip_RestsOnTheNearestItemStart_AndTheLastAgainstTheEnd()
        {
            var starts = new[] { 0f, 290f, 580f };
            const float content = 870f, viewport = 374f;
            Assert.AreEqual(0f, NavStripRule.SnapOffset(starts, 100f, content, viewport), "nearer the first");
            Assert.AreEqual(290f, NavStripRule.SnapOffset(starts, 200f, content, viewport), "past halfway: the second");
            Assert.AreEqual(NavStripRule.MaxOffset(content, viewport), NavStripRule.SnapOffset(starts, 560f, content, viewport), "the last item: against the end");
            Assert.AreEqual(0f, NavStripRule.SnapOffset(starts, -40f, content, viewport), "dragged past the start: back to it");
            Assert.AreEqual(0f, NavStripRule.SnapOffset(starts, 30f, 300f, viewport), "a strip that fits never moves");
            Assert.AreEqual(50f, NavStripRule.SnapOffset(new float[0], 50f, content, viewport), "no items: where it was (clamped)");
        }
    }
}
