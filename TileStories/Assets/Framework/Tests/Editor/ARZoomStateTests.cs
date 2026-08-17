using NUnit.Framework;
using UnityEngine;

namespace TileStories.Tests
{
    // Tier-0 tests for the read-only ARZoomState. ARZoomState has no MonoBehaviour
    // lifetime, so it can be asserted in EditMode without a scene -- mirrors the
    // contract of StatusRampTests (spec section 9). All asserts are deterministic;
    // no NaN, no platform-dependent Mathf behaviour beyond Clamp.
    public class ARZoomStateTests
    {
        private const float Min = 1f;
        private const float Max = 4f;

        [SetUp]
        public void ResetState()
        {
            // ARZoomState is a static global; reset to the no-zoom default between
            // tests so they cannot pollute one another.
            ARZoomState.ResetToBase(Min, Max);
        }

        [TearDown]
        public void ResetStateAfter()
        {
            ARZoomState.ResetToBase(Min, Max);
        }

        [Test]
        public void DefaultZoomFactorIsOne()
        {
            // ResetToBase sets 1.0; that is the contract for "no zoom".
            Assert.That(ARZoomState.ZoomFactor, Is.EqualTo(1.0f).Within(1e-6f));
        }

        [Test]
        public void SetZoom_WritesInRange()
        {
            ARZoomState.SetZoom(2.5f, Min, Max);
            Assert.That(ARZoomState.ZoomFactor, Is.EqualTo(2.5f).Within(1e-6f));
        }

        [Test]
        public void SetZoom_ClampsAboveMax()
        {
            ARZoomState.SetZoom(99f, Min, Max);
            Assert.That(ARZoomState.ZoomFactor, Is.EqualTo(Max).Within(1e-6f));
        }

        [Test]
        public void SetZoom_ClampsBelowMin()
        {
            ARZoomState.SetZoom(-5f, Min, Max);
            Assert.That(ARZoomState.ZoomFactor, Is.EqualTo(Min).Within(1e-6f));
        }

        [Test]
        public void SetZoom_ClampsAtBothExactBounds()
        {
            ARZoomState.SetZoom(Max, Min, Max);
            Assert.That(ARZoomState.ZoomFactor, Is.EqualTo(Max).Within(1e-6f));

            ARZoomState.SetZoom(Min, Min, Max);
            Assert.That(ARZoomState.ZoomFactor, Is.EqualTo(Min).Within(1e-6f));
        }

        [Test]
        public void ResetToBase_SetsToOne()
        {
            ARZoomState.SetZoom(3.0f, Min, Max);
            ARZoomState.ResetToBase(Min, Max);
            Assert.That(ARZoomState.ZoomFactor, Is.EqualTo(1.0f).Within(1e-6f));
        }

        [Test]
        public void ResetToBase_RespectsMinAboveOne()
        {
            // ResetToBase writes 1.0f through SetZoom, which clamps to [min, max].
            // If a wall's zoom_min floors above 1.0, the reset target is clamped to
            // that min (never below the floor) -- 1.0 is unreachable but never broken.
            ARZoomState.SetZoom(9f, 2f, 8f);
            ARZoomState.ResetToBase(2f, 8f);
            Assert.That(ARZoomState.ZoomFactor, Is.EqualTo(2.0f).Within(1e-6f));
        }

        [Test]
        public void SetZoom_ClampsInfinityToBounds()
        {
            // Mathf.Clamp(Infinity, 1, 4) -> 4 ; Mathf.Clamp(-Infinity, 1, 4) -> 1.
            // Deterministic (no NaN); guards a pinched scale from blowing up.
            ARZoomState.SetZoom(float.PositiveInfinity, Min, Max);
            Assert.That(ARZoomState.ZoomFactor, Is.EqualTo(Max).Within(1e-6f));

            ARZoomState.SetZoom(float.NegativeInfinity, Min, Max);
            Assert.That(ARZoomState.ZoomFactor, Is.EqualTo(Min).Within(1e-6f));
        }
    }
}
