using NUnit.Framework;
using UnityEngine;

namespace TileStories.Tests
{
    // Tier-0 tests for the pure-static zoom math in ARZoomController/ARZoomMath.
    // No scene, no input devices, no ARZoomController instance needed -- mirrors
    // LODController's static-method testability contract (spec section 9).
    public class ARZoomMathTests
    {
        // Defaults mirror the LivingRoom/WallConfigData defaults: min=1, max=4,
        // step=1.5, levels=2. With step=1.5 the discrete levels are [1.0, 1.5, 2.25].
        private const float Min = 1f;
        private const float Max = 4f;
        private const float Step = 1.5f;
        private const int Levels = 2;

        [Test]
        public void ComputeTapLevels_StandardConfig()
        {
            // 1.0 * 1.5^0 = 1.0 ; 1.0 * 1.5^1 = 1.5 ; 1.0 * 1.5^2 = 2.25 ; all in range.
            float[] levels = ARZoomMath.ComputeTapLevels(Step, Levels, Min, Max);
            Assert.That(levels, Is.EqualTo(new[] { 1.0f, 1.5f, 2.25f }));
        }

        [Test]
        public void ComputeTapLevels_ClampedAndCollapsedAtMax()
        {
            // [1.0, 3.0, 4.0] -- 3.0 ok, 9.0 clamped to 4.0 (distinct from 3.0).
            float[] levels = ARZoomMath.ComputeTapLevels(3f, 2, Min, Max);
            Assert.That(levels, Is.EqualTo(new[] { 1.0f, 3.0f, 4.0f }));
        }

        [Test]
        public void ComputeTapLevels_ClampsAtMin()
        {
            // step=0.5, tapLevels=2: 0.5 and 0.25 both below min(1) -> clamped to 1.0,
            // collapsed (not distinct) -> just base level.
            float[] levels = ARZoomMath.ComputeTapLevels(0.5f, 2, Min, Max);
            Assert.That(levels, Is.EqualTo(new[] { 1.0f }));
        }

        [Test]
        public void ComputeTapLevels_GuardsNonPositiveStep()
        {
            Assert.That(ARZoomMath.ComputeTapLevels(0f, Levels, Min, Max), Is.EqualTo(new[] { 1.0f }));
            Assert.That(ARZoomMath.ComputeTapLevels(-1f, Levels, Min, Max), Is.EqualTo(new[] { 1.0f }));
        }

        [Test]
        public void ComputeTapLevels_GuardsNegativeLevels()
        {
            Assert.That(ARZoomMath.ComputeTapLevels(Step, -5, Min, Max), Is.EqualTo(new[] { 1.0f }));
        }

        [Test]
        public void ComputeTapLevels_BaseAlwaysFirst()
        {
            float[] levels = ARZoomMath.ComputeTapLevels(Step, Levels, Min, Max);
            Assert.That(levels[0], Is.EqualTo(1.0f).Within(1e-6f));
        }

        [Test]
        public void NextTapLevel_CyclesForwardAndWrapsToBase()
        {
            // 1.0 -> 1.5 -> 2.25 -> 1.0 (3rd tap returns to base, per config comment)
            float a = ARZoomMath.NextTapLevel(1.0f, Step, Levels, Min, Max);
            float b = ARZoomMath.NextTapLevel(a, Step, Levels, Min, Max);
            float c = ARZoomMath.NextTapLevel(b, Step, Levels, Min, Max);
            Assert.That(a, Is.EqualTo(1.5f).Within(1e-5f));
            Assert.That(b, Is.EqualTo(2.25f).Within(1e-5f));
            Assert.That(c, Is.EqualTo(1.0f).Within(1e-5f));
        }

        [Test]
        public void NextTapLevel_NoWrap_CapsAtMax()
        {
            // + button (wrap:false): at max already -> stays at max (no jarring wrap to base).
            Assert.That(ARZoomMath.NextTapLevel(2.25f, Step, Levels, Min, Max, wrap: false), Is.EqualTo(2.25f).Within(1e-5f));
            // one step below max advances to max.
            Assert.That(ARZoomMath.NextTapLevel(1.5f, Step, Levels, Min, Max, wrap: false), Is.EqualTo(2.25f).Within(1e-5f));
            // 2.0 snaps nearest to 2.25, then caps at max (no wrap to base).
            Assert.That(ARZoomMath.NextTapLevel(2.0f, Step, Levels, Min, Max, wrap: false), Is.EqualTo(2.25f).Within(1e-5f));
        }

        [Test]
        public void NextTapLevel_NoWrap_FromBelowAdvances()
        {
            Assert.That(ARZoomMath.NextTapLevel(1.0f, Step, Levels, Min, Max, wrap: false), Is.EqualTo(1.5f).Within(1e-5f));
        }

        [Test]
        public void PreviousTapLevel_RetreatsOneLevel()
        {
            Assert.That(ARZoomMath.PreviousTapLevel(2.25f, Step, Levels, Min, Max), Is.EqualTo(1.5f).Within(1e-5f));
            Assert.That(ARZoomMath.PreviousTapLevel(1.5f, Step, Levels, Min, Max), Is.EqualTo(1.0f).Within(1e-5f));
            Assert.That(ARZoomMath.PreviousTapLevel(1.4f, Step, Levels, Min, Max), Is.EqualTo(1.0f).Within(1e-5f));
        }

        [Test]
        public void PreviousTapLevel_AtBaseStaysBase()
        {
            Assert.That(ARZoomMath.PreviousTapLevel(1.0f, Step, Levels, Min, Max), Is.EqualTo(1.0f).Within(1e-5f));
            Assert.That(ARZoomMath.PreviousTapLevel(1.2f, Step, Levels, Min, Max), Is.EqualTo(1.0f).Within(1e-5f));
            Assert.That(ARZoomMath.PreviousTapLevel(1.4f, Step, Levels, Min, Max), Is.EqualTo(1.0f).Within(1e-5f));
        }

        [Test]
        public void NextDoubleTapTarget_TogglesBaseToMax()
        {
            // near base (1.0) -> max; anything else -> base.
            Assert.That(ARZoomMath.NextDoubleTapTarget(1.0f, Min, Max), Is.EqualTo(Max).Within(1e-5f));
            Assert.That(ARZoomMath.NextDoubleTapTarget(4.0f, Min, Max), Is.EqualTo(1.0f).Within(1e-5f));
            Assert.That(ARZoomMath.NextDoubleTapTarget(2.0f, Min, Max), Is.EqualTo(1.0f).Within(1e-5f));
        }

        [Test]
        public void NextDoubleTapTarget_FarFromMaxIsStillBase()
        {
            // 3.9 is NOT near base, so double-tap goes back to base (not max).
            Assert.That(ARZoomMath.NextDoubleTapTarget(3.9f, Min, Max), Is.EqualTo(1.0f).Within(1e-5f));
        }

        [Test]
        public void ApplyPinchScale_ClampsToRange()
        {
            // 3.5 * 2 = 7 -> clamped to max 4
            Assert.That(ARZoomMath.ApplyPinchScale(3.5f, 2f, Min, Max), Is.EqualTo(Max).Within(1e-5f));
            // 1.5 * 0.1 = 0.15 -> clamped to min 1
            Assert.That(ARZoomMath.ApplyPinchScale(1.5f, 0.1f, Min, Max), Is.EqualTo(Min).Within(1e-5f));
            // in-range multiplication unchanged
            Assert.That(ARZoomMath.ApplyPinchScale(2.0f, 1.5f, Min, Max), Is.EqualTo(3.0f).Within(1e-5f));
        }

        [Test]
        public void StepTowardTarget_ReachesTargetWhenDeltaEqualsSpeed()
        {
            // deltaTime == transitionSpeed -> t=1 -> exactly target.
            Assert.That(ARZoomMath.StepTowardTarget(1.0f, 2.0f, 0.25f, 0.25f), Is.EqualTo(2.0f).Within(1e-5f));
        }

        [Test]
        public void StepTowardTarget_ApproachesMonotonicallyWithoutOvershoot()
        {
            // mid-step: t=0.5 -> 1.5 (between start and target, no overshoot).
            float mid = ARZoomMath.StepTowardTarget(1.0f, 2.0f, 0.25f, 0.125f);
            Assert.That(mid, Is.EqualTo(1.5f).Within(1e-5f));

            // large delta is clamped to t=1 -> target, still no overshoot past it.
            float big = ARZoomMath.StepTowardTarget(1.0f, 2.0f, 0.25f, 5.0f);
            Assert.That(big, Is.LessThanOrEqualTo(2.0f + 1e-5f));
            Assert.That(big, Is.EqualTo(2.0f).Within(1e-4f));

            // descending direction stays monotonic and bounded.
            float down = ARZoomMath.StepTowardTarget(3.0f, 1.0f, 0.25f, 0.1f);
            Assert.That(down, Is.LessThan(3.0f));
            Assert.That(down, Is.GreaterThan(1.0f));
        }

        [Test]
        public void StepTowardTarget_InstantWhenZeroOrNegativeSpeed()
        {
            Assert.That(ARZoomMath.StepTowardTarget(1.0f, 2.0f, 0f, 0.01f), Is.EqualTo(2.0f).Within(1e-5f));
            Assert.That(ARZoomMath.StepTowardTarget(1.0f, 2.0f, -1f, 0.01f), Is.EqualTo(2.0f).Within(1e-5f));
        }

        [Test]
        public void StepTowardTarget_AlreadyAtTarget()
        {
            Assert.That(ARZoomMath.StepTowardTarget(2.0f, 2.0f, 0.25f, 0.01f), Is.EqualTo(2.0f).Within(1e-5f));
        }
    }
}
