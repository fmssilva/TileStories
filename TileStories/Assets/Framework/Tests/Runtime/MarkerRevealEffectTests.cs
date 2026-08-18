using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace TileStories.Tests
{
    // Tier 0 / Tier 0.5 tests for MarkerRevealEffect -- verifies the reveal
    // animation respects the delaySeconds and durationSeconds parameters.
    // PlayMode test because coroutines only tick in Play Mode.
    public class MarkerRevealEffectTests
    {
        private GameObject _go;
        private MarkerRevealEffect _effect;
        private CanvasGroup _canvasGroup;
        private RectTransform _rect;

        [SetUp]
        public void SetUp()
        {
                        _go = new GameObject("TestMarker", typeof(RectTransform), typeof(CanvasGroup));
            _rect = _go.GetComponent<RectTransform>();
            _canvasGroup = _go.GetComponent<CanvasGroup>();
                        _effect = _go.AddComponent<MarkerRevealEffect>();
            // Awake auto-resolves _canvasGroup and _rootRect from GetComponent
        }

        [TearDown]
        public void TearDown()
        {
                        Object.Destroy(_go);
        }

        // Play with delay=0, duration=0.3. Marker should start hidden
        // (alpha=0, scale=0) and become fully visible after the duration.
        [UnityTest]
        public IEnumerator Play_WithZeroDelay_CompletesToFullVisibility()
        {
                                    _effect.Play(0f, 0.3f);

            // EnsureStartHidden runs synchronously in Play() (alpha=0). With
            // delay=0 the coroutine enters its while-loop immediately, so after
            // a frame alpha may be slightly non-zero. Assert the reveal has not
            // COMPLETED rather than asserting exact alpha=0, which is fragile
            // against coroutine scheduling in the Unity Test Framework.
            yield return null;

            Assert.That(_canvasGroup.alpha, Is.LessThan(0.5f), "alpha should be < 0.5 (reveal in progress, not complete)");
            Assert.That(_rect.localScale.x, Is.LessThan(0.5f), "scale.x should be < 0.5 (reveal in progress, not complete)");

            // Wait for the duration plus a buffer
            yield return new WaitForSeconds(0.3f + 0.1f);
            yield return null; // ensure RevealCoroutine's final SetFullAlphaAndScale has run

            Assert.AreEqual(1f, _canvasGroup.alpha, 0.001f, "alpha should be 1 after reveal completes");
            Assert.AreEqual(1f, _rect.localScale.x, 0.001f, "scale.x should be 1 after reveal completes");
        }

        // Play with delay=0.2, duration=0.3. Marker should stay hidden
        // during the delay, then animate during the duration.
        [UnityTest]
        public IEnumerator Play_WithDelay_StaysHiddenUntilDelayExpires()
        {
                        _effect.Play(0.2f, 0.3f);

            yield return null; // coroutine starts; EnsureStartHidden runs synchronously in Play(), then coroutine pauses at delay

            Assert.That(_canvasGroup.alpha, Is.LessThan(0.5f), "alpha should be < 0.5 during delay (not yet revealed)");
            Assert.That(_rect.localScale.x, Is.LessThan(0.5f), "scale.x should be < 0.5 during delay (not yet revealed)");

            // Wait during the delay period (0.1s < 0.2s delay)
            yield return new WaitForSeconds(0.1f);

                        Assert.That(_canvasGroup.alpha, Is.LessThan(0.5f), "alpha should still be < 0.5 during delay");
            Assert.That(_rect.localScale.x, Is.LessThan(0.5f), "scale.x should still be < 0.5 during delay");

            // Wait for delay + duration + buffer
            yield return new WaitForSeconds(0.2f + 0.3f + 0.15f);
            yield return null; // ensure RevealCoroutine's final SetFullAlphaAndScale has run

            Assert.AreEqual(1f, _canvasGroup.alpha, 0.001f, "alpha should be 1 after delay+duration");
            Assert.AreEqual(Vector3.one, _rect.localScale, "scale should be 1 after delay+duration");
        }

        // Play with a very short 0.1s duration -- verify it completes faster
        // than a 0.5s duration would.
        [UnityTest]
        public IEnumerator Play_ShortDuration_CompletesBeforeLongDuration()
        {
            // Short duration
                        var goShort = new GameObject("ShortMarker", typeof(RectTransform), typeof(CanvasGroup));
            var rectShort = goShort.GetComponent<RectTransform>();
            var cgShort = goShort.GetComponent<CanvasGroup>();
            var effectShort = goShort.AddComponent<MarkerRevealEffect>();

            // Long duration
                        var goLong = new GameObject("LongMarker", typeof(RectTransform), typeof(CanvasGroup));
            var rectLong = goLong.GetComponent<RectTransform>();
            var cgLong = goLong.GetComponent<CanvasGroup>();
            var effectLong = goLong.AddComponent<MarkerRevealEffect>();

            effectShort.Play(0f, 0.1f);
            effectLong.Play(0f, 0.5f);

            yield return null;

            // After 0.15s: short should be done, long should still be animating
            yield return new WaitForSeconds(0.15f);
            yield return null; // ensure short coroutine's SetFullAlphaAndScale has run

            Assert.AreEqual(1f, cgShort.alpha, 0.001f, "short-reveal marker should be fully visible");
            Assert.AreEqual(1f, rectShort.localScale.x, 0.001f, "short-reveal scale should be 1");

                        Assert.That(cgLong.alpha, Is.LessThan(1f), "long-reveal marker should NOT be fully visible yet");

                        Object.Destroy(goShort);
            Object.Destroy(goLong);
        }

        // Play with a 0s duration -- marker should pop in instantly after
        // the delay (no animation frames needed).
        [UnityTest]
        public IEnumerator Play_ZeroDuration_PopsInAfterDelay()
        {
                        _effect.Play(0.1f, 0f);

            yield return null;

                        Assert.That(_canvasGroup.alpha, Is.LessThan(0.5f), "alpha should be < 0.5 before delay expires");

            yield return new WaitForSeconds(0.15f);
            yield return null; // ensure coroutine's post-delay step (SetFullAlphaAndScale) has run

            Assert.AreEqual(1f, _canvasGroup.alpha, 0.001f, "alpha should be 1 after 0-duration reveal");
            Assert.AreEqual(Vector3.one, _rect.localScale, "scale should be 1 after 0-duration reveal");
        }

        // After the reveal coroutine settles, alpha and scale must stay exactly at their
        // settled values -- no drift. This is the §5 cross-domain handoff contract:
        // LODController (and any future distance-compensation layer) multiplies against
        // these channels after the reveal; if the reveal kept writing to them after
        // completing, it would silently corrupt whatever value that layer had set.
        [UnityTest]
        public IEnumerator Play_AfterCompletion_DoesNotDriftAlphaOrScale()
        {
            _effect.Play(0f, 0.1f);

            // Wait for completion plus a small buffer.
            yield return new WaitForSeconds(0.25f);
            yield return null; // ensure final SetFullAlphaAndScale has executed

            Assert.AreEqual(1f, _canvasGroup.alpha, 0.001f, "alpha must be 1 after settle");
            Assert.AreEqual(Vector3.one, _rect.localScale, "scale must be Vector3.one after settle");

            float alphaAtSettle = _canvasGroup.alpha;
            Vector3 scaleAtSettle = _rect.localScale;

            // Wait several more frames and confirm neither value drifts.
            for (int i = 0; i < 5; i++) yield return null;

            Assert.AreEqual(alphaAtSettle, _canvasGroup.alpha, 0.0001f,
                "alpha must not drift after completion -- LODController writes to this channel next");
            Assert.AreEqual(scaleAtSettle.x, _rect.localScale.x, 0.0001f,
                "scale.x must not drift after completion -- distance-compensation writes to this channel next");
        }
    }
}