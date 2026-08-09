using System.Collections;
using UnityEngine;

namespace TileStories
{
    // Handles the initial reveal animation for a POI marker: waits
    // revealDelaySeconds, then fades in alpha 0->1 and scales up localScale
    // 0->1 over a short fixed duration. Only active at runtime; in Edit Mode
    // (authoring tool populate/refresh), markers must be fully visible
    // immediately since coroutines do not tick in Edit Mode.

        [DisallowMultipleComponent]
    public class MarkerRevealEffect : MonoBehaviour
    {
        [Header("References (auto-resolved if unassigned)")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _rootRect;

        private void Awake()
        {
            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();
            if (_rootRect == null)
                _rootRect = GetComponent<RectTransform>();
        }

        // Play the reveal sequence: wait delaySeconds, then fade in alpha 0->1
        // and scale up localScale 0->1 over durationSeconds. In Edit Mode,
        // immediately set full opacity and scale -- there is no animation
        // outside Play Mode.
        public void Play(float delaySeconds, float durationSeconds)
        {
            if (!Application.isPlaying)
            {
                SetFullAlphaAndScale();
                return;
            }

            EnsureStartHidden();
            StartCoroutine(RevealCoroutine(delaySeconds, durationSeconds));
        }

        private void EnsureStartHidden()
        {
            if (_canvasGroup != null) _canvasGroup.alpha = 0f;
            if (_rootRect != null) _rootRect.localScale = Vector3.zero;
        }

        private void SetFullAlphaAndScale()
        {
            if (_canvasGroup != null) _canvasGroup.alpha = 1f;
            if (_rootRect != null) _rootRect.localScale = Vector3.one;
        }

                private IEnumerator RevealCoroutine(float delaySeconds, float durationSeconds)
        {
            if (delaySeconds > 0f)
                yield return new WaitForSeconds(delaySeconds);

            float elapsed = 0f;
                        while (elapsed < durationSeconds)
            {
                elapsed += Time.deltaTime;
                            float t = Mathf.Clamp01(elapsed / durationSeconds);
                float smooth = t * t * (3f - 2f * t); // smoothstep

                if (_canvasGroup != null) _canvasGroup.alpha = smooth;
                if (_rootRect != null) _rootRect.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, smooth);
                yield return null;
            }

            SetFullAlphaAndScale();
        }
    }
}
